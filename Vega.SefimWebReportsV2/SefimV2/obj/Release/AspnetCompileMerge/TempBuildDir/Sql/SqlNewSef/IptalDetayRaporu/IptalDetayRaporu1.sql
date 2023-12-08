
declare @Trh1 nvarchar(20) = '{TARIH1}';
declare @Trh2 nvarchar(20) = '{TARIH2}';


SELECT 
 ISNULL(Sum(Quantity*OriginalPrice),0) AS ToplamIptalTutari,
 ISNULL(Sum(Quantity),0) AS ToplamIptalMiktari
FROM DeletedBill
WHERE 
DeletingTime >= @Trh1 AND DeletingTime <= @Trh2 and ProductName Not Like '$%'
