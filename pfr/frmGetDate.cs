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
    public partial class frmGetDate : Form
    {
        public frmGetDate(DateTime dt)
        {
            InitializeComponent();
            Dt = dt;
        }

        public DateTime Dt 
        {
            get { return tbDate.Value; }
            set { tbDate.Value = value; }
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            DialogResult = System.Windows.Forms.DialogResult.OK;
            Close();
        }
    }
}
