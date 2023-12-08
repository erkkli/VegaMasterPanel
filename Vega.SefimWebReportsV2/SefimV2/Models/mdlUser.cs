using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SefimV2.Models
{
    public class mdlUser
    {
        public int ID { get; set; }
        public int CariID { get; set; }
        public string UserName { get; set; }
        public string Pass { get; set; }
        public string Ad { get; set; }
        public string Soyad { get; set; }

    }
}