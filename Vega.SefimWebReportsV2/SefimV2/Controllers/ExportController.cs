using ClosedXML.Excel;
using SefimV2.Models;
using SefimV2.SendMailGetDataCRUD;
using SefimV2.ViewModelSendMail.CiroRaporlariSendMail;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Web;
using System.Web.Configuration;
using System.Web.Mvc;
using System.Web.UI;
using System.Web.UI.WebControls;
using static SefimV2.Helper.Singleton;

namespace SefimV2.Controllers
{
    public class ExportController : Controller
    {
        // GET: Export
        public ActionResult Index()
        {
            DataTable dt = new DataTable();
            dt.Columns.AddRange(new DataColumn[3] {
                        new DataColumn("SiparişId"),
                        new DataColumn("Ürün"),
                        new DataColumn("Adet")});
            dt.Rows.Add(101, "Cam bardak", 5);
            dt.Rows.Add(102, "Pantolon", 2);
            dt.Rows.Add(103, "Tişört", 12);
            dt.Rows.Add(104, "Gömlek", 9);

            string dosyaAdi = "ornek_dosya_adi";
            var grid = new GridView();
            grid.DataSource = dt;
            grid.DataBind();

            Response.ClearContent();
            Response.Charset = "utf-8";
            Response.AddHeader("content-disposition", "attachment; filename=" + dosyaAdi + ".xls");

            Response.ContentType = "application/vnd.ms-excel";
            StringWriter sw = new StringWriter();
            HtmlTextWriter htw = new HtmlTextWriter(sw);

            grid.RenderControl(htw);

            Response.Write(sw.ToString());
            Response.End();

            return View();
        }

        public void GridExportToExcel()
        {
            DataTable dt = new DataTable();
            dt.Columns.AddRange(new DataColumn[3] {
                        new DataColumn("SiparişId"),
                        new DataColumn("Ürün"),
                        new DataColumn("Adet")});
            dt.Rows.Add(101, "Cam bardak", 5);
            dt.Rows.Add(102, "Pantolon", 2);
            dt.Rows.Add(103, "Tişört", 12);
            dt.Rows.Add(104, "Gömlek", 9);

            string dosyaAdi = "ornek_dosya_adi";
            var grid = new GridView();
            grid.DataSource = dt;
            grid.DataBind();

            Response.ClearContent();
            Response.Charset = "utf-8";
            Response.AddHeader("content-disposition", "attachment; filename=" + dosyaAdi + ".xls");

            Response.ContentType = "application/vnd.ms-excel";
            StringWriter sw = new StringWriter();
            HtmlTextWriter htw = new HtmlTextWriter(sw);

            grid.RenderControl(htw);

            Response.Write(sw.ToString());
            Response.End();
        }



        #region TEST DATATABLE GET
        private DataTable GetData()
        {
            //ModelFunctions f = new ModelFunctions();
            //DateTime startDate = DateTime.Now;
            // f.SqlConnOpen();
            //    DataTable dt = f.DataTable("select * from SubeSettings Where Status=1");
            //string constr = ConfigurationManager.ConnectionStrings["constr"].ConnectionString;
            string constr = "Data Source=" + WebConfigurationManager.AppSettings["Server"] + ";Initial Catalog=" + WebConfigurationManager.AppSettings["DBName"] + ";Persist Security Info=True;User Id=" + WebConfigurationManager.AppSettings["User"] + ";Password=" + WebConfigurationManager.AppSettings["Password"] + "; MultipleActiveResultSets=true";

            using (SqlConnection con = new SqlConnection(constr))
            {
                using (SqlCommand cmd = new SqlCommand("select * from SubeSettings Where Status=1"))
                {
                    using (SqlDataAdapter sda = new SqlDataAdapter())
                    {
                        cmd.Connection = con;
                        sda.SelectCommand = cmd;
                        using (DataTable dt = new DataTable())
                        {
                            sda.Fill(dt);
                            return dt;
                        }
                    }
                }
            }
        }
        #endregion


        #region EXPORT AND SEND EXPORT EXCEL DATA EKREM

       
        public void ExportExcel(  )
        {
            //List<KasaCiroReportSendMailViewModel> list = SendMailKasaCiroCRUD.List(Convert.ToDateTime("2019-09-01 00:00:00"), Convert.ToDateTime("2019-09-30 00:00:00"));
            //ListtoDataTableConverter converter = new ListtoDataTableConverter();

            //DataTable dt = converter.ToDataTable(list);



            ////Get the GridView Data from database.
            ////DataTable dt = GetData();

            ////Set DataTable Name which will be the name of Excel Sheet.
            //dt.TableName = "GridView_Data";
            ////Create a New Workbook.
            //using (XLWorkbook wb = new XLWorkbook())
            //{
            //    //Add the DataTable as Excel Worksheet.
            //    wb.Worksheets.Add(dt);

            //    using (MemoryStream memoryStream = new MemoryStream())
            //    {
            //        //Save the Excel Workbook to MemoryStream.
            //        wb.SaveAs(memoryStream);

            //        //Convert MemoryStream to Byte array.
            //        byte[] bytes = memoryStream.ToArray();
            //        memoryStream.Close();

            //        SmtpClient smtp = new SmtpClient();
            //        smtp.Host = "smtp.gmail.com";
            //        smtp.EnableSsl = true;
            //        System.Net.NetworkCredential credentials = new System.Net.NetworkCredential();
            //        credentials.UserName = "ekrem.demo06@gmail.com";
            //        credentials.Password = "mnbvcxzaqwerty.";
            //        smtp.UseDefaultCredentials = true;
            //        smtp.Credentials = credentials;
            //        smtp.Port = 587;

            //        //MailMessage mm = new MailMessage("ekrem.demo06@gmail.com", "mnbvcxzaqwerty.");
            //        MailMessage mm = new MailMessage();
            //        mm.From = new MailAddress("ekrem.demo06@gmail.com", "Vega Master Test");
            //        mm.Subject = "GridView Exported Excel";
            //        mm.Body = "GridView Exported Excel Attachment";
            //        //Add Byte array as Attachment.
            //        mm.Attachments.Add(new System.Net.Mail.Attachment(new MemoryStream(bytes), "GridView.xlsx"));
            //        mm.IsBodyHtml = true;
            //        mm.To.Add("ekremerkekli@gmail.com");
            //        smtp.Send(mm);                  
            //    }
            //}
        }
        #endregion

    }
}