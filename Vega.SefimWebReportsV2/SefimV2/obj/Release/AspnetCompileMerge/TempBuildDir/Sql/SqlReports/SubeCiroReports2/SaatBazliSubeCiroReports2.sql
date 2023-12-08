DECLARE @Sube nvarchar(100) ='{SUBEADI}';
DECLARE @Tar1 nvarchar(20) ='{TARIH1}';
DECLARE @Tar2 nvarchar(20) ='{TARIH2}';
DECLARE @EndDate nvarchar(20) ='{EndDate}';
DECLARE @Saat nvarchar(20) ='{SAAT}';

select t.TARIH,t.SAAT,sum(t.TUTAR) TUTAR from (
SELECT 
case when CAST([Date] AS time)<'07:00:00' then CAST([Date]-1 AS date) else CAST([Date] AS date) end  TARIH,
--case when CAST([Date] AS time)<'07:00:00' then [Date]-1 else [Date] end TARIH,
LEFT(CONVERT(varchar,[Date],114),2)+':00'  as SAAT
,sum([Price]*[Quantity]) AS TUTAR
FROM [dbo].[BillWithHeader] 

where [Date]>= @Tar1  and [Date] <= @Tar2
Group by LEFT(CONVERT(varchar,[Date],114),2)+':00'
, [Date] ) as t
 --@Saat
group by 
t.TARIH,t.SAAT