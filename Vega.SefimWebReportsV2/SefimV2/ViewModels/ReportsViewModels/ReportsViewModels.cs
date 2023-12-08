using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SefimV2.ViewModels.ReportsViewModels
{
    public class ReportsViewModels
    {

        public class DashboarCriterSalesCostPriviousMonth
        {
            public DateTime DateStr { get; set; }
            public double TotalCost { get; set; }
            public decimal TotalCost2 { get; set; }
            public int DayNumber { get; set; }
            public string CRITER { get; set; }
        }
    }
}