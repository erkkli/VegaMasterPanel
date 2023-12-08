using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SefimV2.ViewModels.SPosKabulIslemleri
{
    public class BelgeDepoKabul
    {
        public int IND { get; set; }
        public string GONDERENDEPOKODU { get; set; }
        public string KABULEDECEKDEPOKODU { get; set; }
        public DateTime TARIH { get; set; }
        public string FISNO { get; set; }
        public string OZELKOD3 { get; set; }
        public string OZELKOD4 { get; set; }
        public int BelgeID { get; set; }
    }
}