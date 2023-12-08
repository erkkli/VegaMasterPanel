using System.ComponentModel.DataAnnotations;

namespace SefimV2.Enums
{
    public partial class General
    {
        public enum OnayDurumu : int
        {
            [Display(Name = "Onaylı")]
            Onayli = 1,
            [Display(Name = "Onay Bekliyor")]
            OnayBekliyor = 0,
            [Display(Name = "Ret")]
            Ret = -1
        }
    }
}