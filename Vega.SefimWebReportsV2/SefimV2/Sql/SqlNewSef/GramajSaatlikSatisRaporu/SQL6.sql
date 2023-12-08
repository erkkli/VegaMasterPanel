SET language english
DECLARE @TRH1 AS DATETIME='{TARIH1}'
DECLARE @TRH2 AS DATETIME='{TARIH2}'
DECLARE @KOD1 NVARCHAR(100)='{KOD1}'
DECLARE @KOD2 AS NVARCHAR(100)='{KOD2}'
DECLARE @URUNADI AS NVARCHAR(100)='{URUNADI}'
DECLARE @TEKTRH AS NVARCHAR(100)='{TEKTRH}'
DECLARE @GUN AS NVARCHAR(100)='{GUN}'
DECLARE @SAAT AS NVARCHAR(100)='{SAAT}'
DECLARE @SUBEADI AS NVARCHAR(100)='{SUBEADI}'


SELECT 
SUBEADI,
SUM(Quantity) as Miktar,
AGIRLIK,
sum(Quantity*AGIRLIK) AS Kg,
SUM(Quantity*Price) as Tutar,
(SELECT SUM(Quantity) FROM [dbo].[Bill] WHERE Date>=@TRH1  AND Date<=@TRH2 AND KOD1 LIKE '%DONER%' #KOD1WHERE# #KOD2WHERE# #URUNADIWHERE# #TEKTRHWHERE# #GUNWHERE# #SAATWHERE# #SUBEADIWHERE#) AS ToplamSat�s,
(SELECT SUM(Price*Quantity) FROM [dbo].[Bill] WHERE Date>=@TRH1  AND Date<=@TRH2 AND KOD1 LIKE '%DONER%' #KOD1WHERE# #KOD2WHERE# #URUNADIWHERE# #TEKTRHWHERE# #GUNWHERE# #SAATWHERE# #SUBEADIWHERE#) AS ToplamTutar

 FROM [dbo].[Bill]
WHERE Date>=@TRH1  AND Date<=@TRH2 #KOD1WHERE# #KOD2WHERE# #URUNADIWHERE# #TEKTRHWHERE# #GUNWHERE# #SAATWHERE# #SUBEADIWHERE#
AND KOD1 LIKE '%DONER%'
group by
AGIRLIK,
SUBEADI