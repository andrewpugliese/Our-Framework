using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using B1.TaskProcessing;
using B1.DataAccess;

namespace B1.Test.SampleTasks
{
    public class SampleTaskRun1Minute : TaskProcess
    {
        DateTime _started = DateTime.Now;
        int _count = 0;
        string _taskStatusMsg = null;

        public SampleTaskRun1Minute(DataAccessMgr daMgr
            , string taskId
            , string parameters
            , TaskCompletedDelegate taskCompletedHandler)
            : base(daMgr, taskId, parameters, taskCompletedHandler)
        {
        }

        public override TaskStatusEnum TaskFunctionBody(string payload)
        {
            _taskStatusMsg = this.ToString() + "; Iteration: " + _count.ToString();

            TimeSpan ts = DateTime.Now - _started;
            if (ts.TotalSeconds < 60)   
                return TaskStatusEnum.InProcess;
            else return TaskStatusEnum.Completed;
        }

        public override string TaskStatus()
        {
            return _taskStatusMsg;
        }

        public override string TaskDescription()
        {
            return "This is a sample task that will run for 60 seconds before completing.";
        }
    }
}
