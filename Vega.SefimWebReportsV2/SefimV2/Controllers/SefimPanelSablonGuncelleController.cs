using SefimV2.Models;
using SefimV2.Repository;
using SefimV2.ViewModels.SefimPanelSablonFiyatGuncelle;
using SefimV2.ViewModels.SPosVeriGonderimi;
using System.Linq;
using System.Web.Mvc;

namespace SefimV2.Controllers
{
    [CheckLoggedIn]
    public class SefimPanelSablonGuncelleController : BaseController
    {
        #region Aktif Şube Listesi
        [HttpGet]
        public ActionResult IsActiveSubeList(string sablonName, string productGroup, string productName, string subeIdGrupList, string subeGrupList, string subeId)
        {
            #region YETKİLİ ID (Left Menude filtrele de yapıyorum)    
            bool cookieExists = HttpContext.Request.Cookies["PRAUT"] != null;
            if (cookieExists == false)
            {
                return Redirect("/Authentication/Login");
            }
            string kullaniciId = Request.Cookies["PRAUT"].Value;
            ViewBag.YetkiliID = kullaniciId;
            #endregion

            var sablonList = new SPosSubeSablonFiyatGuncellemeCRUD().GetEklenmisSablonFiyatSubeList(kullaniciId);
            sablonList.HedefSubeId = subeId;

            sablonList.IsSelectedSubeList = sablonList.IsSelectedSubeList.Where(x => x.SablonName == sablonName).ToList();
            return View(sablonList);
        }

        #endregion Aktif Şube Listesi

