DECLARE @Sube nvarchar(100) = '{SUBEADI}';
DECLARE @Trh1 nvarchar(20) = '{TARIH1}';
DECLARE @Trh2 nvarchar(20) = '{TARIH2}';
DECLARE @EndDate nvarchar(20) ='{EndDate}';

 Select 

 ROW_NUMBER() OVER(ORDER BY CAST(CONVERT(VARCHAR, DATEADD(DAY, 0, DATEDIFF(DAY, 0, DateStrNow)), 100) AS DATETIME) ASC) AS DayCount,
  sum(Cash) Cash,
  sum(credit) credit,
  sum(ticket) ticket,
  sum(debit) debit,
  sum(online) online, 
  sum(ikram) ikram,
  sum(ToplamCiro) ToplamCiro,
  DateStrNow,
  DayNumber
 from (

SELECT 
       ISNULL(Sum(CashPayment), 0.0) AS Cash,
       ISNULL(Sum(CreditPayment), 0.0) AS Credit,
       ISNULL(Sum(TicketPayment), 0.0) AS Ticket,
       ISNULL(Sum(Debit), 0.0) AS Debit,
	   ISNULL(Sum(OnlinePayment), 0.0) AS Online,
       0.0 AS ikram,
       SUM(CashPayment+CreditPayment+TicketPayment+OnlinePayment) AS ToplamCiro,
	   case when CAST(PaymentTime AS time)< @EndDate then CAST(PaymentTime-1 AS date) else CAST(PaymentTime AS date) end AS DateStrNow,
       CAST(DAY(case when CAST(PaymentTime AS time)< @EndDate then PaymentTime-1 else PaymentTime end) AS INT) AS DayNumber
FROM Payment
WHERE 
      case when CAST(PaymentTime AS time)< @EndDate then PaymentTime-1 else PaymentTime end >= @Trh1
  AND case when CAST(PaymentTime AS time)< @EndDate then PaymentTime-1 else PaymentTime end <= @Trh2
  
  GROUP BY PaymentTime
  ) 
  t

GROUP BY 
DateStrNow,
DayNumber
ORDER BY DateStrNow