using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;

namespace SefimV2.Models
{
    public class SubeSettings
    {
        public int id { get; set; }
        public DateTime CreateDate { get; set; }
        public int CreateDate_Timestamp { get; set; }
        public int ModifyCounter { get; set; }
        public DateTime UpdateDate { get; set; }
        public int UpdateDate_Timestamp { get; set; }
        public string SubeName { get; set; }
        public string SubeIP { get; set; }
        public string SqlName { get; set; }
        public string SqlPassword { get; set; }
        public string DBName { get; set; }
        public string FirmaID { get; set; }
        public string DonemID { get; set; }
        public string DepoID { get; set; }

        public string ZimmetFirmaAdi { get; set; }
        public string ZimmetFirmaKodu { get; set; }
        public string ZimmetIND { get; set; }
        public string ZimmetFiyat { get; set; }

        public SubeSettings() { }

        public static List<SubeSettings> List()
        {
            ModelFunctions f = new ModelFunctions();
            List<SubeSettings> liste = new List<SubeSettings>();
            try
            {
                f.SqlConnOpen();
                string CmdString = "Select * from SubeSettings";
                OleDbCommand Cmd = new OleDbCommand(CmdString, f.ConnOle);
                OleDbDataReader Dr = Cmd.ExecuteReader();
                while (Dr.Read())
                {
                    SubeSettings item = new SubeSettings();
                    item.id = f.ToInt1(Dr["id"].ToString());
                    item.CreateDate = f.ToDateTime(Dr["CreateDate"].ToString());
                    item.CreateDate_Timestamp = f.ToInt(Dr["CreateDate_Timestamp"].ToString());
                    item.ModifyCounter = f.ToInt(Dr["ModifyCounter"].ToString());
                    item.UpdateDate = f.ToDateTime(Dr["UpdateDate"].ToString());
                    item.SubeName = Dr["SubeName"].ToString();
                    item.SubeIP = Dr["SubeIP"].ToString();
                    item.SqlName = Dr["SqlName"].ToString();
                    item.SqlPassword = Dr["SqlPassword"].ToString();
                    item.FirmaID = Dr["FirmaID"].ToString();
                    item.DonemID = Dr["DonemID"].ToString();
                    item.DepoID = Dr["DepoID"].ToString();
                    liste.Add(item);
                }
                f.SqlConnClose();
            }
            catch { liste = null; }

            return liste;

        }

        public static int RowCount(string query = "")
        {
            ModelFunctions f = new ModelFunctions();
            int result = 0;
            try
            {
                f.SqlConnOpen();
                string CmdString = "Select count(*) as 'toplam' from SubeSettings ";
                if (query != null && query != "") { CmdString = CmdString + " where " + query + " "; }
                OleDbCommand Cmd = new OleDbCommand(CmdString, f.ConnOle);
                OleDbDataReader Dr = Cmd.ExecuteReader();
                while (Dr.Read())
                {
                    result = f.ToInt(Dr["toplam"].ToString());
                }
                f.SqlConnClose();
            }
            catch { result = 0; }

            return result;

        }

        public static string Add(SubeSettings prms)
        {
            ModelFunctions f = new ModelFunctions();
            string resut = "true";
            try
            {
                f.SqlConnOpen();
                string TimeStamp = DateTime.Now.ToFileTimeUtc().ToString();
                string CmdString = "Insert Into SubeSettings(CreateDate,CreateDate_Timestamp,SubeName,SubeIP,SqlName,SqlPassword,FirmaID,DonemID,DepoID)" +
                    "Values(" +
                    "getdate() , " +
                    "'" + TimeStamp + "' , " +
                    "'" + prms.SubeName + "' , " +
                    "'" + prms.SubeIP + "' , " +
                    "'" + prms.SqlName + "' , " +
                    "'" + prms.SqlPassword + "' , " +
                    "'" + prms.FirmaID + "' , " +
                    "'" + prms.DonemID + "' , " +
                    "'" + prms.DepoID + "'  " +
                    ")";
                OleDbCommand Cmd = new OleDbCommand(CmdString, f.ConnOle);
                Cmd.ExecuteNonQuery();
                f.SqlConnClose();
            }
            catch (OleDbException ex) { resut = ex.Message.ToString(); }

            return resut;

        }

