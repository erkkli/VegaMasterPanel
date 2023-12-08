using ClosedXML.Excel;
using iTextSharp.text;
using iTextSharp.text.html.simpleparser;
using iTextSharp.text.pdf;
using SefimV2.Models;
using SefimV2.ViewModels.GetTime;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Net.Mail;
using System.Reflection;
using System.Text;
using System.Web.Hosting;

namespace SefimV2.Helper
{
    public class Singleton
    {
        #region Set StartDate and EndDate   

        public static TimeViewModel GetTimeViewModel(string StartDate_, string EndDate_, string durum)
        {
            TimeViewModel viewModel = new TimeViewModel();
            try
            {
                Saat saat = Ayarlar.SaatListe();

                #region DateFilter            

                string gun = (DateTime.Today.ToString("dd"));
                string ay = DateTime.Today.ToString("MM");
                string yil = DateTime.Today.ToString("yyyy");
                //string gunSonu_ = (DateTime.Now.AddDays(1).ToString("dd")); //Convert.ToInt32(gun) + 1;
                string a1 = DateTime.Now.AddDays(1).ToShortDateString();
                string gunSonuGun = Convert.ToDateTime(a1).ToString("dd");
                string gunSonuAy = Convert.ToDateTime(a1).ToString("MM");
                string gunsonuYil = Convert.ToDateTime(a1).ToString("yyyy");
                //DateTime date2h = new DateTime(Convert.ToInt32(yil), Convert.ToInt32(ay), Convert.ToInt32(gun), Convert.ToInt32(startHours), Convert.ToInt32(0),0);
                //string bugun = Convert.ToDateTime((DateTime.Today.ToShortDateString() + " " + saat.StartTime)).ToString("yyyy-MM-dd HH:mm");     //yil+ "-"+ay+"-"+gun+" "+startHours+":"+"00";//date2h.ToString() ;  //Convert.ToDateTime((DateTime.Today.ToShortDateString() + " " + saat.StartTime)).ToString("yyyy-MM-dd HH:mm");
                //string gunSonu = Convert.ToDateTime((DateTime.Now.AddDays(1).ToShortDateString() + " " + saat.EndTime)).ToString("yyyy-MM-dd HH:mm"); //yil + "-" + ay + "-" + gunSonu_ + " " + startHours + ":" + "00";//Convert.ToDateTime((DateTime.Now.AddDays(1).ToShortDateString() + " " + saat.EndTime)).ToString("yyyy-MM-dd HH:mm");
                string bugun = yil + "-" + ay + "-" + gun + " " + saat.StartTime;//date2h.ToString() ;  //Convert.ToDateTime((DateTime.Today.ToShortDateString() + " " + saat.StartTime)).ToString("yyyy-MM-dd HH:mm");
                string gunSonu = gunsonuYil + "-" + gunSonuAy + "-" + gunSonuGun + " " + saat.EndTime;//Convert.ToDateTime((DateTime.Now.AddDays(1).ToShortDateString() + " " + saat.EndTime)).ToString("yyyy-MM-dd HH:mm");
                                                                                                      //
                                                                                                      //System.Globalization.CultureInfo cultureinfo = new System.Globalization.CultureInfo("nl-NL");
                                                                                                      //DateTime dt = DateTime.Parse(date, cultureinfo);
                                                                                                      //
                #region Yeni Tarih Filtreleme 

                //ViewBag ile tarihler taşınıyor          
                viewModel.StartDate = bugun;
                viewModel.EndDate = gunSonu;

                DateTime now = DateTime.Now;
                var startDate = new DateTime(now.Year, now.Month, now.Day).ToString();
                var oncekiGun = now.AddDays(-1).ToString("yyyy-MM-dd") + " " + saat.StartTime;
                var oncekiGunSonu = now.AddDays(-1).ToString("yyyy-MM-dd") + " " + saat.EndTime;

                #endregion Yeni Tarih Filtreleme 

                if (durum.Equals("bugun"))
                {
                    viewModel.StartDate = bugun;
                    viewModel.EndDate = gunSonu;
                }
                if (durum.Equals("dun"))
                {
                    #region Yeni Tarih Filtreleme
                    bugun = now.AddDays(-1).ToString("yyyy-MM-dd") + " " + saat.StartTime;
                    gunSonu = now.AddDays(0).ToString("yyyy-MM-dd") + " " + saat.EndTime;
                    viewModel.StartDate = bugun;
                    viewModel.EndDate = gunSonu;
                    #endregion
                }
                if (durum.Equals("buAy"))
                {
                    #region Yeni Tarih filtreleme              
                    var startDate11 = new DateTime(now.Year, now.Month, 1);
                    var endDate11 = startDate11.AddMonths(1).AddDays(-1);
                    bugun = startDate11.ToString("yyyy-MM-dd") + " " + saat.StartTime;
                    gunSonu = endDate11.ToString("yyyy-MM-dd") + " " + saat.EndTime;

                    viewModel.StartDate = bugun;
                    viewModel.EndDate = gunSonu;
                    #endregion
                }
                if (durum.Equals("tarihAraligi"))
                {
                    //string saatNull = Convert.ToDateTime(StartDate_).ToString("HH");
                    //if (saatNull == "00")
                    //{
                    //    viewModel.StartDate = StartDate_ + " " + " 00:00";
                    //    viewModel.EndDate = EndDate_ + " " + " 23:59";
                    //}
                    //else
                    //{  Convert.ToDateTime(tarihAraligiEndDate.ToString()).ToString("dd'/'MM'/'yyyy HH:mm");
                    viewModel.StartDate = Convert.ToDateTime(StartDate_.ToString()).ToString("dd'/'MM'/'yyyy HH:mm");
                    viewModel.EndDate = Convert.ToDateTime(EndDate_.ToString()).ToString("dd'/'MM'/'yyyy HH:mm");

                    //Convert.ToDateTime(course.date.ToString()).ToString("dd'/'MM'/'yyyy");
                    //}
                }

                #endregion DateFilter
            }
            catch (Exception ex) { WritingLogFile("GetTimeViewModel", ex.Message.ToString()); }

            return viewModel;
        }

