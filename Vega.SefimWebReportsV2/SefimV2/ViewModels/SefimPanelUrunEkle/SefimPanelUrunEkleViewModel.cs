using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SefimV2.Models
{
    public class SefimPanelUrunEkleViewModel
    {
        public long Id { get; set; }
        public string ProductName { get; set; }
        public string ProductGroup { get; set; }
        public string ProductCode { get; set; }
        public string Order { get; set; }
        public string Price { get; set; } //decimal
        //list
        public decimal ProductPrice { get; set; } //decimal
        public decimal Choice1Price { get; set; }//decimal
        public decimal Choice2Price { get; set; }//decimal
        public decimal OptionsPrice { get; set; }//decimal

        public string VatRate { get; set; }//decimal
        public bool FreeItem { get; set; }
        public string InvoiceName { get; set; }
        public string ProductType { get; set; }
        public string Plu { get; set; }
        public bool SkipOptionSelection { get; set; }
        public string Favorites { get; set; }
        public bool Aktarildi { get; set; }
        public int SubeId { get; set; }
        public string SubeName { get; set; }
        public int ProductPkId { get; set; }
        public bool IsUpdate { get; set; }
        public bool YeniUrunMu { get; set; }
        public bool R { get; set; }
        public bool S { get; set; }

        public List<Choice1> Choice1 { get; set; }
        public List<Choice2> Choice2 { get; set; }
        public List<Options> Options { get; set; }
        public List<OptionCats> OptionCats { get; set; }
        public List<Bom> Boms { get; set; }
        public List<BomOptions> BomOptionss { get; set; }
    }
    public class Choice1
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string Name { get; set; }
        public string Price { get; set; }//decimal      
        public Guid secimId { get; set; }
    }
    public class Choice2
    {
        public int Id { get; set; }
        public Guid secim1Id { get; set; }
        public int ProductId { get; set; }
        public int Choice1Id { get; set; }
        public string Name { get; set; }
        public string Price { get; set; }//decimal
       
    }
    public class Options
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string Name { get; set; }
        public string OptionCatsName { get; set; }
        public string Price { get; set; }//decimal    
        public bool Quantitative { get; set; }
        public string Category { get; set; }
        public Guid? OptionCatsId { get; set; }
    }
    public class OptionCats
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string Name { get; set; }
        public string MaxSelections { get; set; }//decimal
        public string MinSelections { get; set; }
        public Guid? optionCatsId { get; set; }
    }

    public class Bom
    {
        public int Id { get; set; }
        public string ProductName { get; set; }
        public string MaterialName { get; set; }
        public string MalinCinsi { get; set; }
        public string Quantity { get; set; }
        public string Unit { get; set; }
        public int StokID { get; set; }
        public int ProductId { get; set; }
        public int SubeId { get; set; }
        public bool YeniUrunMu { get; set; }
    }
    public class BomOptions
    {
        public int Id { get; set; }
        public int OptionsId { get; set; }
        public string OptionsName { get; set; }
        public string MaterialName { get; set; }
        public string Quantity { get; set; }
        public string Unit { get; set; }
        public int StokID { get; set; }
        public string ProductName { get; set; }
        public int SubeId { get; set; }
        public bool YeniUrunMu { get; set; }
        public int ProductId { get; set; }
    }
    public class SelectedProductItem
    {
        public int Id { get; set; }
        public bool YeniUrunMu { get; set; }
        public int SubeId { get; set; }
    }
}