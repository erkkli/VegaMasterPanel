declare @par1 nvarchar(20) = '{TARIH1}';
declare @par2 nvarchar(20) = '{TARIH2}';
declare @Sube nvarchar(100) = '{SUBEADI}';
declare @Sube2 nvarchar(100) = '{SUBE2}';
declare @Kasa nvarchar(100) = '{KASAKODU}';

WITH
acikhesap as (

SELECT 
@Sube as Sube, 
0 AS Cash, 0 AS Credit, 0  AS Ticket, (TUTAR*KUR) AS Debit, 0 AS ikram, 0 AS TableNo, 0 AS Discount, 0 AS iptal,0 AS iade,
0 AS zayi , 0 PaketToplam
FROM TBLSPOSSATISTAHSILAT WITH(NOLOCK)
WHERE ISLEMTIPI=4 AND IADE=0 AND IPTAL=0 AND OLUSTURMATARIHI>=@par1 AND OLUSTURMATARIHI<=@par2
AND SUBEIND IN (SELECT IND FROM TBLSPOSSUBELER  WITH(NOLOCK) WHERE ISNULL(IsDeleted,0)=0 AND KODU=@Sube2)   
AND KASAIND IN (SELECT IND FROM TBLSPOSKASALAR  WITH(NOLOCK) WHERE ISNULL(IsDeleted,0)=0AND Charindex('&'+KODU+'&',@kasa)>0)  
UNION ALL
SELECT 
@Sube as Sube, 
0 AS Cash, 0 AS Credit, 0 AS Ticket, (TUTAR*KUR)*-1 AS Debit, 0 AS ikram, 0 AS TableNo, 0 AS Discount, 0 AS iptal,0 AS iade,
0 AS zayi , 0 PaketToplam
FROM TBLSPOSSATISTAHSILAT WITH(NOLOCK)
WHERE ISLEMTIPI=4 AND IADE=1 AND IPTAL=0 AND OLUSTURMATARIHI>=@par1 AND OLUSTURMATARIHI<=@par2
AND SUBEIND IN (SELECT IND FROM TBLSPOSSUBELER  WITH(NOLOCK) WHERE ISNULL(IsDeleted,0)=0 AND KODU=@Sube2)   
AND KASAIND IN (SELECT IND FROM TBLSPOSKASALAR  WITH(NOLOCK) WHERE ISNULL(IsDeleted,0)=0AND Charindex('&'+KODU+'&',@kasa)>0)  
 ),

acikhesaptahsil as (

SELECT 
@Sube as Sube, 
0 AS Cash, 0 AS Credit, 0  AS Ticket, (TUTAR*KUR) AS Debit, 0 AS ikram, 0 AS TableNo, 0 AS Discount, 0 AS iptal,0 AS iade,
0 AS zayi , 0 PaketToplam
FROM TBLSPOSTAHSILATHAREKET H  WITH(NOLOCK) 
LEFT JOIN TBLSPOSTAHSILATBASLIK B WITH(NOLOCK)  ON B.IND=H.BASLIKIND
WHERE ISLEMTIPI=1  AND H.OLUSTURMATARIHI>=@par1 AND H.OLUSTURMATARIHI<=@par2
AND SUBEIND IN (SELECT IND FROM TBLSPOSSUBELER  WITH(NOLOCK) WHERE ISNULL(IsDeleted,0)=0 AND KODU=@Sube2)   
AND KASAIND IN (SELECT IND FROM TBLSPOSKASALAR  WITH(NOLOCK) WHERE ISNULL(IsDeleted,0)=0AND Charindex('&'+KODU+'&',@kasa)>0)  
 )


 SELECT 
COUNT(*) AS OrderCount,	

sum(acikhesaptahsil) Total,
sum(acikhesap) Debit
FROM (


SELECT  SUM(debit) acikhesap,0 as acikhesaptahsil   FROM acikhesap

UNION ALL SELECT  0 acikhesap,SUM(debit) as acikhesaptahsil   FROM acikhesaptahsil ) as t

