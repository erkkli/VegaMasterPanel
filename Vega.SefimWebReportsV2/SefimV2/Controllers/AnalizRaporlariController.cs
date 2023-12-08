using SefimV2.Helper;
using SefimV2.Models;
using SefimV2.Models.ReportsDetailCRUD;
using SefimV2.Repository;
using SefimV2.ViewModels.ReportsDetail;
using SefimV2.ViewModels.Result;
using SefimV2.ViewModels.User;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web.Configuration;
using System.Web.Mvc;

namespace SefimV2.Controllers
{
    [CheckLoggedIn]
    public class AnalizRaporlariController : Controller
    {
        // GET: ReportsDetail2
        public ActionResult Index()
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

            ViewBag.LimitSize = 5;
            ViewBag.LimitSizeSubeUrun = 10;
            ViewBag.TplamSatis = 10;

            #region Masa üstü raporu /* List<ReportsDetailViewModel>*/
            //List<SubeCiro> list = SubeCiroCRUD.List(Convert.ToDateTime("01.10.2020 00:00:00"), Convert.ToDateTime("13.01.2021 23:59:59"), Id);
            #endregion Masa üstü raporu

            return View();
        }

        #region Şubelerin satış istatikleri
        [HttpGet]
        public JsonResult SubeCiroSatisIstatistik(string StartDate, string EndDate, int FilterData, string SubeId)
        {
            ViewBag.LimitSize = FilterData;
            string ID = Request.Cookies["PRAUT"].Value;
            JsonResultModel result = new JsonResultModel
            {
                IsSuccess = false
            };

            var NameList = new List<string>();
            var CategoriesList = new List<string>();
            Dictionary<string, List<decimal?>> dataList = new Dictionary<string, List<decimal?>>();

            var saat = Ayarlar.SaatListe();
            var startDate = Convert.ToDateTime(StartDate).ToString("dd'/'MM'/'yyyy " + saat.StartTime + "");
            var endDate = Convert.ToDateTime(EndDate).ToString("dd'/'MM'/'yyyy " + saat.EndTime + "");
            List<KasaCiro> SubeCiro = SubeCiroReportsCRUD2.List(Convert.ToDateTime(startDate), Convert.ToDateTime(endDate), saat.EndTime, ID, SubeId);
            List<KasaCiro> CiroyaGoreListe = SubeCiro.OrderByDescending(o => o.ToplamCiro).ToList();
            var dataGrpupList = (SubeCiro).GroupBy(x => x.Sube).ToList();

            if (CiroyaGoreListe != null)
            {
                var Test1 = dataGrpupList.Skip(0).ToList();
                foreach (var group in Test1)
                {
                    List<decimal?> nowVal = new List<decimal?>();
                    var dataCriterList = SubeCiro.Where(x => x.Sube == group.Key).ToList();

                    for (int i = 0; i < dataCriterList.Count; i++)
                    {
                        CategoriesList.Add("(" + (i + 1).ToString() + ")/" + Convert.ToDateTime(dataCriterList[i].DateStr).ToString("dd-MM-yyyy"));

                        decimal? nowTotal = null;
                        var dataDayResult = dataCriterList.Where(x => x.DayCount == i + 1).FirstOrDefault();

                        if (dataDayResult != null && dataDayResult.ToplamCiro != null && dataDayResult.ToplamCiro != "")
                        {
                            nowTotal = Convert.ToDecimal(dataDayResult.ToplamCiro);
                        }
                        nowVal.Add(nowTotal);
                    }

                    #region Value Kontrol // Bütün değerleri null olan grubun verileri eklenmiyor
                    var valControl = nowVal.Where(x => x != null).ToList();

                    if (valControl.Count > 0)
                    {
                        dataList.Add(group.Key, nowVal);
                        NameList.Add(group.Key);
                    }
                    #endregion
                }
            }
            return Json(new
            {
                Categories = CategoriesList,
                Name = NameList,
                Data = dataList,
            }, JsonRequestBehavior.AllowGet);
        }
        #endregion Şubelerin satış istatikleri

