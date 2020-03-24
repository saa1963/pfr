using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace pfr
{
    public partial class frmGetAccount : Form
    {
        public frmGetAccount()
        {
            InitializeComponent();
        }

        public string Acc
        {
            get => tbAcc.Text;
        }
    }
}
