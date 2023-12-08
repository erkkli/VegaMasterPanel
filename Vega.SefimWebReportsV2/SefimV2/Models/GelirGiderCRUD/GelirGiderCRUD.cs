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
    public class GelirGiderCRUD
    {
        public static List<GelirGiderViewModels> List(DateTime Date1, DateTime Date2, string subeid, string ID)
        {
            List<GelirGiderViewModels> Liste = new List<GelirGiderViewModels>();
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
                    #region  SEFİM YENI - ESKİ FASTER SQL
                    string AppDbType = f.RTS(r, "AppDbType");
                    string AppDbTypeStatus = f.RTS(r, "AppDbTypeStatus");
                    string Query = "";

                    #region Faster Kasalar seçildiyse ona göre query oluşturuluyor. 
                    string FasterSubeIND = f.RTS(r, "FasterSubeID");
                    string QueryFasterSube = string.Empty;
                    if (FasterSubeIND != null)
                    {
                        QueryFasterSube = "  and  SUBEIND IN(" + FasterSubeIND + ") ";
                    }
                    #endregion Faster Kasalar seçildiyse ona göre query oluşturuluyor.

                    if (AppDbType == "1" || AppDbType == "2")// 1 = yeni şefim, 2 =eski Şefim, 3 = Faster
                    {
                        if (subeid == null && subeid.Equals("0") || subeid == "")// sube secili degilse ilk giris yapilan sql  
                            Query = File.ReadAllText(HostingEnvironment.MapPath("/Sql/SqlNewSef/GelirGiderRaporu/GelirGiderRaporu1.sql"), System.Text.UTF8Encoding.Default);

                        if (subeid != null && !subeid.Equals("0") && subeid != "")
                            Query = File.ReadAllText(HostingEnvironment.MapPath("/Sql/SqlNewSef/GelirGiderRaporu/GelirGiderRaporu2.sql"), System.Text.UTF8Encoding.Default);
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

                        if (AppDbTypeStatus == "True")
                        {
                            #region FASTER ONLINE QUARY
                            if (subeid == null && subeid.Equals("0") || subeid == "")
                                Query =
                                                            " DECLARE @FirmaInd nvarchar(100) = '{FIRMAIND}'; " +
                                                            " DECLARE @Sube nvarchar(100) = '{SUBE}'" +
                                                            " DECLARE @par1 nvarchar(20) = '{TARIH1}';" +
                                                            " DECLARE @par2 nvarchar(20) = '{TARIH2}';" +
                                                            " select" +
                                                            " (SELECT SUBEADI " +
                                                            "  FROM F0" + FirmaId + "TBLKRDSUBELER " +
                                                            "   WHERE IND = KI.SUBEIND) AS Sube1, " +
                                                            "  (SELECT KASAADI " +
                                                            "    FROM F0" + FirmaId + "TBLKRDKASALAR " +
                                                            "    WHERE IND = KI.KASAIND) AS Kasa, " +
                                                            "  (SELECT IND " +
                                                            "  FROM F0" + FirmaId + "TBLKRDKASALAR " +
                                                            "   WHERE IND = KI.KASAIND) AS Id," +
                                                            " SUM(ISNULL(GELIR, 0)) Gelir, SUM(ISNULL(GIDER, 0)) Gider from TBLFASTERKASAISLEMLERI KI " +
                                                            " LEFT JOIN DBO.TBLUSERS USR ON KI.USERNO = USR.IND " +
                                                            " where ISLEMTARIHI>= @par1 AND ISLEMTARIHI<= @par2 " +
                                                            " " + QueryFasterSube +
                                                            " GROUP BY " +
                                                            " KI.SUBEIND, " +
                                                            " KI.KASAIND ";

                            if (subeid != null && !subeid.Equals("0") && subeid != "")
                                Query =
                                                            "DECLARE @FirmaInd nvarchar(100) = '{FIRMAIND}';" +
                                                            "DECLARE @Sube nvarchar(100) = '{SUBE}' " +
                                                            "DECLARE @par1 nvarchar(20) = '{TARIH1}';" +
                                                            "DECLARE @par2 nvarchar(20) = '{TARIH2}';" +
                                                            " select " +
                                                            " (SELECT SUBEADI " +
                                                           "  FROM F0" + FirmaId + "TBLKRDSUBELER " +
                                                            "  WHERE IND = KI.SUBEIND) AS Sube1, " +
                                                            " (SELECT KASAADI " +
                                                            "  FROM F0" + FirmaId + "TBLKRDKASALAR " +
                                                            "  WHERE IND = KI.KASAIND) AS Kasa, " +
                                                            " (SELECT IND " +
                                                            "  FROM F0" + FirmaId + "TBLKRDKASALAR " +
                                                            "  WHERE IND = KI.KASAIND) AS Id, " +
                                                            " ISNULL(GELIR, 0) Gelir, ISNULL(GIDER, 0) Gider, " +
                                                            " USR.USERNAME , KI.TARIH AS Date,ISNULL(KI.ACIKLAMA,'') ACIKLAMA from TBLFASTERKASAISLEMLERI KI " +
                                                            " LEFT JOIN DBO.TBLUSERS USR ON KI.USERNO = USR.IND " +
                                                            " where ISLEMTARIHI>= @par1 AND ISLEMTARIHI<= @par2 " +
                                                            " " + QueryFasterSube;

                            #endregion FASTER ONLINE QUARY
                        }
                        else
                        {
                            //Query = File.ReadAllText(HostingEnvironment.MapPath("/Sql/SqlFaster/PersonelCiro/PersonelCiroFasterOFLINE1.sql"), System.Text.UTF8Encoding.Default);
                            if (subeid == null && subeid.Equals("0") || subeid == "")
                                Query = File.ReadAllText(HostingEnvironment.MapPath("/Sql/SqlFaster/GelirGider/GelirGiderOFFLINE1.sql"), System.Text.UTF8Encoding.Default);
                            if (subeid != null && !subeid.Equals("0") && subeid != "")
                                Query = File.ReadAllText(HostingEnvironment.MapPath("/Sql/SqlFaster/GelirGider/GelirGiderOFFLINE2.sql"), System.Text.UTF8Encoding.Default);
                        }
                    }
                    #endregion

                    Query = Query.Replace("{TARIH1}", QueryTimeStart);
                    Query = Query.Replace("{TARIH2}", QueryTimeEnd);
                    Query = Query.Replace("{SUBE}", SubeAdi);
                    Query = Query.Replace("{FIRMAIND}", FirmaId);

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
                                                GelirGiderViewModels items = new GelirGiderViewModels();
                                                items.Sube = SubeAdi + "-" + sube["Sube1"].ToString() + "-" + sube["Kasa"].ToString();
                                                items.SubeID = SubeId + "~" + (sube["Id"].ToString());
                                                items.ToplamGelir += f.RTD(sube, "Gelir");
                                                items.ToplamGider += f.RTD(sube, "Gider");
                                                Liste.Add(items);
                                            }
                                        }
                                        else
                                        {
                                            GelirGiderViewModels items = new GelirGiderViewModels();
                                            items.Sube = SubeAdi; //f.RTS(SubeUrunCiroDt.Rows[0], "Sube");
                                            items.SubeID = SubeId;
                                            //items.PersonelAdi = "Kasa Satış"; //f.RTS(SubeR, "PersonelAdi");
                                            foreach (DataRow SubeR in AcikHesapDt.Rows)
                                            {
                                                items.ToplamGelir += f.RTD(SubeR, "Gelir");
                                                items.ToplamGider += f.RTD(SubeR, "Gider");
                                            }
                                            Liste.Add(items);
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
                                                    GelirGiderViewModels items = new GelirGiderViewModels();
                                                    items.Sube = SubeAdi;
                                                    items.SubeID = (SubeId);
                                                    items.UserName = f.RTS(SubeR, "USERNAME");
                                                    items.Gelir = f.RTD(SubeR, "Gelir");
                                                    items.Gider = f.RTD(SubeR, "Gider");
                                                    items.DateTime = f.RTS(SubeR, "Date");
                                                    items.Description = f.RTS(SubeR, "ACIKLAMA"); //f.RTS(SubeR, "Description");
                                                    Liste.Add(items);
                                                }
                                            }
                                        }
                                        else
                                        {
                                            foreach (DataRow SubeR in AcikHesapDt.Rows)
                                            {
                                                GelirGiderViewModels items = new GelirGiderViewModels();
                                                items.Sube = SubeAdi;
                                                items.SubeID = (SubeId);
                                                items.UserName = f.RTS(SubeR, "UserName");
                                                items.IslemTipi = f.RTS(SubeR, "IslemTipi");
                                                items.Gelir = f.RTD(SubeR, "Gelir");
                                                items.Gider = f.RTD(SubeR, "Gider");
                                                items.DateTime = f.RTS(SubeR, "Date");
                                                items.Description = f.RTS(SubeR, "Description");
                                                Liste.Add(items);
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    GelirGiderViewModels items = new GelirGiderViewModels();
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
                                                        GelirGiderViewModels items = new GelirGiderViewModels();
                                                        items.Sube = SubeAdi + "-" + sube["Sube1"].ToString() + "-" + sube["Kasa"].ToString();
                                                        items.SubeID = SubeId + "~" + (sube["Id"].ToString());
                                                        items.ToplamGelir += f.RTD(sube, "Gelir");
                                                        items.ToplamGider += f.RTD(sube, "Gider");
                                                        Liste.Add(items);
                                                    }
                                                }
                                                else
                                                {
                                                    GelirGiderViewModels items = new GelirGiderViewModels();
                                                    items.Sube = SubeAdi; //f.RTS(SubeUrunCiroDt.Rows[0], "Sube");
                                                    items.SubeID = SubeId;
                                                    //items.PersonelAdi = "Kasa Satış"; //f.RTS(SubeR, "PersonelAdi");
                                                    foreach (DataRow SubeR in AcikHesapDt.Rows)
                                                    {
                                                        items.ToplamGelir += f.RTD(SubeR, "Gelir");
                                                        items.ToplamGider += f.RTD(SubeR, "Gider");
                                                    }
                                                    Liste.Add(items);
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
                                                            GelirGiderViewModels items = new GelirGiderViewModels();
                                                            items.Sube = SubeAdi;
                                                            items.SubeID = (SubeId);
                                                            items.UserName = f.RTS(SubeR, "USERNAME");
                                                            items.Gelir = f.RTD(SubeR, "Gelir");
                                                            items.Gider = f.RTD(SubeR, "Gider");
                                                            items.DateTime = f.RTS(SubeR, "Date");
                                                            items.Description = f.RTS(SubeR, "ACIKLAMA"); //f.RTS(SubeR, "Description");
                                                            Liste.Add(items);
                                                        }
                                                    }
                                                }
                                                else
                                                {
                                                    foreach (DataRow SubeR in AcikHesapDt.Rows)
                                                    {
                                                        GelirGiderViewModels items = new GelirGiderViewModels();
                                                        items.Sube = SubeAdi;
                                                        items.SubeID = (SubeId);
                                                        items.UserName = f.RTS(SubeR, "UserName");
                                                        items.IslemTipi = f.RTS(SubeR, "IslemTipi");
                                                        items.Gelir = f.RTD(SubeR, "Gelir");
                                                        items.Gider = f.RTD(SubeR, "Gider");
                                                        items.DateTime = f.RTS(SubeR, "Date");
                                                        items.Description = f.RTS(SubeR, "Description");
                                                        Liste.Add(items);
                                                    }
                                                }
                                            }
                                        }
                                        else
                                        {
                                            GelirGiderViewModels items = new GelirGiderViewModels();
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