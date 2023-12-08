using SefimV2.ViewModels.User;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Hosting;

namespace SefimV2.Models
{
    public class ChartsReportsSubeCiroCRUD
    {
        public static List<SubeCiro> List(DateTime Date1, DateTime Date2, string ID)
        {
            List<SubeCiro> Liste = new List<SubeCiro>();
            ModelFunctions ff = new ModelFunctions();
            DateTime startDate = DateTime.Now;

            #region GİRİŞ YAPAN KULLANICININ YETKİLİ OLDUĞU ŞUBELER LİSTELENİYOR
            UserViewModel model = UsersListCRUD.YetkiliSubesi(ID);
            #endregion
            try
            {
                #region SUBSTATION LIST                
                ff.SqlConnOpen();
                DataTable dt = ff.DataTable("SELECT * FROM SubeSettings WHERE Status=1  ");
                ff.SqlConnClose();
                #endregion SUBSTATION LIST

                #region PARALLEL FOREACH

                var dtList = dt.AsEnumerable().ToList<DataRow>();
                Parallel.ForEach(dtList, r =>
                {
                    ModelFunctions f = new ModelFunctions();

                    string SubeId = r["ID"].ToString();
                    string SubeAdi = r["SubeName"].ToString();
                    string SubeIP = r["SubeIP"].ToString();
                    string SqlName = r["SqlName"].ToString();
                    string SqlPassword = r["SqlPassword"].ToString();
                    string DBName = r["DBName"].ToString();
                    string QueryTimeStart = Date1.ToString("yyyy/MM/dd HH:mm:ss");
                    string QueryTimeEnd = Date2.ToString("yyyy/MM/dd HH:mm:ss");

                    string FirmaId_SUBE = "F0" + r["FirmaID"].ToString() + "TBLKRDSUBELER"; //Online Faster kullanan şubede Firma ve kasa ID set ediliyor.
                    string FirmaId_KASA = "F0" + r["FirmaID"].ToString() + "TBLKRDKASALAR";


                    #region  SEFİM YENI - ESKİ FASTER SQL
                    string AppDbType = f.RTS(r, "AppDbType");
                    string AppDbTypeStatus = f.RTS(r, "AppDbTypeStatus"); //AppDbTypeStatus=True ise Faster>online
                    string Query = "";
                    if (AppDbType == "1")// 1 = yeni şefim, 2 =eski Şefim, 3 = Faster
                    {
                        Query = File.ReadAllText(HostingEnvironment.MapPath("/Sql/SqlCharts/SqlChartsNew/SubeToplamCiroNewSefim.sql"), System.Text.UTF8Encoding.Default);
                    }
                    else if (AppDbType == "2")
                    {
                        Query = File.ReadAllText(HostingEnvironment.MapPath("/Sql/SqlCharts/SqlChartsOld/SubeToplamCiroOldSefim.sql"), System.Text.UTF8Encoding.Default);
                    }
                    else if (AppDbType == "3")
                    {

                        if (AppDbTypeStatus == "True")
                        {
                            #region FASTER ONLINE QUARY                              
                            Query =
                                " declare @Sube nvarchar(100) = '{SUBEADI}';" +
                                " declare @Trh1 nvarchar(20) = '{TARIH1}';" +
                                " declare @Trh2 nvarchar(20) = '{TARIH2}';" +
                                " WITH Toplamsatis AS " +
                                " ( SELECT @Sube as Sube," +
                                " (SELECT SUBEADI FROM  " + FirmaId_SUBE + " WHERE IND=TBLFASTERODEMELER.SUBEIND) AS Sube1," +
                                " (SELECT KASAADI FROM " + FirmaId_KASA + " WHERE IND=TBLFASTERODEMELER.KASAIND) AS Kasa, ODENEN AS cash, 0 AS Credit, 0 AS Ticket, 0 AS KasaToplam " +
                                " FROM DBO.TBLFASTERODEMELER WHERE ODEMETIPI=0 AND ISNULL(IADE,0)=0 AND ISLEMTARIHI >= @Trh1 AND ISLEMTARIHI <= @Trh2" +
                                " UNION ALL SELECT @Sube as Sube," +
                                " (SELECT SUBEADI FROM  " + FirmaId_SUBE + " WHERE IND=TBLFASTERODEMELER.SUBEIND) AS Sube1," +
                                " (SELECT KASAADI FROM " + FirmaId_KASA + " WHERE IND=TBLFASTERODEMELER.KASAIND) AS Kasa, 0 AS cash, ODENEN AS Credit, 0 AS Ticket, 0 AS KasaToplam  " +
                                " FROM DBO.TBLFASTERODEMELER WHERE ODEMETIPI=1 AND ISNULL(IADE,0)=0 AND ISLEMTARIHI >= @Trh1 AND ISLEMTARIHI <= @Trh2 " +
                                " UNION ALL SELECT @Sube as Sube," +
                                " (SELECT SUBEADI FROM  " + FirmaId_SUBE + " WHERE IND=TBLFASTERODEMELER.SUBEIND) AS Sube1," +
                                " (SELECT KASAADI FROM " + FirmaId_KASA + " WHERE IND=TBLFASTERODEMELER.KASAIND) AS Kasa, 0 AS cash, 0 AS Credit,ODENEN AS Ticket,0 AS KasaToplam  " +
                                " FROM DBO.TBLFASTERODEMELER WHERE ODEMETIPI=2 AND ISNULL(IADE,0)=0 AND ISLEMTARIHI >= @Trh1 AND ISLEMTARIHI <= @Trh2" +
                                " UNION ALL SELECT @Sube as Sube," +
                                " (SELECT SUBEADI FROM  " + FirmaId_SUBE + " WHERE IND=TBLFASTERKASAISLEMLERI.SUBEIND) AS Sube1," +
                                " (SELECT KASAADI FROM " + FirmaId_KASA + " WHERE IND=TBLFASTERKASAISLEMLERI.KASAIND) AS Kasa, 0 AS cash, 0 AS Credit,0 AS Ticket, SUM(GELIR-GIDER)	 AS KasaToplam   " +
                                " FROM DBO.TBLFASTERKASAISLEMLERI WHERE ISLEMTARIHI >= @Trh1 AND ISLEMTARIHI <= @Trh2 " +
                                " GROUP BY SUBEIND ,KASAIND )" +
                                " SELECT Sube,Sube1,Kasa,SUM(Cash) AS Cash ,SUM(Credit) AS Credit ,Sum(Ticket) AS Ticket ,Sum(KasaToplam) as KasaToplam,SUM(Cash+Credit+Ticket) AS ToplamCiro,0 AS Saniye,'' AS RowStyle,'' AS RowError FROM toplamsatis GROUP BY Sube,Sube1,Kasa";
                            #endregion FASTER ONLINE QUARY
                        }
                        else
                        {
                            Query = File.ReadAllText(HostingEnvironment.MapPath("/Sql/SqlFaster/KasaCiro/KasaCiroFasterOFLINE.sql"), System.Text.UTF8Encoding.Default);
                        }

                    }
                    #endregion

                    Query = Query.Replace("{SUBEADI}", SubeAdi);
                    Query = Query.Replace("{TARIH1}", QueryTimeStart);
                    Query = Query.Replace("{TARIH2}", QueryTimeEnd);

                    if (ID == "1")
                    {
                        #region SUPER ADMİN                      
                        #region GET DATA                
                        try
                        {
                            try
                            {
                                string connString = "Provider=SQLOLEDB;Server=" + SubeIP + ";User ID=" + SqlName + ";Password=" + SqlPassword + ";Database=" + DBName + "";

                                DataTable SubeCiroDt = new DataTable();
                                SubeCiroDt = f.GetSubeDataWithQuery((f.NewConnectionString(SubeIP, DBName, SqlName, SqlPassword)), Query.ToString());

                                if (SubeCiroDt.Rows.Count > 0)
                                {

                                    if (AppDbType == "3")
                                    {
                                        #region FASTER (AppDbType=3 faster kullanan şube)
                                        foreach (DataRow sube in SubeCiroDt.Rows)
                                        {
                                            SubeCiro items = new SubeCiro();
                                            items.Sube = sube["Sube"].ToString() + "-" + sube["Sube1"].ToString() + "-" + sube["Kasa"].ToString(); //KasaCiroDt.Rows[0]["Sube"].ToString();
                                            items.SubeId = SubeId;
                                            items.Cash = Convert.ToDecimal(sube["Cash"]);
                                            items.Credit = Convert.ToDecimal(sube["Credit"]);
                                            items.Ticket = Convert.ToDecimal(sube["Ticket"]);
                                            items.Ciro = Convert.ToDecimal(sube["ToplamCiro"]);
                                            Liste.Add(items);
                                        }
                                        #endregion FASTER (AppDbType=3 faster kullanan şube)
                                    }
                                    else
                                    {
                                        foreach (DataRow SubeR in SubeCiroDt.Rows)
                                        {
                                            SubeCiro items = new SubeCiro();
                                            items.Sube = f.RTS(SubeR, "Sube");
                                            items.SubeId = SubeId;
                                            items.Cash = f.RTD(SubeR, "Cash");
                                            items.Credit = f.RTD(SubeR, "Credit");
                                            items.Ticket = f.RTD(SubeR, "Ticket");
                                            //items.ikram = f.RTD(SubeR, "ikram");
                                            //items.TableNo = f.RTD(SubeR, "TableNo");
                                            //items.Discount = f.RTD(SubeR, "Discount");
                                            //items.iptal = f.RTD(SubeR, "iptal");
                                            //items.Zayi = f.RTD(SubeR, "Zayi");
                                            //items.GecenZaman = Convert.ToInt32(sure.TotalSeconds);
                                            //items.Debit = f.RTD(SubeR, "Debit");
                                            //items.OpenTable = f.RTD(SubeR, "OpenTable");
                                            items.Ciro = f.RTD(SubeR, "ToplamCiro");
                                            //items.Ciro = items.Cash + items.Credit + items.Ticket + items.Debit;
                                            Liste.Add(items);
                                        }
                                    }
                                }
                                else
                                {
                                    SubeCiro items = new SubeCiro();
                                    items.Sube = SubeAdi + " (Data Yok)";
                                    Liste.Add(items);
                                }
                            }
                            catch (Exception) { throw new Exception(SubeAdi); }
                        }
                        catch (System.Exception ex)
                        {
                            try
                            {
                                #region EX                       
                                //log metnini oluştur
                                string ErrorMessage = "Ciro Raporu Alınamadı.";
                                //string SystemErrorMessage = ex.Message.ToString();
                                //string LogText = "";
                                //LogText += "Hata Zamanı : " + DateTime.Now.ToString() + Environment.NewLine;
                                //LogText += "Hata Mesajı : " + ErrorMessage + Environment.NewLine;
                                //LogText += "Hata Açıklaması : " + SystemErrorMessage + Environment.NewLine;
                                //LogText += "-----------------" + Environment.NewLine;
                                SubeCiro items = new SubeCiro();
                                items.Sube = SubeAdi;
                                items.SubeId = SubeId;
                                items.ErrorMessage = ErrorMessage;
                                items.ErrorStatus = true;
                                items.ErrorCode = "01";
                                Liste.Add(items);
                                //string LogFolder = "/Uploads/Logs/Error";
                                //if (Directory.Exists(HostingEnvironment.MapPath(LogFolder)) == false) { Directory.CreateDirectory(HostingEnvironment.MapPath(LogFolder)); }
                                //string LogFile = "Sube-" + SubeId + ".txt";
                                //string LogFilePath = HostingEnvironment.MapPath(LogFolder + "/" + LogFile);               
                                //if (File.Exists(LogFilePath) == false)
                                //{
                                //    string FirstLine = "Created on " + DateTime.Now.ToString() + Environment.NewLine + Environment.NewLine;
                                //    File.WriteAllText(LogFilePath, FirstLine);
                                //}
                                //File.AppendAllText(LogFilePath, LogText);
                                #endregion
                            }
                            catch (Exception)
                            { }
                        }
                        #endregion GET DATA  
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
                                    try
                                    {
                                        string connString = "Provider=SQLOLEDB;Server=" + SubeIP + ";User ID=" + SqlName + ";Password=" + SqlPassword + ";Database=" + DBName + "";

                                        DataTable SubeCiroDt = new DataTable();
                                        SubeCiroDt = f.GetSubeDataWithQuery((f.NewConnectionString(SubeIP, DBName, SqlName, SqlPassword)), Query.ToString());

                                        if (SubeCiroDt.Rows.Count > 0)
                                        {

                                            if (AppDbType == "3")
                                            {
                                                #region FASTER (AppDbType=3 faster kullanan şube)
                                                foreach (DataRow sube in SubeCiroDt.Rows)
                                                {
                                                    SubeCiro items = new SubeCiro();
                                                    items.Sube = sube["Sube"].ToString() + "-" + sube["Sube1"].ToString() + "-" + sube["Kasa"].ToString(); //KasaCiroDt.Rows[0]["Sube"].ToString();
                                                    items.SubeId = SubeId;
                                                    items.Cash = Convert.ToDecimal(sube["Cash"]);
                                                    items.Credit = Convert.ToDecimal(sube["Credit"]);
                                                    items.Ticket = Convert.ToDecimal(sube["Ticket"]);
                                                    items.Ciro = Convert.ToDecimal(sube["ToplamCiro"]);
                                                    Liste.Add(items);
                                                }
                                                #endregion FASTER (AppDbType=3 faster kullanan şube)
                                            }
                                            else
                                            {
                                                foreach (DataRow SubeR in SubeCiroDt.Rows)
                                                {
                                                    SubeCiro items = new SubeCiro();
                                                    items.Sube = f.RTS(SubeR, "Sube");
                                                    items.SubeId = SubeId;
                                                    items.Cash = f.RTD(SubeR, "Cash");
                                                    items.Credit = f.RTD(SubeR, "Credit");
                                                    items.Ticket = f.RTD(SubeR, "Ticket");
                                                    //items.ikram = f.RTD(SubeR, "ikram");
                                                    //items.TableNo = f.RTD(SubeR, "TableNo");
                                                    //items.Discount = f.RTD(SubeR, "Discount");
                                                    //items.iptal = f.RTD(SubeR, "iptal");
                                                    //items.Zayi = f.RTD(SubeR, "Zayi");
                                                    //items.GecenZaman = Convert.ToInt32(sure.TotalSeconds);
                                                    //items.Debit = f.RTD(SubeR, "Debit");
                                                    //items.OpenTable = f.RTD(SubeR, "OpenTable");
                                                    items.Ciro = f.RTD(SubeR, "ToplamCiro");
                                                    //items.Ciro = items.Cash + items.Credit + items.Ticket + items.Debit;
                                                    Liste.Add(items);
                                                }
                                            }
                                        }
                                        else
                                        {
                                            SubeCiro items = new SubeCiro();
                                            items.Sube = SubeAdi + " (Data Yok)";
                                            Liste.Add(items);
                                        }
                                    }
                                    catch (Exception) { throw new Exception(SubeAdi); }
                                }
                                catch (System.Exception ex)
                                {
                                    try
                                    {
                                        #region EX                       
                                        //log metnini oluştur
                                        string ErrorMessage = "Ciro Raporu Alınamadı.";
                                        //string SystemErrorMessage = ex.Message.ToString();
                                        //string LogText = "";
                                        //LogText += "Hata Zamanı : " + DateTime.Now.ToString() + Environment.NewLine;
                                        //LogText += "Hata Mesajı : " + ErrorMessage + Environment.NewLine;
                                        //LogText += "Hata Açıklaması : " + SystemErrorMessage + Environment.NewLine;
                                        //LogText += "-----------------" + Environment.NewLine;
                                        SubeCiro items = new SubeCiro();
                                        items.Sube = SubeAdi;
                                        items.SubeId = SubeId;
                                        items.ErrorMessage = ErrorMessage;
                                        items.ErrorStatus = true;
                                        items.ErrorCode = "01";
                                        Liste.Add(items);
                                        //string LogFolder = "/Uploads/Logs/Error";
                                        //if (Directory.Exists(HostingEnvironment.MapPath(LogFolder)) == false) { Directory.CreateDirectory(HostingEnvironment.MapPath(LogFolder)); }
                                        //string LogFile = "Sube-" + SubeId + ".txt";
                                        //string LogFilePath = HostingEnvironment.MapPath(LogFolder + "/" + LogFile);               
                                        //if (File.Exists(LogFilePath) == false)
                                        //{
                                        //    string FirstLine = "Created on " + DateTime.Now.ToString() + Environment.NewLine + Environment.NewLine;
                                        //    File.WriteAllText(LogFilePath, FirstLine);
                                        //}
                                        //File.AppendAllText(LogFilePath, LogText);
                                        #endregion
                                    }
                                    catch (Exception)
                                    { }
                                }
                                #endregion GET DATA  
                            }
                        }
                        #endregion
                    }
                });

                #endregion PARALLEL FOREACH
            }
            catch (DataException ex) { }

            return Liste;
        }
    }
}