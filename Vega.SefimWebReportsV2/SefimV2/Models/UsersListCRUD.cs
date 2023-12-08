using SefimV2.ViewModels.User;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using static SefimV2.ViewModels.User.UserViewModel;

namespace SefimV2.Models
{
    public class UsersListCRUD
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

                    if (f.RTS(r, "UserName") != "Admin")
                    {
                        model.ID = Convert.ToInt32(f.RTS(r, "ID"));
                        model.IsAdmin = 1;//Convert.ToInt32(f.RTS(r, "IsAdmin"));
                        model.UserName = f.RTS(r, "UserName");
                        model.Name = f.RTS(r, "Name");
                        model.Password = f.RTS(r, "Password");
                        model.Gsm = f.RTS(r, "Gsm");
                        model.EMail = f.RTS(r, "EMail");

                        model.Status = Convert.ToBoolean(f.RTS(r, "Status"));
                        Liste.Add(model);
                    }
                }

                //UserViewModel model = new UserViewModel();
                f.SqlConnClose();
            }
            catch (System.Exception ex)
            { }

            return Liste;
        }


        public mdlActionResult UserProfileInsert(UserViewModel course, int[] Auth)
        {

            ModelFunctions f = new ModelFunctions();
            mdlActionResult result = new mdlActionResult();
            result.result_STR = "Başarısız";
            result.result_INT = 0;
            result.result_BOOL = false;
            result.result_OBJECT = null;
            string resut = "true";
            int SonKayitID = 0;


            try
            {
                string sub = "12";
                f.SqlConnOpen();
                string TimeStamp = DateTime.Now.ToFileTimeUtc().ToString();
                string CmdString =

                    " Insert Into Users(CreateDate, CreateDate_Timestamp, ModifyCounter, UpdateDate, UpdateDate_Timestamp, UserName, Password, IsAdmin, SubeID, Name, EMail, Gsm, Status)" +
                    "Values(" +
                    "getdate()," +
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
                        "'1'" + "   )";
                OleDbCommand Cmd = new OleDbCommand(CmdString, f.ConnOle);
                Cmd.ExecuteNonQuery();
                f.SqlConnClose();
            }
            catch (OleDbException ex) { resut = ex.Message.ToString(); result.IsSuccess = false; result.UserMessage = ex.ToString(); return result; }


            #region SON KAYIT ID 
            f.SqlConnOpen();
            DataTable dt1 = f.DataTable("  SELECT top 1 * FROM Users order by ID desc");
            foreach (DataRow r in dt1.Rows)
            {
                SonKayitID = Convert.ToInt32(f.RTS(r, "ID"));
            }
            f.SqlConnClose();
            #endregion


            #region YETKI INSERT           
            try
            {
                f.SqlConnOpen();

                FR_SubeListesiViewModel model = new FR_SubeListesiViewModel();
                foreach (var item in Auth)
                {
                    //string aa = item.SubeID.ToString();
                    //string bb = item.SubeName;
                    model.SubeID = item;

                    string TimeStamp = DateTime.Now.ToFileTimeUtc().ToString();
                    string CmdString =

                        " Insert Into UserSubeRelations(CreateDate, CreateDate_Timestamp, ModifyCounter, UpdateDate, UpdateDate_Timestamp, UserID, SubeID)" +
                        "Values(" +
                        "getdate()," +
                       "''," +
                        " '0'," +
                        "''," +
                        "''," +
                        "'" + SonKayitID.ToString() + "' ," +
                        "'" + item + "'" + "   )";
                    OleDbCommand Cmd = new OleDbCommand(CmdString, f.ConnOle);
                    Cmd.ExecuteNonQuery();

                }

                f.SqlConnClose();
            }
            catch (OleDbException ex) { resut = ex.Message.ToString(); result.IsSuccess = false; result.UserMessage = ex.ToString(); return result; }
            #endregion


            result.result_STR = "Başarılı";
            result.result_INT = 1;
            result.result_BOOL = true;
            //result.result_OBJECT = SaatListe();
            result.IsSuccess = true;
            result.UserMessage = "Başarılı";
            return result;
        }


        internal mdlActionResult UserProfileUpdate(UserViewModel course, int[] Auth)
        {
            ModelFunctions f = new ModelFunctions();
            mdlActionResult result = new mdlActionResult();
            result.result_STR = "Başarısız";
            result.result_INT = 0;
            result.result_BOOL = false;
            result.result_OBJECT = null;
            string resut = "true";


            #region Sube Yetki Listesi
            //try
            //{
            //    f.SqlConnOpen();
            //    //DataTable dt = f.DataTable("select * from Users where ID=" + id + " ");
            //    #region ŞubeListesi
            //    List<FR_SubeListesiViewModel> frSubeList = new List<FR_SubeListesiViewModel>();
            //    DataTable dt1 = f.DataTable("select * from SubeSettings Where ID="+course.ID );
            //    foreach (DataRow r in dt1.Rows)
            //    {
            //        course.YetkiListesi += (f.RTS(r, "SubeID")) + ",";
            //    }
            //    //model.FR_SubeListesi = frSubeList;
            //    #endregion
            //    f.SqlConnClose();
            //}
            //catch (System.Exception ex)
            //{ }

            #region gUNCELLENECEK KAYIT ONCE SILINIR 

            try
            {
                f.SqlConnOpen();
                DataTable dt = f.DataTable("delete from UserSubeRelations where UserID= " + course.ID + "");
                f.SqlConnClose();
            }
            catch (OleDbException ex)
            {
                resut = ex.Message.ToString();
            }
            #endregion


            #region YETKI INSERT           
            try
            {
                f.SqlConnOpen();

                FR_SubeListesiViewModel model = new FR_SubeListesiViewModel();
                foreach (var item in Auth)
                {
                    //string aa = item.SubeID.ToString();
                    //string bb = item.SubeName;
                    model.SubeID = item;
                    string TimeStamp = DateTime.Now.ToFileTimeUtc().ToString();
                    string CmdString =

                        " Insert Into UserSubeRelations(CreateDate, CreateDate_Timestamp, ModifyCounter, UpdateDate, UpdateDate_Timestamp, UserID, SubeID)" +
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
            catch (OleDbException ex) { resut = ex.Message.ToString(); result.IsSuccess = false; result.UserMessage = ex.ToString(); return result; }
            #endregion


            #region GUNCELLE         
            //try
            //{
            //    //" Insert Into UserSubeRelations(CreateDate, CreateDate_Timestamp, ModifyCounter, UpdateDate, UpdateDate_Timestamp, UserID, SubeID)" +
            //    f.SqlConnOpen();
            //    foreach (var item in Auth)
            //    {
            //        string TimeStamp = DateTime.Now.ToFileTimeUtc().ToString();
            //        string CmdString =
            //            "Update UserSubeRelations Set " +
            //            " CreateDate = getDate()," +
            //            "CreateDate_Timestamp = '0'," +
            //            "ModifyCounter = 0," +
            //            "UpdateDate = getdate()," +
            //            "UpdateDate_Timestamp = ''," +
            //            "UserID = '" + course.ID + "'," +
            //            "SubeID = '" + item + "' WHERE UserID ='" + course.ID + "' ";

            //        OleDbCommand Cmd = new OleDbCommand(CmdString, f.ConnOle);
            //        Cmd.ExecuteNonQuery();
            //    }
            //    f.SqlConnClose();
            //}
            //catch (OleDbException ex) { resut = ex.Message.ToString(); result.IsSuccess = false; result.UserMessage = ex.ToString(); return result; }
            #endregion


            #endregion

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
                    "SubeID = '" + course.SubeID + "'," +
                    "Name = '" + course.Name + "'," +
                    "EMail = '" + course.EMail + "'," +
                    "Gsm = '" + course.Gsm + "'," +
                    "Status = '" + course.Status + "' WHERE ID ='" + course.ID + "' ";

                OleDbCommand Cmd = new OleDbCommand(CmdString, f.ConnOle);
                Cmd.ExecuteNonQuery();
                f.SqlConnClose();
            }
            catch (OleDbException ex) { resut = ex.Message.ToString(); result.IsSuccess = false; result.UserMessage = ex.ToString(); return result; }
            result.result_STR = "Başarılı";
            result.result_INT = 1;
            result.result_BOOL = true;
            result.IsSuccess = true;
            result.UserMessage = "Başarılı";
            //result.result_OBJECT = SaatListe();
            return result;
        }


        internal mdlActionResult UserProfileDelete(int course)
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
                DataTable dt = f.DataTable("delete from Users where ID= " + course + "");
                f.SqlConnClose();

            }
            catch (OleDbException ex)
            {
                resut = ex.Message.ToString();
            }
            result.result_STR = "Başarılı";
            result.result_INT = 1;
            result.result_BOOL = true;
            //result.result_OBJECT = SaatListe();
            return result;
        }


        public static UserViewModel GetUser(int id, string ID)
        {
            UserViewModel model = new UserViewModel();

            if (ID != "1")
            {
                model.YetkiStatus = "1";
                model.YetkiStatusAciklama = "Yetkiniz Bulunmamaktadır";
                return model;
            }

            ModelFunctions f = new ModelFunctions();
            DateTime startDate = DateTime.Now;
            try
            {
                f.SqlConnOpen();
                DataTable dt = f.DataTable("select * from Users where ID=" + id + " ");

                foreach (DataRow r in dt.Rows)
                {
                    model.ID = Convert.ToInt32(f.RTS(r, "ID"));
                    model.IsAdmin = 1;//Convert.ToInt32(f.RTS(r, "IsAdmin"));
                    model.UserName = f.RTS(r, "UserName");
                    model.Name = f.RTS(r, "Name");
                    model.Password = f.RTS(r, "Password");
                    model.Gsm = f.RTS(r, "Gsm");
                    model.EMail = f.RTS(r, "EMail");
                    model.SubeID = Convert.ToInt32(f.RTS(r, "SubeID"));
                    model.Status = Convert.ToBoolean(f.RTS(r, "Status"));
                }


                #region ŞubeListesi
                List<FR_SubeListesiViewModel> frSubeList = new List<FR_SubeListesiViewModel>();
                DataTable dt1 = f.DataTable("select * from SubeSettings");
                foreach (DataRow r in dt1.Rows)
                {

                    FR_SubeListesiViewModel subeListModel = new FR_SubeListesiViewModel();
                    subeListModel.SubeID = Convert.ToInt32(f.RTS(r, "ID"));
                    subeListModel.SubeName = (f.RTS(r, "SubeName"));
                    frSubeList.Add(subeListModel);
                }
                model.FR_SubeListesi = frSubeList;


                #endregion



                f.SqlConnClose();
            }
            catch (System.Exception ex)
            { }
            return model;
        }



        private static List<SelectListItem> SubeList()
        {
            List<SelectListItem> items = new List<SelectListItem>();




            ModelFunctions f = new ModelFunctions();
            //DateTime startDate = DateTime.Now;
            try
            {
                f.SqlConnOpen();
                DataTable dt = f.DataTable("select  ID, SubeName from SubeSettings");

                foreach (DataRow r in dt.Rows)
                {
                    UserViewModel model = new UserViewModel();

                    items.Add(new SelectListItem
                    {
                        Text = f.RTS(r, "SubeName").ToString(),
                        Value = f.RTS(r, "ID").ToString(),
                    });
                }

                f.SqlConnClose();
            }
            catch (System.Exception ex)
            { }

            return items;


        }





        public static UserViewModel GetSube(string ID)
        {
            UserViewModel model = new UserViewModel();


            if (ID != "1")
            {
                model.YetkiStatus = "1";
                model.YetkiStatusAciklama = "Yetkiniz Bulunmamaktadır";
                return model;
            }

            ModelFunctions f = new ModelFunctions();
            DateTime startDate = DateTime.Now;
            try
            {
                f.SqlConnOpen();
                //DataTable dt = f.DataTable("select * from Users where ID=" + id + " ");

                #region ŞubeListesi
                List<FR_SubeListesiViewModel> frSubeList = new List<FR_SubeListesiViewModel>();
                DataTable dt1 = f.DataTable("select * from SubeSettings");
                foreach (DataRow r in dt1.Rows)
                {

                    FR_SubeListesiViewModel subeListModel = new FR_SubeListesiViewModel();
                    subeListModel.SubeID = Convert.ToInt32(f.RTS(r, "ID"));
                    subeListModel.SubeName = (f.RTS(r, "SubeName"));
                    frSubeList.Add(subeListModel);
                }
                model.FR_SubeListesi = frSubeList;
                #endregion

                f.SqlConnClose();
            }
            catch (System.Exception ex)
            { }
            return model;
        }

        public static UserViewModel YetkiliSubesi(string ID)
        {
            UserViewModel model = new UserViewModel();

            ModelFunctions f = new ModelFunctions();
            DateTime startDate = DateTime.Now;
            try
            {
                f.SqlConnOpen();
                //DataTable dt = f.DataTable("select * from Users where ID=" + id + " ");

                #region ŞubeListesi
                List<FR_SubeListesiViewModel> frSubeList = new List<FR_SubeListesiViewModel>();
                DataTable dt1 = f.DataTable("select * from UserSubeRelations Where UserId=" + ID);
                foreach (DataRow r in dt1.Rows)
                {
                    FR_SubeListesiViewModel subeListModel = new FR_SubeListesiViewModel();
                    subeListModel.SubeID = Convert.ToInt32(f.RTS(r, "SubeID"));
                    subeListModel.SubeName = (f.RTS(r, "SubeName"));
                    frSubeList.Add(subeListModel);
                }
                model.FR_SubeListesi = frSubeList;
                #endregion

                f.SqlConnClose();
            }
            catch (System.Exception ex)
            { }
            return model;

        }

    }
}