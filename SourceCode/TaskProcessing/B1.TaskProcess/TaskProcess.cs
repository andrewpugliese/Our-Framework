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
    public abstract class TaskProcess
    {
        public enum ProcessStatusEnum { Ready = 0, Working = 1, Paused = 2, Stopped = 3, Completed = 4, Failed = -1 };
        public enum TaskStatusEnum { InProcess = 1, Completed = 2, Failed = 3 };
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

        public void Start()
        {
            using (LoggingContext lc = new LoggingContext(string.Format("ThreadPool: {0}; TaskProcess QueueCode: {1}"
                    , _threadPoolLabel, _taskQueueCode)))
            {
                try
                {
                    while (_processStatus == ProcessStatusEnum.Ready
                          || _processStatus == ProcessStatusEnum.Working)
                    {
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

                        TaskStatusEnum taskStatus = TaskFunctionBody(_parameters);
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
                    _processStatus = ProcessStatusEnum.Failed;
                }
                finally
                {
                    _taskCompletedHandler(_taskQueueCode, _processStatus);
                }
                _daMgr.loggingMgr.Trace("Finished", enumTraceLevel.Level5);
            }
        }

        public abstract TaskStatusEnum TaskFunctionBody(string parameters);
        public abstract string TaskDescription();
        public abstract string TaskStatus();

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

        public void Pause()
        {
            using (LoggingContext lc = new LoggingContext(string.Format("ThreadPool: {0}; TaskProcess QueueCode: {1}"
                    , _threadPoolLabel, _taskQueueCode)))
            {
                _daMgr.loggingMgr.Trace("Pausing", enumTraceLevel.Level5);
                _processStatus = ProcessStatusEnum.Paused;
            }
        }

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

        public string Status()
        {
            return string.Format("Process Status: {0} Task Status Msg: {1}{2}"
                , _processStatus.ToString(), TaskStatus(), Environment.NewLine);
        }
    }
}
