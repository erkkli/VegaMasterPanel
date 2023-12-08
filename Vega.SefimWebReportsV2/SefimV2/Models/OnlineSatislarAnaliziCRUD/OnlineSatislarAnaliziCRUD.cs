using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Configuration;
using System.Web.Hosting;

namespace SefimV2.Models.OnlineSatislarAnaliziCRUD
{
    public class OnlineSatislarAnaliziCRUD
    {
        //static string SqlConnString = "Data Source=" + WebConfigurationManager.AppSettings["Server"] + ";Initial Catalog=VegaMasterRapor;Persist Security Info=True;User Id=" + WebConfigurationManager.AppSettings["User"] + ";Password=" + WebConfigurationManager.AppSettings["Password"] + "; MultipleActiveResultSets=true";
        static string SqlConnString = "Data Source=" + WebConfigurationManager.AppSettings["Server"] + ";Initial Catalog=" + WebConfigurationManager.AppSettings["DBName"] + ";Persist Security Info=True;User Id=" + WebConfigurationManager.AppSettings["User"] + ";Password=" + WebConfigurationManager.AppSettings["Password"] + "; MultipleActiveResultSets=true";
        public static string UrunlereGore(DateTime startDate, DateTime endDate, string URUNADI, string SUBEADI, string KANAL, string ODEMETIPI, string SATISTIPI, string OPSIYONTIPI)
        {
            return GenelSql("SQL6.sql",startDate,endDate,URUNADI,SUBEADI,KANAL,ODEMETIPI,SATISTIPI,OPSIYONTIPI);
           
        }
        public static string SubelereGore(DateTime startDate, DateTime endDate, string URUNADI, string SUBEADI, string KANAL, string ODEMETIPI, string SATISTIPI, string OPSIYONTIPI)
        {
            return GenelSql("SQL5.sql", startDate, endDate, URUNADI, SUBEADI, KANAL, ODEMETIPI, SATISTIPI, OPSIYONTIPI);

        }
        public static string TahsilatTiplerineGore(DateTime startDate, DateTime endDate, string URUNADI, string SUBEADI, string KANAL, string ODEMETIPI, string SATISTIPI, string OPSIYONTIPI)
        {
            return GenelSql("SQL4.sql", startDate, endDate, URUNADI, SUBEADI, KANAL, ODEMETIPI, SATISTIPI, OPSIYONTIPI);

        }
        public static string OnlinePlatformlaraGore(DateTime startDate, DateTime endDate, string URUNADI, string SUBEADI, string KANAL, string ODEMETIPI, string SATISTIPI, string OPSIYONTIPI)
        {
            return GenelSql("SQL3.sql", startDate, endDate, URUNADI, SUBEADI, KANAL, ODEMETIPI, SATISTIPI, OPSIYONTIPI);

        }
        public static string SatisKanallarinaGore(DateTime startDate, DateTime endDate, string URUNADI, string SUBEADI, string KANAL, string ODEMETIPI, string SATISTIPI, string OPSIYONTIPI)
        {
            return GenelSql("SQL1.sql", startDate, endDate, URUNADI, SUBEADI, KANAL, ODEMETIPI, SATISTIPI, OPSIYONTIPI);

        }
        public static string OpsiyonTipineGore(DateTime startDate, DateTime endDate, string URUNADI, string SUBEADI, string KANAL, string ODEMETIPI, string SATISTIPI, string OPSIYONTIPI)
        {
            return GenelSql("SQL2.sql", startDate, endDate, URUNADI, SUBEADI, KANAL, ODEMETIPI, SATISTIPI, OPSIYONTIPI);

        }

        public static string GenelSql(string sqlPath, DateTime startDate, DateTime endDate, string URUNADI, string SUBEADI, string KANAL, string ODEMETIPI, string SATISTIPI, string OPSIYONTIPI)
        {
            if (URUNADI != "")
                URUNADI = ("'" + URUNADI + "'").Replace(",", "','").Replace(",''", "");
            if (SUBEADI != "")
                SUBEADI = ("'" + SUBEADI + "'").Replace(",", "','").Replace(",''", "");
            if (KANAL != "")
                KANAL = ("'" + KANAL + "'").Replace(",", "','").Replace(",''", "");
            if (ODEMETIPI != "")
                ODEMETIPI = ("'" + ODEMETIPI + "'").Replace(",", "','").Replace(",''", "");
            if (SATISTIPI != "")
                SATISTIPI = ("'" + SATISTIPI + "'").Replace(",", "','").Replace(",''", "");
            if (OPSIYONTIPI != "")
                OPSIYONTIPI = ("'" + OPSIYONTIPI + "'").Replace(",", "','").Replace(",''", "");
            string Query = "";
            Query = File.ReadAllText(HostingEnvironment.MapPath("/Sql/SqlNewSef/OnlineSatislarAnalizi/"+sqlPath), System.Text.UTF8Encoding.Default);
            Query = Query.Replace("{TARIH1}", startDate.ToString("yyyy-MM-dd 00:00:00"));
            Query = Query.Replace("{TARIH2}", endDate.ToString("yyyy-MM-dd 23:59:59"));
            Query = Query.Replace("{KANAL}", "");
            Query = Query.Replace("{SUBEADI}", "");
            Query = Query.Replace("{URUNADI}", "");
            Query = Query.Replace("{ODEMETIPI}", "");

            if (OPSIYONTIPI.Trim() == "")
            {
                Query = Query.Replace("#OPSIYONTIPIWHERE#", "");
            }
            else
            {
                if (OPSIYONTIPI == "Direk Satis")
                {
                    Query = Query.Replace("#OPSIYONTIPIWHERE#", "AND ISNULL(hr.Options,'')=''");

                }
                else
                {
                    Query = Query.Replace("#OPSIYONTIPIWHERE#", "AND ISNULL(hr.Options,'')!=''");

                }

            }
            if (SATISTIPI.Trim() == "")
            {
                Query = Query.Replace("#SATISTIPIWHERE#", "");
            }
            else
            {
                if (SATISTIPI == "'KASA'")
                {
                    Query = Query.Replace("#SATISTIPIWHERE#", "AND ISNULL(hr.OrderInd,'')=''");

                }
                else
                {
                    Query = Query.Replace("#SATISTIPIWHERE#", "AND ISNULL(hr.OrderInd,'')!=''");

                }

            }
            if (ODEMETIPI.Trim() == "")
            {
                Query = Query.Replace("#ODEMETIPIWHERE#", "");
            }
            else
            {
                Query = Query.Replace("#ODEMETIPIWHERE#", "AND OdemeYemekCeki  in (" + ODEMETIPI + ")");

            }
            if (KANAL.Trim() == "")
            {
                Query = Query.Replace("#KANALWHERE#", "");
            }
            else
            {
                Query = Query.Replace("#KANALWHERE#", "AND ISNULL(platform,'MAGAZA') in (" + KANAL + ")");

            }

            if (SUBEADI.Trim() == "")
            {
                Query = Query.Replace("#SUBEADIWHERE#", "");
            }
            else
            {
                Query = Query.Replace("#SUBEADIWHERE#", "AND SUBEADI  in (" + SUBEADI + ")");

            }

            if (URUNADI.Trim() == "")
            {
                Query = Query.Replace("#URUNADIWHERE#", "");
            }
            else
            {
                Query = Query.Replace("#URUNADIWHERE#", "AND ProductName in (" + URUNADI + ")");

            }
            ModelFunctions f = new ModelFunctions();
            var dt = f.GetSubeDataWithQuery(SqlConnString, Query);
            return JsonConvert.SerializeObject(dt);

        }

    }
}