using SefimV2.Helper;
using SefimV2.Models;
using SefimV2.Models.ReportsDetailCRUD;
using SefimV2.Repository;
using SefimV2.ViewModels.GetTime;
using SefimV2.ViewModels.ReportsDetail;
using SefimV2.ViewModels.Result;
using SefimV2.ViewModels.User;
using SefimV2.ViewModels.WaiterSales;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Web.Configuration;
using System.Web.Mvc;

namespace SefimV2.Controllers
{
    [CheckLoggedIn]
    public class ReportsDetailController : Controller
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

        //
        #region ŞUBELERİN (SUBE CİRO) SATIŞ İSTATİSLİĞİ   (LİNE)

        [HttpGet]
        public JsonResult SubeCiroSatisIstatistik(string StartDate, string EndDate, int FilterData)
        {
            ViewBag.LimitSize = FilterData;
            string ID = Request.Cookies["PRAUT"].Value;
            JsonResultModel result = new JsonResultModel();
            result.IsSuccess = false;

            var NameList = new List<string>();
            var CategoriesList = new List<string>();
            Dictionary<string, List<decimal?>> dataList = new Dictionary<string, List<decimal?>>();

            #region Secilen Tarih Filterelemesine Göre Yapılan İşlemler

            string tarihAraligiStartDate = "";
            string tarihAraligiEndDate = "";
            string subeid = "";
            string durum = "";

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
            catch (Exception ex)
            {
                Singleton.WritingLogFile("SubeUrunlerController_Details", ex.Message.ToString());
            }

            TimeViewModel viewModel = Singleton.GetTimeViewModel(tarihAraligiStartDate, tarihAraligiEndDate, durum);
            #endregion

            var saat = Ayarlar.SaatListe();

            var startDate = Convert.ToDateTime(StartDate).ToString("dd'/'MM'/'yyyy " + saat.StartTime + "");
            var endDate = Convert.ToDateTime(EndDate).ToString("dd'/'MM'/'yyyy " + saat.EndTime + "");
            List<KasaCiro> SubeCiro = SubeCiroReportsCRUD.List(Convert.ToDateTime(startDate), Convert.ToDateTime(endDate), ID, "");
            List<KasaCiro> CiroyaGoreListe = SubeCiro.OrderByDescending(o => o.ToplamCiro).ToList();
            var dataGrpupList = (SubeCiro).GroupBy(x => x.Sube).ToList();

            if (CiroyaGoreListe != null)
            {
                #region TOTAL DAY

                TimeSpan t_s = Convert.ToDateTime(StartDate) - Convert.ToDateTime(EndDate);
                int z_ = Math.Abs(t_s.Days);
                //aptal işlemler.
                if (z_ == 1)
                {
                    z_ = 2;
                }
                if (z_ < 2)
                {
                    if (t_s.Hours == -23)
                    {
                        z_ = 1;
                    }
                }

                #endregion TOTAL DAY
                //for (int i = 1; i <= z_; i++)
                //{
                //    #region ILK ve SON TARIH SET EDILIYOR                
                //    if (i == 1)
                //    {
                //        CategoriesList.Add(Convert.ToDateTime(StartDate).ToString("dd-MM-yyyy") + "-" + i.ToString());
                //    }
                //    else if (i == z_)
                //    {
                //        CategoriesList.Add(Convert.ToDateTime(EndDate).ToString("dd-MM-yyyy") + "-" + i.ToString());
                //    }
                //    else
                //    {
                //        CategoriesList.Add(i.ToString());
                //    }
                //    #endregion ILK ve SON TARIH SET EDILIYOR
                //}

                if (FilterData != -1)
                {
                    var Test1 = dataGrpupList.Skip(0).Take(Convert.ToInt32(FilterData)).ToList();
                    foreach (var group in Test1)
                    {
                        var dataCriterList = SubeCiro.Where(x => x.Sube == group.Key).ToList();

                        List<decimal?> nowVal = new List<decimal?>();

                        //for (int i = 1; i <= z_; i++)
                        //{
                        //    #region CategoriesList / ILK ve SON TARIH SET EDILIYOR                
                        //    if (i == 1)
                        //    {
                        //        CategoriesList.Add(Convert.ToDateTime(StartDate).ToString("dd-MM-yyyy") + "-" + i.ToString());
                        //    }
                        //    else if (i == z_)
                        //    {
                        //        CategoriesList.Add(Convert.ToDateTime(EndDate).ToString("dd-MM-yyyy") + "-" + i.ToString());
                        //    }
                        //    else
                        //    {
                        //        CategoriesList.Add(i.ToString());
                        //    }
                        //    #endregion CategoriesList / ILK ve SON TARIH SET EDILIYOR

                        //    decimal? nowTotal = null;
                        //    var dataDayResult = dataCriterList.Where(x => x.DayCount == i).FirstOrDefault();

                        //    if (dataDayResult != null)
                        //    {
                        //        nowTotal = Convert.ToDecimal(dataDayResult.ToplamCiro);
                        //    }
                        //    nowVal.Add(nowTotal);
                        //}

                        for (int i = 0; i < dataCriterList.Count; i++)
                        {
                            CategoriesList.Add("(" + (i + 1).ToString() + ")/" + Convert.ToDateTime(dataCriterList[i].DateStr).ToString("dd-MM-yyyy"));

                            decimal? nowTotal = null;
                            var dataDayResult = dataCriterList.Where(x => x.DayCount == i).FirstOrDefault();

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
                else
                {
                    foreach (var group in dataGrpupList)
                    {
                        var dataCriterList = SubeCiro.Where(x => x.Sube == group.Key).ToList();

                        List<decimal?> nowVal = new List<decimal?>();

                        for (int i = 1; i <= z_; i++)
                        {

                            decimal? nowTotal = null;
                            var dataDayResult = dataCriterList.Where(x => x.DayCount == i).FirstOrDefault();

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
            }
            return Json(new
            {
                Categories = CategoriesList,
                Name = NameList,
                Data = dataList,
            }, JsonRequestBehavior.AllowGet);
        }

        #endregion ŞUBELERİN (SUBE CİRO) SATIŞ İSTATİSLİĞİ   (LİNE)

        //

        #region İki Tarih Aralıklı (SUBE CİRO) SATIŞ İSTATİSLİĞİ   (LİNE)

        [HttpGet]
        public JsonResult IkiTarihAralikliKarsilastirmaRaporu(string StartDate, string EndDate, string SubeId_A)
        {
            ViewBag.LimitSize = SubeId_A;
            string ID = Request.Cookies["PRAUT"].Value;
            JsonResultModel result = new JsonResultModel();
            result.IsSuccess = false;

            var NameList = new List<string>();
            var CategoriesList = new List<string>();
            Dictionary<string, List<decimal?>> dataList = new Dictionary<string, List<decimal?>>();

            #region Secilen Tarih Filterelemesine Göre Yapılan İşlemler           
            string tarihAraligiStartDate = "";
            string tarihAraligiEndDate = "";
            string subeid = "";
            string durum = "";


            DateTime yilSonGunu = Convert.ToDateTime(StartDate);
            var lastDayOfMonth = DateTime.DaysInMonth(yilSonGunu.Year, yilSonGunu.Month);
            EndDate = yilSonGunu.Year + "." + yilSonGunu.Month + "." + lastDayOfMonth + " 23:59:59";

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
            catch (Exception ex)
            {
                Singleton.WritingLogFile("SubeUrunlerController_Details", ex.Message.ToString());
            }

            TimeViewModel viewModel = Singleton.GetTimeViewModel(tarihAraligiStartDate, tarihAraligiEndDate, durum);
            #endregion

            //tüm şubeler ve secili şube oldugunda, burdaki iş kuralı uygulanıyor.
            var subeIdSplit = SubeId_A.Split(',');
            if (subeIdSplit.Length > 0 && subeIdSplit[0] == "0")
                SubeId_A = "0";

            // SubeId_A sıfıra eşit olmadığı durumlarda and Id in (şubeler) setleniyor.Değilse tüm şubeler çağrılıyor.
            if (SubeId_A != "0")
                SubeId_A = "and Id in (" + SubeId_A + ")";
            else
                SubeId_A = "";
            List<KasaCiro> SubeCiro = SubeCiroReportsCRUD.List(Convert.ToDateTime(Convert.ToDateTime(StartDate).ToString("dd'/'MM'/'yyyy HH:mm")), Convert.ToDateTime(Convert.ToDateTime(EndDate).ToString("dd'/'MM'/'yyyy HH:mm")), ID, SubeId_A);
            List<KasaCiro> CiroyaGoreListe = SubeCiro.OrderByDescending(o => o.ToplamCiro).ToList();
            var dataGrpupList = (SubeCiro).GroupBy(x => x.Sube).ToList();

            if (CiroyaGoreListe != null)
            {
                var testK = CiroyaGoreListe.Sum(x => Convert.ToDouble(x.ToplamCiro));
                #region CategoriesList / ILK ve SON TARIH SET EDILIYOR                
                for (int i = 1; i <= 31; i++)
                {
                    //CategoriesList.Add(i.ToString());
                    CategoriesList.Add(i.ToString() + "-" + yilSonGunu.Month + "-" + yilSonGunu.Year);
                }
                #endregion CategoriesList / ILK ve SON TARIH SET EDILIYOR

                if (SubeId_A != "0" && !string.IsNullOrWhiteSpace(SubeId_A))
                {
                    //var Test1 = dataGrpupList.Skip(0).Take(Convert.ToInt32(SubeId_A)).ToList();

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
                else
                {
                    foreach (var group in dataGrpupList)
                    {
                        var dataCriterList = SubeCiro.Where(x => x.Sube == group.Key).ToList();

                        List<decimal?> nowVal = new List<decimal?>();

                        for (int i = 1; i <= 31; i++)
                        {

                            decimal? nowTotal = null;
                            var dataDayResult = dataCriterList.Where(x => x.DayCount == i).FirstOrDefault();

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
            }
            return Json(new
            {
                Categories = CategoriesList,
                Name = NameList,
                Data = dataList,
            }, JsonRequestBehavior.AllowGet);
            //}
        }

        #endregion İki Tarih Aralıklı (SUBE CİRO) SATIŞ İSTATİSLİĞİ   (LİNE)

        //

        //
        #region Ürün Satış istatistikleri

        public JsonResult CustomerState([DefaultValue(0)] int Limit)
        {
            #region YETKİLİ ID (Left Menude filtrele de yapıyorum)

            bool cookieExists = HttpContext.Request.Cookies["PRAUT"] != null;
            if (cookieExists == false)
            {
                //return Redirect("/Authentication/Login");
            }
            string ID = Request.Cookies["PRAUT"].Value;
            ViewBag.YetkiliID = ID;
            #endregion

            #region Secilen Tarih Filterelemesine Göre Yapılan İşlemler           
            string tarihAraligiStartDate = "";
            string tarihAraligiEndDate = "";
            string subeid = "";
            string durum = "";

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
            catch (Exception ex)
            {
                Singleton.WritingLogFile("SubeUrunlerController_Details", ex.Message.ToString());
            }
            TimeViewModel viewModel = Singleton.GetTimeViewModel(tarihAraligiStartDate, tarihAraligiEndDate, durum);

            ViewBag.StartDateTime = viewModel.StartDate;
            ViewBag.EndDateTime = viewModel.EndDate;
            ViewBag.Pages = "Urun Satis Raporu";
            ViewBag.PageNavi = "Ürün Satış Raporu";
            ViewBag.SubeId = subeid;
            #endregion

            List<SubeUrun> dataList = SubeUrunCRUD.List(Convert.ToDateTime(viewModel.StartDate), Convert.ToDateTime(viewModel.EndDate), subeid, ID, "", "");

            if (Limit > 100) { Limit = 100; }

            DateTime DateNow = System.DateTime.Now;
            string EndDate = DateNow.ToString("yyyy-MM-dd");
            string StartDate = DateNow.AddDays(-30).ToString("yyyy-MM-dd");

            return Json(new
            {
                StartDate = StartDate,
                EndDate = EndDate,
                Data = dataList,
            }, JsonRequestBehavior.AllowGet);
        }

        #endregion Ürün Satış istatistikleri

        //


        //
        #region ŞUBEDE ENÇOK SATILAN URUN DAĞILIMI  (PİE) SubstationsProductsSales

        public JsonResult SubstationsProductsSalesPie(string StartDateTime, string EndDateTime, int FilterData, string SubeId)
        {
            #region YETKİLİ ID (Left Menude filtrele de yapıyorum)
            string ID = Request.Cookies["PRAUT"].Value;
            ViewBag.YetkiliID = ID;
            #endregion

            JsonResultModel result = new JsonResultModel();
            result.IsSuccess = false;

            var NameList = new List<string>();
            var DataList = new List<Dictionary<string, string>>();

            #region NEW SQL WEB REPORT          
            List<SubeUrun> list = SubeUrunReportsCRUD.List(Convert.ToDateTime(Convert.ToDateTime(StartDateTime).ToString("dd'/'MM'/'yyyy HH:mm")), Convert.ToDateTime(Convert.ToDateTime(EndDateTime).ToString("dd'/'MM'/'yyyy HH:mm")), SubeId, ID, "");
            List<SubeUrun> CiroyaGoreListe = list.OrderByDescending(o => o.Debit).ToList();
            //var nakit = SubeCiro.Sum(x => x.Cash);
            //var kredi = SubeCiro.Sum(x => x.Credit);
            #endregion NEW SQL WEB REPORT

            //if (list.Count > 0)
            //{
            //var ttt = SubeCiro.OrderByDescending(x => x.Cash);
            if (FilterData != -1)
            {
                var Test1 = CiroyaGoreListe.Skip(0).Take(Convert.ToInt32(FilterData)).ToList();

                #region SUBEDEKI FilterData 'e GORE URUNLER LİSTELENİYOR

                if (SubeId == "0")
                {
                    foreach (var item in Test1)
                    {
                        Dictionary<string, string> countDataList = new Dictionary<string, string>();
                        NameList.Add(item.Sube.ToString());
                        countDataList.Add("name", "(" + item.Sube + ") " + item.ProductName + "");
                        countDataList.Add("y", item.Debit.ToString().Replace(",", ".") + "&" + item.Miktar.ToString());
                        DataList.Add(countDataList);
                    }
                }
                else
                {
                    foreach (var item in Test1)
                    {
                        Dictionary<string, string> countDataList = new Dictionary<string, string>();
                        NameList.Add(item.Sube.ToString());
                        countDataList.Add("name", item.ProductName);
                        countDataList.Add("y", item.Debit.ToString().Replace(",", ".") + "&" + item.Miktar.ToString());
                        DataList.Add(countDataList);

                        //Dictionary<string, string> countDataList1 = new Dictionary<string, string>();
                        //NameList.Add(item.Sube.ToString());
                        //countDataList1.Add("name", item.ProductName);
                        //countDataList1.Add("y", item.Miktar.ToString());
                        //DataList.Add(countDataList1);
                    }
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
                #endregion SUBEDEKI FilterData 'e GORE URUNLER LİSTELENİYOR
            }
            else
            {
                #region ŞUBEDEKI TÜM URUNLER LİSTELENIYOR              
                if (SubeId == "0")
                {
                    foreach (var item in CiroyaGoreListe)
                    {
                        Dictionary<string, string> countDataList = new Dictionary<string, string>();
                        NameList.Add(item.Sube.ToString());
                        countDataList.Add("name", "(" + item.Sube + ") " + item.ProductName + "");
                        countDataList.Add("y", item.Debit.ToString().Replace(",", ".") + "&" + item.Miktar.ToString());
                        DataList.Add(countDataList);
                    }
                }
                else
                {
                    foreach (var item in CiroyaGoreListe)
                    {
                        //Dictionary<string, string> countDataList = new Dictionary<string, string>();
                        //NameList.Add(item.Sube.ToString());
                        //countDataList.Add("name", "(" + item.Sube + ") " + item.ProductName);
                        //countDataList.Add("y", item.Debit.ToString());
                        //DataList.Add(countDataList);
                        Dictionary<string, string> countDataList = new Dictionary<string, string>();
                        NameList.Add(item.Sube.ToString());
                        countDataList.Add("name", item.ProductName);
                        countDataList.Add("y", item.Debit.ToString().Replace(",", ".") + "&" + item.Miktar.ToString());
                        DataList.Add(countDataList);
                    }
                }
                #endregion ŞUBEDEKI TÜM URUNLER LİSTELENIYOR
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

        //

        //
        #region ŞUBEDE ENÇOK SATIŞ YAPAN KASİYER DAĞILIMI  (PİE) SubstationsPersonSalesPie

        public JsonResult SubstationsPersonSalesPie(string StartDateTime, string EndDateTime, int FilterData, string SubeId)
        {
            #region YETKİLİ ID (Left Menude filtrele de yapıyorum)
            string ID = Request.Cookies["PRAUT"].Value;
            ViewBag.YetkiliID = ID;
            #endregion

            JsonResultModel result = new JsonResultModel();
            result.IsSuccess = false;

            var NameList = new List<string>();
            var DataList = new List<Dictionary<string, string>>();

            #region NEW SQL WEB REPORT          
            List<PersonelCiro> list = SubePersonelReportsCRUD.List(Convert.ToDateTime(Convert.ToDateTime(StartDateTime).ToString("dd'/'MM'/'yyyy HH:mm")), Convert.ToDateTime(Convert.ToDateTime(EndDateTime).ToString("dd'/'MM'/'yyyy HH:mm")), SubeId, ID);
            List<PersonelCiro> CiroyaGoreListe = list.OrderByDescending(o => o.Total).ToList();
            //var nakit = SubeCiro.Sum(x => x.Cash);
            //var kredi = SubeCiro.Sum(x => x.Credit);
            #endregion NEW SQL WEB REPORT

            //if (list.Count > 0)
            //{
            //var ttt = SubeCiro.OrderByDescending(x => x.Cash);
            if (FilterData != -1)
            {
                var Test1 = CiroyaGoreListe.Skip(0).Take(Convert.ToInt32(FilterData)).ToList();

                #region SUBEDEKI FilterData 'e GORE URUNLER LİSTELENİYOR

                if (SubeId == "0")
                {
                    foreach (var item in Test1)
                    {
                        Dictionary<string, string> countDataList = new Dictionary<string, string>();
                        NameList.Add(item.Sube.ToString());
                        countDataList.Add("name", "(" + item.Sube + ") " + item.ReceivedByUserName + "");
                        countDataList.Add("y", item.Total.ToString().Replace(",", ".") + "&" + item.Discount.ToString());
                        DataList.Add(countDataList);
                    }
                }
                else
                {
                    var Test1_ = CiroyaGoreListe.Skip(0).Take(Convert.ToInt32(FilterData)).FirstOrDefault();
                    foreach (var item in Test1)
                    {
                        Dictionary<string, string> countDataList = new Dictionary<string, string>();
                        NameList.Add(item.Sube.ToString());
                        countDataList.Add("name", item.ReceivedByUserName);
                        countDataList.Add("y", item.Total.ToString().Replace(",", ".") + "&" + item.Discount.ToString());
                        DataList.Add(countDataList);

                    }
                }

                #endregion SUBEDEKI FilterData 'e GORE URUNLER LİSTELENİYOR
            }
            else
            {
                #region ŞUBEDEKI TÜM URUNLER LİSTELENIYOR              
                if (SubeId == "0")
                {
                    foreach (var item in CiroyaGoreListe)
                    {
                        Dictionary<string, string> countDataList = new Dictionary<string, string>();
                        NameList.Add(item.Sube.ToString());
                        countDataList.Add("name", "(" + item.Sube + ") " + item.ReceivedByUserName + "");
                        countDataList.Add("y", item.Total.ToString().Replace(",", ".") + "&" + item.Discount.ToString());
                        DataList.Add(countDataList);
                    }
                }
                else
                {
                    foreach (var item in CiroyaGoreListe)
                    {
                        Dictionary<string, string> countDataList = new Dictionary<string, string>();
                        NameList.Add(item.Sube.ToString());
                        countDataList.Add("name", item.ReceivedByUserName);
                        countDataList.Add("y", item.Total.ToString().Replace(",", ".") + "&" + item.Discount.ToString());
                        DataList.Add(countDataList);
                    }
                }
                #endregion ŞUBEDEKI TÜM URUNLER LİSTELENIYOR
            }
            //}
            return Json(new
            {
                Name = NameList,
                Data = DataList
            }, JsonRequestBehavior.AllowGet);
            //}
        }

        #endregion ŞUBEDE ENÇOK SATIŞ YAPAN KASİYER DAĞILIMI  (PİE) SubstationsPersonSalesPie

        //



        //
        #region ŞUBEDE ENÇOK SATIŞ YAPAN GARSON DAĞILIMI  (PİE) SubstationsWaiterPersonSalesPie

        public JsonResult SubstationsWaiterPersonSalesPie(string StartDateTime, string EndDateTime, int FilterData, string SubeId)
        {
            #region YETKİLİ ID (Left Menude filtrele de yapıyorum)
            string ID = Request.Cookies["PRAUT"].Value;
            ViewBag.YetkiliID = ID;
            #endregion

            JsonResultModel result = new JsonResultModel();
            result.IsSuccess = false;

            var NameList = new List<string>();
            var DataList = new List<Dictionary<string, string>>();

            #region NEW SQL WEB REPORT          
            List<WaiterSalesViewModel> list = SubeGarsonPersonelReportsCRUD.List(Convert.ToDateTime(Convert.ToDateTime(StartDateTime).ToString("dd'/'MM'/'yyyy HH:mm")), Convert.ToDateTime(Convert.ToDateTime(EndDateTime).ToString("dd'/'MM'/'yyyy HH:mm")), SubeId, ID);
            List<WaiterSalesViewModel> CiroyaGoreListe = list.OrderByDescending(o => o.Total).ToList();
            //var nakit = SubeCiro.Sum(x => x.Cash);
            //var kredi = SubeCiro.Sum(x => x.Credit);
            #endregion NEW SQL WEB REPORT

            //if (list.Count > 0)
            //{
            //var ttt = SubeCiro.OrderByDescending(x => x.Cash);
            if (FilterData != -1)
            {
                var Test1 = CiroyaGoreListe.Skip(0).Take(Convert.ToInt32(FilterData)).ToList();

                #region SUBEDEKI FilterData 'e GORE URUNLER LİSTELENİYOR

                if (SubeId == "0")
                {
                    foreach (var item in Test1)
                    {
                        Dictionary<string, string> countDataList = new Dictionary<string, string>();
                        NameList.Add(item.Sube.ToString());
                        countDataList.Add("name", "(" + item.Sube + ") " + item.UserName + "");
                        countDataList.Add("y", item.Total.ToString().Replace(",", "."));
                        DataList.Add(countDataList);
                    }
                }
                else
                {
                    var Test1_ = CiroyaGoreListe.Skip(0).Take(Convert.ToInt32(FilterData)).FirstOrDefault();
                    foreach (var item in Test1)
                    {
                        Dictionary<string, string> countDataList = new Dictionary<string, string>();
                        NameList.Add(item.Sube.ToString());
                        countDataList.Add("name", item.UserName);
                        countDataList.Add("y", item.Total.ToString().Replace(",", "."));
                        DataList.Add(countDataList);

                    }
                }

                #endregion SUBEDEKI FilterData 'e GORE URUNLER LİSTELENİYOR
            }
            else
            {
                #region ŞUBEDEKI TÜM URUNLER LİSTELENIYOR              
                if (SubeId == "0")
                {
                    foreach (var item in CiroyaGoreListe)
                    {
                        Dictionary<string, string> countDataList = new Dictionary<string, string>();
                        NameList.Add(item.Sube.ToString());
                        countDataList.Add("name", "(" + item.Sube + ") " + item.UserName + "");
                        countDataList.Add("y", item.Total.ToString().Replace(",", "."));
                        DataList.Add(countDataList);
                    }
                }
                else
                {
                    foreach (var item in CiroyaGoreListe)
                    {
                        Dictionary<string, string> countDataList = new Dictionary<string, string>();
                        NameList.Add(item.Sube.ToString());
                        countDataList.Add("name", item.UserName);
                        countDataList.Add("y", item.Total.ToString().Replace(",", "."));
                        DataList.Add(countDataList);
                    }
                }
                #endregion ŞUBEDEKI TÜM URUNLER LİSTELENIYOR
            }
            //}
            return Json(new
            {
                Name = NameList,
                Data = DataList
            }, JsonRequestBehavior.AllowGet);
            //}
        }

        #endregion ŞUBEDE ENÇOK SATIŞ YAPAN GARSON DAĞILIMI  (PİE) SubstationsWaiterPersonSalesPie
        //

        #region ÜRÜNE GÖRE ŞUBELERDEKİ SATIŞ İSTATİSLİĞİ (PİE) ProductsSubstationsSalesPie

        public JsonResult ProductsSubstationsSalesPie(string StartDateTime, string EndDateTime, int FilterData, string SubeId, string UrunAdi)
        {
            #region YETKİLİ ID (Left Menude filtrele de yapıyorum)
            string ID = Request.Cookies["PRAUT"].Value;
            ViewBag.YetkiliID = ID;
            #endregion

            JsonResultModel result = new JsonResultModel();
            result.IsSuccess = false;

            var NameList = new List<string>();
            var DataList = new List<Dictionary<string, string>>();

            #region NEW SQL WEB REPORT          
            List<ReportsDetailViewModel> list = UruneGoreSubeSatisReportsCRUD.List(Convert.ToDateTime(Convert.ToDateTime(StartDateTime).ToString("dd'/'MM'/'yyyy HH:mm")), Convert.ToDateTime(Convert.ToDateTime(EndDateTime).ToString("dd'/'MM'/'yyyy HH:mm")), SubeId, ID, UrunAdi);
            List<ReportsDetailViewModel> CiroyaGoreListe = list.OrderByDescending(o => o.Debit).ToList();

            #endregion NEW SQL WEB REPORT

            if (FilterData != -1)
            {
                var Test1 = CiroyaGoreListe.Skip(0).Take(Convert.ToInt32(FilterData)).ToList();

                #region SUBEDEKI FilterData 'e GORE URUNLER LİSTELENİYOR

                if (SubeId == "0")
                {
                    foreach (var item in Test1)
                    {
                        Dictionary<string, string> countDataList = new Dictionary<string, string>();
                        NameList.Add(item.Sube.ToString());
                        countDataList.Add("name", "(" + item.Sube + ") " + item.ProductName + "-" + item.ProductGroup);
                        countDataList.Add("y", item.Miktar.ToString() + "&" + item.Debit.ToString().Replace(",", ".") + "&" + item.ProductGroup);
                        DataList.Add(countDataList);
                    }
                }
                else
                {
                    foreach (var item in Test1)
                    {
                        Dictionary<string, string> countDataList = new Dictionary<string, string>();
                        NameList.Add(item.Sube.ToString());
                        countDataList.Add("name", item.ProductName);
                        countDataList.Add("y", item.Miktar.ToString() + "&" + item.Debit.ToString().Replace(",", "."));
                        DataList.Add(countDataList);

                        //Dictionary<string, string> countDataList1 = new Dictionary<string, string>();
                        //NameList.Add(item.Sube.ToString());
                        //countDataList1.Add("name", item.ProductName);
                        //countDataList1.Add("y", item.Miktar.ToString());
                        //DataList.Add(countDataList1);
                    }
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
                #endregion SUBEDEKI FilterData 'e GORE URUNLER LİSTELENİYOR
            }
            else
            {
                #region ŞUBEDEKI TÜM URUNLER LİSTELENIYOR              
                if (SubeId == "0")
                {
                    foreach (var item in CiroyaGoreListe)
                    {
                        Dictionary<string, string> countDataList = new Dictionary<string, string>();
                        NameList.Add(item.Sube.ToString());
                        countDataList.Add("name", "(" + item.Sube + ") " + item.ProductName + "");
                        countDataList.Add("y", item.Miktar.ToString() + "&" + item.Debit.ToString().Replace(",", "."));
                        DataList.Add(countDataList);
                    }
                }
                else
                {
                    foreach (var item in CiroyaGoreListe)
                    {
                        //Dictionary<string, string> countDataList = new Dictionary<string, string>();
                        //NameList.Add(item.Sube.ToString());
                        //countDataList.Add("name", "(" + item.Sube + ") " + item.ProductName);
                        //countDataList.Add("y", item.Debit.ToString());
                        //DataList.Add(countDataList);
                        Dictionary<string, string> countDataList = new Dictionary<string, string>();
                        NameList.Add(item.Sube.ToString());
                        countDataList.Add("name", item.ProductName);
                        countDataList.Add("y", item.Miktar.ToString() + "&" + item.Debit.ToString().Replace(",", "."));
                        DataList.Add(countDataList);
                    }
                }
                #endregion ŞUBEDEKI TÜM URUNLER LİSTELENIYOR
            }
            //}
            return Json(new
            {
                Name = NameList,
                Data = DataList,
                ToplamSatisTutari = CiroyaGoreListe.Sum(x => x.Debit),
                ToplamSatisAdedi = CiroyaGoreListe.Sum(x => x.Miktar)
            }, JsonRequestBehavior.AllowGet);
            //}
        }

        #endregion ÜRÜNE GÖRE ŞUBELERDEKİ SATIŞ İSTATİSLİĞİ (PİE) ProductsSubstationsSalesPie

        #region  Ürüne Göre Şube Satış İstatistikleri (Liste)

        public JsonResult ProductsSubstationsSalesPie_2(string StartDateTime, string EndDateTime, int FilterData, string SubeId, string UrunAdi)
        {

            #region YETKİLİ ID (Left Menude filtrele de yapıyorum)
            string ID = Request.Cookies["PRAUT"].Value;
            ViewBag.YetkiliID = ID;
            #endregion

            JsonResultModel result = new JsonResultModel();
            result.IsSuccess = false;

            var NameList = new List<string>();
            var DataList = new List<Dictionary<string, string>>();

            #region NEW SQL WEB REPORT          
            List<ReportsDetailViewModel> list = UruneGoreSubeSatisReportsCRUD.List(Convert.ToDateTime(Convert.ToDateTime(StartDateTime).ToString("dd'/'MM'/'yyyy HH:mm")), Convert.ToDateTime(Convert.ToDateTime(EndDateTime).ToString("dd'/'MM'/'yyyy HH:mm")), SubeId, ID, UrunAdi);
            List<ReportsDetailViewModel> CiroyaGoreListe = list.OrderByDescending(o => o.Debit).ToList();

            #endregion NEW SQL WEB REPORT

            if (FilterData != -1)
            {
                var Test1 = CiroyaGoreListe.Skip(0).Take(Convert.ToInt32(FilterData)).ToList();

                #region SUBEDEKI FilterData 'e GORE URUNLER LİSTELENİYOR

                if (SubeId == "0")
                {
                    foreach (var item in Test1)
                    {
                        Dictionary<string, string> countDataList = new Dictionary<string, string>();
                        NameList.Add(item.Sube.ToString());
                        countDataList.Add("name", "(" + item.Sube + ") " + item.ProductName + "-" + item.ProductGroup);
                        countDataList.Add("y", item.Miktar.ToString() + "&" + item.Debit.ToString().Replace(",", ".") + "&" + item.ProductGroup);
                        DataList.Add(countDataList);
                    }
                }
                else
                {
                    foreach (var item in Test1)
                    {
                        Dictionary<string, string> countDataList = new Dictionary<string, string>();
                        NameList.Add(item.Sube.ToString());
                        countDataList.Add("name", item.ProductName);
                        countDataList.Add("y", item.Miktar.ToString() + "&" + item.Debit.ToString().Replace(",", "."));
                        DataList.Add(countDataList);

                        //Dictionary<string, string> countDataList1 = new Dictionary<string, string>();
                        //NameList.Add(item.Sube.ToString());
                        //countDataList1.Add("name", item.ProductName);
                        //countDataList1.Add("y", item.Miktar.ToString());
                        //DataList.Add(countDataList1);
                    }
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
                #endregion SUBEDEKI FilterData 'e GORE URUNLER LİSTELENİYOR
            }
            else
            {
                #region ŞUBEDEKI TÜM URUNLER LİSTELENIYOR              
                if (SubeId == "0")
                {
                    foreach (var item in CiroyaGoreListe)
                    {
                        Dictionary<string, string> countDataList = new Dictionary<string, string>();
                        NameList.Add(item.Sube.ToString());
                        countDataList.Add("name", "(" + item.Sube + ") " + item.ProductName + "");
                        countDataList.Add("y", item.Miktar.ToString() + "&" + item.Debit.ToString().Replace(",", "."));
                        DataList.Add(countDataList);
                    }
                }
                else
                {
                    foreach (var item in CiroyaGoreListe)
                    {
                        //Dictionary<string, string> countDataList = new Dictionary<string, string>();
                        //NameList.Add(item.Sube.ToString());
                        //countDataList.Add("name", "(" + item.Sube + ") " + item.ProductName);
                        //countDataList.Add("y", item.Debit.ToString());
                        //DataList.Add(countDataList);
                        Dictionary<string, string> countDataList = new Dictionary<string, string>();
                        NameList.Add(item.Sube.ToString());
                        countDataList.Add("name", item.ProductName);
                        countDataList.Add("y", item.Miktar.ToString() + "&" + item.Debit.ToString().Replace(",", "."));
                        DataList.Add(countDataList);
                    }
                }
                #endregion ŞUBEDEKI TÜM URUNLER LİSTELENIYOR
            }
            //}
            return Json(new
            {
                Data = list
            }, JsonRequestBehavior.AllowGet);
            //}
        }

        #endregion Ürüne Göre Şube Satış İstatistikleri (Liste)


        #region **Masa Üstü Raporları**

        #region Ürün grubuna göre, masaüstü raporu

        public JsonResult UrunGrubunaGoreMasaUstuRapor(string StartDateTime, string EndDateTime, int FilterData, string SubeId, string UrunAdi)
        {
            List<Singleton.AyList> AyList = (List<Singleton.AyList>)Session["AyList"];

            #region YETKİLİ ID (Left Menude filtrele de yapıyorum)
            string ID = Request.Cookies["PRAUT"].Value;
            ViewBag.YetkiliID = ID;
            #endregion

            JsonResultModel result = new JsonResultModel();
            result.IsSuccess = false;

            if (StartDateTime == "0" || EndDateTime == "0")
            {
                List<UrunGrubu> list_ = new List<UrunGrubu>();
                list_ = null;
                return Json(new
                {
                    Data = list_
                }, JsonRequestBehavior.AllowGet);
            }
            var dataMontYear = Singleton.SetMonthYear().Where(x => x.MonthYear == StartDateTime).FirstOrDefault().SqlScriptStartDate;
            StartDateTime = dataMontYear;

            var NameList = new List<string>();
            var DataList = new List<Dictionary<string, string>>();

            #region NEW SQL WEB REPORT                      
            List<UrunGrubu> list = UrunGrubuCRUD.List(Convert.ToDateTime(Convert.ToDateTime(StartDateTime).ToString("dd'/'MM'/'yyyy HH:mm")), Convert.ToDateTime(Convert.ToDateTime(EndDateTime).ToString("dd'/'MM'/'yyyy HH:mm")), SubeId, null, ID, false);
            List<UrunGrubu> CiroyaGoreListe = list.OrderByDescending(o => o.Debit).ToList();

            #endregion NEW SQL WEB REPORT

            if (FilterData != -1)
            {
                var Test1 = CiroyaGoreListe.Skip(0).Take(Convert.ToInt32(FilterData)).ToList();

                #region SUBEDEKI FilterData 'e GORE URUNLER LİSTELENİYOR

                if (SubeId == "0")
                {
                    foreach (var item in Test1)
                    {
                        Dictionary<string, string> countDataList = new Dictionary<string, string>();
                        NameList.Add(item.Sube.ToString());
                        countDataList.Add("name", "(" + item.Sube + ") " + item.ProductName + "-" + item.ProductGroup);
                        countDataList.Add("y", item.Miktar.ToString() + "&" + item.Debit.ToString().Replace(",", ".") + "&" + item.ProductGroup);
                        DataList.Add(countDataList);
                    }
                }
                else
                {
                    foreach (var item in Test1)
                    {
                        Dictionary<string, string> countDataList = new Dictionary<string, string>();
                        NameList.Add(item.Sube.ToString());
                        countDataList.Add("name", item.ProductName);
                        countDataList.Add("y", item.Miktar.ToString() + "&" + item.Debit.ToString().Replace(",", "."));
                        DataList.Add(countDataList);

                        //Dictionary<string, string> countDataList1 = new Dictionary<string, string>();
                        //NameList.Add(item.Sube.ToString());
                        //countDataList1.Add("name", item.ProductName);
                        //countDataList1.Add("y", item.Miktar.ToString());
                        //DataList.Add(countDataList1);
                    }
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
                #endregion SUBEDEKI FilterData 'e GORE URUNLER LİSTELENİYOR
            }
            else
            {
                #region ŞUBEDEKI TÜM URUNLER LİSTELENIYOR              
                if (SubeId == "0")
                {
                    foreach (var item in CiroyaGoreListe)
                    {
                        Dictionary<string, string> countDataList = new Dictionary<string, string>();
                        NameList.Add(item.Sube.ToString());
                        countDataList.Add("name", "(" + item.Sube + ") " + item.ProductName + "");
                        countDataList.Add("y", item.Miktar.ToString() + "&" + item.Debit.ToString().Replace(",", "."));
                        DataList.Add(countDataList);
                    }
                }
                else
                {
                    foreach (var item in CiroyaGoreListe)
                    {
                        //Dictionary<string, string> countDataList = new Dictionary<string, string>();
                        //NameList.Add(item.Sube.ToString());
                        //countDataList.Add("name", "(" + item.Sube + ") " + item.ProductName);
                        //countDataList.Add("y", item.Debit.ToString());
                        //DataList.Add(countDataList);
                        Dictionary<string, string> countDataList = new Dictionary<string, string>();
                        NameList.Add(item.Sube.ToString());
                        countDataList.Add("name", item.ProductName);
                        countDataList.Add("y", item.Miktar.ToString() + "&" + item.Debit.ToString().Replace(",", "."));
                        DataList.Add(countDataList);
                    }
                }
                #endregion ŞUBEDEKI TÜM URUNLER LİSTELENIYOR
            }
            //}
            return Json(new
            {
                Data = list
            }, JsonRequestBehavior.AllowGet);
            //}
        }

        #endregion Ürün grubuna göre, masaüstü raporu

        #region Ürüne göre, masaüstü raporu

        public JsonResult UruneGoreMasaUstuRapor(string StartDateTime, string EndDateTime, int FilterData, string SubeId, string UrunAdi)
        {
            List<Singleton.AyList> AyList = (List<Singleton.AyList>)Session["AyList"];

            #region YETKİLİ ID (Left Menude filtrele de yapıyorum)
            string ID = Request.Cookies["PRAUT"].Value;
            ViewBag.YetkiliID = ID;
            #endregion

            JsonResultModel result = new JsonResultModel();
            result.IsSuccess = false;

            if (StartDateTime == "0" || EndDateTime == "0")
            {
                List<UrunGrubu> list_ = new List<UrunGrubu>();
                list_ = null;
                return Json(new
                {
                    Data = list_
                }, JsonRequestBehavior.AllowGet);
            }
            var dataMontYear = Singleton.SetMonthYear().Where(x => x.MonthYear == StartDateTime).FirstOrDefault().SqlScriptStartDate;
            StartDateTime = dataMontYear;

            var NameList = new List<string>();
            var DataList = new List<Dictionary<string, string>>();

            #region NEW SQL WEB REPORT    

            List<SubeUrun> list = SubeUrunCRUD.List(Convert.ToDateTime(Convert.ToDateTime(StartDateTime).ToString("dd'/'MM'/'yyyy HH:mm")), Convert.ToDateTime(Convert.ToDateTime(EndDateTime).ToString("dd'/'MM'/'yyyy HH:mm")), SubeId, ID, "", "");
            List<SubeUrun> CiroyaGoreListe = list.OrderByDescending(o => o.Debit).ToList();

            #endregion NEW SQL WEB REPORT

            if (FilterData != -1)
            {
                var Test1 = CiroyaGoreListe.Skip(0).Take(Convert.ToInt32(FilterData)).ToList();

                #region SUBEDEKI FilterData 'e GORE URUNLER LİSTELENİYOR

                if (SubeId == "0")
                {
                    foreach (var item in Test1)
                    {
                        Dictionary<string, string> countDataList = new Dictionary<string, string>();
                        NameList.Add(item.Sube.ToString());
                        countDataList.Add("name", "(" + item.Sube + ") " + item.ProductName + "-" + item.ProductName);
                        countDataList.Add("y", item.Miktar.ToString() + "&" + item.Debit.ToString().Replace(",", ".") + "&" + item.ProductName);
                        DataList.Add(countDataList);
                    }
                }
                else
                {
                    foreach (var item in Test1)
                    {
                        Dictionary<string, string> countDataList = new Dictionary<string, string>();
                        NameList.Add(item.Sube.ToString());
                        countDataList.Add("name", item.ProductName);
                        countDataList.Add("y", item.Miktar.ToString() + "&" + item.Debit.ToString().Replace(",", "."));
                        DataList.Add(countDataList);

                        //Dictionary<string, string> countDataList1 = new Dictionary<string, string>();
                        //NameList.Add(item.Sube.ToString());
                        //countDataList1.Add("name", item.ProductName);
                        //countDataList1.Add("y", item.Miktar.ToString());
                        //DataList.Add(countDataList1);
                    }
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
                #endregion SUBEDEKI FilterData 'e GORE URUNLER LİSTELENİYOR
            }
            else
            {
                #region ŞUBEDEKI TÜM URUNLER LİSTELENIYOR              
                if (SubeId == "0")
                {
                    foreach (var item in CiroyaGoreListe)
                    {
                        Dictionary<string, string> countDataList = new Dictionary<string, string>();
                        NameList.Add(item.Sube.ToString());
                        countDataList.Add("name", "(" + item.Sube + ") " + item.ProductName + "");
                        countDataList.Add("y", item.Miktar.ToString() + "&" + item.Debit.ToString().Replace(",", "."));
                        DataList.Add(countDataList);
                    }
                }
                else
                {
                    foreach (var item in CiroyaGoreListe)
                    {
                        //Dictionary<string, string> countDataList = new Dictionary<string, string>();
                        //NameList.Add(item.Sube.ToString());
                        //countDataList.Add("name", "(" + item.Sube + ") " + item.ProductName);
                        //countDataList.Add("y", item.Debit.ToString());
                        //DataList.Add(countDataList);
                        Dictionary<string, string> countDataList = new Dictionary<string, string>();
                        NameList.Add(item.Sube.ToString());
                        countDataList.Add("name", item.ProductName);
                        countDataList.Add("y", item.Miktar.ToString() + "&" + item.Debit.ToString().Replace(",", "."));
                        DataList.Add(countDataList);
                    }
                }
                #endregion ŞUBEDEKI TÜM URUNLER LİSTELENIYOR
            }
            //}
            return Json(new
            {
                Data = list
            }, JsonRequestBehavior.AllowGet);
            //}
        }

        #endregion Ürüne göre, masaüstü raporu

        #region Şubeye göre, masaüstü raporu

        public JsonResult SubeyeGoreMasaUstuRapor(string StartDateTime, string EndDateTime, int FilterData, string SubeId, string UrunAdi)
        {
            List<Singleton.AyList> AyList = (List<Singleton.AyList>)Session["AyList"];

            #region YETKİLİ ID (Left Menude filtrele de yapıyorum)
            string ID = Request.Cookies["PRAUT"].Value;
            ViewBag.YetkiliID = ID;
            #endregion  YETKİLİ ID (Left Menude filtrele de yapıyorum)

            JsonResultModel result = new JsonResultModel();
            result.IsSuccess = false;

            if (StartDateTime == "0" || EndDateTime == "0")
            {
                List<UrunGrubu> list_ = new List<UrunGrubu>();
                list_ = null;
                return Json(new
                {
                    Data = list_
                }, JsonRequestBehavior.AllowGet);
            }

            var dataMontYear = Singleton.SetMonthYear().Where(x => x.MonthYear == StartDateTime).FirstOrDefault().SqlScriptStartDate;
            StartDateTime = dataMontYear;

            var NameList = new List<string>();
            var DataList = new List<Dictionary<string, string>>();

            ////TODO bu kısım düzeltilecek.
            //if (string.IsNullOrEmpty(SubeId))
            //{
            #region NEW SQL WEB REPORT  

            List<UrunGrubu> list = UrunGrubuCRUD.List(Convert.ToDateTime(Convert.ToDateTime(StartDateTime).ToString("dd'/'MM'/'yyyy HH:mm")), Convert.ToDateTime(Convert.ToDateTime(EndDateTime).ToString("dd'/'MM'/'yyyy HH:mm")), SubeId, UrunAdi.Replace("_", " "), ID, false);
            List<UrunGrubu> CiroyaGoreListe = list.OrderByDescending(o => o.Debit).ToList();

            #endregion NEW SQL WEB REPORT
            return Json(new
            {
                Data = list
            }, JsonRequestBehavior.AllowGet);
            //}
            //else
            //{
            //    #region NEW SQL WEB REPORT  

            //    List<SubeUrun> list = SubeUrunCRUD.List(Convert.ToDateTime(Convert.ToDateTime(StartDateTime).ToString("dd'/'MM'/'yyyy HH:mm")), Convert.ToDateTime(Convert.ToDateTime(EndDateTime).ToString("dd'/'MM'/'yyyy HH:mm")), SubeId, Id, "", "");
            //    List<SubeUrun> CiroyaGoreListe = list.OrderByDescending(o => o.Debit).ToList();

            //    #endregion NEW SQL WEB REPORT
            //    return Json(new
            //    {
            //        Data = list
            //    }, JsonRequestBehavior.AllowGet);
            //}
        }

        #endregion Ürüne göre, masaüstü raporu

        #region Saat/Gün Raporu

        public JsonResult SaatGunMasaUstuRapor(string StartDateTime, string EndDateTime, int FilterData, string SubeId, string Saat, string SaatGunQuery)
        {
            //SaatGunQuery=0 Gün Tutar
            //SaatGunQuery=1 detay

            List<Singleton.AyList> AyList = (List<Singleton.AyList>)Session["AyList"];

            #region YETKİLİ ID (Left Menude filtrele de yapıyorum)
            string ID = Request.Cookies["PRAUT"].Value;
            ViewBag.YetkiliID = ID;
            #endregion  YETKİLİ ID (Left Menude filtrele de yapıyorum)

            JsonResultModel result = new JsonResultModel();
            result.IsSuccess = false;
            if (SaatGunQuery == "0")
            {
                StartDateTime = Convert.ToDateTime(Singleton.SetMonthYear()[0].SqlScriptStartDate).ToString(("dd'/'MM'/'yyyy ")) + Saat + ":00";
                EndDateTime = Convert.ToDateTime(Singleton.SetMonthYear()[0].SqlScriptEndDate).ToString(("dd'/'MM'/'yyyy ")) + Saat + ":59";
            }
            else
            {
                StartDateTime = Convert.ToDateTime(StartDateTime).ToString(("dd'/'MM'/'yyyy ")) + Saat + ":00";
                EndDateTime = Convert.ToDateTime(EndDateTime).ToString(("dd'/'MM'/'yyyy ")) + Saat + ":59";
            }

            //Convert.ToDateTime(EndDate_.ToString()).ToString("dd'/'MM'/'yyyy HH:mm");
            var NameList = new List<string>();
            var DataList = new List<Dictionary<string, string>>();

            //TODO bu kısım düzeltilecek.
            //if (string.IsNullOrEmpty(SubeId))
            //{
            //    #region NEW SQL WEB REPORT  
            //    List<UrunGrubu> list = UrunGrubuCRUD.List(Convert.ToDateTime(Convert.ToDateTime(StartDateTime).ToString("dd'/'MM'/'yyyy HH:mm")), Convert.ToDateTime(Convert.ToDateTime(EndDateTime).ToString("dd'/'MM'/'yyyy HH:mm")), SubeId, Saat.Replace("_", " "), Id);
            //    List<UrunGrubu> CiroyaGoreListe = list.OrderByDescending(o => o.Debit).ToList();
            //    #endregion NEW SQL WEB REPORT
            //    return Json(new
            //    {
            //        Data = list
            //    }, JsonRequestBehavior.AllowGet);
            //}
            //else
            //{
            #region NEW SQL WEB REPORT  

            List<SubeUrun> list = SubeUrunCRUD.List(Convert.ToDateTime(Convert.ToDateTime(StartDateTime).ToString("dd'/'MM'/'yyyy HH:mm")), Convert.ToDateTime(Convert.ToDateTime(EndDateTime).ToString("dd'/'MM'/'yyyy HH:mm")), SubeId, ID, "", SaatGunQuery);
            List<SubeUrun> CiroyaGoreListe = list.OrderByDescending(o => o.Debit).ToList();

            #endregion NEW SQL WEB REPORT
            return Json(new
            {
                Data = list
            }, JsonRequestBehavior.AllowGet);
            //}
        }

        #endregion Saat/Gün Raporu

        #region Saat/Gün Raporu

        public JsonResult GunSaatMasaUstuRapor(string StartDateTime, string EndDateTime, int FilterData, string SubeId, string Gun, string SaatGunQuery)
        {
            //SaatGunQuery=0 Gün Tutar
            //SaatGunQuery=1 detay
            List<Singleton.AyList> AyList = (List<Singleton.AyList>)Session["AyList"];

            #region YETKİLİ ID (Left Menude filtrele de yapıyorum)
            string ID = Request.Cookies["PRAUT"].Value;
            ViewBag.YetkiliID = ID;
            #endregion  YETKİLİ ID (Left Menude filtrele de yapıyorum)

            JsonResultModel result = new JsonResultModel();
            result.IsSuccess = false;
            if (SaatGunQuery == "3")
            {
                if (Gun == "0")
                {
                    Gun = DateTime.Now.ToString("dd'/'MM'/'yyyy");
                }
                StartDateTime = Gun + " 00:00:00";//Convert.ToDateTime(Singleton.SetMonthYear()[0].SqlScriptStartDate).ToString(("dd'/'MM'/'yyyy "));
                EndDateTime = Gun + " 23:59:59";//Convert.ToDateTime(Singleton.SetMonthYear()[0].SqlScriptEndDate).ToString(("dd'/'MM'/'yyyy ")) ;
            }
            else
            {
                //StartDateTime = StartDateTime.Substring(0, 2);
                EndDateTime = EndDateTime.Substring(0, 2);
                StartDateTime = Gun + " " + StartDateTime + ":00";//Convert.ToDateTime(Singleton.SetMonthYear()[0].SqlScriptStartDate).ToString(("dd'/'MM'/'yyyy "));
                EndDateTime = Gun + " " + EndDateTime + ":59";//Convert.ToDateTime(Singleton.SetMonthYear()[0].SqlScriptEndDate).ToString(("dd'/'MM'/'yyyy ")) ;
            }

            //Convert.ToDateTime(EndDate_.ToString()).ToString("dd'/'MM'/'yyyy HH:mm");
            var NameList = new List<string>();
            var DataList = new List<Dictionary<string, string>>();

            #region NEW SQL WEB REPORT  

            List<SubeUrun> list = SubeUrunCRUD.List(Convert.ToDateTime(Convert.ToDateTime(StartDateTime).ToString("dd'/'MM'/'yyyy HH:mm")), Convert.ToDateTime(Convert.ToDateTime(EndDateTime).ToString("dd'/'MM'/'yyyy HH:mm")), SubeId, ID, "", SaatGunQuery);
            List<SubeUrun> CiroyaGoreListe = list.OrderByDescending(o => o.Debit).ToList();

            #endregion NEW SQL WEB REPORT
            return Json(new
            {
                Data = list
            }, JsonRequestBehavior.AllowGet);
            //}
        }

        #endregion Saat/Gün Raporu

        #endregion **Masa Üstü Raporları**


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