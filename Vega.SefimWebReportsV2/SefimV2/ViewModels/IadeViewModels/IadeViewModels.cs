using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Hosting;

namespace SefimV2.Models
{
    public class IadeViewModel
    {
        public int id { get; set; }
        public string SubeID { get; set; }
        public string Sube { get; set; } = "";
        public string UserName { get; set; } = "";
        public string ProductName { get; set; } = "";
        public decimal Price { get; set; }
        public decimal Quantity { get; set; }
        public decimal SumTotal { get; set; }
        public decimal SumQuantity { get; set; }

        public string DateTime { get; set; } = "";

        public string ErrorMessage { get; set; }
        public bool ErrorStatus { get; set; }
        public string ErrorCode { get; set; }
 
        public IadeViewModel() { }

    }
}