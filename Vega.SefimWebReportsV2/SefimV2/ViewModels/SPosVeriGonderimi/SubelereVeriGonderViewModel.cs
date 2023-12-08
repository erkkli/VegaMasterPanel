using SefimV2.ViewModels.SubeSettings;
using System.Collections.Generic;

namespace SefimV2.ViewModels.SPosVeriGonderimi
{
    public class SubelereVeriGonderViewModel
    {
        public string SubeIdGrupList { get; set; }
        public string HedefSubeId { get; set; }//Tek şubeden >çok şubeye
        public List<SubeSettingsViewModel> IsSelectedSubeList { get; set; }
        public List<GuncellenecekSubeGruplari> GuncellenecekSubeGruplariList { get; set; }
        //public List<SubeSettingsViewModel> FiyatGuncellemsiHazirlananSubeList { get; internal set; }

        public List<string> SuccessAlertList { get; set; }
    }

    public class GuncellenecekSubeGruplari
    {
        bool SubelereGonderilsinMi { get; set; }
        public string GuncellenecekSubeGrupAdi { get; set; }
        public string GuncellenecekSubeGrupId { get; set; }
        public string  TumSubelereYay { get; set; }
        public string GuncellenecekSubeSablonAdi { get; set; }//Şablon için kullanılacak prop.
        public string GuncellenecekSubeSablonProductTemplatePkId { get; set; }//Şablon için kullanılacak prop.
        public List<SubeSettingsViewModel> FiyatGuncellemsiHazirlananSubeList { get; set; }
    }
}