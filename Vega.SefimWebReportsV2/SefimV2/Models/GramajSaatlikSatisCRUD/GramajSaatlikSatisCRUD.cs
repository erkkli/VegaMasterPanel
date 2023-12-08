using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Configuration;
using System.Web.Hosting;

namespace SefimV2.Models.GramajSaatlikSatisCRUD
{
    public class GramajSaatlikSatisCRUD
    {
        //static string SqlConnString = "Data Source=" + WebConfigurationManager.AppSettings["Server"] + ";Initial Catalog=VegaMasterRapor;Persist Security Info=True;User Id=" + WebConfigurationManager.AppSettings["User"] + ";Password=" + WebConfigurationManager.AppSettings["Password"] + "; MultipleActiveResultSets=true";
        static string SqlConnString = "Data Source=" + WebConfigurationManager.AppSettings["Server"] + ";Initial Catalog=" + WebConfigurationManager.AppSettings["DBName"] + ";Persist Security Info=True;User Id=" + WebConfigurationManager.AppSettings["User"] + ";Password=" + WebConfigurationManager.AppSettings["Password"] + "; MultipleActiveResultSets=true";
        public static string DonerCesitlerineGore(DateTime startDate, DateTime endDate, string KOD1, string KOD2, string URUNADI, string TEKTRH, string GUN, string SAAT, string SUBEADI)
        {
            return GenelSql("SQL1.sql",startDate,endDate,KOD1,KOD2,URUNADI,TEKTRH,GUN,SAAT,SUBEADI);
            
        }

        public static string TarihlereGore(DateTime startDate, DateTime endDate, string KOD1, string KOD2, string URUNADI, string TEKTRH, string GUN, string SAAT, string SUBEADI)
        {
            return GenelSql("SQL2.sql", startDate, endDate, KOD1, KOD2, URUNADI, TEKTRH, GUN, SAAT, SUBEADI);

        }
        public static string GunlereGore(DateTime startDate, DateTime endDate, string KOD1, string KOD2, string URUNADI, string TEKTRH, string GUN, string SAAT, string SUBEADI)
        {
            return GenelSql("SQL3.sql", startDate, endDate, KOD1, KOD2, URUNADI, TEKTRH, GUN, SAAT, SUBEADI);

        }
        public static string SaatlereGore(DateTime startDate, DateTime endDate, string KOD1, string KOD2, string URUNADI, string TEKTRH, string GUN, string SAAT, string SUBEADI)
        {
            return GenelSql("SQL4.sql", startDate, endDate, KOD1, KOD2, URUNADI, TEKTRH, GUN, SAAT, SUBEADI);

        }
        public static string UrunlereGore(DateTime startDate, DateTime endDate, string KOD1, string KOD2, string URUNADI, string TEKTRH, string GUN, string SAAT, string SUBEADI)
        {
            return GenelSql("SQL5.sql", startDate, endDate, KOD1, KOD2, URUNADI, TEKTRH, GUN, SAAT, SUBEADI);

        }
        public static string SubelereGoreDetayli(DateTime startDate, DateTime endDate, string KOD1, string KOD2, string URUNADI, string TEKTRH, string GUN, string SAAT, string SUBEADI)
        {
            return GenelSql("SQL6.sql", startDate, endDate, KOD1, KOD2, URUNADI, TEKTRH, GUN, SAAT, SUBEADI);

        }
        public static string SubelereGore(DateTime startDate, DateTime endDate, string KOD1, string KOD2, string URUNADI, string TEKTRH, string GUN, string SAAT, string SUBEADI)
        {
            return GenelSql("SQL7.sql", startDate, endDate, KOD1, KOD2, URUNADI, TEKTRH, GUN, SAAT, SUBEADI);

        }


        public static string GenelSql(string sqlPath, DateTime startDate, DateTime endDate, string KOD1, string KOD2, string URUNADI, string TEKTRH, string GUN, string SAAT, string SUBEADI)
        {
            if (KOD1 != "")
                KOD1 = ("'" + KOD1 + "'").Replace(",", "','").Replace(",''", "");
            if (KOD2 != "")
                KOD2 = ("'" + KOD2 + "'").Replace(",", "','").Replace(",''", "");
            if (URUNADI != "")
                URUNADI = ("'" + URUNADI + "'").Replace(",", "','").Replace(",''", "");
            if (GUN != "")
                GUN = ("'" + GUN + "'").Replace(",", "','").Replace(",''", "");
            if (SUBEADI != "")
                SUBEADI = ("'" + SUBEADI + "'").Replace(",", "','").Replace(",''", "");
            string Query = "";
            Query = File.ReadAllText(HostingEnvironment.MapPath("/Sql/SqlNewSef/GramajSaatlikSatisRaporu/" + sqlPath), System.Text.UTF8Encoding.Default);
            Query = Query.Replace("{TARIH1}", startDate.ToString("yyyy-MM-dd 00:00:00"));
            Query = Query.Replace("{TARIH2}", endDate.ToString("yyyy-MM-dd 23:59:59"));
            Query = Query.Replace("{KOD1}", "");
            Query = Query.Replace("{KOD2}", "");
            Query = Query.Replace("{URUNADI}", "");
            Query = Query.Replace("{SUBEADI}", "");
            if (KOD1.Trim() == "")
            {
                Query = Query.Replace("#KOD1WHERE#", "");
            }
            else
            {
                Query = Query.Replace("#KOD1WHERE#", "AND KOD1 in (" + KOD1 + ")");

            }
            if (KOD2.Trim() == "")
            {
                Query = Query.Replace("#KOD2WHERE#", "");
            }
            else
            {
                Query = Query.Replace("#KOD2WHERE#", "AND KOD2 in (" + KOD2 + ")");

            }
            if (URUNADI.Trim() == "")
            {
                Query = Query.Replace("#URUNADIWHERE#", "");
            }
            else
            {
                Query = Query.Replace("#URUNADIWHERE#", "AND ProductName in (" + URUNADI + ")");

            }
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
            if (GUN.Trim() == "")
            {
                Query = Query.Replace("#GUNWHERE#", "");
            }
            else
            {

                Query = Query.Replace("#GUNWHERE#", " AND datename(dw,Date) in (" + GUN + ")");

            }
            if (SAAT.Trim() == "")
            {
                Query = Query.Replace("#SAATWHERE#", "");
            }
            else
            {
                if (Convert.ToInt32(SAAT) < 10)
                {
                    SAAT = "0" + SAAT;
                }
                Query = Query.Replace("#SAATWHERE#", "AND (CONVERT(TIME, Date)>=CONVERT(TIME, '" + SAAT + ":00:00" + "')) AND (CONVERT(TIME, Date)<=CONVERT(TIME, '" + SAAT + ":59:59" + "'))");

            }

            ModelFunctions f = new ModelFunctions();
            var dt = f.GetSubeDataWithQuery(SqlConnString, Query);
            return JsonConvert.SerializeObject(dt);

        }


    }
}