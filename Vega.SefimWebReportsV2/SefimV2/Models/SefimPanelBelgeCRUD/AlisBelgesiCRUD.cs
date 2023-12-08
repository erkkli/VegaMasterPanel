using SefimV2.Helper;
using SefimV2.ViewModels.SPosKabulIslemleri;
using SefimV2.ViewModels.User;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web.Configuration;
using System.Web.Mvc;
using Vega.Belge.Wrapper;
using static SefimV2.Enums.General;

namespace SefimV2.Models.SefimPanelBelgeCRUD
{
    public class AlisBelgesiCRUD
    {
        ModelFunctions modelFunctions = new ModelFunctions();
        #region Config local copy db connction setting       
        static string subeIp = WebConfigurationManager.AppSettings["Server"];
        static string dbName = WebConfigurationManager.AppSettings["DBName"];
        static string sqlKullaniciName = WebConfigurationManager.AppSettings["User"];
        static string sqlKullaniciPassword = WebConfigurationManager.AppSettings["Password"];
        #endregion

        public ActionResultMessages Insert(BelgeAlisGiderCreate obj, UserViewModel usModel)
        {
            ActionResultMessages result = new ActionResultMessages();
            try
            {
                var sqlData = new SqlData(new SqlConnection(modelFunctions.NewConnectionString(subeIp, dbName, sqlKullaniciName, sqlKullaniciPassword)));
                string TimeStamp = DateTime.Now.ToFileTimeUtc().ToString();
                string CmdString = "INSERT INTO [dbo].[Belge]([Cari],[BelgeNo],[BelgeKod],[Tarih],[BelgeTutar],[AraToplam],[BelgeKDV],[NetToplam],[BelgeTip],[IskontoTop],[AIsk1],[AIsk2],[AIsk3],[AIsk4],[Vade],[Termin],[Parabirimi],[Kur],[VgFatInd],[Terminal],[Aktarim],[Kapali],[BelgeNot],[PesinAd],[PesinVergiNo],[OzelKod],[Depo],[CariExtId],[KayitTarihi],[CikanDepo],[UID],[OzelKod9],[Personel],[Depozitolu],[OtvToplam],[OdemeTutar],[SonBakiye],[BelgeOzelKod1],[BelgeOzelKod2],[BelgeOzelKod3],[BelgeOzelKod4],[Sube],[Kasa],[OnayDurumu],[SablonAdi],[Sablon],[Kdv],[AltCari],[AltBelgeNo],[AltBelgeTarihi])" +
     "VALUES" +
            "(@par1, @par2, @par3, @par4, @par5, @par6, @par7, @par8, @par9, @par10, @par11, @par12, @par13, @par14, @par15, @par16, @par17, @par18, @par19, @par20, @par21, @par22, @par23, @par24, @par25, @par26, @par27, @par28, @par29, @par30, @par31, @par32, @par33, @par34, @par35, @par36, @par37, @par38, @par39, @par40, @par41,@par42,@par43, 0,@par44,@par45,@par46,@par47   ,@par48,@par49)" +
            "select CAST(scope_identity() AS int);";


                if (obj.BelgeTip == BelgeTipi.SayimGirisBelgesi)
                {
                    obj.CikanDepo = obj.Depo;
                }


                int ID = sqlData.ExecuteScalarSql(CmdString, new object[] {
                    obj.Cari,
                   obj.BelgeNo,
                   obj.BelgeKod,
                   obj.Tarih,
                   obj.BelgeTutar,
                   obj.AraToplam,
                   obj.BelgeKDV,
                   obj.NetToplam,
                  obj.BelgeTip,
                   obj.IskontoTop,
                   obj.AIsk1,
                   obj.AIsk2,
                   obj.AIsk3,
                   obj.AIsk4,
                   obj.Vade,
                   obj.Termin,
                   obj.Parabirimi,
                   obj.Kur,
                   obj.VgFatInd,
                   obj.Terminal,
                   obj.Aktarim,
                   obj.Kapali,
                   obj.BelgeNot,
                   obj.PesinAd ,
                   obj.PesinVergiNo ,
                   obj.OzelKod ,
                   obj.Depo ,
                   obj.CariExtId,
                   DateTime.Now.Date,
                   obj.CikanDepo,
                    obj.UID,
                   obj.OzelKod9,
                   obj.Personel,
                  obj.Depozitolu,
                  obj.OtvToplam,
                  obj.OdemeTutar,
                  obj.SonBakiye,
                        obj.BelgeOzelKod1,
                         obj.BelgeOzelKod2,
                          obj.BelgeOzelKod3,
                           obj.BelgeOzelKod4,
                           obj.Sube,
                           obj.Kasa,
                           obj.SablonAdi,
                           obj.Sablon,
                           obj.Kdv,
                           obj.AltCari,
                           obj.AltBelgeNo,
                           obj.AltBelgeTarihi
                    });

                obj.Id = ID;
                string CmdStringBelgeHareket = "INSERT INTO [dbo].[BelgeHareket]([Belge],[Stok],[Barcode],[Miktar],[Fiyat],[Tutar],[Isk1],[Isk2],[Isk3],[Isk4],[AltIskonto],[IskToplam],[SatirToplam],[VgFatHarInd],[KDV],[KDVTutar],[StokExtId],[SatirSayisi],[Parabirimi],[Kur],[Aciklama],[RBMiktar],[MiktarBos],[Kampanya],[Otv],[OtvTutar],[MalinCinsi],[Birim],[SayimTarihDahil],[ZayiMiktar])" +
     "VALUES" +
     "(@par1, @par2, @par3, @par4, @par5, @par6, @par7, @par8, @par9, @par10, @par11, @par12, @par13, @par14, @par15, @par16, @par17, @par18, @par19, @par20, @par21, @par22, @par23, @par24, @par25, @par26, @par27,@par28,@par29,@par30)";

                //sqlData.executeSqlWithParameters(CmdString, parameters);

                if (obj.BelgeHarekets != null)
                {
                    decimal BelgeTutar = 0;
                    foreach (var belgehareket in obj.BelgeHarekets)
                    {

                        if (obj.BelgeTip == BelgeTipi.DepoTransfer || obj.BelgeTip == BelgeTipi.DepoTransferKabul || obj.BelgeTip == BelgeTipi.TalepBelgesi)
                        {
                            List<SefimPanelStok> lst = GetStokDetayList(usModel.FirmaID, Convert.ToInt32(belgehareket.Stok), 4);
                            if (lst.Count > 0)
                            {

                                belgehareket.Fiyat = lst[0].SatisFiyati;

                            }
                        }


                        BelgeTutar += belgehareket.Fiyat;
                        sqlData.ExecuteSql(CmdStringBelgeHareket, new object[] {
                        ID,
                        belgehareket.Stok,
                        belgehareket.Barcode,
                        belgehareket.Miktar,
                        belgehareket.Fiyat,
                        belgehareket.Tutar,
                        belgehareket.Isk1,
                        belgehareket.Isk2,
                        belgehareket.Isk3,
                        belgehareket.Isk4,
                        belgehareket.AltIskonto,
                        belgehareket.IskToplam,
                        belgehareket.SatirToplam,
                        belgehareket.VgFatHarInd,
                        belgehareket.KDV,
                        belgehareket.KDVTutar,
                        belgehareket.StokExtId,
                        belgehareket.SatirSayisi,
                        belgehareket.Parabirimi,
                        belgehareket.Kur,
                        belgehareket.Aciklama,
                        belgehareket.RBMiktar,
                        belgehareket.MiktarBos,
                        belgehareket.Kampanya,
                        belgehareket.Otv,
                        belgehareket.OtvTutar,
                        belgehareket.MalinCinsi,
                        belgehareket.Birim,
                        belgehareket.SayimTarihDahil,
                        belgehareket.ZayiMiktar
                    });
                    }
                    string cmdUpdate = "Update Belge set BelgeTutar=" + BelgeTutar.ToString().Replace(",", ".") + " where Id=" + ID;
                }
            }
            catch (Exception ex)
            {
                result.IsSuccess = false;
                result.UserMessage = ex.ToString();
                return result;
            }
            result.result_STR = "Başarılı";
            result.result_INT = 1;
            result.result_BOOL = true;
            result.result_OBJECT = obj;
            result.IsSuccess = true;
            result.UserMessage = "İşlem Başarılı";
            return result;
        }
        public ActionResultMessages Delete(int Id)
        {
            ActionResultMessages result = new ActionResultMessages();
            try
            {
                var sqlData = new SqlData(new SqlConnection(modelFunctions.NewConnectionString(subeIp, dbName, sqlKullaniciName, sqlKullaniciPassword)));
                string TimeStamp = DateTime.Now.ToFileTimeUtc().ToString();
                string CmdString = "delete from Belge where Id=" + Id;



                sqlData.ExecuteSql(CmdString, new object[] {
                    Id
                    });
            }
            catch (Exception ex)
            {
                result.IsSuccess = false;
                result.UserMessage = ex.ToString();
                return result;
            }
            result.result_STR = "Başarılı";
            result.result_INT = 1;
            result.result_BOOL = true;
            //result.result_OBJECT = SaatListe();
            result.IsSuccess = true;
            result.UserMessage = "İşlem Başarılı";
            return result;
        }
        public ActionResultMessages Update(BelgeAlisGiderCreate obj)
        {
            ActionResultMessages result = new ActionResultMessages();
            try
            {
                var sqlData = new SqlData(new SqlConnection(modelFunctions.NewConnectionString(subeIp, dbName, sqlKullaniciName, sqlKullaniciPassword)));
                string TimeStamp = DateTime.Now.ToFileTimeUtc().ToString();
                string CmdString = "Update [dbo].[Belge] set [Cari] = @par1 ,[BelgeNo] = @par2 ,[BelgeKod] = @par3 ,[Tarih] = @par4 ,[BelgeTutar] = @par5 ,[AraToplam] = @par6 ,[BelgeKDV] = @par7 ,[NetToplam] = @par8 ,[BelgeTip] = @par9 ,[IskontoTop] = @par10 ,[AIsk1] = @par11 ,[AIsk2] = @par12 ,[AIsk3] = @par13 ,[AIsk4] = @par14 ,[Vade] = @par15 ,[Termin] = @par16 ,[Parabirimi] = @par17 ,[Kur] = @par18 ,[VgFatInd] = @par19 ,[Terminal] = @par20 ,[Aktarim] = @par21 ,[Kapali] = @par22 ,[BelgeNot] = @par23 ,[PesinAd] = @par24 ,[PesinVergiNo] = @par25 ,[OzelKod] = @par26 ,[Depo] = @par27 ,[CariExtId] = @par28 ,[KayitTarihi] = @par29 ,[CikanDepo] = @par30 ,[UID] = @par31 ,[OzelKod9] = @par32 ,[Personel] = @par33 ,[Depozitolu] = @par34 ,[OtvToplam] = @par35 ,[OdemeTutar] = @par36 ,[SonBakiye] =@par37,[BelgeOzelKod1] = @par38 ,[BelgeOzelKod2] = @par39 ,[BelgeOzelKod3] = @par40 ,[BelgeOzelKod4] =@par41, [Sube]=@par42, [Kasa]=@par43, [SablonAdi]=@par44,[Sablon]=@par45,[BelgeNoArctos]=@par46,Kdv=@par47,AltCari=@par48 where OnayDurumu=0 and Id=" + obj.Id;


                if (obj.BelgeTip == BelgeTipi.SayimGirisBelgesi)
                {
                    obj.CikanDepo = obj.Depo;
                }

                int res = sqlData.ExecuteSql(CmdString, new object[] {
                    obj.Cari,
                   obj.BelgeNo,
                   obj.BelgeKod,
                   obj.Tarih,
                   obj.BelgeTutar,
                   obj.AraToplam,
                   obj.BelgeKDV,
                   obj.NetToplam,
                  obj.BelgeTip,
                   obj.IskontoTop,
                   obj.AIsk1,
                   obj.AIsk2,
                   obj.AIsk3,
                   obj.AIsk4,
                   obj.Vade,
                   obj.Termin,
                   obj.Parabirimi,
                   obj.Kur,
                   obj.VgFatInd,
                   obj.Terminal,
                   obj.Aktarim,
                   obj.Kapali,
                   obj.BelgeNot,
                   obj.PesinAd ,
                   obj.PesinVergiNo ,
                   obj.OzelKod ,
                   obj.Depo ,
                   obj.CariExtId,
                   DateTime.Now.Date,
                   obj.CikanDepo,
                    obj.UID,
                   obj.OzelKod9,
                   obj.Personel,
                  obj.Depozitolu,
                  obj.OtvToplam,
                  obj.OdemeTutar,
                  obj.SonBakiye,
                  obj.BelgeOzelKod1,
                         obj.BelgeOzelKod2,
                          obj.BelgeOzelKod3,
                           obj.BelgeOzelKod4,
                           obj.Sube,
                           obj.Kasa,
                            obj.SablonAdi,
                            obj.Sablon,
                            obj.BelgeNoArctos,
                            obj.Kdv,
                            obj.AltCari
                    });





                if (res > 0)
                {

                    string CmdStringBelgeHareket = "INSERT INTO [dbo].[BelgeHareket]([Belge],[Stok],[Barcode],[Miktar],[Fiyat],[Tutar],[Isk1],[Isk2],[Isk3],[Isk4],[AltIskonto],[IskToplam],[SatirToplam],[VgFatHarInd],[KDV],[KDVTutar],[StokExtId],[SatirSayisi],[Parabirimi],[Kur],[Aciklama],[RBMiktar],[MiktarBos],[Kampanya],[Otv],[OtvTutar],[MalinCinsi],[Birim],[SayimTarihDahil],[ZayiMiktar])" +
         "VALUES" +
         "(@par1, @par2, @par3, @par4, @par5, @par6, @par7, @par8, @par9, @par10, @par11, @par12, @par13, @par14, @par15, @par16, @par17, @par18, @par19, @par20, @par21, @par22, @par23, @par24, @par25, @par26,@par27,@par28,@par29,@par30)";

                    //sqlData.executeSqlWithParameters(CmdString, parameters);

                    if (obj.BelgeHarekets != null)
                    {
                        #region **************silinen kayıtlar varsa silinmesi için yazıldı.
                        string ids = "";
                        foreach (var belgehareket in obj.BelgeHarekets)
                        {
                            if (belgehareket.Id > 0)
                            {
                                ids += belgehareket.Id + ",";
                            }
                        }

                        if (ids != "")
                        {
                            string CmdStringBelgeHareketRemove = "delete from [dbo].[BelgeHareket] where Belge = " + obj.Id + " and Id not in(" + ids.Substring(0, ids.Length - 1) + ")";
                            sqlData.ExecuteSql(CmdStringBelgeHareketRemove);
                        }
                        else
                        {
                            string CmdStringBelgeHareketRemove = "delete from [dbo].[BelgeHareket] where Belge = " + obj.Id;
                            sqlData.ExecuteSql(CmdStringBelgeHareketRemove);
                        }
                        #endregion **************silinen kayıtlar varsa silinmesi için yazıldı.

                        foreach (var belgehareket in obj.BelgeHarekets)
                        {
                            if (belgehareket.Id > 0)
                            {

                                //varsa update yoksa insert hareket için.
                                string CmdStringBelgeHareketUpdate = "Update [dbo].[BelgeHareket] set [Belge] = @par1 ,[Stok] = @par2 ,[Barcode] = @par3 ,[Miktar] = @par4 ,[Fiyat] = @par5 ,[Tutar] = @par6 ,[Isk1] = @par7 ,[Isk2] = @par8 ,[Isk3] = @par9 ,[Isk4] = @par10 ,[AltIskonto] = @par11 ,[IskToplam] = @par12 ,[SatirToplam] = @par13 ,[VgFatHarInd] = @par14 ,[KDV] = @par15 ,[KDVTutar] = @par16 ,[StokExtId] = @par17 ,[SatirSayisi] = @par18 ,[Parabirimi] = @par19 ,[Kur] = @par20 ,[Aciklama] = @par21 ,[RBMiktar] = @par22 ,[MiktarBos] = @par23 ,[Kampanya] = @par24 ,[Otv] = @par25 ,[OtvTutar] = @par26,[MalinCinsi]=@par27,[Birim]=@par28,[SayimTarihDahil]=@par29,[ZayiMiktar]=@par30 where Id=" + belgehareket.Id;
                                sqlData.ExecuteSql(CmdStringBelgeHareketUpdate, new object[] {
                        obj.Id,
                        belgehareket.Stok,
                        belgehareket.Barcode,
                        belgehareket.Miktar,
                        belgehareket.Fiyat,
                        belgehareket.Tutar,
                        belgehareket.Isk1,
                        belgehareket.Isk2,
                        belgehareket.Isk3,
                        belgehareket.Isk4,
                        belgehareket.AltIskonto,
                        belgehareket.IskToplam,
                        belgehareket.SatirToplam,
                        belgehareket.VgFatHarInd,
                        belgehareket.KDV,
                        belgehareket.KDVTutar,
                        belgehareket.StokExtId,
                        belgehareket.SatirSayisi,
                        belgehareket.Parabirimi,
                        belgehareket.Kur,
                        belgehareket.Aciklama,
                        belgehareket.RBMiktar,
                        belgehareket.MiktarBos,
                        belgehareket.Kampanya,
                        belgehareket.Otv,
                        belgehareket.OtvTutar,
                        belgehareket.MalinCinsi,
                         belgehareket.Birim,
                        belgehareket.SayimTarihDahil,
                        belgehareket.ZayiMiktar
                    });
                            }
                            else
                            {
                                sqlData.ExecuteSql(CmdStringBelgeHareket, new object[] {
                        obj.Id,
                        belgehareket.Stok,
                        belgehareket.Barcode,
                        belgehareket.Miktar,
                        belgehareket.Fiyat,
                        belgehareket.Tutar,
                        belgehareket.Isk1,
                        belgehareket.Isk2,
                        belgehareket.Isk3,
                        belgehareket.Isk4,
                        belgehareket.AltIskonto,
                        belgehareket.IskToplam,
                        belgehareket.SatirToplam,
                        belgehareket.VgFatHarInd,
                        belgehareket.KDV,
                        belgehareket.KDVTutar,
                        belgehareket.StokExtId,
                        belgehareket.SatirSayisi,
                        belgehareket.Parabirimi,
                        belgehareket.Kur,
                        belgehareket.Aciklama,
                        belgehareket.RBMiktar,
                        belgehareket.MiktarBos,
                        belgehareket.Kampanya,
                        belgehareket.Otv,
                        belgehareket.OtvTutar,
                        belgehareket.MalinCinsi,
                          belgehareket.Birim,
                          belgehareket.SayimTarihDahil,
                                    belgehareket.ZayiMiktar
                    });
                            }

                        }


                    }
                }
            }
            catch (Exception ex)
            {
                result.IsSuccess = false;
                result.UserMessage = ex.ToString();
                return result;
            }
            result.result_STR = "Başarılı";
            result.result_INT = 1;
            result.result_BOOL = true;
            //result.result_OBJECT = SaatListe();
            result.IsSuccess = true;
            result.UserMessage = "İşlem Başarılı";
            return result;
        }
        public ActionResultMessages Onay(int Id, int UserId, UserViewModel usModel)
        {
            ActionResultMessages result = new ActionResultMessages();
            try
            {
                var sqlData = new SqlData(new SqlConnection(modelFunctions.NewConnectionString(subeIp, dbName, sqlKullaniciName, sqlKullaniciPassword)));
                string TimeStamp = DateTime.Now.ToFileTimeUtc().ToString();
                string CmdString = "Update Belge set OnayDurumu=" + ((int)OnayDurumu.Onayli) + ",OnaylayanId=" + UserId.ToString() + " where Id=" + Id.ToString();

                //artık onaylı vegadb yaz
                ModelFunctions f = new ModelFunctions();
                f.SqlConnOpen();
                DataTable dt = f.DataTable("SELECT Id,DBName,IP,SqlName,SqlPassword  FROM  VegaDbSettings");
                string VegaDbId;
                string VegaDbName;
                string VegaDbIp;
                string VegaDbSqlName;
                string VegaDbSqlPassword;
                string connString = "";
                foreach (DataRow r in dt.Rows)
                {
                    VegaDbId = f.RTS(r, "Id");
                    VegaDbName = f.RTS(r, "DBName");
                    VegaDbIp = f.RTS(r, "IP");
                    VegaDbSqlName = f.RTS(r, "SqlName");
                    VegaDbSqlPassword = f.RTS(r, "SqlPassword");

                    connString = "Server=" + VegaDbIp + ";User Id=" + VegaDbSqlName + ";Password=" + VegaDbSqlPassword + ";Database=" + VegaDbName + "";

                }

                BelgeAlisGiderCreate blg = GetBelge(Id.ToString());






                #region*********Onaylı Belge Acrtos Tarafına Yazma********
                if (blg.BelgeTip == BelgeTipi.ZayiGirisBelgesi || blg.BelgeTip == BelgeTipi.SayimGirisBelgesi)
                {
                    if (blg.BelgeTip == BelgeTipi.ZayiGirisBelgesi)
                    {
                        #region**********Sayım Zayi Belgesi****************
                        BelgeAlisGiderCreate sayzayiblg = new BelgeAlisGiderCreate();
                        sayzayiblg.BelgeHarekets = new List<BelgeHareket>();
                        sayzayiblg.BelgeTip = BelgeTipi.ZayiGirisBelgesi;
                        sayzayiblg.Tarih = blg.Tarih;
                        sayzayiblg.BelgeNot = blg.BelgeNot;
                        sayzayiblg.Depo = blg.Depo;
                        sayzayiblg.BelgeNo = blg.BelgeNo;
                        sayzayiblg.BelgeKDV = blg.BelgeKDV;
                        sayzayiblg.AraToplam = blg.AraToplam;
                        sayzayiblg.AIsk1 = blg.AIsk1;
                        sayzayiblg.AIsk2 = blg.AIsk2;
                        sayzayiblg.AIsk3 = blg.AIsk3;
                        sayzayiblg.AIsk4 = blg.AIsk4;
                        sayzayiblg.AIsk4 = blg.AIsk4;
                        sayzayiblg.Sube = blg.Sube;
                        sayzayiblg.Cari = blg.Cari;
                        sayzayiblg.BelgeOzelKod1 = blg.BelgeOzelKod1;
                        sayzayiblg.BelgeOzelKod2 = blg.BelgeOzelKod2;
                        sayzayiblg.BelgeOzelKod3 = blg.BelgeOzelKod3;
                        sayzayiblg.BelgeOzelKod4 = blg.BelgeOzelKod4;

                        sayzayiblg.Kdv = blg.Kdv;
                        sayzayiblg.KayitTarihi = blg.KayitTarihi;
                        sayzayiblg.Kasa = blg.Kasa;
                        sayzayiblg.Kur = blg.Kur;
                        sayzayiblg.Kapali = blg.Kapali;
                        sayzayiblg.IskontoTop = blg.IskontoTop;
                        sayzayiblg.Id = blg.Id;
                        sayzayiblg.OzelKod = blg.OzelKod;
                        sayzayiblg.OzelKod9 = blg.OzelKod9;
                        sayzayiblg.Personel = blg.Personel;
                        sayzayiblg.PesinAd = blg.PesinAd;
                        sayzayiblg.SonBakiye = blg.SonBakiye;
                        sayzayiblg.Termin = blg.Termin;
                        sayzayiblg.Terminal = blg.Terminal;
                        sayzayiblg.UID = blg.UID;
                        sayzayiblg.Vade = blg.Vade;
                        #endregion**********Sayım Zayi Belgesi****************

                        foreach (var hareket in blg.BelgeHarekets)
                        {

                            BelgeHareket blhr = (BelgeHareket)hareket.Clone();
                            sayzayiblg.OzelKod = "ZAYI";
                            blhr.Miktar = hareket.Miktar;


                            List<SefimPanelStok> stokitems = new List<SefimPanelStok>();
                            stokitems = AlisBelgesiCRUD.GetStokDetayList(new UserCRUD().GetUserForSubeSettings(UserId.ToString()).FirmaID, Convert.ToInt32(blhr.Stok), 4);//generic alınmalı

                            if (stokitems.Count > 0)
                            {
                                blhr.Fiyat = stokitems[0].SatisFiyati;
                                blhr.Tutar = stokitems[0].SatisFiyati * blhr.Miktar;
                            }

                            sayzayiblg.BelgeHarekets.Add(blhr);
                        }
                        if (sayzayiblg.BelgeHarekets.Count > 0)
                        {
                            string BelgeNo = BelgeKaydet(connString, usModel, sayzayiblg);
                            blg.BelgeNo += BelgeNo.Split('*')[1].ToString() + ",";
                            blg.BelgeNoArctos = BelgeNo.Split('*')[0];
                            Update(blg);
                        }
                    }
                    else
                    {
                        #region**********Sayım Giriş Belgesi****************
                        BelgeAlisGiderCreate saygirblg = new BelgeAlisGiderCreate();
                        saygirblg.BelgeHarekets = new List<BelgeHareket>();
                        saygirblg.BelgeTip = BelgeTipi.SayimGirisBelgesi;
                        saygirblg.Tarih = blg.Tarih;
                        saygirblg.BelgeNot = blg.BelgeNot;
                        saygirblg.Depo = blg.Depo;
                        saygirblg.BelgeNo = blg.BelgeNo;
                        saygirblg.BelgeKDV = blg.BelgeKDV;
                        saygirblg.AraToplam = blg.AraToplam;
                        saygirblg.AIsk1 = blg.AIsk1;
                        saygirblg.AIsk2 = blg.AIsk2;
                        saygirblg.AIsk3 = blg.AIsk3;
                        saygirblg.AIsk4 = blg.AIsk4;
                        saygirblg.AIsk4 = blg.AIsk4;
                        saygirblg.Sube = blg.Sube;
                        saygirblg.Cari = blg.Cari;
                        saygirblg.BelgeOzelKod1 = blg.BelgeOzelKod1;
                        saygirblg.BelgeOzelKod2 = blg.BelgeOzelKod2;
                        saygirblg.BelgeOzelKod3 = blg.BelgeOzelKod3;
                        saygirblg.BelgeOzelKod4 = blg.BelgeOzelKod4;
                        saygirblg.Kdv = blg.Kdv;
                        saygirblg.KayitTarihi = blg.KayitTarihi;
                        saygirblg.Kasa = blg.Kasa;
                        saygirblg.Kur = blg.Kur;
                        saygirblg.Kapali = blg.Kapali;
                        saygirblg.IskontoTop = blg.IskontoTop;
                        saygirblg.Id = blg.Id;
                        saygirblg.OzelKod = blg.OzelKod;
                        saygirblg.OzelKod9 = blg.OzelKod9;
                        saygirblg.Personel = blg.Personel;
                        saygirblg.PesinAd = blg.PesinAd;
                        saygirblg.SonBakiye = blg.SonBakiye;
                        saygirblg.Termin = blg.Termin;
                        saygirblg.Terminal = blg.Terminal;
                        saygirblg.UID = blg.UID;
                        saygirblg.Vade = blg.Vade;
                        #endregion**********Sayım Giriş Belgesi****************

                        #region**********Sayım Zayi Belgesi****************
                        BelgeAlisGiderCreate sayzayiblg = new BelgeAlisGiderCreate();
                        sayzayiblg.BelgeHarekets = new List<BelgeHareket>();
                        sayzayiblg.BelgeTip = BelgeTipi.ZayiGirisBelgesi;
                        sayzayiblg.Tarih = blg.Tarih;
                        sayzayiblg.BelgeNot = blg.BelgeNot;
                        sayzayiblg.Depo = blg.Depo;
                        sayzayiblg.BelgeNo = blg.BelgeNo;
                        sayzayiblg.BelgeKDV = blg.BelgeKDV;
                        sayzayiblg.AraToplam = blg.AraToplam;
                        sayzayiblg.AIsk1 = blg.AIsk1;
                        sayzayiblg.AIsk2 = blg.AIsk2;
                        sayzayiblg.AIsk3 = blg.AIsk3;
                        sayzayiblg.AIsk4 = blg.AIsk4;
                        sayzayiblg.AIsk4 = blg.AIsk4;
                        sayzayiblg.Sube = blg.Sube;
                        sayzayiblg.Cari = blg.Cari;
                        sayzayiblg.BelgeOzelKod1 = blg.BelgeOzelKod1;
                        sayzayiblg.BelgeOzelKod2 = blg.BelgeOzelKod2;
                        sayzayiblg.BelgeOzelKod3 = blg.BelgeOzelKod3;
                        sayzayiblg.BelgeOzelKod4 = blg.BelgeOzelKod4;

                        sayzayiblg.Kdv = blg.Kdv;
                        sayzayiblg.KayitTarihi = blg.KayitTarihi;
                        sayzayiblg.Kasa = blg.Kasa;
                        sayzayiblg.Kur = blg.Kur;
                        sayzayiblg.Kapali = blg.Kapali;
                        sayzayiblg.IskontoTop = blg.IskontoTop;
                        sayzayiblg.Id = blg.Id;
                        sayzayiblg.OzelKod = blg.OzelKod;
                        sayzayiblg.OzelKod9 = blg.OzelKod9;
                        sayzayiblg.Personel = blg.Personel;
                        sayzayiblg.PesinAd = blg.PesinAd;
                        sayzayiblg.SonBakiye = blg.SonBakiye;
                        sayzayiblg.Termin = blg.Termin;
                        sayzayiblg.Terminal = blg.Terminal;
                        sayzayiblg.UID = blg.UID;
                        sayzayiblg.Vade = blg.Vade;
                        #endregion**********Sayım Zayi Belgesi****************
                        #region**********Sayım Fire Belgesi****************
                        BelgeAlisGiderCreate sayfireblg = new BelgeAlisGiderCreate();
                        sayfireblg.BelgeHarekets = new List<BelgeHareket>();
                        sayfireblg.BelgeTip = BelgeTipi.ZayiGirisBelgesi;
                        sayfireblg.Tarih = blg.Tarih;
                        sayfireblg.BelgeNot = blg.BelgeNot;
                        sayfireblg.Depo = blg.Depo;
                        sayfireblg.BelgeNo = blg.BelgeNo;
                        sayfireblg.BelgeKDV = blg.BelgeKDV;
                        sayfireblg.AraToplam = blg.AraToplam;
                        sayfireblg.AIsk1 = blg.AIsk1;
                        sayfireblg.AIsk2 = blg.AIsk2;
                        sayfireblg.AIsk3 = blg.AIsk3;
                        sayfireblg.AIsk4 = blg.AIsk4;
                        sayfireblg.AIsk4 = blg.AIsk4;
                        sayfireblg.Sube = blg.Sube;
                        sayfireblg.Cari = blg.Cari;
                        sayfireblg.BelgeOzelKod1 = blg.BelgeOzelKod1;
                        sayfireblg.BelgeOzelKod2 = blg.BelgeOzelKod2;
                        sayfireblg.BelgeOzelKod3 = blg.BelgeOzelKod3;
                        sayfireblg.BelgeOzelKod4 = blg.BelgeOzelKod4;

                        sayfireblg.Kdv = blg.Kdv;
                        sayfireblg.KayitTarihi = blg.KayitTarihi;
                        sayfireblg.Kasa = blg.Kasa;
                        sayfireblg.Kur = blg.Kur;
                        sayfireblg.Kapali = blg.Kapali;
                        sayfireblg.IskontoTop = blg.IskontoTop;
                        sayfireblg.Id = blg.Id;
                        sayfireblg.OzelKod = blg.OzelKod;
                        sayfireblg.OzelKod9 = blg.OzelKod9;
                        sayfireblg.Personel = blg.Personel;
                        sayfireblg.PesinAd = blg.PesinAd;
                        sayfireblg.SonBakiye = blg.SonBakiye;
                        sayfireblg.Termin = blg.Termin;
                        sayfireblg.Terminal = blg.Terminal;
                        sayfireblg.UID = blg.UID;
                        sayfireblg.Vade = blg.Vade;
                        #endregion**********Sayım Fire Belgesi****************
                        foreach (var hareket in blg.BelgeHarekets)
                        {
                            //Depoya göre stok getirmek için yazıldı.
                            decimal DepoMiktar = GetStokForDepo(usModel.FirmaID, usModel.DonemID, Convert.ToInt32(hareket.Stok), Convert.ToInt32(blg.Depo), hareket.SayimTarihDahil);
                            hareket.Envanter = DepoMiktar;
                            if (DepoMiktar < hareket.Miktar)
                            {
                                var girMiktar = hareket.Miktar - DepoMiktar;
                                hareket.Miktar = girMiktar;
                                saygirblg.BelgeHarekets.Add(hareket);
                            }
                            else
                            {
                                //ozel kod 5   ZAYI ve FIRE  miktarları 0 dan büyükse belge oluştur gönder.
                                decimal FIRE = DepoMiktar - (hareket.Miktar + hareket.ZayiMiktar);
                                decimal ZAYI = hareket.ZayiMiktar;
                                if (FIRE > 0)
                                {
                                    BelgeHareket blhr = (BelgeHareket)hareket.Clone();
                                    sayfireblg.OzelKod = "FIRE";
                                    blhr.Miktar = FIRE;
                                    sayfireblg.BelgeHarekets.Add(blhr);
                                }
                                if (ZAYI > 0)
                                {
                                    BelgeHareket blhr = (BelgeHareket)hareket.Clone();
                                    sayzayiblg.OzelKod = "ZAYI";
                                    blhr.Miktar = ZAYI;
                                    List<SefimPanelStok> stokitems = new List<SefimPanelStok>();
                                    stokitems = AlisBelgesiCRUD.GetStokDetayList(new UserCRUD().GetUserForSubeSettings(UserId.ToString()).FirmaID, Convert.ToInt32(blhr.Stok), 4);//generic alınmalı

                                    if (stokitems.Count > 0)
                                    {
                                        blhr.Fiyat = stokitems[0].SatisFiyati;
                                        blhr.Tutar = stokitems[0].SatisFiyati * blhr.Miktar;
                                    }
                                    sayzayiblg.BelgeHarekets.Add(blhr);
                                }

                            }
                        }



                        //[ENVANTER_ORG]-([SAYIM] +[ZAYI]) >= 0 AND ZAYI> 0


                        if (saygirblg.BelgeHarekets.Count > 0)
                        {
                            string BelgeNo = BelgeKaydet(connString, usModel, saygirblg);
                            blg.BelgeNo += BelgeNo.Split('*')[1].ToString() + ",";
                            blg.BelgeNoArctos = BelgeNo.Split('*')[0];
                            Update(blg);
                        }
                        if (sayfireblg.BelgeHarekets.Count > 0)
                        {
                            string BelgeNo = BelgeKaydet(connString, usModel, sayfireblg);
                            blg.BelgeNo += BelgeNo.Split('*')[1].ToString() + ",";
                            blg.BelgeNoArctos = BelgeNo.Split('*')[0];
                            Update(blg);
                        }
                        if (sayzayiblg.BelgeHarekets.Count > 0)
                        {
                            string BelgeNo = BelgeKaydet(connString, usModel, sayzayiblg);
                            blg.BelgeNo += BelgeNo.Split('*')[1].ToString() + ",";
                            blg.BelgeNoArctos = BelgeNo.Split('*')[0];
                            Update(blg);
                        }
                    }

                }
                else
                {


                    //using (VegaBelgeWrapper vegaBelgeWrapper = new VegaBelgeWrapper(conStr, UserSettings.Instance.FirmaPrefix, UserSettings.Instance.DonemPrefix, 100))

                    //    VegaBelgeBaslik belgeBaslik = new VegaBelgeBaslik();
                    //belgeBaslik.CariNo = 0;
                    //belgeBaslik.BelgeAlSat = VegaBelgeHareketTipi.Giris;
                    //belgeBaslik.Izahat = URETIMGIRIS;
                    //belgeBaslik.DepoNo = branchItem.Store.IND;
                    //belgeBaslik.Tarih = startDate.Date.Date;
                    //belgeBaslik.OzelKod1 = branchItem.Branch.SUBEADI;
                    //belgeBaslik.OzelKod2 = branchItem.Case.KASAADI;
                    //belgeBaslik.KdvDahil = 1;
                    //belgeBaslik.HareketDeposu = branchItem.Store.IND;
                    //vegaBelgeWrapper.BelgeEkle(belgeBaslik);
                    //var idP = SaveOrGetProduct(inputProductionHeader.ProductName, inputProductionHeader.VatRate, false, branchItem);
                    //var arrP = idP.Split(';');
                    //VegaBelgeHareket belgeHareket = new VegaBelgeHareket();
                    //belgeHareket.StokNo = Convert.ToInt32(arrP[0]);
                    //belgeHareket.BirimNo = Convert.ToInt32(arrP[1]);
                    //belgeHareket.DepoNo = branchItem.Store.IND;
                    //belgeHareket.Miktar = Convert.ToDecimal(inputProductionHeader.Quantity);
                    //belgeHareket.Fiyat = Convert.ToDecimal(inputProductionHeader.Price);
                    //belgeHareket.Kdv = inputProductionHeader.VatRate;
                    //belgeHareket.MalinCinsi = inputProductionHeader.ProductName;
                    //belgeHareket.StokKodu = inputProductionHeader.ProductName;
                    //belgeHareket.Tarih = inputProductionHeader.Date;
                    //vegaBelgeWrapper.SatirEkle(belgeHareket);





                    string BelgeNo = BelgeKaydet(connString, usModel, blg);
                    blg.BelgeNo = BelgeNo.Split('*')[1].ToString();
                    blg.BelgeNoArctos = BelgeNo.Split('*')[0];
                    if (blg.BelgeNo != "0" && blg.BelgeNoArctos != "0")
                        Update(blg);
                    else
                    {
                        throw new Exception("Kabul edilmiş belge tekrar kabul edilemez. Lütfen kontrol ediniz.");
                    }
                    #region ************Depo Transfer Belgesi için Depo Transfer Kabul Belgesi oluşturuluyor**********
                    if (blg.BelgeTip == BelgeTipi.DepoTransfer)
                    {
                        blg = GetBelge(Id.ToString());
                        BelgeAlisGiderCreate blgalis = new BelgeAlisGiderCreate();
                        blgalis.AIsk1 = blg.AIsk1;
                        blgalis.AIsk2 = blg.AIsk2;
                        blgalis.AIsk3 = blg.AIsk3;
                        blgalis.AIsk4 = blg.AIsk4;
                        blgalis.Aktarim = blg.Aktarim;
                        blgalis.AraToplam = blg.AraToplam;
                        blgalis.BelgeHarekets = blg.BelgeHarekets;
                        blgalis.BelgeKDV = blg.BelgeKDV;
                        blgalis.BelgeKod = blg.BelgeKod;
                        blgalis.BelgeNo = blg.BelgeNo;
                        blgalis.BelgeNot = blg.BelgeNot;
                        blgalis.BelgeOzelKod1 = blg.BelgeOzelKod1;
                        blgalis.BelgeOzelKod2 = blg.BelgeOzelKod2;
                        blgalis.BelgeOzelKod3 = blg.BelgeOzelKod3;
                        blgalis.BelgeOzelKod4 = blg.BelgeOzelKod4;
                        blgalis.BelgeTip = BelgeTipi.DepoTransferKabul;
                        blgalis.BelgeTutar = blg.BelgeTutar;
                        blgalis.Cari = blg.Cari;
                        blgalis.CariExtId = blg.CariExtId;
                        blgalis.CikanDepo = blg.Depo;
                        blgalis.Depo = blg.CikanDepo;
                        blgalis.Depozitolu = blg.Depozitolu;
                        blgalis.IskontoTop = blg.IskontoTop;
                        blgalis.Kapali = blg.Kapali;
                        blgalis.Kasa = blg.Kasa;
                        blgalis.Kdv = blg.Kdv;
                        blgalis.Kur = blg.Kur;
                        blgalis.NetToplam = blg.NetToplam;
                        blgalis.OdemeTutar = blg.OdemeTutar;
                        blgalis.OtvToplam = blg.OtvToplam;
                        blgalis.OzelKod = blg.OzelKod;
                        blgalis.OzelKod9 = blg.OzelKod9;
                        blgalis.Parabirimi = blg.Parabirimi;
                        blgalis.Personel = blg.Personel;
                        blgalis.PesinAd = blg.PesinAd;
                        blgalis.PesinVergiNo = blg.PesinVergiNo;
                        blgalis.Sablon = blg.Sablon;
                        blgalis.SablonAdi = blg.SablonAdi;
                        blgalis.SonBakiye = blg.SonBakiye;
                        blgalis.Sube = blg.Sube;
                        blgalis.Tarih = blg.Tarih;
                        blgalis.Termin = blg.Termin;
                        blgalis.Terminal = blg.Terminal;
                        blgalis.UID = blg.UID;
                        blgalis.Vade = blg.Vade;
                        blgalis.VgFatInd = blg.VgFatInd;
                        blgalis.AltBelgeNo = blg.BelgeNoArctos;
                        blgalis.AltBelgeTarihi = blg.KayitTarihi;
                        Insert(blgalis, usModel);
                    }
                    #endregion ************Depo Transfer Belgesi için Depo Transfer Kabul Belgesi oluşturuluyor**********
                }
                #endregion//*********Onaylı Belge Yazma********

                sqlData.ExecuteSql(CmdString, new object[] {
                    Id
                    });
            }
            catch (Exception ex)
            {
                result.IsSuccess = false;
                result.UserMessage = ex.ToString();
                return result;
            }
            result.result_STR = "Başarılı";
            result.result_INT = 1;
            result.result_BOOL = true;
            //result.result_OBJECT = SaatListe();
            result.IsSuccess = true;
            result.UserMessage = "İşlem Başarılı";
            return result;
        }

