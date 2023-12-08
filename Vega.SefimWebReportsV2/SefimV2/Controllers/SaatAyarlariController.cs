using SefimV2.Models;
using SefimV2.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SefimV2.Controllers
{
    [CheckLoggedIn]
    public class SaatAyarlariController : Controller
    {
        // GET: SaatAyarlari
        public ActionResult Index()
        {
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
            Saat saat = Ayarlar.SaatListe();
            return View(saat);
        }

        [HttpPost]
        public ActionResult Save(Saat saat)
        {
            ActionResultMessages result = new ActionResultMessages();

            if (string.IsNullOrEmpty(saat.StartTime) || string.IsNullOrEmpty(saat.EndTime))
            {
                result.result_STR = "Alanlar Boş Geçilemez.";
                result.result_INT = 0;
                result.result_BOOL = false;
                result.UserMessage = "Alanlar Boş Geçilemez.";
                return Json(result, JsonRequestBehavior.AllowGet);
            }


            if (saat != null)
            {
                if (saat.StartTime != null & saat.EndTime != null)
                {
                    result = new Ayarlar().SaatAyariGuncelle(saat);
                }
                else
                {
                    result = new Ayarlar().SaatAyariEkle(saat);
                }
            }
            return Json(result, JsonRequestBehavior.AllowGet);
        }
    }
}