using System.ComponentModel;

namespace SefimV2.ViewModels.IskontoDetayRaporuViewModel
{
    public class IkramDetayRaporuViewModel
    {
        [DisplayName("Şube")]
        public string Sube { get; set; } = "";
        public int SubeId { get; set; }

        [DisplayName("Id")]
        public long Id { get; set; }

        [DisplayName("İkram Tarihi")]
        public string IkramTarihi { get; set; }

        [DisplayName("Masa No")]
        public string MasaNo { get; set; }

        [DisplayName("Ürün Adı")]
        public string UrunAdi { get; set; }

        [DisplayName("Personel")]
        public string Personel { get; set; }

        [DisplayName("İkram Açıklaması")]
        public string IkramAciklamasi { get; set; }

        [DisplayName("İkram Tutarı")]
        public decimal IkramTutari { get; set; }

        [DisplayName("Toplam İkram Tutarı"  )]
        public decimal IkramToplamTutari { get; set; }
    }
}