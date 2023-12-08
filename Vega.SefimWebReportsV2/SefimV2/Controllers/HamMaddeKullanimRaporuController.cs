using SefimV2.Helper;
using SefimV2.Models;
using SefimV2.Repository;
using SefimV2.ViewModels.GetTime;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using System.Web.Mvc;

namespace SefimV2.Controllers
{
    [CheckLoggedIn]
    public class HamMaddeKullanimRaporuController : Controller
    {
        // GET: HamMaddeKullanimRaporu
        public ActionResult Index()
        {
            return View();
        }

        #region LIST       
        public ActionResult List()
        {
            List<HamMaddeKullanimViewModel> list;
            string durum = string.Empty;
            string subeid = string.Empty;
            string SendEmail = string.Empty;
            string tarihAraligiEndDate = string.Empty;
            string tarihAraligiStartDate = string.Empty;
            try
            {
                #region YETKİLİ ID (Left Menude filtrele de yapıyorum)    
                bool cookieExists = HttpContext.Request.Cookies["PRAUT"] != null;
                if (cookieExists == false)
                {
                    return Redirect("/Authentication/Login");
                }
                string ID = Request.Cookies["PRAUT"].Value;
                ViewBag.YetkiliID = ID;
                #endregion

                #region Secilen Tarih Filterelemesine Göre Yapılan İşlemler           

                try
                {
                    if (Request.QueryString["SendEmail"] != null)
                        SendEmail = Request.QueryString["SendEmail"].ToString();

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
                    if (SendEmail == "Email")
                    {
                    }
                }
                catch (Exception ex)
                {
                    Singleton.WritingLogFile("HamMaddeKullanimRaporuController_List", ex.Message.ToString());
                }

                TimeViewModel viewModel = Singleton.GetTimeViewModel(tarihAraligiStartDate, tarihAraligiEndDate, durum);

                viewModel.StartDate = Convert.ToDateTime(viewModel.StartDate).ToString("yyyy-MM-dd") + " " + "00:00";
                viewModel.EndDate = Convert.ToDateTime(viewModel.EndDate).ToString("yyyy-MM-dd") + " " + "00:00";

                ViewBag.StartDateTime = viewModel.StartDate;
                ViewBag.EndDateTime = viewModel.EndDate;
                ViewBag.Pages = "Hammadde Kullanım ve Durum Raporu";
                ViewBag.PageNavi = "Hammadde Kullanım ve Durum Raporu";
                #endregion

                list = HamMaddeKullanimCRUD.List(Convert.ToDateTime(viewModel.StartDate), Convert.ToDateTime(viewModel.EndDate), "", "SubeList");
            }
            catch (Exception ex)
            {
                Singleton.WritingLogFile2("HamMaddeKullanimRaporuController", ex.ToString(), " :BaslangicTar:" + Convert.ToDateTime(tarihAraligiStartDate.ToString()).ToString("dd'/'MM'/'yyyy HH:mm") + " :BitisTar:" + Convert.ToDateTime(tarihAraligiEndDate.ToString()).ToString("dd'/'MM'/'yyyy HH:mm"), ex.StackTrace);
                return Redirect("/Authentication/Login");
            }
            return View(list);
        }
        #endregion LIST

        #region Details       
        public ActionResult Details()
        {
            List<HamMaddeKullanimViewModel> list;
            string durum = string.Empty;
            string subeid = string.Empty;
            string SendEmail = string.Empty;
            string tarihAraligiEndDate = string.Empty;
            string tarihAraligiStartDate = string.Empty;

            try
            {
                #region YETKİLİ ID (Left Menude filtrele de yapıyorum)    
                bool cookieExists = HttpContext.Request.Cookies["PRAUT"] != null;
                if (cookieExists == false)
                {
                    return Redirect("/Authentication/Login");
                }
                string ID = Request.Cookies["PRAUT"].Value;
                ViewBag.YetkiliID = ID;
                #endregion

                #region Secilen Tarih Filterelemesine Göre Yapılan İşlemler           

                try
                {
                    if (Request.QueryString["SendEmail"] != null)
                        SendEmail = Request.QueryString["SendEmail"].ToString();

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
                    if (SendEmail == "Email")
                    { }
                }
                catch (Exception ex)
                {
                    Singleton.WritingLogFile("HamMaddeKullanimRaporuController_Details", ex.Message.ToString());
                }

                TimeViewModel viewModel = Singleton.GetTimeViewModel(tarihAraligiStartDate, tarihAraligiEndDate, durum);
                viewModel.StartDate = Convert.ToDateTime(viewModel.StartDate).ToString("yyyy-MM-dd") + " " + "00:00";
                viewModel.EndDate = Convert.ToDateTime(viewModel.EndDate).ToString("yyyy-MM-dd") + " " + "00:00";

                ViewBag.StartDateTime = viewModel.StartDate;
                ViewBag.EndDateTime = viewModel.EndDate;
                ViewBag.Pages = "Hammadde Kullanım ve Durum Raporu";
                ViewBag.PageNavi = "Hammadde Kullanım ve Durum Raporu";
                #endregion

                var culture = new CultureInfo("tr-TR");
                culture.NumberFormat.CurrencySymbol = "";
                CultureInfo.DefaultThreadCurrentCulture = culture;
                CultureInfo.DefaultThreadCurrentUICulture = culture;
                Thread.CurrentThread.CurrentCulture = culture;
                Thread.CurrentThread.CurrentUICulture = culture;

                list = HamMaddeKullanimCRUD.List(Convert.ToDateTime(viewModel.StartDate), Convert.ToDateTime(viewModel.EndDate), subeid, ID);
            }
            catch (Exception ex)
            {
                Singleton.WritingLogFile2("HamMaddeKullanimRaporuController", ex.ToString(), " :BaslangicTar:" + Convert.ToDateTime(tarihAraligiStartDate.ToString()).ToString("dd'/'MM'/'yyyy HH:mm") + " :BitisTar:" + Convert.ToDateTime(tarihAraligiEndDate.ToString()).ToString("dd'/'MM'/'yyyy HH:mm"), ex.StackTrace);
                return Redirect("/Authentication/Login");
            }
            return View(list);
        }
        #endregion Details   
    }
}