using SefimV2.Helper;
using System;
using System.Data;
using System.Data.OleDb;
using System.Data.SqlClient;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Configuration;

namespace SefimV2
{
    public class ModelFunctions
    {
        public string SqlConnString = "Provider=SQLOLEDB;Data Source=" + WebConfigurationManager.AppSettings["Server"] + ";Initial Catalog=" + WebConfigurationManager.AppSettings["DBName"] + ";Persist Security Info=True;User Id=" + WebConfigurationManager.AppSettings["User"] + ";Password=" + WebConfigurationManager.AppSettings["Password"] + "; MultipleActiveResultSets=true";
        public string MongoConnString = WebConfigurationManager.AppSettings["MongoConnString"];
        public string MongoDBName = WebConfigurationManager.AppSettings["MongoDBName"];
        public OleDbConnection ConnOle;
        public OleDbConnection ExternalConnOle;
        static readonly SqlConnection m_GlobalConnection = null;

        public DataTable GetSubeDataWithQuery(string conStr, string sqlText)
        {
            DataTable dtSonuc = new DataTable();
            SqlConnection con = new SqlConnection(conStr);
            try
            {
                using (con)
                {
                    if (con.State != ConnectionState.Open)
                        con.Open();
                    if (con.State == ConnectionState.Open)
                    {
                        using (SqlCommand cmd = new SqlCommand(sqlText, con))
                        {
                            cmd.CommandTimeout = 50;//8;//15;//default 30
                            using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                            {
                                da.Fill(dtSonuc);
                            }
                        };
                    }
                    con.Close();
                }
            }
            catch (Exception ex)
            {
                Singleton.WritingLogFile2("GetSubeDataWithQuery", ex.ToString(), null, sqlText);
                con.Close();
                throw new Exception("Data alınamadı. Hata Detayı:" + ex.Message);
                //throw new NullReferenceException("Data yok.");

            }
            return dtSonuc;
        }

        public string NewConnectionString(string SubeIP, string DBName, string SqlName, string SqlPassword)
        {
            var csb = new SqlConnectionStringBuilder();
            try
            {
                csb.DataSource = SubeIP;
                csb.InitialCatalog = DBName;
                csb.UserID = SqlName;
                csb.Password = SqlPassword;
                csb.ConnectTimeout = 50;
                csb.MultipleActiveResultSets = true;
                csb.ApplicationIntent = ApplicationIntent.ReadOnly;
                csb.Pooling = false;
                csb.LoadBalanceTimeout = 30;
                csb.MaxPoolSize = 2000;
                //SubeCiroDt = f.GetSubeDataWithQuery(csb.ToString(), Query.ToString());
            }
            catch (Exception ex)
            {
                string asd = ex.Message;
                Singleton.WritingLogFile("MdlFunctionsNewConnectionString", ex.Message.ToString() + " InnerException: " + ex.InnerException);
            }

            return csb.ToString();
        }

        public static DataTable ExecuteSQL(String _SQLScript, String _DBName)
        {
            string mesaj = "";
            //for (int i = 0; i < 10; i++)
            //{
            try
            {
                SqlDataAdapter __DataAdapter = new SqlDataAdapter(_SQLScript, m_GlobalConnection);
                DataTable __DataTable = new DataTable();
                __DataAdapter.SelectCommand.CommandTimeout = 60 * 5; //5dk
                __DataAdapter.Fill(__DataTable);

                return __DataTable;
            }
            catch (Exception ex)
            {
                mesaj = ex.Message;
                Thread.Sleep(1000);
            }
            //}
            DataTable DataTable = new DataTable(mesaj);

            return DataTable;
        }

        public void SqlConnOpen(bool ExternalDb = false, string ConnString = "")
        {
            #region ESKİ         
            try
            {
                if (ExternalDb == false)
                {
                    ConnOle = new OleDbConnection(SqlConnString);
                    ConnOle.Open();
                }
                else
                {
                    ExternalConnOle = new OleDbConnection(ConnString);
                    ExternalConnOle.Open();
                }
            }
            catch (DataException ex)
            {
                Singleton.WritingLogFile("SqlConnOpen", ex.Message.ToString() + " InnerException: " + ex.InnerException);
            }

            #endregion
        }
        public void SqlConTask()
        {
            try
            {
                ExternalConnOle.Open();
            }
            catch (Exception ex)
            { }
        }

        public void SQlCloseTask()
        {

            try
            {
                //if (ExternalDb == false)
                //{
                //    ConnOle.Close();
                //}
                //else
                //{
                ExternalConnOle.Close();

                //}
            }
            catch (System.Data.DataException ex) { }
        }

