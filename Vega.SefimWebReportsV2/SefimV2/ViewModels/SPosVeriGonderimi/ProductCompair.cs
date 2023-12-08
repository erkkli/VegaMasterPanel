using System.Collections.Generic;
using System.ComponentModel;

namespace SefimV2.ViewModels.SPosVeriGonderimi
{
    public class ProductCompair
    {
        public int Id { get; set; }
        public int PkId { get; set; }
        public int ProductId { get; set; }
        public string ProductGroup { get; set; }
        public string ProductName { get; set; }
        public List<UrunEdit2> SubeList { get; set; }
        public string IsManuelInsert { get; set; }

        #region Choice       
        public int Choice1Id { get; set; }
        public int Choice1PkId { get; set; }
        public int Choice1VarMi { get; set; }

        [DisplayName("Choice(1) Fiyat")]
        public decimal ChoicePrice { get; set; }

        [DisplayName("Choice(1) Adı")]
        public string ChoiceProductName { get; set; }
        //Choice2 
        public int Choice2Id { get; set; }
        public int Choice2VarMi { get; set; }
        #endregion Choice

        #region Options
        public int OptionsId { get; set; }
        public int OptionsVarMi { get; set; }

        [DisplayName("Options Fiyat")]
        public decimal OptionsPrice { get; set; }

        [DisplayName("Options Adı")]
        public string OptionsProductName { get; set; }
        #endregion Options
    }

    public class UrunEdit2
    {
        public decimal? SubePriceValue { get; set; }
        public long SubeId { get; set; }
        public string SubeName { get; set; }
        public int PkId { get; set; }//Şablon fiyat güncellemede ürün İd alanını alır.
    }

    public class ProductCompairSube
    {
        public string SubeAdi { get; set; }
        public long SubeId { get; set; }
    }
}