        [HttpGet]
        public ActionResult SablonList(string subeIdGrupList, string sablonName, string tumSubelereYay, bool isUpdate = false)
        {
            #region YETKİLİ ID (Left Menude filtrele de yapıyorum)    
            bool cookieExists = HttpContext.Request.Cookies["PRAUT"] != null;
            if (cookieExists == false)
            {
                return Redirect("/Authentication/Login");
            }
            string kullaniciId = Request.Cookies["PRAUT"].Value;
            ViewBag.YetkiliID = kullaniciId;
            #endregion

            var sablonCRUD = new SPosSubeSablonFiyatGuncellemeCRUD();
            //Template Tablosunu oluşturur.
            var productTemplateTableUpdate = sablonCRUD.SubeSablonDeleteCreate(kullaniciId);
            var sablonVarMi = sablonCRUD.GetEklenmisSablonFiyatSubeList(kullaniciId);

            if (isUpdate)
            {
                var result = sablonCRUD.IsUpdateProductTemplatePrice(null, subeIdGrupList, sablonName, tumSubelereYay, kullaniciId);
                if (result.IsSuccess)
                {
                    return SendNotificationAfterRedirect(result.UserMessage, "SablonList", "SefimPanelSablonGuncelle");
                }
                ModelState.AddModelError("", result.UserMessage);
            }

            if (sablonVarMi.GuncellenecekSubeGruplariList.Count == 0 && (sablonVarMi.IsSelectedSubeList.Count == 0 || sablonVarMi.IsSelectedSubeList.FirstOrDefault().SubeName == null))
            {
                var resultSablonCreate = sablonCRUD.SubeSablonDeleteCreate(kullaniciId);
                if (resultSablonCreate.IsSuccess)
                {
                    var sablonList = sablonCRUD.GetEklenmisSablonFiyatSubeList(kullaniciId);
                    return View(sablonList);
                }
            }

            return View(sablonVarMi);
        }

        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult SablonList(SablonSubeViewModel model, string SubeIdGrupList, string sablonName, string tumSubelereYay, bool isUpdate = false)
        {
            #region YETKİLİ ID (Left Menude filtrele de yapıyorum)    
            bool cookieExists = HttpContext.Request.Cookies["PRAUT"] != null;
            if (cookieExists == false)
            {
                return Redirect("/Authentication/Login");
            }
            string kullaniciId = Request.Cookies["PRAUT"].Value;
            ViewBag.YetkiliID = kullaniciId;
            #endregion

            if (model.IsSelectedSubeList == null)
            {
                return SendWarningAfterRedirect("Lütfen fiyat güncellemesi yapılacak hedef şube seçiniz.", "SablonList", "SefimPanelSablonGuncelle");
            }

            var sablonGrupKontrol = model.IsSelectedSubeList.Where(x => x.IsSelectedHedefSube).Select(x => x.SablonName).Distinct().ToList();
            if (sablonGrupKontrol.Count > 1)
            {
                return SendWarningAfterRedirect("Farklı şablonlar gruplanamaz.Lütfen aynı şablonların seçili olduğundan emin olunuz.", "SablonList", "SefimPanelSablonGuncelle");
            }

            var isSelectedKaynakSube = model.IsSelectedSubeList.Where(x => x.IsSelectedKaynakSube);
            var isSelectedHedefSube = model.IsSelectedSubeList.Where(x => x.IsSelectedHedefSube);
            var subeName = model.IsSelectedSubeList.FirstOrDefault().SubeName;
            if (isSelectedHedefSube == null || isSelectedHedefSube.Count() == 0 || subeName == null)
            {
                ModelState.AddModelError(string.Empty, "Lütfen fiyat güncellemesi yapılacak hedef şube seçiniz.");
            }

            var sablonVarMi = new SPosSubeSablonFiyatGuncellemeCRUD().GetEklenmisSablonFiyatSubeList(kullaniciId);
            if (sablonVarMi.FiyatGuncellemsiHazirlananSubeList != null && sablonVarMi.FiyatGuncellemsiHazirlananSubeList.Count > 0)
            {
                ModelState.AddModelError(string.Empty, "Güncellenmesi devam eden şablon mevcuttur.");
            }


            if (isUpdate)
            {
                var result = new SPosSubeSablonFiyatGuncellemeCRUD().IsUpdateProductTemplatePrice(model, SubeIdGrupList, sablonName, tumSubelereYay, kullaniciId);
                if (result.IsSuccess)
                {
                    return SendNotificationAfterRedirect(result.UserMessage, "SablonList", "SefimPanelSablonGuncelle");
                }
                ModelState.AddModelError("", result.UserMessage);
            }


            if (ModelState.IsValid)
            {
                var result = new SPosSubeSablonFiyatGuncellemeCRUD().SubeSablonInsertLocalTable(model, kullaniciId);
                if (result.IsSuccess)
                {
                    return SendNotificationAfterRedirect(result.UserMessage, "SablonList", "SefimPanelSablonGuncelle");
                }
                ModelState.AddModelError("", result.UserMessage);
            }

            return View(model);
        }


        #region Şablon product update       
        //[HttpGet]
        //public ActionResult EditSubeSablonPrice(string productGroup, string SubeIdGrupList, string sablonName)
        //{
        //    var result = new SPosSubeSablonFiyatGuncellemeCRUD().GetByProductForSubeListEdit(productGroup, SubeIdGrupList, sablonName);
        //    if (!result.IsSuccess)
        //    {
        //        SendWarning(result.ErrorList.FirstOrDefault());
        //    }

        //    ViewBag.SubeName = result.SubeAdi;
        //    result.SubeIdGrupList = SubeIdGrupList;
        //    result.SablonName = sablonName;

        //    return View(result);
        //}

        [HttpGet]
        public ActionResult EditSubeSablonPrice(string productGroup, string SubeIdGrupList, string sablonName)
        {
            var result = new SPosSubeSablonFiyatGuncellemeCRUD().GetByProductForSubeListEdit2(productGroup, SubeIdGrupList, sablonName);
            if (!result.IsSuccess)
            {
                SendWarning(result.ErrorList.FirstOrDefault());
            }

            //ViewBag.SubeName = result.SubeAdi;
            result.SubeIdGrupList = SubeIdGrupList;
            result.SablonName = sablonName;

            return View(result);
        }
        //

