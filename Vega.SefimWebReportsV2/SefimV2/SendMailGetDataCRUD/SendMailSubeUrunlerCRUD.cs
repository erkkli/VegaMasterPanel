using SefimV2.Models;
using SefimV2.ViewModels.User;
using SefimV2.ViewModelSendMail.SubeUrunlerReportSendMail;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Hosting;

namespace SefimV2.SendMailGetDataCRUD
{
    public class SendMailSubeUrunlerCRUD
    {

        public static List<SubeUrunlerReportSendMailViewModel> List(DateTime Date1, DateTime Date2, string subeid, string ID)
        {
            List<SubeUrunlerReportSendMailViewModel> Liste = new List<SubeUrunlerReportSendMailViewModel>();
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
                    string Query = "";
                    string SubeId = f.RTS(r, "Id");
                    string SubeAdi = f.RTS(r, "SubeName");
                    string SubeIP = f.RTS(r, "SubeIP");
                    string SqlName = f.RTS(r, "SqlName");
                    string SqlPassword = f.RTS(r, "SqlPassword");
                    string DBName = f.RTS(r, "DBName");
                    //string QueryTimeStart = Date1;
                    //string QueryTimeEnd = Date2;
                    string QueryTimeStart = Date1.ToString("yyyy-MM-dd HH:mm:ss");
                    string QueryTimeEnd = Date2.ToString("yyyy-MM-dd HH:mm:ss");

                    Query = File.ReadAllText(HostingEnvironment.MapPath("/Sql/SubeUrunSubeBazli.sql"), System.Text.Encoding.UTF8);
                    Query = Query.Replace("{SUBEADI}", SubeAdi);
                    Query = Query.Replace("{TARIH1}", QueryTimeStart);
                    Query = Query.Replace("{TARIH2}", QueryTimeEnd);

                    if (ID == "1")
                    {
                        #region GET DATA                  
                        try
                        {
                            string connString = "Provider=SQLOLEDB;Server=" + SubeIP + ";User Id=" + SqlName + ";Password=" + SqlPassword + ";Database=" + DBName + "";
                            f.SqlConnOpen(true, connString);
                            DataTable SubeUrunCiroDt = f.DataTable(Query, true);
                            if (SubeUrunCiroDt.Rows.Count > 0)
                            {
                                if (subeid.Equals(""))
                                {
                                    SubeUrunlerReportSendMailViewModel items = new SubeUrunlerReportSendMailViewModel();
                                    items.Sube = SubeAdi; //f.RTS(SubeUrunCiroDt.Rows[0], "Sube");
                                                          //items.SubeID = Convert.ToInt32(SubeId);
                                    if (subeid.Equals(""))
                                    {
                                        foreach (DataRow SubeR in SubeUrunCiroDt.Rows)
                                        {
                                            items.Miktar += Convert.ToUInt64(f.RTD(SubeR, "MIKTAR"));
                                            items.Tutar += f.RTD(SubeR, "TUTAR");
                                        }
                                    }
                                    //items.GecenZaman = Convert.ToInt32(sure.TotalSeconds);
                                    Liste.Add(items);
                                }
                                else
                                {
                                    foreach (DataRow SubeR in SubeUrunCiroDt.Rows)
                                    {
                                        SubeUrunlerReportSendMailViewModel items = new SubeUrunlerReportSendMailViewModel();
                                        items.Sube = SubeAdi; // f.RTS(SubeR, "Sube");
                                                              //items.SubeID = Convert.ToInt32(SubeId);
                                        items.Miktar = Convert.ToUInt64(f.RTD(SubeR, "MIKTAR"));
                                        items.Urun = f.RTS(SubeR, "ProductName");
                                        items.Tutar = f.RTD(SubeR, "TUTAR");
                                        //items.GecenZaman = Convert.ToInt32(sure.TotalSeconds);
                                        Liste.Add(items);
                                    }
                                }
                            }
                            f.SqlConnClose(true);
                        }
                        catch (System.Exception ex)
                        {
                            //log metnini oluştur
                            string ErrorMessage = "Şube/Ürün Raporu Alınamadı.";
                            string SystemErrorMessage = ex.Message.ToString();
                            string LogText = "";
                            LogText += "Hata Zamanı : " + DateTime.Now.ToString() + Environment.NewLine;
                            LogText += "Hata Mesajı : " + ErrorMessage + Environment.NewLine;
                            LogText += "Hata Açıklaması : " + SystemErrorMessage + Environment.NewLine;
                            LogText += "-----------------" + Environment.NewLine;

                            SubeUrunlerReportSendMailViewModel items = new SubeUrunlerReportSendMailViewModel();
                            items.Sube = SubeAdi;
                            //items.SubeID = Convert.ToInt32(SubeId);
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
                                    DataTable SubeUrunCiroDt = f.DataTable(Query, true);
                                    if (SubeUrunCiroDt.Rows.Count > 0)
                                    {
                                        if (subeid.Equals(""))
                                        {
                                            SubeUrunlerReportSendMailViewModel items = new SubeUrunlerReportSendMailViewModel();
                                            items.Sube = SubeAdi; //f.RTS(SubeUrunCiroDt.Rows[0], "Sube");
                                                                  //items.SubeID = Convert.ToInt32(SubeId);
                                            if (subeid.Equals(""))
                                            {
                                                foreach (DataRow SubeR in SubeUrunCiroDt.Rows)
                                                {
                                                    items.Miktar += Convert.ToUInt64(f.RTD(SubeR, "MIKTAR"));
                                                    items.Tutar += f.RTD(SubeR, "TUTAR");
                                                }
                                            }
                                            //items.GecenZaman = Convert.ToInt32(sure.TotalSeconds);
                                            Liste.Add(items);
                                        }
                                        else
                                        {
                                            foreach (DataRow SubeR in SubeUrunCiroDt.Rows)
                                            {
                                                SubeUrunlerReportSendMailViewModel items = new SubeUrunlerReportSendMailViewModel();
                                                items.Sube = SubeAdi; // f.RTS(SubeR, "Sube");
                                                                      //items.SubeID = Convert.ToInt32(SubeId);
                                                items.Miktar = Convert.ToUInt64(f.RTD(SubeR, "MIKTAR"));
                                                items.Urun = f.RTS(SubeR, "ProductName");
                                                items.Tutar = f.RTD(SubeR, "TUTAR");
                                                //items.GecenZaman = Convert.ToInt32(sure.TotalSeconds);
                                                Liste.Add(items);
                                            }
                                        }
                                    }
                                    f.SqlConnClose(true);
                                }
                                catch (System.Exception ex)
                                {
                                    //log metnini oluştur
                                    string ErrorMessage = "Şube/Ürün Raporu Alınamadı.";
                                    string SystemErrorMessage = ex.Message.ToString();
                                    string LogText = "";
                                    LogText += "Hata Zamanı : " + DateTime.Now.ToString() + Environment.NewLine;
                                    LogText += "Hata Mesajı : " + ErrorMessage + Environment.NewLine;
                                    LogText += "Hata Açıklaması : " + SystemErrorMessage + Environment.NewLine;
                                    LogText += "-----------------" + Environment.NewLine;

                                    SubeUrunlerReportSendMailViewModel items = new SubeUrunlerReportSendMailViewModel();
                                    items.Sube = SubeAdi;
                                    //items.SubeID = Convert.ToInt32(SubeId);
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