using SefimV2.Models;
using SefimV2.Models.SefimPanelBelgeCRUD;
using SefimV2.Repository;
using SefimV2.ViewModels.SPosKabulIslemleri;
using SefimV2.ViewModels.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using static SefimV2.Enums.General;

namespace SefimV2.Controllers
{
    [CheckLoggedIn]
    public class SefimPanelBelgeIslemleriController : BaseController
    {
        // GET: SefimPanelBelgeIslemleri
        //public ActionResult Index()
        //{
        //    BelgeAlisGiderCreate model = new BelgeAlisGiderCreate();
        //    model.Tarih = DateTime.Now;
        //    return View(model);
        //}
        public ActionResult AlisBelgesiCreate()
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

            //ModelState.AddModelError("DepoID", "Mesaj");

            //var yetki= bus

            ViewBag.ActionTitle = "Belge";
            ViewBag.Controller = "AlisBelgesiCreate";
            ViewBag.PageTitle = "Alış Belgesi";
            string BelgeID = HttpContext.Request.Params["Id"];
            string Page = HttpContext.Request.Params["Page"];
            if (((Enums.General.BelgeTipi)Convert.ToInt32(HttpContext.Request.Params["BelgeTip"])) > 0 || BelgeID != null)
            {
                if (BelgeID != null)
                {
                    BelgeAlisGiderCreate blg;
                    if (Convert.ToInt32(BelgeID) > 0)
                    {
                        blg = new AlisBelgesiCRUD().GetBelge(BelgeID);
                        blg.Page = Page;
                    }
                    else
                    {
                        string BelgeNo = HttpContext.Request.Params["BelgeNo"];
                        UserViewModel us = new UserCRUD().GetUserForSubeSettings(ID);
                        BelgeID = new AlisBelgesiCRUD().GetArctosBelgeAndCreate(BelgeNo, us);
                        blg = new AlisBelgesiCRUD().GetBelge(BelgeID);
                        blg.Page = Page;
                    }

                    return View(blg);
                }
                else
                {
                    BelgeAlisGiderCreate blg = new BelgeAlisGiderCreate();
                    blg.BelgeTip = ((Enums.General.BelgeTipi)Convert.ToInt32(HttpContext.Request.Params["BelgeTip"]));
                    blg.BelgeHarekets = new List<BelgeHareket>();
                    blg.Page = Page;
                    return View(blg);
                }
            }
            else
            {
                return SendWarningAfterRedirect("Lütfen geçerli bir belge seçiniz.", "AlisBelgesiList", "SefimPanelBelgeIslemleri");
            }
        }

        public ActionResult BelgeStok()
        {
            ViewBag.ActionTitle = "Belge";
            ViewBag.Controller = "AlisBelgesiCreate";
            ViewBag.PageTitle = "Stok İşlemleri";

            return View();
        }

