using SefimV2.Models.PorsiyonSatislariRaporuCRUD;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SefimV2.Controllers
{
    public class PorsiyonSatislariRaporuController : Controller
    {
        // GET: PorsiyonSatislariRaporu
        public ActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public string UrunListesi(string StartDate, string EndDate, string KOD1, string URUNADI, string TEKTRH, string SUBEADI, string PORSIYONTIPI, string MinDate, string MaxDate)
        {

            return PorsiyonSatislariRaporuCRUD.UrunListesi(Convert.ToDateTime(StartDate), Convert.ToDateTime(EndDate),KOD1,URUNADI,TEKTRH,SUBEADI, PORSIYONTIPI, MinDate,MaxDate);
        } 
        [HttpGet]
        public string TariheGore(string StartDate, string EndDate, string KOD1, string URUNADI, string TEKTRH, string SUBEADI, string PORSIYONTIPI, string MinDate, string MaxDate)
        {

            return PorsiyonSatislariRaporuCRUD.TariheGore(Convert.ToDateTime(StartDate), Convert.ToDateTime(EndDate),KOD1,URUNADI,TEKTRH,SUBEADI, PORSIYONTIPI, MinDate,MaxDate);
        } 
        [HttpGet]
        public string SubelerPorsiyonAdetleri(string StartDate, string EndDate, string KOD1, string URUNADI, string TEKTRH, string SUBEADI, string PORSIYONTIPI, string MinDate, string MaxDate)
        {

            return PorsiyonSatislariRaporuCRUD.SubelerPorsiyonAdetleri(Convert.ToDateTime(StartDate), Convert.ToDateTime(EndDate),KOD1,URUNADI,TEKTRH,SUBEADI, PORSIYONTIPI, MinDate,MaxDate);
        }
          [HttpGet]
        public string DonerCesidineGore(string StartDate, string EndDate, string KOD1, string URUNADI, string TEKTRH, string SUBEADI, string PORSIYONTIPI, string MinDate, string MaxDate)
        {

            return PorsiyonSatislariRaporuCRUD.DonerCesidineGore(Convert.ToDateTime(StartDate), Convert.ToDateTime(EndDate),KOD1,URUNADI,TEKTRH,SUBEADI, PORSIYONTIPI, MinDate,MaxDate);
        }

        [HttpGet]
        public string SaateGore(string StartDate, string EndDate, string KOD1, string URUNADI, string TEKTRH, string SUBEADI, string PORSIYONTIPI, string MinDate, string MaxDate)
        {

            return PorsiyonSatislariRaporuCRUD.SaateGore(Convert.ToDateTime(StartDate), Convert.ToDateTime(EndDate), KOD1, URUNADI, TEKTRH, SUBEADI, PORSIYONTIPI, MinDate, MaxDate);
        }
        [HttpGet]
        public string PorsiyonTipineGore(string StartDate, string EndDate, string KOD1, string URUNADI, string TEKTRH, string SUBEADI, string PORSIYONTIPI, string MinDate, string MaxDate)
        {

            return PorsiyonSatislariRaporuCRUD.PorsiyonTipineGore(Convert.ToDateTime(StartDate), Convert.ToDateTime(EndDate), KOD1, URUNADI, TEKTRH, SUBEADI, PORSIYONTIPI, MinDate, MaxDate);
        }
    }
}