        #endregion Set StartDate and EndDate 

        #region Writter Log      

        public static string WritingLogFile(string ControllerName, string ex)
        {
            #region Writing Log 

            try
            {
                //log metnini oluştur
                string ErrorMessage = ControllerName + " <_HATA_>";
                string SystemErrorMessage = ex.ToString();
                string LogText = string.Empty;
                LogText += "Hata Zamanı: " + DateTime.Now.ToString() + Environment.NewLine;
                LogText += "Hata Mesajı: " + ErrorMessage + Environment.NewLine;
                LogText += "Hata Detay Açıklaması: " + SystemErrorMessage + Environment.NewLine;
                LogText += "------" + Environment.NewLine;
                string LogFolder = "/Uploads/Logs/Error";
                if (Directory.Exists(HostingEnvironment.MapPath(LogFolder)) == false) { Directory.CreateDirectory(HostingEnvironment.MapPath(LogFolder)); }
                string LogFile = ControllerName + ".txt";
                string LogFilePath = HostingEnvironment.MapPath(LogFolder + "/" + LogFile);

                if (File.Exists(LogFilePath) == false)
                {
                    string FirstLine = "Created on " + DateTime.Now.ToString() + Environment.NewLine + Environment.NewLine;
                    File.WriteAllText(LogFilePath, FirstLine);
                }
                File.AppendAllText(LogFilePath, LogText);
            }
            catch (Exception)
            {
                return "işlem Başarısız.";
            }

            #endregion Writing Log 

            return "işlem Başarılı.";
        }

