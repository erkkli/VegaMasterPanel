declare @par1 nvarchar(20) = '{TARIH1}';
declare @par2 nvarchar(20) = '{TARIH2}';
declare @Sube nvarchar(100) = '{SUBEADI}';


SELECT sum(Price*(-Quantity)) Tutar,abs(sum(Quantity)) Miktar,ProductName from PaidBill 

WHERE [PaymentTime]>=@par1 AND [PaymentTime]<=@par2 AND ISNULL(Quantity,0)<0

group by ProductName



