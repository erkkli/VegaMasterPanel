using iTextSharp.text;
using iTextSharp.text.html.simpleparser;
using iTextSharp.text.pdf;
using SefimV2.Helper;
using SefimV2.Helper.ExcelHelper;
using SefimV2.Models;
using SefimV2.Repository;
using SefimV2.SendMailGetDataCRUD;
using SefimV2.ViewModels.GetTime;
using SefimV2.ViewModelSendMail.UrunGrubuReportSendMail;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Web.Mvc;

namespace SefimV2.Controllers
{
    [CheckLoggedIn]
    public class UrunGrubuController : Controller
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
            ViewBag.Pages = "Urun Grubu";
            ViewBag.PageNavi = "Ürün Grubu Satış Raporu";
            #endregion Secilen Tarih Filterelemesine Göre Yapılan İşlemler  

            List<UrunGrubu> list = UrunGrubuCRUD.List(Convert.ToDateTime(viewModel.StartDate), Convert.ToDateTime(viewModel.EndDate), subeid, productgrup, ID, false);

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

            List<UrunGrubu> list = UrunGrubuCRUD.List(Convert.ToDateTime(viewModel.StartDate), Convert.ToDateTime(viewModel.EndDate), subeid, productgrup, ID, false);

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

            List<UrunGrubu> list = UrunGrubuCRUD.List(Convert.ToDateTime(viewModel.StartDate), Convert.ToDateTime(viewModel.EndDate), subeid, productgrup.Replace("_", " "), ID, false);

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
            string productgrup = null;
            ActionResultMessages data = new ActionResultMessages();
            #region Zorunu alan kontrolleri
            if (string.IsNullOrEmpty(ePostaAdress))
            {
                data.UserMessage = "Alanlar Boş Geçilemez. Lütfen Kontrol Ediniz.";// Singleton.getObject().ActionMessage("inputEmpty");
                return Json(data, JsonRequestBehavior.AllowGet);
            }
            #endregion            

            //TimeViewModel viewModel_ = Singleton.GetTimeViewModel(StartDate, EndDate, durum);
            List<UrunGrubuReportSendMailViewModel> list_ = SendMailUrunGrubuCRUD.List(Convert.ToDateTime(StartDate), Convert.ToDateTime(EndDate), SubeId, productgrup, ID);

            Singleton.ListtoDataTableConverter converter = new Singleton.ListtoDataTableConverter();

            DataTable dt = converter.ToDataTable(list_);

            Singleton sq = new Singleton();
            sq.ExportExcel(dt, ePostaAdress, Pages, StartDate, EndDate);

