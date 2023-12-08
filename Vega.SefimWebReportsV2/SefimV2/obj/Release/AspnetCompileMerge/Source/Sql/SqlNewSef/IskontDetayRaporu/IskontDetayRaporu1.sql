
declare @Trh1 nvarchar(20) = '{TARIH1}';
declare @Trh2 nvarchar(20) = '{TARIH2}';


SELECT SUM(T.DiscountTotal) AS DiscountTotal FROM (

SELECT ISNULL(Sum(Discount),0) as DiscountTotal  FROM dbo.Payment AS P 
WHERE P.Discount>0 AND
[PaymentTime]>=@Trh1 AND [PaymentTime]<=@Trh2

UNION ALL
SELECT ISNULL(SUM(Discount),0) DiscountTotal FROM PhoneOrderHeader WHERE  Discount>0 AND  CreationTime>=@Trh1 AND CreationTime<=@Trh2 
) AS T