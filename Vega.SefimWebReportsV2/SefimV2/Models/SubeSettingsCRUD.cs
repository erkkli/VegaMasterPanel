using SefimV2.ViewModels.SubeSettings;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.Linq;
using System.Web;

namespace SefimV2.Models
{
    public class SubeSettingsCRUD
    {
        public static List<SubeSettingsViewModel> List()
        {
            List<SubeSettingsViewModel> Liste = new List<SubeSettingsViewModel>();
            ModelFunctions f = new ModelFunctions();
            DateTime startDate = DateTime.Now;

            try
            {
                f.SqlConnOpen();
                DataTable dt = f.DataTable("select * from SubeSettings");
                foreach (DataRow r in dt.Rows)
                {
                    SubeSettingsViewModel model = new SubeSettingsViewModel();
                    model.ID = Convert.ToInt32(f.RTS(r, "ID"));
                    model.SubeName = (f.RTS(r, "SubeName"));
                    model.SubeIP = f.RTS(r, "SubeIP");
                    model.SqlName = f.RTS(r, "SqlName");
                    model.SqlPassword = f.RTS(r, "SqlPassword");
                    model.DBName = f.RTS(r, "DBName");
                    model.FirmaID = f.RTS(r, "FirmaID");
                    model.DepoID = f.RTS(r, "DepoID");
                    model.DonemID = f.RTS(r, "DonemID");
                    model.Status = Convert.ToBoolean(f.RTS(r, "Status"));
                    model.AppDbType = Convert.ToInt32(f.RTS(r, "AppDbType"));
                    model.AppDbTypeStatus = Convert.ToBoolean(f.RTS(r, "AppDbTypeStatus"));

                    #region ENVANTER DB BAGLANIP GEREKLİ BILGILER ALIIYOR 
                    try
                    {
                        f.SqlConnOpen();
                        DataTable dt_e = f.DataTable("SELECT ID,DBName,IP,SqlName,SqlPassword  FROM  VegaDbSettings");
                        string Query = "";
                        foreach (DataRow Envanter in dt_e.Rows)
                        {
                            string VegaDbId = f.RTS(Envanter, "ID");
                            string VegaDbName = f.RTS(Envanter, "DBName");
                            string VegaDbIp = f.RTS(Envanter, "IP");
                            string VegaDbSqlName = f.RTS(Envanter, "SqlName");
                            string VegaDbSqlPassword = f.RTS(Envanter, "SqlPassword");

                            #region ENVANTER'DEN FİRMA, DÖNEM ve DEPOADI'NI  getiriyor                           
                            for (int i = 0; i < 3; i++)
                            {
                                if (i == 0)
                                {
                                    Query = "SELECT IND, KOD, KISAAD FROM TBLFIRMA WHERE IND=" + model.FirmaID + "";
                                    string connString = "Provider=SQLOLEDB;Server=" + VegaDbIp + ";User ID=" + VegaDbSqlName + ";Password=" + VegaDbSqlPassword + ";Database=" + VegaDbName + "";
                                    f.SqlConnOpen(true, connString);
                                    DataTable EnvanterDt = f.DataTable(Query, true);
                                    if (EnvanterDt.Rows.Count > 0)
                                    {
                                        foreach (DataRow EnvanterR in EnvanterDt.Rows)
                                        {
                                            model.FirmaID = f.RTS(EnvanterR, "KOD");
                                        }
                                    }
                                }
                                else if (i == 1)
                                {
                                    Query = "SELECT FIND, IND, DONEM FROM TBLDONEM   WHERE IND =" + model.DonemID + "";
                                    string connString = "Provider=SQLOLEDB;Server=" + VegaDbIp + ";User ID=" + VegaDbSqlName + ";Password=" + VegaDbSqlPassword + ";Database=" + VegaDbName + "";
                                    f.SqlConnOpen(true, connString);
                                    DataTable EnvanterDt = f.DataTable(Query, true);
                                    if (EnvanterDt.Rows.Count > 0)
                                    {
                                        foreach (DataRow EnvanterR in EnvanterDt.Rows)
                                        {
                                            model.DonemID = f.RTS(EnvanterR, "DONEM");
                                        }
                                    }
                                }
                                else if (i == 2)
                                {
                                    Query = "SELECT IND, DEPOADI, DEPOKODU FROM F0100TBLDEPOLAR WHERE IND=" + model.DepoID + "";
                                    string connString = "Provider=SQLOLEDB;Server=" + VegaDbIp + ";User ID=" + VegaDbSqlName + ";Password=" + VegaDbSqlPassword + ";Database=" + VegaDbName + "";
                                    f.SqlConnOpen(true, connString);
                                    DataTable EnvanterDt = f.DataTable(Query, true);
                                    if (EnvanterDt.Rows.Count > 0)
                                    {
                                        foreach (DataRow EnvanterR in EnvanterDt.Rows)
                                        {
                                            model.DepoID = f.RTS(EnvanterR, "DEPOADI");
                                        }
                                    }
                                }
                            }
                            #endregion
                        }
                        f.SqlConnClose();
                    }
                    catch (Exception) { }
                    #endregion

                    Liste.Add(model);
                }

                f.SqlConnClose();
            }
            catch (System.Exception ex) { }

            return Liste;
        }