        #region Saat Bazlı Şube Ciro Raporu
        public JsonResult SaatBazliSubeCiroTable(string StartDateTime, string EndDateTime, string SubeId, string Saat, string UrunGrubuAdi, string UrunAdi, string SatisTipi, bool DetayRaporMu = false)
        {
            #region Yetkili id (Left Menude filtrele de yapıyorum)
            string ID = Request.Cookies["PRAUT"].Value;
            ViewBag.YetkiliID = ID;
            #endregion

            var saat = Ayarlar.SaatListe();
            if (DetayRaporMu)
            {
                EndDateTime = EndDateTime + " " + saat.EndTime;
            }
            else if (string.IsNullOrWhiteSpace(Saat) || Saat == "null")
            {
                EndDateTime = EndDateTime + " " + saat.EndTime;
            }

            List<KasaCiro> resultList = SaatBazliSubeCiroReportsCRUD2.List(Convert.ToDateTime(StartDateTime + " " + saat.StartTime), Convert.ToDateTime(EndDateTime), SubeId, saat.EndTime, Saat, ID, UrunGrubuAdi, UrunAdi, SatisTipi, DetayRaporMu, false)
                .Where(x => x.Tarih != null)
                //.OrderBy(x => x.Tarih)
                .ToList();

            if (!DetayRaporMu)
            {
                resultList = resultList
                    .GroupBy(x => x.Tarih)
                    .Select(y => new KasaCiro
                    {
                        Ciro = y.Select(b => b.Ciro).Sum(),
                        Sube = y.Select(b => b.Sube).FirstOrDefault(),
                        SubeId = y.Select(b => b.SubeId).FirstOrDefault(),
                        ToplamCiro = y.Select(b => b.ToplamCiro).FirstOrDefault(),
                        Tarih = y.Select(b => b.Tarih).FirstOrDefault(),
                        StartDate = y.Select(b => b.StartDate).FirstOrDefault(),
                        EndDate = y.Select(b => b.EndDate).FirstOrDefault(),
                    })
                    //.OrderBy(z => z.Tarih)
                    .ToList();
            }
            return Json(new
            {
                Data = resultList
            }, JsonRequestBehavior.AllowGet);
        }

        private readonly string[] createTableList = {
                        "06:00" ,
                        "07:00",
                        "08:00 ",
                        "09:00",
                        "10:00",
                        "11:00",
                        "12:00",
                        "13:00",
                        "14:00",
                        "15:00",
                        "16:00",
                        "17:00",
                        "18:00",
                        "19:00",
                        "20:00",
                        "21:00",
                        "22:00",
                        "23:00",
                        "00:00",
                        "01:00",
                        "02:00",
                        "03:00",
                        "04:00",
                        "05:00",
         };

