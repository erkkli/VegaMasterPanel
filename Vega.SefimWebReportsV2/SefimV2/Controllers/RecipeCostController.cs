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
using System.Globalization;
using System.Web.Mvc;

namespace SefimV2.Controllers
{
    [CheckLoggedIn]
    public class RecipeCostController : Controller
    {
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
            string hammaddeStokTipi = string.Empty;

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

                if (Request.QueryString["HammaddeStokTipi"] != null)
                    hammaddeStokTipi = Request.QueryString["HammaddeStokTipi"].ToString();
            }
            catch (Exception ex)
            {
                Singleton.WritingLogFile("RecipeCostController_Details", ex.Message.ToString());
            }
            TimeViewModel viewModel = Singleton.GetTimeViewModel(tarihAraligiStartDate, tarihAraligiEndDate, durum);

            ViewBag.StartDateTime = viewModel.StartDate;
            ViewBag.EndDateTime = viewModel.EndDate;
            ViewBag.Pages = "Recete Maliyet Raporu";
            ViewBag.PageNavi = "Recete Maliyet Raporu";
            ViewBag.SubeId = subeid;
            #endregion

            //System.Globalization.CultureInfo ci = new System.Globalization.CultureInfo("tr-TR");
            //ci.NumberFormat.CurrencySymbol = "";
            //ci.NumberFormat.CurrencyDecimalDigits = 0;
            //System.Threading.Thread.CurrentThread.CurrentCulture = ci;
            //System.Threading.Thread.CurrentThread.CurrentUICulture = ci;

            var culture = new CultureInfo("tr-TR");
            culture.NumberFormat.CurrencyGroupSeparator = "";
            culture.NumberFormat.NumberGroupSeparator = "";
            //TL ve % işaretlerinin sağda görünmesi için. 
            //https://learn.microsoft.com/en-us/dotnet/api/system.globalization.numberformatinfo.currencypositivepattern?view=net-7.0
            culture.NumberFormat.CurrencyNegativePattern = 3;
            culture.NumberFormat.CurrencyPositivePattern = 3;
            culture.NumberFormat.PercentPositivePattern = 3;
            culture.NumberFormat.PercentNegativePattern = 3;
            culture.NumberFormat.NumberNegativePattern = 3;

