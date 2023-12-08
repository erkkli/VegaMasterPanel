declare @par1 nvarchar(20) = '{TARIH1}';
declare @par2 nvarchar(20) = '{TARIH2}';
declare @par3 nvarchar(50) = '{ProductName}';

--güncelhali

;WITH 

ProEqu AS 
(
SELECT ISNULL(pq.ProductName, '') + ISNULL('.' + (
      SELECT name
      FROM Choice1 ch1
      WHERE ch1.Id = pq.choice1Id
      ), '') + ISNULL('.' + (
      SELECT name
      FROM Choice2 ch2
      WHERE ch2.Id = pq.choice2Id
      ), '') ProductName
  ,Multiplier
  ,EquProductName
FROM ProductEqu Pq
),
 pr AS
  (SELECT product.Id ProductId,
          ISNULL(Product.ProductName, '')+ISNULL('.'+Choice1.Name, '')+ISNULL('.'+Choice2.Name, '') prname,ProductGroup,
          InvoiceName,
		  Product.Price+ISNULL(Choice1.Price,0)+ISNULL(Choice2.Price,0) price
   FROM Product
   LEFT JOIN Choice1 ON Choice1.ProductId=Product.Id
   LEFT JOIN Choice2 ON Choice2.ProductId=Product.Id
   AND Choice2.Choice1Id=Choice1.Id)
,optequ AS 
(
SELECT oq.ProductName
  ,(oq.ProductName + oq.Options) oqproname
  ,EquProduct
  ,miktar
  ,AnaUrun
FROM OptionsEqu oq
),
Base AS 
(
SELECT 
   B.ProductId
  ,B.ProductName
  ,P.ProductName PName
  ,B.Date PaymentTime
  ,B.Quantity
    ,b.Options
  ,CASE WHEN CHARINDEX('x', LTRIM(RTRIM(O.s))) > 0 THEN SUBSTRING(LTRIM(RTRIM(O.s)), CHARINDEX('x', LTRIM(RTRIM(O.s))) + 1, 100) ELSE LTRIM(RTRIM(O.s)) END AS Opt
  ,CASE WHEN CHARINDEX('x', LTRIM(RTRIM(O.s))) > 0 THEN SUBSTRING(LTRIM(RTRIM(O.s)), 1, CHARINDEX('x', LTRIM(RTRIM(O.s))) - 1) ELSE '1' END AS OptQty
  ,B.HeaderId
FROM Bill B
    LEFT JOIN Product P ON B.ProductId = P.Id
    CROSS APPLY dbo.SplitString(',', B.Options) AS O
WHERE ISNULL(B.Options, '') <> '' 
  AND B.Date BETWEEN @par1 AND @par2
  --and B.HeaderId=181336
),
BillPrice AS
(
SELECT
   Bp.ProductName
  ,Bp.Price
  ,Bp.MinDate
  ,Bp.Options
  ,CASE WHEN Bp.MaxDate=MAX(Bp.MaxDate) OVER(PARTITION BY Bp.ProductName) THEN GETDATE() ELSE Bp.MaxDate END MaxDate 
FROM
     (
      SELECT 
         ProductName
        ,Price
		,Options
        ,MIN(Date) MinDate
        ,MAX(Date) MaxDate 
      FROM Bill B 
	  	  where UserName='PAKET'
      GROUP BY ProductName,Price,Options
     ) Bp
),
OptSatislar AS
(
SELECT 
   Oe.EquProduct
  ,Oe.Miktar
  ,B.ProductName
  ,B.HeaderId
  ,B.PaymentTime
  ,B.ProductId
  ,Quantity * (CASE WHEN ISNUMERIC(OptQty) = 1 THEN CAST(OptQty AS INT) ELSE 1 END) AS Quantity
  ,Opt AS OptionsName
  ,B.ProductName aaa
  ,ISNULL(Oe.EquProduct, opt) ProductName2
  ,case when ISNULL(Oe.MenuFiyat,0)=0 THEN  CASE WHEN oe.AnaUrun=1 THEN MAX(Bp.Price) ELSE ISNULL(Oe.MenuFiyat,0) END ELSE ISNULL(Oe.MenuFiyat,0) END  MenuFiyat
FROM Base B
     LEFT JOIN OptionsEqu Oe On Oe.ProductName+Oe.Options=B.PName+ISNULL(B.Opt, '')
     LEFT JOIN BillPrice Bp On B.ProductName+ISNULL(b.Options,'')=Bp.ProductName +ISNULL(bp.Options,'')
	  AND B.PaymentTime BETWEEN Bp.MinDate and Bp.MaxDate
	 	 	  where (select top 1 Category from Options where [name]=b.Opt) <>'RAPORDISI' OR  (opt not like '%istemiyorum%' and opt not like '%istiyorum%')
GROUP BY 
   Oe.EquProduct
  ,Oe.Miktar
  ,oe.AnaUrun
  ,B.ProductName
  ,B.HeaderId
  ,B.PaymentTime
  ,B.ProductId
  ,Oe.MenuFiyat
  ,Quantity * (CASE WHEN ISNUMERIC(OptQty) = 1 THEN CAST(OptQty AS INT) ELSE 1 END)
  ,Opt
)
  ,opttotal
