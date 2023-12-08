using System;
using System.Data.SqlClient;
using System.Web.Configuration;

namespace SefimV2.Helper
{
    public class DataService
    {
        private static SqlConnection con;
        public static SqlConnection GetConnection()
        {
            string SqlConnString = "Data Source=" + WebConfigurationManager.AppSettings["Server"] + ";Initial Catalog=master;Persist Security Info=True;User Id=" + WebConfigurationManager.AppSettings["User"] + ";Password=" + WebConfigurationManager.AppSettings["Password"] + "; MultipleActiveResultSets=true";
            con = new SqlConnection(SqlConnString);

            try
            {
                con.Open();
                if (con == null)
                {
                    Singleton.WritingLog("DataService", "GetConnection():Con=Null>" + " ConString:" + SqlConnString);
                }
                //con.Close();
                //if (con.State != ConnectionState.Open)
                //{ 
                Singleton.WritingLog("DataService", "Constatus:" + con.State);

                //}
            }
            catch (Exception ex)
            {
                Singleton.WritingLogFile2("DataService_Constr:", ex.Message.ToString(), " StackTrace:", ex.StackTrace);
                con.Close();

                try
                {
                    con.Open();
                }
                catch (Exception ex1)
                {

                    Singleton.WritingLog("DataServiceException2", "Hata:" + ex1);
                }

            }

            //finally
            //{
            //    Singleton.WritingLog("DataService", "GetConnection():Con.Close()>" + " Con.Close" + " State:" + con.State);
            //    con.Close();
            //    //if (con.State != ConnectionState.Open)
            //    //{
            //    //    Singleton.WritingLog("DataService", "GetConnection():Con.Open()>" + " Con.Open");
            //    //    con.Open();
            //    //}
            //}
            return con;
        }

        public static string GetValue(string urunId)
        {
            var conn = GetConnection();
            SqlCommand cmdCommand = new SqlCommand("select [MUSTERIKOD],[MYKEY] from [TBLURUN] where [URUNID]=" + urunId, conn);
            SqlDataReader reader = cmdCommand.ExecuteReader();
            string retval = null;
            try
            {
                if (!reader.Read())
                {
                    return null;
                }
                if (!reader.HasRows)
                {
                    return null;
                }
                retval = reader.GetValue(0) + ";" + reader.GetValue(1);
            }
            finally
            {
                reader.Close();
            }
            return retval;
        }

        public static void SetValue(string urunId, string mykey, string mKod)
        {
            var conn = GetConnection();
            SqlCommand cmdCommand = new SqlCommand("select Count([MYKEY]) from [TBLURUN] where [URUNID]=" + urunId, conn);
            var res = (int)cmdCommand.ExecuteScalar();
            if (res > 0)
            {
                SqlCommand cmdUpdate = new SqlCommand("update [TBLURUN] set [MYKEY]=@p1 where [URUNID]=@p2 AND [MUSTERIKOD]=@p3", conn);
                cmdUpdate.Parameters.AddWithValue("@p1", mykey);
                cmdUpdate.Parameters.AddWithValue("@p2", urunId);
                cmdUpdate.Parameters.AddWithValue("@p3", mKod);
                cmdUpdate.ExecuteNonQuery();
            }
            else
            {
                SqlCommand cmdUpdate = new SqlCommand("insert into [TBLURUN] ([MYKEY],[URUNID],[MUSTERIKOD],[FRM]) values(@par1,@par2,@par3,@par4)", conn);
                cmdUpdate.Parameters.AddWithValue("@par1", mykey);
                cmdUpdate.Parameters.AddWithValue("@par2", urunId);
                cmdUpdate.Parameters.AddWithValue("@par3", mKod);
                cmdUpdate.Parameters.AddWithValue("@par4", 0);
                cmdUpdate.ExecuteNonQuery();
            }
        }

        public static void DeleteValue(string urunId, string mKod)
        {
            var conn = GetConnection();
            SqlCommand cmdCommand = new SqlCommand("Delete From [TBLURUN] Where [URUNID]=@par1 AND [MUSTERIKOD]=@par2", conn);
            cmdCommand.Parameters.AddWithValue("@par1", urunId);
            cmdCommand.Parameters.AddWithValue("@par2", mKod);
            cmdCommand.ExecuteNonQuery();
        }
    }
}