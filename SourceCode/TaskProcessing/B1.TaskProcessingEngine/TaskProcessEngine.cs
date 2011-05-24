using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Reflection;

using B1.DataAccess;
using B1.CacheManagement;
using B1.ILoggingManagement;

namespace B1.TaskProcessing
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
        public enum EngineStatusEnum { Off = 0, Started = 1, Running = 2, Paused = 3, Stopped = 4};
        DataAccessMgr _daMgr = null;
        EngineStatusEnum _engineStatus = EngineStatusEnum.Off;
        byte _maxTaskProcesses = 1;
        byte _tasksInProcess = 0;
        object _taskCounterLock = new object();
        ManualResetEvent _stopEvent = new ManualResetEvent(false);
        ManualResetEvent _resumeEvent = new ManualResetEvent(false);
        System.Threading.Thread _mainThread = null;

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
        public TaskProcessEngine(DataAccessMgr daMgr, byte maxTaskProcesses = 1)
        {
            if (daMgr.loggingMgr == null)
                throw new ExceptionEvent(enumExceptionEventCodes.NullOrEmptyParameter
                    , "TaskProcessingEngine requires a DataAccessMgr with a LoggingMgr.");
            _daMgr = daMgr;
            _maxTaskProcesses = maxTaskProcesses;
            _engineStatus = EngineStatusEnum.Started;
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
            TaskQueueStructure tqs = new TaskQueueStructure();
            tqs.Payload = "Sample Payload";
            tqs.TaskId = "B1.Test.SampleTasks.SampleTaskRun1Minute";
            tqs.AssemblyPath = @"C:\B1\Devel\Framework\SourceCode\Test\B1.Test.SampleTasks\bin\Debug";
            tqs.AssemblyName = "B1.Test.SampleTasks";
            return tqs;
        }
    }
}
