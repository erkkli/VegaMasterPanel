using SefimV2.Helper;
using SefimV2.ViewModels.SPosVeriGonderimi;
using SefimV2.ViewModels.SubeSettings;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Web.Configuration;
using System.Web.Mvc;
using static SefimV2.ViewModels.SPosVeriGonderimi.UrunEditViewModel;

namespace SefimV2.Models
{
    public class SPosSubeFiyatGuncellemeCRUD
    {
        ModelFunctions mF = new ModelFunctions();
        #region Config local copy db connction setting       
        static readonly string subeIp = WebConfigurationManager.AppSettings["Server"];
        static readonly string dbName = WebConfigurationManager.AppSettings["DBName"];
        static readonly string sqlKullaniciName = WebConfigurationManager.AppSettings["User"];
        static readonly string sqlKullaniciPassword = WebConfigurationManager.AppSettings["Password"];
        #endregion

        #region Şube listesi

        public List<SubeSettingsViewModel> GetSubeList()
        {
            var liste = new List<SubeSettingsViewModel>();

            mF.SqlConnOpen();
            var dt = mF.DataTable("Select Id, SubeName,DBName, * From SubeSettings Where AppDbTypeStatus in (0,1) and Status=1");
            mF.SqlConnClose();

            foreach (DataRow r in dt.Rows)
            {
                SubeSettingsViewModel model = new SubeSettingsViewModel
                {
                    ID = Convert.ToInt32(mF.RTS(r, "Id")),
                    SubeName = (mF.RTS(r, "SubeName")),
                    DBName = mF.RTS(r, "DBName")

                    //model.SubeIP = modelFunctions.RTS(r, "SubeIP");
                    //model.SqlName = modelFunctions.RTS(r, "SqlName");
                    //model.SqlPassword = modelFunctions.RTS(r, "SqlPassword");
                    //model.FirmaID = modelFunctions.RTS(r, "FirmaID");                
                    //model.Status = Convert.ToBoolean(modelFunctions.RTS(r, "Status"));
                    //model.AppDbType = Convert.ToInt32(modelFunctions.RTS(r, "AppDbType"));
                    //model.AppDbTypeStatus = Convert.ToBoolean(modelFunctions.RTS(r, "AppDbTypeStatus"));
                };

                liste.Add(model);
            }

            return liste;
        }
        public SubelereVeriGonderViewModel GetSubeListIsSelected()
        {
            var liste = new SubelereVeriGonderViewModel
            {
                IsSelectedSubeList = new List<SubeSettingsViewModel>()
            };

            mF.SqlConnOpen();
            var dt = mF.DataTable("Select Id, SubeName,DBName, * From SubeSettings Where AppDbTypeStatus in (0,1) and Status=1 ");
            mF.SqlConnClose();

            foreach (DataRow r in dt.Rows)
            {
                SubeSettingsViewModel model = new SubeSettingsViewModel
                {
                    ID = Convert.ToInt32(mF.RTS(r, "Id")),
                    SubeName = (mF.RTS(r, "SubeName")),
                    DBName = mF.RTS(r, "DBName")
                };
                liste.IsSelectedSubeList.Add(model);
            }

            return liste;
        }
        public SubelereVeriGonderViewModel GetLocalSubeListIsSelectedRemovePreviousList(string kullaniciId)
        {
            #region Local pruduct tablosundaki şubeler alınır 
            // Temp Pruduct tablosuna eklenen subeleri çeker güncellemeye gidecek şube listesine dahil edilmez.

            /*Önceki Gruplama*/
            // var sqlQuery = "Select SubeId, SubeName from Product group by SubeName, SubeId";
            mF.SqlConnOpen();
            var sqlQuery = "Select GuncellenecekSubeAdiGrubu, GuncellenecekSubeIdGrubu from Product Where YeniUrunMu=0 group by GuncellenecekSubeAdiGrubu,GuncellenecekSubeIdGrubu";
            var localSubeList = mF.DataTable(sqlQuery);
            mF.SqlConnClose();

            var listeLocal = new SubelereVeriGonderViewModel
            {
                IsSelectedSubeList = new List<SubeSettingsViewModel>(),
                GuncellenecekSubeGruplariList = new List<GuncellenecekSubeGruplari>()
            };
            GuncellenecekSubeGruplari guncellenecekSubeGruplari = new GuncellenecekSubeGruplari
            {
                FiyatGuncellemsiHazirlananSubeList = new List<SubeSettingsViewModel>()
            };

            foreach (DataRow item in localSubeList.Rows)
            {
                guncellenecekSubeGruplari = new GuncellenecekSubeGruplari
                {
                    FiyatGuncellemsiHazirlananSubeList = new List<SubeSettingsViewModel>()
                };

                guncellenecekSubeGruplari.GuncellenecekSubeGrupId = mF.RTS(item, "GuncellenecekSubeIdGrubu");
                guncellenecekSubeGruplari.GuncellenecekSubeGrupAdi = mF.RTS(item, "GuncellenecekSubeAdiGrubu");
                listeLocal.GuncellenecekSubeGruplariList.Add(guncellenecekSubeGruplari);
                var parsSubeId = guncellenecekSubeGruplari.GuncellenecekSubeGrupId.TrimEnd(',').Split(',');
                var parsSubeAdi = guncellenecekSubeGruplari.GuncellenecekSubeGrupAdi.TrimEnd(',').Split(',');

                //Ürün ekle sayfasından gelen kayıtlarda NULL olarak set edildiğinden dolayı gruplamaya dahil olamıyor.
                if (!string.IsNullOrEmpty(guncellenecekSubeGruplari.GuncellenecekSubeGrupId))
                {
                    for (int i = 0; i < parsSubeId.Length; i++)
                    {
                        SubeSettingsViewModel model = new SubeSettingsViewModel
                        {
                            ID = Convert.ToInt32(parsSubeId[i]),
                            SubeName = parsSubeAdi[i]
                        };
                        guncellenecekSubeGruplari.FiyatGuncellemsiHazirlananSubeList.Add(model);
                    }
                }
            }

            //foreach (DataRow item in localSubeList.Rows)
            //{
            //    SubeSettingsViewModel model = new SubeSettingsViewModel();
            //    model.ID = Convert.ToInt32(mF.RTS(item, "SubeId"));
            //    model.SubeName = (mF.RTS(item, "SubeName"));
            //    //listeLocal.IsSelectedSubeList.Add(model);
            //    listeLocal.FiyatGuncellemsiHazirlananSubeList.Add(model);
            //}

            #endregion Local pruduct tablosundaki şubeler alınır 

            mF.SqlConnOpen();
            var dt = mF.DataTable("Select Id, SubeName,DBName, * From SubeSettings Where AppDbTypeStatus in (0,1) and Status=1 ");
            mF.SqlConnClose();

            var kullaniciSubeYetkisi = UsersListCRUD.KullaniciSubeYetkiListesi(kullaniciId);
            foreach (DataRow r in dt.Rows)
            {
                var subeId = Convert.ToInt32(mF.RTS(r, "ID"));
                if (kullaniciSubeYetkisi != null && kullaniciSubeYetkisi.FR_SubeListesi.Where(x => x.SubeID == Convert.ToInt32(subeId)).Select(x => x.SubeID).Any())
                {
                    var girmeyecekIdList = listeLocal.GuncellenecekSubeGruplariList.SelectMany(x => x.FiyatGuncellemsiHazirlananSubeList.Select(y => y.ID).ToList());
                    var guncellenenSube = listeLocal.GuncellenecekSubeGruplariList.Select(x => x.FiyatGuncellemsiHazirlananSubeList.Select(z => z.ID).FirstOrDefault());

                    if (!girmeyecekIdList.Contains(subeId))
                    {
                        SubeSettingsViewModel model = new SubeSettingsViewModel
                        {
                            ID = Convert.ToInt32(mF.RTS(r, "ID")),
                            SubeName = (mF.RTS(r, "SubeName")),
                            DBName = mF.RTS(r, "DBName")
                        };
                        listeLocal.IsSelectedSubeList.Add(model);
                    }
                }
            }

            if (listeLocal.IsSelectedSubeList == null || listeLocal.IsSelectedSubeList.Count() == 0)
            {
                SubeSettingsViewModel model = new SubeSettingsViewModel();
                listeLocal.IsSelectedSubeList = new List<SubeSettingsViewModel>();
                listeLocal.IsSelectedSubeList.Add(model);
            }

            #region MyRegion

            //var dt = mF.DataTable("Select Id, SubeName,DBName, * From SubeSettings Where AppDbTypeStatus in (0,1) and Status=1 ");
            //
            //foreach (DataRow r in dt.Rows)
            //{
            //    var subeId = Convert.ToInt32(mF.RTS(r, "Id"));
            //    if (!listeLocal.FiyatGuncellemsiHazirlananSubeList.Where(x => x.ID == subeId).Any())
            //    {
            //        SubeSettingsViewModel model = new SubeSettingsViewModel();
            //        model.ID = Convert.ToInt32(mF.RTS(r, "Id"));
            //        model.SubeName = (mF.RTS(r, "SubeName"));
            //        model.DBName = mF.RTS(r, "DBName");
            //        listeLocal.IsSelectedSubeList.Add(model);
            //    }
            //}
            //mF.SqlConnClose();
            //if (listeLocal.IsSelectedSubeList == null || listeLocal.IsSelectedSubeList.Count() == 0)
            //{
            //    SubeSettingsViewModel model = new SubeSettingsViewModel();
            //    listeLocal.IsSelectedSubeList = new List<SubeSettingsViewModel>();
            //    listeLocal.IsSelectedSubeList.Add(model);
            //}

            #endregion

            return listeLocal;
        }

        #endregion Şube listesi

        #region Products güncelleme

