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
    public partial class TaskProcessingQueueAdmin : Form
    {
        DataAccessMgr _daMgr = null;
        DataRow _taskQueueItem = null;
        Dictionary<string, object> _editedColumns = new Dictionary<string, object>(StringComparer.CurrentCultureIgnoreCase);
        DataTable _taskIds = null;
        Int32? _userCode = null;
        
        public TaskProcessingQueueAdmin(DataAccessMgr daMgr, DataRow taskQueueItem, Int32? userCode)
        {
            InitializeComponent();
            dtpStatusDateTime.Format  = dtpCompletedDateTime.Format = dtpStartedDateTime.Format = dtpWaitForDateTime.Format 
                    = DateTimePickerFormat.Custom;
            dtpStatusDateTime.CustomFormat = dtpCompletedDateTime.CustomFormat = dtpStartedDateTime.CustomFormat 
                    = dtpWaitForDateTime.CustomFormat = "yyyy/MM/dd hh:mm:ss";
            _daMgr = daMgr;
            _taskQueueItem = taskQueueItem;
            _userCode = userCode;
            PopulateForm();   
        }

        public Dictionary<string, object> EditedColumns
        {
            get { return _editedColumns; }
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
                dtpCompletedDateTime.Enabled = dtpStartedDateTime.Enabled = dtpWaitForDateTime.Enabled = false;
                cbCompletedDtNull.Checked = cbStartedDtNull.Checked = cbWaitedDtNull.Checked = true;
                dtpStatusDateTime.Visible = lblStatusDateTime.Visible = false;
                cbWaitForTasks.Enabled = btnDelDepTask.Enabled = btnAddDepTask.Enabled = btnChangeDepTask.Enabled = false;
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
                else
                {
                    cbCompletedDtNull.Checked = true;
                    dtpCompletedDateTime.Enabled = false;
                }

                dtpStatusDateTime.Value = Convert.ToDateTime(_taskQueueItem[TaskProcessing.Constants.StatusDateTime]);

                if (_taskQueueItem[TaskProcessing.Constants.StartedDateTime] != DBNull.Value)
                    dtpStartedDateTime.Value = Convert.ToDateTime(_taskQueueItem[TaskProcessing.Constants.StartedDateTime]);
                else
                {
                    cbStartedDtNull.Checked = true;
                    dtpStartedDateTime.Enabled = false;
                }

                if (_taskQueueItem[TaskProcessing.Constants.WaitForDateTime] != DBNull.Value)
                    dtpWaitForDateTime.Value = Convert.ToDateTime(_taskQueueItem[TaskProcessing.Constants.WaitForDateTime]);
                else
                {
                    cbWaitedDtNull.Checked = true;
                    dtpWaitForDateTime.Enabled = false;
                }

                tbTaskParams.Text = _taskQueueItem[TaskProcessing.Constants.TaskParameters].ToString();
                tbTaskRemarks.Text = _taskQueueItem[TaskProcessing.Constants.TaskRemark].ToString();
                tbWaitForEngine.Text = _taskQueueItem[TaskProcessing.Constants.WaitForEngineId].ToString();
                tbWaitForConfig.Text = _taskQueueItem[TaskProcessing.Constants.WaitForConfigId].ToString();

                if (_taskQueueItem[TaskProcessing.Constants.WaitForNoUsers] != DBNull.Value)
                    cbWaitNoUsers.Checked = Convert.ToBoolean(_taskQueueItem[TaskProcessing.Constants.WaitForNoUsers]);

                cbWaitForTasks.Checked = Convert.ToBoolean(_taskQueueItem[TaskProcessing.Constants.WaitForTasks]);
                btnAddDepTask.Enabled = btnChangeDepTask.Enabled = btnDelDepTask.Enabled = cbWaitForTasks.Checked;

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
                            , cmbTaskId.SelectedItem);

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

                if (Convert.ToBoolean(_taskQueueItem[TaskProcessing.Constants.WaitForTasks])
                        != cbWaitForTasks.Checked)
                    _editedColumns.Add(TaskProcessing.Constants.WaitForTasks
                            , cbWaitForTasks.Checked);

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

        private void cbWaitForTasks_CheckedChanged(object sender, EventArgs e)
        {
            if (cbWaitForTasks.Checked)
            {
                btnAddDepTask.Enabled = btnChangeDepTask.Enabled = btnDelDepTask.Enabled = dgvWaitForTasks.Enabled = true;
                RefreshDependentTasks();
            }
            else btnAddDepTask.Enabled = btnChangeDepTask.Enabled = btnDelDepTask.Enabled = dgvWaitForTasks.Enabled = false;
        }

        void RefreshDependentTasks()
        {
            dgvWaitForTasks.DataSource = TaskProcessing.TaskProcessingQueue.TaskDependenciesList(_daMgr, Convert.ToInt32(tbTaskQueueCode.Text));
            dgvWaitForTasks.Refresh();
        }

        private void btnAddDepTask_Click(object sender, EventArgs e)
        {
            DependentTaskAdmin taskAdmin = new DependentTaskAdmin(_daMgr, tbTaskQueueCode.Text, null);
            DialogResult dr = taskAdmin.ShowDialog();
            if (dr == System.Windows.Forms.DialogResult.OK)
            {
                Int32 rowsChanged = _daMgr.ExecuteNonQuery(TaskProcessing.TaskProcessingQueue.GetDependencyDmlCmd(
                            _daMgr
                            , null
                            , taskAdmin.EditedColumns
                            , _userCode)
                        , null
                        , null);
                RefreshDependentTasks();
            }
        }

        private void btnChangeDepTask_Click(object sender, EventArgs e)
        {
            if (dgvWaitForTasks.RowCount == 0)
                MessageBox.Show("No rows found, nothing to change.");
            else if (dgvWaitForTasks.SelectedRows.Count == 0)
                MessageBox.Show("No rows select. Please select");
            else
            {
                DependentTaskAdmin taskAdmin = new DependentTaskAdmin(_daMgr
                        , tbTaskQueueCode.Text
                        , (dgvWaitForTasks.CurrentRow.DataBoundItem as DataRowView).Row);
                DialogResult dr = taskAdmin.ShowDialog();
                if (dr == System.Windows.Forms.DialogResult.OK)
                {
                    Int32 rowsChanged = _daMgr.ExecuteNonQuery(TaskProcessing.TaskProcessingQueue.GetDependencyDmlCmd(
                                _daMgr
                                , (dgvWaitForTasks.CurrentRow.DataBoundItem as DataRowView).Row
                                , taskAdmin.EditedColumns
                                , _userCode)
                            , null
                            , null);
                    RefreshDependentTasks();
                }
             }

        }

        private void btnDelDepTask_Click(object sender, EventArgs e)
        {
            if (dgvWaitForTasks.RowCount == 0)
                MessageBox.Show("No rows found, nothing to delete.");
            else if (dgvWaitForTasks.SelectedRows.Count == 0)
                MessageBox.Show("No rows select. Please select");
            else
            {
                DialogResult dlg = MessageBox.Show("Are you sure you want to delete this queue item?"
                        , "Delete Queue Item"
                        , MessageBoxButtons.YesNo);
                if (dlg == System.Windows.Forms.DialogResult.Yes)
                {
                    DataRow dr = (dgvWaitForTasks.CurrentRow.DataBoundItem as DataRowView).Row;
                    _daMgr.ExecuteNonQuery(TaskProcessing.TaskProcessingQueue.GetDeleteDependencyTaskCmd(_daMgr, dr)
                            , null, null);
                }
                RefreshDependentTasks();
            }

        }
    }
}
