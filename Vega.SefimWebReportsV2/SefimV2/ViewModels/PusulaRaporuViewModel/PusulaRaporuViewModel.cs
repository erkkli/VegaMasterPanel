using System.ComponentModel;

namespace SefimV2.ViewModels.PusulaRaporuViewModel
{
    public class PusulaRaporuViewModel
    {
        public string AppDbType { get; set; }

        [DisplayName("Şube")]
        public string Sube { get; set; } = "";

        public int SubeId { get; set; }

        [DisplayName("Id")]
        public long Id { get; set; }

        [DisplayName("Id")]
        public string VPosId { get; set; }

        [DisplayName("Masa NO")]
        public string MasaNo { get; set; }

        [DisplayName("Tutar")]
        public decimal Tutar { get; set; }

        [DisplayName("İlk Sipariş Alan")]
        public string IlkSiparisAlan { get; set; }

        [DisplayName("Son Sipariş Alan")]
        public string SonSiparisAlan { get; set; }

        [DisplayName("İlk Sipariş Zamanı")]
        public string IlkSiparisZamani { get; set; }

        [DisplayName("Son Sipariş Zamanı")]
        public string SonSiparisZamani { get; set; }

        [DisplayName("Süre")]
        public string Sure { get; set; }

        [DisplayName("Tahsilat Yapan")]
        public string TahsilatYapn { get; set; }

        [DisplayName("Müşteri")]
        public string Musteri { get; set; }

        //

        [DisplayName("Sipariş Zamanı")]
        public string SiparisZamani { get; set; }

        [DisplayName("Sipariş Alan")]
        public string SiparisiAlan { get; set; }

        [DisplayName("Ürün")]
        public string UrunAdi { get; set; }

        [DisplayName("Miktar")]
        public decimal Miktar { get; set; }

        [DisplayName("Fiyat")]
        public decimal Fiyat { get; set; }

        [DisplayName("Masa Taşıma")]
        public string MasaTasima { get; set; }


        //
        [DisplayName("Kredi Kartı")]
        public decimal CreditPayment { get; set; }

        [DisplayName("Nakit Ödeme")]
        public decimal CashPayment { get; set; }

        [DisplayName("Yemek Çeki Ödeme")]
        public decimal TicketPayment { get; set; }

        [DisplayName("Online Ödeme")]
        public decimal OnlinePayment { get; set; }

        [DisplayName("Online İskonto")]
        public decimal Discount { get; set; }

        [DisplayName("Açık Hesap")]
        public decimal AcikHesap { get; set; }

        [DisplayName("Ödeme Zamanı")]
        public string OdemeZamani { get; set; }

        [DisplayName("Cari Adı")]
        public string CariAdi { get; set; }


    }
}