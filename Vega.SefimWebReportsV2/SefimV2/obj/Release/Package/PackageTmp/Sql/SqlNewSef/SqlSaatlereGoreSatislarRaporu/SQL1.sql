DECLARE @TRH1 AS DATETIME='{TARIH1}'
DECLARE @TRH2 AS DATETIME='{TARIH2}'
SELECT 
CONVERT(nvarchar(10),date,102) as Tarih,
count(DISTINCT(ProductName)) as [�r�n Sayisi] ,
SUM(Quantity) as Miktar,Sum(Quantity*Price) as Tutar
 FROM [dbo].[Bill]
WHERE Date>=@TRH1  AND Date<=@TRH2 #BEL�RL�SAATLER# #TEKTRHWHERE#
group by
CONVERT(nvarchar(10),date,102) 