declare @par1 nvarchar(20) = '{TARIH1}';
declare @par2 nvarchar(20) = '{TARIH2}';
declare @Sube nvarchar(100) = '{SUBEADI}';

select 
sum(Gelir) Gelir,
sum(Gider) Gider
from (
select 
'Gelir' AS ISLEM,0 Gider, sum(Total) Gelir from DirectTransaction where [Date]>=@par1 AND [Date]<=@par2
and ISNULL(Total,0)>0

UNION select 
'Gider' AS ISLEM,sum(Total) Gelir, 0 Gider
from DirectTransaction where [Date]>=@par1 AND [Date]<=@par2
and ISNULL(Total,0)<0 ) t