using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Reflection;
using System.Data;
using System.Data.Common;

using B1.DataAccess;
using B1.CacheManagement;
using B1.ILoggingManagement;
using B1.SessionManagement;
using B1.TaskProcessingFunctions;

namespace B1.TaskProcessingEngine
{

    internal struct TaskQueueStructure
    {
        public string TaskId;
        public string Parameters;
        public string AssemblyName;
        public string AssemblyPath;
    }

    /// <summary>
    /// This class will retrieve and manage the processing of tasks from the TaskProcessingQueue database table.
    /// <para>Once the engine is started, it will continue to dequeue tasks until it is stopped.</para>
    /// <para>When a task is dequeued it will be dispatched to an available task handler.</para>
    /// <para>When the task queue is empty, the engine will idle until tasks are added or the engine is stopped.</para>
    /// <para>The engine can be paused (it will idle without dequeing) until resumed or stopped.</para>
    /// </summary>
    public class TaskProcessEngine : IDisposable
    {
        public enum EngineStatusEnum { Off = 0, Started = 1, Running = 2, Paused = 3, Stopped = 4 };
        enum QueueStatusEnum { NotQueued = 0, Queued = 32, InProcess = 64, Failed = 128, Succeeded = 255 };
        DataAccessMgr _daMgr = null;
        EngineStatusEnum _engineStatus = EngineStatusEnum.Off;
        byte _maxTaskProcesses = 1;
        byte _tasksInProcess = 0;
        object _taskCounterLock = new object();
        ManualResetEvent _stopEvent = new ManualResetEvent(false);
        ManualResetEvent _resumeEvent = new ManualResetEvent(false);
        Thread _mainThread = null;
        string _taskAssemblyPath = null;
        SignonControl _signonControl = null;
        string _engineId = null;
        string _configId = null;
        PagingMgr _queuedTasks = null;
        PagingMgr _queuedTasksNoUsersOnly = null;
        DbCommand _deQueueTask = null;
        DbCommand _taskDependencies = null;

        CacheMgr<Thread> _taskProcessThreads = 
                new CacheMgr<Thread>(StringComparer.CurrentCultureIgnoreCase);
        CacheMgr<TaskProcess> _taskProcesses =
                new CacheMgr<TaskProcess>(StringComparer.CurrentCultureIgnoreCase);
        CacheMgr<Assembly> _taskAssemblies =
                new CacheMgr<Assembly>(StringComparer.CurrentCultureIgnoreCase);
 

        /// <summary>
        /// Constructs a new instance of the TaskProcessingEngine class
        /// </summary>
        /// <param name="daMgr">DataAccessMgr object instance</param>
        /// <param name="maxTaskProcesses">Configures the engine for a maximum number of concurrent task handlers</param>
        public TaskProcessEngine(DataAccessMgr daMgr
                , string taskAssemblyPath
                , string engineId
                , string configId
                , SignonControl signonControl
                , byte maxTaskProcesses = 1)
        {
            if (daMgr.loggingMgr == null)
                throw new ExceptionEvent(enumExceptionEventCodes.NullOrEmptyParameter
                    , "TaskProcessingEngine requires a DataAccessMgr with a LoggingMgr.");
            _daMgr = daMgr;
            _maxTaskProcesses = maxTaskProcesses;
            _taskAssemblyPath = taskAssemblyPath;
            _engineId = engineId;
            _configId = configId;
            _signonControl = signonControl;
            CacheDbCommands();
            _engineStatus = EngineStatusEnum.Started;
        }

        bool NoUsers
        {
            get
            {
                return _signonControl.SignonControlData.ForceSignoff
                  && _signonControl.SignonControlData.RestrictSignon;
            }
        }

        /// <summary>
        /// Initiates the dequeing of tasks from the queue
        /// </summary>
        public void Start()
        {
            _mainThread = new Thread(Run);
            _mainThread.IsBackground = true;
            _mainThread.Start();
        }

