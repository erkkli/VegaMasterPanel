using SefimV2.Helper;
using SefimV2.Models;
using SefimV2.Repository;
using SefimV2.ViewModels.GetTime;
using SefimV2.ViewModels.Result;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web.Mvc;

namespace SefimV2.Controllers
{
    [CheckLoggedIn]
    public class OzetRaporlarController : Controller
    {
        // GET: OzetRaporlar
        public ActionResult Index()
        {
            //ViewBag.LimitSize = 20;
            return View();
        }

        public ActionResult List()
        {
            try
            {
                #region YETKİLİ ID (Left Menude filtrele de yapıyorum)

                bool cookieExists = HttpContext.Request.Cookies["PRAUT"] != null;
                if (cookieExists == false)
                {
                    return Redirect("/Authentication/Login");
                }
                string ID = Request.Cookies["PRAUT"].Value;
                ViewBag.YetkiliID = ID;
                var kullaniciData = BussinessHelper.GetByUserInfoForId("", "", ID);
                ViewBag.SubeSirasiGorunsunMu = kullaniciData.SubeSirasiGorunsunMu;                             
                ViewBag.KullaniciUygulamaTipi = kullaniciData.UygulamaTipi;

                #endregion YETKİLİ ID (Left Menude filtrele de yapıyorum)

                #region DateFilter 

                string statusButton = string.Empty;
                Saat saat = Ayarlar.SaatListe();
                string gun = (DateTime.Today.ToString("dd"));
                string ay = DateTime.Today.ToString("MM");
                string yil = DateTime.Today.ToString("yyyy");
                string gunSonu_ = (DateTime.Today.AddDays(1).ToString("dd")); //Convert.ToInt32(gun) + 1;
                string a1 = DateTime.Now.AddDays(1).ToShortDateString();
                string gunSonuGun = Convert.ToDateTime(a1).ToString("dd");
                string gunSonuAy = Convert.ToDateTime(a1).ToString("MM");
                string gunsonuYil = Convert.ToDateTime(a1).ToString("yyyy");
                string bugun = yil + "-" + ay + "-" + gun + " " + saat.StartTime;//date2h.ToString() ;  //Convert.ToDateTime((DateTime.Today.ToShortDateString() + " " + saat.StartTime)).ToString("yyyy-MM-dd HH:mm");
                string gunSonu = gunsonuYil + "-" + gunSonuAy + "-" + gunSonuGun + " " + saat.EndTime;//Convert.ToDateTime((DateTime.Now.AddDays(1).ToShortDateString() + " " + saat.EndTime)).ToString("yyyy-MM-dd HH:mm");
                //string bugun = Convert.ToDateTime((DateTime.Today.ToShortDateString() + " " + saat.StartTime)).ToString("d.M.yyyy HH:mm");
                //string gunSonu = Convert.ToDateTime((DateTime.Now.AddDays(1).ToShortDateString() + " " + saat.EndTime)).ToString("d.M.yyyy HH:mm");

                #region Yeni Tarih Filtreleme 

                //ViewBag ile tarihler taşınıyor
                //DateTime GelenGun = new DateTime();
                //DateTime GelenGunSonu = new DateTime();
                ViewBag.Bugun = bugun;
                ViewBag.GunSonu = gunSonu;

                DateTime now = DateTime.Now;
                var startDate = new DateTime(now.Year, now.Month, now.Day).ToString();
                var oncekiGun = now.AddDays(-1).ToString("yyyy-MM-dd") + " " + saat.StartTime;
                var oncekiGunSonu = now.AddDays(-1).ToString("yyyy-MM-dd") + " " + saat.EndTime;
                ViewBag.oncekiGun = oncekiGun;
                ViewBag.oncekiGunSonu = oncekiGunSonu;
                //DateTime thisDate = new DateTime(2008, 3, 15);
                //DateTimeFormatInfo fmt = (new CultureInfo("hr-HR")).DateTimeFormat;
                //string ddd = thisDate.ToString("d", fmt);
                //String.Format("{0:d/M/yyyy HH:mm:ss}", GelenGun);

                #endregion Yeni Tarih Filtreleme 

                string durum = string.Empty;
                if (Request.QueryString["durum"] != null)
                    durum = Request.QueryString["durum"].ToString();

                if (durum.Equals("bugun"))
                {
                    //GelenGun = Convert.ToDateTime(bugun);
                    //GelenGunSonu = Convert.ToDateTime(gunSonu);
                    ViewBag.Bugun = bugun;
                    ViewBag.GunSonu = gunSonu;
                    statusButton = "bugun";
                }

                if (durum.Equals("dun"))
                {
                    #region Yeni Tarih Filtreleme
                    bugun = now.AddDays(-1).ToString("yyyy-MM-dd") + " " + saat.StartTime;
                    gunSonu = now.AddDays(0).ToString("yyyy-MM-dd") + " " + saat.EndTime;
                    ViewBag.Bugun = bugun;
                    ViewBag.GunSonu = gunSonu;
                    //DateTime now = DateTime.Now;
                    //startDate = new DateTime(now.Year, now.Month, now.Day).ToString();
                    oncekiGun = now.AddDays(-2).ToString("yyyy-MM-dd") + " " + saat.StartTime;
                    oncekiGunSonu = now.AddDays(-1).ToString("yyyy-MM-dd") + " " + saat.EndTime;
                    ViewBag.oncekiGun = oncekiGun;
                    ViewBag.oncekiGunSonu = oncekiGunSonu;
                    statusButton = "dun";
                    #endregion
                    //GelenGun = Convert.ToDateTime(bugun).AddDays(-1);
                    //GelenGunSonu = Convert.ToDateTime(gunSonu).AddDays(-1);
                }

                if (durum.Equals("buAy"))
                {
                    #region Yeni Tarih filtreleme
                    //DateTime now = DateTime.Now;
                    var startDate11 = new DateTime(now.Year, now.Month, 1);
                    var endDate11 = startDate11.AddMonths(1).AddDays(-1);
                    bugun = startDate11.ToString("yyyy-MM-dd") + " " + saat.StartTime;
                    gunSonu = endDate11.ToString("yyyy-MM-dd") + " " + saat.EndTime;
                    ViewBag.Bugun = bugun;
                    ViewBag.GunSonu = gunSonu;
                    statusButton = "buAy";
                    #endregion

                    //GelenGun = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
                    //GelenGun = GelenGun.AddHours(Convert.ToInt32(saat.StartTime.Split(':')[0]));
                    //GelenGun = GelenGun.AddMinutes(Convert.ToInt32(saat.StartTime.Split(':')[1]));
                    //GelenGunSonu = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
                    //GelenGunSonu = GelenGunSonu.AddHours(Convert.ToInt32(saat.EndTime.Split(':')[0]));
                    //GelenGunSonu = GelenGunSonu.AddMinutes(Convert.ToInt32(saat.EndTime.Split(':')[1]));
                    //GelenGunSonu = GelenGunSonu.AddMonths(1);
                }

                if (durum.Equals("tarihAraligi"))
                {
                    if (Request.QueryString["tarihBas"] != null && Request.QueryString["tarihBitis"] != null)
                    {
                        bugun = (Convert.ToDateTime(Request.QueryString["tarihBas"].ToString())).ToString();
                        gunSonu = (Convert.ToDateTime(Request.QueryString["tarihBitis"].ToString())).ToString();
                        ViewBag.Bugun = bugun;
                        ViewBag.GunSonu = gunSonu;
                        statusButton = "tarihAraligi";
                    }
                }
                #endregion DateFilter 

                //ViewBag.Gun = GelenGun.Day.ToString();
                ViewBag.LimitSize = 5;
                ViewBag.StatusButton = statusButton;

                #region GetExportData
                //ExportController ıst = new ExportController();
                //ıst.ExportExcel();
                ////List<KasaCiroReportSendMailViewModel> list = SendMailKasaCiroCRUD.List(Convert.ToDateTime("2019-09-01 00:00:00"), Convert.ToDateTime("2019-09-15 00:00:00"));
                ////Singleton.ListtoDataTableConverter converter = new Singleton.ListtoDataTableConverter();
                ////DataTable dt = converter.ToDataTable(list);
                ////Singleton sq = new Singleton();
                ////sq.ExportExcel(dt);
                #endregion
            }
            catch (Exception)
            {
                try
                { return Redirect("/Authentication/Login"); }
                catch (Exception) { }
            }

            return View();
        }

