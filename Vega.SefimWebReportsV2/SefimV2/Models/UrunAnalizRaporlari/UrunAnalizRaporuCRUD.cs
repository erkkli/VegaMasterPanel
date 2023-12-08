using SefimV2.ViewModels.UrunAnalizRaporu;
using SefimV2.ViewModels.User;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Hosting;

namespace SefimV2.Models
{
    public class UrunAnalizRaporuCRUD
    {
        public static List<UrunAnalizRaporuViewModel> List(DateTime Date1, DateTime Date2, string subeid, string ID, string productGroup)
        {
            List<UrunAnalizRaporuViewModel> Liste = new List<UrunAnalizRaporuViewModel>();
            ModelFunctions ff = new ModelFunctions();
            DateTime startDate = DateTime.Now;
            #region GİRİŞ YAPAN KULLANICININ YETKİLİ OLDUĞU ŞUBELER LİSTELENİYOR
            UserViewModel model = UsersListCRUD.YetkiliSubesi(ID);
            #endregion

            string subeid_ = "";

            try
            {
                UTF8Encoding utf8 = new UTF8Encoding();
                #region Kırılımlarda ilk once db de tanımlı subeyı alıyoruz.   
                if (!subeid.Equals(""))
                {
                    string[] tableNo = subeid.Split('~');
                    subeid_ = tableNo[0];
                }
                #endregion   Kırılımlarda ilk once db de tanımlı subeyı alıyoruz.  

                #region SUBSTATION LIST               
                ff.SqlConnOpen();
                string filter = "Where Status=1";
                if (subeid_ != null && !subeid_.Equals("0") && !subeid_.Equals(""))
                    filter += " and Id=" + subeid_;
                DataTable dt = ff.DataTable("select * from SubeSettings " + filter);
                ff.SqlConnClose();
                #endregion SUBSTATION LIST


                #region SUBSTATION LIST                
                ff.SqlConnOpen();
                DataTable dtVegaDb = ff.DataTable("SELECT Id,DBName,IP,SqlName,SqlPassword  FROM  VegaDbSettings");
                ff.SqlConnClose();
                string VegaDbName = "";
                string VegaDbSqlName;
                foreach (DataRow r in dtVegaDb.Rows)
                {
                    VegaDbName = ff.RTS(r, "DBName");
                    VegaDbSqlName = ff.RTS(r, "SqlName");
                }

                #endregion SUBSTATION LIST


                #region PARALLEL FORECH
                var dtList = dt.AsEnumerable().ToList<DataRow>();
                Parallel.ForEach(dtList, r =>
                {
                    ModelFunctions f = new ModelFunctions();
                    //foreach (DataRow r in dt.Rows)
                    //{
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
                    string AppDbType = f.RTS(r, "AppDbType");
                    string AppDbTypeStatus = f.RTS(r, "AppDbTypeStatus");
                    string DepoEnvanter = "[" + VegaDbName + "].[dbo]." + "F0" + r["FirmaID"].ToString() + "D0001TBLDEPOENVANTER";
                    string StokHareket = "[" + VegaDbName + "].[dbo]." + "F0" + r["FirmaID"].ToString() + "D0001TBLSTOKHAREKETLERI";
                    string Stoklar = "[" + VegaDbName + "].[dbo]." + "F0" + r["FirmaID"].ToString() + "TBLSTOKLAR";
                    string Query = "";

                    if (AppDbType == "1" || AppDbType == "2")
                    {
                        if (subeid != null && !subeid.Equals("0") && productGroup == null)// sube secili degilse ilk giris yapilan sql
                            Query = File.ReadAllText(HostingEnvironment.MapPath("/Sql/SqlUrunAnalizRaporu/UrunAnalizRaporu.sql"), System.Text.UTF8Encoding.Default);
                        if (subeid == null || subeid.Equals("0"))
                            Query = File.ReadAllText(HostingEnvironment.MapPath("/Sql/UrunGrup.sql"), System.Text.UTF8Encoding.Default);
                        if (subeid != null && !subeid.Equals("0") && productGroup != null)
                            Query =
                                             "  declare @par1 nvarchar(20) = '{TARIH1}';" +
                                             "  declare @par2 nvarchar(20) = '{TARIH2}';" +
                                             "  declare @par3 nvarchar(20) = '{productGroup}';" +
                                 " ; WITH " +
                                 " base as (SELECT ProductName, PaymentTime, Quantity, LTRIM(RTRIM(split.s)) AS Opt, ProductId FROM " +
                                 " PaidBill " +
                                 " CROSS APPLY " +
                                 " SplitString(',', PaidBill.Options) as split " +
                                 " WHERE ISNULL(Options, '') <> '' AND PaymentTime>= @par1 AND PaymentTime <= @par2    )," +
                                 " base_with_optqty as ( " +
                                 " SELECT  base.ProductName,base.PaymentTime,	base.Quantity,	ProductId,CASE WHEN CHARINDEX('x', base.Opt) > 0 THEN " +
                                 " SUBSTRING(base.Opt, 1, CHARINDEX('x', base.Opt) - 1) ELSE '1' END AS OptQty, CASE" +
                                 "  WHEN CHARINDEX('x', base.Opt) > 0 THEN SUBSTRING(base.Opt,CHARINDEX('x', base.Opt) + 1,100)" +
                                 " ELSE base.Opt END AS Opt FROM base)," +
                                 " satislar as (SELECT Quantity * (CASE WHEN ISNUMERIC(OptQty) = 1 THEN CAST(OptQty AS INT) ELSE 1 END ) AS Quantity, Opt as ProductName " +
                                 " FROM base_with_optqty bwo  )," +
                                 " envanter AS(" +
                                 " SELECT STOKNO, ENVANTER DBS, TARIH FROM " + DepoEnvanter + " WHERE TARIH<@par1 " +
                                 " )," +
                                 " hareketler as (SELECT" +
                                 " STOKNO, IZAHAT, GIREN, BIRIMMALIYET, BIRIMFIYAT, TUTAR, TARIH " +

                                     " FROM " + StokHareket + " WHERE ISNULL(GIREN,0)>0 " +
                                     " AND TARIH>=@par1 AND TARIH<=@par2 " +
                                     " )," +
                                     " stoklar as (" +
                                 " SELECT IND, STOKKODU, MALINCINSI, MALIYET FROM " + Stoklar + " " +
                                 " ) " +
                                 " select" +
                                 " SUM(DONBASISTOK) AS DONBASISTOK," +
                                 " SUM(DONICIGIRENLER) AS DONICIGIRENLER," +
                                 " SUM(TOPENV) AS TOPENV," +
                                 " SUM(ORTMALIYET) AS ORTMALIYET," +
                                 " SUM(DONICIGIRTUTAR) AS DONICIGIRTUTAR," +
                                 " SUM(TOPENVTUTAR) AS TOPENVTUTAR," +
                                 " SUM(Miktar) SATISMIKTAR ," +
                                 " SUM(Tutar) AS SATISTUTAR," +
                                 " SUM(ORTMALIYET) * SUM(Miktar) AS DONICISATMALIYET," +
                                 " SUM(Tutar) -(SUM(ORTMALIYET) * SUM(Miktar)) AS DONICIBRUTKAR" +
                                 " from(" +
                                 " select" +
                                 " (SELECT sum(DBS) FROM envanter where STOKNO = S.IND) DONBASISTOK," +
                                 " SUM(ISNULL(GIREN, 0)) DONICIGIRENLER," +
                                 " (SELECT sum(DBS) FROM envanter where STOKNO = S.IND) + SUM(ISNULL(GIREN, 0)) AS TOPENV," +
                                  "  CASE WHEN(CASE WHEN SUM(ISNULL(TUTAR, 0)) = 0 THEN 1 ELSE SUM(ISNULL(TUTAR, 0)) / SUM(ISNULL(GIREN, 0)) END) = 1  THEN AVG(ISNULL(S.MALIYET,0)) ELSE" +
                                   "     (CASE WHEN SUM(ISNULL(TUTAR, 0)) = 0 THEN 1 ELSE SUM(ISNULL(TUTAR, 0)) / SUM(ISNULL(GIREN, 0)) END) END AS ORTMALIYET," +
                                 " SUM(ISNULL(TUTAR, 0)) DONICIGIRTUTAR," +
                                 " (SELECT sum(DBS) FROM envanter where STOKNO = S.IND) +SUM(ISNULL(GIREN, 0)) *" +
                                 "  (CASE WHEN(CASE WHEN SUM(ISNULL(TUTAR, 0)) = 0 THEN 1 ELSE SUM(ISNULL(TUTAR, 0)) / SUM(ISNULL(GIREN, 0)) END) = 1  THEN AVG(ISNULL(S.MALIYET,0)) ELSE" +
                                 "     (CASE WHEN SUM(ISNULL(TUTAR, 0)) = 0 THEN 1 ELSE SUM(ISNULL(TUTAR, 0)) / SUM(ISNULL(GIREN, 0)) END) END ) TOPENVTUTAR," +
                                 " 0 as Miktar, " +
                                 " 0 as Tutar" +
                                 " from" +
                                 " stoklar as s" +
                                 " LEFT join hareketler as h ON s.IND = h.STOKNO" +
                                 " WHERE" +
                                 " MALINCINSI = '" + productGroup + "' " +
                                 " GROUP BY" +
                                 " S.IND" +
                                 " UNION ALL" +
                                 " SELECT" +
                                 " 0 DONBASISTOK," +
                                 " 0 DONICIGIRENLER," +
                                 " 0 TOPENV," +
                                 " 0 ORTMALIYET," +
                                 " 0 DONICIGIRTUTAR," +
                                 " 0 TOPENVTUTAR," +
                                 " a.Miktar," +
                                 " a.Tutar" +
                                 " from" +
                                 " (" +
                                 " SELECT ProductName + ' ' + '(***Seçenek Menü İçeriği***)' as ProductName," +
                                 " sum(Quantity) as Miktar," +
                                 " 0 as Tutar" +
                                 " FROM  satislar where ProductName = '" + productGroup + "' " +
                                 " GROUP BY satislar.ProductName" +
                                 " UNION SELECT" +
                                 " t.ProductName," +
                                 " sum(t.toplam) as Miktar," +
                                 " sum(t.tutar) as Tutar" +
                                 " FROM" +
                                 " (SELECT" +
                                 " P.Id as ProductId," +
                                 " SUM(B.Quantity) AS Toplam," +
                                 " B.ProductName," +
                                 " SUM(ISNULL(B.Quantity, 0) * ISNULL(B.Price, 0)) AS Tutar" +
                                 " FROM Bill AS B" +
                                 " LEFT JOIN Product as P ON P.Id = B.ProductId" +
                                 " WHERE [Date] >= @par1 AND[Date] <= @par2" +
                                 " GROUP BY" +
                                 " B.ProductName," +
                                 " B.HeaderId," +
                                 " p.Id" +
                                 " ) as t" +
                                 " where ProductName = '" + productGroup + "'   " +
                                 " GROUP BY" +
                                  "   t.ProductName," +
                                   "  t.ProductId" +
                                  "   ) as a" +
                                 "	) as X"
                                    ;

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
                        #endregion  Kırılımlarda Ana Şube Altındaki IND id alıyorum 

                        if (AppDbTypeStatus == "True")
                        {
                            if (subeid == null && subeid.Equals("0") || subeid == "")// sube secili degilse ilk giris yapilan sql

                                #region FASTER ONLINE QUARY
                                Query =
                                        " declare @Sube nvarchar(100) = '{SUBEADI}';" +
                                        " declare @par1 nvarchar(20) = '{TARIH1}';" +
                                        " declare @par2 nvarchar(20) = '{TARIH2}';" +
                                        " (SELECT (SELECT SUBEADI FROM  " + FirmaId_SUBE + " WHERE IND=FSB.SUBEIND) AS Sube1," +
                                        " (SELECT KASAADI FROM  " + FirmaId_KASA + " WHERE IND=FSB.KASAIND) AS Kasa," +
                                        " (SELECT IND FROM  F0" + FirmaId + "TBLKRDKASALAR WHERE IND=FSB.KASAIND) AS Id," +
                                        " SUM(FSH.MIKTAR) AS MIKTAR, SUM((((MIKTAR*SATISFIYATI) * (100-ISNULL(FSH.ISK1,0))/100) * (100-ISNULL(FSH.ISK2,0))/100) * (100-ISNULL(FSB.ALTISKORAN,0))/100)  AS TUTAR " +
                                        " FROM TBLFASTERSATISHAREKET AS FSH " +
                                        " LEFT JOIN TBLFASTERSATISBASLIK AS FSB ON FSH.BASLIKIND=FSB.BASLIKIND AND FSH.SUBEIND=FSB.SUBEIND AND FSH.KASAIND=FSB.KASAIND " +
                                        " LEFT JOIN F0" + FirmaId + "TBLSTOKLAR AS STK ON FSH.STOKIND=STK.IND WHERE FSH.ISLEMTARIHI>=@par1 AND FSH.ISLEMTARIHI<=@par2 AND ISNULL(FSB.IADE,0)=0 " +
                                        " GROUP BY FSB.SUBEIND,FSB.KASAIND)";
                            #endregion FASTER ONLINE QUARY

                            if (subeid != null && !subeid.Equals("0") && subeid != "")
                                #region FASTER ONLINE QUARY
                                Query =
                                        " declare @Sube nvarchar(100) = '{SUBEADI}';" +
                                        " declare @par1 nvarchar(20) = '{TARIH1}';" +
                                        " declare @par2 nvarchar(20) = '{TARIH2}';" +
                                        " (SELECT (SELECT SUBEADI FROM  " + FirmaId_SUBE + " WHERE IND=FSB.SUBEIND) AS Sube1," +
                                        " (SELECT KASAADI FROM  " + FirmaId_KASA + " WHERE IND=FSB.KASAIND) AS Kasa," +
                                        " (SELECT IND FROM  F0" + FirmaId + "TBLKRDKASALAR WHERE IND=FSB.KASAIND) AS Id," +
                                        " SUM(FSH.MIKTAR) AS MIKTAR, SUM((((MIKTAR*SATISFIYATI) * (100-ISNULL(FSH.ISK1,0))/100) * (100-ISNULL(FSH.ISK2,0))/100) * (100-ISNULL(FSB.ALTISKORAN,0))/100)  AS TUTAR,  STK.MALINCINSI AS ProductName   " +
                                        " FROM TBLFASTERSATISHAREKET AS FSH " +
                                        " LEFT JOIN TBLFASTERSATISBASLIK AS FSB ON FSH.BASLIKIND=FSB.BASLIKIND AND FSH.SUBEIND=FSB.SUBEIND AND FSH.KASAIND=FSB.KASAIND " +
                                        " LEFT JOIN F0" + FirmaId + "TBLSTOKLAR AS STK ON FSH.STOKIND=STK.IND WHERE FSH.ISLEMTARIHI>=@par1 AND FSH.ISLEMTARIHI<=@par2 AND ISNULL(FSB.IADE,0)=0 " +
                                        " GROUP BY FSB.SUBEIND,FSB.KASAIND,STK.MALINCINSI)";
                            #endregion FASTER ONLINE QUARY
                        }
                        else
                        {
                            Query = File.ReadAllText(HostingEnvironment.MapPath("/Sql/SqlFaster/SubeUrun/SubeUrunFasterOFLINE1.sql"), System.Text.UTF8Encoding.Default);
                        }
                    }
                    //}
                    //else
                    //{

                    //    if (AppDbType == "1" || AppDbType == "2")
                    //    {
                    //        #region ŞUBEDE EN COK SATILAN URUNU ALMAK ICIN
                    //        Query = File.ReadAllText(HostingEnvironment.MapPath("/Sql/EncokSatisYapanUrun.sql"), System.Text.Encoding.UTF8);
                    //        #endregion
                    //    }
                    //    else if (AppDbType == "3")
                    //    {
                    //        if (AppDbTypeStatus == "True")
                    //        {
                    //            #region FASTER ONLINE QUARY
                    //            Query =
                    //                 " declare @Sube nvarchar(100) = '{SUBEADI}';" +
                    //                 " declare @par1 nvarchar(20) = '{TARIH1}';" +
                    //                 " declare @par2 nvarchar(20) = '{TARIH2}';" +
                    //                 " (SELECT (SELECT SUBEADI FROM  " + FirmaId_SUBE + " WHERE IND=FSB.SUBEIND) AS Sube1," +
                    //                 " (SELECT KASAADI FROM  " + FirmaId_KASA + " WHERE IND=FSB.KASAIND) AS Kasa," +
                    //                 " (SELECT IND FROM  F0" + FirmaId + "TBLKRDKASALAR WHERE IND=FSB.KASAIND) AS Id," +
                    //                 " SUM(FSH.MIKTAR) AS MIKTAR, SUM((((MIKTAR*SATISFIYATI) * (100-ISNULL(FSH.ISK1,0))/100) * (100-ISNULL(FSH.ISK2,0))/100) * (100-ISNULL(FSB.ALTISKORAN,0))/100)  AS TUTAR, STK.MALINCINSI AS ProductName  " +
                    //                 " FROM TBLFASTERSATISHAREKET AS FSH " +
                    //                 " LEFT JOIN TBLFASTERSATISBASLIK AS FSB ON FSH.BASLIKIND=FSB.BASLIKIND AND FSH.SUBEIND=FSB.SUBEIND AND FSH.KASAIND=FSB.KASAIND " +
                    //                 " LEFT JOIN F0" + FirmaId + "TBLSTOKLAR AS STK ON FSH.STOKIND=STK.IND WHERE FSH.ISLEMTARIHI>=@par1 AND FSH.ISLEMTARIHI<=@par2 AND ISNULL(FSB.IADE,0)=0 " +
                    //                 " GROUP BY FSB.SUBEIND,FSB.KASAIND,STK.MALINCINSI)";
                    //            #endregion FASTER ONLINE QUARY
                    //        }
                    //        else
                    //        {
                    //            Query = File.ReadAllText(HostingEnvironment.MapPath("/Sql/SqlFaster/SubeUrun/SubeUrunFasterOFLINE.sql"), System.Text.UTF8Encoding.Default);
                    //        }
                    //    }

                    Query = Query.Replace("{SUBEADI}", SubeAdi);
                    Query = Query.Replace("{TARIH1}", QueryTimeStart);
                    Query = Query.Replace("{TARIH2}", QueryTimeEnd);
                    var productGroupEncoding = "";
                    if (productGroup != null)
                    {
                        byte[] bytes = UTF8Encoding.Default.GetBytes(productGroup);
                        productGroupEncoding = UTF8Encoding.Default.GetString(bytes);
                    }

                    Query = Query.Replace("{productGroup}", productGroupEncoding);

                    if (ID == "1")
                    {
                        #region GET DATA                  
                        try
                        {
                            string connString = "Provider=SQLOLEDB;Server=" + SubeIP + ";User Id=" + SqlName + ";Password=" + SqlPassword + ";Database=" + DBName + "";
                            try
                            {
                                DataTable UrunAnalizRaporuDt = new DataTable();
                                UrunAnalizRaporuDt = f.GetSubeDataWithQuery((f.NewConnectionString(SubeIP, DBName, SqlName, SqlPassword)), Query.ToString());

                                if (UrunAnalizRaporuDt.Rows.Count > 0)
                                {
                                    if (subeid.Equals(""))
                                    {
                                        if (AppDbType == "3")
                                        {
                                            #region FASTER -(AppDbType = 3 faster kullanan şube)                                           
                                            foreach (DataRow sube in UrunAnalizRaporuDt.Rows)
                                            {
                                                UrunAnalizRaporuViewModel items = new UrunAnalizRaporuViewModel();
                                                items.SubeID = SubeId + "~" + sube["Id"].ToString();
                                                items.Sube = SubeAdi + "-" + sube["Sube1"].ToString() + "-" + sube["Kasa"].ToString();
                                                items.Miktar += Convert.ToDecimal(sube["MIKTAR"]);//, "MIKTAR");
                                                items.Tutar += Convert.ToDecimal(sube["TUTAR"]); //, "MIKTAR");f.RTD(SubeR, "TUTAR");
                                                items.ProductName = f.RTS(sube, "ProductName"); //sube["ProductName"].ToString();  //f.RTS(SubeR, "ProductName");
                                                Liste.Add(items);
                                            }
                                            #endregion FASTER -(AppDbType = 3 faster kullanan şube)
                                        }
                                        else
                                        {
                                            UrunAnalizRaporuViewModel items = new UrunAnalizRaporuViewModel();
                                            items.Sube = SubeAdi; //f.RTS(SubeUrunCiroDt.Rows[0], "Sube");
                                            items.SubeID = (SubeId);
                                            if (subeid.Equals(""))
                                            {
                                                foreach (DataRow SubeR in UrunAnalizRaporuDt.Rows)
                                                {
                                                    items.Miktar += f.RTD(SubeR, "Miktar");
                                                    items.Tutar += f.RTD(SubeR, "Tutar");
                                                    items.ProductName = f.RTS(SubeR, "ProductName");
                                                }
                                            }
                                            Liste.Add(items);
                                        }
                                    }
                                    else if (!subeid.Equals("") && productGroup == null)
                                    {

                                        if (AppDbType == "3")
                                        {
                                            foreach (DataRow SubeR in UrunAnalizRaporuDt.Rows)
                                            {
                                                if (subeid == SubeR["Id"].ToString())
                                                {
                                                    UrunAnalizRaporuViewModel items = new UrunAnalizRaporuViewModel();
                                                    items.Sube = f.RTS(SubeR, "Sube");
                                                    items.SubeID = (SubeId);
                                                    items.Miktar = f.RTD(SubeR, "MIKTAR");
                                                    items.ProductName = f.RTS(SubeR, "ProductName");
                                                    //items.Debit = f.RTD(SubeR, "TUTAR");
                                                    //items.GecenZaman = Convert.ToInt32(sure.TotalSeconds);
                                                    Liste.Add(items);
                                                }
                                            }
                                        }
                                        else
                                        {
                                            foreach (DataRow SubeR in UrunAnalizRaporuDt.Rows)
                                            {
                                                UrunAnalizRaporuViewModel items = new UrunAnalizRaporuViewModel();
                                                items.Sube = f.RTS(SubeR, "Sube");
                                                items.SubeID = (SubeId);
                                                items.ProductName = f.RTS(SubeR, "ProductName");
                                                items.Miktar = f.RTD(SubeR, "Miktar");
                                                items.Tutar = f.RTD(SubeR, "Tutar");
                                                items.BirimFiyat = f.RTD(SubeR, "BirimFiyat");
                                                items.ReceteMaliyet = f.RTD(SubeR, "ReceteMaliyet");
                                                items.CostMaliyet = f.RTD(SubeR, "CostMaliyet");
                                                items.ReceteKar = f.RTD(SubeR, "ReceteKar");
                                                items.CostKar = f.RTD(SubeR, "CostKar");
                                                Liste.Add(items);
                                            }
                                        }
                                    }
                                    else if (!subeid.Equals("") && productGroup != null)
                                    {
                                        foreach (DataRow SubeR in UrunAnalizRaporuDt.Rows)
                                        {
                                            UrunAnalizRaporuViewModel items = new UrunAnalizRaporuViewModel();
                                            items.Sube = f.RTS(SubeR, "Sube");
                                            items.SubeID = (SubeId);
                                            items.DONBASISTOK = f.RTD(SubeR, "DONBASISTOK");
                                            items.DONICIGRENLER = f.RTD(SubeR, "DONICIGRENLER");
                                            items.TOPENV = f.RTD(SubeR, "TOPENV");
                                            items.ORTMALIYET = f.RTD(SubeR, "ORTMALIYET");
                                            items.DONICIGIRTUTAR = f.RTD(SubeR, "DONICIGIRTUTAR");
                                            items.TOPENVTUTAR = f.RTD(SubeR, "TOPENVTUTAR");
                                            items.SATISMIKTAR = f.RTD(SubeR, "SATISMIKTAR");
                                            items.SATISTUTAR = f.RTD(SubeR, "SATISTUTAR");
                                            items.DONICISATMALIYET = f.RTD(SubeR, "DONICISATMALIYET");
                                            items.DONICIBRUTKAR = f.RTD(SubeR, "DONICIBRUTKAR");
                                            Liste.Add(items);
                                        }
                                    }
                                }
                                else
                                {
                                    UrunAnalizRaporuViewModel items = new UrunAnalizRaporuViewModel();
                                    items.Sube = SubeAdi + " (Data Yok)";
                                    items.SubeID = (SubeId);
                                    Liste.Add(items);
                                }
                            }
                            catch (Exception) { throw new Exception(SubeAdi); }
                        }
                        catch (System.Exception ex)
                        {
                            try
                            {
                                #region EX                            
                                //log metnini oluştur
                                string ErrorMessage = "Şube/Ürün Raporu Alınamadı.";
                                UrunAnalizRaporuViewModel items = new UrunAnalizRaporuViewModel();
                                items.Sube = ex.Message + " (Erişim Yok)";
                                items.SubeID = (SubeId);
                                Liste.Add(items);
                                #endregion
                            }
                            catch (Exception)
                            { }
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

                            }
                        }
                        #endregion
                    }
                });
                #endregion PARALLEL FORECH

            }
            catch (DataException ex) { }

            return Liste;
        }

    }
}