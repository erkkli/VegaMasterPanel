using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vega.Belge.Wrapper
{
    public class VegaBelgeHareket
    {
        public int StokNo { get; set; }
        public decimal Miktar { get; set; }
        public decimal Fiyat { get; set; }
        public string Aciklama { get; set; }
        public int DepoNo { get; set; }
        public int BirimNo { get; set; }
        public decimal Kdv { get; set; }
        public int Vade { get; set; }
        public int Izahat { get; set; }
        public string StokKodu { get; set; }
        public string MalinCinsi { get; set; }
        public DateTime Tarih { get; set; }
        public int BankaId { get; set; }
        public int Kur { get; set; }
        public string ParaBirimi { get; set; }

        public decimal Sayim { get; set; }
        public decimal Maliyet { get; set; }

        public decimal Envanter { get; set; }
        public decimal ISK1 { get; set; }
        public decimal ISK2 { get; set; }
        public decimal ISK3 { get; set; }
        public decimal ISK4 { get; set; }
        public decimal ISK5 { get; set; }
        public decimal ISK6 { get; set; }

    }
}
