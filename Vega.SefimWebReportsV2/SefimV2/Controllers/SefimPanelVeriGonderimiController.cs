using SefimV2.Helper;
using SefimV2.Models;
using SefimV2.Repository;
using SefimV2.ViewModels.SPosVeriGonderimi;
using System;
using System.Linq;
using System.Web.Mvc;

namespace SefimV2.Controllers
{
    [CheckLoggedIn]
    public class SefimPanelVeriGonderimiController : BaseController
    {
        public ActionResult _Modal(string ModalId)
        {
            if (string.IsNullOrEmpty(ModalId))
                ViewBag.ModalId = "genericModal";
            else
                ViewBag.ModalId = ModalId;
            return PartialView();
        }
        // GET: SPosHome
        public ActionResult Main()
        {
            return View();
        }

        #region Şube Listesi       
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
            ViewBag.KullaniciUygulamaTipi = BussinessHelper.GetByUserInfoForId("", "", ID).UygulamaTipi;
            #endregion

            var resultList = new SPosSubeFiyatGuncellemeCRUD().GetSubeList();
            return View(resultList);
        }
        #endregion Şube Listesi


        #region Aktif Şube Listesi
        [HttpGet]
        public ActionResult IsActiveSubeList(string productGroup, string productName, string subeIdGrupList, string subeGrupList, string subeId)
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

            var resultList = new SPosSubeFiyatGuncellemeCRUD().GetLocalSubeListIsSelectedRemovePreviousList(kullaniciId);
            resultList.HedefSubeId = subeId;

