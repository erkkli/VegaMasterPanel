declare @Sube nvarchar(100) = '{SUBEADI}';
declare @Trh1 nvarchar(20) = '01.01.2020';
declare @Trh2 nvarchar(20) = '01.08.2020';

select 
Sube,Sube1,Kasa_No,sUM(Nakit) AS Cash,Sum(Visa) as Credit,Sum(Ticket) as Ticket,Sum(Debit) AS Debit, Sum(Tableno) as TableNo, Sum(Discount) as Discount,Sum(Ikram) as ikram,Sum(Zayi) as Zayi,
SUm(case when t.Iptal=1 then t.Toplam else 0 end ) as iptal,
SUm(case when t.Iptal=0 then t.Toplam else 0 end ) as ToplamCiro

 from (
SELECT 

(SELECT top 1  OZELKOD1 FROM F0107TBLPOSKASATANIM WHERE KASANO=Hr.Kasa_No) AS Sube,
(SELECT top 1 OZELKOD1 FROM F0107TBLPOSKASATANIM WHERE KASANO=Hr.Kasa_No) AS Sube1,
hr.Kasa_no,
ISNULL((SELECT SUM(Tutar) from VEGABOS_YENITEST..Odeme WHERE Tus_no=0 and belge_Id=Hr.belge_Id),0) AS Nakit,
ISNULL((SELECT SUM(Tutar) from VEGABOS_YENITEST..Odeme WHERE Tus_no IN (1,2,3,4) and belge_Id=Hr.belge_Id),0) AS Visa,
0 AS Ticket,
ISNULL((SELECT SUM(Tutar) from VEGABOS_YENITEST..Odeme WHERE Tus_no IN (5,6) and belge_Id=Hr.belge_Id),0) AS Debit,
0 AS Ikram
,0 as Tableno
,SUM(DISCOUNTTOTAL) as Discount
,Iptal
,0 as Zayi,
SUm(hr.Toplam) as Toplam


FROM VEGABOS_YENITEST..BELGE  as hr 
where hr.BELGETARIH>=@Trh1 AND hr.BELGETARIH<=@Trh2
GROUP BY Sicil_No,Kasa_No,hr.Belge_ID,Iptal) as t

group by
Sube,Sube1,Kasa_No 

