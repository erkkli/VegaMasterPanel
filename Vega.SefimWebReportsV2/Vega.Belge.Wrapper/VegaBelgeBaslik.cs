using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vega.Belge.Wrapper
{
    public class VegaBelgeBaslik
    {
        public int CariNo { get; set; }
        public int Izahat { get; set; }
        public int? DepoNo { get; set; }
        public VegaBelgeHareketTipi BelgeAlSat { get; set; }
        public decimal Iskonto { get; set; }
        public DateTime Tarih { get; set; }
        public string AltNot { get; set; }
        public string OzelKod1 { get; set; }
        public string OzelKod2 { get; set; }
        public string OzelKod3 { get; set; }
        public string OzelKod4 { get; set; }
        public string OzelKod5 { get; set; }
        public string OzelKod6 { get; set; }
        public string OzelKod7 { get; set; }
        public string OzelKod8 { get; set; }
        public string OzelKod9 { get; set; }
        public short Iade { get; set; }
        public int BaslikInd { get; set; }
        public int KdvDahil { get; set; }
        public int HareketDeposu { get; set; }
        public VegaEfatura EFatura { get; set; }
        public int DepoTransferId { get; set; }
        public string BelgeNo { get; set; }
        public string AltBelgeNo { get; set; }
        public DateTime? AltBelgeTarihi { get; set; }
    }

    public enum VegaBelgeHareketTipi
    {
        Giris = 0,
        Cikis = 1
    }

    public enum VegaEfatura 
    {
        Hayir=0,
        Evet=1
    }
}
