using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace vrlibwin.Model.License
{
    public class LicenseStatusResult
    {
        public bool Status { get; set; }
        public string Message { get; set; }
        public string Moduller { get; set; }
        public int KullaniciSayisi { get; set; }
        public DateTime LisansBitis { get; set; }
        public DateTime SonErisimZaman { get; set; }
        public int MinimumErisimGun { get; set; }
        public int UrunId { get; set; }

        public DateTime GuncellemeBitis { get; set; }
    }
}
