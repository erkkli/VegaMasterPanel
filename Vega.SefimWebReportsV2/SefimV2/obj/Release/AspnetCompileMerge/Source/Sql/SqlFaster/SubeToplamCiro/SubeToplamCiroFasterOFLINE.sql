DECLARE @Sube nvarchar(100) = '{SUBEADI}';
DECLARE @Trh1 nvarchar(20) = '{TARIH1}';
DECLARE @Trh2 nvarchar(20) = '{TARIH2}';
DECLARE @FirmaInd nvarchar(100) = '{FIRMAIND}';
DECLARE @SUBEADI nvarchar(20) = '{SUBEADITBL}';
DECLARE @KASAADI nvarchar(20) = '{KASAADI}';

WITH Toplamsatis AS
  (SELECT @Sube AS Sube,

   (SELECT TOP 1 SUBEADI
      FROM TBLFASTERKASALAR
      WHERE SUBENO=TBLFASTERODEMELER.SUBEIND) AS Sube1,

     (SELECT KASAADI
      FROM TBLFASTERKASALAR
      WHERE KASANO=TBLFASTERODEMELER.KASAIND) AS Kasa,
          ODENEN AS cash,
          0 AS Credit,
          0 AS Ticket,
          0 AS Debit,
          0 AS ikram,
          0 AS TableNo,
          0 AS Discount,
          0 AS iptal,
		  0 AS iptal2,
          0 AS zayi
   FROM DBO.TBLFASTERODEMELER
   WHERE ODEMETIPI = 0
     AND ISNULL(IADE, 0) = 0
     AND ISLEMTARIHI >= @Trh1
     AND ISLEMTARIHI <= @Trh2
   
   UNION ALL SELECT @Sube AS Sube,

    (SELECT TOP 1 SUBEADI
      FROM TBLFASTERKASALAR
      WHERE SUBENO=TBLFASTERODEMELER.SUBEIND) AS Sube1,

     (SELECT KASAADI
      FROM TBLFASTERKASALAR
      WHERE KASANO=TBLFASTERODEMELER.KASAIND) AS Kasa,
                    ODENEN*-1 AS cash,
                    0 AS Credit,
                    0 AS Ticket,
                    0 AS Debit,
                    0 AS ikram,
                    0 AS TableNo,
                    0 AS Discount,
                    0 AS iptal,
					0 AS iptal2,
                    0 AS zayi
   FROM DBO.TBLFASTERODEMELER
   WHERE ODEMETIPI=0
     AND ISNULL(IADE, 0)=1
     AND ISLEMTARIHI >= @Trh1
     AND ISLEMTARIHI <= @Trh2
   


   UNION ALL SELECT @Sube AS Sube,

   (SELECT TOP 1 SUBEADI
      FROM TBLFASTERKASALAR
      WHERE SUBENO=TBLFASTERODEMELER.SUBEIND) AS Sube1,

     (SELECT KASAADI
      FROM TBLFASTERKASALAR
      WHERE KASANO=TBLFASTERODEMELER.KASAIND) AS Kasa,
                    0 AS cash,
                    ODENEN AS Credit,
                    0 AS Ticket,
                    0 AS Debit,
                    0 AS ikram,
                    0 AS TableNo,
                    0 AS Discount,
                    0 AS iptal,
					0 AS iptal2,
                    0 AS zayi
   FROM DBO.TBLFASTERODEMELER
   WHERE ODEMETIPI = 1
     AND ISNULL(IADE, 0)= 0
     AND ISLEMTARIHI >= @Trh1
     AND ISLEMTARIHI <= @Trh2
  
   UNION ALL SELECT @Sube AS Sube,

   (SELECT TOP 1 SUBEADI
      FROM TBLFASTERKASALAR
      WHERE SUBENO=TBLFASTERODEMELER.SUBEIND) AS Sube1,

     (SELECT KASAADI
      FROM TBLFASTERKASALAR
      WHERE KASANO=TBLFASTERODEMELER.KASAIND) AS Kasa,
                    0 AS cash,
                    ODENEN*-1 AS Credit,
                    0 AS Ticket,
                    0 AS Debit,
                    0 AS ikram,
                    0 AS TableNo,
                    0 AS Discount,
                    0 AS iptal,
					0 AS iptal2,
                    0 AS zayi
   FROM DBO.TBLFASTERODEMELER
   WHERE ODEMETIPI=1
     AND ISNULL(IADE, 0)=1
     AND ISLEMTARIHI >= @Trh1
     AND ISLEMTARIHI <= @Trh2
    
   UNION ALL SELECT @Sube AS Sube,

   (SELECT TOP 1 SUBEADI
      FROM TBLFASTERKASALAR
      WHERE SUBENO=TBLFASTERODEMELER.SUBEIND) AS Sube1,

     (SELECT KASAADI
      FROM TBLFASTERKASALAR
      WHERE KASANO=TBLFASTERODEMELER.KASAIND) AS Kasa,
                    0 AS cash,
                    0 AS Credit,
                    ODENEN AS Ticket,
                    0 AS Debit,
                    0 AS ikram,
                    0 AS TableNo,
                    0 AS Discount,
                    0 AS iptal,
					0 AS iptal2,
                    0 AS zayi
   FROM DBO.TBLFASTERODEMELER
   WHERE ODEMETIPI = 2
     AND ISNULL(IADE, 0)= 0
     AND ISLEMTARIHI >= @Trh1
     AND ISLEMTARIHI <= @Trh2
    
   UNION ALL SELECT @Sube AS Sube,

   (SELECT TOP 1 SUBEADI
      FROM TBLFASTERKASALAR
      WHERE SUBENO=TBLFASTERODEMELER.SUBEIND) AS Sube1,

     (SELECT KASAADI
      FROM TBLFASTERKASALAR
      WHERE KASANO=TBLFASTERODEMELER.KASAIND) AS Kasa,
                    0 AS cash,
                    0 AS Credit,
                    ODENEN*-1 AS Ticket,
                    0 AS Debit,
                    0 AS ikram,
                    0 AS TableNo,
                    0 AS Discount,
                    0 AS iptal,
					0 AS iptal2,
                    0 AS zayi
   FROM DBO.TBLFASTERODEMELER
   WHERE ODEMETIPI=2
     AND ISNULL(IADE, 0)=1
     AND ISLEMTARIHI >= @Trh1
     AND ISLEMTARIHI <= @Trh2
    
   UNION ALL SELECT @Sube AS Sube,

   (SELECT TOP 1 SUBEADI
      FROM TBLFASTERKASALAR
      WHERE SUBENO=TBLFASTERODEMELER.SUBEIND) AS Sube1,

     (SELECT KASAADI
      FROM TBLFASTERKASALAR
      WHERE KASANO=TBLFASTERODEMELER.KASAIND) AS Kasa,
                    0 AS cash,
                    0 AS Credit,
                    0 AS Ticket,
                    0 AS Debit,
                    ODENEN AS ikram,
                    0 AS TableNo,
                    0 AS Discount,
                    0 AS iptal,
					0 AS iptal2,
                    0 AS zayi
   FROM DBO.TBLFASTERODEMELER
   WHERE ODEMETIPI = 3
     AND ISNULL(IADE, 0)= 0
     AND ISLEMTARIHI >= @Trh1
     AND ISLEMTARIHI <= @Trh2
    
   UNION ALL SELECT @Sube AS Sube,

   (SELECT TOP 1 SUBEADI
      FROM TBLFASTERKASALAR
      WHERE SUBENO=TBLFASTERODEMELER.SUBEIND) AS Sube1,

     (SELECT KASAADI
      FROM TBLFASTERKASALAR
      WHERE KASANO=TBLFASTERODEMELER.KASAIND) AS Kasa,
                    0 AS cash,
                    0 AS Credit,
                    0 AS Ticket,
                    ODENEN AS Debit,
                    0 AS ikram,
                    0 AS TableNo,
                    0 AS Discount,
                    0 AS iptal,
					0 AS iptal2,
                    0 AS zayi
   FROM DBO.TBLFASTERODEMELER
   WHERE ODEMETIPI = 4
     AND ISNULL(IADE, 0)= 0
     AND ISLEMTARIHI >= @Trh1
     AND ISLEMTARIHI <= @Trh2
   
   UNION ALL SELECT @Sube AS Sube,

   (SELECT TOP 1 SUBEADI
      FROM TBLFASTERKASALAR
      WHERE SUBENO=TBLFASTERODEMELER.SUBEIND) AS Sube1,

     (SELECT KASAADI
      FROM TBLFASTERKASALAR
      WHERE KASANO=TBLFASTERODEMELER.KASAIND) AS Kasa,
                    0 AS cash,
                    0 AS Credit,
                    0 AS Ticket,
                    ODENEN*-1 AS Debit,
                    0 AS ikram,
                    0 AS TableNo,
                    0 AS Discount,
                    0 AS iptal,
					0 AS iptal2,
                    0 AS zayi
   FROM DBO.TBLFASTERODEMELER
   WHERE ODEMETIPI = 4
     AND ISNULL(IADE, 0)= 1
     AND ISLEMTARIHI >= @Trh1
     AND ISLEMTARIHI <= @Trh2
    

   UNION ALL SELECT @Sube AS Sube,

   (SELECT TOP 1 SUBEADI
      FROM TBLFASTERKASALAR
      WHERE SUBENO=TBLFASTERSATISBASLIK.SUBEIND) AS Sube1,

     (SELECT KASAADI
      FROM TBLFASTERKASALAR
      WHERE KASANO=TBLFASTERSATISBASLIK.KASAIND) AS Kasa,
                    0 AS cash,
                    0 AS Credit,
                    0 AS Ticket,
                    0 AS Debit,
                    0 AS ikram,
                    0 AS TableNo,
                    SATIRISK+ALTISK AS Discount,
                    0 AS iptal,
					0 AS iptal2,
                    0 AS zayi
   FROM DBO.TBLFASTERSATISBASLIK
   WHERE ISNULL(IADE, 0) = 0
     AND ISLEMTARIHI >= @Trh1
     AND ISLEMTARIHI <= @Trh2
  
   UNION ALL SELECT @Sube AS Sube,

   (SELECT TOP 1 SUBEADI
      FROM TBLFASTERKASALAR
      WHERE SUBENO=TBLFASTERODEMELER.SUBEIND) AS Sube1,

     (SELECT KASAADI
      FROM TBLFASTERKASALAR
      WHERE KASANO=TBLFASTERODEMELER.KASAIND) AS Kasa,
                    0 AS cash,
                    0 AS Credit,
                    0 AS Ticket,
                    0 AS Debit,
                    0 AS ikram,
                    0 AS TableNo,
                    0 AS Discount,
					0 AS iptal,
                    ODENEN AS iptal2,
                    0 AS zayi
   FROM DBO.TBLFASTERODEMELER
   WHERE ISNULL(IADE, 0) = 1
     AND ODEMETIPI NOT IN (0,
                           1,
                           2,
                           4)
     AND ISLEMTARIHI >= @Trh1
     AND ISLEMTARIHI <= @Trh2


	    UNION ALL SELECT @Sube AS Sube,

   (SELECT TOP 1 SUBEADI
      FROM TBLFASTERKASALAR
      WHERE SUBENO=TBLFASTERODEMELER.SUBEIND) AS Sube1,

     (SELECT KASAADI
      FROM TBLFASTERKASALAR
      WHERE KASANO=TBLFASTERODEMELER.KASAIND) AS Kasa,
                    0 AS cash,
                    0 AS Credit,
                    0 AS Ticket,
                    0 AS Debit,
                    0 AS ikram,
                    0 AS TableNo,
                    0 AS Discount,
                    ODENEN AS iptal,
					0 AS iptal2,
                    0 AS zayi
   FROM DBO.TBLFASTERODEMELER
   WHERE ISNULL(IADE, 0) = 1
     AND ODEMETIPI  IN (0,
                           1,
                           2,
                           4)
     AND ISLEMTARIHI >= @Trh1
     AND ISLEMTARIHI <= @Trh2
  

   UNION ALL SELECT @Sube AS Sube,

     (SELECT TOP 1 SUBEADI
      FROM TBLFASTERKASALAR
      WHERE SUBENO=TBLFASTERSATISBASLIK.SUBEIND) AS Sube1,

     (SELECT KASAADI
      FROM TBLFASTERKASALAR
      WHERE KASANO=TBLFASTERSATISBASLIK.KASAIND) AS Kasa,
                    0 AS cash,
                    0 AS Credit,
                    0 AS Ticket,
                    0 AS Debit,
                    0 AS ikram,
                    COUNT(*) AS TableNo,
                    0 AS Discount,
                    0 AS iptal,
					0 AS iptal2,
                    0 AS zayi
   FROM DBO.TBLFASTERSATISBASLIK
   WHERE ISNULL(IADE, 0) = 0
     AND ISLEMTARIHI >= @Trh1
     AND ISLEMTARIHI <= @Trh2
 
   GROUP BY SUBEIND,
            KASAIND)
SELECT Sube,
       Sube1,
       Kasa,
       SUM(Cash) AS Cash,
       SUM(Credit) AS Credit,
       Sum(Ticket) AS Ticket,
       Sum(Debit) AS Debit,
       Sum(ikram) AS ikram,
       Sum(TableNo) AS TableNo,
       Sum(Discount) AS Discount,
       Sum(iptal) AS iptal,
       Sum(Zayi) AS Zayi,
       SUM(Cash + Credit + Ticket + Debit) -SUM(iptal2) AS ToplamCiro,
       0 AS Saniye,
       '' AS RowStyle,
       '' AS RowError
FROM toplamsatis
GROUP BY Sube,
         Sube1,
         Kasa





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