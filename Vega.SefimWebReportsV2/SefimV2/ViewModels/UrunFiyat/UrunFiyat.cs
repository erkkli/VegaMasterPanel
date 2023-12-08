namespace SefimV2.Models
{
    public class UrunFiyat
    {  //[ProductName]
        //,[ProductGroup]
        //,[ProductCode]
        //,[Order]
        //,[Price]
        //,[VatRate]
        //,[FreeItem]
        //,[InvoiceName]
        //,[ProductType]
        //,[Plu]
        //,[SkipOptionSelection]
        //,[Favorites]
        //,[Aktarildi]
        //,[IsSynced]
        //,[IsUpdated]

        public string SubeID { get; set; }
        public string Sube { get; set; } = "";
        public string ProductName { get; set; } = "";
        public string ProductGroup { get; set; } = "";
        public string ProductCode { get; set; }
        public string Order { get; set; }
        public decimal Price { get; set; }
        public decimal VatRate { get; set; }
        public string FreeItem { get; set; }
        public string InvoiceName { get; set; }
        public string ProductType { get; set; }
        public string Plu { get; set; }
        public string SkipOptionSelection { get; set; }
        public string Favorites { get; set; }

        //[dbo].[Choice1]
        //([ProductId]
        //,[Name]
        //,[Price]
        //,[Aktarildi]
        //,[IsSynced]
        //,[IsUpdated])

        public string Choice1ProductId { get; set; }
        public string Choice1Name { get; set; }
        public string Choice1Price { get; set; }

        //[dbo].[Choice2]
        //([ProductId]
        //,[Choice1Id]
        //,[Name]
        //,[Price]
        //,[Aktarildi]
        //,[IsSynced]
        //,[IsUpdated])

        public string Choice2ProductId { get; set; }
        public string Choice1Id { get; set; }
        public string Choice2Name { get; set; }
        public string Choice2Price { get; set; }
    }
}