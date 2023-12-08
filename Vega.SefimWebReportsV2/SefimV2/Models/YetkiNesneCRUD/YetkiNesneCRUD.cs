using SefimV2.ViewModels.YetkiNesneViewModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.Data.SqlClient;
using System.Linq;
using System.Web.Configuration;
using static SefimV2.ViewModels.YetkiNesneViewModel.YetkiNesneDetailVewModel;
using static SefimV2.ViewModels.YetkiNesneViewModel.YetkiNesneViewModel;
using static SefimV2.ViewModels.YetkiNesneViewModel.YetkiUserVewModel;

namespace SefimV2.Models
{
    public class YetkiNesneCRUD
    {
        ModelFunctions mF = new ModelFunctions();
        #region Config local copy db connction setting       
        static string subeIp = WebConfigurationManager.AppSettings["Server"];
        static string dbName = WebConfigurationManager.AppSettings["DBName"];
        static string sqlKullaniciName = WebConfigurationManager.AppSettings["User"];
        static string sqlKullaniciPassword = WebConfigurationManager.AppSettings["Password"];
        #endregion

        public List<YetkiNesneViewModel> GetList()
        {
            var result = new ActionResultMessages() { IsSuccess = true, UserMessage = "İşlem Başarılı" };
            var getYekiMenu = mF.GetSubeDataWithQuery(mF.NewConnectionString(subeIp, dbName, sqlKullaniciName, sqlKullaniciPassword), "Select * From [dbo].[YetkiNesne] Where IsActive=1");
            var model = new List<YetkiNesneViewModel>();

            try
            {
                //YetkiNesne
                foreach (DataRow yetkiMenu in getYekiMenu.Rows)
                {
                    var modelYetki = new YetkiNesneViewModel();
                    modelYetki.Id = Convert.ToInt32(mF.RTS(yetkiMenu, "Id"));
                    modelYetki.YetkiAdi = mF.RTS(yetkiMenu, "YetkiAdi");
                    modelYetki.Aciklama = mF.RTS(yetkiMenu, "Aciklama");
                    model.Add(modelYetki);
                }
            }
            catch (OleDbException ex) { result.IsSuccess = false; result.UserMessage = ex.ToString(); return model; }

            return model;
        }
        public YetkiNesneViewModel GetForCreate()
        {
            var result = new ActionResultMessages() { IsSuccess = true, UserMessage = "İşlem Başarılı" };
            var getYekiMenu = mF.GetSubeDataWithQuery(mF.NewConnectionString(subeIp, dbName, sqlKullaniciName, sqlKullaniciPassword), "Select * From [dbo].[YetkiMenu] Where IsActive=1");
            var getYekiIslemTip = mF.GetSubeDataWithQuery(mF.NewConnectionString(subeIp, dbName, sqlKullaniciName, sqlKullaniciPassword), "Select * From [dbo].[YetkiIslemTip] where IsActive=1");
            var model = new YetkiNesneViewModel
            {
                YetkiMenuList = new List<YetkiMenu>()
            };
            //model.YetkiMenuList = new List<YetkiIslemTip>();

            try
            {
                //YetkiMenu
                foreach (DataRow yetkiMenu in getYekiMenu.Rows)
                {
                    var modelMenu = new YetkiMenu
                    {
                        Id = Convert.ToInt32(mF.RTS(yetkiMenu, "Id")),
                        UstMenuId = mF.RTI(yetkiMenu, "UstMenuId"),
                        Adi = mF.RTS(yetkiMenu, "Adi"),
                        YetkiDegeri = mF.RTS(yetkiMenu, "YetkiDegeri")
                    };
                    model.YetkiMenuList.Add(modelMenu);

                    modelMenu.YetkiIslemTipList = new List<YetkiMenu.YetkiIslemTip>();
                    //YetkiIslemTip
                    foreach (DataRow islemTipi in getYekiIslemTip.Rows)
                    {
                        //var modelIslemTipi = new YetkiIslemTip();
                        var modelIslemTipiMenu = new YetkiMenu.YetkiIslemTip
                        {
                            Id = Convert.ToInt32(mF.RTS(islemTipi, "Id")),
                            IslemTipiAdi = mF.RTS(islemTipi, "IslemTipiAdi"),
                            IslemTipi = mF.RTI(islemTipi, "IslemTipi")
                        };
                        modelMenu.YetkiIslemTipList.Add(modelIslemTipiMenu);
                    }
                }
            }
            catch (OleDbException ex) { result.IsSuccess = false; result.UserMessage = ex.ToString(); return model; }

            return model;
        }
        public ActionResultMessages Create(YetkiNesneViewModel model)
        {
            var result = new ActionResultMessages() { IsSuccess = true, UserMessage = "İşlem Başarılı" };

            try
            {
                var getYekiNesneVarMi = mF.GetSubeDataWithQuery(mF.NewConnectionString(subeIp, dbName, sqlKullaniciName, sqlKullaniciPassword), "Select * From [dbo].[YetkiNesne] where IsActive=1 and YetkiAdi='" + model.YetkiAdi + "' ");
                if (getYekiNesneVarMi.AsEnumerable().ToList().Count() > 0)
                {
                    result.IsSuccess = false;
                    result.UserMessage = "Yetki Adı daha önceden oluşturulmuştur.";
                    return result;
                }

                var sqlData = new SqlData(new SqlConnection(mF.NewConnectionString(subeIp, dbName, sqlKullaniciName, sqlKullaniciPassword)));
                sqlData.ExecuteSql("INSERT INTO [dbo].[YetkiNesne] ( [YetkiAdi] ,[Aciklama] ,[CreatedDate] ,[UpdatedDate] ,[IsActive] ) VALUES ( @par1, @par2, @par3, @par4, @par5)",
                    new object[]
                    {
                        model.YetkiAdi,
                        model.Aciklama,
                        DateTime.Now.Date,
                        null,
                        true
                    });

                var getYekiNesne = mF.GetSubeDataWithQuery(mF.NewConnectionString(subeIp, dbName, sqlKullaniciName, sqlKullaniciPassword), "Select * From [dbo].[YetkiNesne] where IsActive=1 and YetkiAdi='" + model.YetkiAdi + "' ");
                foreach (DataRow yetkiNesne in getYekiNesne.Rows)
                {
                    model.Id = mF.RTI(yetkiNesne, "Id");
                }

                model.YetkiMenuList = model.YetkiMenuList.Where(x => x.CheckedYetkiMenu == true).ToList();
                foreach (var yetkiMenu in model.YetkiMenuList)
                {
                    yetkiMenu.YetkiIslemTipList.Where(x => x.IslemTipi == 1).FirstOrDefault().CheckedYetkiIslemTip = true;
                    yetkiMenu.YetkiIslemTipList = yetkiMenu.YetkiIslemTipList.Where(x => x.CheckedYetkiIslemTip).ToList();
                    foreach (var yetkiIslemTip in yetkiMenu.YetkiIslemTipList)
                    {
                        sqlData.ExecuteSql("INSERT INTO [dbo].[YetkiNesneDetay]( [YetkiNesneId] ,[MenuId] ,[IslemTipiId] ) VALUES ( @par1, @par2, @par3 )",
                          new object[]
                          {
                               model.Id,
                               yetkiMenu.Id,
                               yetkiIslemTip.Id
                            });
                    }
                }
            }
            catch (OleDbException ex) { result.IsSuccess = false; result.UserMessage = ex.ToString(); return result; }

            return result;
        }
        public YetkiNesneViewModel GetById(int id)
        {
            var result = new ActionResultMessages() { IsSuccess = true, UserMessage = "İşlem Başarılı" };
            var getYekiMenu = mF.GetSubeDataWithQuery(mF.NewConnectionString(subeIp, dbName, sqlKullaniciName, sqlKullaniciPassword), "Select * From [dbo].[YetkiMenu] Where IsActive=1");
            var getYekiIslemTip = mF.GetSubeDataWithQuery(mF.NewConnectionString(subeIp, dbName, sqlKullaniciName, sqlKullaniciPassword), "Select * From [dbo].[YetkiIslemTip] where IsActive=1");
            var getYekiNesne = mF.GetSubeDataWithQuery(mF.NewConnectionString(subeIp, dbName, sqlKullaniciName, sqlKullaniciPassword), "Select * From [dbo].[YetkiNesne] where IsActive=1 and Id=" + id);
            var getYekiNesneDetay = mF.GetSubeDataWithQuery(mF.NewConnectionString(subeIp, dbName, sqlKullaniciName, sqlKullaniciPassword), "Select * From [dbo].[YetkiNesneDetay] where YetkiNesneId=" + id);
            var model = new YetkiNesneViewModel();
            model.YetkiMenuList = new List<YetkiMenu>();
            //model.YetkiMenuList = new List<YetkiIslemTip>();

            try
            {
                //YetkiNesneDetay
                var yetkiDetayModel = new YetkiNesneDetailVewModel();
                yetkiDetayModel.YetkiNesneDetailList = new List<YetkiNesneDetail>();
                foreach (DataRow yetkiMenu in getYekiNesneDetay.Rows)
                {
                    var modelYekiNesneDetail = new YetkiNesneDetail();
                    modelYekiNesneDetail.Id = mF.RTI(yetkiMenu, "Id");
                    modelYekiNesneDetail.YetkiNesneId = mF.RTI(yetkiMenu, "YetkiNesneId");
                    modelYekiNesneDetail.MenuId = mF.RTI(yetkiMenu, "MenuId");
                    modelYekiNesneDetail.IslemTipiId = mF.RTI(yetkiMenu, "IslemTipiId");
                    yetkiDetayModel.YetkiNesneDetailList.Add(modelYekiNesneDetail);
                }

                //YetkiNesne                
                foreach (DataRow yetkiMenu in getYekiNesne.Rows)
                {
                    model.Id = id;
                    model.YetkiAdi = mF.RTS(yetkiMenu, "YetkiAdi");
                    model.Aciklama = mF.RTS(yetkiMenu, "Aciklama");
                }

                //YetkiMenu
                foreach (DataRow yetkiMenu in getYekiMenu.Rows)
                {
                    var modelMenu = new YetkiMenu();
                    modelMenu.Id = Convert.ToInt32(mF.RTS(yetkiMenu, "Id"));
                    modelMenu.UstMenuId = mF.RTI(yetkiMenu, "UstMenuId");
                    modelMenu.Adi = mF.RTS(yetkiMenu, "Adi");
                    modelMenu.YetkiDegeri = mF.RTS(yetkiMenu, "YetkiDegeri");

                    if (yetkiDetayModel.YetkiNesneDetailList.Count() > 0)
                    {
                        if (yetkiDetayModel.YetkiNesneDetailList.Where(x => x.MenuId == modelMenu.Id).Any())
                        {
                            modelMenu.CheckedYetkiMenu = true;
                            modelMenu.YetkiNesneDetayId = yetkiDetayModel.YetkiNesneDetailList.Where(x => x.MenuId == modelMenu.Id).FirstOrDefault().Id;
                        }
                    }
                    model.YetkiMenuList.Add(modelMenu);

                    //YetkiIslemTip
                    modelMenu.YetkiIslemTipList = new List<YetkiMenu.YetkiIslemTip>();
                    foreach (DataRow islemTipi in getYekiIslemTip.Rows)
                    {
                        //var modelIslemTipi = new YetkiIslemTip();
                        var modelIslemTipiMenu = new YetkiMenu.YetkiIslemTip();
                        modelIslemTipiMenu.Id = Convert.ToInt32(mF.RTS(islemTipi, "Id"));
                        modelIslemTipiMenu.IslemTipiAdi = mF.RTS(islemTipi, "IslemTipiAdi");
                        modelIslemTipiMenu.IslemTipi = mF.RTI(islemTipi, "IslemTipi");

                        if (yetkiDetayModel.YetkiNesneDetailList.Count() > 0)
                        {
                            if (yetkiDetayModel.YetkiNesneDetailList.Where(x => x.IslemTipiId == modelIslemTipiMenu.IslemTipi).Where(x => x.MenuId == modelMenu.Id).Any())
                            {
                                modelIslemTipiMenu.CheckedYetkiIslemTip = true;
                            }
                        }

                        modelMenu.YetkiIslemTipList.Add(modelIslemTipiMenu);
                    }
                }
            }
            catch (OleDbException ex) { result.IsSuccess = false; result.UserMessage = ex.ToString(); return model; }

            return model;
        }
        public ActionResultMessages Update(YetkiNesneViewModel model)
        {
            var result = new ActionResultMessages() { IsSuccess = true, UserMessage = "İşlem Başarılı" };

            var getYekiNesneVarMi = mF.GetSubeDataWithQuery(mF.NewConnectionString(subeIp, dbName, sqlKullaniciName, sqlKullaniciPassword), "Select * From [dbo].[YetkiNesne] where IsActive=1 and Id='" + model.Id + "' ").AsEnumerable().FirstOrDefault();
            var updatedYetkiAdi = getYekiNesneVarMi.ItemArray[1].ToString();


            var getYekiNesneList = mF.GetSubeDataWithQuery(mF.NewConnectionString(subeIp, dbName, sqlKullaniciName, sqlKullaniciPassword), "Select * From [dbo].[YetkiNesne] where IsActive=1").AsEnumerable().ToList();
            if (getYekiNesneList.Count() > 0)
            {
                foreach (var yetkiAdi in getYekiNesneList)
                {
                    var yetkiNesneAdi = mF.RTS(yetkiAdi, "YetkiAdi");
                    if (yetkiNesneAdi != updatedYetkiAdi && model.YetkiAdi == yetkiNesneAdi)
                    {
                        result.IsSuccess = false;
                        result.UserMessage = "Yetki Adı daha önceden oluşturulmuştur.";
                        return result;
                    }
                }               
            }

            var sqlData = new SqlData(new SqlConnection(mF.NewConnectionString(subeIp, dbName, sqlKullaniciName, sqlKullaniciPassword)));
            try
            {
                sqlData.ExecuteSql("UPDATE [dbo].[YetkiNesne] SET [YetkiAdi] = @par1, [Aciklama] = @par2, [CreatedDate] =@par3, [UpdatedDate] = @par4, [IsActive] =@par5 WHERE Id=@par6",
                    new object[]
                         {
                            model.YetkiAdi,
                            model.Aciklama,
                            null,
                            DateTime.Now.Date,
                            1,
                            model.Id
                        });

                model.YetkiMenuList = model.YetkiMenuList.Where(x => x.CheckedYetkiMenu == true).ToList();
                //Delete YetkiNesneDetay
                sqlData.ExecuteSql("Delete From [dbo].[YetkiNesneDetay]  where YetkiNesneId=@par1", new object[] { model.Id, });
                foreach (var yetkiMenu in model.YetkiMenuList)
                {
                    yetkiMenu.YetkiIslemTipList.Where(x => x.IslemTipi == 1).FirstOrDefault().CheckedYetkiIslemTip = true;
                    yetkiMenu.YetkiIslemTipList = yetkiMenu.YetkiIslemTipList.Where(x => x.CheckedYetkiIslemTip).ToList();
                    foreach (var yetkiIslemTip in yetkiMenu.YetkiIslemTipList)
                    {
                        sqlData.ExecuteSql("INSERT INTO [dbo].[YetkiNesneDetay]( [YetkiNesneId] ,[MenuId] ,[IslemTipiId] ) VALUES ( @par1, @par2, @par3 )",
                          new object[]
                          {
                               model.Id,
                               yetkiMenu.Id,
                               yetkiIslemTip.IslemTipi
                            });
                    }
                }


                #region YetkiUser tablosuna kayıt yapar

                var getYekiNesneDetay = mF.GetSubeDataWithQuery(mF.NewConnectionString(subeIp, dbName, sqlKullaniciName, sqlKullaniciPassword), "Select * From [dbo].[YetkiNesneDetay] where YetkiNesneId=" + model.Id);
                var getYekiUser = mF.GetSubeDataWithQuery(mF.NewConnectionString(subeIp, dbName, sqlKullaniciName, sqlKullaniciPassword), "Select * From [dbo].[YetkiUser] where YetkiNesneId=" + model.Id).AsEnumerable().ToList();

                var yetkiUser = new YetkiUserVewModel();
                yetkiUser.YetkiUserDetailModelList = new List<YetkiUserDetailModel>();
                foreach (var user in getYekiUser)
                {
                    var modelUser = new YetkiUserDetailModel();
                    modelUser.UserId = mF.RTI(user, "UserId");
                    //modelUser.YetkiNesneId = mF.RTI(user, "YetkiNesneId");
                    yetkiUser.YetkiUserDetailModelList.Add(modelUser);
                }

                var userList = yetkiUser.YetkiUserDetailModelList.ToList().Select(x => x.UserId).Distinct().ToList();

                foreach (var user in userList)
                {
                    mF.SqlConnOpen();
                    var yetkiUserDelete = mF.DataTable("Delete From YetkiUser where UserId= " + user + "");
                    mF.SqlConnClose();

                    foreach (DataRow yetkiMenu in getYekiNesneDetay.Rows)
                    {
                        var Id = mF.RTI(yetkiMenu, "Id");
                        var MenuId = mF.RTI(yetkiMenu, "MenuId");
                        var IslemTipi = mF.RTI(yetkiMenu, "IslemTipi");
                        var YetkiNesneId = mF.RTI(yetkiMenu, "YetkiNesneId");

                        sqlData.ExecuteSql("INSERT INTO [dbo].[YetkiUser] ( [MenuId],[IslemTipiId],[UserId],[YetkiNesneId],[IsActive] ) VALUES ( @par1, @par2, @par3, @par4,@par5 )",
                            new object[]
                            {
                                MenuId,
                                IslemTipi,
                                user,
                                model.Id,
                                1
                            });
                    }
                }

                #endregion YetkiUser tablosuna kayıt yapar

            }
            catch (OleDbException ex) { result.IsSuccess = false; result.UserMessage = ex.ToString(); return result; }

            return result;
        }
    }
}