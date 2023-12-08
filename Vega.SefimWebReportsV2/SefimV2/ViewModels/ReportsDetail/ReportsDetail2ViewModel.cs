using SefimV2.Models;
using System.Collections.Generic;
using System.ComponentModel;

namespace SefimV2.ViewModels.ReportsDetail
{
    public class ReportsDetail2ViewModel
    {
        [DisplayName("Başlama Tarih")]
        public string BaslamaTarihi { get; set; }

        [DisplayName("Bitiş Tarih")]
        public string BitisTarihi { get; set; } 
        public string Sube { get; set; } 
        public string Saat { get; set; }
        public List<SubeCiro> AramaSonucu { get; set; }
    }
}