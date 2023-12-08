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
    public class HamMaddeKullanimCRUD
    {
        public static List<HamMaddeKullanimViewModel> List(DateTime Date1, DateTime Date2, string subeId, string ID)
        {
            List<HamMaddeKullanimViewModel> Liste = new List<HamMaddeKullanimViewModel>();
            ModelFunctions ff = new ModelFunctions();

            #region GİRİŞ YAPAN KULLANICININ YETKİLİ OLDUĞU ŞUBELER LİSTELENİYOR
            UserViewModel model = UsersListCRUD.YetkiliSubesi(ID);
            #endregion
            //string subeid_ = string.Empty;
            try
            {
                //#region SUBSTATION LIST       
                //ff.SqlConnOpen();
                //DataTable dt = ff.DataTable("SELECT * FROM SubeSettings WHERE Status=1  ");
                //var dtList = dt.AsEnumerable().ToList<DataRow>();
                //ff.SqlConnClose();
                //#endregion SUBSTATION LIST

                #region SUBSTATION LIST                
                ff.SqlConnOpen();
                string filter = "Where Status=1";
                if (subeId != null && !subeId.Equals("0") && !subeId.Equals(""))
                    filter += " and Id=" + subeId;
                DataTable dt = ff.DataTable("Select * From SubeSettings " + filter);
                var dtList = dt.AsEnumerable().ToList<DataRow>();
                ff.SqlConnClose();
                #endregion SUBSTATION LIST

                #region VEGA DB database ismini çekmek için.
                string vegaDb = string.Empty;
                ff.SqlConnOpen();
                DataTable dataVegaDb = ff.DataTable("select* from VegaDbSettings ");
                var vegaDBList = dataVegaDb.AsEnumerable().ToList<DataRow>();
                foreach (var item in vegaDBList)
                {
                    vegaDb = item["DBName"].ToString();
                }
                ff.SqlConnClose();
                #endregion VEGA DB database ismini çekmek için.  

                try
                {
                    #region Şubeler Listeleniyor.  
                    if (ID == "SubeList")
                    {
                        foreach (DataRow r in dt.Rows)
                        {
                            HamMaddeKullanimViewModel items = new HamMaddeKullanimViewModel
                            {
                                SubeId = r["Id"].ToString(),
                                Sube = r["SubeName"].ToString()
                            };
                            Liste.Add(items);
                        }
                        return Liste;
                    }
                    #endregion Şubeler Listeleniyor.

                    #region PARALEL FORECH

                    var locked = new Object();
                    int DonemId = 0;
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
                        string Firma_NPOS = r["FirmaID"].ToString();
                        string DepoId = f.RTS(r, "DepoID");
                        string QueryTimeStart = Date1.ToString("yyyy/MM/dd HH:mm:ss");
                        string QueryTimeEnd = Date2.ToString("yyyy/MM/dd HH:mm:ss");
                        DonemId = f.RTI(r, "DonemID");

                        #region GetAktifDonem
                        string aktifDonem = string.Empty;
                        int uzunluk = 0;
                        int sayi = DonemId;
                        while (sayi > 0)
                        {
                            uzunluk++;
                            sayi = sayi / 10;
                        }
                        if (uzunluk == 1)
                        {
                            aktifDonem = "D000" + DonemId;
                        }
                        else if (uzunluk == 2)
                        {
                            aktifDonem = "D00" + DonemId;
                        }
                        #endregion GetAktifDonem

                        #region  SEFİM YENI - ESKİ FASTER SQL

                        string AppDbType = f.RTS(r, "AppDbType");
                        string AppDbTypeStatus = f.RTS(r, "AppDbTypeStatus");
                        string Query = string.Empty;

                        #region Faster Kasalar seçildiyse ona göre query oluşturuluyor. 
                        string FasterSubeIND = f.RTS(r, "FasterSubeID");
                        string QueryFasterSube = string.Empty;
                        if (FasterSubeIND != null)
                        {
                            QueryFasterSube = "  and SUBEIND IN(" + FasterSubeIND + ") ";
                        }
                        #endregion Faster Kasalar seçildiyse ona göre query oluşturuluyor.

                        if (AppDbType == "1" || AppDbType == "2")// 1 = yeni şefim, 2 =eski Şefim, 3 = Faster
                        {
                            //Query = File.ReadAllText(HostingEnvironment.MapPath("/Sql/SqlNewSef/SubeToplamCiroNewSefim.sql"), System.Text.UTF8Encoding.Default);
                            //Query =
                            // " Declare @par1 nvarchar(20) = '{TARIH1}';" +
                            // " Declare @par2 nvarchar(20) = '{TARIH2}';" +
                            // " WITH" +
                            // " ent as (" +
                            // " SELECT " +
                            // "      [pkeyValue]" +
                            // "  FROM [entegrasyon].[dbo].[Integration] where dbname like '" + DBName + "%' and izahat in (96,97)" +
                            // "  group by " +
                            // "    [pkeyValue])," +
                            // " pro as" +
                            // " (select " +
                            // " ISNULL(P.ProductName,'')+ISNULL('.'+ch1.Name,'')+ISNULL('.'+ch2.Name,'') pname," +
                            // " ISNULL(P.Price,0)+ISNULL(ch1.Price,0)+ISNULL(ch2.Price,0) pprice," +
                            // " ProductGroup" +
                            // " from Product p" +
                            // " left join Choice1 ch1 on ch1.ProductId=p.Id" +
                            // " left join Choice2 ch2 on ch2.ProductId=p.Id and ch2.Choice1Id=ch1.Id" +
                            // " )," +
                            // " ProEqu AS " +
                            // " (" +
                            // " SELECT ISNULL(pq.ProductName, '') ProductName" +
                            // "  ,Multiplier" +
                            // "  ,EquProductName" +
                            // " FROM ProductEqu Pq" +
                            // " )" +
                            // " ,optequ AS " +
                            // " (" +
                            // " SELECT oq.ProductName" +
                            // "  ,(oq.ProductName + oq.Options) oqproname" +
                            // "  ,EquProduct" +
                            // "  ,pro.ProductGroup equproductgroup" +
                            // "  ,miktar" +
                            // "  ,Options" +
                            // "  ,AnaUrun" +
                            // "  ,MenuYuzde" +
                            // "  ,MenuFiyat" +
                            // " FROM OptionsEqu oq" +
                            // "  left join pro on pro.pname=oq.EquProduct" +
                            // " )," +
                            // " Base AS " +
                            // " (" +
                            // " SELECT " +
                            // "   B.ProductId" +
                            // "  ,B.ProductName" +
                            // "  ,P.ProductName PName" +
                            // "  ,P.ProductGroup PGroup" +
                            // "  ,B.Date PaymentTime" +
                            // "  ,B.Quantity" +
                            // "  ,B.Price bprice" +
                            // "  ,b.Options" +
                            // "  ,CASE WHEN CHARINDEX('x', LTRIM(RTRIM(O.s))) > 0 THEN SUBSTRING(LTRIM(RTRIM(O.s)), CHARINDEX('x', LTRIM(RTRIM(O.s))) + 1, 100) ELSE LTRIM(RTRIM(O.s)) END AS Opt" +
                            // "  ,CASE WHEN CHARINDEX('x', LTRIM(RTRIM(O.s))) > 0 THEN SUBSTRING(LTRIM(RTRIM(O.s)), 1, CHARINDEX('x', LTRIM(RTRIM(O.s))) - 1) ELSE '1' END AS OptQty" +
                            // "  ,B.HeaderId" +
                            // " FROM Bill B" +
                            // "    LEFT JOIN Product P ON B.ProductId = P.Id" +
                            // "    CROSS APPLY dbo.SplitString(',', B.Options) AS O" +
                            // " WHERE ISNULL(B.Options, '') <> '' " +
                            // "  AND B.Date BETWEEN @par1 AND @par2" +
                            // " )," +
                            // " BillPrice AS" +
                            // " (" +
                            // " SELECT" +
                            // "   Bp.ProductName" +
                            // "  ,Bp.Price" +
                            // "  ,Bp.MinDate" +
                            // "  ,Bp.Options" +
                            // "  ,CASE WHEN Bp.MaxDate=MAX(Bp.MaxDate) OVER(PARTITION BY Bp.ProductName) THEN GETDATE() ELSE Bp.MaxDate END MaxDate " +
                            // " FROM" +
                            // "     (" +
                            // "      SELECT " +
                            // "         ProductName" +
                            // "        ,Price" +
                            // "		  ,Options" +
                            // "        ,MIN(Date) MinDate" +
                            // "        ,MAX(Date) MaxDate " +
                            // "      FROM Bill B " +
                            // "	  	  where UserName='PAKET'" +
                            // "      GROUP BY ProductName,Price,Options" +
                            // "     ) Bp" +
                            // " )," +
                            // " OptSatislar AS" +
                            // " (" +
                            // " SELECT " +
                            // "    Oe.EquProduct" +
                            // "   ,Oe.Miktar" +
                            // "   ,equproductgroup" +
                            // "   ,B.ProductName" +
                            // "   ,B.HeaderId" +
                            // "   ,B.PaymentTime" +
                            // "   ,B.ProductId" +
                            // "   ,Quantity * (CASE WHEN ISNUMERIC(OptQty) = 1 THEN CAST(OptQty AS INT) ELSE 1 END) AS Quantity" +
                            // "   ,Opt AS OptionsName" +
                            // "   ,B.ProductName aaa" +
                            // "   ,ISNULL(Oe.EquProduct, opt) ProductName2" +
                            // "   ,case " +
                            // "   when ISNULL(OE.MenuYuzde,0)>0  THEN  b.bprice*(ISNULL(OE.MenuYuzde,0)/100)" +
                            // "   when ISNULL(Oe.MenuFiyat,0)=0 AND  oe.AnaUrun=1 AND ISNULL(MAX(Bp.Price),0)>0  THEN  MAX(Bp.Price)" +
                            // "   when ISNULL(Oe.MenuFiyat,0)=0 AND  oe.AnaUrun=1 AND ISNULL(MAX(Bp.Price),0)=0  THEN b.bprice" +
                            // "   ELSE ISNULL(Oe.MenuFiyat,0) END  MenuFiyat" +
                            // " FROM Base B" +
                            // "     LEFT JOIN optequ Oe On Oe.ProductName+Oe.Options=B.PName+ISNULL(B.Opt, '')" +
                            // "     LEFT JOIN BillPrice Bp On B.ProductName+ISNULL(b.Options,'')=Bp.ProductName +ISNULL(bp.Options,'') AND B.PaymentTime BETWEEN Bp.MinDate and Bp.MaxDate" +
                            // "     LEFT JOIN Pro on pro.pname=ISNULL(Oe.EquProduct, opt)" +
                            // "	   where (select top 1 Category from Options where [name]=b.Opt) <>'RAPORDISI' OR  (opt not like '%istemiyorum%' and opt not like '%istiyorum%')" +
                            // " GROUP BY " +
                            // "   Oe.EquProduct" +
                            // "  ,Oe.Miktar" +
                            // "  ,oe.AnaUrun" +
                            // "  ,B.ProductName" +
                            // "  ,oe.equproductgroup  " +
                            // "  ,B.HeaderId" +
                            // "  ,B.PaymentTime" +
                            // "  ,B.ProductId" +
                            // "  ,OE.MenuYuzde" +
                            // "  ,Oe.MenuFiyat" +
                            // "  ,B.bprice" +
                            // "  ,Quantity * (CASE WHEN ISNUMERIC(OptQty) = 1 THEN CAST(OptQty AS INT) ELSE 1 END)" +
                            // "  ,Opt" +
                            // " )" +
                            // "  ,opttotal" +
                            // " AS (" +
                            // "  SELECT optsatislar.ProductId" +
                            // "    ,ISNULL(EquProduct, optsatislar.ProductName) BillProductName" +
                            // "    ,ISNULL(EquProduct, optsatislar.OptionsName ) ProductName" +
                            // "    ,equproductgroup ProductGroup" +
                            // "    ,'' InvoiceName" +
                            // "    ,sum(Quantity) OrjToplam" +
                            // "    ,sum(Quantity * ISNULL(Miktar, 1)) Toplam" +
                            // "    ,(sum(MenuFiyat)) * sum(Quantity) OrjTutar" +
                            // "    ,CASE WHEN sum(MenuFiyat) = 0 AND SUM(ISNULL(Miktar, 0)) > 0 THEN 0 ELSE sum(optsatislar.MenuFiyat*Quantity) END Tutar" +
                            // "  FROM optsatislar" +
                            // "  GROUP BY EquProduct" +
                            // "    ,optsatislar.ProductName" +
                            // "    ,optsatislar.OptionsName" +
                            // "    ,equproductgroup " +
                            // "    ,optsatislar.ProductId" +
                            // "  )" +
                            // "  ,indirim" +
                            // " AS (" +
                            // "  SELECT TOP 9999999999 ISNULL(avg(ISNULL(b2.discount, 1)), 1) / (CASE WHEN sum(Quantity * Price) = 0 THEN 1 ELSE sum(Quantity * Price) END) indirimtutar" +
                            // "    ,HeaderId indHeaderId" +
                            // "  FROM PaidBill b2" +
                            // "  WHERE PaymentTime >= @par1" +
                            // "  GROUP BY HeaderId" +
                            // "  )" +
                            // "  ,paketindirim" +
                            // " AS (" +
                            // "  SELECT TOP 9999999 ISNULL(ph.discount, 0) / (" +
                            // "      SELECT sum(Quantity * Price)" +
                            // "      FROM bill b2" +
                            // "      WHERE b2.HeaderId = ph.HeaderId" +
                            // "      ) pktindirim" +
                            // "    ,HeaderId pktheaderId" +
                            // "  FROM PhoneOrderHeader ph" +
                            // "  )" +
                            // "  ,satislar2" +
                            // " AS (" +
                            // "  SELECT " +
                            // "     ent.pkeyValue" +
                            // "    ,B.HeaderId" +
                            // "    ,pq.EquProductName" +
                            // "    ,P.Id AS ProductId" +
                            // "    ,SUM(Quantity * ISNULL(pq.Multiplier, 1)) AS Toplam" +
                            // "	  ,SUM(Quantity ) AS Toplam2" +
                            // "    ,ISNULL(pq.EquProductName, B.ProductName) ProductName" +
                            // "    ,P.ProductGroup" +
                            // "    ,SUM((ISNULL(Quantity, 0) * CASE WHEN ISNULL(pq.Multiplier, 1)=0 THEN 1 ELSE ISNULL(pq.Multiplier, 1) END  ) * ISNULL(B.Price, 0)) AS Tutar" +
                            // "    ,(" +
                            // "      SELECT SUM(Bill.Price * Bill.Quantity)" +
                            // "      FROM dbo.BillWithHeader AS Bill" +
                            // "      WHERE BillState = 0 AND HeaderId = b.HeaderId AND ProductId = b.ProductId" +
                            // "      ) AS OPENTABLE" +
                            // "    ,(" +
                            // "      SELECT SUM(Bill.Quantity)" +
                            // "      FROM dbo.BillWithHeader AS Bill" +
                            // "      WHERE BillState = 0 AND HeaderId = b.HeaderId AND ProductId = b.ProductId" +
                            // "      ) AS OPENTABLEQuantity" +
                            // "    ,0 AS Discount" +
                            // "    ,SUM((ISNULL(Quantity, 0) * ISNULL(pq.Multiplier, 1)) * ISNULL(B.Price, 0)) * (avg(ind.indirimtutar)) INDIRIM" +
                            // "    ,SUM((ISNULL(Quantity, 0) * ISNULL(pq.Multiplier, 1)) * ISNULL(B.Price, 0)) * avg(pkt.pktindirim) PAKETINDIRIM" +
                            // "    ,CASE WHEN sum(ISNULL(B.Price, 0)) = 0 THEN sum((ISNULL(B.OriginalPrice, 0))) ELSE 0 END IKRAM" +
                            // "  FROM dbo.Bill AS b WITH (NOLOCK)" +
                            // "   LEFT JOIN proequ pq ON pq.ProductName = B.ProductName" +
                            // "   LEFT JOIN dbo.Product AS P ON P.Id = B.ProductId" +
                            // "   LEFT JOIN indirim AS ind ON ind.indHeaderId = b.HeaderId" +
                            // "   LEFT JOIN paketindirim AS pkt ON pkt.pktheaderId = b.HeaderId" +
                            // "   LEFT JOIN ent on ent.pkeyValue=b.HeaderId" +
                            // "  WHERE [Date] >= @par1 AND [Date] <= @par2 AND P.ProductName NOT LIKE '$%'" +
                            // "  GROUP BY " +
                            // "    ent.pkeyValue" +
                            // "    ,B.HeaderId" +
                            // "    ,pq.EquProductName" +
                            // "    ,B.ProductName" +
                            // "    ,b.ProductId" +
                            // "    ,P.ProductGroup" +
                            // "    ,B.HeaderId" +
                            // "    ,p.Id" +
                            // "  )," +
                            // "  TOTALSATIS AS (" +
                            // " SELECT sum(MIKTAR) MIKTAR" +
                            // "  ,a.pkeyvalue" +
                            // "  ,a.ProductName" +
                            // "  ,Sum(TUTAR) TUTAR" +
                            // "  ,SUM(INDIRIM) INDIRIM" +
                            // "  ,SUM(IKRAM) IKRAM" +
                            // " FROM (" +
                            // "  SELECT CASE WHEN sum(opttotal.Toplam) = 0 THEN sum(opttotal.OrjToplam) ELSE sum(opttotal.Toplam) END AS MIKTAR" +
                            // "    ,ProductName" +
                            // "    ,sum(ISNULL(opttotal.Tutar, 0)) AS TUTAR" +
                            // "    ,0 INDIRIM" +
                            // "    ,0 IKRAM" +
                            // "	  ,0 pkeyvalue" +
                            // "  FROM opttotal" +
                            // "  GROUP BY " +
                            // "    opttotal.ProductName" +
                            // "  UNION" +
                            // "  SELECT sum(toplam) AS MIKTAR" +
                            // "    ,ProductName" +
                            // "    ,sum(tutar) AS TUTAR" +
                            // "    ,SUM(ISNULL(INDIRIM, 0)) + SUM(ISNULL(PAKETINDIRIM, 0)) INDIRIM" +
                            // "    ,SUM(IKRAM) IKRAM" +
                            // "	  ,pkeyValue" +
                            // "  FROM satislar2" +
                            // "  GROUP BY " +
                            // "   pkeyvalue," +
                            // "   ProductName" +
                            // "  ) AS a" +
                            // "   where MIKTAR<>0" +
                            // " group by" +
                            // " a.pkeyvalue," +
                            // " a.ProductName" +
                            // " )," +
                            // " envanter AS" +
                            // "  (SELECT STOKNO SIND," +
                            // "  SUM(ENVANTER) ENVANTER" +
                            // "   FROM " + vegaDb + ".DBO.F0" + FirmaId + aktifDonem + "TBLDEPOENVANTER" +
                            // "   WHERE TARIH<@par1 AND DEPO IN (" + DepoId + ")" +
                            // "   GROUP BY" +
                            // "   STOKNO" +
                            // " )," +
                            // "    DIenvanter AS" +
                            // "  (SELECT STOKNO SIND," +
                            // "  SUM(ENVANTER) DIENVANTER" +
                            // "   FROM " + vegaDb + ".DBO.F0" + FirmaId + aktifDonem + "TBLDEPOENVANTER" + // F0" + FirmaId + __ResultString + "TBLDEPOENVANTER
                            // "   WHERE TARIH>=@par1 AND TARIH<=@par2 AND DEPO IN (" + DepoId + ")" +
                            // "   GROUP BY" +
                            // "   STOKNO" +
                            // " )," +
                            // " maliyet AS" +
                            // " (SELECT S.MALIYET," +
                            // "          S.IND," +
                            // "	        B.BIRIMADI," +
                            // "          S.STOKKODU HMKODU," +
                            // "          S.MALINCINSI HMADI" +
                            // "   FROM " + vegaDb + ".DBO.F0" + FirmaId + "TBLSTOKLAR S" +
                            // "   LEFT JOIN " + vegaDb + ".DBO.F0" + FirmaId + "TBLBIRIMLEREX B ON B.IND=S.BIRIMEX WHERE S.KOD5='SEFIMWEBREPORT')," +
                            // "     recete AS" +
                            // "  (SELECT " +
                            // "   maliyet.HMKODU," +
                            // "   maliyet.HMADI," +
                            // "   maliyet.BIRIMADI," +
                            // "   maliyet.MALIYET," +
                            // "  [Quantity] miktar," +
                            // "          StokID," +
                            // "          ProductId," +
                            // "          ProductName" +
                            // "   FROM " + DBName + ".[dbo].[Bom]  " +
                            // "    left join maliyet on maliyet.IND=" + DBName + ".[dbo].[Bom].StokID " +
                            // " UNION ALL SELECT" +
                            // "    maliyet.HMKODU," +
                            // "    maliyet.HMADI," +
                            // "    maliyet.BIRIMADI," +
                            // "    maliyet.MALIYET," +
                            // "    Quantity miktar," +
                            // "    StokID," +
                            // "    ProductName," +
                            // "    OptionsName" +
                            // "   FROM " + DBName + ".[dbo].BomOptions" +
                            // "     left join maliyet on maliyet.IND=" + DBName + ".[dbo].BomOptions.StokID " +
                            // "   WHERE ISNULL(MaliyetDahil, 0)=1)" +
                            // "   select " +
                            // "    HMADI" +
                            // "	 ,sum(recetemiktar) recetemiktar" +
                            // "	 ,sum(entolmayanrecetemiktar) recetemiktar" +
                            // "   ,BIRIMADI" +
                            // "   ,ReceteBirimMaliyet" +
                            // "   ,(sum(ISNULL(recetemiktar,0)) +sum(ISNULL(entolmayanrecetemiktar,0))) * ISNULL(ReceteBirimMaliyet,0) ReceteKullanilanTutar" +
                            // "   ,ISNULL(ENVANTER,0) DonemBasiEnvanter" +
                            // "   ,ISNULL(DIENVANTER,0)+sum(ISNULL(recetemiktar,0)) DonemIciGonderilen" +
                            // "   ,sum(ISNULL(recetemiktar,0)) +sum(ISNULL(entolmayanrecetemiktar,0)) as DonemIciCikan" +
                            // "   ,ISNULL(ENVANTER,0) + (ISNULL(DIENVANTER,0)+sum(ISNULL(recetemiktar,0))) - (sum(ISNULL(recetemiktar,0)) +sum(ISNULL(entolmayanrecetemiktar,0))) Kalan" +
                            // "   from (" +
                            // "      select" +
                            // "	  	  recete.HMADI," +
                            // "        CASE WHEN ISNULL(pkeyvalue,0) = 0 then 0 else TOTALSATIS.MIKTAR*recete.miktar end recetemiktar," +
                            // "        CASE WHEN ISNULL(pkeyvalue,0) = 0 then TOTALSATIS.MIKTAR*recete.miktar else 0 end entolmayanrecetemiktar," +
                            // "        recete.BIRIMADI," +
                            // "        envanter.ENVANTER," +
                            // "        DIenvanter.DIENVANTER" +
                            // "       ,pkeyvalue" +
                            // "       ,recete.MALIYET ReceteBirimMaliyet" +
                            // "   from Recete" +
                            // "    LEFT JOIN TOTALSATIS on TOTALSATIS.ProductName=recete.ProductName" +
                            // "    LEFT JOIN envanter on envanter.SIND=recete.StokID" +
                            // "    LEFT JOIN DIenvanter on DIenvanter.SIND=recete.StokID" +
                            // "	) as t" +
                            // " Where ISNULL(t.HMADI,'')<>'' " +
                            // " GROUP BY" +
                            // "	 HMADI" +
                            // "  ,BIRIMADI" +
                            // "  ,ENVANTER" +
                            // "  ,DIENVANTER" +
                            // "  ,ReceteBirimMaliyet" +
                            // "   order by " +
                            // "   HMADI ";

                            //Query =
                            //     " Declare @par1 nvarchar(20) = '{TARIH1}';" +
                            //     " Declare @par2 nvarchar(20) = '{TARIH2}';" +
                            //     " WITH ent as" +
                            //     "  (SELECT [pkeyValue]" +
                            //     "   FROM [entegrasyon].[dbo].[Integration]" +
                            //     "   where dbname like  '" + DBName + "%'" +
                            //     "     and izahat in (96, 97)" +
                            //     "   group by [pkeyValue])," +
                            //     "     pro as" +
                            //     "  (select ISNULL(P.ProductName, '')+ISNULL('.'+ch1.Name, '')+ISNULL('.'+ch2.Name, '') pname," +
                            //     "          ISNULL(P.Price, 0)+ISNULL(ch1.Price, 0)+ISNULL(ch2.Price, 0) pprice," +
                            //     "          ProductGroup" +
                            //     "   from Product p" +
                            //     "   left join Choice1 ch1 on ch1.ProductId=p.Id" +
                            //     "   left join Choice2 ch2 on ch2.ProductId=p.Id" +
                            //     "   and ch2.Choice1Id=ch1.Id)," +
                            //     "     ProEqu AS" +
                            //     "  (SELECT ISNULL(pq.ProductName, '') ProductName ," +
                            //     "          Multiplier ," +
                            //     "          EquProductName" +
                            //     "   FROM ProductEqu Pq)," +
                            //     "     optequ AS" +
                            //     "  (SELECT oq.ProductName ," +
                            //     "          (oq.ProductName + oq.Options) oqproname ," +
                            //     "          EquProduct ," +
                            //     "          pro.ProductGroup equproductgroup ," +
                            //     "          miktar ," +
                            //     "          Options ," +
                            //     "          AnaUrun ," +
                            //     "          MenuYuzde ," +
                            //     "          MenuFiyat" +
                            //     "   FROM OptionsEqu oq" +
                            //     "   left join pro on pro.pname=oq.EquProduct)," +
                            //     "     Base AS" +
                            //     "  (SELECT B.ProductId ," +
                            //     "          B.ProductName ," +
                            //     "          P.ProductName PName ," +
                            //     "          P.ProductGroup PGroup ," +
                            //     "          B.Date PaymentTime ," +
                            //     "          B.Quantity ," +
                            //     "          B.Price bprice ," +
                            //     "          b.Options ," +
                            //     "          CASE" +
                            //     "              WHEN CHARINDEX('x', LTRIM(RTRIM(O.s))) > 0 THEN SUBSTRING(LTRIM(RTRIM(O.s)), CHARINDEX('x', LTRIM(RTRIM(O.s))) + 1, 100)" +
                            //     "              ELSE LTRIM(RTRIM(O.s))" +
                            //     "          END AS Opt ," +
                            //     "          CASE" +
                            //     "              WHEN CHARINDEX('x', LTRIM(RTRIM(O.s))) > 0 THEN SUBSTRING(LTRIM(RTRIM(O.s)), 1, CHARINDEX('x', LTRIM(RTRIM(O.s))) - 1)" +
                            //     "              ELSE '1'" +
                            //     "          END AS OptQty ," +
                            //     "          B.HeaderId" +
                            //     "   FROM Bill B" +
                            //     "   LEFT JOIN Product P ON B.ProductId = P.Id CROSS APPLY dbo.SplitString(',', B.Options) AS O" +
                            //     "   WHERE ISNULL(B.Options, '') <> ''" +
                            //     "     AND B.Date BETWEEN @par1 AND @par2 )," +
                            //     "     BillPrice AS" +
                            //     "  (SELECT Bp.ProductName ," +
                            //     "          Bp.Price ," +
                            //     "          Bp.MinDate ," +
                            //     "          Bp.Options ," +
                            //     "          CASE" +
                            //     "              WHEN Bp.MaxDate=MAX(Bp.MaxDate) OVER(PARTITION BY Bp.ProductName) THEN GETDATE()" +
                            //     "              ELSE Bp.MaxDate" +
                            //     "          END MaxDate" +
                            //     "   FROM" +
                            //     "     (SELECT ProductName ," +
                            //     "             Price ," +
                            //     "             Options ," +
                            //     "             MIN(Date) MinDate ," +
                            //     "             MAX(Date) MaxDate" +
                            //     "      FROM Bill B" +
                            //     "      where UserName='PAKET'" +
                            //     "      GROUP BY ProductName," +
                            //     "               Price," +
                            //     "               Options) Bp)," +
                            //     "     OptSatislar AS" +
                            //     "  (SELECT Oe.EquProduct ," +
                            //     "          Oe.Miktar ," +
                            //     "          equproductgroup ," +
                            //     "          B.ProductName ," +
                            //     "          B.HeaderId ," +
                            //     "          B.PaymentTime ," +
                            //     "          B.ProductId ," +
                            //     "          Quantity * (CASE" +
                            //     "                          WHEN ISNUMERIC(OptQty) = 1 THEN CAST(OptQty AS INT)" +
                            //     "                          ELSE 1" +
                            //     "                      END) AS Quantity ," +
                            //     "          Opt AS OptionsName ," +
                            //     "          B.ProductName aaa ," +
                            //     "          ISNULL(Oe.EquProduct, opt) ProductName2 ," +
                            //     "          case" +
                            //     "              when ISNULL(OE.MenuYuzde, 0)>0 THEN b.bprice*(ISNULL(OE.MenuYuzde, 0)/100)" +
                            //     "              when ISNULL(Oe.MenuFiyat, 0)=0" +
                            //     "                   AND oe.AnaUrun=1" +
                            //     "                   AND ISNULL(MAX(Bp.Price), 0)>0 THEN MAX(Bp.Price)" +
                            //     "              when ISNULL(Oe.MenuFiyat, 0)=0" +
                            //     "                   AND oe.AnaUrun=1" +
                            //     "                   AND ISNULL(MAX(Bp.Price), 0)=0 THEN b.bprice" +
                            //     "              ELSE ISNULL(Oe.MenuFiyat, 0)" +
                            //     "          END MenuFiyat" +
                            //     "   FROM Base B" +
                            //     "   LEFT JOIN optequ Oe On Oe.ProductName+Oe.Options=B.PName+ISNULL(B.Opt, '')" +
                            //     "   LEFT JOIN BillPrice Bp On B.ProductName+ISNULL(b.Options, '')=Bp.ProductName +ISNULL(bp.Options, '')" +
                            //     "   AND B.PaymentTime BETWEEN Bp.MinDate and Bp.MaxDate" +
                            //     "   LEFT JOIN Pro on pro.pname=ISNULL(Oe.EquProduct, opt)" +
                            //     "   where" +
                            //     "       (select top 1 Category" +
                            //     "        from Options" +
                            //     "        where [name]=b.Opt) <>'RAPORDISI'" +
                            //     "     OR (opt not like '%istemiyorum%'" +
                            //     "         and opt not" +
                            //     " like '%istiyorum%')" +
                            //     "   GROUP BY Oe.EquProduct ," +
                            //     "            Oe.Miktar ," +
                            //     "            oe.AnaUrun ," +
                            //     "            B.ProductName ," +
                            //     "            oe.equproductgroup ," +
                            //     "            B.HeaderId ," +
                            //     "            B.PaymentTime ," +
                            //     "            B.ProductId ," +
                            //     "            OE.MenuYuzde ," +
                            //     "            Oe.MenuFiyat ," +
                            //     "            B.bprice ," +
                            //     "            Quantity * (CASE" +
                            //     "                            WHEN ISNUMERIC(OptQty) = 1 THEN CAST(OptQty AS INT)" +
                            //     "                            ELSE 1" +
                            //     "                        END) ," +
                            //     "            Opt) ," +
                            //     "     opttotal AS" +
                            //     "  (SELECT optsatislar.ProductId ," +
                            //     "          ISNULL(EquProduct, optsatislar.ProductName) BillProductName ," +
                            //     "          ISNULL(EquProduct, optsatislar.OptionsName) ProductName ," +
                            //     "          equproductgroup ProductGroup ," +
                            //     "          '' InvoiceName ," +
                            //     "             sum(Quantity) OrjToplam ," +
                            //     "             sum(Quantity * ISNULL(Miktar, 1)) Toplam ," +
                            //     "             (sum(MenuFiyat)) * sum(Quantity) OrjTutar ," +
                            //     "             CASE" +
                            //     "                 WHEN sum(MenuFiyat) = 0" +
                            //     "                      AND SUM(ISNULL(Miktar, 0)) > 0 THEN 0" +
                            //     "                 ELSE sum(optsatislar.MenuFiyat*Quantity)" +
                            //     "             END Tutar" +
                            //     "   FROM optsatislar" +
                            //     "   GROUP BY EquProduct ," +
                            //     "            optsatislar.ProductName ," +
                            //     "            optsatislar.OptionsName ," +
                            //     "            equproductgroup ," +
                            //     "            optsatislar.ProductId) ," +
                            //     "     indirim AS" +
                            //     "  (SELECT TOP 9999999999 ISNULL(avg(ISNULL(b2.discount, 1)), 1) / (CASE" +
                            //     "                                                                       WHEN sum(Quantity * Price) = 0 THEN 1" +
                            //     "                                                                       ELSE sum(Quantity * Price)" +
                            //     "                                                                   END) indirimtutar ," +
                            //     "                         HeaderId indHeaderId" +
                            //     "   FROM PaidBill b2" +
                            //     "   WHERE PaymentTime >= @par1" +
                            //     "   GROUP BY HeaderId) ," +
                            //     "     paketindirim AS" +
                            //     "  (SELECT TOP 9999999 ISNULL(ph.discount, 0) /" +
                            //     "     (SELECT sum(Quantity * Price)" +
                            //     "      FROM bill b2" +
                            //     "      WHERE b2.HeaderId = ph.HeaderId ) pktindirim ," +
                            //     "                      HeaderId pktheaderId" +
                            //     "   FROM PhoneOrderHeader ph) ," +
                            //     "     satislar2 AS" +
                            //     "  (SELECT ent.pkeyValue ," +
                            //     "          B.HeaderId ," +
                            //     "          pq.EquProductName ," +
                            //     "          P.Id AS ProductId ," +
                            //     "          SUM(Quantity * ISNULL(pq.Multiplier, 1)) AS Toplam ," +
                            //     "          SUM(Quantity) AS Toplam2 ," +
                            //     "          ISNULL(pq.EquProductName, B.ProductName) ProductName ," +
                            //     "          P.ProductGroup ," +
                            //     "          SUM((ISNULL(Quantity, 0) * CASE" +
                            //     "                                         WHEN ISNULL(pq.Multiplier, 1)=0 THEN 1" +
                            //     "                                         ELSE ISNULL(pq.Multiplier, 1)" +
                            //     "                                     END) * ISNULL(B.Price, 0)) AS Tutar ," +
                            //     "     (SELECT SUM(Bill.Price * Bill.Quantity)" +
                            //     "      FROM dbo.BillWithHeader AS Bill" +
                            //     "      WHERE BillState = 0" +
                            //     "        AND HeaderId = b.HeaderId" +
                            //     "        AND ProductId = b.ProductId ) AS OPENTABLE ," +
                            //     "     (SELECT SUM(Bill.Quantity)" +
                            //     "      FROM dbo.BillWithHeader AS Bill" +
                            //     "      WHERE BillState = 0" +
                            //     "        AND HeaderId = b.HeaderId" +
                            //     "        AND ProductId = b.ProductId ) AS OPENTABLEQuantity ," +
                            //     "          0 AS Discount ," +
                            //     "          SUM((ISNULL(Quantity, 0) * ISNULL(pq.Multiplier, 1)) * ISNULL(B.Price, 0)) * (avg(ind.indirimtutar)) INDIRIM ," +
                            //     "          SUM((ISNULL(Quantity, 0) * ISNULL(pq.Multiplier, 1)) * ISNULL(B.Price, 0)) * avg(pkt.pktindirim) PAKETINDIRIM ," +
                            //     "          CASE" +
                            //     "              WHEN sum(ISNULL(B.Price, 0)) = 0 THEN sum((ISNULL(B.OriginalPrice, 0)))" +
                            //     "              ELSE 0" +
                            //     "          END IKRAM" +
                            //     "   FROM dbo.Bill AS b WITH (NOLOCK)" +
                            //     "   LEFT JOIN proequ pq ON pq.ProductName = B.ProductName" +
                            //     "   LEFT JOIN dbo.Product AS P ON P.Id = B.ProductId" +
                            //     "   LEFT JOIN indirim AS ind ON ind.indHeaderId = b.HeaderId" +
                            //     "   LEFT JOIN paketindirim AS pkt ON pkt.pktheaderId = b.HeaderId" +
                            //     "   LEFT JOIN ent on ent.pkeyValue=b.HeaderId" +
                            //     "   WHERE [Date] >= @par1" +
                            //     "     AND [Date] <= @par2" +
                            //     "     AND P.ProductName NOT LIKE '$%'" +
                            //     "   GROUP BY ent.pkeyValue ," +
                            //     "            B.HeaderId ," +
                            //     "            pq.EquProductName ," +
                            //     "            B.ProductName ," +
                            //     "            b.ProductId ," +
                            //     "            P.ProductGroup ," +
                            //     "            B.HeaderId ," +
                            //     "            p.Id)," +
                            //     "     TOTALSATIS AS" +
                            //     "  (SELECT sum(MIKTAR) MIKTAR ," +
                            //     "          a.pkeyvalue ," +
                            //     "          a.ProductName ," +
                            //     "          Sum(TUTAR) TUTAR ," +
                            //     "          SUM(INDIRIM) INDIRIM ," +
                            //     "          SUM(IKRAM) IKRAM" +
                            //     "   FROM" +
                            //     "     (SELECT CASE" +
                            //     "                 WHEN sum(opttotal.Toplam) = 0 THEN sum(opttotal.OrjToplam)" +
                            //     "                 ELSE sum(opttotal.Toplam)" +
                            //     "             END AS MIKTAR ," +
                            //     "             ProductName ," +
                            //     "             sum(ISNULL(opttotal.Tutar, 0)) AS TUTAR ," +
                            //     "             0 INDIRIM ," +
                            //     "             0 IKRAM ," +
                            //     "             0 pkeyvalue" +
                            //     "      FROM opttotal" +
                            //     "      GROUP BY opttotal.ProductName" +
                            //     "      UNION SELECT sum(toplam) AS MIKTAR ," +
                            //     "                   ProductName ," +
                            //     "                   sum(tutar) AS TUTAR ," +
                            //     "                   SUM(ISNULL(INDIRIM, 0)) + SUM(ISNULL(PAKETINDIRIM, 0)) INDIRIM ," +
                            //     "                   SUM(IKRAM) IKRAM ," +
                            //     "                   pkeyValue" +
                            //     "      FROM satislar2" +
                            //     "      GROUP BY pkeyvalue," +
                            //     "               ProductName) AS a" +
                            //     "   where MIKTAR<>0" +
                            //     "   group by a.pkeyvalue," +
                            //     "            a.ProductName)," +
                            //     "     envanter AS" +
                            //     "  (SELECT STOKNO SIND," +
                            //     "          SUM(ENVANTER) ENVANTER" +
                            //     "   FROM " + vegaDb + ".DBO.F0" + FirmaId + aktifDonem + "TBLDEPOENVANTER" +
                            //     "   WHERE TARIH<@par1" +
                            //     "     AND DEPO IN (" + DepoId + ")" +
                            //     "   GROUP BY STOKNO)," +
                            //     "     DIenvanter AS" +
                            //     "  (SELECT STOKNO SIND," +
                            //     "          SUM(ENVANTER) DIENVANTER" +
                            //     "   FROM " + vegaDb + ".DBO.F0" + FirmaId + aktifDonem + "TBLDEPOENVANTER" +
                            //     "   WHERE TARIH>=@par1" +
                            //     "     AND TARIH<=@par2" +
                            //     "     AND DEPO IN (" + DepoId + ")" +
                            //     "	 AND ENVANTER>0" +
                            //     "   GROUP BY STOKNO)," +
                            //     "        DICenvanter AS" +
                            //     "  (SELECT STOKNO SIND," +
                            //     "          SUM(ENVANTER) DICENVANTER" +
                            //     "   FROM " + vegaDb + ".DBO.F0" + FirmaId + aktifDonem + "TBLDEPOENVANTER" +
                            //     "   WHERE TARIH>=@par1" +
                            //     "     AND TARIH<=@par2" +
                            //     "     AND DEPO IN (" + DepoId + ")" +
                            //     "	 AND ENVANTER<0" +
                            //     "   GROUP BY STOKNO)," +
                            //     "     maliyet AS" +
                            //     "  (SELECT S.MALIYET," +
                            //     "          S.IND," +
                            //     "          B.BIRIMADI," +
                            //     "          S.STOKKODU HMKODU," +
                            //     "          S.MALINCINSI HMADI" +
                            //     "   FROM " + vegaDb + ".DBO.F0" + FirmaId + "TBLSTOKLAR S" +
                            //     "   LEFT JOIN  " + vegaDb + ".DBO.F0" + FirmaId + "TBLBIRIMLEREX B ON B.IND=S.BIRIMEX" +
                            //     "   WHERE S.KOD5='SEFIMWEBREPORT' AND STATUS IN (0,1))," +
                            //     "     recete AS" +
                            //     "  (SELECT maliyet.HMKODU," +
                            //     "          maliyet.HMADI," +
                            //     "          maliyet.BIRIMADI," +
                            //     "          maliyet.MALIYET," +
                            //     "          [Quantity] miktar," +
                            //     "          StokID," +
                            //     "          ProductId," +
                            //     "          ProductName" +
                            //     "   FROM " + DBName + ".[dbo].[Bom]  " +
                            //     "    left join maliyet on maliyet.IND=" + DBName + ".[dbo].[Bom].StokID " +
                            //     "   UNION ALL SELECT maliyet.HMKODU," +
                            //     "                    maliyet.HMADI," +
                            //     "                    maliyet.BIRIMADI," +
                            //     "                    maliyet.MALIYET," +
                            //     "                    Quantity miktar," +
                            //     "                    StokID," +
                            //     "                    ProductName," +
                            //     "                    OptionsName" +

                            //     "   FROM " + DBName + ".[dbo].BomOptions" +
                            //     "   left join maliyet on maliyet.IND=" + DBName + ".[dbo].BomOptions.StokID " +
                            //     "   WHERE ISNULL(MaliyetDahil, 0)=1)" +
                            //     " select HMADI ," +
                            //     "       sum(recetemiktar) recetemiktar ," +
                            //     "       sum(entolmayanrecetemiktar) recetemiktar ," +
                            //     "       BIRIMADI ," +
                            //     "       ReceteBirimMaliyet ," +
                            //     "       (sum(ISNULL(recetemiktar, 0)) +sum(ISNULL(entolmayanrecetemiktar, 0))) * ISNULL(ReceteBirimMaliyet, 0) ReceteKullanilanTutar ," +
                            //     "       ISNULL(ENVANTER, 0) DonemBasiEnvanter ," +
                            //     "       ISNULL(DIENVANTER, 0)+sum(ISNULL(recetemiktar, 0)) DonemIciGonderilen ," +
                            //     "       ISNULL(DICENVANTER, 0)+sum(ISNULL(recetemiktar, 0)) +sum(ISNULL(entolmayanrecetemiktar, 0)) as DonemIciCikan ," +
                            //     "       ISNULL(ENVANTER, 0) + ((ISNULL(DIENVANTER, 0)+ISNULL(DICENVANTER, 0))+sum(ISNULL(recetemiktar, 0))) - (sum(ISNULL(recetemiktar, 0)) +sum(ISNULL(entolmayanrecetemiktar, 0))) Kalan" +
                            //     " from" +
                            //    " (select maliyet.HMADI," +
                            //    "          CASE" +
                            //    "              WHEN ISNULL(pkeyvalue, 0) = 0 then 0" +
                            //    "              else TOTALSATIS.MIKTAR*recete.miktar" +
                            //    "          end recetemiktar," +
                            //    "          CASE" +
                            //    "              WHEN ISNULL(pkeyvalue, 0) = 0 then TOTALSATIS.MIKTAR*recete.miktar" +
                            //    "              else 0" +
                            //    "          end entolmayanrecetemiktar," +
                            //    "          recete.BIRIMADI," +
                            //    "          envanter.ENVANTER," +
                            //    "          DIenvanter.DIENVANTER," +
                            //    "          DICenvanter.DICENVANTER," +
                            //    "          pkeyvalue," +
                            //    "          recete.MALIYET ReceteBirimMaliyet" +
                            //    "   from maliyet" +
                            //    "   LEFT JOIN Recete on Recete.StokID=maliyet.IND" +
                            //    "   LEFT JOIN TOTALSATIS on TOTALSATIS.ProductName=recete.ProductName" +
                            //    "   LEFT JOIN envanter on envanter.SIND=maliyet.IND" +
                            //    "   LEFT JOIN DIenvanter on DIenvanter.SIND=maliyet.IND" +
                            //    "   LEFT JOIN DICenvanter on DICenvanter.SIND=maliyet.IND) as t" +
                            //    " Where ISNULL(t.HMADI, '')<>'' and  ISNULL(DIENVANTER, 0)+ISNULL(DICENVANTER, 0)+ISNULL(ENVANTER, 0) <> 0" +
                            //    " GROUP BY HMADI," +
                            //    "         BIRIMADI," +
                            //    "         ENVANTER," +
                            //    "         DIENVANTER," +
                            //    "         DICENVANTER," +
                            //    "         ReceteBirimMaliyet" +
                            //    " order by HMADI";

Query =
@" Declare @par1 nvarchar(20) = '{TARIH1}';
   Declare @par2 nvarchar(20) = '{TARIH2}';

WITH ent as
  (SELECT [pkeyValue]
   FROM [entegrasyon].[dbo].[Integration]
   where dbname like '" + DBName + "%'" +
     @" and izahat in (96,97)
   group by [pkeyValue]),
     pro as
  (select ISNULL(P.ProductName, '')+ISNULL('.'+ch1.Name, '')+ISNULL('.'+ch2.Name, '') pname,
          ISNULL(P.Price, 0)+ISNULL(ch1.Price, 0)+ISNULL(ch2.Price, 0) pprice,
          ProductGroup
   from Product p
   left join Choice1 ch1 on ch1.ProductId=p.Id
   left join Choice2 ch2 on ch2.ProductId=p.Id
   and ch2.Choice1Id=ch1.Id),
     ProEqu AS
  (SELECT ISNULL(pq.ProductName, '') ProductName,
          Multiplier,
          EquProductName
   FROM ProductEqu Pq),
     optequ AS
  (SELECT oq.ProductName,
          (oq.ProductName + oq.Options) oqproname,
          EquProduct,
          pro.ProductGroup equproductgroup,
          miktar,
          Options,
          AnaUrun,
          MenuYuzde,
          MenuFiyat
   FROM OptionsEqu oq
   left join pro on pro.pname=oq.EquProduct),
     Base AS
  (SELECT B.ProductId,
          B.ProductName,
          P.ProductName PName,
          P.ProductGroup PGroup,
          B.Date PaymentTime,
          B.Quantity,
          B.Price bprice,
          b.Options,
          CASE
              WHEN CHARINDEX('x', LTRIM(RTRIM(O.s))) > 0 THEN SUBSTRING(LTRIM(RTRIM(O.s)), CHARINDEX('x', LTRIM(RTRIM(O.s))) + 1, 100)
              ELSE LTRIM(RTRIM(O.s))
          END AS Opt,
          CASE
              WHEN CHARINDEX('x', LTRIM(RTRIM(O.s))) > 0 THEN SUBSTRING(LTRIM(RTRIM(O.s)), 1, CHARINDEX('x', LTRIM(RTRIM(O.s))) - 1)
              ELSE '1'
          END AS OptQty,
          B.HeaderId
   FROM Bill B
   LEFT JOIN Product P ON B.ProductId = P.Id CROSS APPLY dbo.SplitString(',', B.Options) AS O
   WHERE ISNULL(B.Options, '') <> ''
     AND B.Date BETWEEN @par1 AND @par2 ),
     BillPrice AS
  (SELECT Bp.ProductName,
          Bp.Price,
          Bp.MinDate,
          Bp.Options,
          CASE
              WHEN Bp.MaxDate=MAX(Bp.MaxDate) OVER(PARTITION BY Bp.ProductName) THEN GETDATE()
              ELSE Bp.MaxDate
          END MaxDate
   FROM
     (SELECT ProductName,
             Price,
             Options,
             MIN(Date) MinDate,
             MAX(Date) MaxDate
      FROM Bill B
      where UserName='PAKET'
      GROUP BY ProductName,
               Price,
               Options) Bp),
     OptSatislar AS
  (SELECT Oe.EquProduct,
          Oe.Miktar,
          equproductgroup,
          B.ProductName,
          B.HeaderId,
          B.PaymentTime,
          B.ProductId,
          Quantity * (CASE
                          WHEN ISNUMERIC(OptQty) = 1 THEN CAST(OptQty AS INT)
                          ELSE 1
                      END) AS Quantity,
          Opt AS OptionsName,
          B.ProductName aaa,
          ISNULL(Oe.EquProduct, opt) ProductName2,
          case
              when ISNULL(OE.MenuYuzde, 0)>0 THEN b.bprice*(ISNULL(OE.MenuYuzde, 0)/100)
              when ISNULL(Oe.MenuFiyat, 0)=0
                   AND oe.AnaUrun=1
                   AND ISNULL(MAX(Bp.Price), 0)>0 THEN MAX(Bp.Price)
              when ISNULL(Oe.MenuFiyat, 0)=0
                   AND oe.AnaUrun=1
                   AND ISNULL(MAX(Bp.Price), 0)=0 THEN b.bprice
              ELSE ISNULL(Oe.MenuFiyat, 0)
          END MenuFiyat
   FROM Base B
   LEFT JOIN optequ Oe On Oe.ProductName+Oe.Options=B.PName+ISNULL(B.Opt, '')
   LEFT JOIN BillPrice Bp On B.ProductName+ISNULL(b.Options, '')=Bp.ProductName +ISNULL(bp.Options, '')
   AND B.PaymentTime BETWEEN Bp.MinDate and Bp.MaxDate
   LEFT JOIN Pro on pro.pname=ISNULL(Oe.EquProduct, opt)
   where
       (select top 1 Category
        from Options
        where [name]=b.Opt) <>'RAPORDISI'
     OR (opt not like '%istemiyorum%'
         and opt not like '%istiyorum%')
   GROUP BY Oe.EquProduct,
            Oe.Miktar,
            oe.AnaUrun,
            B.ProductName,
            oe.equproductgroup,
            B.HeaderId,
            B.PaymentTime,
            B.ProductId,
            OE.MenuYuzde,
            Oe.MenuFiyat,
            B.bprice,
            Quantity * (CASE
                            WHEN ISNUMERIC(OptQty) = 1 THEN CAST(OptQty AS INT)
                            ELSE 1
                        END),
            Opt),
     opttotal AS
  (SELECT optsatislar.ProductId,
          ISNULL(EquProduct, optsatislar.ProductName) BillProductName,
          ISNULL(EquProduct, optsatislar.OptionsName) ProductName,
          equproductgroup ProductGroup,
          '' InvoiceName,
             sum(Quantity) OrjToplam,
             sum(Quantity * ISNULL(Miktar, 1)) Toplam,
             (sum(MenuFiyat)) * sum(Quantity) OrjTutar,
             CASE
                 WHEN sum(MenuFiyat) = 0
                      AND SUM(ISNULL(Miktar, 0)) > 0 THEN 0
                 ELSE sum(optsatislar.MenuFiyat*Quantity)
             END Tutar,
			 pkeyvalue
   FROM optsatislar
      LEFT JOIN ent on ent.pkeyValue=optsatislar.HeaderId
   GROUP BY EquProduct,
            optsatislar.ProductName,
            optsatislar.OptionsName,
            equproductgroup,
            optsatislar.ProductId,pkeyvalue),
     indirim AS
  (SELECT TOP 9999999999 ISNULL(avg(ISNULL(b2.discount, 1)), 1) / (CASE
                                                                       WHEN sum(Quantity * Price) = 0 THEN 1
                                                                       ELSE sum(Quantity * Price)
                                                                   END) indirimtutar,
                         HeaderId indHeaderId
   FROM PaidBill b2
   WHERE PaymentTime >= @par1
   GROUP BY HeaderId),
     paketindirim AS
  (SELECT TOP 9999999 ISNULL(ph.discount, 0) /
     (SELECT sum(Quantity * Price)
      FROM bill b2
      WHERE b2.HeaderId = ph.HeaderId ) pktindirim,
                      HeaderId pktheaderId
   FROM PhoneOrderHeader ph),
     satislar2 AS
  (SELECT ent.pkeyValue,
          B.HeaderId,
          pq.EquProductName,
          P.Id AS ProductId,
          SUM(Quantity * ISNULL(pq.Multiplier, 1)) AS Toplam,
          SUM(Quantity) AS Toplam2,
          ISNULL(pq.EquProductName, B.ProductName) ProductName,
          P.ProductGroup,
          SUM((ISNULL(Quantity, 0) * CASE
                                         WHEN ISNULL(pq.Multiplier, 1)=0 THEN 1
                                         ELSE ISNULL(pq.Multiplier, 1)
                                     END) * ISNULL(B.Price, 0)) AS Tutar,

     (SELECT SUM(Bill.Price * Bill.Quantity)
      FROM dbo.BillWithHeader AS Bill
      WHERE BillState = 0
        AND HeaderId = b.HeaderId
        AND ProductId = b.ProductId ) AS OPENTABLE,

     (SELECT SUM(Bill.Quantity)
      FROM dbo.BillWithHeader AS Bill
      WHERE BillState = 0
        AND HeaderId = b.HeaderId
        AND ProductId = b.ProductId ) AS OPENTABLEQuantity,
          0 AS Discount,
          SUM((ISNULL(Quantity, 0) * ISNULL(pq.Multiplier, 1)) * ISNULL(B.Price, 0)) * (avg(ind.indirimtutar)) INDIRIM,
          SUM((ISNULL(Quantity, 0) * ISNULL(pq.Multiplier, 1)) * ISNULL(B.Price, 0)) * avg(pkt.pktindirim) PAKETINDIRIM,
          CASE
              WHEN sum(ISNULL(B.Price, 0)) = 0 THEN sum((ISNULL(B.OriginalPrice, 0)))
              ELSE 0
          END IKRAM
   FROM dbo.Bill AS b WITH (NOLOCK)
   LEFT JOIN proequ pq ON pq.ProductName = B.ProductName
   LEFT JOIN dbo.Product AS P ON P.Id = B.ProductId
   LEFT JOIN indirim AS ind ON ind.indHeaderId = b.HeaderId
   LEFT JOIN paketindirim AS pkt ON pkt.pktheaderId = b.HeaderId
   LEFT JOIN ent on ent.pkeyValue=b.HeaderId
   WHERE [Date] >= @par1
     AND [Date] <= @par2
     AND P.ProductName NOT LIKE '$%'
   GROUP BY ent.pkeyValue,
            B.HeaderId,
            pq.EquProductName,
            B.ProductName,
            b.ProductId,
            P.ProductGroup,
            B.HeaderId,
            p.Id),
     TOTALSATIS AS
  (SELECT sum(MIKTAR) MIKTAR,
          a.pkeyvalue,
          a.ProductName,
          Sum(TUTAR) TUTAR,
          SUM(INDIRIM) INDIRIM,
          SUM(IKRAM) IKRAM
   FROM
     (SELECT CASE
                 WHEN sum(opttotal.Toplam) = 0 THEN sum(opttotal.OrjToplam)
                 ELSE sum(opttotal.Toplam)
             END AS MIKTAR,
             ProductName,
             sum(ISNULL(opttotal.Tutar, 0)) AS TUTAR,
             0 INDIRIM,
             0 IKRAM,
              pkeyvalue
      FROM opttotal
      GROUP BY opttotal.ProductName,pkeyvalue
      UNION SELECT sum(toplam) AS MIKTAR,
                   ProductName,
                   sum(tutar) AS TUTAR,
                   SUM(ISNULL(INDIRIM, 0)) + SUM(ISNULL(PAKETINDIRIM, 0)) INDIRIM,
                   SUM(IKRAM) IKRAM,
                   pkeyValue
      FROM satislar2
      GROUP BY pkeyvalue,
               ProductName) AS a
   where MIKTAR<>0
   group by a.pkeyvalue,
            a.ProductName),

     envanter AS
  (SELECT STOKNO SIND,
          SUM(ENVANTER) ENVANTER
    FROM " + vegaDb + ".DBO.F0" + FirmaId + aktifDonem + "TBLDEPOENVANTER " +
 " WHERE TARIH<@par1 "+
 " AND DEPO IN (" + DepoId + ") " +
   @" GROUP BY STOKNO),
     DIenvanter AS
  (SELECT STOKNO SIND,
          SUM(ENVANTER) DIENVANTER 
   
   FROM " + vegaDb + ".DBO.F0" + FirmaId + aktifDonem + "TBLDEPOENVANTER " +

   @" WHERE TARIH>=@par1
     AND TARIH<=@par2
   AND DEPO IN (" + DepoId + ") " +
    @" AND ENVANTER>0
   GROUP BY STOKNO),
     DICenvanter AS
  (SELECT STOKNO SIND,
          SUM(ENVANTER)*-1 DICENVANTER
  FROM " + vegaDb + ".DBO.F0" + FirmaId + aktifDonem + "TBLDEPOENVANTER " +
@" WHERE TARIH>=@par1
     AND TARIH<=@par2
      AND DEPO IN (" + DepoId + ")" +
    @" AND ENVANTER<0
   GROUP BY STOKNO),
     maliyet AS
  (SELECT S.MALIYET,
          S.IND,
          B.BIRIMADI,
          S.STOKKODU HMKODU,
          S.MALINCINSI HMADI
  FROM " + vegaDb + ".DBO.F0" + FirmaId + "TBLSTOKLAR S " +
 
"   LEFT JOIN  " + vegaDb + ".DBO.F0" + FirmaId + "TBLBIRIMLEREX B ON B.IND=S.BIRIMEX " +
  @" WHERE S.KOD5='SEFIMWEBREPORT'
     AND STATUS IN (0,
                    1)),
     recete AS
  (SELECT maliyet.HMKODU,
          maliyet.HMADI,
          maliyet.BIRIMADI,
          maliyet.MALIYET,
          [Quantity] miktar,
          StokID,
          ProductId,
          ProductName
   FROM " + DBName + @".[dbo].[Bom] 
   left join maliyet on maliyet.IND= " + DBName + @".[dbo].[Bom].StokID 
   UNION ALL SELECT maliyet.HMKODU,
                    maliyet.HMADI,
                    maliyet.BIRIMADI,
                    maliyet.MALIYET,
                    Quantity miktar,
                    StokID,
                    ProductName,
                    OptionsName
 FROM  " + DBName + @".[dbo].BomOptions
   left join maliyet on maliyet.IND = " + DBName + @".[dbo].BomOptions.StokID
   WHERE ISNULL(MaliyetDahil, 0)=1)
select HMADI,
       sum(recetemiktar) recetemiktar,
       sum(entolmayanrecetemiktar) recetemiktar2,
       BIRIMADI,
       ReceteBirimMaliyet,
       (sum(ISNULL(recetemiktar, 0)) +sum(ISNULL(entolmayanrecetemiktar, 0))) * ISNULL(ReceteBirimMaliyet, 0) ReceteKullanilanTutar,
       ISNULL(ENVANTER, 0) DonemBasiEnvanter,
       ISNULL(DIENVANTER, 0) DonemIciGonderilen,
       ISNULL(DICENVANTER, 0)+sum(entolmayanrecetemiktar) as DonemIciCikan,
       ISNULL(ENVANTER, 0) + ((ISNULL(DIENVANTER, 0)+ISNULL(DICENVANTER*-1, 0))+sum(ISNULL(recetemiktar, 0))) - (sum(ISNULL(recetemiktar, 0)) +sum(ISNULL(entolmayanrecetemiktar, 0))) Kalan
from
  (select maliyet.HMADI,
          CASE
              WHEN ISNULL(pkeyvalue, 0) = 0 then 0
              else TOTALSATIS.MIKTAR*recete.miktar
          end recetemiktar,
          CASE
              WHEN ISNULL(pkeyvalue, 0) = 0 then TOTALSATIS.MIKTAR*recete.miktar
              else 0
          end entolmayanrecetemiktar,
          recete.BIRIMADI,
          envanter.ENVANTER,
          DIenvanter.DIENVANTER,
          DICenvanter.DICENVANTER,
          pkeyvalue,
          recete.MALIYET ReceteBirimMaliyet
   from maliyet
   LEFT JOIN Recete on Recete.StokID=maliyet.IND
   LEFT JOIN TOTALSATIS on TOTALSATIS.ProductName=recete.ProductName
   LEFT JOIN envanter on envanter.SIND=maliyet.IND
   LEFT JOIN DIenvanter on DIenvanter.SIND=maliyet.IND
   LEFT JOIN DICenvanter on DICenvanter.SIND=maliyet.IND) as t
Where ISNULL(t.HMADI, '')<>''
  and ISNULL(DIENVANTER, 0)+ISNULL(DICENVANTER, 0)+ISNULL(ENVANTER, 0) <> 0  
GROUP BY HMADI,
         BIRIMADI,
         ENVANTER,
         DIENVANTER,
         DICENVANTER,
         ReceteBirimMaliyet
order by HMADI ";

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

                                //Query =
                                //                    "DECLARE @Sube nvarchar(100) = '{SUBEADI}';" +
                                //                    "DECLARE @Trh1 nvarchar(20) = '{TARIH1}';" +
                                //                    "DECLARE @Trh2 nvarchar(20) = '{TARIH2}';" +
                                //                    "DECLARE @SUBEADI nvarchar(20) = '{SUBEADITBL}';" +
                                //                    "DECLARE @KASAADI nvarchar(20) = '{KASAADI}';" +
                                //                    "DECLARE @FirmaInd nvarchar(100) = '{FIRMAIND}';" +
                                //                                        "WITH Toplamsatis AS" +
                                //                                        "  (SELECT @Sube AS Sube," +
                                //                                        "     (SELECT SUBEADI" +
                                //                                        "      FROM " + FirmaId_SUBE + "" +
                                //                                        "      WHERE IND = TBLFASTERODEMELER.SUBEIND) AS Sube1," +
                                //                                        "     (SELECT KASAADI" +
                                //                                        "      FROM  " + FirmaId_KASA + " " +
                                //                                        "      WHERE IND = TBLFASTERODEMELER.KASAIND) AS Kasa," +
                                //                                        "          ODENEN AS cash," +
                                //                                        "          0 AS Credit," +
                                //                                        "          0 AS Ticket," +
                                //                                        "          0 AS Debit," +
                                //                                        "          0 AS ikram," +
                                //                                        "          0 AS TableNo," +
                                //                                        "          0 AS Discount," +
                                //                                        "          0 AS iptal," +
                                //                                        "		  0 AS iptal2," +
                                //                                        "          0 AS zayi" +
                                //                                        "   FROM DBO.TBLFASTERODEMELER" +
                                //                                        "   WHERE ODEMETIPI = 0" +
                                //                                        "     AND ISNULL(IADE, 0) = 0" +
                                //                                        "     AND ISLEMTARIHI >= @Trh1" +
                                //                                        "     AND ISLEMTARIHI <= @Trh2" +
                                //                                        //"     AND FIRMAIND= @FirmaInd" +
                                //                                        "   " + QueryFasterSube +
                                //                                        "   UNION ALL SELECT @Sube AS Sube," +
                                //                                        "     (SELECT SUBEADI" +
                                //                                        "      FROM " + FirmaId_SUBE + "" +
                                //                                        "      WHERE IND = TBLFASTERODEMELER.SUBEIND) AS Sube1," +
                                //                                        "     (SELECT KASAADI" +
                                //                                        "      FROM  " + FirmaId_KASA + " " +
                                //                                        "      WHERE IND = TBLFASTERODEMELER.KASAIND) AS Kasa," +
                                //                                        "                    ODENEN*-1 AS cash," +
                                //                                        "                    0 AS Credit," +
                                //                                        "                    0 AS Ticket," +
                                //                                        "                    0 AS Debit," +
                                //                                        "                    0 AS ikram," +
                                //                                        "                    0 AS TableNo," +
                                //                                        "                    0 AS Discount," +
                                //                                        "                    0 AS iptal," +
                                //                                        "					0 AS iptal2," +
                                //                                        "                    0 AS zayi" +
                                //                                        "   FROM DBO.TBLFASTERODEMELER" +
                                //                                        "   WHERE ODEMETIPI=0" +
                                //                                        "     AND ISNULL(IADE, 0)=1" +
                                //                                        "     AND ISLEMTARIHI >= @Trh1" +
                                //                                        "     AND ISLEMTARIHI <= @Trh2" +
                                //                                         //"     AND FIRMAIND= @FirmaInd" +
                                //                                         "   " + QueryFasterSube +
                                //                                        "   UNION ALL SELECT @Sube AS Sube," +
                                //                                        "     (SELECT SUBEADI" +
                                //                                        "      FROM " + FirmaId_SUBE + "" +
                                //                                        "      WHERE IND = TBLFASTERODEMELER.SUBEIND) AS Sube1," +
                                //                                        "     (SELECT KASAADI" +
                                //                                        "      FROM  " + FirmaId_KASA + " " +
                                //                                        "      WHERE IND = TBLFASTERODEMELER.KASAIND) AS Kasa," +
                                //                                        "                    0 AS cash," +
                                //                                        "                    ODENEN AS Credit," +
                                //                                        "                    0 AS Ticket," +
                                //                                        "                    0 AS Debit," +
                                //                                        "                    0 AS ikram," +
                                //                                        "                    0 AS TableNo," +
                                //                                        "                    0 AS Discount," +
                                //                                        "                    0 AS iptal," +
                                //                                        "					0 AS iptal2," +
                                //                                        "                    0 AS zayi" +
                                //                                        "   FROM DBO.TBLFASTERODEMELER" +
                                //                                        "   WHERE ODEMETIPI = 1" +
                                //                                        "     AND ISNULL(IADE, 0)= 0" +
                                //                                        "     AND ISLEMTARIHI >= @Trh1" +
                                //                                        "     AND ISLEMTARIHI <= @Trh2" +
                                //                                         //"     AND FIRMAIND= @FirmaInd" +
                                //                                         "   " + QueryFasterSube +
                                //                                        "   UNION ALL SELECT @Sube AS Sube," +
                                //                                        "     (SELECT SUBEADI" +
                                //                                        "      FROM " + FirmaId_SUBE + "" +
                                //                                        "      WHERE IND = TBLFASTERODEMELER.SUBEIND) AS Sube1," +
                                //                                        "     (SELECT KASAADI" +
                                //                                        "      FROM  " + FirmaId_KASA + " " +
                                //                                        "      WHERE IND = TBLFASTERODEMELER.KASAIND) AS Kasa," +
                                //                                        "                    0 AS cash," +
                                //                                        "                    ODENEN*-1 AS Credit," +
                                //                                        "                    0 AS Ticket," +
                                //                                        "                    0 AS Debit," +
                                //                                        "                    0 AS ikram," +
                                //                                        "                    0 AS TableNo," +
                                //                                        "                    0 AS Discount," +
                                //                                        "                    0 AS iptal," +
                                //                                        "					0 AS iptal2," +
                                //                                        "                    0 AS zayi" +
                                //                                        "   FROM DBO.TBLFASTERODEMELER" +
                                //                                        "   WHERE ODEMETIPI=1" +
                                //                                        "     AND ISNULL(IADE, 0)=1" +
                                //                                        "     AND ISLEMTARIHI >= @Trh1" +
                                //                                        "     AND ISLEMTARIHI <= @Trh2" +
                                //                                         //"     AND FIRMAIND= @FirmaInd" +
                                //                                         "   " + QueryFasterSube +
                                //                                        "   UNION ALL SELECT @Sube AS Sube," +
                                //                                        "     (SELECT SUBEADI" +
                                //                                        "      FROM " + FirmaId_SUBE + "" +
                                //                                        "      WHERE IND = TBLFASTERODEMELER.SUBEIND) AS Sube1," +
                                //                                        "     (SELECT KASAADI" +
                                //                                        "      FROM  " + FirmaId_KASA + " " +
                                //                                        "      WHERE IND = TBLFASTERODEMELER.KASAIND) AS Kasa," +
                                //                                        "                    0 AS cash," +
                                //                                        "                    0 AS Credit," +
                                //                                        "                    ODENEN AS Ticket," +
                                //                                        "                    0 AS Debit," +
                                //                                        "                    0 AS ikram," +
                                //                                        "                    0 AS TableNo," +
                                //                                        "                    0 AS Discount," +
                                //                                        "                    0 AS iptal," +
                                //                                        "					0 AS iptal2," +
                                //                                        "                    0 AS zayi" +
                                //                                        "   FROM DBO.TBLFASTERODEMELER" +
                                //                                        "   WHERE ODEMETIPI = 2" +
                                //                                        "     AND ISNULL(IADE, 0)= 0" +
                                //                                        "     AND ISLEMTARIHI >= @Trh1" +
                                //                                        "     AND ISLEMTARIHI <= @Trh2" +
                                //                                         //"     AND FIRMAIND= @FirmaInd" +
                                //                                         "   " + QueryFasterSube +
                                //                                        "   UNION ALL SELECT @Sube AS Sube," +
                                //                                        "     (SELECT SUBEADI" +
                                //                                        "      FROM " + FirmaId_SUBE + "" +
                                //                                        "      WHERE IND = TBLFASTERODEMELER.SUBEIND) AS Sube1," +
                                //                                        "     (SELECT KASAADI" +
                                //                                        "      FROM  " + FirmaId_KASA + " " +
                                //                                        "      WHERE IND = TBLFASTERODEMELER.KASAIND) AS Kasa," +
                                //                                        "                    0 AS cash," +
                                //                                        "                    0 AS Credit," +
                                //                                        "                    ODENEN*-1 AS Ticket," +
                                //                                        "                    0 AS Debit," +
                                //                                        "                    0 AS ikram," +
                                //                                        "                    0 AS TableNo," +
                                //                                        "                    0 AS Discount," +
                                //                                        "                    0 AS iptal," +
                                //                                        "					0 AS iptal2," +
                                //                                        "                    0 AS zayi" +
                                //                                        "   FROM DBO.TBLFASTERODEMELER" +
                                //                                        "   WHERE ODEMETIPI=2" +
                                //                                        "     AND ISNULL(IADE, 0)=1" +
                                //                                        "     AND ISLEMTARIHI >= @Trh1" +
                                //                                        "     AND ISLEMTARIHI <= @Trh2" +
                                //                                         //"     AND FIRMAIND= @FirmaInd" +
                                //                                         "   " + QueryFasterSube +
                                //                                        "   UNION ALL SELECT @Sube AS Sube," +
                                //                                        "     (SELECT SUBEADI" +
                                //                                        "      FROM " + FirmaId_SUBE + "" +
                                //                                        "      WHERE SUBEIND = TBLFASTERODEMELER.SUBEIND) AS Sube1," +
                                //                                        "     (SELECT KASAADI" +
                                //                                        "      FROM  " + FirmaId_KASA + " " +
                                //                                        "      WHERE IND = TBLFASTERODEMELER.KASAIND) AS Kasa," +
                                //                                        "                    0 AS cash," +
                                //                                        "                    0 AS Credit," +
                                //                                        "                    0 AS Ticket," +
                                //                                        "                    0 AS Debit," +
                                //                                        "                    ODENEN AS ikram," +
                                //                                        "                    0 AS TableNo," +
                                //                                        "                    0 AS Discount," +
                                //                                        "                    0 AS iptal," +
                                //                                        "					0 AS iptal2," +
                                //                                        "                    0 AS zayi" +
                                //                                        "   FROM DBO.TBLFASTERODEMELER" +
                                //                                        "   WHERE ODEMETIPI = 3" +
                                //                                        "     AND ISNULL(IADE, 0)= 0" +
                                //                                        "     AND ISLEMTARIHI >= @Trh1" +
                                //                                        "     AND ISLEMTARIHI <= @Trh2" +
                                //                                         //"     AND FIRMAIND= @FirmaInd" +
                                //                                         "   " + QueryFasterSube +
                                //                                        "   UNION ALL SELECT @Sube AS Sube," +
                                //                                        "     (SELECT SUBEADI" +
                                //                                        "      FROM " + FirmaId_SUBE + "" +
                                //                                        "      WHERE IND = TBLFASTERODEMELER.SUBEIND) AS Sube1," +
                                //                                        "     (SELECT KASAADI" +
                                //                                        "      FROM  " + FirmaId_KASA + " " +
                                //                                        "      WHERE IND = TBLFASTERODEMELER.KASAIND) AS Kasa," +
                                //                                        "                    0 AS cash," +
                                //                                        "                    0 AS Credit," +
                                //                                        "                    0 AS Ticket," +
                                //                                        "                    ODENEN AS Debit," +
                                //                                        "                    0 AS ikram," +
                                //                                        "                    0 AS TableNo," +
                                //                                        "                    0 AS Discount," +
                                //                                        "                    0 AS iptal," +
                                //                                        "					0 AS iptal2," +
                                //                                        "                    0 AS zayi" +
                                //                                        "   FROM DBO.TBLFASTERODEMELER" +
                                //                                        "   WHERE ODEMETIPI = 4" +
                                //                                        "     AND ISNULL(IADE, 0)= 0" +
                                //                                        "     AND ISLEMTARIHI >= @Trh1" +
                                //                                        "     AND ISLEMTARIHI <= @Trh2" +
                                //                                         //"     AND FIRMAIND= @FirmaInd" +
                                //                                         "   " + QueryFasterSube +
                                //                                        "   UNION ALL SELECT @Sube AS Sube," +
                                //                                        "     (SELECT SUBEADI" +
                                //                                        "      FROM " + FirmaId_SUBE + "" +
                                //                                        "      WHERE IND = TBLFASTERODEMELER.SUBEIND) AS Sube1," +
                                //                                        "     (SELECT KASAADI" +
                                //                                        "      FROM  " + FirmaId_KASA + " " +
                                //                                        "      WHERE IND = TBLFASTERODEMELER.KASAIND) AS Kasa," +
                                //                                        "                    0 AS cash," +
                                //                                        "                    0 AS Credit," +
                                //                                        "                    0 AS Ticket," +
                                //                                        "                    ODENEN*-1 AS Debit," +
                                //                                        "                    0 AS ikram," +
                                //                                        "                    0 AS TableNo," +
                                //                                        "                    0 AS Discount," +
                                //                                        "                    0 AS iptal," +
                                //                                        "					0 AS iptal2," +
                                //                                        "                    0 AS zayi" +
                                //                                        "   FROM DBO.TBLFASTERODEMELER" +
                                //                                        "   WHERE ODEMETIPI = 4" +
                                //                                        "     AND ISNULL(IADE, 0)= 1" +
                                //                                        "     AND ISLEMTARIHI >= @Trh1" +
                                //                                        "     AND ISLEMTARIHI <= @Trh2" +
                                //                                         //"     AND FIRMAIND= @FirmaInd" +
                                //                                         "   " + QueryFasterSube +
                                //                                        "   UNION ALL SELECT @Sube AS Sube," +
                                //                                        "     (SELECT SUBEADI" +
                                //                                        "      FROM " + FirmaId_SUBE + "" +
                                //                                        "      WHERE IND = TBLFASTERSATISBASLIK.SUBEIND) AS Sube1," +
                                //                                        "     (SELECT KASAADI" +
                                //                                        "      FROM  " + FirmaId_KASA + " " +
                                //                                        "      WHERE IND = TBLFASTERSATISBASLIK.KASAIND) AS Kasa," +
                                //                                        "                    0 AS cash," +
                                //                                        "                    0 AS Credit," +
                                //                                        "                    0 AS Ticket," +
                                //                                        "                    0 AS Debit," +
                                //                                        "                    0 AS ikram," +
                                //                                        "                    0 AS TableNo," +
                                //                                        "                    SATIRISK+ALTISK AS Discount," +
                                //                                        "                    0 AS iptal," +
                                //                                        "					0 AS iptal2," +
                                //                                        "                    0 AS zayi" +
                                //                                        "   FROM DBO.TBLFASTERSATISBASLIK" +
                                //                                        "   WHERE ISNULL(IADE, 0) = 0" +
                                //                                        "     AND ISLEMTARIHI >= @Trh1" +
                                //                                        "     AND ISLEMTARIHI <= @Trh2" +
                                //                                         //"     AND FIRMAIND= @FirmaInd" +
                                //                                         "   " + QueryFasterSube +
                                //                                        "   UNION ALL SELECT @Sube AS Sube," +
                                //                                        "     (SELECT SUBEADI" +
                                //                                        "      FROM " + FirmaId_SUBE + "" +
                                //                                        "      WHERE IND = TBLFASTERODEMELER.SUBEIND) AS Sube1," +
                                //                                        "     (SELECT KASAADI" +
                                //                                        "      FROM  " + FirmaId_KASA + " " +
                                //                                        "      WHERE IND = TBLFASTERODEMELER.KASAIND) AS Kasa," +
                                //                                        "                    0 AS cash," +
                                //                                        "                    0 AS Credit," +
                                //                                        "                    0 AS Ticket," +
                                //                                        "                    0 AS Debit," +
                                //                                        "                    0 AS ikram," +
                                //                                        "                    0 AS TableNo," +
                                //                                        "                    0 AS Discount," +
                                //                                        "					0 AS iptal," +
                                //                                        "                    ODENEN AS iptal2," +
                                //                                        "                    0 AS zayi" +
                                //                                        "   FROM DBO.TBLFASTERODEMELER" +
                                //                                        "   WHERE ISNULL(IADE, 0) = 1" +
                                //                                        "     AND ODEMETIPI NOT IN (0," +
                                //                                        "                           1," +
                                //                                        "                           2," +
                                //                                        "                           4)" +
                                //                                        "     AND ISLEMTARIHI >= @Trh1" +
                                //                                        "     AND ISLEMTARIHI <= @Trh2" +
                                //                                        //"     AND FIRMAIND= @FirmaInd" +
                                //                                        "   " + QueryFasterSube +
                                //                                        "   UNION ALL SELECT @Sube AS Sube," +
                                //                                        "     (SELECT SUBEADI" +
                                //                                        "      FROM " + FirmaId_SUBE + "" +
                                //                                        "      WHERE IND = TBLFASTERODEMELER.SUBEIND) AS Sube1," +
                                //                                        "     (SELECT KASAADI" +
                                //                                        "      FROM  " + FirmaId_KASA + " " +
                                //                                        "      WHERE IND = TBLFASTERODEMELER.KASAIND) AS Kasa," +
                                //                                        "                    0 AS cash," +
                                //                                        "                    0 AS Credit," +
                                //                                        "                    0 AS Ticket," +
                                //                                        "                    0 AS Debit," +
                                //                                        "                    0 AS ikram," +
                                //                                        "                    0 AS TableNo," +
                                //                                        "                    0 AS Discount," +
                                //                                        "                    ODENEN AS iptal," +
                                //                                        "					0 AS iptal2," +
                                //                                        "                    0 AS zayi" +
                                //                                        "   FROM DBO.TBLFASTERODEMELER" +
                                //                                        "   WHERE ISNULL(IADE, 0) = 1" +
                                //                                        "     AND ODEMETIPI  IN (0," +
                                //                                        "                           1," +
                                //                                        "                           2," +
                                //                                        "                           4)" +
                                //                                        "     AND ISLEMTARIHI >= @Trh1" +
                                //                                        "     AND ISLEMTARIHI <= @Trh2" +
                                //                                         //"     AND FIRMAIND= @FirmaInd" +
                                //                                         "   " + QueryFasterSube +
                                //                                        "   UNION ALL SELECT @Sube AS Sube," +
                                //                                        "     (SELECT SUBEADI" +
                                //                                        "      FROM " + FirmaId_SUBE + "" +
                                //                                        "      WHERE IND = TBLFASTERSATISBASLIK.SUBEIND) AS Sube1," +
                                //                                        "     (SELECT KASAADI" +
                                //                                        "      FROM  " + FirmaId_KASA + " " +
                                //                                        "      WHERE IND = TBLFASTERSATISBASLIK.KASAIND) AS Kasa," +
                                //                                        "                    0 AS cash," +
                                //                                        "                    0 AS Credit," +
                                //                                        "                    0 AS Ticket," +
                                //                                        "                    0 AS Debit," +
                                //                                        "                    0 AS ikram," +
                                //                                        "                    COUNT(*) AS TableNo," +
                                //                                        "                    0 AS Discount," +
                                //                                        "                    0 AS iptal," +
                                //                                        "					0 AS iptal2," +
                                //                                        "                    0 AS zayi" +
                                //                                        "   FROM DBO.TBLFASTERSATISBASLIK" +
                                //                                        "   WHERE ISNULL(IADE, 0) = 0" +
                                //                                        "     AND ISLEMTARIHI >= @Trh1" +
                                //                                        "     AND ISLEMTARIHI <= @Trh2" +
                                //                                         //"     AND FIRMAIND= @FirmaInd" +
                                //                                         "   " + QueryFasterSube +
                                //                                        "   GROUP BY SUBEIND," +
                                //                                        "            KASAIND)" +
                                //                                        "SELECT Sube," +
                                //                                        "       Sube1," +
                                //                                        "       Kasa," +
                                //                                        "       SUM(Cash) AS Cash," +
                                //                                        "       SUM(Credit) AS Credit," +
                                //                                        "       Sum(Ticket) AS Ticket," +
                                //                                        "       Sum(Debit) AS Debit," +
                                //                                        "       Sum(ikram) AS ikram," +
                                //                                        "       Sum(TableNo) AS TableNo," +
                                //                                        "       Sum(Discount) AS Discount," +
                                //                                        "       Sum(iptal) AS iptal," +
                                //                                        "       Sum(Zayi) AS Zayi," +
                                //                                        "       SUM(Cash + Credit + Ticket + Debit) -SUM(iptal2) AS ToplamCiro," +
                                //                                        "       0 AS Saniye," +
                                //                                        "       '' AS RowStyle," +
                                //                                        "       '' AS RowError" +
                                //                                        " FROM toplamsatis " +
                                //                                        " GROUP BY Sube," +
                                //                                        "         Sube1," +
                                //                                        "         Kasa";

                                Query =
                                        " Declare @par1 nvarchar(20) = '{TARIH1}';" +
                                        " Declare @par2 nvarchar(20) = '{TARIH2}';" +
                                        " WITH ent as" +
                                        "  (SELECT [pkeyValue]" +
                                        "   FROM [entegrasyon].[dbo].[Integration]" +
                                        "   where dbname like  '" + DBName + "%'" +
                                        "     and izahat in (96," +
                                        "                    97)" +
                                        "   group by [pkeyValue])," +
                                        "    " +
                                        "     TOTALSATIS AS" +
                                        "  (" +
                                        " SELECT T.Sube1 ," +
                                        "       T.Kasa," +
                                        "       T.Id," +
                                        "       SUM(T.MIKTAR) MIKTAR," +
                                        "       SUM(T.TUTAR) TUTAR," +
                                        "       T.ProductName ," +
                                        "       T.ProductCode" +
                                        " FROM (" +
                                        "        (SELECT" +
                                        "           (SELECT SUBEADI" +
                                        "            FROM F0" + FirmaId + "TBLKRDSUBELER" +
                                        "            WHERE IND = FSB.SUBEIND ) AS Sube1 ," +
                                        "           (SELECT KASAADI" +
                                        "            FROM F0" + FirmaId + "TBLKRDKASALAR" +
                                        "            WHERE IND = FSB.KASAIND ) AS Kasa ," +
                                        "           (SELECT IND" +
                                        "            FROM F0" + FirmaId + "TBLKRDKASALAR" +
                                        "            WHERE IND = FSB.KASAIND ) AS Id ," +
                                        "                SUM(FSH.MIKTAR) AS MIKTAR ," +
                                        "                SUM((((FSH.MIKTAR * FSH.SATISFIYATI) * (100 - ISNULL(FSH.ISK1, 0)) / 100) * (100 - ISNULL(FSH.ISK2, 0)) / 100) * (100 - ISNULL(FSB.ALTISKORAN, 0)) / 100) AS TUTAR ," +
                                        "                STK.MALINCINSI AS ProductName ," +
                                        "                STK.STOKKODU AS ProductCode" +
                                        "         FROM TBLFASTERSATISHAREKET AS FSH" +
                                        "         LEFT JOIN TBLFASTERSATISBASLIK AS FSB ON FSH.BASLIKIND = FSB.BASLIKIND" +
                                        "         AND FSH.SUBEIND = FSB.SUBEIND" +
                                        "         AND FSH.KASAIND = FSB.KASAIND" +
                                        "         AND FSH.BASBARCODE = FSB.BARCODE" +
                                        "         LEFT JOIN F0" + FirmaId + "TBLSTOKLAR AS STK ON FSH.STOKIND = STK.IND" +
                                        "         LEFT JOIN F0" + FirmaId + "TBLBIRIMLEREX AS BR ON FSH.BIRIMIND = BR.IND" +
                                        "         WHERE FSH.ISLEMTARIHI >= @par1" +
                                        "           AND FSH.ISLEMTARIHI <= @par2" +
                                        "           AND ISNULL(FSB.IADE, 0) = 0" +
                                        "           and FSH.SUBEIND IN(" + FasterSubeIND + ")" +
                                        "         GROUP BY FSB.SUBEIND ," +
                                        "                  FSB.KASAIND ," +
                                        "                  STK.MALINCINSI ," +
                                        "                  STK.STOKKODU" +
                                        "         UNION ALL SELECT" +
                                        "           (SELECT SUBEADI" +
                                        "            FROM F0" + FirmaId + "TBLKRDSUBELER" +
                                        "            WHERE IND = FSB.SUBEIND ) AS Sube1 ," +
                                        "           (SELECT KASAADI" +
                                        "            FROM F0" + FirmaId + "TBLKRDKASALAR" +
                                        "            WHERE IND = FSB.KASAIND ) AS Kasa ," +
                                        "           (SELECT IND" +
                                        "            FROM F0" + FirmaId + "TBLKRDKASALAR" +
                                        "            WHERE IND = FSB.KASAIND ) AS Id ," +
                                        "                          SUM(FSH.MIKTAR) * - 1.00 AS MIKTAR ," +
                                        "                                              SUM((((FSH.MIKTAR * FSH.SATISFIYATI) * (100 - ISNULL(FSH.ISK1, 0)) / 100) * (100 - ISNULL(FSH.ISK2, 0)) / 100) * (100 - ISNULL(FSB.ALTISKORAN, 0)) / 100) * - 1.00 AS TUTAR ," +
                                        "                                                                                                                                                                                                            STK.MALINCINSI AS ProductName ," +
                                        "                                                                                                                                                                                                            STK.STOKKODU AS ProductCode" +
                                        "         FROM TBLFASTERSATISHAREKET AS FSH" +
                                        "         LEFT JOIN TBLFASTERSATISBASLIK AS FSB ON FSH.BASLIKIND = FSB.BASLIKIND" +
                                        "         AND FSH.SUBEIND = FSB.SUBEIND" +
                                        "         AND FSH.KASAIND = FSB.KASAIND" +
                                        "         AND FSH.BASBARCODE = FSB.BARCODE" +
                                        "         LEFT JOIN F0" + FirmaId + "TBLSTOKLAR AS STK ON FSH.STOKIND = STK.IND" +
                                        "         LEFT JOIN F0" + FirmaId + "TBLBIRIMLEREX AS BR ON FSH.BIRIMIND = BR.IND" +
                                        "         WHERE FSH.ISLEMTARIHI >= @par1" +
                                        "           AND FSH.ISLEMTARIHI <= @par2" +
                                        "           AND ISNULL(FSB.IADE, 0) = 1" +
                                        "           and FSH.SUBEIND IN(" + FasterSubeIND + ")" +
                                        "         GROUP BY FSB.SUBEIND ," +
                                        "                  FSB.KASAIND ," +
                                        "                  STK.MALINCINSI ," +
                                        "                  STK.STOKKODU)) T" +
                                        " GROUP BY T.Sube1 ," +
                                        "         T.Kasa," +
                                        "         T.Id," +
                                        "         T.ProductCode," +
                                        "         T.ProductName)," +
                                        "     envanter AS" +
                                        "  (SELECT STOKNO SIND," +
                                        "          SUM(ENVANTER) ENVANTER" +
                                        "   FROM " + vegaDb + ".DBO.F0" + FirmaId + aktifDonem + "TBLDEPOENVANTER" +
                                        "   WHERE TARIH<@par1" +
                                        "     AND DEPO IN (" + DepoId + ")" +
                                        "   GROUP BY STOKNO)," +
                                        "     DIenvanter AS" +
                                        "  (SELECT STOKNO SIND," +
                                        "          SUM(ENVANTER) DIENVANTER" +
                                        "   FROM " + vegaDb + ".DBO.F0" + FirmaId + aktifDonem + "TBLDEPOENVANTER" +
                                        "   WHERE TARIH>=@par1" +
                                        "     AND TARIH<=@par2" +
                                        "     AND DEPO IN (" + DepoId + ")" +
                                        "     AND ENVANTER>0" +
                                        "   GROUP BY STOKNO)," +
                                        "     DICenvanter AS" +
                                        "  (SELECT STOKNO SIND," +
                                        "          SUM(ENVANTER) DICENVANTER" +
                                        "   FROM " + vegaDb + ".DBO.F0" + FirmaId + aktifDonem + "TBLDEPOENVANTER" +
                                        "   WHERE TARIH>=@par1" +
                                        "     AND TARIH<=@par2" +
                                        "     AND DEPO IN (" + DepoId + ")" +
                                        "     AND ENVANTER<0" +
                                        "   GROUP BY STOKNO)," +
                                        "     maliyet AS" +
                                        "  (SELECT S.MALIYET," +
                                        "          S.IND," +
                                        "          B.BIRIMADI," +
                                        "          S.STOKKODU HMKODU," +
                                        "          S.MALINCINSI HMADI" +
                                        "   FROM " + vegaDb + ".DBO.F0" + FirmaId + "TBLSTOKLAR S" +
                                        "   LEFT JOIN  " + vegaDb + ".DBO.F0" + FirmaId + "TBLBIRIMLEREX B ON B.IND=S.BIRIMEX" +
                                        "   WHERE S.KOD5='SEFIMWEBREPORT'" +
                                        "     AND STATUS IN (0," +
                                        "                    1))" +
                                        " select HMADI," +
                                        "       sum(recetemiktar) recetemiktar," +
                                        "       sum(entolmayanrecetemiktar) recetemiktar," +
                                        "       BIRIMADI," +
                                        "       ReceteBirimMaliyet," +
                                        "       (sum(ISNULL(recetemiktar, 0)) +sum(ISNULL(entolmayanrecetemiktar, 0))) * ISNULL(ReceteBirimMaliyet, 0) ReceteKullanilanTutar," +
                                        "       ISNULL(ENVANTER, 0) DonemBasiEnvanter," +
                                        "       ISNULL(DIENVANTER, 0)+sum(ISNULL(recetemiktar, 0)) DonemIciGonderilen," +
                                        "       ISNULL(DICENVANTER, 0)+sum(ISNULL(recetemiktar, 0)) +sum(ISNULL(entolmayanrecetemiktar, 0)) as DonemIciCikan," +
                                        "       ISNULL(ENVANTER, 0) + ((ISNULL(DIENVANTER, 0)+ISNULL(DICENVANTER, 0))+sum(ISNULL(recetemiktar, 0))) - (sum(ISNULL(recetemiktar, 0)) +sum(ISNULL(entolmayanrecetemiktar, 0))) Kalan" +
                                        " from" +
                                        "  (select maliyet.HMADI," +
                                        "          0  recetemiktar," +
                                        "          0  entolmayanrecetemiktar," +
                                        "          '' BIRIMADI," +
                                        "          envanter.ENVANTER," +
                                        "          DIenvanter.DIENVANTER," +
                                        "          DICenvanter.DICENVANTER," +
                                        "          0 pkeyvalue," +
                                        "          0 ReceteBirimMaliyet" +
                                        "   from maliyet" +
                                        "  " +
                                        "   LEFT JOIN TOTALSATIS on TOTALSATIS.ProductName=maliyet.HMADI" +
                                        "   LEFT JOIN envanter on envanter.SIND=maliyet.IND" +
                                        "   LEFT JOIN DIenvanter on DIenvanter.SIND=maliyet.IND" +
                                        "   LEFT JOIN DICenvanter on DICenvanter.SIND=maliyet.IND) as t" +
                                        " Where ISNULL(t.HMADI, '')<>''  and  ISNULL(DIENVANTER, 0)+ISNULL(DICENVANTER, 0)+ISNULL(ENVANTER, 0) <> 0 " +

                                        " GROUP BY HMADI," +
                                        "         BIRIMADI," +
                                        "         ENVANTER," +
                                        "         DIENVANTER," +
                                        "         DICENVANTER," +
                                        "         ReceteBirimMaliyet" +
                                        " order by HMADI";


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

                            //Query =
                            //        " declare @Sube nvarchar(100) = '{SUBEADI}';" +
                            //        " declare @Trh1 nvarchar(20) = '{TARIH1}';" +
                            //        " declare @Trh2 nvarchar(20) = '{TARIH2}';" +
                            //        " select" +
                            //        " @Sube, Sube, Sube1, Kasa_No as Kasa ,sUM(Nakit) AS Cash, Sum(Visa) as Credit,Sum(Ticket) as Ticket,Sum(Debit) AS Debit, Sum(Tableno) as TableNo, Sum(Discount) as Discount,Sum(Ikram) as ikram,Sum(Zayi) as Zayi," +
                            //        " SUm(case when t.Iptal = 1 then t.Toplam else 0 end) as iptal," +
                            //        " SUm(case when t.Iptal = 0 then t.Toplam else 0 end) as ToplamCiro " +
                            //        " from( " +
                            //        " SELECT " +
                            //        " (SELECT top 1 OZELKOD1 FROM  " + vega_Db + "..F0" + Firma_NPOS + "TBLPOSKASATANIM WHERE KASANO = Hr.Kasa_No) AS Sube, " +
                            //        " (SELECT top 1 OZELKOD1 FROM   " + vega_Db + "..F0" + Firma_NPOS + "TBLPOSKASATANIM WHERE KASANO = Hr.Kasa_No) AS Sube1," +
                            //        "  hr.Kasa_no, " +
                            //        " ISNULL((SELECT SUM(Tutar) from " + DBName + "..Odeme WHERE Tus_no = 0 and belge_Id = Hr.belge_Id),0) AS Nakit," +
                            //        " ISNULL((SELECT SUM(Tutar) from " + DBName + "..Odeme WHERE Tus_no IN(1, 2, 3, 4) and belge_Id = Hr.belge_Id),0) AS Visa," +
                            //        " 0 AS Ticket, " +
                            //        " ISNULL((SELECT SUM(Tutar) from " + DBName + "..Odeme WHERE Tus_no IN(5, 6) and belge_Id = Hr.belge_Id),0) AS Debit," +
                            //        " 0 AS Ikram " +
                            //        " ,0 as Tableno " +
                            //        " ,SUM(DISCOUNTTOTAL) as Discount " +
                            //        " ,Iptal " +
                            //        " ,0 as Zayi, " +
                            //        " SUm(hr.Toplam) as Toplam " +
                            //        " FROM " + DBName + "..BELGE as hr " +
                            //        " where hr.Tarih >= @Trh1 AND hr.Tarih <= @Trh2 " +
                            //        " GROUP BY Sicil_No,Kasa_No,hr.Belge_ID,Iptal) as t " +
                            //        " group by " +
                            //        " Sube,Sube1,Kasa_No  ";

                            Query =
                            " declare @Sube nvarchar(100) = '{SUBEADI}';" +
                            " declare @Trh1 nvarchar(20) = '{TARIH1}';" +
                            " declare @Trh2 nvarchar(20) = '{TARIH2}';" +
                            " SELECT @Sube," +
                            "	Sube," +
                            "	Sube1," +
                            "	Kasa_No AS Kasa," +
                            "	sUM(Nakit) AS Cash," +
                            "	Sum(Visa) AS Credit," +
                            "	Sum(Ticket) AS Ticket," +
                            "	Sum(Debit) AS Debit," +
                            "	Sum(Tableno) AS TableNo," +
                            "	Sum(Discount) AS Discount," +
                            "	SUm(CASE " +
                            "			WHEN t.Iptal = 1" +
                            "				THEN t.Toplam" +
                            "			ELSE 0" +
                            "			END) AS iptal," +
                            "	SUm(CASE " +
                            "			WHEN t.Iptal = 0" +
                            "				THEN t.Toplam" +
                            "			ELSE 0" +
                            "			END)/Sum(Ticket) AS ikram," +
                            "	Sum(Zayi) AS Zayi," +
                            "	SUm(CASE " +
                            "			WHEN t.Iptal = 1" +
                            "				THEN t.Toplam" +
                            "			ELSE 0" +
                            "			END) AS iptal," +
                            "	SUm(CASE " +
                            "			WHEN t.Iptal = 0" +
                            "				THEN t.Toplam" +
                            "			ELSE 0" +
                            "			END) AS ToplamCiro" +
                            " FROM (" +
                            "	SELECT (" +
                            "			SELECT TOP 1 OZELKOD1" +

                            "		FROM  " + vegaDb + "..F0" + Firma_NPOS + "TBLPOSKASATANIM" +
                            "			WHERE KASANO = Hr.Kasa_No" +
                            "			) AS Sube," +
                            "		(" +
                            "			SELECT TOP 1 OZELKOD1" +
                            "			FROM  " + vegaDb + "..F0" + Firma_NPOS + "TBLPOSKASATANIM" +
                            "			WHERE KASANO = Hr.Kasa_No" +
                            "			) AS Sube1," +
                            "		hr.Kasa_no," +
                            "		ISNULL((" +
                            "				SELECT SUM(Tutar)" +
                            "				FROM  " + DBName + "..Odeme" +
                            "				WHERE Tus_no = 0" +
                            "					AND belge_Id = Hr.belge_Id" +
                            "				), 0) AS Nakit," +
                            "		ISNULL((" +
                            " SELECT SUM(Tutar)" +
                            "				FROM  " + DBName + "..Odeme" +
                            "				WHERE Tus_no IN (" +
                            "						1," +
                            "						2," +
                            "						3," +
                            "						4" +
                            "						)" +
                            "					AND belge_Id = Hr.belge_Id" +
                            "				), 0) AS Visa," +
                            "		count(*) AS Ticket," +
                            "		ISNULL((" +
                            "				SELECT SUM(Tutar)" +
                            "				FROM  " + DBName + "..Odeme" +
                            "				WHERE Tus_no IN (" +
                            "						5," +
                            "						6" +
                            "						)" +
                            "					AND belge_Id = Hr.belge_Id" +
                            "				), 0) AS Debit," +
                            "		0 AS Ikram," +
                            "		0 AS Tableno," +
                            "		SUM(DISCOUNTTOTAL) AS Discount," +
                            "		Iptal," +
                            "		0 AS Zayi," +
                            "		SUm(hr.Toplam) AS Toplam" +
                            "	FROM  " + DBName + "..BELGE AS hr" +
                            "	WHERE hr.Tarih >= @Trh1" +
                            "		AND hr.Tarih <= @Trh2" +
                            "	GROUP BY Sicil_No," +
                            "		Kasa_No," +
                            "		hr.Belge_ID," +
                            "		Iptal" +
                            "	) AS t" +
                            " GROUP BY Sube," +
                            "	Sube1," +
                            "	Kasa_No";

                            #endregion NPOS QUARY 
                        }

                        #endregion SEFİM YENI-ESKİ FASTER SQL

                        Query = Query.Replace("{SUBEADI}", SubeAdi);
                        Query = Query.Replace("{TARIH1}", QueryTimeStart);
                        Query = Query.Replace("{TARIH2}", QueryTimeEnd);
                        Query = Query.Replace("{SUBEADITBL}", FirmaId_SUBE);//F0101TBLKRDSUBELER
                        Query = Query.Replace("{KASAADI}", FirmaId_KASA);//F0101TBLKRDKASALAR
                        Query = Query.Replace("{FIRMAIND}", Firma_NPOS);

                        if (ID == "1")
                        {
                            #region SUPER ADMİN 

                            try
                            {
                                DataTable SubeCiroDt = new DataTable();
                                SubeCiroDt = f.GetSubeDataWithQuery(f.NewConnectionString(SubeIP, DBName, SqlName, SqlPassword), Query.ToString());

                                try
                                {
                                    if (SubeCiroDt.Rows.Count > 0)
                                    {
                                        if (AppDbType == "3" || AppDbType == "4")
                                        {
                                            #region FASTER (AppDbType=3 faster kullanan şube) 

                                            foreach (DataRow sube in SubeCiroDt.Rows)
                                            {
                                                HamMaddeKullanimViewModel items = new HamMaddeKullanimViewModel();
                                                if (AppDbType == "3")
                                                {
                                                    items.Sube = SubeAdi; //sube["Sube"].ToString() + "-" + sube["Sube1"].ToString() + "-" + sube["Kasa"].ToString(); //KasaCiroDt.Rows[0]["Sube"].ToString();
                                                }
                                                else
                                                {
                                                    items.Sube = SubeAdi;//--+ "_" + sube["Sube"].ToString() + "-" + sube["Sube1"].ToString() + "-" + sube["Kasa"].ToString(); //KasaCiroDt.Rows[0]["Sube"].ToString();
                                                }
                                                items.SubeId = SubeId;
                                                items.HMADI = sube["HMADI"].ToString();//Convert.ToDecimal(SubeCiroDt.Rows[su]["Cash"]); //f.RTD(SubeR, "Cash");
                                                items.BIRIMADI = sube["BIRIMADI"].ToString();//f.RTD(SubeR, "Credit");
                                                items.ReceteBirimMaliyeti = f.RTD(sube, "ReceteBirimMaliyeti") == null ? 0 : f.RTD(sube, "ReceteBirimMaliyeti");
                                                items.ReceteKullanilanTutar = f.RTD(sube, "ReceteKullanilanTutar") == null ? 0 : f.RTD(sube, "ReceteKullanilanTutar");
                                                items.DonemBasiEnvanter = f.RTD(sube, "DonemBasiEnvanter") == null ? 0 : f.RTD(sube, "DonemBasiEnvanter");
                                                items.DonemIciGonderilen = f.RTD(sube, "DonemIciGonderilen") == null ? 0 : f.RTD(sube, "DonemIciGonderilen");
                                                items.DonemIciCikan = f.RTD(sube, "DonemIciCikan") == null ? 0 : f.RTD(sube, "DonemIciCikan");
                                                items.Kalan = f.RTD(sube, "Kalan") == null ? 0 : f.RTD(sube, "Kalan");

                                                lock (locked)
                                                {
                                                    Liste.Add(items);
                                                }
                                            }

                                            #endregion FASTER (AppDbType=3 faster kullanan şube)
                                        }
                                        else
                                        {
                                            foreach (DataRow sube in SubeCiroDt.Rows)
                                            {
                                                HamMaddeKullanimViewModel items = new HamMaddeKullanimViewModel
                                                {
                                                    Sube = SubeAdi,
                                                    SubeId = SubeId,
                                                    HMADI = sube["HMADI"].ToString(),
                                                    BIRIMADI = sube["BIRIMADI"].ToString(),
                                                    ReceteBirimMaliyeti = f.RTD(sube, "ReceteBirimMaliyeti") == null ? 0 : f.RTD(sube, "ReceteBirimMaliyeti"),
                                                    ReceteKullanilanTutar = f.RTD(sube, "ReceteKullanilanTutar") == null ? 0 : f.RTD(sube, "ReceteKullanilanTutar"),
                                                    DonemBasiEnvanter = f.RTD(sube, "DonemBasiEnvanter") == null ? 0 : f.RTD(sube, "DonemBasiEnvanter"),
                                                    DonemIciGonderilen = f.RTD(sube, "DonemIciGonderilen") == null ? 0 : f.RTD(sube, "DonemIciGonderilen"),
                                                    DonemIciCikan = f.RTD(sube, "DonemIciCikan") == null ? 0 : f.RTD(sube, "DonemIciCikan"),
                                                    Kalan = f.RTD(sube, "Kalan")
                                                };

                                                lock (locked)
                                                {
                                                    Liste.Add(items);
                                                }
                                            }
                                        }
                                    }
                                    else
                                    {
                                        HamMaddeKullanimViewModel items = new HamMaddeKullanimViewModel
                                        {
                                            Sube = SubeAdi + " (Data Yok) ",
                                            SubeId = SubeId
                                        };

                                        Liste.Add(items);
                                    }
                                }
                                catch (Exception ex)
                                {
                                    Singleton.WritingLogFile2("HamMaddeKullanimCRUD", ex.ToString(), null, ex.StackTrace);
                                }
                            }
                            catch (Exception ex)
                            {
                                #region EX  
                                try
                                {
                                    Singleton.WritingLogFile2("HamMaddeKullanimCRUD", ex.ToString(), null, ex.StackTrace);
                                    HamMaddeKullanimViewModel items = new HamMaddeKullanimViewModel
                                    {
                                        Sube = SubeAdi + " (Erişim Yok)",
                                        SubeId = SubeId,
                                    };
                                    Liste.Add(items);
                                }
                                catch (Exception) { }
                                #endregion
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
                                        SubeCiroDt = f.GetSubeDataWithQuery(f.NewConnectionString(SubeIP, DBName, SqlName, SqlPassword), Query.ToString());

                                        try
                                        {
                                            if (SubeCiroDt.Rows.Count > 0)
                                            {
                                                if (AppDbType == "3" || AppDbType == "4")
                                                {
                                                    #region FASTER (AppDbType=3 faster kullanan şube) 

                                                    foreach (DataRow sube in SubeCiroDt.Rows)
                                                    {
                                                        HamMaddeKullanimViewModel items = new HamMaddeKullanimViewModel();
                                                        if (AppDbType == "3")
                                                        {
                                                            items.Sube = sube["Sube"].ToString() + "-" + sube["Sube1"].ToString() + "-" + sube["Kasa"].ToString(); //KasaCiroDt.Rows[0]["Sube"].ToString();
                                                        }
                                                        else
                                                        {
                                                            items.Sube = SubeAdi + "_" + sube["Sube"].ToString() + "-" + sube["Sube1"].ToString() + "-" + sube["Kasa"].ToString(); //KasaCiroDt.Rows[0]["Sube"].ToString();
                                                        }
                                                        items.SubeId = SubeId;
                                                        items.HMADI = sube["HMADI"].ToString();//Convert.ToDecimal(SubeCiroDt.Rows[su]["Cash"]); //f.RTD(SubeR, "Cash");
                                                        items.BIRIMADI = sube["BIRIMADI"].ToString();//f.RTD(SubeR, "Credit");
                                                        items.ReceteBirimMaliyeti = Convert.ToDecimal(sube["ReceteBirimMaliyeti"]);//f.RTD(SubeR, "Ticket");
                                                        items.ReceteKullanilanTutar = Convert.ToDecimal(sube["ReceteKullanilanTutar"]); //f.RTD(SubeR, "ikram");
                                                        items.DonemBasiEnvanter = Convert.ToDecimal(sube["DonemBasiEnvanter"]); //f.RTD(SubeR, "TableNo");
                                                        items.DonemIciGonderilen = Convert.ToDecimal(sube["DonemIciGonderilen"]); //f.RTD(SubeR, "Discount");                                         
                                                        items.DonemIciCikan = Convert.ToDecimal(sube["DonemIciCikan"]);//f.RTD(SubeR, "Zayi");                                                                                      
                                                        items.Kalan = Convert.ToDecimal(sube["Kalan"]); //f.RTD(SubeR, "Debit");

                                                        lock (locked)
                                                        {
                                                            Liste.Add(items);
                                                        }
                                                    }

                                                    #endregion FASTER (AppDbType=3 faster kullanan şube)
                                                }
                                                else
                                                {
                                                    foreach (DataRow sube in SubeCiroDt.Rows)
                                                    {
                                                        HamMaddeKullanimViewModel items = new HamMaddeKullanimViewModel
                                                        {
                                                            Sube = SubeAdi,
                                                            SubeId = SubeId,
                                                            HMADI = sube["HMADI"].ToString(),
                                                            BIRIMADI = sube["BIRIMADI"].ToString(),
                                                            ReceteBirimMaliyeti = f.RTD(sube, "ReceteBirimMaliyeti") == null ? 0 : f.RTD(sube, "ReceteBirimMaliyeti"),
                                                            ReceteKullanilanTutar = f.RTD(sube, "ReceteKullanilanTutar") == null ? 0 : f.RTD(sube, "ReceteKullanilanTutar"),
                                                            DonemBasiEnvanter = f.RTD(sube, "DonemBasiEnvanter") == null ? 0 : f.RTD(sube, "DonemBasiEnvanter"),
                                                            DonemIciGonderilen = f.RTD(sube, "DonemIciGonderilen") == null ? 0 : f.RTD(sube, "DonemIciGonderilen"),
                                                            DonemIciCikan = f.RTD(sube, "DonemIciCikan") == null ? 0 : f.RTD(sube, "DonemIciCikan"),
                                                            Kalan = f.RTD(sube, "Kalan")
                                                        };

                                                        lock (locked)
                                                        {
                                                            Liste.Add(items);
                                                        }
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                HamMaddeKullanimViewModel items = new HamMaddeKullanimViewModel
                                                {
                                                    Sube = SubeAdi + " (Data Yok) ",
                                                    SubeId = SubeId
                                                };

                                                Liste.Add(items);
                                            }
                                        }
                                        catch (Exception ex)
                                        {
                                            Singleton.WritingLogFile2("HamMaddeKullanimCRUD", ex.ToString(), null, ex.StackTrace);
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        #region EX  
                                        try
                                        {
                                            Singleton.WritingLogFile2("HamMaddeKullanimCRUD", ex.ToString(), null, ex.StackTrace);
                                            HamMaddeKullanimViewModel items = new HamMaddeKullanimViewModel
                                            {
                                                Sube = SubeAdi + " (Erişim Yok)",
                                                SubeId = SubeId,
                                            };
                                            Liste.Add(items);
                                        }
                                        catch (Exception) { }
                                        #endregion
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