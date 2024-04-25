using iTextSharp.text;
using iTextSharp.text.html.simpleparser;
using iTextSharp.text.pdf;
using SefimV2.Helper;
using SefimV2.Models;
using SefimV2.Models.ProductSefimCRUD;
using SefimV2.Repository;
using SefimV2.ViewModels.GetTime;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading;
using System.Web.Mvc;

namespace SefimV2.Controllers
{
    [CheckLoggedIn]
    public class UrunFiyatController : Controller
    {
        // GET: UrunGrubu
        public ActionResult Index()
        {
            return View();
        }
        #region LIST        
        public ActionResult List()
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
            string tarihAraligiStartDate = string.Empty;
            string tarihAraligiEndDate = string.Empty;
            string productgrup = string.Empty;
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
                if (Request.QueryString["productgrup"] != null)
                {
                    productgrup = Request.QueryString["productgrup"].ToString();
                }
                productgrup = null;
            }
            catch (Exception ex)
            {
                Singleton.WritingLogFile("SubeUrunlerController_Details", ex.Message.ToString());
            }

            TimeViewModel viewModel = Singleton.GetTimeViewModel(tarihAraligiStartDate, tarihAraligiEndDate, durum);
            ViewBag.StartDateTime = viewModel.StartDate;
            ViewBag.EndDateTime = viewModel.EndDate;
            ViewBag.Pages = "Urun Fiyat";
            ViewBag.PageNavi = "Ürün Fiyat Listesi";
            #endregion Secilen Tarih Filterelemesine Göre Yapılan İşlemler  

            List<UrunFiyat> list = UrunFiyatCRUD.List(Convert.ToDateTime(viewModel.StartDate), Convert.ToDateTime(viewModel.EndDate), subeid, productgrup, ID);

            return View(list);
        }
        #endregion LIST

        #region DETAIL       
        public ActionResult Details()
        {
            string ID = Request.Cookies["PRAUT"].Value;

            #region Secilen Tarih Filterelemesine Göre Yapılan İşlemler  
            string tarihAraligiStartDate = string.Empty;
            string tarihAraligiEndDate = string.Empty;
            string productgrup = string.Empty;
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

                if (Request.QueryString["productgrup"] != null)
                {
                    productgrup = Request.QueryString["productgrup"].ToString().Replace("_", " ");
                }
                productgrup = null;
            }
            catch (Exception ex)
            {
                Singleton.WritingLogFile("SubeUrunlerController_Details", ex.Message.ToString());
            }
            #endregion

            TimeViewModel viewModel = Singleton.GetTimeViewModel(tarihAraligiStartDate, tarihAraligiEndDate, durum);
            ViewBag.StartDateTime = viewModel.StartDate;
            ViewBag.EndDateTime = viewModel.EndDate;
            ViewBag.Pages = "Urun Grubu Detay";
            ViewBag.SubeId = subeid;

            List<UrunFiyat> list = UrunFiyatCRUD.List(Convert.ToDateTime(viewModel.StartDate), Convert.ToDateTime(viewModel.EndDate), subeid, productgrup, ID);

            return View(list);
        }
        #endregion DETAIL

        #region PRODUCTS GROUP DETAIL     
        public ActionResult ProductGroupDetails()
        {
            string ID = Request.Cookies["PRAUT"].Value;
            #region Secilen Tarih Filterelemesine Göre Yapılan İşlemler  
            string tarihAraligiStartDate = string.Empty;
            string tarihAraligiEndDate = string.Empty;
            string productgrup = string.Empty;
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

                if (Request.QueryString["productgrup"] != null)
                {
                    string test_ = Request.QueryString["productgrup"].ToString();
                    string test2_ = Request.QueryString["productgrup"].ToString().Replace("_ _", " + ");
                    string test3_ = Request.QueryString["productgrup"].ToString().Replace("_ _", " + ").Replace("_", " ").Replace("]", "&");
                    productgrup = Request.QueryString["productgrup"].ToString().Replace("_ _", " + ").Replace("_", " ").Replace("]", "&");
                    //string aa = productgrup.Replace("]","&");
                }
            }
            catch (Exception ex)
            {
                Singleton.WritingLogFile("SubeUrunlerController_Details", ex.Message.ToString());
            }
            #endregion

            TimeViewModel viewModel = Singleton.GetTimeViewModel(tarihAraligiStartDate, tarihAraligiEndDate, durum);
            ViewBag.StartDateTime = viewModel.StartDate;
            ViewBag.EndDateTime = viewModel.EndDate;
            ViewBag.Pages = "Urun Grubu Detay";
            ViewBag.SubeId = subeid;
            ViewBag.ProductGroup = productgrup;

            List<UrunFiyat> list = UrunFiyatCRUD.List(Convert.ToDateTime(viewModel.StartDate), Convert.ToDateTime(viewModel.EndDate), subeid, productgrup.Replace("_", " "), ID);

            return View(list);
        }
        #endregion PRODUCTS GROUP DETAIL

        public ActionResult ProductList(string SubeIds)
        {
            var culture = new CultureInfo("tr-TR");
            culture.NumberFormat.CurrencyGroupSeparator = "";
            culture.NumberFormat.NumberGroupSeparator = "";
            //TL ve % işaretlerinin sağda görünmesi için.           
            culture.NumberFormat.CurrencyNegativePattern = 1;
            culture.NumberFormat.CurrencyPositivePattern = 3;
            culture.NumberFormat.PercentPositivePattern = 3;
            culture.NumberFormat.PercentNegativePattern = 3;
            culture.NumberFormat.NumberNegativePattern = 1;

            Thread.CurrentThread.CurrentCulture = culture;
            Thread.CurrentThread.CurrentUICulture = culture;
            CultureInfo.DefaultThreadCurrentCulture = culture;
            CultureInfo.DefaultThreadCurrentUICulture = culture;

            List<SefimPanelUrunEkleViewModel> lst = new List<SefimPanelUrunEkleViewModel>();
            try
            {
                if (!string.IsNullOrEmpty(SubeIds))
                {
                    lst = UrunFiyatCRUD.ProductList(SubeIds, false);
                }
            }
            catch (Exception ex)
            {
                lst = new List<SefimPanelUrunEkleViewModel>();
            }

            return View(lst);
        }


    }
}