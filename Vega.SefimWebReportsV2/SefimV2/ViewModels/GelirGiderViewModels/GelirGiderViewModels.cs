using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Hosting;

namespace SefimV2.Models
{
    public class GelirGiderViewModels
    {
        public int id { get; set; }
        public string SubeID { get; set; }
        public string Sube { get; set; } = "";
        public string UserName { get; set; } = "";
        public string IslemTipi { get; set; } = "";
        public decimal Gelir { get; set; }
        public decimal Gider { get; set; }
        public string DateTime { get; set; } = "";
        public string Description { get; set; }
        public decimal ToplamGelir { get; set; }
        public decimal ToplamGider { get; set; }

     
        public string ErrorMessage { get; set; }
        public bool ErrorStatus { get; set; }
        public string ErrorCode { get; set; }
 
        public GelirGiderViewModels() { }

    }
}