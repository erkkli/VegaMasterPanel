      
declare @par1 nvarchar(20) = '{TARIH1}';
declare @par2 nvarchar(20) = '{TARIH2}';

SELECT  TOP 1 WITH TIES (SUM(CashPayment) + SUM(CreditPayment) + SUM(TicketPayment)) as Total,
 SUM(0) amount

, SUM(CashPayment) CashPayment
, SUM(CreditPayment) CreditPayment
, SUM(TicketPayment) TicketPayment
, SUM(0) Debit
, SUM(Discount) Discount
,ReceivedByUserName 

 FROM Payment  
 WHERE [PaymentTime]>=@par1 and [PaymentTime]<=@par2
 GROUP BY ReceivedByUserName

 ORDER BY  Total DESC