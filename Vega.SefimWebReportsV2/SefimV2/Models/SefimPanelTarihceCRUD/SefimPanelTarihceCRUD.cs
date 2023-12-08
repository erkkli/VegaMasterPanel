using SefimV2.Helper;
using SefimV2.ViewModels.SPosVeriGonderimi;
using SefimV2.ViewModels.SubeSettings;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Web.Configuration;
using System.Web.Mvc;
using static SefimV2.ViewModels.SPosVeriGonderimi.UrunEditViewModel;

namespace SefimV2.Models
{
    public class SefimPanelTarihceCRUD
    {
        ModelFunctions modelFunctions = new ModelFunctions();
        #region Config local copy db connction setting       
        static string subeIp = WebConfigurationManager.AppSettings["Server"];
        static string dbName = WebConfigurationManager.AppSettings["DBName"];
        static string sqlKullaniciName = WebConfigurationManager.AppSettings["User"];
        static string sqlKullaniciPassword = WebConfigurationManager.AppSettings["Password"];
        #endregion

        #region Şube listesi

        public List<SubeSettingsViewModel> GetSubeList()
        {
            var liste = new List<SubeSettingsViewModel>();
            modelFunctions.SqlConnOpen();
            var dt = modelFunctions.DataTable("Select Id, SubeName,DBName, * From SubeSettings Where AppDbTypeStatus in (0,1) ");
            foreach (DataRow r in dt.Rows)
            {
                SubeSettingsViewModel model = new SubeSettingsViewModel();
                model.ID = Convert.ToInt32(modelFunctions.RTS(r, "Id"));
                model.SubeName = (modelFunctions.RTS(r, "SubeName"));
                //model.SubeIP = modelFunctions.RTS(r, "SubeIP");
                //model.SqlName = modelFunctions.RTS(r, "SqlName");
                //model.SqlPassword = modelFunctions.RTS(r, "SqlPassword");
                model.DBName = modelFunctions.RTS(r, "DBName");
                //model.FirmaID = modelFunctions.RTS(r, "FirmaID");                
                //model.Status = Convert.ToBoolean(modelFunctions.RTS(r, "Status"));
                //model.AppDbType = Convert.ToInt32(modelFunctions.RTS(r, "AppDbType"));
                //model.AppDbTypeStatus = Convert.ToBoolean(modelFunctions.RTS(r, "AppDbTypeStatus"));
                liste.Add(model);
            }
            modelFunctions.SqlConnClose();

            return liste;
        }
        public SubelereVeriGonderViewModel GetSubeListIsSelected()
        {
            var liste = new SubelereVeriGonderViewModel();
            liste.IsSelectedSubeList = new List<SubeSettingsViewModel>();
            modelFunctions.SqlConnOpen();
            var dt = modelFunctions.DataTable("Select Id, SubeName,DBName, * From SubeSettings Where AppDbTypeStatus in (0,1) ");
            foreach (DataRow r in dt.Rows)
            {
                SubeSettingsViewModel model = new SubeSettingsViewModel();
                model.ID = Convert.ToInt32(modelFunctions.RTS(r, "Id"));
                model.SubeName = (modelFunctions.RTS(r, "SubeName"));
                model.DBName = modelFunctions.RTS(r, "DBName");
                liste.IsSelectedSubeList.Add(model);
            }
            modelFunctions.SqlConnClose();

            return liste;
        }

        #endregion Şube listesi

        #region Products güncelleme

        public UrunEditViewModel GetBySubeIdForProductTarihce(int SubeId, string ProductGroup)
        {
            var urunList = new UrunEditViewModel();

            try
            {
                DataTable dataSubeUrunlist;
                var sqlData = new SqlData(new SqlConnection(modelFunctions.NewConnectionString(subeIp, dbName, sqlKullaniciName, sqlKullaniciPassword)));
                dataSubeUrunlist = modelFunctions.GetSubeDataWithQuery((modelFunctions.NewConnectionString(subeIp, dbName, sqlKullaniciName, sqlKullaniciPassword))
                                                                          , ProductGroup == "null" || ProductGroup == null ? SqlData.getProducTarihceLocalDbSqlQuery(SubeId) : SqlData.getProductTarihceForProductGroupSqlQuery(ProductGroup));
                var modelSube = SqlData.GetSube(SubeId);

                urunList.UrunEditList = new List<UrunEdit>();
                urunList.SubeId = SubeId;
                urunList.SubeAdi = modelSube.SubeName;

                foreach (DataRow item in dataSubeUrunlist.Rows)
                {
                    UrunEdit model = new UrunEdit();
                    model.Id = modelFunctions.RTI(item, "Id");
                    model.InvoiceName = modelFunctions.RTS(item, "InvoiceName");
                    //model.Order = modelFunctions.RTS(item, "[Order]");
                    model.Plu = modelFunctions.RTS(item, "Plu");
                    model.Price = modelFunctions.RTD(item, "Price");
                    model.ProductCode = modelFunctions.RTS(item, "ProductCode");
                    model.ProductGroup = modelFunctions.RTS(item, "ProductGroup");
                    model.ProductName = modelFunctions.RTS(item, "ProductName");
                    model.ProductType = modelFunctions.RTS(item, "ProductType");
                    model.VatRate = modelFunctions.RTD(item, "VatRate");
                    model.VatRate = modelFunctions.RTD(item, "VatRate");
                    model.IsUpdateDate = modelFunctions.RTS(item, "IsUpdateDate");
                    model.IsUpdateKullanici = modelFunctions.RTS(item, "IsUpdateKullanici");

                    //model.Choice1Id = modelFunctions.RTI(item, "Choice1Id");
                    //model.ChoiceProductName = modelFunctions.RTS(item, "Choice1_Name");
                    //model.ChoicePrice = modelFunctions.RTD(item, "Choice1_Price");

                    //model.Choice2Id = modelFunctions.RTI(item, "Choice2Id");
                    //model.Choice2ProductName = modelFunctions.RTS(item, "Choice2_Name");
                    //model.Choice2Price = modelFunctions.RTD(item, "Choice2_Price");

                    //model.OptionsId = modelFunctions.RTI(item, "OptionsId");
                    //model.OptionsProductName = modelFunctions.RTS(item, "OptionsName");
                    //model.OptionsPrice = modelFunctions.RTD(item, "Option_Price");

                    urunList.UrunEditList.Add(model);
                }

                return urunList;

            }
            catch (Exception ex)
            {
                urunList.ErrorList = new List<string>();
                urunList.ErrorList.Add(ex.Message.ToString());
                return urunList;
            }
        }

        #endregion Products güncelleme


        public UrunEditViewModel GetBySubeIdForChoice1Tarihce(int SubeId, string ProductName, string ProductGroup)
        {
            #region Choice1 listesi

            DataTable dataSubeUrunlist;
            var sqlData = new SqlData(new SqlConnection(modelFunctions.NewConnectionString(subeIp, dbName, sqlKullaniciName, sqlKullaniciPassword)));
            dataSubeUrunlist = modelFunctions.GetSubeDataWithQuery((modelFunctions.NewConnectionString(subeIp, dbName, sqlKullaniciName, sqlKullaniciPassword)), SqlData.getChoice1TarihceListSqlQuery(ProductGroup, ProductName));
            var modelSube = SqlData.GetSube(SubeId);

            var choiceUrunList = new UrunEditViewModel();
            choiceUrunList.UrunEditList = new List<UrunEdit>();
            choiceUrunList.SubeId = SubeId;
            choiceUrunList.SubeAdi = modelSube.SubeName;

            foreach (DataRow item in dataSubeUrunlist.Rows)
            {
                UrunEdit model = new UrunEdit();
                model.Id = modelFunctions.RTI(item, "ProductId");
                model.Choice1Id = modelFunctions.RTI(item, "Id");
                model.Price = modelFunctions.RTD(item, "Product_Price");
                model.ChoicePrice = modelFunctions.RTD(item, "Choice1_Price");
                model.ProductGroup = modelFunctions.RTS(item, "ProductGroup");
                model.ProductName = modelFunctions.RTS(item, "ProductName");
                model.ChoiceProductName = modelFunctions.RTS(item, "ChoiceProductName");
                model.IsUpdateDate = modelFunctions.RTS(item, "IsUpdateDate");
                model.IsUpdateKullanici = modelFunctions.RTS(item, "IsUpdateKullanici");
                choiceUrunList.UrunEditList.Add(model);
            }

            #endregion  Choice1 listesi

            #region Options listesi

            DataTable dataSubeUrunOptionslist;
            dataSubeUrunOptionslist = modelFunctions.GetSubeDataWithQuery((modelFunctions.NewConnectionString(subeIp, dbName, sqlKullaniciName, sqlKullaniciPassword)), SqlData.getTarihceOptionsListSqlQuery(ProductGroup, ProductName));

            choiceUrunList.UrunOptionsEditList = new List<UrunEdit>();
            foreach (DataRow item in dataSubeUrunOptionslist.Rows)
            {
                UrunEdit model = new UrunEdit();
                model.Id = modelFunctions.RTI(item, "ProductId");
                model.OptionsId = modelFunctions.RTI(item, "Id");
                model.Price = modelFunctions.RTD(item, "Product_Price");
                model.OptionsPrice = modelFunctions.RTD(item, "Option_Price");
                model.ProductGroup = modelFunctions.RTS(item, "ProductGroup");
                model.ProductName = modelFunctions.RTS(item, "ProductName");
                model.OptionsProductName = modelFunctions.RTS(item, "OptionsName");
                model.IsUpdateDate = modelFunctions.RTS(item, "IsUpdateDate");
                model.IsUpdateKullanici = modelFunctions.RTS(item, "IsUpdateKullanici");
                choiceUrunList.UrunOptionsEditList.Add(model);
            }

            #endregion Options listesi


            return choiceUrunList;
        }



