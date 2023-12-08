using SefimV2.ViewModels.VegaDBSettings;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.Linq;
using System.Web;

namespace SefimV2.Models
{
    public class VegaDBSettingsCRUD
    {
        public static List<VegaDBSettingsViewModel> List()
        {
            List<VegaDBSettingsViewModel> Liste = new List<VegaDBSettingsViewModel>();
            ModelFunctions f = new ModelFunctions();
            DateTime startDate = DateTime.Now;
            try
            {
                f.SqlConnOpen();
                DataTable dt = f.DataTable("select * from VegaDbSettings");
                foreach (DataRow r in dt.Rows)
                {
                    VegaDBSettingsViewModel model = new VegaDBSettingsViewModel();

                    model.ID = Convert.ToInt32(f.RTS(r, "Id"));
                    model.IP = f.RTS(r, "IP");
                    model.SqlName = f.RTS(r, "SqlName");
                    model.SqlPassword = f.RTS(r, "SqlPassword");
                    model.DBName = f.RTS(r, "DBName");
                    Liste.Add(model);
                }

                f.SqlConnClose();
            }
            catch (System.Exception ex) { }

            return Liste;
        }

        public ActionResultMessages VegaDBSettingsInsert(VegaDBSettingsViewModel course)
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
                string CmdString = "Insert Into VegaDBSettings(CreateDate,CreateDate_Timestamp,IP,SqlName,SqlPassword,DBName)" +
                    "Values(" +
                    "getdate() , " +
                    "'" + TimeStamp + "' , " +
                    "'" + course.IP + "' , " +
                    "'" + course.SqlName + "' , " +
                    "'" + course.SqlPassword + "' , " +
                    "'" + course.DBName + "' " +
                    //"'" + course.Status + "'  " +
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

        internal ActionResultMessages VegaDBSettingsUpdate(VegaDBSettingsViewModel course)
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

                int ModifyCounter = 1;
                string TimeStamp = DateTime.Now.ToFileTimeUtc().ToString();
                OleDbCommand CmdModifyCounter = new OleDbCommand("select ModifyCounter from VegaDBSettings where Id='" + course.ID + "'", f.ConnOle);
                OleDbDataReader RdrModifyCounter = CmdModifyCounter.ExecuteReader();
                while (RdrModifyCounter.Read())
                {
                    ModifyCounter = f.ToInt(RdrModifyCounter["ModifyCounter"].ToString()) + 1;
                }

                string CmdString = "Update VegaDBSettings Set " +
                "UpdateDate=getdate() , " +
                "UpdateDate_Timestamp='" + TimeStamp + "' , " +
                "ModifyCounter='" + ModifyCounter.ToString() + "'  ";
                if (course.IP != null) { CmdString = CmdString + " , IP='" + course.IP + "' "; }
                if (course.SqlName != null) { CmdString = CmdString + " , SqlName='" + course.SqlName + "' "; }
                if (course.SqlPassword != null) { CmdString = CmdString + " , SqlPassword='" + course.SqlPassword + "' "; }
                if (course.DBName != null) { CmdString = CmdString + " , DBName='" + course.DBName + "' "; }
                CmdString = CmdString + " where Id= '" + course.ID + "' ";
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

        internal ActionResultMessages VegaDBSettingsDelete(int course)
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
                DataTable dt = f.DataTable("delete from VegaDBSettings where Id= " + course + "");
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

        public static VegaDBSettingsViewModel GetUser(int id)
        {
            VegaDBSettingsViewModel model = new VegaDBSettingsViewModel();

            ModelFunctions f = new ModelFunctions();
            DateTime startDate = DateTime.Now;
            try
            {
                f.SqlConnOpen();
                DataTable dt = f.DataTable("Select * from VegaDBSettings where Id=" + id + " ");

                foreach (DataRow r in dt.Rows)
                {
                    model.ID = Convert.ToInt32(f.RTS(r, "Id"));
                    model.IP = f.RTS(r, "IP");
                    model.SqlName = f.RTS(r, "SqlName");
                    model.SqlPassword = f.RTS(r, "SqlPassword");
                    model.DBName = f.RTS(r, "DBName");
                }
                f.SqlConnClose();
            }
            catch (System.Exception ex) { }
            return model;
        }
    }
}