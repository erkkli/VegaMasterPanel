using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Configuration;
using System.Web.Hosting;

namespace SefimV2.Models.YanUrunlerRaporuCRUD
{
    public class YanUrunlerRaporuCRUD
    {
        //static string SqlConnString = "Data Source=" + WebConfigurationManager.AppSettings["Server"] + ";Initial Catalog=VegaMasterRapor;Persist Security Info=True;User Id=" + WebConfigurationManager.AppSettings["User"] + ";Password=" + WebConfigurationManager.AppSettings["Password"] + "; MultipleActiveResultSets=true";
        static string SqlConnString = "Data Source=" + WebConfigurationManager.AppSettings["Server"] + ";Initial Catalog=" + WebConfigurationManager.AppSettings["DBName"] + ";Persist Security Info=True;User Id=" + WebConfigurationManager.AppSettings["User"] + ";Password=" + WebConfigurationManager.AppSettings["Password"] + "; MultipleActiveResultSets=true";
       
        public static string SubelereGore(DateTime startDate, DateTime endDate, string TEKTRH, string SUBEADI)
        {
            return GenelSql("SQL1.sql",startDate,endDate,TEKTRH,SUBEADI);

        }


        public static string TariheGore(DateTime startDate, DateTime endDate, string TEKTRH, string SUBEADI)
        {
            return GenelSql("SQL2.sql", startDate, endDate, TEKTRH, SUBEADI);


        }
        public static string GenelSql(string sqlPath, DateTime startDate, DateTime endDate, string TEKTRH, string SUBEADI)
        {
            if (SUBEADI != "")
                SUBEADI = ("'" + SUBEADI + "'").Replace(",", "','").Replace(",''", "");
            string Query = "";
            Query = File.ReadAllText(HostingEnvironment.MapPath("/Sql/SqlNewSef/YanUrunlerRaporu/"+sqlPath), System.Text.UTF8Encoding.Default);
            Query = Query.Replace("{TARIH1}", startDate.ToString("yyyy-MM-dd 00:00:00"));
            Query = Query.Replace("{TARIH2}", endDate.ToString("yyyy-MM-dd 23:59:59"));

            if (SUBEADI.Trim() == "")
            {
                Query = Query.Replace("#SUBEADIWHERE#", "");
            }
            else
            {
                Query = Query.Replace("#SUBEADIWHERE#", "AND SUBEADI in (" + SUBEADI + ")");

            }
            if (TEKTRH.Trim() == "")
            {
                Query = Query.Replace("#TEKTRHWHERE#", "");
            }
            else
            {

                Query = Query.Replace("#TEKTRHWHERE#", "AND Date>'" + Convert.ToDateTime(TEKTRH).ToString("yyyy-MM-dd 00:00:00") + "' AND Date<'" + Convert.ToDateTime(TEKTRH).ToString("yyyy-MM-dd 23:59:59") + "'");

            }

            ModelFunctions f = new ModelFunctions();
            var dt = f.GetSubeDataWithQuery(SqlConnString, Query);
            return JsonConvert.SerializeObject(dt);

        }

    }
}