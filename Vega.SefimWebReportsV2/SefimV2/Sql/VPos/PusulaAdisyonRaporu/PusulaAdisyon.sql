      	
declare @par1 nvarchar(20) = '{TARIH1}';
declare @par2 nvarchar(20) = '{TARIH2}';
declare @Sube nvarchar(100) = '{SUBEADI}';
declare @Sube2 nvarchar(100) = '{SUBE2}';
declare @Kasa nvarchar(100) = '{KASAKODU}';


	SELECT 
	B.IND Id,
	B.BELGETIPI BillType,
	B.BELGENO TableNumber,
	(SELECT SUM(TUTAR) From TBLSPOSSATISBASLIK WITH(NOLOCK)  WHERE IND=B.IND) AS Total,
	(SELECT TOP 1 ISNULL(KODU,'')+'/'+ISNULL(ADI,'') From TBLSPOSKULLANICILAR WITH(NOLOCK)  WHERE IND=B.IND ORDER BY OLUSTURMATARIHI ASC) AS FirstUserName,
	(SELECT TOP 1 ISNULL(KODU,'')+'/'+ISNULL(ADI,'') From TBLSPOSKULLANICILAR WITH(NOLOCK)  WHERE IND=B.IND ORDER BY OLUSTURMATARIHI DESC) AS LastUserName,
	(SELECT MIN(OLUSTURMATARIHI) From TBLSPOSSATISBASLIK WITH(NOLOCK)  WHERE IND=B.IND) AS FirstOrderTime,
	(SELECT MAX(OLUSTURMATARIHI) From TBLSPOSSATISBASLIK WITH(NOLOCK)  WHERE IND=B.IND ) AS LastOrderTime,
	(SELECT TOP 1 ISNULL(KODU,'')+'/'+ISNULL(ADI,'') From TBLSPOSKULLANICILAR WITH(NOLOCK)  WHERE IND=B.KULLANICIIND)  AS Cashier,
	(SELECT TOP 1 ISNULL(KODU,'')+'/'+ISNULL(ADI,'') From TBLSPOSCARILER WITH(NOLOCK)  WHERE IND=B.CARIIND) AS CustomerName
	FROM TBLSPOSSATISBASLIK B WITH(NOLOCK) 
	WHERE  B.IADE=0 AND B.IPTAL=0 and B.BEKLEYENFIS=0 AND ISNULL(B.P_KAYNAK,'')='' AND B.OLUSTURMATARIHI>=@par1 AND B.OLUSTURMATARIHI<=@par2  
	AND SUBEIND IN (SELECT IND FROM TBLSPOSSUBELER  WITH(NOLOCK) WHERE ISNULL(IsDeleted,0)=0 AND KODU=@Sube2)   
AND KASAIND IN (SELECT IND FROM TBLSPOSKASALAR  WITH(NOLOCK) WHERE ISNULL(IsDeleted,0)=0 AND Charindex('&'+KODU+'&',@kasa)>0)