        #region Product,Options,Choice1,Choice2 local db'ye kayıt için jenerik insert class

        public ActionResultMessages LocalDbProductAndOprionsAndChoice1AndChoice2Insert(UrunEditViewModel model, bool choiceMu)
        {
            var result = new ActionResultMessages
            {
                IsSuccess = true,
                UserMessage = "İşlem başarılı",
            };

            var dataSubeUrunlist = modelFunctions.GetSubeDataWithQuery((modelFunctions.NewConnectionString(subeIp, dbName, sqlKullaniciName, sqlKullaniciPassword)), SqlData.getProductLocalDbSqlQuery(model.SubeId));
            var sqlData = new SqlData(new SqlConnection(modelFunctions.NewConnectionString(subeIp, dbName, sqlKullaniciName, sqlKullaniciPassword)));
            var modelSube = SqlData.GetSube(model.SubeId);

            if (choiceMu)
            {
                dataSubeUrunlist = modelFunctions.GetSubeDataWithQuery((modelFunctions.NewConnectionString(modelSube.SubeIP, modelSube.DBName, modelSube.SqlName, modelSube.SqlPassword)), SqlData.getProductSqlQuery());

                foreach (DataRow item in dataSubeUrunlist.Rows)
                {
                    //Product
                    sqlData.ExecuteSql(" Insert Into  Product( ProductName, ProductGroup, Price, SubeId, ProductPkId ) Values( @par1, @par2, @par3, @par4, @par5 )",
                        new object[] { modelFunctions.RTS(item, "ProductName"), modelFunctions.RTS(item, "ProductGroup"), modelFunctions.RTS(item, "Price"), model.SubeId, modelFunctions.RTI(item, "Id") });

                    //Choice1
                    var dataSubeChoiceUrunlist = modelFunctions.GetSubeDataWithQuery((modelFunctions.NewConnectionString(modelSube.SubeIP, modelSube.DBName, modelSube.SqlName, modelSube.SqlPassword)), SqlData.getChoice1SqlQuery(modelFunctions.RTI(item, "Id")));
                    if (dataSubeChoiceUrunlist.Rows.Count > 0)
                    {
                        foreach (DataRow dataChoice1 in dataSubeChoiceUrunlist.Rows)
                        {
                            sqlData.ExecuteSql(" Insert Into  Choice1( ProductId, Name, Price, SubeId, Choice1PkId ) Values( @par1, @par2, @par3, @par4, @par5 )",
                            new object[] { modelFunctions.RTI(dataChoice1, "ProductId"), modelFunctions.RTS(dataChoice1, "ChoiceProductName"), modelFunctions.RTD(dataChoice1, "Choice1_Price"), model.SubeId, modelFunctions.RTI(dataChoice1, "Id") });

                            //Choice2
                            var dataSubeChoice2Urunlist = modelFunctions.GetSubeDataWithQuery((modelFunctions.NewConnectionString(modelSube.SubeIP, modelSube.DBName, modelSube.SqlName, modelSube.SqlPassword)),
                                                                                                                                        SqlData.getChoice2SqlQuery(modelFunctions.RTI(dataChoice1, "Id"), modelFunctions.RTI(item, "Id")));
                            if (dataSubeChoice2Urunlist.Rows.Count > 0)
                            {
                                foreach (DataRow dataChoice2 in dataSubeChoice2Urunlist.Rows)
                                {
                                    sqlData.ExecuteSql(" Insert Into  Choice2( ProductId, Choice1Id, Name, Price, SubeId, Choice2PkId ) Values( @par1, @par2, @par3, @par4, @par5 )",
                                    new object[] { modelFunctions.RTI(dataChoice2, "ProductId"), modelFunctions.RTI(dataChoice1, "Id"), modelFunctions.RTS(dataChoice2, "Choice2ProductName"), modelFunctions.RTD(dataChoice2, "Choice2_Price"), model.SubeId, modelFunctions.RTI(dataChoice2, "Id") });
                                }
                            }
                            //Choice2
                        }
                    }

                    //Options
                    var dataSubeUrunOptionslist = modelFunctions.GetSubeDataWithQuery((modelFunctions.NewConnectionString(modelSube.SubeIP, modelSube.DBName, modelSube.SqlName, modelSube.SqlPassword)), SqlData.getOptionsSqlQuery(modelFunctions.RTI(item, "Id")));
                    if (dataSubeUrunOptionslist.Rows.Count > 0)
                    {
                        foreach (DataRow dataOptions in dataSubeUrunOptionslist.Rows)
                        {
                            sqlData.ExecuteSql(" Insert Into  Options(ProductId, Name, Price, Category, SubeId, OptionsPkId ) Values( @par1, @par2, @par3, @par4, @par5, @par6)",
                            new object[] { modelFunctions.RTI(dataOptions, "ProductId"), modelFunctions.RTS(dataOptions, "OptionsName"), modelFunctions.RTD(dataOptions, "Option_Price"), modelFunctions.RTD(dataOptions, "OptionsCategory"), model.SubeId, modelFunctions.RTI(dataOptions, "Id") });
                        }
                    }
                }
            }
            else
            {
                //TODO mükerrer kod oldu sonra code review yapılabilir.
                foreach (var modelList in model.UrunEditList)
                {
                    if (dataSubeUrunlist.Rows.Count == 0)
                    {
                        //Product
                        sqlData.ExecuteSql(" Insert Into  Product( ProductName, ProductGroup, Price, SubeId, ProductPkId ) Values( @par1, @par2, @par3, @par4, @par5 )",
                            new object[] { modelList.ProductName, modelList.ProductGroup, modelList.Price, model.SubeId, modelList.Id });

                        //Choice1
                        var dataSubeChoiceUrunlist = modelFunctions.GetSubeDataWithQuery((modelFunctions.NewConnectionString(modelSube.SubeIP, modelSube.DBName, modelSube.SqlName, modelSube.SqlPassword)), SqlData.getChoice1SqlQuery(modelList.Id));
                        if (dataSubeChoiceUrunlist.Rows.Count > 0)
                        {
                            foreach (DataRow dataChoice1 in dataSubeChoiceUrunlist.Rows)
                            {
                                sqlData.ExecuteSql(" Insert Into  Choice1( ProductId, Name, Price, SubeId, Choice1PkId ) Values( @par1, @par2, @par3, @par4, @par5 )",
                                new object[] { modelFunctions.RTI(dataChoice1, "ProductId"), modelFunctions.RTS(dataChoice1, "ChoiceProductName"), modelFunctions.RTD(dataChoice1, "Choice1_Price"), model.SubeId, modelFunctions.RTI(dataChoice1, "Id") });

                                //Choice2
                                var dataSubeChoice2Urunlist = modelFunctions.GetSubeDataWithQuery((modelFunctions.NewConnectionString(modelSube.SubeIP, modelSube.DBName, modelSube.SqlName, modelSube.SqlPassword)),
                                                                                                                                            SqlData.getChoice2SqlQuery(modelFunctions.RTI(dataChoice1, "Id"), modelList.Id));
                                if (dataSubeChoice2Urunlist.Rows.Count > 0)
                                {
                                    foreach (DataRow dataChoice2 in dataSubeChoice2Urunlist.Rows)
                                    {
                                        sqlData.ExecuteSql(" Insert Into  Choice2( ProductId, Choice1Id, Name, Price, SubeId, Choice2PkId ) Values( @par1, @par2, @par3, @par4, @par5 )",
                                        new object[] { modelFunctions.RTI(dataChoice2, "ProductId"), modelFunctions.RTI(dataChoice1, "Id"), modelFunctions.RTS(dataChoice2, "Choice2ProductName"), modelFunctions.RTD(dataChoice2, "Choice2_Price"), model.SubeId, modelFunctions.RTI(dataChoice2, "Id") });
                                    }
                                }
                                //Choice2
                            }
                        }

                        //Options
                        var dataSubeUrunOptionslist = modelFunctions.GetSubeDataWithQuery((modelFunctions.NewConnectionString(modelSube.SubeIP, modelSube.DBName, modelSube.SqlName, modelSube.SqlPassword)), SqlData.getOptionsSqlQuery(modelList.Id));
                        if (dataSubeUrunOptionslist.Rows.Count > 0)
                        {
                            foreach (DataRow dataOptions in dataSubeUrunOptionslist.Rows)
                            {
                                sqlData.ExecuteSql(" Insert Into  Options(ProductId, Name, Price, Category, SubeId, OptionsPkId ) Values( @par1, @par2, @par3, @par4, @par5, @par6)",
                                new object[] { modelFunctions.RTI(dataOptions, "ProductId"), modelFunctions.RTS(dataOptions, "OptionsName"), modelFunctions.RTD(dataOptions, "Option_Price"), modelFunctions.RTD(dataOptions, "OptionsCategory"), model.SubeId, modelFunctions.RTI(dataOptions, "Id") });
                            }
                        }
                    }
                }
            }

            return result;
        }

