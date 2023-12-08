using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Configuration;
using System.Web.Hosting;

namespace SefimV2.Models.SaatlereGoreSatislarRaporuCRUD
{
    public class SaatlereGoreSatislarRaporuCRUD
    {
        //static string SqlConnString = "Data Source=" + WebConfigurationManager.AppSettings["Server"] + ";Initial Catalog=VegaMasterRapor;Persist Security Info=True;User Id=" + WebConfigurationManager.AppSettings["User"] + ";Password=" + WebConfigurationManager.AppSettings["Password"] + "; MultipleActiveResultSets=true";
        static string SqlConnString = "Data Source=" + WebConfigurationManager.AppSettings["Server"] + ";Initial Catalog=" + WebConfigurationManager.AppSettings["DBName"] + ";Persist Security Info=True;User Id=" + WebConfigurationManager.AppSettings["User"] + ";Password=" + WebConfigurationManager.AppSettings["Password"] + "; MultipleActiveResultSets=true";


        public static string OzetBilgi(DateTime startDate, DateTime endDate, string TEKTRH, string MinDate, string MaxDate)
        {
            string Query = "";
            Query = File.ReadAllText(HostingEnvironment.MapPath("/Sql/SqlNewSef/SqlSaatlereGoreSatislarRaporu/SQL1.sql"), System.Text.UTF8Encoding.Default);
            Query = Query.Replace("{TARIH1}", startDate.ToString("yyyy-MM-dd 00:00:00"));
            Query = Query.Replace("{TARIH2}", endDate.ToString("yyyy-MM-dd 23:59:59"));
            if (MaxDate == "Invalid Date" || MinDate == "Invalid Date")
            {
                Query = Query.Replace("#BELİRLİSAATLER#", "");

            }
            else
            {
                Query = Query.Replace("#BELİRLİSAATLER#", "AND (CONVERT(TIME, Date)>=CONVERT(TIME, '" + Convert.ToDateTime(MinDate).AddHours(-3).ToString("HH:mm:ss") + "')) AND (CONVERT(TIME, Date)<=CONVERT(TIME, '" + Convert.ToDateTime(MaxDate).AddHours(-3).ToString("HH:mm:ss") + "'))");
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
        public static string SaatlereGoreSatisTutari(DateTime startDate, DateTime endDate, string TEKTRH, string MinDate, string MaxDate)
        {
            string Query = "";
            Query = File.ReadAllText(HostingEnvironment.MapPath("/Sql/SqlNewSef/SqlSaatlereGoreSatislarRaporu/SQL3.sql"), System.Text.UTF8Encoding.Default);
            Query = Query.Replace("{TARIH1}", startDate.ToString("yyyy-MM-dd 00:00:00"));
            Query = Query.Replace("{TARIH2}", endDate.ToString("yyyy-MM-dd 23:59:59"));
            if (MaxDate == "Invalid Date" || MinDate == "Invalid Date")
            {
                Query = Query.Replace("#BELİRLİSAATLER#", "");

            }
            else
            {
                Query = Query.Replace("#BELİRLİSAATLER#", "AND (CONVERT(TIME, Date)>=CONVERT(TIME, '" + Convert.ToDateTime(MinDate).AddHours(-3).ToString("HH:mm:ss") + "')) AND (CONVERT(TIME, Date)<=CONVERT(TIME, '" + Convert.ToDateTime(MaxDate).AddHours(-3).ToString("HH:mm:ss") + "'))");
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
        public static string SaatlereGoreSatisMiktari(DateTime startDate, DateTime endDate, string TEKTRH, string MinDate, string MaxDate)
        {
            string Query = "";
            Query = File.ReadAllText(HostingEnvironment.MapPath("/Sql/SqlNewSef/SqlSaatlereGoreSatislarRaporu/SQL2.sql"), System.Text.UTF8Encoding.Default);
            Query = Query.Replace("{TARIH1}", startDate.ToString("yyyy-MM-dd 00:00:00"));
            Query = Query.Replace("{TARIH2}", endDate.ToString("yyyy-MM-dd 23:59:59"));
            if (MaxDate == "Invalid Date" || MinDate == "Invalid Date")
            {
                Query = Query.Replace("#BELİRLİSAATLER#", "");

            }
            else
            {
                Query = Query.Replace("#BELİRLİSAATLER#", "AND (CONVERT(TIME, Date)>=CONVERT(TIME, '" + Convert.ToDateTime(MinDate).AddHours(-3).ToString("HH:mm:ss") + "')) AND (CONVERT(TIME, Date)<=CONVERT(TIME, '" + Convert.ToDateTime(MaxDate).AddHours(-3).ToString("HH:mm:ss") + "'))");
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