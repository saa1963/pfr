﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using pfr.Properties;
using System.IO;
using System.Xml.Serialization;
using System.Data.Entity.Infrastructure;
using Microsoft.Office.Interop.Excel;

namespace pfr
{
    public partial class frmMain : Form
    {
        BindingSource bs = new BindingSource();
        public frmMain()
        {
            InitializeComponent();
            dgv.AutoGenerateColumns = false;
            RefreshData();
            if (Settings.Default.login.ToUpper() != "SA")
            {
                mnuSofit.Enabled = false;
                mnuProtokol.Enabled = false;
                mnuCard.Enabled = false;
                mnuFiles.Enabled = false;
                mnuOptions.Enabled = false;
                mnuXXI.Enabled = false;
            }
            else
            {
                mnuOrders.Enabled = false;
            }
        }

        private void RefreshData()
        {
            DateTime dt1 = tbDate.Value.Date;
            DateTime dt2 = dt1.AddDays(1);
            using (var ctx = new pfrEntities1(Utils.Current.cn))
            {
                bs.DataSource = null;
                if (Settings.Default.login.ToUpper() != "SA")
                {
                    var doffice = (from user in ctx.UserSet where user.Login == Settings.Default.login select user.DOffice).ToArray()[0];
                    bs.DataSource = ctx.TrnSet.Include("SpisSet").
                        Where(s => s.DateReg >= dt1 && s.DateReg < dt2 && s.DOffice == doffice).ToList();
                }
                else
                {
                    bs.DataSource = ctx.TrnSet.Include("SpisSet").Where(s => s.DateReg >= dt1 && s.DateReg < dt2).ToList();
                }
                dgv.DataSource = bs;
            }
        }

        private void mnuOptions_Click(object sender, EventArgs e)
        {
            var f = new frmOptions();
            f.ShowDialog();
        }

        private void tbDate_ValueChanged(object sender, EventArgs e)
        {
            RefreshData();
        }

        private void mnuOdb_Click(object sender, EventArgs e)
        {
            Odb();
        }

