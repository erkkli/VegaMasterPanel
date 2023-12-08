using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.OleDb;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Configuration;

namespace SefimV2
{
    public class ModelFunctions
    {

        public string SqlConnString = "Provider=SQLOLEDB;Data Source=" + WebConfigurationManager.AppSettings["Server"] + ";Initial Catalog=" + WebConfigurationManager.AppSettings["DBName"] + ";Persist Security Info=True;User ID=" + WebConfigurationManager.AppSettings["User"] + ";Password=" + WebConfigurationManager.AppSettings["Password"] + "; MultipleActiveResultSets=true";
        public OleDbConnection ConnOle;
        public OleDbConnection ExternalConnOle;

        static SqlConnection m_GlobalConnection = null;



        public DataTable GetSubeDataWithQuery(string conStr, string sqlText)
        {
            DataTable dtSonuc = new DataTable();

            //try
            //{
                using (SqlConnection con = new SqlConnection(conStr))
                {
                    con.Open();
                    if (con.State == ConnectionState.Open)
                    {
                        using (SqlCommand cmd = new SqlCommand(sqlText, con))
                        {
                            cmd.CommandTimeout = 15;//default 30
                            using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                            {
                                da.Fill(dtSonuc);
                            }
                        };
                    }
                    con.Close();
                }

            //}
            //catch (Exception)
            //{ }
            return dtSonuc;
        }

        public string NewConnectionString(string SubeIP, string DBName, string SqlName, string SqlPassword)
        {           
            SqlConnectionStringBuilder csb = new SqlConnectionStringBuilder();
            try
            {

                csb.DataSource = SubeIP;
                csb.InitialCatalog = DBName;
                csb.UserID = SqlName;
                csb.Password = SqlPassword;
                csb.ConnectTimeout = 3;
                csb.MultipleActiveResultSets = true;
                csb.ApplicationIntent = ApplicationIntent.ReadOnly;
                csb.Pooling = false;
                //csb.LoadBalanceTimeout = 2;
                //csb.MaxPoolSize = 1;
                //SubeCiroDt = f.GetSubeDataWithQuery(csb.ToString(), Query.ToString());
            }
            catch (Exception ex)
            {
                string asd = ex.Message;
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
            #region YENI
            //try
            //{
            //    if (ExternalDb == false)
            //    {
            //        ConnOle = new OleDbConnection(SqlConnString);
            //        ConnOle.Open();
            //    }
            //    else
            //    {
            //        ExternalConnOle = new OleDbConnection(ConnString);

            //        #region MyRegion
            //        //ExternalConnOle.ConnectionTimeout = 5;
            //        // Task.Run(() =>
            //        //{
            //        //    using (var conn = new OleDbConnection(ConnString))
            //        //    {
            //        //        conn.Open();
            //        //    }
            //        //}).Wait(5);
            //        //var task = System.Threading.Tasks.Task.Run(() => ExternalConnOle.Open()).Wait(2);
            //        //var task = System.Threading.Tasks.Task.Run(() => SqlConTask()).Wait(7);
            //        #endregion
            //        var task = Task.Run(() => SqlConTask());
            //        if (task.Wait(TimeSpan.FromSeconds(4)))
            //        {
            //            string aaa = task.Status.ToString();
            //            string jhjh = task.IsCanceled.ToString();

            //            return;
            //        }
            //        else { throw new Exception("Timed out"); }

            //        #region MyRegion                
            //        //var task = Task.Run(() => SomeMethod(input));
            //        //if (task.Wait(TimeSpan.FromSeconds(10)))
            //        //    return task.Result;
            //        //else
            //        //    throw new Exception("Timed out");
            //        //ExternalConnOle.Open();
            //        #endregion
            //    }
            //}
            //catch (System.Data.DataException ex) { }
            #endregion

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
            catch (System.Data.DataException ex)
            {
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
            catch (System.Data.DataException ex) { }
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
            catch (System.Data.DataException ex) { }
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
            catch (System.Exception ex) { }
            return sonuc;
        }
    }






}