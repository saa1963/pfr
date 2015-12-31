using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace pfr
{
    public partial class frmChangePassword : Form
    {
        public frmChangePassword()
        {
            InitializeComponent();
        }

        public string OldPassword
        {
            get { return tbOld.Text; }
            set { tbOld.Text = value; }
        }

        public string NewPassword1
        {
            get { return tbNew1.Text; }
            set { tbNew1.Text = value; }
        }

        public string NewPassword2
        {
            get { return tbNew2.Text; }
            set { tbNew2.Text = value; }
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            if (NewPassword1 != NewPassword2)
            {
                MessageBox.Show("Новый пароль введен неправильно.");
                ActiveControl = tbNew1;
                return;
            }
            DialogResult = System.Windows.Forms.DialogResult.OK;
            Close();
        }
    }
}
