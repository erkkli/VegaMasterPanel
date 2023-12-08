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
    public class ReportsDetailComparisonController : Controller
    {
        // GET: ReportsDetail
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



        #region Şube Karşılaştırma Raporları

        public JsonResult SubstationComparisonReport(string startDateTime, string endDateTime, int filterData, string subeId, string urunAdi)
        {
            #region YETKİLİ ID (Left Menude filtrele de yapıyorum)
            string ID = Request.Cookies["PRAUT"].Value;
            ViewBag.YetkiliID = ID;
            #endregion

            JsonResultModel result = new JsonResultModel
            {
                IsSuccess = false
            };

            var NameList = new List<string>();
            var DataList = new List<Dictionary<string, string>>();

            #region NEW SQL WEB REPORT          
            List<SubeCiro> list = ReportsDetailComparisonCRUD.List(Convert.ToDateTime(Convert.ToDateTime(startDateTime).ToString("dd'/'MM'/'yyyy HH:mm")), Convert.ToDateTime(Convert.ToDateTime(endDateTime).ToString("dd'/'MM'/'yyyy HH:mm")), subeId, ID);
            //List<SubeCiro> CiroyaGoreListe = list.OrderByDescending(o => o.Debit).ToList();

            #endregion NEW SQL WEB REPORT

                     
           return Json(new
            {
                Data = list
            }, JsonRequestBehavior.AllowGet);          
        }

        #endregion Şube Karşılaştırma Raporları

        // Dropdown List

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
            catch (Exception ex) { }

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
                               "   left join Choice2 c2 on c2.ProductId = p.Id  where p.ProductName Not Like '$%' Order By ProductName ";
                DataTable subeUrunDt = new DataTable();
                subeUrunDt = f.GetSubeDataWithQuery((f.NewConnectionString(Sunucu, VeriTabani, User, Password)), Query.ToString());

                foreach (DataRow r in subeUrunDt.Rows)
                {
                    SubeUrun model = new SubeUrun();
                    items.Add(new SelectListItem
                    {
                        Text = f.RTS(r, "ProductName").ToString(),
                        Value = f.RTS(r, "UrunID"),
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
            catch (Exception ex) { }

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
                string Query = " SELECT COUNT(Id), ProductGroup  FROM Product  GROUP BY ProductGroup  HAVING COUNT(Id) >0 ";
                DataTable subeUrunDt = new DataTable();
                subeUrunDt = f.GetSubeDataWithQuery((f.NewConnectionString(Sunucu, VeriTabani, User, Password)), Query.ToString());

                foreach (DataRow r in subeUrunDt.Rows)
                {
                    items.Add(new SelectListItem
                    {
                        Text = f.RTS(r, "ProductGroup").ToString(),
                        Value = f.RTS(r, "ProductGroup"),
                    });
                }
                f.SqlConnClose();
            }
            catch (Exception ex) { }

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
            catch (Exception ex) { }

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