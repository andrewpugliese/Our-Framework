using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

using B1.DataAccess;

namespace B1.TaskProcessing
{
    public abstract class TaskProcess
    {
        public enum ProcessStatusEnum { Ready = 0, Working = 1, Paused = 2, Stopped = 3, Completed = 4, Failed = -1 };
        public enum TaskStatusEnum { InProcess = 1, Completed = 2, Failed = 3 };
        public delegate void TaskCompletedDelegate(string taskId, ProcessStatusEnum processStatus);
        protected DataAccessMgr _daMgr = null;
        protected ProcessStatusEnum _processStatus = ProcessStatusEnum.Ready;
        protected string _parameters = null;
        ManualResetEvent _stopEvent = new ManualResetEvent(false);
        ManualResetEvent _resumeEvent = new ManualResetEvent(false);
        TaskCompletedDelegate _taskCompletedHandler = null;
        string _taskId = null;

        public TaskProcess(DataAccessMgr daMgr
            , string taskId
            , string parameters
            , TaskCompletedDelegate taskCompletedHandler)
        {
            _daMgr = daMgr;
            _taskId = taskId;
            _parameters = parameters;
            _taskCompletedHandler = taskCompletedHandler;
            _processStatus = ProcessStatusEnum.Ready;
        }

        public void Start()
        {
            try
            {
                while (_processStatus != ProcessStatusEnum.Stopped)
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
                        _taskCompletedHandler(_taskId, _processStatus);
                    }

                }
            }
            catch
            {
                _processStatus = ProcessStatusEnum.Failed;
            }
            finally
            {
                _taskCompletedHandler(_taskId, _processStatus);
            }
        }

        public abstract TaskStatusEnum TaskFunctionBody(string parameters);
        public abstract string TaskDescription();
        public abstract string TaskStatus();

        public void Stop()
        {
            _processStatus = ProcessStatusEnum.Stopped;
            _stopEvent.Set();   // signal to stop
        }

        public void Pause()
        {
            _processStatus = ProcessStatusEnum.Paused;
        }

        public void Resume()
        {
            _processStatus = ProcessStatusEnum.Working;
            _resumeEvent.Set();   // signal to resume
        }

        public string Status()
        {
            return string.Format("Process Status: {0} Task Status Msg: {1}{2}"
                , _processStatus.ToString(), TaskStatus(), Environment.NewLine);
        }
    }
}
