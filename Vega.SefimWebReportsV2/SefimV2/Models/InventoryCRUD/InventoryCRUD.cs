using SefimV2.Controllers;
using SefimV2.ViewModels.Inventory;
using SefimV2.ViewModels.User;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Hosting;

namespace SefimV2.Models
{
    public class InventoryCRUD
    {
        public String SubeName;
        public String StartDate;
        public String EndDate;
        public static List<InventoryVievModel> List(DateTime Date1, DateTime Date2, string subeid, string ID)
        {
            List<InventoryVievModel> Liste = new List<InventoryVievModel>();
            ModelFunctions f = new ModelFunctions();
            DateTime startDate = DateTime.Now;
            #region GİRİŞ YAPAN KULLANICININ YETKİLİ OLDUĞU ŞUBELER LİSTELENİYOR
            UserViewModel model = UsersListCRUD.YetkiliSubesi(ID);
            string appDbTypeEnvater = "";
            #endregion

            try
            {
                f.SqlConnOpen();

                string filter = "Where Status=1";
                if (subeid != null && !subeid.Equals(""))
                    filter += " and Id=" + subeid;
                DataTable dt_ = f.DataTable("select * from SubeSettings " + filter);

                string DepoId = "";
                string SubeId = "";
                string FirmaId = "";
                int DonemId = 0;
                string SubeAdi = "";
                string SubeIP = "";
                string SqlName = "";
                string SqlPassword = "";
                string DBName = "";
                string AppDbType = "";

                if (subeid.Equals(""))
                {
                    if (ID == "1")
                    {
                        #region Şubeler Listeleniyor.                   
                        foreach (DataRow d in dt_.Rows)
                        {
                            InventoryVievModel items = new InventoryVievModel();
                            DepoId = f.RTS(d, "DepoID");
                            SubeId = f.RTS(d, "Id");
                            items.SubeID = Convert.ToInt32(SubeId);
                            items.Sube = f.RTS(d, "SubeName");
                            Liste.Add(items);
                        }
                        #endregion Şubeler Listeleniyor.
                    }
                    else
                    {
                        #region KULLANICININ ŞUBE YETKİ KONTROLU YAPILIYOR             
                        foreach (var item in model.FR_SubeListesi)
                        {

                            #region Şubeler Listeleniyor.                   
                            foreach (DataRow d in dt_.Rows)
                            {
                                InventoryVievModel items = new InventoryVievModel();
                                DepoId = f.RTS(d, "DepoID");
                                SubeId = f.RTS(d, "Id");
                                items.SubeID = Convert.ToInt32(SubeId);
                                items.Sube = f.RTS(d, "SubeName");

                                if (item.SubeID == Convert.ToInt32(SubeId))
                                {
                                    Liste.Add(items);
                                }
                            }
                            #endregion Şubeler Listeleniyor.  
                        }
                        #endregion KULLANICININ ŞUBE YETKİ KONTROLU YAPILIYOR   
                    }
                }
                else
                {
                    #region Şube ID boş değilse VegaDB quary için  Gerekli parametreleri almak için.                   
                    foreach (DataRow d in dt_.Rows)
                    {
                        DepoId = f.RTS(d, "DepoID");
                        DonemId = f.RTI(d, "DonemID");
                        FirmaId = f.RTS(d, "FirmaID");
                        SubeId = f.RTS(d, "Id");
                        SubeAdi = f.RTS(d, "SubeName");
                        SubeIP = f.RTS(d, "SubeIP");
                        SqlName = f.RTS(d, "SqlName");
                        SqlPassword = f.RTS(d, "SqlPassword");
                        DBName = f.RTS(d, "DBName");
                        AppDbType = f.RTS(d, "AppDbType");
                        appDbTypeEnvater = AppDbType;
                    }
                    #endregion Şube ID boş değilse VegaDB quary için  Gerekli parametreleri almak için.   

                    DataTable dt = f.DataTable("SELECT Id,DBName,IP,SqlName,SqlPassword  FROM  VegaDbSettings");
                    foreach (DataRow r in dt.Rows)
                    {
                        string VegaDbId = f.RTS(r, "Id");
                        string VegaDbName = f.RTS(r, "DBName");
                        string VegaDbIp = f.RTS(r, "IP");
                        string VegaDbSqlName = f.RTS(r, "SqlName");
                        string VegaDbSqlPassword = f.RTS(r, "SqlPassword");
                        string QueryTimeStart = Date1.ToString("yyyy-MM-dd HH:mm:ss");
                        string QueryTimeEnd = Date2.ToString("yyyy-MM-dd HH:mm:ss");

                        #region GetAktifDonem
                        String __ResultString = String.Empty;
                        int __Uzunluk = 0;
                        int __Sayi = DonemId;
                        while (__Sayi > 0)
                        {
                            __Uzunluk++;
                            __Sayi = __Sayi / 10;
                        }
                        if (__Uzunluk == 1)
                        {
                            __ResultString = "D000" + DonemId;
                        }
                        else if (__Uzunluk == 2)
                        {
                            __ResultString = "D00" + DonemId;
                        }
                        #endregion GetAktifDonem

                        #region  SEFİM YENI - ESKİ FASTER SQL
                        //string AppDbType = "1"; //f.RTS(r, "AppDbType");
                        string Query = "";
                        if (AppDbType == "1" || AppDbType == "2")// 1 = yeni şefim, 2 =eski Şefim, 3 = Faster
                        {
                            Query = File.ReadAllText(HostingEnvironment.MapPath("/Sql/SqlVegaDb/SatilanEnvanter.sql"), System.Text.UTF8Encoding.Default);// 
                        }
                        //else if (AppDbType == "2")
                        //{
                        //    Query = File.ReadAllText(HostingEnvironment.MapPath("/Sql/WaiterSales.sql"), System.Text.UTF8Encoding.Default);
                        //}
                        else if (AppDbType == "3")
                        {
                            //Query = File.ReadAllText(HostingEnvironment.MapPath("/Sql/SqlFaster/KasaCiroFASTER.sql"), System.Text.Encoding.UTF8);
                            Query =
                                      " DECLARE @par1 nvarchar(20) = '{TARIH1}';" +
                                      " DECLARE @par2 nvarchar(20) = '{TARIH2}';" +
                                      " SELECT " +
                                      " T.Sube1," +
                                      " T.Kasa," +
                                      " T.Id," +
                                      " SUM(T.MIKTAR) MIKTAR," +
                                      " SUM(T.TUTAR) TUTAR," +
                                      " T.MALINCINSI  as ProductName," +
                                      " T.STOKKODU" +
                                      " FROM (" +
                                      "  (SELECT" +
                                      "   (SELECT TOP 1 SUBEADI" +
                                      "      FROM TBLFASTERKASALAR" +
                                      "      WHERE SUBENO=FSB.SUBEIND) AS Sube1," +
                                      "     (SELECT KASAADI" +
                                      "      FROM TBLFASTERKASALAR" +
                                      "      WHERE KASANO=FSB.KASAIND) AS Kasa," +
                                      "     (SELECT IND" +
                                      "      FROM TBLFASTERKASALAR" +
                                      "      WHERE IND=FSB.KASAIND) AS Id," +
                                      "          SUM(FSH.MIKTAR) AS MIKTAR," +
                                      "          SUM((((MIKTAR*SATISFIYATI) * (100-ISNULL(FSH.ISK1, 0))/100) * (100-ISNULL(FSH.ISK2, 0))/100) * (100-ISNULL(FSB.ALTISKORAN, 0))/100) AS TUTAR," +
                                      "		  ISNULL(STK.MALINCINSI,'') MALINCINSI," +
                                      "		  ISNULL(STK.STOKKODU,'') STOKKODU" +
                                      "   FROM TBLFASTERSATISHAREKET AS FSH" +
                                      "   LEFT JOIN TBLFASTERSATISBASLIK AS FSB ON FSH.BASLIKIND=FSB.BASLIKIND" +
                                      "   AND FSH.SUBEIND=FSB.SUBEIND" +
                                      "   AND FSH.KASAIND=FSB.KASAIND" +
                                      "   LEFT JOIN TBLFASTERSTOKLAR AS STK ON FSH.STOKIND=STK.STOKNO" +
                                      "   WHERE FSH.ISLEMTARIHI>=@par1" +
                                      "     AND FSH.ISLEMTARIHI<=@par2" +
                                      "     AND ISNULL(FSB.IADE, 0)=0" +
                                      "   GROUP BY FSB.SUBEIND," +
                                      "            FSB.KASAIND," +
                                      "			STK.MALINCINSI," +
                                      "			STK.STOKKODU" +
                                      "			" +
                                      "	UNION ALL" +
                                      "	SELECT" +
                                      "   (SELECT TOP 1 SUBEADI" +
                                      "      FROM TBLFASTERKASALAR" +
                                      "      WHERE SUBENO=FSB.SUBEIND) AS Sube1," +
                                      "     (SELECT KASAADI" +
                                      "      FROM TBLFASTERKASALAR" +
                                      "      WHERE KASANO=FSB.KASAIND) AS Kasa," +
                                      "     (SELECT IND" +
                                      "      FROM TBLFASTERKASALAR" +
                                      "      WHERE IND=FSB.KASAIND) AS Id," +
                                      "          SUM(FSH.MIKTAR)*-1 AS MIKTAR," +
                                      "          SUM((((MIKTAR*SATISFIYATI) * (100-ISNULL(FSH.ISK1, 0))/100) * (100-ISNULL(FSH.ISK2, 0))/100) * (100-ISNULL(FSB.ALTISKORAN, 0))/100) *-1.00 AS TUTAR," +
                                      "		  ISNULL(STK.MALINCINSI,'') MALINCINSI," +
                                      "		  ISNULL(STK.STOKKODU,'') STOKKODU" +
                                      "   FROM TBLFASTERSATISHAREKET AS FSH" +
                                      "   LEFT JOIN TBLFASTERSATISBASLIK AS FSB ON FSH.BASLIKIND=FSB.BASLIKIND" +
                                      "   AND FSH.SUBEIND=FSB.SUBEIND" +
                                      "   AND FSH.KASAIND=FSB.KASAIND" +
                                      "   LEFT JOIN TBLFASTERSTOKLAR AS STK ON FSH.STOKIND=STK.STOKNO" +
                                      "   WHERE FSH.ISLEMTARIHI>=@par1" +
                                      "     AND FSH.ISLEMTARIHI<=@par2" +
                                      "     AND ISNULL(FSB.IADE, 0)=1" +
                                      "   GROUP BY FSB.SUBEIND," +
                                      "            FSB.KASAIND," +
                                      "			STK.MALINCINSI," +
                                      "			STK.STOKKODU" +
                                      "			" +
                                      "			) ) T" +
                                      "			GROUP BY " +
                                      "			T.Sube1," +
                                      " T.Kasa," +
                                      " T.Id," +
                                      " T.MALINCINSI," +
                                      " T.STOKKODU";

                            //" DECLARE @par1 nvarchar(20) = '{TARIH1}'; " +
                            //" DECLARE @par2 nvarchar(20) = '{TARIH2}'; " +
                            //" DECLARE @par3 int = " + DepoId + " ; " +
                            //" SELECT STK.MALINCINSI ProductName, " +
                            //"      SUM(ENVANTER) AS MIKTAR, " +
                            //"      SUM(ENVANTER) AS TUTAR " +
                            //" FROM F0" + FirmaId + __ResultString + "TBLDEPOENVANTER AS ENV " +
                            //" LEFT JOIN F0" + FirmaId + "TBLSTOKLAR AS STK ON STK.IND = ENV.STOKNO " +
                            //" WHERE TARIH<= @par2 " +
                            //" AND ENV.DEPO = @par3 " +
                            //" GROUP BY ENV.STOKNO, " +
                            //"      STK.MALINCINSI" +
                            //" ORDER BY ENV.STOKNO ASC ";
                        }
                        #endregion  SEFİM YENI - ESKİ FASTER SQL

                        Query = Query.Replace("{TARIH1}", QueryTimeStart);
                        Query = Query.Replace("{TARIH2}", QueryTimeEnd);
                        string connString_ = "Provider=SQLOLEDB;Server=" + SubeIP + ";User Id=" + SqlName + ";Password=" + SqlPassword + ";Database=" + DBName + "";
                        f.SqlConnOpen(true, connString_);

                        DataTable PaymentDt = f.DataTable(Query, true);

                        #region Gün içinde Satılan Envanter(Ürünler)
                        DataTable __Envanter = f.DataTable(Query, true); //GetVegaDepoEnvanter(Convert.ToInt32(DepoId), Convert.ToInt32(FirmaId), Convert.ToInt32(DonemId));
                        #endregion

                        if (PaymentDt.Rows.Count > 0)
                        {
                            string __SQLScript = "";
                            if (AppDbType == "1" || AppDbType == "2")
                            {
                                #region SQL (VEGADB ENVANTER)                   
                                __SQLScript =
                                    "SELECT EnvanterliStok.IND AS STOKIND, " +
                                    "EnvanterliStok.STOKKODU, " +
                                    "EnvanterliStok.MALINCINSI, " +
                                    "EnvanterliStok.KOD7 AS MARKA, " +
                                    "EnvanterliStok.KOD5 AS KOD5, " +
                                    "EnvanterliStok.KOD1 AS KISAACIKLAMA, " +
                                    "EnvanterliStok.GARANTI, " +
                                    "BIRIMLER.SATISFIYATI1, " +
                                    "BIRIMLER.SATISFIYATI2, " +
                                    "BIRIMLER.SATISFIYATI3, " +
                                    "BIRIMLER.PB1, " +
                                    "BIRIMLER.PB2, " +
                                    "BIRIMLER.PB3, " +
                                    "BIRIMLER.KDV, " +
                                    "BIRIMLER.BARCODE, " +
                                    "EnvanterliStok.KARTINACILMATARIHI, " +
                                    "EnvanterliStok.Envanter " +
                                    "FROM " +
                                    "(SELECT " +
                                    "F0" + FirmaId + "TBLSTOKLAR.*, MYSTOKLAR.Envanter FROM " +
                                    "(SELECT STOKLAR.IND AS STOKIND, " +
                                    "SUM(Isnull(DEPOENVANTER.ENVANTER,'0')) AS Envanter " +
                                    "FROM F0" + FirmaId + "TBLSTOKLAR AS STOKLAR " +
                                    "LEFT JOIN F0" + FirmaId + __ResultString + "TBLDEPOENVANTER AS DEPOENVANTER " +
                                    "ON STOKLAR.IND = DEPOENVANTER.STOKNO " +
                                    "WHERE STATUS=1 AND DEPOENVANTER.DEPO=" + DepoId + " group by STOKLAR.IND) " +
                                    "AS MYSTOKLAR " +
                                    "LEFT JOIN F0" + FirmaId + "TBLSTOKLAR ON F0" + FirmaId + "TBLSTOKLAR.IND = MYSTOKLAR.STOKIND) " +
                                    "AS EnvanterliStok " +
                                    "INNER JOIN F0" + FirmaId + "TBLBIRIMLEREX AS BIRIMLER " +
                                    "ON EnvanterliStok.IND = BIRIMLER.STOKNO " +
                                    "WHERE STATUS=1 AND EnvanterliStok.ANABIRIM = BIRIMLER.IND AND EnvanterliStok.KOD5='SEFIMWEBREPORT'";
                                #endregion SQL (VEGADB ENVANTER)  
                            }
                            else if (AppDbType == "3")
                            {
                                #region SQL FASTERPOS (VEGADB ENVANTER)                   
                                __SQLScript =
                                            " DECLARE @par3 int = " + DepoId + "; " +
                                            " SELECT EnvanterliStok.IND AS STOKIND," +
                                                 "  EnvanterliStok.STOKKODU, " +
                                                 "  EnvanterliStok.MALINCINSI, " +
                                                 "  EnvanterliStok.KOD7 AS MARKA, " +
                                                 "  EnvanterliStok.KOD5 AS KOD5, " +
                                                 "  EnvanterliStok.KOD1 AS KISAACIKLAMA, " +
                                                 "  EnvanterliStok.GARANTI, " +
                                                 "  BIRIMLER.SATISFIYATI1, " +
                                                 "  BIRIMLER.SATISFIYATI2, " +
                                                 "  BIRIMLER.SATISFIYATI3, " +
                                                 "  BIRIMLER.PB1, " +
                                                 "  BIRIMLER.PB2, " +
                                                 "  BIRIMLER.PB3, " +
                                                 "  BIRIMLER.KDV, " +
                                                 "  BIRIMLER.BARCODE, " +
                                                 "  EnvanterliStok.KARTINACILMATARIHI, " +
                                                 "  EnvanterliStok.Envanter " +
                                             " FROM " +
                                             " (SELECT F0" + FirmaId + "TBLSTOKLAR.*, " +
                                             " MYSTOKLAR.Envanter " +
                                             " FROM " +
                                             " (SELECT STOKLAR.IND AS STOKIND, " +
                                             "         SUM(Isnull(DEPOENVANTER.ENVANTER, '0')) AS Envanter" +
                                             " FROM F0" + FirmaId + "TBLSTOKLAR AS STOKLAR" +
                                             " LEFT JOIN F0" + FirmaId + __ResultString + "TBLDEPOENVANTER AS DEPOENVANTER ON STOKLAR.IND = DEPOENVANTER.STOKNO" +
                                             " WHERE ISNULL(STATUS, 0) = 1" +
                                             " AND DEPOENVANTER.DEPO = @par3" +
                                             " GROUP BY STOKLAR.IND) AS MYSTOKLAR" +
                                             " LEFT JOIN F0" + FirmaId + "TBLSTOKLAR ON F0" + FirmaId + "TBLSTOKLAR.IND = MYSTOKLAR.STOKIND) AS EnvanterliStok" +
                                             " INNER JOIN F0" + FirmaId + "TBLBIRIMLEREX AS BIRIMLER ON EnvanterliStok.IND = BIRIMLER.STOKNO" +
                                             " WHERE ISNULL(STATUS,0)= 1" +
                                             " AND EnvanterliStok.ANABIRIM = BIRIMLER.IND" +
                                             " AND(EnvanterliStok.KOD5 = 'SEFIMWEBREPORT' OR EnvanterliStok.KOD5 = 'MASTER')";
                                #endregion SQL FASTERPOS (VEGADB ENVANTER)  
                            }

                            if (ID == "1")
                            {
                                #region GET DATA
                                try
                                {
                                    string connString = "Provider=SQLOLEDB;Server=" + VegaDbIp + ";User Id=" + VegaDbSqlName + ";Password=" + VegaDbSqlPassword + ";Database=" + VegaDbName + "";
                                    f.SqlConnOpen(true, connString);
                                    DataTable AcikHesapDt = f.DataTable(__SQLScript, true);

                                    #region  VEGADB (Databas'den Envanteri alıyorum ve VegaDB deki envanterden gun içinde satılan envanteri çıkarıp son kalan envanteri hesaplıyor.)
                                    DataTable __ResultSubeEnvanterTable = null;
                                    //if (__VegaEnvanterTable != null && __VegaEnvanterTable.Rows.Count > 0)
                                    //{
                                    __ResultSubeEnvanterTable = GetResultRelationTable(AcikHesapDt, __Envanter, AppDbType);
                                    //}
                                    #endregion

                                    if (AcikHesapDt.Rows.Count > 0)
                                    {
                                        if (subeid.Equals(""))
                                        {
                                            InventoryVievModel items = new InventoryVievModel();
                                            foreach (DataRow SubeR in AcikHesapDt.Rows)
                                            {
                                                items.UrunAdi = f.RTS(SubeR, "MALINCINSI");
                                                items.Envanter = f.RTD(SubeR, "Envanter");
                                                items.id = f.RTI(SubeR, "STOKIND");
                                            }
                                            Liste.Add(items);
                                        }
                                        else
                                        {
                                            foreach (DataRow ss in __ResultSubeEnvanterTable.Rows)
                                            {
                                                InventoryVievModel items = new InventoryVievModel();
                                                items.UrunAdi = f.RTS(ss, "ProductName");
                                                items.Envanter = f.RTD(ss, "AKTIFENVANTER");
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
                                    string ErrorMessage = "Envanter Raporu Alınamadı.";
                                    string SystemErrorMessage = ex.Message.ToString();
                                    string LogText = "";
                                    LogText += "Hata Zamanı : " + DateTime.Now.ToString() + Environment.NewLine;
                                    LogText += "Hata Mesajı : " + ErrorMessage + Environment.NewLine;
                                    LogText += "Hata Açıklaması : " + SystemErrorMessage + Environment.NewLine;
                                    LogText += "-----------------" + Environment.NewLine;
                                    InventoryVievModel items = new InventoryVievModel();
                                    //items.CustomerName = cu;
                                    //items.Debit = SubeId;
                                    //items.Sube = SubeAdi + " (Erişim Yok)";
                                    //items.SubeID = Convert.ToInt32(SubeId);
                                    //items.UserName = "*";
                                    //items.Total = 0;//f.RTD(SubeR, "TUTAR");
                                    items.ErrorMessage = ErrorMessage;
                                    items.ErrorStatus = true;
                                    items.ErrorCode = "01";
                                    Liste.Add(items);
                                    string LogFolder = "/Uploads/Logs/Error";
                                    if (Directory.Exists(HostingEnvironment.MapPath(LogFolder)) == false) { Directory.CreateDirectory(HostingEnvironment.MapPath(LogFolder)); }
                                    string LogFile = "Sube-" + ".txt";
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
                                            string connString = "Provider=SQLOLEDB;Server=" + VegaDbIp + ";User Id=" + VegaDbSqlName + ";Password=" + VegaDbSqlPassword + ";Database=" + VegaDbName + "";
                                            f.SqlConnOpen(true, connString);
                                            DataTable AcikHesapDt = f.DataTable(__SQLScript, true);

                                            #region  VEGADB (Databas'den Envanteri alıyorum ve VegaDB deki envanterden gun içinde satılan envanteri çıkarıp son kalan envanteri hesaplıyor.)
                                            DataTable __ResultSubeEnvanterTable = null;
                                            //if (__VegaEnvanterTable != null && __VegaEnvanterTable.Rows.Count > 0)
                                            //{
                                            __ResultSubeEnvanterTable = GetResultRelationTable(AcikHesapDt, __Envanter, AppDbType);
                                            //}
                                            #endregion

                                            if (AcikHesapDt.Rows.Count > 0)
                                            {
                                                if (subeid.Equals(""))
                                                {
                                                    InventoryVievModel items = new InventoryVievModel();
                                                    foreach (DataRow SubeR in AcikHesapDt.Rows)
                                                    {
                                                        items.UrunAdi = f.RTS(SubeR, "MALINCINSI");
                                                        items.Envanter = f.RTD(SubeR, "Envanter");
                                                        items.id = f.RTI(SubeR, "STOKIND");
                                                    }
                                                    Liste.Add(items);
                                                }
                                                else
                                                {
                                                    foreach (DataRow ss in __ResultSubeEnvanterTable.Rows)
                                                    {
                                                        InventoryVievModel items = new InventoryVievModel();
                                                        items.UrunAdi = f.RTS(ss, "ProductName");
                                                        items.Envanter = f.RTD(ss, "AKTIFENVANTER");
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
                                            string ErrorMessage = "Envanter Raporu Alınamadı.";
                                            string SystemErrorMessage = ex.Message.ToString();
                                            string LogText = "";
                                            LogText += "Hata Zamanı : " + DateTime.Now.ToString() + Environment.NewLine;
                                            LogText += "Hata Mesajı : " + ErrorMessage + Environment.NewLine;
                                            LogText += "Hata Açıklaması : " + SystemErrorMessage + Environment.NewLine;
                                            LogText += "-----------------" + Environment.NewLine;
                                            InventoryVievModel items = new InventoryVievModel();
                                            //items.CustomerName = cu;
                                            //items.Debit = SubeId;
                                            //items.Sube = SubeAdi + " (Erişim Yok)";
                                            //items.SubeID = Convert.ToInt32(SubeId);
                                            //items.UserName = "*";
                                            //items.Total = 0;//f.RTD(SubeR, "TUTAR");
                                            items.ErrorMessage = ErrorMessage;
                                            items.ErrorStatus = true;
                                            items.ErrorCode = "01";
                                            Liste.Add(items);
                                            string LogFolder = "/Uploads/Logs/Error";
                                            if (Directory.Exists(HostingEnvironment.MapPath(LogFolder)) == false) { Directory.CreateDirectory(HostingEnvironment.MapPath(LogFolder)); }
                                            string LogFile = "Sube-" + ".txt";
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
                        else
                        { }
                    }
                    f.SqlConnClose();
                }
            }
            catch (DataException ex) { }

            return Liste;
        }

        public static DataTable GetResultRelationTable(DataTable _VegaEnvanterTable, DataTable _SefimSatisTable, string appDbType)
        {
            #region FASTER VE SEFIM ICIN (SEFIM=STOKKODU, FASTER=MALINCINSI)
            string malinCinsiStokKodu = "";
            if (appDbType == "3")
            {
                malinCinsiStokKodu = "MALINCINSI";
            }
            else
            {
                malinCinsiStokKodu = "MALINCINSI";
            }
            #endregion FASTER VE SEFIM ICIN (SEFIM=STOKKODU, FASTER=MALINCINSI)

            DataTable __ResultTable = new DataTable();
            try
            {
                __ResultTable.Columns.Add("ProductName", typeof(string));
                //__ResultTable.Columns.Add("SUBE", typeof(string));
                __ResultTable.Columns.Add("MIKTAR", typeof(decimal));
                __ResultTable.Columns.Add("Envanter", typeof(decimal));
                __ResultTable.Columns.Add("KOD5", typeof(string));

                if (_SefimSatisTable != null && _SefimSatisTable.Rows.Count > 0)
                {
                    var result = from dataRows1 in _VegaEnvanterTable.AsEnumerable()
                                 join dataRows2 in _SefimSatisTable.AsEnumerable()
                                 on dataRows1.Field<string>("MALINCINSI") equals dataRows2.Field<string>("ProductName") into lj
                                 from r in lj.DefaultIfEmpty()
                                 select
             __ResultTable.LoadDataRow(new object[]
              {
                dataRows1.Field<string>(malinCinsiStokKodu),
                //r == null ? "" : r.Field<string>("SUBE"),
                r == null ? 0 : r.Field<decimal>("MIKTAR"),
                dataRows1.Field<decimal>("Envanter"),
                dataRows1.Field<string>("KOD5")
                  }, true);

                    DataTable __Table = result.CopyToDataTable();
                    __Table.Columns.Add("AKTIFENVANTER");

                    if (__Table != null && __Table.Rows.Count > 0)
                    {
                        for (int i = 0; i < __Table.Rows.Count; i++)
                        {
                            int __Satislar = Convert.ToInt32(__Table.Rows[i]["MIKTAR"]);
                            int __VegaEnvanter = Convert.ToInt32(__Table.Rows[i]["Envanter"]);
                            string __VegaKod5 = Convert.ToString(__Table.Rows[i]["KOD5"]);

                            if (__VegaEnvanter >= 0)
                            {
                                int __AktifEnvanter = (__VegaEnvanter) - (__Satislar);
                                __Table.Rows[i]["AKTIFENVANTER"] = __AktifEnvanter;
                                __Table.Rows[i]["KOD5"] = __VegaKod5;
                            }
                            else if (__VegaEnvanter < 0)
                            {
                                if (__Satislar > __VegaEnvanter)
                                {
                                    //Depoya  giriş yapılmadığında _Satislar'ı - ile çarptık.
                                    int __AktifEnvan = (-__Satislar) - (-__VegaEnvanter);
                                    __Table.Rows[i]["AKTIFENVANTER"] = __AktifEnvan;
                                    __Table.Rows[i]["KOD5"] = __VegaKod5;
                                }
                            }
                        }
                    }
                    return __Table;
                }
            }
            catch (System.Exception ex)
            {
                #region EXP                             
                //log metnini oluştur
                string ErrorMessage = "Envanter Raporu Alınamadı. (GetResultRelationTable)";
                string SystemErrorMessage = ex.Message.ToString();
                string LogText = "";
                LogText += "Hata Zamanı : " + DateTime.Now.ToString() + Environment.NewLine;
                LogText += "Hata Mesajı : " + ErrorMessage + Environment.NewLine;
                LogText += "Hata Açıklaması : " + SystemErrorMessage + Environment.NewLine;
                LogText += "-----------------" + Environment.NewLine;
                string LogFolder = "/Uploads/Logs/Error";
                if (Directory.Exists(HostingEnvironment.MapPath(LogFolder)) == false) { Directory.CreateDirectory(HostingEnvironment.MapPath(LogFolder)); }
                string LogFile = "Sube-" + ".txt";
                string LogFilePath = HostingEnvironment.MapPath(LogFolder + "/" + LogFile);

                if (File.Exists(LogFilePath) == false)
                {
                    string FirstLine = "Created on " + DateTime.Now.ToString() + Environment.NewLine + Environment.NewLine;
                    File.WriteAllText(LogFilePath, FirstLine);
                }
                File.AppendAllText(LogFilePath, LogText);
                #endregion
            }
            return __ResultTable;
        }

        public String GetAktifDonem(int _DonemID)
        {
            String __ResultString = String.Empty;
            int __Uzunluk = 0;
            int __Sayi = _DonemID;
            while (__Sayi > 0)
            {
                __Uzunluk++;
                __Sayi = __Sayi / 10;
            }
            if (__Uzunluk == 1)
            {
                __ResultString = "D000" + _DonemID;
            }
            else if (__Uzunluk == 2)
            {
                __ResultString = "D00" + _DonemID;
            }
            return __ResultString;
        }
    }
}