        #region ŞUBEYE GÖRE CİRO DAĞILIMI  (PİE)
        public JsonResult SubstationTotalAmountPie(string StartDateTime, string EndDateTime)
        {
            string ID = Request.Cookies["PRAUT"].Value;

            JsonResultModel result = new JsonResultModel();
            result.IsSuccess = false;

            #region START-END DATETİME
            DateTime NowDate = System.DateTime.Now;
            #region Geçici olarak eklendi
            //int Dayfirst = NowDate.Day;
            //DateTime StartDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1); // Ay ilk günü
            //if (Dayfirst == 1)
            //{
            //    DateTime StartDate2 = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1); // Ay ilk günü
            //    StartDate = StartDate2.AddMonths(-1);
            //}
            #endregion Geçici olarak eklendi
            //DateTime EndDate = StartDate.AddMonths(1).AddDays(-1); // Ay son günü
            //string str_StartDate = StartDate.ToString("yyyy-MM-dd 00:00:00");
            //string str_EndDate = EndDate.ToString("yyyy-MM-dd 23:59:59");
            //DateTime new_Str_StartDate = new DateTime(DateTime.Now.Year, (DateTime.Now.Month - 1), 1);
            //DateTime new_Str_EndDate = new_Str_StartDate.AddMonths(1).AddDays(-1); // Ay son günü
            //string new_Str_StartDate_ = new_Str_StartDate.ToString("yyyy-MM-dd 00:00:00");
            //string new_Str_EndDate_ = new_Str_EndDate.ToString("yyyy-MM-dd 23:59:59");
            ////var MonthNameNow = Singleton.getObject().getMonthFormDateNow(new_Str_StartDate);
            ////var MonthNamePrivious = Singleton.getObject().getMonthFormDatePrivious(new_Str_StartDate);
            #endregion START-END DATETİME

            #region NEW SQL WEB REPORT

            #region Secilen Tarih Filterelemesine Göre Yapılan İşlemler           
            //string tarihAraligiStartDate = "";
            //string tarihAraligiEndDate = "";
            //string subeid = "";
            //string durum = "";
            //try
            //{
            //    if (Request.QueryString["durum"] != null)
            //        durum = Request.QueryString["durum"].ToString();
            //    if (Request.QueryString["tarihBas"] != null && Request.QueryString["tarihBitis"] != null)
            //    {
            //        tarihAraligiStartDate = Request.QueryString["tarihBas"].ToString();
            //        tarihAraligiEndDate = Request.QueryString["tarihBitis"].ToString();
            //    }
            //    if (Request.QueryString["subeid"] != null)
            //    {
            //        subeid = Request.QueryString["subeid"].ToString();
            //    }
            //}
            //catch (Exception ex)
            //{
            //    Singleton.WritingLogFile("SubeUrunlerController_Details", ex.Message.ToString());
            //}
            //TimeViewModel viewModel = Singleton.GetTimeViewModel(tarihAraligiStartDate, tarihAraligiEndDate, durum);
            #endregion Secilen Tarih Filterelemesine Göre Yapılan İşlemler 

            var NameList = new List<string>();
            var DataList = new List<Dictionary<string, string>>();

            List<SubeCiro> list = ChartsReportsSubeCiroCRUD.List(Convert.ToDateTime(StartDateTime), Convert.ToDateTime(EndDateTime), ID);
            List<SubeCiro> CiroyaGoreListe = list.OrderByDescending(o => o.Ciro).ToList();

            #endregion NEW SQL WEB REPORT

            if (list.Count > 0)
            {
                if (list.Count != -1)
                {
                    foreach (var item in CiroyaGoreListe)
                    {
                        Dictionary<string, string> countDataList = new Dictionary<string, string>();
                        NameList.Add(item.Sube.ToString());
                        countDataList.Add("name", item.Sube.ToString());
                        countDataList.Add("y", item.Ciro.ToString().Replace(",", "."));
                        DataList.Add(countDataList);
                    }
                    ////if (dataSabstationTable.Count > FilterData)
                    ////{
                    ////    List<DashboardSabstationTotalAmoun> Test2 = new List<DashboardSabstationTotalAmoun>();
                    ////    Test2 = dataSabstationTable.Skip(Convert.ToInt32(FilterData)).ToList();
                    ////    var Test3 = Test2.Sum(x => x.TOTAL_AMOUNT);
                    ////    Dictionary<string, string> countDataList2 = new Dictionary<string, string>();
                    ////    NameList.Add("Diğer");
                    ////    countDataList2.Add("name", "Diğer");
                    ////    countDataList2.Add("y", Test3.ToString());
                    ////    DataList.Add(countDataList2);
                    ////}
                }
                else
                {
                    ////foreach (var item in dataSabstationTable)
                    ////{
                    ////    Dictionary<string, string> countDataList = new Dictionary<string, string>();
                    ////    NameList.Add(item.SUBSTATION_NAME.ToString());
                    ////    countDataList.Add("name", item.SUBSTATION_NAME.ToString());
                    ////    countDataList.Add("y", item.TOTAL_AMOUNT.ToString());
                    ////    DataList.Add(countDataList);
                    ////}
                }
            }
            return Json(new
            {
                Name = NameList,
                Data = DataList
            }, JsonRequestBehavior.AllowGet);
            //}
        }

