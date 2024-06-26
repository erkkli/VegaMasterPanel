﻿
declare @par1 nvarchar(20) = '{TARIH1}';
declare @par2 nvarchar(20) = '{TARIH2}';

WITH base as (
SELECT
	PaymentTime,
	Quantity,
	LTRIM(RTRIM(split.s)) AS Opt,
	ProductId,Date Tarih
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
	Tarih,
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
	Opt as ProductName,Tarih
from
	base_with_optqty	where Opt Not Like '$%' 
)
select 

 sum(a.MIKTAR) as MIKTAR,
 sum(a.TUTAR) as TUTAR,
 a.Tarih

from (

SELECT 

sum(Quantity) as MIKTAR,
ProductName + ' ' + '(Menü İçi Satış)' as ProductName,
0 as TUTAR,convert(varchar,Tarih, 104) Tarih

From  satislar
Where Tarih>=@par1 AND Tarih<=@par2
Group By satislar.ProductName,Tarih

UNION

Select 
 sum(t.toplam) as MIKTAR,
 t.ProductName,
 sum(t.tutar) as TUTAR
,convert(varchar,Tarih, 104) Tarih from 

(
SELECT 
P.Id as ProductId,SUM(B.Quantity) AS Toplam,B.ProductName,SUM(ISNULL(B.Quantity,0) * ISNULL(B.Price,0)) AS Tutar,[Date] Tarih
  FROM dbo.Bill AS B
LEFT JOIN dbo.Product as P ON P.Id=B.ProductId
 WHERE
[Date]>=@par1 AND [Date]<=@par2 and  B.ProductName Not Like '$%' 
 GROUP BY
 B.ProductName,
 B.HeaderId,
 p.Id ,[Date]
 )  as t

 Group By
 t.ProductName ,
 t.ProductId,
 convert(varchar,Tarih, 104)
 ) As a 

Group By
a.Tarih

Order By
a.Tarih