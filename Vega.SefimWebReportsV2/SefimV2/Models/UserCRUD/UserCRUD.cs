using SefimV2.Helper;
using SefimV2.ViewModels.User;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;

namespace SefimV2.Models
{
    public class UserCRUD
    {
        public static List<UserViewModel> List()
        {
            List<UserViewModel> Liste = new List<UserViewModel>();
            ModelFunctions f = new ModelFunctions();
            DateTime startDate = DateTime.Now;
            try
            {
                f.SqlConnOpen();
                DataTable dt = f.DataTable("select * from Users");

                foreach (DataRow r in dt.Rows)
                {
                    UserViewModel model = new UserViewModel();
                    if (f.RTS(r, "UserName") == "Admin")
                    {
                        model.ID = Convert.ToInt32(f.RTS(r, "Id"));
                        model.IsAdmin = 1;//Convert.ToInt32(f.RTS(r, "IsAdmin"));
                        model.UserName = f.RTS(r, "UserName");
                        model.Name = f.RTS(r, "Name");
                        model.Password = f.RTS(r, "Password");
                        model.Gsm = f.RTS(r, "Gsm");
                        model.EMail = f.RTS(r, "EMail");
                        //model.Status = Convert.ToInt32(f.RTS(r, "Status"));
                        Liste.Add(model);
                    }
                }

                f.SqlConnClose();
            }
            catch (Exception ex)
            {

            }

            return Liste;
        }

        internal ActionResultMessages UserProfileUpdate(UserViewModel course)
        {
            course.UserName = "Admin";

            ModelFunctions f = new ModelFunctions();
            ActionResultMessages result = new ActionResultMessages();
            result.result_STR = "Başarısız";
            result.result_INT = 0;
            result.result_BOOL = false;
            result.result_OBJECT = null;
            string resut = "true";
            try
            {
                f.SqlConnOpen();
                string TimeStamp = DateTime.Now.ToFileTimeUtc().ToString();
                string CmdString =
                    "Update Users Set " +
                    " CreateDate = getDate()," +
                    "CreateDate_Timestamp = '0'," +
                    "ModifyCounter = 0," +
                    "UpdateDate = getdate()," +
                    "UpdateDate_Timestamp = ''," +
                    "UserName = '" + course.UserName + "'," +
                    "Password = '" + course.Password + "'," +
                    "IsAdmin = '1'," +
                    "SubeID = '0'," +
                    "Name = '" + course.Name + "'," +
                    "EMail = '" + course.EMail + "'," +
                    "Gsm = '" + course.Gsm + "'," +
                    "BelgeTipYetkisi = '" + course.BelgeTipiYetkiList + "'," +
                    "Status = '" + 1 + "' WHERE Id ='" + course.ID + "' ";

                OleDbCommand Cmd = new OleDbCommand(CmdString, f.ConnOle);
                Cmd.ExecuteNonQuery();
                f.SqlConnClose();
            }
            catch (OleDbException ex)
            {
                resut = ex.Message.ToString();
            }

            result.result_STR = "Başarılı";
            result.result_INT = 1;
            result.result_BOOL = true;
            result.IsSuccess = true;
            result.UserMessage = "Başarılı";
            //result.result_OBJECT = SaatListe();
            return result;
        }

