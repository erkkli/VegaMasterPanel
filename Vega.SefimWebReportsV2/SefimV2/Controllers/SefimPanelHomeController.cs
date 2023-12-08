using SefimV2.Helper;
using SefimV2.Models.SefimPanelHomeCRUD;
using SefimV2.Repository;
using System.Web.Mvc;

namespace SefimV2.Controllers
{
    [CheckLoggedIn]
    public class SefimPanelHomeController : Controller
    {
        // GET: SPosHome
        public ActionResult Main()
        {
            #region YETKİLİ ID (Left Menude filtrele de yapıyorum)    
            bool cookieExists = HttpContext.Request.Cookies["PRAUT"] != null;
            if (cookieExists == false)
            {
                return Redirect("/Authentication/Login");
            }
            string Id = Request.Cookies["PRAUT"].Value;
            ViewBag.YetkiliID = Id;
            #endregion

            var bussinessHelper = new BussinessHelper();
            Singleton.WritingLog("SefimPanelIp", "Ip Adres:" + bussinessHelper.GetIp() + " User Id:" + ViewBag.YetkiliID);
        
            var result = new SefimPanelHomeCRUD().GetSubeList(Id);

            var kullaniciData = BussinessHelper.GetByUserInfoForId("", "", Id);
            ViewBag.KullaniciUygulamaTipi = kullaniciData.UygulamaTipi;

            return View(result);
        }
    }
}