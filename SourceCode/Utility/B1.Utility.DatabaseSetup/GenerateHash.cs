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
    internal partial class GenerateHash : Form
    {
        internal GenerateHash(string password, string hash, string salt)
        {
            InitializeComponent();
            tbPwd.Text = password;
            tbPwdHash.Text = hash;
            tbSalt.Text = salt;
        }

        private void btnPwdOk_Click(object sender, EventArgs e)
        {
            this.DialogResult = System.Windows.Forms.DialogResult.OK;
        }
    }
}
