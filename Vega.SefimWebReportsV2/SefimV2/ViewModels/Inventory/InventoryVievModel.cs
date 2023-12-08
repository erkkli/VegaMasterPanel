using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SefimV2.ViewModels.Inventory
{
    public class InventoryVievModel
    {
        public int id { get; set; }
        public int SubeID { get; set; }
        public string Sube { get; set; } = "";
        public string UrunAdi { get; set; }
        public decimal Envanter { get; set; }
        public string ErrorMessage { get; set; }
        public bool ErrorStatus { get; set; }
        public string ErrorCode { get; set; }
        public InventoryVievModel() { }


        public decimal CanliEnvanter { get; set; }
        public decimal CanliSatislar { get; set; }
        public string StokKodu { get; set; }
    }
}