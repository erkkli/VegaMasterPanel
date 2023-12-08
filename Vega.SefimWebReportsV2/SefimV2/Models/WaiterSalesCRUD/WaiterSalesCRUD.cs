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
        public static List<WaiterSalesViewModel> List(DateTime Date1, DateTime Date2, string subeid, string personel, string ID)
        {
            if (personel == "alt=\"expand/collapse\"")
            {
                personel = "NULL";
            }

            List<WaiterSalesViewModel> Liste = new List<WaiterSalesViewModel>();
            ModelFunctions ff = new ModelFunctions();
            DateTime startDate = DateTime.Now;
            #region GİRİŞ YAPAN KULLANICININ YETKİLİ OLDUĞU ŞUBELER LİSTELENİYOR
            UserViewModel model = UsersListCRUD.YetkiliSubesi(ID);
            #endregion
            string subeid_ = "";
            try
            {
                #region Kırılımlarda ilk once db de tanımlı subeyı alıyoruz.                    
                if (!subeid.Equals(""))
                {
                    string[] tableNo = subeid.Split('~');
                    subeid_ = tableNo[0];
                }
                #endregion Kırılımlarda ilk once db de tanımlı subeyı alıyoruz. 

                #region SUBSTATION LIST                
                ff.SqlConnOpen();
                string filter = "Where Status=1";
                if (subeid_ != null && !subeid_.Equals("0") && !subeid_.Equals(""))
                    filter += " and Id=" + subeid_;
                DataTable dt = ff.DataTable("select * from SubeSettings " + filter);
                ff.SqlConnClose();
                #endregion SUBSTATION LIST

                #region PARALLEL FOREACH  

                var locked = new Object();
                var dtList = dt.AsEnumerable().ToList<DataRow>();
                Parallel.ForEach(dtList, r =>
                {
                    ModelFunctions f = new ModelFunctions();
                    string AppDbType = f.RTS(r, "AppDbType");
                    string AppDbTypeStatus = f.RTS(r, "AppDbTypeStatus");
                    string SubeId = r["Id"].ToString();
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
                    string urunEslestirmeVarMi = f.RTS(r, "UrunEslestirmeVarMi");
                    string vPosSubeKodu = r["VPosSubeKodu"].ToString();
                    string vPosKasaKodu = r["VPosKasaKodu"].ToString();
                    if (AppDbType == "5")
                    {
                        if (!string.IsNullOrWhiteSpace(vPosSubeKodu))
                        {
                            var kasaName = f.GetSubeDataWithQuery(f.NewConnectionString(SubeIP, DBName, SqlName, SqlPassword), "Select * from TBLSPOSSUBELER where IND='" + vPosSubeKodu + "' ");
                            vPosSubeKodu = kasaName.Rows[0]["KODU"].ToString();
                        }
                    }

                    #region  SEFİM YENI - ESKİ FASTER SQL
                  
                    string Query = string.Empty;

                    #region Faster Kasalar seçildiyse ona göre query oluşturuluyor. 
                    string FasterSubeIND = f.RTS(r, "FasterSubeID");
                    string QueryFasterSube = string.Empty;
                    if (FasterSubeIND != null)
                    {
                        QueryFasterSube = "  and  FSH.SUBEIND IN(" + FasterSubeIND + ") ";
                    }
                    #endregion Faster Kasalar seçildiyse ona göre query oluşturuluyor.


                    if (AppDbType == "1")// 1 = yeni şefim, 2 =eski Şefim, 3 = Faster
                    {
                        if (subeid != null && !subeid.Equals("0") && personel == null)// sube secili degilse ilk giris yapilan sql  
                            Query = File.ReadAllText(HostingEnvironment.MapPath("/Sql/WaiterSales.sql"), System.Text.UTF8Encoding.Default);

                        if (subeid != null && !subeid.Equals("0") && personel == "0")
                            Query = File.ReadAllText(HostingEnvironment.MapPath("/Sql/WaiterSales2.sql"), System.Text.UTF8Encoding.Default);

                        if (subeid != null && !subeid.Equals("0") && personel != null && personel != "0")
                        {
                            if (string.IsNullOrEmpty(urunEslestirmeVarMi) || urunEslestirmeVarMi == "False")
                            {
                                Query = File.ReadAllText(HostingEnvironment.MapPath("/Sql/WaiterSales3.sql"), System.Text.UTF8Encoding.Default);
                            }
                            else
                            {
                                Query = File.ReadAllText(HostingEnvironment.MapPath("/Sql/SqlNewSef/WaiterSales/WaiterSales3UrunEslestirme.sql"), System.Text.UTF8Encoding.Default);
                            }
                        }
                    }
                    else if (AppDbType == "2")
                    {
                        if (subeid != null && !subeid.Equals("0") && personel == null)// sube secili degilse ilk giris yapilan sql  
                            Query = File.ReadAllText(HostingEnvironment.MapPath("/Sql/WaiterSales.sql"), System.Text.UTF8Encoding.Default);

                        if (subeid != null && !subeid.Equals("0") && personel == "0")
                            Query = File.ReadAllText(HostingEnvironment.MapPath("/Sql/WaiterSales2.sql"), System.Text.UTF8Encoding.Default);

                        if (subeid != null && !subeid.Equals("0") && personel != null && personel != "0")
                        {
                            if (string.IsNullOrEmpty(urunEslestirmeVarMi) || urunEslestirmeVarMi == "False")
                            {
                                Query = File.ReadAllText(HostingEnvironment.MapPath("/Sql/WaiterSales3.sql"), System.Text.UTF8Encoding.Default);
                            }
                            else
                            {
                                Query = File.ReadAllText(HostingEnvironment.MapPath("/Sql/SqlNewSef/WaiterSales/WaiterSales3UrunEslestirme.sql"), System.Text.UTF8Encoding.Default);
                            }
                        }
                    }
                    else if (AppDbType == "3")
                    {
                        #region Kırılımlarda Ana Şube Altındaki IND id alıyorum                         
                        if (!subeid.Equals(""))
                        {
                            string[] tableNo = subeid.Split('~');
                            if (tableNo.Length >= 2)
                            {
                                subeid = tableNo[1];
                            }

                            #region faster için (aynı ID de subeler var bunları ilk listede tek şubenın urungruplarını alıyoruz.Sonra 2. kırılımı almak için productGrubu False yaparak içeri alıyoruz.)
                            //if (personel == null)
                            //{
                            //    personel = "False";
                            //}
                            #endregion

                        }
                        #endregion #region Kırılımlarda Ana Şube Altındaki IND id alıyorum 

                        if (AppDbTypeStatus == "True")
                        {
                            #region FASTER ONLINE QUARY

                            //Query =
                            //            "  declare @par1 nvarchar(20) = '{TARIH1}' ;" +
                            //            "  declare @par2 nvarchar(20) = '{TARIH2}' ;" +
                            //            "  (SELECT " +
                            //            "  (SELECT SUBEADI FROM F0" + FirmaId + "TBLKRDSUBELER WHERE IND = FSB.SUBEIND) AS Sube1," +
                            //            "  (SELECT KASAADI FROM F0" + FirmaId + "TBLKRDKASALAR WHERE IND = FSB.KASAIND) AS Kasa," +
                            //            "  SUM(FSH.MIKTAR) AS MIKTAR," +
                            //            "  SUM((((MIKTAR * SATISFIYATI) * (100 - ISNULL(FSH.ISK1, 0)) / 100) * (100 - ISNULL(FSH.ISK2, 0)) / 100) * (100 - ISNULL(FSB.ALTISKORAN, 0)) / 100)  AS TUTAR, " +
                            //            "  ISNULL(FSH.PERKODU, 'YOK') PERSONEL " +
                            //            "  FROM TBLFASTERSATISHAREKET AS FSH " +
                            //            "  LEFT JOIN TBLFASTERSATISBASLIK AS FSB ON FSH.BASLIKIND = FSB.BASLIKIND AND FSH.SUBEIND = FSB.SUBEIND AND FSH.KASAIND = FSB.KASAIND " +
                            //            " LEFT JOIN F0" + FirmaId + "TBLSTOKLAR AS STK ON FSH.STOKIND = STK.IND " +
                            //            " WHERE FSH.ISLEMTARIHI >= @par1 AND FSH.ISLEMTARIHI <= @par2 AND ISNULL(FSB.IADE,0) = 0 " +
                            //           //WHERE FSH.ISLEMTARIHI >= @par1 AND FSH.ISLEMTARIHI <= @par2 AND ISNULL(FSB.IADE,0) = 0
                            //           " GROUP BY FSB.SUBEIND,FSB.KASAIND,FSH.PERKODU) ";
                            if (subeid != null && !subeid.Equals("0") && personel == null)// sube secili degilse ilk giris yapilan sql  
                                Query =
                                     "DECLARE @par1 nvarchar(20) = '{TARIH1}' ;" +
                                     "DECLARE @par2 nvarchar(20) = '{TARIH2}' ;" +
                                     "DECLARE @Sube nvarchar(100) = '{SUBE}';" +
                                    " SELECT " +
                                    " T.Sube1," +
                                    " T.Kasa," +
                                    " T.Id," +
                                    " SUM(T.MIKTAR) MIKTAR," +
                                    " SUM(T.TUTAR) TUTAR," +
                                    " T.ProductName," +
                                    " T.PERKODU" +
                                    " FROM (" +
                                    "  (SELECT" +
                                    "      (SELECT SUBEADI" +
                                    "      FROM  " + FirmaId_SUBE + " " +
                                    "      WHERE IND=FSB.SUBEIND) AS Sube1," +
                                    "     (SELECT KASAADI" +
                                    "      FROM " + FirmaId_KASA + " " +
                                    "      WHERE IND=FSB.KASAIND) AS Kasa," +
                                    "     (SELECT IND" +
                                    "      FROM " + FirmaId_KASA + " " +
                                    "      WHERE IND=FSB.KASAIND) AS Id," +
                                    "          SUM(FSH.MIKTAR) AS MIKTAR," +
                                    "          SUM((((MIKTAR*SATISFIYATI) * (100-ISNULL(FSH.ISK1, 0))/100) * (100-ISNULL(FSH.ISK2, 0))/100) * (100-ISNULL(FSB.ALTISKORAN, 0))/100) AS TUTAR," +
                                    "          STK.MALINCINSI AS ProductName," +
                                    "		  FSH.PERKODU" +
                                    "   FROM TBLFASTERSATISHAREKET AS FSH" +
                                    "   LEFT JOIN TBLFASTERSATISBASLIK AS FSB ON FSH.BASLIKIND=FSB.BASLIKIND and  FSH.BASBARCODE=FSB.BARCODE" +
                                    "   AND FSH.SUBEIND=FSB.SUBEIND" +
                                    "   AND FSH.KASAIND=FSB.KASAIND" +
                                    "   LEFT JOIN TBLFASTERSTOKLAR AS STK ON FSH.STOKIND=STK.STOKNO" +
                                    "   WHERE FSH.ISLEMTARIHI>=@par1" +
                                    "     AND FSH.ISLEMTARIHI<=@par2" +
                                    "     AND ISNULL(FSB.IADE, 0)=0" +
                                    " " + QueryFasterSube +
                                    "   GROUP BY FSB.SUBEIND," +
                                    "            FSB.KASAIND," +
                                    "            STK.MALINCINSI," +
                                    "			FSH.PERKODU" +
                                    "			" +
                                    "			UNION ALL SELECT" +
                                    " (SELECT SUBEADI" +
                                    "      FROM " + FirmaId_KASA + " " +
                                    "      WHERE IND=FSB.SUBEIND) AS Sube1," +
                                    "     (SELECT KASAADI" +
                                    "      FROM " + FirmaId_KASA + " " +
                                    "      WHERE IND=FSB.KASAIND) AS Kasa," +
                                    "     (SELECT IND" +
                                    "      FROM " + FirmaId_KASA + " " +
                                    "      WHERE IND=FSB.KASAIND) AS Id," +
                                    "          SUM(FSH.MIKTAR)*-1.00 AS MIKTAR," +
                                    "          SUM((((MIKTAR*SATISFIYATI) * (100-ISNULL(FSH.ISK1, 0))/100) * (100-ISNULL(FSH.ISK2, 0))/100) * (100-ISNULL(FSB.ALTISKORAN, 0))/100) *-1.00 AS TUTAR," +
                                    "          STK.MALINCINSI AS ProductName," +
                                    "		  FSH.PERKODU" +
                                    "   FROM TBLFASTERSATISHAREKET AS FSH" +
                                    "   LEFT JOIN TBLFASTERSATISBASLIK AS FSB ON FSH.BASLIKIND=FSB.BASLIKIND and  FSH.BASBARCODE=FSB.BARCODE" +
                                    "   AND FSH.SUBEIND=FSB.SUBEIND" +
                                    "   AND FSH.KASAIND=FSB.KASAIND" +
                                    "   LEFT JOIN TBLFASTERSTOKLAR AS STK ON FSH.STOKIND=STK.STOKNO" +
                                    "   WHERE FSH.ISLEMTARIHI>=@par1" +
                                    "     AND FSH.ISLEMTARIHI<=@par2" +
                                    "     AND ISNULL(FSB.IADE, 0)=1" +
                                    " " + QueryFasterSube +
                                    "   GROUP BY FSB.SUBEIND," +
                                    "            FSB.KASAIND," +
                                    "            STK.MALINCINSI," +
                                    "			FSH.PERKODU" +
                                    "			" +
                                    "			) ) T" +
                                    "			GROUP BY " +
                                    "			T.Sube1," +
                                    " T.Kasa," +
                                    " T.Id," +
                                    " T.ProductName," +
                                    " T.PERKODU";

                            if (subeid != null && !subeid.Equals("0") && personel == "0")
                                ////Query =
                                ////"DECLARE @par1 nvarchar(20) = '{TARIH1}' ;" +
                                ////"DECLARE @par2 nvarchar(20) = '{TARIH2}' ;" +
                                ////"DECLARE @Sube nvarchar(100) = '{SUBE}';" +
                                ////" SELECT T.Sube1," +
                                ////" COUNT(ISLEMSAYISI) ISLEMSAYISI," +
                                //////"       T.Kasa," +
                                //////"       T.Id," +
                                ////"       SUM(T.MIKTAR) MIKTAR," +
                                ////"       SUM(T.TUTAR) TUTAR," +
                                ////"  " +
                                ////"       T.PERKODU" +
                                ////" FROM (" +
                                ////"        (SELECT" +
                                ////"           (SELECT SUBEADI" +
                                ////"            FROM  " + FirmaId_SUBE + " " +
                                ////"            WHERE IND=FSB.SUBEIND) AS Sube1," +
                                //////"           (SELECT KASAADI" +
                                //////"            FROM " + FirmaId_KASA + " " +
                                //////"            WHERE IND=FSB.KASAIND) AS Kasa," +
                                //////"           (SELECT IND" +
                                //////"            FROM " + FirmaId_KASA + " " +
                                //////"            WHERE IND=FSB.KASAIND) AS Id," +
                                ////"                FSB.IND  ISLEMSAYISI," +
                                ////"                SUM(FSH.MIKTAR) AS MIKTAR," +
                                ////"                SUM((((MIKTAR*SATISFIYATI) * (100-ISNULL(FSH.ISK1, 0))/100) * (100-ISNULL(FSH.ISK2, 0))/100) * (100-ISNULL(FSB.ALTISKORAN, 0))/100) AS TUTAR," +
                                ////"                STK.MALINCINSI AS ProductName," +
                                ////"                FSH.PERKODU" +
                                ////"         FROM TBLFASTERSATISHAREKET AS FSH" +
                                ////"         LEFT JOIN TBLFASTERSATISBASLIK AS FSB ON FSH.BASLIKIND=FSB.BASLIKIND" +
                                ////"         AND FSH.SUBEIND=FSB.SUBEIND" +
                                ////"         AND FSH.KASAIND=FSB.KASAIND" +
                                ////"         LEFT JOIN TBLFASTERSTOKLAR AS STK ON FSH.STOKIND=STK.STOKNO" +
                                ////"         WHERE FSH.ISLEMTARIHI>=@par1" +
                                ////"           AND FSH.ISLEMTARIHI<=@par2" +
                                ////"           AND ISNULL(FSB.IADE, 0)=0" +
                                ////" " + QueryFasterSube +
                                ////"         GROUP BY FSB.SUBEIND," +
                                ////"                  FSB.KASAIND," +
                                ////"                  STK.MALINCINSI," +
                                ////"                  FSH.PERKODU,  FSB.IND  " +
                                ////"         UNION ALL SELECT" +
                                ////"           (SELECT SUBEADI" +
                                ////"            FROM " + FirmaId_KASA + " " +
                                ////"            WHERE IND=FSB.SUBEIND) AS Sube1," +
                                ////    //"           (SELECT KASAADI" +
                                ////    //"            FROM " + FirmaId_KASA + " " +
                                ////    //"            WHERE IND=FSB.KASAIND) AS Kasa," +
                                ////    //"           (SELECT IND" +
                                ////    //"            FROM " + FirmaId_KASA + " " +
                                ////    //"            WHERE IND=FSB.KASAIND) AS Id," +
                                ////    "                FSB.IND  ISLEMSAYISI," +
                                ////"                          SUM(FSH.MIKTAR)*-1.00 AS MIKTAR," +
                                ////"                          SUM((((MIKTAR*SATISFIYATI) * (100-ISNULL(FSH.ISK1, 0))/100) * (100-ISNULL(FSH.ISK2, 0))/100) * (100-ISNULL(FSB.ALTISKORAN, 0))/100) *-1.00 AS TUTAR," +
                                ////"                          STK.MALINCINSI AS ProductName," +
                                ////"                          FSH.PERKODU" +
                                ////"         FROM TBLFASTERSATISHAREKET AS FSH" +
                                ////"         LEFT JOIN TBLFASTERSATISBASLIK AS FSB ON FSH.BASLIKIND=FSB.BASLIKIND" +
                                ////"         AND FSH.SUBEIND=FSB.SUBEIND" +
                                ////"         AND FSH.KASAIND=FSB.KASAIND" +
                                ////"         LEFT JOIN TBLFASTERSTOKLAR AS STK ON FSH.STOKIND=STK.STOKNO" +
                                ////"         WHERE FSH.ISLEMTARIHI>=@par1" +
                                ////"           AND FSH.ISLEMTARIHI<=@par2" +
                                ////"           AND ISNULL(FSB.IADE, 0)=1" +
                                ////" " + QueryFasterSube +
                                ////"         GROUP BY FSB.SUBEIND," +
                                ////"                  FSB.KASAIND," +
                                ////"                  STK.MALINCINSI," +
                                ////"                  FSH.PERKODU,  FSB.IND  )) T" +
                                ////"  GROUP BY T.Sube1," +
                                //////"         T.Kasa," +
                                //////"         T.Id," +
                                ////"         T.PERKODU";


                                // Query =
                                //    "DECLARE @par1 nvarchar(20) = '{TARIH1}' ;" +
                                //    "DECLARE @par2 nvarchar(20) = '{TARIH2}' ;" +
                                //    "DECLARE @Sube nvarchar(100) = '{SUBE}';" +
                                //    " SELECT T.Sube1," +
                                //    "       (ISLEMSAYISI) ISLEMSAYISI," +
                                //    "       SUM(T.MIKTAR) MIKTAR," +
                                //    "       SUM(T.TUTAR) TUTAR," +
                                //    "       T.PERKODU " +
                                //    " FROM (" +
                                //    "        (SELECT" +
                                //    "           (SELECT SUBEADI" +
                                //    "      FROM  " + FirmaId_SUBE + " " +
                                //    "            WHERE IND=FSB.SUBEIND) AS Sube1," +
                                //    "                                ((SELECT " +
                                //    "               COUNT(FSB.IND )" +
                                //    "                " +
                                //    "         FROM TBLFASTERSATISBASLIK AS FSB " +
                                //    "       " +
                                //    "         WHERE FSB.ISLEMTARIHI>=@par1" +
                                //    "           AND FSB.ISLEMTARIHI<=@par2" +
                                //    "           AND ISNULL(FSB.IADE, 0)=0" +
                                //    "           AND FSB.SUBEIND IN(" + FasterSubeIND + ") ) ) ISLEMSAYISI," +
                                //    "                SUM(FSH.MIKTAR) AS MIKTAR," +
                                //    "                SUM((((MIKTAR*SATISFIYATI) * (100-ISNULL(FSH.ISK1, 0))/100) * (100-ISNULL(FSH.ISK2, 0))/100) * (100-ISNULL(FSB.ALTISKORAN, 0))/100) AS TUTAR," +
                                //    "                STK.MALINCINSI AS ProductName," +
                                //    "                FSH.PERKODU" +
                                //    "         FROM TBLFASTERSATISHAREKET AS FSH" +
                                //    "         LEFT JOIN TBLFASTERSATISBASLIK AS FSB ON FSH.BASLIKIND=FSB.BASLIKIND" +
                                //    "         AND FSH.SUBEIND=FSB.SUBEIND" +
                                //    "         AND FSH.KASAIND=FSB.KASAIND" +
                                //    "         LEFT JOIN TBLFASTERSTOKLAR AS STK ON FSH.STOKIND=STK.STOKNO" +
                                //    "         WHERE FSH.ISLEMTARIHI>=@par1" +
                                //    "           AND FSH.ISLEMTARIHI<=@par2" +
                                //    "           AND ISNULL(FSB.IADE, 0)=0" +
                                //    "           AND FSH.SUBEIND IN(" + FasterSubeIND + ")" +
                                //    "         GROUP BY FSB.SUBEIND," +
                                //    "                  FSB.KASAIND," +
                                //    "                  STK.MALINCINSI," +
                                //    "                  FSH.PERKODU," +
                                //    "                  FSB.IND" +
                                //    "         UNION ALL SELECT" +
                                //    "           (SELECT SUBEADI" +
                                //"            FROM " + FirmaId_KASA + " " +
                                //    "            WHERE IND=FSB.SUBEIND) AS Sube1," +
                                //    "                                          ((SELECT " +
                                //    "               count(FSB.IND )" +
                                //    "                " +
                                //    "         FROM TBLFASTERSATISBASLIK AS FSB " +
                                //    "       " +
                                //    "         WHERE FSB.ISLEMTARIHI>=@par1" +
                                //    "           AND FSB.ISLEMTARIHI<=@par2" +
                                //    "           AND ISNULL(FSB.IADE, 0)=1" +
                                //    "           AND FSB.SUBEIND IN(" + FasterSubeIND + ") ) ) ISLEMSAYISI," +
                                //    "                          SUM(FSH.MIKTAR)*-1.00 AS MIKTAR," +
                                //    "                          SUM((((MIKTAR*SATISFIYATI) * (100-ISNULL(FSH.ISK1, 0))/100) * (100-ISNULL(FSH.ISK2, 0))/100) * (100-ISNULL(FSB.ALTISKORAN, 0))/100) *-1.00 AS TUTAR," +
                                //    "                          STK.MALINCINSI AS ProductName," +
                                //    "                          FSH.PERKODU" +
                                //    "         FROM TBLFASTERSATISHAREKET AS FSH" +
                                //    "         LEFT JOIN TBLFASTERSATISBASLIK AS FSB ON FSH.BASLIKIND=FSB.BASLIKIND" +
                                //    "         AND FSH.SUBEIND=FSB.SUBEIND" +
                                //    "         AND FSH.KASAIND=FSB.KASAIND" +
                                //    "         LEFT JOIN TBLFASTERSTOKLAR AS STK ON FSH.STOKIND=STK.STOKNO" +
                                //    "         WHERE FSH.ISLEMTARIHI>=@par1" +
                                //    "           AND FSH.ISLEMTARIHI<=@par2" +
                                //    "           AND ISNULL(FSB.IADE, 0)=1" +
                                //    "           AND FSH.SUBEIND IN(" + FasterSubeIND + ")" +
                                //    "         GROUP BY FSB.SUBEIND," +
                                //    "                  FSB.KASAIND," +
                                //    "                  STK.MALINCINSI," +
                                //    "                  FSH.PERKODU," +
                                //    "                  FSB.IND)) T" +
                                //    " GROUP BY T.Sube1, " +
                                //    "         T.PERKODU," +
                                //    "		 T.ISLEMSAYISI  ";


                                Query =
                                " DECLARE @par1 nvarchar(20) = '{TARIH1}' ;" +
                                " DECLARE @par2 nvarchar(20) = '{TARIH2}' ;" +
                                " DECLARE @Sube nvarchar(100) = '{SUBE}';" +
                                " WITH " +
                                " ISLEMSAY AS (SELECT COUNT(FSB.IND) SAY,FSH.PERNO" +
                                " FROM TBLFASTERSATISHAREKET AS FSH" +
                                " LEFT JOIN TBLFASTERSATISBASLIK AS FSB ON FSH.BASLIKIND=FSB.BASLIKIND and  FSH.BASBARCODE=FSB.BARCODE" +
                                " WHERE FSB.ISLEMTARIHI>=@par1" +
                                " AND FSB.ISLEMTARIHI<=@par2" +
                                " AND ISNULL(FSB.IADE, 0)=0" +
                                " AND FSB.SUBEIND IN (" + FasterSubeIND + ") GROUP BY FSH.PERNO)" +
                                " SELECT T.Sube1," +
                                " (ISLEMSAYISI) ISLEMSAYISI," +
                                " SUM(T.MIKTAR) MIKTAR," +
                                " SUM(T.TUTAR) TUTAR," +
                                " T.PERKODU" +
                                " FROM (" +
                                "        (SELECT" +
                                "           (SELECT SUBEADI" +
                                "            FROM  " + FirmaId_SUBE + "" +
                                "            WHERE IND=FSB.SUBEIND) AS Sube1," +
                                "   SUM(SAY) ISLEMSAYISI," +
                                "                SUM(FSH.MIKTAR) AS MIKTAR," +
                                "                SUM((((MIKTAR*SATISFIYATI) * (100-ISNULL(FSH.ISK1, 0))/100) * (100-ISNULL(FSH.ISK2, 0))/100) * (100-ISNULL(FSB.ALTISKORAN, 0))/100) AS TUTAR," +
                                "                STK.MALINCINSI AS ProductName," +
                                "                FSH.PERKODU" +
                                "         FROM TBLFASTERSATISHAREKET AS FSH" +
                                "         LEFT JOIN TBLFASTERSATISBASLIK AS FSB ON FSH.BASLIKIND=FSB.BASLIKIND and  FSH.BASBARCODE=FSB.BARCODE" +
                                "         AND FSH.SUBEIND=FSB.SUBEIND" +
                                "         AND FSH.KASAIND=FSB.KASAIND" +
                                "         LEFT JOIN TBLFASTERSTOKLAR AS STK ON FSH.STOKIND=STK.STOKNO" +
                                "		 LEFT JOIN ISLEMSAY ON ISLEMSAY.PERNO=FSH.PERNO" +
                                "         WHERE FSH.ISLEMTARIHI>=@par1" +
                                "           AND FSH.ISLEMTARIHI<=@par2" +
                                "           AND ISNULL(FSB.IADE, 0)=0" +
                                "           AND FSH.SUBEIND IN(" + FasterSubeIND + ")" +
                                "         GROUP BY FSB.SUBEIND," +
                                "                  FSB.KASAIND," +
                                "                  STK.MALINCINSI," +
                                "                  FSH.PERKODU," +
                                "                  FSB.IND" +
                                "         UNION ALL SELECT" +
                                "           (SELECT SUBEADI" +
                                "          FROM " + FirmaId_KASA + " " +
                                "            WHERE IND=FSB.SUBEIND) AS Sube1," +
                                "            SUM(SAY) ISLEMSAYISI," +
                                "                          SUM(FSH.MIKTAR)*-1.00 AS MIKTAR," +
                                "                          SUM((((MIKTAR*SATISFIYATI) * (100-ISNULL(FSH.ISK1, 0))/100) * (100-ISNULL(FSH.ISK2, 0))/100) * (100-ISNULL(FSB.ALTISKORAN, 0))/100) *-1.00 AS TUTAR," +
                                "                          STK.MALINCINSI AS ProductName," +
                                "                          FSH.PERKODU" +
                                "         FROM TBLFASTERSATISHAREKET AS FSH" +
                                "         LEFT JOIN TBLFASTERSATISBASLIK AS FSB ON FSH.BASLIKIND=FSB.BASLIKIND and  FSH.BASBARCODE=FSB.BARCODE" +
                                "         AND FSH.SUBEIND=FSB.SUBEIND" +
                                "         AND FSH.KASAIND=FSB.KASAIND" +
                                "         LEFT JOIN TBLFASTERSTOKLAR AS STK ON FSH.STOKIND=STK.STOKNO" +
                                "		  LEFT JOIN ISLEMSAY ON ISLEMSAY.PERNO=FSH.PERNO" +
                                "         WHERE FSH.ISLEMTARIHI>=@par1" +
                                "           AND FSH.ISLEMTARIHI<=@par2" +
                                "           AND ISNULL(FSB.IADE, 0)=1" +
                                "           AND FSH.SUBEIND IN(" + FasterSubeIND + ")" +
                                "         GROUP BY FSB.SUBEIND," +
                                "                  FSB.KASAIND," +
                                "                  STK.MALINCINSI," +
                                "                  FSH.PERKODU," +
                                "                  FSB.IND)) T" +
                                " GROUP BY T.Sube1," +
                                " T.PERKODU," +
                                " T.ISLEMSAYISI";


                            if (subeid != null && !subeid.Equals("0") && personel != null && personel != "0")
                                Query =
                                         "DECLARE @par1 nvarchar(20) = '{TARIH1}' ;" +
                                         "DECLARE @par2 nvarchar(20) = '{TARIH2}' ;" +
                                         "DECLARE @Sube nvarchar(100) = '{SUBE}';" +
                                         "DECLARE @par3 nvarchar(100) = '{Personel}';" +

                                         " SELECT " +
                                         " T.Sube1," +
                                         " T.Kasa," +
                                         " T.Id," +
                                          " COUNT(ISLEMSAYISI) ISLEMSAYISI," +
                                         " SUM(T.MIKTAR) MIKTAR," +
                                         " SUM(T.TUTAR) TUTAR," +
                                         " T.ProductName," +
                                         " T.PERKODU" +
                                         " FROM (" +
                                         "  (SELECT" +
                                         "      (SELECT SUBEADI" +
                                         "      FROM  " + FirmaId_SUBE + " " +
                                         "      WHERE IND=FSB.SUBEIND) AS Sube1," +
                                         "     (SELECT KASAADI" +
                                         "      FROM " + FirmaId_KASA + " " +
                                         "      WHERE IND=FSB.KASAIND) AS Kasa," +
                                         "     (SELECT IND" +
                                         "      FROM " + FirmaId_KASA + " " +
                                         "      WHERE IND=FSB.KASAIND) AS Id," +
                                         "          SUM(FSH.MIKTAR) AS MIKTAR," +
                                         "                FSB.IND  ISLEMSAYISI," +
                                         "          SUM((((MIKTAR*SATISFIYATI) * (100-ISNULL(FSH.ISK1, 0))/100) * (100-ISNULL(FSH.ISK2, 0))/100) * (100-ISNULL(FSB.ALTISKORAN, 0))/100) AS TUTAR," +
                                         "          STK.MALINCINSI AS ProductName," +
                                         "		  FSH.PERKODU" +
                                         "   FROM TBLFASTERSATISHAREKET AS FSH" +
                                         "   LEFT JOIN TBLFASTERSATISBASLIK AS FSB ON FSH.BASLIKIND=FSB.BASLIKIND and  FSH.BASBARCODE=FSB.BARCODE" +
                                         "   AND FSH.SUBEIND=FSB.SUBEIND" +
                                         "   AND FSH.KASAIND=FSB.KASAIND" +
                                         "   LEFT JOIN TBLFASTERSTOKLAR AS STK ON FSH.STOKIND=STK.STOKNO" +
                                         "   WHERE FSH.ISLEMTARIHI>=@par1" +
                                         "     AND FSH.ISLEMTARIHI<=@par2" +
                                         "     AND ISNULL(FSB.IADE, 0)=0" +
                                         "     AND FSH.PERKODU=@par3 " +
                                         " " + QueryFasterSube +
                                         "   GROUP BY FSB.SUBEIND," +
                                         "            FSB.KASAIND," +
                                         "            STK.MALINCINSI," +
                                         "			FSH.PERKODU,  FSB.IND " +
                                         "			" +
                                         "			UNION ALL SELECT" +
                                         " (SELECT SUBEADI" +
                                         "      FROM " + FirmaId_KASA + " " +
                                         "      WHERE IND=FSB.SUBEIND) AS Sube1," +
                                         "     (SELECT KASAADI" +
                                         "      FROM " + FirmaId_KASA + " " +
                                         "      WHERE IND=FSB.KASAIND) AS Kasa," +
                                         "     (SELECT IND" +
                                         "      FROM " + FirmaId_KASA + " " +
                                         "      WHERE IND=FSB.KASAIND) AS Id," +
                                            "                FSB.IND  ISLEMSAYISI," +
                                         "          SUM(FSH.MIKTAR)*-1.00 AS MIKTAR," +
                                         "          SUM((((MIKTAR*SATISFIYATI) * (100-ISNULL(FSH.ISK1, 0))/100) * (100-ISNULL(FSH.ISK2, 0))/100) * (100-ISNULL(FSB.ALTISKORAN, 0))/100) *-1.00 AS TUTAR," +
                                         "          STK.MALINCINSI AS ProductName," +
                                         "		  FSH.PERKODU" +
                                         "   FROM TBLFASTERSATISHAREKET AS FSH" +
                                         "   LEFT JOIN TBLFASTERSATISBASLIK AS FSB ON FSH.BASLIKIND=FSB.BASLIKIND and  FSH.BASBARCODE=FSB.BARCODE" +
                                         "   AND FSH.SUBEIND=FSB.SUBEIND" +
                                         "   AND FSH.KASAIND=FSB.KASAIND" +
                                         "   LEFT JOIN TBLFASTERSTOKLAR AS STK ON FSH.STOKIND=STK.STOKNO" +
                                         "   WHERE FSH.ISLEMTARIHI>=@par1" +
                                         "     AND FSH.ISLEMTARIHI<=@par2" +
                                         "     AND ISNULL(FSB.IADE, 0)=1" +
                                         "     AND FSH.PERKODU=@par3 " +
                                         " " + QueryFasterSube +
                                         "   GROUP BY FSB.SUBEIND," +
                                         "            FSB.KASAIND," +
                                         "            STK.MALINCINSI," +
                                         "			FSH.PERKODU,  FSB.IND  " +
                                         "			" +
                                         "			) ) T where PERKODU=@par3 " +
                                         "			GROUP BY " +
                                         "			T.Sube1," +
                                         " T.Kasa," +
                                         " T.Id," +
                                         " T.ProductName," +
                                         " T.PERKODU";

                            #endregion FASTER ONLINE QUARY
                        }
                        else
                        {
                            if (subeid != null && !subeid.Equals("0") && personel == null)// sube secili degilse ilk giris yapilan sql  
                                Query = File.ReadAllText(HostingEnvironment.MapPath("/Sql/SqlFaster/WaiterSales/WaiterSalesOFLINE1.sql"), System.Text.UTF8Encoding.Default);

                            if (subeid != null && !subeid.Equals("0") && personel == "0")
                                Query = File.ReadAllText(HostingEnvironment.MapPath("/Sql/SqlFaster/WaiterSales/WaiterSalesOFLINE2.sql"), System.Text.UTF8Encoding.Default);

                            if (subeid != null && !subeid.Equals("0") && personel != null && personel != "0")
                                Query = File.ReadAllText(HostingEnvironment.MapPath("/Sql/SqlFaster/WaiterSales/WaiterSalesOFLINE3.sql"), System.Text.UTF8Encoding.Default);
                        }
                    }
                    else if (AppDbType == "5")
                    {
                        if (subeid != null && !subeid.Equals("0") && personel == null)// sube secili degilse ilk giris yapilan sql  
                            Query = File.ReadAllText(HostingEnvironment.MapPath("/Sql/VPos/PersonelTahsilatRaporu/WaiterSales.sql"), System.Text.UTF8Encoding.Default);

                        if (subeid != null && !subeid.Equals("0") && personel == "0")
                            Query = File.ReadAllText(HostingEnvironment.MapPath("/Sql/VPos/PersonelTahsilatRaporu/WaiterSales2.sql"), System.Text.UTF8Encoding.Default);

                        if (subeid != null && !subeid.Equals("0") && personel != null && personel != "0")
                        {
                            Query = File.ReadAllText(HostingEnvironment.MapPath("/Sql/VPos/PersonelTahsilatRaporu/WaiterSales3UrunEslestirme.sql"), System.Text.UTF8Encoding.Default);

                        }
                    }
                    #endregion SEFİM YENI - ESKİ FASTER SQL

                    Query = Query.Replace("{TARIH1}", QueryTimeStart);
                    Query = Query.Replace("{TARIH2}", QueryTimeEnd);
                    Query = Query.Replace("{SUBE}", SubeAdi);
                    Query = Query.Replace("{Personel}", personel);
                    Query = Query.Replace("{SUBE2}", vPosSubeKodu);
                    Query = Query.Replace("{KASAKODU}", vPosKasaKodu);

                    if (ID == "1")
                    {
                        #region GET DATA
                        try
                        {
                            string connString = "Provider=SQLOLEDB;Server=" + SubeIP + ";User Id=" + SqlName + ";Password=" + SqlPassword + ";Database=" + DBName + "";

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
                                            lock (locked)
                                            {
                                                Liste.Add(items);
                                            }
                                        }
                                        else
                                        {
                                            WaiterSalesViewModel items = new WaiterSalesViewModel();
                                            items.Sube = SubeAdi; //f.RTS(SubeUrunCiroDt.Rows[0], "Sube");
                                            items.SubeID = Convert.ToInt32(SubeId);
                                            items.UserName = "Personel Satış"; //f.RTS(SubeR, "PersonelAdi");
                                            foreach (DataRow SubeR in AcikHesapDt.Rows)
                                            {
                                                items.Total += f.RTD(SubeR, "TUTAR");
                                            }

                                            lock (locked)
                                            {
                                                Liste.Add(items);
                                            }
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
                                                items.Miktar = f.RTD(SubeR, "MIKTAR");
                                                items.IslemSayisi = f.RTD(SubeR, "ISLEMSAYISI");
                                                items.Perkodu = f.RTS(SubeR, "PERKODU");
                                                items.ProductName = f.RTS(SubeR, "ProductName");

                                                lock (locked)
                                                {
                                                    Liste.Add(items);
                                                }
                                            }
                                        }
                                        else
                                        {
                                            foreach (DataRow SubeR in AcikHesapDt.Rows)
                                            {
                                                WaiterSalesViewModel items = new WaiterSalesViewModel();
                                                items.Sube = SubeAdi;
                                                items.SubeID = Convert.ToInt32(SubeId);
                                                //items.UserName = f.RTS(SubeR, "PERKODU");
                                                items.Perkodu = f.RTS(SubeR, "PERKODU");
                                                items.Total = f.RTD(SubeR, "TUTAR");
                                                items.Miktar = f.RTD(SubeR, "MIKTAR");
                                                items.ProductName = f.RTS(SubeR, "ProductName");
                                                items.IslemSayisi = f.RTD(SubeR, "islemsayisi");

                                                lock (locked)
                                                {
                                                    Liste.Add(items);
                                                }
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
                                    lock (locked)
                                    {
                                        Liste.Add(items);
                                    }
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
                                    string connString = "Provider=SQLOLEDB;Server=" + SubeIP + ";User Id=" + SqlName + ";Password=" + SqlPassword + ";Database=" + DBName + "";

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
                                                    lock (locked)
                                                    {
                                                        Liste.Add(items);
                                                    }
                                                }
                                                else
                                                {
                                                    WaiterSalesViewModel items = new WaiterSalesViewModel();
                                                    items.Sube = SubeAdi; //f.RTS(SubeUrunCiroDt.Rows[0], "Sube");
                                                    items.SubeID = Convert.ToInt32(SubeId);
                                                    items.UserName = "Personel Satış"; //f.RTS(SubeR, "PersonelAdi");
                                                    foreach (DataRow SubeR in AcikHesapDt.Rows)
                                                    {
                                                        items.Total += f.RTD(SubeR, "TUTAR");
                                                    }

                                                    lock (locked)
                                                    {
                                                        Liste.Add(items);
                                                    }
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
                                                        items.Miktar = f.RTD(SubeR, "MIKTAR");
                                                        items.IslemSayisi = f.RTD(SubeR, "ISLEMSAYISI");
                                                        items.Perkodu = f.RTS(SubeR, "PERKODU");
                                                        items.ProductName = f.RTS(SubeR, "ProductName");

                                                        lock (locked)
                                                        {
                                                            Liste.Add(items);
                                                        }
                                                    }
                                                }
                                                else
                                                {
                                                    foreach (DataRow SubeR in AcikHesapDt.Rows)
                                                    {
                                                        WaiterSalesViewModel items = new WaiterSalesViewModel();
                                                        items.Sube = SubeAdi;
                                                        items.SubeID = Convert.ToInt32(SubeId);
                                                        //items.UserName = f.RTS(SubeR, "PERKODU");
                                                        items.Perkodu = f.RTS(SubeR, "PERKODU");
                                                        items.Total = f.RTD(SubeR, "TUTAR");
                                                        items.Miktar = f.RTD(SubeR, "MIKTAR");
                                                        items.ProductName = f.RTS(SubeR, "ProductName");
                                                        items.IslemSayisi = f.RTD(SubeR, "islemsayisi");

                                                        lock (locked)
                                                        {
                                                            Liste.Add(items);
                                                        }
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
                                            lock (locked)
                                            {
                                                Liste.Add(items);
                                            }
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
                        #endregion KULLANICININ ŞUBE YETKİ KONTROLU YAPILIYOR   
                    }
                });
                #endregion PARALLEL FOREACH     
            }
            catch (DataException ex) { }
            return Liste;
        }
    }
}