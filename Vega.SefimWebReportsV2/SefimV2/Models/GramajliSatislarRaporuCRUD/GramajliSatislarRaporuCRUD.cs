using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Configuration;
using System.Web.Hosting;

namespace SefimV2.Models.GramajliSatislarRaporuCRUD
{
    public class GramajliSatislarRaporuCRUD
    {
        //static string SqlConnString = "Data Source=" + WebConfigurationManager.AppSettings["Server"] + ";Initial Catalog=VegaMasterRapor;Persist Security Info=True;User Id=" + WebConfigurationManager.AppSettings["User"] + ";Password=" + WebConfigurationManager.AppSettings["Password"] + "; MultipleActiveResultSets=true";
        static string SqlConnString = "Data Source=" + WebConfigurationManager.AppSettings["Server"] + ";Initial Catalog=" + WebConfigurationManager.AppSettings["DBName"] + ";Persist Security Info=True;User Id=" + WebConfigurationManager.AppSettings["User"] + ";Password=" + WebConfigurationManager.AppSettings["Password"] + "; MultipleActiveResultSets=true";
        public static string TavukDonerGramajlari(DateTime startDate, DateTime endDate, string SUBEADI)
        {
            return GenelSql("SQL1.sql",startDate,endDate,SUBEADI);
           
        }

        public static string EtDonerGramajlari(DateTime startDate, DateTime endDate, string SUBEADI)
        {
            return GenelSql("SQL2.sql", startDate, endDate, SUBEADI);

        }

        public static string ToplamDonerGramajlari(DateTime startDate, DateTime endDate, string SUBEADI)
        {
            return GenelSql("SQL3.sql", startDate, endDate, SUBEADI);

        }


        public static string GenelSql(string sqlPath, DateTime startDate, DateTime endDate, string SUBEADI)
        {
            if (SUBEADI != "")
                SUBEADI = ("'" + SUBEADI + "'").Replace(",", "','").Replace(",''", "");
            string Query = "";
            Query = File.ReadAllText(HostingEnvironment.MapPath("/Sql/SqlNewSef/GramajliSatislarRaporu/"+sqlPath), System.Text.UTF8Encoding.Default);
            Query = Query.Replace("{TARIH1}", startDate.ToString("yyyy-MM-dd 00:00:00"));
            Query = Query.Replace("{TARIH2}", endDate.ToString("yyyy-MM-dd 23:59:59"));

            Query = Query.Replace("{SUBEADI}", "");

            if (SUBEADI.Trim() == "")
            {
                Query = Query.Replace("#SUBEADIWHERE#", "");
            }
            else
            {
                Query = Query.Replace("#SUBEADIWHERE#", "AND SUBEADI in (" + SUBEADI + ")");

            }


            ModelFunctions f = new ModelFunctions();
            var dt = f.GetSubeDataWithQuery(SqlConnString, Query);
            return JsonConvert.SerializeObject(dt);

        }
    }
}