            return Json(list_, JsonRequestBehavior.AllowGet);
            //}
        }
        #endregion  SEND MAİL

        #region  SEND MAİL DETAİL
        [HttpGet]
        public JsonResult SendReportMailDetail(string StartDate, string EndDate, string ePostaAdress, string Pages, string SubeId)
        {
            string ID = Request.Cookies["PRAUT"].Value;

            if (SubeId == null)
            {
                SubeId = string.Empty;
            }
            string productgrup = null;
            ActionResultMessages data = new ActionResultMessages();
            #region Zorunu alan kontrolleri
            if (string.IsNullOrEmpty(ePostaAdress))
            {
                data.UserMessage = "Alanlar Boş Geçilemez. Lütfen Kontrol Ediniz.";// Singleton.getObject().ActionMessage("inputEmpty");
                return Json(data, JsonRequestBehavior.AllowGet);
            }
            #endregion            


            //TimeViewModel viewModel_ = Singleton.GetTimeViewModel(StartDate, EndDate, durum);
            List<UrunGrubuDetailSendMailViewModel> list_ = SendMailUrunGrubuDetailCRDU.List(Convert.ToDateTime(StartDate), Convert.ToDateTime(EndDate), SubeId, productgrup, ID);

            Singleton.ListtoDataTableConverter converter = new Singleton.ListtoDataTableConverter();
            DataTable dt = converter.ToDataTable(list_);
            Singleton sq = new Singleton();
            sq.ExportExcel(dt, ePostaAdress, Pages, StartDate, EndDate);

            return Json(list_, JsonRequestBehavior.AllowGet);
            //}
        }
        #endregion SEND MAİL DETAİL

        #region  SEND MAİL PRODUCT
        [HttpGet]
        public JsonResult SendReportMailProduct(string StartDate, string EndDate, string ePostaAdress, string Pages, string SubeId, string ProductGroup)
        {
            string ID = Request.Cookies["PRAUT"].Value;

            if (SubeId == null)
            {
                SubeId = "";
            }
            //string productgrup = null;
            ActionResultMessages data = new ActionResultMessages();
            #region Zorunu alan kontrolleri
            if (string.IsNullOrEmpty(ePostaAdress))
            {
                data.UserMessage = "Alanlar Boş Geçilemez. Lütfen Kontrol Ediniz.";// Singleton.getObject().ActionMessage("inputEmpty");
                return Json(data, JsonRequestBehavior.AllowGet);
            }
            #endregion            

            //TimeViewModel viewModel_ = Singleton.GetTimeViewModel(StartDate, EndDate, durum);
            List<UrunGrubuProductSendMailViewModel> list_ = SendMailUrunGrubuProductCRUD.List(Convert.ToDateTime(StartDate), Convert.ToDateTime(EndDate), SubeId, ProductGroup, ID);

            Singleton.ListtoDataTableConverter converter = new Singleton.ListtoDataTableConverter();

            DataTable dt = converter.ToDataTable(list_);

            Singleton sq = new Singleton();
            sq.ExportExcel(dt, ePostaAdress, Pages, StartDate, EndDate);

            return Json(list_, JsonRequestBehavior.AllowGet);
            //}
        }
        #endregion SEND MAİL PRODUCT

        #region PDF
        [HttpPost]
        [ValidateInput(false)]
        public FileResult Export(string GridHtml)
        {
            //using (MemoryStream stream = new System.IO.MemoryStream())
            //{
            //    StringBuilder sb = new StringBuilder();            
            string banner = " <table width = '100%' cellspacing = '0' cellpadding = '2' ><b> Sipariş Tablosu </b> ";
            //"<tr><td align='center' style='background-color: #18B5F0' colspan = '2'><b>Sipariş Tablosu</b></td></tr>";
            string banner2 = " </table> ";
            string rt = banner + GridHtml + banner2;
            //    string[] stringSeparators = new string[] { "\r\n" };
            //    string[] lines = GridHtml.Split(stringSeparators, StringSplitOptions.None);
            //    string asdf = lines[1].Replace(" ","");
            //    StringReader sr = new StringReader(rt);                
            //    Document pdfDoc = new Document(PageSize.A4, 10f, 10f, 100f, 0f);
            //    PdfWriter writer = PdfWriter.GetInstance(pdfDoc, stream);
            //    pdfDoc.Open();
            //    XMLWorkerHelper.GetInstance().ParseXHtml(writer, pdfDoc, sr);
            //    pdfDoc.Close();
            //    return File(stream.ToArray(), "application/pdf", "PDF_.pdf");
            //}
            StringBuilder sb = new StringBuilder();
            sb.Append("<div>");
            sb.Append("<b align='center' style='background-color:#b22222;margin-bottom:100px '>Sipariş Tablosu </b> <br/>");
            sb.Append("<table width='100%' cellspacing='5' cellpadding='2'> ");
            //sb.Append(" <b>Sipariş Tablosu</b> ");
            //sb.Append("<tr><td align='center' style='background-color: #18B5F0'><b>Sipariş Tablosu</b></td></tr>");
            //sb.Append("<tr><td colspan = '2'></td></tr>");
            sb.Append(GridHtml);
            sb.Append("</table>");
            sb.Append("</div>");
            StringReader sr = new StringReader(sb.ToString());

            Document pdfDoc = new Document(PageSize.A3, 5f, 5f, 80f, 0f);
            HTMLWorker htmlparser = new HTMLWorker(pdfDoc);

            #region Türkçe karakter sorunu için yazılması gereken kod bloğu.
            FontFactory.Register(Path.Combine("C:\\Windows\\Fonts\\Arial.ttf"), "Garamond"); // kendi türkçe karakter desteği olan fontunuzu da girebilirsiniz.
            StyleSheet css = new StyleSheet();
            css.LoadTagStyle("body", "face", "Garamond");
            css.LoadTagStyle("body", "encoding", "Identity-H");
            css.LoadTagStyle("body", "size", "9pt");
            htmlparser.SetStyleSheet(css);
            #endregion


            using (MemoryStream stream = new System.IO.MemoryStream())
            {
                PdfWriter writer = PdfWriter.GetInstance(pdfDoc, stream);
                pdfDoc.Open();
                htmlparser.Parse(sr);
                pdfDoc.Close();
                byte[] bytes = stream.ToArray();
                stream.Close();
                return File(stream.ToArray(), "application/pdf", "TestPDF.pdf");
            }
        }
        #endregion PDF




        #region Excel Export 
        [HttpGet]
        public FileResult ExcelExportDetayliUrunGrubuRaporu(string Durum, string StartDate, string EndDate, string SubeId)
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
            var result = UrunGrubuCRUD.List(Convert.ToDateTime(viewModel.StartDate), Convert.ToDateTime(viewModel.EndDate), subeid, null, ID, true);
            //UrunGrubuCRUD.List(Convert.ToDateTime(viewModel.StartDate), Convert.ToDateTime(viewModel.EndDate), subeid, productgrup, ID);
            result = result.OrderBy(x => x.Sube).ToList();
            var sumMiktar = result.Sum(x => x.Miktar);
            var sumDebit = result.Sum(x => x.Debit);
            var sumIkram = result.Sum(x => x.Ikram);
            var sumIndirim = result.Sum(x => x.Indirim);
            var sumNetTutar = result.Sum(x => x.NetTutar);

            var dataTable = new DataTable("Urun_Raporu");
            dataTable.Columns.Add("ŞUBE", typeof(string));
            dataTable.Columns.Add("ÜRÜN GRUBU", typeof(string));
            //dataTable.Columns.Add("ÜRÜN", typeof(string));
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
                        item.ProductGroup,
                        //item.ProductName,
                        decimal.Round(item.Miktar, 2, MidpointRounding.AwayFromZero),
                        decimal.Round(item.Debit, 2, MidpointRounding.AwayFromZero),
                        decimal.Round(item.Ikram, 2, MidpointRounding.AwayFromZero),
                        decimal.Round(item.Indirim, 2, MidpointRounding.AwayFromZero),
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
            var fileName = "Başlangıç Tarihi " + StartDate + " Bitiş Tarihi " + EndDate + " Tarihli Detaylı Ürün Grubu Raporu _(" + DateTime.Now + ")" + ".xlsx";
            ExcelHelper excelHelper = new ExcelHelper();
            return File(excelHelper.WriteExcelFile(dataTable), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }

        #endregion Excel Export 

    }
}