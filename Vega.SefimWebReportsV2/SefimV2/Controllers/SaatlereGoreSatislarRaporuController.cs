using SefimV2.Models.SaatlereGoreSatislarRaporuCRUD;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SefimV2.Controllers
{
    public class SaatlereGoreSatislarRaporuController : Controller
    {
        // GET: SaatlereGöreSatislarRaporu
        public ActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public string OzetBilgi(string StartDate, string EndDate, string TEKTRH, string MinDate, string MaxDate)
        {

            return SaatlereGoreSatislarRaporuCRUD.OzetBilgi(Convert.ToDateTime(StartDate), Convert.ToDateTime(EndDate), TEKTRH, MinDate, MaxDate);
        }

        [HttpGet]
        public string SaatlereGoreSatisTutari(string StartDate, string EndDate, string TEKTRH, string MinDate, string MaxDate)
        {

            return SaatlereGoreSatislarRaporuCRUD.SaatlereGoreSatisTutari(Convert.ToDateTime(StartDate), Convert.ToDateTime(EndDate), TEKTRH, MinDate, MaxDate);
        }

        [HttpGet]
        public string SaatlereGoreSatisMiktari(string StartDate, string EndDate, string TEKTRH, string MinDate, string MaxDate)
        {

            return SaatlereGoreSatislarRaporuCRUD.SaatlereGoreSatisMiktari(Convert.ToDateTime(StartDate), Convert.ToDateTime(EndDate), TEKTRH, MinDate, MaxDate);
        }
    }
}