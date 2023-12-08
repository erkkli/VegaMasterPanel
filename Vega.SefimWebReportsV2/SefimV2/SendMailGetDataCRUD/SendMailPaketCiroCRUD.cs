using SefimV2.Models;
using SefimV2.ViewModels.User;
using SefimV2.ViewModelSendMail.PaketCiroReportSendMail;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Hosting;

namespace SefimV2.SendMailGetDataCRUD
{
    public class SendMailPaketCiroCRUD
    {
        public static List<PaketCiroReportSendMailViewModel> List(DateTime Date1, DateTime Date2, string ID)
        {
            List<PaketCiroReportSendMailViewModel> Liste = new List<PaketCiroReportSendMailViewModel>();
            ModelFunctions f = new ModelFunctions();
            DateTime startDate = DateTime.Now;
            #region GİRİŞ YAPAN KULLANICININ YETKİLİ OLDUĞU ŞUBELER LİSTELENİYOR
            UserViewModel model = UsersListCRUD.YetkiliSubesi(ID);
            #endregion
            try
            {
                f.SqlConnOpen();
                DataTable dt = f.DataTable("select * from SubeSettings Where Status=1");
                TimeSpan sure = DateTime.Now - startDate;

                foreach (DataRow r in dt.Rows)
                {
                    string SubeId = f.RTS(r, "Id");
                    string SubeAdi = f.RTS(r, "SubeName");
                    string SubeIP = f.RTS(r, "SubeIP");
                    string SqlName = f.RTS(r, "SqlName");
                    string SqlPassword = f.RTS(r, "SqlPassword");
                    string DBName = f.RTS(r, "DBName");
                    string QueryTimeStart = Date1.ToString("yyyy-MM-dd HH:mm:ss");
                    string QueryTimeEnd = Date2.ToString("yyyy-MM-dd HH:mm:ss");


                    #region  SEFİM YENI - ESKİ FASTER SQL
                    string AppDbType = f.RTS(r, "AppDbType");
                    string Query = "";
                    if (AppDbType == "1")// 1 = yeni şefim, 2 =eski Şefim, 3 = Faster
                    {
                        Query = File.ReadAllText(HostingEnvironment.MapPath("/Sql/SqlNewSef/PaketCiroNewSefim.sql"), System.Text.Encoding.UTF8);
                    }
                    else if (AppDbType == "2")
                    {
                        Query = File.ReadAllText(HostingEnvironment.MapPath("/Sql/PaketCiro.sql"), System.Text.Encoding.UTF8);
                    }
                    else if (AppDbType == "3")
                    {
                        //Query = File.ReadAllText(HostingEnvironment.MapPath("/Sql/SqlFaster/SubeToplamCiroFASTER.sql"), System.Text.Encoding.UTF8);
                    }
                    #endregion                   
                   
                    Query = Query.Replace("{TARIH1}", QueryTimeStart);
                    Query = Query.Replace("{TARIH2}", QueryTimeEnd);
                    Query = Query.Replace("{SUBE}", SubeAdi);

                    if (ID == "1")
                    {
                        #region GET DATA
                        try
                        {
                            string connString = "Provider=SQLOLEDB;Server=" + SubeIP + ";User Id=" + SqlName + ";Password=" + SqlPassword + ";Database=" + DBName + "";
                            f.SqlConnOpen(true, connString);
                            DataTable AcikHesapDt = f.DataTable(Query, true);
                            foreach (DataRow SubeR in AcikHesapDt.Rows)
                            {
                                PaketCiroReportSendMailViewModel items = new PaketCiroReportSendMailViewModel();
                                items.Sube = SubeAdi;
                                //items.SubeID = Convert.ToInt32(SubeId);
                                //items.Adet = f.RTI(SubeR, "ADET");
                                //items.Debit = f.RTD(SubeR, "TUTAR");
                                //items.PhoneOrderDebit = f.RTI(SubeR, "PhoneOrderDebit");

                                items.Adet = f.RTI(SubeR, "OrderCount");
                                items.Toplam = f.RTD(SubeR, "Total");
                                items.AcikHesap = f.RTD(SubeR, "Debit");
                                items.Nakit = f.RTD(SubeR, "CashPayment");
                                items.Kredi = f.RTD(SubeR, "CreditPayment");
                                items.YemekKarti = f.RTD(SubeR, "TicketPayment");
                                items.İndirim = f.RTD(SubeR, "Discount");
                                items.AlinanTahsilat = f.RTD(SubeR, "CollectedTotal");
                                items.KalanTahsilat = f.RTD(SubeR, "Balance");

                                Liste.Add(items);
                            }
                            f.SqlConnClose(true);
                        }
                        catch (System.Exception ex)
                        {
                            //log metnini oluştur
                            string ErrorMessage = "Kasa Raporu Alınamadı.";
                            string SystemErrorMessage = ex.Message.ToString();
                            string LogText = "";
                            LogText += "Hata Zamanı : " + DateTime.Now.ToString() + Environment.NewLine;
                            LogText += "Hata Mesajı : " + ErrorMessage + Environment.NewLine;
                            LogText += "Hata Açıklaması : " + SystemErrorMessage + Environment.NewLine;
                            LogText += "-----------------" + Environment.NewLine;

                            PaketCiroReportSendMailViewModel items = new PaketCiroReportSendMailViewModel();

                            items.Sube = SubeAdi + " (Erişim veya Data Yok)";
                            //items.SubeID = Convert.ToInt32(SubeId);

                            //items.CustomerName = cu;
                            //items.Debit = SubeId;
                            //items.ErrorMessage = ErrorMessage;
                            //items.ErrorStatus = true;
                            //items.ErrorCode = "01";

                            // bağlantı hatasında eklenecek
                            items.Sube = SubeAdi + " (Erişim Yok)";
                            //items.SubeID = Convert.ToInt32(SubeId);
                            items.Adet = 0;//f.RTI(SubeR, "ADET");
                            items.AcikHesap = 0;// f.RTD(SubeR, "TUTAR");
                                                //items.PhoneOrderDebit = 0; //f.RTI(SubeR, "PhoneOrderDebit");
                                                //
                            Liste.Add(items);


                            string LogFolder = "/Uploads/Logs/Error";
                            if (Directory.Exists(HostingEnvironment.MapPath(LogFolder)) == false) { Directory.CreateDirectory(HostingEnvironment.MapPath(LogFolder)); }
                            string LogFile = "Sube-" + SubeId + ".txt";
                            string LogFilePath = HostingEnvironment.MapPath(LogFolder + "/" + LogFile);


                            if (File.Exists(LogFilePath) == false)
                            {
                                string FirstLine = "Created on " + DateTime.Now.ToString() + Environment.NewLine + Environment.NewLine;
                                File.WriteAllText(LogFilePath, FirstLine);
                            }
                            File.AppendAllText(LogFilePath, LogText);
                        }
                        #endregion
                    }
                    else
                    {
                        #region KULLANICININ ŞUBE YETKİ KONTROLU YAPILIYOR             
                        foreach (var item in model.FR_SubeListesi)
                        {
                            if (item.SubeID == Convert.ToInt32(SubeId))
                            {
                                #region GET DATA
                                try
                                {
                                    string connString = "Provider=SQLOLEDB;Server=" + SubeIP + ";User Id=" + SqlName + ";Password=" + SqlPassword + ";Database=" + DBName + "";
                                    f.SqlConnOpen(true, connString);
                                    DataTable AcikHesapDt = f.DataTable(Query, true);
                                    foreach (DataRow SubeR in AcikHesapDt.Rows)
                                    {
                                        PaketCiroReportSendMailViewModel items = new PaketCiroReportSendMailViewModel();
                                        items.Sube = SubeAdi;
                                        //items.SubeID = Convert.ToInt32(SubeId);
                                        //items.Adet = f.RTI(SubeR, "ADET");
                                        //items.Debit = f.RTD(SubeR, "TUTAR");
                                        //items.PhoneOrderDebit = f.RTI(SubeR, "PhoneOrderDebit");

                                        items.Adet = f.RTI(SubeR, "OrderCount");
                                        items.Toplam = f.RTD(SubeR, "Total");
                                        items.AcikHesap = f.RTD(SubeR, "Debit");
                                        items.Nakit = f.RTD(SubeR, "CashPayment");
                                        items.Kredi = f.RTD(SubeR, "CreditPayment");
                                        items.YemekKarti = f.RTD(SubeR, "TicketPayment");
                                        items.İndirim = f.RTD(SubeR, "Discount");
                                        items.AlinanTahsilat = f.RTD(SubeR, "CollectedTotal");
                                        items.KalanTahsilat = f.RTD(SubeR, "Balance");

                                        Liste.Add(items);
                                    }
                                    f.SqlConnClose(true);
                                }
                                catch (System.Exception ex)
                                {
                                    //log metnini oluştur
                                    string ErrorMessage = "Kasa Raporu Alınamadı.";
                                    string SystemErrorMessage = ex.Message.ToString();
                                    string LogText = "";
                                    LogText += "Hata Zamanı : " + DateTime.Now.ToString() + Environment.NewLine;
                                    LogText += "Hata Mesajı : " + ErrorMessage + Environment.NewLine;
                                    LogText += "Hata Açıklaması : " + SystemErrorMessage + Environment.NewLine;
                                    LogText += "-----------------" + Environment.NewLine;

                                    PaketCiroReportSendMailViewModel items = new PaketCiroReportSendMailViewModel();

                                    items.Sube = SubeAdi + " (Erişim veya Data Yok)";
                                    //items.SubeID = Convert.ToInt32(SubeId);

                                    //items.CustomerName = cu;
                                    //items.Debit = SubeId;
                                    //items.ErrorMessage = ErrorMessage;
                                    //items.ErrorStatus = true;
                                    //items.ErrorCode = "01";

                                    // bağlantı hatasında eklenecek
                                    items.Sube = SubeAdi + " (Erişim Yok)";
                                    //items.SubeID = Convert.ToInt32(SubeId);
                                    items.Adet = 0;//f.RTI(SubeR, "ADET");
                                    items.AcikHesap = 0;// f.RTD(SubeR, "TUTAR");
                                                        //items.PhoneOrderDebit = 0; //f.RTI(SubeR, "PhoneOrderDebit");
                                                        //
                                    Liste.Add(items);

                                    string LogFolder = "/Uploads/Logs/Error";
                                    if (Directory.Exists(HostingEnvironment.MapPath(LogFolder)) == false) { Directory.CreateDirectory(HostingEnvironment.MapPath(LogFolder)); }
                                    string LogFile = "Sube-" + SubeId + ".txt";
                                    string LogFilePath = HostingEnvironment.MapPath(LogFolder + "/" + LogFile);


                                    if (File.Exists(LogFilePath) == false)
                                    {
                                        string FirstLine = "Created on " + DateTime.Now.ToString() + Environment.NewLine + Environment.NewLine;
                                        File.WriteAllText(LogFilePath, FirstLine);
                                    }
                                    File.AppendAllText(LogFilePath, LogText);
                                }
                                #endregion
                            }
                        }
                        #endregion
                    }
                }
                f.SqlConnClose();
            }
            catch (DataException ex) { }

            return Liste;

        }
    }
}