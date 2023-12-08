      
declare @par1 nvarchar(20) = '{TARIH1}';
declare @par2 nvarchar(20) = '{TARIH2}';

--WITH base as (
--SELECT
--	PaymentTime,
--	Quantity,
--	LTRIM(RTRIM(split.s)) AS Opt,
--	ProductId,
--	UserName
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
--	base.ProductId,
--	base.UserName,
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
--	Opt as ProductName,UserName
--from
--	base_with_optqty	
--)
--select * from (

--SELECT 

--sum(Quantity) as MIKTAR,
--ProductName + ' ' + '(Menü Ýçi Satýþ)' as ProductName,
--0 as TUTAR,
--UserName AS PERKODU
--from  satislar
--group by satislar.ProductName,satislar.UserName

--UNION

--select 
--sum(t.toplam) as MIKTAR,
--t.ProductName,
--sum(t.tutar) as TUTAR,
--t.UserName
--from (
--SELECT 
--P.Id as ProductId,
--b.UserName,
--SUM(B.Quantity) AS Toplam,B.ProductName,SUM(ISNULL(B.Quantity,0) * ISNULL(B.Price,0)) AS Tutar
--FROM dbo.Bill AS B
--LEFT JOIN dbo.Product as P ON P.Id=B.ProductId

--WHERE
--[Date]>=@par1 AND [Date]<=@par2
 

--GROUP BY 
--B.ProductName,
--B.HeaderId,
--b.UserName,
--p.Id ) as t
--group by
--t.ProductName,
--t.UserName ,
--t.ProductId ) as a
--order by
--a.ProductName





SELECT 
UserName AS PERKODU,sum(Quantity*Price) TUTAR
FROM Bill
WHERE Date>@par1 and Date<=@par2
GROUP BY UserName
