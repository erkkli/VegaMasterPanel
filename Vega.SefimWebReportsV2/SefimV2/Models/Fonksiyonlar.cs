using System;
using System.Data;
using System.Data.OleDb;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.Configuration;
using System.Web.Mvc;

namespace SefimV2
{
    public class Fonksiyonlar
    {
        public string Server = WebConfigurationManager.AppSettings["Server"];
        private string User = WebConfigurationManager.AppSettings["User"];
        private string Password = WebConfigurationManager.AppSettings["Password"];
        private string dbname = WebConfigurationManager.AppSettings["DBName"];
        private string SqlConnString;
        public OleDbConnection ConnOle;
        private string[] SifreArray = { "Y6", "P8", "U3", "V1", "B7", "2M", "9N", "S5", "9V", "Z8" };

        public Fonksiyonlar()
        {
            SqlConnString = "Data Source=" + Server + ";Initial Catalog=" + dbname + ";Persist Security Info=True;User Id=" + User + ";Password=" + Password + "; MultipleActiveResultSets=true";
        }

        public string EncodeUrlString(string url)
        {
            return Uri.EscapeUriString(url).ToString();
        }
        public string DecodeUrlString(string url)
        {
            string newUrl;
            while ((newUrl = Uri.UnescapeDataString(url)) != url)
                url = newUrl;
            return newUrl;
        }

