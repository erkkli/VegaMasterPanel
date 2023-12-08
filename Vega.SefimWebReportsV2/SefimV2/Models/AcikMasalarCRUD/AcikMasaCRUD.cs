using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Hosting;

namespace SefimV2.Models
{
    public class AcikMasaCRUD
    {
        public static List<AcikMasa> List(DateTime Date1, DateTime Date2, string subeid)
        {
            List<AcikMasa> Liste = new List<AcikMasa>();
            ModelFunctions f = new ModelFunctions();
            DateTime startDate = DateTime.Now;
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
                    string Query = "";
                    if (subeid.Equals(""))
                        Query = File.ReadAllText(HostingEnvironment.MapPath("/Sql/AcikMasa.sql"), System.Text.UTF8Encoding.Default);
                    else
                        Query = File.ReadAllText(HostingEnvironment.MapPath("/Sql/AcikMasaUrunDetay.sql"), System.Text.UTF8Encoding.Default);

                    Query = Query.Replace("{TARIH1}", QueryTimeStart);
                    Query = Query.Replace("{TARIH2}", QueryTimeEnd);
                    if (!subeid.Equals(""))
                        Query = Query.Replace("{@SubeAdi}", SubeAdi);

                    try
                    {
                        string connString = "Provider=SQLOLEDB;Server=" + SubeIP + ";User Id=" + SqlName + ";Password=" + SqlPassword + ";Database=" + DBName + "";
                        f.SqlConnOpen(true, connString);
                        DataTable AcikHesapDt = f.DataTable(Query, true);
                        foreach (DataRow SubeR in AcikHesapDt.Rows)
                        {
                            AcikMasa items = new AcikMasa();
                            items.SubeID = Convert.ToInt32(SubeId);
                            items.Sube = SubeAdi;
                            items.TarihMin = f.RTS(SubeR, "TarihMin");
                            items.TarihMax = f.RTS(SubeR, "TarihMax");
                            items.TableNumber = f.RTS(SubeR, "TableNumber");
                            items.Debit = f.RTD(SubeR, "Debit");
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

                        AcikMasa items = new AcikMasa();
                        //items.CustomerName = cu;
                        //items.Debit = SubeId;
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