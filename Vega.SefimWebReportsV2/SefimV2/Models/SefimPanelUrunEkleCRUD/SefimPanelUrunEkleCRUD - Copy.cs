using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Configuration;
using System.Web.Mvc;

namespace SefimV2.Models.ProductSefimCRUD
{
    public class SefimPanelUrunEkleCRUD
    {
        ModelFunctions mF = new ModelFunctions();
        #region Config local copy db connction setting       
        static string subeIp = WebConfigurationManager.AppSettings["Server"];
        static string dbName = WebConfigurationManager.AppSettings["DBName"];
        static string sqlKullaniciName = WebConfigurationManager.AppSettings["User"];
        static string sqlKullaniciPassword = WebConfigurationManager.AppSettings["Password"];
        #endregion
        public ActionResultMessages Insert(SefimPanelUrunEkleViewModel Product)
        {
            ActionResultMessages result = new ActionResultMessages();

            using (SqlConnection con = new SqlConnection(mF.NewConnectionString(subeIp, dbName, sqlKullaniciName, sqlKullaniciPassword)))
            {
                con.Open();
                SqlTransaction transaction = con.BeginTransaction();
                SqlData sqlData = new SqlData(con);

                try
                {

                    //şubelere göre insert yapılacak

                    if (Product.Id > 0)
                    {
                        //Eski datalar silinecek burası transaction olmalı
                        sqlData.executeScalarTransactionSql("delete from Product where Id=" + Product.Id + " select count(*) from Product where Id=" + Product.Id, transaction);
                        sqlData.executeScalarTransactionSql("delete from Choice1 where ProductId=" + Product.Id + " select count(*) from Choice1 where Id=" + Product.Id, transaction);
                        sqlData.executeScalarTransactionSql("delete from Choice2 where ProductId=" + Product.Id + " select count(*) from Choice2 where Id=" + Product.Id, transaction);
                        sqlData.executeScalarTransactionSql("delete from Options where ProductId=" + Product.Id + " select count(*) from Options where Id=" + Product.Id, transaction);
                        sqlData.executeScalarTransactionSql("delete from OptionCats where ProductId=" + Product.Id + " select count(*) from OptionCats where Id=" + Product.Id, transaction);
                    }


                    string CmdString = "INSERT INTO [dbo].[Product]([ProductName],[ProductGroup],[ProductCode],[Order],[Price],[VatRate],[FreeItem],[InvoiceName],[ProductType],[Plu],[SkipOptionSelection],[Favorites],[SubeId],[SubeName],[YeniUrunMu],[ProductPkId]) VALUES" +
                "(@par1, @par2, @par3, @par4, @par5, @par6, @par7, @par8, @par9, @par10, @par11, @par12, @par13, @par14, @par15,@par16)" +
                "select CAST(scope_identity() AS int);";
                    int ID = sqlData.executeScalarTransactionSql(CmdString, transaction, new object[] {
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
                    true,
                    0
                });

                    if (Product.Choice1 != null && Product.Choice1.Count > 0)
                    {
                        foreach (var ch1item in Product.Choice1)
                        {
                            string CmdStringChoice1 = "INSERT INTO [dbo].[Choice1]([ProductId],[Name],[Price],[SubeId],[SubeName],[Choice1PkId],[YeniUrunMu]) values " +
                               "(@par1, @par2, @par3, @par4, @par5, @par6,1)" +
                    "select CAST(scope_identity() AS int);";
                            int choice1Id = sqlData.executeScalarTransactionSql(CmdStringChoice1, transaction, new object[] {
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
                                    if (ch2item.secim1Id == ch1item.secimId)
                                    {
                                        string CmdStringChoice2 = "INSERT INTO [dbo].[Choice2]([ProductId],[Choice1Id],[Name],[Price],[SubeId],[SubeName],[Choice2PkId],[YeniUrunMu]) values " +
                                       "(@par1, @par2, @par3, @par4, @par5, @par6, @par7,1)" +
                            "select CAST(scope_identity() AS int);";
                                        int choice2Id = sqlData.executeScalarTransactionSql(CmdStringChoice2, transaction, new object[] {
                        ID,
                        choice1Id,
                        ch1item.Name,
                        ch1item.Price,
                        Product.SubeId,
                        Product.SubeName,
                        0
                    });
                                    }
                                }
                            }

                        }
                    }

                    if (Product.OptionCats != null && Product.OptionCats.Count > 0)
                    {
                        foreach (var optionCats in Product.OptionCats)
                        {
                            string CmdStringOptionCats = "INSERT INTO [dbo].[OptionCats]([Name],[ProductId],[MaxSelections],[MinSelections],[SubeId],[SubeName],[OptionCatsPkId],[YeniUrunMu]) values " +
                                 "(@par1, @par2, @par3, @par4, @par5, @par6, @par7,1)" +
                      "select CAST(scope_identity() AS int);";
                            int optionCatsId = sqlData.executeScalarTransactionSql(CmdStringOptionCats, transaction, new object[] {
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
                                        int choice2Id = sqlData.executeScalarTransactionSql(CmdStringOptions, transaction, new object[] {
                                            options.Name,
                                            options.Price,
                                            options.Quantitative,
                                            ID,
                                            optionCatsId,
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
                            if (options.OptionCatsId == null)
                            {
                                string CmdStringOptions = "INSERT INTO [dbo].[Options]([Name],[Price],[Quantitative],[ProductId],[Category],[SubeId],[SubeName],[OptionsPkId],[YeniUrunMu]) values " +
                                     "(@par1, @par2, @par3, @par4, @par5, @par6, @par7, @par8,1)" +
                                      "select CAST(scope_identity() AS int);";
                                int choice2Id = sqlData.executeScalarTransactionSql(CmdStringOptions, transaction, new object[] {
                                    options.Name,
                                    options.Price,
                                    options.Quantitative,
                                    ID,
                                    "",
                                    Product.SubeId,
                                    Product.SubeName,
                                    0
                                });
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
                    return result;
                }
            }
            result.result_STR = "Başarılı";
            result.result_INT = 1;
            result.result_BOOL = true;
            result.result_OBJECT = Product;
            result.IsSuccess = true;
            result.UserMessage = "İşlem Başarılı";
            return result;

        }
        public ActionResultMessages Delete(int id)
        {
            ActionResultMessages result = new ActionResultMessages();
            ModelFunctions f = new ModelFunctions();

            try
            {
                f.SqlConnOpen();
                string CmdString = "Delete from Product where Id=" + id;
                CmdString += " Delete from Choice1 where ProductId=" + id;
                CmdString += " Delete from Choice2 where ProductId=" + id;
                CmdString += " Delete from Options where ProductId=" + id;
                CmdString += " Delete from OptionCats where ProductId=" + id;

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

        public ActionResultMessages Copy(int id, List<SelectListItem> SubeList)
        {
            var result = new ActionResultMessages();
            result.UserMessage = "";

            try
            {
                var product = GetProduct(id);

                #region **** Kendi Kaydını eklemek için *****

                var resultKendisi = Insert(product);
                if (!resultKendisi.IsSuccess)
                {
                    result.UserMessage += "Şube:" + product.SubeName + " hata:" + resultKendisi.UserMessage;
                }
                #endregion **** Kendi Kaydını eklemek için *****

                foreach (var sube in SubeList)
                {
                    product.SubeId = Convert.ToInt32(sube.Value);
                    product.SubeName = sube.Text;

                    ActionResultMessages msg = Insert(product);
                    if (!msg.IsSuccess)
                    {
                        result.UserMessage += "Şube:" + product.SubeName + " hata:" + msg.UserMessage;
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

        public static List<SefimPanelUrunEkleViewModel> ProductList()
        {
            List<SefimPanelUrunEkleViewModel> Liste = new List<SefimPanelUrunEkleViewModel>();
            ModelFunctions f = new ModelFunctions();
            try
            {
                #region SUBSTATION LIST               
                f.SqlConnOpen();
                DataTable dt = f.DataTable("select * from SubeSettings Where Status=1");
                var dtList = dt.AsEnumerable().ToList<DataRow>();
                f.SqlConnClose();
                #endregion SUBSTATION LIST

                #region PARALLEL FOREACH

                var locked = new Object();
                Parallel.ForEach(dtList, r =>
                {
                    //foreach (DataRow r in dt.Rows)
                    //{
                    string SubeId = f.RTS(r, "Id");
                    string SubeAdi = f.RTS(r, "SubeName");
                    string SubeIP = f.RTS(r, "SubeIP");
                    string SqlName = f.RTS(r, "SqlName");
                    string SqlPassword = f.RTS(r, "SqlPassword");
                    string DBName = f.RTS(r, "DBName");


                    string Query = "select * from Product";

                    #region GET DATA
                    try
                    {
                        DataTable AcikHesapDt = new DataTable();
                        AcikHesapDt = f.GetSubeDataWithQuery((f.NewConnectionString(SubeIP, DBName, SqlName, SqlPassword)), Query.ToString());

                        foreach (DataRow SubeR in AcikHesapDt.Rows)
                        {

                            //items.Sube = SubeCiroDt.Rows[0]["Sube"].ToString(); //f.RTS(SubeR, "Sube");
                            //items.SubeId = SubeId;
                            //items.Cash = Convert.ToDecimal(SubeCiroDt.Rows[0]["Cash"]); //f.RTD(SubeR, "Cash");
                            SefimPanelUrunEkleViewModel items = new SefimPanelUrunEkleViewModel();
                            items.ProductName = f.RTS(SubeR, "ProductName");
                            items.ProductGroup = f.RTS(SubeR, "ProductGroup");
                            items.ProductCode = f.RTS(SubeR, "ProductCode");
                            items.Order = f.RTS(SubeR, "[Order]");
                            items.Price = f.RTS(SubeR, "Price");
                            items.VatRate = f.RTS(SubeR, "VatRate");
                            if (SubeR["FreeItem"] != DBNull.Value)
                            {
                                items.FreeItem = Convert.ToBoolean(f.RTS(SubeR, "FreeItem"));
                            }
                            items.InvoiceName = f.RTS(SubeR, "InvoiceName");
                            items.ProductType = f.RTS(SubeR, "ProductType");
                            items.Plu = f.RTS(SubeR, "Plu");
                            if (SubeR["SkipOptionSelection"] != DBNull.Value)
                            {
                                items.SkipOptionSelection = Convert.ToBoolean(f.RTS(SubeR, "SkipOptionSelection"));
                            }
                            items.Favorites = f.RTS(SubeR, "Favorites");
                            if (SubeR["Aktarildi"] != DBNull.Value)
                            {
                                items.Aktarildi = Convert.ToBoolean(f.RTS(SubeR, "Aktarildi"));
                            }

                            items.ProductPkId = f.RTI(SubeR, "ProductPkId");


                            items.SubeId = Convert.ToInt32(SubeId);
                            items.SubeName = SubeAdi;
                            //items.Adet = f.RTI(SubeR, "ADET");
                            //items.Debit = f.RTD(SubeR, "TUTAR");
                            //items.PhoneOrderDebit = f.RTI(SubeR, "PhoneOrderDebit");

                            lock (locked)
                            {
                                Liste.Add(items);
                            }
                        }
                    }
                    catch (System.Exception ex)
                    {

                    }
                    #endregion

                });
                #endregion PARALLEL FOREACH
            }
            catch (DataException ex) { }


            try
            {
                f.SqlConnOpen();
                DataTable dt = f.DataTable("select * from Product Where YeniUrunMu = 1");
                foreach (DataRow SubeR in dt.Rows)
                {

                    //items.Sube = SubeCiroDt.Rows[0]["Sube"].ToString(); //f.RTS(SubeR, "Sube");
                    //items.SubeId = SubeId;
                    //items.Cash = Convert.ToDecimal(SubeCiroDt.Rows[0]["Cash"]); //f.RTD(SubeR, "Cash");
                    SefimPanelUrunEkleViewModel items = new SefimPanelUrunEkleViewModel();
                    items.ProductName = f.RTS(SubeR, "ProductName");
                    items.ProductGroup = f.RTS(SubeR, "ProductGroup");
                    items.ProductCode = f.RTS(SubeR, "ProductCode");
                    items.Order = f.RTS(SubeR, "[Order]");
                    items.Price = f.RTS(SubeR, "Price");
                    items.VatRate = f.RTS(SubeR, "VatRate");
                    if (SubeR["FreeItem"] != DBNull.Value)
                    {
                        items.FreeItem = Convert.ToBoolean(f.RTS(SubeR, "FreeItem"));
                    }
                    items.InvoiceName = f.RTS(SubeR, "InvoiceName");
                    items.ProductType = f.RTS(SubeR, "ProductType");
                    items.Plu = f.RTS(SubeR, "Plu");
                    if (SubeR["SkipOptionSelection"] != DBNull.Value)
                    {
                        items.SkipOptionSelection = Convert.ToBoolean(f.RTS(SubeR, "SkipOptionSelection"));
                    }
                    items.Favorites = f.RTS(SubeR, "Favorites");
                    if (SubeR["Aktarildi"] != DBNull.Value)
                    {
                        items.Aktarildi = Convert.ToBoolean(f.RTS(SubeR, "Aktarildi"));
                    }

                    items.ProductPkId = f.RTI(SubeR, "ProductPkId");


                    items.SubeId = Convert.ToInt32(f.RTI(SubeR, "SubeId"));
                    items.SubeName = f.RTS(SubeR, "SubeName");
                    items.YeniUrunMu = true;
                    items.Id = f.RTI(SubeR, "Id");
                    //items.Adet = f.RTI(SubeR, "ADET");
                    //items.Debit = f.RTD(SubeR, "TUTAR");
                    //items.PhoneOrderDebit = f.RTI(SubeR, "PhoneOrderDebit");


                    Liste.Add(items);

                }




                f.SqlConnClose();
            }
            catch (System.Exception ex) { }



            return Liste;
        }

        public SefimPanelUrunEkleViewModel GetProduct(int Id)
        {
            SefimPanelUrunEkleViewModel items = new SefimPanelUrunEkleViewModel();
            ModelFunctions f = new ModelFunctions();
            try
            {
                f.SqlConnOpen();
                DataTable dt = f.DataTable("select * from Product Where YeniUrunMu = 1 and Id=" + Id);
                foreach (DataRow SubeR in dt.Rows)
                {

                    //items.Sube = SubeCiroDt.Rows[0]["Sube"].ToString(); //f.RTS(SubeR, "Sube");
                    //items.SubeId = SubeId;
                    //items.Cash = Convert.ToDecimal(SubeCiroDt.Rows[0]["Cash"]); //f.RTD(SubeR, "Cash");

                    items.ProductName = f.RTS(SubeR, "ProductName");
                    items.ProductGroup = f.RTS(SubeR, "ProductGroup");
                    items.ProductCode = f.RTS(SubeR, "ProductCode");
                    items.Order = f.RTS(SubeR, "Order");
                    items.Price = f.RTS(SubeR, "Price").Replace(",", ".");
                    items.VatRate = f.RTS(SubeR, "VatRate").Replace(",", ".");
                    if (SubeR["FreeItem"] != DBNull.Value)
                    {
                        items.FreeItem = Convert.ToBoolean(f.RTS(SubeR, "FreeItem"));
                    }
                    items.InvoiceName = f.RTS(SubeR, "InvoiceName");
                    items.ProductType = f.RTS(SubeR, "ProductType");
                    items.Plu = f.RTS(SubeR, "Plu");
                    if (SubeR["SkipOptionSelection"] != DBNull.Value)
                    {
                        items.SkipOptionSelection = Convert.ToBoolean(f.RTS(SubeR, "SkipOptionSelection"));
                    }
                    items.Favorites = f.RTS(SubeR, "Favorites");
                    if (SubeR["Aktarildi"] != DBNull.Value)
                    {
                        items.Aktarildi = Convert.ToBoolean(f.RTS(SubeR, "Aktarildi"));
                    }

                    items.ProductPkId = f.RTI(SubeR, "ProductPkId");


                    items.SubeId = Convert.ToInt32(f.RTI(SubeR, "SubeId"));
                    items.SubeName = f.RTS(SubeR, "SubeName");
                    items.YeniUrunMu = true;
                    items.Id = f.RTI(SubeR, "Id");
                    //items.Adet = f.RTI(SubeR, "ADET");
                    //items.Debit = f.RTD(SubeR, "TUTAR");
                    //items.PhoneOrderDebit = f.RTI(SubeR, "PhoneOrderDebit");


                    items.Choice1 = new List<Choice1>();
                    items.Choice2 = new List<Choice2>();
                    items.Options = new List<Options>();
                    items.OptionCats = new List<OptionCats>();

                    DataTable dtChoise1 = f.DataTable("select * from Choice1 Where ProductId=" + Id);
                    foreach (DataRow choise1 in dtChoise1.Rows)
                    {
                        Choice1 choice1 = new Choice1();
                        choice1.Id = f.RTI(choise1, "Id");
                        choice1.Name = f.RTS(choise1, "Name");
                        choice1.Price = f.RTS(choise1, "Price").Replace(",", ".");
                        choice1.ProductId = f.RTI(choise1, "ProductId");

                        items.Choice1.Add(choice1);
                    }

                    DataTable dtChoise2 = f.DataTable("select * from Choice2 Where ProductId=" + Id);
                    foreach (DataRow choise2 in dtChoise2.Rows)
                    {
                        Choice2 choice2 = new Choice2();
                        choice2.Id = f.RTI(choise2, "Id");
                        choice2.Name = f.RTS(choise2, "Name");
                        choice2.Price = f.RTS(choise2, "Price").Replace(",", ".");
                        choice2.Choice1Id = f.RTI(choise2, "Choice1Id");
                        choice2.ProductId = f.RTI(choise2, "ProductId");

                        items.Choice2.Add(choice2);
                    }

                    DataTable dtOptions = f.DataTable("select * from Options Where ProductId=" + Id);
                    foreach (DataRow opt in dtOptions.Rows)
                    {
                        Options options = new Options();
                        options.Id = f.RTI(opt, "Id");
                        options.Name = f.RTS(opt, "Name");
                        options.Price = f.RTS(opt, "Price").Replace(",", ".");
                        options.Quantitative = Convert.ToBoolean(f.RTS(opt, "Quantitative"));
                        options.Category = f.RTS(opt, "Category");
                        options.ProductId = f.RTI(opt, "ProductId");

                        items.Options.Add(options);
                    }

                    DataTable dtOptionCats = f.DataTable("select * from OptionCats Where ProductId=" + Id);
                    foreach (DataRow optCat in dtOptionCats.Rows)
                    {
                        OptionCats optionCats = new OptionCats();
                        optionCats.Id = f.RTI(optCat, "Id");
                        optionCats.Name = f.RTS(optCat, "Name");
                        optionCats.MaxSelections = f.RTS(optCat, "MaxSelections").Replace(",", ".");
                        optionCats.MinSelections = f.RTS(optCat, "MinSelections").Replace(",", ".");
                        optionCats.ProductId = f.RTI(optCat, "ProductId");

                        items.OptionCats.Add(optionCats);
                    }
                }


                f.SqlConnClose();

            }
            catch
            {

            }
            return items;
        }

        public static List<SelectListItem> GetSubeList()
        {
            List<SelectListItem> items = new List<SelectListItem>();
            ModelFunctions f = new ModelFunctions();
            try
            {
                f.SqlConnOpen();
                DataTable dt = f.DataTable("SELECT [Id],[CreateDate],[CreateDate_Timestamp],[ModifyCounter],[UpdateDate],[UpdateDate_Timestamp],[SubeName],[SubeIP],[SqlName],[SqlPassword],[DBName],[FirmaID],[DonemID],[DepoID],[Status],[AppDbType],[AppDbTypeStatus],[FasterSubeID],[SefimPanelZimmetCagrisi],[BelgeSayimTarihDahil],[ServiceAdress] FROM[SefimReportDB_Yeni_1].[dbo].[SubeSettings]");
                foreach (DataRow r in dt.Rows)
                {
                    items.Add(new SelectListItem
                    {
                        Text = f.RTS(r, "SubeName").ToString(),
                        Value = f.RTS(r, "Id"),
                    });
                }
                f.SqlConnClose();
            }
            catch (System.Exception ex) { }
            return items;
        }

        //local deki pruduct tablosunu hedef şubelere insert yapar,
        public ActionResultMessages IsInsertProduct()
        {
            var result = new ActionResultMessages() { IsSuccess = true, UserMessage = "İşlem Başarılı", };

            #region Local pruduct tablosundaki şubeler alınır 

            var insertSProductSubeList = mF.GetSubeDataWithQuery(mF.NewConnectionString(subeIp, dbName, sqlKullaniciName, sqlKullaniciPassword), SqlData.getProductJoinSubesettingsSqlQuery());
            foreach (DataRow itemSube in insertSProductSubeList.Rows)
            {
                var insertSubeId = mF.RTS(itemSube, "SubeId");
                var insertSubeIp = mF.RTS(itemSube, "SubeIp");
                var insertDbName = mF.RTS(itemSube, "DBName");
                var insertsubeName = mF.RTS(itemSube, "SubeName");
                var insertSqlKullaniciName = mF.RTS(itemSube, "SqlName");
                var insertSqlKullaniciPassword = mF.RTS(itemSube, "SqlPassword");

                var tempProductList = mF.GetSubeDataWithQuery(mF.NewConnectionString(subeIp, dbName, sqlKullaniciName, sqlKullaniciPassword), SqlData.insertProductSqlQuery(dbName, insertDbName));

                foreach (DataRow itemPruduct in tempProductList.Rows)
                {
                    var sqlDataInsert = new SqlData(new SqlConnection(mF.NewConnectionString(insertSubeIp, insertDbName, insertSqlKullaniciName, insertSqlKullaniciPassword)));

                    string CmdString = "INSERT INTO [dbo].[Product]([ProductName],[ProductGroup],[ProductCode],[Order],[Price],[VatRate],[FreeItem],[InvoiceName],[ProductType],[Plu],[SkipOptionSelection],[Favorites]) VALUES" +
                                       "(@par1, @par2, @par3, @par4, @par5, @par6, @par7, @par8, @par9, @par10, @par11, @par12) select CAST(scope_identity() AS int)";
                    int productId = sqlDataInsert.executeScalarSql(CmdString, new object[] {
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
                                            });

                    var tempChoice1List = mF.GetSubeDataWithQuery(mF.NewConnectionString(subeIp, dbName, sqlKullaniciName, sqlKullaniciPassword), SqlData.insertChoice1SqlQuery(itemPruduct["Id"].ToString(), insertDbName));
                    foreach (DataRow itemChoice1 in tempChoice1List.Rows)
                    {
                        string CmdStringChoice1 = "INSERT INTO [dbo].[Choice1]([ProductId],[Name],[Price]) values (@par1, @par2, @par3) select CAST(scope_identity() AS int)";
                        int choice1Id = sqlDataInsert.executeScalarSql(CmdStringChoice1, new object[] { productId, itemChoice1["Name"], itemChoice1["Price"] });

                        var tempChoice2List = mF.GetSubeDataWithQuery(mF.NewConnectionString(subeIp, dbName, sqlKullaniciName, sqlKullaniciPassword), SqlData.insertChoice2SqlQuery(itemChoice1["Id"].ToString(), insertDbName));
                        foreach (DataRow itemChoice2 in tempChoice2List.Rows)
                        {
                            string CmdStringChoice2 = "INSERT INTO [dbo].[Choice2]([ProductId],[Choice1Id],[Name],[Price]) values (@par1, @par2, @par3, @par4) select CAST(scope_identity() AS int) ";
                            int choice2Id = sqlDataInsert.executeScalarSql(CmdStringChoice2, new object[] { productId, choice1Id, itemChoice2["Name"], itemChoice2["Price"] });
                        }
                    }

                    //options
                    var tempOptionsList = mF.GetSubeDataWithQuery(mF.NewConnectionString(subeIp, dbName, sqlKullaniciName, sqlKullaniciPassword), SqlData.insertOptionsSqlQuery(itemPruduct["Id"].ToString(), insertDbName));
                    foreach (DataRow itemOptions in tempOptionsList.Rows)
                    {
                        string cmdStringOptions = "INSERT INTO [dbo].[Options]([Name],[Price],[Quantitative],[ProductId],[Category]) values (@par1, @par2, @par3, @par4, @par5) select CAST(scope_identity() AS int)";
                        int choice2Id = sqlDataInsert.executeScalarSql(cmdStringOptions, new object[] {
                        itemOptions["Name"],
                        itemOptions["Price"],
                        itemOptions["Quantitative"],
                        productId,
                        itemOptions["Category"]
                    });
                    }
                }
            }

            #endregion Local pruduct tablosundaki şubeler alınır 

            #region Product tablosunda Delete  yapar
            var sqlData = new SqlData(new SqlConnection(mF.NewConnectionString(subeIp, dbName, sqlKullaniciName, sqlKullaniciPassword)));
            sqlData.executeSql("Delete Product where YeniUrunmu=1 ", new object[] { });
            sqlData.executeSql("Delete Choice1 where YeniUrunmu=1 ", new object[] { });
            sqlData.executeSql("Delete Choice2 where YeniUrunmu=1 ", new object[] { });
            sqlData.executeSql("Delete Options where YeniUrunmu=1 ", new object[] { });

            #endregion Product tablosunda Delete yapar

            return result;
        }
    }
}