        public async Task MyMethodAsync()
        {
            Task<int> longRunningTask = LongRunningOperationAsync();
            // independent work which doesn't need the result of LongRunningOperationAsync can be done here

            //and now we call await on the task 
            int result = await longRunningTask;
            //use the result 
            //Console.WriteLine(result);
        }
        public async Task<int> LongRunningOperationAsync() // assume we return an int from this long running operation 
        {
            await Task.Delay(1000); // 1 second delay
            return 1;
        }
        //
        public void SqlConnClose_1(bool ExternalDb = false, string ConnString = "")
        {
            try
            {
                if (ExternalDb == false)
                {
                    OleDbConnection ExternalConnOle1 = new OleDbConnection(ConnString);
                    ConnOle.Close();
                }
                else
                {
                    OleDbConnection ExternalConnOle1 = new OleDbConnection(ConnString);
                    ExternalConnOle.Close();
                }
            }
            catch (System.Data.DataException ex) { }
        }
        //
        public void SqlConnClose(bool ExternalDb = false)
        {
            try
            {
                if (ExternalDb == false)
                {
                    ConnOle.Close();
                }
                else
                {
                    //OleDbConnection ExternalConnOle1 = new OleDbConnection(ConnString);
                    ExternalConnOle.Close();
                    //var task = Task.Run(() => SQlCloseTask());
                    //if (task.Wait(TimeSpan.FromSeconds(4)))
                    //{
                    //    string aaa = task.Status.ToString();
                    //    string jhjh = task.IsCanceled.ToString();
                    //    return;
                    //}
                    //else { throw new Exception("Timed out"); }
                }
            }
            catch (DataException ex)
            {
                Singleton.WritingLogFile("MdlFunctionsSqlConnClose", ex.Message.ToString() + " InnerException: " + ex.InnerException);
            }
        }
        public DataTable DataTable(string komut, bool ExternalDb = false)
        {
            DataTable dt = new DataTable();
            try
            {
                OleDbCommand command;
                if (ExternalDb == false) { command = new OleDbCommand(komut, ConnOle); }
                else { command = new OleDbCommand(komut, ExternalConnOle); }

                dt.Load(command.ExecuteReader());
            }
            catch (DataException ex)
            {
                Singleton.WritingLogFile("MdlFunctionsDataTable", ex.Message.ToString() + " InnerException: " + ex.InnerException);
            }
            return dt;
        }
        public int ToInt(string val)
        {
            int sonuc = 0;
            try { sonuc = Convert.ToInt32(val); }
            catch (System.Exception ex) { }
            return sonuc;
        }
        public string ToIntString(string val) { return ToInt(val).ToString(); }
        public int ToInt1(string val) { int sonuc = ToInt(val); if (sonuc < 1) { sonuc = 1; } return sonuc; }
        public decimal ToDecimal(string val)
        {
            decimal sonuc = 0;
            try
            {
                val = val.Replace(".", "");
                sonuc = Convert.ToDecimal(val);
            }
            catch (System.Exception ex) { }
            return sonuc;
        }
        public string ToDecimalString(string val) { return ToDecimal(val).ToString(); }
        public string ToDecimalSqlStr(string val)
        {
            string sonuc = "0";
            try
            {
                sonuc = Convert.ToDecimal(val).ToString();
                sonuc = sonuc.Replace(".", "");
                sonuc = sonuc.Replace(",", ".");
            }
            catch (System.Exception ex) { }
            return sonuc;
        }
        public DateTime ToDateTime(string val)
        {
            DateTime sonuc = Convert.ToDateTime("01.01.1900 00:00:00");
            try
            {
                sonuc = Convert.ToDateTime(val);
            }
            catch { }
            return sonuc;
        }
        public string RTS(DataRow r, string column)
        {
            string sonuc = "";
            try
            {
                sonuc = r[column].ToString();
            }
            catch (System.Exception ex) { }
            return sonuc;
        }
        public decimal RTD(DataRow r, string column)
        {
            decimal sonuc = 0;
            try
            {
                sonuc = Convert.ToDecimal(r[column]);
            }
            catch (System.Exception ex) { }
            return sonuc;
        }
        public int RTI(DataRow r, string column)
        {
            int sonuc = 0;
            try
            {
                sonuc = Convert.ToInt32(r[column].ToString());
            }
            catch (Exception ex) { }
            return sonuc;
        }
    }
}