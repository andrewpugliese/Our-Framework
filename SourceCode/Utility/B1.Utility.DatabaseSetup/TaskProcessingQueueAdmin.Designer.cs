namespace B1.Utility.DatabaseSetup
{
    partial class TaskProcessingQueueAdmin
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.cbWaitForTasks = new System.Windows.Forms.CheckBox();
            this.dgvWaitForTasks = new System.Windows.Forms.DataGridView();
            this.cbWaitNoUsers = new System.Windows.Forms.CheckBox();
            this.label7 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            this.label10 = new System.Windows.Forms.Label();
            this.label11 = new System.Windows.Forms.Label();
            this.cbClearParam = new System.Windows.Forms.CheckBox();
            this.tbTaskParams = new System.Windows.Forms.TextBox();
            this.tbWaitForEngine = new System.Windows.Forms.TextBox();
            this.tbWaitForConfig = new System.Windows.Forms.TextBox();
            this.tbTaskQueueCode = new System.Windows.Forms.TextBox();
            this.tbTaskRemarks = new System.Windows.Forms.TextBox();
            this.label12 = new System.Windows.Forms.Label();
            this.dtpWaitForDateTime = new System.Windows.Forms.DateTimePicker();
            this.dtpStartedDateTime = new System.Windows.Forms.DateTimePicker();
            this.dtpCompletedDateTime = new System.Windows.Forms.DateTimePicker();
            this.dtpStatusDateTime = new System.Windows.Forms.DateTimePicker();
            this.tbIntervalCount = new System.Windows.Forms.TextBox();
            this.label13 = new System.Windows.Forms.Label();
            this.label14 = new System.Windows.Forms.Label();
            this.nudIntervalRequeueSec = new System.Windows.Forms.NumericUpDown();
            this.btnSave = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.rbTPQNotQueued = new System.Windows.Forms.RadioButton();
            this.rbTPQqueued = new System.Windows.Forms.RadioButton();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.rbTPQSucceeded = new System.Windows.Forms.RadioButton();
            this.rbTPQFailed = new System.Windows.Forms.RadioButton();
            this.rbTPQInProcess = new System.Windows.Forms.RadioButton();
            this.nudPriority = new System.Windows.Forms.NumericUpDown();
            this.cbStartedDtNull = new System.Windows.Forms.CheckBox();
            this.cbWaitedDtNull = new System.Windows.Forms.CheckBox();
            this.cmbTaskId = new System.Windows.Forms.ComboBox();
            this.cbCompletedDtNull = new System.Windows.Forms.CheckBox();
            ((System.ComponentModel.ISupportInitialize)(this.dgvWaitForTasks)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudIntervalRequeueSec)).BeginInit();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudPriority)).BeginInit();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 29);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(88, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "TaskQueueCode";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(143, 31);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(40, 13);
            this.label2.TabIndex = 1;
            this.label2.Text = "TaskId";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(658, 27);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(63, 13);
            this.label4.TabIndex = 3;
            this.label4.Text = "PriorityCode";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(721, 27);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(83, 13);
            this.label5.TabIndex = 4;
            this.label5.Text = "StatusDateTime";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(901, 29);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(90, 13);
            this.label6.TabIndex = 5;
            this.label6.Text = "WaitForDateTime";
            // 
            // cbWaitForTasks
            // 
            this.cbWaitForTasks.AutoSize = true;
            this.cbWaitForTasks.Location = new System.Drawing.Point(22, 344);
            this.cbWaitForTasks.Name = "cbWaitForTasks";
            this.cbWaitForTasks.Size = new System.Drawing.Size(127, 17);
            this.cbWaitForTasks.TabIndex = 6;
            this.cbWaitForTasks.Text = "Wait For Other Tasks";
            this.cbWaitForTasks.UseVisualStyleBackColor = true;
            // 
            // dgvWaitForTasks
            // 
            this.dgvWaitForTasks.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvWaitForTasks.Location = new System.Drawing.Point(21, 367);
            this.dgvWaitForTasks.Name = "dgvWaitForTasks";
            this.dgvWaitForTasks.Size = new System.Drawing.Size(812, 140);
            this.dgvWaitForTasks.TabIndex = 7;
            // 
            // cbWaitNoUsers
            // 
            this.cbWaitNoUsers.AutoSize = true;
            this.cbWaitNoUsers.Location = new System.Drawing.Point(981, 151);
            this.cbWaitNoUsers.Name = "cbWaitNoUsers";
            this.cbWaitNoUsers.Size = new System.Drawing.Size(113, 17);
            this.cbWaitNoUsers.TabIndex = 8;
            this.cbWaitNoUsers.Text = "Wait For No Users";
            this.cbWaitNoUsers.UseVisualStyleBackColor = true;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(848, 189);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(92, 13);
            this.label7.TabIndex = 9;
            this.label7.Text = "Wait For EngineId";
            this.label7.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(979, 189);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(89, 13);
            this.label8.TabIndex = 10;
            this.label8.Text = "Wait For ConfigId";
            this.label8.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(285, 120);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(87, 13);
            this.label9.TabIndex = 11;
            this.label9.Text = "StartedDateTime";
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(487, 120);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(103, 13);
            this.label10.TabIndex = 12;
            this.label10.Text = "CompletedDateTime";
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(19, 179);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(84, 13);
            this.label11.TabIndex = 13;
            this.label11.Text = "TaskParameters";
            // 
            // cbClearParam
            // 
            this.cbClearParam.AutoSize = true;
            this.cbClearParam.Location = new System.Drawing.Point(124, 178);
            this.cbClearParam.Name = "cbClearParam";
            this.cbClearParam.Size = new System.Drawing.Size(122, 17);
            this.cbClearParam.TabIndex = 14;
            this.cbClearParam.Text = "Clear Params at End";
            this.cbClearParam.UseVisualStyleBackColor = true;
            // 
            // tbTaskParams
            // 
            this.tbTaskParams.Location = new System.Drawing.Point(22, 213);
            this.tbTaskParams.Name = "tbTaskParams";
            this.tbTaskParams.Size = new System.Drawing.Size(811, 20);
            this.tbTaskParams.TabIndex = 15;
            // 
            // tbWaitForEngine
            // 
            this.tbWaitForEngine.Location = new System.Drawing.Point(851, 213);
            this.tbWaitForEngine.Name = "tbWaitForEngine";
            this.tbWaitForEngine.Size = new System.Drawing.Size(125, 20);
            this.tbWaitForEngine.TabIndex = 16;
            // 
            // tbWaitForConfig
            // 
            this.tbWaitForConfig.Location = new System.Drawing.Point(982, 213);
            this.tbWaitForConfig.Name = "tbWaitForConfig";
            this.tbWaitForConfig.Size = new System.Drawing.Size(112, 20);
            this.tbWaitForConfig.TabIndex = 17;
            // 
            // tbTaskQueueCode
            // 
            this.tbTaskQueueCode.Location = new System.Drawing.Point(15, 58);
            this.tbTaskQueueCode.Name = "tbTaskQueueCode";
            this.tbTaskQueueCode.ReadOnly = true;
            this.tbTaskQueueCode.Size = new System.Drawing.Size(125, 20);
            this.tbTaskQueueCode.TabIndex = 18;
            // 
            // tbTaskRemarks
            // 
            this.tbTaskRemarks.Location = new System.Drawing.Point(22, 295);
            this.tbTaskRemarks.Name = "tbTaskRemarks";
            this.tbTaskRemarks.Size = new System.Drawing.Size(811, 20);
            this.tbTaskRemarks.TabIndex = 21;
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Location = new System.Drawing.Point(19, 261);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(73, 13);
            this.label12.TabIndex = 20;
            this.label12.Text = "TaskRemarks";
            // 
            // dtpWaitForDateTime
            // 
            this.dtpWaitForDateTime.Location = new System.Drawing.Point(904, 55);
            this.dtpWaitForDateTime.Name = "dtpWaitForDateTime";
            this.dtpWaitForDateTime.Size = new System.Drawing.Size(187, 20);
            this.dtpWaitForDateTime.TabIndex = 22;
            this.dtpWaitForDateTime.Value = new System.DateTime(2011, 7, 12, 3, 49, 47, 0);
            // 
            // dtpStartedDateTime
            // 
            this.dtpStartedDateTime.Location = new System.Drawing.Point(288, 150);
            this.dtpStartedDateTime.Name = "dtpStartedDateTime";
            this.dtpStartedDateTime.Size = new System.Drawing.Size(179, 20);
            this.dtpStartedDateTime.TabIndex = 23;
            // 
            // dtpCompletedDateTime
            // 
            this.dtpCompletedDateTime.Location = new System.Drawing.Point(490, 150);
            this.dtpCompletedDateTime.Name = "dtpCompletedDateTime";
            this.dtpCompletedDateTime.Size = new System.Drawing.Size(183, 20);
            this.dtpCompletedDateTime.TabIndex = 24;
            // 
            // dtpStatusDateTime
            // 
            this.dtpStatusDateTime.Enabled = false;
            this.dtpStatusDateTime.Location = new System.Drawing.Point(721, 55);
            this.dtpStatusDateTime.Name = "dtpStatusDateTime";
            this.dtpStatusDateTime.Size = new System.Drawing.Size(177, 20);
            this.dtpStatusDateTime.TabIndex = 25;
            // 
            // tbIntervalCount
            // 
            this.tbIntervalCount.Location = new System.Drawing.Point(720, 150);
            this.tbIntervalCount.Name = "tbIntervalCount";
            this.tbIntervalCount.Size = new System.Drawing.Size(70, 20);
            this.tbIntervalCount.TabIndex = 29;
            // 
            // label13
            // 
            this.label13.AutoSize = true;
            this.label13.Location = new System.Drawing.Point(717, 120);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(70, 13);
            this.label13.TabIndex = 28;
            this.label13.Text = "IntervalCount";
            // 
            // label14
            // 
            this.label14.AutoSize = true;
            this.label14.Location = new System.Drawing.Point(836, 120);
            this.label14.Name = "label14";
            this.label14.Size = new System.Drawing.Size(128, 13);
            this.label14.TabIndex = 30;
            this.label14.Text = "IntervalRequeueSeconds";
            // 
            // nudIntervalRequeueSec
            // 
            this.nudIntervalRequeueSec.Location = new System.Drawing.Point(840, 148);
            this.nudIntervalRequeueSec.Name = "nudIntervalRequeueSec";
            this.nudIntervalRequeueSec.Size = new System.Drawing.Size(123, 20);
            this.nudIntervalRequeueSec.TabIndex = 31;
            // 
            // btnSave
            // 
            this.btnSave.Location = new System.Drawing.Point(969, 402);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(75, 23);
            this.btnSave.TabIndex = 32;
            this.btnSave.Text = "Save";
            this.btnSave.UseVisualStyleBackColor = true;
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.Location = new System.Drawing.Point(969, 464);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 23);
            this.btnCancel.TabIndex = 33;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // rbTPQNotQueued
            // 
            this.rbTPQNotQueued.AutoSize = true;
            this.rbTPQNotQueued.Location = new System.Drawing.Point(11, 19);
            this.rbTPQNotQueued.Name = "rbTPQNotQueued";
            this.rbTPQNotQueued.Size = new System.Drawing.Size(83, 17);
            this.rbTPQNotQueued.TabIndex = 196;
            this.rbTPQNotQueued.Text = "Not Queued";
            this.rbTPQNotQueued.UseVisualStyleBackColor = true;
            // 
            // rbTPQqueued
            // 
            this.rbTPQqueued.AutoSize = true;
            this.rbTPQqueued.Checked = true;
            this.rbTPQqueued.Location = new System.Drawing.Point(104, 18);
            this.rbTPQqueued.Name = "rbTPQqueued";
            this.rbTPQqueued.Size = new System.Drawing.Size(63, 17);
            this.rbTPQqueued.TabIndex = 197;
            this.rbTPQqueued.TabStop = true;
            this.rbTPQqueued.Text = "Queued";
            this.rbTPQqueued.UseVisualStyleBackColor = true;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.rbTPQSucceeded);
            this.groupBox1.Controls.Add(this.rbTPQFailed);
            this.groupBox1.Controls.Add(this.rbTPQNotQueued);
            this.groupBox1.Controls.Add(this.rbTPQInProcess);
            this.groupBox1.Controls.Add(this.rbTPQqueued);
            this.groupBox1.Location = new System.Drawing.Point(22, 93);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(240, 66);
            this.groupBox1.TabIndex = 198;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Status";
            // 
            // rbTPQSucceeded
            // 
            this.rbTPQSucceeded.AutoSize = true;
            this.rbTPQSucceeded.Enabled = false;
            this.rbTPQSucceeded.Location = new System.Drawing.Point(148, 41);
            this.rbTPQSucceeded.Name = "rbTPQSucceeded";
            this.rbTPQSucceeded.Size = new System.Drawing.Size(80, 17);
            this.rbTPQSucceeded.TabIndex = 203;
            this.rbTPQSucceeded.Text = "Succeeded";
            this.rbTPQSucceeded.UseVisualStyleBackColor = true;
            // 
            // rbTPQFailed
            // 
            this.rbTPQFailed.AutoSize = true;
            this.rbTPQFailed.Enabled = false;
            this.rbTPQFailed.Location = new System.Drawing.Point(89, 41);
            this.rbTPQFailed.Name = "rbTPQFailed";
            this.rbTPQFailed.Size = new System.Drawing.Size(53, 17);
            this.rbTPQFailed.TabIndex = 202;
            this.rbTPQFailed.Text = "Failed";
            this.rbTPQFailed.UseVisualStyleBackColor = true;
            // 
            // rbTPQInProcess
            // 
            this.rbTPQInProcess.AutoSize = true;
            this.rbTPQInProcess.Enabled = false;
            this.rbTPQInProcess.Location = new System.Drawing.Point(11, 41);
            this.rbTPQInProcess.Name = "rbTPQInProcess";
            this.rbTPQInProcess.Size = new System.Drawing.Size(72, 17);
            this.rbTPQInProcess.TabIndex = 201;
            this.rbTPQInProcess.Text = "InProcess";
            this.rbTPQInProcess.UseVisualStyleBackColor = true;
            // 
            // nudPriority
            // 
            this.nudPriority.Location = new System.Drawing.Point(664, 55);
            this.nudPriority.Maximum = new decimal(new int[] {
            255,
            0,
            0,
            0});
            this.nudPriority.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.nudPriority.Name = "nudPriority";
            this.nudPriority.Size = new System.Drawing.Size(51, 20);
            this.nudPriority.TabIndex = 199;
            this.nudPriority.Value = new decimal(new int[] {
            32,
            0,
            0,
            0});
            // 
            // cbStartedDtNull
            // 
            this.cbStartedDtNull.AutoSize = true;
            this.cbStartedDtNull.Checked = true;
            this.cbStartedDtNull.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cbStartedDtNull.Location = new System.Drawing.Point(372, 119);
            this.cbStartedDtNull.Name = "cbStartedDtNull";
            this.cbStartedDtNull.Size = new System.Drawing.Size(44, 17);
            this.cbStartedDtNull.TabIndex = 201;
            this.cbStartedDtNull.Text = "Null";
            this.cbStartedDtNull.UseVisualStyleBackColor = true;
            this.cbStartedDtNull.CheckedChanged += new System.EventHandler(this.cbStartedDtNull_CheckedChanged);
            // 
            // cbWaitedDtNull
            // 
            this.cbWaitedDtNull.AutoSize = true;
            this.cbWaitedDtNull.Location = new System.Drawing.Point(1000, 31);
            this.cbWaitedDtNull.Name = "cbWaitedDtNull";
            this.cbWaitedDtNull.Size = new System.Drawing.Size(44, 17);
            this.cbWaitedDtNull.TabIndex = 202;
            this.cbWaitedDtNull.Text = "Null";
            this.cbWaitedDtNull.UseVisualStyleBackColor = true;
            this.cbWaitedDtNull.CheckedChanged += new System.EventHandler(this.cbWaitedDtNull_CheckedChanged);
            // 
            // cmbTaskId
            // 
            this.cmbTaskId.FormattingEnabled = true;
            this.cmbTaskId.Location = new System.Drawing.Point(146, 55);
            this.cmbTaskId.Name = "cmbTaskId";
            this.cmbTaskId.Size = new System.Drawing.Size(512, 21);
            this.cmbTaskId.TabIndex = 203;
            this.cmbTaskId.DropDown += new System.EventHandler(this.cmbTaskId_DropDown);
            // 
            // cbCompletedDtNull
            // 
            this.cbCompletedDtNull.AutoSize = true;
            this.cbCompletedDtNull.Checked = true;
            this.cbCompletedDtNull.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cbCompletedDtNull.Enabled = false;
            this.cbCompletedDtNull.Location = new System.Drawing.Point(596, 116);
            this.cbCompletedDtNull.Name = "cbCompletedDtNull";
            this.cbCompletedDtNull.Size = new System.Drawing.Size(44, 17);
            this.cbCompletedDtNull.TabIndex = 200;
            this.cbCompletedDtNull.Text = "Null";
            this.cbCompletedDtNull.UseVisualStyleBackColor = true;
            this.cbCompletedDtNull.CheckedChanged += new System.EventHandler(this.cbCompletedDtNull_CheckedChanged);
            // 
            // TaskProcessingQueueAdmin
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1106, 532);
            this.Controls.Add(this.cmbTaskId);
            this.Controls.Add(this.cbWaitedDtNull);
            this.Controls.Add(this.cbStartedDtNull);
            this.Controls.Add(this.cbCompletedDtNull);
            this.Controls.Add(this.nudPriority);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnSave);
            this.Controls.Add(this.nudIntervalRequeueSec);
            this.Controls.Add(this.label14);
            this.Controls.Add(this.tbIntervalCount);
            this.Controls.Add(this.label13);
            this.Controls.Add(this.dtpStatusDateTime);
            this.Controls.Add(this.dtpCompletedDateTime);
            this.Controls.Add(this.dtpStartedDateTime);
            this.Controls.Add(this.dtpWaitForDateTime);
            this.Controls.Add(this.tbTaskRemarks);
            this.Controls.Add(this.label12);
            this.Controls.Add(this.tbTaskQueueCode);
            this.Controls.Add(this.tbWaitForConfig);
            this.Controls.Add(this.tbWaitForEngine);
            this.Controls.Add(this.tbTaskParams);
            this.Controls.Add(this.cbClearParam);
            this.Controls.Add(this.label11);
            this.Controls.Add(this.label10);
            this.Controls.Add(this.label9);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.cbWaitNoUsers);
            this.Controls.Add(this.dgvWaitForTasks);
            this.Controls.Add(this.cbWaitForTasks);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Name = "TaskProcessingQueueAdmin";
            this.Text = "TaskProcessingQueueAdmin";
            ((System.ComponentModel.ISupportInitialize)(this.dgvWaitForTasks)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudIntervalRequeueSec)).EndInit();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudPriority)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.CheckBox cbWaitForTasks;
        private System.Windows.Forms.DataGridView dgvWaitForTasks;
        private System.Windows.Forms.CheckBox cbWaitNoUsers;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.CheckBox cbClearParam;
        private System.Windows.Forms.TextBox tbTaskParams;
        private System.Windows.Forms.TextBox tbWaitForEngine;
        private System.Windows.Forms.TextBox tbWaitForConfig;
        private System.Windows.Forms.TextBox tbTaskQueueCode;
        private System.Windows.Forms.TextBox tbTaskRemarks;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.DateTimePicker dtpWaitForDateTime;
        private System.Windows.Forms.DateTimePicker dtpStartedDateTime;
        private System.Windows.Forms.DateTimePicker dtpCompletedDateTime;
        private System.Windows.Forms.DateTimePicker dtpStatusDateTime;
        private System.Windows.Forms.TextBox tbIntervalCount;
        private System.Windows.Forms.Label label13;
        private System.Windows.Forms.Label label14;
        private System.Windows.Forms.NumericUpDown nudIntervalRequeueSec;
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.RadioButton rbTPQNotQueued;
        private System.Windows.Forms.RadioButton rbTPQqueued;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.NumericUpDown nudPriority;
        private System.Windows.Forms.RadioButton rbTPQSucceeded;
        private System.Windows.Forms.RadioButton rbTPQFailed;
        private System.Windows.Forms.RadioButton rbTPQInProcess;
        private System.Windows.Forms.CheckBox cbStartedDtNull;
        private System.Windows.Forms.CheckBox cbWaitedDtNull;
        private System.Windows.Forms.ComboBox cmbTaskId;
        private System.Windows.Forms.CheckBox cbCompletedDtNull;
    }
}