        #endregion  Product,Options,Choice1,Choice2 local db'ye kayıt için jenerik insert class

        #region Choice1,Choice2 güncelleme

        public UrunEditViewModel GetByIdForChoiceEdit(int SubeId, int productId)
        {
            #region Local db'den  ProductPkId alıyor.       
            var dataSubeUrunPkList = modelFunctions.GetSubeDataWithQuery((modelFunctions.NewConnectionString(subeIp, dbName, sqlKullaniciName, sqlKullaniciPassword)), SqlData.getProductLocalDbPkIdSqlQuery(productId));
            var productIdLocal = 0;
            foreach (DataRow item in dataSubeUrunPkList.Rows)
            {
                productIdLocal = modelFunctions.RTI(item, "ProductPkId");
            }
            #endregion

            #region Choice1 listesi

            DataTable dataSubeUrunlist;
            var sqlData = new SqlData(new SqlConnection(modelFunctions.NewConnectionString(subeIp, dbName, sqlKullaniciName, sqlKullaniciPassword)));
            dataSubeUrunlist = modelFunctions.GetSubeDataWithQuery((modelFunctions.NewConnectionString(subeIp, dbName, sqlKullaniciName, sqlKullaniciPassword)), SqlData.getChoice1LocalDbSqlQuery(SubeId, productIdLocal));
            var modelSube = SqlData.GetSube(SubeId);
            if (dataSubeUrunlist.Rows.Count == 0)
            {
                dataSubeUrunlist = modelFunctions.GetSubeDataWithQuery((modelFunctions.NewConnectionString(modelSube.SubeIP, modelSube.DBName, modelSube.SqlName, modelSube.SqlPassword)), SqlData.getChoice1SqlQuery(productId));
            }

            var choiceUrunList = new UrunEditViewModel();
            choiceUrunList.UrunEditList = new List<UrunEdit>();
            choiceUrunList.SubeId = SubeId;
            choiceUrunList.SubeAdi = modelSube.SubeName;

            foreach (DataRow item in dataSubeUrunlist.Rows)
            {
                UrunEdit model = new UrunEdit();
                model.Id = modelFunctions.RTI(item, "ProductId");
                model.Choice1Id = modelFunctions.RTI(item, "Id");
                model.Price = modelFunctions.RTD(item, "Product_Price");
                model.ChoicePrice = modelFunctions.RTD(item, "Choice1_Price");
                model.ProductGroup = modelFunctions.RTS(item, "ProductGroup");
                model.ProductName = modelFunctions.RTS(item, "ProductName");
                model.ChoiceProductName = modelFunctions.RTS(item, "ChoiceProductName");
                choiceUrunList.UrunEditList.Add(model);
            }

            #endregion  Choice1 listesi

            #region Options listesi

            DataTable dataSubeUrunOptionslist;
            dataSubeUrunOptionslist = modelFunctions.GetSubeDataWithQuery((modelFunctions.NewConnectionString(subeIp, dbName, sqlKullaniciName, sqlKullaniciPassword)), SqlData.getOptionsLocalDbSqlQuery(SubeId, productIdLocal));
            if (dataSubeUrunOptionslist.Rows.Count == 0)
            {
                dataSubeUrunOptionslist = modelFunctions.GetSubeDataWithQuery((modelFunctions.NewConnectionString(modelSube.SubeIP, modelSube.DBName, modelSube.SqlName, modelSube.SqlPassword)), SqlData.getOptionsSqlQuery(productId));

            }
            choiceUrunList.UrunOptionsEditList = new List<UrunEdit>();
            foreach (DataRow item in dataSubeUrunOptionslist.Rows)
            {
                UrunEdit model = new UrunEdit();
                model.Id = modelFunctions.RTI(item, "ProductId");
                model.OptionsId = modelFunctions.RTI(item, "Id");
                model.Price = modelFunctions.RTD(item, "Product_Price");
                model.OptionsPrice = modelFunctions.RTD(item, "Option_Price");
                model.ProductGroup = modelFunctions.RTS(item, "ProductGroup");
                model.ProductName = modelFunctions.RTS(item, "ProductName");
                model.OptionsProductName = modelFunctions.RTS(item, "OptionsName");
                choiceUrunList.UrunOptionsEditList.Add(model);
            }

            #endregion Options listesi


            return choiceUrunList;
        }
        public ActionResultMessages ChoiceUpdate(UrunEditViewModel model)
        {
            var result = new ActionResultMessages()
            {
                IsSuccess = true,
                UserMessage = "İşlem Başarılı",
            };

            #region Local db'den  ProductPkId alıyor.       
            var dataSubeUrunPkList = modelFunctions.GetSubeDataWithQuery((modelFunctions.NewConnectionString(subeIp, dbName, sqlKullaniciName, sqlKullaniciPassword)), SqlData.getProductLocalDbPkIdSqlQuery(model.UrunEditList.FirstOrDefault().Id));
            var productIdLocal = 0;
            foreach (DataRow item in dataSubeUrunPkList.Rows)
            {
                productIdLocal = modelFunctions.RTI(item, "Choice1PkId");
            }
            #endregion

            var modelSube = SqlData.GetSube(model.SubeId);
            //var sqlData = new SqlData(new SqlConnection(modelFunctions.NewConnectionString(modelSube.SubeIP, modelSube.DBName, modelSube.SqlName, modelSube.SqlPassword)));
            var sqlData = new SqlData(new SqlConnection(modelFunctions.NewConnectionString(subeIp, dbName, sqlKullaniciName, sqlKullaniciPassword)));
            //var dataSubeUrunlist = modelFunctions.GetSubeDataWithQuery((modelFunctions.NewConnectionString(modelSube.SubeIP, modelSube.DBName, modelSube.SqlName, modelSube.SqlPassword)), SqlData.getChoice1(model.UrunEditList[1].Id));
            var dataSubeUrunlist = modelFunctions.GetSubeDataWithQuery((modelFunctions.NewConnectionString(subeIp, dbName, sqlKullaniciName, sqlKullaniciPassword)), SqlData.getChoice1LocalDbSqlQuery(model.SubeId, model.UrunEditList.FirstOrDefault().Id));

            foreach (var modelList in model.UrunEditList)
            {
                foreach (DataRow dataUrunSube in dataSubeUrunlist.Rows)
                {
                    var tt = modelFunctions.RTD(dataUrunSube, "Choice1_Price");
                    var ww = modelFunctions.RTS(dataUrunSube, "ChoiceProductName");
                    if (modelList.Id == modelFunctions.RTI(dataUrunSube, "ProductId") && modelList.ChoiceProductName == modelFunctions.RTS(dataUrunSube, "ChoiceProductName") && modelList.ChoicePrice != modelFunctions.RTD(dataUrunSube, "Choice1_Price"))
                    {
                        sqlData.ExecuteSql("update Choice1 set Price = @par1 Where Id = @par2", new object[] { modelList.ChoicePrice, modelList.Choice1Id });
                        break;
                    }
                }
            }

            //choice1 local db'de kaydı yoksa ana db'den data locale aktarılır.
            if (dataSubeUrunlist.Rows.Count == 0)
            {
                LocalDbProductAndOprionsAndChoice1AndChoice2Insert(model, true);
            }

            return result;
        }
        public UrunEditViewModel GetByIdForChoice2Edit(int SubeId, int productId, int Choice1Id)
        {
            var modelSube = SqlData.GetSube(SubeId);
            DataTable dataSubeUrunlist = modelFunctions.GetSubeDataWithQuery((modelFunctions.NewConnectionString(modelSube.SubeIP, modelSube.DBName, modelSube.SqlName, modelSube.SqlPassword)), SqlData.getChoice2SqlQuery(Choice1Id, productId));

            var choiceUrunList = new UrunEditViewModel();
            choiceUrunList.UrunEditList = new List<UrunEdit>();
            choiceUrunList.SubeId = SubeId;
            choiceUrunList.SubeAdi = modelSube.SubeName;

            foreach (DataRow item in dataSubeUrunlist.Rows)
            {
                UrunEdit model = new UrunEdit();
                model.Id = modelFunctions.RTI(item, "ProductId");
                model.Choice2Id = modelFunctions.RTI(item, "Id");
                model.Price = modelFunctions.RTD(item, "Product_Price");
                model.Choice2Price = modelFunctions.RTD(item, "Choice2_Price");
                model.ProductGroup = modelFunctions.RTS(item, "ProductGroup");
                model.ProductName = modelFunctions.RTS(item, "ProductName");
                model.Choice2ProductName = modelFunctions.RTS(item, "Choice2ProductName");
                choiceUrunList.UrunEditList.Add(model);
            }

            return choiceUrunList;
        }
        public ActionResultMessages Choice2Update(UrunEditViewModel model)
        {
            var result = new ActionResultMessages()
            {
                IsSuccess = true,
                UserMessage = "İşlem Başarılı",
            };

            var modelSube = SqlData.GetSube(model.SubeId);
            var sqlData = new SqlData(new SqlConnection(modelFunctions.NewConnectionString(modelSube.SubeIP, modelSube.DBName, modelSube.SqlName, modelSube.SqlPassword)));
            var dataSubeUrunlist = modelFunctions.GetSubeDataWithQuery((modelFunctions.NewConnectionString(modelSube.SubeIP, modelSube.DBName, modelSube.SqlName, modelSube.SqlPassword)), SqlData.getChoice1SqlQuery(model.UrunEditList[1].Id));

            foreach (var modelList in model.UrunEditList)
            {
                foreach (DataRow dataUrunSube in dataSubeUrunlist.Rows)
                {
                    if (modelList.Choice1Id == modelFunctions.RTI(dataUrunSube, "Id") && modelList.ChoicePrice != modelFunctions.RTD(dataUrunSube, "ChoicePrice"))
                    {
                        sqlData.ExecuteSql("update Choice1 set Price = @par1 Where Id = @par2", new object[] { modelList.ChoicePrice, modelList.Choice1Id });
                        break;
                    }
                }
            }

            return result;
        }

