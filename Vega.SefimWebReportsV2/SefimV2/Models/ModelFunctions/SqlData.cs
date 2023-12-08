using SefimV2.Helper;
using SefimV2.ViewModels.SubeSettings;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text;

namespace SefimV2
{
    public class SqlData
    {
        #region Sql Parametr

        public SqlConnection connection;
        public SqlData(SqlConnection con)
        {
            this.connection = con;
        }
        public int ExecuteSql(string sqlCommand, params object[] args)
        {
            SqlCommand cmd = GetSqlCommand(sqlCommand, args);
            lock (connection)
            {
                return cmd.ExecuteNonQuery();
            }
        }
        public int ExecuteScalarSql(string sqlCommand, params object[] args)
        {
            SqlCommand cmd = GetSqlCommand(sqlCommand, args);
            lock (connection)
            {
                return (int)cmd.ExecuteScalar();
            }
        }
        public int ExecuteScalarTransactionSql(string sqlCommand, SqlTransaction transaction, params object[] args)
        {
            SqlCommand cmd = GetSqlCommandTransaction(sqlCommand, args, transaction);
            lock (connection)
            {
                return (int)cmd.ExecuteScalar();
            }
        }
        public SqlCommand GetSqlCommand(string sqlCommand, object[] args)
        {
            if (this.connection.State != ConnectionState.Open)
            {
                this.connection.Open();
            }

            SqlCommand c = new SqlCommand(sqlCommand, this.connection);

            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] == null)
                    args[i] = DBNull.Value;
                c.Parameters.AddWithValue("par" + (i + 1).ToString(), args[i]);
            }
            return c;
        }
        public SqlCommand GetSqlCommandTransaction(string sqlCommand, object[] args, SqlTransaction transaction)
        {
            if (this.connection.State != ConnectionState.Open)
            {
                this.connection.Open();
            }

            SqlCommand c = new SqlCommand(sqlCommand, this.connection, transaction);

            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] == null)
                    args[i] = DBNull.Value;
                c.Parameters.AddWithValue("par" + (i + 1).ToString(), args[i]);
            }
            return c;
        }
        public object GetSqlValue(string sqlCommand, params object[] par1)
        {
            lock (this.connection)
            {
                SqlCommand c = GetSqlCommand(sqlCommand, par1);
                SqlDataReader reader = c.ExecuteReader();
                object retval = null;
                try
                {
                    if (!reader.Read()) return null;
                    if (!reader.HasRows) return null;
                    retval = reader.GetValue(0);
                }
                finally
                {
                    reader.Close();
                }
                return retval;
            }
        }

        #endregion Sql Parametr

        public static SubeSettingsViewModel GetSube(int subeId)
        {
            var model = new SubeSettingsViewModel();
            var mf = new ModelFunctions();

            mf.SqlConnOpen();
            var dataSubeler = mf.DataTable("SELECT * FROM SubeSettings WHERE AppDbTypeStatus in (0,1) and Status=1 and Id=" + subeId);
            mf.SqlConnClose();

            foreach (DataRow item in dataSubeler.Rows)
            {
                model.ID = Convert.ToInt32(mf.RTS(item, "Id"));
                model.SubeName = (mf.RTS(item, "SubeName"));
                model.SubeIP = mf.RTS(item, "SubeIP");
                model.SqlName = mf.RTS(item, "SqlName");
                model.SqlPassword = mf.RTS(item, "SqlPassword");
                model.DBName = mf.RTS(item, "DBName");
                model.SubeName = mf.RTS(item, "SubeName");
            }
            return model;
        }
        public static string getLocalSubeGroupByListSqlQuery()
        {
            return (@"  Select SubeId, SubeName from Product group by SubeName, SubeId");
        }

        public static List<SubeSettingsViewModel> GetIsActivAllSube()
        {
            var mf = new ModelFunctions();
            var subeList = new List<SubeSettingsViewModel>();
            mf.SqlConnOpen();
            var dataSubeler = mf.DataTable("SELECT * FROM SubeSettings WHERE AppDbTypeStatus in (0,1) and Status=1");
            mf.SqlConnClose();

            foreach (DataRow item in dataSubeler.Rows)
            {
                var model = new SubeSettingsViewModel
                {
                    ID = Convert.ToInt32(mf.RTS(item, "Id")),
                    SubeIP = mf.RTS(item, "SubeIP"),
                    SqlName = mf.RTS(item, "SqlName"),
                    SqlPassword = mf.RTS(item, "SqlPassword"),
                    DBName = mf.RTS(item, "DBName"),
                    SubeName = mf.RTS(item, "SubeName")
                };
                subeList.Add(model);
            }
            return subeList;
        }


        public static string getProductSqlQuery()
        {
            return (@"SELECT 
                            [ProductName]
                            ,[ProductGroup]
                            ,[ProductCode]
                            ,[Order]
                            ,[Price]
                            ,[VatRate]
                            ,[FreeItem]
                            ,[InvoiceName]
                            ,[ProductType]
                            ,[Plu]
                            ,[SkipOptionSelection]
                            ,[Favorites]
                            ,[Aktarildi]
                            ,[Id] as ProductPkId
                      FROM [Product] 
                    Where ProductGroup Not Like '$%'  
                         and  ProductGroup <> '[R]Rezervasyon' 
                         and  ProductGroup <> 'Getir' 
                         and  ProductGroup <> 'Yemeksepeti' 
                         and  ProductGroup <> 'Trendyol' 
                    ");

        }

        public static string GetProductSqlQuery2()
        {
            return (@"SELECT [Id]
                            ,[ProductName]
                            ,[ProductGroup]
                            ,[ProductCode]
                            ,[Order]
                            ,[Price]
                            ,[VatRate]
                            ,[FreeItem]
                            ,[InvoiceName]
                            ,[ProductType]
                            ,[Plu]
                            ,[SkipOptionSelection]
                            ,[Favorites]
                            ,[Aktarildi]
                            ,[IsUpdated]
                      FROM [Product] 
                    Where ProductGroup Not Like '$%'  
                         and  ProductGroup <> '[R]Rezervasyon' 
                         and  ProductGroup <> 'Getir' 
                         and  ProductGroup <> 'Yemeksepeti' 
                         and  ProductGroup <> 'Trendyol' 
                    ");

        }

        public static string getChoice1SqlQuery()
        {
            return (@"SELECT [Id]
                            ,[ProductId]
                            ,[Name]
                            ,[Price]
                            ,[Aktarildi]
                            ,[SubeId]
                            ,[Choice1PkId]
                            ,[SubeName]
                            ,[IsUpdate]
                           FROM [dbo].[Choice1] ");
        }
        public static string getChoice2SqlQuery()
        {
            return (@"SELECT [Id]
                            ,[ProductId]
                            ,[Choice1Id]
                            ,[Name]
                            ,[Price]
                            ,[Aktarildi]
                            ,[SubeId]
                            ,[Choice2PkId]
                            ,[SubeName]
                            ,[IsUpdate]
                          FROM [dbo].[Choice2]");
        }
        public static string getOptionsSqlQuery()
        {
            return (@"SELECT [Id]
                            ,[Name]
                            ,[Price]
                            ,[Quantitative]
                            ,[ProductId]
                            ,[Category]
                            ,[Aktarildi]
                            ,[SubeId]
                            ,[SubeName]
                            ,[OptionsPkId]
                            ,[IsUpdate]
                          FROM [dbo].[Options]");
        }

        public static string getProductLocalDbSqlQuery(int subeId)
        {
            return (@"SELECT Id
                            ,ProductName
                            ,ProductGroup
                            ,ProductCode
                            ,[Order]
                            ,Price
                            ,VatRate
                            ,FreeItem
                            ,InvoiceName
                            ,ProductType
                            ,Plu
                            ,SkipOptionSelection
                            ,Favorites
                            --,Aktarildi
                            ,SubeId
                            ,SubeName
                            ,ProductPkId
                      FROM Product  
                           Where SubeId=" + subeId);
        }

        //Parametresiz localdeki product tablosunu çeker.
        public static string getProductLocalSqlQuery()
        {
            var sqlQuery = new StringBuilder();
            sqlQuery.AppendLine(@"DECLARE @cols AS NVARCHAR(MAX), @query  AS NVARCHAR(MAX);
                                select @cols = STUFF((SELECT distinct 
                                           ',' + QUOTENAME(SubeName)
                                               FROM Product
                                               FOR XML PATH(''), TYPE
                                               ).value('.', 'NVARCHAR(MAX)') 
                                               ,1,1,'')

                                SET @query = '
                                SELECT P.*
                                FROM 
                                     (SELECT 
                                ProductName,ProductGroup,Price,SubeName,SubeId
                                  FROM Product ) C

                                  PIVOT
                                (
                                   SUM(Price)
                                FOR SubeName IN( '+@cols+' )
                                ) AS P

                                '
                                execute(@query)");

            return (sqlQuery.ToString());
        }
        public static string getProductLocalDbForInsertSqlQuery()
        {
            return (@"SELECT Id
                            ,ProductName
                            ,ProductGroup
                            ,ProductCode
                            ,[Order]
                            ,Price
                            ,VatRate
                            ,FreeItem
                            ,InvoiceName
                            ,ProductType
                            ,Plu
                            ,SkipOptionSelection
                            ,Favorites
                            --,Aktarildi
                            ,SubeId
                            ,SubeName
                            ,ProductPkId
                      FROM Product  
                     ");
        }

        public static string getProductLocalDbForInsertSqlQuery_()
        {
            return (@"SELECT *                    FROM Product                  ");
        }


        public static string getProductLocalDbPkIdSqlQuery(int productId)
        {
            return (@"Select Id, SubeId, ProductPkId From Product Where Id=" + productId);
            //return Utility.getDecimal(getSqlValue("SELECT Id, SubeId, ProductPkId FROM Product Where Id=@par1", Id));
        }
        public static string getChoice1LocalDbPkIdSqlQuery(int productId)
        {
            return (@"Select Id, SubeId, ProductId, Choice1PkId From Choice1 Where ProductPkId=" + productId);
            //return Utility.getDecimal(getSqlValue("SELECT Id, SubeId, ProductPkId FROM Product Where Id=@par1", Id));
        }
        public static string getProductAndChoiceAndOptionsSqlQuery()
        {
            return (@"select p.Id, p.ProductGroup, p.ProductName ProductName,P.Price , c1.Id Choice1Id,c1.Name Choice1_Name,c1.Price Choice1_Price,c2.Id Choice2Id,c2.Name Choice2_Name,
                        c2.Price Choice2_Price, o.Id OptionId, o.Name OptionsName,o.Price Option_Price from
                        Product p 
                        left join yaprakdb.dbo.Choice1 c1 on p.Id=c1.ProductId
                        left join yaprakdb.dbo.Choice2 c2 on c1.Id=c2.Choice1Id and c1.ProductId=c2.ProductId
                        left join yaprakdb.dbo.Options O on O.ProductId=P.Id
                     ");
        }
        public static string getProductForProductGroupSqlQuery2(string productGroup, string subeIdGrubuList, string sablonName)
        {
            string[] sp = productGroup.Split(',');
            string filter = " Where ";
            for (int i = 0; i < sp.Length; i++)
            {
                if (sp.Length - 1 == i)
                {
                    filter += " t.ProductGroup='" + sp[i] + "'";
                }
                else
                {
                    filter += " t.ProductGroup='" + sp[i] + "' or  ";
                }
            }

            var query = (@"select* from(
                     Select
                        ISNULL((Select top 1 ProductId from SablonChoice1 where ProductId = ptp.ProductId), 0) Choice1VarMi,
                        ISNULL((Select  top 1 ProductId from SablonOptions where ProductId = ptp.ProductId),0) OptionsVarMi,
                        p.ProductGroup, p.ProductName,  ptp.Id, ptp.Price productprice,0 c1price,0 c2price,0 optionprice,ptp.ProductId, ptp.Choice1Id,ptp.Choice2Id,ptp.OptionsId,
	                    ptp.ProductTemplatePricePkId,p.ProductPkId,ptp.SubeId,ptp.SubeName,
	                    ch1.Name ChoiceProductName, ch2.Name Choice2ProductName, op.Name OptionsProductName,
						ptp.GuncellenecekSubeIdGrubu,
						ptp.TemplateName,
                        ptp.IsManuelInsert
                      From ProductTemplatePrice ptp
                        Left join SablonProduct p on p.ProductPkId = ptp.ProductId
                        Left join SablonChoice1 ch1 on ptp.ProductId = ch1.ProductId  and ptp.Choice1Id = ch1.Choice1PkId
                        Left join SablonChoice2 ch2 on ptp.ProductId = ch2.ProductId     and ptp.Choice2Id = ch2.Choice2PkId
                        Left join SablonOptions op on ptp.ProductId = op.ProductId and ptp.OptionsId = op.OptionsPkId
                        Where ptp.SubeId is not null
                         and ptp.Choice1Id = 0 and ptp.Choice2Id = 0 and ptp.OptionsId = 0
						 and ptp.GuncellenecekSubeIdGrubu='" + subeIdGrubuList + @"'
						 and ptp.TemplateName='" + sablonName + @"'
                      Group by
                        p.ProductGroup, p.ProductName,  ptp.Id, ptp.Price,ptp.ProductId, ptp.Choice1Id,ptp.Choice2Id,ptp.OptionsId,
                        ptp.ProductTemplatePricePkId,p.ProductPkId, ch1.Name,ptp.ProductId,ptp.SubeId,ptp.SubeName,ch2.Name,op.Name,
						ptp.GuncellenecekSubeIdGrubu,
						ptp.TemplateName,
                        ptp.IsManuelInsert

                    UNION ALL

                    Select
                        ISNULL((Select  top 1 ProductId from SablonChoice1 where ProductId = ptp.ProductId),0) Choice1VarMi,
                        ISNULL((Select  top 1 ProductId from SablonOptions where ProductId = ptp.ProductId),0) OptionsVarMi,
                        p.ProductGroup, p.ProductName,  ptp.Id, 0 productprice, ptp.Price c1price,0 c2price,0 optionprice,ptp.ProductId, ptp.Choice1Id,ptp.Choice2Id,ptp.OptionsId,
	                    ptp.ProductTemplatePricePkId,p.ProductPkId,ptp.SubeId,ptp.SubeName,
	                    ch1.Name ChoiceProductName, ch2.Name Choice2ProductName, op.Name OptionsProductName,
					    ptp.GuncellenecekSubeIdGrubu,
						ptp.TemplateName,
                        ptp.IsManuelInsert
                      From ProductTemplatePrice ptp
                        Left join SablonProduct p on p.ProductPkId = ptp.ProductId
                        Left join SablonChoice1 ch1 on ptp.ProductId = ch1.ProductId  and ptp.Choice1Id = ch1.Choice1PkId
                        Left join SablonChoice2 ch2 on ptp.ProductId = ch2.ProductId     and ptp.Choice2Id = ch2.Choice2PkId
                        Left join SablonOptions op on ptp.ProductId = op.ProductId and ptp.OptionsId = op.OptionsPkId
                        Where ptp.SubeId is not null
                         and ptp.Choice1Id > 0 and ptp.Choice2Id = 0 and ptp.OptionsId = 0
						 and ptp.GuncellenecekSubeIdGrubu='" + subeIdGrubuList + @"'
						 and ptp.TemplateName='" + sablonName + @"'
                      Group by
                        p.ProductGroup, p.ProductName,  ptp.Id, ptp.Price,ptp.ProductId, ptp.Choice1Id,ptp.Choice2Id,ptp.OptionsId,
                        ptp.ProductTemplatePricePkId,p.ProductPkId, ch1.Name,ptp.ProductId,ptp.SubeId,ptp.SubeName,ch2.Name,op.Name,
						  ptp.GuncellenecekSubeIdGrubu,
						ptp.TemplateName,
                        ptp.IsManuelInsert

                        UNION ALL

                      Select
                        ISNULL((Select  top 1 ProductId from SablonChoice1 where ProductId = ptp.ProductId),0) Choice1VarMi,
                        ISNULL((Select  top 1 ProductId from SablonOptions where ProductId = ptp.ProductId),0) OptionsVarMi,
                        p.ProductGroup, p.ProductName,  ptp.Id, 0 productprice, 0 c1price,ptp.Price c2price,0 optionprice,ptp.ProductId, ptp.Choice1Id,ptp.Choice2Id,ptp.OptionsId,
	                    ptp.ProductTemplatePricePkId,p.ProductPkId,ptp.SubeId,ptp.SubeName,
	                    ch1.Name ChoiceProductName, ch2.Name Choice2ProductName, op.Name OptionsProductName,
					    ptp.GuncellenecekSubeIdGrubu,
						ptp.TemplateName,
                        ptp.IsManuelInsert

                      From ProductTemplatePrice ptp
                        Left join SablonProduct p on p.ProductPkId = ptp.ProductId
                        Left join SablonChoice1 ch1 on ptp.ProductId = ch1.ProductId  and ptp.Choice1Id = ch1.Choice1PkId
                        Left join SablonChoice2 ch2 on ptp.ProductId = ch2.ProductId     and ptp.Choice2Id = ch2.Choice2PkId
                        Left join SablonOptions op on ptp.ProductId = op.ProductId and ptp.OptionsId = op.OptionsPkId
                        Where ptp.SubeId is not null
                         and ptp.Choice1Id > 0 and ptp.Choice2Id > 0 and ptp.OptionsId = 0
						 and ptp.GuncellenecekSubeIdGrubu='" + subeIdGrubuList + @"'
						 and ptp.TemplateName='" + sablonName + @"'
                      Group by
                        p.ProductGroup, p.ProductName,  ptp.Id, ptp.Price,ptp.ProductId, ptp.Choice1Id,ptp.Choice2Id,ptp.OptionsId,
                        ptp.ProductTemplatePricePkId,p.ProductPkId, ch1.Name,ptp.ProductId,ptp.SubeId,ptp.SubeName,ch2.Name,op.Name,
						ptp.GuncellenecekSubeIdGrubu,
						ptp.TemplateName,
                        ptp.IsManuelInsert

                        UNION ALL

                      Select
                        ISNULL((Select  top 1 ProductId from SablonChoice1 where ProductId = ptp.ProductId),0) Choice1VarMi,
                        ISNULL((Select  top 1 ProductId from SablonOptions where ProductId = ptp.ProductId),0) OptionsVarMi,
                        p.ProductGroup, p.ProductName,  ptp.Id, 0 productprice, 0 c1price,0 c2price,ptp.Price optionprice, ptp.ProductId, ptp.Choice1Id,ptp.Choice2Id,ptp.OptionsId,
	                    ptp.ProductTemplatePricePkId,p.ProductPkId,ptp.SubeId,ptp.SubeName,
	                    ch1.Name ChoiceProductName, ch2.Name Choice2ProductName, op.Name OptionsProductName,
						ptp.GuncellenecekSubeIdGrubu,
						ptp.TemplateName,
                        ptp.IsManuelInsert
                      From ProductTemplatePrice ptp
                        Left join SablonProduct p on p.ProductPkId = ptp.ProductId
                        Left join SablonChoice1 ch1 on ptp.ProductId = ch1.ProductId  and ptp.Choice1Id = ch1.Choice1PkId
                        Left join SablonChoice2 ch2 on ptp.ProductId = ch2.ProductId     and ptp.Choice2Id = ch2.Choice2PkId
                        Left join SablonOptions op on ptp.ProductId = op.ProductId and ptp.OptionsId = op.OptionsPkId
                        Where ptp.SubeId is not null
                        and ptp.Choice1Id = 0 and ptp.Choice2Id = 0 and ptp.OptionsId > 0
						and ptp.GuncellenecekSubeIdGrubu='" + subeIdGrubuList + @"'
						and ptp.TemplateName='" + sablonName + @"'
                      Group by
                        p.ProductGroup, p.ProductName,  ptp.Id, ptp.Price,ptp.ProductId, ptp.Choice1Id,ptp.Choice2Id,ptp.OptionsId,
                        ptp.ProductTemplatePricePkId,p.ProductPkId, ch1.Name,ptp.ProductId,ptp.SubeId,ptp.SubeName,ch2.Name,op.Name,
						ptp.GuncellenecekSubeIdGrubu,
						ptp.TemplateName, 
                        ptp.IsManuelInsert
                    ) t  " + filter + " and t.GuncellenecekSubeIdGrubu='" + subeIdGrubuList + @"' and t.TemplateName='" + sablonName + "' "

                  );

            return query;

        }

        public static string getProductForProductGroupSqlQuery(string productGroup)
        {
            string[] sp = productGroup.Split(',');
            string filter = " Where ";
            for (int i = 0; i < sp.Length; i++)
            {
                if (sp.Length - 1 == i)
                {
                    filter += "p.ProductGroup='" + sp[i] + "'";
                }
                else
                {
                    filter += " p.ProductGroup='" + sp[i] + "' or  ";
                }
            }

            return (@"Select 
                        ISNULL((Select  top 1 ProductId from Choice1 where ProductId=p.ProductPkId),0) Choice1VarMi,
                        ISNULL((Select  top 1 ProductId from Options where ProductId=p.ProductPkId),0) OptionsVarMi,
                        p.ProductGroup, p.ProductName, p.SubeName, p.Price, p.SubeId, p.Id 
                      From Product p
				      Left join SubeSettings ss on p.SubeId=ss.Id 
                      " + filter + " and ss.Status=1 Order By p.ProductGroup, p.ProductName   ");
        }

        //Fiyat guncelleme ekranında urun grubu filter için sube gruplamaya istinaden eklendi.
        public static string getProductForProductGroupSqlQueryVeriGonder(string productGroup, string subeIdGrupList)
        {
            string[] sp = productGroup.Split(',');
            string filter = " Where ";
            for (int i = 0; i < sp.Length; i++)
            {
                if (sp.Length - 1 == i)
                {
                    filter += "p.ProductGroup='" + sp[i] + "'";
                }
                else
                {
                    filter += " p.ProductGroup='" + sp[i] + "' or  ";
                }
            }

            return (@"Select 
                        ISNULL((Select  top 1 ProductId from Choice1 where ProductId=p.ProductPkId),0) Choice1VarMi,
                        ISNULL((Select  top 1 ProductId from Options where ProductId=p.ProductPkId),0) OptionsVarMi,
                        p.ProductGroup, p.ProductName, p.SubeName, p.Price, p.SubeId, p.Id 
                      From Product p
				      Left join SubeSettings ss on p.SubeId=ss.Id 
                      " + filter + " and ss.Status=1 and p.GuncellenecekSubeIdGrubu='" + subeIdGrupList + "'  Order By p.ProductGroup, p.ProductName   ");
        }


        public static string getChoice1SqlQuery(int productId)
        {
            return (@"Select  p.ProductGroup ,p.ProductName, c1.Id,c1.ProductId ,c1.Name ChoiceProductName ,P.Price Product_Price,c1.Price Choice1_Price 
                      From Product p 
                     Left Join Choice1 c1 on p.Id=c1.ProductId
                     Where c1.ProductId=" + productId);
        }

        public static string getChoice1SqlQuery2()
        {
            return (@"SELECT  [Id] as Choice1PkId
                              ,[ProductId]
                              ,[Name]
                              ,[Price]
                              ,[Aktarildi]

                          FROM [dbo].[Choice1]");
        }

        public static string getChoice1LocalDbSqlQuery(int subeId, int productId)
        {
            return (@"Select  p.ProductGroup ,p.ProductName, c1.Id,c1.ProductId ,c1.Name ChoiceProductName ,P.Price Product_Price,c1.Price Choice1_Price 
                     From Product p 
                     Left Join Choice1 c1 on p.ProductPkId=c1.ProductId
                     Where c1.SubeId=" + subeId + " and c1.ProductId=" + productId);
        }
        public static string getChoice2SqlQuery(int Choice1Id, int productId)
        {
            return (@"Select  p.ProductGroup ,p.ProductName, c2.Id,c2.ProductId ,c2.Name ChoiceProductName ,P.Price Product_Price,c2.Price Choice2_Price 
                      From Product p 
                      Left Join Choice1 c1 on p.Id=c1.ProductId
					  Left join Choice2 c2 on c1.Id=c2.Choice1Id and c1.ProductId=c2.ProductId
                      Where c2.Choice1Id=" + Choice1Id);
        }

        public static string getChoice2SqlQuery2()
        {
            return (@"SELECT  [Id] as Choice2PkId
                              ,[ProductId]
                              ,[Choice1Id]
                              ,[Name]
                              ,[Price]
                              ,[Aktarildi]                             
                          FROM [dbo].[Choice2]");
        }

        public static string getChoice2LocalDbSqlQuery(int Choice1Id, int productId)
        {
            return (@"Select  p.ProductGroup ,p.ProductName, c2.Id,c2.ProductId ,c2.Name ChoiceProductName ,P.Price Product_Price,c2.Price Choice2_Price 
                      From Product p 
                      Left Join Choice1 c1 on p.Id=c1.ProductId
					  Left join Choice2 c2 on c1.Id=c2.Choice1Id and c1.ProductId=c2.ProductId
                      Where c2.Choice1Id=" + Choice1Id);
        }
        public static string getOptionsSqlQuery(int productId)
        {
            return (@"Select p.ProductGroup, p.ProductName, o.Id, o.ProductId, o.Name OptionsName, P.Price Product_Price, o.Price Option_Price, o.Category
                      From Product p 
                      Left Join Options O on O.ProductId=P.Id
                      Where o.ProductId=" + productId);
        }

        public static string getOptionsSqlQuery2()
        {
            return (@"SELECT [Id] as OptionsPkId
                              ,[Name]
                              ,[Price]
                              ,[Quantitative]
                              ,[ProductId]
                              ,[Category]
                              ,[Aktarildi]     
                          FROM [dbo].[Options]");
        }
        public static string getOptionsLocalDbSqlQuery(int subeId, int productId)
        {
            return (@"Select p.ProductGroup, p.ProductName, o.Id, o.ProductId, o.Name OptionsName, P.Price Product_Price, o.Price Option_Price, o.Category
                      From Product p 
                      Left Join Options O on O.ProductId=P.ProductPkId
                      Where o.ProductId=" + productId + " and O.SubeId=" + subeId);
        }
        public static string getUrunGrubuSqlQuery()
        {
            return (@"Select Count(Id), ProductGroup 
                      From Product  Where ProductGroup Not Like '$%' 
                      Group By ProductGroup  
                      HAVING COUNT(Id) >0 ");
        }

        public static string GetSablonProductUrunGrubuSqlQuery()
        {
            return (@" Select Count(Id), ProductGroup 
                       From SablonProduct  Where ProductGroup Not Like '$%' 
                       Group By ProductGroup  
                       HAVING COUNT(Id) >0 ");
        }

        #region Local db'den datalar alınır

        public static string getLocalProductListSqlQuery(string SubeIdGrupList)
        {
            var query = "Select ISNULL((Select   ProductId from Choice1 where ProductId=p.ProductPkId and SubeId=p.SubeId and GuncellenecekSubeIdGrubu='" + SubeIdGrupList + "' group by ProductId),0) Choice1VarMi," +
                        "       ISNULL((Select   ProductId from Options where ProductId=p.ProductPkId and  SubeId=p.SubeId and GuncellenecekSubeIdGrubu='" + SubeIdGrupList + "' group by ProductId),0) OptionsVarMi," +
                        "       p.ProductGroup, p.ProductName, p.SubeName, p.Price, p.SubeId, p.Id " +
                        "       From Product p" +
                        "       Left join SubeSettings ss on p.SubeId=ss.Id " +
                        " Where ss.Status=1 and p.GuncellenecekSubeIdGrubu='" + SubeIdGrupList + "'  Order By p.ProductGroup, p.ProductName ";

            return query;
        }
        public static string getLocalChoice1tListSqlQuery(string productGroup, string productName, string subeIdGrupList)
        {
            return (@"Select  ISNULL((Select  top 1 ProductId from Choice2 where ProductId=p.ProductPkId and Choice1Id=ch.Choice1PkId),0) Choice2VarMi, 
                       p.ProductGroup ,p.ProductName,p.Id PId, ch.Id, ch.ProductId, ch.Name ChoiceProductName, ch.Price Choice1_Price, ch.SubeId,ch.SubeName, ch.Choice1PkId
                      From Choice1 ch
				      Left join SubeSettings ss on ch.SubeId=ss.Id
					  Left join Product p on ch.SubeId=p.SubeId and ch.ProductId=p.ProductPkId
					  Where ss.Status=1 and p.ProductGroup='" + productGroup + "' and p.ProductName='" + productName + "' and  p.GuncellenecekSubeIdGrubu='" + subeIdGrupList + "'  Order By p.ProductGroup, p.ProductName"
                   );
        }
        public static string GetLocalChoice2tListSqlQuery(int SubeId, int Choice1Id, string subeIdGrupList, string choice1Name)
        {
            //var query = (@"  Select  p.ProductGroup ,p.ProductName, c2.Id,c2.ProductId ,c2.Name ChoiceProductName ,P.Price Product_Price,c2.Price Choice2_Price , c1.Choice1PkId,c2.SubeId, c2.SubeName 
            //From Product p 
            //Left Join Choice1 c1 on p.ProductPkId=c1.ProductId      
            //Left join Choice2 c2 on c1.Choice1PkId=c2.Choice1Id and c1.ProductId=c2.ProductId            
            //Where c2.Choice1Id=" + Choice1Id + " and c2.GuncellenecekSubeIdGrubu='" + subeIdGrupList + "'");


            var query = (@"  Select distinct  p.ProductGroup ,p.ProductName, c2.Id,c2.ProductId ,c2.Name ChoiceProductName ,P.Price Product_Price,c2.Price Choice2_Price , c1.Choice1PkId,c2.SubeId, c2.SubeName 
					  From Product p 
					  Left Join Choice1 c1 on p.ProductPkId=c1.ProductId      
					  Left join Choice2 c2 on c1.Choice1PkId=c2.Choice1Id and c1.ProductId=c2.ProductId            
					  Where " +
                      "c1.Name='" + choice1Name + "' " +
                      "and c2.GuncellenecekSubeIdGrubu='" + subeIdGrupList + "'");

            return query;
        }
        public static string getLocalOptionstListSqlQuery(string subeId, string productId, string subeIdGrupList)
        {
            return (@"Select  p.ProductGroup ,p.ProductName,p.Id PId, op.Id, op.ProductId, op.Name OptionsName, op.Price Option_Price, op.SubeId,op.SubeName         
	                  From Options op        
	                  Left join SubeSettings ss on op.SubeId=ss.Id  
                      Left join Product p on op.SubeId=p.SubeId and op.ProductId=p.ProductPkId
					  Where ss.Status=1 and p.ProductGroup='" + subeId + "' and p.ProductName='" + productId + "'  and  p.GuncellenecekSubeIdGrubu='" + subeIdGrupList + "'   Order By p.ProductGroup, p.ProductName"
                   );
        }
        public static string getLocalChoice1ListSqlQuery(string productGroup, string ProductName)
        {
            return (@" Select  p.ProductGroup ,p.ProductName, c1.Id,c1.ProductId ,c1.Name ChoiceProductName ,P.Price Product_Price,c1.Price Choice1_Price ,c1.SubeId,c1.SubeName
                      From Product p 
                      Left Join Choice1 c1 on p.ProductPkId=c1.ProductId
                       where p.ProductGroup='" + productGroup + "' and p.ProductName='" + ProductName + "' Group by  p.ProductGroup, p.ProductName, c1.Id, c1.ProductId, c1.Name, P.Price, c1.Price, c1.SubeId, c1.SubeName ");
        }
        public static string getLocalChoice2UpdateListSqlQuery(int SubeId, int Choice1Id, string subeIdGrupList, string choice1Name)
        {
            //return (@"  Select  p.ProductGroup, p.ProductName, c2.Id, c2.ProductId, c2.Name ChoiceProductName, P.Price Product_Price, c2.Price Choice2_Price , c1.Choice1PkId,c2.SubeId 
            //From Product p 
            //Left Join Choice1 c1 on p.ProductPkId=c1.ProductId      
            //Left join Choice2 c2 on c1.Choice1PkId=c2.Choice1Id and c1.ProductId=c2.ProductId            
            //Where c2.Id=" + Choice2Id);

            var query = (@"Select distinct  p.ProductGroup, p.ProductName, c2.Id, c2.ProductId, c2.Name ChoiceProductName, P.Price Product_Price, c2.Price Choice2_Price, c1.Choice1PkId, c2.SubeId, c2.SubeName 
					  From Product p 
					  Left Join Choice1 c1 on p.ProductPkId=c1.ProductId      
					  Left join Choice2 c2 on c1.Choice1PkId=c2.Choice1Id and c1.ProductId=c2.ProductId            
					  Where " +
                               "c1.Name='" + choice1Name + "' " +
                               "and c2.GuncellenecekSubeIdGrubu='" + subeIdGrupList + "'");

            return query;
        }

        public static string getLocalChoice2ListSqlQuery()
        {
            return (@" Select  p.ProductGroup ,p.ProductName, c2.Id,c2.ProductId ,c2.Name ChoiceProductName ,P.Price Product_Price,c2.Price Choice2_Price 
                           From Product p 
                           Left Join Choice1 c1 on p.Id=c1.ProductId
            Left join Choice2 c2 on c1.Id=c2.Choice1Id and c1.ProductId=c2.ProductId
                          ");
        }
        public static string getLocalOptionsListSqlQuery(int productId)
        {
            return (@"Select p.ProductGroup, p.ProductName, o.Id, o.ProductId, o.Name OptionsName, P.Price Product_Price, o.Price Option_Price, o.Category, o.SubeId, o.SubeName 
                      From Product p 
                      Left Join Options o On  p.ProductPkId= o.ProductId 
                      Where o.ProductId=" + productId + "  ");
        }

        #endregion local db'den datalar alınır

        #region Tarihçe quary

        public static string getProducTarihceLocalDbSqlQuery(int subeId)
        {
            return (@"SELECT [Id]
                            ,[ProductName]
                            ,[ProductGroup]
                            ,[ProductCode]
                            ,[Order]
                            ,[Price]
                            ,[VatRate]
                            ,[FreeItem]
                            ,[InvoiceName]
                            ,[ProductType]
                            ,[Plu]
                            ,[SkipOptionSelection]
                            ,[Favorites]
                            ,[Aktarildi]
                            ,[SubeId]
                            ,[SubeName]
                            ,[ProductPkId]
                            ,[IsUpdate]
                            ,[IsUpdateDate]
                            ,[IsUpdateKullanici]
                      FROM [dbo].[ProductTarihce]
                           Where SubeId=" + subeId);
        }
        public static string getProductTarihceForProductGroupSqlQuery(string productGroup)
        {
            string[] sp = productGroup.Split(',');
            string filter = " Where ";
            for (int i = 0; i < sp.Length; i++)
            {
                if (sp.Length - 1 == i)
                {
                    filter += " ProductGroup='" + sp[i] + "'";
                }
                else
                {
                    filter += " ProductGroup='" + sp[i] + "' or  ";
                }
            }

            return (@"Select * From ProductTarihce " + filter);
        }
        public static string getChoice1TarihceListSqlQuery(string productGroup, string productName)
        {
            return (@"Select  p.ProductGroup ,p.ProductName,p.Id PId, ch.Id, ch.ProductId, ch.Name ChoiceProductName, ch.Price Choice1_Price, ch.SubeId,ch.SubeName, ch.Choice1PkId, ch.IsUpdateDate, ch.IsUpdateKullanici
                      From Choice1Tarihce ch
				      Left join SubeSettings ss on ch.SubeId=ss.Id
					  Left join ProductTarihce  p on ch.SubeId=p.SubeId and ch.ProductId=p.ProductPkId
					  Where p.ProductGroup='" + productGroup + "' and p.ProductName='" + productName + "' Order By p.ProductGroup, p.ProductName"
                   );
        }
        public static string getTarihceOptionsListSqlQuery(string productGroup, string productName)
        {
            return (@"Select  p.ProductGroup ,p.ProductName,p.Id PId, op.Id, op.ProductId, op.Name OptionsName, op.Price Option_Price, op.SubeId,op.SubeName, op.IsUpdateDate, op.IsUpdateKullanici       
	                  From OptionsTarihce op        
	                  Left join SubeSettings ss on op.SubeId=ss.Id  
                      Left join ProductTarihce p on op.SubeId=p.SubeId and op.ProductId=p.ProductPkId
					  Where p.ProductGroup='" + productGroup + "' and p.ProductName='" + productName + "' Order By p.ProductGroup, p.ProductName"
                    );
        }

        #endregion Tarihçe quary

        #region Şablon quary

        public static string getSablonDbSqlQuery()
        {
            return (@"SELECT [Id]
                            ,[Name]
                            ,[Aktarildi]
                     FROM [dbo].[ProductTemplate]"
                          );
        }
        public static string getLocalProductTemplatePriceSqlQuery(string sablonName)
        {
            return (@" select* from(" +
"                     Select" +
"                        ISNULL((Select  top 1 ProductId from SablonChoice1 where ProductId=ptp.ProductId),0) Choice1VarMi," +
"                        ISNULL((Select  top 1 ProductId from SablonOptions where ProductId=ptp.ProductId),0) OptionsVarMi," +
"                        p.ProductGroup, p.ProductName,  ptp.Id, ptp.Price productprice,0 c1price,0 c2price,0 optionprice,ptp.ProductId, ptp.Choice1Id,ptp.Choice2Id,ptp.OptionsId," +
"	                    ptp.ProductTemplatePricePkId,p.ProductPkId,ptp.SubeId,ptp.SubeName," +
"	                    ch1.Name ChoiceProductName,ch2.Name Choice2ProductName , op.Name OptionsProductName" +
"                      From ProductTemplatePrice ptp" +
"                        Left join SablonProduct p on p.ProductPkId=ptp.ProductId " +
"	                    Left join SablonChoice1 ch1 on ptp.ProductId=ch1.ProductId 	and ptp.Choice1Id=ch1.Choice1PkId				" +
"                        Left join SablonChoice2 ch2 on  ptp.ProductId=ch2.ProductId 	and ptp.Choice2Id=ch2.Choice2PkId" +
"	                    Left join SablonOptions op on ptp.ProductId=op.ProductId and ptp.OptionsId=op.OptionsPkId" +
"	                    Where ptp.SubeId is not null" +
"						 and ptp.Choice1Id=0 and ptp.Choice2Id=0 and ptp.OptionsId=0" +
"						 AND ptp.TemplateName='" + sablonName + "'" +
"	                  Group by " +
"                        p.ProductGroup, p.ProductName,  ptp.Id, ptp.Price,ptp.ProductId, ptp.Choice1Id,ptp.Choice2Id,ptp.OptionsId," +
"                        ptp.ProductTemplatePricePkId,p.ProductPkId, ch1.Name,ptp.ProductId,ptp.SubeId,ptp.SubeName,ch2.Name,op.Name " +
"                    UNION ALL" +
"                    Select" +
"                        ISNULL((Select  top 1 ProductId from SablonChoice1 where ProductId=ptp.ProductId),0) Choice1VarMi," +
"                        ISNULL((Select  top 1 ProductId from SablonOptions where ProductId=ptp.ProductId),0) OptionsVarMi," +
"                        p.ProductGroup, p.ProductName,  ptp.Id, 0 productprice, ptp.Price c1price,0 c2price,0 optionprice,ptp.ProductId, ptp.Choice1Id,ptp.Choice2Id,ptp.OptionsId," +
"	                    ptp.ProductTemplatePricePkId,p.ProductPkId,ptp.SubeId,ptp.SubeName," +
"	                    ch1.Name ChoiceProductName,ch2.Name Choice2ProductName , op.Name OptionsProductName" +
"                      From ProductTemplatePrice ptp" +
"                        Left join SablonProduct p on p.ProductPkId=ptp.ProductId " +
"	                    Left join SablonChoice1 ch1 on ptp.ProductId=ch1.ProductId 	and ptp.Choice1Id=ch1.Choice1PkId				" +
"                        Left join SablonChoice2 ch2 on  ptp.ProductId=ch2.ProductId 	and ptp.Choice2Id=ch2.Choice2PkId" +
"	                    Left join SablonOptions op on ptp.ProductId=op.ProductId and ptp.OptionsId=op.OptionsPkId" +
"	                    Where ptp.SubeId is not null" +
"						 and ptp.Choice1Id>0 and ptp.Choice2Id=0 and ptp.OptionsId=0" +
"						 AND ptp.TemplateName='" + sablonName + "'" +
"	                  Group by " +
"                        p.ProductGroup, p.ProductName,  ptp.Id, ptp.Price,ptp.ProductId, ptp.Choice1Id,ptp.Choice2Id,ptp.OptionsId," +
"                        ptp.ProductTemplatePricePkId,p.ProductPkId, ch1.Name,ptp.ProductId,ptp.SubeId,ptp.SubeName,ch2.Name,op.Name " +
"						UNION ALL" +
"                      Select" +
"                        ISNULL((Select  top 1 ProductId from SablonChoice1 where ProductId=ptp.ProductId),0) Choice1VarMi," +
"                        ISNULL((Select  top 1 ProductId from SablonOptions where ProductId=ptp.ProductId),0) OptionsVarMi," +
"                        p.ProductGroup, p.ProductName,  ptp.Id, 0 productprice, 0 c1price,ptp.Price c2price,0 optionprice,ptp.ProductId, ptp.Choice1Id,ptp.Choice2Id,ptp.OptionsId," +
"	                    ptp.ProductTemplatePricePkId,p.ProductPkId,ptp.SubeId,ptp.SubeName," +
"	                    ch1.Name ChoiceProductName,ch2.Name Choice2ProductName , op.Name OptionsProductName" +
"                      From ProductTemplatePrice ptp" +
"                        Left join SablonProduct p on p.ProductPkId=ptp.ProductId " +
"	                    Left join SablonChoice1 ch1 on ptp.ProductId=ch1.ProductId 	and ptp.Choice1Id=ch1.Choice1PkId				" +
"                        Left join SablonChoice2 ch2 on  ptp.ProductId=ch2.ProductId 	and ptp.Choice2Id=ch2.Choice2PkId" +
"	                    Left join SablonOptions op on ptp.ProductId=op.ProductId and ptp.OptionsId=op.OptionsPkId" +
"	                    Where ptp.SubeId is not null" +
"						 and ptp.Choice1Id>0 and ptp.Choice2Id>0 and ptp.OptionsId=0" +
"						 AND ptp.TemplateName='" + sablonName + "'" +
"	                  Group by " +
"                        p.ProductGroup, p.ProductName,  ptp.Id, ptp.Price,ptp.ProductId, ptp.Choice1Id,ptp.Choice2Id,ptp.OptionsId," +
"                        ptp.ProductTemplatePricePkId,p.ProductPkId, ch1.Name,ptp.ProductId,ptp.SubeId,ptp.SubeName,ch2.Name,op.Name " +
"						" +
"						UNION ALL" +
"                      Select" +
"                        ISNULL((Select  top 1 ProductId from SablonChoice1 where ProductId=ptp.ProductId),0) Choice1VarMi," +
"                        ISNULL((Select  top 1 ProductId from SablonOptions where ProductId=ptp.ProductId),0) OptionsVarMi," +
"                        p.ProductGroup, p.ProductName,  ptp.Id, 0 productprice, 0 c1price,0 c2price,ptp.Price optionprice,ptp.ProductId, ptp.Choice1Id,ptp.Choice2Id,ptp.OptionsId," +
"	                    ptp.ProductTemplatePricePkId,p.ProductPkId,ptp.SubeId,ptp.SubeName," +
"	                    ch1.Name ChoiceProductName,ch2.Name Choice2ProductName , op.Name OptionsProductName" +
"                      From ProductTemplatePrice ptp" +
"                        Left join SablonProduct p on p.ProductPkId=ptp.ProductId " +
"	                    Left join SablonChoice1 ch1 on ptp.ProductId=ch1.ProductId 	and ptp.Choice1Id=ch1.Choice1PkId				" +
"                        Left join SablonChoice2 ch2 on  ptp.ProductId=ch2.ProductId 	and ptp.Choice2Id=ch2.Choice2PkId" +
"	                    Left join SablonOptions op on ptp.ProductId=op.ProductId and ptp.OptionsId=op.OptionsPkId" +
"	                    Where ptp.SubeId is not null" +
"						and ptp.Choice1Id=0 and ptp.Choice2Id=0 and ptp.OptionsId>0" +
"						 AND ptp.TemplateName='" + sablonName + "'" +
"	                  Group by " +
"                        p.ProductGroup, p.ProductName,  ptp.Id, ptp.Price,ptp.ProductId, ptp.Choice1Id,ptp.Choice2Id,ptp.OptionsId," +
"                        ptp.ProductTemplatePricePkId,p.ProductPkId, ch1.Name,ptp.ProductId,ptp.SubeId,ptp.SubeName,ch2.Name,op.Name " +
"                       ) t "

         );
        }
        public static string getByIdLocalProductTemplatePriceSqlQuery(string id)
        {
            return (@" SELECT  ptp.[Id]
                              ,ptp.[TemplateId]                            
                              ,ptp.[ProductId]
                              ,ptp.[Choice1Id]
                              ,ptp.[Choice2Id]
                              ,ptp.[OptionsId]
                              ,ptp.[Price]
                              ,ptp.[Aktarildi]  
                              ,[SubeId]
                          FROM [dbo].[ProductTemplatePrice] ptp
                        Where ptp.Id='" + id + "' ");
        }
        public static string getSablonListLocalDbSqlQuery()
        {
            return (@"SELECT [Id]
                             ,[Name]
                             ,[Aktarildi]
                             ,[SubeId]
                             ,[SubeName]
                             ,[ProductTemplatePkId]
                             ,[IsUpdate]
                      FROM [dbo].[ProductTemplate]");
        }
        public static string getProductTemplatePriceSqlQuery(string sablonName)
        {
            return (@" SELECT * FROM [dbo].[ProductTemplatePrice] ptp
					  Inner Join ProductTemplate pt on ptp.TemplateId=pt.Id
					  Where pt.Name='" + sablonName + "'");
        }
        public static string getProductTemplatePriceSqlQuery2(string sablonName, string SubeIdGrupList)
        {
            return (@" SELECT  ptp.[Id]
                              ,ptp.[TemplateId]                            
                              ,ptp.[ProductId]
                              ,ptp.[Choice1Id]
                              ,ptp.[Choice2Id]
                              ,ptp.[OptionsId]
                              ,ptp.[Price]
                              ,ptp.[Aktarildi]                            
                          FROM [dbo].[ProductTemplatePrice] ptp
						  Inner Join ProductTemplate pt on ptp.TemplateId=pt.Id
						  where pt.Name='" + sablonName + "'");
        }

        public static string getBySubeIdLocalProductTemplatePriceSqlQuery(int subeId)
        {
            return (@" SELECT  * FROM [dbo].[ProductTemplatePrice] ptp Where  ptp.IsManuelInsert is null and ptp.SubeId='" + subeId + "' ");
        }

        public static string getLocalSablonProductListSqlQuery(string SubeIdGrupList, string sablonName)
        {
            var query =
                     "SELECT * " +
                    " FROM" +
                    "  (SELECT ISNULL(" +
                    "                   (SELECT top 1 ProductId" +
                    "                    FROM SablonChoice1" +
                    "                    WHERE ProductId=ptp.ProductId),0) Choice1VarMi," +
                    "          ISNULL(" +
                    "                   (SELECT top 1 ProductId" +
                    "                    FROM SablonOptions" +
                    "                    WHERE ProductId=ptp.ProductId),0) OptionsVarMi," +
                    "          ISNULL(" +
                    "                   (SELECT top 1 ProductId" +
                    "                    FROM SablonChoice2" +
                    "                    WHERE ProductId=ptp.ProductId" +
                    "                      AND Choice1Id=ptp.Choice1Id),0) Choice2VarMi," +
                    "          p.ProductGroup," +
                    "          p.ProductName," +
                    "          ptp.Id," +
                    "          ptp.Price productprice," +
                    "          0 c1price," +
                    "          0 c2price," +
                    "          0 optionprice," +
                    "          ptp.ProductId," +
                    "          ptp.Choice1Id," +
                    "          ptp.Choice2Id," +
                    "          ptp.OptionsId," +
                    "		  ptp.IsManuelInsert," +
                    "         ptp.TemplateName," +
                    "          ptp.ProductTemplatePricePkId," +
                    "          p.ProductPkId," +
                    "          ptp.SubeId," +
                    "          ptp.SubeName,		" +
                    "          ch1.Name ChoiceProductName," +
                    "          ch2.Name Choice2ProductName," +
                    "          op.Name OptionsProductName," +
                    "          p.GuncellenecekSubeIdGrubu" +
                    "   FROM ProductTemplatePrice ptp" +
                    "   LEFT JOIN SablonProduct p ON p.ProductPkId=ptp.ProductId" +
                    "   LEFT JOIN SablonChoice1 ch1 ON ptp.ProductId=ch1.ProductId" +
                    "   AND ptp.Choice1Id=ch1.Choice1PkId" +
                    "   LEFT JOIN SablonChoice2 ch2 ON ptp.ProductId=ch2.ProductId" +
                    "   AND ptp.Choice2Id=ch2.Choice2PkId" +
                    "   LEFT JOIN SablonOptions op ON ptp.ProductId=op.ProductId" +
                    "   AND ptp.OptionsId=op.OptionsPkId" +
                    "   WHERE ptp.SubeId IS NOT NULL" +
                    "     AND ptp.Choice1Id=0" +
                    "     AND ptp.Choice2Id=0" +
                    "     AND ptp.OptionsId=0" +
                    "     AND ptp.GuncellenecekSubeIdGrubu='" + SubeIdGrupList + "'" +
                    "     AND ptp.TemplateName='" + sablonName + "' " +
                    "   GROUP BY p.ProductGroup," +
                    "            p.ProductName," +
                    "            ptp.Id," +
                    "            ptp.Price," +
                    "            ptp.ProductId," +
                    "            ptp.Choice1Id," +
                    "            ptp.Choice2Id," +
                    "            ptp.OptionsId," +
                    "			ptp.IsManuelInsert," +
                    "         ptp.TemplateName," +
                    "            ptp.ProductTemplatePricePkId," +
                    "            p.ProductPkId," +
                    "            ch1.Name," +
                    "            ptp.ProductId," +
                    "            ptp.SubeId," +
                    "            ptp.SubeName," +
                    "            ch2.Name," +
                    "            op.Name," +
                    "            p.GuncellenecekSubeIdGrubu" +
                    "   UNION ALL SELECT ISNULL(" +
                    "                             (SELECT top 1 ProductId" +
                    "                              FROM SablonChoice1" +
                    "                              WHERE ProductId=ptp.ProductId),0) Choice1VarMi," +
                    "                    ISNULL(" +
                    "                             (SELECT top 1 ProductId" +
                    "                              FROM SablonOptions" +
                    "                              WHERE ProductId=ptp.ProductId),0) OptionsVarMi," +
                    "                    ISNULL(" +
                    "                             (SELECT top 1 ProductId" +
                    "                              FROM SablonChoice2" +
                    "                              WHERE ProductId=ptp.ProductId" +
                    "                                AND Choice1Id=ptp.Choice1Id),0) Choice2VarMi," +
                    "                    p.ProductGroup," +
                    "                    p.ProductName," +
                    "                    ptp.Id," +
                    "                    0 productprice," +
                    "                    ptp.Price c1price," +
                    "                    0 c2price," +
                    "                    0 optionprice," +
                    "                    ptp.ProductId," +
                    "                    ptp.Choice1Id," +
                    "                    ptp.Choice2Id," +
                    "                    ptp.OptionsId," +
                    "					ptp.IsManuelInsert," +
                    "         ptp.TemplateName," +
                    "                    ptp.ProductTemplatePricePkId," +
                    "                    p.ProductPkId," +
                    "                    ptp.SubeId," +
                    "                    ptp.SubeName," +
                    "                    ch1.Name ChoiceProductName," +
                    "                    ch2.Name Choice2ProductName," +
                    "                    op.Name OptionsProductName," +
                    "                    p.GuncellenecekSubeIdGrubu" +
                    "   FROM ProductTemplatePrice ptp" +
                    "   LEFT JOIN SablonProduct p ON p.ProductPkId=ptp.ProductId" +
                    "   LEFT JOIN SablonChoice1 ch1 ON ptp.ProductId=ch1.ProductId" +
                    "   AND ptp.Choice1Id=ch1.Choice1PkId" +
                    "   LEFT JOIN SablonChoice2 ch2 ON ptp.ProductId=ch2.ProductId" +
                    "   AND ptp.Choice2Id=ch2.Choice2PkId" +
                    "   LEFT JOIN SablonOptions op ON ptp.ProductId=op.ProductId" +
                    "   AND ptp.OptionsId=op.OptionsPkId" +
                    "   WHERE ptp.SubeId IS NOT NULL" +
                    "     AND ptp.Choice1Id>0" +
                    "     AND ptp.Choice2Id=0" +
                    "     AND ptp.OptionsId=0" +
                    "     AND ptp.GuncellenecekSubeIdGrubu='" + SubeIdGrupList + "'" +
                    "     AND ptp.TemplateName='" + sablonName + "' " +
                    "   GROUP BY p.ProductGroup," +
                    "            p.ProductName," +
                    "            ptp.Id," +
                    "            ptp.Price," +
                    "            ptp.ProductId," +
                    "            ptp.Choice1Id," +
                    "            ptp.Choice2Id," +
                    "            ptp.OptionsId," +
                    "			ptp.IsManuelInsert," +
                    "         ptp.TemplateName," +
                    "            ptp.ProductTemplatePricePkId," +
                    "            p.ProductPkId," +
                    "            ch1.Name," +
                    "            ptp.ProductId," +
                    "            ptp.SubeId," +
                    "            ptp.SubeName," +
                    "            ch2.Name," +
                    "            op.Name," +
                    "            p.GuncellenecekSubeIdGrubu" +
                    "   UNION ALL SELECT ISNULL(" +
                    "                             (SELECT top 1 ProductId" +
                    "                              FROM SablonChoice1" +
                    "                              WHERE ProductId=ptp.ProductId),0) Choice1VarMi," +
                    "                    ISNULL(" +
                    "                             (SELECT top 1 ProductId" +
                    "                              FROM SablonOptions" +
                    "                              WHERE ProductId=ptp.ProductId),0) OptionsVarMi," +
                    "                    ISNULL(" +
                    "                             (SELECT top 1 ProductId" +
                    "                              FROM SablonChoice2" +
                    "                              WHERE ProductId=ptp.ProductId" +
                    "                                AND Choice1Id=ptp.Choice1Id),0) Choice2VarMi," +
                    "                    p.ProductGroup," +
                    "                    p.ProductName," +
                    "                    ptp.Id," +
                    "                    0 productprice," +
                    "                    0 c1price," +
                    "                    ptp.Price c2price," +
                    "                    0 optionprice," +
                    "                    ptp.ProductId," +
                    "                    ptp.Choice1Id," +
                    "                    ptp.Choice2Id," +
                    "                    ptp.OptionsId," +
                    "					ptp.IsManuelInsert," +
                    "         ptp.TemplateName," +
                    "                    ptp.ProductTemplatePricePkId," +
                    "                    p.ProductPkId," +
                    "                    ptp.SubeId," +
                    "                    ptp.SubeName," +
                    "                    ch1.Name ChoiceProductName," +
                    "                    ch2.Name Choice2ProductName," +
                    "                    op.Name OptionsProductName," +
                    "                    p.GuncellenecekSubeIdGrubu" +
                    "   FROM ProductTemplatePrice ptp" +
                    "   LEFT JOIN SablonProduct p ON p.ProductPkId=ptp.ProductId" +
                    "   LEFT JOIN SablonChoice1 ch1 ON ptp.ProductId=ch1.ProductId" +
                    "   AND ptp.Choice1Id=ch1.Choice1PkId" +
                    "   LEFT JOIN SablonChoice2 ch2 ON ptp.ProductId=ch2.ProductId" +
                    "   AND ptp.Choice2Id=ch2.Choice2PkId" +
                    "   LEFT JOIN SablonOptions op ON ptp.ProductId=op.ProductId" +
                    "   AND ptp.OptionsId=op.OptionsPkId" +
                    "   WHERE ptp.SubeId IS NOT NULL" +
                    "     AND ptp.Choice1Id>0" +
                    "     AND ptp.Choice2Id>0" +
                    "     AND ptp.OptionsId=0" +
                    "     AND ptp.GuncellenecekSubeIdGrubu='" + SubeIdGrupList + "'" +
                    "     AND ptp.TemplateName='" + sablonName + "' " +
                    "   GROUP BY p.ProductGroup," +
                    "            p.ProductName," +
                    "            ptp.Id," +
                    "            ptp.Price," +
                    "            ptp.ProductId," +
                    "            ptp.Choice1Id," +
                    "            ptp.Choice2Id," +
                    "            ptp.OptionsId," +
                    "			ptp.IsManuelInsert," +
                    "         ptp.TemplateName," +
                    "            ptp.ProductTemplatePricePkId," +
                    "            p.ProductPkId," +
                    "            ch1.Name," +
                    "            ptp.ProductId," +
                    "            ptp.SubeId," +
                    "            ptp.SubeName," +
                    "            ch2.Name," +
                    "            op.Name," +
                    "            p.GuncellenecekSubeIdGrubu" +
                    "   UNION ALL SELECT ISNULL(" +
                    "                             (SELECT top 1 ProductId" +
                    "                              FROM SablonChoice1" +
                    "                              WHERE ProductId=ptp.ProductId),0) Choice1VarMi," +
                    "                    ISNULL(" +
                    "                             (SELECT top 1 ProductId" +
                    "                              FROM SablonOptions" +
                    "                              WHERE ProductId=ptp.ProductId),0) OptionsVarMi," +
                    "                    ISNULL(" +
                    "                             (SELECT top 1 ProductId" +
                    "                              FROM SablonChoice2" +
                    "                              WHERE ProductId=ptp.ProductId" +
                    "                                AND Choice1Id=ptp.Choice1Id),0) Choice2VarMi," +
                    "                    p.ProductGroup," +
                    "                    p.ProductName," +
                    "                    ptp.Id," +
                    "                    0 productprice," +
                    "                    0 c1price," +
                    "                    0 c2price," +
                    "                    ptp.Price optionprice," +
                    "                    ptp.ProductId," +
                    "                    ptp.Choice1Id," +
                    "                    ptp.Choice2Id," +
                    "                    ptp.OptionsId," +
                    "					ptp.IsManuelInsert," +
                    "         ptp.TemplateName," +
                    "                    ptp.ProductTemplatePricePkId," +
                    "                    p.ProductPkId," +
                    "                    ptp.SubeId," +
                    "                    ptp.SubeName," +
                    "                    ch1.Name ChoiceProductName," +
                    "                    ch2.Name Choice2ProductName," +
                    "                    op.Name OptionsProductName," +
                    "                    p.GuncellenecekSubeIdGrubu" +
                    "   FROM ProductTemplatePrice ptp" +
                    "   LEFT JOIN SablonProduct p ON p.ProductPkId=ptp.ProductId" +
                    "   LEFT JOIN SablonChoice1 ch1 ON ptp.ProductId=ch1.ProductId" +
                    "   AND ptp.Choice1Id=ch1.Choice1PkId" +
                    "   LEFT JOIN SablonChoice2 ch2 ON ptp.ProductId=ch2.ProductId" +
                    "   AND ptp.Choice2Id=ch2.Choice2PkId" +
                    "   LEFT JOIN SablonOptions op ON ptp.ProductId=op.ProductId" +
                    "   AND ptp.OptionsId=op.OptionsPkId" +
                    "   WHERE ptp.SubeId IS NOT NULL" +
                    "     AND ptp.Choice1Id=0" +
                    "     AND ptp.Choice2Id=0" +
                    "     AND ptp.OptionsId>0" +
                    "     AND ptp.GuncellenecekSubeIdGrubu='" + SubeIdGrupList + "'" +
                    "     AND ptp.TemplateName='" + sablonName + "' " +
                    "   GROUP BY p.ProductGroup," +
                    "            p.ProductName," +
                    "            ptp.Id," +
                    "            ptp.Price," +
                    "            ptp.ProductId," +
                    "            ptp.Choice1Id," +
                    "            ptp.Choice2Id," +
                    "            ptp.OptionsId," +
                    "			ptp.IsManuelInsert," +
                    "         ptp.TemplateName," +
                    "            ptp.ProductTemplatePricePkId," +
                    "            p.ProductPkId," +
                    "            ch1.Name," +
                    "            ptp.ProductId," +
                    "            ptp.SubeId," +
                    "            ptp.SubeName," +
                    "            ch2.Name," +
                    "            op.Name," +
                    "            p.GuncellenecekSubeIdGrubu) t where  t.GuncellenecekSubeIdGrubu='" + SubeIdGrupList + "' AND t.TemplateName='" + sablonName + "' ";


            return query;
        }
        public static string getLocalSablonChoice1ListSqlQuery(string productGroup, string productName, string subeIdGrupList, string sablonName)
        {
            var query = (@"SELECT *
      FROM
  (SELECT ISNULL(
                   (SELECT top 1 ProductId
                    FROM SablonChoice1
                    WHERE ProductId=ptp.ProductId),0) Choice1VarMi,
          ISNULL(
                   (SELECT top 1 ProductId
                    FROM SablonOptions
                    WHERE ProductId=ptp.ProductId),0) OptionsVarMi,
          ISNULL(
                   (SELECT top 1 ProductId
                    FROM SablonChoice2
                    WHERE ProductId=ptp.ProductId
                      AND Choice1Id=ptp.Choice1Id
                      AND Choice2Id >0 ),0) Choice2VarMi,
          p.ProductGroup,
          p.ProductName,
          ptp.Id,
          ptp.Price productprice,
          0 c1price,
          0 c2price,
          0 optionprice,
          ptp.ProductId,
          ptp.Choice1Id,
          ptp.Choice2Id,
          ptp.OptionsId,
          ptp.IsManuelInsert,
          ptp.ProductTemplatePricePkId,
          p.ProductPkId,
          ptp.SubeId,
          ptp.SubeName,
          ch1.Name ChoiceProductName,
          ch2.Name Choice2ProductName,
          op.Name OptionsProductName,
          p.GuncellenecekSubeIdGrubu,
		  ptp.TemplateName
   FROM ProductTemplatePrice ptp
   LEFT JOIN SablonProduct p ON p.ProductPkId=ptp.ProductId
   LEFT JOIN SablonChoice1 ch1 ON ptp.ProductId=ch1.ProductId
   AND ptp.Choice1Id=ch1.Choice1PkId
AND ptp.SubeId=ch1.SubeId
   LEFT JOIN SablonChoice2 ch2 ON ptp.ProductId=ch2.ProductId
   AND ptp.Choice2Id=ch2.Choice2PkId
   LEFT JOIN SablonOptions op ON ptp.ProductId=op.ProductId
   AND ptp.OptionsId=op.OptionsPkId
   WHERE ptp.SubeId IS NOT NULL
     AND ptp.Choice1Id=0
     AND ptp.Choice2Id=0
     AND ptp.OptionsId=0
   GROUP BY p.ProductGroup,
            p.ProductName,
            ptp.Id,
            ptp.Price,
            ptp.ProductId,
            ptp.Choice1Id,
            ptp.Choice2Id,
            ptp.OptionsId,
            ptp.IsManuelInsert,
            ptp.ProductTemplatePricePkId,
            p.ProductPkId,
            ch1.Name,
            ptp.ProductId,
            ptp.SubeId,
            ptp.SubeName,
            ch2.Name,
            op.Name,
            p.GuncellenecekSubeIdGrubu,
			ptp.TemplateName
   UNION ALL SELECT ISNULL(
                             (SELECT top 1 ProductId
                              FROM SablonChoice1
                              WHERE ProductId=ptp.ProductId),0) Choice1VarMi,
                    ISNULL(
                             (SELECT top 1 ProductId
                              FROM SablonOptions
                              WHERE ProductId=ptp.ProductId),0) OptionsVarMi,
                    ISNULL(
                             (SELECT top 1 ProductId
                              FROM SablonChoice2
                              WHERE ProductId=ptp.ProductId
                                AND Choice1Id=ptp.Choice1Id
                                AND Choice2Id >0 ),0) Choice2VarMi,
                    p.ProductGroup,
                    p.ProductName,
                    ptp.Id,
                    0 productprice,
                    ptp.Price c1price,
                    0 c2price,
                    0 optionprice,
                    ptp.ProductId,
                    ptp.Choice1Id,
                    ptp.Choice2Id,
                    ptp.OptionsId,
                    ptp.IsManuelInsert,
                    ptp.ProductTemplatePricePkId,
                    p.ProductPkId,
                    ptp.SubeId,
                    ptp.SubeName,
                    ch1.Name ChoiceProductName,
                    ch2.Name Choice2ProductName,
                    op.Name OptionsProductName,
                    p.GuncellenecekSubeIdGrubu,
					ptp.TemplateName
   FROM ProductTemplatePrice ptp
   LEFT JOIN SablonProduct p ON p.ProductPkId=ptp.ProductId
   LEFT JOIN SablonChoice1 ch1 ON ptp.ProductId=ch1.ProductId
   AND ptp.Choice1Id=ch1.Choice1PkId
AND ptp.SubeId=ch1.SubeId
   LEFT JOIN SablonChoice2 ch2 ON ptp.ProductId=ch2.ProductId
   AND ptp.Choice2Id=ch2.Choice2PkId
   LEFT JOIN SablonOptions op ON ptp.ProductId=op.ProductId
   AND ptp.OptionsId=op.OptionsPkId
   WHERE ptp.SubeId IS NOT NULL
     AND ptp.Choice1Id>0
     AND ptp.Choice2Id=0
     AND ptp.OptionsId=0
     AND p.ProductGroup='" + productGroup + @" '
     AND p.ProductName = '" + productName + @"'
     AND ptp.GuncellenecekSubeIdGrubu = '" + subeIdGrupList + @"'
     and ptp.TemplateName = '" + sablonName + @"'
   GROUP BY p.ProductGroup,
            p.ProductName,
            ptp.Id,
            ptp.Price,
            ptp.ProductId,
            ptp.Choice1Id,
            ptp.Choice2Id,
            ptp.OptionsId,
            ptp.IsManuelInsert,
            ptp.ProductTemplatePricePkId,
            p.ProductPkId,
            ch1.Name,
            ptp.ProductId,
            ptp.SubeId,
            ptp.SubeName,
            ch2.Name,
            op.Name,
            p.GuncellenecekSubeIdGrubu,
			ptp.TemplateName
   UNION ALL SELECT ISNULL(
                             (SELECT top 1 ProductId
                              FROM SablonChoice1
                              WHERE ProductId = ptp.ProductId),0) Choice1VarMi,
                    ISNULL(
                             (SELECT top 1 ProductId
                              FROM SablonOptions
                              WHERE ProductId = ptp.ProductId),0) OptionsVarMi,
                    ISNULL(
                             (SELECT top 1 ProductId
                              FROM SablonChoice2
                              WHERE ProductId = ptp.ProductId
                                AND Choice1Id = ptp.Choice1Id
                                AND Choice2Id > 0),0) Choice2VarMi,
                    p.ProductGroup,
                    p.ProductName,
                    ptp.Id,
                    0 productprice,
                    0 c1price,
                    ptp.Price c2price,
                    0 optionprice,
                    ptp.ProductId,
                    ptp.Choice1Id,
                    ptp.Choice2Id,
                    ptp.OptionsId,
                    ptp.IsManuelInsert,
                    ptp.ProductTemplatePricePkId,
                    p.ProductPkId,
                    ptp.SubeId,
                    ptp.SubeName,
                    ch1.Name ChoiceProductName,
                    ch2.Name Choice2ProductName,
                    op.Name OptionsProductName,
                    p.GuncellenecekSubeIdGrubu,
					ptp.TemplateName
   FROM ProductTemplatePrice ptp
   LEFT JOIN SablonProduct p ON p.ProductPkId = ptp.ProductId
   LEFT JOIN SablonChoice1 ch1 ON ptp.ProductId = ch1.ProductId
   AND ptp.Choice1Id = ch1.Choice1PkId
AND ptp.SubeId=ch1.SubeId
   LEFT JOIN SablonChoice2 ch2 ON ptp.ProductId = ch2.ProductId
   AND ptp.Choice2Id = ch2.Choice2PkId
   LEFT JOIN SablonOptions op ON ptp.ProductId = op.ProductId
   AND ptp.OptionsId = op.OptionsPkId
   WHERE ptp.SubeId IS NOT NULL
     AND ptp.Choice1Id > 0
     AND ptp.Choice2Id > 0
     AND ptp.OptionsId = 0
     AND p.ProductGroup='" + productGroup + @" '
     AND p.ProductName = '" + productName + @"'
     AND ptp.GuncellenecekSubeIdGrubu = '" + subeIdGrupList + @"'
     and ptp.TemplateName = '" + sablonName + @"'
   GROUP BY p.ProductGroup,
            p.ProductName,
            ptp.Id,
            ptp.Price,
            ptp.ProductId,
            ptp.Choice1Id,
            ptp.Choice2Id,
            ptp.OptionsId,
            ptp.IsManuelInsert,
            ptp.ProductTemplatePricePkId,
            p.ProductPkId,
            ch1.Name,
            ptp.ProductId,
            ptp.SubeId,
            ptp.SubeName,
            ch2.Name,
            op.Name,
            p.GuncellenecekSubeIdGrubu,
			ptp.TemplateName
   UNION ALL SELECT ISNULL(
                             (SELECT top 1 ProductId
                              FROM SablonChoice1
                              WHERE ProductId = ptp.ProductId),0) Choice1VarMi,
                    ISNULL(
                             (SELECT top 1 ProductId
                              FROM SablonOptions
                              WHERE ProductId = ptp.ProductId),0) OptionsVarMi,
                    ISNULL(
                             (SELECT top 1 ProductId
                              FROM SablonChoice2
                              WHERE ProductId = ptp.ProductId
                                AND Choice1Id = ptp.Choice1Id
                                AND Choice2Id > 0),0) Choice2VarMi,
                    p.ProductGroup,
                    p.ProductName,
                    ptp.Id,
                    0 productprice,
                    0 c1price,
                    0 c2price,
                    ptp.Price optionprice,
                    ptp.ProductId,
                    ptp.Choice1Id,
                    ptp.Choice2Id,
                    ptp.OptionsId,
                    ptp.IsManuelInsert,
                    ptp.ProductTemplatePricePkId,
                    p.ProductPkId,
                    ptp.SubeId,
                    ptp.SubeName,
                    ch1.Name ChoiceProductName,
                    ch2.Name Choice2ProductName,
                    op.Name OptionsProductName,
                    p.GuncellenecekSubeIdGrubu,
					ptp.TemplateName
   FROM ProductTemplatePrice ptp
   LEFT JOIN SablonProduct p ON p.ProductPkId = ptp.ProductId
   LEFT JOIN SablonChoice1 ch1 ON ptp.ProductId = ch1.ProductId
   AND ptp.Choice1Id = ch1.Choice1PkId
AND ptp.SubeId=ch1.SubeId
   LEFT JOIN SablonChoice2 ch2 ON ptp.ProductId = ch2.ProductId
   AND ptp.Choice2Id = ch2.Choice2PkId
   LEFT JOIN SablonOptions op ON ptp.ProductId = op.ProductId
   AND ptp.OptionsId = op.OptionsPkId
   WHERE ptp.SubeId IS NOT NULL
     AND ptp.Choice1Id = 0
     AND ptp.Choice2Id = 0
     AND ptp.OptionsId > 0
     AND p.ProductGroup='" + productGroup + @" '
     AND p.ProductName = '" + productName + @"'
     AND ptp.GuncellenecekSubeIdGrubu = '" + subeIdGrupList + @"'
     and ptp.TemplateName = '" + sablonName + @"'
   GROUP BY p.ProductGroup,
            p.ProductName,
            ptp.Id,
            ptp.Price,
            ptp.ProductId,
            ptp.Choice1Id,
            ptp.Choice2Id,
            ptp.OptionsId,
            ptp.IsManuelInsert,
            ptp.ProductTemplatePricePkId,
            p.ProductPkId,
            ch1.Name,
            ptp.ProductId,
            ptp.SubeId,
            ptp.SubeName,
            ch2.Name,
            op.Name,
            p.GuncellenecekSubeIdGrubu,
			ptp.TemplateName
			) t
WHERE t.ProductGroup = '" + productGroup + @"'
  AND t.ProductName ='" + productName + @"' AND t.GuncellenecekSubeIdGrubu = '" + subeIdGrupList + "'  AND t.TemplateName ='" + sablonName + @"'");

            return query;
        }
        public static string getUpdatedSablonChoice1ListSqlQuery(string productGroup, string productName, string subeIdGrupList, string sablonName)
        {
            var query = (@"SELECT * " +
"FROM" +
"  (SELECT ISNULL(" +
"                   (SELECT top 1 ProductId" +
"                    FROM SablonChoice1" +
"                    WHERE ProductId=ptp.ProductId),0) Choice1VarMi," +
"          ISNULL(" +
"                   (SELECT top 1 ProductId" +
"                    FROM SablonOptions" +
"                    WHERE ProductId=ptp.ProductId),0) OptionsVarMi," +
"          ISNULL(" +
"                   (SELECT top 1 ProductId" +
"                    FROM SablonChoice2" +
"                    WHERE ProductId=ptp.ProductId" +
"                      AND Choice1Id=ptp.Choice1Id" +
"                      AND Choice2Id >0 ),0) Choice2VarMi," +
"          p.ProductGroup," +
"          p.ProductName," +
"          ptp.Id," +
"          ptp.Price productprice," +
"          0 c1price," +
"          0 c2price," +
"          0 optionprice," +
"          ptp.ProductId," +
"          ptp.Choice1Id," +
"          ptp.Choice2Id," +
"          ptp.OptionsId," +
"          ptp.ProductTemplatePricePkId," +
"          p.ProductPkId," +
"          ptp.SubeId," +
"          ptp.SubeName," +
"          ch1.Name ChoiceProductName," +
"          ch2.Name Choice2ProductName," +
"          op.Name OptionsProductName," +
"		  ptp.GuncellenecekSubeIdGrubu," +
"	      ptp.TemplateName" +
"   FROM ProductTemplatePrice ptp" +
"   LEFT JOIN SablonProduct p ON p.ProductPkId=ptp.ProductId" +
"   LEFT JOIN SablonChoice1 ch1 ON ptp.ProductId=ch1.ProductId" +
"   AND ptp.Choice1Id=ch1.Choice1PkId" +
"   LEFT JOIN SablonChoice2 ch2 ON ptp.ProductId=ch2.ProductId" +
"   AND ptp.Choice2Id=ch2.Choice2PkId" +
"   LEFT JOIN SablonOptions op ON ptp.ProductId=op.ProductId" +
"   AND ptp.OptionsId=op.OptionsPkId" +
"   WHERE ptp.SubeId IS NOT NULL" +
"     AND ptp.Choice1Id=0" +
"     AND ptp.Choice2Id=0" +
"     AND ptp.OptionsId=0" +
"   GROUP BY p.ProductGroup," +
"            p.ProductName," +
"            ptp.Id," +
"            ptp.Price," +
"            ptp.ProductId," +
"            ptp.Choice1Id," +
"            ptp.Choice2Id," +
"            ptp.OptionsId," +
"            ptp.ProductTemplatePricePkId," +
"            p.ProductPkId," +
"            ch1.Name," +
"            ptp.ProductId," +
"            ptp.SubeId," +
"            ptp.SubeName," +
"            ch2.Name," +
"            op.Name," +
"           ptp.GuncellenecekSubeIdGrubu," +
"	      ptp.TemplateName" +
"   UNION ALL SELECT ISNULL(" +
"                             (SELECT top 1 ProductId" +
"                              FROM SablonChoice1" +
"                              WHERE ProductId=ptp.ProductId),0) Choice1VarMi," +
"                    ISNULL(" +
"                             (SELECT top 1 ProductId" +
"                              FROM SablonOptions" +
"                              WHERE ProductId=ptp.ProductId),0) OptionsVarMi," +
"                    ISNULL(" +
"                             (SELECT top 1 ProductId" +
"                              FROM SablonChoice2" +
"                              WHERE ProductId=ptp.ProductId" +
"                                AND Choice1Id=ptp.Choice1Id" +
"                                AND Choice2Id >0 ),0) Choice2VarMi," +
"                    p.ProductGroup," +
"                    p.ProductName," +
"                    ptp.Id," +
"                    0 productprice," +
"                    ptp.Price c1price," +
"                    0 c2price," +
"                    0 optionprice," +
"                    ptp.ProductId," +
"                    ptp.Choice1Id," +
"                    ptp.Choice2Id," +
"                    ptp.OptionsId," +
"                    ptp.ProductTemplatePricePkId," +
"                    p.ProductPkId," +
"                    ptp.SubeId," +
"                    ptp.SubeName," +
"                    ch1.Name ChoiceProductName," +
"                    ch2.Name Choice2ProductName," +
"                    op.Name OptionsProductName," +
"                   ptp.GuncellenecekSubeIdGrubu," +
"	               ptp.TemplateName" +
"   FROM ProductTemplatePrice ptp" +
"   LEFT JOIN SablonProduct p ON p.ProductPkId=ptp.ProductId" +
"   LEFT JOIN SablonChoice1 ch1 ON ptp.ProductId=ch1.ProductId" +
"   AND ptp.Choice1Id=ch1.Choice1PkId" +
"   LEFT JOIN SablonChoice2 ch2 ON ptp.ProductId=ch2.ProductId" +
"   AND ptp.Choice2Id=ch2.Choice2PkId" +
"   LEFT JOIN SablonOptions op ON ptp.ProductId=op.ProductId" +
"   AND ptp.OptionsId=op.OptionsPkId" +
"   WHERE ptp.SubeId IS NOT NULL" +
"     AND ptp.Choice1Id>0" +
"     AND ptp.Choice2Id=0" +
"     AND ptp.OptionsId=0" +
"     AND p.ProductGroup='" + productGroup + "'" +
"     AND p.ProductName='" + productName + "'" +
"	 AND ptp.GuncellenecekSubeIdGrubu = '" + subeIdGrupList + "'" +
"     and ptp.TemplateName = '" + sablonName + "'" +
"   GROUP BY p.ProductGroup," +
"            p.ProductName," +
"            ptp.Id," +
"            ptp.Price," +
"            ptp.ProductId," +
"            ptp.Choice1Id," +
"            ptp.Choice2Id," +
"            ptp.OptionsId," +
"            ptp.ProductTemplatePricePkId," +
"            p.ProductPkId," +
"            ch1.Name," +
"            ptp.ProductId," +
"            ptp.SubeId," +
"            ptp.SubeName," +
"            ch2.Name," +
"            op.Name," +
"			 ptp.GuncellenecekSubeIdGrubu," +
"	        ptp.TemplateName" +
"   UNION ALL SELECT ISNULL(" +
"                             (SELECT top 1 ProductId" +
"                              FROM SablonChoice1" +
"                              WHERE ProductId=ptp.ProductId),0) Choice1VarMi," +
"                    ISNULL(" +
"                             (SELECT top 1 ProductId" +
"                              FROM SablonOptions" +
"                              WHERE ProductId=ptp.ProductId),0) OptionsVarMi," +
"                    ISNULL(" +
"                             (SELECT top 1 ProductId" +
"                              FROM SablonChoice2" +
"                              WHERE ProductId=ptp.ProductId" +
"                                AND Choice1Id=ptp.Choice1Id" +
"                                AND Choice2Id >0 ),0) Choice2VarMi," +
"                    p.ProductGroup," +
"                    p.ProductName," +
"                    ptp.Id," +
"                    0 productprice," +
"                    0 c1price," +
"                    ptp.Price c2price," +
"                    0 optionprice," +
"                    ptp.ProductId," +
"                    ptp.Choice1Id," +
"                    ptp.Choice2Id," +
"                    ptp.OptionsId," +
"                    ptp.ProductTemplatePricePkId," +
"                    p.ProductPkId," +
"                    ptp.SubeId," +
"                    ptp.SubeName," +
"                    ch1.Name ChoiceProductName," +
"                    ch2.Name Choice2ProductName," +
"                    op.Name OptionsProductName," +
"					ptp.GuncellenecekSubeIdGrubu," +
"	                ptp.TemplateName" +
"   FROM ProductTemplatePrice ptp" +
"   LEFT JOIN SablonProduct p ON p.ProductPkId=ptp.ProductId" +
"   LEFT JOIN SablonChoice1 ch1 ON ptp.ProductId=ch1.ProductId" +
"   AND ptp.Choice1Id=ch1.Choice1PkId" +
"   LEFT JOIN SablonChoice2 ch2 ON ptp.ProductId=ch2.ProductId" +
"   AND ptp.Choice2Id=ch2.Choice2PkId" +
"   LEFT JOIN SablonOptions op ON ptp.ProductId=op.ProductId" +
"   AND ptp.OptionsId=op.OptionsPkId" +
"   WHERE ptp.SubeId IS NOT NULL" +
"     AND ptp.Choice1Id>0" +
"     AND ptp.Choice2Id>0" +
"     AND ptp.OptionsId=0" +
"     AND p.ProductGroup='" + productGroup + "'" +
"     AND p.ProductName='" + productName + "'" +
"	 AND ptp.GuncellenecekSubeIdGrubu = '" + subeIdGrupList + "'" +
"     and ptp.TemplateName = '" + sablonName + "'" +
"   GROUP BY p.ProductGroup," +
"            p.ProductName," +
"            ptp.Id," +
"            ptp.Price," +
"            ptp.ProductId," +
"            ptp.Choice1Id," +
"            ptp.Choice2Id," +
"            ptp.OptionsId," +
"            ptp.ProductTemplatePricePkId," +
"            p.ProductPkId," +
"            ch1.Name," +
"            ptp.ProductId," +
"            ptp.SubeId," +
"            ptp.SubeName," +
"            ch2.Name," +
"            op.Name," +
"			 ptp.GuncellenecekSubeIdGrubu," +
"	      ptp.TemplateName" +
"   UNION ALL SELECT ISNULL(" +
"                             (SELECT top 1 ProductId" +
"                              FROM SablonChoice1" +
"                              WHERE ProductId=ptp.ProductId),0) Choice1VarMi," +
"                    ISNULL(" +
"                             (SELECT top 1 ProductId" +
"                              FROM SablonOptions" +
"                              WHERE ProductId=ptp.ProductId),0) OptionsVarMi," +
"                    ISNULL(" +
"                             (SELECT top 1 ProductId" +
"                              FROM SablonChoice2" +
"                              WHERE ProductId=ptp.ProductId" +
"                                AND Choice1Id=ptp.Choice1Id" +
"                                AND Choice2Id >0 ),0) Choice2VarMi," +
"                    p.ProductGroup," +
"                    p.ProductName," +
"                    ptp.Id," +
"                    0 productprice," +
"                    0 c1price," +
"                    0 c2price," +
"                    ptp.Price optionprice," +
"                    ptp.ProductId," +
"                    ptp.Choice1Id," +
"                    ptp.Choice2Id," +
"                    ptp.OptionsId," +
"                    ptp.ProductTemplatePricePkId," +
"                    p.ProductPkId," +
"                    ptp.SubeId," +
"                    ptp.SubeName," +
"                    ch1.Name ChoiceProductName," +
"                    ch2.Name Choice2ProductName," +
"                    op.Name OptionsProductName," +
"					 ptp.GuncellenecekSubeIdGrubu," +
"	      ptp.TemplateName" +
"   FROM ProductTemplatePrice ptp" +
"   LEFT JOIN SablonProduct p ON p.ProductPkId=ptp.ProductId" +
"   LEFT JOIN SablonChoice1 ch1 ON ptp.ProductId=ch1.ProductId" +
"   AND ptp.Choice1Id=ch1.Choice1PkId" +
"   LEFT JOIN SablonChoice2 ch2 ON ptp.ProductId=ch2.ProductId" +
"   AND ptp.Choice2Id=ch2.Choice2PkId" +
"   LEFT JOIN SablonOptions op ON ptp.ProductId=op.ProductId" +
"   AND ptp.OptionsId=op.OptionsPkId" +
"   WHERE ptp.SubeId IS NOT NULL" +
"     AND ptp.Choice1Id=0" +
"     AND ptp.Choice2Id=0" +
"     AND ptp.OptionsId>0" +
"     AND p.ProductGroup='" + productGroup + "'" +
"     AND p.ProductName='" + productName + "'" +
"	 AND ptp.GuncellenecekSubeIdGrubu = '" + subeIdGrupList + "'" +
"     and ptp.TemplateName = '" + sablonName + "'" +
"   GROUP BY p.ProductGroup," +
"            p.ProductName," +
"            ptp.Id," +
"            ptp.Price," +
"            ptp.ProductId," +
"            ptp.Choice1Id," +
"            ptp.Choice2Id," +
"            ptp.OptionsId," +
"            ptp.ProductTemplatePricePkId," +
"            p.ProductPkId," +
"            ch1.Name," +
"            ptp.ProductId," +
"            ptp.SubeId," +
"            ptp.SubeName," +
"            ch2.Name," +
"            op.Name," +
"			 ptp.GuncellenecekSubeIdGrubu," +
"	      ptp.TemplateName" +
"			" +
"			) t " +
" WHERE t.ProductGroup='" + productGroup + "'" +
"  AND t.ProductName='" + productName + "'" +
"  AND t.ChoiceProductName IS NOT NULL" +
"  and t.TemplateName='" + sablonName + "'" +
"  AND t.GuncellenecekSubeIdGrubu='" + subeIdGrupList + "'" +
"ORDER BY t.ChoiceProductName");


            return query;
        }
        public static string getUpdatedSablonOptionsListSqlQuery(string productGroup, string productName, string subeIdGrupList, string sablonName)
        {
            return (@"SELECT * " +
" FROM " +
"  (SELECT ISNULL( " +
"                   (SELECT top 1 ProductId" +
"                    FROM SablonChoice1" +
"                    WHERE ProductId=ptp.ProductId),0) Choice1VarMi," +
"          ISNULL(" +
"                   (SELECT top 1 ProductId" +
"                    FROM SablonOptions" +
"                    WHERE ProductId=ptp.ProductId),0) OptionsVarMi," +
"          ISNULL(" +
"                   (SELECT top 1 ProductId" +
"                    FROM SablonChoice2" +
"                    WHERE ProductId=ptp.ProductId" +
"                      AND Choice1Id=ptp.Choice1Id and Choice2Id >0 ),0) Choice2VarMi," +
"          p.ProductGroup," +
"          p.ProductName," +
"          ptp.Id," +
"          ptp.Price productprice," +
"          0 c1price," +
"          0 c2price," +
"          0 optionprice," +
"          ptp.ProductId," +
"          ptp.Choice1Id," +
"          ptp.Choice2Id," +
"          ptp.OptionsId," +
"          ptp.ProductTemplatePricePkId," +
"          p.ProductPkId," +
"          ptp.SubeId," +
"          ptp.SubeName," +
"          ch1.Name ChoiceProductName," +
"          ch2.Name Choice2ProductName," +
"          op.Name OptionsProductName," +
"		  ptp.GuncellenecekSubeIdGrubu," +
"	      ptp.TemplateName" +
"   FROM ProductTemplatePrice ptp" +
"   LEFT JOIN SablonProduct p ON p.ProductPkId=ptp.ProductId" +
"   LEFT JOIN SablonChoice1 ch1 ON ptp.ProductId=ch1.ProductId" +
"   AND ptp.Choice1Id=ch1.Choice1PkId" +
"   LEFT JOIN SablonChoice2 ch2 ON ptp.ProductId=ch2.ProductId" +
"   AND ptp.Choice2Id=ch2.Choice2PkId" +
"   LEFT JOIN SablonOptions op ON ptp.ProductId=op.ProductId" +
"   AND ptp.OptionsId=op.OptionsPkId" +
"   WHERE ptp.SubeId IS NOT NULL" +
"     AND ptp.Choice1Id=0" +
"     AND ptp.Choice2Id=0" +
"     AND ptp.OptionsId=0" +
"   GROUP BY p.ProductGroup," +
"            p.ProductName," +
"            ptp.Id," +
"            ptp.Price," +
"            ptp.ProductId," +
"            ptp.Choice1Id," +
"            ptp.Choice2Id," +
"            ptp.OptionsId," +
"            ptp.ProductTemplatePricePkId," +
"            p.ProductPkId," +
"            ch1.Name," +
"            ptp.ProductId," +
"            ptp.SubeId," +
"            ptp.SubeName," +
"            ch2.Name," +
"            op.Name," +
"		  ptp.GuncellenecekSubeIdGrubu," +
"	      ptp.TemplateName" +
"   UNION ALL SELECT ISNULL(" +
"                             (SELECT top 1 ProductId" +
"                              FROM SablonChoice1" +
"                              WHERE ProductId=ptp.ProductId),0) Choice1VarMi," +
"                    ISNULL(" +
"                             (SELECT top 1 ProductId" +
"                              FROM SablonOptions" +
"                              WHERE ProductId=ptp.ProductId),0) OptionsVarMi," +
"                    ISNULL(" +
"                             (SELECT top 1 ProductId" +
"                              FROM SablonChoice2" +
"                              WHERE ProductId=ptp.ProductId" +
"                                AND Choice1Id=ptp.Choice1Id and Choice2Id >0  ),0) Choice2VarMi," +
"                    p.ProductGroup," +
"                    p.ProductName," +
"                    ptp.Id," +
"                    0 productprice," +
"                    ptp.Price c1price," +
"                    0 c2price," +
"                    0 optionprice," +
"                    ptp.ProductId," +
"                    ptp.Choice1Id," +
"                    ptp.Choice2Id," +
"                    ptp.OptionsId," +
"                    ptp.ProductTemplatePricePkId," +
"                    p.ProductPkId," +
"                    ptp.SubeId," +
"                    ptp.SubeName," +
"                    ch1.Name ChoiceProductName," +
"                    ch2.Name Choice2ProductName," +
"                    op.Name OptionsProductName," +
"		  ptp.GuncellenecekSubeIdGrubu," +
"	      ptp.TemplateName" +
"   FROM ProductTemplatePrice ptp" +
"   LEFT JOIN SablonProduct p ON p.ProductPkId=ptp.ProductId" +
"   LEFT JOIN SablonChoice1 ch1 ON ptp.ProductId=ch1.ProductId" +
"   AND ptp.Choice1Id=ch1.Choice1PkId" +
"   LEFT JOIN SablonChoice2 ch2 ON ptp.ProductId=ch2.ProductId" +
"   AND ptp.Choice2Id=ch2.Choice2PkId" +
"   LEFT JOIN SablonOptions op ON ptp.ProductId=op.ProductId" +
"   AND ptp.OptionsId=op.OptionsPkId" +
"   WHERE ptp.SubeId IS NOT NULL" +
"     AND ptp.Choice1Id>0" +
"     AND ptp.Choice2Id=0" +
"     AND ptp.OptionsId=0" +
"     AND p.ProductGroup='" + productGroup + "'" +
"     AND p.ProductName='" + productName + "'" +
"	 AND ptp.GuncellenecekSubeIdGrubu = '" + subeIdGrupList + "'" +
"     and ptp.TemplateName = '" + sablonName + "'" +
"   GROUP BY p.ProductGroup," +
"            p.ProductName," +
"            ptp.Id," +
"            ptp.Price," +
"            ptp.ProductId," +
"            ptp.Choice1Id," +
"            ptp.Choice2Id," +
"            ptp.OptionsId," +
"            ptp.ProductTemplatePricePkId," +
"            p.ProductPkId," +
"            ch1.Name," +
"            ptp.ProductId," +
"            ptp.SubeId," +
"            ptp.SubeName," +
"            ch2.Name," +
"            op.Name," +
"		  ptp.GuncellenecekSubeIdGrubu," +
"	      ptp.TemplateName" +
"   UNION ALL SELECT ISNULL(" +
"                             (SELECT top 1 ProductId" +
"                              FROM SablonChoice1" +
"                              WHERE ProductId=ptp.ProductId),0) Choice1VarMi," +
"                    ISNULL(" +
"                             (SELECT top 1 ProductId" +
"                              FROM SablonOptions" +
"                              WHERE ProductId=ptp.ProductId),0) OptionsVarMi," +
"                    ISNULL(" +
"                             (SELECT top 1 ProductId" +
"                              FROM SablonChoice2" +
"                              WHERE ProductId=ptp.ProductId" +
"                                AND Choice1Id=ptp.Choice1Id and Choice2Id >0  ),0) Choice2VarMi," +
"                    p.ProductGroup," +
"                    p.ProductName," +
"                    ptp.Id," +
"                    0 productprice," +
"                    0 c1price," +
"                    ptp.Price c2price," +
"                    0 optionprice," +
"                    ptp.ProductId," +
"                    ptp.Choice1Id," +
"                    ptp.Choice2Id," +
"                    ptp.OptionsId," +
"                    ptp.ProductTemplatePricePkId," +
"                    p.ProductPkId," +
"                    ptp.SubeId," +
"                    ptp.SubeName," +
"                    ch1.Name ChoiceProductName," +
"                    ch2.Name Choice2ProductName," +
"                    op.Name OptionsProductName," +
"		  ptp.GuncellenecekSubeIdGrubu," +
"	      ptp.TemplateName" +
"   FROM ProductTemplatePrice ptp" +
"   LEFT JOIN SablonProduct p ON p.ProductPkId=ptp.ProductId" +
"   LEFT JOIN SablonChoice1 ch1 ON ptp.ProductId=ch1.ProductId" +
"   AND ptp.Choice1Id=ch1.Choice1PkId" +
"   LEFT JOIN SablonChoice2 ch2 ON ptp.ProductId=ch2.ProductId" +
"   AND ptp.Choice2Id=ch2.Choice2PkId" +
"   LEFT JOIN SablonOptions op ON ptp.ProductId=op.ProductId" +
"   AND ptp.OptionsId=op.OptionsPkId" +
"   WHERE ptp.SubeId IS NOT NULL" +
"     AND ptp.Choice1Id>0" +
"     AND ptp.Choice2Id>0" +
"     AND ptp.OptionsId=0" +
"     AND p.ProductGroup='" + productGroup + "'" +
"     AND p.ProductName='" + productName + "'" +
"	 AND ptp.GuncellenecekSubeIdGrubu = '" + subeIdGrupList + "'" +
"     and ptp.TemplateName = '" + sablonName + "'" +
"   GROUP BY p.ProductGroup," +
"            p.ProductName," +
"            ptp.Id," +
"            ptp.Price," +
"            ptp.ProductId," +
"            ptp.Choice1Id," +
"            ptp.Choice2Id," +
"            ptp.OptionsId," +
"            ptp.ProductTemplatePricePkId," +
"            p.ProductPkId," +
"            ch1.Name," +
"            ptp.ProductId," +
"            ptp.SubeId," +
"            ptp.SubeName," +
"            ch2.Name," +
"            op.Name," +
"		  ptp.GuncellenecekSubeIdGrubu," +
"	      ptp.TemplateName" +
"   UNION ALL SELECT ISNULL(" +
"                             (SELECT top 1 ProductId" +
"                              FROM SablonChoice1" +
"                              WHERE ProductId=ptp.ProductId),0) Choice1VarMi," +
"                    ISNULL(" +
"                             (SELECT top 1 ProductId" +
"                              FROM SablonOptions" +
"                              WHERE ProductId=ptp.ProductId),0) OptionsVarMi," +
"                    ISNULL(" +
"                             (SELECT top 1 ProductId" +
"                              FROM SablonChoice2" +
"                              WHERE ProductId=ptp.ProductId" +
"                                AND Choice1Id=ptp.Choice1Id and Choice2Id >0  ),0) Choice2VarMi," +
"                    p.ProductGroup," +
"                    p.ProductName," +
"                    ptp.Id," +
"                    0 productprice," +
"                    0 c1price," +
"                    0 c2price," +
"                    ptp.Price optionprice," +
"                    ptp.ProductId," +
"                    ptp.Choice1Id," +
"                    ptp.Choice2Id," +
"                    ptp.OptionsId," +
"                    ptp.ProductTemplatePricePkId," +
"                    p.ProductPkId," +
"                    ptp.SubeId," +
"                    ptp.SubeName," +
"                    ch1.Name ChoiceProductName," +
"                    ch2.Name Choice2ProductName," +
"                    op.Name OptionsProductName," +
"		  ptp.GuncellenecekSubeIdGrubu," +
"	      ptp.TemplateName" +
"   FROM ProductTemplatePrice ptp" +
"   LEFT JOIN SablonProduct p ON p.ProductPkId=ptp.ProductId" +
"   LEFT JOIN SablonChoice1 ch1 ON ptp.ProductId=ch1.ProductId" +
"   AND ptp.Choice1Id=ch1.Choice1PkId" +
"   LEFT JOIN SablonChoice2 ch2 ON ptp.ProductId=ch2.ProductId" +
"   AND ptp.Choice2Id=ch2.Choice2PkId" +
"   LEFT JOIN SablonOptions op ON ptp.ProductId=op.ProductId" +
"   AND ptp.OptionsId=op.OptionsPkId" +
"   WHERE ptp.SubeId IS NOT NULL" +
"     AND ptp.Choice1Id=0" +
"     AND ptp.Choice2Id=0" +
"     AND ptp.OptionsId>0" +
"     AND p.ProductGroup='" + productGroup + "'" +
"     AND p.ProductName='" + productName + "'" +
"	 AND ptp.GuncellenecekSubeIdGrubu = '" + subeIdGrupList + "'" +
"     and ptp.TemplateName = '" + sablonName + "'" +
"   GROUP BY p.ProductGroup," +
"            p.ProductName," +
"            ptp.Id," +
"            ptp.Price," +
"            ptp.ProductId," +
"            ptp.Choice1Id," +
"            ptp.Choice2Id," +
"            ptp.OptionsId," +
"            ptp.ProductTemplatePricePkId," +
"            p.ProductPkId," +
"            ch1.Name," +
"            ptp.ProductId," +
"            ptp.SubeId," +
"            ptp.SubeName," +
"            ch2.Name," +
"            op.Name," +
"		  ptp.GuncellenecekSubeIdGrubu," +
"	      ptp.TemplateName" +
") t where t.ProductGroup='" + productGroup + "' and t.ProductName='" + productName + "' And t.OptionsProductName is not null  " +
"  and t.TemplateName='" + sablonName + "'" +
"  AND t.GuncellenecekSubeIdGrubu='" + subeIdGrupList + "'" +
"ORDER BY t.ChoiceProductName");

        }
        //IsUpdated Şubelere yayma.
        public static string getProductSablonJoinSubesettingsSqlQuery()
        {
            var sqlQuery = @"Select p.SubeId, ss.DBName,ss.SqlName,ss.SqlPassword,ss.SubeIP   from SubeSettings ss
                        inner join Product p on p.SubeId =ss.Id
                        group by ss.DBName,ss.SqlName,ss.SqlPassword,ss.SubeIP,p.SubeId
                        order by p.SubeId
                        ";
            return sqlQuery;
        }

        //Şablonun ürünleri yoksa durumu için 
        public static string getForSablonProductChoiceChoice2OptionsSqlQuery()
        {
            var sqlQuery = @"
                                select 
                                t.ProductId,
                                t.ProductName,
                                t.ProductGroup,
                                t.Choice1Id,
                                t.Choice2Id,
                                t.OptionId,
                                t.Price
                                from (
                                select 
                                p.Id ProductId, p.ProductGroup, p.ProductName ProductName,
                                0  Choice1Id,'' Choice1Name,0 Choice2Id,'' Choice2Name,
                                0 OptionId,'' OptionsName, p.Price
                                from
                                Product p 

                                UNION ALL

                                select 
                                p.Id ProductId, p.ProductGroup, p.ProductName ProductName,
                                c1.Id Choice1Id,c1.Name Choice1Name,0 Choice2Id,'' Choice2Name,
                                0 OptionId,'' OptionsName, c1.Price
                                from
                                Product p 
                                inner join Choice1 c1 on p.Id=c1.ProductId

                                UNION ALL

                                select 
                                p.Id ProductId, p.ProductGroup, p.ProductName ProductName,
                                0  Choice1Id,'' Choice1Name,c2.Id Choice2Id,c2.Name Choice2Name,
                                0 OptionId,'' OptionsName, c2.Price
                                from
                                Product p 
                                inner join Choice1 c1 on p.Id=c1.ProductId
                                inner join Choice2 c2 on c1.Id=c2.Choice1Id and c1.ProductId=c2.ProductId

                                UNION ALL

                                select 
                                p.Id ProductId, p.ProductGroup, p.ProductName ProductName,
                                0  Choice1Id,'' Choice1Name,0 Choice2Id,'' Choice2Name,
                                o.Id OptionId,o.Name OptionsName, O.Price
                                from
                                Product p 
                                inner join Options O on O.ProductId=P.Id
                                ) 
                                t
                                group by 
                                ProductId,
                                t.ProductName,
                                t.ProductGroup,
                                t.Choice1Id,
                                t.Choice2Id,
                                t.OptionId,
                                t.Price ";
            return sqlQuery;
        }

        public static string getForSablonProductChoiceChoice2OptionsSqlQuery2(string kaynakDb, string sablonName, string subeId, string guncellenecekSubeIdGrubu)
        {
            var sqlQuery = @"Select * From (Select 
                                 t.ProductId,
                                 t.ProductName,
                                 t.ProductGroup,
                                 t.Choice1Id,
                                 t.Choice2Id,
                                 t.OptionId,
                                 t.Price
                                From (
                                Select 
                                 p.Id ProductId, p.ProductGroup, p.ProductName ProductName,
                                 0  Choice1Id,'' Choice1Name,0 Choice2Id,'' Choice2Name,
                                 0 OptionId,'' OptionsName, p.Price
                                From
                                 Product p 

                                UNION ALL

                                Select 
                                 p.Id ProductId, p.ProductGroup, p.ProductName ProductName,
                                 c1.Id Choice1Id,c1.Name Choice1Name,0 Choice2Id,'' Choice2Name,
                                 0 OptionId,'' OptionsName, c1.Price
                                From
                                 Product p 
                                inner join Choice1 c1 on p.Id=c1.ProductId

                                UNION ALL

                                Select 
                                 p.Id ProductId, p.ProductGroup, p.ProductName ProductName,
                                 0  Choice1Id,'' Choice1Name,c2.Id Choice2Id,c2.Name Choice2Name,
                                 0 OptionId,'' OptionsName, c2.Price
                                From
                                 Product p 
                                inner join Choice1 c1 on p.Id=c1.ProductId
                                inner join Choice2 c2 on c1.Id=c2.Choice1Id and c1.ProductId=c2.ProductId

                                UNION ALL

                                Select 
                                 p.Id ProductId, p.ProductGroup, p.ProductName ProductName,
                                 0  Choice1Id,'' Choice1Name,0 Choice2Id,'' Choice2Name,
                                 o.Id OptionId,o.Name OptionsName, O.Price
                                From
                                 Product p 
                                inner join Options O on O.ProductId=P.Id
                                ) 
                                t
                                Group By 
                                 ProductId,
                                 t.ProductName,
                                 t.ProductGroup,
                                 t.Choice1Id,
                                 t.Choice2Id,
                                 t.OptionId,
                                 t.Price
                                ) as tblSube 
                                Where not exists
                                 (Select * From " + kaynakDb + @".[dbo].[ProductTemplatePrice] ptp 
                                Where tblSube.ProductId=ptp.ProductId 
                                 and tblSube.Choice1Id=ptp.Choice1Id 
                                 and tblSube.Choice2Id=ptp.Choice2Id 
                                 and tblSube.OptionId=ptp.OptionsId
                                 and ptp.TemplateName='" + sablonName + @"'
                                 and ptp.GuncellenecekSubeIdGrubu = '" + guncellenecekSubeIdGrubu + @"'
                                 and ptp.SubeId = '" + subeId + "' )";
            return sqlQuery;
        }


        #region IsUpdate Şueblere yayma

        public static string getSablonProductJoinSubesettingsSqlQuery(string subeIdGrupList)
        {
            return (@"Select p.SubeId, ss.SubeName, ss.DBName,ss.SqlName,ss.SqlPassword,ss.SubeIP   from SubeSettings ss
                        inner join SablonProduct p on p.SubeId =ss.Id
                        Where p.GuncellenecekSubeIdGrubu='" + subeIdGrupList + "' Group by ss.SubeName,ss.DBName,ss.SqlName,ss.SqlPassword,ss.SubeIP,p.SubeId Order by p.SubeId ");
        }
        public static string updatedSablonProductSqlQuery(string dbKaynak, string dbHedef, string sablonName, string subeId)
        {
            var sqlQuery = (@" UPDATE prdt " +
"                                SET prdt.Price = urr.ptpprice, " +
"                                    prdt.IsUpdated=1 " +
"                                FROM " + dbHedef + ".dbo.ProductTemplatePrice prdt , " +
"                                  ( " +
"                                    SELECT h.ptpId, " +
"                                          k.ptpprice," +
"                                          k.ptname kptname," +
"                                          h.ptname hptname," +
"                                          h.PID," +
"		                                  k.SubeId ksubeid " +
"                                   FROM" +
"                                     (SELECT ptp.Id ptpId,ptp.SubeId," +
"                                             ptp.price ptpprice," +
"                                             pt.name ptname," +
"                                             p.Id PID," +
"                                             p.ProductName+p.ProductGroup pname " +
"                                      FROM " + dbKaynak + ".[dbo].[ProductTemplatePrice] ptp " +
"                                      LEFT JOIN " + dbKaynak + ".dbo.[ProductTemplate] pt ON pt.ProductTemplatePkId=ptp.TemplateId " +
"                                      LEFT JOIN " + dbKaynak + ".dbo.SablonProduct p ON p.ProductPkId=ptp.ProductId " +
"                                      WHERE ptp.Choice1Id=0 " +
"                                        AND ptp.Choice2Id=0 " +
"                                        AND ptp.OptionsId=0  and ptp.SubeId=" + subeId + "  ) k " +
"                                   LEFT JOIN " +
"                                     (SELECT ptp.Id ptpId,0 SubeId, " +
"                                             ptp.price ptpprice, " +
"                                             pt.name ptname, " +
"                                             p.Id PID, " +
"                                             p.ProductName+p.ProductGroup pname " +
"                                      FROM " + dbHedef + ".dbo.ProductTemplatePrice ptp " +
"                                      LEFT JOIN " + dbHedef + ".[dbo].[ProductTemplate] pt ON pt.Id=ptp.TemplateId " +
"                                      LEFT JOIN " + dbHedef + ".dbo.Product p ON p.Id=ptp.ProductId " +
"                                      WHERE ptp.Choice1Id=0 " +
"                                        AND ptp.Choice2Id=0 " +
"                                        AND ptp.OptionsId=0 ) h " +
"		                                ON k.pname=h.pname " +
"                                   AND k.ptname=h.ptname " +
"                                   ) urr " +
"                                WHERE prdt.Id=urr.ptpId AND URR.kptname='" + sablonName + "'  and urr.ksubeid='" + subeId + "'  ");

            return sqlQuery;

        }
        public static string updatedSablonChoice1SqlQuery(string dbKaynak, string dbHedef, string sablonName, string subeId)
        {
            var query = (@" UPDATE prdt " +
                            " SET prdt.Price=urr.ptpprice," +
                            "    prdt.IsUpdated=1 " +
                            " FROM " + dbHedef + ".dbo.ProductTemplatePrice prdt ," +
                            "  (SELECT h.ptpId," +
                            "          k.ptpprice," +
                            "          k.ptname kptname," +
                            "          h.ptname hptname," +
                            "          h.PID," +
                            "          h.C1ID," +
                            "		  k.SubeId ksubeid " +
                            "   FROM" +
                            "     (SELECT ptp.Id ptpId,ptp.SubeId," +
                            "             ptp.price ptpprice," +
                            "             pt.name ptname," +
                            "             p.Id PID," +
                            "             p.ProductName+p.ProductGroup pname," +
                            "             c1.Id C1ID," +
                            "             c1.Name name1 " +
                            "      FROM " + dbKaynak + ".[dbo].[ProductTemplatePrice] ptp " +
                            "      LEFT JOIN " + dbKaynak + ".dbo.[ProductTemplate] pt ON pt.ProductTemplatePkId=ptp.TemplateId " +
                            "      LEFT JOIN " + dbKaynak + ".dbo.SablonProduct p ON p.ProductPkId=ptp.ProductId " +
                            "      LEFT JOIN " + dbKaynak + ".dbo.SablonChoice1 c1 ON ptp.ProductId=c1.ProductId " +
                            "      AND ptp.Choice1Id=c1.Choice1PkId " +
                            "      WHERE ptp.OptionsId=0 " +
                            "        AND ptp.Choice2Id=0  and ptp.SubeId=" + subeId + " and c1.SubeId=" + subeId + " ) k " +
                            "   LEFT JOIN " +
                            "     (SELECT ptp.Id ptpId,0 SubeId," +
                            "             ptp.price ptpprice," +
                            "             pt.name ptname," +
                            "             p.Id PID," +
                            "             p.ProductName+p.ProductGroup pname," +
                            "             c1.Id C1ID," +
                            "             c1.Name name1 " +
                            "      FROM " + dbHedef + ".dbo.ProductTemplatePrice ptp " +
                            "      LEFT JOIN " + dbHedef + ".[dbo].[ProductTemplate] pt ON pt.Id=ptp.TemplateId " +
                            "      LEFT JOIN " + dbHedef + ".dbo.Product p ON p.Id=ptp.ProductId " +
                            "      LEFT JOIN " + dbHedef + ".dbo.Choice1 c1 ON ptp.ProductId=c1.ProductId " +
                            "      AND ptp.Choice1Id=c1.Id " +
                            "      WHERE ptp.OptionsId=0 " +
                            "        AND ptp.Choice2Id=0 ) h ON k.pname=h.pname " +
                            "   AND k.name1=h.name1 " +
                            "   AND k.ptname=h.ptname " +
                            "   ) urr " +
                            " WHERE prdt.Id=urr.ptpId AND URR.kptname='" + sablonName + "'   and urr.ksubeid='" + subeId + "'");
            return query;
        }
        public static string updatedSablonChoice2SqlQuery(string dbKaynak, string dbHedef, string sablonName, string subeId)
        {
            var sqlQuery = (@" UPDATE prdt " +
                            " SET prdt.Price = urr.ptpprice," +
                            "    prdt.IsUpdated=1 " +
                            " FROM " + dbHedef + ".dbo.ProductTemplatePrice prdt ," +
                            "  (SELECT h.ptpId," +
                            "          k.ptpprice," +
                            "          k.ptname kptname," +
                            "          h.ptname hptname," +
                            "          h.PID," +
                            "          h.C1ID," +
                            "		k.SubeId ksubeid " +
                            "   FROM " +
                            "     (SELECT ptp.Id ptpId,ptp.SubeId," +
                            "             ptp.price ptpprice," +
                            "             pt.name ptname," +
                            "             p.Id PID," +
                            "             p.ProductName+p.ProductGroup pname," +
                            "             c1.Id C1ID," +
                            "             c1.Name name1," +
                            "             c2.Name name2 " +
                            "      FROM " + dbKaynak + ".[dbo].[ProductTemplatePrice] ptp " +
                            "      LEFT JOIN " + dbKaynak + ".dbo.[ProductTemplate] pt ON pt.ProductTemplatePkId=ptp.TemplateId " +
                            "      LEFT JOIN " + dbKaynak + ".dbo.SablonProduct p ON p.ProductPkId=ptp.ProductId " +
                            "      LEFT JOIN " + dbKaynak + ".dbo.SablonChoice1 c1 ON ptp.ProductId=c1.ProductId " +
                            "      AND ptp.Choice1Id=c1.Choice1PkId " +
                            "      LEFT JOIN " + dbKaynak + ".dbo.SablonChoice2 c2 ON ptp.ProductId=c2.ProductId " +
                            "      AND ptp.Choice2Id=c2.Choice2PkId " +
                            "      WHERE ptp.OptionsId=0 " +
                            "        AND ptp.Choice2Id<>0   and ptp.SubeId=" + subeId + " and c1.SubeId=" + subeId + " and c2.SubeId=" + subeId + ") k " +
                            "   LEFT JOIN " +
                            "     (SELECT ptp.Id ptpId,0 SubeId," +
                            "             ptp.price ptpprice," +
                            "             pt.name ptname," +
                            "             p.Id PID, " +
                            "             p.ProductName+p.ProductGroup pname, " +
                            "             c1.Id C1ID, " +
                            "             c1.Name name1, " +
                            "             c2.Name name2 " +
                            "      FROM " + dbHedef + ".dbo.ProductTemplatePrice ptp " +
                            "      LEFT JOIN " + dbHedef + ".[dbo].[ProductTemplate] pt ON pt.Id=ptp.TemplateId " +
                            "      LEFT JOIN " + dbHedef + ".dbo.Product p ON p.Id=ptp.ProductId " +
                            "      LEFT JOIN " + dbHedef + ".dbo.Choice1 c1 ON ptp.ProductId=c1.ProductId " +
                            "      AND ptp.Choice1Id=c1.Id " +
                            "      LEFT JOIN " + dbHedef + ".dbo.Choice2 c2 ON ptp.ProductId=c2.ProductId " +
                            "      AND ptp.Choice2Id=c2.Id " +
                            "      WHERE ptp.OptionsId=0 " +
                            "        AND ptp.Choice2Id<>0 ) h " +
                            "		ON k.pname=h.pname " +
                            "   AND k.name1=h.name1 " +
                            "   AND k.name2=h.name2 " +
                            "   AND k.ptname=h.ptname " +
                            " ) urr " +
                            " WHERE prdt.Id=urr.ptpId AND URR.kptname='" + sablonName + "'  and urr.ksubeid='" + subeId + "' ");
            return sqlQuery;
        }
        public static string updatedSablonOptionsSqlQuery(string dbKaynak, string dbHedef, string sablonName, string subeId)
        {
            var sqlQuery = (@"UPDATE prdt " +
                            " SET prdt.Price=urr.ptpprice, " +
                            "    prdt.IsUpdated=1" +
                            " FROM  " + dbHedef + ".dbo.ProductTemplatePrice prdt ," +
                            "  (SELECT h.ptpId," +
                            "          k.ptpprice," +
                            "          k.ptname kptname," +
                            "          h.ptname hptname," +
                            "          h.PID," +
                            "		k.SubeId ksubeid" +
                            "   FROM " +
                            "     (SELECT ptp.Id ptpId,ptp.SubeId," +
                            "             ptp.price ptpprice," +
                            "             pt.name ptname," +
                            "             p.Id PID," +
                            "             p.ProductName+p.ProductGroup pname," +
                            "             c1.Name name1" +
                            "      FROM " + dbKaynak + ".[dbo].[ProductTemplatePrice] ptp" +
                            "      LEFT JOIN " + dbKaynak + ".dbo.[ProductTemplate] pt ON pt.ProductTemplatePkId=ptp.TemplateId" +
                            "      LEFT JOIN " + dbKaynak + ".dbo.SablonProduct p ON p.ProductPkId=ptp.ProductId" +
                            "      LEFT JOIN " + dbKaynak + ".dbo.SablonOptions c1 ON ptp.ProductId=c1.ProductId" +
                            "      AND ptp.OptionsId=c1.OptionsPkId" +
                            "      WHERE ptp.Choice1Id=0 " +
                            "        AND ptp.Choice2Id=0  and ptp.SubeId=" + subeId + " and c1.SubeId=" + subeId + ") k " +
                            "   LEFT JOIN " +
                            "     (SELECT ptp.Id ptpId, 0 SubeId," +
                            "             ptp.price ptpprice," +
                            "             pt.name ptname," +
                            "             p.Id PID," +
                            "             p.ProductName+p.ProductGroup pname," +
                            "             c1.Name name1 " +
                            "      FROM  " + dbHedef + ".dbo.ProductTemplatePrice ptp " +
                            "      LEFT JOIN  " + dbHedef + ".[dbo].[ProductTemplate] pt ON pt.Id=ptp.TemplateId " +
                            "      LEFT JOIN  " + dbHedef + ".dbo.Product p ON p.Id=ptp.ProductId " +
                            "      LEFT JOIN  " + dbHedef + ".dbo.Options c1 ON ptp.ProductId=c1.ProductId " +
                            "      AND ptp.OptionsId=c1.Id " +
                            "      WHERE ptp.Choice1Id=0 " +
                            "        AND ptp.Choice2Id=0 ) h ON k.pname=h.pname " +
                            "   AND k.name1=h.name1 " +
                            "   AND k.ptname=h.ptname " +
                            "  ) urr " +
                            " WHERE prdt.Id=urr.ptpId AND URR.kptname='" + sablonName + "'  and urr.ksubeid='" + subeId + "'");

            return sqlQuery;
        }

        public static string getForInsertSablonProductTemplatePriceSqlQuery(string subeId, string sablonNam)
        {
            var sqlQuery = (@"Select * From ProductTemplatePrice where IsManuelInsert=1 and TemplateName='" + sablonNam + "' and SubeId=" + subeId + "");

            return sqlQuery;
        }


        #endregion IsUpdate Şueblere yayma

        #endregion Şablon quary

        #region IsUpdate Şueblere yayma

        public static string getProductJoinSubesettingsSqlQuery(string SubeIdGrupList)
        {
            return (@"Select p.SubeId, ss.SubeName, ss.DBName,ss.SqlName,ss.SqlPassword,ss.SubeIP   from SubeSettings ss
                        inner join Product p on p.SubeId =ss.Id
                        where p.GuncellenecekSubeIdGrubu='" + SubeIdGrupList + "' group by  ss.SubeName, ss.DBName,ss.SqlName,ss.SqlPassword,ss.SubeIP,p.SubeId order by p.SubeId ");
        }
        public static string getProductJoinSubesettingsSqlQuery2(int subeId)
        {
            return (@"Select p.SubeId, ss.SubeName, ss.DBName,ss.SqlName,ss.SqlPassword,ss.SubeIP 
                        From SubeSettings ss 
                        inner join Product p  on p.SubeId =ss.Id
                        where p.SubeId='" + subeId + "' group by  ss.SubeName, ss.DBName,ss.SqlName,ss.SqlPassword,ss.SubeIP,p.SubeId order by p.SubeId ");
        }

        public static string getProductJoinSubesettingsUrunEkleSqlQuery()
        {
            return (@"Select p.SubeId, ss.DBName,ss.SqlName,ss.SqlPassword,ss.SubeIP   from SubeSettings ss
                        inner join Product p on p.SubeId =ss.Id
                       group by ss.DBName,ss.SqlName,ss.SqlPassword,ss.SubeIP,p.SubeId order by p.SubeId ");
        }

        public static string getBomAndBomOptionJoinSubesettingsUrunEkleSqlQuery()
        {
            return (@"select *from (
                      Select p.SubeId, ss.DBName,ss.SqlName,ss.SqlPassword,ss.SubeIP   from SubeSettings ss
                        inner join Bom p on p.SubeId =ss.Id 
                       
					   union all
                      Select p.SubeId, ss.DBName,ss.SqlName,ss.SqlPassword,ss.SubeIP   from SubeSettings ss
                        inner join BomOptions p on p.SubeId =ss.Id
                       
						) as tbl
                       group by DBName,SqlName,SqlPassword,SubeIP,SubeId order by SubeId ");
        }

        public static string updatedProductSqlQuery(string dbKaynak, string dbHedef, string subeId)
        {
            var sqlQuery = (@" update prdt set prdt.Price=u.Price, prdt.IsUpdated=1 from " + dbHedef + ".dbo.Product prdt " +
                           " ,( " +
                           " select k.Price,k.pr1,k.pr2,h.PID,h.C1ID,h.C2ID  from " +
                           " ( " +
                           " select p.Id PID,p.ProductName+p.ProductGroup pname, c1.Id C1ID,c1.Name name1,c2.Id C2ID,c2.Name name2,P.Price,c1.Price pr1,c2.Price pr2 from " +
                           " " + dbKaynak + ".dbo.Product p " +
                           " left join " + dbKaynak + ".dbo.Choice1 c1 on p.ProductPkId=c1.ProductId " +
                           " left join " + dbKaynak + ".dbo.Choice2 c2 on c1.Id=c2.Choice1Id and c1.ProductId=c2.ProductId  Where p.SubeId= " + subeId +
                           " ) k " +
                           " left join " +
                           " (" +
                           " select p.Id PID,p.ProductName+p.ProductGroup pname, c1.Id C1ID,c1.Name name1,c2.Id C2ID,c2.Name name2,P.Price,c1.Price pr1,c2.Price pr2 from " +
                           " " + dbHedef + ".dbo.Product p " +
                           " left join " + dbHedef + ".dbo.Choice1 c1 on p.Id=c1.ProductId " +
                           " left join " + dbHedef + ".dbo.Choice2 c2 on c1.Id=c2.Choice1Id and c1.ProductId=c2.ProductId " +
                           " ) h " +
                           " on k.pname=h.pname " +
                           " ) u " +
                           " where  prdt.Id=u.PID ");
            Singleton.WritingLogFile2("updatedProductSqlQuery", sqlQuery, null, "Kaynak:" + dbKaynak + "Hedef:" + dbHedef);
            return sqlQuery;

        }
        public static string updatedChoice1SqlQuery(string dbKaynak, string dbHedef, string subeId)
        {

            var query = (@"Update prdt set prdt.Price = u.pr1,  prdt.IsUpdated=1 from " + dbHedef + ".dbo.Choice1 prdt " +
                        " ,(" +
                        " select k.Price,k.pr1,k.pr2,h.PID,h.C1ID,h.C2ID  from " +
                        " (" +
                        " select p.Id PID,p.ProductName+p.ProductGroup pname, c1.Id C1ID,c1.Name name1,c2.Id C2ID,c2.Name name2,P.Price,c1.Price pr1,c2.Price pr2 from " +
                        " " + dbKaynak + ".dbo.Product p " +
                        " left join " + dbKaynak + ".dbo.Choice1 c1 on p.ProductPkId=c1.ProductId " +
                        " left join " + dbKaynak + ".dbo.Choice2 c2 on c1.Id=c2.Choice1Id and c1.ProductId=c2.ProductId Where p.SubeId= " + subeId + " and c1.SubeId=" + subeId +
                        " ) k " +
                        " left join " +
                        " (" +
                        " select p.Id PID,p.ProductName+p.ProductGroup pname, c1.Id C1ID,c1.Name name1,c2.Id C2ID,c2.Name name2,P.Price,c1.Price pr1,c2.Price pr2 from " +
                        " " + dbHedef + ".dbo.Product p " +
                        " left join " + dbHedef + ".dbo.Choice1 c1 on p.Id=c1.ProductId " +
                        " left join " + dbHedef + ".dbo.Choice2 c2 on c1.Id=c2.Choice1Id and c1.ProductId=c2.ProductId " +
                        " ) h " +
                        " on k.pname=h.pname and k.name1=h.name1 " +
                        " ) u " +
                        " where  prdt.Id=u.C1ID ");

            Singleton.WritingLogFile2("updatedChoice1SqlQuery", query, null, "Kaynak:" + dbKaynak + "Hedef:" + dbHedef);

            return query;
        }
        public static string updatedChoice2SqlQuery(string dbKaynak, string dbHedef, string subeId)
        {
            var sqlQuery = (@" Update prdt set prdt.Price=u.pr2, prdt.IsUpdated=1 from " + dbHedef + ".dbo.Choice2 prdt " +
                             " ,( " +
                             " select k.Price,k.pr1,k.pr2,h.PID,h.C1ID,h.C2ID  from " +
                             " ( " +
                             " select p.Id PID,p.ProductName+p.ProductGroup pname, c1.Id C1ID,c1.Name name1,c2.Id C2ID,c2.Name name2,P.Price,c1.Price pr1,c2.Price pr2 from " +
                             " " + dbKaynak + ".dbo.Product p " +
                             " left join " + dbKaynak + ".dbo.Choice1 c1 on p.ProductPkId=c1.ProductId " +
                             " left join " + dbKaynak + ".dbo.Choice2 c2 on c1.Choice1PkId=c2.Choice1Id and c1.ProductId=c2.ProductId Where p.SubeId= " + subeId + " and c1.SubeId=" + subeId + "  and  c2.SubeId=" + subeId + " ) k " +
                             " left join " +
                             " (" +
                             " select p.Id PID,p.ProductName+p.ProductGroup pname, c1.Id C1ID,c1.Name name1,c2.Id C2ID,c2.Name name2,P.Price,c1.Price pr1,c2.Price pr2 from " +
                             " " + dbHedef + ".dbo.Product p " +
                             " left join " + dbHedef + ".dbo.Choice1 c1 on p.Id=c1.ProductId " +
                             " left join " + dbHedef + ".dbo.Choice2 c2 on c1.Id=c2.Choice1Id and c1.ProductId=c2.ProductId " +
                             " ) h " +
                             " on k.pname=h.pname and k.name1=h.name1 " +
                             " and k.name2=h.name2 " +
                             " ) u" +
                             " where  prdt.Id=u.C2ID ");

            Singleton.WritingLogFile2("updatedChoice2SqlQuery", sqlQuery, null, "Kaynak:" + dbKaynak + "Hedef:" + dbHedef);
            return sqlQuery;
        }
        public static string updatedOptionsSqlQuery(string dbKaynak, string dbHedef, string subeId)
        {
            var sqlQuery = (@"update prdt set prdt.Price=u.pr1, prdt.IsUpdated=1 from  " + dbHedef + ".dbo.Options prdt " +
                            " ,(" +
                            " select k.Price,k.pr1,h.PID,h.C1ID  from " +
                            " ( " +
                            " select p.Id PID,p.ProductName+p.ProductGroup pname, c1.Id C1ID,c1.Name name1,P.Price,c1.Price pr1 from " +
                            " " + dbKaynak + ".dbo.Product p " +
                            " left join " + dbKaynak + ".dbo.Options c1 on p.ProductPkId=c1.ProductId  Where p.SubeId= " + subeId + " and c1.SubeId=" + subeId +
                            " ) k " +
                            " left join " +
                            " (" +
                            " select p.Id PID,p.ProductName+p.ProductGroup pname, c1.Id C1ID,c1.Name name1,P.Price,c1.Price pr1 from " +
                            "  " + dbHedef + ".dbo.Product p " +
                            " left join  " + dbHedef + ".dbo.Options c1 on p.Id=c1.ProductId " +
                            " ) h " +
                            " on k.pname=h.pname and k.name1=h.name1  " +
                            " ) u " +
                            " where  prdt.Id=u.C1ID ");

            Singleton.WritingLogFile2("updatedOptionsSqlQuery", sqlQuery, null, "Kaynak:" + dbKaynak + "Hedef:" + dbHedef);

            return sqlQuery;
        }

        //Temp tablodaki bilgileri alıp ana Db de güncellemek için kullanılan queryler.
        public static string getUptadetProductSqlQuery(string subeId)
        {
            return (@"Select * From Product p 
                      Where p.SubeId='" + subeId + "' order by p.ProductPkId");
        }
        public static string getUptadetChoice1SqlQuery(string subeId)
        {
            return (@"Select p.ProductPkId, p.ProductGroup,p.ProductName, ch.Name,ch.Price,ch.Choice1PkId From Choice1 ch 
                      inner join Product p on  ch.ProductId=p.ProductPkId
                      Where ch.SubeId='" + subeId + "' order by ch.Choice1PkId");
        }
        public static string getUptadetChoice2SqlQuery(string subeId)
        {
            return (@"Select * From Product p 
                      Where p.SubeId='" + subeId + "' order by p.ProductPkId");
        }
        public static string getUptadetOptionsSqlQuery(string subeId)
        {
            return (@"Select * From Product p 
                      Where p.SubeId='" + subeId + "' order by p.ProductPkId");
        }

        #endregion IsUpdate Şueblere yayma
        #region IsInsert Şubelere yayma

        public static string insertProductSqlQuery(string subeId, string dbHedef)
        {
            var query = @"select * from product where YeniUrunmu=1 and SubeId=" + subeId;

            return query;

        }
        public static string insertChoice1SqlQuery(string productId, string dbHedef, string subeId)
        {
            var query = @"select * from Choice1 where YeniUrunmu=1 and ProductId=" + productId + " and SubeId=" + subeId;
            return query;
        }
        public static string insertChoice2SqlQuery(string choice1Id, string dbHedef, string subeId)
        {
            var sqlQuery = @"select * from Choice2 where YeniUrunmu=1 and Choice1Id=" + choice1Id + " and SubeId=" + subeId;
            return sqlQuery;
        }
        public static string insertOptionsForCategorySqlQuery(string productId, string CategoryId, string dbHedef, string subeId)
        {
            var sqlQuery = @"select * from Options where YeniUrunmu=1 and ProductId=" + productId + " and Category='" + CategoryId + "'" + " and SubeId=" + subeId;

            return sqlQuery;
        }

        public static string insertOptionsSqlQuery(string productId, string dbHedef, string subeId)
        {
            var sqlQuery = @"select * from Options where YeniUrunmu=1 and ProductId=" + productId + " and SubeId=" + subeId + " and (Category is null or Category='' or Category='0')";

            return sqlQuery;
        }

        //Yeni Oluşturuldu// 25.11.2023 
        public static string insertOptionsSqlQuery2(string productId, string dbHedef, string subeId)
        {
            var sqlQuery = @"select * from Options where YeniUrunmu=1 and ProductId=" + productId + " and SubeId=" + subeId + " ";

            return sqlQuery;
        }

        public static string insertOptionCatsSqlQuery(string productId, string dbHedef, string subeId)
        {
            var sqlQuery = @"select * from OptionCats where ProductId=" + productId + " and SubeId=" + subeId;

            return sqlQuery;
        }

        public static string insertBomsSqlQuery(string productId, string dbHedef, string subeId)
        {
            var sqlQuery = @"select * from Bom where YeniUrunMu=1 and ProductId=" + productId + " and SubeId=" + subeId;

            return sqlQuery;
        }

        public static string insertBomOptionsSqlQuery(string optionsId, string productName, string dbHedef, string subeId)
        {
            var sqlQuery = @"select * from BomOptions where YeniUrunMu=1 and OptionsId=" + optionsId + " and ProductName='" + productName + "'" + " and SubeId=" + subeId; ;

            return sqlQuery;
        }

        public static string insertBomsNotProductSqlQuery(string subeId, string dbHedef)
        {
            var sqlQuery = @"select * from Bom where SubeId=" + subeId;

            return sqlQuery;
        }
        public static string insertBomsNotProductProductIdSqlQuery(string subeId, string dbHedef)
        {
            var sqlQuery = @"select ProductName from Bom where SubeId=" + subeId + " Group By ProductName";

            return sqlQuery;
        }

        public static string insertBomOptionsNotProductSqlQuery(string subeId, string dbHedef)
        {
            var sqlQuery = @"select * from BomOptions where SubeId=" + subeId;

            return sqlQuery;
        }
        public static string insertBomOptionsNotProductProductIdSqlQuery(string subeId, string dbHedef)
        {
            var sqlQuery = @"select ProductName from BomOptions where SubeId=" + subeId + " Group By ProductName";

            return sqlQuery;
        }

        #endregion IsInsert Şubelere yayma


        #region Subelerde ortak ürün edit List query

        public static string getLocalOrtakProductListSqlQuery(string productGroup, string productName, string subeIdGrupList)
        {
            var query = (@"Select 
                        ISNULL((Select  top 1 ProductId from Choice1 where ProductId=p.ProductPkId),0) Choice1VarMi,
                        ISNULL((Select  top 1 ProductId from Options where ProductId=p.ProductPkId),0) OptionsVarMi,
                        p.ProductGroup, p.ProductName, p.SubeName, p.Price, p.SubeId, p.Id 
                        From Product p
				        Left join SubeSettings ss on p.SubeId=ss.Id					 
					    Where ss.Status=1 and p.ProductName='" + productName + "'" + " and p.GuncellenecekSubeIdGrubu='" + subeIdGrupList + "'  Order By p.ProductGroup, p.ProductName "
                        );

            return query;
        }
        public static string getLocalCommonOptionstListSqlQuery(string productGroup, string productName, string optionsName, string subeIdGrupList)
        {
            return (@"Select  p.ProductGroup ,p.ProductName,p.Id PId, op.Id, op.ProductId, op.Name OptionsName, op.Price Option_Price, op.SubeId,op.SubeName         
	                  From Options op        
	                  Left join SubeSettings ss on op.SubeId=ss.Id  
                      Left join Product p on op.SubeId=p.SubeId and op.ProductId=p.ProductPkId
					  Where ss.Status=1   and op.Name='" + optionsName + "'" + " and p.GuncellenecekSubeIdGrubu = '" + subeIdGrupList + "'  Order By p.ProductGroup, p.ProductName"
                   );
        }

        #endregion

        #region Şubelerde ortak ürün edit query
        public static string getLocalSablonCommonOptionsListSqlQuery(string productGroup, string productName, string optionsName, string subeIdGrupList)
        {
            return (@"SELECT * " +
                    " FROM " +
                    "  (SELECT ISNULL(" +
                    "                   (SELECT top 1 ProductId" +
                    "                    FROM SablonChoice1" +
                    "                    WHERE ProductId=ptp.ProductId),0) Choice1VarMi," +
                    "          ISNULL(" +
                    "                   (SELECT top 1 ProductId" +
                    "                    FROM SablonOptions" +
                    "                    WHERE ProductId=ptp.ProductId),0) OptionsVarMi," +
                    "          ISNULL(" +
                    "                   (SELECT top 1 ProductId" +
                    "                    FROM SablonChoice2" +
                    "                    WHERE ProductId=ptp.ProductId" +
                    "                      AND Choice1Id=ptp.Choice1Id" +
                    "                      AND Choice2Id >0 ),0) Choice2VarMi," +
                    "          p.ProductGroup," +
                    "          p.ProductName," +
                    "          ptp.Id," +
                    "          ptp.Price productprice," +
                    "          0 c1price," +
                    "          0 c2price," +
                    "          0 optionprice," +
                    "          ptp.ProductId," +
                    "          ptp.Choice1Id," +
                    "          ptp.Choice2Id," +
                    "          ptp.OptionsId," +
                    "          ptp.ProductTemplatePricePkId," +
                    "          p.ProductPkId," +
                    "          ptp.SubeId," +
                    "          ptp.SubeName," +
                    "          ch1.Name ChoiceProductName," +
                    "          ch2.Name Choice2ProductName," +
                    "          op.Name OptionsProductName" +
                    " , p.GuncellenecekSubeIdGrubu" +
                    "   FROM ProductTemplatePrice ptp" +
                    "   LEFT JOIN SablonProduct p ON p.ProductPkId=ptp.ProductId" +
                    "   LEFT JOIN SablonChoice1 ch1 ON ptp.ProductId=ch1.ProductId" +
                    "   AND ptp.Choice1Id=ch1.Choice1PkId" +
                    "   LEFT JOIN SablonChoice2 ch2 ON ptp.ProductId=ch2.ProductId" +
                    "   AND ptp.Choice2Id=ch2.Choice2PkId" +
                    "   LEFT JOIN SablonOptions op ON ptp.ProductId=op.ProductId" +
                    "   AND ptp.OptionsId=op.OptionsPkId" +
                    "   WHERE ptp.SubeId IS NOT NULL" +
                    "     AND ptp.Choice1Id=0" +
                    "     AND ptp.Choice2Id=0" +
                    "     AND ptp.OptionsId=0" +

                    "   GROUP BY p.ProductGroup," +
                    "            p.ProductName," +
                    "            ptp.Id," +
                    "            ptp.Price," +
                    "            ptp.ProductId," +
                    "            ptp.Choice1Id," +
                    "            ptp.Choice2Id," +
                    "            ptp.OptionsId," +
                    "            ptp.ProductTemplatePricePkId," +
                    "            p.ProductPkId," +
                    "            ch1.Name," +
                    "            ptp.ProductId," +
                    "            ptp.SubeId," +
                    "            ptp.SubeName," +
                    "            ch2.Name," +
                    "            op.Name" +
                    " , p.GuncellenecekSubeIdGrubu" +
                    "   UNION ALL SELECT ISNULL(" +
                    "                             (SELECT top 1 ProductId" +
                    "                              FROM SablonChoice1" +
                    "                              WHERE ProductId=ptp.ProductId),0) Choice1VarMi," +
                    "                    ISNULL(" +
                    "                             (SELECT top 1 ProductId" +
                    "                              FROM SablonOptions" +
                    "                              WHERE ProductId=ptp.ProductId),0) OptionsVarMi," +
                    "                    ISNULL(" +
                    "                             (SELECT top 1 ProductId" +
                    "                              FROM SablonChoice2" +
                    "                              WHERE ProductId=ptp.ProductId" +
                    "                                AND Choice1Id=ptp.Choice1Id" +
                    "                                AND Choice2Id >0 ),0) Choice2VarMi," +
                    "                    p.ProductGroup," +
                    "                    p.ProductName," +
                    "                    ptp.Id," +
                    "                    0 productprice," +
                    "                    ptp.Price c1price," +
                    "                    0 c2price," +
                    "                    0 optionprice," +
                    "                    ptp.ProductId," +
                    "                    ptp.Choice1Id," +
                    "                    ptp.Choice2Id," +
                    "                    ptp.OptionsId," +
                    "                    ptp.ProductTemplatePricePkId," +
                    "                    p.ProductPkId," +
                    "                    ptp.SubeId," +
                    "                    ptp.SubeName," +
                    "                    ch1.Name ChoiceProductName," +
                    "                    ch2.Name Choice2ProductName," +
                    "                    op.Name OptionsProductName" +
                    " , p.GuncellenecekSubeIdGrubu" +
                    "   FROM ProductTemplatePrice ptp" +
                    "   LEFT JOIN SablonProduct p ON p.ProductPkId=ptp.ProductId" +
                    "   LEFT JOIN SablonChoice1 ch1 ON ptp.ProductId=ch1.ProductId" +
                    "   AND ptp.Choice1Id=ch1.Choice1PkId" +
                    "   LEFT JOIN SablonChoice2 ch2 ON ptp.ProductId=ch2.ProductId" +
                    "   AND ptp.Choice2Id=ch2.Choice2PkId" +
                    "   LEFT JOIN SablonOptions op ON ptp.ProductId=op.ProductId" +
                    "   AND ptp.OptionsId=op.OptionsPkId" +
                    "   WHERE ptp.SubeId IS NOT NULL" +
                    "     AND ptp.Choice1Id>0" +
                    "     AND ptp.Choice2Id=0" +
                    "     AND ptp.OptionsId=0" +
                    "	 AND op.Name='" + optionsName + "'" +
                    "	  and ptp.GuncellenecekSubeIdGrubu='" + subeIdGrupList + " '" +
                    "   GROUP BY p.ProductGroup," +
                    "            p.ProductName," +
                    "            ptp.Id," +
                    "            ptp.Price," +
                    "            ptp.ProductId," +
                    "            ptp.Choice1Id," +
                    "            ptp.Choice2Id," +
                    "            ptp.OptionsId," +
                    "            ptp.ProductTemplatePricePkId," +
                    "            p.ProductPkId," +
                    "            ch1.Name," +
                    "            ptp.ProductId," +
                    "            ptp.SubeId," +
                    "            ptp.SubeName," +
                    "            ch2.Name," +
                    "            op.Name" +
                    " , p.GuncellenecekSubeIdGrubu" +
                    "   UNION ALL SELECT ISNULL(" +
                    "                             (SELECT top 1 ProductId" +
                    "                              FROM SablonChoice1" +
                    "                              WHERE ProductId=ptp.ProductId),0) Choice1VarMi," +
                    "                    ISNULL(" +
                    "                             (SELECT top 1 ProductId" +
                    "                              FROM SablonOptions" +
                    "                              WHERE ProductId=ptp.ProductId),0) OptionsVarMi," +
                    "                    ISNULL(" +
                    "                             (SELECT top 1 ProductId" +
                    "                              FROM SablonChoice2" +
                    "                              WHERE ProductId=ptp.ProductId" +
                    "                                AND Choice1Id=ptp.Choice1Id" +
                    "                                AND Choice2Id >0 ),0) Choice2VarMi," +
                    "                    p.ProductGroup," +
                    "                    p.ProductName," +
                    "                    ptp.Id," +
                    "                    0 productprice," +
                    "                    0 c1price," +
                    "                    ptp.Price c2price," +
                    "                    0 optionprice," +
                    "                    ptp.ProductId," +
                    "                    ptp.Choice1Id," +
                    "                    ptp.Choice2Id," +
                    "                    ptp.OptionsId," +
                    "                    ptp.ProductTemplatePricePkId," +
                    "                    p.ProductPkId," +
                    "                    ptp.SubeId," +
                    "                    ptp.SubeName," +
                    "                    ch1.Name ChoiceProductName," +
                    "                    ch2.Name Choice2ProductName," +
                    "                    op.Name OptionsProductName" +
                    " , p.GuncellenecekSubeIdGrubu" +
                    "   FROM ProductTemplatePrice ptp" +
                    "   LEFT JOIN SablonProduct p ON p.ProductPkId=ptp.ProductId" +
                    "   LEFT JOIN SablonChoice1 ch1 ON ptp.ProductId=ch1.ProductId" +
                    "   AND ptp.Choice1Id=ch1.Choice1PkId" +
                    "   LEFT JOIN SablonChoice2 ch2 ON ptp.ProductId=ch2.ProductId" +
                    "   AND ptp.Choice2Id=ch2.Choice2PkId" +
                    "   LEFT JOIN SablonOptions op ON ptp.ProductId=op.ProductId" +
                    "   AND ptp.OptionsId=op.OptionsPkId" +
                    "   WHERE ptp.SubeId IS NOT NULL" +
                    "     AND ptp.Choice1Id>0" +
                    "     AND ptp.Choice2Id>0" +
                    "     AND ptp.OptionsId=0" +
                    "	 AND op.Name='" + optionsName + "'" +
                    "	  and ptp.GuncellenecekSubeIdGrubu='" + subeIdGrupList + " '" +
                    "   GROUP BY p.ProductGroup," +
                    "            p.ProductName," +
                    "            ptp.Id," +
                    "            ptp.Price," +
                    "            ptp.ProductId," +
                    "            ptp.Choice1Id," +
                    "            ptp.Choice2Id," +
                    "            ptp.OptionsId," +
                    "            ptp.ProductTemplatePricePkId," +
                    "            p.ProductPkId," +
                    "            ch1.Name," +
                    "            ptp.ProductId," +
                    "            ptp.SubeId," +
                    "            ptp.SubeName," +
                    "            ch2.Name," +
                    "            op.Name" +
                    " , p.GuncellenecekSubeIdGrubu" +
                    "   UNION ALL SELECT ISNULL(" +
                    "                             (SELECT top 1 ProductId" +
                    "                              FROM SablonChoice1" +
                    "                              WHERE ProductId=ptp.ProductId),0) Choice1VarMi," +
                    "                    ISNULL(" +
                    "                             (SELECT top 1 ProductId" +
                    "                              FROM SablonOptions" +
                    "                              WHERE ProductId=ptp.ProductId),0) OptionsVarMi," +
                    "                    ISNULL(" +
                    "                             (SELECT top 1 ProductId" +
                    "                              FROM SablonChoice2" +
                    "                              WHERE ProductId=ptp.ProductId" +
                    "                                AND Choice1Id=ptp.Choice1Id" +
                    "                                AND Choice2Id >0 ),0) Choice2VarMi," +
                    "                    p.ProductGroup," +
                    "                    p.ProductName," +
                    "                    ptp.Id," +
                    "                    0 productprice," +
                    "                    0 c1price," +
                    "                    0 c2price," +
                    "                    ptp.Price optionprice," +
                    "                    ptp.ProductId," +
                    "                    ptp.Choice1Id," +
                    "                    ptp.Choice2Id," +
                    "                    ptp.OptionsId," +
                    "                    ptp.ProductTemplatePricePkId," +
                    "                    p.ProductPkId," +
                    "                    ptp.SubeId," +
                    "                    ptp.SubeName," +
                    "                    ch1.Name ChoiceProductName," +
                    "                    ch2.Name Choice2ProductName," +
                    "                    op.Name OptionsProductName" +
                    " , p.GuncellenecekSubeIdGrubu" +
                    "   FROM ProductTemplatePrice ptp" +
                    "   LEFT JOIN SablonProduct p ON p.ProductPkId=ptp.ProductId" +
                    "   LEFT JOIN SablonChoice1 ch1 ON ptp.ProductId=ch1.ProductId" +
                    "   AND ptp.Choice1Id=ch1.Choice1PkId" +
                    "   LEFT JOIN SablonChoice2 ch2 ON ptp.ProductId=ch2.ProductId" +
                    "   AND ptp.Choice2Id=ch2.Choice2PkId" +
                    "   LEFT JOIN SablonOptions op ON ptp.ProductId=op.ProductId" +
                    "   AND ptp.OptionsId=op.OptionsPkId" +
                    "   WHERE ptp.SubeId IS NOT NULL" +
                    "     AND ptp.Choice1Id=0" +
                    "     AND ptp.Choice2Id=0" +
                    "     AND ptp.OptionsId>0" +
                    "	 AND op.Name='" + optionsName + "'" +
                    "	  and ptp.GuncellenecekSubeIdGrubu='" + subeIdGrupList + " '" +
                    "   GROUP BY p.ProductGroup," +
                    "            p.ProductName," +
                    "            ptp.Id," +
                    "            ptp.Price," +
                    "            ptp.ProductId," +
                    "            ptp.Choice1Id," +
                    "            ptp.Choice2Id," +
                    "            ptp.OptionsId," +
                    "            ptp.ProductTemplatePricePkId," +
                    "            p.ProductPkId," +
                    "            ch1.Name," +
                    "            ptp.ProductId," +
                    "            ptp.SubeId," +
                    "            ptp.SubeName," +
                    "            ch2.Name," +
                    "            op.Name" +
                    " , p.GuncellenecekSubeIdGrubu" +
                    ") t" +
                    " WHERE " +
                    "  t.OptionsProductName='" + optionsName + "' and t.GuncellenecekSubeIdGrubu = '" + subeIdGrupList + " '");
        }

        #endregion
    }
}