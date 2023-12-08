using SefimV2.Models;
using SefimV2.ViewModels.User;
using SefimV2.ViewModelSendMail.KisiOrtalamaReportSendMail;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Hosting;

namespace SefimV2.SendMailGetDataCRUD
{
    public class SendMailKisiOrtalamaCRUD
    {
        public static List<KisiOrtalamaReportSendMailViewModel> List(DateTime Date1, DateTime Date2, string ID)
        {
            List<KisiOrtalamaReportSendMailViewModel> Liste = new List<KisiOrtalamaReportSendMailViewModel>();
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
                    string Query = File.ReadAllText(HostingEnvironment.MapPath("/Sql/KisiOrtalamaRaporlar.sql"), System.Text.Encoding.UTF8);
                    Query = Query.Replace("{TARIH1}", QueryTimeStart);
                    Query = Query.Replace("{TARIH2}", QueryTimeEnd);
                    Query = Query.Replace("{SUBE}", SubeAdi);


                    if (ID == "1")
                    {
                        #region MyRegion                   
                        try
                        {
                            string connString = "Provider=SQLOLEDB;Server=" + SubeIP + ";User Id=" + SqlName + ";Password=" + SqlPassword + ";Database=" + DBName + "";
                            f.SqlConnOpen(true, connString);
                            DataTable AcikHesapDt = f.DataTable(Query, true);
                            foreach (DataRow SubeR in AcikHesapDt.Rows)
                            {
                                KisiOrtalamaReportSendMailViewModel items = new KisiOrtalamaReportSendMailViewModel();
                                items.Sube = SubeAdi;
                                //items.SubeID = Convert.ToInt32(SubeId);
                                //items.Adet = f.RTI(SubeR, "ADET");
                                //items.Debit = f.RTD(SubeR, "TUTAR");
                                //items.PhoneOrderDebit = f.RTI(SubeR, "PhoneOrderDebit");
                                items.Kisi = f.RTI(SubeR, "Kisi");
                                items.Toplam = f.RTD(SubeR, "Total");
                                items.Ortalama = f.RTD(SubeR, "Ortalama");
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

                            KisiOrtalamaReportSendMailViewModel items = new KisiOrtalamaReportSendMailViewModel();
                            //items.CustomerName = cu;
                            //items.Debit = SubeId;
                            //items.ErrorMessage = ErrorMessage;
                            //items.ErrorStatus = true;
                            //items.ErrorCode = "01";

                            // bağlantı hatasında eklenecek
                            items.Sube = SubeAdi + " (Erişim Yok)";
                            //items.SubeID = Convert.ToInt32(SubeId);
                            items.Kisi = 0;//f.RTI(SubeR, "ADET");
                                           //items.Total = 0;// f.RTD(SubeR, "TUTAR");
                            items.Ortalama = 0; //f.RTI(SubeR, "PhoneOrderDebit");
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
                                #region MyRegion                   
                                try
                                {
                                    string connString = "Provider=SQLOLEDB;Server=" + SubeIP + ";User Id=" + SqlName + ";Password=" + SqlPassword + ";Database=" + DBName + "";
                                    f.SqlConnOpen(true, connString);
                                    DataTable AcikHesapDt = f.DataTable(Query, true);
                                    foreach (DataRow SubeR in AcikHesapDt.Rows)
                                    {
                                        KisiOrtalamaReportSendMailViewModel items = new KisiOrtalamaReportSendMailViewModel();
                                        items.Sube = SubeAdi;
                                        //items.SubeID = Convert.ToInt32(SubeId);
                                        //items.Adet = f.RTI(SubeR, "ADET");
                                        //items.Debit = f.RTD(SubeR, "TUTAR");
                                        //items.PhoneOrderDebit = f.RTI(SubeR, "PhoneOrderDebit");
                                        items.Kisi = f.RTI(SubeR, "Kisi");
                                        items.Toplam = f.RTD(SubeR, "Total");
                                        items.Ortalama = f.RTD(SubeR, "Ortalama");
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

                                    KisiOrtalamaReportSendMailViewModel items = new KisiOrtalamaReportSendMailViewModel();
                                    //items.CustomerName = cu;
                                    //items.Debit = SubeId;
                                    //items.ErrorMessage = ErrorMessage;
                                    //items.ErrorStatus = true;
                                    //items.ErrorCode = "01";

                                    // bağlantı hatasında eklenecek
                                    items.Sube = SubeAdi + " (Erişim Yok)";
                                    //items.SubeID = Convert.ToInt32(SubeId);
                                    items.Kisi = 0;//f.RTI(SubeR, "ADET");
                                                   //items.Total = 0;// f.RTD(SubeR, "TUTAR");
                                    items.Ortalama = 0; //f.RTI(SubeR, "PhoneOrderDebit");           //
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