using SefimV2.Helper;
using SefimV2.ViewModels.Icmal;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Configuration;
using System.Web.Hosting;
using static SefimV2.Enums.General;

namespace SefimV2.Models
{
    public class IcmalCRUD
    {
        ModelFunctions mF = new ModelFunctions();
        #region Config local copy db connction setting       
        static readonly string masterDbSubeIp = WebConfigurationManager.AppSettings["Server"];
        static readonly string masterDbName = WebConfigurationManager.AppSettings["DBName"];
        static readonly string masterSqlKullaniciName = WebConfigurationManager.AppSettings["User"];
        static readonly string masterSqlKullaniciPassword = WebConfigurationManager.AppSettings["Password"];
        #endregion

        public static List<IcmalViewModel> IcmalSubeList(string kullaniciId)
        {
            //GİRİŞ YAPAN KULLANICININ YETKİLİ OLDUĞU ŞUBELER LİSTELENİYOR
            //var model = UsersListCRUD.YetkiliSubesi(kullaniciId);
            var mfunc = new ModelFunctions();
            var icmalList = new List<IcmalViewModel>();

            try
            {
                //Kullanıcının yetkili olduğu şube
                var kullaniciSubeYetkisi = UsersListCRUD.KullaniciSubeYetkiListesi(kullaniciId);

                mfunc.SqlConnOpen();
                DataTable dt = mfunc.DataTable("select * from SubeSettings Where Status=1 ");
                mfunc.SqlConnClose();

                foreach (DataRow r in dt.Rows)
                {
                    string subeId = mfunc.RTS(r, "ID");
                    if (kullaniciSubeYetkisi != null && kullaniciSubeYetkisi.FR_SubeListesi.Where(x => x.SubeID == Convert.ToInt32(subeId)).Select(x => x.SubeID).Any())
                    {
                        string SubeAdi = mfunc.RTS(r, "SubeName");
                        string SubeIP = mfunc.RTS(r, "SubeIP");
                        string SqlName = mfunc.RTS(r, "SqlName");
                        string SqlPassword = mfunc.RTS(r, "SqlPassword");
                        string DBName = mfunc.RTS(r, "DBName");

                        var model = new IcmalViewModel
                        {
                            SubeId = mfunc.RTS(r, "ID"),
                            SubeName = mfunc.RTS(r, "SubeName"),
                        };
                        icmalList.Add(model);
                    }
                }
            }
            catch (DataException ex)
            {
                Singleton.WritingLogFile2("IcmalSubeList", ex.ToString(), null, ex.StackTrace);
            }

            return icmalList;
        }
        public static List<IcmalViewModel> IcmalList(DateTime Date1, DateTime Date2, string ID, string subeId)
        {
            //GİRİŞ YAPAN KULLANICININ YETKİLİ OLDUĞU ŞUBELER LİSTELENİYOR
            var model = UsersListCRUD.YetkiliSubesi(ID);
            var mfunc = new ModelFunctions();
            var icmalList = new List<IcmalViewModel>();

            try
            {
                #region SUBSTATION LIST                
                mfunc.SqlConnOpen();
                string filter = "Where Status=1";
                if (subeId != null && !subeId.Equals("0") && !subeId.Equals(""))
                    filter += " and Id=" + subeId;
                var dt = mfunc.DataTable("Select * From SubeSettings " + filter);
                var subeList = dt.AsEnumerable().ToList<DataRow>();
                mfunc.SqlConnClose();
                #endregion SUBSTATION LIST

                try
                {
                    #region PARALEL FORECH

                    var locked = new Object();

                    Parallel.ForEach(subeList, r =>
                    {
                        string SubeId = r["Id"].ToString();
                        string SubeAdi = r["SubeName"].ToString();
                        string SubeIP = r["SubeIP"].ToString();
                        string SqlName = r["SqlName"].ToString();
                        string SqlPassword = r["SqlPassword"].ToString();
                        string DBName = r["DBName"].ToString();
                        string FirmaId_SUBE = "F0" + r["FirmaID"].ToString() + "TBLKRDSUBELER";
                        string FirmaId_KASA = "F0" + r["FirmaID"].ToString() + "TBLKRDKASALAR";
                        string Firma_NPOS = r["FirmaID"].ToString();
                        string QueryTimeStart = Date1.ToString("yyyy/MM/dd HH:mm:ss");
                        string QueryTimeEnd = Date2.ToString("yyyy/MM/dd HH:mm:ss");

                        #region  SEFİM YENI - ESKİ FASTER SQL

                        string AppDbType = mfunc.RTS(r, "AppDbType");
                        string AppDbTypeStatus = mfunc.RTS(r, "AppDbTypeStatus");
                        string Query = string.Empty;

                        if (AppDbType == "1")// 1 = yeni şefim, 2 =eski Şefim, 3 = Faster
                        {
                            //Query = " Select v.IcmalPaymentId, v.SubeId,  v.PaymentDate, " +
                            //               "   Sum(v.CashAmount) as CashAmount ,Sum(v.KreditAmount) as KreditAmount, Sum(v.TicketAmount) as TicketAmount,Sum(v.OnlineAmount) as OnlineAmount, sum(v.DebitAmount) as DebitAmount " +
                            //               " From" +
                            //               "   (" +
                            //               "	 Select " +
                            //               "	 icp.Id as IcmalPaymentId, icp.SubeId as SubeId," +
                            //               "	 CONVERT(VARCHAR,icp.PaymentDate, 105) PaymentDate, Sum(icp.CashAmount) CashAmount, 0 as KreditAmount, 0 as TicketAmount, Sum(icp.OnlineAmount) as OnlineAmount, sum(icp.DebitAmount) as DebitAmount	 " +
                            //               "	 From IcmalPayment icp  Group By CONVERT(VARCHAR,icp.PaymentDate, 105),icp.Id,icp.SubeId" +
                            //               "	 Union all" +
                            //               "	 Select " +
                            //               " icp.IcmalPaymentId,	i.SubeId as SubeId," +
                            //               "	  CONVERT(VARCHAR,icp.PaymentDate, 105) PaymentDate , 0 as CashAmount, Sum(icp.Amount) KreditAmount , 0 as TicketAmount, 0 as OnlineAmount, 0 as DebitAmount		 " +
                            //               "	From IcmalKreditPayment icp " +
                            //               "	inner join IcmalPayment i on icp.IcmalPaymentId=i.Id" +
                            //               "	Group By CONVERT(VARCHAR,icp.PaymentDate, 105),icp.Id,i.SubeId,icp.IcmalPaymentId" +
                            //               "	 Union all" +
                            //               "    Select " +
                            //               "	 icp.IcmalPaymentId, i.SubeId as SubeId," +
                            //               "	 CONVERT(VARCHAR,icp.PaymentDate, 105) PaymentDate, 0 as CashAmount, 0 as KreditAmount,  Sum(icp.Amount) TicketAmount, 0 as OnlineAmount, 0 as DebitAmount 			" +
                            //               "	 From IcmalTicketPayment icp " +
                            //               "	 inner join IcmalPayment i on icp.IcmalPaymentId=i.Id	" +
                            //               "	Group By CONVERT(VARCHAR,icp.PaymentDate, 105),icp.Id,i.SubeId, icp.IcmalPaymentId" +
                            //               "	) v	" +
                            //               "	where v.SubeId =" + SubeId + "  " +
                            //               " Group By v.paymentDate, v.SubeId, v.IcmalPaymentId " +
                            //               " order by v.PaymentDate desc ";
                            Query = " SELECT " +
                                    "     v.IcmalPaymentId," +
                                    "     v.ConfirmationStatus" +
                                    "	,v.SubeId" +
                                    "	,v.PaymentDate" +
                                    "	,Sum(v.CashAmount) AS CashAmount" +
                                    "	,Sum(v.KreditAmount) AS KreditAmount" +
                                    "	,Sum(v.TicketAmount) AS TicketAmount" +
                                    "	,Sum(v.OnlineAmount) AS OnlineAmount" +
                                    "	,sum(v.DebitAmount) AS DebitAmount" +
                                    " FROM (" +
                                    "	SELECT icp.Id AS IcmalPaymentId," +
                                    "	       icp.ConfirmationStatus As ConfirmationStatus" +
                                    "		,icp.SubeId AS SubeId" +
                                    "		,CONVERT(VARCHAR, icp.PaymentDate, 105) PaymentDate" +
                                    "		,Sum(icp.CashAmount) CashAmount" +
                                    "		,0 AS KreditAmount" +
                                    "		,0 AS TicketAmount" +
                                    "		,Sum(icp.OnlineAmount) AS OnlineAmount" +
                                    "		,sum(icp.DebitAmount) AS DebitAmount" +
                                    "	FROM IcmalPayment icp" +
                                    "	GROUP BY CONVERT(VARCHAR, icp.PaymentDate, 105)" +
                                    "		,icp.Id" +
                                    "		,icp.SubeId" +
                                    "		,icp.ConfirmationStatus" +
                                    "	" +
                                    "	UNION ALL" +
                                    "	" +
                                    "	SELECT icp.IcmalPaymentId," +
                                    "	i.ConfirmationStatus As ConfirmationStatus" +
                                    "		,i.SubeId AS SubeId" +
                                    "		,CONVERT(VARCHAR, icp.PaymentDate, 105) PaymentDate" +
                                    "		,0 AS CashAmount" +
                                    "		,Sum(icp.Amount) KreditAmount" +
                                    "		,0 AS TicketAmount" +
                                    "		,0 AS OnlineAmount" +
                                    "		,0 AS DebitAmount" +
                                    "	FROM IcmalKreditPayment icp" +
                                    "	INNER JOIN IcmalPayment i ON icp.IcmalPaymentId = i.Id" +
                                    "	GROUP BY CONVERT(VARCHAR, icp.PaymentDate, 105)" +
                                    "		,icp.Id" +
                                    "		,i.SubeId" +
                                    "		,icp.IcmalPaymentId" +
                                    "		,i.ConfirmationStatus" +
                                    "	" +
                                    "	UNION ALL" +
                                    "	" +
                                    "	SELECT icp.IcmalPaymentId," +
                                    "	i.ConfirmationStatus As ConfirmationStatus" +
                                    "		,i.SubeId AS SubeId" +
                                    "		,CONVERT(VARCHAR, icp.PaymentDate, 105) PaymentDate" +
                                    "		,0 AS CashAmount" +
                                    "		,0 AS KreditAmount" +
                                    "		,Sum(icp.Amount) TicketAmount" +
                                    "		,0 AS OnlineAmount" +
                                    "		,0 AS DebitAmount" +
                                    "	FROM IcmalTicketPayment icp" +
                                    "	INNER JOIN IcmalPayment i ON icp.IcmalPaymentId = i.Id" +
                                    "	GROUP BY CONVERT(VARCHAR, icp.PaymentDate, 105)" +
                                    "		,icp.Id" +
                                    "		,i.SubeId" +
                                    "		,icp.IcmalPaymentId" +
                                    "		,i.ConfirmationStatus" +
                                    "	) v " +
                                    " WHERE v.SubeId = " + SubeId + " " +
                                    " GROUP BY v.paymentDate" +
                                    "	,v.SubeId" +
                                    "	,v.IcmalPaymentId" +
                                    "	,v.ConfirmationStatus" +
                                    " ORDER BY v.PaymentDate DESC";
                        }
                        else if (AppDbType == "2")
                        {
                            Query = File.ReadAllText(HostingEnvironment.MapPath("/Sql/SubeToplamCiro.sql"), System.Text.UTF8Encoding.Default);
                        }
                        else if (AppDbType == "3")//Faster
                        { }
                        else if (AppDbType == "4")//NPOS>4
                        { }

                        #endregion SEFİM YENI-ESKİ FASTER SQL

                        Query = Query.Replace("{SUBEADI}", SubeAdi);
                        Query = Query.Replace("{TARIH1}", QueryTimeStart);
                        Query = Query.Replace("{TARIH2}", QueryTimeEnd);

                        if (ID == "1")
                        {
                            #region SUPER ADMİN 

                            try
                            {
                                var SubeCiroDt = new DataTable();
                                SubeCiroDt = mfunc.GetSubeDataWithQuery(mfunc.NewConnectionString(masterDbSubeIp, masterDbName, masterSqlKullaniciName, masterSqlKullaniciPassword), Query.ToString());

                                try
                                {
                                    if (SubeCiroDt.Rows.Count > 0)
                                    {
                                        foreach (DataRow SubeR in SubeCiroDt.Rows)
                                        {
                                            var items = new IcmalViewModel
                                            {
                                                SubeName = SubeAdi,
                                                SubeId = SubeId,
                                                Id = mfunc.RTI(SubeR, "IcmalPaymentId"),
                                                CashAmount = mfunc.RTD(SubeR, "CashAmount"),
                                                KreditAmount = mfunc.RTD(SubeR, "KreditAmount"),
                                                TicketAmount = mfunc.RTD(SubeR, "TicketAmount"),
                                                OnlineAmount = mfunc.RTD(SubeR, "OnlineAmount"),
                                                DebitAmount = mfunc.RTD(SubeR, "DebitAmount"),
                                                PaymentDate = mfunc.RTS(SubeR, "PaymentDate"),
                                                OnayDurumu = (OnayDurumu)mfunc.RTI(SubeR, "ConfirmationStatus"),
                                            };

                                            lock (locked)
                                            {
                                                icmalList.Add(items);
                                            }
                                        }
                                    }
                                    else
                                    {
                                        var items = new IcmalViewModel
                                        {
                                            SubeName = SubeAdi + " (Data Yok) ",
                                            SubeId = SubeId,
                                        };

                                        lock (locked)
                                        {
                                            icmalList.Add(items);
                                        }
                                    }
                                }
                                catch (Exception) { throw new Exception(SubeAdi); }
                            }
                            catch (Exception ex)
                            {

                            }

                            #endregion SUPER ADMİN 
                        }
                        else
                        {
                            #region KULLANICININ ŞUBE YETKİ KONTROLU YAPILIYOR    

                            foreach (var item in model.FR_SubeListesi)
                            {
                                if (item.SubeID == Convert.ToInt32(SubeId))
                                {
                                    #region SUPER ADMİN 

                                    try
                                    {
                                        var SubeCiroDt = new DataTable();
                                        SubeCiroDt = mfunc.GetSubeDataWithQuery(mfunc.NewConnectionString(masterDbSubeIp, masterDbName, masterSqlKullaniciName, masterSqlKullaniciPassword), Query.ToString());

                                        try
                                        {
                                            if (SubeCiroDt.Rows.Count > 0)
                                            {
                                                foreach (DataRow SubeR in SubeCiroDt.Rows)
                                                {
                                                    var items = new IcmalViewModel
                                                    {
                                                        SubeName = SubeAdi,
                                                        SubeId = SubeId,
                                                        Id = mfunc.RTI(SubeR, "IcmalPaymentId"),
                                                        CashAmount = mfunc.RTD(SubeR, "CashAmount"),
                                                        KreditAmount = mfunc.RTD(SubeR, "KreditAmount"),
                                                        TicketAmount = mfunc.RTD(SubeR, "TicketAmount"),
                                                        OnlineAmount = mfunc.RTD(SubeR, "OnlineAmount"),
                                                        DebitAmount = mfunc.RTD(SubeR, "DebitAmount"),
                                                        PaymentDate = mfunc.RTS(SubeR, "PaymentDate"),
                                                        OnayDurumu = (OnayDurumu)mfunc.RTI(SubeR, "ConfirmationStatus"),
                                                    };

                                                    lock (locked)
                                                    {
                                                        icmalList.Add(items);
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                var items = new IcmalViewModel
                                                {
                                                    SubeName = SubeAdi + " (Data Yok) ",
                                                    SubeId = SubeId,
                                                };

                                                lock (locked)
                                                {
                                                    icmalList.Add(items);
                                                }
                                            }
                                        }
                                        catch (Exception) { throw new Exception(SubeAdi); }
                                    }
                                    catch (Exception ex)
                                    {

                                    }

                                    #endregion SUPER ADMİN 
                                }
                            }

                            #endregion KULLANICININ ŞUBE YETKİ KONTROLU YAPILIYOR  
                        }
                    });
                    #endregion PARALEL FORECH
                }
                catch (Exception ex)
                {
                    Singleton.WritingLogFile2("IcmalList", ex.ToString(), null, ex.StackTrace);
                }
            }
            catch (DataException ex)
            {
                Singleton.WritingLogFile2("IcmalList", ex.ToString(), null, ex.StackTrace);
            }

            return icmalList;
        }
        public static IcmalViewModel IcmalCompareList(DateTime Date1, DateTime Date2, string ID, string subeId, int IcmalPaymentId)
        {
            //GİRİŞ YAPAN KULLANICININ YETKİLİ OLDUĞU ŞUBELER LİSTELENİYOR
            var model = UsersListCRUD.YetkiliSubesi(ID);
            var mfunc = new ModelFunctions();
            var icmalList = new IcmalViewModel()
            {
                IcmalSalesTypesTemp = new List<IcmalSalesTypes>(),
                IcmalSalesTypesBranch = new List<IcmalSalesTypes>(),
            };

            try
            {
                var dateTime = Singleton.GetTimeViewModel(Date1.ToString(), Date2.ToString(), "bugun");

                #region SUBSTATION LIST                
                mfunc.SqlConnOpen();
                string filter = "Where Status=1";
                if (subeId != null && !subeId.Equals("0") && !subeId.Equals(""))
                    filter += " and Id=" + subeId;
                var dt = mfunc.DataTable("Select * From SubeSettings " + filter);
                var subeList = dt.AsEnumerable().ToList<DataRow>();
                mfunc.SqlConnClose();
                #endregion SUBSTATION LIST

                try
                {
                    #region PARALEL FORECH

                    var locked = new Object();

                    Parallel.ForEach(subeList, r =>
                    {
                        string SubeId = r["Id"].ToString();
                        string SubeAdi = r["SubeName"].ToString();
                        string SubeIP = r["SubeIP"].ToString();
                        string SqlName = r["SqlName"].ToString();
                        string SqlPassword = r["SqlPassword"].ToString();
                        string DBName = r["DBName"].ToString();
                        string FirmaId_SUBE = "F0" + r["FirmaID"].ToString() + "TBLKRDSUBELER";
                        string FirmaId_KASA = "F0" + r["FirmaID"].ToString() + "TBLKRDKASALAR";
                        string Firma_NPOS = r["FirmaID"].ToString();
                        string QueryTimeStart = dateTime.StartDate; //Date1.ToString("yyyy/MM/dd HH:mm:ss");
                        string QueryTimeEnd = dateTime.EndDate; //Date2.ToString("yyyy/MM/dd HH:mm:ss");

                        #region  SEFİM YENI - ESKİ FASTER SQL

                        string AppDbType = mfunc.RTS(r, "AppDbType");
                        string AppDbTypeStatus = mfunc.RTS(r, "AppDbTypeStatus");
                        string Query = string.Empty;

                        if (AppDbType == "1")// 1 = yeni şefim, 2 =eski Şefim, 3 = Faster
                        {

                            Query = " SELECT " +
                                    "	 v.ConfirmationStatus" +
                                    "	,v.IcmalPaymentId" +
                                    "	,v.SubeId" +
                                    "	,v.PaymentDate" +
                                    "	,Sum(v.CashAmount) AS CashAmount" +
                                    "	,Sum(v.KreditAmount) AS KreditAmount" +
                                    "	,Sum(v.TicketAmount) AS TicketAmount" +
                                    "	,Sum(v.OnlineAmount) AS OnlineAmount" +
                                    "	,sum(v.DebitAmount) AS DebitAmount" +
                                    " FROM (" +
                                    "	SELECT " +
                                    "	    icp.ConfirmationStatus" +
                                    "	    ,icp.Id AS IcmalPaymentId		" +
                                    "		,icp.SubeId AS SubeId" +
                                    "		,CONVERT(VARCHAR, icp.PaymentDate, 105) PaymentDate" +
                                    "		,Sum(icp.CashAmount) CashAmount" +
                                    "		,0 AS KreditAmount" +
                                    "		,0 AS TicketAmount" +
                                    "		,Sum(icp.OnlineAmount) AS OnlineAmount" +
                                    "		,sum(icp.DebitAmount) AS DebitAmount" +
                                    "	FROM IcmalPayment icp" +
                                    "	GROUP BY CONVERT(VARCHAR, icp.PaymentDate, 105)" +
                                    "		,icp.Id" +
                                    "		,icp.SubeId" +
                                    "		,icp.ConfirmationStatus" +
                                    "	" +
                                    "	UNION ALL" +
                                    "	" +
                                    "	SELECT " +
                                    "	    i.ConfirmationStatus" +
                                    "		,icp.IcmalPaymentId" +
                                    "		,i.SubeId AS SubeId" +
                                    "		,CONVERT(VARCHAR, icp.PaymentDate, 105) PaymentDate" +
                                    "		,0 AS CashAmount" +
                                    "		,Sum(icp.Amount) KreditAmount" +
                                    "		,0 AS TicketAmount" +
                                    "		,0 AS OnlineAmount" +
                                    "		,0 AS DebitAmount" +
                                    "	FROM IcmalKreditPayment icp" +
                                    "	INNER JOIN IcmalPayment i ON icp.IcmalPaymentId = i.Id" +
                                    "	GROUP BY CONVERT(VARCHAR, icp.PaymentDate, 105)" +
                                    "		,icp.Id" +
                                    "		,i.SubeId" +
                                    "		,icp.IcmalPaymentId" +
                                 "          ,i.ConfirmationStatus " +
                                    "	UNION ALL" +
                                    "	" +
                                    "	SELECT " +
                                    "	    i.ConfirmationStatus" +
                                    "		,icp.IcmalPaymentId" +
                                    "		,i.SubeId AS SubeId" +
                                    "		,CONVERT(VARCHAR, icp.PaymentDate, 105) PaymentDate" +
                                    "		,0 AS CashAmount" +
                                    "		,0 AS KreditAmount" +
                                    "		,Sum(icp.Amount) AS TicketAmount" +
                                    "		,0 AS OnlineAmount" +
                                    "		,0 AS DebitAmount" +
                                    "	FROM IcmalTicketPayment icp" +
                                    "	INNER JOIN IcmalPayment i ON icp.IcmalPaymentId = i.Id" +
                                    "	GROUP BY CONVERT(VARCHAR, icp.PaymentDate, 105)" +
                                    "		,icp.Id" +
                                    "		,i.SubeId" +
                                    "		,icp.IcmalPaymentId" +
                                    "       ,i.ConfirmationStatus " +
                                    "	) v" +
                                    " WHERE v.SubeId = " + SubeId + " " +
                                    "	AND v.IcmalPaymentId = " + IcmalPaymentId + " " +
                                    " GROUP BY v.paymentDate" +
                                    "	,v.SubeId" +
                                    "	,v.IcmalPaymentId" +
                                    "	,v.ConfirmationStatus" +
                                    " ORDER BY v.PaymentDate DESC";
                        }
                        else if (AppDbType == "2")
                        {
                            Query = File.ReadAllText(HostingEnvironment.MapPath("/Sql/SubeToplamCiro.sql"), System.Text.UTF8Encoding.Default);
                        }
                        else if (AppDbType == "3")//Faster
                        { }
                        else if (AppDbType == "4")//NPOS>4
                        { }

                        #endregion SEFİM YENI-ESKİ FASTER SQL

                        Query = Query.Replace("{SUBEADI}", SubeAdi);
                        Query = Query.Replace("{TARIH1}", QueryTimeStart);
                        Query = Query.Replace("{TARIH2}", QueryTimeEnd);

                        if (ID == "1")
                        {
                            #region SUPER ADMİN 

                            try
                            {
                                var icmalTempDataList = new DataTable();
                                icmalTempDataList = mfunc.GetSubeDataWithQuery(mfunc.NewConnectionString(masterDbSubeIp, masterDbName, masterSqlKullaniciName, masterSqlKullaniciPassword), Query.ToString());
                                try
                                {
                                    if (icmalTempDataList.Rows.Count > 0)
                                    {
                                        foreach (DataRow SubeR in icmalTempDataList.Rows)
                                        {
                                            var items = new IcmalSalesTypes
                                            {

                                                SubeName = SubeAdi,
                                                IcmalPaymentId = mfunc.RTI(SubeR, "IcmalPaymentId"),
                                                CashAmount = mfunc.RTD(SubeR, "CashAmount"),
                                                KreditAmount = mfunc.RTD(SubeR, "KreditAmount"),
                                                TicketAmount = mfunc.RTD(SubeR, "TicketAmount"),
                                                OnlineAmount = mfunc.RTD(SubeR, "OnlineAmount"),
                                                DebitAmount = mfunc.RTD(SubeR, "DebitAmount"),
                                                OnayDurumu = (OnayDurumu)mfunc.RTI(SubeR, "ConfirmationStatus"),
                                            };

                                            lock (locked)
                                            {
                                                icmalList.IcmalSalesTypesTemp.Add(items);
                                            }
                                        }
                                    }
                                    else
                                    {
                                        var items = new IcmalViewModel
                                        {
                                            SubeName = SubeAdi + " (Data Yok) ",
                                            SubeId = SubeId,
                                        };
                                    }

                                    Query = File.ReadAllText(HostingEnvironment.MapPath("/Sql/SqlNewSef/SubeToplamCiroNewSefim.sql"), UTF8Encoding.Default);
                                    Query = Query.Replace("{SUBEADI}", SubeAdi);
                                    Query = Query.Replace("{TARIH1}", QueryTimeStart);
                                    Query = Query.Replace("{TARIH2}", QueryTimeEnd);
                                    var icmalSubeDataList = new DataTable();
                                    icmalSubeDataList = mfunc.GetSubeDataWithQuery(mfunc.NewConnectionString(SubeIP, DBName, SqlName, SqlPassword), Query.ToString());

                                    if (icmalSubeDataList.Rows.Count > 0)
                                    {
                                        foreach (DataRow SubeR in icmalSubeDataList.Rows)
                                        {
                                            var items = new IcmalSalesTypes
                                            {
                                                SubeName = SubeAdi,
                                                CashAmount = mfunc.RTD(SubeR, "Cash"),
                                                KreditAmount = mfunc.RTD(SubeR, "Credit"),
                                                TicketAmount = mfunc.RTD(SubeR, "Ticket"),
                                                OnlineAmount = mfunc.RTD(SubeR, "OnlinePayment"),
                                                DebitAmount = mfunc.RTD(SubeR, "Debit"),
                                            };

                                            lock (locked)
                                            {
                                                icmalList.IcmalSalesTypesBranch.Add(items);
                                            }
                                        }
                                    }
                                    else
                                    {
                                        var items = new IcmalViewModel
                                        {
                                            SubeName = SubeAdi + " (Data Yok) ",
                                            SubeId = SubeId,
                                        };
                                    }
                                }
                                catch (Exception)
                                {
                                    throw new Exception(SubeAdi);
                                }
                            }
                            catch (Exception ex)
                            { }

                            #endregion SUPER ADMİN 
                        }
                        else
                        {
                            #region KULLANICININ ŞUBE YETKİ KONTROLU YAPILIYOR    

                            foreach (var item in model.FR_SubeListesi)
                            {
                                if (item.SubeID == Convert.ToInt32(SubeId))
                                {
                                    #region SUPER ADMİN 

                                    try
                                    {
                                        var icmalTempDataList = new DataTable();
                                        icmalTempDataList = mfunc.GetSubeDataWithQuery(mfunc.NewConnectionString(masterDbSubeIp, masterDbName, masterSqlKullaniciName, masterSqlKullaniciPassword), Query.ToString());
                                        try
                                        {
                                            if (icmalTempDataList.Rows.Count > 0)
                                            {
                                                foreach (DataRow SubeR in icmalTempDataList.Rows)
                                                {
                                                    var items = new IcmalSalesTypes
                                                    {

                                                        SubeName = SubeAdi,
                                                        IcmalPaymentId = mfunc.RTI(SubeR, "IcmalPaymentId"),
                                                        CashAmount = mfunc.RTD(SubeR, "CashAmount"),
                                                        KreditAmount = mfunc.RTD(SubeR, "KreditAmount"),
                                                        TicketAmount = mfunc.RTD(SubeR, "TicketAmount"),
                                                        OnlineAmount = mfunc.RTD(SubeR, "OnlineAmount"),
                                                        DebitAmount = mfunc.RTD(SubeR, "DebitAmount"),
                                                        OnayDurumu = (OnayDurumu)mfunc.RTI(SubeR, "ConfirmationStatus"),
                                                    };

                                                    lock (locked)
                                                    {
                                                        icmalList.IcmalSalesTypesTemp.Add(items);
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                var items = new IcmalViewModel
                                                {
                                                    SubeName = SubeAdi + " (Data Yok) ",
                                                    SubeId = SubeId,
                                                };
                                            }

                                            Query = File.ReadAllText(HostingEnvironment.MapPath("/Sql/SqlNewSef/SubeToplamCiroNewSefim.sql"), UTF8Encoding.Default);
                                            Query = Query.Replace("{SUBEADI}", SubeAdi);
                                            Query = Query.Replace("{TARIH1}", QueryTimeStart);
                                            Query = Query.Replace("{TARIH2}", QueryTimeEnd);
                                            var icmalSubeDataList = new DataTable();
                                            icmalSubeDataList = mfunc.GetSubeDataWithQuery(mfunc.NewConnectionString(SubeIP, DBName, SqlName, SqlPassword), Query.ToString());

                                            if (icmalSubeDataList.Rows.Count > 0)
                                            {
                                                foreach (DataRow SubeR in icmalSubeDataList.Rows)
                                                {
                                                    var items = new IcmalSalesTypes
                                                    {
                                                        SubeName = SubeAdi,
                                                        CashAmount = mfunc.RTD(SubeR, "Cash"),
                                                        KreditAmount = mfunc.RTD(SubeR, "Credit"),
                                                        TicketAmount = mfunc.RTD(SubeR, "Ticket"),
                                                        OnlineAmount = mfunc.RTD(SubeR, "OnlinePayment"),
                                                        DebitAmount = mfunc.RTD(SubeR, "Debit"),
                                                    };

                                                    lock (locked)
                                                    {
                                                        icmalList.IcmalSalesTypesBranch.Add(items);
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                var items = new IcmalViewModel
                                                {
                                                    SubeName = SubeAdi + " (Data Yok) ",
                                                    SubeId = SubeId,
                                                };
                                            }
                                        }
                                        catch (Exception)
                                        {
                                            throw new Exception(SubeAdi);
                                        }
                                    }
                                    catch (Exception ex)
                                    { }

                                    #endregion SUPER ADMİN 
                                }
                            }

                            #endregion KULLANICININ ŞUBE YETKİ KONTROLU YAPILIYOR  
                        }
                    });
                    #endregion PARALEL FORECH
                }
                catch (Exception ex)
                {
                    Singleton.WritingLogFile2("IcmalCompareList", ex.ToString(), null, ex.StackTrace);
                }
            }
            catch (DataException ex)
            {
                Singleton.WritingLogFile2("IcmalCompareList", ex.ToString(), null, ex.StackTrace);
            }

            return icmalList;
        }

        #region Create

        public static IcmalViewModel GetCreatedIcmalList(DateTime Date1, DateTime Date2, string kullaniciId, string subeId)
        {
            //GİRİŞ YAPAN KULLANICININ YETKİLİ OLDUĞU ŞUBELER LİSTELENİYOR
            //var model = UsersListCRUD.YetkiliSubesi(kullaniciId);
            var mfunc = new ModelFunctions();
            var icmalList = new IcmalViewModel()
            {
                IcmalSalesTypesTemp = new List<IcmalSalesTypes>(),
                IcmalSalesTypesBranch = new List<IcmalSalesTypes>(),
            };

            try
            {
                //Kullanıcının yetkili olduğu şube
                var kullaniciSubeYetkisi = UsersListCRUD.KullaniciSubeYetkiListesi(kullaniciId);

                #region SUBSTATION LIST                
                mfunc.SqlConnOpen();
                string filter = "Where Status=1";
                if (subeId != null && !subeId.Equals("0") && !subeId.Equals(""))
                    filter += " and Id=" + subeId;
                var dt = mfunc.DataTable("Select * From SubeSettings " + filter);
                var subeList = dt.AsEnumerable().ToList<DataRow>();
                mfunc.SqlConnClose();
                #endregion SUBSTATION LIST

                try
                {
                    #region PARALEL FORECH

                    var locked = new Object();

                    Parallel.ForEach(subeList, r =>
                    {
                        string SubeId = r["Id"].ToString();
                        if (kullaniciSubeYetkisi != null && kullaniciSubeYetkisi.FR_SubeListesi.Where(x => x.SubeID == Convert.ToInt32(SubeId)).Select(x => x.SubeID).Any())
                        {
                            string SubeAdi = r["SubeName"].ToString();
                            string SubeIP = r["SubeIP"].ToString();
                            string SqlName = r["SqlName"].ToString();
                            string SqlPassword = r["SqlPassword"].ToString();
                            string DBName = r["DBName"].ToString();
                            string FirmaId_SUBE = "F0" + r["FirmaID"].ToString() + "TBLKRDSUBELER";
                            string FirmaId_KASA = "F0" + r["FirmaID"].ToString() + "TBLKRDKASALAR";
                            string Firma_NPOS = r["FirmaID"].ToString();
                            string QueryTimeStart = Date1.ToString("yyyy/MM/dd HH:mm:ss");
                            string QueryTimeEnd = Date2.ToString("yyyy/MM/dd HH:mm:ss");

                            #region  SEFİM YENI - ESKİ FASTER SQL

                            string AppDbType = mfunc.RTS(r, "AppDbType");
                            string AppDbTypeStatus = mfunc.RTS(r, "AppDbTypeStatus");
                            string Query = string.Empty;

                            if (AppDbType == "1")// 1 = yeni şefim, 2 =eski Şefim, 3 = Faster
                            {
                                Query = " Select v.ConfirmationStatus, v.IcmalPaymentId, v.SubeId,  v.PaymentDate, " +
                                        "   Sum(v.CashAmount) as CashAmount ,Sum(v.KreditAmount) as KreditAmount, Sum(v.TicketAmount) as TicketAmount,Sum(v.OnlineAmount) as OnlineAmount, sum(v.DebitAmount) as DebitAmount " +
                                        " From" +
                                        "   (" +
                                        "	 Select " +
                                        "	 icp.ConfirmationStatus, icp.Id as IcmalPaymentId, icp.SubeId as SubeId," +
                                        "	 CONVERT(VARCHAR,icp.PaymentDate, 105) PaymentDate, Sum(icp.CashAmount) CashAmount, 0 as KreditAmount, 0 as TicketAmount, Sum(icp.OnlineAmount) as OnlineAmount, sum(icp.DebitAmount) as DebitAmount	 " +
                                        "	 From IcmalPayment icp  Group By CONVERT(VARCHAR,icp.PaymentDate, 105),icp.Id,icp.SubeId,icp.ConfirmationStatus " +
                                        "	 Union all" +
                                        "	 Select " +
                                        "    i.ConfirmationStatus,  icp.IcmalPaymentId,	i.SubeId as SubeId," +
                                        "	  CONVERT(VARCHAR,icp.PaymentDate, 105) PaymentDate , 0 as CashAmount, Sum(icp.Amount) KreditAmount , 0 as TicketAmount, 0 as OnlineAmount, 0 as DebitAmount		 " +
                                        "	From IcmalKreditPayment icp " +
                                        "	inner join IcmalPayment i on icp.IcmalPaymentId=i.Id" +
                                        "	Group By CONVERT(VARCHAR,icp.PaymentDate, 105),icp.Id,i.SubeId,icp.IcmalPaymentId,i.ConfirmationStatus " +
                                        "	 Union all" +
                                        "    Select " +
                                        "	i.ConfirmationStatus, icp.IcmalPaymentId, i.SubeId as SubeId," +
                                        "	 CONVERT(VARCHAR,icp.PaymentDate, 105) PaymentDate, 0 as CashAmount, 0 as KreditAmount,  Sum(icp.Amount) TicketAmount, 0 as OnlineAmount, 0 as DebitAmount 			" +
                                        "	 From IcmalTicketPayment icp " +
                                        "	 inner join IcmalPayment i on icp.IcmalPaymentId=i.Id	" +
                                        "	Group By CONVERT(VARCHAR,icp.PaymentDate, 105),icp.Id,i.SubeId, icp.IcmalPaymentId,i.ConfirmationStatus " +
                                        "	) v	" +
                                        "	where v.SubeId =" + SubeId + " " +
                                        " Group By v.paymentDate, v.SubeId, v.IcmalPaymentId, v.ConfirmationStatus " +
                                        " order by v.PaymentDate desc ";
                            }
                            else if (AppDbType == "2")
                            { }
                            else if (AppDbType == "3")//Faster
                            { }
                            else if (AppDbType == "4")//NPOS>4
                            { }

                            #endregion SEFİM YENI-ESKİ FASTER SQL

                            Query = Query.Replace("{SUBEADI}", SubeAdi);
                            Query = Query.Replace("{TARIH1}", QueryTimeStart);
                            Query = Query.Replace("{TARIH2}", QueryTimeEnd);

                            #region SUPER ADMİN 

                            try
                            {
                                var SubeCiroDt = new DataTable();
                                SubeCiroDt = mfunc.GetSubeDataWithQuery(mfunc.NewConnectionString(masterDbSubeIp, masterDbName, masterSqlKullaniciName, masterSqlKullaniciPassword), Query.ToString());

                                try
                                {
                                    if (SubeCiroDt.Rows.Count > 0)
                                    {
                                        foreach (DataRow SubeR in SubeCiroDt.Rows)
                                        {
                                            var items = new IcmalSalesTypes
                                            {
                                                SubeId = SubeId,
                                                SubeName = SubeAdi,
                                                UserId = kullaniciId,
                                                IcmalPaymentId = mfunc.RTI(SubeR, "IcmalPaymentId"),
                                                PaymentDate = mfunc.RTS(SubeR, "PaymentDate"),
                                                CashAmount = mfunc.RTD(SubeR, "CashAmount"),
                                                KreditAmount = mfunc.RTD(SubeR, "KreditAmount"),
                                                TicketAmount = mfunc.RTD(SubeR, "TicketAmount"),
                                                OnlineAmount = mfunc.RTD(SubeR, "OnlineAmount"),
                                                DebitAmount = mfunc.RTD(SubeR, "DebitAmount"),
                                                OnayDurumu = (OnayDurumu)mfunc.RTD(SubeR, "ConfirmationStatus")
                                            };

                                            lock (locked)
                                            {
                                                icmalList.IcmalSalesTypesTemp.Add(items);
                                            }
                                        }
                                    }
                                    else
                                    {
                                        var items = new IcmalViewModel
                                        {
                                            SubeName = SubeAdi + " (Data Yok) ",
                                            SubeId = SubeId,
                                        };

                                        //lock (locked)
                                        //{
                                        //    icmalList.Add(items);
                                        //}
                                    }
                                }
                                catch (Exception) { throw new Exception(SubeAdi); }
                            }
                            catch (Exception ex)
                            { }

                            #endregion SUPER ADMİN                            
                        }
                    });
                    #endregion PARALEL FORECH
                }
                catch (Exception ex)
                {
                    Singleton.WritingLogFile2("GetCreatedIcmalList", ex.ToString(), null, ex.StackTrace);
                }
            }
            catch (DataException ex)
            {
                Singleton.WritingLogFile2("GetCreatedIcmalList", ex.ToString(), null, ex.StackTrace);
            }

            return icmalList;
        }
        public ActionResultMessages IcmalCreate(IcmalViewModel viewModel)
        {
            var result = new ActionResultMessages { IsSuccess = true, UserMessage = "İşlem Başarılı", };
            var paymentDate = DateTime.Now;

            using (SqlConnection con = new SqlConnection(mF.NewConnectionString(masterDbSubeIp, masterDbName, masterSqlKullaniciName, masterSqlKullaniciPassword)))
            {
                con.Open();
                var transaction = con.BeginTransaction();
                var sqlData = new SqlData(con);

                try
                {
                    var icmalPaymentQuery = " Select * from IcmalPayment Where  CONVERT(VARCHAR,PaymentDate, 105)='" + paymentDate.ToString("dd'-'MM'-'yyyy") + "' and SubeId=" + viewModel.SubeId + "";
                    var cmalPaymentDataTable = mF.GetSubeDataWithQuery(mF.NewConnectionString(masterDbSubeIp, masterDbName, masterSqlKullaniciName, masterSqlKullaniciPassword), icmalPaymentQuery.ToString());
                    if (cmalPaymentDataTable.Rows.Count > 0)
                    {
                        result.UserMessage = paymentDate.ToString("dd'-'MM'-'yyyy") + " Tarihi için daha önceden kayıt girilmiştir.Aynı gün için ikinci bir kayıt girilemez.";
                        result.IsSuccess = false;
                        return result;
                    }

                    var icmalPaymentInsertScript = "INSERT INTO [dbo].[IcmalPayment] " +
                                                   "([SubeId],[SubeName],[PaymentTypeId],[PaymentTypeName],[PaymentDate],[CashAmount],[KreditAmount],[TicketAmount],[OnlineAmount],[DebitAmount],[UserId],[ConfirmationStatus],[CreatedDate],[UpdateDate])" +
                                                   "VALUES(@par1, @par2, @par3, @par4, @par5, @par6, @par7, @par8, @par9, @par10, @par11, @par12, @par13, @par14) select CAST(scope_identity() AS int);";

                    var icmalPaymentId = sqlData.ExecuteScalarTransactionSql(icmalPaymentInsertScript, transaction,
                        new object[]
                        {
                            viewModel.SubeId,
                            viewModel.SubeName,
                            viewModel.PaymentTypeId,
                            viewModel.PaymentTypeName,
                            paymentDate,
                            viewModel.CashAmount,
                            viewModel.KreditAmount,
                            viewModel.TicketAmount,
                            viewModel.OnlineAmount,
                            viewModel.DebitAmount,
                            viewModel.UserId,
                            OnayDurumu.OnayBekliyor.GetHashCode(),
                            paymentDate,
                            null
                        });

                    //
                    if (viewModel.KreditPaymentBankTypeList != null)
                    {
                        foreach (var kreditPay in viewModel.KreditPaymentBankTypeList)
                        {
                            var icmalKreditPaymentInsertScript = "INSERT INTO [dbo].[IcmalKreditPayment] " +
                                                                 "([IcmalPaymentId],[BankaName],[BankBkmId],[Amount],[PaymentDate],[UserId],[CreatedDate],[UpdateDate])" +
                                                                 "VALUES(@par1, @par2, @par3, @par4, @par5, @par6,@par7,@par8) select CAST(scope_identity() AS int);";
                            var icmalKreditPaymentId = sqlData.ExecuteScalarTransactionSql(icmalKreditPaymentInsertScript, transaction,
                                     new object[]
                                     {
                                     icmalPaymentId,
                                     kreditPay.BankName,
                                     kreditPay.BankBkmId,
                                     kreditPay.Amount,
                                     paymentDate,
                                     viewModel.UserId,
                                     paymentDate,
                                     null
                                     });
                        }
                    }

                    if (viewModel.TicketPaymentTicketTypeList != null)
                    {
                        foreach (var ticketPay in viewModel.TicketPaymentTicketTypeList)
                        {
                            var icmalTicketPaymentInsertScript = "INSERT INTO [dbo].[IcmalTicketPayment] " +
                                                                 "([IcmalPaymentId],[TicketName],[TicketId],[Amount],[PaymentDate],[UserId],[CreatedDate],[UpdateDate])" +
                                                                 "VALUES(@par1, @par2, @par3, @par4, @par5, @par6,@par7,@par8) select CAST(scope_identity() AS int);";
                            var icmalTicketPaymentId = sqlData.ExecuteScalarTransactionSql(icmalTicketPaymentInsertScript, transaction,
                                     new object[]
                                     {
                                     icmalPaymentId,
                                     ticketPay.TicketName,
                                     ticketPay.TicketId,
                                     ticketPay.Amount,
                                     paymentDate,
                                     viewModel.UserId,
                                     paymentDate,
                                     null
                                     });
                        }
                    }

                    transaction.Commit();
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    Singleton.WritingLogFile2("IcmalCRUD_IcmalCreate", ex.ToString(), null, ex.StackTrace);
                    result.IsSuccess = false;
                    result.UserMessage = "Bir Hata Oluştu";
                    return result;
                }
            }

            return result;
        }

        #endregion Create

        #region Update
        public static IcmalViewModel GetByIdForIcmalUpdate(int id, string kullaniciId, string subeId)
        {
            //GİRİŞ YAPAN KULLANICININ YETKİLİ OLDUĞU ŞUBELER LİSTELENİYOR
            //var model = UsersListCRUD.YetkiliSubesi(kullaniciId);
            var mfunc = new ModelFunctions();
            var icmalList = new IcmalViewModel()
            {
                KreditPaymentBankTypeList = new List<KreditPaymentBank>(),
                TicketPaymentTicketTypeList = new List<TicketPaymentTicketTypeList>(),
            };
            DateTime Date1;
            DateTime Date2;

            try
            {
                //Kullanıcının yetkili olduğu şube
                var kullaniciSubeYetkisi = UsersListCRUD.KullaniciSubeYetkiListesi(kullaniciId);

                #region SUBSTATION LIST                
                mfunc.SqlConnOpen();
                string filter = "Where Status=1";
                if (subeId != null && !subeId.Equals("0") && !subeId.Equals(""))
                    filter += " and Id=" + subeId;
                var dt = mfunc.DataTable("Select * From SubeSettings " + filter);
                var subeList = dt.AsEnumerable().ToList<DataRow>();
                mfunc.SqlConnClose();
                #endregion SUBSTATION LIST

                try
                {
                    #region PARALEL FORECH

                    var locked = new Object();

                    Parallel.ForEach(subeList, r =>
                    {
                        string SubeId = r["Id"].ToString();
                        if (kullaniciSubeYetkisi != null && kullaniciSubeYetkisi.FR_SubeListesi.Where(x => x.SubeID == Convert.ToInt32(SubeId)).Select(x => x.SubeID).Any())
                        {
                            string SubeAdi = r["SubeName"].ToString();
                            string SubeIP = r["SubeIP"].ToString();
                            string SqlName = r["SqlName"].ToString();
                            string SqlPassword = r["SqlPassword"].ToString();
                            string DBName = r["DBName"].ToString();
                            string FirmaId_SUBE = "F0" + r["FirmaID"].ToString() + "TBLKRDSUBELER";
                            string FirmaId_KASA = "F0" + r["FirmaID"].ToString() + "TBLKRDKASALAR";
                            string Firma_NPOS = r["FirmaID"].ToString();
                            //string QueryTimeStart = Date1.ToString("yyyy/MM/dd HH:mm:ss");
                            //string QueryTimeEnd = Date2.ToString("yyyy/MM/dd HH:mm:ss");

                            #region  SEFİM YENI - ESKİ FASTER SQL

                            string AppDbType = mfunc.RTS(r, "AppDbType");
                            string AppDbTypeStatus = mfunc.RTS(r, "AppDbTypeStatus");
                            string Query = string.Empty;

                            if (AppDbType == "1")// 1 = yeni şefim, 2 =eski Şefim, 3 = Faster
                            {
                                Query = " Select * from IcmalPayment Where Id=" + id + " and SubeId=" + subeId + "";
                            }
                            else if (AppDbType == "2")
                            {
                                Query = File.ReadAllText(HostingEnvironment.MapPath("/Sql/SubeToplamCiro.sql"), System.Text.UTF8Encoding.Default);
                            }
                            else if (AppDbType == "3")//Faster
                            { }
                            else if (AppDbType == "4")//NPOS>4
                            { }

                            #endregion SEFİM YENI-ESKİ FASTER SQL

                            Query = Query.Replace("{SUBEADI}", SubeAdi);
                            //Query = Query.Replace("{TARIH1}", QueryTimeStart);
                            //Query = Query.Replace("{TARIH2}", QueryTimeEnd);

                            #region SUPER ADMİN 

                            try
                            {
                                var dataTable = new DataTable();
                                dataTable = mfunc.GetSubeDataWithQuery(mfunc.NewConnectionString(masterDbSubeIp, masterDbName, masterSqlKullaniciName, masterSqlKullaniciPassword), Query.ToString());

                                try
                                {
                                    if (dataTable.Rows.Count > 0)
                                    {
                                        foreach (DataRow SubeR in dataTable.Rows)
                                        {
                                            icmalList.Id = id;
                                            icmalList.SubeId = SubeId;
                                            icmalList.SubeName = SubeAdi;
                                            icmalList.PaymentDate = mfunc.RTS(SubeR, "PaymentDate");
                                            icmalList.CashAmount = mfunc.RTD(SubeR, "CashAmount");
                                            icmalList.KreditAmount = mfunc.RTD(SubeR, "KreditAmount");
                                            icmalList.TicketAmount = mfunc.RTD(SubeR, "TicketAmount");
                                            icmalList.OnlineAmount = mfunc.RTD(SubeR, "OnlineAmount");
                                            icmalList.DebitAmount = mfunc.RTD(SubeR, "DebitAmount");
                                        }

                                        //Kredili Satış Listesi                                     
                                        Query = "Select * from IcmalKreditPayment  where IcmalPaymentId=" + id + " ";
                                        dataTable = mfunc.GetSubeDataWithQuery(mfunc.NewConnectionString(masterDbSubeIp, masterDbName, masterSqlKullaniciName, masterSqlKullaniciPassword), Query.ToString());
                                        foreach (DataRow subeK in dataTable.Rows)
                                        {
                                            var itemsK = new KreditPaymentBank
                                            {
                                                IcmalPaymentId = id,
                                                IcmalKreditPaymentId = mfunc.RTI(subeK, "Id"),
                                                Amount = mfunc.RTD(subeK, "Amount"),
                                                BankaTipi = (BankaTipi)mfunc.RTI(r, "BankaTipi"),
                                                BankBkmId = mfunc.RTI(subeK, "BankBkmId"),
                                                BankName = mfunc.RTS(subeK, "BankaName"),
                                            };

                                            lock (locked)
                                            {
                                                icmalList.KreditPaymentBankTypeList.Add(itemsK);
                                            }
                                        }

                                        //Yemek Kartlı satış listesi
                                        Query = "Select * from IcmalTicketPayment  where IcmalPaymentId=" + id + " ";
                                        dataTable = mfunc.GetSubeDataWithQuery(mfunc.NewConnectionString(masterDbSubeIp, masterDbName, masterSqlKullaniciName, masterSqlKullaniciPassword), Query.ToString());
                                        foreach (DataRow subeT in dataTable.Rows)
                                        {
                                            var itemsT = new TicketPaymentTicketTypeList
                                            {
                                                IcmalPaymentId = id,
                                                IcmalTicketPaymentId = mfunc.RTI(subeT, "Id"),
                                                Amount = mfunc.RTD(subeT, "Amount"),
                                                YemekKartiTipi = (YemekKartiTipi)mfunc.RTI(r, "YemekKartiTipi"),
                                                TicketId = mfunc.RTI(subeT, "TicketId"),
                                                TicketName = mfunc.RTS(subeT, "TicketName"),
                                            };

                                            lock (locked)
                                            {
                                                icmalList.TicketPaymentTicketTypeList.Add(itemsT);
                                            }
                                        }
                                    }
                                    else
                                    {
                                        var items = new IcmalViewModel
                                        {
                                            SubeName = SubeAdi + " (Data Yok) ",
                                            SubeId = SubeId,
                                        };

                                        //lock (locked)
                                        //{
                                        //    icmalList.Add(items);
                                        //}
                                    }
                                }
                                catch (Exception) { throw new Exception(SubeAdi); }
                            }
                            catch (Exception ex)
                            { }

                            #endregion SUPER ADMİN                            
                        }
                    });
                    #endregion PARALEL FORECH
                }
                catch (Exception ex)
                {
                    Singleton.WritingLogFile2("GetByIdForIcmalUpdate", ex.ToString(), null, ex.StackTrace);
                }
            }
            catch (DataException ex)
            {
                Singleton.WritingLogFile2("GetByIdForIcmalUpdate", ex.ToString(), null, ex.StackTrace);
            }

            return icmalList;
        }
        public ActionResultMessages IcmalUpdate(IcmalViewModel viewModel)
        {
            var result = new ActionResultMessages { IsSuccess = true, UserMessage = "İşlem Başarılı", };
            var paymentDate = DateTime.Now;
          
            using (SqlConnection con = new SqlConnection(mF.NewConnectionString(masterDbSubeIp, masterDbName, masterSqlKullaniciName, masterSqlKullaniciPassword)))
            {
                //
                var query = "Select * from IcmalPayment  where Id=" + viewModel.Id + " ";
                var icmalData = mF.GetSubeDataWithQuery(mF.NewConnectionString(masterDbSubeIp, masterDbName, masterSqlKullaniciName, masterSqlKullaniciPassword), query.ToString());
                var onayDurumu = (OnayDurumu)mF.RTI(icmalData.Rows[0], "ConfirmationStatus");
                if (onayDurumu == OnayDurumu.Onayli)
                {
                    result.IsSuccess = false;
                    result.UserMessage = "Onaylı icmal güncellenemez.";
                    return result;
                }

                con.Open();
                var transaction = con.BeginTransaction();
                var sqlData = new SqlData(con);

                try
                {
                    var icmalPaymentInsertScript = " BEGIN TRAN UPDATE [dbo].[IcmalPayment] SET" +
                                                   " [CashAmount]=@par1,[KreditAmount]=@par2,[TicketAmount]=@par3,[OnlineAmount]=@par4,[DebitAmount]=@par5,[UserId]=@par6,[UpdateDate]=@par7 " +
                                                   " Where Id=@par8 and SubeId=@par9 SELECT @@TRANCOUNT AS OpenTransactions COMMIT TRAN SELECT @@TRANCOUNT AS OpenTransactions ";

                    var icmalPaymentId = sqlData.ExecuteScalarTransactionSql(icmalPaymentInsertScript, transaction,
                        new object[]
                        {
                            viewModel.CashAmount,
                            viewModel.KreditAmount,
                            viewModel.TicketAmount,
                            viewModel.OnlineAmount,
                            viewModel.DebitAmount,
                            viewModel.UserId,
                            paymentDate,
                            viewModel.Id,
                            viewModel.SubeId,
                        });

                    //
                    if (viewModel.KreditPaymentBankTypeList != null)
                    {
                        foreach (var kreditPay in viewModel.KreditPaymentBankTypeList)
                        {
                            if (kreditPay.IcmalKreditPaymentId == 0)
                            {
                                var icmalKreditPaymentInsertScript = " INSERT INTO [dbo].[IcmalKreditPayment] " +
                                                                     " ([IcmalPaymentId],[BankaName],[BankBkmId],[Amount],[PaymentDate],[UserId],[CreatedDate],[UpdateDate])" +
                                                                     " VALUES(@par1, @par2, @par3, @par4, @par5, @par6,@par7,@par8) select CAST(scope_identity() AS int);";
                                var icmalKreditPaymentId = sqlData.ExecuteScalarTransactionSql(icmalKreditPaymentInsertScript, transaction,
                                         new object[]
                                         {
                                             viewModel.Id,
                                             kreditPay.BankName,
                                             kreditPay.BankBkmId,
                                             kreditPay.Amount,
                                             Convert.ToDateTime(viewModel.PaymentDate),
                                             viewModel.UserId,
                                             paymentDate,
                                             null
                                         });
                            }
                            else
                            {
                                var icmalKreditPaymentInsertScript = " BEGIN TRAN UPDATE [dbo].[IcmalKreditPayment] SET " +
                                                                     " [BankaName]=@par1,[BankBkmId]=@par2,[Amount]=@par3,[UserId]=@par4,[UpdateDate]=@par5 " +
                                                                     " Where IcmalPaymentId=@par6 and Id=@par7 SELECT @@TRANCOUNT AS OpenTransactions COMMIT TRAN SELECT @@TRANCOUNT AS OpenTransactions";
                                var icmalKreditPaymentId = sqlData.ExecuteScalarTransactionSql(icmalKreditPaymentInsertScript, transaction,
                                         new object[]
                                         {
                                             kreditPay.BankName,
                                             kreditPay.BankBkmId,
                                             kreditPay.Amount,
                                             viewModel.UserId,
                                             paymentDate,
                                             viewModel.Id,
                                             kreditPay.IcmalKreditPaymentId
                                         });
                            }
                        }
                    }

                    if (viewModel.TicketPaymentTicketTypeList != null)
                    {
                        foreach (var ticketPay in viewModel.TicketPaymentTicketTypeList)
                        {
                            if (ticketPay.IcmalTicketPaymentId == 0)
                            {
                                var icmalTicketPaymentInsertScript = " INSERT INTO [dbo].[IcmalTicketPayment] " +
                                                                     " ([IcmalPaymentId],[TicketName],[TicketId],[Amount],[PaymentDate],[UserId],[CreatedDate],[UpdateDate])" +
                                                                     " VALUES(@par1, @par2, @par3, @par4, @par5, @par6,@par7,@par8) select CAST(scope_identity() AS int);";
                                var icmalTicketPaymentId = sqlData.ExecuteScalarTransactionSql(icmalTicketPaymentInsertScript, transaction,
                                         new object[]
                                         {
                                             viewModel.Id,
                                             ticketPay.TicketName,
                                             ticketPay.TicketId,
                                             ticketPay.Amount,
                                             Convert.ToDateTime(viewModel.PaymentDate),
                                             viewModel.UserId,
                                             paymentDate,
                                             null,
                                         });
                            }
                            else
                            {
                                var icmalTicketPaymentInsertScript = " BEGIN TRAN UPDATE [dbo].[IcmalTicketPayment] SET " +
                                                                     " [TicketName]=@par1,[TicketId]=@par2,[Amount]=@par3,[UserId]=@par4,[UpdateDate]=@par5 " +
                                                                     " Where IcmalPaymentId=@par6 and Id=@par7 SELECT @@TRANCOUNT AS OpenTransactions COMMIT TRAN SELECT @@TRANCOUNT AS OpenTransactions";
                                var icmalTicketPaymentId = sqlData.ExecuteScalarTransactionSql(icmalTicketPaymentInsertScript, transaction,
                                         new object[]
                                         {
                                             ticketPay.TicketName,
                                             ticketPay.TicketId,
                                             ticketPay.Amount,
                                             viewModel.UserId,
                                             paymentDate,
                                             viewModel.Id,
                                             ticketPay.IcmalTicketPaymentId
                                         });
                            }
                        }
                    }

                    transaction.Commit();
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    Singleton.WritingLogFile2("IcmalCRUD_IcmalUpdate", ex.ToString(), null, ex.StackTrace);
                    result.IsSuccess = false;
                    result.UserMessage = "Bir Hata Oluştu";
                    return result;
                }
            }

            return result;
        }

        #endregion Update

        public ActionResultMessages IcmalOnayla(string Id, string SubeId, OnayDurumu onayDurumu)
        {
            var result = new ActionResultMessages() { IsSuccess = true, UserMessage = "İşlem Başarılı", };
            using (SqlConnection con = new SqlConnection(mF.NewConnectionString(masterDbSubeIp, masterDbName, masterSqlKullaniciName, masterSqlKullaniciPassword)))
            {
                con.Open();
                var transaction = con.BeginTransaction();
                var sqlData = new SqlData(con);
                var xx = onayDurumu.GetHashCode();
                try
                {
                    sqlData.ExecuteScalarTransactionSql("BEGIN TRAN Update IcmalPayment Set ConfirmationStatus=@par1 Where Id=@par2 SELECT @@TRANCOUNT AS OpenTransactions COMMIT TRAN SELECT @@TRANCOUNT AS OpenTransactions", transaction, new object[]
                    {
                        onayDurumu.GetHashCode(),
                        Id,
                    });

                    transaction.Commit();
                }
                catch (Exception ex)
                {
                    transaction.Rollback();

                    Singleton.WritingLogFile2("IcmalOnayla", ex.ToString(), null, ex.StackTrace);
                    result.IsSuccess = false;
                    result.UserMessage = "İşlem Başarısız.";
                }
            }

            return result;
        }
    }
}