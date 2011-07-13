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

namespace B1.Utility.DatabaseSetup
{
    public partial class TaskProcessingQueueAdmin : Form
    {
        DataAccessMgr _daMgr = null;
        DataRow _taskQueueItem = null;
        Dictionary<string, object> _editedColumns = new Dictionary<string, object>(StringComparer.CurrentCultureIgnoreCase);
        DataTable _taskIds = null;
        
        public TaskProcessingQueueAdmin(DataAccessMgr daMgr, DataRow taskQueueItem)
        {
            InitializeComponent();
            _daMgr = daMgr;
            _taskQueueItem = taskQueueItem;
            PopulateForm();   
        }

        public static DbCommand GetDeleteQueueItemCmd(DataAccessMgr daMgr, DataRow taskQueueItem)
        {
            if (taskQueueItem == null
                || !taskQueueItem.Table.Columns.Contains(TaskProcessing.Constants.TaskQueueCode))
                throw new ArgumentException();
            DbTableDmlMgr dmlMgr = daMgr.DbCatalogGetTableDmlMgr(DataAccess.Constants.SCHEMA_CORE
                        , TaskProcessing.Constants.TaskProcessingQueue);
            dmlMgr.SetWhereCondition(w => w.Column(TaskProcessing.Constants.TaskQueueCode) 
                    == w.Parameter(TaskProcessing.Constants.TaskQueueCode));
            DbCommand dbCmd = daMgr.BuildDeleteDbCommand(dmlMgr);
            dbCmd.Parameters[daMgr.BuildParamName(TaskProcessing.Constants.TaskQueueCode)].Value 
                    = Convert.ToInt32(taskQueueItem[TaskProcessing.Constants.TaskQueueCode]);
            return dbCmd;
        }

        public Dictionary<string, object> EditedColumns
        {
            get { return _editedColumns; }
        }

        public DbCommand GetDmlCmd(Int32? userCode = null)
        {
            DbCommand dbCmd = null;
            DbTableDmlMgr dmlMgr = _daMgr.DbCatalogGetTableDmlMgr(DataAccess.Constants.SCHEMA_CORE
                        , TaskProcessing.Constants.TaskProcessingQueue);

            foreach(string column in _editedColumns.Keys)
                dmlMgr.AddColumn(column);
            if (_taskQueueItem == null) // add new item
            {
                dmlMgr.AddColumn(TaskProcessing.Constants.LastModifiedUserCode);
                dmlMgr.AddColumn(TaskProcessing.Constants.LastModifiedDateTime);
                dbCmd = _daMgr.BuildInsertDbCommand(dmlMgr);
            }

            else dbCmd = _daMgr.BuildChangeDbCommand(dmlMgr, TaskProcessing.Constants.LastModifiedUserCode
                    , TaskProcessing.Constants.LastModifiedDateTime);

            foreach (string column in _editedColumns.Keys)
                dbCmd.Parameters[_daMgr.BuildParamName(column)].Value
                        = _editedColumns[column];

            if (_taskQueueItem == null) // add new
            {
                if (userCode.HasValue)
                {
                    dbCmd.Parameters[_daMgr.BuildParamName(TaskProcessing.Constants.LastModifiedUserCode)].Value
                        = userCode.Value;
                    dbCmd.Parameters[_daMgr.BuildParamName(TaskProcessing.Constants.LastModifiedDateTime)].Value
                        = _daMgr.DbSynchTime;
                }
                else
                {
                    dbCmd.Parameters[_daMgr.BuildParamName(TaskProcessing.Constants.LastModifiedUserCode)].Value
                        = DBNull.Value;
                    dbCmd.Parameters[_daMgr.BuildParamName(TaskProcessing.Constants.LastModifiedDateTime)].Value
                        = DBNull.Value;
                }
            }
            else  // change; where condition params
            {
                dbCmd.Parameters[_daMgr.BuildParamName(TaskProcessing.Constants.LastModifiedUserCode)].Value
                    = _taskQueueItem[TaskProcessing.Constants.LastModifiedUserCode];
                dbCmd.Parameters[_daMgr.BuildParamName(TaskProcessing.Constants.LastModifiedDateTime)].Value
                    = _taskQueueItem[TaskProcessing.Constants.LastModifiedDateTime];
                // set portion of the update
                if (userCode.HasValue)
                {
                    dbCmd.Parameters[_daMgr.BuildParamName(TaskProcessing.Constants.LastModifiedUserCode, true)].Value
                        = userCode.Value;
                    dbCmd.Parameters[_daMgr.BuildParamName(TaskProcessing.Constants.LastModifiedDateTime, true)].Value
                        = _daMgr.DbSynchTime;
                }
                else
                {
                    dbCmd.Parameters[_daMgr.BuildParamName(TaskProcessing.Constants.LastModifiedUserCode, true)].Value
                        = DBNull.Value;
                    dbCmd.Parameters[_daMgr.BuildParamName(TaskProcessing.Constants.LastModifiedDateTime, true)].Value
                        = DBNull.Value;
                }
            }

            return dbCmd;
        }