        //[ValidateAntiForgeryToken]
        //[HttpPost]
        //public ActionResult EditSubeSablonPrice(UrunEditViewModel model)
        //{
        //    model.KullaniciId = Request.Cookies["PRAUT"].Value;
        //    //TODO eğer bir değer doluysa insert ya da updsate yapılabilir, null ise delete işlemi, yapılabilir.
        //    if (ModelState.IsValid)
        //    {
        //        var result = new SPosSubeSablonFiyatGuncellemeCRUD().UpdateByProductForSubeList(model);
        //        if (result.IsSuccess)
        //        {
        //            return SendNotificationAfterRedirect(result.UserMessage, "EditSubeSablonPrice", "SefimPanelSablonGuncelle", new { SubeIdGrupList = model.SubeIdGrupList, sablonName = model.SablonName });
        //        }
        //        ModelState.AddModelError("", result.UserMessage);
        //    }
        //    var data = new SPosSubeSablonFiyatGuncellemeCRUD().GetByProductForSubeListEdit("", model.SubeIdGrupList, model.SablonName);
        //    return View(data);
        //}


        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult EditSubeSablonPrice(UrunEditViewModel2 model)
        {
            model.KullaniciId = Request.Cookies["PRAUT"].Value;
            //TODO eğer bir değer doluysa insert ya da updsate yapılabilir, null ise delete işlemi, yapılabilir.
            if (ModelState.IsValid)
            {
                var result = new SPosSubeSablonFiyatGuncellemeCRUD().UpdateByProductForSubeList2(model);
                if (result.IsSuccess)
                {
                    return SendNotificationAfterRedirect(result.UserMessage, "EditSubeSablonPrice", "SefimPanelSablonGuncelle", new { SubeIdGrupList = model.SubeIdGrupList, sablonName = model.SablonName });
                }

                ModelState.AddModelError("", result.UserMessage);
            }

            return View(model);
        }

        #endregion Şablon product update



        #region Choice1 update
        //[HttpGet]
        //public ActionResult SablonChoice1EditForSubeList(string subeId, string productId, string subeIdGrupList, string sablonName)
        //{
        //    var model = new SPosSubeSablonFiyatGuncellemeCRUD().GetByChoice1ForSubeListEdit(subeId, productId, subeIdGrupList, sablonName);
        //    ViewBag.SubeName = model.SubeAdi;
        //    model.SubeIdGrupList = subeIdGrupList;
        //    model.SablonName = sablonName;
        //    return View(model);
        //}

        //[ValidateAntiForgeryToken]
        //[HttpPost]
        //public ActionResult SablonChoice1EditForSubeList(UrunEditViewModel model)
        //{
        //    //if (ModelState.IsValid)
        //    //{
        //    model.KullaniciId = Request.Cookies["PRAUT"].Value;
        //    var result = new SPosSubeSablonFiyatGuncellemeCRUD().UpdateBySablonChoice1ForSubeList(model);
        //    if (result.IsSuccess)
        //    {
        //        //return SendNotificationAfterRedirect(result.UserMessage, "EditSubeProduct", "SefimPanelVeriGonderimi");
        //        SendNotification(result.UserMessage);
        //        model = new SPosSubeSablonFiyatGuncellemeCRUD().GetByChoice1ForSubeListEdit(model.productCompairsList.FirstOrDefault().ProductGroup, model.productCompairsList.FirstOrDefault().ProductName, model.SubeIdGrupList, model.SablonName);
        //        //return SendNotificationAfterRedirect(result.UserMessage, "SablonChoice1EditForSubeList", "SefimPanelSablonGuncelle", new { subeId = model.productCompairsList.FirstOrDefault().ProductGroup, productId = model.productCompairsList.FirstOrDefault().ProductName });
        //        return View(model);
        //    }
        //    //ModelState.AddModelError("", result.UserMessage);
        //    //}
        //    return View(model);
        //}

