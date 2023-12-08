
declare @Trh1 nvarchar(20) = '{TARIH1}';
declare @Trh2 nvarchar(20) = '{TARIH2}';


SELECT TableNo,PaymentTime,ReceivedByUserName,DiscountReason,CashPayment,CreditPayment,TicketPayment,Discount   FROM dbo.Payment AS P 
WHERE P.Discount>0 AND
[PaymentTime]>=@Trh1 AND [PaymentTime]<=@Trh2

UNION ALL
SELECT 'PAKETÇÝ' TableNo,CreationTime PaymentTime,CreatedByUserName ReceivedByUserName,'' DiscountReason,0 CashPayment,0 CreditPayment,0 TicketPayment,Discount  FROM PhoneOrderHeader WHERE 
 Discount>0 
AND CreationTime>=@Trh1 AND CreationTime<=@Trh2 
