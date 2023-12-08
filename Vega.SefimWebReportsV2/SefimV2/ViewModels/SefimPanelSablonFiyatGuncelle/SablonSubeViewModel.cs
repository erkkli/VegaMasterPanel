using SefimV2.ViewModels.SPosVeriGonderimi;
using SefimV2.ViewModels.SubeSettings;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace SefimV2.ViewModels.SefimPanelSablonFiyatGuncelle
{
    public class SablonSubeViewModel
    {
        public string HedefSubeId { get; set; }//Tek şubeden >çok şubeye
        public List<SablonSube> SablonSubeList { get; set; }
        public List<SablonSube> FiyatGuncellemsiHazirlananSubeList { get; set; }

        public List<GuncellenecekSubeGruplari> GuncellenecekSubeGruplariList { get; set; }
        public List<SubeSettingsViewModel> IsSelectedSubeList { get; set; }
    }

    public class SablonSube
    {
        public int Id { get; set; }
        public int SubeId { get; set; }
        [DisplayName("Şube Adı")]
        public string SubeAdi { get; set; }
        [DisplayName("Şablon Adı")]
        public string SablonAdi { get; set; }
        public int ProductTemplatePkId { get; set; }
        public bool IsSelectedSablon { get; set; }
        bool IsUpdated { get; set; }
    }
}