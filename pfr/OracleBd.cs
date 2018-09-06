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
        //      private string OraCn = String.Format(@"Data Source=(DESCRIPTION =
        //      (ADDRESS = (PROTOCOL = TCP)(HOST = {0})(PORT = {2}))
        //      (CONNECT_DATA =
        //        (SERVER = DEDICATED)
        //        (SERVICE_NAME = {3})
        //      )
        //);User ID=PFR;Password={1}", Settings.Default.OraIp, Settings.Default.OraPasword, Settings.Default.OraPort, Settings.Default.OraService);

        //private string OraCn = @"Data Source=ODB;User ID=PFR;Password=zxc";
        private string OraCn = @"Data Source=TEST;User ID=PFR;Password=zxc";

        internal bool AddVedInform(int from, int to)
        {
            bool rt = false;
            try
            {
                using (var cn = new OracleConnection(OraCn))
                {
                    decimal success = -1;
                    var cmd = new OracleCommand();
                    cmd.Connection = cn;
                    cmd.CommandText = "BSV.Set_Dinfo";
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;

                    //itnnum_in, itrnnum_out
                    cmd.Parameters.Add("itnnum_in", OracleDbType.Decimal, System.Data.ParameterDirection.Input).Value = from;
                    cmd.Parameters.Add("itnnum_out", OracleDbType.Decimal, System.Data.ParameterDirection.Input).Value = to;
                    cmd.Parameters.Add("success", OracleDbType.Decimal, System.Data.ParameterDirection.ReturnValue);

                    cmd.ExecuteNonQuery();
                    success = Convert.ToDecimal(cmd.Parameters["kiekis"].Value);
                    rt = (success == 0);
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
            return rt;
        }

        internal int ExistPlat(DateTime dtPlat, int numPlat, decimal sumPlat)
        {
            int rt = 0;
            try
            {
                using (var cn = new OracleConnection(OraCn))
                {
                    cn.Open();
                    var cmd = new OracleCommand(
                        "select itrnnum from XXI.\"trn\" where DTRNTRAN BETWEEN :dt1 AND :dt2 AND ITRNDOCNUM = :num AND MTRNSUM = :sm", cn);
                    cmd.Parameters.Add("dt1", OracleDbType.Date).Value = dtPlat;
                    cmd.Parameters.Add("dt2", OracleDbType.Date).Value = dtPlat.AddDays(4);
                    cmd.Parameters.Add("num", OracleDbType.Int64).Value = numPlat;
                    cmd.Parameters.Add("sm", OracleDbType.Decimal).Value = sumPlat;
                    var ob = cmd.ExecuteScalar();
                    if (ob != null)
                        rt = Convert.ToInt32(ob);
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
            return rt;
        }

        public bool IsExistAcc(string acc, out string otd)
        {
            otd = "9999";
            try
            {
                using (OracleConnection cn =
                    new OracleConnection(OraCn))
                {
                    cn.Open();
                    var cmd = new OracleCommand(String.Format("select iaccotd from XXI.\"acc\" where caccacc = '{0}' and caccprizn = 'О'", acc), cn);
                    using (var dr = cmd.ExecuteReader())
                    {
                        if (dr.Read())
                        {
                            otd = dr[0].ToString().PadLeft(4, '0');
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

        public bool ExistTransaction(string acc, decimal sm, DateTime dt, int idtrn, ref DateTime dfakt, ref int itrnnum)
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
                            itrnnum = Convert.ToInt32(dr.GetOracleDecimal(0).Value);
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
