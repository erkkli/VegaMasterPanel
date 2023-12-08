using SefimV2.Helper;
using SefimV2.ViewModels.SefimPanelSablonFiyatGuncelle;
using SefimV2.ViewModels.SPosVeriGonderimi;
using SefimV2.ViewModels.SubeSettings;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web.Configuration;

namespace SefimV2.Models
{
    public class SPosSubeSablonFiyatGuncellemeCRUD
    {
        readonly ModelFunctions mf = new ModelFunctions();
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
            mf.SqlConnOpen();
            var dt = mf.DataTable("Select Id, SubeName,DBName, * From SubeSettings Where AppDbTypeStatus in (0,1) and Status=1 ");
            mf.SqlConnClose();
            foreach (DataRow r in dt.Rows)
            {
                SubeSettingsViewModel model = new SubeSettingsViewModel
                {
                    ID = Convert.ToInt32(mf.RTS(r, "Id")),
                    SubeName = (mf.RTS(r, "SubeName")),
                    SubeIP = mf.RTS(r, "SubeIP"),
                    SqlName = mf.RTS(r, "SqlName"),
                    SqlPassword = mf.RTS(r, "SqlPassword"),
                    DBName = mf.RTS(r, "DBName"),
                    FirmaID = mf.RTS(r, "FirmaID")
                };
                //model.Status = Convert.ToBoolean(modelFunctions.RTS(r, "Status"));
                //model.AppDbType = Convert.ToInt32(modelFunctions.RTS(r, "AppDbType"));
                //model.AppDbTypeStatus = Convert.ToBoolean(modelFunctions.RTS(r, "AppDbTypeStatus"));
                liste.Add(model);
            }

            return liste;
        }
        public SubelereVeriGonderViewModel GetSubeListIsSelected()
        {
            var liste = new SubelereVeriGonderViewModel();
            liste.IsSelectedSubeList = new List<SubeSettingsViewModel>();
            mf.SqlConnOpen();
            var dt = mf.DataTable("Select Id, SubeName,DBName, * From SubeSettings Where AppDbTypeStatus in (0,1) and Status=1");
            foreach (DataRow r in dt.Rows)
            {
                SubeSettingsViewModel model = new SubeSettingsViewModel();
                model.ID = Convert.ToInt32(mf.RTS(r, "Id"));
                model.SubeName = (mf.RTS(r, "SubeName"));
                model.DBName = mf.RTS(r, "DBName");
                liste.IsSelectedSubeList.Add(model);
            }
            mf.SqlConnClose();

            return liste;
        }
        public SubelereVeriGonderViewModel GetLocalSubeListIsSelectedRemovePreviousList()
        {
            mf.SqlConnOpen();

            #region Local pruduct tablosundaki şubeler alınır 
            // Temp Pruduct tablosuna eklenen subeleri çeker güncellemeye gidecek şube listesine dahil edilmez.

            var localSubeList = mf.DataTable("Select SubeId, SubeName from ProductTemplatePrice group by SubeName, SubeId");
            var listeLocal = new SubelereVeriGonderViewModel();
            listeLocal.IsSelectedSubeList = new List<SubeSettingsViewModel>();
            foreach (DataRow item in localSubeList.Rows)
            {
                SubeSettingsViewModel model = new SubeSettingsViewModel();
                model.ID = Convert.ToInt32(mf.RTS(item, "SubeId"));
                model.SubeName = (mf.RTS(item, "SubeName"));
                listeLocal.IsSelectedSubeList.Add(model);
            }

            #endregion Local pruduct tablosundaki şubeler alınır 

            var dt = mf.DataTable("Select Id, SubeName,DBName, * From SubeSettings Where AppDbTypeStatus in (0,1) and Status=1");
            var liste = new SubelereVeriGonderViewModel();
            liste.IsSelectedSubeList = new List<SubeSettingsViewModel>();
            foreach (DataRow r in dt.Rows)
            {
                SubeSettingsViewModel model = new SubeSettingsViewModel();
                model.ID = Convert.ToInt32(mf.RTS(r, "Id"));
                model.SubeName = (mf.RTS(r, "SubeName"));
                model.DBName = mf.RTS(r, "DBName");
                if (!listeLocal.IsSelectedSubeList.Where(x => x.ID == model.ID).Any())
                {
                    liste.IsSelectedSubeList.Add(model);
                }
            }
            mf.SqlConnClose();

            return liste;
        }
        #endregion Şube listesi