        #endregion Choice1,Choice2 güncelleme

        #region Options güncelleme

        public UrunEditViewModel GetByIdForOptionsEdit(int SubeId, int productId)
        {
            var modelSube = SqlData.GetSube(SubeId);
            DataTable dataSubeUrunlist = modelFunctions.GetSubeDataWithQuery((modelFunctions.NewConnectionString(modelSube.SubeIP, modelSube.DBName, modelSube.SqlName, modelSube.SqlPassword)), SqlData.getOptionsSqlQuery(productId));

            var choiceUrunList = new UrunEditViewModel();
            choiceUrunList.UrunEditList = new List<UrunEdit>();
            choiceUrunList.SubeId = SubeId;
            choiceUrunList.SubeAdi = modelSube.SubeName;

            foreach (DataRow item in dataSubeUrunlist.Rows)
            {
                UrunEdit model = new UrunEdit();
                model.Id = modelFunctions.RTI(item, "ProductId");
                model.OptionsId = modelFunctions.RTI(item, "Id");
                model.Price = modelFunctions.RTD(item, "Product_Price");
                model.OptionsPrice = modelFunctions.RTD(item, "Option_Price");
                model.ProductGroup = modelFunctions.RTS(item, "ProductGroup");
                model.ProductName = modelFunctions.RTS(item, "ProductName");
                model.OptionsProductName = modelFunctions.RTS(item, "OptionsName");
                choiceUrunList.UrunEditList.Add(model);
            }

            return choiceUrunList;
        }
        public ActionResultMessages OptionsUpdate(UrunEditViewModel model)
        {
            var result = new ActionResultMessages()
            {
                IsSuccess = true,
                UserMessage = "İşlem Başarılı",
            };

            var modelSube = SqlData.GetSube(model.SubeId);
            var sqlData = new SqlData(new SqlConnection(modelFunctions.NewConnectionString(modelSube.SubeIP, modelSube.DBName, modelSube.SqlName, modelSube.SqlPassword)));
            var dataSubeUrunlist = modelFunctions.GetSubeDataWithQuery((modelFunctions.NewConnectionString(modelSube.SubeIP, modelSube.DBName, modelSube.SqlName, modelSube.SqlPassword)), SqlData.getOptionsSqlQuery(model.UrunOptionsEditList[1].Id));

            foreach (var modelList in model.UrunOptionsEditList)
            {
                foreach (DataRow dataUrunSube in dataSubeUrunlist.Rows)
                {
                    if (modelList.OptionsId == modelFunctions.RTI(dataUrunSube, "Id") && modelList.OptionsPrice != modelFunctions.RTD(dataUrunSube, "ChoicePrice"))
                    {
                        sqlData.ExecuteSql("update Options set Price = @par1 Where Id = @par2", new object[] { modelList.OptionsPrice, modelList.OptionsId });
                        break;
                    }
                }
            }

            return result;
        }

        #endregion Options güncelleme


        #region  Şube ürün Grubu Listesi alınıyor

        public List<SelectListItem> SubeUrunGrubuListJson(int subeId)
        {
            List<SelectListItem> selectListItem = new List<SelectListItem>();

            var modelSube = SqlData.GetSube(subeId);
            var sqlData = new SqlData(new SqlConnection(modelFunctions.NewConnectionString(modelSube.SubeIP, modelSube.DBName, modelSube.SqlName, modelSube.SqlPassword)));
            var dataSubeUrunGrubuList = modelFunctions.GetSubeDataWithQuery((modelFunctions.NewConnectionString(modelSube.SubeIP, modelSube.DBName, modelSube.SqlName, modelSube.SqlPassword)), SqlData.getUrunGrubuSqlQuery());

            foreach (DataRow r in dataSubeUrunGrubuList.Rows)
            {
                selectListItem.Add(new SelectListItem
                {
                    Text = modelFunctions.RTS(r, "ProductGroup").ToString(),
                    Value = modelFunctions.RTS(r, "ProductGroup"),
                });
            }

            return selectListItem;
        }
        public List<SelectListItem> LocalSubeUrunGrubuListJson()
        {
            List<SelectListItem> selectListItem = new List<SelectListItem>();

            var dataSubeUrunGrubuList = modelFunctions.GetSubeDataWithQuery((modelFunctions.NewConnectionString(subeIp, dbName, sqlKullaniciName, sqlKullaniciPassword)), SqlData.getUrunGrubuSqlQuery());

            foreach (DataRow r in dataSubeUrunGrubuList.Rows)
            {
                selectListItem.Add(new SelectListItem
                {
                    Text = modelFunctions.RTS(r, "ProductGroup").ToString(),
                    Value = modelFunctions.RTS(r, "ProductGroup"),
                });
            }

            return selectListItem;
        }

        #endregion Şube ürün Grubu Listesi alınıyor

        #region Şubelere fiyat yayma

        public ActionResultMessages SubelereFiyatGonder(SubelereVeriGonderViewModel model)
        {
            var result = new ActionResultMessages()
            {
                IsSuccess = true,
                UserMessage = "İşlem Başarılı",
            };

            var kaynakSubeId = model.IsSelectedSubeList.Where(x => x.IsSelectedKaynakSube).FirstOrDefault().ID;
            var dataKaynakSubeUrunlist = modelFunctions.GetSubeDataWithQuery((modelFunctions.NewConnectionString(subeIp, dbName, sqlKullaniciName, sqlKullaniciPassword)), SqlData.getProductLocalDbSqlQuery(kaynakSubeId));
            model.IsSelectedSubeList = model.IsSelectedSubeList.Where(x => x.IsSelectedHedefSube).ToList();
            foreach (var item in model.IsSelectedSubeList)
            {
                var modelSube = SqlData.GetSube(item.ID);
                var dataHedefSubeUrunList = modelFunctions.GetSubeDataWithQuery(modelFunctions.NewConnectionString(modelSube.SubeIP, modelSube.DBName, modelSube.SqlName, modelSube.SqlPassword), SqlData.getProductSqlQuery());
                var sqlData = new SqlData(new SqlConnection(modelFunctions.NewConnectionString(modelSube.SubeIP, modelSube.DBName, modelSube.SqlName, modelSube.SqlPassword)));

                foreach (DataRow kaynakSube in dataKaynakSubeUrunlist.Rows)
                {
                    foreach (DataRow hedefSube in dataHedefSubeUrunList.Rows)
                    {
                        if (modelFunctions.RTS(kaynakSube, "ProductName") == modelFunctions.RTS(hedefSube, "ProductName")
                           && modelFunctions.RTS(kaynakSube, "ProductGroup") == modelFunctions.RTS(hedefSube, "ProductGroup"))
                        {
                            sqlData.ExecuteSql("Update Product set Price= @par1 Where ProductName = @par2 and ProductGroup = @par3",
                                 new object[] { modelFunctions.RTS(kaynakSube, "Price"), modelFunctions.RTS(kaynakSube, "ProductName"), modelFunctions.RTS(kaynakSube, "ProductGroup") });
                            break;
                        }
                    }
                }
            }

            return result;
        }

        #endregion Şubelere fiyat yayma

        #region Seçili şubelerdeki pruduct, choice1,choice2,options tablolarınını merkez (local temp db'ye aktarma) 

