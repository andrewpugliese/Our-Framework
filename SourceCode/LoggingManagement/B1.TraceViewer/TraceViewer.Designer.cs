namespace B1.TraceViewer
{
    partial class TraceViewer
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
            if(disposing && (components != null))
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
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.udTracePageSize = new System.Windows.Forms.NumericUpDown();
            this.btnPrevious = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.tbNumTraceMessages = new System.Windows.Forms.TextBox();
            this.btnNext = new System.Windows.Forms.Button();
            this.rtbTrace = new System.Windows.Forms.RichTextBox();
            this.btnTop = new System.Windows.Forms.Button();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.lvLog = new System.Windows.Forms.ListView();
            this.Time = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.Machine = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.Process = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.Message = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.label2 = new System.Windows.Forms.Label();
            this.btnFind = new System.Windows.Forms.Button();
            this.btnLast = new System.Windows.Forms.Button();
            this.tabControl1.SuspendLayout();
            this.tabPage1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.udTracePageSize)).BeginInit();
            this.tabPage2.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabControl1
            // 
            this.tabControl1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Controls.Add(this.tabPage2);
            this.tabControl1.Location = new System.Drawing.Point(12, 42);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(602, 488);
            this.tabControl1.TabIndex = 4;
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.btnLast);
            this.tabPage1.Controls.Add(this.btnFind);
            this.tabPage1.Controls.Add(this.label2);
            this.tabPage1.Controls.Add(this.udTracePageSize);
            this.tabPage1.Controls.Add(this.btnPrevious);
            this.tabPage1.Controls.Add(this.label1);
            this.tabPage1.Controls.Add(this.tbNumTraceMessages);
            this.tabPage1.Controls.Add(this.btnNext);
            this.tabPage1.Controls.Add(this.rtbTrace);
            this.tabPage1.Controls.Add(this.btnTop);
            this.tabPage1.Location = new System.Drawing.Point(4, 22);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(594, 462);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "Trace";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // udTracePageSize
            // 
            this.udTracePageSize.Location = new System.Drawing.Point(518, 7);
            this.udTracePageSize.Name = "udTracePageSize";
            this.udTracePageSize.Size = new System.Drawing.Size(41, 20);
            this.udTracePageSize.TabIndex = 13;
            this.udTracePageSize.ValueChanged += new System.EventHandler(this.udTracePageSize_ValueChanged);
            // 
            // btnPrevious
            // 
            this.btnPrevious.Enabled = false;
            this.btnPrevious.Location = new System.Drawing.Point(77, 6);
            this.btnPrevious.Name = "btnPrevious";
            this.btnPrevious.Size = new System.Drawing.Size(77, 23);
            this.btnPrevious.TabIndex = 10;
            this.btnPrevious.Text = "<< Previous";
            this.btnPrevious.UseVisualStyleBackColor = true;
            this.btnPrevious.Click += new System.EventHandler(this.btnPrevious_Click);
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(388, 442);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(95, 13);
            this.label1.TabIndex = 9;
            this.label1.Text = "Messages Traced:";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // tbNumTraceMessages
            // 
            this.tbNumTraceMessages.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.tbNumTraceMessages.Location = new System.Drawing.Point(487, 439);
            this.tbNumTraceMessages.Name = "tbNumTraceMessages";
            this.tbNumTraceMessages.ReadOnly = true;
            this.tbNumTraceMessages.Size = new System.Drawing.Size(100, 20);
            this.tbNumTraceMessages.TabIndex = 8;
            // 
            // btnNext
            // 
            this.btnNext.Location = new System.Drawing.Point(160, 6);
            this.btnNext.Name = "btnNext";
            this.btnNext.Size = new System.Drawing.Size(77, 23);
            this.btnNext.TabIndex = 7;
            this.btnNext.Text = "Next >>";
            this.btnNext.UseVisualStyleBackColor = true;
            this.btnNext.Click += new System.EventHandler(this.btnNext_Click);
            // 
            // rtbTrace
            // 
            this.rtbTrace.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.rtbTrace.BackColor = System.Drawing.SystemColors.Window;
            this.rtbTrace.Location = new System.Drawing.Point(6, 44);
            this.rtbTrace.Name = "rtbTrace";
            this.rtbTrace.ReadOnly = true;
            this.rtbTrace.Size = new System.Drawing.Size(582, 388);
            this.rtbTrace.TabIndex = 6;
            this.rtbTrace.Text = "";
            this.rtbTrace.WordWrap = false;
            // 
            // btnTop
            // 
            this.btnTop.Enabled = false;
            this.btnTop.Location = new System.Drawing.Point(6, 6);
            this.btnTop.Name = "btnTop";
            this.btnTop.Size = new System.Drawing.Size(65, 23);
            this.btnTop.TabIndex = 5;
            this.btnTop.Text = "<| Top";
            this.btnTop.UseVisualStyleBackColor = true;
            this.btnTop.Click += new System.EventHandler(this.btnTop_Click);
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.lvLog);
            this.tabPage2.Location = new System.Drawing.Point(4, 22);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(594, 462);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "Log";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // lvLog
            // 
            this.lvLog.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.lvLog.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.Time,
            this.Machine,
            this.Process,
            this.Message});
            this.lvLog.Location = new System.Drawing.Point(6, 51);
            this.lvLog.Name = "lvLog";
            this.lvLog.Size = new System.Drawing.Size(521, 403);
            this.lvLog.TabIndex = 4;
            this.lvLog.UseCompatibleStateImageBehavior = false;
            this.lvLog.View = System.Windows.Forms.View.Details;
            // 
            // Time
            // 
            this.Time.Text = "Time";
            // 
            // Machine
            // 
            this.Machine.Text = "Machine";
            // 
            // Process
            // 
            this.Process.Text = "Process";
            // 
            // Message
            // 
            this.Message.Text = "Message";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(377, 9);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(135, 13);
            this.label2.TabIndex = 14;
            this.label2.Text = "Page Size (messages):";
            this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // btnFind
            // 
            this.btnFind.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnFind.Location = new System.Drawing.Point(6, 436);
            this.btnFind.Name = "btnFind";
            this.btnFind.Size = new System.Drawing.Size(77, 23);
            this.btnFind.TabIndex = 15;
            this.btnFind.Text = "Find";
            this.btnFind.UseVisualStyleBackColor = true;
            this.btnFind.Click += new System.EventHandler(this.btnFind_Click);
            // 
            // btnLast
            // 
            this.btnLast.Location = new System.Drawing.Point(243, 6);
            this.btnLast.Name = "btnLast";
            this.btnLast.Size = new System.Drawing.Size(77, 23);
            this.btnLast.TabIndex = 16;
            this.btnLast.Text = "Last |>";
            this.btnLast.UseVisualStyleBackColor = true;
            this.btnLast.Click += new System.EventHandler(this.btnLast_Click);
            // 
            // TraceViewer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(626, 542);
            this.Controls.Add(this.tabControl1);
            this.Name = "TraceViewer";
            this.Text = "Tracing And Logging";
            this.tabControl1.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.tabPage1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.udTracePageSize)).EndInit();
            this.tabPage2.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.Button btnNext;
        private System.Windows.Forms.RichTextBox rtbTrace;
        private System.Windows.Forms.Button btnTop;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.ListView lvLog;
        private System.Windows.Forms.ColumnHeader Time;
        private System.Windows.Forms.ColumnHeader Machine;
        private System.Windows.Forms.ColumnHeader Process;
        private System.Windows.Forms.ColumnHeader Message;
        private System.Windows.Forms.TextBox tbNumTraceMessages;
        private System.Windows.Forms.Button btnPrevious;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.NumericUpDown udTracePageSize;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button btnFind;
        private System.Windows.Forms.Button btnLast;

    }
}

