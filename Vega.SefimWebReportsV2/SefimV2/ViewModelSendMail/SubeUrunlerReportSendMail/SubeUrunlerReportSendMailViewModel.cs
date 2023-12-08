using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SefimV2.ViewModelSendMail.SubeUrunlerReportSendMail
{
    public class SubeUrunlerReportSendMailViewModel
    {

        //public int SubeID { get; set; }
        public string Sube { get; set; } = "";
        public UInt64 Miktar { get; set; }

        public string Urun { get; set; } = "";
        public decimal Tutar { get; set; } //Debit
        //public double GecenZaman { get; set; }
        //public decimal TotalDebit { get; set; }
        //public string ErrorMessage { get; set; }
        //public bool ErrorStatus { get; set; }
        //public string ErrorCode { get; set; }
    }
}