        public ActionResultMessages SubeProductInsertLocalTable(SubelereVeriGonderViewModel model)
        {
            var result = new ActionResultMessages
            {
                IsSuccess = true,
                UserMessage = "İşlem başarılı",
            };

            foreach (var isSelectedSubeItem in model.IsSelectedSubeList.Where(x => x.IsSelectedHedefSube))
            {
                var sqlData = new SqlData(new SqlConnection(modelFunctions.NewConnectionString(subeIp, dbName, sqlKullaniciName, sqlKullaniciPassword)));
                var modelSube = SqlData.GetSube(isSelectedSubeItem.ID);
                var dataSubeUrunlist = modelFunctions.GetSubeDataWithQuery((modelFunctions.NewConnectionString(modelSube.SubeIP, modelSube.DBName, modelSube.SqlName, modelSube.SqlPassword)), SqlData.getProductSqlQuery());

                foreach (DataRow item in dataSubeUrunlist.Rows)
                {
                    //Product
                    sqlData.ExecuteSql(" Insert Into  Product( ProductName, ProductGroup, Price, SubeId, SubeName, ProductPkId ) Values( @par1, @par2, @par3, @par4, @par5, @par6 )",
                        new object[]
                        {
                            modelFunctions.RTS(item, "ProductName"),
                            modelFunctions.RTS(item, "ProductGroup"),
                            modelFunctions.RTS(item, "Price"),
                            isSelectedSubeItem.ID,
                            isSelectedSubeItem.SubeName,
                            modelFunctions.RTI(item, "Id")
                        });

                    //Choice1
                    var dataSubeChoiceUrunlist = modelFunctions.GetSubeDataWithQuery((modelFunctions.NewConnectionString(modelSube.SubeIP, modelSube.DBName, modelSube.SqlName, modelSube.SqlPassword)), SqlData.getChoice1SqlQuery(modelFunctions.RTI(item, "Id")));
                    if (dataSubeChoiceUrunlist.Rows.Count > 0)
                    {
                        foreach (DataRow dataChoice1 in dataSubeChoiceUrunlist.Rows)
                        {
                            sqlData.ExecuteSql(" Insert Into  Choice1( ProductId, Name, Price, SubeId, SubeName, Choice1PkId ) Values( @par1, @par2, @par3, @par4, @par5, @par6 )",
                            new object[]
                            {
                                modelFunctions.RTI(dataChoice1, "ProductId"),
                                modelFunctions.RTS(dataChoice1, "ChoiceProductName"),
                                modelFunctions.RTD(dataChoice1, "Choice1_Price"),
                                isSelectedSubeItem.ID,
                                isSelectedSubeItem.SubeName,
                                modelFunctions.RTI(dataChoice1, "Id")
                            });

                            //Choice2
                            var dataSubeChoice2Urunlist = modelFunctions.GetSubeDataWithQuery((modelFunctions.NewConnectionString(modelSube.SubeIP, modelSube.DBName, modelSube.SqlName, modelSube.SqlPassword)),
                                                                                                                                        SqlData.getChoice2SqlQuery(modelFunctions.RTI(dataChoice1, "Id"), modelFunctions.RTI(item, "Id")));
                            if (dataSubeChoice2Urunlist.Rows.Count > 0)
                            {
                                foreach (DataRow dataChoice2 in dataSubeChoice2Urunlist.Rows)
                                {
                                    sqlData.ExecuteSql(" Insert Into  Choice2( ProductId, Choice1Id, Name, Price, SubeId, SubeName, Choice2PkId ) Values( @par1, @par2, @par3, @par4, @par5, @par6 )",
                                    new object[]
                                    {
                                        modelFunctions.RTI(dataChoice2, "ProductId"),
                                        modelFunctions.RTI(dataChoice1, "Id"),
                                        modelFunctions.RTS(dataChoice2, "Choice2ProductName"),
                                        modelFunctions.RTD(dataChoice2, "Choice2_Price"),
                                        isSelectedSubeItem.ID,
                                        isSelectedSubeItem.SubeName,
                                        modelFunctions.RTI(dataChoice2, "Id")
                                    });
                                }
                            }
                            //Choice2
                        }
                    }

                    //Options
                    var dataSubeUrunOptionslist = modelFunctions.GetSubeDataWithQuery((modelFunctions.NewConnectionString(modelSube.SubeIP, modelSube.DBName, modelSube.SqlName, modelSube.SqlPassword)), SqlData.getOptionsSqlQuery(modelFunctions.RTI(item, "Id")));
                    if (dataSubeUrunOptionslist.Rows.Count > 0)
                    {
                        foreach (DataRow dataOptions in dataSubeUrunOptionslist.Rows)
                        {
                            sqlData.ExecuteSql(" Insert Into  Options(ProductId, Name, Price, Category, SubeId, SubeName, OptionsPkId ) Values( @par1, @par2, @par3, @par4, @par5, @par6, @par7)",
                            new object[]
                            {
                                modelFunctions.RTI(dataOptions, "ProductId"),
                                modelFunctions.RTS(dataOptions, "OptionsName"),
                                modelFunctions.RTD(dataOptions, "Option_Price"),
                                modelFunctions.RTD(dataOptions, "OptionsCategory"),
                                isSelectedSubeItem.ID,
                                isSelectedSubeItem.SubeName,
                                modelFunctions.RTI(dataOptions, "Id")
                            });
                        }
                    }
                }
            }

            return result;
        }
        public UrunEditViewModel GetByProductForSubeListEdit(string productGroup_, string SubeIdGrupList)
        {
            var result = new ActionResultMessages
            {
                IsSuccess = true,
                UserMessage = "İşlem başarılı",
            };

            #region Local database'e veriler kaydetmek için kullanıldı.

            var sqlData = new SqlData(new SqlConnection(modelFunctions.NewConnectionString(subeIp, dbName, sqlKullaniciName, sqlKullaniciPassword)));
            DataTable dataSubeUrunlist = modelFunctions.GetSubeDataWithQuery((modelFunctions.NewConnectionString(subeIp, dbName, sqlKullaniciName, sqlKullaniciPassword)),
                productGroup_ == null || productGroup_ == "null" ? SqlData.getLocalProductListSqlQuery(SubeIdGrupList) : SqlData.getProductForProductGroupSqlQuery(productGroup_));

            #endregion Local database'e veriler kaydetmek için kullanıldı.

            var returnViewModel = new UrunEditViewModel() { };
            returnViewModel.SubeList = new List<ProductCompairSube>();
            returnViewModel.productCompairsList = new List<ProductCompair>();

            var subeIdList = new List<long>();
            foreach (DataRow item in dataSubeUrunlist.Rows)
            {
                subeIdList.Add(modelFunctions.RTI(item, "SubeId"));
            }

            subeIdList = subeIdList.Distinct().ToList();

            foreach (var sube in subeIdList)
            {
                var drSube = dataSubeUrunlist.Select("SubeId=" + sube)[0];
                var subeName = modelFunctions.RTS(drSube, "SubeName");
                returnViewModel.SubeList.Add(new ProductCompairSube
                {
                    SubeId = sube,
                    SubeAdi = subeName,
                });
            }

            foreach (DataRow item in dataSubeUrunlist.Rows)
            {
                var productGroup = modelFunctions.RTS(item, "ProductGroup");
                var productName = modelFunctions.RTS(item, "ProductName");
                var subeName = modelFunctions.RTS(item, "SubeName");
                var subePrice = modelFunctions.RTD(item, "Price");
                var subeId = modelFunctions.RTI(item, "SubeId");
                var productId = modelFunctions.RTI(item, "Id");

                var pc = returnViewModel.productCompairsList.Where(x => x.ProductName == productName && x.ProductGroup == productGroup).FirstOrDefault();

                if (pc == null)
                {
                    pc = new ProductCompair();
                    pc.ProductGroup = productGroup;
                    pc.ProductName = productName;
                    pc.ProductId = productId;

                    returnViewModel.productCompairsList.Add(pc);
                    pc.SubeList = new List<UrunEdit2>();
                }

                var urunEdit2 = new UrunEdit2()
                {
                    SubeId = subeId,
                    SubePriceValue = subePrice
                };

                pc.SubeList.Add(urunEdit2);
            }

            foreach (var sube in returnViewModel.SubeList)
            {
                foreach (var product in returnViewModel.productCompairsList)
                {
                    if (!product.SubeList.Where(x => x.SubeId == sube.SubeId).Any())
                    {
                        product.SubeList.Add(new UrunEdit2 { SubeId = sube.SubeId });
                    }
                }
            }


            //var returnViewModel = new UrunEditViewModel()
            //{
            //    productCompairsList = sonuc,
            //    getSubeList = sonuc.SelectMany(x => x.SubeList.Select(y => y.SubeAdi)).Distinct().ToList(),
            //};

            return returnViewModel;
        }
        public ActionResultMessages UpdateByProductForSubeList(UrunEditViewModel model)
        {
            var result = new ActionResultMessages()
            {
                IsSuccess = true,
                UserMessage = "İşlem Başarılı",
            };


            var kullaniciData = BussinessHelper.GetByUserInfoForId("", "", model.KullaniciId);

            var modelSube = SqlData.GetSube(model.SubeId);

            #region Local database'e veriler kaydetmek için kullanıldı.

            var sqlData = new SqlData(new SqlConnection(modelFunctions.NewConnectionString(subeIp, dbName, sqlKullaniciName, sqlKullaniciPassword)));
            var dataSubeUrunlist = modelFunctions.GetSubeDataWithQuery((modelFunctions.NewConnectionString(subeIp, dbName, sqlKullaniciName, sqlKullaniciPassword)), SqlData.getProductLocalDbForInsertSqlQuery());

            #endregion Local database'e veriler kaydetmek için kullanıldı.

            foreach (var modelProduct in model.productCompairsList)
            {
                foreach (DataRow dataUrunSube in dataSubeUrunlist.Rows)
                {
                    foreach (var price in modelProduct.SubeList)
                    {
                        if (modelProduct.ProductName == modelFunctions.RTS(dataUrunSube, "ProductName")
                             && modelProduct.ProductGroup == modelFunctions.RTS(dataUrunSube, "ProductGroup")
                             && price.SubeId == modelFunctions.RTD(dataUrunSube, "SubeId")
                             && price.SubePriceValue != modelFunctions.RTD(dataUrunSube, "Price")
                            )
                        {
                            sqlData.ExecuteSql("update Product set Price = @par1 Where SubeId = @par2 and ProductGroup = @par3 and  ProductName = @par4",
                             new object[]
                                  {
                                    price.SubePriceValue,
                                    price.SubeId,
                                    modelProduct.ProductGroup,
                                    modelProduct.ProductName
                                 });

                            sqlData.ExecuteSql(" Insert Into  ProductTarihce( ProductName, ProductGroup, Price, SubeId,SubeName, ProductPkId, IsUpdateDate,IsUpdateKullanici ) Values( @par1, @par2, @par3, @par4, @par5, @par6, @par7, @par8 )",
                          new object[] {
                              modelFunctions.RTS(dataUrunSube, "ProductName"),
                              modelFunctions.RTS(dataUrunSube, "ProductGroup"),
                              modelFunctions.RTS(dataUrunSube, "Price"),
                              price.SubeId,
                              model.SubeList.Where(x=>x.SubeId==price.SubeId).FirstOrDefault().SubeAdi,
                              modelFunctions.RTI(dataUrunSube, "Id"),
                              DateTime.Now.Date.ToString("dd'/'MM'/'yyyy HH:mm"),
                              kullaniciData.UserName
                          });
                            break;
                        }
                    }
                }
            }

            return result;
        }

