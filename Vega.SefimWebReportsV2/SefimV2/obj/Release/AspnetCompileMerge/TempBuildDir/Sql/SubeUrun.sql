exec subebazli '##SubeUrunler', '( SELECT ProductName, SUM(Quantity) AS MIKTAR, SUM(Price*Quantity)  AS TUTAR FROM Bill WHERE [Date]>=''{TARIH1}''
AND [Date]<=''{TARIH2}'' AND ProductName NOT LIKE ''$%'' GROUP BY ProductName )'; WITH vPaidBill AS( select ServerName as Sube, TUTAR from ##SubeUrunler )
SELECT Sube SUBE, sum(vPaidBill.TUTAR) as TUTAR from vPaidBill where Sube not in ('local') and Sube='{SUBEADI}' group by SUBE 