        public static string WritingLogFile2(string ControllerName, string Exception, string DateTimeFormat, string StackTrace)
        {
            #region Writing Log           
            try
            {
                string errorMessage = ControllerName + " Hata Oluştu";
                string systemErrorMessage = Exception.ToString();
                string logText = "-----------------Log Baslangıç" + Environment.NewLine;
                logText += "Hata Zamanı : " + DateTime.Now.ToString() + Environment.NewLine;
                logText += "Hata Mesajı : " + errorMessage + Environment.NewLine;
                logText += "Hata Açıklaması : " + systemErrorMessage + Environment.NewLine;
                logText += " Tarih Formatları " + DateTimeFormat + Environment.NewLine;
                logText += "InnerException: " + StackTrace + Environment.NewLine;
                logText += "-----------------Log Bıtıs" + Environment.NewLine;
                string logFolder = "/Uploads/Logs/Error";
                if (Directory.Exists(HostingEnvironment.MapPath(logFolder)) == false) { Directory.CreateDirectory(HostingEnvironment.MapPath(logFolder)); }
                string logFile = ControllerName + ".txt";
                string logFilePath = HostingEnvironment.MapPath(logFolder + "/" + logFile);

                if (File.Exists(logFilePath) == false)
                {
                    string firstLine = "Created on " + DateTime.Now.ToString() + Environment.NewLine + Environment.NewLine;
                    File.WriteAllText(logFilePath, firstLine);
                }
                File.AppendAllText(logFilePath, logText);
            }
            catch (Exception) { }

            #endregion  Writing Log   

            return ControllerName;
        }

        public static string WritingLog(string pages, string logText)
        {
            try
            {
                #region EX                         

                string LogText = string.Empty;
                LogText += "DateTime: " + DateTime.Now.ToString() + Environment.NewLine;
                LogText += "ActionName: " + pages + Environment.NewLine;
                LogText += "LogText: " + logText + Environment.NewLine;
                LogText += "-----------------" + Environment.NewLine;
                string LogFolder = "/Uploads/Logs/Error";
                if (Directory.Exists(HostingEnvironment.MapPath(LogFolder)) == false) { Directory.CreateDirectory(HostingEnvironment.MapPath(LogFolder)); }
                string LogFile = pages + ".txt";
                string LogFilePath = HostingEnvironment.MapPath(LogFolder + "/" + LogFile);
                if (File.Exists(LogFilePath) == false)
                {
                    string FirstLine = "Created on " + DateTime.Now.ToString() + Environment.NewLine;
                    File.WriteAllText(LogFilePath, FirstLine);
                }
                File.AppendAllText(LogFilePath, LogText);

                #endregion EX
            }
            catch (Exception) { }

            return "";
        }

        public void CreateLog(string ePostaAdress, string pages)
        {
            try
            {
                #region LOG               
                //log metnini oluştur
                string ErrorMessage = "E-Posta Gonderme ";
                //string SystemErrorMessage = ex.Message.ToString();
                string LogText = "";
                LogText += "-----------------" + Environment.NewLine;
                LogText += "Gönderim Zamanı: " + DateTime.Now.ToString() + Environment.NewLine;
                LogText += "Gönderilen Rapor: " + pages + " Rapor " + ErrorMessage + Environment.NewLine;
                //LogText += "Hata Açıklaması : " + SystemErrorMessage + Environment.NewLine;             
                LogText += "Gönderilen E-Posta Adresi: " + ePostaAdress + Environment.NewLine;
                LogText += "-----------------" + Environment.NewLine;
                //KisiOrtalamaReportSendMailViewModel items = new KisiOrtalamaReportSendMailViewModel();
                //items.CustomerName = cu;
                //items.Debit = SubeId;
                //items.ErrorMessage = ErrorMessage;
                //items.ErrorStatus = true;
                //items.ErrorCode = "01";

                //// bağlantı hatasında eklenecek
                //items.Sube = SubeAdi + " (Erişim Yok)";
                ////items.SubeID = Convert.ToInt32(SubeId);
                //items.Kisi = 0;//f.RTI(SubeR, "ADET");
                //               //items.Total = 0;// f.RTD(SubeR, "TUTAR");
                //items.Ortalama = 0; //f.RTI(SubeR, "PhoneOrderDebit");
                //                    //
                //Liste.Add(items);

                string LogFolder = "/Uploads/Logs/Error";
                if (Directory.Exists(HostingEnvironment.MapPath(LogFolder)) == false) { Directory.CreateDirectory(HostingEnvironment.MapPath(LogFolder)); }
                string LogFile = "SendMail-" + "" + ".txt";
                string LogFilePath = HostingEnvironment.MapPath(LogFolder + "/" + LogFile);

                if (File.Exists(LogFilePath) == false)
                {
                    string FirstLine = "Created on " + DateTime.Now.ToString() + Environment.NewLine + Environment.NewLine;
                    File.WriteAllText(LogFilePath, FirstLine);
                }
                File.AppendAllText(LogFilePath, LogText);
                #endregion
            }
            catch (Exception)
            { }
        }

