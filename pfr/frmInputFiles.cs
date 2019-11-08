using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Serialization;

namespace pfr
{
    public partial class frmInputFiles : Form
    {
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
        BindingSource bs1 = new BindingSource();
        BindingSource bs2 = new BindingSource();
        pfrEntities1 ctx = new pfrEntities1(Utils.Current.cn);
        //BindingList<OpisSet> bss = new BindingList<OpisSet>()
        
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
            var lst = ctx.OpisSet.Where(s => s.DateReg >= ((MonthPeriod)tbPeriod.SelectedItem).Dt1 && 
                s.DateReg <= ((MonthPeriod)tbPeriod.SelectedItem).Dt2).OrderByDescending(s => s.DateReg).ToList();
            bs1.DataSource = new BindingList<OpisSet>(lst);
            dgv1.DataSource = bs1;
        }

        private void AutoReceive(bool isSendtoXXI, DateTime dt)
        {
            string message;
            if (!Utils.Current.СоздатьПапкиДляВводаВывода())
            {
                return;
            }
            var savedTrn = new Dictionary<int, List<int>>();
            var msg = new clsProcessingInput().DoIt(savedTrn, dt);
            MessageBox.Show(msg);
            int countTotal = 0;
            int countTotalReg = 0;
            int countOpis = 0;
            int countOpisTotal = savedTrn.Count;
            //int kolmir = 0;
            foreach (var opisId in savedTrn)
            {
                int countDoc = 0;
                var opis = ctx.OpisSet.Find(opisId.Key);
                countTotal += opis.Kol;
                foreach (var trnId in opisId.Value)
                {
                    var o = ctx.TrnSet.Find(trnId);
                    //var checkMir = new OracleBd().CheckMir(o.Acc);
                    //var isMir = (checkMir != 2 && checkMir != -1234);
                    var ar = (from ds in ctx.DoSet where ds.Kod == o.DOffice select ds).ToArray()[0];
                    var DebAcc = ar.Acc47422;
                    var OdbUser = ar.Login;
                    if (new OracleBd().RegisterDoc(DebAcc: DebAcc, CredAcc: o.Acc, Sum: o.Sm, Dt: o.DateReg.Date, User: OdbUser,
                                Info: String.Format("Переч.пенсии из ПФР за {0} {1}г. [Id-{2}]", Utils.months[o.SpisSet.mec - 1],
                                o.SpisSet.god, o.Id), IdTrn: o.Id, isSendtoXXI: isSendtoXXI))
                    {
                        o.DFakt = dt.Date;
                        o.KodZachisl = "З1";
                        logger.Info(String.Format("Зарегистрирован платежный ордер. Дебет {0} Кредит {1} {2} на сумму {3}", DebAcc, o.Acc, o.Fio, o.Sm));
                        countDoc++;
                    }
                }
                opis.KolObrab1 = countDoc;
                countTotalReg += countDoc;
                ctx.SaveChanges();
                if (opis.Kol == opis.KolObrab)
                {
                    new clsProcessing().Otchet0(opis, dt);
                    countOpis++;
                    logger.Info(String.Format("По описи {0} сформирован отчет", opis.FileName));
                }
                else
                {
                    message = String.Format("По описи {0} отчет не сформирован, не все платежи проведены", opis.FileName);
                    logger.Warn(message);
                }
                //foreach (var spis in opis.SpisSet)
                //{
                //    SaveDeptInfo(spis);
                //}
            }
            ctx.SaveChanges();
            message = String.Format("Зарегистрировано в Инверсии {0} документов из {1}\r\nСформировано {2} из {3} отчетов",
                countTotalReg, countTotal, countOpis, countOpisTotal);
            logger.Info(message);
            MessageBox.Show(message);
            RefreshData1();
        }

        private void mnuVx_Click(object sender, EventArgs e)
        {
            AutoReceive(true, DateTime.Now);
            //AutoReceive(true, new DateTime(2019, 10, 22, 16, 30, 0));
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
            new clsProcessing().Otchet0(o, DateTime.Now);
            ctx.SaveChanges();
            RefreshData2();
            MessageBox.Show("Отчет сформирован");
        }

