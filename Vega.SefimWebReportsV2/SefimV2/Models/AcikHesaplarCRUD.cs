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
    public class AcikHesaplarCRUD
    {
        public static List<AcikHesaplar> List(DateTime Date1, DateTime Date2, string subeid, string ID)
        {
            List<AcikHesaplar> Liste = new List<AcikHesaplar>();
            ModelFunctions ff = new ModelFunctions();
            DateTime startDate = DateTime.Now;

            #region GİRİŞ YAPAN KULLANICININ YETKİLİ OLDUĞU ŞUBELER LİSTELENİYOR
            UserViewModel model = UsersListCRUD.YetkiliSubesi(ID);
            #endregion

            try
            {
                #region GET SUBSTATION LIST               
                ff.SqlConnOpen();
                string filter = "";
                if (!subeid.Equals(""))
                    filter += " and ID=" + subeid;
                DataTable dt = ff.DataTable("select * from SubeSettings Where Status=1" + filter);
                ff.SqlConnClose();
                #endregion GET SUBSTATION LIST  

                #region PARALEL FORECH

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
                        Query = File.ReadAllText(HostingEnvironment.MapPath("/Sql/SqlNewSef/AcikHesaplarNewSefim.sql"), System.Text.UTF8Encoding.Default);
                    }
                    else if (AppDbType == "2")
                    {
                        Query = File.ReadAllText(HostingEnvironment.MapPath("/Sql/AcikHesaplar.sql"), System.Text.UTF8Encoding.Default);
                    }
                    else if (AppDbType == "3")
                    {
                        if (AppDbTypeStatus == "True")
                        {
                            #region FASTER ONLINE QUARY                           
                              Query =
                                    " declare @Sube nvarchar(100) = '{SUBE}';" +
                                    " declare @Trh1 nvarchar(20) =   '{TARIH1}'; " +
                                    " declare @Trh2 nvarchar(20) ='{TARIH2}';" +
                                    " WITH Toplamsatis AS (" +
                                    " SELECT @Sube as Sube," +
                                    " (SELECT SUBEADI FROM  " + FirmaId_SUBE + "  WHERE IND=TBLFASTERODEMELER.SUBEIND) AS Sube1," +
                                    " (SELECT KASAADI FROM " + FirmaId_KASA + " WHERE IND=TBLFASTERODEMELER.KASAIND) AS Kasa,0 AS Tahsilat, ODENEN AS Debit,0 AS zayi " +
                                    " FROM DBO.TBLFASTERODEMELER WHERE  ODEMETIPI=4 AND ISNULL(IADE,0)=0 AND ISLEMTARIHI >= @Trh1 AND ISLEMTARIHI <= @Trh2" +
                                    " UNION ALL SELECT @Sube as Sube," +
                                    " (SELECT SUBEADI FROM  " + FirmaId_SUBE + "  WHERE IND=TBLFASTERODEMELER.SUBEIND) AS Sube1," +
                                    " (SELECT KASAADI FROM " + FirmaId_KASA + " WHERE IND=TBLFASTERODEMELER.KASAIND) AS Kasa,ODENEN AS Tahsilat, 0 AS Debit,0 AS zayi " +
                                    " FROM DBO.TBLFASTERODEMELER WHERE   ISNULL(IADE,0)=0 AND ISNULL(TAHSILAT,0)=1 AND ISNULL(GIRIS,0)=1 AND ISLEMTARIHI >= @Trh1 AND ISLEMTARIHI <= @Trh2" +
                                    " UNION ALL SELECT @Sube as Sube," +
                                    " (SELECT SUBEADI FROM  " + FirmaId_SUBE + "  WHERE IND=TBLFASTERODEMELER.SUBEIND) AS Sube1," +
                                    " (SELECT KASAADI FROM " + FirmaId_KASA + " WHERE IND=TBLFASTERODEMELER.KASAIND) AS Kasa,ODENEN*-1 AS Tahsilat, 0 AS Debit,0 AS zayi  " +
                                    " FROM DBO.TBLFASTERODEMELER WHERE   ISNULL(IADE,0)=0 AND ISNULL(TAHSILAT,0)=0 AND ISNULL(GIRIS,0)=0 AND ISLEMTARIHI >= @Trh1 AND ISLEMTARIHI <= @Trh2 ) " +
                                    " SELECT Sube,Sube1,Kasa,SUM(Tahsilat) AS Total ,SUM(Debit) AS Debit FROM toplamsatis  GROUP BY Sube,Sube1,Kasa"
                                    ;
                            #endregion FASTER ONLINE QUARY
                        }
                        else
                        {
                            Query = File.ReadAllText(HostingEnvironment.MapPath("/Sql/SqlFaster/AcikHesap/AcikHesaplarFasterOFLINE.sql"), System.Text.UTF8Encoding.Default);
                        }
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
                            try
                            {
                                string connString = "Provider=SQLOLEDB;Server=" + SubeIP + ";User ID=" + SqlName + ";Password=" + SqlPassword + ";Database=" + DBName + "";

                                DataTable AcikHesapDt = new DataTable();
                                AcikHesapDt = f.GetSubeDataWithQuery((f.NewConnectionString(SubeIP, DBName, SqlName, SqlPassword)), Query.ToString());

                                if (AcikHesapDt.Rows.Count > 0)
                                {
                                    if (subeid.Equals(""))
                                    {                                                                            
                                        if (AppDbType == "3")
                                        {
                                            #region FASTER-(AppDbType = 3 faster kullanan şube)
                                            foreach (DataRow SubeR in AcikHesapDt.Rows)
                                            {
                                                AcikHesaplar items = new AcikHesaplar();
                                                items.Sube = SubeAdi;
                                                items.SubeID = Convert.ToInt32(SubeId);
                                                //items.CustomerName = AcikHesapDt.Rows[0]["CustomerName"].ToString();// f.RTS(AcikHesapDt.Rows[0], "CustomerName");
                                                //items.AlinanTahsilat = Convert.ToDecimal(AcikHesapDt.Rows[0]["AlinanTahsilat"]); //f.RTD(AcikHesapDt.Rows[0], "AlinanTahsilat");
                                                items.Debit =Convert.ToDecimal(SubeR["Debit"])  ;//f.RTD(SubeR, "Debit");
                                                items.Total =Convert.ToDecimal(SubeR["Total"])  ;//f.RTD(SubeR, "Total");                                              
                                                Liste.Add(items);
                                            }
                                            #endregion FASTER-(AppDbType = 3 faster kullanan şube)
                                        }
                                        else
                                        {
                                            #region SEFIM
                                            
                                            AcikHesaplar items = new AcikHesaplar();
                                            items.Sube = SubeAdi;
                                            items.SubeID = Convert.ToInt32(SubeId);
                                            //items.CustomerName = AcikHesapDt.Rows[0]["CustomerName"].ToString();// f.RTS(AcikHesapDt.Rows[0], "CustomerName");
                                            //items.AlinanTahsilat = Convert.ToDecimal(AcikHesapDt.Rows[0]["AlinanTahsilat"]); //f.RTD(AcikHesapDt.Rows[0], "AlinanTahsilat");
                                            foreach (DataRow SubeR in AcikHesapDt.Rows)
                                            {
                                                items.Debit = f.RTD(SubeR, "Debit");
                                                items.Total = f.RTD(SubeR, "Total");
                                                //items.CreditPayment = f.RTD(SubeR, "CreditPayment");
                                                //items.TicketPayment = f.RTD(SubeR, "TicketPayment");
                                                //items.Discount = f.RTD(SubeR, "Discount");
                                                //items.CollectedTotal = f.RTD(SubeR, "CollectedTotal");
                                                //items.Balance = f.RTD(SubeR, "Balance");
                                            }
                                            Liste.Add(items);

                                            #endregion SEFIM
                                        }
                                    }
                                    else
                                    {
                                        foreach (DataRow SubeR in AcikHesapDt.Rows)
                                        {
                                            AcikHesaplar items = new AcikHesaplar();
                                            items.Sube = SubeAdi;
                                            items.SubeID = Convert.ToInt32(SubeId);
                                            items.CustomerName = f.RTS(SubeR, "CustomerName");
                                            items.Debit = f.RTD(SubeR, "Debit");
                                            Liste.Add(items);

                                        }
                                    }
                                }
                                else
                                {
                                    AcikHesaplar items = new AcikHesaplar();
                                    items.Sube = SubeAdi + " (Data Yok) ";
                                    items.SubeID = Convert.ToInt32(SubeId);                                
                                    Liste.Add(items);
                                }
                            }
                            catch (Exception) { throw new Exception(SubeAdi); }
                        }
                        catch (System.Exception ex)
                        {
                            #region EX
                            ////log metnini oluştur
                            string ErrorMessage = "Kasa Raporu Alınamadı.";
                            //string SystemErrorMessage = ex.Message.ToString();
                            //string LogText = "";
                            //LogText += "Hata Zamanı : " + DateTime.Now.ToString() + Environment.NewLine;
                            //LogText += "Hata Mesajı : " + ErrorMessage + Environment.NewLine;
                            //LogText += "Hata Açıklaması : " + SystemErrorMessage + Environment.NewLine;
                            //LogText += "-----------------" + Environment.NewLine;
                            AcikHesaplar items = new AcikHesaplar();
                            items.Sube = ex.Message + " (Erişim Yok) ";
                            items.SubeID = Convert.ToInt32(SubeId);
                            //items.CustomerName = cu;
                            //items.Debit = SubeId;
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
                                        //f.SqlConnOpen(true, connString);
                                        //DataTable AcikHesapDt = f.DataTable(Query, true);
                                        DataTable AcikHesapDt = new DataTable();
                                        AcikHesapDt = f.GetSubeDataWithQuery((f.NewConnectionString(SubeIP, DBName, SqlName, SqlPassword)), Query.ToString());

                                        if (AcikHesapDt.Rows.Count > 0)
                                        {
                                            if (subeid.Equals(""))
                                            {
                                                if (AppDbType == "3")
                                                {
                                                    #region FASTER -(AppDbType = 3 faster kullanan şube)
                                                    foreach (DataRow SubeR in AcikHesapDt.Rows)
                                                    {
                                                        AcikHesaplar items = new AcikHesaplar();
                                                        items.Sube = SubeAdi;
                                                        items.SubeID = Convert.ToInt32(SubeId);
                                                        //items.CustomerName = AcikHesapDt.Rows[0]["CustomerName"].ToString();// f.RTS(AcikHesapDt.Rows[0], "CustomerName");
                                                        //items.AlinanTahsilat = Convert.ToDecimal(AcikHesapDt.Rows[0]["AlinanTahsilat"]); //f.RTD(AcikHesapDt.Rows[0], "AlinanTahsilat");
                                                        items.Debit = Convert.ToDecimal(SubeR["Debit"]);//f.RTD(SubeR, "Debit");
                                                        items.Total = Convert.ToDecimal(SubeR["Total"]);//f.RTD(SubeR, "Total");                                              
                                                        Liste.Add(items);
                                                    }
                                                    #endregion FASTER -(AppDbType = 3 faster kullanan şube)
                                                }
                                                else
                                                {
                                                    AcikHesaplar items = new AcikHesaplar();
                                                    items.Sube = SubeAdi;
                                                    items.SubeID = Convert.ToInt32(SubeId);
                                                    //items.CustomerName = AcikHesapDt.Rows[0]["CustomerName"].ToString();// f.RTS(AcikHesapDt.Rows[0], "CustomerName");
                                                    //items.AlinanTahsilat = Convert.ToDecimal(AcikHesapDt.Rows[0]["AlinanTahsilat"]); //f.RTD(AcikHesapDt.Rows[0], "AlinanTahsilat");
                                                    foreach (DataRow SubeR in AcikHesapDt.Rows)
                                                    {
                                                        items.Debit = f.RTD(SubeR, "Debit");
                                                        items.Total = f.RTD(SubeR, "Total");
                                                        //items.CreditPayment = f.RTD(SubeR, "CreditPayment");
                                                        //items.TicketPayment = f.RTD(SubeR, "TicketPayment");
                                                        //items.Discount = f.RTD(SubeR, "Discount");
                                                        //items.CollectedTotal = f.RTD(SubeR, "CollectedTotal");
                                                        //items.Balance = f.RTD(SubeR, "Balance");
                                                    }
                                                    Liste.Add(items);
                                                }
                                            }
                                            else
                                            {
                                                foreach (DataRow SubeR in AcikHesapDt.Rows)
                                                {
                                                    AcikHesaplar items = new AcikHesaplar();
                                                    items.Sube = SubeAdi;
                                                    items.SubeID = Convert.ToInt32(SubeId);
                                                    items.CustomerName = f.RTS(SubeR, "CustomerName");
                                                    items.Debit = f.RTD(SubeR, "Debit");
                                                    Liste.Add(items);

                                                }
                                            }
                                        }
                                        else
                                        {
                                            AcikHesaplar items = new AcikHesaplar();
                                            items.Sube = SubeAdi + " (Data Yok) ";
                                            items.SubeID = Convert.ToInt32(SubeId);
                                            Liste.Add(items);
                                        }
                                    }
                                    catch (Exception) { throw new Exception(SubeAdi); }
                                }
                                catch (System.Exception ex)
                                {
                                    #region EX
                                    ////log metnini oluştur
                                    string ErrorMessage = "Kasa Raporu Alınamadı.";
                                    //string SystemErrorMessage = ex.Message.ToString();
                                    //string LogText = "";
                                    //LogText += "Hata Zamanı : " + DateTime.Now.ToString() + Environment.NewLine;
                                    //LogText += "Hata Mesajı : " + ErrorMessage + Environment.NewLine;
                                    //LogText += "Hata Açıklaması : " + SystemErrorMessage + Environment.NewLine;
                                    //LogText += "-----------------" + Environment.NewLine;
                                    AcikHesaplar items = new AcikHesaplar();
                                    items.Sube = ex.Message + " (Erişim veya Data Yok) ";
                                    items.SubeID = Convert.ToInt32(SubeId);
                                    //items.CustomerName = cu;
                                    //items.Debit = SubeId;
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
                                #endregion
                            }
                        }
                        #endregion
                    }
                });

                #endregion
            }
            catch (DataException ex) { }

            return Liste;
        }
    }
}