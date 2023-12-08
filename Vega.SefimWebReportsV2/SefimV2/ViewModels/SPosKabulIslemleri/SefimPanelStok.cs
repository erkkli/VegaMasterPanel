using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;

namespace SefimV2.ViewModels.SPosKabulIslemleri
{
    public class SefimPanelStok
    {
        public int StokInd { get; set; }
        public int BirimInd { get; set; }
        public string StokKodu { get; set; }
        public string Barkod { get; set; }

        [DisplayName("Malın Cinsi")]
        public string MalinCinsi { get; set; }
        public decimal SatisFiyati { get; set; }
        public decimal AlisFiyati { get; set; }
        public int KdvDahil { get; set; }
        public decimal Kdv { get; set; }
        public string BirimAdi { get; set; }
    }
}