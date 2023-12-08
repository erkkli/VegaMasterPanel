declare @par1 nvarchar(20) = '{TARIH1}';
declare @par2 nvarchar(20) = '{TARIH2}';
declare @par3 nvarchar(50) = '{PersonelYemekRaporuAdi}';
declare @Sube nvarchar(100) = '{SUBEADI}';
declare @Sube2 nvarchar(100) = '{SUBE2}';
declare @Kasa nvarchar(100) = '{KASAKODU}';

            WITH SATISRAPORU AS (
                SELECT
                    BAS.TARIH,
                    BAS.OLUSTURMATARIHI,
                    STOKIND,
                    STOKKODU,
					STK.OZELKOD1,
                    STOKADI,
                    BIRIM,
                    BAS.SUBEIND,
					BAS.KASAIND,
                    CASE WHEN BAS.IADE=0 THEN (HAR.MIKTAR) ELSE 0 END AS SATIS_MIKTARI,
                    CASE WHEN BAS.IADE<>0 THEN (HAR.MIKTAR) ELSE 0 END AS IADE_MIKTARI,
                    CASE WHEN BAS.IADE=0 THEN (HAR.MIKTAR*HAR.FIYAT) ELSE 0 END AS SATIS_TUTARI_TL,
                    CASE WHEN BAS.IADE<>0 THEN (HAR.MIKTAR*HAR.FIYAT) ELSE 0 END AS IADE_TUTARI_TL,
                    CASE WHEN BAS.IADE=0 THEN (HAR.ISKTUTARI) ELSE 0 END AS SATIS_ISKTUTARI_TL,
                    CASE WHEN BAS.IADE<>0 THEN (HAR.ISKTUTARI) ELSE 0 END AS IADE_ISKTUTARI_TL
                 FROM TBLSPOSSATISHAREKET AS HAR WITH(NOLOCK)
                 JOIN TBLSPOSSATISBASLIK AS BAS WITH(NOLOCK) ON BAS.IND=HAR.BASLIKIND
				 LEFT JOIN TBLSPOSSTOKLAR AS STK WITH(NOLOCK) ON STK.IND=HAR.STOKIND
                 WHERE ISNULL(BAS.IsDeleted,0)=0 AND ISNULL(HAR.IsDeleted,0)=0 AND ISNULL(BAS.BEKLEYENFIS,0)=0
                    )
                    
                SELECT

				SUM(SATIS_MIKTARI)-SUM(IADE_MIKTARI) AS MIKTAR,
				STOKADI  ProductName,
				OZELKOD1 ProductGroup,
				SUM(SATIS_TUTARI_TL) -SUM(IADE_TUTARI_TL) TUTAR,
				SUM(SATIS_ISKTUTARI_TL)-SUM(IADE_ISKTUTARI_TL) INDIRIM,
				0 IKRAM

                FROM SATISRAPORU
                WHERE 
                    OLUSTURMATARIHI>=@par1 AND OLUSTURMATARIHI<=@par2
                    AND SUBEIND IN (SELECT IND FROM TBLSPOSSUBELER  WITH(NOLOCK) WHERE ISNULL(IsDeleted,0)=0 AND KODU=@Sube2)   
AND KASAIND IN (SELECT IND FROM TBLSPOSKASALAR  WITH(NOLOCK) WHERE ISNULL(IsDeleted,0)=0 AND Charindex('&'+KODU+'&',@kasa)>0) 

					AND OZELKOD1=@par3
               
                GROUP BY 
                STOKADI,
					OZELKOD1
                