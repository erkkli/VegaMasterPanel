using System;

namespace SefimV2.Models
{
    public class KasaCiro
    {

        public int id { get; set; }
        public string Sube { get; set; } = "";
        public string SubeId { get; set; } = "";
        public decimal Ciro { get; set; }
        public decimal Cash { get; set; }
        public decimal Credit { get; set; }
        public decimal Ticket { get; set; }
        public double GecenZaman { get; set; }
        public string ErrorMessage { get; set; }
        public bool ErrorStatus { get; set; }
        public string ErrorCode { get; set; }

        public decimal GelirGider { get; set; } = 0;
        public decimal OnlinePayment { get; set; }




        #region SUBSTATION TOTAL (CİRO)
        public DateTime DateStr { get; set; }
        public double TotalCost { get; set; }
        public decimal TotalCost2 { get; set; }
        public int DayNumber { get; set; }
        public string CRITER { get; set; }
        public string ToplamCiro { get; set; }
        public int DayCount { get; set; }
        //
        public string Tarih { get; set; }
        public string Saat { get; set; }
        #endregion SUBSTATION TOTAL (CİRO)
        public KasaCiro() { }

        //DetayRapor2
        public string StartDate { get; set; }
        public string EndDate { get; set; }

        public double Tutar { get; set; }

    }
}