using SefimV2.Helper;
using SefimV2.Models;
using SefimV2.Repository;
using SefimV2.ViewModels.Icmal;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Web.Mvc;
using static SefimV2.Enums.General;

namespace SefimV2.Controllers
{
    [CheckLoggedIn]
    public class IcmalController : BaseController
    {
        // GET: Icmal

        #region List       
        public ActionResult List()
        {
            List<IcmalViewModel> resultList;
            try
            {
                #region YETKİLİ ID (Left Menude filtrele de yapıyorum)    
                bool cookieExists = HttpContext.Request.Cookies["PRAUT"] != null;
                if (cookieExists == false)
                {
                    return Redirect("/Authentication/Login");
                }
                var kullaniciId = Request.Cookies["PRAUT"].Value;
                #endregion

                resultList = IcmalCRUD.IcmalSubeList(kullaniciId);
            }
            catch (Exception ex)
            {
                Singleton.WritingLogFile2("CiroRaporlarController", ex.ToString(), "", ex.StackTrace);
                return Redirect("/Authentication/Login");
            }
            return View(resultList);
        }

        public ActionResult IcmalList()
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

            string subeId = string.Empty;
            if (Request.QueryString["subeid"] != null)
            {
                subeId = Request.QueryString["subeid"].ToString();
            }
            var resultList = IcmalCRUD.IcmalList(DateTime.Now, DateTime.Now, kullaniciId, subeId);

            return View(resultList);

        }
        #endregion List         

