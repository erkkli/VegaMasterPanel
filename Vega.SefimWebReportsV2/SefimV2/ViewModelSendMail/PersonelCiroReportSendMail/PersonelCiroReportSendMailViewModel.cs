using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SefimV2.ViewModelSendMail.PersonelCiroReportSendMail
{
    public class PersonelCiroReportSendMailViewModel
    {
        //public int id { get; set; }
        //public int SubeID { get; set; }
        public string Sube { get; set; } = "";
        public string PersonelAdi { get; set; } = "";
        public decimal Tutar { get; set; }
        //public string ErrorMessage { get; set; }
        //public bool ErrorStatus { get; set; }
        //public string ErrorCode { get; set; }


        public decimal Nakit { get; set; }
        public decimal Kredi { get; set; }
        public decimal YemekCeki { get; set; }
        //public decimal Total { get; set; }

        public PersonelCiroReportSendMailViewModel() { }
    }
}