        private string BelgeKaydet(string connString, UserViewModel usModel, BelgeAlisGiderCreate blg)
        {
            string BelgeNoId = "";

            using (VegaBelgeWrapper vegaBelgeWrapper = new VegaBelgeWrapper(connString, "F0" + usModel.FirmaID.ToString(), "D" + usModel.DonemID.ToString().PadLeft(4, '0'), 100))//UserId standart 100 basılsın istendi. Burak Yıkmaz
            {
                VegaBelgeBaslik baslik = new VegaBelgeBaslik();
                baslik.BelgeNo = blg.BelgeNo;
                baslik.EFatura = VegaEfatura.Hayir;
                baslik.Tarih = blg.Tarih.Value;
                baslik.OzelKod4 = blg.AltCari;
                baslik.AltBelgeNo = blg.AltBelgeNo;
                baslik.AltBelgeTarihi = blg.AltBelgeTarihi;

                //TODO: Şubenin ve carinin adı alınacak. giriş çıkış miktar 0 basılıyor. belge wrapper kontrol edilecek.
                //1-sube
                //2-kasa
                //3-musteritemsilcii
                //4-althesap
                //5-belgenevi

                List<SelectListItem> itemssub = new List<SelectListItem>();
                List<SelectListItem> itemskasa = new List<SelectListItem>();
                itemssub = AlisBelgesiCRUD.GetSubeList(usModel.FirmaID, usModel.FasterSubeID);//generic alınmalı

                List<SelectListItem> selecteditemssub = itemssub.Where(a => a.Value == blg.Sube).ToList();
                if (selecteditemssub.Count > 0)
                {
                    baslik.OzelKod1 = selecteditemssub[0].Text;//blg.Sube;
                }

                itemskasa = AlisBelgesiCRUD.GetKasaList(usModel.FirmaID);//, ((int)blg.BelgeTip), usModel.SefimPanelZimmetCagrisi);//generic alınmalı
                List<SelectListItem> selecteditemskasa = itemskasa.Where(a => a.Value == blg.Kasa).ToList();
                if (selecteditemskasa.Count > 0)
                {
                    baslik.OzelKod2 = selecteditemskasa[0].Text; //blg.Cari;
                }
                baslik.OzelKod3 = blg.Personel;
                baslik.OzelKod6 = blg.BelgeOzelKod1;
                baslik.OzelKod7 = blg.BelgeOzelKod2;
                baslik.OzelKod8 = blg.BelgeOzelKod3;
                baslik.OzelKod9 = blg.BelgeOzelKod4;
                baslik.OzelKod5 = blg.OzelKod;

                if (!string.IsNullOrEmpty(blg.CikanDepo))
                {
                    baslik.HareketDeposu = Convert.ToInt32(blg.CikanDepo);
                    baslik.DepoNo = Convert.ToInt32(blg.Depo);
                }
                else
                {
                    if (!string.IsNullOrEmpty(blg.Depo))
                    {
                        baslik.HareketDeposu = Convert.ToInt32(blg.Depo);
                        //baslik.DepoNo = Convert.ToInt32(blg.Depo);
                    }
                }

                if (blg.BelgeTip == BelgeTipi.TalepBelgesi || blg.BelgeTip == BelgeTipi.ZayiGirisBelgesi)
                {
                    baslik.DepoNo = Convert.ToInt32(blg.Depo);
                }

                //iade olanlar satış iade true
                //diğerleri alış
                //talep önemi yok.
                if (blg.BelgeTip == BelgeTipi.IadeFaturasi || blg.BelgeTip == BelgeTipi.IadeIrsaliyesi)//İade Tipleri  //belge wrapper kontol alış işlemlerinde iade 1 geldi.
                {
                    baslik.BelgeAlSat = VegaBelgeHareketTipi.Giris;//0
                    baslik.Iade = 1;
                }
                else if (blg.BelgeTip == BelgeTipi.TalepBelgesi)
                {
                    baslik.BelgeAlSat = VegaBelgeHareketTipi.Giris;//0
                    baslik.Iade = 0;
                }
                else if (blg.BelgeTip == BelgeTipi.SayimGirisBelgesi)
                {
                    baslik.BelgeAlSat = VegaBelgeHareketTipi.Cikis;//1
                    baslik.Iade = 0;
                }
                else if (blg.BelgeTip == BelgeTipi.ZayiGirisBelgesi)
                {
                    baslik.BelgeAlSat = VegaBelgeHareketTipi.Giris;//0
                    baslik.Iade = 0;
                }
                else
                {
                    baslik.BelgeAlSat = VegaBelgeHareketTipi.Cikis;//1
                    baslik.Iade = 0;
                }

                baslik.KdvDahil = 0;
                baslik.AltNot = blg.BelgeNot;
                baslik.BaslikInd = blg.Id;
                baslik.Izahat = ((int)blg.BelgeTip);
                if (!string.IsNullOrEmpty(blg.Cari))
                {
                    baslik.CariNo = Convert.ToInt32(blg.Cari);
                }
                if (blg.BelgeTip == BelgeTipi.DepoTransferKabul)
                {
                    List<BelgeDepoKabul> blgkbllst = new SPosKabulIslemleriCRUD().GetKabulList(usModel.FirmaID, usModel.DonemID, usModel.DepoID);
                    if (!string.IsNullOrEmpty(blg.BelgeNo))
                        baslik.DepoTransferId = Convert.ToInt32(blg.BelgeNo);
                    if (blgkbllst.Where(a => a.IND == baslik.DepoTransferId).Count() == 0)
                    {
                        return "0*0";
                    }
                }

                vegaBelgeWrapper.BelgeEkle(baslik);

                int ZimFiyat = new UserCRUD().GetZimFiyat(usModel.ID.ToString(), baslik.CariNo);
                foreach (var har in blg.BelgeHarekets)
                {
                    VegaBelgeHareket hareket = new VegaBelgeHareket();





                    hareket.Aciklama = har.Aciklama;
                    if (!string.IsNullOrEmpty(blg.Depo))
                    {
                        hareket.DepoNo = Convert.ToInt32(blg.Depo);
                    }

                    hareket.Fiyat = har.Fiyat;

                    if (baslik.Izahat == 128 || baslik.Izahat == 129 || baslik.Izahat == 135)
                    {
                        List<SefimPanelStok> lst = GetStokDetayList(usModel.FirmaID, Convert.ToInt32(har.Stok), 4);
                        if (lst.Count > 0)
                        {
                            hareket.BirimNo = lst[0].BirimInd;
                            hareket.StokKodu = lst[0].StokKodu;//lst[0].MalinCinsi;
                            hareket.MalinCinsi = lst[0].MalinCinsi; // har.MalinCinsi;
                            hareket.Fiyat = lst[0].SatisFiyati;
                        }
                    }
                    else
                    {
                        List<SefimPanelStok> lst = GetStokDetayList(usModel.FirmaID, Convert.ToInt32(har.Stok), ZimFiyat);
                        if (lst.Count > 0)
                        {
                            hareket.BirimNo = lst[0].BirimInd;
                            hareket.StokKodu = lst[0].StokKodu;//lst[0].MalinCinsi;
                            hareket.MalinCinsi = lst[0].MalinCinsi; // har.MalinCinsi;
                        }
                    }
                    hareket.Izahat = ((int)blg.BelgeTip);

                    hareket.Kdv = har.KDV;
                    hareket.Miktar = har.Miktar;
                    hareket.Sayim = har.Miktar;
                    hareket.Envanter = har.Envanter;//Depo Miktarı
                    hareket.ISK1 = har.Isk1;
                    hareket.ISK2 = har.Isk2;
                    hareket.ISK3 = har.Isk3;
                    hareket.ISK4 = har.Isk4;


                    if (!string.IsNullOrEmpty(har.Stok))
                    {
                        hareket.StokNo = Convert.ToInt32(har.Stok);
                    }
                    hareket.Tarih = blg.Tarih.Value;
                    vegaBelgeWrapper.SatirEkle(hareket);
                }
                BelgeNoId = vegaBelgeWrapper.BelgeKaydet();
            }
            return BelgeNoId;
        }

