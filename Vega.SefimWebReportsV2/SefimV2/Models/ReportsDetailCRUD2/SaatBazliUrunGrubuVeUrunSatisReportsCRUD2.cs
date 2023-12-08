using SefimV2.Helper;
using SefimV2.ViewModels.ReportsDetail;
using SefimV2.ViewModels.User;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Hosting;

namespace SefimV2.Models.ReportsDetailCRUD
{
    public class SaatBazliUrunGrubuVeUrunSatisReportsCRUD2
    {
        public static List<ReportsDetailViewModel> List(DateTime Date1, DateTime Date2, string subeId, string endDate, string id, string urunGrubu, string urunAdi, string saatList, string satisTipi, bool DetayRaporMu)
        {
            Date2 = Date2.AddDays(1);
            List<ReportsDetailViewModel> Liste = new List<ReportsDetailViewModel>();
            ModelFunctions mf = new ModelFunctions();

            #region GİRİŞ YAPAN KULLANICININ YETKİLİ OLDUĞU ŞUBELER LİSTELENİYOR
            UserViewModel model = UsersListCRUD.YetkiliSubesi(id);
            #endregion

            string subeid_ = string.Empty;
            try
            {
                #region Kırılımlarda ilk once db de tanımlı subeyı alıyoruz.   
                //if (!subeid.Equals(""))
                //{
                //    string[] tableNo = subeid.Split('~');
                //    subeid_ = tableNo[0];
                //}
                #endregion   Kırılımlarda ilk once db de tanımlı subeyı alıyoruz.  

                #region SUBSTATION LIST      
                string[] subeSplit = subeId.Split(',');
                string filter = " Where  Status=1";
                if (subeId != "null" && !string.IsNullOrWhiteSpace(subeId))
                {
                    filter = " Where  Status=1 and";
                    for (int i = 0; i < subeSplit.Length; i++)
                    {
                        if (subeSplit.Length - 1 == i)
                        {
                            filter += " ID='" + subeSplit[i] + "'";
                        }
                        else
                        {
                            filter += " ID='" + subeSplit[i] + "' or  ";
                        }
                    }
                }

                mf.SqlConnOpen();
                DataTable dt = mf.DataTable("Select * From SubeSettings  " + filter);
                mf.SqlConnClose();
                #endregion SUBSTATION LIST

                //Saat filter
                string filterSaat = string.Empty;
                if (saatList != "null" && !string.IsNullOrWhiteSpace(saatList))
                {
                    string[] saatSplit = saatList.Split(',');
                    filterSaat = " Where (";
                    for (int i = 0; i < saatSplit.Length; i++)
                    {
                        if (saatSplit.Length - 1 == i)
                        {
                            filterSaat += " SAAT='" + saatSplit[i] + "' )";
                        }
                        else
                        {
                            filterSaat += " SAAT='" + saatSplit[i] + "' OR  ";
                        }
                    }
                }

                //Ürün Grubu Listesi
                string filterUrunGrubu = string.Empty;
                if (urunGrubu != "null" && !string.IsNullOrWhiteSpace(urunGrubu))
                {
                    string[] urunGrubuAdiSplit = urunGrubu.Split(',');
                    filterUrunGrubu = "  and (";
                    for (int i = 0; i < urunGrubuAdiSplit.Length; i++)
                    {
                        if (urunGrubuAdiSplit.Length - 1 == i)
                        {
                            filterUrunGrubu += "  a.ProductGroup='" + urunGrubuAdiSplit[i] + "' )";
                        }
                        else
                        {
                            filterUrunGrubu += "  a.ProductGroup='" + urunGrubuAdiSplit[i] + "' OR ";
                        }
                    }
                }

                //Ürün Listesi
                string filterUrun = string.Empty;
                if (urunAdi != "null" && !string.IsNullOrWhiteSpace(urunAdi))
                {
                    string[] urunAdiSplit = urunAdi.Split(',');
                    filterUrun = "  and ( ";
                    for (int i = 0; i < urunAdiSplit.Length; i++)
                    {
                        if (urunAdiSplit.Length - 1 == i)
                        {
                            filterUrun += " a.ProductName='" + urunAdiSplit[i] + "' )";
                        }
                        else
                        {
                            filterUrun += " a.ProductName='" + urunAdiSplit[i] + "' OR ";
                        }
                    }
                }

                //Satis Tipi Listesi
                string filterSatisTipi = string.Empty;
                if (satisTipi != "null" && !string.IsNullOrWhiteSpace(satisTipi))
                {
                    string[] satisTipiSplit = satisTipi.Split(',');
                    filterSatisTipi = " and (";
                    for (int i = 0; i < satisTipiSplit.Length; i++)
                    {
                        if (satisTipiSplit.Length - 1 == i)
                        {
                            filterSatisTipi += "BillType='" + satisTipiSplit[i] + "' )";
                        }
                        else
                        {
                            filterSatisTipi += " BillType='" + satisTipiSplit[i] + "' OR ";
                        }
                    }
                }

                #region PARALLEL FORECH
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


                    string Query = string.Empty;

                    if (AppDbType == "1" && DetayRaporMu == false)
                    {
                        //Query = File.ReadAllText(HostingEnvironment.MapPath("/Sql/SqlReports/UruneGoreSubeSatisReports/UruneGoreSubeSatisReports.sql"), System.Text.UTF8Encoding.Default);

                        Query =
                        " DECLARE @Sube nvarchar(100) ='{SUBEADI}';" +
                        " DECLARE @par1 nvarchar(20) = '{TARIH1}';" +
                        " DECLARE @par2 nvarchar(20) = '{TARIH2}';" +
                        " DECLARE @par3EndDate nvarchar(20) = '{EndDate}';" +
                        " ;WITH" +
                        " ProEqu AS " +
                        " (" +
                        " SELECT ISNULL(pq.ProductName, '') + ISNULL('.' + (" +
                        "      SELECT name" +
                        "      FROM Choice1 ch1" +
                        "      WHERE ch1.Id = pq.choice1Id" +
                        "      ), '') + ISNULL('.' + (" +
                        "      SELECT name" +
                        "      FROM Choice2 ch2" +
                        "      WHERE ch2.Id = pq.choice2Id" +
                        "      ), '') ProductName" +
                        "  ,Multiplier" +
                        "  ,EquProductName" +
                        " FROM ProductEqu Pq" +
                        " )" +
                        " ,optequ AS " +
                        " (" +
                        " SELECT oq.ProductName" +
                        "  ,(oq.ProductName + oq.Options) oqproname" +
                        "  ,EquProduct" +
                        "  ,miktar" +
                        "  ,AnaUrun" +
                        " FROM OptionsEqu oq" +
                        " )," +
                        " Base AS " +
                        " (" +
                        " SELECT " +
                        "       case when CAST(B.[Date] AS time)< @par3EndDate then CAST(B.[Date] - 1 AS date) else CAST(B.[Date] AS date) end TARIH, " +
                        "          LEFT(" +
                        "            CONVERT(varchar, B.[Date], 114), " +
                        "            2" +
                        "          )+ ':00' as SAAT, " +
                        "   B.ProductId" +
                        "  ,B.ProductName" +
                        "  ,P.ProductName PName" +
                        "  ,B.Date PaymentTime" +
                        "  ,B.Quantity" +
                        "    ,b.Options" +
                        "  ,CASE WHEN CHARINDEX('x', LTRIM(RTRIM(O.s))) > 0 THEN SUBSTRING(LTRIM(RTRIM(O.s)), CHARINDEX('x', LTRIM(RTRIM(O.s))) + 1, 100) ELSE LTRIM(RTRIM(O.s)) END AS Opt" +
                        "  ,CASE WHEN CHARINDEX('x', LTRIM(RTRIM(O.s))) > 0 THEN SUBSTRING(LTRIM(RTRIM(O.s)), 1, CHARINDEX('x', LTRIM(RTRIM(O.s))) - 1) ELSE '1' END AS OptQty" +
                        "  ,B.HeaderId" +
                        "  ,p.ProductGroup" +
                        " FROM Bill B" +
                        " LEFT JOIN BillHeader bh ON B.HeaderId = bh.Id" +
                        "    LEFT JOIN Product P ON B.ProductId = P.Id" +
                        "    CROSS APPLY dbo.splitstring(',', B.Options) AS O" +
                        " WHERE ISNULL(B.Options, '') <> '' " +
                        "  AND B.Date BETWEEN @par1 AND @par2" +
                        "  and (BillType=0 or BillType=2) " +
                        " )," +
                        " BillPrice AS" +
                        " (" +
                        " SELECT" +
                        "   Bp.ProductName" +
                        "  ,Bp.Price" +
                        "  ,Bp.MinDate" +
                        "  ,Bp.Options" +
                        "  ,CASE WHEN Bp.MaxDate=MAX(Bp.MaxDate) OVER(PARTITION BY Bp.ProductName) THEN GETDATE() ELSE Bp.MaxDate END MaxDate " +
                        " FROM" +
                        "     (" +
                        "      SELECT " +
                        "         ProductName" +
                        "        ,Price" +
                        "		,Options" +
                        "        ,MIN(Date) MinDate" +
                        "        ,MAX(Date) MaxDate " +
                        "      FROM Bill B " +
                        "      GROUP BY ProductName,Price,Options" +
                        "     ) Bp" +
                        " )," +
                        " OptSatislar AS" +
                        " (" +
                        " SELECT " +
                        " TARIH" +
                        " ,SAAT" +
                        "  ,Oe.EquProduct" +
                        "  ,Oe.Miktar" +
                        "  ,B.ProductName" +
                        "  ,B.HeaderId" +
                        "  ,B.PaymentTime" +
                        "  ,B.ProductId" +
                        "  ,Quantity * (CASE WHEN ISNUMERIC(OptQty) = 1 THEN CAST(OptQty AS INT) ELSE 1 END) AS Quantity" +
                        "  ,Opt AS OptionsName" +
                        "  ,B.ProductName aaa" +
                        "    ,B.ProductGroup " +
                        "  ,ISNULL(Oe.EquProduct, opt) ProductName2" +
                        "  ,case when ISNULL(Oe.MenuFiyat,0)=0 THEN  CASE WHEN oe.AnaUrun=1 THEN MAX(Bp.Price) ELSE ISNULL(Oe.MenuFiyat,0) END ELSE ISNULL(Oe.MenuFiyat,0) END  MenuFiyat" +
                        " FROM Base B" +
                        "     LEFT JOIN OptionsEqu Oe On Oe.ProductName+Oe.Options=B.PName+ISNULL(B.Opt, '')" +
                        "     LEFT JOIN BillPrice Bp On B.ProductName+ISNULL(b.Options,'')=Bp.ProductName +ISNULL(bp.Options,'')" +
                        "	  AND B.PaymentTime BETWEEN Bp.MinDate and Bp.MaxDate" +
                        "	 	 	  where (select top 1 Category from Options where [name]=b.Opt) <>'RAPORDISI' OR  (opt not like '%istemiyorum%' and opt not like '%istiyorum%')" +
                        " GROUP BY " +
                        " TARIH" +
                        " ,SAAT" +
                        "  ,Oe.EquProduct" +
                        "  ,Oe.Miktar" +
                        "  ,oe.AnaUrun" +
                        "  ,B.ProductName" +
                        "   ,B.ProductGroup " +
                        "  ,B.HeaderId" +
                        "  ,B.PaymentTime" +
                        "  ,B.ProductId" +
                        "  ,Oe.MenuFiyat" +
                        "  ,Quantity * (CASE WHEN ISNUMERIC(OptQty) = 1 THEN CAST(OptQty AS INT) ELSE 1 END)" +
                        "  ,Opt" +
                        " )" +
                        "  ,opttotal" +
                        " AS (" +
                        "  SELECT TARIH,SAAT,optsatislar.ProductId" +
                        "    ,ISNULL(EquProduct, optsatislar.ProductName) BillProductName" +
                        "    ,ISNULL(EquProduct, optsatislar.OptionsName ) ProductName" +
                        "    , ProductGroup" +
                        "    ,'' InvoiceName" +
                        "    ,sum(Quantity) OrjToplam" +
                        "    ,sum(Quantity * ISNULL(Miktar, 1)) Toplam" +
                        "    ,(sum(MenuFiyat)) * sum(Quantity) OrjTutar" +
                        "    ,CASE WHEN sum(MenuFiyat) = 0 AND SUM(ISNULL(Miktar, 0)) > 0 THEN 0 ELSE sum(optsatislar.MenuFiyat*Quantity) END Tutar" +
                        "  FROM optsatislar" +
                        "" + filterSaat + "" +
                        "  GROUP BY TARIH,SAAT,EquProduct" +
                        "    ,optsatislar.ProductName" +
                        "    ,optsatislar.OptionsName" +
                        "    ,ProductGroup," +
                        "    optsatislar.ProductId" +
                        "  )" +
                        "  ,indirim" +
                        " AS (" +
                        "  SELECT TOP 9999999999 ISNULL(avg(ISNULL(b2.discount, 1)), 1) / (CASE WHEN sum(Quantity * Price) = 0 THEN 1 ELSE sum(Quantity * Price) END) indirimtutar" +
                        "    ,HeaderId indHeaderId" +
                        "  FROM PaidBill b2" +
                        "  WHERE PaymentTime >= @par1" +
                        "  GROUP BY HeaderId" +
                        "  )" +
                        "  ,paketindirim" +
                        " AS (" +
                        "  SELECT TOP 9999999 ISNULL(ph.discount, 0) / (" +
                        "      SELECT sum(Quantity * Price)" +
                        "      FROM bill b2" +
                        "      WHERE b2.HeaderId = ph.HeaderId" +
                        "      ) pktindirim" +
                        "    ,HeaderId pktheaderId" +
                        "  FROM PhoneOrderHeader ph" +
                        "  )" +
                        "  ,satislar2" +
                        " AS (" +
                        "  SELECT " +
                        "         case when CAST(B.[Date] AS time)< @par3EndDate then CAST(B.[Date] - 1 AS date) else CAST(B.[Date] AS date) end TARIH, " +
                        "          LEFT(" +
                        "            CONVERT(varchar, B.[Date], 114), " +
                        "            2" +
                        "          )+ ':00' as SAAT, " +
                        "  pq.EquProductName" +
                        "    ,P.Id AS ProductId" +
                        "    ,SUM(Quantity * ISNULL(pq.Multiplier, 1)) AS Toplam" +
                        "	,SUM(Quantity ) AS Toplam2" +
                        "    ,ISNULL(pq.EquProductName, B.ProductName) ProductName" +
                        "    ,P.ProductGroup" +
                        "    ,SUM((ISNULL(Quantity, 0) * CASE WHEN ISNULL(pq.Multiplier, 1)=0 THEN 1 ELSE ISNULL(pq.Multiplier, 1) END  ) * ISNULL(B.Price, 0)) AS Tutar" +
                        "    ,(" +
                        "      SELECT SUM(Bill.Price * Bill.Quantity)" +
                        "      FROM dbo.BillWithHeader AS Bill" +
                        "      WHERE BillState = 0 AND HeaderId = b.HeaderId AND ProductId = b.ProductId" +
                        "      ) AS OPENTABLE" +
                        "    ,(" +
                        "      SELECT SUM(Bill.Quantity)" +
                        "      FROM dbo.BillWithHeader AS Bill" +
                        "      WHERE BillState = 0 AND HeaderId = b.HeaderId AND ProductId = b.ProductId" +
                        "      ) AS OPENTABLEQuantity" +
                        "    ,0 AS Discount" +
                        "    ,SUM((ISNULL(Quantity, 0) * ISNULL(pq.Multiplier, 1)) * ISNULL(B.Price, 0)) * (avg(ind.indirimtutar)) INDIRIM" +
                        "    ,SUM((ISNULL(Quantity, 0) * ISNULL(pq.Multiplier, 1)) * ISNULL(B.Price, 0)) * avg(pkt.pktindirim) PAKETINDIRIM" +
                        "    ,CASE WHEN sum(ISNULL(B.Price, 0)) = 0 THEN sum((ISNULL(B.OriginalPrice, 0))) ELSE 0 END IKRAM" +
                        "  FROM dbo.Bill AS b WITH (NOLOCK)" +
                        "  LEFT JOIN proequ pq ON pq.ProductName = B.ProductName" +
                        "  LEFT JOIN dbo.Product AS P ON P.Id = B.ProductId" +
                        "  LEFT JOIN indirim AS ind ON ind.indHeaderId = b.HeaderId" +
                        "  LEFT JOIN paketindirim AS pkt ON pkt.pktheaderId = b.HeaderId" +
                        "  LEFT JOIN dbo.BillHeader AS bh ON bh.Id=b.HeaderId" +
                        "  WHERE [Date] >= @par1 AND [Date] <= @par2 AND P.ProductName NOT LIKE '$%'" +
                        "  and (BillType=0 or BillType=2) " +
                        "  GROUP BY pq.EquProductName" +
                        "    ,B.ProductName" +
                        "    ,b.ProductId" +
                        "    ,P.ProductGroup" +
                        "    ,B.HeaderId" +
                        "    ,p.Id" +
                        "	  ,LEFT(" +
                        "            CONVERT(varchar, [Date], 114), " +
                        "            2" +
                        "          )+ ':00', " +
                        "          [Date]" +
                        "  )" +
                        " SELECT " +
                        " sum(MIKTAR) MIKTAR" +
                        "  ,a.ProductGroup" +
                        "  ,Sum(TUTAR) TUTAR" +
                        "  ,SUM(INDIRIM) INDIRIM" +
                        "  ,SUM(IKRAM) IKRAM" +
                        " FROM (" +
                        "  SELECT CASE WHEN sum(opttotal.Toplam) = 0 THEN sum(opttotal.OrjToplam) ELSE sum(opttotal.Toplam) END AS MIKTAR,TARIH" +
                        "    ,ProductName" +
                        "    ,ProductGroup AS ProductGroup" +
                        "    ,sum(ISNULL(opttotal.Tutar, 0)) AS TUTAR" +
                        "    ,0 INDIRIM" +
                        "    ,0 IKRAM" +
                        "  FROM opttotal" +
                        "  GROUP BY TARIH,opttotal.ProductName" +
                        " ,opttotal.ProductGroup" +
                        "  UNION" +
                        "  " +
                        "  SELECT sum(toplam) AS MIKTAR,TARIH" +
                        "    ,ProductName" +
                        "    ,ProductGroup" +
                        "    ,sum(tutar) AS TUTAR" +
                        "    ,SUM(ISNULL(INDIRIM, 0)) + SUM(ISNULL(PAKETINDIRIM, 0)) INDIRIM" +
                        "    ,SUM(IKRAM) IKRAM" +
                        "  FROM satislar2" +
                        "" + filterSaat + "" +
                        "  GROUP BY TARIH,ProductName" +
                        "    ,ProductGroup" +
                        "    ,ProductId" +
                        "  ) AS a" +
                        " where MIKTAR<>0 " +
                        " AND a.TARIH=@par1" +
                        " " + filterUrun + "" +
                        " " + filterUrunGrubu + "" +
                        " GROUP BY a.ProductGroup" +
                        " ORDER BY a.ProductGroup";
                    }
                    else if (AppDbType == "1" && DetayRaporMu == true)
                    {
                        Query =
                               " DECLARE @Sube nvarchar(100) ='{SUBEADI}';" +
                               " DECLARE @par1 nvarchar(20) = '{TARIH1}';" +
                               " DECLARE @par2 nvarchar(20) = '{TARIH2}';" +
                               " DECLARE @par3EndDate nvarchar(20) = '{EndDate}';" +
                               " ;WITH" +
                               " ProEqu AS " +
                               " (" +
                               " SELECT ISNULL(pq.ProductName, '') + ISNULL('.' + (" +
                               "      SELECT name" +
                               "      FROM Choice1 ch1" +
                               "      WHERE ch1.Id = pq.choice1Id" +
                               "      ), '') + ISNULL('.' + (" +
                               "      SELECT name" +
                               "      FROM Choice2 ch2" +
                               "      WHERE ch2.Id = pq.choice2Id" +
                               "      ), '') ProductName" +
                               "  ,Multiplier" +
                               "  ,EquProductName" +
                               " FROM ProductEqu Pq" +
                               " )" +
                               " ,optequ AS " +
                               " (" +
                               " SELECT oq.ProductName" +
                               "  ,(oq.ProductName + oq.Options) oqproname" +
                               "  ,EquProduct" +
                               "  ,miktar" +
                               "  ,AnaUrun" +
                               " FROM OptionsEqu oq" +
                               " )," +
                               " Base AS " +
                               " (" +
                               " SELECT " +
                               "       case when CAST(B.[Date] AS time)< @par3EndDate then CAST(B.[Date] - 1 AS date) else CAST(B.[Date] AS date) end TARIH, " +
                               "          LEFT(" +
                               "            CONVERT(varchar, B.[Date], 114), " +
                               "            2" +
                               "          )+ ':00' as SAAT, " +
                               "   B.ProductId" +
                               "  ,B.ProductName" +
                               "  ,P.ProductName PName" +
                               "  ,B.Date PaymentTime" +
                               "  ,B.Quantity" +
                               "    ,b.Options" +
                               "  ,CASE WHEN CHARINDEX('x', LTRIM(RTRIM(O.s))) > 0 THEN SUBSTRING(LTRIM(RTRIM(O.s)), CHARINDEX('x', LTRIM(RTRIM(O.s))) + 1, 100) ELSE LTRIM(RTRIM(O.s)) END AS Opt" +
                               "  ,CASE WHEN CHARINDEX('x', LTRIM(RTRIM(O.s))) > 0 THEN SUBSTRING(LTRIM(RTRIM(O.s)), 1, CHARINDEX('x', LTRIM(RTRIM(O.s))) - 1) ELSE '1' END AS OptQty" +
                               "  ,B.HeaderId" +
                               "  ,p.ProductGroup" +
                               " FROM Bill B" +
                               " LEFT JOIN BillHeader bh ON B.HeaderId = bh.Id" +
                               "    LEFT JOIN Product P ON B.ProductId = P.Id" +
                               "    CROSS APPLY dbo.splitstring(',', B.Options) AS O" +
                               " WHERE ISNULL(B.Options, '') <> '' " +
                               "  AND B.Date BETWEEN @par1 AND @par2" +
                               "  and (BillType=0 or BillType=2) " +
                               " )," +
                               " BillPrice AS" +
                               " (" +
                               " SELECT" +
                               "   Bp.ProductName" +
                               "  ,Bp.Price" +
                               "  ,Bp.MinDate" +
                               "  ,Bp.Options" +
                               "  ,CASE WHEN Bp.MaxDate=MAX(Bp.MaxDate) OVER(PARTITION BY Bp.ProductName) THEN GETDATE() ELSE Bp.MaxDate END MaxDate " +
                               " FROM" +
                               "     (" +
                               "      SELECT " +
                               "         ProductName" +
                               "        ,Price" +
                               "		,Options" +
                               "        ,MIN(Date) MinDate" +
                               "        ,MAX(Date) MaxDate " +
                               "      FROM Bill B " +
                               "      GROUP BY ProductName,Price,Options" +
                               "     ) Bp" +
                               " )," +
                               " OptSatislar AS" +
                               " (" +
                               " SELECT " +
                               " TARIH" +
                               " ,SAAT" +
                               "  ,Oe.EquProduct" +
                               "  ,Oe.Miktar" +
                               "  ,B.ProductName" +
                               "  ,B.HeaderId" +
                               "  ,B.PaymentTime" +
                               "  ,B.ProductId" +
                               "  ,Quantity * (CASE WHEN ISNUMERIC(OptQty) = 1 THEN CAST(OptQty AS INT) ELSE 1 END) AS Quantity" +
                               "  ,Opt AS OptionsName" +
                               "  ,B.ProductName aaa" +
                               "  ,B.ProductGroup " +
                               "  ,ISNULL(Oe.EquProduct, opt) ProductName2" +
                               "  ,case when ISNULL(Oe.MenuFiyat,0)=0 THEN  CASE WHEN oe.AnaUrun=1 THEN MAX(Bp.Price) ELSE ISNULL(Oe.MenuFiyat,0) END ELSE ISNULL(Oe.MenuFiyat,0) END  MenuFiyat" +
                               " FROM Base B" +
                               "     LEFT JOIN OptionsEqu Oe On Oe.ProductName+Oe.Options=B.PName+ISNULL(B.Opt, '')" +
                               "     LEFT JOIN BillPrice Bp On B.ProductName+ISNULL(b.Options,'')=Bp.ProductName +ISNULL(bp.Options,'')" +
                               "	  AND B.PaymentTime BETWEEN Bp.MinDate and Bp.MaxDate" +
                               "	 	 	  where (select top 1 Category from Options where [name]=b.Opt) <>'RAPORDISI' OR  (opt not like '%istemiyorum%' and opt not like '%istiyorum%')" +
                               " GROUP BY " +
                               "   TARIH" +
                               "  ,SAAT" +
                               "  ,Oe.EquProduct" +
                               "  ,Oe.Miktar" +
                               "  ,oe.AnaUrun" +
                               "  ,B.ProductName" +
                               "  ,B.ProductGroup " +
                               "  ,B.HeaderId" +
                               "  ,B.PaymentTime" +
                               "  ,B.ProductId" +
                               "  ,Oe.MenuFiyat" +
                               "  ,Quantity * (CASE WHEN ISNUMERIC(OptQty) = 1 THEN CAST(OptQty AS INT) ELSE 1 END)" +
                               "  ,Opt" +
                               " )" +
                               "  ,opttotal" +
                               " AS (" +
                               "  SELECT TARIH,SAAT,optsatislar.ProductId" +
                               "    ,ISNULL(EquProduct, optsatislar.ProductName) BillProductName" +
                               "    ,ISNULL(EquProduct, optsatislar.OptionsName ) ProductName" +
                               "    , ProductGroup" +
                               "    ,'' InvoiceName" +
                               "    ,sum(Quantity) OrjToplam" +
                               "    ,sum(Quantity * ISNULL(Miktar, 1)) Toplam" +
                               "    ,(sum(MenuFiyat)) * sum(Quantity) OrjTutar" +
                               "    ,CASE WHEN sum(MenuFiyat) = 0 AND SUM(ISNULL(Miktar, 0)) > 0 THEN 0 ELSE sum(optsatislar.MenuFiyat*Quantity) END Tutar" +
                               "  FROM optsatislar" +
                               " " + filterSaat + " " +
                               "  GROUP BY TARIH,SAAT,EquProduct" +
                               "    ,optsatislar.ProductName" +
                               "    ,optsatislar.OptionsName" +
                               "    ," +
                               "    ProductGroup," +
                               "    optsatislar.ProductId" +
                               "  )" +
                               "  ,indirim" +
                               " AS (" +
                               "  SELECT TOP 9999999999 ISNULL(avg(ISNULL(b2.discount, 1)), 1) / (CASE WHEN sum(Quantity * Price) = 0 THEN 1 ELSE sum(Quantity * Price) END) indirimtutar" +
                               "    ,HeaderId indHeaderId" +
                               "  FROM PaidBill b2" +
                               "  WHERE PaymentTime >= @par1" +
                               "  GROUP BY HeaderId" +
                               "  )" +
                               "  ,paketindirim" +
                               " AS (" +
                               "  SELECT TOP 9999999 ISNULL(ph.discount, 0) / (" +
                               "      SELECT sum(Quantity * Price)" +
                               "      FROM bill b2" +
                               "      WHERE b2.HeaderId = ph.HeaderId" +
                               "      ) pktindirim" +
                               "    ,HeaderId pktheaderId" +
                               "  FROM PhoneOrderHeader ph" +
                               "  )" +
                               "  ,satislar2" +
                               " AS (" +
                               "  SELECT " +
                               "         case when CAST(B.[Date] AS time)< @par3EndDate then CAST(B.[Date] - 1 AS date) else CAST(B.[Date] AS date) end TARIH, " +
                               "          LEFT(" +
                               "            CONVERT(varchar, B.[Date], 114), " +
                               "            2" +
                               "          )+ ':00' as SAAT, " +
                               "  pq.EquProductName" +
                               "    ,P.Id AS ProductId" +
                               "    ,SUM(Quantity * ISNULL(pq.Multiplier, 1)) AS Toplam" +
                               "	,SUM(Quantity ) AS Toplam2" +
                               "    ,ISNULL(pq.EquProductName, B.ProductName) ProductName" +
                               "    ,P.ProductGroup" +
                               "    ,SUM((ISNULL(Quantity, 0) * CASE WHEN ISNULL(pq.Multiplier, 1)=0 THEN 1 ELSE ISNULL(pq.Multiplier, 1) END  ) * ISNULL(B.Price, 0)) AS Tutar" +
                               "    ,(" +
                               "      SELECT SUM(Bill.Price * Bill.Quantity)" +
                               "      FROM dbo.BillWithHeader AS Bill" +
                               "      WHERE BillState = 0 AND HeaderId = b.HeaderId AND ProductId = b.ProductId" +
                               "      ) AS OPENTABLE" +
                               "    ,(" +
                               "      SELECT SUM(Bill.Quantity)" +
                               "      FROM dbo.BillWithHeader AS Bill" +
                               "      WHERE BillState = 0 AND HeaderId = b.HeaderId AND ProductId = b.ProductId" +
                               "      ) AS OPENTABLEQuantity" +
                               "    ,0 AS Discount" +
                               "    ,SUM((ISNULL(Quantity, 0) * ISNULL(pq.Multiplier, 1)) * ISNULL(B.Price, 0)) * (avg(ind.indirimtutar)) INDIRIM" +
                               "    ,SUM((ISNULL(Quantity, 0) * ISNULL(pq.Multiplier, 1)) * ISNULL(B.Price, 0)) * avg(pkt.pktindirim) PAKETINDIRIM" +
                               "    ,CASE WHEN sum(ISNULL(B.Price, 0)) = 0 THEN sum((ISNULL(B.OriginalPrice, 0))) ELSE 0 END IKRAM" +
                               "  FROM dbo.Bill AS b WITH (NOLOCK)" +
                               "  LEFT JOIN proequ pq ON pq.ProductName = B.ProductName" +
                               "  LEFT JOIN dbo.Product AS P ON P.Id = B.ProductId" +
                               "  LEFT JOIN indirim AS ind ON ind.indHeaderId = b.HeaderId" +
                               "  LEFT JOIN paketindirim AS pkt ON pkt.pktheaderId = b.HeaderId" +
                               "  LEFT JOIN dbo.BillHeader AS bh ON bh.Id=b.HeaderId" +
                               "  WHERE [Date] >= @par1 AND [Date] <= @par2 AND P.ProductName NOT LIKE '$%'" +
                               "  " +
                               "  and (BillType=0 or BillType=2) " +
                               "  GROUP BY pq.EquProductName" +
                               "    ,B.ProductName" +
                               "    ,b.ProductId" +
                               "    ,P.ProductGroup" +
                               "    ,B.HeaderId" +
                               "    ,p.Id" +
                               "	  ,LEFT(" +
                               "            CONVERT(varchar, [Date], 114), " +
                               "            2" +
                               "          )+ ':00', " +
                               "          [Date]" +
                               "  )" +
                               " SELECT " +
                               " sum(MIKTAR) MIKTAR" +
                               "  ,a.ProductName" +
                               "  ,Sum(TUTAR) TUTAR" +
                               "  ,SUM(INDIRIM) INDIRIM" +
                               "  ,SUM(IKRAM) IKRAM" +
                               " FROM (" +
                               "  SELECT CASE WHEN sum(opttotal.Toplam) = 0 THEN sum(opttotal.OrjToplam) ELSE sum(opttotal.Toplam) END AS MIKTAR,TARIH" +
                               "    ,ProductName" +
                               "    ,ProductGroup AS ProductGroup" +
                               "    ,sum(ISNULL(opttotal.Tutar, 0)) AS TUTAR" +
                               "    ,0 INDIRIM" +
                               "    ,0 IKRAM" +
                               "  FROM opttotal" +
                               "  GROUP BY TARIH,opttotal.ProductName" +
                               " ,opttotal.ProductGroup     " +
                               "  UNION" +
                               "  " +
                               "  SELECT sum(toplam) AS MIKTAR,TARIH" +
                               "    ,ProductName" +
                               "    ,ProductGroup" +
                               "    ,sum(tutar) AS TUTAR" +
                               "    ,SUM(ISNULL(INDIRIM, 0)) + SUM(ISNULL(PAKETINDIRIM, 0)) INDIRIM" +
                               "    ,SUM(IKRAM) IKRAM" +
                               "  FROM satislar2" +
                               " " + filterSaat + " " +
                               "  GROUP BY TARIH,ProductName" +
                               "    ,ProductGroup" +
                               "    ,ProductId" +
                               "  ) AS a" +
                               " where MIKTAR<>0 " +
                               " AND a.TARIH>=@par1 and a.TARIH<=@par2 " +
                               " " + filterUrunGrubu + "" +
                               " " + filterUrun + " " +
                               " GROUP BY a.ProductName" +
                               " ORDER BY a.ProductName";

                        //    {
                        //        Query = " DECLARE @Sube nvarchar(100) ='{SUBEADI}';" +
                        //                " DECLARE @par1 nvarchar(20) = '{TARIH1}';" +
                        //                " DECLARE @par2 nvarchar(20) = '{TARIH2}';" +
                        //                "DECLARE @EndDate nvarchar(20) = '07:00';" +
                        //                ";WITH" +
                        //                " ProEqu AS " +
                        //                " (" +
                        //                " SELECT ISNULL(pq.ProductName, '') + ISNULL('.' + (" +
                        //                "      SELECT name" +
                        //                "      FROM Choice1 ch1" +
                        //                "      WHERE ch1.Id = pq.choice1Id" +
                        //                "      ), '') + ISNULL('.' + (" +
                        //                "      SELECT name" +
                        //                "      FROM Choice2 ch2" +
                        //                "      WHERE ch2.Id = pq.choice2Id" +
                        //                "      ), '') ProductName" +
                        //                "  ,Multiplier" +
                        //                "  ,EquProductName" +
                        //                " FROM ProductEqu Pq" +
                        //                " )" +
                        //                " ,optequ AS " +
                        //                " (" +
                        //                " SELECT oq.ProductName" +
                        //                "  ,(oq.ProductName + oq.Options) oqproname" +
                        //                "  ,EquProduct" +
                        //                "  ,miktar" +
                        //                "  ,AnaUrun" +
                        //                " FROM OptionsEqu oq" +
                        //                " )," +
                        //                " Base AS " +
                        //                " (" +
                        //                " SELECT " +
                        //                "       case when CAST(B.[Date] AS time)< '07:00:00' then CAST(B.[Date] - 1 AS date) else CAST(B.[Date] AS date) end TARIH, " +
                        //                "          LEFT(" +
                        //                "            CONVERT(varchar, B.[Date], 114), " +
                        //                "            2" +
                        //                "          )+ ':00' as SAAT, " +
                        //                "   B.ProductId" +
                        //                "  ,B.ProductName" +
                        //                "  ,P.ProductName PName" +
                        //                "  ,B.Date PaymentTime" +
                        //                "  ,B.Quantity" +
                        //                "    ,b.Options" +
                        //                "  ,CASE WHEN CHARINDEX('x', LTRIM(RTRIM(O.s))) > 0 THEN SUBSTRING(LTRIM(RTRIM(O.s)), CHARINDEX('x', LTRIM(RTRIM(O.s))) + 1, 100) ELSE LTRIM(RTRIM(O.s)) END AS Opt" +
                        //                "  ,CASE WHEN CHARINDEX('x', LTRIM(RTRIM(O.s))) > 0 THEN SUBSTRING(LTRIM(RTRIM(O.s)), 1, CHARINDEX('x', LTRIM(RTRIM(O.s))) - 1) ELSE '1' END AS OptQty" +
                        //                "  ,B.HeaderId" +
                        //                "  ,p.ProductGroup" +
                        //                " FROM Bill B" +
                        //                " LEFT JOIN BillHeader bh ON B.HeaderId = bh.Id" +
                        //                "    LEFT JOIN Product P ON B.ProductId = P.Id" +
                        //                "    CROSS APPLY dbo.splitstring(',', B.Options) AS O" +
                        //                " WHERE ISNULL(B.Options, '') <> '' " +
                        //                "  AND B.Date BETWEEN @par1 AND @par2" +
                        //                " " + filterSatisTipi + "  " +
                        //                " )," +
                        //                " BillPrice AS" +
                        //                " (" +
                        //                " SELECT" +
                        //                "   Bp.ProductName" +
                        //                "  ,Bp.Price" +
                        //                "  ,Bp.MinDate" +
                        //                "  ,Bp.Options" +
                        //                "  ,CASE WHEN Bp.MaxDate=MAX(Bp.MaxDate) OVER(PARTITION BY Bp.ProductName) THEN GETDATE() ELSE Bp.MaxDate END MaxDate " +
                        //                " FROM" +
                        //                "     (" +
                        //                "      SELECT " +
                        //                "         ProductName" +
                        //                "        ,Price" +
                        //                "		,Options" +
                        //                "        ,MIN(Date) MinDate" +
                        //                "        ,MAX(Date) MaxDate " +
                        //                "      FROM Bill B " +
                        //                "      GROUP BY ProductName,Price,Options" +
                        //                "     ) Bp" +
                        //                " )," +
                        //                " OptSatislar AS" +
                        //                " (" +
                        //                " SELECT " +
                        //                " TARIH" +
                        //                " ,SAAT" +
                        //                "  ,Oe.EquProduct" +
                        //                "  ,Oe.Miktar" +
                        //                "  ,B.ProductName" +
                        //                "  ,B.HeaderId" +
                        //                "  ,B.PaymentTime" +
                        //                "  ,B.ProductId" +
                        //                "  ,Quantity * (CASE WHEN ISNUMERIC(OptQty) = 1 THEN CAST(OptQty AS INT) ELSE 1 END) AS Quantity" +
                        //                "  ,Opt AS OptionsName" +
                        //                "  ,B.ProductName aaa" +
                        //                "    ,B.ProductGroup " +
                        //                "  ,ISNULL(Oe.EquProduct, opt) ProductName2" +
                        //                "  ,case when ISNULL(Oe.MenuFiyat,0)=0 THEN  CASE WHEN oe.AnaUrun=1 THEN MAX(Bp.Price) ELSE ISNULL(Oe.MenuFiyat,0) END ELSE ISNULL(Oe.MenuFiyat,0) END  MenuFiyat" +
                        //                " FROM Base B" +
                        //                "     LEFT JOIN OptionsEqu Oe On Oe.ProductName+Oe.Options=B.PName+ISNULL(B.Opt, '')" +
                        //                "     LEFT JOIN BillPrice Bp On B.ProductName+ISNULL(b.Options,'')=Bp.ProductName +ISNULL(bp.Options,'')" +
                        //                "	  AND B.PaymentTime BETWEEN Bp.MinDate and Bp.MaxDate" +
                        //                "	 	 	  where (select top 1 Category from Options where [name]=b.Opt) <>'RAPORDISI' OR  (opt not like '%istemiyorum%' and opt not like '%istiyorum%')" +
                        //                " GROUP BY " +
                        //                " TARIH" +
                        //                " ,SAAT" +
                        //                "  ,Oe.EquProduct" +
                        //                "  ,Oe.Miktar" +
                        //                "  ,oe.AnaUrun" +
                        //                "  ,B.ProductName" +
                        //                "   ,B.ProductGroup " +
                        //                "  ,B.HeaderId" +
                        //                "  ,B.PaymentTime" +
                        //                "  ,B.ProductId" +
                        //                "  ,Oe.MenuFiyat" +
                        //                "  ,Quantity * (CASE WHEN ISNUMERIC(OptQty) = 1 THEN CAST(OptQty AS INT) ELSE 1 END)" +
                        //                "  ,Opt" +
                        //                " )" +
                        //                "  ,opttotal" +
                        //                " AS (" +
                        //                "  SELECT TARIH,SAAT,optsatislar.ProductId" +
                        //                "    ,ISNULL(EquProduct, optsatislar.ProductName) BillProductName" +
                        //                "    ,ISNULL(EquProduct, optsatislar.OptionsName ) ProductName" +
                        //                "    , ProductGroup" +
                        //                "    ,'' InvoiceName" +
                        //                "    ,sum(Quantity) OrjToplam" +
                        //                "    ,sum(Quantity * ISNULL(Miktar, 1)) Toplam" +
                        //                "    ,(sum(MenuFiyat)) * sum(Quantity) OrjTutar" +
                        //                "    ,CASE WHEN sum(MenuFiyat) = 0 AND SUM(ISNULL(Miktar, 0)) > 0 THEN 0 ELSE sum(optsatislar.MenuFiyat*Quantity) END Tutar" +
                        //                "  FROM optsatislar" +
                        //                " " + filterSaat + "" +
                        //                "  GROUP BY TARIH,SAAT,EquProduct" +
                        //                "    ,optsatislar.ProductName" +
                        //                "    ,optsatislar.OptionsName" +
                        //                "    ," +
                        //                "    ProductGroup," +
                        //                "    optsatislar.ProductId" +
                        //                "  )" +
                        //                "  ,indirim" +
                        //                " AS (" +
                        //                "  SELECT TOP 9999999999 ISNULL(avg(ISNULL(b2.discount, 1)), 1) / (CASE WHEN sum(Quantity * Price) = 0 THEN 1 ELSE sum(Quantity * Price) END) indirimtutar" +
                        //                "    ,HeaderId indHeaderId" +
                        //                "  FROM PaidBill b2" +
                        //                "  WHERE PaymentTime >= @par1" +
                        //                "  GROUP BY HeaderId" +
                        //                "  )" +
                        //                "  ,paketindirim" +
                        //                " AS (" +
                        //                "  SELECT TOP 9999999 ISNULL(ph.discount, 0) / (" +
                        //                "      SELECT sum(Quantity * Price)" +
                        //                "      FROM bill b2" +
                        //                "      WHERE b2.HeaderId = ph.HeaderId" +
                        //                "      ) pktindirim" +
                        //                "    ,HeaderId pktheaderId" +
                        //                "  FROM PhoneOrderHeader ph" +
                        //                "  )" +
                        //                "  ,satislar2" +
                        //                " AS (" +
                        //                "  SELECT " +
                        //                "         case when CAST(B.[Date] AS time)< '07:00:00' then CAST(B.[Date] - 1 AS date) else CAST(B.[Date] AS date) end TARIH, " +
                        //                "          LEFT(" +
                        //                "            CONVERT(varchar, B.[Date], 114), " +
                        //                "            2" +
                        //                "          )+ ':00' as SAAT, " +
                        //                "  pq.EquProductName" +
                        //                "    ,P.Id AS ProductId" +
                        //                "    ,SUM(Quantity * ISNULL(pq.Multiplier, 1)) AS Toplam" +
                        //                "	,SUM(Quantity ) AS Toplam2" +
                        //                "    ,ISNULL(pq.EquProductName, B.ProductName) ProductName" +
                        //                "    ,P.ProductGroup" +
                        //                "    ,SUM((ISNULL(Quantity, 0) * CASE WHEN ISNULL(pq.Multiplier, 1)=0 THEN 1 ELSE ISNULL(pq.Multiplier, 1) END  ) * ISNULL(B.Price, 0)) AS Tutar" +
                        //                "    ,(" +
                        //                "      SELECT SUM(Bill.Price * Bill.Quantity)" +
                        //                "      FROM dbo.BillWithHeader AS Bill" +
                        //                "      WHERE BillState = 0 AND HeaderId = b.HeaderId AND ProductId = b.ProductId" +
                        //                "      ) AS OPENTABLE" +
                        //                "    ,(" +
                        //                "      SELECT SUM(Bill.Quantity)" +
                        //                "      FROM dbo.BillWithHeader AS Bill" +
                        //                "      WHERE BillState = 0 AND HeaderId = b.HeaderId AND ProductId = b.ProductId" +
                        //                "      ) AS OPENTABLEQuantity" +
                        //                "    ,0 AS Discount" +
                        //                "    ,SUM((ISNULL(Quantity, 0) * ISNULL(pq.Multiplier, 1)) * ISNULL(B.Price, 0)) * (avg(ind.indirimtutar)) INDIRIM" +
                        //                "    ,SUM((ISNULL(Quantity, 0) * ISNULL(pq.Multiplier, 1)) * ISNULL(B.Price, 0)) * avg(pkt.pktindirim) PAKETINDIRIM" +
                        //                "    ,CASE WHEN sum(ISNULL(B.Price, 0)) = 0 THEN sum((ISNULL(B.OriginalPrice, 0))) ELSE 0 END IKRAM" +
                        //                "  FROM dbo.Bill AS b WITH (NOLOCK)" +
                        //                "  LEFT JOIN proequ pq ON pq.ProductName = B.ProductName" +
                        //                "  LEFT JOIN dbo.Product AS P ON P.Id = B.ProductId" +
                        //                "  LEFT JOIN indirim AS ind ON ind.indHeaderId = b.HeaderId" +
                        //                "  LEFT JOIN paketindirim AS pkt ON pkt.pktheaderId = b.HeaderId" +
                        //                "  LEFT JOIN dbo.BillHeader AS bh ON bh.Id=b.HeaderId" +
                        //                "  WHERE [Date] >= @par1 AND [Date] <= @par2 AND P.ProductName NOT LIKE '$%'" +
                        //                " " + filterSatisTipi + " " +
                        //                "  GROUP BY pq.EquProductName" +
                        //                "    ,B.ProductName" +
                        //                "    ,b.ProductId" +
                        //                "    ,P.ProductGroup" +
                        //                "    ,B.HeaderId" +
                        //                "    ,p.Id" +
                        //                "	  ,LEFT(" +
                        //                "            CONVERT(varchar, [Date], 114), " +
                        //                "            2" +
                        //                "          )+ ':00', " +
                        //                "          [Date]" +
                        //                "  )" +
                        //                " SELECT " +
                        //                " sum(MIKTAR) MIKTAR" +
                        //                "  ,a.ProductName" +
                        //                "  ,Sum(TUTAR) TUTAR" +
                        //                "  ,SUM(INDIRIM) INDIRIM" +
                        //                "  ,SUM(IKRAM) IKRAM" +
                        //                " FROM (" +
                        //                "  SELECT CASE WHEN sum(opttotal.Toplam) = 0 THEN sum(opttotal.OrjToplam) ELSE sum(opttotal.Toplam) END AS MIKTAR,TARIH" +
                        //                "    ,ProductName" +
                        //                "    ,ProductGroup AS ProductGroup" +
                        //                "    ,sum(ISNULL(opttotal.Tutar, 0)) AS TUTAR" +
                        //                "    ,0 INDIRIM" +
                        //                "    ,0 IKRAM" +
                        //                "  FROM opttotal" +
                        //                "  GROUP BY TARIH,opttotal.ProductName" +
                        //                " ,opttotal.ProductGroup" +
                        //                "  " +
                        //                "  UNION" +
                        //                "  " +
                        //                "  SELECT sum(toplam) AS MIKTAR,TARIH" +
                        //                "    ,ProductName" +
                        //                "    ,ProductGroup" +
                        //                "    ,sum(tutar) AS TUTAR" +
                        //                "    ,SUM(ISNULL(INDIRIM, 0)) + SUM(ISNULL(PAKETINDIRIM, 0)) INDIRIM" +
                        //                "    ,SUM(IKRAM) IKRAM" +
                        //                "  FROM satislar2" +
                        //               " " + filterSaat + " " +
                        //                "  GROUP BY TARIH,ProductName" +
                        //                "    ,ProductGroup" +
                        //                "    ,ProductId" +
                        //                "  ) AS a" +
                        //                " where MIKTAR<>0 " +
                        //                " AND a.TARIH=@par1 " +
                        //                "" + filterUrunGrubu + "" +
                        //                "" + filterUrun + "" +
                        //                " GROUP BY a.ProductName" +
                        //                " ORDER BY a.ProductName";
                        //    //}
                    }
                    else if (AppDbType == "3")
                    {
                        #region Kırılımlarda Ana Şube Altındaki IND id alıyorum 
                        if (!subeId.Equals(""))
                        {
                            string[] tableNo = subeId.Split('~');
                            if (tableNo.Length >= 2)
                            {
                                subeId = tableNo[1];
                            }
                        }
                        #endregion  Kırılımlarda Ana Şube Altındaki IND id alıyorum 

                        if (AppDbTypeStatus == "True")
                        {
                            if (subeId == null && subeId.Equals("0") || subeId == "")// sube secili degilse ilk giris yapilan sql

                                #region FASTER ONLINE QUARY
                                Query =
                                        " declare @Sube nvarchar(100) = '{SUBEADI}';" +
                                        " declare @par1 nvarchar(20) = '{TARIH1}';" +
                                        " declare @par2 nvarchar(20) = '{TARIH2}';" +
                                        " (SELECT (SELECT SUBEADI FROM  " + FirmaId_SUBE + " WHERE IND=FSB.SUBEIND) AS Sube1," +
                                        " (SELECT KASAADI FROM  " + FirmaId_KASA + " WHERE IND=FSB.KASAIND) AS Kasa," +
                                        " (SELECT IND FROM  F0" + FirmaId + "TBLKRDKASALAR WHERE IND=FSB.KASAIND) AS Id," +
                                        " SUM(FSH.MIKTAR) AS MIKTAR, SUM((((MIKTAR*SATISFIYATI) * (100-ISNULL(FSH.ISK1,0))/100) * (100-ISNULL(FSH.ISK2,0))/100) * (100-ISNULL(FSB.ALTISKORAN,0))/100)  AS TUTAR " +
                                        " FROM TBLFASTERSATISHAREKET AS FSH " +
                                        " LEFT JOIN TBLFASTERSATISBASLIK AS FSB ON FSH.BASLIKIND=FSB.BASLIKIND AND FSH.SUBEIND=FSB.SUBEIND AND FSH.KASAIND=FSB.KASAIND " +
                                        " LEFT JOIN F0" + FirmaId + "TBLSTOKLAR AS STK ON FSH.STOKIND=STK.IND WHERE FSH.ISLEMTARIHI>=@par1 AND FSH.ISLEMTARIHI<=@par2 AND ISNULL(FSB.IADE,0)=0 " +
                                        " GROUP BY FSB.SUBEIND,FSB.KASAIND)";
                            #endregion FASTER ONLINE QUARY

                            if (subeId != null && !subeId.Equals("0") && subeId != "")
                                #region FASTER ONLINE QUARY
                                Query =
                                        " declare @Sube nvarchar(100) = '{SUBEADI}';" +
                                        " declare @par1 nvarchar(20) = '{TARIH1}';" +
                                        " declare @par2 nvarchar(20) = '{TARIH2}';" +
                                        " (SELECT (SELECT SUBEADI FROM  " + FirmaId_SUBE + " WHERE IND=FSB.SUBEIND) AS Sube1," +
                                        " (SELECT KASAADI FROM  " + FirmaId_KASA + " WHERE IND=FSB.KASAIND) AS Kasa," +
                                        " (SELECT IND FROM  F0" + FirmaId + "TBLKRDKASALAR WHERE IND=FSB.KASAIND) AS Id," +
                                        " SUM(FSH.MIKTAR) AS MIKTAR, SUM((((MIKTAR*SATISFIYATI) * (100-ISNULL(FSH.ISK1,0))/100) * (100-ISNULL(FSH.ISK2,0))/100) * (100-ISNULL(FSB.ALTISKORAN,0))/100)  AS TUTAR,  STK.MALINCINSI AS ProductName   " +
                                        " FROM TBLFASTERSATISHAREKET AS FSH " +
                                        " LEFT JOIN TBLFASTERSATISBASLIK AS FSB ON FSH.BASLIKIND=FSB.BASLIKIND AND FSH.SUBEIND=FSB.SUBEIND AND FSH.KASAIND=FSB.KASAIND " +
                                        " LEFT JOIN F0" + FirmaId + "TBLSTOKLAR AS STK ON FSH.STOKIND=STK.IND WHERE FSH.ISLEMTARIHI>=@par1 AND FSH.ISLEMTARIHI<=@par2 AND ISNULL(FSB.IADE,0)=0 " +
                                        " GROUP BY FSB.SUBEIND,FSB.KASAIND,STK.MALINCINSI)";
                            #endregion FASTER ONLINE QUARY
                        }
                        else
                        {
                            Query = File.ReadAllText(HostingEnvironment.MapPath("/Sql/SqlFaster/SubeUrun/SubeUrunFasterOFLINE.sql"), System.Text.UTF8Encoding.Default);
                        }
                    }
                    else if (AppDbType == "5" && DetayRaporMu == false)
                    {
                        Query = File.ReadAllText(HostingEnvironment.MapPath("/Sql/VPos/DetayRaporlar/SubeBazliCiroRaporu2.sql"), System.Text.UTF8Encoding.Default);
                    }
                    else if (AppDbType == "5" && DetayRaporMu == true)
                    {
                        Query = File.ReadAllText(HostingEnvironment.MapPath("/Sql/VPos/DetayRaporlar/SubeBazliCiroRaporu3.sql"), System.Text.UTF8Encoding.Default);
                    }

                    Query = Query.Replace("{SUBEADI}", SubeAdi);
                    Query = Query.Replace("{TARIH1}", QueryTimeStart);
                    Query = Query.Replace("{TARIH2}", QueryTimeEnd);
                    Query = Query.Replace("{EndDate}", endDate);
                    //Query = Query.Replace("{ProductName}", urunAdi);
                    Query = Query.Replace("{ProductName}", urunGrubu);
                    Query = Query.Replace("{SUBE2}", vPosSubeKodu);
                    Query = Query.Replace("{KASAKODU}", vPosKasaKodu);
                    Query = Query.Replace("{SAAT}", filterSaat);

                    //Query = Query.Replace("{ProductName}", "%" + urunAdi + "%");



                    if (id == "1")
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
                                    if (subeId.Equals(""))
                                    {
                                        if (AppDbType == "3")
                                        {

                                            #region FASTER -(AppDbType = 3 faster kullanan şube)                                           
                                            foreach (DataRow sube in SubeUrunCiroDt.Rows)
                                            {
                                                ReportsDetailViewModel items = new ReportsDetailViewModel();
                                                items.SubeID = SubeId + "~" + sube["Id"].ToString();
                                                items.Sube = SubeAdi + "-" + sube["Sube1"].ToString() + "-" + sube["Kasa"].ToString();
                                                items.Miktar += Convert.ToDecimal(sube["MIKTAR"]);//, "MIKTAR");
                                                items.Debit += Convert.ToDecimal(sube["TUTAR"]); //, "MIKTAR");f.RTD(SubeR, "TUTAR");
                                                items.ProductName = f.RTS(sube, "ProductName"); //sube["ProductName"].ToString();  //f.RTS(SubeR, "ProductName");
                                                Liste.Add(items);
                                            }
                                            #endregion FASTER -(AppDbType = 3 faster kullanan şube)
                                        }
                                        else if (AppDbType == "4")
                                        {
                                            #region NPOS -(AppDbType=4 NPOS şube)                                           
                                            foreach (DataRow sube in SubeUrunCiroDt.Rows)
                                            {
                                                ReportsDetailViewModel items = new ReportsDetailViewModel();
                                                items.SubeID = SubeId + "~" + sube["Kasa"].ToString();
                                                items.Sube = SubeAdi + "_" + sube["Sube"].ToString() + "-" + sube["Sube1"].ToString() + "-" + sube["Kasa"].ToString();
                                                items.Miktar += Convert.ToDecimal(sube["MIKTAR"]);//, "MIKTAR");
                                                items.Debit += Convert.ToDecimal(sube["TUTAR"]); //, "MIKTAR");f.RTD(SubeR, "TUTAR");
                                                items.ProductName = f.RTS(sube, "ProductName"); //sube["ProductName"].ToString();  //f.RTS(SubeR, "ProductName");
                                                Liste.Add(items);
                                            }
                                            #endregion  NPOS -(AppDbType=4 NPOS şube)    
                                        }
                                        else
                                        {
                                            #region SEFIM                                            
                                            ReportsDetailViewModel items = new ReportsDetailViewModel();
                                            items.Sube = SubeAdi; //f.RTS(SubeUrunCiroDt.Rows[0], "Sube");
                                            items.SubeID = (SubeId);
                                            if (subeId.Equals(""))
                                            {
                                                foreach (DataRow SubeR in SubeUrunCiroDt.Rows)
                                                {
                                                    items.Miktar = f.RTD(SubeR, "MIKTAR");
                                                    items.Debit = f.RTD(SubeR, "TUTAR");
                                                    items.ProductName = f.RTS(SubeR, "ProductName");
                                                    items.ProductGroup = f.RTS(SubeR, "ProductGroup");
                                                    Liste.Add(items);
                                                }
                                            }
                                            #endregion SEFIM
                                        }
                                    }
                                    else
                                    {
                                        if (AppDbType == "3")
                                        {
                                            foreach (DataRow SubeR in SubeUrunCiroDt.Rows)
                                            {
                                                if (subeId == SubeR["Id"].ToString())
                                                {
                                                    ReportsDetailViewModel items = new ReportsDetailViewModel();
                                                    items.Sube = f.RTS(SubeR, "Sube");
                                                    items.SubeID = (SubeId);
                                                    items.Miktar = f.RTD(SubeR, "MIKTAR");
                                                    items.ProductName = f.RTS(SubeR, "ProductName");
                                                    items.Debit = f.RTD(SubeR, "TUTAR");
                                                    //items.GecenZaman = Convert.ToInt32(sure.TotalSeconds);
                                                    Liste.Add(items);
                                                }
                                            }
                                        }
                                        else
                                        {
                                            foreach (DataRow SubeR in SubeUrunCiroDt.Rows)
                                            {
                                                ReportsDetailViewModel items = new ReportsDetailViewModel
                                                {
                                                    Sube = SubeAdi,
                                                    SubeID = (SubeId),
                                                    StartDate = Date1.ToString("dd.MM.yyyy"),
                                                    EndDate = Date2.ToString("dd.MM.yyyy"),
                                                    SaatList = saatList,
                                                    FilterUrunAdiList = urunAdi,
                                                    Miktar = f.RTD(SubeR, "MIKTAR"),
                                                    ProductName = f.RTS(SubeR, "ProductName"),
                                                    Debit = f.RTD(SubeR, "TUTAR"),
                                                    ProductGroup = f.RTS(SubeR, "ProductGroup")
                                                };
                                                Liste.Add(items);
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    //ReportsDetailViewModel items = new ReportsDetailViewModel();
                                    //items.Sube = SubeAdi + " (Data Yok)";
                                    //items.SubeID = (SubeId);
                                    //Liste.Add(items);
                                }
                            }
                            catch (Exception ex)
                            {
                                Singleton.WritingLogFile("SaatBazliUrunGrubuVeUrunSatisReportsCRUD2_Exception", ex.Message);
                                throw new Exception(SubeAdi);
                            }
                        }
                        catch (Exception ex)
                        {
                            try
                            {
                                #region EX                            

                                Singleton.WritingLogFile("SaatBazliUrunGrubuVeUrunSatisReportsCRUD2_Exception", ex.Message);

                                ReportsDetailViewModel items = new ReportsDetailViewModel
                                {
                                    Sube = ex.Message + " (Erişim Yok)",
                                    SubeID = (SubeId)
                                };
                                #endregion
                            }
                            catch (Exception)
                            { }
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
                                        DataTable SubeUrunCiroDt = new DataTable();
                                        SubeUrunCiroDt = f.GetSubeDataWithQuery(f.NewConnectionString(SubeIP, DBName, SqlName, SqlPassword), Query.ToString());

                                        if (SubeUrunCiroDt.Rows.Count > 0)
                                        {
                                            if (subeId.Equals(""))
                                            {
                                                if (AppDbType == "3")
                                                {

                                                    #region FASTER -(AppDbType = 3 faster kullanan şube)                                           
                                                    foreach (DataRow sube in SubeUrunCiroDt.Rows)
                                                    {
                                                        ReportsDetailViewModel items = new ReportsDetailViewModel
                                                        {
                                                            SubeID = SubeId + "~" + sube["Id"].ToString(),
                                                            Sube = SubeAdi + "-" + sube["Sube1"].ToString() + "-" + sube["Kasa"].ToString()
                                                        };
                                                        items.Miktar += Convert.ToDecimal(sube["MIKTAR"]);//, "MIKTAR");
                                                        items.Debit += Convert.ToDecimal(sube["TUTAR"]); //, "MIKTAR");f.RTD(SubeR, "TUTAR");
                                                        items.ProductName = f.RTS(sube, "ProductName"); //sube["ProductName"].ToString();  //f.RTS(SubeR, "ProductName");
                                                        Liste.Add(items);
                                                    }
                                                    #endregion FASTER -(AppDbType = 3 faster kullanan şube)
                                                }
                                                else if (AppDbType == "4")
                                                {
                                                    #region NPOS -(AppDbType=4 NPOS şube)                                           
                                                    foreach (DataRow sube in SubeUrunCiroDt.Rows)
                                                    {
                                                        ReportsDetailViewModel items = new ReportsDetailViewModel
                                                        {
                                                            SubeID = SubeId + "~" + sube["Kasa"].ToString(),
                                                            Sube = SubeAdi + "_" + sube["Sube"].ToString() + "-" + sube["Sube1"].ToString() + "-" + sube["Kasa"].ToString()
                                                        };
                                                        items.Miktar += Convert.ToDecimal(sube["MIKTAR"]);//, "MIKTAR");
                                                        items.Debit += Convert.ToDecimal(sube["TUTAR"]); //, "MIKTAR");f.RTD(SubeR, "TUTAR");
                                                        items.ProductName = f.RTS(sube, "ProductName"); //sube["ProductName"].ToString();  //f.RTS(SubeR, "ProductName");
                                                        Liste.Add(items);
                                                    }
                                                    #endregion  NPOS -(AppDbType=4 NPOS şube)    
                                                }
                                                else
                                                {
                                                    #region SEFIM                                            
                                                    ReportsDetailViewModel items = new ReportsDetailViewModel
                                                    {
                                                        Sube = SubeAdi, //f.RTS(SubeUrunCiroDt.Rows[0], "Sube");
                                                        SubeID = (SubeId)
                                                    };
                                                    if (subeId.Equals(""))
                                                    {
                                                        foreach (DataRow SubeR in SubeUrunCiroDt.Rows)
                                                        {
                                                            items.Miktar = f.RTD(SubeR, "MIKTAR");
                                                            items.Debit = f.RTD(SubeR, "TUTAR");
                                                            items.ProductName = f.RTS(SubeR, "ProductName");
                                                            items.ProductGroup = f.RTS(SubeR, "ProductGroup");
                                                            Liste.Add(items);
                                                        }
                                                    }
                                                    #endregion SEFIM
                                                }
                                            }
                                            else
                                            {
                                                if (AppDbType == "3")
                                                {
                                                    foreach (DataRow SubeR in SubeUrunCiroDt.Rows)
                                                    {
                                                        if (subeId == SubeR["Id"].ToString())
                                                        {
                                                            ReportsDetailViewModel items = new ReportsDetailViewModel
                                                            {
                                                                Sube = f.RTS(SubeR, "Sube"),
                                                                SubeID = (SubeId),
                                                                Miktar = f.RTD(SubeR, "MIKTAR"),
                                                                ProductName = f.RTS(SubeR, "ProductName"),
                                                                Debit = f.RTD(SubeR, "TUTAR")
                                                            };
                                                            //items.GecenZaman = Convert.ToInt32(sure.TotalSeconds);
                                                            Liste.Add(items);
                                                        }
                                                    }
                                                }
                                                else
                                                {
                                                    foreach (DataRow SubeR in SubeUrunCiroDt.Rows)
                                                    {
                                                        ReportsDetailViewModel items = new ReportsDetailViewModel
                                                        {
                                                            Sube = SubeAdi,
                                                            SubeID = (SubeId),
                                                            StartDate = Date1.ToString("dd.MM.yyyy"),
                                                            EndDate = Date2.ToString("dd.MM.yyyy"),
                                                            SaatList = saatList,
                                                            FilterUrunAdiList = urunAdi,
                                                            Miktar = f.RTD(SubeR, "MIKTAR"),
                                                            ProductName = f.RTS(SubeR, "ProductName"),
                                                            Debit = f.RTD(SubeR, "TUTAR"),
                                                            ProductGroup = f.RTS(SubeR, "ProductGroup")
                                                        };
                                                        Liste.Add(items);
                                                    }
                                                }
                                            }
                                        }
                                        else
                                        {
                                            //ReportsDetailViewModel items = new ReportsDetailViewModel();
                                            //items.Sube = SubeAdi + " (Data Yok)";
                                            //items.SubeID = (SubeId);
                                            //Liste.Add(items);
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        Singleton.WritingLogFile("SaatBazliUrunGrubuVeUrunSatisReportsCRUD2_Exception", ex.Message);
                                        throw new Exception(SubeAdi);
                                    }
                                }
                                catch (Exception ex)
                                {
                                    try
                                    {
                                        #region EX                            

                                        Singleton.WritingLogFile("SaatBazliUrunGrubuVeUrunSatisReportsCRUD2_Exception", ex.Message);

                                        ReportsDetailViewModel items = new ReportsDetailViewModel
                                        {
                                            Sube = ex.Message + " (Erişim Yok)",
                                            SubeID = (SubeId)
                                        };
                                        #endregion
                                    }
                                    catch (Exception)
                                    { }
                                }
                                #endregion
                            }
                        }
                        #endregion
                    }
                });
                #endregion PARALLEL FORECH

            }
            catch (DataException ex)
            {
                Singleton.WritingLogFile("SaatBazliUrunGrubuVeUrunSatisReportsCRUD2_Exception", ex.Message);
            }

            return Liste;
        }
    }
}