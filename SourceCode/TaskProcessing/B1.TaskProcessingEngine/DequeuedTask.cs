using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.Common;

namespace B1.TaskProcessing
{
    /// <summary>
    /// Used by the TaskProcessingEngine (TPE) as a data structure to pass data 'popped' from the TPQ 
    /// </summary>
    internal class DequeuedTask
    {
        DataRow _dequeuedTaskData;

        /// <summary>
        /// Stores the queued task Datarow
        /// </summary>
        /// <param name="dequeuedTaskData"></param>
        internal DequeuedTask(DataRow dequeuedTaskData)
        {
            _dequeuedTaskData = dequeuedTaskData;
        }

        /// <summary>
        /// Returns task Id from the dequeued data
        /// </summary>
        internal string TaskId
        {
            get { return _dequeuedTaskData[TaskProcessing.Constants.TaskId].ToString(); }
        }

        /// <summary>
        /// Returns the task queue code from the dequeued data
        /// </summary>
        internal int TaskQueueCode 
        { 
            get { return Convert.ToInt32(_dequeuedTaskData[TaskProcessing.Constants.TaskQueueCode]); } 
        }

        /// <summary>
        /// Returns the list of parameters from the dequeued data 
        /// </summary>
        internal string Parameters 
        { 
            get { return _dequeuedTaskData[TaskProcessing.Constants.TaskParameters].ToString(); } 
        }

        /// <summary>
        /// Returns the Assembly Name parameter from the dequeued data 
        /// </summary>
        internal string AssemblyName
        {
            get { return _dequeuedTaskData[TaskProcessing.Constants.AssemblyName].ToString(); }
        }

        /// <summary>
        /// Returns the Task Parameters parameter from the dequeued data 
        /// </summary>
        internal string TaskParameters
        {
            get { return _dequeuedTaskData[TaskProcessing.Constants.TaskParameters].ToString(); }
        }

        /// <summary>
        /// Returns the Clear Parameter At End parameter from the dequeued data 
        /// </summary>
        internal bool ClearParametersAtEnd
        {
            get { return Convert.ToBoolean(_dequeuedTaskData[TaskProcessing.Constants.ClearParametersAtEnd]); }
        }

        /// <summary>
        /// Returns the Interval seconds to requeue parameter from the dequeued data 
        /// </summary>
        internal int IntervalSecondsRequeue
        {
            get { return Convert.ToInt32(_dequeuedTaskData[TaskProcessing.Constants.IntervalSecondsRequeue]); }
        }

        /// <summary>
        /// Returns the Interval Count parameter from the dequeued data 
        /// </summary>
        internal int IntervalCount
        {
            get { return Convert.ToInt32(_dequeuedTaskData[TaskProcessing.Constants.IntervalCount]); }
        }

        /// <summary>
        /// Returns the dequeued data DataRow
        /// </summary>
        internal DataRow DequeuedTaskData
        {
            get { return _dequeuedTaskData; }
        }

    }
}
