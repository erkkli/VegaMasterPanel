using SefimV2.Helper;
using SefimV2.Models;
using SefimV2.Repository;
using SefimV2.ViewModels.GetTime;
using SefimV2.ViewModels.Inventory;
using System;
using System.Collections.Generic;
using System.Web.Mvc;

namespace SefimV2.Controllers
{
    [CheckLoggedIn]
    public class InventoryController : Controller
    {
        // GET: Inventory
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
            catch (Exception ex)
            {
                Singleton.WritingLogFile("SubeUrunlerController_Details", ex.Message.ToString());
            }

            TimeViewModel viewModel = Singleton.GetTimeViewModel(tarihAraligiStartDate, tarihAraligiEndDate, durum);
            ViewBag.StartDateTime = viewModel.StartDate;
            ViewBag.EndDateTime = viewModel.EndDate;
            ViewBag.Pages = "Envanter Raporları";
            ViewBag.PageNavi = "Envanter Raporu";
            #endregion
            List<InventoryVievModel> list = InventoryCRUD.List(Convert.ToDateTime(viewModel.StartDate), Convert.ToDateTime(viewModel.EndDate), subeid, ID);
            return View(list);
        }

        public ActionResult Details()
        {
            string ID = Request.Cookies["PRAUT"].Value;

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
            #endregion

            TimeViewModel viewModel = Singleton.GetTimeViewModel(tarihAraligiStartDate, tarihAraligiEndDate, durum);

            #region ENVANTER  (Tarih filtrelemesi yok. Sadece gün içindeki envanterin sayımı yapılıyor.)          
            Saat saat = Ayarlar.SaatListe();
            string bugun_ = Convert.ToDateTime((DateTime.Today.ToShortDateString() + " " + saat.StartTime)).ToString("d.M.yyyy HH:mm");
            string gunSonu_ = Convert.ToDateTime((DateTime.Now.AddDays(1).ToShortDateString() + " " + saat.EndTime)).ToString("d.M.yyyy HH:mm");
            #endregion

            ViewBag.StartDateTime = viewModel.StartDate;
            ViewBag.EndDateTime = viewModel.EndDate;
            ViewBag.Pages = "Envanter Detay";
            ViewBag.SubeId = subeid;

            //List<SubeUrun> list = SubeUrunCRUD.List(Convert.ToDateTime(viewModel.StartDate), Convert.ToDateTime(viewModel.EndDate), subeid, Id);
            List<InventoryVievModel> list = InventoryCRUD.List(Convert.ToDateTime(bugun_), Convert.ToDateTime(gunSonu_), subeid, ID);

            return View(list);
        }
    }
}