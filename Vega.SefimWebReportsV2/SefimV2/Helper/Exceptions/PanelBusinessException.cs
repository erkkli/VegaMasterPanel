using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Web;

namespace SefimV2.Helper.Exceptions
{
    public class PanelBusinessException: FaultException
    {


        public string ErrorDetails { get; private set; }
        public bool LoglansinMi { get; private set; }
        public string Baslik { get; private set; }

        public PanelBusinessException(string message, string details = "", string baslik = "Hata Oluştu", bool loglansinMi = true) : base(message)
        {
            ErrorDetails = details;
            LoglansinMi = loglansinMi;
            Baslik = baslik;
        }
    }
}