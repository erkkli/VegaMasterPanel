--exec subebazli '##SubeUrunler', '( SELECT ProductName, SUM(Quantity) AS MIKTAR, SUM(Price*Quantity)  AS TUTAR FROM Bill WHERE [Date]>=''{TARIH1}''
--AND [Date]<=''{TARIH2}'' AND ProductName NOT LIKE ''$%'' GROUP BY ProductName )','{SUBEADI}'; WITH vPaidBill AS( select ServerName as Sube, ProductName, MIKTAR, TUTAR from ##SubeUrunler )
--SELECT ProductName, Sube SUBE, vPaidBill.MIKTAR, vPaidBill.TUTAR from vPaidBill where Sube not in ('local') 





--declare @par1 nvarchar(20) = '{TARIH1}';
--declare @par2 nvarchar(20) = '{TARIH2}';

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
--ProductName + ' ' + '(Menü Ýçi Satýþ)' as ProductName,
--0 as TUTAR
--from  satislar
--group by satislar.ProductName

--UNION


--select 
--sum(t.toplam) as MIKTAR,
--t.ProductName,
--sum(t.tutar) as TUTAR from (
--SELECT 
--P.Id as ProductId,
--SUM(B.Quantity) AS Toplam,B.ProductName,SUM(ISNULL(B.Quantity,0) * ISNULL(B.Price,0)) AS Tutar
--  FROM dbo.Bill AS B

--LEFT JOIN dbo.Product as P ON P.Id=B.ProductId

--WHERE
--[Date]>=@par1 AND [Date]<=@par2
 

--GROUP BY B.ProductName,
--B.HeaderId,
--p.Id ) as t
--group by
--t.ProductName,
--t.ProductId ) as a
--order by
--a.MIKTAR desc



declare @par1 nvarchar(20) = '{TARIH1}';
declare @par2 nvarchar(20) = '{TARIH2}';

SELECT TOP 1 WITH TIES ProductName, SUM(Quantity) AS MIKTAR, SUM(Price*Quantity) AS TUTAR 
FROM Bill
WHERE [Date]>=@par1 AND [Date]<=@par2 AND ProductName NOT LIKE '$%' 
GROUP BY ProductName order by  TUTAR desc 











--SELECT ProductName, SUM(Quantity) AS MIKTAR, SUM(Price*Quantity)  AS TUTAR FROM Bill WHERE [Date]>='{TARIH1}'
--AND [Date]<='{TARIH2}' AND ProductName NOT LIKE '$%' GROUP BY ProductName 