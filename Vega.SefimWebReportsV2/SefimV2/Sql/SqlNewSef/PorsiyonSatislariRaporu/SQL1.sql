DECLARE @TRH1 AS DATETIME='{TARIH1}'
DECLARE @TRH2 AS DATETIME='{TARIH2}'
DECLARE @KOD1 NVARCHAR(100)='{KOD1}'
DECLARE @URUNADI AS NVARCHAR(100)='{URUNADI}'
DECLARE @SUBEADI AS NVARCHAR(100)='{SUBEADI}'


SELECT *,(t.Tutar/t.ToplamCiro)*100 as CiroOranOran,
(t.SubeToplamMiktar/t.ToplamMiktar)*100 as [Toplam%Miktar]
FROM (
SELECT 
SUBEADI,
sum(case when  ISNULL(KOD3,'')<>'KG' and KOD1 LIKE '%DONER%' THEN (Quantity*AGIRLIK)*10 ELSE 0 END) As AdetMiktar,
sum(case when  ISNULL(KOD3,'')='KG' and KOD1 LIKE '%DONER%' THEN (Quantity*AGIRLIK)*10 ELSE 0 END) As KilolukMiktar,
SUM(case when KOD1 LIKE '%DONER%' THEN (Quantity*Price)ELSE 0 END) as Tutar,
(SELECT SUM(Quantity*Price) FROM  [dbo].[Bill] where  Date>=@TRH1  AND Date<=@TRH2 AND SUBEADI=hR.SUBEADI #KOD1WHERE# #URUNADIWHERE# #TEKTRHWHERE# #SUBEADIWHERE# #BEL�RL�SAATLER# #PORSIYONTIPIWHERE#) as ToplamCiro,
(SELECT SUM(Quantity*AGIRLIK)*10 FROM  [dbo].[Bill] where  Date>=@TRH1  AND Date<=@TRH2 AND SUBEADI=hR.SUBEADI and KOD1 LIKE '%DONER%' #KOD1WHERE# #URUNADIWHERE# #TEKTRHWHERE# #SUBEADIWHERE# #BEL�RL�SAATLER# #PORSIYONTIPIWHERE#) as SubeToplamMiktar,
(SELECT SUM(Quantity*AGIRLIK)*10 FROM  [dbo].[Bill] where  Date>=@TRH1  AND Date<=@TRH2 AND  KOD1 LIKE '%DONER%' #KOD1WHERE# #URUNADIWHERE# #TEKTRHWHERE# #SUBEADIWHERE# #BEL�RL�SAATLER# #PORSIYONTIPIWHERE#) as ToplamMiktar

 FROM [dbo].[Bill] AS HR
WHERE Date>=@TRH1  AND Date<=@TRH2  #KOD1WHERE# #URUNADIWHERE# #TEKTRHWHERE# #SUBEADIWHERE# #BEL�RL�SAATLER# #PORSIYONTIPIWHERE#

group by
SUBEADI
) AS T 