            return View(resultList);
        }

        #endregion Aktif Şube Listesi


        #region Şubeye Fiyat Gönder önceki ekranlar

        [Obsolete("Önceki analize göre yapılmıştı.SubeSettings tablosunda tanımlı subeleri getirir")]
        [HttpGet]
        public ActionResult SubeyeFiyatGonderSubeList()
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

            var resultList = new SPosSubeFiyatGuncellemeCRUD().GetSubeListIsSelected();
            return View(resultList);
        }


        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult SubeyeFiyatGonderSubeList(SubelereVeriGonderViewModel model)
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

            var isSelectedKaynakSube = model.IsSelectedSubeList.Where(x => x.IsSelectedKaynakSube);
            var isSelectedHedefSube = model.IsSelectedSubeList.Where(x => x.IsSelectedHedefSube);

            if (isSelectedHedefSube.Count() == 0)
            {
                ModelState.AddModelError(string.Empty, "Lütfen fiyat güncellemsi yapılacak hedef şube seçiniz.");
            }
            if (isSelectedKaynakSube.Count() == 0)
            {
                ModelState.AddModelError(string.Empty, "Lütfen kaynak şube seçiniz.");
            }
            if (isSelectedKaynakSube.Count() > 1)
            {
                ModelState.AddModelError(string.Empty, "Birden fazla kaynak şube seçilemez.");
            }

            if (ModelState.IsValid)
            {
                var result = new SPosSubeFiyatGuncellemeCRUD().SubelereFiyatGonder(model);
                if (result.IsSuccess)
                {
                    return SendNotificationAfterRedirect(result.UserMessage, "SubeyeFiyatGonderSubeList", "SefimPanelVeriGonderimi");
                }

                ModelState.AddModelError("", result.UserMessage);
            }

            //var resultList = new SPosSubeFiyatGuncellemeCRUD().GetSubeListIsSelected();
            return View(model);
        }

        [HttpGet]
        public ActionResult FiyatGuncelleEdit(int subeId, string productGroup)
        {

            var model = new SPosSubeFiyatGuncellemeCRUD().GetByIdForEdit(subeId, productGroup);
            ViewBag.SubeName = model.SubeAdi;
            return View(model);
        }

        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult FiyatGuncelleEdit(UrunEditViewModel model)
        {
            if (ModelState.IsValid)
            {
                var result = new SPosSubeFiyatGuncellemeCRUD().Update(model);
                if (result.IsSuccess)
                {
                    //return View(model);
                    //return SendNotificationAfterRedirect(result.UserMessage, "FiyatGuncelleEdit", "SPosVeriGonderimi", new { subeId = model.SubeId });
                    return SendNotificationAfterRedirect(result.UserMessage, "FiyatGuncelleEdit", "SefimPanelVeriGonderimi", new { subeId = model.SubeId });
                }

                ModelState.AddModelError("", result.UserMessage);
            }

            return View(model);
        }

        [HttpGet]
        public ActionResult Choice1Edit(int subeId, int productId)
        {
            var model = new SPosSubeFiyatGuncellemeCRUD().GetByIdForChoiceEdit(subeId, productId);

            ViewBag.SubeName = model.SubeAdi;
            return View(model);
        }

        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult Choice1Edit(UrunEditViewModel model)
        {
            if (ModelState.IsValid)
            {
                var result = new SPosSubeFiyatGuncellemeCRUD().ChoiceUpdate(model);
                if (result.IsSuccess)
                {
                    return SendNotificationAfterRedirect(result.UserMessage, "FiyatGuncelleEdit", "SefimPanelVeriGonderimi", new { subeId = model.SubeId });
                }

                ModelState.AddModelError("", result.UserMessage);
            }

            return View(model);
        }

        [HttpGet]
        public ActionResult Choice2Edit(int subeId, int productId, int Choice1Id)
        {
            var model = new SPosSubeFiyatGuncellemeCRUD().GetByIdForChoice2Edit(subeId, productId, Choice1Id);

            ViewBag.SubeName = model.SubeAdi;
            return View(model);
        }

        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult Choice2Edit(UrunEditViewModel model)
        {
            if (ModelState.IsValid)
            {
                var result = new SPosSubeFiyatGuncellemeCRUD().ChoiceUpdate(model);
                if (result.IsSuccess)
                {
                    return SendNotificationAfterRedirect(result.UserMessage, "FiyatGuncelleEdit", "SefimPanelVeriGonderimi", new { subeId = model.SubeId });
                }

                ModelState.AddModelError("", result.UserMessage);
            }

            return View(model);
        }

        [HttpGet]
        public ActionResult OptionsEdit(int subeId, int productId)
        {
            var model = new SPosSubeFiyatGuncellemeCRUD().GetByIdForOptionsEdit(subeId, productId);

            ViewBag.SubeName = model.SubeAdi;
            return View(model);
        }

        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult OptionsEdit(UrunEditViewModel model)
        {
            if (ModelState.IsValid)
            {
                var result = new SPosSubeFiyatGuncellemeCRUD().OptionsUpdate(model);
                if (result.IsSuccess)
                {
                    //return SendNotificationAfterRedirect(result.UserMessage, "FiyatGuncelleEdit", "SefimPanelVeriGonderimi", new { subeId = model.SubeId });
                    SendNotification(result.UserMessage);

                    return SendNotificationAfterRedirect(result.UserMessage, "FiyatGuncelleEdit", "SefimPanelVeriGonderimi", new { subeId = model.SubeId });
                }
                else
                {
                    ModelState.AddModelError("", result.UserMessage);

                }

            }

            return PartialView(model);
        }

        #endregion Şubeye Fiyat Gönder önceki ekranlar

        #region ** (05.02.2022 toplantı sonrası) Şubelerdeki ürünleri local db'ye aktarma ve dinamik olarak şubeleri sayfada gösterme **

        [HttpGet]
        public ActionResult GetSubeListInsertSubePruduct(string SubeIdGrupList, bool isUpdate = false)
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

            //if (isUpdate)
            //{
            //    var result = new SPosSubeFiyatGuncellemeCRUD().IsUpdateProduct(SubeIdGrupList, kullaniciId);
            //    if (result.IsSuccess)
            //    {
            //        return SendNotificationAfterRedirect(result.UserMessage, "GetSubeListInsertSubePruduct", "SefimPanelVeriGonderimi");
            //    }
            //    ModelState.AddModelError("", result.UserMessage);
            //}

            var resultList = new SPosSubeFiyatGuncellemeCRUD().GetLocalSubeListIsSelectedRemovePreviousList(kullaniciId);

            return View(resultList);
        }

        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult GetSubeListInsertSubePruduct(SubelereVeriGonderViewModel model, string SubeIdGrupList, bool isUpdate = false)
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

            #region Hedef şubelerde fiyat güncellemesi yapar

            if (isUpdate)
            {
                var isSelectedUpdatedSubeList = model.GuncellenecekSubeGruplariList.SelectMany(y => y.FiyatGuncellemsiHazirlananSubeList.Where(x => x.IsSelectedHazirlananSubeList == true).Select(x => x.IsSelectedHazirlananSubeList).ToList());
                if (!isSelectedUpdatedSubeList.Contains(true))
                {
                    if (SubeIdGrupList == "TumSubelereYay" || !string.IsNullOrWhiteSpace(model.HedefSubeId))
                    {
                        var tumSubeYayIsSelected = model.IsSelectedSubeList.Where(x => x.IsSelectedHedefSube == true).Select(x => x.IsSelectedHazirlananSubeList).Any();
                        if (!tumSubeYayIsSelected)
                        {
                            return SendWarningAfterRedirect("Lütfen fiyat güncellemesi tamamlanmış bir tane şube seçiniz.(Tüm şubelere yaymak için seçili bir şube olmalıdır.)", "GetSubeListInsertSubePruduct", "SefimPanelVeriGonderimi");
                        }
                    }
                    else if (SubeIdGrupList != "TumSubelereYay")
                    {
                        return SendWarningAfterRedirect("Lütfen fiyat güncellemesi tamamlanmış şube seçiniz.", "GetSubeListInsertSubePruduct", "SefimPanelVeriGonderimi");
                    }
                }

                model.SubeIdGrupList = SubeIdGrupList;
                var result = new SPosSubeFiyatGuncellemeCRUD().IsUpdateProduct2(model, kullaniciId);

                if (result.ErrorList != null && result.ErrorList.Count > 0)
                {
                    foreach (var messages in result.ErrorList)
                    {
                        ModelState.AddModelError("", messages);
                    }
                }

                //if (result.IsSuccess)
                //{                   
                //    return SendNotificationAfterRedirect("İşlem Başarılı", "GetSubeListInsertSubePruduct", "SefimPanelVeriGonderimi");
                //}            
                //ModelState.AddModelError("", result.UserMessage);

                var resultList = new SPosSubeFiyatGuncellemeCRUD().GetLocalSubeListIsSelectedRemovePreviousList(kullaniciId);
                resultList.SuccessAlertList = result.SuccessAlertList;
                return View(resultList);
            }

            #endregion  Hedef şubelerde fiyat güncellemesi yapar


            if (model.IsSelectedSubeList == null)
            {
                return SendWarningAfterRedirect("Lütfen fiyat güncellemesi yapılacak hedef şube seçiniz.", "GetSubeListInsertSubePruduct", "SefimPanelVeriGonderimi");
            }

            var isSelectedKaynakSube = model.IsSelectedSubeList.Where(x => x.IsSelectedKaynakSube);
            var isSelectedHedefSube = model.IsSelectedSubeList.Where(x => x.IsSelectedHedefSube);
            var subeName = model.IsSelectedSubeList.FirstOrDefault().SubeName;
            if (isSelectedHedefSube == null || isSelectedHedefSube.Count() == 0 || subeName == null)
            {
                ModelState.AddModelError(string.Empty, "Lütfen fiyat güncellemesi yapılacak hedef şube seçiniz.");
            }

            if (ModelState.IsValid)
            {
                var result = new SPosSubeFiyatGuncellemeCRUD().SubeProductInsertLocalTable2(model);
                if (result.IsSuccess)
                {
                    //return SendNotificationAfterRedirect(result.UserMessage, "EditSubeProduct", "SefimPanelVeriGonderimi");
                    return SendNotificationAfterRedirect(result.UserMessage, "GetSubeListInsertSubePruduct", "SefimPanelVeriGonderimi");
                }

                ModelState.AddModelError("", result.UserMessage);
            }

            //var resultList = new SPosSubeFiyatGuncellemeCRUD().GetSubeListIsSelected();
            return View(model);
        }

        #region Prpduct update       
        [HttpGet]
        public ActionResult EditSubeProduct(string productGroup, string subeIdGrupList)
        {
            var result = new SPosSubeFiyatGuncellemeCRUD().GetByProductForSubeListEdit(productGroup, subeIdGrupList);
            if (!result.IsSuccess)
            {
                SendWarning(result.ErrorList.FirstOrDefault());
            }

            ViewBag.SubeName = result.SubeAdi;
            result.SubeIdGrupList = subeIdGrupList;
            result.SubeyeGoreUrunGrubuComboList = productGroup;

            return View(result);
        }

        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult EditSubeProduct(UrunEditViewModel model)
        {
            model.KullaniciId = Request.Cookies["PRAUT"].Value;
            //TODO eğer bir değer doluysa insert ya da updsate yapılabilir, null ise delete işlemi, yapılabilir.
            //if (ModelState.IsValid)
            //{
            var result = new SPosSubeFiyatGuncellemeCRUD().UpdateByProductForSubeList(model);
            if (result.IsSuccess)
            {
                //return View(model);
                //return SendNotificationAfterRedirect(result.UserMessage, "FiyatGuncelleEdit", "SPosVeriGonderimi", new { subeId = model.SubeId });
                return SendNotificationAfterRedirect(result.UserMessage, "EditSubeProduct", "SefimPanelVeriGonderimi", new { productGroup = model.SubeyeGoreUrunGrubuComboList, SubeIdGrupList = model.SubeIdGrupList });
            }

            ModelState.AddModelError("", result.UserMessage);
            //}

            return View(model);
        }

        #endregion Prpduct update

        #region Choice1 update
        [HttpGet]
        public ActionResult Choice1EditForSubeList(string subeId, string productId, string subeIdGrupList)
        {
            var result = new SPosSubeFiyatGuncellemeCRUD().GetByChoice1ForSubeListEdit(subeId, productId, subeIdGrupList);
            if (!result.IsSuccess)
            {
                SendWarning(result.ErrorList.FirstOrDefault());
            }

            ViewBag.SubeName = result.SubeAdi;
            result.SubeIdGrupList = subeIdGrupList;

            return View(result);
        }

        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult Choice1EditForSubeList(UrunEditViewModel model)
        {
            model.KullaniciId = Request.Cookies["PRAUT"].Value;
            var result = new SPosSubeFiyatGuncellemeCRUD().UpdateByChoice1ForSubeList(model);
            if (result.IsSuccess)
            {
                SendNotification(result.UserMessage);
                model = new SPosSubeFiyatGuncellemeCRUD().GetByChoice1ForSubeListEdit(model.productCompairsList.FirstOrDefault().ProductGroup, model.productCompairsList.FirstOrDefault().ProductName, model.SubeIdGrupList);

                return View(model);
            }

            return View(model);
        }

        #endregion Choice1 update


        #region Choice2 update
        [HttpGet]
        public ActionResult Choice2EditForSubeList(int subeId, int choice1Id, string subeIdGrupList, string choice1Name)
        {
            var result = new SPosSubeFiyatGuncellemeCRUD().GetByChoice2ForSubeListEdit(subeId, choice1Id, subeIdGrupList, choice1Name);
            if (!result.IsSuccess)
            {
                SendWarning(result.ErrorList.FirstOrDefault());
            }

            ViewBag.SubeName = result.SubeAdi;
            result.SubeIdGrupList = subeIdGrupList;
            result.Choice1ProductName = choice1Name;

            return View(result);
        }

        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult Choice2EditForSubeList(UrunEditViewModel model)
        {
            var subeIdGrupList_ = model.SubeIdGrupList;
            var choice1ProductName_ = model.Choice1ProductName;
            model.KullaniciId = Request.Cookies["PRAUT"].Value;
            var result = new SPosSubeFiyatGuncellemeCRUD().UpdateByChoice2ForSubeList(model);
            if (result.IsSuccess)
            {
                SendNotification(result.UserMessage);
                model = new SPosSubeFiyatGuncellemeCRUD().GetByChoice2ForSubeListEdit(
                                                                                        model.SubeId,
                                                                                        model.productCompairsList.FirstOrDefault().Choice1Id,
                                                                                        model.SubeIdGrupList,
                                                                                        model.Choice1ProductName
                                                                                      );

                return SendNotificationAfterRedirect(result.UserMessage, "Choice2EditForSubeList", "SefimPanelVeriGonderimi",
                                                        new
                                                        {
                                                            subeId = model.SubeId,
                                                            choice1Id = model.productCompairsList.FirstOrDefault().Choice1Id,
                                                            subeIdGrupList = subeIdGrupList_,
                                                            choice1Name = choice1ProductName_
                                                        });
            }

            ModelState.AddModelError("", result.UserMessage);

            return View(model);
        }

        #endregion Choice2 update


        #region Options update
        [HttpGet]
        public ActionResult OptionsEditForSubeList(string subeId, string productId, string subeIdGrupList)
        {
            var result = new SPosSubeFiyatGuncellemeCRUD().GetByChoice1ForSubeListEdit(subeId, productId, subeIdGrupList);
            if (!result.IsSuccess)
            {
                SendWarning(result.ErrorList.FirstOrDefault());
            }

            ViewBag.SubeName = result.SubeAdi;
            result.SubeIdGrupList = subeIdGrupList;

            return View(result);
        }

        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult OptionsEditForSubeList(UrunEditViewModel model)
        {
            var subeIdGrupList_ = model.SubeIdGrupList;
            model.KullaniciId = Request.Cookies["PRAUT"].Value;
            var result = new SPosSubeFiyatGuncellemeCRUD().UpdateByOptionsForSubeList(model, false, model.SubeIdGrupList);
            if (result.IsSuccess)
            {
                model = new SPosSubeFiyatGuncellemeCRUD().GetByChoice1ForSubeListEdit(model.productOptionsCompairsList.FirstOrDefault().ProductGroup, model.productOptionsCompairsList.FirstOrDefault().ProductName, model.SubeIdGrupList);
                return SendNotificationAfterRedirect(result.UserMessage, "Choice1EditForSubeList", "SefimPanelVeriGonderimi",
                    new
                    {
                        subeId = model.productOptionsCompairsList.FirstOrDefault().ProductGroup,
                        productId = model.productOptionsCompairsList.FirstOrDefault().ProductName,
                        subeIdGrupList = subeIdGrupList_
                    });
            }

            ModelState.AddModelError("", result.UserMessage);

            return View(model);
        }

        #endregion Options



        #region Ortak ürün güncelleme
        [HttpGet]
        public ActionResult SubeCommonProductEdit(string productGroup, string productName, string subeIdGrupList)
        {
            var model = new SPosSubeFiyatGuncellemeCRUD().GetByOrtakUrunForSubeListEdit(productGroup, productName, subeIdGrupList);

            ViewBag.SubeName = model.SubeAdi;
            model.SubeIdGrupList = subeIdGrupList;
            return View(model);
        }

        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult SubeCommonProductEdit(UrunEditViewModel model)
        {
            model.KullaniciId = Request.Cookies["PRAUT"].Value;
            //TODO eğer bir değer doluysa insert ya da updsate yapılabilir, null ise delete işlemi, yapılabilir.
            //if (ModelState.IsValid)
            //{
            var result = new SPosSubeFiyatGuncellemeCRUD().UpdateByProductForSubeList(model);
            if (result.IsSuccess)
            {
                //return View(model);
                //return SendNotificationAfterRedirect(result.UserMessage, "FiyatGuncelleEdit", "SPosVeriGonderimi", new { subeId = model.SubeId });
                return SendNotificationAfterRedirect(result.UserMessage, "EditSubeProduct", "SefimPanelVeriGonderimi", new { SubeIdGrupList = model.SubeIdGrupList });
            }

            ModelState.AddModelError("", result.UserMessage);
            //}

            return View(model);
        }

        #endregion ortak ürün güncelleme

        #region Ortak options güncelleme
        [HttpGet]
        public ActionResult SubeCommonOptionsEdit(string productGroup, string productName, string optionsName, string subeIdGrupList)
        {
            var model = new SPosSubeFiyatGuncellemeCRUD().GetByCommonOptionsForSubeListEdit(productGroup, productName, optionsName, subeIdGrupList);

            ViewBag.SubeName = model.SubeAdi;
            model.SubeIdGrupList = model.SubeIdGrupList;
            return View(model);
        }

        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult SubeCommonOptionsEdit(UrunEditViewModel model)
        {
            //if (ModelState.IsValid)
            //{
            var SubeIdGrupList_ = model.SubeIdGrupList;
            model.KullaniciId = Request.Cookies["PRAUT"].Value;
            var result = new SPosSubeFiyatGuncellemeCRUD().UpdateByOptionsForSubeList(model, true, model.SubeIdGrupList);
            if (result.IsSuccess)
            {
                model = new SPosSubeFiyatGuncellemeCRUD().GetByChoice1ForSubeListEdit(model.productOptionsCompairsList.FirstOrDefault().ProductGroup, model.productOptionsCompairsList.FirstOrDefault().ProductName, model.SubeIdGrupList);
                return SendNotificationAfterRedirect(result.UserMessage, "Choice1EditForSubeList", "SefimPanelVeriGonderimi",
                    new
                    {
                        subeId = model.productOptionsCompairsList.FirstOrDefault().ProductGroup,
                        productId = model.productOptionsCompairsList.FirstOrDefault().ProductName,
                        subeIdGrupList = SubeIdGrupList_
                    });
                //SendNotification(result.UserMessage);
                //return View(model);
            }

            ModelState.AddModelError("", result.UserMessage);
            //}

            return View(model);
        }

        #endregion ortak options güncelleme


        #region IsUpdate

        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult IsUpdatePrpduct(string SubeIdGrupList)
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

            //TODO eğer bir değer doluysa insert ya da updsate yapılabilir, null ise delete işlemi, yapılabilir.
            if (ModelState.IsValid)
            {
                var result = new SPosSubeFiyatGuncellemeCRUD().IsUpdateProduct(SubeIdGrupList, kullaniciId);
                if (result.IsSuccess)
                {
                    return SendNotificationAfterRedirect(result.UserMessage, "EditSubeProduct", "SefimPanelVeriGonderimi");
                }

                ModelState.AddModelError("", result.UserMessage);
            }

            var model = new SPosSubeFiyatGuncellemeCRUD().GetSubeListIsSelected();
            return View(model);
        }
        #endregion IsUpdate 

        #endregion ** (05.02.2022 toplantı sonrası) Şubelerdeki ürünleri local db'ye aktarma ve dinamik olarak şubeleri sayfada gösterme **

        #region  Şube ürün Grubu Listesi alınıyor

        [HttpPost]
        public JsonResult SubeUrunGrubuListJson(int subeId)
        {
            return Json(new SPosSubeFiyatGuncellemeCRUD().SubeUrunGrubuListJson(subeId).ToList(), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult LocalSubeUrunGrubuListJson()
        {
            return Json(new SPosSubeFiyatGuncellemeCRUD().LocalSubeUrunGrubuListJson().ToList(), JsonRequestBehavior.AllowGet);
        }

        //Şablon Ürün grup filter.
        [HttpPost]
        public JsonResult LocalSubeSablonUrunGrubuListJson()
        {
            return Json(new SPosSubeFiyatGuncellemeCRUD().LocalSubeSablonUrunGrubuListJson().ToList(), JsonRequestBehavior.AllowGet);
        }

        #endregion Şube ürün Grubu Listesi alınıyor


        #region Delete hazırlanan ürünler
        [HttpPost]
        public JsonResult Delete(string SubeIdGrupList, string IsSelectedSubeList)
        {
            var result = new SPosSubeFiyatGuncellemeCRUD().DeleteTempProducData(SubeIdGrupList);
            return Json((result), JsonRequestBehavior.AllowGet);
        }


        //[HttpPost]
        public ActionResult DeleteSube(string subeIdGrupList, string subeId, string subeAdiGrupList, string subeName)
        {
            var result = new SPosSubeFiyatGuncellemeCRUD().DeleteForSubeId(subeIdGrupList, subeId, subeAdiGrupList, subeName);
            if (result.IsSuccess)
            {
                return SendNotificationAfterRedirect(result.UserMessage, "GetSubeListInsertSubePruduct", "SefimPanelVeriGonderimi");
            }

            ModelState.AddModelError("", result.UserMessage);

            return SendErrorAfterRedirect(result.UserMessage, "GetSubeListInsertSubePruduct", "SefimPanelVeriGonderimi");
        }
        #endregion
    }
}