        void Run()
        {
            _engineStatus = EngineStatusEnum.Running;
            DataTable queuedTasks = new DataTable();
            Int16 pageSize = 0;
            while (_engineStatus != EngineStatusEnum.Stopped)
            {
                while (_engineStatus == EngineStatusEnum.Running)
                {
                    if (NoUsers)
                    {
                        queuedTasks = pageSize == 0 ? _queuedTasksNoUsersOnly.GetFirstPage() 
                                : _queuedTasksNoUsersOnly.GetNextPage();
                        pageSize = _queuedTasksNoUsersOnly.PageSize;
                    }
                    else
                    {
                        queuedTasks = pageSize == 0 ? _queuedTasks.GetFirstPage()
                                : _queuedTasks.GetNextPage();
                        pageSize = _queuedTasks.PageSize;
                    }
                    foreach (DataRow queuedTask in queuedTasks.Rows)
                    {
                        while (_tasksInProcess < _maxTaskProcesses)
                        {
                            Int64? taskQueueCode = TaskReadyToProcess(queuedTask, new CacheMgr<Int64?>(), null);
                            TaskQueueStructure? newTask = taskQueueCode.HasValue ? DequeueTask(taskQueueCode.Value) 
                                    : null;
                            if (newTask.HasValue)
                                try
                                {
                                    ProcessTask(newTask.Value);
                                }
                                catch (Exception e)
                                {
                                    _daMgr.loggingMgr.WriteToLog(e);
                                }
                        }
                        Thread.Sleep(500);
                    }
                    if (queuedTasks.Rows.Count < pageSize)
                        pageSize = 0;
                    Thread.Sleep(1000);
                }
                if (_engineStatus == EngineStatusEnum.Paused)
                {
                    WaitHandle[] waithandles = new WaitHandle[] { _stopEvent, _resumeEvent };
                    int waitResult = WaitHandle.WaitAny(waithandles);
                    if (waitResult == 0)
                        _stopEvent.Reset();
                    if (waitResult == 1)
                        _resumeEvent.Reset();
                }
            }

        }

        TaskProcess LoadTaskProcess(TaskQueueStructure taskMetaData)
        {
            TaskProcess taskProcess = Core.ObjectFactory.Create<TaskProcess>(
                string.Format("{0}\\{1}", string.IsNullOrEmpty(taskMetaData.AssemblyPath) ? "." 
                    : taskMetaData.AssemblyPath
                        , taskMetaData.AssemblyName)
                , taskMetaData.TaskId
                , _daMgr
                , taskMetaData.TaskId
                , taskMetaData.Parameters
                , new TaskProcess.TaskCompletedDelegate(TaskCompleted));
            return taskProcess;
        }

        void ProcessTask(TaskQueueStructure taskQueueData)
        {
            TaskProcess taskProcess = LoadTaskProcess(taskQueueData);
            lock (_taskCounterLock)
            {
                _taskProcesses.Add(taskQueueData.ToString(), taskProcess);
                Thread taskProcessThread = new Thread(taskProcess.Start);
                taskProcessThread.IsBackground = true;
                _taskProcessThreads.Add(taskQueueData.TaskId, taskProcessThread);
                taskProcessThread.Start();
                ++_tasksInProcess;
            }
        }

        public void Stop()
        {
            _engineStatus = EngineStatusEnum.Stopped;
            _stopEvent.Set();   // signal to stop
            lock (_taskCounterLock)
            {
                foreach (string taskProcessKey in _taskProcesses.Keys)
                {
                    TaskProcess taskProcess = _taskProcesses.Get(taskProcessKey);
                    taskProcess.Stop();
                }
            }
        }

        public void Pause()
        {
            _engineStatus = EngineStatusEnum.Paused;
        }

        public void Resume()
        {
            _engineStatus = EngineStatusEnum.Running;
            _resumeEvent.Set();   // signal to resume
        }

