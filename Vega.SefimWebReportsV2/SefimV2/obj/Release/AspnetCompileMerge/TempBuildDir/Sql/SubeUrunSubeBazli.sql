
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
	Quantity * (CASE WHEN ISNUMERIC(OptQty) = 1 THEN CAST(OptQty AS INT) ELSE 1 END ) AS Quantity,	
	Opt as ProductName
from
	base_with_optqty	where Opt Not Like '$%' 
)
select 

* from (

SELECT 

sum(Quantity) as MIKTAR,
ProductName + ' ' + '(Menü Ýçi Satýþ)' as ProductName,
0 as TUTAR,
0 as INDIRIM,
0 as IKRAM
from  satislar
group by satislar.ProductName

UNION


select 
sum(t.toplam) as MIKTAR,
t.ProductName,
sum(t.tutar) as TUTAR,
SUM(INDIRIM)+SUM(PAKETINDIRIM) INDIRIM,
SUM(IKRAM) IKRAM

from 

(
SELECT 
P.Id as ProductId,SUM(B.Quantity) AS Toplam,B.ProductName,SUM(ISNULL(B.Quantity,0) * ISNULL(B.Price,0)) AS Tutar,0 as INDIRIM,0 PAKETINDIRIM,0 IKRAM
  FROM dbo.Bill AS B
LEFT JOIN dbo.Product as P ON P.Id=B.ProductId
WHERE
[Date]>=@par1 AND [Date]<=@par2 and  B.ProductName Not Like '$%' 
GROUP BY B.ProductName,
B.HeaderId,
p.Id 


UNION ALL

SELECT 
PR.Id as ProductId,0 AS Toplam,B.ProductName,0 AS Tutar, 
SUM(ISNULL(B.Quantity,0) * ISNULL(B.Price,0))*
(SUM(ISNULL(Discount,0))  / 
CASE WHEN (SUM(ISNULL(CashPayment,0))+SUM(ISNULL(CreditPayment,0))+SUM(ISNULL(TicketPayment,0))+SUM(ISNULL(OnlinePayment,0))+SUM(ISNULL(Debit,0))+SUM(ISNULL(Discount,0)))=0 THEN 1 ELSE 
(SUM(ISNULL(CashPayment,0))+SUM(ISNULL(CreditPayment,0))+SUM(ISNULL(TicketPayment,0))+SUM(ISNULL(OnlinePayment,0))+SUM(ISNULL(Debit,0))+SUM(ISNULL(Discount,0))) END) INDIRIM,0 PAKETINDIRIM,
--Case When sum((ISNULL(B.OriginalPrice,0)-ISNULL(B.Price,0))) < 0   Then 0 Else  sum((ISNULL(B.OriginalPrice,0)-ISNULL(B.Price,0))) End  IKRAM
Case When sum(ISNULL(B.Price,0)) = 0   Then sum((ISNULL(B.OriginalPrice,0))) Else 0 End  IKRAM
  FROM dbo.Payment AS P
LEFT JOIN dbo.Bill as B ON P.HeaderId=B.HeaderId
LEFT JOIN dbo.Product as PR ON PR.Id=B.ProductId
WHERE
[Date]>=@par1 AND [Date]<=@par2 and  B.ProductName Not Like '$%' 
GROUP BY B.ProductName,
B.HeaderId,
PR.Id 

--UNION ALL
--SELECT 
--PR.Id as ProductId,0 AS Toplam,B.ProductName,0 AS Tutar, 0 INDIRIM,
--SUM(b.Quantity*b.Price)*
--(  SUM(ISNULL(ph.Discount,0))/ (SUM(b.Quantity*b.Price)+SUM(ISNULL(ph.Discount,0)))  ) PAKETINDIRIM,
--0 IKRAM
--  FROM  dbo.PhoneOrderHeader as PH
--LEFT JOIN dbo.Bill as B ON B.HeaderId=PH.HeaderId
--LEFT JOIN dbo.Product as PR ON PR.Id=B.ProductId
--WHERE
--[Date]>=@par1 AND [Date]<=@par2 and  B.ProductName Not Like '$%' 
--GROUP BY B.ProductName,
--B.HeaderId,
--PR.Id 

UNION ALL
SELECT 
PR.Id as ProductId,0 AS Toplam,B.ProductName,0 AS Tutar, 0 INDIRIM,
SUM(b.Quantity*b.Price)*
(  avg(ISNULL(ph.Discount,0))/ ( SELECT case when  SUM(Quantity*Price)=0 then 1 else SUM(Quantity*Price) end   FROM Bill where HeaderId=B.HeaderId )) PAKETINDIRIM,
0 IKRAM
  FROM  dbo.PhoneOrderHeader as PH
LEFT JOIN dbo.Bill as B ON B.HeaderId=PH.HeaderId
LEFT JOIN dbo.Product as PR ON PR.Id=B.ProductId
WHERE
[Date]>=@par1 AND [Date]<=@par2 and  B.ProductName Not Like '$%' 
GROUP BY B.ProductName,
B.HeaderId,
PR.Id

)   as t

group by
t.ProductName ) as a 
order by
a.ProductName