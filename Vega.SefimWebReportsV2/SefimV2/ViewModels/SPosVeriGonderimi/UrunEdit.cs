using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace SefimV2.ViewModels.SPosVeriGonderimi
{
    public class UrunEditViewModel
    {
        public int SubeId { get; set; }
        public string SubeAdi { get; set; }
        public string SubeIdGrupList { get; set;}
        public List<UrunEdit> UrunEditList { get; set; }
        public List<UrunEdit> UrunOptionsEditList { get; set; }
        public bool IsSuccess { get; set; }//Hataya düştüğünde=false
        public List<string> ErrorList { get; set; }
        public List<ProductCompair> productCompairsList { get; set; }
        public List<ProductCompair> productOptionsCompairsList { get; set; }
        public List<ProductCompairSube> SubeList { get; set; }
        public string KullaniciId { get; set; }
        public string SablonName { get; set; }
        //ürün gruplarını alan combobox
        public string SubeyeGoreUrunGrubuComboList { get; set; }

        //Yeni eklendi ch1 Name taşımak için.
        public string Choice1ProductName { get; set; }

        public class UrunEdit
        {
            public int Id { get; set; }

            public int SubeIdLocal { get; set; }
            public string SubeAdiLocal { get; set; }

            [DisplayName("Ürün Adı")]
            //[Required(ErrorMessage = "{0} alanı boş bırakılamaz")]
            public string ProductName { get; set; }

            //[Required(ErrorMessage = "{0} alanı boş bırakılamaz")]
            [DisplayName("Ürün Grubu")]
            public string ProductGroup { get; set; }

            [DisplayName("Ürün Kodu")]
            public string ProductCode { get; set; }

            [DisplayName("Sipariş")]
            public string Order { get; set; }

            [DisplayName("Ürün Fiyatı")]
            //[Required(ErrorMessage = "{0} bilgisi giriniz.")]
            [Range(0, 999999999999999, ErrorMessage = "{0} {1} ile {2} değerleri arasında olabilir.")]
            //[RegularExpression(@"\d+(\,\d{1,2})?", ErrorMessage = "{0} değeri virgülden sonra en fazla 2 basamak olabilir.")]
            [DataType(DataType.Currency)]
            [DisplayFormat(DataFormatString = "{0:C}")]
            public decimal Price { get; set; }

            //[Required(ErrorMessage = "{0} alanı boş bırakılamaz")]
            [DisplayName("KDV")]
            public decimal VatRate { get; set; }
            public bool FreeItem { get; set; }
            public string InvoiceName { get; set; }
            public string ProductType { get; set; }
            public string Plu { get; set; }
            public bool SkipOptionSelection { get; set; }
            public string Favorites { get; set; }
            public bool Aktarildi { get; set; }

            #region Choice1, Choice2

            public int Choice1Id { get; set; }
            public int Choice2Id { get; set; }
            
            [DisplayName("Seçim-1 Fiyat")]
            //[Required(ErrorMessage = "{0} bilgisi giriniz.")]
            [Range(0, 999999999999999, ErrorMessage = "{0} {1} ile {2} değerleri arasında olabilir.")]
            [DataType(DataType.Currency)]
            [DisplayFormat(DataFormatString = "{0:C}")]
            public decimal ChoicePrice { get; set; }

            [DisplayName("Seçim-1 Adı")]
            //[Required(ErrorMessage = "{0} alanı boş bırakılamaz")]
            public string ChoiceProductName { get; set; }

            [DisplayName("Seçim-2 Fiyat")]
            //[Required(ErrorMessage = "{0} bilgisi giriniz.")]
            [Range(0, 999999999999999, ErrorMessage = "{0} {1} ile {2} değerleri arasında olabilir.")]
            [DataType(DataType.Currency)]
            [DisplayFormat(DataFormatString = "{0:C}")]
            public decimal Choice2Price { get; set; }

            [DisplayName("Seçim-2 Adı")]
            //[Required(ErrorMessage = "{0} alanı boş bırakılamaz")]
            public string Choice2ProductName { get; set; }
        
            #endregion Choice1, Choice2

            #region Options

            public int OptionsId { get; set; }
            
            [DisplayName("Seçenek Fiyat")]
            //[Required(ErrorMessage = "{0} bilgisi giriniz.")]
            [Range(0, 999999999999999, ErrorMessage = "{0} {1} ile {2} değerleri arasında olabilir.")]
            [DataType(DataType.Currency)]
            [DisplayFormat(DataFormatString = "{0:C}")]
            public decimal OptionsPrice { get; set; }

            [DisplayName("Seçenek Adı")]
            //[Required(ErrorMessage = "{0} alanı boş bırakılamaz")]
            public string OptionsProductName { get; set; }

            [DisplayName("Katagori Adı")]
            public string OptionsCategory { get; set; }

            #endregion Options

            #region Tarihçe

            [DisplayName("Güncellenme Tarihi")]
            public string IsUpdateDate { get; set; }

            [DisplayName("Güncelleyen Kullanıcı")]
            public string IsUpdateKullanici { get; set; }

            #endregion

        }
    }
}