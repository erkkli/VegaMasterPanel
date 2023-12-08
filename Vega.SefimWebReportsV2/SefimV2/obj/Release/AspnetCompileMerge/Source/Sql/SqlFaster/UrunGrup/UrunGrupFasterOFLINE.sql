﻿DECLARE @Sube nvarchar(100) = '{SUBEADI}';
DECLARE @par1 nvarchar(20) = '{TARIH1}';
DECLARE @par2 nvarchar(20) = '{TARIH2}';

--SELECT ProductName, SUM(Quantity) AS MIKTAR, SUM(Price*Quantity)  AS TUTAR FROM Bill WHERE [Date]>=@par1
--AND [Date]<=@par2 AND ProductName NOT LIKE '$%'   GROUP BY ProductName order by  TUTAR asc

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
          SUM((((MIKTAR*SATISFIYATI) * (100-ISNULL(FSH.ISK1, 0))/100) * (100-ISNULL(FSH.ISK2, 0))/100) * (100-ISNULL(FSB.ALTISKORAN, 0))/100) AS TUTAR
   FROM TBLFASTERSATISHAREKET AS FSH
   LEFT JOIN TBLFASTERSATISBASLIK AS FSB ON FSH.BASLIKIND=FSB.BASLIKIND
   AND FSH.SUBEIND=FSB.SUBEIND
   AND FSH.KASAIND=FSB.KASAIND --LEFT JOIN TBLFASTERSTOKLAR AS STK ON FSH.STOKIND=STK.IND

   LEFT JOIN TBLFASTERSTOKLAR AS STK ON FSH.STOKIND=STK.STOKNO
   WHERE FSH.ISLEMTARIHI>=@par1
     AND FSH.ISLEMTARIHI<=@par2
     AND ISNULL(FSB.IADE, 0)=0
   GROUP BY FSB.SUBEIND,
            FSB.KASAIND)