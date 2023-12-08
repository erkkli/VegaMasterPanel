exec subebazli '##stoksatisadet', '(SELECT  Bill.ProductName, SUM(Bill.Quantity) AS MIKTAR,
SUM(Bill.Price*Quantity)  AS TUTAR, Product.ProductGroup AS ProductGroup FROM Product 
INNER JOIN Bill ON Product.Id = Bill.ProductId where [Date]>=''{TARIH1}'' AND [Date]<=''{TARIH2}''
AND Product.ProductName NOT LIKE ''$%'' GROUP BY Bill.ProductName, Product.ProductGroup)'; 
WITH vPaidBill AS( select ServerName as Sube, MIKTAR, TUTAR, ProductGroup from ##stoksatisadet ) SELECT  Sube SUBE,
SUM(vPaidBill.TUTAR) AS TUTAR from vPaidBill where Sube not in ('local') AND Sube='{SubeAdi}' AND ProductGroup = '{ProductGroup}' GROUP BY Sube