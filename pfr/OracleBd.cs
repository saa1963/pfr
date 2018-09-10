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
            using (var cn = new OracleConnection(OraCn))
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

        public object GetDeptInfo(int itrnnum)
        {
            try
            {
                using (OracleConnection cn =
                    new OracleConnection(OraCn))
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

        public bool RegisterDoc(string DebAcc, string CredAcc, decimal Sum, DateTime Dt, string User, string Info)
        {
            bool rt = false;
            using (var cn = new OracleConnection(OraCn))
            {
                int success = -1;
                cn.Open();
                var cmd = new OracleCommand();
                cmd.Connection = cn;
                cmd.CommandText = "IDOC_REG.RegisterR2";
                cmd.CommandType = System.Data.CommandType.StoredProcedure;

                // Возвращаемое значение
                cmd.Parameters.Add("Success", OracleDbType.Varchar2, System.Data.ParameterDirection.ReturnValue);
                // Сообщение об ошибке для возврата
                cmd.Parameters.Add("ErrorMsg", OracleDbType.Varchar2, System.Data.ParameterDirection.Output);
                // Тип операции BO1
                cmd.Parameters.Add("OpType", OracleDbType.Int16, System.Data.ParameterDirection.Input).Value = 1;
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
                // Очередность платежа
                cmd.Parameters.Add("Priority", OracleDbType.Int16, System.Data.ParameterDirection.Input).Value = DBNull.Value;
                // Тип операции 2 порядка
                cmd.Parameters.Add("SubOpType", OracleDbType.Int16, System.Data.ParameterDirection.Input).Value = 1;
                // Наш корсчет
                cmd.Parameters.Add("CorAccO", OracleDbType.Varchar2, 25, System.Data.ParameterDirection.Input).Value = DBNull.Value;
                // Код МФО/МСО/КУ Банка-корреспондента
                cmd.Parameters.Add("MFOa", OracleDbType.Varchar2, 11, System.Data.ParameterDirection.Input).Value = DBNull.Value;
                // Кор.Счет Банка-корреспондента
                cmd.Parameters.Add("CorAccA", OracleDbType.Varchar2, 25, System.Data.ParameterDirection.Input).Value = DBNull.Value;
                // Название Банка-корреспондента
                cmd.Parameters.Add("CorAccAName", OracleDbType.Varchar2, 128, System.Data.ParameterDirection.Input).Value = DBNull.Value;
                // Наименование счета получателя
                cmd.Parameters.Add("RecipientName", OracleDbType.Varchar2, 300, System.Data.ParameterDirection.Input).Value = DBNull.Value;
                // ИНН корреспондента
                cmd.Parameters.Add("INNA", OracleDbType.Varchar2, 13, System.Data.ParameterDirection.Input).Value = DBNull.Value;
                // Способ доставки платежа (почта/телеграф/?л.платеж)
                cmd.Parameters.Add("DeliveryWay", OracleDbType.Varchar2, 1, System.Data.ParameterDirection.Input).Value = DBNull.Value;
                // Наименование клиента нашего банка
                cmd.Parameters.Add("Client_Name", OracleDbType.Varchar2, 300, System.Data.ParameterDirection.Input).Value = DBNull.Value;
                // ИНН клиента нашего банка
                cmd.Parameters.Add("Client_INN", OracleDbType.Varchar2, 13, System.Data.ParameterDirection.Input).Value = DBNull.Value;
                // Код участника (СБ РФ) корреспондента
                cmd.Parameters.Add("SBCodeA", OracleDbType.Int32, System.Data.ParameterDirection.Input).Value = DBNull.Value;
                // Валюта документа (суммы)
                cmd.Parameters.Add("cDocCurrency", OracleDbType.Char, 3, System.Data.ParameterDirection.Input).Value = DBNull.Value;
                // Вид операции
                cmd.Parameters.Add("cVO", OracleDbType.Varchar2, 2, System.Data.ParameterDirection.Input).Value = "17";
                // Флаг: игнорировать возникновение красного сальдо
                cmd.Parameters.Add("bIgnoreRB", OracleDbType.Boolean, System.Data.ParameterDirection.Input).Value = false;
                // Флаг: генерировать ошибку при требовании редактирования комиссий
                cmd.Parameters.Add("cEditComms", OracleDbType.Varchar2, System.Data.ParameterDirection.Input).Value = "NO";
                // Дата поступления документа в банк
                cmd.Parameters.Add("dShadow", OracleDbType.Date, System.Data.ParameterDirection.Input).Value = DBNull.Value;
                // Условия оплаты документа
                cmd.Parameters.Add("cCondPay", OracleDbType.Varchar2, 1024, System.Data.ParameterDirection.Input).Value = DBNull.Value;
                // КПП корреспондента
                cmd.Parameters.Add("KPPA", OracleDbType.Varchar2, 9, System.Data.ParameterDirection.Input).Value = DBNull.Value;
                // Ведомственная информация
                cmd.Parameters.Add("KPPA", OracleDbType., System.Data.ParameterDirection.Input).Value = DBNull.Value;

                cmd.ExecuteNonQuery();
                success = ((Oracle.ManagedDataAccess.Types.OracleDecimal)cmd.Parameters["Success"].Value).ToInt32();
                rt = (success == 0);
            }
            return rt;
        }
    }
}
