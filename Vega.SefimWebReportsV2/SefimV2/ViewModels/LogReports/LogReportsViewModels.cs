using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SefimV2.ViewModels.LogReports
{
    public class LogReportsViewModels
    {
        // USR.USERNAME,
        //SATIS.ISLEMTARIHI,
        //SATIS.ACIKLAMA,
        //SATIS.BELGENO,
        //CAR.FIRMAADI,
        //STK.STOKKODU,
        //STK.MALINCINSI

        public string SubeID { get; set; }
        public string Sube { get; set; } = "";
        public string USERNAME { get; set; }
        public string ISLEMTARIHI { get; set; }
        public string ACIKLAMA { get; set; } = "";
        public string FIRMAADI { get; set; }
        public string STOKKODU { get; set; } = "";
        public string MALINCINSI { get; set; }

        public string BELGENO { get; set; }

    }
}