        void PopulateForm()
        {
            dtpCompletedDateTime.Format = dtpStartedDateTime.Format = dtpStatusDateTime.Format = dtpWaitForDateTime.Format 
                    = DateTimePickerFormat.Time;
            if (_taskQueueItem == null) // add new item
            {
                btnSave.Text = "Add";
                tbTaskQueueCode.Text 
                        = _daMgr.GetNextUniqueId(TaskProcessing.Constants.TaskQueueCode, 1, Int32.MaxValue, 1).ToString();
            }
            else // change item
            {
                TaskProcessing.TaskProcessingQueue.StatusCodeEnum status =
                        (TaskProcessing.TaskProcessingQueue.StatusCodeEnum)
                        Convert.ToByte(_taskQueueItem[TaskProcessing.Constants.StatusCode]);
                rbTPQFailed.Enabled = rbTPQInProcess.Enabled = rbTPQSucceeded.Enabled = false;

                if (status == TaskProcessing.TaskProcessingQueue.StatusCodeEnum.InProcess)
                {
                    btnSave.Enabled = false;
                }
                else
                {
                    btnSave.Enabled = true;
                }

                btnSave.Text = "Change";
                tbTaskQueueCode.Text = _taskQueueItem[TaskProcessing.Constants.TaskQueueCode].ToString();

                LoadTaskIds();
                cmbTaskId.SelectedItem = _taskQueueItem[TaskProcessing.Constants.TaskId].ToString();
                nudPriority.Value = Convert.ToByte(_taskQueueItem[TaskProcessing.Constants.PriorityCode]);

                if (_taskQueueItem[TaskProcessing.Constants.CompletedDateTime] != DBNull.Value)
                    dtpCompletedDateTime.Value = Convert.ToDateTime(_taskQueueItem[TaskProcessing.Constants.CompletedDateTime]);
                else cbCompletedDtNull.Checked = true;

                dtpStatusDateTime.Value = Convert.ToDateTime(_taskQueueItem[TaskProcessing.Constants.StatusDateTime]);

                if (_taskQueueItem[TaskProcessing.Constants.StartedDateTime] != DBNull.Value)
                    dtpStartedDateTime.Value = Convert.ToDateTime(_taskQueueItem[TaskProcessing.Constants.StartedDateTime]);
                else cbStartedDtNull.Checked = true;

                if (_taskQueueItem[TaskProcessing.Constants.WaitForDateTime] != DBNull.Value)
                    dtpWaitForDateTime.Value = Convert.ToDateTime(_taskQueueItem[TaskProcessing.Constants.WaitForDateTime]);
                else cbWaitedDtNull.Checked = true;

                tbTaskParams.Text = _taskQueueItem[TaskProcessing.Constants.TaskParameters].ToString();
                tbTaskRemarks.Text = _taskQueueItem[TaskProcessing.Constants.TaskRemark].ToString();
                tbWaitForEngine.Text = _taskQueueItem[TaskProcessing.Constants.WaitForEngineId].ToString();
                tbWaitForConfig.Text = _taskQueueItem[TaskProcessing.Constants.WaitForConfigId].ToString();

                if (_taskQueueItem[TaskProcessing.Constants.WaitForNoUsers] != DBNull.Value)
                    cbWaitNoUsers.Checked = Convert.ToBoolean(_taskQueueItem[TaskProcessing.Constants.WaitForNoUsers]);

                cbWaitForTasks.Checked = Convert.ToBoolean(_taskQueueItem[TaskProcessing.Constants.WaitForTasks]);

                tbIntervalCount.Text = _taskQueueItem[TaskProcessing.Constants.IntervalCount].ToString();
                if (_taskQueueItem[TaskProcessing.Constants.IntervalSecondsRequeue] != DBNull.Value)
                    nudIntervalRequeueSec.Value = Convert.ToInt32(_taskQueueItem[TaskProcessing.Constants.IntervalSecondsRequeue]);
            }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (cmbTaskId.SelectedItem == null)
            {
                MessageBox.Show("Please selet TaskId");
                cmbTaskId.Focus();
                return;
            }
            if (_taskQueueItem == null) // add
            {
                // since this was an add, there is no datarow; all fields can be added to the return container
                _editedColumns.Add(TaskProcessing.Constants.TaskQueueCode
                        , Convert.ToInt64(tbTaskQueueCode.Text));
                _editedColumns.Add(TaskProcessing.Constants.TaskId
                        , cmbTaskId.SelectedItem.ToString());

                _editedColumns.Add(TaskProcessing.Constants.PriorityCode
                        , Convert.ToByte(nudPriority.Value));

                // if WaitDateTime is not null, add it
                if (!cbWaitedDtNull.Checked)
                    _editedColumns.Add(TaskProcessing.Constants.WaitForDateTime
                            , dtpWaitForDateTime.Value);

                if (rbTPQNotQueued.Checked)
                    _editedColumns.Add(TaskProcessing.Constants.StatusCode
                            , Convert.ToByte(TaskProcessing.TaskProcessingQueue.StatusCodeEnum.NotQueued));

                _editedColumns.Add(TaskProcessing.Constants.IntervalCount
                        , 0); 
                _editedColumns.Add(TaskProcessing.Constants.IntervalSecondsRequeue
                        , nudIntervalRequeueSec.Value);
 
                if (!string.IsNullOrEmpty(tbTaskParams.Text))
                    _editedColumns.Add(TaskProcessing.Constants.TaskParameters
                            , tbTaskParams.Text);

                _editedColumns.Add(TaskProcessing.Constants.ClearParametersAtEnd
                            , cbClearParam.Checked);

                _editedColumns.Add(TaskProcessing.Constants.WaitForNoUsers
                            , cbWaitNoUsers.Checked);

                _editedColumns.Add(TaskProcessing.Constants.WaitForTasks
                            , cbWaitForTasks.Checked);

                if (!string.IsNullOrEmpty(tbWaitForConfig.Text))
                    _editedColumns.Add(TaskProcessing.Constants.WaitForConfigId
                            , tbWaitForConfig.Text);

                if (!string.IsNullOrEmpty(tbWaitForEngine.Text))
                    _editedColumns.Add(TaskProcessing.Constants.WaitForEngineId
                            , tbWaitForEngine.Text);

                if (!string.IsNullOrEmpty(tbTaskRemarks.Text))
                    _editedColumns.Add(TaskProcessing.Constants.TaskRemark
                            , tbTaskRemarks.Text);
            }
            else // change
            {   
                // since this was a change, we compare the UI control to the datarow column; 
                // all changed fields can be added to the return container
                if (!_taskQueueItem[TaskProcessing.Constants.TaskQueueCode].ToString().Equals(
                        tbTaskQueueCode.Text))
                    _editedColumns.Add(TaskProcessing.Constants.TaskQueueCode
                            , Convert.ToInt64(tbTaskQueueCode.Text));

                if (!_taskQueueItem[TaskProcessing.Constants.TaskId].ToString().Equals(
                    cmbTaskId.SelectedItem.ToString()))
                    _editedColumns.Add(TaskProcessing.Constants.TaskId
                            , _taskQueueItem[TaskProcessing.Constants.TaskId]);

                if (!_taskQueueItem[TaskProcessing.Constants.PriorityCode].ToString().Equals(
                    Convert.ToByte(nudPriority.Value).ToString()))
                    _editedColumns.Add(TaskProcessing.Constants.PriorityCode
                        , Convert.ToByte(nudPriority.Value));

                TaskProcessing.TaskProcessingQueue.StatusCodeEnum status 
                        = TaskProcessing.TaskProcessingQueue.StatusCodeEnum.NotQueued;
                if (rbTPQFailed.Checked)
                    status = TaskProcessing.TaskProcessingQueue.StatusCodeEnum.Failed;
                if (rbTPQNotQueued.Checked)
                    status = TaskProcessing.TaskProcessingQueue.StatusCodeEnum.NotQueued;
                if (rbTPQqueued.Checked)
                    status = TaskProcessing.TaskProcessingQueue.StatusCodeEnum.Queued;
                if (rbTPQSucceeded.Checked)
                    status = TaskProcessing.TaskProcessingQueue.StatusCodeEnum.Succeeded;
                if (!(Convert.ToByte(_taskQueueItem[TaskProcessing.Constants.StatusCode])
                    == Convert.ToByte(status)))
                    _editedColumns.Add(TaskProcessing.Constants.StatusCode
                        , Convert.ToByte(status));

                if (Convert.ToDateTime(_taskQueueItem[TaskProcessing.Constants.WaitForDateTime])
                    != dtpWaitForDateTime.Value)
                    _editedColumns.Add(TaskProcessing.Constants.WaitForDateTime
                            , dtpWaitForDateTime.Value);

                if ((_taskQueueItem[TaskProcessing.Constants.StartedDateTime] != DBNull.Value
                    && (cbStartedDtNull.Checked || Convert.ToDateTime(_taskQueueItem[TaskProcessing.Constants.StartedDateTime])
                        != dtpStartedDateTime.Value))
                    || (_taskQueueItem[TaskProcessing.Constants.StartedDateTime] == DBNull.Value
                    && !cbStartedDtNull.Checked))
                    _editedColumns.Add(TaskProcessing.Constants.StartedDateTime
                            , dtpStartedDateTime.Value);

                if (status == TaskProcessing.TaskProcessingQueue.StatusCodeEnum.Queued)
                    _editedColumns.Add(TaskProcessing.Constants.CompletedDateTime
                            , DBNull.Value);

                if (Convert.ToBoolean(_taskQueueItem[TaskProcessing.Constants.WaitForNoUsers])
                        != cbWaitNoUsers.Checked)
                    _editedColumns.Add(TaskProcessing.Constants.WaitForNoUsers
                            , cbWaitNoUsers.Checked);

                if (Convert.ToBoolean(_taskQueueItem[TaskProcessing.Constants.ClearParametersAtEnd])
                        != cbClearParam.Checked)
                    _editedColumns.Add(TaskProcessing.Constants.ClearParametersAtEnd
                            , cbClearParam.Checked);

                if (Convert.ToInt32(_taskQueueItem[TaskProcessing.Constants.IntervalCount])
                    != Convert.ToInt32(tbIntervalCount.Text))
                    _editedColumns.Add(TaskProcessing.Constants.IntervalCount
                        , Convert.ToInt32(tbIntervalCount.Text));
                
                if (Convert.ToDateTime(_taskQueueItem[TaskProcessing.Constants.WaitForDateTime])
                    != dtpWaitForDateTime.Value)
                    _editedColumns.Add(TaskProcessing.Constants.WaitForDateTime
                            , dtpWaitForDateTime.Value);

                if (Convert.ToInt32(_taskQueueItem[TaskProcessing.Constants.IntervalSecondsRequeue])
                    != Convert.ToInt32(nudIntervalRequeueSec.Value))
                    _editedColumns.Add(TaskProcessing.Constants.IntervalSecondsRequeue
                            , Convert.ToInt32(nudIntervalRequeueSec.Value));

                if (!_taskQueueItem[TaskProcessing.Constants.WaitForConfigId].ToString().Equals(
                    tbWaitForConfig.Text))
                    _editedColumns.Add(TaskProcessing.Constants.WaitForConfigId
                            , tbWaitForConfig.Text);

                if (!_taskQueueItem[TaskProcessing.Constants.WaitForEngineId].ToString().Equals(
                    tbWaitForEngine.Text))
                    _editedColumns.Add(TaskProcessing.Constants.WaitForEngineId
                            , tbWaitForEngine.Text);

                if (!_taskQueueItem[TaskProcessing.Constants.TaskParameters].ToString().Equals(
                    tbTaskParams.Text))
                    _editedColumns.Add(TaskProcessing.Constants.TaskParameters
                        , tbTaskParams.Text);

                if (!_taskQueueItem[TaskProcessing.Constants.TaskRemark].ToString().Equals(
                    tbTaskRemarks.Text))
                    _editedColumns.Add(TaskProcessing.Constants.TaskRemark
                        , tbTaskRemarks.Text);
            }
            this.DialogResult = System.Windows.Forms.DialogResult.OK;
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = System.Windows.Forms.DialogResult.Cancel;
        }

