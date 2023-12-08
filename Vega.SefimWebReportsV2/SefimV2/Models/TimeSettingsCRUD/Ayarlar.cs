using SefimV2.Helper;
using System;
using System.Data;
using System.Data.OleDb;
using System.Web;

namespace SefimV2.Models
{
    public class Ayarlar
    {
        //TODO Uygulama Tipi       
        //private static readonly string uygulamaTipi = WebConfigurationManager.AppSettings["UygulamaTipi"];

        #region GET USER LOGIN       
        internal static AdminViewModel GetUserInfo(string userName, string password, int productId)
        {
            var ipAdres = new BussinessHelper().GetIp();
            Singleton.WritingLog("LoginAction", "Ip Adres:" + ipAdres + " User Name:" + userName);

            var model = new AdminViewModel();
            var mf = new ModelFunctions();
            string filter = " where 1=1";
            filter += " and UserName='" + userName + "' and Password='" + password + "'";

            try
            {
                mf.SqlConnOpen();
                DataTable dt = mf.DataTable("Select * From Users" + filter);
                mf.SqlConnClose();

                foreach (DataRow r in dt.Rows)
                {
                    model.UserName = mf.RTS(r, "UserName");
                    model.Password = mf.RTS(r, "Password");
                    model.UygulamaTipi = mf.RTS(r, "UygulmaTipi").ToString();
                    model.ID = Convert.ToInt32(mf.RTS(r, "Id"));
                }

                #region KULLANICI ID ALMAK İÇİN            
                //HttpCookie cookie = new HttpCookie("PRAUT", mdlAdmin.Id.ToString());
                ////cookie.Expires = DateTime.Now.AddMinutes(minute);
                //cookie.Expires = DateTime.Now.AddYears(2);
                //HttpContext.Current.Response.Cookies.Add(cookie);
                ////HttpContext.Response.Cookies.Add(cookie);                
                //System.Web.HttpContext.Current.Response.Cookies.Add(cookie);

                //Ürün Tipini Tutar.
                HttpCookie cookieProductId = new HttpCookie("PRTYPE", productId.ToString())
                {
                    Expires = DateTime.Now.AddMinutes(120)
                };
                HttpContext.Current.Response.Cookies.Add(cookieProductId);

                //Kişiye Atanmış Uygulama Tipini Tutar.
                HttpCookie cookieApptype = new HttpCookie("ATYPE", model.UygulamaTipi.ToString())
                {
                    Expires = DateTime.Now.AddMinutes(120)
                };
                HttpContext.Current.Response.Cookies.Add(cookieApptype);

                HttpCookie cookie = new HttpCookie("PRAUT", model.ID.ToString())
                {
                    Expires = DateTime.Now.AddMinutes(119)
                };
                System.Web.HttpContext.Current.Response.Cookies.Add(cookie);

                HttpCookie cookieVM = new HttpCookie("VGMSTRS", model.ID.ToString())
                {
                    Expires = DateTime.Now.AddMinutes(119)
                };
                System.Web.HttpContext.Current.Response.Cookies.Add(cookieVM);


                #region Başarılı Girişler için bir Giriş Çerezi ("PRAUT") Oluşturuluyor; Kullanıcı Adı ve SessionID Buraya Şifrelenerek Yazılıyor ve Buna Ömür Atanıyor
                //HttpCookie cookie = new HttpCookie("PRAUT", mdlAdmin.Id.ToString());
                //string SessionID = Guid.NewGuid().ToString().ToUpper();
                //cookie["Ukey"] = OEncoder.OEncode(mdlAdmin.Id.ToString());
                //cookie["SessionID"] = OEncoder.OEncode(SessionID);
                //cookie.Expires = DateTime.Now.AddMinutes(120);
                //HttpContext.Current.Response.Cookies.Add(cookie);
                #endregion Başarılı Girişler için bir Giriş Çerezi ("PRAUT") Oluşturuluyor; Kullanıcı Adı ve SessionID Buraya Şifrelenerek Yazılıyor ve Buna Ömür Atanıyor
                //var kullaniciSubeYetkisi = UsersListCRUD.KullaniciSubeYetkiListesi(model.ID.ToString());
                //HttpContext.Current.Session["UsersBranchList"] = kullaniciSubeYetkisi;
                //HttpContext.Current.Session["KullaniciId"] = model.ID;

                #endregion KULLANICI ID ALMAK İÇİN  
            }
            catch (Exception ex)
            {
                Singleton.WritingLogFile("GetUserInfo KullanıcıAdi:" + userName, ex.Message.ToString());
            }

            return model;
        }
        #endregion GET USER LOGIN