        #region Choice1 update

        public UrunEditViewModel GetByChoice1ForSubeListEdit(string subeId_, string productId_)
        {
            var result = new ActionResultMessages { IsSuccess = true, UserMessage = "İşlem başarılı", };

            #region Choice1 listesi

            #region Local database'e veriler kaydetmek için kullanıldı.
            var sqlData = new SqlData(new SqlConnection(modelFunctions.NewConnectionString(subeIp, dbName, sqlKullaniciName, sqlKullaniciPassword)));
            DataTable dataSubeUrunlist = modelFunctions.GetSubeDataWithQuery((modelFunctions.NewConnectionString(subeIp, dbName, sqlKullaniciName, sqlKullaniciPassword)), SqlData.getLocalChoice1tListSqlQuery(subeId_, productId_,""));
            #endregion Local database'e veriler kaydetmek için kullanıldı.

            var returnViewModel = new UrunEditViewModel() { };
            returnViewModel.SubeList = new List<ProductCompairSube>();
            returnViewModel.productCompairsList = new List<ProductCompair>();

            var subeIdList = new List<long>();
            foreach (DataRow item in dataSubeUrunlist.Rows)
            {
                subeIdList.Add(modelFunctions.RTI(item, "SubeId"));
            }
            subeIdList = subeIdList.Distinct().ToList();

            foreach (var sube in subeIdList)
            {
                var drSube = dataSubeUrunlist.Select("SubeId=" + sube)[0];
                var subeName = modelFunctions.RTS(drSube, "SubeName");
                returnViewModel.SubeList.Add(new ProductCompairSube
                {
                    SubeId = sube,
                    SubeAdi = subeName,
                });
            }

            foreach (DataRow item in dataSubeUrunlist.Rows)
            {
                var productGroup = modelFunctions.RTS(item, "ProductGroup");
                var productName = modelFunctions.RTS(item, "ProductName");
                var subeName = modelFunctions.RTS(item, "SubeName");
                var subeId = modelFunctions.RTI(item, "SubeId");
                var choice1Id = modelFunctions.RTI(item, "Id");
                var choice1PkId = modelFunctions.RTI(item, "Choice1PkId");
                var id = modelFunctions.RTI(item, "PId");
                var productId = modelFunctions.RTI(item, "ProductId");
                var price = modelFunctions.RTD(item, "Product_Price");
                var choicePrice = modelFunctions.RTD(item, "Choice1_Price");
                var choiceProductName = modelFunctions.RTS(item, "ChoiceProductName");

                var pc = returnViewModel.productCompairsList.Where(x => x.ProductName == productName && x.ProductGroup == productGroup && x.ChoiceProductName == choiceProductName).FirstOrDefault();
                if (pc == null)
                {
                    pc = new ProductCompair();
                    pc.ProductGroup = productGroup;
                    pc.ProductName = productName;
                    pc.ProductId = productId;
                    pc.Choice1Id = choice1Id;
                    pc.Choice1PkId = choice1PkId;
                    pc.ChoicePrice = choicePrice;
                    pc.ChoiceProductName = choiceProductName;
                    returnViewModel.productCompairsList.Add(pc);
                    pc.SubeList = new List<UrunEdit2>();
                }

                var urunEdit2 = new UrunEdit2()
                {
                    SubeId = subeId,
                    SubePriceValue = choicePrice
                };

                pc.SubeList.Add(urunEdit2);
            }

            foreach (var sube in returnViewModel.SubeList)
            {
                foreach (var product in returnViewModel.productCompairsList)
                {
                    if (!product.SubeList.Where(x => x.SubeId == sube.SubeId).Any())
                    {
                        product.SubeList.Add(new UrunEdit2 { SubeId = sube.SubeId });
                    }
                }
            }
            #endregion Choice1 listesi

            #region Options listesi

            #region Local database'e veriler kaydetmek için kullanıldı.
            DataTable dataSubeUrunOptionslist = modelFunctions.GetSubeDataWithQuery((modelFunctions.NewConnectionString(subeIp, dbName, sqlKullaniciName, sqlKullaniciPassword)), SqlData.getLocalOptionstListSqlQuery(subeId_, productId_,""));
            #endregion Local database'e veriler kaydetmek için kullanıldı.

            returnViewModel.productOptionsCompairsList = new List<ProductCompair>();
            foreach (DataRow item in dataSubeUrunOptionslist.Rows)
            {
                var productGroup = modelFunctions.RTS(item, "ProductGroup");
                var productName = modelFunctions.RTS(item, "ProductName");
                var subeName = modelFunctions.RTS(item, "SubeName");
                var subeId = modelFunctions.RTI(item, "SubeId");
                var optionsId = modelFunctions.RTI(item, "Id");
                var id = modelFunctions.RTI(item, "PId");
                var productId = modelFunctions.RTI(item, "ProductId");
                var optionsPrice = modelFunctions.RTD(item, "Option_Price");
                var optionsName = modelFunctions.RTS(item, "OptionsName");

                var pc = returnViewModel.productOptionsCompairsList.Where(x => x.ProductName == productName && x.ProductGroup == productGroup && x.OptionsProductName == optionsName).FirstOrDefault();
                if (pc == null)
                {
                    pc = new ProductCompair();
                    pc.ProductGroup = productGroup;
                    pc.ProductName = productName;
                    pc.ProductId = productId;
                    pc.OptionsId = optionsId;
                    pc.OptionsPrice = optionsPrice;
                    pc.OptionsProductName = optionsName;
                    returnViewModel.productOptionsCompairsList.Add(pc);
                    pc.SubeList = new List<UrunEdit2>();
                }

                var urunEdit2 = new UrunEdit2()
                {
                    SubeId = subeId,
                    SubePriceValue = optionsPrice
                };

                pc.SubeList.Add(urunEdit2);
            }

            foreach (var sube in returnViewModel.SubeList)
            {
                foreach (var product in returnViewModel.productOptionsCompairsList)
                {
                    if (!product.SubeList.Where(x => x.SubeId == sube.SubeId).Any())
                    {
                        product.SubeList.Add(new UrunEdit2 { SubeId = sube.SubeId });
                    }
                }
            }

            #endregion Options listesi

            return returnViewModel;
        }
        public ActionResultMessages UpdateByChoice1ForSubeList(UrunEditViewModel model)
        {
            var result = new ActionResultMessages()
            {
                IsSuccess = true,
                UserMessage = "İşlem Başarılı",
            };

            var kullaniciData = BussinessHelper.GetByUserInfoForId("", "", model.KullaniciId);

            #region Local database'e veriler kaydetmek için kullanıldı.

            var sqlData = new SqlData(new SqlConnection(modelFunctions.NewConnectionString(subeIp, dbName, sqlKullaniciName, sqlKullaniciPassword)));
            var dataSubeUrunlist = modelFunctions.GetSubeDataWithQuery(modelFunctions.NewConnectionString(subeIp, dbName, sqlKullaniciName, sqlKullaniciPassword),
                                   SqlData.getLocalChoice1ListSqlQuery(model.productCompairsList.FirstOrDefault().ProductGroup, model.productCompairsList.FirstOrDefault().ProductName));

            #endregion Local database'e veriler kaydetmek için kullanıldı.

            foreach (var modelProduct in model.productCompairsList)
            {
                foreach (DataRow dataUrunSube in dataSubeUrunlist.Rows)
                {
                    foreach (var price in modelProduct.SubeList)
                    {
                        var pn = modelFunctions.RTS(dataUrunSube, "ProductName");
                        var pn1 = modelFunctions.RTS(dataUrunSube, "ProductGroup");
                        var pn3 = modelFunctions.RTS(dataUrunSube, "ChoiceProductName");
                        var pn4 = modelFunctions.RTS(dataUrunSube, "SubeId");
                        var pn5 = modelFunctions.RTS(dataUrunSube, "Choice1_Price");
                        var pn6 = modelFunctions.RTS(dataUrunSube, "ProductId");

                        if (modelProduct.ProductName == modelFunctions.RTS(dataUrunSube, "ProductName")
                             && modelProduct.ProductGroup == modelFunctions.RTS(dataUrunSube, "ProductGroup")
                             && modelProduct.ChoiceProductName == modelFunctions.RTS(dataUrunSube, "ChoiceProductName")
                             && price.SubeId == modelFunctions.RTD(dataUrunSube, "SubeId")
                             && price.SubePriceValue != modelFunctions.RTD(dataUrunSube, "Choice1_Price")
                            )
                        {
                            sqlData.ExecuteSql("Update choice1 set Price = @par1 Where Name = @par2 and SubeId=@par3 and ProductId=@par4",
                             new object[]
                                  {
                                    price.SubePriceValue,
                                    modelProduct.ChoiceProductName,
                                    price.SubeId,
                                    modelProduct.ProductId
                                 });

                            sqlData.ExecuteSql(" Insert Into  Choice1Tarihce( ProductId, Name, Price, SubeId, SubeName, Choice1PkId,IsUpdateDate,IsUpdateKullanici ) Values( @par1, @par2, @par3, @par4, @par5,@par6,@par7,@par8 )",
                                 new object[]
                                 {
                                     modelFunctions.RTI(dataUrunSube, "ProductId"),
                                     modelFunctions.RTS(dataUrunSube, "ChoiceProductName"),
                                     modelFunctions.RTD(dataUrunSube, "Choice1_Price"),
                                     model.SubeId,
                                     model.SubeList.Where(x=>x.SubeId==price.SubeId).FirstOrDefault().SubeAdi,
                                     modelFunctions.RTI(dataUrunSube, "Id"),
                                     DateTime.Now.Date.ToString("dd'/'MM'/'yyyy HH:mm"),
                                     kullaniciData.UserName
                                 });
                            break;
                        }
                    }
                }
            }

            return result;
        }

