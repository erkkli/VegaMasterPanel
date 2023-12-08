
declare @Sube nvarchar(100) = '{SUBEADI}';
declare @Trh1 nvarchar(20) = '{TARIH1}';
declare @Trh2 nvarchar(20) = '{TARIH2}';
declare @SUBEADI nvarchar(20) = '{SUBEADITBL}';
declare @KASAADI nvarchar(20) = '{KASAADI}';

declare @sqlCommand nvarchar(max);
--DECLARE @sqlCommand varchar(1000)
--DECLARE @columnList varchar(75)
--DECLARE @city varchar(75)

SET @SUBEADI = '{SUBEADITBL}'
SET @KASAADI = '{KASAADI}'

SET @Trh1 = '{TARIH1}'
SET @Trh2 = '{TARIH2}'


--SET @sqlCommand = 'SELECT ' + @columnList + ' FROM Person.Address WHERE City = ' + @city

--EXEC (@sqlCommand)
SET @sqlCommand =
'
WITH Toplamsatis 
AS ( 

SELECT
'+@Sube+' as Sube,
(SELECT SUBEADI FROM '+@SUBEADI+' WHERE IND=TBLFASTERODEMELER.SUBEIND) AS Sube1,
(SELECT KASAADI FROM '+@KASAADI+' WHERE IND=TBLFASTERODEMELER.KASAIND) AS Kasa,
 ODENEN AS cash, 0 AS Credit, 0 AS Ticket, 0 AS Debit, 0 AS ikram, 0 AS TableNo, 0 AS Discount, 0 AS iptal,
0 AS zayi  FROM DBO.TBLFASTERODEMELER
WHERE ODEMETIPI=0 AND ISNULL(IADE,0)=0 AND 
TARIH >= @Trh1 AND TARIH <= @Trh2 



UNION ALL

SELECT
'+@Sube+' as Sube,
(SELECT SUBEADI FROM '+@SUBEADI+' WHERE IND=TBLFASTERODEMELER.SUBEIND) AS Sube1,
(SELECT KASAADI FROM  '+@KASAADI+' WHERE IND=TBLFASTERODEMELER.KASAIND) AS Kasa,
 0 AS cash, ODENEN AS Credit, 0 AS Ticket, 0 AS Debit, 0 AS ikram, 0 AS TableNo, 0 AS Discount, 0 AS iptal,
0 AS zayi  FROM DBO.TBLFASTERODEMELER
WHERE ODEMETIPI=1 AND ISNULL(IADE,0)=0 AND 
TARIH >= @Trh1 AND TARIH <= @Trh2


UNION ALL

SELECT
'+@Sube+' as Sube,
(SELECT SUBEADI FROM '+@SUBEADI+' WHERE IND=TBLFASTERODEMELER.SUBEIND) AS Sube1,
(SELECT KASAADI FROM  '+@KASAADI+' WHERE IND=TBLFASTERODEMELER.KASAIND) AS Kasa,
 0 AS cash, 0 AS Credit,ODENEN AS Ticket, 0 AS Debit, 0 AS ikram, 0 AS TableNo, 0 AS Discount, 0 AS iptal,
0 AS zayi  FROM DBO.TBLFASTERODEMELER
WHERE ODEMETIPI=2 AND ISNULL(IADE,0)=0 AND 
TARIH >= @Trh1 AND TARIH <= @Trh2


UNION ALL

SELECT
'+@Sube+' as Sube,
(SELECT SUBEADI FROM '+@SUBEADI+' WHERE SUBEIND=TBLFASTERODEMELER.SUBEIND) AS Sube1,
(SELECT KASAADI FROM  '+@KASAADI+' WHERE IND=TBLFASTERODEMELER.KASAIND) AS Kasa,
 0 AS cash, 0 AS Credit,0 AS Ticket, 0 AS Debit,ODENEN AS ikram, 0 AS TableNo, 0 AS Discount, 0 AS iptal,
0 AS zayi  FROM DBO.TBLFASTERODEMELER
WHERE ODEMETIPI=3 AND ISNULL(IADE,0)=0 AND 
TARIH >= @Trh1 AND TARIH <= @Trh2


UNION ALL

SELECT
'+@Sube+' as Sube,
(SELECT SUBEADI FROM '+@SUBEADI+' WHERE IND=TBLFASTERODEMELER.SUBEIND) AS Sube1,
(SELECT KASAADI FROM  '+@KASAADI+' WHERE IND=TBLFASTERODEMELER.KASAIND) AS Kasa,
 0 AS cash, 0 AS Credit,0 AS Ticket, ODENEN AS Debit,0 AS ikram, 0 AS TableNo, 0 AS Discount, 0 AS iptal,
0 AS zayi  FROM DBO.TBLFASTERODEMELER
WHERE ODEMETIPI=4 AND ISNULL(IADE,0)=0 AND 
TARIH >= @Trh1 AND TARIH <= @Trh2

UNION ALL

SELECT
'+@Sube+' as Sube,
(SELECT SUBEADI FROM '+@SUBEADI+' WHERE IND=TBLFASTERSATISBASLIK.SUBEIND) AS Sube1,
(SELECT KASAADI FROM  '+@KASAADI+' WHERE IND=TBLFASTERSATISBASLIK.KASAIND) AS Kasa,
 0 AS cash, 0 AS Credit,0 AS Ticket, 0 AS Debit,0 AS ikram, 0 AS TableNo, SATIRISK+ALTISK AS Discount, 0 AS iptal,
0 AS zayi  FROM DBO.TBLFASTERSATISBASLIK
WHERE ISNULL(IADE,0)=0 AND 
TARIH >= @Trh1 AND TARIH <= @Trh2


UNION ALL

SELECT
'+@Sube+' as Sube,
(SELECT SUBEADI FROM '+@SUBEADI+' WHERE IND=TBLFASTERODEMELER.SUBEIND) AS Sube1,
(SELECT KASAADI FROM  '+@KASAADI+' WHERE IND=TBLFASTERODEMELER.KASAIND) AS Kasa,
 0 AS cash, 0 AS Credit,0 AS Ticket, 0 AS Debit,0 AS ikram, 0 AS TableNo, 0 AS Discount, ODENEN AS iptal,
0 AS zayi  FROM DBO.TBLFASTERODEMELER
WHERE ISNULL(IADE,0)=1 AND
TARIH >= @Trh1 AND TARIH <= @Trh2


UNION ALL
SELECT
'+@Sube+' as Sube,
(SELECT SUBEADI FROM '+@SUBEADI+' WHERE IND=TBLFASTERSATISBASLIK.SUBEIND) AS Sube1,
(SELECT KASAADI FROM  '+@KASAADI+' WHERE IND=TBLFASTERSATISBASLIK.KASAIND) AS Kasa,
 0 AS cash, 0 AS Credit,0 AS Ticket, 0 AS Debit,0 AS ikram, COUNT(*) AS TableNo, 0 AS Discount, 0 AS iptal,
0 AS zayi  FROM DBO.TBLFASTERSATISBASLIK
WHERE ISNULL(IADE,0)=0 AND
TARIH >= @Trh1 AND TARIH <= @Trh2
GROUP BY 
SUBEIND ,
KASAIND 



) 
SELECT Sube,Sube1,Kasa
,SUM(Cash) AS Cash 
,SUM(Credit) AS Credit 
,Sum(Ticket) AS Ticket 
,Sum(Debit) AS Debit 
,Sum(ikram) AS ikram 
,Sum(TableNo) AS TableNo 
,Sum(Discount) AS Discount 
,Sum(iptal) AS iptal 
,Sum(Zayi) AS Zayi
,SUM(Cash+Credit+Ticket+Debit) AS ToplamCiro
,0 AS Saniye
,'' AS RowStyle
,'' AS RowError
FROM toplamsatis  
GROUP BY Sube,Sube1,Kasa

