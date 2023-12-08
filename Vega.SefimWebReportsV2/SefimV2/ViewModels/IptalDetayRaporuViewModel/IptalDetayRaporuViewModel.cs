using System.ComponentModel;

namespace SefimV2.ViewModels.IptalDetayRaporuViewModel
{
    public class IptalDetayRaporuViewModel
    {
        [DisplayName("Şube")]
        public string Sube { get; set; } = "";

        public int SubeId { get; set; }

        [DisplayName("Id")]
        public long Id { get; set; }

        [DisplayName("Silme Zamanı")]
        public string SilmeZamani { get; set; }

        [DisplayName("Sipariş Zamanı")]
        public string SiparisZamani { get; set; }

        [DisplayName("Masa No")]
        public string MasaNo { get; set; }

        [DisplayName("Sipariş Alan")]
        public string SiparisAlanKullanici { get; set; }

        [DisplayName("Silme Açıklaması")]
        public string SilmeAciklamasi { get; set; }

        [DisplayName("Silen Kullanıcı")]
        public string SilenKullanici { get; set; }

        [DisplayName("Ürün")]
        public string UrunAdi { get; set; }

        [DisplayName("Miktar")]
        public decimal Miktar { get; set; }

        [DisplayName("Fiyat")]
        public decimal Fiyat { get; set; }

        [DisplayName("Toplam Tutar")]
        public decimal ToplamTutar { get; set; }


        [DisplayName("Basım Adedi")]
        public string BasimAdedi { get; set; }

        [DisplayName("Detay")]
        public string SilmeDetay { get; set; }

        [DisplayName("Toplam Tutarı")]
        public decimal ToplamIptalTutari { get; set; }

        [DisplayName("Toplam Miktar")]
        public decimal ToplamIptalMiktari { get; set; }
    }
}