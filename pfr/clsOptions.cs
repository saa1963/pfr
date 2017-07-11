using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using pfr.Properties;
using System.Drawing.Design;
using System.IO;

namespace pfr
{
    class clsOptions
    {
        private string _exportpath = Settings.Default.PathInputOutput;
        private string _dolzh = Settings.Default.Dolzh;
        private string _ruk = Settings.Default.Ruk;
        private string _oraip = Settings.Default.OraIp;
        private string _orapassword = Settings.Default.OraPasword;
        private string _oraport = Settings.Default.OraPort;
        private string _oraservice = Settings.Default.OraService;

        [Category("Пути")]
        [DisplayName("Путь")]
        [Description("Путь для обмена файлами")]
        [Editor(typeof(FolderEditor), typeof(UITypeEditor))]
        public string PathInputOutput
        {
            get
            {
                return _exportpath;
            }
            set { _exportpath = value; }
        }

        [Category("Руководство")]
        [DisplayName("Должность")]
        [Description("Должность руководителя")]
        public string Dolzh
        {
            get
            {
                return _dolzh;
            }
            set { _dolzh = value; }
        }

        [Category("Руководство")]
        [DisplayName("ФИО руководителя")]
        [Description("ФИО руководителя")]
        public string Ruk
        {
            get
            {
                return _ruk;
            }
            set { _ruk = value; }
        }

        //[Category("Инверсия")]
        //[DisplayName("Ip адрес сервера")]
        //[Description("Ip адрес сервера")]
        //public string OraIp
        //{
        //    get
        //    {
        //        return _oraip;
        //    }
        //    set { _oraip = value; }
        //}

        //[Category("Инверсия")]
        //[DisplayName("Номер порта")]
        //[Description("Номер порта")]
        //public string OraPort
        //{
        //    get
        //    {
        //        return _oraport;
        //    }
        //    set { _oraport = value; }
        //}

        //[Category("Инверсия")]
        //[DisplayName("Наименование сервиса")]
        //[Description("Наименование сервиса")]
        //public string OraService
        //{
        //    get
        //    {
        //        return _oraservice;
        //    }
        //    set { _oraservice = value; }
        //}

        //[Category("Инверсия")]
        //[DisplayName("Пароль")]
        //[Description("Пароль")]
        //public string OraPassword
        //{
        //    get
        //    {
        //        return _orapassword;
        //    }
        //    set { _orapassword = value; }
        //}
    }

    class FolderEditor : System.Windows.Forms.Design.FolderNameEditor
    {
        /// <summary>
        /// Настройка фильтра расширений 
        /// </summary>
        protected override void InitializeDialog(FolderBrowser folderBrowser)
        {
            base.InitializeDialog(folderBrowser);
        }
    }
}
