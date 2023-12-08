declare @par1 nvarchar(20) = '{TARIH1}';
declare @par2 nvarchar(20) = '{TARIH2}';
declare @Sube nvarchar(100) = '{SUBEADI}';


select 
'Gelir' AS IslemTipi,0 Gider, (Total) Gelir,
Id,Date,Description,UserName,CustomerName from DirectTransaction where [Date]>=@par1 AND [Date]<=@par2
and ISNULL(Total,0)>0

UNION select 
'Gider' AS IslemTipi,(Total) Gelir, 0 Gider,
Id,Date,Description,UserName,CustomerName
from DirectTransaction where [Date]>=@par1 AND [Date]<=@par2
and ISNULL(Total,0)<0



