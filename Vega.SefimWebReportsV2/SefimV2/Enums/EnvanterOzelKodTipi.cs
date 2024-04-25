using System.ComponentModel.DataAnnotations;

namespace SefimV2.Enums
{
    public partial class General
    {
        // 1.ÖZEL KOD, 2.ÖZEL KOD,3. ÖZEL KOD şeklinde seçimler gelecek.
        //Text Box içinde SefimWebReports yazısı default gelecek bu kısmı silip kendimiz yazabileceğiz.
        //Sonada bir buton koyabiliriz.Bu butona basıldığıda Text Box içerisinde yazanı seçili Özel koda basacaz.

        public enum EnvanterOzelKodTipi : byte
        {
            [Display(Name = "Seçiniz")]
            Unset = 0,

            [Display(Name = "1.ÖZEL KOD")]
            KOD1 = 1,

            [Display(Name = "2.ÖZEL KOD")]
            KOD2 = 2,

            [Display(Name = "3.ÖZEL KOD")]
            KOD3 = 3,

            [Display(Name = "4.ÖZEL KOD")]
            KOD4 = 4,

            [Display(Name = "5.ÖZEL KOD")]
            KOD5 = 5,

            [Display(Name = "6.ÖZEL KOD")]
            KOD6 = 6,

            [Display(Name = "7.ÖZEL KOD")]
            KOD7 = 7,

            [Display(Name = "8.ÖZEL KOD")]
            KOD8 = 8,

            [Display(Name = "9.ÖZEL KOD")]
            KOD9 = 9,



        }
    }
}