        private void Odb()
        {
            int i = 0;
            decimal sm0 = 0;
            string s = "";
            var f = new frmGetDate(tbDate.Value.Date);
            if (f.ShowDialog() != System.Windows.Forms.DialogResult.OK) return;
            using (var ctx = new pfrEntities1(Utils.Current.cn))
            {
                //var doffice = (from user in ctx.UserSet where user.Login == Settings.Default.login select user.DOffice).ToArray()[0];
                foreach (DataGridViewRow r in dgv.SelectedRows)
                {
                    var o = (TrnSet)r.DataBoundItem;
                    int? itrnnum = o.SpisSet.ITrnNum;
                    var ar = (from ds in ctx.DoSet where ds.Kod == o.DOffice select ds).ToArray()[0];
                    var DebAcc = ar.Acc47422;
                    var OdbUser = ar.Login;
                    s += "# Doc Begin\r\n";
                    s += String.Format("Address={0}\r\n", "0");
                    s += String.Format("Deb_Acc={0}\r\n", DebAcc);
                    s += String.Format("Deb_Cur={0}\r\n", "RUR");
                    s += String.Format("Deb_Sum={0}\r\n", o.Sm.ToString("F2"));
                    s += String.Format("Cre_Acc={0}\r\n", o.Acc);
                    s += String.Format("Cre_Cur={0}\r\n", "RUR");
                    s += String.Format("Cre_Sum={0}\r\n", o.Sm.ToString("F2"));
                    s += String.Format("BO1={0}\r\n", "1");
                    s += String.Format("BO2={0}\r\n", "1");
                    s += String.Format("VO={0}\r\n", "17");
                    s += String.Format("Date_Reg={0}\r\n", f.Dt.ToString("dd.MM.yyyy"));
                    s += String.Format("Date_Doc={0}\r\n", f.Dt.ToString("dd.MM.yyyy"));
                    s += String.Format("Date_Val={0}\r\n", f.Dt.ToString("dd.MM.yyyy"));
                    s += String.Format("Date_Trn={0}\r\n", f.Dt.ToString("dd.MM.yyyy"));
                    s += String.Format("Doc_Num={0}\r\n", "2");
                    s += String.Format("User={0}\r\n", OdbUser);
                    s += String.Format("Purpose=Переч.пенсии из ПФР за {0} {1}г. [Id-{2}]\r\n", Utils.months[o.SpisSet.mec - 1], o.SpisSet.god, o.Id);

                    if (itrnnum.HasValue)
                    {
                        dynamic deptInfo = new OracleBd().GetDeptInfo(itrnnum.Value);
                        if (deptInfo != null)
                        {
                            // ведомственная информация
                            s += String.Format("STATUSSOSTAVIT={0}\r\n", deptInfo.CCREATSTATUS);
                            s += String.Format("KBK_F={0}\r\n", deptInfo.CBUDCODE);
                            s += String.Format("OKATO={0}\r\n", deptInfo.COKATOCODE);
                            s += String.Format("POKOSNPLAT={0}\r\n", deptInfo.CNALPURP);
                            s += String.Format("POKNALPERIOD={0}\r\n", deptInfo.CNALPERIOD);
                            s += String.Format("POKNUMDOC={0}\r\n", deptInfo.CNALDOCNUM);
                            s += String.Format("POKDATEDOC={0}\r\n", deptInfo.CNALDOCDATE);
                            s += String.Format("POKTYPEPLAT={0}\r\n", deptInfo.CNALTYPE);
                            s += String.Format("DOC_INDEX={0}\r\n", deptInfo.CDOCINDEX);
                        }
                    }

                    s += "# Doc End\r\n";
                    i++;
                    sm0 += o.Sm;
                }
            }
            if (i > 0)
            {
                SaveFileDialog sfd = new SaveFileDialog();
                sfd.Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*";
                sfd.InitialDirectory = String.IsNullOrWhiteSpace(Settings.Default.SaveFilePath)
                    ? "M:\\" : Settings.Default.SaveFilePath;
                sfd.FileName = "pfr.txt";
                if (sfd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    File.WriteAllText(sfd.FileName, s, Encoding.GetEncoding(1251));
                    Settings.Default.SaveFilePath = new FileInfo(sfd.FileName).DirectoryName;
                    Settings.Default.Save();
                    MessageBox.Show(String.Format("Выгружено {0} документов на сумму {1}.", i, sm0.ToString("F2")));
                }
            }
            else
            {
                MessageBox.Show("Не выбрано ни одного документа.");
            }
        }

