using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Oracle.ManagedDataAccess.Client;
using System.Windows.Forms;
using pfr.Properties;

namespace pfr
{
    public class OracleBd
    {
        static string OraCn = String.Format(@"Data Source=(DESCRIPTION =
        (ADDRESS = (PROTOCOL = TCP)(HOST = {0})(PORT = {2}))
        (CONNECT_DATA =
          (SERVER = DEDICATED)
          (SERVICE_NAME = {3})
        )
  );User ID=PFR;Password={1}", Settings.Default.OraIp, Settings.Default.OraPasword, Settings.Default.OraPort, Settings.Default.OraService);
        public bool IsExistAcc(string acc)
        {
            try
            {
                using (OracleConnection cn =
                    new OracleConnection(OraCn))
                {
                    cn.Open();
                    var cmd = new OracleCommand(String.Format("select caccacc from XXI.\"acc\" where caccacc = '{0}' and caccprizn = 'О'", acc), cn);
                    using (var dr = cmd.ExecuteReader())
                    {
                        return dr.Read();
                    }
                }
            }
            catch (OracleException e1)
            {
                MessageBox.Show(e1.Message);
            }
            catch (Exception e2)
            {
                MessageBox.Show(e2.Message);
            }
            return false;
        }

        public bool ExistTransaction(string acc, decimal sm, DateTime dt, int idtrn, ref DateTime dfakt)
        {
            try
            {
                using (OracleConnection cn =
                    new OracleConnection(OraCn))
                {
                    cn.Open();
                    var sqlstr = String.Format(
                        "select itrnnum, dtrntran from XXI.\"trn\" where ctrnaccc = '{0}' and mtrnsumc = {1} " +
                            "and dtrntran >= TO_DATE('{2}', 'DD.MM.YYYY') and ctrnpurp like '%[Id-{3}]%'",
                        acc, sm.ToString("F2").Replace(',', '.'), dt.ToString("dd.MM.yyyy"), idtrn);
                    var cmd = new OracleCommand(sqlstr, cn);
                    using (var dr = cmd.ExecuteReader())
                    {
                        if (dr.Read())
                        {
                            dfakt = dr.GetOracleDate(1).Value;
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    }
                }
            }
            catch (OracleException e1)
            {
                MessageBox.Show(e1.Message);
            }
            catch (Exception e2)
            {
                MessageBox.Show(e2.Message);
            }
            return false;
        }
    }
}
