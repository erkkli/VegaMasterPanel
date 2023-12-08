      
declare @par1 nvarchar(20) = '{TARIH1}';
declare @par2 nvarchar(20) = '{TARIH2}';

--SELECT 
--UserName AS PERKODU,sum(Quantity*Price) TUTAR , SUM(Quantity) AS MIKTAR
--FROM Bill
--WHERE Date>@par1 and Date<=@par2
--GROUP BY UserName

SELECT 

(select COUNT(hh) from (
SELECT bb.HeaderId hh
FROM Bill bb
WHERE bb.Date>@par1 and bb.Date<=@par2 and bb.UserName=b.UserName
group by bb.HeaderId ) a) islemsayisi,

UserName AS PERKODU,sum(Quantity*Price) TUTAR , SUM(Quantity) AS MIKTAR
FROM Bill b
WHERE Date>@par1 and Date<=@par2
GROUP BY UserName
