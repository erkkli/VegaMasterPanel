using SefimV2.Helper;
using SefimV2.ViewModels.User;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Hosting;

namespace SefimV2.Models
{
    public class SubeUrunCRUD
    {
        public static List<SubeUrun> List(DateTime Date1, DateTime Date2, string subeid, string ID, string payReportID, string SaatGun, bool TopluExcel = false)
        {
            List<SubeUrun> Liste = new List<SubeUrun>();
            ModelFunctions ff = new ModelFunctions();
            DateTime startDate = DateTime.Now;
            #region GİRİŞ YAPAN KULLANICININ YETKİLİ OLDUĞU ŞUBELER LİSTELENİYOR
            UserViewModel model = UsersListCRUD.YetkiliSubesi(ID);
            #endregion

            string subeid_ = string.Empty;
            try
            {
                #region Kırılımlarda ilk once db de tanımlı subeyı alıyoruz.   
                if (!subeid.Equals("") && subeid != "exportExcel")
                {
                    string[] tableNo = subeid.Split('~');
                    subeid_ = tableNo[0];
                }
                #endregion   Kırılımlarda ilk once db de tanımlı subeyı alıyoruz.  

                #region SUBSTATION LIST 

                ff.SqlConnOpen();
                string filter = "Where Status=1";
                if (subeid_ != null && !subeid_.Equals("0") && !subeid_.Equals(""))
                    filter += " and Id=" + subeid_;
                DataTable dt = ff.DataTable("select * from SubeSettings " + filter);
                ff.SqlConnClose();

                #endregion SUBSTATION LIST

                #region VEGA DB database ismini çekmek için.

                string vega_Db = "";
                ff.SqlConnOpen();
                DataTable dataVegaDb = ff.DataTable("select* from VegaDbSettings ");
                var vegaDBList = dataVegaDb.AsEnumerable().ToList<DataRow>();
                foreach (var item in vegaDBList)
                {
                    vega_Db = item["DBName"].ToString();
                }
                ff.SqlConnClose();

                #endregion VEGA DB database ismini çekmek için. 

                #region PARALLEL FORECH

                var locked = new Object();
                var dtList = dt.AsEnumerable().ToList<DataRow>();

                _ = Parallel.ForEach(dtList, new ParallelOptions { MaxDegreeOfParallelism = 20 }, r =>
                //Parallel.ForEach(dtList, r =>
                {
                    ModelFunctions f = new ModelFunctions();
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
                    string AppDbType = f.RTS(r, "AppDbType");
                    string AppDbTypeStatus = f.RTS(r, "AppDbTypeStatus");
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
                    string vPosKasaKoduParametr = string.Empty;
                    string Query = string.Empty;

                    #region Faster Kasalar seçildiyse ona göre query oluşturuluyor. 

                    string FasterSubeIND = f.RTS(r, "FasterSubeID");
                    string QueryFasterSube = string.Empty;
                    if (FasterSubeIND != null)
                    {
                        QueryFasterSube = "  and  FSH.SUBEIND IN(" + FasterSubeIND + ") ";
                    }

                    #endregion Faster Kasalar seçildiyse ona göre query oluşturuluyor.

                    if (payReportID.Equals(""))
                    {
                        if (AppDbType == "1" /*|| AppDbType == "2"*/)
                        {
                            if (SaatGun == "0") // Masa Üstü Raporu, SaatGun Raporu 1.Kırılım
                            {
                                Query = File.ReadAllText(HostingEnvironment.MapPath("/Sql/SqlReports/SaatGunMasaUstuReports/SaatGunMasaUstuReports.sql"), System.Text.UTF8Encoding.Default);
                            }
                            else if (SaatGun == "1") //  Masa Üstü Raporu, SaatGun Raporu 2.Kırılım
                            {
                                Query = File.ReadAllText(HostingEnvironment.MapPath("/Sql/SqlReports/SaatGunMasaUstuReports/SaatGunMasaUstuReportsDetay.sql"), System.Text.UTF8Encoding.Default);
                            }
                            else if (SaatGun == "3") //  Masa Üstü Raporu,
                            {
                                Query = File.ReadAllText(HostingEnvironment.MapPath("/Sql/SqlReports/GunSaatMasaUstuReports/GunSaatMasaUstuReports.sql"), System.Text.UTF8Encoding.Default);
                            }
                            else
                            {
                                if (string.IsNullOrEmpty(urunEslestirmeVarMi) || urunEslestirmeVarMi == "False")
                                {
                                    Query = File.ReadAllText(HostingEnvironment.MapPath("/Sql/SubeUrunSubeBazli.sql"), System.Text.UTF8Encoding.Default);
                                }
                                else
                                {
                                    Query = File.ReadAllText(HostingEnvironment.MapPath("/Sql/SqlNewSef/SubeUrunRaporu/SubeUrunSubeBazliUrunEslestirme.sql"), System.Text.UTF8Encoding.Default);
                                }
                            }
                        }
                        else if (AppDbType == "2")
                        {
                            if (SaatGun == "0") // Masa Üstü Raporu, SaatGun Raporu 1.Kırılım
                            {
                                Query = File.ReadAllText(HostingEnvironment.MapPath("/Sql/SqlReports/SaatGunMasaUstuReports/SaatGunMasaUstuReports.sql"), System.Text.UTF8Encoding.Default);
                            }
                            else if (SaatGun == "1") //  Masa Üstü Raporu, SaatGun Raporu 2.Kırılım
                            {
                                Query = File.ReadAllText(HostingEnvironment.MapPath("/Sql/SqlReports/SaatGunMasaUstuReports/SaatGunMasaUstuReportsDetay.sql"), System.Text.UTF8Encoding.Default);
                            }
                            else if (SaatGun == "3") //  Masa Üstü Raporu,
                            {
                                Query = File.ReadAllText(HostingEnvironment.MapPath("/Sql/SqlReports/GunSaatMasaUstuReports/GunSaatMasaUstuReports.sql"), System.Text.UTF8Encoding.Default);
                            }
                            else
                            {
                                if (string.IsNullOrEmpty(urunEslestirmeVarMi) || urunEslestirmeVarMi == "False")
                                {
                                    Query = File.ReadAllText(HostingEnvironment.MapPath("/Sql/UrunGrubuEskiSefimReports/SubeUrunSubeBazli.sql"), System.Text.UTF8Encoding.Default);
                                }
                                else
                                {
                                    Query = File.ReadAllText(HostingEnvironment.MapPath("/Sql/SqlNewSef/SubeUrunRaporu/SubeUrunSubeBazliUrunEslestirme.sql"), System.Text.UTF8Encoding.Default);
                                }
                            }
                        }
                        else if (AppDbType == "3")
                        {
                            #region Kırılımlarda Ana Şube Altındaki IND id alıyorum 

                            if (!subeid.Equals("") && subeid != "exportExcel")
                            {
                                string[] tableNo = subeid.Split('~');
                                if (tableNo.Length >= 2)
                                {
                                    subeid = tableNo[1];
                                }
                            }

                            #endregion  Kırılımlarda Ana Şube Altındaki IND id alıyorum 

                            if (AppDbTypeStatus == "True")
                            {
                                if (subeid == null && subeid.Equals("0") || subeid == "")// sube secili degilse ilk giris yapilan sql

                                    #region FASTER ONLINE QUARY                             
                                    // Faster Online List
                                    Query =
                                                        " DECLARE @Sube nvarchar(100) = '{SUBEADI}';" +
                                                        " DECLARE @par1 nvarchar(20) = '{TARIH1}';" +
                                                        " DECLARE @par2 nvarchar(20) = '{TARIH2}';" +

                                                        " SELECT T.Sube1," +
                                                        "       T.Kasa," +
                                                        "       T.Id," +
                                                        "       SUM(T.MIKTAR) MIKTAR," +
                                                        "       SUM(T.TUTAR) TUTAR" +
                                                        " FROM (" +
                                                        "        (SELECT" +
                                                        "           (SELECT SUBEADI" +
                                                        "         FROM " + FirmaId_SUBE + " " +
                                                        "            WHERE IND=FSB.SUBEIND) AS Sube1," +
                                                        "           (SELECT KASAADI" +
                                                        "          FROM " + FirmaId_KASA + " " +
                                                        "            WHERE IND=FSB.KASAIND) AS Kasa," +
                                                        "           (SELECT IND" +
                                                        "            FROM " + FirmaId_KASA + " " +
                                                        "            WHERE IND=FSB.KASAIND) AS Id," +

                                                        "                SUM(FSH.MIKTAR) AS MIKTAR," +
                                                        "                SUM((((MIKTAR*SATISFIYATI) * (100-ISNULL(FSH.ISK1, 0))/100) * (100-ISNULL(FSH.ISK2, 0))/100) * (100-ISNULL(FSB.ALTISKORAN, 0))/100) AS TUTAR" +
                                                        "         FROM TBLFASTERSATISHAREKET AS FSH" +
                                                        "         LEFT JOIN TBLFASTERSATISBASLIK AS FSB ON FSH.BASLIKIND=FSB.BASLIKIND  and  FSH.BASBARCODE = FSB.BARCODE" +
                                                        "         AND FSH.SUBEIND=FSB.SUBEIND" +
                                                        "         AND FSH.KASAIND=FSB.KASAIND " +
                                                        "          AND FSH.BASBARCODE = FSB.BARCODE " +
                                                        "         LEFT JOIN F0" + FirmaId + "TBLSTOKLAR AS STK ON FSH.STOKIND=STK.IND and FSH.BIRIMIND=STK.BIRIMEX" +
                                                        "         WHERE FSH.ISLEMTARIHI>=@par1" +
                                                        "           AND FSH.ISLEMTARIHI<=@par2" +
                                                        "           AND ISNULL(FSB.IADE, 0)=0" +
                                                             " " + QueryFasterSube +
                                                        "         GROUP BY FSB.SUBEIND," +
                                                        "                  FSB.KASAIND" +
                                                        "         UNION ALL SELECT" +
                                                        "           (SELECT SUBEADI" +
                                                          "      FROM " + FirmaId_SUBE + "" +
                                                        "            WHERE IND=FSB.SUBEIND) AS Sube1," +
                                                        "           (SELECT KASAADI" +
                                                          "      FROM " + FirmaId_KASA + " " +
                                                        "            WHERE IND=FSB.KASAIND) AS Kasa," +
                                                        "           (SELECT IND" +
                                                          "      FROM " + FirmaId_KASA + " " +
                                                        "            WHERE IND=FSB.KASAIND) AS Id," +
                                                        "                          SUM(FSH.MIKTAR)*-1 AS MIKTAR," +
                                                        "                          SUM((((MIKTAR*SATISFIYATI) * (100-ISNULL(FSH.ISK1, 0))/100) * (100-ISNULL(FSH.ISK2, 0))/100) * (100-ISNULL(FSB.ALTISKORAN, 0))/100) *-1.00 AS TUTAR" +
                                                        "         FROM TBLFASTERSATISHAREKET AS FSH" +
                                                        "         LEFT JOIN TBLFASTERSATISBASLIK AS FSB ON FSH.BASLIKIND=FSB.BASLIKIND  and  FSH.BASBARCODE = FSB.BARCODE" +
                                                        "         AND FSH.SUBEIND=FSB.SUBEIND" +
                                                        "         AND FSH.KASAIND=FSB.KASAIND" +
                                                        "          AND FSH.BASBARCODE = FSB.BARCODE " +
                                                        "         LEFT JOIN  F0" + FirmaId + "TBLSTOKLAR AS STK ON FSH.STOKIND=STK.IND and FSH.BIRIMIND=STK.BIRIMEX" +
                                                        "         WHERE FSH.ISLEMTARIHI>=@par1" +
                                                        "           AND FSH.ISLEMTARIHI<=@par2" +
                                                        "           AND ISNULL(FSB.IADE, 0)=1" +
                                                           " " + QueryFasterSube +
                                                        "         GROUP BY FSB.SUBEIND," +
                                                        "                  FSB.KASAIND)) T" +
                                                        " GROUP BY T.Sube1," +
                                                        "         T.Kasa," +
                                                        "         T.Id";

                                #endregion FASTER ONLINE QUARY

                                if (subeid != null && !subeid.Equals("0") && subeid != "")
                                    #region FASTER ONLINE QUARY
                                    //Query =
                                    //        " declare @Sube nvarchar(100) = '{SUBEADI}';" +
                                    //        " declare @par1 nvarchar(20) = '{TARIH1}';" +
                                    //        " declare @par2 nvarchar(20) = '{TARIH2}';" +
                                    //        " DECLARE @FirmaInd nvarchar(100) = '{FIRMAIND}';" +
                                    //        " (SELECT (SELECT SUBEADI FROM  " + FirmaId_SUBE + " WHERE IND=FSB.SUBEIND) AS Sube1," +
                                    //        " (SELECT KASAADI FROM  " + FirmaId_KASA + " WHERE IND=FSB.KASAIND) AS Kasa," +
                                    //        " (SELECT IND FROM  F0" + FirmaId + "TBLKRDKASALAR WHERE IND=FSB.KASAIND) AS Id," +
                                    //        " SUM(FSH.MIKTAR) AS MIKTAR, SUM((((MIKTAR*SATISFIYATI) * (100-ISNULL(FSH.ISK1,0))/100) * (100-ISNULL(FSH.ISK2,0))/100) * (100-ISNULL(FSB.ALTISKORAN,0))/100)  AS TUTAR,  STK.MALINCINSI AS ProductName   " +
                                    //        " FROM TBLFASTERSATISHAREKET AS FSH " +
                                    //        " LEFT JOIN TBLFASTERSATISBASLIK AS FSB ON FSH.BASLIKIND=FSB.BASLIKIND AND FSH.SUBEIND=FSB.SUBEIND AND FSH.KASAIND=FSB.KASAIND " +
                                    //        " LEFT JOIN F0" + FirmaId + "TBLSTOKLAR AS STK ON FSH.STOKIND=STK.IND WHERE FSH.ISLEMTARIHI>=@par1 AND FSH.ISLEMTARIHI<=@par2  AND FIRMAIND= @FirmaInd  AND ISNULL(FSB.IADE,0)=0 " +
                                    //        " GROUP BY FSB.SUBEIND,FSB.KASAIND,STK.MALINCINSI)";

                                    Query =
                                            "DECLARE @Sube nvarchar(100) = '{SUBEADI}';" +
                                            "DECLARE @par1 nvarchar(20) = '{TARIH1}';" +
                                            "DECLARE @par2 nvarchar(20) = '{TARIH2}';" +
                                            " SELECT T.Sube1" +
                                            "	,T.Kasa" +
                                            "	,T.Id" +
                                            "	,SUM(T.MIKTAR) MIKTAR" +
                                            "	,SUM(T.TUTAR) TUTAR" +
                                            "	,T.ProductName" +
                                            "	,T.ProductCode" +
                                            "	" +
                                            " FROM ( " +
                                            "	(" +
                                            "		SELECT (" +
                                            "				SELECT SUBEADI" +
                                            "				FROM  " + FirmaId_SUBE + "" +
                                            "				WHERE IND = FSB.SUBEIND" +
                                            "				) AS Sube1" +
                                            "			,(" +
                                            "				SELECT KASAADI" +
                                            "				FROM  " + FirmaId_KASA + " " +
                                            "				WHERE IND = FSB.KASAIND" +
                                            "				) AS Kasa" +
                                            "			,(" +
                                            "				SELECT IND" +
                                            "				FROM  " + FirmaId_KASA + " " +
                                            "				WHERE IND = FSB.KASAIND" +
                                            "				) AS Id" +
                                            "			,SUM(FSH.MIKTAR) AS MIKTAR" +
                                            "			,SUM((((FSH.MIKTAR * FSH.SATISFIYATI) * (100 - ISNULL(FSH.ISK1, 0)) / 100) * (100 - ISNULL(FSH.ISK2, 0)) / 100) * (100 - ISNULL(FSB.ALTISKORAN, 0)) / 100) AS TUTAR" +
                                            "			,STK.MALINCINSI AS ProductName" +
                                            "			,STK.STOKKODU AS ProductCode" +
                                            "		FROM TBLFASTERSATISHAREKET AS FSH" +
                                            "		LEFT JOIN TBLFASTERSATISBASLIK AS FSB ON FSH.BASLIKIND = FSB.BASLIKIND" +
                                            "			AND FSH.SUBEIND = FSB.SUBEIND" +
                                            "			AND FSH.KASAIND = FSB.KASAIND" +
                                            "          AND FSH.BASBARCODE = FSB.BARCODE " +
                                            "		LEFT JOIN F0" + FirmaId + "TBLSTOKLAR AS STK ON FSH.STOKIND = STK.IND" +
                                            "		LEFT JOIN F0" + FirmaId + "TBLBIRIMLEREX AS BR ON FSH.BIRIMIND = BR.IND" +
                                            "		WHERE FSH.ISLEMTARIHI >= @par1" +
                                            "			AND FSH.ISLEMTARIHI <= @par2" +
                                            "			AND ISNULL(FSB.IADE, 0) = 0" +
                                             " " + QueryFasterSube +
                                            "		GROUP BY FSB.SUBEIND" +
                                            "			,FSB.KASAIND" +
                                            "			,STK.MALINCINSI" +
                                            "			,STK.STOKKODU" +
                                            "		" +
                                            "		UNION ALL" +
                                            "		" +
                                            "		SELECT (" +
                                            "				SELECT SUBEADI" +
                                            "				FROM  " + FirmaId_SUBE + "" +
                                            "				WHERE IND = FSB.SUBEIND" +
                                            "				) AS Sube1" +
                                            "			,(" +
                                            "				SELECT KASAADI" +
                                            "				FROM  " + FirmaId_KASA + " " +
                                            "				WHERE IND = FSB.KASAIND" +
                                            "				) AS Kasa" +
                                            "			,(" +
                                            "				SELECT IND" +
                                            "				FROM  " + FirmaId_KASA + " " +
                                            "				WHERE IND = FSB.KASAIND" +
                                            "				) AS Id" +
                                            "			,SUM(FSH.MIKTAR) * - 1.00 AS MIKTAR" +
                                            "			,SUM((((FSH.MIKTAR * FSH.SATISFIYATI) * (100 - ISNULL(FSH.ISK1, 0)) / 100) * (100 - ISNULL(FSH.ISK2, 0)) / 100) * (100 - ISNULL(FSB.ALTISKORAN, 0)) / 100) * - 1.00 AS TUTAR" +
                                            "			,STK.MALINCINSI AS ProductName" +
                                            "			,STK.STOKKODU AS ProductCode" +
                                            "		FROM TBLFASTERSATISHAREKET AS FSH" +
                                            "		LEFT JOIN TBLFASTERSATISBASLIK AS FSB ON FSH.BASLIKIND = FSB.BASLIKIND" +
                                            "			AND FSH.SUBEIND = FSB.SUBEIND" +
                                            "			AND FSH.KASAIND = FSB.KASAIND" +
                                            "          AND FSH.BASBARCODE = FSB.BARCODE " +
                                            "		LEFT JOIN F0" + FirmaId + "TBLSTOKLAR AS STK ON FSH.STOKIND = STK.IND" +
                                            "		LEFT JOIN F0" + FirmaId + "TBLBIRIMLEREX AS BR ON FSH.BIRIMIND = BR.IND" +
                                            "		WHERE FSH.ISLEMTARIHI >= @par1" +
                                            "			AND FSH.ISLEMTARIHI <= @par2" +
                                            "			AND ISNULL(FSB.IADE, 0) = 1" +
                                               " " + QueryFasterSube +
                                            "		GROUP BY FSB.SUBEIND" +
                                            "			,FSB.KASAIND" +
                                            "			,STK.MALINCINSI" +
                                            "			,STK.STOKKODU" +
                                            "		)" +
                                            "	) T " +
                                            " GROUP BY T.Sube1 " +
                                            "	,T.Kasa" +
                                            "	,T.Id" +
                                            "	,T.ProductCode" +
                                            "	,T.ProductName" +
                                            "	ORDER BY MIKTAR DESC";

                                #endregion FASTER ONLINE QUARY
                            }
                            else
                            {
                                if (subeid == null && subeid.Equals("0") || subeid == "")
                                    Query =
                                      "DECLARE @Sube nvarchar(100) = '{SUBEADI}';" +
                                      "DECLARE @par1 nvarchar(20) = '{TARIH1}';" +
                                      "DECLARE @par2 nvarchar(20) = '{TARIH2}';" +
                                      " SELECT " +
                                      " T.Sube1," +
                                      " T.Kasa," +
                                      " T.Id," +
                                      " SUM(T.MIKTAR) MIKTAR," +
                                      " SUM(T.TUTAR) TUTAR" +
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
                                      "      WHERE KASANO=FSB.KASAIND) AS Id," +
                                      "          SUM(FSH.MIKTAR) AS MIKTAR," +
                                      "          SUM((((MIKTAR*SATISFIYATI) * (100-ISNULL(FSH.ISK1, 0))/100) * (100-ISNULL(FSH.ISK2, 0))/100) * (100-ISNULL(FSB.ALTISKORAN, 0))/100) AS TUTAR" +
                                      "   FROM TBLFASTERSATISHAREKET AS FSH" +
                                      "   LEFT JOIN TBLFASTERSATISBASLIK AS FSB ON FSH.BASLIKIND=FSB.BASLIKIND" +
                                      "   AND FSH.SUBEIND=FSB.SUBEIND" +
                                      "   AND FSH.KASAIND=FSB.KASAIND" +
                                      "          AND FSH.BASBARCODE = FSB.BARCODE " +
                                      "  LEFT JOIN TBLFASTERSTOKLAR AS STK ON FSH.STOKIND=STK.STOKNO and FSH.BIRIMIND=STK.BIRIMIND" +
                                      "   WHERE FSH.ISLEMTARIHI>=@par1" +
                                      "     AND FSH.ISLEMTARIHI<=@par2" +
                                      "     AND ISNULL(FSB.IADE, 0)=0" +
                                      "   GROUP BY FSB.SUBEIND," +
                                      "            FSB.KASAIND" +
                                      "			" +
                                      "	UNION ALL" +
                                      "	SELECT" +
                                      "  (SELECT TOP 1 SUBEADI" +
                                      "      FROM TBLFASTERKASALAR" +
                                      "      WHERE SUBENO=FSB.SUBEIND) AS Sube1," +
                                      "     (SELECT KASAADI" +
                                      "      FROM TBLFASTERKASALAR" +
                                      "      WHERE KASANO=FSB.KASAIND) AS Kasa," +
                                      "     (SELECT IND" +
                                      "      FROM TBLFASTERKASALAR" +
                                      "      WHERE KASANO=FSB.KASAIND) AS Id," +
                                      "          SUM(FSH.MIKTAR)*-1 AS MIKTAR," +
                                      "          SUM((((MIKTAR*SATISFIYATI) * (100-ISNULL(FSH.ISK1, 0))/100) * (100-ISNULL(FSH.ISK2, 0))/100) * (100-ISNULL(FSB.ALTISKORAN, 0))/100) *-1.00 AS TUTAR" +
                                      "   FROM TBLFASTERSATISHAREKET AS FSH" +
                                      "   LEFT JOIN TBLFASTERSATISBASLIK AS FSB ON FSH.BASLIKIND=FSB.BASLIKIND  " +
                                      "   AND FSH.SUBEIND=FSB.SUBEIND" +
                                      "   AND FSH.KASAIND=FSB.KASAIND" +
                                      "          AND FSH.BASBARCODE = FSB.BARCODE " +
                                      "   LEFT JOIN TBLFASTERSTOKLAR AS STK ON FSH.STOKIND=STK.STOKNO and FSH.BIRIMIND=STK.BIRIMIND " +
                                      "   WHERE FSH.ISLEMTARIHI>=@par1" +
                                      "     AND FSH.ISLEMTARIHI<=@par2" +
                                      "     AND ISNULL(FSB.IADE, 0)=1" +
                                      "   GROUP BY FSB.SUBEIND," +
                                      "            FSB.KASAIND " +
                                      "			" +
                                      "			) ) T " +
                                      "			GROUP BY " +
                                      "			T.Sube1," +
                                      "T.Kasa," +
                                      "T.Id";

                                //Query = File.ReadAllText(HostingEnvironment.MapPath("/Sql/SqlFaster/SubeUrun/SubeUrunFasterOFLINE1.sql"), System.Text.UTF8Encoding.Default);
                                if (subeid != null && !subeid.Equals("0") && subeid != "")
                                    Query =
                                       " DECLARE @Sube nvarchar(100) = '{SUBEADI}';" +
                                       " DECLARE @par1 nvarchar(20) = '{TARIH1}';" +
                                       " DECLARE @par2 nvarchar(20) = '{TARIH2}';" +
                                       " SELECT " +
                                       " T.Sube1," +
                                       " T.Kasa," +
                                       " T.ID," +
                                       " T.MALINCINSI ProductName," +
                                       " SUM(T.MIKTAR) MIKTAR," +
                                       " SUM(T.TUTAR) TUTAR" +
                                       " FROM (" +
                                       "  (SELECT" +
                                       "     (SELECT SUBEADI" +
                                       "       FROM " + FirmaId_SUBE + " " +
                                       "      WHERE IND=FSB.SUBEIND) AS Sube1," +
                                       "     (SELECT KASAADI" +
                                       "     FROM " + FirmaId_KASA + "" +
                                       "      WHERE IND=FSB.KASAIND) AS Kasa," +
                                       "     (SELECT IND" +
                                       "     FROM " + FirmaId_KASA + "" +
                                       "      WHERE IND=FSB.KASAIND) AS ID," +
                                       "          SUM(FSH.MIKTAR) AS MIKTAR," +
                                       "          SUM((((MIKTAR*SATISFIYATI) * (100-ISNULL(FSH.ISK1, 0))/100) * (100-ISNULL(FSH.ISK2, 0))/100) * (100-ISNULL(FSB.ALTISKORAN, 0))/100) AS TUTAR" +
                                       "		    ,STK.MALINCINSI" +
                                       "   FROM TBLFASTERSATISHAREKET AS FSH" +
                                       "   LEFT JOIN TBLFASTERSATISBASLIK AS FSB ON FSH.BASLIKIND=FSB.BASLIKIND" +
                                       "   AND FSH.SUBEIND=FSB.SUBEIND" +
                                       "   AND FSH.KASAIND=FSB.KASAIND" +
                                       "   LEFT JOIN TBLFASTERSTOKLAR AS STK ON FSH.STOKIND=STK.STOKNO" +
                                       "   WHERE FSH.ISLEMTARIHI>=@par1" +
                                       "     AND FSH.ISLEMTARIHI<=@par2" +
                                       "     AND ISNULL(FSB.IADE, 0)=0" +
                                       "   GROUP BY FSB.SUBEIND," +
                                       "            FSB.KASAIND  ,STK.MALINCINSI" +
                                       "			" +
                                       "	UNION ALL" +
                                       "	SELECT" +
                                       "     (SELECT SUBEADI" +
                                       "       FROM " + FirmaId_SUBE + " " +
                                       "      WHERE IND=FSB.SUBEIND) AS Sube1," +
                                       "     (SELECT KASAADI" +
                                       "     FROM " + FirmaId_KASA + "" +
                                       "      WHERE IND=FSB.KASAIND) AS Kasa," +
                                       "     (SELECT IND" +
                                       "     FROM " + FirmaId_KASA + "" +
                                       "      WHERE IND=FSB.KASAIND) AS ID," +
                                       "          SUM(FSH.MIKTAR)*-1 AS MIKTAR," +
                                       "          SUM((((MIKTAR*SATISFIYATI) * (100-ISNULL(FSH.ISK1, 0))/100) * (100-ISNULL(FSH.ISK2, 0))/100) * (100-ISNULL(FSB.ALTISKORAN, 0))/100) *-1.00 AS TUTAR" +
                                       "		    ,STK.MALINCINSI" +
                                       "   FROM TBLFASTERSATISHAREKET AS FSH" +
                                       "   LEFT JOIN TBLFASTERSATISBASLIK AS FSB ON FSH.BASLIKIND=FSB.BASLIKIND" +
                                       "   AND FSH.SUBEIND=FSB.SUBEIND" +
                                       "   AND FSH.KASAIND=FSB.KASAIND" +
                                       "   LEFT JOIN TBLFASTERSTOKLAR AS STK ON FSH.STOKIND=STK.STOKNO" +
                                       "   WHERE FSH.ISLEMTARIHI>=@par1" +
                                       "     AND FSH.ISLEMTARIHI<=@par2" +
                                       "     AND ISNULL(FSB.IADE, 0)=1" +
                                       "   GROUP BY FSB.SUBEIND," +
                                       "            FSB.KASAIND  ,STK.MALINCINSI" +
                                       "			" +
                                       "			) ) T" +
                                       "			GROUP BY " +
                                       "			T.Sube1," +
                                       " T.Kasa," +
                                       " T.ID," +
                                       " T.MALINCINSI ";
                                //Query = File.ReadAllText(HostingEnvironment.MapPath("/Sql/SqlFaster/SubeUrun/SubeUrunFasterOFLINE.sql"), System.Text.UTF8Encoding.Default);
                            }
                        }
                        else if (AppDbType == "4")//NPOS>4
                        {
                            #region Kırılımlarda Ana Şube Altındaki IND id alıyorum                         
                            if (!subeid.Equals(""))
                            {
                                string[] tableNo = subeid.Split('~');
                                if (tableNo.Length >= 2)
                                {
                                    subeid = tableNo[1];
                                }

                            }
                            #endregion #region Kırılımlarda Ana Şube Altındaki IND id alıyorum 

                            #region NPOS QUARY 1.KIRILIM
                            if (subeid == null && subeid.Equals("0") || subeid == "")// sube secili degilse ilk giris yapilan sql

                                Query =
                                    " declare @Sube nvarchar(100) = '{SUBEADI}';" +
                                    " declare @Trh1 nvarchar(20) = '{TARIH1}';" +
                                    " declare @Trh2 nvarchar(20) = '{TARIH2}';" +
                                    " SELECT " +
                                    " Sube,Sube1,Kasa, SUM(MIKTAR) AS MIKTAR, SUM(TUTAR) AS TUTAR " +
                                    " FROM(SELECT Sube, Sube1, Kasa_No as Kasa, Sum(Tutar) as TUTAR, Sum(Miktar) as MIKTAR " +
                                    " FROM " + vega_Db + "..TBLMASTERENPOSSTOK AS HR  WHERE HR.BELGETARIH >= @Trh1 and HR.BELGETARIH <= @Trh2 " +
                                    " group by  Sube, Sube1, Kasa_No) AS T  group by Sube,Sube1,Kasa ,MIKTAR,TUTAR ";
                            #endregion NPOS QUARY 1.KIRILIM

                            #region NPOS QUARY 2.KIRILIM                       
                            if (subeid != null && !subeid.Equals("0") && subeid != "")
                                Query =
                                    " declare @Sube nvarchar(100) = '{SUBEADI}';" +
                                    " declare @Trh1 nvarchar(20) = '{TARIH1}';" +
                                    " declare @Trh2 nvarchar(20) = '{TARIH2}';" +
                                    " declare @ProductName nvarchar(20) = '{ProductName}';" +
                                    " SELECT " +
                                    " Sube,Sube1,Kasa_No AS Kasa, KOD1 AS ProductGroup ,MALINCINSI as ProductName ,Sum(Tutar) as TUTAR,Sum(Miktar) as MIKTAR " +
                                    " FROM " + vega_Db + "..TBLMASTERENPOSSTOK AS HR " +
                                    " WHERE HR.BELGETARIH >= @Trh1 and HR.BELGETARIH <= @Trh2 " +
                                    " group by " +
                                    " MALINCINSI," +
                                    " Sube,Sube1,Kasa_No,kod1 ";
                            #endregion NPOS QUARY 2.KIRILIM                        
                        }
                        else if (AppDbType == "5")
                        {
                            Query = File.ReadAllText(HostingEnvironment.MapPath("/Sql/VPos/UrunSatisRaporu/UrunSatisRaporu1.sql"), System.Text.UTF8Encoding.Default);
                        }
                    }
                    else
                    {
                        if (AppDbType == "1" || AppDbType == "2")
                        {
                            #region ŞUBEDE EN COK SATILAN URUNU ALMAK ICIN
                            Query = File.ReadAllText(HostingEnvironment.MapPath("/Sql/EncokSatisYapanUrun.sql"), System.Text.UTF8Encoding.Default);
                            #endregion
                        }
                        else if (AppDbType == "3")
                        {
                            if (AppDbTypeStatus == "True")
                            {
                                #region FASTER ONLINE QUARY
                                //Query =
                                //     " declare @Sube nvarchar(100) = '{SUBEADI}';" +
                                //     " declare @par1 nvarchar(20) = '{TARIH1}';" +
                                //     " declare @par2 nvarchar(20) = '{TARIH2}';" +
                                //     " DECLARE @FirmaInd nvarchar(100) = '{FIRMAIND}';" +
                                //     " (SELECT (SELECT SUBEADI FROM  " + FirmaId_SUBE + " WHERE IND=FSB.SUBEIND) AS Sube1," +
                                //     " (SELECT KASAADI FROM  " + FirmaId_KASA + " WHERE IND=FSB.KASAIND) AS Kasa," +
                                //     " (SELECT IND FROM  F0" + FirmaId + "TBLKRDKASALAR WHERE IND=FSB.KASAIND) AS Id," +
                                //     " SUM(FSH.MIKTAR) AS MIKTAR, SUM((((MIKTAR*SATISFIYATI) * (100-ISNULL(FSH.ISK1,0))/100) * (100-ISNULL(FSH.ISK2,0))/100) * (100-ISNULL(FSB.ALTISKORAN,0))/100)  AS TUTAR, STK.MALINCINSI AS ProductName  " +
                                //     " FROM TBLFASTERSATISHAREKET AS FSH " +
                                //     " LEFT JOIN TBLFASTERSATISBASLIK AS FSB ON FSH.BASLIKIND=FSB.BASLIKIND AND FSH.SUBEIND=FSB.SUBEIND AND FSH.KASAIND=FSB.KASAIND " +
                                //     " LEFT JOIN F0" + FirmaId + "TBLSTOKLAR AS STK ON FSH.STOKIND=STK.IND WHERE FSH.ISLEMTARIHI>=@par1 AND FSH.ISLEMTARIHI<=@par2 AND FIRMAIND= @FirmaInd  AND ISNULL(FSB.IADE,0)=0 " +
                                //     " GROUP BY FSB.SUBEIND,FSB.KASAIND,STK.MALINCINSI)";

                                Query =
                                               "DECLARE @Sube nvarchar(100) = '{SUBEADI}';" +
                                               "DECLARE @par1 nvarchar(20) = '{TARIH1}';" +
                                               "DECLARE @par2 nvarchar(20) = '{TARIH2}';" +
                                               "SELECT " +
                                               "T.Sube1," +
                                               "T.Kasa," +
                                               "T.Id," +
                                               " SUM(T.MIKTAR) MIKTAR," +
                                               " SUM(T.TUTAR) TUTAR," +
                                               " T.ProductName " +
                                               " FROM (" +
                                               "  (SELECT" +
                                               "     (SELECT SUBEADI" +
                                               "      FROM " + FirmaId_SUBE + "" +
                                               "      WHERE IND=FSB.SUBEIND) AS Sube1," +
                                               "     (SELECT KASAADI" +
                                               "      FROM  " + FirmaId_KASA + " " +
                                               "      WHERE IND=FSB.KASAIND) AS Kasa," +
                                               "     (SELECT IND" +
                                               "      FROM  " + FirmaId_KASA + "" +
                                               "      WHERE IND=FSB.KASAIND) AS Id," +
                                               "          SUM(FSH.MIKTAR) AS MIKTAR," +
                                               "          SUM((((MIKTAR*SATISFIYATI) * (100-ISNULL(FSH.ISK1, 0))/100) * (100-ISNULL(FSH.ISK2, 0))/100) * (100-ISNULL(FSB.ALTISKORAN, 0))/100) AS TUTAR," +
                                               "          STK.MALINCINSI AS ProductName" +
                                               "   FROM TBLFASTERSATISHAREKET AS FSH" +
                                               "   LEFT JOIN TBLFASTERSATISBASLIK AS FSB ON FSH.BASLIKIND=FSB.BASLIKIND  and  FSH.BASBARCODE = FSB.BARCODE" +
                                               "   AND FSH.SUBEIND=FSB.SUBEIND" +
                                               "   AND FSH.KASAIND=FSB.KASAIND" +
                                               "    AND FSH.BASBARCODE = FSB.BARCODE " +
                                               "   LEFT JOIN F0" + FirmaId + "TBLSTOKLAR AS STK ON FSH.STOKIND=STK.IND" +
                                               "   WHERE FSH.ISLEMTARIHI>=@par1" +
                                               "     AND FSH.ISLEMTARIHI<=@par2" +
                                               "     AND ISNULL(FSB.IADE, 0)=0" +
                                               "   GROUP BY FSB.SUBEIND," +
                                               "            FSB.KASAIND," +
                                               "            STK.MALINCINSI" +
                                               "			" +
                                               "			UNION ALL SELECT" +
                                               "     (SELECT SUBEADI" +
                                               "      FROM " + FirmaId_SUBE + "" +
                                               "      WHERE IND=FSB.SUBEIND) AS Sube1," +
                                               "     (SELECT KASAADI" +
                                               "      FROM  " + FirmaId_KASA + "" +
                                               "      WHERE IND=FSB.KASAIND) AS Kasa," +
                                               "     (SELECT IND" +
                                               "      FROM  " + FirmaId_KASA + " " +
                                               "      WHERE IND=FSB.KASAIND) AS Id," +
                                               "          SUM(FSH.MIKTAR)*-1.00 AS MIKTAR," +
                                               "          SUM((((MIKTAR*SATISFIYATI) * (100-ISNULL(FSH.ISK1, 0))/100) * (100-ISNULL(FSH.ISK2, 0))/100) * (100-ISNULL(FSB.ALTISKORAN, 0))/100) *-1.00 AS TUTAR," +
                                               "          STK.MALINCINSI AS ProductName" +
                                               "   FROM TBLFASTERSATISHAREKET AS FSH" +
                                               "   LEFT JOIN TBLFASTERSATISBASLIK AS FSB ON FSH.BASLIKIND=FSB.BASLIKIND  and  FSH.BASBARCODE = FSB.BARCODE" +
                                               "   AND FSH.SUBEIND=FSB.SUBEIND" +
                                               "   AND FSH.KASAIND=FSB.KASAIND" +
                                               "    AND FSH.BASBARCODE = FSB.BARCODE " +
                                               "   LEFT JOIN F0" + FirmaId + "TBLSTOKLAR AS STK ON FSH.STOKIND=STK.IND" +
                                               "   WHERE FSH.ISLEMTARIHI>=@par1" +
                                               "     AND FSH.ISLEMTARIHI<=@par2" +
                                               "     AND ISNULL(FSB.IADE, 0)=1" +
                                               "   GROUP BY FSB.SUBEIND," +
                                               "            FSB.KASAIND," +
                                               "            STK.MALINCINSI" +
                                               "			" +
                                               "			) ) T " +
                                               "			GROUP BY " +
                                               "			T.Sube1," +
                                               " T.Kasa," +
                                               "T.Id," +
                                               "T.ProductName";
                                #endregion FASTER ONLINE QUARY
                            }
                            else
                            {

                                //Query =
                                //   "DECLARE @Sube nvarchar(100) = '{SUBEADI}';" +
                                //   "DECLARE @par1 nvarchar(20) = '{TARIH1}';" +
                                //   "DECLARE @par2 nvarchar(20) = '{TARIH2}';" +
                                //   "SELECT " +
                                //   "T.Sube1," +
                                //   "T.Kasa," +
                                //   "T.Id," +
                                //   " SUM(T.MIKTAR) MIKTAR," +
                                //   " SUM(T.TUTAR) TUTAR, " +
                                //   " T.ProductName " +
                                //   " FROM (" +
                                //   "  (SELECT" +
                                //   "     (SELECT SUBEADI" +
                                //   "       FROM " + FirmaId_SUBE + " " +
                                //   "      WHERE IND=FSB.SUBEIND) AS Sube1," +
                                //   "     (SELECT KASAADI" +
                                //   "     FROM " + FirmaId_KASA + "" +
                                //   "      WHERE IND=FSB.KASAIND) AS Kasa," +
                                //   "     (SELECT IND" +
                                //   "     FROM " + FirmaId_KASA + "" +
                                //   "      WHERE IND=FSB.KASAIND) AS Id," +
                                //   "          SUM(FSH.MIKTAR) AS MIKTAR," +
                                //   "          SUM((((MIKTAR*SATISFIYATI) * (100-ISNULL(FSH.ISK1, 0))/100) * (100-ISNULL(FSH.ISK2, 0))/100) * (100-ISNULL(FSB.ALTISKORAN, 0))/100) AS TUTAR," +
                                //   "          STK.MALINCINSI AS ProductName" +
                                //   "   FROM TBLFASTERSATISHAREKET AS FSH" +
                                //   "   LEFT JOIN TBLFASTERSATISBASLIK AS FSB ON FSH.BASLIKIND=FSB.BASLIKIND" +
                                //   "   AND FSH.SUBEIND=FSB.SUBEIND" +
                                //   "   AND FSH.KASAIND=FSB.KASAIND" +
                                //   "    AND FSH.BASBARCODE = FSB.BARCODE " +
                                //   "   LEFT JOIN TBLFASTERSTOKLAR AS STK ON FSH.STOKIND=STK.STOKNO" +
                                //   "   WHERE FSH.ISLEMTARIHI>=@par1" +
                                //   "     AND FSH.ISLEMTARIHI<=@par2" +
                                //   "     AND ISNULL(FSB.IADE, 0)=0" +
                                //   "   GROUP BY FSB.SUBEIND," +
                                //   "            FSB.KASAIND," +
                                //   "            STK.MALINCINSI" +
                                //   "			" +
                                //   "			UNION ALL SELECT" +
                                //   "     (SELECT SUBEADI" +
                                //   "     FROM " + FirmaId_SUBE + " " +
                                //   "      WHERE IND=FSB.SUBEIND) AS Sube1," +
                                //   "     (SELECT KASAADI" +
                                //   "      FROM " + FirmaId_KASA + "" +
                                //   "      WHERE IND=FSB.KASAIND) AS Kasa," +
                                //   "     (SELECT IND" +
                                //   "      FROM " + FirmaId_KASA + "" +
                                //   "      WHERE IND=FSB.KASAIND) AS Id," +
                                //   "          SUM(FSH.MIKTAR)*-1.00 AS MIKTAR," +
                                //   "          SUM((((MIKTAR*SATISFIYATI) * (100-ISNULL(FSH.ISK1, 0))/100) * (100-ISNULL(FSH.ISK2, 0))/100) * (100-ISNULL(FSB.ALTISKORAN, 0))/100) *-1.00 AS TUTAR," +
                                //   "          STK.MALINCINSI AS ProductName" +
                                //   "   FROM TBLFASTERSATISHAREKET AS FSH" +
                                //   "   LEFT JOIN TBLFASTERSATISBASLIK AS FSB ON FSH.BASLIKIND=FSB.BASLIKIND" +
                                //   "   AND FSH.SUBEIND=FSB.SUBEIND" +
                                //   "   AND FSH.KASAIND=FSB.KASAIND" +
                                //   "    AND FSH.BASBARCODE = FSB.BARCODE " +
                                //   "   LEFT JOIN TBLFASTERSTOKLAR AS STK ON FSH.STOKIND=STK.STOKNO" +
                                //   "   WHERE FSH.ISLEMTARIHI>=@par1" +
                                //   "     AND FSH.ISLEMTARIHI<=@par2" +
                                //   "     AND ISNULL(FSB.IADE, 0)=1" +
                                //   "   GROUP BY FSB.SUBEIND," +
                                //   "            FSB.KASAIND," +
                                //   "            STK.MALINCINSI" +
                                //   "			" +
                                //   "			) ) T" +
                                //   "			GROUP BY " +
                                //   "			T.Sube1," +
                                //   "T.Kasa," +
                                //   "T.Id," +
                                //   "T.ProductName";

                                Query =
                                " DECLARE @Sube nvarchar(100) = '{SUBEADI}';" +
                                " DECLARE @par1 nvarchar(20) = '{TARIH1}';" +
                                " DECLARE @par2 nvarchar(20) = '{TARIH2}';" +
                                " SELECT " +
                                " T.Sube1," +
                                " T.Kasa," +
                                " T.ID," +
                                " T.MALINCINSI ProductName," +
                                " SUM(T.MIKTAR) MIKTAR," +
                                " SUM(T.TUTAR) TUTAR" +
                                " FROM (" +
                                "  (SELECT" +
                                "     (SELECT SUBEADI" +
                                "       FROM " + FirmaId_SUBE + " " +
                                "      WHERE IND=FSB.SUBEIND) AS Sube1," +
                                "     (SELECT KASAADI" +
                                "     FROM " + FirmaId_KASA + "" +
                                "      WHERE IND=FSB.KASAIND) AS Kasa," +
                                "     (SELECT IND" +
                                "     FROM " + FirmaId_KASA + "" +
                                "      WHERE IND=FSB.KASAIND) AS ID," +
                                "          SUM(FSH.MIKTAR) AS MIKTAR," +
                                "          SUM((((MIKTAR*SATISFIYATI) * (100-ISNULL(FSH.ISK1, 0))/100) * (100-ISNULL(FSH.ISK2, 0))/100) * (100-ISNULL(FSB.ALTISKORAN, 0))/100) AS TUTAR" +
                                "		    ,STK.MALINCINSI" +
                                "   FROM TBLFASTERSATISHAREKET AS FSH" +
                                "   LEFT JOIN TBLFASTERSATISBASLIK AS FSB ON FSH.BASLIKIND=FSB.BASLIKIND" +
                                "   AND FSH.SUBEIND=FSB.SUBEIND" +
                                "   AND FSH.KASAIND=FSB.KASAIND" +
                                "   LEFT JOIN TBLFASTERSTOKLAR AS STK ON FSH.STOKIND=STK.STOKNO" +
                                "   WHERE FSH.ISLEMTARIHI>=@par1" +
                                "     AND FSH.ISLEMTARIHI<=@par2" +
                                "     AND ISNULL(FSB.IADE, 0)=0" +
                                "   GROUP BY FSB.SUBEIND," +
                                "            FSB.KASAIND  ,STK.MALINCINSI" +
                                "			" +
                                "	UNION ALL" +
                                "	SELECT" +
                                "     (SELECT SUBEADI" +
                                "       FROM " + FirmaId_SUBE + " " +
                                "      WHERE IND=FSB.SUBEIND) AS Sube1," +
                                "     (SELECT KASAADI" +
                                "     FROM " + FirmaId_KASA + "" +
                                "      WHERE IND=FSB.KASAIND) AS Kasa," +
                                "     (SELECT IND" +
                                "     FROM " + FirmaId_KASA + "" +
                                "      WHERE IND=FSB.KASAIND) AS ID," +
                                "          SUM(FSH.MIKTAR)*-1 AS MIKTAR," +
                                "          SUM((((MIKTAR*SATISFIYATI) * (100-ISNULL(FSH.ISK1, 0))/100) * (100-ISNULL(FSH.ISK2, 0))/100) * (100-ISNULL(FSB.ALTISKORAN, 0))/100) *-1.00 AS TUTAR" +
                                "		    ,STK.MALINCINSI" +
                                "   FROM TBLFASTERSATISHAREKET AS FSH" +
                                "   LEFT JOIN TBLFASTERSATISBASLIK AS FSB ON FSH.BASLIKIND=FSB.BASLIKIND" +
                                "   AND FSH.SUBEIND=FSB.SUBEIND" +
                                "   AND FSH.KASAIND=FSB.KASAIND" +
                                "   LEFT JOIN TBLFASTERSTOKLAR AS STK ON FSH.STOKIND=STK.STOKNO" +
                                "   WHERE FSH.ISLEMTARIHI>=@par1" +
                                "     AND FSH.ISLEMTARIHI<=@par2" +
                                "     AND ISNULL(FSB.IADE, 0)=1" +
                                "   GROUP BY FSB.SUBEIND," +
                                "            FSB.KASAIND  ,STK.MALINCINSI" +
                                "			" +
                                "			) ) T" +
                                "			GROUP BY " +
                                "			T.Sube1," +
                                " T.Kasa," +
                                " T.ID," +
                                " T.MALINCINSI ";




                                //Query = File.ReadAllText(HostingEnvironment.MapPath("/Sql/SqlFaster/SubeUrun/SubeUrunFasterOFLINE.sql"), System.Text.UTF8Encoding.Default);
                            }
                        }
                        else if (AppDbType == "4")
                        {
                            #region NPOS QUARY (En Çok Satılan Ürün)
                            Query =
                                 " declare @Sube nvarchar(100) = '{SUBEADI}';" +
                                 " declare @Trh1 nvarchar(20) = '{TARIH1}';" +
                                 " declare @Trh2 nvarchar(20) = '{TARIH2}';" +
                                 " declare @ProductName nvarchar(20) = '{ProductName}';" +
                                 " SELECT TOP 6 WITH TIES Kasa_no,  " +
                                 " Sube,Sube1,Kasa_No AS Kasa, KOD1 AS ProductGroup ,MALINCINSI as ProductName ,Sum(Tutar) as TUTAR,Sum(Miktar) as MIKTAR " +
                                 " FROM " + vega_Db + "..TBLMASTERENPOSSTOK AS HR " +
                                 " WHERE HR.BELGETARIH >= @Trh1 and HR.BELGETARIH <= @Trh2 " +
                                 " group by " +
                                 " MALINCINSI," +
                                 " Sube,Sube1,Kasa_No,kod1  order by TUTAR desc ";
                            #endregion NPOS QUARY (En Çok Satılan Ürün)
                        }
                        else if (AppDbType == "5")
                        {
                            Query = File.ReadAllText(HostingEnvironment.MapPath("/Sql/VPos/DashboardSubeToplamCiro/EncokSatisYapanUrunVpos.sql"), System.Text.UTF8Encoding.Default);
                        }
                    }

                    Query = Query.Replace("{SUBEADI}", SubeAdi);
                    Query = Query.Replace("{TARIH1}", QueryTimeStart);
                    Query = Query.Replace("{TARIH2}", QueryTimeEnd);
                    Query = Query.Replace("{FIRMAIND}", FirmaId);
                    Query = Query.Replace("{KASALAR}", FirmaId_KASA);
                    Query = Query.Replace("{SUBELER}", FirmaId_SUBE);
                    Query = Query.Replace("{SUBE2}", vPosSubeKodu);
                    Query = Query.Replace("{KASAKODU}", vPosKasaKodu);

                    if (ID == "1")
                    {
                        #region GET DATA   

                        try
                        {
                            try
                            {
                                DataTable SubeUrunCiroDt = new DataTable();
                                SubeUrunCiroDt = f.GetSubeDataWithQuery(f.NewConnectionString(SubeIP, DBName, SqlName, SqlPassword), Query.ToString());

                                if (SubeUrunCiroDt.Rows.Count > 0)
                                {
                                    if (subeid.Equals("") && subeid != "exportExcel")
                                    {
                                        #region Subeid '' Subelere göre toplamları göst.

                                        if (AppDbType == "3")
                                        {
                                            #region FASTER -(AppDbType = 3 faster kullanan şube)      

                                            foreach (DataRow sube in SubeUrunCiroDt.Rows)
                                            {
                                                SubeUrun items = new SubeUrun
                                                {
                                                    SubeID = SubeId + "~" + sube["Id"].ToString(),
                                                    Sube = SubeAdi + "-" + sube["Sube1"].ToString() + "-" + sube["Kasa"].ToString()
                                                };
                                                items.Miktar += Convert.ToDecimal(sube["MIKTAR"]);//, "MIKTAR");
                                                items.Debit += Convert.ToDecimal(sube["TUTAR"]); //, "MIKTAR");f.RTD(SubeR, "TUTAR");
                                                items.ProductName = f.RTS(sube, "ProductName"); //sube["ProductName"].ToString();  //f.RTS(SubeR, "ProductName");
                                                lock (locked)
                                                {
                                                    Liste.Add(items);
                                                }
                                            }

                                            #endregion FASTER -(AppDbType = 3 faster kullanan şube)
                                        }
                                        else if (AppDbType == "4")
                                        {
                                            #region NPOS                                           
                                            foreach (DataRow sube in SubeUrunCiroDt.Rows)
                                            {
                                                SubeUrun items = new SubeUrun
                                                {
                                                    SubeID = SubeId + "~" + sube["Kasa"].ToString(),
                                                    Sube = SubeAdi + "_" + sube["Sube"].ToString() + "-" + sube["Sube1"].ToString() + "-" + sube["Kasa"].ToString()
                                                };
                                                items.Miktar += Convert.ToDecimal(sube["MIKTAR"]);//, "MIKTAR");
                                                items.Debit += Convert.ToDecimal(sube["TUTAR"]); //, "MIKTAR");f.RTD(SubeR, "TUTAR");
                                                items.ProductName = f.RTS(sube, "ProductName"); //sube["ProductName"].ToString();  //f.RTS(SubeR, "ProductName");
                                                lock (locked)
                                                {
                                                    Liste.Add(items);
                                                }
                                            }
                                            #endregion NPOS
                                        }
                                        else
                                        {
                                            #region SEFIM   

                                            SubeUrun items = new SubeUrun
                                            {
                                                Sube = SubeAdi, //f.RTS(SubeUrunCiroDt.Rows[0], "Sube");
                                                SubeID = (SubeId)
                                            };
                                            if (subeid.Equals(""))
                                            {
                                                foreach (DataRow SubeR in SubeUrunCiroDt.Rows)
                                                {
                                                    items.Miktar += f.RTD(SubeR, "MIKTAR");
                                                    items.Debit += f.RTD(SubeR, "TUTAR");
                                                    items.ProductName = f.RTS(SubeR, "ProductName");
                                                    items.Ikram += f.RTD(SubeR, "IKRAM");
                                                    items.Indirim += f.RTD(SubeR, "INDIRIM");
                                                }

                                                items.NetTutar = items.Debit - (items.Indirim/* + items.Ikram //08/04/23*/);
                                            }
                                            lock (locked)
                                            {
                                                Liste.Add(items);
                                            }
                                            #endregion SEFIM
                                        }
                                        #endregion Subeid '' Subelere göre toplamları göst.
                                    }
                                    else
                                    {
                                        #region Subeid ile detay alacak ise

                                        if (AppDbType == "3")
                                        {
                                            #region FASTER                                       
                                            foreach (DataRow SubeR in SubeUrunCiroDt.Rows)
                                            {
                                                if (subeid == SubeR["Id"].ToString())
                                                {
                                                    SubeUrun items = new SubeUrun
                                                    {
                                                        Sube = f.RTS(SubeR, "Sube"),
                                                        SubeID = (SubeId),
                                                        Miktar = f.RTD(SubeR, "MIKTAR"),
                                                        ProductName = f.RTS(SubeR, "ProductName"),
                                                        Debit = f.RTD(SubeR, "TUTAR")
                                                    };
                                                    //items.GecenZaman = Convert.ToInt32(sure.TotalSeconds);
                                                    lock (locked)
                                                    {
                                                        Liste.Add(items);
                                                    }
                                                }
                                            }
                                            #endregion FASTER
                                        }
                                        else if (AppDbType == "4")
                                        {
                                            #region NPOS                                            
                                            foreach (DataRow SubeR in SubeUrunCiroDt.Rows)
                                            {
                                                if (subeid == SubeR["Kasa"].ToString())
                                                {
                                                    SubeUrun items = new SubeUrun();
                                                    items.Sube = f.RTS(SubeR, "Sube");
                                                    items.SubeID = (SubeId);
                                                    items.Miktar = f.RTD(SubeR, "MIKTAR");
                                                    items.ProductName = f.RTS(SubeR, "ProductName");
                                                    items.Debit = f.RTD(SubeR, "TUTAR");
                                                    //items.GecenZaman = Convert.ToInt32(sure.TotalSeconds);
                                                    lock (locked)
                                                    {
                                                        Liste.Add(items);
                                                    }
                                                }
                                            }
                                            #endregion NPOS
                                        }
                                        else if (TopluExcel)
                                        {
                                            #region SEFIM 

                                            foreach (DataRow SubeR in SubeUrunCiroDt.Rows)
                                            {
                                                var items = new SubeUrun
                                                {
                                                    Sube = SubeAdi,
                                                    SubeID = (SubeId),
                                                    Miktar = f.RTD(SubeR, "MIKTAR"),
                                                    ProductName = f.RTS(SubeR, "ProductName"),
                                                    Debit = f.RTD(SubeR, "TUTAR"),
                                                    TarihGun = f.RTS(SubeR, "Tarih"),
                                                    TarihSaat = f.RTS(SubeR, "Saat") == null ? "-" : f.RTS(SubeR, "Saat"),
                                                    Ikram = f.RTD(SubeR, "IKRAM"),
                                                    Indirim = f.RTD(SubeR, "INDIRIM")
                                                };
                                                items.NetTutar = items.Debit - (items.Indirim /*+ items.Ikram*/);
                                                lock (locked)
                                                {
                                                    Liste.Add(items);
                                                }
                                            }

                                            #endregion SEFIM
                                        }
                                        else
                                        {
                                            #region SEFIM 

                                            foreach (DataRow SubeR in SubeUrunCiroDt.Rows)
                                            {
                                                SubeUrun items = new SubeUrun();
                                                items.Sube = f.RTS(SubeR, "Sube");
                                                items.SubeID = (SubeId);
                                                items.Miktar = f.RTD(SubeR, "MIKTAR");
                                                items.ProductName = f.RTS(SubeR, "ProductName");
                                                items.Debit = f.RTD(SubeR, "TUTAR");
                                                items.TarihGun = f.RTS(SubeR, "Tarih");
                                                items.TarihSaat = f.RTS(SubeR, "Saat") == null ? "-" : f.RTS(SubeR, "Saat");
                                                items.Ikram = f.RTD(SubeR, "IKRAM");
                                                items.Indirim = f.RTD(SubeR, "INDIRIM");
                                                items.NetTutar = items.Debit - (items.Indirim /*+ items.Ikram*/);
                                                lock (locked)
                                                {
                                                    Liste.Add(items);
                                                }
                                            }

                                            #endregion SEFIM
                                        }

                                        #endregion Subeid ile detay alacak ise
                                    }
                                }
                                else
                                {
                                    SubeUrun items = new SubeUrun
                                    {
                                        Sube = SubeAdi + " (Data Yok)",
                                        SubeID = (SubeId),
                                        TarihGun = "-",
                                        TarihSaat = "-"
                                    };
                                    lock (locked)
                                    {
                                        Liste.Add(items);
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                try
                                {
                                    #region EX                            

                                    Singleton.WritingLogFile2("SubeUrunCRUD1", ex.ToString(), null, ex.StackTrace);
                                    SubeUrun items = new SubeUrun
                                    {
                                        Sube = ex.Message + " (Erişim Yok)",
                                        SubeID = (SubeId),
                                        ErrorMessage = "Şube/Ürün Raporu Alınamadı.",
                                        ErrorStatus = true,
                                        ErrorCode = "01"
                                    };
                                    Liste.Add(items);

                                    #endregion
                                }
                                catch (Exception) { }
                            }
                        }
                        catch (Exception ex)
                        {
                            try
                            {
                                #region EX                            

                                Singleton.WritingLogFile2("SubeUrunCRUD", ex.ToString(), null, ex.StackTrace);
                                SubeUrun items = new SubeUrun
                                {
                                    Sube = ex.Message + " (Erişim Yok)",
                                    SubeID = (SubeId),
                                    ErrorMessage = "Şube/Ürün Raporu Alınamadı.",
                                    ErrorStatus = true,
                                    ErrorCode = "01"
                                };
                                Liste.Add(items);

                                #endregion
                            }
                            catch (Exception) { }
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
                                    try
                                    {
                                        DataTable SubeUrunCiroDt = new DataTable();
                                        SubeUrunCiroDt = f.GetSubeDataWithQuery(f.NewConnectionString(SubeIP, DBName, SqlName, SqlPassword), Query.ToString());

                                        if (SubeUrunCiroDt.Rows.Count > 0)
                                        {
                                            if (subeid.Equals("") && subeid != "exportExcel")
                                            {
                                                #region Subeid '' Subelere göre toplamları göst.

                                                if (AppDbType == "3")
                                                {
                                                    #region FASTER -(AppDbType = 3 faster kullanan şube)      

                                                    foreach (DataRow sube in SubeUrunCiroDt.Rows)
                                                    {
                                                        SubeUrun items = new SubeUrun
                                                        {
                                                            SubeID = SubeId + "~" + sube["Id"].ToString(),
                                                            Sube = SubeAdi + "-" + sube["Sube1"].ToString() + "-" + sube["Kasa"].ToString()
                                                        };
                                                        items.Miktar += Convert.ToDecimal(sube["MIKTAR"]);//, "MIKTAR");
                                                        items.Debit += Convert.ToDecimal(sube["TUTAR"]); //, "MIKTAR");f.RTD(SubeR, "TUTAR");
                                                        items.ProductName = f.RTS(sube, "ProductName"); //sube["ProductName"].ToString();  //f.RTS(SubeR, "ProductName");
                                                        lock (locked)
                                                        {
                                                            Liste.Add(items);
                                                        }
                                                    }

                                                    #endregion FASTER -(AppDbType = 3 faster kullanan şube)
                                                }
                                                else if (AppDbType == "4")
                                                {
                                                    #region NPOS                                           
                                                    foreach (DataRow sube in SubeUrunCiroDt.Rows)
                                                    {
                                                        SubeUrun items = new SubeUrun
                                                        {
                                                            SubeID = SubeId + "~" + sube["Kasa"].ToString(),
                                                            Sube = SubeAdi + "_" + sube["Sube"].ToString() + "-" + sube["Sube1"].ToString() + "-" + sube["Kasa"].ToString()
                                                        };
                                                        items.Miktar += Convert.ToDecimal(sube["MIKTAR"]);//, "MIKTAR");
                                                        items.Debit += Convert.ToDecimal(sube["TUTAR"]); //, "MIKTAR");f.RTD(SubeR, "TUTAR");
                                                        items.ProductName = f.RTS(sube, "ProductName"); //sube["ProductName"].ToString();  //f.RTS(SubeR, "ProductName");
                                                        lock (locked)
                                                        {
                                                            Liste.Add(items);
                                                        }
                                                    }
                                                    #endregion NPOS
                                                }
                                                else
                                                {
                                                    #region SEFIM   

                                                    SubeUrun items = new SubeUrun
                                                    {
                                                        Sube = SubeAdi, //f.RTS(SubeUrunCiroDt.Rows[0], "Sube");
                                                        SubeID = (SubeId)
                                                    };
                                                    if (subeid.Equals(""))
                                                    {
                                                        foreach (DataRow SubeR in SubeUrunCiroDt.Rows)
                                                        {
                                                            items.Miktar += f.RTD(SubeR, "MIKTAR");
                                                            items.Debit += f.RTD(SubeR, "TUTAR");
                                                            items.ProductName = f.RTS(SubeR, "ProductName");
                                                            items.Ikram += f.RTD(SubeR, "IKRAM");
                                                            items.Indirim += f.RTD(SubeR, "INDIRIM");
                                                        }

                                                        items.NetTutar = items.Debit - (items.Indirim/* + items.Ikram //08/04/23*/);
                                                    }
                                                    lock (locked)
                                                    {
                                                        Liste.Add(items);
                                                    }
                                                    #endregion SEFIM
                                                }
                                                #endregion Subeid '' Subelere göre toplamları göst.
                                            }
                                            else
                                            {
                                                #region Subeid ile detay alacak ise

                                                if (AppDbType == "3")
                                                {
                                                    #region FASTER                                       
                                                    foreach (DataRow SubeR in SubeUrunCiroDt.Rows)
                                                    {
                                                        if (subeid == SubeR["Id"].ToString())
                                                        {
                                                            SubeUrun items = new SubeUrun
                                                            {
                                                                Sube = f.RTS(SubeR, "Sube"),
                                                                SubeID = (SubeId),
                                                                Miktar = f.RTD(SubeR, "MIKTAR"),
                                                                ProductName = f.RTS(SubeR, "ProductName"),
                                                                Debit = f.RTD(SubeR, "TUTAR")
                                                            };
                                                            //items.GecenZaman = Convert.ToInt32(sure.TotalSeconds);
                                                            lock (locked)
                                                            {
                                                                Liste.Add(items);
                                                            }
                                                        }
                                                    }
                                                    #endregion FASTER
                                                }
                                                else if (AppDbType == "4")
                                                {
                                                    #region NPOS                                            
                                                    foreach (DataRow SubeR in SubeUrunCiroDt.Rows)
                                                    {
                                                        if (subeid == SubeR["Kasa"].ToString())
                                                        {
                                                            SubeUrun items = new SubeUrun();
                                                            items.Sube = f.RTS(SubeR, "Sube");
                                                            items.SubeID = (SubeId);
                                                            items.Miktar = f.RTD(SubeR, "MIKTAR");
                                                            items.ProductName = f.RTS(SubeR, "ProductName");
                                                            items.Debit = f.RTD(SubeR, "TUTAR");
                                                            //items.GecenZaman = Convert.ToInt32(sure.TotalSeconds);
                                                            lock (locked)
                                                            {
                                                                Liste.Add(items);
                                                            }
                                                        }
                                                    }
                                                    #endregion NPOS
                                                }
                                                else if (TopluExcel)
                                                {
                                                    #region SEFIM 

                                                    foreach (DataRow SubeR in SubeUrunCiroDt.Rows)
                                                    {
                                                        var items = new SubeUrun
                                                        {
                                                            Sube = SubeAdi,
                                                            SubeID = (SubeId),
                                                            Miktar = f.RTD(SubeR, "MIKTAR"),
                                                            ProductName = f.RTS(SubeR, "ProductName"),
                                                            Debit = f.RTD(SubeR, "TUTAR"),
                                                            TarihGun = f.RTS(SubeR, "Tarih"),
                                                            TarihSaat = f.RTS(SubeR, "Saat") == null ? "-" : f.RTS(SubeR, "Saat"),
                                                            Ikram = f.RTD(SubeR, "IKRAM"),
                                                            Indirim = f.RTD(SubeR, "INDIRIM")
                                                        };
                                                        items.NetTutar = items.Debit - (items.Indirim /*+ items.Ikram*/);
                                                        lock (locked)
                                                        {
                                                            Liste.Add(items);
                                                        }
                                                    }

                                                    #endregion SEFIM
                                                }
                                                else
                                                {
                                                    #region SEFIM 

                                                    foreach (DataRow SubeR in SubeUrunCiroDt.Rows)
                                                    {
                                                        SubeUrun items = new SubeUrun();
                                                        items.Sube = f.RTS(SubeR, "Sube");
                                                        items.SubeID = (SubeId);
                                                        items.Miktar = f.RTD(SubeR, "MIKTAR");
                                                        items.ProductName = f.RTS(SubeR, "ProductName");
                                                        items.Debit = f.RTD(SubeR, "TUTAR");
                                                        items.TarihGun = f.RTS(SubeR, "Tarih");
                                                        items.TarihSaat = f.RTS(SubeR, "Saat") == null ? "-" : f.RTS(SubeR, "Saat");
                                                        items.Ikram = f.RTD(SubeR, "IKRAM");
                                                        items.Indirim = f.RTD(SubeR, "INDIRIM");
                                                        items.NetTutar = items.Debit - (items.Indirim /*+ items.Ikram*/);
                                                        lock (locked)
                                                        {
                                                            Liste.Add(items);
                                                        }
                                                    }

                                                    #endregion SEFIM
                                                }

                                                #endregion Subeid ile detay alacak ise
                                            }
                                        }
                                        else
                                        {
                                            SubeUrun items = new SubeUrun
                                            {
                                                Sube = SubeAdi + " (Data Yok)",
                                                SubeID = (SubeId),
                                                TarihGun = "-",
                                                TarihSaat = "-"
                                            };
                                            lock (locked)
                                            {
                                                Liste.Add(items);
                                            }
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        try
                                        {
                                            #region EX                            

                                            Singleton.WritingLogFile2("SubeUrunCRUD1", ex.ToString(), null, ex.StackTrace);
                                            SubeUrun items = new SubeUrun
                                            {
                                                Sube = ex.Message + " (Erişim Yok)",
                                                SubeID = (SubeId),
                                                ErrorMessage = "Şube/Ürün Raporu Alınamadı.",
                                                ErrorStatus = true,
                                                ErrorCode = "01"
                                            };
                                            Liste.Add(items);

                                            #endregion
                                        }
                                        catch (Exception) { }
                                    }
                                }
                                catch (Exception ex)
                                {
                                    try
                                    {
                                        #region EX                            

                                        Singleton.WritingLogFile2("SubeUrunCRUD", ex.ToString(), null, ex.StackTrace);
                                        SubeUrun items = new SubeUrun
                                        {
                                            Sube = ex.Message + " (Erişim Yok)",
                                            SubeID = (SubeId),
                                            ErrorMessage = "Şube/Ürün Raporu Alınamadı.",
                                            ErrorStatus = true,
                                            ErrorCode = "01"
                                        };
                                        Liste.Add(items);

                                        #endregion
                                    }
                                    catch (Exception) { }
                                }

                                #endregion GET DATA 
                            }
                        }

                        #endregion KULLANICININ ŞUBE YETKİ KONTROLU YAPILIYOR  
                    }
                });

                #endregion PARALLEL FORECH

            }
            catch (DataException ex) { Singleton.WritingLogFile2("SubeUrunCRUD", ex.ToString(), null, ex.StackTrace); }

            return Liste;
        }
    }
}