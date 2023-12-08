using SefimV2.Models.GramajliSatislarRaporuCRUD;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SefimV2.Controllers
{
    public class GramajliSatislarRaporuController : Controller
    {
        // GET: GramajlıSatislarRaporu
        public ActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public string TavukDonerGramajlari(string StartDate, string EndDate, string SUBEADI)
        {

            return GramajliSatislarRaporuCRUD.TavukDonerGramajlari(Convert.ToDateTime(StartDate), Convert.ToDateTime(EndDate), SUBEADI);
        }
        [HttpGet]
        public string EtDonerGramajlari(string StartDate, string EndDate, string SUBEADI)
        {

            return GramajliSatislarRaporuCRUD.EtDonerGramajlari(Convert.ToDateTime(StartDate), Convert.ToDateTime(EndDate), SUBEADI);
        }
        [HttpGet]
        public string ToplamDonerGramajlari(string StartDate, string EndDate, string SUBEADI)
        {

            return GramajliSatislarRaporuCRUD.ToplamDonerGramajlari(Convert.ToDateTime(StartDate), Convert.ToDateTime(EndDate), SUBEADI);
        }
    }
}