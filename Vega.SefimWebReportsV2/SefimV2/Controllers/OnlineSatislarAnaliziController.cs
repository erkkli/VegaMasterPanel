using Newtonsoft.Json;
using SefimV2.Models.OnlineSatislarAnaliziCRUD;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Configuration;
using System.Web.Hosting;
using System.Web.Mvc;

namespace SefimV2.Controllers
{
    public class OnlineSatislarAnaliziController : Controller
    {
        // GET: OnlineSatislarAnalizi
        public ActionResult Index()
        {
            return View();
        }
        [HttpGet]
        public string SatisKanallarinaGore(string StartDate, string EndDate, string URUNADI, string SUBEADI, string KANAL,string ODEMETIPI, string SATISTIPI, string OPSIYONTIPI)
        {

            return OnlineSatislarAnaliziCRUD.SatisKanallarinaGore(Convert.ToDateTime(StartDate), Convert.ToDateTime(EndDate), URUNADI, SUBEADI, KANAL, ODEMETIPI,SATISTIPI,OPSIYONTIPI);
        }

        [HttpGet]
        public string OpsiyonTipineGore(string StartDate, string EndDate, string URUNADI, string SUBEADI, string KANAL, string ODEMETIPI, string SATISTIPI, string OPSIYONTIPI)
        {

            return OnlineSatislarAnaliziCRUD.OpsiyonTipineGore(Convert.ToDateTime(StartDate), Convert.ToDateTime(EndDate), URUNADI, SUBEADI, KANAL, ODEMETIPI, SATISTIPI, OPSIYONTIPI);
        }

        [HttpGet]
        public string OnlinePlatformlaraGore(string StartDate, string EndDate, string URUNADI, string SUBEADI, string KANAL, string ODEMETIPI, string SATISTIPI, string OPSIYONTIPI)
        {

            return OnlineSatislarAnaliziCRUD.OnlinePlatformlaraGore(Convert.ToDateTime(StartDate), Convert.ToDateTime(EndDate), URUNADI, SUBEADI, KANAL, ODEMETIPI, SATISTIPI, OPSIYONTIPI);
        }

        [HttpGet]
        public string TahsilatTiplerineGore(string StartDate, string EndDate, string URUNADI, string SUBEADI, string KANAL, string ODEMETIPI, string SATISTIPI, string OPSIYONTIPI)
        {

            return OnlineSatislarAnaliziCRUD.TahsilatTiplerineGore(Convert.ToDateTime(StartDate), Convert.ToDateTime(EndDate), URUNADI, SUBEADI, KANAL, ODEMETIPI, SATISTIPI, OPSIYONTIPI);
        }

        [HttpGet]
        public string SubelereGore(string StartDate, string EndDate, string URUNADI, string SUBEADI, string KANAL, string ODEMETIPI, string SATISTIPI, string OPSIYONTIPI)
        {

            return OnlineSatislarAnaliziCRUD.SubelereGore(Convert.ToDateTime(StartDate), Convert.ToDateTime(EndDate), URUNADI, SUBEADI, KANAL, ODEMETIPI, SATISTIPI, OPSIYONTIPI);
        }

        [HttpGet]
        public string UrunlereGore(string StartDate, string EndDate, string URUNADI, string SUBEADI, string KANAL, string ODEMETIPI, string SATISTIPI, string OPSIYONTIPI)
        {

            return OnlineSatislarAnaliziCRUD.UrunlereGore(Convert.ToDateTime(StartDate), Convert.ToDateTime(EndDate), URUNADI, SUBEADI, KANAL, ODEMETIPI, SATISTIPI, OPSIYONTIPI);
        }
    }
}