        public string Status()
        {
            return string.Format("Engine Status: {0}; taskHandlers available: {1}; taskHandlers processing: {2}{3}"
                , _engineStatus.ToString(), _maxTaskProcesses - _tasksInProcess, _tasksInProcess, Environment.NewLine);
        }

        public void TaskCompleted(string taskId, TaskProcess.ProcessStatusEnum processStatus)
        {
            lock (_taskCounterLock)
            {
                _taskProcesses.Remove(taskId);
                _taskProcessThreads.Remove(taskId);
                --_tasksInProcess;
            }
        }

        public void Dispose()
        {
            lock (_taskCounterLock)
            {
                foreach (string taskProcessKey in _taskProcessThreads.Keys)
                {
                    Thread taskThread = _taskProcessThreads.Get(taskProcessKey);
                    if (taskThread.IsAlive)
                        if (!taskThread.Join(1000))
                            taskThread.Abort();
                }
                _taskProcesses.Clear();
                _taskProcessThreads.Clear();
            }
            if (_mainThread.IsAlive)
                if (!_mainThread.Join(1000))
                    _mainThread.Abort();
        }

        TaskQueueStructure? DequeueTask(Int64 taskQueueCode)
        {

            _deQueueTask.Parameters[_daMgr.BuildParamName(DataAccess.Constants.PageSize)].Value = taskQueueCode;
            DataTable dequeuedTask = _daMgr.ExecuteDataSet(_deQueueTask, null, null).Tables[0];

            if (dequeuedTask != null && dequeuedTask.Rows.Count > 0)
            {
                TaskQueueStructure newTask = new TaskQueueStructure();
                newTask.TaskId = dequeuedTask.Rows[0][TaskProcessingFunctions.Constants.TaskId].ToString();
                newTask.Parameters = dequeuedTask.Rows[0][TaskProcessingFunctions.Constants.TaskParameters].ToString();
                newTask.AssemblyName = dequeuedTask.Rows[0][TaskProcessingFunctions.Constants.AssemblyName].ToString();
                newTask.AssemblyPath = _taskAssemblyPath;
                return newTask;
            }
            return null;
        }

        Int64? TaskReadyToProcess(DataRow queuedTask,  CacheMgr<Int64?> taskDependencies, Int64? parentTask)
        {
            if (Convert.ToBoolean(queuedTask[TaskProcessingFunctions.Constants.WaitForNoUsers])
            && !NoUsers)
                return null;
            if (_engineId != queuedTask[TaskProcessingFunctions.Constants.WaitForEngineId].ToString())
                return null;
            if (_configId != queuedTask[TaskProcessingFunctions.Constants.WaitForConfigId].ToString())
                return null;
            Int64 taskQueueCode = Convert.ToInt64(queuedTask[TaskProcessingFunctions.Constants.TaskQueueCode]);
            
            if (!taskDependencies.Exists(taskQueueCode.ToString()))
                taskDependencies.Add(taskQueueCode.ToString(), parentTask);
            else throw new ArgumentException("Circular Dependency found for task");

            if (Convert.ToBoolean(queuedTask[TaskProcessingFunctions.Constants.WaitForTasks]))
                return TaskDependenciesCompleted(taskQueueCode, taskDependencies);
            return taskQueueCode;
        }

        Int64? TaskDependenciesCompleted(Int64 taskQueueCode,  CacheMgr<Int64?> taskDependencies)
        {
            _taskDependencies.Parameters[_daMgr.BuildParamName(TaskProcessingFunctions.Constants.TaskQueueCode)].Value 
                    = taskQueueCode;
            DataTable dependencyTasks = _daMgr.ExecuteDataSet(_taskDependencies, null, null).Tables[0];
            foreach (DataRow dependentTask in dependencyTasks.Rows)
            {
                Int64? dependentTaskQueueCode = TaskReadyToProcess(dependentTask, taskDependencies, taskQueueCode);
                if (!dependentTaskQueueCode.HasValue)
                    return null;
            }
            return taskQueueCode;
        }

