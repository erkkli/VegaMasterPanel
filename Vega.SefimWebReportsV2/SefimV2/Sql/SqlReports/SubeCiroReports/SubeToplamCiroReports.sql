DECLARE @Sube nvarchar(100) = '{SUBEADI}';

DECLARE @Trh1 nvarchar(20) = '{TARIH1}';

DECLARE @Trh2 nvarchar(20) = '{TARIH2}';

--declare @Sube nvarchar(100) = 'A Şubesi';
 --                  declare @Trh1 nvarchar(20) = '2019-02-01 00:00:00';
 --                  declare @Trh2 nvarchar(20) = '2019-02-28 23:59:59';

SELECT ROW_NUMBER() OVER(ORDER BY CAST(CONVERT(VARCHAR, DATEADD(DAY, 0, DATEDIFF(DAY, 0, PaymentTime)), 100) AS DATETIME) ASC) AS DayCount,
       ISNULL(Sum(CashPayment), 0.0) AS Cash,
       ISNULL(Sum(CreditPayment), 0.0) AS Credit,
       ISNULL(Sum(TicketPayment), 0.0) AS Ticket,
       ISNULL(Sum(Debit), 0.0) AS Debit,
       0.0 AS ikram,
       SUM(CashPayment+CreditPayment+TicketPayment) AS ToplamCiro,
       CAST(CONVERT(VARCHAR, DATEADD(DAY, 0, DATEDIFF(DAY, 0, PaymentTime)), 100) AS DATETIME) AS DateStrNow,
       CAST(DAY(PaymentTime) AS INT) AS DayNumber
FROM Payment
WHERE 
      CAST(CONVERT(VARCHAR, DATEADD(DAY, 0, DATEDIFF(DAY, 0, PaymentTime)), 100) AS DATETIME) >= @Trh1
  AND CAST(CONVERT(VARCHAR, DATEADD(DAY, 0, DATEDIFF(DAY, 0, PaymentTime)), 100) AS DATETIME) <= @Trh2
GROUP BY CAST(CONVERT(VARCHAR, DATEADD(DAY, 0, DATEDIFF(DAY, 0, PaymentTime)), 100) AS DATETIME),
         CAST(DAY(PaymentTime) AS INT)
ORDER BY DateStrNow