'
EXEC (@sqlCommand)


--declare @Sube nvarchar(100) = '{SUBEADI}';
--declare @Trh1 nvarchar(20) = '{TARIH1}';
--declare @Trh2 nvarchar(20) = '{TARIH2}';

--WITH Toplamsatis 
--AS ( 

--SELECT
--@Sube as Sube,

-- SUM(ODENEN) AS cash, 0 AS Credit, 0 AS Ticket, 0 AS Debit, 0 AS ikram, 0 AS TableNo, 0 AS Discount, 0 AS iptal,
--0 AS zayi  FROM DBO.TBLFASTERODEMELER
--WHERE ODEMETIPI=0 AND
--ISLEMTARIHI >= @Trh1 AND ISLEMTARIHI <= @Trh2

--UNION ALL

--SELECT
--@Sube as Sube,

-- 0 AS cash, SUM(ODENEN) AS Credit, 0 AS Ticket, 0 AS Debit, 0 AS ikram, 0 AS TableNo, 0 AS Discount, 0 AS iptal,
--0 AS zayi  FROM DBO.TBLFASTERODEMELER
--WHERE ODEMETIPI=1 AND
--ISLEMTARIHI >= @Trh1 AND ISLEMTARIHI <= @Trh2

--UNION ALL

--SELECT
--@Sube as Sube,

-- 0 AS cash, 0 AS Credit,SUM(ODENEN) AS Ticket, 0 AS Debit, 0 AS ikram, 0 AS TableNo, 0 AS Discount, 0 AS iptal,
--0 AS zayi  FROM DBO.TBLFASTERODEMELER
--WHERE ODEMETIPI=2 AND
--ISLEMTARIHI >= @Trh1 AND ISLEMTARIHI <= @Trh2

--UNION ALL

--SELECT
--@Sube as Sube,

-- 0 AS cash, 0 AS Credit,0 AS Ticket, 0 AS Debit,SUM(ODENEN) AS ikram, 0 AS TableNo, 0 AS Discount, 0 AS iptal,
--0 AS zayi  FROM DBO.TBLFASTERODEMELER
--WHERE ODEMETIPI=3 AND
--ISLEMTARIHI >= @Trh1 AND ISLEMTARIHI <= @Trh2

--UNION ALL

--SELECT
--@Sube as Sube,

-- 0 AS cash, 0 AS Credit,0 AS Ticket, SUM(ODENEN) AS Debit,0 AS ikram, 0 AS TableNo, 0 AS Discount, 0 AS iptal,
--0 AS zayi  FROM DBO.TBLFASTERODEMELER
--WHERE ODEMETIPI=4 AND
--ISLEMTARIHI >= @Trh1 AND ISLEMTARIHI <= @Trh2

--UNION ALL

--SELECT
--@Sube as Sube,

-- 0 AS cash, 0 AS Credit,0 AS Ticket, 0 AS Debit,0 AS ikram, 0 AS TableNo, SUM(SATIRISK)+SUM(ALTISK) AS Discount, 0 AS iptal,
--0 AS zayi  FROM DBO.TBLFASTERSATISBASLIK
--WHERE 
--ISLEMTARIHI >= @Trh1 AND ISLEMTARIHI <= @Trh2

--UNION ALL

--SELECT
--@Sube as Sube,

-- 0 AS cash, 0 AS Credit,0 AS Ticket, 0 AS Debit,0 AS ikram, 0 AS TableNo, 0 AS Discount, SUM(ODENEN) AS iptal,
--0 AS zayi  FROM DBO.TBLFASTERODEMELER
--WHERE ISNULL(IADE,0)=1 AND
--ISLEMTARIHI >= @Trh1 AND ISLEMTARIHI <= @Trh2

--UNION ALL
--SELECT
--@Sube as Sube,

-- 0 AS cash, 0 AS Credit,0 AS Ticket, 0 AS Debit,0 AS ikram, COUNT(*) AS TableNo, 0 AS Discount, 0 AS iptal,
--0 AS zayi  FROM DBO.TBLFASTERSATISBASLIK
--WHERE ISNULL(IADE,0)=0 AND
--ISLEMTARIHI >= @Trh1 AND ISLEMTARIHI <= @Trh2


--) 
--SELECT Sube 
--,SUM(Cash) AS Cash 
--,SUM(Credit) AS Credit 
--,Sum(Ticket) AS Ticket 
--,Sum(Debit) AS Debit 
--,Sum(ikram) AS ikram 
--,Sum(TableNo) AS TableNo 
--,Sum(Discount) AS Discount 
--,Sum(iptal) AS iptal 
--,Sum(Zayi) AS Zayi
--,SUM(Cash+Credit+Ticket+Debit) AS ToplamCiro
--,0 AS Saniye
--,'' AS RowStyle
--,'' AS RowError
--FROM toplamsatis  
--GROUP BY Sube