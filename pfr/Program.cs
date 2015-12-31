using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace pfr
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            var f = new frmLogin();
            var r = f.ShowDialog();
            if (f.DialogResult == DialogResult.OK)
            {
                Application.Run(new frmMain());
            }
        }
    }
}
