using SefimV2.Models;
using SefimV2.Repository;
using SefimV2.ViewModels.VegaDBSettings;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SefimV2.Controllers
{
    [CheckLoggedIn]
    public class VegaDBSettingsController : Controller
    {
        // GET: VegaDBAyarlari
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult List(string ut)
        {
            if (ut != "sp")
            {
                #region AYARLAR LİSTESİNİ GÖSTERMEK İÇİN (_LayoutPags De kontrolu yapılıyor)

                bool cookieExists = HttpContext.Request.Cookies["PRAUT"] != null;
                if (cookieExists == false)
                {
                    return Redirect("/Authentication/Login");
                }
                string ID = Request.Cookies["PRAUT"].Value;
                ViewBag.YetkiliID = ID;
                #endregion
            }

            #region Navigasyon için eklenen kodlar
            Dictionary<string, string> PageNavi = new Dictionary<string, string>();
            PageNavi.Add("VegaDB  Listesi", "Index");
            //PageNavi.Add("Yeni Ekle", "");
            #endregion

            ViewBag.ActionTitle = "Listesi";
            ViewBag.Controller = "VegaDBSettings";
            ViewBag.PageTitle = "VegaDB (Envanter)";
            //ViewBag.Page = Page;
            ViewBag.PageNavi = PageNavi;
            ViewBag.UygulamaTipi = ut;

            List<VegaDBSettingsViewModel> list = VegaDBSettingsCRUD.List();
            return View(list);
        }

        #region Form Ekleme & Düzenleme İşlemleri
        public ActionResult Ekle([DefaultValue(0)]int id, [DefaultValue("All")] string Page, string ut)
        {
            VegaDBSettingsViewModel model = new VegaDBSettingsViewModel();
            ViewBag.UygulamaTipi = ut;
            #region Navigasyon için eklenen kodlar

            Dictionary<string, string> PageNavi = new Dictionary<string, string>();
            PageNavi.Add("VegaDB (Envanter) Listesi", "Index");
            PageNavi.Add("Yeni Ekle", "");

            #endregion Navigasyon için eklenen kodlar

            try
            {
                #region Ekleme İşlemleri
                model.ID = 0;
                model.Status = true;
                ViewBag.ActionTitle = "Ekle";
                #endregion Ekleme İşlemleri

                #region Güncelle
                if (id != 0)
                {
                    //UserViewModel model = new UserViewModel();
                    model = VegaDBSettingsCRUD.GetUser(id);
                    ViewBag.ActionTitle = "Düzenle";
                }
                #endregion Güncelle

                ViewBag.Controller = "VegaDBSettings";
                ViewBag.PageTitle = "VegaDB";
                ViewBag.Page = Page;
                ViewBag.PageNavi = PageNavi;
            }
            catch (Exception ex)
            { }

            return View(model);
        }
        #endregion

        #region INSERT / UPDATE
        //[ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult Ekle(VegaDBSettingsViewModel course, string ut)
        {
            ActionResultMessages result = new ActionResultMessages();
            ViewBag.UygulamaTipi = ut;
            //result.IsSuccess = false;

            #region Zorunu alan kontrolleri
            if (string.IsNullOrEmpty(course.DBName) || string.IsNullOrEmpty(course.SqlName.ToString()) || string.IsNullOrEmpty(course.SqlPassword.ToString()) || string.IsNullOrEmpty(course.IP))
            {
                result.UserMessage = "Alanlar Boş Geçilemez. Lütfen Kontrol Ediniz.";// Singleton.getObject().ActionMessage("inputEmpty");
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            #endregion            

            if (course.ID == 0)
            {
                #region Ekleme İşlemleri                
                try
                {
                    result = new VegaDBSettingsCRUD().VegaDBSettingsInsert(course);
                }
                catch (Exception ex)
                {
                    result.UserMessage = ex.Message;
                }
                #endregion Ekleme İşlemleri  
            }
            else
            {
                #region Guncelle
                try
                {
                    result = new VegaDBSettingsCRUD().VegaDBSettingsUpdate(course);
                }
                catch (Exception ex)
                {
                    result.UserMessage = ex.Message;
                }
                #endregion Guncelle
            }

            return Json(result, JsonRequestBehavior.AllowGet);
        }
        #endregion INSERT / UPDATE

        #region Delete
        public ActionResult Delete(int id)
        {
            ActionResultMessages result = new ActionResultMessages();
            result = new VegaDBSettingsCRUD().VegaDBSettingsDelete(id);
            return RedirectToAction("List");
        }

        [HttpPost]
        public ActionResult Delete(int id, FormCollection collection)
        {
            ActionResultMessages result = new ActionResultMessages();
            result = new VegaDBSettingsCRUD().VegaDBSettingsDelete(id);

            return Json(result, JsonRequestBehavior.AllowGet);
        }
        #endregion Delete
    }
}