DECLARE @par1 nvarchar(20) = '{TARIH1}';

DECLARE @par2 nvarchar(20) = '{TARIH2}';

;

WITH mbom AS
  (SELECT top 1 ProductName,
              Cost CMALIYET,
              BomCost RMALIYET
   FROM [Cost]
   WHERE CostEnd<@par2
   ORDER BY CostStart DESC),
     base AS
  (SELECT ProductName,
          PaymentTime,
          Quantity,
          LTRIM(RTRIM(split.s)) AS Opt,
          ProductId
   FROM dbo.PaidBill CROSS APPLY dbo.SplitString(',', PaidBill.Options) AS SPLIT
   WHERE ISNULL(OPTIONS, '')<>''
     AND PaymentTime>=@par1
     AND PaymentTime <= @par2 ),
     base_with_optqty AS
  (SELECT base.ProductName,
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
  (SELECT m.RMALIYET,
          m.CMALIYET,
          Quantity * (CASE
                          WHEN ISNUMERIC(OptQty) = 1 THEN CAST(OptQty AS INT)
                          ELSE 1
                      END) AS Quantity,
          Opt AS ProductName
   FROM base_with_optqty bwo
   LEFT JOIN mbom m ON m.ProductName=bwo.ProductName)
SELECT a.*
FROM
  (SELECT ProductName + ' ' + '(***Seçenek Menü Ýçeriði***)' AS ProductName,
          sum(Quantity) AS Miktar,
          0 AS Tutar,
          0 AS BirimFiyat,
          ISNULL(RMALIYET, 0) AS ReceteMaliyet,
          ISNULL(CMALIYET, 0) AS CostMaliyet,
          0 AS ReceteKar,
          0 AS CostKar
   FROM satislar
   GROUP BY satislar.ProductName,
            satislar.CMALIYET,
            satislar.RMALIYET
   UNION SELECT t.ProductName,
                sum(t.toplam) AS Miktar,
                sum(t.tutar) AS Tutar,
                AVG(t.BirimFiyat) AS BirimFiyat,
                ISNULL(t.RECETEMALIYET, 0) AS ReceteMaliyet,
                ISNULL(t.COSTMALIYET, 0) AS CostMaliyet,
                sum(t.tutar) - ISNULL(t.RECETEMALIYET, 0) AS ReceteKar,
                sum(t.tutar) - ISNULL(t.COSTMALIYET, 0) AS CostKar
   FROM
     (SELECT m.RMALIYET AS RECETEMALIYET,
             m.CMALIYET AS COSTMALIYET,
             P.Id AS ProductId,
             SUM(B.Quantity) AS Toplam,
             B.ProductName,
             SUM(ISNULL(B.Quantity, 0) * ISNULL(B.Price, 0)) AS Tutar,
             AVG(ISNULL(B.Price, 0)) BirimFiyat
      FROM dbo.Bill AS B
      LEFT JOIN dbo.Product AS P ON P.Id=B.ProductId
      LEFT JOIN mbom AS m ON M.ProductName=B.ProductName
      WHERE [Date]>=@par1
        AND [Date]<=@par2
      GROUP BY B.ProductName,
               B.HeaderId,
               p.Id,
               m.RMALIYET,
               m.CMALIYET) AS t
   GROUP BY t.ProductName,
            t.ProductId,
            t.RECETEMALIYET,
            t.COSTMALIYET) AS a
ORDER BY a.ProductName