        #endregion ŞUBEYE GÖRE CİRO DAĞILIMI  (PİE)

        #region ÖDEME TİPLERİNE GÖRE CİRO DAĞILIMI **** (LİNE)
        [HttpGet]
        public JsonResult PaymentTaypeTotalAmount(string StartDateTime, string EndDateTime)
        {
            string ID = Request.Cookies["PRAUT"].Value;
            JsonResultModel result = new JsonResultModel();
            result.IsSuccess = false;

            var NameList = new List<string>();
            var CategoriesList = new List<string>();
            var DataList = new List<Dictionary<string, string>>();
            var DataDetailList = new List<string>();
            Dictionary<String, List<decimal>> abc = new Dictionary<string, List<decimal>>();

            #region Secilen Tarih Filterelemesine Göre Yapılan İşlemler           
            //string tarihAraligiStartDate = "";
            //string tarihAraligiEndDate = "";
            //string subeid = "";
            //string durum = "";
            //try
            //{
            //    if (Request.QueryString["durum"] != null)
            //        durum = Request.QueryString["durum"].ToString();
            //    if (Request.QueryString["tarihBas"] != null && Request.QueryString["tarihBitis"] != null)
            //    {
            //        tarihAraligiStartDate = Request.QueryString["tarihBas"].ToString();
            //        tarihAraligiEndDate = Request.QueryString["tarihBitis"].ToString();
            //    }

            //    if (Request.QueryString["subeid"] != null)
            //    {
            //        subeid = Request.QueryString["subeid"].ToString();
            //    }
            //}
            //catch (Exception ex)
            //{
            //    Singleton.WritingLogFile("SubeUrunlerController_Details", ex.Message.ToString());
            //}
            //TimeViewModel viewModel = Singleton.GetTimeViewModel(tarihAraligiStartDate, tarihAraligiEndDate, durum);
            #endregion Secilen Tarih Filterelemesine Göre Yapılan İşlemler 

            List<SubeCiro> SubeCiro = ChartsReportsSubeCiroCRUD.List(Convert.ToDateTime(StartDateTime), Convert.ToDateTime(EndDateTime), ID);
            //var nakit = SubeCiro.Sum(x => x.Cash);
            //var kredi = SubeCiro.Sum(x => x.Credit);
            //var yemekKartı = SubeCiro.Sum(x => x.Ticket);

            if (SubeCiro != null)
            {
                NameList.Add("Nakit");
                NameList.Add("Kredi");
                NameList.Add("Yemek Kartı");

                abc.Add("Nakit", new List<decimal>
                { });
                abc.Add("Kredi", new List<decimal>
                { });
                abc.Add("Yemek Kartı", new List<decimal>
                { });

                foreach (var item in SubeCiro)
                {
                    Dictionary<string, string> countDataList = new Dictionary<string, string>();
                    CategoriesList.Add(item.Sube);

                    countDataList.Add("name", "Nakit ");
                    countDataList.Add("y", item.Cash.ToString().Replace(",", "."));
                    DataList.Add(countDataList);

                    Dictionary<string, string> countDataList2 = new Dictionary<string, string>();
                    countDataList2.Add("name", "Kredi ");
                    countDataList2.Add("y", item.Credit.ToString().Replace(",", "."));
                    //countDataList2.Add("y", item.toplam.Value);
                    DataList.Add(countDataList2);

                    Dictionary<string, string> countDataList3 = new Dictionary<string, string>();
                    countDataList3.Add("name", "Yemek Kartı ");
                    countDataList3.Add("y", item.Ticket.ToString().Replace(",", "."));
                    //countDataList2.Add("y", item.toplam.Value);
                    DataList.Add(countDataList3);

                    abc["Nakit"].Add(Convert.ToDecimal(item.Cash));
                    abc["Kredi"].Add(Convert.ToDecimal(item.Credit));
                    abc["Yemek Kartı"].Add(Convert.ToDecimal(item.Ticket));
                }
            }
            return Json(new
            {
                Categories = CategoriesList,
                Name = NameList,
                Data = abc
            }, JsonRequestBehavior.AllowGet);
            //}
        }

