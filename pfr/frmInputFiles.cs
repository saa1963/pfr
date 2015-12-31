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
    public partial class frmInputFiles : Form
    {
        BindingSource bs1 = new BindingSource();
        BindingSource bs2 = new BindingSource();
        pfrEntities1 ctx = new pfrEntities1(Utils.Current.cn);
        
        public frmInputFiles()
        {
            InitializeComponent();
            dgv1.AutoGenerateColumns = false;
            dgv2.AutoGenerateColumns = false;
            bs1.PositionChanged += bs1_PositionChanged;

            MonthPeriod t2 = MonthPeriod.Current.AddYear(-5);
            for (var t1 = MonthPeriod.Current; t1.CompareTo(t2) > 0; t1 = t1.AddMonth(-1) )
                tbPeriod.Items.Add(t1);
            tbPeriod.SelectedIndex = 0;

            RefreshData1();
        }

        void bs1_PositionChanged(object sender, EventArgs e)
        {
            RefreshData2();
        }

        private void RefreshData2()
        {
            if (bs1.Current == null)
            {
                bs2.DataSource = null;
                dgv2.DataSource = bs2;
            }
            else
            {
                OpisSet o = (OpisSet)bs1.Current;
                Status[] st = new Status[3];
                st[0] = new Status();
                st[0].Name = "Подтверждение о прочтении массива";
                st[0].YesNo = (o.FileName1 != null);
                st[1] = new Status();
                st[1].Name = "Отчет о зачислении/незачислении";
                st[1].YesNo = (o.FileName2 != null);
                st[2] = new Status();
                st[2].Name = "Подтверждение о прочтении отчета";
                st[2].YesNo = (o.FileName3 != null);
                bs2.DataSource = null;
                bs2.DataSource = st;
                dgv2.DataSource = bs2;
            }
        }

        private void RefreshData1()
        {
            bs1.DataSource = null;
            bs1.DataSource = ctx.OpisSet.Where(s => s.DateReg >= ((MonthPeriod)tbPeriod.SelectedItem).Dt1 && 
                s.DateReg <= ((MonthPeriod)tbPeriod.SelectedItem).Dt2).OrderByDescending(s => s.DateReg).ToList();
            dgv1.DataSource = bs1;
        }

        private void mnuVx_Click(object sender, EventArgs e)
        {
            if (!Utils.Current.СоздатьПапкиДляВводаВывода())
            {
                return;
            }
            var msg = new clsProcessingInput().DoIt();
            RefreshData1();
            MessageBox.Show(msg);
        }

        private void mnuOtchet_Click(object sender, EventArgs e)
        {
            if (bs1.Current == null) return;
            OpisSet o = (OpisSet)bs1.Current;
            if (o.Kol != o.KolObrab)
            {
                MessageBox.Show(String.Format("Осталось {0} необработанных поручений. Нельзя сформировать отчет.", o.Kol - o.KolObrab));
                return;
            }
            if (o.FileName2 != null)
            {
                if (MessageBox.Show("Для данного массива отчет уже формировался. Вы хотите сформировать его заново?",
                    "", MessageBoxButtons.YesNo) 
                    != System.Windows.Forms.DialogResult.Yes) return;
            }
            new clsProcessing().Otchet0(o);
            ctx.SaveChanges();
            RefreshData2();
            MessageBox.Show("Отчет сформирован");
        }

        private void mnuCheck_Click(object sender, EventArgs e)
        {
            int totkol = 0, obr = 0;
            DateTime dfakt = DateTime.MinValue;
            if (bs1.Current == null) return;
            OpisSet o = (OpisSet)bs1.Current;
            foreach (var spis in o.SpisSet)
            {
                var or = new OracleBd();
                foreach (var trn in spis.TrnSet)
                {
                    if (trn.KodZachisl == null)
                    {
                        if (or.ExistTransaction(trn.Acc, trn.Sm, trn.DateReg.Date, trn.Id, ref dfakt))
                        {
                            trn.DFakt = dfakt;
                            trn.KodZachisl = "31";
                            obr++;
                        }
                    }
                    else
                    {
                        obr++;
                    }
                    totkol++;
                }
            }
            o.KolObrab = obr;
            ctx.SaveChanges();
            MessageBox.Show(String.Format("Осталось {0} необработанных поручений.", totkol - obr));
        }

        private void mnuManulCheck_Click(object sender, EventArgs e)
        {
            if (bs1.Current == null) return;
            OpisSet o = (OpisSet)bs1.Current;
            var lst = new List<TrnSet>();
            foreach (var spis in o.SpisSet)
            {
                foreach (var trn in spis.TrnSet)
                {
                    lst.Add(trn);
                }
            }
            var f = new frmPreOtchet(lst);
            f.ShowDialog();
            o.KolObrab = f.Obrabotano;
            bs1.ResetCurrentItem();
            ctx.SaveChanges();
        }

        private void frmInputFiles_FormClosed(object sender, FormClosedEventArgs e)
        {
            ctx.Dispose();
        }

        private void tbPeriod_SelectedIndexChanged(object sender, EventArgs e)
        {
            RefreshData1();
        }
    }

    class Status
    {
        public string Name { get; set; }
        public bool YesNo { get; set; }
    }

    class MonthPeriod: IComparable<MonthPeriod>
    {
        static string[] text = { "январь", "февраль", "март", "апрель", 
                                   "май", "июнь", "июль", "август", "сентябрь", 
                                   "октябрь", "ноябрь", "декабрь" };
        public MonthPeriod(int year, int month)
        {
            Year = year;
            Month = month;
        }
        public DateTime Dt1
        {
            get { return new DateTime(Year, Month, 1); }
        }
        public DateTime Dt2
        {
            get { return Dt1.AddMonths(1).AddDays(-1); }
        }
        public int Month { get; set; }
        public int Year { get; set; }
        static public MonthPeriod Current
        {
            get
            {
                return new MonthPeriod(DateTime.Now.Year, DateTime.Now.Month);
            }
        }
        public int CompareTo(MonthPeriod other)
        {
            if (Dt1 < other.Dt1) return -1;
            else if (Dt1 == other.Dt1) return 0;
            else return 1;
        }
        public MonthPeriod AddYear(int n)
        {
            DateTime t = Dt1.AddYears(n);
            return new MonthPeriod(t.Year, t.Month);
        }
        public MonthPeriod AddMonth(int n)
        {
            DateTime t = Dt1.AddMonths(n);
            return new MonthPeriod(t.Year, t.Month);
        }
        public override string ToString()
        {
            return text[Month - 1] + " " + Year.ToString();
        }
    }
}
