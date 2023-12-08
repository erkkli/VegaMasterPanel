using SefimV2.ViewModels.User;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Hosting;

namespace SefimV2.Models
{
    public class KasaCiroCRUD
    {
        public static List<KasaCiro> List(DateTime Date1, DateTime Date2, string ID)
        {
            List<KasaCiro> Liste = new List<KasaCiro>();
            ModelFunctions ff = new ModelFunctions();
            DateTime startDate = DateTime.Now;
            #region GİRİŞ YAPAN KULLANICININ YETKİLİ OLDUĞU ŞUBELER LİSTELENİYOR
            UserViewModel model = UsersListCRUD.YetkiliSubesi(ID);
            #endregion
            try
            {
                #region SUBSTATION LIST               
                ff.SqlConnOpen();
                DataTable dt = ff.DataTable("select * from SubeSettings Where Status=1");
                ff.SqlConnClose();
                #endregion SUBSTATION LIST

                #region VEGA DB database ismini çekmek için.
                string vega_Db = "";
                ff.SqlConnOpen();
                DataTable dataVegaDb = ff.DataTable("select* from VegaDbSettings ");
                var vegaDBList = dataVegaDb.AsEnumerable().ToList<DataRow>();
                foreach (var item in vegaDBList)
                {
                    vega_Db = item["DBName"].ToString();
                }
                ff.SqlConnClose();
                #endregion VEGA DB database ismini çekmek için.
                try
                {
                    #region PARALEL FOREACH
                    //foreach (DataRow r in dt.AsEnumerable())
                    //{
                    var dtList = dt.AsEnumerable().ToList<DataRow>();
                    Parallel.ForEach(dtList, r =>
                    {
                        ModelFunctions f = new ModelFunctions();
                        //var thr = new Thread(() =>
                        //  {
                        string AppDbType = f.RTS(r, "AppDbType");
                        string AppDbTypeStatus = f.RTS(r, "AppDbTypeStatus"); //AppDbTypeStatus=True ise Faster>online  
                        string SubeId = r["Id"].ToString();
                        string SubeAdi = r["SubeName"].ToString();
                        string SubeIP = r["SubeIP"].ToString();
                        string SqlName = r["SqlName"].ToString();
                        string SqlPassword = r["SqlPassword"].ToString();
                        string DBName = r["DBName"].ToString();
                        string QueryTimeStart = Date1.ToString("yyyy/MM/dd HH:mm:ss");
                        string QueryTimeEnd = Date2.ToString("yyyy/MM/dd HH:mm:ss");
                        string FirmaId_SUBE = "F0" + r["FirmaID"].ToString() + "TBLKRDSUBELER"; //Online Faster kullanan şubede Firma ve kasa Id set ediliyor.
                        string FirmaId_KASA = "F0" + r["FirmaID"].ToString() + "TBLKRDKASALAR";
                        string Firma_NPOS = r["FirmaID"].ToString();
                        string vPosSubeKodu = r["VPosSubeKodu"].ToString();
                        string vPosKasaKodu = r["VPosKasaKodu"].ToString();
                        if (AppDbType == "5")
                        {
                            if (!string.IsNullOrWhiteSpace(vPosSubeKodu))
                            {
                                var kasaName = f.GetSubeDataWithQuery(f.NewConnectionString(SubeIP, DBName, SqlName, SqlPassword), "Select * from TBLSPOSSUBELER where IND='" + vPosSubeKodu + "' ");
                                vPosSubeKodu = kasaName.Rows[0]["KODU"].ToString();
                            }
                        }

                        #region  SEFİM YENI - ESKİ FASTER SQL
                                                 
                        string Query = string.Empty;

                        #region Faster Kasalar seçildiyse ona göre query oluşturuluyor. 
                        string FasterSubeIND = f.RTS(r, "FasterSubeID");
                        string QueryFasterSube = string.Empty;
                        if (FasterSubeIND != null)
                        {
                            QueryFasterSube = "  and SUBEIND IN(" + FasterSubeIND + ") ";
                        }
                        #endregion Faster Kasalar seçildiyse ona göre query oluşturuluyor.

                        if (AppDbType == "1")// 1 = yeni şefim, 2 =eski Şefim, 3 = Faster
                        {
                            Query = File.ReadAllText(HostingEnvironment.MapPath("/Sql/SqlNewSef/KasaCiroNewSefim.sql"), System.Text.Encoding.UTF8);
                        }
                        else if (AppDbType == "2")
                        {
                            Query = File.ReadAllText(HostingEnvironment.MapPath("/Sql/KasaCiro.sql"), System.Text.Encoding.UTF8);
                        }
                        else if (AppDbType == "3")
                        {
                            if (AppDbTypeStatus == "True")
                            {
                                #region FASTER ONLINE QUARY                              
                                Query =
                                        " declare @Sube nvarchar(100) = '{SUBEADI}';" +
                                        " declare @Trh1 nvarchar(20) = '{TARIH1}';" +
                                        " declare @Trh2 nvarchar(20) = '{TARIH2}';" +
                                        " DECLARE @FirmaInd nvarchar(100) = '{FIRMAIND}';" +
                                         " WITH Toplamsatis AS " +
                                         "  (SELECT @Sube AS Sube," +
                                         "     (SELECT SUBEADI" +
                                         "      FROM " + FirmaId_SUBE + " " +
                                         "      WHERE IND=TBLFASTERODEMELER.SUBEIND) AS Sube1," +
                                         "     (SELECT KASAADI" +
                                         "      FROM " + FirmaId_KASA + "" +
                                         "      WHERE IND=TBLFASTERODEMELER.KASAIND) AS Kasa," +
                                         "          ODENEN AS cash," +
                                         "          0 AS Credit," +
                                         "          0 AS Ticket," +
                                         "          0 AS KasaToplam," +
                                         "		  0 AS iptal" +
                                         "   FROM DBO.TBLFASTERODEMELER" +
                                         "   WHERE ODEMETIPI=0" +
                                         "     AND ISNULL(IADE, 0)=0" +
                                         "     AND ISLEMTARIHI >= @Trh1" +
                                         "     AND ISLEMTARIHI <= @Trh2" +
                                         "    " + QueryFasterSube +
                                         "	UNION ALL SELECT @Sube AS Sube," +
                                         "     (SELECT SUBEADI" +
                                         "      FROM " + FirmaId_SUBE + " " +
                                         "      WHERE IND=TBLFASTERODEMELER.SUBEIND) AS Sube1," +
                                         "     (SELECT KASAADI" +
                                         "      FROM " + FirmaId_KASA + "" +
                                         "      WHERE IND=TBLFASTERODEMELER.KASAIND) AS Kasa," +
                                         "          ODENEN*-1 AS cash," +
                                         "          0 AS Credit," +
                                         "          0 AS Ticket," +
                                         "          0 AS KasaToplam," +
                                         "		  0 AS iptal" +
                                         "   FROM DBO.TBLFASTERODEMELER" +
                                         "   WHERE ODEMETIPI=0" +
                                         "     AND ISNULL(IADE, 0)=1" +
                                         "     AND ISLEMTARIHI >= @Trh1" +
                                         "     AND ISLEMTARIHI <= @Trh2" +
                                         "    " + QueryFasterSube +
                                         "   UNION ALL SELECT @Sube AS Sube," +
                                         "     (SELECT SUBEADI" +
                                         "      FROM " + FirmaId_SUBE + " " +
                                         "      WHERE IND=TBLFASTERODEMELER.SUBEIND) AS Sube1," +
                                         "     (SELECT KASAADI" +
                                         "      FROM " + FirmaId_KASA + "" +
                                         "      WHERE IND=TBLFASTERODEMELER.KASAIND) AS Kasa," +
                                         "                    0 AS cash," +
                                         "                    ODENEN AS Credit," +
                                         "                    0 AS Ticket," +
                                         "                    0 AS KasaToplam," +
                                         "		  0 AS iptal" +
                                         "   FROM DBO.TBLFASTERODEMELER" +
                                         "   WHERE ODEMETIPI=1" +
                                         "     AND ISNULL(IADE, 0)=0" +
                                         "     AND ISLEMTARIHI >= @Trh1" +
                                         "     AND ISLEMTARIHI <= @Trh2" +
                                         "    " + QueryFasterSube +
                                         "	  UNION ALL SELECT @Sube AS Sube," +
                                         "     (SELECT SUBEADI" +
                                         "      FROM " + FirmaId_SUBE + " " +
                                         "      WHERE IND=TBLFASTERODEMELER.SUBEIND) AS Sube1," +
                                         "     (SELECT KASAADI" +
                                         "      FROM " + FirmaId_KASA + "" +
                                         "      WHERE IND=TBLFASTERODEMELER.KASAIND) AS Kasa," +
                                         "                    0 AS cash," +
                                         "                    ODENEN*-1 AS Credit," +
                                         "                    0 AS Ticket," +
                                         "                    0 AS KasaToplam," +
                                         "		  0 AS iptal" +
                                         "   FROM DBO.TBLFASTERODEMELER" +
                                         "   WHERE ODEMETIPI=1" +
                                         "     AND ISNULL(IADE, 0)=1" +
                                         "     AND ISLEMTARIHI >= @Trh1" +
                                         "     AND ISLEMTARIHI <= @Trh2" +
                                         "    " + QueryFasterSube +
                                         "	    UNION ALL SELECT @Sube AS Sube," +
                                         "     (SELECT SUBEADI" +
                                         "      FROM " + FirmaId_SUBE + " " +
                                         "      WHERE IND=TBLFASTERODEMELER.SUBEIND) AS Sube1," +
                                         "     (SELECT KASAADI" +
                                         "      FROM " + FirmaId_KASA + "" +
                                         "      WHERE IND=TBLFASTERODEMELER.KASAIND) AS Kasa," +
                                         "	  0 AS cash," +
                                         "                    0 AS Credit," +
                                         "                    0 AS Ticket," +
                                         "                    0 AS KasaToplam," +
                                         "                    ODENEN AS iptal" +
                                         "        " +
                                         "   FROM DBO.TBLFASTERODEMELER" +
                                         "   WHERE ISNULL(IADE, 0) = 1 AND ODEMETIPI NOT IN (0,1,2)" +
                                         "     AND ISLEMTARIHI >= @Trh1" +
                                         "     AND ISLEMTARIHI <= @Trh2" +
                                         "    " + QueryFasterSube +
                                         "   UNION ALL SELECT @Sube AS Sube," +
                                         "     (SELECT SUBEADI" +
                                         "      FROM " + FirmaId_SUBE + " " +
                                         "      WHERE IND=TBLFASTERODEMELER.SUBEIND) AS Sube1," +
                                         "     (SELECT KASAADI" +
                                         "      FROM " + FirmaId_KASA + "" +
                                         "      WHERE IND=TBLFASTERODEMELER.KASAIND) AS Kasa," +
                                         "                    0 AS cash," +
                                         "                    0 AS Credit," +
                                         "                    ODENEN AS Ticket," +
                                         "                    0 AS KasaToplam," +
                                         "		  0 AS iptal" +
                                         "   FROM DBO.TBLFASTERODEMELER" +
                                         "   WHERE ODEMETIPI=2" +
                                         "     AND ISNULL(IADE, 0)=0" +
                                         "     AND ISLEMTARIHI >= @Trh1" +
                                         "     AND ISLEMTARIHI <= @Trh2" +
                                         "    " + QueryFasterSube +
                                         "	    UNION ALL SELECT @Sube AS Sube," +
                                         "     (SELECT SUBEADI" +
                                         "      FROM " + FirmaId_SUBE + "" +
                                         "      WHERE IND=TBLFASTERODEMELER.SUBEIND) AS Sube1," +
                                         "     (SELECT KASAADI" +
                                         "      FROM " + FirmaId_KASA + "" +
                                         "      WHERE IND=TBLFASTERODEMELER.KASAIND) AS Kasa," +
                                         "                    0 AS cash," +
                                         "                    0 AS Credit," +
                                         "                    ODENEN*-1 AS Ticket," +
                                         "                    0 AS KasaToplam," +
                                         "		  0 AS iptal" +
                                         "   FROM DBO.TBLFASTERODEMELER" +
                                         "   WHERE ODEMETIPI=2" +
                                         "     AND ISNULL(IADE, 0)=1" +
                                         "     AND ISLEMTARIHI >= @Trh1" +
                                         "     AND ISLEMTARIHI <= @Trh2" +
                                         "    " + QueryFasterSube +
                                         "   UNION ALL SELECT @Sube AS Sube," +
                                         "     (SELECT SUBEADI" +
                                         "      FROM " + FirmaId_SUBE + "" +
                                         "      WHERE IND=TBLFASTERKASAISLEMLERI.SUBEIND) AS Sube1," +
                                         "     (SELECT KASAADI" +
                                         "      FROM " + FirmaId_KASA + "" +
                                         "      WHERE IND=TBLFASTERKASAISLEMLERI.KASAIND) AS Kasa," +
                                         "                    0 AS cash," +
                                         "                    0 AS Credit," +
                                         "                    0 AS Ticket," +
                                         "                    SUM(GELIR-GIDER) AS KasaToplam," +
                                         "		  0 AS iptal" +
                                         "   FROM DBO.TBLFASTERKASAISLEMLERI" +
                                         "   WHERE ISLEMTARIHI >= @Trh1" +
                                         "     AND ISLEMTARIHI <= @Trh2" +
                                         "    " + QueryFasterSube +
                                         "   GROUP BY SUBEIND," +
                                         "            KASAIND)" +
                                         "SELECT Sube," +
                                         "       Sube1," +
                                         "       Kasa," +
                                         "       SUM(Cash) AS Cash," +
                                         "       SUM(Credit) AS Credit," +
                                         "       Sum(Ticket) AS Ticket," +
                                         "       Sum(KasaToplam) AS KasaToplam," +
                                         "       SUM(Cash+Credit+Ticket)-SUM(iptal) AS ToplamCiro," +
                                         "	   " +
                                         "       0 AS Saniye," +
                                         "       '' AS RowStyle," +
                                         "       '' AS RowError " +
                                         " FROM toplamsatis " +
                                         " GROUP BY Sube," +
                                         "         Sube1," +
                                         "         Kasa ";
                                #endregion FASTER ONLINE QUARY
                            }
                            else
                            {
                                Query = File.ReadAllText(HostingEnvironment.MapPath("/Sql/SqlFaster/KasaCiro/KasaCiroFasterOFLINE.sql"), System.Text.UTF8Encoding.Default);
                            }
                        }
                        else if (AppDbType == "4") //NPOS>4
                        {
                            #region NPOS QUARY
                            Query =
                                   " declare @Sube nvarchar(100) = '{SUBEADI}';" +
                                   " declare @Trh1 nvarchar(20) = '{TARIH1}';" +
                                   " declare @Trh2 nvarchar(20) = '{TARIH2}';" +

                                   " select" +
                                   " @Sube, Sube, Sube1, Kasa_No as Kasa ,sUM(Nakit) AS Cash, Sum(Visa) as Credit,Sum(Ticket) as Ticket,Sum(Debit) AS Debit, Sum(Tableno) as TableNo, Sum(Discount) as Discount,Sum(Ikram) as ikram,Sum(Zayi) as Zayi," +
                                   " SUm(case when t.Iptal = 1 then t.Toplam else 0 end) as iptal," +
                                   " SUm(case when t.Iptal = 0 then t.Toplam else 0 end) as ToplamCiro " +
                                   " from( " +
                                   " SELECT " +
                                   " (SELECT top 1 OZELKOD1 FROM " + vega_Db + "..F0" + Firma_NPOS + "TBLPOSKASATANIM WHERE KASANO = Hr.Kasa_No) AS Sube, " +
                                   " (SELECT top 1 OZELKOD1 FROM " + vega_Db + "..F0" + Firma_NPOS + "TBLPOSKASATANIM WHERE KASANO = Hr.Kasa_No) AS Sube1," +
                                   "  hr.Kasa_no, " +
                                   " ISNULL((SELECT SUM(Tutar) from " + DBName + "..Odeme WHERE Tus_no = 0 and belge_Id = Hr.belge_Id),0) AS Nakit," +
                                   " ISNULL((SELECT SUM(Tutar) from " + DBName + "..Odeme WHERE Tus_no IN(1, 2, 3, 4) and belge_Id = Hr.belge_Id),0) AS Visa," +
                                   " 0 AS Ticket, " +
                                   " ISNULL((SELECT SUM(Tutar) from " + DBName + "..Odeme WHERE Tus_no IN(5, 6) and belge_Id = Hr.belge_Id),0) AS Debit," +
                                   " 0 AS Ikram " +
                                   " ,0 as Tableno " +
                                   " ,SUM(DISCOUNTTOTAL) as Discount " +
                                   " ,Iptal " +
                                   " ,0 as Zayi, " +
                                   " SUm(hr.Toplam) as Toplam " +
                                   " FROM " + DBName + "..BELGE as hr " +
                                   " where hr.BELGETARIH >= @Trh1 AND hr.BELGETARIH <= @Trh2 " +
                                   " GROUP BY Sicil_No,Kasa_No,hr.Belge_ID,Iptal) as t " +
                                   " group by " +
                                   " Sube,Sube1,Kasa_No  ";
                            #endregion NPOS QUARY
                        }
                        else if (AppDbType=="5")
                        {
                            Query = File.ReadAllText(HostingEnvironment.MapPath("/Sql/VPos/KasaRaporu/KasaRaporu.sql"), System.Text.UTF8Encoding.Default);
                        }
                        #endregion

                        Query = Query.Replace("{SUBEADI}", SubeAdi);
                        Query = Query.Replace("{TARIH1}", QueryTimeStart);
                        Query = Query.Replace("{TARIH2}", QueryTimeEnd);
                        Query = Query.Replace("{FIRMAIND}", Firma_NPOS);
                        Query = Query.Replace("{SUBE2}", vPosSubeKodu);
                        Query = Query.Replace("{KASAKODU}", vPosKasaKodu);

                        if (ID == "1")
                        {
                            #region GET DATA                    
                            try
                            {
                                string connString = "Provider=SQLOLEDB;Server=" + SubeIP + ";User Id=" + SqlName + ";Password=" + SqlPassword + ";Database=" + DBName + "";

                                try
                                {
                                    DataTable KasaCiroDt = new DataTable();
                                    KasaCiroDt = f.GetSubeDataWithQuery((f.NewConnectionString(SubeIP, DBName, SqlName, SqlPassword)), Query.ToString());
                                    if (KasaCiroDt.Rows.Count > 0)
                                    {
                                        if (AppDbType == "3" || AppDbType == "4")
                                        {
                                            #region FASTER (AppDbType=3 faster kullanan şube)
                                            foreach (DataRow sube in KasaCiroDt.Rows)
                                            {
                                                KasaCiro items = new KasaCiro();
                                                if (AppDbType == "3")
                                                {
                                                    items.Sube = sube["Sube"].ToString() + "-" + sube["Sube1"].ToString() + "-" + sube["Kasa"].ToString(); //KasaCiroDt.Rows[0]["Sube"].ToString();
                                                }
                                                else
                                                {
                                                    items.Sube = SubeAdi + "_" + sube["Sube"].ToString() + "-" + sube["Sube1"].ToString() + "-" + sube["Kasa"].ToString(); //KasaCiroDt.Rows[0]["Sube"].ToString();
                                                }

                                                items.SubeId = SubeId;
                                                items.Cash = Convert.ToDecimal(sube["Cash"]);
                                                items.Credit = Convert.ToDecimal(sube["Credit"]);
                                                items.Ticket = Convert.ToDecimal(sube["Ticket"]);
                                                items.Ciro = Convert.ToDecimal(sube["ToplamCiro"]);
                                                Liste.Add(items);
                                            }
                                            #endregion FASTER (AppDbType=3 faster kullanan şube)
                                        }
                                        else
                                        {
                                            KasaCiro items = new KasaCiro
                                            {
                                                Sube = SubeAdi,//KasaCiroDt.Rows[0]["Sube"].ToString();
                                                SubeId = SubeId,
                                                Cash = Convert.ToDecimal(KasaCiroDt.Rows[0]["Cash"]),
                                                Credit = Convert.ToDecimal(KasaCiroDt.Rows[0]["Credit"]),
                                                Ticket = Convert.ToDecimal(KasaCiroDt.Rows[0]["Ticket"]),
                                                Ciro = Convert.ToDecimal(KasaCiroDt.Rows[0]["ToplamCiro"])
                                            };
                                            if (AppDbType == "1")
                                            {
                                                items.GelirGider = Convert.ToDecimal(KasaCiroDt.Rows[0]["GelirGider"]);
                                            }
                                            items.OnlinePayment = f.RTD(KasaCiroDt.Rows[0], "OnlinePayment");
                                            Liste.Add(items);
                                        }
                                    }
                                    else
                                    {
                                        KasaCiro items = new KasaCiro();
                                        items.Sube = SubeAdi + " (Data Yok)";
                                        Liste.Add(items);
                                    }
                                }
                                catch (Exception) { throw new Exception(SubeAdi); }
                            }
                            catch (System.Exception ex)
                            {
                                #region EX                               
                                //log metnini oluştur
                                string ErrorMessage = "Kasa Raporu Alınamadı.";
                                //string SystemErrorMessage = ex.Message.ToString();
                                //string LogText = "";
                                //LogText += "Hata Zamanı : " + DateTime.Now.ToString() + Environment.NewLine;
                                //LogText += "Hata Mesajı : " + ErrorMessage + Environment.NewLine;
                                //LogText += "Hata Açıklaması : " + SystemErrorMessage + Environment.NewLine;
                                //LogText += "-----------------" + Environment.NewLine;

                                KasaCiro items = new KasaCiro();
                                items.Sube = ex.Message + " (Erişim Yok) ";
                                items.SubeId = "";
                                items.ErrorMessage = ErrorMessage;
                                items.ErrorStatus = true;
                                items.ErrorCode = "01";
                                Liste.Add(items);
                                //string LogFolder = "/Uploads/Logs/Error";
                                //if (Directory.Exists(HostingEnvironment.MapPath(LogFolder)) == false) { Directory.CreateDirectory(HostingEnvironment.MapPath(LogFolder)); }
                                //string LogFile = "Sube-" + SubeId + ".txt";
                                //string LogFilePath = HostingEnvironment.MapPath(LogFolder + "/" + LogFile);

                                //if (File.Exists(LogFilePath) == false)
                                //{
                                //    string FirstLine = "Created on " + DateTime.Now.ToString() + Environment.NewLine + Environment.NewLine;
                                //    File.WriteAllText(LogFilePath, FirstLine);
                                //}
                                //File.AppendAllText(LogFilePath, LogText);
                                #endregion
                            }
                            #endregion
                        }
                        else
                        {
                            #region KULLANICININ ŞUBE YETKİ KONTROLU YAPILIYOR             
                            foreach (var item in model.FR_SubeListesi)
                            {
                                if (item.SubeID == Convert.ToInt32(SubeId))
                                {
                                    #region GET DATA                    
                                    try
                                    {
                                        string connString = "Provider=SQLOLEDB;Server=" + SubeIP + ";User Id=" + SqlName + ";Password=" + SqlPassword + ";Database=" + DBName + "";

                                        try
                                        {
                                            DataTable KasaCiroDt = new DataTable();
                                            KasaCiroDt = f.GetSubeDataWithQuery((f.NewConnectionString(SubeIP, DBName, SqlName, SqlPassword)), Query.ToString());
                                            if (KasaCiroDt.Rows.Count > 0)
                                            {
                                                if (AppDbType == "3" || AppDbType == "4")
                                                {
                                                    #region FASTER (AppDbType=3 faster kullanan şube)
                                                    foreach (DataRow sube in KasaCiroDt.Rows)
                                                    {
                                                        KasaCiro items = new KasaCiro();
                                                        if (AppDbType == "3")
                                                        {
                                                            items.Sube = sube["Sube"].ToString() + "-" + sube["Sube1"].ToString() + "-" + sube["Kasa"].ToString(); //KasaCiroDt.Rows[0]["Sube"].ToString();
                                                        }
                                                        else
                                                        {
                                                            items.Sube = SubeAdi + "_" + sube["Sube"].ToString() + "-" + sube["Sube1"].ToString() + "-" + sube["Kasa"].ToString(); //KasaCiroDt.Rows[0]["Sube"].ToString();
                                                        }

                                                        items.SubeId = SubeId;
                                                        items.Cash = Convert.ToDecimal(sube["Cash"]);
                                                        items.Credit = Convert.ToDecimal(sube["Credit"]);
                                                        items.Ticket = Convert.ToDecimal(sube["Ticket"]);
                                                        items.Ciro = Convert.ToDecimal(sube["ToplamCiro"]);
                                                        Liste.Add(items);
                                                    }
                                                    #endregion FASTER (AppDbType=3 faster kullanan şube)
                                                }
                                                else
                                                {
                                                    KasaCiro items = new KasaCiro
                                                    {
                                                        Sube = SubeAdi,//KasaCiroDt.Rows[0]["Sube"].ToString();
                                                        SubeId = SubeId,
                                                        Cash = Convert.ToDecimal(KasaCiroDt.Rows[0]["Cash"]),
                                                        Credit = Convert.ToDecimal(KasaCiroDt.Rows[0]["Credit"]),
                                                        Ticket = Convert.ToDecimal(KasaCiroDt.Rows[0]["Ticket"]),
                                                        Ciro = Convert.ToDecimal(KasaCiroDt.Rows[0]["ToplamCiro"])
                                                    };
                                                    if (AppDbType == "1")
                                                    {
                                                        items.GelirGider = Convert.ToDecimal(KasaCiroDt.Rows[0]["GelirGider"]);
                                                    }
                                                    items.OnlinePayment = f.RTD(KasaCiroDt.Rows[0], "OnlinePayment");
                                                    Liste.Add(items);
                                                }
                                            }
                                            else
                                            {
                                                KasaCiro items = new KasaCiro();
                                                items.Sube = SubeAdi + " (Data Yok)";
                                                Liste.Add(items);
                                            }
                                        }
                                        catch (Exception) { throw new Exception(SubeAdi); }
                                    }
                                    catch (System.Exception ex)
                                    {
                                        #region EX                               
                                        //log metnini oluştur
                                        string ErrorMessage = "Kasa Raporu Alınamadı.";
                                        //string SystemErrorMessage = ex.Message.ToString();
                                        //string LogText = "";
                                        //LogText += "Hata Zamanı : " + DateTime.Now.ToString() + Environment.NewLine;
                                        //LogText += "Hata Mesajı : " + ErrorMessage + Environment.NewLine;
                                        //LogText += "Hata Açıklaması : " + SystemErrorMessage + Environment.NewLine;
                                        //LogText += "-----------------" + Environment.NewLine;

                                        KasaCiro items = new KasaCiro();
                                        items.Sube = ex.Message + " (Erişim Yok) ";
                                        items.SubeId = "";
                                        items.ErrorMessage = ErrorMessage;
                                        items.ErrorStatus = true;
                                        items.ErrorCode = "01";
                                        Liste.Add(items);
                                        //string LogFolder = "/Uploads/Logs/Error";
                                        //if (Directory.Exists(HostingEnvironment.MapPath(LogFolder)) == false) { Directory.CreateDirectory(HostingEnvironment.MapPath(LogFolder)); }
                                        //string LogFile = "Sube-" + SubeId + ".txt";
                                        //string LogFilePath = HostingEnvironment.MapPath(LogFolder + "/" + LogFile);

                                        //if (File.Exists(LogFilePath) == false)
                                        //{
                                        //    string FirstLine = "Created on " + DateTime.Now.ToString() + Environment.NewLine + Environment.NewLine;
                                        //    File.WriteAllText(LogFilePath, FirstLine);
                                        //}
                                        //File.AppendAllText(LogFilePath, LogText);
                                        #endregion
                                    }
                                    #endregion
                                }
                            }
                            #endregion
                        }
                    });
                    //}
                    #endregion PARALEL FOREACH
                }
                catch (Exception eee) { }

                #region foreach-2           
                //foreach (DataRow r in dt.Rows)
                //{
                //    string SubeId = f.RTS(r, "Id");
                //    string SubeAdi = f.RTS(r, "SubeName");
                //    string SubeIP = f.RTS(r, "SubeIP");
                //    string SqlName = f.RTS(r, "SqlName");
                //    string SqlPassword = f.RTS(r, "SqlPassword");
                //    string DBName = f.RTS(r, "DBName");
                //    string QueryTimeStart = Date1.ToString("yyyy/MM/dd HH:mm:ss");
                //    string QueryTimeEnd = Date2.ToString("yyyy/MM/dd HH:mm:ss");

                //    #region  SEFİM YENI - ESKİ FASTER SQL
                //    string AppDbType = f.RTS(r, "AppDbType");
                //    string Query = "";
                //    if (AppDbType == "1")// 1 = yeni şefim, 2 =eski Şefim, 3 = Faster
                //    {
                //        Query = File.ReadAllText(HostingEnvironment.MapPath("/Sql/SqlNewSef/KasaCiroNewSefim.sql"), System.Text.UTF8Encoding.Default);
                //    }
                //    else if (AppDbType == "2")
                //    {
                //        Query = File.ReadAllText(HostingEnvironment.MapPath("/Sql/KasaCiro.sql"), System.Text.UTF8Encoding.Default);
                //    }
                //    else if (AppDbType == "3")
                //    {
                //        //Query = File.ReadAllText(HostingEnvironment.MapPath("/Sql/SqlFaster/KasaCiroFASTER.sql"), System.Text.Encoding.UTF8);
                //    }
                //    #endregion             

                //    Query = Query.Replace("{SUBEADI}", SubeAdi);
                //    Query = Query.Replace("{TARIH1}", QueryTimeStart);
                //    Query = Query.Replace("{TARIH2}", QueryTimeEnd);

                //    if (Id == "1")
                //    {
                //        #region GET DATA                    
                //        try
                //        {
                //            string connString = "Provider=SQLOLEDB;Server=" + SubeIP + ";User Id=" + SqlName + ";Password=" + SqlPassword + ";Database=" + DBName + "";
                //            f.SqlConnOpen(true, connString);

                //            DataTable KasaCiroDt = f.DataTable(Query, true);
                //            foreach (DataRow SubeR in KasaCiroDt.Rows)
                //            {
                //                KasaCiro items = new KasaCiro();
                //                items.Sube = f.RTS(SubeR, "Sube");
                //                items.SubeId = SubeId;
                //                items.Cash = f.RTD(SubeR, "Cash");
                //                items.Credit = f.RTD(SubeR, "Credit");
                //                items.Ticket = f.RTD(SubeR, "Ticket");
                //                items.Ciro = f.RTD(SubeR, "Ciro");
                //                items.GelirGider = f.RTD(SubeR, "GelirGider");
                //                //items.GecenZaman = Convert.ToInt32(sure.TotalSeconds);
                //                //items.Ciro = items.Cash + items.Credit + items.Ticket;
                //                Liste.Add(items);
                //            }
                //            f.SqlConnClose(true);
                //        }
                //        catch (System.Exception ex)
                //        {
                //            //log metnini oluştur
                //            string ErrorMessage = "Kasa Raporu Alınamadı.";
                //            //string SystemErrorMessage = ex.Message.ToString();
                //            //string LogText = "";
                //            //LogText += "Hata Zamanı : " + DateTime.Now.ToString() + Environment.NewLine;
                //            //LogText += "Hata Mesajı : " + ErrorMessage + Environment.NewLine;
                //            //LogText += "Hata Açıklaması : " + SystemErrorMessage + Environment.NewLine;
                //            //LogText += "-----------------" + Environment.NewLine;

                //            KasaCiro items = new KasaCiro();
                //            items.Sube = SubeAdi + " (Erişim veya Data Yok) ";
                //            items.SubeId = SubeId;
                //            items.ErrorMessage = ErrorMessage;
                //            items.ErrorStatus = true;
                //            items.ErrorCode = "01";
                //            Liste.Add(items);


                //            //string LogFolder = "/Uploads/Logs/Error";
                //            //if (Directory.Exists(HostingEnvironment.MapPath(LogFolder)) == false) { Directory.CreateDirectory(HostingEnvironment.MapPath(LogFolder)); }
                //            //string LogFile = "Sube-" + SubeId + ".txt";
                //            //string LogFilePath = HostingEnvironment.MapPath(LogFolder + "/" + LogFile);


                //            //if (File.Exists(LogFilePath) == false)
                //            //{
                //            //    string FirstLine = "Created on " + DateTime.Now.ToString() + Environment.NewLine + Environment.NewLine;
                //            //    File.WriteAllText(LogFilePath, FirstLine);
                //            //}
                //            //File.AppendAllText(LogFilePath, LogText);
                //        }
                //        #endregion
                //    }
                //    else
                //    {
                //        #region KULLANICININ ŞUBE YETKİ KONTROLU YAPILIYOR             
                //        foreach (var item in model.FR_SubeListesi)
                //        {
                //            if (item.SubeID == Convert.ToInt32(SubeId))
                //            {
                //                #region GET DATA                    
                //                try
                //                {
                //                    string connString = "Provider=SQLOLEDB;Server=" + SubeIP + ";User Id=" + SqlName + ";Password=" + SqlPassword + ";Database=" + DBName + "";
                //                    f.SqlConnOpen(true, connString);
                //                    DataTable KasaCiroDt = f.DataTable(Query, true);
                //                    foreach (DataRow SubeR in KasaCiroDt.Rows)
                //                    {
                //                        KasaCiro items = new KasaCiro();
                //                        items.Sube = f.RTS(SubeR, "Sube");
                //                        items.SubeId = SubeId;
                //                        items.Cash = f.RTD(SubeR, "Cash");
                //                        items.Credit = f.RTD(SubeR, "Credit");
                //                        items.Ticket = f.RTD(SubeR, "Ticket");
                //                        items.Ciro = f.RTD(SubeR, "Ciro");
                //                        //items.GecenZaman = Convert.ToInt32(sure.TotalSeconds);

                //                        //items.Ciro = items.Cash + items.Credit + items.Ticket;
                //                        Liste.Add(items);

                //                    }
                //                    f.SqlConnClose(true);
                //                }
                //                catch (System.Exception ex)
                //                {
                //                    //log metnini oluştur
                //                    string ErrorMessage = "Kasa Raporu Alınamadı.";
                //                    //string SystemErrorMessage = ex.Message.ToString();
                //                    //string LogText = "";
                //                    //LogText += "Hata Zamanı : " + DateTime.Now.ToString() + Environment.NewLine;
                //                    //LogText += "Hata Mesajı : " + ErrorMessage + Environment.NewLine;
                //                    //LogText += "Hata Açıklaması : " + SystemErrorMessage + Environment.NewLine;
                //                    //LogText += "-----------------" + Environment.NewLine;

                //                    KasaCiro items = new KasaCiro();
                //                    items.Sube = SubeAdi + " (Erişim veya Data Yok) ";
                //                    items.SubeId = SubeId;
                //                    items.ErrorMessage = ErrorMessage;
                //                    items.ErrorStatus = true;
                //                    items.ErrorCode = "01";
                //                    Liste.Add(items);


                //                    //string LogFolder = "/Uploads/Logs/Error";
                //                    //if (Directory.Exists(HostingEnvironment.MapPath(LogFolder)) == false) { Directory.CreateDirectory(HostingEnvironment.MapPath(LogFolder)); }
                //                    //string LogFile = "Sube-" + SubeId + ".txt";
                //                    //string LogFilePath = HostingEnvironment.MapPath(LogFolder + "/" + LogFile);


                //                    //if (File.Exists(LogFilePath) == false)
                //                    //{
                //                    //    string FirstLine = "Created on " + DateTime.Now.ToString() + Environment.NewLine + Environment.NewLine;
                //                    //    File.WriteAllText(LogFilePath, FirstLine);
                //                    //}
                //                    //File.AppendAllText(LogFilePath, LogText);
                //                }
                //                #endregion
                //            }
                //        }
                //        #endregion
                //    }


                //}

                #endregion

            }
            catch (DataException ex) { }

            return Liste;
        }
    }
}