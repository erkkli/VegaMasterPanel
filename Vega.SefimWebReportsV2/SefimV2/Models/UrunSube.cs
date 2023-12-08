using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Hosting;

namespace SefimV2.Models
{
    public class UrunSube
    {

        public int SubeID { get; set; }
        public decimal Miktar { get; set; }
        public string Sube { get; set; } = "";
        public string ProductName { get; set; } = "";
        public string ProductGroup { get; set; } = "";
        public decimal Debit { get; set; }
        public double GecenZaman { get; set; }
        public decimal TotalDebit { get; set; }
        public string ErrorMessage { get; set; }
        public bool ErrorStatus { get; set; }
        public string ErrorCode { get; set; }

        public UrunSube() { }       
    }
}