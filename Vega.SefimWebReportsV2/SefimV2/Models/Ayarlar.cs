using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace SefimV2.Models
{
    public class Ayarlar
    {

        #region TIME LIST
        
         public static Saat SaatListe()
        {
            Saat saat = new Saat();
            ModelFunctions f = new ModelFunctions();
            DateTime startDate = DateTime.Now;
            try
            {
                f.SqlConnOpen();
                DataTable dt = f.DataTable("select * from UserTimeLine");

                foreach (DataRow r in dt.Rows)
                {
                    saat.StartTime = f.RTS(r, "StartTime");
                    saat.EndTime = f.RTS(r, "EndTime");

                }
                f.SqlConnClose();
            }
            catch (System.Exception ex)
            {
            }

            return saat;
        }

        #endregion TIME LIST

        #region TIME UPDATE
        
        internal mdlActionResult SaatAyariGuncelle(Saat saat)
        {
            ModelFunctions f = new ModelFunctions();
            mdlActionResult result = new mdlActionResult();
            result.result_STR = "Başarısız";
            result.result_INT = 0;
            result.result_BOOL = false;
            result.result_OBJECT = null;
            string resut = "true";
            try
            {
                f.SqlConnOpen();
                string TimeStamp = DateTime.Now.ToFileTimeUtc().ToString();
                string CmdString = "Update UserTimeLine Set " +
                "UpdateDate=getdate() , " +
                "UpdateDate_Timestamp='" + TimeStamp + "' , " +
                "StartTime='" + saat.StartTime + "' , " +
                "EndTime='" + saat.EndTime + "'  ";
                OleDbCommand Cmd = new OleDbCommand(CmdString, f.ConnOle);
                Cmd.ExecuteNonQuery();
                f.SqlConnClose();
            }
            catch (OleDbException ex) { resut = ex.Message.ToString(); }
            result.result_STR = "Başarılı";
            result.result_INT = 1;
            result.result_BOOL = true;
            result.result_OBJECT = SaatListe();
            return result;
        }

        #endregion TIME UPDATE

        #region TIME INSERT
       
        public mdlActionResult SaatAyariEkle(Saat saat)
        {

            ModelFunctions f = new ModelFunctions();
            mdlActionResult result = new mdlActionResult();
            result.result_STR = "Başarısız";
            result.result_INT = 0;
            result.result_BOOL = false;
            result.result_OBJECT = null;
            string resut = "true";
            try
            {
                f.SqlConnOpen();
                string TimeStamp = DateTime.Now.ToFileTimeUtc().ToString();
                string CmdString = "Insert Into UserTimeLine(CreateDate,CreateDate_Timestamp,StartTime,EndTime)" +
                    "Values(" +
                    "getdate() , " +
                    "'" + TimeStamp + "' , " +
                    "'" + saat.StartTime + "' , " +
                    "'" + saat.EndTime + "' " + ")";
                OleDbCommand Cmd = new OleDbCommand(CmdString, f.ConnOle);
                Cmd.ExecuteNonQuery();
                f.SqlConnClose();
            }
            catch (OleDbException ex) { resut = ex.Message.ToString(); }
            result.result_STR = "Başarılı";
            result.result_INT = 1;
            result.result_BOOL = true;
            result.result_OBJECT = SaatListe();
            return result;
        }

        #endregion TIME INSERT


        #region GET USER LOGIN       
        internal static mdlAdmin GetUserInfo(string userName, string password)
        {
            mdlAdmin mdlAdmin = new mdlAdmin();
            ModelFunctions f = new ModelFunctions();
            string filter = " where 1=1";
            filter += " and UserName='" + userName + "' and Password='" + password + "'";
            DateTime startDate = DateTime.Now;
            try
            {
                f.SqlConnOpen();
                DataTable dt = f.DataTable("select * from Users" + filter);

                foreach (DataRow r in dt.Rows)
                {
                    mdlAdmin.UserName = f.RTS(r, "UserName");
                    mdlAdmin.Password = f.RTS(r, "Password");
                    mdlAdmin.ID = Convert.ToInt32(f.RTS(r, "ID"));
                }
                f.SqlConnClose();

                #region KULLANICI ID ALMAK İÇİN            
                HttpCookie cookie = new HttpCookie("PRAUT", mdlAdmin.ID.ToString());
                //cookie.Expires = DateTime.Now.AddMinutes(minute);
                HttpContext.Current.Response.Cookies.Add(cookie);
                //HttpContext.Response.Cookies.Add(cookie);            

                #region Başarılı Girişler için bir Giriş Çerezi ("PRAUT") Oluşturuluyor; Kullanıcı Adı ve SessionID Buraya Şifrelenerek Yazılıyor ve Buna Ömür Atanıyor
                //HttpCookie cookie = new HttpCookie("PRAUT", mdlAdmin.ID.ToString());
                //string SessionID = Guid.NewGuid().ToString().ToUpper();
                //cookie["Ukey"] = OEncoder.OEncode(mdlAdmin.ID.ToString());
                //cookie["SessionID"] = OEncoder.OEncode(SessionID);
                //cookie.Expires = DateTime.Now.AddMinutes(120);
                //HttpContext.Current.Response.Cookies.Add(cookie);
                #endregion
                #endregion
            }
            catch (System.Exception ex) { }

            return mdlAdmin;
        }
        #endregion GET USER LOGIN
    }
}