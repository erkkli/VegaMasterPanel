using System.ComponentModel.DataAnnotations;

namespace SefimV2.Enums
{
    public partial class General
    {
        public enum BankaTipi : byte
        {
            [Display(Name = "DİĞER BANKA")]
            DigerBanka = 66,

            [Display(Name = "A-BANK")]
            ABank = 124,

            [Display(Name = "AKBANK")]
            AkBank = 46,

            [Display(Name = "AKTIFBANK")]
            AktifBank = 143,

            [Display(Name = "ALBARAKA")]
            AlbarakaBank = 203,

            //[Display(Name = "BANK ASYA")]
            //BankAsya = 208,

            [Display(Name = "CITIBANK")]
            CitiBank = 92,

            [Display(Name = "DENIZ")]
            DenizBank = 134,

            [Display(Name = "FINANSBANK")]
            FinansBank = 111,

            [Display(Name = "FORTIS")]
            FortisBank = 71,

            [Display(Name = "GARANTI")]
            GrantiBank = 62,

            [Display(Name = "HALKBANK")]
            HalkBank = 12,

            [Display(Name = "HSBC")]
            HsbcBank = 123,

            [Display(Name = "ISBANK")]
            IsBank = 64,

            [Display(Name = "SEKERBANK")]
            SekerBank = 59,

            //[Display(Name = "SEKERSIRKET")]
            //SekerSirketBank = 9009,

            [Display(Name = "TEB")]
            TebBank = 32,

            [Display(Name = "TFKB")]
            TfkbBank = 206,

            [Display(Name = "VAKIF")]
            VakifBank = 15,

            [Display(Name = "YKB")]
            YkbBank = 67,

            [Display(Name = "ZIRAATBANK")]
            ZiraatBank = 10,

            [Display(Name = "ADABANK")]
            AdaBank = 100,

            [Display(Name = "ING BANK")]
            IngBank = 99,
        }
    }
}