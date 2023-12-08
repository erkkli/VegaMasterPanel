using System;
using System.Collections.Generic;
using System.Web.Mvc;
using static SefimV2.Enums.General;

namespace SefimV2.ViewModels.User
{
    public class UserViewModel
    {
        public int ID { get; set; }
        public DateTime CreateDate { get; set; }
        public int CreateDate_Timestamp { get; set; }
        public int ModifyCounter { get; set; }
        public DateTime UpdateDate { get; set; }
        public int UpdateDate_Timestamp { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public int IsAdmin { get; set; }
        public int? SubeID { get; set; }
        public string Name { get; set; }
        public string EMail { get; set; }
        public string Gsm { get; set; }
        public bool Status { get; set; }
        public string DepoID { get; set; } //kullanıcının tanımlı deposu SubeSettings tablosundan alınıyor.
        public int FirmaID { get; set; } //kullanıcının tanımlı firma SubeSettings tablosundan alınıyor.
        public int DonemID { get; set; } //kullanıcının tanımlı dönem SubeSettings tablosundan alınıyor.
        public string FasterSubeID { get; set; } //kullanıcının tanımlı SubeID SubeSettings tablosundan alınıyor.
        public string BelgeTipYetkisi { get; set; }
        public string SefimPanelZimmetCagrisi { get; set; }
        public bool BelgeSayimTarihDahil { get; set; }
        public List<SelectListItem> SubeList { get; set; }

        public string ErrorMessages { get; set; }

        public List<FR_SubeListesiViewModel> FR_SubeListesi { get; set; }

        public class FR_SubeListesiViewModel
        {
            public int ID { get; set; }
            public string SubeName { get; set; }
            public int SubeID { get; set; }
            public int UserID { get; set; }
        }
        public string YetkiListesi { get; set; }
        public string YetkiStatus { get; set; }
        public string YetkiStatusAciklama { get; set; }
        public bool SubeSirasiGorunsunMu { get; set; }
        public UygulamaTipi UygulamaTipi { get; set; }
        public string Uygulama { get; set; }

        public BelgeTipi BelgeTipiYetki { get; set; }
        public string BelgeTipiYetkiList { get; set; }
        public IEnumerable<SelectListItem> ActionsList { get; set; }
        public string YetkiNesneAdi { get; set; }//Yetki Tanımlama için
        public int YetkiNesneId { get; set; }//
        public UserViewModel() { }
    }
}