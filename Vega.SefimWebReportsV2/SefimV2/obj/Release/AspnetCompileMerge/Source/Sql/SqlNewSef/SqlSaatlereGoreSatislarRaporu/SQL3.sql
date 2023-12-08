DECLARE @TRH1 AS DATETIME='{TARIH1}'
DECLARE @TRH2 AS DATETIME='{TARIH2}'
SELECT 
SUBEADI,
case WHEN DATEPART(HOUR, [date])=0 THEN 24 else  DATEPART(HOUR, [date]) end  as Saat,
SUM(Quantity*Price) as Tutar
 FROM [dbo].[Bill]
WHERE Date>=@TRH1  AND Date<=@TRH2 #BELİRLİSAATLER# #TEKTRHWHERE#
group by
SUBEADI,
DATEPART(HOUR, [date]) 
order by 
DATEPART(HOUR, [date]) 
