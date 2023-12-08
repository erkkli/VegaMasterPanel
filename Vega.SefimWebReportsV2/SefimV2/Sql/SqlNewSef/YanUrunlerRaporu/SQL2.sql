
WITH TBLVERI AS (
SELECT * FROM (
SELECT T.Tarih,T.KOD3,SUM(T.MIKTAR) AS MIKTAR,sum(Tutar) as TUTAR FROM (
SELECT 
NEWID() AS TEKIL,
KOD1 AS KOD3,
CONVERT(nvarchar(10),DATE,102) AS Tarih,
SUM(Quantity) as MIKTAR,
SUM(Quantity*Price) AS TUTAR

 FROM [dbo].[Bill] AS HR
WHERE Date>='{TARIH1}'  AND Date<='{TARIH2}' #SUBEADIWHERE# #TEKTRHWHERE#
AND KOD3 LIKE '%YAN URUNLER%'
group by
KOD1,
CONVERT(nvarchar(10),DATE,102)
UNION
SELECT 
NEWID() AS TEKIL,
HAR.VEGAKOD1,
CONVERT(nvarchar(10),DATE,102) as Tarih,
SUM(MIKTAR*ANAMIKTAR) as Miktar,
SUM(MIKTAR*ANAMIKTAR*HAR.Price) as TUTAR

 FROM [dbo].TBLBILLOPTIONS AS HAR
 LEFT JOIN [Bill] AS BAS ON BAS.Id=HAR.BIND
WHERE TARIH>'{TARIH1}'  AND TARIH<='{TARIH2}' #SUBEADIWHERE# #TEKTRHWHERE#
AND HAR.KOD3 LIKE '%YAN URUNLER%'
group by
HAR.VEGAKOD1,
CONVERT(nvarchar(10),DATE,102)
) AS T
group by
T.Tarih,T.KOD3
) AS T) 

SELECT 
*,
(SELECT SUM (MIKTAR) FROM TBLVERI WHERE Tarih=VER.Tarih) AS ToplamTarihMiktar,
(SELECT SUM (TUTAR) FROM TBLVERI WHERE Tarih=VER.Tarih) AS ToplamTarihTutar,
(SELECT SUM (TUTAR) FROM TBLVERI) AS ToplamTutar,
(TUTAR/(SELECT SUM (TUTAR) FROM TBLVERI))*100 AS TarihOran
FROM TBLVERI AS VER