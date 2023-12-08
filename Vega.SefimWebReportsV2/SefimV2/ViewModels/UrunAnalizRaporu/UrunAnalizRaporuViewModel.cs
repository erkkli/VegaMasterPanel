using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SefimV2.ViewModels.UrunAnalizRaporu
{
    public class UrunAnalizRaporuViewModel
    {
        public string SubeID { get; set; }      
        public string Sube { get; set; } = "";
        public string ProductName { get; set; } = "";
        public decimal Miktar { get; set; }
        public decimal Tutar { get; set; }
        public decimal BirimFiyat { get; set; }
        public decimal ReceteMaliyet { get; set; }
        public decimal CostMaliyet { get; set; }
        public decimal ReceteKar { get; set; }
        public decimal CostKar { get; set; }


        #region Alış - Satış ViewModel

        public decimal DONBASISTOK { get; set; }
        public decimal DONICIGRENLER { get; set; }
        public decimal TOPENV { get; set; }
        public decimal ORTMALIYET { get; set; }
        public decimal DONICIGIRTUTAR { get; set; }
        public decimal TOPENVTUTAR { get; set; }
        public decimal SATISMIKTAR { get; set; }
        public decimal SATISTUTAR { get; set; }
        public decimal DONICISATMALIYET { get; set; }
        public decimal DONICIBRUTKAR { get; set; }

        #endregion Alış - Satış ViewModel



        #region Sayım Dönemi
        public string SayimDonemi { get; set; }
        public decimal ReceteMaliyeti { get; set; }
        public decimal GerceklesenMaliyet { get; set; }
        #endregion Sayım Dönemi



        #region Personel Satış
        public string UserName { get; set; }
        //public string ProductName { get; set; }
        public decimal PersonelMiktar { get; set; }
        public decimal personelTutar { get; set; }
        //public decimal BirimFiyat { get; set; }

        #endregion Personel Satış

        #region Şube Ürün  Satış
        public string UrunSube { get; set; }
        //public string ProductName { get; set; }
        public decimal UrunMiktar { get; set; }
        public decimal UrunTutar { get; set; }
        //public decimal BirimFiyat { get; set; }

        #endregion Şube Ürün  Satış

    }
}