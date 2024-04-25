using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Web.Configuration;
using System.Configuration;
using System.Data.SqlClient;
using SefimDesktop;

namespace FormWebConfig
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            var selectedIndex = cmBoxUygulamaSec.SelectedIndex;
            //var selectedIndexServisAktifMi = comboBoxServisAktifMi.SelectedIndex;
            //TODO Uygulama Tipi
            //[Display(Name = "Vaga Master")]
            //VegaMaster = 1,
            //[Display(Name = "Şefim Panel")]
            //SefimPanel = 2,
            //[Display(Name = "Vega Master-Şefim Panel")]
            //VegaMasterSefimPanel = 3,

            if (selectedIndex == 0)
            {
                MessageBox.Show("Lütfen uygulama seçiniz.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }

            if (!string.IsNullOrEmpty(VeriTabani.Text) && !string.IsNullOrEmpty(User.Text) && !string.IsNullOrEmpty(Password.Text) && !string.IsNullOrEmpty(Sunucu.Text))
            {
                try
                {
                    string masterConString = "Data Source=" + Sunucu.Text + ";Initial Catalog=master ;Persist Security Info=True;User ID=" + User.Text + ";Password=" + Password.Text + "";
                    var resultDbChack = DataBaseVarMi(masterConString, VeriTabani.Text);

                    if (string.IsNullOrEmpty(resultDbChack))
                        return;

                    string _ConnectionString = "Data Source=" + Sunucu.Text + ";Initial Catalog=" + VeriTabani.Text + ";Persist Security Info=True;User ID=" + User.Text + ";Password=" + Password.Text + "";
                    Configuration webConfigApp;
                    webConfigApp = WebConfigurationManager.OpenWebConfiguration("/", "VegaMaster", null, Environment.MachineName);
                    webConfigApp.AppSettings.Settings["User"].Value = User.Text;
                    webConfigApp.AppSettings.Settings["Password"].Value = Password.Text;
                    webConfigApp.AppSettings.Settings["DBName"].Value = VeriTabani.Text;
                    webConfigApp.AppSettings.Settings["SefimDetayDBName"].Value = SefimVeritabaniTxt.Text;
                    webConfigApp.AppSettings.Settings["Server"].Value = Sunucu.Text;
                    webConfigApp.AppSettings.Settings["UygulamaTipi"].Value = selectedIndex.ToString();
                    //webConfigApp.AppSettings.Settings["ServisAktifMi"].Value = selectedIndexServisAktifMi.ToString();
                    if (string.IsNullOrWhiteSpace(txtServisAktifMi.Text))
                    {
                        webConfigApp.AppSettings.Settings["MongoConnString"].Value = string.Empty;
                    }
                    else
                    {
                        webConfigApp.AppSettings.Settings["MongoConnString"].Value = txtServisAktifMi.Text;
                    }
                    webConfigApp.Save();

                    TabloKontrol(_ConnectionString);
                    MessageBox.Show("Kayıt Başarı ile Yapıldı", "Başarılı", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                }
                catch (Exception _Ex)
                {
                    MessageBox.Show(_Ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                MessageBox.Show("Alanlar Boş Geçilemez. Lütfen Tüm Alanları Doldurunuz", "Uyari", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            Configuration webConfigApp;
            webConfigApp = WebConfigurationManager.OpenWebConfiguration("/", "VegaMaster", null, Environment.MachineName);
            User.Text = webConfigApp.AppSettings.Settings["User"].Value;
            Password.Text = webConfigApp.AppSettings.Settings["Password"].Value;
            VeriTabani.Text = webConfigApp.AppSettings.Settings["DBName"].Value;
            SefimVeritabaniTxt.Text = webConfigApp.AppSettings.Settings["SefimDetayDBName"].Value;
            Sunucu.Text = webConfigApp.AppSettings.Settings["Server"].Value;
            var uygulamatipi = webConfigApp.AppSettings.Settings["UygulamaTipi"].Value;
            var servisUrl = webConfigApp.AppSettings.Settings["MongoConnString"].Value;
            var servisAktifMi = webConfigApp.AppSettings.Settings["ServisAktifMi"].Value;
            txtServisAktifMi.Text = servisUrl.ToString();

            //[Display(Name = "Vaga Master")]
            //VegaMaster = 1,
            //[Display(Name = "Şefim Panel")]
            //SefimPanel = 2,
            //[Display(Name = "Vega Master-Şefim Panel")]
            //VegaMasterSefimPanel = 3,
            cmBoxUygulamaSec.Items.Insert(0, "Seçiniz");
            cmBoxUygulamaSec.Items.Insert(1, "Vaga Master");
            cmBoxUygulamaSec.Items.Insert(2, "Şefim Panel");
            cmBoxUygulamaSec.Items.Insert(3, "Vega Master-Şefim Panel");

            if (!string.IsNullOrEmpty(uygulamatipi))
            {
                if (uygulamatipi == "0")
                {
                    cmBoxUygulamaSec.SelectedIndex = cmBoxUygulamaSec.FindStringExact("Seçiniz");
                }
                else if (uygulamatipi == "1")
                {
                    cmBoxUygulamaSec.SelectedIndex = cmBoxUygulamaSec.FindStringExact("Vaga Master");
                }
                else if (uygulamatipi == "2")
                {
                    cmBoxUygulamaSec.SelectedIndex = cmBoxUygulamaSec.FindStringExact("Şefim Panel");
                }
                else if (uygulamatipi == "3")
                {
                    cmBoxUygulamaSec.SelectedIndex = cmBoxUygulamaSec.FindStringExact("Vega Master-Şefim Panel");
                }
            }


            //comboBoxServisAktifMi.Items.Insert(0, "Seçiniz");
            //comboBoxServisAktifMi.Items.Insert(1, "Aktif");
            //comboBoxServisAktifMi.Items.Insert(2, "Pasif");
            //if (servisAktifMi == null || string.IsNullOrWhiteSpace(servisAktifMi))
            //{
            //    comboBoxServisAktifMi.SelectedIndex = comboBoxServisAktifMi.FindStringExact("Pasif");
            //}
            //else if (servisAktifMi != null && !string.IsNullOrWhiteSpace(servisAktifMi))
            //{
            //    comboBoxServisAktifMi.SelectedIndex = comboBoxServisAktifMi.FindStringExact("AKtif");
            //}

            //
        }

        private void button3_Click(object sender, EventArgs e)
        {
            SqlConnectionControl(Sunucu.Text, VeriTabani.Text, User.Text, Password.Text, SefimVeritabaniTxt.Text);
        }

        private void DBCreate()
        {
            LisCreate();
            string connString = "Data Source=" + Sunucu.Text + ";Initial Catalog=master;User ID = " + User.Text + "; Password = " + Password.Text + "";
            SqlConnection __SqlConnection;
            try
            {
                __SqlConnection = new SqlConnection(connString);
                string str = "CREATE DATABASE " + VeriTabani.Text;
                SqlCommand cmd = new SqlCommand(str, __SqlConnection);
                __SqlConnection.Open();
                cmd.ExecuteNonQuery();
                __SqlConnection.Close();
                MessageBox.Show("Database Oluşturuldu! Onaylayabilirsiniz. ", "Başarılı", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);

            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Console.WriteLine("exception occured while creating table:" + e.Message + "\t" + e.GetType());
            }
        }

        private void LisCreate()
        {
            string connString_ = "Data Source=" + Sunucu.Text + ";Initial Catalog=master;User ID = " + User.Text + "; Password = " + Password.Text + "";
            try
            {
                SqlConnection __SqlConnection_ = new SqlConnection(connString_);
                __SqlConnection_.Open();
                string createTableSql = @"if not exists (select * from sysobjects where name='TBLURUN' and xtype='U')
                        CREATE TABLE [dbo].[TBLURUN](
                         [IND] [int] IDENTITY(1,1) NOT NULL,
                         [FRM] [bit] NULL,
                         [MUSTERIKOD] [nvarchar](50) NULL,
                         [URUNID] [int] NULL,
                         [MYKEY] [nvarchar](max) NULL,
                         CONSTRAINT [PK__TBLURUN__C490CCE2FA4C1393] PRIMARY KEY CLUSTERED 
                        (
                         [IND] ASC
                        )WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
                        ) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

                        ALTER TABLE [dbo].[TBLURUN] ADD  CONSTRAINT [DF__TBLURUN__FRM__1D9B5BB6]  DEFAULT ((0)) FOR [FRM]";

                var commandCreate = new SqlCommand(createTableSql, __SqlConnection_);
                commandCreate.ExecuteNonQuery();
            }
            catch (Exception)
            { }

            try
            {
                using (SqlConnection connection = new SqlConnection(connString_))
                {
                    connection.Open();
                    using (SqlCommand command = new SqlCommand(" ALTER TABLE [TBLURUN] ALTER COLUMN [MYKEY] NVARCHAR(MAX) "))
                    {
                        command.Connection = connection;
                        command.ExecuteNonQuery();
                    }
                    connection.Close();
                }
            }
            catch (Exception)
            { }

        }

        public void SqlConnectionControl(String _ServerName, String _DatabaseName, String _UserName, String _Password, string _SefimDBName)
        {
            string connectionString = String.Empty;
            SqlConnection sqlConnection;
            List<string> dbNames = new List<string>
            {
                _DatabaseName,
                _SefimDBName
            };

            foreach (var item in dbNames)
            {
                try
                {
                    connectionString = "Data Source=" + _ServerName + ";Initial Catalog=" + item + ";Persist Security Info=True;User ID=" + _UserName + ";Password=" + _Password + "";
                    sqlConnection = new SqlConnection(connectionString);
                    sqlConnection.Open();
                    MessageBox.Show(item + " Veri Tabanı için bağlantı başarılı ! ", "Başarılı", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                    sqlConnection.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(item + " Veritabanı İçin Bağlantı Sağlanamadı!" + ex.Message, "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void DBKontrol(string _ServerName, string databaseName, string _UserName, string _Password)
        {
            SqlConnection __SqlConnection;
            string connString = "Data Source=" + _ServerName + ";Initial Catalog=master;User ID = " + _UserName + "; Password = " + _Password + "";
            string sqlCreateDBQuery;
            bool result = false;

            #region TBLURUN tablosunda MYKEY alanını MAX yapar.

            //ALTER TABLE [TBLURUN] ALTER COLUMN[MYKEY] NVARCHAR(MAX)
            try
            {
                using (SqlConnection connection = new SqlConnection(connString))
                {
                    connection.Open();
                    using (SqlCommand command = new SqlCommand("ALTER TABLE [TBLURUN] ALTER COLUMN[MYKEY] NVARCHAR(MAX"))
                    {
                        command.Connection = connection;
                        command.ExecuteNonQuery();
                    }
                    connection.Close();
                }
            }
            catch (Exception)
            { }

            #endregion TBLURUN tablosunda MYKEY alanını MAX yapar.
            try
            {
                __SqlConnection = new SqlConnection(connString);
                sqlCreateDBQuery = string.Format("SELECT database_id FROM sys.databases WHERE Name = '{0}'", databaseName);

                using (__SqlConnection)
                {
                    using (SqlCommand sqlCmd = new SqlCommand(sqlCreateDBQuery, __SqlConnection))
                    {
                        __SqlConnection.Open();

                        object resultObj = sqlCmd.ExecuteScalar();

                        int databaseID = 0;

                        if (resultObj != null)
                        {
                            int.TryParse(resultObj.ToString(), out databaseID);
                        }

                        __SqlConnection.Close();

                        result = (databaseID > 0);
                    }
                    //if (!result)
                    //{
                    //    DBCreate(__SqlConnection, databaseName);
                    //}
                }
            }
            catch (Exception ex)
            {
                result = false;
            }
        }

        public static string DataBaseVarMi(string connectionString, string dbName)
        {
            string queryString = " SELECT name FROM master.sys.databases WHERE name = N'" + dbName + "'";
            string resultMesages = string.Empty;

            using (SqlConnection con = new SqlConnection(connectionString))
            {
                con.Open();

                try
                {
                    SqlCommand cmd = new SqlCommand(queryString, con);
                    //resultMesages = cmd.ExecuteNonQuery();
                    var cmdResult = cmd.ExecuteScalar();

                    if (cmdResult == null)
                    {
                        MessageBox.Show(dbName + " adlı veri tabanı bulunamadı. Öncelikle veri tabanını oluşturmanız gerekmektedir.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                    else
                    {
                        resultMesages = cmdResult.ToString();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                }
                con.Close();
            }

            return resultMesages;
        }

        private void TabloKontrol(string connectionString)
        {
            try
            {
                //int result = 0;
                //string sql = "select count(*) from INFORMATION_SCHEMA.TABLES where table_name = 'SubeSettings'";
                //using (SqlConnection conn = new SqlConnection(connectionString))
                //{
                //    SqlCommand cmd = new SqlCommand(sql, conn);
                //    conn.Open();
                //    result = (Int32)cmd.ExecuteScalar();
                //    conn.Close();
                //}
                //if (result == 0)
                //{
                TableCRUD.CreateTables(connectionString);
                //}
                //if (result != 0)
                //{
                try
                {
                    //using (SqlConnection connection = new SqlConnection(connectionString))
                    //{
                    //    connection.Open();
                    //    using (SqlCommand command = new SqlCommand("ALTER TABLE SubeSettings ADD FasterSubeID varchar(255) ALTER TABLE SubeSettings ADD sdasd varchar(255)"))
                    //    {
                    //        command.Connection = connection;
                    //        _ = command.Transaction;
                    //        command.ExecuteNonQuery();
                    //    }
                    //    connection.Close();
                    //}

                    //Yeni eklenen alan eski datalar delete yapılmıyor ihtimaline karşılık eklendi
                    //Tabloya yeni eklenen column'u oluşturmak için kullanılıyor.
                    string[] addColumunList =
                    {
                        "ALTER TABLE Users ADD SubeSirasiGorunsunMu  [bit] NULL",
                        "ALTER TABLE YetkiMenu ADD UNIQUE (Adi)",
                        "ALTER TABLE YetkiIslemTip ADD UNIQUE (IslemTipiAdi)",

                        "ALTER TABLE[dbo].[OptionCats] ADD CONSTRAINT[OptionCats_Aktarildi]  DEFAULT((0)) FOR[Aktarildi]",

                        "ALTER TABLE Product ADD [YeniUrunMu] [bit]  NULL",
                        "ALTER TABLE [dbo].[Product] ADD  CONSTRAINT [DF_Product_YeniUrunMu]  DEFAULT ((0)) FOR [YeniUrunMu]",

                        "ALTER TABLE Options ADD [YeniUrunMu] [bit]  NULL",
                        "ALTER TABLE [dbo].[Options] ADD  CONSTRAINT [DF_Options_YeniUrunMu]  DEFAULT ((0)) FOR [YeniUrunMu]",

                        "ALTER TABLE Choice1 ADD [YeniUrunMu] [bit]  NULL",
                        "ALTER TABLE [dbo].[Choice1] ADD  CONSTRAINT [DF_Choice1_YeniUrunMu]  DEFAULT ((0)) FOR [YeniUrunMu]",

                        "ALTER TABLE Choice2 ADD [YeniUrunMu] [bit]  NULL",
                        "ALTER TABLE [dbo].[Choice2] ADD  CONSTRAINT [DF_Choice2_YeniUrunMu]  DEFAULT ((0)) FOR [YeniUrunMu]",

                        "ALTER TABLE OptionCats ADD [YeniUrunMu] [bit]  NULL",
                        "ALTER TABLE BomOptions ADD [ProductId] [int]  NULL",
                        "ALTER TABLE [YetkiUser] ADD [YetkiNesneId] [int]  NULL",

                        "ALTER TABLE [SubeSettings] ADD FasterSubeID varchar(255)",
                        "ALTER TABLE [SubeSettings] ADD [UrunEslestirmeVarMi][bit] NULL",
                        "ALTER TABLE [SubeSettings] ADD [BelgeSayimTarihDahil] [bit]  NULL",
                        "ALTER TABLE [SubeSettings] ADD [SefimPanelZimmetCagrisi] [nvarchar](50)  NULL",
                        "ALTER TABLE [SubeSettings] ADD [ServiceAdress] [nvarchar](255)  NULL",
                        "ALTER TABLE [SubeSettings] ADD [PersonelYemekRaporuAdi] [nvarchar](500)",
                        "ALTER TABLE [SubeSettings] ADD [VPosSubeKodu] [varchar](255) NULL",
                        "ALTER TABLE [SubeSettings] ADD [VPosKasaKodu] [varchar](255) NULL",


                        "ALTER TABLE Product ADD  [GuncellenecekSubeIdGrubu] [nvarchar](max) NULL   ALTER TABLE Product ADD [GuncellenecekSubeAdiGrubu] [nvarchar](max) NULL",
                        "ALTER TABLE Options ADD  [GuncellenecekSubeIdGrubu] [nvarchar](max) NULL   ALTER TABLE Options ADD [GuncellenecekSubeAdiGrubu] [nvarchar](max) NULL",
                        "ALTER TABLE Choice1 ADD  [GuncellenecekSubeIdGrubu] [nvarchar](max) NULL   ALTER TABLE Choice1 ADD [GuncellenecekSubeAdiGrubu] [nvarchar](max) NULL",
                        "ALTER TABLE Choice2 ADD  [GuncellenecekSubeIdGrubu] [nvarchar](max) NULL   ALTER TABLE Choice2 ADD [GuncellenecekSubeAdiGrubu] [nvarchar](max) NULL",

                        "ALTER TABLE SablonProduct ADD  [TemplateName] [nvarchar](20)  NULL",
                        "ALTER TABLE SablonChoice1 ADD  [TemplateName] [nvarchar](20)  NULL",
                        "ALTER TABLE SablonChoice2 ADD  [TemplateName] [nvarchar](20)  NULL",
                        "ALTER TABLE SablonOptions ADD  [TemplateName] [nvarchar](20)  NULL",

                        "ALTER TABLE SablonProduct ADD  [GuncellenecekSubeIdGrubu] [nvarchar](max) NULL   ALTER TABLE SablonProduct ADD [GuncellenecekSubeAdiGrubu] [nvarchar](max) NULL",
                        "ALTER TABLE SablonChoice1 ADD  [GuncellenecekSubeIdGrubu] [nvarchar](max) NULL   ALTER TABLE SablonChoice1 ADD [GuncellenecekSubeAdiGrubu] [nvarchar](max) NULL",
                        "ALTER TABLE SablonChoice2 ADD  [GuncellenecekSubeIdGrubu] [nvarchar](max) NULL   ALTER TABLE SablonChoice2 ADD [GuncellenecekSubeAdiGrubu] [nvarchar](max) NULL",
                        "ALTER TABLE SablonOptions ADD  [GuncellenecekSubeIdGrubu] [nvarchar](max) NULL   ALTER TABLE SablonOptions ADD [GuncellenecekSubeAdiGrubu] [nvarchar](max) NULL",

                        "ALTER TABLE ProductTemplatePrice ADD  [GuncellenecekSubeIdGrubu] [nvarchar](max) NULL   ALTER TABLE ProductTemplatePrice ADD [GuncellenecekSubeAdiGrubu] [nvarchar](max) NULL",
                        "ALTER TABLE ProductTemplatePrice ADD [IsManuelInsert] [bit]  NULL",
                        "ALTER TABLE ProductTemplatePrice ADD  [TemplateName] [nvarchar](20)  NULL",

                        "ALTER TABLE Users ADD [BelgeTipYetkisi]  [nvarchar](255) NULL",
                        "ALTER TABLE Users ADD [YetkiNesneId] [int]  NULL",
                        "ALTER TABLE Users ADD [YetkiNesneAdi] [nvarchar](250)  NULL",

                        "ALTER TABLE Belge ADD  [Kdv] [bit] NULL",
                        "ALTER TABLE Belge ADD [AltCari] [nvarchar](50)  NULL",
                        "ALTER TABLE Belge ADD [AltBelgeNo] [nvarchar](50)  NULL",
                        "ALTER TABLE Belge ADD [AltBelgeTarihi] [datetime]  NULL",

                        "ALTER TABLE TempProductSube ADD [ProductNameEk] [nvarchar](500)",

                        "ALTER TABLE Product ALTER COLUMN GuncellenecekSubeIdGrubu NVARCHAR(max)",
                        "ALTER TABLE Product ALTER COLUMN GuncellenecekSubeAdiGrubu NVARCHAR(max)",

                        "ALTER TABLE Choice1 ADD  [SubeId] [int] NOT NULL",

                        "ALTER TABLE TempStok ADD [STOKKODU] [nvarchar](500)  NULL",

                        "ALTER TABLE Bom ADD [MalinCinsi]  [nvarchar](450)  NULL",

                        "ALTER TABLE SubeSettings ADD [VPosSubeKodu] [nvarchar](255)  NULL",
                        "ALTER TABLE SubeSettings ADD [VPosKasaKodu]  [nvarchar](255)  NULL",

                        "ALTER TABLE SubeSettings ADD [EnvanterOzelKodTipi] [tinyint]  NULL",
                        "ALTER TABLE SubeSettings ADD [EnvanterOzelKodAdi]  [nvarchar](255)  NULL",


                };

                    string[] insertDataList =
                    {
                        "INSERT INTO [dbo].[Users] ([UserName], [Password], [IsAdmin], [SubeID], [SubeSirasiGorunsunMu], [UygulmaTipi]) VALUES ('Admin','123',1,0,0,3)",
                        "INSERT INTO [dbo].[UserTimeLine] ([CreateDate], [CreateDate_Timestamp], [StartTime], [EndTime]) VALUES ('','','05:00','07:00')",

                        "INSERT INTO [dbo].[YetkiIslemTip] ( [IslemTipi], [IslemTipiAdi], [CreatedDate], [UpdatedDate], [IsActive] ) VALUES('1', 'Görüntüleme', getdate(), '', 1)",
                        "INSERT INTO [dbo].[YetkiIslemTip] ( [IslemTipi], [IslemTipiAdi], [CreatedDate], [UpdatedDate], [IsActive]) VALUES('2', 'Düzenleme', getdate(), '', 1)",
                        "INSERT INTO [dbo].[YetkiIslemTip] ( [IslemTipi], [IslemTipiAdi], [CreatedDate], [UpdatedDate], [IsActive]) VALUES('3', 'Ekleme', getdate(), '', 1)",
                        "INSERT INTO [dbo].[YetkiIslemTip] ( [IslemTipi], [IslemTipiAdi], [CreatedDate], [UpdatedDate], [IsActive]) VALUES('4', 'Silme', getdate(), '', 1)",
                        "INSERT INTO [dbo].[YetkiIslemTip] ( [IslemTipi], [IslemTipiAdi], [CreatedDate], [UpdatedDate], [IsActive]) VALUES('5', 'Onaylama', getdate(), '', 1)",

                        "INSERT [dbo].[YetkiMenu] ( [UstMenuId], [Adi], [YetkiDegeri], [SiraNo], [CreatedDate], [UpdatedDate], [IsActive]) VALUES(NULL, N'Veri Gönderimi', NULL, NULL, getdate(), NULL, 1)",
                        "INSERT [dbo].[YetkiMenu] ( [UstMenuId], [Adi], [YetkiDegeri], [SiraNo], [CreatedDate], [UpdatedDate], [IsActive]) VALUES(1, N'Fiyat Gönder', N'SefimPanelVeriGonderimi/GetSubeListInsertSubePruduct', NULL, getdate(), NULL, 1)",
                        "INSERT [dbo].[YetkiMenu] ( [UstMenuId], [Adi], [YetkiDegeri], [SiraNo], [CreatedDate], [UpdatedDate], [IsActive]) VALUES(1, N'Fiyat Şablon Güncelle', N'SefimPanelSablonGuncelle/SablonList', NULL, getdate(), NULL, 1)",
                        "INSERT [dbo].[YetkiMenu] ( [UstMenuId], [Adi], [YetkiDegeri], [SiraNo], [CreatedDate], [UpdatedDate], [IsActive]) VALUES(1, N'Ürün Ekle / Pasif', N'SefimPanelUrunEkle/ProductList', NULL, getdate(), NULL, 1)",
                        "INSERT [dbo].[YetkiMenu] ( [UstMenuId], [Adi], [YetkiDegeri], [SiraNo], [CreatedDate], [UpdatedDate], [IsActive]) VALUES(1, N'Fiyat Güncelleme Tarihçe', N'SefimPanelTarihce/SubeList', NULL, getdate(), NULL, 1)",
                        "INSERT [dbo].[YetkiMenu] ( [UstMenuId], [Adi], [YetkiDegeri], [SiraNo], [CreatedDate], [UpdatedDate], [IsActive]) VALUES(NULL, N'Belge İşlemleri', NULL, NULL, getdate(), NULL, 1)",
                        "INSERT [dbo].[YetkiMenu] ( [UstMenuId], [Adi], [YetkiDegeri], [SiraNo], [CreatedDate], [UpdatedDate], [IsActive]) VALUES(5, N'Alış İrsaliyesi Oluştur', N'/SefimPanelBelgeIslemleri/AlisBelgesiCreate?BelgeTip=26', NULL, getdate(), NULL, 1)",
                        "INSERT [dbo].[YetkiMenu] ( [UstMenuId], [Adi], [YetkiDegeri], [SiraNo], [CreatedDate], [UpdatedDate], [IsActive]) VALUES(5, N'Alış Faturası Oluştur', N'/SefimPanelBelgeIslemleri/AlisBelgesiCreate?BelgeTip=20', NULL, getdate(), NULL, 1)",
                        "INSERT [dbo].[YetkiMenu] ( [UstMenuId], [Adi], [YetkiDegeri], [SiraNo], [CreatedDate], [UpdatedDate], [IsActive]) VALUES(5, N'İade İrsaliyesi Oluştur', N'/SefimPanelBelgeIslemleri/AlisBelgesiCreate?BelgeTip=29', NULL, getdate(), NULL, 1)",
                        "INSERT [dbo].[YetkiMenu] ( [UstMenuId], [Adi], [YetkiDegeri], [SiraNo], [CreatedDate], [UpdatedDate], [IsActive]) VALUES(5, N'İade Faturası Oluştur', N'/SefimPanelBelgeIslemleri/AlisBelgesiCreate?BelgeTip=23', NULL, getdate(), NULL, 1)",
                        "INSERT [dbo].[YetkiMenu] ( [UstMenuId], [Adi], [YetkiDegeri], [SiraNo], [CreatedDate], [UpdatedDate], [IsActive]) VALUES(5, N'Gider Faturası Oluştur', N'/SefimPanelBelgeIslemleri/AlisBelgesiCreate?BelgeTip=165', NULL, getdate(), NULL, 1)",
                        "INSERT [dbo].[YetkiMenu] ( [UstMenuId], [Adi], [YetkiDegeri], [SiraNo], [CreatedDate], [UpdatedDate], [IsActive]) VALUES(5, N'Talep Belgesi Oluştur', N'/SefimPanelBelgeIslemleri/AlisBelgesiCreate?BelgeTip=135', NULL, getdate(), NULL, 1)",
                        "INSERT [dbo].[YetkiMenu] ( [UstMenuId], [Adi], [YetkiDegeri], [SiraNo], [CreatedDate], [UpdatedDate], [IsActive]) VALUES(5, N'Depo Transfer Oluştur', N'/SefimPanelBelgeIslemleri/AlisBelgesiCreate?BelgeTip=128', NULL, getdate(), NULL, 1)",
                        "INSERT [dbo].[YetkiMenu] ( [UstMenuId], [Adi], [YetkiDegeri], [SiraNo], [CreatedDate], [UpdatedDate], [IsActive]) VALUES(5, N'Sayım Belgesi Oluştur', N'/SefimPanelBelgeIslemleri/AlisBelgesiCreate?BelgeTip=93', NULL, getdate(), NULL, 1)",
                        "INSERT [dbo].[YetkiMenu] ( [UstMenuId], [Adi], [YetkiDegeri], [SiraNo], [CreatedDate], [UpdatedDate], [IsActive]) VALUES(5, N'Belgeler', N'SefimPanelBelgeIslemleri/AlisBelgesiList', NULL, getdate(), NULL, 1)",
                        "INSERT [dbo].[YetkiMenu] ( [UstMenuId], [Adi], [YetkiDegeri], [SiraNo], [CreatedDate], [UpdatedDate], [IsActive]) VALUES(NULL, N'Bekleyen Onaylar', N'SefimPanelBelgeIslemleri/OnayBekleyenBelgeList', NULL, getdate(), NULL, 1)",
                        "INSERT [dbo].[YetkiMenu] ( [UstMenuId], [Adi], [YetkiDegeri], [SiraNo], [CreatedDate], [UpdatedDate], [IsActive]) VALUES(NULL, N'Kabul işlemleri', N'SefimPanelKabulIslemleri/KabulBekleyenIslerList', NULL, getdate(), NULL, 1)",
                        "INSERT [dbo].[YetkiMenu] ( [UstMenuId], [Adi], [YetkiDegeri], [SiraNo], [CreatedDate], [UpdatedDate], [IsActive]) VALUES(NULL, N'Ayarlar', NULL, NULL, getdate(), NULL, 1)",
                        "INSERT [dbo].[YetkiMenu] ( [UstMenuId], [Adi], [YetkiDegeri], [SiraNo], [CreatedDate], [UpdatedDate], [IsActive]) VALUES(16, N'Şube Tanımları', N'SubeSettings/List', NULL, getdate(), NULL, 1)",
                        "INSERT [dbo].[YetkiMenu] ( [UstMenuId], [Adi], [YetkiDegeri], [SiraNo], [CreatedDate], [UpdatedDate], [IsActive]) VALUES(16, N'VegaDb Ayarları', N'VegaDBSettings/List', NULL, getdate(), NULL, 1)",
                        "INSERT [dbo].[YetkiMenu] ( [UstMenuId], [Adi], [YetkiDegeri], [SiraNo], [CreatedDate], [UpdatedDate], [IsActive]) VALUES(16, N'Servis Ayarları', N'SelfOutParametersSettings/List', NULL, getdate(), NULL, 1)",
                        "INSERT [dbo].[YetkiMenu] ( [UstMenuId], [Adi], [YetkiDegeri], [SiraNo], [CreatedDate], [UpdatedDate], [IsActive]) VALUES(16, N'Sayım Şablonları', N'SefimPanelBelgeIslemleri/AlisBelgesiSablonList', NULL, getdate(), NULL, 1)",
                        "INSERT [dbo].[YetkiMenu] ( [UstMenuId], [Adi], [YetkiDegeri], [SiraNo], [CreatedDate], [UpdatedDate], [IsActive]) VALUES(16, N'Yetki Tanımlama', N'YetkiNesne/List', NULL, getdate(), NULL, 1)",
                        "INSERT [dbo].[YetkiMenu] ( [UstMenuId], [Adi], [YetkiDegeri], [SiraNo], [CreatedDate], [UpdatedDate], [IsActive]) VALUES(16, N'Kullanıcı Tanımlama', N'UsersList/List', NULL, getdate(), NULL, 1)",
                        "INSERT [dbo].[YetkiMenu] ( [UstMenuId], [Adi], [YetkiDegeri], [SiraNo], [CreatedDate], [UpdatedDate], [IsActive]) VALUES(NULL, N'Recete Maliyet Raporu', N'RecipeCost/List', NULL, getdate(), NULL, 1)",
                        "INSERT [dbo].[YetkiMenu] ( [UstMenuId], [Adi], [YetkiDegeri], [SiraNo], [CreatedDate], [UpdatedDate], [IsActive]) VALUES(5, N'Zayi Giriş Belgesi Oluştur', N'/SefimPanelBelgeIslemleri/AlisBelgesiCreate?BelgeTip=94', NULL, getdate(), NULL, 1)",
                        "INSERT [dbo].[YetkiMenu] ( [UstMenuId], [Adi], [YetkiDegeri], [SiraNo], [CreatedDate], [UpdatedDate], [IsActive]) VALUES(5, N'Zayi Giriş Belgesi Oluştur', N'/SefimPanelBelgeIslemleri/AlisBelgesiCreate?BelgeTip=94', NULL, getdate(), NULL, 1)",
                        "INSERT [dbo].[YetkiMenu] ( [UstMenuId], [Adi], [YetkiDegeri], [SiraNo], [CreatedDate], [UpdatedDate], [IsActive]) VALUES(2, N'İcmal Giriş', N'Icmal/Create', NULL, getdate(), NULL, 1)",
                        "INSERT [dbo].[YetkiMenu] ( [UstMenuId], [Adi], [YetkiDegeri], [SiraNo], [CreatedDate], [UpdatedDate], [IsActive]) VALUES(2, N'İcmal Listesi', N'Icmal/List', NULL, getdate(), NULL, 1)",

                        "update [dbo].[YetkiMenu] set YetkiDegeri='/SefimPanelBelgeIslemleri/AlisBelgesiCreate?BelgeTip=29' where Adi='İade İrsaliyesi Oluştur'",

                        "INSERT [dbo].[SelfOutParameters] ([Id], [TableName], [IsSelfSync]) VALUES(63, N'TBLPBIPUANYETKI', 0)",
                        "INSERT[dbo].[SelfOutParameters]([Id], [TableName], [IsSelfSync]) VALUES(64, N'TBLPOSPUAN', 0)",
                        "INSERT[dbo].[SelfOutParameters]([Id], [TableName], [IsSelfSync]) VALUES(65, N'TBLPOSPUANACIKLAMA', 0)"
                    };

                    using (SqlConnection con = new SqlConnection(connectionString))
                    {
                        con.Open();
                        SqlCommand cmd = new SqlCommand();

                        foreach (var item in addColumunList)
                        {
                            try
                            {
                                cmd = new SqlCommand(item, con);
                                cmd.ExecuteNonQuery();
                            }
                            catch (Exception)
                            { }
                        }

                        foreach (var item in insertDataList)
                        {
                            try
                            {
                                cmd = new SqlCommand(item, con);
                                cmd.ExecuteNonQuery();
                            }
                            catch (Exception)
                            { }
                        }
                        con.Close();
                    }

                }
                catch (Exception ex) { }
                //}
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            DBCreate();
        }
    }
}