        public static string Update(SubeSettings prms, string ID)
        {

            ModelFunctions f = new ModelFunctions();
            string resut = "true";
            try
            {
                f.SqlConnOpen();

                int ModifyCounter = 1;
                string TimeStamp = DateTime.Now.ToFileTimeUtc().ToString();
                OleDbCommand CmdModifyCounter = new OleDbCommand("select ModifyCounter from SubeSettings where id='" + ID + "'", f.ConnOle);
                OleDbDataReader RdrModifyCounter = CmdModifyCounter.ExecuteReader();
                while (RdrModifyCounter.Read())
                {
                    ModifyCounter = f.ToInt(RdrModifyCounter["ModifyCounter"].ToString()) + 1;
                }

                string CmdString = "Update SubeSettings Set " +
                "UpdateDate=getdate() , " +
                "UpdateDate_Timestamp='" + TimeStamp + "' , " +
                "ModifyCounter='" + ModifyCounter.ToString() + "'  ";
                if (prms.SubeName != null) { CmdString = CmdString + " , SubeName='" + prms.SubeName + "' "; }
                if (prms.SubeIP != null) { CmdString = CmdString + " , SubeIP='" + prms.SubeIP + "' "; }
                if (prms.SqlName != null) { CmdString = CmdString + " , SqlName='" + prms.SqlName + "' "; }
                if (prms.SqlPassword != null) { CmdString = CmdString + " , SqlPassword='" + prms.SqlPassword + "' "; }
                if (prms.FirmaID != null) { CmdString = CmdString + " , FirmaID='" + prms.FirmaID + "' "; }
                if (prms.DonemID != null) { CmdString = CmdString + " , DonemID='" + prms.DonemID + "' "; }
                if (prms.DepoID != null) { CmdString = CmdString + " , DepoID='" + prms.DepoID + "' "; }
                CmdString = CmdString + " where id= '" + ID + "' ";
                OleDbCommand Cmd = new OleDbCommand(CmdString, f.ConnOle);
                Cmd.ExecuteNonQuery();
                f.SqlConnClose();
            }
            catch (OleDbException ex) { resut = ex.Message.ToString(); }

            return resut;

        }

        public static string FasterKasaAdi(string IND, int TanimId)
        {
            ModelFunctions f = new ModelFunctions();
            string KasaAdi = string.Empty;
            try
            {
                #region ENVANTER DB BAGLANIP GEREKLİ BILGILER ALIIYOR                 
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
                    Query = "SELECT * FROM F0" + TanimId + "TBLKRDSUBELER where IND=" + IND;//File.ReadAllText(HostingEnvironment.MapPath("/Sql/SqlVegaDb/Firma.sql"), System.Text.UTF8Encoding.Default);
                    string connString = "Provider=SQLOLEDB;Server=" + VegaDbIp + ";User Id=" + VegaDbSqlName + ";Password=" + VegaDbSqlPassword + ";Database=" + VegaDbName + "";
                    f.SqlConnOpen(true, connString);
                    DataTable AcikHesapDt = f.DataTable(Query, true);
                    if (AcikHesapDt.Rows.Count > 0)
                    {
                        foreach (DataRow SubeR in AcikHesapDt.Rows)
                        {
                            KasaAdi = (f.RTS(SubeR, "SUBEADI"));
                        }
                    }
                }
                f.SqlConnClose();
                #endregion
            }
            catch (System.Exception ex) { }

            return KasaAdi;
        }


        #region Zimmet cari list
       
        public static List<SubeSettings> SefimPanelZimmetCagrisi(string IND, int TanimId)
        {
            ModelFunctions f = new ModelFunctions();
            List<SubeSettings> list = new List<SubeSettings>();
            try
            {
                #region ENVANTER DB BAGLANIP GEREKLİ BILGILER ALIIYOR                 
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
                    Query = " SELECT IND,FIRMAKODU,FIRMAADI,ZIMFIYAT FROM  F0"+TanimId+"TBLCARI WHERE FIRMATIPI=9 ";
                    string connString = "Provider=SQLOLEDB;Server=" + VegaDbIp + ";User Id=" + VegaDbSqlName + ";Password=" + VegaDbSqlPassword + ";Database=" + VegaDbName + "";
                    f.SqlConnOpen(true, connString);
                    DataTable zimmetCagriDt = f.DataTable(Query, true);
                    if (zimmetCagriDt.Rows.Count > 0)
                    {
                        foreach (DataRow SubeR in zimmetCagriDt.Rows)
                        {
                            SubeSettings model = new SubeSettings();
                            model.ZimmetIND = (f.RTS(SubeR, "IND"));
                            model.ZimmetFirmaKodu = f.RTS(SubeR, "FIRMAADI");
                            list.Add(model);
                        }
                    }
                }
                f.SqlConnClose();
                #endregion
            }
            catch (Exception ex) { }

            return list;
        }

        #endregion Zimmet cari list
    }
}