        public ActionResultMessages Ret(int Id, int UserId)
        {
            ActionResultMessages result = new ActionResultMessages();
            try
            {
                var sqlData = new SqlData(new SqlConnection(modelFunctions.NewConnectionString(subeIp, dbName, sqlKullaniciName, sqlKullaniciPassword)));
                string TimeStamp = DateTime.Now.ToFileTimeUtc().ToString();
                string CmdString = "Update Belge set OnayDurumu=" + ((int)OnayDurumu.Ret) + ",OnaylayanId=" + UserId.ToString() + " where Id=" + Id.ToString();

                sqlData.ExecuteSql(CmdString, new object[] {
                    Id
                    });
            }
            catch (Exception ex)
            {
                result.IsSuccess = false;
                result.UserMessage = ex.ToString();
                return result;
            }
            result.result_STR = "Başarılı";
            result.result_INT = 1;
            result.result_BOOL = true;
            //result.result_OBJECT = SaatListe();
            result.IsSuccess = true;
            result.UserMessage = "İşlem Başarılı";
            return result;
        }
        public static List<SelectListItem> GetSubeList(int TanimId, string FasterSubeID)
        {
            List<SelectListItem> items = new List<SelectListItem>();
            ModelFunctions f = new ModelFunctions();
            try
            {
                f.SqlConnOpen();
                DataTable dt = f.DataTable("SELECT Id,DBName,IP,SqlName,SqlPassword  FROM  VegaDbSettings");
                string Query = "";
                foreach (DataRow r in dt.Rows)
                {
                    string VegaDbId = f.RTS(r, "Id");
                    string VegaDbName = f.RTS(r, "DBName");
                    string VegaDbIp = f.RTS(r, "IP");
                    string VegaDbSqlName = f.RTS(r, "SqlName");
                    string VegaDbSqlPassword = f.RTS(r, "SqlPassword");
                    Query = "SELECT SUBEADI,IND FROM F0" + TanimId + "TBLKRDSUBELER"; //where IND in(" + FasterSubeID + ")";
                    string connString = "Provider=SQLOLEDB;Server=" + VegaDbIp + ";User Id=" + VegaDbSqlName + ";Password=" + VegaDbSqlPassword + ";Database=" + VegaDbName + "";
                    f.SqlConnOpen(true, connString);
                    DataTable datatab = f.DataTable(Query, true);
                    if (datatab.Rows.Count > 0)
                    {
                        foreach (DataRow SubeR in datatab.Rows)
                        {
                            BelgeAlisGiderCreate model = new BelgeAlisGiderCreate();
                            items.Add(new SelectListItem
                            {
                                Text = f.RTS(SubeR, "SUBEADI").ToString(),
                                Value = f.RTS(SubeR, "IND"),
                            });
                        }
                    }
                }
                f.SqlConnClose();
            }
            catch (System.Exception ex) { }
            return items.ToList();
        }
        public static List<SelectListItem> GetSubeListForSubeIDs(int TanimId, string FasterSubeID)
        {
            List<SelectListItem> items = new List<SelectListItem>();
            ModelFunctions f = new ModelFunctions();
            try
            {
                f.SqlConnOpen();
                DataTable dt = f.DataTable("SELECT Id,DBName,IP,SqlName,SqlPassword  FROM  VegaDbSettings");
                string Query = "";
                foreach (DataRow r in dt.Rows)
                {
                    string VegaDbId = f.RTS(r, "Id");
                    string VegaDbName = f.RTS(r, "DBName");
                    string VegaDbIp = f.RTS(r, "IP");
                    string VegaDbSqlName = f.RTS(r, "SqlName");
                    string VegaDbSqlPassword = f.RTS(r, "SqlPassword");
                    Query = "SELECT SUBEADI,IND FROM F0" + TanimId + "TBLKRDSUBELER where IND in(" + FasterSubeID + ")";
                    string connString = "Provider=SQLOLEDB;Server=" + VegaDbIp + ";User Id=" + VegaDbSqlName + ";Password=" + VegaDbSqlPassword + ";Database=" + VegaDbName + "";
                    f.SqlConnOpen(true, connString);
                    DataTable datatab = f.DataTable(Query, true);
                    if (datatab.Rows.Count > 0)
                    {
                        foreach (DataRow SubeR in datatab.Rows)
                        {
                            BelgeAlisGiderCreate model = new BelgeAlisGiderCreate();
                            items.Add(new SelectListItem
                            {
                                Text = f.RTS(SubeR, "SUBEADI").ToString(),
                                Value = f.RTS(SubeR, "IND"),
                            });
                        }
                    }
                }
                f.SqlConnClose();
            }
            catch (System.Exception ex) { }
            return items.ToList();
        }
        public static List<SelectListItem> GetDepoList(int TanimId)
        {
            List<SelectListItem> items = new List<SelectListItem>();
            ModelFunctions f = new ModelFunctions();
            try
            {
                f.SqlConnOpen();
                DataTable dt = f.DataTable("SELECT Id,DBName,IP,SqlName,SqlPassword  FROM  VegaDbSettings");
                string Query = "";
                foreach (DataRow r in dt.Rows)
                {
                    string VegaDbId = f.RTS(r, "Id");
                    string VegaDbName = f.RTS(r, "DBName");
                    string VegaDbIp = f.RTS(r, "IP");
                    string VegaDbSqlName = f.RTS(r, "SqlName");
                    string VegaDbSqlPassword = f.RTS(r, "SqlPassword");
                    Query = "SELECT DEPOADI,IND FROM F0" + TanimId + "TBLDEPOLAR WHERE STATUS=1";
                    string connString = "Provider=SQLOLEDB;Server=" + VegaDbIp + ";User Id=" + VegaDbSqlName + ";Password=" + VegaDbSqlPassword + ";Database=" + VegaDbName + "";
                    f.SqlConnOpen(true, connString);
                    DataTable AcikHesapDt = f.DataTable(Query, true);
                    if (AcikHesapDt.Rows.Count > 0)
                    {
                        foreach (DataRow SubeR in AcikHesapDt.Rows)
                        {
                            BelgeAlisGiderCreate model = new BelgeAlisGiderCreate();
                            items.Add(new SelectListItem
                            {
                                Text = f.RTS(SubeR, "DEPOADI").ToString(),
                                Value = f.RTS(SubeR, "IND"),
                            });
                        }
                    }
                }
                f.SqlConnClose();
            }
            catch (System.Exception ex) { }
            return items.ToList();
        }
        public static List<SelectListItem> GetDepoForCari(int TanimId, string DepoId)
        {
            List<SelectListItem> items = new List<SelectListItem>();
            ModelFunctions f = new ModelFunctions();
            try
            {
                f.SqlConnOpen();
                DataTable dt = f.DataTable("SELECT Id,DBName,IP,SqlName,SqlPassword  FROM  VegaDbSettings");
                string Query = "";
                foreach (DataRow r in dt.Rows)
                {
                    string VegaDbId = f.RTS(r, "Id");
                    string VegaDbName = f.RTS(r, "DBName");
                    string VegaDbIp = f.RTS(r, "IP");
                    string VegaDbSqlName = f.RTS(r, "SqlName");
                    string VegaDbSqlPassword = f.RTS(r, "SqlPassword");
                    Query = "SELECT ACIKLAMA,IND FROM F0" + TanimId + "TBLDEPOLAR WHERE STATUS=1 and IND=" + DepoId.ToString();
                    string connString = "Provider=SQLOLEDB;Server=" + VegaDbIp + ";User Id=" + VegaDbSqlName + ";Password=" + VegaDbSqlPassword + ";Database=" + VegaDbName + "";
                    f.SqlConnOpen(true, connString);
                    DataTable AcikHesapDt = f.DataTable(Query, true);
                    if (AcikHesapDt.Rows.Count > 0)
                    {
                        foreach (DataRow SubeR in AcikHesapDt.Rows)
                        {
                            BelgeAlisGiderCreate model = new BelgeAlisGiderCreate();
                            items.Add(new SelectListItem
                            {
                                Text = f.RTS(SubeR, "ACIKLAMA").ToString(),
                                Value = f.RTS(SubeR, "IND"),
                            });
                        }
                    }
                }
                f.SqlConnClose();
            }
            catch (System.Exception ex) { }
            return items.ToList();
        }
        public static List<SelectListItem> GetKasaList(int TanimId)
        {
            List<SelectListItem> items = new List<SelectListItem>();
            ModelFunctions f = new ModelFunctions();
            try
            {
                f.SqlConnOpen();
                DataTable dt = f.DataTable("SELECT Id,DBName,IP,SqlName,SqlPassword  FROM  VegaDbSettings");
                string Query = "";
                foreach (DataRow r in dt.Rows)
                {
                    string VegaDbId = f.RTS(r, "Id");
                    string VegaDbName = f.RTS(r, "DBName");
                    string VegaDbIp = f.RTS(r, "IP");
                    string VegaDbSqlName = f.RTS(r, "SqlName");
                    string VegaDbSqlPassword = f.RTS(r, "SqlPassword");
                    Query = "SELECT SUBEADI,IND FROM F0" + TanimId + "TBLKRDSUBELER";
                    string connString = "Provider=SQLOLEDB;Server=" + VegaDbIp + ";User Id=" + VegaDbSqlName + ";Password=" + VegaDbSqlPassword + ";Database=" + VegaDbName + "";
                    f.SqlConnOpen(true, connString);
                    DataTable AcikHesapDt = f.DataTable(Query, true);
                    if (AcikHesapDt.Rows.Count > 0)
                    {
                        foreach (DataRow SubeR in AcikHesapDt.Rows)
                        {
                            BelgeAlisGiderCreate model = new BelgeAlisGiderCreate();
                            items.Add(new SelectListItem
                            {
                                Text = f.RTS(SubeR, "SUBEADI").ToString(),
                                Value = f.RTS(SubeR, "IND"),
                            });
                        }
                    }
                }
                f.SqlConnClose();
            }
            catch (System.Exception ex) { }
            return items.ToList();
        }
        public static List<SelectListItem> GetSablonList(BelgeTipi blgtp)
        {
            List<SelectListItem> items = new List<SelectListItem>();
            ModelFunctions f = new ModelFunctions();
            try
            {
                f.SqlConnOpen();
                DataTable dt = f.DataTable("select Id,[SablonAdi]  FROM  Belge where [BelgeTip]=" + ((int)blgtp).ToString());
                foreach (DataRow r in dt.Rows)
                {

                    items.Add(new SelectListItem
                    {
                        Text = f.RTS(r, "SablonAdi").ToString(),
                        Value = f.RTS(r, "Id"),
                    });

                }
                f.SqlConnClose();
            }
            catch (System.Exception ex) { }
            return items.ToList();
        }
        public static List<SelectListItem> GetCariList(int TanimId, int BelgeTip, string SefimPanelZimmetCagrisi)
        {


            List<SelectListItem> items = new List<SelectListItem>();
            ModelFunctions f = new ModelFunctions();
            try
            {
                f.SqlConnOpen();
                DataTable dt = f.DataTable("SELECT Id,DBName,IP,SqlName,SqlPassword  FROM  VegaDbSettings");
                string Query = "";
                foreach (DataRow r in dt.Rows)
                {
                    string VegaDbId = f.RTS(r, "Id");
                    string VegaDbName = f.RTS(r, "DBName");
                    string VegaDbIp = f.RTS(r, "IP");
                    string VegaDbSqlName = f.RTS(r, "SqlName");
                    string VegaDbSqlPassword = f.RTS(r, "SqlPassword");
                    if (((BelgeTipi)BelgeTip) == BelgeTipi.TalepBelgesi)
                    {
                        Query = "SELECT IND,FIRMAKODU + ' - ' + FIRMAADI AS FIRMAADI,ZIMFIYAT FROM F0" + TanimId + "TBLCARI WHERE STATUS=1 and FIRMATIPI=9"; //and IND in(" + SefimPanelZimmetCagrisi + ")";
                    }
                    else
                    {
                        Query = "SELECT IND,FIRMAKODU + ' - ' + FIRMAADI AS FIRMAADI,ZIMFIYAT FROM F0" + TanimId + "TBLCARI WHERE STATUS=1";
                    }
                    string connString = "Provider=SQLOLEDB;Server=" + VegaDbIp + ";User Id=" + VegaDbSqlName + ";Password=" + VegaDbSqlPassword + ";Database=" + VegaDbName + "";
                    f.SqlConnOpen(true, connString);
                    DataTable AcikHesapDt = f.DataTable(Query, true);
                    if (AcikHesapDt.Rows.Count > 0)
                    {
                        foreach (DataRow SubeR in AcikHesapDt.Rows)
                        {
                            BelgeAlisGiderCreate model = new BelgeAlisGiderCreate();
                            items.Add(new SelectListItem
                            {
                                Text = f.RTS(SubeR, "FIRMAADI").ToString(),
                                Value = f.RTS(SubeR, "IND"),
                            });
                        }
                    }
                }
                f.SqlConnClose();
            }
            catch (System.Exception ex) { }
            return items.ToList();
        }

