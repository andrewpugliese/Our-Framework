using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

using B1.DataAccess;
using B1.ILoggingManagement;
using B1.LoggingManagement;

namespace B1.TaskProcessing
{
    /// <summary>
    /// This abstract class provides the interface and general functionality of all tasks processed by the TaskProcessingEngine (TPE).
    /// </summary>
    public abstract class TaskProcess
    {
#pragma warning disable 1591 // disable the xmlComments warning
        /// <summary>
        /// Enumeration of all the states of a tasks process
        /// </summary>
        public enum ProcessStatusEnum { Ready = 0, Working = 1, Paused = 2, Stopped = 3, Completed = 4, Failed = -1 };
        /// <summary>
        /// Enumeration of all the states of a task
        /// </summary>
        public enum TaskStatusEnum { InProcess = 1, Completed = 2, Failed = 3 };
#pragma warning disable 1591 // disable the xmlComments warning
        /// <summary>
        /// Delegate used to let caller program know when a task has completed.
        /// </summary>
        /// <param name="taskQueueCode">Unique identifier of the task record in the procesing queue</param>
        /// <param name="processStatus">ProcessStatusEnum on the state of the function</param>
        public delegate void TaskCompletedDelegate(int taskQueueCode, ProcessStatusEnum processStatus);
        protected DataAccessMgr _daMgr = null;
        protected ProcessStatusEnum _processStatus = ProcessStatusEnum.Ready;
        protected string _parameters = null;
        ManualResetEvent _stopEvent = new ManualResetEvent(false);
        ManualResetEvent _resumeEvent = new ManualResetEvent(false);
        TaskCompletedDelegate _taskCompletedHandler = null;
        string _taskId = null;
        int _taskQueueCode = 0;
        string _threadPoolLabel = null;

        /// <summary>
        /// Constructor for a new task process that can queued and processed by Transaction Processing Engine (TPE)
        /// </summary>
        /// <param name="daMgr">A data access manager object</param>
        /// <param name="taskId">Unique Task Identifier</param>
        /// <param name="taskQueueCode">Unique code for the entry in the task processing queue</param>
        /// <param name="parameters">Optional parameters to pass to function</param>
        /// <param name="taskCompletedHandler">A delegate which will be called when task is completed or fails</param>
        /// <param name="threadPoolLabel">A string identifier to be used for the set of threads created under this instance</param>
        public TaskProcess(DataAccessMgr daMgr
            , string taskId
            , int taskQueueCode
            , string parameters
            , TaskCompletedDelegate taskCompletedHandler
            , string threadPoolLabel)
        {
            _daMgr = daMgr;
            _taskId = taskId;
            _taskQueueCode = taskQueueCode;
            _parameters = parameters;
            _taskCompletedHandler = taskCompletedHandler;
            _threadPoolLabel = threadPoolLabel;
            _processStatus = ProcessStatusEnum.Ready;
        }
       
        /// <summary>
        /// Begins the task process
        /// <para>A task process will loop until the underlying function completes or fails.</para>
        /// <para>A process can be paused and resumed while it is in base class's body.  Once control passes to the underlying function
        /// , it is up to the task's designer to implement properly;  the framework does provide sample tasks however.</para>
        /// <para>A task designer has the option to implement a task to run continuously or repeatedly after stopping.</para>
        /// </summary>
        public void Start()
        {
            using (LoggingContext lc = new LoggingContext(string.Format("ThreadPool: {0}; TaskProcess QueueCode: {1}"
                    , _threadPoolLabel, _taskQueueCode)))
            {
                try
                {
                    // loop continuously until process has not completed
                    while (_processStatus == ProcessStatusEnum.Ready
                          || _processStatus == ProcessStatusEnum.Working)
                    {
                        // handle paused (and resume) and stop events
                        if (_processStatus == ProcessStatusEnum.Paused)
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

                        // make call to task's function body in seperate assembly.
                        // pass in the parameters
                        TaskStatusEnum taskStatus = TaskFunctionBody(_parameters);
                        // if task has completed or failed, then this process is over
                        // otherwise continue working and calling function again
                        if (taskStatus == TaskStatusEnum.Completed
                            || taskStatus == TaskStatusEnum.Failed)
                        {
                            if (taskStatus == TaskStatusEnum.Completed)
                                _processStatus = ProcessStatusEnum.Completed;
                            if (taskStatus == TaskStatusEnum.Failed)
                                _processStatus = ProcessStatusEnum.Failed;
                        }

                        _daMgr.loggingMgr.Trace("Working", enumTraceLevel.Level5);
                    }
                }
                catch
                {
                    // If there are any unhandled exceptions, then the task is considered failed
                    // and the process will terminate.
                    _processStatus = ProcessStatusEnum.Failed;
                }
                finally
                {
                    // whether it had succeeded or failed, we let delegate know
                    // that task completed and pass in the status
                    _taskCompletedHandler(_taskQueueCode, _processStatus);
                }
                _daMgr.loggingMgr.Trace("Finished", enumTraceLevel.Level5);
            }
        }

        /// <summary>
        /// Task Function Body which implements work
        /// </summary>
        /// <param name="parameters">String of parameter values</param>
        /// <returns>Enumerator indicating if function failed, completed, or is still working</returns>
        public abstract TaskStatusEnum TaskFunctionBody(string parameters);
        /// <summary>
        /// Describes what the function does
        /// </summary>
        /// <returns>A string description of the task</returns>
        public abstract string TaskDescription();
        /// <summary>
        /// Returns the current status of the function;  Can be a metric, a message, or a state; anything that is
        /// useful for the user to dispay or record.
        /// </summary>
        /// <returns>A string representation of the task's status</returns>
        public abstract string TaskStatus();
        /// <summary>
        /// Stops processing of the task by changing state and sending stopEvent signal.  All tasks to exit from processing.
        /// </summary>
        public void Stop()
        {
            using (LoggingContext lc = new LoggingContext(string.Format("ThreadPool: {0}; TaskProcess QueueCode: {1}"
                    , _threadPoolLabel, _taskQueueCode)))
            {
                _daMgr.loggingMgr.Trace("Stopping", enumTraceLevel.Level5);
                _processStatus = ProcessStatusEnum.Stopped;
                _stopEvent.Set();   // signal to stop
            }
        }
        /// <summary>
        /// Stops processing temporarily and waits for a resume (or stop) event.
        /// </summary>
        public void Pause()
        {
            using (LoggingContext lc = new LoggingContext(string.Format("ThreadPool: {0}; TaskProcess QueueCode: {1}"
                    , _threadPoolLabel, _taskQueueCode)))
            {
                _daMgr.loggingMgr.Trace("Pausing", enumTraceLevel.Level5);
                _processStatus = ProcessStatusEnum.Paused;
            }
        }
        /// <summary>
        /// Resumes processing which was temporarily paused.
        /// </summary>
        public void Resume()
        {
            using (LoggingContext lc = new LoggingContext(string.Format("ThreadPool: {0}; TaskProcess QueueCode: {1}"
                    , _threadPoolLabel, _taskQueueCode)))
            {
                _daMgr.loggingMgr.Trace("Resuming", enumTraceLevel.Level5);
                _processStatus = ProcessStatusEnum.Working;
                _resumeEvent.Set();   // signal to resume
            }
        }

        /// <summary>
        /// Returns status of the process as well as the task.
        /// </summary>
        /// <returns><A string representation of the status or state of the process and task./returns>
        public string Status()
        {
            return string.Format("Process Status: {0} Task Status Msg: {1}{2}"
                , _processStatus.ToString(), TaskStatus(), Environment.NewLine);
        }
    }
}
