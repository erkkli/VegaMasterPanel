using System.Collections.Generic;
using System.ComponentModel;

namespace SefimV2.ViewModels.SPosVeriGonderimi
{
    public class ProductTableColumnMapping
    {

//  [Id] [int] IDENTITY(1,1) NOT NULL,
//  [ProductName] [nvarchar] (450) NULL,
//	[ProductGroup] [nvarchar] (50) NULL,
//	[ProductCode] [nvarchar] (50) NULL,
//	[Order] [nvarchar] (50) NULL,
//	[Price] [decimal](10, 2) NULL,
//	[VatRate] [decimal](4, 2) NULL,
//	[FreeItem] [bit] NULL,
//	[InvoiceName] [nvarchar] (50) NULL,
//	[ProductType] [nvarchar] (50) NULL,
//	[Plu] [nvarchar] (50) NULL,
//	[SkipOptionSelection] [bit] NULL,
//	[Favorites] [nvarchar] (255) NULL,
//	[Aktarildi] [bit] NULL,
//	[SubeId] [int] NOT NULL,
//  [SubeName] [varchar] (255) NOT NULL, 
//  [ProductPkId] [int] NOT NULL, 
//   [IsUpdate] [bit] NULL,
//	[YeniUrunMu] [bit] NULL,
//	[GuncellenecekSubeIdGrubu] [nvarchar] (255) NULL,
//	[GuncellenecekSubeAdiGrubu] [nvarchar] (255) NULL,


        public int Id { get; set; }
        public string ProductName { get; set; }
        public string ProductGroup { get; set; }
        public string ProductCode { get; set; }
        public string Order { get; set; }
        public decimal Price { get; set; }
        public decimal VatRate { get; set; }
        public bool FreeItem { get; set; }
        public string InvoiceName { get; set; }
        public string ProductType { get; set; }
        public string Plu { get; set; }
        public bool SkipOptionSelection { get; set; }
        public string Favorites { get; set; }
    }   
}