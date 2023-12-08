declare @par1 nvarchar(20) = '{TARIH1}';
declare @par2 nvarchar(20) = '{TARIH2}';
declare @Sube nvarchar(100) = '{SUBEADI}';

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
PhoneOrderHeaderx AS (SELECT Payment.* FROM Payment LEFT JOIN IncludedUsers ON Payment.ReceivedByUserName=IncludedUsers.UNJ where Payment.TableNo='DEBIT' And  Payment.Debit=0),
Paymentx AS (SELECT Payment.* FROM Payment LEFT JOIN IncludedUsers ON Payment.ReceivedByUserName=IncludedUsers.UNJ WHERE ISNULL(FastSale,0)=1 ),
Paymentx2 AS (SELECT Payment.* FROM Payment LEFT JOIN IncludedUsers ON Payment.ReceivedByUserName=IncludedUsers.UNJ WHERE Payment.TableNo<>'DEBIT' ),

BillWithHeaderx AS (SELECT BillWithHeader.* FROM BillWithHeader LEFT JOIN IncludedUsers ON BillWithHeader.UserName=IncludedUsers.UNJ)
SELECT 

COUNT(*) AS OrderCount,

ISNULL((SELECT SUM(CashPayment)+SUM(TicketPayment)+SUM(CreditPayment)  FROM DebitPaymentx 
WHERE 
PaymentTime>=@par1 AND PaymentTime<=@par2 
),0) AS Total,

(SELECT  SUM(debit)   as Debit FROM Paymentx2 WHERE  Paymenttime >= @par1 AND Paymenttime <= @par2 )
Debit

--ISNULL((SELECT SUM(CashPayment) FROM DebitPaymentx 
--WHERE 
--PaymentTime>=@par1 AND PaymentTime<=@par2 
--),0) AS CashPayment,

--ISNULL((SELECT SUM(CreditPayment) FROM DebitPaymentx 
--WHERE 
--PaymentTime>=@par1 AND PaymentTime<=@par2 
--),0) AS CreditPayment,

--ISNULL((SELECT SUM(TicketPayment) FROM DebitPaymentx 
--WHERE 
--PaymentTime>=@par1 AND PaymentTime<=@par2 
--),0) AS TicketPayment,

--(SELECT  SUM(Discount)   as Discount FROM DebitPaymentx WHERE  Paymenttime >= @par1 AND Paymenttime <= @par2 )
--AS Discount,

--0 AS	CollectedTotal,

--0 as Balance

FROM PhoneOrderHeaderx as D

WHERE D.PaymentTime>=@par1 AND D.PaymentTime<=@par2




--WITH
--IncludedUsers AS (
--SELECT 
--	UserName AS UNJ 
--FROM [User] 
--WHERE 
--	ISNULL([User].Branch,'''') IN ('''') AND 
--	ISNULL([User].Department,'''') IN ('''')
--    ),

--PaidBillx AS (SELECT PaidBill.* FROM PaidBill LEFT JOIN IncludedUsers ON PaidBill.UserName=IncludedUsers.UNJ),
--Billx AS (SELECT Bill.* FROM Bill LEFT JOIN IncludedUsers ON Bill.UserName=IncludedUsers.UNJ),
--BillHeaderx AS (SELECT BillHeader.* FROM BillHeader WHERE BillHeader.Id IN (SELECT DISTINCT HeaderId FROM Billx)),
--DeletedBillx AS (SELECT DeletedBill.* FROM DeletedBill LEFT JOIN IncludedUsers ON DeletedBill.UserName=IncludedUsers.UNJ),
--DebitPaymentx AS (SELECT Payment.* FROM Payment LEFT JOIN IncludedUsers ON Payment.ReceivedByUserName=IncludedUsers.UNJ where Payment.TableNo='DEBIT' And  Payment.Debit=0),
--Collectx AS (SELECT Payment.* FROM Payment LEFT JOIN IncludedUsers ON Payment.ReceivedByUserName=IncludedUsers.UNJ WHERE ISNULL(FastSale,0)=0 and Payment.TableNo<>'DEBIT' ),
--DirectTransactionx AS (SELECT DirectTransaction.* FROM DirectTransaction LEFT JOIN IncludedUsers ON DirectTransaction.UserName=IncludedUsers.UNJ),
--PhoneOrderHeaderx AS (SELECT Payment.* FROM Payment LEFT JOIN IncludedUsers ON Payment.ReceivedByUserName=IncludedUsers.UNJ where Payment.TableNo='DEBIT' And  Payment.Debit=0),
--Paymentx AS (SELECT Payment.* FROM Payment LEFT JOIN IncludedUsers ON Payment.ReceivedByUserName=IncludedUsers.UNJ WHERE ISNULL(FastSale,0)=1 ),
--Paymentx2 AS (SELECT Payment.* FROM Payment LEFT JOIN IncludedUsers ON Payment.ReceivedByUserName=IncludedUsers.UNJ WHERE Payment.TableNo<>'DEBIT' ),

--BillWithHeaderx AS (SELECT BillWithHeader.* FROM BillWithHeader LEFT JOIN IncludedUsers ON BillWithHeader.UserName=IncludedUsers.UNJ)
--SELECT 

--COUNT(*) AS OrderCount,

--ISNULL((SELECT SUM(CashPayment)+SUM(TicketPayment)+SUM(CreditPayment)  FROM DebitPaymentx 
--WHERE 
--PaymentTime>=@par1 AND PaymentTime<=@par2 
--),0) AS Total,

--(SELECT  SUM(debit)   as Debit FROM Paymentx2 WHERE  Paymenttime >= @par1 AND Paymenttime <= @par2 )
--Debit,

--ISNULL((SELECT SUM(CashPayment) FROM DebitPaymentx 
--WHERE 
--PaymentTime>=@par1 AND PaymentTime<=@par2 
--),0) AS CashPayment,

--ISNULL((SELECT SUM(CreditPayment) FROM DebitPaymentx 
--WHERE 
--PaymentTime>=@par1 AND PaymentTime<=@par2 
--),0) AS CreditPayment,

--ISNULL((SELECT SUM(TicketPayment) FROM DebitPaymentx 
--WHERE 
--PaymentTime>=@par1 AND PaymentTime<=@par2 
--),0) AS TicketPayment,

--(SELECT  SUM(Discount)   as Discount FROM DebitPaymentx WHERE  Paymenttime >= @par1 AND Paymenttime <= @par2 )
--AS Discount,

--0 AS	CollectedTotal,

--0 as Balance

--FROM PhoneOrderHeaderx as D

--WHERE D.PaymentTime>=@par1 AND D.PaymentTime<=@par2