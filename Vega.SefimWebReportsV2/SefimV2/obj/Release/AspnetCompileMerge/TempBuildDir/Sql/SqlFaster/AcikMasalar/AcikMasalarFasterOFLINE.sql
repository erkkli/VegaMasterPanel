DECLARE @Sube nvarchar(100) = '{SubeAdi}';

DECLARE @par1 nvarchar(20) = '{TARIH1}';

DECLARE @par2 nvarchar(20) = '{TARIH2}';


SELECT @Sube AS Sube,

  (SELECT TOP 1 SUBEADI
   FROM TBLFASTERKASALAR
   WHERE SUBENO=TBLFASTERBEKLEYENBASLIK.SUBEIND) AS Sube1,

  (SELECT KASAADI
   FROM TBLFASTERKASALAR
   WHERE KASANO=TBLFASTERBEKLEYENBASLIK.KASAIND) AS Kasa,

  (SELECT IND
   FROM TBLFASTERKASALAR
   WHERE KASANO=TBLFASTERBEKLEYENBASLIK.KASAIND) AS ID,
       FIRMAKODU,
       count(*) AS TOPLAM_MASA,
       sum(TUTAR) TOPLAM_TUTAR
FROM TBLFASTERBEKLEYENBASLIK
WHERE ISLEMTARIHI>=@par1
  AND ISLEMTARIHI<=@par2
GROUP BY SUBEIND,
         KASAIND,
         FIRMAKODU ----select TableNumber, COUNT(TableNumber) as sayi
----from BillWithHeader   where BillState=0 AND ISNULL(BillType,0) IN (0,1) AND Date>=@par1 AND
----Date<=@par2
----group by TableNumber
----having Count (TableNumber) > 0
 ----select count(*)as TOPLAM_MASA ,sum(quantity*price) TOPLAM_TUTAR from BillWithHeader where BillState=0 AND ISNULL(BillType,0) IN (0,1) AND Date>=@par1 AND
----Date<=@par2
 --select TableNumber, count(*) as ProductCount,sum(quantity*price) TOPLAM_TUTAR from BillWithHeader where BillState=0 AND ISNULL(BillType,0) IN (0,1)  and  [Date]>=@par1 AND [Date]<=@par2
 --group by TableNumber
