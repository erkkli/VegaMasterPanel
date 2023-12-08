﻿using SefimV2.Helper;
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
    public class PersonelYemekRaporuCRUD
    {
        public static List<UrunGrubu> List(DateTime Date1, DateTime Date2, string subeid, string productGroup, string ID)
        {
            if (productGroup == "alt=\"expand/collapse\"")
            {
                productGroup = "NULL";
            }

            var Liste = new List<UrunGrubu>();
            var ff = new ModelFunctions();
            #region GİRİŞ YAPAN KULLANICININ YETKİLİ OLDUĞU ŞUBELER LİSTELENİYOR
            UserViewModel model = UsersListCRUD.YetkiliSubesi(ID);
            #endregion

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

            string subeid_ = string.Empty;
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
                    #region SQL QUARY  *(Sefim || (faster || Faster Offline || Faster Online))*

                    ModelFunctions f = new ModelFunctions();
                    string Query = string.Empty;
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
                    string personelYemekRaporuAdi = r["PersonelYemekRaporuAdi"].ToString();
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

                    #region Faster Kasalar seçildiyse ona göre query oluşturuluyor. 

                    string FasterSubeIND = f.RTS(r, "FasterSubeID");
                    string QueryFasterSube = string.Empty;
                    if (FasterSubeIND != null)
                    {
                        QueryFasterSube = "  and  FSH.SUBEIND IN(" + FasterSubeIND + ") ";
                    }

                    #endregion Faster Kasalar seçildiyse ona göre query oluşturuluyor.

                    if (AppDbType == "1" || AppDbType == "2")
                    {
                        if (subeid != null && !subeid.Equals("0") && productGroup == null)// sube secili degilse ilk giris yapilan sql
                        {
                            if (string.IsNullOrEmpty(urunEslestirmeVarMi) || urunEslestirmeVarMi == "False")
                            {
                                Query = File.ReadAllText(HostingEnvironment.MapPath("/Sql/UrunGrupUrunKategori.sql"), System.Text.UTF8Encoding.Default);
                            }
                            else
                            {
                                Query = File.ReadAllText(HostingEnvironment.MapPath("/Sql/SqlNewSef/PersonelYemekRaporu/PersonelYemekRaporu.sql"), System.Text.UTF8Encoding.Default);
                            }
                        }
                        if (subeid == null || subeid.Equals("0"))
                        {
                            Query = File.ReadAllText(HostingEnvironment.MapPath("/Sql/UrunGrup.sql"), System.Text.UTF8Encoding.Default);
                        }
                        if (subeid != null && !subeid.Equals("0") && productGroup != null)
                        {
                            if (string.IsNullOrEmpty(urunEslestirmeVarMi) || urunEslestirmeVarMi == "False")
                            {
                                Query = File.ReadAllText(HostingEnvironment.MapPath("/Sql/UrunGrupDetay.sql"), System.Text.UTF8Encoding.Default);
                            }
                            else
                            {
                                Query = File.ReadAllText(HostingEnvironment.MapPath("/Sql/SqlNewSef/PersonelYemekRaporu/PersonelYemekRaporu.sql"), System.Text.UTF8Encoding.Default);
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
                            if (productGroup == null)
                            {
                                productGroup = "False";
                            }
                            #endregion
                        }
                        #endregion #region Kırılımlarda Ana Şube Altındaki IND id alıyorum 

                        if (AppDbTypeStatus == "True")//Faster Online
                        {
                            if (subeid != null && !subeid.Equals("0") && productGroup == null)// sube secili degilse ilk giris yapilan sql                           
                                #region FASTER ONLINE QUARY 1.liste

                                Query =
                                                "DECLARE @Sube nvarchar(100) = '{SubeAdi}';" +
                                                "DECLARE @par1 nvarchar(20) = '{TARIH1}';" +
                                                "DECLARE @par2 nvarchar(20) = '{TARIH2}';" +
                                                " SELECT T.Sube1," +
                                                "       T.Kasa," +
                                                "       T.Id," +
                                                "       SUM(T.MIKTAR) MIKTAR," +
                                                "       SUM(T.TUTAR) TUTAR" +
                                                " FROM (" +
                                                "        (SELECT" +
                                                "           (SELECT SUBEADI" +
                                                    "      FROM " + FirmaId_SUBE + "" +
                                                "            WHERE IND=FSB.SUBEIND) AS Sube1," +
                                                "           (SELECT KASAADI" +
                                                 "      FROM " + FirmaId_KASA + "" +
                                                "            WHERE IND=FSB.KASAIND) AS Kasa," +
                                                "           (SELECT IND" +
                                                 "      FROM " + FirmaId_KASA + "" +
                                                "            WHERE IND=FSB.KASAIND) AS Id," +
                                                "                SUM(FSH.MIKTAR) AS MIKTAR," +
                                                "                SUM((((MIKTAR*SATISFIYATI) * (100-ISNULL(FSH.ISK1, 0))/100) * (100-ISNULL(FSH.ISK2, 0))/100) * (100-ISNULL(FSB.ALTISKORAN, 0))/100) AS TUTAR" +
                                                "         FROM TBLFASTERSATISHAREKET AS FSH" +
                                                "         LEFT JOIN TBLFASTERSATISBASLIK AS FSB ON FSH.BASLIKIND=FSB.BASLIKIND" +
                                                "         AND FSH.SUBEIND=FSB.SUBEIND" +
                                                "         AND FSH.KASAIND=FSB.KASAIND" +
                                                "         LEFT JOIN  F0" + FirmaId + "TBLSTOKLAR AS STK ON FSH.STOKIND=STK.IND and FSH.BIRIMIND=STK.BIRIMEX" +
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
                                                 "      FROM " + FirmaId_KASA + "" +
                                                "            WHERE IND=FSB.KASAIND) AS Kasa," +
                                                "           (SELECT IND" +
                                                 "      FROM " + FirmaId_KASA + "" +
                                                "            WHERE IND=FSB.KASAIND) AS Id," +
                                                "                          SUM(FSH.MIKTAR)*-1 AS MIKTAR," +
                                                "                          SUM((((MIKTAR*SATISFIYATI) * (100-ISNULL(FSH.ISK1, 0))/100) * (100-ISNULL(FSH.ISK2, 0))/100) * (100-ISNULL(FSB.ALTISKORAN, 0))/100) *-1.00 AS TUTAR" +
                                                "         FROM TBLFASTERSATISHAREKET AS FSH" +
                                                "         LEFT JOIN TBLFASTERSATISBASLIK AS FSB ON FSH.BASLIKIND=FSB.BASLIKIND" +
                                                "         AND FSH.SUBEIND=FSB.SUBEIND" +
                                                "         AND FSH.KASAIND=FSB.KASAIND" +
                                                "         LEFT JOIN  F0" + FirmaId + "TBLSTOKLAR AS STK ON FSH.STOKIND=STK.IND and FSH.BIRIMIND=STK.BIRIMEX" +
                                                "         WHERE FSH.ISLEMTARIHI>=@par1 " +
                                                "           AND FSH.ISLEMTARIHI<=@par2" +
                                                "           AND ISNULL(FSB.IADE, 0)=1" +
                                                 " " + QueryFasterSube +
                                                "         GROUP BY FSB.SUBEIND," +
                                                "                  FSB.KASAIND)) T " +
                                                " GROUP BY T.Sube1," +
                                                "         T.Kasa," +
                                                "         T.Id";



                            #endregion FASTER ONLINE QUARY 

                            if (subeid != null && !subeid.Equals("0") && productGroup == "False")

                                #region FASTER ONLINE QUARY 2. Liste
                                //Query =
                                //         " declare @Sube nvarchar(100) = '{SubeAdi}';" +
                                //         " declare @par1 nvarchar(20) = '{TARIH1}';" +
                                //         " declare @par2 nvarchar(20) = '{TARIH2}';" +
                                //         " DECLARE @FirmaInd nvarchar(100) = '{FIRMAIND}';" +
                                //         " ( SELECT (SELECT SUBEADI FROM " + FirmaId_SUBE + " WHERE IND=FSB.SUBEIND) AS Sube1," +
                                //         " ( SELECT KASAADI FROM " + FirmaId_KASA + " WHERE IND=FSB.KASAIND) AS Kasa," +
                                //         " (  SELECT IND FROM " + FirmaId_KASA + " WHERE IND=FSB.KASAIND) AS Id," +
                                //         " SUM(FSH.MIKTAR) AS MIKTAR, SUM((((MIKTAR*SATISFIYATI) * (100-ISNULL(FSH.ISK1,0))/100) * (100-ISNULL(FSH.ISK2,0))/100) * (100-ISNULL(FSB.ALTISKORAN,0))/100)  AS TUTAR, STK.KOD1 AS ProductName" +
                                //         " FROM TBLFASTERSATISHAREKET AS FSH " +
                                //         " LEFT JOIN TBLFASTERSATISBASLIK AS FSB ON FSH.BASLIKIND=FSB.BASLIKIND AND FSH.SUBEIND=FSB.SUBEIND AND FSH.KASAIND=FSB.KASAIND " +
                                //         " LEFT JOIN F0" + FirmaId + "TBLSTOKLAR AS STK ON FSH.STOKIND=STK.IND WHERE FSH.ISLEMTARIHI>=@par1 AND FSH.ISLEMTARIHI<=@par2  AND FIRMAIND= @FirmaInd  AND ISNULL(FSB.IADE,0)=0 GROUP BY FSB.SUBEIND,FSB.KASAIND,STK.KOD1)";

                                Query =
                                              "DECLARE @Sube nvarchar(100) = '{SubeAdi}';" +
                                              "DECLARE @par1 nvarchar(20) = '{TARIH1}';" +
                                              "DECLARE @par2 nvarchar(20) = '{TARIH2}';" +
                                            " SELECT T.Sube1," +
                                            "       T.Kasa," +
                                            "       T.Id," +
                                            "       SUM(T.MIKTAR) MIKTAR," +
                                            "       SUM(T.TUTAR) TUTAR," +
                                            "       T.ProductName" +
                                            " FROM (" +
                                            "        (SELECT" +
                                            "           (SELECT SUBEADI" +
                                               "      FROM " + FirmaId_SUBE + "" +
                                            "            WHERE IND=FSB.SUBEIND) AS Sube1," +
                                            "           (SELECT KASAADI" +
                                             "      FROM  " + FirmaId_KASA + "" +
                                            "            WHERE IND=FSB.KASAIND) AS Kasa," +
                                            "           (SELECT IND" +
                                             "      FROM  " + FirmaId_KASA + "" +
                                            "            WHERE IND=FSB.KASAIND) AS Id," +
                                            "                SUM(FSH.MIKTAR) AS MIKTAR," +
                                            "                SUM((((FSH.MIKTAR*FSH.SATISFIYATI) * (100-ISNULL(FSH.ISK1, 0))/100) * (100-ISNULL(FSH.ISK2, 0))/100) * (100-ISNULL(FSB.ALTISKORAN, 0))/100) AS TUTAR," +
                                            "                STK.KOD1 AS ProductName" +
                                            "         FROM TBLFASTERSATISHAREKET AS FSH" +
                                            "         LEFT JOIN TBLFASTERSATISBASLIK AS FSB ON FSH.BASLIKIND=FSB.BASLIKIND" +
                                            "         AND FSH.SUBEIND=FSB.SUBEIND" +
                                            "         AND FSH.KASAIND=FSB.KASAIND" +
                                            "         LEFT JOIN F0" + FirmaId + "TBLSTOKLAR AS STK ON FSH.STOKIND=STK.IND  " +
                                            "         LEFT JOIN F0" + FirmaId + "TBLBIRIMLEREX AS BR ON FSH.BIRIMIND=BR.IND    " +
                                            "         WHERE FSH.ISLEMTARIHI>=@par1" +
                                            "           AND FSH.ISLEMTARIHI<=@par2" +
                                            "           AND ISNULL(FSB.IADE, 0)=0" +
                                               " " + QueryFasterSube +
                                            "         GROUP BY FSB.SUBEIND," +
                                            "                  FSB.KASAIND," +
                                            "                  STK.KOD1" +
                                            "         UNION ALL SELECT" +
                                            "           (SELECT SUBEADI" +
                                               "      FROM " + FirmaId_SUBE + "" +
                                            "            WHERE IND=FSB.SUBEIND) AS Sube1," +
                                            "           (SELECT KASAADI" +
                                             "      FROM  " + FirmaId_KASA + "" +
                                            "            WHERE IND=FSB.KASAIND) AS Kasa," +
                                            "           (SELECT IND" +
                                             "      FROM  " + FirmaId_KASA + "" +
                                            "            WHERE IND=FSB.KASAIND) AS Id," +
                                            "                          SUM(FSH.MIKTAR)*-1.00 AS MIKTAR," +
                                            "                          SUM((((FSH.MIKTAR*FSH.SATISFIYATI) * (100-ISNULL(FSH.ISK1, 0))/100) * (100-ISNULL(FSH.ISK2, 0))/100) * (100-ISNULL(FSB.ALTISKORAN, 0))/100) *-1.00 AS TUTAR," +
                                            "                          STK.KOD1 AS ProductName" +
                                            "         FROM TBLFASTERSATISHAREKET AS FSH" +
                                            "         LEFT JOIN TBLFASTERSATISBASLIK AS FSB ON FSH.BASLIKIND=FSB.BASLIKIND" +
                                            "         AND FSH.SUBEIND=FSB.SUBEIND" +
                                            "         AND FSH.KASAIND=FSB.KASAIND" +
                                            "         LEFT JOIN F0" + FirmaId + "TBLSTOKLAR AS STK ON FSH.STOKIND=STK.IND  " +
                                            "         LEFT JOIN F0" + FirmaId + "TBLBIRIMLEREX AS BR ON FSH.BIRIMIND=BR.IND    " +
                                            "         WHERE FSH.ISLEMTARIHI>=@par1" +
                                            "           AND FSH.ISLEMTARIHI<=@par2" +
                                            "           AND ISNULL(FSB.IADE, 0)=1" +
                                               " " + QueryFasterSube +
                                            "         GROUP BY FSB.SUBEIND," +
                                            "                  FSB.KASAIND," +
                                            "                  STK.KOD1)) T " +
                                            " GROUP BY T.Sube1," +
                                            "         T.Kasa," +
                                            "         T.Id," +
                                            "         T.ProductName";

                            #endregion FASTER ONLINE QUARY

                            if (subeid != null && !subeid.Equals("0") && productGroup != null && productGroup != "False")

                                #region FASTER ONLINE 2.KIRILIM   (3. Liste)
                                //Query =
                                //         " declare @Sube nvarchar(100) = '{SubeAdi}';" +
                                //         " declare @par1 nvarchar(20) = '{TARIH1}';" +
                                //         " declare @par2 nvarchar(20) = '{TARIH2}';" +
                                //         " DECLARE @FirmaInd nvarchar(100) = '{FIRMAIND}';" +
                                //         " ( SELECT (SELECT SUBEADI FROM F0" + FirmaId + "TBLKRDSUBELER WHERE IND=FSB.SUBEIND) AS Sube1," +
                                //         " ( SELECT KASAADI FROM F0" + FirmaId + "TBLKRDKASALAR WHERE IND=FSB.KASAIND) AS Kasa," +
                                //         " ( SELECT IND FROM " + FirmaId_KASA + " WHERE IND=FSB.KASAIND) AS Id," +
                                //         " SUM(FSH.MIKTAR) AS MIKTAR, SUM((((MIKTAR*SATISFIYATI) * (100-ISNULL(FSH.ISK1,0))/100) * (100-ISNULL(FSH.ISK2,0))/100) * (100-ISNULL(FSB.ALTISKORAN,0))/100)  AS TUTAR, STK.MALINCINSI AS ProductName " +
                                //         " FROM TBLFASTERSATISHAREKET AS FSH " +
                                //         " LEFT JOIN TBLFASTERSATISBASLIK AS FSB ON FSH.BASLIKIND=FSB.BASLIKIND AND FSH.SUBEIND=FSB.SUBEIND AND FSH.KASAIND=FSB.KASAIND  " +
                                //         " LEFT JOIN F0" + FirmaId + "TBLSTOKLAR AS STK ON FSH.STOKIND=STK.IND WHERE FSH.ISLEMTARIHI>=@par1 AND FSH.ISLEMTARIHI<=@par2  AND FIRMAIND= @FirmaInd  AND ISNULL(FSB.IADE,0)=0   AND STK.KOD1 ='{ProductName}'" +
                                //         " GROUP BY FSB.SUBEIND,FSB.KASAIND,STK.MALINCINSI)";

                                Query =
                                       "DECLARE @Sube nvarchar(100) = '{SubeAdi}';" +
                                       "DECLARE @par1 nvarchar(20) = '{TARIH1}';" +
                                       "DECLARE @par2 nvarchar(20) = '{TARIH2}';" +
                                       " SELECT T.Sube1," +
                                       "       T.Kasa," +
                                       "       T.Id," +
                                       "       SUM(T.MIKTAR) MIKTAR," +
                                       "       SUM(T.TUTAR) TUTAR," +
                                       "       T.ProductName," +
                                       "	   T.ProductCode" +
                                       " FROM (" +
                                       "        (SELECT" +
                                       "           (SELECT TOP 1 SUBEADI" +
                                       "            FROM F0" + FirmaId + "TBLKRDSUBELER" +
                                       "            WHERE IND=FSB.SUBEIND) AS Sube1," +
                                       "           (SELECT KASAADI" +
                                       "            FROM F0" + FirmaId + "TBLKRDKASALAR" +
                                       "            WHERE IND=FSB.KASAIND) AS Kasa," +
                                       "           (SELECT IND" +
                                       "            FROM F0" + FirmaId + "TBLKRDKASALAR" +
                                       "            WHERE IND=FSB.KASAIND) AS Id," +
                                       "                SUM(FSH.MIKTAR) AS MIKTAR," +
                                       "                SUM((((FSH.MIKTAR*FSH.SATISFIYATI) * (100-ISNULL(FSH.ISK1, 0))/100) * (100-ISNULL(FSH.ISK2, 0))/100) * (100-ISNULL(FSB.ALTISKORAN, 0))/100) AS TUTAR," +
                                       "                STK.MALINCINSI AS ProductName," +
                                       "				STK.STOKKODU AS  ProductCode" +
                                       "         FROM TBLFASTERSATISHAREKET AS FSH" +
                                       "         LEFT JOIN TBLFASTERSATISBASLIK AS FSB ON FSH.BASLIKIND=FSB.BASLIKIND" +
                                       "         AND FSH.SUBEIND=FSB.SUBEIND" +
                                       "         AND FSH.KASAIND=FSB.KASAIND" +
                                       "         LEFT JOIN F0" + FirmaId + "TBLSTOKLAR AS STK ON FSH.STOKIND=STK.IND" +
                                       "         LEFT JOIN F0" + FirmaId + "TBLBIRIMLEREX AS BR ON FSH.BIRIMIND = BR.IND" +
                                       "         WHERE FSH.ISLEMTARIHI>=@par1" +
                                       "           AND FSH.ISLEMTARIHI<=@par2" +
                                       "           AND ISNULL(FSB.IADE, 0)=0" +
                                       "           AND STK.KOD1 ='{ProductName}' " +
                                         " " + QueryFasterSube +
                                       "         GROUP BY FSB.SUBEIND," +
                                       "                  FSB.KASAIND," +
                                       "                  STK.MALINCINSI," +
                                       "				  STK.STOKKODU" +
                                       "         UNION ALL SELECT" +
                                       "           (SELECT TOP 1 SUBEADI" +
                                       "            FROM F0" + FirmaId + "TBLKRDSUBELER" +
                                       "            WHERE IND=FSB.SUBEIND) AS Sube1," +
                                       "           (SELECT KASAADI" +
                                       "            FROM F0" + FirmaId + "TBLKRDKASALAR" +
                                       "            WHERE IND=FSB.KASAIND) AS Kasa," +
                                       "           (SELECT IND" +
                                       "            FROM F0" + FirmaId + "TBLKRDKASALAR" +
                                       "            WHERE IND=FSB.KASAIND) AS Id," +
                                       "                          SUM(FSH.MIKTAR)*-1.00 AS MIKTAR," +
                                       "                          SUM((((FSH.MIKTAR*FSH.SATISFIYATI) * (100-ISNULL(FSH.ISK1, 0))/100) * (100-ISNULL(FSH.ISK2, 0))/100) * (100-ISNULL(FSB.ALTISKORAN, 0))/100) *-1.00 AS TUTAR," +
                                       "                          STK.MALINCINSI AS ProductName," +
                                       "						  STK.STOKKODU AS  ProductCode" +
                                       "         FROM TBLFASTERSATISHAREKET AS FSH" +
                                       "         LEFT JOIN TBLFASTERSATISBASLIK AS FSB ON FSH.BASLIKIND=FSB.BASLIKIND" +
                                       "         AND FSH.SUBEIND=FSB.SUBEIND" +
                                       "         AND FSH.KASAIND=FSB.KASAIND" +
                                       "         LEFT JOIN F0" + FirmaId + "TBLSTOKLAR AS STK ON FSH.STOKIND=STK.IND" +
                                       "         LEFT JOIN F0" + FirmaId + "TBLBIRIMLEREX AS BR ON FSH.BIRIMIND = BR.IND" +
                                       "         WHERE FSH.ISLEMTARIHI>=@par1" +
                                       "           AND FSH.ISLEMTARIHI<=@par2" +
                                       "           AND ISNULL(FSB.IADE, 0)=1" +
                                       "           AND STK.KOD1 ='{ProductName}' " +
                                          " " + QueryFasterSube +
                                       "         GROUP BY FSB.SUBEIND," +
                                       "                  FSB.KASAIND," +
                                       "                  STK.MALINCINSI," +
                                       "				  STK.STOKKODU)) T" +
                                       " GROUP BY T.Sube1," +
                                       "         T.Kasa," +
                                       "         T.Id," +
                                       "		 T.ProductCode," +
                                       "         T.ProductName" +
                                       "		 order by MIKTAR DESC";

                            #endregion FASTER ONLINE 2.KIRILIM

                        }
                        else//Faster Offline
                        {
                            if (subeid != null && !subeid.Equals("0") && productGroup == null)// sube secili degilse ilk giris yapilan sql           
                                Query =
                                     "DECLARE @Sube nvarchar(100) = '{SubeAdi}';" +
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
                                         "      (SELECT TOP 1 SUBEADI" +
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
                                         "   LEFT JOIN TBLFASTERSTOKLAR AS STK ON FSH.STOKIND=STK.STOKNO and FSH.BIRIMIND=STK.BIRIMIND" +
                                         "   WHERE FSH.ISLEMTARIHI>=@par1" +
                                         "     AND FSH.ISLEMTARIHI<=@par2" +
                                         "     AND ISNULL(FSB.IADE, 0)=0" +
                                         "   GROUP BY FSB.SUBEIND," +
                                         "            FSB.KASAIND" +
                                         "			" +
                                         "	UNION ALL" +
                                         "	SELECT" +
                                         "      (SELECT TOP 1 SUBEADI" +
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
                                         "   LEFT JOIN TBLFASTERSATISBASLIK AS FSB ON FSH.BASLIKIND=FSB.BASLIKIND " +
                                         "   AND FSH.SUBEIND=FSB.SUBEIND" +
                                         "   AND FSH.KASAIND=FSB.KASAIND" +
                                         "  LEFT JOIN TBLFASTERSTOKLAR AS STK ON FSH.STOKIND=STK.STOKNO and FSH.BIRIMIND=STK.BIRIMIND " +
                                         "   WHERE FSH.ISLEMTARIHI>=@par1" +
                                         "     AND FSH.ISLEMTARIHI<=@par2" +
                                         "     AND ISNULL(FSB.IADE, 0)=1" +
                                         "   GROUP BY FSB.SUBEIND," +
                                         "            FSB.KASAIND" +
                                         "			" +
                                         "			) ) T" +
                                         "			GROUP BY " +
                                         "			T.Sube1," +
                                         " T.Kasa," +
                                         " T.Id";

                            //Query = File.ReadAllText(HostingEnvironment.MapPath("/Sql/SqlFaster/UrunGrup/UrunGrupFasterOFLINE.sql"), System.Text.UTF8Encoding.Default);

                            if (subeid != null && !subeid.Equals("0") && productGroup == "False")
                                Query =
                                "DECLARE @Sube nvarchar(100) = '{SubeAdi}';" +
                                "DECLARE @par1 nvarchar(20) = '{TARIH1}';" +
                                "DECLARE @par2 nvarchar(20) = '{TARIH2}';" +

                                " SELECT " +
                                " T.Sube1," +
                                " T.Kasa," +
                                " T.Id," +
                                " SUM(T.MIKTAR) MIKTAR," +
                                " SUM(T.TUTAR) TUTAR," +
                                " T.ProductName" +
                                " FROM (" +
                                "  (SELECT" +
                                "      (SELECT TOP 1 SUBEADI" +
                                "      FROM TBLFASTERKASALAR" +
                                "      WHERE SUBENO=FSB.SUBEIND) AS Sube1," +
                                "     (SELECT KASAADI" +
                                "      FROM TBLFASTERKASALAR" +
                                "      WHERE KASANO=FSB.KASAIND) AS Kasa," +
                                "     (SELECT IND" +
                                "      FROM TBLFASTERKASALAR" +
                                "      WHERE KASANO=FSB.KASAIND) AS Id," +
                                "          SUM(FSH.MIKTAR) AS MIKTAR," +
                                "          SUM((((MIKTAR*SATISFIYATI) * (100-ISNULL(FSH.ISK1, 0))/100) * (100-ISNULL(FSH.ISK2, 0))/100) * (100-ISNULL(FSB.ALTISKORAN, 0))/100) AS TUTAR," +
                                "          STK.KOD1 AS ProductName" +
                                "   FROM TBLFASTERSATISHAREKET AS FSH" +
                                "   LEFT JOIN TBLFASTERSATISBASLIK AS FSB ON FSH.BASLIKIND=FSB.BASLIKIND" +
                                "   AND FSH.SUBEIND=FSB.SUBEIND" +
                                "   AND FSH.KASAIND=FSB.KASAIND" +
                                "   LEFT JOIN TBLFASTERSTOKLAR AS STK ON FSH.STOKIND=STK.STOKNO and FSH.BIRIMIND=STK.BIRIMIND" +
                                "   WHERE FSH.ISLEMTARIHI>=@par1" +
                                "     AND FSH.ISLEMTARIHI<=@par2" +
                                "     AND ISNULL(FSB.IADE, 0)=0" +
                                "   GROUP BY FSB.SUBEIND," +
                                "            FSB.KASAIND," +
                                "            STK.KOD1" +
                                "			" +
                                "			UNION ALL SELECT" +
                                "      (SELECT TOP 1 SUBEADI" +
                                "      FROM TBLFASTERKASALAR" +
                                "      WHERE SUBENO=FSB.SUBEIND) AS Sube1," +
                                "     (SELECT KASAADI" +
                                "      FROM TBLFASTERKASALAR" +
                                "      WHERE KASANO=FSB.KASAIND) AS Kasa," +
                                "     (SELECT IND" +
                                "      FROM TBLFASTERKASALAR" +
                                "      WHERE KASANO=FSB.KASAIND) AS Id," +
                                "          SUM(FSH.MIKTAR)*-1.00 AS MIKTAR," +
                                "          SUM((((MIKTAR*SATISFIYATI) * (100-ISNULL(FSH.ISK1, 0))/100) * (100-ISNULL(FSH.ISK2, 0))/100) * (100-ISNULL(FSB.ALTISKORAN, 0))/100) *-1.00 AS TUTAR," +
                                "          STK.KOD1 AS ProductName" +
                                "   FROM TBLFASTERSATISHAREKET AS FSH" +
                                "   LEFT JOIN TBLFASTERSATISBASLIK AS FSB ON FSH.BASLIKIND=FSB.BASLIKIND" +
                                "   AND FSH.SUBEIND=FSB.SUBEIND" +
                                "   AND FSH.KASAIND=FSB.KASAIND" +
                                "   LEFT JOIN TBLFASTERSTOKLAR AS STK ON FSH.STOKIND=STK.STOKNO and FSH.BIRIMIND=STK.BIRIMIND" +
                                "   WHERE FSH.ISLEMTARIHI>=@par1" +
                                "     AND FSH.ISLEMTARIHI<=@par2" +
                                "     AND ISNULL(FSB.IADE, 0)=1" +
                                "   GROUP BY FSB.SUBEIND," +
                                "            FSB.KASAIND," +
                                "            STK.KOD1" +
                                "			" +
                                "			) ) T" +
                                "			GROUP BY " +
                                "			T.Sube1," +
                                " T.Kasa," +
                                " T.Id," +
                                " T.ProductName";

                            //Query = File.ReadAllText(HostingEnvironment.MapPath("/Sql/SqlFaster/UrunGrup/UrunGrupFasterOFLINE2.sql"), System.Text.UTF8Encoding.Default);
                            if (subeid != null && !subeid.Equals("0") && productGroup != null && productGroup != "False")
                                Query =
                                 "DECLARE @Sube nvarchar(100) = '{SubeAdi}';" +
                                 "DECLARE @par1 nvarchar(20) = '{TARIH1}';" +
                                 "DECLARE @par2 nvarchar(20) = '{TARIH2}';" +
                                    " SELECT " +
                                    " T.Sube1," +
                                    " T.Kasa," +
                                    " T.Id," +
                                    " SUM(T.MIKTAR) MIKTAR," +
                                    " SUM(T.TUTAR) TUTAR," +
                                    " T.ProductName" +
                                    " FROM (" +
                                    "  (SELECT" +
                                    "    (SELECT TOP 1 SUBEADI" +
                                    "      FROM TBLFASTERKASALAR" +
                                    "      WHERE SUBENO=FSB.SUBEIND) AS Sube1," +
                                    "     (SELECT KASAADI" +
                                    "      FROM TBLFASTERKASALAR" +
                                    "      WHERE KASANO=FSB.KASAIND) AS Kasa," +
                                    "     (SELECT IND" +
                                    "      FROM TBLFASTERKASALAR" +
                                    "      WHERE KASANO=FSB.KASAIND) AS Id," +
                                    "          SUM(FSH.MIKTAR) AS MIKTAR," +
                                    "          SUM((((MIKTAR*SATISFIYATI) * (100-ISNULL(FSH.ISK1, 0))/100) * (100-ISNULL(FSH.ISK2, 0))/100) * (100-ISNULL(FSB.ALTISKORAN, 0))/100) AS TUTAR," +
                                    "          STK.MALINCINSI AS ProductName" +
                                    "   FROM TBLFASTERSATISHAREKET AS FSH" +
                                    "   LEFT JOIN TBLFASTERSATISBASLIK AS FSB ON FSH.BASLIKIND=FSB.BASLIKIND" +
                                    "   AND FSH.SUBEIND=FSB.SUBEIND" +
                                    "   AND FSH.KASAIND=FSB.KASAIND" +
                                    "   LEFT JOIN TBLFASTERSTOKLAR AS STK ON FSH.STOKIND=STK.STOKNO and FSH.BIRIMIND=STK.BIRIMIND" +
                                    "   WHERE FSH.ISLEMTARIHI>=@par1" +
                                    "     AND FSH.ISLEMTARIHI<=@par2" +
                                    "     AND ISNULL(FSB.IADE, 0)=0" +
                                    "  AND STK.KOD1 ='{ProductName}' " +
                                    "   GROUP BY FSB.SUBEIND," +
                                    "            FSB.KASAIND," +
                                    "            STK.MALINCINSI" +
                                    "			" +
                                    "			UNION ALL SELECT" +
                                    "    (SELECT TOP 1 SUBEADI" +
                                    "      FROM TBLFASTERKASALAR" +
                                    "      WHERE SUBENO=FSB.SUBEIND) AS Sube1," +
                                    "     (SELECT KASAADI" +
                                    "      FROM TBLFASTERKASALAR" +
                                    "      WHERE KASANO=FSB.KASAIND) AS Kasa," +
                                    "     (SELECT IND" +
                                    "      FROM TBLFASTERKASALAR" +
                                    "      WHERE KASANO=FSB.KASAIND) AS Id," +
                                    "          SUM(FSH.MIKTAR)*-1.00 AS MIKTAR," +
                                    "          SUM((((MIKTAR*SATISFIYATI) * (100-ISNULL(FSH.ISK1, 0))/100) * (100-ISNULL(FSH.ISK2, 0))/100) * (100-ISNULL(FSB.ALTISKORAN, 0))/100) *-1.00 AS TUTAR," +
                                    "          STK.MALINCINSI AS ProductName" +
                                    "   FROM TBLFASTERSATISHAREKET AS FSH" +
                                    "   LEFT JOIN TBLFASTERSATISBASLIK AS FSB ON FSH.BASLIKIND=FSB.BASLIKIND" +
                                    "   AND FSH.SUBEIND=FSB.SUBEIND" +
                                    "   AND FSH.KASAIND=FSB.KASAIND" +
                                    "  LEFT JOIN TBLFASTERSTOKLAR AS STK ON FSH.STOKIND=STK.STOKNO and FSH.BIRIMIND=STK.BIRIMIND" +
                                    "   WHERE FSH.ISLEMTARIHI>=@par1" +
                                    "     AND FSH.ISLEMTARIHI<=@par2" +
                                    "     AND ISNULL(FSB.IADE, 0)=1" +
                                    "  AND STK.KOD1 ='{ProductName}' " +
                                    "   GROUP BY FSB.SUBEIND," +
                                    "            FSB.KASAIND," +
                                    "            STK.MALINCINSI" +
                                    "			" +
                                    "			) ) T" +
                                    "			GROUP BY " +
                                    "			T.Sube1," +
                                    " T.Kasa," +
                                    " T.Id," +
                                    " T.ProductName";
                            //Query = File.ReadAllText(HostingEnvironment.MapPath("/Sql/SqlFaster/UrunGrup/UrunGrupDetayFasterOFLINE.sql"), System.Text.UTF8Encoding.Default);
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

                            #region faster için (aynı ID de subeler var bunları ilk listede tek şubenın urungruplarını alıyoruz.Sonra 2. kırılımı almak için productGrubu False yaparak içeri alıyoruz.)

                            if (productGroup == null)
                            {
                                productGroup = "False";
                            }

                            #endregion
                        }

                        #endregion #region Kırılımlarda Ana Şube Altındaki IND id alıyorum 

                        #region NPOS QUARY 1.KIRILIM
                        if (subeid != null && !subeid.Equals("0") && productGroup == null)// sube secili degilse ilk giris yapilan sql
                            Query =
                                " declare @Sube nvarchar(100) = '{SUBEADI}';" +
                                " declare @Trh1 nvarchar(20) = '{TARIH1}';" +
                                " declare @Trh2 nvarchar(20) = '{TARIH2}';" +
                                " SELECT " +
                                " Sube,Sube1,Kasa, SUM(MIKTAR) AS MIKTAR, SUM(TUTAR) AS TUTAR " +
                                " FROM(SELECT Sube, Sube1, Kasa_No as Kasa, Sum(Tutar) as TUTAR, Sum(Miktar) as MIKTAR " +
                                " FROM  " + vega_Db + "..TBLMASTERENPOSSTOK AS HR  WHERE HR.BELGETARIH >= @Trh1 and HR.BELGETARIH <= @Trh2 " +
                                " group by  Sube, Sube1, Kasa_No) AS T  group by Sube,Sube1,Kasa ,MIKTAR,TUTAR ";
                        #endregion NPOS QUARY 1.KIRILIM

                        #region NPOS QUARY 2.KIRILIM                       
                        if (subeid != null && !subeid.Equals("0") && productGroup == "False")
                            Query =
                                  " declare @Sube nvarchar(100) = '{SUBEADI}';" +
                                  " declare @Trh1 nvarchar(20) = '{TARIH1}';" +
                                  " declare @Trh2 nvarchar(20) = '{TARIH2}';" +
                                  " SELECT" +
                                  " Sube,Sube1,Kasa_No AS Kasa,KOD1 AS ProductGroup, Sum(Tutar) as TUTAR,Sum(Miktar) as MIKTAR " +
                                  " FROM  " + vega_Db + "..TBLMASTERENPOSSTOK AS HR " +
                                  " WHERE HR.BELGETARIH >= @Trh1 and HR.BELGETARIH <= @Trh2" +
                                  " group by " +
                                  " Sube,Sube1,Kasa_No,kod1 ";
                        #endregion NPOS QUARY 2.KIRILIM

                        #region NPOS QUARY 3.KIRILIM                       
                        if (subeid != null && !subeid.Equals("0") && productGroup != null && productGroup != "False")
                            Query =
                                  " declare @Sube nvarchar(100) = '{SUBEADI}';" +
                                  " declare @Trh1 nvarchar(20) = '{TARIH1}';" +
                                  " declare @Trh2 nvarchar(20) = '{TARIH2}';" +
                                  " declare @ProductName nvarchar(20) = '{ProductName}';" +
                                  " SELECT " +
                                  " Sube,Sube1,Kasa_No AS Kasa, KOD1 AS ProductGroup ,MALINCINSI as ProductName ,Sum(Tutar) as TUTAR,Sum(Miktar) as MIKTAR " +
                                  " FROM  " + vega_Db + "..TBLMASTERENPOSSTOK AS HR " +
                                  " WHERE HR.BELGETARIH >= @Trh1 and HR.BELGETARIH <= @Trh2 " +
                                  " and KOD1 =@ProductName " +
                                  " group by " +
                                  " MALINCINSI," +
                                  " Sube,Sube1,Kasa_No,kod1 ";
                        #endregion NPOS QUARY 2.KIRILIM
                    }
                    else if (AppDbType == "5")
                    {
                        if (subeid != null && !subeid.Equals("0") && productGroup == null)// sube secili degilse ilk giris yapilan sql
                        {
                            Query = File.ReadAllText(HostingEnvironment.MapPath("/Sql/VPos/PersonelYemekRaporu/PersonelYemekRaporu.sql"), System.Text.UTF8Encoding.Default);
                        }
                        if (subeid == null || subeid.Equals("0"))
                        {
                            Query = File.ReadAllText(HostingEnvironment.MapPath("/Sql/UrunGrup.sql"), System.Text.UTF8Encoding.Default);
                        }
                        if (subeid != null && !subeid.Equals("0") && productGroup != null)
                        {
                            Query = File.ReadAllText(HostingEnvironment.MapPath("/Sql/VPos/PersonelYemekRaporu/PersonelYemekRaporu.sql"), System.Text.UTF8Encoding.Default);
                        }
                    }

                    //Query = Query.Replace("{SubeAdi}", SubeAdi);
                    //Query = Query.Replace("{ProductName}", productGroup);
                    Query = Query.Replace("{TARIH1}", QueryTimeStart);
                    Query = Query.Replace("{TARIH2}", QueryTimeEnd);
                    //Query = Query.Replace("{FIRMAIND}", FirmaId);
                    Query = Query.Replace("{PersonelYemekRaporuAdi}", personelYemekRaporuAdi);
                    Query = Query.Replace("{SUBE2}", vPosSubeKodu);
                    Query = Query.Replace("{KASAKODU}", vPosKasaKodu);

                    #endregion SQL QUARY  *(Sefim || (faster || Faster Offline || Faster Online))*

                    if (ID == "1")
                    {
                        #region GET DATA

                        try
                        {
                            try
                            {
                                DataTable UrunGrubuDt = new DataTable();
                                UrunGrubuDt = f.GetSubeDataWithQuery(f.NewConnectionString(SubeIP, DBName, SqlName, SqlPassword), Query.ToString());

                                #region MyRegion

                                if (UrunGrubuDt.Rows.Count > 0)
                                {
                                    if (subeid.Equals("")) /*LİSTE*/
                                    {
                                        if (AppDbType == "3" || AppDbType == "4")//
                                        {
                                            #region FASTER - ONLINE/OFFLINE (AppDbType = 3 faster kullanan şube) || AppDbType = 4 NPOS

                                            foreach (DataRow SubeR in UrunGrubuDt.Rows)
                                            {
                                                UrunGrubu items = new UrunGrubu();

                                                if (AppDbType == "3")
                                                {
                                                    items.Sube = SubeAdi + "-" + SubeR["Sube1"].ToString() + "-" + SubeR["Kasa"].ToString();
                                                    items.SubeID = SubeId + "~" + SubeR["Id"].ToString();
                                                    items.Miktar += Convert.ToDecimal(SubeR["MIKTAR"]); //f.RTD(SubeR, "MIKTAR");
                                                    items.ProductName = f.RTS(SubeR, "ProductName");
                                                    items.ToplamMiktar += Convert.ToDecimal(SubeR["MIKTAR"]); //f.RTD(SubeR, "MIKTAR");
                                                    items.TotalDebit += Convert.ToDecimal(SubeR["TUTAR"]); //f.RTD(SubeR, "TUTAR");
                                                    lock (locked)
                                                    {
                                                        Liste.Add(items);
                                                    }
                                                }
                                                else
                                                {
                                                    items.Sube = SubeAdi + "_" + SubeR["Sube"].ToString() + "-" + SubeR["Sube1"].ToString() + "-" + SubeR["Kasa"].ToString();
                                                    items.SubeID = SubeId + "~" + SubeR["Kasa"].ToString();
                                                    items.Miktar += Convert.ToDecimal(SubeR["MIKTAR"]); //f.RTD(SubeR, "MIKTAR");
                                                    //items.ProductName = f.RTS(SubeR, "ProductName");
                                                    items.ToplamMiktar += Convert.ToDecimal(SubeR["MIKTAR"]); //f.RTD(SubeR, "MIKTAR");
                                                    items.TotalDebit += Convert.ToDecimal(SubeR["TUTAR"]); //f.RTD(SubeR, "TUTAR");
                                                    lock (locked)
                                                    {
                                                        Liste.Add(items);
                                                    }
                                                }
                                            }

                                            #endregion   FASTER-(AppDbType = 3 faster kullanan şube)
                                        }
                                        else
                                        {
                                            #region SEFIM ESKI/YENI (SUBE BAZLI)  

                                            UrunGrubu items = new UrunGrubu
                                            {
                                                Sube = SubeAdi,
                                                SubeID = (SubeId)
                                            };
                                            foreach (DataRow SubeR in UrunGrubuDt.Rows)
                                            {
                                                items.Miktar = f.RTD(SubeR, "MIKTAR");
                                                items.ProductName = f.RTS(SubeR, "ProductName");
                                                items.ToplamMiktar += f.RTD(SubeR, "MIKTAR");
                                                items.TotalDebit += f.RTD(SubeR, "TUTAR");
                                                items.Ikram += f.RTD(SubeR, "IKRAM");
                                                items.Indirim += f.RTD(SubeR, "INDIRIM");
                                            }
                                            items.NetTutar = items.TotalDebit - (items.Indirim + items.Ikram);
                                            lock (locked)
                                            {
                                                Liste.Add(items);
                                            }
                                            #endregion SEFIM ESKI/YENI 
                                        }
                                    }
                                    else if (!subeid.Equals("") && productGroup == "False" || productGroup != "") /*DETAIL*/
                                    {
                                        if (AppDbType == "3" || AppDbType == "4")
                                        {
                                            if (AppDbType == "3")
                                            {
                                                #region 2.KIRILIM FASTER ONLINE/OFLINE         

                                                foreach (DataRow SubeR in UrunGrubuDt.Rows)
                                                {
                                                    if (subeid == SubeR["Id"].ToString())
                                                    {
                                                        UrunGrubu items = new UrunGrubu
                                                        {
                                                            Sube = SubeAdi,
                                                            SubeID = (SubeId),
                                                            ProductGroup = f.RTS(SubeR, "ProductName")
                                                        };
                                                        if (!subeid.Equals("0"))
                                                        {
                                                            items.Miktar = f.RTD(SubeR, "MIKTAR");
                                                            items.ProductName = f.RTS(SubeR, "ProductName");
                                                        }
                                                        items.Debit = f.RTD(SubeR, "TUTAR");
                                                        lock (locked)
                                                        {
                                                            Liste.Add(items);
                                                        }
                                                    }
                                                }

                                                #endregion 2.KIRILIM FASTER ONLINE/OFLINE 
                                            }
                                            else
                                            {
                                                #region 2.KIRILIM NPOS (DETAY URUN DETAY)   

                                                foreach (DataRow SubeR in UrunGrubuDt.Rows)
                                                {
                                                    if (subeid == SubeR["Kasa"].ToString())
                                                    {
                                                        UrunGrubu items = new UrunGrubu
                                                        {
                                                            Sube = SubeAdi,
                                                            SubeID = (SubeId),
                                                            ProductGroup = f.RTS(SubeR, "ProductGroup")
                                                        };
                                                        if (!subeid.Equals("0"))
                                                        {
                                                            items.Miktar = f.RTD(SubeR, "MIKTAR");
                                                            items.ProductName = f.RTS(SubeR, "ProductName");
                                                        }
                                                        items.Debit = f.RTD(SubeR, "TUTAR");
                                                        lock (locked)
                                                        {
                                                            Liste.Add(items);
                                                        }
                                                    }
                                                }

                                                #endregion 2.KIRILIM NPOS (DETAY URUN DETAY)
                                            }
                                        }
                                        else
                                        {
                                            #region 2.KIRILIM SEFIM ESKI/YENI 

                                            foreach (DataRow SubeR in UrunGrubuDt.Rows)
                                            {
                                                UrunGrubu items = new UrunGrubu
                                                {
                                                    Sube = SubeAdi,
                                                    SubeID = (SubeId),
                                                    ProductGroup = f.RTS(SubeR, "ProductGroup")
                                                };
                                                if (!subeid.Equals("0"))
                                                {
                                                    items.Miktar = f.RTD(SubeR, "MIKTAR");
                                                    items.ProductName = f.RTS(SubeR, "ProductName");
                                                }
                                                items.Debit = f.RTD(SubeR, "TUTAR");
                                                items.Ikram = f.RTD(SubeR, "IKRAM");
                                                items.Indirim = f.RTD(SubeR, "INDIRIM");
                                                items.NetTutar = items.Debit - (items.Indirim + items.Ikram);
                                                lock (locked)
                                                {
                                                    Liste.Add(items);
                                                }
                                            }

                                            #endregion 2.KIRILIM SEFIM ESKI/YENI 
                                        }
                                    }
                                    else /*PRODUCTS_DETAIL*/
                                    {
                                        if (AppDbType == "3")
                                        {
                                            #region 3. KIRILIM FASTER ONLINE/OFFLINE 

                                            foreach (DataRow SubeR in UrunGrubuDt.Rows)
                                            {
                                                if (subeid == SubeR["Id"].ToString())
                                                {
                                                    UrunGrubu items = new UrunGrubu
                                                    {
                                                        Sube = f.RTS(SubeR, "Sube"),
                                                        SubeID = (SubeId),
                                                        Miktar = f.RTD(SubeR, "MIKTAR"),
                                                        ProductName = f.RTS(SubeR, "ProductName"),
                                                        Debit = f.RTD(SubeR, "TUTAR")
                                                    };
                                                    lock (locked)
                                                    {
                                                        Liste.Add(items);
                                                    }
                                                }
                                            }
                                            #endregion 3. KIRILIM FASTER ONLINE/OFFLINE 
                                        }
                                        else
                                        {
                                            #region 3. KIRILIM SEFIM ESKI/YENI 

                                            foreach (DataRow SubeR in UrunGrubuDt.Rows)
                                            {
                                                UrunGrubu items = new UrunGrubu
                                                {
                                                    Sube = f.RTS(SubeR, "Sube"),
                                                    SubeID = (SubeId),
                                                    Miktar = f.RTD(SubeR, "MIKTAR"),
                                                    ProductName = f.RTS(SubeR, "ProductName"),
                                                    Debit = f.RTD(SubeR, "TUTAR")
                                                };
                                                lock (locked)
                                                {
                                                    Liste.Add(items);
                                                }
                                            }
                                            #endregion 3. KIRILIM SEFIM ESKI/YENI 
                                        }
                                    }
                                }
                                else
                                {
                                    UrunGrubu items = new UrunGrubu
                                    {
                                        Sube = SubeAdi + " (Data Yok)",
                                        SubeID = (SubeId)
                                    };
                                    lock (locked)
                                    {
                                        Liste.Add(items);
                                    }
                                }

                                #endregion
                            }
                            catch (Exception) { throw new Exception(SubeAdi); }
                        }
                        catch (Exception ex)
                        {
                            #region EX 

                            Singleton.WritingLogFile2("PersonelYemekRaporuCRUD", ex.ToString(), null, ex.StackTrace);
                            UrunGrubu items = new UrunGrubu
                            {
                                Sube = ex.Message + " (Erişim Yok)",
                                SubeID = (SubeId),
                                ErrorStatus = true,
                                ErrorCode = "01"
                            };
                            Liste.Add(items);

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
                                    try
                                    {
                                        DataTable UrunGrubuDt = new DataTable();
                                        UrunGrubuDt = f.GetSubeDataWithQuery(f.NewConnectionString(SubeIP, DBName, SqlName, SqlPassword), Query.ToString());

                                        #region MyRegion

                                        if (UrunGrubuDt.Rows.Count > 0)
                                        {
                                            if (subeid.Equals("")) /*LİSTE*/
                                            {
                                                if (AppDbType == "3" || AppDbType == "4")//
                                                {
                                                    #region FASTER - ONLINE/OFFLINE (AppDbType = 3 faster kullanan şube) || AppDbType = 4 NPOS

                                                    foreach (DataRow SubeR in UrunGrubuDt.Rows)
                                                    {
                                                        UrunGrubu items = new UrunGrubu();

                                                        if (AppDbType == "3")
                                                        {
                                                            items.Sube = SubeAdi + "-" + SubeR["Sube1"].ToString() + "-" + SubeR["Kasa"].ToString();
                                                            items.SubeID = SubeId + "~" + SubeR["Id"].ToString();
                                                            items.Miktar += Convert.ToDecimal(SubeR["MIKTAR"]); //f.RTD(SubeR, "MIKTAR");
                                                            items.ProductName = f.RTS(SubeR, "ProductName");
                                                            items.ToplamMiktar += Convert.ToDecimal(SubeR["MIKTAR"]); //f.RTD(SubeR, "MIKTAR");
                                                            items.TotalDebit += Convert.ToDecimal(SubeR["TUTAR"]); //f.RTD(SubeR, "TUTAR");
                                                            lock (locked)
                                                            {
                                                                Liste.Add(items);
                                                            }
                                                        }
                                                        else
                                                        {
                                                            items.Sube = SubeAdi + "_" + SubeR["Sube"].ToString() + "-" + SubeR["Sube1"].ToString() + "-" + SubeR["Kasa"].ToString();
                                                            items.SubeID = SubeId + "~" + SubeR["Kasa"].ToString();
                                                            items.Miktar += Convert.ToDecimal(SubeR["MIKTAR"]); //f.RTD(SubeR, "MIKTAR");
                                                                                                                //items.ProductName = f.RTS(SubeR, "ProductName");
                                                            items.ToplamMiktar += Convert.ToDecimal(SubeR["MIKTAR"]); //f.RTD(SubeR, "MIKTAR");
                                                            items.TotalDebit += Convert.ToDecimal(SubeR["TUTAR"]); //f.RTD(SubeR, "TUTAR");
                                                            lock (locked)
                                                            {
                                                                Liste.Add(items);
                                                            }
                                                        }
                                                    }

                                                    #endregion   FASTER-(AppDbType = 3 faster kullanan şube)
                                                }
                                                else
                                                {
                                                    #region SEFIM ESKI/YENI (SUBE BAZLI)  

                                                    UrunGrubu items = new UrunGrubu
                                                    {
                                                        Sube = SubeAdi,
                                                        SubeID = (SubeId)
                                                    };
                                                    foreach (DataRow SubeR in UrunGrubuDt.Rows)
                                                    {
                                                        items.Miktar = f.RTD(SubeR, "MIKTAR");
                                                        items.ProductName = f.RTS(SubeR, "ProductName");
                                                        items.ToplamMiktar += f.RTD(SubeR, "MIKTAR");
                                                        items.TotalDebit += f.RTD(SubeR, "TUTAR");
                                                        items.Ikram += f.RTD(SubeR, "IKRAM");
                                                        items.Indirim += f.RTD(SubeR, "INDIRIM");
                                                    }
                                                    items.NetTutar = items.TotalDebit - (items.Indirim + items.Ikram);
                                                    lock (locked)
                                                    {
                                                        Liste.Add(items);
                                                    }
                                                    #endregion SEFIM ESKI/YENI 
                                                }
                                            }
                                            else if (!subeid.Equals("") && productGroup == "False" || productGroup != "") /*DETAIL*/
                                            {
                                                if (AppDbType == "3" || AppDbType == "4")
                                                {
                                                    if (AppDbType == "3")
                                                    {
                                                        #region 2.KIRILIM FASTER ONLINE/OFLINE         

                                                        foreach (DataRow SubeR in UrunGrubuDt.Rows)
                                                        {
                                                            if (subeid == SubeR["Id"].ToString())
                                                            {
                                                                UrunGrubu items = new UrunGrubu
                                                                {
                                                                    Sube = SubeAdi,
                                                                    SubeID = (SubeId),
                                                                    ProductGroup = f.RTS(SubeR, "ProductName")
                                                                };
                                                                if (!subeid.Equals("0"))
                                                                {
                                                                    items.Miktar = f.RTD(SubeR, "MIKTAR");
                                                                    items.ProductName = f.RTS(SubeR, "ProductName");
                                                                }
                                                                items.Debit = f.RTD(SubeR, "TUTAR");
                                                                lock (locked)
                                                                {
                                                                    Liste.Add(items);
                                                                }
                                                            }
                                                        }

                                                        #endregion 2.KIRILIM FASTER ONLINE/OFLINE 
                                                    }
                                                    else
                                                    {
                                                        #region 2.KIRILIM NPOS (DETAY URUN DETAY)   

                                                        foreach (DataRow SubeR in UrunGrubuDt.Rows)
                                                        {
                                                            if (subeid == SubeR["Kasa"].ToString())
                                                            {
                                                                UrunGrubu items = new UrunGrubu
                                                                {
                                                                    Sube = SubeAdi,
                                                                    SubeID = (SubeId),
                                                                    ProductGroup = f.RTS(SubeR, "ProductGroup")
                                                                };
                                                                if (!subeid.Equals("0"))
                                                                {
                                                                    items.Miktar = f.RTD(SubeR, "MIKTAR");
                                                                    items.ProductName = f.RTS(SubeR, "ProductName");
                                                                }
                                                                items.Debit = f.RTD(SubeR, "TUTAR");
                                                                lock (locked)
                                                                {
                                                                    Liste.Add(items);
                                                                }
                                                            }
                                                        }

                                                        #endregion 2.KIRILIM NPOS (DETAY URUN DETAY)
                                                    }
                                                }
                                                else
                                                {
                                                    #region 2.KIRILIM SEFIM ESKI/YENI 

                                                    foreach (DataRow SubeR in UrunGrubuDt.Rows)
                                                    {
                                                        UrunGrubu items = new UrunGrubu
                                                        {
                                                            Sube = SubeAdi,
                                                            SubeID = (SubeId),
                                                            ProductGroup = f.RTS(SubeR, "ProductGroup")
                                                        };
                                                        if (!subeid.Equals("0"))
                                                        {
                                                            items.Miktar = f.RTD(SubeR, "MIKTAR");
                                                            items.ProductName = f.RTS(SubeR, "ProductName");
                                                        }
                                                        items.Debit = f.RTD(SubeR, "TUTAR");
                                                        items.Ikram = f.RTD(SubeR, "IKRAM");
                                                        items.Indirim = f.RTD(SubeR, "INDIRIM");
                                                        items.NetTutar = items.Debit - (items.Indirim + items.Ikram);
                                                        lock (locked)
                                                        {
                                                            Liste.Add(items);
                                                        }
                                                    }

                                                    #endregion 2.KIRILIM SEFIM ESKI/YENI 
                                                }
                                            }
                                            else /*PRODUCTS_DETAIL*/
                                            {
                                                if (AppDbType == "3")
                                                {
                                                    #region 3. KIRILIM FASTER ONLINE/OFFLINE 

                                                    foreach (DataRow SubeR in UrunGrubuDt.Rows)
                                                    {
                                                        if (subeid == SubeR["Id"].ToString())
                                                        {
                                                            UrunGrubu items = new UrunGrubu
                                                            {
                                                                Sube = f.RTS(SubeR, "Sube"),
                                                                SubeID = (SubeId),
                                                                Miktar = f.RTD(SubeR, "MIKTAR"),
                                                                ProductName = f.RTS(SubeR, "ProductName"),
                                                                Debit = f.RTD(SubeR, "TUTAR")
                                                            };
                                                            lock (locked)
                                                            {
                                                                Liste.Add(items);
                                                            }
                                                        }
                                                    }
                                                    #endregion 3. KIRILIM FASTER ONLINE/OFFLINE 
                                                }
                                                else
                                                {
                                                    #region 3. KIRILIM SEFIM ESKI/YENI 

                                                    foreach (DataRow SubeR in UrunGrubuDt.Rows)
                                                    {
                                                        UrunGrubu items = new UrunGrubu
                                                        {
                                                            Sube = f.RTS(SubeR, "Sube"),
                                                            SubeID = (SubeId),
                                                            Miktar = f.RTD(SubeR, "MIKTAR"),
                                                            ProductName = f.RTS(SubeR, "ProductName"),
                                                            Debit = f.RTD(SubeR, "TUTAR")
                                                        };
                                                        lock (locked)
                                                        {
                                                            Liste.Add(items);
                                                        }
                                                    }
                                                    #endregion 3. KIRILIM SEFIM ESKI/YENI 
                                                }
                                            }
                                        }
                                        else
                                        {
                                            UrunGrubu items = new UrunGrubu
                                            {
                                                Sube = SubeAdi + " (Data Yok)",
                                                SubeID = (SubeId)
                                            };
                                            lock (locked)
                                            {
                                                Liste.Add(items);
                                            }
                                        }

                                        #endregion
                                    }
                                    catch (Exception) { throw new Exception(SubeAdi); }
                                }
                                catch (Exception ex)
                                {
                                    #region EX 

                                    Singleton.WritingLogFile2("PersonelYemekRaporuCRUD", ex.ToString(), null, ex.StackTrace);
                                    UrunGrubu items = new UrunGrubu
                                    {
                                        Sube = ex.Message + " (Erişim Yok)",
                                        SubeID = (SubeId),
                                        ErrorStatus = true,
                                        ErrorCode = "01"
                                    };
                                    Liste.Add(items);

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
            catch (DataException ex)
            {
                Singleton.WritingLogFile2("UrunGrubuCRUD", ex.ToString(), null, ex.StackTrace);
            }
            return Liste;
        }
    }
}