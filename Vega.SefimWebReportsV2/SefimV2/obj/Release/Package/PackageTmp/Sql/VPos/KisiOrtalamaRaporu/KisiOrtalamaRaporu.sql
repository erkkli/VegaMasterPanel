declare @par1 nvarchar(20) = '{TARIH1}';
declare @par2 nvarchar(20) ='{TARIH2}';
declare @Sube nvarchar(100) = '{SUBEADI}';
declare @Sube2 nvarchar(100) = '{SUBE2}';
declare @Kasa nvarchar(100) = '{KASAKODU}';
	
	
select sum(t.persons) kisi, sum(total) total,sum(total)/ case when sum(ISNULL(t.persons,0))=0 then 1 else sum(t.persons) end as ortalama from (

SELECT case when B.IADE=0 THEN 1 ELSE -1 END persons,IND,sum(b.TUTAR) total
FROM  TBLSPOSSATISBASLIK B WITH(NOLOCK) 
WHERE   ISNULL(B.BEKLEYENFIS,0)=0 AND ISNULL(B.P_KAYNAK,'')='' AND B.IPTAL=0 AND B.OLUSTURMATARIHI>=@par1 AND B.OLUSTURMATARIHI<=@par2
AND SUBEIND IN (SELECT IND FROM TBLSPOSSUBELER  WITH(NOLOCK) WHERE ISNULL(IsDeleted,0)=0 AND KODU=@Sube2)   
AND KASAIND IN (SELECT IND FROM TBLSPOSKASALAR  WITH(NOLOCK) WHERE ISNULL(IsDeleted,0)=0 AND Charindex('&'+KODU+'&',@kasa)>0)   

  group by IND,IADE ) as t 