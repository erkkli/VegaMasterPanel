using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SefimV2.ViewModelSendMail.AcikHesapReportSendMail
{
    public class AcikHesapReportSendMailViewModel
    {
        //public int id { get; set; }
        //public int SubeID { get; set; }
        //public string CustomerName { get; set; } = "";
        public string Sube { get; set; } = "";
        //public decimal Ciro { get; set; }
        //public decimal AlinanTahsilat { get; set; }
        //public string ErrorMessage { get; set; }
        //public bool ErrorStatus { get; set; }
        //public string ErrorCode { get; set; }



        //public int OrderCount { get; set; }
        //public decimal Total { get; set; }
        //public decimal Debit { get; set; }
        //public decimal CashPayment { get; set; }
        //public decimal CreditPayment { get; set; }
        //public decimal TicketPayment { get; set; }
        //public decimal Discount { get; set; }
        public decimal AcikHesapTahsilat { get; set; }
        public decimal AcikHesabaAktarilan { get; set; }

        public AcikHesapReportSendMailViewModel() { }
    }
}