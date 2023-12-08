using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SefimV2.ViewModels.SelfOutParametersSettings
{
    public class SelfOutParametersSettingsViewModel
    {
        public int Id { get; set; }
        public string TableName { get; set; }
        public bool IsSelfSync { get; set; }

        public SelfOutParametersSettingsViewModel() { }
    }
}