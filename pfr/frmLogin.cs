using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using pfr.Properties;
using System.Data.SqlClient;
using System.Data.Entity.Core.EntityClient;
using Oracle.ManagedDataAccess.Client;

namespace pfr
{
    public partial class frmLogin : Form
    {
        public frmLogin()
        {
            InitializeComponent();
        }

        public string User
        {
            get { return tbUser.Text; }
            set { tbUser.Text = value; }
        }

        public string Base
        {
            get { return tbBase.Text; }
            set { tbBase.Text = value; }
        }

        public string Password
        {
            get { return tbPassword.Text; }
            set { tbPassword.Text = value; }
        }

        private void frmLogin_Load(object sender, EventArgs e)
        {
            //Server = String.IsNullOrWhiteSpace(Settings.Default.server) ? "192.168.20.221" : Settings.Default.server;
            Base = String.IsNullOrWhiteSpace(Settings.Default.database) ? "odb" : Settings.Default.database;
            User = Settings.Default.login ?? "";
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            string cnString;
            if (String.IsNullOrWhiteSpace(User))
            {
                MessageBox.Show("Не введено имя пользователя");
                ActiveControl = tbUser;
                return;
            }

            if (String.IsNullOrWhiteSpace(Base))
            {
                MessageBox.Show("Не введено имя базы данных");
                ActiveControl = tbBase;
                return;
            }

            cnString = @"Data Source=" + Base + ";User ID=" + User + ";Password=" + Password;
            var cn = new OracleConnection(cnString);
            try
            {
                cn.Open();
            }
            catch (Exception exception)
            {
                MessageBox.Show("Ошибка доступа к базе данных.\r\n" + exception.Message);
                return;
            }
            cn.Close();

            Utils.Current.OraCn = @"Data Source=" + Base + ";User ID=" + User + ";Password=" + Password;
            Utils.Current.UserOffice = new OracleBd().UserOffice();

            Settings.Default.database = Base;
            Settings.Default.login = User;
            Settings.Default.Save();

            DialogResult = System.Windows.Forms.DialogResult.OK;
            Close();

        }
    }
}
