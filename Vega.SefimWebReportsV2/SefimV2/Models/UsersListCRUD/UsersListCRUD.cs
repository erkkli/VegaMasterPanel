using SefimV2.Helper;
using SefimV2.ViewModels.User;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.Data.SqlClient;
using System.Web.Configuration;
using static SefimV2.ViewModels.User.UserViewModel;

namespace SefimV2.Models
{
    public class UsersListCRUD
    {
        ModelFunctions mF = new ModelFunctions();
        #region Config local copy db connction setting       
        static readonly string masterDbSubeIp = WebConfigurationManager.AppSettings["Server"];
        static readonly string masterDbName = WebConfigurationManager.AppSettings["DBName"];
        static readonly string masterSqlKullaniciName = WebConfigurationManager.AppSettings["User"];
        static readonly string masterSqlKullaniciPassword = WebConfigurationManager.AppSettings["Password"];
        #endregion


        public static List<UserViewModel> List()
        {
            var Liste = new List<UserViewModel>();
            var mf = new ModelFunctions();

            try
            {
                mf.SqlConnOpen();
                DataTable dt = mf.DataTable("select * from Users");
                mf.SqlConnClose();

                foreach (DataRow r in dt.Rows)
                {
                    var model = new UserViewModel();

                    if (mf.RTS(r, "UserName") != "Admin")
                    {
                        model.ID = Convert.ToInt32(mf.RTS(r, "Id"));
                        model.IsAdmin = 1;//Convert.ToInt32(f.RTS(r, "IsAdmin"));
                        model.UserName = mf.RTS(r, "UserName");
                        model.Name = mf.RTS(r, "Name");
                        model.Password = mf.RTS(r, "Password");
                        model.Gsm = mf.RTS(r, "Gsm");
                        model.EMail = mf.RTS(r, "EMail");
                        model.Status = Convert.ToBoolean(mf.RTS(r, "Status"));
                        Liste.Add(model);
                    }
                }
                //UserViewModel model = new UserViewModel();
            }
            catch (Exception ex)
            {
                Singleton.WritingLogFile2("UsersListCRUDList:", ex.Message.ToString(), "", ex.StackTrace);
            }

            return Liste;
        }

