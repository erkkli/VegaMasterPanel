
declare @Trh1 nvarchar(20) = '{TARIH1}';
declare @Trh2 nvarchar(20) = '{TARIH2}';


SELECT ISNULL(Sum(body.OriginalPrice),0) AS ToplamIkramTutari
 FROM BillHeader  AS header
left JOIN Bill as body on header.Id=body.headerId
JOIN Payment
ON Payment.HeaderId=body.HeaderId
where body.price=0 and body.OrderState=0  and
body.Date>=@Trh1 AND body.date<=@Trh2 