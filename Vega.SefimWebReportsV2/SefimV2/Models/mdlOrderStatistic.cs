using System.Collections.Generic;

namespace SefimV2.Models
{
    public class mdlOrderStatistic
    {
        public decimal TOPLAMSIPARISTUTARI { get; set; }
        public decimal TOPLAMSIPARISSAYISI { get; set; }
        public string TOPLAMSIPARISSAYISI_STR { get; set; }
        public List<Product> ListProduct { get; set; }
    }
    public class Product
    {
        public string URUNADI { get; set; }
        public int URUNSAYISI { get; set; }
        public string STOKKODU { get; set; }
        public string RESIM { get; set; }
    }
}