        public ActionResultMessages UserProfileInsert(UserViewModel course, int[] Auth)
        {
            var f = new ModelFunctions();
            var result = new ActionResultMessages();
            int SonKayitID = 0;

            //using (SqlConnection con = new SqlConnection(mF.NewConnectionString(masterDbSubeIp, masterDbName, masterSqlKullaniciName, masterSqlKullaniciPassword)))
            //{
            //    con.Open();
            //    var transaction = con.BeginTransaction();
            //    var sqlData = new SqlData(con);

            try
            {
                f.SqlConnOpen();
                string userInsertScript = " Insert Into Users(CreateDate, CreateDate_Timestamp, ModifyCounter, UpdateDate, UpdateDate_Timestamp, UserName, " +
                                           " Password, IsAdmin, SubeID, Name, EMail, Gsm, SubeSirasiGorunsunMu, UygulmaTipi, BelgeTipYetkisi," +
                                           " YetkiNesneId, YetkiNesneAdi, Status )" +
                                           " Values(" +
                                           " getdate()," +
                                           "''," +
                                           " '0'," +
                                           "''," +
                                           "''," +
                                           "'" + course.UserName + "'," +
                                           "'" + course.Password + "'," +
                                           "'1'," +
                                           "" + 12 + "," +
                                           "'" + course.Name + "'," +
                                           "'" + course.EMail + "'," +
                                           "'" + course.Gsm + "'," +
                                           "'" + course.SubeSirasiGorunsunMu + "'," +
                                           "'" + course.UygulamaTipi.GetHashCode() + "'," +
                                           "'" + course.BelgeTipiYetkiList + "'," +
                                           "'" + course.YetkiNesneId + "'," +
                                           "'" + course.YetkiNesneAdi + "'," +
                                           "'1'" +
                                           " ) " +
                                            "select CAST(scope_identity() AS int);";
                OleDbCommand Cmd = new OleDbCommand(userInsertScript, f.ConnOle);
                //Cmd.ExecuteNonQuery();
                int PkId = (int)Cmd.ExecuteScalar();
                f.SqlConnClose();

                //                    var sc =
                //                            " Insert Into Users(CreateDate, CreateDate_Timestamp, ModifyCounter, UpdateDate, UpdateDate_Timestamp, UserName, " +
                //                            " Password, IsAdmin, SubeID, Name, EMail, Gsm, SubeSirasiGorunsunMu, UygulmaTipi, BelgeTipYetkisi," +
                //                            " YetkiNesneId, YetkiNesneAdi, Status )" +
                //                            " Values(" +
                //                            " @par1" +
                //                            " @par2," +
                //                            " @par3," +
                //                            " @par4," +
                //                            " @par5," +
                //                            " @par6," +
                //                            " @par7," +
                //                            " @par8," +
                //                            " @par9," +
                //                            " @par10," +
                //                            " @par11," +
                //                            " @par12," +
                //                            " @par13," +
                //                            " @par14," +
                //                            " @par15," +
                //                            " @par16," +
                //                            " @par17," +
                //                            " @par18," +
                //                            " ) " +
                //                            " select CAST(scope_identity() AS int);";

                //                    var icmalPaymentId = sqlData.ExecuteScalarTransactionSql(userInsertScript, transaction,
                //                       new object[]
                //                       {
                //DateTime.Now,
                //null,
                //'0',
                //null,
                //null,
                // course.UserName + "'," +
                //"'" + course.Password + "'," +
                //"'1'," +
                //"" + 12 + "," +
                //"'" + course.Name + "'," +
                //"'" + course.EMail + "'," +
                //"'" + course.Gsm + "'," +
                //"'" + course.SubeSirasiGorunsunMu + "'," +
                //"'" + course.UygulamaTipi.GetHashCode() + "'," +
                //"'" + course.BelgeTipiYetkiList + "'," +
                //"'" + course.YetkiNesneId + "'," +
                //"'" + course.YetkiNesneAdi + "'," +
                //"'1'" +
                //                       });





                #region YetkiUser tablosuna kayıt yapar

                //var sqlData = new SqlData(new SqlConnection(mF.NewConnectionString(masterDbSubeIp, masterDbName, masterSqlKullaniciName, masterSqlKullaniciPassword)));
                //var getYekiNesneDetay = mF.GetSubeDataWithQuery(mF.NewConnectionString(masterDbSubeIp, masterDbName, masterSqlKullaniciName, masterSqlKullaniciPassword), "Select * From [dbo].[YetkiNesneDetay] where YetkiNesneId=" + course.YetkiNesneId);

                //foreach (DataRow yetkiMenu in getYekiNesneDetay.Rows)
                //{
                //    var Id = mF.RTI(yetkiMenu, "Id");
                //    var MenuId = mF.RTI(yetkiMenu, "MenuId");
                //    var IslemTipiId = mF.RTI(yetkiMenu, "IslemTipiId");
                //    var YetkiNesneId = mF.RTI(yetkiMenu, "YetkiNesneId");

                //    sqlData.ExecuteSql("INSERT INTO [dbo].[YetkiUser] ( [MenuId],[IslemTipiId],[UserId],[IsActive] ) VALUES ( @par1, @par2, @par3, @par4 )",
                //        new object[]
                //        {
                //              MenuId,
                //              IslemTipiId,
                //              PkId,
                //              1
                //        });
                //}


                var sqlData = new SqlData(new SqlConnection(mF.NewConnectionString(masterDbSubeIp, masterDbName, masterSqlKullaniciName, masterSqlKullaniciPassword)));
                var getYekiNesneDetay = mF.GetSubeDataWithQuery(mF.NewConnectionString(masterDbSubeIp, masterDbName, masterSqlKullaniciName, masterSqlKullaniciPassword), "Select * From [dbo].[YetkiNesneDetay] where YetkiNesneId=" + course.YetkiNesneId);

                foreach (DataRow yetkiMenu in getYekiNesneDetay.Rows)
                {
                    var Id = mF.RTI(yetkiMenu, "Id");
                    var MenuId = mF.RTI(yetkiMenu, "MenuId");
                    var IslemTipiId = mF.RTI(yetkiMenu, "IslemTipiId");
                    var YetkiNesneId = mF.RTI(yetkiMenu, "YetkiNesneId");

                    sqlData.ExecuteSql("INSERT INTO [dbo].[YetkiUser] ( [MenuId],[IslemTipiId],[UserId],[YetkiNesneId],[IsActive] ) VALUES ( @par1, @par2, @par3, @par4, @par5 )",
                        new object[]
                        {
                        MenuId,
                        IslemTipiId,
                        course.ID,
                        YetkiNesneId,
                        1
                        });
                }

                #endregion YetkiUser tablosuna kayıt yapar
            }
            catch (OleDbException ex)
            {
                Singleton.WritingLogFile2("UsersListCRUDUserProfileInsert:", ex.Message.ToString(), "", ex.StackTrace);
                result.IsSuccess = false; result.UserMessage = "Bir Hata Oluştu." + Environment.NewLine + " Hata detayı:" + ex.ToString(); return result;
            }


            #region SON KAYIT ID 

            f.SqlConnOpen();
            DataTable dt1 = f.DataTable("SELECT top 1 * FROM Users order by Id desc");
            f.SqlConnClose();

            foreach (DataRow r in dt1.Rows)
            {
                SonKayitID = Convert.ToInt32(f.RTS(r, "Id"));
            }
            #endregion


            #region YETKI INSERT           
            try
            {
                f.SqlConnOpen();

                FR_SubeListesiViewModel model = new FR_SubeListesiViewModel();
                foreach (var item in Auth)
                {
                    model.SubeID = item;
                    string UserSubeRelationsCmdString = " Insert Into UserSubeRelations(CreateDate, CreateDate_Timestamp, ModifyCounter, UpdateDate, UpdateDate_Timestamp, UserID, SubeID)" +
                                                        " Values(" +
                                                        " getdate()," +
                                                        " ''," +
                                                        " '0'," +
                                                        "''," +
                                                        "''," +
                                                        "'" + SonKayitID.ToString() + "' ," +
                                                        "'" + item + "'" + "   )";
                    OleDbCommand Cmd_ = new OleDbCommand(UserSubeRelationsCmdString, f.ConnOle);
                    Cmd_.ExecuteNonQuery();
                }

                f.SqlConnClose();
            }
            catch (OleDbException ex)
            {

                Singleton.WritingLogFile2("UsersListCRUDUserYetkiInsert", ex.ToString(), null, ex.StackTrace);
                result.IsSuccess = false; result.UserMessage = ex.ToString(); return result;
            }
            #endregion

            result.IsSuccess = true;
            result.UserMessage = "Başarılı";
            return result;
            //}
        }