        public static List<SelectListItem> GetOzelKod1List(int TanimId)
        {
            List<SelectListItem> items = new List<SelectListItem>();
            ModelFunctions f = new ModelFunctions();
            try
            {
                f.SqlConnOpen();
                DataTable dt = f.DataTable("SELECT Id,DBName,IP,SqlName,SqlPassword  FROM  VegaDbSettings");
                string Query = "";
                foreach (DataRow r in dt.Rows)
                {
                    string VegaDbId = f.RTS(r, "Id");
                    string VegaDbName = f.RTS(r, "DBName");
                    string VegaDbIp = f.RTS(r, "IP");
                    string VegaDbSqlName = f.RTS(r, "SqlName");
                    string VegaDbSqlPassword = f.RTS(r, "SqlPassword");
                    Query = "SELECT * FROM F0" + TanimId + "TBLBELGEKODTAN where CATEGORY=6";//File.ReadAllText(HostingEnvironment.MapPath("/Sql/SqlVegaDb/Firma.sql"), System.Text.UTF8Encoding.Default);
                    string connString = "Provider=SQLOLEDB;Server=" + VegaDbIp + ";User Id=" + VegaDbSqlName + ";Password=" + VegaDbSqlPassword + ";Database=" + VegaDbName + "";
                    f.SqlConnOpen(true, connString);
                    DataTable AcikHesapDt = f.DataTable(Query, true);
                    if (AcikHesapDt.Rows.Count > 0)
                    {


                        foreach (DataRow SubeR in AcikHesapDt.Rows)
                        {
                            items.Add(new SelectListItem
                            {
                                Text = f.RTS(SubeR, "KOD").ToString(),
                                Value = f.RTS(SubeR, "KOD").ToString(),
                            });

                        }
                    }
                }
                f.SqlConnClose();
            }
            catch (System.Exception ex) { }

            return items.ToList();
        }
        public static List<SelectListItem> GetOzelKod2List(int TanimId)
        {
            List<SelectListItem> items = new List<SelectListItem>();
            ModelFunctions f = new ModelFunctions();
            try
            {
                f.SqlConnOpen();
                DataTable dt = f.DataTable("SELECT Id,DBName,IP,SqlName,SqlPassword  FROM  VegaDbSettings");
                string Query = "";
                foreach (DataRow r in dt.Rows)
                {
                    string VegaDbId = f.RTS(r, "Id");
                    string VegaDbName = f.RTS(r, "DBName");
                    string VegaDbIp = f.RTS(r, "IP");
                    string VegaDbSqlName = f.RTS(r, "SqlName");
                    string VegaDbSqlPassword = f.RTS(r, "SqlPassword");
                    Query = "SELECT * FROM F0" + TanimId + "TBLBELGEKODTAN where CATEGORY=7";//File.ReadAllText(HostingEnvironment.MapPath("/Sql/SqlVegaDb/Firma.sql"), System.Text.UTF8Encoding.Default);
                    string connString = "Provider=SQLOLEDB;Server=" + VegaDbIp + ";User Id=" + VegaDbSqlName + ";Password=" + VegaDbSqlPassword + ";Database=" + VegaDbName + "";
                    f.SqlConnOpen(true, connString);
                    DataTable AcikHesapDt = f.DataTable(Query, true);
                    if (AcikHesapDt.Rows.Count > 0)
                    {


                        foreach (DataRow SubeR in AcikHesapDt.Rows)
                        {
                            items.Add(new SelectListItem
                            {
                                Text = f.RTS(SubeR, "KOD").ToString(),
                                Value = f.RTS(SubeR, "KOD").ToString(),
                            });

                        }
                    }
                }
                f.SqlConnClose();
            }
            catch (System.Exception ex) { }

            return items.ToList();
        }
        public static List<SelectListItem> GetOzelKod3List(int TanimId)
        {
            List<SelectListItem> items = new List<SelectListItem>();
            ModelFunctions f = new ModelFunctions();
            try
            {
                f.SqlConnOpen();
                DataTable dt = f.DataTable("SELECT Id,DBName,IP,SqlName,SqlPassword  FROM  VegaDbSettings");
                string Query = "";
                foreach (DataRow r in dt.Rows)
                {
                    string VegaDbId = f.RTS(r, "Id");
                    string VegaDbName = f.RTS(r, "DBName");
                    string VegaDbIp = f.RTS(r, "IP");
                    string VegaDbSqlName = f.RTS(r, "SqlName");
                    string VegaDbSqlPassword = f.RTS(r, "SqlPassword");
                    Query = "SELECT * FROM F0" + TanimId + "TBLBELGEKODTAN where CATEGORY=8";//File.ReadAllText(HostingEnvironment.MapPath("/Sql/SqlVegaDb/Firma.sql"), System.Text.UTF8Encoding.Default);
                    string connString = "Provider=SQLOLEDB;Server=" + VegaDbIp + ";User Id=" + VegaDbSqlName + ";Password=" + VegaDbSqlPassword + ";Database=" + VegaDbName + "";
                    f.SqlConnOpen(true, connString);
                    DataTable AcikHesapDt = f.DataTable(Query, true);
                    if (AcikHesapDt.Rows.Count > 0)
                    {


                        foreach (DataRow SubeR in AcikHesapDt.Rows)
                        {
                            items.Add(new SelectListItem
                            {
                                Text = f.RTS(SubeR, "KOD").ToString(),
                                Value = f.RTS(SubeR, "KOD").ToString(),
                            });

                        }
                    }
                }
                f.SqlConnClose();
            }
            catch (System.Exception ex) { }

            return items.ToList();
        }
        public static List<SelectListItem> GetOzelKod4List(int TanimId)
        {
            List<SelectListItem> items = new List<SelectListItem>();
            ModelFunctions f = new ModelFunctions();
            try
            {
                f.SqlConnOpen();
                DataTable dt = f.DataTable("SELECT Id,DBName,IP,SqlName,SqlPassword  FROM  VegaDbSettings");
                string Query = "";
                foreach (DataRow r in dt.Rows)
                {
                    string VegaDbId = f.RTS(r, "Id");
                    string VegaDbName = f.RTS(r, "DBName");
                    string VegaDbIp = f.RTS(r, "IP");
                    string VegaDbSqlName = f.RTS(r, "SqlName");
                    string VegaDbSqlPassword = f.RTS(r, "SqlPassword");
                    Query = "SELECT * FROM F0" + TanimId + "TBLBELGEKODTAN where CATEGORY=9";//File.ReadAllText(HostingEnvironment.MapPath("/Sql/SqlVegaDb/Firma.sql"), System.Text.UTF8Encoding.Default);
                    string connString = "Provider=SQLOLEDB;Server=" + VegaDbIp + ";User Id=" + VegaDbSqlName + ";Password=" + VegaDbSqlPassword + ";Database=" + VegaDbName + "";
                    f.SqlConnOpen(true, connString);
                    DataTable AcikHesapDt = f.DataTable(Query, true);
                    if (AcikHesapDt.Rows.Count > 0)
                    {


                        foreach (DataRow SubeR in AcikHesapDt.Rows)
                        {
                            items.Add(new SelectListItem
                            {
                                Text = f.RTS(SubeR, "KOD").ToString(),
                                Value = f.RTS(SubeR, "KOD").ToString(),
                            });

                        }
                    }
                }
                f.SqlConnClose();
            }
            catch (System.Exception ex) { }

            return items.ToList();
        }
        public static List<SelectListItem> GetOzelKod5List(int TanimId)
        {
            List<SelectListItem> items = new List<SelectListItem>();
            ModelFunctions f = new ModelFunctions();
            try
            {
                f.SqlConnOpen();
                DataTable dt = f.DataTable("SELECT Id,DBName,IP,SqlName,SqlPassword  FROM  VegaDbSettings");
                string Query = "";
                foreach (DataRow r in dt.Rows)
                {
                    string VegaDbId = f.RTS(r, "Id");
                    string VegaDbName = f.RTS(r, "DBName");
                    string VegaDbIp = f.RTS(r, "IP");
                    string VegaDbSqlName = f.RTS(r, "SqlName");
                    string VegaDbSqlPassword = f.RTS(r, "SqlPassword");
                    Query = "SELECT * FROM F0" + TanimId + "TBLBELGEKODTAN where CATEGORY=5";//File.ReadAllText(HostingEnvironment.MapPath("/Sql/SqlVegaDb/Firma.sql"), System.Text.UTF8Encoding.Default);
                    string connString = "Provider=SQLOLEDB;Server=" + VegaDbIp + ";User Id=" + VegaDbSqlName + ";Password=" + VegaDbSqlPassword + ";Database=" + VegaDbName + "";
                    f.SqlConnOpen(true, connString);
                    DataTable AcikHesapDt = f.DataTable(Query, true);
                    if (AcikHesapDt.Rows.Count > 0)
                    {


                        foreach (DataRow SubeR in AcikHesapDt.Rows)
                        {
                            items.Add(new SelectListItem
                            {
                                Text = f.RTS(SubeR, "KOD").ToString(),
                                Value = f.RTS(SubeR, "KOD").ToString(),
                            });

                        }
                    }
                }
                f.SqlConnClose();
            }
            catch (System.Exception ex) { }

            return items.ToList();
        }

