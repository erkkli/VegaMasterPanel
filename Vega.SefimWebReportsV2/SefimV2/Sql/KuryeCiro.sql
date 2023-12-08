declare @par1 nvarchar(20) = '{TARIH1}';
declare @par2 nvarchar(20) = '{TARIH2}';

WITH

Collectx AS (SELECT Collect.* FROM Collect ),
PhoneOrderHeaderx AS (SELECT PhoneOrderHeader.* FROM PhoneOrderHeader )

SELECT

D.Deliverer,
D.CreationTime,
D.OrderNo,
COUNT(*) AS OrderCount,

(SELECT SUM(ISNULL(B.Price,0)*ISNULL(B.Quantity,0))-ISNULL((SELECT SUM(Discount) FROM PhoneOrderHeaderx
JOIN BillHeader B ON B.Id = PhoneOrderHeaderx.HeaderId
WHERE
PhoneOrderHeaderx.OrderNo=D.Orderno and
PhoneOrderHeaderx.Deliverer=D.Deliverer AND
PhoneOrderHeaderx.CreationTime>=@par1 AND
PhoneOrderHeaderx.CreationTime<=@par2
and B.Id=PhoneOrderHeaderx.HeaderId

),0

) FROM PhoneOrderHeaderx
JOIN Bill B ON B.HeaderId = PhoneOrderHeaderx.HeaderId
WHERE PhoneOrderHeaderx.Deliverer=D.Deliverer AND
PhoneOrderHeaderx.CreationTime>=@par1 AND
PhoneOrderHeaderx.CreationTime<=@par2 and
B.HeaderId=D.HeaderId)
 AS Total,

(SELECT SUM(ISNULL(B.Price,0)*ISNULL(B.Quantity,0))FROM PhoneOrderHeaderx
JOIN Bill B ON B.HeaderId = PhoneOrderHeaderx.HeaderId
WHERE PhoneOrderHeaderx.Deliverer=D.Deliverer AND
ISNULL(PhoneOrderHeaderx.Paid,0)=0 AND
PhoneOrderHeaderx.CreationTime>=@par1 AND
PhoneOrderHeaderx.CreationTime<=@par2)
-
ISNULL((SELECT SUM(Discount) FROM PhoneOrderHeaderx
WHERE PhoneOrderHeaderx.Deliverer=D.Deliverer AND
ISNULL(PhoneOrderHeaderx.Paid,0)=0 AND
PhoneOrderHeaderx.CreationTime>=@par1 AND
PhoneOrderHeaderx.CreationTime<=@par2),0)

AS Debit,

ISNULL((SELECT SUM(CashPayment) FROM Collectx
WHERE Collectx.Deliverer=D.Deliverer AND
PaymentTime>=@par1 AND PaymentTime<=@par2
),0) AS CashPayment,

ISNULL((SELECT SUM(CreditPayment) FROM Collectx
WHERE Collectx.Deliverer=D.Deliverer AND
PaymentTime>=@par1 AND PaymentTime<=@par2
),0) AS CreditPayment,

ISNULL((SELECT SUM(TicketPayment) FROM Collectx
WHERE Collectx.Deliverer=D.Deliverer AND
PaymentTime>=@par1 AND PaymentTime<=@par2
),0) AS TicketPayment,


ISNULL((SELECT SUM(Discount) FROM Collectx
WHERE Collectx.Deliverer=D.Deliverer AND
PaymentTime>=@par1 AND PaymentTime<=@par2
),0) AS Discount,

ISNULL((SELECT SUM(CashPayment) + SUM(CreditPayment)+ SUM(TicketPayment)
FROM Collectx WHERE Collectx.Deliverer=D.Deliverer AND
PaymentTime>=@par1 AND PaymentTime<=@par2
),0) AS CollectedTotal,

(SELECT SUM(ISNULL(B.Price,0)*ISNULL(B.Quantity,0))
FROM PhoneOrderHeaderx
JOIN Bill B ON B.HeaderId = PhoneOrderHeaderx.HeaderId
WHERE PhoneOrderHeaderx.Deliverer=D.Deliverer AND
ISNULL(PhoneOrderHeaderx.Paid,0)=1 AND
PhoneOrderHeaderx.CreationTime>=@par1 AND
PhoneOrderHeaderx.CreationTime<=@par2)
-
ISNULL((SELECT SUM(Discount) FROM PhoneOrderHeaderx
WHERE PhoneOrderHeaderx.Deliverer=D.Deliverer AND
ISNULL(PhoneOrderHeaderx.Paid,0)=1 AND
PhoneOrderHeaderx.CreationTime>=@par1 AND
PhoneOrderHeaderx.CreationTime<=@par2),0)
-
ISNULL((SELECT SUM(CashPayment) + SUM(CreditPayment)+ SUM(TicketPayment) + SUM(Discount)
FROM Collectx WHERE Collectx.Deliverer=D.Deliverer AND
PaymentTime>=@par1 AND PaymentTime<=@par2
),0)  as Balance

FROM PhoneOrderHeaderx as D
WHERE D.CreationTime>=@par1 AND D.CreationTime<=@par2
GROUP BY D.Deliverer,D.CreationTime,D.OrderNo,D.HeaderId
ORDER BY D.Deliverer

