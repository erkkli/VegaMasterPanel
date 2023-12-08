using System;
using System.Collections.Generic;
using System.ComponentModel;
using static SefimV2.Enums.General;

namespace SefimV2.ViewModels.SPosKabulIslemleri
{
    public class BelgeAlisGiderCreate
    {
        public int Id { get; set; }

        [DisplayName("Kasa")]
        public string Kasa { get; set; }

        [DisplayName("Şube")]
        public string Sube { get; set; }

        [DisplayName("Cari")]
        public string Cari { get; set; }

        [DisplayName("Alt Cari")]
        public string AltCari { get; set; }

        [DisplayName("Belge No")]
        public string BelgeNo { get; set; }
        [DisplayName("Belge No Arctos")]
        public string BelgeNoArctos { get; set; }

        [DisplayName("Belge Kodu")]
        public string BelgeKod { get; set; }

        [DisplayName("Tarih")]
        public DateTime? Tarih { get; set; }

        [DisplayName("Belge Tutarı")]//TUTAR
        public decimal BelgeTutar { get; set; }

        [DisplayName("Ara Toplam")]//ARATOPLAM
        public decimal AraToplam { get; set; }

        [DisplayName("Belge KDV")]
        public decimal BelgeKDV { get; set; }

        [DisplayName("Net Toplam")]
        public decimal NetToplam { get; set; }

        [DisplayName("Belge Tipi")]
        public BelgeTipi BelgeTip { get; set; }

        [DisplayName("İskonto")]
        public decimal IskontoTop { get; set; }

        [DisplayName("Kdv")]
        public bool Kdv { get; set; }

        [DisplayName("Belge ÖK-1")]
        public string BelgeOzelKod1 { get; set; }

        [DisplayName("Belge ÖK-2")]
        public string BelgeOzelKod2 { get; set; }

        [DisplayName("Belge ÖK-3")]
        public string BelgeOzelKod3 { get; set; }

        [DisplayName("Belge ÖK-4")]
        public string BelgeOzelKod4 { get; set; }

        [DisplayName("Müşteri Temsilcisi")]
        public string Personel { get; set; }

        public decimal AIsk1 { get; set; }
        public decimal AIsk2 { get; set; }
        public decimal AIsk3 { get; set; }
        public decimal AIsk4 { get; set; }
        public DateTime? Vade { get; set; }
        public DateTime? Termin { get; set; }
        public string Parabirimi { get; set; }
        public decimal Kur { get; set; }
        public int VgFatInd { get; set; }
        public int Terminal { get; set; }
        public int Aktarim { get; set; }
        public int Kapali { get; set; }
        public string BelgeNot { get; set; }
        public string PesinAd { get; set; }
        public string PesinVergiNo { get; set; }
        [DisplayName("Belge Nevi")]
        public string OzelKod { get; set; }
        public string Depo { get; set; }
        public int CariExtId { get; set; }
        public DateTime? KayitTarihi { get; set; }
        [DisplayName("Transfer Edilecek Depo")]
        public string CikanDepo { get; set; }
        public string UID { get; set; }
        public string OzelKod9 { get; set; }

        public bool Depozitolu { get; set; }

        public decimal OtvToplam { get; set; }
        public decimal OdemeTutar { get; set; }
        public decimal SonBakiye { get; set; }

        public int OnayDurumu { get; set; }
        public int OnaylayanId { get; set; }
        [DisplayName("Şablon Adı")]
        public string SablonAdi { get; set; }
        [DisplayName("Şablon")]
        public string Sablon { get; set; }
        public string Page { get; set; }
        public string AltBelgeNo { get; set; }
        public DateTime? AltBelgeTarihi { get; set; }
        public BelgeAlisGiderCreate() { }
        public List<BelgeHareket> BelgeHarekets { get; set; }
        //[DisplayName("Ürün Fiyat")]
        //[Required(ErrorMessage = "{0} bilgisi giriniz.")]
        //[Range(0, 999999999999999, ErrorMessage = "{0} {1} ile {2} değerleri arasında olabilir.")]
        ////[RegularExpression(@"\d+(\,\d{1,2})?", ErrorMessage = "{0} değeri virgülden sonra en fazla 2 basamak olabilir.")]
        //[DataType(DataType.Currency)]
        //[DisplayFormat(DataFormatString = "{0:C}")]
        //public decimal Price { get; set; }                
    }
    public class BelgeList
    {
        public BelgeList() { }
        public List<BelgeAlisGiderCreate> Belgeler { get; set; }
        public BelgeTipi BelgeTipi { get; set; }
        public string Sube { get; set; }
        public string BaslangicTarihi { get; set; }
        public string BitisTarihi { get; set; }
        public OnayDurumu OnayDurumu { get; set; }
    }
}