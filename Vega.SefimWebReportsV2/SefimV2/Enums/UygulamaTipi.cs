using System.ComponentModel.DataAnnotations;

namespace SefimV2.Enums
{
    public partial class General
    {
        public enum UygulamaTipi : byte
        {
            [Display(Name = "Vaga Master")]
            VegaMaster = 1,
            [Display(Name = "Şefim Panel")]
            SefimPanel = 2,
            [Display(Name = "Vega Master-Şefim Panel")]
            VegaMasterSefimPanel = 3,
        }
    }
}