        #region Sablon Create - tüm şubelerdeki sablon tablosunu local db'ye çeker
        public ActionResultMessages SubeSablonCreate()
        {
            var result = new ActionResultMessages { IsSuccess = true, UserMessage = "İşlem başarılı", };

            try
            {
                var resultSubeList = new SPosSubeSablonFiyatGuncellemeCRUD().GetSubeList();
                var sqlDataPanel = new SqlData(new SqlConnection(mf.NewConnectionString(subeIp, dbName, sqlKullaniciName, sqlKullaniciPassword)));

                foreach (var sube in resultSubeList)
                {
                    var dataSubeSablon = mf.GetSubeDataWithQuery((mf.NewConnectionString(sube.SubeIP, sube.DBName, sube.SqlName, sube.SqlPassword)), SqlData.getSablonDbSqlQuery());

                    foreach (DataRow sablon in dataSubeSablon.Rows)
                    {
                        sqlDataPanel.ExecuteSql("Insert Into  ProductTemplate( Name,SubeId,SubeName,ProductTemplatePkId )Values( @par1, @par2, @par3, @par4)",
                            new object[]
                            {
                                mf.RTS(sablon, "Name"),
                                sube.ID,
                                sube.SubeName,
                                mf.RTS(sablon, "Id"),
                            });
                    }
                }
            }
            catch (Exception ex)
            {
                Singleton.WritingLogFile2("SPosSubeSablonFiyatGuncellemeCRUD_SubeSablonCreate:", ex.Message.ToString(), "", ex.StackTrace);
                return new ActionResultMessages { IsSuccess = false, UserMessage = "Şubelerden şablon bilgisi alınamadı." };
            }

            return result;
        }
        public ActionResultMessages SubeSablonDeleteCreate(string kullaniciId)
        {
            var result = new ActionResultMessages { IsSuccess = true, UserMessage = "İşlem başarılı", };

            try
            {
                var resultSubeList = new SPosSubeSablonFiyatGuncellemeCRUD().GetSubeList();
                var sqlDataPanel = new SqlData(new SqlConnection(mf.NewConnectionString(subeIp, dbName, sqlKullaniciName, sqlKullaniciPassword)));

                //Template Tablosunu siler.
                sqlDataPanel.ExecuteSql("Delete ProductTemplate", new object[] { });

                //Kullanıcı şubeye yetkili mi
                var kullaniciSubeYetkisi = UsersListCRUD.KullaniciSubeYetkiListesi(kullaniciId);

                foreach (var sube in resultSubeList)
                {
                    if (kullaniciSubeYetkisi != null && kullaniciSubeYetkisi.FR_SubeListesi.Where(x => x.SubeID == Convert.ToInt32(sube.ID)).Select(x => x.SubeID).Any())
                    {
                        var dataSubeSablon = mf.GetSubeDataWithQuery((mf.NewConnectionString(sube.SubeIP, sube.DBName, sube.SqlName, sube.SqlPassword)), SqlData.getSablonDbSqlQuery());
                        foreach (DataRow sablon in dataSubeSablon.Rows)
                        {
                            sqlDataPanel.ExecuteSql("Insert Into ProductTemplate( Name,SubeId,SubeName,ProductTemplatePkId )Values( @par1, @par2, @par3, @par4)",
                                new object[]
                                {
                                    mf.RTS(sablon, "Name"),
                                    sube.ID,
                                    sube.SubeName,
                                    mf.RTS(sablon, "Id"),
                                });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Singleton.WritingLogFile2("SPosSubeSablonFiyatGuncellemeCRUD_SubeSablonDeleteCreate:", ex.Message.ToString(), "", ex.StackTrace);
                return new ActionResultMessages { IsSuccess = false, UserMessage = "Şubelerden şablon bilgisi alınamadı." };
            }

            return result;
        }
        public SablonSubeViewModel GetBySablonList()
        {
            var sablonList = new SablonSubeViewModel();
            sablonList.SablonSubeList = new List<SablonSube>();
            try
            {
                var dataSablonList = mf.GetSubeDataWithQuery(mf.NewConnectionString(subeIp, dbName, sqlKullaniciName, sqlKullaniciPassword), SqlData.getSablonListLocalDbSqlQuery());

                foreach (DataRow item in dataSablonList.Rows)
                {
                    var model = new SablonSube
                    {
                        Id = mf.RTI(item, "Id"),
                        SubeId = mf.RTI(item, "SubeId"),
                        SubeAdi = mf.RTS(item, "SubeName"),
                        SablonAdi = mf.RTS(item, "Name"),
                        ProductTemplatePkId = mf.RTI(item, "ProductTemplatePkId")
                    };
                    sablonList.SablonSubeList.Add(model);
                }

                var distinctSablonList = sablonList.SablonSubeList.Select(x => x.SablonAdi).Distinct().ToList();

                return sablonList;
            }
            catch (Exception ex)
            {
                Singleton.WritingLogFile2("SPosSubeSablonFiyatGuncellemeCRUD_GetBySablonList:", ex.Message.ToString(), "", ex.StackTrace);
                return sablonList;
            }
        }
        public SablonSubeViewModel GetEklenmisSablonFiyatSubeList(string kullaniciId)
        {
            #region Old           
            //mf.SqlConnOpen();
            //#region Local pruduct tablosundaki şubeler alınır 
            //// Temp Pruduct tablosuna eklenen subeleri çeker güncellemeye gidecek şube listesine dahil edilmez.
            //var localSubeList = mf.DataTable("Select SubeId, SubeName,TemplateName from ProductTemplatePrice group by SubeName, SubeId,TemplateName");
            //var sablonList = new SablonSubeViewModel();
            //sablonList.FiyatGuncellemsiHazirlananSubeList = new List<SablonSube>();
            //foreach (DataRow item in localSubeList.Rows)
            //{
            //    var model = new SablonSube();
            //    model.SubeId = Convert.ToInt32(mf.RTS(item, "SubeId"));
            //    model.SubeAdi = (mf.RTS(item, "SubeName"));
            //    model.SablonAdi = mf.RTS(item, "TemplateName");
            //    model.ProductTemplatePkId = mf.RTI(item, "ProductTemplatePkId");
            //    sablonList.FiyatGuncellemsiHazirlananSubeList.Add(model);
            //}
            //#endregion Local pruduct tablosundaki şubeler alınır 
            //var dt = mf.DataTable("SELECT [Id],[Name],[Aktarildi],[SubeId],[SubeName],[ProductTemplatePkId],[IsUpdate] FROM [dbo].[ProductTemplate]");
            //sablonList.SablonSubeList = new List<SablonSube>();
            //foreach (DataRow r in dt.Rows)
            //{
            //    var model = new SablonSube();
            //    model.SubeId = Convert.ToInt32(mf.RTS(r, "Id"));
            //    model.SubeAdi = (mf.RTS(r, "SubeName"));
            //    model.SablonAdi = mf.RTS(r, "Name");
            //    model.ProductTemplatePkId = mf.RTI(r, "ProductTemplatePkId");
            //    if (!sablonList.SablonSubeList.Where(x => x.SablonAdi == model.SablonAdi && x.SubeAdi == model.SubeAdi).Any())
            //    {
            //        if (!sablonList.SablonSubeList.Where(x => x.SubeId == model.SubeId).Any())
            //        {
            //            sablonList.SablonSubeList.Add(model);
            //        }
            //    }
            //}
            //mf.SqlConnClose();
            //var sablonList_ = sablonList.SablonSubeList.Select(x => x.SablonAdi).Distinct().ToList();
            //var sablonLis = sablonList.SablonSubeList.OrderBy(x => x.SablonAdi).ToList();
            //return sablonList;
            #endregion

            #region Local pruduct tablosundaki şubeler alınır 
            // Temp Pruduct tablosuna eklenen subeleri çeker güncellemeye gidecek şube listesine dahil edilmez.
            /*Önceki Gruplama*/
            // var sqlQuery = "Select SubeId, SubeName from Product group by SubeName, SubeId";

            var sqlQuery = "Select GuncellenecekSubeAdiGrubu, GuncellenecekSubeIdGrubu,TemplateName From SablonProduct Group By GuncellenecekSubeAdiGrubu, GuncellenecekSubeIdGrubu,TemplateName";
            mf.SqlConnOpen();
            var localSubeList = mf.DataTable(sqlQuery);
            mf.SqlConnClose();

            //var listeLocal = new SubelereVeriGonderViewModel();
            var listLocal = new SablonSubeViewModel();
            listLocal.IsSelectedSubeList = new List<SubeSettingsViewModel>();
            //listeLocal.FiyatGuncellemsiHazirlananSubeList = new List<SubeSettingsViewModel>();
            listLocal.GuncellenecekSubeGruplariList = new List<GuncellenecekSubeGruplari>();
            GuncellenecekSubeGruplari guncellenecekSubeGruplari = new GuncellenecekSubeGruplari
            {
                FiyatGuncellemsiHazirlananSubeList = new List<SubeSettingsViewModel>()
            };

            //Kullanıcı şubeye yetkili mi
            var kullaniciSubeYetkisi = UsersListCRUD.KullaniciSubeYetkiListesi(kullaniciId);

            foreach (DataRow item in localSubeList.Rows)
            {
                guncellenecekSubeGruplari = new GuncellenecekSubeGruplari
                {
                    FiyatGuncellemsiHazirlananSubeList = new List<SubeSettingsViewModel>()
                };
                guncellenecekSubeGruplari.GuncellenecekSubeGrupId = mf.RTS(item, "GuncellenecekSubeIdGrubu");
                guncellenecekSubeGruplari.GuncellenecekSubeGrupAdi = mf.RTS(item, "GuncellenecekSubeAdiGrubu");
                guncellenecekSubeGruplari.GuncellenecekSubeSablonAdi = mf.RTS(item, "TemplateName");
                listLocal.GuncellenecekSubeGruplariList.Add(guncellenecekSubeGruplari);

                var parsSubeId = guncellenecekSubeGruplari.GuncellenecekSubeGrupId.TrimEnd(',').Split(',');
                var parsSubeAdi = guncellenecekSubeGruplari.GuncellenecekSubeGrupAdi.TrimEnd(',').Split(',');

                //Ürün ekle sayfasından gelen kayıtlarda NULL olarak set edildiğinden dolayı gruplamaya dahil olamıyor.
                if (!string.IsNullOrEmpty(guncellenecekSubeGruplari.GuncellenecekSubeGrupId))
                {
                    for (int i = 0; i < parsSubeId.Length; i++)
                    {
                        if (kullaniciSubeYetkisi != null && kullaniciSubeYetkisi.FR_SubeListesi.Where(x => x.SubeID == Convert.ToInt32(parsSubeId[i])).Select(x => x.SubeID).Any())
                        {
                            SubeSettingsViewModel modelGuncellenecekSube = new SubeSettingsViewModel
                            {
                                ID = Convert.ToInt32(parsSubeId[i]),
                                SubeName = parsSubeAdi[i],
                                SablonName = guncellenecekSubeGruplari.GuncellenecekSubeSablonAdi
                            };
                            guncellenecekSubeGruplari.FiyatGuncellemsiHazirlananSubeList.Add(modelGuncellenecekSube);
                            //listeLocal.FiyatGuncellemsiHazirlananSubeList.Add(model);
                        }
                    }
                }
            }

            #endregion Local pruduct tablosundaki şubeler alınır 
            mf.SqlConnOpen();
            //var dt = mf.DataTable("Select Id, SubeName,DBName, * From SubeSettings Where AppDbTypeStatus in (0,1) and Status=1 ");
            var dt = mf.DataTable("SELECT [Id],[Name],[Aktarildi],[SubeId],[SubeName],[ProductTemplatePkId],[IsUpdate] FROM [dbo].[ProductTemplate] order by Name desc");
            mf.SqlConnClose();

            foreach (DataRow r in dt.Rows)
            {
                var subeSablonName = mf.RTS(r, "Name");
                var subeId = Convert.ToInt32(mf.RTS(r, "SubeId"));
                var girmeyecekIdList = listLocal.GuncellenecekSubeGruplariList.SelectMany(x => x.FiyatGuncellemsiHazirlananSubeList.Where(y => y.ID == subeId && y.SablonName == subeSablonName).Select(y => y.ID).ToList());
                var guncellenenSube = listLocal.GuncellenecekSubeGruplariList.Select(x => x.FiyatGuncellemsiHazirlananSubeList.Select(z => z.ID == subeId).FirstOrDefault());
                var dd = listLocal.GuncellenecekSubeGruplariList.Select(x => x.FiyatGuncellemsiHazirlananSubeList.Where(y => y.ID == subeId).Select(z => z.ID == subeId).FirstOrDefault());
                var girmeyecekSablonList = listLocal.GuncellenecekSubeGruplariList.SelectMany(x => x.FiyatGuncellemsiHazirlananSubeList.Select(y => y.SablonName).ToList());
                var girmeyecekSubeIdList = listLocal.GuncellenecekSubeGruplariList.SelectMany(x => x.FiyatGuncellemsiHazirlananSubeList.Select(y => y.ID == subeId).ToList());

                if (!girmeyecekSablonList.Contains(subeSablonName) || !girmeyecekIdList.Contains(subeId))
                {
                    if (kullaniciSubeYetkisi != null && kullaniciSubeYetkisi.FR_SubeListesi.Where(x => x.SubeID == Convert.ToInt32(subeId)).Select(x => x.SubeID).Any())
                    {
                        SubeSettingsViewModel model = new SubeSettingsViewModel
                        {
                            ID = Convert.ToInt32(mf.RTS(r, "SubeId")),
                            SubeName = (mf.RTS(r, "SubeName")),
                            DBName = mf.RTS(r, "DBName"),
                            SablonName = mf.RTS(r, "Name"),
                            ProductTemplatePkId = mf.RTI(r, "ProductTemplatePkId")
                        };
                        listLocal.IsSelectedSubeList.Add(model);
                    }
                }
            }

            if (listLocal.IsSelectedSubeList == null || listLocal.IsSelectedSubeList.Count() == 0)
            {
                SubeSettingsViewModel model = new SubeSettingsViewModel();
                listLocal.IsSelectedSubeList = new List<SubeSettingsViewModel>();
                listLocal.IsSelectedSubeList.Add(model);
            }

            return listLocal;
        }
        //
        public ActionResultMessages SablonProductChoice1Choice2OptionsInsertLocalDb(SablonSubeViewModel model, string isSelectedSubeId, string isSelectedSubeAdi, string kullaniciId)
        {
            var result = new ActionResultMessages { IsSuccess = true, UserMessage = "İşlem başarılı", };
            var sablonName = model.IsSelectedSubeList.Where(y => y.IsSelectedHedefSube).Select(x => x.SablonName).FirstOrDefault();
            var sablonProductPkId = model.IsSelectedSubeList.Where(y => y.IsSelectedHedefSube).Select(x => x.ProductTemplatePkId).FirstOrDefault();
            //Kullanıcının yetkili olduğu şube.
            var kullaniciSubeYetkisi = UsersListCRUD.KullaniciSubeYetkiListesi(kullaniciId);

            foreach (var isSelectedSubeItem in model.IsSelectedSubeList.Where(x => x.IsSelectedHedefSube))
            {
                if (kullaniciSubeYetkisi != null && kullaniciSubeYetkisi.FR_SubeListesi.Where(x => x.SubeID == Convert.ToInt32(isSelectedSubeItem.ID)).Select(x => x.SubeID).Any())
                {
                    var modelSube = SqlData.GetSube(isSelectedSubeItem.ID);
                    var sqlData = new SqlData(new SqlConnection(mf.NewConnectionString(subeIp, dbName, sqlKullaniciName, sqlKullaniciPassword)));
                    var dataSubeUrunlist = mf.GetSubeDataWithQuery(mf.NewConnectionString(modelSube.SubeIP, modelSube.DBName, modelSube.SqlName, modelSube.SqlPassword), SqlData.getProductSqlQuery());
                    var dataSubeSablon = mf.GetSubeDataWithQuery(mf.NewConnectionString(modelSube.SubeIP, modelSube.DBName, modelSube.SqlName, modelSube.SqlPassword), SqlData.getProductTemplatePriceSqlQuery(sablonName));
                    var sablonVarMi = dataSubeSablon.AsEnumerable().ToList();

                    if (sablonVarMi != null && sablonVarMi.Count > 0)
                    {
                        foreach (DataRow item in dataSubeSablon.Rows)
                        {
                            //ProductTemplatePrice
                            sqlData.ExecuteSql("Insert Into ProductTemplatePrice( TemplateId,TemplateName,ProductId,Choice1Id,Choice2Id,OptionsId,Price,SubeId,SubeName,ProductTemplatePricePkId,GuncellenecekSubeIdGrubu,GuncellenecekSubeAdiGrubu )" +
                                               "Values(@par1, @par2, @par3, @par4, @par5, @par6, @par7, @par8, @par9, @par10, @par11, @par12)",
                                new object[]
                                {
                                   mf.RTI(item, "TemplateId"),
                                   sablonName,
                                   mf.RTI(item, "ProductId"),
                                   mf.RTI(item, "Choice1Id"),
                                   mf.RTI(item, "Choice2Id"),
                                   mf.RTI(item, "OptionsId"),
                                   mf.RTD(item, "Price"),
                                   modelSube.ID,
                                   modelSube.SubeName,
                                   mf.RTI(item, "Id"),
                                   isSelectedSubeId,
                                   isSelectedSubeAdi
                                });
                        }

                        foreach (DataRow item in dataSubeUrunlist.AsEnumerable().Distinct().ToList())
                        {
                            var pPrice = Convert.ToDecimal(mf.RTD(item, "Price"));
                            //Product
                            sqlData.ExecuteSql(" Insert Into  SablonProduct( ProductName, ProductGroup, Price, SubeId, SubeName, ProductPkId,TemplateName, GuncellenecekSubeIdGrubu,GuncellenecekSubeAdiGrubu  ) Values( @par1, @par2, @par3, @par4, @par5, @par6, @par7, @par8, @par9 )",
                                new object[]
                                {
                                mf.RTS(item, "ProductName"),
                                mf.RTS(item, "ProductGroup"),
                                pPrice,
                                modelSube.ID,
                                modelSube.SubeName,
                                mf.RTI(item, "ProductPkId"),
                                sablonName,
                                isSelectedSubeId,
                                isSelectedSubeAdi
                                });

                            //Choice1
                            var dataSubeChoiceUrunlist = mf.GetSubeDataWithQuery((mf.NewConnectionString(modelSube.SubeIP, modelSube.DBName, modelSube.SqlName, modelSube.SqlPassword)), SqlData.getChoice1SqlQuery(mf.RTI(item, "ProductPkId")));
                            if (dataSubeChoiceUrunlist.Rows.Count > 0)
                            {
                                foreach (DataRow dataChoice1 in dataSubeChoiceUrunlist.Rows)
                                {
                                    var ch1Price = Convert.ToDecimal(mf.RTD(dataChoice1, "Choice1_Price"));
                                    sqlData.ExecuteSql(" Insert Into  SablonChoice1( ProductId, Name, Price, SubeId, SubeName, Choice1PkId,TemplateName, GuncellenecekSubeIdGrubu,GuncellenecekSubeAdiGrubu  ) Values( @par1, @par2, @par3, @par4, @par5, @par6, @par7, @par8, @par9 )",
                                    new object[]
                                    {
                                    mf.RTI(dataChoice1, "ProductId"),
                                    mf.RTS(dataChoice1, "ChoiceProductName"),
                                    ch1Price,
                                    modelSube.ID,
                                    modelSube.SubeName,
                                    mf.RTI(dataChoice1, "Id"),
                                     sablonName,
                                    isSelectedSubeId,
                                    isSelectedSubeAdi
                                    });

                                    //Choice2
                                    var dataSubeChoice2Urunlist = mf.GetSubeDataWithQuery(mf.NewConnectionString(modelSube.SubeIP, modelSube.DBName, modelSube.SqlName, modelSube.SqlPassword),
                                                                                                                                                SqlData.getChoice2SqlQuery(mf.RTI(dataChoice1, "Id"), mf.RTI(item, "ProductPkId")));
                                    if (dataSubeChoice2Urunlist.Rows.Count > 0)
                                    {
                                        foreach (DataRow dataChoice2 in dataSubeChoice2Urunlist.Rows)
                                        {
                                            var ch2Price = Convert.ToDecimal(mf.RTD(dataChoice2, "Choice2_Price"));
                                            var choice1 = mf.RTI(dataChoice1, "Id");
                                            var Choic2eName = mf.RTS(dataChoice2, "ChoiceProductName");
                                            sqlData.ExecuteSql("Insert Into  SablonChoice2( ProductId, Choice1Id, Name, Price, SubeId, SubeName, Choice2PkId,TemplateName,  GuncellenecekSubeIdGrubu,GuncellenecekSubeAdiGrubu ) Values( @par1, @par2, @par3, @par4, @par5, @par6,@Par7, @par8, @par9,@par10 )",
                                            new object[]
                                            {
                                            mf.RTI(dataChoice2, "ProductId"),
                                            mf.RTI(dataChoice1, "Id"),
                                            mf.RTS(dataChoice2, "ChoiceProductName"),
                                            ch2Price,
                                            modelSube.ID,
                                            modelSube.SubeName,
                                            mf.RTI(dataChoice2, "Id"),
                                             sablonName,
                                            isSelectedSubeId,
                                            isSelectedSubeAdi
                                            });
                                        }
                                    }
                                    //Choice2
                                }
                            }

                            //Options
                            var dataSubeUrunOptionslist = mf.GetSubeDataWithQuery((mf.NewConnectionString(modelSube.SubeIP, modelSube.DBName, modelSube.SqlName, modelSube.SqlPassword)), SqlData.getOptionsSqlQuery(mf.RTI(item, "ProductPkId")));
                            if (dataSubeUrunOptionslist.Rows.Count > 0)
                            {
                                foreach (DataRow dataOptions in dataSubeUrunOptionslist.Rows)
                                {
                                    var opPrice = Convert.ToDecimal(mf.RTD(dataOptions, "Option_Price"));
                                    sqlData.ExecuteSql(" Insert Into  SablonOptions(ProductId, Name, Price, Category, SubeId, SubeName, OptionsPkId,TemplateName,  GuncellenecekSubeIdGrubu,GuncellenecekSubeAdiGrubu) Values( @par1, @par2, @par3, @par4, @par5, @par6, @par7, @par8, @par9, @par10)",
                                    new object[]
                                    {
                                    mf.RTI(dataOptions, "ProductId"),
                                    mf.RTS(dataOptions, "OptionsName"),
                                    opPrice,
                                    mf.RTD(dataOptions, "OptionsCategory"),
                                    modelSube.ID,
                                    modelSube.SubeName,
                                    mf.RTI(dataOptions, "Id"),
                                     sablonName,
                                    isSelectedSubeId,
                                    isSelectedSubeAdi
                                    });
                                }
                            }
                        }
                    }
                    else //Şablon yoksa
                    {
                        //var dataForSablonSubeUrunlist = mf.GetSubeDataWithQuery(mf.NewConnectionString(modelSube.SubeIP, modelSube.DBName, modelSube.SqlName, modelSube.SqlPassword), SqlData.getForSablonProductChoiceChoice2OptionsSqlQuery());
                        //foreach (DataRow item in dataForSablonSubeUrunlist.Rows)
                        //{
                        //    //ProductTemplatePrice insert
                        //    sqlData.ExecuteSql("Insert Into  ProductTemplatePrice( TemplateId,TemplateName,ProductId,Choice1Id,Choice2Id,OptionsId,Price,SubeId,SubeName,ProductTemplatePricePkId,GuncellenecekSubeIdGrubu,GuncellenecekSubeAdiGrubu,IsManuelInsert )" +
                        //                       "Values(@par1, @par2, @par3, @par4, @par5, @par6, @par7, @par8, @par9, @par10, @par11, @par12,@par13)",
                        //        new object[]
                        //        {
                        //           sablonProductPkId, //mf.RTI(item, "TemplateId"),
                        //           sablonName,
                        //           mf.RTI(item, "ProductId"),
                        //           mf.RTI(item, "Choice1Id"),
                        //           mf.RTI(item, "Choice2Id"),
                        //           mf.RTI(item, "OptionsId"),
                        //           0,//mf.RTD(item, "Price"),
                        //           modelSube.ID,
                        //           modelSube.SubeName,
                        //           mf.RTI(item, "Id"),
                        //           isSelectedSubeId,
                        //           isSelectedSubeAdi,
                        //           true
                        //        });
                        //}

                        //foreach (DataRow item in dataSubeUrunlist.AsEnumerable().Distinct().ToList())
                        //{
                        //    var pPrice = Convert.ToDecimal(mf.RTD(item, "Price"));
                        //    //Product
                        //    sqlData.ExecuteSql(" Insert Into  SablonProduct( ProductName, ProductGroup, Price, SubeId, SubeName, ProductPkId,TemplateName, GuncellenecekSubeIdGrubu,GuncellenecekSubeAdiGrubu  ) Values( @par1, @par2, @par3, @par4, @par5, @par6, @par7, @par8, @par9 )",
                        //        new object[]
                        //        {
                        //            mf.RTS(item, "ProductName"),
                        //            mf.RTS(item, "ProductGroup"),
                        //            0, //pPrice,
                        //            modelSube.ID,
                        //            modelSube.SubeName,
                        //            mf.RTI(item, "Id"),
                        //            sablonName,
                        //            isSelectedSubeId,
                        //            isSelectedSubeAdi
                        //        });

                        //    //Choice1
                        //    var dataSubeChoiceUrunlist = mf.GetSubeDataWithQuery((mf.NewConnectionString(modelSube.SubeIP, modelSube.DBName, modelSube.SqlName, modelSube.SqlPassword)), SqlData.getChoice1SqlQuery(mf.RTI(item, "Id")));
                        //    if (dataSubeChoiceUrunlist.Rows.Count > 0)
                        //    {
                        //        foreach (DataRow dataChoice1 in dataSubeChoiceUrunlist.Rows)
                        //        {
                        //            var ch1Price = Convert.ToDecimal(mf.RTD(dataChoice1, "Choice1_Price"));
                        //            sqlData.ExecuteSql(" Insert Into  SablonChoice1( ProductId, Name, Price, SubeId, SubeName, Choice1PkId,TemplateName, GuncellenecekSubeIdGrubu,GuncellenecekSubeAdiGrubu  ) Values( @par1, @par2, @par3, @par4, @par5, @par6, @par7, @par8, @par9 )",
                        //            new object[]
                        //            {
                        //            mf.RTI(dataChoice1, "ProductId"),
                        //            mf.RTS(dataChoice1, "ChoiceProductName"),
                        //            0, //ch1Price,
                        //            modelSube.ID,
                        //            modelSube.SubeName,
                        //            mf.RTI(dataChoice1, "Id"),
                        //                sablonName,
                        //            isSelectedSubeId,
                        //            isSelectedSubeAdi
                        //            });

                        //            //Choice2
                        //            var dataSubeChoice2Urunlist = mf.GetSubeDataWithQuery(mf.NewConnectionString(modelSube.SubeIP, modelSube.DBName, modelSube.SqlName, modelSube.SqlPassword),
                        //                                                                                                                        SqlData.getChoice2SqlQuery(mf.RTI(dataChoice1, "Id"), mf.RTI(item, "Id")));
                        //            if (dataSubeChoice2Urunlist.Rows.Count > 0)
                        //            {
                        //                foreach (DataRow dataChoice2 in dataSubeChoice2Urunlist.Rows)
                        //                {
                        //                    var ch2Price = Convert.ToDecimal(mf.RTD(dataChoice2, "Choice2_Price"));
                        //                    var choice1 = mf.RTI(dataChoice1, "Id");
                        //                    var Choic2eName = mf.RTS(dataChoice2, "ChoiceProductName");

                        //                    sqlData.ExecuteSql("Insert Into  SablonChoice2( ProductId, Choice1Id, Name, Price, SubeId, SubeName, Choice2PkId,TemplateName,  GuncellenecekSubeIdGrubu,GuncellenecekSubeAdiGrubu ) Values( @par1, @par2, @par3, @par4, @par5, @par6,@Par7, @par8, @par9,@par10 )",
                        //                    new object[]
                        //                    {
                        //                    mf.RTI(dataChoice2, "ProductId"),
                        //                    mf.RTI(dataChoice1, "Id"),
                        //                    mf.RTS(dataChoice2, "ChoiceProductName"),
                        //                    0,//ch2Price,
                        //                    modelSube.ID,
                        //                    modelSube.SubeName,
                        //                    mf.RTI(dataChoice2, "Id"),
                        //                     sablonName,
                        //                    isSelectedSubeId,
                        //                    isSelectedSubeAdi
                        //                    });
                        //                }
                        //            }
                        //            //Choice2
                        //        }
                        //    }

                        //    //Options
                        //    var dataSubeUrunOptionslist = mf.GetSubeDataWithQuery((mf.NewConnectionString(modelSube.SubeIP, modelSube.DBName, modelSube.SqlName, modelSube.SqlPassword)), SqlData.getOptionsSqlQuery(mf.RTI(item, "Id")));
                        //    if (dataSubeUrunOptionslist.Rows.Count > 0)
                        //    {
                        //        foreach (DataRow dataOptions in dataSubeUrunOptionslist.Rows)
                        //        {
                        //            var opPrice = Convert.ToDecimal(mf.RTD(dataOptions, "Option_Price"));
                        //            sqlData.ExecuteSql(" Insert Into  SablonOptions(ProductId, Name, Price, Category, SubeId, SubeName, OptionsPkId,TemplateName,  GuncellenecekSubeIdGrubu,GuncellenecekSubeAdiGrubu) Values( @par1, @par2, @par3, @par4, @par5, @par6, @par7, @par8, @par9, @par10)",
                        //            new object[]
                        //            {
                        //            mf.RTI(dataOptions, "ProductId"),
                        //            mf.RTS(dataOptions, "OptionsName"),
                        //            0,//opPrice,
                        //            mf.RTD(dataOptions, "OptionsCategory"),
                        //            modelSube.ID,
                        //            modelSube.SubeName,
                        //            mf.RTI(dataOptions, "Id"),
                        //             sablonName,
                        //            isSelectedSubeId,
                        //            isSelectedSubeAdi
                        //            });
                        //        }
                        //    }
                        //}
                    }
                }
            }
            return result;
        }
        public ActionResultMessages SubeSablonInsertLocalTable(SablonSubeViewModel model, string kullaniciId)
        {
            string isSelectedSubeId = string.Empty;
            string isSelectedSubeAdi = string.Empty;
            foreach (var item in model.IsSelectedSubeList.Where(x => x.IsSelectedHedefSube))
            {
                isSelectedSubeId += item.ID + ",";
                isSelectedSubeAdi += item.SubeName + ",";
            }
            var result = new ActionResultMessages { IsSuccess = true, UserMessage = "İşlem başarılı", };
            var urunlerInsertresult = SablonProductChoice1Choice2OptionsInsertLocalDb(model, isSelectedSubeId, isSelectedSubeAdi, kullaniciId);
            //var resultSubeList = new SPosSubeSablonFiyatGuncellemeCRUD().GetSubeList();
            var sablonProductPkId = model.IsSelectedSubeList.Where(y => y.IsSelectedHedefSube).Select(x => x.ProductTemplatePkId).FirstOrDefault();
            var sablonName = model.IsSelectedSubeList.Where(y => y.IsSelectedHedefSube).Select(x => x.SablonName).FirstOrDefault();

            //Kullanici yetkili şubeleri.
            var kullaniciSubeYetkisi = UsersListCRUD.KullaniciSubeYetkiListesi(kullaniciId);

            foreach (var isSelectedSubeItem in model.IsSelectedSubeList.Where(x => x.IsSelectedHedefSube))
            {
                var sube = SqlData.GetSube(isSelectedSubeItem.ID);

                if (kullaniciSubeYetkisi != null && kullaniciSubeYetkisi.FR_SubeListesi.Where(x => x.SubeID == Convert.ToInt32(sube.ID)).Select(x => x.SubeID).Any())
                {
                    var sqlData = new SqlData(new SqlConnection(mf.NewConnectionString(subeIp, dbName, sqlKullaniciName, sqlKullaniciPassword)));
                    //Code review yapılabilir.Yukarıdaki foreach ile birleştirilebilir.
                    #region ProductTemplatePrice Tablosunda yoksa kaydediliyor.

                    var dataForSablonSubeUrunlist = mf.GetSubeDataWithQuery(mf.NewConnectionString(sube.SubeIP, sube.DBName, sube.SqlName, sube.SqlPassword),
                                                        SqlData.getForSablonProductChoiceChoice2OptionsSqlQuery2(dbName, sablonName, sube.ID.ToString(), isSelectedSubeId));

                    //using (SqlBulkCopy bulkCopy = new SqlBulkCopy(mf.NewConnectionString(sube.SubeIP, sube.DBName, sube.SqlName, sube.SqlPassword)))
                    //{
                    //    bulkCopy.DestinationTableName = "ProductTemplatePrice";
                    //    try
                    //    {
                    //        bulkCopy.WriteToServer(dataForSablonSubeUrunlist);
                    //        //return ("Aktarım Tamamlandı");
                    //    }
                    //    catch (Exception ex)
                    //    {
                    //        //return (ex.Message);
                    //    }
                    //}

                    foreach (DataRow yeniSablonPriceItem in dataForSablonSubeUrunlist.Rows)
                    {
                        var Price = mf.RTI(yeniSablonPriceItem, "Price");
                        var SubeId = mf.RTI(yeniSablonPriceItem, "SubeId");
                        var SubeName = mf.RTI(yeniSablonPriceItem, "SubeName");
                        var productId = mf.RTI(yeniSablonPriceItem, "ProductId");
                        var choice1Id = mf.RTI(yeniSablonPriceItem, "Choice1Id");
                        var choice2Id = mf.RTI(yeniSablonPriceItem, "Choice2Id");
                        var optionsId = mf.RTI(yeniSablonPriceItem, "OptionsId");
                        var TemplateId = mf.RTI(yeniSablonPriceItem, "TemplateId");
                        var TemplateName = mf.RTI(yeniSablonPriceItem, "TemplateName");
                        var ProductTemplatePricePkId = mf.RTI(yeniSablonPriceItem, "ProductTemplatePricePkId");

                        //ProductTemplatePrice insert
                        sqlData.ExecuteSql("Insert Into  ProductTemplatePrice( TemplateId,TemplateName,ProductId,Choice1Id,Choice2Id,OptionsId,Price,SubeId,SubeName,ProductTemplatePricePkId,GuncellenecekSubeIdGrubu,GuncellenecekSubeAdiGrubu,IsManuelInsert )" +
                                           "Values(@par1, @par2, @par3, @par4, @par5, @par6, @par7, @par8, @par9, @par10, @par11, @par12,@par13)",
                        new object[]
                        {
                                   sablonProductPkId, //mf.RTI(item, "TemplateId"),
                                   sablonName,
                                   mf.RTI(yeniSablonPriceItem, "ProductId"),
                                   mf.RTI(yeniSablonPriceItem, "Choice1Id"),
                                   mf.RTI(yeniSablonPriceItem, "Choice2Id"),
                                   mf.RTI(yeniSablonPriceItem, "OptionsId"),
                                   0, //mf.RTD(item, "Price"),
                                   sube.ID, //modelSube.ID,
                                   sube.SubeName, //modelSube.SubeName,
                                   mf.RTI(yeniSablonPriceItem, "Id"),
                                   isSelectedSubeId,
                                   isSelectedSubeAdi,
                                   true
                        });
                    }

                    #endregion ProductTemplatePrice Tablosunda yoksa kaydediliyor.
                }
            }

            return result;
        }

        //public string BulkInsert(DataTable dt, string KaydedilecekTAbloAdı)
        //{
        //    using (SqlBulkCopy bulkCopy = new SqlBulkCopy(Connection))
        //    {
        //        bulkCopy.DestinationTableName = KaydedilecekTAbloAdı;
        //        try
        //        {
        //            bulkCopy.WriteToServer(dt);
        //            return ("Aktarım Tamamlandı");
        //        }
        //        catch (Exception ex)
        //        {
        //            return (ex.Message);
        //        }
        //    }
        //}
        #endregion Sablon Create - tüm şubelerdeki sablon tablosunu local db'ye çeker

        #region (05.02.2022 toplantı sonrası) Seçili şubelerdeki pruduct, choice1,choice2,options tablolarınını merkez (local temp db'ye aktarma) 

        #region Product update
        public UrunEditViewModel GetByProductForSubeListEdit(string productGroup_, string SubeIdGrupList, string sablonName)
        {
            var result = new ActionResultMessages { IsSuccess = true, UserMessage = "İşlem başarılı", };
            var returnViewModel = new UrunEditViewModel() { };
            returnViewModel.SubeList = new List<ProductCompairSube>();
            returnViewModel.productCompairsList = new List<ProductCompair>();
            returnViewModel.IsSuccess = true;

            try
            {
                DataTable dataSubeUrunlist = mf.GetSubeDataWithQuery(mf.NewConnectionString(subeIp, dbName, sqlKullaniciName, sqlKullaniciPassword),
                                                                     productGroup_ == null || productGroup_ == "null"
                                                                      ? SqlData.getLocalSablonProductListSqlQuery(SubeIdGrupList, sablonName)
                                                                      : SqlData.getProductForProductGroupSqlQuery2(productGroup_, SubeIdGrupList, sablonName));

                var subeIdList = new List<long>();
                foreach (DataRow item in dataSubeUrunlist.Rows)
                {
                    subeIdList.Add(mf.RTI(item, "SubeId"));
                }

                subeIdList = subeIdList.Distinct().ToList();
                foreach (var sube in subeIdList)
                {
                    var drSube = dataSubeUrunlist.Select("SubeId=" + sube)[0];
                    var subeName = mf.RTS(drSube, "SubeName");
                    returnViewModel.SubeList.Add(new ProductCompairSube
                    {
                        SubeId = sube,
                        SubeAdi = subeName,
                    });
                }

                foreach (DataRow item in dataSubeUrunlist.Rows)
                {
                    var productGroup = mf.RTS(item, "ProductGroup");
                    var productName = mf.RTS(item, "ProductName");
                    var subeName = mf.RTS(item, "SubeName");
                    var subePrice = mf.RTD(item, "productprice");
                    var subeId = mf.RTI(item, "SubeId");
                    var productId = mf.RTI(item, "ProductId");
                    var optionsVarMi = mf.RTI(item, "OptionsVarMi");
                    var choice1VarMi = mf.RTI(item, "Choice1VarMi");
                    var choiceProductName = mf.RTS(item, "ChoiceProductName");
                    var id = mf.RTI(item, "ProductTemplatePricePkId");
                    var pkId = mf.RTI(item, "Id");
                    var isManuelInsert = mf.RTS(item, "IsManuelInsert");
                    var pc = returnViewModel.productCompairsList.Where(x => x.ProductName == productName && x.ProductGroup == productGroup).FirstOrDefault();

                    if (pc == null && !string.IsNullOrEmpty(productName))
                    {
                        pc = new ProductCompair
                        {
                            Id = id,
                            PkId = pkId,
                            ProductId = productId,
                            ProductName = productName,
                            ProductGroup = productGroup,
                            OptionsVarMi = optionsVarMi,
                            Choice1VarMi = choice1VarMi,
                            ChoiceProductName = choiceProductName,
                            IsManuelInsert = isManuelInsert
                        };
                        returnViewModel.productCompairsList.Add(pc);
                        pc.SubeList = new List<UrunEdit2>();
                    }

                    var urunEdit2 = new UrunEdit2()
                    {
                        SubeId = subeId,
                        SubePriceValue = subePrice,
                        PkId = pkId,

                    };

                    //var price = result.productCompairsList[i].SubeList.Distinct().Where(y => y.SubeId == result.SubeList[a].SubeId).FirstOrDefault();
                    //var priceIndex = result.productCompairsList[i].SubeList.IndexOf(price);

                    if (!string.IsNullOrEmpty(productName))
                    {
                        pc.SubeList.Add(urunEdit2);
                    }
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
                Singleton.WritingLogFile2("SPosSubeSablonFiyatGuncellemeCRUD_GetByProductForSubeListEdit", ex.ToString(), null, ex.StackTrace);
                returnViewModel.IsSuccess = false;
                returnViewModel.ErrorList.Add("Bir hata oluştu.");
                return returnViewModel;
            }

            returnViewModel.IsSuccess = true;

            return returnViewModel;
        }

        public UrunEditViewModel2 GetByProductForSubeListEdit2(string productGroup_, string SubeIdGrupList, string sablonName)
        {
            var result = new ActionResultMessages { IsSuccess = true, UserMessage = "İşlem başarılı", };
            var returnViewModel = new UrunEditViewModel2() { };
            returnViewModel.SubeList = new List<ProductCompairSube>();
            returnViewModel.IsSuccess = true;
            try
            {
                DataTable dataSubeUrunlist = mf.GetSubeDataWithQuery(mf.NewConnectionString(subeIp, dbName, sqlKullaniciName, sqlKullaniciPassword),
                                                                     productGroup_ == null || productGroup_ == "null"
                                                                      ? SqlData.getLocalSablonProductListSqlQuery(SubeIdGrupList, sablonName)
                                                                      : SqlData.getProductForProductGroupSqlQuery2(productGroup_, SubeIdGrupList, sablonName));

                var subeIdList = new List<long>();
                foreach (DataRow item in dataSubeUrunlist.Rows)
                {
                    subeIdList.Add(mf.RTI(item, "SubeId"));
                }

                subeIdList = subeIdList.Distinct().ToList();
                foreach (var sube in subeIdList)
                {
                    var drSube = dataSubeUrunlist.Select("SubeId=" + sube)[0];
                    var subeName = mf.RTS(drSube, "SubeName");
                    returnViewModel.SubeList.Add(new ProductCompairSube
                    {
                        SubeId = sube,
                        SubeAdi = subeName,
                    });
                }

                returnViewModel.ProductList = dataSubeUrunlist.AsEnumerable().ToList()
                    .Select(item => new
                    {
                        ProductGroup = mf.RTS(item, "ProductGroup"),
                        ProductName = mf.RTS(item, "ProductName"),
                        GuncellenecekSubeIdGrubu = mf.RTS(item, "GuncellenecekSubeIdGrubu"),
                        //IsManuelInsert = mf.RTS(item, "IsManuelInsert"),
                    })
                    .Distinct()
                    .Select(item => new UrunEditItemViewModel
                    {
                        //Id= mf.RTI(item, "Id"),
                        ProductGroup = item.ProductGroup,
                        ProductName = item.ProductName,
                        //IsManuelInsert = item.IsManuelInsert == string.Empty ? false : Convert.ToBoolean(item.IsManuelInsert),
                        UrunEditPriceList = new List<UrunEditPrice>(),
                    })
                    .OrderBy(x => x.ProductGroup)
                    .ToList();

                var dbProDuctItemList = dataSubeUrunlist.AsEnumerable().ToList()
                      .Select(item => new
                      {
                          Id = mf.RTI(item, "Id"),
                          SubeId = mf.RTI(item, "SubeId"),
                          ProductId = mf.RTI(item, "ProductId"),
                          subePrice = mf.RTD(item, "productprice"),
                          ProductName = mf.RTS(item, "ProductName"),
                          ProductGroup = mf.RTS(item, "ProductGroup"),
                          OptionsVarMi = mf.RTI(item, "OptionsVarMi"),
                          Choice1VarMi = mf.RTI(item, "Choice1VarMi"),
                          IsManuelInsert = mf.RTS(item, "IsManuelInsert"),
                      }).ToList()
                      .OrderBy(x => x.ProductGroup)
                      .ToList();

                var ProductList = returnViewModel.ProductList.OrderBy(x => x.ProductGroup).ToList();
                foreach (var productItem in ProductList)
                {
                    foreach (var sube in returnViewModel.SubeList)
                    {
                        var priceItem = dbProDuctItemList
                            .Where(x => x.SubeId == sube.SubeId)
                            .Where(x => x.ProductGroup == productItem.ProductGroup)
                            .Where(x => x.ProductName == productItem.ProductName)
                            .FirstOrDefault();

                        if (priceItem == null)
                        {
                            productItem.UrunEditPriceList.Add(new UrunEditPrice { SubeId = sube.SubeId, Price = null });
                        }
                        else
                            productItem.UrunEditPriceList.Add(new UrunEditPrice
                            {
                                Id = priceItem.Id,
                                SubeId = sube.SubeId,
                                Price = priceItem.subePrice,
                                ProductId = priceItem.ProductId,
                                Choice1VarMi = priceItem.Choice1VarMi,
                                OptionsVarMi = priceItem.OptionsVarMi,
                                IsManuelInsert = priceItem.IsManuelInsert == string.Empty ? false : Convert.ToBoolean(priceItem.IsManuelInsert),
                            });
                    }
                }
            }
            catch (Exception ex)
            {
                Singleton.WritingLogFile2("SPosSubeSablonFiyatGuncellemeCRUD_GetByProductForSubeListEdit", ex.ToString(), null, ex.StackTrace);
                returnViewModel.IsSuccess = false;
                returnViewModel.ErrorList.Add("Bir hata oluştu.");
                return returnViewModel;
            }
            returnViewModel.IsSuccess = true;

            return returnViewModel;
        }

        public ActionResultMessages UpdateByProductForSubeList(UrunEditViewModel model)
        {
            var result = new ActionResultMessages() { IsSuccess = true, UserMessage = "İşlem Başarılı", };

            try
            {
                var kullaniciData = BussinessHelper.GetByUserInfoForId("", "", model.KullaniciId);
                //var modelSube = SqlData.GetSube(model.SubeId);

                #region Local database'e veriler kaydetmek için kullanıldı.
                var sqlData = new SqlData(new SqlConnection(mf.NewConnectionString(subeIp, dbName, sqlKullaniciName, sqlKullaniciPassword)));
                var dataSubeUrunlist = mf.GetSubeDataWithQuery((mf.NewConnectionString(subeIp, dbName, sqlKullaniciName, sqlKullaniciPassword)), SqlData.getLocalProductTemplatePriceSqlQuery(model.SablonName));
                #endregion Local database'e veriler kaydetmek için kullanıldı.

                foreach (var modelProduct in model.productCompairsList)
                {
                    foreach (DataRow dataUrunSube in dataSubeUrunlist.Rows)
                    {
                        foreach (var price in modelProduct.SubeList)
                        {
                            var id = mf.RTD(dataUrunSube, "Id");
                            var subeId = mf.RTD(dataUrunSube, "SubeId");
                            var productId = mf.RTI(dataUrunSube, "ProductId");
                            var productprice = mf.RTD(dataUrunSube, "productprice");
                            var productTemplatePricePkId = mf.RTD(dataUrunSube, "ProductTemplatePricePkId");
                            var pkId = price.PkId;

                            var pr = mf.RTD(dataUrunSube, "productprice");
                            var prId = mf.RTD(dataUrunSube, "Id");

                            if (price.SubeId == mf.RTD(dataUrunSube, "SubeId") &&
                                price.SubePriceValue != null &&
                                price.SubePriceValue != mf.RTD(dataUrunSube, "productprice") &&
                                price.PkId == mf.RTD(dataUrunSube, "Id")
                                )
                            {
                                sqlData.ExecuteSql("update ProductTemplatePrice set Price = @par1 Where SubeId = @par2 and  Id=@par3 ",
                                 new object[]
                                      {
                                        price.SubePriceValue,
                                        price.SubeId,
                                        price.PkId,
                                          //modelProduct.ProductId,
                                          //modelProduct.ProductGroup,
                                          //modelProduct.ProductName
                                      });

                                //  sqlData.executeSql(" Insert Into  ProductTarihce( ProductName, ProductGroup, Price, SubeId,SubeName, ProductPkId, IsUpdateDate,IsUpdateKullanici ) Values( @par1, @par2, @par3, @par4, @par5, @par6, @par7, @par8 )",
                                //new object[] {
                                //    mf.RTS(dataUrunSube, "ProductName"),
                                //    mf.RTS(dataUrunSube, "ProductGroup"),
                                //    mf.RTS(dataUrunSube, "Price"),
                                //    price.SubeId,
                                //    model.SubeList.Where(x=>x.SubeId==price.SubeId).FirstOrDefault().SubeAdi,
                                //    mf.RTI(dataUrunSube, "Id"),
                                //    DateTime.Now.Date.ToString("dd'/'MM'/'yyyy HH:mm"),
                                //    kullaniciData.UserName
                                //});
                                break;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Singleton.WritingLogFile2("SPosSubeSablonFiyatGuncellemeCRUD_UpdateByProductForSubeList", ex.ToString(), null, ex.StackTrace);
                result.IsSuccess = false;
                result.UserMessage = "Bir hata oluştu.";
                return result;
            }

            return result;
        }

        public ActionResultMessages UpdateByProductForSubeList2(UrunEditViewModel2 model)
        {
            var result = new ActionResultMessages() { IsSuccess = true, UserMessage = "İşlem Başarılı", };

            try
            {
                var kullaniciData = BussinessHelper.GetByUserInfoForId("", "", model.KullaniciId);
                //var modelSube = SqlData.GetSube(model.SubeId);

                #region Local database'e veriler kaydetmek için kullanıldı.
                var sqlData = new SqlData(new SqlConnection(mf.NewConnectionString(subeIp, dbName, sqlKullaniciName, sqlKullaniciPassword)));
                var dataSubeUrunlist = mf.GetSubeDataWithQuery((mf.NewConnectionString(subeIp, dbName, sqlKullaniciName, sqlKullaniciPassword)), SqlData.getLocalProductTemplatePriceSqlQuery(model.SablonName));
                #endregion Local database'e veriler kaydetmek için kullanıldı.

                foreach (var modelProduct in model.ProductList)
                {
                    foreach (DataRow dataUrunSube in dataSubeUrunlist.Rows)
                    {
                        foreach (var price in modelProduct.UrunEditPriceList)
                        {
                            var id = mf.RTD(dataUrunSube, "Id");
                            var subeId = mf.RTD(dataUrunSube, "SubeId");
                            var productId = mf.RTI(dataUrunSube, "ProductId");
                            var productprice = mf.RTD(dataUrunSube, "productprice");
                            var productTemplatePricePkId = mf.RTD(dataUrunSube, "ProductTemplatePricePkId");
                            var pkId = price.Id;

                            var pr = mf.RTD(dataUrunSube, "productprice");
                            var prId = mf.RTD(dataUrunSube, "Id");

                            if (price.SubeId == mf.RTD(dataUrunSube, "SubeId") &&
                                price.Price != null &&
                                price.Price != mf.RTD(dataUrunSube, "productprice") &&
                                price.Id == mf.RTD(dataUrunSube, "Id")
                                )
                            {
                                sqlData.ExecuteSql("update ProductTemplatePrice set Price = @par1 Where SubeId = @par2 and  Id=@par3 ",
                                 new object[]
                                      {
                                        price.Price,
                                        price.SubeId,
                                        price.Id,
                                          //modelProduct.ProductId,
                                          //modelProduct.ProductGroup,
                                          //modelProduct.ProductName
                                      });
                                break;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Singleton.WritingLogFile2("SPosSubeSablonFiyatGuncellemeCRUD_UpdateByProductForSubeList", ex.ToString(), null, ex.StackTrace);
                result.IsSuccess = false;
                result.UserMessage = "Bir hata oluştu.";
                return result;
            }

            return result;
        }

        #endregion Product update


        public UrunEditViewModel2 GetByChoice1ForSubeListEdit(string subeId_, string productId_, string subeIdGrupList)
        {
            var result = new ActionResultMessages { IsSuccess = true, UserMessage = "İşlem başarılı", };
            var returnViewModel = new UrunEditViewModel2() { };

            DataTable dataSubeChoice1Urunlist = mf.GetSubeDataWithQuery(mf.NewConnectionString(subeIp, dbName, sqlKullaniciName, sqlKullaniciPassword),
                                                                        SqlData.getLocalChoice1tListSqlQuery(subeId_, productId_, subeIdGrupList));

            #region Choice1 listesi
            returnViewModel.SubeList = new List<ProductCompairSube>();
            returnViewModel.IsSuccess = true;

            var subeIdList = new List<long>();
            foreach (DataRow item in dataSubeChoice1Urunlist.Rows)
            {
                subeIdList.Add(mf.RTI(item, "SubeId"));
            }
            subeIdList = subeIdList.Distinct().ToList();

            foreach (var sube in subeIdList)
            {
                var drSube = dataSubeChoice1Urunlist.Select("SubeId=" + sube)[0];
                var subeName = mf.RTS(drSube, "SubeName");
                returnViewModel.SubeList.Add(new ProductCompairSube
                {
                    SubeId = sube,
                    SubeAdi = subeName,
                });
            }


            returnViewModel.ProductList = dataSubeChoice1Urunlist.AsEnumerable().ToList()
                    .Select(item => new
                    {
                        ProductGroup = mf.RTS(item, "ProductGroup"),
                        ProductName = mf.RTS(item, "ProductName"),
                        ChoiceProductName = mf.RTS(item, "ChoiceProductName"),
                        GuncellenecekSubeIdGrubu = mf.RTS(item, "GuncellenecekSubeIdGrubu"),
                    })
                    .Distinct()
                    .Select(item => new UrunEditItemViewModel
                    {
                        //Id= mf.RTI(item, "Id"),
                        ProductGroup = item.ProductGroup,
                        ProductName = item.ProductName,
                        ChoiceProductName = item.ChoiceProductName,
                        UrunEditPriceList = new List<UrunEditPrice>(),
                    })
                    .OrderBy(x => x.ProductGroup)
                    .ToList();

            var dbProDuctItemList = dataSubeChoice1Urunlist.AsEnumerable().ToList()
                  .Select(item => new
                  {
                      SubeId = mf.RTI(item, "SubeId"),
                      subePrice = mf.RTD(item, "productprice"),
                      OptionsVarMi = mf.RTI(item, "OptionsVarMi"),
                      Choice1VarMi = mf.RTI(item, "Choice1VarMi"),
                      ProductGroup = mf.RTS(item, "ProductGroup"),
                      ProductName = mf.RTS(item, "ProductName"),
                      IsManuelInsert = mf.RTS(item, "IsManuelInsert"),
                      Id = mf.RTI(item, "Id"),
                      ProductId = mf.RTI(item, "ProductId"),
                      /////////
                      SubeName = mf.RTS(item, "SubeName"),
                      //SubeId = mf.RTI(item, "SubeId"),
                      Choice1Id = mf.RTI(item, "Choice1Id"),
                      Choice2Id = mf.RTI(item, "Choice2Id"),
                      //var productId = mf.RTI(item, "ProductId");
                      ChoicePrice = mf.RTD(item, "c1price"),
                      Choice2VarMi = mf.RTI(item, "Choice2VarMi"),
                      ChoiceProductName = mf.RTS(item, "ChoiceProductName"),

                  }).ToList()
                  .OrderBy(x => x.ProductGroup)
                  .ToList();

            foreach (var productItem in returnViewModel.ProductList.OrderBy(x => x.ProductGroup).ToList())
            {
                foreach (var sube in returnViewModel.SubeList)
                {
                    var priceItem = dbProDuctItemList
                        .Where(x => x.SubeId == sube.SubeId)
                        .Where(x => x.ProductGroup == productItem.ProductGroup)
                        .Where(x => x.ProductName == productItem.ProductName)
                        .FirstOrDefault();

                    if (priceItem == null)
                    {
                        productItem.UrunEditPriceList.Add(new UrunEditPrice { SubeId = sube.SubeId, Price = null });
                    }
                    else
                        productItem.UrunEditPriceList.Add(new UrunEditPrice
                        {
                            SubeId = sube.SubeId,
                            Price = priceItem.subePrice,
                            Choice1VarMi = priceItem.Choice1VarMi,
                            OptionsVarMi = priceItem.OptionsVarMi,
                            IsManuelInsert = priceItem.IsManuelInsert == string.Empty ? false : Convert.ToBoolean(priceItem.IsManuelInsert),
                            Id = priceItem.Id,
                            ProductId = priceItem.ProductId,
                            Choice1Id = priceItem.Choice1Id,
                            Choice2Id = priceItem.Choice2Id,
                            ChoicePrice = priceItem.ChoicePrice,
                            Choice2VarMi = priceItem.Choice2VarMi,
                        });
                }
            }
            #endregion Choice1 listesi

            #region Options listesi
            #region Local database'e veriler kaydetmek için kullanıldı.
            DataTable dataSubeUrunOptionslist = mf.GetSubeDataWithQuery((mf.NewConnectionString(subeIp, dbName, sqlKullaniciName, sqlKullaniciPassword)), SqlData.getLocalOptionstListSqlQuery(subeId_, productId_, subeIdGrupList));
            #endregion Local database'e veriler kaydetmek için kullanıldı.


            returnViewModel.ProductOptionsList = dataSubeUrunOptionslist.AsEnumerable().ToList()
                   .Select(item => new
                   {
                       ProductGroup = mf.RTS(item, "ProductGroup"),
                       ProductName = mf.RTS(item, "ProductName"),
                       OptionsProductName = mf.RTS(item, "OptionsProductName"),
                       GuncellenecekSubeIdGrubu = mf.RTS(item, "GuncellenecekSubeIdGrubu"),
                   })
                   .Distinct()
                   .Select(item => new UrunEditItemViewModel
                   {
                       //Id= mf.RTI(item, "Id"),
                       ProductGroup = item.ProductGroup,
                       ProductName = item.ProductName,
                       OptionsProductName = item.OptionsProductName,
                       UrunEditPriceList = new List<UrunEditPrice>(),
                   })
                   .OrderBy(x => x.ProductGroup)
                   .ToList();

            var dbProDuctOptionsItemList = dataSubeUrunOptionslist.AsEnumerable().ToList()
                  .Select(item => new
                  {
                      Id = mf.RTI(item, "Id"),
                      SubeId = mf.RTI(item, "SubeId"),
                      SubeName = mf.RTS(item, "SubeName"),
                      ProductGroup = mf.RTS(item, "ProductGroup"),
                      ProductName = mf.RTS(item, "ProductName"),
                      subePrice = mf.RTD(item, "productprice"),
                      ProductId = mf.RTI(item, "ProductId"),
                      OptionsId = mf.RTI(item, "OptionsId"),
                      optionprice = mf.RTD(item, "optionprice"),
                      OptionsVarMi = mf.RTI(item, "OptionsVarMi"),
                      Choice1VarMi = mf.RTI(item, "Choice1VarMi"),
                      OptionsProductName = mf.RTS(item, "OptionsProductName"),
                      IsManuelInsert = mf.RTS(item, "IsManuelInsert"),
                      ProductTemplatePricePkId = mf.RTI(item, "ProductTemplatePricePkId"),
                  }).ToList()
                  .OrderBy(x => x.ProductGroup)
                  .ToList();

            foreach (var productItem in returnViewModel.ProductList.OrderBy(x => x.ProductGroup).ToList())
            {
                foreach (var sube in returnViewModel.SubeList)
                {
                    var priceItem = dbProDuctOptionsItemList
                        .Where(x => x.SubeId == sube.SubeId)
                        .Where(x => x.ProductGroup == productItem.ProductGroup)
                        .Where(x => x.ProductName == productItem.ProductName)
                        .FirstOrDefault();

                    if (priceItem == null)
                    {
                        productItem.UrunEditPriceList.Add(new UrunEditPrice { SubeId = sube.SubeId, Price = null });
                    }
                    else
                        productItem.UrunEditPriceList.Add(new UrunEditPrice
                        {
                            SubeId = sube.SubeId,
                            Price = priceItem.subePrice,
                            Choice1VarMi = priceItem.Choice1VarMi,
                            OptionsVarMi = priceItem.OptionsVarMi,
                            IsManuelInsert = priceItem.IsManuelInsert == string.Empty ? false : Convert.ToBoolean(priceItem.IsManuelInsert),
                            Id = priceItem.Id,
                            ProductId = priceItem.ProductId,
                            OptionsPrice = priceItem.optionprice,
                            OptionsId = priceItem.OptionsId,
                        });
                }
            }

            #endregion Options listesi

            return returnViewModel;
        }



        #region Choice1 update
        public UrunEditViewModel GetByChoice1ForSubeListEdit(string productGroup_, string productName_, string subeIdGrupList, string sablonName)
        {
            var result = new ActionResultMessages { IsSuccess = true, UserMessage = "İşlem başarılı", };

            #region Choice1 listesi

            var dataSubeUrunlist = mf.GetSubeDataWithQuery(mf.NewConnectionString(subeIp, dbName, sqlKullaniciName, sqlKullaniciPassword), SqlData.getLocalSablonChoice1ListSqlQuery(productGroup_, productName_, subeIdGrupList, sablonName));
            var returnViewModel = new UrunEditViewModel() { };
            returnViewModel.SubeList = new List<ProductCompairSube>();
            returnViewModel.productCompairsList = new List<ProductCompair>();
            returnViewModel.IsSuccess = true;

            var subeIdList = new List<long>();
            foreach (DataRow item in dataSubeUrunlist.Rows)
            {
                subeIdList.Add(mf.RTI(item, "SubeId"));
            }
            subeIdList = subeIdList.Distinct().ToList();

            foreach (var sube in subeIdList)
            {
                var drSube = dataSubeUrunlist.Select("SubeId=" + sube)[0];
                var subeName = mf.RTS(drSube, "SubeName");
                returnViewModel.SubeList.Add(new ProductCompairSube
                {
                    SubeId = sube,
                    SubeAdi = subeName,
                });
            }

            foreach (DataRow item in dataSubeUrunlist.Rows)
            {
                var id = mf.RTI(item, "ProductTemplatePricePkId");
                var productGroup = mf.RTS(item, "ProductGroup");
                var productName = mf.RTS(item, "ProductName");
                var subeName = mf.RTS(item, "SubeName");
                var subeId = mf.RTI(item, "SubeId");
                var choice1Id = mf.RTI(item, "Choice1Id");
                var choice2Id = mf.RTI(item, "Choice2Id");
                var productId = mf.RTI(item, "ProductId");
                var choicePrice = mf.RTD(item, "c1price");
                var choice2VarMi = mf.RTI(item, "Choice2VarMi");
                var choiceProductName = mf.RTS(item, "ChoiceProductName");
                var pkId = mf.RTI(item, "Id");
                var isManuelInsert = mf.RTS(item, "IsManuelInsert");

                var pc = returnViewModel.productCompairsList.Where(x => x.ProductName == productName && x.ProductGroup == productGroup && x.ChoiceProductName == choiceProductName).FirstOrDefault();
                //Choice2 var Mı?
                if (choice2VarMi > 0 && pc.ChoiceProductName == choiceProductName)
                {
                    pc.Choice2VarMi = choice2VarMi;
                }

                if (pc == null && !string.IsNullOrEmpty(choiceProductName))
                {
                    pc = new ProductCompair();
                    pc.Id = id;
                    pc.PkId = pkId;
                    pc.ProductGroup = productGroup;
                    pc.ProductName = productName;
                    pc.ProductId = productId;
                    pc.Choice1Id = choice1Id;
                    pc.ChoicePrice = choicePrice;
                    pc.Choice2VarMi = choice2VarMi;
                    pc.ChoiceProductName = choiceProductName;
                    pc.IsManuelInsert = isManuelInsert;
                    returnViewModel.productCompairsList.Add(pc);
                    pc.SubeList = new List<UrunEdit2>();
                }

                var urunEdit2 = new UrunEdit2()
                {
                    PkId = pkId,
                    SubeId = subeId,
                    SubePriceValue = choicePrice
                };
                if (!string.IsNullOrEmpty(choiceProductName))
                {
                    pc.SubeList.Add(urunEdit2);
                }
            }

            foreach (var sube in returnViewModel.SubeList)
            {
                foreach (var product in returnViewModel.productCompairsList)
                {
                    if (!product.SubeList.Where(x => x.SubeId == sube.SubeId).Any())
                    {
                        product.SubeList.Add(new UrunEdit2
                        {
                            SubeId = sube.SubeId
                        });
                    }
                }
            }
            #endregion Choice1 listesi

            #region Options listesi
            #region Local database'e veriler kaydetmek için kullanıldı.
            DataTable dataSubeUrunOptionslist = mf.GetSubeDataWithQuery(mf.NewConnectionString(subeIp, dbName, sqlKullaniciName, sqlKullaniciPassword), SqlData.getLocalSablonChoice1ListSqlQuery(productGroup_, productName_, subeIdGrupList, sablonName));
            #endregion Local database'e veriler kaydetmek için kullanıldı.

            returnViewModel.productOptionsCompairsList = new List<ProductCompair>();
            foreach (DataRow item in dataSubeUrunOptionslist.Rows)
            {
                var id = mf.RTI(item, "ProductTemplatePricePkId");
                var productGroup = mf.RTS(item, "ProductGroup");
                var productName = mf.RTS(item, "ProductName");
                var subeName = mf.RTS(item, "SubeName");
                var subeId = mf.RTI(item, "SubeId");
                var optionsId = mf.RTI(item, "OptionsId");
                var productId = mf.RTI(item, "ProductId");
                var optionsPrice = mf.RTD(item, "optionprice");
                var optionsName = mf.RTS(item, "OptionsProductName");
                var pkId = mf.RTI(item, "Id");
                var isManuelInsert = mf.RTS(item, "IsManuelInsert");

                var pc = returnViewModel.productOptionsCompairsList.Where(x => x.ProductName == productName && x.ProductGroup == productGroup && x.OptionsProductName == optionsName).FirstOrDefault();
                if (pc == null && !string.IsNullOrEmpty(optionsName))
                {
                    pc = new ProductCompair();
                    pc.Id = id;
                    pc.PkId = pkId;
                    pc.ProductGroup = productGroup;
                    pc.ProductName = productName;
                    pc.ProductId = productId;
                    pc.OptionsId = optionsId;
                    pc.OptionsPrice = optionsPrice;
                    pc.OptionsProductName = optionsName;
                    pc.IsManuelInsert = isManuelInsert;
                    returnViewModel.productOptionsCompairsList.Add(pc);
                    pc.SubeList = new List<UrunEdit2>();
                }

                var urunEdit2 = new UrunEdit2()
                {
                    PkId = pkId,
                    SubeId = subeId,
                    SubePriceValue = optionsPrice
                };
                if (!string.IsNullOrEmpty(optionsName))
                {
                    pc.SubeList.Add(urunEdit2);
                }
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

            returnViewModel.IsSuccess = true;
            return returnViewModel;
        }

        public UrunEditViewModel2 GetByChoice1ForSubeListEdit2(string productGroup_, string productName_, string subeIdGrupList, string sablonName)
        {
            var result = new ActionResultMessages { IsSuccess = true, UserMessage = "İşlem başarılı", };
            var returnViewModel = new UrunEditViewModel2() { };
            #region Choice1 listesi

            var dataSubeUrunlist = mf.GetSubeDataWithQuery(mf.NewConnectionString(subeIp, dbName, sqlKullaniciName, sqlKullaniciPassword), SqlData.getLocalSablonChoice1ListSqlQuery(productGroup_, productName_, subeIdGrupList, sablonName));
            returnViewModel.SubeList = new List<ProductCompairSube>();
            returnViewModel.IsSuccess = true;

            var subeIdList = new List<long>();
            foreach (DataRow item in dataSubeUrunlist.Rows)
            {
                subeIdList.Add(mf.RTI(item, "SubeId"));
            }
            subeIdList = subeIdList.Distinct().ToList();

            foreach (var sube in subeIdList)
            {
                var drSube = dataSubeUrunlist.Select("SubeId=" + sube)[0];
                var subeName = mf.RTS(drSube, "SubeName");
                returnViewModel.SubeList.Add(new ProductCompairSube
                {
                    SubeId = sube,
                    SubeAdi = subeName,
                });
            }
            /////////////////
            //foreach (DataRow item in dataSubeUrunlist.Rows)
            //{
            //    var id = mf.RTI(item, "ProductTemplatePricePkId");
            //    var productGroup = mf.RTS(item, "ProductGroup");
            //    var productName = mf.RTS(item, "ProductName");
            //    var subeName = mf.RTS(item, "SubeName");
            //    var subeId = mf.RTI(item, "SubeId");
            //    var choice1Id = mf.RTI(item, "Choice1Id");
            //    var choice2Id = mf.RTI(item, "Choice2Id");
            //    var productId = mf.RTI(item, "ProductId");
            //    var choicePrice = mf.RTD(item, "c1price");
            //    var choice2VarMi = mf.RTI(item, "Choice2VarMi");
            //    var choiceProductName = mf.RTS(item, "ChoiceProductName");
            //    var pkId = mf.RTI(item, "Id");
            //    var isManuelInsert = mf.RTS(item, "IsManuelInsert");
            //    var pc = returnViewModel.productCompairsList.Where(x => x.ProductName == productName && x.ProductGroup == productGroup && x.ChoiceProductName == choiceProductName).FirstOrDefault();
            //    //Choice2 var Mı?
            //    if (choice2VarMi > 0 && pc.ChoiceProductName == choiceProductName)
            //    {
            //        pc.Choice2VarMi = choice2VarMi;
            //    }
            //    if (pc == null && !string.IsNullOrEmpty(choiceProductName))
            //    {
            //        pc = new ProductCompair();
            //        pc.Id = id;
            //        pc.PkId = pkId;
            //        pc.ProductGroup = productGroup;
            //        pc.ProductName = productName;
            //        pc.ProductId = productId;
            //        pc.Choice1Id = choice1Id;
            //        pc.ChoicePrice = choicePrice;
            //        pc.Choice2VarMi = choice2VarMi;
            //        pc.ChoiceProductName = choiceProductName;
            //        pc.IsManuelInsert = isManuelInsert;
            //        returnViewModel.productCompairsList.Add(pc);
            //        pc.SubeList = new List<UrunEdit2>();
            //    }
            //    var urunEdit2 = new UrunEdit2()
            //    {
            //        PkId = pkId,
            //        SubeId = subeId,
            //        SubePriceValue = choicePrice
            //    };
            //    if (!string.IsNullOrEmpty(choiceProductName))
            //    {
            //        pc.SubeList.Add(urunEdit2);
            //    }
            //}
            //foreach (var sube in returnViewModel.SubeList)
            //{
            //    foreach (var product in returnViewModel.productCompairsList)
            //    {
            //        if (!product.SubeList.Where(x => x.SubeId == sube.SubeId).Any())
            //        {
            //            product.SubeList.Add(new UrunEdit2
            //            {
            //                SubeId = sube.SubeId
            //            });
            //        }
            //    }
            //}


            returnViewModel.ProductList = dataSubeUrunlist.AsEnumerable().ToList()
                    .Select(item => new
                    {
                        ProductName = mf.RTS(item, "ProductName"),
                        ProductGroup = mf.RTS(item, "ProductGroup"),
                        ChoiceProductName = mf.RTS(item, "ChoiceProductName"),
                        //Id = mf.RTI(item, "Id"),
                        GuncellenecekSubeIdGrubu = mf.RTS(item, "GuncellenecekSubeIdGrubu"),
                    })
                    .Distinct()
                    .Select(item => new UrunEditItemViewModel
                    {
                        //Id= mf.RTI(item, "Id"),
                        //Id = item.Id,
                        ProductGroup = item.ProductGroup,
                        ProductName = item.ProductName,
                        ChoiceProductName = item.ChoiceProductName,
                        UrunEditPriceList = new List<UrunEditPrice>(),
                    })
                    .Where(x => !string.IsNullOrEmpty(x.ChoiceProductName))
                    .OrderBy(x => x.ProductGroup)
                    .ToList();

            var dbProDuctItemList = dataSubeUrunlist.AsEnumerable().ToList()
                  .Select(item => new
                  {
                      Id = mf.RTI(item, "Id"),
                      SubeId = mf.RTI(item, "SubeId"),
                      SubeName = mf.RTS(item, "SubeName"),
                      ProductName = mf.RTS(item, "ProductName"),
                      ProductGroup = mf.RTS(item, "ProductGroup"),
                      OptionsVarMi = mf.RTI(item, "OptionsVarMi"),
                      Choice1VarMi = mf.RTI(item, "Choice1VarMi"),
                      Choice2VarMi = mf.RTI(item, "Choice2VarMi"),
                      ProductId = mf.RTI(item, "ProductId"),
                      Choice1Id = mf.RTI(item, "Choice1Id"),
                      Choice2Id = mf.RTI(item, "Choice2Id"),
                      ChoicePrice = mf.RTD(item, "c1price"),
                      ChoiceProductName = mf.RTS(item, "ChoiceProductName"),
                      IsManuelInsert = mf.RTS(item, "IsManuelInsert"),
                  }).ToList()
                  .Where(x => !string.IsNullOrWhiteSpace(x.ChoiceProductName))
                  .OrderBy(x => x.ProductGroup)
                  .ToList();

            var productList = returnViewModel.ProductList.Where(x => !string.IsNullOrEmpty(x.ChoiceProductName)).OrderBy(x => x.ProductGroup).ToList();
            foreach (var productItem in productList)
            {
                foreach (var sube in returnViewModel.SubeList)
                {
                    var priceItem = dbProDuctItemList
                        .Where(x => x.SubeId == sube.SubeId)
                        .Where(x => x.ProductGroup == productItem.ProductGroup)
                        .Where(x => x.ProductName == productItem.ProductName)
                        .Where(x => x.ChoiceProductName == productItem.ChoiceProductName)
                        .FirstOrDefault();

                    var priceItemChoice2VarMi = dbProDuctItemList
                        .Where(x => x.SubeId == sube.SubeId)
                        .Where(x => x.ProductGroup == productItem.ProductGroup)
                        .Where(x => x.ProductName == productItem.ProductName)
                        .Where(x => x.ChoiceProductName == productItem.ChoiceProductName)
                        .Where(x => x.Choice2VarMi > 0)
                        .FirstOrDefault();

                    if (priceItem == null)
                    {
                        productItem.UrunEditPriceList.Add(new UrunEditPrice
                        {
                            SubeId = sube.SubeId,
                            Price = null,
                            ChoicePrice = null,
                        });
                    }
                    else
                        productItem.UrunEditPriceList.Add(new UrunEditPrice
                        {
                            Id = priceItem.Id,
                            SubeId = sube.SubeId,
                            ProductId = priceItem.ProductId,
                            Choice1Id = priceItem.Choice1Id,
                            Choice2Id = priceItem.Choice2Id,
                            ChoicePrice = priceItem.ChoicePrice,
                            Choice1VarMi = priceItem.Choice1VarMi,
                            OptionsVarMi = priceItem.OptionsVarMi,
                            Choice2VarMi = priceItemChoice2VarMi != null ? priceItemChoice2VarMi.Choice2VarMi : 0,//priceItem.Choice2VarMi,
                            IsManuelInsert = priceItem.IsManuelInsert == string.Empty ? false : Convert.ToBoolean(priceItem.IsManuelInsert),
                        });
                }
            }
            #endregion Choice1 listesi

            #region Options listesi
            #region Local database'e veriler kaydetmek için kullanıldı.
            DataTable dataSubeUrunOptionslist = mf.GetSubeDataWithQuery(mf.NewConnectionString(subeIp, dbName, sqlKullaniciName, sqlKullaniciPassword), SqlData.getLocalSablonChoice1ListSqlQuery(productGroup_, productName_, subeIdGrupList, sablonName));
            #endregion Local database'e veriler kaydetmek için kullanıldı.

            //returnViewModel.productOptionsCompairsList = new List<ProductCompair>();
            //foreach (DataRow item in dataSubeUrunOptionslist.Rows)
            //{
            //    var id = mf.RTI(item, "ProductTemplatePricePkId");
            //    var productGroup = mf.RTS(item, "ProductGroup");
            //    var productName = mf.RTS(item, "ProductName");
            //    var subeName = mf.RTS(item, "SubeName");
            //    var subeId = mf.RTI(item, "SubeId");
            //    var optionsId = mf.RTI(item, "OptionsId");
            //    var productId = mf.RTI(item, "ProductId");
            //    var optionsPrice = mf.RTD(item, "optionprice");
            //    var optionsName = mf.RTS(item, "OptionsProductName");
            //    var pkId = mf.RTI(item, "Id");
            //    var isManuelInsert = mf.RTS(item, "IsManuelInsert");
            //    var pc = returnViewModel.productOptionsCompairsList.Where(x => x.ProductName == productName && x.ProductGroup == productGroup && x.OptionsProductName == optionsName).FirstOrDefault();
            //    if (pc == null && !string.IsNullOrEmpty(optionsName))
            //    {
            //        pc = new ProductCompair();
            //        pc.Id = id;
            //        pc.PkId = pkId;
            //        pc.ProductGroup = productGroup;
            //        pc.ProductName = productName;
            //        pc.ProductId = productId;
            //        pc.OptionsId = optionsId;
            //        pc.OptionsPrice = optionsPrice;
            //        pc.OptionsProductName = optionsName;
            //        pc.IsManuelInsert = isManuelInsert;
            //        returnViewModel.productOptionsCompairsList.Add(pc);
            //        pc.SubeList = new List<UrunEdit2>();
            //    }
            //    var urunEdit2 = new UrunEdit2()
            //    {
            //        PkId = pkId,
            //        SubeId = subeId,
            //        SubePriceValue = optionsPrice
            //    };
            //    if (!string.IsNullOrEmpty(optionsName))
            //    {
            //        pc.SubeList.Add(urunEdit2);
            //    }
            //}
            //foreach (var sube in returnViewModel.SubeList)
            //{
            //    foreach (var product in returnViewModel.productOptionsCompairsList)
            //    {
            //        if (!product.SubeList.Where(x => x.SubeId == sube.SubeId).Any())
            //        {
            //            product.SubeList.Add(new UrunEdit2 { SubeId = sube.SubeId });
            //        }
            //    }
            //}
            ///////////////////////////////////////

            returnViewModel.ProductOptionsList = dataSubeUrunOptionslist.AsEnumerable().ToList()
                   .Select(item => new
                   {
                       ProductGroup = mf.RTS(item, "ProductGroup"),
                       ProductName = mf.RTS(item, "ProductName"),
                       OptionsProductName = mf.RTS(item, "OptionsProductName"),
                       GuncellenecekSubeIdGrubu = mf.RTS(item, "GuncellenecekSubeIdGrubu"),
                   })
                   .Distinct()
                   .Select(item => new UrunEditItemViewModel
                   {
                       //Id= mf.RTI(item, "Id"),
                       ProductGroup = item.ProductGroup,
                       ProductName = item.ProductName,
                       OptionsProductName = item.OptionsProductName,
                       UrunEditPriceList = new List<UrunEditPrice>(),
                   })
                   .Where(x => !string.IsNullOrEmpty(x.OptionsProductName))
                   .OrderBy(x => x.ProductGroup)
                   .ToList();

            var dbProDuctOptionsItemList = dataSubeUrunOptionslist.AsEnumerable().ToList()
                  .Select(item => new
                  {
                      Id = mf.RTI(item, "Id"),
                      SubeId = mf.RTI(item, "SubeId"),
                      SubeName = mf.RTS(item, "SubeName"),
                      OptionsVarMi = mf.RTI(item, "OptionsVarMi"),
                      Choice1VarMi = mf.RTI(item, "Choice1VarMi"),
                      ProductGroup = mf.RTS(item, "ProductGroup"),
                      ProductName = mf.RTS(item, "ProductName"),
                      IsManuelInsert = mf.RTS(item, "IsManuelInsert"),
                      ProductId = mf.RTI(item, "ProductId"),
                      ProductTemplatePricePkId = mf.RTI(item, "ProductTemplatePricePkId"),
                      OptionsId = mf.RTI(item, "OptionsId"),
                      optionprice = mf.RTD(item, "optionprice"),
                      OptionsProductName = mf.RTS(item, "OptionsProductName"),
                  }).ToList()
                  .Where(x => !string.IsNullOrEmpty(x.OptionsProductName))
                  .OrderBy(x => x.ProductGroup)
                  .ToList();

            var productOptionsList = returnViewModel.ProductOptionsList.Where(x => !string.IsNullOrEmpty(x.OptionsProductName)).OrderBy(x => x.ProductGroup).ToList();
            foreach (var productItem in productOptionsList)
            {
                foreach (var sube in returnViewModel.SubeList)
                {
                    var priceItem = dbProDuctOptionsItemList
                        .Where(x => x.SubeId == sube.SubeId)
                        .Where(x => x.ProductName == productItem.ProductName)
                        .Where(x => x.ProductGroup == productItem.ProductGroup)
                        .Where(x => x.OptionsProductName == productItem.OptionsProductName)
                        .FirstOrDefault();

                    if (priceItem == null)
                    {
                        productItem.UrunEditPriceList.Add(new UrunEditPrice { SubeId = sube.SubeId, Price = null, OptionsPrice = null });
                    }
                    else
                        productItem.UrunEditPriceList.Add(new UrunEditPrice
                        {
                            Id = priceItem.Id,
                            SubeId = sube.SubeId,
                            Choice1VarMi = priceItem.Choice1VarMi,
                            OptionsVarMi = priceItem.OptionsVarMi,
                            ProductId = priceItem.ProductId,
                            OptionsId = priceItem.OptionsId,
                            OptionsPrice = priceItem.optionprice,
                            IsManuelInsert = priceItem.IsManuelInsert == string.Empty ? false : Convert.ToBoolean(priceItem.IsManuelInsert),
                        });
                }
            }

            #endregion Options listesi

            returnViewModel.IsSuccess = true;
            return returnViewModel;
        }
        public ActionResultMessages UpdateBySablonChoice1ForSubeList(UrunEditViewModel model)
        {
            var result = new ActionResultMessages() { IsSuccess = true, UserMessage = "İşlem Başarılı", };
            var kullaniciData = BussinessHelper.GetByUserInfoForId("", "", model.KullaniciId);
            var sqlData = new SqlData(new SqlConnection(mf.NewConnectionString(subeIp, dbName, sqlKullaniciName, sqlKullaniciPassword)));
            var dataSubeUrunlist = mf.GetSubeDataWithQuery(mf.NewConnectionString(subeIp, dbName, sqlKullaniciName, sqlKullaniciPassword),
                                        SqlData.getUpdatedSablonChoice1ListSqlQuery
                                        (
                                            model.productCompairsList.FirstOrDefault().ProductGroup,
                                            model.productCompairsList.FirstOrDefault().ProductName,
                                            model.SubeIdGrupList,
                                            model.SablonName
                                          ));

            foreach (var modelProduct in model.productCompairsList)
            {
                foreach (DataRow dataUrunSube in dataSubeUrunlist.Rows)
                {
                    foreach (var price in modelProduct.SubeList)
                    {
                        var pn = mf.RTS(dataUrunSube, "ProductName");
                        var pn1 = mf.RTS(dataUrunSube, "ProductGroup");
                        var choiceProductName = mf.RTS(dataUrunSube, "ChoiceProductName");
                        var subeId = mf.RTI(dataUrunSube, "SubeId");
                        var c1price = mf.RTD(dataUrunSube, "c1price");
                        var pn6 = mf.RTS(dataUrunSube, "ProductId");
                        var pn7 = mf.RTS(dataUrunSube, "Choice1Id");
                        var pn9 = mf.RTS(dataUrunSube, "ProductTemplatePricePkId");
                        var p10 = mf.RTD(dataUrunSube, "Id");
                        var p11 = mf.RTD(dataUrunSube, "PkId");

                        //if (price.SubeId == mf.RTD(dataUrunSube, "SubeId") &&                            
                        //   price.SubePriceValue != null &&
                        //   price.SubePriceValue != mf.RTD(dataUrunSube, "productprice") &&
                        //   price.PkId == mf.RTD(dataUrunSube, "Id")
                        //   )
                        //if (price.SubePriceValue != mf.RTD(dataUrunSube, "c1price") &&
                        //    price.PkId == mf.RTD(dataUrunSube, "Id") &&
                        //    modelProduct.ProductId == mf.RTI(dataUrunSube, "ProductId") 
                        //    //modelProduct.Id == mf.RTD(dataUrunSube, "ProductTemplatePricePkId")
                        //    )
                        if (
                            //c1price != 0 &&
                            price.SubeId == subeId &&
                            price.SubePriceValue != mf.RTD(dataUrunSube, "c1price") &&
                            modelProduct.ChoiceProductName == choiceProductName
                           )
                        {
                            sqlData.ExecuteSql("update ProductTemplatePrice set Price=@par1 Where SubeId=@par2 and  Id=@par3   ",
                             new object[]
                                  {
                                    price.SubePriceValue,
                                    price.SubeId,
                                    price.PkId,
                                      //modelProduct.Choice1Id,
                                      //modelProduct.ProductGroup,
                                      //modelProduct.ProductName
                                  });

                            //sqlData.executeSql(" Insert Into  Choice1Tarihce( ProductId, Name, Price, SubeId, SubeName, Choice1PkId,IsUpdateDate,IsUpdateKullanici ) Values( @par1, @par2, @par3, @par4, @par5,@par6,@par7,@par8 )",
                            //     new object[]
                            //     {
                            //         mf.RTI(dataUrunSube, "ProductId"),
                            //         mf.RTS(dataUrunSube, "ChoiceProductName"),
                            //         mf.RTD(dataUrunSube, "Choice1_Price"),
                            //         model.SubeId,
                            //         model.SubeList.Where(x=>x.SubeId==price.SubeId).FirstOrDefault().SubeAdi,
                            //         mf.RTI(dataUrunSube, "Id"),
                            //         DateTime.Now.Date.ToString("dd'/'MM'/'yyyy HH:mm"),
                            //         kullaniciData.UserName
                            //     });
                            break;
                        }
                    }
                }
            }

            return result;
        }

        public ActionResultMessages UpdateBySablonChoice1ForSubeList2(UrunEditViewModel2 model)
        {
            var result = new ActionResultMessages() { IsSuccess = true, UserMessage = "İşlem Başarılı", };
            var kullaniciData = BussinessHelper.GetByUserInfoForId("", "", model.KullaniciId);
            var sqlData = new SqlData(new SqlConnection(mf.NewConnectionString(subeIp, dbName, sqlKullaniciName, sqlKullaniciPassword)));
            var dataSubeUrunlist = mf.GetSubeDataWithQuery(mf.NewConnectionString(subeIp, dbName, sqlKullaniciName, sqlKullaniciPassword),
                                        SqlData.getUpdatedSablonChoice1ListSqlQuery
                                        (
                                            model.ProductList.FirstOrDefault().ProductGroup,
                                            model.ProductList.FirstOrDefault().ProductName,
                                            model.SubeIdGrupList,
                                            model.SablonName
                                          ));


            var updateUrunList = dataSubeUrunlist.AsEnumerable().ToList()
                    .Select(item => new
                    {
                        ProductGroup = mf.RTS(item, "ProductGroup"),
                        ProductName = mf.RTS(item, "ProductName"),
                        ChoiceProductName = mf.RTS(item, "ChoiceProductName"),
                        GuncellenecekSubeIdGrubu = mf.RTS(item, "GuncellenecekSubeIdGrubu"),
                    })
                    .Distinct()
                    .Select(item => new UrunEditItemViewModel
                    {
                        //Id= mf.RTI(item, "Id"),
                        ProductGroup = item.ProductGroup,
                        ProductName = item.ProductName,
                        ChoiceProductName = item.ChoiceProductName,
                        UrunEditPriceList = new List<UrunEditPrice>(),
                    })
                    .Where(x => !string.IsNullOrEmpty(x.ChoiceProductName))
                    .OrderBy(x => x.ProductGroup)
                    .ToList();


            foreach (var modelProduct in model.ProductList)
            {
                foreach (DataRow dataUrunSube in dataSubeUrunlist.Rows)
                {
                    foreach (var price in modelProduct.UrunEditPriceList)
                    {
                        var pn = mf.RTS(dataUrunSube, "ProductName");
                        var pn1 = mf.RTS(dataUrunSube, "ProductGroup");
                        var choiceProductName = mf.RTS(dataUrunSube, "ChoiceProductName");
                        var subeId = mf.RTI(dataUrunSube, "SubeId");
                        var c1price = mf.RTD(dataUrunSube, "c1price");
                        var pn6 = mf.RTS(dataUrunSube, "ProductId");
                        var pn7 = mf.RTS(dataUrunSube, "Choice1Id");
                        var pn9 = mf.RTS(dataUrunSube, "ProductTemplatePricePkId");
                        var p10 = mf.RTD(dataUrunSube, "Id");
                        var p11 = mf.RTD(dataUrunSube, "PkId");

                        if (
                            price.SubeId == subeId &&
                            price.Id == mf.RTD(dataUrunSube, "Id") &&
                            price.ChoicePrice != mf.RTD(dataUrunSube, "c1price") &&
                            modelProduct.ChoiceProductName == choiceProductName
                           )
                        {
                            sqlData.ExecuteSql("update ProductTemplatePrice set Price=@par1 Where SubeId=@par2 and  Id=@par3   ",
                             new object[]
                                  {
                                    price.ChoicePrice,
                                    price.SubeId,
                                    price.Id,
                                  });
                            break;
                        }
                    }
                }
            }

            return result;
        }


        //public ActionResultMessages UpdateByOptionsForSubeList(UrunEditViewModel2 model, bool commonOptionsUpdated)
        //{
        //    var result = new ActionResultMessages()
        //    {
        //        IsSuccess = true,
        //        UserMessage = "İşlem Başarılı",
        //    };

        //    var kullaniciData = BussinessHelper.GetByUserInfoForId("", "", model.KullaniciId);

        //    #region Local database'e veriler kaydetmek için kullanıldı.
        //    //var productId = model.ProductOptionsList.Select(x => x.ProductId).FirstOrDefault();

        //    var sqlData = new SqlData(new SqlConnection(mf.NewConnectionString(subeIp, dbName, sqlKullaniciName, sqlKullaniciPassword)));

        //    DataTable dataSubeUrunlist;
        //    if (commonOptionsUpdated)
        //    {
        //        dataSubeUrunlist = mf.GetSubeDataWithQuery(mf.NewConnectionString(subeIp, dbName, sqlKullaniciName, sqlKullaniciPassword),
        //                                               SqlData.getLocalSablonCommonOptionsListSqlQuery("", "", model.ProductOptionsList.FirstOrDefault().OptionsProductName, model.SubeIdGrupList));
        //    }
        //    else
        //    {
        //        dataSubeUrunlist = mf.GetSubeDataWithQuery(mf.NewConnectionString(subeIp, dbName, sqlKullaniciName, sqlKullaniciPassword),
        //                                               SqlData.getUpdatedSablonOptionsListSqlQuery
        //                                               (model.ProductOptionsList.FirstOrDefault().ProductGroup,
        //                                                model.ProductOptionsList.FirstOrDefault().ProductName,
        //                                                model.SubeIdGrupList,
        //                                                model.SablonName));
        //    }


        //    #endregion Local database'e veriler kaydetmek için kullanıldı.

        //    foreach (var modelProduct in model.ProductOptionsList)
        //    {
        //        foreach (DataRow dataUrunSube in dataSubeUrunlist.Rows)
        //        {
        //            foreach (var price in modelProduct.UrunEditPriceList)
        //            {
        //                var pn = mf.RTS(dataUrunSube, "ProductName");
        //                var pn1 = mf.RTS(dataUrunSube, "ProductGroup");
        //                var optionsProductName = mf.RTS(dataUrunSube, "OptionsProductName");
        //                var subeId = mf.RTI(dataUrunSube, "SubeId");
        //                var optionprice = mf.RTD(dataUrunSube, "optionprice");
        //                var pn6 = mf.RTS(dataUrunSube, "ProductId");
        //                var pn7 = mf.RTS(dataUrunSube, "OptionsId");
        //                var pn8 = mf.RTS(dataUrunSube, "Id");
        //                var pn9 = mf.RTS(dataUrunSube, "ProductTemplatePricePkId");

        //                if (
        //                    price.SubeId == subeId &&
        //                    price.Price != mf.RTD(dataUrunSube, "optionprice") &&
        //                    modelProduct.OptionsProductName == optionsProductName
        //                 )
        //                {
        //                    sqlData.ExecuteSql("Update ProductTemplatePrice set Price=@par1 Where SubeId=@par2 and Id=@par3 ",
        //                     new object[]
        //                          {
        //                            price.Price,
        //                            price.SubeId,
        //                            price.Id,
        //                          });
        //                }
        //            }
        //        }
        //    }

        //    return result;
        //}

        public ActionResultMessages UpdateByOptionsForSubeList2(UrunEditViewModel2 model, bool commonOptionsUpdated)
        {
            var result = new ActionResultMessages()
            {
                IsSuccess = true,
                UserMessage = "İşlem Başarılı",
            };

            var kullaniciData = BussinessHelper.GetByUserInfoForId("", "", model.KullaniciId);

            #region Local database'e veriler kaydetmek için kullanıldı.
            //var productId = model.ProductOptionsList.Select(x => x.ProductId).FirstOrDefault();

            var sqlData = new SqlData(new SqlConnection(mf.NewConnectionString(subeIp, dbName, sqlKullaniciName, sqlKullaniciPassword)));

            DataTable dataSubeUrunlist;
            if (commonOptionsUpdated)
            {
                dataSubeUrunlist = mf.GetSubeDataWithQuery(mf.NewConnectionString(subeIp, dbName, sqlKullaniciName, sqlKullaniciPassword),
                                                       SqlData.getLocalSablonCommonOptionsListSqlQuery("", "", model.ProductOptionsList.FirstOrDefault().OptionsProductName, model.SubeIdGrupList));
            }
            else
            {
                dataSubeUrunlist = mf.GetSubeDataWithQuery(mf.NewConnectionString(subeIp, dbName, sqlKullaniciName, sqlKullaniciPassword),
                                                       SqlData.getUpdatedSablonOptionsListSqlQuery
                                                       (model.ProductOptionsList.FirstOrDefault().ProductGroup,
                                                        model.ProductOptionsList.FirstOrDefault().ProductName,
                                                        model.SubeIdGrupList,
                                                        model.SablonName));
            }


            #endregion Local database'e veriler kaydetmek için kullanıldı.

            foreach (var modelProduct in model.ProductOptionsList)
            {
                foreach (DataRow dataUrunSube in dataSubeUrunlist.Rows)
                {
                    foreach (var price in modelProduct.UrunEditPriceList)
                    {
                        var pn = mf.RTS(dataUrunSube, "ProductName");
                        var pn1 = mf.RTS(dataUrunSube, "ProductGroup");
                        var optionsProductName = mf.RTS(dataUrunSube, "OptionsProductName");
                        var subeId = mf.RTI(dataUrunSube, "SubeId");
                        var optionprice = mf.RTD(dataUrunSube, "optionprice");
                        var pn6 = mf.RTS(dataUrunSube, "ProductId");
                        var pn7 = mf.RTS(dataUrunSube, "OptionsId");
                        var pn8 = mf.RTS(dataUrunSube, "Id");
                        var pn9 = mf.RTS(dataUrunSube, "ProductTemplatePricePkId");

                        if (
                            price.SubeId == subeId
                            && price.OptionsPrice != mf.RTD(dataUrunSube, "optionprice")
                            && modelProduct.OptionsProductName == optionsProductName
                            && price.Id == mf.RTI(dataUrunSube, "Id")
                         )
                        {
                            sqlData.ExecuteSql("Update ProductTemplatePrice set Price=@par1 Where SubeId=@par2 and Id=@par3 ",
                             new object[]
                                  {
                                    price.OptionsPrice,
                                    price.SubeId,
                                    price.Id,
                                  });
                        }
                    }
                }
            }

            return result;
        }
        #endregion Choice1 update

        #region Choice2 update
        //public UrunEditViewModel GetByChoice2ForSubeListEdit(string productGroup_, string productName_, string subeIdGrupList, string sablonName)
        //{
        //    var result = new ActionResultMessages { IsSuccess = true, UserMessage = "İşlem başarılı", };
        //    var returnViewModel = new UrunEditViewModel() { };
        //    returnViewModel.SubeList = new List<ProductCompairSube>();
        //    returnViewModel.productCompairsList = new List<ProductCompair>();
        //    returnViewModel.IsSuccess = true;

        //    try
        //    {
        //        #region Choice1 

        //        #region Local database'e veriler kaydetmek için kullanıldı.
        //        var sqlData = new SqlData(new SqlConnection(mf.NewConnectionString(subeIp, dbName, sqlKullaniciName, sqlKullaniciPassword)));
        //        DataTable dataSubeUrunlist = mf.GetSubeDataWithQuery(mf.NewConnectionString(subeIp, dbName, sqlKullaniciName, sqlKullaniciPassword),
        //                                     SqlData.getLocalSablonChoice1ListSqlQuery(productGroup_, productName_, subeIdGrupList, sablonName));
        //        #endregion Local database'e veriler kaydetmek için kullanıldı.

        //        var subeIdList = new List<long>();
        //        foreach (DataRow item in dataSubeUrunlist.Rows)
        //        {
        //            subeIdList.Add(mf.RTI(item, "SubeId"));
        //        }

        //        subeIdList = subeIdList.Distinct().ToList();
        //        foreach (var sube in subeIdList)
        //        {
        //            var drSube = dataSubeUrunlist.Select("SubeId=" + sube)[0];
        //            var subeName = mf.RTS(drSube, "SubeName");
        //            returnViewModel.SubeList.Add(new ProductCompairSube
        //            {
        //                SubeId = sube,
        //                SubeAdi = subeName,
        //            });
        //        }

        //        foreach (DataRow item in dataSubeUrunlist.Rows)
        //        {
        //            var id = mf.RTI(item, "ProductTemplatePricePkId");
        //            var productGroup = mf.RTS(item, "ProductGroup");
        //            var productName = mf.RTS(item, "ProductName");
        //            var subeName = mf.RTS(item, "SubeName");
        //            var subeId = mf.RTI(item, "SubeId");
        //            var choice1Id = mf.RTI(item, "Choice1Id");
        //            var choice2Id = mf.RTI(item, "Choice2Id");
        //            var productId = mf.RTI(item, "ProductId");
        //            var choice2Price = mf.RTD(item, "c2price");
        //            var choice2ProductName = mf.RTS(item, "Choice2ProductName");
        //            var choice2VarMi = mf.RTI(item, "Choice2VarMi");
        //            var isManuelInsert = mf.RTS(item, "IsManuelInsert");

        //            var pc = returnViewModel.productCompairsList.Where(x => x.ProductName == productName && x.ProductGroup == productGroup && x.ChoiceProductName == choice2ProductName).FirstOrDefault();
        //            if (pc == null && !string.IsNullOrEmpty(choice2ProductName))
        //            {
        //                pc = new ProductCompair
        //                {
        //                    Id = id,
        //                    ProductGroup = productGroup,
        //                    ProductName = productName,
        //                    ProductId = productId,
        //                    Choice1Id = choice1Id,
        //                    Choice2Id = choice2Id,
        //                    ChoicePrice = choice2Price,
        //                    Choice2VarMi = choice2VarMi,
        //                    ChoiceProductName = choice2ProductName,
        //                    IsManuelInsert = isManuelInsert
        //                };
        //                returnViewModel.productCompairsList.Add(pc);
        //                pc.SubeList = new List<UrunEdit2>();
        //            }

        //            var urunEdit2 = new UrunEdit2()
        //            {
        //                SubeId = subeId,
        //                SubePriceValue = choice2Price
        //            };

        //            if (!string.IsNullOrEmpty(choice2ProductName))
        //            {
        //                pc.SubeList.Add(urunEdit2);
        //            }
        //        }

        //        foreach (var sube in returnViewModel.SubeList)
        //        {
        //            foreach (var product in returnViewModel.productCompairsList)
        //            {
        //                if (!product.SubeList.Where(x => x.SubeId == sube.SubeId).Any())
        //                {
        //                    product.SubeList.Add(new UrunEdit2 { SubeId = sube.SubeId });
        //                }
        //            }
        //        }
        //        #endregion Choice1 listesi
        //    }
        //    catch (Exception ex)
        //    {
        //        Singleton.WritingLogFile2("SPosSubeSablonFiyatGuncellemeCRUD_GetByChoice2ForSubeListEdit", ex.Message.ToString(), "", ex.StackTrace);
        //        returnViewModel.IsSuccess = false;
        //        returnViewModel.ErrorList.Add("Bir hata oluştu.");
        //        return returnViewModel;
        //    }

        //    returnViewModel.IsSuccess = true;
        //    return returnViewModel;
        //}
        public UrunEditViewModel2 GetByChoice2ForSubeListEdit2(string productGroup_, string productName_, string subeIdGrupList, string sablonName)
        {
            var result = new ActionResultMessages { IsSuccess = true, UserMessage = "İşlem başarılı", };
            var returnViewModel = new UrunEditViewModel2() { };
            returnViewModel.SubeList = new List<ProductCompairSube>();
            returnViewModel.IsSuccess = true;

            try
            {
                #region Choice1 

                #region Local database'e veriler kaydetmek için kullanıldı.
                var sqlData = new SqlData(new SqlConnection(mf.NewConnectionString(subeIp, dbName, sqlKullaniciName, sqlKullaniciPassword)));
                DataTable dataSubeUrunlist = mf.GetSubeDataWithQuery(mf.NewConnectionString(subeIp, dbName, sqlKullaniciName, sqlKullaniciPassword),
                                             SqlData.getLocalSablonChoice1ListSqlQuery(productGroup_, productName_, subeIdGrupList, sablonName));
                #endregion Local database'e veriler kaydetmek için kullanıldı.

                //foreach (DataRow item in dataSubeUrunlist.Rows)
                //{
                //    var id = mf.RTI(item, "ProductTemplatePricePkId");
                //    var productGroup = mf.RTS(item, "ProductGroup");
                //    var productName = mf.RTS(item, "ProductName");
                //    var subeName = mf.RTS(item, "SubeName");
                //    var subeId = mf.RTI(item, "SubeId");
                //    var choice1Id = mf.RTI(item, "Choice1Id");
                //    var choice2Id = mf.RTI(item, "Choice2Id");
                //    var productId = mf.RTI(item, "ProductId");
                //    var choice2Price = mf.RTD(item, "c2price");
                //    var choice2ProductName = mf.RTS(item, "Choice2ProductName");
                //    var choice2VarMi = mf.RTI(item, "Choice2VarMi");
                //    var isManuelInsert = mf.RTS(item, "IsManuelInsert");

                //    var pc = returnViewModel.productCompairsList.Where(x => x.ProductName == productName && x.ProductGroup == productGroup && x.ChoiceProductName == choice2ProductName).FirstOrDefault();
                //    if (pc == null && !string.IsNullOrEmpty(choice2ProductName))
                //    {
                //        pc = new ProductCompair
                //        {
                //            Id = id,
                //            ProductGroup = productGroup,
                //            ProductName = productName,
                //            ProductId = productId,
                //            Choice1Id = choice1Id,
                //            Choice2Id = choice2Id,
                //            ChoicePrice = choice2Price,
                //            Choice2VarMi = choice2VarMi,
                //            ChoiceProductName = choice2ProductName,
                //            IsManuelInsert = isManuelInsert
                //        };
                //        returnViewModel.productCompairsList.Add(pc);
                //        pc.SubeList = new List<UrunEdit2>();
                //    }

                //    var urunEdit2 = new UrunEdit2()
                //    {
                //        SubeId = subeId,
                //        SubePriceValue = choice2Price
                //    };

                //    if (!string.IsNullOrEmpty(choice2ProductName))
                //    {
                //        pc.SubeList.Add(urunEdit2);
                //    }
                //}

                returnViewModel.SubeList = new List<ProductCompairSube>();
                returnViewModel.IsSuccess = true;

                var subeIdList = new List<long>();
                foreach (DataRow item in dataSubeUrunlist.Rows)
                {
                    subeIdList.Add(mf.RTI(item, "SubeId"));
                }
                subeIdList = subeIdList.Distinct().ToList();

                foreach (var sube in subeIdList)
                {
                    var drSube = dataSubeUrunlist.Select("SubeId=" + sube)[0];
                    var subeName = mf.RTS(drSube, "SubeName");
                    returnViewModel.SubeList.Add(new ProductCompairSube
                    {
                        SubeId = sube,
                        SubeAdi = subeName,
                    });
                }

                returnViewModel.ProductList = dataSubeUrunlist.AsEnumerable().ToList()
                        .Select(item => new
                        {
                            ProductGroup = mf.RTS(item, "ProductGroup"),
                            ProductName = mf.RTS(item, "ProductName"),
                            ChoiceProductName = mf.RTS(item, "Choice2ProductName"),
                            GuncellenecekSubeIdGrubu = mf.RTS(item, "GuncellenecekSubeIdGrubu"),
                        })
                        .Distinct()
                        .Select(item => new UrunEditItemViewModel
                        {
                            //Id= mf.RTI(item, "Id"),
                            ProductGroup = item.ProductGroup,
                            ProductName = item.ProductName,
                            ChoiceProductName = item.ChoiceProductName,
                            UrunEditPriceList = new List<UrunEditPrice>(),
                        })
                        .Where(x => !string.IsNullOrEmpty(x.ChoiceProductName))
                        .OrderBy(x => x.ProductGroup)
                        .ToList();

                var dbProDuctItemList = dataSubeUrunlist.AsEnumerable().ToList()
                      .Select(item => new
                      {
                          Id = mf.RTI(item, "Id"),
                          SubeId = mf.RTI(item, "SubeId"),
                          SubeName = mf.RTS(item, "SubeName"),
                          ProductName = mf.RTS(item, "ProductName"),
                          ProductGroup = mf.RTS(item, "ProductGroup"),
                          ProductId = mf.RTI(item, "ProductId"),
                          Choice1Id = mf.RTI(item, "Choice1Id"),
                          Choice2Id = mf.RTI(item, "Choice2Id"),
                          ChoicePrice = mf.RTD(item, "c2price"),
                          Choice2VarMi = mf.RTI(item, "Choice2VarMi"),
                          ChoiceProductName = mf.RTS(item, "Choice2ProductName"),
                          IsManuelInsert = mf.RTS(item, "IsManuelInsert"),
                      }).ToList()
                      .Where(x => !string.IsNullOrWhiteSpace(x.ChoiceProductName))
                      .OrderBy(x => x.ProductGroup)
                      .ToList();
                var productList = returnViewModel.ProductList.Where(x => !string.IsNullOrEmpty(x.ChoiceProductName)).OrderBy(x => x.ProductGroup).ToList();
                foreach (var productItem in productList)
                {
                    foreach (var sube in returnViewModel.SubeList)
                    {
                        var priceItem = dbProDuctItemList
                            .Where(x => x.SubeId == sube.SubeId)
                            .Where(x => x.ProductGroup == productItem.ProductGroup)
                            .Where(x => x.ProductName == productItem.ProductName)
                             .Where(x => x.ChoiceProductName == productItem.ChoiceProductName)
                            .FirstOrDefault();

                        if (priceItem == null)
                        {
                            productItem.UrunEditPriceList.Add(new UrunEditPrice
                            {
                                SubeId = sube.SubeId,
                                Price = null,
                                ChoicePrice = null,
                            });
                        }
                        else
                            productItem.UrunEditPriceList.Add(new UrunEditPrice
                            {
                                Id = priceItem.Id,
                                SubeId = sube.SubeId,
                                ProductId = priceItem.ProductId,
                                Choice1Id = priceItem.Choice1Id,
                                Choice2Id = priceItem.Choice2Id,
                                ChoicePrice = priceItem.ChoicePrice,
                                Choice2VarMi = priceItem.Choice2VarMi,
                                IsManuelInsert = priceItem.IsManuelInsert == string.Empty ? false : Convert.ToBoolean(priceItem.IsManuelInsert),
                            });
                    }
                }
                #endregion Choice1 listesi
            }
            catch (Exception ex)
            {
                Singleton.WritingLogFile2("SPosSubeSablonFiyatGuncellemeCRUD_GetByChoice2ForSubeListEdit", ex.Message.ToString(), "", ex.StackTrace);
                returnViewModel.IsSuccess = false;
                returnViewModel.ErrorList.Add("Bir hata oluştu.");
                return returnViewModel;
            }

            returnViewModel.IsSuccess = true;
            return returnViewModel;
        }

        public ActionResultMessages UpdateByChoice2ForSubeList(UrunEditViewModel model)
        {
            var result = new ActionResultMessages() { IsSuccess = true, UserMessage = "İşlem Başarılı", };
            var kullaniciData = BussinessHelper.GetByUserInfoForId("", "", model.KullaniciId);
            var sqlData = new SqlData(new SqlConnection(mf.NewConnectionString(subeIp, dbName, sqlKullaniciName, sqlKullaniciPassword)));
            var dataSubeUrunlist = mf.GetSubeDataWithQuery(mf.NewConnectionString(subeIp, dbName, sqlKullaniciName, sqlKullaniciPassword),
                                        SqlData.getLocalSablonChoice1ListSqlQuery(model.productCompairsList.FirstOrDefault().ProductGroup,
                                                                                  model.productCompairsList.FirstOrDefault().ProductName,
                                                                                  model.SubeIdGrupList,
                                                                                  model.SablonName
                                                                                  ));

            foreach (var modelProduct in model.productCompairsList)
            {
                foreach (DataRow dataUrunSube in dataSubeUrunlist.Rows)
                {
                    foreach (var price in modelProduct.SubeList)
                    {
                        var pn = mf.RTS(dataUrunSube, "ProductName");
                        var pn1 = mf.RTS(dataUrunSube, "ProductGroup");
                        var pn3 = mf.RTS(dataUrunSube, "Choice2ProductName");
                        var pn4 = mf.RTS(dataUrunSube, "SubeId");
                        var pn5 = mf.RTS(dataUrunSube, "c2price");
                        var pn6 = mf.RTS(dataUrunSube, "ProductId");
                        var pn7 = mf.RTS(dataUrunSube, "Choice1Id");
                        var pn9 = mf.RTS(dataUrunSube, "ProductTemplatePricePkId");
                        var pn10 = mf.RTS(dataUrunSube, "Choice2Id");

                        //if (modelProduct.ProductName == mf.RTS(dataUrunSube, "ProductName")
                        //     && modelProduct.ProductGroup == mf.RTS(dataUrunSube, "ProductGroup")
                        //     && modelProduct.ChoiceProductName == mf.RTS(dataUrunSube, "Choice2ProductName")
                        //     && price.SubeId == mf.RTD(dataUrunSube, "SubeId")
                        //     && price.SubePriceValue != mf.RTD(dataUrunSube, "c2price")
                        //    )
                        if (modelProduct.ProductId == mf.RTI(dataUrunSube, "ProductId")
                              && price.SubeId == mf.RTD(dataUrunSube, "SubeId")
                              && price.SubePriceValue != mf.RTD(dataUrunSube, "c2price")
                              && modelProduct.Id == mf.RTD(dataUrunSube, "ProductTemplatePricePkId")
                              && modelProduct.Choice2Id == mf.RTD(dataUrunSube, "Choice2Id")
                            )
                        {
                            sqlData.ExecuteSql("update ProductTemplatePrice set Price=@par1 Where SubeId=@par2 and  ProductTemplatePricePkId=@par3 and Choice2Id=@par4  ",
                             new object[]
                                  {
                                        price.SubePriceValue,
                                        price.SubeId,
                                        modelProduct.Id,
                                        modelProduct.Choice2Id,
                                      //modelProduct.ProductGroup,
                                      //modelProduct.ProductName
                                  });

                            //sqlData.executeSql(" Insert Into  Choice2Tarihce( ProductId, Choice1Id, Name, Price, SubeId,SubeName, Choice2PkId, IsUpdateDate,IsUpdateKullanici ) Values( @par1, @par2, @par3, @par4, @par5,@par6 )",
                            //          new object[]
                            //          {
                            //          mf.RTI(dataUrunSube, "ProductId"),
                            //          mf.RTI(dataUrunSube, "Id"),
                            //          mf.RTS(dataUrunSube, "ChoiceProductName"),
                            //          mf.RTD(dataUrunSube, "Choice2_Price"),
                            //          model.SubeId,
                            //          model.SubeList.Where(x=>x.SubeId==price.SubeId).FirstOrDefault().SubeAdi,
                            //          mf.RTI(dataUrunSube, "Id")
                            //          });

                            break;
                        }
                    }
                }
            }

            return result;
        }


        /// <summary>
        /// //
        /// </summary>
        /// <param name="_productGroup"></param>
        /// <param name="_productName"></param>
        /// <param name="_optionsName"></param>
        /// <param name="subeIdGrupList"></param>
        /// <returns></returns>
        public ActionResultMessages UpdateByChoice2ForSubeList2(UrunEditViewModel2 model)
        {
            var result = new ActionResultMessages() { IsSuccess = true, UserMessage = "İşlem Başarılı", };
            var kullaniciData = BussinessHelper.GetByUserInfoForId("", "", model.KullaniciId);
            var sqlData = new SqlData(new SqlConnection(mf.NewConnectionString(subeIp, dbName, sqlKullaniciName, sqlKullaniciPassword)));
            var dataSubeUrunlist = mf.GetSubeDataWithQuery(mf.NewConnectionString(subeIp, dbName, sqlKullaniciName, sqlKullaniciPassword),
                                                                                   SqlData.getLocalSablonChoice1ListSqlQuery(model.ProductList.FirstOrDefault().ProductGroup,
                                                                                   model.ProductList.FirstOrDefault().ProductName,
                                                                                   model.SubeIdGrupList,
                                                                                   model.SablonName));
            foreach (var modelProduct in model.ProductList)
            {
                foreach (DataRow dataUrunSube in dataSubeUrunlist.Rows)
                {
                    foreach (var price in modelProduct.UrunEditPriceList)
                    {
                        var pn = mf.RTS(dataUrunSube, "ProductName");
                        var pn1 = mf.RTS(dataUrunSube, "ProductGroup");
                        var pn3 = mf.RTS(dataUrunSube, "Choice2ProductName");
                        var pn4 = mf.RTS(dataUrunSube, "SubeId");
                        var pn5 = mf.RTS(dataUrunSube, "c2price");
                        var pn6 = mf.RTS(dataUrunSube, "ProductId");
                        var pn7 = mf.RTS(dataUrunSube, "Choice1Id");
                        var pn9 = mf.RTS(dataUrunSube, "ProductTemplatePricePkId");
                        var pn10 = mf.RTS(dataUrunSube, "Choice2Id");

                        if (
                            //price.ProductId == mf.RTI(dataUrunSube, "ProductId")
                            //&& 
                            price.SubeId == mf.RTD(dataUrunSube, "SubeId")
                            && price.ChoicePrice != mf.RTD(dataUrunSube, "c2price")
                            //&& modelProduct.Id == mf.RTD(dataUrunSube, "ProductTemplatePricePkId")
                            && price.Id == mf.RTD(dataUrunSube, "Id")
                            && price.Choice2Id == mf.RTD(dataUrunSube, "Choice2Id")
                            )
                        {
                            //sqlData.ExecuteSql("update ProductTemplatePrice set Price=@par1 Where SubeId=@par2 and  ProductTemplatePricePkId=@par3 and Choice2Id=@par4  ",
                            sqlData.ExecuteSql("update ProductTemplatePrice set Price=@par1 Where SubeId=@par2 and  Id=@par3 and Choice2Id=@par4  ",
                                new object[]
                                     {
                                        price.ChoicePrice,
                                        price.SubeId,
                                        price.Id,
                                        price.Choice2Id,
                                     });
                            break;
                        }
                    }
                }
            }

            return result;
        }


        #endregion Choice2 update


        #region Şubelerdeki ortak options(Options table) update yapma
        public UrunEditViewModel2 GetBySablonCommonOptionsForSubeListEdit(string _productGroup, string _productName, string _optionsName, string subeIdGrupList)
        {
            var result = new ActionResultMessages { IsSuccess = true, UserMessage = "İşlem başarılı", };

            #region Options listesi
            var returnViewModel = new UrunEditViewModel2() { };
            returnViewModel.SubeList = new List<ProductCompairSube>();
            returnViewModel.IsSuccess = true;
            var subeIdList = new List<long>();

            DataTable dataSubeUrunOptionslist = mf.GetSubeDataWithQuery(mf.NewConnectionString(subeIp, dbName, sqlKullaniciName, sqlKullaniciPassword),
                                                           SqlData.getLocalSablonCommonOptionsListSqlQuery(_productGroup, _productName, _optionsName, subeIdGrupList));

            foreach (DataRow item in dataSubeUrunOptionslist.Rows)
            {
                subeIdList.Add(mf.RTI(item, "SubeId"));
            }

            subeIdList = subeIdList.Distinct().ToList();
            foreach (var sube in subeIdList)
            {
                var drSube = dataSubeUrunOptionslist.Select("SubeId=" + sube)[0];
                var subeName = mf.RTS(drSube, "SubeName");
                returnViewModel.SubeList.Add(new ProductCompairSube
                {
                    SubeId = sube,
                    SubeAdi = subeName,
                });
            }

            //returnViewModel.productOptionsCompairsList = new List<ProductCompair>();
            //foreach (DataRow item in dataSubeUrunOptionslist.Rows)
            //{
            //    var id = mf.RTI(item, "ProductTemplatePricePkId");
            //    var productGroup = mf.RTS(item, "ProductGroup");
            //    var productName = mf.RTS(item, "ProductName");
            //    var subeName = mf.RTS(item, "SubeName");
            //    var subeId = mf.RTI(item, "SubeId");
            //    var optionsId = mf.RTI(item, "OptionsId");
            //    var productId = mf.RTI(item, "ProductId");
            //    var optionsPrice = mf.RTD(item, "optionprice");
            //    var optionsName = mf.RTS(item, "OptionsProductName");
            //    var pkId = mf.RTI(item, "Id");
            //    var pc = returnViewModel.productOptionsCompairsList.Where(x => x.ProductName == productName && x.ProductGroup == productGroup && x.OptionsProductName == optionsName).FirstOrDefault();
            //    if (pc == null && !string.IsNullOrEmpty(optionsName))
            //    {
            //        pc = new ProductCompair
            //        {
            //            Id = id,
            //            PkId = pkId,
            //            ProductGroup = productGroup,
            //            ProductName = productName,
            //            ProductId = productId,
            //            OptionsId = optionsId,
            //            OptionsPrice = optionsPrice,
            //            OptionsProductName = optionsName
            //        };
            //        returnViewModel.productOptionsCompairsList.Add(pc);
            //        pc.SubeList = new List<UrunEdit2>();
            //    }
            //    var urunEdit2 = new UrunEdit2()
            //    {
            //        PkId = pkId,
            //        SubeId = subeId,
            //        SubePriceValue = optionsPrice
            //    };
            //    if (!string.IsNullOrEmpty(optionsName))
            //    {
            //        pc.SubeList.Add(urunEdit2);
            //    }
            //}
            //foreach (var sube in returnViewModel.SubeList)
            //{
            //    foreach (var product in returnViewModel.productOptionsCompairsList)
            //    {
            //        if (!product.SubeList.Where(x => x.SubeId == sube.SubeId).Any())
            //        {
            //            product.SubeList.Add(new UrunEdit2 { SubeId = sube.SubeId });
            //        }
            //    }
            //}

            returnViewModel.ProductOptionsList = dataSubeUrunOptionslist.AsEnumerable().ToList()
                .Select(item => new
                {
                    ProductGroup = mf.RTS(item, "ProductGroup"),
                    ProductName = mf.RTS(item, "ProductName"),
                    OptionsProductName = mf.RTS(item, "OptionsProductName"),
                    GuncellenecekSubeIdGrubu = mf.RTS(item, "GuncellenecekSubeIdGrubu"),
                })
                .Distinct()
                .Select(item => new UrunEditItemViewModel
                {
                    //Id= mf.RTI(item, "Id"),
                    ProductGroup = item.ProductGroup,
                    ProductName = item.ProductName,
                    OptionsProductName = item.OptionsProductName,
                    UrunEditPriceList = new List<UrunEditPrice>(),
                })
                .Where(x => !string.IsNullOrEmpty(x.OptionsProductName))
                .OrderBy(x => x.ProductGroup)
                .ToList();

            var dbProDuctOptionsItemList = dataSubeUrunOptionslist.AsEnumerable().ToList()
                  .Select(item => new
                  {
                      Id = mf.RTI(item, "Id"),
                      SubeId = mf.RTI(item, "SubeId"),
                      SubeName = mf.RTS(item, "SubeName"),
                      OptionsVarMi = mf.RTI(item, "OptionsVarMi"),
                      Choice1VarMi = mf.RTI(item, "Choice1VarMi"),
                      ProductGroup = mf.RTS(item, "ProductGroup"),
                      ProductName = mf.RTS(item, "ProductName"),
                      IsManuelInsert = mf.RTS(item, "IsManuelInsert"),
                      ProductId = mf.RTI(item, "ProductId"),
                      ProductTemplatePricePkId = mf.RTI(item, "ProductTemplatePricePkId"),
                      OptionsId = mf.RTI(item, "OptionsId"),
                      optionprice = mf.RTD(item, "optionprice"),
                      OptionsProductName = mf.RTS(item, "OptionsProductName"),
                  }).ToList()
                  .Where(x => !string.IsNullOrEmpty(x.OptionsProductName))
                  .OrderBy(x => x.ProductGroup)
                  .ToList();

            var productOptionsList = returnViewModel.ProductOptionsList.Where(x => !string.IsNullOrEmpty(x.OptionsProductName)).OrderBy(x => x.ProductGroup).ToList();
            foreach (var productItem in productOptionsList)
            {
                foreach (var sube in returnViewModel.SubeList)
                {
                    var priceItem = dbProDuctOptionsItemList
                        .Where(x => x.SubeId == sube.SubeId)
                        .Where(x => x.ProductGroup == productItem.ProductGroup)
                        .Where(x => x.ProductName == productItem.ProductName)
                        .FirstOrDefault();

                    if (priceItem == null)
                    {
                        productItem.UrunEditPriceList.Add(new UrunEditPrice { SubeId = sube.SubeId, Price = null, OptionsPrice = null });
                    }
                    else
                        productItem.UrunEditPriceList.Add(new UrunEditPrice
                        {
                            Id = priceItem.Id,
                            SubeId = sube.SubeId,
                            Choice1VarMi = priceItem.Choice1VarMi,
                            OptionsVarMi = priceItem.OptionsVarMi,
                            ProductId = priceItem.ProductId,
                            OptionsId = priceItem.OptionsId,
                            OptionsPrice = priceItem.optionprice,
                            IsManuelInsert = priceItem.IsManuelInsert == string.Empty ? false : Convert.ToBoolean(priceItem.IsManuelInsert),
                        });
                }
            }


            #endregion Options listesi

            return returnViewModel;
        }

        #endregion  Şubelerdeki ortak options(Options table) update yapma

        public ActionResultMessages IsUpdateProductTemplatePrice(SablonSubeViewModel model, string subeIdGrupList, string sablonName, string tumSubelereYay, string kullaniciId)
        {
            var result = new ActionResultMessages() { IsSuccess = true, UserMessage = "İşlem Başarılı", };
            var updatesubeName = string.Empty;

            try
            {
                //Kullanıcı şube yetkisi
                var kullaniciSubeYetkisi = UsersListCRUD.KullaniciSubeYetkiListesi(kullaniciId);

                if (tumSubelereYay == "TumSubelereYay")
                {
                    var spiltSubeId = subeIdGrupList.TrimEnd(',').Split(',').FirstOrDefault();

                    if (spiltSubeId.Count() == 1)
                    {

                    }

                    var subeId = model.HedefSubeId;


                    #region Local pruduct tablosundaki şubeler alınır 

                    //var updatedSProductSubeList = mf.GetSubeDataWithQuery(mf.NewConnectionString(subeIp, dbName, sqlKullaniciName, sqlKullaniciPassword), SqlData.getSablonProductJoinSubesettingsSqlQuery(subeIdGrupList));
                    //var updatedSProductSubeList = SqlData.GetIsActivAllSube();
                    var updatedSProductSubeList = model.IsSelectedSubeList.Where(x => x.IsSelectedHedefSube == true).ToList();
                    foreach (var itemSube in updatedSProductSubeList)
                    {
                        var updateSubeId = itemSube.ID; //mf.RTS(itemSube, "SubeId");

                        if (kullaniciSubeYetkisi != null && kullaniciSubeYetkisi.FR_SubeListesi.Where(x => x.SubeID == Convert.ToInt32(updateSubeId)).Select(x => x.SubeID).Any())
                        {
                            var getSube = SqlData.GetSube(itemSube.ID);

                            var updateSubeIp = getSube.SubeIP; //mf.RTS(itemSube, "SubeIp");
                            var updateDbName = getSube.DBName; //mf.RTS(itemSube, "DBName");
                            updatesubeName = getSube.SubeName; //mf.RTS(itemSube, "SubeName");
                            var updateSqlKullaniciName = getSube.SqlName;//mf.RTS(itemSube, "SqlName");
                            var updateSqlKullaniciPassword = getSube.SqlPassword; //mf.RTS(itemSube, "SqlPassword");


                            var updatedProduct = mf.GetSubeDataWithQuery(mf.NewConnectionString(updateSubeIp, updateDbName, updateSqlKullaniciName, updateSqlKullaniciPassword), SqlData.updatedSablonProductSqlQuery(dbName, updateDbName, sablonName, subeId.ToString()));
                            var updatedChoice1 = mf.GetSubeDataWithQuery(mf.NewConnectionString(updateSubeIp, updateDbName, updateSqlKullaniciName, updateSqlKullaniciPassword), SqlData.updatedSablonChoice1SqlQuery(dbName, updateDbName, sablonName, subeId.ToString()));
                            var updatedChoice2 = mf.GetSubeDataWithQuery(mf.NewConnectionString(updateSubeIp, updateDbName, updateSqlKullaniciName, updateSqlKullaniciPassword), SqlData.updatedSablonChoice2SqlQuery(dbName, updateDbName, sablonName, subeId.ToString()));
                            var updatedOptions = mf.GetSubeDataWithQuery(mf.NewConnectionString(updateSubeIp, updateDbName, updateSqlKullaniciName, updateSqlKullaniciPassword), SqlData.updatedSablonOptionsSqlQuery(dbName, updateDbName, sablonName, subeId.ToString()));

                            #region Sablon Product ekleme
                            var sqlDataSube = new SqlData(new SqlConnection(mf.NewConnectionString(updateSubeIp, updateDbName, updateSqlKullaniciName, updateSqlKullaniciPassword)));
                            var insertProductTemplatePriceList = mf.GetSubeDataWithQuery(mf.NewConnectionString(subeIp, dbName, sqlKullaniciName, sqlKullaniciPassword), SqlData.getForInsertSablonProductTemplatePriceSqlQuery(subeId.ToString(), sablonName));
                            foreach (DataRow item in insertProductTemplatePriceList.Rows)
                            {
                                var price = mf.RTD(item, "Price");
                                //Tutar sıfırdan farlı ise yeni şablon eklendi anlamına geliyor.
                                if (price != 0)
                                {
                                    //ProductTemplatePrice İnsert
                                    sqlDataSube.ExecuteSql("Insert Into  ProductTemplatePrice( TemplateId,ProductId,Choice1Id,Choice2Id,OptionsId,Price ) Values (@par1, @par2, @par3, @par4, @par5, @par6)",
                                        new object[]
                                        {
                                           mf.RTI(item, "TemplateId"),
                                           mf.RTI(item, "ProductId"),
                                           mf.RTI(item, "Choice1Id"),
                                           mf.RTI(item, "Choice2Id"),
                                           mf.RTI(item, "OptionsId"),
                                       price
                                        });
                                }
                            }

                            #endregion
                        }
                    }

                    #endregion Local pruduct tablosundaki şubeler alınır 

                    #region Product tablosunda Delete  yapar
                    //var sqlData = new SqlData(new SqlConnection(mf.NewConnectionString(subeIp, dbName, sqlKullaniciName, sqlKullaniciPassword)));
                    //sqlData.ExecuteSql("Delete SablonProduct where GuncellenecekSubeIdGrubu='" + subeIdGrupList + "' and TemplateName='" + sablonName + "' ", new object[] { });
                    //sqlData.ExecuteSql("Delete SablonChoice1 where GuncellenecekSubeIdGrubu='" + subeIdGrupList + "' and TemplateName='" + sablonName + "' ", new object[] { });
                    //sqlData.ExecuteSql("Delete SablonChoice2 where GuncellenecekSubeIdGrubu='" + subeIdGrupList + "' and TemplateName='" + sablonName + "' ", new object[] { });
                    //sqlData.ExecuteSql("Delete SablonOptions where GuncellenecekSubeIdGrubu='" + subeIdGrupList + "' and TemplateName='" + sablonName + "' ", new object[] { });
                    //sqlData.ExecuteSql("Delete ProductTemplatePrice where GuncellenecekSubeIdGrubu='" + subeIdGrupList + "' and TemplateName='" + sablonName + "' ", new object[] { });

                    ////var splitSubeIdList = subeIdGrupList.Split(',');
                    ////foreach (var subeId in splitSubeIdList)
                    ////{
                    ////    if (!string.IsNullOrEmpty(subeId))
                    ////    {
                    ////        sqlData.executeSql("Delete ProductTemplate where SubeId='" + subeId + "'  and Name='" + sablonName + "' ", new object[] { });
                    ////    }
                    ////}
                    #endregion Product tablosunda Delete yapar

                }
                else
                {
                    #region Local pruduct tablosundaki şubeler alınır 

                    var updatedSProductSubeList = mf.GetSubeDataWithQuery(mf.NewConnectionString(subeIp, dbName, sqlKullaniciName, sqlKullaniciPassword), SqlData.getSablonProductJoinSubesettingsSqlQuery(subeIdGrupList));
                    foreach (DataRow itemSube in updatedSProductSubeList.Rows)
                    {
                        var updateSubeId = mf.RTS(itemSube, "SubeId");

                        if (kullaniciSubeYetkisi != null && kullaniciSubeYetkisi.FR_SubeListesi.Where(x => x.SubeID == Convert.ToInt32(updateSubeId)).Select(x => x.SubeID).Any())
                        {
                            var updateSubeIp = mf.RTS(itemSube, "SubeIp");
                            var updateDbName = mf.RTS(itemSube, "DBName");
                            updatesubeName = mf.RTS(itemSube, "SubeName");
                            var updateSqlKullaniciName = mf.RTS(itemSube, "SqlName");
                            var updateSqlKullaniciPassword = mf.RTS(itemSube, "SqlPassword");
                            var updatedProduct = mf.GetSubeDataWithQuery(mf.NewConnectionString(updateSubeIp, updateDbName, updateSqlKullaniciName, updateSqlKullaniciPassword), SqlData.updatedSablonProductSqlQuery(dbName, updateDbName, sablonName, updateSubeId));
                            var updatedChoice1 = mf.GetSubeDataWithQuery(mf.NewConnectionString(updateSubeIp, updateDbName, updateSqlKullaniciName, updateSqlKullaniciPassword), SqlData.updatedSablonChoice1SqlQuery(dbName, updateDbName, sablonName, updateSubeId));
                            var updatedChoice2 = mf.GetSubeDataWithQuery(mf.NewConnectionString(updateSubeIp, updateDbName, updateSqlKullaniciName, updateSqlKullaniciPassword), SqlData.updatedSablonChoice2SqlQuery(dbName, updateDbName, sablonName, updateSubeId));
                            var updatedOptions = mf.GetSubeDataWithQuery(mf.NewConnectionString(updateSubeIp, updateDbName, updateSqlKullaniciName, updateSqlKullaniciPassword), SqlData.updatedSablonOptionsSqlQuery(dbName, updateDbName, sablonName, updateSubeId));

                            #region Sablon Product ekleme
                            var sqlDataSube = new SqlData(new SqlConnection(mf.NewConnectionString(updateSubeIp, updateDbName, updateSqlKullaniciName, updateSqlKullaniciPassword)));
                            var insertProductTemplatePriceList = mf.GetSubeDataWithQuery(mf.NewConnectionString(subeIp, dbName, sqlKullaniciName, sqlKullaniciPassword), SqlData.getForInsertSablonProductTemplatePriceSqlQuery(updateSubeId, sablonName));
                            foreach (DataRow item in insertProductTemplatePriceList.Rows)
                            {
                                var price = mf.RTD(item, "Price");
                                //Tutar sıfırdan farlı ise yeni şablon eklendi anlamına geliyor.
                                if (price != 0)
                                {
                                    //ProductTemplatePrice İnsert
                                    sqlDataSube.ExecuteSql("Insert Into  ProductTemplatePrice( TemplateId,ProductId,Choice1Id,Choice2Id,OptionsId,Price ) Values (@par1, @par2, @par3, @par4, @par5, @par6)",
                                        new object[]
                                        {
                                       mf.RTI(item, "TemplateId"),
                                       mf.RTI(item, "ProductId"),
                                       mf.RTI(item, "Choice1Id"),
                                       mf.RTI(item, "Choice2Id"),
                                       mf.RTI(item, "OptionsId"),
                                       price
                                        });
                                }
                            }

                            #endregion
                        }
                    }

                    #endregion Local pruduct tablosundaki şubeler alınır 

                    #region Product tablosunda Delete  yapar
                    var sqlData = new SqlData(new SqlConnection(mf.NewConnectionString(subeIp, dbName, sqlKullaniciName, sqlKullaniciPassword)));
                    sqlData.ExecuteSql("Delete SablonProduct where GuncellenecekSubeIdGrubu='" + subeIdGrupList + "' and TemplateName='" + sablonName + "' ", new object[] { });
                    sqlData.ExecuteSql("Delete SablonChoice1 where GuncellenecekSubeIdGrubu='" + subeIdGrupList + "' and TemplateName='" + sablonName + "' ", new object[] { });
                    sqlData.ExecuteSql("Delete SablonChoice2 where GuncellenecekSubeIdGrubu='" + subeIdGrupList + "' and TemplateName='" + sablonName + "' ", new object[] { });
                    sqlData.ExecuteSql("Delete SablonOptions where GuncellenecekSubeIdGrubu='" + subeIdGrupList + "' and TemplateName='" + sablonName + "' ", new object[] { });
                    sqlData.ExecuteSql("Delete ProductTemplatePrice where GuncellenecekSubeIdGrubu='" + subeIdGrupList + "' and TemplateName='" + sablonName + "' ", new object[] { });

                    //var splitSubeIdList = subeIdGrupList.Split(',');
                    //foreach (var subeId in splitSubeIdList)
                    //{
                    //    if (!string.IsNullOrEmpty(subeId))
                    //    {
                    //        sqlData.executeSql("Delete ProductTemplate where SubeId='" + subeId + "'  and Name='" + sablonName + "' ", new object[] { });
                    //    }
                    //}
                    #endregion Product tablosunda Delete yapar
                }



            }
            catch (Exception ex)
            {
                Singleton.WritingLogFile2("SPosSubeSablonFiyatGuncellemeCRUD_IsUpdateProductTemplatePrice", ex.ToString(), null, ex.StackTrace);
                result.IsSuccess = false;
                result.UserMessage = updatesubeName + " şubesine bağlantı kurulamadı. Lütfen şubenin aktif olduğundan emin olunuz.";
                return result;
            }

            return result;
        }


        public ActionResultMessages DeleteProductTemplatePrice(string subeIdGrupList, string sablonName)
        {
            var result = new ActionResultMessages() { IsSuccess = true, UserMessage = "İşlem Başarılı", };

            try
            {
                #region Product tablosunda Delete  yapar
                var sqlData = new SqlData(new SqlConnection(mf.NewConnectionString(subeIp, dbName, sqlKullaniciName, sqlKullaniciPassword)));
                sqlData.ExecuteSql("Delete SablonProduct where GuncellenecekSubeIdGrubu='" + subeIdGrupList + "' and TemplateName='" + sablonName + "' ", new object[] { });
                sqlData.ExecuteSql("Delete SablonChoice1 where GuncellenecekSubeIdGrubu='" + subeIdGrupList + "' and TemplateName='" + sablonName + "' ", new object[] { });
                sqlData.ExecuteSql("Delete SablonChoice2 where GuncellenecekSubeIdGrubu='" + subeIdGrupList + "' and TemplateName='" + sablonName + "' ", new object[] { });
                sqlData.ExecuteSql("Delete SablonOptions where GuncellenecekSubeIdGrubu='" + subeIdGrupList + "' and TemplateName='" + sablonName + "' ", new object[] { });
                sqlData.ExecuteSql("Delete ProductTemplatePrice where GuncellenecekSubeIdGrubu='" + subeIdGrupList + "' and TemplateName='" + sablonName + "' ", new object[] { });
                #endregion Product tablosunda Delete yapar
            }
            catch (Exception ex)
            {
                Singleton.WritingLogFile2("DeleteProductTemplatePrice", ex.ToString(), null, ex.StackTrace);
                result.IsSuccess = false;
                result.UserMessage = "İşlem Başarısız.";
            }

            return result;
        }

        #endregion (05.02.2022 toplantı sonrası) Seçili şubelerdeki pruduct, choice1,choice2,options tablolarınını merkez (local temp db'ye aktarma

    }
}