using SefimV2.ViewModels.User;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Hosting;

namespace SefimV2.Models
{
    public class AcikMasaIslemCRUD
    {
        public static List<AcikMasaIslem> List(DateTime Date1, DateTime Date2, string subeid, string masano, string ID)
        {
            List<AcikMasaIslem> Liste = new List<AcikMasaIslem>();
            List<AcikMasaIslem> ListeTotal = new List<AcikMasaIslem>();
            ModelFunctions ff = new ModelFunctions();
            DateTime startDate = DateTime.Now;

            #region GİRİŞ YAPAN KULLANICININ YETKİLİ OLDUĞU ŞUBELER LİSTELENİYOR
            UserViewModel model = UsersListCRUD.YetkiliSubesi(ID);
            #endregion
            string subeid_ = "";
            try
            {
                #region Kırılımlarda ilk once db de tanımlı subeyı alıyoruz.              
                if (!subeid.Equals(""))
                {
                    string[] tableNo = subeid.Split('~');
                    subeid_ = tableNo[0];
                }
                #endregion Kırılımlarda ilk once db de tanımlı subeyı alıyoruz.

                #region SUBSTATION LIST               
                ff.SqlConnOpen();
                string filter = "Where Status=1";
                if (subeid_ != null && !subeid_.Equals("0") && !subeid_.Equals(""))
                    filter += " and Id=" + subeid_;
                DataTable dt = ff.DataTable("select * from SubeSettings " + filter);
                ff.SqlConnClose();
                #endregion SUBSTATION LIST

                try
                {
                    #region PARALEL FOREACH
                    var dtList = dt.AsEnumerable().ToList<DataRow>();
                    Parallel.ForEach(dtList, r =>
                    {
                        ModelFunctions f = new ModelFunctions();

                        //foreach (DataRow r in dt.Rows)
                        //{        
                        string AppDbType = f.RTS(r, "AppDbType");
                        string AppDbTypeStatus = f.RTS(r, "AppDbTypeStatus");
                        string SubeId = r["Id"].ToString();
                        string SubeAdi = r["SubeName"].ToString();
                        string SubeIP = r["SubeIP"].ToString();
                        string SqlName = r["SqlName"].ToString();
                        string SqlPassword = r["SqlPassword"].ToString();
                        string DBName = r["DBName"].ToString();
                        string FirmaId = r["FirmaID"].ToString();
                        string FirmaId_SUBE = "F0" + r["FirmaID"].ToString() + "TBLKRDSUBELER";
                        string FirmaId_KASA = "F0" + r["FirmaID"].ToString() + "TBLKRDKASALAR";
                        string QueryTimeStart = Date1.ToString("yyyy/MM/dd HH:mm:ss");
                        string QueryTimeEnd = Date2.ToString("yyyy/MM/dd HH:mm:ss");
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

                        string Query = "";

                        #region Faster Kasalar seçildiyse ona göre query oluşturuluyor. 
                        string FasterSubeIND = f.RTS(r, "FasterSubeID");
                        string QueryFasterSube = string.Empty;
                        if (FasterSubeIND != null)
                        {
                            QueryFasterSube = "  and SUBEIND IN(" + FasterSubeIND + ") ";
                        }
                        #endregion Faster Kasalar seçildiyse ona göre query oluşturuluyor.

                        if (AppDbType == "1" || AppDbType == "2")// 1 = yeni şefim, 2 =eski Şefim, 3 = Faster
                        {
                            if (subeid.Equals(""))
                                Query = File.ReadAllText(HostingEnvironment.MapPath("/Sql/AcikMasalar.sql"), System.Text.UTF8Encoding.Default);// 1.List
                            if (!subeid.Equals("") && masano.Equals(""))
                                Query = File.ReadAllText(HostingEnvironment.MapPath("/Sql/AcikMasaDetay.sql"), System.Text.UTF8Encoding.Default);// 2.List
                            if (!subeid.Equals("") && !masano.Equals(""))
                                Query = File.ReadAllText(HostingEnvironment.MapPath("/Sql/AcikMasaUrunleriDetay.sql"), System.Text.UTF8Encoding.Default);// 3. List Detay
                        }
                        else if (AppDbType == "3")
                        {

                            #region Kırılımlarda Ana Şube Altındaki IND id alıyorum                      
                            if (!subeid.Equals(""))
                            {
                                string[] tableNo = subeid.Split('~');
                                if (tableNo.Length >= 2)
                                {
                                    subeid = tableNo[1];
                                }
                            }
                            #endregion Kırılımlarda Ana Şube Altındaki IND id alıyorum

                            #region MasaNo yu stringden kurtar detay için ID gönderiyor
                            if (!masano.Equals(""))
                            {
                                string[] tableNo = masano.Split('~');
                                if (tableNo.Length >= 2)
                                {
                                    masano = tableNo[1];
                                }
                            }
                            #endregion  MasaNo yu stringden kurtar detay için ID gönderiyor

                            if (AppDbTypeStatus == "True")
                            {
                                #region FASTER ONLINE QUARY  

                                if (subeid.Equals(""))

                                    #region FASTER (1. Liste)
                                    Query =
                                             " declare @Sube nvarchar(100) = '{SubeAdi}';" +
                                             " declare @par1 nvarchar(20) =   '{TARIH1}'; " +
                                             " declare @par2 nvarchar(20) ='{TARIH2}'; " +
                                             "  SELECT @Sube AS Sube, " +
                                             " (SELECT SUBEADI " +
                                             "  FROM  " + FirmaId_SUBE + "  " +
                                             "  WHERE IND = TBLFASTERBEKLEYENBASLIK.SUBEIND) AS Sube1, " +
                                             "   (SELECT KASAADI  " +
                                             "   FROM " + FirmaId_KASA + " " +
                                             "    WHERE IND = TBLFASTERBEKLEYENBASLIK.KASAIND) AS Kasa, " +
                                             "      (SELECT  IND " +
                                             "      FROM " + FirmaId_KASA + " " +
                                             "       WHERE IND = TBLFASTERBEKLEYENBASLIK.KASAIND) AS Id, " +
                                             "         FIRMAKODU, " +
                                             "          count(*) AS TOPLAM_MASA, " +
                                             "          sum(TUTAR) TOPLAM_TUTAR " +
                                             " FROM TBLFASTERBEKLEYENBASLIK " +
                                             "  WHERE ISLEMTARIHI >= @par1 " +
                                             "   AND ISLEMTARIHI<= @par2 " +
                                             "  " + QueryFasterSube +
                                             "  GROUP BY SUBEIND, " +
                                             "         KASAIND, " +
                                             "         FIRMAKODU  ";

                                #endregion FASTER (1. Liste)
                                if (!subeid.Equals("") && masano.Equals(""))

                                    #region FASTER (2.Liste Seçili Şubeye göre)
                                    Query =
                                                " declare @Sube nvarchar(100) = '{SubeAdi}';" +
                                                " declare @par1 nvarchar(20) =   '{TARIH1}'; " +
                                                " declare @par2 nvarchar(20) ='{TARIH2}';" +
                                                "   SELECT @Sube AS Sube, " +
                                                "  (SELECT SUBEADI  " +
                                                "   FROM  " + FirmaId_SUBE + " " +
                                                "   WHERE IND = TBLFASTERBEKLEYENBASLIK.SUBEIND) AS Sube1, " +
                                                "    (SELECT KASAADI   " +
                                                "     FROM  " + FirmaId_KASA + "   " +
                                                "     WHERE IND = TBLFASTERBEKLEYENBASLIK.KASAIND) AS Kasa, " +
                                                "      (SELECT  IND     " +
                                                "       FROM " + FirmaId_KASA + "    " +
                                                "       WHERE IND = TBLFASTERBEKLEYENBASLIK.KASAIND) AS Id, " +
                                                "           FIRMAKODU TableNumber, " +
                                                "       min(ISLEMTARIHI) AS Date, " +
                                                "       COUNT(FIRMAKODU) AS TOPLAM_MASA, " +
                                                "       sum(TUTAR) AS TUTAR, " +
                                                "       BASLIKIND " +
                                                " FROM TBLFASTERBEKLEYENBASLIK " +
                                                " WHERE ISLEMTARIHI>= @par1 " +
                                                "  AND ISLEMTARIHI<= @par2 " +
                                                 "  " + QueryFasterSube +
                                                " GROUP BY SUBEIND, " +
                                                "         KASAIND, " +
                                                "         FIRMAKODU, " +
                                                "         BASLIKIND " +
                                                " HAVING COUNT(FIRMAKODU) > 0 ";

                                #endregion FASTER (2.Liste Seçili Şubeye göre)
                                if (!subeid.Equals("") && !masano.Equals(""))

                                    #region FASTER (3. Liste Seçili masa numarasının detayı)                                       
                                    Query =
                                            " declare @Sube nvarchar(100) = '{SubeAdi}';" +
                                            " declare @par1 nvarchar(20) =   '{TARIH1}'; " +
                                            " declare @par2 nvarchar(20) ='{TARIH2}';" +
                                            " declare @par3 int = '{TableNumber}'; " +
                                            "   SELECT @Sube AS Sube,  " +
                                            "  (SELECT SUBEADI" +
                                            "   FROM " + FirmaId_SUBE + "  " +
                                            "   WHERE IND = FBB.SUBEIND) AS Sube1," +
                                            "    (SELECT KASAADI " +
                                            "     FROM " + FirmaId_KASA + "  " +
                                            "     WHERE IND = FBB.KASAIND) AS Kasa," +
                                            "      (SELECT IND FROM " + FirmaId_KASA + "    " +
                                            "       WHERE IND = FBB.KASAIND) AS Id, " +
                                            "             FBB.FIRMAKODU TableNumber, " +
                                            "             STK.MALINCINSI ProductName, " +
                                            "  FBH.PERKODU, " +
                                            "             FBH.ISLEMTARIHI Date, (MIKTAR * SATISFIYATI) AS TUTAR " +
                                            " FROM TBLFASTERBEKLEYENHAREKET FBH " +
                                            " LEFT JOIN TBLFASTERBEKLEYENBASLIK FBB ON FBB.BASLIKIND = FBH.BASLIKIND " +
                                            " LEFT JOIN F0" + FirmaId + "TBLSTOKLAR STK ON STK.IND = FBH.STOKIND " +
                                            " WHERE FBH.ISLEMTARIHI >= @par1 " +
                                            "  AND FBH.ISLEMTARIHI <= @par2 " +
                                             "  " + QueryFasterSube +
                                            "  AND FBB.BASLIKIND = @par3 ";
                                #endregion  FASTER (3. Liste Seçili masa numarasının detayı)

                                #endregion  FASTER ONLINE QUARY
                            }
                            else
                            {
                                #region FASTER OFFLINE QUARY

                                if (subeid.Equals(""))
                                    Query = File.ReadAllText(HostingEnvironment.MapPath("/Sql/SqlFaster/AcikMasalar/AcikMasalarFasterOFLINE.sql"), System.Text.UTF8Encoding.Default);// 1.List
                                if (!subeid.Equals("") && masano.Equals(""))
                                    Query = File.ReadAllText(HostingEnvironment.MapPath("/Sql/SqlFaster/AcikMasalar/AcikMasalarDetayFasterOFLINE.sql"), System.Text.UTF8Encoding.Default);// 2.List
                                if (!subeid.Equals("") && !masano.Equals(""))
                                    Query = File.ReadAllText(HostingEnvironment.MapPath("/Sql/SqlFaster/AcikMasalar/AcikMasalarUrunDetayFasterOFLINE.sql"), System.Text.UTF8Encoding.Default); // 3. List Detay
                                #endregion FASTER OFFLINE QUARY
                            }
                        }
                        else if (AppDbType == "5")
                        {
                            if (subeid.Equals(""))
                                Query = File.ReadAllText(HostingEnvironment.MapPath("/Sql/VPos/AcikMasaRaporu/AcikMasaRaporu1.sql"), System.Text.UTF8Encoding.Default);// 1.List
                            if (!subeid.Equals("") && masano.Equals(""))
                                Query = File.ReadAllText(HostingEnvironment.MapPath("/Sql/VPos/AcikMasaRaporu/AcikMasaDetay.sql"), System.Text.UTF8Encoding.Default);// 2.List
                            if (!subeid.Equals("") && !masano.Equals(""))
                                Query = File.ReadAllText(HostingEnvironment.MapPath("/Sql/VPos/AcikMasaRaporu/AcikMasaUrunleriDetay.sql"), System.Text.UTF8Encoding.Default);// 3.List

                        }
                        #endregion

                        Query = Query.Replace("{TARIH1}", QueryTimeStart);
                        Query = Query.Replace("{TARIH2}", QueryTimeEnd);
                        Query = Query.Replace("{SubeAdi}", SubeAdi);
                        Query = Query.Replace("{SUBE2}", vPosSubeKodu);
                        Query = Query.Replace("{KASAKODU}", vPosKasaKodu);
                        if (!subeid.Equals("") && !masano.Equals(""))
                            Query = Query.Replace("{TableNumber}", masano);

                        if (ID == "1")
                        {
                            #region GET DATA
                            try
                            {
                                string connString = "Provider=SQLOLEDB;Server=" + SubeIP + ";User Id=" + SqlName + ";Password=" + SqlPassword + ";Database=" + DBName + "";
                                try
                                {
                                    //f.SqlConnOpen(true, connString);
                                    //DataTable AcikHesapDt = f.DataTable(Query, true);
                                    DataTable AcikHesapDt = new DataTable();
                                    AcikHesapDt = f.GetSubeDataWithQuery((f.NewConnectionString(SubeIP, DBName, SqlName, SqlPassword)), Query.ToString());

                                    if (AcikHesapDt.Rows.Count > 0)
                                    {
                                        if (subeid.Equals(""))
                                        {
                                            if (AppDbType == "3")
                                            {
                                                #region FASTER (AppDbType=3 faster kullanan şube)
                                                foreach (DataRow sube in AcikHesapDt.Rows)
                                                {
                                                    AcikMasaIslem items_ = new AcikMasaIslem();
                                                    //items_.SubeID = Convert.ToInt32(SubeId);
                                                    items_.SubeID = SubeId + "~" + (sube["Id"].ToString());
                                                    items_.Sube = SubeAdi + "-" + sube["Sube1"].ToString() + "-" + sube["Kasa"].ToString();
                                                    items_.MasaSayisi = Convert.ToInt32((sube["TOPLAM_MASA"]));
                                                    items_.Debit = Convert.ToDecimal((sube["TOPLAM_TUTAR"]));
                                                    items_.TotalDebit += Convert.ToDecimal((sube["TOPLAM_TUTAR"]));
                                                    //foreach (DataRow SubeR in AcikHesapDt.Rows)
                                                    //{
                                                    //    items_.MasaSayisi += 1; //Convert.ToInt32(f.RTS(SubeR, "TOPLAM_MASA"));
                                                    //    items_.TotalDebit += f.RTD(SubeR, "TOPLAM_TUTAR");
                                                    //}
                                                    Liste.Add(items_);
                                                }
                                                #endregion  FASTER (AppDbType=3 faster kullanan şube)
                                            }
                                            else
                                            {
                                                AcikMasaIslem items = new AcikMasaIslem();
                                                items.SubeID = (SubeId);
                                                items.Sube = SubeAdi;
                                                //items.TarihMin = f.RTS(AcikHesapDt.Rows[0], "TarihMin");
                                                //items.TarihMax = f.RTS(AcikHesapDt.Rows[0], "TarihMax");
                                                items.TableNumber = f.RTS(AcikHesapDt.Rows[0], "TOPLAM_MASA");
                                                items.Debit = f.RTD(AcikHesapDt.Rows[0], "TOPLAM_TUTAR");
                                                foreach (DataRow SubeR in AcikHesapDt.Rows)
                                                {
                                                    items.MasaSayisi += 1; //Convert.ToInt32(f.RTS(SubeR, "TOPLAM_MASA"));
                                                    items.TotalDebit += f.RTD(SubeR, "TOPLAM_TUTAR");
                                                }
                                                Liste.Add(items);
                                            }
                                        }
                                        else
                                        {
                                            if (AppDbType == "3")
                                            {
                                                #region FASTER -2. Masa Detay (AppDbType=3 faster kullanan şube )
                                                foreach (DataRow SubeR in AcikHesapDt.Rows)
                                                {
                                                    if (subeid == SubeR["Id"].ToString())
                                                    {
                                                        AcikMasaIslem items = new AcikMasaIslem();
                                                        items.SubeID = (SubeId);
                                                        items.Sube = SubeAdi;
                                                        items.TarihMin = Convert.ToDateTime(f.RTS(SubeR, "Date")).ToString();
                                                        ////items.TarihMax = f.RTS(SubeR, "TarihMax");
                                                        items.TableNumber = f.RTS(SubeR, "TableNumber") + " ~" + f.RTS(SubeR, "BASLIKIND");
                                                        items.Debit = f.RTD(SubeR, "TUTAR");
                                                        //items.TotalDebit += f.RTD(SubeR, "Tutar");
                                                        //items.GecenSure = DateTime.Now.ToShortTimeString();
                                                        items.GecenSure = (DateTime.Now - (Convert.ToDateTime(items.TarihMin))).ToString().Substring(0, 5);//(Convert.ToDateTime(DateTime.Now - (Convert.ToDateTime(items.TarihMin)))).ToShortTimeString();
                                                        if (!subeid.Equals("") && masano != null)
                                                            items.ProductName += f.RTS(SubeR, "ProductName");

                                                        items.UserName = f.RTS(SubeR, "PERKODU");
                                                        Liste.Add(items);
                                                    }
                                                }
                                                #endregion  FASTER -2. Masa Detay (AppDbType=3 faster kullanan şube )
                                            }
                                            else
                                            {
                                                foreach (DataRow SubeR in AcikHesapDt.Rows)
                                                {
                                                    AcikMasaIslem items = new AcikMasaIslem();
                                                    items.SubeID = (SubeId);
                                                    items.Sube = SubeAdi;
                                                    items.TarihMin = Convert.ToDateTime(f.RTS(SubeR, "Date")).ToString();
                                                    ////items.TarihMax = f.RTS(SubeR, "TarihMax");
                                                    items.TableNumber = f.RTS(SubeR, "TableNumber");
                                                    items.Debit = f.RTD(SubeR, "TUTAR");
                                                    //items.TotalDebit += f.RTD(SubeR, "Tutar");
                                                    //items.GecenSure = DateTime.Now.ToShortTimeString();
                                                    //items.GecenSure = (DateTime.Now - (Convert.ToDateTime(items.TarihMin))).ToString().Substring(0, 5);//(Convert.ToDateTime(DateTime.Now - (Convert.ToDateTime(items.TarihMin)))).ToShortTimeString();
                                                    var dtd = (DateTime.Now - (Convert.ToDateTime(items.TarihMin)));
                                                    items.GecenSure = dtd.Days + " gün - " + dtd.Hours + " saat - " + dtd.Minutes + " dk.";

                                                    if (!subeid.Equals("") && masano != null)
                                                        items.ProductName += f.RTS(SubeR, "ProductName");
                                                    items.UserName = f.RTS(SubeR, "username");

                                                    Liste.Add(items);
                                                }

                                            }
                                        }
                                        //f.SqlConnClose(true);
                                    }
                                    else
                                    {
                                        AcikMasaIslem items = new AcikMasaIslem();
                                        items.Sube = SubeAdi + " (Data Yok) ";
                                        items.SubeID = (SubeId);
                                        Liste.Add(items);
                                    }
                                }
                                catch (Exception) { throw new Exception(SubeAdi); }
                            }
                            catch (System.Exception ex)
                            {
                                #region MyRegion                          
                                //log metnini oluştur
                                string ErrorMessage = "Kasa Raporu Alınamadı.";
                                //string SystemErrorMessage = ex.Message.ToString();
                                //string LogText = "";
                                //LogText += "Hata Zamanı : " + DateTime.Now.ToString() + Environment.NewLine;
                                //LogText += "Hata Mesajı : " + ErrorMessage + Environment.NewLine;
                                //LogText += "Hata Açıklaması : " + SystemErrorMessage + Environment.NewLine;
                                //LogText += "-----------------" + Environment.NewLine;
                                AcikMasaIslem items = new AcikMasaIslem();
                                items.Sube = ex.Message + " (Erişim Yok) ";
                                items.SubeID = (SubeId);
                                //items.CustomerName = cu;
                                //items.Debit = SubeId;
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
                                            //f.SqlConnOpen(true, connString);
                                            //DataTable AcikHesapDt = f.DataTable(Query, true);
                                            DataTable AcikHesapDt = new DataTable();
                                            AcikHesapDt = f.GetSubeDataWithQuery((f.NewConnectionString(SubeIP, DBName, SqlName, SqlPassword)), Query.ToString());

                                            if (AcikHesapDt.Rows.Count > 0)
                                            {
                                                if (subeid.Equals(""))
                                                {
                                                    if (AppDbType == "3")
                                                    {
                                                        #region FASTER (AppDbType=3 faster kullanan şube)
                                                        foreach (DataRow sube in AcikHesapDt.Rows)
                                                        {
                                                            AcikMasaIslem items_ = new AcikMasaIslem();
                                                            //items_.SubeID = Convert.ToInt32(SubeId);
                                                            items_.SubeID = SubeId + "~" + (sube["Id"].ToString());
                                                            items_.Sube = SubeAdi + "-" + sube["Sube1"].ToString() + "-" + sube["Kasa"].ToString();
                                                            items_.MasaSayisi = Convert.ToInt32((sube["TOPLAM_MASA"]));
                                                            items_.Debit = Convert.ToDecimal((sube["TOPLAM_TUTAR"]));
                                                            items_.TotalDebit += Convert.ToDecimal((sube["TOPLAM_TUTAR"]));
                                                            //foreach (DataRow SubeR in AcikHesapDt.Rows)
                                                            //{
                                                            //    items_.MasaSayisi += 1; //Convert.ToInt32(f.RTS(SubeR, "TOPLAM_MASA"));
                                                            //    items_.TotalDebit += f.RTD(SubeR, "TOPLAM_TUTAR");
                                                            //}
                                                            Liste.Add(items_);
                                                        }
                                                        #endregion  FASTER (AppDbType=3 faster kullanan şube)
                                                    }
                                                    else
                                                    {
                                                        AcikMasaIslem items = new AcikMasaIslem();
                                                        items.SubeID = (SubeId);
                                                        items.Sube = SubeAdi;
                                                        //items.TarihMin = f.RTS(AcikHesapDt.Rows[0], "TarihMin");
                                                        //items.TarihMax = f.RTS(AcikHesapDt.Rows[0], "TarihMax");
                                                        items.TableNumber = f.RTS(AcikHesapDt.Rows[0], "TOPLAM_MASA");
                                                        items.Debit = f.RTD(AcikHesapDt.Rows[0], "TOPLAM_TUTAR");
                                                        foreach (DataRow SubeR in AcikHesapDt.Rows)
                                                        {
                                                            items.MasaSayisi += 1; //Convert.ToInt32(f.RTS(SubeR, "TOPLAM_MASA"));
                                                            items.TotalDebit += f.RTD(SubeR, "TOPLAM_TUTAR");
                                                        }
                                                        Liste.Add(items);
                                                    }
                                                }
                                                else
                                                {
                                                    if (AppDbType == "3")
                                                    {
                                                        #region FASTER -2. Masa Detay (AppDbType=3 faster kullanan şube )
                                                        foreach (DataRow SubeR in AcikHesapDt.Rows)
                                                        {
                                                            if (subeid == SubeR["Id"].ToString())
                                                            {
                                                                AcikMasaIslem items = new AcikMasaIslem();
                                                                items.SubeID = (SubeId);
                                                                items.Sube = SubeAdi;
                                                                items.TarihMin = Convert.ToDateTime(f.RTS(SubeR, "Date")).ToString();
                                                                ////items.TarihMax = f.RTS(SubeR, "TarihMax");
                                                                items.TableNumber = f.RTS(SubeR, "TableNumber") + " ~" + f.RTS(SubeR, "BASLIKIND");
                                                                items.Debit = f.RTD(SubeR, "TUTAR");
                                                                //items.TotalDebit += f.RTD(SubeR, "Tutar");
                                                                //items.GecenSure = DateTime.Now.ToShortTimeString();
                                                                items.GecenSure = (DateTime.Now - (Convert.ToDateTime(items.TarihMin))).ToString().Substring(0, 5);//(Convert.ToDateTime(DateTime.Now - (Convert.ToDateTime(items.TarihMin)))).ToShortTimeString();
                                                                if (!subeid.Equals("") && masano != null)
                                                                    items.ProductName += f.RTS(SubeR, "ProductName");

                                                                items.UserName = f.RTS(SubeR, "PERKODU");
                                                                Liste.Add(items);
                                                            }
                                                        }
                                                        #endregion  FASTER -2. Masa Detay (AppDbType=3 faster kullanan şube )
                                                    }
                                                    else
                                                    {
                                                        foreach (DataRow SubeR in AcikHesapDt.Rows)
                                                        {
                                                            AcikMasaIslem items = new AcikMasaIslem();
                                                            items.SubeID = (SubeId);
                                                            items.Sube = SubeAdi;
                                                            items.TarihMin = Convert.ToDateTime(f.RTS(SubeR, "Date")).ToString();
                                                            ////items.TarihMax = f.RTS(SubeR, "TarihMax");
                                                            items.TableNumber = f.RTS(SubeR, "TableNumber");
                                                            items.Debit = f.RTD(SubeR, "TUTAR");
                                                            //items.TotalDebit += f.RTD(SubeR, "Tutar");
                                                            //items.GecenSure = DateTime.Now.ToShortTimeString();
                                                            //items.GecenSure = (DateTime.Now - (Convert.ToDateTime(items.TarihMin))).ToString().Substring(0, 5);//(Convert.ToDateTime(DateTime.Now - (Convert.ToDateTime(items.TarihMin)))).ToShortTimeString();
                                                            var dtd = (DateTime.Now - (Convert.ToDateTime(items.TarihMin)));
                                                            items.GecenSure = dtd.Days + " gün - " + dtd.Hours + " saat - " + dtd.Minutes + " dk.";

                                                            if (!subeid.Equals("") && masano != null)
                                                                items.ProductName += f.RTS(SubeR, "ProductName");
                                                            items.UserName = f.RTS(SubeR, "username");

                                                            Liste.Add(items);
                                                        }

                                                    }
                                                }
                                                //f.SqlConnClose(true);
                                            }
                                            else
                                            {
                                                AcikMasaIslem items = new AcikMasaIslem();
                                                items.Sube = SubeAdi + " (Data Yok) ";
                                                items.SubeID = (SubeId);
                                                Liste.Add(items);
                                            }
                                        }
                                        catch (Exception) { throw new Exception(SubeAdi); }
                                    }
                                    catch (System.Exception ex)
                                    {
                                        #region MyRegion                          
                                        //log metnini oluştur
                                        string ErrorMessage = "Kasa Raporu Alınamadı.";
                                        //string SystemErrorMessage = ex.Message.ToString();
                                        //string LogText = "";
                                        //LogText += "Hata Zamanı : " + DateTime.Now.ToString() + Environment.NewLine;
                                        //LogText += "Hata Mesajı : " + ErrorMessage + Environment.NewLine;
                                        //LogText += "Hata Açıklaması : " + SystemErrorMessage + Environment.NewLine;
                                        //LogText += "-----------------" + Environment.NewLine;
                                        AcikMasaIslem items = new AcikMasaIslem();
                                        items.Sube = ex.Message + " (Erişim Yok) ";
                                        items.SubeID = (SubeId);
                                        //items.CustomerName = cu;
                                        //items.Debit = SubeId;
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
                    #endregion PARALEL FOREACH
                }
                catch (Exception eee)
                { }
            }
            catch (DataException ex) { }
            return Liste;
        }


        public static DataTable Subeler()
        {
            ModelFunctions f = new ModelFunctions();
            DataTable dt = new DataTable();
            DateTime startDate = DateTime.Now;
            try
            {
                f.SqlConnOpen();
                string filter = "Where Status=1";
                dt = f.DataTable("select * from SubeSettings " + filter);
            }
            catch (Exception ex) { }
            return dt;
        }
    }
}