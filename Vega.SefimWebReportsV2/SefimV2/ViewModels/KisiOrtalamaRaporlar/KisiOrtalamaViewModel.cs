using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SefimV2.ViewModels.KisiOrtalamaRaporlar
{
    public class KisiOrtalamaViewModel
    {
        public int id { get; set; }
        public int SubeID { get; set; }
        public string Sube { get; set; } = "";
        public int Kisi { get; set; }
        public decimal Total { get; set; }
        public decimal Ortalama { get; set; }
        public string ErrorMessage { get; set; }
        public bool ErrorStatus { get; set; }
        public string ErrorCode { get; set; }
        public KisiOrtalamaViewModel() { }
    }
}