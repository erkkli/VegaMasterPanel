DECLARE @TRH1 AS DATETIME='{TARIH1}'
DECLARE @TRH2 AS DATETIME='{TARIH2}'
DECLARE @SUBEADI AS NVARCHAR(100)='{SUBEADI}'

SELECT 
CONVERT(nvarchar(102),date,102) as Tarih,
ProductName AS ÜrünAdi,
SUM(Quantity) as Miktar,
AGIRLIK as Agirlik
 FROM [dbo].[Bill]
WHERE Date>=@TRH1  AND Date<=@TRH2 #TEKTRHWHERE# #SUBEADIWHERE#
AND KOD3 LIKE '%KG%'
group by
CONVERT(nvarchar(102),date,102),
AGIRLIK,
ProductName
order by ProductName