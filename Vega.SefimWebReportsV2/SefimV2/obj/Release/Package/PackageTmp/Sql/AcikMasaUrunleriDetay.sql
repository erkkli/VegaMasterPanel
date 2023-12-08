
(SELECT TableNumber, ProductName,Date, (Price*Quantity)  AS TUTAR,username FROM BillWithHeader 
WHERE[Date]>='{TARIH1}' AND [Date]<='{TARIH2}' AND TableNumber='{TableNumber}' AND BillState=0 AND ISNULL(BillType,0) IN (0,1)) 