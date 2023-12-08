declare @par1 nvarchar(20) = '{TARIH1}';
declare @par2 nvarchar(20) = '{TARIH2}';
declare @Sube nvarchar(100) = '{SUBEADI}';
declare @Sube2 nvarchar(100) = '{SUBE2}';
declare @Kasa nvarchar(100) = '{KASAKODU}';
DECLARE @par3EndDate nvarchar(20) = '{SAAT}';


       WITH SATISRAPORU AS (
                SELECT
				case when CAST( BAS.OLUSTURMATARIHI AS time)< @par3EndDate then CAST( BAS.OLUSTURMATARIHI -1 AS date)
              else CAST( BAS.OLUSTURMATARIHI AS date)        end TARIH,
          LEFT(CONVERT(varchar,  BAS.OLUSTURMATARIHI, 114), 2)+ ':00' as SAAT,
                    BAS.TARIH TARIH2,
                    BAS.OLUSTURMATARIHI,
                    STOKIND,
                    STOKKODU,
                    STOKADI,
					STK.OZELKOD1,
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
                 LEFT JOIN TBLSPOSSATISBASLIK AS BAS WITH(NOLOCK) ON BAS.IND=HAR.BASLIKIND
				 LEFT JOIN TBLSPOSSTOKLAR AS STK WITH(NOLOCK) ON STK.IND=HAR.STOKIND
                 WHERE ISNULL(BAS.IsDeleted,0)=0 AND ISNULL(HAR.IsDeleted,0)=0 AND ISNULL(BAS.BEKLEYENFIS,0)=0
                    )                    
                SELECT
				--TARIH,
				--SAAT,
				SUM(SATIS_MIKTARI)-SUM(IADE_MIKTARI) AS MIKTAR,
				--STOKADI  ProductName,
				OZELKOD1 ProductGroup,
				SUM(SATIS_TUTARI_TL) -SUM(IADE_TUTARI_TL) TUTAR,
				SUM(SATIS_ISKTUTARI_TL)-SUM(IADE_ISKTUTARI_TL) INDIRIM,
				0 IKRAM
               
                FROM SATISRAPORU
                WHERE 
                    TARIH=@par1 
                  AND SUBEIND IN (SELECT IND FROM TBLSPOSSUBELER  WITH(NOLOCK) WHERE ISNULL(IsDeleted,0)=0 AND KODU=@Sube2)   
					AND KASAIND IN (SELECT IND FROM TBLSPOSKASALAR  WITH(NOLOCK) WHERE ISNULL(IsDeleted,0)=0 AND Charindex('&'+KODU+'&',@kasa)>0) 
             
                GROUP BY 
				OZELKOD1
				--TARIH,
				--SAAT,
                    --STOKKODU,
                    --STOKADI,
                    --BIRIM
                ORDER BY 
				OZELKOD1

				--STOKKODU,
    --                STOKADI   