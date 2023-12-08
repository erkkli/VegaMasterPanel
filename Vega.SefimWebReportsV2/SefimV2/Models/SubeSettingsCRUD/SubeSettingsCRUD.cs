using MongoDB.Bson;
using MongoDB.Driver;
using SefimV2.Helper;
using SefimV2.ViewModels.SubeSettings;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;

namespace SefimV2.Models
{
    public class SubeSettingsCRUD
    {
        public static List<SubeSettingsViewModel> List()
        {
            var list = new List<SubeSettingsViewModel>();
            ModelFunctions mf = new ModelFunctions();

            try
            {
                mf.SqlConnOpen();
                DataTable dt = mf.DataTable("select * from SubeSettings");
                foreach (DataRow r in dt.Rows)
                {
                    SubeSettingsViewModel model = new SubeSettingsViewModel
                    {
                        ID = Convert.ToInt32(mf.RTS(r, "Id")),
                        SubeName = (mf.RTS(r, "SubeName")),
                        SubeIP = mf.RTS(r, "SubeIP"),
                        SqlName = mf.RTS(r, "SqlName"),
                        SqlPassword = mf.RTS(r, "SqlPassword"),
                        DBName = mf.RTS(r, "DBName"),
                        FirmaID = mf.RTS(r, "FirmaID"),
                        DepoID = mf.RTS(r, "DepoID"),
                        DonemID = mf.RTS(r, "DonemID"),
                        Status = Convert.ToBoolean(mf.RTS(r, "Status")),
                        AppDbType = Convert.ToInt32(mf.RTS(r, "AppDbType")),
                        AppDbTypeStatus = Convert.ToBoolean(mf.RTS(r, "AppDbTypeStatus"))
                    };

                    if (mf.RTS(r, "BelgeSayimTarihDahil") != "")
                    {
                        model.BelgeSayimTarihDahil = Convert.ToBoolean(mf.RTS(r, "BelgeSayimTarihDahil"));
                    }
                    //model.UrunEslestirmeVarMi = Convert.ToBoolean(f.RTS(r, "UrunEslestirmeVarMi"));

                    #region ENVANTER DB BAGLANIP GEREKLİ BILGILER ALIIYOR 
                    try
                    {
                        mf.SqlConnOpen();
                        DataTable dt_e = mf.DataTable("SELECT Id,DBName,IP,SqlName,SqlPassword  FROM  VegaDbSettings");
                        string Query = string.Empty;

                        foreach (DataRow Envanter in dt_e.Rows)
                        {
                            string VegaDbId = mf.RTS(Envanter, "Id");
                            string VegaDbName = mf.RTS(Envanter, "DBName");
                            string VegaDbIp = mf.RTS(Envanter, "IP");
                            string VegaDbSqlName = mf.RTS(Envanter, "SqlName");
                            string VegaDbSqlPassword = mf.RTS(Envanter, "SqlPassword");

                            #region ENVANTER'DEN FİRMA, DÖNEM ve DEPOADI'NI  getiriyor                           
                            for (int i = 0; i < 3; i++)
                            {
                                if (i == 0)
                                {
                                    Query = "SELECT IND, KOD, KISAAD FROM TBLFIRMA WHERE IND=" + model.FirmaID + "";
                                    string connString = "Provider=SQLOLEDB;Server=" + VegaDbIp + ";User Id=" + VegaDbSqlName + ";Password=" + VegaDbSqlPassword + ";Database=" + VegaDbName + "";
                                    mf.SqlConnOpen(true, connString);
                                    DataTable EnvanterDt = mf.DataTable(Query, true);
                                    if (EnvanterDt.Rows.Count > 0)
                                    {
                                        foreach (DataRow EnvanterR in EnvanterDt.Rows)
                                        {
                                            model.FirmaID = mf.RTS(EnvanterR, "KOD");
                                        }
                                    }
                                }
                                else if (i == 1)
                                {
                                    Query = "SELECT FIND, IND, DONEM FROM TBLDONEM   WHERE IND =" + model.DonemID + "";
                                    string connString = "Provider=SQLOLEDB;Server=" + VegaDbIp + ";User Id=" + VegaDbSqlName + ";Password=" + VegaDbSqlPassword + ";Database=" + VegaDbName + "";
                                    mf.SqlConnOpen(true, connString);
                                    DataTable EnvanterDt = mf.DataTable(Query, true);
                                    if (EnvanterDt.Rows.Count > 0)
                                    {
                                        foreach (DataRow EnvanterR in EnvanterDt.Rows)
                                        {
                                            model.DonemID = mf.RTS(EnvanterR, "DONEM");
                                        }
                                    }
                                }
                                else if (i == 2)
                                {
                                    Query = "SELECT IND, DEPOADI, DEPOKODU FROM F0100TBLDEPOLAR WHERE IND=" + model.DepoID + "";
                                    string connString = "Provider=SQLOLEDB;Server=" + VegaDbIp + ";User Id=" + VegaDbSqlName + ";Password=" + VegaDbSqlPassword + ";Database=" + VegaDbName + "";
                                    mf.SqlConnOpen(true, connString);
                                    DataTable EnvanterDt = mf.DataTable(Query, true);
                                    if (EnvanterDt.Rows.Count > 0)
                                    {
                                        foreach (DataRow EnvanterR in EnvanterDt.Rows)
                                        {
                                            model.DepoID = mf.RTS(EnvanterR, "DEPOADI");
                                        }
                                    }
                                }
                            }
                            #endregion
                        }
                        mf.SqlConnClose();
                    }
                    catch (Exception ex)
                    {
                        Singleton.WritingLogFile2("SubeSettingsCRUDListEnvanter:", ex.Message.ToString(), "", ex.StackTrace);
                    }
                    #endregion

                    list.Add(model);
                }

                mf.SqlConnClose();
            }
            catch (Exception ex)
            {
                Singleton.WritingLogFile2("SubeSettingsCRUDList:", ex.Message.ToString(), "", ex.StackTrace);
            }

            return list;
        }

