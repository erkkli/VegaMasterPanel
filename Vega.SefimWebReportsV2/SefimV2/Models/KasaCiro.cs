using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Hosting;

namespace SefimV2.Models
{
    public class KasaCiro
    {

        public int id { get; set; }
        public string Sube { get; set; } = "";
        public string SubeId { get; set; } = ""; 
        public decimal Ciro { get; set; }
        public decimal Cash { get; set; }
        public decimal Credit { get; set; }
        public decimal Ticket { get; set; }
        public double GecenZaman { get; set; }
        public string ErrorMessage { get; set; }
        public bool ErrorStatus { get; set; }
        public string ErrorCode { get; set; }

        public decimal GelirGider { get; set; } = 0;

        public KasaCiro() { }     

    }
}