using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Reflection;
using System.Data;
using System.Data.Common;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.ServiceModel.Channels;

using B1.DataAccess;
using B1.CacheManagement;
using B1.ILoggingManagement;
using B1.LoggingManagement;
using B1.SessionManagement;
using B1.TaskProcessing;
using B1.Wpf.Controls;

namespace B1.TaskProcessing
{
    /// <summary>
    /// This class will retrieve and manage the processing of tasks from the TaskProcessingQueue database table.
    /// <para>Once the engine is started, it will continue to dequeue tasks until it is stopped.</para>
    /// <para>When a task is dequeued it will be dispatched to an available task handler.</para>
    /// <para>When the task queue is empty, the engine will idle until tasks are added or the engine is stopped.</para>
    /// <para>The engine can be paused (it will idle without dequeing) until resumed or stopped.</para>
    /// </summary>
    public class TaskProcessingEngine : TaskProcessing.IHostTPE//, TaskProcessing.IRemoteEngineHost
    {
        public enum EngineStatusEnum { Off = 0, Started = 1, Running = 2, Paused = 3, Stopped = 4 };
        static string _engineId = null;
        static string _configId = null;
        static Dictionary<string, string> _clientConnections = new Dictionary<string, string>(StringComparer.CurrentCultureIgnoreCase);
        static Dictionary<string, string> _configSettings = new Dictionary<string, string>(StringComparer.CurrentCultureIgnoreCase);
        static int _parallelTaskLimit = Environment.ProcessorCount * 6;
        static int _maxTaskProcesses = 1;
        static byte _tasksInProcess = 0;
        static object _maxTaskLock = new object();
        static DataAccessMgr _daMgr = null;
        static EngineStatusEnum _engineStatus = EngineStatusEnum.Off;
        object _taskCounterLock = new object();
        ManualResetEvent _stopEvent = new ManualResetEvent(false);
        ManualResetEvent _resumeEvent = new ManualResetEvent(false);
        Thread _mainThread = null;
        string _taskAssemblyPath = null;
        SignonControl _signonControl = null;
        PagingMgr _queuedTasks = null;
        PagingMgr _queuedTasksNoUsersOnly = null;
        DbCommand _deQueueTask = null;
        DbCommand _taskDependencies = null;
        string _wcfHostBaseAddress = null;
        ServiceHost _wcfServiceHost = null;

        CacheMgr<TasksInProcess> _taskProcesses =
                new CacheMgr<TasksInProcess>(StringComparer.CurrentCultureIgnoreCase);

        /// <summary>
        /// Constructs a new instance of the TaskProcessingEngine class
        /// </summary>
        /// <param name="daMgr">DataAccessMgr object instance</param>
        /// <param name="taskAssemblyPath">String path to task implementation assemblies can be found</param>
        /// <param name="engineId">Unique identifier of this TPE instance</param>
        /// <param name="configId">Optional configuration identifier to be used to compare to task configurations</param>
        /// <param name="signonControl">SignonControl object providing runtime configuration</param>
        /// <param name="maxTaskProcesses">Configures the engine for a maximum number of concurrent task handlers</param>
        /// <param name="wcfHostBaseAddress">Optional string address that host will use for WCF clients</param>
        public TaskProcessingEngine(DataAccessMgr daMgr
                , string taskAssemblyPath
                , string engineId
                , string configId
                , SignonControl signonControl
                , int maxTaskProcesses = 1
                , string wcfHostBaseAddress = null)
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
                _wcfHostBaseAddress = wcfHostBaseAddress;
                _configSettings.Add(DataAccess.Constants.ConnectionKey, _daMgr.ConnectionKey);
                _configSettings.Add(ILoggingManagement.Constants.LoggingKey, _daMgr.loggingMgr.LoggingKey);
                _configSettings.Add(Constants.MaxTasksInParallel, maxTaskProcesses.ToString());
                _configSettings.Add(Constants.TaskAssemblyPath, taskAssemblyPath);
                _configSettings.Add(Constants.EngineId, engineId);
                _configSettings.Add(Constants.ConfigId, configId);
                _configSettings.Add(TaskProcessing.Constants.HostEndpointAddress, wcfHostBaseAddress);

