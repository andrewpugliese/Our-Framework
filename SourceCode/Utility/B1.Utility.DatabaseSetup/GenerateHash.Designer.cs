namespace B1.Utility.DatabaseSetup
{
    partial class GenerateHash
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
            this.lblSignonResults = new System.Windows.Forms.Label();
            this.tbPwd = new System.Windows.Forms.TextBox();
            this.lblOldPwd = new System.Windows.Forms.Label();
            this.btnPwdOk = new System.Windows.Forms.Button();
            this.tbSalt = new System.Windows.Forms.TextBox();
            this.lblConfirmPwd = new System.Windows.Forms.Label();
            this.tbPwdHash = new System.Windows.Forms.TextBox();
            this.label22 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // lblSignonResults
            // 
            this.lblSignonResults.AutoSize = true;
            this.lblSignonResults.Location = new System.Drawing.Point(19, 261);
            this.lblSignonResults.Name = "lblSignonResults";
            this.lblSignonResults.Size = new System.Drawing.Size(0, 13);
            this.lblSignonResults.TabIndex = 196;
            // 
            // tbPwd
            // 
            this.tbPwd.Location = new System.Drawing.Point(22, 49);
            this.tbPwd.Name = "tbPwd";
            this.tbPwd.ReadOnly = true;
            this.tbPwd.Size = new System.Drawing.Size(184, 20);
            this.tbPwd.TabIndex = 195;
            // 
            // lblOldPwd
            // 
            this.lblOldPwd.AutoSize = true;
            this.lblOldPwd.Location = new System.Drawing.Point(19, 33);
            this.lblOldPwd.Name = "lblOldPwd";
            this.lblOldPwd.Size = new System.Drawing.Size(53, 13);
            this.lblOldPwd.TabIndex = 194;
            this.lblOldPwd.Text = "Password";
            // 
            // btnPwdOk
            // 
            this.btnPwdOk.Location = new System.Drawing.Point(342, 212);
            this.btnPwdOk.Name = "btnPwdOk";
            this.btnPwdOk.Size = new System.Drawing.Size(75, 23);
            this.btnPwdOk.TabIndex = 192;
            this.btnPwdOk.Text = "Ok";
            this.btnPwdOk.UseVisualStyleBackColor = true;
            this.btnPwdOk.Click += new System.EventHandler(this.btnPwdOk_Click);
            // 
            // tbSalt
            // 
            this.tbSalt.Location = new System.Drawing.Point(22, 160);
            this.tbSalt.Name = "tbSalt";
            this.tbSalt.ReadOnly = true;
            this.tbSalt.Size = new System.Drawing.Size(741, 20);
            this.tbSalt.TabIndex = 191;
            // 
            // lblConfirmPwd
            // 
            this.lblConfirmPwd.AutoSize = true;
            this.lblConfirmPwd.Location = new System.Drawing.Point(19, 144);
            this.lblConfirmPwd.Name = "lblConfirmPwd";
            this.lblConfirmPwd.Size = new System.Drawing.Size(25, 13);
            this.lblConfirmPwd.TabIndex = 190;
            this.lblConfirmPwd.Text = "Salt";
            // 
            // tbPwdHash
            // 
            this.tbPwdHash.Location = new System.Drawing.Point(22, 105);
            this.tbPwdHash.Name = "tbPwdHash";
            this.tbPwdHash.ReadOnly = true;
            this.tbPwdHash.Size = new System.Drawing.Size(741, 20);
            this.tbPwdHash.TabIndex = 189;
            // 
            // label22
            // 
            this.label22.AutoSize = true;
            this.label22.Location = new System.Drawing.Point(19, 89);
            this.label22.Name = "label22";
            this.label22.Size = new System.Drawing.Size(81, 13);
            this.label22.TabIndex = 188;
            this.label22.Text = "Password Hash";
            // 
            // GenerateHash
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(775, 276);
            this.Controls.Add(this.lblSignonResults);
            this.Controls.Add(this.tbPwd);
            this.Controls.Add(this.lblOldPwd);
            this.Controls.Add(this.btnPwdOk);
            this.Controls.Add(this.tbSalt);
            this.Controls.Add(this.lblConfirmPwd);
            this.Controls.Add(this.tbPwdHash);
            this.Controls.Add(this.label22);
            this.Name = "GenerateHash";
            this.Text = "GenerateHash";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lblSignonResults;
        private System.Windows.Forms.TextBox tbPwd;
        private System.Windows.Forms.Label lblOldPwd;
        private System.Windows.Forms.Button btnPwdOk;
        private System.Windows.Forms.TextBox tbSalt;
        private System.Windows.Forms.Label lblConfirmPwd;
        private System.Windows.Forms.TextBox tbPwdHash;
        private System.Windows.Forms.Label label22;
    }
}