using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Hosting;

namespace SefimV2.Models
{
    public class AcikMasaIslem
    {
        public int id { get; set; }
        public int SubeID { get; set; }
        public int MasaSayisi { get; set; }
        public string TableNumber { get; set; } = "";
        public string Sube { get; set; } = "";
        public string ProductName { get; set; } = "";
        public decimal Debit { get; set; }
        public decimal TotalDebit { get; set; }
        public string GecenSure { get; set; } = "";
        public string TarihMin { get; set; }
        public string TarihMax { get; set; }
        public string ErrorMessage { get; set; }
        public bool ErrorStatus { get; set; }
        public string ErrorCode { get; set; }
        public List<AcikMasaIslem> ListTotal { get; set; }

        public AcikMasaIslem() { }
       
    }
}