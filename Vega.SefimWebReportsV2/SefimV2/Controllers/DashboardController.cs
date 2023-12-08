using SefimV2.Helper;
using SefimV2.Models;
using SefimV2.Repository;
using SefimV2.ViewModels.GetTime;
using SefimV2.ViewModels.Result;
using System;
using System.Web;
using System.Web.Configuration;
using System.Web.Mvc;

namespace SefimV2.Controllers
{
    [CheckLoggedIn]
    public class DashboardController : Controller
    {
        // GET: Dashboard
        public ActionResult Index()
        {
            try
            {
                #region YETKİLİ ID (Left Menude filtrele de yapıyorum)

                bool cookieExists = HttpContext.Request.Cookies["PRAUT"] != null;
                if (cookieExists == false)
                {
                    try
                    {
                        HttpCookie currentUserCookie = HttpContext.Request.Cookies["VGMSTR"];
                        HttpContext.Response.Cookies.Remove("VGMSTR");
                        currentUserCookie.Expires = DateTime.Now.AddDays(-1);
                        currentUserCookie.Value = null;
                        HttpContext.Response.SetCookie(currentUserCookie);
                    }
                    catch (Exception) { }

                    return Redirect("/Authentication/Login");
                }
                string ID = Request.Cookies["PRAUT"].Value;
                ViewBag.YetkiliID = ID;

                var kullaniciData = BussinessHelper.GetByUserInfoForId("", "", ID);
                ViewBag.SubeSirasiGorunsunMu = kullaniciData.SubeSirasiGorunsunMu;
                ViewBag.KullaniciUygulamaTipi = kullaniciData.UygulamaTipi;

                //uygulama tipini Şefim Panel veya Vega Master mı anlamak için set edildi.
                string uygulamaTipi = WebConfigurationManager.AppSettings["UygulamaTipi"];
                ViewBag.UygulamTipi = uygulamaTipi;

                #endregion YETKİLİ ID (Left Menude filtrele de yapıyorum)

                #region DateFilter 

                Saat saat = Ayarlar.SaatListe();
                string gun = (DateTime.Today.ToString("dd"));
                string ay = DateTime.Today.ToString("MM");
                string yil = DateTime.Today.ToString("yyyy");
                string gunSonu_ = (DateTime.Today.AddDays(1).ToString("dd")); //Convert.ToInt32(gun) + 1;
                string a1 = DateTime.Now.AddDays(1).ToShortDateString();
                string gunSonuGun = Convert.ToDateTime(a1).ToString("dd");
                string gunSonuAy = Convert.ToDateTime(a1).ToString("MM");
                string gunsonuYil = Convert.ToDateTime(a1).ToString("yyyy");
                string bugun = yil + "-" + ay + "-" + gun + " " + saat.StartTime;//date2h.ToString() ;  //Convert.ToDateTime((DateTime.Today.ToShortDateString() + " " + saat.StartTime)).ToString("yyyy-MM-dd HH:mm");
                string gunSonu = gunsonuYil + "-" + gunSonuAy + "-" + gunSonuGun + " " + saat.EndTime;//Convert.ToDateTime((DateTime.Now.AddDays(1).ToShortDateString() + " " + saat.EndTime)).ToString("yyyy-MM-dd HH:mm");

                #region Yeni Tarih Filtreleme 

                //ViewBag ile tarihler taşınıyor
                DateTime GelenGun = new DateTime();
                ViewBag.Bugun = bugun;
                ViewBag.GunSonu = gunSonu;

                DateTime now = DateTime.Now;
                var startDate = new DateTime(now.Year, now.Month, now.Day).ToString();
                var oncekiGun = now.AddDays(-1).ToString("yyyy-MM-dd") + " " + saat.StartTime;
                var oncekiGunSonu = now.AddDays(-1).ToString("yyyy-MM-dd") + " " + saat.EndTime;
                ViewBag.oncekiGun = oncekiGun;
                ViewBag.oncekiGunSonu = oncekiGunSonu;

                #endregion Yeni Tarih Filtreleme 

                string durum = string.Empty;
                if (Request.QueryString["durum"] != null)
                    durum = Request.QueryString["durum"].ToString();
                if (durum == "")
                {
                    durum = "bugun";
                }

                if (durum.Equals("bugun"))
                {
                    ViewBag.Bugun = bugun;
                    ViewBag.GunSonu = gunSonu;
                }
                if (durum.Equals("dun"))
                {
                    #region Yeni Tarih Filtreleme

                    bugun = now.AddDays(-1).ToString("yyyy-MM-dd") + " " + saat.StartTime;
                    gunSonu = now.AddDays(0).ToString("yyyy-MM-dd") + " " + saat.EndTime;
                    ViewBag.Bugun = bugun;
                    ViewBag.GunSonu = gunSonu;
                    //DateTime now = DateTime.Now;
                    //startDate = new DateTime(now.Year, now.Month, now.Day).ToString();
                    oncekiGun = now.AddDays(-2).ToString("yyyy-MM-dd") + " " + saat.StartTime;
                    oncekiGunSonu = now.AddDays(-1).ToString("yyyy-MM-dd") + " " + saat.EndTime;
                    ViewBag.oncekiGun = oncekiGun;
                    ViewBag.oncekiGunSonu = oncekiGunSonu;

                    #endregion Yeni Tarih Filtreleme
                }
                if (durum.Equals("buAy"))
                {
                    #region Yeni Tarih filtreleme

                    //DateTime now = DateTime.Now;
                    var startDate11 = new DateTime(now.Year, now.Month, 1);
                    var endDate11 = startDate11.AddMonths(1).AddDays(-1);
                    bugun = startDate11.ToString("yyyy-MM-dd") + " " + saat.StartTime;
                    gunSonu = endDate11.ToString("yyyy-MM-dd") + " " + saat.EndTime;
                    ViewBag.Bugun = bugun;
                    ViewBag.GunSonu = gunSonu;

                    #endregion Yeni Tarih filtreleme

                    //GelenGun = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
                    //GelenGun = GelenGun.AddHours(Convert.ToInt32(saat.StartTime.Split(':')[0]));
                    //GelenGun = GelenGun.AddMinutes(Convert.ToInt32(saat.StartTime.Split(':')[1]));
                    //GelenGunSonu = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
                    //GelenGunSonu = GelenGunSonu.AddHours(Convert.ToInt32(saat.EndTime.Split(':')[0]));
                    //GelenGunSonu = GelenGunSonu.AddMinutes(Convert.ToInt32(saat.EndTime.Split(':')[1]));
                    //GelenGunSonu = GelenGunSonu.AddMonths(1);
                }
                if (durum.Equals("tarihAraligi"))
                {
                    if (Request.QueryString["tarihBas"] != null && Request.QueryString["tarihBitis"] != null)
                    {
                        bugun = (Convert.ToDateTime(Request.QueryString["tarihBas"].ToString())).ToString();
                        gunSonu = (Convert.ToDateTime(Request.QueryString["tarihBitis"].ToString())).ToString();
                        ViewBag.Bugun = bugun;
                        ViewBag.GunSonu = gunSonu;
                    }
                }

                #endregion DateFilter   

                ViewBag.Gun = GelenGun.Day.ToString();
                ViewBag.LimitSize = 5;
            }
            catch (Exception ex)
            {
                Singleton.WritingLogFile2("DashboardController", ex.ToString(), null, ex.StackTrace);
            }

            return View();
        }