                _daMgr.loggingMgr.Trace(string.Format("Constructed {0}"
                        , !string.IsNullOrEmpty(_wcfHostBaseAddress) ? " with WCF host address" : "")
                , enumTraceLevel.Level2);
            }
        }

        /// <summary>
        /// Constructor used for WCF server
        /// </summary>
        internal TaskProcessingEngine()
        {
            if (string.IsNullOrEmpty(_engineId))
                throw new ExceptionEvent(enumExceptionEventCodes.MissingRequiredConfigurationValue
                    , "Cannot create Remote TaskProcessingEngine because local instance not created.");
        }

        /// <summary>
        /// Starts the main engine thread and initiates the dequeing of tasks from the queue
        /// </summary>
        public void Start()
        {
            _mainThread = new Thread(Run);
            _mainThread.IsBackground = true;
            _mainThread.Start();
            _engineStatus = EngineStatusEnum.Started;
        }

        /// <summary>
        /// Signals a stop event and stops all tasks processes and the WCF service host.
        /// </summary>
        public void Stop()
        {
            using (LoggingContext lc = new LoggingContext("Task Processing Engine: " + _engineId))
            {
                _daMgr.loggingMgr.Trace("Stopping.", enumTraceLevel.Level2);
                _engineStatus = EngineStatusEnum.Stopped;
                _stopEvent.Set();   // signal to stop
                StopServiceHost();
                lock (_taskCounterLock)
                {
                    foreach (string taskProcessKey in _taskProcesses.Keys)
                    {
                        TaskProcess taskProcess = _taskProcesses.Get(taskProcessKey).Process;
                        taskProcess.Stop();
                    }
                }
                if (_configSettings != null
                    && _configSettings.Count > 0)
                    _configSettings.Clear();
                if (_clientConnections != null
                    && _clientConnections.Count > 0)
                    _clientConnections.Clear();
            }
        }

        /// <summary>
        /// Temporarily suspends all processing of tasks until a resume event is encountered
        /// </summary>
        public void Pause()
        {
            Pause(null);
        }

        /// <summary>
        /// Temporarily suspends all processing of tasks until a resume event is encountered
        /// </summary>
        /// <param name="remoteClientId">Unique identifier of remote client application</param>
        public void Pause(string remoteClientId)
        {
            using (LoggingContext lc = new LoggingContext("Task Processing Engine: " + _engineId))
            {
                _daMgr.loggingMgr.Trace(string.Format("Pausing{0}", !string.IsNullOrEmpty(remoteClientId)
                        ? ". Initiated by RemoteClientId: " + remoteClientId : "")
                        , enumTraceLevel.Level2);
                _engineStatus = EngineStatusEnum.Paused;
            }
        }

        /// <summary>
        /// Signals a resume event and causes all suspended threads to continue porcessing
        /// </summary>
        public void Resume()
        {
            Resume(null);
        }

        /// <summary>
        /// Signals a resume event and causes all suspended threads to continue porcessing
        /// </summary>
        /// <param name="remoteClientId">Unique identifier of remote client application</param>
        public void Resume(string remoteClientId)
        {
            using (LoggingContext lc = new LoggingContext("Task Processing Engine: " + _engineId))
            {
                _daMgr.loggingMgr.Trace(string.Format("Resuming{0}", !string.IsNullOrEmpty(remoteClientId)
                        ? ". Initiated by RemoteClientId: " + remoteClientId : "")
                        , enumTraceLevel.Level2);
                _engineStatus = EngineStatusEnum.Running;
                _resumeEvent.Set();   // signal to resume
            }
        }

        /// <summary>
        /// Returns a string description of the TPE instance status
        /// </summary>
        /// <returns></returns>
        public string Status()
        {
            return Status(null);
        }

        /// <summary>
        /// Returns a string description of the TPE instance status
        /// </summary>
        /// <param name="remoteClientId">Unique identifier of remote client application</param>
        /// <returns></returns>
        public string Status(string remoteClientId)
        {
            string status = string.Format("Engine Status: {0}; taskHandlers available: {1}; taskHandlers processing: {2}; remoteClients: {3}{4}"
                , _engineStatus.ToString(), _maxTaskProcesses - _tasksInProcess, _tasksInProcess, _clientConnections.Count(), Environment.NewLine);
            if (string.IsNullOrEmpty(remoteClientId))
                return status;
            else
            {
                Dictionary<string, string> dynamicSettings = new Dictionary<string, string>(StringComparer.CurrentCultureIgnoreCase);
                dynamicSettings.Add(TaskProcessing.Constants.MaxTasksInParallel, _maxTaskProcesses.ToString());
                dynamicSettings.Add(ILoggingManagement.Constants.TraceLevel, _daMgr.loggingMgr.TraceLevel.ToString());
                dynamicSettings.Add(TaskProcessing.Constants.EngineStatus, _engineStatus.ToString());
                dynamicSettings.Add(TaskProcessing.Constants.StatusMsg, status);
                return Core.Functions.Serialize(dynamicSettings);
            }

        }

        internal string EngineId
        {
            get { return _engineId; }
        }

        internal void Trace(string msg, enumTraceLevel traceLevel)
        {
            _daMgr.loggingMgr.Trace(msg, traceLevel);
        }

        /// <summary>
        /// Returns current state of the engine
        /// </summary>
        public EngineStatusEnum EngineStatus
        {
            get { return _engineStatus; }
        }

        /// <summary>
        /// Validates a remote connection
        /// </summary>
        /// <param name="remoteClientId">Unique identifier of remote client application</param>
        public void Connect(string remoteClientId)
        {
            if (!_clientConnections.ContainsKey(remoteClientId))
                _clientConnections.Add(remoteClientId, DateTime.UtcNow.ToString());
        }

        /// <summary>
        /// Acknowledges a disconnection of a remote client 
        /// </summary>
        /// <param name="remoteClientId">Unique identifier of remote client application</param>
        public void Disconnect(string remoteClientId)
        {
            if (_clientConnections.ContainsKey(remoteClientId))
                _clientConnections.Remove(remoteClientId);
        }

        /// <summary>
        /// Returns the dictionary of confiuration settings
        /// </summary>
        /// <returns></returns>
        public Dictionary<string, string> ConfigSettings()
        {
            return _configSettings;
        }

        /// <summary>
        /// Returns the dictionary of remote connections
        /// </summary>
        /// <returns></returns>
        public Dictionary<string, string> RemoteClients()
        {
            return _clientConnections;
        }

        /// <summary>
        /// Returns the dictionary of runtime settings which may include configuration settings
        /// that can be changed during runtime.
        /// </summary>
        /// <returns></returns>
        public Dictionary<string, string> DynamicSettings()
        {
            Dictionary<string, string> dynamicSettings = new Dictionary<string, string>(StringComparer.CurrentCultureIgnoreCase);
            dynamicSettings.Add(TaskProcessing.Constants.MaxTasksInParallel, _maxTaskProcesses.ToString());
            dynamicSettings.Add(ILoggingManagement.Constants.TraceLevel, _daMgr.loggingMgr.TraceLevel.ToString());
            dynamicSettings.Add(TaskProcessing.Constants.EngineStatus, _engineStatus.ToString());
            dynamicSettings.Add(TaskProcessing.Constants.StatusMsg, Status());
            return dynamicSettings;
        }

        /// <summary>
        /// Changes the setting which controls the maximum number of tasks that can be run
        /// simulataneously on this TPE instance
        /// </summary>
        /// <param name="delta">The change in the number of tasks (positive  or negative)</param>
        /// <returns>The new maximum value</returns>
        public int SetMaxTasks(int delta)
        {
            return SetMaxTasks(null, delta);
        }

        /// <summary>
        /// Changes the setting which controls the maximum number of tasks that can be run
        /// simulataneously on this TPE instance.
        /// </summary>
        /// <param name="remoteClientId">Unique identifier of remote client application</param>
        /// <param name="delta">The change in the number of tasks (positive  or negative)</param>
        /// <returns>The new maximum value</returns>
        public int SetMaxTasks(string remoteClientId, int delta)
        {
            using (LoggingContext lc = new LoggingContext("Task Processing Engine: " + _engineId))
            {
                _daMgr.loggingMgr.Trace(string.Format("MaxTasksSet{0}", !string.IsNullOrEmpty(remoteClientId)
                        ? ". Initiated by RemoteClientId: " + remoteClientId : "")
                        , enumTraceLevel.Level2);
                lock (_maxTaskLock)
                {
                    if (delta != 0)
                    {
                        if (delta > 0)
                            if ((delta + _maxTaskProcesses) <= _parallelTaskLimit)
                                _maxTaskProcesses = _maxTaskProcesses + delta;
                            else
                            {
                                _daMgr.loggingMgr.WriteToLog(string.Format("MaxTasks LIMIT EXCEEDED ({0}); Change will be ignored", _parallelTaskLimit)
                                        , System.Diagnostics.EventLogEntryType.Warning, enumEventPriority.Normal);
                            }
                        else if ((delta + _maxTaskProcesses) > 0)
                            _maxTaskProcesses = _maxTaskProcesses + delta;
                        else _daMgr.loggingMgr.WriteToLog(string.Format("MaxTasks MINIMUM EXCEEDED ({0}); Change will be ignored", 1)
                                , System.Diagnostics.EventLogEntryType.Warning, enumEventPriority.Normal);
                    }
                    return _maxTaskProcesses;
                }
            }
        }

        /// <summary>
        /// Returns the current setting for maximum tasks in parallel
        /// </summary>
        public int MaxTasks
        {
            get { return _maxTaskProcesses; }
        }

        /// <summary>
        /// Changes the trace level setting to the given level
        /// </summary>
        /// <param name="traceLevel"></param>
        public void SetTraceLevel(string traceLevel)
        {
            SetTraceLevel(null, traceLevel);
        }

        /// <summary>
        /// Changes the trace level setting to the given level
        /// </summary>
        /// <param name="remoteClientId">Unique identifier of remote client application</param>
        /// <param name="traceLevel"></param>
        public void SetTraceLevel(string remoteClientId, string traceLevel)
        {
            _daMgr.loggingMgr.TraceLevel = ILoggingManagement.Constants.ToTraceLevel(traceLevel);
        }

        /// <summary>
        /// Indicates whether or not there are no users allowed on system.
        /// </summary>
        bool NoUsers
        {
            get
            {
                return _signonControl.SignonControlData.ForceSignoff
                  && _signonControl.SignonControlData.RestrictSignon;
            }
        }

        /// <summary>
        /// Called at startup to check to see if there are any tasks found in the running state for this 
        /// instance's engineId.  If so, then those status must be changed because they cannot be running
        /// since this instance has not started yet and it is the only engine instance allowed with that 
        /// same engineId.
        /// </summary>
        /// <returns>The number of tasks recovered</returns>
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

        #region "Wcf Host Methods"
        private ServiceHost StartServiceHost()
        {
            // Step 1 of the address configuration procedure: Create a URI to serve as the base address.
            Uri baseAddress = new Uri(_wcfHostBaseAddress); //string.Format("{0}//{1}", _wcfHostBaseAddress, _engineId));
            // Create a binding that uses a username/password credential.

            // Step 2 of the hosting procedure: Create ServiceHost
            _wcfServiceHost = new ServiceHost(typeof(TaskProcessing.RemoteHostProxy), baseAddress);
            try
            {
                WSHttpBinding binding = new WSHttpBinding(SecurityMode.None);
                //binding.Security.Message.ClientCredentialType = MessageCredentialType.UserName;


                // Step 3 of the hosting procedure: Add a service endpoint. (with/without securityMode defined by binding)
                _wcfServiceHost.AddServiceEndpoint(
                    typeof(TaskProcessing.IRemoteHostTPE),
                    binding,
                    _engineId);

                // Step 4 of the hosting procedure: Enable metadata exchange.
               // ServiceMetadataBehavior smb = new ServiceMetadataBehavior();
               // smb.HttpGetEnabled = true;
                //_wcfServiceHost.Description.Behaviors.Add(smb);

                // Step 5 of the hosting procedure: Start (and then stop) the service.
                _wcfServiceHost.Open();

            }
            catch (CommunicationException ce)
            {
                using (LoggingContext lc = new LoggingContext("Task Processing Engine: " + _engineId))
                {
                    _daMgr.loggingMgr.WriteToLog(ce);
                }
                _wcfServiceHost.Abort();
                _wcfServiceHost = null;
            }
            return _wcfServiceHost;
        }

        private void StopServiceHost()
        {
            if (_wcfServiceHost != null)
            {
                _wcfServiceHost.Close();
                _wcfServiceHost = null;
                _clientConnections.Clear();
            }
        }

        #endregion

        /// <summary>
        /// Main engine loop.  Runs continuously until stopped.
        /// </summary>
        void Run()
        {
            using (LoggingContext lc = new LoggingContext("Task Processing Engine: " + _engineId))
            {
                // start WCF if configured for it and not started
                if (!string.IsNullOrEmpty(_wcfHostBaseAddress)
                    && _wcfServiceHost == null)
                {
                    _wcfServiceHost = StartServiceHost();
                }

                _engineStatus = EngineStatusEnum.Running;
                DataTable queuedTasks = new DataTable();
                Int16 pageSize = 0;
                while (_engineStatus != EngineStatusEnum.Stopped)
                {
                    while (_engineStatus == EngineStatusEnum.Running)
                    {
                        // all queue queries are paged
                        // when no users are allowed on system, we can query those tasks that require no users
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
                        else // otherwise we query all tasks
                        {
                            queuedTasks = pageSize == 0 ? _queuedTasks.GetFirstPage()
                                    : _queuedTasks.GetNextPage();
                            pageSize = _queuedTasks.PageSize;
                            _daMgr.loggingMgr.Trace(string.Format("{0} queued Tasks (page size: {1})."
                                        , queuedTasks.Rows.Count
                                        , pageSize)
                                    , enumTraceLevel.Level4);
                        }
                        // loop through the paged data to see if an item can be dequeued
                        // (a logical operation where status can only be changed by 1 thread)
                        // (thus 'locking the record' for that TPE)
                        // NOTE: These are the records that are recovered if this instance crashes
                        // without changing the status of these records
                        foreach (DataRow queuedTask in queuedTasks.Rows)
                        {
                            // as long as there are available 'threads' to process records
                            if (_tasksInProcess < _maxTaskProcesses)
                            {
                                // check the task to see if it meets all conditions (e.g. dependencies, datetime, etc)
                                Int32? taskQueueCode = TaskReadyToProcess(queuedTask, new CacheMgr<Int32?>(), null);
                                // if it has a value, then it can be processed
                                if (taskQueueCode.HasValue)
                                    try
                                    {
                                        // Process the Task ONLY if it can be successfully dequeued as described above
                                        ProcessTask(DequeueTask(taskQueueCode.Value));
                                    }
                                    catch (Exception e)
                                    {
                                        _daMgr.loggingMgr.WriteToLog(e);
                                    }
                            }
                            else // if we dont have enough threads; sleep
                            {
                                _daMgr.loggingMgr.Trace(string.Format("MaxTasksInProcessReached: {0}"
                                            , _maxTaskProcesses), enumTraceLevel.Level5);
                                Thread.Sleep(500);
                            }
                        }
                        // if the number of records paged was less then a full page size, then it means we dont have 
                        // a full queue; so we can sleep
                        if (queuedTasks.Rows.Count < pageSize)
                            pageSize = 0;
                        Thread.Sleep(1000);
                    }
                    // if the engine was paused, then wait for a resume or stop event)
                    if (_engineStatus == EngineStatusEnum.Paused)
                    {
                        WaitHandle[] waithandles = new WaitHandle[] { _stopEvent, _resumeEvent };
                        int waitResult = WaitHandle.WaitAny(waithandles);
                        if (waitResult == 0)
                            _stopEvent.Reset();
                        if (waitResult == 1)
                            _resumeEvent.Reset();
                    }
                    else Thread.Sleep(500);
                }
            }
            Off(); // if we are here then the engine must be turned off
        }

        /// <summary>
        /// Creates a TaskProcess for the task by loading the Assembly
        /// which contains the task implementation.  Each task process will be passed
        /// a delegate which will be called when proceess is stopped
        /// </summary>
        /// <param name="dequeuedTask">The data structure containing the information dequeued from Task Processing Queue</param>
        /// <returns>TaskProces object</returns>
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

        /// <summary>
        /// Process the task process object on a new thread
        /// </summary>
        /// <param name="dequeuedTask">The data structure containing the information dequeued from Task Processing Queue</param>
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

        /// <summary>
        /// The delegate handler for the task completion event.
        /// <para>The function updates the database status and cleans up the memory collections of tasks in process</para>
        /// </summary>
        /// <param name="taskQueueCode">The unique identifier of the task processing queueu recod</param>
        /// <param name="processStatus">Indicates whether process succeeded or failed</param>
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

        /// <summary>
        /// Engine has stopped running
        /// Stop all tasks and wait for threads to join
        /// </summary>
        public void Off()
        {
            if (_engineStatus != EngineStatusEnum.Stopped)
                Stop();
            using (LoggingContext lc = new LoggingContext("Task Processing Engine: " + _engineId))
            {
                _daMgr.loggingMgr.Trace("Turning Off", enumTraceLevel.Level2);
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
                _daMgr.loggingMgr.Trace("Engine is Off", enumTraceLevel.Level2);
            }
        }

        /// <summary>
        /// Makes database call on given task queue code to 'lock' the record
        /// (change the status)
        /// </summary>
        /// <param name="taskQueueCode">The unique identifier of the task processing queue record</param>
        /// <returns>A DequeuedTask object when successful; NULL otherwise</returns>
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

        /// <summary>
        /// Checks all conditions necessary to attempt to dequeue a task; for example: Dependencies, NoUsers, Configurations, etc
        /// </summary>
        /// <param name="queuedTask">DataRow of task information</param>
        /// <param name="tasksVisited">Used to determine if there are cyclic dependencies</param>
        /// <param name="parentTask">Used to determine if there are cyclic dependencies</param>
        /// <returns>TaskQueueCode when ready, NULL otherwise</returns>
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

        /// <summary>
        /// Determines if the dependencies of the given task queue code have been met
        /// </summary>
        /// <param name="taskQueueCode">unique identifier of the task processing queue to test</param>
        /// <param name="taskVisited">Used to determine cyclic dependencies</param>
        /// <returns></returns>
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

        /// <summary>
        /// Builds and caches the DbCommands used
        /// </summary>
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