        internal ActionResultMessages UserProfileUpdate(UserViewModel course, int[] Auth)
        {
            var f = new ModelFunctions();
            var result = new ActionResultMessages();

            #region GUNCELLENECEK KAYIT ONCE SILINIR 

            try
            {
                f.SqlConnOpen();
                DataTable dt = f.DataTable("Delete from UserSubeRelations where UserID= " + course.ID + "");
                f.SqlConnClose();
            }
            catch (OleDbException ex)
            {
                Singleton.WritingLogFile2("UserProfileUpdateCRUDDelete", ex.ToString(), null, ex.StackTrace);
                result.IsSuccess = false; result.UserMessage = ex.ToString(); return result;
            }
            #endregion


            #region YETKI INSERT           
            try
            {
                f.SqlConnOpen();

                var model = new FR_SubeListesiViewModel();
                foreach (var item in Auth)
                {
                    model.SubeID = item;
                    string TimeStamp = DateTime.Now.ToFileTimeUtc().ToString();
                    string CmdString = "Insert Into UserSubeRelations(CreateDate, CreateDate_Timestamp, ModifyCounter, UpdateDate, UpdateDate_Timestamp, UserID, SubeID)" +
                                       "Values(" +
                                       "getdate()," +
                                       "''," +
                                       " '0'," +
                                       "''," +
                                       "''," +
                                       "'" + course.ID + "' ," +
                                       "'" + item + "'" + "   )";
                    OleDbCommand Cmd = new OleDbCommand(CmdString, f.ConnOle);
                    Cmd.ExecuteNonQuery();
                }

                f.SqlConnClose();
            }
            catch (OleDbException ex)
            {
                Singleton.WritingLogFile2("UserProfileUpdateCRUDInsert", ex.ToString(), null, ex.StackTrace);
                result.IsSuccess = false; result.UserMessage = ex.ToString(); return result;
            }
            #endregion

            #region YetkiUser tablosuna kayıt yapar
            f.SqlConnOpen();
            var yetkiUserDelete = f.DataTable("Delete From YetkiUser where UserId= " + course.ID + "");
            f.SqlConnClose();

            var sqlData = new SqlData(new SqlConnection(mF.NewConnectionString(masterDbSubeIp, masterDbName, masterSqlKullaniciName, masterSqlKullaniciPassword)));
            var getYekiNesneDetay = mF.GetSubeDataWithQuery(mF.NewConnectionString(masterDbSubeIp, masterDbName, masterSqlKullaniciName, masterSqlKullaniciPassword), "Select * From [dbo].[YetkiNesneDetay] where YetkiNesneId=" + course.YetkiNesneId);

            foreach (DataRow yetkiMenu in getYekiNesneDetay.Rows)
            {
                var Id = mF.RTI(yetkiMenu, "Id");
                var MenuId = mF.RTI(yetkiMenu, "MenuId");
                var IslemTipiId = mF.RTI(yetkiMenu, "IslemTipiId");
                var YetkiNesneId = mF.RTI(yetkiMenu, "YetkiNesneId");

                sqlData.ExecuteSql("INSERT INTO [dbo].[YetkiUser] ( [MenuId],[IslemTipiId],[UserId],[YetkiNesneId],[IsActive] ) VALUES ( @par1, @par2, @par3, @par4, @par5 )",
                    new object[]
                    {
                        MenuId,
                        IslemTipiId,
                        course.ID,
                        YetkiNesneId,
                        1
                    });
            }

            #endregion YetkiUser tablosuna kayıt yapar

            try
            {
                f.SqlConnOpen();
                string TimeStamp = DateTime.Now.ToFileTimeUtc().ToString();

                string CmdString = "Update Users Set " +
                                   "CreateDate = getDate()," +
                                   "CreateDate_Timestamp = '0'," +
                                   "ModifyCounter = 0," +
                                   "UpdateDate = getdate()," +
                                   "UpdateDate_Timestamp = ''," +
                                   "UserName = '" + course.UserName + "'," +
                                   "Password = '" + course.Password + "'," +
                                   "IsAdmin = '1'," +
                                   "SubeID = '" + course.SubeID + "'," +
                                   "Name = '" + course.Name + "'," +
                                   "EMail = '" + course.EMail + "'," +
                                   "Gsm = '" + course.Gsm + "'," +
                                   "SubeSirasiGorunsunMu = '" + course.SubeSirasiGorunsunMu + "'," +
                                   "UygulmaTipi = '" + course.UygulamaTipi.GetHashCode() + "'," +
                                   "BelgeTipYetkisi = '" + course.BelgeTipiYetkiList + "'," +
                                   "YetkiNesneId = '" + course.YetkiNesneId + "'," +
                                   "Status = '" + course.Status + "' WHERE Id ='" + course.ID + "' ";

                OleDbCommand Cmd = new OleDbCommand(CmdString, f.ConnOle);
                Cmd.ExecuteNonQuery();
                f.SqlConnClose();
            }
            catch (OleDbException ex)
            {
                Singleton.WritingLogFile2("UserProfileUpdateCRUDYetkiUser", ex.ToString(), null, ex.StackTrace);
                result.IsSuccess = false; result.UserMessage = "Kullanıcı tablosuna kayıt eklenemedi. Detay:" + ex.ToString(); return result;
            }

            result.IsSuccess = true;
            result.UserMessage = "İşlem Başarılı";
            return result;
        }

