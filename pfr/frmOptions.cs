using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using pfr.Properties;
using System.Reflection;

namespace pfr
{
    public partial class frmOptions : Form
    {
        clsOptions opt;
        public frmOptions()
        {
            InitializeComponent();
            opt = new clsOptions();
            opt.PathInputOutput = Settings.Default.PathInputOutput;
        }

        private void frmOptions_Load(object sender, EventArgs e)
        {
            RestoreGridSplitterPos();
            pg.SelectedObject = opt;
        }

        private void frmOptions_FormClosing(object sender, FormClosingEventArgs e)
        {
            Settings.Default.PathInputOutput = opt.PathInputOutput;
            Settings.Default.Save();
            SaveGridSplitterPos();
        }

        /// <summary>
        /// Сохранение положения разделителя в гриде
        /// </summary>
        private void SaveGridSplitterPos()
        {
            Type type = pg.GetType();
            FieldInfo field = type.GetField("gridView",
              BindingFlags.NonPublic | BindingFlags.Instance);

            object valGrid = field.GetValue(pg);
            Type gridType = valGrid.GetType();
            Settings.Default.GridSplitterPos = (int)gridType.InvokeMember(
              "GetLabelWidth",
              BindingFlags.Public | BindingFlags.InvokeMethod | BindingFlags.Instance,
              null,
              valGrid, new object[] { });
        }

        /// <summary>
        /// Восстановление положения разделителя в гриде
        /// </summary>
        private void RestoreGridSplitterPos()
        {
                Type type = pg.GetType();
                FieldInfo field = type.GetField("gridView",
                  BindingFlags.NonPublic | BindingFlags.Instance);

                object valGrid = field.GetValue(pg);
                Type gridType = valGrid.GetType();
                gridType.InvokeMember("MoveSplitterTo",
                  BindingFlags.NonPublic | BindingFlags.InvokeMethod
                    | BindingFlags.Instance,
                  null,
                  valGrid, new object[] { Settings.Default.GridSplitterPos });
        }
    }
}
