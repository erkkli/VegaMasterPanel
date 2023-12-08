using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace SefimV2.ViewModelSendMail.CiroReportSendMail
{
    public class CiroReportSendMailViewModel
    {
        
        public string Sube { get; set; } = "";
        [DisplayFormat(DataFormatString = "{0:#,###.00;-#,###.00;0}")]
        public decimal Ciro { get; set; }
        public decimal Nakit { get; set; }
        public decimal Kredi { get; set; }
        public decimal YemekKarti { get; set; }
        public decimal AcikHesap { get; set; }
        public decimal AcikMasalar { get; set; }
        public decimal ikram { get; set; }
        public decimal MasaSayisi { get; set; }
        public decimal İndirim { get; set; }
        [DisplayFormat(DataFormatString = "{0:0.##}")]
        public decimal iptal { get; set; }
        public decimal Zayi { get; set; }


        public CiroReportSendMailViewModel() { }

    }
}