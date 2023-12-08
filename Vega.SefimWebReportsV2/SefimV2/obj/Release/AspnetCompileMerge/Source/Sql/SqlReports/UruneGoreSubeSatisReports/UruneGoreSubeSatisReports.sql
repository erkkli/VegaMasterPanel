


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
	base_with_optqty	
)
select * from (

SELECT 

sum(Quantity) as MIKTAR,
ProductName,
'Seçenekler' AS ProductGroup,
0 as TUTAR
from  satislar 
group by satislar.ProductName


UNION


select 
sum(t.toplam) as MIKTAR,
t.ProductName,
t.ProductGroup,
sum(t.tutar) as TUTAR from (
SELECT 
P.Id as ProductId,
SUM(B.Quantity) AS Toplam,B.ProductName,P.ProductGroup,SUM(ISNULL(B.Quantity,0) * ISNULL(B.Price,0)) AS Tutar,

(SELECT SUM(Bill.Price*Bill.Quantity) FROM dbo.BillWithHeader AS Bill WHERE BillState=0
and HeaderId=b.HeaderId AND ProductId=P.Id
) AS OPENTABLE,
(SELECT SUM(Bill.Quantity) FROM dbo.BillWithHeader AS Bill WHERE BillState=0
and HeaderId=b.HeaderId AND ProductId=P.Id
) AS OPENTABLEQuantity,
(SELECT SUM(PAY.Discount) FROM dbo.Payment AS PAY
where PAY.HeaderId=B.HeaderId
) as Discount
  FROM dbo.Bill AS B

LEFT JOIN dbo.Product as P ON P.Id=B.ProductId

WHERE
[Date]>=@par1 AND [Date]<=@par2   AND B.ProductName Like '{ProductName}'
 


GROUP BY B.ProductName,
P.ProductGroup,
B.HeaderId,
p.Id ) as t

WHERE t.ProductName  Like '{ProductName}'
group by
t.ProductName,
t.ProductGroup
) as a
where a.ProductName  Like '{ProductName}'
order by
a.ProductGroup




--WITH
--BillWithHeaderx AS (SELECT bw.*,p.ProductGroup FROM BillWithHeader bw
--left join product  p on p.Id=bw.ProductId
--where p.ProductGroup ='Seçenekler'
--  )

--select ProductGroup,ProductName, SUM(Price * Quantity) as TUTAR, SUM(Quantity) as MIKTAR from BillWithHeaderx
--            where [Date]>='2020.01.01 05:00:00' and [Date]< '2020.01.31 06:30:00' and (BillType=0 or BillType=1)
--            GROUP BY ProductGroup,ProductName








