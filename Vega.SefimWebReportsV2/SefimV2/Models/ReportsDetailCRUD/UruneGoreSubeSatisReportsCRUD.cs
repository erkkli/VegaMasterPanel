﻿using SefimV2.ViewModels.ReportsDetail;
using SefimV2.ViewModels.User;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Hosting;

namespace SefimV2.Models.ReportsDetailCRUD
{
    public class UruneGoreSubeSatisReportsCRUD
    {
        public static List<ReportsDetailViewModel> List(DateTime Date1, DateTime Date2, string subeid, string ID, string urunAdi)
        {
            string payReportID = "";
            //if (subeid == "0")
            //{
            //    subeid = "";
            //    payReportID = "1";
            //}
            List<ReportsDetailViewModel> Liste = new List<ReportsDetailViewModel>();
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
                #endregion   Kırılımlarda ilk once db de tanımlı subeyı alıyoruz.  

                #region SUBSTATION LIST               
                ff.SqlConnOpen();
                string filter = "Where Status=1";
                if (subeid_ != null && !subeid_.Equals("0") && !subeid_.Equals(""))
                    filter += " and Id=" + subeid_;
                DataTable dt = ff.DataTable("select * from SubeSettings " + filter);
                ff.SqlConnClose();
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
                    string Query = "";

                    if (payReportID.Equals(""))
                    {
                        if (AppDbType == "1" || AppDbType == "2")
                        {
                            Query = File.ReadAllText(HostingEnvironment.MapPath("/Sql/SqlReports/UruneGoreSubeSatisReports/UruneGoreSubeSatisReports.sql"), System.Text.UTF8Encoding.Default);
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
                                Query = File.ReadAllText(HostingEnvironment.MapPath("/Sql/SqlFaster/SubeUrun/SubeUrunFasterOFLINE.sql"), System.Text.UTF8Encoding.Default);
                            }
                        }
                    }
                    else
                    {
                        if (AppDbType == "1" || AppDbType == "2")
                        {
                            #region SEFIM- ŞUBEDE EN COK SATILAN URUNU ALMAK ICIN
                            Query = File.ReadAllText(HostingEnvironment.MapPath("/Sql/EncokSatisYapanUrun.sql"), System.Text.Encoding.UTF8);
                            #endregion
                        }
                        else if (AppDbType == "3")
                        {
                            #region FASTER                            
                            if (AppDbTypeStatus == "True")
                            {
                                #region FASTER ONLINE QUARY
                                Query =
                                     " declare @Sube nvarchar(100) = '{SUBEADI}';" +
                                     " declare @par1 nvarchar(20) = '{TARIH1}';" +
                                     " declare @par2 nvarchar(20) = '{TARIH2}';" +
                                     " (SELECT (SELECT SUBEADI FROM  " + FirmaId_SUBE + " WHERE IND=FSB.SUBEIND) AS Sube1," +
                                     " (SELECT KASAADI FROM  " + FirmaId_KASA + " WHERE IND=FSB.KASAIND) AS Kasa," +
                                     " (SELECT IND FROM  F0" + FirmaId + "TBLKRDKASALAR WHERE IND=FSB.KASAIND) AS Id," +
                                     " SUM(FSH.MIKTAR) AS MIKTAR, SUM((((MIKTAR*SATISFIYATI) * (100-ISNULL(FSH.ISK1,0))/100) * (100-ISNULL(FSH.ISK2,0))/100) * (100-ISNULL(FSB.ALTISKORAN,0))/100)  AS TUTAR, STK.MALINCINSI AS ProductName  " +
                                     " FROM TBLFASTERSATISHAREKET AS FSH " +
                                     " LEFT JOIN TBLFASTERSATISBASLIK AS FSB ON FSH.BASLIKIND=FSB.BASLIKIND AND FSH.SUBEIND=FSB.SUBEIND AND FSH.KASAIND=FSB.KASAIND " +
                                     " LEFT JOIN F0" + FirmaId + "TBLSTOKLAR AS STK ON FSH.STOKIND=STK.IND WHERE FSH.ISLEMTARIHI>=@par1 AND FSH.ISLEMTARIHI<=@par2 AND ISNULL(FSB.IADE,0)=0 " +
                                     " GROUP BY FSB.SUBEIND,FSB.KASAIND,STK.MALINCINSI)";
                                #endregion FASTER ONLINE QUARY
                            }
                            else
                            {
                                Query = File.ReadAllText(HostingEnvironment.MapPath("/Sql/SqlFaster/SubeUrun/SubeUrunFasterOFLINE.sql"), System.Text.UTF8Encoding.Default);
                            }
                            #endregion FASTER
                        }
                        else if (AppDbType == "4")
                        {
                            #region NPOS QUARY (En Çok Satılan Ürün)
                            Query =
                                 " declare @Sube nvarchar(100) = '{SUBEADI}';" +
                                 " declare @Trh1 nvarchar(20) = '{TARIH1}';" +
                                 " declare @Trh2 nvarchar(20) = '{TARIH2}';" +
                                 " declare @ProductName nvarchar(20) = '{ProductName}';" +
                                 " SELECT TOP 6 WITH TIES Kasa_no,  " +
                                 " Sube,Sube1,Kasa_No AS Kasa, KOD1 AS ProductGroup ,MALINCINSI as ProductName ,Sum(Tutar) as TUTAR,Sum(Miktar) as MIKTAR " +
                                 " FROM TBLMASTERENPOSSTOK AS HR " +
                                 " WHERE HR.BELGETARIH >= @Trh1 and HR.BELGETARIH <= @Trh2 " +
                                 " group by " +
                                 " MALINCINSI," +
                                 " Sube,Sube1,Kasa_No,kod1  order by TUTAR desc ";
                            #endregion NPOS QUARY (En Çok Satılan Ürün)
                        }
                    }

