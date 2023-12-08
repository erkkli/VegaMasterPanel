using System.ComponentModel;

namespace SefimV2.ViewModels.IskontoDetayRaporuViewModel
{
    public class IskontoDetayRaporuViewModel
    {
        [DisplayName("Şube")]
        public string Sube { get; set; } = "";

        public int SubeId { get; set; }

        [DisplayName("Id")]
        public long Id { get; set; }
   
        [DisplayName("İskonto Tarihi")]
        public string IskontoTarihi { get; set; }

        [DisplayName("Masa No")]
        public string MasaNo { get; set; }

        [DisplayName("Personel")]
        public string Personel { get; set; }

        [DisplayName("Net Tutarı")]
        public decimal TotalPayment { get; set; }

        [DisplayName("İskonto Açıklaması")]
        public string IskontoAciklamasi { get; set; }

        [DisplayName("İskonto Tutarı")]
        public decimal Discount { get; set; }

        [DisplayName("Toplam İndirim")]
        public decimal DiscountTotal { get; set; }

        [DisplayName("İndirim Oranı")]
        public decimal DiscountRate { get; set; }

        [DisplayName("İndirim Oranı")]
        public decimal Ciro { get; set; }

        [DisplayName("Toplam Tutarı")]
        public decimal ToplamSatis { get; set; }


    }
}