        public ActionResultMessages UserProfileInsert(UserViewModel course)
        {

            ModelFunctions f = new ModelFunctions();
            ActionResultMessages result = new ActionResultMessages();
            result.result_STR = "Başarısız";
            result.result_INT = 0;
            result.result_BOOL = false;
            result.result_OBJECT = null;
            string resut = "true";
            try
            {
                f.SqlConnOpen();
                string TimeStamp = DateTime.Now.ToFileTimeUtc().ToString();
                string CmdString = " Insert Into Users(CreateDate, CreateDate_Timestamp, ModifyCounter, UpdateDate, UpdateDate_Timestamp, UserName, Password, IsAdmin, SubeID, Name, EMail, Gsm, BelgeTipYetkisi, Status)" +
                                   "Values(" +
                                  "getdate()," +
                                  "''," +
                                  " '0'," +
                                  "''," +
                                  "''," +
                                  "'Admin'," +
                                  "'123'," +
                                  "'1'," +
                                  "'0'," +
                                  "'Demo1'," +
                                  "'mail'," +
                                 "'6666'," +
                                "'" + course.BelgeTipiYetkiList + "' , " +
                                 "'1'" + " " +
                                 " )";
                OleDbCommand Cmd = new OleDbCommand(CmdString, f.ConnOle);
                Cmd.ExecuteNonQuery();
                f.SqlConnClose();
            }
            catch (OleDbException ex) { resut = ex.Message.ToString(); }
            result.result_STR = "Başarılı";
            result.result_INT = 1;
            result.result_BOOL = true;
            //result.result_OBJECT = SaatListe();
            return result;
        }
        public static UserViewModel GetUser(int id)
        {
            UserViewModel model = new UserViewModel();
            ModelFunctions f = new ModelFunctions();
            DateTime startDate = DateTime.Now;
            try
            {
                f.SqlConnOpen();
                DataTable dt = f.DataTable("select * from Users where Id=" + id + " ");

                foreach (DataRow r in dt.Rows)
                {

                    model.ID = Convert.ToInt32(f.RTS(r, "Id"));
                    model.IsAdmin = 1;//Convert.ToInt32(f.RTS(r, "IsAdmin"));
                    model.UserName = f.RTS(r, "UserName");
                    model.Name = f.RTS(r, "Name");
                    model.Password = f.RTS(r, "Password");
                    model.Gsm = f.RTS(r, "Gsm");
                    model.EMail = f.RTS(r, "EMail");
                    //model.Status = Convert.ToInt32(f.RTS(r, "Status"));
                }

                f.SqlConnClose();
            }
            catch (Exception ex)
            {
            }
            return model;
        }

        public UserViewModel GetUserForSubeSettings(string id)
        {
            UserViewModel model = new UserViewModel();
            ModelFunctions f = new ModelFunctions();
            DateTime startDate = DateTime.Now;
            try
            {
                f.SqlConnOpen();
                DataTable dt = f.DataTable(" SELECT us.[Id]" +
                                              ", us.[CreateDate]" +
                                              ", us.[CreateDate_Timestamp]" +
                                              ", us.[ModifyCounter]" +
                                              ", us.[UpdateDate]" +
                                              ", us.[UpdateDate_Timestamp]" +
                                              " ,[UserName]" +
                                              " ,[Password]" +
                                              " ,[IsAdmin]" +
                                              " ,[SubeID]" +
                                              " ,[Name]" +
                                              " ,[EMail]" +
                                              " ,[Gsm]" +
                                              " , us.[Status]" +
                                              " ,[SubeSirasiGorunsunMu]" +
                                              " , st.DepoID " +
                                              " ,st.FirmaID " +
                                               ",st.DonemID" +
                                              " ,st.FasterSubeID " +
                                              ",us.BelgeTipYetkisi " +
                                              ",st.SefimPanelZimmetCagrisi " +
                                                ",st.BelgeSayimTarihDahil " +
                                          " FROM [dbo].[Users] us " +
                                          " left outer join [dbo].[SubeSettings] st on st.Id = us.SubeID " +
                                          " where us.Id =" + id);//22.08.23
                                                                 //" where st.Id =" + id);

                foreach (DataRow r in dt.Rows)
                {
                    model.ID = Convert.ToInt32(f.RTS(r, "Id"));
                    model.IsAdmin = 1;//Convert.ToInt32(f.RTS(r, "IsAdmin"));
                    model.UserName = f.RTS(r, "UserName");
                    model.Name = f.RTS(r, "Name");
                    model.Password = f.RTS(r, "Password");
                    model.Gsm = f.RTS(r, "Gsm");
                    model.EMail = f.RTS(r, "EMail");
                    model.DepoID = f.RTS(r, "DepoID");
                    if (f.RTS(r, "FirmaID") != "")
                    {
                        model.FirmaID = Convert.ToInt32(f.RTS(r, "FirmaID"));
                    }
                    if (f.RTS(r, "DonemID") != "")
                    {
                        model.DonemID = Convert.ToInt32(f.RTS(r, "DonemID"));
                    }
                    model.FasterSubeID = f.RTS(r, "FasterSubeID");
                    model.BelgeTipYetkisi = f.RTS(r, "BelgeTipYetkisi");
                    model.SefimPanelZimmetCagrisi = f.RTS(r, "SefimPanelZimmetCagrisi");
                    model.BelgeSayimTarihDahil = Convert.ToBoolean(f.RTS(r, "BelgeSayimTarihDahil"));
                    //model.Status = Convert.ToInt32(f.RTS(r, "Status"));
                }

                f.SqlConnClose();
            }
            catch (Exception ex)
            {
                Singleton.WritingLog("GetUserForSubeSettings:", "ex:" + ex.Message);
            }
            return model;
        }

