using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Configuration;
using System.Web.Hosting;

namespace SefimV2.Models.PorsiyonSatislariRaporuCRUD
{
    public class PorsiyonSatislariRaporuCRUD
    {
        //static string SqlConnString = "Data Source=" + WebConfigurationManager.AppSettings["Server"] + ";Initial Catalog=VegaMasterRapor;Persist Security Info=True;User Id=" + WebConfigurationManager.AppSettings["User"] + ";Password=" + WebConfigurationManager.AppSettings["Password"] + "; MultipleActiveResultSets=true";
        static string SqlConnString = "Data Source=" + WebConfigurationManager.AppSettings["Server"] + ";Initial Catalog=" + WebConfigurationManager.AppSettings["DBName"] + ";Persist Security Info=True;User Id=" + WebConfigurationManager.AppSettings["User"] + ";Password=" + WebConfigurationManager.AppSettings["Password"] + "; MultipleActiveResultSets=true";
        public static string UrunListesi(DateTime startDate, DateTime endDate, string KOD1, string URUNADI, string TEKTRH, string SUBEADI,string PORSIYONTIPI, string MinDate, string MaxDate)
        {
            return GenelSql("SQL5.sql",startDate,endDate,KOD1,URUNADI,TEKTRH,SUBEADI,PORSIYONTIPI,MinDate,MaxDate);
           
        }
        public static string TariheGore(DateTime startDate, DateTime endDate, string KOD1, string URUNADI, string TEKTRH, string SUBEADI, string PORSIYONTIPI, string MinDate, string MaxDate)
        {
            return GenelSql("SQL4.sql", startDate, endDate, KOD1, URUNADI, TEKTRH, SUBEADI, PORSIYONTIPI, MinDate, MaxDate);

        }
        public static string DonerCesidineGore(DateTime startDate, DateTime endDate, string KOD1, string URUNADI, string TEKTRH, string SUBEADI, string PORSIYONTIPI, string MinDate, string MaxDate)
        {
            return GenelSql("SQL3.sql", startDate, endDate, KOD1, URUNADI, TEKTRH, SUBEADI, PORSIYONTIPI, MinDate, MaxDate);

        }
        public static string SubelerPorsiyonAdetleri(DateTime startDate, DateTime endDate, string KOD1, string URUNADI, string TEKTRH, string SUBEADI, string PORSIYONTIPI, string MinDate, string MaxDate)
        {
            return GenelSql("SQL1.sql", startDate, endDate, KOD1, URUNADI, TEKTRH, SUBEADI, PORSIYONTIPI, MinDate, MaxDate);

        }
        public static string SaateGore(DateTime startDate, DateTime endDate, string KOD1, string URUNADI, string TEKTRH, string SUBEADI, string PORSIYONTIPI, string MinDate, string MaxDate)
        {
            return GenelSql("SQL6.sql", startDate, endDate, KOD1, URUNADI, TEKTRH, SUBEADI, PORSIYONTIPI, MinDate, MaxDate);

        }
        public static string PorsiyonTipineGore(DateTime startDate, DateTime endDate, string KOD1, string URUNADI, string TEKTRH, string SUBEADI, string PORSIYONTIPI, string MinDate, string MaxDate)
        {
            return GenelSql("SQL2.sql", startDate, endDate, KOD1, URUNADI, TEKTRH, SUBEADI, PORSIYONTIPI, MinDate, MaxDate);


        }


        public static string GenelSql(string sqlPath, DateTime startDate, DateTime endDate, string KOD1, string URUNADI, string TEKTRH, string SUBEADI, string PORSIYONTIPI, string MinDate, string MaxDate)
        {
            if (KOD1 != "")
                KOD1 = ("'" + KOD1 + "'").Replace(",", "','").Replace(",''", "");
            if (URUNADI != "")
                URUNADI = ("'" + URUNADI + "'").Replace(",", "','").Replace(",''", "");
            if (SUBEADI != "")
                SUBEADI = ("'" + SUBEADI + "'").Replace(",", "','").Replace(",''", "");
            if (PORSIYONTIPI != "")
                PORSIYONTIPI = ("'" + PORSIYONTIPI + "'").Replace(",", "','").Replace(",''", "");
            string Query = "";
            Query = File.ReadAllText(HostingEnvironment.MapPath("/Sql/SqlNewSef/PorsiyonSatislariRaporu/"+sqlPath), System.Text.UTF8Encoding.Default);
            Query = Query.Replace("{TARIH1}", startDate.ToString("yyyy-MM-dd 00:00:00"));
            Query = Query.Replace("{TARIH2}", endDate.ToString("yyyy-MM-dd 23:59:59"));
            Query = Query.Replace("{KOD1}", "");
            Query = Query.Replace("{URUNADI}", "");
            Query = Query.Replace("{SUBEADI}", "");

            if (PORSIYONTIPI.Trim() == "")
            {
                Query = Query.Replace("#PORSIYONTIPIWHERE#", "");
            }
            else
            {
                if (PORSIYONTIPI == "Adetli Porsiyon")
                {
                    Query = Query.Replace("#PORSIYONTIPIWHERE#", "AND ISNULL(KOD3,'')!='KG'");

                }
                else
                {
                    Query = Query.Replace("#PORSIYONTIPIWHERE#", "AND ISNULL(KOD3,'')='KG'");

                }

            }

            if (KOD1.Trim() == "")
            {
                Query = Query.Replace("#KOD1WHERE#", "");
            }
            else
            {
                Query = Query.Replace("#KOD1WHERE#", "AND KOD1 in (" + KOD1 + ")");

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