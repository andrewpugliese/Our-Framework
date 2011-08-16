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
using B1.TaskProcessing;

namespace B1.TaskProcessing
{

    internal struct TaskQueueStructure
    {
        public string TaskId;
        public int TaskQueueCode;
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
    public class TaskProcessingEngine : IDisposable
    {
        public enum EngineStatusEnum { Off = 0, Started = 1, Running = 2, Paused = 3, Stopped = 4 };
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
        public TaskProcessingEngine(DataAccessMgr daMgr
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

        int RecoverTasksInProcess()
        {
            return 0;
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
                        if (_tasksInProcess < _maxTaskProcesses)
                        {
                            Int32? taskQueueCode = TaskReadyToProcess(queuedTask, new CacheMgr<Int32?>(), null);
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
                        else Thread.Sleep(500);
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
                else Thread.Sleep(1000);
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
                , taskMetaData.TaskQueueCode
                , taskMetaData.Parameters
                , new TaskProcess.TaskCompletedDelegate(TaskCompleted));
            return taskProcess;
        }

        void ProcessTask(TaskQueueStructure taskQueueData)
        {
            TaskProcess taskProcess = LoadTaskProcess(taskQueueData);
            lock (_taskCounterLock)
            {
                _taskProcesses.Add(taskQueueData.TaskQueueCode.ToString(), taskProcess);
                Thread taskProcessThread = new Thread(taskProcess.Start);
                taskProcessThread.IsBackground = true;
                _taskProcessThreads.Add(taskQueueData.TaskQueueCode.ToString(), taskProcessThread);
                taskProcessThread.Start();
                ++_tasksInProcess;
            }
        }

        public byte MaxTaskProcesses(sbyte delta)
        {
            if (delta != 0)
            if (delta > 0 && (delta + _maxTaskProcesses) <= byte.MaxValue)
                _maxTaskProcesses = Convert.ToByte(_maxTaskProcesses + delta);
            if (delta < 0 && (delta + _maxTaskProcesses) > 0)
                _maxTaskProcesses = Convert.ToByte(_maxTaskProcesses + delta);
            return _maxTaskProcesses;
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

        void TaskCompleted(int taskQueueCode, TaskProcess.ProcessStatusEnum processStatus)
        {
            lock (_taskCounterLock)
            {
                if (!_taskProcesses.Exists(taskQueueCode.ToString()))
                    throw new ApplicationException("taskQueueCode not found");

                _taskProcesses.Remove(taskQueueCode.ToString());

                if (!_taskProcessThreads.Exists(taskQueueCode.ToString()))
                    throw new ApplicationException("taskQueueCode not found");

                _taskProcessThreads.Remove(taskQueueCode.ToString());
                
                if (_tasksInProcess > 0)
                    --_tasksInProcess;
                else throw new ApplicationException("tasksInProcess overflow");
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

            _deQueueTask.Parameters[_daMgr.BuildParamName(TaskProcessing.Constants.TaskQueueCode)].Value = taskQueueCode;
            _deQueueTask.Parameters[_daMgr.BuildParamName(TaskProcessing.Constants.ProcessEngineId)].Value = _engineId;
            DataTable dequeuedTask = _daMgr.ExecuteDataSet(_deQueueTask, null, null).Tables[0];

            if (dequeuedTask != null && dequeuedTask.Rows.Count > 0)
            {
                TaskQueueStructure newTask = new TaskQueueStructure();
                newTask.TaskId = dequeuedTask.Rows[0][TaskProcessing.Constants.TaskId].ToString();
                newTask.TaskQueueCode = Convert.ToInt32(dequeuedTask.Rows[0][TaskProcessing.Constants.TaskQueueCode]);
                newTask.Parameters = dequeuedTask.Rows[0][TaskProcessing.Constants.TaskParameters].ToString();
                newTask.AssemblyName = dequeuedTask.Rows[0][TaskProcessing.Constants.AssemblyName].ToString();
                newTask.AssemblyPath = _taskAssemblyPath;
                return newTask;
            }
            return null;
        }

        Int32? TaskReadyToProcess(DataRow queuedTask,  CacheMgr<Int32?> tasksVisited, Int32? parentTask)
        {
            if (!NoUsers
                && Convert.ToBoolean(queuedTask[TaskProcessing.Constants.WaitForNoUsers]))
                return null;
            if (queuedTask[TaskProcessing.Constants.WaitForEngineId] != DBNull.Value
                && _engineId != queuedTask[TaskProcessing.Constants.WaitForEngineId].ToString())
                return null;
            if (queuedTask[TaskProcessing.Constants.WaitForConfigId] != DBNull.Value
                && _configId != queuedTask[TaskProcessing.Constants.WaitForConfigId].ToString())
                return null;
            Int32 taskQueueCode = Convert.ToInt32(queuedTask[TaskProcessing.Constants.TaskQueueCode]);
            bool waitForTasks = Convert.ToBoolean(queuedTask[TaskProcessing.Constants.WaitForTasks]);

            if (waitForTasks)
            {
                Int32 dependentTaskQueueCode = taskQueueCode;
                if (queuedTask.Table.Columns.Contains(TaskProcessing.Constants.WaitTaskQueueCode))
                    dependentTaskQueueCode = Convert.ToInt32(queuedTask[TaskProcessing.Constants.WaitTaskQueueCode]);

                if (!tasksVisited.Exists(dependentTaskQueueCode.ToString()))
                {
                    tasksVisited.Add(dependentTaskQueueCode.ToString(), parentTask);
                    if (!TaskDependenciesCompleted(dependentTaskQueueCode, tasksVisited))
                        return null;
                }
                else
                {
                    string msg = string.Format("Circular Dependency found for task: {0}; Task Chain: {1}"
                        , taskQueueCode, tasksVisited.Keys.ToString());
                    throw new ArgumentException(msg);
                }
                
            }

            return taskQueueCode;
        }

        bool TaskDependenciesCompleted(Int32 taskQueueCode,  CacheMgr<Int32?> taskVisited)
        {
            _taskDependencies.Parameters[_daMgr.BuildParamName(TaskProcessing.Constants.TaskQueueCode)].Value 
                    = taskQueueCode;
            DataTable dependencyTasks = _daMgr.ExecuteDataSet(_taskDependencies, null, null).Tables[0];
            foreach (DataRow dependentTask in dependencyTasks.Rows)
            {
                Int32? dependentTaskQueueCode = TaskReadyToProcess(dependentTask, taskVisited, taskQueueCode);
                if (!dependentTaskQueueCode.HasValue)
                    return false;
            }
            return true;
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
                    , TaskProcessing.Constants.TaskProcessingQueue
                    , TaskProcessing.Constants.StatusCode);
            dmlUpdate.AddColumn(TaskProcessing.Constants.StatusDateTime, Core.EnumDateTimeLocale.UTC);
            dmlUpdate.AddColumn(TaskProcessing.Constants.StartedDateTime, Core.EnumDateTimeLocale.UTC);
            dmlUpdate.AddColumn(TaskProcessing.Constants.ProcessEngineId);
            dmlUpdate.AddColumn(TaskProcessing.Constants.CompletedDateTime);
            dmlUpdate.SetWhereCondition(w => w.Column(TaskProcessing.Constants.TaskQueueCode)
                    == w.Parameter(TaskProcessing.Constants.TaskQueueCode));
            DbCommand cmdChange = _daMgr.BuildChangeDbCommand(dmlUpdate, TaskProcessing.Constants.StatusCode);
            cmdChange.Parameters[_daMgr.BuildParamName(TaskProcessing.Constants.StatusCode, true)].Value
                    = Convert.ToByte(TaskProcessingQueue.StatusCodeEnum.InProcess);
            cmdChange.Parameters[_daMgr.BuildParamName(TaskProcessing.Constants.StatusCode)].Value
                    = Convert.ToByte(TaskProcessingQueue.StatusCodeEnum.Queued);
            DbTableDmlMgr dmlSelect = new DbTableDmlMgr(_daMgr
                     , DataAccess.Constants.SCHEMA_CORE
                     , TaskProcessing.Constants.TaskProcessingQueue
                     , TaskProcessing.Constants.TaskId
                     , TaskProcessing.Constants.TaskQueueCode
                     , TaskProcessing.Constants.TaskParameters
                     , TaskProcessing.Constants.ClearParametersAtEnd
                     , TaskProcessing.Constants.IntervalCount
                     , TaskProcessing.Constants.IntervalSecondsRequeue);

            dmlSelect.AddJoin(DataAccess.Constants.SCHEMA_CORE
                    , TaskProcessing.Constants.TaskRegistrations
                    , DbTableJoinType.Inner
                    , j => j.AliasedColumn(dmlSelect.MainTable.TableAlias, TaskProcessing.Constants.TaskId)
                        == j.Column(TaskProcessing.Constants.TaskRegistrations
                            , TaskProcessing.Constants.TaskId)
                    , TaskProcessing.Constants.AssemblyName);

            dmlSelect.SetWhereCondition(w => w.Column(TaskProcessing.Constants.TaskQueueCode) 
                    == w.Parameter(TaskProcessing.Constants.TaskQueueCode)
                    && w.Column(TaskProcessing.Constants.StatusCode) == w.Value(
                        Convert.ToByte(TaskProcessingQueue.StatusCodeEnum.InProcess))
                    && w.Column(TaskProcessing.Constants.ProcessEngineId) == w.Parameter(
                        TaskProcessing.Constants.ProcessEngineId));

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
                     , TaskProcessing.Constants.TaskProcessingQueue
                     , TaskProcessing.Constants.WaitForDateTime
                     , TaskProcessing.Constants.WaitForConfigId
                     , TaskProcessing.Constants.WaitForEngineId
                     , TaskProcessing.Constants.WaitForTasks
                     , TaskProcessing.Constants.WaitForNoUsers
                     , TaskProcessing.Constants.TaskQueueCode);

            dmlSelect.AddJoin(DataAccess.Constants.SCHEMA_CORE
                    , TaskProcessing.Constants.TaskDependencies
                    , DbTableJoinType.Inner
                    , j => j.Column(TaskProcessing.Constants.TaskProcessingQueue
                            , TaskProcessing.Constants.TaskQueueCode)
                        == j.Column(TaskProcessing.Constants.TaskDependencies
                            , TaskProcessing.Constants.TaskQueueCode)
                    , TaskProcessing.Constants.WaitTaskQueueCode
                    , TaskProcessing.Constants.WaitTaskCompletionCode);

            dmlSelect.SetWhereCondition(w => w.Column(TaskProcessing.Constants.TaskQueueCode) 
                    == w.Parameter(TaskProcessing.Constants.TaskQueueCode));

            return _daMgr.BuildSelectDbCommand(dmlSelect, null);
        }


        PagingMgr BuildCmdGetQueuedTasksList(bool noUsersOnly)
        {
            DbTableDmlMgr dmlSelectMgr = _daMgr.DbCatalogGetTableDmlMgr(DataAccess.Constants.SCHEMA_CORE
                     , TaskProcessing.Constants.TaskProcessingQueue
                     , TaskProcessing.Constants.StatusCode
                     , TaskProcessing.Constants.PriorityCode
                     , TaskProcessing.Constants.WaitForDateTime
                     , TaskProcessing.Constants.WaitForConfigId
                     , TaskProcessing.Constants.WaitForEngineId
                     , TaskProcessing.Constants.WaitForTasks
                     , TaskProcessing.Constants.WaitForNoUsers
                     , TaskProcessing.Constants.TaskQueueCode);
            dmlSelectMgr.SetWhereCondition(w => w.Column(TaskProcessing.Constants.StatusCode)
                    == w.Value(Convert.ToByte(TaskProcessingQueue.StatusCodeEnum.Queued))
                    && w.Column(TaskProcessing.Constants.WaitForDateTime) <= w.Function(
                        _daMgr.GetDbTimeAs(Core.EnumDateTimeLocale.UTC, null)));
            
            if (noUsersOnly)
            {
                System.Linq.Expressions.Expression expNoUsers =
                    DbPredicate.CreatePredicatePart(w => w.Column(TaskProcessing.Constants.WaitForNoUsers)
                            == 1);
                dmlSelectMgr.AddToWhereCondition(System.Linq.Expressions.ExpressionType.AndAlso, expNoUsers);
            }

            dmlSelectMgr.AddOrderByColumnAscending(TaskProcessing.Constants.StatusCode);
            dmlSelectMgr.AddOrderByColumnAscending(TaskProcessing.Constants.PriorityCode);
            dmlSelectMgr.AddOrderByColumnAscending(TaskProcessing.Constants.WaitForDateTime);

            return new PagingMgr(_daMgr
                    , dmlSelectMgr
                    , DataAccess.Constants.PageSize
                    , Convert.ToInt16(_maxTaskProcesses * 3)
                    , null);
        }
    }
}
