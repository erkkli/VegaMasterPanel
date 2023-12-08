      
declare @par1 nvarchar(20) = '{TARIH1}';
declare @par2 nvarchar(20) = '{TARIH2}';

--SELECT 
--UserName,sum(Price*Quantity) TUTAR
--FROM BillWithHeader
--where 
--Date>@par1 and Date<=@par2
--group by 
--UserName
--order by 
--Username


SELECT Top 1
 SUM(0) amount
,(SUM(CashPayment) + SUM(CreditPayment) + SUM(TicketPayment) ) as Total
, SUM(CashPayment) CashPayment
, SUM(CreditPayment) CreditPayment
, SUM(TicketPayment) TicketPayment
, SUM(0) Debit
, SUM(Discount) Discount
,ReceivedByUserName ReceivedByUserName

 FROM Payment  
 WHERE [PaymentTime]>=@par1 and [PaymentTime]<=@par2
 GROUP BY ReceivedByUserName

UNION ALL

SELECT
 SUM(0) amount
,(SUM(CashPayment) + SUM(CreditPayment) + SUM(TicketPayment) ) as Total
, SUM(CashPayment) CashPayment
, SUM(CreditPayment) CreditPayment
, SUM(TicketPayment) TicketPayment
, SUM(0) Debit
, SUM(Discount) Discount
,UserName ReceivedByUserName

 FROM PrePayment  
 WHERE [PaymentTime]>=@par1 and [PaymentTime]<=@par2
 GROUP BY UserName
 ORDER BY Total DESC

   
   
   
   --SELECT 
   --     Sum(Price) as TUTAR 
   --     ,ReceivedByUserName  as PersonelAdi
   --     FROM PaidBill 
   --     WHERE [Date]>='{TARIH1}' AND 
   --           [Date]<='{TARIH2}'
   --     group by ReceivedByUserName