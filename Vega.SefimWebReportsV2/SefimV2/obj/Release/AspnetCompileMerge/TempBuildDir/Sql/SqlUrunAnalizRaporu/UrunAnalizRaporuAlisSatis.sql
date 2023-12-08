

		--declare @par1 nvarchar(20) = '{TARIH1}';
		--declare @par2 nvarchar(20) = '{TARIH2}';  declare @tablo table (Id bigint, BillState varchar(1),BillType varchar(1),Tarih datetime)
	
		declare @par1 nvarchar(20) = '{TARIH1}';
		declare @par2 nvarchar(20) = '{TARIH2}';
		declare @par3DEnvanter nvarchar(50)= '{DepoEnvanter}';
	    declare @par4SHrkt nvarchar(50) = '{StokHareket}';
		declare @par5Stoklar nvarchar(50) = '{Stoklar}';

;WITH

base as ( SELECT	ProductName,PaymentTime,Quantity,LTRIM(RTRIM(split.s)) AS Opt,	ProductId FROM	
----------------------------
PaidBill
----------------------------
CROSS APPLY 
-----------------------------
SplitString(',',PaidBill.Options)  as split
-----------------------------
WHERE ISNULL(Options,'')<>'' AND PaymentTime>=@par1 AND PaymentTime <= @par2	),
base_with_optqty as (
SELECT 	base.ProductName,base.PaymentTime,	base.Quantity,	ProductId,CASE  WHEN CHARINDEX('x',base.Opt) > 0 THEN
SUBSTRING(base.Opt,1,CHARINDEX('x',base.Opt)-1) ELSE '1' END AS OptQty, CASE 
WHEN CHARINDEX('x',base.Opt) > 0 THEN SUBSTRING(base.Opt,CHARINDEX('x',base.Opt)+1,100)
ELSE base.Opt END AS Opt FROM base),
satislar as  (	 SELECT	 Quantity * (CASE WHEN ISNUMERIC(OptQty) = 1 THEN CAST(OptQty AS INT) ELSE 1 END ) AS Quantity,Opt as ProductName
FROM base_with_optqty bwo  ),

--VEGA TARAFI --------------

envanter  AS (

SELECT STOKNO,ENVANTER DBS,TARIH FROM  "{DepoEnvanter}"  WHERE TARIH<@par1

),

hareketler as (SELECT 

STOKNO,IZAHAT,GIREN,BIRIMMALIYET,BIRIMFIYAT,TUTAR,TARIH

FROM   @StokHareket WHERE ISNULL(GIREN,0)>0
AND TARIH>=@par1 AND TARIH<=@par2
),

stoklar as (
SELECT IND,STOKKODU,MALINCINSI,MALIYET FROM   @Stoklar 
)

select 

SUM(DONBASISTOK) AS DONBASISTOK,
SUM(DONICIGIRENLER) AS DONICIGIRENLER,
SUM(TOPENV) AS TOPENV,
SUM(ORTMALIYET) AS ORTMALIYET,
SUM(DONICIGIRTUTAR) AS DONICIGIRTUTAR,
SUM(TOPENVTUTAR) AS TOPENVTUTAR,
SUM(Miktar) SATISMIKTAR ,
SUM(Tutar) AS SATISTUTAR,

SUM(ORTMALIYET)*SUM(Miktar) AS DONICISATMALIYET,
SUM(Tutar) - (SUM(ORTMALIYET)*SUM(Miktar)) AS DONICIBRUTKAR


