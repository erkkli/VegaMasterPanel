using System.ComponentModel.DataAnnotations;

namespace SefimV2.Models
{
    public class UrunGrubu
    {
        public string SubeID { get; set; }
        public decimal Miktar { get; set; }
        public string Sube { get; set; } = "";
        public string ProductName { get; set; } = "";
        public string ProductGroup { get; set; } = "";

        //[System.ComponentModel.DataAnnotations.DisplayFormat(DataFormatString = "{0:n2}", ApplyFormatInEditMode = true)]
        [DisplayFormat(DataFormatString = "{0:n2}", ApplyFormatInEditMode = true)]
        [System.ComponentModel.DataAnnotations.RegularExpression(@"^[0-9]{1,4}([,][0-9]{1,2})?$")]
        public decimal Debit { get; set; }
        public double GecenZaman { get; set; }
        public decimal TotalDebit { get; set; }
        public decimal ToplamMiktar { get; set; }

        public string ErrorMessage { get; set; }
        public bool ErrorStatus { get; set; }
        public string ErrorCode { get; set; }

        public UrunGrubu() { }

        public decimal Indirim { get; set; }
        public decimal Ikram { get; set; }
        public decimal NetTutar { get; set; }
    }
}