using DocumentFormat.OpenXml.Drawing.Charts;
using DocumentFormat.OpenXml.EMMA;
using DocumentFormat.OpenXml.Office2010.Excel;
using DocumentFormat.OpenXml.Spreadsheet;
using iTextSharp.text;
using Jint;
using Microsoft.Ajax.Utilities;
using SefimV2.Helper;
using SefimV2.Models.SefimPanelBelgeCRUD;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Configuration;
using System.Web.Mvc;
using DataTable = System.Data.DataTable;
//
namespace SefimV2.Models.ProductSefimCRUD
{
    public class SefimPanelUrunEkleCRUD
    {
        readonly ModelFunctions mF = new ModelFunctions();
        #region Config local copy db connction setting       
        static readonly string subeIp = WebConfigurationManager.AppSettings["Server"];
        static readonly string dbName = WebConfigurationManager.AppSettings["DBName"];
        static readonly string sqlKullaniciName = WebConfigurationManager.AppSettings["User"];
        static readonly string sqlKullaniciPassword = WebConfigurationManager.AppSettings["Password"];
        #endregion
        public ActionResultMessages Insert(List<SefimPanelUrunEkleViewModel> ProductList, bool urunKopyalamaMi = false)
        {
            var result = new ActionResultMessages();

            var UrunEklemeMi = mF.GetSubeDataWithQuery(mF.NewConnectionString(subeIp, dbName, sqlKullaniciName, sqlKullaniciPassword), "Select * from Product where  GuncellenecekSubeIdGrubu is not null");
            if (UrunEklemeMi.AsEnumerable().Count() > 0)
            {
                result.Param1 = 1099;
                result.IsSuccess = false;
                result.UserMessage = "Ürün Fiyat Güncelleme işlemi yapılmaktadır.Lütfen önce Ürün Fiyat Güncelleme işlemini tamamlayınız.";
                return result;
            }

            var getSube = mF.GetSubeDataWithQuery(mF.NewConnectionString(subeIp, dbName, sqlKullaniciName, sqlKullaniciPassword), "Select * from SubeSettings ");

            //subeid ye göre distinct çekilip product listesi PuroductName ve ProductGroup in çekilerek tek seferde sorgulanabilir
            var subeIds = ProductList.Where(x => x.Id == 0).DistinctBy(x => x.SubeId).Select(x => x.SubeId).ToList();
            foreach (var subeId in subeIds)
            {
                foreach (DataRow itemSube in getSube.AsEnumerable().Where(x => x.Field<long>("Id") == subeId).ToList())
                {
                    var insertSubeIp = mF.RTS(itemSube, "SubeIp");
                    var insertDbName = mF.RTS(itemSube, "DBName");
                    var insertSqlKullaniciName = mF.RTS(itemSube, "SqlName");
                    var insertSqlKullaniciPassword = mF.RTS(itemSube, "SqlPassword");
                    var sqlData = new SqlData(new SqlConnection(mF.NewConnectionString(insertSubeIp, insertDbName, insertSqlKullaniciName, insertSqlKullaniciPassword)));

                    var urunVarMi = sqlData.GetSqlValue(" Select * from Product where ProductName in ('" + string.Join(",", ProductList.Where(x => x.Id == 0 && x.SubeId == subeId).Select(x => x.ProductName).ToList()).Replace(",", "','") + "') and ProductGroup in ('" + string.Join(",", ProductList.Where(x => x.Id == 0 && x.SubeId == subeId).Select(x => x.ProductGroup).ToList()).Replace(",", "','") + "')");
                    if (urunVarMi != null)
                    {
                        result.IsSuccess = false;
                        result.UserMessage = "Yeni Ürün oluşturmak istediğiniz şubede aynı isim ve aynı grup da bir ürün mevcuttur.";
                        return result;
                    }
                }
            }

            using (SqlConnection con = new SqlConnection(mF.NewConnectionString(subeIp, dbName, sqlKullaniciName, sqlKullaniciPassword)))
            {
                con.Open();
                SqlTransaction transaction = con.BeginTransaction();
                SqlData sqlData = new SqlData(con);

                try
                {
                    foreach (var Product in ProductList.OrderBy(x => x.SubeId).Where(x => !string.IsNullOrWhiteSpace(x.ProductName)).Where(x => !string.IsNullOrWhiteSpace(x.Price)).ToList())
                    {
                        //Aynı Ürün varmı kontrolü.
                        //if (Product.Id == 0)
                        //{
                        //    //var getSube = mF.GetSubeDataWithQuery(mF.NewConnectionString(subeIp, dbName, sqlKullaniciName, sqlKullaniciPassword), "Select * from SubeSettings Where Id=" + Product.SubeId);
                        //    foreach (DataRow itemSube in getSube.AsEnumerable().Where(x => x.Field<int>("Id") == Product.SubeId))
                        //    {
                        //        var insertSubeIp = mF.RTS(itemSube, "SubeIp");
                        //        var insertDbName = mF.RTS(itemSube, "DBName");
                        //        var insertSqlKullaniciName = mF.RTS(itemSube, "SqlName");
                        //        var insertSqlKullaniciPassword = mF.RTS(itemSube, "SqlPassword");
                        //        var sqlData = new SqlData(new SqlConnection(mF.NewConnectionString(insertSubeIp, insertDbName, insertSqlKullaniciName, insertSqlKullaniciPassword)));

                        //        var urunVarMi = sqlData.GetSqlValue(" Select * from Product where ProductName='" + Product.ProductName + "' and ProductGroup='" + Product.ProductGroup + "'");
                        //        if (urunVarMi != null)
                        //        {
                        //            result.IsSuccess = false;
                        //            result.UserMessage = "Yeni Ürün oluşturmak istediğiniz şubede aynı isim ve aynı grup da bir ürün mevcuttur.";
                        //            return result;
                        //        }
                        //    }
                        //}
                        //


                        //şubelere göre insert yapılacak
                        if (Product.Id > 0)
                        {

                            //Eski datalar silinecek burası transaction olmalı
                            sqlData.ExecuteScalarTransactionSql("delete from Product where Id=" + Product.Id + " select count(*) from Product where Id=" + Product.Id, transaction);
                            sqlData.ExecuteScalarTransactionSql("delete from Choice1 where ProductId=" + Product.Id + " select count(*) from Choice1 where Id=" + Product.Id, transaction);
                            sqlData.ExecuteScalarTransactionSql("delete from Choice2 where ProductId=" + Product.Id + " select count(*) from Choice2 where Id=" + Product.Id, transaction);
                            sqlData.ExecuteScalarTransactionSql("delete from Options where ProductId=" + Product.Id + " select count(*) from Options where Id=" + Product.Id, transaction);
                            sqlData.ExecuteScalarTransactionSql("delete from OptionCats where ProductId=" + Product.Id + " select count(*) from OptionCats where Id=" + Product.Id, transaction);
                        }


                        string CmdString = " declare @ProductCount int=0" +
                                           " set @ProductCount=(select count(*) from [dbo].[Product] where [ProductName]=@par1 and [ProductGroup]=@par2 and [SubeId]=convert(int,@par13))" +
                                           " if(@ProductCount=0)" +
                                           " Begin " +
                                               "INSERT INTO [dbo].[Product]([ProductName],[ProductGroup],[ProductCode],[Order],[Price],[VatRate],[FreeItem],[InvoiceName],[ProductType],[Plu],[SkipOptionSelection],[Favorites],[SubeId],[SubeName],[YeniUrunMu],[ProductPkId]) VALUES" +
                                               "(@par1, @par2, @par3, @par4, @par5, @par6, @par7, @par8, @par9, @par10, @par11, @par12, @par13, @par14, @par15,0)" +
                                               "select CAST(scope_identity() AS int)" +
                                           " End " + Environment.NewLine +
                                           " else " +
                                           " Begin " + Environment.NewLine +
                                               "select CAST(-1 AS int)--INSERT INTO [dbo].[Product]([ProductName],[ProductGroup],[ProductCode],[Order],[Price],[VatRate],[FreeItem],[InvoiceName],[ProductType],[Plu],[SkipOptionSelection],[Favorites],[SubeId],[SubeName],[YeniUrunMu],[ProductPkId]) VALUES" +
                                               "--(@par1+'_kopya'+(select convert(nvarchar(20), (Select Cast(Datediff(minute, '19700101', Cast(GETDATE() As date)) As bigint) * 60000 + Datediff(ms, '19000101', Cast(GETDATE() As time))))), @par2, @par3, @par4, @par5, @par6, @par7, @par8, @par9, @par10, @par11, @par12, @par13, @par14, @par15,0)" + Environment.NewLine +
                                           " End ";
                        int ID = sqlData.ExecuteScalarTransactionSql(CmdString, transaction, new object[]
                                    {
                                        Product.ProductName,
                                        Product.ProductGroup,
                                        Product.ProductCode,
                                        Product.Order,
                                        Product.Price,
                                        Product.VatRate,
                                        Product.FreeItem,
                                        Product.InvoiceName,
                                        Product.ProductType,
                                        Product.Plu,
                                        Product.SkipOptionSelection,
                                        Product.Favorites,
                                        Product.SubeId,
                                        Product.SubeName,
                                        true
                                    });

                        if (ID > -1)
                        {
                            if (Product.Choice1 != null && Product.Choice1.Count > 0)
                            {
                                foreach (var ch1item in Product.Choice1)
                                {
                                    string CmdStringChoice1 = "INSERT INTO [dbo].[Choice1]([ProductId],[Name],[Price],[SubeId],[SubeName],[Choice1PkId],[YeniUrunMu]) values " +
                                                              "(@par1, @par2, @par3, @par4, @par5, @par6,1)" +
                                                             "select CAST(scope_identity() AS int);";
                                    int choice1Id = sqlData.ExecuteScalarTransactionSql(CmdStringChoice1, transaction, new object[] {
                                                                        ID,
                                                                        ch1item.Name,
                                                                        ch1item.Price,
                                                                        Product.SubeId,
                                                                        Product.SubeName,
                                                                        0
                                                                    });

                                    if (Product.Choice2 != null && Product.Choice2.Count > 0)
                                    {
                                        foreach (var ch2item in Product.Choice2)
                                        {
                                            if (ch2item.secim1Id == ch1item.secimId && ch2item.secim1Id != new Guid())
                                            {
                                                string CmdStringChoice2 = "INSERT INTO [dbo].[Choice2]([ProductId],[Choice1Id],[Name],[Price],[SubeId],[SubeName],[Choice2PkId],[YeniUrunMu]) values " +
                                                                          "(@par1, @par2, @par3, @par4, @par5, @par6, @par7,1)" +
                                                                          "select CAST(scope_identity() AS int);";
                                                int choice2Id = sqlData.ExecuteScalarTransactionSql(CmdStringChoice2, transaction, new object[] {
                                                                    ID,
                                                                    choice1Id,
                                                                    ch2item.Name,
                                                                    ch2item.Price,
                                                                    Product.SubeId,
                                                                    Product.SubeName,
                                                                    0
                                                                });
                                            }
                                            else
                                            {
                                                if (ch2item.Choice1Id == ch1item.Id && ch2item.Choice1Id != 0)
                                                {
                                                    string CmdStringChoice2 = "INSERT INTO [dbo].[Choice2]([ProductId],[Choice1Id],[Name],[Price],[SubeId],[SubeName],[Choice2PkId],[YeniUrunMu]) values " +
                                                                              "(@par1, @par2, @par3, @par4, @par5, @par6, @par7,1)" +
                                                                              "select CAST(scope_identity() AS int);";
                                                    int choice2Id = sqlData.ExecuteScalarTransactionSql(CmdStringChoice2, transaction, new object[] {
                                                                        ID,
                                                                        choice1Id,
                                                                        ch2item.Name,
                                                                        ch2item.Price,
                                                                        Product.SubeId,
                                                                        Product.SubeName,
                                                                        0
                                                                    });
                                                }
                                            }
                                        }
                                    }
                                }
                            }

                            if (!urunKopyalamaMi)
                            {
                                if (Product.OptionCats != null && Product.OptionCats.Count > 0)
                                {
                                    foreach (var optionCats in Product.OptionCats)
                                    {
                                        string CmdStringOptionCats = "INSERT INTO [dbo].[OptionCats]([Name],[ProductId],[MaxSelections],[MinSelections],[SubeId],[SubeName],[OptionCatsPkId],[YeniUrunMu]) values " +
                                                                     "(@par1, @par2, @par3, @par4, @par5, @par6, @par7,1)" +
                                                                     " select CAST(scope_identity() AS int);";
                                        int optionCatsId = sqlData.ExecuteScalarTransactionSql(CmdStringOptionCats, transaction, new object[]
                                        {
                                                                optionCats.Name,
                                                                ID,
                                                                optionCats.MaxSelections,
                                                                optionCats.MinSelections,
                                                                Product.SubeId,
                                                                Product.SubeName,
                                                                0
                                        });

                                        //Kategor girilmiş kayıtlar için.
                                        if (Product.Options != null && Product.Options.Count > 0)
                                        {
                                            foreach (var options in Product.Options)
                                            {
                                                if (options.OptionCatsId != null && optionCats.optionCatsId != null && options.OptionCatsId == optionCats.optionCatsId)
                                                {
                                                    string CmdStringOptions = "INSERT INTO [dbo].[Options]([Name],[Price],[Quantitative],[ProductId],[Category],[SubeId],[SubeName],[OptionsPkId],[YeniUrunMu]) values " +
                                                         "(@par1, @par2, @par3, @par4, @par5, @par6, @par7, @par8,1)" +
                                                                  "select CAST(scope_identity() AS int);";
                                                    int choice2Id = sqlData.ExecuteScalarTransactionSql(CmdStringOptions, transaction, new object[] {
                                            options.Name,
                                            options.Price,
                                            options.Quantitative,
                                            ID,
                                            optionCats.Name,//optionCatsId, //optionCats.Name,
                                            Product.SubeId,
                                            Product.SubeName,
                                            0
                                        });
                                                }
                                                //else if (options.Category == optionCats.Id.ToString() && options.Category != 0.ToString())// 18.11.23 guncellendı, // 08
                                                else if (options.Category == optionCats.Name.ToString() && options.Category != 0.ToString())
                                                {
                                                    string CmdStringOptions = "INSERT INTO [dbo].[Options]([Name],[Price],[Quantitative],[ProductId],[Category],[SubeId],[SubeName],[OptionsPkId],[YeniUrunMu]) values " +
                                                        "(@par1, @par2, @par3, @par4, @par5, @par6, @par7, @par8,1)" +
                                                                 "select CAST(scope_identity() AS int);";
                                                    int choice2Id = sqlData.ExecuteScalarTransactionSql(CmdStringOptions, transaction, new object[] {
                                            options.Name,
                                            options.Price,
                                            options.Quantitative,
                                            ID,
                                             //optionCatsId,
                                            Product.SubeId,
                                            Product.SubeName,
                                            0
                                        });
                                                }
                                            }
                                        }
                                    }
                                }


                                //Kategori girilmemiş kayıtlar için
                                if (Product.Options != null && Product.Options.Count > 0)
                                {
                                    foreach (var options in Product.Options)
                                    {
                                        if (options.OptionCatsId == null && (options.Category == "0" || string.IsNullOrEmpty(options.Category))) //25.11.2023 kopyalamada options tablosunu aktarmadığı için kapatıldı.
                                        {
                                            string CmdStringOptions = "INSERT INTO [dbo].[Options]([Name],[Price],[Quantitative],[ProductId],[Category],[SubeId],[SubeName],[OptionsPkId],[YeniUrunMu]) values " +
                                             "(@par1, @par2, @par3, @par4, @par5, @par6, @par7, @par8,1)" +
                                              "select CAST(scope_identity() AS int);";
                                            int choice2Id = sqlData.ExecuteScalarTransactionSql(CmdStringOptions, transaction, new object[] {
                                    options.Name,
                                    options.Price,
                                    options.Quantitative,
                                    ID,
                                    "",  //options .Category,   //"", //25.11.2023
                                    Product.SubeId,
                                    Product.SubeName,
                                    0
                                });
                                        }
                                    }
                                }
                            }

                            if (urunKopyalamaMi)
                            {
                                //Kategori girilmemiş kayıtlar için
                                if (Product.Options != null && Product.Options.Count > 0)
                                {
                                    foreach (var options in Product.Options)
                                    {
                                        string CmdStringOptions = " INSERT INTO [dbo].[Options]([Name],[Price],[Quantitative],[ProductId],[Category],[SubeId],[SubeName],[OptionsPkId],[YeniUrunMu]) values " +
                                                                  " (@par1, @par2, @par3, @par4, @par5, @par6, @par7, @par8,1)" +
                                                                  " select CAST(scope_identity() AS int);";
                                        int choice2Id = sqlData.ExecuteScalarTransactionSql(CmdStringOptions, transaction, new object[]
                                        {
                                                    options.Name,
                                                    options.Price,
                                                    options.Quantitative,
                                                    ID,
                                                    options.Category,  //options .Category,   //"", //25.11.2023
                                                    Product.SubeId,
                                                    Product.SubeName,
                                                    0
                                        });
                                    }
                                }

                                //Kategori girilmemiş kayıtlar için
                                if (Product.OptionCats != null && Product.OptionCats.Count > 0)
                                {
                                    foreach (var optionCats in Product.OptionCats)
                                    {
                                        string CmdStringOptionCats = " INSERT INTO [dbo].[OptionCats]([Name],[ProductId],[MaxSelections],[MinSelections],[SubeId],[SubeName],[OptionCatsPkId],[YeniUrunMu]) values " +
                                                                     " (@par1, @par2, @par3, @par4, @par5, @par6, @par7,1)" +
                                                                     " select CAST(scope_identity() AS int);";
                                        int optionCatsId = sqlData.ExecuteScalarTransactionSql(CmdStringOptionCats, transaction, new object[]
                                        {
                                                    optionCats.Name,
                                                    ID,
                                                    optionCats.MaxSelections,
                                                    optionCats.MinSelections,
                                                    Product.SubeId,
                                                    Product.SubeName,
                                                    0
                                        });
                                    }
                                }
                            }
                        }
                    }

                    transaction.Commit();
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    result.IsSuccess = false;
                    result.UserMessage = ex.ToString();
                    Singleton.WritingLogFile2("SefimPanelUrunEkleme_Insert", ex.ToString(), null, ex.StackTrace);
                    return result;
                }
                finally
                {
                    con.Close();
                }
            }

            result.IsSuccess = true;
            result.UserMessage = "İşlem Başarılı";

            return result;
        }