AS (
  SELECT optsatislar.ProductId
    ,ISNULL(EquProduct, optsatislar.ProductName) BillProductName
    ,ISNULL(EquProduct, optsatislar.OptionsName ) ProductName
    ,'Secenekler' ProductGroup
    ,'' InvoiceName
    ,sum(Quantity) OrjToplam
    ,sum(Quantity * ISNULL(Miktar, 1)) Toplam
    ,(sum(MenuFiyat)) * sum(Quantity) OrjTutar
    ,CASE WHEN sum(MenuFiyat) = 0 AND SUM(ISNULL(Miktar, 0)) > 0 THEN 0 ELSE sum(optsatislar.MenuFiyat*Quantity) END Tutar
  FROM optsatislar
  LEFT JOIN  pr on  pr.prname=optsatislar.EquProduct
  GROUP BY EquProduct
    ,optsatislar.ProductName
    ,optsatislar.OptionsName
    ,
    --  pr.InvoiceName,
    	pr.ProductGroup,
    optsatislar.ProductId
  )
  ,indirim
AS (
  SELECT TOP 9999999999 ISNULL(avg(ISNULL(b2.discount, 1)), 1) / (CASE WHEN sum(Quantity * Price) = 0 THEN 1 ELSE sum(Quantity * Price) END) indirimtutar
    ,HeaderId indHeaderId
  FROM PaidBill b2
  WHERE PaymentTime >= @par1
  GROUP BY HeaderId
  )
  ,paketindirim
AS (
  SELECT TOP 9999999 ISNULL(ph.discount, 0) / (
      SELECT sum(Quantity * Price)
      FROM bill b2
      WHERE b2.HeaderId = ph.HeaderId
      ) pktindirim
    ,HeaderId pktheaderId
  FROM PhoneOrderHeader ph
  )
  ,satislar2
AS (
  SELECT pq.EquProductName
    ,P.Id AS ProductId
    ,SUM(Quantity * ISNULL(pq.Multiplier, 1)) AS Toplam
	,SUM(Quantity ) AS Toplam2
    ,ISNULL(pq.EquProductName, B.ProductName) ProductName
    ,P.ProductGroup
    ,SUM((ISNULL(Quantity, 0) * CASE WHEN ISNULL(pq.Multiplier, 1)=0 THEN 1 ELSE ISNULL(pq.Multiplier, 1) END  ) * ISNULL(B.Price, 0)) AS Tutar
    ,(
      SELECT SUM(Bill.Price * Bill.Quantity)
      FROM dbo.BillWithHeader AS Bill
      WHERE BillState = 0 AND HeaderId = b.HeaderId AND ProductId = b.ProductId
      ) AS OPENTABLE
    ,(
      SELECT SUM(Bill.Quantity)
      FROM dbo.BillWithHeader AS Bill
      WHERE BillState = 0 AND HeaderId = b.HeaderId AND ProductId = b.ProductId
      ) AS OPENTABLEQuantity
    ,0 AS Discount
    ,SUM((ISNULL(Quantity, 0) * ISNULL(pq.Multiplier, 1)) * ISNULL(B.Price, 0)) * (avg(ind.indirimtutar)) INDIRIM
    ,SUM((ISNULL(Quantity, 0) * ISNULL(pq.Multiplier, 1)) * ISNULL(B.Price, 0)) * avg(pkt.pktindirim) PAKETINDIRIM
    ,CASE WHEN sum(ISNULL(B.Price, 0)) = 0 THEN sum((ISNULL(B.OriginalPrice, 0))) ELSE 0 END IKRAM
  FROM dbo.Bill AS b WITH (NOLOCK)
  LEFT JOIN proequ pq ON pq.ProductName = B.ProductName
  LEFT JOIN dbo.Product AS P ON P.Id = B.ProductId
  LEFT JOIN indirim AS ind ON ind.indHeaderId = b.HeaderId
  LEFT JOIN paketindirim AS pkt ON pkt.pktheaderId = b.HeaderId
  WHERE [Date] >= @par1 AND [Date] <= @par2 AND P.ProductName NOT LIKE '$%'
  GROUP BY pq.EquProductName
    ,B.ProductName
    ,b.ProductId
    ,P.ProductGroup
    ,B.HeaderId
    ,p.Id
  )

SELECT sum(MIKTAR) MIKTAR
  ,a.ProductName
  ,a.ProductGroup
  ,Sum(TUTAR) TUTAR
  ,SUM(INDIRIM) INDIRIM
  ,SUM(IKRAM) IKRAM