from (

select 
(SELECT sum(DBS) FROM envanter where STOKNO=S.IND) DONBASISTOK,
SUM(ISNULL(GIREN,0)) DONICIGIRENLER,
(SELECT sum(DBS) FROM envanter where STOKNO=S.IND) + SUM(ISNULL(GIREN,0)) AS TOPENV,

CASE WHEN (CASE WHEN SUM(ISNULL(TUTAR,0))=0 THEN 1 ELSE SUM(ISNULL(TUTAR,0)) /  SUM(ISNULL(GIREN,0)) END) = 1  THEN AVG(ISNULL(S.MALIYET,0)) ELSE 
(CASE WHEN SUM(ISNULL(TUTAR,0))=0 THEN 1 ELSE SUM(ISNULL(TUTAR,0)) /  SUM(ISNULL(GIREN,0)) END) END AS ORTMALIYET,
SUM(ISNULL(TUTAR,0)) DONICIGIRTUTAR,

(SELECT sum(DBS) FROM envanter where STOKNO=S.IND) + SUM(ISNULL(GIREN,0)) * 
(CASE WHEN (CASE WHEN SUM(ISNULL(TUTAR,0))=0 THEN 1 ELSE SUM(ISNULL(TUTAR,0)) /  SUM(ISNULL(GIREN,0)) END) = 1  THEN AVG(ISNULL(S.MALIYET,0)) ELSE 
(CASE WHEN SUM(ISNULL(TUTAR,0))=0 THEN 1 ELSE SUM(ISNULL(TUTAR,0)) /  SUM(ISNULL(GIREN,0)) END) END ) TOPENVTUTAR,
0 as Miktar, 
0 as Tutar

from
stoklar as s 
LEFT join hareketler as h  ON s.IND=h.STOKNO
WHERE 
MALINCINSI='ET DONER.125 GR' 
GROUP BY 
S.IND

UNION ALL
SELECT
 0 DONBASISTOK,
 0 DONICIGIRENLER,
 0 TOPENV,
 0 ORTMALIYET,
 0 DONICIGIRTUTAR,
 0 TOPENVTUTAR,
 a.Miktar,
 a.Tutar
 
 from
 (
 SELECT  ProductName + ' ' + '(***Seçenek Menü Ýçeriði***)' as ProductName, 
sum(Quantity) as Miktar, 
0 as Tutar
FROM  satislar where ProductName='ET DONER.125 GR'
GROUP BY satislar.ProductName
UNION SELECT 


t.ProductName,
sum(t.toplam) as Miktar,
sum(t.tutar) as Tutar
FROM 
( SELECT 

P.Id as ProductId,
SUM(B.Quantity) AS Toplam,
B.ProductName,
SUM(ISNULL(B.Quantity,0) * ISNULL(B.Price,0)) AS Tutar

FROM Bill AS B
LEFT JOIN Product as P ON P.Id=B.ProductId
WHERE [Date]>=@par1 AND [Date]<=@par2
GROUP BY
B.ProductName,
B.HeaderId,
p.Id

) as t 

where ProductName='ET DONER.125 GR'

GROUP BY
	t.ProductName,
	t.ProductId 	
	) as a  
	) as X












--		declare @par1 nvarchar(20) = '{TARIH1}';
--		declare @par2 nvarchar(20) = '{TARIH2}';
	
--		--declare @par1 nvarchar(20) = '2020.01.20 05:00:00';
--		--declare @par2 nvarchar(20) = '2020.01.29 06:30:00';

--;WITH

--base as ( SELECT	ProductName,PaymentTime,Quantity,LTRIM(RTRIM(split.s)) AS Opt,	ProductId FROM	
------------------------------
--[yaprakdb].[dbo].PaidBill
------------------------------
--CROSS APPLY 
-------------------------------
--[yaprakdb].[dbo].SplitString(',',PaidBill.Options)  as split
-------------------------------
--WHERE ISNULL(Options,'')<>'' AND PaymentTime>=@par1 AND PaymentTime <= @par2	),
--base_with_optqty as (
--SELECT 	base.ProductName,base.PaymentTime,	base.Quantity,	ProductId,CASE  WHEN CHARINDEX('x',base.Opt) > 0 THEN
--SUBSTRING(base.Opt,1,CHARINDEX('x',base.Opt)-1) ELSE '1' END AS OptQty, CASE 
--WHEN CHARINDEX('x',base.Opt) > 0 THEN SUBSTRING(base.Opt,CHARINDEX('x',base.Opt)+1,100)
--ELSE base.Opt END AS Opt FROM base),
--satislar as  (	 SELECT	 Quantity * (CASE WHEN ISNUMERIC(OptQty) = 1 THEN CAST(OptQty AS INT) ELSE 1 END ) AS Quantity,Opt as ProductName
--FROM base_with_optqty bwo  ),

----VEGA TARAFI --------------

--envanter  AS (

--SELECT STOKNO,ENVANTER DBS,TARIH FROM [COSTDB].[dbo].F0101D0001TBLDEPOENVANTER WHERE TARIH<@par1

--),

--hareketler as (SELECT 

--STOKNO,IZAHAT,GIREN,BIRIMMALIYET,BIRIMFIYAT,TUTAR,TARIH

--FROM [COSTDB].[dbo].F0101D0001TBLSTOKHAREKETLERI WHERE ISNULL(GIREN,0)>0
--AND TARIH>=@par1 AND TARIH<=@par2

--),