        void CacheDbCommands()
        {
            _queuedTasks = BuildCmdGetQueuedTasksList(false);
            _queuedTasksNoUsersOnly = BuildCmdGetQueuedTasksList(true);
            _deQueueTask = BuildDeQueueCmd();
            _taskDependencies = BuildGetDependenciesCmd();

        }

        DbCommand BuildDeQueueCmd()
        {
            DbCommandMgr cmdMgr = new DbCommandMgr(_daMgr);
            DbTableDmlMgr dmlUpdate = new DbTableDmlMgr(_daMgr
                    , DataAccess.Constants.SCHEMA_CORE
                    , TaskProcessingFunctions.Constants.TaskProcessingQueue
                    , TaskProcessingFunctions.Constants.StatusCode);
            dmlUpdate.AddColumn(TaskProcessingFunctions.Constants.StatusDateTime, Core.EnumDateTimeLocale.UTC);
            dmlUpdate.AddColumn(TaskProcessingFunctions.Constants.StartedDateTime, Core.EnumDateTimeLocale.UTC);
            dmlUpdate.AddColumn(TaskProcessingFunctions.Constants.CompletedDateTime);
            dmlUpdate.SetWhereCondition(w => w.Column(TaskProcessingFunctions.Constants.TaskQueueCode)
                    == w.Parameter(TaskProcessingFunctions.Constants.TaskQueueCode)
                    && w.Column(TaskProcessingFunctions.Constants.ProcessEngineId) == w.Parameter(
                        TaskProcessingFunctions.Constants.ProcessEngineId));
            DbCommand cmdChange = _daMgr.BuildChangeDbCommand(dmlUpdate, TaskProcessingFunctions.Constants.StatusCode);
            cmdChange.Parameters[_daMgr.BuildParamName(TaskProcessingFunctions.Constants.StatusCode, true)].Value
                    = Convert.ToByte(QueueStatusEnum.InProcess);
            cmdChange.Parameters[_daMgr.BuildParamName(TaskProcessingFunctions.Constants.StatusCode)].Value
                    = Convert.ToByte(QueueStatusEnum.Queued);
            DbTableDmlMgr dmlSelect = new DbTableDmlMgr(_daMgr
                     , DataAccess.Constants.SCHEMA_CORE
                     , TaskProcessingFunctions.Constants.TaskProcessingQueue
                     , TaskProcessingFunctions.Constants.TaskId
                     , TaskProcessingFunctions.Constants.TaskParameters
                     , TaskProcessingFunctions.Constants.ClearParametersAtEnd
                     , TaskProcessingFunctions.Constants.IntervalCount
                     , TaskProcessingFunctions.Constants.IntervalSecondsRequeue);

            dmlSelect.AddJoin(DataAccess.Constants.SCHEMA_CORE
                    , TaskProcessingFunctions.Constants.TaskRegistrations
                    , DbTableJoinType.Inner
                    , j => j.AliasedColumn(dmlSelect.MainTable.TableAlias, TaskProcessingFunctions.Constants.TaskId)
                        == j.Column(TaskProcessingFunctions.Constants.TaskRegistrations
                            , TaskProcessingFunctions.Constants.TaskId)
                    , TaskProcessingFunctions.Constants.AssemblyName);

            dmlSelect.SetWhereCondition(w => w.Column(TaskProcessingFunctions.Constants.TaskQueueCode) 
                    == w.Parameter(TaskProcessingFunctions.Constants.TaskQueueCode)
                    && w.Column(TaskProcessingFunctions.Constants.StatusCode) == w.Value(
                        Convert.ToByte(QueueStatusEnum.InProcess))
                    && w.Column(TaskProcessingFunctions.Constants.ProcessEngineId) == w.Parameter(
                        TaskProcessingFunctions.Constants.ProcessEngineId));

            DbCommand cmdSelect = _daMgr.BuildSelectDbCommand(dmlSelect, null);
            cmdMgr.AddDbCommand(cmdChange);
            cmdMgr.AddDbCommand(cmdSelect);
            return cmdMgr.DbCommandBlock;
        }

