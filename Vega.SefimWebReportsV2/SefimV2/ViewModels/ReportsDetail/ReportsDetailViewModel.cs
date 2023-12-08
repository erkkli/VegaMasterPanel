namespace SefimV2.ViewModels.ReportsDetail
{
    public class ReportsDetailViewModel
    {
        public string StartDate { get; set; }
        public string EndDate { get; set; }
      


        public string SubeID { get; set; }
        public decimal Miktar { get; set; }
        public string Sube { get; set; } = "";
        public string ProductName { get; set; } = "";
        public decimal Debit { get; set; }
        public decimal TotalDebit { get; set; }
        public int UrunID { get; set; }
        public string ProductGroup { get; set; }
        public string SaatList { get; set; }
        public string FilterUrunAdiList { get; set; }
    }
}