
Declare @par1 nvarchar(20) = '{TARIH1}';
Declare @par2 nvarchar(20) = '{TARIH2}';

	SELECT 
	BillHeader.Id,
	BillHeader.BillType,
	BillHeader.TableNumber,
	(SELECT SUM(Quantity * Price) From Bill WHERE HeaderId=BillHeader.Id) AS Total,
	(SELECT TOP 1 UserName From Bill WHERE HeaderId=BillHeader.Id ORDER BY [Date] ASC) AS FirstUserName,
	(SELECT TOP 1 UserName From Bill WHERE HeaderId=BillHeader.Id ORDER BY [Date] DESC) AS LastUserName,
	(SELECT MIN(Date) From Bill WHERE HeaderId=BillHeader.Id) AS FirstOrderTime,
	(SELECT MAX(Date) From Bill WHERE HeaderId=BillHeader.Id) AS LastOrderTime,
	(SELECT TOP 1 ReceivedByUserName From Payment WHERE HeaderId=BillHeader.Id) AS Cashier,
	(SELECT TOP 1 CustomerName From Payment WHERE HeaderId=BillHeader.Id) AS CustomerName
	FROM BillHeader
	WHERE BillHeader.Id IN (SELECT HeaderId FROM Bill WHERE Date>=@par1 AND Date <=@par2)