        internal ActionResultMessages UserProfileDelete(int userId)
        {
            var mf = new ModelFunctions();
            var result = new ActionResultMessages();

            try
            {
                mf.SqlConnOpen();
                var usersDelete = mf.DataTable("Delete From Users where Id= " + userId + "");

                #region YetkiUser delete
                var yetkiUserDelete = mf.DataTable("Delete From YetkiUser where UserId= " + userId + "");
                #endregion
                mf.SqlConnClose();
            }
            catch (OleDbException ex)
            {
                Singleton.WritingLogFile2("UserProfileDeleteCRUDDelete", ex.ToString(), null, ex.StackTrace);
                result.UserMessage = ex.Message;
                result.IsSuccess = false;
                return result;
            }

            result.UserMessage = "Başarılı";
            result.IsSuccess = true;
            return result;
        }

        public static UserViewModel GetUser(int id, string ID)
        {
            var mf = new ModelFunctions();
            var model = new UserViewModel();

            //if (ID != "1")
            //{
            //    model.YetkiStatus = "1";
            //    model.YetkiStatusAciklama = "Yetkiniz Bulunmamaktadır";
            //    return model;
            //}

            try
            {
                mf.SqlConnOpen();
                DataTable dtUsers = mf.DataTable("Select * from Users where Id=" + id + " ");
                mf.SqlConnClose();

                foreach (DataRow r in dtUsers.Rows)
                {
                    model.ID = Convert.ToInt32(mf.RTS(r, "Id"));
                    model.IsAdmin = 1;
                    model.UserName = mf.RTS(r, "UserName");
                    model.Name = mf.RTS(r, "Name");
                    model.Password = mf.RTS(r, "Password");
                    model.Gsm = mf.RTS(r, "Gsm");
                    model.EMail = mf.RTS(r, "EMail");
                    model.SubeID = Convert.ToInt32(mf.RTS(r, "SubeID"));
                    model.Status = Convert.ToBoolean(mf.RTS(r, "Status"));
                    model.SubeSirasiGorunsunMu = Convert.ToBoolean(mf.RTS(r, "SubeSirasiGorunsunMu") == string.Empty ? "false" : mf.RTS(r, "SubeSirasiGorunsunMu"));
                    var uTipi = mf.RTI(r, "UygulmaTipi");
                    model.UygulamaTipi = (Enums.General.UygulamaTipi)uTipi;
                    model.BelgeTipiYetkiList = mf.RTS(r, "BelgeTipYetkisi");
                    model.YetkiNesneAdi = mf.RTS(r, "YetkiNesneAdi");
                    model.YetkiNesneId = mf.RTI(r, "YetkiNesneId");
                }

                #region Şube Listesi

                var frSubeList = new List<FR_SubeListesiViewModel>();

                mf.SqlConnOpen();
                DataTable dt1 = mf.DataTable("select * from SubeSettings");
                mf.SqlConnClose();

                foreach (DataRow r in dt1.Rows)
                {
                    FR_SubeListesiViewModel subeListModel = new FR_SubeListesiViewModel
                    {
                        SubeID = Convert.ToInt32(mf.RTS(r, "Id")),
                        SubeName = (mf.RTS(r, "SubeName"))
                    };
                    frSubeList.Add(subeListModel);
                }
                model.FR_SubeListesi = frSubeList;

                #endregion Şube Listesi

                #region BelgeTipi yetki listesi

                string[] belgeTipiYetkiList = model.BelgeTipiYetkiList.Split(',');

                #endregion BelgeTipi yetki listesi
            }
            catch (Exception ex)
            {
                Singleton.WritingLogFile2("UsersListCRUDGetUser:", ex.Message.ToString(), "", ex.StackTrace);
            }

            return model;
        }

