using SefimV2.Helper;
using SefimV2.Models;
using SefimV2.Repository;
using SefimV2.SendMailGetDataCRUD;
using SefimV2.ViewModels.GetTime;
using SefimV2.ViewModelSendMail.KuryeCiroReportSendMail;
using SefimV2.ViewModelSendMail.PaketCiroReportSendMail;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SefimV2.Controllers
{
    [CheckLoggedIn]
    public class PaketCiroController : Controller
    {
        // GET: PaketCiro
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
            catch (Exception ex)
            {
                Singleton.WritingLogFile("SubeUrunlerController_Details", ex.Message.ToString());
            }

            TimeViewModel viewModel = Singleton.GetTimeViewModel(tarihAraligiStartDate, tarihAraligiEndDate, durum);

            ViewBag.StartDateTime = viewModel.StartDate;
            ViewBag.EndDateTime = viewModel.EndDate;
            ViewBag.Pages = "Paket Ciro";
            ViewBag.PageNavi = "Paket Servis Raporu";
            #endregion

            List<PaketCiro> list = PaketCiroCRUD.List(Convert.ToDateTime(viewModel.StartDate), Convert.ToDateTime(viewModel.EndDate), ID);
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
            catch (Exception ex)
            {
                Singleton.WritingLogFile("SubeUrunlerController_Details", ex.Message.ToString());
            }
            #endregion

            TimeViewModel viewModel = Singleton.GetTimeViewModel(tarihAraligiStartDate, tarihAraligiEndDate, durum);
            ViewBag.StartDateTime = viewModel.StartDate;
            ViewBag.EndDateTime = viewModel.EndDate;
            ViewBag.Pages = "Paket Ciro Detay";
            ViewBag.SubeId = subeid;
            ViewBag.ID = ID;

            List<KuryeCiro> list = KuryeCiroCRUD.List(Convert.ToDateTime(viewModel.StartDate), Convert.ToDateTime(viewModel.EndDate), subeid, ID, false);

            return View(list);
        }
        #endregion DETAIL

        #region  SEND MAİL
        [HttpGet]
        public JsonResult SendReportMail(string StartDate, string EndDate, string ePostaAdress, string Pages)
        {
            string ID = Request.Cookies["PRAUT"].Value;
            ActionResultMessages data = new ActionResultMessages();
            #region Zorunu alan kontrolleri
            if (string.IsNullOrEmpty(ePostaAdress))
            {
                data.UserMessage = "Alanlar Boş Geçilemez. Lütfen Kontrol Ediniz.";// Singleton.getObject().ActionMessage("inputEmpty");
                return Json(data, JsonRequestBehavior.AllowGet);
            }
            #endregion            


            //TimeViewModel viewModel_ = Singleton.GetTimeViewModel(StartDate, EndDate, durum);
            List<PaketCiroReportSendMailViewModel> list_ = SendMailPaketCiroCRUD.List(Convert.ToDateTime(StartDate), Convert.ToDateTime(EndDate), ID);

            Singleton.ListtoDataTableConverter converter = new Singleton.ListtoDataTableConverter();

            DataTable dt = converter.ToDataTable(list_);

            Singleton sq = new Singleton();
            sq.ExportExcel(dt, ePostaAdress, Pages, StartDate, EndDate);

            return Json(list_, JsonRequestBehavior.AllowGet);
            //}
        }
        #endregion SEND MAİL

        #region  SEND MAİL KURYE CİRO DETAY
        [HttpGet]
        public JsonResult SendReportMailKurye(string StartDate, string EndDate, string ePostaAdress, string Pages, string SubeId)
        {
            string ID = Request.Cookies["PRAUT"].Value;
            ActionResultMessages data = new ActionResultMessages();
            #region Zorunu alan kontrolleri
            if (string.IsNullOrEmpty(ePostaAdress))
            {
                data.UserMessage = "Alanlar Boş Geçilemez. Lütfen Kontrol Ediniz.";// Singleton.getObject().ActionMessage("inputEmpty");
                return Json(data, JsonRequestBehavior.AllowGet);
            }
            #endregion            


            //TimeViewModel viewModel_ = Singleton.GetTimeViewModel(StartDate, EndDate, durum);
            List<KuryeCiroReportSendMailViewModel> list_ = SendMailKuryeCiroCRUD.List(Convert.ToDateTime(StartDate), Convert.ToDateTime(EndDate), SubeId, ID);

            Singleton.ListtoDataTableConverter converter = new Singleton.ListtoDataTableConverter();

            DataTable dt = converter.ToDataTable(list_);

            Singleton sq = new Singleton();
            sq.ExportExcel(dt, ePostaAdress, Pages, StartDate, EndDate);

            return Json(list_, JsonRequestBehavior.AllowGet);
            //}
        }
        #endregion SEND MAİL KURYE CİRO DETAY
    }
}