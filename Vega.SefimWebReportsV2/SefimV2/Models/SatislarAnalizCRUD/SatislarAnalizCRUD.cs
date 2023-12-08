using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Configuration;
using System.Web.Hosting;

namespace SefimV2.Models.SatislarAnalizCRUD
{
    public class SatislarAnalizCRUD
    {
        //static string SqlConnString = "Data Source=" + WebConfigurationManager.AppSettings["Server"] + ";Initial Catalog=VegaMasterRapor;Persist Security Info=True;User Id=" + WebConfigurationManager.AppSettings["User"] + ";Password=" + WebConfigurationManager.AppSettings["Password"] + "; MultipleActiveResultSets=true";
        static string SqlConnString = "Data Source=" + WebConfigurationManager.AppSettings["Server"] + ";Initial Catalog=" + WebConfigurationManager.AppSettings["DBName"] + ";Persist Security Info=True;User Id=" + WebConfigurationManager.AppSettings["User"] + ";Password=" + WebConfigurationManager.AppSettings["Password"] + "; MultipleActiveResultSets=true";

        public static string OzetBilgi(DateTime startDate, DateTime endDate, string KOD1, string ProductGroup, string URUNADI, string SUBEADI, string MinDate, string MaxDate)
        {
            return GenelSql("SQL1.sql", startDate, endDate, KOD1, ProductGroup, URUNADI, SUBEADI, MinDate, MaxDate);
        }
        public static string GruplaraGoreSatislar(DateTime startDate, DateTime endDate, string KOD1, string ProductGroup, string URUNADI, string SUBEADI, string MinDate, string MaxDate)
        {
            return GenelSql("SQL2.sql", startDate, endDate, KOD1, ProductGroup, URUNADI, SUBEADI, MinDate, MaxDate);

        }
        public static string UrunlereGoreSatislar(DateTime startDate, DateTime endDate, string KOD1, string ProductGroup, string URUNADI, string SUBEADI, string MinDate, string MaxDate)
        {
            return GenelSql("SQL3.sql", startDate, endDate, KOD1, ProductGroup, URUNADI, SUBEADI, MinDate, MaxDate);

        }
        public static string DonerCesitlerineGoreSatislar(DateTime startDate, DateTime endDate, string KOD1, string ProductGroup, string URUNADI, string SUBEADI, string MinDate, string MaxDate)
        {
            return GenelSql("SQL5.sql", startDate, endDate, KOD1, ProductGroup, URUNADI, SUBEADI, MinDate, MaxDate);

        }
        public static string SubelereGoreSatislar(DateTime startDate, DateTime endDate, string KOD1, string ProductGroup, string URUNADI, string SUBEADI, string MinDate, string MaxDate)
        {
            return GenelSql("SQL6.sql", startDate, endDate, KOD1, ProductGroup, URUNADI, SUBEADI, MinDate, MaxDate);

        }
        public static string SaateGoreSatislar(DateTime startDate, DateTime endDate, string KOD1, string ProductGroup, string URUNADI, string SUBEADI, string MinDate, string MaxDate)
        {
            return GenelSql("SQL7.sql", startDate, endDate, KOD1, ProductGroup, URUNADI, SUBEADI, MinDate, MaxDate);

        }
        public static string GunlereGoreSatislar(DateTime startDate, DateTime endDate, string KOD1, string ProductGroup, string URUNADI, string SUBEADI, string MinDate, string MaxDate)
        {
            return GenelSql("SQL8.sql", startDate, endDate, KOD1, ProductGroup, URUNADI, SUBEADI, MinDate, MaxDate);

        }
        public static string GenelSql(string sqlPath, DateTime startDate, DateTime endDate, string KOD1, string ProductGroup, string URUNADI, string SUBEADI, string MinDate, string MaxDate)
        {
            string Query = "";
            if (ProductGroup != "")
                ProductGroup = ("'" + ProductGroup + "'").Replace(",", "','").Replace(",''", "");
            if (URUNADI != "")
                URUNADI = ("'" + URUNADI + "'").Replace(",", "','").Replace(",''", "");
            if (SUBEADI != "")
                SUBEADI = ("'" + SUBEADI + "'").Replace(",", "','").Replace(",''", "");
            if (KOD1 != "")
                KOD1 = ("'" + KOD1 + "'").Replace(",", "','").Replace(",''", "");
            Query = File.ReadAllText(HostingEnvironment.MapPath("/Sql/SqlNewSef/SqlSatislarAnalizRaporu/" + sqlPath), System.Text.UTF8Encoding.Default);
            Query = Query.Replace("{TARIH1}", startDate.ToString("yyyy-MM-dd 00:00:00"));
            Query = Query.Replace("{TARIH2}", endDate.ToString("yyyy-MM-dd 23:59:59"));
            Query = Query.Replace("{KOD1}", "");
            Query = Query.Replace("{ProductGroup}", "");
            Query = Query.Replace("{URUNADI}", "");
            Query = Query.Replace("{SUBEADI}", "");

            if (SUBEADI.Trim() == "")
            {
                Query = Query.Replace("#SUBEADIWHERE#", "");
            }
            else
            {
                Query = Query.Replace("#SUBEADIWHERE#", "AND SUBEADI in (" + SUBEADI + ")");

            }
            if (KOD1.Trim() == "")
            {
                Query = Query.Replace("#KOD1WHERE#", "");
            }
            else
            {
                Query = Query.Replace("#KOD1WHERE#", "AND KOD1 in (" + KOD1 + ")");

            }
            if (ProductGroup.Trim() == "")
            {
                Query = Query.Replace("#ProductGroupWHERE#", "");
            }
            else
            {
                Query = Query.Replace("#ProductGroupWHERE#", "AND ProductGroup in (" + ProductGroup + ")");

            }
            if (URUNADI.Trim() == "")
            {
                Query = Query.Replace("#URUNADIWHERE#", "");
            }
            else
            {
                Query = Query.Replace("#URUNADIWHERE#", "AND ProductName in (" + URUNADI + ")");

            }
            if (MaxDate == "Invalid Date" || MinDate == "Invalid Date")
            {
                Query = Query.Replace("#BELİRLİSAATLER#", "");

            }
            else
            {
                Query = Query.Replace("#BELİRLİSAATLER#", "AND (CONVERT(TIME, Date)>=CONVERT(TIME, '" + Convert.ToDateTime(MinDate).AddHours(-3).ToString("HH:mm:ss") + "')) AND (CONVERT(TIME, Date)<=CONVERT(TIME, '" + Convert.ToDateTime(MaxDate).AddHours(-3).ToString("HH:mm:ss") + "'))");

            }
            ModelFunctions f = new ModelFunctions();
            var dt = f.GetSubeDataWithQuery(SqlConnString, Query);
            return JsonConvert.SerializeObject(dt);

        }

    }
}