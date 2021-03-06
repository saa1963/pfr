using pfr.Properties;
using System;
using System.Collections.Generic;
using System.Data.Entity.Core.EntityClient;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace pfr
{
    class Utils
    {
        static public string[] months = {"январь", "февраль", "март", "апрель", "май", "июнь", 
                                     "июль", "август", "сентябрь", "октябрь", "ноябрь", "декабрь"};
        static private Utils _Current = null;
        private EntityConnection _cn = null;
        static public Utils Current
        {
            get
            {
                if (_Current == null)
                {
                    _Current = new Utils();
                }
                return _Current;
            }
        }
        private Utils()
        {
        }
        public EntityConnection cn
        {
            get
            {
                return _cn;
            }
            set
            {
                _cn = value;
            }
        }

        public string OraCn { get; set; }
        public string UserOffice { get; set; }

        public int GetSequence(string pname)
        {
            using (var ctx = new pfrEntities1(Utils.Current.cn))
            {
                var name = new SqlParameter();
                name.ParameterName = "@name";
                name.Direction = System.Data.ParameterDirection.Input;
                name.SqlDbType = System.Data.SqlDbType.VarChar;
                name.Size = 50;
                name.Value = pname;

                var def = new SqlParameter();
                def.ParameterName = "@def";
                def.Direction = System.Data.ParameterDirection.Input;
                def.SqlDbType = System.Data.SqlDbType.Int;
                def.Value = 1;

                var inc = new SqlParameter();
                inc.ParameterName = "@inc";
                inc.Direction = System.Data.ParameterDirection.Input;
                inc.SqlDbType = System.Data.SqlDbType.Int;
                inc.Value = 1;

                var curdt = new SqlParameter();
                curdt.ParameterName = "@curdt";
                curdt.Direction = System.Data.ParameterDirection.Input;
                curdt.SqlDbType = System.Data.SqlDbType.DateTime;
                curdt.Value = DateTime.Now;

                var output = new SqlParameter();
                output.ParameterName = "@rt";
                output.Direction = System.Data.ParameterDirection.Output;
                output.SqlDbType = System.Data.SqlDbType.Int;

                ctx.Database.ExecuteSqlCommand("exec GetSequenceNumber3 @name, @def, @inc, @curdt, @rt output", 
                    name, def, inc, curdt, output);
                return (int)output.Value;
            }
        }

        public bool СоздатьПапкиДляВводаВывода()
        {
            string path;
            if (String.IsNullOrWhiteSpace(Settings.Default.PathInputOutput))
            {
                path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "PFR");
                Settings.Default.PathInputOutput = path;
                Settings.Default.Save();
            }
            else
            {
                path = Settings.Default.PathInputOutput;
            }
            try
            {
                Directory.CreateDirectory(Path.Combine(path, clsConst.FolderFromPFR, clsConst.FolderArchive));
                Directory.CreateDirectory(Path.Combine(path, clsConst.FolderFromPFR, clsConst.FolderBad));
                Directory.CreateDirectory(Path.Combine(path, clsConst.FolderToPFR, clsConst.FolderArchive));
                return true;
            }
            catch
            {
                MessageBox.Show(String.Format("Невозможно создать папку для ввода/вывода {0}", path));
                return false;
            }
        }

        public string LogMessage
        {
            get => String.Format("Подробно в журнале событий в папке {0}",
                        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "pfr"));
        }

        public string BankName
        {
            get => "АО Банк \"ТКПБ\"";
        }

        public int Obrabotano(OpisSet opis)
        {
            int obr = 0;
            foreach (var o in opis.SpisSet)
            {
                foreach(var o1 in o.TrnSet)
                {
                    if (o1.KodZachisl != null) obr++;
                }
            }
            return obr;
        }

        public void DeepLogException(NLog.Logger logger,  Exception e)
        {
            do
            {
                logger.Error(e.Message);
                e = e.InnerException;
            }
            while (e.InnerException != null);
        }
    }
}
