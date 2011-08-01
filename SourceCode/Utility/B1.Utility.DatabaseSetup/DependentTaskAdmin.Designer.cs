namespace B1.Utility.DatabaseSetup
{
    partial class DependentTaskAdmin
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
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnSave = new System.Windows.Forms.Button();
            this.cmbWaitTasks = new System.Windows.Forms.ComboBox();
            this.tbTaskQueueCode = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.lblWaitTaskQueueCode = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.cmbWaitTaskCompCodes = new System.Windows.Forms.ComboBox();
            this.SuspendLayout();
            // 
            // btnCancel
            // 
            this.btnCancel.Location = new System.Drawing.Point(368, 218);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 23);
            this.btnCancel.TabIndex = 35;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // btnSave
            // 
            this.btnSave.Location = new System.Drawing.Point(287, 218);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(75, 23);
            this.btnSave.TabIndex = 34;
            this.btnSave.Text = "Save";
            this.btnSave.UseVisualStyleBackColor = true;
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            // 
            // cmbWaitTasks
            // 
            this.cmbWaitTasks.FormattingEnabled = true;
            this.cmbWaitTasks.Location = new System.Drawing.Point(12, 132);
            this.cmbWaitTasks.Name = "cmbWaitTasks";
            this.cmbWaitTasks.Size = new System.Drawing.Size(350, 21);
            this.cmbWaitTasks.TabIndex = 207;
            this.cmbWaitTasks.DropDown += new System.EventHandler(this.cmbWaitTasks_DropDown);
            // 
            // tbTaskQueueCode
            // 
            this.tbTaskQueueCode.Location = new System.Drawing.Point(10, 57);
            this.tbTaskQueueCode.Name = "tbTaskQueueCode";
            this.tbTaskQueueCode.ReadOnly = true;
            this.tbTaskQueueCode.Size = new System.Drawing.Size(191, 20);
            this.tbTaskQueueCode.TabIndex = 206;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 30);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(88, 13);
            this.label1.TabIndex = 204;
            this.label1.Text = "TaskQueueCode";
            // 
            // lblWaitTaskQueueCode
            // 
            this.lblWaitTaskQueueCode.AutoSize = true;
            this.lblWaitTaskQueueCode.Location = new System.Drawing.Point(12, 116);
            this.lblWaitTaskQueueCode.Name = "lblWaitTaskQueueCode";
            this.lblWaitTaskQueueCode.Size = new System.Drawing.Size(53, 13);
            this.lblWaitTaskQueueCode.TabIndex = 208;
            this.lblWaitTaskQueueCode.Text = "WaitTask";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(386, 116);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(133, 13);
            this.label2.TabIndex = 210;
            this.label2.Text = "WaitTask CompletionCode";
            // 
            // cmbWaitTaskCompCodes
            // 
            this.cmbWaitTaskCompCodes.FormattingEnabled = true;
            this.cmbWaitTaskCompCodes.Location = new System.Drawing.Point(386, 132);
            this.cmbWaitTaskCompCodes.Name = "cmbWaitTaskCompCodes";
            this.cmbWaitTaskCompCodes.Size = new System.Drawing.Size(350, 21);
            this.cmbWaitTaskCompCodes.TabIndex = 209;
            this.cmbWaitTaskCompCodes.DropDown += new System.EventHandler(this.cmbWaitTaskCompCodes_DropDown);
            // 
            // DependentTaskAdmin
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(747, 260);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.cmbWaitTaskCompCodes);
            this.Controls.Add(this.lblWaitTaskQueueCode);
            this.Controls.Add(this.cmbWaitTasks);
            this.Controls.Add(this.tbTaskQueueCode);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnSave);
            this.Name = "DependentTaskAdmin";
            this.Text = "Dependent Task Admin";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.ComboBox cmbWaitTasks;
        private System.Windows.Forms.TextBox tbTaskQueueCode;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label lblWaitTaskQueueCode;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ComboBox cmbWaitTaskCompCodes;
    }
}