        [HttpGet]
        public ActionResult SablonChoice1EditForSubeList(string subeId, string productId, string subeIdGrupList, string sablonName)
        {
            var model = new SPosSubeSablonFiyatGuncellemeCRUD().GetByChoice1ForSubeListEdit2(subeId, productId, subeIdGrupList, sablonName);
            //ViewBag.SubeName = model.SubeAdi;
            model.SubeIdGrupList = subeIdGrupList;
            model.SablonName = sablonName;

            return View(model);
        }

        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult SablonChoice1EditForSubeList(UrunEditViewModel2 model)
        {
            //if (ModelState.IsValid)
            //{
            model.KullaniciId = Request.Cookies["PRAUT"].Value;
            var result = new SPosSubeSablonFiyatGuncellemeCRUD().UpdateBySablonChoice1ForSubeList2(model);
            if (result.IsSuccess)
            {
                //return SendNotificationAfterRedirect(result.UserMessage, "EditSubeProduct", "SefimPanelVeriGonderimi");
                SendNotification(result.UserMessage);
                model = new SPosSubeSablonFiyatGuncellemeCRUD().GetByChoice1ForSubeListEdit2(model.ProductList.FirstOrDefault().ProductGroup, model.ProductList.FirstOrDefault().ProductName, model.SubeIdGrupList, model.SablonName);
                //return SendNotificationAfterRedirect(result.UserMessage, "SablonChoice1EditForSubeList", "SefimPanelSablonGuncelle", new { subeId = model.productCompairsList.FirstOrDefault().ProductGroup, productId = model.productCompairsList.FirstOrDefault().ProductName });
                return View(model);
            }

            //ModelState.AddModelError("", result.UserMessage);
            //}

            return View(model);
        }
        #endregion Choice1 update



        #region Choice2 update
        //[HttpGet]
        //public ActionResult SablonChoice2EditForSubeList(string subeId, string choice1Id, string subeIdGrupList, string sablonName)
        //{
        //    var result = new SPosSubeSablonFiyatGuncellemeCRUD().GetByChoice2ForSubeListEdit(subeId, choice1Id, subeIdGrupList, sablonName);
        //    if (!result.IsSuccess)
        //    {
        //        SendWarning(result.ErrorList.FirstOrDefault());
        //    }

        //    ViewBag.SubeName = result.SubeAdi;
        //    result.SubeIdGrupList = subeIdGrupList;
        //    result.SablonName = sablonName;

        //    return View(result);
        //}

        //[ValidateAntiForgeryToken]
        //[HttpPost]
        //public ActionResult SablonChoice2EditForSubeList(UrunEditViewModel model)
        //{
        //    model.KullaniciId = Request.Cookies["PRAUT"].Value;
        //    var result = new SPosSubeSablonFiyatGuncellemeCRUD().UpdateByChoice2ForSubeList(model);
        //    if (result.IsSuccess)
        //    {
        //        SendNotification(result.UserMessage);
        //        model = new SPosSubeSablonFiyatGuncellemeCRUD().GetByChoice2ForSubeListEdit(model.productCompairsList.FirstOrDefault().ProductGroup, model.productCompairsList.FirstOrDefault().ProductName, model.SubeIdGrupList, model.SablonName);
        //        return SendNotificationAfterRedirect(result.UserMessage, "SablonChoice2EditForSubeList", "SefimPanelSablonGuncelle",
        //            new
        //            {
        //                subeId = model.productCompairsList.FirstOrDefault().ProductGroup,
        //                choice1Id = model.productCompairsList.FirstOrDefault().ProductName,
        //                subeIdGrupList = model.SubeIdGrupList
        //            });
        //    }

        //    return View(model);
        //}

