using SefimV2.Models;
using SefimV2.Repository;
using SefimV2.ViewModels.YetkiNesneViewModel;
using System.Web.Mvc;

namespace SefimV2.Controllers
{
    [CheckLoggedIn]
    public class YetkiNesneController : BaseController
    {
        [HttpGet]
        public ActionResult List(string ut)
        {
            ViewBag.UygulamaTipi = ut;

            if (ut != "sp")
            {
                #region AYARLAR LİSTESİNİ GÖSTERMEK İÇİN (_LayoutPags De kontrolu yapılıyor)

                bool cookieExists = HttpContext.Request.Cookies["PRAUT"] != null;
                if (cookieExists == false)
                {
                    return Redirect("/Authentication/Login");
                }
                string id = Request.Cookies["PRAUT"].Value;
                ViewBag.YetkiliID = id;
                #endregion
            }


            var result = new YetkiNesneCRUD().GetList();

            return View(result);
        }


        [HttpGet]
        public ActionResult Create(string ut)
        {
            ViewBag.UygulamaTipi = ut;
            #region YETKİLİ ID (Left Menude filtrele de yapıyorum)    
            bool cookieExists = HttpContext.Request.Cookies["PRAUT"] != null;
            if (cookieExists == false)
            {
                return Redirect("/Authentication/Login");
            }
            string id = Request.Cookies["PRAUT"].Value;
            ViewBag.YetkiliID = id;
            #endregion

            var result = new YetkiNesneCRUD().GetForCreate();

            return View(result);
        }

        [HttpPost]
        [ValidateAntiForgeryToken()]
        public ActionResult Create(YetkiNesneViewModel model, string ut)
        {
            ViewBag.UygulamaTipi = ut;

            #region YETKİLİ ID (Left Menude filtrele de yapıyorum)    
            //var appType = Request.Cookies["ATYPE"].Value;
            bool cookieExists = HttpContext.Request.Cookies["PRAUT"] != null;
            if (cookieExists == false)
            {
                return Redirect("/Authentication/Login");
            }
            string id = Request.Cookies["PRAUT"].Value;
            ViewBag.YetkiliID = id;
            #endregion
            //model.YetkiAdi = model.YetkiDegeri.Trim();

            if (ModelState.IsValid)
            {

                var result = new YetkiNesneCRUD().Create(model);
                if (result.IsSuccess)
                {
                    return SendNotificationAfterRedirect(result.UserMessage, "List", "YetkiNesne", new { ut = ut });
                }
                ModelState.AddModelError("", result.UserMessage);
            }
            model = new YetkiNesneCRUD().GetForCreate();

            return View(model);
        }

        [HttpGet]
        public ActionResult Edit(int id, string ut)
        {
            ViewBag.UygulamaTipi = ut;
            #region YETKİLİ ID (Left Menude filtrele de yapıyorum)    
            bool cookieExists = HttpContext.Request.Cookies["PRAUT"] != null;
            if (cookieExists == false)
            {
                return Redirect("/Authentication/Login");
            }
            string iD = Request.Cookies["PRAUT"].Value;
            ViewBag.YetkiliID = iD;
            #endregion

            var model = new YetkiNesneCRUD().GetById(id);

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken()]
        public ActionResult Edit(YetkiNesneViewModel model, string ut)
        {
            ViewBag.UygulamaTipi = ut;

            #region YETKİLİ ID (Left Menude filtrele de yapıyorum)    
            bool cookieExists = HttpContext.Request.Cookies["PRAUT"] != null;
            if (cookieExists == false)
            {
                return Redirect("/Authentication/Login");
            }
            string iD = Request.Cookies["PRAUT"].Value;
            ViewBag.YetkiliID = iD;
            #endregion

            var result = new YetkiNesneCRUD().Update(model);
            if (result.IsSuccess)
            {
                return SendNotificationAfterRedirect(result.UserMessage, "List", "YetkiNesne", new { ut = ut });                
            }

            ModelState.AddModelError("", result.UserMessage);

            return View(model);
        }
    }
}