        private void mnuProtokol_Click(object sender, EventArgs e)
        {
            int row = 1;
            var lst = (List<TrnSet>)bs.DataSource;
            var lst40817 = lst.Where(q => q.Acc.Substring(0, 5) == "40817").
                Select(q => new clsProtokol() { Doffice = q.DOffice, Acc = q.Acc, Sm = q.Sm });
            var lst423 = lst.Where(q => q.Acc.Substring(0, 3) == "423").GroupBy(q => q.DOffice).
                Select(grp => new clsProtokol{Doffice = grp.Key, Acc = grp.Max(q => "423"), Sm = grp.Sum(q => q.Sm)});
            List<clsProtokol> lst0 = lst40817.Union(lst423).OrderBy(q => new clsProtokol(){ Doffice=q.Doffice, Acc=q.Acc, Sm=q.Sm}).ToList();

            Microsoft.Office.Interop.Excel.Application app = new Microsoft.Office.Interop.Excel.Application();
            Microsoft.Office.Interop.Excel._Workbook wb = app.Workbooks.Add();
            Microsoft.Office.Interop.Excel._Worksheet wsh = wb.Worksheets["Лист1"];
            wsh.PageSetup.Zoom = false;
            wsh.PageSetup.FitToPagesWide = 1;
            wsh.PageSetup.FitToPagesTall = 500;
            Range r = wsh.get_Range("a1").EntireRow.EntireColumn;
            r.Font.Size = 10;
            wsh.Range["A:B"].NumberFormat = "@";
            wsh.Columns["A"].ColumnWidth = 18;
            wsh.Columns["B"].ColumnWidth = 26;
            wsh.Columns["C"].ColumnWidth = 16;

            wsh.Cells[row, 1] = "АКБ \"ТКПБ\" (ОАО) г.Тамбов";
            row++;
            wsh.Cells[row, 1] = "ПРОТОКОЛ ПОЛУЧЕНИЯ ПЕНСИОННЫХ ДОКУМЕНТОВ";
            row++;
            wsh.Cells[row, 1] = String.Format("Дата документов {0}", tbDate.Value.Date.ToString("dd.MM.yyyy"));
            row++;
            row++;
            wsh.Cells[row, 1] = "Подразделение";
            wsh.Cells[row, 2] = "Счет";
            wsh.Cells[row, 3] = "Сумма";
            row++;
            foreach (var o in lst0)
            {
                wsh.Cells[row, 1] = o.Doffice;
                wsh.Cells[row, 2] = o.Acc;
                wsh.Cells[row, 3] = o.Sm;
                row++;
            }
            wsh.Cells[row, 1] = "Итого документов на сумму";
            wsh.Cells[row, 3] = String.Format("=SUM(C6:C{0})", row - 1);

            var b = wsh.Range[wsh.Cells[5, 1], wsh.Cells[row - 1, 3]].Borders;
            b[XlBordersIndex.xlEdgeBottom].LineStyle = XlLineStyle.xlContinuous;
            b[XlBordersIndex.xlEdgeLeft].LineStyle = XlLineStyle.xlContinuous;
            b[XlBordersIndex.xlEdgeRight].LineStyle = XlLineStyle.xlContinuous;
            b[XlBordersIndex.xlEdgeTop].LineStyle = XlLineStyle.xlContinuous;
            b[XlBordersIndex.xlInsideHorizontal].LineStyle = XlLineStyle.xlContinuous;
            b[XlBordersIndex.xlInsideVertical].LineStyle = XlLineStyle.xlContinuous;

            app.Visible = true;
        }

        private void mnuSofit_Click(object sender, EventArgs e)
        {
            var lst = (List<TrnSet>)bs.DataSource;
            var lst40817 = lst.Where(q => q.Acc.Substring(0, 5) == "40817");
            string s = "";
            int i = 0;
            decimal sm0 = 0;
            foreach (var o in lst40817)
            {
                s += String.Format("{0},47422810200000000111,{1},{2},0\r\n", i + 1, o.Acc, o.Sm.ToString("F2").Replace(',', '.'));
                i++;
                sm0 += o.Sm;
            }
            if (i > 0)
            {
                SaveFileDialog sfd = new SaveFileDialog();
                sfd.Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*";
                sfd.InitialDirectory = String.IsNullOrWhiteSpace(Settings.Default.SofitPath)
                    ? "M:\\" : Settings.Default.SofitPath;
                sfd.FileName = "tr.txt";
                if (sfd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    File.WriteAllText(sfd.FileName, s, Encoding.GetEncoding(1251));
                    Settings.Default.SofitPath = new FileInfo(sfd.FileName).DirectoryName;
                    Settings.Default.Save();
                    MessageBox.Show(String.Format("Выгружено {0} документов на сумму {1}.", i, sm0.ToString("F2")));
                }
            }
            else
            {
                MessageBox.Show("Не выбрано ни одного документа.");
            }
        }

