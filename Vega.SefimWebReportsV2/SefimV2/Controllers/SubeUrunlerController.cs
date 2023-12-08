using SefimV2.Helper;
using SefimV2.Helper.ExcelHelper;
using SefimV2.Models;
using SefimV2.Repository;
using SefimV2.SendMailGetDataCRUD;
using SefimV2.ViewModels.GetTime;
using SefimV2.ViewModelSendMail.SubeUrunlerReportSendMail;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web.Mvc;

namespace SefimV2.Controllers
{
    [CheckLoggedIn]
    public class SubeUrunlerController : Controller
    {
        // GET: SubeUrunler
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
            catch (Exception ex)
            {
                Singleton.WritingLogFile("SubeUrunlerController_Details", ex.Message.ToString());
            }

            TimeViewModel viewModel = Singleton.GetTimeViewModel(tarihAraligiStartDate, tarihAraligiEndDate, durum);

            ViewBag.StartDateTime = viewModel.StartDate;
            ViewBag.EndDateTime = viewModel.EndDate;
            ViewBag.Pages = "Urun Satis Raporu";
            ViewBag.PageNavi = "Ürün Satış Raporu";
            ViewBag.SubeId = subeid;
            #endregion

            List<SubeUrun> list = SubeUrunCRUD.List(Convert.ToDateTime(viewModel.StartDate), Convert.ToDateTime(viewModel.EndDate), subeid, ID, "", "");
            return View(list);
        }
        #endregion LIST

        #region STATUS BARLAR
        //[HttpGet]
        //public ActionResult List(string StartDate, string EndDate)
        //{


        //    StartDate = "2019-09-12 08:31:00";
        //    EndDate = "2019-09-12 23:31:00";
        //    List<SubeUrun> list = SubeUrunCRUD.List(StartDate, EndDate, "");

        //    return View(list);
        //}
        #endregion STATUS BARLAR

        #region DETAIL LIST        
        public ActionResult Details()
        {
            string ID = Request.Cookies["PRAUT"].Value;

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
            catch (Exception ex)
            {
                Singleton.WritingLogFile("SubeUrunlerController_Details", ex.Message.ToString());
            }
            #endregion

            TimeViewModel viewModel = Singleton.GetTimeViewModel(tarihAraligiStartDate, tarihAraligiEndDate, durum);
            ViewBag.StartDateTime = viewModel.StartDate;
            ViewBag.EndDateTime = viewModel.EndDate;
            ViewBag.Pages = "Şube Detay Urunleri";
            ViewBag.SubeId = subeid;

            List<SubeUrun> list = SubeUrunCRUD.List(Convert.ToDateTime(viewModel.StartDate), Convert.ToDateTime(viewModel.EndDate), subeid, ID, "", "");

            return View(list);
        }
        #endregion DETAIL LIST

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
            List<SubeUrunlerReportSendMailViewModel> list_ = SendMailSubeUrunlerCRUD.List(Convert.ToDateTime(StartDate), Convert.ToDateTime(EndDate), SubeId, ID);

            Singleton.ListtoDataTableConverter converter = new Singleton.ListtoDataTableConverter();

            DataTable dt = converter.ToDataTable(list_);

            Singleton sq = new Singleton();
            sq.ExportExcel(dt, ePostaAdress, Pages, StartDate, EndDate);

            return Json(list_, JsonRequestBehavior.AllowGet);
            //}
        }
        #endregion SEND MAİL


        #region Excel Export 
        [HttpGet]
        public FileResult ExcelExportDetayliUrunRaporu(string Durum, string StartDate, string EndDate, string SubeId)
        {
            string ID = Request.Cookies["PRAUT"].Value;

            #region Secilen Tarih Filterelemesine Göre Yapılan İşlemler  
            string tarihAraligiStartDate = string.Empty;
            string tarihAraligiEndDate = string.Empty;
            string subeid = string.Empty;
            string durum = string.Empty;
            string tumStoklarGosterilsinMi = string.Empty;

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
                if (Request.QueryString["tumStoklarGetirilsinMi"] != null)
                {
                    tumStoklarGosterilsinMi = Request.QueryString["tumStoklarGetirilsinMi"].ToString();
                }
            }
            catch (Exception ex)
            {
                Singleton.WritingLogFile("RecipeCostController_Details", ex.Message.ToString());
            }
            #endregion

            var viewModel = Singleton.GetTimeViewModel(StartDate, EndDate, durum);
            ViewBag.StartDateTime = viewModel.StartDate;
            ViewBag.EndDateTime = viewModel.EndDate;
            ViewBag.Pages = "Şube Detay Urunleri";
            ViewBag.SubeId = subeid;
            subeid = "exportExcel";

            //subeid = string.Empty;
            var result = SubeUrunCRUD.List(Convert.ToDateTime(viewModel.StartDate), Convert.ToDateTime(viewModel.EndDate), subeid, ID, "", "", true);
            result = result.OrderBy(x => x.Sube).ToList();
            var sumMiktar = result.Sum(x => x.Miktar);
            var sumDebit = result.Sum(x => x.Debit);
            var sumIkram = result.Sum(x => x.Ikram);
            var sumIndirim = result.Sum(x => x.Indirim);
            var sumNetTutar = result.Sum(x => x.NetTutar);

            var dataTable = new DataTable("Urun_Raporu");
            dataTable.Columns.Add("ŞUBE", typeof(string));
            dataTable.Columns.Add("ÜRÜN", typeof(string));
            dataTable.Columns.Add("MİKTAR", typeof(decimal));
            dataTable.Columns.Add("TUTAR", typeof(decimal));
            dataTable.Columns.Add("İKRAM", typeof(decimal));
            dataTable.Columns.Add("İNDİRİM", typeof(decimal));
            dataTable.Columns.Add("NET TUTAR", typeof(decimal));

            foreach (var item in result)
            {
                dataTable.Rows.Add
                    (
                        item.Sube,
                        item.ProductName,
                        decimal.Round(item.Miktar, 2, MidpointRounding.AwayFromZero),
                        decimal.Round(item.Debit, 2, MidpointRounding.AwayFromZero),
                        decimal.Round(item.Ikram, 2, MidpointRounding.AwayFromZero)  ,
                        decimal.Round(item.Indirim, 2, MidpointRounding.AwayFromZero)   ,
                        decimal.Round(item.NetTutar, 2, MidpointRounding.AwayFromZero)   
                    );
            }
            dataTable.Rows.Add(
                "TÜM ŞUBE TOPLAMLARI:",
                "-",
               

                   decimal.Round(sumMiktar, 2, MidpointRounding.AwayFromZero),
                        decimal.Round(sumDebit, 2, MidpointRounding.AwayFromZero),
                        decimal.Round(sumIkram, 2, MidpointRounding.AwayFromZero),
                        decimal.Round(sumIndirim, 2, MidpointRounding.AwayFromZero),
                        decimal.Round(sumNetTutar, 2, MidpointRounding.AwayFromZero)
                );
            var fileName = "Başlangıç Tarihi " + StartDate + " Bitiş Tarihi " + EndDate + " Tarihli Detaylı Ürün Raporu _(" + DateTime.Now + ")" + ".xlsx";
            ExcelHelper excelHelper = new ExcelHelper();
            return File(excelHelper.WriteExcelFile(dataTable), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }

        #endregion Excel Export 
    }
}