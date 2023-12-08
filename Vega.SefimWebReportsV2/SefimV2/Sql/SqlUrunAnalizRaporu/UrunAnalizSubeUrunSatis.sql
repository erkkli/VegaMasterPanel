
		declare @par1 nvarchar(20) = '{TARIH1}';
		declare @par2 nvarchar(20) = '{TARIH2}';
	
		;WITH

		base as
		(
		SELECT	ProductName,PaymentTime,Quantity,LTRIM(RTRIM(split.s)) AS Opt,	ProductId
		FROM	dbo.PaidBill
		CROSS APPLY
			dbo.SplitString(',',PaidBill.Options)  as split
		WHERE
			ISNULL(Options,'')<>'' AND PaymentTime>=@par1 AND PaymentTime <= @par2	),

		base_with_optqty as 

		(
		SELECT 	base.ProductName,base.PaymentTime,	base.Quantity,	ProductId,
			CASE 
				WHEN CHARINDEX('x',base.Opt) > 0 THEN
				SUBSTRING(base.Opt,1,CHARINDEX('x',base.Opt)-1)
			ELSE
				'1'
			END AS OptQty,
			CASE 
				WHEN CHARINDEX('x',base.Opt) > 0 THEN
				SUBSTRING(base.Opt,CHARINDEX('x',base.Opt)+1,100)
			ELSE
				base.Opt
			END AS Opt	
		FROM base),
	
		satislar as 
		(	
					SELECT	
					Quantity * (CASE WHEN ISNUMERIC(OptQty) = 1 THEN CAST(OptQty AS INT) ELSE 1 END ) AS Quantity,Opt as ProductName
					FROM base_with_optqty bwo
				
					)

		SELECT a.* FROM 

		( SELECT 

		ProductName + ' ' + '(***Seçenek Menü Ýçeriði***)' as ProductName, 
		sum(Quantity) as Miktar, 
		0 as Tutar,
		0 as BirimFiyat


		FROM  satislar

		GROUP BY satislar.ProductName

		UNION

		SELECT 
		t.ProductName,
		sum(t.toplam) as Miktar,
		sum(t.tutar) as Tutar,
		AVG(t.BirimFiyat) as BirimFiyat

		FROM 
		(

		SELECT 

		P.Id as ProductId,
		SUM(B.Quantity) AS Toplam,
		B.ProductName,
		SUM(ISNULL(B.Quantity,0) * ISNULL(B.Price,0)) AS Tutar,
		AVG(ISNULL(B.Price,0)) BirimFiyat

		FROM dbo.Bill AS B

		LEFT JOIN dbo.Product as P ON P.Id=B.ProductId


		WHERE [Date]>=@par1 AND [Date]<=@par2
 
		GROUP BY
			B.ProductName,
			B.HeaderId,
			p.Id

		) as t

		GROUP BY
			t.ProductName,
			t.ProductId 	
		) as a

		where a.ProductName='{productGroup}'

		ORDER BY
			a.ProductName