        private void mnuCard_Click(object sender, EventArgs e)
        {
            var lst = (List<TrnSet>)bs.DataSource;
            var lst40817 = lst.Where(q => q.Acc.Substring(0, 5) == "40817").OrderBy(q => q.DOffice).ThenBy(q => q.Acc).ToArray();

            Microsoft.Office.Interop.Excel.Application app = new Microsoft.Office.Interop.Excel.Application();
            Microsoft.Office.Interop.Excel._Workbook wb = app.Workbooks.Add();
            Microsoft.Office.Interop.Excel._Worksheet wsh = wb.Worksheets["Лист1"];
            wsh.PageSetup.Zoom = false;
            wsh.PageSetup.FitToPagesWide = 1;
            wsh.PageSetup.FitToPagesTall = 500;
            Range r = wsh.get_Range("a1").EntireRow.EntireColumn;
            r.Font.Size = 10;
            wsh.Range["A:B"].NumberFormat = "@";
            wsh.Columns["A"].ColumnWidth = 26;
            wsh.Columns["B"].ColumnWidth = 26;

            string doffice = "9999";
            int row = 1, row0 = 0; ;
            decimal sm0 = 0;
            for (int i = 0; i < lst40817.Length; i++)
            {
                if (lst40817[i].DOffice != doffice)
                {
                    if (i != 0)
                    {
                        // подвал предыдущего подотчета
                        var b = wsh.Range[wsh.Cells[row0, 1], wsh.Cells[row - 1, 3]].Borders;
                        b[XlBordersIndex.xlEdgeBottom].LineStyle = XlLineStyle.xlContinuous;
                        b[XlBordersIndex.xlEdgeLeft].LineStyle = XlLineStyle.xlContinuous;
                        b[XlBordersIndex.xlEdgeRight].LineStyle = XlLineStyle.xlContinuous;
                        b[XlBordersIndex.xlEdgeTop].LineStyle = XlLineStyle.xlContinuous;
                        b[XlBordersIndex.xlInsideHorizontal].LineStyle = XlLineStyle.xlContinuous;
                        b[XlBordersIndex.xlInsideVertical].LineStyle = XlLineStyle.xlContinuous;

                        wsh.Cells[row, 1] = Slepov.Russian.СуммаПрописью.Сумма.Пропись(sm0, Slepov.Russian.СуммаПрописью.Валюта.Рубли);
                        row++;
                        row++;
                        wsh.Cells[row, 1] = "Подпись:";
                    }
                    // шапка подотчета
                    doffice = lst40817[i].DOffice;
                    wsh.Cells[row, 1] = "АКБ \"ТКПБ\" (ОАО) г.Тамбов";
                    row++;
                    wsh.Cells[row, 1] = String.Format("Дата: {0}", tbDate.Value.Date.ToString("dd.MM.yyyy"));
                    row++;
                    wsh.Cells[row, 1] = "Пополнение карточных счетов";
                    row++;
                    wsh.Cells[row, 1] = String.Format("Переч.пенсии из ПФР за {0} {1}г.", 
                        Utils.months[lst40817[i].SpisSet.mec - 1], lst40817[i].SpisSet.god);
                    row++;
                    row++;
                    sm0 = 0;
                    row0 = row;
                }
                wsh.Cells[row, 1] = "47422810200000000111";
                wsh.Cells[row, 2] = lst40817[i].Acc;
                wsh.Cells[row, 3] = lst40817[i].Sm;
                row++;
                sm0 += lst40817[i].Sm;
            }
            if (row > 1)
            {
                // подвал предыдущего подотчета
                var b = wsh.Range[wsh.Cells[row0, 1], wsh.Cells[row - 1, 3]].Borders;
                b[XlBordersIndex.xlEdgeBottom].LineStyle = XlLineStyle.xlContinuous;
                b[XlBordersIndex.xlEdgeLeft].LineStyle = XlLineStyle.xlContinuous;
                b[XlBordersIndex.xlEdgeRight].LineStyle = XlLineStyle.xlContinuous;
                b[XlBordersIndex.xlEdgeTop].LineStyle = XlLineStyle.xlContinuous;
                b[XlBordersIndex.xlInsideHorizontal].LineStyle = XlLineStyle.xlContinuous;
                b[XlBordersIndex.xlInsideVertical].LineStyle = XlLineStyle.xlContinuous;

                wsh.Cells[row, 1] = Slepov.Russian.СуммаПрописью.Сумма.Пропись(sm0, Slepov.Russian.СуммаПрописью.Валюта.Рубли);
                row++;
                row++;
                wsh.Cells[row, 1] = "Подпись:";
            }
            app.Visible = true;
        }

        private void mnuFiles_Click(object sender, EventArgs e)
        {
            var f = new frmInputFiles();
            f.ShowDialog();
            RefreshData();
        }

