using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Hosting;

namespace SefimV2.Models
{
    public class PersonelCiro
    {
        public int id { get; set; }
        public string SubeID { get; set; }
        public string Sube { get; set; } = "";
        public string PersonelAdi { get; set; } = "";
        public decimal Debit { get; set; }
        public string ErrorMessage { get; set; }
        public bool ErrorStatus { get; set; }
        public string ErrorCode { get; set; }
        public decimal TotalDebit { get; set; }

        public string ReceivedByUserName { get; set; } = "";
        public decimal CashPayment { get; set; }
        public decimal CreditPayment { get; set; }
        public decimal TicketPayment { get; set; }
        public decimal DebitPayment { get; set; }

        public decimal IkramPayment { get; set; }
        public decimal HediyeCeki { get; set; }

        public decimal Total { get; set; }
        public decimal Discount { get; set; }

        public PersonelCiro() { }

    }
}