        public static List<SelectListItem> GetStokSelectList(int TanimId)
        {
            List<SelectListItem> items = new List<SelectListItem>();
            ModelFunctions f = new ModelFunctions();
            try
            {

                f.SqlConnOpen();
                DataTable dt = f.DataTable("SELECT Id,DBName,IP,SqlName,SqlPassword  FROM  VegaDbSettings");
                string Query = "";
                foreach (DataRow r in dt.Rows)
                {
                    string VegaDbId = f.RTS(r, "Id");
                    string VegaDbName = f.RTS(r, "DBName");
                    string VegaDbIp = f.RTS(r, "IP");
                    string VegaDbSqlName = f.RTS(r, "SqlName");
                    string VegaDbSqlPassword = f.RTS(r, "SqlPassword");
                    Query = "SELECT S.STOKKODU + ' | ' + S.MALINCINSI + ' | ' + ISNULL(B.BARCODE,'') + ' | ' + ISNULL(B.BIRIMADI,'') AS STOK, S.IND STOKIND from F0" + TanimId + "TBLSTOKLAR S,F0" + TanimId + "TBLBIRIMLEREX B WHERE S.IND=B.STOKNO AND STATUS=1";
                    string connString = "Provider=SQLOLEDB;Server=" + VegaDbIp + ";User Id=" + VegaDbSqlName + ";Password=" + VegaDbSqlPassword + ";Database=" + VegaDbName + "";
                    f.SqlConnOpen(true, connString);

                    Singleton.WritingLogFile2("GetStokSelectList_Log:", "TanımID:" + TanimId, "", "Query:" + Query);

                    DataTable AcikHesapDt = f.DataTable(Query, true);
                    if (AcikHesapDt.Rows.Count > 0)
                    {
                        foreach (DataRow SubeR in AcikHesapDt.Rows)
                        {
                            BelgeAlisGiderCreate model = new BelgeAlisGiderCreate();
                            items.Add(new SelectListItem
                            {
                                Text = f.RTS(SubeR, "STOK").ToString(),
                                Value = f.RTS(SubeR, "STOKIND"),
                            });
                        }
                    }
                }
                f.SqlConnClose();
            }
            catch (Exception ex)
            {
                Singleton.WritingLogFile2("GetStokSelectList:", ex.Message.ToString(), "", ex.StackTrace);
            }
            return items.ToList();
        }

