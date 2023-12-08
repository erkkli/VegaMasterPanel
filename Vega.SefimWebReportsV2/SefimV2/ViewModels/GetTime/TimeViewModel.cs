using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SefimV2.ViewModels.GetTime
{
    public class TimeViewModel
    {
        public string StartDate { get; set; }

        public string EndDate { get; set; }
    }

    public class MasaUstuRaparuDateModel
    {
        public string MonthYear { get; set; }
        public string Month { get; set; }
        public string Year { get; set; }
        public string SqlScriptStartDate { get; set; }
        public string SqlScriptEndDate { get; set; }
    }


 
}