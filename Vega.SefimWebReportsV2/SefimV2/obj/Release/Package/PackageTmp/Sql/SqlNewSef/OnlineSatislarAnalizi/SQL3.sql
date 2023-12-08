
DECLARE @TRH1 AS DATETIME='{TARIH1}'
DECLARE @TRH2 AS DATETIME='{TARIH2}'
DECLARE @SUBEADI AS NVARCHAR(100)='{SUBEADI}'
DECLARE @KANAL AS NVARCHAR(100)='{KANAL}'
DECLARE @URUNADI AS NVARCHAR(100)='{URUNADI}'
DECLARE @ODEMETIPI AS NVARCHAR(100)='{ODEMETIPI}'

Select 
t.Kanal,t.Fissayisi,t.Miktar,t.Tutar,t.Tutar/t.Fissayisi as SepetOrtalmasi,t.ToplamTUtar,
(T.Tutar/T.ToplamTUtar)*100 AS Oran,t.SubeSayiyi,t.ÜrünSayiyi

from (
SELECT 
count(DISTINCT(HeaderId)) AS Fissayisi,
ISNULL(hr.platform,'MAGAZA') as Kanal,
(SELECT COUNT(DISTINCT(SUBEADI)) FROM [dbo].[Bill] AS hr1 WHERE  (ISNULL(hr1.platform,'MAGAZA'))=(ISNULL(hr.platform,'MAGAZA'))   and hr1.Date>=@TRH1  AND hr1.Date<=@TRH2 AND hr1.OrderInd IS NOT NULL #ODEMETIPIWHERE# #KANALWHERE# #SUBEADIWHERE# #URUNADIWHERE#  ) as SubeSayiyi,
(SELECT COUNT(DISTINCT(ProductName)) FROM [dbo].[Bill] AS hr1 WHERE (ISNULL(hr1.platform,'MAGAZA'))=(ISNULL(hr.platform,'MAGAZA'))   and hr1.Date>=@TRH1  AND hr1.Date<=@TRH2 AND hr1.OrderInd IS NOT NULL #ODEMETIPIWHERE# #KANALWHERE# #SUBEADIWHERE# #URUNADIWHERE#  ) as ÜrünSayiyi,
sum(Quantity) as Miktar,
SUM(Price*Quantity) as Tutar,
(SELECT SUM(Quantity*Price) FROM [dbo].[Bill] AS hr1 WHERE  (ISNULL(hr.platform,'MAGAZA'))=(ISNULL(hr.platform,'MAGAZA'))   and hr1.Date>=@TRH1  AND hr1.Date<=@TRH2 AND hr1.OrderInd IS NOT NULL #ODEMETIPIWHERE# #KANALWHERE# #SUBEADIWHERE# #URUNADIWHERE#  ) as ToplamTUtar
 FROM [dbo].[Bill] AS hr WHERE Date>=@TRH1  AND Date<=@TRH2 AND OrderInd IS NOT NULL #ODEMETIPIWHERE# #KANALWHERE# #SUBEADIWHERE# #URUNADIWHERE# #SATISTIPIWHERE# #OPSIYONTIPIWHERE#

group by

ISNULL(hr.platform,'MAGAZA') ) as t