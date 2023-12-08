declare @par1 nvarchar(20) = '{TARIH1}';
declare @par2 nvarchar(20) = '{TARIH2}';



--select TableNumber, COUNT(TableNumber) as sayi
--from BillWithHeader   where BillState=0 AND ISNULL(BillType,0) IN (0,1) AND Date>=@par1 AND 
--Date<=@par2
--group by TableNumber 
--having Count (TableNumber) > 0



--select count(*)as TOPLAM_MASA ,sum(quantity*price) TOPLAM_TUTAR from BillWithHeader where BillState=0 AND ISNULL(BillType,0) IN (0,1) AND Date>=@par1 AND 
--Date<=@par2




select TableNumber, count(*) as ProductCount,sum(quantity*price) TOPLAM_TUTAR from BillWithHeader where BillState=0 AND ISNULL(BillType,0) IN (0,1)  and  [Date]>=@par1 AND [Date]<=@par2

group by TableNumber