        #endregion Writter Log

        #region LİST TO DATATABLE

        //List<SubeCiro> list = SubeCiroCRUD.List(Convert.ToDateTime(viewModel.StartDate), Convert.ToDateTime(viewModel.EndDate));
        //public static List<SubeCiro> ConvertDataTable<T>(DataTable dt)
        //{
        //    string Id = Request.Cookies["PRAUT"].Value;
        //    //List<T> data = new List<T>();
        //    List<SubeCiro> data = SubeCiroCRUD.List(Convert.ToDateTime("2019-09-01 00:00:00"), Convert.ToDateTime("2019-09-30 00:00:00"));
        //    foreach (DataRow row in dt.Rows)
        //    {
        //        SubeCiro item = GetItem<SubeCiro>(row);
        //        data.Add(item);
        //    }
        //    return data;
        //}
        private static T GetItem<T>(DataRow dr)
        {
            Type temp = typeof(T);
            T obj = Activator.CreateInstance<T>();

            foreach (DataColumn column in dr.Table.Columns)
            {
                foreach (PropertyInfo pro in temp.GetProperties())
                {
                    if (pro.Name == column.ColumnName)
                        pro.SetValue(obj, dr[column.ColumnName], null);
                    else
                        continue;
                }
            }
            return obj;
        }

        public class ListtoDataTableConverter
        {
            public DataTable ToDataTable<T>(List<T> items)
            {
                DataTable dataTable = new DataTable(typeof(T).Name);

                //Get all the properties
                PropertyInfo[] Props = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);

                foreach (PropertyInfo prop in Props)
                {
                    //Setting column names as Property names
                    dataTable.Columns.Add(prop.Name);
                }

                foreach (T item in items)
                {
                    var values = new object[Props.Length];
                    for (int i = 0; i < Props.Length; i++)
                    {
                        //inserting property values to datatable rows
                        values[i] = Props[i].GetValue(item, null);
                    }

                    dataTable.Rows.Add(values);
                }

                //put a breakpoint here and check datatable

                return dataTable;
            }
        }

        #endregion LİST TO DATATABLE

        #region export medot