        [HttpGet]
        public ActionResult Create()
        {
            #region YETKİLİ ID (Left Menude filtrele de yapıyorum)    
            bool cookieExists = HttpContext.Request.Cookies["PRAUT"] != null;
            if (cookieExists == false)
            {
                return Redirect("/Authentication/Login");
            }
            var userId = Request.Cookies["PRAUT"].Value;
            #endregion
            var viewModel = new IcmalViewModel()
            {
                UserId = userId
            };
            var resultList = IcmalCRUD.GetCreatedIcmalList(DateTime.Now, DateTime.Now, viewModel.UserId, "");
            //var userYetki = BussinessHelper.GetByUserIdForYetkiUser(userId);
            resultList.UserId = userId;
            return View(resultList);
        }

        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult Create(IcmalViewModel viewModel)
        {
            viewModel.UserId = Request.Cookies["PRAUT"].Value;

            //ModelState.Clear();
            //TryValidateModel(viewModel);

            if (Request.IsAjaxRequest())
            {
                ModelState.Clear();

                if (viewModel.Islem == "KreditPaymentAdd")
                {
                    if (viewModel.KreditPayment.BankaTipi == null)
                    {
                        ModelState.AddModelError("BankaTipi", "Banka Seçiniz.");
                        return PartialView(viewModel);
                    }

                    if (viewModel.KreditPaymentBankTypeList == null)
                    {
                        viewModel.KreditPaymentBankTypeList = new List<KreditPaymentBank>();
                    }

                    var displayAttribute = viewModel.KreditPayment.BankaTipi.GetType()
                                                .GetMember(viewModel.KreditPayment.BankaTipi.ToString())
                                                .First()
                                                .GetCustomAttribute<DisplayAttribute>();
                    string displayName = displayAttribute?.GetName();
                    viewModel.KreditPayment.BankBkmId = viewModel.KreditPayment.BankaTipi.GetHashCode();
                    viewModel.KreditPayment.BankName = displayName;
                    viewModel.KreditPaymentBankTypeList.Add(viewModel.KreditPayment);
                }
                if (viewModel.Islem == "KreditPaymentUpdate")
                {
                    if (viewModel.KreditPaymentBankTypeList == null)
                    {
                        viewModel.KreditPayment = new KreditPaymentBank();
                    }
                    viewModel.KreditPaymentBankTypeList[viewModel.Index.Value].BankaTipi = viewModel.KreditPayment.BankaTipi;
                    viewModel.KreditPaymentBankTypeList[viewModel.Index.Value].Amount = viewModel.KreditPayment.Amount;
                    viewModel.KreditPaymentBankTypeList[viewModel.Index.Value].BankBkmId = viewModel.KreditPayment.BankBkmId;
                }
                if (viewModel.Islem == "KreditPaymentDelete")
                {
                    if (viewModel.KreditPaymentBankTypeList == null)
                    {
                        viewModel.KreditPaymentBankTypeList = new List<KreditPaymentBank>();
                    }
                    if (viewModel.KreditPaymentBankTypeList != null)
                    {
                        viewModel.KreditPaymentBankTypeList.RemoveAt(viewModel.Index.GetValueOrDefault());
                    }
                }

                //Yemek kartlı satış
                if (viewModel.Islem == "TicketPaymentAdd")
                {
                    if (viewModel.TicketPayment.YemekKartiTipi == null)
                    {
                        ModelState.AddModelError("YemekKartiTipi", "Yemek Kartı Seçiniz.");
                        return PartialView(viewModel);
                    }

                    if (viewModel.TicketPaymentTicketTypeList == null)
                    {
                        viewModel.TicketPaymentTicketTypeList = new List<TicketPaymentTicketTypeList>();
                    }
                    var displayAttribute = viewModel.TicketPayment.YemekKartiTipi.GetType()
                                              .GetMember(viewModel.TicketPayment.YemekKartiTipi.ToString())
                                              .First()
                                              .GetCustomAttribute<DisplayAttribute>();
                    string displayName = displayAttribute?.GetName();
                    viewModel.TicketPayment.TicketId = viewModel.TicketPayment.YemekKartiTipi.GetHashCode();
                    viewModel.TicketPayment.TicketName = displayName;
                    viewModel.TicketPaymentTicketTypeList.Add(viewModel.TicketPayment);
                }
                if (viewModel.Islem == "TicketPaymentUpdate")
                {
                    if (viewModel.TicketPaymentTicketTypeList == null)
                    {
                        viewModel.TicketPayment = new TicketPaymentTicketTypeList();
                    }
                    viewModel.TicketPaymentTicketTypeList[viewModel.Index.Value].YemekKartiTipi = viewModel.TicketPayment.YemekKartiTipi;
                    viewModel.TicketPaymentTicketTypeList[viewModel.Index.Value].Amount = viewModel.TicketPayment.Amount;
                    viewModel.TicketPaymentTicketTypeList[viewModel.Index.Value].TicketId = viewModel.TicketPayment.TicketId;
                }
                if (viewModel.Islem == "TicketPaymentDelete")
                {
                    if (viewModel.TicketPaymentTicketTypeList == null)
                    {
                        viewModel.TicketPaymentTicketTypeList = new List<TicketPaymentTicketTypeList>();
                    }
                    if (viewModel.TicketPaymentTicketTypeList != null)
                    {
                        viewModel.TicketPaymentTicketTypeList.RemoveAt(viewModel.Index.GetValueOrDefault());
                    }
                }


                return PartialView(viewModel);
            }

            if (viewModel.SubeId == "0")
            {
                ModelState.AddModelError("SubeSec", "Lütfen Şube Seçiniz");
            }


            if (ModelState.IsValid)
            {
                var result = new IcmalCRUD().IcmalCreate(viewModel);
                if (result.IsSuccess)
                {
                    return SendNotificationAfterRedirect(result.UserMessage, "Create", "Icmal");
                }

                SendError(result.UserMessage);
            }
            var resultList = IcmalCRUD.GetCreatedIcmalList(DateTime.Now, DateTime.Now, viewModel.UserId, "");

            return View(resultList);
        }