        public string Encrypt(string clearText)
        {
            string EncryptionKey = "mr3448";
            byte[] clearBytes = Encoding.Unicode.GetBytes(clearText);
            using (Aes encryptor = Aes.Create())
            {
                Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(EncryptionKey, new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 });
                encryptor.Key = pdb.GetBytes(32);
                encryptor.IV = pdb.GetBytes(16);
                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, encryptor.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(clearBytes, 0, clearBytes.Length);
                        cs.Close();
                    }
                    clearText = Convert.ToBase64String(ms.ToArray());
                }
            }
            return clearText;
        }
        public string Decrypt(string cipherText)
        {
            string EncryptionKey = "mr3448";
            cipherText = cipherText.Replace(" ", "+");
            byte[] cipherBytes = Convert.FromBase64String(cipherText);
            using (Aes encryptor = Aes.Create())
            {
                Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(EncryptionKey, new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 });
                encryptor.Key = pdb.GetBytes(32);
                encryptor.IV = pdb.GetBytes(16);
                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, encryptor.CreateDecryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(cipherBytes, 0, cipherBytes.Length);
                        cs.Close();
                    }
                    cipherText = Encoding.Unicode.GetString(ms.ToArray());
                }
            }
            return cipherText;
        }

        public string CC(string text)
        {
            string sonuc = "";
            if (text != null && text != "")
            {
                sonuc = text;
                sonuc = sonuc.ToLower();
                sonuc = sonuc.Trim();
                sonuc = sonuc.Replace(" ", "");
                sonuc = sonuc.Replace("ğ", "g");
                sonuc = sonuc.Replace("ü", "u");
                sonuc = sonuc.Replace("ş", "s");
                sonuc = sonuc.Replace("ı", "i");
                sonuc = sonuc.Replace("ö", "o");
                sonuc = sonuc.Replace("ç", "c");
                sonuc = sonuc.Replace("İ", "i");
                sonuc = sonuc.Replace("'", "");
                sonuc = sonuc.Replace("\"", "");
                sonuc = sonuc.Replace("!", "");
                sonuc = sonuc.Replace("&", "");
                sonuc = sonuc.Replace("^", "");
                sonuc = sonuc.Replace("+", "-");
                sonuc = sonuc.Replace("---", "-");
                sonuc = sonuc.Replace("--", "-");
                sonuc = sonuc.Replace("?", "-");
                sonuc = sonuc.Replace("/", "");
                sonuc = sonuc.Replace("\\", "");
                sonuc = sonuc.Replace("*", "");
                sonuc = sonuc.Replace("%", "");
                sonuc = sonuc.Replace("(", "");
                sonuc = sonuc.Replace(")", "");
                sonuc = sonuc.Replace("[", "");
                sonuc = sonuc.Replace("]", "");
                sonuc = sonuc.Replace("{", "");
                sonuc = sonuc.Replace("}", "");
                sonuc = sonuc.Replace("_", "-");
                sonuc = sonuc.Replace("|", "-");
                sonuc = sonuc.Replace("#", "hashtag");
                sonuc = sonuc.Replace("$", "usd");
                sonuc = sonuc.Replace("€", "euro");
                sonuc = sonuc.Replace("<", "");
                sonuc = sonuc.Replace(">", "");
                sonuc = sonuc.Replace(".", "-");
                sonuc = sonuc.Replace(",", "-");
                sonuc = sonuc.Replace(":", "-");
                sonuc = sonuc.Replace(";", "-");
                sonuc = sonuc.Replace("~", "");
                sonuc = sonuc.Replace("´", "");
                sonuc = sonuc.Replace("æ", "ae");
                sonuc = sonuc.Replace("ß", "ss");
            }
            return sonuc;
        }

        public DataTable DataTable(string komut)
        {
            DataTable dt = new DataTable();
            try
            {
                OleDbCommand command = new OleDbCommand(komut, ConnOle);
                dt.Load(command.ExecuteReader());
            }
            catch (System.Data.DataException ex) { }
            return dt;
        }

        public DataTable DataTablePage(string komut, int sayfa, int hersayfada)
        {
            DataTable dt = new DataTable();
            try
            {
                OleDbCommand command = new OleDbCommand(komut, ConnOle);
                dt.Load(command.ExecuteReader());
            }
            catch (System.Data.DataException ex) { }
            return dt;
        }

        public string TS(string str)
        {
            string sonuc = "";
            try
            {
                sonuc = str.ToString();
            }
            catch (System.Exception ex) { }
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

        public int RCI(DataRow r, string column)
        {
            int sonuc = 0;
            try
            {
                sonuc = Convert.ToInt32(r[column].ToString());
            }
            catch (System.Exception ex) { }
            return sonuc;
        }

        public void SqlConnOpen()
        {
            try
            {
                
                ConnOle = new OleDbConnection(SqlConnString);
                ConnOle.Open();
            }
            catch (System.Data.DataException ex) { }
        }

        public string SqlConnTestReturnString()
        {
            string result = "true";
            try
            {
                ConnOle = new OleDbConnection(SqlConnString);
                ConnOle.Open();
                ConnOle.Close();
            }
            catch (Exception ex) { result = ex.Message.ToString(); }
            return result;
        }

        public void SqlConnClose()
        {
            try
            {
                ConnOle.Close();
            }
            catch (System.Data.DataException ex) { }
        }

        public void SQLQuery(string komut)
        {
            try
            {
                OleDbCommand command = new OleDbCommand(komut, ConnOle);
                command.ExecuteNonQuery();
            }
            catch (System.Data.DataException ex) { }
        }

        public bool SQLQueryReturnBool(string komut)
        {
            bool sonuc = true;
            try
            {
                OleDbCommand command = new OleDbCommand(komut, ConnOle);
                command.ExecuteNonQuery();
            }
            catch (System.Data.DataException ex) { sonuc = false; }
            return sonuc;
        }

        public string SQLQueryReturnId(string komut)
        {
            string id = "";
            try
            {
                OleDbCommand command = new OleDbCommand(komut, ConnOle);
                OleDbDataReader reader = command.ExecuteReader();
                while (reader.Read()) { id = reader["id"].ToString(); }
            }
            catch (System.Data.DataException ex) { }
            return id;
        }

        public string SQLQueryReturnString(string komut)
        {
            string sonuc = "false";
            try
            {
                OleDbCommand command = new OleDbCommand(komut, ConnOle);
                command.ExecuteNonQuery();
                sonuc = "true";
            }
            catch (System.Data.DataException ex) { sonuc = ex.Message.ToString(); }
            return sonuc;
        }

        public int SQLQueryRowCount(string komut)
        {
            int sonuc = 0;
            komut = "select count(*) as toplam from " + komut;
            try
            {
                OleDbCommand command = new OleDbCommand(komut, ConnOle);
                OleDbDataReader reader = command.ExecuteReader();
                while (reader.Read()) { sonuc = Convert.ToInt32(reader["toplam"].ToString()); }
            }
            catch (System.Data.DataException ex) { }
            return sonuc;
        }

        public int PageNumberFinder(WebViewPage v)
        {
            int sonuc = 1;
            try { sonuc = Convert.ToInt32(v.ViewContext.RouteData.Values["page"].ToString()); }
            catch (Exception ex) { }
            return sonuc;
        }

        public string Sifrele(string veri)
        {
            string sifrelenmisVeri = "";
            try
            {
                Random rnd = new Random();
                string Son = SifreArray[rnd.Next(0, 9)];
                byte[] veriByteDizisi = System.Text.Encoding.Unicode.GetBytes(veri);
                sifrelenmisVeri = System.Convert.ToBase64String(veriByteDizisi);
                sifrelenmisVeri = sifrelenmisVeri.Replace(@"=", @"N" + Son + "");
            }
            catch { }
            return sifrelenmisVeri;
        }

        public string SifreleSabit(string veri)
        {
            string sifrelenmisVeri = "";
            try
            {
                Random rnd = new Random();
                string Son = "V1";
                byte[] veriByteDizisi = System.Text.Encoding.Unicode.GetBytes(veri);
                sifrelenmisVeri = System.Convert.ToBase64String(veriByteDizisi);
                sifrelenmisVeri = sifrelenmisVeri.Replace(@"=", @"N" + Son + "");
            }
            catch { }
            return sifrelenmisVeri;
        }

        public string SifreCoz(string cozVeri)
        {
            string orjinalVeri = "";
            try
            {
                for (int i = 0; i < SifreArray.Length; i++)
                {
                    if (cozVeri.IndexOf("N" + SifreArray[i] + "") > -1)
                    {
                        cozVeri = cozVeri.Replace("N" + SifreArray[i] + "", "=");
                    }
                }

                byte[] cozByteDizi = System.Convert.FromBase64String(cozVeri);
                orjinalVeri = System.Text.Encoding.Unicode.GetString(cozByteDizi);
            }
            catch { }
            return orjinalVeri;
        }

        public string RastgeleSayi(int max)
        {
            Random rastgele = new Random();

            string harfler = "1234567890";
            string uret = "";
            for (int i = 0; i < max; i++)
            {
                uret += harfler[rastgele.Next(harfler.Length)];
            }
            return uret;
        }

        public string RastgeleKod(int max)
        {
            Random rastgele = new Random();

            string harfler = "ABCDEFGHIJKQLMNOPRSTUVWYZabcdefghijkqlmnoprstuvwyz1234567890";
            string uret = "";
            for (int i = 0; i < max; i++)
            {
                uret += harfler[rastgele.Next(harfler.Length)];
            }
            return uret;
        }

        public string RastgeleUpp(int max)
        {
            Random rastgele = new Random();

            string harfler = "ABCDEFGHIJKQLMNOPRSTUVWYZ1234567890";
            string uret = "";
            for (int i = 0; i < max; i++)
            {
                uret += harfler[rastgele.Next(harfler.Length)];
            }
            return uret;
        }

        public string RastgeleHarf(int max)
        {
            Random rastgele = new Random();

            string harfler = "ABCDEFGHIJKQLMNOPRSTUVWYZabcdefghijkqlmnoprstuvwyz";
            string uret = "";
            for (int i = 0; i < max; i++)
            {
                uret += harfler[rastgele.Next(harfler.Length)];
            }
            return uret;
        }

        public string RastgeleHarfUpp(int max)
        {
            Random rastgele = new Random();

            string harfler = "ABCDEFGHIJKQLMNOPRSTUVWYZ";
            string uret = "";
            for (int i = 0; i < max; i++)
            {
                uret += harfler[rastgele.Next(harfler.Length)];
            }
            return uret;
        }

        public string NewTempUserId()
        {
            return DateTime.Now.ToString("ddMMyyyymmHHssfff") + "-" + RastgeleUpp(5);
        }

        public string SqlInjectionKontrol(string value)
        {
            if (value != null && value != String.Empty && value != "")
            {
                value = value.Trim();
                value = value.Replace("'", "");
                value = value.Replace("\"", "");
                value = value.Replace("=", "");
                string checkvalue = value.ToLower();
                string[] strList = checkvalue.Split(' ');
                string[] strDangerList = { "where ", "select ", "from ", "delete ", "drop ", "alter table ", "table ", "insert into ", "update ", "set ", "join ", "script ", "body ", "alert ", "insert ", "or ", "and " };

                for (int i = 0; i < strList.Length; i++)
                {
                    for (int j = 0; j < strDangerList.Length; j++)
                    {
                        if (strList[i].ToString() == strDangerList[j])
                        {
                            value = Regex.Replace(checkvalue, "" + strDangerList[j] + "", "");
                        }
                    }
                }
            }

            return value;
        }

        public bool SqlInjectionKontrolBool(string value)
        {
            bool sonuc = true;
            if (value.IndexOf("'") > -1) { sonuc = false; };
            if (value.IndexOf("\"") > -1) { sonuc = false; };
            if (value.IndexOf("=") > -1) { sonuc = false; };
            string checkvalue = value.ToLower();
            string[] strList = checkvalue.Split(' ');
            string[] strDangerList = { "where", "select", "from", "delete", "drop", "alter table", "table", "insert into", "update", "set", "join", "script", "body", "alert", "insert", "or", "and" };

            for (int i = 0; i < strList.Length; i++)
            {
                for (int j = 0; j < strDangerList.Length; j++)
                {
                    if (strList[i].ToString() == strDangerList[j]) { sonuc = false; }
                }
            }
            value = Regex.Replace(checkvalue, " ", "");
            return sonuc;
        }

        public bool MailValidateReturnBool(string email)
        {
            bool sonuc = false;
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                sonuc = addr.Address == email;
            }
            catch (System.Exception ex) { }
            return sonuc;
        }

        public string GsmFormat(string gsm)
        {
            string sonuc = gsm;
            try
            {
                sonuc = sonuc.Replace("(", "");
                sonuc = sonuc.Replace(")", "");
                sonuc = sonuc.Replace(" ", "");
                sonuc = sonuc.Replace("-", "");
                sonuc = sonuc.Replace("_", "");
                sonuc = sonuc.Trim();
                if (Left(sonuc, 1) == "0") { sonuc = Right(sonuc, sonuc.Length - 1); }
            }
            catch { }
            return sonuc;
        }

        public bool GsmValidateReturnBool(string gsm)
        {
            bool sonuc = false;
            try { if (Convert.ToInt64(gsm).ToString().Length == 10) { sonuc = true; } } catch (System.Exception ex) { }
            return sonuc;
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

        public string BSE(string val)
        {
            string sonuc = val;
            if (sonuc.Length < 2) { sonuc = "0" + sonuc; }
            return sonuc;
        }

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

        public string Left(string val, int say)
        {
            string sonuc = "";
            if (say > val.Length) { say = val.Length; }
            sonuc = val.Substring(0, say);
            return sonuc;
        }

        public string Right(string str, int length)
        {
            str = (str ?? string.Empty);
            return (str.Length >= length)
                ? str.Substring(str.Length - length, length)
                : str;
        }

        public string NullControl(string val)
        {
            string sonuc = "";
            if (val == null || val == "") { sonuc = ""; }
            else
            {
                sonuc = val.ToString();
            }
            return sonuc;
        }

        public int DefaultEveryPage(string everypage)
        {
            int sonuc = 25;
            try { int s = Convert.ToInt32(everypage); if (s < 1) { s = 25; } sonuc = s; }
            catch (System.Exception ex) { }
            return sonuc;
        }

        public int DefaultPageIndex(string page)
        {
            int sonuc = 0;
            try { int s = Convert.ToInt32(page); s = s - 1; if (s < 1) { s = 0; } sonuc = s; }
            catch (System.Exception ex) { }
            return sonuc;
        }

        public int PageCountFind(int count, int everypage)
        {
            int sonuc = 1;
            try
            {
                int c = Convert.ToInt32(count); int e = DefaultEveryPage(everypage.ToString());
                sonuc = Convert.ToInt32(c / everypage); if (c % everypage > 0) { sonuc = sonuc + 1; }
            }
            catch (System.Exception ex) { }
            return sonuc;
        }

        public string DefaultSort(string sort)
        {
            string sonuc = "id";
            if (sort != "" && sort != null) { sonuc = sort; }
            return sonuc;
        }

        public string DefaultSortType(string sorttype)
        {
            string sonuc = "desc";
            if (sorttype != "" && sorttype != null) { sonuc = sorttype; }
            return sonuc;
        }
    }
}