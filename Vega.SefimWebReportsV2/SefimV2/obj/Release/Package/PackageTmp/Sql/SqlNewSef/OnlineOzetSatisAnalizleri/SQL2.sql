
DECLARE @TRH1 AS DATETIME='{TARIH1}'
DECLARE @TRH2 AS DATETIME='{TARIH2}'
DECLARE @SUBEADI AS NVARCHAR(100)='{SUBEADI}'
DECLARE @KANAL AS NVARCHAR(100)='{KANAL}'


Select 
t.Tarih,t.Fisayisi,t.Miktar,t.Tutar,t.Tutar/t.Fisayisi as SepetOrtalmasi,t.ToplamTUtar,
(T.Tutar/T.ToplamTUtar)*100 AS Oran

from (
SELECT
CONVERT(nvarchar(10),Date,102) as Tarih,
count(DISTINCT(HeaderId)) AS Fisayisi,
sum(Quantity) as Miktar,
SUM(Price*Quantity) as Tutar,(SELECT SUM(Quantity*Price) FROM [dbo].[Bill] AS hr1 WHERE  LEN(hr1.PlatForm) >0  and hr1.Date>=@TRH1  AND hr1.Date<=@TRH2 AND hr1.OrderInd IS NOT NULL #TEKTRHWHERE# #SUBEADIWHERE# #KANALWHERE#) as ToplamTUtar
 FROM [dbo].[Bill] AS hr WHERE Date>=@TRH1  AND Date<=@TRH2 AND OrderInd IS NOT NULL #TEKTRHWHERE# #SUBEADIWHERE# #KANALWHERE#
AND LEN(hr.PlatForm) >0 
group by
CONVERT(nvarchar(10),Date,102)

) as t
