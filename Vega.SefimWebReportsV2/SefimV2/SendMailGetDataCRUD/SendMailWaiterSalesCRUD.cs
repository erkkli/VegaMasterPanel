using SefimV2.Models;
using SefimV2.ViewModels.User;
using SefimV2.ViewModelSendMail.WaiterSalesReportSendMail;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Hosting;

namespace SefimV2.SendMailGetDataCRUD
{
    public class SendMailWaiterSalesCRUD
    {
        public static List<WaiterSalesReportSendMailViewModel> List(DateTime Date1, DateTime Date2, string subeid, string ID)
        {
            List<WaiterSalesReportSendMailViewModel> Liste = new List<WaiterSalesReportSendMailViewModel>();
            ModelFunctions f = new ModelFunctions();
            DateTime startDate = DateTime.Now;
            #region GİRİŞ YAPAN KULLANICININ YETKİLİ OLDUĞU ŞUBELER LİSTELENİYOR
            UserViewModel model = UsersListCRUD.YetkiliSubesi(ID);
            #endregion

            try
            {
                f.SqlConnOpen();
                string filter = "Where Status=1";

                if (subeid != null && !subeid.Equals("0") && !subeid.Equals(""))
                    filter += " and Id=" + subeid;

                DataTable dt = f.DataTable("select * from SubeSettings " + filter);
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
                        Query = File.ReadAllText(HostingEnvironment.MapPath("/Sql/WaiterSales.sql"), System.Text.UTF8Encoding.Default);
                    }
                    else if (AppDbType == "2")
                    {
                        Query = File.ReadAllText(HostingEnvironment.MapPath("/Sql/WaiterSales.sql"), System.Text.UTF8Encoding.Default);
                    }
                    else if (AppDbType == "3")
                    {
                        //Query = File.ReadAllText(HostingEnvironment.MapPath("/Sql/SqlFaster/KasaCiroFASTER.sql"), System.Text.Encoding.UTF8);
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
                            if (AcikHesapDt.Rows.Count > 0)
                            {
                                if (subeid.Equals(""))
                                {
                                    WaiterSalesReportSendMailViewModel items = new WaiterSalesReportSendMailViewModel();
                                    items.Sube = SubeAdi; //f.RTS(SubeUrunCiroDt.Rows[0], "Sube");
                                    //items.SubeID = Convert.ToInt32(SubeId);
                                    items.Personel = "Personel Satış"; //f.RTS(SubeR, "PersonelAdi");
                                    foreach (DataRow SubeR in AcikHesapDt.Rows)
                                    {
                                        items.Toplam += f.RTD(SubeR, "Total");
                                    }
                                    //items.GecenZaman = Convert.ToInt32(sure.TotalSeconds);
                                    Liste.Add(items);
                                }
                                else
                                {
                                    foreach (DataRow SubeR in AcikHesapDt.Rows)
                                    {
                                        WaiterSalesReportSendMailViewModel items = new WaiterSalesReportSendMailViewModel();
                                        items.Sube = SubeAdi;
                                        //items.SubeID = Convert.ToInt32(SubeId);
                                        items.Personel = f.RTS(SubeR, "UserName");
                                        items.Toplam = f.RTD(SubeR, "Total");
                                        Liste.Add(items);
                                    }
                                }
                            }
                            f.SqlConnClose(true);
                        }
                        catch (System.Exception ex)
                        {
                            #region EXP
                            //log metnini oluştur
                            string ErrorMessage = "WaiterSalesSendMail Raporu Alınamadı.";
                            string SystemErrorMessage = ex.Message.ToString();
                            string LogText = "";
                            LogText += "Hata Zamanı : " + DateTime.Now.ToString() + Environment.NewLine;
                            LogText += "Hata Mesajı : " + ErrorMessage + Environment.NewLine;
                            LogText += "Hata Açıklaması : " + SystemErrorMessage + Environment.NewLine;
                            LogText += "-----------------" + Environment.NewLine;
                            WaiterSalesReportSendMailViewModel items = new WaiterSalesReportSendMailViewModel();
                            //items.CustomerName = cu;
                            //items.Debit = SubeId;
                            items.Sube = SubeAdi + " (Erişim Yok)";
                            //items.SubeID = Convert.ToInt32(SubeId);
                            items.Personel = "*";
                            items.Toplam = 0;//f.RTD(SubeR, "TUTAR");
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
                            #endregion
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
                                            WaiterSalesReportSendMailViewModel items = new WaiterSalesReportSendMailViewModel();
                                            items.Sube = SubeAdi; //f.RTS(SubeUrunCiroDt.Rows[0], "Sube");
                                                                  //items.SubeID = Convert.ToInt32(SubeId);
                                            items.Personel = "Personel Satış"; //f.RTS(SubeR, "PersonelAdi");
                                            foreach (DataRow SubeR in AcikHesapDt.Rows)
                                            {
                                                items.Toplam += f.RTD(SubeR, "Total");
                                            }
                                            //items.GecenZaman = Convert.ToInt32(sure.TotalSeconds);
                                            Liste.Add(items);
                                        }
                                        else
                                        {
                                            foreach (DataRow SubeR in AcikHesapDt.Rows)
                                            {
                                                WaiterSalesReportSendMailViewModel items = new WaiterSalesReportSendMailViewModel();
                                                items.Sube = SubeAdi;
                                                //items.SubeID = Convert.ToInt32(SubeId);
                                                items.Personel = f.RTS(SubeR, "UserName");
                                                items.Toplam = f.RTD(SubeR, "Total");
                                                Liste.Add(items);
                                            }
                                        }
                                    }
                                    f.SqlConnClose(true);
                                }
                                catch (System.Exception ex)
                                {
                                    #region EXP
                                    //log metnini oluştur
                                    string ErrorMessage = "WaiterSalesSendMail Raporu Alınamadı.";
                                    string SystemErrorMessage = ex.Message.ToString();
                                    string LogText = "";
                                    LogText += "Hata Zamanı : " + DateTime.Now.ToString() + Environment.NewLine;
                                    LogText += "Hata Mesajı : " + ErrorMessage + Environment.NewLine;
                                    LogText += "Hata Açıklaması : " + SystemErrorMessage + Environment.NewLine;
                                    LogText += "-----------------" + Environment.NewLine;
                                    WaiterSalesReportSendMailViewModel items = new WaiterSalesReportSendMailViewModel();
                                    //items.CustomerName = cu;
                                    //items.Debit = SubeId;
                                    items.Sube = SubeAdi + " (Erişim Yok)";
                                    //items.SubeID = Convert.ToInt32(SubeId);
                                    items.Personel = "*";
                                    items.Toplam = 0;//f.RTD(SubeR, "TUTAR");
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
                                    #endregion
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