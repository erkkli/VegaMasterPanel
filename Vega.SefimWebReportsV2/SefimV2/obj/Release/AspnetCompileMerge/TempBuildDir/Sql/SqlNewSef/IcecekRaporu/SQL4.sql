DECLARE @TRH1 AS DATETIME='{TARIH1}'
DECLARE @TRH2 AS DATETIME='{TARIH2}'
DECLARE @URUNGRUBU AS NVARCHAR(MAX)='{URUNGRUBU}'
DECLARE @SUBEADI AS NVARCHAR(100)='{SUBEADI}'
DECLARE @SATISTIPI AS NVARCHAR(100)='{SATISTIPI}'
DECLARE @URUNADI AS NVARCHAR(100)='{URUNADI}'

SELECT 
SUBEADI,
CASE WHEN ISNULL(Options,'')='' then 'Direk Sati�' else  'Opsiyon Sati�' END SatisTipi,
SUM(Quantity) as Miktar,NEWID() AS TEKIL
 FROM [dbo].[Bill] AS BAS
WHERE Date>=@TRH1  AND Date<=@TRH2 #URUNGRUBUWHERE# #SUBEADIWHERE# #SATISTIPIWHERE# #URUNADIWHERE#
AND KOD1 LIKE '%��ECEK%' 
group by
CASE WHEN ISNULL(Options,'')='' then 'Direk Sati�' else  'Opsiyon Sati�' END,
SUBEADI
UNION
SELECT 
SUBEADI,
'Opsiyon Sati�' as SatisTipi,
sum(MIKTAR*ANAMIKTAR),NEWID() AS TEKIL
 FROM [dbo].TBLBILLOPTIONS AS  HAR
 LEFT JOIN [Bill] AS BAS  ON BAS.Id=HAR.BIND
WHERE TARIH>=@TRH1  AND TARIH<=@TRH2 #URUNGRUBUWHERE2# #SUBEADIWHERE# #SATISTIPIWHERE# #URUNADIWHERE2#
AND VEGAKOD1 LIKE '%��ECEK%'
GROUP BY
SUBEADI