        [HttpPost]
        public JsonResult AlisBelgesiGetBelge(int BelgeId)
        {
            BelgeAlisGiderCreate result = new AlisBelgesiCRUD().GetBelge(BelgeId.ToString());
            return Json((result), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult AlisBelgesiGetAltCari(int CariId)
        {
            string ID = Request.Cookies["PRAUT"].Value;
            List<SelectListItem> result = new AlisBelgesiCRUD().GetAltCari(CariId.ToString(), new UserCRUD().GetUserForSubeSettings(ID).FirmaID.ToString());
            return Json((result), JsonRequestBehavior.AllowGet);
        }


        #region  Sube Listesi Bilgileri alınıyor
        [HttpPost]
        public JsonResult SubeListJson()
        {
            string ID = Request.Cookies["PRAUT"].Value;
            List<SelectListItem> items = new List<SelectListItem>();
            UserViewModel us = new UserCRUD().GetUserForSubeSettings(ID);
            items = AlisBelgesiCRUD.GetSubeList(us.FirmaID, us.FasterSubeID);//generic alınmalı

            return Json((items).ToList(), JsonRequestBehavior.AllowGet);
        }
        #endregion Sube Listesi Bilgileri alınıyor

        #region  Depo Listesi Bilgileri alınıyor
        [HttpPost]
        public JsonResult DepoListJson()
        {
            string ID = Request.Cookies["PRAUT"].Value;
            List<SelectListItem> items = new List<SelectListItem>();
            items = AlisBelgesiCRUD.GetDepoList(new UserCRUD().GetUserForSubeSettings(ID).FirmaID);//generic alınmalı
            return Json((items).ToList(), JsonRequestBehavior.AllowGet);
        }
        #endregion Depo Listesi Bilgileri alınıyor

        #region  Depoya göre Zimmet Bilgileri alınıyor
        [HttpPost]
        public JsonResult DepoForCariJson()
        {
            string ID = Request.Cookies["PRAUT"].Value;
            List<SelectListItem> items = new List<SelectListItem>();
            var user = new UserCRUD().GetUserForSubeSettings(ID);
            items = AlisBelgesiCRUD.GetDepoForCari(user.FirmaID, user.DepoID);//generic alınmalı
            return Json((items).ToList(), JsonRequestBehavior.AllowGet);
        }
        #endregion Depo Listesi Bilgileri alınıyor

        #region Kasa Listesi Bilgileri alınıyor
        [HttpPost]
        public JsonResult KasaSelectListJson()
        {
            string ID = Request.Cookies["PRAUT"].Value;
            List<SelectListItem> items = new List<SelectListItem>();
            items = AlisBelgesiCRUD.GetKasaList(new UserCRUD().GetUserForSubeSettings(ID).FirmaID);//generic alınmalı
            return Json((items).ToList(), JsonRequestBehavior.AllowGet);
        }
        #endregion Kasa Listesi Bilgileri alınıyor


        #region Sablon Listesi Bilgileri alınıyor
        [HttpPost]
        public JsonResult SablonSelectListJson()
        {
            string ID = Request.Cookies["PRAUT"].Value;
            List<SelectListItem> items = new List<SelectListItem>();
            items = AlisBelgesiCRUD.GetSablonList(BelgeTipi.SayimSablon);
            return Json((items).ToList(), JsonRequestBehavior.AllowGet);
        }
        #endregion Sablon Listesi Bilgileri alınıyor


        #region  Cari Listesi Bilgileri alınıyor
        [HttpPost]
        public JsonResult CariListJson(int BelgeTip)
        {
            string ID = Request.Cookies["PRAUT"].Value;
            List<SelectListItem> items = new List<SelectListItem>();
            UserViewModel us = new UserCRUD().GetUserForSubeSettings(ID);
            items = AlisBelgesiCRUD.GetCariList(us.FirmaID, BelgeTip, us.SefimPanelZimmetCagrisi);//generic alınmalı
            if (((BelgeTipi)BelgeTip) == BelgeTipi.TalepBelgesi)
            {
                if (items.Count > 0)
                {
                    foreach (var item in items)
                    {
                        if (item.Value == us.SefimPanelZimmetCagrisi)
                        {
                            items[0].Selected = true;
                        }
                    }

                }

            }
            return Json((items).ToList(), JsonRequestBehavior.AllowGet);
        }
        #endregion Cari Listesi Bilgileri alınıyor

        #region Özel Kodlar ComboBox Listesi alınıyor
        [HttpPost]
        public JsonResult BelgeOzelKod1Json()
        {
            string ID = Request.Cookies["PRAUT"].Value;
            List<SelectListItem> items = new List<SelectListItem>();
            UserViewModel us = new UserCRUD().GetUserForSubeSettings(ID);
            items = AlisBelgesiCRUD.GetOzelKod1List(us.FirmaID);//generic alınmalı
            return Json((items).ToList(), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult BelgeOzelKod2Json()
        {
            string ID = Request.Cookies["PRAUT"].Value;
            List<SelectListItem> items = new List<SelectListItem>();
            UserViewModel us = new UserCRUD().GetUserForSubeSettings(ID);
            items = AlisBelgesiCRUD.GetOzelKod2List(us.FirmaID);//generic alınmalı
            return Json((items).ToList(), JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public JsonResult BelgeOzelKod3Json()
        {
            string ID = Request.Cookies["PRAUT"].Value;
            List<SelectListItem> items = new List<SelectListItem>();
            UserViewModel us = new UserCRUD().GetUserForSubeSettings(ID);
            items = AlisBelgesiCRUD.GetOzelKod3List(us.FirmaID);//generic alınmalı
            return Json((items).ToList(), JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public JsonResult BelgeOzelKod4Json()
        {
            string ID = Request.Cookies["PRAUT"].Value;
            List<SelectListItem> items = new List<SelectListItem>();
            UserViewModel us = new UserCRUD().GetUserForSubeSettings(ID);
            items = AlisBelgesiCRUD.GetOzelKod4List(us.FirmaID);//generic alınmalı
            return Json((items).ToList(), JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public JsonResult BelgeOzelKod5Json()
        {
            string ID = Request.Cookies["PRAUT"].Value;
            List<SelectListItem> items = new List<SelectListItem>();
            UserViewModel us = new UserCRUD().GetUserForSubeSettings(ID);
            items = AlisBelgesiCRUD.GetOzelKod5List(us.FirmaID);//generic alınmalı
            return Json((items).ToList(), JsonRequestBehavior.AllowGet);
        }
        #endregion Özel Kodlar ComboBox Listesi alınıyor

        [HttpPost]
        public JsonResult StokSelectListJson()
        {
            string ID = Request.Cookies["PRAUT"].Value;
            List<SelectListItem> items = new List<SelectListItem>();
            items = AlisBelgesiCRUD.GetStokSelectList(new UserCRUD().GetUserForSubeSettings(ID).FirmaID);//generic alınmalı            
            return Json((items).ToList(), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult StokSelectListJsonUrunEkle(int SubeId)
        {
            string ID = Request.Cookies["PRAUT"].Value;
            List<SelectListItem> items = new List<SelectListItem>();
            //items = AlisBelgesiCRUD.GetStokSelectList(new UserCRUD().GetUserForSubeSettings(ID).FirmaID);//generic alınmalı
            items = AlisBelgesiCRUD.GetStokSelectList(new UserCRUD().GetUserForSubeSettings2Yeni(SubeId.ToString()).FirmaID);//generic alınmalı
            return Json((items).ToList(), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult StokSelectListHizmetJson()
        {
            string ID = Request.Cookies["PRAUT"].Value;
            List<SelectListItem> items = new List<SelectListItem>();
            items = AlisBelgesiCRUD.GetStokSelectHizmetList(new UserCRUD().GetUserForSubeSettings(ID).FirmaID);//generic alınmalı
            return Json((items).ToList(), JsonRequestBehavior.AllowGet);
        }


        [HttpPost]
        public JsonResult AlisBelgesiDelete(int BelgeId)
        {
            string ID = Request.Cookies["PRAUT"].Value;
            ActionResultMessages result = new AlisBelgesiCRUD().Delete(BelgeId);
            return Json((result), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult AlisBelgesiOnay(int BelgeId)
        {
            string ID = Request.Cookies["PRAUT"].Value;
            ActionResultMessages result = new AlisBelgesiCRUD().Onay(BelgeId, Convert.ToInt32(ID), new UserCRUD().GetUserForSubeSettings(ID));
            return Json((result), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult AlisBelgesiRet(int BelgeId)
        {
            string ID = Request.Cookies["PRAUT"].Value;
            ActionResultMessages result = new AlisBelgesiCRUD().Ret(BelgeId, Convert.ToInt32(ID));
            return Json((result), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult AlisBelgesiCreateInsert(BelgeAlisGiderCreate obj)
        {
            string ID = Request.Cookies["PRAUT"].Value;
            UserViewModel us = new UserCRUD().GetUserForSubeSettings(ID);
            foreach (var item in obj.BelgeHarekets)
            {
                item.SayimTarihDahil = us.BelgeSayimTarihDahil;
            }
            if (obj.Id > 0)
            {
                var result = new AlisBelgesiCRUD().Update(obj);

                if (result.IsSuccess)
                {
                    var ret = SendNotificationAfterAjaxRedirect(result.UserMessage, "AlisBelgesiList", "SefimPanelBelgeIslemleri");
                    return Json(ret, JsonRequestBehavior.AllowGet);
                }
                ModelState.AddModelError("", result.UserMessage);
            }
            else
            {
                var result = new AlisBelgesiCRUD().Insert(obj, us);
                if (result.IsSuccess)
                {
                    var ret = SendNotificationAfterAjaxRedirect(result.UserMessage, "AlisBelgesiList", "SefimPanelBelgeIslemleri");
                    return Json(ret, JsonRequestBehavior.AllowGet);
                }

                ModelState.AddModelError(result.UserMessage, result.UserMessage);
            }

            return Json(obj, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult StokDetayListJson(int StokId, int Cari)
        {
            string ID = Request.Cookies["PRAUT"].Value;
            List<SefimPanelStok> items = new List<SefimPanelStok>();
            int ZimFiyat = new UserCRUD().GetZimFiyat(ID, Cari);
            items = AlisBelgesiCRUD.GetStokDetayList(new UserCRUD().GetUserForSubeSettings(ID).FirmaID, StokId, ZimFiyat);//generic alınmalı
            return Json((items).ToList(), JsonRequestBehavior.AllowGet);
        }


        [HttpGet]
        public JsonResult StokDetayListJson2Yeni(int StokId, int Cari, int SubeId)
        {
            string ID = Request.Cookies["PRAUT"].Value;
            List<SefimPanelStok> items = new List<SefimPanelStok>();
            int ZimFiyat = new UserCRUD().GetZimFiyat(ID, Cari);
            items = AlisBelgesiCRUD.GetStokDetayList(new UserCRUD().GetUserForSubeSettings2Yeni(SubeId.ToString()).FirmaID, StokId, ZimFiyat);//generic alınmalı
            return Json((items).ToList(), JsonRequestBehavior.AllowGet);
        }


        public ActionResult AlisBelgesiList(string baslangictarihi = "", string bitistarihi = "", string belgetipi = "", string sube = "", string onaydurumu = "")
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

            ViewBag.ActionTitle = "Belge";
            ViewBag.Controller = "AlisBelgesiCreate";
            ViewBag.PageTitle = "Alış Belgesi";

            BelgeList blglst = new AlisBelgesiCRUD().BelgeList(baslangictarihi, bitistarihi, belgetipi, sube, onaydurumu);
            blglst.BaslangicTarihi = baslangictarihi;
            blglst.BitisTarihi = bitistarihi;
            blglst.Sube = sube;
            return View(blglst);
        }


        public ActionResult AlisBelgesiSablonList()
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

            ViewBag.ActionTitle = "Belge";
            ViewBag.Controller = "AlisBelgesiCreate";
            ViewBag.PageTitle = "Alış Belgesi";

            BelgeList blglst = new AlisBelgesiCRUD().BelgeSablonList(BelgeTipi.SayimSablon);
            return View(blglst);
        }
        public ActionResult OnayBekleyenBelgeList(string baslangictarihi = "", string bitistarihi = "", string belgetipi = "", string sube = "")
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

            ViewBag.ActionTitle = "Belge";
            ViewBag.Controller = "AlisBelgesiCreate";
            ViewBag.PageTitle = "Alış Belgesi";


            UserViewModel us = new UserCRUD().GetUserForSubeSettings(ID);

            BelgeList blglst = new AlisBelgesiCRUD().OnayBekleyenBelgeList(us.BelgeTipYetkisi, baslangictarihi, bitistarihi, belgetipi, sube);
            blglst.BaslangicTarihi = baslangictarihi;
            blglst.BitisTarihi = bitistarihi;
            blglst.Sube = sube;
            return View(blglst);
        }

    }
}