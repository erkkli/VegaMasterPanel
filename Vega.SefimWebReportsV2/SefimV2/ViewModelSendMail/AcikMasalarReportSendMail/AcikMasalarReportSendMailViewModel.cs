using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SefimV2.ViewModelSendMail.AcikMasalarReportSendMail
{
    public class AcikMasalarReportSendMailViewModel
    {


        //public int id { get; set; }
        //public int SubeID { get; set; }
        public string Sube { get; set; } = "";
        public int MasaSayisi { get; set; }
        public string MasaNo { get; set; } = "";
        public string Urun { get; set; } = "";
        public decimal Tutar { get; set; }
        public decimal ToplamTutar { get; set; }

        //public string GecenSure { get; set; } = "";
        //public string TarihMin { get; set; }
        //public string TarihMax { get; set; }
        //public string ErrorMessage { get; set; }
        //public bool ErrorStatus { get; set; }
        //public string ErrorCode { get; set; }
        //public List<AcikMasalarReportSendMailViewModel> ListTotal { get; set; }

        public AcikMasalarReportSendMailViewModel() { }

    }
}