        public static List<Bom> GetStokSelectList2(int TanimId, int StokId)
        {
            var items = new List<Bom>();
            var f = new ModelFunctions();
            try
            {
                f.SqlConnOpen();
                var dtVegaDbSettings = f.DataTable("SELECT Id,DBName,IP,SqlName,SqlPassword  FROM  VegaDbSettings");
                string Query = string.Empty;
                foreach (DataRow r in dtVegaDbSettings.Rows)
                {
                    string VegaDbId = f.RTS(r, "Id");
                    string VegaDbName = f.RTS(r, "DBName");
                    string VegaDbIp = f.RTS(r, "IP");
                    string VegaDbSqlName = f.RTS(r, "SqlName");
                    string VegaDbSqlPassword = f.RTS(r, "SqlPassword");
                    Query = "SELECT S.IND, S.STOKKODU , S.MALINCINSI , ISNULL(B.BARCODE,'') , ISNULL(B.BIRIMADI,'') AS STOK, S.IND STOKIND from F0" + TanimId + "TBLSTOKLAR S,F0" + TanimId + "TBLBIRIMLEREX B WHERE S.IND=B.STOKNO AND STATUS=1 and S.IND=" + StokId;
                   
                    Singleton.WritingLog("GetStokSelectList2", "bomStokId:" + StokId + " TanimId: " + TanimId + " QueryBom:" + Query.ToString());

                    string connString = "Provider=SQLOLEDB;Server=" + VegaDbIp + ";User Id=" + VegaDbSqlName + ";Password=" + VegaDbSqlPassword + ";Database=" + VegaDbName + "";
                    f.SqlConnOpen(true, connString);
                    var tblStoklarDt = f.DataTable(Query, true);
                    if (tblStoklarDt.Rows.Count > 0)
                    {
                        foreach (DataRow SubeR in tblStoklarDt.Rows)
                        {
                            var model = new Bom();
                            items.Add(new Bom
                            {
                                MalinCinsi = f.RTS(SubeR, "MALINCINSI").ToString(),
                                MaterialName = f.RTS(SubeR, "STOKKODU"),
                                StokID = f.RTI(SubeR, "IND"),
                            });
                        }
                    }
                }
                f.SqlConnClose();
            }
            catch (Exception ex)
            {
                Singleton.WritingLogFile2("AlisBelgesiCRUD_GetStokSelectList2:", ex.Message.ToString(), "", ex.StackTrace);
            }
            return items;
        }

        public static List<SelectListItem> GetStokSelectHizmetList(int TanimId)
        {
            List<SelectListItem> items = new List<SelectListItem>();
            ModelFunctions f = new ModelFunctions();
            try
            {
                f.SqlConnOpen();
                DataTable dt = f.DataTable("SELECT Id,DBName,IP,SqlName,SqlPassword  FROM  VegaDbSettings");
                string Query = "";
                foreach (DataRow r in dt.Rows)
                {
                    string VegaDbId = f.RTS(r, "Id");
                    string VegaDbName = f.RTS(r, "DBName");
                    string VegaDbIp = f.RTS(r, "IP");
                    string VegaDbSqlName = f.RTS(r, "SqlName");
                    string VegaDbSqlPassword = f.RTS(r, "SqlPassword");
                    Query = "SELECT S.STOKKODU + ' - ' + S.MALINCINSI + ' - ' + ISNULL(B.BARCODE,'')+ ' - ' + ISNULL(B.BIRIMADI,'') AS STOK, S.IND STOKIND from F0" + TanimId + "TBLSTOKLAR S,F0" + TanimId + "TBLBIRIMLEREX B WHERE S.IND=B.STOKNO AND STATUS=1 AND STOKTIPI=3";
                    string connString = "Provider=SQLOLEDB;Server=" + VegaDbIp + ";User Id=" + VegaDbSqlName + ";Password=" + VegaDbSqlPassword + ";Database=" + VegaDbName + "";
                    f.SqlConnOpen(true, connString);
                    DataTable AcikHesapDt = f.DataTable(Query, true);
                    if (AcikHesapDt.Rows.Count > 0)
                    {
                        foreach (DataRow SubeR in AcikHesapDt.Rows)
                        {
                            BelgeAlisGiderCreate model = new BelgeAlisGiderCreate();
                            items.Add(new SelectListItem
                            {
                                Text = f.RTS(SubeR, "STOK").ToString(),
                                Value = f.RTS(SubeR, "STOKIND"),
                            });
                        }
                    }
                }
                f.SqlConnClose();
            }
            catch (System.Exception ex) { }
            return items.ToList();
        }

        public static List<SefimPanelStok> GetStokDetayList(int TanimId, int StokId, int ZimFiyat)
        {
            List<SefimPanelStok> items = new List<SefimPanelStok>();
            ModelFunctions f = new ModelFunctions();
            try
            {
                f.SqlConnOpen();
                DataTable dt = f.DataTable("SELECT Id,DBName,IP,SqlName,SqlPassword  FROM  VegaDbSettings");
                string Query = "";
                foreach (DataRow r in dt.Rows)
                {
                    string VegaDbId = f.RTS(r, "Id");
                    string VegaDbName = f.RTS(r, "DBName");
                    string VegaDbIp = f.RTS(r, "IP");
                    string VegaDbSqlName = f.RTS(r, "SqlName");
                    string VegaDbSqlPassword = f.RTS(r, "SqlPassword");

                    if (ZimFiyat == 4)
                    {
                        Query = "SELECT S.IND STOKIND,B.BIRIMADI,B.IND BIRIMIND,S.STOKKODU,B.BARCODE,S.MALINCINSI,S.ALISFIYATI,S.MALIYET SATISFIYATI,B.KDVDAHIL,S.ALISKDVORANI as KDV from F0" + TanimId + "TBLSTOKLAR S,F0" + TanimId + "TBLBIRIMLEREX B WHERE S.IND=B.STOKNO AND STATUS=1 AND S.IND=" + StokId + "";
                    }
                    else if (ZimFiyat > 0 && ZimFiyat < 10 && ZimFiyat != 4 && ZimFiyat != 5)
                    {
                        Query = "SELECT S.IND STOKIND,B.BIRIMADI,B.IND BIRIMIND,S.STOKKODU,B.BARCODE,S.MALINCINSI,S.ALISFIYATI,case when " + ZimFiyat + ">0 then B.SATISFIYATI" + ZimFiyat + " else 0 end SATISFIYATI,B.KDVDAHIL,S.ALISKDVORANI as KDV from F0" + TanimId + "TBLSTOKLAR S,F0" + TanimId + "TBLBIRIMLEREX B WHERE S.IND=B.STOKNO AND STATUS=1 AND S.IND=" + StokId + "";
                    }
                    else if (ZimFiyat == 5 || ZimFiyat == 10)
                    {
                        Query = "SELECT S.IND STOKIND,B.BIRIMADI,B.IND BIRIMIND,S.STOKKODU,B.BARCODE,S.MALINCINSI,S.ALISFIYATI,S.ALISFIYATI as SATISFIYATI,B.KDVDAHIL,S.ALISKDVORANI as KDV from F0" + TanimId + "TBLSTOKLAR S,F0" + TanimId + "TBLBIRIMLEREX B WHERE S.IND=B.STOKNO AND STATUS=1 AND S.IND=" + StokId + "";
                    }
                    else
                    {
                        Query = "SELECT S.IND STOKIND,B.BIRIMADI,B.IND BIRIMIND,S.STOKKODU,B.BARCODE,S.MALINCINSI,S.ALISFIYATI, B.SATISFIYATI1 SATISFIYATI,B.KDVDAHIL,S.ALISKDVORANI as KDV from F0" + TanimId + "TBLSTOKLAR S,F0" + TanimId + "TBLBIRIMLEREX B WHERE S.IND=B.STOKNO AND STATUS=1 AND S.IND=" + StokId + "";
                    }
                    string connString = "Provider=SQLOLEDB;Server=" + VegaDbIp + ";User Id=" + VegaDbSqlName + ";Password=" + VegaDbSqlPassword + ";Database=" + VegaDbName + "";
                    f.SqlConnOpen(true, connString);
                    DataTable AcikHesapDt = f.DataTable(Query, true);
                    if (AcikHesapDt.Rows.Count > 0)
                    {
                        foreach (DataRow SubeR in AcikHesapDt.Rows)
                        {
                            SefimPanelStok model = new SefimPanelStok();
                            items.Add(new SefimPanelStok
                            {
                                StokInd = f.RTI(SubeR, "STOKIND"),
                                BirimInd = f.RTI(SubeR, "BIRIMIND"),
                                StokKodu = f.RTS(SubeR, "STOKKODU").ToString(),
                                Barkod = f.RTS(SubeR, "BARCODE").ToString(),
                                MalinCinsi = f.RTS(SubeR, "MALINCINSI").ToString(),
                                SatisFiyati = f.RTD(SubeR, "SATISFIYATI"),
                                AlisFiyati = f.RTD(SubeR, "ALISFIYATI"),
                                KdvDahil = f.RTI(SubeR, "KDVDAHIL"),
                                Kdv = f.RTD(SubeR, "KDV"),
                                BirimAdi = f.RTS(SubeR, "BIRIMADI").ToString(),
                            });
                        }
                    }
                }
                f.SqlConnClose();
            }
            catch (System.Exception ex) { }
            return items.ToList();
        }
        public static decimal GetStokForDepo(int TanimId, int DonemId, int StokId, int DepoId, bool SayimTarihDahil)
        {
            decimal Adet = new decimal();
            ModelFunctions f = new ModelFunctions();
            try
            {
                f.SqlConnOpen();
                DataTable dt = f.DataTable("SELECT Id,DBName,IP,SqlName,SqlPassword  FROM  VegaDbSettings");
                string Query = "";
                foreach (DataRow r in dt.Rows)
                {
                    string VegaDbId = f.RTS(r, "Id");
                    string VegaDbName = f.RTS(r, "DBName");
                    string VegaDbIp = f.RTS(r, "IP");
                    string VegaDbSqlName = f.RTS(r, "SqlName");
                    string VegaDbSqlPassword = f.RTS(r, "SqlPassword");


                    if (SayimTarihDahil)
                    {
                        Query = "SELECT SUM(ENVANTER) as Adet FROM F0" + TanimId.ToString() + "D" + DonemId.ToString().PadLeft(4, '0') + "TBLDEPOENVANTER WHERE DEPO = " + DepoId + " AND STOKNO = " + StokId;
                    }
                    else
                    {
                        Query = "SELECT SUM(ENVANTER) as Adet FROM F0" + TanimId.ToString() + "D" + DonemId.ToString().PadLeft(4, '0') + "TBLDEPOENVANTER WHERE DEPO = " + DepoId + " AND STOKNO = " + StokId + " AND TARIH<'" + DateTime.Now.ToString("yyyy.MM.dd") + "'";
                    }


                    string connString = "Provider=SQLOLEDB;Server=" + VegaDbIp + ";User Id=" + VegaDbSqlName + ";Password=" + VegaDbSqlPassword + ";Database=" + VegaDbName + "";
                    f.SqlConnOpen(true, connString);

                    DataTable AcikHesapDt = f.DataTable(Query, true);
                    if (AcikHesapDt.Rows.Count > 0)
                    {

                        foreach (DataRow SubeR in AcikHesapDt.Rows)
                        {
                            Adet = f.RTD(SubeR, "Adet");
                        }
                    }
                }
                f.SqlConnClose();
            }
            catch (System.Exception ex) { }
            return Adet;
        }
        public BelgeList BelgeList(string BaslangicTarihi = "", string BitisTarihi = "", string BelgeTipi = "", string Sube = "", string OnayDurumu = "")
        {
            BelgeList blglst = new BelgeList();
            blglst.Belgeler = new List<BelgeAlisGiderCreate>();
            ModelFunctions f = new ModelFunctions();
            try
            {
                f.SqlConnOpen();
                string queryPlus = "";
                if (!string.IsNullOrEmpty(BaslangicTarihi))
                {
                    queryPlus += "and Belge.Tarih>='" + BaslangicTarihi + "'";
                }
                if (!string.IsNullOrEmpty(BitisTarihi))
                {
                    queryPlus += "and Belge.Tarih<='" + BitisTarihi + "'";
                }
                if (!string.IsNullOrEmpty(BelgeTipi) && BelgeTipi != "0")
                {
                    queryPlus += "and Belge.BelgeTip=" + BelgeTipi;
                }
                if (!string.IsNullOrEmpty(Sube) && Sube != "0")
                {
                    queryPlus += "and Belge.Sube=" + Sube;
                }
                if (!string.IsNullOrEmpty(OnayDurumu) && OnayDurumu != "0")
                {
                    queryPlus += "and Belge.OnayDurumu=" + OnayDurumu;
                }

                DataTable dt = f.DataTable("SELECT [Id],[Cari],[BelgeNo],[BelgeKod],[Tarih],[BelgeTutar],[AraToplam],[BelgeKDV],[NetToplam],[BelgeTip],[IskontoTop],[AIsk1],[AIsk2],[AIsk3],[AIsk4],[Vade],[Termin],[Parabirimi],[Kur],[VgFatInd],[Terminal],[Aktarim],[Kapali],[BelgeNot],[PesinAd],[PesinVergiNo],[OzelKod],[Depo],[CariExtId],[KayitTarihi],[CikanDepo],[UID],[OzelKod9],[Personel],[Depozitolu],[OtvToplam],[OdemeTutar],[SonBakiye],[BelgeOzelKod1],[BelgeOzelKod2],[BelgeOzelKod3],[BelgeOzelKod4],[OnayDurumu],[OnaylayanId],[BelgeNoArctos],(select top 1 SubeSettings.SubeName from SubeSettings where SubeSettings.FasterSubeID=Belge.Sube) as Sube  FROM  Belge where [BelgeTip]<>255" + queryPlus);
                foreach (DataRow r in dt.Rows)
                {

                    BelgeAlisGiderCreate blg = new BelgeAlisGiderCreate();
                    blg.Id = f.RTI(r, "Id");
                    blg.Cari = f.RTS(r, "Cari").ToString();
                    blg.BelgeNo = f.RTS(r, "BelgeNo").ToString();
                    blg.BelgeKod = f.RTS(r, "BelgeKod").ToString();
                    blg.Tarih = Convert.ToDateTime(f.RTS(r, "Tarih").ToString());
                    blg.BelgeTutar = f.RTD(r, "BelgeTutar");
                    blg.AraToplam = f.RTI(r, "AraToplam");
                    blg.BelgeKDV = f.RTI(r, "BelgeKDV");
                    blg.NetToplam = f.RTI(r, "NetToplam");
                    blg.BelgeTip = ((Enums.General.BelgeTipi)f.RTI(r, "BelgeTip"));
                    blg.IskontoTop = f.RTI(r, "IskontoTop");
                    blg.AIsk1 = f.RTD(r, "AIsk1");
                    blg.AIsk2 = f.RTD(r, "AIsk2");
                    blg.AIsk3 = f.RTI(r, "AIsk3");
                    blg.AIsk4 = f.RTI(r, "AIsk4");
                    //blg.//Vade = f.RTI(r, "Vade"),
                    //blg.//Termin = f.RTI(r, "Termin"),
                    //blg.Parabirimi = f.RTS(r, "Parabirimi").ToString(),
                    blg.Kur = f.RTI(r, "Kur");
                    blg.VgFatInd = f.RTI(r, "VgFatInd");
                    blg.Terminal = f.RTI(r, "Terminal");
                    blg.Aktarim = f.RTI(r, "Aktarim");
                    blg.Kapali = f.RTI(r, "Kapali");
                    blg.BelgeNot = f.RTS(r, "BelgeNot").ToString();
                    blg.PesinAd = f.RTS(r, "PesinAd").ToString();
                    blg.PesinVergiNo = f.RTS(r, "PesinVergiNo").ToString();
                    blg.OzelKod = f.RTS(r, "OzelKod").ToString();
                    blg.Depo = f.RTS(r, "Depo").ToString();
                    blg.CariExtId = f.RTI(r, "CariExtId");
                    blg.KayitTarihi = Convert.ToDateTime(f.RTS(r, "KayitTarihi"));
                    blg.CikanDepo = f.RTS(r, "CikanDepo");
                    blg.UID = f.RTS(r, "UID").ToString();
                    blg.OzelKod9 = f.RTS(r, "OzelKod9").ToString();
                    blg.Personel = f.RTS(r, "Personel").ToString();
                    //blg.//Depozitolu = f.RTS(r, "Depozitolu"),
                    //blg.OtvToplam = f.RTI(r, "OtvToplam"),
                    blg.OdemeTutar = f.RTD(r, "OdemeTutar");
                    blg.SonBakiye = f.RTI(r, "SonBakiye");
                    blg.BelgeOzelKod1 = f.RTS(r, "BelgeOzelKod1");
                    blg.BelgeOzelKod2 = f.RTS(r, "BelgeOzelKod2");
                    blg.BelgeOzelKod3 = f.RTS(r, "BelgeOzelKod3");
                    blg.BelgeOzelKod4 = f.RTS(r, "BelgeOzelKod4");
                    blg.OnayDurumu = f.RTI(r, "OnayDurumu");
                    blg.OnaylayanId = f.RTI(r, "OnaylayanId");
                    blg.Sube = f.RTS(r, "Sube");
                    blg.BelgeNoArctos = f.RTS(r, "BelgeNoArctos").ToString();
                    blglst.Belgeler.Add(blg);



                }
                f.SqlConnClose();
            }
            catch (System.Exception ex) { }
            return blglst;
        }
        public BelgeList BelgeSablonList(BelgeTipi blgtp)
        {
            BelgeList blglst = new BelgeList();
            blglst.Belgeler = new List<BelgeAlisGiderCreate>();
            ModelFunctions f = new ModelFunctions();
            try
            {
                f.SqlConnOpen();
                DataTable dt = f.DataTable("SELECT [Id],[Cari],[BelgeNo],[BelgeKod],[Tarih],[BelgeTutar],[AraToplam],[BelgeKDV],[NetToplam],[BelgeTip],[IskontoTop],[AIsk1],[AIsk2],[AIsk3],[AIsk4],[Vade],[Termin],[Parabirimi],[Kur],[VgFatInd],[Terminal],[Aktarim],[Kapali],[BelgeNot],[PesinAd],[PesinVergiNo],[OzelKod],[Depo],[CariExtId],[KayitTarihi],[CikanDepo],[UID],[OzelKod9],[Personel],[Depozitolu],[OtvToplam],[OdemeTutar],[SonBakiye],[BelgeOzelKod1],[BelgeOzelKod2],[BelgeOzelKod3],[BelgeOzelKod4],[OnayDurumu],[OnaylayanId],[SablonAdi]  FROM  Belge where [BelgeTip]=" + ((int)blgtp).ToString());
                foreach (DataRow r in dt.Rows)
                {

                    BelgeAlisGiderCreate blg = new BelgeAlisGiderCreate();
                    blg.Id = f.RTI(r, "Id");
                    blg.Cari = f.RTS(r, "Cari").ToString();
                    blg.BelgeNo = f.RTS(r, "BelgeNo").ToString();
                    blg.BelgeKod = f.RTS(r, "BelgeKod").ToString();
                    blg.Tarih = Convert.ToDateTime(f.RTS(r, "Tarih").ToString());
                    blg.BelgeTutar = f.RTI(r, "BelgeTutar");
                    blg.AraToplam = f.RTI(r, "AraToplam");
                    blg.BelgeKDV = f.RTI(r, "BelgeKDV");
                    blg.NetToplam = f.RTI(r, "NetToplam");
                    blg.BelgeTip = ((Enums.General.BelgeTipi)f.RTI(r, "BelgeTip"));
                    blg.IskontoTop = f.RTI(r, "IskontoTop");
                    blg.AIsk1 = f.RTD(r, "AIsk1");
                    blg.AIsk2 = f.RTD(r, "AIsk2");
                    blg.AIsk3 = f.RTI(r, "AIsk3");
                    blg.AIsk4 = f.RTI(r, "AIsk4");
                    //blg.//Vade = f.RTI(r, "Vade"),
                    //blg.//Termin = f.RTI(r, "Termin"),
                    //blg.Parabirimi = f.RTS(r, "Parabirimi").ToString(),
                    blg.Kur = f.RTI(r, "Kur");
                    blg.VgFatInd = f.RTI(r, "VgFatInd");
                    blg.Terminal = f.RTI(r, "Terminal");
                    blg.Aktarim = f.RTI(r, "Aktarim");
                    blg.Kapali = f.RTI(r, "Kapali");
                    blg.BelgeNot = f.RTS(r, "BelgeNot").ToString();
                    blg.PesinAd = f.RTS(r, "PesinAd").ToString();
                    blg.PesinVergiNo = f.RTS(r, "PesinVergiNo").ToString();
                    blg.OzelKod = f.RTS(r, "OzelKod").ToString();
                    blg.Depo = f.RTS(r, "Depo").ToString();
                    blg.CariExtId = f.RTI(r, "CariExtId");
                    blg.KayitTarihi = Convert.ToDateTime(f.RTS(r, "KayitTarihi"));
                    blg.CikanDepo = f.RTS(r, "CikanDepo");
                    blg.UID = f.RTS(r, "UID").ToString();
                    blg.OzelKod9 = f.RTS(r, "OzelKod9").ToString();
                    blg.Personel = f.RTS(r, "Personel").ToString();
                    //blg.//Depozitolu = f.RTS(r, "Depozitolu"),
                    //blg.OtvToplam = f.RTI(r, "OtvToplam"),
                    blg.OdemeTutar = f.RTD(r, "OdemeTutar");
                    blg.SonBakiye = f.RTI(r, "SonBakiye");
                    blg.BelgeOzelKod1 = f.RTS(r, "BelgeOzelKod1");
                    blg.BelgeOzelKod2 = f.RTS(r, "BelgeOzelKod2");
                    blg.BelgeOzelKod3 = f.RTS(r, "BelgeOzelKod3");
                    blg.BelgeOzelKod4 = f.RTS(r, "BelgeOzelKod4");
                    blg.OnayDurumu = f.RTI(r, "OnayDurumu");
                    blg.OnaylayanId = f.RTI(r, "OnaylayanId");
                    blg.SablonAdi = f.RTS(r, "SablonAdi");
                    blglst.Belgeler.Add(blg);



                }
                f.SqlConnClose();
            }
            catch (System.Exception ex) { }
            return blglst;
        }

