DECLARE @TRH1 AS DATETIME='{TARIH1}'
DECLARE @TRH2 AS DATETIME='{TARIH2}'
DECLARE @KOD1 NVARCHAR(100)='{KOD1}'
DECLARE @URUNADI AS NVARCHAR(100)='{URUNADI}'
DECLARE @SUBEADI AS NVARCHAR(100)='{SUBEADI}'


SELECT *,
(t.AdetMiktar/t.ToplamMiktar)*100 as [Toplam%MiktarAdet],
(t.KilolukMiktar/t.ToplamMiktar)*100 as [Toplam%MiktarKiloluk]
FROM (
SELECT 
sum(case when  ISNULL(KOD3,'')<>'KG' and KOD1 LIKE '%DONER%' THEN (Quantity*AGIRLIK)*10 ELSE 0 END) As AdetMiktar,
sum(case when  ISNULL(KOD3,'')='KG' and KOD1 LIKE '%DONER%' THEN (Quantity*AGIRLIK)*10 ELSE 0 END) As KilolukMiktar,
SUM(Quantity*AGIRLIK)*10 ToplamMiktar,
SUM(Quantity*Price) AS ToplamTutar,
sum(case when  ISNULL(KOD3,'')<>'KG' THEN Quantity*Price ELSE 0 END) As AdetTutar,
sum(case when  ISNULL(KOD3,'')='KG' THEN Quantity*Price ELSE 0 END) As KgTutar




 FROM [dbo].[Bill] AS HR
WHERE Date>=@TRH1  AND Date<=@TRH2 and KOD1 LIKE '%DONER%'  #KOD1WHERE# #URUNADIWHERE# #TEKTRHWHERE# #SUBEADIWHERE# #BELÝRLÝSAATLER# #PORSIYONTIPIWHERE#



) AS T 
