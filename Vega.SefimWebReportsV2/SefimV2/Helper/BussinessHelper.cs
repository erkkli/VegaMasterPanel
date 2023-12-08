using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using SefimV2.Models;
using SefimV2.ViewModels.SefimPanelHome;
using SefimV2.ViewModels.User;
using SefimV2.ViewModels.YetkiNesneViewModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Configuration;
using System.Web.Mvc;
using static SefimV2.ViewModels.YetkiNesneViewModel.YetkiUserVewModel;

namespace SefimV2.Helper
{
    public class BussinessHelper
    {
        #region Config local copy db connction setting  
        static readonly string subeIp = WebConfigurationManager.AppSettings["Server"];
        static readonly string dbName = WebConfigurationManager.AppSettings["DBName"];
        static readonly string sqlKullaniciName = WebConfigurationManager.AppSettings["User"];
        static readonly string sqlKullaniciPassword = WebConfigurationManager.AppSettings["Password"];
        static readonly string BRANCH_ACCESS_URL = WebConfigurationManager.AppSettings["BRANCH_ACCESS_URL"];
        #endregion

        #region SefimPanelUrunEkle ComboBox listeleri
        public static List<SelectListItem> ProductGroups { get; set; }
        public static List<SelectListItem> InvoiceNames { get; set; }
        public static List<SelectListItem> ProductTypes { get; set; }
        public static List<SelectListItem> Favoritess { get; set; }
        #endregion

