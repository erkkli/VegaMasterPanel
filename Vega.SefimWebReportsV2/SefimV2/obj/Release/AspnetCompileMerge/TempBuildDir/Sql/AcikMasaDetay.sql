--exec subebazli '##AktifMasalar', '(SELECT TableNumber, Date, (Price*Quantity)  AS TUTAR FROM BillWithHeader 
--WHERE [Date]>=''{TARIH1}'' AND [Date]<=''{TARIH2}'' AND BillState=0 AND ISNULL(BillType,0) IN (0,1)) 
--','{SubeAdi}'; SELECT * from ( SELECT Toplamsatis.ServerName AS Sube ,
--Toplamsatis.TableNumber ,min(Toplamsatis.Date) AS TarihMin ,max(Toplamsatis.Date) AS TarihMax ,
--sum(Toplamsatis.TUTAR) AS TUTAR FROM ##AktifMasalar AS Toplamsatis GROUP BY Toplamsatis.ServerName,Toplamsatis.TableNumber )
--As Temp WHERE TEMP.Sube not in ('local') Order by Temp.TarihMin Desc 

--SELECT TableNumber, Date, (Price*Quantity) AS TUTAR  FROM BillWithHeader 
--WHERE [Date]>='{TARIH1}' AND [Date]<='{TARIH2}' AND BillState=0 AND ISNULL(BillType,0) IN (0,1)

--order by TableNumber
  declare @par1 nvarchar(20) = '{TARIH1}';
  declare @par2 nvarchar(20) = '{TARIH2}';

--select TableNumber, COUNT(TableNumber) as sayi, sum(quantity*price)  as TUTAR
--from BillWithHeader   where BillState=0 AND ISNULL(BillType,0) IN (0,1) AND Date>=@par1 AND 
--Date<=@par2
--group by TableNumber
--having Count (TableNumber) > 0

select TableNumber,min(Date) as Date, COUNT(TableNumber) as sayi, sum(quantity*price)  as TUTAR   
from BillWithHeader   where BillState=0 AND ISNULL(BillType,0) IN (0,1) AND Date>=@par1 AND 
Date<=@par2
group by TableNumber
having Count (TableNumber) > 0
