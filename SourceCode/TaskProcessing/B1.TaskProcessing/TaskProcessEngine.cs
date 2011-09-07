using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

using B1.DataAccess;

namespace B1.TaskProcessing
{

    internal struct TaskDataStructure
    {
        public string TaskId;
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
    public class TaskProcessEngine
    {
        public enum EngineStatusEnum { Off = 0, Started = 1, Running = 2, Paused = 3, Stopped = 4};
        DataAccessMgr _daMgr = null;
        EngineStatusEnum _engineStatus = EngineStatusEnum.Off;
        byte _maxTaskHandlers = 1;
        byte _tasksInProcess = 0;
        ManualResetEvent _stopEvent = new ManualResetEvent(false);
        ManualResetEvent _resumeEvent = new ManualResetEvent(false);
        Dictionary<string, Thread> _taskHandlerThreads = new Dictionary<string, Thread>(StringComparer.CurrentCultureIgnoreCase);
        Dictionary<string, object> _taskHandlers = new Dictionary<string, object>(StringComparer.CurrentCultureIgnoreCase);


        /// <summary>
        /// Constructs a new instance of the TaskProcessingEngine class
        /// </summary>
        /// <param name="daMgr">DataAccessMgr object instance</param>
        /// <param name="maxTaskHandlers">Configures the engine for a maximum number of concurrent task handlers</param>
        public TaskProcessEngine(DataAccessMgr daMgr, byte maxTaskHandlers = 1)
        {
            _daMgr = daMgr;
            _maxTaskHandlers = maxTaskHandlers;
            _engineStatus = EngineStatusEnum.Started;
        }

        /// <summary>
        /// Initiates the dequeuing of tasks from the queue
        /// </summary>
        public void Start()
        {
            _engineStatus = EngineStatusEnum.Running;
            while (_engineStatus != EngineStatusEnum.Stopped)
            {
                while (_engineStatus == EngineStatusEnum.Running)
                {
                    while (_tasksInProcess < _maxTaskHandlers)
                    {
                        TaskDataStructure? task = DequeueTask();
                        if (task.HasValue)
                            ProcessTask(task.Value);
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

        void ProcessTask(TaskDataStructure task)
        {
            _taskHandlers.Add(task.ToString(), task);
           // _taskHandlerThreads.Add(task.TaskId, new Thread());
        }

        public void Stop()
        {
            _engineStatus = EngineStatusEnum.Stopped;
            _stopEvent.Set();   // signal to stop
            foreach (object task in _taskHandlers.Keys)
            {
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
                , _engineStatus.ToString(), _maxTaskHandlers - _tasksInProcess, _tasksInProcess, Environment.NewLine);
        }

        TaskDataStructure? DequeueTask()
        {
            return null;
        }
    }
}
