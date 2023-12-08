using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vega.Belge.Wrapper
{
    public class VegaBelgeKasa
    {
        public int BelgeNo { get; set;}
        public DateTime Tarih { get; set; }
        public decimal Gelir { get; set; }
        public decimal Gider { get; set; }
        
        public string ParaBirimi { get; set; }
        public int Kur { get; set; }
        public string Aciklama { get; set; }

        public DateTime IslemTarihi { get; set; }
        public string Sube { get; set; }
        public string Kasa { get; set; }
        public int KdvDahil { get; set; }
        public int IslemTipi { get; set; }
        public int Izahat { get; set; }
    }
}
