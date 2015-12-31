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

namespace pfr
{
    public partial class frmLogin : Form
    {
        public frmLogin()
        {
            InitializeComponent();
        }

        public string Server
        {
            get { return tbServer.Text; }
            set { tbServer.Text = value; }
        }

        public string Database
        {
            get { return tbDatabase.Text; }
            set { tbDatabase.Text = value; }
        }

        public string Login
        {
            get { return tbLogin.Text; }
            set { tbLogin.Text = value; }
        }

        public string Password
        {
            get { return tbPassword.Text; }
            set { tbPassword.Text = value; }
        }

        private void frmLogin_Load(object sender, EventArgs e)
        {
            Server = String.IsNullOrWhiteSpace(Settings.Default.server) ? "192.168.20.221" : Settings.Default.server;
            Database = String.IsNullOrWhiteSpace(Settings.Default.database) ? "pfr" : Settings.Default.database;
            Login = Settings.Default.login ?? "";
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            string cnString;
            if (String.IsNullOrWhiteSpace(Login))
            {
                MessageBox.Show("Не введено имя пользователя");
                ActiveControl = tbLogin;
                return;
            }

            cnString = "Data Source=" + Server +
                ";Initial Catalog=" + Database +
                ";Persist Security Info=True;User ID=" + Login +
                ";Password=" + Password;
            var cn = new SqlConnection(cnString);
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


            // Initialize the EntityConnectionStringBuilder.
            EntityConnectionStringBuilder entityBuilder =
                new EntityConnectionStringBuilder();

            //Set the provider name.
            entityBuilder.Provider = "System.Data.SqlClient";

            // Set the provider-specific connection string.
            entityBuilder.ProviderConnectionString = "Data Source=" + Server +
                ";Initial Catalog=" + Database +
                ";Persist Security Info=True;User ID=" + Login +
                ";Password=" + Password;

            // Set the Metadata location.
            entityBuilder.Metadata = @"res://*/Pfr.csdl|
                            res://*/Pfr.ssdl|
                            res://*/Pfr.msl";

            Utils.Current.cn = new EntityConnection(entityBuilder.ToString());

            Settings.Default.server = Server;
            Settings.Default.database = Database;
            Settings.Default.login = Login;
            Settings.Default.Save();

            DialogResult = System.Windows.Forms.DialogResult.OK;
            Close();
        }

        private void btnChangePassword_Click(object sender, EventArgs e)
        {
            if (String.IsNullOrWhiteSpace(Server))
            {
                MessageBox.Show("Не введен сервер.");
                ActiveControl = tbServer;
                return;
            }
            if (String.IsNullOrWhiteSpace(Database))
            {
                MessageBox.Show("Не введено название базы данных.");
                ActiveControl = tbDatabase;
                return;
            }
            if (String.IsNullOrWhiteSpace(Login))
            {
                MessageBox.Show("Не введено имя пользователя.");
                ActiveControl = tbLogin;
                return;
            }
            var f = new frmChangePassword();
            if (f.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                using (var cn1 = new SqlConnection("Data Source=" + Server +
                ";Initial Catalog=" + Database +
                ";Persist Security Info=True;User ID=" + Login +
                ";Password=" + f.OldPassword))
                {
                    try
                    {
                        cn1.Open();
                        SqlCommand cmd = new SqlCommand();
                        cmd.Connection = cn1;
                        cmd.CommandText =
                            "ALTER LOGIN " + Login + " WITH PASSWORD = '" + f.NewPassword1 +
                            "' OLD_PASSWORD = '" + f.OldPassword + "'";
                        cmd.ExecuteNonQuery();
                        MessageBox.Show("Пароль изменен");
                    }
                    catch (Exception exception)
                    {
                        MessageBox.Show("Ошибка доступа к базе данных.\r\n" + exception.Message);
                        return;
                    }
                }
            }
        }
    }
}
