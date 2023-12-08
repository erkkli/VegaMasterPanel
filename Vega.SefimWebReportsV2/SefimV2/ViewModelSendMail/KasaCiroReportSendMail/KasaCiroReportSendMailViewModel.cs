using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SefimV2.ViewModelSendMail.CiroRaporlariSendMail
{
    public class KasaCiroReportSendMailViewModel
    {
        //public int id { get; set; }
        public string Sube { get; set; } = "";
        //public string SubeId { get; set; } = "";
        public decimal Ciro { get; set; }
        public decimal Nakit { get; set; }
        public decimal Kredi { get; set; }
        public decimal YemekKarti { get; set; }
        public decimal GelirGider { get; set; }

        public KasaCiroReportSendMailViewModel() { }
    }
}