--stoklar as (
--SELECT IND,STOKKODU,MALINCINSI,MALIYET FROM [COSTDB].[dbo].F0101TBLSTOKLAR 
--)




--select 

--SUM(DONBASISTOK) AS DONBASISTOK,
--SUM(DONICIGIRENLER) AS DONICIGIRENLER,
--SUM(TOPENV) AS TOPENV,
--SUM(ORTMALIYET) AS ORTMALIYET,
--SUM(DONICIGIRTUTAR) AS DONICIGIRTUTAR,
--SUM(TOPENVTUTAR) AS TOPENVTUTAR,
--SUM(Miktar) SATISMIKTAR ,
--SUM(Tutar) AS SATISTUTAR,

--SUM(ORTMALIYET)*SUM(Miktar) AS DONICISATMALIYET,
--SUM(Tutar) - (SUM(ORTMALIYET)*SUM(Miktar)) AS DONICIBRUTKAR


--from (

--select 
--(SELECT sum(DBS) FROM envanter where STOKNO=S.IND) DONBASISTOK,
--SUM(ISNULL(GIREN,0)) DONICIGIRENLER,
--(SELECT sum(DBS) FROM envanter where STOKNO=S.IND) + SUM(ISNULL(GIREN,0)) AS TOPENV,

--CASE WHEN (CASE WHEN SUM(ISNULL(TUTAR,0))=0 THEN 1 ELSE SUM(ISNULL(TUTAR,0)) /  SUM(ISNULL(GIREN,0)) END) = 1  THEN AVG(ISNULL(S.MALIYET,0)) ELSE 
--(CASE WHEN SUM(ISNULL(TUTAR,0))=0 THEN 1 ELSE SUM(ISNULL(TUTAR,0)) /  SUM(ISNULL(GIREN,0)) END) END AS ORTMALIYET,
--SUM(ISNULL(TUTAR,0)) DONICIGIRTUTAR,

--(SELECT sum(DBS) FROM envanter where STOKNO=S.IND) + SUM(ISNULL(GIREN,0)) * 
--(CASE WHEN (CASE WHEN SUM(ISNULL(TUTAR,0))=0 THEN 1 ELSE SUM(ISNULL(TUTAR,0)) /  SUM(ISNULL(GIREN,0)) END) = 1  THEN AVG(ISNULL(S.MALIYET,0)) ELSE 
--(CASE WHEN SUM(ISNULL(TUTAR,0))=0 THEN 1 ELSE SUM(ISNULL(TUTAR,0)) /  SUM(ISNULL(GIREN,0)) END) END ) TOPENVTUTAR,
--0 as Miktar, 
--0 as Tutar

--from
--stoklar as s 
--LEFT join hareketler as h  ON s.IND=h.STOKNO
--WHERE 
--MALINCINSI='ET DONER.125 GR' 
--GROUP BY 
--S.IND

--UNION ALL
--SELECT
-- 0 DONBASISTOK,
-- 0 DONICIGIRENLER,
-- 0 TOPENV,
-- 0 ORTMALIYET,
-- 0 DONICIGIRTUTAR,
-- 0 TOPENVTUTAR,
-- a.Miktar,
-- a.Tutar
 
-- from
-- (



-- SELECT  ProductName + ' ' + '(***Seçenek Menü Ýçeriði***)' as ProductName, 
--sum(Quantity) as Miktar, 
--0 as Tutar
--FROM  satislar where ProductName='ET DONER.125 GR'
--GROUP BY satislar.ProductName
--UNION SELECT 


--t.ProductName,
--sum(t.toplam) as Miktar,
--sum(t.tutar) as Tutar
--FROM 
--( SELECT 

--P.Id as ProductId,
--SUM(B.Quantity) AS Toplam,
--B.ProductName,
--SUM(ISNULL(B.Quantity,0) * ISNULL(B.Price,0)) AS Tutar

--FROM [yaprakdb].[dbo].Bill AS B
--LEFT JOIN [yaprakdb].[dbo].Product as P ON P.Id=B.ProductId
--WHERE [Date]>=@par1 AND [Date]<=@par2
--GROUP BY
--B.ProductName,
--B.HeaderId,
--p.Id

--) as t 

--where ProductName='ET DONER.125 GR'

--GROUP BY
--	t.ProductName,
--	t.ProductId 	
--	) as a  
--	) as X