using SefimV2.Models;
using SefimV2.ViewModels.User;
using SefimV2.ViewModelSendMail.CiroReportSendMail;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Hosting;

namespace SefimV2.SendMailGetDataCRUD
{
    public class SendMailCiroCRUD
    {
        public static List<CiroReportSendMailViewModel> List(DateTime Date1, DateTime Date2, string ID)
        {
            List<CiroReportSendMailViewModel> Liste = new List<CiroReportSendMailViewModel>();
            ModelFunctions ff = new ModelFunctions();
            DateTime startDate = DateTime.Now;
            #region GİRİŞ YAPAN KULLANICININ YETKİLİ OLDUĞU ŞUBELER LİSTELENİYOR
            UserViewModel model = UsersListCRUD.YetkiliSubesi(ID);
            #endregion

            try
            {
                ff.SqlConnOpen();
                DataTable dt = ff.DataTable("select * from SubeSettings Where Status=1");
                ff.SqlConnClose();

                var dtList = dt.AsEnumerable().ToList<DataRow>();
                Parallel.ForEach(dtList, r =>
                {
                    ModelFunctions f = new ModelFunctions();
                    string SubeId = f.RTS(r, "Id");
                    string SubeAdi = f.RTS(r, "SubeName");
                    string SubeIP = f.RTS(r, "SubeIP");
                    string SqlName = f.RTS(r, "SqlName");
                    string SqlPassword = f.RTS(r, "SqlPassword");
                    string DBName = f.RTS(r, "DBName");
                    string QueryTimeStart = Date1.ToString("yyyy/MM/dd HH:mm:ss");
                    string QueryTimeEnd = Date2.ToString("yyyy/MM/dd HH:mm:ss");

                    #region  SEFİM YENI - ESKİ FASTER SQL
                    string AppDbType = f.RTS(r, "AppDbType");
                    string Query = "";
                    if (AppDbType == "1")// 1 = yeni şefim, 2 =eski Şefim, 3 = Faster
                    {
                        Query = File.ReadAllText(HostingEnvironment.MapPath("/Sql/SqlNewSef/SubeToplamCiroNewSefim.sql"), System.Text.Encoding.UTF8);
                    }
                    else if (AppDbType == "2")
                    {
                        Query = File.ReadAllText(HostingEnvironment.MapPath("/Sql/SubeToplamCiro.sql"), System.Text.Encoding.UTF8);
                    }
                    else if (AppDbType == "3")
                    {
                        Query = File.ReadAllText(HostingEnvironment.MapPath("/Sql/SqlFaster/SubeToplamCiroFASTER.sql"), System.Text.Encoding.UTF8);
                    }
                    #endregion

                    Query = Query.Replace("{SUBEADI}", SubeAdi);
                    Query = Query.Replace("{TARIH1}", QueryTimeStart);
                    Query = Query.Replace("{TARIH2}", QueryTimeEnd);


                    if (ID == "1")
                    {
                        #region  GET DATA
                        try
                        {
                            //string connString = "Provider=SQLOLEDB;Server=" + SubeIP + ";User Id=" + SqlName + ";Password=" + SqlPassword + ";Database=" + DBName + "";
                            //f.SqlConnOpen(true, connString);
                            //DataTable SubeCiroDt = f.DataTable(Query, true);
                            DataTable KasaCiroDt = new DataTable();
                            KasaCiroDt = f.GetSubeDataWithQuery((f.NewConnectionString(SubeIP, DBName, SqlName, SqlPassword)), Query.ToString());
                            if (KasaCiroDt.Rows.Count > 0)
                            {
                            //    foreach (DataRow SubeR in SubeCiroDt.Rows)
                            //{
                                CiroReportSendMailViewModel items = new CiroReportSendMailViewModel();
                                items.Sube = SubeAdi;//f.RTS(SubeR, "Sube");
                                //items.SubeId = SubeId;
                                items.Nakit = Convert.ToDecimal(KasaCiroDt.Rows[0]["Cash"]);//f.RTD(SubeR, "Cash");
                                items.Kredi = Convert.ToDecimal(KasaCiroDt.Rows[0]["Credit"]); //f.RTD(SubeR, "Credit");
                                items.YemekKarti = Convert.ToDecimal(KasaCiroDt.Rows[0]["Ticket"]); //f.RTD(SubeR, "Ticket");
                                items.ikram = Convert.ToDecimal(KasaCiroDt.Rows[0]["ikram"]);//f.RTD(SubeR, "ikram");
                                items.MasaSayisi = Convert.ToDecimal(KasaCiroDt.Rows[0]["TableNo"]); //f.RTD(SubeR, "TableNo");
                                items.İndirim = Convert.ToDecimal(KasaCiroDt.Rows[0]["Discount"]);//f.RTD(SubeR, "Discount");
                                items.iptal = Convert.ToDecimal(KasaCiroDt.Rows[0]["iptal"]);//f.RTD(SubeR, "iptal");
                                items.Zayi = Convert.ToDecimal(KasaCiroDt.Rows[0]["Zayi"]);// f.RTD(SubeR, "Zayi");
                                items.AcikHesap = Convert.ToDecimal(KasaCiroDt.Rows[0]["Debit"]);//f.RTD(SubeR, "Debit");
                                //items.AcikMasalar = Convert.ToDecimal(KasaCiroDt.Rows[0]["OpenTable"]);//f.RTD(SubeR, "OpenTable");// f.RTD(SubeR, "AcikMasalar");
                                items.Ciro = Convert.ToDecimal(KasaCiroDt.Rows[0]["ToplamCiro"]);//f.RTD(SubeR, "ToplamCiro");//items.Cash + items.Credit + items.Ticket + items.Debit;
                                Liste.Add(items);
                            }
                            //f.SqlConnClose(true);
                        }
                        catch (System.Exception ex)
                        {
                            //log metnini oluştur
                            string ErrorMessage = "Ciro Raporu Alınamadı.";
                            string SystemErrorMessage = ex.Message.ToString();
                            string LogText = "";
                            LogText += "Hata Zamanı : " + DateTime.Now.ToString() + Environment.NewLine;
                            LogText += "Hata Mesajı : " + ErrorMessage + Environment.NewLine;
                            LogText += "Hata Açıklaması : " + SystemErrorMessage + Environment.NewLine;
                            LogText += "-----------------" + Environment.NewLine;

                            CiroReportSendMailViewModel items = new CiroReportSendMailViewModel();
                            items.Sube = SubeAdi;
                            //items.SubeId = SubeId;
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
                                #region  GET DATA
                                try
                                {
                                    //string connString = "Provider=SQLOLEDB;Server=" + SubeIP + ";User Id=" + SqlName + ";Password=" + SqlPassword + ";Database=" + DBName + "";
                                    //f.SqlConnOpen(true, connString);
                                    //DataTable SubeCiroDt = f.DataTable(Query, true);
                                    DataTable KasaCiroDt = new DataTable();
                                    KasaCiroDt = f.GetSubeDataWithQuery((f.NewConnectionString(SubeIP, DBName, SqlName, SqlPassword)), Query.ToString());
                                    if (KasaCiroDt.Rows.Count > 0)
                                    {
                                        //    foreach (DataRow SubeR in SubeCiroDt.Rows)
                                        //{
                                        CiroReportSendMailViewModel items = new CiroReportSendMailViewModel();
                                        items.Sube = SubeAdi;//f.RTS(SubeR, "Sube");
                                                             //items.SubeId = SubeId;
                                        items.Nakit = Convert.ToDecimal(KasaCiroDt.Rows[0]["Cash"]);//f.RTD(SubeR, "Cash");
                                        items.Kredi = Convert.ToDecimal(KasaCiroDt.Rows[0]["Credit"]); //f.RTD(SubeR, "Credit");
                                        items.YemekKarti = Convert.ToDecimal(KasaCiroDt.Rows[0]["Ticket"]); //f.RTD(SubeR, "Ticket");
                                        items.ikram = Convert.ToDecimal(KasaCiroDt.Rows[0]["ikram"]);//f.RTD(SubeR, "ikram");
                                        items.MasaSayisi = Convert.ToDecimal(KasaCiroDt.Rows[0]["TableNo"]); //f.RTD(SubeR, "TableNo");
                                        items.İndirim = Convert.ToDecimal(KasaCiroDt.Rows[0]["Discount"]);//f.RTD(SubeR, "Discount");
                                        items.iptal = Convert.ToDecimal(KasaCiroDt.Rows[0]["iptal"]);//f.RTD(SubeR, "iptal");
                                        items.Zayi = Convert.ToDecimal(KasaCiroDt.Rows[0]["Zayi"]);// f.RTD(SubeR, "Zayi");
                                        items.AcikHesap = Convert.ToDecimal(KasaCiroDt.Rows[0]["Debit"]);//f.RTD(SubeR, "Debit");
                                                                                                         //items.AcikMasalar = Convert.ToDecimal(KasaCiroDt.Rows[0]["OpenTable"]);//f.RTD(SubeR, "OpenTable");// f.RTD(SubeR, "AcikMasalar");
                                        items.Ciro = Convert.ToDecimal(KasaCiroDt.Rows[0]["ToplamCiro"]);//f.RTD(SubeR, "ToplamCiro");//items.Cash + items.Credit + items.Ticket + items.Debit;
                                        Liste.Add(items);
                                    }
                                    //f.SqlConnClose(true);
                                }
                                catch (System.Exception ex)
                                {
                                    //log metnini oluştur
                                    string ErrorMessage = "Ciro Raporu Alınamadı.";
                                    string SystemErrorMessage = ex.Message.ToString();
                                    string LogText = "";
                                    LogText += "Hata Zamanı : " + DateTime.Now.ToString() + Environment.NewLine;
                                    LogText += "Hata Mesajı : " + ErrorMessage + Environment.NewLine;
                                    LogText += "Hata Açıklaması : " + SystemErrorMessage + Environment.NewLine;
                                    LogText += "-----------------" + Environment.NewLine;

                                    CiroReportSendMailViewModel items = new CiroReportSendMailViewModel();
                                    items.Sube = SubeAdi;
                                    //items.SubeId = SubeId;
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
                });

            }
            catch (DataException ex) { }

            return Liste;

        }


    }
}