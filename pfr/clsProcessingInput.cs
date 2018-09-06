using pfr.Properties;
using pfr.Xsd1;
using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Serialization;

namespace pfr
{
    internal class clsProcessingInput
    {
        public string DoIt()
        {
            int kolisx = 0, kolvx = 0;
            string doffice = null; ;
            string folder = null; ;

            var ИменаФайловСписковОписейНаЗачисление = 
                Directory.GetFiles(Path.Combine(Settings.Default.PathInputOutput, clsConst.FolderFromPFR),
                "PFR-???-Y-????-ORG-???-???-??????-DIS-???-DCK-?????-???-DOC-OPPF-FSB-????.XML");
            foreach (var ИмяФайлаОписьНаЗачисление in ИменаФайловСписковОписейНаЗачисление)
            {
                pfr.Xsd.ФайлПФР Опись = null;
                var ИменаФайловСписковНаЗачисление = НайтиСпискиФайловНаЗачисление(ИмяФайлаОписьНаЗачисление, ref Опись);
                try
                {
                    if (ИменаФайловСписковНаЗачисление != null)
                    {
                        var Списки = new List<pfr.Xsd1.ФайлПФР>();
                        pfr.Xsd1.ФайлПФР Список = null;
                        var auxDict = new Dictionary<string, int>();
                        foreach (var ИмяФайлаСписокНаЗачисление in ИменаФайловСписковНаЗачисление)
                        {
                            if (!(new clsProcessing().ОбработатьСписокНаЗачисление(ИмяФайлаСписокНаЗачисление, ref Список)))
                            {
                                MessageBox.Show(String.Format("Файл списка {0} не соответствует формату.", new FileInfo(ИмяФайлаСписокНаЗачисление).Name));
                                // Сформировать отрицательный ответ
                                folder = clsConst.FolderBad;
                                goto delete;
                            }
                            var dtPlat = new DateTime(Int32.Parse(Список.ПачкаВходящихДокументов.ВХОДЯЩАЯ_ОПИСЬ.ДатаПлатежногоПоручения.Substring(6, 4)),
                                Int32.Parse(Список.ПачкаВходящихДокументов.ВХОДЯЩАЯ_ОПИСЬ.ДатаПлатежногоПоручения.Substring(3, 2)),
                                Int32.Parse(Список.ПачкаВходящихДокументов.ВХОДЯЩАЯ_ОПИСЬ.ДатаПлатежногоПоручения.Substring(0, 2)));
                            var numPlat = Int32.Parse(Список.ПачкаВходящихДокументов.ВХОДЯЩАЯ_ОПИСЬ.НомерПлатежногоПоручения);
                            var sumPlat = Список.ПачкаВходящихДокументов.ВХОДЯЩАЯ_ОПИСЬ.СуммаПоЧастиМассива;
                            int idPlat;
                            if ((idPlat = new OracleBd().ExistPlat(dtPlat, numPlat, sumPlat)) == 0)
                            {
                                MessageBox.Show(String.Format("Для файла списка {0} не найдена платежка № {1} с датой между {2} и {3} на сумму {4}. " + 
                                    "Списки по описи {5} не обработаны. Попробуйте повторно обработать их после проведения платежа в Инверсии XXI век.",
                                    new object[] { new FileInfo(ИмяФайлаСписокНаЗачисление).Name, numPlat, dtPlat, dtPlat.AddDays(4), sumPlat, new FileInfo(ИмяФайлаОписьНаЗачисление).Name}));
                                throw new ExceptionNotFindPlat();
                            }
                            auxDict.Add(Список.ИмяФайла, idPlat);
                            kolvx++;
                            Списки.Add(Список);
                        }
                        #region Пишем в базу, даем положительный ответ
                        folder = clsConst.FolderArchive;
                        using (var ctx = new pfrEntities1(Utils.Current.cn))
                        {
                            var o = new OpisSet();
                            o.DateReg = DateTime.Now;
                            o.FileName = new FileInfo(ИмяФайлаОписьНаЗачисление).Name;
                            o.Xml = File.ReadAllText(ИмяФайлаОписьНаЗачисление, Encoding.GetEncoding(1251));
                            o.God = int.Parse(Опись.ПачкаВходящихДокументов.ВХОДЯЩАЯ_ОПИСЬ.Год);
                            o.Mec = int.Parse(Опись.ПачкаВходящихДокументов.ВХОДЯЩАЯ_ОПИСЬ.Месяц);
                            o.Kol = int.Parse(Опись.ПачкаВходящихДокументов.ВХОДЯЩАЯ_ОПИСЬ.ОбщееКоличествоПорученийПоМассиву);
                            o.KolObrab = 0;
                            o.NumMassiv = Опись.ПачкаВходящихДокументов.ВХОДЯЩАЯ_ОПИСЬ.СистемныйНомерМассива;
                            o.Sm = Опись.ПачкаВходящихДокументов.ВХОДЯЩАЯ_ОПИСЬ.ОбщаяСуммаПоМассиву;
                            ctx.OpisSet.Add(o);
                            ctx.SaveChanges();

                            foreach (var ИмяФайлаСписокНаЗачисление in ИменаФайловСписковНаЗачисление)
                            {
                                Список = Списки.First(s0 => s0.ИмяФайла == new FileInfo(ИмяФайлаСписокНаЗачисление).Name);

                                var s = new SpisSet();
                                s.DateReg = o.DateReg;
                                s.FileName = Список.ИмяФайла;
                                s.OpisId = o.Id;
                                s.Xml = File.ReadAllText(ИмяФайлаСписокНаЗачисление, Encoding.GetEncoding(1251));
                                s.mec = int.Parse(Список.ПачкаВходящихДокументов.ВХОДЯЩАЯ_ОПИСЬ.Месяц);
                                s.god = int.Parse(Список.ПачкаВходящихДокументов.ВХОДЯЩАЯ_ОПИСЬ.Год);
                                s.Kol = int.Parse(Список.ПачкаВходящихДокументов.ВХОДЯЩАЯ_ОПИСЬ.КоличествоПорученийПоЧастиМассива);
                                s.Sm = Список.ПачкаВходящихДокументов.ВХОДЯЩАЯ_ОПИСЬ.СуммаПоЧастиМассива;
                                s.NumPart = int.Parse(Список.ПачкаВходящихДокументов.ВХОДЯЩАЯ_ОПИСЬ.НомерЧастиМассива);
                                s.ITrnNum = auxDict[Список.ИмяФайла];
                                ctx.SpisSet.Add(s);
                                ctx.SaveChanges();

                                foreach (var z in Список.ПачкаВходящихДокументов.СПИСОК_НА_ЗАЧИСЛЕНИЕ.СведенияОполучателе)
                                {
                                    var zch = new TrnSet();
                                    zch.IdSpis = s.Id;
                                    zch.DateReg = o.DateReg;
                                    zch.Acc = z.НомерСчета;
                                    zch.Fam = z.ФИО.Фамилия.Trim();
                                    zch.Imya = z.ФИО.Имя.Trim();
                                    zch.Otch = z.ФИО.Отчество.Trim();
                                    zch.KodRaiona = z.КодРайона;
                                    zch.NumInMassiv = int.Parse(z.НомерВмассиве);
                                    zch.NumVplDelo = z.НомерВыплатногоДела;
                                    zch.Sm = z.СуммаКдоставке;
                                    zch.StraxNum = z.СтраховойНомер;
                                    ctx.TrnSet.Add(zch);
                                    if (!new OracleBd().IsExistAcc(z.НомерСчета, out doffice))
                                    {
                                        doffice = "9999";
                                    }
                                    zch.DOffice = doffice;
                                    ctx.SaveChanges();
                                    foreach (var z0 in z.ВсеВыплаты.Выплата)
                                    {
                                        var zch1 = new TrnSet1();
                                        zch1.Dt1 = z0.ДатаНачалаПериода;
                                        zch1.Dt2 = z0.ДатаКонцаПериода;
                                        zch1.IdTrn = zch.Id;
                                        zch1.Sm = z0.СуммаКвыплате;
                                        zch1.VidVplPZ = z0.ВидВыплатыПоПЗ;
                                        ctx.TrnSet1.Add(zch1);
                                    }
                                }
                            }
                            kolisx++;
                            var fname1 = new clsProcessing().СформироватьПоложительныйОтвет(Опись, Списки);
                            o.FileName1 = new FileInfo(fname1).Name;
                            o.Xml1 = File.ReadAllText(fname1, Encoding.GetEncoding(1251));
                            ctx.SaveChanges();
                        }
                        #endregion
                        kolvx++;
                    }
                    else
                    {
                        MessageBox.Show(String.Format("Файл описи {0} не соответствует формату.", new FileInfo(ИмяФайлаОписьНаЗачисление).Name));
                        //Сформировать отрицательный ответ
                        folder = clsConst.FolderBad;
                        File.Copy(ИмяФайлаОписьНаЗачисление,
                            Path.Combine(Settings.Default.PathInputOutput, clsConst.FolderFromPFR, folder,
                            new FileInfo(ИмяФайлаОписьНаЗачисление).Name), true);
                        File.Delete(ИмяФайлаОписьНаЗачисление);
                    }
                }
                catch (DbUpdateException e1)
                {
                    MessageBox.Show("Ошибка БД, обратитесь к администратору.");
                    folder = clsConst.FolderBad;
                }
                catch (ExceptionNotFindPlat)
                {
                    continue;
                }
            delete:
                File.SetAttributes(ИмяФайлаОписьНаЗачисление, FileAttributes.Normal);
                File.Copy(ИмяФайлаОписьНаЗачисление,
                        Path.Combine(Settings.Default.PathInputOutput, clsConst.FolderFromPFR, folder,
                        new FileInfo(ИмяФайлаОписьНаЗачисление).Name), true);
                File.Delete(ИмяФайлаОписьНаЗачисление);
                foreach (var ИмяФайлаСписокНаЗачисление in ИменаФайловСписковНаЗачисление)
                {
                    File.SetAttributes(ИмяФайлаСписокНаЗачисление, FileAttributes.Normal);
                    File.Copy(ИмяФайлаСписокНаЗачисление,
                        Path.Combine(Settings.Default.PathInputOutput, clsConst.FolderFromPFR, folder,
                        new FileInfo(ИмяФайлаСписокНаЗачисление).Name), true);
                    File.Delete(ИмяФайлаСписокНаЗачисление);
                }
            }
            return String.Format("Обработано {0} входящих файлов. Сформировано {1} исходящих файлов.", kolvx, kolisx);
        }

        

