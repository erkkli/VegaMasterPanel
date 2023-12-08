using SefimV2.Models;
using SefimV2.Models.ProductSefimCRUD;
using SefimV2.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace SefimV2.Controllers
{
    [CheckLoggedIn]
    public class SefimPanelUrunEkleController : BaseController
    {
        // GET: Product

        [HttpPost]
        public JsonResult ProductInsert(SefimPanelUrunEkleViewModel Product)
        {
            var result =new  ActionResultMessages();
            if (Product.Id > 0)
            {
                 result = new SefimPanelUrunEkleCRUD().UpdateProduct(Product);
            }
            else
            {
                 result = new SefimPanelUrunEkleCRUD().Insert(Product);
            }
          
            if (result.IsSuccess)
            {
                var mesaj = SendNotificationAfterAjaxRedirect(result.UserMessage, "ProductList", "SefimPanelUrunEkle");
                return Json((result), JsonRequestBehavior.AllowGet);
            }
            return Json((result), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult ReceteInsert(List<Bom> bomList, List<BomOptions> bomOptionsList, bool yeniUrunMu)
        {
            var result = new SefimPanelUrunEkleCRUD().InsertRecete(bomList, bomOptionsList, yeniUrunMu);
            if (result.IsSuccess)
            {
                var mesaj = SendNotificationAfterAjaxRedirect(result.UserMessage, "ProductList", "SefimPanelUrunEkle");
                return Json((mesaj), JsonRequestBehavior.AllowGet);
            }
            return Json((result), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult ProductOptionsJson(int Id, int SubeId)
        {
            List<SelectListItem> items = new List<SelectListItem>();
            items = SefimPanelUrunEkleCRUD.GetProductOptionsList(Id, SubeId);//generic alınmalı
            return Json((items).ToList(), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult ProductDelete(int Id, int SubeId)
        {
            var result = new SefimPanelUrunEkleCRUD().Delete(Id, SubeId);
            return Json((result), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult ProductCopy(int Id, List<SelectListItem> SubeList, int SubeId, bool YeniUrunMu)
        {
            #region YETKİLİ ID (Left Menude filtrele de yapıyorum)    
            bool cookieExists = HttpContext.Request.Cookies["PRAUT"] != null;
            if (cookieExists == false)
            {
                return Json("/Authentication/Login", JsonRequestBehavior.AllowGet);

            }
            string kullaniciId = Request.Cookies["PRAUT"].Value;
            ViewBag.YetkiliID = kullaniciId;
            #endregion

            var result = new SefimPanelUrunEkleCRUD().Copy(Id, SubeList, SubeId, YeniUrunMu, kullaniciId);
            return Json((result), JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public JsonResult ProductsCopy(List<SelectedProductItem> ProductList, List<SelectListItem> SubeList)
        {
            #region YETKİLİ ID (Left Menude filtrele de yapıyorum)    
            bool cookieExists = HttpContext.Request.Cookies["PRAUT"] != null;
            if (cookieExists == false)
            {
                return Json("/Authentication/Login", JsonRequestBehavior.AllowGet);

            }
            string kullaniciId = Request.Cookies["PRAUT"].Value;
            ViewBag.YetkiliID = kullaniciId;
            #endregion
            ActionResultMessages result = new ActionResultMessages();
            foreach (var item in ProductList)
            {
                result = new SefimPanelUrunEkleCRUD().Copy(item.Id, SubeList, item.SubeId, item.YeniUrunMu, kullaniciId);
            }

            return Json((result), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult AktifPasif(int Id, int SubeId, bool YeniUrunMu, bool Aktif)
        {
            #region YETKİLİ ID (Left Menude filtrele de yapıyorum)    
            bool cookieExists = HttpContext.Request.Cookies["PRAUT"] != null;
            if (cookieExists == false)
            {
                return Json("/Authentication/Login", JsonRequestBehavior.AllowGet);

            }
            string kullaniciId = Request.Cookies["PRAUT"].Value;
            ViewBag.YetkiliID = kullaniciId;
            #endregion

            var result = new SefimPanelUrunEkleCRUD().AktifPasif(Id, SubeId, YeniUrunMu, Aktif, kullaniciId);
            return Json((result), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult ReceteCopyForSube(List<SelectListItem> MainSubeList, List<SelectListItem> SubeList)
        {
            var result = new SefimPanelUrunEkleCRUD().CopyForSube(MainSubeList, SubeList);
            return Json((result), JsonRequestBehavior.AllowGet);
        }

        public ActionResult ProductList(string SubeIds)
        {
            List<SefimPanelUrunEkleViewModel> lst = new List<SefimPanelUrunEkleViewModel>();
            try
            {
                if (!string.IsNullOrEmpty(SubeIds))
                {
                    lst = SefimPanelUrunEkleCRUD.ProductList(SubeIds);
                }
            }
            catch (Exception ex)
            {
                lst = new List<SefimPanelUrunEkleViewModel>();
            }

            return View(lst);
        }

        [HttpPost]
        public JsonResult GetProduct(int Id)
        {
            SefimPanelUrunEkleViewModel obj = SefimPanelUrunEkleCRUD.GetProduct(Id);
            return Json((obj), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult GetProductRemote(int Id, int SubeId)
        {
            SefimPanelUrunEkleViewModel obj = SefimPanelUrunEkleCRUD.GetProductRemote(Id, SubeId);
            return Json((obj), JsonRequestBehavior.AllowGet);
        }


        [HttpPost]
        public JsonResult GetProductForSube(int Id, int SubeId)
        {
            #region YETKİLİ ID (Left Menude filtrele de yapıyorum)    
            bool cookieExists = HttpContext.Request.Cookies["PRAUT"] != null;
            if (cookieExists == false)
            {
                return Json("/Authentication/Login", JsonRequestBehavior.AllowGet);

            }
            string kullaniciId = Request.Cookies["PRAUT"].Value;
            ViewBag.YetkiliID = kullaniciId;
            #endregion

            SefimPanelUrunEkleViewModel obj = new SefimPanelUrunEkleCRUD().GetProductForSube(Id, SubeId, kullaniciId);
            return Json((obj), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult SendProductSube()
        {
            var result = new SefimPanelUrunEkleCRUD().IsInsertProduct();

            if (result.IsSuccess)
            {
                var mesaj = SendNotificationAfterAjaxRedirect(result.UserMessage, "ProductList", "SefimPanelUrunEkle");
                return Json((mesaj), JsonRequestBehavior.AllowGet);
            }
            SendError(result.UserMessage);
            return Json((result), JsonRequestBehavior.AllowGet);
        }

        public ActionResult UpdatedProductList(string SubeIds)
        {
            List<SefimPanelUrunEkleViewModel> lst = new List<SefimPanelUrunEkleViewModel>();
            try
            {
                if (!string.IsNullOrEmpty(SubeIds))
                {
                    lst = UrunFiyatCRUD.ProductList(SubeIds, true);
                }
            }
            catch (Exception ex)
            {
                lst = new List<SefimPanelUrunEkleViewModel>();
            }

            return View(lst);
        }


        #region  Sube Listesi Bilgileri alınıyor
        [HttpPost]
        public JsonResult SubeListJson()
        {
            string kullaniciId = Request.Cookies["PRAUT"].Value;
            List<SelectListItem> items = new List<SelectListItem>();
            items = SefimPanelUrunEkleCRUD.GetSubeList(kullaniciId);
            return Json(items.ToList(), JsonRequestBehavior.AllowGet);
        }
        #endregion Sube Listesi Bilgileri alınıyor
        #region  Gönderilmeyi Bekleyen Reçete Listesi Bilgileri alınıyor
        [HttpPost]
        public JsonResult SubeSendBomListJson()
        {
            string kullaniciId = Request.Cookies["PRAUT"].Value;
            List<SelectListItem> items = new List<SelectListItem>();
            items = SefimPanelUrunEkleCRUD.GetSubeSendBomList(kullaniciId);
            return Json(items.ToList(), JsonRequestBehavior.AllowGet);
        }
        #endregion Sube Listesi Bilgileri alınıyor

        #region AutoComplete için Listeler çekiliyor
        [HttpPost]
        public JsonResult ProductGroupsJson()
        {
            string ID = Request.Cookies["PRAUT"].Value;
            List<SelectListItem> items = new List<SelectListItem>();
            items = SefimPanelUrunEkleCRUD.GetSubeProductGroup();//generic alınmalı
            //items = items.Distinct().Select(x=>x.Text)..ToList();
            var dd = items.Select(x => x.Text).Distinct().ToList();
            return Json((dd).ToList(), JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public JsonResult ProductTypesJson()
        {
            string ID = Request.Cookies["PRAUT"].Value;
            List<SelectListItem> items = new List<SelectListItem>();
            items = SefimPanelUrunEkleCRUD.GetSubeProductTypes();//generic alınmalı
            var dd = items.Select(x => x.Text).Distinct().ToList();
            return Json((dd).ToList(), JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public JsonResult InvoiceNamesJson()
        {
            string ID = Request.Cookies["PRAUT"].Value;
            List<SelectListItem> items = new List<SelectListItem>();
            items = SefimPanelUrunEkleCRUD.GetSubeInvoiceNames();//generic alınmalı
            var dd = items.Select(x => x.Text).Distinct().ToList();
            return Json((dd).ToList(), JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public JsonResult FavoritessJson()
        {
            string ID = Request.Cookies["PRAUT"].Value;
            List<SelectListItem> items = new List<SelectListItem>();
            items = SefimPanelUrunEkleCRUD.GetSubeFavoritess();//generic alınmalı
            var dd = items.Select(x => x.Text).Distinct().ToList();
            return Json((dd).ToList(), JsonRequestBehavior.AllowGet);
        }
        #endregion  AutoComplete için Listeler çekiliyor
    }
}