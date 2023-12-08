using SefimV2.Helper;
using SefimV2.Models;
using SefimV2.Repository;
using SefimV2.ViewModels.SubeSettings;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Web.Mvc;
using static SefimV2.ViewModels.SubeSettings.SubeSettingsViewModel;

namespace SefimV2.Controllers
{
    [CheckLoggedIn]
    public class SubeSettingsController : Controller
    {
        // GET: SubeSettings
        public ActionResult Index()
        {
            #region Page Navi işlemleri
            Dictionary<string, string> PageNavi = new Dictionary<string, string>();
            PageNavi.Add("Şube Listesi", "");
            ViewBag.PageNavi = PageNavi;
            #endregion

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
                string ID = Request.Cookies["PRAUT"].Value;
                ViewBag.YetkiliID = ID;
                ViewBag.KullaniciUygulamaTipi = BussinessHelper.GetByUserInfoForId("", "", ID).UygulamaTipi;
                #endregion
            }

            #region Navigasyon için eklenen kodlar
            Dictionary<string, string> PageNavi = new Dictionary<string, string>();
            PageNavi.Add("Şubeler Listesi", "Index");
            //PageNavi.Add("Yeni Ekle", "");
            #endregion

            ViewBag.ActionTitle = "Listesi";
            ViewBag.Controller = "SubeSettings";
            ViewBag.PageTitle = "Şube";
            //ViewBag.Page = Page;
            ViewBag.PageNavi = PageNavi;
            ViewBag.UygulamaTipi = ut;

            List<SubeSettingsViewModel> list = SubeSettingsCRUD.List();
            return View(list);
        }

        #region Form Ekleme & Düzenleme İşlemleri
        public ActionResult Ekle([DefaultValue(0)]int id, [DefaultValue("All")] string Page, string ut)
        {
            ViewBag.UygulamaTipi = ut;

            #region AYARLAR LİSTESİNİ GÖSTERMEK İÇİN (_LayoutPags De kontrolu yapılıyor)
            string ID = Request.Cookies["PRAUT"].Value;
            ViewBag.YetkiliID = ID;
            #endregion

            var model = new SubeSettingsViewModel();
            #region Navigasyon için eklenen kodlar
            Dictionary<string, string> PageNavi = new Dictionary<string, string>
            {
                { "Şube Listesi", "Index" },
                { "Yeni Ekle", "" }
            };
            #endregion
            try
            {
                #region Ekleme İşlemleri
                model.ID = 0;
                model.FirmaID = "0";
                model.DepoID = "0";
                model.DonemID = "0";
                model.ZimmetCariInd = "0";
                model.Status = true;
                ViewBag.ActionTitle = "Şube Ekle";
                #endregion

                #region Zimmet cari 

                //List<SelectListItem> itemsDropdownZimmet = new List<SelectListItem>();
                //var zimmetCari = SubeSettings.SefimPanelZimmetCagrisi("", Convert.ToInt32(model.FirmaID));

                //foreach (var item in zimmetCari)
                //{
                //    itemsDropdownZimmet.Add(new SelectListItem
                //    {
                //        Text = item.ZimmetFirmaAdi,
                //        Value = item.ZimmetIND,
                //        Selected = true
                //    });
                //}
                //ViewBag.ZimmetCari = itemsDropdownZimmet;

                #endregion Zimmet cari 

                #region Güncelle
                if (id != 0)
                {
                    ViewBag.ActionTitle = "Şube Düzenle";
                    model.ZimmetCariInd = model.ZimmetCariInd == null ? "0" : model.ZimmetCariInd;
                    model = SubeSettingsCRUD.GetUser(id);

                    if (!string.IsNullOrWhiteSpace(model.FasterKasaListesi))
                    {
                        string[] fasterKasalar = model.FasterKasaListesi.Split(',');
                        var itemsDropdown = new List<SelectListItem>();
                        var fasterKasaList = new List<FR_FasterKasalar>();
                        foreach (var item in fasterKasalar)
                        {
                            var kasaAdi = SubeSettings.FasterKasaAdi(item, Convert.ToInt32(model.FirmaID));
                            itemsDropdown.Add(new SelectListItem
                            {
                                Text = kasaAdi,
                                Value = item,
                                Selected = true
                            });
                            fasterKasaList.Add(new FR_FasterKasalar
                            {
                                ID = Convert.ToInt32(item),
                                KasaAdi = kasaAdi
                            });
                        }
                        ViewBag.FasterKasalar = itemsDropdown;
                        model.FR_FasterKasaListesi = fasterKasaList;
                    }

                    if (!string.IsNullOrWhiteSpace(model.VPosKasaKodu))
                    {
                        string[] vposKasaList = model.VPosKasaKodu.Split('&');
                        var selectListItems = new List<SelectListItem>();
                        var vPosKasaList = new List<VPosKasalarList>();
                        foreach (var item in vposKasaList)
                        {
                            if (!string.IsNullOrWhiteSpace(item))
                            {
                                selectListItems.Add(new SelectListItem
                                {
                                    Text = item,
                                    Value = item,
                                    Selected = true
                                });

                                vPosKasaList.Add(new VPosKasalarList
                                {
                                    ID = (item),
                                    KasaAdi = item.ToString(),
                                });
                            }
                        }
                        ViewBag.VPosKasaKodu = selectListItems;
                        model.VPosKasalarList = vPosKasaList;
                    }

                }
                #endregion

                ViewBag.Controller = "SubeSettings";
                ViewBag.Page = Page;
                ViewBag.PageNavi = PageNavi;
            }
            catch (Exception ex)
            {
                Singleton.WritingLog("SubeSettingsException", "Hata" + ex);
            }

            return View(model);
        }
        #endregion Form Ekleme & Düzenleme İşlemleri

