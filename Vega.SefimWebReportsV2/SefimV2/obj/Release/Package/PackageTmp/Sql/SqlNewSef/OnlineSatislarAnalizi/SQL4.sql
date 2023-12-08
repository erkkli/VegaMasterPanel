
DECLARE @TRH1 AS DATETIME='{TARIH1}'
DECLARE @TRH2 AS DATETIME='{TARIH2}'
DECLARE @SUBEADI AS NVARCHAR(100)='{SUBEADI}'
DECLARE @KANAL AS NVARCHAR(100)='{KANAL}'
DECLARE @URUNADI AS NVARCHAR(100)='{URUNADI}'
DECLARE @ODEMETIPI AS NVARCHAR(100)='{ODEMETIPI}'

Select 
t.OdemeYemekCeki as Odemetipi,t.Fisayisi,t.Miktar,t.Tutar,t.Tutar/t.Fisayisi as SepetOrtalmasi,t.ToplamTUtar,
(T.Tutar/T.ToplamTUtar)*100 AS Oran,t.SubeSayiyi,t.ÜrünSayiyi

from (
SELECT 
count(DISTINCT(HeaderId)) AS Fisayisi,
OdemeYemekCeki,
(SELECT COUNT(DISTINCT(SUBEADI)) FROM [dbo].[Bill] AS hr1 WHERE  hr1.OdemeYemekCeki=(hr.OdemeYemekCeki)   and hr1.Date>=@TRH1  AND hr1.Date<=@TRH2 #ODEMETIPIWHERE# #KANALWHERE# #SUBEADIWHERE# #URUNADIWHERE# ) as SubeSayiyi,
(SELECT COUNT(DISTINCT(ProductName)) FROM [dbo].[Bill] AS hr1 WHERE hr1.OdemeYemekCeki=(hr.OdemeYemekCeki)   and hr1.Date>=@TRH1  AND hr1.Date<=@TRH2 #ODEMETIPIWHERE# #KANALWHERE# #SUBEADIWHERE# #URUNADIWHERE# ) as ÜrünSayiyi,
sum(Quantity) as Miktar,
SUM(Price*Quantity) as Tutar,
(SELECT SUM(Quantity*Price) FROM [dbo].[Bill] AS hr1 WHERE  (OdemeYemekCeki)=(OdemeYemekCeki)   and hr1.Date>=@TRH1  AND hr1.Date<=@TRH2 #ODEMETIPIWHERE# #KANALWHERE# #SUBEADIWHERE# #URUNADIWHERE# ) as ToplamTUtar
 FROM [dbo].[Bill] AS hr WHERE Date>=@TRH1  AND Date<=@TRH2 #ODEMETIPIWHERE# #KANALWHERE# #SUBEADIWHERE# #URUNADIWHERE# #SATISTIPIWHERE# #OPSIYONTIPIWHERE#

group by

OdemeYemekCeki ) as t where t.OdemeYemekCeki is not null


