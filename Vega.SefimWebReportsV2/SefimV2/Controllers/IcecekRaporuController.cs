using SefimV2.Models.IcecekRaporuCRUD;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SefimV2.Controllers
{
    public class IcecekRaporuController : Controller
    {
        // GET: IcecekRaporu
        public ActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public string ToplamIcecekSatislari(string StartDate, string EndDate, string URUNGRUBU, string URUNADI, string SUBEADI,string SATISTIPI)
        {

            return IcecekRaporuCRUD.ToplamIcecekSatislari(Convert.ToDateTime(StartDate), Convert.ToDateTime(EndDate),URUNGRUBU, URUNADI, SUBEADI,SATISTIPI);
        } 
        [HttpGet]
        public string UrunlereGore(string StartDate, string EndDate, string URUNGRUBU, string URUNADI, string SUBEADI,string SATISTIPI)
        {

            return IcecekRaporuCRUD.UrunlereGore(Convert.ToDateTime(StartDate), Convert.ToDateTime(EndDate),URUNGRUBU, URUNADI, SUBEADI,SATISTIPI);
        } 
        [HttpGet]
        public string IcecekGruplarinaGore(string StartDate, string EndDate, string URUNGRUBU, string URUNADI, string SUBEADI,string SATISTIPI)
        {

            return IcecekRaporuCRUD.IcecekGruplarinaGore(Convert.ToDateTime(StartDate), Convert.ToDateTime(EndDate),URUNGRUBU, URUNADI, SUBEADI,SATISTIPI);
        }
        [HttpGet]
        public string SubelereGore(string StartDate, string EndDate, string URUNGRUBU, string URUNADI, string SUBEADI, string SATISTIPI)
        {

            return IcecekRaporuCRUD.SubelereGore(Convert.ToDateTime(StartDate), Convert.ToDateTime(EndDate), URUNGRUBU, URUNADI, SUBEADI, SATISTIPI);
        }
        [HttpGet]
        public string SatisTipineGore(string StartDate, string EndDate, string URUNGRUBU, string URUNADI, string SUBEADI, string SATISTIPI)
        {

            return IcecekRaporuCRUD.SatisTipineGore(Convert.ToDateTime(StartDate), Convert.ToDateTime(EndDate), URUNGRUBU, URUNADI, SUBEADI, SATISTIPI);
        }
    }
}