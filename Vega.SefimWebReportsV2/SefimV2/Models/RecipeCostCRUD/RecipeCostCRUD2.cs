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
    public class RecipeCostCRUD2
    {
        public static List<SubeUrun> List(DateTime Date1, DateTime Date2, string subeid, string ID, string payReportID, string SaatGun, int KirilimNo, bool tumStoklarGetirilsinMi)
        {
            List<SubeUrun> Liste = new List<SubeUrun>();
            ModelFunctions ff = new ModelFunctions();

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
                DataTable dt = ff.DataTable("select * from SubeSettings  " + filter + " order by SubeName");
                ff.SqlConnClose();

                #endregion SUBSTATION LIST

                #region VEGA DB database ismini çekmek için.

                string vega_Db = string.Empty;
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
                Parallel.ForEach(dtList, r =>
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
                    string query = string.Empty;

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
                        if (AppDbType == "1" || AppDbType == "2")
                        {
                            if (SaatGun == "0") // Masa Üstü Raporu, SaatGun Raporu 1.Kırılım
                            {
                                query = File.ReadAllText(HostingEnvironment.MapPath("/Sql/SqlReports/SaatGunMasaUstuReports/SaatGunMasaUstuReports.sql"), System.Text.UTF8Encoding.Default);
                            }
                            else if (SaatGun == "1") //  Masa Üstü Raporu, SaatGun Raporu 2.Kırılım
                            {
                                query = File.ReadAllText(HostingEnvironment.MapPath("/Sql/SqlReports/SaatGunMasaUstuReports/SaatGunMasaUstuReportsDetay.sql"), System.Text.UTF8Encoding.Default);
                            }
                            else if (SaatGun == "3") //  Masa Üstü Raporu,
                            {
                                query = File.ReadAllText(HostingEnvironment.MapPath("/Sql/SqlReports/GunSaatMasaUstuReports/GunSaatMasaUstuReports.sql"), System.Text.UTF8Encoding.Default);
                            }
                            else if (KirilimNo == 2 && tumStoklarGetirilsinMi) //Tüm stokları getirmek için güncellendi
                            {
                                //query =
                                //         " Declare @par1 nvarchar(20) = '{TARIH1}';" +
                                //         " Declare @par2 nvarchar(20) = '{TARIH2}';" +
                                //         " WITH " +
                                //         " pro as" +
                                //         " (select " +
                                //         " ISNULL(P.ProductName,'')+ISNULL('.'+ch1.Name,'')+ISNULL('.'+ch2.Name,'') pname," +
                                //         " ISNULL(P.Price,0)+ISNULL(ch1.Price,0)+ISNULL(ch2.Price,0) pprice," +
                                //         " ProductGroup" +
                                //         " from Product p" +
                                //         " left join Choice1 ch1 on ch1.ProductId=p.Id" +
                                //         " left join Choice2 ch2 on ch2.ProductId=p.Id and ch2.Choice1Id=ch1.Id" +
                                //         " )," +
                                //         " maliyet AS" +
                                //         "  (SELECT MALIYET," +
                                //         "          IND," +
                                //         "          STOKKODU HMKODU," +
                                //         "          MALINCINSI HMADI" +
                                //         "   FROM " + vega_Db + ".DBO.F0" + FirmaId + "TBLSTOKLAR), " +
                                //         "     recete AS" +
                                //         "  (SELECT [Quantity]," +
                                //         "          StokID," +
                                //         "          ProductId," +
                                //         "          ProductName" +
                                //         "   FROM [dbo].[Bom])," +
                                //         "     receteOptions AS" +
                                //         "  (SELECT sum(Quantity) bommiktar," +
                                //         "          StokID," +
                                //         "          ProductName," +
                                //         "          OptionsName" +
                                //         "   FROM [dbo].BomOptions" +
                                //         "   WHERE ISNULL(MaliyetDahil, 0)=1" +
                                //         "   GROUP BY StokID," +
                                //         "            ProductName," +
                                //         "            OptionsName)," +
                                //         "     ProEqu AS" +
                                //         "  (SELECT ISNULL(pq.ProductName, '') + ISNULL('.' +" +
                                //         "                                                (SELECT name" +
                                //         "                                                 FROM Choice1 ch1" +
                                //         "                                                 WHERE ch1.Id = pq.choice1Id ), '') + ISNULL('.' +" +
                                //         "                                                                                               (SELECT name" +
                                //         "                                                                                                FROM Choice2 ch2" +
                                //         "                                                                                                WHERE ch2.Id = pq.choice2Id ), '') ProductName ," +
                                //         "          Multiplier ," +
                                //         "          EquProductName" +
                                //         "   FROM ProductEqu Pq) ," +
                                //         "     optequ AS" +
                                //         "  (SELECT oq.ProductName ," +
                                //         "          (oq.ProductName + oq.Options) oqproname ," +
                                //         "          EquProduct ," +
                                //         "          miktar ," +
                                //         "          AnaUrun" +
                                //         "   FROM OptionsEqu oq)," +
                                //         "     Base AS" +
                                //         "  (SELECT B.ProductId ," +
                                //         "          B.ProductName ," +
                                //         "          P.ProductName PName ," +
                                //         "          B.Date PaymentTime ," +
                                //         "          B.Quantity ," +
                                //         "          b.Options ," +
                                //         "          CASE" +
                                //         "              WHEN CHARINDEX('x', LTRIM(RTRIM(O.s))) > 0 THEN SUBSTRING(LTRIM(RTRIM(O.s)), CHARINDEX('x', LTRIM(RTRIM(O.s))) + 1, 100)" +
                                //         "              ELSE LTRIM(RTRIM(O.s))" +
                                //         "          END AS Opt ," +
                                //         "          CASE" +
                                //         "              WHEN CHARINDEX('x', LTRIM(RTRIM(O.s))) > 0 THEN SUBSTRING(LTRIM(RTRIM(O.s)), 1, CHARINDEX('x', LTRIM(RTRIM(O.s))) - 1)" +
                                //         "              ELSE '1'" +
                                //         "          END AS OptQty ," +
                                //         "          B.HeaderId" +
                                //         "   FROM Bill B" +
                                //         "   LEFT JOIN Product P ON B.ProductId = P.Id CROSS APPLY dbo.SplitString(',', B.Options) AS O" +
                                //         "   WHERE ISNULL(B.Options, '') <> ''" +
                                //         "     AND B.Date BETWEEN @par1 AND @par2 )," +
                                //         "     BillPrice AS" +
                                //         "  (SELECT Bp.ProductName ," +
                                //         "          Bp.Price ," +
                                //         "          Bp.MinDate ," +
                                //         "          Bp.Options ," +
                                //         "          CASE" +
                                //         "              WHEN Bp.MaxDate=MAX(Bp.MaxDate) OVER(PARTITION BY Bp.ProductName) THEN GETDATE()" +
                                //         "              ELSE Bp.MaxDate" +
                                //         "          END MaxDate" +
                                //         "   FROM" +
                                //         "     (SELECT ProductName ," +
                                //         "             Price ," +
                                //         "             Options ," +
                                //         "             MIN(Date) MinDate ," +
                                //         "             MAX(Date) MaxDate" +
                                //         "      FROM Bill B" +
                                //         "      WHERE UserName='PAKET'" +
                                //         "      GROUP BY ProductName," +
                                //         "               Price," +
                                //         "               Options) Bp)," +
                                //         "     OptSatislar AS" +
                                //         "  (SELECT Oe.EquProduct ," +
                                //         "          Oe.Miktar ," +
                                //         "          B.ProductName ," +
                                //         "          B.HeaderId ," +
                                //         "          B.PaymentTime ," +
                                //         "          B.ProductId ," +
                                //         "          Quantity * (CASE" +
                                //         "                          WHEN ISNUMERIC(OptQty) = 1 THEN CAST(OptQty AS INT)" +
                                //         "                          ELSE 1" +
                                //         "                      END) AS Quantity ," +
                                //         "          Opt AS OptionsName ," +
                                //         "          B.ProductName aaa ," +
                                //         "          ISNULL(Oe.EquProduct, opt) ProductName2 ," +
                                //         "          CASE" +
                                //         "              WHEN ISNULL(Oe.MenuFiyat, 0)=0 THEN CASE" +
                                //         "                                                      WHEN oe.AnaUrun=1 THEN MAX(Bp.Price)" +
                                //         "                                                      ELSE ISNULL(Oe.MenuFiyat, 0)" +
                                //         "                                                  END" +
                                //         "              ELSE ISNULL(Oe.MenuFiyat, 0)" +
                                //         "          END MenuFiyat" +
                                //         "   FROM Base B" +
                                //         "   LEFT JOIN OptionsEqu Oe ON Oe.ProductName+Oe.Options=B.PName+ISNULL(B.Opt, '')" +
                                //         "   LEFT JOIN BillPrice Bp ON Oe.EquProduct=Bp.ProductName" +
                                //         "   AND B.PaymentTime BETWEEN Bp.MinDate AND Bp.MaxDate" +
                                //         "   WHERE opt not like '%istemiyorum%'" +
                                //         "     OR opt not like '%istiyorum%'" +
                                //         "   GROUP BY Oe.EquProduct ," +
                                //         "            Oe.Miktar ," +
                                //         "            oe.AnaUrun ," +
                                //         "            B.ProductName ," +
                                //         "            B.HeaderId ," +
                                //         "            B.PaymentTime ," +
                                //         "            B.ProductId ," +
                                //         "            Oe.MenuFiyat ," +
                                //         "            Quantity * (CASE" +
                                //         "                            WHEN ISNUMERIC(OptQty) = 1 THEN CAST(OptQty AS INT)" +
                                //         "                            ELSE 1" +
                                //         "                        END) ," +
                                //         "            Opt) ," +
                                //         "     opttotal AS" +
                                //         "  (SELECT optsatislar.ProductId ," +
                                //         "          ISNULL(EquProduct, optsatislar.ProductName) BillProductName ," +
                                //         "          ISNULL(EquProduct, optsatislar.OptionsName) ProductName ," +
                                //         "          '' ProductGroup ," +
                                //         "             '' InvoiceName ," +
                                //         "                sum(Quantity) OrjToplam ," +
                                //         "                sum(Quantity * ISNULL(Miktar, 1)) Toplam ," +
                                //         "                (sum(MenuFiyat)) * sum(Quantity) OrjTutar ," +
                                //         "                CASE" +
                                //         "                    WHEN sum(MenuFiyat) = 0" +
                                //         "                         AND SUM(ISNULL(Miktar, 0)) > 0 THEN 0" +
                                //         "                    ELSE sum(optsatislar.MenuFiyat*Quantity)" +
                                //         "                END Tutar ," +
                                //         "                sum(receteOptions.bommiktar) RECETEMIKTAR ," +
                                //         "                sum(Quantity * ISNULL(Miktar, 1))*sum(ISNULL(receteOptions.bommiktar, 0)) RECETEMIKTARTOPLAM ," +
                                //         "                sum(maliyet.MALIYET) HMMALIYET ," +
                                //         "                (SUM(optsatislar.Quantity*ISNULL(receteOptions.bommiktar, 0)))*sum(ISNULL(maliyet.MALIYET, 0)) RECETETUTAR" +
                                //         "   FROM optsatislar" +
                                //         "   LEFT" +
                                //         " JOIN receteOptions ON receteOptions.OptionsName=optsatislar.OptionsName" +
                                //         "   AND receteOptions.ProductName=OptSatislar.ProductName" +
                                //         "   LEFT" +
                                //         " JOIN maliyet ON maliyet.IND=receteOptions.StokID" +
                                //         "   GROUP BY EquProduct ," +
                                //         "            optsatislar.ProductName ," +
                                //         "            optsatislar.OptionsName ," +
                                //         "            optsatislar.ProductId) ," +
                                //         "     indirim AS" +
                                //         "  (SELECT TOP 9999999999 ISNULL(avg(ISNULL(b2.discount, 1)), 1) / (CASE" +
                                //         "                                                                       WHEN sum(Quantity * Price) = 0 THEN 1" +
                                //         "                                                                       ELSE sum(Quantity * Price)" +
                                //         "                                                                   END) indirimtutar ," +
                                //         "                         HeaderId indHeaderId" +
                                //         "   FROM PaidBill b2" +
                                //         "   WHERE PaymentTime >= @par1" +
                                //         "   GROUP BY HeaderId) ," +
                                //         "     paketindirim AS" +
                                //         "  (SELECT TOP 9999999 ISNULL(ph.discount, 0) /" +
                                //         "     (SELECT sum(Quantity * Price)" +
                                //         "      FROM bill b2" +
                                //         "      WHERE b2.HeaderId = ph.HeaderId ) pktindirim ," +
                                //         "                      HeaderId pktheaderId" +
                                //         "   FROM PhoneOrderHeader ph) ," +
                                //         "     satislar2 AS" +
                                //         "  (SELECT pq.EquProductName ," +
                                //         "          P.Id AS ProductId ," +
                                //         "          SUM(b.Quantity * ISNULL(pq.Multiplier, 1)) AS Toplam ," +
                                //         "          ISNULL(pq.EquProductName, B.ProductName) ProductName ," +
                                //         "          P.ProductGroup ," +
                                //         "          SUM((ISNULL(b.Quantity, 0) * CASE" +
                                //         "                                           WHEN ISNULL(pq.Multiplier, 1)=0 THEN 1" +
                                //         "                                           ELSE ISNULL(pq.Multiplier, 1)" +
                                //         "                                       END) * ISNULL(B.Price, 0)) AS Tutar ," +
                                //         "     (SELECT SUM(Bill.Price * Bill.Quantity)" +
                                //         "      FROM dbo.BillWithHeader AS Bill" +
                                //         "      WHERE BillState = 0" +
                                //         "        AND HeaderId = b.HeaderId" +
                                //         "        AND ProductId = b.ProductId ) AS OPENTABLE ," +
                                //         "     (SELECT SUM(Bill.Quantity)" +
                                //         "      FROM dbo.BillWithHeader AS Bill" +
                                //         "      WHERE BillState = 0" +
                                //         "        AND HeaderId = b.HeaderId" +
                                //         "        AND ProductId = b.ProductId ) AS OPENTABLEQuantity ," +
                                //         "          0 AS Discount ," +
                                //         "          SUM((ISNULL(b.Quantity, 0) * ISNULL(pq.Multiplier, 1)) * ISNULL(B.Price, 0)) * (avg(ind.indirimtutar)) INDIRIM ," +
                                //         "          SUM((ISNULL(b.Quantity, 0) * ISNULL(pq.Multiplier, 1)) * ISNULL(B.Price, 0)) * avg(pkt.pktindirim) PAKETINDIRIM ," +
                                //         "          CASE" +
                                //         "              WHEN sum(ISNULL(B.Price, 0)) = 0 THEN sum((ISNULL(B.OriginalPrice, 0)))" +
                                //         "              ELSE 0" +
                                //         "          END IKRAM ," +
                                //         "          (recete.Quantity) RECETEMIKTAR ," +
                                //         "          SUM(b.Quantity*ISNULL(recete.Quantity, 0)) RECETEMIKTARTOPLAM ," +
                                //         "          maliyet.MALIYET HMMALIYET ," +
                                //         "          (SUM(b.Quantity*ISNULL(recete.Quantity, 0)))*(ISNULL(maliyet.MALIYET, 0)) RECETETUTAR" +
                                //         "   FROM dbo.Bill AS b WITH (NOLOCK)" +
                                //         "   LEFT JOIN proequ pq ON pq.ProductName = B.ProductName" +
                                //         "   LEFT JOIN dbo.Product AS P ON P.Id = B.ProductId" +
                                //         "   LEFT JOIN indirim AS ind ON ind.indHeaderId = b.HeaderId" +
                                //         "   LEFT JOIN paketindirim AS pkt ON pkt.pktheaderId = b.HeaderId" +
                                //         "   LEFT JOIN recete ON recete.ProductId=b.ProductId" +
                                //         "   AND recete.ProductName=b.ProductName" +
                                //         "   LEFT JOIN maliyet ON maliyet.IND=recete.StokID" +
                                //         "   WHERE [Date] >= @par1" +
                                //         "     AND [Date] <= @par2" +
                                //         "     AND P.ProductName NOT LIKE '$%'" +
                                //         "   GROUP BY pq.EquProductName ," +
                                //         "            B.ProductName ," +
                                //         "            b.ProductId ," +
                                //         "            P.ProductGroup ," +
                                //         "            B.HeaderId ," +
                                //         "            p.Id ," +
                                //         "            recete.Quantity ," +
                                //         "            b.Quantity," +
                                //         "            maliyet.MALIYET)," +
                                //         " satisproduct as (" +
                                //         " SELECT sum(MIKTAR) MIKTAR ," +
                                //         "       a.ProductName ," +
                                //         "       Sum(TUTAR) TUTAR ," +
                                //         "       SUM(INDIRIM) INDIRIM ," +
                                //         "       SUM(IKRAM) IKRAM," +
                                //         "       SUM(RECETEMIKTARTOPLAM) RECETEMIKTARTOPLAM," +
                                //         "       SUM(RECETETUTAR) RECETETUTAR" +
                                //         " FROM" +
                                //         "  (SELECT CASE" +
                                //         "              WHEN sum(opttotal.Toplam) = 0 THEN sum(opttotal.OrjToplam)" +
                                //         "              ELSE sum(opttotal.Toplam)" +
                                //         "          END AS MIKTAR ," +
                                //         "          opttotal.ProductName ," +
                                //         "          ProductGroup AS ProductGroup ," +
                                //         "          sum(ISNULL(opttotal.Tutar, 0)) AS TUTAR ," +
                                //         "          0 INDIRIM ," +
                                //         "          0 IKRAM," +
                                //         "          SUM(RECETEMIKTARTOPLAM) RECETEMIKTARTOPLAM," +
                                //         "          SUM(RECETETUTAR) RECETETUTAR" +
                                //         "   FROM opttotal" +
                                //         "   GROUP BY opttotal.ProductName ," +
                                //         "            opttotal.ProductGroup" +
                                //         "   UNION SELECT sum(toplam) AS MIKTAR ," +
                                //         "                satislar2.ProductName ," +
                                //         "                satislar2.ProductGroup ," +
                                //         "                sum(tutar) AS TUTAR ," +
                                //         "                SUM(ISNULL(INDIRIM, 0)) + SUM(ISNULL(PAKETINDIRIM, 0)) INDIRIM ," +
                                //         "                SUM(IKRAM) IKRAM," +
                                //         "                SUM(RECETEMIKTARTOPLAM) RECETEMIKTARTOPLAM," +
                                //         "                SUM(RECETETUTAR) RECETETUTAR" +
                                //         "   FROM satislar2" +
                                //         "   GROUP BY satislar2.ProductName ," +
                                //         "            satislar2.ProductGroup ," +
                                //         "            satislar2.ProductId) AS a" +
                                //         " WHERE MIKTAR<>0" +
                                //         " GROUP BY a.ProductName" +
                                //         " )" +
                                //         " SELECT ISNULL(satisproduct.MIKTAR, 0) MIKTAR," +
                                //         " pro.pname ProductName," +
                                //         " ISNULL(satisproduct.TUTAR, 0) TUTAR," +
                                //         " ISNULL(satisproduct.INDIRIM, 0) INDIRIM," +
                                //         " ISNULL(satisproduct.IKRAM, 0) IKRAM," +
                                //         " ISNULL(recete.Quantity, 0) RECETEMIKTARTOPLAM," +
                                //         " ISNULL(recete.Quantity, 0) * ISNULL(maliyet.MALIYET, 0) RECETETUTAR" +
                                //         " FROM recete" +
                                //         " LEFT JOIN pro ON recete.ProductName = pname" +
                                //         " LEFT JOIN satisproduct ON satisproduct.ProductName = recete.ProductName" +
                                //         " LEFT JOIN maliyet ON maliyet.IND = recete.StokID" +
                                //         " where pro.pname is not null ";

                                //query = " Declare @par1 nvarchar(20) = '{TARIH1}';" +
                                //        " Declare @par2 nvarchar(20) = '{TARIH2}';" +
                                //        " WITH pro as" +
                                //        "  (select ISNULL(P.ProductName, '')+ISNULL('.'+ch1.Name, '')+ISNULL('.'+ch2.Name, '') pname," +
                                //        "          ISNULL(P.Price, 0)+ISNULL(ch1.Price, 0)+ISNULL(ch2.Price, 0) pprice," +
                                //        "          ProductGroup" +
                                //        "   from Product p" +
                                //        "   left join Choice1 ch1 on ch1.ProductId=p.Id" +
                                //        "   left join Choice2 ch2 on ch2.ProductId=p.Id" +
                                //        "   and ch2.Choice1Id=ch1.Id)," +
                                //        "     maliyet AS" +
                                //        "  (SELECT MALIYET," +
                                //        "          IND," +
                                //        "          STOKKODU HMKODU," +
                                //        "          MALINCINSI HMADI" +
                                //        "   FROM " + vega_Db + ".DBO.F0" + FirmaId + "TBLSTOKLAR), " +
                                //        "     recete AS" +
                                //        "  (SELECT [Quantity]," +
                                //        "          StokID," +
                                //        "          ProductId," +
                                //        "          ProductName" +
                                //        "   FROM [dbo].[Bom])," +
                                //        "     receteOptions AS" +
                                //        "  (SELECT sum(Quantity) bommiktar," +
                                //        "          StokID," +
                                //        "          ProductName," +
                                //        "          OptionsName" +
                                //        "   FROM [dbo].BomOptions" +
                                //        "   WHERE ISNULL(MaliyetDahil, 0)=1" +
                                //        "   GROUP BY StokID," +
                                //        "            ProductName," +
                                //        "            OptionsName)," +
                                //        "     ProEqu AS" +
                                //        "  (SELECT ISNULL(pq.ProductName, '') + ISNULL('.' +" +
                                //        "                                                (SELECT name" +
                                //        "                                                 FROM Choice1 ch1" +
                                //        "                                                 WHERE ch1.Id = pq.choice1Id ), '') + ISNULL('.' +" +
                                //        "                                                                                               (SELECT name" +
                                //        "                                                                                                FROM Choice2 ch2" +
                                //        "                                                                                                WHERE ch2.Id = pq.choice2Id ), '') ProductName," +
                                //        "          Multiplier," +
                                //        "          EquProductName" +
                                //        "   FROM ProductEqu Pq)," +
                                //        "     optequ AS" +
                                //        "  (SELECT oq.ProductName," +
                                //        "          (oq.ProductName + oq.Options) oqproname," +
                                //        "          EquProduct," +
                                //        "          miktar," +
                                //        "          AnaUrun" +
                                //        "   FROM OptionsEqu oq)," +
                                //        "     Base AS" +
                                //        "  (SELECT B.ProductId," +
                                //        "          B.ProductName," +
                                //        "          P.ProductName PName," +
                                //        "          B.Date PaymentTime," +
                                //        "          B.Quantity," +
                                //        "          b.Options," +
                                //        "          CASE" +
                                //        "              WHEN CHARINDEX('x', LTRIM(RTRIM(O.s))) > 0 THEN SUBSTRING(LTRIM(RTRIM(O.s)), CHARINDEX('x', LTRIM(RTRIM(O.s))) + 1, 100)" +
                                //        "              ELSE LTRIM(RTRIM(O.s))" +
                                //        "          END AS Opt," +
                                //        "          CASE" +
                                //        "              WHEN CHARINDEX('x', LTRIM(RTRIM(O.s))) > 0 THEN SUBSTRING(LTRIM(RTRIM(O.s)), 1, CHARINDEX('x', LTRIM(RTRIM(O.s))) - 1)" +
                                //        "              ELSE '1'" +
                                //        "          END AS OptQty," +
                                //        "          B.HeaderId" +
                                //        "   FROM Bill B" +
                                //        "   LEFT JOIN Product P ON B.ProductId = P.Id CROSS APPLY dbo.SplitString(',', B.Options) AS O" +
                                //        "   WHERE ISNULL(B.Options, '') <> ''" +
                                //        "     AND B.Date BETWEEN @par1 AND @par2 )," +
                                //        "     BillPrice AS" +
                                //        "  (SELECT Bp.ProductName," +
                                //        "          Bp.Price," +
                                //        "          Bp.MinDate," +
                                //        "          Bp.Options," +
                                //        "          CASE" +
                                //        "              WHEN Bp.MaxDate=MAX(Bp.MaxDate) OVER(PARTITION BY Bp.ProductName) THEN GETDATE()" +
                                //        "              ELSE Bp.MaxDate" +
                                //        "          END MaxDate" +
                                //        "   FROM" +
                                //        "     (SELECT ProductName," +
                                //        "             Price," +
                                //        "             Options," +
                                //        "             MIN(Date) MinDate," +
                                //        "             MAX(Date) MaxDate" +
                                //        "      FROM Bill B" +
                                //        "      WHERE UserName='PAKET'" +
                                //        "      GROUP BY ProductName," +
                                //        "               Price," +
                                //        "               Options) Bp)," +
                                //        "     OptSatislar AS" +
                                //        "  (SELECT Oe.EquProduct," +
                                //        "          Oe.Miktar," +
                                //        "          B.ProductName ," +
                                //        "          B.HeaderId," +
                                //        "          B.PaymentTime," +
                                //        "          B.ProductId," +
                                //        "          Quantity * (CASE" +
                                //        "                          WHEN ISNUMERIC(OptQty) = 1 THEN CAST(OptQty AS INT)" +
                                //        "                          ELSE 1" +
                                //        "                      END) AS Quantity," +
                                //        "          Opt AS OptionsName," +
                                //        "          B.ProductName aaa," +
                                //        "          ISNULL(Oe.EquProduct, opt) ProductName2," +
                                //        "          CASE" +
                                //        "              WHEN ISNULL(Oe.MenuFiyat, 0)=0 THEN CASE" +
                                //        "                                                      WHEN oe.AnaUrun=1 THEN MAX(Bp.Price)" +
                                //        "                                                      ELSE ISNULL(Oe.MenuFiyat, 0)" +
                                //        "                                                  END" +
                                //        "              ELSE ISNULL(Oe.MenuFiyat, 0)" +
                                //        "          END MenuFiyat" +
                                //        "   FROM Base B" +
                                //        "   LEFT JOIN OptionsEqu Oe ON Oe.ProductName+Oe.Options=B.PName+ISNULL(B.Opt, '')" +
                                //        "   LEFT JOIN BillPrice Bp ON Oe.EquProduct=Bp.ProductName" +
                                //        "   AND B.PaymentTime BETWEEN Bp.MinDate AND Bp.MaxDate" +
                                //        "   WHERE opt not like '%istemiyorum%'" +
                                //        "     OR opt not like '%istiyorum%'" +
                                //        "   GROUP BY Oe.EquProduct," +
                                //        "            Oe.Miktar," +
                                //        "            oe.AnaUrun," +
                                //        "            B.ProductName ," +
                                //        "            B.HeaderId," +
                                //        "            B.PaymentTime," +
                                //        "            B.ProductId," +
                                //        "            Oe.MenuFiyat," +
                                //        "            Quantity * (CASE" +
                                //        "                            WHEN ISNUMERIC(OptQty) = 1 THEN CAST(OptQty AS INT)" +
                                //        "                            ELSE 1" +
                                //        "                        END)," +
                                //        "            Opt)," +
                                //        "     opttotal AS" +
                                //        "  (SELECT optsatislar.ProductId," +
                                //        "          ISNULL(EquProduct, optsatislar.ProductName) BillProductName," +
                                //        "          ISNULL(EquProduct, optsatislar.OptionsName) ProductName," +
                                //        "          '' ProductGroup," +
                                //        "             '' InvoiceName," +
                                //        "                sum(Quantity) OrjToplam," +
                                //        "                sum(Quantity * ISNULL(Miktar, 1)) Toplam," +
                                //        "                (sum(MenuFiyat)) * sum(Quantity) OrjTutar," +
                                //        "                CASE" +
                                //        "                    WHEN sum(MenuFiyat) = 0" +
                                //        "                         AND SUM(ISNULL(Miktar, 0)) > 0 THEN 0" +
                                //        "                    ELSE sum(optsatislar.MenuFiyat*Quantity)" +
                                //        "                END Tutar," +
                                //        "                sum(receteOptions.bommiktar) RECETEMIKTAR," +
                                //        "                sum(Quantity * ISNULL(Miktar, 1))*sum(ISNULL(receteOptions.bommiktar, 0)) RECETEMIKTARTOPLAM," +
                                //        "                sum(maliyet.MALIYET) HMMALIYET," +
                                //        "                (SUM(optsatislar.Quantity*ISNULL(receteOptions.bommiktar, 0)))*sum(ISNULL(maliyet.MALIYET, 0)) RECETETUTAR" +
                                //        "   FROM optsatislar" +
                                //        "   LEFT JOIN receteOptions ON receteOptions.OptionsName=optsatislar.OptionsName" +
                                //        "   AND receteOptions.ProductName=OptSatislar.ProductName" +
                                //        "   LEFT JOIN maliyet ON maliyet.IND=receteOptions.StokID" +
                                //        "   GROUP BY EquProduct," +
                                //        "            optsatislar.ProductName," +
                                //        "            optsatislar.OptionsName," +
                                //        "            optsatislar.ProductId)," +
                                //        "     indirim AS" +
                                //        "  (SELECT TOP 9999999999 ISNULL(avg(ISNULL(b2.discount, 1)), 1) / (CASE" +
                                //        "                                                                       WHEN sum(Quantity * Price) = 0 THEN 1" +
                                //        "                                                                       ELSE sum(Quantity * Price)" +
                                //        "                                                                   END) indirimtutar," +
                                //        "                         HeaderId indHeaderId" +
                                //        "   FROM PaidBill b2" +
                                //        "   WHERE PaymentTime >= @par1" +
                                //        "   GROUP BY HeaderId)," +
                                //        "     paketindirim AS" +
                                //        "  (SELECT TOP 9999999 ISNULL(ph.discount, 0) /" +
                                //        "     (SELECT sum(Quantity * Price)" +
                                //        "      FROM bill b2" +
                                //        "      WHERE b2.HeaderId = ph.HeaderId ) pktindirim," +
                                //        "                      HeaderId pktheaderId" +
                                //        "   FROM PhoneOrderHeader ph)," +
                                //        "     satislar2 AS" +
                                //        "  (SELECT pq.EquProductName," +
                                //        "          P.Id AS ProductId," +
                                //        "          (b.Quantity * ISNULL(pq.Multiplier, 1)) AS Toplam," +
                                //        "          ISNULL(pq.EquProductName, B.ProductName) ProductName," +
                                //        "          P.ProductGroup," +
                                //        "          avg((ISNULL(b.Quantity, 0) * CASE" +
                                //        "                                           WHEN ISNULL(pq.Multiplier, 1)=0 THEN 1" +
                                //        "                                           ELSE ISNULL(pq.Multiplier, 1)" +
                                //        "                                       END) * ISNULL(B.Price, 0)) AS Tutar," +
                                //        "     (SELECT SUM(Bill.Price * Bill.Quantity)" +
                                //        "      FROM dbo.BillWithHeader AS Bill" +
                                //        "      WHERE BillState = 0" +
                                //        "        AND HeaderId = b.HeaderId" +
                                //        "        AND ProductId = b.ProductId ) AS OPENTABLE," +
                                //        "     (SELECT SUM(Bill.Quantity)" +
                                //        "      FROM dbo.BillWithHeader AS Bill" +
                                //        "      WHERE BillState = 0" +
                                //        "        AND HeaderId = b.HeaderId" +
                                //        "        AND ProductId = b.ProductId ) AS OPENTABLEQuantity," +
                                //        "          0 AS Discount," +
                                //        "          SUM((ISNULL(b.Quantity, 0) * ISNULL(pq.Multiplier, 1)) * ISNULL(B.Price, 0)) * (avg(ind.indirimtutar)) INDIRIM," +
                                //        "          SUM((ISNULL(b.Quantity, 0) * ISNULL(pq.Multiplier, 1)) * ISNULL(B.Price, 0)) * avg(pkt.pktindirim) PAKETINDIRIM," +
                                //        "          CASE" +
                                //        "              WHEN sum(ISNULL(B.Price, 0)) = 0 THEN sum((ISNULL(B.OriginalPrice, 0)))" +
                                //        "              ELSE 0" +
                                //        "          END IKRAM ," +
                                //        "          (recete.Quantity) RECETEMIKTAR," +
                                //        "          SUM(b.Quantity*ISNULL(recete.Quantity, 0)) RECETEMIKTARTOPLAM," +
                                //        "          maliyet.MALIYET HMMALIYET," +
                                //        "          (SUM(b.Quantity*ISNULL(recete.Quantity, 0)))*(ISNULL(maliyet.MALIYET, 0)) RECETETUTAR" +
                                //        "   FROM dbo.Bill AS b WITH (NOLOCK)" +
                                //        "   LEFT JOIN proequ pq ON pq.ProductName = B.ProductName" +
                                //        "   LEFT JOIN dbo.Product AS P ON P.Id = B.ProductId" +
                                //        "   LEFT JOIN indirim AS ind ON ind.indHeaderId = b.HeaderId" +
                                //        "   LEFT JOIN paketindirim AS pkt ON pkt.pktheaderId = b.HeaderId" +
                                //        "   LEFT JOIN recete ON recete.ProductId=b.ProductId" +
                                //        "   AND recete.ProductName=b.ProductName" +
                                //        "   LEFT JOIN maliyet ON maliyet.IND=recete.StokID" +
                                //        "   WHERE [Date] >= @par1" +
                                //        "     AND [Date] <= @par2" +
                                //        "     AND P.ProductName NOT LIKE '$%'" +
                                //        "   GROUP BY pq.EquProductName," +
                                //        "            B.ProductName," +
                                //        "            b.ProductId," +
                                //        "            P.ProductGroup," +
                                //        "            B.HeaderId," +
                                //        "            p.Id," +
                                //        "            recete.Quantity," +
                                //        "			pq.Multiplier," +
                                //        "            b.Quantity," +
                                //        "            maliyet.MALIYET)," +
                                //        "            satisproduct as" +
                                //        "            (SELECT sum(MIKTAR) MIKTAR," +
                                //        "          a.ProductName," +
                                //        "          Sum(TUTAR) TUTAR," +
                                //        "          SUM(INDIRIM) INDIRIM," +
                                //        "          SUM(IKRAM) IKRAM," +
                                //        "          SUM(RECETEMIKTARTOPLAM) RECETEMIKTARTOPLAM," +
                                //        "          SUM(RECETETUTAR) RECETETUTAR" +
                                //        "   FROM" +
                                //        "     (SELECT CASE" +
                                //        "                 WHEN sum(opttotal.Toplam) = 0 THEN sum(opttotal.OrjToplam)" +
                                //        "                 ELSE sum(opttotal.Toplam)" +
                                //        "             END AS MIKTAR," +
                                //        "             opttotal.ProductName," +
                                //        "             ProductGroup AS ProductGroup," +
                                //        "             sum(ISNULL(opttotal.Tutar, 0)) AS TUTAR," +
                                //        "             0 INDIRIM," +
                                //        "             0 IKRAM," +
                                //        "             SUM(RECETEMIKTARTOPLAM) RECETEMIKTARTOPLAM," +
                                //        "             SUM(RECETETUTAR) RECETETUTAR" +
                                //        "      FROM opttotal" +
                                //        "      GROUP BY opttotal.ProductName," +
                                //        "               opttotal.ProductGroup" +
                                //        "      UNION SELECT AVG(toplam) AS MIKTAR," +
                                //        "                   satislar2.ProductName," +
                                //        "                   satislar2.ProductGroup," +
                                //        "                   avg(tutar) AS TUTAR," +
                                //        "                   SUM(ISNULL(INDIRIM, 0)) + SUM(ISNULL(PAKETINDIRIM, 0)) INDIRIM," +
                                //        "                   SUM(IKRAM) IKRAM," +
                                //        "                   SUM(RECETEMIKTARTOPLAM) RECETEMIKTARTOPLAM," +
                                //        "                   SUM(RECETETUTAR) RECETETUTAR" +
                                //        "      FROM satislar2" +
                                //        "      GROUP BY satislar2.ProductName," +
                                //        "               satislar2.ProductGroup," +
                                //        "               satislar2.ProductId) AS a" +
                                //        "   WHERE MIKTAR<>0" +
                                //        "   GROUP BY a.ProductName)" +
                                //        " SELECT AVG(ISNULL(satisproduct.MIKTAR, 0)) MIKTAR," +
                                //        "       pro.pname ProductName," +
                                //        "       AVG(ISNULL(satisproduct.TUTAR, 0)) TUTAR," +
                                //        "       SUM(ISNULL(satisproduct.INDIRIM, 0)) INDIRIM," +
                                //        "       SUM(ISNULL(satisproduct.IKRAM, 0)) IKRAM," +
                                //        "       SUM(ISNULL(recete.Quantity, 0)) RECETEMIKTARTOPLAM," +
                                //        "       SUM(ISNULL(recete.Quantity, 0) * ISNULL(maliyet.MALIYET, 0)) RECETETUTAR" +
                                //        " FROM recete" +
                                //        " LEFT JOIN pro ON recete.ProductName = pname" +
                                //        " LEFT JOIN satisproduct ON satisproduct.ProductName = recete.ProductName" +
                                //        " LEFT JOIN maliyet ON maliyet.IND = recete.StokID" +
                                //        " where pro.pname is not null" +
                                //        " GROUP BY pro.pname";
                                query =
                               " Declare @par1 nvarchar(20) = '{TARIH1}';" +
                               " Declare @par2 nvarchar(20) = '{TARIH2}';" +
                               " WITH pro as" +
                               "  (select ISNULL(P.ProductName, '')+ISNULL('.'+ch1.Name, '')+ISNULL('.'+ch2.Name, '') pname," +
                               "          ISNULL(P.Price, 0)+ISNULL(ch1.Price, 0)+ISNULL(ch2.Price, 0) pprice," +
                               "          ProductGroup" +
                               "   from Product p" +
                               "   left join Choice1 ch1 on ch1.ProductId=p.Id" +
                               "   left join Choice2 ch2 on ch2.ProductId=p.Id" +
                               "   and ch2.Choice1Id=ch1.Id)," +
                               "     maliyet AS" +
                               "  (SELECT MALIYET," +
                               "          IND," +
                               "          STOKKODU HMKODU," +
                               "          MALINCINSI HMADI" +
                               "   FROM " + vega_Db + ".DBO.F0" + FirmaId + "TBLSTOKLAR), " +
                               "  " +
                               "     ProEqu AS" +
                               "  (SELECT ISNULL(pq.ProductName, '') + ISNULL('.' +" +
                               "                                                (SELECT name" +
                               "                                                 FROM Choice1 ch1" +
                               "                                                 WHERE ch1.Id = pq.choice1Id ), '') + ISNULL('.' +" +
                               "                                                                                               (SELECT name" +
                               "                                                                                                FROM Choice2 ch2" +
                               "                                                                                                WHERE ch2.Id = pq.choice2Id ), '') ProductName," +
                               "          Multiplier," +
                               "          EquProductName" +
                               "   FROM ProductEqu Pq)," +
                               "     optequ AS" +
                               "  (SELECT oq.ProductName," +
                               "          (oq.ProductName + oq.Options) oqproname," +
                               "          EquProduct," +
                               "          miktar," +
                               "          AnaUrun" +
                               "   FROM OptionsEqu oq)," +
                               "     Base AS" +
                               "  (SELECT B.ProductId," +
                               "          B.ProductName," +
                               "          P.ProductName PName," +
                               "          B.Date PaymentTime," +
                               "          B.Quantity," +
                               "          b.Options," +
                               "          CASE" +
                               "              WHEN CHARINDEX('x', LTRIM(RTRIM(O.s))) > 0 THEN SUBSTRING(LTRIM(RTRIM(O.s)), CHARINDEX('x', LTRIM(RTRIM(O.s))) + 1, 100)" +
                               "              ELSE LTRIM(RTRIM(O.s))" +
                               "          END AS Opt," +
                               "          CASE" +
                               "              WHEN CHARINDEX('x', LTRIM(RTRIM(O.s))) > 0 THEN SUBSTRING(LTRIM(RTRIM(O.s)), 1, CHARINDEX('x', LTRIM(RTRIM(O.s))) - 1)" +
                               "              ELSE '1'" +
                               "          END AS OptQty," +
                               "          B.HeaderId" +
                               "   FROM Bill B" +
                               "   LEFT JOIN Product P ON B.ProductId = P.Id CROSS APPLY dbo.SplitString(',', B.Options) AS O" +
                               "   WHERE ISNULL(B.Options, '') <> ''" +
                               "     AND B.Date BETWEEN @par1 AND @par2 )," +
                               "     BillPrice AS" +
                               "  (SELECT Bp.ProductName," +
                               "          Bp.Price," +
                               "          Bp.MinDate," +
                               "          Bp.Options," +
                               "          CASE" +
                               "              WHEN Bp.MaxDate=MAX(Bp.MaxDate) OVER(PARTITION BY Bp.ProductName) THEN GETDATE()" +
                               "              ELSE Bp.MaxDate" +
                               "          END MaxDate" +
                               "   FROM" +
                               "     (SELECT ProductName," +
                               "             Price," +
                               "             Options," +
                               "             MIN(Date) MinDate," +
                               "             MAX(Date) MaxDate" +
                               "      FROM Bill B" +
                               "      WHERE UserName='PAKET'" +
                               "      GROUP BY ProductName," +
                               "               Price," +
                               "               Options) Bp)," +
                               "     OptSatislar AS " +
                               "  (SELECT Oe.EquProduct," +
                               "          Oe.Miktar," +
                               "          B.ProductName," +
                               "          B.HeaderId," +
                               "          B.PaymentTime," +
                               "          B.ProductId," +
                               "          Quantity * (CASE" +
                               "                          WHEN ISNUMERIC(OptQty) = 1 THEN CAST(OptQty AS INT)" +
                               "                          ELSE 1" +
                               "                      END) AS Quantity," +
                               "          Opt AS OptionsName," +
                               "          B.ProductName aaa," +
                               "          ISNULL(Oe.EquProduct, opt) ProductName2," +
                               "          CASE" +
                               "              WHEN ISNULL(Oe.MenuFiyat, 0)=0 THEN CASE" +
                               "                                                      WHEN oe.AnaUrun=1 THEN MAX(Bp.Price)" +
                               "                                                      ELSE ISNULL(Oe.MenuFiyat, 0)" +
                               "                                                  END" +
                               "              ELSE ISNULL(Oe.MenuFiyat, 0)" +
                               "          END MenuFiyat" +
                               "   FROM Base B" +
                               "   LEFT JOIN OptionsEqu Oe ON Oe.ProductName+Oe.Options=B.PName+ISNULL(B.Opt, '')" +
                               "   LEFT JOIN BillPrice Bp ON Oe.EquProduct=Bp.ProductName" +
                               "   AND B.PaymentTime BETWEEN Bp.MinDate AND Bp.MaxDate" +
                               "   WHERE opt not like '%istemiyorum%'" +
                               "     OR opt not like '%istiyorum%'" +
                               "   GROUP BY Oe.EquProduct," +
                               "            Oe.Miktar," +
                               "            oe.AnaUrun," +
                               "            B.ProductName," +
                               "            B.HeaderId," +
                               "            B.PaymentTime," +
                               "            B.ProductId," +
                               "            Oe.MenuFiyat," +
                               "            Quantity * (CASE" +
                               "                            WHEN ISNUMERIC(OptQty) = 1 THEN CAST(OptQty AS INT)" +
                               "                            ELSE 1" +
                               "                        END)," +
                               "            Opt)," +
                               "     opttotal AS" +
                               "  (SELECT optsatislar.ProductId," +
                               "          ISNULL(EquProduct, optsatislar.ProductName) BillProductName," +
                               "          ISNULL(EquProduct, optsatislar.OptionsName) ProductName," +
                               "          '' ProductGroup," +
                               "             '' InvoiceName," +
                               "                sum(Quantity) OrjToplam," +
                               "                sum(Quantity * ISNULL(Miktar, 1)) Toplam," +
                               "                (sum(MenuFiyat)) * sum(Quantity) OrjTutar," +
                               "                CASE" +
                               "                    WHEN sum(MenuFiyat) = 0" +
                               "                         AND SUM(ISNULL(Miktar, 0)) > 0 THEN 0" +
                               "                    ELSE sum(optsatislar.MenuFiyat*Quantity)" +
                               "                END Tutar" +
                               "   FROM optsatislar" +
                               "   GROUP BY EquProduct," +
                               "            optsatislar.ProductName," +
                               "            optsatislar.OptionsName," +
                               "            optsatislar.ProductId)," +
                               "     indirim AS" +
                               "  (SELECT TOP 9999999999 ISNULL(avg(ISNULL(b2.discount, 1)), 1) / (CASE" +
                               "                                                                       WHEN sum(Quantity * Price) = 0 THEN 1" +
                               "                                                                       ELSE sum(Quantity * Price)" +
                               "                                                                   END) indirimtutar," +
                               "                         HeaderId indHeaderId" +
                               "   FROM PaidBill b2" +
                               "   WHERE PaymentTime >= @par1" +
                               "   GROUP BY HeaderId)," +
                               "     paketindirim AS" +
                               "  (SELECT TOP 9999999 ISNULL(ph.discount, 0) /" +
                               "     (SELECT sum(Quantity * Price)" +
                               "      FROM bill b2" +
                               "      WHERE b2.HeaderId = ph.HeaderId ) pktindirim," +
                               "                      HeaderId pktheaderId" +
                               "   FROM PhoneOrderHeader ph)," +
                               " satislar2 AS" +
                               "  (SELECT pq.EquProductName," +
                               "          P.Id AS ProductId," +
                               "          SUM(b.Quantity * ISNULL(pq.Multiplier, 1)) AS Toplam," +
                               "          ISNULL(pq.EquProductName, B.ProductName) ProductName," +
                               "          P.ProductGroup," +
                               "          sum((ISNULL(b.Quantity, 0) * CASE" +
                               "                                           WHEN ISNULL(pq.Multiplier, 1)=0 THEN 1" +
                               "                                           ELSE ISNULL(pq.Multiplier, 1)" +
                               "                                       END) * ISNULL(B.Price, 0)) AS Tutar," +
                               "     (SELECT SUM(Bill.Price * Bill.Quantity)" +
                               "      FROM dbo.BillWithHeader AS Bill" +
                               "      WHERE BillState = 0" +
                               "        AND HeaderId = b.HeaderId" +
                               "        AND ProductId = b.ProductId ) AS OPENTABLE," +
                               "     (SELECT SUM(Bill.Quantity)" +
                               "      FROM dbo.BillWithHeader AS Bill" +
                               "      WHERE BillState = 0" +
                               "        AND HeaderId = b.HeaderId" +
                               "        AND ProductId = b.ProductId ) AS OPENTABLEQuantity," +
                               "          0 AS Discount," +
                               "          SUM((ISNULL(b.Quantity, 0) * ISNULL(pq.Multiplier, 1)) * ISNULL(B.Price, 0)) * (avg(ind.indirimtutar)) INDIRIM," +
                               "          SUM((ISNULL(b.Quantity, 0) * ISNULL(pq.Multiplier, 1)) * ISNULL(B.Price, 0)) * avg(pkt.pktindirim) PAKETINDIRIM," +
                               "          CASE" +
                               "              WHEN sum(ISNULL(B.Price, 0)) = 0 THEN sum((ISNULL(B.OriginalPrice, 0)))" +
                               "              ELSE 0" +
                               "          END IKRAM " +
                               "   FROM dbo.Bill AS b WITH (NOLOCK)" +
                               "   LEFT JOIN proequ pq ON pq.ProductName = B.ProductName" +
                               "   LEFT JOIN dbo.Product AS P ON P.Id = B.ProductId" +
                               "   LEFT JOIN indirim AS ind ON ind.indHeaderId = b.HeaderId" +
                               "   LEFT JOIN paketindirim AS pkt ON pkt.pktheaderId = b.HeaderId" +
                               "   WHERE [Date] >= @par1" +
                               "     AND [Date] <= @par2" +
                               "     AND P.ProductName NOT LIKE '$%'" +
                               "   GROUP BY pq.EquProductName," +
                               "            B.ProductName," +
                               "            b.ProductId," +
                               "            P.ProductGroup," +
                               "            B.HeaderId," +
                               "            p.Id," +
                               "            pq.Multiplier)," +
                               "     satisproduct as" +
                               "  (SELECT sum(MIKTAR) MIKTAR," +
                               "          a.ProductName," +
                               "          Sum(TUTAR) TUTAR," +
                               "          SUM(INDIRIM) INDIRIM," +
                               "          SUM(IKRAM) IKRAM" +
                               "   FROM" +
                               "     (SELECT CASE" +
                               "                 WHEN sum(opttotal.Toplam) = 0 THEN sum(opttotal.OrjToplam)" +
                               "                 ELSE sum(opttotal.Toplam)" +
                               "             END AS MIKTAR," +
                               "             opttotal.ProductName," +
                               "             ProductGroup AS ProductGroup," +
                               "             sum(ISNULL(opttotal.Tutar, 0)) AS TUTAR," +
                               "             0 INDIRIM," +
                               "             0 IKRAM" +
                               "      FROM opttotal" +
                               "      GROUP BY opttotal.ProductName," +
                               "               opttotal.ProductGroup" +
                               "      UNION SELECT sum(toplam) AS MIKTAR," +
                               "                   satislar2.ProductName," +
                               "                   satislar2.ProductGroup," +
                               "                   sum(tutar) AS TUTAR," +
                               "                   SUM(ISNULL(INDIRIM, 0)) + SUM(ISNULL(PAKETINDIRIM, 0)) INDIRIM," +
                               "                   SUM(IKRAM) IKRAM" +
                               "      FROM satislar2" +
                               "      GROUP BY satislar2.ProductName," +
                               "               satislar2.ProductGroup," +
                               "               satislar2.ProductId) AS a" +
                               "   WHERE MIKTAR<>0" +
                               "   GROUP BY a.ProductName)," +
                               "      recete AS" +
                               "  (SELECT [Quantity] RQuantity ," +
                               "          StokID," +
                               "		  MaterialName," +
                               "          ProductId," +
                               "          ProductName" +
                               "   FROM [dbo].[Bom]" +
                               "   UNION ALL" +
                               "   SELECT sum(Quantity) RQuantity," +
                               "          StokID," +
                               "		MaterialName," +
                               "          ProductName," +
                               "          OptionsName" +
                               "   FROM [dbo].BomOptions" +
                               "   WHERE ISNULL(MaliyetDahil, 0)=1" +
                               "   GROUP BY StokID," +
                               "            ProductName," +
                               "            OptionsName,MaterialName)" +
                               " SELECT AVG(ISNULL(t.MIKTAR, 0)) MIKTAR," +
                               "       t.HMADI ProductName," +
                               "       AVG(ISNULL(t.TUTAR, 0)) TUTAR," +
                               "       SUM(ISNULL(t.INDIRIM, 0)) INDIRIM," +
                               "       SUM(ISNULL(t.IKRAM, 0)) IKRAM," +
                               "       SUM(ISNULL(t.RECETEMIKTARTOPLAM, 0)) RECETEMIKTARTOPLAM," +
                               "       SUM(ISNULL(t.RECETETUTAR, 0))  RECETETUTAR" +
                               "	   FROM (" +
                               " select " +
                               " b.*," +
                               "  (ISNULL(b.MIKTAR,1)*ISNULL(recete.RQuantity, 1)) RECETEMIKTARTOPLAM ," +
                               "          ((ISNULL(b.MIKTAR,1)*ISNULL(recete.RQuantity, 1)))*(case when ISNULL(maliyet.MALIYET,0)=0 then m2.MALIYET else  ISNULL(maliyet.MALIYET,0) end ) RECETETUTAR," +
                               "		  m3.HMADI" +
                               "		  " +
                               "		  from maliyet m3" +
                               " LEFT JOIN satisproduct b ON m3.HMADI=b.ProductName " +
                               " LEFT JOIN recete ON recete.ProductName=m3.HMADI" +
                               " LEFT JOIN maliyet ON maliyet.IND=recete.StokID" +
                               " LEFT JOIN maliyet m2 ON m2.HMADI=b.ProductName " +
                               " ) as t" +
                               " group by " +
                               " t.HMADI ";
                            }
                            else if (KirilimNo == 2 && !tumStoklarGetirilsinMi) //Tüm stoklar false
                            {
                                query =
                                       " Declare @par1 nvarchar(20) = '{TARIH1}';" +
                                       " Declare @par2 nvarchar(20) = '{TARIH2}';" +
                                       " WITH pro as" +
                                       "  (select ISNULL(P.ProductName, '')+ISNULL('.'+ch1.Name, '')+ISNULL('.'+ch2.Name, '') pname," +
                                       "          ISNULL(P.Price, 0)+ISNULL(ch1.Price, 0)+ISNULL(ch2.Price, 0) pprice," +
                                       "          ProductGroup" +
                                       "   from Product p" +
                                       "   left join Choice1 ch1 on ch1.ProductId=p.Id" +
                                       "   left join Choice2 ch2 on ch2.ProductId=p.Id" +
                                       "   and ch2.Choice1Id=ch1.Id)," +
                                       "     maliyet AS" +
                                       "  (SELECT MALIYET," +
                                       "          IND," +
                                       "          STOKKODU HMKODU," +
                                       "          MALINCINSI HMADI" +
                                       "   FROM " + vega_Db + ".DBO.F0" + FirmaId + "TBLSTOKLAR), " +
                                       "     ProEqu AS" +
                                       "  (SELECT ISNULL(pq.ProductName, '') + ISNULL('.' +" +
                                       "                                                (SELECT name" +
                                       "                                                 FROM Choice1 ch1" +
                                       "                                                 WHERE ch1.Id = pq.choice1Id ), '') + ISNULL('.' +" +
                                       "                                                                                               (SELECT name" +
                                       "                                                                                                FROM Choice2 ch2" +
                                       "                                                                                                WHERE ch2.Id = pq.choice2Id ), '') ProductName," +
                                       "          Multiplier," +
                                       "          EquProductName" +
                                       "   FROM ProductEqu Pq)," +
                                       "     optequ AS" +
                                       "  (SELECT oq.ProductName," +
                                       "          (oq.ProductName + oq.Options) oqproname," +
                                       "          EquProduct," +
                                       "          miktar," +
                                       "          AnaUrun" +
                                       "   FROM OptionsEqu oq)," +
                                       "     Base AS" +
                                       "  (SELECT B.ProductId," +
                                       "          B.ProductName," +
                                       "          P.ProductName PName," +
                                       "          B.Date PaymentTime," +
                                       "          B.Quantity," +
                                       "          b.Options," +
                                       "          CASE" +
                                       "              WHEN CHARINDEX('x', LTRIM(RTRIM(O.s))) > 0 THEN SUBSTRING(LTRIM(RTRIM(O.s)), CHARINDEX('x', LTRIM(RTRIM(O.s))) + 1, 100)" +
                                       "              ELSE LTRIM(RTRIM(O.s))" +
                                       "          END AS Opt," +
                                       "          CASE" +
                                       "              WHEN CHARINDEX('x', LTRIM(RTRIM(O.s))) > 0 THEN SUBSTRING(LTRIM(RTRIM(O.s)), 1, CHARINDEX('x', LTRIM(RTRIM(O.s))) - 1)" +
                                       "              ELSE '1'" +
                                       "          END AS OptQty," +
                                       "          B.HeaderId" +
                                       "   FROM Bill B" +
                                       "   LEFT JOIN Product P ON B.ProductId = P.Id CROSS APPLY dbo.SplitString(',', B.Options) AS O" +
                                       "   WHERE ISNULL(B.Options, '') <> ''" +
                                       "     AND B.Date BETWEEN @par1 AND @par2 )," +
                                       "     BillPrice AS" +
                                       "  (SELECT Bp.ProductName," +
                                       "          Bp.Price," +
                                       "          Bp.MinDate," +
                                       "          Bp.Options," +
                                       "          CASE" +
                                       "              WHEN Bp.MaxDate=MAX(Bp.MaxDate) OVER(PARTITION BY Bp.ProductName) THEN GETDATE()" +
                                       "              ELSE Bp.MaxDate" +
                                       "          END MaxDate" +
                                       "   FROM" +
                                       "     (SELECT ProductName," +
                                       "             Price," +
                                       "             Options," +
                                       "             MIN(Date) MinDate," +
                                       "             MAX(Date) MaxDate" +
                                       "      FROM Bill B" +
                                       "      WHERE UserName='PAKET'" +
                                       "      GROUP BY ProductName," +
                                       "               Price," +
                                       "               Options) Bp)," +
                                       "     OptSatislar AS" +
                                       "  (SELECT Oe.EquProduct," +
                                       "          Oe.Miktar," +
                                       "          B.ProductName," +
                                       "          B.HeaderId," +
                                       "          B.PaymentTime," +
                                       "          B.ProductId," +
                                       "          Quantity * (CASE" +
                                       "                          WHEN ISNUMERIC(OptQty) = 1 THEN CAST(OptQty AS INT)" +
                                       "                          ELSE 1" +
                                       "                      END) AS Quantity," +
                                       "          Opt AS OptionsName," +
                                       "          B.ProductName aaa," +
                                       "          ISNULL(Oe.EquProduct, opt) ProductName2," +
                                       "          CASE" +
                                       "              WHEN ISNULL(Oe.MenuFiyat, 0)=0 THEN CASE" +
                                       "                                                      WHEN oe.AnaUrun=1 THEN MAX(Bp.Price)" +
                                       "                                                      ELSE ISNULL(Oe.MenuFiyat, 0)" +
                                       "                                                  END" +
                                       "              ELSE ISNULL(Oe.MenuFiyat, 0)" +
                                       "          END MenuFiyat" +
                                       "   FROM Base B" +
                                       "   LEFT JOIN OptionsEqu Oe ON Oe.ProductName+Oe.Options=B.PName+ISNULL(B.Opt, '')" +
                                       "   LEFT JOIN BillPrice Bp ON Oe.EquProduct=Bp.ProductName" +
                                       "   AND B.PaymentTime BETWEEN Bp.MinDate AND Bp.MaxDate" +
                                       "   WHERE opt not like '%istemiyorum%'" +
                                       "     OR opt not like '%istiyorum%'" +
                                       "   GROUP BY Oe.EquProduct," +
                                       "            Oe.Miktar," +
                                       "            oe.AnaUrun," +
                                       "            B.ProductName," +
                                       "            B.HeaderId," +
                                       "            B.PaymentTime," +
                                       "            B.ProductId," +
                                       "            Oe.MenuFiyat," +
                                       "            Quantity * (CASE" +
                                       "                            WHEN ISNUMERIC(OptQty) = 1 THEN CAST(OptQty AS INT)" +
                                       "                            ELSE 1" +
                                       "                        END)," +
                                       "            Opt)," +
                                       "     opttotal AS" +
                                       "  (SELECT optsatislar.ProductId," +
                                       "          ISNULL(EquProduct, optsatislar.ProductName) BillProductName," +
                                       "          ISNULL(EquProduct, optsatislar.OptionsName) ProductName," +
                                       "          '' ProductGroup," +
                                       "             '' InvoiceName," +
                                       "                sum(Quantity) OrjToplam," +
                                       "                sum(Quantity * ISNULL(Miktar, 1)) Toplam," +
                                       "                (sum(MenuFiyat)) * sum(Quantity) OrjTutar," +
                                       "                CASE" +
                                       "                    WHEN sum(MenuFiyat) = 0" +
                                       "                         AND SUM(ISNULL(Miktar, 0)) > 0 THEN 0" +
                                       "                    ELSE sum(optsatislar.MenuFiyat*Quantity)" +
                                       "                END Tutar" +
                                       "   FROM optsatislar" +
                                       "   GROUP BY EquProduct," +
                                       "            optsatislar.ProductName," +
                                       "            optsatislar.OptionsName," +
                                       "            optsatislar.ProductId)," +
                                       "     indirim AS" +
                                       "  (SELECT TOP 9999999999 ISNULL(avg(ISNULL(b2.discount, 1)), 1) / (CASE" +
                                       "                                                                       WHEN sum(Quantity * Price) = 0 THEN 1" +
                                       "                                                                       ELSE sum(Quantity * Price)" +
                                       "                                                                   END) indirimtutar," +
                                       "                         HeaderId indHeaderId" +
                                       "   FROM PaidBill b2" +
                                       "   WHERE PaymentTime >= @par1" +
                                       "   GROUP BY HeaderId)," +
                                       "     paketindirim AS" +
                                       "  (SELECT TOP 9999999 ISNULL(ph.discount, 0) /" +
                                       "     (SELECT sum(Quantity * Price)" +
                                       "      FROM bill b2" +
                                       "      WHERE b2.HeaderId = ph.HeaderId ) pktindirim," +
                                       "                      HeaderId pktheaderId" +
                                       "   FROM PhoneOrderHeader ph)," +
                                       " satislar2 AS" +
                                       "  (SELECT pq.EquProductName," +
                                       "          P.Id AS ProductId," +
                                       "          SUM(b.Quantity * ISNULL(pq.Multiplier, 1)) AS Toplam," +
                                       "          ISNULL(pq.EquProductName, B.ProductName) ProductName," +
                                       "          P.ProductGroup," +
                                       "          sum((ISNULL(b.Quantity, 0) * CASE" +
                                       "                                           WHEN ISNULL(pq.Multiplier, 1)=0 THEN 1" +
                                       "                                           ELSE ISNULL(pq.Multiplier, 1)" +
                                       "                                       END) * ISNULL(B.Price, 0)) AS Tutar," +
                                       "     (SELECT SUM(Bill.Price * Bill.Quantity)" +
                                       "      FROM dbo.BillWithHeader AS Bill" +
                                       "      WHERE BillState = 0" +
                                       "        AND HeaderId = b.HeaderId" +
                                       "        AND ProductId = b.ProductId ) AS OPENTABLE," +
                                       "     (SELECT SUM(Bill.Quantity)" +
                                       "      FROM dbo.BillWithHeader AS Bill" +
                                       "      WHERE BillState = 0" +
                                       "        AND HeaderId = b.HeaderId" +
                                       "        AND ProductId = b.ProductId ) AS OPENTABLEQuantity," +
                                       "          0 AS Discount," +
                                       "          SUM((ISNULL(b.Quantity, 0) * ISNULL(pq.Multiplier, 1)) * ISNULL(B.Price, 0)) * (avg(ind.indirimtutar)) INDIRIM," +
                                       "          SUM((ISNULL(b.Quantity, 0) * ISNULL(pq.Multiplier, 1)) * ISNULL(B.Price, 0)) * avg(pkt.pktindirim) PAKETINDIRIM," +
                                       "          CASE" +
                                       "              WHEN sum(ISNULL(B.Price, 0)) = 0 THEN sum((ISNULL(B.OriginalPrice, 0)))" +
                                       "              ELSE 0" +
                                       "          END IKRAM " +
                                       "   FROM dbo.Bill AS b WITH (NOLOCK)" +
                                       "   LEFT JOIN proequ pq ON pq.ProductName = B.ProductName" +
                                       "   LEFT JOIN dbo.Product AS P ON P.Id = B.ProductId" +
                                       "   LEFT JOIN indirim AS ind ON ind.indHeaderId = b.HeaderId" +
                                       "   LEFT JOIN paketindirim AS pkt ON pkt.pktheaderId = b.HeaderId" +
                                       "   WHERE [Date] >= @par1" +
                                       "     AND [Date] <= @par2" +
                                       "     AND P.ProductName NOT LIKE '$%'" +
                                       "   GROUP BY pq.EquProductName," +
                                       "            B.ProductName," +
                                       "            b.ProductId," +
                                       "            P.ProductGroup," +
                                       "            B.HeaderId," +
                                       "            p.Id," +
                                       "            pq.Multiplier)," +
                                       "     satisproduct as" +
                                       "  (SELECT sum(MIKTAR) MIKTAR," +
                                       "          a.ProductName," +
                                       "          Sum(TUTAR) TUTAR," +
                                       "          SUM(INDIRIM) INDIRIM," +
                                       "          SUM(IKRAM) IKRAM" +
                                       "   FROM" +
                                       "     (SELECT CASE" +
                                       "                 WHEN sum(opttotal.Toplam) = 0 THEN sum(opttotal.OrjToplam)" +
                                       "                 ELSE sum(opttotal.Toplam)" +
                                       "             END AS MIKTAR," +
                                       "             opttotal.ProductName," +
                                       "             ProductGroup AS ProductGroup," +
                                       "             sum(ISNULL(opttotal.Tutar, 0)) AS TUTAR," +
                                       "             0 INDIRIM," +
                                       "             0 IKRAM" +
                                       "      FROM opttotal" +
                                       "      GROUP BY opttotal.ProductName," +
                                       "               opttotal.ProductGroup" +
                                       "      UNION SELECT sum(toplam) AS MIKTAR," +
                                       "                   satislar2.ProductName," +
                                       "                   satislar2.ProductGroup," +
                                       "                   sum(tutar) AS TUTAR," +
                                       "                   SUM(ISNULL(INDIRIM, 0)) + SUM(ISNULL(PAKETINDIRIM, 0)) INDIRIM," +
                                       "                   SUM(IKRAM) IKRAM" +
                                       "      FROM satislar2" +
                                       "      GROUP BY satislar2.ProductName," +
                                       "               satislar2.ProductGroup," +
                                       "               satislar2.ProductId) AS a" +
                                       "   WHERE MIKTAR<>0" +
                                       "   GROUP BY a.ProductName)," +
                                       "      recete AS" +
                                       "  (SELECT [Quantity] RQuantity ," +
                                       "          StokID," +
                                       "		  MaterialName," +
                                       "          ProductId," +
                                       "          ProductName" +
                                       "   FROM [dbo].[Bom]" +
                                       "   UNION ALL" +
                                       "   SELECT sum(Quantity) RQuantity," +
                                       "          StokID," +
                                       "		MaterialName," +
                                       "          ProductName," +
                                       "          OptionsName" +
                                       "   FROM [dbo].BomOptions" +
                                       "   WHERE ISNULL(MaliyetDahil, 0)=1" +
                                       "   GROUP BY StokID," +
                                       "            ProductName," +
                                       "            OptionsName,MaterialName)" +
                                       " SELECT AVG(ISNULL(t.MIKTAR, 0)) MIKTAR," +
                                       "       t.ProductName ProductName," +
                                       "       AVG(ISNULL(t.TUTAR, 0)) TUTAR," +
                                       "       SUM(ISNULL(t.INDIRIM, 0)) INDIRIM," +
                                       "       SUM(ISNULL(t.IKRAM, 0)) IKRAM," +
                                       "       SUM(ISNULL(t.RECETEMIKTARTOPLAM, 0)) RECETEMIKTARTOPLAM," +
                                       "       SUM(ISNULL(t.RECETETUTAR, 0))  RECETETUTAR" +
                                       "	   FROM (" +
                                       " select " +
                                       " b.*," +
                                       "  (b.MIKTAR*ISNULL(recete.RQuantity, 1)) RECETEMIKTARTOPLAM ," +
                                       "          ((b.MIKTAR*ISNULL(recete.RQuantity, 1)))*(case when ISNULL(maliyet.MALIYET,0)=0 then m2.MALIYET else  ISNULL(maliyet.MALIYET,0) end ) RECETETUTAR" +
                                       "		  " +
                                       "		  from satisproduct b" +
                                       " LEFT JOIN recete ON recete.ProductName=b.ProductName" +
                                       " LEFT JOIN maliyet ON maliyet.IND=recete.StokID" +
                                       " LEFT JOIN maliyet m2 ON m2.HMADI=b.ProductName  ) as t" +
                                       " group by " +
                                       " t.ProductName";
                            }
                            else
                            {
                                if (string.IsNullOrEmpty(urunEslestirmeVarMi) || urunEslestirmeVarMi == "False")
                                {
                                    query = File.ReadAllText(HostingEnvironment.MapPath("/Sql/SubeUrunSubeBazli.sql"), System.Text.UTF8Encoding.Default);
                                }
                                else
                                {
                                    query = File.ReadAllText(HostingEnvironment.MapPath("/Sql/SqlNewSef/SubeUrunRaporu/SubeUrunSubeBazliUrunEslestirme.sql"), System.Text.UTF8Encoding.Default);
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
                            }

                            #endregion  Kırılımlarda Ana Şube Altındaki IND id alıyorum 

                            if (AppDbTypeStatus == "True")
                            {
                                if (subeid == null && subeid.Equals("0") || subeid == "")// sube secili degilse ilk giris yapilan sql

                                    #region FASTER ONLINE QUARY                             

                                    query =
                                                    "DECLARE @Sube nvarchar(100) = '{SUBEADI}';" +
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
                                                    "         LEFT JOIN TBLFASTERSATISBASLIK AS FSB ON FSH.BASLIKIND=FSB.BASLIKIND" +
                                                    "         AND FSH.SUBEIND=FSB.SUBEIND" +
                                                    "         AND FSH.KASAIND=FSB.KASAIND" +
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
                                                    "         LEFT JOIN TBLFASTERSATISBASLIK AS FSB ON FSH.BASLIKIND=FSB.BASLIKIND" +
                                                    "         AND FSH.SUBEIND=FSB.SUBEIND" +
                                                    "         AND FSH.KASAIND=FSB.KASAIND" +
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

                                    query =
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
                                    query =
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
                                    query =
                                    "DECLARE @Sube nvarchar(100) = '{SUBEADI}';" +
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
                                            "          STK.MALINCINSI AS ProductName" +
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
                                            "   LEFT JOIN TBLFASTERSTOKLAR AS STK ON FSH.STOKIND=STK.STOKNO and FSH.BIRIMIND=STK.BIRIMIND" +
                                            "   WHERE FSH.ISLEMTARIHI>=@par1" +
                                            "     AND FSH.ISLEMTARIHI<=@par2" +
                                            "     AND ISNULL(FSB.IADE, 0)=1" +
                                            "   GROUP BY FSB.SUBEIND," +
                                            "            FSB.KASAIND," +
                                            "            STK.MALINCINSI" +
                                            "			" +
                                            "			) ) T" +
                                            "			GROUP BY " +
                                            "			T.Sube1," +
                                            "T.Kasa," +
                                            "T.Id," +
                                            "T.ProductName";
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

                                query =
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
                                query =
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
                    }
                    else
                    {
                        if (AppDbType == "1" || AppDbType == "2")
                        {
                            #region ŞUBEDE EN COK SATILAN URUNU ALMAK ICIN
                            query = File.ReadAllText(HostingEnvironment.MapPath("/Sql/EncokSatisYapanUrun.sql"), System.Text.UTF8Encoding.Default);
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

                                query =
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
                                               "   LEFT JOIN TBLFASTERSATISBASLIK AS FSB ON FSH.BASLIKIND=FSB.BASLIKIND" +
                                               "   AND FSH.SUBEIND=FSB.SUBEIND" +
                                               "   AND FSH.KASAIND=FSB.KASAIND" +
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
                                               "   LEFT JOIN TBLFASTERSATISBASLIK AS FSB ON FSH.BASLIKIND=FSB.BASLIKIND" +
                                               "   AND FSH.SUBEIND=FSB.SUBEIND" +
                                               "   AND FSH.KASAIND=FSB.KASAIND" +
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

                                query =
                                   "DECLARE @Sube nvarchar(100) = '{SUBEADI}';" +
                                   "DECLARE @par1 nvarchar(20) = '{TARIH1}';" +
                                   "DECLARE @par2 nvarchar(20) = '{TARIH2}';" +
                                   "SELECT " +
                                   "T.Sube1," +
                                   "T.Kasa," +
                                   "T.Id," +
                                   " SUM(T.MIKTAR) MIKTAR," +
                                   " SUM(T.TUTAR) TUTAR, " +
                                   " T.ProductName " +
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
                                   "      WHERE IND=FSB.KASAIND) AS Id," +
                                   "          SUM(FSH.MIKTAR) AS MIKTAR," +
                                   "          SUM((((MIKTAR*SATISFIYATI) * (100-ISNULL(FSH.ISK1, 0))/100) * (100-ISNULL(FSH.ISK2, 0))/100) * (100-ISNULL(FSB.ALTISKORAN, 0))/100) AS TUTAR," +
                                   "          STK.MALINCINSI AS ProductName" +
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
                                   "            STK.MALINCINSI" +
                                   "			" +
                                   "			UNION ALL SELECT" +
                                   "     (SELECT SUBEADI" +
                                   "     FROM " + FirmaId_SUBE + " " +
                                   "      WHERE IND=FSB.SUBEIND) AS Sube1," +
                                   "     (SELECT KASAADI" +
                                   "      FROM " + FirmaId_KASA + "" +
                                   "      WHERE IND=FSB.KASAIND) AS Kasa," +
                                   "     (SELECT IND" +
                                   "      FROM " + FirmaId_KASA + "" +
                                   "      WHERE IND=FSB.KASAIND) AS Id," +
                                   "          SUM(FSH.MIKTAR)*-1.00 AS MIKTAR," +
                                   "          SUM((((MIKTAR*SATISFIYATI) * (100-ISNULL(FSH.ISK1, 0))/100) * (100-ISNULL(FSH.ISK2, 0))/100) * (100-ISNULL(FSB.ALTISKORAN, 0))/100) *-1.00 AS TUTAR," +
                                   "          STK.MALINCINSI AS ProductName" +
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
                                   "            STK.MALINCINSI" +
                                   "			" +
                                   "			) ) T" +
                                   "			GROUP BY " +
                                   "			T.Sube1," +
                                   "T.Kasa," +
                                   "T.Id," +
                                   "T.ProductName";
                                //Query = File.ReadAllText(HostingEnvironment.MapPath("/Sql/SqlFaster/SubeUrun/SubeUrunFasterOFLINE.sql"), System.Text.UTF8Encoding.Default);
                            }
                        }
                        else if (AppDbType == "4")
                        {
                            #region NPOS QUARY (En Çok Satılan Ürün)
                            query =
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
                    }

                    query = query.Replace("{SUBEADI}", SubeAdi);
                    query = query.Replace("{TARIH1}", QueryTimeStart);
                    query = query.Replace("{TARIH2}", QueryTimeEnd);
                    query = query.Replace("{FIRMAIND}", FirmaId);
                    query = query.Replace("{KASALAR}", FirmaId_KASA);
                    query = query.Replace("{SUBELER}", FirmaId_SUBE);

                    if (ID == "1")
                    {
                        #region GET DATA   

                        try
                        {
                            try
                            {
                                DataTable SubeUrunCiroDt = new DataTable();
                                SubeUrunCiroDt = f.GetSubeDataWithQuery(f.NewConnectionString(SubeIP, DBName, SqlName, SqlPassword), query.ToString());

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
                                                SubeUrun items = new SubeUrun();
                                                items.SubeID = SubeId + "~" + sube["Id"].ToString();
                                                items.Sube = SubeAdi + "-" + sube["Sube1"].ToString() + "-" + sube["Kasa"].ToString();
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
                                                SubeUrun items = new SubeUrun();
                                                items.SubeID = SubeId + "~" + sube["Kasa"].ToString();
                                                items.Sube = SubeAdi + "_" + sube["Sube"].ToString() + "-" + sube["Sube1"].ToString() + "-" + sube["Kasa"].ToString();
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

                                            //Recete Maliyet Raporu
                                            List<SubeUrun> receteMaliyetList = RecipeCostCRUD.List(Convert.ToDateTime(Date1), Convert.ToDateTime(Date2), SubeId, ID, 0, "", tumStoklarGetirilsinMi);

                                            SubeUrun items = new SubeUrun
                                            {
                                                Sube = SubeAdi, //f.RTS(SubeUrunCiroDt.Rows[0], "Sube");
                                                SubeID = (SubeId)
                                            };
                                            if (subeid.Equals("") && subeid != "exportExcel")
                                            {
                                                foreach (DataRow SubeR in SubeUrunCiroDt.Rows)
                                                {
                                                    items.Miktar += f.RTD(SubeR, "MIKTAR");
                                                    items.Debit += f.RTD(SubeR, "TUTAR");
                                                    items.ProductName = f.RTS(SubeR, "ProductName");
                                                    items.Ikram += f.RTD(SubeR, "IKRAM");
                                                    items.Indirim += f.RTD(SubeR, "INDIRIM");

                                                    foreach (SubeUrun receteMaliyet in receteMaliyetList)
                                                    {
                                                        if (receteMaliyet.ProductName == items.ProductName)
                                                        {
                                                            items.ReceteTutari += receteMaliyet.ReceteTutari;

                                                            //items.ReceteBirimMaliyeti += items.ReceteTutari / items.Miktar;
                                                            items.ReceteBirimMaliyeti = items.ReceteTutari > 0M && items.Miktar > 0M ? Math.Abs(items.ReceteTutari) / Math.Abs(items.Miktar) : 0;
                                                        }
                                                    }
                                                }
                                                
                                                items.NetTutar = items.Debit - (items.Indirim /*+ items.Ikram*/);
                                                //items.KarTutari = items.Debit - items.ReceteTutari;
                                                items.KarTutari = items.NetTutar - items.ReceteBirimMaliyeti;
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
                                        else if (KirilimNo == 2 && tumStoklarGetirilsinMi)//Tüm stokları getirmek için güncellendi
                                        {
                                            #region SEFIM Tüm Stoklar

                                            foreach (DataRow SubeR in SubeUrunCiroDt.Rows)
                                            {
                                                if (!string.IsNullOrWhiteSpace(f.RTS(SubeR, "ProductName")) && f.RTS(SubeR, "ProductName") != null)
                                                {
                                                    SubeUrun items = new SubeUrun
                                                    {
                                                        Sube = f.RTS(SubeR, "Sube"),
                                                        SubeID = SubeId,
                                                        Miktar = f.RTD(SubeR, "MIKTAR"),
                                                        ProductName = f.RTS(SubeR, "ProductName"),
                                                        Debit = Math.Abs(f.RTD(SubeR, "TUTAR")),
                                                        Ikram = f.RTD(SubeR, "IKRAM"),
                                                        Indirim = Math.Abs(f.RTD(SubeR, "INDIRIM")),
                                                        ReceteTutari = Math.Abs(f.RTD(SubeR, "RECETETUTAR")),
                                                        ReceteToplamMiktari = Math.Abs(f.RTD(SubeR, "RECETEMIKTARTOPLAM")),
                                                    };

                                                    items.NetTutar = Math.Abs(items.Debit) - (Math.Abs(items.Indirim)/* + Math.Abs(items.Ikram)*/);
                                                    items.KarTutari = Math.Abs(items.Debit) - Math.Abs(items.ReceteTutari);
                                                    items.ReceteBirimMaliyeti = items.ReceteTutari > 0M && items.Miktar > 0M ? Math.Abs(items.ReceteTutari) / Math.Abs(items.Miktar) : 0;
                                                    Liste.Add(items);
                                                }
                                            }
                                            #endregion SEFIM Tüm Stoklar
                                        }
                                        else
                                        {
                                            #region SEFIM 

                                            //Recete Maliyet Raporu
                                            List<SubeUrun> receteMaliyetList = RecipeCostCRUD.List(Convert.ToDateTime(Date1), Convert.ToDateTime(Date2), SubeId, ID, 0, "");

                                            foreach (DataRow SubeR in SubeUrunCiroDt.Rows)
                                            {
                                                SubeUrun items = new SubeUrun
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
                                                items.NetTutar = items.Debit - (items.Indirim/* + items.Ikram*/);

                                                foreach (SubeUrun receteMaliyet in receteMaliyetList)
                                                {
                                                    if (receteMaliyet.ProductName == items.ProductName)
                                                    {
                                                        items.ReceteTutari = receteMaliyet.ReceteTutari;
                                                        //items.KarTutari = items.Debit - items.ReceteTutari;
                                                        
                                                        //items.ReceteBirimMaliyeti = items.ReceteTutari / items.Miktar;
                                                        items.ReceteBirimMaliyeti = items.ReceteTutari > 0M && items.Miktar > 0M ? Math.Abs(items.ReceteTutari) / Math.Abs(items.Miktar) : 0;
                                                        items.KarTutari = items.NetTutar - items.ReceteTutari;
                                                    }
                                                }
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
                                Singleton.WritingLogFile2("RecipeCostCRUD2:", ex.Message.ToString(), "", ex.StackTrace);
                                throw new Exception(SubeAdi);
                            }
                        }
                        catch (Exception ex)
                        {
                            try
                            {
                                #region EX                            

                                Singleton.WritingLogFile2("RecipeCostCRUD2:", ex.ToString(), null, ex.StackTrace);
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
                                        SubeUrunCiroDt = f.GetSubeDataWithQuery(f.NewConnectionString(SubeIP, DBName, SqlName, SqlPassword), query.ToString());

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
                                                        SubeUrun items = new SubeUrun();
                                                        items.SubeID = SubeId + "~" + sube["Id"].ToString();
                                                        items.Sube = SubeAdi + "-" + sube["Sube1"].ToString() + "-" + sube["Kasa"].ToString();
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
                                                        SubeUrun items = new SubeUrun();
                                                        items.SubeID = SubeId + "~" + sube["Kasa"].ToString();
                                                        items.Sube = SubeAdi + "_" + sube["Sube"].ToString() + "-" + sube["Sube1"].ToString() + "-" + sube["Kasa"].ToString();
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

                                                    //Recete Maliyet Raporu
                                                    List<SubeUrun> receteMaliyetList = RecipeCostCRUD.List(Convert.ToDateTime(Date1), Convert.ToDateTime(Date2), SubeId, ID, 0, "", tumStoklarGetirilsinMi);

                                                    SubeUrun items = new SubeUrun
                                                    {
                                                        Sube = SubeAdi, //f.RTS(SubeUrunCiroDt.Rows[0], "Sube");
                                                        SubeID = (SubeId)
                                                    };
                                                    if (subeid.Equals("") && subeid != "exportExcel")
                                                    {
                                                        foreach (DataRow SubeR in SubeUrunCiroDt.Rows)
                                                        {
                                                            items.Miktar += f.RTD(SubeR, "MIKTAR");
                                                            items.Debit += f.RTD(SubeR, "TUTAR");
                                                            items.ProductName = f.RTS(SubeR, "ProductName");
                                                            items.Ikram += f.RTD(SubeR, "IKRAM");
                                                            items.Indirim += f.RTD(SubeR, "INDIRIM");

                                                            foreach (SubeUrun receteMaliyet in receteMaliyetList)
                                                            {
                                                                if (receteMaliyet.ProductName == items.ProductName)
                                                                {
                                                                    items.ReceteTutari += receteMaliyet.ReceteTutari;

                                                                    //items.ReceteBirimMaliyeti += items.ReceteTutari / items.Miktar;
                                                                    items.ReceteBirimMaliyeti = items.ReceteTutari > 0M && items.Miktar > 0M ? Math.Abs(items.ReceteTutari) / Math.Abs(items.Miktar) : 0;
                                                                }
                                                            }
                                                        }

                                                        items.NetTutar = items.Debit - (items.Indirim /*+ items.Ikram*/);
                                                        //items.KarTutari = items.Debit - items.ReceteTutari;
                                                        items.KarTutari = items.NetTutar - items.ReceteBirimMaliyeti;
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
                                                else if (KirilimNo == 2 && tumStoklarGetirilsinMi)//Tüm stokları getirmek için güncellendi
                                                {
                                                    #region SEFIM Tüm Stoklar

                                                    foreach (DataRow SubeR in SubeUrunCiroDt.Rows)
                                                    {
                                                        if (!string.IsNullOrWhiteSpace(f.RTS(SubeR, "ProductName")) && f.RTS(SubeR, "ProductName") != null)
                                                        {
                                                            SubeUrun items = new SubeUrun
                                                            {
                                                                Sube = f.RTS(SubeR, "Sube"),
                                                                SubeID = SubeId,
                                                                Miktar = f.RTD(SubeR, "MIKTAR"),
                                                                ProductName = f.RTS(SubeR, "ProductName"),
                                                                Debit = Math.Abs(f.RTD(SubeR, "TUTAR")),
                                                                Ikram = f.RTD(SubeR, "IKRAM"),
                                                                Indirim = Math.Abs(f.RTD(SubeR, "INDIRIM")),
                                                                ReceteTutari = Math.Abs(f.RTD(SubeR, "RECETETUTAR")),
                                                                ReceteToplamMiktari = Math.Abs(f.RTD(SubeR, "RECETEMIKTARTOPLAM")),
                                                            };

                                                            items.NetTutar = Math.Abs(items.Debit) - (Math.Abs(items.Indirim)/* + Math.Abs(items.Ikram)*/);
                                                            items.KarTutari = Math.Abs(items.Debit) - Math.Abs(items.ReceteTutari);
                                                            items.ReceteBirimMaliyeti = items.ReceteTutari > 0M && items.Miktar > 0M ? Math.Abs(items.ReceteTutari) / Math.Abs(items.Miktar) : 0;
                                                            Liste.Add(items);
                                                        }
                                                    }
                                                    #endregion SEFIM Tüm Stoklar
                                                }
                                                else
                                                {
                                                    #region SEFIM 

                                                    //Recete Maliyet Raporu
                                                    List<SubeUrun> receteMaliyetList = RecipeCostCRUD.List(Convert.ToDateTime(Date1), Convert.ToDateTime(Date2), SubeId, ID, 0, "");

                                                    foreach (DataRow SubeR in SubeUrunCiroDt.Rows)
                                                    {
                                                        SubeUrun items = new SubeUrun
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
                                                        items.NetTutar = items.Debit - (items.Indirim/* + items.Ikram*/);

                                                        foreach (SubeUrun receteMaliyet in receteMaliyetList)
                                                        {
                                                            if (receteMaliyet.ProductName == items.ProductName)
                                                            {
                                                                items.ReceteTutari = receteMaliyet.ReceteTutari;
                                                                //items.KarTutari = items.Debit - items.ReceteTutari;

                                                                //items.ReceteBirimMaliyeti = items.ReceteTutari / items.Miktar;
                                                                items.ReceteBirimMaliyeti = items.ReceteTutari > 0M && items.Miktar > 0M ? Math.Abs(items.ReceteTutari) / Math.Abs(items.Miktar) : 0;
                                                                items.KarTutari = items.NetTutar - items.ReceteTutari;
                                                            }
                                                        }
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
                                        Singleton.WritingLogFile2("RecipeCostCRUD2:", ex.Message.ToString(), "", ex.StackTrace);
                                        throw new Exception(SubeAdi);
                                    }
                                }
                                catch (Exception ex)
                                {
                                    try
                                    {
                                        #region EX                            

                                        Singleton.WritingLogFile2("RecipeCostCRUD2:", ex.ToString(), null, ex.StackTrace);
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
            catch (DataException ex)
            {
                Singleton.WritingLogFile2("RecipeCostCRUD2", ex.ToString(), null, ex.StackTrace);
            }

            return Liste.OrderBy(x => x.Sube).ToList();
        }
    }
}