FROM (
  SELECT CASE WHEN sum(opttotal.Toplam) = 0 THEN sum(opttotal.OrjToplam) ELSE sum(opttotal.Toplam) END AS MIKTAR
    ,ProductName
    ,ProductGroup AS ProductGroup
    ,sum(ISNULL(opttotal.Tutar, 0)) AS TUTAR
    ,0 INDIRIM
    ,0 IKRAM
  FROM opttotal
  GROUP BY opttotal.ProductName
    ,opttotal.ProductGroup
  
  UNION
  
  SELECT sum(toplam) AS MIKTAR
    ,ProductName
    ,ProductGroup
    ,sum(tutar) AS TUTAR
    ,SUM(ISNULL(INDIRIM, 0)) + SUM(ISNULL(PAKETINDIRIM, 0)) INDIRIM
    ,SUM(IKRAM) IKRAM
  FROM satislar2
  GROUP BY ProductName
    ,ProductGroup
    ,ProductId
  ) AS a
  WHERE 
  MIKTAR<>0 and 
a.ProductGroup=@par3
and  a.ProductName NOT LIKE '$%'


group by
a.ProductName,
a.ProductGroup
order by
a.ProductGroup,
a.ProductName


--WITH base as (
--SELECT
--	PaymentTime,
--	Quantity,
--	LTRIM(RTRIM(split.s)) AS Opt,
--	ProductId
--FROM
--	dbo.PaidBill
--CROSS APPLY
--	dbo.SplitString(',',PaidBill.Options)  as split
--WHERE
--	ISNULL(Options,'')<>'' AND PaymentTime>=@par1 AND PaymentTime <= @par2
--	),
--base_with_optqty as (
--SELECT 
--	base.PaymentTime,
--	base.Quantity,
--	ProductId,
--	CASE 
--		WHEN CHARINDEX('x',base.Opt) > 0 THEN
--		SUBSTRING(base.Opt,1,CHARINDEX('x',base.Opt)-1)
--	ELSE
--		'1'
--	END AS OptQty,
	
--	CASE 
--		WHEN CHARINDEX('x',base.Opt) > 0 THEN
--		SUBSTRING(base.Opt,CHARINDEX('x',base.Opt)+1,100)
--	ELSE
--		base.Opt
--	END AS Opt	
--FROM 
--	base),
	
--satislar as (	
--SELECT	
--	PaymentTime,
--	ProductId,
--	Quantity * (CASE WHEN ISNUMERIC(OptQty) = 1 THEN CAST(OptQty AS INT) ELSE 1 END ) AS Quantity,	
--	Opt as ProductName
--from
--	base_with_optqty	
--)
--select * from (

--SELECT 

--sum(Quantity) as MIKTAR,
--ProductName,
--'Seçenekler' AS ProductGroup,
--0 as TUTAR
--from  satislar 
--group by satislar.ProductName

--UNION

--select 
--sum(t.toplam) as MIKTAR,
--t.ProductName,
--t.ProductGroup,
--sum(t.tutar) as TUTAR from (
--SELECT 
--P.Id as ProductId,
--SUM(B.Quantity) AS Toplam,B.ProductName,P.ProductGroup,SUM(ISNULL(B.Quantity,0) * ISNULL(B.Price,0)) AS Tutar,

--(SELECT SUM(Bill.Price*Bill.Quantity) FROM dbo.BillWithHeader AS Bill WHERE BillState=0
--and HeaderId=b.HeaderId AND ProductId=P.Id
--) AS OPENTABLE,
--(SELECT SUM(Bill.Quantity) FROM dbo.BillWithHeader AS Bill WHERE BillState=0
--and HeaderId=b.HeaderId AND ProductId=P.Id
--) AS OPENTABLEQuantity,
--(SELECT SUM(PAY.Discount) FROM dbo.Payment AS PAY
--where PAY.HeaderId=B.HeaderId
--) as Discount
--  FROM dbo.Bill AS B

--LEFT JOIN dbo.Product as P ON P.Id=B.ProductId

--WHERE
--[Date]>=@par1 AND [Date]<=@par2 and P.ProductName Not Like '$%'    AND ProductGroup='{ProductName}'
 


--GROUP BY B.ProductName,
--P.ProductGroup,
--B.HeaderId,
--p.Id ) as t

--WHERE t.ProductGroup ='{ProductName}'
--group by
--t.ProductName,
--t.ProductGroup,
--t.ProductId ) as a
--where a.ProductGroup ='{ProductName}'
--order by
--a.ProductGroup




--WITH
--BillWithHeaderx AS (SELECT bw.*,p.ProductGroup FROM BillWithHeader bw
--left join product  p on p.Id=bw.ProductId
--where p.ProductGroup ='Seçenekler'
--  )
--select ProductGroup,ProductName, SUM(Price * Quantity) as TUTAR, SUM(Quantity) as MIKTAR from BillWithHeaderx
--            where [Date]>='2020.01.01 05:00:00' and [Date]< '2020.01.31 06:30:00' and (BillType=0 or BillType=1)
--            GROUP BY ProductGroup,ProductName