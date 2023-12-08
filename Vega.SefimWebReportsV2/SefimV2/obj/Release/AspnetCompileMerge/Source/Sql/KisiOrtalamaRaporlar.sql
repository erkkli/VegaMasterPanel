declare @par1 nvarchar(20) = '{TARIH1}';
declare @par2 nvarchar(20) ='{TARIH2}';


select sum(t.persons) kisi, sum(total) total,sum(total)/ case when sum(ISNULL(t.persons,0))=0 then 1 else sum(t.persons) end as ortalama from (


SELECT Persons,HeaderId,sum(Quantity*Price) total
  FROM BillWithHeader
  where ISNULL(Persons,'') <>''  AND Date>=@par1 AND 
Date<=@par2
  group by Persons,HeaderId ) as t 