        internal static AdminViewModel GetByUserInfoForId(string userName = null, string password = null, string kullaniciId = null)
        {
            var model = new AdminViewModel();
            var mf = new ModelFunctions();
            string filter = " where 1=1 ";
            if (!string.IsNullOrEmpty(userName))
                filter += " and UserName='" + userName + "'";
            if (!string.IsNullOrEmpty(password))
                filter += " and Password='" + password + "'";
            if (!string.IsNullOrEmpty(kullaniciId))
                filter += " and Id=" + kullaniciId + " ";

            try
            {
                mf.SqlConnOpen();
                DataTable dt = mf.DataTable("Select * From Users" + filter);
                mf.SqlConnClose();

                foreach (DataRow r in dt.Rows)
                {
                    model.UserName = mf.RTS(r, "UserName");
                    model.Password = mf.RTS(r, "Password");
                    model.ID = Convert.ToInt32(mf.RTS(r, "Id"));
                    model.SubeSirasiGorunsunMu = mf.RTS(r, "SubeSirasiGorunsunMu") == string.Empty ? "false" : mf.RTS(r, "SubeSirasiGorunsunMu").ToLower();
                    var uygulamaTipi = mf.RTS(r, "UygulmaTipi");
                    model.UygulamaTipi = uygulamaTipi;
                }
            }
            catch (Exception ex)
            {
                Singleton.WritingLogFile("GetByUserInfoForId", ex.Message.ToString());
            }

            return model;
        }
        internal static UserViewModel GetByUserGroupsInfoForId(string userName = null, string password = null, string kullaniciId = null)
        {
            var model = new UserViewModel();
            var mf = new ModelFunctions();
            string filter = " where 1=1 ";
            if (!string.IsNullOrEmpty(userName))
                filter += " and UserName='" + userName + "'";
            if (!string.IsNullOrEmpty(password))
                filter += " and Password='" + password + "'";
            if (!string.IsNullOrEmpty(kullaniciId))
                filter += " and Id=" + kullaniciId + " ";

            try
            {
                mf.SqlConnOpen();
                DataTable dt = mf.DataTable("Select * From UsersGroup" + filter);
                mf.SqlConnClose();

                foreach (DataRow r in dt.Rows)
                {
                    model.UserName = mf.RTS(r, "UserName");
                    model.Password = mf.RTS(r, "Password");
                    model.ID = Convert.ToInt32(mf.RTS(r, "Id"));
                    var uygulamaTipi = mf.RTS(r, "UygulamaTipi");
                    model.Uygulama = uygulamaTipi;
                }
            }
            catch (Exception ex)
            {
                Singleton.WritingLogFile("GetByUserInfoForId", ex.Message.ToString());
            }

            return model;
        }
        public static YetkiUserVewModel GetByUserIdForYetkiUser(string id)
        {
            var result = new ActionResultMessages() { IsSuccess = true, UserMessage = "İşlem Başarılı" };
            var model = new YetkiUserVewModel();
            try
            {
                var mF = new ModelFunctions();
                var getYekiMenu = mF.GetSubeDataWithQuery(mF.NewConnectionString(subeIp, dbName, sqlKullaniciName, sqlKullaniciPassword), "Select * From [dbo].[YetkiMenu] Where IsActive=1");
                var getYekiIslemTip = mF.GetSubeDataWithQuery(mF.NewConnectionString(subeIp, dbName, sqlKullaniciName, sqlKullaniciPassword), "Select * From [dbo].[YetkiIslemTip] where IsActive=1");
                var getUserYetkiMenuList = mF.GetSubeDataWithQuery(mF.NewConnectionString(subeIp, dbName, sqlKullaniciName, sqlKullaniciPassword), "Select * From [dbo].[YetkiUser] where UserId=" + id);

                //YetkiNesneDetay
                model.YetkiUserDetailModelList = new List<YetkiUserDetailModel>();
                foreach (DataRow yetkiMenu in getUserYetkiMenuList.Rows)
                {
                    var userModel = new YetkiUserDetailModel
                    {
                        Id = mF.RTI(yetkiMenu, "Id"),
                        MenuId = mF.RTI(yetkiMenu, "MenuId"),
                        IslemTipiId = mF.RTI(yetkiMenu, "IslemTipiId"),
                        YetkiNesneId = mF.RTI(yetkiMenu, "YetkiNesneId")
                    };

                    model.YetkiUserDetailModelList.Add(userModel);
                }

                var yetkiliOlduguMenuList = model.YetkiUserDetailModelList.Select(x => x.MenuId).Distinct().ToList();
                //YetkiMenu
                model.YetkiMenuUserList = new List<YetkiMenuUser>();
                foreach (DataRow yetkiMenu in getYekiMenu.Rows)
                {
                    foreach (var yetki in yetkiliOlduguMenuList)
                    {
                        if (yetki == mF.RTI(yetkiMenu, "Id"))
                        {
                            var modelMenu = new YetkiMenuUser
                            {
                                Id = mF.RTI(yetkiMenu, "Id"),
                                Adi = mF.RTS(yetkiMenu, "Adi"),
                                UstMenuId = mF.RTI(yetkiMenu, "UstMenuId"),
                                YetkiDegeri = mF.RTS(yetkiMenu, "YetkiDegeri")
                            };
                            model.YetkiMenuUserList.Add(modelMenu);

                            //YetkiIslemTip
                            modelMenu.YetkiIslemTipUserList = new List<YetkiIslemTipUser>();
                            foreach (DataRow islemTipi in getYekiIslemTip.Rows)
                            {
                                var islemTipYetki = model.YetkiUserDetailModelList.Where(x => x.IslemTipiId == mF.RTI(islemTipi, "IslemTipi")).Where(x => x.MenuId == yetki).Any();
                                if (islemTipYetki)
                                {
                                    var modelIslemTipiMenu = new YetkiIslemTipUser
                                    {
                                        Id = mF.RTI(islemTipi, "Id"),
                                        IslemTipi = mF.RTI(islemTipi, "IslemTipi"),
                                        IslemTipiAdi = mF.RTS(islemTipi, "IslemTipiAdi"),
                                        MenuYetkiDegeri = mF.RTS(yetkiMenu, "YetkiDegeri"),

                                    };
                                    modelMenu.YetkiIslemTipUserList.Add(modelIslemTipiMenu);
                                }
                            }
                        }
                    }
                }
            }
            catch (OleDbException ex)
            {
                Singleton.WritingLogFile("GetByUserIdForYetkiUser", " Yetki Sorgu Hatası. Detay:" + Environment.NewLine + ex.Message.ToString());
                result.IsSuccess = false;
                result.UserMessage = "Yetki Hatası. Detay:" + ex.ToString();
                return model;
            }

            return model;
        }

        #region İp adres alma
        public string GetIp()
        {
            var strHostName = string.Empty;
            try
            {
                strHostName = Dns.GetHostName();
                IPHostEntry ipEntry = Dns.GetHostEntry(strHostName);
                var addr = ipEntry.AddressList;
                strHostName = addr[1].ToString();
            }
            catch (Exception ex)
            {
                Singleton.WritingLogFile2("GetIp", ex.ToString(), null, ex.StackTrace);
            }
            return strHostName;
        }
        #endregion