        private List<string> НайтиСпискиФайловНаЗачисление(string ИмяФайлаОписьНаЗачисление, ref pfr.Xsd.ФайлПФР o)
        {
            List<string> rt = new List<string>();
            XmlSerializer serializer = new XmlSerializer(typeof(pfr.Xsd.ФайлПФР));

            using (var reader = new StreamReader(ИмяФайлаОписьНаЗачисление, Encoding.GetEncoding(1251)))
            {
                try
                {
                    o = (pfr.Xsd.ФайлПФР)serializer.Deserialize(reader);
                }
                catch (InvalidOperationException e)
                {
                    MessageBox.Show(String.Format("Ошибка обработки файла {0}. {1}", ИмяФайлаОписьНаЗачисление, e.Message));
                    return null;
                }
            }
            rt = new List<string>();
            foreach (var fname in o.ПачкаВходящихДокументов.ОПИСЬ_ПЕРЕДАВАЕМЫХ_ФАЙЛОВ_НА_ЗАЧИСЛЕНИЕ.ПередаваемыйФайл)
            {
                rt.Add(Path.Combine(Settings.Default.PathInputOutput, clsConst.FolderFromPFR, fname.ИмяФайла));
            }
            return rt;
        }
    }

    class ExceptionNotFindPlat: Exception
    {
        public ExceptionNotFindPlat() : base() { }
    }
}