        public static UserViewModel GetSube(string ID)
        {
            UserViewModel model = new UserViewModel();

            //if (ID != "1")
            //{
            //    model.YetkiStatus = "1";
            //    model.YetkiStatusAciklama = "Yetkiniz Bulunmamaktadır";
            //    return model;
            //}

            var mf = new ModelFunctions();
            try
            {
                #region ŞubeListesi

                var frSubeList = new List<FR_SubeListesiViewModel>();

                mf.SqlConnOpen();
                DataTable dt1 = mf.DataTable("select * from SubeSettings");
                mf.SqlConnClose();

                foreach (DataRow r in dt1.Rows)
                {
                    FR_SubeListesiViewModel subeListModel = new FR_SubeListesiViewModel
                    {
                        SubeID = Convert.ToInt32(mf.RTS(r, "Id")),
                        SubeName = (mf.RTS(r, "SubeName"))
                    };
                    frSubeList.Add(subeListModel);
                }
                model.FR_SubeListesi = frSubeList;
                #endregion               
            }
            catch (Exception ex)
            {
                Singleton.WritingLogFile2("UsersListCRUDGetSube:", ex.Message.ToString(), "", ex.StackTrace);
            }

            return model;
        }

        public static UserViewModel YetkiliSubesi(string ID)
        {
            var model = new UserViewModel();
            var mf = new ModelFunctions();

            try
            {
                #region ŞubeListesi
                var yetkiliSubeList = new List<FR_SubeListesiViewModel>();

                mf.SqlConnOpen();
                DataTable dt1 = mf.DataTable("select * from UserSubeRelations Where UserId=" + ID);
                mf.SqlConnClose();

                foreach (DataRow r in dt1.Rows)
                {
                    FR_SubeListesiViewModel subeListModel = new FR_SubeListesiViewModel
                    {
                        SubeID = Convert.ToInt32(mf.RTS(r, "SubeID")),
                        SubeName = mf.RTS(r, "SubeName"),
                        UserID = mf.RTI(r, "UserID")
                    };
                    yetkiliSubeList.Add(subeListModel);
                }
                model.FR_SubeListesi = yetkiliSubeList;
                #endregion               
            }
            catch (Exception ex)
            {
                Singleton.WritingLogFile2("UsersListCRUDYetkiliSubesi:", ex.Message.ToString(), "", ex.StackTrace);
            }

            return model;
        }

