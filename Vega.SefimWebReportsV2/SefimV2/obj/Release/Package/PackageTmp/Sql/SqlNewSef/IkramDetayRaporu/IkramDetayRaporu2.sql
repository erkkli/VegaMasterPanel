
declare @Trh1 nvarchar(20) = '{TARIH1}';
declare @Trh2 nvarchar(20) = '{TARIH2}';

SELECT  month(body.Date) as ay, day(body.Date) as gun, YEAR(body.Date) as yil, convert(char(5), body.Date, 108) as zaman,
body.comment,body.OriginalPrice*Quantity as OriginalPrice,body.productName,header.TableNumber,body.UserName,body.OriginalPrice
 FROM BillHeader  AS header
left JOIN Bill as body on header.Id=body.headerId
JOIN Payment
ON Payment.HeaderId=body.HeaderId
where body.price=0 and body.OrderState=0  and
body.Date>=@Trh1 AND body.date<=@Trh2 