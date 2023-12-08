using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using System.Data.OleDb;
using System.IO;
using System.Web.Hosting;

namespace SefimV2.Models
{
    public class SubeCiro
    {

        public int id { get; set; }
        public string Sube { get; set; } = "";
        public string SubeId { get; set; } = "";
        public decimal Ciro { get; set; }
        public decimal Cash { get; set; }
        public decimal Credit { get; set; }
        public decimal Ticket { get; set; }
        public decimal Debit { get; set; }
        public decimal ikram { get; set; }
        public decimal TableNo { get; set; }
        public decimal Discount { get; set; }
        public decimal iptal { get; set; }
        public decimal Zayi { get; set; }
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

//        window.setTimeout(function () { $('#ToplamCiroID').html(data['ToplamCiro'] + ' %');
//    }, 1000);
//                        window.setTimeout(function () { $('#ToplamAdisyonID').html(data['ToplamTableNo']);
//}, 1000);
//                        window.setTimeout(function () { $('#ToplamIkramID').html(data['Toplamikram'] + ' %'); }, 1000);
//                        window.setTimeout(function () { $('#ToplamZayiID').html(data['ToplamZayi']); }, 1000);
//                        window.setTimeout(function () { $('#ToplamIndirimID').html(data['ToplamDiscount'] + ' %'); }, 1000);
//                        window.setTimeout(function () { $('#ToplamIptalID').html(data['Toplamiptal']); }, 1000);
        public SubeCiro() { }      

    }
}