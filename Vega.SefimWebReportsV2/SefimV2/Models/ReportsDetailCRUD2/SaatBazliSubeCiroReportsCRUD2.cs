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
    public class SaatBazliSubeCiroReportsCRUD2
    {
        public static List<KasaCiro> List(DateTime Date1, DateTime Date2, string subeId, string endDate, string saatList, string ID, string urunGrubu, string urunAdi, string satisTipi, bool DetayRaporMu, bool chartMi)
        {
            Date2 = Date2.AddDays(1);

            List<KasaCiro> Liste = new List<KasaCiro>();
            ModelFunctions ff = new ModelFunctions();
            #region GİRİŞ YAPAN KULLANICININ YETKİLİ OLDUĞU ŞUBELER LİSTELENİYOR
            UserViewModel model = UsersListCRUD.YetkiliSubesi(ID);
            #endregion
            try
            {
                #region SUBSTATION LIST               
                //Şube filter
                string[] subeSplit = subeId.Split(',');
                string filter = "Where  Status=1";
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
                            filterSaat += " SAAT='" + new DateTime(TimeSpan.Parse(saatSplit[i]).Ticks).ToString("HH:mm") + "' )";
                        }
                        else
                        {
                            filterSaat += " SAAT='" + new DateTime(TimeSpan.Parse(saatSplit[i]).Ticks).ToString("HH:mm") + "' OR  ";
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


                ff.SqlConnOpen();
                DataTable dt = ff.DataTable("select * from SubeSettings  " + filter);
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

                try
                {
                    #region PARALEL FOREACH
                    //foreach (DataRow r in dt.AsEnumerable())
                    //{
                    var dtList = dt.AsEnumerable().ToList<DataRow>();
                    Parallel.ForEach(dtList, r =>
                    {
                        ModelFunctions f = new ModelFunctions();
                        string AppDbType = f.RTS(r, "AppDbType");
                        string AppDbTypeStatus = f.RTS(r, "AppDbTypeStatus"); //AppDbTypeStatus=True ise Faster>online
                        string SubeId = r["Id"].ToString();
                        string SubeAdi = r["SubeName"].ToString();
                        string SubeIP = r["SubeIP"].ToString();
                        string SqlName = r["SqlName"].ToString();
                        string SqlPassword = r["SqlPassword"].ToString();
                        string DBName = r["DBName"].ToString();
                        string QueryTimeStart = Date1.ToString("yyyy/MM/dd HH:mm:ss");
                        string QueryTimeEnd = Date2.ToString("yyyy/MM/dd HH:mm:ss");
                        string FirmaId_SUBE = "F0" + r["FirmaID"].ToString() + "TBLKRDSUBELER"; //Online Faster kullanan şubede Firma ve kasa Id set ediliyor.
                        string FirmaId_KASA = "F0" + r["FirmaID"].ToString() + "TBLKRDKASALAR";
                        string Firma_NPOS = r["FirmaID"].ToString();
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
                        #region  SEFİM YENI - ESKİ - FASTER SQL

                        string query = string.Empty;

                        if (AppDbType == "1" && chartMi == true)
                        {
                            //query = " DECLARE @Sube nvarchar(100) ='{SUBEADI}';" +
                            //        " DECLARE @Tar1 nvarchar(20) = '{TARIH1}';" +
                            //        " DECLARE @Tar2 nvarchar(20) = '{TARIH2}';" +
                            //        " DECLARE @EndDate nvarchar(20) = '{EndDate}';" +
                            //        "  Select t.TARIH,t.SAAT,sum(t.TUTAR) TUTAR from (" +
                            //        "  SELECT " +
                            //        "  case when CAST([Date] AS time)<'07:00:00' then CAST([Date]-1 AS date) else CAST([Date] AS date) end  TARIH," +
                            //        "  LEFT(CONVERT(varchar,[Date],114),2)+':00' as SAAT" +
                            //        "  ,sum([Price]*[Quantity]) AS TUTAR" +
                            //        "  FROM [dbo].[BillWithHeader] " +
                            //        "  where [Date]>= @Tar1  and [Date] <= @Tar2" +
                            //        "  Group by LEFT(CONVERT(varchar,[Date],114),2)+':00'" +
                            //        ", [Date] ) as t" +
                            //        "  " + filterSaat + " " +
                            //        "  group by " +
                            //        "  t.TARIH,t.SAAT ";

                            ////--Bir önceki query--//
                            //query = " DECLARE @Sube nvarchar(100) ='{SUBEADI}';" +
                            //        " DECLARE @Tar1 nvarchar(20) = '{TARIH1}';" +
                            //        " DECLARE @Tar2 nvarchar(20) = '{TARIH2}';" +
                            //        " DECLARE @EndDate nvarchar(20) = '{EndDate}';" +
                            //        " select t.TARIH,t.SAAT,sum(t.TUTAR) TUTAR from ( Select t.TARIH,t.SAAT,sum(t.TUTAR) " +
                            //        " TUTAR from ( " +
                            //        " SELECT   case when CAST(B.[Date] AS time)<'07:00:00'  then CAST(B.[Date]-1 AS date) else CAST(B.[Date] AS date) end  TARIH, " +
                            //        " LEFT(CONVERT(varchar,B.[Date],114),2)+':00' as SAAT  , sum(B.[Price]*B.[Quantity]) AS TUTAR " +
                            //        " FROM [dbo].[BillWithHeader] AS B" +
                            //        " LEFT JOIN  Product as P ON P.Id=B.ProductId   and  B.[Date]>= @Tar1  and B.[Date] <= @Tar2" +
                            //        " where B.[Date]>= @Tar1  and B.[Date] <= @Tar2  " +
                            //        " " + filterUrun + filterUrunGrubu + " " +
                            //        " Group by LEFT(CONVERT(varchar,[Date],114),2)+':00',  [Date] " +
                            //        " ) as t  " +
                            //        " group by   t.TARIH,t.SAAT " +
                            //        " ) as t " +
                            //          " " + filterSaat + " " +
                            //        " GROUP BY  t.TARIH, t.SAAT";


                            query =
                                   " DECLARE @Sube nvarchar(100) ='{SUBEADI}';" +
                                   " DECLARE @par1 nvarchar(20) = '{TARIH1}';" +
                                   " DECLARE @par2 nvarchar(20) = '{TARIH2}';" +
                                   " DECLARE @par3EndDate nvarchar(20) = '{EndDate}';" +

                                   " WITH ProEqu AS (" +
                                    "  SELECT " +
                                    "    ISNULL(pq.ProductName, '') + ISNULL(" +
                                    "      '.' + (" +
                                    "        SELECT " +
                                    "          name " +
                                    "        FROM " +
                                    "          Choice1 ch1 " +
                                    "        WHERE " +
                                    "          ch1.Id = pq.choice1Id" +
                                    "      ), " +
                                    "      ''" +
                                    "    ) + ISNULL(" +
                                    "      '.' + (" +
                                    "        SELECT " +
                                    "          name " +
                                    "        FROM " +
                                    "          Choice2 ch2 " +
                                    "        WHERE " +
                                    "          ch2.Id = pq.choice2Id" +
                                    "      ), " +
                                    "      ''" +
                                    "    ) ProductName, " +
                                    "    Multiplier, " +
                                    "    EquProductName " +
                                    "  FROM " +
                                    "    ProductEqu Pq" +
                                    " ), " +
                                    " optequ AS (" +
                                    "  SELECT " +
                                    "    oq.ProductName, " +
                                    "    (oq.ProductName + oq.Options) oqproname, " +
                                    "    EquProduct, " +
                                    "    miktar, " +
                                    "    AnaUrun " +
                                    "  FROM " +
                                    "    OptionsEqu oq" +
                                    " ), " +
                                    " Base AS (" +
                                    "  SELECT " +
                                    "    case when CAST(B.[Date] AS time)< @par3EndDate then CAST(B.[Date] - 1 AS date) else CAST(B.[Date] AS date) end TARIH, " +
                                    "    LEFT(" +
                                    "      CONVERT(varchar, B.[Date], 114), " +
                                    "      2" +
                                    "    )+ ':00' as SAAT, " +
                                    "    B.ProductId, " +
                                    "    B.ProductName, " +
                                    "    P.ProductName PName, " +
                                    "    B.Date PaymentTime, " +
                                    "    B.Quantity, " +
                                    "    b.Options, " +
                                    "    CASE WHEN CHARINDEX(" +
                                    "      'x', " +
                                    "      LTRIM(" +
                                    "        RTRIM(O.s)" +
                                    "      )" +
                                    "    ) > 0 THEN SUBSTRING(" +
                                    "      LTRIM(" +
                                    "        RTRIM(O.s)" +
                                    "      ), " +
                                    "      CHARINDEX(" +
                                    "        'x', " +
                                    "        LTRIM(" +
                                    "          RTRIM(O.s)" +
                                    "        )" +
                                    "      ) + 1, " +
                                    "      100" +
                                    "    ) ELSE LTRIM(" +
                                    "      RTRIM(O.s)" +
                                    "    ) END AS Opt, " +
                                    "    CASE WHEN CHARINDEX(" +
                                    "      'x', " +
                                    "      LTRIM(" +
                                    "        RTRIM(O.s)" +
                                    "      )" +
                                    "    ) > 0 THEN SUBSTRING(" +
                                    "      LTRIM(" +
                                    "        RTRIM(O.s)" +
                                    "      ), " +
                                    "      1, " +
                                    "      CHARINDEX(" +
                                    "        'x', " +
                                    "        LTRIM(" +
                                    "          RTRIM(O.s)" +
                                    "        )" +
                                    "      ) -1" +
                                    "    ) ELSE '1' END AS OptQty, " +
                                    "    B.HeaderId, " +
                                    "    p.ProductGroup " +
                                    "  FROM " +
                                    "    Bill B " +
                                    "    LEFT JOIN BillHeader bh ON B.HeaderId = bh.Id " +
                                    "    LEFT JOIN Product P ON B.ProductId = P.Id CROSS APPLY dbo.splitstring(',', B.Options) AS O " +
                                    "  WHERE " +
                                    "    ISNULL(B.Options, '') <> '' " +
                                    "    AND B.Date BETWEEN @par1 AND @par2" +
                                      "  " + filterSatisTipi + "  " +
                                    " ), " +
                                    " BillPrice AS(" +
                                    "  SELECT " +
                                    "    Bp.ProductName, " +
                                    "    Bp.Price, " +
                                    "    Bp.MinDate, " +
                                    "    Bp.Options, " +
                                    "    CASE WHEN Bp.MaxDate = MAX(Bp.MaxDate) OVER(PARTITION BY Bp.ProductName) THEN GETDATE() ELSE Bp.MaxDate END MaxDate " +
                                    "  FROM " +
                                    "    (" +
                                    "      SELECT " +
                                    "        ProductName, " +
                                    "        Price, " +
                                    "        Options, " +
                                    "        MIN(Date) MinDate, " +
                                    "        MAX(Date) MaxDate " +
                                    "      FROM " +
                                    "        Bill B " +
                                    "      GROUP BY " +
                                    "        ProductName, " +
                                    "        Price, " +
                                    "        Options" +
                                    "    ) Bp" +
                                    " ), " +
                                    " OptSatislar AS(" +
                                    "  SELECT " +
                                    "    TARIH, " +
                                    "    SAAT, " +
                                    "    Oe.EquProduct, " +
                                    "    Oe.Miktar, " +
                                    "    B.ProductName, " +
                                    "    B.HeaderId, " +
                                    "    B.PaymentTime, " +
                                    "    B.ProductId, " +
                                    "    Quantity * (" +
                                    "      CASE WHEN ISNUMERIC(OptQty) = 1 THEN CAST(OptQty AS INT) ELSE 1 END" +
                                    "    ) AS Quantity, " +
                                    "    Opt AS OptionsName, " +
                                    "    B.ProductName aaa, " +
                                    "    B.ProductGroup, " +
                                    "    ISNULL(Oe.EquProduct, opt) ProductName2, " +
                                    "    case when ISNULL(Oe.MenuFiyat, 0)= 0 THEN CASE WHEN oe.AnaUrun = 1 THEN MAX(Bp.Price) ELSE ISNULL(Oe.MenuFiyat, 0) END ELSE ISNULL(Oe.MenuFiyat, 0) END MenuFiyat " +
                                    "  FROM " +
                                    "    Base B " +
                                    "    LEFT JOIN OptionsEqu Oe On Oe.ProductName + Oe.Options = B.PName + ISNULL(B.Opt, '') " +
                                    "    LEFT JOIN BillPrice Bp On B.ProductName + ISNULL(b.Options, '')= Bp.ProductName + ISNULL(bp.Options, '') " +
                                    "    AND B.PaymentTime BETWEEN Bp.MinDate " +
                                    "    and Bp.MaxDate " +
                                    "  where " +
                                    "    (" +
                                    "      select " +
                                    "        top 1 Category " +
                                    "      from " +
                                    "        Options " +
                                    "      where " +
                                    "        [name] = b.Opt" +
                                    "    ) <> 'RAPORDISI' " +
                                    "    OR (" +
                                    "      opt not like '%istemiyorum%' " +
                                    "      and opt not like '%istiyorum%'" +
                                    "    ) " +
                                    "  GROUP BY " +
                                    "    TARIH, " +
                                    "    SAAT, " +
                                    "    Oe.EquProduct, " +
                                    "    Oe.Miktar, " +
                                    "    oe.AnaUrun, " +
                                    "    B.ProductName, " +
                                    "    B.ProductGroup, " +
                                    "    B.HeaderId, " +
                                    "    B.PaymentTime, " +
                                    "    B.ProductId, " +
                                    "    Oe.MenuFiyat, " +
                                    "    Quantity * (" +
                                    "      CASE WHEN ISNUMERIC(OptQty) = 1 THEN CAST(OptQty AS INT) ELSE 1 END" +
                                    "    ), " +
                                    "    Opt" +
                                    " ), " +
                                    " opttotal AS (" +
                                    "  SELECT " +
                                    "    TARIH, " +
                                    "    SAAT, " +
                                    "    optsatislar.ProductId, " +
                                    "    ISNULL(" +
                                    "      EquProduct, optsatislar.ProductName" +
                                    "    ) BillProductName, " +
                                    "    ISNULL(" +
                                    "      EquProduct, optsatislar.OptionsName" +
                                    "    ) ProductName, " +
                                    "    ProductGroup, " +
                                    "    '' InvoiceName, " +
                                    "    sum(Quantity) OrjToplam, " +
                                    "    sum(" +
                                    "      Quantity * ISNULL(Miktar, 1)" +
                                    "    ) Toplam, " +
                                    "    (" +
                                    "      sum(MenuFiyat)" +
                                    "    ) * sum(Quantity) OrjTutar, " +
                                    "    CASE WHEN sum(MenuFiyat) = 0 " +
                                    "    AND SUM(" +
                                    "      ISNULL(Miktar, 0)" +
                                    "    ) > 0 THEN 0 ELSE sum(optsatislar.MenuFiyat * Quantity) END Tutar " +
                                    "  FROM   optsatislar " +
                                      " " + filterSaat + " " +
                                    "  GROUP BY " +
                                    "    TARIH, " +
                                    "    SAAT, " +
                                    "    EquProduct, " +
                                    "    optsatislar.ProductName, " +
                                    "    optsatislar.OptionsName, " +
                                    "    ProductGroup, " +
                                    "    optsatislar.ProductId" +
                                    " ), " +
                                    " indirim AS (" +
                                    "  SELECT " +
                                    "    TOP 9999999999 ISNULL(" +
                                    "      avg(" +
                                    "        ISNULL(b2.discount, 1)" +
                                    "      ), " +
                                    "      1" +
                                    "    ) / (" +
                                    "      CASE WHEN sum(Quantity * Price) = 0 THEN 1 ELSE sum(Quantity * Price) END" +
                                    "    ) indirimtutar, " +
                                    "    HeaderId indHeaderId " +
                                    "  FROM " +
                                    "    PaidBill b2 " +
                                    "  WHERE " +
                                    "    PaymentTime >= @par1 " +
                                    "  GROUP BY " +
                                    "    HeaderId" +
                                    " ), " +
                                    " paketindirim AS (" +
                                    "  SELECT " +
                                    "    TOP 9999999 ISNULL(ph.discount, 0) / (" +
                                    "     SELECT CASE WHEN sum(Quantity * Price)=0 THEN 1 ELSE sum(Quantity * Price) END " +
                                    "      FROM " +
                                    "        bill b2 " +
                                    "      WHERE " +
                                    "        b2.HeaderId = ph.HeaderId" +
                                    "    ) pktindirim, " +
                                    "    HeaderId pktheaderId " +
                                    "  FROM " +
                                    "    PhoneOrderHeader ph" +
                                    " ), " +
                                    " satislar2 AS (" +
                                    "  SELECT " +
                                    "    case when CAST(B.[Date] AS time)<  @par3EndDate then CAST(B.[Date] - 1 AS date) else CAST(B.[Date] AS date) end TARIH, " +
                                    "    LEFT(" +
                                    "      CONVERT(varchar, B.[Date], 114), " +
                                    "      2" +
                                    "    )+ ':00' as SAAT, " +
                                    "    pq.EquProductName, " +
                                    "    P.Id AS ProductId, " +
                                    "    SUM(" +
                                    "      Quantity * ISNULL(pq.Multiplier, 1)" +
                                    "    ) AS Toplam, " +
                                    "    SUM(Quantity) AS Toplam2, " +
                                    "    ISNULL(" +
                                    "      pq.EquProductName, B.ProductName" +
                                    "    ) ProductName, " +
                                    "    P.ProductGroup, " +
                                    "    SUM(" +
                                    "      (" +
                                    "        ISNULL(Quantity, 0) * CASE WHEN ISNULL(pq.Multiplier, 1)= 0 THEN 1 ELSE ISNULL(pq.Multiplier, 1) END" +
                                    "      ) * ISNULL(B.Price, 0)" +
                                    "    ) AS Tutar, " +
                                    "    (" +
                                    "      SELECT " +
                                    "        SUM(Bill.Price * Bill.Quantity) " +
                                    "      FROM " +
                                    "        dbo.BillWithHeader AS Bill " +
                                    "      WHERE " +
                                    "        BillState = 0 " +
                                    "        AND HeaderId = b.HeaderId " +
                                    "        AND ProductId = b.ProductId" +
                                    "    ) AS OPENTABLE, " +
                                    "    (" +
                                    "      SELECT " +
                                    "        SUM(Bill.Quantity) " +
                                    "      FROM " +
                                    "        dbo.BillWithHeader AS Bill " +
                                    "      WHERE " +
                                    "        BillState = 0 " +
                                    "        AND HeaderId = b.HeaderId " +
                                    "        AND ProductId = b.ProductId" +
                                    "    ) AS OPENTABLEQuantity, " +
                                    "    0 AS Discount, " +
                                    "    SUM(" +
                                    "      (" +
                                    "        ISNULL(Quantity, 0) * ISNULL(pq.Multiplier, 1)" +
                                    "      ) * ISNULL(B.Price, 0)" +
                                    "    ) * (" +
                                    "      avg(ind.indirimtutar)" +
                                    "    ) INDIRIM, " +
                                    "    SUM(" +
                                    "      (" +
                                    "        ISNULL(Quantity, 0) * ISNULL(pq.Multiplier, 1)" +
                                    "      ) * ISNULL(B.Price, 0)" +
                                    "    ) * avg(pkt.pktindirim) PAKETINDIRIM, " +
                                    "    CASE WHEN sum(" +
                                    "      ISNULL(B.Price, 0)" +
                                    "    ) = 0 THEN sum(" +
                                    "      (" +
                                    "        ISNULL(B.OriginalPrice, 0)" +
                                    "      )" +
                                    "    ) ELSE 0 END IKRAM " +
                                    "  FROM " +
                                    "    dbo.Bill AS b WITH (NOLOCK) " +
                                    "    LEFT JOIN proequ pq ON pq.ProductName = B.ProductName " +
                                    "    LEFT JOIN dbo.Product AS P ON P.Id = B.ProductId " +
                                    "    LEFT JOIN indirim AS ind ON ind.indHeaderId = b.HeaderId " +
                                    "    LEFT JOIN paketindirim AS pkt ON pkt.pktheaderId = b.HeaderId " +
                                    "    LEFT JOIN dbo.BillHeader AS bh ON bh.Id = b.HeaderId " +
                                    "  WHERE " +
                                    "    [Date] >= @par1 AND [Date] <= @par2 AND P.ProductName NOT LIKE '$%' " +
                                     "  " + filterSatisTipi + "  " +
                                    "  GROUP BY " +
                                    "    pq.EquProductName, " +
                                    "    B.ProductName, " +
                                    "    b.ProductId, " +
                                    "    P.ProductGroup, " +
                                    "    B.HeaderId, " +
                                    "    p.Id, " +
                                    "    LEFT(" +
                                    "      CONVERT(varchar, [Date], 114), " +
                                    "      2" +
                                    "    )+ ':00', " +
                                    "    [Date]" +
                                    " ) " +
                                    " SELECT " +
                                    "  sum(MIKTAR) MIKTAR, " +
                                    "  SAAT," +
                                    "  a.TARIH, " +
                                    "  Sum(TUTAR) TUTAR, " +
                                    "  SUM(INDIRIM) INDIRIM, " +
                                    "  SUM(IKRAM) IKRAM " +
                                    " FROM " +
                                    "  (" +
                                    "    SELECT " +
                                    "      CASE WHEN sum(opttotal.Toplam) = 0 THEN sum(opttotal.OrjToplam) ELSE sum(opttotal.Toplam) END AS MIKTAR, " +
                                    "      TARIH, " +
                                    "	    SAAT," +
                                    "      ProductName, " +
                                    "      ProductGroup AS ProductGroup, " +
                                    "      sum(" +
                                    "        ISNULL(opttotal.Tutar, 0)" +
                                    "      ) AS TUTAR, " +
                                    "      0 INDIRIM, " +
                                    "      0 IKRAM " +
                                    "    FROM " +
                                    "      opttotal " +
                                    "    GROUP BY " +
                                    "      TARIH," +
                                    "	  SAAT," +
                                    "      opttotal.ProductName, " +
                                    "      opttotal.ProductGroup " +
                                    "    UNION " +
                                    "    SELECT " +
                                    "      sum(toplam) AS MIKTAR, " +
                                    "      TARIH, " +
                                    "	    SAAT," +
                                    "      ProductName, " +
                                    "      ProductGroup, " +
                                    "      sum(tutar) AS TUTAR, " +
                                    "      SUM(" +
                                    "        ISNULL(INDIRIM, 0)" +
                                    "      ) + SUM(" +
                                    "        ISNULL(PAKETINDIRIM, 0)" +
                                    "      ) INDIRIM, " +
                                    "      SUM(IKRAM) IKRAM " +
                                    "    FROM   satislar2 " +
                                    " " + filterSaat + " " +
                                    "    GROUP BY " +
                                    "      TARIH, " +
                                    "	    SAAT," +
                                    "      ProductName, " +
                                    "      ProductGroup, " +
                                    "      ProductId" +
                                    "  ) AS a " +
                                    " where " +
                                    "  MIKTAR <> 0 " +
                                    " " + filterUrun + " " +
                                    " " + filterUrunGrubu + " " +
                                    " GROUP BY " +
                                    "  a.TARIH " +
                                    "  ,a.SAAT" +
                                    " ORDER BY " +
                                    "  a.TARIH" +
                                    "  ,a.SAAT";


                        }
                        else if (AppDbType == "1" && DetayRaporMu == false)// 1 = yeni şefim, 2 =eski Şefim, 3 = Faster
                        {
                            //Yeni eklendi saat bazli rapor çekiyor. (28.07.2022)
                            //Query = File.ReadAllText(HostingEnvironment.MapPath("/Sql/SqlReports/SubeCiroReports2/SaatBazliSubeCiroReports2.sql"), System.Text.Encoding.UTF8);
                            //query = " DECLARE @Sube nvarchar(100) ='{SUBEADI}';" +
                            //        " DECLARE @Tar1 nvarchar(20) = '{TARIH1}';" +
                            //        " DECLARE @Tar2 nvarchar(20) = '{TARIH2}';" +
                            //        " DECLARE @EndDate nvarchar(20) = '{EndDate}';" +
                            //        "  Select t.TARIH,t.SAAT,sum(t.TUTAR) TUTAR from (" +
                            //        "  SELECT " +
                            //        "  case when CAST([Date] AS time)<'07:00:00' then CAST([Date]-1 AS date) else CAST([Date] AS date) end  TARIH," +
                            //        "  LEFT(CONVERT(varchar,[Date],114),2)+':00' as SAAT" +
                            //        "  ,sum([Price]*[Quantity]) AS TUTAR" +
                            //        "  FROM [dbo].[BillWithHeader] " +
                            //        "  where [Date]>= @Tar1  and [Date] <= @Tar2" +
                            //        "  Group by LEFT(CONVERT(varchar,[Date],114),2)+':00'" +
                            //        ", [Date] ) as t" +
                            //        "  " + filterSaat + " " +
                            //        "  group by " +
                            //        "  t.TARIH,t.SAAT ";
                            //query = " DECLARE @Sube nvarchar(100) ='{SUBEADI}';" +
                            //        " DECLARE @Tar1 nvarchar(20) = '{TARIH1}';" +
                            //        " DECLARE @Tar2 nvarchar(20) = '{TARIH2}';" +
                            //        " DECLARE @EndDate nvarchar(20) = '{EndDate}';" +
                            //        " select t.TARIH,sum(t.TUTAR) TUTAR from (" +
                            //        " Select t.TARIH,t.SAAT,sum(t.TUTAR) TUTAR from (  SELECT   case when CAST([Date] AS time)<'07:00:00' " +
                            //        " then CAST([Date]-1 AS date) else CAST([Date] AS date) end  TARIH,  LEFT(CONVERT(varchar,[Date],114),2)+':00' as SAAT  ," +
                            //        " sum([Price]*[Quantity]) AS TUTAR  FROM [dbo].[BillWithHeader] " +
                            //        " where [Date]>= @Tar1  and [Date] <= @Tar2 " +
                            //        " Group by LEFT(CONVERT(varchar,[Date],114),2)+':00', " +
                            //        " [Date] ) as t    " +
                            //        " group by  " +
                            //        " t.TARIH,t.SAAT ) as t " +
                            //        " GROUP BY " +
                            //        " t.TARIH";

                            //query = " DECLARE @Sube nvarchar(100) ='{SUBEADI}';" +
                            //         " DECLARE @Tar1 nvarchar(20) = '{TARIH1}';" +
                            //         " DECLARE @Tar2 nvarchar(20) = '{TARIH2}';" +
                            //         " DECLARE @EndDate nvarchar(20) = '{EndDate}';" +
                            //         " select t.TARIH,sum(t.TUTAR) TUTAR from ( Select t.TARIH,t.SAAT,sum(t.TUTAR) " +
                            //         " TUTAR from ( " +
                            //         " SELECT   case when CAST(B.[Date] AS time)<'07:00:00'  then CAST(B.[Date]-1 AS date) else CAST(B.[Date] AS date) end  TARIH, " +
                            //         " LEFT(CONVERT(varchar,B.[Date],114),2)+':00' as SAAT  , sum(B.[Price]*B.[Quantity]) AS TUTAR " +
                            //         " FROM [dbo].[BillWithHeader] AS B" +
                            //         " LEFT JOIN  Product as P ON P.Id=B.ProductId   and  B.[Date]>= @Tar1  and B.[Date] <= @Tar2" +
                            //         " " +
                            //         " where B.[Date]>= @Tar1  and B.[Date] <= @Tar2  " +
                            //         " " + filterUrun + filterUrunGrubu + " " +
                            //         " Group by LEFT(CONVERT(varchar,[Date],114),2)+':00',  [Date] " +
                            //         " ) as t  " +
                            //         " group by   t.TARIH,t.SAAT " +
                            //         " ) as t " +
                            //           " " + filterSaat + " " +
                            //         " GROUP BY  t.TARIH";

                            query =
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
                            ")" +
                            ",optequ AS " +
                            "(" +
                            " SELECT oq.ProductName" +
                            "  ,(oq.ProductName + oq.Options) oqproname" +
                            "  ,EquProduct" +
                            "  ,miktar" +
                            "  ,AnaUrun" +
                            " FROM OptionsEqu oq" +
                            " )," +
                            " Base AS " +
                            "(" +
                            " SELECT " +
                            "       case when CAST(B.[Date] AS time)< @par3EndDate then CAST(B.[Date] -1 AS date) else CAST(B.[Date] AS date) end TARIH, " +
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
                            "  " + filterSatisTipi + "  " +
                            ")," +
                            " BillPrice AS" +
                            "(" +
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
                            ")," +
                            " OptSatislar AS" +
                            "(" +
                            " SELECT " +
                            "  TARIH" +
                            "  ,SAAT" +
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
                            "  GROUP BY " +
                            "  TARIH" +
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
                            ")" +
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
                            "      SELECT CASE WHEN sum(Quantity* Price)= 0 THEN 1 ELSE sum(Quantity* Price) END " +
                            "      FROM bill b2" +
                            "      WHERE b2.HeaderId = ph.HeaderId" +
                            "      ) pktindirim" +
                            "    ,HeaderId pktheaderId" +
                            "  FROM PhoneOrderHeader ph" +
                            "  )" +
                            "  ,satislar2" +
                            " AS (" +
                            "  SELECT " +
                            "         case when CAST(B.[Date] AS time)< @par3EndDate then CAST(B.[Date] -1  AS date) else CAST(B.[Date] AS date) end TARIH, " +
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
                          "  " + filterSatisTipi + "  " +
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
                            ",a.TARIH" +
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
                            "  " +
                            "   " +
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
                            " " + filterUrun + " " +
                            " " + filterUrunGrubu + " " +
                            " GROUP BY a.TARIH" +
                            " ORDER BY a.TARIH";
                        }
                        else if (AppDbType == "1" && DetayRaporMu == true)
                        {
                            //query = "  DECLARE @Sube nvarchar(100) ='{SUBEADI}';" +
                            //        "  DECLARE @Tar1 nvarchar(20) = '{TARIH1}';" +
                            //        "  DECLARE @Tar2 nvarchar(20) = '{TARIH2}';" +
                            //        "  DECLARE @EndDate nvarchar(20) = '{EndDate}';" +
                            //        "  Select t.TARIH,sum(t.TUTAR) TUTAR from (" +
                            //        "  SELECT " +
                            //        "  case when CAST([Date] AS time)<'07:00:00' then CAST([Date]-1 AS date) else CAST([Date] AS date) end  TARIH," +
                            //        "  LEFT(CONVERT(varchar,[Date],114),2)+':00' as SAAT" +
                            //        "  ,sum([Price]*[Quantity]) AS TUTAR" +
                            //        "  FROM [dbo].[BillWithHeader] " +
                            //        "  where [Date]>= @Tar1  and [Date] <= @Tar2" +
                            //        "  Group by LEFT(CONVERT(varchar,[Date],114),2)+':00'" +
                            //        ", [Date] ) as t" +
                            //        "  " + filterSaat + " " +
                            //        "  group by " +
                            //        "  t.TARIH ";

                            //query = "  DECLARE @Sube nvarchar(100) ='{SUBEADI}';" +
                            //        "  DECLARE @Tar1 nvarchar(20) = '{TARIH1}';" +
                            //        "  DECLARE @Tar2 nvarchar(20) = '{TARIH2}';" +
                            //        "  DECLARE @EndDate nvarchar(20) = '{EndDate}';" +
                            //        "  Select t.TARIH,sum(t.TUTAR) TUTAR from(" +
                            //        "  SELECT   case when CAST(B.[Date] AS time)<'07:00:00' then CAST(B.[Date]-1 AS date) else CAST(B.[Date] AS date) end  TARIH, " +
                            //        "  LEFT(CONVERT(varchar,[Date],114),2)+':00' as SAAT  ,sum( B.[Price]*B.[Quantity]) AS TUTAR  " +
                            //        "  FROM [dbo].[BillWithHeader]  AS B " +
                            //        "   LEFT JOIN  Product as P ON P.Id=B.ProductId  and  B.[Date]>= @Tar1  and B.[Date] <= @Tar2 " +
                            //        "  where B.[Date]>= @Tar1  and B.[Date] <= @Tar2  " +
                            //         " " + filterUrun + filterUrunGrubu + " " +
                            //        "  Group by LEFT(CONVERT(varchar,[Date],114),2)+':00', [Date] " +
                            //        "  ) as t" +
                            //        " " + filterSaat + " " +
                            //        "  group by   t.TARIH ";


                            query = " DECLARE @Sube nvarchar(100) ='{SUBEADI}';" +
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
                                    ")," +
                                    " Base AS " +
                                    "(" +
                                    " SELECT " +
                                    "       case when CAST(B.[Date] AS time)< @par3EndDate then CAST(B.[Date] -1  AS date) else CAST(B.[Date] AS date) end TARIH, " +
                                    "          LEFT(" +
                                    "            CONVERT(varchar, B.[Date], 114), " +
                                    "            2" +
                                    "          )+ ':00' as SAAT, " +
                                    "   B.ProductId" +
                                    "  ,B.ProductName" +
                                    "  ,P.ProductName PName" +
                                    "  ,B.Date PaymentTime" +
                                    "  ,B.Quantity" +
                                    "  ,b.Options" +
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
                                    "  " + filterSatisTipi + "  " +
                                    " )," +
                                    " BillPrice AS" +
                                    "(" +
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
                                    "		 ,Options" +
                                    "        ,MIN(Date) MinDate" +
                                    "        ,MAX(Date) MaxDate " +
                                    "      FROM Bill B " +
                                    "      GROUP BY ProductName,Price,Options" +
                                    "     ) Bp" +
                                    " )," +
                                    " OptSatislar AS" +
                                    " (" +
                                    " SELECT " +
                                    "   TARIH" +
                                    "  ,SAAT" +
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
                                    "         case when CAST(B.[Date] AS time)<  @par3EndDate then CAST(B.[Date] -1  AS date) else CAST(B.[Date] AS date) end TARIH, " +
                                    "          LEFT(" +
                                    "            CONVERT(varchar, B.[Date], 114), " +
                                    "            2" +
                                    "          )+ ':00' as SAAT, " +
                                    "  pq.EquProductName" +
                                    "    ,P.Id AS ProductId" +
                                    "    ,SUM(Quantity * ISNULL(pq.Multiplier, 1)) AS Toplam" +
                                    "	 ,SUM(Quantity ) AS Toplam2" +
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
                                    "  " + filterSatisTipi + "  " +
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
                                    "   sum(MIKTAR) MIKTAR" +
                                    "  ,a.TARIH" +
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
                                    "     " +
                                    "  UNION  " +
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
                                    " AND a.TARIH=@par1" +
                                    " " + filterUrun + " " +
                                    " " + filterUrunGrubu + " " +
                                    " GROUP BY a.TARIH" +
                                    " ORDER BY a.TARIH asc ";

                        }
                        else if (AppDbType == "2")
                        {
                            query = File.ReadAllText(HostingEnvironment.MapPath("/Sql/SqlReports/SubeCiroReports2/SaatBazliSubeCiroReports2.sql"), System.Text.Encoding.UTF8);
                        }
                        else if (AppDbType == "3")
                        {
                            if (AppDbTypeStatus == "True")
                            {
                                #region FASTER ONLINE QUARY                              
                                query =
                                    " declare @Sube nvarchar(100) = '{SUBEADI}';" +
                                    " declare @Trh1 nvarchar(20) = '{TARIH1}';" +
                                    " declare @Trh2 nvarchar(20) = '{TARIH2}';" +
                                    " WITH Toplamsatis AS " +
                                    " ( SELECT @Sube as Sube," +
                                    " (SELECT SUBEADI FROM  " + FirmaId_SUBE + " WHERE IND=TBLFASTERODEMELER.SUBEIND) AS Sube1," +
                                    " (SELECT KASAADI FROM " + FirmaId_KASA + " WHERE IND=TBLFASTERODEMELER.KASAIND) AS Kasa, ODENEN AS cash, 0 AS Credit, 0 AS Ticket, 0 AS KasaToplam " +
                                    " FROM DBO.TBLFASTERODEMELER WHERE ODEMETIPI=0 AND ISNULL(IADE,0)=0 AND ISLEMTARIHI >= @Trh1 AND ISLEMTARIHI <= @Trh2" +
                                    " UNION ALL SELECT @Sube as Sube," +
                                    " (SELECT SUBEADI FROM  " + FirmaId_SUBE + " WHERE IND=TBLFASTERODEMELER.SUBEIND) AS Sube1," +
                                    " (SELECT KASAADI FROM " + FirmaId_KASA + " WHERE IND=TBLFASTERODEMELER.KASAIND) AS Kasa, 0 AS cash, ODENEN AS Credit, 0 AS Ticket, 0 AS KasaToplam  " +
                                    " FROM DBO.TBLFASTERODEMELER WHERE ODEMETIPI=1 AND ISNULL(IADE,0)=0 AND ISLEMTARIHI >= @Trh1 AND ISLEMTARIHI <= @Trh2 " +
                                    " UNION ALL SELECT @Sube as Sube," +
                                    " (SELECT SUBEADI FROM  " + FirmaId_SUBE + " WHERE IND=TBLFASTERODEMELER.SUBEIND) AS Sube1," +
                                    " (SELECT KASAADI FROM " + FirmaId_KASA + " WHERE IND=TBLFASTERODEMELER.KASAIND) AS Kasa, 0 AS cash, 0 AS Credit,ODENEN AS Ticket,0 AS KasaToplam  " +
                                    " FROM DBO.TBLFASTERODEMELER WHERE ODEMETIPI=2 AND ISNULL(IADE,0)=0 AND ISLEMTARIHI >= @Trh1 AND ISLEMTARIHI <= @Trh2" +
                                    " UNION ALL SELECT @Sube as Sube," +
                                    " (SELECT SUBEADI FROM  " + FirmaId_SUBE + " WHERE IND=TBLFASTERKASAISLEMLERI.SUBEIND) AS Sube1," +
                                    " (SELECT KASAADI FROM " + FirmaId_KASA + " WHERE IND=TBLFASTERKASAISLEMLERI.KASAIND) AS Kasa, 0 AS cash, 0 AS Credit,0 AS Ticket, SUM(GELIR-GIDER)	 AS KasaToplam   " +
                                    " FROM DBO.TBLFASTERKASAISLEMLERI WHERE ISLEMTARIHI >= @Trh1 AND ISLEMTARIHI <= @Trh2 " +
                                    " GROUP BY SUBEIND ,KASAIND )" +
                                    " SELECT Sube,Sube1,Kasa,SUM(Cash) AS Cash ,SUM(Credit) AS Credit ,Sum(Ticket) AS Ticket ,Sum(KasaToplam) as KasaToplam,SUM(Cash+Credit+Ticket) AS ToplamCiro,0 AS Saniye,'' AS RowStyle,'' AS RowError FROM toplamsatis GROUP BY Sube,Sube1,Kasa";
                                #endregion FASTER ONLINE QUARY
                            }
                            else
                            {
                                query = File.ReadAllText(HostingEnvironment.MapPath("/Sql/SqlFaster/KasaCiro/KasaCiroFasterOFLINE.sql"), System.Text.UTF8Encoding.Default);
                            }
                        }
                        else if (AppDbType == "4")//NPOS>4
                        {
                            #region NPOS QUARY 
                            //ROW_NUMBER() OVER(ORDER BY CAST(CONVERT(VARCHAR, DATEADD(DAY, 0, DATEDIFF(DAY, 0, hr.BELGETARIH)), 100) AS DATETIME) asc) AS DayCount,
                            //CAST( CONVERT(VARCHAR, DATEADD(DAY, 0, DATEDIFF(DAY, 0, hr.BELGETARIH)), 100) AS DATETIME) as DateStrNow,
                            //CAST(DAY(hr.BELGETARIH) AS INT) as DayNumber,

                            query =
                                    " declare @Sube nvarchar(100) = '{SUBEADI}';" +
                                    " declare @Trh1 nvarchar(20) = '{TARIH1}';" +
                                    " declare @Trh2 nvarchar(20) = '{TARIH2}';" +

                                    " select " +
                                    " DayCount,DayNumber, DateStrNow, Sube,Sube1,Kasa_No as Kasa ,sUM(Nakit) AS Cash,Sum(Visa) as Credit,Sum(Ticket) as Ticket,Sum(Debit) AS Debit, Sum(Tableno) as TableNo, Sum(Discount) as Discount,Sum(Ikram) as ikram,Sum(Zayi) as Zayi," +
                                    " SUm(case when t.Iptal = 1 then t.Toplam else 0 end) as iptal," +
                                    " SUm(case when t.Iptal = 0 then t.Toplam else 0 end) as ToplamCiro " +
                                    " from( " +
                                    " SELECT " +
                                   " ROW_NUMBER() OVER(ORDER BY CAST(CONVERT(VARCHAR, DATEADD(DAY, 0, DATEDIFF(DAY, 0, hr.BELGETARIH)), 100) AS DATETIME) asc) AS DayCount," +
                                   " CAST( CONVERT(VARCHAR, DATEADD(DAY, 0, DATEDIFF(DAY, 0, hr.BELGETARIH)), 100) AS DATETIME) as DateStrNow," +
                                    " CAST(DAY(hr.BELGETARIH) AS INT) as DayNumber," +
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
                                    " where  CAST( CONVERT(VARCHAR, DATEADD(DAY, 0, DATEDIFF(DAY, 0,hr.BELGETARIH)), 100) AS DATETIME) >= @Trh1 AND  CAST( CONVERT(VARCHAR, DATEADD(DAY, 0, DATEDIFF(DAY, 0,hr.BELGETARIH)), 100) AS DATETIME) <= @Trh2 " +
                                    " GROUP BY Sicil_No,Kasa_No,hr.Belge_ID,Iptal,CAST( CONVERT(VARCHAR, DATEADD(DAY, 0, DATEDIFF(DAY, 0,hr.BELGETARIH)), 100) AS DATETIME),CAST(DAY(hr.BELGETARIH) AS INT)  ) as t " +
                                    " group by " +
                                    " Sube,Sube1,Kasa_No ,DayCount,DayNumber,DateStrNow  ";
                            #endregion NPOS QUARY 
                        }
                        if (AppDbType == "5" && chartMi == true)
                        {
                            query = File.ReadAllText(HostingEnvironment.MapPath("/Sql/VPos/DetayRaporlar/SubeBazliCiroRaporuGrafik.sql"), System.Text.UTF8Encoding.Default);
                        }
                        else if (AppDbType == "5" && DetayRaporMu == false)
                        {
                            query = File.ReadAllText(HostingEnvironment.MapPath("/Sql/VPos/DetayRaporlar/SubeBazliCiroRaporu.sql"), System.Text.UTF8Encoding.Default);
                        }
                        else if (AppDbType == "5" && DetayRaporMu == true)
                        {
                            query = File.ReadAllText(HostingEnvironment.MapPath("/Sql/VPos/DetayRaporlar/SubeBazliCiroRaporu1.sql"), System.Text.UTF8Encoding.Default);
                        }


                        #endregion

                        query = query.Replace("{SUBEADI}", SubeAdi);
                        query = query.Replace("{TARIH1}", QueryTimeStart);
                        query = query.Replace("{TARIH2}", QueryTimeEnd);
                        query = query.Replace("{EndDate}", endDate);
                        query = query.Replace("{SAAT}", filterSaat);
                        query = query.Replace("{SUBE2}", vPosSubeKodu);
                        query = query.Replace("{KASAKODU}", vPosKasaKodu);

                        if (ID == "1")
                        {
                            #region GET DATA                    
                            try
                            {
                                try
                                {
                                    DataTable KasaCiroDt = new DataTable();
                                    KasaCiroDt = f.GetSubeDataWithQuery((f.NewConnectionString(SubeIP, DBName, SqlName, SqlPassword)), query.ToString());
                                    if (KasaCiroDt.Rows.Count > 0)
                                    {
                                        if (AppDbType == "3" || AppDbType == "4")
                                        {
                                            #region FASTER (AppDbType=3 faster kullanan şube)
                                            foreach (DataRow sube in KasaCiroDt.Rows)
                                            {
                                                KasaCiro items = new KasaCiro();

                                                if (AppDbType == "3")
                                                {
                                                    items.Sube = sube["Sube"].ToString() + "-" + sube["Sube1"].ToString() + "-" + sube["Kasa"].ToString(); //KasaCiroDt.Rows[0]["Sube"].ToString();
                                                }
                                                else
                                                {
                                                    items.Sube = SubeAdi + "_" + sube["Sube"].ToString() + "-" + sube["Sube1"].ToString() + "-" + sube["Kasa"].ToString(); //KasaCiroDt.Rows[0]["Sube"].ToString();
                                                }

                                                items.SubeId = SubeId;

                                                items.ToplamCiro = sube["ToplamCiro"].ToString();
                                                items.DateStr = Convert.ToDateTime(sube["DateStrNow"]);
                                                items.DayNumber = Convert.ToInt32(sube["DayNumber"]);
                                                items.DayCount = Convert.ToInt32(sube["DayCount"]);
                                                //items.Cash = Convert.ToDecimal(sube["Cash"]);
                                                //items.Credit = Convert.ToDecimal(sube["Credit"]);
                                                //items.Ticket = Convert.ToDecimal(sube["Ticket"]);
                                                //items.Ciro = Convert.ToDecimal(sube["ToplamCiro"]);
                                                Liste.Add(items);
                                            }
                                            #endregion FASTER (AppDbType=3 faster kullanan şube)
                                        }
                                        else
                                        {
                                            foreach (DataRow sube in KasaCiroDt.Rows)
                                            {
                                                var tarih = string.Empty;
                                                if (chartMi)
                                                {
                                                    //chart
                                                }
                                                else
                                                {
                                                    tarih = Convert.ToDateTime(sube["TARIH"]).ToString("dd.MM.yyyy");
                                                }

                                                KasaCiro items = new KasaCiro
                                                {
                                                    Sube = SubeAdi,
                                                    SubeId = SubeId,
                                                    Tarih = tarih,//Convert.ToDateTime(sube["TARIH"]).ToString("dd.MM.yyyy"),
                                                    Saat = f.RTS(sube, "SAAT"),
                                                    Ciro = Convert.ToDecimal(sube["TUTAR"]),
                                                    StartDate = Date1.ToString("dd.MM.yyyy"),
                                                    EndDate = Date2.ToString("dd.MM.yyyy"),

                                                };
                                                Liste.Add(items);
                                            }
                                        }
                                    }
                                    else
                                    {
                                        KasaCiro items = new KasaCiro
                                        {
                                            Sube = SubeAdi + " (Data Yok)"
                                        };
                                        Liste.Add(items);
                                    }
                                }
                                catch (Exception ex)
                                {
                                    Singleton.WritingLogFile2("SaatBazliSubeCiroReportsCRUD2:", ex.Message, "", "");
                                    throw new Exception(SubeAdi);
                                }
                            }
                            catch (Exception ex)
                            {
                                #region EX   
                                Singleton.WritingLogFile("SaatBazliSubeCiroReportsCRUD2:", ex.Message.ToString());

                                KasaCiro items = new KasaCiro
                                {
                                    Sube = ex.Message + " (Erişim Yok) ",
                                    SubeId = "",
                                    ErrorMessage = "Kasa Raporu Alınamadı.",
                                    ErrorStatus = true,
                                    ErrorCode = "01"
                                };
                                Liste.Add(items);
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
                                            DataTable KasaCiroDt = new DataTable();
                                            KasaCiroDt = f.GetSubeDataWithQuery((f.NewConnectionString(SubeIP, DBName, SqlName, SqlPassword)), query.ToString());
                                            if (KasaCiroDt.Rows.Count > 0)
                                            {
                                                if (AppDbType == "3" || AppDbType == "4")
                                                {
                                                    #region FASTER (AppDbType=3 faster kullanan şube)
                                                    foreach (DataRow sube in KasaCiroDt.Rows)
                                                    {
                                                        KasaCiro items = new KasaCiro();

                                                        if (AppDbType == "3")
                                                        {
                                                            items.Sube = sube["Sube"].ToString() + "-" + sube["Sube1"].ToString() + "-" + sube["Kasa"].ToString(); //KasaCiroDt.Rows[0]["Sube"].ToString();
                                                        }
                                                        else
                                                        {
                                                            items.Sube = SubeAdi + "_" + sube["Sube"].ToString() + "-" + sube["Sube1"].ToString() + "-" + sube["Kasa"].ToString(); //KasaCiroDt.Rows[0]["Sube"].ToString();
                                                        }

                                                        items.SubeId = SubeId;

                                                        items.ToplamCiro = sube["ToplamCiro"].ToString();
                                                        items.DateStr = Convert.ToDateTime(sube["DateStrNow"]);
                                                        items.DayNumber = Convert.ToInt32(sube["DayNumber"]);
                                                        items.DayCount = Convert.ToInt32(sube["DayCount"]);
                                                        //items.Cash = Convert.ToDecimal(sube["Cash"]);
                                                        //items.Credit = Convert.ToDecimal(sube["Credit"]);
                                                        //items.Ticket = Convert.ToDecimal(sube["Ticket"]);
                                                        //items.Ciro = Convert.ToDecimal(sube["ToplamCiro"]);
                                                        Liste.Add(items);
                                                    }
                                                    #endregion FASTER (AppDbType=3 faster kullanan şube)
                                                }
                                                else
                                                {
                                                    foreach (DataRow sube in KasaCiroDt.Rows)
                                                    {
                                                        var tarih = string.Empty;
                                                        if (chartMi)
                                                        {
                                                            //chart
                                                        }
                                                        else
                                                        {
                                                            tarih = Convert.ToDateTime(sube["TARIH"]).ToString("dd.MM.yyyy");
                                                        }

                                                        KasaCiro items = new KasaCiro
                                                        {
                                                            Sube = SubeAdi,
                                                            SubeId = SubeId,
                                                            Tarih = tarih,//Convert.ToDateTime(sube["TARIH"]).ToString("dd.MM.yyyy"),
                                                            Saat = f.RTS(sube, "SAAT"),
                                                            Ciro = Convert.ToDecimal(sube["TUTAR"]),
                                                            StartDate = Date1.ToString("dd.MM.yyyy"),
                                                            EndDate = Date2.ToString("dd.MM.yyyy"),

                                                        };
                                                        Liste.Add(items);
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                KasaCiro items = new KasaCiro
                                                {
                                                    Sube = SubeAdi + " (Data Yok)"
                                                };
                                                Liste.Add(items);
                                            }
                                        }
                                        catch (Exception ex)
                                        {
                                            Singleton.WritingLogFile2("SaatBazliSubeCiroReportsCRUD2:", ex.Message, "", "");
                                            throw new Exception(SubeAdi);
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        #region EX   
                                        Singleton.WritingLogFile("SaatBazliSubeCiroReportsCRUD2:", ex.Message.ToString());

                                        KasaCiro items = new KasaCiro
                                        {
                                            Sube = ex.Message + " (Erişim Yok) ",
                                            SubeId = "",
                                            ErrorMessage = "Kasa Raporu Alınamadı.",
                                            ErrorStatus = true,
                                            ErrorCode = "01"
                                        };
                                        Liste.Add(items);
                                        #endregion
                                    }
                                    #endregion
                                }
                            }
                            #endregion
                        }
                    });
                    //}
                    #endregion PARALEL FOREACH
                }
                catch (Exception ex) { Singleton.WritingLogFile("SaatBazliSubeCiroReportsCRUD2:", ex.Message.ToString()); }
            }
            catch (DataException ex) { Singleton.WritingLogFile("SaatBazliSubeCiroReportsCRUD2:", ex.Message.ToString()); }

            return Liste;
        }
    }
}