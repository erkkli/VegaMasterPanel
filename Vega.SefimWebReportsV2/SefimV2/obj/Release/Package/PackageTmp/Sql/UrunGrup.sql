
EXEC subebazli '##SubeUrunGruplari' ,'( SELECT SUM(Quantity) AS MIKTAR,  SUM(Bill.Price*Quantity)  AS TUTAR, 
Product.ProductGroup AS ProductGroup FROM Product INNER JOIN Bill ON Product.Id = Bill.ProductId 
WHERE [Date]>=''{TARIH1}'' AND [Date]<=''{TARIH2}'' AND Bill.ProductName NOT LIKE ''$%''
GROUP BY Product.ProductGroup )' ,'{SubeAdi}'; WITH vPaidBill AS 
( SELECT ServerName AS Sube ,TUTAR FROM ##SubeUrunGruplari ) 
SELECT Sube SUBE ,sum(vPaidBill.TUTAR) as TUTAR FROM vPaidBill WHERE Sube NOT IN ('local') and Sube='{SubeAdi}' group by SUBE