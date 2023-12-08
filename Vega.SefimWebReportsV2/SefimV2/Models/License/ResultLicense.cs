using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace vrlibwin.Model.License
{
    public class ResultLicense
    {
        public bool Sonuc { get; set; }
        public string Mesaj { get; set; }
        public JsonLicenseList[] JsonLisansList { get; set; }
    }
}
