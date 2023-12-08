namespace SefimV2.Models
{
    public class SubeCiro
    {
        public int id { get; set; }
        public string Sube { get; set; } = "";
        public string SubeId { get; set; } = "";
        public decimal Ciro { get; set; } = 0;
        public decimal Cash { get; set; }
        public decimal Credit { get; set; }
        public decimal Ticket { get; set; }
        public decimal Debit { get; set; }
        public decimal ikram { get; set; }
        public decimal TableNo { get; set; } = 0;
        public decimal Discount { get; set; }
        public decimal iptal { get; set; }
        public decimal iade { get; set; }
        public decimal Zayi { get; set; }
        public decimal PaketSatis { get; set; }
        public double GecenZaman { get; set; }
        public string ErrorMessage { get; set; }
        public bool ErrorStatus { get; set; }
        public string ErrorCode { get; set; }
        public decimal OpenTable { get; set; }
        public decimal GelirGider { get; set; }
        public string ToplamCiro { get; set; }
        public string ToplamIkram { get; set; }
        public string ToplamTableNo { get; set; }
        public string ToplamDiscount { get; set; }
        public string ToplamIptal { get; set; }
        public string ToplamZayi { get; set; }
        public decimal OnlinePayment { get; set; }
        public decimal AdisyonOrtalamasi { get; set; }

        public decimal Bakiye { get; set; } = 0;
        public SubeCiro() { }
    }
}