using SefimV2.ViewModels.User;
using SefimV2.ViewModels.WaiterSales;
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
    public class WaiterSalesCRUD
    {
        public static List<WaiterSalesViewModel> List(DateTime Date1, DateTime Date2, string subeid, string ID)
        {
            List<WaiterSalesViewModel> Liste = new List<WaiterSalesViewModel>();
            ModelFunctions ff = new ModelFunctions();
            DateTime startDate = DateTime.Now;
            #region GİRİŞ YAPAN KULLANICININ YETKİLİ OLDUĞU ŞUBELER LİSTELENİYOR
            UserViewModel model = UsersListCRUD.YetkiliSubesi(ID);
            #endregion

            try
            {
                #region SUBSTATION LIST               
                ff.SqlConnOpen();
                string filter = "Where Status=1";
                if (subeid != null && !subeid.Equals("0") && !subeid.Equals(""))
                    filter += " and ID=" + subeid;
                DataTable dt = ff.DataTable("select * from SubeSettings " + filter);
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
                    string FirmaId = r["FirmaID"].ToString();
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
                        Query = File.ReadAllText(HostingEnvironment.MapPath("/Sql/WaiterSales.sql"), System.Text.UTF8Encoding.Default);
                    }
                    else if (AppDbType == "2")
                    {
                        Query = File.ReadAllText(HostingEnvironment.MapPath("/Sql/WaiterSales.sql"), System.Text.UTF8Encoding.Default);
                    }
                    else if (AppDbType == "3")
                    {
                        if (AppDbTypeStatus == "True")
                        {
                            #region FASTER ONLINE QUARY
                            Query =
                                    " declare @Trh1 nvarchar(20) = '{TARIH1}';" +
                                    " declare @Trh2 nvarchar(20) = '{TARIH2}'; " +
                                    " SELECT T.SUBE1,T.KASA,T.PERSONEL,SUM(ISNULL(MIKTAR,0)) AS MIKTAR,SUM(ISNULL(TUTAR,0)) AS TUTAR " +
                                    " FROM (( SELECT ( SELECT SUBEADI FROM F0" + FirmaId + "TBLKRDSUBELER WHERE IND=FSB.SUBEIND) AS Sube1," +
                                    " ( SELECT KASAADI FROM F0 " + FirmaId + " TBLKRDKASALAR WHERE IND=FSB.KASAIND) AS Kasa,SUM(FSH.MIKTAR) AS MIKTAR,SUM(((( MIKTAR*SATISFIYATI ) * (100-ISNULL(FSH.ISK1,0))/100) * (100-ISNULL(FSH.ISK2,0))/100) * (100-ISNULL(FSB.ALTISKORAN,0))/100)  AS TUTAR, CASE WHEN  ISNULL(FSH.PERNO,0) <> 0 THEN ( SELECT AD+'-'+SOYAD" +
                                    " FROM F0" + FirmaId + "TBLPERSONEL WHERE IND=FSH.PERNO) ELSE (SELECT USERNAME FROM TBLUSERS WHERE IND=FSB.USERIND) END AS PERSONEL,STK.MALINCINSI AS ProductName  FROM TBLFASTERSATISHAREKET AS FSH " +
                                    " LEFT JOIN TBLFASTERSATISBASLIK AS FSB ON FSH.BASLIKIND=FSB.BASLIKIND AND FSH.SUBEIND=FSB.SUBEIND AND FSH.KASAIND=FSB.KASAIND " +
                                    " LEFT JOIN F0" + FirmaId + "TBLSTOKLAR AS STK ON FSH.STOKIND=STK.IND WHERE FSH.ISLEMTARIHI>=@Trh1 AND FSH.ISLEMTARIHI<=@Trh2 AND ISNULL(FSB.IADE,0)=0 " +
                                    " GROUP BY FSB.SUBEIND,FSB.KASAIND,STK.MALINCINSI,FSH.PERNO,FSB.USERIND)  )" +
                                    " AS T GROUP BYT.SUBE1,T.KASA,T.PERSONEL";
                            #endregion FASTER ONLINE QUARY
                        }
                        else
                        {
                            Query = File.ReadAllText(HostingEnvironment.MapPath("/Sql/SqlFaster/PersonelSatis/PersonelSatisFasterOFLINE.sql"), System.Text.UTF8Encoding.Default);
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
                            string connString = "Provider=SQLOLEDB;Server=" + SubeIP + ";User ID=" + SqlName + ";Password=" + SqlPassword + ";Database=" + DBName + "";

                            try
                            {
                                DataTable AcikHesapDt = new DataTable();
                                AcikHesapDt = f.GetSubeDataWithQuery((f.NewConnectionString(SubeIP, DBName, SqlName, SqlPassword)), Query.ToString());

                                if (AcikHesapDt.Rows.Count > 0)
                                {
                                    if (subeid.Equals(""))
                                    {
                                        if (AppDbType == "3")
                                        {
                                            WaiterSalesViewModel items = new WaiterSalesViewModel();
                                            items.Sube = SubeAdi; //f.RTS(SubeUrunCiroDt.Rows[0], "Sube");
                                            items.SubeID = Convert.ToInt32(SubeId);
                                            items.UserName = "Personel Satış"; //f.RTS(SubeR, "PersonelAdi");
                                            foreach (DataRow SubeR in AcikHesapDt.Rows)
                                            {
                                                items.Total += f.RTD(SubeR, "TUTAR");
                                            }
                                            Liste.Add(items);
                                        }
                                        else
                                        {
                                            WaiterSalesViewModel items = new WaiterSalesViewModel();
                                            items.Sube = SubeAdi; //f.RTS(SubeUrunCiroDt.Rows[0], "Sube");
                                            items.SubeID = Convert.ToInt32(SubeId);
                                            items.UserName = "Personel Satış"; //f.RTS(SubeR, "PersonelAdi");
                                            foreach (DataRow SubeR in AcikHesapDt.Rows)
                                            {
                                                items.Total += f.RTD(SubeR, "Total");
                                            }
                                            Liste.Add(items);
                                        }
                                    }
                                    else
                                    {
                                        if (AppDbType == "3")
                                        {
                                            foreach (DataRow SubeR in AcikHesapDt.Rows)
                                            {
                                                WaiterSalesViewModel items = new WaiterSalesViewModel();
                                                items.Sube = SubeAdi;
                                                items.SubeID = Convert.ToInt32(SubeId);
                                                items.UserName = f.RTS(SubeR, "PERSONEL");
                                                items.Total = f.RTD(SubeR, "TUTAR");
                                                Liste.Add(items);
                                            }
                                        }
                                        else
                                        {
                                            foreach (DataRow SubeR in AcikHesapDt.Rows)
                                            {
                                                WaiterSalesViewModel items = new WaiterSalesViewModel();
                                                items.Sube = SubeAdi;
                                                items.SubeID = Convert.ToInt32(SubeId);
                                                items.UserName = f.RTS(SubeR, "UserName");
                                                items.Total = f.RTD(SubeR, "Total");
                                                Liste.Add(items);
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    WaiterSalesViewModel items = new WaiterSalesViewModel();
                                    items.Sube = SubeAdi + " (Data Yok)";
                                    items.SubeID = Convert.ToInt32(SubeId);
                                    items.UserName = "*";
                                    items.Total = 0;
                                    Liste.Add(items);
                                }
                            }
                            catch (Exception) { throw new Exception(SubeAdi); }
                        }
                        catch (System.Exception ex)
                        {
                            #region EX                          
                            //log metnini oluştur
                            string ErrorMessage = "Kasa Raporu Alınamadı.";
                            //string SystemErrorMessage = ex.Message.ToString();
                            //string LogText = "";
                            //LogText += "Hata Zamanı : " + DateTime.Now.ToString() + Environment.NewLine;
                            //LogText += "Hata Mesajı : " + ErrorMessage + Environment.NewLine;
                            //LogText += "Hata Açıklaması : " + SystemErrorMessage + Environment.NewLine;
                            //LogText += "-----------------" + Environment.NewLine;
                            WaiterSalesViewModel items = new WaiterSalesViewModel();
                            //items.CustomerName = cu;
                            //items.Debit = SubeId;
                            items.Sube = ex.Message + " (Erişim Yok)";
                            items.SubeID = Convert.ToInt32(SubeId);
                            items.UserName = "*";
                            items.Total = 0;//f.RTD(SubeR, "TUTAR");
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
                            #endregion EX
                        }
                        #endregion GET DATA
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

                                    try
                                    {
                                        DataTable AcikHesapDt = new DataTable();
                                        AcikHesapDt = f.GetSubeDataWithQuery((f.NewConnectionString(SubeIP, DBName, SqlName, SqlPassword)), Query.ToString());

                                        if (AcikHesapDt.Rows.Count > 0)
                                        {
                                            if (subeid.Equals(""))
                                            {
                                                if (AppDbType == "3")
                                                {
                                                    WaiterSalesViewModel items = new WaiterSalesViewModel();
                                                    items.Sube = SubeAdi; //f.RTS(SubeUrunCiroDt.Rows[0], "Sube");
                                                    items.SubeID = Convert.ToInt32(SubeId);
                                                    items.UserName = "Personel Satış"; //f.RTS(SubeR, "PersonelAdi");
                                                    foreach (DataRow SubeR in AcikHesapDt.Rows)
                                                    {
                                                        items.Total += f.RTD(SubeR, "TUTAR");
                                                    }
                                                    Liste.Add(items);
                                                }
                                                else
                                                {
                                                    WaiterSalesViewModel items = new WaiterSalesViewModel();
                                                    items.Sube = SubeAdi; //f.RTS(SubeUrunCiroDt.Rows[0], "Sube");
                                                    items.SubeID = Convert.ToInt32(SubeId);
                                                    items.UserName = "Personel Satış"; //f.RTS(SubeR, "PersonelAdi");
                                                    foreach (DataRow SubeR in AcikHesapDt.Rows)
                                                    {
                                                        items.Total += f.RTD(SubeR, "Total");
                                                    }
                                                    Liste.Add(items);
                                                }
                                            }
                                            else
                                            {
                                                if (AppDbType == "3")
                                                {
                                                    foreach (DataRow SubeR in AcikHesapDt.Rows)
                                                    {
                                                        WaiterSalesViewModel items = new WaiterSalesViewModel();
                                                        items.Sube = SubeAdi;
                                                        items.SubeID = Convert.ToInt32(SubeId);
                                                        items.UserName = f.RTS(SubeR, "PERSONEL");
                                                        items.Total = f.RTD(SubeR, "TUTAR");
                                                        Liste.Add(items);
                                                    }
                                                }
                                                else
                                                {
                                                    foreach (DataRow SubeR in AcikHesapDt.Rows)
                                                    {
                                                        WaiterSalesViewModel items = new WaiterSalesViewModel();
                                                        items.Sube = SubeAdi;
                                                        items.SubeID = Convert.ToInt32(SubeId);
                                                        items.UserName = f.RTS(SubeR, "UserName");
                                                        items.Total = f.RTD(SubeR, "Total");
                                                        Liste.Add(items);
                                                    }
                                                }
                                            }
                                        }
                                        else
                                        {
                                            WaiterSalesViewModel items = new WaiterSalesViewModel();
                                            items.Sube = SubeAdi + " (Data Yok)";
                                            items.SubeID = Convert.ToInt32(SubeId);
                                            items.UserName = "*";
                                            items.Total = 0;
                                            Liste.Add(items);
                                        }
                                    }
                                    catch (Exception) { throw new Exception(SubeAdi); }
                                }
                                catch (System.Exception ex)
                                {
                                    #region EX                          
                                    //log metnini oluştur
                                    string ErrorMessage = "Kasa Raporu Alınamadı.";
                                    //string SystemErrorMessage = ex.Message.ToString();
                                    //string LogText = "";
                                    //LogText += "Hata Zamanı : " + DateTime.Now.ToString() + Environment.NewLine;
                                    //LogText += "Hata Mesajı : " + ErrorMessage + Environment.NewLine;
                                    //LogText += "Hata Açıklaması : " + SystemErrorMessage + Environment.NewLine;
                                    //LogText += "-----------------" + Environment.NewLine;
                                    WaiterSalesViewModel items = new WaiterSalesViewModel();
                                    //items.CustomerName = cu;
                                    //items.Debit = SubeId;
                                    items.Sube = ex.Message + " (Erişim Yok)";
                                    items.SubeID = Convert.ToInt32(SubeId);
                                    items.UserName = "*";
                                    items.Total = 0;//f.RTD(SubeR, "TUTAR");
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
                                    #endregion EX
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