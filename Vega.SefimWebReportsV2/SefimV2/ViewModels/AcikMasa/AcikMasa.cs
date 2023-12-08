using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Hosting;

namespace SefimV2.Models
{
    public class AcikMasa
    {
        public int id { get; set; }
        public int SubeID { get; set; }
        public string TableNumber { get; set; } = "";
        public string Sube { get; set; } = "";
        public decimal Debit { get; set; }
        public string TarihMin { get; set; }
        public string TarihMax { get; set; }
        public string ErrorMessage { get; set; }
        public bool ErrorStatus { get; set; }
        public string ErrorCode { get; set; }

        public AcikMasa() { }
        
    }
}