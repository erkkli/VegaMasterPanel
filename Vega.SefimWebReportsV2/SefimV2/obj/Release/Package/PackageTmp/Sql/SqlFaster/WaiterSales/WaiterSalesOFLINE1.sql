﻿DECLARE @Sube nvarchar(100) = '{SUBE}';
DECLARE @par1 nvarchar(20) = '{TARIH1}';
DECLARE @par2 nvarchar(20) = '{TARIH2}';
DECLARE @par3 nvarchar(100) = '{Personel}';


SELECT 
T.Sube1,
T.Kasa,
T.ID,

SUM(T.MIKTAR) MIKTAR,
SUM(T.TUTAR) TUTAR,
T.ProductName,
T.PERKODU
FROM (

  (SELECT
      (SELECT TOP 1 SUBEADI
      FROM TBLFASTERKASALAR
      WHERE SUBENO=FSB.SUBEIND) AS Sube1,

     (SELECT KASAADI
      FROM TBLFASTERKASALAR
      WHERE KASANO=FSB.KASAIND) AS Kasa,

     (SELECT IND
      FROM TBLFASTERKASALAR
      WHERE KASANO=FSB.KASAIND) AS ID,
          SUM(FSH.MIKTAR) AS MIKTAR,
          SUM((((MIKTAR*SATISFIYATI) * (100-ISNULL(FSH.ISK1, 0))/100) * (100-ISNULL(FSH.ISK2, 0))/100) * (100-ISNULL(FSB.ALTISKORAN, 0))/100) AS TUTAR,
          STK.MALINCINSI AS ProductName,
		  FSH.PERKODU
   FROM TBLFASTERSATISHAREKET AS FSH
   LEFT JOIN TBLFASTERSATISBASLIK AS FSB ON FSH.BASLIKIND=FSB.BASLIKIND
   AND FSH.SUBEIND=FSB.SUBEIND
   AND FSH.KASAIND=FSB.KASAIND
   LEFT JOIN TBLFASTERSTOKLAR AS STK ON FSH.STOKIND=STK.STOKNO
   WHERE FSH.ISLEMTARIHI>=@par1
     AND FSH.ISLEMTARIHI<=@par2
     AND ISNULL(FSB.IADE, 0)=0
   GROUP BY FSB.SUBEIND,
            FSB.KASAIND,
            STK.MALINCINSI,
			FSH.PERKODU
			
			UNION ALL SELECT
    (SELECT TOP 1 SUBEADI
      FROM TBLFASTERKASALAR
      WHERE SUBENO=FSB.SUBEIND) AS Sube1,

     (SELECT KASAADI
      FROM TBLFASTERKASALAR
      WHERE KASANO=FSB.KASAIND) AS Kasa,

     (SELECT IND
      FROM TBLFASTERKASALAR
      WHERE KASANO=FSB.KASAIND) AS ID,
          SUM(FSH.MIKTAR)*-1.00 AS MIKTAR,
          SUM((((MIKTAR*SATISFIYATI) * (100-ISNULL(FSH.ISK1, 0))/100) * (100-ISNULL(FSH.ISK2, 0))/100) * (100-ISNULL(FSB.ALTISKORAN, 0))/100) *-1.00 AS TUTAR,
          STK.MALINCINSI AS ProductName,
		  FSH.PERKODU
   FROM TBLFASTERSATISHAREKET AS FSH
   LEFT JOIN TBLFASTERSATISBASLIK AS FSB ON FSH.BASLIKIND=FSB.BASLIKIND
   AND FSH.SUBEIND=FSB.SUBEIND
   AND FSH.KASAIND=FSB.KASAIND
   LEFT JOIN TBLFASTERSTOKLAR AS STK ON FSH.STOKIND=STK.STOKNO
   WHERE FSH.ISLEMTARIHI>=@par1
     AND FSH.ISLEMTARIHI<=@par2
     AND ISNULL(FSB.IADE, 0)=1
   GROUP BY FSB.SUBEIND,
            FSB.KASAIND,
            STK.MALINCINSI,
			FSH.PERKODU
			
			) ) T
			GROUP BY 
			T.Sube1,
T.Kasa,
T.ID,
T.ProductName,
T.PERKODU


--SELECT SUM(T.SAY) ISLEMSAYISI,
--       SUM(T.MIKTAR) MIKTAR,
--       SUM(T.TUTAR) TUTAR,
--       T.PERSONEL
--FROM(
--       (SELECT
--          (SELECT count(BASLIKIND)
--           FROM TBLFASTERSATISBASLIK
--           WHERE BASLIKIND = FSH.BASLIKIND) SAY,
--               SUM(FSH.MIKTAR) AS MIKTAR,
--               SUM((((MIKTAR * SATISFIYATI) * (100 - ISNULL(FSH.ISK1, 0)) / 100) * (100 - ISNULL(FSH.ISK2, 0)) / 100) * (100 - ISNULL(FSB.ALTISKORAN, 0)) / 100) AS TUTAR,
--               ISNULL(FSH.PERKODU, 'YOK') PERSONEL
--        FROM TBLFASTERSATISHAREKET AS FSH
--        LEFT JOIN TBLFASTERSATISBASLIK AS FSB ON FSH.BASLIKIND = FSB.BASLIKIND
--        AND FSH.SUBEIND = FSB.SUBEIND
--        AND FSH.KASAIND = FSB.KASAIND
--        LEFT JOIN TBLFASTERSTOKLAR AS STK ON FSH.STOKIND=STK.STOKNO
--        WHERE FSH.ISLEMTARIHI >= @par1
--          AND FSH.ISLEMTARIHI <= @par2
--          AND ISNULL(FSB.IADE, 0) = 0
--        GROUP BY FSH.PERKODU,
--                 FSH.BASLIKIND
				 
--				 UNION ALL SELECT
--         0 SAY,
--               SUM(FSH.MIKTAR) * -1.00 AS MIKTAR,
--               SUM((((MIKTAR * SATISFIYATI) * (100 - ISNULL(FSH.ISK1, 0)) / 100) * (100 - ISNULL(FSH.ISK2, 0)) / 100) * (100 - ISNULL(FSB.ALTISKORAN, 0)) / 100) * -1.00 AS TUTAR,
--               ISNULL(FSH.PERKODU, 'YOK') PERSONEL
--        FROM TBLFASTERSATISHAREKET AS FSH
--        LEFT JOIN TBLFASTERSATISBASLIK AS FSB ON FSH.BASLIKIND = FSB.BASLIKIND
--        AND FSH.SUBEIND = FSB.SUBEIND
--        AND FSH.KASAIND = FSB.KASAIND
--        LEFT JOIN TBLFASTERSTOKLAR AS STK ON FSH.STOKIND=STK.STOKNO
--        WHERE FSH.ISLEMTARIHI >= @par1
--          AND FSH.ISLEMTARIHI <= @par2
--          AND ISNULL(FSB.IADE, 0) = 1
--        GROUP BY FSH.PERKODU,
--                 FSH.BASLIKIND
				 
--				 )			 				 
--				 ) AS T
--GROUP BY T.PERSONEL 

