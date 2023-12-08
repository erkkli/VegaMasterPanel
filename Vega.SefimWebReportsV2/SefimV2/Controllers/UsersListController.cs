using SefimV2.Helper;
using SefimV2.Models;
using SefimV2.Repository;
using SefimV2.ViewModels.Result;
using SefimV2.ViewModels.User;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Web.Mvc;

namespace SefimV2.Controllers
{
    [CheckLoggedIn]
    public class UsersListController : BaseController
    {
        // GET: UsersList
        public new ActionResult Index()
        {
            #region Page Navi işlemleri
            Dictionary<string, string> PageNavi = new Dictionary<string, string>();
            PageNavi.Add("Kullanıcı Listesi", "");
            ViewBag.PageNavi = PageNavi;
            #endregion
            return View();
        }

        public ActionResult List()
        {
            #region AYARLAR LİSTESİNİ GÖSTERMEK İÇİN (_LayoutPags De kontrolu yapılıyor)
            bool cookieExists = HttpContext.Request.Cookies["PRAUT"] != null;
            if (cookieExists == false)
            {
                return Redirect("/Authentication/Login");
            }
            string ID = Request.Cookies["PRAUT"].Value;
            ViewBag.YetkiliID = ID;
            ViewBag.KullaniciUygulamaTipi = BussinessHelper.GetByUserInfoForId("", "", ID).UygulamaTipi;
            #endregion

            #region Navigasyon için eklenen kodlar
            Dictionary<string, string> PageNavi = new Dictionary<string, string>
            {
                { "Kullanıcılar Listesi", "Index" }
            };
            #endregion
            ViewBag.ActionTitle = "Listesi";
            ViewBag.Controller = "User";
            ViewBag.PageTitle = "Kullanıcı";
            ViewBag.PageNavi = PageNavi;

            var model = UsersListCRUD.List();
            return View(model);
        }

        #region Form Ekleme & Düzenleme İşlemleri
        public ActionResult Ekle([DefaultValue(0)] int id, [DefaultValue("All")] string Page)
        {
            #region AYARLAR LİSTESİNİ GÖSTERMEK İÇİN (_LayoutPags De kontrolu yapılıyor)
            bool cookieExists = HttpContext.Request.Cookies["PRAUT"] != null;
            if (cookieExists == false)
            {
                return Redirect("/Authentication/Login");
            }
            string kullaniciId = Request.Cookies["PRAUT"].Value;
            ViewBag.YetkiliID = kullaniciId;
            #endregion

            var model = new UserViewModel();
            var jsonResult = new JsonResultModel();

            try
            {
                #region Ekleme İşlemleri
                model.ID = 0;
                model.Status = true;
                model.SubeID = 0;
                model.SubeSirasiGorunsunMu = false;
                ViewBag.ActionTitle = "Ekle";
                #endregion

                #region Güncelle
                if (id != 0)
                {
                    model = UsersListCRUD.GetUser(id, kullaniciId);

                    ViewBag.ActionTitle = "Düzenle";
                    string[] list = model.BelgeTipiYetkiList.Split(',');
                    List<string> text = new List<string>();
                    foreach (var item in list)
                    {
                        if (item == "135") { text.Add("Talep Belgesi"); }
                        else if (item == "93") { text.Add("Sayım Giriş Belgesi"); }
                        else if (item == "94") { text.Add("Zayi Giriş Belgesi"); }
                        else if (item == "20") { text.Add("Alış Faturası"); }
                        if (item == "165") { text.Add("Gider Faturası"); }
                        else if (item == "23") { text.Add("İade Faturası"); }
                        else if (item == "26") { text.Add("Alış İrsaliyesi"); }
                        else if (item == "29") { text.Add("İade İrsaliyesi"); }
                        else if (item == "128") { text.Add("Depo Transfer"); }
                        else if (item == "129") { text.Add("Depo Transfer Kabul"); }

                    }
                    ViewBag.BelgeTipiYetkiler = text;
                    if (model.YetkiStatus == "1")
                    {
                        jsonResult.UserMessage = model.YetkiStatusAciklama;
                    }
                }
                else
                {
                    model = UsersListCRUD.GetSube(kullaniciId);
                    model.Status = true;
                    model.SubeID = model.SubeID == null ? 0 : model.SubeID;
                }

                #endregion

                ViewBag.Controller = "UsersList";
                ViewBag.PageTitle = "Kullanıcı";
                ViewBag.Page = Page;
            }
            catch (Exception ex)
            {
                Singleton.WritingLogFile2("UsersListController:", ex.Message.ToString(), "", ex.StackTrace);
            }

            return View(model);
        }
        #endregion Form Ekleme & Düzenleme İşlemleri

