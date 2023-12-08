using SefimV2.Helper;
using SefimV2.Models;
using SefimV2.Repository;
using SefimV2.ViewModels.GetTime;
using SefimV2.ViewModels.WaiterSales;
using SefimV2.ViewModelSendMail.WaiterSalesReportSendMail;
using System;
using System.Collections.Generic;
using System.Data;
using System.Web.Mvc;

namespace SefimV2.Controllers
{
    [CheckLoggedIn]
    public class WaiterSalesController : Controller
    {
        // GET: WaiterSales
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
            string tarihAraligiStartDate = "";
            string tarihAraligiEndDate = "";
            string subeid = "";
            string durum = "";
            string personel = "";

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

                if (Request.QueryString["personel"] != null)
                {
                    personel = Request.QueryString["personel"].ToString();
                }
                personel = null;
            }
            catch (Exception ex)
            {
                Singleton.WritingLogFile("WaiterSalesController_List", ex.Message.ToString());
            }
            TimeViewModel viewModel = Singleton.GetTimeViewModel(tarihAraligiStartDate, tarihAraligiEndDate, durum);

            ViewBag.StartDateTime = viewModel.StartDate;
            ViewBag.EndDateTime = viewModel.EndDate;
            ViewBag.Pages = "Personel Satis Raporu";
            ViewBag.PageNavi = "Personel Satış Raporu";
            #endregion

            List<WaiterSalesViewModel> list = WaiterSalesCRUD.List(Convert.ToDateTime(viewModel.StartDate), Convert.ToDateTime(viewModel.EndDate), subeid, personel, ID);
            return View(list);
        }
        #endregion LIST

        #region DETAIL       
        public ActionResult Details()
        {
            string ID = Request.Cookies["PRAUT"].Value;

            #region Secilen Tarih Filterelemesine Göre Yapılan İşlemler  
            string tarihAraligiStartDate = "";
            string tarihAraligiEndDate = "";
            string subeid = "";
            string durum = "";
            string personel = "";

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

                if (Request.QueryString["personel"] != null)
                {
                    personel = Request.QueryString["productgrup"].ToString().Replace("_", " ");
                }
                personel = "0";
            }
            catch (Exception ex)
            {
                Singleton.WritingLogFile("SubeUrunlerController_Details", ex.Message.ToString());
            }
            #endregion Secilen Tarih Filterelemesine Göre Yapılan İşlemler 

            TimeViewModel viewModel = Singleton.GetTimeViewModel(tarihAraligiStartDate, tarihAraligiEndDate, durum);
            ViewBag.StartDateTime = viewModel.StartDate;
            ViewBag.EndDateTime = viewModel.EndDate;
            ViewBag.Pages = "Personel Satis Detay";
            ViewBag.SubeId = subeid;

            List<WaiterSalesViewModel> list = WaiterSalesCRUD.List(Convert.ToDateTime(viewModel.StartDate), Convert.ToDateTime(viewModel.EndDate), subeid, personel, ID);

            return View(list);
        }
        #endregion DETAIL


        #region PRODUCTS GROUP DETAIL     
        public ActionResult PersonelDetails()
        {
            string ID = Request.Cookies["PRAUT"].Value;
            #region Secilen Tarih Filterelemesine Göre Yapılan İşlemler  
            string tarihAraligiStartDate = "";
            string tarihAraligiEndDate = "";
            string subeid = "";
            string durum = "";
            string personel = "";

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

                if (Request.QueryString["personel"] != null)
                {
                    personel = Request.QueryString["personel"].ToString().Replace("_ _", " + ").Replace("_", " ").Replace("]", "&");
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
            ViewBag.Pages = "Personel Detay";
            ViewBag.SubeId = subeid;
            ViewBag.ProductGroup = personel;

            //List<UrunGrubu> list = UrunGrubuCRUD.List(GelenGun, GelenGunSonu, subeid, productgrup);
            List<WaiterSalesViewModel> list = WaiterSalesCRUD.List(Convert.ToDateTime(viewModel.StartDate), Convert.ToDateTime(viewModel.EndDate), subeid, personel.Replace("_", " "), ID);
            //List<UrunGrubu> CiroyaGoreListe = list.OrderByDescending(o => o.ProductGroup). ToList();

            return View(list);
        }
        #endregion PRODUCTS GROUP DETAIL



        #region  SEND MAİL
        [HttpGet]
        public JsonResult SendReportMail(string StartDate, string EndDate, string ePostaAdress, string Pages, string SubeId)
        {
            string ID = Request.Cookies["PRAUT"].Value;

            if (SubeId == null)
            {
                SubeId = "";
            }

            ActionResultMessages data = new ActionResultMessages();
            #region Zorunu alan kontrolleri
            if (string.IsNullOrEmpty(ePostaAdress))
            {
                data.UserMessage = "Alanlar Boş Geçilemez. Lütfen Kontrol Ediniz.";// Singleton.getObject().ActionMessage("inputEmpty");
                return Json(data, JsonRequestBehavior.AllowGet);
            }
            #endregion            

            //TimeViewModel viewModel_ = Singleton.GetTimeViewModel(StartDate, EndDate, durum);
            List<WaiterSalesReportSendMailViewModel> list_ = SendMailGetDataCRUD.SendMailWaiterSalesCRUD.List(Convert.ToDateTime(StartDate), Convert.ToDateTime(EndDate), SubeId, ID);
            Singleton.ListtoDataTableConverter converter = new Singleton.ListtoDataTableConverter();
            DataTable dt = converter.ToDataTable(list_);
            Singleton sq = new Singleton();
            sq.ExportExcel(dt, ePostaAdress, Pages, StartDate, EndDate);

            return Json(list_, JsonRequestBehavior.AllowGet);
            //}
        }
        #endregion  SEND MAİL
    }
}