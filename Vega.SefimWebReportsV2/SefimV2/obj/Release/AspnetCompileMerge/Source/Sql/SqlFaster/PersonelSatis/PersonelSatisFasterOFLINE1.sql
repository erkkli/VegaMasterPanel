
declare @Sube nvarchar(100) = '{SUBE}';
declare @Trh1 nvarchar(20) = '{TARIH1}';
declare @Trh2 nvarchar(20) = '{TARIH2}';


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

     (SELECT USERNAME
      FROM TBLUSERS
      WHERE IND=TBLFASTERODEMELER.USERIND) AS PERSONEL
   FROM DBO.TBLFASTERODEMELER
   WHERE ODEMETIPI=0
     AND ISNULL(IADE, 0)=0
     AND TARIH >= @Trh1
     AND TARIH <= @Trh2
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

     (SELECT USERNAME
      FROM TBLUSERS
      WHERE IND=TBLFASTERODEMELER.USERIND) AS PERSONEL
   FROM DBO.TBLFASTERODEMELER
   WHERE ODEMETIPI=1
     AND ISNULL(IADE, 0)=0
     AND TARIH >= @Trh1
     AND TARIH <= @Trh2
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

     (SELECT USERNAME
      FROM TBLUSERS
      WHERE IND=TBLFASTERODEMELER.USERIND) AS PERSONEL
   FROM DBO.TBLFASTERODEMELER
   WHERE ODEMETIPI=2
     AND ISNULL(IADE, 0)=0
     AND TARIH >= @Trh1
     AND TARIH <= @Trh2 )
SELECT 
       SUM(Cash) AS CashPayment,
       SUM(Credit) AS CreditPayment,
       Sum(Ticket) AS TicketPayment,
       Sum(cash+Credit+Ticket) AS Total,
       PERSONEL
    
FROM toplamsatis
GROUP BY 
         PERSONEL