        public UrunEditViewModel GetByIdForEdit(int SubeId, string ProductGroup)
        {
            var urunList = new UrunEditViewModel();

            try
            {
                DataTable dataSubeUrunlist;
                dataSubeUrunlist = mF.GetSubeDataWithQuery((mF.NewConnectionString(subeIp, dbName, sqlKullaniciName, sqlKullaniciPassword))
                                                                          , ProductGroup == "null" || ProductGroup == null ? SqlData.getProductLocalDbSqlQuery(SubeId) : SqlData.getProductForProductGroupSqlQuery(ProductGroup));
                var modelSube = SqlData.GetSube(SubeId);
                if (dataSubeUrunlist.Rows.Count == 0)
                {
                    dataSubeUrunlist = mF.GetSubeDataWithQuery((mF.NewConnectionString(modelSube.SubeIP, modelSube.DBName, modelSube.SqlName, modelSube.SqlPassword))
                                                                          , ProductGroup == "null" || ProductGroup == null ? SqlData.getProductSqlQuery() : SqlData.getProductForProductGroupSqlQuery(ProductGroup));
                }

                urunList.UrunEditList = new List<UrunEdit>();
                urunList.SubeId = SubeId;
                urunList.SubeAdi = modelSube.SubeName;

                foreach (DataRow item in dataSubeUrunlist.Rows)
                {
                    UrunEdit model = new UrunEdit
                    {
                        Id = mF.RTI(item, "Id"),
                        InvoiceName = mF.RTS(item, "InvoiceName"),
                        //model.Order = modelFunctions.RTS(item, "[Order]");
                        Plu = mF.RTS(item, "Plu"),
                        Price = mF.RTD(item, "Price"),
                        ProductCode = mF.RTS(item, "ProductCode"),
                        ProductGroup = mF.RTS(item, "ProductGroup"),
                        ProductName = mF.RTS(item, "ProductName"),
                        ProductType = mF.RTS(item, "ProductType"),
                        VatRate = mF.RTD(item, "VatRate")
                    };

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
        public ActionResultMessages Update(UrunEditViewModel model)
        {
            var result = new ActionResultMessages()
            {
                IsSuccess = true,
                UserMessage = "İşlem Başarılı",
            };

            #region Local database'e veriler kaydetmek için kullanıldı.

            var sqlData = new SqlData(new SqlConnection(mF.NewConnectionString(subeIp, dbName, sqlKullaniciName, sqlKullaniciPassword)));
            var dataSubeUrunlist = mF.GetSubeDataWithQuery((mF.NewConnectionString(subeIp, dbName, sqlKullaniciName, sqlKullaniciPassword)), SqlData.getProductLocalDbSqlQuery(model.SubeId));
            //Önceki kullanım direkt olarak şubedeki db'ye değişikliklei kaydediyordu.
            //var modelSube = SqlData.getSube(model.SubeId);
            //var sqlData = new SqlData(new SqlConnection(modelFunctions.NewConnectionString(modelSube.SubeIP, modelSube.DBName, modelSube.SqlName, modelSube.SqlPassword)));
            //var dataSubeUrunlist = modelFunctions.GetSubeDataWithQuery((modelFunctions.NewConnectionString(modelSube.SubeIP, modelSube.DBName, modelSube.SqlName, modelSube.SqlPassword)), SqlData.getProduct());
            #endregion Local database'e veriler kaydetmek için kullanıldı.

            foreach (var modelList in model.UrunEditList)
            {
                foreach (DataRow dataUrunSube in dataSubeUrunlist.Rows)
                {
                    if (/*modelList.Id == modelFunctions.RTI(dataUrunSube, "ProductPkId")*/
                        modelList.ProductName == mF.RTS(dataUrunSube, "ProductName")
                        && modelList.ProductGroup == mF.RTS(dataUrunSube, "ProductGroup")
                        && model.SubeId == mF.RTD(dataUrunSube, "SubeId")
                        && modelList.Price != mF.RTD(dataUrunSube, "Price")
                        )
                    {
                        sqlData.ExecuteSql("update Product set Price = @par1 Where Id = @par2", new object[] { modelList.Price, modelList.Id });
                        break;
                    }
                }
            }

            if (dataSubeUrunlist.Rows.Count == 0)
            {
                LocalDbProductAndOprionsAndChoice1AndChoice2Insert(model, false);
            }

            return result;
        }

        #endregion Products güncelleme

        #region Product,Options,Choice1,Choice2 local db'ye kayıt için jenerik insert class

        public ActionResultMessages LocalDbProductAndOprionsAndChoice1AndChoice2Insert(UrunEditViewModel model, bool choiceMu)
        {
            var result = new ActionResultMessages
            {
                IsSuccess = true,
                UserMessage = "İşlem başarılı",
            };

            var dataSubeUrunlist = mF.GetSubeDataWithQuery((mF.NewConnectionString(subeIp, dbName, sqlKullaniciName, sqlKullaniciPassword)), SqlData.getProductLocalDbSqlQuery(model.SubeId));
            var sqlData = new SqlData(new SqlConnection(mF.NewConnectionString(subeIp, dbName, sqlKullaniciName, sqlKullaniciPassword)));
            var modelSube = SqlData.GetSube(model.SubeId);

            if (choiceMu)
            {
                dataSubeUrunlist = mF.GetSubeDataWithQuery((mF.NewConnectionString(modelSube.SubeIP, modelSube.DBName, modelSube.SqlName, modelSube.SqlPassword)), SqlData.getProductSqlQuery());

                foreach (DataRow item in dataSubeUrunlist.Rows)
                {
                    //Product
                    sqlData.ExecuteSql(" Insert Into  Product( ProductName, ProductGroup, Price, SubeId, ProductPkId ) Values( @par1, @par2, @par3, @par4, @par5 )",
                        new object[] { mF.RTS(item, "ProductName"), mF.RTS(item, "ProductGroup"), mF.RTS(item, "Price"), model.SubeId, mF.RTI(item, "Id") });

                    //Choice1
                    var dataSubeChoiceUrunlist = mF.GetSubeDataWithQuery((mF.NewConnectionString(modelSube.SubeIP, modelSube.DBName, modelSube.SqlName, modelSube.SqlPassword)), SqlData.getChoice1SqlQuery(mF.RTI(item, "Id")));
                    if (dataSubeChoiceUrunlist.Rows.Count > 0)
                    {
                        foreach (DataRow dataChoice1 in dataSubeChoiceUrunlist.Rows)
                        {
                            sqlData.ExecuteSql(" Insert Into  Choice1( ProductId, Name, Price, SubeId, Choice1PkId ) Values( @par1, @par2, @par3, @par4, @par5 )",
                            new object[] { mF.RTI(dataChoice1, "ProductId"), mF.RTS(dataChoice1, "ChoiceProductName"), mF.RTD(dataChoice1, "Choice1_Price"), model.SubeId, mF.RTI(dataChoice1, "Id") });

                            //Choice2
                            var dataSubeChoice2Urunlist = mF.GetSubeDataWithQuery((mF.NewConnectionString(modelSube.SubeIP, modelSube.DBName, modelSube.SqlName, modelSube.SqlPassword)),
                                                                                                                                        SqlData.getChoice2SqlQuery(mF.RTI(dataChoice1, "Id"), mF.RTI(item, "Id")));
                            if (dataSubeChoice2Urunlist.Rows.Count > 0)
                            {
                                foreach (DataRow dataChoice2 in dataSubeChoice2Urunlist.Rows)
                                {
                                    sqlData.ExecuteSql(" Insert Into  Choice2( ProductId, Choice1Id, Name, Price, SubeId, Choice2PkId ) Values( @par1, @par2, @par3, @par4, @par5 )",
                                    new object[] { mF.RTI(dataChoice2, "ProductId"), mF.RTI(dataChoice1, "Id"), mF.RTS(dataChoice2, "Choice2ProductName"), mF.RTD(dataChoice2, "Choice2_Price"), model.SubeId, mF.RTI(dataChoice2, "Id") });
                                }
                            }
                            //Choice2
                        }
                    }

                    //Options
                    var dataSubeUrunOptionslist = mF.GetSubeDataWithQuery((mF.NewConnectionString(modelSube.SubeIP, modelSube.DBName, modelSube.SqlName, modelSube.SqlPassword)), SqlData.getOptionsSqlQuery(mF.RTI(item, "Id")));
                    if (dataSubeUrunOptionslist.Rows.Count > 0)
                    {
                        foreach (DataRow dataOptions in dataSubeUrunOptionslist.Rows)
                        {
                            sqlData.ExecuteSql(" Insert Into  Options(ProductId, Name, Price, Category, SubeId, OptionsPkId ) Values( @par1, @par2, @par3, @par4, @par5, @par6)",
                            new object[] { mF.RTI(dataOptions, "ProductId"), mF.RTS(dataOptions, "OptionsName"), mF.RTD(dataOptions, "Option_Price"), mF.RTD(dataOptions, "OptionsCategory"), model.SubeId, mF.RTI(dataOptions, "Id") });
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
                        var dataSubeChoiceUrunlist = mF.GetSubeDataWithQuery((mF.NewConnectionString(modelSube.SubeIP, modelSube.DBName, modelSube.SqlName, modelSube.SqlPassword)), SqlData.getChoice1SqlQuery(modelList.Id));
                        if (dataSubeChoiceUrunlist.Rows.Count > 0)
                        {
                            foreach (DataRow dataChoice1 in dataSubeChoiceUrunlist.Rows)
                            {
                                sqlData.ExecuteSql(" Insert Into  Choice1( ProductId, Name, Price, SubeId, Choice1PkId ) Values( @par1, @par2, @par3, @par4, @par5 )",
                                new object[] { mF.RTI(dataChoice1, "ProductId"), mF.RTS(dataChoice1, "ChoiceProductName"), mF.RTD(dataChoice1, "Choice1_Price"), model.SubeId, mF.RTI(dataChoice1, "Id") });

                                //Choice2
                                var dataSubeChoice2Urunlist = mF.GetSubeDataWithQuery((mF.NewConnectionString(modelSube.SubeIP, modelSube.DBName, modelSube.SqlName, modelSube.SqlPassword)),
                                                                                                                                            SqlData.getChoice2SqlQuery(mF.RTI(dataChoice1, "Id"), modelList.Id));
                                if (dataSubeChoice2Urunlist.Rows.Count > 0)
                                {
                                    foreach (DataRow dataChoice2 in dataSubeChoice2Urunlist.Rows)
                                    {
                                        sqlData.ExecuteSql(" Insert Into  Choice2( ProductId, Choice1Id, Name, Price, SubeId, Choice2PkId ) Values( @par1, @par2, @par3, @par4, @par5 )",
                                        new object[] { mF.RTI(dataChoice2, "ProductId"), mF.RTI(dataChoice1, "Id"), mF.RTS(dataChoice2, "Choice2ProductName"), mF.RTD(dataChoice2, "Choice2_Price"), model.SubeId, mF.RTI(dataChoice2, "Id") });
                                    }
                                }
                                //Choice2
                            }
                        }

                        //Options
                        var dataSubeUrunOptionslist = mF.GetSubeDataWithQuery((mF.NewConnectionString(modelSube.SubeIP, modelSube.DBName, modelSube.SqlName, modelSube.SqlPassword)), SqlData.getOptionsSqlQuery(modelList.Id));
                        if (dataSubeUrunOptionslist.Rows.Count > 0)
                        {
                            foreach (DataRow dataOptions in dataSubeUrunOptionslist.Rows)
                            {
                                sqlData.ExecuteSql(" Insert Into  Options(ProductId, Name, Price, Category, SubeId, OptionsPkId ) Values( @par1, @par2, @par3, @par4, @par5, @par6)",
                                new object[] { mF.RTI(dataOptions, "ProductId"), mF.RTS(dataOptions, "OptionsName"), mF.RTD(dataOptions, "Option_Price"), mF.RTD(dataOptions, "OptionsCategory"), model.SubeId, mF.RTI(dataOptions, "Id") });
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
            var dataSubeUrunPkList = mF.GetSubeDataWithQuery((mF.NewConnectionString(subeIp, dbName, sqlKullaniciName, sqlKullaniciPassword)), SqlData.getProductLocalDbPkIdSqlQuery(productId));
            var productIdLocal = 0;
            foreach (DataRow item in dataSubeUrunPkList.Rows)
            {
                productIdLocal = mF.RTI(item, "ProductPkId");
            }
            #endregion

            #region Choice1 listesi

            DataTable dataSubeUrunlist;
            var sqlData = new SqlData(new SqlConnection(mF.NewConnectionString(subeIp, dbName, sqlKullaniciName, sqlKullaniciPassword)));
            dataSubeUrunlist = mF.GetSubeDataWithQuery((mF.NewConnectionString(subeIp, dbName, sqlKullaniciName, sqlKullaniciPassword)), SqlData.getChoice1LocalDbSqlQuery(SubeId, productIdLocal));
            var modelSube = SqlData.GetSube(SubeId);
            if (dataSubeUrunlist.Rows.Count == 0)
            {
                dataSubeUrunlist = mF.GetSubeDataWithQuery((mF.NewConnectionString(modelSube.SubeIP, modelSube.DBName, modelSube.SqlName, modelSube.SqlPassword)), SqlData.getChoice1SqlQuery(productId));
            }

            var choiceUrunList = new UrunEditViewModel();
            choiceUrunList.UrunEditList = new List<UrunEdit>();
            choiceUrunList.SubeId = SubeId;
            choiceUrunList.SubeAdi = modelSube.SubeName;

            foreach (DataRow item in dataSubeUrunlist.Rows)
            {
                UrunEdit model = new UrunEdit();
                model.Id = mF.RTI(item, "ProductId");
                model.Choice1Id = mF.RTI(item, "Id");
                model.Price = mF.RTD(item, "Product_Price");
                model.ChoicePrice = mF.RTD(item, "Choice1_Price");
                model.ProductGroup = mF.RTS(item, "ProductGroup");
                model.ProductName = mF.RTS(item, "ProductName");
                model.ChoiceProductName = mF.RTS(item, "ChoiceProductName");
                choiceUrunList.UrunEditList.Add(model);
            }

            #endregion  Choice1 listesi

            #region Options listesi

            DataTable dataSubeUrunOptionslist;
            dataSubeUrunOptionslist = mF.GetSubeDataWithQuery((mF.NewConnectionString(subeIp, dbName, sqlKullaniciName, sqlKullaniciPassword)), SqlData.getOptionsLocalDbSqlQuery(SubeId, productIdLocal));
            if (dataSubeUrunOptionslist.Rows.Count == 0)
            {
                dataSubeUrunOptionslist = mF.GetSubeDataWithQuery((mF.NewConnectionString(modelSube.SubeIP, modelSube.DBName, modelSube.SqlName, modelSube.SqlPassword)), SqlData.getOptionsSqlQuery(productId));

            }
            choiceUrunList.UrunOptionsEditList = new List<UrunEdit>();
            foreach (DataRow item in dataSubeUrunOptionslist.Rows)
            {
                UrunEdit model = new UrunEdit();
                model.Id = mF.RTI(item, "ProductId");
                model.OptionsId = mF.RTI(item, "Id");
                model.Price = mF.RTD(item, "Product_Price");
                model.OptionsPrice = mF.RTD(item, "Option_Price");
                model.ProductGroup = mF.RTS(item, "ProductGroup");
                model.ProductName = mF.RTS(item, "ProductName");
                model.OptionsProductName = mF.RTS(item, "OptionsName");
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
            var dataSubeUrunPkList = mF.GetSubeDataWithQuery((mF.NewConnectionString(subeIp, dbName, sqlKullaniciName, sqlKullaniciPassword)), SqlData.getProductLocalDbPkIdSqlQuery(model.UrunEditList.FirstOrDefault().Id));
            var productIdLocal = 0;
            foreach (DataRow item in dataSubeUrunPkList.Rows)
            {
                productIdLocal = mF.RTI(item, "Choice1PkId");
            }
            #endregion

            var modelSube = SqlData.GetSube(model.SubeId);
            //var sqlData = new SqlData(new SqlConnection(modelFunctions.NewConnectionString(modelSube.SubeIP, modelSube.DBName, modelSube.SqlName, modelSube.SqlPassword)));
            var sqlData = new SqlData(new SqlConnection(mF.NewConnectionString(subeIp, dbName, sqlKullaniciName, sqlKullaniciPassword)));
            //var dataSubeUrunlist = modelFunctions.GetSubeDataWithQuery((modelFunctions.NewConnectionString(modelSube.SubeIP, modelSube.DBName, modelSube.SqlName, modelSube.SqlPassword)), SqlData.getChoice1(model.UrunEditList[1].Id));
            var dataSubeUrunlist = mF.GetSubeDataWithQuery((mF.NewConnectionString(subeIp, dbName, sqlKullaniciName, sqlKullaniciPassword)), SqlData.getChoice1LocalDbSqlQuery(model.SubeId, model.UrunEditList.FirstOrDefault().Id));

            foreach (var modelList in model.UrunEditList)
            {
                foreach (DataRow dataUrunSube in dataSubeUrunlist.Rows)
                {
                    var tt = mF.RTD(dataUrunSube, "Choice1_Price");
                    var ww = mF.RTS(dataUrunSube, "ChoiceProductName");
                    if (modelList.Id == mF.RTI(dataUrunSube, "ProductId") && modelList.ChoiceProductName == mF.RTS(dataUrunSube, "ChoiceProductName") && modelList.ChoicePrice != mF.RTD(dataUrunSube, "Choice1_Price"))
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
            DataTable dataSubeUrunlist = mF.GetSubeDataWithQuery((mF.NewConnectionString(modelSube.SubeIP, modelSube.DBName, modelSube.SqlName, modelSube.SqlPassword)), SqlData.getChoice2SqlQuery(Choice1Id, productId));

            var choiceUrunList = new UrunEditViewModel();
            choiceUrunList.UrunEditList = new List<UrunEdit>();
            choiceUrunList.SubeId = SubeId;
            choiceUrunList.SubeAdi = modelSube.SubeName;

            foreach (DataRow item in dataSubeUrunlist.Rows)
            {
                UrunEdit model = new UrunEdit();
                model.Id = mF.RTI(item, "ProductId");
                model.Choice2Id = mF.RTI(item, "Id");
                model.Price = mF.RTD(item, "Product_Price");
                model.Choice2Price = mF.RTD(item, "Choice2_Price");
                model.ProductGroup = mF.RTS(item, "ProductGroup");
                model.ProductName = mF.RTS(item, "ProductName");
                model.Choice2ProductName = mF.RTS(item, "Choice2ProductName");
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
            var sqlData = new SqlData(new SqlConnection(mF.NewConnectionString(modelSube.SubeIP, modelSube.DBName, modelSube.SqlName, modelSube.SqlPassword)));
            var dataSubeUrunlist = mF.GetSubeDataWithQuery((mF.NewConnectionString(modelSube.SubeIP, modelSube.DBName, modelSube.SqlName, modelSube.SqlPassword)), SqlData.getChoice1SqlQuery(model.UrunEditList[1].Id));

            foreach (var modelList in model.UrunEditList)
            {
                foreach (DataRow dataUrunSube in dataSubeUrunlist.Rows)
                {
                    if (modelList.Choice1Id == mF.RTI(dataUrunSube, "Id") && modelList.ChoicePrice != mF.RTD(dataUrunSube, "ChoicePrice"))
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
            DataTable dataSubeUrunlist = mF.GetSubeDataWithQuery((mF.NewConnectionString(modelSube.SubeIP, modelSube.DBName, modelSube.SqlName, modelSube.SqlPassword)), SqlData.getOptionsSqlQuery(productId));

            var choiceUrunList = new UrunEditViewModel();
            choiceUrunList.UrunEditList = new List<UrunEdit>();
            choiceUrunList.SubeId = SubeId;
            choiceUrunList.SubeAdi = modelSube.SubeName;

            foreach (DataRow item in dataSubeUrunlist.Rows)
            {
                UrunEdit model = new UrunEdit();
                model.Id = mF.RTI(item, "ProductId");
                model.OptionsId = mF.RTI(item, "Id");
                model.Price = mF.RTD(item, "Product_Price");
                model.OptionsPrice = mF.RTD(item, "Option_Price");
                model.ProductGroup = mF.RTS(item, "ProductGroup");
                model.ProductName = mF.RTS(item, "ProductName");
                model.OptionsProductName = mF.RTS(item, "OptionsName");
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
            var sqlData = new SqlData(new SqlConnection(mF.NewConnectionString(modelSube.SubeIP, modelSube.DBName, modelSube.SqlName, modelSube.SqlPassword)));
            var dataSubeUrunlist = mF.GetSubeDataWithQuery((mF.NewConnectionString(modelSube.SubeIP, modelSube.DBName, modelSube.SqlName, modelSube.SqlPassword)), SqlData.getOptionsSqlQuery(model.UrunOptionsEditList[1].Id));

            foreach (var modelList in model.UrunOptionsEditList)
            {
                foreach (DataRow dataUrunSube in dataSubeUrunlist.Rows)
                {
                    if (modelList.OptionsId == mF.RTI(dataUrunSube, "Id") && modelList.OptionsPrice != mF.RTD(dataUrunSube, "ChoicePrice"))
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
            var dataSubeUrunGrubuList = mF.GetSubeDataWithQuery((mF.NewConnectionString(modelSube.SubeIP, modelSube.DBName, modelSube.SqlName, modelSube.SqlPassword)), SqlData.getUrunGrubuSqlQuery());

            foreach (DataRow r in dataSubeUrunGrubuList.Rows)
            {
                selectListItem.Add(new SelectListItem
                {
                    Text = mF.RTS(r, "ProductGroup").ToString(),
                    Value = mF.RTS(r, "ProductGroup"),
                });
            }

            return selectListItem;
        }
        public List<SelectListItem> LocalSubeUrunGrubuListJson()
        {
            List<SelectListItem> selectListItem = new List<SelectListItem>();

            var dataSubeUrunGrubuList = mF.GetSubeDataWithQuery((mF.NewConnectionString(subeIp, dbName, sqlKullaniciName, sqlKullaniciPassword)), SqlData.getUrunGrubuSqlQuery());

            foreach (DataRow r in dataSubeUrunGrubuList.Rows)
            {
                selectListItem.Add(new SelectListItem
                {
                    Text = mF.RTS(r, "ProductGroup").ToString(),
                    Value = mF.RTS(r, "ProductGroup"),
                });
            }

            return selectListItem;
        }


        public List<SelectListItem> LocalSubeSablonUrunGrubuListJson()
        {
            List<SelectListItem> selectListItem = new List<SelectListItem>();

            var dataSubeUrunGrubuList = mF.GetSubeDataWithQuery((mF.NewConnectionString(subeIp, dbName, sqlKullaniciName, sqlKullaniciPassword)), SqlData.GetSablonProductUrunGrubuSqlQuery());

            foreach (DataRow r in dataSubeUrunGrubuList.Rows)
            {
                selectListItem.Add(new SelectListItem
                {
                    Text = mF.RTS(r, "ProductGroup").ToString(),
                    Value = mF.RTS(r, "ProductGroup"),
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
            var dataKaynakSubeUrunlist = mF.GetSubeDataWithQuery((mF.NewConnectionString(subeIp, dbName, sqlKullaniciName, sqlKullaniciPassword)), SqlData.getProductLocalDbSqlQuery(kaynakSubeId));
            model.IsSelectedSubeList = model.IsSelectedSubeList.Where(x => x.IsSelectedHedefSube).ToList();
            foreach (var item in model.IsSelectedSubeList)
            {
                var modelSube = SqlData.GetSube(item.ID);
                var dataHedefSubeUrunList = mF.GetSubeDataWithQuery(mF.NewConnectionString(modelSube.SubeIP, modelSube.DBName, modelSube.SqlName, modelSube.SqlPassword), SqlData.getProductSqlQuery());
                var sqlData = new SqlData(new SqlConnection(mF.NewConnectionString(modelSube.SubeIP, modelSube.DBName, modelSube.SqlName, modelSube.SqlPassword)));

                foreach (DataRow kaynakSube in dataKaynakSubeUrunlist.Rows)
                {
                    foreach (DataRow hedefSube in dataHedefSubeUrunList.Rows)
                    {
                        if (mF.RTS(kaynakSube, "ProductName") == mF.RTS(hedefSube, "ProductName")
                           && mF.RTS(kaynakSube, "ProductGroup") == mF.RTS(hedefSube, "ProductGroup"))
                        {
                            sqlData.ExecuteSql("Update Product set Price= @par1 Where ProductName = @par2 and ProductGroup = @par3",
                                 new object[] { mF.RTS(kaynakSube, "Price"), mF.RTS(kaynakSube, "ProductName"), mF.RTS(kaynakSube, "ProductGroup") });
                            break;
                        }
                    }
                }
            }

            return result;
        }

        #endregion Şubelere fiyat yayma

        #region (05.02.2022 toplantı sonrası) Seçili şubelerdeki pruduct, choice1,choice2,options tablolarınını merkez (local temp db'ye aktarma) 

        #region Product update

        public ActionResultMessages SubeProductInsertLocalTable(SubelereVeriGonderViewModel model)
        {
            var result = new ActionResultMessages { IsSuccess = true, UserMessage = "İşlem başarılı", };

            string isSelectedSubeId = string.Empty;
            string isSelectedSubeAdi = string.Empty;
            foreach (var item in model.IsSelectedSubeList.Where(x => x.IsSelectedHedefSube))
            {
                isSelectedSubeId += item.ID + ",";
                isSelectedSubeAdi += item.SubeName + ",";
            }

            //string[] vs = { };
            //for (int i = 0; i < model.IsSelectedSubeList.Where(x => x.IsSelectedHedefSube).Count(); i++)
            //{
            //   vs[i] = model.IsSelectedSubeList[i].ID.ToString();  
            //}

            //Seçili bir şubeye bağlantı kurulamıyorsa işlmler.Code review yapılabilir. Daha jenerik bir kontrol yapılabilir.

            var dtList = model.IsSelectedSubeList.Where(x => x.IsSelectedHedefSube).AsEnumerable().Distinct().ToList().AsEnumerable().ToList();
            Parallel.ForEach(dtList, isSelectedSubeItem =>
            {
                //foreach (var isSelectedSubeItem in model.IsSelectedSubeList.Where(x => x.IsSelectedHedefSube))
                //{
                var modelSube = SqlData.GetSube(isSelectedSubeItem.ID);
                try
                {
                    var dataSubeUrunlist = mF.GetSubeDataWithQuery(mF.NewConnectionString(modelSube.SubeIP, modelSube.DBName, modelSube.SqlName, modelSube.SqlPassword), SqlData.getProductSqlQuery());
                }
                catch (Exception ex)
                {
                    Singleton.WritingLogFile2("SPosSubeFiyatGuncellemeCRUD_SubeProductInsertLocalTable", ex.ToString(), null, ex.StackTrace);
                    result.IsSuccess = false;
                    result.UserMessage += Environment.NewLine + modelSube.SubeName + " şubesine bağlantı kurulamadı. Lütfen seçili olan şubenin aktif olduğundan emin olunuz.";
                    //return result;
                }
                //}
            });
            if (!result.IsSuccess)
            {
                return result;
            }

            foreach (var isSelectedSubeItem in model.IsSelectedSubeList.Where(x => x.IsSelectedHedefSube))
            {
                var modelSube = SqlData.GetSube(isSelectedSubeItem.ID);
                try
                {
                    var sqlData = new SqlData(new SqlConnection(mF.NewConnectionString(subeIp, dbName, sqlKullaniciName, sqlKullaniciPassword)));
                    var dataSubeUrunlist = mF.GetSubeDataWithQuery(mF.NewConnectionString(modelSube.SubeIP, modelSube.DBName, modelSube.SqlName, modelSube.SqlPassword), SqlData.getProductSqlQuery());

                    foreach (DataRow item in dataSubeUrunlist.AsEnumerable().Distinct().ToList())
                    {
                        var pPrice = Convert.ToDecimal(mF.RTD(item, "Price"));
                        //Product
                        sqlData.ExecuteSql(" Insert Into  Product( ProductName, ProductGroup, Price, SubeId, SubeName, ProductPkId, GuncellenecekSubeIdGrubu, GuncellenecekSubeAdiGrubu ) Values( @par1, @par2, @par3, @par4, @par5, @par6,@par7,@par8 )",
                            new object[]
                            {
                                mF.RTS(item, "ProductName"),
                                mF.RTS(item, "ProductGroup"),
                                pPrice,
                                isSelectedSubeItem.ID,
                                isSelectedSubeItem.SubeName,
                                mF.RTI(item, "Id"),
                                isSelectedSubeId,
                                isSelectedSubeAdi
                        });

                        //Choice1
                        var dataSubeChoiceUrunlist = mF.GetSubeDataWithQuery((mF.NewConnectionString(modelSube.SubeIP, modelSube.DBName, modelSube.SqlName, modelSube.SqlPassword)), SqlData.getChoice1SqlQuery(mF.RTI(item, "Id")));
                        if (dataSubeChoiceUrunlist.Rows.Count > 0)
                        {
                            foreach (DataRow dataChoice1 in dataSubeChoiceUrunlist.Rows)
                            {
                                var ch1Price = Convert.ToDecimal(mF.RTD(dataChoice1, "Choice1_Price"));
                                sqlData.ExecuteSql(" Insert Into  Choice1( ProductId, Name, Price, SubeId, SubeName, Choice1PkId,  GuncellenecekSubeIdGrubu, GuncellenecekSubeAdiGrubu ) Values( @par1, @par2, @par3, @par4, @par5, @par6, @par7,@par8 )",
                                 new object[]
                                 {
                                        mF.RTI(dataChoice1, "ProductId"),
                                        mF.RTS(dataChoice1, "ChoiceProductName"),
                                        ch1Price,
                                        isSelectedSubeItem.ID,
                                        isSelectedSubeItem.SubeName,
                                        mF.RTI(dataChoice1, "Id"),
                                        isSelectedSubeId,
                                        isSelectedSubeAdi
                                });

                                //Choice2
                                var dataSubeChoice2Urunlist = mF.GetSubeDataWithQuery((mF.NewConnectionString(modelSube.SubeIP, modelSube.DBName, modelSube.SqlName, modelSube.SqlPassword)),
                                                                                      SqlData.getChoice2SqlQuery(mF.RTI(dataChoice1, "Id"), mF.RTI(item, "Id")));
                                if (dataSubeChoice2Urunlist.Rows.Count > 0)
                                {
                                    foreach (DataRow dataChoice2 in dataSubeChoice2Urunlist.Rows)
                                    {
                                        var ch2Price = Convert.ToDecimal(mF.RTD(dataChoice2, "Choice2_Price"));
                                        var pName = mF.RTS(dataChoice2, "ChoiceProductName");
                                        sqlData.ExecuteSql(" Insert Into  Choice2( ProductId, Choice1Id, Name, Price, SubeId, SubeName, Choice2PkId,  GuncellenecekSubeIdGrubu, GuncellenecekSubeAdiGrubu ) Values( @par1, @par2, @par3, @par4, @par5, @par6, @par7, @par8, @par9 )",
                                         new object[]
                                         {
                                                mF.RTI(dataChoice2, "ProductId"),
                                                mF.RTI(dataChoice1, "Id"),
                                                mF.RTS(dataChoice2, "ChoiceProductName"),
                                                ch2Price,
                                                isSelectedSubeItem.ID,
                                                isSelectedSubeItem.SubeName,
                                                mF.RTI(dataChoice2, "Id"),
                                                isSelectedSubeId,
                                                isSelectedSubeAdi
                                });
                                    }
                                }
                                //Choice2
                            }
                        }

                        //Options
                        var dataSubeUrunOptionslist = mF.GetSubeDataWithQuery((mF.NewConnectionString(modelSube.SubeIP, modelSube.DBName, modelSube.SqlName, modelSube.SqlPassword)), SqlData.getOptionsSqlQuery(mF.RTI(item, "Id")));
                        if (dataSubeUrunOptionslist.Rows.Count > 0)
                        {
                            foreach (DataRow dataOptions in dataSubeUrunOptionslist.Rows)
                            {
                                var opPrice = Convert.ToDecimal(mF.RTD(dataOptions, "Option_Price"));
                                sqlData.ExecuteSql(" Insert Into  Options(ProductId, Name, Price, Category, SubeId, SubeName, OptionsPkId,  GuncellenecekSubeIdGrubu, GuncellenecekSubeAdiGrubu ) Values( @par1, @par2, @par3, @par4, @par5, @par6, @par7, @par8, @par9)",
                                 new object[]
                                 {
                                        mF.RTI(dataOptions, "ProductId"),
                                        mF.RTS(dataOptions, "OptionsName"),
                                        opPrice,
                                        mF.RTD(dataOptions, "OptionsCategory"),
                                        isSelectedSubeItem.ID,
                                        isSelectedSubeItem.SubeName,
                                        mF.RTI(dataOptions, "Id"),
                                        isSelectedSubeId,
                                        isSelectedSubeAdi
                        });
                            }
                        }
                        //});
                    }
                }
                catch (Exception ex)
                {
                    Singleton.WritingLogFile2("SPosSubeFiyatGuncellemeCRUD_IsUpdateProduct", ex.ToString(), null, ex.StackTrace);
                    result.IsSuccess = false;
                    result.UserMessage = modelSube.SubeName + " şubesine bağlantı kurulamadı. Lütfen şubenin aktif olduğundan emin olunuz.";
                    return result;
                }
            }

            return result;
        }

        public ActionResultMessages SubeProductInsertLocalTable2(SubelereVeriGonderViewModel model)
        {
            var result = new ActionResultMessages { IsSuccess = true, UserMessage = "İşlem başarılı", };

            string isSelectedSubeId = string.Empty;
            string isSelectedSubeAdi = string.Empty;
            foreach (var item in model.IsSelectedSubeList.Where(x => x.IsSelectedHedefSube))
            {
                isSelectedSubeId += item.ID + ",";
                isSelectedSubeAdi += item.SubeName + ",";
            }

            //Seçili bir şubeye bağlantı kurulamıyorsa işlmler.Code review yapılabilir. Daha jenerik bir kontrol yapılabilir.
            var dtList = model.IsSelectedSubeList.Where(x => x.IsSelectedHedefSube).AsEnumerable().Distinct().ToList().AsEnumerable().ToList();
            //Parallel.ForEach(dtList, isSelectedSubeItem =>
            //{
            foreach (var isSelectedSubeItem in model.IsSelectedSubeList.Where(x => x.IsSelectedHedefSube))
            {
                var modelSube = SqlData.GetSube(isSelectedSubeItem.ID);
                try
                {
                    var dataSubeUrunlist = mF.GetSubeDataWithQuery(mF.NewConnectionString(modelSube.SubeIP, modelSube.DBName, modelSube.SqlName, modelSube.SqlPassword), SqlData.getProductSqlQuery());
                }
                catch (Exception ex)
                {
                    Singleton.WritingLogFile2("SPosSubeFiyatGuncellemeCRUD_SubeProductInsertLocalTable", ex.ToString(), null, ex.StackTrace);
                    result.IsSuccess = false;
                    result.UserMessage += Environment.NewLine + modelSube.SubeName + " şubesine bağlantı kurulamadı. Lütfen seçili olan şubenin aktif olduğundan emin olunuz.";
                    //return result;
                }
            }
            //});
            if (!result.IsSuccess)
            {
                return result;
            }

            string logTable = string.Empty;
            int tempInsertCount = 0;
            foreach (var isSelectedSubeItem in model.IsSelectedSubeList.Where(x => x.IsSelectedHedefSube))
            {
                var modelSube = SqlData.GetSube(isSelectedSubeItem.ID);

                using (SqlConnection con = new SqlConnection(mF.NewConnectionString(subeIp, dbName, sqlKullaniciName, sqlKullaniciPassword)))
                {
                    con.Open();
                    SqlTransaction transaction = con.BeginTransaction();
                    //SqlData sqlData = new SqlData(con);
                    try
                    {
                        // sqlData = new SqlData(new SqlConnection(mF.NewConnectionString(subeIp, dbName, sqlKullaniciName, sqlKullaniciPassword)));
                        var dataSubeUrunlist = mF.GetSubeDataWithQuery(mF.NewConnectionString(modelSube.SubeIP, modelSube.DBName, modelSube.SqlName, modelSube.SqlPassword), SqlData.getProductSqlQuery());

                        #region Product_Tablosu

                        dataSubeUrunlist.Columns.Add(new DataColumn("Id", typeof(Int32)));
                        dataSubeUrunlist.Columns.Add(new DataColumn("SubeId", typeof(Int32)));
                        dataSubeUrunlist.Columns.Add(new DataColumn("SubeName", typeof(string)));
                        //dataSubeUrunlist.Columns.Add(new DataColumn("ProductPkId", typeof(Int32)));
                        dataSubeUrunlist.Columns.Add(new DataColumn("IsUpdate", typeof(bool)));
                        dataSubeUrunlist.Columns.Add(new DataColumn("YeniUrunMu", typeof(bool)));
                        dataSubeUrunlist.Columns.Add(new DataColumn("GuncellenecekSubeIdGrubu", typeof(string)));
                        dataSubeUrunlist.Columns.Add(new DataColumn("GuncellenecekSubeAdiGrubu", typeof(string)));

                        DataTable dataTableList = dataSubeUrunlist;
                        for (int i = 0; i < dataTableList.AsEnumerable().Count(); i++)
                        {
                            dataSubeUrunlist.Rows[i]["ProductPkId"] = dataSubeUrunlist.Rows[i]["ProductPkId"]; // mF.RTI(dataSubeUrunlist.Rows[i]["Id"], "Id");
                            dataSubeUrunlist.Rows[i]["SubeId"] = isSelectedSubeItem.ID;
                            dataSubeUrunlist.Rows[i]["SubeName"] = isSelectedSubeItem.SubeName;
                            dataSubeUrunlist.Rows[i]["IsUpdate"] = 1;
                            dataSubeUrunlist.Rows[i]["YeniUrunMu"] = 0;
                            dataSubeUrunlist.Rows[i]["GuncellenecekSubeIdGrubu"] = isSelectedSubeId;
                            dataSubeUrunlist.Rows[i]["GuncellenecekSubeAdiGrubu"] = isSelectedSubeAdi;
                        }

                        logTable = "Product_Tablosu";
                        using (SqlBulkCopy bulkCopy = new SqlBulkCopy(con, SqlBulkCopyOptions.KeepNulls, transaction))
                        {
                            bulkCopy.DestinationTableName = "Product";
                            bulkCopy.ColumnMappings.Add("Id", "Id");
                            bulkCopy.ColumnMappings.Add("ProductName", "ProductName");
                            bulkCopy.ColumnMappings.Add("ProductGroup", "ProductGroup");
                            bulkCopy.ColumnMappings.Add("ProductCode", "ProductCode");
                            bulkCopy.ColumnMappings.Add("Order", "Order");
                            bulkCopy.ColumnMappings.Add("Price", "Price");
                            bulkCopy.ColumnMappings.Add("VatRate", "VatRate");
                            bulkCopy.ColumnMappings.Add("FreeItem", "FreeItem");
                            bulkCopy.ColumnMappings.Add("InvoiceName", "InvoiceName");
                            bulkCopy.ColumnMappings.Add("ProductType", "ProductType");
                            bulkCopy.ColumnMappings.Add("Plu", "Plu");
                            bulkCopy.ColumnMappings.Add("SkipOptionSelection", "SkipOptionSelection");
                            bulkCopy.ColumnMappings.Add("Favorites", "Favorites");
                            bulkCopy.ColumnMappings.Add("Aktarildi", "Aktarildi");
                            bulkCopy.ColumnMappings.Add("SubeId", "SubeId");
                            bulkCopy.ColumnMappings.Add("SubeName", "SubeName");
                            bulkCopy.ColumnMappings.Add("ProductPkId", "ProductPkId");
                            bulkCopy.ColumnMappings.Add("IsUpdate", "IsUpdate");
                            bulkCopy.ColumnMappings.Add("YeniUrunMu", "YeniUrunMu");
                            bulkCopy.ColumnMappings.Add("GuncellenecekSubeIdGrubu", "GuncellenecekSubeIdGrubu");
                            bulkCopy.ColumnMappings.Add("GuncellenecekSubeAdiGrubu", "GuncellenecekSubeAdiGrubu");
                            bulkCopy.WriteToServer(dataSubeUrunlist);
                        }

                        #endregion

                        #region Choice1
                        //Choice1
                        logTable = "Choice1_Tablosu";
                        var dataSubeChoiceUrunlist = mF.GetSubeDataWithQuery(mF.NewConnectionString(modelSube.SubeIP, modelSube.DBName, modelSube.SqlName, modelSube.SqlPassword), SqlData.getChoice1SqlQuery2());
                        if (dataSubeChoiceUrunlist.Rows.Count > 0)
                        {
                            dataSubeChoiceUrunlist.Columns.Add(new DataColumn("Id", typeof(Int32)));
                            dataSubeChoiceUrunlist.Columns.Add(new DataColumn("SubeId", typeof(Int32)));
                            dataSubeChoiceUrunlist.Columns.Add(new DataColumn("SubeName", typeof(string)));
                            //dataSubeChoiceUrunlist.Columns.Add(new DataColumn("Choice1PkId", typeof(Int32)));
                            dataSubeChoiceUrunlist.Columns.Add(new DataColumn("IsUpdate", typeof(bool)));
                            dataSubeChoiceUrunlist.Columns.Add(new DataColumn("YeniUrunMu", typeof(bool)));
                            dataSubeChoiceUrunlist.Columns.Add(new DataColumn("GuncellenecekSubeIdGrubu", typeof(string)));
                            dataSubeChoiceUrunlist.Columns.Add(new DataColumn("GuncellenecekSubeAdiGrubu", typeof(string)));

                            DataTable dataTableC1List = dataSubeChoiceUrunlist;
                            for (int i = 0; i < dataTableC1List.AsEnumerable().Count(); i++)
                            {
                                dataSubeChoiceUrunlist.Rows[i]["SubeId"] = isSelectedSubeItem.ID;
                                dataSubeChoiceUrunlist.Rows[i]["SubeName"] = isSelectedSubeItem.SubeName;
                                dataSubeChoiceUrunlist.Rows[i]["Choice1PkId"] = dataSubeChoiceUrunlist.Rows[i]["Choice1PkId"];
                                dataSubeChoiceUrunlist.Rows[i]["IsUpdate"] = 1;
                                dataSubeChoiceUrunlist.Rows[i]["YeniUrunMu"] = 0;
                                dataSubeChoiceUrunlist.Rows[i]["GuncellenecekSubeIdGrubu"] = isSelectedSubeId;
                                dataSubeChoiceUrunlist.Rows[i]["GuncellenecekSubeAdiGrubu"] = isSelectedSubeAdi;
                            }

                            using (SqlBulkCopy bulkCopy = new SqlBulkCopy(con, SqlBulkCopyOptions.KeepNulls, transaction))
                            {
                                //using (SqlBulkCopy bulkCopy = new SqlBulkCopy(mF.NewConnectionString(subeIp, dbName, sqlKullaniciName, sqlKullaniciPassword)))
                                //{
                                bulkCopy.DestinationTableName = "Choice1";
                                bulkCopy.ColumnMappings.Add("Id", "Id");
                                bulkCopy.ColumnMappings.Add("ProductId", "ProductId");
                                bulkCopy.ColumnMappings.Add("Name", "Name");
                                bulkCopy.ColumnMappings.Add("Price", "Price");
                                bulkCopy.ColumnMappings.Add("Aktarildi", "Aktarildi");
                                bulkCopy.ColumnMappings.Add("SubeId", "SubeId");
                                bulkCopy.ColumnMappings.Add("SubeName", "SubeName");
                                bulkCopy.ColumnMappings.Add("Choice1PkId", "Choice1PkId");
                                bulkCopy.ColumnMappings.Add("IsUpdate", "IsUpdate");
                                bulkCopy.ColumnMappings.Add("YeniUrunMu", "YeniUrunMu");
                                bulkCopy.ColumnMappings.Add("GuncellenecekSubeIdGrubu", "GuncellenecekSubeIdGrubu");
                                bulkCopy.ColumnMappings.Add("GuncellenecekSubeAdiGrubu", "GuncellenecekSubeAdiGrubu");

                                // [Id]
                                //,[ProductId]
                                //,[Name]
                                //,[Price]
                                //,[Aktarildi]
                                //,[SubeId]
                                //,[SubeName]
                                //,[Choice1PkId]
                                //,[IsUpdate]
                                //,[YeniUrunMu]
                                //,[GuncellenecekSubeIdGrubu]
                                //,[GuncellenecekSubeAdiGrubu]


                                bulkCopy.WriteToServer(dataSubeChoiceUrunlist);
                            }
                        }
                        #endregion


                        //Choice2
                        logTable = "Choice2_Tablosu";
                        var dataSubeChoice2Urunlist = mF.GetSubeDataWithQuery(mF.NewConnectionString(modelSube.SubeIP, modelSube.DBName, modelSube.SqlName, modelSube.SqlPassword), SqlData.getChoice2SqlQuery2());
                        if (dataSubeChoice2Urunlist.Rows.Count > 0)
                        {
                            dataSubeChoice2Urunlist.Columns.Add(new DataColumn("Id", typeof(Int32)));
                            dataSubeChoice2Urunlist.Columns.Add(new DataColumn("SubeId", typeof(Int32)));
                            dataSubeChoice2Urunlist.Columns.Add(new DataColumn("SubeName", typeof(string)));
                            //dataSubeChoice2Urunlist.Columns.Add(new DataColumn("Choice2PkId", typeof(Int32)));
                            dataSubeChoice2Urunlist.Columns.Add(new DataColumn("IsUpdate", typeof(bool)));
                            dataSubeChoice2Urunlist.Columns.Add(new DataColumn("YeniUrunMu", typeof(bool)));
                            dataSubeChoice2Urunlist.Columns.Add(new DataColumn("GuncellenecekSubeIdGrubu", typeof(string)));
                            dataSubeChoice2Urunlist.Columns.Add(new DataColumn("GuncellenecekSubeAdiGrubu", typeof(string)));

                            DataTable dataTableC2List = dataSubeChoice2Urunlist;
                            for (int i = 0; i < dataTableC2List.AsEnumerable().Count(); i++)
                            {
                                dataSubeChoice2Urunlist.Rows[i]["SubeId"] = isSelectedSubeItem.ID;
                                dataSubeChoice2Urunlist.Rows[i]["SubeName"] = isSelectedSubeItem.SubeName;
                                dataSubeChoice2Urunlist.Rows[i]["Choice2PkId"] = dataSubeChoice2Urunlist.Rows[i]["Choice2PkId"];
                                dataSubeChoice2Urunlist.Rows[i]["IsUpdate"] = 1;
                                dataSubeChoice2Urunlist.Rows[i]["YeniUrunMu"] = 0;
                                dataSubeChoice2Urunlist.Rows[i]["GuncellenecekSubeIdGrubu"] = isSelectedSubeId;
                                dataSubeChoice2Urunlist.Rows[i]["GuncellenecekSubeAdiGrubu"] = isSelectedSubeAdi;
                            }
                            using (SqlBulkCopy bulkCopy = new SqlBulkCopy(con, SqlBulkCopyOptions.KeepNulls, transaction))
                            {
                                bulkCopy.DestinationTableName = "Choice2";
                                bulkCopy.ColumnMappings.Add("Id", "Id");
                                bulkCopy.ColumnMappings.Add("ProductId", "ProductId");
                                bulkCopy.ColumnMappings.Add("Choice1Id", "Choice1Id");
                                bulkCopy.ColumnMappings.Add("Name", "Name");
                                bulkCopy.ColumnMappings.Add("Price", "Price");
                                bulkCopy.ColumnMappings.Add("Aktarildi", "Aktarildi");
                                bulkCopy.ColumnMappings.Add("SubeId", "SubeId");
                                bulkCopy.ColumnMappings.Add("SubeName", "SubeName");
                                bulkCopy.ColumnMappings.Add("Choice2PkId", "Choice2PkId");
                                bulkCopy.ColumnMappings.Add("IsUpdate", "IsUpdate");
                                bulkCopy.ColumnMappings.Add("YeniUrunMu", "YeniUrunMu");
                                bulkCopy.ColumnMappings.Add("GuncellenecekSubeIdGrubu", "GuncellenecekSubeIdGrubu");
                                bulkCopy.ColumnMappings.Add("GuncellenecekSubeAdiGrubu", "GuncellenecekSubeAdiGrubu");
                                bulkCopy.WriteToServer(dataSubeChoice2Urunlist);
                            }
                            //Choice2
                        }

                        ////Options
                        logTable = "Options_Tablosu";
                        var dataSubeUrunOptionslist = mF.GetSubeDataWithQuery(mF.NewConnectionString(modelSube.SubeIP, modelSube.DBName, modelSube.SqlName, modelSube.SqlPassword), SqlData.getOptionsSqlQuery2());
                        if (dataSubeUrunOptionslist.Rows.Count > 0)
                        {
                            dataSubeUrunOptionslist.Columns.Add(new DataColumn("Id", typeof(Int32)));
                            dataSubeUrunOptionslist.Columns.Add(new DataColumn("SubeId", typeof(Int32)));
                            dataSubeUrunOptionslist.Columns.Add(new DataColumn("SubeName", typeof(string)));
                            //dataSubeUrunOptionslist.Columns.Add(new DataColumn("OptionsPkId", typeof(Int32)));
                            dataSubeUrunOptionslist.Columns.Add(new DataColumn("IsUpdate", typeof(bool)));
                            dataSubeUrunOptionslist.Columns.Add(new DataColumn("YeniUrunMu", typeof(bool)));
                            dataSubeUrunOptionslist.Columns.Add(new DataColumn("GuncellenecekSubeIdGrubu", typeof(string)));
                            dataSubeUrunOptionslist.Columns.Add(new DataColumn("GuncellenecekSubeAdiGrubu", typeof(string)));


                            DataTable dataTableOptionsList = dataSubeUrunOptionslist;
                            for (int i = 0; i < dataTableOptionsList.AsEnumerable().Count(); i++)
                            {
                                dataSubeUrunOptionslist.Rows[i]["SubeId"] = isSelectedSubeItem.ID;
                                dataSubeUrunOptionslist.Rows[i]["SubeName"] = isSelectedSubeItem.SubeName;
                                dataSubeUrunOptionslist.Rows[i]["OptionsPkId"] = dataSubeUrunOptionslist.Rows[i]["OptionsPkId"];
                                dataSubeUrunOptionslist.Rows[i]["IsUpdate"] = 1;
                                dataSubeUrunOptionslist.Rows[i]["YeniUrunMu"] = 0;
                                dataSubeUrunOptionslist.Rows[i]["GuncellenecekSubeIdGrubu"] = isSelectedSubeId;
                                dataSubeUrunOptionslist.Rows[i]["GuncellenecekSubeAdiGrubu"] = isSelectedSubeAdi;
                            }
                            using (SqlBulkCopy bulkCopy = new SqlBulkCopy(con, SqlBulkCopyOptions.KeepNulls, transaction))
                            {
                                bulkCopy.DestinationTableName = "Options";
                                bulkCopy.ColumnMappings.Add("Id", "Id");
                                bulkCopy.ColumnMappings.Add("Name", "Name");
                                bulkCopy.ColumnMappings.Add("Price", "Price");
                                bulkCopy.ColumnMappings.Add("Quantitative", "Quantitative");
                                bulkCopy.ColumnMappings.Add("ProductId", "ProductId");
                                bulkCopy.ColumnMappings.Add("Category", "Category");
                                bulkCopy.ColumnMappings.Add("Aktarildi", "Aktarildi");
                                bulkCopy.ColumnMappings.Add("SubeId", "SubeId");
                                bulkCopy.ColumnMappings.Add("SubeName", "SubeName");
                                bulkCopy.ColumnMappings.Add("OptionsPkId", "OptionsPkId");
                                bulkCopy.ColumnMappings.Add("IsUpdate", "IsUpdate");
                                bulkCopy.ColumnMappings.Add("YeniUrunMu", "YeniUrunMu");
                                bulkCopy.ColumnMappings.Add("GuncellenecekSubeIdGrubu", "GuncellenecekSubeIdGrubu");
                                bulkCopy.ColumnMappings.Add("GuncellenecekSubeAdiGrubu", "GuncellenecekSubeAdiGrubu");
                                bulkCopy.WriteToServer(dataSubeUrunOptionslist);
                            }
                        }

                        transaction.Commit();
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        Singleton.WritingLogFile2("SPosSubeFiyatGuncellemeCRUD_IsUpdateProduct", "Sube Adı:" + isSelectedSubeItem.SubeName + " Tablo Adı:" + logTable + " tablosunda>>>" + ex.ToString(), null, ex.StackTrace);
                        result.IsSuccess = false;
                        result.UserMessage = modelSube.SubeName + " şubesine bağlantı kurulamadı. Lütfen şubenin aktif olduğundan emin olunuz.";
                        logTable = string.Empty;
                        return result;
                    }
                }
            }

            return result;
        }

        public UrunEditViewModel GetByProductForSubeListEdit(string productGroup_, string subeIdGrupList)
        {
            var result = new ActionResultMessages { IsSuccess = true, UserMessage = "İşlem başarılı", };
            var returnViewModel = new UrunEditViewModel() { };
            returnViewModel.SubeList = new List<ProductCompairSube>();
            returnViewModel.productCompairsList = new List<ProductCompair>();
            returnViewModel.ErrorList = new List<string>();
            returnViewModel.IsSuccess = true;

            try
            {
                #region Local database'e veriler kaydetmek için kullanıldı.
                var sqlData = new SqlData(new SqlConnection(mF.NewConnectionString(subeIp, dbName, sqlKullaniciName, sqlKullaniciPassword)));
                DataTable dataSubeUrunlist = mF.GetSubeDataWithQuery(mF.NewConnectionString(subeIp, dbName, sqlKullaniciName, sqlKullaniciPassword),
                                                                      productGroup_ == null || productGroup_ == "null"
                                                                      ? SqlData.getLocalProductListSqlQuery(subeIdGrupList)
                                                                      : SqlData.getProductForProductGroupSqlQueryVeriGonder(productGroup_, subeIdGrupList));
                #endregion Local database'e veriler kaydetmek için kullanıldı.

                var subeIdList = new List<long>();
                foreach (DataRow item in dataSubeUrunlist.Rows)
                {
                    subeIdList.Add(mF.RTI(item, "SubeId"));
                }

                subeIdList = subeIdList.Distinct().ToList();
                foreach (var sube in subeIdList)
                {
                    var drSube = dataSubeUrunlist.Select("SubeId=" + sube)[0];
                    var subeName = mF.RTS(drSube, "SubeName");
                    returnViewModel.SubeList.Add(new ProductCompairSube
                    {
                        SubeId = sube,
                        SubeAdi = subeName,
                    });
                }

                foreach (DataRow item in dataSubeUrunlist.Rows)
                {
                    var productGroup = mF.RTS(item, "ProductGroup");
                    var productName = mF.RTS(item, "ProductName");
                    var subeName = mF.RTS(item, "SubeName");
                    var subePrice = mF.RTD(item, "Price");
                    var subeId = mF.RTI(item, "SubeId");
                    var productId = mF.RTI(item, "Id");
                    var optionsVarMi = mF.RTI(item, "OptionsVarMi");
                    var choice1VarMi = mF.RTI(item, "Choice1VarMi");
                    var choiceProductName = mF.RTS(item, "ChoiceProductName");

                    var pc = returnViewModel.productCompairsList.Where(x => x.ProductName == productName && x.ProductGroup == productGroup).FirstOrDefault();
                    if (pc == null)
                    {
                        pc = new ProductCompair
                        {
                            ProductGroup = productGroup,
                            ProductName = productName,
                            ProductId = productId,
                            OptionsVarMi = optionsVarMi,
                            Choice1VarMi = choice1VarMi
                        };
                        returnViewModel.productCompairsList.Add(pc);
                        pc.SubeList = new List<UrunEdit2>();
                    }

                    var urunEdit2 = new UrunEdit2()
                    {
                        PkId = productId,
                        SubeId = subeId,
                        SubeName = subeName,
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
            }
            catch (Exception ex)
            {
                Singleton.WritingLogFile2("SPosSubeFiyatGuncellemeCRUD_GetByProductForSubeListEdit:", ex.Message.ToString(), "", ex.StackTrace);
                returnViewModel.IsSuccess = false;
                returnViewModel.ErrorList.Add("Bir hata oluştu.");
                return returnViewModel;
            }

            return returnViewModel;
        }
        public ActionResultMessages UpdateByProductForSubeList(UrunEditViewModel model)
        {
            var result = new ActionResultMessages() { IsSuccess = true, UserMessage = "İşlem Başarılı", };
            var kullaniciData = BussinessHelper.GetByUserInfoForId("", "", model.KullaniciId);
            var modelSube = SqlData.GetSube(model.SubeId);

            try
            {
                #region Local database'e veriler kaydetmek için kullanıldı.
                var sqlData = new SqlData(new SqlConnection(mF.NewConnectionString(subeIp, dbName, sqlKullaniciName, sqlKullaniciPassword)));
                var dataSubeUrunlist = mF.GetSubeDataWithQuery((mF.NewConnectionString(subeIp, dbName, sqlKullaniciName, sqlKullaniciPassword)), SqlData.getProductLocalDbForInsertSqlQuery());
                #endregion Local database'e veriler kaydetmek için kullanıldı.

                foreach (var modelProduct in model.productCompairsList)
                {
                    foreach (DataRow dataUrunSube in dataSubeUrunlist.Rows)
                    {
                        foreach (var price in modelProduct.SubeList)
                        {
                            if (modelProduct.ProductName == mF.RTS(dataUrunSube, "ProductName")
                                 && modelProduct.ProductGroup == mF.RTS(dataUrunSube, "ProductGroup")
                                 && price.SubeId == mF.RTD(dataUrunSube, "SubeId")
                                 && price.SubePriceValue != null
                                 && price.SubePriceValue != mF.RTD(dataUrunSube, "Price")
                                )
                            {
                                var productPrice = Convert.ToDecimal(price.SubePriceValue);
                                sqlData.ExecuteSql("update Product set Price = @par1 Where SubeId = @par2 and ProductGroup = @par3 and  ProductName = @par4 and Id=@par5",
                                 new object[]
                                      {
                                        productPrice,
                                        price.SubeId,
                                        modelProduct.ProductGroup,
                                        modelProduct.ProductName,
                                        price.PkId
                                     });

                                var oldPrice = Convert.ToDecimal(mF.RTS(dataUrunSube, "Price"));
                                sqlData.ExecuteSql(" Insert Into  ProductTarihce( ProductName, ProductGroup, Price, SubeId,SubeName, ProductPkId, IsUpdateDate,IsUpdateKullanici ) Values( @par1, @par2, @par3, @par4, @par5, @par6, @par7, @par8 )",
                                new object[] {
                                      mF.RTS(dataUrunSube, "ProductName"),
                                      mF.RTS(dataUrunSube, "ProductGroup"),
                                      oldPrice,
                                      price.SubeId,
                                      model.SubeList.Where(x=>x.SubeId==price.SubeId).FirstOrDefault().SubeAdi,
                                      mF.RTI(dataUrunSube, "Id"),
                                      DateTime.Now.Date.ToString("dd'/'MM'/'yyyy HH:mm"),
                                      kullaniciData.UserName
                              });
                                break;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Singleton.WritingLogFile2("SPosSubeFiyatGuncellemeCRUD_UpdateByProductForSubeList", ex.ToString(), null, ex.StackTrace);
                result.IsSuccess = false;
                result.UserMessage = "Bir hata oluştu.";
                return result;
            }

            return result;
        }
        #endregion Product update

        #region Choice1 update

        public UrunEditViewModel GetByChoice1ForSubeListEdit(string subeId_, string productId_, string subeIdGrupList)
        {
            var result = new ActionResultMessages { IsSuccess = true, UserMessage = "İşlem başarılı", };
            var returnViewModel = new UrunEditViewModel() { };
            returnViewModel.SubeList = new List<ProductCompairSube>();
            returnViewModel.productCompairsList = new List<ProductCompair>();
            returnViewModel.SubeIdGrupList = subeIdGrupList;
            returnViewModel.IsSuccess = true;
            var subeIdList = new List<long>();

            DataTable dataSubeChoice1Urunlist = mF.GetSubeDataWithQuery(mF.NewConnectionString(subeIp, dbName, sqlKullaniciName, sqlKullaniciPassword),
                                                                        SqlData.getLocalChoice1tListSqlQuery(subeId_, productId_, subeIdGrupList));
            try
            {
                #region Choice1 listesi

                foreach (DataRow item in dataSubeChoice1Urunlist.Rows)
                {
                    subeIdList.Add(mF.RTI(item, "SubeId"));
                }
                subeIdList = subeIdList.Distinct().ToList();

                foreach (var sube in subeIdList)
                {
                    var drSube = dataSubeChoice1Urunlist.Select("SubeId=" + sube)[0];
                    var subeName = mF.RTS(drSube, "SubeName");
                    returnViewModel.SubeList.Add(new ProductCompairSube
                    {
                        SubeId = sube,
                        SubeAdi = subeName,
                    });
                }

                foreach (DataRow item in dataSubeChoice1Urunlist.Rows)
                {
                    var productGroup = mF.RTS(item, "ProductGroup");
                    var productName = mF.RTS(item, "ProductName");
                    var subeName = mF.RTS(item, "SubeName");
                    var subeId = mF.RTI(item, "SubeId");
                    var id = mF.RTI(item, "PId");
                    var choice1Id = mF.RTI(item, "Id");
                    var productId = mF.RTI(item, "ProductId");
                    var choice1PkId = mF.RTI(item, "Choice1PkId");
                    var price = mF.RTD(item, "Product_Price");
                    var choicePrice = mF.RTD(item, "Choice1_Price");
                    var choice2VarMi = mF.RTI(item, "Choice2VarMi");
                    var choiceProductName = mF.RTS(item, "ChoiceProductName");

                    var pc = returnViewModel.productCompairsList.Where(x => x.ProductName == productName && x.ProductGroup == productGroup && x.ChoiceProductName == choiceProductName).FirstOrDefault();
                    if (pc == null)
                    {
                        pc = new ProductCompair
                        {
                            ProductGroup = productGroup,
                            ProductName = productName,
                            ProductId = productId,
                            Choice1Id = choice1Id,
                            Choice1PkId = choice1PkId,
                            ChoicePrice = choicePrice,
                            Choice2VarMi = choice2VarMi,
                            ChoiceProductName = choiceProductName
                        };
                        returnViewModel.productCompairsList.Add(pc);
                        pc.SubeList = new List<UrunEdit2>();
                    }

                    var urunEdit2 = new UrunEdit2()
                    {
                        PkId = choice1Id,
                        SubeId = subeId,
                        SubePriceValue = choicePrice,
                        SubeName = subeName,
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
            }
            catch (Exception ex)
            {
                Singleton.WritingLogFile2("SPosSubeFiyatGuncellemeCRUD_GetByChoice1ForSubeListEdit_Choice1", ex.ToString(), null, ex.StackTrace);
                returnViewModel.IsSuccess = false;
                returnViewModel.ErrorList.Add("Seçim-1'de bir hata oluştu.");
                return returnViewModel;
            }

            try
            {
                #region Options listesi

                #region Local database'e veriler kaydetmek için kullanıldı.
                DataTable dataSubeUrunOptionslist = mF.GetSubeDataWithQuery((mF.NewConnectionString(subeIp, dbName, sqlKullaniciName, sqlKullaniciPassword)), SqlData.getLocalOptionstListSqlQuery(subeId_, productId_, subeIdGrupList));
                #endregion Local database'e veriler kaydetmek için kullanıldı.

                //choice1 yok ama options varsa 
                if (dataSubeChoice1Urunlist.Rows.Count == 0)
                {
                    returnViewModel = new UrunEditViewModel() { };
                    returnViewModel.SubeList = new List<ProductCompairSube>();
                    subeIdList = new List<long>();

                    foreach (DataRow item in dataSubeUrunOptionslist.Rows)
                    {
                        subeIdList.Add(mF.RTI(item, "SubeId"));
                    }

                    subeIdList = subeIdList.Distinct().ToList();
                    foreach (var sube in subeIdList)
                    {
                        var drSube = dataSubeUrunOptionslist.Select("SubeId=" + sube)[0];
                        var subeName = mF.RTS(drSube, "SubeName");
                        returnViewModel.SubeList.Add(new ProductCompairSube
                        {
                            SubeId = sube,
                            SubeAdi = subeName,
                        });
                    }
                }
                //

                returnViewModel.productOptionsCompairsList = new List<ProductCompair>();
                foreach (DataRow item in dataSubeUrunOptionslist.Rows)
                {
                    var productGroup = mF.RTS(item, "ProductGroup");
                    var productName = mF.RTS(item, "ProductName");
                    var subeName = mF.RTS(item, "SubeName");
                    var subeId = mF.RTI(item, "SubeId");
                    var optionsId = mF.RTI(item, "Id");
                    var id = mF.RTI(item, "PId");
                    var productId = mF.RTI(item, "ProductId");
                    var optionsPrice = mF.RTD(item, "Option_Price");
                    var optionsName = mF.RTS(item, "OptionsName");

                    var pc = returnViewModel.productOptionsCompairsList.Where(x => x.ProductName == productName && x.ProductGroup == productGroup && x.OptionsProductName == optionsName).FirstOrDefault();
                    if (pc == null)
                    {
                        pc = new ProductCompair
                        {
                            ProductGroup = productGroup,
                            ProductName = productName,
                            ProductId = productId,
                            OptionsId = optionsId,
                            OptionsPrice = optionsPrice,
                            OptionsProductName = optionsName
                        };
                        returnViewModel.productOptionsCompairsList.Add(pc);
                        pc.SubeList = new List<UrunEdit2>();
                    }

                    var urunEdit2 = new UrunEdit2()
                    {
                        PkId = optionsId,
                        SubeId = subeId,
                        SubePriceValue = optionsPrice,
                        SubeName = subeName,
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
            }
            catch (Exception ex)
            {
                Singleton.WritingLogFile2("SPosSubeFiyatGuncellemeCRUD_GetByChoice1ForSubeListEdit_Options", ex.ToString(), null, ex.StackTrace);
                returnViewModel.IsSuccess = false;
                returnViewModel.ErrorList.Add("Seçeneklerde bir hata oluştu.");
                return returnViewModel;
            }
            returnViewModel.IsSuccess = true;
            return returnViewModel;
        }
        public ActionResultMessages UpdateByChoice1ForSubeList(UrunEditViewModel model)
        {
            var result = new ActionResultMessages()
            {
                IsSuccess = true,
                UserMessage = "İşlem Başarılı",
            };

            try
            {
                var kullaniciData = BussinessHelper.GetByUserInfoForId("", "", model.KullaniciId);

                #region Local database'e veriler kaydetmek için kullanıldı.
                var sqlData = new SqlData(new SqlConnection(mF.NewConnectionString(subeIp, dbName, sqlKullaniciName, sqlKullaniciPassword)));
                var dataSubeUrunlist = mF.GetSubeDataWithQuery(mF.NewConnectionString(subeIp, dbName, sqlKullaniciName, sqlKullaniciPassword),
                                                              SqlData.getLocalChoice1ListSqlQuery(model.productCompairsList.FirstOrDefault().ProductGroup, model.productCompairsList.FirstOrDefault().ProductName)).AsEnumerable().ToList();

                #endregion Local database'e veriler kaydetmek için kullanıldı.

                foreach (var modelProduct in model.productCompairsList)
                {
                    foreach (DataRow dataUrunSube in dataSubeUrunlist.ToList())
                    {
                        foreach (var price in modelProduct.SubeList)
                        {
                            var pn = mF.RTS(dataUrunSube, "ProductName");
                            var pn1 = mF.RTS(dataUrunSube, "ProductGroup");
                            var pn3 = mF.RTS(dataUrunSube, "ChoiceProductName");
                            var pn4 = mF.RTS(dataUrunSube, "SubeId");
                            var pn5 = mF.RTS(dataUrunSube, "Choice1_Price");
                            var pn6 = mF.RTS(dataUrunSube, "ProductId");
                            var pn7 = mF.RTS(dataUrunSube, "Id");

                            var cPkId = price.PkId;

                            if (modelProduct.ProductName == mF.RTS(dataUrunSube, "ProductName")
                                 && modelProduct.ProductGroup == mF.RTS(dataUrunSube, "ProductGroup")
                                 && modelProduct.ChoiceProductName == mF.RTS(dataUrunSube, "ChoiceProductName")
                                 && price.SubeId == mF.RTD(dataUrunSube, "SubeId")
                                 && price.SubePriceValue != mF.RTD(dataUrunSube, "Choice1_Price")
                                 && price.PkId == mF.RTI(dataUrunSube, "Id")
                                )
                            {
                                var chPrice = Convert.ToDecimal(price.SubePriceValue);
                                sqlData.ExecuteSql("Update choice1 set Price=@par1 Where Name=@par2 and SubeId=@par3 and Id=@par4",  //and ProductId=@par4",
                                 new object[]
                                      {
                                         chPrice,
                                         modelProduct.ChoiceProductName,
                                         price.SubeId,
                                         price.PkId,
                                         //modelProduct.ProductId
                                      });

                                var chPriceOld = Convert.ToDecimal(mF.RTD(dataUrunSube, "Choice1_Price"));
                                sqlData.ExecuteSql(" Insert Into  Choice1Tarihce( ProductId, Name, Price, SubeId, SubeName, Choice1PkId,IsUpdateDate,IsUpdateKullanici ) Values( @par1, @par2, @par3, @par4, @par5,@par6,@par7,@par8 )",
                                     new object[]
                                     {
                                         mF.RTI(dataUrunSube, "ProductId"),
                                         mF.RTS(dataUrunSube, "ChoiceProductName"),
                                         chPriceOld,
                                         model.SubeId,
                                         price.SubeName,
                                         mF.RTI(dataUrunSube, "Id"),
                                         DateTime.Now.Date.ToString("dd'/'MM'/'yyyy HH:mm"),
                                         kullaniciData.UserName
                                     });
                                break;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Singleton.WritingLogFile2("SPosSubeFiyatGuncellemeCRUD_UpdateByChoice1ForSubeList", ex.ToString(), null, ex.StackTrace);
                result.IsSuccess = false;
                result.UserMessage = "Bir hata oluştu.";
                return result;
            }

            return result;
        }
        public ActionResultMessages UpdateByOptionsForSubeList(UrunEditViewModel model, bool commonOptionsUpdated, string subeIdGrupList)
        {
            var result = new ActionResultMessages() { IsSuccess = true, UserMessage = "İşlem Başarılı", };

            try
            {
                var kullaniciData = BussinessHelper.GetByUserInfoForId("", "", model.KullaniciId);

                #region Local database'e veriler kaydetmek için kullanıldı.
                var productId = model.productOptionsCompairsList.Select(x => x.ProductId).FirstOrDefault();

                var sqlData = new SqlData(new SqlConnection(mF.NewConnectionString(subeIp, dbName, sqlKullaniciName, sqlKullaniciPassword)));
                DataTable dataSubeUrunlist;
                if (commonOptionsUpdated)
                {
                    dataSubeUrunlist = mF.GetSubeDataWithQuery(mF.NewConnectionString(subeIp, dbName, sqlKullaniciName, sqlKullaniciPassword),
                                        SqlData.getLocalCommonOptionstListSqlQuery(
                                                                                    model.productOptionsCompairsList.FirstOrDefault().ProductGroup,
                                                                                    model.productOptionsCompairsList.FirstOrDefault().ProductName,
                                                                                    model.productOptionsCompairsList.FirstOrDefault().OptionsProductName,
                                                                                    model.SubeIdGrupList));
                }
                else
                {
                    dataSubeUrunlist = mF.GetSubeDataWithQuery((mF.NewConnectionString(subeIp, dbName, sqlKullaniciName, sqlKullaniciPassword)), SqlData.getLocalOptionsListSqlQuery(productId));
                }

                #endregion Local database'e veriler kaydetmek için kullanıldı.

                foreach (var modelProduct in model.productOptionsCompairsList)
                {
                    foreach (DataRow dataUrunSube in dataSubeUrunlist.Rows)
                    {
                        foreach (var price in modelProduct.SubeList)
                        {
                            var pn = mF.RTS(dataUrunSube, "ProductName");
                            var pn1 = mF.RTS(dataUrunSube, "ProductGroup");
                            var pn3 = mF.RTS(dataUrunSube, "OptionsName");
                            var pn4 = mF.RTS(dataUrunSube, "SubeId");
                            var pn5 = mF.RTS(dataUrunSube, "Option_Price");
                            var pn6 = mF.RTS(dataUrunSube, "ProductId");
                            var pn7 = mF.RTS(dataUrunSube, "ProductId");

                            if (modelProduct.ProductName == mF.RTS(dataUrunSube, "ProductName")
                                 && modelProduct.ProductGroup == mF.RTS(dataUrunSube, "ProductGroup")
                                 && modelProduct.OptionsProductName == mF.RTS(dataUrunSube, "OptionsName")
                                 && price.SubeId == mF.RTD(dataUrunSube, "SubeId")
                                 && price.SubePriceValue != null
                                 && price.SubePriceValue != mF.RTD(dataUrunSube, "Option_Price")
                                )
                            {
                                var opPrice = Convert.ToDecimal(price.SubePriceValue);
                                sqlData.ExecuteSql("Update Options set Price = @par1 Where Name = @par2 and SubeId=@par3 and Id=@par4", //and ProductId=@par4",
                                 new object[]
                                      {
                                       opPrice,
                                       modelProduct.OptionsProductName,
                                       price.SubeId,
                                       price.PkId,
                                       //modelProduct.ProductId
                                      });

                                var oldPrice = Convert.ToDecimal(mF.RTD(dataUrunSube, "Option_Price"));
                                sqlData.ExecuteSql(" Insert Into  OptionsTarihce(ProductId, Name, Price, Category, SubeId, SubeName, OptionsPkId, IsUpdateDate,IsUpdateKullanici ) Values( @par1, @par2, @par3, @par4, @par5, @par6,@par7,@par8,@par9)",
                                   new object[]
                                   {
                                       mF.RTI(dataUrunSube, "ProductId"),
                                       mF.RTS(dataUrunSube, "OptionsName"),
                                       oldPrice,
                                       mF.RTD(dataUrunSube, "OptionsCategory"),
                                       price.SubeId,
                                       price.SubeName,
                                       mF.RTI(dataUrunSube, "Id"),
                                       DateTime.Now.Date.ToString("dd'/'MM'/'yyyy HH:mm"),
                                       kullaniciData.UserName
                                   });
                                break;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Singleton.WritingLogFile2("SPosSubeFiyatGuncellemeCRUD_UpdateByOptionsForSubeList", ex.ToString(), null, ex.StackTrace);
                result.IsSuccess = false;
                result.UserMessage = "Bir hata oluştu.";
                return result;
            }

            return result;
        }

        #endregion Choice1 update

        #region Choice2 update

        public UrunEditViewModel GetByChoice2ForSubeListEdit(int subeId_, int choice1Id_, string subeIdGrupList, string choice1Name)
        {
            var result = new ActionResultMessages { IsSuccess = true, UserMessage = "İşlem başarılı", };
            var returnViewModel = new UrunEditViewModel() { };
            returnViewModel.SubeList = new List<ProductCompairSube>();
            returnViewModel.productCompairsList = new List<ProductCompair>();
            returnViewModel.IsSuccess = true;
            returnViewModel.ErrorList = new List<string>();

            try
            {
                #region Choice1 listesi

                #region Local database'e veriler kaydetmek için kullanıldı.
                var sqlData = new SqlData(new SqlConnection(mF.NewConnectionString(subeIp, dbName, sqlKullaniciName, sqlKullaniciPassword)));
                DataTable dataSubeUrunlist = mF.GetSubeDataWithQuery(mF.NewConnectionString(subeIp, dbName, sqlKullaniciName, sqlKullaniciPassword), SqlData.GetLocalChoice2tListSqlQuery(subeId_, choice1Id_, subeIdGrupList, choice1Name));
                #endregion Local database'e veriler kaydetmek için kullanıldı.

                var subeIdList = new List<long>();
                foreach (DataRow item in dataSubeUrunlist.Rows)
                {
                    subeIdList.Add(mF.RTI(item, "SubeId"));
                }

                subeIdList = subeIdList.Distinct().ToList();
                foreach (var sube in subeIdList)
                {
                    var drSube = dataSubeUrunlist.Select("SubeId=" + sube)[0];
                    var subeName = mF.RTS(drSube, "SubeName");
                    returnViewModel.SubeList.Add(new ProductCompairSube
                    {
                        SubeId = sube,
                        SubeAdi = subeName,
                    });
                }

                foreach (DataRow item in dataSubeUrunlist.Rows)
                {
                    var productGroup = mF.RTS(item, "ProductGroup");
                    var productName = mF.RTS(item, "ProductName");
                    var subeName = mF.RTS(item, "SubeName");
                    var subeId = mF.RTI(item, "SubeId");
                    var choice1Id = mF.RTI(item, "Choice1PkId");
                    var choice2Id = mF.RTI(item, "Id");
                    var id = mF.RTI(item, "PId");
                    var productId = mF.RTI(item, "ProductId");
                    var price = mF.RTD(item, "Product_Price");
                    var choicePrice = mF.RTD(item, "Choice2_Price");
                    var choiceProductName = mF.RTS(item, "ChoiceProductName");

                    var pc = returnViewModel.productCompairsList.Where(x => x.ProductName == productName && x.ProductGroup == productGroup && x.ChoiceProductName == choiceProductName).FirstOrDefault();
                    if (pc == null)
                    {
                        pc = new ProductCompair
                        {
                            ProductGroup = productGroup,
                            ProductName = productName,
                            ProductId = productId,
                            Choice1Id = choice1Id,
                            Choice2Id = choice2Id,
                            ChoicePrice = choicePrice,
                            ChoiceProductName = choiceProductName
                        };
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
            }
            catch (Exception ex)
            {
                Singleton.WritingLogFile2("SPosSubeFiyatGuncellemeCRUD_GetByChoice2ForSubeListEdit_Choice2", ex.ToString(), null, ex.StackTrace);
                returnViewModel.IsSuccess = false;
                returnViewModel.ErrorList.Add("Seçim-2'de bir hata oluştu.");
                return returnViewModel;
            }

            return returnViewModel;
        }
        public ActionResultMessages UpdateByChoice2ForSubeList(UrunEditViewModel model)
        {
            var result = new ActionResultMessages() { IsSuccess = true, UserMessage = "İşlem Başarılı", };

            try
            {
                #region Local database'e veriler kaydetmek için kullanıldı.
                var sqlData = new SqlData(new SqlConnection(mF.NewConnectionString(subeIp, dbName, sqlKullaniciName, sqlKullaniciPassword)));
                //var dataSubeUrunlist = mF.GetSubeDataWithQuery(mF.NewConnectionString(subeIp, dbName, sqlKullaniciName, sqlKullaniciPassword), SqlData.getLocalChoice2UpdateListSqlQuery(model.productCompairsList.FirstOrDefault().Choice2Id));
                var dataSubeUrunlist = mF.GetSubeDataWithQuery(mF.NewConnectionString(subeIp, dbName, sqlKullaniciName, sqlKullaniciPassword), SqlData.getLocalChoice2UpdateListSqlQuery(0, 0, model.SubeIdGrupList, model.Choice1ProductName));
                #endregion Local database'e veriler kaydetmek için kullanıldı.

                foreach (var modelProduct in model.productCompairsList)
                {
                    foreach (DataRow dataUrunSube in dataSubeUrunlist.Rows)
                    {
                        foreach (var price in modelProduct.SubeList)
                        {
                            var pn = mF.RTS(dataUrunSube, "ProductName");
                            var pn1 = mF.RTS(dataUrunSube, "ProductGroup");
                            var pn3 = mF.RTS(dataUrunSube, "ChoiceProductName");
                            var pn4 = mF.RTS(dataUrunSube, "SubeId");
                            var pn5 = mF.RTS(dataUrunSube, "Choice2_Price");
                            var pn6 = mF.RTS(dataUrunSube, "ProductId");
                            var id = mF.RTS(dataUrunSube, "Id");
                            var subeAdi = model.SubeList.Where(x => x.SubeId == price.SubeId).FirstOrDefault().SubeAdi;

                            if (modelProduct.ProductName == mF.RTS(dataUrunSube, "ProductName")
                                 && modelProduct.ProductGroup == mF.RTS(dataUrunSube, "ProductGroup")
                                 && modelProduct.ChoiceProductName == mF.RTS(dataUrunSube, "ChoiceProductName")
                                 && price.SubeId == mF.RTD(dataUrunSube, "SubeId")
                                 && price.SubePriceValue != mF.RTD(dataUrunSube, "Choice2_Price")
                                )
                            {
                                var ch2Price = Convert.ToDecimal(price.SubePriceValue);
                                sqlData.ExecuteSql("Update choice2 set Price = @par1 Where Name = @par2 and SubeId=@par3 and Id=@par4",
                                 new object[]
                                      {
                                        ch2Price,
                                        modelProduct.ChoiceProductName,
                                        price.SubeId,
                                        id
                                     });

                                var oldCh2Price = Convert.ToDecimal(mF.RTD(dataUrunSube, "Choice2_Price"));
                                var kullaniciData = BussinessHelper.GetByUserInfoForId("", "", model.KullaniciId);
                                sqlData.ExecuteSql(" Insert Into  Choice2Tarihce( ProductId, Choice1Id, Name, Price, SubeId,SubeName, Choice2PkId, IsUpdateDate,IsUpdateKullanici ) Values( @par1, @par2, @par3, @par4, @par5,@par6,@par7,@par8,@par9 )",
                                    new object[]
                                    {
                                        mF.RTI(dataUrunSube, "ProductId"),
                                        mF.RTI(dataUrunSube, "Id"),
                                        mF.RTS(dataUrunSube, "ChoiceProductName"),
                                        oldCh2Price,
                                        model.SubeId,
                                        model.SubeList.Where(x=>x.SubeId==price.SubeId).FirstOrDefault().SubeAdi,
                                        mF.RTI(dataUrunSube, "Id"),
                                        DateTime.Now.Date.ToString("dd'/'MM'/'yyyy HH:mm"),
                                        kullaniciData.UserName
                                    });

                                break;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Singleton.WritingLogFile2("SPosSubeFiyatGuncellemeCRUD_UpdateByChoice2ForSubeList", ex.ToString(), null, ex.StackTrace);
                result.IsSuccess = false;
                result.UserMessage = "Bir hata oluştu.";
                return result;
            }

            return result;
        }

        #endregion Choice2 update


        #region Şubelerdeki ortak ürün(Product table) update yapma
        public UrunEditViewModel GetByOrtakUrunForSubeListEdit(string _productGroup, string _productName, string subeIdGrupList)
        {
            var result = new ActionResultMessages { IsSuccess = true, UserMessage = "İşlem başarılı", };
            var returnViewModel = new UrunEditViewModel() { };
            returnViewModel.SubeList = new List<ProductCompairSube>();
            returnViewModel.productCompairsList = new List<ProductCompair>();
            returnViewModel.IsSuccess = true;

            try
            {
                #region Local database'e veriler kaydetmek için kullanıldı.
                var sqlData = new SqlData(new SqlConnection(mF.NewConnectionString(subeIp, dbName, sqlKullaniciName, sqlKullaniciPassword)));
                DataTable dataSubeUrunlist = mF.GetSubeDataWithQuery(mF.NewConnectionString(subeIp, dbName, sqlKullaniciName, sqlKullaniciPassword), SqlData.getLocalOrtakProductListSqlQuery(_productGroup, _productName, subeIdGrupList));
                #endregion Local database'e veriler kaydetmek için kullanıldı.

                var subeIdList = new List<long>();
                foreach (DataRow item in dataSubeUrunlist.Rows)
                {
                    subeIdList.Add(mF.RTI(item, "SubeId"));
                }

                subeIdList = subeIdList.Distinct().ToList();

                foreach (var sube in subeIdList)
                {
                    var drSube = dataSubeUrunlist.Select("SubeId=" + sube)[0];
                    var subeName = mF.RTS(drSube, "SubeName");
                    returnViewModel.SubeList.Add(new ProductCompairSube
                    {
                        SubeId = sube,
                        SubeAdi = subeName,
                    });
                }

                foreach (DataRow item in dataSubeUrunlist.Rows)
                {
                    var productGroup = mF.RTS(item, "ProductGroup");
                    var productName = mF.RTS(item, "ProductName");
                    var subeName = mF.RTS(item, "SubeName");
                    var subePrice = mF.RTD(item, "Price");
                    var subeId = mF.RTI(item, "SubeId");
                    var productId = mF.RTI(item, "Id");
                    var optionsVarMi = mF.RTI(item, "OptionsVarMi");
                    var choice1VarMi = mF.RTI(item, "Choice1VarMi");
                    var choiceProductName = mF.RTS(item, "ChoiceProductName");

                    var pc = returnViewModel.productCompairsList.Where(x => x.ProductName == productName && x.ProductGroup == productGroup).FirstOrDefault();
                    if (pc == null)
                    {
                        pc = new ProductCompair
                        {
                            ProductGroup = productGroup,
                            ProductName = productName,
                            ProductId = productId,
                            OptionsVarMi = optionsVarMi,
                            Choice1VarMi = choice1VarMi
                        };
                        returnViewModel.productCompairsList.Add(pc);
                        pc.SubeList = new List<UrunEdit2>();
                    }

                    var urunEdit2 = new UrunEdit2()
                    {
                        PkId = productId,
                        SubeId = subeId,
                        SubeName = subeName,
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
            }
            catch (Exception ex)
            {
                Singleton.WritingLogFile2("SPosSubeFiyatGuncellemeCRUD_GetByOrtakUrunForSubeListEdit", ex.ToString(), null, ex.StackTrace);
                returnViewModel.IsSuccess = false;
                returnViewModel.ErrorList.Add("Bir hata oluştu.");
                return returnViewModel;
            }
            return returnViewModel;
        }

        #endregion  Şubelerdeki ortak ürün(Product table) update yapma

        #region Şubelerdeki ortak options(Options table) update yapma
        public UrunEditViewModel GetByCommonOptionsForSubeListEdit(string _productGroup, string _productName, string _optionsName, string subeIdGrupList)
        {
            var returnViewModel = new UrunEditViewModel() { };
            returnViewModel.SubeList = new List<ProductCompairSube>();
            returnViewModel.productCompairsList = new List<ProductCompair>();
            returnViewModel.IsSuccess = true;

            try
            {
                #region Options listesi
                DataTable dataSubeUrunOptionslist = mF.GetSubeDataWithQuery(mF.NewConnectionString(subeIp, dbName, sqlKullaniciName, sqlKullaniciPassword),
                                                         SqlData.getLocalCommonOptionstListSqlQuery(_productGroup, _productName, _optionsName, subeIdGrupList));

                var subeIdList = new List<long>();
                foreach (DataRow item in dataSubeUrunOptionslist.Rows)
                {
                    subeIdList.Add(mF.RTI(item, "SubeId"));
                }

                subeIdList = subeIdList.Distinct().ToList();
                foreach (var sube in subeIdList)
                {
                    var drSube = dataSubeUrunOptionslist.Select("SubeId=" + sube)[0];
                    var subeName = mF.RTS(drSube, "SubeName");
                    returnViewModel.SubeList.Add(new ProductCompairSube
                    {
                        SubeId = sube,
                        SubeAdi = subeName,
                    });
                }

                returnViewModel.productOptionsCompairsList = new List<ProductCompair>();
                foreach (DataRow item in dataSubeUrunOptionslist.Rows)
                {
                    var productGroup = mF.RTS(item, "ProductGroup");
                    var productName = mF.RTS(item, "ProductName");
                    var subeName = mF.RTS(item, "SubeName");
                    var subeId = mF.RTI(item, "SubeId");
                    var optionsId = mF.RTI(item, "Id");
                    var id = mF.RTI(item, "PId");
                    var productId = mF.RTI(item, "ProductId");
                    var optionsPrice = mF.RTD(item, "Option_Price");
                    var optionsName = mF.RTS(item, "OptionsName");

                    var pc = returnViewModel.productOptionsCompairsList.Where(x => x.ProductName == productName && x.ProductGroup == productGroup && x.OptionsProductName == optionsName).FirstOrDefault();
                    if (pc == null)
                    {
                        pc = new ProductCompair
                        {
                            ProductGroup = productGroup,
                            ProductName = productName,
                            ProductId = productId,
                            OptionsId = optionsId,
                            OptionsPrice = optionsPrice,
                            OptionsProductName = optionsName
                        };
                        returnViewModel.productOptionsCompairsList.Add(pc);
                        pc.SubeList = new List<UrunEdit2>();
                    }

                    var urunEdit2 = new UrunEdit2()
                    {
                        PkId = optionsId,
                        SubeId = subeId,
                        SubePriceValue = optionsPrice,
                        SubeName = subeName,
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
            }
            catch (Exception ex)
            {
                Singleton.WritingLogFile2("SPosSubeFiyatGuncellemeCRUD_GetByCommonOptionsForSubeListEdit", ex.ToString(), null, ex.StackTrace);
                returnViewModel.IsSuccess = false;
                returnViewModel.ErrorList.Add("Bir hata oluştu.");
                return returnViewModel;
            }

            return returnViewModel;
        }

        #endregion  Şubelerdeki ortak options(Options table) update yapma


        public ActionResultMessages IsUpdateProduct(string SubeIdGrupList, string kullaniciId)
        {
            var result = new ActionResultMessages() { IsSuccess = true, UserMessage = "İşlem Başarılı", };
            string updatesubeName = string.Empty;

            #region Local pruduct tablosundaki şubeler alınır 

            var updatedSProductSubeList = mF.GetSubeDataWithQuery(mF.NewConnectionString(subeIp, dbName, sqlKullaniciName, sqlKullaniciPassword), SqlData.getProductJoinSubesettingsSqlQuery(SubeIdGrupList));
            var yeniSubeIdGrupList = string.Empty;
            var kullaniciSubeYetkisi = UsersListCRUD.KullaniciSubeYetkiListesi(kullaniciId);
            foreach (DataRow itemSube in updatedSProductSubeList.Rows)
            {
                var updateSubeId = mF.RTS(itemSube, "SubeId");

                try
                {
                    //Kullanıcının şube yetki kontrolü.
                    if (kullaniciSubeYetkisi != null && kullaniciSubeYetkisi.FR_SubeListesi.Where(x => x.SubeID == Convert.ToInt32(updateSubeId)).Select(x => x.SubeID).Any())
                    {
                        var updateSubeIp = mF.RTS(itemSube, "SubeIp");
                        var updateDbName = mF.RTS(itemSube, "DBName");
                        updatesubeName = mF.RTS(itemSube, "SubeName");
                        var updateSqlKullaniciName = mF.RTS(itemSube, "SqlName");
                        var updateSqlKullaniciPassword = mF.RTS(itemSube, "SqlPassword");

                        //var aa = UpdateByProductForSubeList_(Convert.ToInt32(updateSubeId));
                        var updatedProduct = mF.GetSubeDataWithQuery(mF.NewConnectionString(updateSubeIp, updateDbName, updateSqlKullaniciName, updateSqlKullaniciPassword), SqlData.updatedProductSqlQuery(dbName, updateDbName, updateSubeId));
                        var updatedChoice1 = mF.GetSubeDataWithQuery(mF.NewConnectionString(updateSubeIp, updateDbName, updateSqlKullaniciName, updateSqlKullaniciPassword), SqlData.updatedChoice1SqlQuery(dbName, updateDbName, updateSubeId));
                        var updatedChoice2 = mF.GetSubeDataWithQuery(mF.NewConnectionString(updateSubeIp, updateDbName, updateSqlKullaniciName, updateSqlKullaniciPassword), SqlData.updatedChoice2SqlQuery(dbName, updateDbName, updateSubeId));
                        var updatedOptions = mF.GetSubeDataWithQuery(mF.NewConnectionString(updateSubeIp, updateDbName, updateSqlKullaniciName, updateSqlKullaniciPassword), SqlData.updatedOptionsSqlQuery(dbName, updateDbName, updateSubeId));

                        #region Product tablosunda Delete  yapar
                        var sqlData = new SqlData(new SqlConnection(mF.NewConnectionString(subeIp, dbName, sqlKullaniciName, sqlKullaniciPassword)));
                        sqlData.ExecuteSql("Delete Product where GuncellenecekSubeIdGrubu='" + SubeIdGrupList + "' and SubeId='" + updateSubeId + "'  ", new object[] { });
                        sqlData.ExecuteSql("Delete Choice1 where GuncellenecekSubeIdGrubu='" + SubeIdGrupList + "' and SubeId='" + updateSubeId + "'  ", new object[] { });
                        sqlData.ExecuteSql("Delete Choice2 where GuncellenecekSubeIdGrubu='" + SubeIdGrupList + "' and SubeId='" + updateSubeId + "'  ", new object[] { });
                        sqlData.ExecuteSql("Delete Options where GuncellenecekSubeIdGrubu='" + SubeIdGrupList + "' and SubeId='" + updateSubeId + "'  ", new object[] { });

                        #endregion Product tablosunda Delete yapar

                        var parsSubeId = SubeIdGrupList.TrimEnd(',').Split(',');
                        var subeId_ = string.Empty;

                        for (int i = 0; i < parsSubeId.Length; i++)
                        {
                            if (parsSubeId[i] != updateSubeId)
                            {
                                yeniSubeIdGrupList += parsSubeId[i] + ",";
                            }
                        }

                        sqlData.ExecuteSql("update Product set GuncellenecekSubeIdGrubu = @par1 Where GuncellenecekSubeIdGrubu = @par2",
                                new object[]
                                     {
                                    yeniSubeIdGrupList,
                                    //updateSubeId,
                                    SubeIdGrupList
                                    });
                    }
                }
                catch (Exception ex)
                {
                    Singleton.WritingLogFile2("SPosSubeFiyatGuncellemeCRUD_IsUpdateProduct", ex.ToString(), null, ex.StackTrace);
                    result.IsSuccess = false;
                    result.UserMessage = updatesubeName + " şubesine bağlantı kurulamadı. Lütfen şubenin aktif olduğundan emin olunuz.";
                    //return result;
                }
            }

            #endregion Local pruduct tablosundaki şubeler alınır 

            return result;
        }

        public ActionResultMessages IsUpdateProduct2(SubelereVeriGonderViewModel model, string kullaniciId)
        {
            var result = new ActionResultMessages() { IsSuccess = true, /*UserMessage = "İşlem Başarılı"*/ };
            result.ErrorList = new List<string>();
            result.SuccessAlertList = new List<string>();
            string SubeIdGrupList = model.SubeIdGrupList;
            string updatesubeName = string.Empty;


            if (model.SubeIdGrupList == "TumSubelereYay")
            {

                //var spiltSubeId = SubeIdGrupList.TrimEnd(',').Split(',').FirstOrDefault();
                //if (spiltSubeId.Count() == 1)
                //{
                //    // kontrol eklenebilir.
                //}

                //var subeId = model.GuncellenecekSubeGruplariList.FirstOrDefault().FiyatGuncellemsiHazirlananSubeList.FirstOrDefault().ID;
                var subeId = Convert.ToInt32(model.HedefSubeId);


                #region Local pruduct tablosundaki şubeler alınır 

                //var isSelectedUpdatedSubeList = model.GuncellenecekSubeGruplariList.FirstOrDefault().FiyatGuncellemsiHazirlananSubeList.Where(x => x.IsSelectedHazirlananSubeList == true).ToList();                
                //var isSelectedUpdatedSubeList = SqlData.GetIsActivAllSube();
                var isSelectedUpdatedSubeList = model.IsSelectedSubeList.Where(x => x.IsSelectedHedefSube == true).ToList();
                foreach (var isSelectedSubeId in isSelectedUpdatedSubeList)
                {
                    var updatedSProductSubeList = mF.GetSubeDataWithQuery(mF.NewConnectionString(subeIp, dbName, sqlKullaniciName, sqlKullaniciPassword), SqlData.getProductJoinSubesettingsSqlQuery2(subeId));
                    var yeniSubeIdGrupList = string.Empty;
                    var kullaniciSubeYetkisi = UsersListCRUD.KullaniciSubeYetkiListesi(kullaniciId);
                    //foreach (DataRow itemSube in updatedSProductSubeList.Rows)
                    //{
                    //var updateSubeId = mF.RTS(itemSube, "SubeId");
                    var updateSubeId = isSelectedSubeId.ID.ToString(); //mF.RTS(itemSube, "SubeId");
                    using (SqlConnection con = new SqlConnection(mF.NewConnectionString(subeIp, dbName, sqlKullaniciName, sqlKullaniciPassword)))
                    {
                        con.Open();
                        SqlTransaction transaction = con.BeginTransaction();
                        SqlData sqlData = new SqlData(con);

                        try
                        {
                            //Kullanıcının şube yetki kontrolü.
                            if (kullaniciSubeYetkisi != null && kullaniciSubeYetkisi.FR_SubeListesi.Where(x => x.SubeID == Convert.ToInt32(updateSubeId)).Select(x => x.SubeID).Any())
                            {
                                var getSube = SqlData.GetSube(isSelectedSubeId.ID);
                                var updateSubeIp = getSube.SubeIP; //mF.RTS(itemSube, "SubeIp");
                                var updateDbName = getSube.DBName;//mF.RTS(itemSube, "DBName");
                                updatesubeName = getSube.SubeName;//mF.RTS(itemSube, "SubeName");
                                var updateSqlKullaniciName = getSube.SqlName;//mF.RTS(itemSube, "SqlName");
                                var updateSqlKullaniciPassword = getSube.SqlPassword;// mF.RTS(itemSube, "SqlPassword");

                                //var aa = UpdateByProductForSubeList_(Convert.ToInt32(updateSubeId));
                                var updatedProduct = mF.GetSubeDataWithQuery(mF.NewConnectionString(updateSubeIp, updateDbName, updateSqlKullaniciName, updateSqlKullaniciPassword), SqlData.updatedProductSqlQuery(dbName, updateDbName, subeId.ToString()));
                                var updatedChoice1 = mF.GetSubeDataWithQuery(mF.NewConnectionString(updateSubeIp, updateDbName, updateSqlKullaniciName, updateSqlKullaniciPassword), SqlData.updatedChoice1SqlQuery(dbName, updateDbName, subeId.ToString()));
                                var updatedChoice2 = mF.GetSubeDataWithQuery(mF.NewConnectionString(updateSubeIp, updateDbName, updateSqlKullaniciName, updateSqlKullaniciPassword), SqlData.updatedChoice2SqlQuery(dbName, updateDbName, subeId.ToString()));
                                var updatedOptions = mF.GetSubeDataWithQuery(mF.NewConnectionString(updateSubeIp, updateDbName, updateSqlKullaniciName, updateSqlKullaniciPassword), SqlData.updatedOptionsSqlQuery(dbName, updateDbName, subeId.ToString()));

                                #region Product tablosunda Delete  yapar
                                ////var sqlData = new SqlData(new SqlConnection(mF.NewConnectionString(subeIp, dbName, sqlKullaniciName, sqlKullaniciPassword)));
                                //sqlData.ExecuteScalarTransactionSql("Delete Product where GuncellenecekSubeIdGrubu='" + SubeIdGrupList + "' and SubeId=" + updateSubeId + " select count(*) from Product where SubeId= " + updateSubeId, transaction);
                                //sqlData.ExecuteScalarTransactionSql("Delete Choice1 where GuncellenecekSubeIdGrubu='" + SubeIdGrupList + "' and SubeId=" + updateSubeId + " select count(*) from Choice1 where SubeId= " + updateSubeId, transaction);
                                //sqlData.ExecuteScalarTransactionSql("Delete Choice2 where GuncellenecekSubeIdGrubu='" + SubeIdGrupList + "' and SubeId=" + updateSubeId + " select count(*) from Choice2 where SubeId= " + updateSubeId, transaction);
                                //sqlData.ExecuteScalarTransactionSql("Delete Options where GuncellenecekSubeIdGrubu='" + SubeIdGrupList + "' and SubeId=" + updateSubeId + " select count(*) from Options where SubeId= " + updateSubeId, transaction);

                                #endregion Product tablosunda Delete yapar

                                //var parsSubeId = SubeIdGrupList.TrimEnd(',').Split(',');
                                //var subeId_ = string.Empty;
                                //for (int i = 0; i < parsSubeId.Length; i++)
                                //{
                                //    if (parsSubeId[i] != updateSubeId)
                                //    {
                                //        yeniSubeIdGrupList += parsSubeId[i] + ",";
                                //    }
                                //}

                                //sqlData.ExecuteScalarTransactionSql("BEGIN TRAN update Product set GuncellenecekSubeIdGrubu = @par1 Where GuncellenecekSubeIdGrubu = @par2 SELECT @@TRANCOUNT AS OpenTransactions COMMIT TRAN SELECT @@TRANCOUNT AS OpenTransactions", transaction, new object[]
                                //   {
                                //    yeniSubeIdGrupList,
                                //    //updateSubeId,
                                //    SubeIdGrupList
                                //   });
                                //sqlData.ExecuteScalarTransactionSql("BEGIN TRAN update Choice1 set GuncellenecekSubeIdGrubu = @par1 Where GuncellenecekSubeIdGrubu = @par2 SELECT @@TRANCOUNT AS OpenTransactions COMMIT TRAN SELECT @@TRANCOUNT AS OpenTransactions", transaction, new object[]
                                //{
                                //    yeniSubeIdGrupList,
                                //    SubeIdGrupList
                                //});
                                //sqlData.ExecuteScalarTransactionSql("BEGIN TRAN update Choice2 set GuncellenecekSubeIdGrubu = @par1 Where GuncellenecekSubeIdGrubu = @par2 SELECT @@TRANCOUNT AS OpenTransactions COMMIT TRAN SELECT @@TRANCOUNT AS OpenTransactions", transaction, new object[]
                                //{
                                //    yeniSubeIdGrupList,
                                //    SubeIdGrupList
                                //});
                                //sqlData.ExecuteScalarTransactionSql("BEGIN TRAN update Options set GuncellenecekSubeIdGrubu = @par1 Where GuncellenecekSubeIdGrubu = @par2 SELECT @@TRANCOUNT AS OpenTransactions COMMIT TRAN SELECT @@TRANCOUNT AS OpenTransactions", transaction, new object[]
                                //{
                                //    yeniSubeIdGrupList,
                                //    SubeIdGrupList
                                //});

                                //transaction.Commit();

                                SubeIdGrupList = yeniSubeIdGrupList;
                                result.SuccessAlertList.Add("(Başarılı) " + updatesubeName + " şubesine fiyat güncellemesi başarılı bir şekilde gönderildi." + " " + Environment.NewLine);
                            }
                        }
                        catch (Exception ex)
                        {
                            transaction.Rollback();
                            Singleton.WritingLogFile2("SPosSubeFiyatGuncellemeCRUD_IsUpdateProduct", ex.ToString(), null, ex.StackTrace);
                            result.IsSuccess = false;
                            result.ErrorList.Add(updatesubeName + " şubesine bağlantı kurulamadı veya bir hata oluştu. Lütfen şubenin aktif olduğundan emin olunuz." + " " + Environment.NewLine);
                            //return result;
                        }
                    }
                    //}
                }
                #endregion Local pruduct tablosundaki şubeler alınır 
            }
            else
            {
                #region Local pruduct tablosundaki şubeler alınır 

                var isSelectedUpdatedSubeList = model.GuncellenecekSubeGruplariList.FirstOrDefault().FiyatGuncellemsiHazirlananSubeList.Where(x => x.IsSelectedHazirlananSubeList == true).ToList();
                foreach (var isSelectedSubeId in isSelectedUpdatedSubeList)
                {
                    var updatedSProductSubeList = mF.GetSubeDataWithQuery(mF.NewConnectionString(subeIp, dbName, sqlKullaniciName, sqlKullaniciPassword), SqlData.getProductJoinSubesettingsSqlQuery2(isSelectedSubeId.ID));
                    var yeniSubeIdGrupList = string.Empty;
                    var kullaniciSubeYetkisi = UsersListCRUD.KullaniciSubeYetkiListesi(kullaniciId);
                    foreach (DataRow itemSube in updatedSProductSubeList.Rows)
                    {
                        var updateSubeId = mF.RTS(itemSube, "SubeId");
                        using (SqlConnection con = new SqlConnection(mF.NewConnectionString(subeIp, dbName, sqlKullaniciName, sqlKullaniciPassword)))
                        {
                            con.Open();
                            SqlTransaction transaction = con.BeginTransaction();
                            SqlData sqlData = new SqlData(con);

                            try
                            {
                                //Kullanıcının şube yetki kontrolü.
                                if (kullaniciSubeYetkisi != null && kullaniciSubeYetkisi.FR_SubeListesi.Where(x => x.SubeID == Convert.ToInt32(updateSubeId)).Select(x => x.SubeID).Any())
                                {
                                    var updateSubeIp = mF.RTS(itemSube, "SubeIp");
                                    var updateDbName = mF.RTS(itemSube, "DBName");
                                    updatesubeName = mF.RTS(itemSube, "SubeName");
                                    var updateSqlKullaniciName = mF.RTS(itemSube, "SqlName");
                                    var updateSqlKullaniciPassword = mF.RTS(itemSube, "SqlPassword");

                                    //var aa = UpdateByProductForSubeList_(Convert.ToInt32(updateSubeId));
                                    var updatedProduct = mF.GetSubeDataWithQuery(mF.NewConnectionString(updateSubeIp, updateDbName, updateSqlKullaniciName, updateSqlKullaniciPassword), SqlData.updatedProductSqlQuery(dbName, updateDbName, updateSubeId));
                                    var updatedChoice1 = mF.GetSubeDataWithQuery(mF.NewConnectionString(updateSubeIp, updateDbName, updateSqlKullaniciName, updateSqlKullaniciPassword), SqlData.updatedChoice1SqlQuery(dbName, updateDbName, updateSubeId));
                                    var updatedChoice2 = mF.GetSubeDataWithQuery(mF.NewConnectionString(updateSubeIp, updateDbName, updateSqlKullaniciName, updateSqlKullaniciPassword), SqlData.updatedChoice2SqlQuery(dbName, updateDbName, updateSubeId));
                                    var updatedOptions = mF.GetSubeDataWithQuery(mF.NewConnectionString(updateSubeIp, updateDbName, updateSqlKullaniciName, updateSqlKullaniciPassword), SqlData.updatedOptionsSqlQuery(dbName, updateDbName, updateSubeId));

                                    #region Product tablosunda Delete  yapar
                                    //var sqlData = new SqlData(new SqlConnection(mF.NewConnectionString(subeIp, dbName, sqlKullaniciName, sqlKullaniciPassword)));
                                    sqlData.ExecuteScalarTransactionSql("Delete Product where GuncellenecekSubeIdGrubu='" + SubeIdGrupList + "' and SubeId=" + updateSubeId + " select count(*) from Product where SubeId= " + updateSubeId, transaction);
                                    sqlData.ExecuteScalarTransactionSql("Delete Choice1 where GuncellenecekSubeIdGrubu='" + SubeIdGrupList + "' and SubeId=" + updateSubeId + " select count(*) from Choice1 where SubeId= " + updateSubeId, transaction);
                                    sqlData.ExecuteScalarTransactionSql("Delete Choice2 where GuncellenecekSubeIdGrubu='" + SubeIdGrupList + "' and SubeId=" + updateSubeId + " select count(*) from Choice2 where SubeId= " + updateSubeId, transaction);
                                    sqlData.ExecuteScalarTransactionSql("Delete Options where GuncellenecekSubeIdGrubu='" + SubeIdGrupList + "' and SubeId=" + updateSubeId + " select count(*) from Options where SubeId= " + updateSubeId, transaction);

                                    #endregion Product tablosunda Delete yapar

                                    var parsSubeId = SubeIdGrupList.TrimEnd(',').Split(',');
                                    var subeId_ = string.Empty;
                                    for (int i = 0; i < parsSubeId.Length; i++)
                                    {
                                        if (parsSubeId[i] != updateSubeId)
                                        {
                                            yeniSubeIdGrupList += parsSubeId[i] + ",";
                                        }
                                    }

                                    sqlData.ExecuteScalarTransactionSql("BEGIN TRAN update Product set GuncellenecekSubeIdGrubu = @par1 Where GuncellenecekSubeIdGrubu = @par2 SELECT @@TRANCOUNT AS OpenTransactions COMMIT TRAN SELECT @@TRANCOUNT AS OpenTransactions", transaction, new object[]
                                       {
                                        yeniSubeIdGrupList,
                                        //updateSubeId,
                                        SubeIdGrupList
                                       });
                                    sqlData.ExecuteScalarTransactionSql("BEGIN TRAN update Choice1 set GuncellenecekSubeIdGrubu = @par1 Where GuncellenecekSubeIdGrubu = @par2 SELECT @@TRANCOUNT AS OpenTransactions COMMIT TRAN SELECT @@TRANCOUNT AS OpenTransactions", transaction, new object[]
                                    {
                                        yeniSubeIdGrupList,
                                        SubeIdGrupList
                                    });
                                    sqlData.ExecuteScalarTransactionSql("BEGIN TRAN update Choice2 set GuncellenecekSubeIdGrubu = @par1 Where GuncellenecekSubeIdGrubu = @par2 SELECT @@TRANCOUNT AS OpenTransactions COMMIT TRAN SELECT @@TRANCOUNT AS OpenTransactions", transaction, new object[]
                                    {
                                        yeniSubeIdGrupList,
                                        SubeIdGrupList
                                    });
                                    sqlData.ExecuteScalarTransactionSql("BEGIN TRAN update Options set GuncellenecekSubeIdGrubu = @par1 Where GuncellenecekSubeIdGrubu = @par2 SELECT @@TRANCOUNT AS OpenTransactions COMMIT TRAN SELECT @@TRANCOUNT AS OpenTransactions", transaction, new object[]
                                    {
                                        yeniSubeIdGrupList,
                                        SubeIdGrupList
                                    });

                                    transaction.Commit();

                                    SubeIdGrupList = yeniSubeIdGrupList;
                                    result.SuccessAlertList.Add("(Başarılı) " + updatesubeName + " şubesine fiyat güncellemesi başarılı bir şekilde gönderildi." + " " + Environment.NewLine);
                                }
                            }
                            catch (Exception ex)
                            {
                                transaction.Rollback();
                                Singleton.WritingLogFile2("SPosSubeFiyatGuncellemeCRUD_IsUpdateProduct", ex.ToString(), null, ex.StackTrace);
                                result.IsSuccess = false;
                                result.ErrorList.Add(updatesubeName + " şubesine bağlantı kurulamadı veya bir hata oluştu. Lütfen şubenin aktif olduğundan emin olunuz." + " " + Environment.NewLine);
                                //return result;
                            }
                        }
                    }
                }
                #endregion Local pruduct tablosundaki şubeler alınır 
            }


            return result;
        }

        public ActionResultMessages DeleteTempProducData(string SubeIdGrupList)
        {
            var result = new ActionResultMessages() { IsSuccess = true, UserMessage = "İşlem Başarılı", };

            try
            {
                #region Product tablosunda Delete  yapar

                var sqlData = new SqlData(new SqlConnection(mF.NewConnectionString(subeIp, dbName, sqlKullaniciName, sqlKullaniciPassword)));
                sqlData.ExecuteSql("Delete Product where GuncellenecekSubeIdGrubu='" + SubeIdGrupList + "' ", new object[] { });
                sqlData.ExecuteSql("Delete Choice1 where GuncellenecekSubeIdGrubu='" + SubeIdGrupList + "'   ", new object[] { });
                sqlData.ExecuteSql("Delete Choice2 where GuncellenecekSubeIdGrubu='" + SubeIdGrupList + "' ", new object[] { });
                sqlData.ExecuteSql("Delete Options where GuncellenecekSubeIdGrubu='" + SubeIdGrupList + "' ", new object[] { });

                #endregion Product tablosunda Delete yapar
            }
            catch (Exception ex)
            {
                Singleton.WritingLogFile2("DeleteTempProducData", ex.ToString(), null, ex.StackTrace);
                result.IsSuccess = false;
                result.UserMessage = "İşlem Başarısız.";
            }

            return result;
        }


        public ActionResultMessages DeleteForSubeId(string SubeIdGrupList, string SubeId, string SubeAdiGrupList, string SubeName)
        {
            var result = new ActionResultMessages() { IsSuccess = true, UserMessage = "İşlem Başarılı", };

            var yeniSubeIdGrupList = string.Empty;
            var parsSubeId = SubeIdGrupList.TrimEnd(',').Split(',');
            var subeId_ = string.Empty;
            for (int i = 0; i < parsSubeId.Length; i++)
            {
                if (parsSubeId[i] != SubeId)
                {
                    yeniSubeIdGrupList += parsSubeId[i] + ",";
                }
            }


            var yeniSubeAdiGrupList = string.Empty;
            var parsSubeAdi = SubeAdiGrupList.TrimEnd(',').Split(',');
            var subeAdi_ = string.Empty;
            for (int i = 0; i < parsSubeAdi.Length; i++)
            {
                if (parsSubeAdi[i] != SubeName)
                {
                    yeniSubeAdiGrupList += parsSubeAdi[i] + ",";
                }
            }


            using (SqlConnection con = new SqlConnection(mF.NewConnectionString(subeIp, dbName, sqlKullaniciName, sqlKullaniciPassword)))
            {
                con.Open();
                SqlTransaction transaction = con.BeginTransaction();
                SqlData sqlData = new SqlData(con);
                try
                {
                    #region Product tablosunda Delete  yapar

                    //var sqlData = new SqlData(new SqlConnection(mF.NewConnectionString(subeIp, dbName, sqlKullaniciName, sqlKullaniciPassword)));
                    sqlData.ExecuteScalarTransactionSql("Delete Product where GuncellenecekSubeIdGrubu='" + SubeIdGrupList + "' and SubeId=" + SubeId + " select count(*) from Product where SubeId= " + SubeId, transaction);
                    sqlData.ExecuteScalarTransactionSql("BEGIN TRAN update Product set GuncellenecekSubeIdGrubu=@par1 , GuncellenecekSubeAdiGrubu=@par2  Where GuncellenecekSubeIdGrubu=@par3 and SubeId!=@par4 SELECT @@TRANCOUNT AS OpenTransactions COMMIT TRAN SELECT @@TRANCOUNT AS OpenTransactions", transaction, new object[]
                    {
                        yeniSubeIdGrupList,
                        yeniSubeAdiGrupList,
                        SubeIdGrupList,
                        SubeId
                    });

                    sqlData.ExecuteScalarTransactionSql("Delete Choice1 where GuncellenecekSubeIdGrubu='" + SubeIdGrupList + "' and SubeId=" + SubeId + " select count(*) from Product where SubeId= " + SubeId, transaction);
                    sqlData.ExecuteScalarTransactionSql("BEGIN TRAN update Choice1 set GuncellenecekSubeIdGrubu = @par1 , GuncellenecekSubeAdiGrubu=@par2  Where GuncellenecekSubeIdGrubu = @par3 and SubeId!=@par4 SELECT @@TRANCOUNT AS OpenTransactions COMMIT TRAN SELECT @@TRANCOUNT AS OpenTransactions", transaction, new object[]
                    {
                        yeniSubeIdGrupList,
                        yeniSubeAdiGrupList,
                        SubeIdGrupList,
                        SubeId
                    });

                    sqlData.ExecuteScalarTransactionSql("Delete Choice2 where GuncellenecekSubeIdGrubu='" + SubeIdGrupList + "' and SubeId=" + SubeId + " select count(*) from Product where SubeId= " + SubeId, transaction);
                    sqlData.ExecuteScalarTransactionSql("BEGIN TRAN update Choice2 set GuncellenecekSubeIdGrubu = @par1 , GuncellenecekSubeAdiGrubu=@par2  Where GuncellenecekSubeIdGrubu = @par3 and SubeId!=@par4 SELECT @@TRANCOUNT AS OpenTransactions COMMIT TRAN SELECT @@TRANCOUNT AS OpenTransactions", transaction, new object[]
                    {
                        yeniSubeIdGrupList,
                        yeniSubeAdiGrupList,
                        SubeIdGrupList,
                        SubeId
                    });
                    sqlData.ExecuteScalarTransactionSql("Delete Options where GuncellenecekSubeIdGrubu='" + SubeIdGrupList + "' and SubeId=" + SubeId + " select count(*) from Product where SubeId= " + SubeId, transaction);
                    sqlData.ExecuteScalarTransactionSql("BEGIN TRAN update Options  set GuncellenecekSubeIdGrubu = @par1 , GuncellenecekSubeAdiGrubu=@par2  Where GuncellenecekSubeIdGrubu = @par3 and SubeId!=@par4 SELECT @@TRANCOUNT AS OpenTransactions COMMIT TRAN SELECT @@TRANCOUNT AS OpenTransactions", transaction, new object[]
                    {
                        yeniSubeIdGrupList,
                        yeniSubeAdiGrupList,
                        SubeIdGrupList,
                        SubeId
                    });

                    transaction.Commit();

                    #endregion Product tablosunda Delete yapar
                }
                catch (Exception ex)
                {
                    transaction.Rollback();

                    Singleton.WritingLogFile2("DeleteForSubeId", ex.ToString(), null, ex.StackTrace);
                    result.IsSuccess = false;
                    result.UserMessage = "İşlem Başarısız.";
                }
            }

            return result;
        }
        #endregion (05.02.2022 toplantı sonrası) Seçili şubelerdeki pruduct, choice1,choice2,options tablolarınını merkez (local temp db'ye aktarma

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



        #region Şubeye fiyat gönder

        public ActionResultMessages UpdateByProductForSubeList_(int subeId)
        {
            Singleton.WritingLogFile2("SPosSubeFiyatGuncellemeCRUD_UpdateByProductForSubeList_Started", subeId.ToString(), null, " ");
            var result = new ActionResultMessages() { IsSuccess = true, UserMessage = "İşlem Başarılı", };
            //var kullaniciData = BussinessHelper.GetByUserInfoForId("", "", model.KullaniciId);
            var modelSube = SqlData.GetSube(subeId);

            try
            {

                var tempData = mF.GetSubeDataWithQuery((mF.NewConnectionString(subeIp, dbName, sqlKullaniciName, sqlKullaniciPassword)), SqlData.getProductLocalDbSqlQuery(subeId));

                #region Local database'e veriler kaydetmek için kullanıldı.
                var sqlData = new SqlData(new SqlConnection(mF.NewConnectionString(modelSube.SubeIP, modelSube.DBName, modelSube.SqlName, modelSube.SqlPassword)));
                var dataSubeUrunlist = mF.GetSubeDataWithQuery(mF.NewConnectionString(modelSube.SubeIP, modelSube.DBName, modelSube.SqlName, modelSube.SqlPassword), SqlData.getProductLocalDbForInsertSqlQuery_());
                #endregion Local database'e veriler kaydetmek için kullanıldı.

                foreach (DataRow modelProduct in tempData.Rows /*model.productCompairsList*/)
                {
                    foreach (DataRow dataUrunSube in dataSubeUrunlist.Rows)
                    {
                        var pn = mF.RTS(modelProduct, "ProductName");
                        var pn1 = mF.RTS(dataUrunSube, "ProductName");

                        var pg = mF.RTS(modelProduct, "ProductGroup");
                        var pg1 = mF.RTS(dataUrunSube, "ProductGroup");

                        var sId = mF.RTD(modelProduct, "SubeId");
                        var sId1 = mF.RTD(dataUrunSube, "SubeId");

                        var p = mF.RTD(modelProduct, "Price");
                        var p1 = mF.RTD(modelProduct, "Price");
                        var p2 = mF.RTD(dataUrunSube, "Price");


                        //foreach (var price in modelProduct.SubeList)
                        //{
                        if (mF.RTS(modelProduct, "ProductName") == mF.RTS(dataUrunSube, "ProductName")
                             && mF.RTS(modelProduct, "ProductGroup") == mF.RTS(dataUrunSube, "ProductGroup")
                             //&& mF.RTD(modelProduct, "SubeId") == mF.RTD(dataUrunSube, "SubeId")
                             && mF.RTD(modelProduct, "Price") != null
                             && mF.RTD(modelProduct, "Price") != mF.RTD(dataUrunSube, "Price")
                            )
                        {
                            var productPrice = Convert.ToDecimal(mF.RTD(modelProduct, "Price"));
                            sqlData.ExecuteSql("update Product set Price = @par1 Where SubeId = @par2 and ProductGroup = @par3 and  ProductName = @par4 ",
                             new object[]
                                  {
                                        productPrice,
                                        mF.RTD(modelProduct, "SubeId"),
                                        mF.RTS(modelProduct, "ProductGroup"),
                                       mF.RTS(modelProduct, "ProductName"),
                                        //mF.RTD(modelProduct, "ProductPkId")
                                 });
                        }
                        //}
                    }
                }
            }
            catch (Exception ex)
            {
                Singleton.WritingLogFile2("SPosSubeFiyatGuncellemeCRUD_UpdateByProductForSubeList_", ex.ToString(), null, ex.StackTrace);
                result.IsSuccess = false;
                result.UserMessage = "Bir hata oluştu.";
                return result;
            }

            return result;
        }


        #endregion

    }
}