        public UserViewModel GetUserForSubeSettings2Yeni(string subeId)
        {
            UserViewModel model = new UserViewModel();
            ModelFunctions f = new ModelFunctions();

            try
            {
                f.SqlConnOpen();
                DataTable dt = f.DataTable(" SELECT us.[Id]" +
                                              ", us.[CreateDate]" +
                                              ", us.[CreateDate_Timestamp]" +
                                              ", us.[ModifyCounter]" +
                                              ", us.[UpdateDate]" +
                                              ", us.[UpdateDate_Timestamp]" +
                                              " ,[UserName]" +
                                              " ,[Password]" +
                                              " ,[IsAdmin]" +
                                              " ,[SubeID]" +
                                              " ,[Name]" +
                                              " ,[EMail]" +
                                              " ,[Gsm]" +
                                              " , us.[Status]" +
                                              " ,[SubeSirasiGorunsunMu]" +
                                              " , st.DepoID " +
                                              " ,st.FirmaID " +
                                              " ,st.DonemID" +
                                              " ,st.FasterSubeID " +
                                              " ,us.BelgeTipYetkisi " +
                                              " ,st.SefimPanelZimmetCagrisi " +
                                              " ,st.BelgeSayimTarihDahil " +
                                          " FROM [dbo].[Users] us " +
                                          " left outer join [dbo].[SubeSettings] st on st.Id = us.SubeID " +
                                          //" where us.Id =" + id);//22.08.23
                                          " where st.Id =" + subeId);
                f.SqlConnClose();

                foreach (DataRow r in dt.Rows)
                {
                    model.ID = Convert.ToInt32(f.RTS(r, "Id"));
                    model.IsAdmin = 1;//Convert.ToInt32(f.RTS(r, "IsAdmin"));
                    model.UserName = f.RTS(r, "UserName");
                    model.Name = f.RTS(r, "Name");
                    model.Password = f.RTS(r, "Password");
                    model.Gsm = f.RTS(r, "Gsm");
                    model.EMail = f.RTS(r, "EMail");
                    model.DepoID = f.RTS(r, "DepoID");
                    if (f.RTS(r, "FirmaID") != "")
                    {
                        model.FirmaID = Convert.ToInt32(f.RTS(r, "FirmaID"));
                    }
                    if (f.RTS(r, "DonemID") != "")
                    {
                        model.DonemID = Convert.ToInt32(f.RTS(r, "DonemID"));
                    }
                    model.FasterSubeID = f.RTS(r, "FasterSubeID");
                    model.BelgeTipYetkisi = f.RTS(r, "BelgeTipYetkisi");
                    model.SefimPanelZimmetCagrisi = f.RTS(r, "SefimPanelZimmetCagrisi");
                    model.BelgeSayimTarihDahil = Convert.ToBoolean(f.RTS(r, "BelgeSayimTarihDahil"));
                    //model.Status = Convert.ToInt32(f.RTS(r, "Status"));
                }


            }
            catch (Exception ex)
            {
                Singleton.WritingLog("GetUserForSubeSettings ", "ex " + ex.Message);
            }
            return model;
        }