        #endregion ÖDEME TİPLERİNE GÖRE CİRO DAĞILIMI **** (LİNE)

        #region ŞUBEDE ENÇOK SATILAN URUN DAĞILIMI  (PİE)
        public JsonResult KuryeCiroAmountPie(string StartDateTime, string EndDateTime, int FilterData)
        {
            #region YETKİLİ ID (Left Menude filtrele de yapıyorum)
            string ID = Request.Cookies["PRAUT"].Value;
            ViewBag.YetkiliID = ID;
            #endregion

            JsonResultModel result = new JsonResultModel();
            result.IsSuccess = false;

            #region START - END DATETİME
            //DateTime NowDate = System.DateTime.Now;
            #region Geçici olarak eklendi
            //int Dayfirst = NowDate.Day;
            //DateTime StartDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1); // Ay ilk günü
            //if (Dayfirst == 1)
            //{
            //    DateTime StartDate2 = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1); // Ay ilk günü
            //    StartDate = StartDate2.AddMonths(-1);
            //}
            #endregion
            //DateTime EndDate = StartDate.AddMonths(1).AddDays(-1); // Ay son günü
            //string str_StartDate = StartDate.ToString("yyyy-MM-dd 00:00:00");
            //string str_EndDate = EndDate.ToString("yyyy-MM-dd 23:59:59");
            //DateTime new_Str_StartDate = new DateTime(DateTime.Now.Year, (DateTime.Now.Month - 1), 1);
            //DateTime new_Str_EndDate = new_Str_StartDate.AddMonths(1).AddDays(-1); // Ay son günü
            //string new_Str_StartDate_ = new_Str_StartDate.ToString("yyyy-MM-dd 00:00:00");
            //string new_Str_EndDate_ = new_Str_EndDate.ToString("yyyy-MM-dd 23:59:59");

            //var MonthNameNow = Singleton.getObject().getMonthFormDateNow(new_Str_StartDate);
            //var MonthNamePrivious = Singleton.getObject().getMonthFormDatePrivious(new_Str_StartDate);
            #endregion

            var NameList = new List<string>();
            var DataList = new List<Dictionary<string, string>>();

            #region NEW SQL WEB REPORT          
            List<SubeUrun> list = SubeUrunCRUD.List(Convert.ToDateTime(StartDateTime), Convert.ToDateTime(EndDateTime), "", ID, "1", "");
            List<SubeUrun> CiroyaGoreListe = list.OrderByDescending(o => o.Debit).ToList();
            //var nakit = SubeCiro.Sum(x => x.Cash);
            //var kredi = SubeCiro.Sum(x => x.Credit);
            #endregion

            //if (list.Count > 0)
            //{
            //var ttt = SubeCiro.OrderByDescending(x => x.Cash);
            if (FilterData != -1)
            {
                var Test1 = CiroyaGoreListe.Skip(0).Take(Convert.ToInt32(FilterData)).ToList();
                foreach (var item in Test1)
                {
                    Dictionary<string, string> countDataList = new Dictionary<string, string>();
                    NameList.Add(item.Sube.ToString());
                    countDataList.Add("name", "(" + item.Sube + ") " + item.ProductName);
                    countDataList.Add("y", item.Debit.ToString().Replace(",", "."));
                    DataList.Add(countDataList);

                    //Dictionary<string, string> countDataList1 = new Dictionary<string, string>();
                    //NameList.Add(item.Sube.ToString());
                    //countDataList1.Add("name", item.ProductName);
                    //countDataList1.Add("y", item.Miktar.ToString());
                    //DataList.Add(countDataList1);
                }
                ////if (dataSabstationTable.Count > FilterData)
                ////{
                ////    List<DashboardSabstationTotalAmoun> Test2 = new List<DashboardSabstationTotalAmoun>();
                ////    Test2 = dataSabstationTable.Skip(Convert.ToInt32(FilterData)).ToList();
                ////    var Test3 = Test2.Sum(x => x.TOTAL_AMOUNT);
                ////    Dictionary<string, string> countDataList2 = new Dictionary<string, string>();
                ////    NameList.Add("Diğer");
                ////    countDataList2.Add("name", "Diğer");
                ////    countDataList2.Add("y", Test3.ToString());
                ////    DataList.Add(countDataList2);
                ////}
            }
            else
            {
                foreach (var item in CiroyaGoreListe)
                {
                    Dictionary<string, string> countDataList = new Dictionary<string, string>();
                    NameList.Add(item.Sube.ToString());
                    countDataList.Add("name", "(" + item.Sube + ") " + item.ProductName);
                    countDataList.Add("y", item.Debit.ToString().Replace(",", "."));
                    DataList.Add(countDataList);
                }
            }
            //}
            return Json(new
            {
                Name = NameList,
                Data = DataList
            }, JsonRequestBehavior.AllowGet);
            //}
        }

