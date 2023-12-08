
declare @Sube nvarchar(100) = '{SUBE}';
declare @Trh1 nvarchar(20) = '{TARIH1}';
declare @Trh2 nvarchar(20) = '{TARIH2}';



SELECT T.Sube1,
       T.Kasa,
       T.ID,
       SUM(T.MIKTAR) MIKTAR,
       SUM(T.TUTAR) TUTAR,
       T.ProductName
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
                          SUM((((MIKTAR*SATISFIYATI) * (100-ISNULL(FSH.ISK1, 0))/100) * (100-ISNULL(FSH.ISK2, 0))/100) * (100-ISNULL(FSB.ALTISKORAN, 0))/100)  AS TUTAR,
                          STK.MALINCINSI AS ProductName
         FROM TBLFASTERSATISHAREKET AS FSH
         LEFT JOIN TBLFASTERSATISBASLIK AS FSB ON FSH.BASLIKIND=FSB.BASLIKIND
         AND FSH.SUBEIND=FSB.SUBEIND
         AND FSH.KASAIND=FSB.KASAIND
         LEFT JOIN TBLFASTERSTOKLAR AS STK ON FSH.STOKIND=STK.STOKNO
         WHERE FSH.ISLEMTARIHI>=@Trh1
           AND FSH.ISLEMTARIHI<=@Trh2
           AND ISNULL(FSB.IADE, 0)=1
         GROUP BY FSB.SUBEIND,
                  FSB.KASAIND,
                  STK.MALINCINSI)) T
GROUP BY T.Sube1,
         T.Kasa,
         T.ID,
         T.ProductName
