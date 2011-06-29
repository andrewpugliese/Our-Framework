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
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.cbWaitForTasks = new System.Windows.Forms.CheckBox();
            this.dgvWaitForTasks = new System.Windows.Forms.DataGridView();
            this.checkBox1 = new System.Windows.Forms.CheckBox();
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
            this.tbTaskId = new System.Windows.Forms.TextBox();
            this.tbTaskRemarks = new System.Windows.Forms.TextBox();
            this.label12 = new System.Windows.Forms.Label();
            this.dtpWaitForDateTime = new System.Windows.Forms.DateTimePicker();
            this.dtpStartedDateTime = new System.Windows.Forms.DateTimePicker();
            this.dtpCompletedDateTime = new System.Windows.Forms.DateTimePicker();
            this.dtpStatusDateTime = new System.Windows.Forms.DateTimePicker();
            this.tbTPQStatusCode = new System.Windows.Forms.TextBox();
            this.tbTPQPriorityCode = new System.Windows.Forms.TextBox();
            this.tbIntervalCount = new System.Windows.Forms.TextBox();
            this.label13 = new System.Windows.Forms.Label();
            this.label14 = new System.Windows.Forms.Label();
            this.nudIntervalRequeueSec = new System.Windows.Forms.NumericUpDown();
            this.btnSave = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.dgvWaitForTasks)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudIntervalRequeueSec)).BeginInit();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(24, 30);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(88, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "TaskQueueCode";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(127, 30);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(40, 13);
            this.label2.TabIndex = 1;
            this.label2.Text = "TaskId";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(267, 29);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(62, 13);
            this.label3.TabIndex = 2;
            this.label3.Text = "StatusCode";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(363, 29);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(63, 13);
            this.label4.TabIndex = 3;
            this.label4.Text = "PriorityCode";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(456, 29);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(83, 13);
            this.label5.TabIndex = 4;
            this.label5.Text = "StatusDateTime";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(625, 29);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(90, 13);
            this.label6.TabIndex = 5;
            this.label6.Text = "WaitForDateTime";
            // 
            // cbWaitForTasks
            // 
            this.cbWaitForTasks.AutoSize = true;
            this.cbWaitForTasks.Location = new System.Drawing.Point(28, 344);
            this.cbWaitForTasks.Name = "cbWaitForTasks";
            this.cbWaitForTasks.Size = new System.Drawing.Size(127, 17);
            this.cbWaitForTasks.TabIndex = 6;
            this.cbWaitForTasks.Text = "Wait For Other Tasks";
            this.cbWaitForTasks.UseVisualStyleBackColor = true;
            // 
            // dgvWaitForTasks
            // 
            this.dgvWaitForTasks.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvWaitForTasks.Location = new System.Drawing.Point(27, 367);
            this.dgvWaitForTasks.Name = "dgvWaitForTasks";
            this.dgvWaitForTasks.Size = new System.Drawing.Size(812, 140);
            this.dgvWaitForTasks.TabIndex = 7;
            // 
            // checkBox1
            // 
            this.checkBox1.AutoSize = true;
            this.checkBox1.Location = new System.Drawing.Point(751, 30);
            this.checkBox1.Name = "checkBox1";
            this.checkBox1.Size = new System.Drawing.Size(113, 17);
            this.checkBox1.TabIndex = 8;
            this.checkBox1.Text = "Wait For No Users";
            this.checkBox1.UseVisualStyleBackColor = true;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(893, 31);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(92, 13);
            this.label7.TabIndex = 9;
            this.label7.Text = "Wait For EngineId";
            this.label7.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(994, 31);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(89, 13);
            this.label8.TabIndex = 10;
            this.label8.Text = "Wait For ConfigId";
            this.label8.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(24, 101);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(87, 13);
            this.label9.TabIndex = 11;
            this.label9.Text = "StartedDateTime";
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(226, 101);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(103, 13);
            this.label10.TabIndex = 12;
            this.label10.Text = "CompletedDateTime";
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(25, 179);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(84, 13);
            this.label11.TabIndex = 13;
            this.label11.Text = "TaskParameters";
            // 
            // cbClearParam
            // 
            this.cbClearParam.AutoSize = true;
            this.cbClearParam.Location = new System.Drawing.Point(130, 178);
            this.cbClearParam.Name = "cbClearParam";
            this.cbClearParam.Size = new System.Drawing.Size(122, 17);
            this.cbClearParam.TabIndex = 14;
            this.cbClearParam.Text = "Clear Params at End";
            this.cbClearParam.UseVisualStyleBackColor = true;
            // 
            // tbTaskParams
            // 
            this.tbTaskParams.Location = new System.Drawing.Point(28, 213);
            this.tbTaskParams.Name = "tbTaskParams";
            this.tbTaskParams.Size = new System.Drawing.Size(811, 20);
            this.tbTaskParams.TabIndex = 15;
            // 
            // tbWaitForEngine
            // 
            this.tbWaitForEngine.Location = new System.Drawing.Point(896, 64);
            this.tbWaitForEngine.Name = "tbWaitForEngine";
            this.tbWaitForEngine.Size = new System.Drawing.Size(89, 20);
            this.tbWaitForEngine.TabIndex = 16;
            // 
            // tbWaitForConfig
            // 
            this.tbWaitForConfig.Location = new System.Drawing.Point(997, 64);
            this.tbWaitForConfig.Name = "tbWaitForConfig";
            this.tbWaitForConfig.Size = new System.Drawing.Size(89, 20);
            this.tbWaitForConfig.TabIndex = 17;
            // 
            // tbTaskQueueCode
            // 
            this.tbTaskQueueCode.Location = new System.Drawing.Point(27, 59);
            this.tbTaskQueueCode.Name = "tbTaskQueueCode";
            this.tbTaskQueueCode.Size = new System.Drawing.Size(89, 20);
            this.tbTaskQueueCode.TabIndex = 18;
            // 
            // tbTaskId
            // 
            this.tbTaskId.Location = new System.Drawing.Point(130, 59);
            this.tbTaskId.Name = "tbTaskId";
            this.tbTaskId.Size = new System.Drawing.Size(122, 20);
            this.tbTaskId.TabIndex = 19;
            // 
            // tbTaskRemarks
            // 
            this.tbTaskRemarks.Location = new System.Drawing.Point(28, 295);
            this.tbTaskRemarks.Name = "tbTaskRemarks";
            this.tbTaskRemarks.Size = new System.Drawing.Size(811, 20);
            this.tbTaskRemarks.TabIndex = 21;
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Location = new System.Drawing.Point(25, 261);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(73, 13);
            this.label12.TabIndex = 20;
            this.label12.Text = "TaskRemarks";
            // 
            // dtpWaitForDateTime
            // 
            this.dtpWaitForDateTime.Location = new System.Drawing.Point(631, 60);
            this.dtpWaitForDateTime.Name = "dtpWaitForDateTime";
            this.dtpWaitForDateTime.Size = new System.Drawing.Size(143, 20);
            this.dtpWaitForDateTime.TabIndex = 22;
            // 
            // dtpStartedDateTime
            // 
            this.dtpStartedDateTime.Location = new System.Drawing.Point(27, 131);
            this.dtpStartedDateTime.Name = "dtpStartedDateTime";
            this.dtpStartedDateTime.Size = new System.Drawing.Size(143, 20);
            this.dtpStartedDateTime.TabIndex = 23;
            // 
            // dtpCompletedDateTime
            // 
            this.dtpCompletedDateTime.Location = new System.Drawing.Point(229, 131);
            this.dtpCompletedDateTime.Name = "dtpCompletedDateTime";
            this.dtpCompletedDateTime.Size = new System.Drawing.Size(143, 20);
            this.dtpCompletedDateTime.TabIndex = 24;
            // 
            // dtpStatusDateTime
            // 
            this.dtpStatusDateTime.Location = new System.Drawing.Point(459, 60);
            this.dtpStatusDateTime.Name = "dtpStatusDateTime";
            this.dtpStatusDateTime.Size = new System.Drawing.Size(143, 20);
            this.dtpStatusDateTime.TabIndex = 25;
            // 
            // tbTPQStatusCode
            // 
            this.tbTPQStatusCode.Location = new System.Drawing.Point(270, 59);
            this.tbTPQStatusCode.Name = "tbTPQStatusCode";
            this.tbTPQStatusCode.Size = new System.Drawing.Size(70, 20);
            this.tbTPQStatusCode.TabIndex = 26;
            // 
            // tbTPQPriorityCode
            // 
            this.tbTPQPriorityCode.Location = new System.Drawing.Point(366, 59);
            this.tbTPQPriorityCode.Name = "tbTPQPriorityCode";
            this.tbTPQPriorityCode.Size = new System.Drawing.Size(70, 20);
            this.tbTPQPriorityCode.TabIndex = 27;
            // 
            // tbIntervalCount
            // 
            this.tbIntervalCount.Location = new System.Drawing.Point(459, 131);
            this.tbIntervalCount.Name = "tbIntervalCount";
            this.tbIntervalCount.Size = new System.Drawing.Size(70, 20);
            this.tbIntervalCount.TabIndex = 29;
            // 
            // label13
            // 
            this.label13.AutoSize = true;
            this.label13.Location = new System.Drawing.Point(456, 101);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(70, 13);
            this.label13.TabIndex = 28;
            this.label13.Text = "IntervalCount";
            // 
            // label14
            // 
            this.label14.AutoSize = true;
            this.label14.Location = new System.Drawing.Point(575, 101);
            this.label14.Name = "label14";
            this.label14.Size = new System.Drawing.Size(128, 13);
            this.label14.TabIndex = 30;
            this.label14.Text = "IntervalRequeueSeconds";
            // 
            // nudIntervalRequeueSec
            // 
            this.nudIntervalRequeueSec.Location = new System.Drawing.Point(579, 129);
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
            // 
            // btnCancel
            // 
            this.btnCancel.Location = new System.Drawing.Point(969, 464);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 23);
            this.btnCancel.TabIndex = 33;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            // 
            // TaskProcessingQueueAdmin
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1106, 532);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnSave);
            this.Controls.Add(this.nudIntervalRequeueSec);
            this.Controls.Add(this.label14);
            this.Controls.Add(this.tbIntervalCount);
            this.Controls.Add(this.label13);
            this.Controls.Add(this.tbTPQPriorityCode);
            this.Controls.Add(this.tbTPQStatusCode);
            this.Controls.Add(this.dtpStatusDateTime);
            this.Controls.Add(this.dtpCompletedDateTime);
            this.Controls.Add(this.dtpStartedDateTime);
            this.Controls.Add(this.dtpWaitForDateTime);
            this.Controls.Add(this.tbTaskRemarks);
            this.Controls.Add(this.label12);
            this.Controls.Add(this.tbTaskId);
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
            this.Controls.Add(this.checkBox1);
            this.Controls.Add(this.dgvWaitForTasks);
            this.Controls.Add(this.cbWaitForTasks);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Name = "TaskProcessingQueueAdmin";
            this.Text = "TaskProcessingQueueAdmin";
            ((System.ComponentModel.ISupportInitialize)(this.dgvWaitForTasks)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudIntervalRequeueSec)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.CheckBox cbWaitForTasks;
        private System.Windows.Forms.DataGridView dgvWaitForTasks;
        private System.Windows.Forms.CheckBox checkBox1;
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
        private System.Windows.Forms.TextBox tbTaskId;
        private System.Windows.Forms.TextBox tbTaskRemarks;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.DateTimePicker dtpWaitForDateTime;
        private System.Windows.Forms.DateTimePicker dtpStartedDateTime;
        private System.Windows.Forms.DateTimePicker dtpCompletedDateTime;
        private System.Windows.Forms.DateTimePicker dtpStatusDateTime;
        private System.Windows.Forms.TextBox tbTPQStatusCode;
        private System.Windows.Forms.TextBox tbTPQPriorityCode;
        private System.Windows.Forms.TextBox tbIntervalCount;
        private System.Windows.Forms.Label label13;
        private System.Windows.Forms.Label label14;
        private System.Windows.Forms.NumericUpDown nudIntervalRequeueSec;
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.Button btnCancel;
    }
}