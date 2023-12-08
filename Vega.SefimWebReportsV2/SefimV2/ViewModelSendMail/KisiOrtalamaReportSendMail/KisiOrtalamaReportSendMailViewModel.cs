using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SefimV2.ViewModelSendMail.KisiOrtalamaReportSendMail
{
    public class KisiOrtalamaReportSendMailViewModel
    {
        //public int id { get; set; }
        //public int SubeID { get; set; }
        public string Sube { get; set; } = "";
        public int Kisi { get; set; }
        public decimal Toplam { get; set; }
        public decimal Ortalama { get; set; }
        //public string ErrorMessage { get; set; }
        //public bool ErrorStatus { get; set; }
        //public string ErrorCode { get; set; }


        public KisiOrtalamaReportSendMailViewModel() { }

    }
}