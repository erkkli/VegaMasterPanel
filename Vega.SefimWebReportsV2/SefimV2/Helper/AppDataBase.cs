using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace SefimV2.Helper
{
    public class AppDataBase
    {
        public SqlConnection conn { get; set; }
        public SqlCommand cmd { get; set; }
        private SqlDataAdapter dap { get; set; }
        public SqlCommand rcmd { get; set; }
        public SqlDataReader dr { get; set; }
        public int insertid { get; set; }
        public AppDataBase()
        {
            conn = new SqlConnection(ConfigurationManager.ConnectionStrings["Cnn"].ConnectionString);
        }
        public void _cnnopen()
        {
            if (conn.State == ConnectionState.Closed)
            {
                conn.Open();
            }
        }
        public void _cnnclose()
        {
            if (conn.State == ConnectionState.Open)
            {
                conn.Close();
            }
        }
        public DataTable SorguGridDT(string sp)
        {
            if (cmd == null)
            {
                cmd = new SqlCommand();
            }
            cmd = new SqlCommand(sp, conn);
            cmd.CommandType = CommandType.Text;
            cmd = new SqlCommand(sp, conn);
            dap = new SqlDataAdapter(cmd);
            dt = new DataTable();
            dap.Fill(dt);
            cmd.Dispose();
            dap.Dispose();
            return dt;
        }
        public SqlDataReader SPQRDR(string sp, bool _param, string[] veri, object[] parametre) //StoredProcedure Sorgu Reader
        {
            if (cmd == null)
            {
                cmd = new SqlCommand();
            }
            cmd = new SqlCommand(sp, conn);
            dap = new SqlDataAdapter(cmd);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandTimeout = 0;
            if (_param == true)
            {
                for (int m = 0; m < veri.GetLength(0); ++m)
                {
                    AddParameter(veri[m], parametre[m]);
                }
            }
            _cnnopen();
            dr = cmd.ExecuteReader(CommandBehavior.CloseConnection);
            cmd.Dispose();
            dap.Dispose();
            ClearParameters();
            return dr;
        }

        public SqlDataReader SPQRDR22(string sp, bool _param, string[] veri, string parametre) //StoredProcedure Sorgu Reader
        {
            if (cmd == null)
            {
                cmd = new SqlCommand();
            }
            cmd = new SqlCommand(sp, conn);
            dap = new SqlDataAdapter(cmd);
            cmd.CommandType = CommandType.StoredProcedure;
            //Sorguda Parametre Gönderilecekse
            if (_param == true)
            {
                for (int m = 0; m < veri.GetLength(0); ++m)
                {
                    AddParameter(veri[m], parametre);
                }
            }
            _cnnopen();
            dr = cmd.ExecuteReader(CommandBehavior.CloseConnection);
            cmd.Dispose();
            dap.Dispose();
            ClearParameters();
            return dr;
        }

        public SqlDataReader TXTQRDR(string sql)
        {
            if (cmd == null)
            {
                cmd = new SqlCommand();
            }
            cmd.Connection = conn;
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = sql;
            dap = new SqlDataAdapter(cmd);
            _cnnopen();
            dr = cmd.ExecuteReader(CommandBehavior.CloseConnection);
            cmd.Dispose();
            dap.Dispose();
            ClearParameters();
            return dr;
        }

        public SqlDataReader TXTQRDRs(string sql)
        {
            if (cmd == null)
            {
                cmd = new SqlCommand();
            }
            cmd.Connection = conn;
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = sql;
            dap = new SqlDataAdapter(cmd);
            _cnnopen();
            dr = cmd.ExecuteReader(CommandBehavior.CloseConnection);
            cmd.Dispose();
            dap.Dispose();
            ClearParameters();
            return dr;
        }

        public void TXTQ(string sql)
        {
            if (cmd == null)
            {
                cmd = new SqlCommand();
            }
            cmd.Connection = conn;
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = sql;
            _cnnopen();
            cmd.ExecuteNonQuery();
            _cnnclose();
            cmd.Dispose();
            ClearParameters();
        }

        public object RunSqlScalar(string sql)
        {
            if (cmd == null)
            {
                cmd = new SqlCommand();
            }
            cmd.Connection = conn;
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = sql;
            _cnnopen();
            object result = cmd.ExecuteScalar();
            _cnnclose();
            cmd.Dispose();
            ClearParameters();
            return result;
        }

        public object RunSPScalar(string sp_name)
        {
            if (cmd == null)
            {
                cmd = new SqlCommand();
            }
            cmd.Connection = conn;
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = sp_name;
            _cnnopen();
            object result = cmd.ExecuteScalar();
            _cnnclose();
            cmd.Dispose();
            ClearParameters();
            return result;
        }

        public void RunSPQuery(string sp_name, bool Id, string value)
        {
            if (cmd == null)
            {
                cmd = new SqlCommand();
            }
            cmd.Connection = conn;
            cmd.CommandType = CommandType.StoredProcedure;
            if (Id == true)
            {
                cmd.Parameters.Add(value, SqlDbType.Int);
                cmd.Parameters[value].Direction = ParameterDirection.Output;
            }
            cmd.CommandText = sp_name;
            _cnnopen();
            cmd.ExecuteNonQuery();
            if (Id == true)
            {
                insertid = Convert.ToInt32(cmd.Parameters[value].Value.ToString());
            }
            conn.Close();
            cmd.Dispose();
            ClearParameters();
        }

        public string returnTransactionValue { get; set; }

        public string returnStringTransactionValue { get; set; }

        public void RunSPQueryTransaction(string sp_name, bool Id, string value)
        {
            if (cmd == null)
            {
                cmd = new SqlCommand();
            }
            cmd.Connection = conn;
            cmd.CommandType = CommandType.StoredProcedure;
            if (Id == true)
            {
                cmd.Parameters.Add(value, SqlDbType.Int);
                cmd.Parameters[value].Direction = ParameterDirection.Output;
            }
            cmd.CommandText = sp_name;
            _cnnopen();

            //
            SqlParameter returnParameter = cmd.Parameters.Add("RetVal", SqlDbType.Int);
            SqlParameter returnstringParameter = cmd.Parameters.Add("@Return_Message", SqlDbType.VarChar, 1024);
            returnParameter.Direction = ParameterDirection.ReturnValue;
            returnstringParameter.Direction = ParameterDirection.Output;
            //
            cmd.ExecuteNonQuery();
            //      
            returnTransactionValue = (string)returnParameter.Value.ToString();
            returnStringTransactionValue = (string)returnstringParameter.Value.ToString();

            if (Id == true)
            {
                insertid = Convert.ToInt32(cmd.Parameters[value].Value.ToString());
            }

            conn.Close();
            cmd.Dispose();
            ClearParameters();
        }

        public void DLT(string[] dbparam, object[] value, string sp)
        {
            for (int m = 0; m < dbparam.GetLength(0); ++m)
            {
                AddParameter(dbparam[m], value[m]);
            }
            RunSPQuery(sp, false, null);
            ClearParameters();
        }

        public void AddParameter(string name, object value)
        {
            if (cmd == null)
            {
                cmd = new SqlCommand();
            }
            cmd.Parameters.AddWithValue(name, value);
        }

        public void ClearParameters()
        {
            cmd.Parameters.Clear();
        }

        private DataTable dt;
        public DataTable SPsorguDT(string sp)
        {
            if (cmd == null)
            {
                cmd = new SqlCommand();
            }
            cmd = new SqlCommand(sp, conn);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd = new SqlCommand(sp, conn);
            dap = new SqlDataAdapter(cmd);
            dt = new DataTable();
            dap.Fill(dt);
            cmd.Dispose();
            dap.Dispose();
            ClearParameters();
            return dt;
        }

        private DataTable dtS;
        public DataTable SPsorguDTS(string sp)
        {
            if (cmd == null)
            {
                cmd = new SqlCommand();
            }
            cmd = new SqlCommand(sp, conn);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd = new SqlCommand(sp, conn);
            dap = new SqlDataAdapter(cmd);
            dtS = new DataTable("dt");
            dap.Fill(dtS);
            cmd.Dispose();
            dap.Dispose();
            ClearParameters();
            return dtS;
        }

        private DataTable dtSPARAM;
        public DataTable SPsorguDTSPARAM(string sp, DateTime tarih, string doktorkod)
        {
            if (cmd == null)
            {
                cmd = new SqlCommand();
            }
            cmd = new SqlCommand(sp, conn);
            cmd.CommandType = CommandType.StoredProcedure;
            SqlParameter tarihprm = cmd.Parameters.AddWithValue("@GELISTARIHI", tarih);
            SqlParameter kodprm = cmd.Parameters.AddWithValue("@DOKTORKOD", doktorkod);
            tarihprm.SqlDbType = SqlDbType.DateTime;
            kodprm.SqlDbType = SqlDbType.NVarChar;

            cmd = new SqlCommand(sp, conn);
            dap = new SqlDataAdapter(cmd);
            dtSPARAM = new DataTable("dt");
            dap.Fill(dtSPARAM);
            cmd.Dispose();
            dap.Dispose();
            ClearParameters();
            return dtSPARAM;
        }
    }
}