        public static UserViewModel KullaniciSubeYetkiListesi(string kullaniciId)
        {
            var mf = new ModelFunctions();
            var model = new UserViewModel();
            var subeList = new List<FR_SubeListesiViewModel>();

            try
            {
                string sqlQuery = string.Empty;

                if (kullaniciId == "1")
                {
                    sqlQuery = (@"Select ss.ID as SubeID From SubeSettings ss Where ss.Status = 1");
                }
                else
                {
                    sqlQuery = (@"Select usr.ID,usr.UserID,usr.SubeID From UserSubeRelations  usr
                                              inner join SubeSettings ss on usr.SubeID = ss.ID
                                              Where ss.Status = 1 and  UserId =" + kullaniciId);
                }

                mf.SqlConnOpen();
                DataTable dt = mf.DataTable(sqlQuery);
                mf.SqlConnClose();

                foreach (DataRow r in dt.Rows)
                {
                    FR_SubeListesiViewModel sube = new FR_SubeListesiViewModel
                    {
                        SubeID = Convert.ToInt32(mf.RTS(r, "SubeID")),
                        UserID = kullaniciId == "1" ? 1 : mf.RTI(r, "UserID"),
                    };
                    subeList.Add(sube);
                }

                model.FR_SubeListesi = subeList;
            }
            catch (Exception ex)
            {
                model.ErrorMessages = "Kullanıcı yetkisi bulunamadı.";
                Singleton.WritingLogFile2("UsersListCRUDKullaniciSubeYetkiListesi", ex.Message, "", ex.StackTrace);
            }

            return model;
        }
    }
}