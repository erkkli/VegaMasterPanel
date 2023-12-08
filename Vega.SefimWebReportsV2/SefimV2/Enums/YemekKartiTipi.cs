using System.ComponentModel.DataAnnotations;

namespace SefimV2.Enums
{
    public partial class General
    {
        public enum YemekKartiTipi : byte
        {
            [Display(Name = "Diğer Kartlar")]
            DigerCardlar = 66,

            [Display(Name = "Multinet")]
            Multinet = 1,

            [Display(Name = "Sodexo")]
            Sodexo = 2,

            [Display(Name = "SetCard")]
            SetCard = 3,

            [Display(Name = "Ticket")]
            Ticket = 4,

            [Display(Name = "MetropolCard")]
            MetropolCard = 5,

            [Display(Name = "Yemekmatik Kart")]
            YemekmatikCard = 6,

            [Display(Name = "Paye Kart")]
            PayeCard = 7,           
        }
    }
}