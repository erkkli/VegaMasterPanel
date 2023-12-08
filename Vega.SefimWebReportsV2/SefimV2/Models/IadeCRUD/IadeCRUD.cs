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
    public class IadeCRUD
    {
        public static List<IadeViewModel> List(DateTime Date1, DateTime Date2, string subeid, string ID, string productGroup)
        {
            if (productGroup == "alt=\"expand/collapse\"")
            {
                productGroup = "NULL";
            }

            List<IadeViewModel> Liste = new List<IadeViewModel>();
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

                #region PARALLEL FOREACH
                var dtList = dt.AsEnumerable().ToList<DataRow>();
                Parallel.ForEach(dtList, r =>
                {
                    ModelFunctions f = new ModelFunctions();
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
                  
                    string Query = string.Empty;

                    #region Faster Kasalar seçildiyse ona göre query oluşturuluyor. 
                    string FasterSubeIND = f.RTS(r, "FasterSubeID");
                    string QueryFasterSube = string.Empty;
                    if (FasterSubeIND != null)
                    {
                        QueryFasterSube = "  and  FSH.SUBEIND IN(" + FasterSubeIND + ") ";
                    }
                    #endregion Faster Kasalar seçildiyse ona göre query oluşturuluyor.


                    if (AppDbType == "1" || AppDbType == "2")// 1 = yeni şefim, 2 =eski Şefim, 3 = Faster
                    {
                        if (subeid != null && !subeid.Equals("0") && productGroup == null)// sube secili degilse ilk giris yapilan sql
                            Query = File.ReadAllText(HostingEnvironment.MapPath("/Sql/SqlNewSef/IadeRaporu/IadeRaporu1.sql"), System.Text.UTF8Encoding.Default);
                        if (subeid != null || !subeid.Equals("0") && productGroup == "0")
                            Query = File.ReadAllText(HostingEnvironment.MapPath("/Sql/SqlNewSef/IadeRaporu/IadeRaporu2.sql"), System.Text.UTF8Encoding.Default);
                        if (subeid != null && !subeid.Equals("0") && productGroup != null && productGroup != "0")
                            Query = File.ReadAllText(HostingEnvironment.MapPath("/Sql/SqlNewSef/IadeRaporu/IadeRaporu3.sql"), System.Text.UTF8Encoding.Default);
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

                            #region faster için (aynı ID de subeler var bunları ilk listede tek şubenın urungruplarını alıyoruz.Sonra 2. kırılımı almak için productGrubu False yaparak içeri alıyoruz.)
                            if (productGroup == null)
                            {
                                productGroup = "False";
                            }
                            #endregion
                        }
                        #endregion #region Kırılımlarda Ana Şube Altındaki IND id alıyorum 

                        if (AppDbTypeStatus == "True")
                        {
                            #region FASTER ONLINE QUARY
                            if (subeid != null && !subeid.Equals("0") && productGroup == null)// sube secili degilse ilk giris yapilan sql
                                Query =
                                                            " DECLARE @FirmaInd nvarchar(100) = '{FIRMAIND}'; " +
                                                            " DECLARE @Sube nvarchar(100) = '{SUBE}'" +
                                                            " DECLARE @Trh1 nvarchar(20) = '{TARIH1}';" +
                                                            " DECLARE @Trh2 nvarchar(20) = '{TARIH2}';" +
                                                            "  SELECT @Sube AS Sube,  " +
                                                            " (SELECT TOP 1 SUBEADI " +
                                                            "  FROM F0" + FirmaId + "TBLKRDSUBELER " +
                                                            "  WHERE IND = FO.SUBEIND) AS Sube1, " +
                                                            " (SELECT KASAADI " +
                                                            "  FROM F0" + FirmaId + "TBLKRDKASALAR " +
                                                            "  WHERE IND = FO.KASAIND) AS Kasa, " +
                                                            " (SELECT IND " +
                                                            " FROM F0" + FirmaId + "TBLKRDKASALAR " +
                                                            " WHERE IND = FO.KASAIND) AS Id, " +
                                                            "     SUM(ISNULL(ODENEN, 0)) Tutar " +
                                                            "   FROM DBO.TBLFASTERODEMELER FO " +
                                                            "  LEFT JOIN DBO.TBLFASTERUSERS USR ON FO.USERIND = USR.IND " +
                                                            "  WHERE ISNULL(FO.IADE, 0) = 1 " +
                                                            "    AND ODEMETIPI IN(0, " +
                                                            "                     1, " +
                                                            "                     2, " +
                                                            "                     4) " +
                                                            "   AND ISLEMTARIHI >= @Trh1 " +
                                                            "   AND ISLEMTARIHI <= @Trh2 " +
                                                            " " + QueryFasterSube +
                                                            " GROUP BY " +
                                                            " FO.SUBEIND,FO.KASAIND";

                            if (subeid != null || !subeid.Equals("0") && productGroup == "0")
                                Query =
                                        " DECLARE @FirmaInd nvarchar(100) = '{FIRMAIND}'; " +
                                        " DECLARE @Sube nvarchar(100) = '{SUBE}'" +
                                        " DECLARE @Trh1 nvarchar(20) = '{TARIH1}';" +
                                        " DECLARE @Trh2 nvarchar(20) = '{TARIH2}';" +
                                        " SELECT T.Sube1," +
                                        "       T.Kasa," +
                                        "       T.Id," +
                                        "       SUM(T.MIKTAR) MIKTAR," +
                                        "       SUM(T.TUTAR) TUTAR," +
                                        "       T.ProductName" +
                                        " FROM (" +
                                        "        (SELECT" +
                                        "           (SELECT SUBEADI" +
                                        "            FROM F0" + FirmaId + "TBLKRDSUBELER" +
                                        "            WHERE IND=FSB.SUBEIND) AS Sube1," +
                                        "           (SELECT KASAADI" +
                                        "            FROM F0" + FirmaId + "TBLKRDKASALAR" +
                                        "            WHERE IND=FSB.KASAIND) AS Kasa," +
                                        "           (SELECT IND" +
                                        "            FROM F0" + FirmaId + "TBLKRDKASALAR" +
                                        "            WHERE IND=FSB.KASAIND) AS Id," +
                                        "                          SUM(FSH.MIKTAR) AS MIKTAR," +
                                        "                          SUM((((MIKTAR*SATISFIYATI) * (100-ISNULL(FSH.ISK1, 0))/100) * (100-ISNULL(FSH.ISK2, 0))/100) * (100-ISNULL(FSB.ALTISKORAN, 0))/100)  AS TUTAR," +
                                        "                          STK.MALINCINSI AS ProductName" +
                                        "         FROM TBLFASTERSATISHAREKET AS FSH" +
                                        "         LEFT JOIN TBLFASTERSATISBASLIK AS FSB ON FSH.BASLIKIND=FSB.BASLIKIND" +
                                        "         AND FSH.SUBEIND=FSB.SUBEIND" +
                                        "         AND FSH.KASAIND=FSB.KASAIND" +
                                        "         LEFT JOIN F0" + FirmaId + "TBLSTOKLAR AS STK ON FSH.STOKIND=STK.IND" +
                                        "         WHERE FSH.ISLEMTARIHI>=@Trh1" +
                                        "           AND FSH.ISLEMTARIHI<=@Trh2" +
                                        "           AND ISNULL(FSB.IADE, 0)=1" +
                                        " " + QueryFasterSube +
                                        "         GROUP BY FSB.SUBEIND," +
                                        "                  FSB.KASAIND," +
                                        "                  STK.MALINCINSI)) T" +
                                        " GROUP BY T.Sube1," +
                                        "         T.Kasa," +
                                        "         T.Id," +
                                        "         T.ProductName ";


                            if (subeid != null && !subeid.Equals("0") && productGroup != null && productGroup != "0")
                                Query =
                                        " DECLARE @FirmaInd nvarchar(100) = '{FIRMAIND}'; " +
                                        " DECLARE @Sube nvarchar(100) = '{SUBE}'" +
                                        " DECLARE @Trh1 nvarchar(20) = '{TARIH1}';" +
                                        " DECLARE @Trh2 nvarchar(20) = '{TARIH2}';" +
                                        "     SELECT @Sube AS Sube," +
                                        "     (SELECT SUBEADI" +
                                        "      FROM F0" + FirmaId + "TBLKRDSUBELER" +
                                        "      WHERE IND = FO.SUBEIND) AS Sube1," +
                                        "     (SELECT KASAADI" +
                                        "      FROM F0" + FirmaId + "TBLKRDKASALAR" +
                                        "      WHERE IND = FO.KASAIND) AS Kasa," +
                                        "	  FO.IND Id, TARIH Date,USR.USERNAME UserName,'' ProductName,0 ProductId,0 Choice1Id,0 Choice2Id,'' Options,ODENEN Price,0 Quantity,'' Comment,0 HeaderId,0 OrderId" +
                                        "   FROM DBO.TBLFASTERODEMELER FO" +
                                        "   LEFT JOIN DBO.TBLUSERS USR ON FO.USERIND=USR.IND" +
                                        "   WHERE ISNULL(FO.IADE, 0) = 1" +
                                        "     AND ODEMETIPI IN (0," +
                                        "                       1," +
                                        "                       2," +
                                        "                       4)" +
                                        "     AND ISLEMTARIHI >= @Trh1" +
                                        "     AND ISLEMTARIHI <= @Trh2" +
                                          " " + QueryFasterSube;




                            #endregion FASTER ONLINE QUARY
                        }
                        else
                        {
                            if (subeid != null && !subeid.Equals("0") && productGroup == null)// sube secili degilse ilk giris yapilan sql
                                Query = File.ReadAllText(HostingEnvironment.MapPath("/Sql/SqlFaster/IadeRaporu/IadeRaporuFasterOFLINE1.sql"), System.Text.UTF8Encoding.Default);

                            if (subeid != null || !subeid.Equals("0") && productGroup == "0")
                                Query = File.ReadAllText(HostingEnvironment.MapPath("/Sql/SqlFaster/IadeRaporu/IadeRaporuFasterOFLINE2.sql"), System.Text.UTF8Encoding.Default);

                            if (subeid != null && !subeid.Equals("0") && productGroup != null && productGroup != "0")
                                Query = File.ReadAllText(HostingEnvironment.MapPath("/Sql/SqlFaster/IadeRaporu/IadeRaporuFasterOFLINE1.sql"), System.Text.UTF8Encoding.Default);
                        }
                    }
                    else if (AppDbType == "5")
                    {
                        if (subeid != null && !subeid.Equals("0") && productGroup == null)// sube secili degilse ilk giris yapilan sql
                            Query = File.ReadAllText(HostingEnvironment.MapPath("/Sql/VPos/IadeRaporu/IadeRaporu1.sql"), System.Text.UTF8Encoding.Default);
                        if (subeid != null || !subeid.Equals("0") && productGroup == "0")
                            Query = File.ReadAllText(HostingEnvironment.MapPath("/Sql/VPos/IadeRaporu/IadeRaporu2.sql"), System.Text.UTF8Encoding.Default);
                        if (subeid != null && !subeid.Equals("0") && productGroup != null && productGroup != "0")
                            Query = File.ReadAllText(HostingEnvironment.MapPath("/Sql/VPos/IadeRaporu/IadeRaporu3.sql"), System.Text.UTF8Encoding.Default);
                    }
                    #endregion


                    Query = Query.Replace("{TARIH1}", QueryTimeStart);
                    Query = Query.Replace("{TARIH2}", QueryTimeEnd);
                    Query = Query.Replace("{SUBE}", SubeAdi);
                    Query = Query.Replace("{FIRMAIND}", FirmaId);
                    Query = Query.Replace("{ProductName}", productGroup);
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
                                DataTable AcikHesapDt = new DataTable();
                                AcikHesapDt = f.GetSubeDataWithQuery((f.NewConnectionString(SubeIP, DBName, SqlName, SqlPassword)), Query.ToString());

                                if (AcikHesapDt.Rows.Count > 0)
                                {
                                    if (subeid.Equals(""))
                                    {
                                        if (AppDbType == "3")
                                        {
                                            foreach (DataRow sube in AcikHesapDt.Rows)
                                            {
                                                IadeViewModel items = new IadeViewModel();
                                                items.Sube = SubeAdi + "-" + sube["Sube1"].ToString() + "-" + sube["Kasa"].ToString();
                                                items.SubeID = SubeId + "~" + (sube["Id"].ToString());
                                                ////items.PersonelAdi = "Kasa Satış";
                                                ////items.Total += f.RTD(sube, "Total");
                                                //}
                                                items.SumQuantity += 0;//f.RTD(sube, "Miktar");
                                                items.SumTotal += f.RTD(sube, "Tutar");
                                                Liste.Add(items);
                                            }
                                        }
                                        else
                                        {
                                            IadeViewModel items = new IadeViewModel();
                                            items.Sube = SubeAdi; //f.RTS(SubeUrunCiroDt.Rows[0], "Sube");
                                            items.SubeID = SubeId;
                                            foreach (DataRow SubeR in AcikHesapDt.Rows)
                                            {
                                                items.SumQuantity += f.RTD(SubeR, "Miktar");
                                                items.SumTotal += f.RTD(SubeR, "Tutar");
                                            }
                                            Liste.Add(items);
                                        }
                                    }
                                    else if (!subeid.Equals("") && productGroup == "0") /*DETAIL*/
                                    {
                                        if (AppDbType == "3")
                                        {
                                            foreach (DataRow SubeR in AcikHesapDt.Rows)
                                            {
                                                if (subeid == SubeR["Id"].ToString())
                                                {
                                                    IadeViewModel items = new IadeViewModel();
                                                    items.Sube = SubeAdi;
                                                    items.SubeID = (SubeId);
                                                    items.ProductName = f.RTS(SubeR, "ProductName");
                                                    items.SumQuantity = f.RTD(SubeR, "MIKTAR");
                                                    items.SumTotal = f.RTD(SubeR, "TUTAR");
                                                    Liste.Add(items);
                                                }
                                            }
                                        }
                                        else if (!subeid.Equals("") && productGroup != "null")
                                        {
                                            #region 2.KIRILIM SEFIM ESKI/YENI 

                                            foreach (DataRow SubeR in AcikHesapDt.Rows)
                                            {
                                                IadeViewModel items = new IadeViewModel();
                                                items.Sube = SubeAdi;
                                                items.SubeID = (SubeId);
                                                items.ProductName = f.RTS(SubeR, "ProductName");
                                                if (!subeid.Equals("0"))
                                                {
                                                    items.SumQuantity = f.RTD(SubeR, "Miktar");
                                                    items.SumTotal = f.RTD(SubeR, "Tutar");
                                                }
                                                Liste.Add(items);
                                            }

                                            #endregion 2.KIRILIM SEFIM ESKI/YENI 
                                        }
                                    }
                                    else
                                    {
                                        if (AppDbType == "3")
                                        {
                                            foreach (DataRow SubeR in AcikHesapDt.Rows)
                                            {
                                                if (subeid == SubeR["Id"].ToString())
                                                {
                                                    IadeViewModel items = new IadeViewModel();
                                                    items.Sube = SubeAdi;
                                                    items.SubeID = (SubeId);
                                                    items.UserName = f.RTS(SubeR, "UserName");
                                                    items.ProductName = f.RTS(SubeR, "ProductName");
                                                    items.Quantity = f.RTD(SubeR, "Quantity");
                                                    items.Price = f.RTD(SubeR, "Price");
                                                    items.DateTime = f.RTS(SubeR, "Date");
                                                    Liste.Add(items);
                                                }
                                            }
                                        }
                                        else
                                        {
                                            foreach (DataRow SubeR in AcikHesapDt.Rows)
                                            {
                                                IadeViewModel items = new IadeViewModel();
                                                items.Sube = SubeAdi;
                                                items.SubeID = (SubeId);
                                                items.UserName = f.RTS(SubeR, "UserName");
                                                items.ProductName = f.RTS(SubeR, "ProductName");
                                                items.Quantity = f.RTD(SubeR, "Quantity");
                                                items.Price = f.RTD(SubeR, "Price");
                                                items.DateTime = f.RTS(SubeR, "Date");
                                                Liste.Add(items);
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    IadeViewModel items = new IadeViewModel();
                                    items.Sube = SubeAdi + " (Data Yok) ";
                                    items.SubeID = (SubeId);

                                    Liste.Add(items);
                                }
                            }
                            catch (Exception) { throw new Exception(SubeAdi); }

                        }
                        catch (System.Exception ex)
                        {
                        }
                        #endregion GET DATA
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
                                        DataTable AcikHesapDt = new DataTable();
                                        AcikHesapDt = f.GetSubeDataWithQuery((f.NewConnectionString(SubeIP, DBName, SqlName, SqlPassword)), Query.ToString());

                                        if (AcikHesapDt.Rows.Count > 0)
                                        {
                                            if (subeid.Equals(""))
                                            {
                                                if (AppDbType == "3")
                                                {
                                                    foreach (DataRow sube in AcikHesapDt.Rows)
                                                    {
                                                        IadeViewModel items = new IadeViewModel();
                                                        items.Sube = SubeAdi + "-" + sube["Sube1"].ToString() + "-" + sube["Kasa"].ToString();
                                                        items.SubeID = SubeId + "~" + (sube["Id"].ToString());
                                                        ////items.PersonelAdi = "Kasa Satış";
                                                        ////items.Total += f.RTD(sube, "Total");
                                                        //}
                                                        items.SumQuantity += 0;//f.RTD(sube, "Miktar");
                                                        items.SumTotal += f.RTD(sube, "Tutar");
                                                        Liste.Add(items);
                                                    }
                                                }
                                                else
                                                {
                                                    IadeViewModel items = new IadeViewModel();
                                                    items.Sube = SubeAdi; //f.RTS(SubeUrunCiroDt.Rows[0], "Sube");
                                                    items.SubeID = SubeId;
                                                    foreach (DataRow SubeR in AcikHesapDt.Rows)
                                                    {
                                                        items.SumQuantity += f.RTD(SubeR, "Miktar");
                                                        items.SumTotal += f.RTD(SubeR, "Tutar");
                                                    }
                                                    Liste.Add(items);
                                                }
                                            }
                                            else if (!subeid.Equals("") && productGroup == "0") /*DETAIL*/
                                            {
                                                if (AppDbType == "3")
                                                {
                                                    foreach (DataRow SubeR in AcikHesapDt.Rows)
                                                    {
                                                        if (subeid == SubeR["Id"].ToString())
                                                        {
                                                            IadeViewModel items = new IadeViewModel();
                                                            items.Sube = SubeAdi;
                                                            items.SubeID = (SubeId);
                                                            items.ProductName = f.RTS(SubeR, "ProductName");
                                                            items.SumQuantity = f.RTD(SubeR, "MIKTAR");
                                                            items.SumTotal = f.RTD(SubeR, "TUTAR");
                                                            Liste.Add(items);
                                                        }
                                                    }
                                                }
                                                else if (!subeid.Equals("") && productGroup != "null")
                                                {
                                                    #region 2.KIRILIM SEFIM ESKI/YENI 

                                                    foreach (DataRow SubeR in AcikHesapDt.Rows)
                                                    {
                                                        IadeViewModel items = new IadeViewModel();
                                                        items.Sube = SubeAdi;
                                                        items.SubeID = (SubeId);
                                                        items.ProductName = f.RTS(SubeR, "ProductName");
                                                        if (!subeid.Equals("0"))
                                                        {
                                                            items.SumQuantity = f.RTD(SubeR, "Miktar");
                                                            items.SumTotal = f.RTD(SubeR, "Tutar");
                                                        }
                                                        Liste.Add(items);
                                                    }

                                                    #endregion 2.KIRILIM SEFIM ESKI/YENI 
                                                }
                                            }
                                            else
                                            {
                                                if (AppDbType == "3")
                                                {
                                                    foreach (DataRow SubeR in AcikHesapDt.Rows)
                                                    {
                                                        if (subeid == SubeR["Id"].ToString())
                                                        {
                                                            IadeViewModel items = new IadeViewModel();
                                                            items.Sube = SubeAdi;
                                                            items.SubeID = (SubeId);
                                                            items.UserName = f.RTS(SubeR, "UserName");
                                                            items.ProductName = f.RTS(SubeR, "ProductName");
                                                            items.Quantity = f.RTD(SubeR, "Quantity");
                                                            items.Price = f.RTD(SubeR, "Price");
                                                            items.DateTime = f.RTS(SubeR, "Date");
                                                            Liste.Add(items);
                                                        }
                                                    }
                                                }
                                                else
                                                {
                                                    foreach (DataRow SubeR in AcikHesapDt.Rows)
                                                    {
                                                        IadeViewModel items = new IadeViewModel();
                                                        items.Sube = SubeAdi;
                                                        items.SubeID = (SubeId);
                                                        items.UserName = f.RTS(SubeR, "UserName");
                                                        items.ProductName = f.RTS(SubeR, "ProductName");
                                                        items.Quantity = f.RTD(SubeR, "Quantity");
                                                        items.Price = f.RTD(SubeR, "Price");
                                                        items.DateTime = f.RTS(SubeR, "Date");
                                                        Liste.Add(items);
                                                    }
                                                }
                                            }
                                        }
                                        else
                                        {
                                            IadeViewModel items = new IadeViewModel();
                                            items.Sube = SubeAdi + " (Data Yok) ";
                                            items.SubeID = (SubeId);

                                            Liste.Add(items);
                                        }
                                    }
                                    catch (Exception) { throw new Exception(SubeAdi); }

                                }
                                catch (System.Exception ex)
                                {
                                }
                                #endregion GET DATA
                            }
                        }
                        #endregion
                    }
                });
                #endregion PARALLEL FOREACH
            }
            catch (DataException ex) { }

            return Liste;
        }

        public static PersonelCiro Print()
        {
            return null;
        }
    }
}