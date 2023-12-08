using SefimV2.ViewModels.SelfOutParametersSettings;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.Linq;
using System.Web;

namespace SefimV2.Models
{
    public class SelfOutParametersSettingsCRUD
    {
        public static List<SelfOutParametersSettingsViewModel> List()
        {
            List<SelfOutParametersSettingsViewModel> Liste = new List<SelfOutParametersSettingsViewModel>();
            ModelFunctions f = new ModelFunctions();
            DateTime startDate = DateTime.Now;
            try
            {
                f.SqlConnOpen();
                DataTable dt = f.DataTable("select * from SelfOutParameters");
                foreach (DataRow r in dt.Rows)
                {
                    SelfOutParametersSettingsViewModel model = new SelfOutParametersSettingsViewModel();
                    model.Id = f.RTI(r, "Id");
                    model.IsSelfSync = Convert.ToBoolean(f.RTS(r, "IsSelfSync"));
                    model.TableName = f.RTS(r, "TableName");
                    Liste.Add(model);
                }

                f.SqlConnClose();
            }
            catch (System.Exception ex) { }

            return Liste;
        }

        public static ActionResultMessages SelfOutParametersSettingsUpdate(SelfOutParametersSettingsViewModel course)
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

                string CmdString = "Update SelfOutParameters Set ";
                CmdString = CmdString + " IsSelfSync='" + course.IsSelfSync.ToString() + "' ";
                CmdString = CmdString + " where Id= '" + course.Id + "' ";
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
    }
}