        #region INSERT / UPDATE
        //[ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult Ekle(SubeSettingsViewModel course, string[] FasterKasalar, string[] VPosKasaKodu, string ut)
        {
            var result = new ActionResultMessages();
            //result.IsSuccess = false;
            ViewBag.UygulamaTipi = ut;
            #region Zorunu alan kontrolleri
            if (course.AppDbType == 0 || string.IsNullOrEmpty(course.DBName) || string.IsNullOrEmpty(course.SqlName.ToString()) || string.IsNullOrEmpty(course.SqlPassword.ToString()) || string.IsNullOrEmpty(course.SubeIP) || string.IsNullOrEmpty(course.SubeName))
            {
                result.UserMessage = "Alanlar Boş Geçilemez. Lütfen Kontrol Ediniz.";// Singleton.getObject().ActionMessage("inputEmpty");
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            #endregion Zorunu alan kontrolleri

            #region FASTER İÇİN EKLENEN KASALARI ALIYORUM(güncel girilmediyse db den alıp setliyorum. !! geniş zamanda daha uygun yapılacak!!)           
            if (FasterKasalar == null)
            {
                //var dataFasterKasaList = SubeSettingsCRUD.GetUser(course.Id);
                //course.FasterKasaListesi = dataFasterKasaList.FasterKasaListesi;
                course.FasterKasaListesi = null;
            }
            else
            {
                foreach (var item in FasterKasalar)
                {
                    course.FasterKasaListesi += item + ",";
                }
                course.FasterKasaListesi = course.FasterKasaListesi.Substring(0, course.FasterKasaListesi.Length - 1);
            }
            #endregion FASTER İÇİN EKLENEN KASALARI ALIYORUM(güncel girilmediyse db den alıp setliyorum. !! geniş zamanda daha uygun yapılacak!!)


            if (course.AppDbType == 5)
            {
                if (VPosKasaKodu != null)
                {
                    course.VPosKasaKodu = null;
                    foreach (var kasa in VPosKasaKodu)
                    {
                        course.VPosKasaKodu += "&" + kasa + "&";
                    }
                }
                else
                {
                    course.VPosKasaKodu = null;
                }
            }

            if (course.AppDbType == 5)
            {
                if (course.VPosSubeKodu != null)
                {
                    //string[] vposKasaList = course.VPosSubeKodu.Split(';');
                    //course.VPosSubeKodu = vposKasaList[0];
                }
                else
                {
                    course.VPosKasaKodu = null;
                }
            }


            if (course.ID == 0)
            {
                #region Ekleme İşlemleri                
                try
                {
                    result = new SubeSettingsCRUD().SubeSettingsInsert(course);

                }
                catch (Exception ex)
                {
                    result.UserMessage = ex.Message;
                }
                #endregion
            }
            else
            {
                #region Guncelle
                try
                {
                    result = new SubeSettingsCRUD().SubeSettingsUpdate(course);
                }
                catch (Exception ex)
                {
                    result.UserMessage = ex.Message;
                }
                #endregion
            }

            return Json(result, JsonRequestBehavior.AllowGet);
        }
        #endregion INSERT / UPDATE

        #region Delete
        public ActionResult Delete(int id)
        {
            ActionResultMessages result = new ActionResultMessages();
            result = new SubeSettingsCRUD().SubeSettingsDelete(id);
            return RedirectToAction("List");
        }

        [HttpPost]
        public ActionResult Delete(int id, FormCollection collection)
        {
            ActionResultMessages result = new ActionResultMessages();
            result = new SubeSettingsCRUD().SubeSettingsDelete(id);

            return Json(result, JsonRequestBehavior.AllowGet);
        }
        #endregion Delete


        #region ŞUBENİN AKTİF VE PASİF DURUMU
        [HttpGet]
        public ActionResult SubeAktifPasifYapma(int Sube_ID)
        {
            ActionResultMessages result = new ActionResultMessages();
            result = new SubeSettingsCRUD().SubeSettingsStatusUpdate(Sube_ID);

            result.IsSuccess = true;
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        #endregion   ŞUBENİN AKTİF VE PASİF DURUMU

        #region  Firma Listesi Bilgileri alınıyor
        [HttpPost]
        public JsonResult FirmaListtJson()
        {
            List<SelectListItem> items = new List<SelectListItem>();
            ModelFunctions f = new ModelFunctions();
            try
            {
                #region ENVANTER DB BAGLANIP GEREKLİ BILGILER ALIIYOR                
                f.SqlConnOpen();
                DataTable dt = f.DataTable("SELECT Id,DBName,IP,SqlName,SqlPassword  FROM  VegaDbSettings");
                string Query = "";
                foreach (DataRow r in dt.Rows)
                {
                    string VegaDbId = f.RTS(r, "Id");
                    string VegaDbName = f.RTS(r, "DBName");
                    string VegaDbIp = f.RTS(r, "IP");
                    string VegaDbSqlName = f.RTS(r, "SqlName");
                    string VegaDbSqlPassword = f.RTS(r, "SqlPassword");
                    Query = "SELECT IND, KOD, KISAAD FROM TBLFIRMA";//File.ReadAllText(HostingEnvironment.MapPath("/Sql/SqlVegaDb/Firma.sql"), System.Text.UTF8Encoding.Default);
                    string connString = "Provider=SQLOLEDB;Server=" + VegaDbIp + ";User Id=" + VegaDbSqlName + ";Password=" + VegaDbSqlPassword + ";Database=" + VegaDbName + "";
                    f.SqlConnOpen(true, connString);
                    DataTable AcikHesapDt = f.DataTable(Query, true);
                    if (AcikHesapDt.Rows.Count > 0)
                    {
                        foreach (DataRow SubeR in AcikHesapDt.Rows)
                        {
                            SubeSettingsViewModel model = new SubeSettingsViewModel();
                            items.Add(new SelectListItem
                            {
                                Text = f.RTS(SubeR, "KOD").ToString(),
                                Value = f.RTS(SubeR, "IND"),
                            });
                        }
                    }
                }
                f.SqlConnClose();
                #endregion
            }
            catch (System.Exception ex) { }

            return Json((items).ToList(), JsonRequestBehavior.AllowGet);
        }
        #endregion Firma Listesi Bilgileri alınıyor

        #region  Dönem Listesi Bilgileri alınıyor
        [HttpPost]
        public JsonResult DonemListJson(int TanimId)
        {
            List<SelectListItem> items = new List<SelectListItem>();
            ModelFunctions f = new ModelFunctions();
            try
            {
                #region ENVANTER DB BAGLANIP GEREKLİ BILGILER ALIIYOR                
                f.SqlConnOpen();
                DataTable dt = f.DataTable("SELECT Id,DBName,IP,SqlName,SqlPassword  FROM  VegaDbSettings");
                string Query = "";
                foreach (DataRow r in dt.Rows)
                {
                    string VegaDbId = f.RTS(r, "Id");
                    string VegaDbName = f.RTS(r, "DBName");
                    string VegaDbIp = f.RTS(r, "IP");
                    string VegaDbSqlName = f.RTS(r, "SqlName");
                    string VegaDbSqlPassword = f.RTS(r, "SqlPassword");
                    Query = "SELECT FIND, IND, DONEM FROM TBLDONEM WHERE FIND = " + TanimId + "";//File.ReadAllText(HostingEnvironment.MapPath(" /Sql/SqlVegaDb/Firma.sql"), System.Text.UTF8Encoding.Default);
                    //"SELECT FIND, IND, DONEM FROM TBLDONEM WHERE FIND = " + _FirmaIND);
                    string connString = "Provider=SQLOLEDB;Server=" + VegaDbIp + ";User Id=" + VegaDbSqlName + ";Password=" + VegaDbSqlPassword + ";Database=" + VegaDbName + "";
                    f.SqlConnOpen(true, connString);
                    DataTable AcikHesapDt = f.DataTable(Query, true);
                    if (AcikHesapDt.Rows.Count > 0)
                    {
                        foreach (DataRow SubeR in AcikHesapDt.Rows)
                        {
                            SubeSettingsViewModel model = new SubeSettingsViewModel();
                            items.Add(new SelectListItem
                            {
                                Text = f.RTS(SubeR, "DONEM").ToString(),
                                Value = f.RTS(SubeR, "IND"),
                            });
                        }
                    }
                }
                f.SqlConnClose();
                #endregion
            }
            catch (System.Exception ex) { }

            return Json((items).ToList(), JsonRequestBehavior.AllowGet);
        }
        #endregion Dönem Listesi Bilgileri alınıyor

        #region  Zimmet carisi Listesi alınıyor
        [HttpPost]
        public JsonResult ZimmetCariListJson(int TanimId)
        {
            List<SelectListItem> items = new List<SelectListItem>();
            ModelFunctions f = new ModelFunctions();
            try
            {
                f.SqlConnOpen();
                DataTable dt = f.DataTable("SELECT Id,DBName,IP,SqlName,SqlPassword  FROM  VegaDbSettings");
                string Query = "";
                foreach (DataRow r in dt.Rows)
                {
                    string VegaDbId = f.RTS(r, "Id");
                    string VegaDbName = f.RTS(r, "DBName");
                    string VegaDbIp = f.RTS(r, "IP");
                    string VegaDbSqlName = f.RTS(r, "SqlName");
                    string VegaDbSqlPassword = f.RTS(r, "SqlPassword");
                    Query = " SELECT IND,FIRMAKODU,FIRMAADI,ZIMFIYAT FROM  F0" + TanimId + "TBLCARI WHERE FIRMATIPI=9 ";
                    string connString = "Provider=SQLOLEDB;Server=" + VegaDbIp + ";User Id=" + VegaDbSqlName + ";Password=" + VegaDbSqlPassword + ";Database=" + VegaDbName + "";
                    f.SqlConnOpen(true, connString);
                    DataTable zimmetCagriDt = f.DataTable(Query, true);
                    if (zimmetCagriDt.Rows.Count > 0)
                    {
                        foreach (DataRow SubeR in zimmetCagriDt.Rows)
                        {
                            items.Add(new SelectListItem
                            {
                                Text = f.RTS(SubeR, "IND") + " " + f.RTS(SubeR, "FIRMAADI").ToString(),
                                Value = f.RTS(SubeR, "IND"),
                            });
                        }
                    }
                }
                f.SqlConnClose();
            }
            catch (Exception ex) { }

            return Json((items).ToList(), JsonRequestBehavior.AllowGet);
        }
        #endregion Zimmet carisi Listesi alınıyor

        #region  Depo Listesi Bilgileri alınıyor
        [HttpPost]
        public JsonResult DepoListJson(int TanimId)
        {
            List<SelectListItem> items = new List<SelectListItem>();
            ModelFunctions f = new ModelFunctions();
            try
            {
                #region ENVANTER DB BAGLANIP GEREKLİ BILGILER ALIIYOR                
                f.SqlConnOpen();
                DataTable dt = f.DataTable("SELECT Id,DBName,IP,SqlName,SqlPassword  FROM  VegaDbSettings");
                string Query = "";
                foreach (DataRow r in dt.Rows)
                {
                    string VegaDbId = f.RTS(r, "Id");
                    string VegaDbName = f.RTS(r, "DBName");
                    string VegaDbIp = f.RTS(r, "IP");
                    string VegaDbSqlName = f.RTS(r, "SqlName");
                    string VegaDbSqlPassword = f.RTS(r, "SqlPassword");
                    Query = "SELECT IND, DEPOADI, DEPOKODU FROM F0" + TanimId + "TBLDEPOLAR";//File.ReadAllText(HostingEnvironment.MapPath("/Sql/SqlVegaDb/Firma.sql"), System.Text.UTF8Encoding.Default);
                    string connString = "Provider=SQLOLEDB;Server=" + VegaDbIp + ";User Id=" + VegaDbSqlName + ";Password=" + VegaDbSqlPassword + ";Database=" + VegaDbName + "";
                    f.SqlConnOpen(true, connString);
                    DataTable AcikHesapDt = f.DataTable(Query, true);
                    if (AcikHesapDt.Rows.Count > 0)
                    {
                        foreach (DataRow SubeR in AcikHesapDt.Rows)
                        {
                            SubeSettingsViewModel model = new SubeSettingsViewModel();
                            items.Add(new SelectListItem
                            {
                                Text = f.RTS(SubeR, "DEPOADI").ToString(),
                                Value = f.RTS(SubeR, "IND"),
                            });
                        }
                    }
                }
                f.SqlConnClose();
                #endregion
            }
            catch (System.Exception ex) { }

            return Json((items).ToList(), JsonRequestBehavior.AllowGet);
        }
        #endregion Depo Listesi Bilgileri alınıyor

        #region  FasterKasa  Listesi Bilgileri alınıyor
        [HttpPost]
        public JsonResult FasterKasaListtJson(int TanimId)
        {
            List<SelectListItem> items = new List<SelectListItem>();
            ModelFunctions f = new ModelFunctions();
            SubeSettingsViewModel viewModel = new SubeSettingsViewModel();
            List<FR_FasterKasalar> fasterKasalar = new List<FR_FasterKasalar>();
            try
            {
                #region ENVANTER DB BAGLANIP GEREKLİ BILGILER ALIIYOR                 
                f.SqlConnOpen();
                DataTable dt = f.DataTable("SELECT Id,DBName,IP,SqlName,SqlPassword  FROM  VegaDbSettings");
                string Query = "";
                foreach (DataRow r in dt.Rows)
                {
                    string VegaDbId = f.RTS(r, "Id");
                    string VegaDbName = f.RTS(r, "DBName");
                    string VegaDbIp = f.RTS(r, "IP");
                    string VegaDbSqlName = f.RTS(r, "SqlName");
                    string VegaDbSqlPassword = f.RTS(r, "SqlPassword");
                    Query = "SELECT * FROM F0" + TanimId + "TBLKRDSUBELER";//File.ReadAllText(HostingEnvironment.MapPath("/Sql/SqlVegaDb/Firma.sql"), System.Text.UTF8Encoding.Default);
                    string connString = "Provider=SQLOLEDB;Server=" + VegaDbIp + ";User Id=" + VegaDbSqlName + ";Password=" + VegaDbSqlPassword + ";Database=" + VegaDbName + "";
                    f.SqlConnOpen(true, connString);
                    DataTable AcikHesapDt = f.DataTable(Query, true);
                    if (AcikHesapDt.Rows.Count > 0)
                    {


                        foreach (DataRow SubeR in AcikHesapDt.Rows)
                        {
                            fasterKasalar.Add(new FR_FasterKasalar
                            {
                                ID = f.RTI(SubeR, "IND"),
                                KasaAdi = f.RTS(SubeR, "SUBEADI").ToString(),

                            });




                            items.Add(new SelectListItem
                            {
                                Text = f.RTS(SubeR, "SUBEADI").ToString(),
                                Value = f.RTS(SubeR, "IND"),
                            });

                            viewModel.FasterKasaListesi += Convert.ToInt32(f.RTI(SubeR, "IND")) + ",";


                        }
                        ViewBag.FasterKasalar = items;
                    }
                }
                f.SqlConnClose();
                #endregion
            }
            catch (System.Exception ex) { }

            return Json((items).ToList(), JsonRequestBehavior.AllowGet);
        }
        #endregion Firma Listesi Bilgileri alınıyor



        #region  VPos Kasa Listesi alınıyor
        [HttpPost]
        public JsonResult VPosSubeListJson(string AppDbType, string SubeIP, string SqlName, string DBName, string SqlPassword)
        {
            var items = new List<SelectListItem>();
            var f = new ModelFunctions();
            try
            {
                string Query = "";
                //string VegaDbId = f.RTS(r, "Id");
                string VegaDbName = DBName;//f.RTS(r, "DBName");
                string VegaDbIp = SubeIP;//f.RTS(r, "IP");
                string VegaDbSqlName = SqlName;//f.RTS(r, "SqlName");
                string VegaDbSqlPassword = SqlPassword;//f.RTS(r, "SqlPassword");
                Query = "Select sb.Kodu,sb.IND from TBLSPOSSUBELER sb inner join  TBLSPOSKASALAR ks on sb.IND = ks.SUBEIND group by sb.KODU,sb.IND";//"Select * from TBLSPOSSUBELER";//" SELECT IND,FIRMAKODU,FIRMAADI,ZIMFIYAT FROM  F0" + TanimId + "TBLCARI WHERE FIRMATIPI=9 ";
                string connString = "Provider=SQLOLEDB;Server=" + VegaDbIp + ";User Id=" + VegaDbSqlName + ";Password=" + VegaDbSqlPassword + ";Database=" + VegaDbName + "";
                f.SqlConnOpen(true, connString);
                DataTable zimmetCagriDt = f.DataTable(Query, true);
                if (zimmetCagriDt.Rows.Count > 0)
                {
                    foreach (DataRow SubeR in zimmetCagriDt.Rows)
                    {
                        items.Add(new SelectListItem
                        {
                            Text = f.RTS(SubeR, "KODU").ToString(),
                            Value = f.RTS(SubeR, "IND"),
                        });
                    }
                }

                f.SqlConnClose();
            }
            catch (Exception ex) { }

            return Json((items).ToList(), JsonRequestBehavior.AllowGet);
        }
        #endregion VPos Kasa Listesi alınıyor

        #region  VPos Kasa Listesi alınıyor
        [HttpPost]
        public JsonResult VPosKasaListJson(string AppDbType, string SubeIP, string SqlName, string DBName, string SqlPassword, string SubeKodu,string Id)
        {
            var items = new List<SelectListItem>();
            var f = new ModelFunctions();
            try
            {
                ////var subeKodu = SubeKodu.Split(';');
                ////SubeKodu = subeKodu[1];
                //f.SqlConnOpen();
                //DataTable dt = f.DataTable("Select * from TBLSPOSKASALAR");
                string Query = "";
                //foreach (DataRow r in dt.Rows)
                //{
                //string VegaDbId = f.RTS(r, "Id");
                string VegaDbName = DBName;//f.RTS(r, "DBName");
                string VegaDbIp = SubeIP;//f.RTS(r, "IP");
                string VegaDbSqlName = SqlName;//f.RTS(r, "SqlName");
                string VegaDbSqlPassword = SqlPassword;//f.RTS(r, "SqlPassword");
                Query = "Select ks.Kodu from TBLSPOSSUBELER sb inner join  TBLSPOSKASALAR ks on sb.IND = ks.SUBEIND where ks.SUBEIND='" + SubeKodu + "' ";//"Select * from TBLSPOSKASALAR Where SUBEIND='" + SubeKodu + "'";//" SELECT IND,FIRMAKODU,FIRMAADI,ZIMFIYAT FROM  F0" + TanimId + "TBLCARI WHERE FIRMATIPI=9 ";
                string connString = "Provider=SQLOLEDB;Server=" + VegaDbIp + ";User Id=" + VegaDbSqlName + ";Password=" + VegaDbSqlPassword + ";Database=" + VegaDbName + "";
                f.SqlConnOpen(true, connString);
                DataTable zimmetCagriDt = f.DataTable(Query, true);
                if (zimmetCagriDt.Rows.Count > 0)
                {
                    foreach (DataRow SubeR in zimmetCagriDt.Rows)
                    {
                        items.Add(new SelectListItem
                        {
                            Text = f.RTS(SubeR, "KODU").ToString(),
                            Value = f.RTS(SubeR, "KODU"),
                        });
                    }
                }
                //}
                f.SqlConnClose();
            }
            catch (Exception ex) { }

            return Json((items).ToList(), JsonRequestBehavior.AllowGet);
        }
        #endregion VPos Kasa Listesi alınıyor
    }
}