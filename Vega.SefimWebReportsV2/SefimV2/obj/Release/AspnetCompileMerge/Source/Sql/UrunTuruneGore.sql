exec subebazli '##stoksatisadet', '(SELECT  Bill.ProductName, SUM(Bill.Quantity) AS MIKTAR,
SUM(Bill.Price*Quantity)  AS TUTAR, Product.ProductGroup AS ProductGroup FROM Product 
INNER JOIN Bill ON Product.Id = Bill.ProductId where [Date]>=''{TARIH1}'' AND [Date]<=''{TARIH2}''
AND Product.ProductName NOT LIKE ''$%'' GROUP BY Bill.ProductName, Product.ProductGroup)'; 
WITH vPaidBill AS( select ServerName as Sube, ProductName, MIKTAR, TUTAR, ProductGroup from ##stoksatisadet ) SELECT ProductName, Sube SUBE, vPaidBill.MIKTAR, 
vPaidBill.TUTAR, ProductGroup from vPaidBill where Sube not in ('local') AND Sube='{SubeAdi}' AND ProductGroup = '{ProductGroup}'