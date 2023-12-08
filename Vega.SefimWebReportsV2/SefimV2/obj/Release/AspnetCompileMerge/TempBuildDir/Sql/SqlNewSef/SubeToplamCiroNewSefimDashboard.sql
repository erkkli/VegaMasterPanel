declare @Sube nvarchar(100) = '{SUBEADI}';
declare @Trh1 nvarchar(20) = '{TARIH1}';
declare @Trh2 nvarchar(20) = '{TARIH2}';

WITH Toplamsatis 
AS ( 
SELECT 
@Sube as Sube, 
0 AS Cash, 0 AS Credit, 0 AS Ticket, 0 AS Debit, 0 AS ikram, 0 AS TableNo, 0 AS Discount, 0 AS iptal,
ISNULL(Sum(Quantity * OriginalPrice),0) AS zayi 
FROM PaidBill
WHERE 
(PaidBill.Comment  LIKE  '%zayi%' or PaidBill.Comment LIKE '%ZAYI%')   
and PaidBill.PaymentTime >= @Trh1 and PaidBill.PaymentTime <= @Trh2
and ISNULL(Price,0) = 0

UNION 

SELECT 
@Sube as Sube, 
ISNULL(Sum(CashPayment),0) as Cash, ISNULL(Sum(CreditPayment),0) as Credit, ISNULL(Sum(TicketPayment),0) as Ticket,
0 as Debit, 0 AS ikram, 0 AS TableNo, 0 AS Discount, 0 AS iptal, 0 AS zayi
FROM Payment
WHERE 
Paymenttime >= @Trh1 AND Paymenttime <= @Trh2 and ISNULL(FastSale,0)=0 and Payment.TableNo<>'DEBIT' 

UNION 

SELECT 
@Sube as Sube, 
ISNULL(Sum(CashPayment),0) as Cash, ISNULL(Sum(CreditPayment),0) as Credit, ISNULL(Sum(TicketPayment),0) as Ticket, 
ISNULL(Sum(Debit),0) as Debit, 0 AS ikram, Count(TableNo) as TableNo, ISNULL(Sum(Discount),0) as Discount,
0 AS iptal, 0 AS zayi
FROM Payment
WHERE 
Paymenttime >= @Trh1 AND Paymenttime <= @Trh2 and  ISNULL(FastSale,0)=1 

UNION 

SELECT 
@Sube as Sube, 
0 as Cash, 0 as Credit, 0 as Ticket, 0 as Debit, ISNULL(Sum(Quantity*OriginalPrice),0) AS ikram, 0 as TableNo, 0 as Discount, 0 AS iptal, 0 AS zayi
FROM PaidBill
WHERE 
Paymenttime >= @Trh1 AND Paymenttime <= @Trh2 and Price=0 and ProductName Not Like '$%'
and PaidBill.Comment not LIKE '%ZAYi%' and PaidBill.Comment not LIKE '%ZAYI%'

UNION

SELECT 
@Sube as Sube, 
0 as Cash, 0 as Credit, 0 as Ticket, 0 as Debit, 0 AS ikram, 0 as TableNo, 0 as Discount, ISNULL(Sum(Quantity*OriginalPrice),0) AS iptal, 0 AS zayi
FROM DeletedBill
WHERE 
DeletingTime >= @Trh1 AND DeletingTime <= @Trh2 and ProductName Not Like '$%'

UNION ALL

SELECT 
@Sube as Sube, 
0 as Cash, 0 as Credit, 0 as Ticket, 0 as Debit, 0 AS ikram, 0 as TableNo, 0 as Discount, 0 AS iptal, 0 AS zayi,
ISNULL(SUM(ISNULL(B.Price,0)*ISNULL(B.Quantity,0)),0) - (SELECT SUM(Discount) FROM PhoneOrderHeader 
WHERE 
PhoneOrderHeader.CreationTime>=@Trh1 AND 
PhoneOrderHeader.CreationTime<=@Trh2) 

FROM PhoneOrderHeader D
JOIN Bill B ON B.HeaderId = D .HeaderId
WHERE  
D .CreationTime>=@Trh1 AND  D .CreationTime<=@Trh2

) 
SELECT Sube 
,SUM(Cash) AS Cash 
,SUM(Credit) AS Credit 
,Sum(Ticket) AS Ticket 
,SUM(PaketToplam) as PaketToplam
,Sum(Debit) AS Debit 
,Sum(ikram) AS ikram 
,Sum(TableNo) AS TableNo 
,Sum(Discount) AS Discount 
,Sum(iptal) AS iptal 
,Sum(Zayi) AS Zayi
,SUM(Cash+Credit+Ticket+Debit) AS ToplamCiro
,0 AS Saniye
,'' AS RowStyle
,'' AS RowError
FROM toplamsatis  
GROUP BY Sube