            List<SubeUrun> list = RecipeCostCRUD2.List(Convert.ToDateTime(viewModel.StartDate), Convert.ToDateTime(viewModel.EndDate), subeid, ID, "", "", 0, false, hammaddeStokTipi);
            return View(list);
        }
        #endregion LIST

        #region DETAIL LIST        
        public ActionResult Details()
        {
            string ID = Request.Cookies["PRAUT"].Value;

            #region Secilen Tarih Filterelemesine Göre Yapılan İşlemler  
            string tarihAraligiStartDate = "";
            string tarihAraligiEndDate = "";
            string subeid = string.Empty;
            string durum = string.Empty;
            string tumStoklarGosterilsinMi = string.Empty;
            string hammaddeStokTipi = string.Empty;

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

                if (Request.QueryString["HammaddeStokTipi"] != null)
                    hammaddeStokTipi = Request.QueryString["HammaddeStokTipi"].ToString();
            }
            catch (Exception ex)
            {
                Singleton.WritingLogFile("RecipeCostController_Details", ex.Message.ToString());
            }
            #endregion

            TimeViewModel viewModel = Singleton.GetTimeViewModel(tarihAraligiStartDate, tarihAraligiEndDate, durum);
            ViewBag.StartDateTime = viewModel.StartDate;
            ViewBag.EndDateTime = viewModel.EndDate;
            ViewBag.Pages = "Şube Detay Urunleri";
            ViewBag.SubeId = subeid;



            var culture = new CultureInfo("tr-TR");
            culture.NumberFormat.CurrencyGroupSeparator = "";
            culture.NumberFormat.NumberGroupSeparator = "";

            //TL ve % işaretlerinin sağda görünmesi için. 
            //https://learn.microsoft.com/en-us/dotnet/api/system.globalization.numberformatinfo.currencypositivepattern?view=net-7.0
            culture.NumberFormat.CurrencyNegativePattern = 3;
            culture.NumberFormat.CurrencyPositivePattern = 3;
            culture.NumberFormat.PercentPositivePattern = 3;
            culture.NumberFormat.PercentNegativePattern = 3;
            culture.NumberFormat.NumberNegativePattern = 3;

            List<SubeUrun> list = RecipeCostCRUD2.List(Convert.ToDateTime(viewModel.StartDate), Convert.ToDateTime(viewModel.EndDate), subeid, ID, "", "", 2, Convert.ToBoolean(tumStoklarGosterilsinMi), hammaddeStokTipi);

            return View(list);
        }
        #endregion DETAIL LIST


        #region PRODUCTS GROUP DETAIL     
        public ActionResult ProductDetails()
        {
            string ID = Request.Cookies["PRAUT"].Value;
            #region Secilen Tarih Filterelemesine Göre Yapılan İşlemler  
            string tarihAraligiStartDate = string.Empty;
            string tarihAraligiEndDate = string.Empty;
            string productName = string.Empty;
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
                if (Request.QueryString["productName"] != null)
                {
                    productName = Request.QueryString["ProductName"].ToString();
                    productName = Request.QueryString["ProductName"].ToString().Replace("_ _", " + ").Replace("_", " ").Replace("]", "&");
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
            ViewBag.Pages = "Urun  Detay";
            ViewBag.SubeId = subeid;
            ViewBag.ProductGroup = productName;



            var culture = new CultureInfo("tr-TR");
            culture.NumberFormat.CurrencyGroupSeparator = "";
            culture.NumberFormat.NumberGroupSeparator = "";

            //TL ve % işaretlerinin sağda görünmesi için. 
            //https://learn.microsoft.com/en-us/dotnet/api/system.globalization.numberformatinfo.currencypositivepattern?view=net-7.0
            culture.NumberFormat.CurrencyNegativePattern = 3;
            culture.NumberFormat.CurrencyPositivePattern = 3;
            culture.NumberFormat.PercentPositivePattern = 3;
            culture.NumberFormat.PercentNegativePattern = 3;
            culture.NumberFormat.NumberNegativePattern = 3;

            List<SubeUrun> list = RecipeCostCRUD.List(Convert.ToDateTime(viewModel.StartDate), Convert.ToDateTime(viewModel.EndDate), subeid, ID, 3, productName, false);

            return View(list);
        }
        #endregion PRODUCTS GROUP DETAIL

        #region Excel Export 
        [HttpGet]
        public FileResult ExcelExportDetayliReceteMaliyet(string StartDate, string EndDate, string SubeId)
        {
            string ID = Request.Cookies["PRAUT"].Value;

            #region Secilen Tarih Filterelemesine Göre Yapılan İşlemler  
            string tarihAraligiStartDate = "";
            string tarihAraligiEndDate = "";
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

            TimeViewModel viewModel = Singleton.GetTimeViewModel(StartDate, EndDate, durum);
            ViewBag.StartDateTime = viewModel.StartDate;
            ViewBag.EndDateTime = viewModel.EndDate;
            ViewBag.Pages = "Şube Detay Urunleri";
            ViewBag.SubeId = subeid;
            subeid = "exportExcel";

            List<SubeUrun> result = RecipeCostCRUD2.List(Convert.ToDateTime(StartDate), Convert.ToDateTime(EndDate), subeid, ID, "", "", 2, false, "");

            var dataTable = new DataTable("Recete_Maliyet");
            dataTable.Columns.Add("ŞUBE", typeof(string));
            dataTable.Columns.Add("ÜRÜN", typeof(string));
            dataTable.Columns.Add("MİKTAR", typeof(string));
            dataTable.Columns.Add("TUTAR", typeof(string));
            dataTable.Columns.Add("İKRAM", typeof(string));
            dataTable.Columns.Add("İNDİRİM", typeof(string));
            dataTable.Columns.Add("NET TUTAR", typeof(string));
            dataTable.Columns.Add("RECETE MALİYETİ", typeof(string));
            dataTable.Columns.Add("RECETE TOPLAM MİKTARI", typeof(string));
            dataTable.Columns.Add("BİRİM MALİYETİ", typeof(string));
            dataTable.Columns.Add("KAR TUTARI", typeof(string));

            foreach (var item in result)
            {
                dataTable.Rows.Add
                    (
                        item.Sube,
                        item.ProductName,
                        item.Miktar,
                        item.Debit,
                        item.Ikram,
                        item.Indirim,
                        item.NetTutar,
                        item.ReceteTutari,
                        item.ReceteToplamMiktari,
                        item.ReceteBirimMaliyeti,
                        item.KarTutari
                    );
            }
            var fileName = "Recete Maliyet Raporu -" + DateTime.Now + "-" + ".xlsx";
            ExcelHelper excelHelper = new ExcelHelper();
            return File(excelHelper.WriteExcelFile(dataTable), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }

        #endregion Excel Export 

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

    }
}