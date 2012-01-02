using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Data.Common;

using B1.DataAccess;
using B1.ILoggingManagement;

namespace B1.Utility.DatabaseSetup
{
    /// <summary>
    /// Class for editing the dependancy tasks of a task process
    /// </summary>
    public partial class DependentTaskAdmin : Form
    {
        DataAccessMgr _daMgr = null;
        DataRow _taskQueueItem = null;
        Dictionary<string, object> _editedColumns = new Dictionary<string, object>(StringComparer.CurrentCultureIgnoreCase);
        DataTable _taskIds = null;
        DataTable _completionCodes = null;
        string _taskQueueCode = null;

        /// <summary>
        /// Constructor for the dependency tasks dialog
        /// </summary>
        /// <param name="daMgr">DataAccessMgr object</param>
        /// <param name="taskQueueCode">The task's unique code that has the dependency</param>
        /// <param name="taskQueueItem">DataRow object containing all the dependency tasks details</param>
        public DependentTaskAdmin(DataAccessMgr daMgr, string taskQueueCode, DataRow taskQueueItem)
        {
            InitializeComponent();
            _daMgr = daMgr;
            _taskQueueCode = taskQueueCode;
            _taskQueueItem = taskQueueItem;
            PopulateForm();
        }

        /// <summary>
        /// Property returns a Dictionary of all columns and their new values 
        /// that have changed from the edit.
        /// </summary>
        public Dictionary<string, object> EditedColumns
        {
            get { return _editedColumns; }
        }

        void PopulateForm()
        {
            if (_taskQueueItem == null) // add new item
            {
                btnSave.Text = "Add";
            }
            else
            {
                btnSave.Text = "Change";
                LoadWaitTasks();
                cmbWaitTasks.SelectedItem = _taskQueueItem[TaskProcessing.Constants.WaitTaskQueueCode].ToString();
                LoadCompletionCodes();
            }
            tbTaskQueueCode.Text = _taskQueueCode;
        }


        private void btnSave_Click(object sender, EventArgs e)
        {
            if (cmbWaitTasks.SelectedItem == null)
            {
                MessageBox.Show("Please selet TaskId");
                cmbWaitTasks.Focus();
                return;
            }
            if (cmbWaitTaskCompCodes.SelectedItem == null)
            {
                MessageBox.Show("Please selet completion code");
                cmbWaitTaskCompCodes.Focus();
                return;
            }
            if (_taskQueueItem == null)
            {
                _editedColumns.Add(TaskProcessing.Constants.WaitTaskQueueCode
                        , GetWaitTaskQueueCode());
              //  _editedColumns.Add(TaskProcessing.Constants.WaitTaskId
                //        , GetWaitTaskId());
                _editedColumns.Add(TaskProcessing.Constants.WaitTaskCompletionCode
                        , GetWaitCompletionCode());
            }
            else
            {
                if (!Convert.ToInt32(_taskQueueItem[TaskProcessing.Constants.WaitTaskQueueCode]).Equals(
                    GetWaitTaskQueueCode()))
                    _editedColumns.Add(TaskProcessing.Constants.WaitTaskQueueCode
                            , GetWaitTaskQueueCode());

                if (!Convert.ToByte(_taskQueueItem[TaskProcessing.Constants.WaitTaskCompletionCode]).Equals(
                    GetWaitCompletionCode()))
                    _editedColumns.Add(TaskProcessing.Constants.WaitTaskCompletionCode
                            , GetWaitCompletionCode());
            }
            // before processing storing selection in the database, we need to verify that it would
            // not result in an endless loop of dependencies
            string deadlockMsg = IsDependencyDeadlock(Convert.ToInt32(tbTaskQueueCode.Text)
                    , GetWaitTaskQueueCode());
            if (!string.IsNullOrEmpty(deadlockMsg))
            {
                MessageBox.Show(deadlockMsg);
                _editedColumns.Clear();
                return;
            }

            if (_editedColumns.Count > 0)
                _editedColumns.Add(TaskProcessing.Constants.TaskQueueCode
                        , Convert.ToInt64(tbTaskQueueCode.Text));
            this.DialogResult = System.Windows.Forms.DialogResult.OK;
        }

        Int32 GetWaitTaskQueueCode()
        {
            return Convert.ToInt32(cmbWaitTasks.SelectedItem.ToString().Split(new char[] { ':' })[0]);
        }

        string GetWaitTaskId()
        {
            return cmbWaitTasks.SelectedItem.ToString().Split(new char[] { ':' })[1];
        }

        byte GetWaitCompletionCode()
        {
            return Convert.ToByte(cmbWaitTaskCompCodes.SelectedItem.ToString().Split(new char[] { ':' })[0]);
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = System.Windows.Forms.DialogResult.Cancel;
        }

        private void cmbWaitTaskCompCodes_DropDown(object sender, EventArgs e)
        {
            if (cmbWaitTaskCompCodes.Items.Count == 0)
                LoadCompletionCodes();
        }