        public void ExportExcel(DataTable dt, string ePostaAdress, string Pages, string StartDate, string EndDate)
        {
            ////List<KasaCiroReportSendMailViewModel> list = SendMailKasaCiroCRUD.List(Convert.ToDateTime("2019-09-01 00:00:00"), Convert.ToDateTime("2019-09-30 00:00:00"));
            ////ListtoDataTableConverter converter = new ListtoDataTableConverter();
            ////DataTable dt = converter.ToDataTable(list);
            //Get the GridView Data from database.
            //DataTable dt = GetData();

            try
            {
                #region LOG
                CreateLog(ePostaAdress, Pages);
                #endregion  LOG
                //Set DataTable Name which will be the name of Excel Sheet.
                dt.TableName = Pages;
                //Create a New Workbook.
                using (XLWorkbook wb = new XLWorkbook())
                {
                    //Add the DataTable as Excel Worksheet.
                    wb.Worksheets.Add(dt);

                    using (MemoryStream memoryStream = new MemoryStream())
                    {
                        //Save the Excel Workbook to MemoryStream.
                        wb.SaveAs(memoryStream);

                        //Convert MemoryStream to Byte array.
                        byte[] bytes = memoryStream.ToArray();
                        memoryStream.Close();

                        SmtpClient smtp = new SmtpClient();
                        smtp.Host = "smtp.gmail.com";
                        smtp.EnableSsl = true;
                        System.Net.NetworkCredential credentials = new System.Net.NetworkCredential();
                        credentials.UserName = "ekrem.demo06@gmail.com";
                        credentials.Password = "mnbvcxzaqwerty.";
                        smtp.UseDefaultCredentials = true;
                        smtp.Credentials = credentials;
                        smtp.Port = 587;

                        //MailMessage mm = new MailMessage("ekrem.demo06@gmail.com", "mnbvcxzaqwerty.");
                        MailMessage mm = new MailMessage();
                        mm.From = new MailAddress("ekrem.demo06@gmail.com", "VEGA MASTER RAPOR");
                        mm.Subject = "Vega Master " + StartDate + " - " + EndDate + " Tarihler Arası " + Pages + " Raporu";
                        mm.Body = StartDate + " - " + EndDate + " Tarihler Arası " + Pages + " Raporu";
                        //Add Byte array as Attachment.
                        mm.Attachments.Add(new System.Net.Mail.Attachment(new MemoryStream(bytes), Pages + ".xlsx"));
                        mm.IsBodyHtml = true;
                        mm.To.Add(ePostaAdress);
                        smtp.Send(mm);
                    }
                }
            }
            catch (Exception ex)
            {
                #region LOG               
                //log metnini oluştur
                string ErrorMessage = "E-Posta Hata ";
                string SystemErrorMessage = ex.Message.ToString();
                string LogText = "";
                LogText += "Hata Zamanı : " + DateTime.Now.ToString() + Environment.NewLine;
                LogText += "Hata Mesajı : " + ErrorMessage + Environment.NewLine;
                LogText += "Hata Açıklaması : " + SystemErrorMessage + Environment.NewLine;
                LogText += "-----------------" + Environment.NewLine;
                LogText += "E-Posta:  " + ePostaAdress + Environment.NewLine;

                //KisiOrtalamaReportSendMailViewModel items = new KisiOrtalamaReportSendMailViewModel();
                //items.CustomerName = cu;
                //items.Debit = SubeId;
                //items.ErrorMessage = ErrorMessage;
                //items.ErrorStatus = true;
                //items.ErrorCode = "01";

                //// bağlantı hatasında eklenecek
                //items.Sube = SubeAdi + " (Erişim Yok)";
                ////items.SubeID = Convert.ToInt32(SubeId);
                //items.Kisi = 0;//f.RTI(SubeR, "ADET");
                //               //items.Total = 0;// f.RTD(SubeR, "TUTAR");
                //items.Ortalama = 0; //f.RTI(SubeR, "PhoneOrderDebit");
                //                    //
                //Liste.Add(items);

                string LogFolder = "/Uploads/Logs/Error";
                if (Directory.Exists(HostingEnvironment.MapPath(LogFolder)) == false) { Directory.CreateDirectory(HostingEnvironment.MapPath(LogFolder)); }
                string LogFile = "SendMail-Error-" + "" + ".txt";
                string LogFilePath = HostingEnvironment.MapPath(LogFolder + "/" + LogFile);

                if (File.Exists(LogFilePath) == false)
                {
                    string FirstLine = "Created on " + DateTime.Now.ToString() + Environment.NewLine + Environment.NewLine;
                    File.WriteAllText(LogFilePath, FirstLine);
                }
                File.AppendAllText(LogFilePath, LogText);
                #endregion
            }
        }

        #endregion  export medot