        #endregion ŞUBEDE ENÇOK SATILAN URUN DAĞILIMI  (PİE)

        #region KULLANILACAK CHARTS    

        #region GEREKLİ OLURSA KULLAN !!!  (PİE)
        /*
        [HttpGet]
        public JsonResult KuponPie()
        {
            //var FilterData = 100;
            //JsonResultModel result = new JsonResultModel();
            //result.IsSuccess = false;

            #region START - END DATETİME

            DateTime NowDate = System.DateTime.Now;

            #region Geçici olarak eklendi
            //int Dayfirst = NowDate.Day;

            //DateTime StartDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1); // Ay ilk günü
            //if (Dayfirst == 1)
            //{
            //    DateTime StartDate2 = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1); // Ay ilk günü
            //    StartDate = StartDate2.AddMonths(-1);
            //}
            #endregion

            //DateTime EndDate = StartDate.AddMonths(1).AddDays(-1); // Ay son günü
            //string str_StartDate = StartDate.ToString("yyyy-MM-dd 00:00:00");
            //string str_EndDate = EndDate.ToString("yyyy-MM-dd 23:59:59");

            //DateTime new_Str_StartDate = new DateTime(DateTime.Now.Year, (DateTime.Now.Month - 1), 1);
            //DateTime new_Str_EndDate = new_Str_StartDate.AddMonths(1).AddDays(-1); // Ay son günü
            //string new_Str_StartDate_ = new_Str_StartDate.ToString("yyyy-MM-dd 00:00:00");
            //string new_Str_EndDate_ = new_Str_EndDate.ToString("yyyy-MM-dd 23:59:59");

            //var MonthNameNow = Singleton.getObject().getMonthFormDateNow(new_Str_StartDate);
            //var MonthNamePrivious = Singleton.getObject().getMonthFormDatePrivious(new_Str_StartDate);
            #endregion

            //var NameList = new List<string>();
            //var DataList = new List<Dictionary<string, string>>();
            var DataList = new List<DashboardTEST>();
            //List<Object[]> DataList = new List<Object[]>();
            List<Object[]> res = new List<Object[]>();
            List<string> NameList = new List<string>();

            //var dataList = new List<DashboardTEST>();
            Dictionary<String, List<int>> abc = new Dictionary<string, List<int>>();

            //using (CRM_SYSTEMEntities myEntity = new CRM_SYSTEMEntities())
            //{
            #region SQL
            //string sqlCouponsCounts = @"with sorgu as (
            //                      SELECT
            //                       (select count(*) from CUSTOMERS_WINNED_PRODUCT) as TotalCouponsCount,
            //                       (select count(*) from CUSTOMERS_WINNED_PRODUCT CWP2 where CWP2.STATUS = 1 and couponaction != 10) as UsedCouponsCount,
            //                       (select count(*) from CUSTOMERS_WINNED_PRODUCT CWP2 where CWP2.STATUS = 0) as NotUsedCouponsCount,
            //                       (select count(*) from CUSTOMERS_WINNED_PRODUCT CWP2 where CWP2.STATUS = 1 and couponaction = 10) as DateEndCouponsCount			
            //                         )
            //                         SELECT * FROM sorgu
            //                            ";
            //var dataCouponsCounts = myEntity.Database.SqlQuery<DashboardCouponsCounts>(sqlCouponsCounts).ToList();
            #endregion
            //List<SubeCiro> list = SubeCiroCRUD.List(Convert.ToDateTime("28.8.2019 04:00"), Convert.ToDateTime("28.8.2019 23:59"));
            List<SubeUrun> list = SubeUrunCRUD.List(Convert.ToDateTime("01/09/2019 04:01"), Convert.ToDateTime("30/09/2019 04:01"), "41");
            List<SubeUrun> CiroyaGoreListe = list.OrderByDescending(o => o.Miktar).ToList();
                int TotalValue = 0;
                //if (sqlCouponsCounts != null)
                //{
                    foreach (var item in CiroyaGoreListe)
                    {
                        //NameList.Add("Toplam Kupon");
                        //NameList.Add("Aktif Kullanıcı");
                        //NameList.Add("Deneme");
                        NameList.Add(item.ProductName);

                        TotalValue = 55;

                        foreach (var item1 in NameList)
                        {

                            if (item1 == item.ProductName)
                            {
                                Object[] array1 = new Object[] { item1, item.Miktar};
                                res.Add(array1);
                            }
                            //else if (item1 == "Deneme")
                            //{
                            //    Object[] array1 = new Object[] { item1,item.iptal };
                            //    res.Add(array1);
                            //}
                            //else if (item1 == "SKT Sayisi")
                            //{
                            //    Object[] array1 = new Object[] { item1, 30 };
                            //    res.Add(array1);
                            //}
                        }
                    }
                //}
                return Json(new
                {
                    TotalValue = TotalValue,
                    Name = NameList,
                    Data = res.ToArray()
                }, JsonRequestBehavior.AllowGet);
            //}
        }

       */
        #endregion