        private void cmbWaitTasks_DropDown(object sender, EventArgs e)
        {
            if (cmbWaitTasks.Items.Count == 0)
                LoadWaitTasks();
        }

        void LoadWaitTasks()
        {
            if (_taskIds == null)
            {
                DbTableDmlMgr dmlMgr = _daMgr.DbCatalogGetTableDmlMgr(DataAccess.Constants.SCHEMA_CORE
                        , TaskProcessing.Constants.TaskProcessingQueue
                        , TaskProcessing.Constants.TaskQueueCode
                        , TaskProcessing.Constants.TaskId);
                dmlMgr.SetWhereCondition(w => w.Column(TaskProcessing.Constants.TaskQueueCode)
                        != w.Parameter(TaskProcessing.Constants.TaskQueueCode));
                DbCommand dbCmd = _daMgr.BuildSelectDbCommand(dmlMgr, null);
                dbCmd.Parameters[_daMgr.BuildParamName(TaskProcessing.Constants.TaskQueueCode)].Value = _taskQueueCode;

                _taskIds = _daMgr.ExecuteDataSet(dbCmd, null, null).Tables[0];
                foreach (DataRow taskId in _taskIds.Rows)
                {
                    cmbWaitTasks.Items.Add(string.Format("{0}:{1}"
                        , taskId[TaskProcessing.Constants.TaskQueueCode]
                        , taskId[TaskProcessing.Constants.TaskId]));
                    if (_taskQueueItem != null
                        && taskId[TaskProcessing.Constants.TaskQueueCode].ToString()
                        == _taskQueueItem[TaskProcessing.Constants.WaitTaskQueueCode].ToString())
                        cmbWaitTasks.SelectedIndex = cmbWaitTasks.Items.Count - 1;
                }
            }
        }

        void LoadCompletionCodes()
        {
            if (_completionCodes == null)
            {
                DbTableDmlMgr dmlMgr = _daMgr.DbCatalogGetTableDmlMgr(DataAccess.Constants.SCHEMA_CORE
                        , TaskProcessing.Constants.TaskStatusCodes
                        , TaskProcessing.Constants.StatusCode
                        , TaskProcessing.Constants.StatusName);
                dmlMgr.SetWhereCondition(w => w.Column(TaskProcessing.Constants.StatusCode) 
                        >= w.Value(Convert.ToByte(TaskProcessing.TaskProcessingQueue.StatusCodeEnum.Failed)));
                _completionCodes = _daMgr.ExecuteDataSet(_daMgr.BuildSelectDbCommand(dmlMgr, null), null, null).Tables[0];
                foreach (DataRow completionCode in _completionCodes.Rows)
                {
                    cmbWaitTaskCompCodes.Items.Add(string.Format("{0}:{1}"
                        , completionCode[TaskProcessing.Constants.StatusCode]
                        , completionCode[TaskProcessing.Constants.StatusName]));
                    if (_taskQueueItem != null
                        && completionCode[TaskProcessing.Constants.StatusCode].ToString()
                        == _taskQueueItem[TaskProcessing.Constants.WaitTaskCompletionCode].ToString())
                        cmbWaitTaskCompCodes.SelectedIndex = cmbWaitTaskCompCodes.Items.Count - 1;
                }
            }
        }

        /// <summary>
        /// Returns a string indicating whether the given dependency will result in a deadlock
        /// If null, then no deadlock; otherwise the result is a description
        /// </summary>
        /// <param name="taskQueueCode">The task to add the dependency relation</param>
        /// <param name="waitTaskQueueCode">The task on which it will be dependent on</param>
        /// <returns>Null, if there are no deadlocks; or a message describing the deadlock</returns>
        string IsDependencyDeadlock(Int32 taskQueueCode, Int32 waitTaskQueueCode)
        {
            DataTable dependencyTasks = TaskProcessing.TaskProcessingQueue.GetDependentTasks(
                    _daMgr, taskQueueCode);
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("The given dependency: {0} depends on {1}, would cause a deadlock because "
                    , taskQueueCode, waitTaskQueueCode);
            sb.AppendFormat("there is a chain of dependencies that leads back to {0}.{1}"
                    , taskQueueCode
                    , Environment.NewLine);
            foreach (DataRow dependency in dependencyTasks.Rows)
            {
                Int32 dependentTaskCode = Convert.ToInt32(dependency[TaskProcessing.Constants.TaskQueueCode]);
                Int32 dependencyTaskCode = Convert.ToInt32(dependency[TaskProcessing.Constants.WaitTaskQueueCode]);
                Int32 level = Convert.ToInt32(dependency["Level"]);
                sb.AppendFormat("{0} depends on {1} at Level: {2}{3}"
                    , dependentTaskCode
                    , dependencyTaskCode
                    , level
                    , Environment.NewLine);
                if (dependentTaskCode == waitTaskQueueCode) // we have a deadlock
                    return sb.ToString();
            }
            return null; // no deadlocks
        }
    }
}
