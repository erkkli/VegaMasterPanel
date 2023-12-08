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
    public class ReportsDetailComparisonCRUD
    {
        //(Convert.ToDateTime(Convert.ToDateTime(startDateTime).ToString("dd'/'MM'/'yyyy HH:mm")), Convert.ToDateTime(Convert.ToDateTime(endDateTime).ToString("dd'/'MM'/'yyyy HH:mm")), subeId, ID, urunAdi);
        public static List<SubeCiro> List(DateTime dateTime1, DateTime dateTime2, string subeId, string kullaniciId)
        {
            ModelFunctions mF = new ModelFunctions();
            List<SubeCiro> Liste = new List<SubeCiro>();

            #region GİRİŞ YAPAN KULLANICININ YETKİLİ OLDUĞU ŞUBELER LİSTELENİYOR
            UserViewModel model = UsersListCRUD.YetkiliSubesi(kullaniciId);
            #endregion

            try
            {
                #region SUBSTATION LIST       

                mF.SqlConnOpen();

                string sqlQuerySubeler = string.Empty;
                if (subeId != "null" && !string.IsNullOrEmpty(subeId))
                {
                    sqlQuerySubeler = "SELECT * FROM SubeSettings WHERE Status=1 and ID in(" + subeId + ")";
                }
                else
                {
                    sqlQuerySubeler = "SELECT * FROM SubeSettings WHERE Status = 1  ";
                }

                DataTable dt = mF.DataTable(sqlQuerySubeler);
                var dtList = dt.AsEnumerable().ToList<DataRow>();

                mF.SqlConnClose();

                #endregion SUBSTATION LIST

                #region VEGA DB database ismini çekmek için.

                string vega_Db = "";

                mF.SqlConnOpen();
                DataTable dataVegaDb = mF.DataTable("select* from VegaDbSettings ");
                var vegaDBList = dataVegaDb.AsEnumerable().ToList<DataRow>();
                foreach (var item in vegaDBList)
                {
                    vega_Db = item["DBName"].ToString();
                }
                mF.SqlConnClose();

                #endregion VEGA DB database ismini çekmek için.  

                try
                {
                    #region PARALEL FORECH

                    var locked = new Object();

                    Parallel.ForEach(dtList, r =>
                    {
                        ModelFunctions f = new ModelFunctions();

                        string SubeId = r["Id"].ToString();
                        string SubeAdi = r["SubeName"].ToString();
                        string SubeIP = r["SubeIP"].ToString();
                        string SqlName = r["SqlName"].ToString();
                        string SqlPassword = r["SqlPassword"].ToString();
                        string DBName = r["DBName"].ToString();
                        string FirmaId_SUBE = "F0" + r["FirmaID"].ToString() + "TBLKRDSUBELER";
                        string FirmaId_KASA = "F0" + r["FirmaID"].ToString() + "TBLKRDKASALAR";
                        string Firma_NPOS = r["FirmaID"].ToString();
                        string QueryTimeStart = dateTime1.ToString("yyyy/MM/dd HH:mm:ss");
                        string QueryTimeEnd = dateTime2.ToString("yyyy/MM/dd HH:mm:ss");

                        #region  SEFİM YENI - ESKİ FASTER SQL

                        string AppDbType = f.RTS(r, "AppDbType");
                        string AppDbTypeStatus = f.RTS(r, "AppDbTypeStatus");
                        string Query = "";

                        #region Faster Kasalar seçildiyse ona göre query oluşturuluyor. 

                        string FasterSubeIND = f.RTS(r, "FasterSubeID");
                        string QueryFasterSube = string.Empty;
                        if (FasterSubeIND != null)
                        {
                            QueryFasterSube = "  and SUBEIND IN(" + FasterSubeIND + ") ";
                        }

                        #endregion Faster Kasalar seçildiyse ona göre query oluşturuluyor.

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
                                //Query =
                                //        " declare @Sube nvarchar(100) = '{SUBEADI}';declare @Trh1 nvarchar(20) = '{TARIH1}';declare @Trh2 nvarchar(20) = '{TARIH2}';declare @SUBEADI nvarchar(20) = '{SUBEADITBL}';declare @KASAADI nvarchar(20) = '{KASAADI}'; DECLARE @FirmaInd nvarchar(100) = '{FIRMAIND}';" +
                                //        " WITH Toplamsatis AS ( " +
                                //        " SELECT @Sube as Sube, (SELECT SUBEADI FROM   " + FirmaId_SUBE + " WHERE IND = TBLFASTERODEMELER.SUBEIND) AS Sube1,(SELECT KASAADI FROM " + FirmaId_KASA + "  WHERE IND = TBLFASTERODEMELER.KASAIND) AS Kasa,  ODENEN AS cash, 0 AS Credit, 0 AS Ticket, 0 AS Debit, 0 AS ikram, 0 AS TableNo, 0 AS Discount, 0 AS iptal,0 AS zayi  FROM DBO.TBLFASTERODEMELER WHERE ODEMETIPI = 0 AND ISNULL(IADE,0) = 0 AND ISLEMTARIHI >= @Trh1 AND ISLEMTARIHI <= @Trh2 AND FIRMAIND= @FirmaInd " +
                                //        " UNION ALL SELECT @Sube as Sube, (SELECT SUBEADI FROM  " + FirmaId_SUBE + " WHERE IND = TBLFASTERODEMELER.SUBEIND) AS Sube1,(SELECT KASAADI FROM " + FirmaId_KASA + "  WHERE IND = TBLFASTERODEMELER.KASAIND) AS Kasa, 0 AS cash, ODENEN AS Credit, 0 AS Ticket, 0 AS Debit, 0 AS ikram, 0 AS TableNo, 0 AS Discount, 0 AS iptal,0 AS zayi  FROM DBO.TBLFASTERODEMELER WHERE ODEMETIPI = 1 AND ISNULL(IADE,0)= 0 AND ISLEMTARIHI >= @Trh1 AND ISLEMTARIHI <= @Trh2 AND FIRMAIND= @FirmaInd " +
                                //        " UNION ALL SELECT @Sube as Sube, (SELECT SUBEADI FROM  " + FirmaId_SUBE + " WHERE IND = TBLFASTERODEMELER.SUBEIND) AS Sube1,(SELECT KASAADI FROM " + FirmaId_KASA + "  WHERE IND = TBLFASTERODEMELER.KASAIND) AS Kasa,  0 AS cash, 0 AS Credit, ODENEN AS Ticket, 0 AS Debit, 0 AS ikram, 0 AS TableNo, 0 AS Discount, 0 AS iptal, 0 AS zayi  FROM DBO.TBLFASTERODEMELER WHERE ODEMETIPI = 2 AND ISNULL(IADE,0)= 0 AND ISLEMTARIHI >= @Trh1 AND ISLEMTARIHI <= @Trh2  AND FIRMAIND= @FirmaInd " +
                                //        " UNION ALL SELECT @Sube as Sube, (SELECT SUBEADI FROM  " + FirmaId_SUBE + " WHERE SUBEIND = TBLFASTERODEMELER.SUBEIND) AS Sube1,(SELECT KASAADI FROM " + FirmaId_KASA + "  WHERE IND = TBLFASTERODEMELER.KASAIND) AS Kasa,  0 AS cash, 0 AS Credit,0 AS Ticket, 0 AS Debit, ODENEN AS ikram, 0 AS TableNo, 0 AS Discount, 0 AS iptal, 0 AS zayi  FROM DBO.TBLFASTERODEMELER WHERE ODEMETIPI = 3 AND ISNULL(IADE,0)= 0 AND ISLEMTARIHI >= @Trh1 AND ISLEMTARIHI <= @Trh2  AND FIRMAIND= @FirmaInd " +
                                //        " UNION ALL SELECT @Sube as Sube, (SELECT SUBEADI FROM  " + FirmaId_SUBE + " WHERE IND = TBLFASTERODEMELER.SUBEIND) AS Sube1,(SELECT KASAADI FROM " + FirmaId_KASA + "  WHERE IND = TBLFASTERODEMELER.KASAIND) AS Kasa, 0 AS cash, 0 AS Credit,0 AS Ticket, ODENEN AS Debit,0 AS ikram, 0 AS TableNo, 0 AS Discount, 0 AS iptal, 0 AS zayi  FROM DBO.TBLFASTERODEMELER WHERE ODEMETIPI = 4 AND ISNULL(IADE,0)= 0 AND ISLEMTARIHI >= @Trh1 AND ISLEMTARIHI <= @Trh2 AND FIRMAIND= @FirmaInd " +
                                //        " UNION ALL SELECT @Sube as Sube, (SELECT SUBEADI FROM  " + FirmaId_SUBE + " WHERE IND = TBLFASTERSATISBASLIK.SUBEIND) AS Sube1,(SELECT KASAADI FROM " + FirmaId_KASA + "  WHERE IND = TBLFASTERSATISBASLIK.KASAIND) AS Kasa,  0 AS cash, 0 AS Credit,0 AS Ticket, 0 AS Debit,0 AS ikram, 0 AS TableNo, SATIRISK+ALTISK AS Discount, 0 AS iptal,0 AS zayi  FROM DBO.TBLFASTERSATISBASLIK WHERE ISNULL(IADE, 0) = 0 AND ISLEMTARIHI >= @Trh1 AND ISLEMTARIHI <= @Trh2  AND FIRMAIND= @FirmaInd " +
                                //        " UNION ALL  SELECT @Sube as Sube, (SELECT SUBEADI FROM  " + FirmaId_SUBE + " WHERE IND = TBLFASTERODEMELER.SUBEIND) AS Sube1,(SELECT KASAADI FROM " + FirmaId_KASA + "  WHERE IND = TBLFASTERODEMELER.KASAIND) AS Kasa,  0 AS cash, 0 AS Credit,0 AS Ticket, 0 AS Debit,0 AS ikram, 0 AS TableNo, 0 AS Discount, ODENEN AS iptal, 0 AS zayi  FROM DBO.TBLFASTERODEMELER WHERE ISNULL(IADE, 0) = 1 AND ISLEMTARIHI >= @Trh1 AND ISLEMTARIHI <= @Trh2  AND FIRMAIND= @FirmaInd" +
                                //        " UNION ALL SELECT @Sube as Sube, (SELECT SUBEADI FROM  " + FirmaId_SUBE + " WHERE IND = TBLFASTERSATISBASLIK.SUBEIND) AS Sube1,   (SELECT KASAADI FROM " + FirmaId_KASA + "  WHERE IND = TBLFASTERSATISBASLIK.KASAIND) AS Kasa,  0 AS cash, 0 AS Credit,0 AS Ticket, 0 AS Debit,0 AS ikram, COUNT(*) AS TableNo, 0 AS Discount, 0 AS iptal, 0 AS zayi  FROM DBO.TBLFASTERSATISBASLIK WHERE ISNULL(IADE, 0) = 0 AND ISLEMTARIHI >= @Trh1 AND ISLEMTARIHI <= @Trh2  AND FIRMAIND= @FirmaInd GROUP BY SUBEIND , KASAIND  " +
                                //        " ) SELECT Sube, Sube1, Kasa , SUM(Cash) AS Cash  , SUM(Credit) AS Credit   , Sum(Ticket) AS Ticket, Sum(Debit) AS Debit, Sum(ikram) AS ikram, Sum(TableNo) AS TableNo, Sum(Discount) AS Discount, Sum(iptal) AS iptal, Sum(Zayi) AS Zayi, SUM(Cash + Credit + Ticket + Debit - iptal) AS ToplamCiro,0 AS Saniye,'' AS RowStyle,'' AS RowError FROM toplamsatis GROUP BY Sube,Sube1,Kasa                                                  ";

                                Query =
                                                    "DECLARE @Sube nvarchar(100) = '{SUBEADI}';" +
                                                    "DECLARE @Trh1 nvarchar(20) = '{TARIH1}';" +
                                                    "DECLARE @Trh2 nvarchar(20) = '{TARIH2}';" +
                                                    "DECLARE @SUBEADI nvarchar(20) = '{SUBEADITBL}';" +
                                                    "DECLARE @KASAADI nvarchar(20) = '{KASAADI}';" +
                                                    "DECLARE @FirmaInd nvarchar(100) = '{FIRMAIND}';" +
                                                                        "WITH Toplamsatis AS" +
                                                                        "  (SELECT @Sube AS Sube," +
                                                                        "     (SELECT SUBEADI" +
                                                                        "      FROM " + FirmaId_SUBE + "" +
                                                                        "      WHERE IND = TBLFASTERODEMELER.SUBEIND) AS Sube1," +
                                                                        "     (SELECT KASAADI" +
                                                                        "      FROM  " + FirmaId_KASA + " " +
                                                                        "      WHERE IND = TBLFASTERODEMELER.KASAIND) AS Kasa," +
                                                                        "          ODENEN AS cash," +
                                                                        "          0 AS Credit," +
                                                                        "          0 AS Ticket," +
                                                                        "          0 AS Debit," +
                                                                        "          0 AS ikram," +
                                                                        "          0 AS TableNo," +
                                                                        "          0 AS Discount," +
                                                                        "          0 AS iptal," +
                                                                        "		  0 AS iptal2," +
                                                                        "          0 AS zayi" +
                                                                        "   FROM DBO.TBLFASTERODEMELER" +
                                                                        "   WHERE ODEMETIPI = 0" +
                                                                        "     AND ISNULL(IADE, 0) = 0" +
                                                                        "     AND ISLEMTARIHI >= @Trh1" +
                                                                        "     AND ISLEMTARIHI <= @Trh2" +
                                                                        //"     AND FIRMAIND= @FirmaInd" +
                                                                        "   " + QueryFasterSube +
                                                                        "   UNION ALL SELECT @Sube AS Sube," +
                                                                        "     (SELECT SUBEADI" +
                                                                        "      FROM " + FirmaId_SUBE + "" +
                                                                        "      WHERE IND = TBLFASTERODEMELER.SUBEIND) AS Sube1," +
                                                                        "     (SELECT KASAADI" +
                                                                        "      FROM  " + FirmaId_KASA + " " +
                                                                        "      WHERE IND = TBLFASTERODEMELER.KASAIND) AS Kasa," +
                                                                        "                    ODENEN*-1 AS cash," +
                                                                        "                    0 AS Credit," +
                                                                        "                    0 AS Ticket," +
                                                                        "                    0 AS Debit," +
                                                                        "                    0 AS ikram," +
                                                                        "                    0 AS TableNo," +
                                                                        "                    0 AS Discount," +
                                                                        "                    0 AS iptal," +
                                                                        "					0 AS iptal2," +
                                                                        "                    0 AS zayi" +
                                                                        "   FROM DBO.TBLFASTERODEMELER" +
                                                                        "   WHERE ODEMETIPI=0" +
                                                                        "     AND ISNULL(IADE, 0)=1" +
                                                                        "     AND ISLEMTARIHI >= @Trh1" +
                                                                        "     AND ISLEMTARIHI <= @Trh2" +
                                                                         //"     AND FIRMAIND= @FirmaInd" +
                                                                         "   " + QueryFasterSube +
                                                                        "   UNION ALL SELECT @Sube AS Sube," +
                                                                        "     (SELECT SUBEADI" +
                                                                        "      FROM " + FirmaId_SUBE + "" +
                                                                        "      WHERE IND = TBLFASTERODEMELER.SUBEIND) AS Sube1," +
                                                                        "     (SELECT KASAADI" +
                                                                        "      FROM  " + FirmaId_KASA + " " +
                                                                        "      WHERE IND = TBLFASTERODEMELER.KASAIND) AS Kasa," +
                                                                        "                    0 AS cash," +
                                                                        "                    ODENEN AS Credit," +
                                                                        "                    0 AS Ticket," +
                                                                        "                    0 AS Debit," +
                                                                        "                    0 AS ikram," +
                                                                        "                    0 AS TableNo," +
                                                                        "                    0 AS Discount," +
                                                                        "                    0 AS iptal," +
                                                                        "					0 AS iptal2," +
                                                                        "                    0 AS zayi" +
                                                                        "   FROM DBO.TBLFASTERODEMELER" +
                                                                        "   WHERE ODEMETIPI = 1" +
                                                                        "     AND ISNULL(IADE, 0)= 0" +
                                                                        "     AND ISLEMTARIHI >= @Trh1" +
                                                                        "     AND ISLEMTARIHI <= @Trh2" +
                                                                        "     AND FIRMAIND= @FirmaInd" +
                                                                         "   " + QueryFasterSube +
                                                                        "   UNION ALL SELECT @Sube AS Sube," +
                                                                        "     (SELECT SUBEADI" +
                                                                        "      FROM " + FirmaId_SUBE + "" +
                                                                        "      WHERE IND = TBLFASTERODEMELER.SUBEIND) AS Sube1," +
                                                                        "     (SELECT KASAADI" +
                                                                        "      FROM  " + FirmaId_KASA + " " +
                                                                        "      WHERE IND = TBLFASTERODEMELER.KASAIND) AS Kasa," +
                                                                        "                    0 AS cash," +
                                                                        "                    ODENEN*-1 AS Credit," +
                                                                        "                    0 AS Ticket," +
                                                                        "                    0 AS Debit," +
                                                                        "                    0 AS ikram," +
                                                                        "                    0 AS TableNo," +
                                                                        "                    0 AS Discount," +
                                                                        "                    0 AS iptal," +
                                                                        "					0 AS iptal2," +
                                                                        "                    0 AS zayi" +
                                                                        "   FROM DBO.TBLFASTERODEMELER" +
                                                                        "   WHERE ODEMETIPI=1" +
                                                                        "     AND ISNULL(IADE, 0)=1" +
                                                                        "     AND ISLEMTARIHI >= @Trh1" +
                                                                        "     AND ISLEMTARIHI <= @Trh2" +
                                                                         //"     AND FIRMAIND= @FirmaInd" +
                                                                         "   " + QueryFasterSube +
                                                                        "   UNION ALL SELECT @Sube AS Sube," +
                                                                        "     (SELECT SUBEADI" +
                                                                        "      FROM " + FirmaId_SUBE + "" +
                                                                        "      WHERE IND = TBLFASTERODEMELER.SUBEIND) AS Sube1," +
                                                                        "     (SELECT KASAADI" +
                                                                        "      FROM  " + FirmaId_KASA + " " +
                                                                        "      WHERE IND = TBLFASTERODEMELER.KASAIND) AS Kasa," +
                                                                        "                    0 AS cash," +
                                                                        "                    0 AS Credit," +
                                                                        "                    ODENEN AS Ticket," +
                                                                        "                    0 AS Debit," +
                                                                        "                    0 AS ikram," +
                                                                        "                    0 AS TableNo," +
                                                                        "                    0 AS Discount," +
                                                                        "                    0 AS iptal," +
                                                                        "					0 AS iptal2," +
                                                                        "                    0 AS zayi" +
                                                                        "   FROM DBO.TBLFASTERODEMELER" +
                                                                        "   WHERE ODEMETIPI = 2" +
                                                                        "     AND ISNULL(IADE, 0)= 0" +
                                                                        "     AND ISLEMTARIHI >= @Trh1" +
                                                                        "     AND ISLEMTARIHI <= @Trh2" +
                                                                         //"     AND FIRMAIND= @FirmaInd" +
                                                                         "   " + QueryFasterSube +
                                                                        "   UNION ALL SELECT @Sube AS Sube," +
                                                                        "     (SELECT SUBEADI" +
                                                                        "      FROM " + FirmaId_SUBE + "" +
                                                                        "      WHERE IND = TBLFASTERODEMELER.SUBEIND) AS Sube1," +
                                                                        "     (SELECT KASAADI" +
                                                                        "      FROM  " + FirmaId_KASA + " " +
                                                                        "      WHERE IND = TBLFASTERODEMELER.KASAIND) AS Kasa," +
                                                                        "                    0 AS cash," +
                                                                        "                    0 AS Credit," +
                                                                        "                    ODENEN*-1 AS Ticket," +
                                                                        "                    0 AS Debit," +
                                                                        "                    0 AS ikram," +
                                                                        "                    0 AS TableNo," +
                                                                        "                    0 AS Discount," +
                                                                        "                    0 AS iptal," +
                                                                        "					0 AS iptal2," +
                                                                        "                    0 AS zayi" +
                                                                        "   FROM DBO.TBLFASTERODEMELER" +
                                                                        "   WHERE ODEMETIPI=2" +
                                                                        "     AND ISNULL(IADE, 0)=1" +
                                                                        "     AND ISLEMTARIHI >= @Trh1" +
                                                                        "     AND ISLEMTARIHI <= @Trh2" +
                                                                         //"     AND FIRMAIND= @FirmaInd" +
                                                                         "   " + QueryFasterSube +
                                                                        "   UNION ALL SELECT @Sube AS Sube," +
                                                                        "     (SELECT SUBEADI" +
                                                                        "      FROM " + FirmaId_SUBE + "" +
                                                                        "      WHERE SUBEIND = TBLFASTERODEMELER.SUBEIND) AS Sube1," +
                                                                        "     (SELECT KASAADI" +
                                                                        "      FROM  " + FirmaId_KASA + " " +
                                                                        "      WHERE IND = TBLFASTERODEMELER.KASAIND) AS Kasa," +
                                                                        "                    0 AS cash," +
                                                                        "                    0 AS Credit," +
                                                                        "                    0 AS Ticket," +
                                                                        "                    0 AS Debit," +
                                                                        "                    ODENEN AS ikram," +
                                                                        "                    0 AS TableNo," +
                                                                        "                    0 AS Discount," +
                                                                        "                    0 AS iptal," +
                                                                        "					0 AS iptal2," +
                                                                        "                    0 AS zayi" +
                                                                        "   FROM DBO.TBLFASTERODEMELER" +
                                                                        "   WHERE ODEMETIPI = 3" +
                                                                        "     AND ISNULL(IADE, 0)= 0" +
                                                                        "     AND ISLEMTARIHI >= @Trh1" +
                                                                        "     AND ISLEMTARIHI <= @Trh2" +
                                                                         //"     AND FIRMAIND= @FirmaInd" +
                                                                         "   " + QueryFasterSube +
                                                                        "   UNION ALL SELECT @Sube AS Sube," +
                                                                        "     (SELECT SUBEADI" +
                                                                        "      FROM " + FirmaId_SUBE + "" +
                                                                        "      WHERE IND = TBLFASTERODEMELER.SUBEIND) AS Sube1," +
                                                                        "     (SELECT KASAADI" +
                                                                        "      FROM  " + FirmaId_KASA + " " +
                                                                        "      WHERE IND = TBLFASTERODEMELER.KASAIND) AS Kasa," +
                                                                        "                    0 AS cash," +
                                                                        "                    0 AS Credit," +
                                                                        "                    0 AS Ticket," +
                                                                        "                    ODENEN AS Debit," +
                                                                        "                    0 AS ikram," +
                                                                        "                    0 AS TableNo," +
                                                                        "                    0 AS Discount," +
                                                                        "                    0 AS iptal," +
                                                                        "					0 AS iptal2," +
                                                                        "                    0 AS zayi" +
                                                                        "   FROM DBO.TBLFASTERODEMELER" +
                                                                        "   WHERE ODEMETIPI = 4" +
                                                                        "     AND ISNULL(IADE, 0)= 0" +
                                                                        "     AND ISLEMTARIHI >= @Trh1" +
                                                                        "     AND ISLEMTARIHI <= @Trh2" +
                                                                         //"     AND FIRMAIND= @FirmaInd" +
                                                                         "   " + QueryFasterSube +
                                                                        "   UNION ALL SELECT @Sube AS Sube," +
                                                                        "     (SELECT SUBEADI" +
                                                                        "      FROM " + FirmaId_SUBE + "" +
                                                                        "      WHERE IND = TBLFASTERODEMELER.SUBEIND) AS Sube1," +
                                                                        "     (SELECT KASAADI" +
                                                                        "      FROM  " + FirmaId_KASA + " " +
                                                                        "      WHERE IND = TBLFASTERODEMELER.KASAIND) AS Kasa," +
                                                                        "                    0 AS cash," +
                                                                        "                    0 AS Credit," +
                                                                        "                    0 AS Ticket," +
                                                                        "                    ODENEN*-1 AS Debit," +
                                                                        "                    0 AS ikram," +
                                                                        "                    0 AS TableNo," +
                                                                        "                    0 AS Discount," +
                                                                        "                    0 AS iptal," +
                                                                        "					0 AS iptal2," +
                                                                        "                    0 AS zayi" +
                                                                        "   FROM DBO.TBLFASTERODEMELER" +
                                                                        "   WHERE ODEMETIPI = 4" +
                                                                        "     AND ISNULL(IADE, 0)= 1" +
                                                                        "     AND ISLEMTARIHI >= @Trh1" +
                                                                        "     AND ISLEMTARIHI <= @Trh2" +
                                                                         //"     AND FIRMAIND= @FirmaInd" +
                                                                         "   " + QueryFasterSube +
                                                                        "   UNION ALL SELECT @Sube AS Sube," +
                                                                        "     (SELECT SUBEADI" +
                                                                        "      FROM " + FirmaId_SUBE + "" +
                                                                        "      WHERE IND = TBLFASTERSATISBASLIK.SUBEIND) AS Sube1," +
                                                                        "     (SELECT KASAADI" +
                                                                        "      FROM  " + FirmaId_KASA + " " +
                                                                        "      WHERE IND = TBLFASTERSATISBASLIK.KASAIND) AS Kasa," +
                                                                        "                    0 AS cash," +
                                                                        "                    0 AS Credit," +
                                                                        "                    0 AS Ticket," +
                                                                        "                    0 AS Debit," +
                                                                        "                    0 AS ikram," +
                                                                        "                    0 AS TableNo," +
                                                                        "                    SATIRISK+ALTISK AS Discount," +
                                                                        "                    0 AS iptal," +
                                                                        "					0 AS iptal2," +
                                                                        "                    0 AS zayi" +
                                                                        "   FROM DBO.TBLFASTERSATISBASLIK" +
                                                                        "   WHERE ISNULL(IADE, 0) = 0" +
                                                                        "     AND ISLEMTARIHI >= @Trh1" +
                                                                        "     AND ISLEMTARIHI <= @Trh2" +
                                                                         //"     AND FIRMAIND= @FirmaInd" +
                                                                         "   " + QueryFasterSube +
                                                                        "   UNION ALL SELECT @Sube AS Sube," +
                                                                        "     (SELECT SUBEADI" +
                                                                        "      FROM " + FirmaId_SUBE + "" +
                                                                        "      WHERE IND = TBLFASTERODEMELER.SUBEIND) AS Sube1," +
                                                                        "     (SELECT KASAADI" +
                                                                        "      FROM  " + FirmaId_KASA + " " +
                                                                        "      WHERE IND = TBLFASTERODEMELER.KASAIND) AS Kasa," +
                                                                        "                    0 AS cash," +
                                                                        "                    0 AS Credit," +
                                                                        "                    0 AS Ticket," +
                                                                        "                    0 AS Debit," +
                                                                        "                    0 AS ikram," +
                                                                        "                    0 AS TableNo," +
                                                                        "                    0 AS Discount," +
                                                                        "					0 AS iptal," +
                                                                        "                    ODENEN AS iptal2," +
                                                                        "                    0 AS zayi" +
                                                                        "   FROM DBO.TBLFASTERODEMELER" +
                                                                        "   WHERE ISNULL(IADE, 0) = 1" +
                                                                        "     AND ODEMETIPI NOT IN (0," +
                                                                        "                           1," +
                                                                        "                           2," +
                                                                        "                           4)" +
                                                                        "     AND ISLEMTARIHI >= @Trh1" +
                                                                        "     AND ISLEMTARIHI <= @Trh2" +
                                                                        //"     AND FIRMAIND= @FirmaInd" +
                                                                        "   " + QueryFasterSube +
                                                                        "   UNION ALL SELECT @Sube AS Sube," +
                                                                        "     (SELECT SUBEADI" +
                                                                        "      FROM " + FirmaId_SUBE + "" +
                                                                        "      WHERE IND = TBLFASTERODEMELER.SUBEIND) AS Sube1," +
                                                                        "     (SELECT KASAADI" +
                                                                        "      FROM  " + FirmaId_KASA + " " +
                                                                        "      WHERE IND = TBLFASTERODEMELER.KASAIND) AS Kasa," +
                                                                        "                    0 AS cash," +
                                                                        "                    0 AS Credit," +
                                                                        "                    0 AS Ticket," +
                                                                        "                    0 AS Debit," +
                                                                        "                    0 AS ikram," +
                                                                        "                    0 AS TableNo," +
                                                                        "                    0 AS Discount," +
                                                                        "                    ODENEN AS iptal," +
                                                                        "					0 AS iptal2," +
                                                                        "                    0 AS zayi" +
                                                                        "   FROM DBO.TBLFASTERODEMELER" +
                                                                        "   WHERE ISNULL(IADE, 0) = 1" +
                                                                        "     AND ODEMETIPI  IN (0," +
                                                                        "                           1," +
                                                                        "                           2," +
                                                                        "                           4)" +
                                                                        "     AND ISLEMTARIHI >= @Trh1" +
                                                                        "     AND ISLEMTARIHI <= @Trh2" +
                                                                         //"     AND FIRMAIND= @FirmaInd" +
                                                                         "   " + QueryFasterSube +
                                                                        "   UNION ALL SELECT @Sube AS Sube," +
                                                                        "     (SELECT SUBEADI" +
                                                                        "      FROM " + FirmaId_SUBE + "" +
                                                                        "      WHERE IND = TBLFASTERSATISBASLIK.SUBEIND) AS Sube1," +
                                                                        "     (SELECT KASAADI" +
                                                                        "      FROM  " + FirmaId_KASA + " " +
                                                                        "      WHERE IND = TBLFASTERSATISBASLIK.KASAIND) AS Kasa," +
                                                                        "                    0 AS cash," +
                                                                        "                    0 AS Credit," +
                                                                        "                    0 AS Ticket," +
                                                                        "                    0 AS Debit," +
                                                                        "                    0 AS ikram," +
                                                                        "                    COUNT(*) AS TableNo," +
                                                                        "                    0 AS Discount," +
                                                                        "                    0 AS iptal," +
                                                                        "					0 AS iptal2," +
                                                                        "                    0 AS zayi" +
                                                                        "   FROM DBO.TBLFASTERSATISBASLIK" +
                                                                        "   WHERE ISNULL(IADE, 0) = 0" +
                                                                        "     AND ISLEMTARIHI >= @Trh1" +
                                                                        "     AND ISLEMTARIHI <= @Trh2" +
                                                                         //"     AND FIRMAIND= @FirmaInd" +
                                                                         "   " + QueryFasterSube +
                                                                        "   GROUP BY SUBEIND," +
                                                                        "            KASAIND)" +
                                                                        "SELECT Sube," +
                                                                        "       Sube1," +
                                                                        "       Kasa," +
                                                                        "       SUM(Cash) AS Cash," +
                                                                        "       SUM(Credit) AS Credit," +
                                                                        "       Sum(Ticket) AS Ticket," +
                                                                        "       Sum(Debit) AS Debit," +
                                                                        "       Sum(ikram) AS ikram," +
                                                                        "       Sum(TableNo) AS TableNo," +
                                                                        "       Sum(Discount) AS Discount," +
                                                                        "       Sum(iptal) AS iptal," +
                                                                        "       Sum(Zayi) AS Zayi," +
                                                                        "       SUM(Cash + Credit + Ticket + Debit) -SUM(iptal2) AS ToplamCiro," +
                                                                        "       0 AS Saniye," +
                                                                        "       '' AS RowStyle," +
                                                                        "       '' AS RowError" +
                                                                        " FROM toplamsatis " +
                                                                        " GROUP BY Sube," +
                                                                        "         Sube1," +
                                                                        "         Kasa";

                                #endregion FASTER ONLINE QUARY
                            }
                            else
                            {
                                Query = File.ReadAllText(HostingEnvironment.MapPath("/Sql/SqlFaster/SubeToplamCiro/SubeToplamCiroFasterOFLINE.sql"), System.Text.UTF8Encoding.Default);
                            }
                        }
                        else if (AppDbType == "4")//NPOS>4
                        {
                            #region NPOS QUARY 

                            Query =
                                    " declare @Sube nvarchar(100) = '{SUBEADI}';" +
                                    " declare @Trh1 nvarchar(20) = '{TARIH1}';" +
                                    " declare @Trh2 nvarchar(20) = '{TARIH2}';" +
                                    " select" +
                                    " @Sube, Sube, Sube1, Kasa_No as Kasa ,sUM(Nakit) AS Cash, Sum(Visa) as Credit,Sum(Ticket) as Ticket,Sum(Debit) AS Debit, Sum(Tableno) as TableNo, Sum(Discount) as Discount,Sum(Ikram) as ikram,Sum(Zayi) as Zayi," +
                                    " SUm(case when t.Iptal = 1 then t.Toplam else 0 end) as iptal," +
                                    " SUm(case when t.Iptal = 0 then t.Toplam else 0 end) as ToplamCiro " +
                                    " from( " +
                                    " SELECT " +
                                    " (SELECT top 1 OZELKOD1 FROM  " + vega_Db + "..F0" + Firma_NPOS + "TBLPOSKASATANIM WHERE KASANO = Hr.Kasa_No) AS Sube, " +
                                    " (SELECT top 1 OZELKOD1 FROM   " + vega_Db + "..F0" + Firma_NPOS + "TBLPOSKASATANIM WHERE KASANO = Hr.Kasa_No) AS Sube1," +
                                    "  hr.Kasa_no, " +
                                    " ISNULL((SELECT SUM(Tutar) from " + DBName + "..Odeme WHERE Tus_no = 0 and belge_Id = Hr.belge_Id),0) AS Nakit," +
                                    " ISNULL((SELECT SUM(Tutar) from " + DBName + "..Odeme WHERE Tus_no IN(1, 2, 3, 4) and belge_Id = Hr.belge_Id),0) AS Visa," +
                                    " 0 AS Ticket, " +
                                    " ISNULL((SELECT SUM(Tutar) from " + DBName + "..Odeme WHERE Tus_no IN(5, 6) and belge_Id = Hr.belge_Id),0) AS Debit," +
                                    " 0 AS Ikram " +
                                    " ,0 as Tableno " +
                                    " ,SUM(DISCOUNTTOTAL) as Discount " +
                                    " ,Iptal " +
                                    " ,0 as Zayi, " +
                                    " SUm(hr.Toplam) as Toplam " +
                                    " FROM " + DBName + "..BELGE as hr " +
                                    " where hr.Tarih >= @Trh1 AND hr.Tarih <= @Trh2 " +
                                    " GROUP BY Sicil_No,Kasa_No,hr.Belge_ID,Iptal) as t " +
                                    " group by " +
                                    " Sube,Sube1,Kasa_No  ";
                            #endregion NPOS QUARY 
                        }

                        #endregion SEFİM YENI-ESKİ FASTER SQL

                        Query = Query.Replace("{SUBEADI}", SubeAdi);
                        Query = Query.Replace("{TARIH1}", QueryTimeStart);
                        Query = Query.Replace("{TARIH2}", QueryTimeEnd);
                        Query = Query.Replace("{SUBEADITBL}", FirmaId_SUBE);//F0101TBLKRDSUBELER
                        Query = Query.Replace("{KASAADI}", FirmaId_KASA);//F0101TBLKRDKASALAR
                        Query = Query.Replace("{FIRMAIND}", Firma_NPOS);

                        if (kullaniciId == "1")
                        {
                            #region SUPER ADMİN 

                            try
                            {
                                DataTable SubeCiroDt = new DataTable();
                                SubeCiroDt = f.GetSubeDataWithQuery((f.NewConnectionString(SubeIP, DBName, SqlName, SqlPassword)), Query.ToString());

                                try
                                {
                                    if (SubeCiroDt.Rows.Count > 0)
                                    {
                                        if (AppDbType == "3" || AppDbType == "4")
                                        {
                                            #region FASTER (AppDbType=3 faster kullanan şube) 

                                            foreach (DataRow sube in SubeCiroDt.Rows)
                                            {
                                                SubeCiro items = new SubeCiro();
                                                if (AppDbType == "3")
                                                {
                                                    items.Sube = sube["Sube"].ToString() + "-" + sube["Sube1"].ToString() + "-" + sube["Kasa"].ToString(); //KasaCiroDt.Rows[0]["Sube"].ToString();
                                                }
                                                else
                                                {
                                                    items.Sube = SubeAdi + "_" + sube["Sube"].ToString() + "-" + sube["Sube1"].ToString() + "-" + sube["Kasa"].ToString(); //KasaCiroDt.Rows[0]["Sube"].ToString();
                                                }
                                                items.SubeId = SubeId;
                                                items.Cash = Convert.ToDecimal(sube["Cash"]);//Convert.ToDecimal(SubeCiroDt.Rows[su]["Cash"]); //f.RTD(SubeR, "Cash");
                                                items.Credit = Convert.ToDecimal(sube["Credit"]);//f.RTD(SubeR, "Credit");
                                                items.Ticket = Convert.ToDecimal(sube["Ticket"]);//f.RTD(SubeR, "Ticket");
                                                items.ikram = Convert.ToDecimal(sube["ikram"]); //f.RTD(SubeR, "ikram");
                                                items.TableNo = Convert.ToDecimal(sube["TableNo"]); //f.RTD(SubeR, "TableNo");
                                                items.Discount = Convert.ToDecimal(sube["Discount"]); //f.RTD(SubeR, "Discount");
                                                items.iptal = 0;//Convert.ToDecimal(sube["iptal"]);//f.RTD(SubeR, "iptal");
                                                items.Zayi = Convert.ToDecimal(sube["Zayi"]);//f.RTD(SubeR, "Zayi");                                                                                      
                                                items.Debit = Convert.ToDecimal(sube["Debit"]); //f.RTD(SubeR, "Debit");
                                                items.Ciro = Convert.ToDecimal(sube["ToplamCiro"]);//f.RTD(SubeR, "ToplamCiro") ;//items.Cash + items.Credit + items.Ticket + items.Debit;                                              
                                                items.iade = Convert.ToDecimal(sube["iptal"]);

                                                lock (locked)
                                                {
                                                    Liste.Add(items);
                                                }
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
                                            items.OnlinePayment = f.RTD(SubeCiroDt.Rows[0], "OnlinePayment");  //Convert.ToDecimal(SubeCiroDt.Rows[0]["OnlinePayment"]);
                                            items.iade = Convert.ToDecimal(SubeCiroDt.Rows[0]["iade"]);//f.RTD(SubeR, "iptal");
                                            items.PaketSatis = Convert.ToDecimal(SubeCiroDt.Rows[0]["PaketToplam"]);

                                            lock (locked)
                                            {
                                                Liste.Add(items);
                                            }
                                        }
                                    }
                                    else
                                    {
                                        SubeCiro items = new SubeCiro();
                                        items.Sube = SubeAdi + " (Data Yok) ";
                                        items.SubeId = SubeId;

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
                                try
                                {
                                    #region EX                            

                                    Singleton.WritingLogFile2("SubeCiroCRUD", ex.ToString(), null, ex.StackTrace);

                                    SubeCiro items = new SubeCiro();
                                    items.Sube = SubeAdi + " (Erişim Yok)";
                                    items.SubeId = SubeId;
                                    items.ErrorMessage = "Ciro Raporu Alınamadı.";
                                    items.ErrorStatus = true;
                                    items.ErrorCode = "01";

                                    lock (locked)
                                    {
                                        Liste.Add(items);
                                    }
                                    #endregion

                                }
                                catch (Exception) { }

                            }

                            #endregion SUPER ADMİN 
                        }
                        else
                        {
                            #region KULLANICININ ŞUBE YETKİ KONTROLU YAPILIYOR    

                            foreach (var item in model.FR_SubeListesi)
                            {
                                if (item.SubeID == Convert.ToInt32(SubeId))
                                {
                                    #region SUPER ADMİN 

                                    try
                                    {
                                        DataTable SubeCiroDt = new DataTable();
                                        SubeCiroDt = f.GetSubeDataWithQuery((f.NewConnectionString(SubeIP, DBName, SqlName, SqlPassword)), Query.ToString());

                                        try
                                        {
                                            if (SubeCiroDt.Rows.Count > 0)
                                            {
                                                if (AppDbType == "3" || AppDbType == "4")
                                                {
                                                    #region FASTER (AppDbType=3 faster kullanan şube) 

                                                    foreach (DataRow sube in SubeCiroDt.Rows)
                                                    {
                                                        SubeCiro items = new SubeCiro();
                                                        if (AppDbType == "3")
                                                        {
                                                            items.Sube = sube["Sube"].ToString() + "-" + sube["Sube1"].ToString() + "-" + sube["Kasa"].ToString(); //KasaCiroDt.Rows[0]["Sube"].ToString();
                                                        }
                                                        else
                                                        {
                                                            items.Sube = SubeAdi + "_" + sube["Sube"].ToString() + "-" + sube["Sube1"].ToString() + "-" + sube["Kasa"].ToString(); //KasaCiroDt.Rows[0]["Sube"].ToString();
                                                        }
                                                        items.SubeId = SubeId;
                                                        items.Cash = Convert.ToDecimal(sube["Cash"]);//Convert.ToDecimal(SubeCiroDt.Rows[su]["Cash"]); //f.RTD(SubeR, "Cash");
                                                        items.Credit = Convert.ToDecimal(sube["Credit"]);//f.RTD(SubeR, "Credit");
                                                        items.Ticket = Convert.ToDecimal(sube["Ticket"]);//f.RTD(SubeR, "Ticket");
                                                        items.ikram = Convert.ToDecimal(sube["ikram"]); //f.RTD(SubeR, "ikram");
                                                        items.TableNo = Convert.ToDecimal(sube["TableNo"]); //f.RTD(SubeR, "TableNo");
                                                        items.Discount = Convert.ToDecimal(sube["Discount"]); //f.RTD(SubeR, "Discount");
                                                        items.iptal = 0;//Convert.ToDecimal(sube["iptal"]);//f.RTD(SubeR, "iptal");
                                                        items.Zayi = Convert.ToDecimal(sube["Zayi"]);//f.RTD(SubeR, "Zayi");                                                                                      
                                                        items.Debit = Convert.ToDecimal(sube["Debit"]); //f.RTD(SubeR, "Debit");
                                                        items.Ciro = Convert.ToDecimal(sube["ToplamCiro"]);//f.RTD(SubeR, "ToplamCiro") ;//items.Cash + items.Credit + items.Ticket + items.Debit;                                              
                                                        items.iade = Convert.ToDecimal(sube["iptal"]);

                                                        lock (locked)
                                                        {
                                                            Liste.Add(items);
                                                        }
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
                                                    items.OnlinePayment = f.RTD(SubeCiroDt.Rows[0], "OnlinePayment");  //Convert.ToDecimal(SubeCiroDt.Rows[0]["OnlinePayment"]);
                                                    items.iade = Convert.ToDecimal(SubeCiroDt.Rows[0]["iade"]);//f.RTD(SubeR, "iptal");
                                                    items.PaketSatis = Convert.ToDecimal(SubeCiroDt.Rows[0]["PaketToplam"]);

                                                    lock (locked)
                                                    {
                                                        Liste.Add(items);
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                SubeCiro items = new SubeCiro();
                                                items.Sube = SubeAdi + " (Data Yok) ";
                                                items.SubeId = SubeId;

                                                lock (locked)
                                                {
                                                    Liste.Add(items);
                                                }
                                            }
                                        }
                                        catch (Exception) { throw new Exception(SubeAdi); }
                                    }
                                    catch (Exception ex)
                                    {
                                        try
                                        {
                                            #region EX                            

                                            Singleton.WritingLogFile2("SubeCiroCRUD", ex.ToString(), null, ex.StackTrace);
                                            SubeCiro items = new SubeCiro();
                                            items.Sube = SubeAdi + " (Erişim Yok)";
                                            items.SubeId = SubeId;
                                            items.ErrorMessage = "Ciro Raporu Alınamadı.";
                                            items.ErrorStatus = true;
                                            items.ErrorCode = "01";

                                            lock (locked)
                                            {
                                                Liste.Add(items);
                                            }
                                            #endregion
                                        }
                                        catch (Exception) { }
                                    }

                                    #endregion SUPER ADMİN 
                                }
                            }

                            #endregion KULLANICININ ŞUBE YETKİ KONTROLU YAPILIYOR  
                        }
                    });
                    #endregion PARALEL FORECH
                }
                catch (Exception ex) { Singleton.WritingLogFile2("SubeCiroCRUD", ex.ToString(), null, ex.StackTrace); }
            }
            catch (DataException ex) { Singleton.WritingLogFile2("SubeCiroCRUD", ex.ToString(), null, ex.StackTrace); }
            return Liste;
        }
    }
}