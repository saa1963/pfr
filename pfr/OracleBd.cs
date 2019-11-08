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
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
        //      private string OraCn = String.Format(@"Data Source=(DESCRIPTION =
        //      (ADDRESS = (PROTOCOL = TCP)(HOST = {0})(PORT = {2}))
        //      (CONNECT_DATA =
        //        (SERVER = DEDICATED)
        //        (SERVICE_NAME = {3})
        //      )
        //);User ID=PFR;Password={1}", Settings.Default.OraIp, Settings.Default.OraPasword, Settings.Default.OraPort, Settings.Default.OraService);

        //private string OraCn = @"Data Source=ODB;User ID=PFR;Password=zxc";
        //private string OraCn = @"Data Source=TEST;User ID=PFR;Password=zxc";
        //private string OraCn = @"Data Source=TEST;User ID=SAA;Password=Si2)Vz1$";

        internal bool AddVedInform(int from, int to)
        {
            bool rt = false;
            using (var cn = new OracleConnection(Utils.Current.OraCn))
            {
                int success = -1;
                cn.Open();
                var cmd = new OracleCommand();
                cmd.Connection = cn;
                cmd.CommandText = "BSV.Set_Dinfo";
                cmd.CommandType = System.Data.CommandType.StoredProcedure;

                //itnnum_in, itrnnum_out
                cmd.Parameters.Add("success", OracleDbType.Int32, System.Data.ParameterDirection.ReturnValue);
                cmd.Parameters.Add("iTRNnum_in", OracleDbType.Int64, System.Data.ParameterDirection.Input).Value = from;
                cmd.Parameters.Add("iTRNnum_out", OracleDbType.Int64, System.Data.ParameterDirection.Input).Value = to;
                
                cmd.ExecuteNonQuery();
                success = ((Oracle.ManagedDataAccess.Types.OracleDecimal)cmd.Parameters["success"].Value).ToInt32();
                rt = (success == 0);
            }
            return rt;
        }

        internal int ExistPlat(DateTime dtPlat, int numPlat, decimal sumPlat)
        {
            int rt = 0;
            try
            {
                using (var cn = new OracleConnection(Utils.Current.OraCn))
                {
                    cn.Open();
                    var cmd = new OracleCommand(
                        "select itrnnum from XXI.TRN_MF where DTRNTRAN >= :dt1 AND DTRNTRAN < :dt2 AND ITRNDOCNUM = :num AND MTRNSUM = :sm", cn);
                    cmd.Parameters.Add("dt1", OracleDbType.Date).Value = dtPlat;
                    cmd.Parameters.Add("dt2", OracleDbType.Date).Value = DateTime.Today.AddDays(1);
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
                    new OracleConnection(Utils.Current.OraCn))
                {
                    cn.Open();
#pragma warning disable CA2100 // Review SQL queries for security vulnerabilities
                    var cmd = new OracleCommand(String.Format("select iaccotd from XXI.ACC_MF where caccacc = '{0}' and caccprizn = 'О'", acc), cn);
#pragma warning restore CA2100 // Review SQL queries for security vulnerabilities
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

        internal int CheckMir(string acc)
        {
            int rt;
            if (acc.Substring(0, 5) != "40817") return 0;
            try
            {
                OracleCommand cmd = new OracleCommand();
                using (var cn = new OracleConnection(Utils.Current.OraCn))
                {
                    cn.Open();
                    cmd.Connection = cn;
                    cmd.CommandText = "XXI.BSV.get_CardMir_Acc";
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.BindByName = true;

                    // Возвращаемое значение
                    var Success = new OracleParameter("Success", OracleDbType.Decimal);
                    Success.Direction = System.Data.ParameterDirection.ReturnValue;
                    cmd.Parameters.Add(Success);

                    // Сообщение об ошибке для возврата
                    var ErrorMsg = new OracleParameter("msgErr", OracleDbType.Varchar2, 4000);
                    ErrorMsg.Direction = System.Data.ParameterDirection.Output;
                    cmd.Parameters.Add(ErrorMsg);

                    // Счет кредита
                    cmd.Parameters.Add("acc", OracleDbType.Varchar2, 25, System.Data.ParameterDirection.Input).Value = acc;

                    cmd.ExecuteNonQuery();
                    rt = Convert.ToInt32((decimal)(Oracle.ManagedDataAccess.Types.OracleDecimal)cmd.Parameters["Success"].Value);
                    var eMsg = ((Oracle.ManagedDataAccess.Types.OracleString)cmd.Parameters["msgErr"].Value).ToString().Trim();
                    if (rt == 2)
                        logger.Warn($"Счет {acc} не привязан к карте МИР");
                    return rt;
                }
            }
            catch (Exception ex)
            {
                while (ex != null)
                {
                    logger.Error(ex.Message);
                    ex = ex.InnerException;
                }
                return -1234;
            }
        }

        public bool ExistTransaction(string acc, decimal sm, DateTime dt, int idtrn, ref DateTime dfakt, ref int itrnnum)
        {
            try
            {
                using (OracleConnection cn =
                    new OracleConnection(Utils.Current.OraCn))
                {
                    cn.Open();
                    var sqlstr = String.Format(
                        "select itrnnum, dtrntran from XXI.TRN_MF where ctrnaccc = '{0}' and mtrnsumc = {1} " +
                            "and dtrntran >= TO_DATE('{2}', 'DD.MM.YYYY') and ctrnpurp like '%[Id-{3}]%'",
                        acc, sm.ToString("F2").Replace(',', '.'), dt.ToString("dd.MM.yyyy"), idtrn);
#pragma warning disable CA2100 // Review SQL queries for security vulnerabilities
                    var cmd = new OracleCommand(sqlstr, cn);
#pragma warning restore CA2100 // Review SQL queries for security vulnerabilities
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

        public object GetDeptInfo(int itrnnum)
        {
            try
            {
                using (OracleConnection cn =
                    new OracleConnection(Utils.Current.OraCn))
                {
                    cn.Open();
                    var cmd = new OracleCommand("select * from XXI.trn_dept_info where inum = :inum", cn);
                    cmd.Parameters.Add("inum", itrnnum);
                    using (var dr = cmd.ExecuteReader())
                    {
                        if (dr.Read())
                        {
                            return new
                            {
                                CCREATSTATUS = dr["CCREATSTATUS"] is DBNull ? "" : dr["CCREATSTATUS"].ToString(),
                                CBUDCODE = dr["CBUDCODE"] is DBNull ? "" : dr["CBUDCODE"].ToString(),
                                COKATOCODE = dr["COKATOCODE"] is DBNull ? "" : dr["COKATOCODE"].ToString(),
                                CNALPURP = dr["CNALPURP"] is DBNull ? "" : dr["CNALPURP"].ToString(),
                                CNALPERIOD = dr["CNALPERIOD"] is DBNull ? "" : dr["CNALPERIOD"].ToString(),
                                CNALDOCNUM = dr["CNALDOCNUM"] is DBNull ? "" : dr["CNALDOCNUM"].ToString(),
                                CNALDOCDATE = dr["CNALDOCDATE"] is DBNull ? "" : dr["CNALDOCDATE"].ToString(),
                                CNALTYPE = dr["CNALTYPE"] is DBNull ? "" : dr["CNALTYPE"].ToString(),
                                CNALFLAG = dr["CNALFLAG"] is DBNull ? "" : dr["CNALFLAG"].ToString(),
                                CDOCINDEX = dr["CCREATSTATUS"] is DBNull ? "" : dr["CCREATSTATUS"].ToString(),
                                CDOCINDEX_NZ = dr["CDOCINDEX_NZ"] is DBNull ? "" : dr["CDOCINDEX_NZ"].ToString()
                            };
                        }
                        else
                            return null;
                    }
                }
            }
            catch (Exception e)
            {
                var message = String.Format("Ошибка получения ведомственной информации. ITRNNUM = {0}", itrnnum);
                throw new SaaException(message, e);
            }
        }

        public bool RegisterDoc(string DebAcc, string CredAcc, decimal Sum, DateTime Dt, string User, string Info, int IdTrn, bool isSendtoXXI)
        {
            if (!isSendtoXXI) return true;
            try
            {
                bool rt = false;
                OracleCommand cmd = new OracleCommand();
                using (var cn = new OracleConnection(Utils.Current.OraCn))
                {
                    cn.Open();

#pragma warning disable CA2100 // Review SQL queries for security vulnerabilities
                    cmd.CommandText = String.Format(
                            "select itrnnum from XXI.TRN_MF where ctrnaccc = '{0}' and mtrnsumc = {1} " +
                                "and dtrntran >= TO_DATE('{2}', 'DD.MM.YYYY') and dtrntran < TO_DATE('{3}', 'DD.MM.YYYY') and ctrnpurp like '%[Id-{4}]%'",
                            CredAcc, Sum.ToString("F2").Replace(',', '.'), Dt.ToString("dd.MM.yyyy"), Dt.AddDays(1).ToString("dd.MM.yyyy"), IdTrn);
#pragma warning restore CA2100 // Review SQL queries for security vulnerabilities
                    cmd.CommandType = System.Data.CommandType.Text;
                    cmd.Connection = cn;
                    var o = cmd.ExecuteScalar();
                    if (o == null)
                    {
                        string success = "REG_UNKNOWN_ERROR";
                        cmd.Connection = cn;
                        cmd.CommandText = "XXI.SAA.RegisterPfr";
                        cmd.CommandType = System.Data.CommandType.StoredProcedure;
                        cmd.BindByName = true;

                        // Возвращаемое значение
                        var Success = new OracleParameter("Success", OracleDbType.Varchar2, 4000);
                        Success.Direction = System.Data.ParameterDirection.ReturnValue;
                        cmd.Parameters.Add(Success);
                        // Сообщение об ошибке для возврата
                        var ErrorMsg = new OracleParameter("ErrorMsg", OracleDbType.Varchar2, 4000);
                        ErrorMsg.Direction = System.Data.ParameterDirection.Output;
                        cmd.Parameters.Add(ErrorMsg);
                        // Тип операции BO1
                        var OpType = new OracleParameter("OpType", OracleDbType.Int16);
                        OpType.Direction = System.Data.ParameterDirection.Input;
                        OpType.Value = 1;
                        cmd.Parameters.Add(OpType);
                        // Дата регистрации
                        cmd.Parameters.Add("RegDate", OracleDbType.Date, System.Data.ParameterDirection.Input).Value = Dt;
                        // Счет дебета
                        cmd.Parameters.Add("PayerAcc", OracleDbType.Varchar2, 25, System.Data.ParameterDirection.Input).Value = DebAcc;
                        // Счет кредита
                        cmd.Parameters.Add("RecipientAcc", OracleDbType.Varchar2, 25, System.Data.ParameterDirection.Input).Value = CredAcc;
                        // Сумма, если документ нац.вал. то это сумма в рублях
                        cmd.Parameters.Add("Summa", OracleDbType.Decimal, System.Data.ParameterDirection.Input).Value = Sum;
                        // Дата документа
                        cmd.Parameters.Add("DocDate", OracleDbType.Date, System.Data.ParameterDirection.Input).Value = Dt;
                        // Назначение платежа
                        cmd.Parameters.Add("Purpose", OracleDbType.Varchar2, 1024, System.Data.ParameterDirection.Input).Value = Info;
                        // Номер документа
                        cmd.Parameters.Add("DocNum", OracleDbType.Int64, System.Data.ParameterDirection.Input).Value = 2;
                        // Номер пачки
                        cmd.Parameters.Add("BatNum", OracleDbType.Int16, System.Data.ParameterDirection.Input).Value = 0;
                        // Дата платежа (валютирования)
                        cmd.Parameters.Add("ValDate", OracleDbType.Date, System.Data.ParameterDirection.Input).Value = Dt;
                        // Тип операции 2 порядка
                        cmd.Parameters.Add("SubOpType", OracleDbType.Int16, System.Data.ParameterDirection.Input).Value = 1;
                        // Валюта документа (суммы)
                        cmd.Parameters.Add("cDocCurrency", OracleDbType.Char, 3, System.Data.ParameterDirection.Input).Value = "RUR";
                        // Вид операции
                        cmd.Parameters.Add("cVO", OracleDbType.Varchar2, 2, System.Data.ParameterDirection.Input).Value = "17";
                        // Владелец документа
                        cmd.Parameters.Add("cIDOpen", OracleDbType.Varchar2, 4000, System.Data.ParameterDirection.Input).Value = User;
                        cmd.ExecuteNonQuery();
                        success = ((Oracle.ManagedDataAccess.Types.OracleString)cmd.Parameters["Success"].Value).ToString().Trim();
                        var eMsg = ((Oracle.ManagedDataAccess.Types.OracleString)cmd.Parameters["ErrorMsg"].Value).ToString().Trim();
                        rt = (success == "Ok");
                        if (!rt)
                            logger.Error(eMsg);
                    }
                    else
                    {
                        logger.Error(String.Format("Ошибка при регистрации документа. Этот документ уже существует. Дебет {0} Кредит {1} на сумму {2}", DebAcc, CredAcc, Sum));
                        rt = false;
                    }
                }
                return rt;
            }
            catch(Exception e)
            {
                var mess = String.Format("Ошибка при регистрации документа. Дебет {0} Кредит {1} на сумму {2}", DebAcc, CredAcc, Sum);
                logger.Error(mess);
                throw new SaaException(mess, e);
            }
        }

        public string UserOffice()
        {
            var user = Settings.Default.login;
            OracleCommand cmd = new OracleCommand();
            using (var cn = new OracleConnection(Utils.Current.OraCn))
            {
                cn.Open();
                short numOffice = -1;
                cmd.Connection = cn;
                cmd.CommandText = "SELECT iusrbranch FROM XXI.V_USR WHERE UPPER(cusrlogname) = :logname";
                cmd.CommandType = System.Data.CommandType.Text;
                cmd.BindByName = true;

                // Логин пользователя
                var p_cusrlogname = new OracleParameter("logname", OracleDbType.Varchar2, 30);
                p_cusrlogname.Direction = System.Data.ParameterDirection.Input;
                p_cusrlogname.Value = user.ToUpper();
                cmd.Parameters.Add(p_cusrlogname);

                numOffice  = (short)cmd.ExecuteScalar();
                return numOffice.ToString().PadLeft(4, '0');
            }
        }
    }
}
