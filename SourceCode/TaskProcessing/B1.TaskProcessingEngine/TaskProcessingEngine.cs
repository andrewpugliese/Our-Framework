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
using B1.LoggingManagement;
using B1.SessionManagement;
using B1.TaskProcessing;

namespace B1.TaskProcessing
{
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

        CacheMgr<TasksInProcess> _taskProcesses =
                new CacheMgr<TasksInProcess>(StringComparer.CurrentCultureIgnoreCase);

        /// <summary>
        /// Constructs a new instance of the TaskProcessingEngine class
        /// </summary>
        /// <param name="daMgr">DataAccessMgr object instance</param>
        /// <param name="engineId"></param>
        /// <param name="configId"></param>
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
            using (LoggingContext lc = new LoggingContext("Task Processing Engine: " + engineId))
            {
                _daMgr = daMgr;
                _maxTaskProcesses = maxTaskProcesses;
                _taskAssemblyPath = taskAssemblyPath;
                _engineId = engineId;
                _configId = configId;
                _signonControl = signonControl;
                CacheDbCommands();
                RecoverTasksInProcess();
                _engineStatus = EngineStatusEnum.Started;
                _daMgr.loggingMgr.Trace("Constructed.", enumTraceLevel.Level2);
            }
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
            _daMgr.loggingMgr.Trace("Recovering any tasks left in process status.", enumTraceLevel.Level2);
            DbCommand selectItemsInProcess = BuildSelectItemsInProcessCmd();
            selectItemsInProcess.Parameters[_daMgr.BuildParamName(TaskProcessing.Constants.ProcessEngineId)].Value
                    = _engineId;
            DataTable itemsInProcess = _daMgr.ExecuteDataSet(selectItemsInProcess, null, null).Tables[0];
            int itemsReset = 0;
            if (itemsInProcess.Rows.Count > 0)
            {
                DbCommand resetItemsInProcess = null;
                foreach (DataRow itemInProcess in itemsInProcess.Rows)
                {
                    if (resetItemsInProcess == null)
                        resetItemsInProcess = BuildResetItemsInProcessCmd();
                    else resetItemsInProcess = _daMgr.CloneDbCommand(resetItemsInProcess);

                    int taskQueueCode = Convert.ToInt32(itemInProcess[TaskProcessing.Constants.TaskQueueCode]);
                    resetItemsInProcess.Parameters[_daMgr.BuildParamName(TaskProcessing.Constants.TaskQueueCode)].Value
                            = taskQueueCode;
                    resetItemsInProcess.Parameters[_daMgr.BuildParamName(TaskProcessing.Constants.StatusMsg)].Value
                            = "StatusCode Reset from InProcess during Engine Startup.";
                    _daMgr.ExecuteNonQuery(resetItemsInProcess, null, null);
                    ++itemsReset;
                }
                _daMgr.loggingMgr.Trace(string.Format("{0} tasks recovered.", itemsReset), enumTraceLevel.Level2);
            }
            return itemsReset;
        }

        void Run()
        {
            using (LoggingContext lc = new LoggingContext("Task Processing Engine: " + _engineId))
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
                            _daMgr.loggingMgr.Trace(string.Format("Users Restricted.  {0} queued Tasks (page size: {1})."
                                        , queuedTasks.Rows.Count
                                        , pageSize)
                                    , enumTraceLevel.Level4);
                        }
                        else
                        {
                            queuedTasks = pageSize == 0 ? _queuedTasks.GetFirstPage()
                                    : _queuedTasks.GetNextPage();
                            pageSize = _queuedTasks.PageSize;
                            _daMgr.loggingMgr.Trace(string.Format("{0} queued Tasks (page size: {1})."
                                        , queuedTasks.Rows.Count
                                        , pageSize)
                                    , enumTraceLevel.Level4);
                        }
                        foreach (DataRow queuedTask in queuedTasks.Rows)
                        {
                            if (_tasksInProcess < _maxTaskProcesses)
                            {
                                Int32? taskQueueCode = TaskReadyToProcess(queuedTask, new CacheMgr<Int32?>(), null);
                                if (taskQueueCode.HasValue)
                                    try
                                    {
                                        ProcessTask(DequeueTask(taskQueueCode.Value));
                                    }
                                    catch (Exception e)
                                    {
                                        _daMgr.loggingMgr.WriteToLog(e);
                                    }
                            }
                            else
                            {
                                _daMgr.loggingMgr.Trace(string.Format("MaxTasksInProcessReached: {0}"
                                            , _maxTaskProcesses), enumTraceLevel.Level5);
                                Thread.Sleep(500);
                            }
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
        }

        TaskProcess LoadTaskProcess(DequeuedTask dequeuedTask)
        {
            TaskProcess taskProcess = Core.ObjectFactory.Create<TaskProcess>(
                string.Format("{0}\\{1}", string.IsNullOrEmpty(_taskAssemblyPath) ? "."
                    : _taskAssemblyPath
                        , dequeuedTask.AssemblyName)
                , dequeuedTask.TaskId
                , _daMgr
                , dequeuedTask.TaskId
                , dequeuedTask.TaskQueueCode
                , dequeuedTask.Parameters
                , new TaskProcess.TaskCompletedDelegate(TaskCompleted)
                , _engineId);
            return taskProcess;
        }

        void ProcessTask(DequeuedTask dequeuedTask)
        {
            TaskProcess taskProcess = LoadTaskProcess(dequeuedTask);
            lock (_taskCounterLock)
            {
                Thread taskProcessThread = new Thread(taskProcess.Start);
                taskProcessThread.IsBackground = true;
                TasksInProcess tasksInProcess = new TasksInProcess(taskProcess, taskProcessThread
                        , dequeuedTask);
                _taskProcesses.Add(dequeuedTask.TaskQueueCode.ToString(), tasksInProcess);
                ++_tasksInProcess;
                taskProcessThread.Start();
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

        /// <summary>
        /// Initiates the dequeing of tasks from the queue
        /// </summary>
        public void Start()
        {
            _mainThread = new Thread(Run);
            _mainThread.IsBackground = true;
            _mainThread.Start();
        }

        public void Stop()
        {
            using (LoggingContext lc = new LoggingContext("Task Processing Engine: " + _engineId))
            {
                _daMgr.loggingMgr.Trace("Stopping.", enumTraceLevel.Level2);
                _engineStatus = EngineStatusEnum.Stopped;
                _stopEvent.Set();   // signal to stop
                lock (_taskCounterLock)
                {
                    foreach (string taskProcessKey in _taskProcesses.Keys)
                    {
                        TaskProcess taskProcess = _taskProcesses.Get(taskProcessKey).Process;
                        taskProcess.Stop();
                    }
                }
            }
        }

        public void Pause()
        {
            using (LoggingContext lc = new LoggingContext("Task Processing Engine: " + _engineId))
            {
                _daMgr.loggingMgr.Trace("Pausing", enumTraceLevel.Level2);
                _engineStatus = EngineStatusEnum.Paused;
            }
        }

        public void Resume()
        {
            using (LoggingContext lc = new LoggingContext("Task Processing Engine: " + _engineId))
            {
                _daMgr.loggingMgr.Trace("Resuming", enumTraceLevel.Level2);
                _engineStatus = EngineStatusEnum.Running;
                _resumeEvent.Set();   // signal to resume
            }
        }

        public string Status()
        {
            return string.Format("Engine Status: {0}; taskHandlers available: {1}; taskHandlers processing: {2}{3}"
                , _engineStatus.ToString(), _maxTaskProcesses - _tasksInProcess, _tasksInProcess, Environment.NewLine);
        }

        void TaskCompleted(int taskQueueCode, TaskProcess.ProcessStatusEnum processStatus)
        {
            using (LoggingContext lc = new LoggingContext("Task Processing Engine: " + _engineId))
            {
                lock (_taskCounterLock)
                {
                    _daMgr.loggingMgr.Trace(string.Format("TaskCompleted Event for TaskQueueCode: {0}"
                            , taskQueueCode), enumTraceLevel.Level4);
                    if (!_taskProcesses.Exists(taskQueueCode.ToString()))
                        throw new ExceptionEvent(enumExceptionEventCodes.TaskQueueCodeNotFoundAtCompletion
                                , string.Format("TaskQueueCode: {0}", taskQueueCode));

                    DequeuedTask dequeuedTask = _taskProcesses.Get(taskQueueCode.ToString()).DequeuedTaskData;
                    DbCommand cmdComplete = BuildCompleteTaskCmd();
                    cmdComplete.Parameters[_daMgr.BuildParamName(TaskProcessing.Constants.TaskQueueCode)].Value
                            = dequeuedTask.TaskQueueCode;
                    if (processStatus == TaskProcess.ProcessStatusEnum.Completed)
                    {
                        cmdComplete.Parameters[_daMgr.BuildParamName(TaskProcessing.Constants.StatusCode)].Value
                                = Convert.ToByte(TaskProcessing.TaskProcessingQueue.StatusCodeEnum.Succeeded);
                        cmdComplete.Parameters[_daMgr.BuildParamName(TaskProcessing.Constants.StatusMsg)].Value
                                = DBNull.Value;
                        cmdComplete.Parameters[_daMgr.BuildParamName(TaskProcessing.Constants.LastCompletedMsg)].Value
                                = DBNull.Value;
                        cmdComplete.Parameters[_daMgr.BuildParamName(TaskProcessing.Constants.LastCompletedCode)].Value
                                = Convert.ToByte(TaskProcessing.TaskProcessingQueue.StatusCodeEnum.Succeeded);
                    }
                    else
                    {
                        cmdComplete.Parameters[_daMgr.BuildParamName(TaskProcessing.Constants.LastCompletedCode)].Value
                                = Convert.ToByte(TaskProcessing.TaskProcessingQueue.StatusCodeEnum.Failed);
                        cmdComplete.Parameters[_daMgr.BuildParamName(TaskProcessing.Constants.StatusCode)].Value
                                = Convert.ToByte(TaskProcessing.TaskProcessingQueue.StatusCodeEnum.Failed);
                        cmdComplete.Parameters[_daMgr.BuildParamName(TaskProcessing.Constants.StatusMsg)].Value
                                = _taskProcesses.Get(taskQueueCode.ToString()).Process.TaskStatus();
                        cmdComplete.Parameters[_daMgr.BuildParamName(TaskProcessing.Constants.LastCompletedMsg)].Value
                                = _taskProcesses.Get(taskQueueCode.ToString()).Process.TaskStatus();
                    }

                    if (_taskProcesses.Get(taskQueueCode.ToString()).DequeuedTaskData.IntervalSecondsRequeue > 0)
                        cmdComplete.Parameters[_daMgr.BuildParamName(TaskProcessing.Constants.IntervalCount)].Value
                                = _taskProcesses.Get(taskQueueCode.ToString()).DequeuedTaskData.IntervalCount + 1;
                    else cmdComplete.Parameters[_daMgr.BuildParamName(TaskProcessing.Constants.IntervalCount)].Value
                            = _taskProcesses.Get(taskQueueCode.ToString()).DequeuedTaskData.IntervalCount;

                    if (_taskProcesses.Get(taskQueueCode.ToString()).DequeuedTaskData.ClearParametersAtEnd)
                        cmdComplete.Parameters[_daMgr.BuildParamName(TaskProcessing.Constants.TaskParameters)].Value
                                 = DBNull.Value;
                    else cmdComplete.Parameters[_daMgr.BuildParamName(TaskProcessing.Constants.TaskParameters)].Value
                                 = _taskProcesses.Get(taskQueueCode.ToString()).DequeuedTaskData.TaskParameters;

                    _daMgr.ExecuteNonQuery(cmdComplete, null, null);

                    _taskProcesses.Remove(taskQueueCode.ToString());

                    if (_tasksInProcess > 0)
                        --_tasksInProcess;
                    else throw new ExceptionEvent(enumExceptionEventCodes.TasksInProcessCounterUnderFlow
                            , string.Format("TaskQueueCode: {0}", taskQueueCode));
                    _daMgr.loggingMgr.Trace(string.Format("TaskCompleted Cleanup Success for TaskQueueCode: {0}"
                            , taskQueueCode), enumTraceLevel.Level4);
                }
            }
        }

        public void Dispose()
        {
            using (LoggingContext lc = new LoggingContext("Task Processing Engine: " + _engineId))
            {
                _daMgr.loggingMgr.Trace("Disposing", enumTraceLevel.Level2);
                while (_tasksInProcess > 0 || _taskProcesses.Keys.Count > 0)
                {
                    lock (_taskCounterLock)
                    {
                        foreach (string taskProcessKey in _taskProcesses.Keys)
                        {
                            Thread taskThread = _taskProcesses.Get(taskProcessKey).ProcessThread;
                            if (taskThread.IsAlive)
                                if (!taskThread.Join(5000))
                                    _daMgr.loggingMgr.Trace(string.Format("Waiting for processThread: {0}"
                                            , taskThread.ManagedThreadId)
                                            , enumTraceLevel.Level2);
                        }
                    }
                    if (_tasksInProcess > 0)
                        _daMgr.loggingMgr.Trace(string.Format("Tasks In Process: {0}", _tasksInProcess)
                            , enumTraceLevel.Level2);
                }
                if (_mainThread.IsAlive)
                    while (!_mainThread.Join(5000))
                        _daMgr.loggingMgr.Trace(string.Format("Waiting for mainThread: {0}"
                                , _mainThread.ManagedThreadId)
                                , enumTraceLevel.Level2);
                _daMgr.loggingMgr.Trace("Disposed", enumTraceLevel.Level2);
            }
        }

        DequeuedTask DequeueTask(Int64 taskQueueCode)
        {
            _daMgr.loggingMgr.Trace(string.Format("Dequeuing TaskCode: {0}", taskQueueCode), enumTraceLevel.Level5);
            _deQueueTask.Parameters[_daMgr.BuildParamName(TaskProcessing.Constants.TaskQueueCode)].Value = taskQueueCode;
            _deQueueTask.Parameters[_daMgr.BuildParamName(TaskProcessing.Constants.ProcessEngineId)].Value = _engineId;
            DataTable dequeuedTaskData = _daMgr.ExecuteDataSet(_deQueueTask, null, null).Tables[0];
            if (dequeuedTaskData != null && dequeuedTaskData.Rows.Count > 0)
                return new DequeuedTask(dequeuedTaskData.Rows[0]);
            else return null;
        }

        Int32? TaskReadyToProcess(DataRow queuedTask,  CacheMgr<Int32?> tasksVisited, Int32? parentTask)
        {
            Int32 taskQueueCode = Convert.ToInt32(queuedTask[TaskProcessing.Constants.TaskQueueCode]);
            using (LoggingContext lc = new LoggingContext(string.Format("TaskQueueCode: {0}", taskQueueCode)))
            {
                if (!NoUsers
                    && Convert.ToBoolean(queuedTask[TaskProcessing.Constants.WaitForNoUsers]))
                {
                    _daMgr.loggingMgr.Trace(string.Format("WaitForNoUsers Required", enumTraceLevel.Level5));
                    return null;
                }
                if (queuedTask[TaskProcessing.Constants.WaitForEngineId] != DBNull.Value
                    && _engineId != queuedTask[TaskProcessing.Constants.WaitForEngineId].ToString())
                {
                    _daMgr.loggingMgr.Trace(string.Format("WaitForEngineId: {0} Required"
                            , queuedTask[TaskProcessing.Constants.WaitForEngineId].ToString()), enumTraceLevel.Level5);
                    return null;
                }
                if (queuedTask[TaskProcessing.Constants.WaitForConfigId] != DBNull.Value
                    && _configId != queuedTask[TaskProcessing.Constants.WaitForConfigId].ToString())
                {
                    _daMgr.loggingMgr.Trace(string.Format("WaitForConfigId: {0} Required"
                            , queuedTask[TaskProcessing.Constants.WaitForConfigId].ToString()), enumTraceLevel.Level5);
                    return null;
                }
                bool waitForTasks = Convert.ToBoolean(queuedTask[TaskProcessing.Constants.WaitForTasks]);

                if (waitForTasks)
                {
                    Int32 dependentTaskQueueCode = taskQueueCode;
                    if (queuedTask.Table.Columns.Contains(TaskProcessing.Constants.WaitTaskQueueCode))
                        dependentTaskQueueCode = Convert.ToInt32(queuedTask[TaskProcessing.Constants.WaitTaskQueueCode]);

                    if (!tasksVisited.Exists(dependentTaskQueueCode.ToString()))
                    {
                        using (LoggingContext lc2 = new LoggingContext(string.Format("DependsOnTask: {0}", dependentTaskQueueCode)))
                        {
                            tasksVisited.Add(dependentTaskQueueCode.ToString(), parentTask);
                            if (!TaskDependenciesCompleted(dependentTaskQueueCode, tasksVisited))
                                return null;
                        }
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
            BuildResetItemsInProcessCmd();
            BuildSelectItemsInProcessCmd();
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
            dmlUpdate.AddColumn(TaskProcessing.Constants.StatusMsg);
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


        DbCommand BuildSelectItemsInProcessCmd()
        {
            DbTableDmlMgr dmlSelect = new DbTableDmlMgr(_daMgr
                     , DataAccess.Constants.SCHEMA_CORE
                     , TaskProcessing.Constants.TaskProcessingQueue
                     , TaskProcessing.Constants.TaskQueueCode);

            dmlSelect.SetWhereCondition(w => w.Column(TaskProcessing.Constants.StatusCode) == w.Value(
                        Convert.ToByte(TaskProcessingQueue.StatusCodeEnum.InProcess))
                    && w.Column(TaskProcessing.Constants.ProcessEngineId) == w.Parameter(
                        TaskProcessing.Constants.ProcessEngineId));

            return _daMgr.BuildSelectDbCommand(dmlSelect, null);
        }

        DbCommand BuildResetItemsInProcessCmd()
        {
            DbTableDmlMgr dmlUpdate = new DbTableDmlMgr(_daMgr
                    , DataAccess.Constants.SCHEMA_CORE
                    , TaskProcessing.Constants.TaskProcessingQueue);
            dmlUpdate.AddColumn(TaskProcessing.Constants.StatusDateTime, Core.EnumDateTimeLocale.UTC);
            dmlUpdate.AddColumn(TaskProcessing.Constants.StatusCode
                    , new DbConstValue(Convert.ToByte(TaskProcessing.TaskProcessingQueue.StatusCodeEnum.Queued)));
            dmlUpdate.AddColumn(TaskProcessing.Constants.StatusMsg);
            dmlUpdate.SetWhereCondition(w => w.Column(TaskProcessing.Constants.TaskQueueCode)
                    == w.Parameter(TaskProcessing.Constants.TaskQueueCode));
            return _daMgr.BuildUpdateDbCommand(dmlUpdate);
        }

        DbCommand BuildCompleteTaskCmd()
        {
            DbTableDmlMgr dmlUpdate = new DbTableDmlMgr(_daMgr
                    , DataAccess.Constants.SCHEMA_CORE
                    , TaskProcessing.Constants.TaskProcessingQueue);
            dmlUpdate.AddColumn(TaskProcessing.Constants.StatusDateTime, Core.EnumDateTimeLocale.UTC);
            dmlUpdate.AddColumn(TaskProcessing.Constants.CompletedDateTime, Core.EnumDateTimeLocale.UTC);
            dmlUpdate.AddColumn(TaskProcessing.Constants.TaskParameters);
            dmlUpdate.AddColumn(TaskProcessing.Constants.StatusCode);
            dmlUpdate.AddColumn(TaskProcessing.Constants.LastCompletedCode);
            dmlUpdate.AddColumn(TaskProcessing.Constants.LastCompletedMsg);
            dmlUpdate.AddColumn(TaskProcessing.Constants.StatusMsg);
            dmlUpdate.AddColumn(TaskProcessing.Constants.IntervalCount);
            dmlUpdate.SetWhereCondition(w => w.Column(TaskProcessing.Constants.TaskQueueCode)
                    == w.Parameter(TaskProcessing.Constants.TaskQueueCode));
            return _daMgr.BuildUpdateDbCommand(dmlUpdate);
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
