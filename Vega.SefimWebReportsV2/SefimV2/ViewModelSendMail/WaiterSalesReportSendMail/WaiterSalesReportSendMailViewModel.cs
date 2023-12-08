using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SefimV2.ViewModelSendMail.WaiterSalesReportSendMail
{
    public class WaiterSalesReportSendMailViewModel
    {
        //public int id { get; set; }
        //public int SubeID { get; set; }
        public string Sube { get; set; } = "";
        public string Personel { get; set; }
        public decimal Toplam { get; set; }
        //public string ErrorMessage { get; set; }
        //public bool ErrorStatus { get; set; }
        //public string ErrorCode { get; set; }
        public WaiterSalesReportSendMailViewModel() { }
    }
}