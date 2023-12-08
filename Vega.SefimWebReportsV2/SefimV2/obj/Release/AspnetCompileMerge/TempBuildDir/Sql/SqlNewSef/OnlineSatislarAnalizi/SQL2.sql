
DECLARE @TRH1 AS DATETIME='{TARIH1}'
DECLARE @TRH2 AS DATETIME='{TARIH2}'
DECLARE @SUBEADI AS NVARCHAR(100)='{SUBEADI}'
DECLARE @KANAL AS NVARCHAR(100)='{KANAL}'
DECLARE @URUNADI AS NVARCHAR(100)='{URUNADI}'
DECLARE @ODEMETIPI AS NVARCHAR(100)='{ODEMETIPI}'

Select 
t.TIP,t.Fisayisi,t.Miktar,t.Tutar,t.Tutar/t.Fisayisi as SepetOrtalmasi,T.SubeSayiyi,T.ÜrünSayiyi
from (
SELECT 

count(DISTINCT(HeaderId)) AS Fisayisi,
CASE WHEN ISNULL(hr.Options,'')='' THEN 'Direk Satis' ELSE 'Opsyion Satis' END TIP,
sum(Quantity) as Miktar,
SUM(Price*Quantity) as Tutar,
(SELECT COUNT(DISTINCT(SUBEADI)) FROM [dbo].[Bill] AS hr1 WHERE  (CASE WHEN ISNULL(hr1.Options,'')='' THEN 'Direk Satis' ELSE 'Opsyion Satis' END)=(CASE WHEN ISNULL(hr.Options,'')='' THEN 'Direk Satis' ELSE 'Opsyion Satis' END)   and hr1.Date>=@TRH1  AND hr1.Date<=@TRH2 #ODEMETIPIWHERE# #KANALWHERE# #SUBEADIWHERE# #URUNADIWHERE# ) as SubeSayiyi,
(SELECT COUNT(DISTINCT(ProductName)) FROM [dbo].[Bill] AS hr1 WHERE  (CASE WHEN ISNULL(hr1.Options,'')='' THEN 'Direk Satis' ELSE 'Opsyion Satis' END)=(CASE WHEN ISNULL(hr.Options,'')='' THEN 'Direk Satis' ELSE 'Opsyion Satis' END)   and hr1.Date>=@TRH1  AND hr1.Date<=@TRH2 #ODEMETIPIWHERE# #KANALWHERE# #SUBEADIWHERE# #URUNADIWHERE# ) as ÜrünSayiyi

 FROM [dbo].[Bill] AS hr WHERE Date>=@TRH1  AND Date<=@TRH2 #ODEMETIPIWHERE# #KANALWHERE# #SUBEADIWHERE# #URUNADIWHERE# #SATISTIPIWHERE# #OPSIYONTIPIWHERE#

group by

CASE WHEN ISNULL(hr.Options,'')='' THEN 'Direk Satis' ELSE 'Opsyion Satis' END ) as t