        public ActionResultMessages SubeSettingsInsert(SubeSettingsViewModel course)
        {
            ModelFunctions mf = new ModelFunctions();
            ActionResultMessages result = new ActionResultMessages
            {
                result_STR = "Başarısız",
                result_INT = 0,
                result_BOOL = false,
                result_OBJECT = null
            };

            string resut = "true";

            try
            {
                mf.SqlConnOpen();
                string TimeStamp = DateTime.Now.ToFileTimeUtc().ToString();
                string CmdString = "Insert Into SubeSettings(CreateDate,CreateDate_Timestamp,SubeName,SubeIP,SqlName,SqlPassword,DBName,FirmaID,DonemID,DepoID,AppDbType,AppDbTypeStatus,FasterSubeID, SefimPanelZimmetCagrisi,Status,ServiceAdress,BelgeSayimTarihDahil,PersonelYemekRaporuAdi, VPosSubeKodu,VPosKasaKodu,UrunEslestirmeVarMi)" +
                                   "Values(" +
                                   "getdate() , " +
                                   "'" + TimeStamp + "' , " +
                                   "'" + course.SubeName + "' , " +
                                   "'" + course.SubeIP + "' , " +
                                   "'" + course.SqlName + "' , " +
                                   "'" + course.SqlPassword + "' , " +
                                   "'" + course.DBName + "' , " +
                                   "'" + course.FirmaID + "' , " +
                                   "'" + course.DonemID + "' , " +
                                   "'" + course.DepoID + "' , " +
                                   "'" + course.AppDbType + "' , " +
                                   "'" + course.AppDbTypeStatus + "' , " +
                                   "'" + course.FasterKasaListesi + "' , " +
                                   "'" + course.ZimmetCariInd + "' , " +
                                   "'" + course.Status + "', " +
                                   "'" + course.ServiceAdress + "', " +
                                   "'" + course.BelgeSayimTarihDahil + "',  " +
                                   "'" + course.PersonelYemekRaporu + "' , " +
                                   "'" + course.VPosSubeKodu + "' , " +
                                   "'" + course.VPosKasaKodu + "' , " +
                                   "'" + course.UrunEslestirmeVarMi + "'  " +
                                   ")" +
                                    "select CAST(scope_identity() AS int);";
                OleDbCommand Cmd = new OleDbCommand(CmdString, mf.ConnOle);
                int Id = (int)Cmd.ExecuteScalar();
                mf.SqlConnClose();

                try
                {
                    if (course.AppDbType == 5)
                    {
                        string connString = "Provider=SQLOLEDB;Server=" + course.SubeIP + ";User Id=" + course.SqlName + ";Password=" + course.SqlPassword + ";Database=" + course.DBName + "";
                        mf.SqlConnOpen(true, connString);
                        mf.DataTable(@"Update [TBLSPOSKASALAR] set ApiUrl = '" + course.ServiceAdress + "' ", true);
                        mf.DataTable(@"Update [TBLSPOSSUBELER] set ApiUrl = '" + course.ServiceAdress + "' ", true);
                        mf.SqlConnClose();
                    }
                }
                catch (Exception ex)
                {
                    Singleton.WritingLogFile2("SubeSettingsCRUD_Update_TBLSPOSKASALAR:", ex.Message.ToString(), "", ex.StackTrace);
                }


                try
                {
                    if (!string.IsNullOrWhiteSpace(mf.MongoConnString))
                    {
                        MongoClient client = new MongoClient(mf.MongoConnString);
                        MongoServer server = client.GetServer();
                        MongoDatabase db = server.GetDatabase(mf.MongoDBName); //veritabanına bağlantı sağlanıyor
                        var branches = db.GetCollection("branches");
                        var sorgu = new QueryDocument { { "externalId", Id } };
                        var sorgu2 = new QueryDocument { { "externalId", Id.ToString() } };
                        BsonDocument bs = branches.FindOne(sorgu);
                        BsonDocument bs2 = branches.FindOne(sorgu2);
                        if (bs == null && bs2 == null)
                        {
                            //Ekleme işlemi yapılıyor
                            branches.Insert(new BsonDocument
                        {
                            {"remoteApiUrl",course.ServiceAdress??""},//http://192.168.0.217:8081
                            {"serviceName",course.SubeName},
                            {"externalId", Id},
                            {"database", course.DBName},
                            {"name", course.SubeName},
                            {"updatedAt", DateTime.Now.ToUniversalTime()}
                        });
                        }
                        else
                        {
                            var guncelle = new UpdateDocument { { "$set", new BsonDocument{
                            {"remoteApiUrl", course.ServiceAdress??""},
                            {"serviceName",course.SubeName},
                            {"externalId", course.ID},
                            {"database", course.DBName},
                            {"name", course.SubeName},
                            {"updatedAt", DateTime.Now.ToUniversalTime()}
                        } } };
                            if (bs != null)
                            {
                                //Güncelleme işlemi yapılıyor
                                branches.Update(sorgu, guncelle);
                            }
                            if (bs2 != null)
                            {
                                //Güncelleme işlemi yapılıyor
                                branches.Update(sorgu2, guncelle);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Singleton.WritingLogFile2("SubeSettingsCRUDSubeSettingsInsert_MongoServer:", ex.Message.ToString(), "", ex.StackTrace);
                    resut = ex.Message.ToString(); result.IsSuccess = false; result.UserMessage = ex.ToString(); 
                    //return result;
                }


            }
            catch (OleDbException ex)
            {
                Singleton.WritingLogFile2("SubeSettingsCRUDSubeSettingsInsert:", ex.Message.ToString(), "", ex.StackTrace);
                resut = ex.Message.ToString(); result.IsSuccess = false; result.UserMessage = ex.ToString(); return result;
            }

            result.result_STR = "Başarılı";
            result.result_INT = 1;
            result.result_BOOL = true;
            //result.result_OBJECT = SaatListe();
            result.IsSuccess = true;
            result.UserMessage = "İşlem Başarılı";
            return result;
        }

        internal ActionResultMessages SubeSettingsUpdate(SubeSettingsViewModel course)
        {
            var mf = new ModelFunctions();
            var result = new ActionResultMessages
            {
                result_STR = "Başarısız",
                result_INT = 0,
                result_BOOL = false,
                result_OBJECT = null
            };

            string resut = "true";

            try
            {
                mf.SqlConnOpen();

                int ModifyCounter = 1;
                string TimeStamp = DateTime.Now.ToFileTimeUtc().ToString();
                OleDbCommand CmdModifyCounter = new OleDbCommand("select ModifyCounter from SubeSettings where Id='" + course.ID + "'", mf.ConnOle);
                OleDbDataReader RdrModifyCounter = CmdModifyCounter.ExecuteReader();
                while (RdrModifyCounter.Read())
                {
                    ModifyCounter = mf.ToInt(RdrModifyCounter["ModifyCounter"].ToString()) + 1;
                }

                string CmdString = "Update SubeSettings Set " +
                                   "UpdateDate=getdate() , " +
                                   "UpdateDate_Timestamp='" + TimeStamp + "' , " +
                                   "ModifyCounter='" + ModifyCounter.ToString() + "'  ";
                if (course.SubeName != null) { CmdString = CmdString + " , SubeName='" + course.SubeName + "' "; }
                if (course.SubeIP != null) { CmdString = CmdString + " , SubeIP='" + course.SubeIP + "' "; }
                if (course.SqlName != null) { CmdString = CmdString + " , SqlName='" + course.SqlName + "' "; }
                if (course.SqlPassword != null) { CmdString = CmdString + " , SqlPassword='" + course.SqlPassword + "' "; }
                if (course.FirmaID != null) { CmdString = CmdString + " , FirmaID='" + course.FirmaID + "' "; }
                if (course.DonemID != null) { CmdString = CmdString + " , DonemID='" + course.DonemID + "' "; }
                if (course.DepoID != null) { CmdString = CmdString + " , DepoID='" + course.DepoID + "' "; }
                if (course.AppDbType != null) { CmdString = CmdString + " , AppDbType='" + course.AppDbType + "' "; }
                if (course.AppDbTypeStatus != null) { CmdString = CmdString + " , AppDbTypeStatus='" + course.AppDbTypeStatus + "' "; }
                if (course.PersonelYemekRaporu != null) { CmdString = CmdString + " , PersonelYemekRaporuAdi='" + course.PersonelYemekRaporu + "' "; }
                if (course.VPosSubeKodu != null) { CmdString = CmdString + " , VPosSubeKodu='" + course.VPosSubeKodu + "' "; }
                if (course.VPosKasaKodu != null) { CmdString = CmdString + " , VPosKasaKodu='" + course.VPosKasaKodu + "' "; }

                if (course.FasterKasaListesi != null) { CmdString = CmdString + " , FasterSubeID='" + course.FasterKasaListesi + "' "; }
                else
                {
                    CmdString = CmdString + " , FasterSubeID=null";
                }
                if (course.ZimmetCariInd != null) { CmdString = CmdString + " , SefimPanelZimmetCagrisi='" + course.ZimmetCariInd + "' "; }

                if (course.DBName != null) { CmdString = CmdString + " , DBName='" + course.DBName + "' "; }
                { CmdString = CmdString + " , Status='" + course.Status + "' "; }
                CmdString = CmdString + " , ServiceAdress='" + course.ServiceAdress + "' ";
                CmdString = CmdString + " , BelgeSayimTarihDahil='" + course.BelgeSayimTarihDahil + "' ";
                CmdString = CmdString + " , UrunEslestirmeVarMi='" + course.UrunEslestirmeVarMi + "' ";
                CmdString = CmdString + " where Id= '" + course.ID + "' ";
                OleDbCommand Cmd = new OleDbCommand(CmdString, mf.ConnOle);
                Cmd.ExecuteNonQuery();
                mf.SqlConnClose();

                try
                {
                    if (course.AppDbType == 5)
                    {
                        string connString = "Provider=SQLOLEDB;Server=" + course.SubeIP + ";User Id=" + course.SqlName + ";Password=" + course.SqlPassword + ";Database=" + course.DBName + "";
                        mf.SqlConnOpen(true, connString);
                        mf.DataTable(@"Update [TBLSPOSKASALAR] set ApiUrl = '" + course.ServiceAdress + "' ", true);
                        mf.DataTable(@"Update [TBLSPOSSUBELER] set ApiUrl = '" + course.ServiceAdress + "' ", true);
                        mf.SqlConnClose();
                    }
                }
                catch (Exception ex)
                {
                    Singleton.WritingLogFile2("SubeSettingsCRUD_Update_TBLSPOSKASALAR:", ex.Message.ToString(), "", ex.StackTrace);
                }

                if (mf.MongoConnString != "")
                {
                    MongoClient client = new MongoClient(mf.MongoConnString);
                    MongoServer server = client.GetServer();
                    MongoDatabase db = server.GetDatabase(mf.MongoDBName); //veritabanına bağlantı sağlanıyor
                    var branches = db.GetCollection("branches");
                    var sorgu = new QueryDocument { { "externalId", course.ID } };
                    var sorgu2 = new QueryDocument { { "externalId", course.ID.ToString() } };
                    BsonDocument bs = branches.FindOne(sorgu);
                    BsonDocument bs2 = branches.FindOne(sorgu2);
                    if (bs == null && bs2 == null)
                    {
                        //Ekleme işlemi yapılıyor
                        branches.Insert(new BsonDocument
                        {
                            {"remoteApiUrl",course.ServiceAdress??""},//http://192.168.0.217:8081
                            {"serviceName",course.SubeName},
                            {"externalId", course.ID},
                            {"database", course.DBName},
                            {"name", course.SubeName},
                            {"updatedAt", DateTime.Now.ToUniversalTime()}
                        });
                    }
                    else
                    {

                        var guncelle = new UpdateDocument { { "$set", new BsonDocument{
                        {"remoteApiUrl", course.ServiceAdress??""},
                        {"serviceName",course.SubeName},
                        {"externalId", course.ID},
                        {"database", course.DBName},
                        {"name", course.SubeName},
                        {"updatedAt", DateTime.Now.ToUniversalTime()}
                    } } };
                        if (bs != null)
                        {
                            //Güncelleme işlemi yapılıyor
                            branches.Update(sorgu, guncelle);
                        }
                        if (bs2 != null)
                        {
                            //Güncelleme işlemi yapılıyor
                            branches.Update(sorgu2, guncelle);
                        }
                    }
                }
            }
            catch (OleDbException ex)
            {
                Singleton.WritingLogFile2("SubeSettingsCRUDSubeSettingsUpdate:", ex.Message.ToString(), "", ex.StackTrace);
                resut = ex.Message.ToString(); resut = ex.Message.ToString(); result.IsSuccess = false; result.UserMessage = ex.ToString(); return result;
            }

            result.result_STR = "Başarılı";
            result.result_INT = 1;
            result.result_BOOL = true;
            //result.result_OBJECT = SaatListe();
            result.IsSuccess = true;
            result.UserMessage = "İşlem Başarılı";
            return result;
        }

        internal ActionResultMessages SubeSettingsDelete(int course)
        {
            ModelFunctions mf = new ModelFunctions();
            ActionResultMessages result = new ActionResultMessages
            {
                result_STR = "Başarısız",
                result_INT = 0,
                result_BOOL = false,
                result_OBJECT = null
            };

            string resultEx = "true";

            try
            {
                mf.SqlConnOpen();
                DataTable dt = mf.DataTable("delete from SubeSettings where Status=1 and Id= " + course + "");
                mf.SqlConnClose();

                if (mf.MongoConnString != "")
                {
                    MongoClient client = new MongoClient(mf.MongoConnString);
                    MongoServer server = client.GetServer();
                    MongoDatabase db = server.GetDatabase(mf.MongoDBName); //veritabanına bağlantı sağlanıyor
                    var branches = db.GetCollection("branches");

                    var sorgu = new QueryDocument { { "externalId", course } };
                    var sorgu2 = new QueryDocument { { "externalId", course.ToString() } };
                    //silme işlemi yapılıyor.
                    branches.Remove(sorgu);
                    branches.Remove(sorgu2);
                }
            }
            catch (OleDbException ex)
            {
                Singleton.WritingLogFile2("SubeSettingsCRUDSubeSettingsDelete:", ex.Message.ToString(), "", ex.StackTrace);
                resultEx = "Bir Hata oluştu. Detay:" + ex.ToString();
            }

            result.result_STR = "Başarılı";
            result.result_INT = 1;
            result.result_BOOL = true;
            //result.result_OBJECT = SaatListe();
            return result;
        }

        public static SubeSettingsViewModel GetUser(int id)
        {
            var model = new SubeSettingsViewModel();
            var mf = new ModelFunctions();

            try
            {
                mf.SqlConnOpen();
                DataTable dt = mf.DataTable("Select * from SubeSettings where Status=1 and Id=" + id + " ");
                mf.SqlConnClose();

                foreach (DataRow r in dt.Rows)
                {
                    model.ID = Convert.ToInt32(mf.RTS(r, "Id"));
                    model.SubeName = (mf.RTS(r, "SubeName"));
                    model.SubeIP = mf.RTS(r, "SubeIP");
                    model.SqlName = mf.RTS(r, "SqlName");
                    model.SqlPassword = mf.RTS(r, "SqlPassword");
                    model.DBName = mf.RTS(r, "DBName");
                    model.FirmaID = mf.RTS(r, "FirmaID");
                    model.DepoID = mf.RTS(r, "DepoID");
                    model.DonemID = mf.RTS(r, "DonemID");
                    model.AppDbType = Convert.ToInt32(mf.RTS(r, "AppDbType"));
                    model.AppDbTypeStatus = Convert.ToBoolean(mf.RTS(r, "AppDbTypeStatus"));
                    model.Status = Convert.ToBoolean(mf.RTS(r, "Status"));
                    model.FasterKasaListesi = mf.RTS(r, "FasterSubeID");
                    model.BelgeSayimTarihDahil = Convert.ToBoolean(mf.RTS(r, "BelgeSayimTarihDahil"));
                    model.ServiceAdress = mf.RTS(r, "ServiceAdress");
                    model.UrunEslestirmeVarMi = Convert.ToBoolean(mf.RTS(r, "UrunEslestirmeVarMi"));
                    model.ZimmetCariInd = mf.RTS(r, "SefimPanelZimmetCagrisi") == null ? "0" : mf.RTS(r, "SefimPanelZimmetCagrisi");
                    model.PersonelYemekRaporu = mf.RTS(r, "PersonelYemekRaporuAdi");
                    model.VPosSubeKodu = mf.RTS(r, "VPosSubeKodu");
                    model.VPosKasaKodu = mf.RTS(r, "VPosKasaKodu");
                }
                //model.ZimmetCariInd = model.ZimmetCariInd == null ? "0" : model.ZimmetCariInd;
            }
            catch (Exception ex)
            {
                Singleton.WritingLogFile2("SubeSettingsCRUDGetUser:", ex.Message.ToString(), "", ex.StackTrace);
            }

            return model;
        }

        internal ActionResultMessages SubeSettingsStatusUpdate(int ID)
        {
            ModelFunctions mf = new ModelFunctions();
            ActionResultMessages result = new ActionResultMessages
            {
                result_STR = "Başarısız",
                result_INT = 0,
                result_BOOL = false,
                result_OBJECT = null
            };

            string resut = "true";
            try
            {
                mf.SqlConnOpen();

                int ModifyCounter = 1;
                string getStatus = "";
                string TimeStamp = DateTime.Now.ToFileTimeUtc().ToString();
                OleDbCommand CmdModifyCounter = new OleDbCommand("select ModifyCounter,Status from SubeSettings where Id='" + ID + "'", mf.ConnOle);
                OleDbDataReader RdrModifyCounter = CmdModifyCounter.ExecuteReader();
                while (RdrModifyCounter.Read())
                {
                    ModifyCounter = mf.ToInt(RdrModifyCounter["ModifyCounter"].ToString()) + 1;
                    getStatus = (RdrModifyCounter["Status"].ToString());
                }
                if (getStatus == "True")
                {
                    getStatus = "False";
                }
                else
                {
                    getStatus = "True";
                }

                string CmdString = "update SubeSettings Set Status ='" + getStatus + "'  where Id = " + ID + "";
                OleDbCommand Cmd = new OleDbCommand(CmdString, mf.ConnOle);
                Cmd.ExecuteNonQuery();
                mf.SqlConnClose();
            }
            catch (OleDbException ex)
            {
                Singleton.WritingLogFile2("SubeSettingsCRUDSubeSettingsStatusUpdate:", ex.Message.ToString(), "", ex.StackTrace);
                resut = ex.Message.ToString(); resut = ex.Message.ToString(); result.IsSuccess = false; result.UserMessage = ex.ToString(); return result;
            }

            result.result_STR = "Başarılı";
            result.result_INT = 1;
            result.result_BOOL = true;
            result.IsSuccess = true;
            result.UserMessage = "İşlem Başarılı";
            return result;
        }
    }
}