        public void SendPDFEmail(DataTable dt)
        {
            string companyName = "ReportNameEkrm";
            int orderNo = 2303;
            StringBuilder sb = new StringBuilder();
            sb.Append("<table width='100%' cellspacing='0' cellpadding='2'>");
            sb.Append("<tr><td align='center' style='background-color: #18B5F0' colspan = '2'><b>Sipariş Tablosu</b></td></tr>");
            sb.Append("<tr><td colspan = '2'></td></tr>");
            sb.Append("<tr><td><b>Sipariş Numarası:</b>");
            sb.Append(orderNo);
            sb.Append("</td><td><b>Tarih: </b>");
            sb.Append(DateTime.Now);
            sb.Append(" </td></tr>");
            sb.Append("<tr><td colspan = '2'><b>Şirket Adı :</b> ");
            sb.Append(companyName);
            sb.Append("</td></tr>");
            sb.Append("</table>");
            sb.Append("<br />");
            sb.Append("<table border = '1'>");
            sb.Append("<tr>");
            foreach (DataColumn column in dt.Columns)
            {
                sb.Append("<th style = 'background-color: #D20B0C;color:#ffffff'>");
                sb.Append(column.ColumnName);
                sb.Append("</th>");
            }
            sb.Append("</tr>");
            foreach (DataRow row in dt.Rows)
            {
                sb.Append("<tr>");
                foreach (DataColumn column in dt.Columns)
                {
                    sb.Append("<td>");
                    sb.Append(row[column]);
                    sb.Append("</td>");
                }
                sb.Append("</tr>");
            }
            sb.Append("</table>");
            StringReader sr = new StringReader(sb.ToString());

            Document pdfDoc = new Document(PageSize.A4, 10f, 10f, 10f, 0f);
            HTMLWorker htmlparser = new HTMLWorker(pdfDoc);

            #region Türkçe karakter sorunu için yazılması gereken kod bloğu.

            FontFactory.Register(Path.Combine("C:\\Windows\\Fonts\\Arial.ttf"), "Garamond"); // kendi türkçe karakter desteği olan fontunuzu da girebilirsiniz.
            StyleSheet css = new StyleSheet();
            css.LoadTagStyle("body", "face", "Garamond");
            css.LoadTagStyle("body", "encoding", "Identity-H");
            css.LoadTagStyle("body", "size", "12pt");
            htmlparser.SetStyleSheet(css);

            #endregion

            using (MemoryStream memoryStream = new MemoryStream())
            {
                PdfWriter writer = PdfWriter.GetInstance(pdfDoc, memoryStream);
                pdfDoc.Open();
                htmlparser.Parse(sr);
                pdfDoc.Close();
                byte[] bytes = memoryStream.ToArray();
                memoryStream.Close();
                //return File(memoryStream.ToArray(), "application/pdf", "TestPDF.pdf");
            }
        }

        public class AyList
        {
            public String MonthYear;
            public String Month;
            public String Year;
            public String SqlScriptStartDate;
            public String SqlScriptEndDate;
            public AyList(String _MonthYear, String _Month, String _Year, String _SqlScriptStartDate, String _SqlScriptEndDate)
            {
                Month = _Month;
                Year = _Year;
                MonthYear = _MonthYear;
                SqlScriptStartDate = _SqlScriptStartDate;
                SqlScriptEndDate = _SqlScriptEndDate;
            }
        }

        public static List<MasaUstuRaparuDateModel> SetMonthYear()
        {
            List<MasaUstuRaparuDateModel> SetMonthYearList = new List<MasaUstuRaparuDateModel>();
            List<AyList> m_AyList = new List<AyList>();

            for (int i = 0; i < 12; i++)
            {
                MasaUstuRaparuDateModel masaUstuRaparuDate = new MasaUstuRaparuDateModel();
                String __Hour = DateTime.Now.ToLongTimeString();
                DateTime __DateTimeAy = DateTime.Now.AddMonths(-i);
                String __Today = __DateTimeAy.Day.ToString();
                String __Month = __DateTimeAy.Month.ToString();
                String __Year = __DateTimeAy.Year.ToString();
                String __DayInMonth = DateTime.DaysInMonth(Convert.ToInt32(__Year), Convert.ToInt32(__Month)).ToString();
                String __MonthYear = __Month + "-" + __Year;
                String __SqlScriptStartDate = __Year + "-" + __Month + "-" + "01" + " 00:00:00";
                String __SqlScriptEndDate = __Year + "-" + __Month + "-" + __DayInMonth + " 23:59:59";

                if (i == 0)
                {
                    __SqlScriptEndDate = __Year + "-" + __Month + "-" + __Today + " " + __Hour;
                }

                m_AyList.Add(new AyList(__MonthYear, __Month, __Year, __SqlScriptStartDate, __SqlScriptEndDate));
                masaUstuRaparuDate.Month = __Month;
                masaUstuRaparuDate.MonthYear = __MonthYear;
                masaUstuRaparuDate.SqlScriptStartDate = __SqlScriptStartDate;
                masaUstuRaparuDate.SqlScriptEndDate = __SqlScriptEndDate;
                masaUstuRaparuDate.Year = __Year;
                SetMonthYearList.Add(masaUstuRaparuDate);
            }

            //Session["AyList"] = m_AyList;

            return SetMonthYearList;
        }
        //List<AyList> AyList = (List<AyList>)Session["AyList"];

