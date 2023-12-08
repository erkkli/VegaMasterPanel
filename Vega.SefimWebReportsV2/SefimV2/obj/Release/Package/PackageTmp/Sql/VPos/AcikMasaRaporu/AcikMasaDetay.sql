
  declare @par1 nvarchar(20) = '{TARIH1}';
  declare @par2 nvarchar(20) = '{TARIH2}';
  declare @Sube nvarchar(100) = '{SUBEADI}';
declare @Sube2 nvarchar(100) = '{SUBE2}';
declare @Kasa nvarchar(100) = '{KASAKODU}';


select  SATISBARKODU TableNumber,min(OLUSTURMATARIHI) as Date, COUNT(*) as sayi,sum(b.TUTAR) TUTAR  
FROM TBLSPOSSATISBASLIK B WITH(NOLOCK) 
WHERE  B.IADE=0 AND B.IPTAL=0 and B.BEKLEYENFIS=1 AND ISNULL(B.P_KAYNAK,'')='' AND B.OLUSTURMATARIHI>=@par1 AND B.OLUSTURMATARIHI<=@par2 
AND SUBEIND IN (SELECT IND FROM TBLSPOSSUBELER  WITH(NOLOCK) WHERE ISNULL(IsDeleted,0)=0 AND KODU=@Sube2)   
AND KASAIND IN (SELECT IND FROM TBLSPOSKASALAR  WITH(NOLOCK) WHERE ISNULL(IsDeleted,0)=0 AND Charindex('&'+KODU+'&',@kasa)>0)

group by SATISBARKODU
having Count (IND) > 0