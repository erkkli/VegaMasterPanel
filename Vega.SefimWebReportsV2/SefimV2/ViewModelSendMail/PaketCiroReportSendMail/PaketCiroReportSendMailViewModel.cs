using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SefimV2.ViewModelSendMail.PaketCiroReportSendMail
{
    public class PaketCiroReportSendMailViewModel
    {
        //public int id { get; set; }
        //public int SubeID { get; set; }
        public string Sube { get; set; } = "";
        //public int Adet { get; set; }


        //public decimal PhoneOrderDebit { get; set; }
        //public string ErrorMessage { get; set; }
        //public bool ErrorStatus { get; set; }
        //public string ErrorCode { get; set; }


        public int Adet { get; set; }
        public decimal Toplam { get; set; }
        public decimal AcikHesap { get; set; }
        public decimal Nakit { get; set; }
        public decimal Kredi { get; set; }
        public decimal YemekKarti { get; set; }
        public decimal İndirim { get; set; }
        public decimal AlinanTahsilat { get; set; }
        public decimal KalanTahsilat { get; set; }



        public PaketCiroReportSendMailViewModel() { }
    }
}