using System;

namespace SefimV2.Models
{
    public class SubeSatislar
    {
        public int Id { get; set; }
        public int IslemTipi { get; set; }
        public int OdemeDurumu { get; set; }
        public DateTime Tarih { get; set; }
        public string ErrorMessage { get; set; }
        public bool ErrorStatus { get; set; }
        public string ErrorCode { get; set; }
        public string Sube { get; set; } = "";
        public string SubeId { get; set; } = "";
        public SubeSatislar() { }
    }
}