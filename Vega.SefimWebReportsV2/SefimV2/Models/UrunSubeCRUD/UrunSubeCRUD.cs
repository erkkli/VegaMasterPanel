using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Web.Hosting;

namespace SefimV2.Models
{
    public class UrunSubeCRUD
    {
        public static List<UrunSube> List(DateTime Date1, DateTime Date2, string subeid, string productGroup)
        {
            List<UrunSube> Liste = new List<UrunSube>();
            ModelFunctions f = new ModelFunctions();
            DateTime startDate = DateTime.Now;
            try
            {
                f.SqlConnOpen();
                string filter = "Where Status=1";

                if (subeid != null && !subeid.Equals("0"))
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
                    string QueryTimeStart = Date1.ToString("yyyy/MM/dd HH:mm:ss");
                    string QueryTimeEnd = Date2.ToString("yyyy/MM/dd HH:mm:ss");

                    if (subeid != null && !subeid.Equals("0"))
                        Query = File.ReadAllText(HostingEnvironment.MapPath("/Sql/UrunTuruneGore.sql"), System.Text.UTF8Encoding.Default);
                    else
                        Query = File.ReadAllText(HostingEnvironment.MapPath("/Sql/UrunTuruneGoreSubeBazli.sql"), System.Text.UTF8Encoding.Default);

                    //if (!subeid.Equals("0"))
                    Query = Query.Replace("{SubeAdi}", SubeAdi);
                    Query = Query.Replace("{ProductGroup}", productGroup);
                    Query = Query.Replace("{TARIH1}", QueryTimeStart);
                    Query = Query.Replace("{TARIH2}", QueryTimeEnd);

                    try
                    {
                        string connString = "Provider=SQLOLEDB;Server=" + SubeIP + ";User Id=" + SqlName + ";Password=" + SqlPassword + ";Database=" + DBName + "";
                        f.SqlConnOpen(true, connString);
                        DataTable KasaCiroDt = f.DataTable(Query, true);
                        foreach (DataRow SubeR in KasaCiroDt.Rows)
                        {
                            UrunSube items = new UrunSube();
                            items.Sube = f.RTS(SubeR, "Sube");
                            items.SubeID = Convert.ToInt32(SubeId);
                            items.ProductGroup = f.RTS(SubeR, "ProductGroup");
                            if (!subeid.Equals("0"))
                            {
                                items.Miktar = f.RTD(SubeR, "MIKTAR");
                                items.ProductName = f.RTS(SubeR, "ProductName");
                            }
                            items.Debit = f.RTD(SubeR, "TUTAR");

                            items.GecenZaman = Convert.ToInt32(sure.TotalSeconds);
                            Liste.Add(items);
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

                        UrunSube items = new UrunSube();
                        items.Sube = SubeAdi;
                        items.SubeID = Convert.ToInt32(SubeId);
                        items.ErrorMessage = ErrorMessage;
                        items.ErrorStatus = true;
                        items.ErrorCode = "01";
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

                }
                f.SqlConnClose();
            }
            catch (DataException ex) { }

            return Liste;
        }
    }
}