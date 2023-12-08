using SefimV2.Helper;
using SefimV2.Models;
using SefimV2.Repository;
using System.Web.Mvc;

namespace SefimV2.Controllers
{
    [CheckLoggedIn]
    public class SefimPanelTarihceController : BaseController
    {
        public ActionResult SubeList()
        {
            #region YETKİLİ ID (Left Menude filtrele de yapıyorum)    
            bool cookieExists = HttpContext.Request.Cookies["PRAUT"] != null;
            if (cookieExists == false)
            {
                return Redirect("/Authentication/Login");
            }
            string ID = Request.Cookies["PRAUT"].Value;
            ViewBag.YetkiliID = ID;
            #endregion

            var resultList = new SPosSubeFiyatGuncellemeCRUD().GetSubeList();
            return View(resultList);
        }

        [HttpGet]
        public ActionResult TarihceProductDetayList(int subeId, string productGroup)
        {
            #region YETKİLİ ID (Left Menude filtrele de yapıyorum)    
            bool cookieExists = HttpContext.Request.Cookies["PRAUT"] != null;
            if (cookieExists == false)
            {
                return Redirect("/Authentication/Login");
            }
            string ID = Request.Cookies["PRAUT"].Value;
            ViewBag.YetkiliID = ID;
            #endregion

            var model = new SefimPanelTarihceCRUD().GetBySubeIdForProductTarihce(subeId, productGroup);
            ViewBag.SubeName = model.SubeAdi;

            return View(model);
        }


        [HttpGet]
        public ActionResult TarihceChoice1DetayList(int subeId, string productName,string productGroup)
        {
            var model = new SefimPanelTarihceCRUD().GetBySubeIdForChoice1Tarihce(subeId, productName, productGroup);

            ViewBag.SubeName = model.SubeAdi;
            return View(model);
        }

        [HttpGet]
        public ActionResult Choice2Edit(int subeId, int productId, int Choice1Id)
        {
            var model = new SPosSubeFiyatGuncellemeCRUD().GetByIdForChoice2Edit(subeId, productId, Choice1Id);

            ViewBag.SubeName = model.SubeAdi;
            return View(model);
        }
    
        [HttpGet]
        public ActionResult OptionsEdit(int subeId, int productId)
        {
            var model = new SPosSubeFiyatGuncellemeCRUD().GetByIdForOptionsEdit(subeId, productId);

            ViewBag.SubeName = model.SubeAdi;
            return View(model);
        }
  
    }
}