        public mdlActionResult SubeSettingsInsert(SubeSettingsViewModel course)
        {
            ModelFunctions f = new ModelFunctions();
            mdlActionResult result = new mdlActionResult();
            result.result_STR = "Başarısız";
            result.result_INT = 0;
            result.result_BOOL = false;
            result.result_OBJECT = null;
            string resut = "true";

            try
            {
                f.SqlConnOpen();
                string TimeStamp = DateTime.Now.ToFileTimeUtc().ToString();
                string CmdString = "Insert Into SubeSettings(CreateDate,CreateDate_Timestamp,SubeName,SubeIP,SqlName,SqlPassword,DBName,FirmaID,DonemID,DepoID,AppDbType,AppDbTypeStatus,Status)" +
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
                    "'" + course.Status + "'  " +
                    ")";
                OleDbCommand Cmd = new OleDbCommand(CmdString, f.ConnOle);
                Cmd.ExecuteNonQuery();
                f.SqlConnClose();

            }
            catch (OleDbException ex) { resut = ex.Message.ToString(); result.IsSuccess = false; result.UserMessage = ex.ToString(); return result; }
            result.result_STR = "Başarılı";
            result.result_INT = 1;
            result.result_BOOL = true;
            //result.result_OBJECT = SaatListe();
            result.IsSuccess = true;
            result.UserMessage = "İşlem Başarılı";
            return result;
        }

        internal mdlActionResult SubeSettingsUpdate(SubeSettingsViewModel course)
        {
            ModelFunctions f = new ModelFunctions();
            mdlActionResult result = new mdlActionResult();
            result.result_STR = "Başarısız";
            result.result_INT = 0;
            result.result_BOOL = false;
            result.result_OBJECT = null;
            string resut = "true";
            try
            {
                f.SqlConnOpen();

                int ModifyCounter = 1;
                string TimeStamp = DateTime.Now.ToFileTimeUtc().ToString();
                OleDbCommand CmdModifyCounter = new OleDbCommand("select ModifyCounter from SubeSettings where ID='" + course.ID + "'", f.ConnOle);
                OleDbDataReader RdrModifyCounter = CmdModifyCounter.ExecuteReader();
                while (RdrModifyCounter.Read())
                {
                    ModifyCounter = f.ToInt(RdrModifyCounter["ModifyCounter"].ToString()) + 1;
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

                if (course.DBName != null) { CmdString = CmdString + " , DBName='" + course.DBName + "' "; }
                { CmdString = CmdString + " , Status='" + course.Status + "' "; }
                CmdString = CmdString + " where ID= '" + course.ID + "' ";
                OleDbCommand Cmd = new OleDbCommand(CmdString, f.ConnOle);
                Cmd.ExecuteNonQuery();
                f.SqlConnClose();
            }
            catch (OleDbException ex)
            {
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

        internal mdlActionResult SubeSettingsDelete(int course)
        {
            ModelFunctions f = new ModelFunctions();
            mdlActionResult result = new mdlActionResult();
            result.result_STR = "Başarısız";
            result.result_INT = 0;
            result.result_BOOL = false;
            result.result_OBJECT = null;
            string resut = "true";
            try
            {
                f.SqlConnOpen();
                DataTable dt = f.DataTable("delete from SubeSettings where ID= " + course + "");
                f.SqlConnClose();

            }
            catch (OleDbException ex) { resut = ex.Message.ToString(); }
            result.result_STR = "Başarılı";
            result.result_INT = 1;
            result.result_BOOL = true;
            //result.result_OBJECT = SaatListe();
            return result;
        }

        public static SubeSettingsViewModel GetUser(int id)
        {
            SubeSettingsViewModel model = new SubeSettingsViewModel();

            ModelFunctions f = new ModelFunctions();
            DateTime startDate = DateTime.Now;
            try
            {
                f.SqlConnOpen();
                DataTable dt = f.DataTable("Select * from SubeSettings where ID=" + id + " ");

                foreach (DataRow r in dt.Rows)
                {
                    model.ID = Convert.ToInt32(f.RTS(r, "ID"));
                    model.SubeName = (f.RTS(r, "SubeName"));
                    model.SubeIP = f.RTS(r, "SubeIP");
                    model.SqlName = f.RTS(r, "SqlName");
                    model.SqlPassword = f.RTS(r, "SqlPassword");
                    model.DBName = f.RTS(r, "DBName");
                    model.FirmaID = f.RTS(r, "FirmaID");
                    model.DepoID = f.RTS(r, "DepoID");
                    model.DonemID = f.RTS(r, "DonemID");
                    model.AppDbType = Convert.ToInt32(f.RTS(r, "AppDbType"));
                    model.AppDbTypeStatus = Convert.ToBoolean(f.RTS(r, "AppDbTypeStatus"));
                    model.Status = Convert.ToBoolean(f.RTS(r, "Status"));
                }

                f.SqlConnClose();
            }
            catch (System.Exception ex) { }
            return model;
        }
    }
}