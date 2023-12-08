      
declare @par1 nvarchar(20) = '{TARIH1}';
declare @par2 nvarchar(20) = '{TARIH2}';

SELECT 
Top 1
UserName,sum(Quantity*Price) Total
FROM Bill
WHERE Date>@par1 and Date<=@par2
GROUP BY UserName
order by Total desc