        public ActionResultMessages UpdateByOptionsForSubeList(UrunEditViewModel model)
        {
            var result = new ActionResultMessages()
            {
                IsSuccess = true,
                UserMessage = "İşlem Başarılı",
            };

            var kullaniciData = BussinessHelper.GetByUserInfoForId("", "", model.KullaniciId);

            #region Local database'e veriler kaydetmek için kullanıldı.
            var productId = model.productOptionsCompairsList.Select(x => x.ProductId).FirstOrDefault();

            var sqlData = new SqlData(new SqlConnection(modelFunctions.NewConnectionString(subeIp, dbName, sqlKullaniciName, sqlKullaniciPassword)));
            var dataSubeUrunlist = modelFunctions.GetSubeDataWithQuery((modelFunctions.NewConnectionString(subeIp, dbName, sqlKullaniciName, sqlKullaniciPassword)), SqlData.getLocalOptionsListSqlQuery(productId));

            #endregion Local database'e veriler kaydetmek için kullanıldı.

            foreach (var modelProduct in model.productOptionsCompairsList)
            {
                foreach (DataRow dataUrunSube in dataSubeUrunlist.Rows)
                {
                    foreach (var price in modelProduct.SubeList)
                    {
                        var pn = modelFunctions.RTS(dataUrunSube, "ProductName");
                        var pn1 = modelFunctions.RTS(dataUrunSube, "ProductGroup");
                        var pn3 = modelFunctions.RTS(dataUrunSube, "OptionsName");
                        var pn4 = modelFunctions.RTS(dataUrunSube, "SubeId");
                        var pn5 = modelFunctions.RTS(dataUrunSube, "Option_Price");
                        var pn6 = modelFunctions.RTS(dataUrunSube, "ProductId");

                        if (modelProduct.ProductName == modelFunctions.RTS(dataUrunSube, "ProductName")
                             && modelProduct.ProductGroup == modelFunctions.RTS(dataUrunSube, "ProductGroup")
                             && modelProduct.OptionsProductName == modelFunctions.RTS(dataUrunSube, "OptionsName")
                             && price.SubeId == modelFunctions.RTD(dataUrunSube, "SubeId")
                             && price.SubePriceValue != modelFunctions.RTD(dataUrunSube, "Option_Price")
                            )
                        {
                            sqlData.ExecuteSql("Update Options set Price = @par1 Where Name = @par2 and SubeId=@par3 and ProductId=@par4",
                             new object[]
                                  {
                                    price.SubePriceValue,
                                    modelProduct.OptionsProductName,
                                    price.SubeId,
                                    modelProduct.ProductId
                                 });

                            sqlData.ExecuteSql(" Insert Into  OptionsTarihce(ProductId, Name, Price, Category, SubeId, SubeName, OptionsPkId, IsUpdateDate,IsUpdateKullanici ) Values( @par1, @par2, @par3, @par4, @par5, @par6,@par7,@par8,@par9)",
                               new object[]
                               {
                                   modelFunctions.RTI(dataUrunSube, "ProductId"),
                                   modelFunctions.RTS(dataUrunSube, "OptionsName"),
                                   modelFunctions.RTD(dataUrunSube, "Option_Price"),
                                   modelFunctions.RTD(dataUrunSube, "OptionsCategory"),
                                   model.SubeId,
                                   model.SubeList.Where(x=>x.SubeId==price.SubeId).FirstOrDefault().SubeAdi,
                                   modelFunctions.RTI(dataUrunSube, "Id"),
                                   DateTime.Now.Date.ToString("dd'/'MM'/'yyyy HH:mm"),
                                  kullaniciData.UserName
                               });
                            break;
                        }
                    }
                }
            }

            return result;
        }

        #endregion Choice1 update

        #region Choice2 update

