DECLARE @TRH1 AS DATETIME='{TARIH1}'
DECLARE @TRH2 AS DATETIME='{TARIH2}'
SELECT 
CONVERT(nvarchar(10),date,102) as Tarih,
count(DISTINCT(ProductName)) as [Ürün Sayisi] ,
SUM(Quantity) as Miktar,Sum(Quantity*Price) as Tutar
 FROM [dbo].[Bill]
WHERE Date>=@TRH1  AND Date<=@TRH2 #BELÝRLÝSAATLER# #TEKTRHWHERE#
group by
CONVERT(nvarchar(10),date,102) 