declare @par1 nvarchar(20) = '{TARIH1}';
declare @par2 nvarchar(20) = '{TARIH2}';

WITH base as (
SELECT
	PaymentTime,
	Quantity,
	LTRIM(RTRIM(split.s)) AS Opt,
	ProductId
FROM
	dbo.PaidBill
CROSS APPLY
	dbo.SplitString(',',PaidBill.Options)  as split
WHERE
	ISNULL(Options,'')<>'' AND PaymentTime>=@par1 AND PaymentTime <= @par2
	),
base_with_optqty as (
SELECT 
	base.PaymentTime,
	base.Quantity,
	ProductId,
	CASE 
		WHEN CHARINDEX('x',base.Opt) > 0 THEN
		SUBSTRING(base.Opt,1,CHARINDEX('x',base.Opt)-1)
	ELSE
		'1'
	END AS OptQty,
	
	CASE 
		WHEN CHARINDEX('x',base.Opt) > 0 THEN
		SUBSTRING(base.Opt,CHARINDEX('x',base.Opt)+1,100)
	ELSE
		base.Opt
	END AS Opt	
FROM 
	base),
	
satislar as (	
SELECT	
	PaymentTime,
	ProductId,
	Quantity * (CASE WHEN ISNUMERIC(OptQty) = 1 THEN CAST(OptQty AS INT) ELSE 1 END ) AS Quantity
from
	base_with_optqty	
)
select * from (

SELECT 

sum(Quantity) as MIKTAR,
'Sešenekler' AS ProductGroup,
0 as TUTAR,
0 INDIRIM,
0 IKRAM

from  satislar

UNION

select 
sum(t.toplam) as MIKTAR,
t.ProductGroup,
sum(t.tutar) as TUTAR,
SUM(INDIRIM)+SUM(PAKETINDIRIM) INDIRIM,
SUM(IKRAM) IKRAM from (
SELECT 
P.Id as ProductId,
SUM(B.Quantity) AS Toplam,P.ProductGroup,SUM(ISNULL(B.Quantity,0) * ISNULL(B.Price,0)) AS Tutar,


(SELECT SUM(Bill.Price*Bill.Quantity) FROM dbo.BillWithHeader AS Bill WHERE BillState=0
and HeaderId=b.HeaderId AND ProductId=P.Id
) AS OPENTABLE,
(SELECT SUM(Bill.Quantity) FROM dbo.BillWithHeader AS Bill WHERE BillState=0
and HeaderId=b.HeaderId AND ProductId=P.Id
) AS OPENTABLEQuantity,
(SELECT SUM(PAY.Discount) FROM dbo.Payment AS PAY
where PAY.HeaderId=B.HeaderId
) as Discount,0 INDIRIM,0 PAKETINDIRIM,0 IKRAM

  FROM dbo.Bill AS B

LEFT JOIN dbo.Product as P ON P.Id=B.ProductId

WHERE
[Date]>=@par1 AND [Date]<=@par2 and P.ProductGroup Not Like '$%' 
 
GROUP BY 
P.ProductGroup,
B.HeaderId,
p.Id

UNION ALL

SELECT 
PR.Id as ProductId,0 AS Toplam,PR.ProductGroup,0 AS Tutar, 0 OPENTABLE,0 OPENTABLEQuantity, 0 Discount,
SUM(ISNULL(B.Quantity,0) * ISNULL(B.Price,0))*
(SUM(ISNULL(Discount,0))  / 
CASE WHEN (SUM(ISNULL(CashPayment,0))+SUM(ISNULL(CreditPayment,0))+SUM(ISNULL(TicketPayment,0))+SUM(ISNULL(Debit,0))+SUM(ISNULL(Discount,0)))=0 THEN 1 ELSE 
(SUM(ISNULL(CashPayment,0))+SUM(ISNULL(CreditPayment,0))+SUM(ISNULL(TicketPayment,0))+SUM(ISNULL(Debit,0))+SUM(ISNULL(Discount,0))) END) INDIRIM,0 PAKETINDIRIM,
--Case When sum((ISNULL(B.OriginalPrice,0)-ISNULL(B.Price,0))) < 0  Then 0 Else  sum((ISNULL(B.OriginalPrice,0)-ISNULL(B.Price,0))) End  IKRAM
Case When sum(ISNULL(B.Price,0)) = 0   Then sum((ISNULL(B.OriginalPrice,0))) Else 0 End  IKRAM
  FROM dbo.Payment AS P
LEFT JOIN dbo.Bill as B ON P.HeaderId=B.HeaderId
LEFT JOIN dbo.Product as PR ON PR.Id=B.ProductId
WHERE
[Date]>=@par1 AND [Date]<=@par2 and  B.ProductName Not Like '$%' 
GROUP BY 
PR.ProductGroup,
B.HeaderId,
PR.Id 

UNION ALL
SELECT 
PR.Id as ProductId,0 AS Toplam,PR.ProductGroup,0 AS Tutar, 0 OPENTABLE,0 OPENTABLEQuantity, 0 Discount, 0 INDIRIM,
SUM(b.Quantity*b.Price)*
(  avg(ISNULL(ph.Discount,0))/ (  SELECT SUM(Quantity*Price) FROM Bill where HeaderId=B.HeaderId  )  ) PAKETINDIRIM,
0 IKRAM
  FROM  dbo.PhoneOrderHeader as PH
LEFT JOIN dbo.Bill as B ON B.HeaderId=PH.HeaderId
LEFT JOIN dbo.Product as PR ON PR.Id=B.ProductId
WHERE
[Date]>=@par1 AND [Date]<=@par2 and  B.ProductName Not Like '$%' 
GROUP BY 
PR.ProductGroup,
B.HeaderId,
PR.Id 

) as t

where t.ProductGroup not like '$%'

group by
t.ProductGroup) as a
order by
a.ProductGroup

--( SELECT SUM(Quantity) AS MIKTAR,  SUM(Bill.Price*Quantity)  AS TUTAR, 
--Product.ProductGroup AS ProductGroup FROM Product INNER JOIN Bill ON Product.Id = Bill.ProductId 
--WHERE [Date]>=@par1 AND [Date]<=@par2 AND Bill.ProductName NOT LIKE '$%'
--GROUP BY Product.ProductGroup )

--WITH

--BillWithHeaderx AS (SELECT bw.*,p.ProductGroup FROM BillWithHeader bw
--left join product  p on p.Id=bw.ProductId
--  )

--select ProductGroup, SUM(Price * Quantity) as TUTAR, SUM(Quantity) as MIKTAR from BillWithHeaderx
--            where [Date]>='{TARIH1}' and [Date]<'{TARIH2}' and (BillType=0 or BillType=1)
--            GROUP BY ProductGroup

--declare @par1 nvarchar(20) = '{TARIH1}';
--declare @par2 nvarchar(20) = '{TARIH2}';
--SELECT ProductName, SUM(Quantity) AS MIKTAR, SUM(Price*Quantity)  AS TUTAR FROM Bill WHERE [Date]>=@par1
--AND [Date]<=@par2 AND ProductName NOT LIKE '$%'   GROUP BY ProductName order by  TUTAR asc 