        [HttpGet]
        public ActionResult SablonChoice2EditForSubeList(string subeId, string choice1Id, string subeIdGrupList, string sablonName)
        {
            var result = new SPosSubeSablonFiyatGuncellemeCRUD().GetByChoice2ForSubeListEdit2(subeId, choice1Id, subeIdGrupList, sablonName);
            if (!result.IsSuccess)
            {
                SendWarning(result.ErrorList.FirstOrDefault());
            }

            //ViewBag.SubeName = result.SubeAdi;
            result.SubeIdGrupList = subeIdGrupList;
            result.SablonName = sablonName;

            return View(result);
        }

        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult SablonChoice2EditForSubeList(UrunEditViewModel2 model)
        {
            var subeIdGrupList = model.SubeIdGrupList;
            var sablonName = model.SablonName;
            var ProductGroup = model.ProductList.FirstOrDefault().ProductGroup;
            var ProductName = model.ProductList.FirstOrDefault().ProductName;

            model.KullaniciId = Request.Cookies["PRAUT"].Value;
            var result = new SPosSubeSablonFiyatGuncellemeCRUD().UpdateByChoice2ForSubeList2(model);
            if (result.IsSuccess)
            {
                SendNotification(result.UserMessage);
                model = new SPosSubeSablonFiyatGuncellemeCRUD().GetByChoice2ForSubeListEdit2(model.ProductList.FirstOrDefault().ProductGroup, model.ProductList.FirstOrDefault().ProductName, model.SubeIdGrupList, model.SablonName);
                return SendNotificationAfterRedirect(result.UserMessage, "SablonChoice2EditForSubeList", "SefimPanelSablonGuncelle",
                    new
                    {
                        subeId = ProductGroup,
                        choice1Id = ProductName,
                        subeIdGrupList = subeIdGrupList,
                        sablonName = sablonName
                    });
            }

            return View(model);
        }
        #endregion Choice2 update




        #region Options update
        //[HttpGet]
        //public ActionResult SablonOptionsEditForSubeList(string subeId, string productId, string subeIdGrupList, string sablonName)
        //{
        //    var model = new SPosSubeFiyatGuncellemeCRUD().GetByChoice1ForSubeListEdit(subeId, productId, subeIdGrupList);
        //    ViewBag.SubeName = model.SubeAdi;
        //    model.SubeIdGrupList = subeIdGrupList;
        //    model.SablonName = sablonName;
        //    return View(model);
        //}

        //[ValidateAntiForgeryToken]
        //[HttpPost]
        //public ActionResult SablonOptionsEditForSubeList(UrunEditViewModel model)
        //{
        //    //if (ModelState.IsValid)
        //    //{
        //    model.KullaniciId = Request.Cookies["PRAUT"].Value;
        //    var result = new SPosSubeSablonFiyatGuncellemeCRUD().UpdateByOptionsForSubeList(model, false);
        //    if (result.IsSuccess)
        //    {
        //        model = new SPosSubeSablonFiyatGuncellemeCRUD().GetByChoice1ForSubeListEdit(model.productOptionsCompairsList.FirstOrDefault().ProductGroup, model.productOptionsCompairsList.FirstOrDefault().ProductName, model.SubeIdGrupList, model.SablonName);
        //        return SendNotificationAfterRedirect(result.UserMessage, "SablonChoice1EditForSubeList", "SefimPanelSablonGuncelle",
        //            new
        //            {
        //                subeId = model.productOptionsCompairsList.FirstOrDefault().ProductGroup,
        //                productId = model.productOptionsCompairsList.FirstOrDefault().ProductName,
        //                subeIdGrupList = model.SubeIdGrupList
        //            });
        //        //SendNotification(result.UserMessage);
        //        //return View(model);
        //    }
        //    ModelState.AddModelError("", result.UserMessage);
        //    //}
        //    return View(model);
        //}

