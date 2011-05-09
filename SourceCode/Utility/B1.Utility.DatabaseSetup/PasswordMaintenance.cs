using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace B1.Utility.DatabaseSetup
{
    /// <summary>
    /// Dialog form for setting a new password
    /// </summary>
    public partial class frmPasswordMaintenance : Form
    {
        string _oldPassword = null;
        string _newPassword = null;

        /// <summary>
        /// Constructor for password maintenance
        /// </summary>
        /// <param name="oldPassword">When not null, user is prompted to enter old password</param>
        public frmPasswordMaintenance(string oldPassword)
        {
            InitializeComponent();
            _oldPassword = oldPassword;

            if (string.IsNullOrEmpty(oldPassword))
            {
                lblOldPwd.Visible = tbOldPwd.Visible = false;
                lblSignonResults.Text = "Please enter (and confirm) a new password; then click Ok to change; Cancel to abort.";
            }
            else
            {
                lblOldPwd.Visible = tbOldPwd.Visible = true;
                lblSignonResults.Text = "Please enter old password, and a new password and confirm new password; then click Ok to change; Cancel to abort.";
            }
        }

        /// <summary>
        /// Returns the new password
        /// </summary>
        public string NewPassword
        {
            get { return _newPassword; }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = System.Windows.Forms.DialogResult.Cancel;
        }

        private void btnPwdOk_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(_oldPassword)
                && _oldPassword != tbOldPwd.Text)
            {
                MessageBox.Show("Old password did not match; please try again.");
                tbOldPwd.Text = tbNewPwd.Text = tbConfirmPwd.Text = null;
                tbOldPwd.Focus();
                return;
            }
            if (tbNewPwd.Text != tbConfirmPwd.Text)
            {
                MessageBox.Show("Password and confirm password did not match; please try again.");
                tbOldPwd.Text = tbNewPwd.Text = tbConfirmPwd.Text = null;
                if (!string.IsNullOrEmpty(_oldPassword))
                    tbOldPwd.Focus();
                else tbNewPwd.Focus();
                return;
            }
            _newPassword = tbNewPwd.Text;
            this.DialogResult = System.Windows.Forms.DialogResult.OK;
        }
    }
}
