using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace SefimV2.ViewModels.SPosVeriGonderimi
{
    public class UrunEditViewModel2
    {
        public string SubeIdGrupList { get; set; }
        public string KullaniciId { get; set; }
        public string SablonName { get; set; }
        //ürün gruplarını alan combobox
        public string SubeyeGoreUrunGrubuComboList { get; set; }
    
        public bool IsSuccess { get; set; }//Hataya düştüğünde=false
        public List<string> ErrorList { get; set; }
        public List<ProductCompairSube> SubeList { get; set; }
        public List<UrunEditItemViewModel> ProductList { get; set; }
        public List<UrunEditItemViewModel> ProductOptionsList { get; set; }
    }

    public class UrunEditItemViewModel
    {
        public int Id { get; set; }
        public string ProductGroup { get; set; }
        public string ProductName { get; set; }
     
        [DisplayName("Choice(1) Adı")]
        public string ChoiceProductName { get; set; }

        [DisplayName("Options Adı")]
        public string OptionsProductName { get; set; }
        public List<UrunEditPrice> UrunEditPriceList { get; set; }
    }
    public class UrunEditPrice
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public long SubeId { get; set; }

        [DisplayName("Ürün Fiyatı")]
        //[Required(ErrorMessage = "{0} bilgisi giriniz.")]
        [Range(0, 999999999999999, ErrorMessage = "{0} {1} ile {2} değerleri arasında olabilir.")]
        //[RegularExpression(@"\d+(\,\d{1,2})?", ErrorMessage = "{0} değeri virgülden sonra en fazla 2 basamak olabilir.")]
        [DataType(DataType.Currency)]
        [DisplayFormat(DataFormatString = "{0:C}")]
        public decimal? Price { get; set; }
        public int Choice1VarMi { get; set; }
        public int OptionsVarMi { get; set; }
        public bool IsManuelInsert { get; set; }

        #region Choice       
        public int Choice1Id { get; set; }
        public int Choice1PkId { get; set; }

        [DisplayName("Choice(1) Fiyat")]
        public decimal? ChoicePrice { get; set; }
   
        //Choice2 
        public int Choice2Id { get; set; }
        public int Choice2VarMi { get; set; }
        #endregion Choice

        #region Options
        public int OptionsId { get; set; }

        [DisplayName("Options Fiyat")]
        public decimal? OptionsPrice { get; set; }

        #endregion Options
    }
}