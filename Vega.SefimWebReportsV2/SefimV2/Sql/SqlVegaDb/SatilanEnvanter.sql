
SELECT ProductName, SUM(Quantity) AS MIKTAR, SUM(Price*Quantity)  AS TUTAR FROM Bill WHERE [Date]>='{TARIH1}' AND [Date]<='{TARIH2}' AND ProductName NOT LIKE '$%' 
GROUP BY ProductName
ORDER BY ProductName ASC





--SELECT EnvanterliStok.IND AS STOKIND, EnvanterliStok.STOKKODU, EnvanterliStok.MALINCINSI, EnvanterliStok.KOD7 AS MARKA, EnvanterliStok.KOD5 AS KOD5,
--EnvanterliStok.KOD1 AS KISAACIKLAMA, EnvanterliStok.GARANTI, BIRIMLER.SATISFIYATI1, BIRIMLER.SATISFIYATI2, BIRIMLER.SATISFIYATI3, BIRIMLER.PB1,
--BIRIMLER.PB2, BIRIMLER.PB3, BIRIMLER.KDV, BIRIMLER.BARCODE, EnvanterliStok.KARTINACILMATARIHI, EnvanterliStok.Envanter 
--FROM (SELECT F0100TBLSTOKLAR.*, MYSTOKLAR.Envanter 
--FROM (SELECT STOKLAR.IND AS STOKIND, SUM(Isnull(DEPOENVANTER.ENVANTER,'0')) AS Envanter 
--FROM F0100TBLSTOKLAR AS STOKLAR
--LEFT JOIN F0100D0002TBLDEPOENVANTER AS DEPOENVANTER ON STOKLAR.IND = DEPOENVANTER.STOKNO
--WHERE STATUS=1 AND DEPOENVANTER.DEPO='{DEPOID}' group by STOKLAR.IND) AS MYSTOKLAR
--LEFT JOIN F0100TBLSTOKLAR ON F0100TBLSTOKLAR.IND = MYSTOKLAR.STOKIND) AS EnvanterliStok 
--INNER JOIN F0100TBLBIRIMLEREX AS BIRIMLER ON EnvanterliStok.IND = BIRIMLER.STOKNO 
--WHERE STATUS=1 AND EnvanterliStok.ANABIRIM = BIRIMLER.IND AND EnvanterliStok.KOD5='SEFIMWEBREPORT'
--order by MALINCINSI ASC