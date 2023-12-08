using SefimV2.Models.MiktarsalKilolukSatialrCRUD;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SefimV2.Controllers
{
    public class MiktarsalKilolukSatislarController : Controller
    {
        // GET: MiktarsalKilolukSatislar
        public ActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public string OzetBilgi(string StartDate, string EndDate, string TEKTRH,string SUBEADI)
        {

            return MiktarsalKilolukSatialrCRUD.OzetBilgi(Convert.ToDateTime(StartDate), Convert.ToDateTime(EndDate),  TEKTRH,  SUBEADI);
        }

        [HttpGet]
        public string SubelereGore(string StartDate, string EndDate, string TEKTRH, string SUBEADI)
        {

            return MiktarsalKilolukSatialrCRUD.SubelereGore(Convert.ToDateTime(StartDate), Convert.ToDateTime(EndDate), TEKTRH, SUBEADI);
        }

        [HttpGet]
        public string TariheGore(string StartDate, string EndDate, string TEKTRH, string SUBEADI)
        {

            return MiktarsalKilolukSatialrCRUD.TariheGore(Convert.ToDateTime(StartDate), Convert.ToDateTime(EndDate), TEKTRH, SUBEADI);
        }
    }
}