        public class MonthDayList
        {
            public String DayMonthYear;
            public String SqlScriptStartDate;
            public String SqlScriptEndDate;
            public MonthDayList(String _DayMonthYear, String _SqlScriptStartDate, String _SqlScriptEndDate)
            {
                DayMonthYear = _DayMonthYear;
                SqlScriptStartDate = _SqlScriptStartDate;
                SqlScriptEndDate = _SqlScriptEndDate;
            }

        }

        public static List<MonthDayList> SetMonthDayList()
        {
            DateTime __DateTimeAy = DateTime.Now;
            String __Month = __DateTimeAy.Month.ToString();
            String __Year = __DateTimeAy.Year.ToString();
            String __LastMonthDay = DateTime.DaysInMonth(Convert.ToInt32(__Year), Convert.ToInt32(__Month)).ToString();
            String __StartDate = __Year + "-" + __Month + "-01 00:00:00";
            String __EndDate = __Year + "-" + __Month + "-" + __LastMonthDay + " 23:59:59";

            //DataTable __DateSubeProduct = GeneralDBOperation.GetDateSubeProduct(__StartDate, __EndDate, SubeName);

            List<MonthDayList> m_MonthDayList = new List<MonthDayList>();
            DateTime __DateTime = DateTime.Now.AddDays(1);
            int __Day = __DateTime.Day;
            for (int i = 1; i < __Day; i++)
            {
                String __DayMonthYear = i + "." + __Month + "." + __Year;
                String __SqlScriptStartDate = __Year + "-" + __Month + "-" + i;
                String __SqlScriptEndDate = __Year + "-" + __Month + "-" + i;
                m_MonthDayList.Add(new MonthDayList(__DayMonthYear, __SqlScriptStartDate, __SqlScriptEndDate));
            }
            //Session["MonthDayList"] = m_MonthDayList;
            return m_MonthDayList;
        }


        #region Banka Id'leri

        private static readonly string[][] BANKALAR = new string[][]
           {
                new string[] {"A-BANK", "124"},
                new string[] {"AKBANK", "46"},
                new string[] {"AKTIFBANK", "143"},
                new string[] {"ALBARAKA", "203"},
                new string[] {"BANK ASYA", "208"},
                new string[] {"CITIBANK", "92"},
                new string[] {"DENIZ", "134"},
                new string[] {"FINANSBANK", "111"},
                new string[] {"FORTIS", "71"},
                new string[] {"GARANTI", "62"},
                new string[] {"HALKBANK", "12"},
                new string[] {"HSBC", "123"},
                new string[] {"ISBANK", "64"},
                new string[] {"SEKERBANK", "59"},
                new string[] {"SEKERSIRKET", "9009"},
                new string[] {"TEB", "32"},
                new string[] {"TFKB", "206"},
                new string[] {"VAKIF", "15"},
                new string[] {"YKB", "67"},
                new string[] {"ZIRAATBANK", "10"},
                new string[] {"ADABANK", "100"},
                new string[] {"ING BANK", "99"},
           };
        #endregion
    }
}