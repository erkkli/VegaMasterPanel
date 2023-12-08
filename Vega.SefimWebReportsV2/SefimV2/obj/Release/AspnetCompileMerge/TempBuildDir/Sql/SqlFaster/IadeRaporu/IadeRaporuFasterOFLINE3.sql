
declare @Sube nvarchar(100) = '{SUBE}';
declare @Trh1 nvarchar(20) = '{TARIH1}';
declare @Trh2 nvarchar(20) = '{TARIH2}';


    SELECT @Sube AS Sube,

   (SELECT TOP 1 SUBEADI
      FROM TBLFASTERKASALAR
      WHERE SUBENO=FO.SUBEIND) AS Sube1,

     (SELECT KASAADI
      FROM TBLFASTERKASALAR
      WHERE KASANO=FO.KASAIND) AS Kasa,

	  FO.IND Id, TARIH Date,USR.USERNAME UserName,'' ProductName,0 ProductId,0 Choice1Id,0 Choice2Id,'' Options,ODENEN Price,0 Quantity,'' Comment,0 HeaderId,0 OrderId

   FROM DBO.TBLFASTERODEMELER FO
   LEFT JOIN DBO.TBLFASTERUSERS USR ON FO.USERIND=USR.USERNO
   WHERE ISNULL(FO.IADE, 0) = 1
     AND ODEMETIPI IN (0,
                       1,
                       2,
                       4)
     AND ISLEMTARIHI >= @Trh1
     AND ISLEMTARIHI <= @Trh2