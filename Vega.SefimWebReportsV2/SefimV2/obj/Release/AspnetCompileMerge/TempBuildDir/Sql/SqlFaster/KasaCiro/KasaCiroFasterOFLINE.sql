DECLARE @Sube nvarchar(100) = '{SUBEADI}';

DECLARE @Trh1 nvarchar(20) = '{TARIH1}';

DECLARE @Trh2 nvarchar(20) = '{TARIH2}';


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
          0 AS KasaToplam,
		  0 AS iptal
   FROM DBO.TBLFASTERODEMELER
   WHERE ODEMETIPI=0
     AND ISNULL(IADE, 0)=0
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
          0 AS KasaToplam,
		  0 AS iptal
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
                    0 AS KasaToplam,
		  0 AS iptal
   FROM DBO.TBLFASTERODEMELER
   WHERE ODEMETIPI=1
     AND ISNULL(IADE, 0)=0
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
                    0 AS KasaToplam,
		  0 AS iptal
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
                    0 AS Ticket,
                    0 AS KasaToplam,
                    ODENEN AS iptal
        
   FROM DBO.TBLFASTERODEMELER
   WHERE ISNULL(IADE, 0) = 1 AND ODEMETIPI NOT IN (0,1,2)
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
                    0 AS KasaToplam,
		  0 AS iptal
   FROM DBO.TBLFASTERODEMELER
   WHERE ODEMETIPI=2
     AND ISNULL(IADE, 0)=0
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
                    0 AS KasaToplam,
		  0 AS iptal
   FROM DBO.TBLFASTERODEMELER
   WHERE ODEMETIPI=2
     AND ISNULL(IADE, 0)=1
     AND ISLEMTARIHI >= @Trh1
     AND ISLEMTARIHI <= @Trh2

   UNION ALL SELECT @Sube AS Sube,

     (SELECT TOP 1 SUBEADI
      FROM TBLFASTERKASALAR
      WHERE SUBENO=TBLFASTERKASAISLEMLERI.SUBEIND) AS Sube1,

     (SELECT KASAADI
      FROM TBLFASTERKASALAR
      WHERE KASANO=TBLFASTERKASAISLEMLERI.KASAIND) AS Kasa,
                    0 AS cash,
                    0 AS Credit,
                    0 AS Ticket,
                    SUM(GELIR-GIDER) AS KasaToplam,
		  0 AS iptal
   FROM DBO.TBLFASTERKASAISLEMLERI
   WHERE ISLEMTARIHI >= @Trh1
     AND ISLEMTARIHI <= @Trh2
   GROUP BY SUBEIND,
            KASAIND)
SELECT Sube,
       Sube1,
       Kasa,
       SUM(Cash) AS Cash,
       SUM(Credit) AS Credit,
       Sum(Ticket) AS Ticket,
       Sum(KasaToplam) AS KasaToplam,
       SUM(Cash+Credit+Ticket)-SUM(iptal) AS ToplamCiro,
	   
       0 AS Saniye,
       '' AS RowStyle,
       '' AS RowError
FROM toplamsatis
GROUP BY Sube,
         Sube1,
         Kasa 