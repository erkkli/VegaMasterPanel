using SefimV2.Models.GramajSaatlikSatisCRUD;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SefimV2.Controllers
{
    public class GramajSaatlikSatisRaporuController : Controller
    {
        // GET: GramajSaatlikSatisRaporu
        public ActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public string DonerCesitlerineGore(string StartDate, string EndDate, string KOD1, string KOD2, string URUNADI, string TEKTRH, string GUN, string SAAT, string SUBEADI)
        {

            return GramajSaatlikSatisCRUD.DonerCesitlerineGore(Convert.ToDateTime(StartDate), Convert.ToDateTime(EndDate), KOD1,KOD2,URUNADI,TEKTRH,GUN,SAAT, SUBEADI);
        }

        [HttpGet]
        public string TarihlereGore(string StartDate, string EndDate, string KOD1, string KOD2, string URUNADI, string TEKTRH, string GUN, string SAAT, string SUBEADI)
        {

            return GramajSaatlikSatisCRUD.TarihlereGore(Convert.ToDateTime(StartDate), Convert.ToDateTime(EndDate), KOD1, KOD2, URUNADI, TEKTRH, GUN, SAAT, SUBEADI);
        }

        [HttpGet]
        public string GunlereGore(string StartDate, string EndDate, string KOD1, string KOD2, string URUNADI, string TEKTRH, string GUN, string SAAT, string SUBEADI)
        {

            return GramajSaatlikSatisCRUD.GunlereGore(Convert.ToDateTime(StartDate), Convert.ToDateTime(EndDate), KOD1, KOD2, URUNADI, TEKTRH, GUN, SAAT, SUBEADI);
        }

        [HttpGet]
        public string SaatlereGore(string StartDate, string EndDate, string KOD1, string KOD2, string URUNADI, string TEKTRH, string GUN, string SAAT, string SUBEADI)
        {

            return GramajSaatlikSatisCRUD.SaatlereGore(Convert.ToDateTime(StartDate), Convert.ToDateTime(EndDate), KOD1, KOD2, URUNADI, TEKTRH, GUN, SAAT, SUBEADI);
        } 
        [HttpGet]
        public string UrunlereGore(string StartDate, string EndDate, string KOD1, string KOD2, string URUNADI, string TEKTRH, string GUN, string SAAT, string SUBEADI)
        {

            return GramajSaatlikSatisCRUD.UrunlereGore(Convert.ToDateTime(StartDate), Convert.ToDateTime(EndDate), KOD1, KOD2, URUNADI, TEKTRH, GUN, SAAT, SUBEADI);
        }
        [HttpGet]
        public string SubelereGoreDetayli(string StartDate, string EndDate, string KOD1, string KOD2, string URUNADI, string TEKTRH, string GUN, string SAAT, string SUBEADI)
        {

            return GramajSaatlikSatisCRUD.SubelereGoreDetayli(Convert.ToDateTime(StartDate), Convert.ToDateTime(EndDate), KOD1, KOD2, URUNADI, TEKTRH, GUN, SAAT, SUBEADI);
        }

        [HttpGet]
        public string SubelereGore(string StartDate, string EndDate, string KOD1, string KOD2, string URUNADI, string TEKTRH, string GUN, string SAAT, string SUBEADI)
        {

            return GramajSaatlikSatisCRUD.SubelereGore(Convert.ToDateTime(StartDate), Convert.ToDateTime(EndDate), KOD1, KOD2, URUNADI, TEKTRH, GUN, SAAT, SUBEADI);
        }
    }
}