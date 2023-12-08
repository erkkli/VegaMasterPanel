using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Hosting;

namespace SefimV2.Models
{
    public class PaketCiro
    {
        public int id { get; set; }
        public int SubeID { get; set; }
        public string Sube { get; set; } = "";
        public int Adet { get; set; }


        public decimal PhoneOrderDebit { get; set; }
        public string ErrorMessage { get; set; }
        public bool ErrorStatus { get; set; }
        public string ErrorCode { get; set; }


        public int OrderCount { get; set; }
        public decimal Total { get; set; }
        public decimal Debit { get; set; } = 0;
        public decimal CashPayment { get; set; }
        public decimal CreditPayment { get; set; }
        public decimal TicketPayment { get; set; }
        public decimal Discount { get; set; }
        public decimal CollectedTotal { get; set; }
        public decimal Balance { get; set; }
        public decimal OnlinePayment { get; set; }
        //online odemelr
        public decimal GETIR { get; set; }
        public decimal TRENDYOL { get; set; }
        public decimal YEMEKSEPETI { get; set; }

        public PaketCiro() { }
    }
}