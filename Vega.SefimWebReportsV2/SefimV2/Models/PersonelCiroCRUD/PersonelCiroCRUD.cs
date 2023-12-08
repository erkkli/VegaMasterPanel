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
    public class PersonelCiroCRUD
    {
        public static List<PersonelCiro> List(DateTime Date1, DateTime Date2, string subeid, string ID)
        {
            List<PersonelCiro> Liste = new List<PersonelCiro>();
            ModelFunctions ff = new ModelFunctions();
            DateTime startDate = DateTime.Now;
            #region GİRİŞ YAPAN KULLANICININ YETKİLİ OLDUĞU ŞUBELER LİSTELENİYOR
            UserViewModel model = UsersListCRUD.YetkiliSubesi(ID);
            #endregion
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
                        QueryFasterSube = "  and SUBEIND IN(" + FasterSubeIND + ") ";
                    }
                    #endregion Faster Kasalar seçildiyse ona göre query oluşturuluyor.

                    if (AppDbType == "1")// 1 = yeni şefim, 2 =eski Şefim, 3 = Faster
                    {
                        Query = File.ReadAllText(HostingEnvironment.MapPath("/Sql/SqlNewSef/KasiyerTahsilatNewSefim.sql"), System.Text.UTF8Encoding.Default);
                    }
                    else if (AppDbType == "2")
                    {
                        Query = File.ReadAllText(HostingEnvironment.MapPath("/Sql/PersonelCiro.sql"), System.Text.UTF8Encoding.Default);
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
                        }
                        #endregion Kırılımlarda Ana Şube Altındaki IND id alıyorum

                        if (AppDbTypeStatus == "True")
                        {
                            #region FASTER ONLINE QUARY
                            if (subeid == null && subeid.Equals("0") || subeid == "")
                                Query =
                                                            " DECLARE @FirmaInd nvarchar(100) = '{FIRMAIND}'; " +
                                                            " DECLARE @Sube nvarchar(100) = '{SUBE}'" +
                                                            " DECLARE @Trh1 nvarchar(20) = '{TARIH1}';" +
                                                            " DECLARE @Trh2 nvarchar(20) = '{TARIH2}';" +
                                                            " WITH Toplamsatis AS " +
                                                            "  (SELECT @Sube AS Sube," +
                                                            "     (SELECT SUBEADI" +
                                                            "      FROM " + FirmaId_SUBE + "" +
                                                            "      WHERE IND = TBLFASTERODEMELER.SUBEIND) AS Sube1," +
                                                            "     (SELECT KASAADI" +
                                                            "      FROM " + FirmaId_KASA + "" +
                                                            "      WHERE IND = TBLFASTERODEMELER.KASAIND) AS Kasa," +
                                                            "     (SELECT IND" +
                                                            "      FROM " + FirmaId_KASA + "" +
                                                            "      WHERE IND = TBLFASTERODEMELER.KASAIND) AS Id," +
                                                            "          ODENEN AS cash," +
                                                            "          0 AS Credit," +
                                                            "          0 AS Ticket," +
                                                            "          0 AS Debit," +
                                                            "          0 AS Ikram," +
                                                            "          0 AS HediyeCeki," +
                                                            "          0 AS KasaToplam," +
                                                            "     (SELECT USERNAME" +
                                                            "      FROM TBLUSERS" +
                                                            "      WHERE IND = TBLFASTERODEMELER.USERIND) AS PERSONEL" +
                                                            "   FROM DBO.TBLFASTERODEMELER" +
                                                            "   WHERE ODEMETIPI = 0" +
                                                            "     AND ISNULL(IADE, 0) = 0" +
                                                            "     AND ISLEMTARIHI >= @Trh1" +
                                                            "     AND ISLEMTARIHI <= @Trh2" +
                                                            "  " + QueryFasterSube +
                                                            "	 UNION ALL SELECT @Sube AS Sube," +
                                                            "     (SELECT SUBEADI" +
                                                            "      FROM " + FirmaId_SUBE + "" +
                                                            "      WHERE IND=TBLFASTERODEMELER.SUBEIND) AS Sube1," +
                                                            "     (SELECT KASAADI" +
                                                            "      FROM " + FirmaId_KASA + "" +
                                                            "      WHERE IND=TBLFASTERODEMELER.KASAIND) AS Kasa," +
                                                            "	   (SELECT IND" +
                                                            "      FROM " + FirmaId_KASA + " " +
                                                            "      WHERE IND = TBLFASTERODEMELER.KASAIND) AS Id," +
                                                            "          ODENEN*-1 AS cash," +
                                                            "             0 AS Credit," +
                                                            "          0 AS Ticket," +
                                                            "          0 AS Debit," +
                                                            "          0 AS Ikram," +
                                                            "          0 AS HediyeCeki," +
                                                            "          0 AS KasaToplam," +
                                                            "		   (SELECT USERNAME" +
                                                            "      FROM TBLUSERS" +
                                                            "      WHERE IND = TBLFASTERODEMELER.USERIND) AS PERSONEL" +
                                                            "   FROM DBO.TBLFASTERODEMELER" +
                                                            "   WHERE ODEMETIPI=0" +
                                                            "     AND ISNULL(IADE, 0)=1" +
                                                            "     AND ISLEMTARIHI >= @Trh1" +
                                                            "     AND ISLEMTARIHI <= @Trh2" +
                                                             "  " + QueryFasterSube +
                                                            "   UNION ALL SELECT @Sube AS Sube," +
                                                            "     (SELECT SUBEADI" +
                                                            "      FROM " + FirmaId_SUBE + "" +
                                                            "      WHERE IND = TBLFASTERODEMELER.SUBEIND) AS Sube1," +
                                                            "     (SELECT KASAADI" +
                                                            "      FROM " + FirmaId_KASA + "" +
                                                            "      WHERE IND = TBLFASTERODEMELER.KASAIND) AS Kasa," +
                                                            "     (SELECT IND" +
                                                            "      FROM " + FirmaId_KASA + "" +
                                                            "      WHERE IND = TBLFASTERODEMELER.KASAIND) AS Id," +
                                                            "                    0 AS cash," +
                                                            "                    ODENEN AS Credit," +
                                                            "                    0 AS Ticket," +
                                                            "                    0 AS Debit," +
                                                            "                    0 AS Ikram," +
                                                            "                    0 AS HediyeCeki," +
                                                            "                    0 AS KasaToplam," +
                                                            "     (SELECT USERNAME" +
                                                            "      FROM TBLUSERS" +
                                                            "      WHERE IND = TBLFASTERODEMELER.USERIND) AS PERSONEL" +
                                                            "   FROM DBO.TBLFASTERODEMELER" +
                                                            "   WHERE ODEMETIPI = 1" +
                                                            "     AND ISNULL(IADE, 0)= 0" +
                                                            "     AND ISLEMTARIHI >= @Trh1" +
                                                            "     AND ISLEMTARIHI <= @Trh2" +
                                                             "  " + QueryFasterSube +
                                                            "	 	 UNION ALL SELECT @Sube AS Sube," +
                                                            "     (SELECT SUBEADI" +
                                                            "      FROM " + FirmaId_SUBE + "" +
                                                            "      WHERE IND=TBLFASTERODEMELER.SUBEIND) AS Sube1," +
                                                            "     (SELECT KASAADI" +
                                                            "      FROM " + FirmaId_KASA + "" +
                                                            "      WHERE IND=TBLFASTERODEMELER.KASAIND) AS Kasa," +
                                                            "	   (SELECT IND" +
                                                            "      FROM " + FirmaId_KASA + "" +
                                                            "      WHERE IND = TBLFASTERODEMELER.KASAIND) AS Id," +
                                                            "          0 AS cash," +
                                                            "             ODENEN*-1 AS Credit," +
                                                            "          0 AS Ticket," +
                                                            "          0 AS Debit," +
                                                            "          0 AS Ikram," +
                                                            "          0 AS HediyeCeki," +
                                                            "          0 AS KasaToplam," +
                                                            "		   (SELECT USERNAME" +
                                                            "      FROM TBLUSERS" +
                                                            "      WHERE IND = TBLFASTERODEMELER.USERIND) AS PERSONEL" +
                                                            "   FROM DBO.TBLFASTERODEMELER" +
                                                            "   WHERE ODEMETIPI=1" +
                                                            "     AND ISNULL(IADE, 0)=1" +
                                                            "     AND ISLEMTARIHI >= @Trh1" +
                                                            "     AND ISLEMTARIHI <= @Trh2" +
                                                             "  " + QueryFasterSube +
                                                            "   UNION ALL SELECT @Sube AS Sube," +
                                                            "     (SELECT SUBEADI" +
                                                            "      FROM " + FirmaId_SUBE + "" +
                                                            "      WHERE IND = TBLFASTERODEMELER.SUBEIND) AS Sube1," +
                                                            "     (SELECT KASAADI" +
                                                            "      FROM " + FirmaId_KASA + "" +
                                                            "      WHERE IND = TBLFASTERODEMELER.KASAIND) AS Kasa," +
                                                            "     (SELECT IND" +
                                                            "      FROM " + FirmaId_KASA + "" +
                                                            "      WHERE IND = TBLFASTERODEMELER.KASAIND) AS Id," +
                                                            "                    0 AS cash," +
                                                            "                    0 AS Credit," +
                                                            "                    ODENEN AS Ticket," +
                                                            "                    0 AS Debit," +
                                                            "                    0 AS Ikram," +
                                                            "                    0 AS HediyeCeki," +
                                                            "                    0 AS KasaToplam," +
                                                            "     (SELECT USERNAME" +
                                                            "      FROM TBLUSERS" +
                                                            "      WHERE IND = TBLFASTERODEMELER.USERIND) AS PERSONEL" +
                                                            "   FROM DBO.TBLFASTERODEMELER" +
                                                            "   WHERE ODEMETIPI = 2" +
                                                            "     AND ISNULL(IADE, 0)= 0" +
                                                            "     AND ISLEMTARIHI >= @Trh1" +
                                                            "     AND ISLEMTARIHI <= @Trh2" +
                                                             "  " + QueryFasterSube +
                                                            "	  UNION ALL SELECT @Sube AS Sube," +
                                                            "     (SELECT SUBEADI" +
                                                            "      FROM " + FirmaId_SUBE + "" +
                                                            "      WHERE IND=TBLFASTERODEMELER.SUBEIND) AS Sube1," +
                                                            "     (SELECT KASAADI" +
                                                            "      FROM " + FirmaId_KASA + "" +
                                                            "      WHERE IND=TBLFASTERODEMELER.KASAIND) AS Kasa," +
                                                            "	   (SELECT IND" +
                                                            "      FROM " + FirmaId_KASA + "" +
                                                            "      WHERE IND = TBLFASTERODEMELER.KASAIND) AS Id," +
                                                            "          0 AS cash," +
                                                            "            0 AS Credit," +
                                                            "          ODENEN*-1 AS Ticket," +
                                                            "          0 AS Debit," +
                                                            "          0 AS Ikram," +
                                                            "          0 AS HediyeCeki," +
                                                            "          0 AS KasaToplam," +
                                                            "		   (SELECT USERNAME" +
                                                            "      FROM TBLUSERS" +
                                                            "      WHERE IND = TBLFASTERODEMELER.USERIND) AS PERSONEL" +
                                                            "   FROM DBO.TBLFASTERODEMELER" +
                                                            "   WHERE ODEMETIPI=2" +
                                                            "     AND ISNULL(IADE, 0)=1" +
                                                            "     AND ISLEMTARIHI >= @Trh1" +
                                                            "     AND ISLEMTARIHI <= @Trh2" +
                                                             "  " + QueryFasterSube +
                                                            "	 " +
                                                            "   UNION ALL SELECT @Sube AS Sube," +
                                                            "     (SELECT SUBEADI" +
                                                            "      FROM " + FirmaId_SUBE + "" +
                                                            "      WHERE IND = TBLFASTERODEMELER.SUBEIND) AS Sube1," +
                                                            "     (SELECT KASAADI" +
                                                            "      FROM " + FirmaId_KASA + "" +
                                                            "      WHERE IND = TBLFASTERODEMELER.KASAIND) AS Kasa," +
                                                            "     (SELECT IND" +
                                                            "      FROM " + FirmaId_KASA + "" +
                                                            "      WHERE IND = TBLFASTERODEMELER.KASAIND) AS Id," +
                                                            "                    0 AS cash," +
                                                            "                    0 AS Credit," +
                                                            "                    0 AS Ticket," +
                                                            "                    ODENEN AS Debit," +
                                                            "                    0 AS Ikram," +
                                                            "                    0 AS HediyeCeki," +
                                                            "                    0 AS KasaToplam," +
                                                            "     (SELECT USERNAME" +
                                                            "      FROM TBLUSERS" +
                                                            "      WHERE IND = TBLFASTERODEMELER.USERIND) AS PERSONEL" +
                                                            "   FROM DBO.TBLFASTERODEMELER" +
                                                            "   WHERE ODEMETIPI = 4" +
                                                            "     AND ISNULL(IADE, 0)= 0" +
                                                            "     AND ISLEMTARIHI >= @Trh1" +
                                                            "     AND ISLEMTARIHI <= @Trh2" +
                                                             "  " + QueryFasterSube +
                                                            "	  UNION ALL SELECT @Sube AS Sube," +
                                                            "     (SELECT SUBEADI" +
                                                            "      FROM " + FirmaId_SUBE + " " +
                                                            "      WHERE IND=TBLFASTERODEMELER.SUBEIND) AS Sube1," +
                                                            "     (SELECT KASAADI" +
                                                            "      FROM " + FirmaId_KASA + "" +
                                                            "      WHERE IND=TBLFASTERODEMELER.KASAIND) AS Kasa," +
                                                            "	   (SELECT IND" +
                                                            "      FROM " + FirmaId_KASA + "" +
                                                            "      WHERE IND = TBLFASTERODEMELER.KASAIND) AS Id," +
                                                            "          0 AS cash," +
                                                            "             0 AS Credit," +
                                                            "          0 AS Ticket," +
                                                            "          ODENEN*-1 AS Debit," +
                                                            "          0 AS Ikram," +
                                                            "          0 AS HediyeCeki," +
                                                            "          0 AS KasaToplam," +
                                                            "		   (SELECT USERNAME" +
                                                            "      FROM TBLUSERS" +
                                                            "      WHERE IND = TBLFASTERODEMELER.USERIND) AS PERSONEL" +
                                                            "   FROM DBO.TBLFASTERODEMELER" +
                                                            "   WHERE ODEMETIPI=4" +
                                                            "     AND ISNULL(IADE, 0)=1" +
                                                            "     AND ISLEMTARIHI >= @Trh1" +
                                                            "     AND ISLEMTARIHI <= @Trh2" +
                                                             "  " + QueryFasterSube +
                                                            "   UNION ALL SELECT @Sube AS Sube," +
                                                            "     (SELECT SUBEADI" +
                                                            "      FROM " + FirmaId_SUBE + "" +
                                                            "      WHERE IND = TBLFASTERODEMELER.SUBEIND) AS Sube1," +
                                                            "     (SELECT KASAADI" +
                                                            "      FROM " + FirmaId_KASA + "" +
                                                            "      WHERE IND = TBLFASTERODEMELER.KASAIND) AS Kasa," +
                                                            "     (SELECT IND" +
                                                            "      FROM " + FirmaId_KASA + "" +
                                                            "      WHERE IND = TBLFASTERODEMELER.KASAIND) AS Id," +
                                                            "                    0 AS cash," +
                                                            "                    0 AS Credit," +
                                                            "                    0 AS Ticket," +
                                                            "                    0 AS Debit," +
                                                            "                    ODENEN AS Ikram," +
                                                            "                    0 AS HediyeCeki," +
                                                            "                    0 AS KasaToplam," +
                                                            "     (SELECT USERNAME" +
                                                            "      FROM TBLUSERS" +
                                                            "      WHERE IND = TBLFASTERODEMELER.USERIND) AS PERSONEL" +
                                                            "   FROM DBO.TBLFASTERODEMELER" +
                                                            "   WHERE ODEMETIPI = 3" +
                                                            "     AND ISNULL(IADE, 0)= 0" +
                                                            "     AND ISLEMTARIHI >= @Trh1" +
                                                            "     AND ISLEMTARIHI <= @Trh2" +
                                                             "  " + QueryFasterSube +
                                                            "   UNION ALL SELECT @Sube AS Sube," +
                                                            "     (SELECT SUBEADI" +
                                                            "      FROM " + FirmaId_SUBE + "" +
                                                            "      WHERE IND = TBLFASTERODEMELER.SUBEIND) AS Sube1," +
                                                            "     (SELECT KASAADI" +
                                                            "      FROM " + FirmaId_KASA + "" +
                                                            "      WHERE IND = TBLFASTERODEMELER.KASAIND) AS Kasa," +
                                                            "     (SELECT IND" +
                                                            "      FROM " + FirmaId_SUBE + "" +
                                                            "      WHERE IND = TBLFASTERODEMELER.KASAIND) AS Id," +
                                                            "                    0 AS cash," +
                                                            "                    0 AS Credit," +
                                                            "                    0 AS Ticket," +
                                                            "                    0 AS Debit," +
                                                            "                    0 AS Ikram," +
                                                            "                    ODENEN AS HediyeCeki," +
                                                            "                    0 AS KasaToplam," +
                                                            "     (SELECT USERNAME" +
                                                            "      FROM TBLUSERS" +
                                                            "      WHERE IND = TBLFASTERODEMELER.USERIND) AS PERSONEL" +
                                                            "   FROM DBO.TBLFASTERODEMELER" +
                                                            "   WHERE ODEMETIPI IN  (7,8)" +
                                                            "     AND ISNULL(IADE, 0)= 0" +
                                                            "     AND ISLEMTARIHI >= @Trh1" +
                                                            "     AND ISLEMTARIHI <= @Trh2" +
                                                             "  " + QueryFasterSube +
                                                            "	  UNION ALL SELECT @Sube AS Sube," +
                                                            "     (SELECT SUBEADI" +
                                                            "      FROM " + FirmaId_SUBE + " " +
                                                            "      WHERE IND=TBLFASTERODEMELER.SUBEIND) AS Sube1," +
                                                            "     (SELECT KASAADI" +
                                                            "      FROM " + FirmaId_KASA + "" +
                                                            "      WHERE IND=TBLFASTERODEMELER.KASAIND) AS Kasa," +
                                                            "	   (SELECT IND" +
                                                            "      FROM " + FirmaId_SUBE + "" +
                                                            "      WHERE IND = TBLFASTERODEMELER.KASAIND) AS Id," +
                                                            "          0 AS cash," +
                                                            "             0 AS Credit," +
                                                            "          0 AS Ticket," +
                                                            "          0 AS Debit," +
                                                            "          0 AS Ikram," +
                                                            "          ODENEN*-1 AS HediyeCeki," +
                                                            "          0 AS KasaToplam," +
                                                            "		   (SELECT USERNAME" +
                                                            "      FROM TBLUSERS" +
                                                            "      WHERE IND = TBLFASTERODEMELER.USERIND) AS PERSONEL" +
                                                            "   FROM DBO.TBLFASTERODEMELER" +
                                                            "   WHERE ODEMETIPI IN  (7,8)" +
                                                            "     AND ISNULL(IADE, 0)=1" +
                                                            "     AND ISLEMTARIHI >= @Trh1" +
                                                            "     AND ISLEMTARIHI <= @Trh2" +
                                                             "  " + QueryFasterSube +
                                                            "	 " +
                                                            "	 )" +
                                                            " SELECT Sube," +
                                                            "       Sube1," +
                                                            "       Kasa," +
                                                            "       SUM(Cash) AS CashPayment," +
                                                            "       SUM(Credit) AS CreditPayment," +
                                                            "       Sum(Ticket) AS TicketPayment," +
                                                            "       Sum(Debit) AS DebitPayment," +
                                                            "       Sum(Ikram) AS IkramtPayment," +
                                                            "       Sum(HediyeCeki) AS HediyeCeki," +
                                                            "       Sum(cash + Credit + Ticket + Debit + Ikram + HediyeCeki) AS Total," +
                                                            "       Id" +
                                                            " FROM toplamsatis " +
                                                            " GROUP BY Sube," +
                                                            "         Sube1," +
                                                            "         Kasa," +
                                                            "         Id ";

                            if (subeid != null && !subeid.Equals("0") && subeid != "")
                                Query =


                                                        " DECLARE @FirmaInd nvarchar(100) = '{FIRMAIND}';" +

                                                            "DECLARE @Sube nvarchar(100) = '{SUBE}' " +
                                                            "DECLARE @Trh1 nvarchar(20) = '{TARIH1}';" +
                                                                "DECLARE @Trh2 nvarchar(20) = '{TARIH2}';" +
                                                                " WITH Toplamsatis AS " +
                                                                "  (SELECT @Sube AS Sube," +
                                                                "     (SELECT SUBEADI" +
                                                                "      FROM " + FirmaId_SUBE + " " +
                                                                "      WHERE IND = TBLFASTERODEMELER.SUBEIND) AS Sube1," +
                                                                "     (SELECT KASAADI" +
                                                                "      FROM " + FirmaId_KASA + "" +
                                                                "      WHERE IND = TBLFASTERODEMELER.KASAIND) AS Kasa," +
                                                                "     (SELECT IND" +
                                                                "      FROM " + FirmaId_KASA + "" +
                                                                "      WHERE IND = TBLFASTERODEMELER.KASAIND) AS Id," +
                                                                "          ODENEN AS cash," +
                                                                "          0 AS Credit," +
                                                                "          0 AS Ticket," +
                                                                "          0 AS Debit," +
                                                                "          0 AS Ikram," +
                                                                "          0 AS HediyeCeki," +
                                                                "          0 AS KasaToplam," +
                                                                "     (SELECT USERNAME" +
                                                                "      FROM TBLUSERS" +
                                                                "      WHERE IND = TBLFASTERODEMELER.USERIND) AS PERSONEL" +
                                                                "   FROM DBO.TBLFASTERODEMELER" +
                                                                "   WHERE ODEMETIPI = 0" +
                                                                "     AND ISNULL(IADE, 0) = 0" +
                                                                "     AND ISLEMTARIHI >= @Trh1" +
                                                                "     AND ISLEMTARIHI <= @Trh2" +
                                                                 "  " + QueryFasterSube +
                                                                "	 UNION ALL SELECT @Sube AS Sube," +
                                                                "     (SELECT SUBEADI" +
                                                                "      FROM " + FirmaId_SUBE + "" +
                                                                "      WHERE IND=TBLFASTERODEMELER.SUBEIND) AS Sube1," +
                                                                "     (SELECT KASAADI" +
                                                                "      FROM " + FirmaId_KASA + "" +
                                                                "      WHERE IND=TBLFASTERODEMELER.KASAIND) AS Kasa," +
                                                                "	   (SELECT IND" +
                                                                "      FROM " + FirmaId_KASA + "" +
                                                                "      WHERE IND = TBLFASTERODEMELER.KASAIND) AS Id," +
                                                                "          ODENEN*-1 AS cash," +
                                                                "             0 AS Credit," +
                                                                "          0 AS Ticket," +
                                                                "          0 AS Debit," +
                                                                "          0 AS Ikram," +
                                                                "          0 AS HediyeCeki," +
                                                                "          0 AS KasaToplam," +
                                                                "		   (SELECT USERNAME" +
                                                                "      FROM TBLUSERS" +
                                                                "      WHERE IND = TBLFASTERODEMELER.USERIND) AS PERSONEL" +
                                                                "   FROM DBO.TBLFASTERODEMELER" +
                                                                "   WHERE ODEMETIPI=0" +
                                                                "     AND ISNULL(IADE, 0)=1" +
                                                                "     AND ISLEMTARIHI >= @Trh1" +
                                                                "     AND ISLEMTARIHI <= @Trh2" +
                                                                 "  " + QueryFasterSube +
                                                                "   UNION ALL SELECT @Sube AS Sube," +
                                                                "     (SELECT SUBEADI" +
                                                                "      FROM " + FirmaId_SUBE + " " +
                                                                "      WHERE IND = TBLFASTERODEMELER.SUBEIND) AS Sube1," +
                                                                "     (SELECT KASAADI" +
                                                                "      FROM " + FirmaId_KASA + "" +
                                                                "      WHERE IND = TBLFASTERODEMELER.KASAIND) AS Kasa," +
                                                                "     (SELECT IND" +
                                                                "      FROM " + FirmaId_KASA + "" +
                                                                "      WHERE IND = TBLFASTERODEMELER.KASAIND) AS Id," +
                                                                "                    0 AS cash," +
                                                                "                    ODENEN AS Credit," +
                                                                "                    0 AS Ticket," +
                                                                "                    0 AS Debit," +
                                                                "                    0 AS Ikram," +
                                                                "                    0 AS HediyeCeki," +
                                                                "                    0 AS KasaToplam," +
                                                                "     (SELECT USERNAME" +
                                                                "      FROM TBLUSERS" +
                                                                "      WHERE IND = TBLFASTERODEMELER.USERIND) AS PERSONEL" +
                                                                "   FROM DBO.TBLFASTERODEMELER" +
                                                                "   WHERE ODEMETIPI = 1" +
                                                                "     AND ISNULL(IADE, 0)= 0" +
                                                                "     AND ISLEMTARIHI >= @Trh1" +
                                                                "     AND ISLEMTARIHI <= @Trh2" +
                                                                 "  " + QueryFasterSube +
                                                                "	 	 UNION ALL SELECT @Sube AS Sube," +
                                                                "     (SELECT SUBEADI" +
                                                                "      FROM " + FirmaId_SUBE + "" +
                                                                "      WHERE IND=TBLFASTERODEMELER.SUBEIND) AS Sube1," +
                                                                "     (SELECT KASAADI" +
                                                                "      FROM " + FirmaId_KASA + "" +
                                                                "      WHERE IND=TBLFASTERODEMELER.KASAIND) AS Kasa," +
                                                                "	   (SELECT IND" +
                                                                "      FROM " + FirmaId_KASA + "" +
                                                                "      WHERE IND = TBLFASTERODEMELER.KASAIND) AS Id," +
                                                                "          0 AS cash," +
                                                                "             ODENEN*-1 AS Credit," +
                                                                "          0 AS Ticket," +
                                                                "          0 AS Debit," +
                                                                "          0 AS Ikram," +
                                                                "          0 AS HediyeCeki," +
                                                                "          0 AS KasaToplam," +
                                                                "		   (SELECT USERNAME" +
                                                                "      FROM TBLUSERS" +
                                                                "      WHERE IND = TBLFASTERODEMELER.USERIND) AS PERSONEL" +
                                                                "   FROM DBO.TBLFASTERODEMELER" +
                                                                "   WHERE ODEMETIPI=1" +
                                                                "     AND ISNULL(IADE, 0)=1" +
                                                                "     AND ISLEMTARIHI >= @Trh1" +
                                                                "     AND ISLEMTARIHI <= @Trh2" +
                                                                 "  " + QueryFasterSube +
                                                                "   UNION ALL SELECT @Sube AS Sube," +
                                                                "     (SELECT SUBEADI" +
                                                                "      FROM " + FirmaId_SUBE + " " +
                                                                "      WHERE IND = TBLFASTERODEMELER.SUBEIND) AS Sube1," +
                                                                "     (SELECT KASAADI" +
                                                                "      FROM " + FirmaId_KASA + "" +
                                                                "      WHERE IND = TBLFASTERODEMELER.KASAIND) AS Kasa," +
                                                                "     (SELECT IND" +
                                                                "      FROM " + FirmaId_KASA + "" +
                                                                "      WHERE IND = TBLFASTERODEMELER.KASAIND) AS Id," +
                                                                "                    0 AS cash," +
                                                                "                    0 AS Credit," +
                                                                "                    ODENEN AS Ticket," +
                                                                "                    0 AS Debit," +
                                                                "                    0 AS Ikram," +
                                                                "                    0 AS HediyeCeki," +
                                                                "                    0 AS KasaToplam," +
                                                                "     (SELECT USERNAME" +
                                                                "      FROM TBLUSERS" +
                                                                "      WHERE IND = TBLFASTERODEMELER.USERIND) AS PERSONEL" +
                                                                "   FROM DBO.TBLFASTERODEMELER" +
                                                                "   WHERE ODEMETIPI = 2" +
                                                                "     AND ISNULL(IADE, 0)= 0" +
                                                                "     AND ISLEMTARIHI >= @Trh1" +
                                                                "     AND ISLEMTARIHI <= @Trh2" +
                                                                 "  " + QueryFasterSube +
                                                                "	  UNION ALL SELECT @Sube AS Sube," +
                                                                "     (SELECT SUBEADI" +
                                                                "      FROM " + FirmaId_SUBE + "" +
                                                                "      WHERE IND=TBLFASTERODEMELER.SUBEIND) AS Sube1," +
                                                                "     (SELECT KASAADI" +
                                                                "      FROM " + FirmaId_KASA + "" +
                                                                "      WHERE IND=TBLFASTERODEMELER.KASAIND) AS Kasa," +
                                                                "	   (SELECT IND" +
                                                                "      FROM " + FirmaId_KASA + "" +
                                                                "      WHERE IND = TBLFASTERODEMELER.KASAIND) AS Id," +
                                                                "          0 AS cash," +
                                                                "            0 AS Credit," +
                                                                "          ODENEN*-1 AS Ticket," +
                                                                "          0 AS Debit," +
                                                                "          0 AS Ikram," +
                                                                "          0 AS HediyeCeki," +
                                                                "          0 AS KasaToplam," +
                                                                "		   (SELECT USERNAME" +
                                                                "      FROM TBLUSERS" +
                                                                "      WHERE IND = TBLFASTERODEMELER.USERIND) AS PERSONEL" +
                                                                "   FROM DBO.TBLFASTERODEMELER" +
                                                                "   WHERE ODEMETIPI=2" +
                                                                "     AND ISNULL(IADE, 0)=1" +
                                                                "     AND ISLEMTARIHI >= @Trh1" +
                                                                "     AND ISLEMTARIHI <= @Trh2" +
                                                                 "  " + QueryFasterSube +
                                                                "	 " +
                                                                "   UNION ALL SELECT @Sube AS Sube," +
                                                                "     (SELECT SUBEADI" +
                                                                "      FROM " + FirmaId_SUBE + " " +
                                                                "      WHERE IND = TBLFASTERODEMELER.SUBEIND) AS Sube1," +
                                                                "     (SELECT KASAADI" +
                                                                "      FROM " + FirmaId_KASA + "" +
                                                                "      WHERE IND = TBLFASTERODEMELER.KASAIND) AS Kasa," +
                                                                "     (SELECT IND" +
                                                                "      FROM " + FirmaId_KASA + "" +
                                                                "      WHERE IND = TBLFASTERODEMELER.KASAIND) AS Id," +
                                                                "                    0 AS cash," +
                                                                "                    0 AS Credit," +
                                                                "                    0 AS Ticket," +
                                                                "                    ODENEN AS Debit," +
                                                                "                    0 AS Ikram," +
                                                                "                    0 AS HediyeCeki," +
                                                                "                    0 AS KasaToplam," +
                                                                "     (SELECT USERNAME" +
                                                                "      FROM TBLUSERS" +
                                                                "      WHERE IND = TBLFASTERODEMELER.USERIND) AS PERSONEL" +
                                                                "   FROM DBO.TBLFASTERODEMELER" +
                                                                "   WHERE ODEMETIPI = 4" +
                                                                "     AND ISNULL(IADE, 0)= 0" +
                                                                "     AND ISLEMTARIHI >= @Trh1" +
                                                                "     AND ISLEMTARIHI <= @Trh2" +
                                                                 "  " + QueryFasterSube +
                                                                "	  UNION ALL SELECT @Sube AS Sube," +
                                                                "     (SELECT SUBEADI" +
                                                                "      FROM " + FirmaId_SUBE + "" +
                                                                "      WHERE IND=TBLFASTERODEMELER.SUBEIND) AS Sube1," +
                                                                "     (SELECT KASAADI" +
                                                                "      FROM " + FirmaId_KASA + "" +
                                                                "      WHERE IND=TBLFASTERODEMELER.KASAIND) AS Kasa," +
                                                                "	   (SELECT IND" +
                                                                "      FROM " + FirmaId_KASA + "" +
                                                                "      WHERE IND = TBLFASTERODEMELER.KASAIND) AS Id," +
                                                                "          0 AS cash," +
                                                                "             0 AS Credit," +
                                                                "          0 AS Ticket," +
                                                                "          ODENEN*-1 AS Debit," +
                                                                "          0 AS Ikram," +
                                                                "          0 AS HediyeCeki," +
                                                                "          0 AS KasaToplam," +
                                                                "		   (SELECT USERNAME" +
                                                                "      FROM TBLUSERS" +
                                                                "      WHERE IND = TBLFASTERODEMELER.USERIND) AS PERSONEL" +
                                                                "   FROM DBO.TBLFASTERODEMELER" +
                                                                "   WHERE ODEMETIPI=4" +
                                                                "     AND ISNULL(IADE, 0)=1" +
                                                                "     AND ISLEMTARIHI >= @Trh1" +
                                                                "     AND ISLEMTARIHI <= @Trh2" +
                                                                 "  " + QueryFasterSube +
                                                                "   UNION ALL SELECT @Sube AS Sube," +
                                                                "     (SELECT SUBEADI" +
                                                                "      FROM " + FirmaId_SUBE + " " +
                                                                "      WHERE IND = TBLFASTERODEMELER.SUBEIND) AS Sube1," +
                                                                "     (SELECT KASAADI" +
                                                                "      FROM " + FirmaId_KASA + "" +
                                                                "      WHERE IND = TBLFASTERODEMELER.KASAIND) AS Kasa," +
                                                                "     (SELECT IND" +
                                                                "      FROM " + FirmaId_KASA + "" +
                                                                "      WHERE IND = TBLFASTERODEMELER.KASAIND) AS Id," +
                                                                "                    0 AS cash," +
                                                                "                    0 AS Credit," +
                                                                "                    0 AS Ticket," +
                                                                "                    0 AS Debit," +
                                                                "                    ODENEN AS Ikram," +
                                                                "                    0 AS HediyeCeki," +
                                                                "                    0 AS KasaToplam," +
                                                                "     (SELECT USERNAME" +
                                                                "      FROM TBLUSERS" +
                                                                "      WHERE IND = TBLFASTERODEMELER.USERIND) AS PERSONEL" +
                                                                "   FROM DBO.TBLFASTERODEMELER" +
                                                                "   WHERE ODEMETIPI = 3" +
                                                                "     AND ISNULL(IADE, 0)= 0" +
                                                                "     AND ISLEMTARIHI >= @Trh1" +
                                                                "     AND ISLEMTARIHI <= @Trh2" +
                                                                 "  " + QueryFasterSube +
                                                                "   UNION ALL SELECT @Sube AS Sube," +
                                                                "     (SELECT SUBEADI" +
                                                                "      FROM " + FirmaId_SUBE + " " +
                                                                "      WHERE IND = TBLFASTERODEMELER.SUBEIND) AS Sube1," +
                                                                "     (SELECT KASAADI" +
                                                                "      FROM " + FirmaId_KASA + "" +
                                                                "      WHERE IND = TBLFASTERODEMELER.KASAIND) AS Kasa," +
                                                                "     (SELECT IND" +
                                                                "      FROM " + FirmaId_KASA + "" +
                                                                "      WHERE IND = TBLFASTERODEMELER.KASAIND) AS Id," +
                                                                "                    0 AS cash," +
                                                                "                    0 AS Credit," +
                                                                "                    0 AS Ticket," +
                                                                "                    0 AS Debit," +
                                                                "                    0 AS Ikram," +
                                                                "                    ODENEN AS HediyeCeki," +
                                                                "                    0 AS KasaToplam," +
                                                                "     (SELECT USERNAME" +
                                                                "      FROM TBLUSERS" +
                                                                "      WHERE IND = TBLFASTERODEMELER.USERIND) AS PERSONEL" +
                                                                "   FROM DBO.TBLFASTERODEMELER" +
                                                                "   WHERE ODEMETIPI IN  (7,8)" +
                                                                "     AND ISNULL(IADE, 0)= 0" +
                                                                "     AND ISLEMTARIHI >= @Trh1" +
                                                                "     AND ISLEMTARIHI <= @Trh2" +
                                                                 "  " + QueryFasterSube +
                                                                "	 " +
                                                                "	 " +
                                                                "	  UNION ALL SELECT @Sube AS Sube," +
                                                                "     (SELECT SUBEADI" +
                                                                "      FROM " + FirmaId_SUBE + "" +
                                                                "      WHERE IND=TBLFASTERODEMELER.SUBEIND) AS Sube1," +
                                                                "     (SELECT KASAADI" +
                                                                "      FROM " + FirmaId_KASA + "" +
                                                                "      WHERE IND=TBLFASTERODEMELER.KASAIND) AS Kasa," +
                                                                "	   (SELECT IND" +
                                                                "      FROM " + FirmaId_KASA + "" +
                                                                "      WHERE IND = TBLFASTERODEMELER.KASAIND) AS Id," +
                                                                "          0 AS cash," +
                                                                "             0 AS Credit," +
                                                                "          0 AS Ticket," +
                                                                "          0 AS Debit," +
                                                                "          0 AS Ikram," +
                                                                "          ODENEN*-1 AS HediyeCeki," +
                                                                "          0 AS KasaToplam," +
                                                                "		   (SELECT USERNAME" +
                                                                "      FROM TBLUSERS" +
                                                                "      WHERE IND = TBLFASTERODEMELER.USERIND) AS PERSONEL" +
                                                                "   FROM DBO.TBLFASTERODEMELER" +
                                                                "   WHERE ODEMETIPI IN  (7,8)" +
                                                                "     AND ISNULL(IADE, 0)=1" +
                                                                "     AND ISLEMTARIHI >= @Trh1" +
                                                                "     AND ISLEMTARIHI <= @Trh2" +
                                                                 "  " + QueryFasterSube +
                                                                "	 " +
                                                                "	 )" +
                                                                " SELECT Sube," +
                                                                "       Sube1," +
                                                                "       Kasa," +
                                                                "       SUM(Cash) AS CashPayment," +
                                                                "       SUM(Credit) AS CreditPayment," +
                                                                "       Sum(Ticket) AS TicketPayment," +
                                                                "       Sum(Debit) AS DebitPayment," +
                                                                "       Sum(Ikram) AS IkramtPayment," +
                                                                "       Sum(HediyeCeki) AS HediyeCeki," +
                                                                "       Sum(cash + Credit + Ticket + Debit + Ikram + HediyeCeki) AS Total," +
                                                                "	   PERSONEL," +
                                                                "       Id" +
                                                                "       " +
                                                                " FROM toplamsatis " +
                                                                " GROUP BY Sube," +
                                                                "         Sube1," +
                                                                "         Kasa," +
                                                                "		 PERSONEL," +
                                                                "         Id ";

                            #endregion FASTER ONLINE QUARY
                        }
                        else
                        {
                            //Query = File.ReadAllText(HostingEnvironment.MapPath("/Sql/SqlFaster/PersonelCiro/PersonelCiroFasterOFLINE1.sql"), System.Text.UTF8Encoding.Default);
                            if (subeid == null && subeid.Equals("0") || subeid == "")
                                Query = File.ReadAllText(HostingEnvironment.MapPath("/Sql/SqlFaster/PersonelCiro/PersonelCiroFasterOFLINE1.sql"), System.Text.UTF8Encoding.Default);
                            if (subeid != null && !subeid.Equals("0") && subeid != "")
                                Query = File.ReadAllText(HostingEnvironment.MapPath("/Sql/SqlFaster/PersonelCiro/PersonelCiroFasterOFLINE2.sql"), System.Text.UTF8Encoding.Default);
                        }
                    }
                    else if (AppDbType == "5")
                    {
                        Query = File.ReadAllText(HostingEnvironment.MapPath("/Sql/VPos/KasiyerTahsilatRaporu/KasiyerTahsilat.sql"), System.Text.UTF8Encoding.Default);
                        //C:\WORK\VegaSefimWebReportsV2\Vega.SefimWebReportsV2\SefimV2\Sql\VPos\KasiyerTahsilatRaporu\KasiyerTahsilat.sql
                    }
                    #endregion

                    Query = Query.Replace("{TARIH1}", QueryTimeStart);
                    Query = Query.Replace("{TARIH2}", QueryTimeEnd);
                    Query = Query.Replace("{SUBE}", SubeAdi);
                    Query = Query.Replace("{FIRMAIND}", FirmaId);
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
                                            foreach (DataRow sube in AcikHesapDt.Rows)
                                            {
                                                PersonelCiro items = new PersonelCiro();
                                                items.Sube = SubeAdi + "-" + sube["Sube1"].ToString() + "-" + sube["Kasa"].ToString();
                                                items.SubeID = SubeId + "~" + (sube["Id"].ToString());
                                                items.PersonelAdi = "Kasa Satış"; //f.RTS(SubeR, "PersonelAdi");
                                                                                  //foreach (DataRow SubeR in AcikHesapDt.Rows)
                                                                                  //{
                                                items.Total += f.RTD(sube, "Total");
                                                //}
                                                Liste.Add(items);
                                            }
                                        }
                                        else
                                        {
                                            PersonelCiro items = new PersonelCiro();
                                            items.Sube = SubeAdi; //f.RTS(SubeUrunCiroDt.Rows[0], "Sube");
                                            items.SubeID = SubeId;
                                            items.PersonelAdi = "Kasa Satış"; //f.RTS(SubeR, "PersonelAdi");
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
                                                if (subeid == SubeR["Id"].ToString())
                                                {
                                                    PersonelCiro items = new PersonelCiro();
                                                    items.Sube = SubeAdi;
                                                    items.SubeID = (SubeId);
                                                    items.ReceivedByUserName = f.RTS(SubeR, "PERSONEL");
                                                    items.CashPayment = f.RTD(SubeR, "CashPayment");
                                                    items.Total = f.RTD(SubeR, "Total");
                                                    items.CreditPayment = f.RTD(SubeR, "CreditPayment");
                                                    items.TicketPayment = f.RTD(SubeR, "TicketPayment");
                                                    items.DebitPayment = f.RTD(SubeR, "DebitPayment");
                                                    items.IkramPayment = f.RTD(SubeR, "IkramPayment");
                                                    items.HediyeCeki = f.RTD(SubeR, "HediyeCeki");
                                                    Liste.Add(items);
                                                }
                                            }
                                        }
                                        else
                                        {
                                            foreach (DataRow SubeR in AcikHesapDt.Rows)
                                            {
                                                PersonelCiro items = new PersonelCiro();
                                                items.Sube = SubeAdi;
                                                items.SubeID = (SubeId);
                                                items.ReceivedByUserName = f.RTS(SubeR, "ReceivedByUserName");
                                                items.CashPayment = f.RTD(SubeR, "CashPayment");
                                                items.Total = f.RTD(SubeR, "Total");
                                                items.CreditPayment = f.RTD(SubeR, "CreditPayment");
                                                items.TicketPayment = f.RTD(SubeR, "TicketPayment");
                                                Liste.Add(items);
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    PersonelCiro items = new PersonelCiro();
                                    items.Sube = SubeAdi + " (Data Yok) ";
                                    items.SubeID = (SubeId);
                                    items.PersonelAdi = "*";
                                    items.Debit = 0;
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
                            PersonelCiro items = new PersonelCiro();
                            //items.CustomerName = cu;
                            //items.Debit = SubeId;
                            items.Sube = ex.Message + " (Erişim Yok) ";
                            items.SubeID = (SubeId);
                            items.PersonelAdi = "*";
                            items.Debit = 0;//f.RTD(SubeR, "TUTAR");
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
                                                    foreach (DataRow sube in AcikHesapDt.Rows)
                                                    {
                                                        PersonelCiro items = new PersonelCiro();
                                                        items.Sube = SubeAdi + "-" + sube["Sube1"].ToString() + "-" + sube["Kasa"].ToString();
                                                        items.SubeID = SubeId + "~" + (sube["Id"].ToString());
                                                        items.PersonelAdi = "Kasa Satış"; //f.RTS(SubeR, "PersonelAdi");
                                                                                          //foreach (DataRow SubeR in AcikHesapDt.Rows)
                                                                                          //{
                                                        items.Total += f.RTD(sube, "Total");
                                                        //}
                                                        Liste.Add(items);
                                                    }
                                                }
                                                else
                                                {
                                                    PersonelCiro items = new PersonelCiro();
                                                    items.Sube = SubeAdi; //f.RTS(SubeUrunCiroDt.Rows[0], "Sube");
                                                    items.SubeID = SubeId;
                                                    items.PersonelAdi = "Kasa Satış"; //f.RTS(SubeR, "PersonelAdi");
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
                                                        if (subeid == SubeR["Id"].ToString())
                                                        {
                                                            PersonelCiro items = new PersonelCiro();
                                                            items.Sube = SubeAdi;
                                                            items.SubeID = (SubeId);
                                                            items.ReceivedByUserName = f.RTS(SubeR, "PERSONEL");
                                                            items.CashPayment = f.RTD(SubeR, "CashPayment");
                                                            items.Total = f.RTD(SubeR, "Total");
                                                            items.CreditPayment = f.RTD(SubeR, "CreditPayment");
                                                            items.TicketPayment = f.RTD(SubeR, "TicketPayment");
                                                            items.DebitPayment = f.RTD(SubeR, "DebitPayment");
                                                            items.IkramPayment = f.RTD(SubeR, "IkramPayment");
                                                            items.HediyeCeki = f.RTD(SubeR, "HediyeCeki");
                                                            Liste.Add(items);
                                                        }
                                                    }
                                                }
                                                else
                                                {
                                                    foreach (DataRow SubeR in AcikHesapDt.Rows)
                                                    {
                                                        PersonelCiro items = new PersonelCiro();
                                                        items.Sube = SubeAdi;
                                                        items.SubeID = (SubeId);
                                                        items.ReceivedByUserName = f.RTS(SubeR, "ReceivedByUserName");
                                                        items.CashPayment = f.RTD(SubeR, "CashPayment");
                                                        items.Total = f.RTD(SubeR, "Total");
                                                        items.CreditPayment = f.RTD(SubeR, "CreditPayment");
                                                        items.TicketPayment = f.RTD(SubeR, "TicketPayment");
                                                        Liste.Add(items);
                                                    }
                                                }
                                            }
                                        }
                                        else
                                        {
                                            PersonelCiro items = new PersonelCiro();
                                            items.Sube = SubeAdi + " (Data Yok) ";
                                            items.SubeID = (SubeId);
                                            items.PersonelAdi = "*";
                                            items.Debit = 0;
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
                                    PersonelCiro items = new PersonelCiro();
                                    //items.CustomerName = cu;
                                    //items.Debit = SubeId;
                                    items.Sube = ex.Message + " (Erişim Yok) ";
                                    items.SubeID = (SubeId);
                                    items.PersonelAdi = "*";
                                    items.Debit = 0;//f.RTD(SubeR, "TUTAR");
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

        public static PersonelCiro Print()
        {
            return null;
        }
    }
}