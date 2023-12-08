using SefimV2.Helper;
using SefimV2.Models;
using SefimV2.Repository;
using SefimV2.ViewModels.SelfOutParametersSettings;
using System.Collections.Generic;
using System.Web.Mvc;

namespace SefimV2.Controllers
{
    [CheckLoggedIn]
    public class SelfOutParametersSettingsController : Controller
    {
        // GET: SelfOutParametersSettings

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
                #endregion
            }

            #region Navigasyon için eklenen kodlar
            Dictionary<string, string> PageNavi = new Dictionary<string, string>();
            PageNavi.Add("Servis Ayarları", "Index");
            //PageNavi.Add("Yeni Ekle", "");
            #endregion

            ViewBag.ActionTitle = "Listesi";
            ViewBag.Controller = "SelfOutParametersSettings";
            ViewBag.PageTitle = "Servis Ayarı";
            ViewBag.PageNavi = PageNavi;
            ViewBag.UygulamaTipi = ut;

            List<SelfOutParametersSettingsViewModel> list = SelfOutParametersSettingsCRUD.List();
            return View(list);
        }


        [HttpPost]
        public ActionResult Update(IEnumerable<SelfOutParametersSettingsViewModel> selfoutlst)
        {
            ViewBag.UygulamaTipi = "sp";
            var result = new ActionResultMessages();
            foreach (var item in selfoutlst)
            {
                result = SelfOutParametersSettingsCRUD.SelfOutParametersSettingsUpdate(item);
            }

            return Json(result, JsonRequestBehavior.AllowGet);
        }
    }
}