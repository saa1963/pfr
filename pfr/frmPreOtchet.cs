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
    public partial class frmPreOtchet : Form
    {
        BindingSource bs = new BindingSource();
        List<KodNch> knch = new List<KodNch>()
        {
            new KodNch(){Kod = null, Name = "Не установлено"},
            new KodNch(){Kod = "З1", Name = "З1 - Зачисление на счет, указанный в списке выплат"},
            new KodNch(){Kod = "З2", Name = "З2 - Зачисление на вновь открытый счет"},
            new KodNch(){Kod = "Н31", Name = "Н31 - Незачисление на счет - отсутствие номера счета"},
            new KodNch(){Kod = "Н32", Name = "Н32 - Незачисление на счет - расхождения в ФИО"},
            new KodNch(){Kod = "Н33", Name = "Н33 - Незачисление на счет - счет закрыт"},
            new KodNch(){Kod = "Н37", Name = "Н37 - Незачисление на счет - отметка о смерти"}
        };
        public frmPreOtchet(List<TrnSet> lst)
        {
            InitializeComponent();
            dgv.AutoGenerateColumns = false;
            var cl = (DataGridViewComboBoxColumn)dgv.Columns["KodZachisl"];
            cl.DataSource = knch;
            cl.DisplayMember = "Name";
            cl.ValueMember = "Kod";
            bs.DataSource = lst;
            dgv.DataSource = bs;
            Obrabotano = 0;
            dgv.CellValueChanged += dgv_CellValueChanged;
        }

        void dgv_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex == 4)
            {
                var o = (TrnSet)bs.Current;
                if (o.KodZachisl == null)
                {
                    o.DFakt = null;
                }
                else if (o.KodZachisl == "З1" || o.KodZachisl == "З2")
                {
                    o.DFakt = o.DateReg.Date;
                }
                else
                {
                    o.DFakt = null;
                }
            }
        }

        private void frmPreOtchet_FormClosing(object sender, FormClosingEventArgs e)
        {
            int obr = 0;
            this.Validate();
            foreach (var o in (List<TrnSet>)bs.DataSource)
            {
                if (o.KodZachisl == null && o.DFakt != null)
                {
                    e.Cancel = true;
                    break;
                }
                else if ((o.KodZachisl == "З1" || o.KodZachisl == "З2") && o.DFakt == null)
                {
                    e.Cancel = true;
                    break;
                }
                else if ((o.KodZachisl != "З1" && o.KodZachisl != "З2" && o.KodZachisl != null) && o.DFakt != null)
                {
                    e.Cancel = true;
                    break;
                }
                else
                {
                    e.Cancel = false;
                }
            }
            if (e.Cancel)
            {
                MessageBox.Show("Неверно введены данные");
                return;
            }
            foreach (var o in (List<TrnSet>)bs.DataSource)
            {
                if (o.KodZachisl != null) obr++;
            }
            Obrabotano = obr;
        }

        public int Obrabotano { get; set; }

        private void mnuCancelAll_Click(object sender, EventArgs e)
        {
            foreach (var o in (List<TrnSet>)bs.DataSource)
            {
                o.KodZachisl = null;
                o.DFakt = null;
            }
            bs.ResetBindings(false);
        }

        private void mnu31All_Click(object sender, EventArgs e)
        {
            foreach (var o in (List<TrnSet>)bs.DataSource)
            {
                o.KodZachisl = "З1";
                o.DFakt = o.DateReg.Date;
            }
            bs.ResetBindings(false);
        }
    }

    class KodNch
    {
        public string Kod { get; set; }
        public string Name { get; set; }
    }
}
