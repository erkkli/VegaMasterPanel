DECLARE @par1 nvarchar(20) = '{TARIH1}';

DECLARE @par2 nvarchar(20) = '{TARIH2}';

;

WITH base AS
  (SELECT UserName,
          ProductName,
          PaymentTime,
          Quantity,
          LTRIM(RTRIM(split.s)) AS Opt,
          ProductId
   FROM dbo.PaidBill CROSS APPLY dbo.SplitString(',', PaidBill.Options) AS SPLIT
   WHERE ISNULL(OPTIONS, '')<>''
     AND PaymentTime>=@par1
     AND PaymentTime <= @par2 ),
     base_with_optqty AS
  (SELECT base.UserName,
          base.ProductName,
          base.PaymentTime,
          base.Quantity,
          ProductId,
          CASE
              WHEN CHARINDEX('x', base.Opt) > 0 THEN SUBSTRING(base.Opt, 1, CHARINDEX('x', base.Opt)-1)
              ELSE '1'
          END AS OptQty,
          CASE
              WHEN CHARINDEX('x', base.Opt) > 0 THEN SUBSTRING(base.Opt, CHARINDEX('x', base.Opt)+1, 100)
              ELSE base.Opt
          END AS Opt
   FROM base),
     satislar AS
  (SELECT Quantity * (CASE
                          WHEN ISNUMERIC(OptQty) = 1 THEN CAST(OptQty AS INT)
                          ELSE 1
                      END) AS Quantity,
          Opt AS ProductName,
          bwo.UserName
   FROM base_with_optqty bwo)
SELECT a.*
FROM
  (SELECT UserName,
          ProductName + ' ' + '(***Seçenek Menü Ýçeriði***)' AS ProductName,
          sum(Quantity) AS Miktar,
          0 AS Tutar,
          0 AS BirimFiyat
   FROM satislar
   GROUP BY satislar.ProductName,
            satislar.UserName
   UNION SELECT t.UserName,
                t.ProductName,
                sum(t.toplam) AS Miktar,
                sum(t.tutar) AS Tutar,
                AVG(t.BirimFiyat) AS BirimFiyat
   FROM
     (SELECT P.Id AS ProductId,
             SUM(B.Quantity) AS Toplam,
             B.ProductName,
             B.UserName,
             SUM(ISNULL(B.Quantity, 0) * ISNULL(B.Price, 0)) AS Tutar,
             AVG(ISNULL(B.Price, 0)) BirimFiyat
      FROM dbo.Bill AS B
      LEFT JOIN dbo.Product AS P ON P.Id=B.ProductId
      WHERE [Date]>=@par1
        AND [Date]<=@par2
      GROUP BY B.UserName,
               B.ProductName,
               B.HeaderId,
               p.Id) AS t
   GROUP BY t.UserName,
            t.ProductName,
            t.ProductId) AS a
WHERE a.ProductName='{productGroup}'
ORDER BY a.ProductName