        public List<BranchAccess> GetBranchAccessList()
        {
            var branchAccess = new List<BranchAccess>();
            var result = new ActionResultMessages { IsSuccess = true, UserMessage = "İşlem başarılı", };

            try
            {
                HttpClient client = new HttpClient
                {
                    BaseAddress = new Uri(BRANCH_ACCESS_URL)
                };
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                HttpResponseMessage response = client.GetAsync("").Result;
                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    //hata açıklaması ekle.
                }
                var branchAccessResponse = new BranchAccess();
                using (HttpContent content = response.Content)
                {
                    System.Threading.Tasks.Task<string> resultMessage = content.ReadAsStringAsync();
                    var jsonString = resultMessage.Result;

                    branchAccess = JsonConvert.DeserializeObject<List<BranchAccess>>(jsonString, new IsoDateTimeConverter { DateTimeFormat = "dd/MM/yyyy h:mm tt" });
                }
            }
            catch (Exception ex)
            {
                Singleton.WritingLogFile2("GetBranchAccessList", ex.ToString(), null, ex.StackTrace);
            }
            return branchAccess;
        }

        //Yetkili şube list
        public List<SelectListItem> GetSubeSelectListItem(string kullaniciId)
        {
            var selectListItem = new List<SelectListItem>();
            var modelFunc = new ModelFunctions();
            try
            {
                var kullaniciSubeYetkisi = UsersListCRUD.KullaniciSubeYetkiListesi(kullaniciId);
                modelFunc.SqlConnOpen();
                var dt = modelFunc.DataTable("SELECT [ID],[CreateDate],[CreateDate_Timestamp],[ModifyCounter],[UpdateDate],[UpdateDate_Timestamp],[SubeName],[SubeIP],[SqlName],[SqlPassword],[DBName],[FirmaID],[DonemID],[DepoID],[Status],[AppDbType],[AppDbTypeStatus],[FasterSubeID],[SefimPanelZimmetCagrisi],[BelgeSayimTarihDahil],[ServiceAdress] FROM [SubeSettings]");
                foreach (DataRow r in dt.Rows)
                {
                    if (kullaniciSubeYetkisi != null && kullaniciSubeYetkisi.FR_SubeListesi.Where(x => x.SubeID == Convert.ToInt32(modelFunc.RTS(r, "ID"))).Select(x => x.SubeID).Any())
                    {
                        selectListItem.Add(new SelectListItem
                        {
                            Text = modelFunc.RTS(r, "SubeName").ToString(),
                            Value = modelFunc.RTS(r, "ID"),
                        });
                    }
                }
                modelFunc.SqlConnClose();
            }
            catch (Exception ex)
            {
                Singleton.WritingLogFile("GetSubeList", " Yetki Sorgu Hatası. Detay:" + Environment.NewLine + ex.Message.ToString());
            }

            return selectListItem;
        }



        //Yetkili şube list
        public List<SelectListItem> GetSubeListForUserId(string kullaniciId)
        {
            var selectListItem = new List<SelectListItem>();
            var modelFunc = new ModelFunctions();
            try
            {
                var kullaniciSubeYetkisi = UsersListCRUD.KullaniciSubeYetkiListesi(kullaniciId);
                modelFunc.SqlConnOpen();
                var dt = modelFunc.DataTable("SELECT [ID],[CreateDate],[CreateDate_Timestamp],[ModifyCounter],[UpdateDate],[UpdateDate_Timestamp],[SubeName],[SubeIP],[SqlName],[SqlPassword],[DBName],[FirmaID],[DonemID],[DepoID],[Status],[AppDbType],[AppDbTypeStatus],[FasterSubeID],[SefimPanelZimmetCagrisi],[BelgeSayimTarihDahil],[ServiceAdress] FROM [SubeSettings]");
                foreach (DataRow r in dt.Rows)
                {
                    if (kullaniciSubeYetkisi != null && kullaniciSubeYetkisi.FR_SubeListesi.Where(x => x.SubeID == Convert.ToInt32(modelFunc.RTS(r, "ID"))).Select(x => x.SubeID).Any())
                    {
                        selectListItem.Add(new SelectListItem
                        {
                            Text = modelFunc.RTS(r, "SubeName").ToString(),
                            Value = modelFunc.RTS(r, "ID"),
                        });
                    }
                }
                modelFunc.SqlConnClose();
            }
            catch (Exception ex)
            {
                Singleton.WritingLogFile("GetSubeList", " Yetki Sorgu Hatası. Detay:" + Environment.NewLine + ex.Message.ToString());
            }

            return selectListItem;
        }

    }
}