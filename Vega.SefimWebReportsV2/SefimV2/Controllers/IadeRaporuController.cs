using SefimV2.Helper;
using SefimV2.Models;
using SefimV2.Repository;
using SefimV2.SendMailGetDataCRUD;
using SefimV2.ViewModels.GetTime;
using SefimV2.ViewModelSendMail.PersonelCiroReportSendMail;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SefimV2.Controllers
{
    [CheckLoggedIn]
    public class IadeRaporuController : Controller
    {
        public object SendMailPersonelCiro { get; private set; }

        // GET: IadeRaporu
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
            string productgrup = "";

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
            ViewBag.Pages = "İade Raporu";
            ViewBag.PageNavi = "İade Raporu";
            #endregion

            List<IadeViewModel> list = IadeCRUD.List(Convert.ToDateTime(viewModel.StartDate), Convert.ToDateTime(viewModel.EndDate), subeid, ID, productgrup);
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
            string productgrup = "";

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
                productgrup = "0";
            }
            catch (Exception ex)
            {
                Singleton.WritingLogFile("SubeUrunlerController_Details", ex.Message.ToString());
            }
            #endregion

            TimeViewModel viewModel = Singleton.GetTimeViewModel(tarihAraligiStartDate, tarihAraligiEndDate, durum);
            ViewBag.StartDateTime = viewModel.StartDate;
            ViewBag.EndDateTime = viewModel.EndDate;
            ViewBag.Pages = "İade Detay";
            ViewBag.SubeId = subeid;

            List<IadeViewModel> list = IadeCRUD.List(Convert.ToDateTime(viewModel.StartDate), Convert.ToDateTime(viewModel.EndDate), subeid, ID, productgrup);

            return View(list);
        }
        #endregion DETAIL

        #region PRODUCTS GROUP DETAIL     
        public ActionResult ProductDetails()
        {
            string ID = Request.Cookies["PRAUT"].Value;
            #region Secilen Tarih Filterelemesine Göre Yapılan İşlemler  
            string tarihAraligiStartDate = "";
            string tarihAraligiEndDate = "";
            string subeid = "";
            string durum = "";
            string productgrup = "";

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
            List<IadeViewModel> list = IadeCRUD.List(Convert.ToDateTime(viewModel.StartDate), Convert.ToDateTime(viewModel.EndDate), subeid, ID, productgrup.Replace("_", " "));

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
            List<PersonelCiroReportSendMailViewModel> list_ = SendMailPersonelCiroCRUD.List(Convert.ToDateTime(StartDate), Convert.ToDateTime(EndDate), SubeId, ID);
            Singleton.ListtoDataTableConverter converter = new Singleton.ListtoDataTableConverter();
            DataTable dt = converter.ToDataTable(list_);
            Singleton sq = new Singleton();
            sq.ExportExcel(dt, ePostaAdress, Pages, StartDate, EndDate);
            return Json(list_, JsonRequestBehavior.AllowGet);
            //}
        }
        #endregion SEND MAİL

    }
}