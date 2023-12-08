using SefimV2.Models;
using SefimV2.ViewModels.User;
using SefimV2.ViewModelSendMail.AcikHesapReportSendMail;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Hosting;

namespace SefimV2.SendMailGetDataCRUD
{
    public class SendMailAcikHesapCRUD
    {

        public static List<AcikHesapReportSendMailViewModel> List(DateTime Date1, DateTime Date2, string subeid, string ID)
        {
            List<AcikHesapReportSendMailViewModel> Liste = new List<AcikHesapReportSendMailViewModel>();
            ModelFunctions f = new ModelFunctions();
            DateTime startDate = DateTime.Now;
            #region GİRİŞ YAPAN KULLANICININ YETKİLİ OLDUĞU ŞUBELER LİSTELENİYOR
            UserViewModel model = UsersListCRUD.YetkiliSubesi(ID);
            #endregion

            try
            {
                f.SqlConnOpen();
                string filter = "";
                if (!subeid.Equals(""))
                    filter += " and Id=" + subeid;
                DataTable dt = f.DataTable("select * from SubeSettings Where Status=1" + filter);
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
                        Query = File.ReadAllText(HostingEnvironment.MapPath("/Sql/SqlNewSef/AcikHesaplarNewSefim.sql"), System.Text.Encoding.UTF8);
                    }
                    else if (AppDbType == "2")
                    {
                        Query = File.ReadAllText(HostingEnvironment.MapPath("/Sql/AcikHesaplar.sql"), System.Text.Encoding.UTF8);
                    }
                    else if (AppDbType == "3")
                    {
                        //Query = File.ReadAllText(HostingEnvironment.MapPath("/Sql/SqlFaster/SubeToplamCiroFASTER.sql"), System.Text.Encoding.UTF8);
                    }
                    #endregion
                    //if (subeid.Equals(""))
                    //    Query = File.ReadAllText(HostingEnvironment.MapPath("/Sql/AcikHesaplar.sql"), System.Text.Encoding.UTF8);
                    //else
                    //    Query = File.ReadAllText(HostingEnvironment.MapPath("/Sql/AcikHesaplarDetay.sql"), System.Text.Encoding.UTF8);

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

                            if (AcikHesapDt.Rows.Count > 0)
                            {
                                if (subeid.Equals(""))
                                {
                                    AcikHesapReportSendMailViewModel items = new AcikHesapReportSendMailViewModel();
                                    items.Sube = SubeAdi;
                                    //items.SubeID = Convert.ToInt32(SubeId);
                                    //items.CustomerName = f.RTS(AcikHesapDt.Rows[0], "CustomerName");
                                    //items.AlinanTahsilat = f.RTD(AcikHesapDt.Rows[0], "AlinanTahsilat");
                                    foreach (DataRow SubeR in AcikHesapDt.Rows)
                                    {
                                        items.AcikHesabaAktarilan = f.RTD(SubeR, "Debit");
                                        items.AcikHesapTahsilat = f.RTD(SubeR, "Total");
                                        //items.Debit += f.RTD(SubeR, "Debit");
                                        //items.Ciro += f.RTD(SubeR, "Total");
                                        //items.CreditPayment = f.RTD(SubeR, "CreditPayment");
                                        //items.TicketPayment = f.RTD(SubeR, "TicketPayment");
                                        //items.Discount = f.RTD(SubeR, "Discount");
                                        //items.AlinanTahsilat = f.RTD(SubeR, "CollectedTotal");
                                        //items.KalanTahsilat = f.RTD(SubeR, "Balance");
                                    }
                                    Liste.Add(items);
                                }
                                //else
                                //{
                                //    foreach (DataRow SubeR in AcikHesapDt.Rows)
                                //    {
                                //        AcikHesapReportSendMailViewModel items = new AcikHesapReportSendMailViewModel();
                                //        items.Sube = SubeAdi;
                                //        //items.SubeID = Convert.ToInt32(SubeId);
                                //        //items.CustomerName = f.RTS(SubeR, "CustomerName");
                                //        items.Ciro = f.RTD(SubeR, "Debit");
                                //        Liste.Add(items);

                                //    }
                                //}
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

                            AcikHesapReportSendMailViewModel items = new AcikHesapReportSendMailViewModel();
                            items.Sube = SubeAdi + " (Erişim Yok) ";
                            //items.SubeID = Convert.ToInt32(SubeId);
                            ////items.CustomerName = cu;
                            ////items.Debit = SubeId;
                            //items.ErrorMessage = ErrorMessage;
                            //items.ErrorStatus = true;
                            //items.ErrorCode = "01";
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

                                    if (AcikHesapDt.Rows.Count > 0)
                                    {
                                        if (subeid.Equals(""))
                                        {
                                            AcikHesapReportSendMailViewModel items = new AcikHesapReportSendMailViewModel();
                                            items.Sube = SubeAdi;
                                            //items.SubeID = Convert.ToInt32(SubeId);
                                            //items.CustomerName = f.RTS(AcikHesapDt.Rows[0], "CustomerName");
                                            //items.AlinanTahsilat = f.RTD(AcikHesapDt.Rows[0], "AlinanTahsilat");
                                            foreach (DataRow SubeR in AcikHesapDt.Rows)
                                            {
                                                items.AcikHesabaAktarilan = f.RTD(SubeR, "Debit");
                                                items.AcikHesabaAktarilan = f.RTD(SubeR, "Total");
                                                //items.Debit += f.RTD(SubeR, "Debit");
                                                //items.Ciro += f.RTD(SubeR, "Total");
                                                //items.CreditPayment = f.RTD(SubeR, "CreditPayment");
                                                //items.TicketPayment = f.RTD(SubeR, "TicketPayment");
                                                //items.Discount = f.RTD(SubeR, "Discount");
                                                //items.AlinanTahsilat = f.RTD(SubeR, "CollectedTotal");
                                                //items.KalanTahsilat = f.RTD(SubeR, "Balance");
                                            }
                                            Liste.Add(items);
                                        }
                                        //else
                                        //{
                                        //    foreach (DataRow SubeR in AcikHesapDt.Rows)
                                        //    {
                                        //        AcikHesapReportSendMailViewModel items = new AcikHesapReportSendMailViewModel();
                                        //        items.Sube = SubeAdi;
                                        //        //items.SubeID = Convert.ToInt32(SubeId);
                                        //        //items.CustomerName = f.RTS(SubeR, "CustomerName");
                                        //        items.Ciro = f.RTD(SubeR, "Debit");
                                        //        Liste.Add(items);

                                        //    }
                                        //}
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

                                    AcikHesapReportSendMailViewModel items = new AcikHesapReportSendMailViewModel();
                                    items.Sube = SubeAdi + " (Erişim Yok) ";
                                    //items.SubeID = Convert.ToInt32(SubeId);
                                    ////items.CustomerName = cu;
                                    ////items.Debit = SubeId;
                                    //items.ErrorMessage = ErrorMessage;
                                    //items.ErrorStatus = true;
                                    //items.ErrorCode = "01";
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