        [HttpGet]
        public ActionResult Update(int id, string subeId)
        {
            #region YETKİLİ ID (Left Menude filtrele de yapıyorum)    
            bool cookieExists = HttpContext.Request.Cookies["PRAUT"] != null;
            if (cookieExists == false)
            {
                return Redirect("/Authentication/Login");
            }
            var userId = Request.Cookies["PRAUT"].Value;
            #endregion
            var viewModel = new IcmalViewModel()
            {
                UserId = userId
            };
            var resultList = IcmalCRUD.GetByIdForIcmalUpdate(id, userId, subeId);
            //var userYetki = BussinessHelper.GetByUserIdForYetkiUser(userId);

            return View(resultList);
        }

        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult Update(IcmalViewModel viewModel)
        {
            var culture = new CultureInfo("tr-Tr");
            culture.NumberFormat.CurrencySymbol = "TL";
            culture.NumberFormat.CurrencyGroupSeparator = ".";
            culture.NumberFormat.CurrencyDecimalSeparator = ",";
            culture.NumberFormat.NumberGroupSeparator = ".";
            culture.NumberFormat.NumberDecimalSeparator = ",";

            viewModel.UserId = Request.Cookies["PRAUT"].Value;

            //ModelState.Clear();
            //TryValidateModel(viewModel);

            if (Request.IsAjaxRequest())
            {
                ModelState.Clear();
                if (viewModel.Islem == "KreditPaymentAdd")
                {
                    if (viewModel.KreditPayment.BankaTipi == null)
                    {
                        ModelState.AddModelError("BankaTipi", "Banka Seçiniz.");
                        return PartialView(viewModel);
                    }

                    if (viewModel.KreditPaymentBankTypeList == null)
                    {
                        viewModel.KreditPaymentBankTypeList = new List<KreditPaymentBank>();
                    }

                    var displayAttribute = viewModel.KreditPayment.BankaTipi.GetType()
                                                .GetMember(viewModel.KreditPayment.BankaTipi.ToString())
                                                .First()
                                                .GetCustomAttribute<DisplayAttribute>();
                    string displayName = displayAttribute?.GetName();
                    viewModel.KreditPayment.BankBkmId = viewModel.KreditPayment.BankaTipi.GetHashCode();
                    viewModel.KreditPayment.BankName = displayName;
                    viewModel.KreditPaymentBankTypeList.Add(viewModel.KreditPayment);
                }
                if (viewModel.Islem == "KreditPaymentUpdate")
                {
                    if (viewModel.KreditPaymentBankTypeList == null)
                    {
                        viewModel.KreditPayment = new KreditPaymentBank();
                    }

                    var displayAttribute = viewModel.KreditPayment.BankaTipi.GetType()
                                             .GetMember(viewModel.KreditPayment.BankaTipi.ToString())
                                             .First()
                                             .GetCustomAttribute<DisplayAttribute>();
                    string displayName = displayAttribute?.GetName();
                    viewModel.KreditPayment.BankBkmId = viewModel.KreditPayment.BankaTipi.GetHashCode();
                    viewModel.KreditPayment.BankName = displayName;

                    viewModel.KreditPaymentBankTypeList[viewModel.Index.Value].BankaTipi = viewModel.KreditPayment.BankaTipi;
                    viewModel.KreditPaymentBankTypeList[viewModel.Index.Value].Amount = viewModel.KreditPayment.Amount;
                    viewModel.KreditPaymentBankTypeList[viewModel.Index.Value].BankBkmId = viewModel.KreditPayment.BankBkmId;
                    viewModel.KreditPaymentBankTypeList[viewModel.Index.Value].BankName = viewModel.KreditPayment.BankName;
                }
                if (viewModel.Islem == "KreditPaymentDelete")
                {
                    if (viewModel.KreditPaymentBankTypeList == null)
                    {
                        viewModel.KreditPaymentBankTypeList = new List<KreditPaymentBank>();
                    }
                    if (viewModel.KreditPaymentBankTypeList != null)
                    {
                        viewModel.KreditPaymentBankTypeList.RemoveAt(viewModel.Index.GetValueOrDefault());
                    }
                }

                //Yemek kartlı satış
                if (viewModel.Islem == "TicketPaymentAdd")
                {
                    if (viewModel.TicketPayment.YemekKartiTipi == null)
                    {
                        ModelState.AddModelError("YemekKartiTipi", "Yemek Kartı Seçiniz.");
                        return PartialView(viewModel);
                    }

                    if (viewModel.TicketPaymentTicketTypeList == null)
                    {
                        viewModel.TicketPaymentTicketTypeList = new List<TicketPaymentTicketTypeList>();
                    }
                    var displayAttribute = viewModel.TicketPayment.YemekKartiTipi.GetType()
                                              .GetMember(viewModel.TicketPayment.YemekKartiTipi.ToString())
                                              .First()
                                              .GetCustomAttribute<DisplayAttribute>();
                    string displayName = displayAttribute?.GetName();
                    viewModel.TicketPayment.TicketId = viewModel.TicketPayment.YemekKartiTipi.GetHashCode();
                    viewModel.TicketPayment.TicketName = displayName;
                    viewModel.TicketPaymentTicketTypeList.Add(viewModel.TicketPayment);
                }
                if (viewModel.Islem == "TicketPaymentUpdate")
                {
                    if (viewModel.TicketPaymentTicketTypeList == null)
                    {
                        viewModel.TicketPayment = new TicketPaymentTicketTypeList();
                    }

                    var displayAttribute = viewModel.TicketPayment.YemekKartiTipi.GetType()
                                              .GetMember(viewModel.TicketPayment.YemekKartiTipi.ToString())
                                              .First()
                                              .GetCustomAttribute<DisplayAttribute>();
                    string displayName = displayAttribute?.GetName();
                    viewModel.TicketPayment.TicketId = viewModel.TicketPayment.YemekKartiTipi.GetHashCode();
                    viewModel.TicketPayment.TicketName = displayName;

                    viewModel.TicketPaymentTicketTypeList[viewModel.Index.Value].YemekKartiTipi = viewModel.TicketPayment.YemekKartiTipi;
                    viewModel.TicketPaymentTicketTypeList[viewModel.Index.Value].Amount = viewModel.TicketPayment.Amount;
                    viewModel.TicketPaymentTicketTypeList[viewModel.Index.Value].TicketId = viewModel.TicketPayment.TicketId;
                    viewModel.TicketPaymentTicketTypeList[viewModel.Index.Value].TicketName = viewModel.TicketPayment.TicketName;
                }
                if (viewModel.Islem == "TicketPaymentDelete")
                {
                    if (viewModel.TicketPaymentTicketTypeList == null)
                    {
                        viewModel.TicketPaymentTicketTypeList = new List<TicketPaymentTicketTypeList>();
                    }
                    if (viewModel.TicketPaymentTicketTypeList != null)
                    {
                        viewModel.TicketPaymentTicketTypeList.RemoveAt(viewModel.Index.GetValueOrDefault());
                    }
                }

                viewModel.KreditPayment = null;
                viewModel.TicketPayment = null;

                return PartialView(viewModel);
            }

            if (viewModel.SubeId == "0")
            {
                ModelState.AddModelError("SubeSec", "Lütfen Şube Seçiniz");
            }
            if (viewModel.OnayDurumu == OnayDurumu.Onayli && viewModel.UserId != "1")
            {
                ModelState.AddModelError("OnayDurumu", "İcmal Onaylı olduğu için güncelleme işlemi yapılamaz.");
            }

            if (ModelState.IsValid)
            {
                var result = new IcmalCRUD().IcmalUpdate(viewModel);
                if (result.IsSuccess)
                {
                    return SendNotificationAfterRedirect(result.UserMessage, "Create", "Icmal");
                }

                SendError(result.UserMessage);
            }

            return View(viewModel);
        }

        public ActionResult IcmalCompare(int id, string subeName, string subeId, string paymentDate)
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

            var result = IcmalCRUD.IcmalCompareList(Convert.ToDateTime(paymentDate), Convert.ToDateTime(paymentDate), kullaniciId, subeId, id);

            return View(result);
        }

        public ActionResult IcmalOnayla(string id, string subeId, OnayDurumu onayDurumu)
        {
            var result = new IcmalCRUD().IcmalOnayla(id, subeId, onayDurumu);
            if (result.IsSuccess)
            {
                return SendNotificationAfterRedirect(result.UserMessage, "List", "Icmal");
            }

            ModelState.AddModelError("", result.UserMessage);

            return SendErrorAfterRedirect(result.UserMessage, "List", "Icmal");
        }


        #region  Sube Listesi Bilgileri alınıyor
        [HttpPost]
        public JsonResult SubeListJson()
        {
            var kullaniciId = Request.Cookies["PRAUT"].Value;
            var items = new List<SelectListItem>();
            items = new BussinessHelper().GetSubeSelectListItem(kullaniciId);
            return Json(items, JsonRequestBehavior.AllowGet);
        }
        #endregion Sube Listesi Bilgileri alınıyor
    }
}