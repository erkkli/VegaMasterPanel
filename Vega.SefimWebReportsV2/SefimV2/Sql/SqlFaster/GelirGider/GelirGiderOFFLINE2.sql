
declare @par1 nvarchar(20) = '{TARIH1}';
declare @par2 nvarchar(20) = '{TARIH2}';

select 
   (SELECT TOP 1 SUBEADI
      FROM TBLFASTERKASALAR
      WHERE SUBENO=KI.SUBEIND) AS Sube1,

     (SELECT KASAADI
      FROM TBLFASTERKASALAR
      WHERE KASANO=KI.KASAIND) AS Kasa,

    (SELECT KASANO
	 FROM TBLFASTERKASALAR
	 WHERE KASANO=KI.KASAIND) AS ID,

ISNULL(GELIR,0) Gelir, ISNULL(GIDER,0) Gider,KI.TARIH AS Date ,ISNULL(KI.ACIKLAMA,'') ACIKLAMA,
USR.USERNAME from TBLFASTERKASAISLEMLERI KI 
 LEFT JOIN DBO.TBLFASTERUSERS USR ON KI.USERNO=USR.USERNO
where ISLEMTARIHI>=@par1 AND ISLEMTARIHI<=@par2
				 