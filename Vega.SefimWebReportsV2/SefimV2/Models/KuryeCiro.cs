using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Hosting;

namespace SefimV2.Models
{
    public class KuryeCiro
    {
        public int id { get; set; }
        public int SubeID { get; set; }
        public int Adet { get; set; }
        public string Sube { get; set; } = "";
        public string PersonelAdi { get; set; } = "";
        public decimal Debit { get; set; }
        public string ErrorMessage { get; set; }
        public bool ErrorStatus { get; set; }
        public string ErrorCode { get; set; }




        public int OrderCount { get; set; }
        public decimal Total { get; set; }
        //public decimal Debit { get; set; }
        public decimal CashPayment { get; set; }
        public decimal CreditPayment { get; set; }
        public decimal TicketPayment { get; set; }
        public decimal Discount { get; set; }
        public decimal CollectedTotal { get; set; }
        public decimal Balance { get; set; }



        public KuryeCiro() { }

    }
}