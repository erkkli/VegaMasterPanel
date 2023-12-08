declare @par1 nvarchar(20) = '{TARIH1}';
declare @par2 nvarchar(20) = '{TARIH2}';

WITH
IncludedUsers AS (
SELECT 
	UserName AS UNJ 
FROM [User] 
WHERE 
	ISNULL([User].Branch,'''') IN ('''') AND 
	ISNULL([User].Department,'''') IN ('''')
    ),


PaidBillx AS (SELECT PaidBill.* FROM PaidBill LEFT JOIN IncludedUsers ON PaidBill.UserName=IncludedUsers.UNJ),
Billx AS (SELECT Bill.* FROM Bill LEFT JOIN IncludedUsers ON Bill.UserName=IncludedUsers.UNJ),
BillHeaderx AS (SELECT BillHeader.* FROM BillHeader WHERE BillHeader.Id IN (SELECT DISTINCT HeaderId FROM Billx)),
DeletedBillx AS (SELECT DeletedBill.* FROM DeletedBill LEFT JOIN IncludedUsers ON DeletedBill.UserName=IncludedUsers.UNJ),
DebitPaymentx AS (SELECT Payment.* FROM Payment LEFT JOIN IncludedUsers ON Payment.ReceivedByUserName=IncludedUsers.UNJ where Payment.TableNo='DEBIT' And  Payment.Debit=0),
Collectx AS (SELECT Payment.* FROM Payment LEFT JOIN IncludedUsers ON Payment.ReceivedByUserName=IncludedUsers.UNJ WHERE ISNULL(FastSale,0)=0 and Payment.TableNo<>'DEBIT' ),
DirectTransactionx AS (SELECT DirectTransaction.* FROM DirectTransaction LEFT JOIN IncludedUsers ON DirectTransaction.UserName=IncludedUsers.UNJ),
PhoneOrderHeaderx AS (SELECT PhoneOrderHeader.* FROM PhoneOrderHeader LEFT JOIN IncludedUsers ON PhoneOrderHeader.CreatedByUserName=IncludedUsers.UNJ),
Paymentx AS (SELECT Payment.* FROM Payment LEFT JOIN IncludedUsers ON Payment.ReceivedByUserName=IncludedUsers.UNJ WHERE ISNULL(FastSale,0)=1 ),
BillWithHeaderx AS (SELECT BillWithHeader.* FROM BillWithHeader LEFT JOIN IncludedUsers ON BillWithHeader.UserName=IncludedUsers.UNJ)
SELECT 


COUNT(*) AS OrderCount,

(SELECT SUM(ISNULL(B.Price,0)*ISNULL(B.Quantity,0))FROM PhoneOrderHeaderx
JOIN Bill B ON B.HeaderId = PhoneOrderHeaderx.HeaderId
WHERE  
PhoneOrderHeaderx.CreationTime>=@par1 AND 
PhoneOrderHeaderx.CreationTime<=@par2)
 AS Total,


(SELECT SUM(ISNULL(B.Price,0)*ISNULL(B.Quantity,0))FROM PhoneOrderHeaderx
JOIN Bill B ON B.HeaderId = PhoneOrderHeaderx.HeaderId
WHERE 
ISNULL(PhoneOrderHeaderx.Paid,0)=0 AND
PhoneOrderHeaderx.CreationTime>=@par1 AND 
PhoneOrderHeaderx.CreationTime<=@par2)
-
ISNULL((SELECT SUM(Discount) FROM PhoneOrderHeaderx
WHERE 
ISNULL(PhoneOrderHeaderx.Paid,0)=0 AND
PhoneOrderHeaderx.CreationTime>=@par1 AND 
PhoneOrderHeaderx.CreationTime<=@par2),0)

AS Debit,


ISNULL((SELECT SUM(CashPayment) FROM Collectx WHERE PaymentTime>=@par1 AND PaymentTime<=@par2 ),0) AS CashPayment,

ISNULL((SELECT SUM(CreditPayment) FROM Collectx WHERE PaymentTime>=@par1 AND PaymentTime<=@par2 ),0) AS CreditPayment,

ISNULL((SELECT SUM(TicketPayment) FROM Collectx WHERE PaymentTime>=@par1 AND PaymentTime<=@par2 ),0) AS TicketPayment,

ISNULL((SELECT SUM(OnlinePayment) FROM Collectx WHERE PaymentTime>=@par1 AND PaymentTime<=@par2 ),0) AS OnlinePayment,
ISNULL((SELECT SUM(Cashpayment+CreditPayment+TicketPayment+OnlinePayment) FROM Collectx WHERE PaymentTime>=@par1 AND PaymentTime<=@par2 and Deliverer='GETIR'   ),0) AS GETIR,
ISNULL((SELECT SUM(Cashpayment+CreditPayment+TicketPayment+OnlinePayment) FROM Collectx WHERE PaymentTime>=@par1 AND PaymentTime<=@par2 and Deliverer='TRENDYOL'   ),0) AS TRENDYOL,
ISNULL((SELECT SUM(Cashpayment+CreditPayment+TicketPayment+OnlinePayment) FROM Collectx WHERE PaymentTime>=@par1 AND PaymentTime<=@par2 and Deliverer='YEMEK SEPETI'   ),0) AS YEMEKSEPETI,

ISNULL((SELECT SUM(Discount) FROM PhoneOrderHeaderx WHERE CreationTime>=@par1 AND CreationTime<=@par2 ),0) 
+ISNULL((SELECT SUM(Discount) FROM Collectx WHERE PaymentTime>=@par1 AND PaymentTime<=@par2 ),0) 

AS Discount,

ISNULL((SELECT SUM(CashPayment) + SUM(CreditPayment)+ SUM(TicketPayment) FROM Collectx WHERE  PaymentTime>=@par1 AND PaymentTime<=@par2 ),0) AS	CollectedTotal,

(SELECT SUM(ISNULL(B.Price,0)*ISNULL(B.Quantity,0)) FROM PhoneOrderHeaderx
JOIN Bill B ON B.HeaderId = PhoneOrderHeaderx.HeaderId
WHERE 
ISNULL(PhoneOrderHeaderx.Paid,0)=1 AND 
PhoneOrderHeaderx.CreationTime>=@par1 AND 
PhoneOrderHeaderx.CreationTime<=@par2)
- 
ISNULL((SELECT SUM(Discount) FROM PhoneOrderHeaderx WHERE ISNULL(PhoneOrderHeaderx.Paid,0)=1 AND PhoneOrderHeaderx.CreationTime>=@par1 AND PhoneOrderHeaderx.CreationTime<=@par2),0)
-
ISNULL((SELECT SUM(CashPayment) + SUM(CreditPayment)+ SUM(TicketPayment) + SUM(Discount) FROM Collectx WHERE PaymentTime>=@par1 AND PaymentTime<=@par2 ),0)  as Balance

FROM PhoneOrderHeaderx as D

WHERE D.CreationTime>=@par1 AND D.CreationTime<=@par2