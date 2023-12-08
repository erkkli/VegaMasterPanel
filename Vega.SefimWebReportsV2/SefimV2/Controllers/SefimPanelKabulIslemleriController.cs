using SefimV2.Models;
using SefimV2.Repository;
using SefimV2.ViewModels.SPosVeriGonderimi;
using SefimV2.ViewModels.User;
using System.Web.Mvc;

namespace SefimV2.Controllers
{
    [CheckLoggedIn]
    public class SefimPanelKabulIslemleriController : BaseController
    {
        // GET: SPosHome
        public ActionResult Main()
        {
            return View();
        }

        #region Kabul Bekleyen İşler 

        public ActionResult KabulBekleyenIslerList()
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

            UserViewModel us = new UserCRUD().GetUserForSubeSettings(ID);
            var resultList = new SPosKabulIslemleriCRUD().GetKabulList(us.FirmaID, us.DonemID, us.DepoID);
            return View(resultList);
        }

        #endregion Kabul Bekleyen İşler


        [HttpGet]
        public ActionResult AlisGiderCreate(int subeId, string productGroup)
        {
            //TODO Sepeti getirecek
            var model = new SPosSubeFiyatGuncellemeCRUD().GetByIdForEdit(subeId, productGroup);

            ViewBag.SubeName = model.SubeAdi;
            return View(model);
        }

        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult AlisGiderCreate(UrunEditViewModel model)
        {
            if (ModelState.IsValid)
            {
                var result = new SPosSubeFiyatGuncellemeCRUD().Update(model);
                if (result.IsSuccess)
                {
                    //return View(model);
                    //return SendNotificationAfterRedirect(result.UserMessage, "FiyatGuncelleEdit", "SPosVeriGonderimi", new { subeId = model.SubeId });
                    return SendErrorAfterRedirect(result.UserMessage, "FiyatGuncelleEdit", "SPosVeriGonderimi", new { subeId = model.SubeId });
                }

                ModelState.AddModelError("", result.UserMessage);
            }

            return View(model);
        }
    }
}