        public UrunEditViewModel GetByChoice2ForSubeListEdit(int subeId_, int choice1Id_)
        {
            var result = new ActionResultMessages { IsSuccess = true, UserMessage = "İşlem başarılı", };

            #region Choice1 listesi

            #region Local database'e veriler kaydetmek için kullanıldı.
            var sqlData = new SqlData(new SqlConnection(modelFunctions.NewConnectionString(subeIp, dbName, sqlKullaniciName, sqlKullaniciPassword)));
            DataTable dataSubeUrunlist = modelFunctions.GetSubeDataWithQuery((modelFunctions.NewConnectionString(subeIp, dbName, sqlKullaniciName, sqlKullaniciPassword)), SqlData.GetLocalChoice2tListSqlQuery(subeId_, choice1Id_,"",""));
            #endregion Local database'e veriler kaydetmek için kullanıldı.

            var returnViewModel = new UrunEditViewModel() { };
            returnViewModel.SubeList = new List<ProductCompairSube>();
            returnViewModel.productCompairsList = new List<ProductCompair>();

            var subeIdList = new List<long>();
            foreach (DataRow item in dataSubeUrunlist.Rows)
            {
                subeIdList.Add(modelFunctions.RTI(item, "SubeId"));
            }
            subeIdList = subeIdList.Distinct().ToList();

            foreach (var sube in subeIdList)
            {
                var drSube = dataSubeUrunlist.Select("SubeId=" + sube)[0];
                var subeName = modelFunctions.RTS(drSube, "SubeName");
                returnViewModel.SubeList.Add(new ProductCompairSube
                {
                    SubeId = sube,
                    SubeAdi = subeName,
                });
            }

            foreach (DataRow item in dataSubeUrunlist.Rows)
            {
                var productGroup = modelFunctions.RTS(item, "ProductGroup");
                var productName = modelFunctions.RTS(item, "ProductName");
                var subeName = modelFunctions.RTS(item, "SubeName");
                var subeId = modelFunctions.RTI(item, "SubeId");
                var choice1Id = modelFunctions.RTI(item, "Id");
                var id = modelFunctions.RTI(item, "PId");
                var productId = modelFunctions.RTI(item, "ProductId");
                var price = modelFunctions.RTD(item, "Product_Price");
                var choicePrice = modelFunctions.RTD(item, "Choice2_Price");
                var choiceProductName = modelFunctions.RTS(item, "ChoiceProductName");

                var pc = returnViewModel.productCompairsList.Where(x => x.ProductName == productName && x.ProductGroup == productGroup && x.ChoiceProductName == choiceProductName).FirstOrDefault();
                if (pc == null)
                {
                    pc = new ProductCompair();
                    pc.ProductGroup = productGroup;
                    pc.ProductName = productName;
                    pc.ProductId = productId;
                    pc.Choice1Id = choice1Id;
                    pc.ChoicePrice = choicePrice;
                    pc.ChoiceProductName = choiceProductName;
                    returnViewModel.productCompairsList.Add(pc);
                    pc.SubeList = new List<UrunEdit2>();
                }

                var urunEdit2 = new UrunEdit2()
                {
                    SubeId = subeId,
                    SubePriceValue = choicePrice
                };

                pc.SubeList.Add(urunEdit2);
            }

            foreach (var sube in returnViewModel.SubeList)
            {
                foreach (var product in returnViewModel.productCompairsList)
                {
                    if (!product.SubeList.Where(x => x.SubeId == sube.SubeId).Any())
                    {
                        product.SubeList.Add(new UrunEdit2 { SubeId = sube.SubeId });
                    }
                }
            }
            #endregion Choice1 listesi

            return returnViewModel;
        }
        public ActionResultMessages UpdateByChoice2ForSubeList(UrunEditViewModel model)
        {
            var result = new ActionResultMessages()
            {
                IsSuccess = true,
                UserMessage = "İşlem Başarılı",
            };

            #region Local database'e veriler kaydetmek için kullanıldı.

            var sqlData = new SqlData(new SqlConnection(modelFunctions.NewConnectionString(subeIp, dbName, sqlKullaniciName, sqlKullaniciPassword)));
            var dataSubeUrunlist = modelFunctions.GetSubeDataWithQuery((modelFunctions.NewConnectionString(subeIp, dbName, sqlKullaniciName, sqlKullaniciPassword)), SqlData.getLocalChoice2ListSqlQuery());

            #endregion Local database'e veriler kaydetmek için kullanıldı.

            foreach (var modelProduct in model.productCompairsList)
            {
                foreach (DataRow dataUrunSube in dataSubeUrunlist.Rows)
                {
                    foreach (var price in modelProduct.SubeList)
                    {
                        var pn = modelFunctions.RTS(dataUrunSube, "ProductName");
                        var pn1 = modelFunctions.RTS(dataUrunSube, "ProductGroup");
                        var pn3 = modelFunctions.RTS(dataUrunSube, "ChoiceProductName");
                        var pn4 = modelFunctions.RTS(dataUrunSube, "SubeId");
                        var pn5 = modelFunctions.RTS(dataUrunSube, "Choice2_Price");
                        var pn6 = modelFunctions.RTS(dataUrunSube, "ProductId");

                        if (modelProduct.ProductName == modelFunctions.RTS(dataUrunSube, "ProductName")
                             && modelProduct.ProductGroup == modelFunctions.RTS(dataUrunSube, "ProductGroup")
                             && modelProduct.ChoiceProductName == modelFunctions.RTS(dataUrunSube, "ChoiceProductName")
                             && price.SubeId == modelFunctions.RTD(dataUrunSube, "SubeId")
                             && price.SubePriceValue != modelFunctions.RTD(dataUrunSube, "Choice1_Price")
                            )
                        {
                            sqlData.ExecuteSql("Update choice2 set Price = @par1 Where Name = @par2 and SubeId=@par3 and ProductId=@par4",
                             new object[]
                                  {
                                    price.SubePriceValue,
                                    modelProduct.ChoiceProductName,
                                    price.SubeId,
                                    modelProduct.ProductId
                                 });

                            sqlData.ExecuteSql(" Insert Into  Choice2Tarihce( ProductId, Choice1Id, Name, Price, SubeId,SubeName, Choice2PkId, IsUpdateDate,IsUpdateKullanici ) Values( @par1, @par2, @par3, @par4, @par5,@par6 )",
                                      new object[]
                                      {
                                          modelFunctions.RTI(dataUrunSube, "ProductId"),
                                          modelFunctions.RTI(dataUrunSube, "Id"),
                                          modelFunctions.RTS(dataUrunSube, "ChoiceProductName"),
                                          modelFunctions.RTD(dataUrunSube, "Choice2_Price"),
                                          model.SubeId,
                                          model.SubeList.Where(x=>x.SubeId==price.SubeId).FirstOrDefault().SubeAdi,
                                          modelFunctions.RTI(dataUrunSube, "Id")
                                      });

                            break;
                        }
                    }
                }
            }

            return result;
        }


        #endregion Choice2 update




        #region Tarihçe Insert


        //public ActionResultMessages ProductTarihceInsert(UrunEditViewModel model, bool choiceMu)
        //{
        //    var result = new ActionResultMessages
        //    {
        //        IsSuccess = true,
        //        UserMessage = "İşlem başarılı",
        //    };

        //    foreach (DataRow item in dataSubeUrunlist.Rows)
        //    {
        //        //Product
        //        sqlData.executeSql(" Insert Into  Product( ProductName, ProductGroup, Price, SubeId, ProductPkId ) Values( @par1, @par2, @par3, @par4, @par5 )",
        //            new object[] { modelFunctions.RTS(item, "ProductName"), modelFunctions.RTS(item, "ProductGroup"), modelFunctions.RTS(item, "Price"), model.SubeId, modelFunctions.RTI(item, "Id") });

        //    }

        //    return result;
        //}



        #endregion Tarihçe Insert

        public ActionResultMessages IsUpdateProduct()
        {
            var result = new ActionResultMessages()
            {
                IsSuccess = true,
                UserMessage = "İşlem Başarılı",
            };

            #region Product tablosunda Isupdate true yapar

            var sqlData = new SqlData(new SqlConnection(modelFunctions.NewConnectionString(subeIp, dbName, sqlKullaniciName, sqlKullaniciPassword)));
            //var dataSubeUrunlist = modelFunctions.GetSubeDataWithQuery((modelFunctions.NewConnectionString(subeIp, dbName, sqlKullaniciName, sqlKullaniciPassword)), SqlData.getProductLocalDbForInsertSqlQuery());
            sqlData.ExecuteSql("update Product set IsUpdate = @par1", new object[] { true });
            sqlData.ExecuteSql("update Choice1 set IsUpdate = @par1", new object[] { true });
            sqlData.ExecuteSql("update Choice2 set IsUpdate = @par1", new object[] { true });
            sqlData.ExecuteSql("update Options set IsUpdate = @par1", new object[] { true });

            #endregion Product tablosunda Isupdate true yapar

            #region Product tablosunda Delete  yapar

            sqlData.ExecuteSql("Delete Product ", new object[] { });
            sqlData.ExecuteSql("Delete Choice1 ", new object[] { });
            sqlData.ExecuteSql("Delete Choice2 ", new object[] { });
            sqlData.ExecuteSql("Delete Options ", new object[] { });

            #endregion Product tablosunda Delete yapar

            return result;
        }

        #endregion Seçili şubelerdeki pruduct, choice1,choice2,options tablolarınını merkez (local temp db'ye aktarma

        #region MyRegion
        private static List<T> ConvertDataTable<T>(DataTable dt)
        {
            List<T> data = new List<T>();
            foreach (DataRow row in dt.Rows)
            {
                T item = GetItem<T>(row);
                data.Add(item);
            }
            return data;
        }
        private static T GetItem<T>(DataRow dr)
        {
            Type temp = typeof(T);
            T obj = Activator.CreateInstance<T>();

            foreach (DataColumn column in dr.Table.Columns)
            {
                foreach (PropertyInfo pro in temp.GetProperties())
                {
                    if (pro.Name == column.ColumnName)
                        pro.SetValue(obj, dr[column.ColumnName], null);
                    else
                        continue;
                }
            }
            return obj;
        }
        //var pivotTable = data.ToPivotTable(
        //      item => item.Year,
        //      item => item.Product,
        //      items => items.Any() ? items.Sum(x => x.Sales) : 0);
        //public static DataTable ToPivotTable<T,TColumn,TRow,TData>(this IEnumerable<T> source, Func<T, TColumn> columnSelector, Expression<Func<T, TRow>> rowSelector, Func<IEnumerable<T>, TData> dataSelector)
        //{
        //    DataTable table = new DataTable();
        //    var rowName = ((MemberExpression)rowSelector.Body).Member.Name;
        //    table.Columns.Add(new DataColumn(rowName));
        //    var columns = source.Select(columnSelector).Distinct();

        //    foreach (var column in columns)
        //        table.Columns.Add(new DataColumn(column.ToString()));

        //    var rows = source.GroupBy(rowSelector.Compile())
        //                     .Select(rowGroup => new
        //                     {
        //                         Key = rowGroup.Key,
        //                         Values = columns.GroupJoin(
        //                             rowGroup,
        //                             c => c,
        //                             r => columnSelector(r),
        //                             (c, columnGroup) => dataSelector(columnGroup))
        //                     });

        //    foreach (var row in rows)
        //    {
        //        var dataRow = table.NewRow();
        //        var items = row.Values.Cast<object>().ToList();
        //        items.Insert(0, row.Key);
        //        dataRow.ItemArray = items.ToArray();
        //        table.Rows.Add(dataRow);
        //    }

        //    return table;
        //}
        #endregion
    }
}