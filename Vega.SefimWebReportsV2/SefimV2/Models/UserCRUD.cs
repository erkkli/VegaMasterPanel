using SefimV2.ViewModels.User;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.Linq;
using System.Web;

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
                        model.ID = Convert.ToInt32(f.RTS(r, "ID"));
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
            catch (System.Exception ex)
            {

            }

            return Liste;
        }

        internal mdlActionResult UserProfileUpdate(UserViewModel course)
        {
            course.UserName = "Admin";

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
                    "Status = '" + 1 + "' WHERE ID ='" + course.ID + "' ";

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

        public mdlActionResult UserProfileInsert(UserViewModel course)
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
                string CmdString =

                    " Insert Into Users(CreateDate, CreateDate_Timestamp, ModifyCounter, UpdateDate, UpdateDate_Timestamp, UserName, Password, IsAdmin, SubeID, Name, EMail, Gsm, Status)" +
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
                        "'1'" + "   )";
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
                    //model.Status = Convert.ToInt32(f.RTS(r, "Status"));
                }

                f.SqlConnClose();
            }
            catch (System.Exception ex)
            {
            }
            return model;
        }
    }
}