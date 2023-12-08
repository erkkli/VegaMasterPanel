using SefimV2.Models;
using SefimV2.Repository;
using SefimV2.ViewModels.User;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web.Mvc;
using static SefimV2.Enums.General;

namespace SefimV2.Controllers
{
    [CheckLoggedIn]
    public class UserController : Controller
    {
        // GET: UserController
        public ActionResult Index()
        {
            #region Page Navi işlemleri
            Dictionary<string, string> PageNavi = new Dictionary<string, string>();
            PageNavi.Add("Kullanıcı Profili", "");
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

            #endregion

            #region Navigasyon için eklenen kodlar
            Dictionary<string, string> PageNavi = new Dictionary<string, string>();
            PageNavi.Add("Kullanıcı Profili", "Index");
            //PageNavi.Add("Yeni Ekle", "");
            #endregion

            ViewBag.ActionTitle = "Profili";
            ViewBag.Controller = "User";
            ViewBag.PageTitle = "Kullanıcı";
            //ViewBag.Page = Page;
            ViewBag.PageNavi = PageNavi;

            List<UserViewModel> list = UserCRUD.List();
            return View(list);
        }

        #region Form Ekleme & Düzenleme İşlemleri

        public ActionResult Ekle([DefaultValue(0)]int id, [DefaultValue("All")] string Page)
        {
            UserViewModel model = new UserViewModel();

            #region AYARLAR LİSTESİNİ GÖSTERMEK İÇİN (_LayoutPags De kontrolu yapılıyor)
            string ID = Request.Cookies["PRAUT"].Value;
            ViewBag.YetkiliID = ID;
            //List<BelgeTipi> belgeTipi = Enum.GetValues(typeof(BelgeTipi)).Cast<BelgeTipi>().ToList();
            //ViewBag.BelgeTipList = new SelectList(belgeTipi);
            #endregion

            try
            {

                ViewBag.ActionTitle = "Ekle";

                #region Güncelle

                if (id != 0)
                {
                    model = UsersListCRUD.GetUser(id, ID);
                    ViewBag.ActionTitle = "Düzenle";
                }

                #endregion Güncelle

                ViewBag.Controller = "User";
                ViewBag.PageTitle = "Kullanıcı";
                ViewBag.Page = Page;
            }
            catch (Exception ex) { }

            return View(model);
        }

        #endregion Form Ekleme & Düzenleme İşlemleri

        #region INSERT / UPDATE
        //[ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult Ekle(UserViewModel course, string[] BelgeTipiYetkiler)
        {
            ActionResultMessages result = new ActionResultMessages();
            result.IsSuccess = false;

            #region Zorunu alan kontrolleri
            //if (string.IsNullOrEmpty(course.SUBSTATION_NAME) || string.IsNullOrEmpty(course.REGION_ID.ToString()) || string.IsNullOrEmpty(course.CITY_ID.ToString()))
            //{
            //    result.UserMessage = Singleton.getObject().ActionMessage("inputEmpty");
            //    return Json(result, JsonRequestBehavior.AllowGet);
            //}
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
                    result = new UserCRUD().UserProfileInsert(course);
                }
                catch (Exception ex)
                {
                    result.UserMessage = ex.Message;
                }

                #endregion  Ekleme İşlemleri 
            }
            else
            {
                #region Guncelle

                try
                {
                    result = new UserCRUD().UserProfileUpdate(course);
                }
                catch (Exception ex)
                {
                    result.UserMessage = ex.Message;
                }

                #endregion Guncelle
            }

            return Json(result, JsonRequestBehavior.AllowGet);
            //return RedirectToAction("List");
        }

        #endregion INSERT / UPDATE


        [HttpGet]
        public JsonResult UserJson()
        {
            string ID = Request.Cookies["PRAUT"].Value;
            //string Id = ViewBag.YetkiliID;
            UserViewModel item = new UserCRUD().GetUserForSubeSettings(ID);
            return Json((item), JsonRequestBehavior.AllowGet);
        }
    }
}