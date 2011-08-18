using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.Common;

namespace B1.TaskProcessing
{
    internal class DequeuedTask
    {
        DataRow _dequeuedTaskData;

        internal DequeuedTask(DataRow dequeuedTaskData)
        {
            _dequeuedTaskData = dequeuedTaskData;
        }

        internal string TaskId
        {
            get { return _dequeuedTaskData[TaskProcessing.Constants.TaskId].ToString(); }
        }

        internal int TaskQueueCode 
        { 
            get { return Convert.ToInt32(_dequeuedTaskData[TaskProcessing.Constants.TaskQueueCode]); } 
        }

        internal string Parameters 
        { 
            get { return _dequeuedTaskData[TaskProcessing.Constants.TaskParameters].ToString(); } 
        }

        internal string AssemblyName
        {
            get { return _dequeuedTaskData[TaskProcessing.Constants.AssemblyName].ToString(); }
        }

        internal string TaskParameters
        {
            get { return _dequeuedTaskData[TaskProcessing.Constants.TaskParameters].ToString(); }
        }

        internal bool ClearParametersAtEnd
        {
            get { return Convert.ToBoolean(_dequeuedTaskData[TaskProcessing.Constants.ClearParametersAtEnd]); }
        }

        internal int IntervalSecondsRequeue
        {
            get { return Convert.ToInt32(_dequeuedTaskData[TaskProcessing.Constants.IntervalSecondsRequeue]); }
        }

        internal int IntervalCount
        {
            get { return Convert.ToInt32(_dequeuedTaskData[TaskProcessing.Constants.IntervalCount]); }
        }

        internal DataRow DequeuedTaskData
        {
            get { return _dequeuedTaskData; }
        }

    }
}
