using iTextSharp.text;
using iTextSharp.text.html.simpleparser;
using iTextSharp.text.pdf;
using SefimV2.Helper;
using SefimV2.Models;
using SefimV2.Repository;
using SefimV2.ViewModels.GetTime;
using SefimV2.ViewModels.IskontoDetayRaporuViewModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Web.Mvc;

namespace SefimV2.Controllers
{
    [CheckLoggedIn]
    public class IkramDetayRaporuController : Controller
    {

        // GET: IptalDetayRaporu
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
                Singleton.WritingLogFile("IskontoDetayRaporuController", ex.Message.ToString());
            }
            TimeViewModel viewModel = Singleton.GetTimeViewModel(tarihAraligiStartDate, tarihAraligiEndDate, durum);
            ViewBag.StartDateTime = viewModel.StartDate;
            ViewBag.EndDateTime = viewModel.EndDate;
            ViewBag.Pages = "İkram Detay Raporu";
            ViewBag.PageNavi = "İkram Detay Raporu";
            #endregion Secilen Tarih Filterelemesine Göre Yapılan İşlemler  


            List<IkramDetayRaporuViewModel> list = IkramDetayRaporuCRUD.List(Convert.ToDateTime(viewModel.StartDate), Convert.ToDateTime(viewModel.EndDate), "", "", ID);
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
            ViewBag.Pages = "İkram Detay Raporu";
            ViewBag.SubeId = subeid;

            //List<UrunGrubu> list = UrunGrubuCRUD.List(GelenGun, GelenGunSonu, subeid, productgrup);
            List<IkramDetayRaporuViewModel> list = IkramDetayRaporuCRUD.List(Convert.ToDateTime(viewModel.StartDate), Convert.ToDateTime(viewModel.EndDate), subeid, productgrup, ID);
            //List<UrunGrubu> CiroyaGoreListe = list.OrderByDescending(o => o.ProductGroup). ToList();
            return View(list);
        }
        #endregion DETAIL

     
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
    }
}