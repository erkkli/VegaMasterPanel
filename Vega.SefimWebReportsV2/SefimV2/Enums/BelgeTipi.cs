using System.ComponentModel.DataAnnotations;

namespace SefimV2.Enums
{
    public partial class General
    {
        public enum BelgeTipi : byte
        {
            [Display(Name = "Talep Belgesi")]
            TalepBelgesi = 135,

            [Display(Name = "Sayım Belgesi")]//Sayım Girişi Belgesi
            SayimGirisBelgesi = 93,

            [Display(Name = "Zayi Giriş Belgesi")]//Sayım Çıkış Belgesi
            ZayiGirisBelgesi = 94,

            [Display(Name = "Alış Faturası")]
            AlisFaturasi = 20,

            [Display(Name = "Gider Faturası")]
            GiderFaturasi = 165,

            [Display(Name = "İade Faturası")]
            IadeFaturasi = 23,

            [Display(Name = "Alış İrsaliyesi")]
            AlisIrsaliyesi = 26,

            [Display(Name = "İade İrsaliyesi")]
            IadeIrsaliyesi = 29,

            [Display(Name = "Depo Transfer")]
            DepoTransfer = 128,

            [Display(Name = "Depo Transfer Kabul")]
            DepoTransferKabul = 129,

            [Display(Name = "Sayım Şablon")]//Sayım Şablon
            SayimSablon = 255
        }
    }
}