        //Chart
        [HttpGet]
        public JsonResult SaatBazliSubeCiroTableChart(string StartDateTime, string EndDateTime, string SubeId, string Saat, string UrunGrubuAdi, string UrunAdi, string SatisTipi, bool DetayRaporMu = false)
        {
            #region YETKİLİ ID (Left Menude filtrele de yapıyorum)    
            bool cookieExists = HttpContext.Request.Cookies["PRAUT"] != null;
            if (cookieExists == false)
            {
                //RedirectToAction("Authentication", "Login");
                ////return RedirectToAction("/Authentication/Login");
                //return Json(new { code = 1 });
            }
            string ID = Request.Cookies["PRAUT"].Value;
            ViewBag.YetkiliID = ID;
            #endregion

            var NameList = new List<string>();
            var CategoriesList = new List<string>();
            Dictionary<string, List<decimal?>> dataList = new Dictionary<string, List<decimal?>>();
            var saat = Ayarlar.SaatListe();
            if (DetayRaporMu)
            {
                EndDateTime = EndDateTime + " " + saat.EndTime;
            }
         
            List<KasaCiro> resultList = SaatBazliSubeCiroReportsCRUD2.List(Convert.ToDateTime(StartDateTime + " " + saat.StartTime), Convert.ToDateTime(EndDateTime), SubeId, saat.EndTime, Saat, ID, UrunGrubuAdi, UrunAdi, SatisTipi, DetayRaporMu, true);
            List<KasaCiro> CiroyaGoreListe = resultList.OrderByDescending(o => o.ToplamCiro).ToList();
            var dataGrpupList = (resultList).GroupBy(x => x.Sube).ToList();

            #region CategoriesList / ILK ve SON TARIH SET EDILIYOR                
            foreach (var item in createTableList)
            {
                CategoriesList.Add(item.ToString());
            }
            #endregion CategoriesList / ILK ve SON TARIH SET EDILIYOR

            foreach (var group in dataGrpupList)
            {
                var dataCriterList = resultList.Where(x => x.Sube == group.Key).ToList();
                List<decimal?> nowVal = new List<decimal?>();

                //for (int i = 1; i <= createTableList.Length; i++)
                foreach (var item in createTableList)
                {
                    decimal? nowTotal = 0;
                    var dataDayResult = dataCriterList.Where(x => x.Saat == item).FirstOrDefault();

                    if (dataDayResult != null)
                    {
                        if (dataDayResult.Ciro > 0)
                        {
                            nowTotal = Convert.ToDecimal(dataDayResult.Ciro);
                        }
                    }
                    nowVal.Add(nowTotal);
                }

                #region Value Kontrol // Bütün değerleri null olan grubun verileri eklenmiyor
                var valControl = nowVal.Where(x => x != null).ToList();

                if (valControl.Count > 0)
                {
                    dataList.Add(group.Key, nowVal);
                    NameList.Add(group.Key);
                }
                #endregion
            }

            return Json(new
            {
                Categories = CategoriesList,
                Name = NameList,
                Data = dataList,
            }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult SaatBazliUrunGrubuVeUrunSatisRaporu(string StartDateTime, string EndDateTime, string SubeId, string Saat, string UrunGrubuAdi, string UrunAdi, string SatisTipi)
        {
            #region Yetkili Id
            string ID = Request.Cookies["PRAUT"].Value;
            ViewBag.YetkiliID = ID;
            #endregion

            var saat = Ayarlar.SaatListe();
            EndDateTime = EndDateTime + " " + saat.EndTime;

            var resultList = SaatBazliUrunGrubuVeUrunSatisReportsCRUD2.List(Convert.ToDateTime(StartDateTime), Convert.ToDateTime(EndDateTime), SubeId, saat.EndTime, ID, UrunGrubuAdi, UrunAdi, Saat, SatisTipi, false)
                 .OrderBy(x => x.ProductName)
                 .Select(x => new ReportsDetailViewModel
                 {
                     Sube = x.Sube,
                     SubeID = x.SubeID,
                     StartDate = StartDateTime,
                     EndDate = EndDateTime,
                     SaatList = Saat,
                     FilterUrunAdiList = UrunAdi,
                     Miktar = x.Miktar,
                     ProductName = x.ProductName,
                     Debit = x.Debit,
                     ProductGroup = x.ProductGroup
                 })
                 .OrderBy(x => x.ProductGroup)
                 .ToList();

            return View(resultList);
        }

        [HttpGet]
        public ActionResult SaatBazliUrunGrubuVeUrunSatisRaporuDetay(string StartDate, string EndDate, string SubeId, string Saat, string UrunGrubuAdi, string UrunAdi, string SatisTipi)
        {
            #region Yetkili Id
            string ID = Request.Cookies["PRAUT"].Value;
            ViewBag.YetkiliID = ID;
            #endregion

            var saat = Ayarlar.SaatListe();
            EndDate = Convert.ToDateTime(EndDate).ToString("dd.MM.yyyy") + " " + saat.EndTime;

            var resultList = SaatBazliUrunGrubuVeUrunSatisReportsCRUD2.List(Convert.ToDateTime(StartDate + " " + saat.StartTime), Convert.ToDateTime(EndDate), SubeId, saat.EndTime, ID, UrunGrubuAdi, UrunAdi, Saat, SatisTipi, true)
                .OrderBy(x => x.ProductName)
                .ToList();

            return View(resultList);
        }

        #endregion Saat Bazlı Şube Ciro Raporu      

        //Dropdown List
        #region  saat Listesi alınıyor
        [HttpPost]
        public JsonResult SaatListJson()
        {
            List<SelectListItem> items = new List<SelectListItem>();
            try
            {
                bool cookieExists = HttpContext.Request.Cookies["PRAUT"] != null;
                if (cookieExists == false)
                {
                    //return Redirect("/Authentication/Login");
                }

                string[] createTableValueList = {
                            "06:00-6:59" ,
                            "07:00-7:59",
                            "08:00-8:59",
                            "09:00-9:59",
                            "10:00-10:59",
                            "11:00-11:59",
                            "12:00-12:59",
                            "13:00-13:59",
                            "14:00-14:59",
                            "15:00-15:59",
                            "16:00-16:59",
                            "17:00-17:59",
                            "18:00-18:59",
                            "19:00-19:59",
                            "20:00-20:59",
                            "21:00-21:59",
                            "22:00-22:59",
                            "23:00-23:59",
                            "00:00-00:59",
                            "01:00-01:59",
                            "02:00-02:59",
                            "03:00-03:59",
                            "04:00-04:59",
                            "05:00-05:59",
                };

                for (int i = 0; i < createTableList.Length; i++)
                {
                    items.Add(new SelectListItem
                    {
                        Text = createTableValueList[i],
                        Value = createTableList[i],
                    });
                }
            }
            catch (Exception ex)
            {
                Singleton.WritingLogFile("SaatListJson:" + "ReportsDetail2Controller", ex.Message.ToString());
            }

            return Json((items).ToList(), JsonRequestBehavior.AllowGet);
        }
        #endregion  Şube Listesi alınıyor

        #region  Şube Listesi alınıyor
        [HttpPost]
        public JsonResult SubelerListJson()
        {
            List<SelectListItem> items = new List<SelectListItem>();
            ModelFunctions f = new ModelFunctions();

            try
            {
                bool cookieExists = HttpContext.Request.Cookies["PRAUT"] != null;
                if (cookieExists == false)
                {
                    //return Redirect("/Authentication/Login");
                }
                string ID = Request.Cookies["PRAUT"].Value;

                if (ID == "1")
                {
                    f.SqlConnOpen();
                    DataTable dt = f.DataTable(" Select  Id, SubeName from SubeSettings Where [Status] ='1' Order By SubeName ");
                    foreach (DataRow r in dt.Rows)
                    {
                        UserViewModel model = new UserViewModel();
                        items.Add(new SelectListItem
                        {
                            Text = f.RTS(r, "SubeName").ToString(),
                            Value = f.RTS(r, "Id"),
                        });
                    }
                    f.SqlConnClose();
                }
                else
                {
                    f.SqlConnOpen();
                    DataTable dt = f.DataTable(" SELECT  USR.UserID,USR.SubeID , SS.SubeName FROM UserSubeRelations USR    INNER JOIN SubeSettings SS ON  USR.SubeID = SS.Id    WHERE USR.UserID = " + ID + " and [Status] = '1'    ORDER BY SS.SubeName ");
                    foreach (DataRow r in dt.Rows)
                    {
                        UserViewModel model = new UserViewModel();
                        items.Add(new SelectListItem
                        {
                            Text = f.RTS(r, "SubeName").ToString(),
                            Value = f.RTS(r, "SubeID"),
                        });
                    }
                    f.SqlConnClose();
                }
            }
            catch (Exception ex)
            {
                Singleton.WritingLogFile("ReportsDetail2_SubelerListJson_Exception:", ex.Message);
            }

            return Json((items).ToList(), JsonRequestBehavior.AllowGet);
        }
        #endregion  Şube Listesi alınıyor

        #region  Şube ürün Listesi alınıyor
        [HttpPost]
        public JsonResult SubeUrunListJson()
        {
            List<SelectListItem> items = new List<SelectListItem>();
            ModelFunctions f = new ModelFunctions();

            try
            {
                bool cookieExists = HttpContext.Request.Cookies["PRAUT"] != null;
                if (cookieExists == false)
                {
                    //return Redirect("/Authentication/Login");
                }
                string ID = Request.Cookies["PRAUT"].Value;

                //if (Id == "1")
                //{
                //
                //Configuration webConfigApp;
                //webConfigApp = WebConfigurationManager.OpenWebConfiguration("/", "VegaMaster", null, Environment.MachineName);
                string User = WebConfigurationManager.AppSettings["User"];
                string Password = WebConfigurationManager.AppSettings["Password"];
                string VeriTabani = WebConfigurationManager.AppSettings["SefimDetayDBName"];
                string Sunucu = WebConfigurationManager.AppSettings["Server"];
                //
                //string Query = "SELECT ıd,ProductName,ProductGroup FROM Product order by ProductName ";
                string Query = " SELECT DISTINCT((CASE WHEN ISNULL(c1.name, '') = '' THEN ISNULL(P.ProductName, '') ELSE " +
                               " ISNULL(P.ProductName, '') + '.' + ISNULL(c1.name, '') + CASE WHEN ISNULL(c2.name, '') = '' then '' else  '.' end + ISNULL(c2.name, '') END)) AS  " +
                               "   ProductName " +
                               "   FROM Product p   left join Choice1 c1 on c1.ProductId = p.Id " +
                               "   left join Choice2 c2 on c2.ProductId = p.Id  where p.ProductName Not Like '$%' and p.ProductName Not Like '%Rezervasyon%' Order By ProductName ";
                DataTable subeUrunDt = new DataTable();
                subeUrunDt = f.GetSubeDataWithQuery((f.NewConnectionString(Sunucu, VeriTabani, User, Password)), Query.ToString());

                foreach (DataRow r in subeUrunDt.Rows)
                {
                    SubeUrun model = new SubeUrun();
                    items.Add(new SelectListItem
                    {
                        Text = f.RTS(r, "ProductName").ToString(),
                        Value = f.RTS(r, "ProductName"),
                        //Value = f.RTS(r, "UrunID"),
                    });
                }
                f.SqlConnClose();
                //}
                //else
                //{
                //    f.SqlConnOpen();
                //    DataTable dt = f.DataTable("   SELECT  USR.UserID,USR.SubeID , SS.SubeName FROM UserSubeRelations USR    INNER JOIN SubeSettings SS ON  USR.SubeID = SS.Id    WHERE USR.UserID = " + Id + " and [Status] = '1'    ORDER BY SS.SubeName ");
                //    foreach (DataRow r in dt.Rows)
                //    {
                //        UserViewModel model = new UserViewModel();
                //        items.Add(new SelectListItem
                //        {
                //            Text = f.RTS(r, "SubeName").ToString(),
                //            Value = f.RTS(r, "SubeID"),
                //        });
                //    }
                //    f.SqlConnClose();
                //}
            }
            catch (Exception ex)
            {
                Singleton.WritingLogFile("ReportsDetail2_SubeUrunListJson_Exception", ex.Message);
            }

            return Json((items).ToList(), JsonRequestBehavior.AllowGet);
        }
        #endregion Şube ürün Listesi alınıyor

        #region  Şube ürün Grubu Listesi alınıyor
        [HttpPost]
        public JsonResult SubeUrunGrubuListJson()
        {
            List<SelectListItem> items = new List<SelectListItem>();
            ModelFunctions f = new ModelFunctions();

            try
            {
                bool cookieExists = HttpContext.Request.Cookies["PRAUT"] != null;
                if (cookieExists == false)
                {
                    //return Redirect("/Authentication/Login");
                }
                string ID = Request.Cookies["PRAUT"].Value;
                string User = WebConfigurationManager.AppSettings["User"];
                string Password = WebConfigurationManager.AppSettings["Password"];
                string VeriTabani = WebConfigurationManager.AppSettings["SefimDetayDBName"];
                string Sunucu = WebConfigurationManager.AppSettings["Server"];
                //                
                string query = "SELECT COUNT(Id), ProductGroup  FROM Product where ProductGroup Not Like '$%' and   ProductGroup Not Like '%Rezervasyon%'  GROUP BY ProductGroup  HAVING COUNT(Id) >0 ";
                DataTable subeUrunDt = new DataTable();
                subeUrunDt = f.GetSubeDataWithQuery(f.NewConnectionString(Sunucu, VeriTabani, User, Password), query.ToString());

                foreach (DataRow r in subeUrunDt.Rows)
                {
                    items.Add(new SelectListItem
                    {
                        Text = f.RTS(r, "ProductGroup").ToString(),
                        Value = f.RTS(r, "ProductGroup").ToString(),
                    });
                }
                f.SqlConnClose();
            }
            catch (Exception ex)
            {
                Singleton.WritingLogFile("ReportsDetail2_SubeUrunGrubuListJson_Exception", ex.Message);
            }

            return Json((items).ToList(), JsonRequestBehavior.AllowGet);
        }
        #endregion Şube ürün Grubu Listesi alınıyor

        #region Masa üstü raporu ay Listesi alınıyor
        [HttpPost]
        public JsonResult MasaUstuRaporAyListJson()
        {
            List<SelectListItem> items = new List<SelectListItem>();

            try
            {
                bool cookieExists = HttpContext.Request.Cookies["PRAUT"] != null;
                if (cookieExists == false)
                {
                    //return Redirect("/Authentication/Login");
                }
                string ID = Request.Cookies["PRAUT"].Value;

                var dayList = Singleton.SetMonthDayList();

                var dataMontYear = Singleton.SetMonthYear();
                foreach (var montYear in dataMontYear)
                {
                    items.Add(new SelectListItem
                    {
                        Text = montYear.MonthYear,
                        Value = montYear.SqlScriptEndDate,
                    });
                }
            }
            catch (Exception ex)
            {
                Singleton.WritingLogFile("ReportsDetail2_MasaUstuRaporAyListJson_Exception", ex.Message);
            }

            return Json((items).ToList(), JsonRequestBehavior.AllowGet);
        }
        #endregion Masa üstü raporu ay Listesi alınıyor

        #region Masa üstü raporu ay Listesi alınıyor
        [HttpPost]
        public JsonResult MasaUstuRaporGunAyListJson()
        {
            List<SelectListItem> items = new List<SelectListItem>();

            try
            {
                bool cookieExists = HttpContext.Request.Cookies["PRAUT"] != null;
                if (cookieExists == false)
                {
                    //return Redirect("/Authentication/Login");
                }
                string ID = Request.Cookies["PRAUT"].Value;

                var dayList = Singleton.SetMonthDayList();

                //var dataMontYear = Singleton.SetMonthYear();
                foreach (var montYear in dayList)
                {
                    items.Add(new SelectListItem
                    {
                        Text = montYear.DayMonthYear,
                        Value = montYear.SqlScriptEndDate,
                    });
                }
            }
            catch (Exception ex) { }

            return Json((items).ToList(), JsonRequestBehavior.AllowGet);
        }
        #endregion Masa üstü raporu ay Listesi alınıyor
    }
}