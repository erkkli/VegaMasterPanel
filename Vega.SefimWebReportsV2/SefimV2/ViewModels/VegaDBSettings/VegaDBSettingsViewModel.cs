using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SefimV2.ViewModels.VegaDBSettings
{
    public class VegaDBSettingsViewModel
    {
        public int ID { get; set; }
        public DateTime CreateDate { get; set; }
        public int CreateDate_Timestamp { get; set; }
        public int ModifyCounter { get; set; }
        public DateTime UpdateDate { get; set; }
        public int UpdateDate_Timestamp { get; set; }
        public string IP { get; set; }
        public string SqlName { get; set; }
        public string SqlPassword { get; set; }
        public string DBName { get; set; }
        public bool Status { get; set; }

        public VegaDBSettingsViewModel() { }
    }
}