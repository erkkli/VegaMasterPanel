DECLARE @TRH1 AS DATETIME='{TARIH1}'
DECLARE @TRH2 AS DATETIME='{TARIH2}'
DECLARE @KOD1 NVARCHAR(100)='{KOD1}'
DECLARE @URUNADI AS NVARCHAR(100)='{URUNADI}'
DECLARE @SUBEADI AS NVARCHAR(100)='{SUBEADI}'

SELECT 
ProductName,
SUM(Quantity) as AdetMiktar,
sum(Quantity*Price) AS TUTAR
FROM [dbo].[Bill] AS HR WHERE Date>=@TRH1  AND Date<=@TRH2 and KOD1 LIKE '%DONER%'  #KOD1WHERE# #URUNADIWHERE# #TEKTRHWHERE# #SUBEADIWHERE# #BELİRLİSAATLER# #PORSIYONTIPIWHERE#
group by
ProductName
