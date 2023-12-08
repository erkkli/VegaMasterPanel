using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SefimV2.ViewModelSendMail.UrunGrubuReportSendMail
{
    public class UrunGrubuDetailSendMailViewModel
    {
        //public int SubeID { get; set; }
        public string Sube { get; set; } = "";

        //public string ProductName { get; set; } = "";
        public string UrunGrubu { get; set; } = "";
        public decimal Miktar { get; set; }
        public decimal Tutar { get; set; }
        //public double GecenZaman { get; set; }
        //public decimal TotalDebit { get; set; }
        //public decimal ToplamMiktar { get; set; }

        //public string ErrorMessage { get; set; }
        //public bool ErrorStatus { get; set; }
        //public string ErrorCode { get; set; }

        public UrunGrubuDetailSendMailViewModel() { }
    }
}