        #region  Dashboard Oranlari
        [HttpGet]
        public JsonResult DasboardOranlari(string StartDate, string EndDate)
        {
            var model = new SubeCiro();
            string ID = Request.Cookies["PRAUT"].Value;

            var result = new JsonResultModel
            {
                IsSuccess = false
            };

            #region Secilen Tarih Filterelemesine Göre Yapılan İşlemler 

            string tarihAraligiStartDate = string.Empty;
            string tarihAraligiEndDate = string.Empty;
            string subeid = string.Empty;
            string durum = string.Empty;

            try
            {
                if (Request.QueryString["durum"] != null)
                    durum = Request.QueryString["durum"].ToString();

                if (Request.QueryString["tarihBas"] != null && Request.QueryString["tarihBitis"] != null)
                {
                    tarihAraligiStartDate = Request.QueryString["tarihBas"].ToString();
                    tarihAraligiEndDate = Request.QueryString["tarihBitis"].ToString();
                }

                if (Request.QueryString["subeid"] != null)
                {
                    subeid = Request.QueryString["subeid"].ToString();
                }
            }
            catch (Exception ex) { Singleton.WritingLogFile("SubeUrunlerController_Details", ex.Message.ToString()); }

            TimeViewModel viewModel = Singleton.GetTimeViewModel(tarihAraligiStartDate, tarihAraligiEndDate, durum);

            #endregion Secilen Tarih Filterelemesine Göre Yapılan İşlemler   

            model = DashboardListCRUD.DashboardList_(Convert.ToDateTime(StartDate), Convert.ToDateTime(EndDate), ID);

            //result = new DashboardListCRUD().DashboardList(model);
            //List<SubeCiro> SubeCiro = ChartsReportsSubeCiroCRUD.List(Convert.ToDateTime("2019-12-01 00:00:00"), Convert.ToDateTime("2019-12-31 00:00:00"), Id);
            //foreach (var item in SubeCiro)
            //{
            //    //item.ToplamCiro += model.ToplamCiro;
            //    //item.ToplamCash += model.ToplamCash;
            //    model.ToplamCash += item.ToplamCash;
            //    model.ToplamCiro += item.ToplamCiro;
            //}
            return Json(model, JsonRequestBehavior.AllowGet);
        }
        #endregion Dashboard Oranlari
    }
}