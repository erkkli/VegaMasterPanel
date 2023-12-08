using SefimV2.Models.YanUrunlerRaporuCRUD;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SefimV2.Controllers
{
    public class YanUrunlerRaporuController : Controller
    {
        // GET: YanUrunlerRaporu
        public ActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public string SubelereGore(string StartDate, string EndDate, string TEKTRH, string SUBEADI)
        {

            return YanUrunlerRaporuCRUD.SubelereGore(Convert.ToDateTime(StartDate), Convert.ToDateTime(EndDate), TEKTRH, SUBEADI);
        }

        [HttpGet]
        public string TariheGore(string StartDate, string EndDate, string TEKTRH, string SUBEADI)
        {

            return YanUrunlerRaporuCRUD.TariheGore(Convert.ToDateTime(StartDate), Convert.ToDateTime(EndDate), TEKTRH, SUBEADI);
        }
    }
}