        [HttpGet]
        public ActionResult SablonOptionsEditForSubeList(string subeId, string productId, string subeIdGrupList, string sablonName)
        {
            var model = new SPosSubeSablonFiyatGuncellemeCRUD().GetByChoice1ForSubeListEdit(subeId, productId, subeIdGrupList);


            //ViewBag.SubeName = model.SubeAdi;
            model.SubeIdGrupList = subeIdGrupList;
            model.SablonName = sablonName;

            return View(model);
        }

        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult SablonOptionsEditForSubeList(UrunEditViewModel2 model)
        {
            var subeIdGrupList = model.SubeIdGrupList;
            var productName = model.ProductOptionsList.FirstOrDefault().ProductName;
            var productGroup = model.ProductOptionsList.FirstOrDefault().ProductGroup;
            var sablonName = model.SablonName;
            //if (ModelState.IsValid)
            //{
            model.KullaniciId = Request.Cookies["PRAUT"].Value;
            var result = new SPosSubeSablonFiyatGuncellemeCRUD().UpdateByOptionsForSubeList2(model, false);
            if (result.IsSuccess)
            {
                model = new SPosSubeSablonFiyatGuncellemeCRUD().GetByChoice1ForSubeListEdit2(model.ProductOptionsList.FirstOrDefault().ProductGroup, model.ProductOptionsList.FirstOrDefault().ProductName, model.SubeIdGrupList, model.SablonName);
                return SendNotificationAfterRedirect(result.UserMessage, "SablonChoice1EditForSubeList", "SefimPanelSablonGuncelle",
                    new
                    {
                        subeId = productGroup,
                        productId = productName,
                        subeIdGrupList = subeIdGrupList,
                        sablonName = sablonName

                    });
                //SendNotification(result.UserMessage);
                //return View(model);
            }

            ModelState.AddModelError("", result.UserMessage);
            //}

            return View(model);
        }

        #endregion Options



        #region Ortak options güncelleme

        [HttpGet]
        public ActionResult SablonCommonOptionsEdit(string productGroup, string productName, string optionsName, string subeIdGrupList, string sablonName)
        {
            var model = new SPosSubeSablonFiyatGuncellemeCRUD().GetBySablonCommonOptionsForSubeListEdit(productGroup, productName, optionsName, subeIdGrupList);

            //ViewBag.SubeName = model.SubeAdi;
            model.SubeIdGrupList = subeIdGrupList;
            model.SablonName = sablonName;
            return View(model);
        }

        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult SablonCommonOptionsEdit(UrunEditViewModel2 model)
        {
            //if (ModelState.IsValid)
            //{
            string subeListesi = model.SubeIdGrupList;
            model.KullaniciId = Request.Cookies["PRAUT"].Value;
            var result = new SPosSubeSablonFiyatGuncellemeCRUD().UpdateByOptionsForSubeList2(model, true);
            if (result.IsSuccess)
            {
                model = new SPosSubeSablonFiyatGuncellemeCRUD().GetByChoice1ForSubeListEdit2(model.ProductOptionsList.FirstOrDefault().ProductGroup, model.ProductOptionsList.FirstOrDefault().ProductName, model.SubeIdGrupList, model.SablonName);
                return SendNotificationAfterRedirect(result.UserMessage, "SablonChoice1EditForSubeList", "SefimPanelSablonGuncelle",
                    new
                    {
                        subeId = model.ProductOptionsList.FirstOrDefault().ProductGroup,
                        productId = model.ProductOptionsList.FirstOrDefault().ProductName,
                        subeIdGrupList = subeListesi
                    });
                //SendNotification(result.UserMessage);
                //return View(model);
            }

            ModelState.AddModelError("", result.UserMessage);
            //}

            return View(model);
        }

        #endregion Ortak options güncelleme

        #region Delete hazırlanan şablon
        [HttpPost]
        public JsonResult Delete(string SubeIdGrupList, string SablonName)
        {
            var result = new SPosSubeSablonFiyatGuncellemeCRUD().DeleteProductTemplatePrice(SubeIdGrupList, SablonName);
            return Json((result), JsonRequestBehavior.AllowGet);
        }

        #endregion
    }
}