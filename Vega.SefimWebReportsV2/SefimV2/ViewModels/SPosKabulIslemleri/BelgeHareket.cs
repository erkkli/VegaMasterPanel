using System.ComponentModel.DataAnnotations;

namespace SefimV2.ViewModels.SPosKabulIslemleri
{
    public class BelgeHareket
    {
        //[BelgeHareket]

        public int Id { get; set; }
        public int Belge { get; set; }
        public string Stok { get; set; }
        public string Barcode { get; set; }
        public decimal Miktar { get; set; }
        public decimal ZayiMiktar { get; set; }
        public decimal Fiyat { get; set; }
        public decimal Tutar { get; set; }
        public decimal Isk1 { get; set; }
        public decimal Isk2 { get; set; }
        public decimal Isk3 { get; set; }
        public decimal Isk4 { get; set; }
        public decimal AltIskonto { get; set; }
        public decimal IskToplam { get; set; }
        public decimal SatirToplam { get; set; }
        public int VgFatHarInd { get; set; }
        public decimal KDV { get; set; }
        public decimal KDVTutar { get; set; }
        public int StokExtId { get; set; }
        public int SatirSayisi { get; set; }
        public decimal Parabirimi { get; set; }
        public string Aciklama { get; set; }
        public string RBMiktar { get; set; }
        public decimal MiktarBos { get; set; }
        public decimal Kampanya { get; set; }
        public decimal Otv { get; set; }
        public decimal OtvTutar { get; set; }
        public string MalinCinsi { get; set; }
        public string Birim { get; set; }
        public decimal Kur { get; set; }
        public bool SayimTarihDahil { get; set; }
        public decimal Envanter { get; set; }
        public object Clone()
        {
            return this.MemberwiseClone();
        }
    }
}