        public BelgeList OnayBekleyenBelgeList(string BelgeTipYetkisi, string BaslangicTarihi = "", string BitisTarihi = "", string BelgeTipi = "", string Sube = "")
        {
            BelgeList blglst = new BelgeList();
            blglst.Belgeler = new List<BelgeAlisGiderCreate>();
            ModelFunctions f = new ModelFunctions();
            try
            {
                f.SqlConnOpen();
                string queryPlus = "";
                if (!string.IsNullOrEmpty(BaslangicTarihi))
                {
                    queryPlus += "and Belge.Tarih>='" + BaslangicTarihi + "'";
                }
                if (!string.IsNullOrEmpty(BitisTarihi))
                {
                    queryPlus += "and Belge.Tarih<='" + BitisTarihi + "'";
                }
                if (!string.IsNullOrEmpty(BelgeTipi) && BelgeTipi != "0")
                {
                    queryPlus += "and Belge.BelgeTip=" + BelgeTipi;
                }
                if (!string.IsNullOrEmpty(Sube) && Sube != "0")
                {
                    queryPlus += "and Belge.Sube=" + Sube;
                }

                DataTable dt = f.DataTable("SELECT Belge.[Id],Belge.[Cari],Belge.[BelgeNo],Belge.[BelgeKod],Belge.[Tarih],Belge.[BelgeTutar],Belge.[AraToplam],Belge.[BelgeKDV],Belge.[NetToplam],Belge.[BelgeTip],Belge.[IskontoTop],Belge.[AIsk1],Belge.[AIsk2],Belge.[AIsk3],Belge.[AIsk4],Belge.[Vade],Belge.[Termin],Belge.[Parabirimi],Belge.[Kur],Belge.[VgFatInd],Belge.[Terminal],Belge.[Aktarim],Belge.[Kapali],Belge.[BelgeNot],Belge.[PesinAd],Belge.[PesinVergiNo],Belge.[OzelKod],Belge.[Depo],Belge.[CariExtId],Belge.[KayitTarihi],Belge.[CikanDepo],Belge.[UID],Belge.[OzelKod9],Belge.[Personel],Belge.[Depozitolu],Belge.[OtvToplam],Belge.[OdemeTutar],Belge.[SonBakiye],Belge.[BelgeOzelKod1],Belge.[BelgeOzelKod2],Belge.[BelgeOzelKod3],Belge.[BelgeOzelKod4],Belge.[BelgeNoArctos],(select top 1 SubeSettings.SubeName from SubeSettings where SubeSettings.FasterSubeID=Belge.Sube) as Sube  FROM  Belge " +
                    "where OnayDurumu=0 and BelgeTip in(" + BelgeTipYetkisi + ")" + queryPlus);
                foreach (DataRow r in dt.Rows)
                {

                    BelgeAlisGiderCreate blg = new BelgeAlisGiderCreate();
                    blg.Id = f.RTI(r, "Id");
                    blg.Cari = f.RTS(r, "Cari").ToString();
                    blg.BelgeNo = f.RTS(r, "BelgeNo").ToString();
                    blg.BelgeKod = f.RTS(r, "BelgeKod").ToString();
                    blg.Tarih = Convert.ToDateTime(f.RTS(r, "Tarih").ToString());
                    blg.BelgeTutar = f.RTD(r, "BelgeTutar");
                    blg.AraToplam = f.RTI(r, "AraToplam");
                    blg.BelgeKDV = f.RTI(r, "BelgeKDV");
                    blg.NetToplam = f.RTI(r, "NetToplam");
                    blg.BelgeTip = ((Enums.General.BelgeTipi)f.RTI(r, "BelgeTip"));
                    blg.IskontoTop = f.RTI(r, "IskontoTop");
                    blg.AIsk1 = f.RTD(r, "AIsk1");
                    blg.AIsk2 = f.RTD(r, "AIsk2");
                    blg.AIsk3 = f.RTI(r, "AIsk3");
                    blg.AIsk4 = f.RTI(r, "AIsk4");
                    //blg.//Vade = f.RTI(r, "Vade"),
                    //blg.//Termin = f.RTI(r, "Termin"),
                    //blg.Parabirimi = f.RTS(r, "Parabirimi").ToString(),
                    blg.Kur = f.RTI(r, "Kur");
                    blg.VgFatInd = f.RTI(r, "VgFatInd");
                    blg.Terminal = f.RTI(r, "Terminal");
                    blg.Aktarim = f.RTI(r, "Aktarim");
                    blg.Kapali = f.RTI(r, "Kapali");
                    blg.BelgeNot = f.RTS(r, "BelgeNot").ToString();
                    blg.PesinAd = f.RTS(r, "PesinAd").ToString();
                    blg.PesinVergiNo = f.RTS(r, "PesinVergiNo").ToString();
                    blg.OzelKod = f.RTS(r, "OzelKod").ToString();
                    blg.Depo = f.RTS(r, "Depo").ToString();
                    blg.CariExtId = f.RTI(r, "CariExtId");
                    blg.KayitTarihi = Convert.ToDateTime(f.RTS(r, "KayitTarihi"));
                    blg.CikanDepo = f.RTS(r, "CikanDepo");
                    blg.UID = f.RTS(r, "UID").ToString();
                    blg.OzelKod9 = f.RTS(r, "OzelKod9").ToString();
                    blg.Personel = f.RTS(r, "Personel").ToString();
                    //blg.//Depozitolu = f.RTS(r, "Depozitolu"),
                    //blg.OtvToplam = f.RTI(r, "OtvToplam"),
                    blg.OdemeTutar = f.RTD(r, "OdemeTutar");
                    blg.SonBakiye = f.RTI(r, "SonBakiye");
                    blg.BelgeOzelKod1 = f.RTS(r, "BelgeOzelKod1");
                    blg.BelgeOzelKod2 = f.RTS(r, "BelgeOzelKod2");
                    blg.BelgeOzelKod3 = f.RTS(r, "BelgeOzelKod3");
                    blg.BelgeOzelKod4 = f.RTS(r, "BelgeOzelKod4");
                    blg.BelgeNoArctos = f.RTS(r, "BelgeNoArctos").ToString();
                    blg.Sube = f.RTS(r, "Sube");

                    blglst.Belgeler.Add(blg);





                }
                f.SqlConnClose();
            }
            catch (System.Exception ex) { }
            return blglst;
        }
        public BelgeAlisGiderCreate GetBelge(string ID)
        {
            BelgeAlisGiderCreate blg = new BelgeAlisGiderCreate();
            ModelFunctions f = new ModelFunctions();
            try
            {
                f.SqlConnOpen();
                DataTable dt = f.DataTable("SELECT [Id],[Cari],[BelgeNo],[BelgeKod],[Tarih],[BelgeTutar],[AraToplam],[BelgeKDV],[NetToplam],[BelgeTip],[IskontoTop],[AIsk1],[AIsk2],[AIsk3],[AIsk4],[Vade],[Termin],[Parabirimi],[Kur],[VgFatInd],[Terminal],[Aktarim],[Kapali],[BelgeNot],[PesinAd],[PesinVergiNo],[OzelKod],[Depo],[CariExtId],[KayitTarihi],[CikanDepo],[UID],[OzelKod9],[Personel],[Depozitolu],[OtvToplam],[OdemeTutar],[SonBakiye],[BelgeOzelKod1],[BelgeOzelKod2],[BelgeOzelKod3],[BelgeOzelKod4],[OnayDurumu],[Sube],[Kasa],[SablonAdi],[Sablon],[BelgeNoArctos],[Kdv],[AltCari]  FROM  Belge where Id=" + ID);
                foreach (DataRow r in dt.Rows)
                {
                    blg.Id = f.RTI(r, "Id");
                    blg.Cari = f.RTS(r, "Cari").ToString();
                    blg.BelgeNo = f.RTS(r, "BelgeNo").ToString();
                    blg.BelgeKod = f.RTS(r, "BelgeKod").ToString();
                    blg.Tarih = Convert.ToDateTime(f.RTS(r, "Tarih").ToString());
                    blg.BelgeTutar = f.RTI(r, "BelgeTutar");
                    blg.AraToplam = f.RTI(r, "AraToplam");
                    blg.BelgeKDV = f.RTI(r, "BelgeKDV");
                    blg.NetToplam = f.RTI(r, "NetToplam");
                    blg.BelgeTip = ((Enums.General.BelgeTipi)f.RTI(r, "BelgeTip"));
                    blg.IskontoTop = f.RTI(r, "IskontoTop");
                    blg.AIsk1 = f.RTD(r, "AIsk1");
                    blg.AIsk2 = f.RTD(r, "AIsk2");
                    blg.AIsk3 = f.RTI(r, "AIsk3");
                    blg.AIsk4 = f.RTI(r, "AIsk4");
                    //blg.//Vade = f.RTI(r, "Vade"),
                    //blg.//Termin = f.RTI(r, "Termin"),
                    //blg.Parabirimi = f.RTS(r, "Parabirimi").ToString(),
                    blg.Kur = f.RTI(r, "Kur");
                    blg.VgFatInd = f.RTI(r, "VgFatInd");
                    blg.Terminal = f.RTI(r, "Terminal");
                    blg.Aktarim = f.RTI(r, "Aktarim");
                    blg.Kapali = f.RTI(r, "Kapali");
                    blg.BelgeNot = f.RTS(r, "BelgeNot").ToString();
                    blg.PesinAd = f.RTS(r, "PesinAd").ToString();
                    blg.PesinVergiNo = f.RTS(r, "PesinVergiNo").ToString();
                    blg.OzelKod = f.RTS(r, "OzelKod").ToString();
                    blg.Depo = f.RTS(r, "Depo").ToString();
                    blg.CariExtId = f.RTI(r, "CariExtId");
                    blg.KayitTarihi = Convert.ToDateTime(f.RTS(r, "KayitTarihi"));
                    blg.CikanDepo = f.RTS(r, "CikanDepo");
                    blg.UID = f.RTS(r, "UID").ToString();
                    blg.OzelKod9 = f.RTS(r, "OzelKod9").ToString();
                    blg.Personel = f.RTS(r, "Personel").ToString();
                    //blg.//Depozitolu = f.RTS(r, "Depozitolu"),
                    //blg.OtvToplam = f.RTI(r, "OtvToplam"),
                    blg.OdemeTutar = f.RTD(r, "OdemeTutar");
                    blg.SonBakiye = f.RTI(r, "SonBakiye");
                    blg.BelgeOzelKod1 = f.RTS(r, "BelgeOzelKod1").ToString();
                    blg.BelgeOzelKod2 = f.RTS(r, "BelgeOzelKod2").ToString();
                    blg.BelgeOzelKod3 = f.RTS(r, "BelgeOzelKod3").ToString();
                    blg.BelgeOzelKod4 = f.RTS(r, "BelgeOzelKod4").ToString();
                    blg.OnayDurumu = f.RTI(r, "OnayDurumu");
                    blg.Sube = f.RTS(r, "Sube");
                    blg.Kasa = f.RTS(r, "Kasa");
                    blg.SablonAdi = f.RTS(r, "SablonAdi");
                    blg.Sablon = f.RTS(r, "Sablon");
                    blg.BelgeNoArctos = f.RTS(r, "BelgeNoArctos").ToString();
                    blg.AltCari = f.RTS(r, "AltCari").ToString();
                    if (!string.IsNullOrEmpty(f.RTS(r, "Kdv")))
                        blg.Kdv = Convert.ToBoolean(f.RTS(r, "Kdv").ToString());
                }

                blg.BelgeHarekets = new List<BelgeHareket>();
                DataTable dtdetay = f.DataTable("SELECT [Id],[Belge],[Stok],[Barcode],[Miktar],[Fiyat],[Tutar],[Isk1],[Isk2],[Isk3],[Isk4],[AltIskonto],[IskToplam],[SatirToplam],[VgFatHarInd],[KDV],[KDVTutar],[StokExtId],[SatirSayisi],[Parabirimi],[Kur],[Aciklama] ,[RBMiktar],[MiktarBos] ,[Kampanya],[Otv],[OtvTutar],[MalinCinsi],[Birim],[SayimTarihDahil],[ZayiMiktar] FROM  BelgeHareket where Belge=" + ID);
                foreach (DataRow r in dtdetay.Rows)
                {
                    BelgeHareket blghrk = new BelgeHareket();
                    blghrk.Id = f.RTI(r, "Id");
                    blghrk.Belge = f.RTI(r, "Belge");
                    blghrk.Stok = f.RTS(r, "Stok");
                    blghrk.Barcode = f.RTS(r, "Barcode");
                    blghrk.Miktar = f.RTD(r, "Miktar");
                    blghrk.Fiyat = f.RTD(r, "Fiyat");
                    blghrk.Tutar = f.RTD(r, "Tutar");
                    blghrk.Isk1 = f.RTD(r, "Isk1");
                    blghrk.Isk2 = f.RTD(r, "Isk2");
                    blghrk.Isk3 = f.RTD(r, "Isk3");
                    blghrk.Isk4 = f.RTD(r, "Isk4");
                    blghrk.AltIskonto = f.RTD(r, "AltIskonto");
                    blghrk.IskToplam = f.RTD(r, "IskToplam");
                    blghrk.SatirToplam = f.RTD(r, "SatirToplam");
                    blghrk.VgFatHarInd = f.RTI(r, "VgFatHarInd");
                    blghrk.KDV = f.RTD(r, "KDV");
                    blghrk.KDVTutar = f.RTD(r, "KDVTutar");
                    blghrk.StokExtId = f.RTI(r, "StokExtId");
                    blghrk.SatirSayisi = f.RTI(r, "SatirSayisi");
                    blghrk.Parabirimi = f.RTD(r, "Parabirimi");
                    blghrk.Kur = f.RTD(r, "Kur");
                    blghrk.Aciklama = f.RTS(r, "Aciklama");
                    blghrk.RBMiktar = f.RTS(r, "RBMiktar");
                    blghrk.MiktarBos = f.RTD(r, "MiktarBos");
                    blghrk.Kampanya = f.RTD(r, "Kampanya");
                    blghrk.Otv = f.RTD(r, "Otv");
                    blghrk.OtvTutar = f.RTD(r, "OtvTutar");
                    blghrk.MalinCinsi = f.RTS(r, "MalinCinsi");
                    blghrk.Birim = f.RTS(r, "Birim");
                    if (!string.IsNullOrEmpty(f.RTS(r, "SayimTarihDahil")))
                        blghrk.SayimTarihDahil = Convert.ToBoolean(f.RTS(r, "SayimTarihDahil"));
                    blghrk.ZayiMiktar = f.RTD(r, "ZayiMiktar");
                    blg.BelgeHarekets.Add(blghrk);
                }
                f.SqlConnClose();
            }
            catch (System.Exception ex) { }
            return blg;
        }
        public List<SelectListItem> GetAltCari(string CariId, string TanimId)
        {
            List<SelectListItem> items = new List<SelectListItem>();
            ModelFunctions f = new ModelFunctions();
            try
            {
                f.SqlConnOpen();
                DataTable dt = f.DataTable("SELECT Id,DBName,IP,SqlName,SqlPassword  FROM  VegaDbSettings");
                string Query = "";
                foreach (DataRow r in dt.Rows)
                {
                    string VegaDbId = f.RTS(r, "Id");
                    string VegaDbName = f.RTS(r, "DBName");
                    string VegaDbIp = f.RTS(r, "IP");
                    string VegaDbSqlName = f.RTS(r, "SqlName");
                    string VegaDbSqlPassword = f.RTS(r, "SqlPassword");
                    Query = "SELECT IND,FIRMAKODU,FIRMAADI, TELNO1,EFATURAALIAS,VERGINO,TCKIMLIKNO,MUSTERITEMSILCISINO FROM F0" + TanimId + "TBLCRMALTKARTLAR  WHERE MUSTERINO = (SELECT IND FROM F0" + TanimId + "TBLCRMMUSTERIKARTI WHERE CARIREF=" + CariId + ")";
                    string connString = "Provider=SQLOLEDB;Server=" + VegaDbIp + ";User Id=" + VegaDbSqlName + ";Password=" + VegaDbSqlPassword + ";Database=" + VegaDbName + "";
                    f.SqlConnOpen(true, connString);
                    DataTable AcikHesapDt = f.DataTable(Query, true);
                    if (AcikHesapDt.Rows.Count > 0)
                    {
                        foreach (DataRow SubeR in AcikHesapDt.Rows)
                        {
                            BelgeAlisGiderCreate model = new BelgeAlisGiderCreate();
                            items.Add(new SelectListItem
                            {
                                Text = f.RTS(SubeR, "FIRMAKODU").ToString() + " - " + f.RTS(SubeR, "FIRMAADI").ToString(),
                                Value = f.RTS(SubeR, "FIRMAKODU"),
                            });
                        }
                    }
                }
                f.SqlConnClose();
            }
            catch (System.Exception ex) { }
            return items.ToList();
        }
        public string GetArctosBelgeAndCreate(string BelgeNo, UserViewModel usModel)
        {
            var sqlData = new SqlData(new SqlConnection(modelFunctions.NewConnectionString(subeIp, dbName, sqlKullaniciName, sqlKullaniciPassword)));
            string TimeStamp = DateTime.Now.ToFileTimeUtc().ToString();

            //artık onaylı vegadb yaz
            ModelFunctions f = new ModelFunctions();
            f.SqlConnOpen();
            DataTable dt = f.DataTable("SELECT Id,DBName,IP,SqlName,SqlPassword  FROM  VegaDbSettings");
            string VegaDbId;
            string VegaDbName;
            string VegaDbIp;
            string VegaDbSqlName;
            string VegaDbSqlPassword;
            string connString = "";
            foreach (DataRow r in dt.Rows)
            {
                VegaDbId = f.RTS(r, "Id");
                VegaDbName = f.RTS(r, "DBName");
                VegaDbIp = f.RTS(r, "IP");
                VegaDbSqlName = f.RTS(r, "SqlName");
                VegaDbSqlPassword = f.RTS(r, "SqlPassword");

                connString = "Server=" + VegaDbIp + ";User Id=" + VegaDbSqlName + ";Password=" + VegaDbSqlPassword + ";Database=" + VegaDbName + "";

            }
            string BlgNo = "";
            using (VegaBelgeWrapper vegaBelgeWrapper = new VegaBelgeWrapper(connString, "F0" + usModel.FirmaID.ToString(), "D" + usModel.DonemID.ToString().PadLeft(4, '0'), 100))//UserId standart 100 basılsın istendi. Burak Yıkmaz
            {
                VegaBelgeBaslik blg = vegaBelgeWrapper.GetBelgeForDepoTransfer(BelgeNo);
                BelgeAlisGiderCreate blgalis = new BelgeAlisGiderCreate();

                List<SelectListItem> itemssub = new List<SelectListItem>();
                List<SelectListItem> itemskasa = new List<SelectListItem>();
                itemssub = AlisBelgesiCRUD.GetSubeList(usModel.FirmaID, usModel.FasterSubeID);//generic alınmalı
                List<SelectListItem> selectedsube = itemssub.Where(a => a.Text == blg.OzelKod1).ToList();
                if (selectedsube.Count > 0)
                {
                    blgalis.Sube = selectedsube[0].Value;//blg.Sube;
                }

                itemskasa = AlisBelgesiCRUD.GetKasaList(usModel.FirmaID);//, ((int)blg.BelgeTip), usModel.SefimPanelZimmetCagrisi);//generic alınmalı
                List<SelectListItem> selectedkasa = itemskasa.Where(a => a.Text == blg.OzelKod2).ToList();
                if (selectedkasa.Count > 0)
                {
                    blgalis.Kasa = selectedkasa[0].Value; //blg.Cari;
                }
                blgalis.Personel = blg.OzelKod3;
                blgalis.BelgeOzelKod1 = blg.OzelKod6;
                blgalis.BelgeOzelKod2 = blg.OzelKod7;
                blgalis.BelgeOzelKod3 = blg.OzelKod8;
                blgalis.BelgeOzelKod4 = blg.OzelKod9;
                blgalis.OzelKod = blg.OzelKod5;

                blgalis.Cari = 0.ToString();
                blgalis.AraToplam = 0;
                blgalis.BelgeKDV = 0;
                blgalis.BelgeKod = "";
                blgalis.BelgeNo = blg.BaslikInd.ToString();
                blgalis.BelgeNoArctos = blg.BelgeNo;
                blgalis.BelgeTip = BelgeTipi.DepoTransferKabul;
                blgalis.BelgeTutar = 0;
                blgalis.Cari = 1.ToString();
                blgalis.BelgeNot = blg.AltNot;
                blgalis.Depo = blg.DepoNo.ToString();
                blgalis.CikanDepo = blg.HareketDeposu.ToString();
                blgalis.Kur = 1;
                blgalis.NetToplam = 0;
                blgalis.OdemeTutar = 0;
                blgalis.OtvToplam = 0;
                blgalis.PesinAd = "";
                blgalis.PesinVergiNo = "";
                blgalis.Sablon = "";
                blgalis.SablonAdi = "";
                blgalis.SonBakiye = 0;
                blgalis.Tarih = blg.Tarih;
                blgalis.Terminal = 0;
                blgalis.VgFatInd = 0;

                blgalis.BelgeHarekets = new List<BelgeHareket>();
                List<VegaBelgeHareket> harekets = vegaBelgeWrapper.GetBelgeHareketsForDepoTransfer(blgalis.BelgeNo);

                foreach (var vhar in harekets)
                {
                    BelgeHareket blghar = new BelgeHareket();
                    blghar.Aciklama = vhar.Aciklama;
                    blghar.Stok = vhar.StokNo.ToString();
                    blghar.Miktar = vhar.Miktar;
                    blghar.KDV = vhar.Kdv;
                    blghar.Envanter = vhar.Envanter;
                    List<SefimPanelStok> lst = GetStokDetayList(usModel.FirmaID, Convert.ToInt32(blghar.Stok), 1);
                    if (lst.Count > 0)
                    {
                        blghar.Birim = lst[0].BirimAdi;
                        blghar.MalinCinsi = lst[0].MalinCinsi;
                    }
                    blgalis.BelgeHarekets.Add(blghar);
                }
                // blgalis.BelgeHarekets = blg.BelgeHarekets;
                BlgNo = ((BelgeAlisGiderCreate)Insert(blgalis, usModel).result_OBJECT).Id.ToString();
            }
            return BlgNo;
        }
    }
}