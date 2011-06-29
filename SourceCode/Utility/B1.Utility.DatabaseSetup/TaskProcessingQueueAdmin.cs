using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Data;
using System.Data.Common;

using B1.DataAccess;

namespace B1.Utility.DatabaseSetup
{
    public partial class TaskProcessingQueueAdmin : Form
    {
        DataAccessMgr _daMgr = null;
        DataRow _taskQueueItem = null;
        
        public TaskProcessingQueueAdmin(DataAccessMgr daMgr, DataRow taskQueueItem)
        {
            InitializeComponent();
            _daMgr = daMgr;
            _taskQueueItem = taskQueueItem;
            PopulateForm();   
        }

        void PopulateForm()
        {
            if (_taskQueueItem == null)
            {
                tbTaskQueueCode.Text = _daMgr.GetNextSequenceNumber(TaskProcessingFunctions.Constants.TaskQueueCode).ToString();
                tbTPQStatusCode.Text = Convert.ToByte(TaskProcessingFunctions.TaskProcessingQueue.QueueListEnum.Queued).ToString();
            }
            else
            {
                tbTaskQueueCode.Text = _taskQueueItem[TaskProcessingFunctions.Constants.TaskQueueCode].ToString();
            }
        }
    }
}