        private void mnuOrders_Click(object sender, EventArgs e)
        {
            var lst = (List<TrnSet>)bs.DataSource;
            var lst423 = lst.Where(q => q.Acc.Substring(0, 3) == "423").OrderBy(q => q.DOffice).ThenBy(q => q.Fio).ToArray();
            var sm = lst423.Sum(s => s.Sm);
            string acc47422;

            Microsoft.Office.Interop.Excel.Application app = new Microsoft.Office.Interop.Excel.Application();
            Microsoft.Office.Interop.Excel._Workbook wb = app.Workbooks.Add();
            Microsoft.Office.Interop.Excel._Worksheet wsh = wb.Worksheets["Лист1"];
            using (var ctx = new pfrEntities1(Utils.Current.cn))
            {
                var doffice = (from user in ctx.UserSet where user.Login == Settings.Default.login select user.DOffice).ToArray()[0];
                var ar = (from ds in ctx.DoSet where ds.Kod == doffice select ds).ToArray()[0];
                acc47422 = ar.Acc47422;


                //wsh.PageSetup.Zoom = false;
                //wsh.PageSetup.FitToPagesWide = 1;
                //wsh.PageSetup.FitToPagesTall = 500;
                wsh.PageSetup.RightMargin = 0.8;
                Range r = wsh.get_Range("a1").EntireRow.EntireColumn;
                r.Font.Size = 11;
                //wsh.Range["A:B"].NumberFormat = "@";
                wsh.Columns["A"].ColumnWidth = 12;
                wsh.Columns["F"].ColumnWidth = 10;
                wsh.Columns["H"].ColumnWidth = 11;

                r = wsh.Range["A1"];
                r.Value = String.Format("БАНКОВСКИЙ ОРДЕР № {0}", Settings.Default.Norder);

                r = wsh.Range["F1"];
                r.HorizontalAlignment = XlHAlign.xlHAlignCenter;
                r.Value = "'" + tbDate.Value.ToString("dd.MM.yyyy");
                r.Borders[XlBordersIndex.xlEdgeBottom].LineStyle = XlLineStyle.xlContinuous;

                r = wsh.Range["I1"];
                r.HorizontalAlignment = XlHAlign.xlHAlignCenter;
                r.Value = "'0401067";
                r.Borders[XlBordersIndex.xlEdgeBottom].LineStyle = XlLineStyle.xlContinuous;
                r.Borders[XlBordersIndex.xlEdgeLeft].LineStyle = XlLineStyle.xlContinuous;
                r.Borders[XlBordersIndex.xlEdgeRight].LineStyle = XlLineStyle.xlContinuous;
                r.Borders[XlBordersIndex.xlEdgeTop].LineStyle = XlLineStyle.xlContinuous;

                r = wsh.Range["F2"];
                r.HorizontalAlignment = XlHAlign.xlHAlignCenter;
                r.Value = "Дата";

                r = wsh.Range["A4:A5"];
                r.Merge();
                r.Borders[XlBordersIndex.xlEdgeBottom].LineStyle = XlLineStyle.xlContinuous;
                r.Borders[XlBordersIndex.xlEdgeRight].LineStyle = XlLineStyle.xlContinuous;
                r.Borders[XlBordersIndex.xlEdgeTop].LineStyle = XlLineStyle.xlContinuous;

                r = wsh.Range["A4"];
                r.HorizontalAlignment = XlHAlign.xlHAlignLeft;
                r.VerticalAlignment = XlVAlign.xlVAlignTop;
                r.WrapText = true;
                r.Value = "Сумма прописью";

                r = wsh.Range["B4:G5"];
                r.Merge();
                r.Borders[XlBordersIndex.xlEdgeLeft].LineStyle = XlLineStyle.xlContinuous;
                r.Borders[XlBordersIndex.xlEdgeBottom].LineStyle = XlLineStyle.xlContinuous;
                r.Borders[XlBordersIndex.xlEdgeRight].LineStyle = XlLineStyle.xlContinuous;
                r.Borders[XlBordersIndex.xlEdgeTop].LineStyle = XlLineStyle.xlContinuous;

                r = wsh.Range["B4"];
                r.HorizontalAlignment = XlHAlign.xlHAlignLeft;
                r.VerticalAlignment = XlVAlign.xlVAlignTop;
                r.WrapText = true;
                r.Value = Slepov.Russian.СуммаПрописью.Сумма.
                    Пропись(sm, Slepov.Russian.СуммаПрописью.Валюта.Рубли, Slepov.Russian.СуммаПрописью.Заглавные.Первая);

                r = wsh.Range["H4:I5"];
                r.Borders[XlBordersIndex.xlEdgeLeft].LineStyle = XlLineStyle.xlContinuous;
                r.Borders[XlBordersIndex.xlEdgeBottom].LineStyle = XlLineStyle.xlContinuous;
                r.Borders[XlBordersIndex.xlInsideHorizontal].LineStyle = XlLineStyle.xlContinuous;
                r.Borders[XlBordersIndex.xlInsideVertical].LineStyle = XlLineStyle.xlContinuous;
                r.Borders[XlBordersIndex.xlEdgeTop].LineStyle = XlLineStyle.xlContinuous;

                r = wsh.Range["H4"];
                r.Value = "Вид.оп.";
                r = wsh.Range["H5"];
                r.Value = "Очер.плат.";
                r = wsh.Range["I4"];
                r.HorizontalAlignment = XlHAlign.xlHAlignCenter;
                r.Value = "'17";
                r = wsh.Range["I5"];
                r.HorizontalAlignment = XlHAlign.xlHAlignCenter;
                r.Value = "'5";

                r = wsh.Range["A6:D6"];
                r.Merge();
                r = wsh.Range["E6:G6"];
                r.Merge();
                r = wsh.Range["H6:I6"];
                r.Merge();
                r = wsh.Range["A6:I6"];
                r.HorizontalAlignment = XlHAlign.xlHAlignCenter;
                r.Borders[XlBordersIndex.xlEdgeBottom].LineStyle = XlLineStyle.xlContinuous;
                r.Borders[XlBordersIndex.xlInsideHorizontal].LineStyle = XlLineStyle.xlContinuous;
                r.Borders[XlBordersIndex.xlInsideVertical].LineStyle = XlLineStyle.xlContinuous;
                r.Borders[XlBordersIndex.xlEdgeTop].LineStyle = XlLineStyle.xlContinuous;
                r = wsh.Range["A6"];
                r.Value = "Плательщик";
                r = wsh.Range["E6"];
                r.Value = "Сч. №";
                r = wsh.Range["H6"];
                r.Value = "Сумма";

                r = wsh.Range["A7:D7"];
                r.Merge();
                r = wsh.Range["E7:G7"];
                r.Merge();
                r = wsh.Range["A8:D8"];
                r.Merge();
                r = wsh.Range["E8:G8"];
                r.Merge();
                r = wsh.Range["A7:G8"];
                r.HorizontalAlignment = XlHAlign.xlHAlignCenter;
                r.Borders[XlBordersIndex.xlEdgeBottom].LineStyle = XlLineStyle.xlContinuous;
                r.Borders[XlBordersIndex.xlInsideHorizontal].LineStyle = XlLineStyle.xlContinuous;
                r.Borders[XlBordersIndex.xlInsideVertical].LineStyle = XlLineStyle.xlContinuous;
                r.Borders[XlBordersIndex.xlEdgeTop].LineStyle = XlLineStyle.xlContinuous;
                r.Borders[XlBordersIndex.xlEdgeRight].LineStyle = XlLineStyle.xlContinuous;
                r = wsh.Range["A7"];
                r.HorizontalAlignment = XlHAlign.xlHAlignLeft;
                r.Value = "АО БАНК \"ТКПБ\"";
                r = wsh.Range["E7"];
                r.Value = "'" + acc47422;
                r = wsh.Range["A8"];
                r.Value = "Получатель";
                r = wsh.Range["E8"];
                r.Value = "Сч. №";
                r = wsh.Range["H7"];
                r.HorizontalAlignment = XlHAlign.xlHAlignRight;
                r.Borders[XlBordersIndex.xlEdgeRight].LineStyle = XlLineStyle.xlContinuous;
                r.Value = "'" + sm.ToString("##########0.00").Replace('.', '-').Replace(',', '-');
                r = wsh.Range["H8"];
                r.Borders[XlBordersIndex.xlEdgeRight].LineStyle = XlLineStyle.xlContinuous;

                int row = 9;
                foreach (var o in lst423)
                {
                    r = wsh.Range[wsh.Cells[row, 1], wsh.Cells[row, 4]];
                    r.Merge();
                    r.HorizontalAlignment = XlHAlign.xlHAlignLeft;
                    r.Borders[XlBordersIndex.xlEdgeRight].LineStyle = XlLineStyle.xlContinuous;
                    r.Value = o.Fio;

                    r = wsh.Range[wsh.Cells[row, 5], wsh.Cells[row, 7]];
                    r.Merge();
                    r.HorizontalAlignment = XlHAlign.xlHAlignCenter;
                    r.Borders[XlBordersIndex.xlEdgeRight].LineStyle = XlLineStyle.xlContinuous;
                    r.Value = "'" + o.Acc;

                    r = wsh.Cells[row, 8];
                    r.HorizontalAlignment = XlHAlign.xlHAlignRight;
                    r.Borders[XlBordersIndex.xlEdgeRight].LineStyle = XlLineStyle.xlContinuous;
                    r.Value = "'" + o.Sm.ToString("##########0.00").Replace('.', '-').Replace(',', '-');
                    row++;
                }

                r = wsh.Range[wsh.Cells[row, 1], wsh.Cells[row, 7]];
                r.Merge();
                r.HorizontalAlignment = XlHAlign.xlHAlignCenter;
                r.Borders[XlBordersIndex.xlEdgeRight].LineStyle = XlLineStyle.xlContinuous;
                r.Borders[XlBordersIndex.xlEdgeBottom].LineStyle = XlLineStyle.xlContinuous;
                r.Borders[XlBordersIndex.xlEdgeTop].LineStyle = XlLineStyle.xlContinuous;
                r.Value = "Назначение платежа";

                r = wsh.Range[wsh.Cells[row, 8], wsh.Cells[row, 9]];
                r.Borders[XlBordersIndex.xlEdgeBottom].LineStyle = XlLineStyle.xlContinuous;
                r.Borders[XlBordersIndex.xlEdgeTop].LineStyle = XlLineStyle.xlContinuous;
                row++;

                r = wsh.Range[wsh.Cells[row, 1], wsh.Cells[row, 7]];
                r.Merge();
                r.HorizontalAlignment = XlHAlign.xlHAlignLeft;
                r.Borders[XlBordersIndex.xlEdgeRight].LineStyle = XlLineStyle.xlContinuous;
                r.Borders[XlBordersIndex.xlEdgeBottom].LineStyle = XlLineStyle.xlContinuous;
                r.Borders[XlBordersIndex.xlEdgeTop].LineStyle = XlLineStyle.xlContinuous;
                var sps = ctx.SpisSet.Find(lst423[0].IdSpis);
                r.Value = String.Format("Переч.пенсии из ПФР за {0} {1}г.", Utils.months[sps.mec - 1], sps.god);

                r = wsh.Range[wsh.Cells[row + 1, 7], wsh.Cells[row + 2, 7]];
                r.Borders[XlBordersIndex.xlEdgeRight].LineStyle = XlLineStyle.xlContinuous;

                r = wsh.Range[wsh.Cells[row, 8], wsh.Cells[row, 9]];
                r.Merge();
                r.HorizontalAlignment = XlHAlign.xlHAlignCenter;
                r.Value = "Отметки банка";
                row += 2;

                r = wsh.Range[wsh.Cells[row, 8], wsh.Cells[row, 9]];
                r.Merge();
                r.HorizontalAlignment = XlHAlign.xlHAlignCenter;
                r.Value = "Подписи";
            }
            app.Visible = true;
        }

        private void testToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string doffice = null;
            if (new OracleBd().IsExistAcc("40702810000700000150", out doffice))
            {
                MessageBox.Show(doffice);
            }
        }
    }
}
