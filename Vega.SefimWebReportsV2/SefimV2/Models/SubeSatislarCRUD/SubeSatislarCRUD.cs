using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Hosting;

namespace SefimV2.Models
{
    public class SubeSatislarCRUD
    {
        public static List<SubeSatislar> List(DateTime Date1, DateTime Date2)
        {
            List<SubeSatislar> Liste = new List<SubeSatislar>();
            ModelFunctions f = new ModelFunctions();

            try
            {
                f.SqlConnOpen();
                DataTable dt = f.DataTable("select * from SubeSettings Where Status=1");
                foreach (DataRow r in dt.Rows)
                {
                    string SubeId = f.RTS(r, "Id");
                    string SubeAdi = f.RTS(r, "SubeName");
                    string SubeIP = f.RTS(r, "SubeIP");
                    string SqlName = f.RTS(r, "SqlName");
                    string SqlPassword = f.RTS(r, "SqlPassword");
                    string DBName = f.RTS(r, "DBName");
                    string QueryTimeStart = Date1.ToString("yyyy/MM/dd HH:mm:ss");
                    string QueryTimeEnd = Date2.ToString("yyyy/MM/dd HH:mm:ss");
                    string Query = File.ReadAllText(HostingEnvironment.MapPath("/Sql/Masalar.sql"), System.Text.Encoding.UTF8);
                    Query = Query.Replace("{TARIH1}", QueryTimeStart);
                    Query = Query.Replace("{TARIH2}", QueryTimeEnd);

                    try
                    {
                        string connString = "Provider=SQLOLEDB;Server=" + SubeIP + ";User Id=" + SqlName + ";Password=" + SqlPassword + ";Database=" + DBName + "";
                        f.SqlConnOpen(true, connString);
                        DataTable SubeMasaDt = f.DataTable(Query, true);
                        foreach (DataRow SubeR in SubeMasaDt.Rows)
                        {
                            SubeSatislar items = new SubeSatislar();
                            items.Id = f.RTI(SubeR, "id");
                            items.SubeId = SubeId;
                            items.Sube = SubeAdi;
                            items.IslemTipi = f.RTI(SubeR, "BillType");
                            items.OdemeDurumu = f.RTI(SubeR, "BillState");
                            Liste.Add(items);

                        }
                        f.SqlConnClose(true);
                    }
                    catch (System.Exception ex)
                    {
                        //log metnini oluştur
                        string ErrorMessage = "Masa Raporu Alınamadı.";
                        string SystemErrorMessage = ex.Message.ToString();
                        string LogText = "";
                        LogText += "Hata Zamanı : " + DateTime.Now.ToString() + Environment.NewLine;
                        LogText += "Hata Mesajı : " + ErrorMessage + Environment.NewLine;
                        LogText += "Hata Açıklaması : " + SystemErrorMessage + Environment.NewLine;
                        LogText += "-----------------" + Environment.NewLine;

                        SubeSatislar items = new SubeSatislar();
                        items.Sube = SubeAdi;
                        items.SubeId = SubeId;
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