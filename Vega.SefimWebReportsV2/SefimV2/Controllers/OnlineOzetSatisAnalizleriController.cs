using SefimV2.Models.OnlineOzetSatisAnalizleriCRUD;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SefimV2.Controllers
{
    public class OnlineOzetSatisAnalizleriController : Controller
    {
        // GET: OnlineOzetSatisAnalizleri
        public ActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public string SatisKanallarinaGore(string StartDate, string EndDate, string TEKTRH, string SUBEADI, string KANAL)
        {

            return OnlineOzetSatisAnalizleriCRUD.SatisKanallarinaGore(Convert.ToDateTime(StartDate), Convert.ToDateTime(EndDate), TEKTRH, SUBEADI, KANAL);
        }
        [HttpGet]
        public string TariheGore(string StartDate, string EndDate, string TEKTRH, string SUBEADI, string KANAL)
        {

            return OnlineOzetSatisAnalizleriCRUD.TariheGore(Convert.ToDateTime(StartDate), Convert.ToDateTime(EndDate), TEKTRH, SUBEADI, KANAL);
        }
        [HttpGet]
        public string SubelereGore(string StartDate, string EndDate, string TEKTRH, string SUBEADI, string KANAL)
        {

            return OnlineOzetSatisAnalizleriCRUD.SubelereGore(Convert.ToDateTime(StartDate), Convert.ToDateTime(EndDate), TEKTRH, SUBEADI, KANAL);
        }
    }
}