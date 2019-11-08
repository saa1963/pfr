using pfr.Properties;
using System;
using System.Collections.Generic;
using System.Data.Entity.Core.EntityClient;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace pfr
{
    static class Program
    {
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            

            var Server = "192.168.20.221";
            var Password = "zxc";

            //var Server = "127.0.0.1";
            //var Password = "1";


            var Database = "pfr";
            var Login = "sa";
            
            

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

            var f = new frmLogin();
            var r = f.ShowDialog();
            if (f.DialogResult == DialogResult.OK)
            {
                var f1 = new frmMain();
                f1.Text = "ПФР " + Settings.Default.login + "@" + Settings.Default.database;
                Application.Run(f1);
            }
        }
    }
}
