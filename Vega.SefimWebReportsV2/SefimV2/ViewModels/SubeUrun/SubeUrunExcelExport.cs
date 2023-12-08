namespace SefimV2.Models
{
    public class SubeUrunExcelExport
    {
        public string SubeID { get; set; }
        public decimal Miktar { get; set; }
        public string Sube { get; set; } = "";
        public string ProductName { get; set; } = "";
        public decimal Debit { get; set; }
        public double GecenZaman { get; set; }
        public decimal TotalDebit { get; set; }
        public string ErrorMessage { get; set; }
        public bool ErrorStatus { get; set; }
        public string ErrorCode { get; set; }
        public string TarihGun { get; set; }
        public string TarihSaat { get; set; }
        public decimal Indirim { get; set; }
        public decimal Ikram { get; set; }
        public decimal NetTutar { get; set; }
        //Recete maliyet
        public decimal ReceteTutari { get; set; }
        public decimal KarTutari { get; set; }
        public decimal ReceteBirimMaliyeti { get; set; }
        public decimal ReceteToplamMiktari { get; set; }        
        //Recete Maliyet detay
        public string HMKODU { get; set; }
        public string HMADI { get; set; }
        //public decimal Miktar { get; set; }
        public string BIRIMADI { get; set; }
        public decimal MaliyetTutari { get; set; }    
        public decimal MALIYET { get; set; }
    }
}