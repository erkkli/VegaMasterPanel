using Newtonsoft.Json;
using SefimV2.Models.SatislarAnalizCRUD;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SefimV2.Controllers
{
    public class DateResponse
    {
        public string startDate { get; set; }
        public string endDate { get; set; }
    }
    public class SatislarAnalizController : Controller
    {
        // GET: SatislarAnaliz
        public ActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public string OzetBilgi(string StartDate, string EndDate, string KOD1, string ProductGroup, string URUNADI, string SUBEADI, string MinDate, string MaxDate)
        {
            
            return SatislarAnalizCRUD.OzetBilgi(Convert.ToDateTime(StartDate), Convert.ToDateTime(EndDate), KOD1, ProductGroup, URUNADI, SUBEADI, MinDate, MaxDate);
        } 
        [HttpGet]
        public string GruplaraGoreSatislar(string StartDate, string EndDate, string KOD1, string ProductGroup, string URUNADI, string SUBEADI, string MinDate, string MaxDate)
        {
            
            return SatislarAnalizCRUD.GruplaraGoreSatislar(Convert.ToDateTime(StartDate), Convert.ToDateTime(EndDate), KOD1, ProductGroup, URUNADI, SUBEADI, MinDate, MaxDate);
        } 
        [HttpGet]
        public string UrunlereGoreSatislar(string StartDate, string EndDate, string KOD1, string ProductGroup, string URUNADI, string SUBEADI, string MinDate, string MaxDate)
        {
            
            return SatislarAnalizCRUD.UrunlereGoreSatislar(Convert.ToDateTime(StartDate), Convert.ToDateTime(EndDate), KOD1, ProductGroup, URUNADI, SUBEADI, MinDate, MaxDate);
        } 
        [HttpGet]
        public string DonerCesitlerineGoreSatislar(string StartDate, string EndDate, string KOD1, string ProductGroup, string URUNADI, string SUBEADI, string MinDate, string MaxDate)
        {
            
            return SatislarAnalizCRUD.DonerCesitlerineGoreSatislar(Convert.ToDateTime(StartDate), Convert.ToDateTime(EndDate), KOD1, ProductGroup, URUNADI, SUBEADI, MinDate, MaxDate);
        }
        
        [HttpGet]
        public string SubelereGoreSatislar(string StartDate, string EndDate, string KOD1, string ProductGroup, string URUNADI, string SUBEADI, string MinDate, string MaxDate)
        {
            
            return SatislarAnalizCRUD.SubelereGoreSatislar(Convert.ToDateTime(StartDate), Convert.ToDateTime(EndDate), KOD1, ProductGroup, URUNADI, SUBEADI, MinDate, MaxDate);
        }
        
        [HttpGet]
        public string SaateGoreSatislar(string StartDate, string EndDate, string KOD1, string ProductGroup, string URUNADI, string SUBEADI, string MinDate, string MaxDate)
        {
            
            return SatislarAnalizCRUD.SaateGoreSatislar(Convert.ToDateTime(StartDate), Convert.ToDateTime(EndDate), KOD1, ProductGroup, URUNADI, SUBEADI, MinDate, MaxDate);
        }
        [HttpGet]
        public string GunlereGoreSatislar(string StartDate, string EndDate, string KOD1, string ProductGroup, string URUNADI, string SUBEADI, string MinDate, string MaxDate)
        {
            
            return SatislarAnalizCRUD.GunlereGoreSatislar(Convert.ToDateTime(StartDate), Convert.ToDateTime(EndDate), KOD1,ProductGroup,URUNADI, SUBEADI, MinDate, MaxDate);
        }

        [HttpGet]
        public string dateButtonClick(string btnID)
        {
            DateResponse response = new DateResponse();
          
            if (btnID== "btnToday")
            {
                response.startDate = DateTime.Now.ToString("dd.MM.yyyy");
                response.endDate = DateTime.Now.ToString("dd.MM.yyyy");
            } 
            if (btnID== "btnYesterday")
            {
                response.startDate = DateTime.Now.AddDays(-1).ToString("dd.MM.yyyy");
                response.endDate = DateTime.Now.AddDays(-1).ToString("dd.MM.yyyy");
            } 
            if (btnID== "btnMonth")
            {
                response.startDate = DateTime.Now.ToString("01.MM.yyyy");
                response.endDate = Convert.ToDateTime(DateTime.Now.ToString("01.MM.yyyy")).AddMonths(1).AddDays(-1).ToString("dd.MM.yyyy");
            }
            return JsonConvert.SerializeObject(response);
        }
    }
}