        #region MÜŞTERİ GRUBANA GÖRE AYLIK SATIŞ İSTATİSLİĞİ   (LİNE (LİNE))
        [HttpGet]
        public JsonResult MusteriGrubunaGoreAylikSatis(string EndDate)
        {
            string ID = Request.Cookies["PRAUT"].Value;
            JsonResultModel result = new JsonResultModel();
            result.IsSuccess = false;

            var NameList = new List<string>();
            var CategoriesList = new List<string>();
            Dictionary<string, List<decimal?>> dataList = new Dictionary<string, List<decimal?>>();

            #region Secilen Tarih Filterelemesine Göre Yapılan İşlemler   

            string tarihAraligiStartDate = string.Empty;
            string tarihAraligiEndDate = string.Empty;
            string subeid = string.Empty;
            string durum = string.Empty;

            try
            {
                if (Request.QueryString["durum"] != null)
                    durum = Request.QueryString["durum"].ToString();

                if (Request.QueryString["tarihBas"] != null && Request.QueryString["tarihBitis"] != null)
                {
                    tarihAraligiStartDate = Request.QueryString["tarihBas"].ToString();
                    tarihAraligiEndDate = Request.QueryString["tarihBitis"].ToString();
                }

                if (Request.QueryString["subeid"] != null)
                {
                    subeid = Request.QueryString["subeid"].ToString();
                }
            }
            catch (Exception ex) { Singleton.WritingLogFile("SubeUrunlerController_Details", ex.Message.ToString()); }

            TimeViewModel viewModel = Singleton.GetTimeViewModel(tarihAraligiStartDate, tarihAraligiEndDate, durum);

            #endregion Secilen Tarih Filterelemesine Göre Yapılan İşlemler  

            List<KasaCiro> SubeCiro = SubeCiroReportsCRUD.List(Convert.ToDateTime("2019-12-01 00:00:00"), Convert.ToDateTime("2019-12-31 23:59:59"), ID, "");
            List<KasaCiro> CiroyaGoreListe = SubeCiro.OrderByDescending(o => o.ToplamCiro).ToList();
            var dataGrpupList = (SubeCiro).GroupBy(x => x.Sube).ToList();

            if (CiroyaGoreListe != null)
            {
                //var dataGrpupList = (from cg in myEntity.CUSTOMER_GROUPS where cg.CRITER != "TEST" && cg.CRITER != "RESTO" select cg).GroupBy(x => x.CRITER).ToList();
                //var dataGrpupList = dataCustomerGroupCost.Select(x => x.CRITER).ToList();

                for (int i = 1; i <= 31; i++)
                {
                    CategoriesList.Add(i.ToString());
                }

                foreach (var group in dataGrpupList)
                {
                    var dataCriterList = SubeCiro.Where(x => x.Sube == group.Key).ToList();

                    List<decimal?> nowVal = new List<decimal?>();

                    for (int i = 1; i <= 31; i++)
                    {
                        decimal? nowTotal = null;
                        var dataDayResult = dataCriterList.Where(x => x.DayNumber == i).FirstOrDefault();

                        if (dataDayResult != null)
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
            /*
                         for (int i = 1; i <= 31; i++)
                       {
                           CategoriesList.Add(i.ToString());
                       }

                       Dictionary<int, List<decimal>> groupData = new Dictionary<int, List<decimal>>();

                       foreach (var group in CiroyaGoreListe)
                       {
                           //var dataCriterList = dataCustomerGroupCost.Where(x => x.CRITER == group.Key).ToList();

                           List<decimal?> nowVal = new List<decimal?>();

                           for (int i = 1; i <= 31; i++)
                           {
                               decimal? nowTotal = null;
                               //var dataDayResult = dataCriterList.Where(x => x.DayNumber == i).FirstOrDefault();

                               //if (dataDayResult != null)
                               //{
                               nowTotal = Convert.ToDecimal(group.ToplamCiro); //dataDayResult.TotalCost2;
                                                        //}
                               nowVal.Add(nowTotal);
                           }

                           #region Value Kontrol // Bütün değerleri null olan grubun verileri eklenmiyor
                           var valControl = nowVal.Where(x => x != null).ToList();

                           if (valControl.Count > 0)
                           {
                               dataList.Add(group.Sube.ToString(), nowVal);
                               NameList.Add(group.Sube);
                           }
                           #endregion
                       }
                       //}

           */
            return Json(new
            {
                Categories = CategoriesList,
                Name = NameList,
                Data = dataList,
            }, JsonRequestBehavior.AllowGet);
            //}
        }

        #endregion MÜŞTERİ GRUBANA GÖRE AYLIK SATIŞ İSTATİSLİĞİ   (LİNE (LİNE))

        #endregion KULLANILACAK CHARTS
    }
}
