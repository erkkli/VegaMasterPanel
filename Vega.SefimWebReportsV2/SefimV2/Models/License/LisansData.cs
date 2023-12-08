using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace vrlibwin.Model.License
{
    public class LisansData
    {
        /// <summary>
        /// Lisansı satan satıcı adı
        /// </summary>
        public string SaticiAd { get; set; }

        /// <summary>
        /// Lisansı satan satıcı ünvanı
        /// </summary>
        public string SaticiUnvan { get; set; }

        /// <summary>
        /// Lisanslanmış ürünün id si, Lisans sistemde kayıtlı ürün id si
        /// </summary>
        public int UrunId { get; set; }

        /// <summary>
        /// Lisanslanmış ürün modülleri
        /// </summary>
        public string Moduller { get; set; }

        /// <summary>
        /// Lisanslanmış kullanıcı sayısı
        /// </summary>
        public int KullaniciSayisi { get; set; }

        /// <summary>
        /// Lisanslamanın yapıldığı donanım değeri
        /// </summary>
        public string DonanimDeger { get; set; }

        /// <summary>
        /// Lisansın durumu id, Lisanslı olup olmadığı
        /// </summary>
        public int LisansDurumId { get; set; }

        /// <summary>
        /// Lisansın Tür id
        /// </summary>
        public int LisansTurId { get; set; }

        /// <summary>
        /// Sipariş türü id
        /// </summary>
        public int SiparisTurId { get; set; }

        /// <summary>
        /// Minimum erişim günü, lisansın yaşam süresi
        /// </summary>
        public int MinimumErisimGun { get; set; }

        /// <summary>
        /// İstemcinin lisans sisteme son eriştiği zaman
        /// </summary>
        public DateTime SonErisimZaman { get; set; }

        /// <summary>
        /// Lisansın başlama tarihi
        /// </summary>
        public DateTime LisansBaslangic { get; set; }

        /// <summary>
        /// Lisansın bitiş tarihi
        /// </summary>
        public DateTime LisansBitis { get; set; }

        /// <summary>
        /// Güncelleme almanın bitiş tarihi
        /// </summary>
        public DateTime GuncellemeBitis { get; set; }

        /// <summary>
        /// Lisansı alan işletme adı
        /// </summary>
        public string IsletmeAd { get; set; }

        /// <summary>
        /// Lisansı alan işletme ünvanı
        /// </summary>
        public string IsletmeUnvan { get; set; }

        /// <summary>
        /// Lisanslanan ürün adı
        /// </summary>
        public string UrunAd { get; set; }

        /// <summary>
        /// Lisans durumu adı
        /// </summary>
        public string LisansDurumAd { get; set; }

        /// <summary>
        /// Lisans tür adı
        /// </summary>
        public string LisansTurAd { get; set; }

        /// <summary>
        /// Sipariş tür adı
        /// </summary>
        public string SiparisTurAd { get; set; }
    }
}
