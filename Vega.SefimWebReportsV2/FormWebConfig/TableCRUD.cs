using System;
using System.Data.SqlClient;

namespace SefimDesktop
{
    public class TableCRUD
    {
        internal static void CreateTables(string connectionString)
        {
            try
            {
                string[] createTableList = {

                @"  CREATE TABLE [dbo].[GroupPermissions](
                [ID] [bigint] IDENTITY(1,1) NOT NULL,
                [CreateDate] [datetime] NULL,
                [CreateDate_Timestamp] [bigint] NULL,
                [ModifyCounter] [bigint] NULL,
                [UpdateDate] [datetime] NULL,
                [UpdateDate_Timestamp] [bigint] NULL,
                [CommandID] [bigint] NULL,
                [GroupID] [bigint] NULL,
                PRIMARY KEY CLUSTERED 
                (
                [ID] ASC
                )WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
                ) ON [PRIMARY]",

                @"  CREATE TABLE [dbo].[Groups](
                [ID] [bigint] IDENTITY(1,1) NOT NULL,
                [CreateDate] [datetime] NULL,
                [CreateDate_Timestamp] [bigint] NULL,
                [ModifyCounter] [bigint] NULL,
                [UpdateDate] [datetime] NULL,
                [UpdateDate_Timestamp] [bigint] NULL,
                [DefaultGroup] [bit] NULL,
                [GroupName] [varchar](255) NULL,
                PRIMARY KEY CLUSTERED 
                (
                [ID] ASC
                )WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
                ) ON [PRIMARY]
                ",

                @"  CREATE TABLE [dbo].[SubeSettings](
                [ID] [bigint] IDENTITY(1,1) NOT NULL,
                [CreateDate] [datetime] NULL,
                [CreateDate_Timestamp] [bigint] NULL,
                [ModifyCounter] [bigint] NULL,
                [UpdateDate] [datetime] NULL,
                [UpdateDate_Timestamp] [bigint] NULL,
                [SubeName] [varchar](255) NOT NULL,
                [SubeIP] [varchar](255) NULL,
                [SqlName] [varchar](255) NULL,
                [SqlPassword] [varchar](255) NULL,
                [DBName] [varchar](255) NULL,
                [FirmaID] [varchar](255) NULL,
                [DonemID] [varchar](255) NULL,
                [DepoID] [varchar](255) NULL,
                [Status] [bit] NULL, 
                [AppDbType] [int] NULL,
                [AppDbTypeStatus] [bit] NULL,
                [FasterSubeID] [varchar](255) NULL,
                [SefimPanelZimmetCagrisi] [nvarchar](50) NULL,
                [BelgeSayimTarihDahil] [bit] NULL,
                [ServiceAdress] [nvarchar](255) NULL,
                [UrunEslestirmeVarMi] [bit] NULL,
                [PersonelYemekRaporuAdi] [varchar](500) NULL,
                [VPosSubeKodu] [varchar](255) NULL,
                [VPosKasaKodu] [varchar](255) NULL,

                PRIMARY KEY CLUSTERED 
                (
                [ID] ASC
                )WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY],
                CONSTRAINT [IX_1_SubeSettings]
                UNIQUE NONCLUSTERED 
                (
                [SubeName] ASC
                )WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
                ) ON [PRIMARY]
                ",

