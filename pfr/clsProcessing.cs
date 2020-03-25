using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Serialization;
using pfr.Properties;
using System.Reflection;
using System.Data.SqlClient;
using System.Xml.XPath;

namespace pfr
{
    public class clsProcessing
    {
        pfrEntities1 ctx = new pfrEntities1(Utils.Current.cn);
        /// <summary>
        /// Десериализация списка на зачисление
        /// </summary>
        /// <param name="ИмяФайлаСписокНаЗачисление">Имя файла со списком</param>
        /// <param name="o">Возвращаемый объект</param>
        /// <returns>Успех или нет</returns>
        public bool ОбработатьСписокНаЗачисление(string ИмяФайлаСписокНаЗачисление, ref pfr.Xsd1.ФайлПФР o)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(pfr.Xsd1.ФайлПФР));

            using (var reader = new StreamReader(ИмяФайлаСписокНаЗачисление, Encoding.GetEncoding(1251)))
            {
                try
                {
                    o = (pfr.Xsd1.ФайлПФР)serializer.Deserialize(reader);
                }
                catch (InvalidOperationException)
                {
                    return false;
                }
            }
            return true;
        }

        public string СформироватьПоложительныйОтвет(pfr.Xsd.ФайлПФР Опись, List<pfr.Xsd1.ФайлПФР> Списки, DateTime dt)
        {
            int seq = Utils.Current.GetSequence("OUT");
            StringBuilder sb = new StringBuilder(Опись.ИмяФайла);
            sb.Remove(0, 3).Insert(0, "OUT");
            sb.Remove(60, 4).Insert(60, "POSD");
            sb.Insert(73, String.Format("-OUTNMB-{0}", seq.ToString(new String('0', 10))));
            string fname = Path.Combine(Settings.Default.PathInputOutput, clsConst.FolderToPFR, sb.ToString());
            using (XmlTextWriter wr = new XmlTextWriter(fname, Encoding.GetEncoding(1251)))
            {
                wr.Formatting = Formatting.Indented;
                wr.WriteStartDocument();
                {
                    wr.WriteStartElement("ФайлПФР");
                    {
                        wr.WriteElementString("ИмяФайла", sb.ToString());
                        wr.WriteStartElement("ЗаголовокФайла");
                        {
                            wr.WriteElementString("ВерсияФормата", "07.00");
                            wr.WriteElementString("ТипФайла", "ВНЕШНИЙ");
                            wr.WriteStartElement("ПрограммаПодготовкиДанных");
                            {
                                wr.WriteElementString("НазваниеПрограммы", "ОБМЕН_С_ПФР");
                                wr.WriteElementString("Версия", Version());
                            }
                            wr.WriteEndElement();
                            wr.WriteElementString("ИсточникДанных", "ДОСТАВЩИК");
                        }
                        wr.WriteEndElement();
                        wr.WriteStartElement("ПачкаИсходящихДокументов");
                        {
                            wr.WriteAttributeString("ДоставочнаяОрганизация", "БАНК");
                            wr.WriteStartElement("ИСХОДЯЩАЯ_ОПИСЬ");
                            {
                                wr.WriteStartElement("СоставительПачки");
                                {
                                    wr.WriteElementString("НаименованиеОрганизации", Utils.Current.BankName);
                                }
                                wr.WriteEndElement();
                                wr.WriteStartElement("СоставДокументов");
                                {
                                    wr.WriteElementString("Количество", "1");
                                    wr.WriteStartElement("НаличиеДокументов");
                                    {
                                        wr.WriteElementString("ТипДокумента", "ПОДТВЕРЖДЕНИЕ_О_ПРОЧТЕНИИ_СПИСКОВ_ПРИ_ЗАЧИСЛЕНИИ");
                                        wr.WriteElementString("Количество", "1");
                                    }
                                    wr.WriteEndElement();
                                }
                                wr.WriteEndElement();
                            }
                            wr.WriteEndElement();
                            wr.WriteStartElement("ТерриториальныйОрганПФР");
                            {
                                wr.WriteStartElement("НалоговыйНомер");
                                {
                                    wr.WriteElementString("ИНН", Опись.ПачкаВходящихДокументов.ВХОДЯЩАЯ_ОПИСЬ.ТерриториальныйОрганПФР.НалоговыйНомер.ИНН);
                                    wr.WriteElementString("КПП", Опись.ПачкаВходящихДокументов.ВХОДЯЩАЯ_ОПИСЬ.ТерриториальныйОрганПФР.НалоговыйНомер.КПП);
                                }
                                wr.WriteEndElement();
                            }
                            wr.WriteEndElement();
                            wr.WriteElementString("НомерБанка", Опись.ИмяФайла.Substring(69, 4));
                            wr.WriteStartElement("ОрганизацияСформировавшаяДокумент");
                            {
                                wr.WriteElementString("НаименованиеОрганизации", Utils.Current.BankName);
                            }
                            wr.WriteEndElement();
                            wr.WriteElementString("ТипМассиваПоручений", "ОСНОВНОЙ");
                            wr.WriteElementString("Месяц", Опись.ПачкаВходящихДокументов.ВХОДЯЩАЯ_ОПИСЬ.Месяц);
                            wr.WriteElementString("Год", Опись.ПачкаВходящихДокументов.ВХОДЯЩАЯ_ОПИСЬ.Год);
                            wr.WriteElementString("ИсходящийНомер", seq.ToString(new String('0', 10)));
                            wr.WriteElementString("ДатаФормирования", dt.ToString("dd.MM.yyyy"));
                            wr.WriteElementString("Должность", Settings.Default.Dolzh);
                            wr.WriteElementString("Руководитель", Settings.Default.Ruk);
                        }
                        wr.WriteEndElement();
                        wr.WriteStartElement("ПОДТВЕРЖДЕНИЕ_О_ПРОЧТЕНИИ_СПИСКОВ_ПРИ_ЗАЧИСЛЕНИИ");
                        {
                            wr.WriteElementString("Количество", (Списки.Count + 2).ToString());
                            wr.WriteStartElement("СведенияОмассивеПоручений");
                            {
                                wr.WriteElementString("ТипСтроки", "ДЕТАЛЬНАЯ");
                                wr.WriteElementString("ИмяФайла", Опись.ИмяФайла);
                                wr.WriteElementString("СистемныйНомерМассива", Опись.ПачкаВходящихДокументов.ВХОДЯЩАЯ_ОПИСЬ.СистемныйНомерМассива);
                                wr.WriteElementString("ПодтверждениеОпрочтении", "ПРОЧТЕНО");
                                wr.WriteElementString("РезультатПроверки", "ПРАВИЛЬНО");
                            }
                            wr.WriteEndElement();
                            foreach (var Список in Списки)
                            {
                                wr.WriteStartElement("СведенияОмассивеПоручений");
                                {
                                    wr.WriteElementString("ТипСтроки", "ДЕТАЛЬНАЯ");
                                    wr.WriteElementString("ИмяФайла", Список.ИмяФайла);
                                    wr.WriteElementString("СистемныйНомерМассива", Список.ПачкаВходящихДокументов.ВХОДЯЩАЯ_ОПИСЬ.СистемныйНомерМассива);
                                    wr.WriteElementString("ПодтверждениеОпрочтении", "ПРОЧТЕНО");
                                    wr.WriteElementString("РезультатПроверки", "ПРАВИЛЬНО");
                                }
                                wr.WriteEndElement();
                            }
                            wr.WriteStartElement("СведенияОмассивеПоручений");
                            {
                                wr.WriteElementString("ТипСтроки", "ИТОГО");
                                wr.WriteElementString("КоличествоПоступившихФайлов", (Списки.Count + 1).ToString());
                                wr.WriteElementString("СистемныйНомерМассива", Опись.ПачкаВходящихДокументов.ВХОДЯЩАЯ_ОПИСЬ.СистемныйНомерМассива);
                            }
                            wr.WriteEndElement();
                            wr.WriteElementString("Решение", "ПРИНЯТО");
                            wr.WriteElementString("ДатаВыдачиДокумента", dt.ToString("dd.MM.yyyy"));
                            wr.WriteElementString("ВремяФормирования", dt.ToString("HH:mm"));
                        }
                        wr.WriteEndElement();
                    }
                    wr.WriteEndElement();
                }
                wr.WriteEndDocument();
            }
            return fname;
        }

        internal string Rasxozhd(OpisSet o, DateTime dt, List<int> trnIds)
        {
            XmlNode vx;
            XPathNavigator nav;
            int seq = Utils.Current.GetSequence("OUT");
            var opisXml = new XmlDocument();
            opisXml.LoadXml(o.Xml);

            StringBuilder sb = new StringBuilder(o.FileName);
            sb.Remove(0, 3).Insert(0, "OUT");
            sb.Remove(60, 4).Insert(60, "SPRA");
            sb.Insert(73, String.Format("-OUTNMB-{0}", seq.ToString(new String('0', 10))));
            string fname = Path.Combine(Settings.Default.PathInputOutput, clsConst.FolderToPFR, sb.ToString());
            using (XmlTextWriter wr = new XmlTextWriter(fname, Encoding.GetEncoding(1251)))
            {
                wr.Formatting = Formatting.Indented;
                wr.WriteStartDocument();
                wr.WriteStartElement("ФайлПФР");
                {
                    wr.WriteElementString("ИмяФайла", sb.ToString());
                    wr.WriteStartElement("ЗаголовокФайла");
                    {
                        wr.WriteElementString("ВерсияФормата", "07.00");
                        wr.WriteElementString("ТипФайла", "ВНЕШНИЙ");
                        wr.WriteStartElement("ПрограммаПодготовкиДанных");
                        {
                            wr.WriteElementString("НазваниеПрограммы", "ОБМЕН_С_ПФР");
                            wr.WriteElementString("Версия", Version());
                        }
                        wr.WriteEndElement();
                        wr.WriteElementString("ИсточникДанных", "ДОСТАВЩИК");
                    }
                    wr.WriteEndElement();
                    wr.WriteStartElement("ПачкаИсходящихДокументов");
                    {
                        wr.WriteAttributeString("ДоставочнаяОрганизация", "БАНК");
                        vx = opisXml.SelectSingleNode("//ВХОДЯЩАЯ_ОПИСЬ");
                        nav = vx.CreateNavigator();
                        wr.WriteNode(nav, false);
                        wr.WriteStartElement("ИСХОДЯЩАЯ_ОПИСЬ");
                        {
                            wr.WriteStartElement("СоставительПачки");
                            {
                                wr.WriteElementString("НаименованиеОрганизации", Utils.Current.BankName);
                            }
                            wr.WriteEndElement();
                            wr.WriteStartElement("СоставДокументов");
                            {
                                wr.WriteElementString("Количество", "1");
                                wr.WriteStartElement("НаличиеДокументов");
                                {
                                    wr.WriteElementString("ТипДокумента", "СПИСОК_РАСХОЖДЕНИЙ");
                                    wr.WriteElementString("Количество", "1");
                                }
                                wr.WriteEndElement();
                            }
                            wr.WriteEndElement();
                            vx = opisXml.SelectSingleNode("//ТерриториальныйОрганПФР");
                            nav = vx.CreateNavigator();
                            wr.WriteNode(nav, false);
                            wr.WriteElementString("НомерБанка", o.FileName.Substring(69, 4));
                            wr.WriteStartElement("ОрганизацияСформировавшаяДокумент");
                            {
                                wr.WriteElementString("НаименованиеОрганизации", Utils.Current.BankName);
                            }
                            wr.WriteEndElement();
                            wr.WriteElementString("ТипМассиваПоручений", "ОСНОВНОЙ");
                            vx = opisXml.SelectSingleNode("//Месяц");
                            nav = vx.CreateNavigator();
                            wr.WriteNode(nav, false);
                            vx = opisXml.SelectSingleNode("//Год");
                            nav = vx.CreateNavigator();
                            wr.WriteNode(nav, false);
                            wr.WriteElementString("ИсходящийНомер", seq.ToString(new String('0', 10)));
                            wr.WriteElementString("ДатаФормирования", dt.ToString("dd.MM.yyyy"));
                            wr.WriteElementString("Должность", Settings.Default.Dolzh);
                            wr.WriteElementString("Руководитель", Settings.Default.Ruk);
                        }
                        wr.WriteEndElement();
                        wr.WriteStartElement("СПИСОК_РАСХОЖДЕНИЙ");
                        {
                            wr.WriteElementString("Количество", trnIds.Count.ToString());
                            wr.WriteElementString("ДатаВыдачиДокумента", dt.ToString("dd.MM.yyyy"));
                            foreach (var trnid in trnIds)
                            {
                                var trn = ctx.TrnSet.Find(trnid);
                                wr.WriteStartElement("Расхождения");
                                {
                                    wr.WriteElementString("НомерВмассиве", trn.NumInMassiv.ToString());
                                    wr.WriteElementString("НомерВыплатногоДела", trn.NumVplDelo);
                                    wr.WriteElementString("КодРайона", trn.KodRaiona);
                                    wr.WriteElementString("СтраховойНомер", trn.StraxNum);
                                    wr.WriteElementString("НомерСчетаПФР", trn.Acc);
                                    wr.WriteElementString("НомерСчетаБанк", trn.Acc1);
                                    wr.WriteElementString("КодРасхождения", trn.KodZachisl);
                                }
                                wr.WriteEndElement();
                            }
                        }
                        wr.WriteEndElement();
                    }
                    wr.WriteEndElement();
                }
                wr.WriteEndElement();
                wr.WriteEndDocument();
            }
            return fname;
        }

        private string Version()
        {
            // Get the version of the executing assembly (that is, this assembly).
            Assembly assem = Assembly.GetEntryAssembly();
            AssemblyName assemName = assem.GetName();
            Version ver = assemName.Version;
            return ver.ToString();
        }

        internal void Otchet0(OpisSet o, DateTime dt)
        {
            string fname;
            FileInfo fi;
            foreach (var spis in o.SpisSet)
            {
                fname = Otchet00(spis, dt);
                fi = new FileInfo(fname);
                File.Copy(fname,
                    Path.Combine(Settings.Default.PathInputOutput, clsConst.FolderToPFR, clsConst.FolderArchive, fi.Name),
                    true);
                spis.FileName1 = fi.Name;
                spis.Xml1 = File.ReadAllText(fname, Encoding.GetEncoding(1251));
            }
            fname = Otchet01(o, dt);
            fi = new FileInfo(fname);
            File.Copy(fname,
                                Path.Combine(Settings.Default.PathInputOutput, clsConst.FolderToPFR, clsConst.FolderArchive, fi.Name),
                                true);
            o.FileName2 = fi.Name;
            o.Xml2 = File.ReadAllText(fname, Encoding.GetEncoding(1251));
        }

        private string Otchet01(OpisSet o, DateTime dt)
        {
            XmlNode vx;
            XPathNavigator nav;
            int isx = Utils.Current.GetSequence("OUT");
            var opisXml = new XmlDocument();
            opisXml.LoadXml(o.Xml);

            StringBuilder sb = new StringBuilder(o.FileName.Trim());
            sb.Remove(0, 3).Insert(0, "OUT");
            sb.Remove(60, 4).Insert(60, "OPVF");
            sb.Insert(73, String.Format("-OUTNMB-{0}", isx.ToString(new String('0', 10))));
            string fname = Path.Combine(Settings.Default.PathInputOutput, clsConst.FolderToPFR, sb.ToString());
            using (XmlTextWriter wr = new XmlTextWriter(fname, Encoding.GetEncoding(1251)))
            {
                wr.Formatting = Formatting.Indented;
                wr.WriteStartDocument();
                {
                    wr.WriteStartElement("ФайлПФР");
                    {
                        wr.WriteElementString("ИмяФайла", sb.ToString());
                        wr.WriteStartElement("ЗаголовокФайла");
                        {
                            wr.WriteElementString("ВерсияФормата", "07.00");
                            wr.WriteElementString("ТипФайла", "ВНЕШНИЙ");
                            wr.WriteStartElement("ПрограммаПодготовкиДанных");
                            {
                                wr.WriteElementString("НазваниеПрограммы", "ОБМЕН_С_ПФР");
                                wr.WriteElementString("Версия", Version());
                            }
                            wr.WriteEndElement();
                            wr.WriteElementString("ИсточникДанных", "ДОСТАВЩИК");
                        }
                        wr.WriteEndElement();
                        wr.WriteStartElement("ПачкаИсходящихДокументов");
                        {
                            wr.WriteAttributeString("ДоставочнаяОрганизация", "БАНК");
                            vx = opisXml.SelectSingleNode("//ВХОДЯЩАЯ_ОПИСЬ");
                            nav = vx.CreateNavigator();
                            wr.WriteNode(nav, false);
                            wr.WriteStartElement("ИСХОДЯЩАЯ_ОПИСЬ");
                            {
                                wr.WriteStartElement("СоставительПачки");
                                {
                                    wr.WriteElementString("НаименованиеОрганизации", Utils.Current.BankName);
                                }
                                wr.WriteEndElement();
                                wr.WriteStartElement("СоставДокументов");
                                {
                                    wr.WriteElementString("Количество", "1");
                                    wr.WriteStartElement("НаличиеДокументов");
                                    {
                                        wr.WriteElementString("ТипДокумента", "ОПИСЬ_ВОЗВРАЩАЕМЫХ_ФАЙЛОВ_ПРИ_ЗАЧИСЛЕНИИ");
                                        wr.WriteElementString("Количество", "1");
                                    }
                                    wr.WriteEndElement();
                                }
                                wr.WriteEndElement();
                                vx = opisXml.SelectSingleNode("//ТерриториальныйОрганПФР");
                                nav = vx.CreateNavigator();
                                wr.WriteNode(nav, false);
                                wr.WriteElementString("НомерБанка", o.FileName.Substring(69, 4));
                                wr.WriteStartElement("ОрганизацияСформировавшаяДокумент");
                                {
                                    wr.WriteElementString("НаименованиеОрганизации", Utils.Current.BankName);
                                }
                                wr.WriteEndElement();
                                wr.WriteElementString("ТипМассиваПоручений", "ОСНОВНОЙ");
                                vx = opisXml.SelectSingleNode("//Месяц");
                                nav = vx.CreateNavigator();
                                wr.WriteNode(nav, false);
                                vx = opisXml.SelectSingleNode("//Год");
                                nav = vx.CreateNavigator();
                                wr.WriteNode(nav, false);
                                wr.WriteElementString("ИсходящийНомер", isx.ToString(new String('0', 10)));
                                wr.WriteElementString("ДатаФормирования", dt.ToString("dd.MM.yyyy"));
                                wr.WriteElementString("Должность", Settings.Default.Dolzh);
                                wr.WriteElementString("Руководитель", Settings.Default.Ruk);
                            }
                            wr.WriteEndElement();
                            wr.WriteStartElement("ОПИСЬ_ВОЗВРАЩАЕМЫХ_ФАЙЛОВ_ПРИ_ЗАЧИСЛЕНИИ");
                            {
                                wr.WriteElementString("КоличествоПередаваемыхФайлов", o.SpisSet.Count.ToString());
                                foreach (var o1 in o.SpisSet)
                                {
                                    wr.WriteStartElement("ПередаваемыйФайл");
                                    {
                                        wr.WriteElementString("ТипДокумента", "ОТЧЕТ_О_ЗАЧИСЛЕНИИ_И_НЕ_ЗАЧИСЛЕНИИ_СУММ");
                                        wr.WriteElementString("ИмяФайла", o1.FileName1);
                                        wr.WriteElementString("КоличествоПолучателей", o1.Kol.ToString());
                                        wr.WriteElementString("НомерЧастиМассива", o1.NumPart.ToString());
                                        wr.WriteElementString("СуммаПоЧастиМассива", o1.Sm.ToString("F2").Replace(',', '.'));
                                    }
                                    wr.WriteEndElement();
                                }
                            }
                            wr.WriteEndElement();
                        }
                        wr.WriteEndElement();
                    }
                    wr.WriteEndElement();
                }
                wr.WriteEndDocument();
            }
            return fname;
        }

        private string Otchet00(SpisSet spis, DateTime dt)
        {
            XmlNode vx;
            XPathNavigator nav;
            int isx = Utils.Current.GetSequence("OUT");
            
            var spisXml = new XmlDocument();
            spisXml.LoadXml(spis.Xml);

            StringBuilder sb = new StringBuilder(spis.FileName.Trim());
            sb.Remove(0, 3).Insert(0, "OUT");
            sb.Remove(60, 4).Insert(60, "OZAC");
            sb.Insert(73, String.Format("-OUTNMB-{0}", isx.ToString(new String('0', 10))));
            string fname = Path.Combine(Settings.Default.PathInputOutput, clsConst.FolderToPFR, sb.ToString());
            using (XmlTextWriter wr = new XmlTextWriter(fname, Encoding.GetEncoding(1251)))
            {
                wr.Formatting = Formatting.Indented;
                wr.WriteStartDocument();
                    wr.WriteStartElement("ФайлПФР");
                        wr.WriteElementString("ИмяФайла", sb.ToString());
                        wr.WriteStartElement("ЗаголовокФайла");
                            wr.WriteElementString("ВерсияФормата", "07.00");
                            wr.WriteElementString("ТипФайла", "ВНЕШНИЙ");
                            wr.WriteStartElement("ПрограммаПодготовкиДанных");
                                wr.WriteElementString("НазваниеПрограммы", "ОБМЕН_С_ПФР");
                                wr.WriteElementString("Версия", Version());
                            wr.WriteEndElement();
                            wr.WriteElementString("ИсточникДанных", "ДОСТАВЩИК");
                        wr.WriteEndElement();
                        wr.WriteStartElement("ПачкаИсходящихДокументов");
                            wr.WriteAttributeString("ДоставочнаяОрганизация", "БАНК");
                            vx = spisXml.SelectSingleNode("//ВХОДЯЩАЯ_ОПИСЬ");
                            nav = vx.CreateNavigator();
                            wr.WriteNode(nav, false);
                            wr.WriteStartElement("ИСХОДЯЩАЯ_ОПИСЬ");
                                wr.WriteStartElement("СоставительПачки");
                                    wr.WriteElementString("НаименованиеОрганизации", Utils.Current.BankName);
                                wr.WriteEndElement();
                                wr.WriteStartElement("СоставДокументов");
                                    wr.WriteElementString("Количество", "1");
                                    wr.WriteStartElement("НаличиеДокументов");
                                        wr.WriteElementString("ТипДокумента", "ОТЧЕТ_О_ЗАЧИСЛЕНИИ_И_НЕ_ЗАЧИСЛЕНИИ_СУММ");
                                        wr.WriteElementString("Количество", "1");
                                    wr.WriteEndElement();
                                wr.WriteEndElement();
                                vx = spisXml.SelectSingleNode("//ТерриториальныйОрганПФР");
                                nav = vx.CreateNavigator();
                                wr.WriteNode(nav, false);
                                wr.WriteElementString("НомерБанка", spis.FileName.Trim().Substring(69, 4));
                                wr.WriteStartElement("ОрганизацияСформировавшаяДокумент");
                                    wr.WriteElementString("НаименованиеОрганизации", Utils.Current.BankName);
                                wr.WriteEndElement();
                                vx = spisXml.SelectSingleNode("//НомерПлатежногоПоручения");
                                if (vx != null)
                                {
                                    nav = vx.CreateNavigator();
                                    wr.WriteNode(nav, false);
                                }
                                vx = spisXml.SelectSingleNode("//ДатаПлатежногоПоручения");
                                if (vx != null)
                                {
                                    nav = vx.CreateNavigator();
                                    wr.WriteNode(nav, false);
                                }
                                wr.WriteElementString("СуммаПоПлатежномуПоручению", "0.00");
                                vx = spisXml.SelectSingleNode("//СистемныйНомерМассива");
                                nav = vx.CreateNavigator();
                                wr.WriteNode(nav, false);
                                vx = spisXml.SelectSingleNode("//ТипМассиваПоручений");
                                nav = vx.CreateNavigator();
                                wr.WriteNode(nav, false);
                                vx = spisXml.SelectSingleNode("//Месяц");
                                nav = vx.CreateNavigator();
                                wr.WriteNode(nav, false);
                                vx = spisXml.SelectSingleNode("//Год");
                                nav = vx.CreateNavigator();
                                wr.WriteNode(nav, false);
                                var smz = spis.TrnSet.Where(s => s.KodZachisl == "З1").Sum(s => s.Sm);
                                var kolz = spis.TrnSet.Where(s => s.KodZachisl == "З1").Count();
                                var smnz = spis.TrnSet.Sum(s => s.Sm) - smz;
                                var kolnz = spis.TrnSet.Count() - kolz;
                                wr.WriteElementString("СуммаЗачисленоПоФилиалу", smz.ToString("F2").Replace(',', '.'));
                                wr.WriteElementString("КоличествоПолучателейЗачислено", kolz.ToString());
                                wr.WriteElementString("СуммаНеЗачисленоПоФилиалу", smnz.ToString("F2").Replace(',', '.'));
                                wr.WriteElementString("КоличествоПолучателейНеЗачислено", kolnz.ToString());
                                wr.WriteElementString("ИсходящийНомер", isx.ToString(new String('0', 10)));
                                wr.WriteElementString("ДатаФормирования", dt.ToString("dd.MM.yyyy"));
                                wr.WriteElementString("Должность", Settings.Default.Dolzh);
                                wr.WriteElementString("Руководитель", Settings.Default.Ruk);
                            wr.WriteEndElement();
                            wr.WriteStartElement("ОТЧЕТ_О_ЗАЧИСЛЕНИИ_И_НЕ_ЗАЧИСЛЕНИИ_СУММ");
                                foreach(var trn in spis.TrnSet)
                                {
                                    wr.WriteStartElement("ОтчетПоПолучателю");
                                        wr.WriteElementString("НомерВмассиве", trn.NumInMassiv.ToString());
                                        wr.WriteElementString("НомерВыплатногоДела", trn.NumVplDelo);
                                        wr.WriteElementString("КодРайона", trn.KodRaiona);
                                        wr.WriteElementString("СтраховойНомер", trn.StraxNum);
                                        wr.WriteStartElement("ФИО");
                                            wr.WriteElementString("Фамилия", trn.Fam);
                                            wr.WriteElementString("Имя", trn.Imya);
                                            wr.WriteElementString("Отчество", trn.Otch);
                                        wr.WriteEndElement();
                                        var acc = string.IsNullOrWhiteSpace(trn.Acc1) ? trn.Acc : trn.Acc1;
                                        wr.WriteElementString("НомерСчета", acc);
                                        wr.WriteStartElement("ВсеВыплаты");
                                            wr.WriteElementString("Количество", trn.TrnSet1.Count.ToString());
                                            foreach(var trn1 in trn.TrnSet1)
                                            {
                                                wr.WriteStartElement("Выплата");
                                                    wr.WriteElementString("ПризнакВыплаты", "ПРОШЕДШАЯ");
                                                    wr.WriteElementString("СуммаКвыплате", trn1.Sm.ToString("F2").Replace(',', '.'));
                                                    wr.WriteElementString("ДатаНачалаПериода", trn1.Dt1);
                                                    wr.WriteElementString("ДатаКонцаПериода", trn1.Dt2);
                                                    wr.WriteElementString("ВидВыплатыПоПЗ", trn1.VidVplPZ.Trim());
                                                wr.WriteEndElement();
                                            }
                                        wr.WriteEndElement();
                                        wr.WriteElementString("СуммаКдоставке", trn.Sm.ToString("F2").Replace(',', '.'));
                                        wr.WriteElementString("ДатаФактическойДоставки", trn.DFakt == null ? "" : trn.DFakt.Value.ToString("dd.MM.yyyy"));
                                        wr.WriteElementString("КодЗачисления", trn.KodZachisl);
                                        wr.WriteElementString("ДатаВыдачиДокумента", dt.ToString("dd.MM.yyyy"));
                                    wr.WriteEndElement();
                                }
                                wr.WriteElementString("СуммаПоФилиалу", spis.Sm.ToString("F2").Replace(',', '.'));
                                wr.WriteElementString("КоличествоПолучателей", spis.TrnSet.Count.ToString());
                                wr.WriteElementString("ДатаВыдачиДокумента", dt.ToString("dd.MM.yyyy"));
                                wr.WriteElementString("СуммаЗачислено", smz.ToString("F2").Replace(',', '.'));
                                wr.WriteElementString("КоличествоПолучателейЗачислено", kolz.ToString());
                                wr.WriteElementString("СуммаНеЗачислено", smnz.ToString("F2").Replace(',', '.'));
                                wr.WriteElementString("КоличествоПолучателейНеЗачислено", kolnz.ToString());
                            wr.WriteEndElement();
                        wr.WriteEndElement();
                    wr.WriteEndElement();
                wr.WriteEndDocument();
            }
            return fname;
        }
    }
}