        public ActionResultMessages UpdateProduct(SefimPanelUrunEkleViewModel Product)
        {
            var result = new ActionResultMessages();
            var insertSubeIp = string.Empty;
            var insertDbName = string.Empty;
            var insertSqlKullaniciName = string.Empty;
            var insertSqlKullaniciPassword = string.Empty;
            var yeniUrunMu = false; ;
            DataTable choice1Data = new DataTable();
            DataTable choice2Data = new DataTable();
            DataTable optionsData = new DataTable();
            DataTable optionsCatsData = new DataTable();

            //Aynı Ürün varmı kontrolü.
            ////if (Product.Id == 0)
            ////{
            var getSube = mF.GetSubeDataWithQuery(mF.NewConnectionString(subeIp, dbName, sqlKullaniciName, sqlKullaniciPassword), "Select * from SubeSettings Where Id=" + Product.SubeId);
            foreach (DataRow itemSube in getSube.Rows)
            {
                insertSubeIp = mF.RTS(itemSube, "SubeIp");
                insertDbName = mF.RTS(itemSube, "DBName");
                insertSqlKullaniciName = mF.RTS(itemSube, "SqlName");
                insertSqlKullaniciPassword = mF.RTS(itemSube, "SqlPassword");
                var sqlData = new SqlData(new SqlConnection(mF.NewConnectionString(insertSubeIp, insertDbName, insertSqlKullaniciName, insertSqlKullaniciPassword)));
                var urunVarMi = sqlData.GetSqlValue(" Select * from Product where ProductName='" + Product.ProductName + "' and ProductGroup='" + Product.ProductGroup + "'");
                if (urunVarMi == null)
                {
                    yeniUrunMu = true;
                }

                //choice1Data = mF.DataTable(" Select * from Choice1 where ProductId=" + Product.Id + " ");
                choice1Data = mF.GetSubeDataWithQuery(mF.NewConnectionString(insertSubeIp, insertDbName, insertSqlKullaniciName, insertSqlKullaniciPassword), "Select * from Choice1 where ProductId = " + Product.Id + " ");
                choice2Data = mF.GetSubeDataWithQuery(mF.NewConnectionString(insertSubeIp, insertDbName, insertSqlKullaniciName, insertSqlKullaniciPassword), "Select * from Choice2 where ProductId = " + Product.Id + " ");
                optionsData = mF.GetSubeDataWithQuery(mF.NewConnectionString(insertSubeIp, insertDbName, insertSqlKullaniciName, insertSqlKullaniciPassword), "Select * from Options where ProductId = " + Product.Id + " ");
                optionsCatsData = mF.GetSubeDataWithQuery(mF.NewConnectionString(insertSubeIp, insertDbName, insertSqlKullaniciName, insertSqlKullaniciPassword), "Select * from OptionCats where ProductId = " + Product.Id + " ");

                ////if (urunVarMi != null)
                ////{
                ////    result.IsSuccess = false;
                ////    result.UserMessage = "Yeni Ürün oluşturmak istediğiniz şubede aynı isim ve aynı grup da bir ürün mevcuttur.";
                ////    return result;
                ////}
            }
            ////}
            //

            if (yeniUrunMu)
            {
                insertSubeIp = subeIp;
                insertDbName = dbName;
                insertSqlKullaniciName = sqlKullaniciName;
                insertSqlKullaniciPassword = sqlKullaniciPassword;
            }

            using (SqlConnection con = new SqlConnection(mF.NewConnectionString(insertSubeIp, insertDbName, insertSqlKullaniciName, insertSqlKullaniciPassword)))
            {
                con.Open();
                SqlTransaction transaction = con.BeginTransaction();
                SqlData sqlData = new SqlData(con);

                try
                {
                    //string CmdString =
                    //        " BEGIN TRAN UPDATE [dbo].[Product] SET " +
                    //        " [ProductName]= @par1 ,[ProductGroup]=@par2 ,[ProductCode]=@par3, [Order]= @par4, [Price]= @par5, [VatRate]=@par6, [FreeItem]= @par7, " +
                    //        " [InvoiceName]=@par8,[ProductType]= @par9,[Plu]=@par10,[SkipOptionSelection]=@par11, [Favorites]= @par12, [SubeId]= @par13, [SubeName]= @par14 " +
                    //        " Where Id=@par15   SELECT @@TRANCOUNT AS OpenTransactions COMMIT TRAN SELECT @@TRANCOUNT AS OpenTransactions ";

                    string CmdString =
                            " BEGIN TRAN UPDATE [dbo].[Product] SET " +
                            " [ProductName]= @par1 ,[ProductGroup]=@par2 ,[ProductCode]=@par3, [Order]= @par4, [Price]= @par5, [VatRate]=@par6, [FreeItem]= @par7, " +
                            " [InvoiceName]=@par8,[ProductType]= @par9,[Plu]=@par10,[SkipOptionSelection]=@par11, [Favorites]= @par12 " +
                            " Where Id=@par13   SELECT @@TRANCOUNT AS OpenTransactions COMMIT TRAN SELECT @@TRANCOUNT AS OpenTransactions ";

                    int ID = sqlData.ExecuteScalarTransactionSql(CmdString, transaction, new object[]
                                {
                                    Product.ProductName,
                                    Product.ProductGroup,
                                    Product.ProductCode,
                                    Product.Order,
                                    Product.Price,
                                    Product.VatRate,
                                    Product.FreeItem,
                                    Product.InvoiceName,
                                    Product.ProductType,
                                    Product.Plu,
                                    Product.SkipOptionSelection,
                                    Product.Favorites,
                                    //Product.SubeId,
                                    //Product.SubeName,
                                    //Product.YeniUrunMu
                                    Product.Id

                                });

                    if (Product.Id > 0)
                    {
                        //Eski datalar silinecek burası transaction olmalı
                        //sqlData.ExecuteScalarTransactionSql("delete from Product where Id=" + Product.Id + " select count(*) from Product where Id=" + Product.Id, transaction);
                        //sqlData.ExecuteScalarTransactionSql("delete from Choice1 where ProductId=" + Product.Id + " select count(*) from Choice1 where Id=" + Product.Id, transaction);
                        //sqlData.ExecuteScalarTransactionSql("delete from Choice2 where ProductId=" + Product.Id + " select count(*) from Choice2 where Id=" + Product.Id, transaction);
                        //sqlData.ExecuteScalarTransactionSql("delete from Options where ProductId=" + Product.Id + " select count(*) from Options where Id=" + Product.Id, transaction);
                        //sqlData.ExecuteScalarTransactionSql("delete from OptionCats where ProductId=" + Product.Id + " select count(*) from OptionCats where Id=" + Product.Id, transaction);
                    }

                    if (ID > -1)
                    {
                        if (Product.Choice1 != null && Product.Choice1.Count > 0)
                        {
                            foreach (var ch1item in Product.Choice1)
                            {
                                if (choice1Data.Rows.Count > 0)
                                {
                                    //ch1 insert
                                    if (ch1item.Id == 0 && ch1item.ProductId == 0)
                                    {
                                        if (ch1item.Id == 0)
                                        {
                                            string CmdStringChoice1 = "INSERT INTO [dbo].[Choice1]([ProductId],[Name],[Price]) values " +
                                                   "(@par1, @par2, @par3)" +
                                                   "select CAST(scope_identity() AS int);";
                                            int choice1InsertId = sqlData.ExecuteScalarTransactionSql(CmdStringChoice1, transaction, new object[] {
                                                                         Product.Id,
                                                                         ch1item.Name,
                                                                         ch1item.Price });

                                            if (Product.Choice2 != null && Product.Choice2.Count > 0)
                                            {
                                                foreach (var ch2item in Product.Choice2)
                                                {
                                                    //foreach (DataRow itemSubeCh2 in choice2Data.Rows)
                                                    //{
                                                    //    var ch2Id = mF.RTD(itemSubeCh2, "Id");
                                                    //    var urunNameCh2 = mF.RTS(itemSubeCh2, "Name");
                                                    //    var ch1IdproductsIdCh2 = mF.RTD(itemSubeCh2, "ProductId");
                                                    //    var ch1IdCh2 = mF.RTD(itemSubeCh2, "Choice1Id");

                                                    if (ch2item.Id == 0 && ch2item.secim1Id == ch1item.secimId && ch2item.ProductId == 0)
                                                    {
                                                        //ch2 insert
                                                        string sqlCh2 = " INSERT INTO [dbo].[Choice2]([ProductId],[Choice1Id],[Name],[Price]) values " +
                                                                        " (@par1, @par2, @par3, @par4)" +
                                                                        " select CAST(scope_identity() AS int);";
                                                        int choice2Id = sqlData.ExecuteScalarTransactionSql(sqlCh2, transaction, new object[] {
                                                                            Product.Id,
                                                                            choice1InsertId,//ch1item.Id,
                                                                            ch2item.Name,
                                                                            ch2item.Price  });
                                                    }
                                                    //}
                                                }
                                            }
                                        }
                                    }
                                    //Update
                                    else
                                    {
                                        foreach (DataRow itemSube in choice1Data.Rows)
                                        {
                                            var ch1Id = mF.RTD(itemSube, "Id");
                                            var urunName = mF.RTS(itemSube, "Name");
                                            var ch1IdproductsId = mF.RTD(itemSube, "ProductId");

                                            //Delete Ch1
                                            if (choice1Data.Rows.Count > Product.Choice1.Count)
                                            {
                                                var deleteItem = Product.Choice1.Where(x => x.Id == ch1Id).FirstOrDefault();
                                                if (deleteItem == null)
                                                {
                                                    sqlData.ExecuteScalarTransactionSql("delete from Choice1 where ProductId=" + Product.Id + " and Id=" + ch1Id + " select count(*) from Choice1 where Id=" + Product.Id, transaction);
                                                    sqlData.ExecuteScalarTransactionSql("delete from Choice2 where ProductId=" + Product.Id + " and Choice1Id=" + ch1Id + " select count(*) from Choice2 where Id=" + Product.Id, transaction);
                                                }
                                            }

                                            if (ch1item.Id == ch1Id)
                                            {
                                                string ch2Update = " BEGIN TRAN UPDATE  [dbo].[Choice1] SET " +
                                                                  " [ProductId]=@par1, [Name]=@par2,[Price]=@par3 where Id=@par4  SELECT @@TRANCOUNT AS OpenTransactions COMMIT TRAN SELECT @@TRANCOUNT AS OpenTransactions";

                                                int choice1InsertId = sqlData.ExecuteScalarTransactionSql(ch2Update, transaction, new object[] {
                                                              Product.Id,
                                                              ch1item.Name,
                                                              ch1item.Price,
                                                              ch1item.Id  });


                                                if (Product.Choice2 != null && Product.Choice2.Count > 0)
                                                {
                                                    foreach (var ch2item in Product.Choice2)
                                                    {
                                                        foreach (DataRow itemSubeCh2 in choice2Data.Rows)
                                                        {
                                                            var ch2Id = mF.RTD(itemSubeCh2, "Id");
                                                            var urunNameCh2 = mF.RTS(itemSubeCh2, "Name");
                                                            var urunPrice = mF.RTS(itemSubeCh2, "Price");
                                                            var ch1IdproductsIdCh2 = mF.RTD(itemSubeCh2, "ProductId");
                                                            var ch1IdCh2 = mF.RTD(itemSubeCh2, "Choice1Id");

                                                            if (ch2item.Id == ch2Id)
                                                            {
                                                                //ch2 update
                                                                string sqlCh2 = " BEGIN TRAN UPDATE  [dbo].[Choice2] SET" +
                                                                                " [ProductId]=@par1,[Choice1Id]=@par2, [Name]=@par3,[Price]=@par4 where Id=@par5  " +
                                                                                " SELECT @@TRANCOUNT AS OpenTransactions COMMIT TRAN SELECT @@TRANCOUNT AS OpenTransactions";

                                                                int choice2Id = sqlData.ExecuteScalarTransactionSql(sqlCh2, transaction, new object[] {
                                                                        Product.Id,
                                                                        ch1IdCh2, //ch1item.Id,
                                                                        ch2item.Name,
                                                                        ch2item.Price,
                                                                        ch2Id    });
                                                            }
                                                            else
                                                            {
                                                                if (ch2item.Id == 0 && ch2item.secim1Id == ch1item.secimId)
                                                                {
                                                                    //var ayniUrunVarMi = Product.Choice2.Where(x => x.Name == urunNameCh2 && x.Price == urunPrice).FirstOrDefault();
                                                                    //if (ayniUrunVarMi == null)
                                                                    //{
                                                                    //ch2 insert
                                                                    string sqlCh2 = " INSERT INTO [dbo].[Choice2]([ProductId],[Choice1Id],[Name],[Price]) values " +
                                                                                    " (@par1, @par2, @par3, @par4)" +
                                                                                    " select CAST(scope_identity() AS int);";
                                                                    int choice2Id = sqlData.ExecuteScalarTransactionSql(sqlCh2, transaction, new object[] {
                                                                                                         Product.Id,
                                                                                                         ch1Id,//ch1item.Id,
                                                                                                         ch2item.Name,
                                                                                                        ch2item.Price  });
                                                                    ch2item.Id = choice2Id;
                                                                    ch2item.Choice1Id = (int)ch1Id;
                                                                    ch2item.ProductId = (int)Product.Id;

                                                                    //ch2item.secim1Id = ch1Id;
                                                                    //}
                                                                }
                                                                else
                                                                {
                                                                    if (choice2Data.Rows.Count > Product.Choice2.Count)
                                                                    {
                                                                        var deleteData = choice2Data.AsEnumerable().ToList();
                                                                        var deleteItem = Product.Choice2.Where(x => x.Id == ch2Id).FirstOrDefault();

                                                                        if (deleteItem == null)
                                                                        {
                                                                            sqlData.ExecuteScalarTransactionSql("delete from Choice2 where ProductId=" + Product.Id + "and Id=" + ch2Id + "  select count(*) from Choice2 where Id=" + Product.Id, transaction);
                                                                        }
                                                                    }
                                                                }
                                                            }
                                                        }

                                                        var ddd = choice2Data.AsEnumerable().ToList();
                                                        if (choice2Data.AsEnumerable().ToList() == null || choice2Data.AsEnumerable().ToList().Count() == 0)
                                                        {
                                                            if (ch2item.Id == 0 && ch2item.secim1Id == ch1item.secimId)
                                                            {
                                                                //var ayniUrunVarMi = Product.Choice2.Where(x => x.Name == urunNameCh2 && x.Price == urunPrice).FirstOrDefault();
                                                                //if (ayniUrunVarMi == null)
                                                                //{
                                                                //ch2 insert
                                                                string sqlCh2 = " INSERT INTO [dbo].[Choice2]([ProductId],[Choice1Id],[Name],[Price]) values " +
                                                                                " (@par1, @par2, @par3, @par4)" +
                                                                                " select CAST(scope_identity() AS int);";
                                                                int choice2Id = sqlData.ExecuteScalarTransactionSql(sqlCh2, transaction, new object[] {
                                                                                                         Product.Id,
                                                                                                         ch1Id,//ch1item.Id,
                                                                                                         ch2item.Name,
                                                                                                        ch2item.Price  });
                                                                //}
                                                            }
                                                        }


                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    if (ch1item.Id == 0)
                                    {
                                        string CmdStringChoice1 = "INSERT INTO [dbo].[Choice1]([ProductId],[Name],[Price]) values " +
                                               "(@par1, @par2, @par3)" +
                                               "select CAST(scope_identity() AS int);";
                                        int choice1InsertId = sqlData.ExecuteScalarTransactionSql(CmdStringChoice1, transaction, new object[] {
                                                              Product.Id,
                                                              ch1item.Name,
                                                              ch1item.Price });

                                        if (Product.Choice2 != null && Product.Choice2.Count > 0)
                                        {
                                            foreach (var ch2item in Product.Choice2)
                                            {
                                                if (choice2Data.AsEnumerable().ToList().Count() > 0)
                                                {

                                                    foreach (DataRow itemSubeCh2 in choice2Data.Rows)
                                                    {
                                                        var ch2Id = mF.RTD(itemSubeCh2, "Id");
                                                        var urunNameCh2 = mF.RTS(itemSubeCh2, "Name");
                                                        var ch1IdproductsIdCh2 = mF.RTD(itemSubeCh2, "ProductId");
                                                        var ch1IdCh2 = mF.RTD(itemSubeCh2, "Choice1Id");

                                                        //if (ch2item.Id == ch2Id)
                                                        //{
                                                        //    //ch2 update
                                                        //    string sqlCh2 = " BEGIN TRAN UPDATE  [dbo].[Choice2] SET" +
                                                        //                  " [ProductId]=@par1,[Choice1Id]=@par2, [Name]=@par3,[Price]=@par4 where Id=@par5  " +
                                                        //                  "SELECT @@TRANCOUNT AS OpenTransactions COMMIT TRAN SELECT @@TRANCOUNT AS OpenTransactions";

                                                        //    int choice2Id = sqlData.ExecuteScalarTransactionSql(sqlCh2, transaction, new object[] {
                                                        //                                             Product.Id,
                                                        //                                             choice1InsertId,//ch1item.Id,
                                                        //                                             ch2item.Name,
                                                        //                                             ch2item.Price,
                                                        //                                                 ch2Id    });
                                                        //}
                                                        //else
                                                        //{
                                                        if (ch2item.Id == 0 && ch2item.secim1Id == ch1item.secimId)
                                                        {
                                                            //ch2 insert
                                                            string sqlCh2 = " INSERT INTO [dbo].[Choice2]([ProductId],[Choice1Id],[Name],[Price]) values " +
                                                                            " (@par1, @par2, @par3, @par4)" +
                                                                            " select CAST(scope_identity() AS int);";
                                                            int choice2Id = sqlData.ExecuteScalarTransactionSql(sqlCh2, transaction, new object[] {
                                                                            Product.Id,
                                                                            choice1InsertId,//ch1item.Id,
                                                                            ch2item.Name,
                                                                            ch2item.Price  });
                                                        }
                                                        //}
                                                    }

                                                }
                                                else
                                                {
                                                    if (ch2item.Id == 0 && ch2item.secim1Id == ch1item.secimId)
                                                    {
                                                        //ch2 insert
                                                        string sqlCh2 = " INSERT INTO [dbo].[Choice2]([ProductId],[Choice1Id],[Name],[Price]) values " +
                                                                        " (@par1, @par2, @par3, @par4)" +
                                                                        " select CAST(scope_identity() AS int);";
                                                        int choice2Id = sqlData.ExecuteScalarTransactionSql(sqlCh2, transaction, new object[] {
                                                                            Product.Id,
                                                                            choice1InsertId,//ch1item.Id,
                                                                            ch2item.Name,
                                                                            ch2item.Price  });
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            sqlData.ExecuteScalarTransactionSql("delete from Choice1 where ProductId=" + Product.Id + " select count(*) from Choice1 where Id=" + Product.Id, transaction);
                            sqlData.ExecuteScalarTransactionSql("delete from Choice2 where ProductId=" + Product.Id + " select count(*) from Choice2 where Id=" + Product.Id, transaction);
                        }


                        //Kategori girilmemiş kayıtlar için
                        if (Product.Options != null && Product.Options.Count > 0)
                        {
                            foreach (var options in Product.Options)
                            {
                                if (optionsData.Rows.Count > 0)
                                {
                                    foreach (DataRow itemSubeCh2 in optionsData.Rows)
                                    {
                                        var optId = mF.RTD(itemSubeCh2, "Id");
                                        var urunNameCh2 = mF.RTS(itemSubeCh2, "Name");
                                        var optProductId = mF.RTD(itemSubeCh2, "ProductId");


                                        //Delete Options
                                        if (optionsData.Rows.Count > Product.Options.Count)
                                        {
                                            var deleteItem = Product.Options.Where(x => x.Id == optId).FirstOrDefault();
                                            if (deleteItem == null)
                                            {
                                                sqlData.ExecuteScalarTransactionSql("delete from Options where ProductId=" + Product.Id + " and Id=" + optId + " select count(*) from Options where Id=" + Product.Id, transaction);
                                            }
                                        }

                                        if (options.Id == optId)
                                        {
                                            var categori = string.Empty;
                                            if (options.ProductId == 0)
                                            {
                                                categori = options.OptionCatsName == null ? "" : options.OptionCatsName;
                                            }
                                            else
                                            {
                                                categori = options.Category == null ? "" : options.Category;
                                            }


                                            string cmdStringOptions = " BEGIN TRAN UPDATE [dbo].[Options] " +
                                                                      " Set [Name]=@par1,[Price]=@par2, [Quantitative]=@par3, [ProductId]=@par4, [Category]=@par5 where ProductId=@par6 and Id=@par7  " +
                                                                      " SELECT @@TRANCOUNT AS OpenTransactions COMMIT TRAN SELECT @@TRANCOUNT AS OpenTransactions";
                                            //+ " SELECT @@TRANCOUNT AS OpenTransactions COMMIT TRAN SELECT @@TRANCOUNT AS OpenTransactions";
                                            int choice2Id = sqlData.ExecuteScalarTransactionSql(cmdStringOptions, transaction, new object[] {
                                                        options.Name,
                                                        options.Price,
                                                        options.Quantitative,
                                                        Product.Id,
                                                        categori,
                                                        Product.Id,
                                                        optId  });
                                        }
                                        else
                                        {
                                            if (options.Id == 0)
                                            {
                                                var categori = options.OptionCatsName == null ? "" : options.OptionCatsName;
                                                string CmdStringOptions = " INSERT INTO [dbo].[Options]([Name],[Price],[Quantitative],[ProductId],[Category]) values " +
                                                                          " (@par1, @par2, @par3, @par4, @par5 ) " +
                                                                          " select CAST(scope_identity() AS int); ";
                                                int choice2Id = sqlData.ExecuteScalarTransactionSql(CmdStringOptions, transaction, new object[]
                                                {
                                                    options.Name,
                                                    options.Price,
                                                    options.Quantitative,
                                                    Product.Id,
                                                    categori
                                                  });

                                                options.Id = choice2Id;
                                                options.ProductId = (int)Product.Id;
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    if (options.Id == 0)
                                    {
                                        var categori = options.OptionCatsName == null ? "" : options.OptionCatsName;
                                        string CmdStringOptions = " INSERT INTO [dbo].[Options]([Name],[Price],[Quantitative],[ProductId],[Category]) values " +
                                                                  " (@par1, @par2, @par3, @par4, @par5 ) " +
                                                                  " select CAST(scope_identity() AS int); ";
                                        int choice2Id = sqlData.ExecuteScalarTransactionSql(CmdStringOptions, transaction, new object[] {
                                                    options.Name,
                                                    options.Price,
                                                    options.Quantitative,
                                                    Product.Id,
                                                    categori });

                                    }
                                }
                            }
                        }
                        else
                        {
                            sqlData.ExecuteScalarTransactionSql("delete from Options where ProductId=" + Product.Id + " select count(*) from Options where Id=" + Product.Id, transaction);
                        }


                        #region OptionCats

                        //OptionCats 
                        if (Product.OptionCats != null && Product.OptionCats.Count > 0)
                        {
                            foreach (var optionsCatsItem in Product.OptionCats)
                            {
                                if (optionsCatsData.Rows.Count > 0)
                                {
                                    foreach (DataRow itemSubeCh2 in optionsCatsData.Rows)
                                    {
                                        var optId = mF.RTD(itemSubeCh2, "Id");
                                        var urunNameCh2 = mF.RTS(itemSubeCh2, "Name");
                                        var optProductId = mF.RTD(itemSubeCh2, "ProductId");
                                        var ccc = optionsCatsData.AsEnumerable().ToList().Count();

                                        //Delete Options
                                        if (optionsCatsData.AsEnumerable().ToList().Count() > Product.OptionCats.Count)
                                        {
                                            var deleteItem = Product.OptionCats.Where(x => x.Id == optId).FirstOrDefault();
                                            if (deleteItem == null)
                                            {
                                                sqlData.ExecuteScalarTransactionSql("delete from OptionCats where ProductId=" + Product.Id + " and Id=" + optId + " select count(*) from OptionCats where Id=" + Product.Id, transaction);
                                            }
                                        }

                                        if (optionsCatsItem.Id == optId)
                                        {
                                            //string cmdStringOptions = " BEGIN TRAN UPDATE [dbo].[Options] " +
                                            //                          " Set [Name]=@par1,[Price]=@par2, [Quantitative]=@par3, [ProductId]=@par4, [Category]=@par5 where ProductId=@par6 and Id=@par7  " +
                                            //                          " SELECT @@TRANCOUNT AS OpenTransactions COMMIT TRAN SELECT @@TRANCOUNT AS OpenTransactions";
                                            ////+ " SELECT @@TRANCOUNT AS OpenTransactions COMMIT TRAN SELECT @@TRANCOUNT AS OpenTransactions";
                                            //int choice2Id = sqlData.ExecuteScalarTransactionSql(cmdStringOptions, transaction, new object[] {
                                            //            optionsCatsItem.Name,
                                            //            optionsCatsItem.Price,
                                            //            optionsCatsItem.Quantitative,
                                            //            Product.Id,
                                            //            optionsCatsItem.Category,
                                            //            Product.Id,
                                            //            optId  });
                                        }
                                        else
                                        {
                                            if (optionsCatsItem.Id == 0)
                                            {
                                                string CmdStringOptionCats = " INSERT INTO [dbo].[OptionCats]([Name],[ProductId],[MaxSelections],[MinSelections]) values " +
                                                                 " (@par1, @par2, @par3, @par4)" +
                                                                 " select CAST(scope_identity() AS int);";
                                                int optionCatsId = sqlData.ExecuteScalarTransactionSql(CmdStringOptionCats, transaction, new object[]
                                                {
                                                    optionsCatsItem.Name,
                                                    Product.Id,
                                                    optionsCatsItem.MaxSelections,
                                                    optionsCatsItem.MinSelections,

                                                });

                                                optionsCatsItem.Id = optionCatsId;
                                                optionsCatsItem.ProductId = (int)Product.Id;
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    string CmdStringOptionCats = " INSERT INTO [dbo].[OptionCats]([Name],[ProductId],[MaxSelections],[MinSelections]) values " +
                                                                 " (@par1, @par2, @par3, @par4)" +
                                                                 " select CAST(scope_identity() AS int);";
                                    int optionCatsId = sqlData.ExecuteScalarTransactionSql(CmdStringOptionCats, transaction, new object[]
                                    {
                                                    optionsCatsItem.Name,
                                                    Product.Id,
                                                    optionsCatsItem.MaxSelections,
                                                    optionsCatsItem.MinSelections,
                                    });

                                }
                            }
                        }
                        else
                        {
                            if (optionsCatsData.Rows.Count > 0)
                            {
                                foreach (DataRow itemSubeCh2 in optionsCatsData.Rows)
                                {
                                    var optId = mF.RTD(itemSubeCh2, "Id");
                                    var urunNameCh2 = mF.RTS(itemSubeCh2, "Name");
                                    var optProductId = mF.RTD(itemSubeCh2, "ProductId");

                                    //sqlData.ExecuteScalarTransactionSql("delete from OptionCats where ProductId=" + Product.Id + " select count(*) from OptionCats where Id=" + Product.Id, transaction);
                                    sqlData.ExecuteScalarTransactionSql("delete from OptionCats where ProductId=" + Product.Id + " and Id=" + optId + " select count(*) from OptionCats where Id=" + Product.Id, transaction);

                                }
                            }
                        }

                        #endregion OptionCats
                    }

                    #region MyRegion
                    //if (ID > -1)
                    //{
                    //    if (Product.Choice1 != null && Product.Choice1.Count > 0)
                    //    {
                    //        sqlData.ExecuteScalarTransactionSql("delete from Choice1 where ProductId=" + Product.Id + " select count(*) from Choice1 where Id=" + Product.Id, transaction);

                    //        foreach (var ch1item in Product.Choice1)
                    //        {
                    //            //CH1
                    //            string CmdStringChoice1 = "INSERT INTO [dbo].[Choice1]([ProductId],[Name],[Price]) values " +
                    //                                       "(@par1, @par2, @par3)" +
                    //                                       "select CAST(scope_identity() AS int);";
                    //            int choice1InsertId = sqlData.ExecuteScalarTransactionSql(CmdStringChoice1, transaction, new object[] {
                    //                                         Product.Id,
                    //                                         ch1item.Name,
                    //                                         ch1item.Price });
                    //            //ch1item.Id = choice1InsertId;



                    //            if (Product.Choice2 != null && Product.Choice2.Count > 0)
                    //            {
                    //                //sqlData.ExecuteScalarTransactionSql("delete from Choice2 where ProductId=" + Product.Id + " and Choice1Id=" + ch1item.Id + " select count(*) from Choice1 where Id=" + Product.Id, transaction);
                    //                sqlData.ExecuteScalarTransactionSql("delete from Choice2 where ProductId=" + Product.Id + " and Choice1Id=" + ch1item.Id + "  select count(*) from Choice2 where Id=" + Product.Id, transaction);
                    //                foreach (var ch2item in Product.Choice2)
                    //                {
                    //                    if (ch2item.secim1Id == ch1item.secimId && ch2item.secim1Id != new Guid())
                    //                    {
                    //                        string sqlCh2 = " INSERT INTO [dbo].[Choice2]([ProductId],[Choice1Id],[Name],[Price]) values " +
                    //                                        " (@par1, @par2, @par3, @par4)" +
                    //                                        " select CAST(scope_identity() AS int);";
                    //                        int choice2Id = sqlData.ExecuteScalarTransactionSql(sqlCh2, transaction, new object[] {
                    //                                                     Product.Id,
                    //                                                     ch1item.Id,
                    //                                                     ch2item.Name,
                    //                                                    ch2item.Price  });
                    //                    }
                    //                    else
                    //                    {
                    //                        if (ch2item.Choice1Id == ch1item.Id && ch2item.Choice1Id != 0)
                    //                        {
                    //                            string sqlCh2 = " INSERT INTO [dbo].[Choice2]([ProductId],[Choice1Id],[Name],[Price]) values " +
                    //                                            " (@par1, @par2, @par3, @par4)" +
                    //                                            " select CAST(scope_identity() AS int);";
                    //                            int choice2Id = sqlData.ExecuteScalarTransactionSql(sqlCh2, transaction, new object[] {
                    //                                                         Product.Id,
                    //                                                         ch1item.Id,
                    //                                                         ch2item.Name,
                    //                                                        ch2item.Price  });
                    //                        }
                    //                    }
                    //                }
                    //            }


                    //            ////Ch2
                    //            //if (Product.Choice2 != null && Product.Choice2.Count > 0)
                    //            //{
                    //            //    sqlData.ExecuteScalarTransactionSql("delete from Choice2 where ProductId=" + Product.Id + " and Choice1Id=" + ch1item.Id + " select count(*) from Choice1 where Id=" + Product.Id, transaction);

                    //            //    foreach (var ch2item in Product.Choice2)
                    //            //    {

                    //            //        if (ch2item.secim1Id == ch1item.secimId && ch2item.secim1Id != new Guid())
                    //            //        {


                    //            //            //
                    //            //            //ch2 insert
                    //            //            string sqlCh2 = " INSERT INTO [dbo].[Choice2]([ProductId],[Choice1Id],[Name],[Price]) values " +
                    //            //                            " (@par1, @par2, @par3, @par4)" +
                    //            //                            " select CAST(scope_identity() AS int);";
                    //            //            int choice2Id = sqlData.ExecuteScalarTransactionSql(sqlCh2, transaction, new object[] {
                    //            //                             Product.Id,
                    //            //                             ch1item.Id,
                    //            //                             ch2item.Name,
                    //            //                            ch2item.Price  });



                    //            //        }
                    //            //        else
                    //            //        {
                    //            //            if (ch2item.Choice1Id == ch1item.Id && ch2item.Choice1Id != 0)
                    //            //            {
                    //            //                string ch2Update = " BEGIN TRAN UPDATE  [dbo].[Choice2] SET" +
                    //            //                                        " [ProductId]=@par1,[Choice1Id]=@par2, [Name]=@par3,[Price]=@par4 where Id=@par5  SELECT @@TRANCOUNT AS OpenTransactions COMMIT TRAN SELECT @@TRANCOUNT AS OpenTransactions";

                    //            //                int choice2Id = sqlData.ExecuteScalarTransactionSql(ch2Update, transaction, new object[] {
                    //            //                                Product.Id,
                    //            //                                ch1item.Id,
                    //            //                                ch2item.Name,
                    //            //                                ch2item.Price,
                    //            //                                ch2item.Id });
                    //            //            }
                    //            //        }


                    //            //    }
                    //            //}
                    //            //else
                    //            //{
                    //            //    if (choice2Data.Rows.Count > 0)
                    //            //    {
                    //            //        sqlData.ExecuteScalarTransactionSql("delete from Choice2 where ProductId=" + Product.Id + " and Choice1Id=" + ch1item.Id, transaction);
                    //            //    }
                    //            //}
                    //        }

                    //        ////insert
                    //        //else
                    //        //{
                    //        //    //string CmdStringChoice2 = "INSERT INTO [dbo].[Choice1]([ProductId],[Name],[Price]) values " +
                    //        //    //                          "(@par1, @par2, @par3)" +
                    //        //    //                          "select CAST(scope_identity() AS int);";
                    //        //    //int choice1InsertId = sqlData.ExecuteScalarTransactionSql(CmdStringChoice2, transaction, new object[] {
                    //        //    //                         Product.Id,
                    //        //    //                         ch1item.Name,
                    //        //    //                         ch1item.Price });
                    //        //    //ch1item.Id = choice1InsertId;

                    //        //}

                    //        ////Ch2
                    //        //if (Product.Choice2 != null && Product.Choice2.Count > 0)
                    //        //{
                    //        //    sqlData.ExecuteScalarTransactionSql("delete from Choice2 where ProductId=" + Product.Id + " and Choice1Id=" + ch1item.Id + " select count(*) from Choice1 where Id=" + Product.Id, transaction);

                    //        //    foreach (var ch2item in Product.Choice2)
                    //        //    {
                    //        //        //
                    //        //        //ch2 insert
                    //        //        string sqlCh2 = " INSERT INTO [dbo].[Choice2]([ProductId],[Choice1Id],[Name],[Price]) values " +
                    //        //                        " (@par1, @par2, @par3, @par4)" +
                    //        //                        " select CAST(scope_identity() AS int);";
                    //        //        int choice2Id = sqlData.ExecuteScalarTransactionSql(sqlCh2, transaction, new object[] {
                    //        //                             Product.Id,
                    //        //                             ch1item.Id,
                    //        //                             ch2item.Name,
                    //        //                             ch2item.Price  });

                    //        //        //

                    //        //        ////Update
                    //        //        //if (choice2Data.Rows.Count > 0)
                    //        //        //{
                    //        //        //    foreach (DataRow itemSube in choice2Data.Rows)
                    //        //        //    {
                    //        //        //        var ch2Id = mF.RTD(itemSube, "Id");
                    //        //        //        var urunName = mF.RTS(itemSube, "Name");
                    //        //        //        var ch1IdproductsId = mF.RTD(itemSube, "ProductId");
                    //        //        //        var ch1Id = mF.RTD(itemSube, "Choice1Id");

                    //        //        //        if (Product.Id == ch1IdproductsId && ch1item.Id == ch1Id && urunName == ch1item.Name)
                    //        //        //        {
                    //        //        //            string CmdStringChoice2 = " BEGIN TRAN UPDATE  [dbo].[Choice2] SET" +
                    //        //        //                                      " [ProductId]=@par1,[Choice1Id]=@par2, [Name]=@par3,[Price]=@par4 where Id=@par5  SELECT @@TRANCOUNT AS OpenTransactions COMMIT TRAN SELECT @@TRANCOUNT AS OpenTransactions";

                    //        //        //            int choice2Id = sqlData.ExecuteScalarTransactionSql(CmdStringChoice2, transaction, new object[] {
                    //        //        //                    Product.Id,
                    //        //        //                    ch1item.Id,
                    //        //        //                    ch2item.Name,
                    //        //        //                    ch2item.Price,
                    //        //        //                    ch2item.Id });

                    //        //        //        }
                    //        //        //        else if (ch2item.secim1Id == ch1item.secimId && ch2item.secim1Id != new Guid())
                    //        //        //        {
                    //        //        //            //ch2 insert
                    //        //        //            string sqlCh2 = " INSERT INTO [dbo].[Choice2]([ProductId],[Choice1Id],[Name],[Price]) values " +
                    //        //        //                            " (@par1, @par2, @par3, @par4)" +
                    //        //        //                            " select CAST(scope_identity() AS int);";
                    //        //        //            int choice2Id = sqlData.ExecuteScalarTransactionSql(sqlCh2, transaction, new object[] {
                    //        //        //                     Product.Id,
                    //        //        //                     ch1item.Id,
                    //        //        //                     ch2item.Name,
                    //        //        //                     ch2item.Price  });
                    //        //        //        }
                    //        //        //        else
                    //        //        //        {
                    //        //        //            sqlData.ExecuteScalarTransactionSql("delete from Choice2 where ProductId=" + Product.Id + " select count(*) from Choice2 where Id=" + ch2Id, transaction);
                    //        //        //        }
                    //        //        //    }
                    //        //        //}
                    //        //        //else
                    //        //        //{
                    //        //        //    //ch2 insert
                    //        //        //    string sqlCh2 = " INSERT INTO [dbo].[Choice2]([ProductId],[Choice1Id],[Name],[Price]) values " +
                    //        //        //                    " (@par1, @par2, @par3, @par4)" +
                    //        //        //                    " select CAST(scope_identity() AS int);";
                    //        //        //    int choice2Id = sqlData.ExecuteScalarTransactionSql(sqlCh2, transaction, new object[] {
                    //        //        //                     Product.Id,
                    //        //        //                     ch1item.Id,
                    //        //        //                     ch2item.Name,
                    //        //        //                     ch2item.Price  });
                    //        //        //}

                    //        //    }
                    //        //}
                    //        //else
                    //        //{
                    //        //    if (choice2Data.Rows.Count > 0)
                    //        //    {
                    //        //        sqlData.ExecuteScalarTransactionSql("delete from Choice2 where ProductId=" + Product.Id + " and Choice1Id=" + ch1item.Id, transaction);
                    //        //    }
                    //        //}



                    //        //// eski kod aşağısı!!
                    //        ////string CmdStringChoice1 = " BEGIN TRAN UPDATE  [dbo].[Choice1] SET" +
                    //        ////    " [ProductId]=@par1, [Name]=@par2,[Price]=@par3, [SubeId]=@par4, [SubeName]=@par5   SELECT @@TRANCOUNT AS OpenTransactions COMMIT TRAN SELECT @@TRANCOUNT AS OpenTransactions";

                    //        //string CmdStringChoice1 = " BEGIN TRAN UPDATE  [dbo].[Choice1] SET " +
                    //        //" [ProductId]=@par1, [Name]=@par2,[Price]=@par3 where Id=@par4   SELECT @@TRANCOUNT AS OpenTransactions COMMIT TRAN SELECT @@TRANCOUNT AS OpenTransactions";

                    //        //int choice1Id = sqlData.ExecuteScalarTransactionSql(CmdStringChoice1, transaction, new object[] {
                    //        //    ID,
                    //        //    ch1item.Name,
                    //        //    ch1item.Price,
                    //        //    //Product.SubeId,
                    //        //    //Product.SubeName,
                    //        //    ch1item.Id });

                    //        //if (Product.Choice2 != null && Product.Choice2.Count > 0)
                    //        //{
                    //        //    foreach (var ch2item in Product.Choice2)
                    //        //    {
                    //        //        if (ch2item.secim1Id == ch1item.secimId && ch2item.secim1Id != new Guid())
                    //        //        {
                    //        //            //string CmdStringChoice2 = "BEGIN TRAN UPDATE  [dbo].[Choice2] SET" +
                    //        //            //                          " [ProductId]=@par1,[Choice1Id]=@par3 [Name]=@par4,[Price]=@par5, [SubeId]=@par6, [SubeName]=@par7   SELECT @@TRANCOUNT AS OpenTransactions COMMIT TRAN SELECT @@TRANCOUNT AS OpenTransactions";

                    //        //            string CmdStringChoice2 = "BEGIN TRAN UPDATE  [dbo].[Choice2] SET" +
                    //        //                                 " [ProductId]=@par1,[Choice1Id]=@par2, [Name]=@par3,[Price]=@par4 where Id=@par5  SELECT @@TRANCOUNT AS OpenTransactions COMMIT TRAN SELECT @@TRANCOUNT AS OpenTransactions";


                    //        //            int choice2Id = sqlData.ExecuteScalarTransactionSql(CmdStringChoice2, transaction, new object[] {
                    //        //                ID,
                    //        //                choice1Id,
                    //        //                ch2item.Name,
                    //        //                ch2item.Price,
                    //        //                //Product.SubeId,
                    //        //                //Product.SubeName,
                    //        //                //0,
                    //        //                ch2item.Id
                    //        //            });
                    //        //        }
                    //        //        //         else
                    //        //        //         {
                    //        //        //             if (ch2item.Choice1Id == ch1item.Id && ch2item.Choice1Id != 0)
                    //        //        //             {
                    //        //        //                 string CmdStringChoice2 = "INSERT INTO [dbo].[Choice2]([ProductId],[Choice1Id],[Name],[Price],[SubeId],[SubeName],[Choice2PkId],[YeniUrunMu]) values " +
                    //        //        //           "(@par1, @par2, @par3, @par4, @par5, @par6, @par7,1)" +
                    //        //        //"select CAST(scope_identity() AS int);";
                    //        //        //                 int choice2Id = sqlData.ExecuteScalarTransactionSql(CmdStringChoice2, transaction, new object[] {
                    //        //        //                     ID,
                    //        //        //                     choice1Id,
                    //        //        //                     ch2item.Name,
                    //        //        //                     ch2item.Price,
                    //        //        //                     Product.SubeId,
                    //        //        //                     Product.SubeName,
                    //        //        //                     0
                    //        //        //               });
                    //        //        //             }
                    //        //        //         }
                    //        //    }
                    //        //}


                    //    }
                    //    else
                    //    {
                    //        //Delete
                    //        sqlData.ExecuteScalarTransactionSql("delete from Choice1 where ProductId=" + Product.Id + " select count(*) from Choice1 where Id=" + Product.Id, transaction);
                    //        sqlData.ExecuteScalarTransactionSql("delete from Choice2 where ProductId=" + Product.Id + " select count(*) from Choice2 where Id=" + Product.Id, transaction);

                    //    }

                    //    //    if (Product.OptionCats != null && Product.OptionCats.Count > 0)
                    //    //    {
                    //    //        foreach (var optionCats in Product.OptionCats)
                    //    //        {
                    //    //            string CmdStringOptionCats = "INSERT INTO [dbo].[OptionCats]([Name],[ProductId],[MaxSelections],[MinSelections],[SubeId],[SubeName],[OptionCatsPkId],[YeniUrunMu]) values " +
                    //    //                 "(@par1, @par2, @par3, @par4, @par5, @par6, @par7,1)" +
                    //    //      "select CAST(scope_identity() AS int);";
                    //    //            int optionCatsId = sqlData.ExecuteScalarTransactionSql(CmdStringOptionCats, transaction, new object[] {
                    //    //    optionCats.Name,
                    //    //    ID,
                    //    //    optionCats.MaxSelections,
                    //    //    optionCats.MinSelections,
                    //    //    Product.SubeId,
                    //    //    Product.SubeName,
                    //    //    0
                    //    //});

                    //    //Kategor girilmiş kayıtlar için.
                    //    //if (Product.Options != null && Product.Options.Count > 0)
                    //    //{
                    //    //    foreach (var options in Product.Options)
                    //    //    {
                    //    //        if (options.OptionCatsId != null && optionCats.optionCatsId != null && options.OptionCatsId == optionCats.optionCatsId)
                    //    //        {
                    //    //            string CmdStringOptions = "INSERT INTO [dbo].[Options]([Name],[Price],[Quantitative],[ProductId],[Category],[SubeId],[SubeName],[OptionsPkId],[YeniUrunMu]) values " +
                    //    //                 "(@par1, @par2, @par3, @par4, @par5, @par6, @par7, @par8,1)" +
                    //    //                          "select CAST(scope_identity() AS int);";
                    //    //            int choice2Id = sqlData.ExecuteScalarTransactionSql(CmdStringOptions, transaction, new object[] {
                    //    //            options.Name,
                    //    //            options.Price,
                    //    //            options.Quantitative,
                    //    //            ID,
                    //    //            optionCatsId,
                    //    //            Product.SubeId,
                    //    //            Product.SubeName,
                    //    //            0
                    //    //        });
                    //    //        }
                    //    //        else if (options.Category == optionCats.Id.ToString() && options.Category != 0.ToString())
                    //    //        {
                    //    //            string CmdStringOptions = "INSERT INTO [dbo].[Options]([Name],[Price],[Quantitative],[ProductId],[Category],[SubeId],[SubeName],[OptionsPkId],[YeniUrunMu]) values " +
                    //    //                "(@par1, @par2, @par3, @par4, @par5, @par6, @par7, @par8,1)" +
                    //    //                         "select CAST(scope_identity() AS int);";
                    //    //            int choice2Id = sqlData.ExecuteScalarTransactionSql(CmdStringOptions, transaction, new object[] {
                    //    //            options.Name,
                    //    //            options.Price,
                    //    //            options.Quantitative,
                    //    //            ID,
                    //    //            optionCatsId,
                    //    //            Product.SubeId,
                    //    //            Product.SubeName,
                    //    //            0
                    //    //        });
                    //    //        }
                    //    //    }
                    //    //}


                    //    //    }
                    //    //}


                    //    ////Kategori girilmemiş kayıtlar için
                    //    //if (Product.Options != null && Product.Options.Count > 0)
                    //    //{
                    //    //    foreach (var options in Product.Options)
                    //    //    {
                    //    //        if (options.OptionCatsId == null && (options.Category == "0" || string.IsNullOrEmpty(options.Category)))
                    //    //        {
                    //    //            string CmdStringOptions = "INSERT INTO [dbo].[Options]([Name],[Price],[Quantitative],[ProductId],[Category],[SubeId],[SubeName],[OptionsPkId],[YeniUrunMu]) values " +
                    //    //                 "(@par1, @par2, @par3, @par4, @par5, @par6, @par7, @par8,1)" +
                    //    //                  "select CAST(scope_identity() AS int);";
                    //    //            int choice2Id = sqlData.ExecuteScalarTransactionSql(CmdStringOptions, transaction, new object[] {
                    //    //            options.Name,
                    //    //            options.Price,
                    //    //            options.Quantitative,
                    //    //            ID,
                    //    //            "",
                    //    //            Product.SubeId,
                    //    //            Product.SubeName,
                    //    //            0
                    //    //        });
                    //    //        }
                    //    //    }
                    //    //}
                    //}
                    #endregion
                    transaction.Commit();
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    result.IsSuccess = false;
                    result.UserMessage = ex.ToString();
                    return result;
                }
            }


            result.IsSuccess = true;
            result.UserMessage = "İşlem Başarılı";

            return result;

        }
        public ActionResultMessages InsertRecete(List<Bom> bomList, List<BomOptions> bomOptionList, bool yeniUrunMu)
        {
            ActionResultMessages result = new ActionResultMessages();

            using (SqlConnection con = new SqlConnection(mF.NewConnectionString(subeIp, dbName, sqlKullaniciName, sqlKullaniciPassword)))
            {
                con.Open();
                SqlTransaction transaction = con.BeginTransaction();
                SqlData sqlData = new SqlData(con);

                if (!yeniUrunMu)//Yeni eklenen reçete değilse öncelikle şubedeki bom ve bomoptions tablosundaki producta ait kayıtlar siliniyor.
                {
                    #region SUBSTATION LIST               
                    mF.SqlConnOpen();
                    string qr = "select * from SubeSettings Where Status=1 and Id=";
                    if (bomList != null && bomList.Count > 0)
                    {
                        qr += bomList[0].SubeId;
                    }
                    else if (bomOptionList != null && bomOptionList.Count > 0)
                    {
                        qr += bomOptionList[0].SubeId;
                    }
                    DataTable dt = mF.DataTable(qr);
                    mF.SqlConnClose();
                    #endregion SUBSTATION LIST

                    foreach (DataRow r in dt.Rows)
                    {
                        string SubeId = mF.RTS(r, "ID");
                        string SubeAdi = mF.RTS(r, "SubeName");
                        string SubeIP = mF.RTS(r, "SubeIP");
                        string SqlName = mF.RTS(r, "SqlName");
                        string SqlPassword = mF.RTS(r, "SqlPassword");
                        string DBName = mF.RTS(r, "DBName");

                        string Query = "delete from Bom where ProductId=";
                        if (bomList != null && bomList.Count > 0)
                        {
                            Query += bomList[0].ProductId + " select * from bom where ProductId=" + bomList[0].ProductId;
                            DataTable BomDt = new DataTable();
                            BomDt = mF.GetSubeDataWithQuery((mF.NewConnectionString(SubeIP, DBName, SqlName, SqlPassword)), Query.ToString());//transaction olmalı
                        }

                        string QueryOptions = "delete from BomOptions where ProductName='";
                        if (bomOptionList != null && bomOptionList.Count > 0)
                        {
                            QueryOptions += bomOptionList[0].ProductName + "' select * from bomOptions where ProductName='" + bomOptionList[0].ProductName + "'";

                            DataTable BomOptionsDt = new DataTable();
                            BomOptionsDt = mF.GetSubeDataWithQuery((mF.NewConnectionString(SubeIP, DBName, SqlName, SqlPassword)), QueryOptions.ToString());//transaction olmalı
                        }
                    }
                }

                if (bomList != null && bomList.Count > 0 && bomList[0].ProductId > 0)
                {
                    //Eski datalar silinecek burası transaction olmalı
                    sqlData.ExecuteScalarTransactionSql("delete from Bom where ProductId=" + bomList[0].ProductId + " and SubeId=" + bomList[0].SubeId + " select count(*) from Product where Id=" + bomList[0].ProductId, transaction);
                }
                if (bomOptionList != null && bomOptionList.Count > 0 && bomOptionList[0].ProductId > 0)
                {
                    sqlData.ExecuteScalarTransactionSql("delete from BomOptions where ProductId='" + bomOptionList[0].ProductId + "' and SubeId=" + bomOptionList[0].SubeId + " select count(*) from Product where Id='" + bomOptionList[0].ProductId + "'", transaction);
                }

                try
                {
                    if (bomList != null)
                        foreach (var bom in bomList)
                        {
                            string CmdStringRecete = " INSERT INTO [dbo].[Bom]([ProductName],[MaterialName],[MalinCinsi],[Quantity],[Unit],[StokID],[ProductId],[SubeId],[YeniUrunMu]) values " +
                                                     " (@par1, @par2, @par3, @par4, @par5, @par6, @par7, @par8,@par9)" +
                                                     " select CAST(scope_identity() AS int);";
                            int choice2Id = sqlData.ExecuteScalarTransactionSql(CmdStringRecete, transaction, new object[] {
                            bom.ProductName,
                            //bom.MaterialName,
                            bom.MalinCinsi!=null?bom.MalinCinsi.Trim():bom.MalinCinsi, //13.03.2023 isteğe istinaden güncellendi
                            bom.MalinCinsi!=null?bom.MalinCinsi.Trim():bom.MalinCinsi,
                            bom.Quantity,
                            bom.Unit,
                            bom.StokID,
                            bom.ProductId,
                            bom.SubeId,
                            yeniUrunMu
                        });
                        }
                    if (bomOptionList != null)
                        foreach (var bomOption in bomOptionList)
                        {
                            string CmdStringBomOption = " INSERT INTO[dbo].[BomOptions]([OptionsId],[OptionsName],[MaterialName],[Quantity],[Unit],[StokID],[ProductName],[SubeId],[YeniUrunMu],[ProductId]) VALUES " +
                                                        " (@par1, @par2, @par3, @par4, @par5, @par6, @par7, @par8, @par9, @par10)" +
                                                        " select CAST(scope_identity() AS int);";
                            int choice2Id = sqlData.ExecuteScalarTransactionSql(CmdStringBomOption, transaction, new object[] {
                            bomOption.OptionsId,
                            bomOption.OptionsName,
                            bomOption.MaterialName,
                            bomOption.Quantity,
                            bomOption.Unit,
                            bomOption.StokID,
                            bomOption.ProductName,
                            bomOption.SubeId,
                            yeniUrunMu,
                            bomOption.ProductId
                        });
                        }
                    transaction.Commit();
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    result.IsSuccess = false;
                    result.UserMessage = ex.ToString();
                    return result;
                }
            }
            result.result_STR = "Başarılı";
            result.result_INT = 1;
            result.result_BOOL = true;
            result.result_OBJECT = bomList;
            result.IsSuccess = true;
            result.UserMessage = "İşlem Başarılı";
            return result;

        }

        public ActionResultMessages Delete(int id, int SubeId)
        {
            ActionResultMessages result = new ActionResultMessages();
            ModelFunctions f = new ModelFunctions();

            try
            {
                f.SqlConnOpen();
                string CmdString = "Delete from Product where Id=" + id + " and SubeId=" + SubeId;
                CmdString += " Delete from Choice1 where ProductId=" + id + " and SubeId=" + SubeId;
                CmdString += " Delete from Choice2 where ProductId=" + id + " and SubeId=" + SubeId;
                CmdString += " Delete from Options where ProductId=" + id + " and SubeId=" + SubeId;
                CmdString += " Delete from OptionCats where ProductId=" + id + " and SubeId=" + SubeId;
                CmdString += " Delete from Bom where ProductId=" + id + " and SubeId=" + SubeId;
                CmdString += " Delete from BomOptions where ProductId=" + id + " and SubeId=" + SubeId;

                OleDbCommand Cmd = new OleDbCommand(CmdString, f.ConnOle);
                Cmd.ExecuteNonQuery();
                f.SqlConnClose();
            }
            catch (Exception ex)
            {
                result.IsSuccess = false;
                result.UserMessage = ex.ToString();
                return result;
            }
            result.result_STR = "Başarılı";
            result.result_INT = 1;
            result.result_BOOL = true;
            result.result_OBJECT = null;
            result.IsSuccess = true;
            result.UserMessage = "İşlem Başarılı";
            return result;
        }

        public ActionResultMessages Copy(Dictionary<int, bool> ids, List<SelectListItem> SubeList, List<int> subeIdList, List<bool> yeniUrunMuList, string kullaniciId)
        {
            var result = new ActionResultMessages
            {
                UserMessage = ""
            };

            try
            {
                List<SefimPanelUrunEkleViewModel> product = new List<SefimPanelUrunEkleViewModel>();


                if (ids.Where(y => y.Value == true).Any())
                {
                    product = GetProduct(ids.Where(y => y.Value == true).Select(x => x.Key).ToList());
                    #region **** Kendi Kaydını eklemek için *****

                    var resultKendisi = Insert(product);
                    if (!resultKendisi.IsSuccess)
                    {
                        if (resultKendisi.Param1 == 1099)
                        {
                            result.Param1 = resultKendisi.Param1;
                            result.UserMessage = resultKendisi.UserMessage;
                            return result;
                        }

                        //result.UserMessage += "Şube:" + product.SubeName + " hata:" + resultKendisi.UserMessage;
                        result.UserMessage += "Şube:" + " hata:" + resultKendisi.UserMessage;
                    }
                    #endregion **** Kendi Kaydını eklemek için *****
                }


                //Şubeden ürünü çekip şubelere kayıt oluşturulacak
                if (ids.Where(y => y.Value == false).Any())
                {
                    product = GetProductForSube(ids.Where(y => y.Value == false).Select(x => x.Key).ToList(), subeIdList, kullaniciId);
                }


                foreach (var sube in SubeList)
                {
                    foreach (var productItem in product)
                    {
                        productItem.SubeId = Convert.ToInt32(sube.Value);
                        productItem.SubeName = sube.Text;
                    }
                    ActionResultMessages msg = Insert(product, true);
                    if (!msg.IsSuccess)
                    {
                        //result.UserMessage += "Şube:" + product.SubeName + " hata:" + msg.UserMessage;
                        result.UserMessage += "Şube:" + " hata:" + msg.UserMessage;
                    }
                }
            }
            catch (Exception ex)
            {
                result.IsSuccess = false;
                result.UserMessage = ex.ToString();
                return result;
            }
            result.result_STR = "Başarılı";
            result.result_INT = 1;
            result.result_BOOL = true;
            result.result_OBJECT = null;
            result.IsSuccess = true;
            if (result.UserMessage == "")
                result.UserMessage = "İşlem Başarılı";
            return result;
        }
        public ActionResultMessages AktifPasif(int Id, int SubeId, bool YeniUrunMu, bool Aktif, string kullaniciId)
        {
            var result = new ActionResultMessages();
            ModelFunctions f = new ModelFunctions();
            result.UserMessage = "";
            try
            {

                string updateCmd = "";
                if (Aktif)
                {
                    updateCmd = "update Product set ProductGroup=REPLACE(ProductGroup,'$','') where Id=" + Id.ToString();
                }
                else
                {
                    updateCmd = "update Product set ProductGroup='$'+ProductGroup where Id=" + Id.ToString();
                }

                if (YeniUrunMu)
                {
                    f.SqlConnOpen();
                    updateCmd += " and SubeId=" + SubeId.ToString();
                    OleDbCommand Cmd = new OleDbCommand(updateCmd, f.ConnOle);
                    Cmd.ExecuteNonQuery();
                    f.SqlConnClose();
                }
                else
                {
                    #region SUBSTATION LIST               
                    mF.SqlConnOpen();
                    DataTable dt = mF.DataTable("select * from SubeSettings Where Status=1 and Id=" + SubeId);
                    mF.SqlConnClose();
                    #endregion SUBSTATION LIST

                    //Kullanıcının yetkili olduğu şube
                    var kullaniciSubeYetkisi = UsersListCRUD.KullaniciSubeYetkiListesi(kullaniciId);

                    foreach (DataRow r in dt.Rows)
                    {
                        string subeId = mF.RTS(r, "ID");

                        if (kullaniciSubeYetkisi != null && kullaniciSubeYetkisi.FR_SubeListesi.Where(x => x.SubeID == Convert.ToInt32(subeId)).Select(x => x.SubeID).Any())
                        {
                            string SubeAdi = mF.RTS(r, "SubeName");
                            string SubeIP = mF.RTS(r, "SubeIP");
                            string SqlName = mF.RTS(r, "SqlName");
                            string SqlPassword = mF.RTS(r, "SqlPassword");
                            string DBName = mF.RTS(r, "DBName");


                            DataTable ProductDt = new DataTable();
                            ProductDt = mF.GetSubeDataWithQuery((mF.NewConnectionString(SubeIP, DBName, SqlName, SqlPassword)), updateCmd.ToString());
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                result.IsSuccess = false;
                result.UserMessage = ex.ToString();
                return result;
            }
            result.result_STR = "Başarılı";
            result.result_INT = 1;
            result.result_BOOL = true;
            result.IsSuccess = true;
            result.UserMessage = "İşlem Başarılı";
            return result;
        }
        public ActionResultMessages CopyForSube(List<SelectListItem> MainSubeList, List<SelectListItem> SubeList)
        {
            var result = new ActionResultMessages
            {
                UserMessage = string.Empty,
            };
            try
            {
                var oncedenKopyalanmisReceteSeubeList = GetSubeSendBomList(null);
                foreach (var item in oncedenKopyalanmisReceteSeubeList)
                {
                    foreach (var sube in SubeList)
                    {
                        if (item.Text == sube.Text)
                        {
                            result.IsSuccess = false;
                            result.UserMessage = sube.Text + " şubesine receteler daha önceden kopyalanmıştır.Lütfen kontrol ediniz. ";
                            return result;
                        }
                    }
                }

                string MainSubeIds = "";
                foreach (var item in MainSubeList)
                {
                    MainSubeIds += item.Value + ",";
                }
                MainSubeIds = MainSubeIds.Substring(0, MainSubeIds.Length - 1);
                ModelFunctions f = new ModelFunctions();
                f.SqlConnOpen();
                DataTable dtarctos = f.DataTable("SELECT Id,DBName,IP,SqlName,SqlPassword  FROM  VegaDbSettings");
                string VegaDbId;
                string VegaDbName;
                string VegaDbIp;
                string VegaDbSqlName;
                string VegaDbSqlPassword;
                string connString = "";
                foreach (DataRow r in dtarctos.Rows)
                {
                    VegaDbId = f.RTS(r, "Id");
                    VegaDbName = f.RTS(r, "DBName");
                    VegaDbIp = f.RTS(r, "IP");
                    VegaDbSqlName = f.RTS(r, "SqlName");
                    VegaDbSqlPassword = f.RTS(r, "SqlPassword");

                    connString = "Server=" + VegaDbIp + ";User Id=" + VegaDbSqlName + ";Password=" + VegaDbSqlPassword + ";Database=" + VegaDbName + "";

                }

                var resultSubeList = new SPosSubeSablonFiyatGuncellemeCRUD().GetSubeList();
                f.DataTable(@"declare @tempProduct int =( select COUNT(*) from sys.tables where name='TempProduct')
                                if (@tempProduct = 0)
                                    create table TempProduct(Id int, ProductName nvarchar(500))");

                f.DataTable(@"declare @tempProductSube int =( select COUNT(*) from sys.tables where name='TempProductSube')
                                if (@tempProductSube = 0)
                                    create table TempProductSube(Id int, ProductName nvarchar(500),SubeId int,ProductNameEk  nvarchar(500))");

                f.DataTable(@"declare @tempBom int =( select COUNT(*) from sys.tables where name='TempBom')
                                if (@tempBom = 0)
                                    create table TempBom([Id] [int] IDENTITY(1,1) NOT NULL,
	                                                    [ProductName] [nvarchar](450) NULL,
	                                                    [MaterialName] [nvarchar](450) NULL,
	                                                    [Quantity] [decimal](10, 4) NOT NULL,
	                                                    [Unit] [nvarchar](20) NOT NULL,
	                                                    [StokID] [int] NOT NULL,
	                                                    [ProductId] [int] NULL,
	                                                    [SubeId] [int] NULL,
	                                                    [YeniUrunMu] [bit] NULL)");

                f.DataTable(@"declare @tempBomOptions int =( select COUNT(*) from sys.tables where name='TempBomOptions')
                                if (@tempBomOptions = 0)
                                    create table TempBomOptions([Id] [int] IDENTITY(1,1) NOT NULL,
	                                                            [OptionsId] [int] NULL,
	                                                            [OptionsName] [nvarchar](450) NULL,
	                                                            [MaterialName] [nvarchar](450) NULL,
	                                                            [Quantity] [decimal](10, 4) NOT NULL,
	                                                            [Unit] [nvarchar](20) NULL,
	                                                            [StokID] [int] NULL,
	                                                            [ProductName] [nvarchar](450) NULL,
	                                                            [SubeId] [int] NULL,
	                                                            [YeniUrunMu] [bit] NULL,
	                                                            [ProductId] [int] NULL)");

                f.DataTable(@"declare @tempStok int =( select COUNT(*) from sys.tables where name='TempStok')
                              if (@tempStok = 0)
                                 create table TempStok(Id int, MalinCinsi nvarchar(500),MalinCinsi2 nvarchar(500),BIRIMADI nvarchar(500), STOKKODU nvarchar(500) )");

                var locked = new Object();
                //şubenin arctostan stokıd si alınmalı
                foreach (var item in SubeList)
                {
                    foreach (var sube in resultSubeList)
                    {
                        if (sube.ID.ToString() == item.Value)
                        {
                            DataTable dattb = f.GetSubeDataWithQuery((f.NewConnectionString(sube.SubeIP, sube.DBName, sube.SqlName, sube.SqlPassword)), "select * from Product");
                            DataTable datprchtb = f.GetSubeDataWithQuery(f.NewConnectionString(sube.SubeIP, sube.DBName, sube.SqlName, sube.SqlPassword),
                                @"SELECT Product.Id as ProductId, ProductName + COALESCE('.' + Choice1.Name, '') + COALESCE('.' + Choice2.Name, '') ProductName,
                                         " + item.Value + @",COALESCE('.' + Choice1.Name, '') + COALESCE('.' + Choice2.Name, '') ProductNameEk
                                        FROM Product
                                        LEFT JOIN Choice1 ON Choice1.ProductId = Product.Id
                                        LEFT JOIN Choice2 ON Choice2.ProductId = Product.Id AND Choice2.Choice1Id = Choice1.Id
                                        WHERE  ProductName NOT LIKE '%\[R]%' ESCAPE '\'" +
                                        " ORDER BY COALESCE('.'+Choice1.Name,'') + COALESCE('.'+Choice2.Name,'')");

                            //foreach (DataRow sablonProductChoice in datprchtb.Rows){
                            Parallel.ForEach(datprchtb.AsEnumerable().ToList<DataRow>(), sablonProductChoice =>
                            {
                                lock (locked)
                                {
                                    f.DataTable("insert into TempProductSube (Id,ProductName,SubeId,ProductNameEk) values (" + f.RTS(sablonProductChoice, "ProductId") + ",'" + f.RTS(sablonProductChoice, "ProductName").Replace("'", "''") + "'," + item.Value + ",'" + f.RTS(sablonProductChoice, "ProductNameEk").Replace("'", "''") + "')");
                                }
                                //}
                            });


                            //foreach (DataRow sablonProduct in dattb.Rows)
                            //{
                            Parallel.ForEach(dattb.AsEnumerable().ToList<DataRow>(), sablonProduct =>
                            {
                                lock (locked)
                                {
                                    f.DataTable("insert into TempProductSube (Id,ProductName,SubeId) values (" + f.RTS(sablonProduct, "Id") + ",'" + f.RTS(sablonProduct, "ProductName").Replace("'", "''") + "'," + item.Value + ")");

                                    f.DataTable(@"insert into TempProductSube (Id,ProductName,SubeId,ProductNameEk)
                                                SELECT
	                                                 Product.Id as ProductId,ProductName+ COALESCE('.'+Choice1.Name,'') + COALESCE('.'+Choice2.Name,'') ProductName,
	                                                " + item.Value + @", COALESCE('.'+Choice1.Name,'') + COALESCE('.'+Choice2.Name,'') ProductNameEk
                                                FROM Product
                                                LEFT JOIN Choice1 ON  Choice1.ProductId = Product.Id
                                                LEFT JOIN Choice2 ON  Choice2.ProductId = Product.Id  AND  Choice2.Choice1Id=Choice1.Id
                                                 WHERE  ProductName NOT LIKE '%\[R]%' ESCAPE '\' and Product.Id=" + f.RTS(sablonProduct, "Id") + @" and Choice1.SubeId=" + item.Value + @" and Choice2.SubeId=" + item.Value +
                                                     "ORDER BY COALESCE('.'+Choice1.Name,'') + COALESCE('.'+Choice2.Name,'')");
                                }
                                //}
                            });
                        }

                        if (sube.ID.ToString() == MainSubeList[0].Value)
                        {

                            DataTable datBomtb = f.GetSubeDataWithQuery(f.NewConnectionString(sube.SubeIP, sube.DBName, sube.SqlName, sube.SqlPassword), "select * from Bom where ProductId is not null");
                            //foreach (DataRow sablonBom in datBomtb.Rows)
                            //{
                            Parallel.ForEach(datBomtb.AsEnumerable().ToList<DataRow>(), sablonBom =>
                            {
                                lock (locked)
                                {
                                    f.DataTable("insert into TempBom (ProductName,MaterialName,Quantity,Unit,StokID,ProductId,SubeId) values ('" + f.RTS(sablonBom, "ProductName").Replace("'", "''") + "','" + f.RTS(sablonBom, "MaterialName").Replace("'", "''") + "'," + f.RTS(sablonBom, "Quantity").Replace(",", ".") + ",'" + f.RTS(sablonBom, "Unit").Replace("'", "''") + "'," + f.RTS(sablonBom, "StokID") + "," + f.RTS(sablonBom, "ProductId") + "," + item.Value + ")");
                                }
                                //}
                            });

                            DataTable datBomOptionstb = f.GetSubeDataWithQuery((f.NewConnectionString(sube.SubeIP, sube.DBName, sube.SqlName, sube.SqlPassword)), "select * from BomOptions");
                            //foreach (DataRow sablonBomOptions in datBomOptionstb.Rows)
                            //{
                            Parallel.ForEach(datBomOptionstb.AsEnumerable().ToList<DataRow>(), sablonBomOptions =>
                            {
                                lock (locked)
                                {
                                    f.DataTable("insert into TempBomOptions (OptionsId,OptionsName,MaterialName,Quantity,Unit,StokID,ProductName,SubeId) values (" + f.RTS(sablonBomOptions, "OptionsId") + ",'" + f.RTS(sablonBomOptions, "OptionsName").Replace("'", "''") + "','" + f.RTS(sablonBomOptions, "MaterialName").Replace("'", "''") + "'," + f.RTS(sablonBomOptions, "Quantity").Replace(",", ".") + ",'" + f.RTS(sablonBomOptions, "Unit").Replace("'", "''") + "'," + f.RTS(sablonBomOptions, "StokID") + ",'" + f.RTS(sablonBomOptions, "ProductName").Replace("'", "''") + "'," + item.Value + ")");
                                }
                            });
                            //}
                        }
                    }
                }

                foreach (var item in SubeList)
                {
                    foreach (var sube in resultSubeList)
                    {
                        if (sube.ID.ToString() == item.Value)
                        {
                            DataTable dattb = f.GetSubeDataWithQuery(f.NewConnectionString(sube.SubeIP, sube.DBName, sube.SqlName, sube.SqlPassword), "select * from Product");
                            //foreach (DataRow sablonProduct in dattb.Rows)
                            //{
                            Parallel.ForEach(dattb.AsEnumerable().ToList<DataRow>(), sablonProduct =>
                            {
                                lock (locked)
                                {
                                    f.DataTable("insert into TempProduct (Id,ProductName) values (" + f.RTS(sablonProduct, "Id") + ",'" + f.RTS(sablonProduct, "ProductName").Replace("'", "''") + "')");
                                }
                            });
                            //}

                            DataTable datStoktb = f.GetSubeDataWithQuery(connString, "select *,S.STOKKODU + ' - ' + S.MALINCINSI + ' - ' + ISNULL(B.BARCODE,'') AS STOK from F0" + sube.FirmaID + "TBLSTOKLAR S, F0" + sube.FirmaID + "TBLBIRIMLEREX B WHERE S.ANABIRIM=B.IND and STATUS=1");
                            //foreach (DataRow sablonStok in datStoktb.Rows)
                            //{
                            Parallel.ForEach(datStoktb.AsEnumerable().ToList<DataRow>(), sablonStok =>
                            {
                                lock (locked)
                                {
                                    f.DataTable(@"insert into TempStok (Id,MalinCinsi,MalinCinsi2,BIRIMADI,STOKKODU) values (" + f.RTS(sablonStok, "IND") + ",'" + f.RTS(sablonStok, "STOK").Replace("'", "''") + "','" + f.RTS(sablonStok, "MALINCINSI").Replace("'", "''") + "','" + f.RTS(sablonStok, "BIRIMADI").Replace("'", "''") + "','" + f.RTS(sablonStok, "STOKKODU").Replace("'", "''") + "')");
                                    //"SELECT S.STOKKODU + ' - ' + S.MALINCINSI + ' - ' + ISNULL(B.BARCODE,'') AS STOK, S.IND STOKIND from F0" + TanimId + "TBLSTOKLAR S,F0" + TanimId + "TBLBIRIMLEREX B WHERE S.ANABIRIM=B.IND AND STATUS=1";
                                }
                            });
                            //}
                        }
                    }

                    DataTable dt = f.DataTable(@"insert into Bom ([ProductName],[MaterialName],[Quantity],[Unit],[StokID],[Aktarildi],[ProductId],[SubeId],[YeniUrunMu]) 
                                                   select ProductName,MaterialName,Quantity,Unit,StokID,Aktarildi,Id,Sube,YeniUrunMu from (
                                                select bom.[ProductName],(select top 1 s.MalinCinsi2 from TempStok s where (s.MalinCinsi=bom.[MaterialName] or s.MalinCinsi2=bom.[MaterialName] or s.STOKKODU=bom.[MaterialName]) and s.BIRIMADI=bom.Unit) MaterialName,bom.[Quantity],bom.[Unit],(select top 1 s.Id from TempStok s where (s.MalinCinsi=bom.[MaterialName] or s.MalinCinsi2=bom.[MaterialName] or s.STOKKODU=bom.[MaterialName]) and s.BIRIMADI=bom.Unit) as StokID,bom.[Aktarildi],Product.Id," + item.Value + @" as Sube ,bom.[YeniUrunMu] from bom 
                                                    inner join Product on Product.ProductName=bom.ProductName and Product.SubeId=" + item.Value + @"
                                                    Where  bom.SubeId in(" + MainSubeIds + @")
                                                union
                                                   select bom.[ProductName],(select top 1 s.MalinCinsi2 from TempStok s where (s.MalinCinsi=bom.[MaterialName] or s.MalinCinsi2=bom.[MaterialName] or s.STOKKODU=bom.[MaterialName]) and s.BIRIMADI=bom.Unit) MaterialName,bom.[Quantity],bom.[Unit],(select top 1 s.Id from TempStok s where (s.MalinCinsi=bom.[MaterialName] or s.MalinCinsi2=bom.[MaterialName] or s.STOKKODU=bom.[MaterialName]) and s.BIRIMADI=bom.Unit) as StokID,bom.[Aktarildi],TempProduct.Id," + item.Value + @" as Sube ,bom.[YeniUrunMu] from bom 
                                                    inner join TempProduct on TempProduct.ProductName = bom.ProductName
                                                    Where  bom.SubeId in (" + MainSubeIds + @")
                                                union
                                                   select bom.[ProductName],(select top 1 s.MalinCinsi2 from TempStok s where (s.MalinCinsi=bom.[MaterialName] or s.MalinCinsi2=bom.[MaterialName] or s.STOKKODU=bom.[MaterialName]) and s.BIRIMADI=bom.Unit) MaterialName,bom.[Quantity],bom.[Unit],(select top 1 s.Id from TempStok s where (s.MalinCinsi=bom.[MaterialName] or s.MalinCinsi2=bom.[MaterialName] or s.STOKKODU=bom.[MaterialName]) and s.BIRIMADI=bom.Unit) as StokID,bom.[Aktarildi],TempProductSube.Id," + item.Value + @" as Sube ,bom.[YeniUrunMu] from bom 
                                                    inner join TempProductSube on TempProductSube.ProductName = bom.ProductName and TempProductSube.SubeId=" + item.Value + @"
                                                    Where  bom.SubeId in (" + MainSubeIds + @")
                                                union 
                                                    select TempBom.ProductName,(select top 1 s.MalinCinsi2 from TempStok s where (s.MalinCinsi=MaterialName or s.MalinCinsi2=MaterialName or s.STOKKODU=[MaterialName]) and s.BIRIMADI=Unit) MaterialName,Quantity,Unit,(select top 1 s.Id from TempStok s where (s.MalinCinsi=MaterialName or s.MalinCinsi2=MaterialName or s.STOKKODU=[MaterialName]) and s.BIRIMADI=Unit) as StokID,NULL as Aktarildi,TempProductSube.Id," + item.Value + @" as Sube,1 as YeniUrunMu from TempBom
                                                    inner join TempProductSube on TempProductSube.ProductName=TempBom.ProductName and TempProductSube.SubeId=" + item.Value + @"
) as ForBoms where StokID is not null
                                                insert into BomOptions ([OptionsId],[OptionsName],[MaterialName],[Quantity],[Unit],[StokID],[ProductName],[SubeId],[YeniUrunMu],[ProductId]) 
                                                
                                                select OptionsId,OptionsName,MaterialName,Quantity,Unit,StokID,ProductName,Sube,YeniUrunMu,Id from (
                                                select BomOptions.[OptionsId],BomOptions.[OptionsName],(select top 1 s.MalinCinsi2 from TempStok s where (s.MalinCinsi=BomOptions.[MaterialName] or s.MalinCinsi2=BomOptions.[MaterialName] or s.STOKKODU=BomOptions.[MaterialName]) and s.BIRIMADI=BomOptions.Unit) MaterialName,BomOptions.[Quantity],BomOptions.[Unit],(select top 1 s.Id from TempStok s where (s.MalinCinsi=BomOptions.[MaterialName] or s.MalinCinsi2=BomOptions.[MaterialName] or s.STOKKODU=BomOptions.[MaterialName]) and s.BIRIMADI=BomOptions.Unit) as StokID,BomOptions.[ProductName]," + item.Value + @" as Sube,BomOptions.[YeniUrunMu],Product.Id from BomOptions 
                                                   inner join Product on Product.ProductName=BomOptions.ProductName and Product.SubeId=" + item.Value + @"
                                                   Where  BomOptions.SubeId in(" + MainSubeIds + @")
                                                union
                                                    select BomOptions.[OptionsId],BomOptions.[OptionsName],(select top 1 s.MalinCinsi2 from TempStok s where (s.MalinCinsi=BomOptions.[MaterialName] or s.MalinCinsi2=BomOptions.[MaterialName]  or s.STOKKODU=BomOptions.[MaterialName]) and s.BIRIMADI=BomOptions.Unit) MaterialName,BomOptions.[Quantity],BomOptions.[Unit],(select top 1 s.Id from TempStok s where (s.MalinCinsi=BomOptions.[MaterialName] or s.MalinCinsi2=BomOptions.[MaterialName] or s.STOKKODU=BomOptions.[MaterialName]) and s.BIRIMADI=BomOptions.Unit) as StokID,BomOptions.[ProductName]," + item.Value + @" as Sube,BomOptions.[YeniUrunMu],TempProduct.Id from BomOptions 
                                                   inner join TempProduct on TempProduct.ProductName=BomOptions.ProductName
                                                   Where  BomOptions.SubeId in(" + MainSubeIds + @")
                                                union
                                                    select BomOptions.[OptionsId],BomOptions.[OptionsName],(select top 1 s.MalinCinsi2 from TempStok s where (s.MalinCinsi=BomOptions.[MaterialName] or s.MalinCinsi2=BomOptions.[MaterialName]  or s.STOKKODU=BomOptions.[MaterialName]) and s.BIRIMADI=BomOptions.Unit) MaterialName,BomOptions.[Quantity],BomOptions.[Unit],(select top 1 s.Id from TempStok s where (s.MalinCinsi=BomOptions.[MaterialName] or s.MalinCinsi2=BomOptions.[MaterialName] or s.STOKKODU=BomOptions.[MaterialName]) and s.BIRIMADI=BomOptions.Unit) as StokID,BomOptions.[ProductName]," + item.Value + @" as Sube,BomOptions.[YeniUrunMu],TempProductSube.Id from BomOptions 
                                                   inner join TempProductSube on TempProductSube.ProductName=BomOptions.ProductName and TempProductSube.SubeId=" + item.Value + @"
                                                   Where  BomOptions.SubeId in(" + MainSubeIds + @")
                                                union 
                                                    select OptionsId,OptionsName,(select top 1 s.MalinCinsi2 from TempStok s where (s.MalinCinsi=TempBomOptions.[MaterialName] or s.MalinCinsi2=TempBomOptions.[MaterialName]  or s.STOKKODU=TempBomOptions.[MaterialName]) and s.BIRIMADI=Unit) MaterialName,Quantity,Unit,(select top 1 s.Id from TempStok s where (s.MalinCinsi=TempBomOptions.[MaterialName] or s.MalinCinsi2=TempBomOptions.[MaterialName] or s.STOKKODU=TempBomOptions.[MaterialName] ) and s.BIRIMADI=Unit) as StokID,TempBomOptions.ProductName," + item.Value + @" as Sube,1 as YeniUrunMu,TempProductSube.Id from TempBomOptions
                                                    inner join TempProductSube on TempProductSube.ProductName=TempBomOptions.ProductName and TempProductSube.SubeId=" + item.Value + @"
) as ForBomOptionss where StokID is not null");

                    f.DataTable("truncate table TempProduct");
                    f.DataTable("truncate table TempStok");
                }
                f.DataTable(@"truncate table TempProductSube
                               truncate table TempBom
                               truncate table TempBomOptions");
                f.SqlConnClose();

            }
            catch (Exception ex)
            {
                result.IsSuccess = false;
                result.UserMessage = ex.ToString();
                return result;
            }
            result.result_STR = "Başarılı";
            result.result_INT = 1;
            result.result_BOOL = true;
            result.IsSuccess = true;
            result.UserMessage = "İşlem Başarılı";
            return result;

        }

        public List<SefimPanelUrunEkleViewModel> GetProductForSube(List<int> ProductIds, List<int> subeIdList, string kullaniciId)
        {
            List<SefimPanelUrunEkleViewModel> productList = new List<SefimPanelUrunEkleViewModel>();

            foreach (var itemSubeId in subeIdList)
            {

                #region SUBSTATION LIST               
                mF.SqlConnOpen();
                DataTable dt = mF.DataTable("select * from SubeSettings Where Status=1 and Id=" + itemSubeId);
                mF.SqlConnClose();
                #endregion SUBSTATION LIST

                //Kullanıcının yetkili olduğu şube
                //23.08.2023 yorum satırına alındı 
                ////var kullaniciSubeYetkisi = UsersListCRUD.KullaniciSubeYetkiListesi(kullaniciId);

                var locked = new Object();
                int? FirmaId = null;

                foreach (DataRow r in dt.Rows)
                {
                    string subeId = mF.RTS(r, "ID");
                    //23.08.2023 yorum satırına alındı 
                    ////if (kullaniciSubeYetkisi != null && kullaniciSubeYetkisi.FR_SubeListesi.Where(x => x.SubeID == Convert.ToInt32(subeId)).Select(x => x.SubeID).Any())
                    ////{
                    string SubeAdi = mF.RTS(r, "SubeName");
                    string SubeIP = mF.RTS(r, "SubeIP");
                    string SqlName = mF.RTS(r, "SqlName");
                    string SqlPassword = mF.RTS(r, "SqlPassword");
                    string DBName = mF.RTS(r, "DBName");

                    if (mF.RTS(r, "FirmaID") != "")
                    {
                        FirmaId = Convert.ToInt32(mF.RTS(r, "FirmaID"));
                    }

                    string Query = "select * from Product where Id in(" + string.Join<int>(",", ProductIds) + ")";
                    string QueryChoice1 = "select * from Choice1 where ProductId in(" + string.Join<int>(",", ProductIds) + ")";
                    string QueryChoice2 = "select * from Choice2 where ProductId in(" + string.Join<int>(",", ProductIds) + ")";
                    string QueryOptions = "select * from Options where ProductId in(" + string.Join<int>(",", ProductIds) + ")";
                    string QueryOptionCats = "select * from OptionCats where ProductId in(" + string.Join<int>(",", ProductIds) + ")";
                    string QueryBom = "select * from bom where ProductId in(" + string.Join<int>(",", ProductIds) + ")";

                    DataTable ProductDt = new DataTable();
                    ProductDt = mF.GetSubeDataWithQuery(mF.NewConnectionString(SubeIP, DBName, SqlName, SqlPassword), Query.ToString());

                    DataTable Choice1Dt = new DataTable();
                    Choice1Dt = mF.GetSubeDataWithQuery(mF.NewConnectionString(SubeIP, DBName, SqlName, SqlPassword), QueryChoice1.ToString());

                    DataTable Choice2Dt = new DataTable();
                    Choice2Dt = mF.GetSubeDataWithQuery(mF.NewConnectionString(SubeIP, DBName, SqlName, SqlPassword), QueryChoice2.ToString());

                    DataTable OptionsDt = new DataTable();
                    OptionsDt = mF.GetSubeDataWithQuery(mF.NewConnectionString(SubeIP, DBName, SqlName, SqlPassword), QueryOptions.ToString());

                    DataTable OptionCatsDt = new DataTable();
                    OptionCatsDt = mF.GetSubeDataWithQuery(mF.NewConnectionString(SubeIP, DBName, SqlName, SqlPassword), QueryOptionCats.ToString());

                    DataTable dtBoms = mF.GetSubeDataWithQuery(mF.NewConnectionString(SubeIP, DBName, SqlName, SqlPassword), QueryBom.ToString());

                    var masterBoms = GetBoms2Birlesecek(ProductIds, Convert.ToInt32(subeId));
                    var masterBomOptions = GetBomOptions2Birlesecek(ProductIds, Convert.ToInt32(subeId));

                    var getHamMaddeNameList = AlisBelgesiCRUD.GetStokSelectList3(FirmaId.Value, 0);

                    //Choice1Dt = mF.GetSubeDataWithQuery((mF.NewConnectionString(SubeIP, DBName, SqlName, SqlPassword)), QueryChoice1.ToString());
                    //foreach (DataRow prdct in ProductDt.Rows)
                    //{
                    Parallel.ForEach(ProductDt.AsEnumerable(), prdct =>
                    {
                        SefimPanelUrunEkleViewModel product = new SefimPanelUrunEkleViewModel
                        {
                            Id = mF.RTI(prdct, "Id"),
                            ProductName = mF.RTS(prdct, "ProductName"),
                            ProductGroup = mF.RTS(prdct, "ProductGroup"),
                            ProductCode = mF.RTS(prdct, "ProductCode"),
                            Order = mF.RTS(prdct, "Order"),
                            Price = mF.RTS(prdct, "Price").Replace(",", "."),
                            VatRate = mF.RTS(prdct, "VatRate").Replace(",", "."),
                            InvoiceName = mF.RTS(prdct, "InvoiceName"),
                            ProductType = mF.RTS(prdct, "ProductType"),
                            Plu = mF.RTS(prdct, "Plu"),
                            ProductPkId = mF.RTI(prdct, "ProductPkId"),
                            SubeId = Convert.ToInt32(subeId),
                            SubeName = SubeAdi,
                            Favorites = mF.RTS(prdct, "Favorites")
                        };

                        if (prdct["FreeItem"] != DBNull.Value)
                        {
                            product.FreeItem = Convert.ToBoolean(mF.RTS(prdct, "FreeItem"));
                        }

                        if (prdct["SkipOptionSelection"] != DBNull.Value)
                        {
                            product.SkipOptionSelection = Convert.ToBoolean(mF.RTS(prdct, "SkipOptionSelection"));
                        }

                        if (prdct["Aktarildi"] != DBNull.Value)
                        {
                            product.Aktarildi = Convert.ToBoolean(mF.RTS(prdct, "Aktarildi"));
                        }

                        product.Choice1 = new List<Choice1>();
                        var dtChoice1List = Choice1Dt.AsEnumerable().Where(x => x.Field<int>("ProductId") == product.Id).ToList<DataRow>();
                        //foreach (DataRow ch1 in Choice1Dt.AsEnumerable().Where(x => x.Field<int>("ProductId") == product.Id))
                        //{
                        Parallel.ForEach(dtChoice1List, ch1 =>
                            {
                                Choice1 choice1 = new Choice1
                                {
                                    Id = mF.RTI(ch1, "Id"),
                                    Name = mF.RTS(ch1, "Name"),
                                    Price = mF.RTS(ch1, "Price").Replace(",", "."),
                                    ProductId = mF.RTI(ch1, "ProductId")
                                };

                                //product.Choice1.Add(choice1);
                                lock (locked)
                                {
                                    product.Choice1.Add(choice1);
                                }
                                //}
                            });

                        product.Choice2 = new List<Choice2>();
                        var dtChoice2List = Choice2Dt.AsEnumerable().Where(x => x.Field<int>("ProductId") == product.Id).ToList<DataRow>();
                        //foreach (DataRow ch2 in Choice2Dt.AsEnumerable().Where(x => x.Field<int>("ProductId") == product.Id))
                        //{
                        Parallel.ForEach(dtChoice1List, ch2 =>
                            {
                                Choice2 choice2 = new Choice2
                                {
                                    Id = mF.RTI(ch2, "Id"),
                                    Name = mF.RTS(ch2, "Name"),
                                    Price = mF.RTS(ch2, "Price").Replace(",", "."),
                                    Choice1Id = mF.RTI(ch2, "Choice1Id"),
                                    ProductId = mF.RTI(ch2, "ProductId")
                                };

                                //product.Choice2.Add(choice2);
                                lock (locked)
                                {
                                    product.Choice2.Add(choice2);
                                }
                                //}
                            });

                        product.Options = new List<Options>();
                        var dtOptionsList = OptionsDt.AsEnumerable().Where(x => x.Field<int>("ProductId") == product.Id).ToList<DataRow>();
                        Parallel.ForEach(dtOptionsList, opt =>
                            {
                                // foreach (DataRow opt in OptionsDt.AsEnumerable().Where(x => x.Field<int>("ProductId") == product.Id))
                                //{
                                Options options = new Options
                                {
                                    Id = mF.RTI(opt, "Id"),
                                    Name = mF.RTS(opt, "Name"),
                                    Price = mF.RTS(opt, "Price").Replace(",", "."),
                                    Quantitative = Convert.ToBoolean(mF.RTS(opt, "Quantitative")),
                                    Category = mF.RTS(opt, "Category"),
                                    ProductId = mF.RTI(opt, "ProductId")
                                };

                                //product.Options.Add(options);
                                lock (locked)
                                {
                                    product.Options.Add(options);
                                }
                                //}
                            });

                        product.OptionCats = new List<OptionCats>();
                        var dtOptionsCatsList = OptionCatsDt.AsEnumerable().Where(x => x.Field<int>("ProductId") == product.Id).ToList<DataRow>();
                        //foreach (DataRow optCat in OptionCatsDt.AsEnumerable().Where(x => x.Field<int>("ProductId") == product.Id))
                        //{
                        Parallel.ForEach(dtOptionsCatsList, optCat =>
                            {
                                OptionCats optionCats = new OptionCats
                                {
                                    Id = mF.RTI(optCat, "Id"),
                                    Name = mF.RTS(optCat, "Name"),
                                    MaxSelections = mF.RTS(optCat, "MaxSelections").Replace(",", "."),
                                    MinSelections = mF.RTS(optCat, "MinSelections").Replace(",", "."),
                                    ProductId = mF.RTI(optCat, "ProductId")
                                };

                                //product.OptionCats.Add(optionCats);
                                lock (locked)
                                {
                                    product.OptionCats.Add(optionCats);
                                }
                                //}
                            });

                        //
                        //var items = AlisBelgesiCRUD.GetStokSelectList2(new UserCRUD().GetUserForSubeSettings(kullaniciId).FirmaID,);//generic alınmalı                      
                        //Singleton.WritingLog("SefimPanelUrunEkleCRUD_Copy2", " UrunId: " + ProductId + " SubeId: " + subeId + " KullaniciId: " + kullaniciId + " QueryBom:" + QueryBom.ToString());

                        product.Boms = new List<Bom>();
                        var dtBomsList = dtBoms.AsEnumerable().Where(x => x.Field<int>("ProductId") == product.Id).ToList<DataRow>();

                        //foreach (DataRow bm in dtBoms.AsEnumerable().Where(x => x.Field<int>("ProductId") == product.Id))
                        //{
                        Parallel.ForEach(dtBomsList, bm =>
                            {
                                var bomStokId = new Bom
                                {
                                    StokID = mF.RTI(bm, "StokID")
                                };

                                var hamMaddeName = string.Empty;
                                //var getHamMaddeName = AlisBelgesiCRUD.GetStokSelectList2(new UserCRUD().GetUserForSubeSettingsForSubeId(subeId).FirmaID, bomStokId.StokID);
                                //var getHamMaddeName = AlisBelgesiCRUD.GetStokSelectList2(FirmaId.Value, bomStokId.StokID);


                                if (getHamMaddeNameList != null && getHamMaddeNameList.Count > 0)
                                {
                                    foreach (var item in getHamMaddeNameList.ToList().Where(x => x.StokID == bomStokId.StokID))
                                    {
                                        hamMaddeName = item.MalinCinsi;
                                    }
                                    // hamMaddeName = getHamMaddeName.Select(x => x.MalinCinsi).ToString();
                                }
                                //Singleton.WritingLog("SefimPanelUrunEkleCRUD_Copy2", "bomStokId:" + bomStokId.StokID + " UrunId: " + ProductId + " SubeId: " + subeId + " KullaniciId: " + kullaniciId + " QueryBom:" + QueryBom.ToString());

                                Bom bom = new Bom
                                {
                                    Id = mF.RTI(bm, "Id"),
                                    ProductName = mF.RTS(bm, "ProductName"),
                                    MaterialName = mF.RTS(bm, "MaterialName"),
                                    MalinCinsi = hamMaddeName, // mF.RTS(bm, "MalinCinsi"),
                                    Quantity = mF.RTS(bm, "Quantity").Replace(",", "."),
                                    Unit = mF.RTS(bm, "Unit"),
                                    StokID = mF.RTI(bm, "StokID"),
                                    ProductId = mF.RTI(bm, "ProductId"),
                                    SubeId = Convert.ToInt32(subeId)
                                };
                                //product.Boms.Add(bom);

                                lock (locked)
                                {
                                    product.Boms.Add(bom);
                                }
                                //}
                            });


                        string QueryBomOptions = "select * from BomOptions where ProductName=";

                        product.BomOptionss = new List<BomOptions>();

                        if (product.Choice1 != null && product.Choice1.Count() == 0)
                        {

                            DataTable dtBomOptionss = mF.GetSubeDataWithQuery(mF.NewConnectionString(SubeIP, DBName, SqlName, SqlPassword), QueryBomOptions.ToString() + "'" + product.ProductName + "'");

                            //foreach (DataRow bmOp in dtBomOptionss.Rows)
                            //{
                            Parallel.ForEach(dtBomOptionss.AsEnumerable(), bmOp =>
                                {
                                    BomOptions bomOps = new BomOptions
                                    {
                                        Id = mF.RTI(bmOp, "Id"),
                                        ProductName = mF.RTS(bmOp, "ProductName"),
                                        MaterialName = mF.RTS(bmOp, "MaterialName"),
                                        Quantity = mF.RTS(bmOp, "Quantity").Replace(",", "."),
                                        Unit = mF.RTS(bmOp, "Unit"),
                                        StokID = mF.RTI(bmOp, "StokID"),
                                        OptionsId = mF.RTI(bmOp, "OptionsId"),
                                        OptionsName = mF.RTS(bmOp, "OptionsName"),
                                        SubeId = Convert.ToInt32(subeId)
                                    };
                                    //product.BomOptionss.Add(bomOps);
                                    lock (locked)
                                    {
                                        product.BomOptionss.Add(bomOps);
                                    }
                                    //}
                                });

                        }
                        else
                        {
                            foreach (var ch1 in product.Choice1)
                            {
                                string productEk = "." + ch1.Name;
                                DataTable dtBomOptionss = mF.GetSubeDataWithQuery(mF.NewConnectionString(SubeIP, DBName, SqlName, SqlPassword), QueryBomOptions.ToString() + "'" + product.ProductName + productEk + "'");

                                //foreach (DataRow bmOp in dtBomOptionss.Rows)
                                //{
                                Parallel.ForEach(dtBomOptionss.AsEnumerable(), bmOp =>
                                    {
                                        BomOptions bomOps = new BomOptions
                                        {
                                            Id = mF.RTI(bmOp, "Id"),
                                            ProductName = mF.RTS(bmOp, "ProductName"),
                                            MaterialName = mF.RTS(bmOp, "MaterialName"),
                                            Quantity = mF.RTS(bmOp, "Quantity").Replace(",", "."),
                                            Unit = mF.RTS(bmOp, "Unit"),
                                            StokID = mF.RTI(bmOp, "StokID"),
                                            OptionsId = mF.RTI(bmOp, "OptionsId"),
                                            OptionsName = mF.RTS(bmOp, "OptionsName"),
                                            SubeId = Convert.ToInt32(subeId)
                                        };

                                        //product.BomOptionss.Add(bomOps);
                                        lock (locked)
                                        {
                                            product.BomOptionss.Add(bomOps);
                                        }
                                        //}
                                    });

                                foreach (var ch2 in product.Choice2)
                                {
                                    if (ch1.Id == ch2.Choice1Id)
                                    {
                                        var productEk2 = "." + ch2.Name;

                                        DataTable dtBomOptions = mF.GetSubeDataWithQuery(mF.NewConnectionString(SubeIP, DBName, SqlName, SqlPassword), QueryBomOptions.ToString() + "'" + product.ProductName + productEk + productEk2 + "'");
                                        //product.BomOptionss = new List<BomOptions>();
                                        //foreach (DataRow bmOp in dtBomOptions.Rows)
                                        //{
                                        Parallel.ForEach(dtBomOptions.AsEnumerable(), bmOp =>
                                            {
                                                BomOptions bomOps = new BomOptions
                                                {
                                                    Id = mF.RTI(bmOp, "Id"),
                                                    ProductName = mF.RTS(bmOp, "ProductName"),
                                                    MaterialName = mF.RTS(bmOp, "MaterialName"),
                                                    Quantity = mF.RTS(bmOp, "Quantity").Replace(",", "."),
                                                    Unit = mF.RTS(bmOp, "Unit"),
                                                    StokID = mF.RTI(bmOp, "StokID"),
                                                    OptionsId = mF.RTI(bmOp, "OptionsId"),
                                                    OptionsName = mF.RTS(bmOp, "OptionsName"),
                                                    SubeId = Convert.ToInt32(subeId)
                                                };
                                                //product.BomOptionss.Add(bomOps);

                                                lock (locked)
                                                {
                                                    product.BomOptionss.Add(bomOps);
                                                }
                                                //}
                                            });
                                    }
                                }


                            }
                        }

                        if (product.Boms == null)
                        {
                            product.Boms = new List<Bom>();
                        }
                        //Masterdaki bom ve bomoptions çekiliyor şubeye bağlı
                        foreach (var item in masterBoms.Where(x => x.ProductId == product.Id))
                        {
                            product.Boms.Add(item);
                        };
                        if (product.BomOptionss == null)
                        {
                            product.BomOptionss = new List<BomOptions>();
                        }
                        foreach (var item in masterBomOptions.Where(x => x.ProductId == product.Id))
                        {
                            product.BomOptionss.Add(item);
                        };


                        lock (locked)
                        {
                            productList.Add(product);
                        }


                        //}
                    });


                }
            }

            return productList;
        }

        public static List<SefimPanelUrunEkleViewModel> ProductList(string SubeIds)
        {
            var list = new List<SefimPanelUrunEkleViewModel>();
            ModelFunctions f = new ModelFunctions();

            BussinessHelper.InvoiceNames = new List<SelectListItem>();
            BussinessHelper.ProductGroups = new List<SelectListItem>();
            BussinessHelper.ProductTypes = new List<SelectListItem>();
            BussinessHelper.Favoritess = new List<SelectListItem>();

            try
            {
                #region SUBSTATION LIST  

                f.SqlConnOpen();
                DataTable dt = f.DataTable("select * from SubeSettings Where Status=1 and ID in(" + SubeIds + ")");
                var dtList = dt.AsEnumerable().ToList<DataRow>();

                DataTable dtMasterBoms = f.DataTable("select * from bom Where SubeId in(" + SubeIds + ")");
                DataTable dtMasterBomOptions = f.DataTable("select * from bomOptions where  SubeId in(" + SubeIds + ")");

                f.SqlConnClose();

                #endregion SUBSTATION LIST

                #region PARALLEL FOREACH

                var locked = new Object();
                Parallel.ForEach(dtList, r =>
                {
                    //foreach (DataRow r in dt.Rows)
                    //{
                    string SubeId = f.RTS(r, "ID");
                    string SubeAdi = f.RTS(r, "SubeName");
                    string SubeIP = f.RTS(r, "SubeIP");
                    string SqlName = f.RTS(r, "SqlName");
                    string SqlPassword = f.RTS(r, "SqlPassword");
                    string DBName = f.RTS(r, "DBName");
                    string Query = "select * from Product";

                    string queryChoice1 = "select * from Choice1 ";
                    string queryChoice2 = "select * from Choice2 ";
                    string queryOptions = "select * from Options ";
                    string queryOptionCats = "select * from OptionCats ";
                    string queryBom = "select * from Bom ";
                    string queryBomOptions = "select * from BomOptions where OptionsId=";

                    #region GET DATA
                    try
                    {
                        DataTable productDt = new DataTable();
                        productDt = f.GetSubeDataWithQuery(f.NewConnectionString(SubeIP, DBName, SqlName, SqlPassword), Query.ToString());
                        var productList = productDt.AsEnumerable().ToList<DataRow>();
                        DataTable dtChoice1 = f.GetSubeDataWithQuery(f.NewConnectionString(SubeIP, DBName, SqlName, SqlPassword), queryChoice1.ToString());
                        DataTable dtChoise2 = f.GetSubeDataWithQuery(f.NewConnectionString(SubeIP, DBName, SqlName, SqlPassword), queryChoice2.ToString());
                        DataTable dtOptions = f.GetSubeDataWithQuery(f.NewConnectionString(SubeIP, DBName, SqlName, SqlPassword), queryOptions.ToString());
                        DataTable dtOptionCats = f.GetSubeDataWithQuery(f.NewConnectionString(SubeIP, DBName, SqlName, SqlPassword), queryOptionCats.ToString());
                        DataTable dtBoms = f.GetSubeDataWithQuery(f.NewConnectionString(SubeIP, DBName, SqlName, SqlPassword), queryBom.ToString());

                        foreach (DataRow SubeR in productDt.Rows)
                        {
                            //Parallel.ForEach(productList, SubeR =>
                            //{
                            //items.Sube = SubeCiroDt.Rows[0]["Sube"].ToString(); //f.RTS(SubeR, "Sube");
                            //items.SubeId = SubeId;
                            //items.Cash = Convert.ToDecimal(SubeCiroDt.Rows[0]["Cash"]); //f.RTD(SubeR, "Cash");
                            SefimPanelUrunEkleViewModel items = new SefimPanelUrunEkleViewModel
                            {
                                Id = f.RTI(SubeR, "Id"),
                                ProductName = f.RTS(SubeR, "ProductName"),
                                ProductGroup = f.RTS(SubeR, "ProductGroup"),
                                ProductCode = f.RTS(SubeR, "ProductCode"),
                                Order = f.RTS(SubeR, "[Order]"),
                                Price = f.RTS(SubeR, "Price").Replace(",", "."),
                                VatRate = f.RTS(SubeR, "VatRate").Replace(",", "."),
                                InvoiceName = f.RTS(SubeR, "InvoiceName"),
                                ProductType = f.RTS(SubeR, "ProductType"),
                                Plu = f.RTS(SubeR, "Plu"),
                                Favorites = f.RTS(SubeR, "Favorites"),
                                ProductPkId = f.RTI(SubeR, "ProductPkId"),
                                SubeId = Convert.ToInt32(SubeId),
                                SubeName = SubeAdi
                            };

                            if (SubeR["FreeItem"] != DBNull.Value)
                            {
                                items.FreeItem = Convert.ToBoolean(f.RTS(SubeR, "FreeItem"));
                            }

                            if (SubeR["SkipOptionSelection"] != DBNull.Value)
                            {
                                items.SkipOptionSelection = Convert.ToBoolean(f.RTS(SubeR, "SkipOptionSelection"));
                            }

                            if (SubeR["Aktarildi"] != DBNull.Value)
                            {
                                items.Aktarildi = Convert.ToBoolean(f.RTS(SubeR, "Aktarildi"));
                            }


                            //items.Adet = f.RTI(SubeR, "ADET");
                            //items.Debit = f.RTD(SubeR, "TUTAR");
                            //items.PhoneOrderDebit = f.RTI(SubeR, "PhoneOrderDebit");

                            items.Choice1 = new List<Choice1>();
                            items.Choice2 = new List<Choice2>();
                            items.Options = new List<Options>();
                            items.OptionCats = new List<OptionCats>();
                            items.Boms = new List<Bom>();
                            items.BomOptionss = new List<BomOptions>();

                            var dtChoice1List = dtChoice1.AsEnumerable().Where(x => x.Field<int>("ProductId") == items.Id).ToList<DataRow>();
                            Parallel.ForEach(dtChoice1List, choise1 =>
                            {
                                Choice1 choice1 = new Choice1
                                {
                                    Id = f.RTI(choise1, "Id"),
                                    Name = f.RTS(choise1, "Name"),
                                    Price = f.RTS(choise1, "Price").Replace(",", "."),
                                    ProductId = f.RTI(choise1, "ProductId")
                                };
                                lock (locked)
                                {
                                    items.Choice1.Add(choice1);
                                }
                            });

                            var dtChoice2List = dtChoise2.AsEnumerable().Where(x => x.Field<int>("ProductId") == items.Id).ToList<DataRow>();
                            Parallel.ForEach(dtChoice2List, choise2 =>
                            {
                                Choice2 choice2 = new Choice2
                                {
                                    Id = f.RTI(choise2, "Id"),
                                    Name = f.RTS(choise2, "Name"),
                                    Price = f.RTS(choise2, "Price").Replace(",", "."),
                                    Choice1Id = f.RTI(choise2, "Choice1Id"),
                                    ProductId = f.RTI(choise2, "ProductId")
                                };
                                lock (locked)
                                {
                                    items.Choice2.Add(choice2);
                                }
                            });

                            var dtOptionsList = dtOptions.AsEnumerable().Where(x => x.Field<int>("ProductId") == items.Id).ToList<DataRow>();
                            Parallel.ForEach(dtOptionsList, opt =>
                            {
                                Options options = new Options();
                                options.Id = f.RTI(opt, "Id");
                                options.Name = f.RTS(opt, "Name");
                                options.Price = f.RTS(opt, "Price").Replace(",", ".");
                                if (opt["Quantitative"] != DBNull.Value)
                                    options.Quantitative = Convert.ToBoolean(f.RTS(opt, "Quantitative"));
                                options.Category = f.RTS(opt, "Category");
                                options.ProductId = f.RTI(opt, "ProductId");


                                DataTable dtBomOptionsCount = f.GetSubeDataWithQuery(f.NewConnectionString(SubeIP, DBName, SqlName, SqlPassword), queryBomOptions.ToString() + options.Id);
                                var dtBomOptionsList = dtBomOptionsCount.AsEnumerable().ToList<DataRow>();
                                Parallel.ForEach(dtBomOptionsList, BomOpt =>
                                {
                                    BomOptions bomOpt = new BomOptions
                                    {
                                        Id = f.RTI(BomOpt, "Id"),
                                        OptionsId = f.RTI(BomOpt, "OptionsId"),
                                        OptionsName = f.RTS(BomOpt, "OptionsName"),
                                        MaterialName = f.RTS(BomOpt, "MaterialName"),
                                        Quantity = f.RTS(BomOpt, "Quantity").Replace(",", "."),
                                        Unit = f.RTS(BomOpt, "Unit"),
                                        StokID = f.RTI(BomOpt, "StokID"),
                                        ProductName = f.RTS(BomOpt, "ProductName"),
                                        SubeId = Convert.ToInt32(SubeId)
                                    };
                                    lock (locked)
                                    {
                                        items.BomOptionss.Add(bomOpt);
                                    }
                                });

                                lock (locked)
                                {
                                    items.Options.Add(options);
                                }
                            });


                            var dtOptionCatsList = dtOptionCats.AsEnumerable().Where(x => x.Field<int>("ProductId") == items.Id).ToList<DataRow>();
                            Parallel.ForEach(dtOptionCatsList, optCat =>
                            {
                                OptionCats optionCats = new OptionCats
                                {
                                    Id = f.RTI(optCat, "Id"),
                                    Name = f.RTS(optCat, "Name"),
                                    MaxSelections = f.RTS(optCat, "MaxSelections").Replace(",", "."),
                                    MinSelections = f.RTS(optCat, "MinSelections").Replace(",", "."),
                                    ProductId = f.RTI(optCat, "ProductId")
                                };
                                lock (locked)
                                {
                                    items.OptionCats.Add(optionCats);
                                }
                            });


                            var dtBomsList = dtBoms.AsEnumerable().Where(x => x.Field<int>("ProductId") == items.Id).ToList<DataRow>();
                            Parallel.ForEach(dtBomsList, bm =>
                            {
                                Bom bom = new Bom
                                {
                                    Id = f.RTI(bm, "Id"),
                                    ProductName = f.RTS(bm, "ProductName"),
                                    MaterialName = f.RTS(bm, "MaterialName"),
                                    Quantity = f.RTS(bm, "Quantity").Replace(",", "."),
                                    Unit = f.RTS(bm, "Unit"),
                                    StokID = f.RTI(bm, "StokID"),
                                    ProductId = f.RTI(bm, "ProductId"),
                                    SubeId = Convert.ToInt32(SubeId)
                                };
                                lock (locked)
                                {
                                    items.Boms.Add(bom);
                                }
                            });

                            //Masterdaki bom ve bomoptions çekiliyor şubeye bağlı
                            foreach (var item in GetBoms2(dtMasterBoms.AsEnumerable().Where(x => x.Field<int>("ProductId") == items.Id).Where(x => x.Field<int>("SubeId") == items.SubeId).ToList<DataRow>()))
                            {
                                lock (locked)
                                {
                                    items.Boms.Add(item);
                                }
                            };

                            foreach (var item in GetBomOptions2(dtMasterBomOptions.AsEnumerable().Where(x => x.Field<int>("ProductId") == items.Id).Where(x => x.Field<int>("SubeId") == items.SubeId).ToList<DataRow>()))
                            {
                                lock (locked)
                                {
                                    items.BomOptionss.Add(item);
                                }
                            };


                            lock (locked)
                            {
                                if (!string.IsNullOrEmpty(items.InvoiceName))
                                    BussinessHelper.InvoiceNames.Add(new SelectListItem() { Text = items.InvoiceName, Value = items.InvoiceName });
                                if (!string.IsNullOrEmpty(items.ProductGroup))
                                    BussinessHelper.ProductGroups.Add(new SelectListItem() { Text = items.ProductGroup, Value = items.ProductGroup });
                                if (!string.IsNullOrEmpty(items.ProductType))
                                    BussinessHelper.ProductTypes.Add(new SelectListItem() { Text = items.ProductType, Value = items.ProductType });
                                if (!string.IsNullOrEmpty(items.Favorites))
                                    BussinessHelper.Favoritess.Add(new SelectListItem() { Text = items.Favorites, Value = items.Favorites });

                                if (items.BomOptionss.Count > 0)
                                {
                                    items.S = true;
                                }
                                else
                                {
                                    items.S = false;
                                }

                                if (items.Boms.Count > 0)
                                {
                                    items.R = true;
                                }
                                else
                                {
                                    items.R = false;
                                }

                                list.Add(items);
                            }
                        }
                        //});
                    }
                    catch (Exception ex)
                    {
                        Singleton.WritingLogFile2("SefimPanelUrunEkleCRUDGetData", ex.Message.ToString(), "", ex.StackTrace);
                    }
                    #endregion GET DATA

                });

                #endregion PARALLEL FOREACH
            }
            catch (DataException ex)
            {
                Singleton.WritingLogFile2("SefimPanelUrunEkleCRUDParallelForeach", ex.Message.ToString(), "", ex.StackTrace);
            }


            try
            {
                f.SqlConnOpen();
                DataTable dt = f.DataTable("select * from Product Where YeniUrunMu = 1 and SubeId in (" + SubeIds + ")");
                f.SqlConnClose();

                foreach (DataRow SubeR in dt.Rows)
                {
                    //items.Sube = SubeCiroDt.Rows[0]["Sube"].ToString(); //f.RTS(SubeR, "Sube");
                    //items.SubeId = SubeId;
                    //items.Cash = Convert.ToDecimal(SubeCiroDt.Rows[0]["Cash"]); //f.RTD(SubeR, "Cash");

                    List<int> ids = new List<int>()
                    {
                        f.RTI(SubeR, "Id"),
                    };

                    List<SefimPanelUrunEkleViewModel> product = GetProduct(ids);

                    foreach (var items in product)
                    {
                        if (items.BomOptionss.Count > 0)
                        {
                            items.S = true;
                        }
                        else
                        {
                            items.S = false;
                        }

                        if (items.Boms.Count > 0)
                        {
                            items.R = true;
                        }
                        else
                        {
                            items.R = false;
                        }
                        list.Add(items);
                    }
                }
            }
            catch (Exception ex)
            {
                Singleton.WritingLogFile2("SefimPanelUrunEkleCRUDGetProduct1", ex.Message.ToString(), "", ex.StackTrace);
            }

            return list;
        }

        public static List<SefimPanelUrunEkleViewModel> GetProduct(List<int> productIdList)
        {
            List<SefimPanelUrunEkleViewModel> productList = new List<SefimPanelUrunEkleViewModel>();
            ModelFunctions mf = new ModelFunctions();

            try
            {
                string QueryProduct = "select * from Product where Id in(" + string.Join<int>(",", productIdList) + ")";
                string QueryChoice1 = "select * from Choice1 where ProductId in(" + string.Join<int>(",", productIdList) + ")";
                string QueryChoice2 = "select * from Choice2 where ProductId in(" + string.Join<int>(",", productIdList) + ")";
                string QueryOptions = "select * from Options where ProductId in(" + string.Join<int>(",", productIdList) + ")";
                string QueryOptionCats = "select * from OptionCats where ProductId in(" + string.Join<int>(",", productIdList) + ")";
                string QueryBom = "select * from bom where ProductId in(" + string.Join<int>(",", productIdList) + ")";
                string QueryBomOptions = "select * from bomOptions where ProductId in(" + string.Join<int>(",", productIdList) + ")";

                mf.SqlConnOpen();
                //DataTable dt = mf.DataTable("select * from Product Where YeniUrunMu = 1 and Id=" + Id);
                DataTable dt = mf.DataTable(QueryProduct);
                DataTable dtChoise1 = mf.DataTable(QueryChoice1);
                DataTable dtChoise2 = mf.DataTable(QueryChoice2);
                DataTable dtOptions = mf.DataTable(QueryOptions);
                DataTable dtOptionCats = mf.DataTable(QueryOptionCats);
                DataTable dtBoms = mf.DataTable(QueryBom); //"select * from bom where ProductId=" + Id + " and SubeId=" + items.SubeId
                DataTable dtBomOptions = mf.DataTable(QueryBomOptions); //"select * from bomOptions where ProductId='" + Id + "' and SubeId=" + items.SubeId

                foreach (DataRow SubeR in dt.Rows)
                {
                    SefimPanelUrunEkleViewModel items = new SefimPanelUrunEkleViewModel();
                    //items.Sube = SubeCiroDt.Rows[0]["Sube"].ToString(); //f.RTS(SubeR, "Sube");
                    //items.SubeId = SubeId;
                    //items.Cash = Convert.ToDecimal(SubeCiroDt.Rows[0]["Cash"]); //f.RTD(SubeR, "Cash");
                    items.ProductName = mf.RTS(SubeR, "ProductName");
                    items.ProductGroup = mf.RTS(SubeR, "ProductGroup");
                    items.ProductCode = mf.RTS(SubeR, "ProductCode");
                    items.Order = mf.RTS(SubeR, "Order");
                    items.Price = mf.RTS(SubeR, "Price").Replace(",", ".");
                    items.VatRate = mf.RTS(SubeR, "VatRate").Replace(",", ".");
                    if (SubeR["FreeItem"] != DBNull.Value)
                    {
                        items.FreeItem = Convert.ToBoolean(mf.RTS(SubeR, "FreeItem"));
                    }
                    items.InvoiceName = mf.RTS(SubeR, "InvoiceName");
                    items.ProductType = mf.RTS(SubeR, "ProductType");
                    items.Plu = mf.RTS(SubeR, "Plu");
                    if (SubeR["SkipOptionSelection"] != DBNull.Value)
                    {
                        items.SkipOptionSelection = Convert.ToBoolean(mf.RTS(SubeR, "SkipOptionSelection"));
                    }
                    items.Favorites = mf.RTS(SubeR, "Favorites");
                    if (SubeR["Aktarildi"] != DBNull.Value)
                    {
                        items.Aktarildi = Convert.ToBoolean(mf.RTS(SubeR, "Aktarildi"));
                    }

                    items.ProductPkId = mf.RTI(SubeR, "ProductPkId");
                    items.SubeId = Convert.ToInt32(mf.RTI(SubeR, "SubeId"));
                    items.SubeName = mf.RTS(SubeR, "SubeName");
                    items.YeniUrunMu = true;
                    items.Id = mf.RTI(SubeR, "Id");

                    //items.Adet = f.RTI(SubeR, "ADET");
                    //items.Debit = f.RTD(SubeR, "TUTAR");
                    //items.PhoneOrderDebit = f.RTI(SubeR, "PhoneOrderDebit");

                    items.Choice1 = new List<Choice1>();
                    items.Choice2 = new List<Choice2>();
                    items.Options = new List<Options>();
                    items.OptionCats = new List<OptionCats>();
                    items.Boms = new List<Bom>();
                    items.BomOptionss = new List<BomOptions>();

                    foreach (DataRow choise1 in dtChoise1.AsEnumerable().Where(x => x.Field<int>("ProductId") == items.Id))
                    {
                        Choice1 choice1 = new Choice1
                        {
                            Id = mf.RTI(choise1, "Id"),
                            Name = mf.RTS(choise1, "Name"),
                            Price = mf.RTS(choise1, "Price").Replace(",", "."),
                            ProductId = mf.RTI(choise1, "ProductId")
                        };

                        items.Choice1.Add(choice1);
                    }

                    foreach (DataRow choise2 in dtChoise2.AsEnumerable().Where(x => x.Field<int>("ProductId") == items.Id))
                    {
                        Choice2 choice2 = new Choice2
                        {
                            Id = mf.RTI(choise2, "Id"),
                            Name = mf.RTS(choise2, "Name"),
                            Price = mf.RTS(choise2, "Price").Replace(",", "."),
                            Choice1Id = mf.RTI(choise2, "Choice1Id"),
                            ProductId = mf.RTI(choise2, "ProductId")
                        };

                        items.Choice2.Add(choice2);
                    }

                    foreach (DataRow opt in dtOptions.AsEnumerable().Where(x => x.Field<int>("ProductId") == items.Id))
                    {
                        Options options = new Options
                        {
                            Id = mf.RTI(opt, "Id"),
                            Name = mf.RTS(opt, "Name"),
                            Price = mf.RTS(opt, "Price").Replace(",", "."),
                            Quantitative = Convert.ToBoolean(mf.RTS(opt, "Quantitative")),
                            Category = mf.RTS(opt, "Category"),
                            ProductId = mf.RTI(opt, "ProductId")
                        };
                        items.Options.Add(options);
                    }

                    foreach (DataRow optCat in dtOptionCats.AsEnumerable().Where(x => x.Field<int>("ProductId") == items.Id))
                    {
                        OptionCats optionCats = new OptionCats
                        {
                            Id = mf.RTI(optCat, "Id"),
                            Name = mf.RTS(optCat, "Name"),
                            MaxSelections = mf.RTS(optCat, "MaxSelections").Replace(",", "."),
                            MinSelections = mf.RTS(optCat, "MinSelections").Replace(",", "."),
                            ProductId = mf.RTI(optCat, "ProductId")
                        };

                        items.OptionCats.Add(optionCats);
                    }

                    foreach (DataRow bm in dtBoms.AsEnumerable().Where(x => x.Field<int>("ProductId") == items.Id && x.Field<int>("SubeId") == items.SubeId))
                    {
                        Bom bom = new Bom
                        {
                            Id = mf.RTI(bm, "Id"),
                            ProductName = mf.RTS(bm, "ProductName"),
                            MaterialName = mf.RTS(bm, "MaterialName"),
                            Quantity = mf.RTS(bm, "Quantity").Replace(",", "."),
                            Unit = mf.RTS(bm, "Unit"),
                            StokID = mf.RTI(bm, "StokID"),
                            ProductId = mf.RTI(bm, "ProductId")
                        };
                        if (!string.IsNullOrEmpty(mf.RTS(bm, "YeniUrunMu")))
                            bom.YeniUrunMu = Convert.ToBoolean(mf.RTS(bm, "YeniUrunMu"));
                        items.Boms.Add(bom);
                    }

                    foreach (DataRow bmOp in dtBomOptions.AsEnumerable().Where(x => x.Field<int>("ProductId") == items.Id && x.Field<int>("SubeId") == items.SubeId))
                    {
                        BomOptions bomOps = new BomOptions
                        {
                            Id = mf.RTI(bmOp, "Id"),
                            ProductName = mf.RTS(bmOp, "ProductName"),
                            MaterialName = mf.RTS(bmOp, "MaterialName"),
                            Quantity = mf.RTS(bmOp, "Quantity").Replace(",", "."),
                            Unit = mf.RTS(bmOp, "Unit"),
                            StokID = mf.RTI(bmOp, "StokID"),
                            OptionsId = mf.RTI(bmOp, "OptionsId"),
                            OptionsName = mf.RTS(bmOp, "OptionsName"),
                            SubeId = mf.RTI(bmOp, "SubeId"),
                            ProductId = mf.RTI(bmOp, "ProductId")
                        };
                        if (!string.IsNullOrEmpty(mf.RTS(bmOp, "YeniUrunMu")))
                            bomOps.YeniUrunMu = Convert.ToBoolean(mf.RTS(bmOp, "YeniUrunMu"));
                        items.BomOptionss.Add(bomOps);
                    }

                    productList.Add(items);
                }

                mf.SqlConnClose();
            }
            catch (Exception ex)
            {
                Singleton.WritingLogFile2("SefimPanelUrunEkleCRUDGetProduct2", ex.Message.ToString(), "", ex.StackTrace);
            }

            return productList;
        }

        public static SefimPanelUrunEkleViewModel GetProductRemote(int Id, int SubeId)
        {
            SefimPanelUrunEkleViewModel items = new SefimPanelUrunEkleViewModel();
            ModelFunctions mf = new ModelFunctions();

            try
            {
                #region SUBSTATION LIST               
                mf.SqlConnOpen();
                DataTable dtSube = mf.DataTable("select * from SubeSettings Where Status=1 and Id=" + SubeId);
                mf.SqlConnClose();
                #endregion SUBSTATION LIST

                foreach (DataRow sube in dtSube.Rows)
                {
                    string SubeAdi = mf.RTS(sube, "SubeName");
                    string SubeIP = mf.RTS(sube, "SubeIP");
                    string SqlName = mf.RTS(sube, "SqlName");
                    string SqlPassword = mf.RTS(sube, "SqlPassword");
                    string DBName = mf.RTS(sube, "DBName");
                    string subeId = mf.RTS(sube, "ID");

                    mf.SqlConnOpen();
                    //DataTable dt = mf.DataTable("select * from Product Where YeniUrunMu = 1 and Id=" + Id);
                    //DataTable dt = mf.DataTable("select * from Product Where  Id=" + Id);
                    string query = "select * from Product Where  Id=" + Id;
                    //string QueryChoice1 = "select* from Choice1 Where ProductId = " + Id;
                    //string QueryChoice2 = "select * from Choice2 Where ProductId=" + Id;
                    //string QueryOptions = "select * from Options where ProductId=" + ProductId;
                    //string QueryOptionCats = "select * from OptionCats where ProductId=" + ProductId;
                    //string QueryBom = "select * from bom where ProductId=" + ProductId;


                    DataTable ProductDt = new DataTable();
                    ProductDt = mf.GetSubeDataWithQuery(mf.NewConnectionString(SubeIP, DBName, SqlName, SqlPassword), query.ToString());
                    string QueryChoice1 = "select* from Choice1 Where ProductId = " + Id;

                    DataTable dtChoise1 = new DataTable();
                    dtChoise1 = mf.GetSubeDataWithQuery(mf.NewConnectionString(SubeIP, DBName, SqlName, SqlPassword), QueryChoice1.ToString());

                    string QueryChoice2 = "select * from Choice2 Where ProductId=" + Id;
                    DataTable dtChoise2 = new DataTable();
                    dtChoise2 = mf.GetSubeDataWithQuery(mf.NewConnectionString(SubeIP, DBName, SqlName, SqlPassword), QueryChoice2.ToString());

                    string QueryOptions = "select * from Options Where ProductId=" + Id;
                    DataTable dtOptions = new DataTable();
                    dtOptions = mf.GetSubeDataWithQuery(mf.NewConnectionString(SubeIP, DBName, SqlName, SqlPassword), QueryOptions.ToString());

                    string QueryOptionCats = "select* from OptionCats Where ProductId = " + Id;
                    DataTable dtOptionCats = new DataTable();
                    dtOptionCats = mf.GetSubeDataWithQuery(mf.NewConnectionString(SubeIP, DBName, SqlName, SqlPassword), QueryOptionCats.ToString());

                    //var queryBoms = "select* from bom where ProductId = " + Id + " and SubeId = " + items.SubeId;
                    var queryBoms = "select* from bom where ProductId = " + Id;
                    DataTable dtBoms = new DataTable();
                    dtBoms = mf.GetSubeDataWithQuery(mf.NewConnectionString(SubeIP, DBName, SqlName, SqlPassword), queryBoms);

                    var queryBomOptions = "select* from bom where ProductId = " + Id;
                    DataTable dtBomOptions = new DataTable();
                    dtBoms = mf.GetSubeDataWithQuery(mf.NewConnectionString(SubeIP, DBName, SqlName, SqlPassword), queryBomOptions);

                    foreach (DataRow SubeR in ProductDt.Rows)
                    {
                        //items.Sube = SubeCiroDt.Rows[0]["Sube"].ToString(); //f.RTS(SubeR, "Sube");
                        //items.SubeId = SubeId;
                        //items.Cash = Convert.ToDecimal(SubeCiroDt.Rows[0]["Cash"]); //f.RTD(SubeR, "Cash");
                        items.ProductName = mf.RTS(SubeR, "ProductName");
                        items.ProductGroup = mf.RTS(SubeR, "ProductGroup");
                        items.ProductCode = mf.RTS(SubeR, "ProductCode");
                        items.Order = mf.RTS(SubeR, "Order");
                        items.Price = mf.RTS(SubeR, "Price").Replace(",", ".");
                        items.VatRate = mf.RTS(SubeR, "VatRate").Replace(",", ".");
                        if (SubeR["FreeItem"] != DBNull.Value)
                        {
                            items.FreeItem = Convert.ToBoolean(mf.RTS(SubeR, "FreeItem"));
                        }
                        items.InvoiceName = mf.RTS(SubeR, "InvoiceName");
                        items.ProductType = mf.RTS(SubeR, "ProductType");
                        items.Plu = mf.RTS(SubeR, "Plu");
                        if (SubeR["SkipOptionSelection"] != DBNull.Value)
                        {
                            items.SkipOptionSelection = Convert.ToBoolean(mf.RTS(SubeR, "SkipOptionSelection"));
                        }
                        items.Favorites = mf.RTS(SubeR, "Favorites");
                        if (SubeR["Aktarildi"] != DBNull.Value)
                        {
                            items.Aktarildi = Convert.ToBoolean(mf.RTS(SubeR, "Aktarildi"));
                        }

                        items.ProductPkId = mf.RTI(SubeR, "ProductPkId");
                        items.SubeId = Convert.ToInt32(mf.RTI(SubeR, "SubeId"));
                        items.SubeName = mf.RTS(SubeR, "SubeName");
                        items.YeniUrunMu = true;
                        items.Id = mf.RTI(SubeR, "Id");

                        //items.Adet = f.RTI(SubeR, "ADET");
                        //items.Debit = f.RTD(SubeR, "TUTAR");
                        //items.PhoneOrderDebit = f.RTI(SubeR, "PhoneOrderDebit");

                        items.Choice1 = new List<Choice1>();
                        items.Choice2 = new List<Choice2>();
                        items.Options = new List<Options>();
                        items.OptionCats = new List<OptionCats>();
                        items.Boms = new List<Bom>();
                        items.BomOptionss = new List<BomOptions>();

                        foreach (DataRow choise1 in dtChoise1.Rows)
                        {
                            var choice1 = new Choice1
                            {
                                Id = mf.RTI(choise1, "Id"),
                                Name = mf.RTS(choise1, "Name"),
                                Price = mf.RTS(choise1, "Price").Replace(",", "."),
                                ProductId = mf.RTI(choise1, "ProductId")
                            };

                            items.Choice1.Add(choice1);
                        }

                        //DataTable dtChoise2 = mf.DataTable("select * from Choice2 Where ProductId=" + Id);
                        foreach (DataRow choise2 in dtChoise2.Rows)
                        {
                            Choice2 choice2 = new Choice2();
                            choice2.Id = mf.RTI(choise2, "Id");
                            choice2.Name = mf.RTS(choise2, "Name");
                            choice2.Price = mf.RTS(choise2, "Price").Replace(",", ".");
                            choice2.Choice1Id = mf.RTI(choise2, "Choice1Id");
                            choice2.ProductId = mf.RTI(choise2, "ProductId");

                            items.Choice2.Add(choice2);
                        }

                        //DataTable dtOptions = mf.DataTable("select * from Options Where ProductId=" + Id);
                        foreach (DataRow opt in dtOptions.Rows)
                        {
                            Options options = new Options
                            {
                                Id = mf.RTI(opt, "Id"),
                                Name = mf.RTS(opt, "Name"),
                                Price = mf.RTS(opt, "Price").Replace(",", "."),
                                Quantitative = Convert.ToBoolean(mf.RTS(opt, "Quantitative")),
                                Category = mf.RTS(opt, "Category"),
                                ProductId = mf.RTI(opt, "ProductId")
                            };
                            items.Options.Add(options);
                        }

                        //DataTable dtOptionCats = mf.DataTable("select * from OptionCats Where ProductId=" + Id);                       
                        foreach (DataRow optCat in dtOptionCats.Rows)
                        {
                            OptionCats optionCats = new OptionCats
                            {
                                Id = mf.RTI(optCat, "Id"),
                                Name = mf.RTS(optCat, "Name"),
                                MaxSelections = mf.RTS(optCat, "MaxSelections").Replace(",", "."),
                                MinSelections = mf.RTS(optCat, "MinSelections").Replace(",", "."),
                                ProductId = mf.RTI(optCat, "ProductId")
                            };

                            items.OptionCats.Add(optionCats);
                        }

                        //DataTable dtBoms = mf.DataTable("select * from bom where ProductId=" + Id + " and SubeId=" + items.SubeId);
                        foreach (DataRow bm in dtBoms.AsEnumerable().Where(x => x.Field<int>("SubeId") == items.SubeId))
                        {
                            Bom bom = new Bom
                            {
                                Id = mf.RTI(bm, "Id"),
                                ProductName = mf.RTS(bm, "ProductName"),
                                MaterialName = mf.RTS(bm, "MaterialName"),
                                Quantity = mf.RTS(bm, "Quantity").Replace(",", "."),
                                Unit = mf.RTS(bm, "Unit"),
                                StokID = mf.RTI(bm, "StokID"),
                                ProductId = mf.RTI(bm, "ProductId")
                            };
                            if (!string.IsNullOrEmpty(mf.RTS(bm, "YeniUrunMu")))
                                bom.YeniUrunMu = Convert.ToBoolean(mf.RTS(bm, "YeniUrunMu"));
                            items.Boms.Add(bom);
                        }

                        //DataTable dtBomOptions = mf.DataTable("select * from bomOptions where ProductId='" + Id + "' and SubeId=" + items.SubeId);
                        foreach (DataRow bmOp in dtBomOptions.AsEnumerable().Where(x => x.Field<int>("SubeId") == items.SubeId))
                        {
                            BomOptions bomOps = new BomOptions
                            {
                                Id = mf.RTI(bmOp, "Id"),
                                ProductName = mf.RTS(bmOp, "ProductName"),
                                MaterialName = mf.RTS(bmOp, "MaterialName"),
                                Quantity = mf.RTS(bmOp, "Quantity").Replace(",", "."),
                                Unit = mf.RTS(bmOp, "Unit"),
                                StokID = mf.RTI(bmOp, "StokID"),
                                OptionsId = mf.RTI(bmOp, "OptionsId"),
                                OptionsName = mf.RTS(bmOp, "OptionsName"),
                                SubeId = mf.RTI(bmOp, "SubeId"),
                                ProductId = mf.RTI(bmOp, "ProductId")
                            };
                            if (!string.IsNullOrEmpty(mf.RTS(bmOp, "YeniUrunMu")))
                                bomOps.YeniUrunMu = Convert.ToBoolean(mf.RTS(bmOp, "YeniUrunMu"));
                            items.BomOptionss.Add(bomOps);
                        }
                    }

                    mf.SqlConnClose();
                }
            }
            catch (Exception ex)
            {
                Singleton.WritingLogFile2("SefimPanelUrunEkleCRUDGetProduct2", ex.Message.ToString(), "", ex.StackTrace);
            }

            return items;
        }


        #region  **********Localden Get Bom ve BomOptions*******
        public static List<Bom> GetBoms(long Id, int SubeId)
        {
            ModelFunctions f = new ModelFunctions();
            List<Bom> bomLst = new List<Bom>();
            f.SqlConnOpen();
            DataTable dtBoms = f.DataTable("select * from bom where ProductId=" + Id + " and SubeId=" + SubeId);
            foreach (DataRow bm in dtBoms.Rows)
            {
                Bom bom = new Bom();
                bom.Id = f.RTI(bm, "Id");
                bom.ProductName = f.RTS(bm, "ProductName");
                bom.MaterialName = f.RTS(bm, "MaterialName");
                bom.MalinCinsi = f.RTS(bm, "MalinCinsi");
                bom.Quantity = f.RTS(bm, "Quantity").Replace(",", ".");
                bom.Unit = f.RTS(bm, "Unit");
                bom.StokID = f.RTI(bm, "StokID");
                bom.ProductId = f.RTI(bm, "ProductId");
                if (!string.IsNullOrEmpty(f.RTS(bm, "YeniUrunMu")))
                    bom.YeniUrunMu = Convert.ToBoolean(f.RTS(bm, "YeniUrunMu"));
                bomLst.Add(bom);
            }
            f.SqlConnClose();
            return bomLst;
        }

        public static List<Bom> GetBoms2(List<DataRow> dtBoms)
        {
            ModelFunctions f = new ModelFunctions();
            List<Bom> bomLst = new List<Bom>();
            foreach (DataRow bm in dtBoms)
            {
                Bom bom = new Bom
                {
                    Id = f.RTI(bm, "Id"),
                    ProductName = f.RTS(bm, "ProductName"),
                    MaterialName = f.RTS(bm, "MaterialName"),
                    MalinCinsi = f.RTS(bm, "MalinCinsi"),
                    Quantity = f.RTS(bm, "Quantity").Replace(",", "."),
                    Unit = f.RTS(bm, "Unit"),
                    StokID = f.RTI(bm, "StokID"),
                    ProductId = f.RTI(bm, "ProductId")
                };
                if (!string.IsNullOrEmpty(f.RTS(bm, "YeniUrunMu")))
                    bom.YeniUrunMu = Convert.ToBoolean(f.RTS(bm, "YeniUrunMu"));
                bomLst.Add(bom);
            }

            return bomLst;
        }
        public static List<Bom> GetBoms2Birlesecek(List<int> Ids, int SubeId)
        {

            var locked = new Object();
            ModelFunctions f = new ModelFunctions();
            List<Bom> bomLst = new List<Bom>();
            f.SqlConnOpen();
            DataTable dtBoms = f.DataTable("select * from bom where ProductId in(" + string.Join(",", Ids) + ") and SubeId=" + SubeId);
            f.SqlConnClose();
            //foreach (DataRow bm in dtBoms.Rows)
            //{
            Parallel.ForEach(dtBoms.AsEnumerable(), bm =>
            {
                Bom bom = new Bom
                {
                    Id = f.RTI(bm, "Id"),
                    ProductName = f.RTS(bm, "ProductName"),
                    MaterialName = f.RTS(bm, "MaterialName"),
                    MalinCinsi = f.RTS(bm, "MalinCinsi"),
                    Quantity = f.RTS(bm, "Quantity").Replace(",", "."),
                    Unit = f.RTS(bm, "Unit"),
                    StokID = f.RTI(bm, "StokID"),
                    ProductId = f.RTI(bm, "ProductId")
                };
                if (!string.IsNullOrEmpty(f.RTS(bm, "YeniUrunMu")))
                    bom.YeniUrunMu = Convert.ToBoolean(f.RTS(bm, "YeniUrunMu"));

                lock (locked)
                {
                    bomLst.Add(bom);
                }

            });

            return bomLst;
        }
        public static List<BomOptions> GetBomOptions(long Id, int SubeId)
        {
            ModelFunctions f = new ModelFunctions();
            f.SqlConnOpen();
            List<BomOptions> bomOptionsLst = new List<BomOptions>();
            DataTable dtBomOptions = f.DataTable("select * from bomOptions where ProductId='" + Id + "' and SubeId=" + SubeId);
            foreach (DataRow bmOp in dtBomOptions.Rows)
            {
                BomOptions bomOps = new BomOptions();
                bomOps.Id = f.RTI(bmOp, "Id");
                bomOps.ProductName = f.RTS(bmOp, "ProductName");
                bomOps.MaterialName = f.RTS(bmOp, "MaterialName");
                bomOps.Quantity = f.RTS(bmOp, "Quantity").Replace(",", ".");
                bomOps.Unit = f.RTS(bmOp, "Unit");
                bomOps.StokID = f.RTI(bmOp, "StokID");
                bomOps.OptionsId = f.RTI(bmOp, "OptionsId");
                bomOps.OptionsName = f.RTS(bmOp, "OptionsName");
                bomOps.SubeId = f.RTI(bmOp, "SubeId");
                bomOps.ProductId = f.RTI(bmOp, "ProductId");
                if (!string.IsNullOrEmpty(f.RTS(bmOp, "YeniUrunMu")))
                    bomOps.YeniUrunMu = Convert.ToBoolean(f.RTS(bmOp, "YeniUrunMu"));
                bomOptionsLst.Add(bomOps);
            }
            f.SqlConnClose();
            return bomOptionsLst;
        }
        public static List<BomOptions> GetBomOptions2(List<DataRow> dtBomOptions)
        {
            ModelFunctions f = new ModelFunctions();
            List<BomOptions> bomOptionsLst = new List<BomOptions>();

            foreach (DataRow bmOp in dtBomOptions)
            {
                BomOptions bomOps = new BomOptions
                {
                    Id = f.RTI(bmOp, "Id"),
                    ProductName = f.RTS(bmOp, "ProductName"),
                    MaterialName = f.RTS(bmOp, "MaterialName"),
                    Quantity = f.RTS(bmOp, "Quantity").Replace(",", "."),
                    Unit = f.RTS(bmOp, "Unit"),
                    StokID = f.RTI(bmOp, "StokID"),
                    OptionsId = f.RTI(bmOp, "OptionsId"),
                    OptionsName = f.RTS(bmOp, "OptionsName"),
                    SubeId = f.RTI(bmOp, "SubeId"),
                    ProductId = f.RTI(bmOp, "ProductId")
                };

                if (!string.IsNullOrEmpty(f.RTS(bmOp, "YeniUrunMu")))
                    bomOps.YeniUrunMu = Convert.ToBoolean(f.RTS(bmOp, "YeniUrunMu"));
                bomOptionsLst.Add(bomOps);
            }

            return bomOptionsLst;
        }

        public static List<BomOptions> GetBomOptions2Birlesecek(List<int> Ids, int SubeId)
        {
            var locked = new Object();
            var f = new ModelFunctions();
            f.SqlConnOpen();
            List<BomOptions> bomOptionsLst = new List<BomOptions>();
            DataTable dtBomOptions = f.DataTable("select * from bomOptions where ProductId in(" + string.Join(",", Ids) + ") and SubeId=" + SubeId);
            f.SqlConnClose();

            //foreach (DataRow bmOp in dtBomOptions.Rows)
            //{
            Parallel.ForEach(dtBomOptions.AsEnumerable(), bmOp =>
            {
                BomOptions bomOps = new BomOptions
                {
                    Id = f.RTI(bmOp, "Id"),
                    ProductName = f.RTS(bmOp, "ProductName"),
                    MaterialName = f.RTS(bmOp, "MaterialName"),
                    Quantity = f.RTS(bmOp, "Quantity").Replace(",", "."),
                    Unit = f.RTS(bmOp, "Unit"),
                    StokID = f.RTI(bmOp, "StokID"),
                    OptionsId = f.RTI(bmOp, "OptionsId"),
                    OptionsName = f.RTS(bmOp, "OptionsName"),
                    SubeId = f.RTI(bmOp, "SubeId"),
                    ProductId = f.RTI(bmOp, "ProductId")
                };
                if (!string.IsNullOrEmpty(f.RTS(bmOp, "YeniUrunMu")))
                    bomOps.YeniUrunMu = Convert.ToBoolean(f.RTS(bmOp, "YeniUrunMu"));
                lock (locked)
                {
                    bomOptionsLst.Add(bomOps);

                }

            });

            return bomOptionsLst;
        }
        #endregion //**********Localden Get Bom ve BomOptions*******
        public static List<SelectListItem> GetSubeList(string kullaniciId)
        {
            List<SelectListItem> items = new List<SelectListItem>();
            ModelFunctions f = new ModelFunctions();
            try
            {
                var kullaniciSubeYetkisi = UsersListCRUD.KullaniciSubeYetkiListesi(kullaniciId);

                f.SqlConnOpen();
                DataTable dt = f.DataTable("SELECT [ID],[CreateDate],[CreateDate_Timestamp],[ModifyCounter],[UpdateDate],[UpdateDate_Timestamp],[SubeName],[SubeIP],[SqlName],[SqlPassword],[DBName],[FirmaID],[DonemID],[DepoID],[Status],[AppDbType],[AppDbTypeStatus],[FasterSubeID],[SefimPanelZimmetCagrisi],[BelgeSayimTarihDahil],[ServiceAdress] FROM [SubeSettings]");
                foreach (DataRow r in dt.Rows)
                {
                    if (kullaniciSubeYetkisi != null && kullaniciSubeYetkisi.FR_SubeListesi.Where(x => x.SubeID == Convert.ToInt32(f.RTS(r, "ID"))).Select(x => x.SubeID).Any())
                    {
                        items.Add(new SelectListItem
                        {
                            Text = f.RTS(r, "SubeName").ToString(),
                            Value = f.RTS(r, "ID"),
                        });
                    }
                }
                f.SqlConnClose();
            }
            catch (System.Exception ex) { }
            return items;
        }
        public static List<SelectListItem> GetSubeSendBomList(string kullaniciId)
        {
            List<SelectListItem> items = new List<SelectListItem>();
            ModelFunctions f = new ModelFunctions();
            try
            {
                //var kullaniciSubeYetkisi = UsersListCRUD.KullaniciSubeYetkiListesi(kullaniciId);

                f.SqlConnOpen();
                DataTable dt = f.DataTable("SELECT Distinct (select SubeName from SubeSettings where ID=SubeId) as Sube,Count(*) as Adet from Bom group by SubeId");
                foreach (DataRow r in dt.Rows)
                {
                    //if (kullaniciSubeYetkisi != null && kullaniciSubeYetkisi.FR_SubeListesi.Where(x => x.SubeID == Convert.ToInt32(f.RTS(r, "ID"))).Select(x => x.SubeID).Any())
                    //{
                    items.Add(new SelectListItem
                    {
                        Text = f.RTS(r, "Sube").ToString(),
                        Value = f.RTS(r, "Adet"),
                    });
                    //}
                }
                f.SqlConnClose();
            }
            catch (Exception ex) { }
            return items;
        }
        public static List<SelectListItem> GetProductOptionsList(int Id, int SubeId)
        {
            List<SelectListItem> items = new List<SelectListItem>();
            ModelFunctions f = new ModelFunctions();
            try
            {
                f.SqlConnOpen();
                DataTable dt = f.DataTable(@"SELECT
                                            COALESCE('.'+Choice1.Name,'') + COALESCE('.'+Choice2.Name,'') ProductName,Product.Id as ProductId,
                                            CAST(0.0 as decimal(28,2)) as Quantity,
                                            ISNULL(
                                            (SELECT TOP 1  '1' FROM Bom WHERE Bom.ProductName=
                                            Product.ProductName+COALESCE('.'+Choice1.Name,'') + COALESCE('.'+Choice2.Name,'')),'0') as HasBom,
                                            ISNULL(
                                            (SELECT TOP 1  '1' FROM BomOptions WHERE BomOptions.ProductName=
                                            Product.ProductName+COALESCE('.'+Choice1.Name,'') + COALESCE('.'+Choice2.Name,'')),'0') as HasOptions
                                            FROM Product
                                            LEFT JOIN Choice1 ON
                                            Choice1.ProductId = Product.Id and Choice1.SubeId = " + SubeId + @"
                                            LEFT JOIN Choice2 ON
                                            Choice2.ProductId = Product.Id AND
                                            Choice2.Choice1Id=Choice1.Id and Choice2.SubeId=" + SubeId + @"
                                            WHERE  ProductName NOT LIKE '%\[R]%' ESCAPE '\' and Product.Id=" + Id +
                                            "ORDER BY COALESCE('.'+Choice1.Name,'') + COALESCE('.'+Choice2.Name,'')");
                f.SqlConnClose();

                //foreach (DataRow r in dt.Rows)
                //{

                var locked = new Object();
                Parallel.ForEach(dt.AsEnumerable(), r =>
                {
                    lock (locked)
                    {
                        items.Add(new SelectListItem
                        {
                            Text = f.RTS(r, "ProductName").ToString(),
                            Value = f.RTS(r, "ProductName"),
                        });
                    }
                    //}
                });


                #region SUBSTATION LIST               
                f.SqlConnOpen();
                DataTable dtt = f.DataTable("select * from SubeSettings Where Status=1 and Id=" + SubeId);
                f.SqlConnClose();
                #endregion SUBSTATION LIST

                foreach (DataRow r in dtt.Rows)
                {
                    string SubeAdi = f.RTS(r, "SubeName");
                    string SubeIP = f.RTS(r, "SubeIP");
                    string SqlName = f.RTS(r, "SqlName");
                    string SqlPassword = f.RTS(r, "SqlPassword");
                    string DBName = f.RTS(r, "DBName");
                    DataTable ProductDt = new DataTable();
                    ProductDt = f.GetSubeDataWithQuery(f.NewConnectionString(SubeIP, DBName, SqlName, SqlPassword),
                                                        @"SELECT
                                                        COALESCE('.'+Choice1.Name,'') + COALESCE('.'+Choice2.Name,'') ProductName,Product.Id as ProductId,
                                                        CAST(0.0 as decimal(28,2)) as Quantity,
                                                        ISNULL(
                                                        (SELECT TOP 1  '1' FROM Bom WHERE Bom.ProductName=
                                                        Product.ProductName+COALESCE('.'+Choice1.Name,'') + COALESCE('.'+Choice2.Name,'')),'0') as HasBom,
                                                        ISNULL(
                                                        (SELECT TOP 1  '1' FROM BomOptions WHERE BomOptions.ProductName=
                                                        Product.ProductName+COALESCE('.'+Choice1.Name,'') + COALESCE('.'+Choice2.Name,'')),'0') as HasOptions
                                                        FROM Product
                                                        LEFT JOIN Choice1 ON
                                                        Choice1.ProductId = Product.Id
                                                        LEFT JOIN Choice2 ON
                                                        Choice2.ProductId = Product.Id AND
                                                        Choice2.Choice1Id=Choice1.Id
                                                        WHERE  ProductName NOT LIKE '%\[R]%' ESCAPE '\' and Product.Id=" + Id +
                                                        "ORDER BY COALESCE('.'+Choice1.Name,'') + COALESCE('.'+Choice2.Name,'')");

                    //foreach (DataRow prdct in ProductDt.Rows)
                    //{
                    Parallel.ForEach(ProductDt.AsEnumerable(), prdct =>
                    {
                        lock (locked)
                        {
                            items.Add(new SelectListItem
                            {
                                Text = f.RTS(prdct, "ProductName").ToString(),
                                Value = f.RTS(prdct, "ProductName"),
                            });
                        }
                        //}
                    });
                }
            }
            catch (Exception ex) { }
            return items;
        }

        public static List<SelectListItem> GetSubeProductGroup()
        {

            List<SelectListItem> items = new List<SelectListItem>();
            try
            {
                foreach (SelectListItem r in BussinessHelper.ProductGroups)
                {
                    items.Add(r);
                }

            }
            catch (System.Exception ex) { }
            return items;
        }
        public static List<SelectListItem> GetSubeFavoritess()
        {

            List<SelectListItem> items = new List<SelectListItem>();
            try
            {
                foreach (SelectListItem r in BussinessHelper.Favoritess)
                {
                    items.Add(r);
                }

                ////foreach (SelectListItem r in BussinessHelper.Favoritess)
                ////{
                //    items.Add(new SelectListItem
                //    {
                //        Text = "Normal",
                //        Value = "Normal",
                //    });
                //    items.Add(new SelectListItem
                //    {
                //        Text = "Standart",
                //        Value = "Standart",
                //    });
                //    items.Add(new SelectListItem
                //    {
                //        Text = "Joker",
                //        Value = "Joker",
                //    });
                //    items.Add(new SelectListItem
                //    {
                //        Text = "OransalKuver",
                //        Value = "OransalKuver",
                //    });
                //    items.Add(new SelectListItem
                //    {
                //        Text = "Kuver",
                //        Value = "Kuver",
                //    });
                ////}

            }
            catch (System.Exception ex) { }
            return items;
        }
        public static List<SelectListItem> GetSubeInvoiceNames()
        {

            List<SelectListItem> items = new List<SelectListItem>();
            try
            {
                foreach (SelectListItem r in BussinessHelper.InvoiceNames)
                {
                    items.Add(r);
                }

            }
            catch (System.Exception ex) { }
            return items;
        }
        public static List<SelectListItem> GetSubeProductTypes()
        {

            List<SelectListItem> items = new List<SelectListItem>();
            try
            {
                foreach (SelectListItem r in BussinessHelper.ProductTypes)
                {
                    items.Add(r);
                }

            }
            catch (System.Exception ex) { }
            return items;
        }

        //local deki pruduct tablosunu hedef şubelere insert yapar,
        public ActionResultMessages IsInsertProduct()
        {
            var result = new ActionResultMessages() { IsSuccess = true, UserMessage = "İşlem Başarılı", };

            try
            {
                #region Local pruduct tablosundaki şubeler alınır 

                var locked = new Object();
                var sqlDatas = new SqlData(new SqlConnection(mF.NewConnectionString(subeIp, dbName, sqlKullaniciName, sqlKullaniciPassword)));
                var insertSProductSubeList = mF.GetSubeDataWithQuery(mF.NewConnectionString(subeIp, dbName, sqlKullaniciName, sqlKullaniciPassword), SqlData.getProductJoinSubesettingsUrunEkleSqlQuery());

                foreach (DataRow itemSube in insertSProductSubeList.Rows)
                {
                    var insertSubeId = mF.RTS(itemSube, "SubeId");
                    var insertSubeIp = mF.RTS(itemSube, "SubeIp");
                    var insertDbName = mF.RTS(itemSube, "DBName");
                    var insertSqlKullaniciName = mF.RTS(itemSube, "SqlName");
                    var insertSqlKullaniciPassword = mF.RTS(itemSube, "SqlPassword");
                    //var insertsubeName = mF.RTS(itemSube, "SubeName");

                    var sqlDataInsert = new SqlData(new SqlConnection(mF.NewConnectionString(insertSubeIp, insertDbName, insertSqlKullaniciName, insertSqlKullaniciPassword)));

                    var tempProductList = mF.GetSubeDataWithQuery(mF.NewConnectionString(subeIp, dbName, sqlKullaniciName, sqlKullaniciPassword),
                                       SqlData.insertProductSqlQuery(insertSubeId, insertDbName));

                    DataTable tempChoice1List = new DataTable();
                    DataTable tempChoice2List = new DataTable();
                    if (tempProductList != null && tempProductList.AsEnumerable().ToList().Count > 0)
                    {

                        tempChoice1List = mF.GetSubeDataWithQuery(mF.NewConnectionString(subeIp, dbName, sqlKullaniciName, sqlKullaniciPassword),
                                      SqlData.insertChoice1SqlQuery(string.Join(",", tempProductList.AsEnumerable().ToList().Select(x => x.Field<int>("Id"))), insertDbName, insertSubeId));

                        if (tempChoice1List != null && tempChoice1List.AsEnumerable().ToList().Count > 0)
                        {
                            tempChoice2List = mF.GetSubeDataWithQuery(mF.NewConnectionString(subeIp, dbName, sqlKullaniciName, sqlKullaniciPassword),
                                         SqlData.insertChoice2SqlQuery(string.Join(",", tempChoice1List.AsEnumerable().ToList().Select(x => x.Field<int>("Id"))), insertDbName, insertSubeId));
                        }
                    }


                    var tempOptionCatsList = mF.GetSubeDataWithQuery(mF.NewConnectionString(subeIp, dbName, sqlKullaniciName, sqlKullaniciPassword),
                                       SqlData.insertOptionCatsSqlQuery(string.Join(",", tempProductList.AsEnumerable().ToList().Select(x => x.Field<int>("Id"))), insertDbName, insertSubeId));

                    DataTable tempOptionsForCategoryList = new DataTable();
                    DataTable tempBomOptionsList = new DataTable();
                    if (tempOptionCatsList != null && tempOptionCatsList.AsEnumerable().ToList().Count() > 0)
                    {
                        tempOptionsForCategoryList = mF.GetSubeDataWithQuery(mF.NewConnectionString(subeIp, dbName, sqlKullaniciName, sqlKullaniciPassword),
                                       SqlData.insertOptionsForCategorySqlQuery(string.Join(",", tempProductList.AsEnumerable().ToList().Select(x => x.Field<int>("Id"))),
                                       string.Join(",", tempOptionCatsList.AsEnumerable().ToList().Select(x => x.Field<int>("Id"))).Replace("'", "''").Replace(",", "','"), insertDbName, insertSubeId));
                        if (tempOptionsForCategoryList != null && tempOptionsForCategoryList.AsEnumerable().ToList().Count() > 0)
                        {
                            tempBomOptionsList = mF.GetSubeDataWithQuery(mF.NewConnectionString(subeIp, dbName, sqlKullaniciName, sqlKullaniciPassword),
                                           SqlData.insertBomOptionsSqlQuery(string.Join(",", tempOptionsForCategoryList.AsEnumerable().ToList().Select(x => x.Field<int>("Id"))),
                                          string.Join(",", tempProductList.AsEnumerable().ToList().Select(x => x.Field<string>("ProductName"))).Replace("'", "''").Replace(",", "','"), insertDbName, insertSubeId));
                        }
                    }

                    var tempOptionsList = mF.GetSubeDataWithQuery(mF.NewConnectionString(subeIp, dbName, sqlKullaniciName, sqlKullaniciPassword),
                                       SqlData.insertOptionsSqlQuery2(string.Join(",", tempProductList.AsEnumerable().ToList().Select(x => x.Field<int>("Id"))), insertDbName, insertSubeId));

                    DataTable tempOptionsBomOptionsList = new DataTable();
                    if (tempOptionsList != null && tempOptionsList.AsEnumerable().ToList().Count() > 0)
                    {
                        tempOptionsBomOptionsList = mF.GetSubeDataWithQuery(mF.NewConnectionString(subeIp, dbName, sqlKullaniciName, sqlKullaniciPassword),
                                       SqlData.insertBomOptionsSqlQuery(string.Join(",", tempOptionsList.AsEnumerable().ToList().Select(x => x.Field<int>("Id"))),
                                     string.Join(",", tempProductList.AsEnumerable().ToList().Select(x => x.Field<string>("ProductName"))).Replace("'", "''").Replace(",", "','"), insertDbName, insertSubeId));
                    }

                    var tempBomList = mF.GetSubeDataWithQuery(mF.NewConnectionString(subeIp, dbName, sqlKullaniciName, sqlKullaniciPassword),
                                       SqlData.insertBomsSqlQuery(string.Join(",", tempProductList.AsEnumerable().ToList().Select(x => x.Field<int>("Id"))), insertDbName, insertSubeId));


                    var tempCH2 = mF.GetSubeDataWithQuery(mF.NewConnectionString(subeIp, dbName, sqlKullaniciName, sqlKullaniciPassword), "Select * from [dbo].[Choice2] where SubeId=" + insertSubeId).AsEnumerable();

                    foreach (DataRow itemPruduct in tempProductList.Rows)
                    {
                        sqlDataInsert.ExecuteScalarSql("delete from bom where ProductName='" + itemPruduct["ProductName"].ToString() + "' select 1");
                        sqlDataInsert.ExecuteScalarSql("delete from BomOptions where ProductName='" + itemPruduct["ProductName"].ToString() + "'" + " select 1");
                        /*28.07.23 de eklendi kopyalama işlemi sonrası şubelere göndermek için.*/
                        //var xx = sqlDataInsert.ExecuteScalarSql("delete from Product where ProductName='" + itemPruduct["ProductName"].ToString() + "' and ProductGroup='" + itemPruduct["ProductGroup"].ToString() + "'" + " select 1");

                        var insertUpdateProductId = sqlDataInsert.GetSqlValue("Select Id from Product where ProductName='" + itemPruduct["ProductName"].ToString() + "' and ProductGroup='" + itemPruduct["ProductGroup"].ToString() + "'" + " select 1");

                        string CmdString = "declare @ProductCount int=0" +
                                           " set @ProductCount= (select count(*) from[dbo].[Product] where[ProductName] = @par1 and [ProductGroup]=@par2)" +
                                           " if(@ProductCount=0)" +
                                           " Begin " +
                                           " INSERT INTO [dbo].[Product]([ProductName],[ProductGroup],[ProductCode],[Order],[Price],[VatRate],[FreeItem],[InvoiceName],[ProductType],[Plu],[SkipOptionSelection],[Favorites],[IsSynced]) VALUES" +
                                           " (@par1, @par2, @par3, @par4, @par5, @par6, @par7, @par8, @par9, @par10, @par11, @par12,@par13) " +
                                           " select CAST(scope_identity() AS int)" +
                                           " End " +
                                           " else " +
                                           " Begin " +
                                           " select -1 --INSERT INTO [dbo].[Product]([ProductName],[ProductGroup],[ProductCode],[Order],[Price],[VatRate],[FreeItem],[InvoiceName],[ProductType],[Plu],[SkipOptionSelection],[Favorites],[IsSynced]) VALUES" +
                                           " --(@par1+'_kopya'+(select convert(nvarchar(20),(Select Cast(Datediff(minute, '19700101', Cast(GETDATE() As date)) As bigint) * 60000 + Datediff(ms, '19000101', Cast(GETDATE() As time))))), @par2, @par3, @par4, @par5, @par6, @par7, @par8, @par9, @par10, @par11, @par12, @par13) " + Environment.NewLine +
                                           " End " +
                                           "";

                        int productId = sqlDataInsert.ExecuteScalarSql(CmdString, new object[] {
                                                itemPruduct["ProductName"],
                                                itemPruduct["ProductGroup"],
                                                itemPruduct["ProductCode"],
                                                itemPruduct["Order"],
                                                itemPruduct["Price"],
                                                itemPruduct["VatRate"],
                                                itemPruduct["FreeItem"],
                                                itemPruduct["InvoiceName"],
                                                itemPruduct["ProductType"],
                                                itemPruduct["Plu"],
                                                itemPruduct["SkipOptionSelection"],
                                                itemPruduct["Favorites"],
                                               0,
                        });


                        if (productId > -1)
                        {
                            //var tempChoice1List = mF.GetSubeDataWithQuery(mF.NewConnectionString(subeIp, dbName, sqlKullaniciName, sqlKullaniciPassword), SqlData.insertChoice1SqlQuery(itemPruduct["Id"].ToString(), insertDbName, insertSubeId));
                            foreach (DataRow itemChoice1 in tempChoice1List.AsEnumerable().ToList().Where(x => x.Field<int>("ProductId").ToString() == itemPruduct["Id"].ToString()))
                            {
                                string CmdStringChoice1 = "INSERT INTO [dbo].[Choice1]([ProductId],[Name],[Price],[IsSynced]) values (@par1, @par2, @par3, @par4) select CAST(scope_identity() AS int)";
                                int choice1Id = sqlDataInsert.ExecuteScalarSql(CmdStringChoice1, new object[] { productId, itemChoice1["Name"], itemChoice1["Price"], 0 });

                                //var tempChoice2List = mF.GetSubeDataWithQuery(mF.NewConnectionString(subeIp, dbName, sqlKullaniciName, sqlKullaniciPassword), SqlData.insertChoice2SqlQuery(itemChoice1["Id"].ToString(), insertDbName, insertSubeId));
                                foreach (DataRow itemChoice2 in tempChoice2List.AsEnumerable().ToList().Where(x => x.Field<int>("Choice1Id").ToString() == itemChoice1["Id"].ToString()))
                                {
                                    string CmdStringChoice2 = "INSERT INTO [dbo].[Choice2]([ProductId],[Choice1Id],[Name],[Price],[IsSynced]) values (@par1, @par2, @par3, @par4, @par5) select CAST(scope_identity() AS int) ";
                                    int choice2Id = sqlDataInsert.ExecuteScalarSql(CmdStringChoice2, new object[] { productId, choice1Id, itemChoice2["Name"], itemChoice2["Price"], 0 });
                                }
                            }

                            //OptionCats aktarılacak.....
                            //options
                            //var tempOptionCatsList = mF.GetSubeDataWithQuery(mF.NewConnectionString(subeIp, dbName, sqlKullaniciName, sqlKullaniciPassword), SqlData.insertOptionCatsSqlQuery(itemPruduct["Id"].ToString(), insertDbName, insertSubeId));
                            foreach (DataRow itemOptionCats in tempOptionCatsList.AsEnumerable().ToList().Where(x => x.Field<int>("ProductId").ToString() == itemPruduct["Id"].ToString()))
                            {
                                string cmdStringOptionCats = "INSERT INTO [dbo].[OptionCats]([Name],[ProductId],[MaxSelections],[MinSelections]) values (@par1, @par2, @par3, @par4) select CAST(scope_identity() AS int)";
                                int optionCatsId = sqlDataInsert.ExecuteScalarSql(cmdStringOptionCats, new object[] {
                                    itemOptionCats["Name"],
                                    productId,
                                    itemOptionCats["MaxSelections"],
                                    itemOptionCats["MinSelections"] });

                                //var tempOptionsForCategoryList = mF.GetSubeDataWithQuery(mF.NewConnectionString(subeIp, dbName, sqlKullaniciName, sqlKullaniciPassword), SqlData.insertOptionsForCategorySqlQuery(itemPruduct["Id"].ToString(), itemOptionCats["Id"].ToString(), insertDbName, insertSubeId));
                                foreach (DataRow itemOptionsForCategory in tempOptionsForCategoryList.AsEnumerable().ToList().Where(x => x.Field<int>("ProductId").ToString() == itemPruduct["Id"].ToString() && x.Field<string>("Category") == itemOptionCats["Id"].ToString()))
                                {
                                    string cmdStringOptions = "INSERT INTO [dbo].[Options]([Name],[Price],[Quantitative],[ProductId],[Category]) values (@par1, @par2, @par3, @par4, @par5) select CAST(scope_identity() AS int)";
                                    int optionsId = sqlDataInsert.ExecuteScalarSql(cmdStringOptions, new object[] {
                                        itemOptionsForCategory["Name"],
                                        itemOptionsForCategory["Price"],
                                        itemOptionsForCategory["Quantitative"],
                                        productId,
                                        optionCatsId });

                                    //var tempBomOptionsList = mF.GetSubeDataWithQuery(mF.NewConnectionString(subeIp, dbName, sqlKullaniciName, sqlKullaniciPassword), SqlData.insertBomOptionsSqlQuery(itemOptionsForCategory["Id"].ToString(), itemPruduct["ProductName"].ToString(), insertDbName, insertSubeId));
                                    foreach (DataRow itemBomOptions in tempBomOptionsList.AsEnumerable().ToList().Where(x => x.Field<string>("ProductName").ToString() == itemPruduct["ProductName"].ToString() && x.Field<string>("OptionsId").ToString() == itemOptionsForCategory["Id"].ToString()))
                                    {
                                        string cmdStringBoms = "INSERT INTO [dbo].[BomOptions]([OptionsId],[OptionsName],[MaterialName],[Quantity],[Unit],[StokID],[ProductName]) values (@par1, @par2, @par3, @par4, @par5, @par6, @par7) select CAST(scope_identity() AS int)";
                                        int optionBomOptionsId = sqlDataInsert.ExecuteScalarSql(cmdStringBoms, new object[] {
                                            optionsId,
                                            itemOptionsForCategory["Name"],
                                            itemBomOptions["MaterialName"],
                                            itemBomOptions["Quantity"],
                                            itemBomOptions["Unit"],
                                            itemBomOptions["StokID"],
                                            itemPruduct["ProductName"] });
                                    }
                                }
                            }

                            ////var tempOptionsList = mF.GetSubeDataWithQuery(mF.NewConnectionString(subeIp, dbName, sqlKullaniciName, sqlKullaniciPassword), SqlData.insertOptionsSqlQuery(itemPruduct["Id"].ToString(), insertDbName, insertSubeId));// 25.11.2023 eminle hataya istineden 
                            //var tempOptionsList = mF.GetSubeDataWithQuery(mF.NewConnectionString(subeIp, dbName, sqlKullaniciName, sqlKullaniciPassword), SqlData.insertOptionsSqlQuery2(itemPruduct["Id"].ToString(), insertDbName, insertSubeId));
                            foreach (DataRow itemOptions in tempOptionsList.AsEnumerable().ToList().Where(x => x.Field<int>("ProductId").ToString() == itemPruduct["Id"].ToString()))
                            {
                                string cmdStringOptions = "INSERT INTO [dbo].[Options]([Name],[Price],[Quantitative],[ProductId],[Category],[IsSynced]) values (@par1, @par2, @par3, @par4, @par5, @par6) select CAST(scope_identity() AS int)";
                                int optionsId = sqlDataInsert.ExecuteScalarSql(cmdStringOptions, new object[] {
                                    itemOptions["Name"],
                                    itemOptions["Price"],
                                    itemOptions["Quantitative"],
                                    productId,
                                    itemOptions["Category"],
                                    0 });

                                //var tempOptionsBomOptionsList = mF.GetSubeDataWithQuery(mF.NewConnectionString(subeIp, dbName, sqlKullaniciName, sqlKullaniciPassword), SqlData.insertBomOptionsSqlQuery(itemOptions["Id"].ToString(), itemPruduct["ProductName"].ToString(), insertDbName, insertSubeId));
                                foreach (DataRow itemBomOptions in tempOptionsBomOptionsList.AsEnumerable().ToList().Where(x => x.Field<int>("OptionsId").ToString() == itemOptions["Id"].ToString() && x.Field<string>("ProductName").ToString() == itemPruduct["ProductName"].ToString()))
                                {
                                    string cmdStringBoms = "INSERT INTO [dbo].[BomOptions]([OptionsId],[OptionsName],[MaterialName],[Quantity],[Unit],[StokID],[ProductName]) values (@par1, @par2, @par3, @par4, @par5, @par6, @par7) select CAST(scope_identity() AS int)";
                                    int optionCatsId = sqlDataInsert.ExecuteScalarSql(cmdStringBoms, new object[] {
                                        optionsId,
                                        itemOptions["Name"],
                                        itemBomOptions["MaterialName"],
                                        itemBomOptions["Quantity"],
                                        itemBomOptions["Unit"],
                                        itemBomOptions["StokID"],
                                        itemPruduct["ProductName"] });
                                }
                            }

                            //var tempBomList = mF.GetSubeDataWithQuery(mF.NewConnectionString(subeIp, dbName, sqlKullaniciName, sqlKullaniciPassword), SqlData.insertBomsSqlQuery(itemPruduct["Id"].ToString(), insertDbName, insertSubeId));
                            foreach (DataRow itemBoms in tempBomList.AsEnumerable().ToList().Where(x => x.Field<int>("ProductId").ToString() == itemPruduct["Id"].ToString()))
                            {
                                string cmdStringBoms = "INSERT INTO [dbo].[Bom]([ProductName],[MaterialName],[Quantity],[Unit],[StokID],[ProductId]) values (@par1, @par2, @par3, @par4, @par5, @par6) select CAST(scope_identity() AS int)";
                                int optionCatsId = sqlDataInsert.ExecuteScalarSql(cmdStringBoms, new object[] {
                                    itemBoms["ProductName"],
                                    itemBoms["MaterialName"],
                                    itemBoms["Quantity"],
                                    itemBoms["Unit"],
                                    itemBoms["StokID"],
                                    productId });
                            }

                            //var sqlDatas = new SqlData(new SqlConnection(mF.NewConnectionString(subeIp, dbName, sqlKullaniciName, sqlKullaniciPassword)));
                            sqlDatas.ExecuteSql("Delete Bom where ProductId=" + itemPruduct["Id"].ToString(), new object[] { });
                            sqlDatas.ExecuteSql("Delete BomOptions where ProductName='" + itemPruduct["ProductName"].ToString() + "'", new object[] { });
                        }
                        //Kopyalama için eklendi.<30.07.2023>
                        else
                        {
                            string updateProduct = " Update  [dbo].[Product] set " +
                                                    " [ProductName]=@par1," +
                                                    "[ProductGroup]=@par2," +
                                                    "[ProductCode]=@par3," +
                                                    "[Order]=@par4," +
                                                    "[Price]=@par5," +
                                                    "[VatRate]=@par6," +
                                                    "[FreeItem]=@par7," +
                                                    "[InvoiceName]=@par8," +
                                                    "[ProductType]=@par9," +
                                                    "[Plu]=@par10," +
                                                    "[SkipOptionSelection]=@par11," +
                                                    "[Favorites]=@par12, " +
                                                    "[IsUpdated]=@par13 " +
                                                    "where Id=@par14 ";

                            int updateProductId = sqlDataInsert.ExecuteSql(updateProduct, new object[] {
                                                    itemPruduct["ProductName"],
                                                    itemPruduct["ProductGroup"],
                                                    itemPruduct["ProductCode"],
                                                    itemPruduct["Order"],
                                                    itemPruduct["Price"],
                                                    itemPruduct["VatRate"],
                                                    itemPruduct["FreeItem"],
                                                    itemPruduct["InvoiceName"],
                                                    itemPruduct["ProductType"],
                                                    itemPruduct["Plu"],
                                                    itemPruduct["SkipOptionSelection"],
                                                    itemPruduct["Favorites"],
                                                    1,
                                                    insertUpdateProductId, });

                            //var tempChoice1List = mF.GetSubeDataWithQuery(mF.NewConnectionString(subeIp, dbName, sqlKullaniciName, sqlKullaniciPassword), SqlData.insertChoice1SqlQuery(itemPruduct["Id"].ToString(), insertDbName, insertSubeId));
                            //DataTable tempChoice2List = new DataTable();
                            foreach (DataRow itemChoice1 in tempChoice1List.AsEnumerable().ToList().Where(x => x.Field<int>("ProductId").ToString() == itemPruduct["Id"].ToString()))
                            {
                                var ch1VarMi1 = sqlDataInsert.GetSqlValue("Select * from [dbo].[Choice1] where  [Name]='" + itemChoice1["Name"].ToString() + "' and ProductId=" + insertUpdateProductId + " select 1");
                                //Insert
                                if (ch1VarMi1 == null)
                                {
                                    string CmdStringChoice1 = "INSERT INTO [dbo].[Choice1]([ProductId],[Name],[Price],[IsSynced]) values (@par1, @par2, @par3, @par4) select CAST(scope_identity() AS int)";
                                    int choice1Id = sqlDataInsert.ExecuteScalarSql(CmdStringChoice1, new object[] { insertUpdateProductId, itemChoice1["Name"], itemChoice1["Price"], 0 });

                                    //tempChoice2List = mF.GetSubeDataWithQuery(mF.NewConnectionString(subeIp, dbName, sqlKullaniciName, sqlKullaniciPassword), SqlData.insertChoice2SqlQuery(itemChoice1["Id"].ToString(), insertDbName, insertSubeId));
                                    foreach (DataRow itemChoice2 in tempChoice2List.AsEnumerable().ToList().Where(x => x.Field<int>("Choice1Id").ToString() == itemChoice1["Id"].ToString()))
                                    {
                                        string CmdStringChoice2 = "INSERT INTO [dbo].[Choice2]([ProductId],[Choice1Id],[Name],[Price],[IsSynced]) values (@par1, @par2, @par3, @par4, @par5) select CAST(scope_identity() AS int) ";
                                        int choice2Id = sqlDataInsert.ExecuteScalarSql(CmdStringChoice2, new object[] { insertUpdateProductId, choice1Id, itemChoice2["Name"], itemChoice2["Price"], 0 });
                                    }
                                }
                                else//Update
                                {
                                    string CmdStringChoice1 = "Update [dbo].[Choice1] set [Name]=@par1,[Price]=@par2 ,[IsUpdated]=@par3 where ProductId=@par4 and Name=@par5 ";
                                    int choice1Id = sqlDataInsert.ExecuteSql(CmdStringChoice1, new object[] { itemChoice1["Name"], itemChoice1["Price"], 1, insertUpdateProductId, itemChoice1["Name"] });

                                    //tempChoice2List = mF.GetSubeDataWithQuery(mF.NewConnectionString(subeIp, dbName, sqlKullaniciName, sqlKullaniciPassword), SqlData.insertChoice2SqlQuery(itemChoice1["Id"].ToString(), insertDbName, insertSubeId));
                                    foreach (DataRow itemChoice2 in tempChoice2List.AsEnumerable().ToList().Where(x => x.Field<int>("Choice1Id").ToString() == itemChoice1["Id"].ToString()))
                                    {
                                        var ch2VarMi = sqlDataInsert.GetSqlValue("Select * from [dbo].[Choice2] where  Choice1Id=" + ch1VarMi1 + " and ProductId=" + insertUpdateProductId + " and Name='" + itemChoice2[3] + "' select 1");
                                        if (ch1VarMi1 != null && ch2VarMi == null)
                                        {
                                            string insertChoice2 = "INSERT INTO [dbo].[Choice2]([ProductId],[Choice1Id],[Name],[Price],[IsSynced]) values (@par1, @par2, @par3, @par4, @par5) select CAST(scope_identity() AS int) ";
                                            int choice2Id = sqlDataInsert.ExecuteScalarSql(insertChoice2, new object[] { insertUpdateProductId, ch1VarMi1, itemChoice2["Name"], itemChoice2["Price"], 0 });
                                        }
                                        else
                                        {
                                            string CmdStringChoice2 = "update [dbo].[Choice2] set [Name]=@par1,[Price]=@par2,[IsUpdated]=@par3 where ProductId=@par4 and Choice1Id=@par5  and Id= " + ch2VarMi;
                                            int choice2Id = sqlDataInsert.ExecuteSql(CmdStringChoice2, new object[] { itemChoice2["Name"], itemChoice2["Price"], 1, insertUpdateProductId, ch1VarMi1, });
                                        }
                                    }
                                }
                            }

                            //Options
                            ////var tempOptionsList = mF.GetSubeDataWithQuery(mF.NewConnectionString(subeIp, dbName, sqlKullaniciName, sqlKullaniciPassword), SqlData.insertOptionsSqlQuery(itemPruduct["Id"].ToString(), insertDbName, insertSubeId));//25.11.23 yorumsatırını alındı
                            //var tempOptionsList = mF.GetSubeDataWithQuery(mF.NewConnectionString(subeIp, dbName, sqlKullaniciName, sqlKullaniciPassword), SqlData.insertOptionsSqlQuery2(itemPruduct["Id"].ToString(), insertDbName, insertSubeId));
                            foreach (DataRow itemOptions in tempOptionsList.AsEnumerable().ToList().Where(x => x.Field<int>("ProductId").ToString() == itemPruduct["Id"].ToString()))
                            {
                                var optionsVarMi = sqlDataInsert.GetSqlValue("Select * from [dbo].[Options] where  [Name]='" + itemOptions["Name"].ToString() + "' and ProductId=" + insertUpdateProductId + " select 1");

                                if (optionsVarMi == null)
                                {
                                    string cmdStringOptions = "INSERT INTO [dbo].[Options]([Name],[Price],[Quantitative],[ProductId],[Category],[IsSynced]) values (@par1, @par2, @par3, @par4, @par5,@par6) select CAST(scope_identity() AS int)";
                                    int optionsId = sqlDataInsert.ExecuteScalarSql(cmdStringOptions, new object[] {
                                        itemOptions["Name"],
                                        itemOptions["Price"],
                                        itemOptions["Quantitative"],
                                        insertUpdateProductId,
                                        itemOptions["Category"],
                                        0 });
                                }
                                else
                                {
                                    string cmdStringOptions = "Update [dbo].[Options] Set [Name]=@par1,[Price]=@par2,[Quantitative]=@par3,[ProductId]=@par4,[Category]=@par5, [IsUpdated]=@par6 where ProductId=@par7 and Name=@par8 ";
                                    int optionsId = sqlDataInsert.ExecuteSql(cmdStringOptions, new object[] {
                                        itemOptions["Name"],
                                        itemOptions["Price"],
                                        itemOptions["Quantitative"],
                                        insertUpdateProductId,
                                        itemOptions["Category"] ,
                                        1,
                                        insertUpdateProductId,
                                         itemOptions["Name"]
                                        });
                                }
                            }

                            //Delete Choice1
                            //var subeChoice1List = mF.GetSubeDataWithQuery(mF.NewConnectionString(insertSubeIp, insertDbName, insertSqlKullaniciName, insertSqlKullaniciPassword),
                            //      "Select * from [dbo].[Choice1] where  ProductId=" + insertUpdateProductId + " select 1").AsEnumerable();
                            foreach (DataRow subeChoice1 in tempChoice1List.AsEnumerable().ToList().Where(x => x.Field<int>("ProductId").ToString() == insertUpdateProductId.ToString()).ToList())
                            {
                                //foreach (DataRow tempChoice1 in tempChoice1List.Rows)
                                //{

                                var name = tempChoice1List.AsEnumerable().ToList().Select(x => x.ItemArray[2]);
                                var subeChoice1Id = mF.RTI(subeChoice1, "Id");
                                if (!name.Contains(subeChoice1["Name"]))
                                {
                                    _ = sqlDataInsert.ExecuteSql("delete from Choice1  where ProductId=" + insertUpdateProductId + " and Name='" + subeChoice1["Name"] + "' ");
                                    _ = sqlDataInsert.ExecuteSql("delete from Choice2  where ProductId=" + insertUpdateProductId + " and Choice1Id=" + subeChoice1["Id"] + "  select 1");
                                }

                                //Delete Choice2
                                //var ch1Id = subeChoice1["Id"];
                                //var subeChoice2List = mF.GetSubeDataWithQuery(mF.NewConnectionString(insertSubeIp, insertDbName, insertSqlKullaniciName, insertSqlKullaniciPassword),
                                //                           "Select * from [dbo].[Choice2] where  ProductId=" + insertUpdateProductId + " and [Choice1Id]=" + subeChoice1["Id"]).AsEnumerable();
                                //var tempCH2 = mF.GetSubeDataWithQuery(mF.NewConnectionString(subeIp, dbName, sqlKullaniciName, sqlKullaniciPassword),
                                //            "Select * from [dbo].[Choice2] where SubeId=" + insertSubeId).AsEnumerable();

                                foreach (DataRow subeChoice2 in tempChoice2List.AsEnumerable().ToList().Where(x => x.Field<int>("Choice1Id").ToString() == subeChoice1["Id"].ToString() && x.Field<int>("ProductId").ToString() == insertUpdateProductId.ToString()))
                                {
                                    var ch1ID = mF.RTI(subeChoice2, "Choice1Id");
                                    var name1 = tempCH2.AsEnumerable().ToList().Select(x => x.ItemArray[3]);
                                    //var n = mF.RTS(subeChoice2, "Name");
                                    //var pID = mF.RTI(subeChoice2, "ProductId");
                                    //var price = mF.RTD(subeChoice2, "Price");
                                    //var pId1 = tempCH2.AsEnumerable().ToList().Select(x => x.ItemArray[1]);
                                    //var vv1 = name1.Contains(subeChoice2["Name"]);
                                    //var zz1 = pId1.Contains(insertUpdateProductId);
                                    //var name11 = tempCH2.AsEnumerable().ToList().Select(x => x.ItemArray[3]).FirstOrDefault();

                                    if (!name1.Contains(subeChoice2["Name"]) && subeChoice1Id == ch1ID)
                                    {
                                        sqlDataInsert.ExecuteSql("delete from Choice2  where ProductId=" + insertUpdateProductId + " and Choice1Id=" + subeChoice1["Id"] + " and Id=" + subeChoice2["Id"]);
                                    }
                                }
                            }

                            //Delete Options
                            //var subeOptionsList = mF.GetSubeDataWithQuery(mF.NewConnectionString(insertSubeIp, insertDbName, insertSqlKullaniciName, insertSqlKullaniciPassword),
                            //                     "Select * from [dbo].[Options] where  ProductId=" + insertUpdateProductId + " select 1").AsEnumerable();

                            foreach (DataRow itemOptions in tempOptionsList.AsEnumerable().ToList().Where(x => x.Field<int>("ProductId").ToString() == itemPruduct["Id"].ToString()))
                            {
                                var name1 = tempOptionsList.AsEnumerable().ToList().Select(x => x.ItemArray[1]);
                                if (!name1.Contains(itemOptions["Name"]))
                                {
                                    sqlDataInsert.ExecuteSql("delete from Options  where ProductId=" + insertUpdateProductId + " and Id=" + itemOptions["Id"]);
                                }
                            }
                        }
                    }
                }

                #region Product tablosunda Delete  yapar

                var sqlData = new SqlData(new SqlConnection(mF.NewConnectionString(subeIp, dbName, sqlKullaniciName, sqlKullaniciPassword)));
                sqlData.ExecuteSql("Delete Product where YeniUrunmu=1 ", new object[] { });
                sqlData.ExecuteSql("Delete Choice1 where YeniUrunmu=1 ", new object[] { });
                sqlData.ExecuteSql("Delete Choice2 where YeniUrunmu=1 ", new object[] { });
                sqlData.ExecuteSql("Delete Options where YeniUrunmu=1 ", new object[] { });
                sqlData.ExecuteSql("Delete OptionCats where YeniUrunmu=1 ", new object[] { });

                #endregion Product tablosunda Delete yapar

                //bom ve bomoptions içinde direk şube içindeki producta eklenenler
                try
                {
                    var insertSBomAndBomOptionSubeList = mF.GetSubeDataWithQuery(mF.NewConnectionString(subeIp, dbName, sqlKullaniciName, sqlKullaniciPassword),
                        SqlData.getBomAndBomOptionJoinSubesettingsUrunEkleSqlQuery());

                    DataTable tempBomProductList = new DataTable();
                    DataTable tempBomListBomAndBomOptionSube = new DataTable();
                    DataTable tempBomOptionProductList = new DataTable();
                    DataTable tempBomOptionList = new DataTable();

                    if (insertSBomAndBomOptionSubeList != null && insertSBomAndBomOptionSubeList.AsEnumerable().ToList().Count > 0)
                    {
                        tempBomProductList = mF.GetSubeDataWithQuery(mF.NewConnectionString(subeIp, dbName, sqlKullaniciName, sqlKullaniciPassword),
                            SqlData.insertBomsNotProductProductIdSqlQuery(string.Join(",", insertSBomAndBomOptionSubeList.AsEnumerable().ToList().Select(x => x.Field<int>("SubeId"))), ""));
                        tempBomListBomAndBomOptionSube = mF.GetSubeDataWithQuery(mF.NewConnectionString(subeIp, dbName, sqlKullaniciName, sqlKullaniciPassword),
                           SqlData.insertBomsNotProductSqlQuery(string.Join(",", insertSBomAndBomOptionSubeList.AsEnumerable().ToList().Select(x => x.Field<int>("SubeId"))), ""));
                        tempBomOptionProductList = mF.GetSubeDataWithQuery(mF.NewConnectionString(subeIp, dbName, sqlKullaniciName, sqlKullaniciPassword),
                           SqlData.insertBomOptionsNotProductProductIdSqlQuery(string.Join(",", insertSBomAndBomOptionSubeList.AsEnumerable().ToList().Select(x => x.Field<int>("SubeId"))), ""));
                        tempBomOptionList = mF.GetSubeDataWithQuery(mF.NewConnectionString(subeIp, dbName, sqlKullaniciName, sqlKullaniciPassword),
                           SqlData.insertBomOptionsNotProductSqlQuery(string.Join(",", insertSBomAndBomOptionSubeList.AsEnumerable().ToList().Select(x => x.Field<int>("SubeId"))), ""));
                    }

                    foreach (DataRow itemSube in insertSBomAndBomOptionSubeList.Rows)
                    {
                        var insertSubeId = mF.RTS(itemSube, "SubeId");
                        var insertSubeIp = mF.RTS(itemSube, "SubeIp");
                        var insertDbName = mF.RTS(itemSube, "DBName");
                        var insertSqlKullaniciName = mF.RTS(itemSube, "SqlName");
                        var insertSqlKullaniciPassword = mF.RTS(itemSube, "SqlPassword");
                        var sqlDataInsert = new SqlData(new SqlConnection(mF.NewConnectionString(insertSubeIp, insertDbName, insertSqlKullaniciName, insertSqlKullaniciPassword)));
                        var sqlDatasBomAndBomOptionSubeList = new SqlData(new SqlConnection(mF.NewConnectionString(insertSubeIp, insertDbName, insertSqlKullaniciName, insertSqlKullaniciPassword)));

                        //var tempBomProductList = mF.GetSubeDataWithQuery(mF.NewConnectionString(subeIp, dbName, sqlKullaniciName, sqlKullaniciPassword), SqlData.insertBomsNotProductProductIdSqlQuery(insertSubeId, insertDbName));
                        //var tempBomList = mF.GetSubeDataWithQuery(mF.NewConnectionString(subeIp, dbName, sqlKullaniciName, sqlKullaniciPassword), SqlData.insertBomsNotProductSqlQuery(insertSubeId, insertDbName));
                        //var tempBomOptionProductList = mF.GetSubeDataWithQuery(mF.NewConnectionString(subeIp, dbName, sqlKullaniciName, sqlKullaniciPassword), SqlData.insertBomOptionsNotProductProductIdSqlQuery(insertSubeId, insertDbName));
                        //var tempBomOptionList = mF.GetSubeDataWithQuery(mF.NewConnectionString(subeIp, dbName, sqlKullaniciName, sqlKullaniciPassword), SqlData.insertBomOptionsNotProductSqlQuery(insertSubeId, insertDbName));

                        //foreach (DataRow item in tempBomProductList.Rows)
                        //{


                        Parallel.ForEach(tempBomProductList.AsEnumerable().Where(x => x.Field<int>("SubeId").ToString() == insertSubeId).ToList<DataRow>(), item =>
                        {
                            lock (locked)
                            {
                                sqlDatasBomAndBomOptionSubeList.ExecuteSql("delete bom where ProductName='" + item["ProductName"].ToString() + "'", new object[] { });
                            }
                        });
                        //}

                        Parallel.ForEach(tempBomListBomAndBomOptionSube.AsEnumerable().Where(x => x.Field<int>("SubeId").ToString() == insertSubeId).ToList<DataRow>(), itemBoms =>
                        {
                            lock (locked)
                            {
                                string cmdStringBoms = "INSERT INTO [dbo].[Bom]([ProductName],[MaterialName],[Quantity],[Unit],[StokID],[ProductId]) values (@par1, @par2, @par3, @par4, @par5, @par6) select CAST(scope_identity() AS int)";
                                int optionCatsId = sqlDataInsert.ExecuteScalarSql(cmdStringBoms, new object[] {
                            itemBoms["ProductName"],
                            itemBoms["MaterialName"],
                            itemBoms["Quantity"],
                            itemBoms["Unit"],
                            itemBoms["StokID"],
                            itemBoms["ProductId"]  });
                            }
                        });

                        //var tempBomOptionProductList = mF.GetSubeDataWithQuery(mF.NewConnectionString(subeIp, dbName, sqlKullaniciName, sqlKullaniciPassword), SqlData.insertBomOptionsNotProductProductIdSqlQuery(insertSubeId, insertDbName));
                        //var tempBomOptionList = mF.GetSubeDataWithQuery(mF.NewConnectionString(subeIp, dbName, sqlKullaniciName, sqlKullaniciPassword), SqlData.insertBomOptionsNotProductSqlQuery(insertSubeId, insertDbName));
                        //foreach (DataRow item in tempBomOptionProductList.Rows)
                        //{
                        Parallel.ForEach(tempBomOptionProductList.AsEnumerable().Where(x => x.Field<int>("SubeId").ToString() == insertSubeId).ToList<DataRow>(), item =>
                        {
                            lock (locked)
                            {
                                //var sqlDatas = new SqlData(new SqlConnection(mF.NewConnectionString(insertSubeIp, insertDbName, insertSqlKullaniciName, insertSqlKullaniciPassword)));
                                sqlDatasBomAndBomOptionSubeList.ExecuteSql("delete BomOptions where ProductName='" + item["ProductName"].ToString() + "'", new object[] { });
                            }
                        });
                        //}

                        //foreach (DataRow itemBomOptions in tempBomOptionList.Rows)
                        //{
                        Parallel.ForEach(tempBomOptionList.AsEnumerable().Where(x => x.Field<int>("SubeId").ToString() == insertSubeId).ToList<DataRow>(), itemBomOptions =>
                        {
                            lock (locked)
                            {
                                string cmdStringBoms = "INSERT INTO [dbo].[BomOptions]([OptionsId],[OptionsName],[MaterialName],[Quantity],[Unit],[StokID],[ProductName]) values (@par1, @par2, @par3, @par4, @par5, @par6, @par7) select CAST(scope_identity() AS int)";
                                int optionCatsId = sqlDataInsert.ExecuteScalarSql(cmdStringBoms, new object[] {
                            itemBomOptions["OptionsId"],
                            itemBomOptions["OptionsName"],
                            itemBomOptions["MaterialName"],
                            itemBomOptions["Quantity"],
                            itemBomOptions["Unit"],
                            itemBomOptions["StokID"],
                            itemBomOptions["ProductName"] });
                            }
                        });
                    }

                    //delete bom ve bomoptions 
                    sqlData.ExecuteSql("Delete Bom ", new object[] { });
                    sqlData.ExecuteSql("Delete BomOptions ", new object[] { });
                }
                catch (Exception ex)
                {
                    Singleton.WritingLogFile2("insertSBomAndBomOptionSubeList", ex.ToString(), null, ex.StackTrace);
                }

                #endregion Local pruduct tablosundaki şubeler alınır                
            }
            catch (Exception ex)
            {
                Singleton.WritingLogFile2("SefimPanelUrunEkleCRUD_IsInsertProduct", ex.Message.ToString(), "", ex.StackTrace);
            }
            return result;
        }
    }
}