        private void mnuCheck_Click(object sender, EventArgs e)
        {
            int totkol = 0, obr = 0;
            DateTime dfakt = DateTime.MinValue;
            int itrnnum = 0;
            if (bs1.Current == null) return;
            OpisSet o = (OpisSet)bs1.Current;
            foreach (var spis in o.SpisSet)
            {
                var or = new OracleBd();
                foreach (var trn in spis.TrnSet)
                {
                    if (trn.KodZachisl == null)
                    {
                        if (or.ExistTransaction(trn.Acc, trn.Sm, trn.DateReg.Date, trn.Id, ref dfakt, ref itrnnum))
                        {
                            if (!or.AddVedInform(spis.ITrnNum.Value, itrnnum))
                            {
                                //MessageBox.Show($"Ошибка добавления ведомственной ин");
                            }
                            trn.DFakt = dfakt;
                            trn.KodZachisl = "З1";
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
            o.KolObrab1 = obr;
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

        private void mnuBindPlat_Click(object sender, EventArgs e)
        {
            if (bs1.Current == null) return;
            OpisSet o = (OpisSet)bs1.Current;
            foreach (var spis in o.SpisSet)
            {
                pfr.Xsd1.ФайлПФР Список = null;
                try
                {
                    ОбработатьСписокНаЗачисление(spis.Xml, ref Список);

                    var dtPlat = new DateTime(Int32.Parse(Список.ПачкаВходящихДокументов.ВХОДЯЩАЯ_ОПИСЬ.ДатаПлатежногоПоручения.Substring(6, 4)),
                                    Int32.Parse(Список.ПачкаВходящихДокументов.ВХОДЯЩАЯ_ОПИСЬ.ДатаПлатежногоПоручения.Substring(3, 2)),
                                    Int32.Parse(Список.ПачкаВходящихДокументов.ВХОДЯЩАЯ_ОПИСЬ.ДатаПлатежногоПоручения.Substring(0, 2)));
                    var numPlat = Int32.Parse(Список.ПачкаВходящихДокументов.ВХОДЯЩАЯ_ОПИСЬ.НомерПлатежногоПоручения);
                    var sumPlat = Список.ПачкаВходящихДокументов.ВХОДЯЩАЯ_ОПИСЬ.СуммаПоЧастиМассива;
                    int idPlat;
                    if ((idPlat = new OracleBd().ExistPlat(dtPlat, numPlat, sumPlat)) == 0)
                    {
                        throw new Exception(String.Format("Для файла списка {0} не найдена платежка № {1} с датой между {2} и {3} на сумму {4}. " +
                            "Списки по описи {5} не обработаны. Попробуйте повторно обработать их после проведения платежа в Инверсии XXI век.",
                            new object[] { new FileInfo(spis.FileName).Name, numPlat, dtPlat, dtPlat.AddDays(4), sumPlat, new FileInfo(spis.OpisSet.FileName).Name }));
                    }
                    spis.ITrnNum = idPlat;
                    logger.Info(String.Format("К файлу списка {0} привязано платежное поручение с ITRNNUM {1}",
                        new object[] { new FileInfo(spis.FileName).Name, spis.ITrnNum }));
                }
                catch (Exception e1)
                {
                    logger.Error(String.Format("Ошибка привязки платежки, файл {0}", spis.FileName));
                    logger.Error(e1.ToString());
                    MessageBox.Show("Ошибка привязки. " + Utils.Current.LogMessage);
                }
            }
            ctx.SaveChanges();
        }

        private void ОбработатьСписокНаЗачисление(string xml, ref pfr.Xsd1.ФайлПФР o)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(pfr.Xsd1.ФайлПФР));

            using (var reader = new StreamReader(new MemoryStream(Encoding.GetEncoding(1251).GetBytes(xml)), Encoding.GetEncoding(1251)))
            {
                try
                {
                    o = (pfr.Xsd1.ФайлПФР)serializer.Deserialize(reader);
                }
                catch (InvalidOperationException e)
                {
                    throw new Exception("Ошибка десериализации XML", e);
                }
            }
        }

        private void mnuDeptInfo_Click(object sender, EventArgs e)
        {
            string message;
            try
            {
                DateTime dfakt = DateTime.MinValue;
                int itrnnum = -1;
                if (bs1.Current == null) return;
                OpisSet o = (OpisSet)bs1.Current;
                foreach (var spis in o.SpisSet)
                {
                    if (spis.ITrnNum.HasValue)
                    {
                        var or = new OracleBd();
                        foreach (var trn in spis.TrnSet)
                        {
                            if (or.ExistTransaction(trn.Acc, trn.Sm, trn.DateReg.Date, trn.Id, ref dfakt, ref itrnnum))
                            {
                                logger.Info(String.Format("Копирование вед.информации для платежа дата: {0} фио: {1} счет: {2} сумма: {3}", 
                                    trn.DateReg.Date, trn.Fio, trn.Acc, trn.Sm));
                                or.AddVedInform(spis.ITrnNum.Value, itrnnum);
                            }
                            else
                            {
                                message = String.Format("Поступление на счет {0} {1} на сумму {2} не проведено в Инверсии. Обработка прервана.",
                                    new object[] { trn.Acc, trn.Fio, trn.Sm });
                                MessageBox.Show(message);
                                throw new SaaException(message);
                            }
                        }
                    }
                    else
                    {
                        message = String.Format("Список {0} не привязан к платежке из ПФР", spis.FileName);
                        MessageBox.Show(message);
                        throw new SaaException(message);
                    }
                    message = String.Format("Ведомственная информация для платежей из файла {0} записана.", spis.FileName);
                    logger.Info(message);
                    MessageBox.Show(message);
                }
            }
            catch(SaaException e1)
            {
                logger.Error(e1.ToString());
            }
            catch (Exception e1)
            {
                logger.Error(e1.ToString());
                MessageBox.Show("Ошибка копирования ведомственной информации. " + Utils.Current.LogMessage);
            }
        }

        private void SaveDeptInfo(SpisSet spis)
        {
            string message;
            DateTime dfakt = DateTime.MinValue;
            int itrnnum = -1;
            try
            {
                var or = new OracleBd();
                foreach (var trn in spis.TrnSet)
                {
                    if (or.ExistTransaction(trn.Acc, trn.Sm, trn.DateReg.Date, trn.Id, ref dfakt, ref itrnnum))
                    {
                        logger.Info(String.Format("Копирование вед.информации для платежа дата: {0} фио: {1} счет: {2} сумма: {3}",
                            trn.DateReg.Date, trn.Fio, trn.Acc, trn.Sm));
                        or.AddVedInform(spis.ITrnNum.Value, itrnnum);
                    }
                    else
                    {
                        message = String.Format("Поступление на счет {0} {1} на сумму {2} не проведено в Инверсии. Обработка прервана.",
                            new object[] { trn.Acc, trn.Fio, trn.Sm });
                        MessageBox.Show(message);
                        throw new SaaException(message);
                    }
                }
                message = String.Format("Ведомственная информация для платежей из файла {0} записана.", spis.FileName);
                logger.Info(message);
            }
            catch (SaaException e1)
            {
                logger.Error(e1.ToString());
            }
            catch (Exception e1)
            {
                logger.Error(e1.ToString());
                MessageBox.Show("Ошибка копирования ведомственной информации. " + Utils.Current.LogMessage);
            }
        }

        private void приемВходящихФайловИзПФРбезОтправкиВXXIToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var f = new frmGetDate(DateTime.Now);
            if (f.ShowDialog() == DialogResult.OK)
            {
                AutoReceive(false, f.Dt);
            }
        }

        private void удалитьМассивToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (bs1.Current == null) return;
            OpisSet o = (OpisSet)bs1.Current;
            ctx.OpisSet.Remove(o);
            ctx.SaveChanges();
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
