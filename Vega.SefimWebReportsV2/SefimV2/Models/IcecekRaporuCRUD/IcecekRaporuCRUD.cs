using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Configuration;
using System.Web.Hosting;

namespace SefimV2.Models.IcecekRaporuCRUD
{
    public class IcecekRaporuCRUD
    {
        //static string SqlConnString = "Data Source=" + WebConfigurationManager.AppSettings["Server"] + ";Initial Catalog=VegaMasterRapor;Persist Security Info=True;User Id=" + WebConfigurationManager.AppSettings["User"] + ";Password=" + WebConfigurationManager.AppSettings["Password"] + "; MultipleActiveResultSets=true";
        static string SqlConnString = "Data Source=" + WebConfigurationManager.AppSettings["Server"] + ";Initial Catalog=" + WebConfigurationManager.AppSettings["DBName"] + ";Persist Security Info=True;User Id=" + WebConfigurationManager.AppSettings["User"] + ";Password=" + WebConfigurationManager.AppSettings["Password"] + "; MultipleActiveResultSets=true";

        public static string ToplamIcecekSatislari(DateTime startDate, DateTime endDate, string URUNGRUBU, string URUNADI, string SUBEADI, string SATISTIPI)
        {
            return GenelSql("SQL2.sql",startDate,endDate,URUNGRUBU,URUNADI,SUBEADI,SATISTIPI);
           
        }
        public static string UrunlereGore(DateTime startDate, DateTime endDate, string URUNGRUBU, string URUNADI, string SUBEADI, string SATISTIPI)
        {
            return GenelSql("SQL5.sql", startDate, endDate, URUNGRUBU, URUNADI, SUBEADI, SATISTIPI);

        
        }
        public static string SatisTipineGore(DateTime startDate, DateTime endDate, string URUNGRUBU, string URUNADI, string SUBEADI, string SATISTIPI)
        {
            return GenelSql("SQL4.sql", startDate, endDate, URUNGRUBU, URUNADI, SUBEADI, SATISTIPI);

          
        }
        public static string IcecekGruplarinaGore(DateTime startDate, DateTime endDate, string URUNGRUBU, string URUNADI, string SUBEADI, string SATISTIPI)
        {
            return GenelSql("SQL1.sql", startDate, endDate, URUNGRUBU, URUNADI, SUBEADI, SATISTIPI);

        }
        public static string SubelereGore(DateTime startDate, DateTime endDate, string URUNGRUBU, string URUNADI, string SUBEADI, string SATISTIPI)
        {
            return GenelSql("SQL3.sql", startDate, endDate, URUNGRUBU, URUNADI, SUBEADI, SATISTIPI);

           
        }


        public static string GenelSql(string sqlPath, DateTime startDate, DateTime endDate, string URUNGRUBU, string URUNADI, string SUBEADI, string SATISTIPI)
        {
            if (URUNGRUBU != "")
                URUNGRUBU = ("'" + URUNGRUBU + "'").Replace(",", "','").Replace(",''", "");
            if (URUNADI != "")
                URUNADI = ("'" + URUNADI + "'").Replace(",", "','").Replace(",''", "");
            if (SUBEADI != "")
                SUBEADI = ("'" + SUBEADI + "'").Replace(",", "','").Replace(",''", "");
            if (SATISTIPI != "")
                SATISTIPI = ("'" + SATISTIPI + "'").Replace(",", "','").Replace(",''", "");
            string Query = "";
            Query = File.ReadAllText(HostingEnvironment.MapPath("/Sql/SqlNewSef/IcecekRaporu/"+sqlPath), System.Text.UTF8Encoding.Default);
            Query = Query.Replace("{TARIH1}", startDate.ToString("yyyy-MM-dd 00:00:00"));
            Query = Query.Replace("{TARIH2}", endDate.ToString("yyyy-MM-dd 23:59:59"));
            Query = Query.Replace("{URUNGRUBU}", "");
            Query = Query.Replace("{SUBEADI}", "");
            Query = Query.Replace("{SATISTIPI}", "");
            Query = Query.Replace("{URUNADI}", "");

            if (URUNADI.Trim() == "")
            {
                Query = Query.Replace("#URUNADIWHERE#", "");
                Query = Query.Replace("#URUNADIWHERE2#", "");

            }
            else
            {
                Query = Query.Replace("#URUNADIWHERE#", "AND BAS.ProductName in (" + URUNADI + ")");
                Query = Query.Replace("#URUNADIWHERE2#", "AND HAR.URUNADI in (" + URUNADI + ")");



            }
            if (SUBEADI.Trim() == "")
            {
                Query = Query.Replace("#SUBEADIWHERE#", "");
                Query = Query.Replace("#SUBEADIWHERE2#", "");
            }
            else
            {
                Query = Query.Replace("#SUBEADIWHERE#", " AND SUBEADI in (" + SUBEADI + ")");
                Query = Query.Replace("#SUBEADIWHERE2#", " AND SUBEADI in (" + SUBEADI + ")");

            }
            if (URUNGRUBU.Trim() == "")
            {
                Query = Query.Replace("#URUNGRUBUWHERE#", "");
                Query = Query.Replace("#URUNGRUBUWHERE2#", "");
            }
            else
            {

                Query = Query.Replace("#URUNGRUBUWHERE#", " AND BAS.ProductGroup in (" + URUNGRUBU + ")");
                Query = Query.Replace("#URUNGRUBUWHERE2#", " AND HAR.ProductGroup in (" + URUNGRUBU + ")");

            }
            if (SATISTIPI.Trim() == "")
            {
                Query = Query.Replace("#SATISTIPIWHERE#", "");
                Query = Query.Replace("#SATISTIPIWHERE2#", "");
            }
            else
            {

                if (SATISTIPI == "Direk Satiş")
                {
                    Query = Query.Replace("#SATISTIPIWHERE#", " AND ISNULL(Options,'')=@SATISTIPI");
                    Query = Query.Replace("#SATISTIPIWHERE2#", " AND IND=-1");

                }
                else
                {
                    Query = Query.Replace("#SATISTIPIWHERE#", " AND ISNULL(Options,'')!=@SATISTIPI");
                    Query = Query.Replace("#SATISTIPIWHERE2#", "");

                }

            }
            ModelFunctions f = new ModelFunctions();
            var dt = f.GetSubeDataWithQuery(SqlConnString, Query);
            return JsonConvert.SerializeObject(dt);

        }
    }
}