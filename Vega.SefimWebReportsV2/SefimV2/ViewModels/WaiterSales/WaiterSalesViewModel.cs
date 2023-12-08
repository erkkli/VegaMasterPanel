using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SefimV2.ViewModels.WaiterSales
{
    public class WaiterSalesViewModel
    {
        public int id { get; set; }
        public int SubeID { get; set; }
        public string Sube { get; set; } = "";
        public string UserName { get; set; }
        public string Perkodu { get; set; }
        public string ProductName { get; set; }
        public decimal Total { get; set; }
        public decimal Miktar { get; set; }
        public decimal IslemSayisi { get; set; }
        public string ErrorMessage { get; set; }
        public bool ErrorStatus { get; set; }
        public string ErrorCode { get; set; }
        public WaiterSalesViewModel() { }


    }
}