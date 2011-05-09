namespace B1.Utility.DatabaseSetup
{
    partial class frmPasswordMaintenance
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
            this.tbConfirmPwd = new System.Windows.Forms.TextBox();
            this.lblConfirmPwd = new System.Windows.Forms.Label();
            this.tbNewPwd = new System.Windows.Forms.TextBox();
            this.label22 = new System.Windows.Forms.Label();
            this.btnPwdOk = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.tbOldPwd = new System.Windows.Forms.TextBox();
            this.lblOldPwd = new System.Windows.Forms.Label();
            this.lblSignonResults = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // tbConfirmPwd
            // 
            this.tbConfirmPwd.Location = new System.Drawing.Point(108, 163);
            this.tbConfirmPwd.Name = "tbConfirmPwd";
            this.tbConfirmPwd.Size = new System.Drawing.Size(184, 20);
            this.tbConfirmPwd.TabIndex = 182;
            this.tbConfirmPwd.UseSystemPasswordChar = true;
            // 
            // lblConfirmPwd
            // 
            this.lblConfirmPwd.AutoSize = true;
            this.lblConfirmPwd.Location = new System.Drawing.Point(105, 147);
            this.lblConfirmPwd.Name = "lblConfirmPwd";
            this.lblConfirmPwd.Size = new System.Drawing.Size(91, 13);
            this.lblConfirmPwd.TabIndex = 181;
            this.lblConfirmPwd.Text = "Confirm Password";
            // 
            // tbNewPwd
            // 
            this.tbNewPwd.Location = new System.Drawing.Point(108, 108);
            this.tbNewPwd.Name = "tbNewPwd";
            this.tbNewPwd.Size = new System.Drawing.Size(184, 20);
            this.tbNewPwd.TabIndex = 180;
            this.tbNewPwd.UseSystemPasswordChar = true;
            // 
            // label22
            // 
            this.label22.AutoSize = true;
            this.label22.Location = new System.Drawing.Point(105, 92);
            this.label22.Name = "label22";
            this.label22.Size = new System.Drawing.Size(78, 13);
            this.label22.TabIndex = 179;
            this.label22.Text = "New Password";
            // 
            // btnPwdOk
            // 
            this.btnPwdOk.Location = new System.Drawing.Point(106, 215);
            this.btnPwdOk.Name = "btnPwdOk";
            this.btnPwdOk.Size = new System.Drawing.Size(75, 23);
            this.btnPwdOk.TabIndex = 183;
            this.btnPwdOk.Text = "Ok";
            this.btnPwdOk.UseVisualStyleBackColor = true;
            this.btnPwdOk.Click += new System.EventHandler(this.btnPwdOk_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.Location = new System.Drawing.Point(206, 215);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 23);
            this.btnCancel.TabIndex = 184;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // tbOldPwd
            // 
            this.tbOldPwd.Location = new System.Drawing.Point(108, 52);
            this.tbOldPwd.Name = "tbOldPwd";
            this.tbOldPwd.Size = new System.Drawing.Size(184, 20);
            this.tbOldPwd.TabIndex = 186;
            this.tbOldPwd.UseSystemPasswordChar = true;
            this.tbOldPwd.Visible = false;
            // 
            // lblOldPwd
            // 
            this.lblOldPwd.AutoSize = true;
            this.lblOldPwd.Location = new System.Drawing.Point(105, 36);
            this.lblOldPwd.Name = "lblOldPwd";
            this.lblOldPwd.Size = new System.Drawing.Size(72, 13);
            this.lblOldPwd.TabIndex = 185;
            this.lblOldPwd.Text = "Old Password";
            // 
            // lblSignonResults
            // 
            this.lblSignonResults.AutoSize = true;
            this.lblSignonResults.Location = new System.Drawing.Point(12, 258);
            this.lblSignonResults.Name = "lblSignonResults";
            this.lblSignonResults.Size = new System.Drawing.Size(163, 13);
            this.lblSignonResults.TabIndex = 187;
            this.lblSignonResults.Text = "Please enter your new password.";
            // 
            // frmPasswordMaintenance
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(430, 310);
            this.Controls.Add(this.lblSignonResults);
            this.Controls.Add(this.tbOldPwd);
            this.Controls.Add(this.lblOldPwd);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnPwdOk);
            this.Controls.Add(this.tbConfirmPwd);
            this.Controls.Add(this.lblConfirmPwd);
            this.Controls.Add(this.tbNewPwd);
            this.Controls.Add(this.label22);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "frmPasswordMaintenance";
            this.Text = "Password Maintenance";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox tbConfirmPwd;
        private System.Windows.Forms.Label lblConfirmPwd;
        private System.Windows.Forms.TextBox tbNewPwd;
        private System.Windows.Forms.Label label22;
        private System.Windows.Forms.Button btnPwdOk;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.TextBox tbOldPwd;
        private System.Windows.Forms.Label lblOldPwd;
        private System.Windows.Forms.Label lblSignonResults;
    }
}