        private void cbWaitedDtNull_CheckedChanged(object sender, EventArgs e)
        {
            dtpWaitForDateTime.Enabled = !cbWaitedDtNull.Checked;
        }

        private void cbStartedDtNull_CheckedChanged(object sender, EventArgs e)
        {
            dtpStartedDateTime.Enabled = !cbStartedDtNull.Checked;
        }

        private void cbCompletedDtNull_CheckedChanged(object sender, EventArgs e)
        {
            dtpCompletedDateTime.Enabled = !cbCompletedDtNull.Checked;
        }

        private void cmbTaskId_DropDown(object sender, EventArgs e)
        {
            LoadTaskIds();
        }

        void LoadTaskIds()
        {
            if (_taskIds == null)
            {
                DbTableDmlMgr dmlMgr = _daMgr.DbCatalogGetTableDmlMgr(DataAccess.Constants.SCHEMA_CORE
                        , TaskProcessing.Constants.TaskRegistrations, TaskProcessing.Constants.TaskId);
                _taskIds = _daMgr.ExecuteDataSet(_daMgr.BuildSelectDbCommand(dmlMgr, null), null, null).Tables[0];
                foreach (DataRow taskId in _taskIds.Rows)
                    cmbTaskId.Items.Add(taskId[TaskProcessing.Constants.TaskId].ToString());
            }
        }
    }
}
