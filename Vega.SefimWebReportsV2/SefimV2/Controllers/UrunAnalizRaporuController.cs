using SefimV2.Helper;
using SefimV2.Models;
using SefimV2.Models.UrunAnalizRaporlari;
using SefimV2.Repository;
using SefimV2.ViewModels.GetTime;
using SefimV2.ViewModels.Result;
using SefimV2.ViewModels.UrunAnalizRaporu;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SefimV2.Controllers
{
    [CheckLoggedIn]
    public class UrunAnalizRaporuController : Controller
    {
        // GET: UrunAnalizRaporu
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult List()
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

            #region Secilen Tarih Filterelemesine Göre Yapılan İşlemler           
            string tarihAraligiStartDate = "";
            string tarihAraligiEndDate = "";
            string subeid = "";
            string durum = "";
            string productgrup = "";

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
                if (Request.QueryString["productgrup"] != null)
                {
                    productgrup = Request.QueryString["productgrup"].ToString();
                }
                productgrup = null;
            }
            catch (Exception ex)
            {
                Singleton.WritingLogFile("SubeUrunlerController_Details", ex.Message.ToString());
            }
            TimeViewModel viewModel = Singleton.GetTimeViewModel(tarihAraligiStartDate, tarihAraligiEndDate, durum);
            ViewBag.StartDateTime = viewModel.StartDate;
            ViewBag.EndDateTime = viewModel.EndDate;
            ViewBag.Pages = "Urun Analiz";
            ViewBag.PageNavi = "Ürün Analiz Raporu";
            #endregion

            List<UrunAnalizRaporuViewModel> list = UrunAnalizRaporuCRUD.List(Convert.ToDateTime(viewModel.StartDate), Convert.ToDateTime(viewModel.EndDate), subeid, ID, productgrup);
            return View(list);
        }

        #region DETAIL LIST        
        public ActionResult Details()
        {
            string ID = Request.Cookies["PRAUT"].Value;

            #region Secilen Tarih Filterelemesine Göre Yapılan İşlemler  
            string tarihAraligiStartDate = "";
            string tarihAraligiEndDate = "";
            string subeid = "";
            string durum = "";
            string productgrup = "";

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

                if (Request.QueryString["productgrup"] != null)
                {
                    productgrup = Request.QueryString["productgrup"].ToString().Replace("_", " ");
                }
                productgrup = null;
            }
            catch (Exception ex)
            {
                Singleton.WritingLogFile("SubeUrunlerController_Details", ex.Message.ToString());
            }
            #endregion

            TimeViewModel viewModel = Singleton.GetTimeViewModel(tarihAraligiStartDate, tarihAraligiEndDate, durum);
            ViewBag.StartDateTime = viewModel.StartDate;
            ViewBag.EndDateTime = viewModel.EndDate;
            ViewBag.Pages = "Urun Analiz Detay";
            ViewBag.SubeId = subeid;

            List<UrunAnalizRaporuViewModel> list = UrunAnalizRaporuCRUD.List(Convert.ToDateTime(viewModel.StartDate), Convert.ToDateTime(viewModel.EndDate), subeid, ID, productgrup);

            return View(list);
        }
        #endregion DETAIL LIST

        #region PRODUCTS GROUP DETAIL     
        public ActionResult ProductGroupDetails()
        {
            string ID = Request.Cookies["PRAUT"].Value;
            #region Secilen Tarih Filterelemesine Göre Yapılan İşlemler  
            string tarihAraligiStartDate = "";
            string tarihAraligiEndDate = "";
            string subeid = "";
            string durum = "";
            string productgrup = "";

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

                if (Request.QueryString["productgrup"] != null)
                {                    
                    productgrup = Request.QueryString["productgrup"].ToString().Replace("_ _", " + ").Replace("_", " ").Replace("]", "&");
                }

            }
            catch (Exception ex)
            {
                Singleton.WritingLogFile("SubeUrunlerController_Details", ex.Message.ToString());
            }
            #endregion

            TimeViewModel viewModel = Singleton.GetTimeViewModel(tarihAraligiStartDate, tarihAraligiEndDate, durum);
            ViewBag.StartDateTime = viewModel.StartDate;
            ViewBag.EndDateTime = viewModel.EndDate;
            ViewBag.Pages = "Urun Analiz Detay";
            ViewBag.SubeId = subeid;
            ViewBag.ProductGroup = productgrup;
            ViewBag.TableNameP = productgrup.Replace(" ", "");

            //List<UrunGrubu> list = UrunGrubuCRUD.List(GelenGun, GelenGunSonu, subeid, productgrup);
            List<UrunAnalizRaporuViewModel> list = UrunAnalizRaporuCRUD.List(Convert.ToDateTime(viewModel.StartDate), Convert.ToDateTime(viewModel.EndDate), subeid, ID, productgrup.Replace("_", " "));
            //List<UrunGrubu> CiroyaGoreListe = list.OrderByDescending(o => o.ProductGroup). ToList();

            return View(list);
        }
        #endregion PRODUCTS GROUP DETAIL


        #region SAYIM-DONEM **** (LİNE)
        [HttpGet]
        public JsonResult PaymentTaypeTotalAmount(string StartDateTime, string EndDateTime ,string subeId,string productGroup)
        {
            string ID = Request.Cookies["PRAUT"].Value;

            JsonResultModel result = new JsonResultModel();
            result.IsSuccess = false;
            var NameList = new List<string>();
            var CategoriesList = new List<string>();
            var DataList = new List<Dictionary<string, string>>();
            var DataDetailList = new List<string>();
            Dictionary<String, List<int>> abc = new Dictionary<string, List<int>>();

            List<UrunAnalizRaporuViewModel> SubeCiro = UrunAnalizSayimDonemCRUD.List(Convert.ToDateTime(StartDateTime), Convert.ToDateTime(EndDateTime), subeId, ID, productGroup);

            if (SubeCiro != null)
            {

                NameList.Add("Recete Maliyet");
                NameList.Add("Gerçek Maliyet");
                //NameList.Add("Yemek Kartı");

                abc.Add("Recete Maliyet", new List<int>
                { });
                abc.Add("Gerçek Maliyet", new List<int>
                { });
                //abc.Add("Yemek Kartı", new List<int>
                //{ });

                foreach (var item in SubeCiro)
                {                   
                    Dictionary<string, string> countDataList = new Dictionary<string, string>();
                    CategoriesList.Add(item.SayimDonemi);

                    countDataList.Add("name", "Recete Maliyet");
                    countDataList.Add("y", item.ReceteMaliyeti.ToString());
                    DataList.Add(countDataList);

                    Dictionary<string, string> countDataList2 = new Dictionary<string, string>();
                    countDataList2.Add("name", "Gerçek Maliyet");
                    countDataList2.Add("y", item.GerceklesenMaliyet.ToString());
                    //countDataList2.Add("y", item.toplam.Value);
                    DataList.Add(countDataList2);
                    //Dictionary<string, string> countDataList3 = new Dictionary<string, string>();
                    //countDataList3.Add("name", "Yemek Kartı ");
                    //countDataList3.Add("y", item.Ticket.ToString());
                    ////countDataList2.Add("y", item.toplam.Value);
                    //DataList.Add(countDataList3);
                    abc["Recete Maliyet"].Add(Convert.ToInt32(item.ReceteMaliyeti));
                    abc["Gerçek Maliyet"].Add(Convert.ToInt32(item.GerceklesenMaliyet));
                    //abc["Yemek Kartı"].Add(Convert.ToInt32(item.Ticket));
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
        #endregion

        #region ŞUBEDEKI PERSONEL SATIŞ **** (LİNE)
        [HttpGet]
        public JsonResult PaymentUsersAmount(string StartDateTime, string EndDateTime, string subeId, string productGroup)
       {
            string ID = Request.Cookies["PRAUT"].Value;

            JsonResultModel result = new JsonResultModel();
            result.IsSuccess = false;
            var NameList = new List<string>();
            var CategoriesList = new List<string>();
            var DataList = new List<Dictionary<string, string>>();
            var DataDetailList = new List<string>();
            Dictionary<String, List<int>> abc = new Dictionary<string, List<int>>();

            List<UrunAnalizRaporuViewModel> SubeCiro = UrunAnalizSubePersoneliSatisCRUD.List(Convert.ToDateTime(StartDateTime), Convert.ToDateTime(EndDateTime), subeId, ID, productGroup);

            if (SubeCiro != null)
            {

                NameList.Add("Miktar");
                NameList.Add("Tutar");
                //NameList.Add("Yemek Kartı");

                abc.Add("Miktar", new List<int>
                { });
                abc.Add("Tutar", new List<int>
                { });
                //abc.Add("Yemek Kartı", new List<int>
                //{ });

                foreach (var item in SubeCiro)
                {
                    Dictionary<string, string> countDataList = new Dictionary<string, string>();
                    CategoriesList.Add(item.UserName);

                    countDataList.Add("name", "Miktar");
                    countDataList.Add("y", item.PersonelMiktar.ToString());
                    DataList.Add(countDataList);

                    Dictionary<string, string> countDataList2 = new Dictionary<string, string>();
                    countDataList2.Add("name", "Tutar");
                    countDataList2.Add("y", item.personelTutar.ToString());
                    //countDataList2.Add("y", item.toplam.Value);
                    DataList.Add(countDataList2);

                    //Dictionary<string, string> countDataList3 = new Dictionary<string, string>();
                    //countDataList3.Add("name", "Yemek Kartı ");
                    //countDataList3.Add("y", item.Ticket.ToString());
                    ////countDataList2.Add("y", item.toplam.Value);
                    //DataList.Add(countDataList3);

                    abc["Miktar"].Add(Convert.ToInt32(item.PersonelMiktar));
                    abc["Tutar"].Add(Convert.ToInt32(item.personelTutar));
                    //abc["Yemek Kartı"].Add(Convert.ToInt32(item.Ticket));
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
        #endregion

        #region ŞUBELERDEKİ URUN SATIŞ **** (LİNE)
        [HttpGet]
        public JsonResult PaymentProductsAmount(string StartDateTime, string EndDateTime, string subeId, string productGroup)
        {
            string ID = Request.Cookies["PRAUT"].Value;

            JsonResultModel result = new JsonResultModel();
            result.IsSuccess = false;
            var NameList = new List<string>();
            var CategoriesList = new List<string>();
            var DataList = new List<Dictionary<string, string>>();
            var DataDetailList = new List<string>();
            Dictionary<String, List<int>> abc = new Dictionary<string, List<int>>();

            List<UrunAnalizRaporuViewModel> SubeCiro = UrunAnalizSubeUrunSatisCRUD.List(Convert.ToDateTime(StartDateTime), Convert.ToDateTime(EndDateTime), subeId, ID, productGroup);

            if (SubeCiro != null)
            {

                NameList.Add("Miktar");
                NameList.Add("Tutar");
                //NameList.Add("Yemek Kartı");

                abc.Add("Miktar", new List<int>
                { });
                abc.Add("Tutar", new List<int>
                { });
                //abc.Add("Yemek Kartı", new List<int>
                //{ });

                foreach (var item in SubeCiro)
                {
                    Dictionary<string, string> countDataList = new Dictionary<string, string>();
                    CategoriesList.Add(item.Sube);

                    countDataList.Add("name", "Miktar");
                    countDataList.Add("y", item.UrunMiktar.ToString());
                    DataList.Add(countDataList);

                    Dictionary<string, string> countDataList2 = new Dictionary<string, string>();
                    countDataList2.Add("name", "Tutar");
                    countDataList2.Add("y", item.UrunTutar.ToString());
                    //countDataList2.Add("y", item.toplam.Value);
                    DataList.Add(countDataList2);

                    //Dictionary<string, string> countDataList3 = new Dictionary<string, string>();
                    //countDataList3.Add("name", "Yemek Kartı ");
                    //countDataList3.Add("y", item.Ticket.ToString());
                    ////countDataList2.Add("y", item.toplam.Value);
                    //DataList.Add(countDataList3);

                    abc["Miktar"].Add(Convert.ToInt32(item.UrunMiktar));
                    abc["Tutar"].Add(Convert.ToInt32(item.UrunTutar));
                    //abc["Yemek Kartı"].Add(Convert.ToInt32(item.Ticket));
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
        #endregion ŞUBELERDEKİ URUN SATIŞ **** (LİNE)




        #region SAYIM DONEM LISTE DAĞILIMI **** (LİNE)
        //[HttpGet]
        public JsonResult CustomerState(string StartDateTime, string EndDateTime, string productGroup)
        {
           
            string ID = Request.Cookies["PRAUT"].Value;
            string subeId = "";
            JsonResultModel result = new JsonResultModel();
            result.IsSuccess = false;
            var NameList = new List<string>();
            var CategoriesList = new List<string>();
            var DataList = new List<Dictionary<string, string>>();
            var DataDetailList = new List<string>();
            Dictionary<String, List<int>> abc = new Dictionary<string, List<int>>();

            List<UrunAnalizRaporuViewModel> SubeCiro = UrunAnalizSayimDonemCRUD.List(Convert.ToDateTime(StartDateTime), Convert.ToDateTime(EndDateTime), subeId, ID, productGroup);

            if (SubeCiro != null)
            {

                NameList.Add("Recete Maliyet");
                NameList.Add("Gerçek Maliyet");
                //NameList.Add("Yemek Kartı");

                abc.Add("Recete Maliyet", new List<int>
                { });
                abc.Add("Gerçek Maliyet", new List<int>
                { });
                //abc.Add("Yemek Kartı", new List<int>
                //{ });

                foreach (var item in SubeCiro)
                {
                    if (item.SayimDonemi !=null)
                    {
                    Dictionary<string, string> countDataList = new Dictionary<string, string>();
                    var sayimDonem_ = Convert.ToDateTime(item.SayimDonemi).ToString("yyyy-MM-dd");
                    CategoriesList.Add(item.SayimDonemi);

                    countDataList.Add("name", "Recete Maliyet");
                    countDataList.Add("y", item.ReceteMaliyeti.ToString());
                    DataList.Add(countDataList);

                    Dictionary<string, string> countDataList2 = new Dictionary<string, string>();
                    countDataList2.Add("name", "Gerçek Maliyet");
                    countDataList2.Add("y", item.GerceklesenMaliyet.ToString());
                    //countDataList2.Add("y", item.toplam.Value);
                    DataList.Add(countDataList2);

                    //Dictionary<string, string> countDataList3 = new Dictionary<string, string>();
                    //countDataList3.Add("name", "Yemek Kartı ");
                    //countDataList3.Add("y", item.Ticket.ToString());
                    ////countDataList2.Add("y", item.toplam.Value);
                    //DataList.Add(countDataList3);

                    abc["Recete Maliyet"].Add(Convert.ToInt32(item.ReceteMaliyeti));
                    abc["Gerçek Maliyet"].Add(Convert.ToInt32(item.GerceklesenMaliyet));
                        //abc["Yemek Kartı"].Add(Convert.ToInt32(item.Ticket));
                    }
                }
            }
            return Json(new
            {
                Categories = CategoriesList,
                Name = NameList,
                Data = SubeCiro //abc
            }, JsonRequestBehavior.AllowGet);
            //}
        }
        #endregion SAYIM DONEM LISTE DAĞILIMI **** (LİNE)

    }
}