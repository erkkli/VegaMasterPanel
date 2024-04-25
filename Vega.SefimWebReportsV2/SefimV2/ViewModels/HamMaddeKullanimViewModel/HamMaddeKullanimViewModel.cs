using static SefimV2.Enums.General;

namespace SefimV2.Models
{
    public class HamMaddeKullanimViewModel
    {
        public int id { get; set; }
        public string Sube { get; set; } = "";
        public string SubeId { get; set; } = "";

        //Recete Maliyet detay
        public string HMADI { get; set; }
        public string BIRIMADI { get; set; }
        public decimal? ReceteBirimMaliyeti { get; set; }
        public decimal? ReceteKullanilanTutar { get; set; }
        public decimal? DonemBasiEnvanter { get; set; }
        public decimal? DonemIciGonderilen { get; set; }
        public decimal? DonemIciCikan { get; set; }
        public decimal? Kalan { get; set; }        
        public HamMaddeKullanimViewModel() { }
    }
}