                    Query = Query.Replace("{SUBEADI}", SubeAdi);
                    Query = Query.Replace("{TARIH1}", QueryTimeStart);
                    Query = Query.Replace("{TARIH2}", QueryTimeEnd);
                    Query = Query.Replace("{ProductName}", "%" + urunAdi + "%");

                    if (ID == "1")
                    {
                        #region GET DATA                  
                        try
                        {
                            string connString = "Provider=SQLOLEDB;Server=" + SubeIP + ";User Id=" + SqlName + ";Password=" + SqlPassword + ";Database=" + DBName + "";
                            try
                            {
                                DataTable SubeUrunCiroDt = new DataTable();
                                SubeUrunCiroDt = f.GetSubeDataWithQuery((f.NewConnectionString(SubeIP, DBName, SqlName, SqlPassword)), Query.ToString());

                                if (SubeUrunCiroDt.Rows.Count > 0)
                                {
                                    if (subeid.Equals(""))
                                    {
                                        if (AppDbType == "3")
                                        {

                                            #region FASTER -(AppDbType = 3 faster kullanan şube)                                           
                                            foreach (DataRow sube in SubeUrunCiroDt.Rows)
                                            {
                                                ReportsDetailViewModel items = new ReportsDetailViewModel();
                                                items.SubeID = SubeId + "~" + sube["Id"].ToString();
                                                items.Sube = SubeAdi + "-" + sube["Sube1"].ToString() + "-" + sube["Kasa"].ToString();
                                                items.Miktar += Convert.ToDecimal(sube["MIKTAR"]);//, "MIKTAR");
                                                items.Debit += Convert.ToDecimal(sube["TUTAR"]); //, "MIKTAR");f.RTD(SubeR, "TUTAR");
                                                items.ProductName = f.RTS(sube, "ProductName"); //sube["ProductName"].ToString();  //f.RTS(SubeR, "ProductName");
                                                Liste.Add(items);
                                            }
                                            #endregion FASTER -(AppDbType = 3 faster kullanan şube)
                                        }
                                        else if (AppDbType == "4")
                                        {
                                            #region NPOS -(AppDbType=4 NPOS şube)                                           
                                            foreach (DataRow sube in SubeUrunCiroDt.Rows)
                                            {
                                                ReportsDetailViewModel items = new ReportsDetailViewModel();
                                                items.SubeID = SubeId + "~" + sube["Kasa"].ToString();
                                                items.Sube = SubeAdi + "_" + sube["Sube"].ToString() + "-" + sube["Sube1"].ToString() + "-" + sube["Kasa"].ToString();
                                                items.Miktar += Convert.ToDecimal(sube["MIKTAR"]);//, "MIKTAR");
                                                items.Debit += Convert.ToDecimal(sube["TUTAR"]); //, "MIKTAR");f.RTD(SubeR, "TUTAR");
                                                items.ProductName = f.RTS(sube, "ProductName"); //sube["ProductName"].ToString();  //f.RTS(SubeR, "ProductName");
                                                Liste.Add(items);
                                            }
                                            #endregion  NPOS -(AppDbType=4 NPOS şube)    
                                        }
                                        else
                                        {
                                            #region SEFIM                                            
                                            ReportsDetailViewModel items = new ReportsDetailViewModel();
                                            items.Sube = SubeAdi; //f.RTS(SubeUrunCiroDt.Rows[0], "Sube");
                                            items.SubeID = (SubeId);
                                            if (subeid.Equals(""))
                                            {
                                                foreach (DataRow SubeR in SubeUrunCiroDt.Rows)
                                                {
                                                    items.Miktar = f.RTD(SubeR, "MIKTAR");
                                                    items.Debit = f.RTD(SubeR, "TUTAR");
                                                    items.ProductName = f.RTS(SubeR, "ProductName");
                                                    items.ProductGroup = f.RTS(SubeR, "ProductGroup");
                                                    Liste.Add(items);
                                                }
                                            }
                                            #endregion SEFIM
                                        }
                                    }
                                    else
                                    {
                                        if (AppDbType == "3")
                                        {
                                            foreach (DataRow SubeR in SubeUrunCiroDt.Rows)
                                            {
                                                if (subeid == SubeR["Id"].ToString())
                                                {
                                                    ReportsDetailViewModel items = new ReportsDetailViewModel();
                                                    items.Sube = f.RTS(SubeR, "Sube");
                                                    items.SubeID = (SubeId);
                                                    items.Miktar = f.RTD(SubeR, "MIKTAR");
                                                    items.ProductName = f.RTS(SubeR, "ProductName");
                                                    items.Debit = f.RTD(SubeR, "TUTAR");
                                                    //items.GecenZaman = Convert.ToInt32(sure.TotalSeconds);
                                                    Liste.Add(items);
                                                }
                                            }
                                        }
                                        else
                                        {
                                            foreach (DataRow SubeR in SubeUrunCiroDt.Rows)
                                            {
                                                ReportsDetailViewModel items = new ReportsDetailViewModel();
                                                //items.Sube = f.RTS(SubeR, "Sube");
                                                items.Sube = SubeAdi;
                                                items.SubeID = (SubeId);
                                                items.Miktar = f.RTD(SubeR, "MIKTAR");
                                                items.ProductName = f.RTS(SubeR, "ProductName");
                                                items.Debit = f.RTD(SubeR, "TUTAR");
                                                items.ProductGroup = f.RTS(SubeR, "ProductGroup");
                                                Liste.Add(items);
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    ReportsDetailViewModel items = new ReportsDetailViewModel();
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
                                ReportsDetailViewModel items = new ReportsDetailViewModel();
                                items.Sube = ex.Message + " (Erişim Yok)";
                                items.SubeID = (SubeId);
                                //items.ErrorMessage = ErrorMessage;
                                //items.ErrorStatus = true;
                                //items.ErrorCode = "01";
                                //Liste.Add(items);
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
                                #region GET DATA                  
                                try
                                {
                                    string connString = "Provider=SQLOLEDB;Server=" + SubeIP + ";User Id=" + SqlName + ";Password=" + SqlPassword + ";Database=" + DBName + "";
                                    try
                                    {
                                        DataTable SubeUrunCiroDt = new DataTable();
                                        SubeUrunCiroDt = f.GetSubeDataWithQuery((f.NewConnectionString(SubeIP, DBName, SqlName, SqlPassword)), Query.ToString());

                                        if (SubeUrunCiroDt.Rows.Count > 0)
                                        {
                                            if (subeid.Equals(""))
                                            {
                                                if (AppDbType == "3")
                                                {
                                                    #region FASTER -(AppDbType = 3 faster kullanan şube)                                           
                                                    foreach (DataRow sube in SubeUrunCiroDt.Rows)
                                                    {
                                                        ReportsDetailViewModel items = new ReportsDetailViewModel();
                                                        items.SubeID = SubeId + "~" + sube["Id"].ToString();
                                                        items.Sube = SubeAdi + "-" + sube["Sube1"].ToString() + "-" + sube["Kasa"].ToString();
                                                        items.Miktar += Convert.ToDecimal(sube["MIKTAR"]);//, "MIKTAR");
                                                        items.Debit += Convert.ToDecimal(sube["TUTAR"]); //, "MIKTAR");f.RTD(SubeR, "TUTAR");
                                                        items.ProductName = f.RTS(sube, "ProductName"); //sube["ProductName"].ToString();  //f.RTS(SubeR, "ProductName");
                                                        Liste.Add(items);
                                                    }
                                                    #endregion FASTER -(AppDbType = 3 faster kullanan şube)
                                                }
                                                else
                                                {
                                                    ReportsDetailViewModel items = new ReportsDetailViewModel();
                                                    items.Sube = SubeAdi; //f.RTS(SubeUrunCiroDt.Rows[0], "Sube");
                                                    items.SubeID = (SubeId);
                                                    if (subeid.Equals(""))
                                                    {
                                                        foreach (DataRow SubeR in SubeUrunCiroDt.Rows)
                                                        {
                                                            items.Miktar = f.RTD(SubeR, "MIKTAR");
                                                            items.Debit = f.RTD(SubeR, "TUTAR");
                                                            items.ProductName = f.RTS(SubeR, "ProductName");
                                                            items.ProductGroup = f.RTS(SubeR, "ProductGroup");
                                                            Liste.Add(items);
                                                        }
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                if (AppDbType == "3")
                                                {
                                                    foreach (DataRow SubeR in SubeUrunCiroDt.Rows)
                                                    {
                                                        if (subeid == SubeR["Id"].ToString())
                                                        {
                                                            ReportsDetailViewModel items = new ReportsDetailViewModel();
                                                            items.Sube = f.RTS(SubeR, "Sube");
                                                            items.SubeID = (SubeId);
                                                            items.Miktar = f.RTD(SubeR, "MIKTAR");
                                                            items.ProductName = f.RTS(SubeR, "ProductName");
                                                            items.Debit = f.RTD(SubeR, "TUTAR");
                                                            //items.GecenZaman = Convert.ToInt32(sure.TotalSeconds);
                                                            Liste.Add(items);
                                                        }
                                                    }
                                                }
                                                else
                                                {
                                                    foreach (DataRow SubeR in SubeUrunCiroDt.Rows)
                                                    {
                                                        ReportsDetailViewModel items = new ReportsDetailViewModel();
                                                        //items.Sube = f.RTS(SubeR, "Sube");
                                                        items.Sube = SubeAdi;
                                                        items.SubeID = (SubeId);
                                                        items.Miktar = f.RTD(SubeR, "MIKTAR");
                                                        items.ProductName = f.RTS(SubeR, "ProductName");
                                                        items.Debit = f.RTD(SubeR, "TUTAR");
                                                        items.ProductGroup = f.RTS(SubeR, "ProductGroup");
                                                        Liste.Add(items);
                                                    }
                                                }
                                            }
                                        }
                                        else
                                        {
                                            ReportsDetailViewModel items = new ReportsDetailViewModel();
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
                                        ReportsDetailViewModel items = new ReportsDetailViewModel();
                                        items.Sube = ex.Message + " (Erişim Yok)";
                                        items.SubeID = (SubeId);
                                        //items.ErrorMessage = ErrorMessage;
                                        //items.ErrorStatus = true;
                                        //items.ErrorCode = "01";
                                        //Liste.Add(items);
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
                                    catch (Exception)
                                    { }
                                }
                                #endregion
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