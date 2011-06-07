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
        public string Payload;
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

        CacheMgr<Thread> _taskProcessThreads = 
                new CacheMgr<Thread>(StringComparer.CurrentCultureIgnoreCase);
        CacheMgr<TaskProcess> _taskProcesses =
                new CacheMgr<TaskProcess>(StringComparer.CurrentCultureIgnoreCase);
        CacheMgr<Assembly> _taskAssemblies =
                new CacheMgr<Assembly>(StringComparer.CurrentCultureIgnoreCase);
        CacheMgr<DbCommand> _engineDbCommands =
                new CacheMgr<DbCommand>(StringComparer.CurrentCultureIgnoreCase);


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
            while (_engineStatus != EngineStatusEnum.Stopped)
            {
                while (_engineStatus == EngineStatusEnum.Running)
                {
                    while (_tasksInProcess < _maxTaskProcesses)
                    {
                        TaskQueueStructure? taskMetaData = DequeueTask();
                        if (taskMetaData.HasValue)
                            try
                            {
                                ProcessTask(taskMetaData.Value);
                            }
                            catch (Exception e)
                            {
                                _daMgr.loggingMgr.WriteToLog(e);
                            }
                        Thread.Sleep(100);
                    }
                    Thread.Sleep(1000);
                }

                if (_engineStatus == EngineStatusEnum.Paused)
                {
                    WaitHandle[] waithandles = new WaitHandle[3];
                    waithandles[0] = _stopEvent;
                    waithandles[1] = _resumeEvent;
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
                string.Format("{0}\\{1}", string.IsNullOrEmpty(taskMetaData.AssemblyPath) ? "." : taskMetaData.AssemblyPath
                        , taskMetaData.AssemblyName)
                , taskMetaData.TaskId
                , _daMgr
                , taskMetaData.TaskId
                , taskMetaData.Payload
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

        TaskQueueStructure? DequeueTask()
        {
            DbCommand dbCmd = _daMgr.CloneDbCommand(_engineDbCommands.Get(Constants.QueuedTaskList + NoUsers.ToString()));
            dbCmd.Parameters[_daMgr.BuildParamName(DataAccess.Constants.PageSize)].Value = 2 * _maxTaskProcesses;
            DataTable queuedTasks = _daMgr.ExecuteDataSet(dbCmd, null, null).Tables[0];
            foreach (DataRow queuedTask in queuedTasks.Rows)
            {
                if (Convert.ToBoolean(queuedTask[Constants.WaitForNoUsers])
                    && !NoUsers)
                    continue;
                if (_engineId != queuedTask[Constants.WaitForEngineId].ToString())
                    continue;
                if (_configId != queuedTask[Constants.WaitForConfigId].ToString())
                    continue;
                Int64 taskQueueCode = Convert.ToInt64(queuedTask[Constants.TaskQueueCode]);
                if (Convert.ToBoolean(queuedTask[Constants.WaitForTasks]))
                    if (!DependenciesCompleted(taskQueueCode))
                        continue;
                TaskQueueStructure? newTask = DequeueTask(taskQueueCode);
                if (newTask.HasValue)
                    return newTask.Value;
                else continue;
            }
            return null;
        }

        TaskQueueStructure? DequeueTask(Int64 taskQueueCode)
        {
            DbCommandMgr cmdMgr = new DbCommandMgr(_daMgr);
            DbTableDmlMgr dmlUpdate = new DbTableDmlMgr(_daMgr
                    , DataAccess.Constants.SCHEMA_CORE
                    , Constants.TaskProcessingQueue
                    , Constants.StatusCode);
            dmlUpdate.AddColumn(Constants.StatusCode, _daMgr.BuildParamName(Constants.StatusCode));

            DbCommand dbCmd = _engineDbCommands.Get(TaskProcessingEngine.Constants.QueuedTaskList);
            dbCmd.Parameters[_daMgr.BuildParamName(DataAccess.Constants.PageSize)].Value = taskQueueCode;
            DataTable dequeuedTask = _daMgr.ExecuteDataSet(dbCmd, null, null).Tables[0];

            if (dequeuedTask != null && dequeuedTask.Rows.Count > 0)
            {
                TaskQueueStructure newTask = new TaskQueueStructure();
                newTask.TaskId = dequeuedTask.Rows[0][Constants.TaskId].ToString();
                newTask.AssemblyName = dequeuedTask.Rows[0][Constants.AssemblyName].ToString();
                newTask.AssemblyPath = _taskAssemblyPath;
                return newTask;
            }
            return null;
        }

        bool DependenciesCompleted(Int64 taskQueueCode)
        {
            return false;
        }

        void CacheDbCommands()
        {
            _queuedTasks = BuildCmdGetQueuedTasksList(false);
            _queuedTasksNoUsersOnly = BuildCmdGetQueuedTasksList(true);
        }

        PagingMgr BuildCmdGetQueuedTasksList(bool noUsersOnly)
        {
            DbTableDmlMgr dmlSelectMgr = _daMgr.DbCatalogGetTableDmlMgr(DataAccess.Constants.SCHEMA_CORE
                     , Constants.TaskProcessingQueue
                     , Constants.WaitForDateTime
                     , Constants.WaitForConfigId
                     , Constants.WaitForEngineId
                     , Constants.WaitForTasks
                     , Constants.TaskQueueCode);
            dmlSelectMgr.SetWhereCondition(w => w.Column(Constants.StatusCode) 
                    == new DbConstValue(Convert.ToByte(QueueStatusEnum.Queued))
                    && w.Column(Constants.WaitForDateTime) <= Core.EnumDateTimeLocale.UTC);
            //if (noUsersOnly)
            //{
            //    dmlSelectMgr.AddToWhereCondition(System.Linq.Expressions.ExpressionType.AndAlso
            //        , 
            //        }

            dmlSelectMgr.AddOrderByColumnAscending(Constants.StatusCode);
            dmlSelectMgr.AddOrderByColumnAscending(Constants.PriorityCode);
            dmlSelectMgr.AddOrderByColumnAscending(Constants.WaitForDateTime);

            return new PagingMgr(_daMgr, null, null, null, null, null);
        }
    }
}