        public UserViewModel GetUserForSubeSettingsForSubeId(string subeId)
        {
            var model = new UserViewModel();
            var f = new ModelFunctions();
            try
            {
                f.SqlConnOpen();
                DataTable dt = f.DataTable(" Select * from SubeSettings where  Status=1 and ID=" + subeId);

                foreach (DataRow r in dt.Rows)
                {
                    model.ID = Convert.ToInt32(f.RTS(r, "Id"));
                    model.IsAdmin = 1;//Convert.ToInt32(f.RTS(r, "IsAdmin"));
                    model.UserName = f.RTS(r, "UserName");
                    model.Name = f.RTS(r, "Name");
                    model.Password = f.RTS(r, "Password");
                    model.Gsm = f.RTS(r, "Gsm");
                    model.EMail = f.RTS(r, "EMail");
                    model.DepoID = f.RTS(r, "DepoID");
                    if (f.RTS(r, "FirmaID") != "")
                    {
                        model.FirmaID = Convert.ToInt32(f.RTS(r, "FirmaID"));
                    }
                    if (f.RTS(r, "DonemID") != "")
                    {
                        model.DonemID = Convert.ToInt32(f.RTS(r, "DonemID"));
                    }
                    model.FasterSubeID = f.RTS(r, "FasterSubeID");
                    model.BelgeTipYetkisi = f.RTS(r, "BelgeTipYetkisi");
                    model.SefimPanelZimmetCagrisi = f.RTS(r, "SefimPanelZimmetCagrisi");
                    model.BelgeSayimTarihDahil = Convert.ToBoolean(f.RTS(r, "BelgeSayimTarihDahil"));
                    //model.Status = Convert.ToInt32(f.RTS(r, "Status"));
                }

                f.SqlConnClose();
            }
            catch (Exception ex)
            {
                Singleton.WritingLog("GetUserForSubeSettingsForSubeIdEx ", "ex " + ex.Message);
            }
            return model;
        }

        public int GetZimFiyat(string UserId, int CariId)
        {
            int res = 0;
            UserViewModel us = GetUserForSubeSettings(UserId);
            ModelFunctions f = new ModelFunctions();
            DateTime startDate = DateTime.Now;
            try
            {
                f.SqlConnOpen();
                DataTable dtt = f.DataTable("SELECT Id,DBName,IP,SqlName,SqlPassword  FROM  VegaDbSettings");
                string VegaDbId = "";
                string VegaDbName = "";

                string VegaDbIp = "";
                string VegaDbSqlName = "";

                string VegaDbSqlPassword = "";
                foreach (DataRow r in dtt.Rows)
                {
                    VegaDbId = f.RTS(r, "Id");
                    VegaDbName = f.RTS(r, "DBName");
                    VegaDbIp = f.RTS(r, "IP");
                    VegaDbSqlName = f.RTS(r, "SqlName");
                    VegaDbSqlPassword = f.RTS(r, "SqlPassword");
                }

                string connString = "Provider=SQLOLEDB;Server=" + VegaDbIp + ";User Id=" + VegaDbSqlName + ";Password=" + VegaDbSqlPassword + ";Database=" + VegaDbName + "";
                f.SqlConnOpen(true, connString);
                if (CariId > 0)
                {
                    DataTable dt = f.DataTable("select ZIMFIYAT from F0" + us.FirmaID + "TBLCARI where IND =" + CariId, true);

                    foreach (DataRow r in dt.Rows)
                    {
                        res = Convert.ToInt32(f.RTS(r, "ZIMFIYAT"));
                    }
                }


                f.SqlConnClose();
            }
            catch (System.Exception ex)
            {
            }
            return res;
        }
    }

}