        #region INSERT / UPDATE
        //[ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult Ekle(UserViewModel course, int[] Auth, string[] BelgeTipiYetkiler)
        {
            var result = new ActionResultMessages()
            {
                IsSuccess = true,
            };

            #region Zorunu alan kontrolleri
            if (string.IsNullOrEmpty(course.Name) ||
                string.IsNullOrEmpty(course.Password) ||
                string.IsNullOrEmpty(course.UserName) ||
                course.SubeID == 0)
            {
                result.UserMessage = "Alanlar Boş Geçilemez. Lütfen Kontrol Ediniz.";
                result.IsSuccess = false;
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            if (course.UygulamaTipi == 0)
            {
                result.IsSuccess = false;
                result.UserMessage = "Uygulama Tipi Boş Geçilemez. Lütfen Kontrol Ediniz.";
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            #endregion

            if (BelgeTipiYetkiler != null)
            {
                foreach (var item in BelgeTipiYetkiler)
                {
                    course.BelgeTipiYetkiList += item + ",";
                }
                course.BelgeTipiYetkiList = course.BelgeTipiYetkiList.Substring(0, course.BelgeTipiYetkiList.Length - 1);
            }

            if (course.ID == 0)
            {
                #region Ekleme İşlemleri                
                try
                {
                    result = new UsersListCRUD().UserProfileInsert(course, Auth);

                    if (!result.IsSuccess)
                    {
                        //return SendNotificationAfterRedirect(result.UserMessage, "List", "UserList");
                        return Json(result, JsonRequestBehavior.AllowGet);
                    }
                }
                catch (Exception ex)
                {
                    result.UserMessage = ex.Message;
                }
                #endregion
            }
            else
            {
                #region Guncelle
                try
                {
                    result = new UsersListCRUD().UserProfileUpdate(course, Auth);

                    if (!result.IsSuccess)
                    {
                        return Json(result, JsonRequestBehavior.AllowGet);
                    }
                }
                catch (Exception ex)
                {
                    result.UserMessage = ex.Message;
                }
                #endregion
            }

            return Json(result, JsonRequestBehavior.AllowGet);

        }
        #endregion INSERT / UPDATE

        #region Delete
        public ActionResult Delete(int id)
        {
            ActionResultMessages result = new ActionResultMessages();
            result = new UsersListCRUD().UserProfileDelete(id);
            return RedirectToAction("List");
        }

        [HttpPost]
        public ActionResult Delete(int id, FormCollection collection)
        {
            ActionResultMessages result = new ActionResultMessages();
            result = new UsersListCRUD().UserProfileDelete(id);
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        #endregion Delete

        #region  Subeler Listesi Bilgileri alınıyor
        [HttpPost]
        public JsonResult SubelerListJson()
        {
            List<SelectListItem> items = new List<SelectListItem>();
            ModelFunctions f = new ModelFunctions();
            //DateTime startDate = DateTime.Now;
            try
            {
                f.SqlConnOpen();
                DataTable dt = f.DataTable("select  Id, SubeName from SubeSettings");

                foreach (DataRow r in dt.Rows)
                {
                    UserViewModel model = new UserViewModel();
                    items.Add(new SelectListItem
                    {
                        Text = f.RTS(r, "SubeName").ToString(),
                        Value = f.RTS(r, "Id"),
                    });
                }
                f.SqlConnClose();
            }
            catch (System.Exception ex) { }

            return Json((items).ToList(), JsonRequestBehavior.AllowGet);
        }
        #endregion Subeler Listesi Bilgileri alınıyor

        #region  YETK'LER Listesi Bilgileri alınıyor
        [HttpPost]
        public JsonResult SubeYetkiListJson(int ID)
        {
            #region MyRegion
            //List<SelectListItem> items = new List<SelectListItem>();
            //ModelFunctions f = new ModelFunctions();
            ////DateTime startDate = DateTime.Now;
            //try
            //{
            //    f.SqlConnOpen();
            //    DataTable dt = f.DataTable("select  Id, SubeName from SubeSettings");
            //    foreach (DataRow r in dt.Rows)
            //    {
            //        UserViewModel model = new UserViewModel();
            //        items.Add(new SelectListItem
            //        {
            //            Text = f.RTS(r, "SubeName").ToString(),
            //            Value = f.RTS(r, "Id"),
            //        });
            //    }
            //    f.SqlConnClose();
            //}
            //catch (System.Exception ex)
            //{ }
            //return Json((items).ToList(), JsonRequestBehavior.AllowGet);
            #endregion

            UserViewModel model = new UserViewModel();
            ModelFunctions f = new ModelFunctions();
            DateTime startDate = DateTime.Now;
            try
            {
                f.SqlConnOpen();

                #region ŞubeListesi
                //List<FR_SubeListesiViewModel> frSubeList = new List<FR_SubeListesiViewModel>();
                DataTable dt1 = f.DataTable("select * from UserSubeRelations Where UserID=" + ID);
                foreach (DataRow r in dt1.Rows)
                {
                    model.YetkiListesi += Convert.ToInt32(f.RTS(r, "SubeID")) + ",";
                }
                #endregion
                f.SqlConnClose();
            }
            catch (System.Exception ex)
            { }
            return Json((model), JsonRequestBehavior.AllowGet);
        }
        #endregion YETK'LER Listesi Bilgileri alınıyor

        #region  Subeler Listesi Bilgileri alınıyor
        [HttpPost]
        public JsonResult FirmaListtJson()
        {
            List<SelectListItem> items = new List<SelectListItem>();
            ModelFunctions f = new ModelFunctions();
            //DateTime startDate = DateTime.Now;
            try
            {
                f.SqlConnOpen();
                DataTable dt = f.DataTable("SELECT IND, KOD, KISAAD FROM TBLFIRMA");

                foreach (DataRow r in dt.Rows)
                {
                    UserViewModel model = new UserViewModel();
                    items.Add(new SelectListItem
                    {
                        Text = f.RTS(r, "KOD").ToString(),
                        Value = f.RTS(r, "IND"),
                    });
                }
                f.SqlConnClose();
            }
            catch (System.Exception ex) { }

            return Json((items).ToList(), JsonRequestBehavior.AllowGet);
        }
        #endregion  Subeler Listesi Bilgileri alınıyor


        #region  YetkiNesne Listesi Bilgileri alınıyor
        [HttpPost]
        public JsonResult YetkiNesneListJson()
        {
            var items = new List<SelectListItem>();
            var f = new ModelFunctions();

            try
            {
                f.SqlConnOpen();
                DataTable dt = f.DataTable("SELECT [Id], [YetkiAdi] FROM [dbo].[YetkiNesne] WHERE IsActive=1");

                foreach (DataRow r in dt.Rows)
                {
                    var model = new UserViewModel();
                    items.Add(new SelectListItem
                    {
                        Text = f.RTS(r, "YetkiAdi").ToString(),
                        Value = f.RTS(r, "Id"),
                    });
                }
                f.SqlConnClose();
            }
            catch (Exception ex) { }

            return Json((items).ToList(), JsonRequestBehavior.AllowGet);
        }
        #endregion YetkiNesne Listesi Bilgileri alınıyor

    }
}