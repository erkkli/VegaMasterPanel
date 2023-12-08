declare @par1 nvarchar(20) = '{TARIH1}';
declare @par2 nvarchar(20) = '{TARIH2}';
declare @Sube nvarchar(100) = '{SUBEADI}';
declare @ProductsName nvarchar(100) = '{ProductName}';

SELECT Id,Date,UserName,ProductName,ProductId,Choice1Id,Choice2Id,Options, (Price*(-Quantity)) Price,abs(Quantity) Quantity,Comment,HeaderId,OrderId from PaidBill 

WHERE [PaymentTime]>=@par1 AND [PaymentTime]<=@par2 AND ISNULL(Quantity,0)<0 and ProductName=@ProductsName
ORDER BY Id


