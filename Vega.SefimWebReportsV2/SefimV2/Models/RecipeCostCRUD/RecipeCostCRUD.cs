using SefimV2.Helper;
using SefimV2.ViewModels.User;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace SefimV2.Models
{
    public class RecipeCostCRUD
    {
        public static List<SubeUrun> List(DateTime Date1, DateTime Date2, string subeid, string ID, int kirilimNo, string productName, bool tumStoklarGetirilsinMi = false)
        {
            ModelFunctions mF = new ModelFunctions();
            List<SubeUrun> Liste = new List<SubeUrun>();

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

                mF.SqlConnOpen();
                string filter = "Where Status=1";
                if (subeid_ != null && !subeid_.Equals("0") && !subeid_.Equals(""))
                    filter += " and Id=" + subeid_;
                DataTable dt = mF.DataTable("select * from SubeSettings " + filter + " order by SubeName");
                mF.SqlConnClose();

                #endregion SUBSTATION LIST

                #region VEGA DB database ismini çekmek için.

                string vega_Db = string.Empty;
                mF.SqlConnOpen();
                DataTable dataVegaDb = mF.DataTable("select* from VegaDbSettings ");
                var vegaDBList = dataVegaDb.AsEnumerable().ToList<DataRow>();
                foreach (var item in vegaDBList)
                {
                    vega_Db = item["DBName"].ToString();
                }
                mF.SqlConnClose();

                #endregion VEGA DB database ismini çekmek için. 

                #region PARALLEL FORECH

                var locked = new Object();
                var dtList = dt.AsEnumerable().ToList<DataRow>();
                Parallel.ForEach(dtList, r =>
                {
                    mF = new ModelFunctions();
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
                    string AppDbType = mF.RTS(r, "AppDbType");
                    string AppDbTypeStatus = mF.RTS(r, "AppDbTypeStatus");
                    string query = string.Empty;

                    #region Faster Kasalar seçildiyse ona göre query oluşturuluyor. 

                    string FasterSubeIND = mF.RTS(r, "FasterSubeID");
                    string QueryFasterSube = string.Empty;
                    if (FasterSubeIND != null)
                    {
                        QueryFasterSube = "  and  FSH.SUBEIND IN(" + FasterSubeIND + ") ";
                    }

                    #endregion Faster Kasalar seçildiyse ona göre query oluşturuluyor.

                    if ((AppDbType == "1" || AppDbType == "2") && kirilimNo == 0)
                    {
                        query =
                            " Declare @par1 nvarchar(20) = '{TARIH1}';" +
                            " Declare @par2 nvarchar(20) = '{TARIH2}';" +
                            " ;WITH " +
                            " maliyet AS" +
                            "  (SELECT MALIYET," +
                            "          IND," +
                            "          STOKKODU HMKODU," +
                            "          MALINCINSI HMADI" +
                            "   FROM " + vega_Db + ".DBO.F0" + FirmaId + "TBLSTOKLAR )," +
                            "   recete AS" +
                            "  (SELECT [Quantity]," +
                            "          StokID," +
                            "          ProductId," +
                            "          ProductName" +
                            "   FROM " + DBName + ".[dbo].[Bom])," +
                            "     receteoptions AS" +
                            "  (SELECT sum(Quantity) bommiktar," +
                            "          StokID," +
                            "          ProductName," +
                            "          OptionsName" +
                            "   FROM " + DBName + ".[dbo].BomOptions" +
                            "   WHERE ISNULL(MaliyetDahil, 0)=1" +
                            "   GROUP BY StokID," +
                            "            ProductName," +
                            "            OptionsName)," +
                            " ProEqu AS " +
                            " ( " +
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
                            " ) " +
                            " ,optequ AS " +
                            " ( " +
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
                            "   B.ProductId" +
                            "  ,B.ProductName" +
                            "  ,P.ProductName PName" +
                            "  ,B.Date PaymentTime" +
                            "  ,B.Quantity" +
                            "  ,b.Options" +
                            "  ,CASE WHEN CHARINDEX('x', LTRIM(RTRIM(O.s))) > 0 THEN SUBSTRING(LTRIM(RTRIM(O.s)), CHARINDEX('x', LTRIM(RTRIM(O.s))) + 1, 100) ELSE LTRIM(RTRIM(O.s)) END AS Opt" +
                            "  ,CASE WHEN CHARINDEX('x', LTRIM(RTRIM(O.s))) > 0 THEN SUBSTRING(LTRIM(RTRIM(O.s)), 1, CHARINDEX('x', LTRIM(RTRIM(O.s))) - 1) ELSE '1' END AS OptQty" +
                            "  ,B.HeaderId" +
                            " FROM Bill B" +
                            "    LEFT JOIN Product P ON B.ProductId = P.Id" +
                            "    CROSS APPLY dbo.SplitString(',', B.Options) AS O" +
                            " WHERE ISNULL(B.Options, '') <> '' " +
                            "  AND B.Date BETWEEN @par1 AND @par2" +
                            " )," +
                            " BillPrice AS" +
                            " ( " +
                            " SELECT" +
                            "   Bp.ProductName" +
                            "  ,Bp.Price" +
                            "  ,Bp.MinDate" +
                            "  ,Bp.Options" +
                            "  ,CASE WHEN Bp.MaxDate=MAX(Bp.MaxDate) OVER(PARTITION BY Bp.ProductName) THEN GETDATE() ELSE Bp.MaxDate END MaxDate " +
                            " FROM" +
                            "     (" +
                            "      SELECT " +
                            "       ProductName" +
                            "      ,Price" +
                            "	   ,Options" +
                            "      ,MIN(Date) MinDate" +
                            "      ,MAX(Date) MaxDate " +
                            "     FROM Bill B " +
                            "	  	  where UserName='PAKET' " +
                            "      GROUP BY ProductName,Price,Options " +
                            "     ) Bp" +
                            ")," +
                            " OptSatislar AS" +
                            " (" +
                            " SELECT " +
                            "   Oe.EquProduct" +
                            "  ,Oe.Miktar" +
                            "  ,B.ProductName" +
                            "  ,B.HeaderId" +
                            "  ,B.PaymentTime" +
                            "  ,B.ProductId" +
                            "  ,Quantity * (CASE WHEN ISNUMERIC(OptQty) = 1 THEN CAST(OptQty AS INT) ELSE 1 END) AS Quantity" +
                            "  ,Opt AS OptionsName" +
                            "  ,B.ProductName aaa" +
                            "  ,ISNULL(Oe.EquProduct, opt) ProductName2" +
                            "  ,case when ISNULL(Oe.MenuFiyat,0)=0 THEN  CASE WHEN oe.AnaUrun=1 THEN MAX(Bp.Price) ELSE ISNULL(Oe.MenuFiyat,0) END ELSE ISNULL(Oe.MenuFiyat,0) END  MenuFiyat" +
                            " FROM Base B" +
                            "     LEFT JOIN OptionsEqu Oe On Oe.ProductName+Oe.Options=B.PName+ISNULL(B.Opt, '')" +
                            "     LEFT JOIN BillPrice Bp On Oe.EquProduct=Bp.ProductName AND B.PaymentTime BETWEEN Bp.MinDate and Bp.MaxDate" +
                            "	 	 where opt not like '%istemiyorum%' or opt not like '%istiyorum%'" +
                            " GROUP BY " +
                            "   Oe.EquProduct" +
                            "  ,Oe.Miktar" +
                            "  ,oe.AnaUrun" +
                            "  ,B.ProductName" +
                            "  ,B.HeaderId" +
                            "  ,B.PaymentTime" +
                            "  ,B.ProductId" +
                            "  ,Oe.MenuFiyat" +
                            "  ,Quantity * (CASE WHEN ISNUMERIC(OptQty) = 1 THEN CAST(OptQty AS INT) ELSE 1 END)" +
                            "  ,Opt" +
                            " ) " +
                            "  ,opttotal" +
                            " AS (" +
                            "  SELECT optsatislar.ProductId" +
                            "    ,ISNULL(EquProduct, optsatislar.ProductName) BillProductName" +
                            "    ,ISNULL(EquProduct, optsatislar.OptionsName ) ProductName" +
                            "    ,'' ProductGroup" +
                            "    ,'' InvoiceName" +
                            "    ,sum(Quantity) OrjToplam" +
                            "    ,sum(Quantity * ISNULL(Miktar, 1)) Toplam" +
                            "    ,(sum(MenuFiyat)) * sum(Quantity) OrjTutar" +
                            "    ,CASE WHEN sum(MenuFiyat) = 0 AND SUM(ISNULL(Miktar, 0)) > 0 THEN 0 ELSE sum(optsatislar.MenuFiyat*Quantity) END Tutar" +
                            "	       ,sum(receteoptions.bommiktar) RECETEMIKTAR" +
                            "                                 ,sum(Quantity * ISNULL(Miktar, 1))*sum(ISNULL(receteoptions.bommiktar,0))  RECETEMIKTARTOPLAM" +
                            "                         " +
                            "                                 , sum(maliyet.MALIYET) HMMALIYET" +
                            "                               ,   (SUM(optsatislar.Quantity*ISNULL(receteoptions.bommiktar,0)))*sum(ISNULL(maliyet.MALIYET,0)) RECETETUTAR" +
                            "  FROM optsatislar" +
                            "   LEFT JOIN receteoptions ON receteoptions.OptionsName=optsatislar.OptionsName AND receteoptions.ProductName=OptSatislar.ProductName" +
                            "   LEFT JOIN maliyet ON maliyet.IND=receteoptions.StokID" +
                            "  GROUP BY EquProduct" +
                            "    ,optsatislar.ProductName" +
                            "    ,optsatislar.OptionsName" +
                            "    ," +
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
                            "  ) " +
                            "  ,satislar2" +
                            " AS (" +
                            "  SELECT pq.EquProductName" +
                            "    ,P.Id AS ProductId" +
                            "    ,SUM(b.Quantity * ISNULL(pq.Multiplier, 1)) AS Toplam" +
                            "    ,ISNULL(pq.EquProductName, B.ProductName) ProductName" +
                            "    ,P.ProductGroup" +
                            "    ,SUM((ISNULL(b.Quantity, 0) * CASE WHEN ISNULL(pq.Multiplier, 1)=0 THEN 1 ELSE ISNULL(pq.Multiplier, 1) END  ) * ISNULL(B.Price, 0)) AS Tutar" +
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
                            "    ,SUM((ISNULL(b.Quantity, 0) * ISNULL(pq.Multiplier, 1)) * ISNULL(B.Price, 0)) * (avg(ind.indirimtutar)) INDIRIM" +
                            "    ,SUM((ISNULL(b.Quantity, 0) * ISNULL(pq.Multiplier, 1)) * ISNULL(B.Price, 0)) * avg(pkt.pktindirim) PAKETINDIRIM" +
                            "    ,CASE WHEN sum(ISNULL(B.Price, 0)) = 0 THEN sum((ISNULL(B.OriginalPrice, 0))) ELSE 0 END IKRAM" +
                            "	       ,(recete.Quantity) RECETEMIKTAR" +
                            "	                                 , SUM(b.Quantity*ISNULL(recete.Quantity,0))  RECETEMIKTARTOPLAM" +
                            "                         " +
                            "                                 , maliyet.MALIYET HMMALIYET" +
                            "                               ,   (SUM(b.Quantity*ISNULL(recete.Quantity,0)))*(ISNULL(maliyet.MALIYET,0)) RECETETUTAR" +
                            "  FROM dbo.Bill AS b WITH (NOLOCK)" +
                            "  LEFT JOIN proequ pq ON pq.ProductName = B.ProductName" +
                            "  LEFT JOIN dbo.Product AS P ON P.Id = B.ProductId" +
                            "  LEFT JOIN indirim AS ind ON ind.indHeaderId = b.HeaderId" +
                            "  LEFT JOIN paketindirim AS pkt ON pkt.pktheaderId = b.HeaderId" +
                            "     LEFT JOIN recete ON recete.ProductId=b.ProductId" +
                            "   AND recete.ProductName=b.ProductName" +
                            "   LEFT JOIN maliyet ON maliyet.IND=recete.StokID" +
                            "  WHERE [Date] >= @par1 AND [Date] <= @par2 AND P.ProductName NOT LIKE '$%'" +
                            "  GROUP BY pq.EquProductName" +
                            "    ,B.ProductName" +
                            "    ,b.ProductId" +
                            "    ,P.ProductGroup" +
                            "    ,B.HeaderId" +
                            "    ,p.Id" +
                            "		,recete.Quantity" +
                            "		,b.Quantity" +
                            "	,maliyet.MALIYET" +
                            "  )" +
                            " SELECT sum(MIKTAR) MIKTAR" +
                            "  ,a.ProductName" +
                            "  ,Sum(TUTAR) TUTAR" +
                            "  ,SUM(INDIRIM) INDIRIM" +
                            "  ,SUM(IKRAM) IKRAM" +
                            "	,SUM(RECETEMIKTARTOPLAM) RECETEMIKTARTOPLAM" +
                            "	,SUM(RECETETUTAR) RECETETUTAR" +
                            " FROM (" +
                            "  SELECT CASE WHEN sum(opttotal.Toplam) = 0 THEN sum(opttotal.OrjToplam) ELSE sum(opttotal.Toplam) END AS MIKTAR" +
                            "    ,opttotal.ProductName" +
                            "    ,ProductGroup AS ProductGroup" +
                            "    ,sum(ISNULL(opttotal.Tutar, 0)) AS TUTAR" +
                            "    ,0 INDIRIM" +
                            "    ,0 IKRAM" +
                            "	,SUM(RECETEMIKTARTOPLAM) RECETEMIKTARTOPLAM" +
                            "	,SUM(RECETETUTAR) RECETETUTAR" +
                            "	                           " +
                            "  FROM opttotal" +
                            "  GROUP BY opttotal.ProductName" +
                            "    ,opttotal.ProductGroup" +
                            "  " +
                            "  UNION" +
                            "  " +
                            "  SELECT sum(toplam) AS MIKTAR" +
                            "    ,satislar2.ProductName" +
                            "    ,satislar2.ProductGroup" +
                            "    ,sum(tutar) AS TUTAR" +
                            "    ,SUM(ISNULL(INDIRIM, 0)) + SUM(ISNULL(PAKETINDIRIM, 0)) INDIRIM" +
                            "    ,SUM(IKRAM) IKRAM" +
                            "	,SUM(RECETEMIKTARTOPLAM) RECETEMIKTARTOPLAM" +
                            "	,SUM(RECETETUTAR) RECETETUTAR" +
                            "  FROM satislar2" +
                            "  GROUP BY satislar2.ProductName" +
                            "    ,satislar2.ProductGroup" +
                            "    ,satislar2.ProductId" +
                            "  ) AS a" +
                            "    where MIKTAR<>0 " +
                            " GROUP BY a.ProductName" +
                            " ORDER BY a.ProductName";
                    }
                    else if (kirilimNo == 3)
                    {
                        //query =
                        //       " Declare @par1 nvarchar(20) = '{TARIH1}';" +
                        //       " Declare @par2 nvarchar(20) = '{TARIH2}';" +
                        //       " Declare @par3 nvarchar(100) = '{ProductName}';" +
                        //       ";" +
                        //       " WITH maliyet AS" +
                        //       "  (SELECT S.MALIYET," +
                        //       "          S.IND," +
                        //       "		  B.BIRIMADI," +
                        //       "          S.STOKKODU HMKODU," +
                        //       "          S.MALINCINSI HMADI" +
                        //       "   FROM " + vega_Db + ".DBO.F0" + FirmaId + "TBLSTOKLAR S " +
                        //       "   LEFT JOIN " + vega_Db + ".DBO.F0" + FirmaId + "TBLBIRIMLEREX B ON B.IND=S.BIRIMEX )," +
                        //       "     recete AS" +
                        //       "  (SELECT " +
                        //       "  maliyet.HMKODU," +
                        //       "  maliyet.HMADI," +
                        //       " maliyet.BIRIMADI," +
                        //       "  maliyet.MALIYET," +
                        //       "  [Quantity] miktar," +
                        //       "          StokID," +
                        //       "          ProductId," +
                        //       "          ProductName" +
                        //       "   FROM [Bom]  " +
                        //       "   left join maliyet on maliyet.IND=[Bom].StokID )," +
                        //       "     receteoptions AS" +
                        //       "  (SELECT" +
                        //       "    maliyet.HMKODU," +
                        //       "  maliyet.HMADI," +
                        //       "   maliyet.BIRIMADI," +
                        //       "    maliyet.MALIYET," +
                        //       " Quantity miktar," +
                        //       "          StokID," +
                        //       "          ProductName," +
                        //       "          OptionsName" +
                        //       "   FROM [dbo].BomOptions" +
                        //       "     left join maliyet on maliyet.IND=[dbo].BomOptions.StokID " +
                        //       "   WHERE ISNULL(MaliyetDahil, 0)=1)" +
                        //       "   select " +
                        //       "   HMKODU,HMADI,Miktar,BIRIMADI,(miktar*MALIYET) MaliyetTutari,MALIYET" +
                        //       "   from (" +
                        //       "			select * from recete" +
                        //       "			union all" +
                        //       "			select * from receteoptions ) t" +
                        //       "			where t.ProductName=@par3 ";

                        query =
                              " Declare @par1 nvarchar(20) = '{TARIH1}';" +
                              " Declare @par2 nvarchar(20) = '{TARIH2}';" +
                              " Declare @par3 nvarchar(100) = '{ProductName}';" +
                              " WITH maliyet" +
                              " AS (" +
                              "	SELECT S.MALIYET" +
                              "		,S.IND" +
                              "		,B.BIRIMADI" +
                              "		,S.STOKKODU HMKODU" +
                              "		,S.MALINCINSI HMADI" +
                              "   FROM " + vega_Db + ".DBO.F0" + FirmaId + "TBLSTOKLAR S " +
                              "   LEFT JOIN " + vega_Db + ".DBO.F0" + FirmaId + "TBLBIRIMLEREX B ON B.IND=S.BIRIMEX )" +
                              "	,recete" +
                              " AS (" +
                              "	SELECT maliyet.HMKODU" +
                              "		,maliyet.HMADI" +
                              "		,maliyet.BIRIMADI" +
                              "		,maliyet.MALIYET" +
                              "		,[Quantity] miktar" +
                              "		,StokID" +
                              "		,ProductId" +
                              "		,ProductName" +
                              "	FROM [Bom]" +
                              "	LEFT JOIN maliyet ON maliyet.IND = [Bom].StokID" +
                              "	)" +
                              "	,receteoptions" +
                              " AS (" +
                              "	SELECT maliyet.HMKODU" +
                              "		,maliyet.HMADI" +
                              "		,maliyet.BIRIMADI" +
                              "		,maliyet.MALIYET" +
                              "		,Quantity miktar" +
                              "		,StokID" +
                              "		,ProductName" +
                              "		,OptionsName" +
                              "	FROM BomOptions" +
                              "	LEFT JOIN maliyet ON maliyet.IND = BomOptions.StokID" +
                              "	WHERE ISNULL(MaliyetDahil, 0) = 1" +
                              "	)" +
                              " SELECT HMKODU" +
                              "	,HMADI" +
                              "	,Miktar" +
                              "	,BIRIMADI" +
                              "	,(miktar * MALIYET) MaliyetTutari" +
                              "	,MALIYET" +
                              " FROM (" +
                              "	SELECT *" +
                              "	FROM recete" +
                              "	" +
                              "	UNION ALL" +
                              "	" +
                              "	SELECT *" +
                              "	FROM receteoptions" +
                              "	) t" +
                              " WHERE t.ProductName = @par3";

                    }


                    query = query.Replace("{SUBEADI}", SubeAdi);
                    query = query.Replace("{TARIH1}", QueryTimeStart);
                    query = query.Replace("{TARIH2}", QueryTimeEnd);
                    query = query.Replace("{FIRMAIND}", FirmaId);
                    query = query.Replace("{KASALAR}", FirmaId_KASA);
                    query = query.Replace("{SUBELER}", FirmaId_SUBE);
                    query = query.Replace("{ProductName}", productName);

                    if (ID == "1")
                    {
                        #region GET DATA   

                        try
                        {
                            try
                            {
                                DataTable SubeUrunCiroDt = new DataTable();
                                SubeUrunCiroDt = mF.GetSubeDataWithQuery(mF.NewConnectionString(SubeIP, DBName, SqlName, SqlPassword), query.ToString());

                                if (SubeUrunCiroDt.Rows.Count > 0)
                                {
                                    if (subeid.Equals("") && subeid != "exportExcel")
                                    { }
                                    else
                                    {
                                        #region Subeid ile detay alacak ise

                                        if (AppDbType == "3")
                                        { }
                                        else
                                        {
                                            #region SEFIM 

                                            foreach (DataRow SubeR in SubeUrunCiroDt.Rows)
                                            {
                                                SubeUrun items = new SubeUrun
                                                {
                                                    Sube = SubeAdi,
                                                    SubeID = (SubeId),
                                                    ProductName = mF.RTS(SubeR, "ProductName"),
                                                    ReceteTutari = mF.RTD(SubeR, "RECETETUTAR"),

                                                    HMKODU = mF.RTS(SubeR, "HMKODU"),
                                                    HMADI = mF.RTS(SubeR, "HMADI"),
                                                    Miktar = mF.RTD(SubeR, "Miktar"),
                                                    BIRIMADI = mF.RTS(SubeR, "BIRIMADI"),
                                                    MaliyetTutari = mF.RTD(SubeR, "MaliyetTutari"),
                                                    MALIYET = mF.RTD(SubeR, "MALIYET"),
                                                    ReceteToplamMiktari= mF.RTD(SubeR, "RECETEMIKTARTOPLAM"),
                                                };

                                                //lock (locked)
                                                //{
                                                Liste.Add(items);
                                                //}
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
                            catch (Exception) { throw new Exception(SubeAdi); }
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
                                        SubeUrunCiroDt = mF.GetSubeDataWithQuery(mF.NewConnectionString(SubeIP, DBName, SqlName, SqlPassword), query.ToString());

                                        if (SubeUrunCiroDt.Rows.Count > 0)
                                        {
                                            if (subeid.Equals(""))
                                            { }
                                            else
                                            {
                                                #region Subeid ile detay alacak ise

                                                if (AppDbType == "3")
                                                { }
                                                else
                                                {
                                                    #region SEFIM 

                                                    foreach (DataRow SubeR in SubeUrunCiroDt.Rows)
                                                    {
                                                        SubeUrun items = new SubeUrun
                                                        {
                                                            Sube = mF.RTS(SubeR, "Sube"),
                                                            SubeID = (SubeId),
                                                            ProductName = mF.RTS(SubeR, "ProductName"),
                                                            ReceteTutari = mF.RTD(SubeR, "RECETETUTAR"),

                                                            HMKODU = mF.RTS(SubeR, "HMKODU"),
                                                            HMADI = mF.RTS(SubeR, "HMADI"),
                                                            Miktar = mF.RTD(SubeR, "Miktar"),
                                                            BIRIMADI = mF.RTS(SubeR, "BIRIMADI"),
                                                            MaliyetTutari = mF.RTD(SubeR, "MaliyetTutari"),
                                                            MALIYET = mF.RTD(SubeR, "MALIYET"),
                                                        };

                                                        //lock (locked)
                                                        //{
                                                        Liste.Add(items);
                                                        //}
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
                                    catch (Exception) { throw new Exception(SubeAdi); }
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