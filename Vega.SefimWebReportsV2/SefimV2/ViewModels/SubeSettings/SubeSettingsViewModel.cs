using System.Collections.Generic;
using static SefimV2.Enums.General;

namespace SefimV2.ViewModels.SubeSettings
{
    public class SubeSettingsViewModel
    {
        public int ID { get; set; }
        public string SubeName { get; set; }
        public string SubeIP { get; set; }
        public string SqlName { get; set; }
        public string SqlPassword { get; set; }
        public string DBName { get; set; }
        public string FirmaID { get; set; }
        public string DonemID { get; set; }
        public string DepoID { get; set; }
        public bool Status { get; set; }
        public int AppDbType { get; set; }
        public bool AppDbTypeStatus { get; set; }
        public bool IsSelectedKaynakSube { get; set; }
        public bool IsSelectedHedefSube { get; set; }
        public bool IsSelectedHazirlananSubeList { get; set; }
        public string ZimmetCariInd { get; set; }
        public bool BelgeSayimTarihDahil { get; set; }
        public List<FR_FasterKasalar> FR_FasterKasaListesi { get; set; }
        public string FasterKasaListesi { get; set; }
        public class FR_FasterKasalar
        {
            public int ID { get; set; }
            public string KasaAdi { get; set; }
        }

        //mongo db için uzaktadaki şueb ip adresi ve portu
        public string ServiceAdress { get; set; }
        public long MongoDbServicesId { get; set; }
        public bool UrunEslestirmeVarMi { get; set; }

        //sablon name için 
        public string SablonName { get; set; }
        public int ProductTemplatePkId { get; set; }
        //Personel Yemek Raporu için eklendi. /*01102022*/
        public string PersonelYemekRaporu { get; set; }
        //VPos
        public string VPosSubeKodu { get; set; }
        public string VPosKasaKodu { get; set; }
        public List<VPosKasalarList> VPosKasalarList { get; set; }

        public string EnvanterOzelKodAdi { get; set; }
        public EnvanterOzelKodTipi EnvanterOzelKodTipi { get; set; }
    }

    public class VPosKasalarList
    {
        public string ID { get; set; }
        public string KasaAdi { get; set; }
    }
}