                @"  CREATE TABLE [dbo].[UserGroups](
                [ID] [int] IDENTITY(1,1) NOT NULL,
                [CreateDate] [datetime] NULL,
                [CreateDate_Timestamp] [bigint] NULL,
                [ModifyCounter] [bigint] NULL,
                [UpdateDate] [datetime] NULL,
                [UpdateDate_Timestamp] [bigint] NULL,
                [GroupID] [bigint] NOT NULL,
                [UserID] [bigint] NOT NULL,
                PRIMARY KEY CLUSTERED 
                (
                [ID] ASC
                )WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
                ) ON [PRIMARY]
                ",

                @"  CREATE TABLE [dbo].[Users](
                [ID] [bigint] IDENTITY(1,1) NOT NULL,
                [CreateDate] [datetime] NULL,
                [CreateDate_Timestamp] [bigint] NULL,
                [ModifyCounter] [bigint] NULL,
                [UpdateDate] [datetime] NULL,
                [UpdateDate_Timestamp] [bigint] NULL,
                [UserName] [varchar](255) NULL,
                [Password] [varchar](255) NULL,
                [IsAdmin] [bit] NULL,
                [SubeID] [bigint] NOT NULL,
                [Name] [nvarchar](255) NULL,
                [EMail] [varchar](255) NULL,
                [Gsm] [varchar](255) NULL,
                [Status] [bit] NULL,
                [SubeSirasiGorunsunMu] [bit] NULL,
                [UygulmaTipi] [bigint] NULL,
                [BelgeTipYetkisi] [varchar](255) NULL,
                [YetkiNesneId] [int] NULL,
                [YetkiNesneAdi] [nvarchar](250) NULL,
                PRIMARY KEY CLUSTERED 
                (
                [ID] ASC
                )WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY],
                CONSTRAINT [IX_1_Users] UNIQUE NONCLUSTERED 
                (
                [UserName] ASC
                )WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
                ) ON [PRIMARY]
                ",


                @"  CREATE TABLE [dbo].[UserSubeRelations](
                [ID] [bigint] IDENTITY(1,1) NOT NULL,
                [CreateDate] [datetime] NULL,
                [CreateDate_Timestamp] [bigint] NULL,
                [ModifyCounter] [bigint] NULL,
                [UpdateDate] [datetime] NULL,
                [UpdateDate_Timestamp] [bigint] NULL,
                [UserID] [bigint] NOT NULL,
                [SubeID] [bigint] NOT NULL,
                CONSTRAINT [PK_UserSubeRelations] PRIMARY KEY CLUSTERED 
                (
                [UserID] ASC,
                [SubeID] ASC
                )WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
                ) ON [PRIMARY]
                ",


                @"  CREATE TABLE [dbo].[UserTimeLine](
                [ID] [bigint] IDENTITY(1,1) NOT NULL,
                [CreateDate] [datetime] NULL,
                [CreateDate_Timestamp] [bigint] NULL,
                [ModifyCounter] [bigint] NULL,
                [UpdateDate] [datetime] NULL,
                [UpdateDate_Timestamp] [bigint] NULL,
                [StartTime] [varchar](255) NOT NULL,
                [EndTime] [varchar](255) NOT NULL,
                CONSTRAINT [PK_UserTimeLine] PRIMARY KEY CLUSTERED 
                (
                [StartTime] ASC,
                [EndTime] ASC
                )WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
                ) ON [PRIMARY]
                ",

                @"  CREATE TABLE [dbo].[UserToUserRelations](
                [ID] [bigint] IDENTITY(1,1) NOT NULL,
                [CreateDate] [datetime] NULL,
                [CreateDate_Timestamp] [bigint] NULL,
                [ModifyCounter] [bigint] NULL,
                [UpdateDate] [datetime] NULL,
                [UpdateDate_Timestamp] [bigint] NULL,
                [RootUserID] [bigint] NOT NULL,
                [UserID] [bigint] NOT NULL,
                CONSTRAINT [PK_UserToUserRelations] PRIMARY KEY CLUSTERED 
                (
                [RootUserID] ASC,
                [UserID] ASC
                )WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY],
                CONSTRAINT [IX_1_UserToUserRelations] UNIQUE NONCLUSTERED 
                (
                [ID] ASC
                )WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
                ) ON [PRIMARY]
                ",

                @"  CREATE TABLE [dbo].[VegaDbSettings](
                [ID][bigint] IDENTITY(1, 1) NOT NULL,
                [CreateDate] [datetime] NULL,
                [CreateDate_Timestamp] [bigint] NULL,
                [ModifyCounter] [bigint] NULL,
                [UpdateDate] [datetime] NULL,
                [UpdateDate_Timestamp] [bigint] NULL,
                [DBName] [varchar] (255) NULL,
                [IP] [varchar] (255) NULL,
                [SqlName] [varchar] (255) NULL,
                [SqlPassword] [varchar] (255) NULL,
                PRIMARY KEY CLUSTERED
                (
                [ID] ASC
                )WITH(PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON[PRIMARY]
                ) ON[PRIMARY]
                ",

                @"   CREATE TABLE [dbo].[AppVer](
                [Name] [nvarchar](255) NOT NULL,
                [Value] [nvarchar](max) NULL
                ) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
                ",

                @" CREATE TABLE [dbo].[Choice1](
                [Id] [int] IDENTITY(1,1) NOT NULL,
                [ProductId] [int] NOT NULL,
                [Name] [nvarchar](50) NOT NULL,
                [Price] [decimal](10, 2) NULL,
                [Aktarildi] [bit]  NULL,
                [SubeId] [int] NOT NULL,
                [SubeName] [varchar](255) NOT NULL,
                [Choice1PkId] [int] NOT NULL,
                [IsUpdate] [bit]  NULL,
                [YeniUrunMu] [bit]  NULL,
                [GuncellenecekSubeIdGrubu] [nvarchar](max) NULL, 
	            [GuncellenecekSubeAdiGrubu] [nvarchar](max) NULL,


                PRIMARY KEY CLUSTERED 
                (
                [Id] ASC
                )WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
                ) ON [PRIMARY] 
                ",

                @" CREATE TABLE [dbo].[Choice2](
                [Id] [int] IDENTITY(1,1) NOT NULL,
                [ProductId] [int] NOT NULL,
                [Choice1Id] [int] NOT NULL,
                [Name] [nvarchar](50) NOT NULL,
                [Price] [decimal](10, 2) NULL,
                [Aktarildi] [bit]  NULL,
                [SubeId] [int] NOT NULL,
                [SubeName] [varchar](255) NOT NULL,
                [Choice2PkId] [int] NOT NULL,
                [IsUpdate] [bit]  NULL,
                [YeniUrunMu] [bit]  NULL,
                [GuncellenecekSubeIdGrubu] [nvarchar](max) NULL, 
	            [GuncellenecekSubeAdiGrubu] [nvarchar](max) NULL,

                PRIMARY KEY CLUSTERED 
                (
                [Id] ASC
                )WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
                ) ON [PRIMARY]
                ",

                @"
                CREATE TABLE [dbo].[Options](
                [Id] [int] IDENTITY(1,1) NOT NULL,
                [Name] [nvarchar](50) NOT NULL,
                [Price] [decimal](10, 2) NULL,
                [Quantitative] [bit] NULL,
                [ProductId] [int] NOT NULL,
                [Category] [nvarchar](255) NOT NULL,
                [Aktarildi] [bit]  NULL,
                [SubeId] [int] NOT NULL,
                [SubeName] [varchar](255) NOT NULL,
                [OptionsPkId] [int] NOT NULL,
                [IsUpdate] [bit]  NULL,
                [YeniUrunMu] [bit]  NULL,
                [GuncellenecekSubeIdGrubu] [nvarchar](max) NULL, 
	            [GuncellenecekSubeAdiGrubu] [nvarchar](max) NULL,

                PRIMARY KEY CLUSTERED 
                (
                [Id] ASC
                )WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
                ) ON [PRIMARY]
                ",

                @" CREATE TABLE [dbo].[Product](
                [Id] [int] IDENTITY(1,1) NOT NULL,
                [ProductName] [nvarchar](450) NULL,
                [ProductGroup] [nvarchar](50) NULL,
                [ProductCode] [nvarchar](50) NULL,
                [Order] [nvarchar](50) NULL,
                [Price] [decimal](10, 2) NULL,
                [VatRate] [decimal](4, 2) NULL,
                [FreeItem] [bit] NULL,
                [InvoiceName] [nvarchar](50) NULL,
                [ProductType] [nvarchar](50) NULL,
                [Plu] [nvarchar](50) NULL,
                [SkipOptionSelection] [bit] NULL,
                [Favorites] [nvarchar](255) NULL,
                [Aktarildi] [bit]  NULL,
                [SubeId] [int] NOT NULL,
                [SubeName] [varchar](255) NOT NULL,
                [ProductPkId] [int] NOT NULL,
                [IsUpdate] [bit]  NULL,
                [YeniUrunMu] [bit]  NULL,
                [GuncellenecekSubeIdGrubu] [nvarchar](max) NULL, 
	            [GuncellenecekSubeAdiGrubu] [nvarchar](max) NULL,

                PRIMARY KEY CLUSTERED 
                (
                [Id] ASC
                )WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
                ) ON [PRIMARY]
                ",

                @" CREATE TABLE [dbo].[Choice1Tarihce](
                [Id] [int] IDENTITY(1,1) NOT NULL,
                [ProductId] [int] NOT NULL,
                [Name] [nvarchar](50) NOT NULL,
                [Price] [decimal](10, 2) NULL,
                [Aktarildi] [bit]  NULL,
                [SubeId] [int] NOT NULL,
                [SubeName] [varchar](255) NOT NULL,
                [Choice1PkId] [int] NOT NULL,
                [IsUpdate] [bit]  NULL,
                [IsUpdateDate] [nvarchar](50) NOT NULL,
                [IsUpdateKullanici] [nvarchar](50) NOT NULL,
                PRIMARY KEY CLUSTERED 
                (
                [Id] ASC
                )WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
                ) ON [PRIMARY] 
                ",

                @" CREATE TABLE [dbo].[Choice2Tarihce](
                [Id] [int] IDENTITY(1,1) NOT NULL,
                [ProductId] [int] NOT NULL,
                [Choice1Id] [int] NOT NULL,
                [Name] [nvarchar](50) NOT NULL,
                [Price] [decimal](10, 2) NULL,
                [Aktarildi] [bit]  NULL,
                [SubeId] [int] NOT NULL,
                [SubeName] [varchar](255) NOT NULL,
                [Choice2PkId] [int] NOT NULL,
                [IsUpdate] [bit]  NULL,
                [IsUpdateDate] [nvarchar](50) NOT NULL,
                [IsUpdateKullanici] [nvarchar](50) NOT NULL,
                PRIMARY KEY CLUSTERED 
                (
                [Id] ASC
                )WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
                ) ON [PRIMARY]
                ",
                @"
                CREATE TABLE [dbo].[OptionsTarihce](
                [Id] [int] IDENTITY(1,1) NOT NULL,
                [Name] [nvarchar](50) NOT NULL,
                [Price] [decimal](10, 2) NULL,
                [Quantitative] [bit] NULL,
                [ProductId] [int] NOT NULL,
                [Category] [nvarchar](255) NOT NULL,
                [Aktarildi] [bit]  NULL,
                [SubeId] [int] NOT NULL,
                [SubeName] [varchar](255) NOT NULL,
                [OptionsPkId] [int] NOT NULL,
                [IsUpdate] [bit]  NULL,
                [IsUpdateDate] [nvarchar](50) NOT NULL,
                [IsUpdateKullanici] [nvarchar](50) NOT NULL,
                PRIMARY KEY CLUSTERED 
                (
                [Id] ASC
                )WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
                ) ON [PRIMARY]
                ",

                @" CREATE TABLE [dbo].[ProductTarihce](
                [Id] [int] IDENTITY(1,1) NOT NULL,
                [ProductName] [nvarchar](450) NULL,
                [ProductGroup] [nvarchar](50) NULL,
                [ProductCode] [nvarchar](50) NULL,
                [Order] [nvarchar](50) NULL,
                [Price] [decimal](10, 2) NULL,
                [VatRate] [decimal](4, 2) NULL,
                [FreeItem] [bit] NULL,
                [InvoiceName] [nvarchar](50) NULL,
                [ProductType] [nvarchar](50) NULL,
                [Plu] [nvarchar](50) NULL,
                [SkipOptionSelection] [bit] NULL,
                [Favorites] [nvarchar](255) NULL,
                [Aktarildi] [bit]  NULL,
                [SubeId] [int] NOT NULL,
                [SubeName] [varchar](255) NOT NULL,
                [ProductPkId] [int] NOT NULL,
                [IsUpdate] [bit]  NULL,
                [IsUpdateDate] [nvarchar](50) NOT NULL,
                [IsUpdateKullanici] [nvarchar](50) NOT NULL,
                PRIMARY KEY CLUSTERED 
                (
                [Id] ASC
                )WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
                ) ON [PRIMARY]
                ",


                @"CREATE TABLE [dbo].[Belge](
                [Id][int] IDENTITY(1, 1) NOT NULL,
                [Cari] [nvarchar] (50) NULL,
                [BelgeNo] [nvarchar] (max) NULL,
                [BelgeKod] [nvarchar] (50) NULL,
                [Tarih] [datetime] NULL,
                [BelgeTutar] [decimal](15, 4) NULL,
                [AraToplam] [decimal](15, 4) NULL,
                [BelgeKDV] [decimal](15, 4) NULL,
                [NetToplam] [decimal](15, 4) NULL,
                [BelgeTip] [smallint] NULL,
                [IskontoTop] [decimal](15, 4) NULL,
                [AIsk1] [decimal](15, 4) NULL,
                [AIsk2] [decimal](15, 4) NULL,
                [AIsk3] [decimal](15, 4) NULL,
                [AIsk4] [decimal](15, 4) NULL,
                [Vade] [datetime] NULL,
                [Termin] [datetime] NULL,
                [Parabirimi] [nvarchar] (5) NULL,
                [Kur] [decimal](15, 4) NULL,
                [VgFatInd] [int] NULL,
                [Terminal] [int] NULL,
                [Aktarim] [int] NULL,
                [Kapali] [int] NULL,
                [BelgeNot] [nvarchar] (100) NULL,
                [PesinAd] [nvarchar] (50) NULL,
                [PesinVergiNo] [nvarchar] (50) NULL,
                [OzelKod] [nvarchar] (50) NULL,
                [Depo] [nvarchar] (50) NULL,
                [CariExtId] [int] NULL,
                [KayitTarihi] [datetime] NULL,
                [CikanDepo] [nvarchar] (50) NULL,
                [UID] [nvarchar] (50) NULL,
                [OzelKod9] [nvarchar] (50) NULL,
                [Personel] [nvarchar] (50) NULL,
                [Depozitolu] [bit] NULL,
                [OtvToplam] [decimal](15, 4) NULL,
                [OdemeTutar] [decimal](15, 4) NULL,
                [SonBakiye] [decimal](15, 4) NULL,
                [BelgeOzelKod1] [nvarchar] (50) NULL,
                [BelgeOzelKod2] [nvarchar] (50) NULL,
                [BelgeOzelKod3] [nvarchar] (50) NULL,
                [BelgeOzelKod4] [nvarchar] (50) NULL,
                [OnayDurumu] [int] NULL,
                [OnaylayanId] [int] NULL,
                [Sube] [nvarchar] (50) NULL,
                [Kasa] [nvarchar] (50) NULL,
                [SablonAdi] [nvarchar](50) NULL,
                [Sablon] [nvarchar](50) NULL,
                [BelgeNoArctos] [nvarchar](25) NULL,
                [Kdv] bit NULL,
                [AltBelgeNo] [nvarchar](50) NULL,
                [AltBelgeTarihi] [datetime] NULL,

                CONSTRAINT[PK_Belge] PRIMARY KEY CLUSTERED
                (
                [Id] ASC
                )WITH(PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON[PRIMARY]
                ) ON[PRIMARY] TEXTIMAGE_ON[PRIMARY]
                ",

                @"CREATE TABLE [dbo].[BelgeHareket](
                [Id] [int] IDENTITY(1,1) NOT NULL,
                [Belge] [int] NULL,
                [Stok] [nvarchar](20) NULL,
                [Barcode] [nvarchar](20) NULL,
                [Miktar] [decimal](15, 3) NULL,
                [Fiyat] [decimal](15, 4) NULL,
                [Tutar] [decimal](15, 4) NULL,
                [Isk1] [decimal](15, 4) NULL,
                [Isk2] [decimal](15, 4) NULL,
                [Isk3] [decimal](15, 4) NULL,
                [Isk4] [decimal](15, 4) NULL,
                [AltIskonto] [decimal](15, 4) NULL,
                [IskToplam] [decimal](15, 4) NULL,
                [SatirToplam] [decimal](15, 4) NULL,
                [VgFatHarInd] [int] NULL,
                [KDV] [decimal](15, 4) NULL,
                [KDVTutar] [decimal](15, 4) NULL,
                [StokExtId] [int] NULL,
                [SatirSayisi] [int] NULL,
                [Parabirimi] [nvarchar](5) NULL,
                [Kur] [decimal](15, 4) NULL,
                [Aciklama] [nvarchar](50) NULL,
                [RBMiktar] [nvarchar](4000) NULL,
                [MiktarBos] [decimal](15, 4) NULL,
                [Kampanya] [int] NULL,
                [Otv] [decimal](15, 4) NULL,
                [OtvTutar] [decimal](15, 4) NULL,
                [MalinCinsi] [nvarchar](max) NULL,
                [Birim] [nvarchar](50) NULL,
                [ZayiMiktar] [decimal](15, 4) NULL,
                [SayimTarihDahil] [bit]  NULL,

                CONSTRAINT [PK_BelgeHareket] PRIMARY KEY CLUSTERED 
                (
                [Id] ASC
                )WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
                ) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
          
                ",

                @"CREATE TABLE [dbo].[ProductTemplate](
                [Id] [int] IDENTITY(1,1) NOT NULL,
                [Name] [nvarchar](20) NOT NULL,
                [Aktarildi] [bit]  NULL,
                [SubeId] [int] NOT NULL,
                [SubeName] [varchar](255) NOT NULL,
                [ProductTemplatePkId] [int] NOT NULL,
                [IsUpdate] [bit]  NULL,
                CONSTRAINT [PK_ProductTemplate] PRIMARY KEY CLUSTERED 
                (
                [Id] ASC
                )WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
                ) ON [PRIMARY]
                ",


                @" CREATE TABLE [dbo].[ProductTemplatePrice](
                [Id] [int] IDENTITY(1,1) NOT NULL,
                [TemplateId] [int] NOT NULL,
                [TemplateName] [nvarchar](20)  NULL,
                [ProductId] [int] NOT NULL,
                [Choice1Id] [int] NOT NULL,
                [Choice2Id] [int] NOT NULL,
                [OptionsId] [int] NOT NULL,
                [Price] [decimal](10, 2) NULL,
                [Aktarildi] [bit]  NULL,
                [SubeId] [int] NOT NULL,
                [SubeName] [varchar](255) NOT NULL,
                [ProductTemplatePricePkId] [int] NOT NULL,
                [IsUpdate] [bit]  NULL,
              
                [GuncellenecekSubeIdGrubu] [nvarchar](max) NULL, 
	            [GuncellenecekSubeAdiGrubu] [nvarchar](max) NULL,
                [IsManuelInsert] [bit]  NULL,

                CONSTRAINT [PK_ProductTemplatePrices] PRIMARY KEY CLUSTERED 
                (
                [Id] ASC
                )WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
                ) ON [PRIMARY]
                ",

                @"
                CREATE TABLE [dbo].[SablonChoice1](
                [Id] [int] IDENTITY(1,1) NOT NULL,
                [ProductId] [int] NOT NULL,
                [Name] [nvarchar](50) NOT NULL,
                [Price] [decimal](10, 2) NULL,
                [Aktarildi] [bit]  NULL,
                [SubeId] [int] NOT NULL,
                [SubeName] [varchar](255) NOT NULL,
                [Choice1PkId] [int] NOT NULL,
                [IsUpdate] [bit]  NULL,
                [TemplateName] [nvarchar](20)  NULL,
                [GuncellenecekSubeIdGrubu] [nvarchar](max) NULL, 
	            [GuncellenecekSubeAdiGrubu] [nvarchar](max) NULL,
                PRIMARY KEY CLUSTERED 
                (
                [Id] ASC
                )WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
                ) ON [PRIMARY] 
                ",


                @"
                CREATE TABLE [dbo].[SablonChoice2](
                [Id] [int] IDENTITY(1,1) NOT NULL,
                [ProductId] [int] NOT NULL,
                [Choice1Id] [int] NOT NULL,
                [Name] [nvarchar](50) NOT NULL,
                [Price] [decimal](10, 2) NULL,
                [Aktarildi] [bit]  NULL,
                [SubeId] [int] NOT NULL,
                [SubeName] [varchar](255) NOT NULL,
                [Choice2PkId] [int] NOT NULL,
                [IsUpdate] [bit]  NULL,
                [TemplateName] [nvarchar](20)  NULL,
                [GuncellenecekSubeIdGrubu] [nvarchar](max) NULL, 
	            [GuncellenecekSubeAdiGrubu] [nvarchar](max) NULL,
                PRIMARY KEY CLUSTERED 
                (
                [Id] ASC
                )WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
                ) ON [PRIMARY]
                ",

                @"
                CREATE TABLE [dbo].[SablonOptions](
                [Id] [int] IDENTITY(1,1) NOT NULL,
                [Name] [nvarchar](50) NOT NULL,
                [Price] [decimal](10, 2) NULL,
                [Quantitative] [bit] NULL,
                [ProductId] [int] NOT NULL,
                [Category] [nvarchar](255) NOT NULL,
                [Aktarildi] [bit]  NULL,
                [SubeId] [int] NOT NULL,
                [SubeName] [varchar](255) NOT NULL,
                [OptionsPkId] [int] NOT NULL,
                [IsUpdate] [bit]  NULL,
                [TemplateName] [nvarchar](20)  NULL,
                [GuncellenecekSubeIdGrubu] [nvarchar](max) NULL, 
	            [GuncellenecekSubeAdiGrubu] [nvarchar](max) NULL,
                PRIMARY KEY CLUSTERED 
                (
                [Id] ASC
                )WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
                ) ON [PRIMARY]
                ",

                @"
                CREATE TABLE [dbo].[SablonProduct](
                [Id] [int] IDENTITY(1,1) NOT NULL,
                [ProductName] [nvarchar](450) NULL,
                [ProductGroup] [nvarchar](50) NULL,
                [ProductCode] [nvarchar](50) NULL,
                [Order] [nvarchar](50) NULL,
                [Price] [decimal](10, 2) NULL,
                [VatRate] [decimal](4, 2) NULL,
                [FreeItem] [bit] NULL,
                [InvoiceName] [nvarchar](50) NULL,
                [ProductType] [nvarchar](50) NULL,
                [Plu] [nvarchar](50) NULL,
                [SkipOptionSelection] [bit] NULL,
                [Favorites] [nvarchar](255) NULL,
                [Aktarildi] [bit]  NULL,
                [SubeId] [int] NOT NULL,
                [SubeName] [varchar](255) NOT NULL,
                [ProductPkId] [int] NOT NULL,
                [IsUpdate] [bit]  NULL,
                [TemplateName] [nvarchar](20)  NULL,
                [GuncellenecekSubeIdGrubu] [nvarchar](max) NULL, 
	            [GuncellenecekSubeAdiGrubu] [nvarchar](max) NULL,
                PRIMARY KEY CLUSTERED 
                (
                [Id] ASC
                )WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
                ) ON [PRIMARY]
                ",

                /*//Servis entegrasyonu için aktarım yönü ayarları tablosu */
                @"
                CREATE TABLE [dbo].[SelfOutParameters](
                [Id] [int] IDENTITY(1,1) NOT NULL,
                [TableName] [nvarchar](50) NULL,
                [IsSelfSync] [bit] NULL,
                CONSTRAINT [PK_SelfOutParameters] PRIMARY KEY CLUSTERED 
                (
                [Id] ASC
                )WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
                ) ON [PRIMARY]

                ALTER TABLE [dbo].[SelfOutParameters] ADD  CONSTRAINT [DF_SelfOutParameters_IsSelfSync]  DEFAULT ((1)) FOR [IsSelfSync]
           
                SET IDENTITY_INSERT [dbo].[SelfOutParameters] ON 

                INSERT [dbo].[SelfOutParameters] ([Id], [TableName], [IsSelfSync]) VALUES (1, N'AppVer', 0)

                INSERT [dbo].[SelfOutParameters] ([Id], [TableName], [IsSelfSync]) VALUES (2, N'Bill', 0)

                INSERT [dbo].[SelfOutParameters] ([Id], [TableName], [IsSelfSync]) VALUES (3, N'BillHeader', 0)

                INSERT [dbo].[SelfOutParameters] ([Id], [TableName], [IsSelfSync]) VALUES (4, N'BillPrinter', 0)

                INSERT [dbo].[SelfOutParameters] ([Id], [TableName], [IsSelfSync]) VALUES (5, N'Bom', 0)

                INSERT [dbo].[SelfOutParameters] ([Id], [TableName], [IsSelfSync]) VALUES (6, N'BomOptions', 0)

                INSERT [dbo].[SelfOutParameters] ([Id], [TableName], [IsSelfSync]) VALUES (7, N'CallLog', 0)

                INSERT [dbo].[SelfOutParameters] ([Id], [TableName], [IsSelfSync]) VALUES (8, N'CallLog2', 0)

                INSERT [dbo].[SelfOutParameters] ([Id], [TableName], [IsSelfSync]) VALUES (9, N'CampaignDetail', 0)

                INSERT [dbo].[SelfOutParameters] ([Id], [TableName], [IsSelfSync]) VALUES (10, N'CampaignHeader', 0)

                INSERT [dbo].[SelfOutParameters] ([Id], [TableName], [IsSelfSync]) VALUES (11, N'CashierState', 0)

                INSERT [dbo].[SelfOutParameters] ([Id], [TableName], [IsSelfSync]) VALUES (12, N'Choice1', 0)

                INSERT [dbo].[SelfOutParameters] ([Id], [TableName], [IsSelfSync]) VALUES (13, N'Choice2', 0)

                INSERT [dbo].[SelfOutParameters] ([Id], [TableName], [IsSelfSync]) VALUES (14, N'Collect', 0)

                INSERT [dbo].[SelfOutParameters] ([Id], [TableName], [IsSelfSync]) VALUES (15, N'CollectPaid', 0)

                INSERT [dbo].[SelfOutParameters] ([Id], [TableName], [IsSelfSync]) VALUES (16, N'Cost', 0)

                INSERT [dbo].[SelfOutParameters] ([Id], [TableName], [IsSelfSync]) VALUES (17, N'Customer', 0)

                INSERT [dbo].[SelfOutParameters] ([Id], [TableName], [IsSelfSync]) VALUES (18, N'DebitPayment', 0)

                INSERT [dbo].[SelfOutParameters] ([Id], [TableName], [IsSelfSync]) VALUES (19, N'DefaultOptions', 0)

                INSERT [dbo].[SelfOutParameters] ([Id], [TableName], [IsSelfSync]) VALUES (20, N'DeletedBill', 0)

                INSERT [dbo].[SelfOutParameters] ([Id], [TableName], [IsSelfSync]) VALUES (21, N'Deliverer', 0)

                INSERT [dbo].[SelfOutParameters] ([Id], [TableName], [IsSelfSync]) VALUES (22, N'DigiPan', 0)

                INSERT [dbo].[SelfOutParameters] ([Id], [TableName], [IsSelfSync]) VALUES (23, N'DirectTransaction', 0)

                INSERT [dbo].[SelfOutParameters] ([Id], [TableName], [IsSelfSync]) VALUES (24, N'DiscountDetail', 0)

                INSERT [dbo].[SelfOutParameters] ([Id], [TableName], [IsSelfSync]) VALUES (25, N'FormalBillPrinter', 0)

                INSERT [dbo].[SelfOutParameters] ([Id], [TableName], [IsSelfSync]) VALUES (26, N'MaterialTransaction', 0)

                INSERT [dbo].[SelfOutParameters] ([Id], [TableName], [IsSelfSync]) VALUES (27, N'OptionCats', 0)

                INSERT [dbo].[SelfOutParameters] ([Id], [TableName], [IsSelfSync]) VALUES (28, N'Options', 0)

                INSERT [dbo].[SelfOutParameters] ([Id], [TableName], [IsSelfSync]) VALUES (29, N'OrderHeader', 0)

                INSERT [dbo].[SelfOutParameters] ([Id], [TableName], [IsSelfSync]) VALUES (30, N'OrderPrinter', 0)

                INSERT [dbo].[SelfOutParameters] ([Id], [TableName], [IsSelfSync]) VALUES (31, N'Payment', 0)

                INSERT [dbo].[SelfOutParameters] ([Id], [TableName], [IsSelfSync]) VALUES (32, N'PaymentDetails', 0)

                INSERT [dbo].[SelfOutParameters] ([Id], [TableName], [IsSelfSync]) VALUES (33, N'PaymentMethods', 0)

                INSERT [dbo].[SelfOutParameters] ([Id], [TableName], [IsSelfSync]) VALUES (34, N'Permission', 0)

                INSERT [dbo].[SelfOutParameters] ([Id], [TableName], [IsSelfSync]) VALUES (35, N'PhoneDictionary', 0)

                INSERT [dbo].[SelfOutParameters] ([Id], [TableName], [IsSelfSync]) VALUES (36, N'PhoneOrderHeader', 0)

                INSERT [dbo].[SelfOutParameters] ([Id], [TableName], [IsSelfSync]) VALUES (37, N'PrePayment', 0)

                INSERT [dbo].[SelfOutParameters] ([Id], [TableName], [IsSelfSync]) VALUES (38, N'PrintJob', 0)

                INSERT [dbo].[SelfOutParameters] ([Id], [TableName], [IsSelfSync]) VALUES (39, N'Product', 0)

                INSERT [dbo].[SelfOutParameters] ([Id], [TableName], [IsSelfSync]) VALUES (40, N'ProductImage', 0)

                INSERT [dbo].[SelfOutParameters] ([Id], [TableName], [IsSelfSync]) VALUES (41, N'Production', 0)

                INSERT [dbo].[SelfOutParameters] ([Id], [TableName], [IsSelfSync]) VALUES (42, N'ProductTemplate', 0)

                INSERT [dbo].[SelfOutParameters] ([Id], [TableName], [IsSelfSync]) VALUES (43, N'ProductTemplateAutomation', 0)

                INSERT [dbo].[SelfOutParameters] ([Id], [TableName], [IsSelfSync]) VALUES (44, N'ProductTemplatePrice', 0)

                INSERT [dbo].[SelfOutParameters] ([Id], [TableName], [IsSelfSync]) VALUES (45, N'ReasonList', 0)

                INSERT [dbo].[SelfOutParameters] ([Id], [TableName], [IsSelfSync]) VALUES (46, N'ReservationList', 0)

                INSERT [dbo].[SelfOutParameters] ([Id], [TableName], [IsSelfSync]) VALUES (47, N'SayimBaslik', 0)

                INSERT [dbo].[SelfOutParameters] ([Id], [TableName], [IsSelfSync]) VALUES (48, N'SayimDetay', 0)

                INSERT [dbo].[SelfOutParameters] ([Id], [TableName], [IsSelfSync]) VALUES (49, N'Shift', 0)

                INSERT [dbo].[SelfOutParameters] ([Id], [TableName], [IsSelfSync]) VALUES (50, N'StockLevel', 0)

                INSERT [dbo].[SelfOutParameters] ([Id], [TableName], [IsSelfSync]) VALUES (51, N'SubeLinks', 0)

                INSERT [dbo].[SelfOutParameters] ([Id], [TableName], [IsSelfSync]) VALUES (52, N'TableGroups', 0)

                INSERT [dbo].[SelfOutParameters] ([Id], [TableName], [IsSelfSync]) VALUES (53, N'TableLock', 0)

                INSERT [dbo].[SelfOutParameters] ([Id], [TableName], [IsSelfSync]) VALUES (54, N'TemplateOverrides', 0)

                INSERT [dbo].[SelfOutParameters] ([Id], [TableName], [IsSelfSync]) VALUES (55, N'TeraPosDefs', 0)

                INSERT [dbo].[SelfOutParameters] ([Id], [TableName], [IsSelfSync]) VALUES (56, N'Test', 0)

                INSERT [dbo].[SelfOutParameters] ([Id], [TableName], [IsSelfSync]) VALUES (57, N'User', 0)

                INSERT [dbo].[SelfOutParameters] ([Id], [TableName], [IsSelfSync]) VALUES (58, N'Value', 0)

                INSERT [dbo].[SelfOutParameters] ([Id], [TableName], [IsSelfSync]) VALUES (59, N'Waste', 0)

                INSERT [dbo].[SelfOutParameters] ([Id], [TableName], [IsSelfSync]) VALUES (60, N'WeighingProducts', 0)

                INSERT [dbo].[SelfOutParameters] ([Id], [TableName], [IsSelfSync]) VALUES (61, N'ProductEqu', 0)

                INSERT [dbo].[SelfOutParameters] ([Id], [TableName], [IsSelfSync]) VALUES (62, N'OptionsEqu', 0)

                INSERT [dbo].[SelfOutParameters] ([Id], [TableName], [IsSelfSync]) VALUES (63, N'TBLPBIPUANYETKI', 0)

                INSERT [dbo].[SelfOutParameters] ([Id], [TableName], [IsSelfSync]) VALUES (64, N'TBLPOSPUAN', 0)

                INSERT [dbo].[SelfOutParameters] ([Id], [TableName], [IsSelfSync]) VALUES (65, N'TBLPOSPUANACIKLAMA', 0)

                SET IDENTITY_INSERT [dbo].[SelfOutParameters] OFF
                ",

                @"declare @tempProduct int =( select COUNT(*) from sys.tables where name='TempProduct')
                                if (@tempProduct = 0)
                                    create table TempProduct(Id int, ProductName nvarchar(500))",

                @"declare @tempProductSube int =( select COUNT(*) from sys.tables where name='TempProductSube')
                                if (@tempProductSube = 0)
                                    create table TempProductSube(Id int, ProductName nvarchar(500),SubeId int,ProductNameEk  nvarchar(500))",

                    @"declare @tempBom int =( select COUNT(*) from sys.tables where name='TempBom')
                                if (@tempBom = 0)
                                    create table TempBom([Id] [int] IDENTITY(1,1) NOT NULL,
	                                                    [ProductName] [nvarchar](450) NULL,
	                                                    [MaterialName] [nvarchar](450) NULL,
	                                                    [Quantity] [decimal](10, 4) NOT NULL,
	                                                    [Unit] [nvarchar](20) NOT NULL,
	                                                    [StokID] [int] NOT NULL,
	                                                    [ProductId] [int] NULL,
	                                                    [SubeId] [int] NULL,
	                                                    [YeniUrunMu] [bit] NULL)",

                @"declare @tempBomOptions int =( select COUNT(*) from sys.tables where name='TempBomOptions')
                                if (@tempBomOptions = 0)
                                    create table TempBomOptions([Id] [int] IDENTITY(1,1) NOT NULL,
	                                                            [OptionsId] [int] NULL,
	                                                            [OptionsName] [nvarchar](450) NULL,
	                                                            [MaterialName] [nvarchar](450) NULL,
	                                                            [Quantity] [decimal](10, 4) NOT NULL,
	                                                            [Unit] [nvarchar](20) NULL,
	                                                            [StokID] [int] NULL,
	                                                            [ProductName] [nvarchar](450) NULL,
	                                                            [SubeId] [int] NULL,
	                                                            [YeniUrunMu] [bit] NULL,
	                                                            [ProductId] [int] NULL)",
                @"declare @tempStok int =( select COUNT(*) from sys.tables where name='TempStok')
                                if (@tempStok = 0)
                                    create table TempStok(Id int, MalinCinsi nvarchar(500),MalinCinsi2 nvarchar(500),BIRIMADI nvarchar(500),STOKKODU nvarchar(500)  )",


                /* //Yekiler*/
                @"CREATE TABLE [dbo].[YetkiIslemTip](
                [Id][int] IDENTITY(1, 1) NOT NULL,
                [IslemTipi] [int] NOT NULL,
                [IslemTipiAdi] [nvarchar] (250) NOT NULL, 
                [CreatedDate] [datetime2] (7) NULL,
                [UpdatedDate] [datetime2] (7) NULL,
                [IsActive] [bit] NOT NULL,
                CONSTRAINT[PK_YetkiIslemTip] PRIMARY KEY CLUSTERED
                (
                [Id] ASC
                )
                WITH(PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON[PRIMARY]) ON[PRIMARY]              
                ",

                @"CREATE TABLE [dbo].[YetkiNesne](
                [Id] [int] IDENTITY(1,1) NOT NULL,
                [YetkiAdi] [nvarchar](250) NOT NULL,
                [Aciklama] [nvarchar](250) NULL,
                [CreatedDate] [datetime2](7) NULL,
                [UpdatedDate] [datetime2](7) NULL,
                [IsActive] [bit] NOT NULL,
                CONSTRAINT [PK_YetkiNesne] PRIMARY KEY CLUSTERED 
                (
                [Id] ASC
                )
                WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]) ON [PRIMARY]
                ",

                @"CREATE TABLE [dbo].[YetkiMenu](
                [Id] [int] IDENTITY(1,1) NOT NULL,
                [UstMenuId] [int] NULL,
                [Adi] [nvarchar](255) NOT NULL,
                [YetkiDegeri] [nvarchar](255) NULL,
                [SiraNo] [int] NULL,
                [CreatedDate] [datetime2](7) NULL,
                [UpdatedDate] [datetime2](7) NULL,
                [IsActive] [bit] NOT NULL,
                CONSTRAINT [PK_YetkiMenu] PRIMARY KEY CLUSTERED 
                (
                [Id] ASC
                )WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]) ON [PRIMARY]           
                ",

                @"CREATE TABLE [dbo].[YetkiNesneDetay](
                [Id] [int] IDENTITY(1,1) NOT NULL,
                [YetkiNesneId] [int] NULL,
                [MenuId] [int] NULL,
                [IslemTipiId] [int] NULL,
                CONSTRAINT [PK_YetkiNesneDetay] PRIMARY KEY CLUSTERED 
                (
                [Id] ASC
                )
                WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY] ) ON [PRIMARY] 
                ",

                @"CREATE TABLE [dbo].[YetkiUser](
                [Id] [int] IDENTITY(1,1) NOT NULL,
                [MenuId] [int] NOT NULL,
                [IslemTipiId] [int] NULL,
                [UserId] [int] NOT NULL,
                [YetkiNesneId] [int] NOT NULL,
                [IsActive] [bit] NOT NULL,
                CONSTRAINT [PK_YetkiUser] PRIMARY KEY CLUSTERED 
                (
                [Id] ASC
                )
                WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY] ) ON [PRIMARY] 
                ",

                /*  //Yekiler
                //Ürün Ekle options tablosunun katogorileri tablosu*/
                @"CREATE TABLE[dbo].[OptionCats] (
                [Id][int] IDENTITY(1, 1) NOT NULL,
                [Name] [nvarchar] (254) NOT NULL,
                [ProductId] [int] NOT NULL,
                [MaxSelections] [int] NOT NULL,
                [MinSelections] [int] NOT NULL,
                [Aktarildi] [bit] NOT NULL,
                [SubeId] [int] NOT NULL,
                [SubeName] [varchar] (255) NOT NULL,
                [OptionCatsPkId] [int] NOT NULL,
                [YeniUrunMu] [bit] NULL,
                CONSTRAINT[PK_OptionCats] PRIMARY KEY CLUSTERED
                (
                [Id] ASC
                )WITH(PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON[PRIMARY]
                ) ON[PRIMARY]
                
                ",

                @"CREATE TABLE [dbo].[Bom](
	            [Id] [int] IDENTITY(1,1) NOT NULL,
	            [ProductName] [nvarchar](450) NULL,
	            [MaterialName] [nvarchar](450) NULL,
                [MalinCinsi] [nvarchar](450) NULL,
	            [Quantity] [decimal](10, 4) NOT NULL,
	            [Unit] [nvarchar](20) NOT NULL,
	            [StokID] [int] NOT NULL,
	            [Aktarildi] [bit]  NULL,
	            [IsUpdated] [bit] NULL,
	            [ProductId] [int] NULL,
                [SubeId] [int] NULL,
                [YeniUrunMu] [bit]  NULL,
                 CONSTRAINT [PK_Bom] PRIMARY KEY CLUSTERED 
                (
	                [Id] ASC
                )WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
                ) ON [PRIMARY]
                ",

                @"CREATE TABLE [dbo].[BomOptions](
	                [Id] [int] IDENTITY(1,1) NOT NULL,
	                [OptionsId] [int] NULL,
	                [OptionsName] [nvarchar](450) NULL,
	                [MaterialName] [nvarchar](450) NULL,
	                [Quantity] [decimal](10, 4) NOT NULL,
	                [Unit] [nvarchar](20) NULL,
	                [StokID] [int] NULL,
	                [ProductName] [nvarchar](450) NULL,
	                [Aktarildi] [bit]  NULL,
                    [SubeId] [int] NULL,
                    [YeniUrunMu] [bit]  NULL,
                    [ProductId] [int] NULL,
                 CONSTRAINT [PK_BomOptions] PRIMARY KEY CLUSTERED 
                (
	                [Id] ASC
                )WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
                ) ON [PRIMARY]
                ",

                //İcmal
                @"CREATE TABLE [dbo].[IcmalPayment](
                [Id] [int] IDENTITY(1,1) NOT NULL,
                [SubeId] [int] NOT NULL,
                [SubeName] [nvarchar] (150) NOT NULL, 
                [PaymentTypeId] [int] NULL,
                [PaymentTypeName] [nvarchar] (100) NULL,
                [PaymentDate] [datetime] NULL,
                [CashAmount] [decimal](10, 2) NULL,
                [KreditAmount] [decimal](10, 2) NULL,
                [TicketAmount] [decimal](10, 2) NULL,
                [OnlineAmount] [decimal](10, 2) NULL,
                [DebitAmount] [decimal](10, 2) NULL,
                [UserId] [int] NOT NULL,
                [ConfirmationStatus] [tinyint] NULL,
                [CreatedDate] [datetime] NULL,
                [UpdateDate] [datetime] NULL,
                CONSTRAINT[PK_IcmalPayment] PRIMARY KEY CLUSTERED
                (
                [Id] ASC
                )WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
                ) ON [PRIMARY]",

                //İcmalKredit
                @"CREATE TABLE [dbo].[IcmalKreditPayment](
                [Id] [int] IDENTITY(1,1) NOT NULL,
                [IcmalPaymentId] [int] NULL,
                [BankaName] [nvarchar] (50) NULL,
                [BankBkmId] [int] NULL,
                [Amount] [decimal](10, 2) NULL,
                [PaymentDate] [datetime] NULL,
                [UserId] [int] NULL,
                [CreatedDate] [datetime] NULL,
                [UpdateDate] [datetime] NULL,
                CONSTRAINT[PK_IcmalKreditPayment] PRIMARY KEY CLUSTERED
                (
                [Id] ASC
                )WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
                ) ON [PRIMARY]",

                //İcmalTicket
                @"CREATE TABLE [dbo].[IcmalTicketPayment](
                [Id] [int] IDENTITY(1,1) NOT NULL,
                [IcmalPaymentId] [int] NULL,
                [TicketName] [nvarchar](50) NULL,
                [TicketId] [int] NULL,
                [Amount] [decimal](10, 2) NULL,
                [PaymentDate] [datetime] NULL,
                [UserId] [int] NULL,
                [CreatedDate] [datetime] NULL,
                [UpdateDate] [datetime] NULL,
                CONSTRAINT [PK_IcmalTicketPayment] PRIMARY KEY CLUSTERED 
                (
                [Id] ASC
                )WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
                ) ON [PRIMARY]"

    };


                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    con.Open();

                    foreach (var item in createTableList)
                    {
                        try
                        {
                            SqlCommand cmd = new SqlCommand(item, con);
                            cmd.ExecuteNonQuery();
                        }
                        catch (Exception ex)
                        {

                        }
                    }

                    con.Close();
                }
            }
            catch (Exception e)
            {
                System.Windows.Forms.MessageBox.Show("exception occured while creating table:" + e.Message + "\t" + e.GetType());
            }
        }
    }
}