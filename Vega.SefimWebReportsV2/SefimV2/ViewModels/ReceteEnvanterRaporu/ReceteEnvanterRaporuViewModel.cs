using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SefimV2.ViewModels.ReceteEnvanterRaporu
{
    public class ReceteEnvanterRaporuViewModel
    {
        #region Alış - Satış ViewModel

        public decimal DONBASISTOK { get; set; }
        public decimal DONICIGRENLER { get; set; }
        public decimal TOPENV { get; set; }
        public decimal ORTMALIYET { get; set; }
        public decimal ORTMALIYETTUTARI { get; set; }
        public decimal TOPENVTUTAR { get; set; }
        public decimal SATISMIKTAR { get; set; }
        public decimal SATISTUTAR { get; set; }
        public decimal DONICISATMALIYET { get; set; }
        public decimal DONICIBRUTKAR { get; set; }

        #endregion Alış - Satış ViewModel

        #region Personel Satış
        public string UserName { get; set; }
        public string ProductName { get; set; }
        public decimal Miktar { get; set; }
        public decimal Tutar { get; set; }
        public decimal BirimFiyat { get; set; }

        #endregion Personel Satış


        #region Sayım Dönemi
        public string SayimDonemi { get; set; }
        public decimal ReceteMaliyeti { get; set; }
        public decimal GerceklesenMaliyet { get; set; }
        #endregion Sayım Dönemi


        #region Şube Bazlı Satış

       /// personel ile aynı
         

        #endregion Şube Bazlı Satış

    }
}