        #region TIME LIST        
        public static Saat SaatListe()
        {
            Saat saat = new Saat();
            ModelFunctions mf = new ModelFunctions();
            DateTime startDate = DateTime.Now;
            try
            {
                mf.SqlConnOpen();
                DataTable dt = mf.DataTable("select * from UserTimeLine");
                mf.SqlConnClose();

                foreach (DataRow r in dt.Rows)
                {
                    saat.StartTime = mf.RTS(r, "StartTime");
                    saat.EndTime = mf.RTS(r, "EndTime");
                }
            }
            catch (Exception ex)
            { Singleton.WritingLogFile("AyarlarSaatListe:", ex.Message.ToString()); }

            return saat;
        }
        #endregion TIME LIST

        #region TIME UPDATE        
        internal ActionResultMessages SaatAyariGuncelle(Saat saat)
        {
            ModelFunctions mf = new ModelFunctions();
            ActionResultMessages result = new ActionResultMessages
            {
                result_STR = "Başarısız",
                result_INT = 0,
                result_BOOL = false,
                result_OBJECT = null
            };
            string resut = "true";

            try
            {
                mf.SqlConnOpen();
                string TimeStamp = DateTime.Now.ToFileTimeUtc().ToString();
                string CmdString = "Update UserTimeLine Set " +
                                   "UpdateDate=getdate() , " +
                                   "UpdateDate_Timestamp='" + TimeStamp + "' , " +
                                   "StartTime='" + saat.StartTime + "' , " +
                                   "EndTime='" + saat.EndTime + "'  ";
                OleDbCommand Cmd = new OleDbCommand(CmdString, mf.ConnOle);
                Cmd.ExecuteNonQuery();
                mf.SqlConnClose();
            }
            catch (OleDbException ex)
            {
                Singleton.WritingLogFile("AyarlarSaatAyariGuncelle:", ex.Message.ToString());
                resut = ex.Message.ToString();
            }

            result.result_STR = "Başarılı";
            result.result_INT = 1;
            result.result_BOOL = true;
            result.result_OBJECT = SaatListe();

            return result;
        }
        #endregion TIME UPDATE

        #region TIME INSERT       
        public ActionResultMessages SaatAyariEkle(Saat saat)
        {
            ModelFunctions mf = new ModelFunctions();
            ActionResultMessages result = new ActionResultMessages
            {
                result_STR = "Başarısız",
                result_INT = 0,
                result_BOOL = false,
                result_OBJECT = null
            };

            string resut = "true";
            try
            {
                mf.SqlConnOpen();
                string TimeStamp = DateTime.Now.ToFileTimeUtc().ToString();
                string CmdString = "Insert Into UserTimeLine(CreateDate,CreateDate_Timestamp,StartTime,EndTime)" +
                                    "Values(" +
                                    "getdate() , " +
                                    "'" + TimeStamp + "' , " +
                                    "'" + saat.StartTime + "' , " +
                                    "'" + saat.EndTime + "' " + ")";
                OleDbCommand Cmd = new OleDbCommand(CmdString, mf.ConnOle);
                Cmd.ExecuteNonQuery();
                mf.SqlConnClose();
            }
            catch (OleDbException ex)
            {
                Singleton.WritingLogFile("AyarlarSaatAyariEkle:", ex.Message.ToString());
                resut = ex.Message.ToString();
            }
            result.result_STR = "Başarılı";
            result.result_INT = 1;
            result.result_BOOL = true;
            result.result_OBJECT = SaatListe();

            return result;
        }
        #endregion TIME INSERT      
    }
}