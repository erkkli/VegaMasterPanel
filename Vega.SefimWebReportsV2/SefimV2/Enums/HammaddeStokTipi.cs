using System.ComponentModel.DataAnnotations;

namespace SefimV2.Enums
{
    public partial class General
    {
//STOK TIPLERI;
//0 - EMTIA
//3 - HIZMET
//18 - HAMMADDE
//19 - MAMUL
//21 - YARI MAMUL
//ve HEPSI olacak başkan

        public enum HammaddeStokTipi : byte
        {
            [Display(Name = "Hepsi")]
            Hepsi = 0,

            [Display(Name = "Emtia")]
            EMTIA = 1,//0,

            [Display(Name = "Hizmet")]
            HIZMET = 3,

            [Display(Name = "Hammadde")]
            HAMMADDE = 18,

            [Display(Name = "Mamül")]
            MAMUL = 19,

            [Display(Name = "Yarı Mamül")]
            YARIMAMUL = 21,                               
        }
    }
}