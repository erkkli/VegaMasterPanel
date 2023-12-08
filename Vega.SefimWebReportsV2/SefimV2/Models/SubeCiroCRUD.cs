using SefimV2.ViewModels.User;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Hosting;

namespace SefimV2.Models
{
    public class SubeCiroCRUD
    {
        public static List<SubeCiro> List(DateTime Date1, DateTime Date2, string ID)
        {
            List<SubeCiro> Liste = new List<SubeCiro>();
            ModelFunctions ff = new ModelFunctions();
            DateTime startDate = DateTime.Now;

            #region GİRİŞ YAPAN KULLANICININ YETKİLİ OLDUĞU ŞUBELER LİSTELENİYOR
            UserViewModel model = UsersListCRUD.YetkiliSubesi(ID);
            #endregion

            int a = 0;

            try
            {
                #region SUBSTATION LIST               
                ff.SqlConnOpen();
                DataTable dt = ff.DataTable("SELECT * FROM SubeSettings WHERE Status=1  ");
                var dtList = dt.AsEnumerable().ToList<DataRow>();
                a = dtList.Count;
                ff.SqlConnClose();
                #endregion SUBSTATION LIST
                try
                {
                    #region PARALEL FORECH

                    Parallel.ForEach(dtList, r =>
                    {
                        //Thread task = new Thread(new ThreadStart(() =>
                        //{
                        //foreach (DataRow r in dt.Rows)
                        //{
                        ModelFunctions f = new ModelFunctions();

                        string SubeId = r["ID"].ToString();
                        string SubeAdi = r["SubeName"].ToString();
                        string SubeIP = r["SubeIP"].ToString();
                        string SqlName = r["SqlName"].ToString();
                        string SqlPassword = r["SqlPassword"].ToString();
                        string DBName = r["DBName"].ToString();
                        string FirmaId_SUBE = "F0" + r["FirmaID"].ToString() + "TBLKRDSUBELER";
                        string FirmaId_KASA = "F0" + r["FirmaID"].ToString() + "TBLKRDKASALAR";
                        string QueryTimeStart = Date1.ToString("yyyy/MM/dd HH:mm:ss");
                        string QueryTimeEnd = Date2.ToString("yyyy/MM/dd HH:mm:ss");

                        #region  SEFİM YENI - ESKİ FASTER SQL
                        string AppDbType = f.RTS(r, "AppDbType");
                        string AppDbTypeStatus = f.RTS(r, "AppDbTypeStatus");
                        string Query = "";
                        if (AppDbType == "1")// 1 = yeni şefim, 2 =eski Şefim, 3 = Faster
                        {
                            Query = File.ReadAllText(HostingEnvironment.MapPath("/Sql/SqlNewSef/SubeToplamCiroNewSefim.sql"), System.Text.UTF8Encoding.Default);
                        }
                        else if (AppDbType == "2")
                        {
                            Query = File.ReadAllText(HostingEnvironment.MapPath("/Sql/SubeToplamCiro.sql"), System.Text.UTF8Encoding.Default);
                        }
                        else if (AppDbType == "3")
                        {
                            if (AppDbTypeStatus == "True")
                            {
                                #region FASTER ONLINE QUARY                                
                                Query =
                                        " declare @Sube nvarchar(100) = '{SUBEADI}';declare @Trh1 nvarchar(20) = '{TARIH1}';declare @Trh2 nvarchar(20) = '{TARIH2}';declare @SUBEADI nvarchar(20) = '{SUBEADITBL}';declare @KASAADI nvarchar(20) = '{KASAADI}';" +
                                        " WITH Toplamsatis AS ( " +
                                        " SELECT @Sube as Sube, (SELECT SUBEADI FROM   " + FirmaId_SUBE + " WHERE IND = TBLFASTERODEMELER.SUBEIND) AS Sube1,(SELECT KASAADI FROM " + FirmaId_KASA + "  WHERE IND = TBLFASTERODEMELER.KASAIND) AS Kasa,  ODENEN AS cash, 0 AS Credit, 0 AS Ticket, 0 AS Debit, 0 AS ikram, 0 AS TableNo, 0 AS Discount, 0 AS iptal,0 AS zayi  FROM DBO.TBLFASTERODEMELER WHERE ODEMETIPI = 0 AND ISNULL(IADE,0) = 0 AND ISLEMTARIHI >= @Trh1 AND ISLEMTARIHI <= @Trh2 " +
                                        " UNION ALL SELECT @Sube as Sube, (SELECT SUBEADI FROM  " + FirmaId_SUBE + " WHERE IND = TBLFASTERODEMELER.SUBEIND) AS Sube1,(SELECT KASAADI FROM " + FirmaId_KASA + "  WHERE IND = TBLFASTERODEMELER.KASAIND) AS Kasa, 0 AS cash, ODENEN AS Credit, 0 AS Ticket, 0 AS Debit, 0 AS ikram, 0 AS TableNo, 0 AS Discount, 0 AS iptal,0 AS zayi  FROM DBO.TBLFASTERODEMELER WHERE ODEMETIPI = 1 AND ISNULL(IADE,0)= 0 AND ISLEMTARIHI >= @Trh1 AND ISLEMTARIHI <= @Trh2 " +
                                        " UNION ALL SELECT @Sube as Sube, (SELECT SUBEADI FROM  " + FirmaId_SUBE + " WHERE IND = TBLFASTERODEMELER.SUBEIND) AS Sube1,(SELECT KASAADI FROM " + FirmaId_KASA + "  WHERE IND = TBLFASTERODEMELER.KASAIND) AS Kasa,  0 AS cash, 0 AS Credit, ODENEN AS Ticket, 0 AS Debit, 0 AS ikram, 0 AS TableNo, 0 AS Discount, 0 AS iptal, 0 AS zayi  FROM DBO.TBLFASTERODEMELER WHERE ODEMETIPI = 2 AND ISNULL(IADE,0)= 0 AND ISLEMTARIHI >= @Trh1 AND ISLEMTARIHI <= @Trh2 " +
                                        " UNION ALL SELECT @Sube as Sube, (SELECT SUBEADI FROM  " + FirmaId_SUBE + " WHERE SUBEIND = TBLFASTERODEMELER.SUBEIND) AS Sube1,(SELECT KASAADI FROM " + FirmaId_KASA + "  WHERE IND = TBLFASTERODEMELER.KASAIND) AS Kasa,  0 AS cash, 0 AS Credit,0 AS Ticket, 0 AS Debit, ODENEN AS ikram, 0 AS TableNo, 0 AS Discount, 0 AS iptal, 0 AS zayi  FROM DBO.TBLFASTERODEMELER WHERE ODEMETIPI = 3 AND ISNULL(IADE,0)= 0 AND ISLEMTARIHI >= @Trh1 AND ISLEMTARIHI <= @Trh2 " +
                                        " UNION ALL SELECT @Sube as Sube, (SELECT SUBEADI FROM  " + FirmaId_SUBE + " WHERE IND = TBLFASTERODEMELER.SUBEIND) AS Sube1,(SELECT KASAADI FROM " + FirmaId_KASA + "  WHERE IND = TBLFASTERODEMELER.KASAIND) AS Kasa, 0 AS cash, 0 AS Credit,0 AS Ticket, ODENEN AS Debit,0 AS ikram, 0 AS TableNo, 0 AS Discount, 0 AS iptal, 0 AS zayi  FROM DBO.TBLFASTERODEMELER WHERE ODEMETIPI = 4 AND ISNULL(IADE,0)= 0 AND ISLEMTARIHI >= @Trh1 AND ISLEMTARIHI <= @Trh2 " +
                                        " UNION ALL SELECT @Sube as Sube, (SELECT SUBEADI FROM  " + FirmaId_SUBE + " WHERE IND = TBLFASTERSATISBASLIK.SUBEIND) AS Sube1,(SELECT KASAADI FROM " + FirmaId_KASA + "  WHERE IND = TBLFASTERSATISBASLIK.KASAIND) AS Kasa,  0 AS cash, 0 AS Credit,0 AS Ticket, 0 AS Debit,0 AS ikram, 0 AS TableNo, SATIRISK+ALTISK AS Discount, 0 AS iptal,0 AS zayi  FROM DBO.TBLFASTERSATISBASLIK WHERE ISNULL(IADE, 0) = 0 AND ISLEMTARIHI >= @Trh1 AND ISLEMTARIHI <= @Trh2 " +
                                        " UNION ALL  SELECT @Sube as Sube, (SELECT SUBEADI FROM  " + FirmaId_SUBE + " WHERE IND = TBLFASTERODEMELER.SUBEIND) AS Sube1,(SELECT KASAADI FROM " + FirmaId_KASA + "  WHERE IND = TBLFASTERODEMELER.KASAIND) AS Kasa,  0 AS cash, 0 AS Credit,0 AS Ticket, 0 AS Debit,0 AS ikram, 0 AS TableNo, 0 AS Discount, ODENEN AS iptal, 0 AS zayi  FROM DBO.TBLFASTERODEMELER WHERE ISNULL(IADE, 0) = 1 AND ISLEMTARIHI >= @Trh1 AND ISLEMTARIHI <= @Trh2 " +
                                        " UNION ALL SELECT @Sube as Sube, (SELECT SUBEADI FROM  " + FirmaId_SUBE + " WHERE IND = TBLFASTERSATISBASLIK.SUBEIND) AS Sube1,   (SELECT KASAADI FROM " + FirmaId_KASA + "  WHERE IND = TBLFASTERSATISBASLIK.KASAIND) AS Kasa,  0 AS cash, 0 AS Credit,0 AS Ticket, 0 AS Debit,0 AS ikram, COUNT(*) AS TableNo, 0 AS Discount, 0 AS iptal, 0 AS zayi  FROM DBO.TBLFASTERSATISBASLIK WHERE ISNULL(IADE, 0) = 0 AND ISLEMTARIHI >= @Trh1 AND ISLEMTARIHI <= @Trh2 GROUP BY SUBEIND , KASAIND  " +
                                        " ) SELECT Sube, Sube1, Kasa , SUM(Cash) AS Cash  , SUM(Credit) AS Credit   , Sum(Ticket) AS Ticket, Sum(Debit) AS Debit, Sum(ikram) AS ikram, Sum(TableNo) AS TableNo, Sum(Discount) AS Discount, Sum(iptal) AS iptal, Sum(Zayi) AS Zayi, SUM(Cash + Credit + Ticket + Debit) AS ToplamCiro,0 AS Saniye,'' AS RowStyle,'' AS RowError FROM toplamsatis GROUP BY Sube,Sube1,Kasa                                                  ";
                                #endregion FASTER ONLINE QUARY
                            }
                            else
                            {
                                Query = File.ReadAllText(HostingEnvironment.MapPath("/Sql/SqlFaster/SubeToplamCiro/SubeToplamCiroFasterOFLINE.sql"), System.Text.UTF8Encoding.Default);
                            }
                        }
                        #endregion

                        Query = Query.Replace("{SUBEADI}", SubeAdi);
                        Query = Query.Replace("{TARIH1}", QueryTimeStart);
                        Query = Query.Replace("{TARIH2}", QueryTimeEnd);
                        Query = Query.Replace("{SUBEADITBL}", FirmaId_SUBE);//F0101TBLKRDSUBELER
                        Query = Query.Replace("{KASAADI}", FirmaId_KASA);//F0101TBLKRDKASALAR

                        if (ID == "1")
                        {
                            #region SUPER ADMİN 
                            
                            #region GET DATA                
                            try
                            {
                                string connString = "Provider=SQLOLEDB;Server=" + SubeIP + ";User ID=" + SqlName + ";Password=" + SqlPassword + ";Database=" + DBName + "";
                                DataTable SubeCiroDt = new DataTable();
                                SubeCiroDt = f.GetSubeDataWithQuery((f.NewConnectionString(SubeIP, DBName, SqlName, SqlPassword)), Query.ToString());

                                try
                                {
                                    if (SubeCiroDt.Rows.Count > 0)
                                    {
                                        if (AppDbType == "3")
                                        {
                                            #region FASTER (AppDbType=3 faster kullanan şube)       
                                            foreach (DataRow sube in SubeCiroDt.Rows)
                                            {
                                                SubeCiro items = new SubeCiro();
                                                items.Sube = sube["Sube"].ToString() + "-" + sube["Sube1"].ToString() + "-" + sube["Kasa"].ToString(); //f.RTS(SubeR, "Sube");
                                                items.SubeId = SubeId;
                                                items.Cash = Convert.ToDecimal(sube["Cash"]);//Convert.ToDecimal(SubeCiroDt.Rows[su]["Cash"]); //f.RTD(SubeR, "Cash");
                                                items.Credit = Convert.ToDecimal(sube["Credit"]);//f.RTD(SubeR, "Credit");
                                                items.Ticket = Convert.ToDecimal(sube["Ticket"]);//f.RTD(SubeR, "Ticket");
                                                items.ikram = Convert.ToDecimal(sube["ikram"]); //f.RTD(SubeR, "ikram");
                                                items.TableNo = Convert.ToDecimal(sube["TableNo"]); //f.RTD(SubeR, "TableNo");
                                                items.Discount = Convert.ToDecimal(sube["Discount"]); //f.RTD(SubeR, "Discount");
                                                items.iptal = Convert.ToDecimal(sube["iptal"]);//f.RTD(SubeR, "iptal");
                                                items.Zayi = Convert.ToDecimal(sube["Zayi"]);//f.RTD(SubeR, "Zayi");                                                                                      
                                                items.Debit = Convert.ToDecimal(sube["Debit"]); //f.RTD(SubeR, "Debit");
                                                                                                //items.OpenTable =Convert.ToDecimal(SubeCiroDt.Rows[0]["OpenTable"].ToString()); //f.RTD(SubeR, "OpenTable");                                                                                             
                                                items.Ciro = Convert.ToDecimal(sube["ToplamCiro"]);//f.RTD(SubeR, "ToplamCiro") ;//items.Cash + items.Credit + items.Ticket + items.Debit;
                                                Liste.Add(items);
                                            }
                                            #endregion FASTER (AppDbType=3 faster kullanan şube)
                                        }
                                        else
                                        {
                                            //foreach (DataRow SubeR in SubeCiroDt.Rows)
                                            //{
                                            SubeCiro items = new SubeCiro();
                                            items.Sube = SubeCiroDt.Rows[0]["Sube"].ToString(); //f.RTS(SubeR, "Sube");
                                            items.SubeId = SubeId;
                                            items.Cash = Convert.ToDecimal(SubeCiroDt.Rows[0]["Cash"]); //f.RTD(SubeR, "Cash");
                                            items.Credit = Convert.ToDecimal(SubeCiroDt.Rows[0]["Credit"]);//f.RTD(SubeR, "Credit");
                                            items.Ticket = Convert.ToDecimal(SubeCiroDt.Rows[0]["Ticket"]);//f.RTD(SubeR, "Ticket");
                                            items.ikram = Convert.ToDecimal(SubeCiroDt.Rows[0]["ikram"]); //f.RTD(SubeR, "ikram");
                                            items.TableNo = Convert.ToDecimal(SubeCiroDt.Rows[0]["TableNo"]); //f.RTD(SubeR, "TableNo");
                                            items.Discount = Convert.ToDecimal(SubeCiroDt.Rows[0]["Discount"]); //f.RTD(SubeR, "Discount");
                                            items.iptal = Convert.ToDecimal(SubeCiroDt.Rows[0]["iptal"]);//f.RTD(SubeR, "iptal");
                                            items.Zayi = Convert.ToDecimal(SubeCiroDt.Rows[0]["Zayi"]);//f.RTD(SubeR, "Zayi");                                                                                      
                                            items.Debit = Convert.ToDecimal(SubeCiroDt.Rows[0]["Debit"]); //f.RTD(SubeR, "Debit");
                                            items.OpenTable =Convert.ToDecimal(SubeCiroDt.Rows[0]["AcikMasalar"].ToString()); //f.RTD(SubeR, "OpenTable");                                                                                             
                                            items.Ciro = Convert.ToDecimal(SubeCiroDt.Rows[0]["ToplamCiro"]);//f.RTD(SubeR, "ToplamCiro") ;//items.Cash + items.Credit + items.Ticket + items.Debit;
                                            Liste.Add(items);
                                        }
                                    }
                                    else
                                    {
                                        SubeCiro items = new SubeCiro();
                                        items.Sube = SubeAdi + " (Data Yok) ";
                                        items.SubeId = SubeId;
                                        Liste.Add(items);
                                    }
                                }
                                catch (Exception) { throw new Exception(SubeAdi); }
                                //}
                                //f.SqlConnClose_1(true,connString);
                                //ExternalConnOle1.Close();
                                //con.Close();
                            }
                            catch (System.Exception ex)
                            {
                                try
                                {
                                    string ErrorMessage = "Ciro Raporu Alınamadı.";
                                    //string SystemErrorMessage = ex.Message.ToString();
                                    //string LogText = "";
                                    //LogText += "Hata Zamanı : " + DateTime.Now.ToString() + Environment.NewLine;
                                    //LogText += "Hata Mesajı : " + ErrorMessage + Environment.NewLine;
                                    //LogText += "Hata Açıklaması : " + SystemErrorMessage + Environment.NewLine;
                                    //LogText += "-----------------" + Environment.NewLine;
                                    SubeCiro items = new SubeCiro();
                                    items.Sube = SubeAdi + " (Erişim Yok)";
                                    items.SubeId = SubeId;
                                    items.ErrorMessage = ErrorMessage;
                                    items.ErrorStatus = true;
                                    items.ErrorCode = "01";
                                    Liste.Add(items);
                                }
                                catch (Exception ee) { }
                                //log metnini oluştur
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
                            }
                            #endregion

                            #endregion

                            --a;
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
                                        string connString = "Provider=SQLOLEDB;Server=" + SubeIP + ";User ID=" + SqlName + ";Password=" + SqlPassword + ";Database=" + DBName + "";
                                        DataTable SubeCiroDt = new DataTable();
                                        SubeCiroDt = f.GetSubeDataWithQuery((f.NewConnectionString(SubeIP, DBName, SqlName, SqlPassword)), Query.ToString());

                                        try
                                        {
                                            if (SubeCiroDt.Rows.Count > 0)
                                            {
                                                if (AppDbType == "3")
                                                {
                                                    #region FASTER (AppDbType=3 faster kullanan şube)       
                                                    foreach (DataRow sube in SubeCiroDt.Rows)
                                                    {
                                                        SubeCiro items = new SubeCiro();
                                                        items.Sube = sube["Sube"].ToString() + "-" + sube["Sube1"].ToString() + "-" + sube["Kasa"].ToString(); //f.RTS(SubeR, "Sube");
                                                        items.SubeId = SubeId;
                                                        items.Cash = Convert.ToDecimal(sube["Cash"]);//Convert.ToDecimal(SubeCiroDt.Rows[su]["Cash"]); //f.RTD(SubeR, "Cash");
                                                        items.Credit = Convert.ToDecimal(sube["Credit"]);//f.RTD(SubeR, "Credit");
                                                        items.Ticket = Convert.ToDecimal(sube["Ticket"]);//f.RTD(SubeR, "Ticket");
                                                        items.ikram = Convert.ToDecimal(sube["ikram"]); //f.RTD(SubeR, "ikram");
                                                        items.TableNo = Convert.ToDecimal(sube["TableNo"]); //f.RTD(SubeR, "TableNo");
                                                        items.Discount = Convert.ToDecimal(sube["Discount"]); //f.RTD(SubeR, "Discount");
                                                        items.iptal = Convert.ToDecimal(sube["iptal"]);//f.RTD(SubeR, "iptal");
                                                        items.Zayi = Convert.ToDecimal(sube["Zayi"]);//f.RTD(SubeR, "Zayi");                                                                                      
                                                        items.Debit = Convert.ToDecimal(sube["Debit"]); //f.RTD(SubeR, "Debit");
                                                                                                        //items.OpenTable =Convert.ToDecimal(SubeCiroDt.Rows[0]["OpenTable"].ToString()); //f.RTD(SubeR, "OpenTable");                                                                                             
                                                        items.Ciro = Convert.ToDecimal(sube["ToplamCiro"]);//f.RTD(SubeR, "ToplamCiro") ;//items.Cash + items.Credit + items.Ticket + items.Debit;
                                                        Liste.Add(items);
                                                    }
                                                    #endregion FASTER (AppDbType=3 faster kullanan şube)
                                                }
                                                else
                                                {
                                                    //foreach (DataRow SubeR in SubeCiroDt.Rows)
                                                    //{
                                                    SubeCiro items = new SubeCiro();
                                                    items.Sube = SubeCiroDt.Rows[0]["Sube"].ToString(); //f.RTS(SubeR, "Sube");
                                                    items.SubeId = SubeId;
                                                    items.Cash = Convert.ToDecimal(SubeCiroDt.Rows[0]["Cash"]); //f.RTD(SubeR, "Cash");
                                                    items.Credit = Convert.ToDecimal(SubeCiroDt.Rows[0]["Credit"]);//f.RTD(SubeR, "Credit");
                                                    items.Ticket = Convert.ToDecimal(SubeCiroDt.Rows[0]["Ticket"]);//f.RTD(SubeR, "Ticket");
                                                    items.ikram = Convert.ToDecimal(SubeCiroDt.Rows[0]["ikram"]); //f.RTD(SubeR, "ikram");
                                                    items.TableNo = Convert.ToDecimal(SubeCiroDt.Rows[0]["TableNo"]); //f.RTD(SubeR, "TableNo");
                                                    items.Discount = Convert.ToDecimal(SubeCiroDt.Rows[0]["Discount"]); //f.RTD(SubeR, "Discount");
                                                    items.iptal = Convert.ToDecimal(SubeCiroDt.Rows[0]["iptal"]);//f.RTD(SubeR, "iptal");
                                                    items.Zayi = Convert.ToDecimal(SubeCiroDt.Rows[0]["Zayi"]);//f.RTD(SubeR, "Zayi");                                                                                      
                                                    items.Debit = Convert.ToDecimal(SubeCiroDt.Rows[0]["Debit"]); //f.RTD(SubeR, "Debit");
                                                    items.OpenTable = Convert.ToDecimal(SubeCiroDt.Rows[0]["AcikMasalar"].ToString()); //f.RTD(SubeR, "OpenTable");                                                                                             
                                                    items.Ciro = Convert.ToDecimal(SubeCiroDt.Rows[0]["ToplamCiro"]);//f.RTD(SubeR, "ToplamCiro") ;//items.Cash + items.Credit + items.Ticket + items.Debit;
                                                    Liste.Add(items);
                                                }
                                            }
                                            else
                                            {
                                                SubeCiro items = new SubeCiro();
                                                items.Sube = SubeAdi + " (Data Yok) ";
                                                items.SubeId = SubeId;
                                                Liste.Add(items);
                                            }
                                        }
                                        catch (Exception) { throw new Exception(SubeAdi); }
                                        //}
                                        //f.SqlConnClose_1(true,connString);
                                        //ExternalConnOle1.Close();
                                        //con.Close();
                                    }
                                    catch (System.Exception ex)
                                    {
                                        try
                                        {
                                            string ErrorMessage = "Ciro Raporu Alınamadı.";
                                            //string SystemErrorMessage = ex.Message.ToString();
                                            //string LogText = "";
                                            //LogText += "Hata Zamanı : " + DateTime.Now.ToString() + Environment.NewLine;
                                            //LogText += "Hata Mesajı : " + ErrorMessage + Environment.NewLine;
                                            //LogText += "Hata Açıklaması : " + SystemErrorMessage + Environment.NewLine;
                                            //LogText += "-----------------" + Environment.NewLine;
                                            SubeCiro items = new SubeCiro();
                                            items.Sube = SubeAdi + " (Erişim Yok)";
                                            items.SubeId = SubeId;
                                            items.ErrorMessage = ErrorMessage;
                                            items.ErrorStatus = true;
                                            items.ErrorCode = "01";
                                            Liste.Add(items);
                                        }
                                        catch (Exception ee) { }
                                        //log metnini oluştur
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
                                    }
                                    #endregion
                                }
                            }
                            #endregion
                        }

                        //}));
                        //task.IsBackground = true;
                        //task.SetApartmentState(ApartmentState.STA);
                        //task.Start();
                    });
                    #endregion
                }
                catch (Exception eee) { }
            }
            catch (DataException ex)
            {
                --a;
            }
            //while (a > 0)
            //{      //}

            return Liste;
        }
    }
}