        DbCommand BuildGetDependenciesCmd()
        {
            DbCommandMgr cmdMgr = new DbCommandMgr(_daMgr);
            DbTableDmlMgr dmlSelect = new DbTableDmlMgr(_daMgr
                     , DataAccess.Constants.SCHEMA_CORE
                     , TaskProcessingFunctions.Constants.TaskDependencies
                     , TaskProcessingFunctions.Constants.WaitForDateTime
                     , TaskProcessingFunctions.Constants.WaitForConfigId
                     , TaskProcessingFunctions.Constants.WaitForEngineId
                     , TaskProcessingFunctions.Constants.WaitForTasks
                     , TaskProcessingFunctions.Constants.WaitForNoUsers
                     , TaskProcessingFunctions.Constants.TaskQueueCode);

            dmlSelect.AddJoin(DataAccess.Constants.SCHEMA_CORE
                    , TaskProcessingFunctions.Constants.TaskProcessingQueue
                    , DbTableJoinType.Inner
                    , j => j.Column(TaskProcessingFunctions.Constants.TaskProcessingQueue
                            , TaskProcessingFunctions.Constants.TaskQueueCode)
                        == j.Column(TaskProcessingFunctions.Constants.TaskDependencies
                            , TaskProcessingFunctions.Constants.WaitTaskQueueCode)
                    , TaskProcessingFunctions.Constants.WaitTaskCompletionCode);

            dmlSelect.SetWhereCondition(w => w.Column(TaskProcessingFunctions.Constants.TaskQueueCode) 
                    == w.Parameter(TaskProcessingFunctions.Constants.TaskQueueCode));

            return _daMgr.BuildSelectDbCommand(dmlSelect, null);
        }


        PagingMgr BuildCmdGetQueuedTasksList(bool noUsersOnly)
        {
            DbTableDmlMgr dmlSelectMgr = _daMgr.DbCatalogGetTableDmlMgr(DataAccess.Constants.SCHEMA_CORE
                     , TaskProcessingFunctions.Constants.TaskProcessingQueue
                     , TaskProcessingFunctions.Constants.WaitForDateTime
                     , TaskProcessingFunctions.Constants.WaitForConfigId
                     , TaskProcessingFunctions.Constants.WaitForEngineId
                     , TaskProcessingFunctions.Constants.WaitForTasks
                     , TaskProcessingFunctions.Constants.TaskQueueCode);
            dmlSelectMgr.SetWhereCondition(w => w.Column(TaskProcessingFunctions.Constants.StatusCode)
                    == w.Value(Convert.ToByte(QueueStatusEnum.Queued))
                    && w.Column(TaskProcessingFunctions.Constants.WaitForDateTime) <= w.Function(
                        _daMgr.GetDbTimeAs(Core.EnumDateTimeLocale.UTC, null)));
            
            if (noUsersOnly)
            {
                System.Linq.Expressions.Expression expNoUsers =
                    DbPredicate.CreatePredicatePart(w => w.Column(TaskProcessingFunctions.Constants.WaitForNoUsers)
                            == 1);
                dmlSelectMgr.AddToWhereCondition(System.Linq.Expressions.ExpressionType.AndAlso, expNoUsers);
            }

            dmlSelectMgr.AddOrderByColumnAscending(TaskProcessingFunctions.Constants.StatusCode);
            dmlSelectMgr.AddOrderByColumnAscending(TaskProcessingFunctions.Constants.PriorityCode);
            dmlSelectMgr.AddOrderByColumnAscending(TaskProcessingFunctions.Constants.WaitForDateTime);

            return new PagingMgr(_daMgr
                    , dmlSelectMgr
                    , DataAccess.Constants.PageSize
                    , Convert.ToInt16(_maxTaskProcesses * 3)
                    , null);
        }
    }
}
