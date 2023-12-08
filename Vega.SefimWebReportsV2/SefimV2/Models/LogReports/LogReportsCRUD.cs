using SefimV2.ViewModels.LogReports;
using SefimV2.ViewModels.User;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Hosting;

namespace SefimV2.Models.LogReports
{
    public class LogReportsCRUD
    {

        public static List<LogReportsViewModels> List(DateTime Date1, DateTime Date2, string subeid, string ID)
        {
            List<LogReportsViewModels> Liste = new List<LogReportsViewModels>();
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
                    if (AppDbType == "1")// 1 = yeni şefim, 2 =eski Şefim, 3 = Faster
                    {
                        Query = File.ReadAllText(HostingEnvironment.MapPath("/Sql/SqlNewSef/KasiyerTahsilatNewSefim.sql"), System.Text.UTF8Encoding.Default);
                    }
                    else if (AppDbType == "2")
                    {
                        Query = File.ReadAllText(HostingEnvironment.MapPath("/Sql/PersonelCiro.sql"), System.Text.UTF8Encoding.Default);
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
                       

                            Query =
                                       " declare @Sube nvarchar(100) = '{SUBE}'; " +
                                       " declare @Trh1 nvarchar(20) = '{TARIH1}';" +
                                       " declare @Trh2 nvarchar(20) = '{TARIH2}';" +                                     
                                       " SELECT " +
                                       " (SELECT SUBEADI FROM F0"+ FirmaId + "TBLKRDSUBELER WHERE IND = SATIS.SUBEIND) AS Sube1, " +
                                       " (SELECT KASAADI FROM F0"+ FirmaId + "TBLKRDKASALAR WHERE IND = SATIS.KASAIND) AS Kasa, " +
                                       " USR.USERNAME, " +
                                       " SATIS.ISLEMTARIHI, " +
                                       " SATIS.ACIKLAMA, " +
                                       " SATIS.BELGENO, " +
                                       " CAR.FIRMAADI, " +
                                       " STK.STOKKODU, " +
                                       " STK.MALINCINSI " +
                                       " FROM TBLFASTERLOG AS SATIS " +
                                       " LEFT JOIN F0"+FirmaId+"TBLCARI AS CAR ON CAR.IND = SATIS.CARIIND " +
                                       " LEFT JOIN F0"+FirmaId+"TBLSTOKLAR AS STK ON STK.IND = SATIS.STOKIND " +
                                       " LEFT JOIN TBLUSERS AS USR ON USR.IND = SATIS.USERIND " +
                                       " WHERE SATIS.ISLEMTARIHI >= @Trh1 and SATIS.ISLEMTARIHI <= @Trh2 " +
                                       //" AND SATIS.KASAIND = 100 " +
                                       //" AND SATIS.SUBEIND = 100 " +
                                       " ORDER BY SATIS.ISLEMTARIHI ";
                            #endregion FASTER ONLINE QUARY
                        }
                        else
                        {
                            Query = File.ReadAllText(HostingEnvironment.MapPath("/Sql/SqlLogReports/LogReportsFasterOFF.sql"), System.Text.UTF8Encoding.Default);
                        }
                    }
                    #endregion


                    Query = Query.Replace("{TARIH1}", QueryTimeStart);
                    Query = Query.Replace("{TARIH2}", QueryTimeEnd);
                    Query = Query.Replace("{SUBE}", SubeAdi);

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
                                                LogReportsViewModels items = new LogReportsViewModels();
                                                items.Sube = SubeAdi + "-" + f.RTS(sube, "Sube1") + "-" + f.RTS(sube, "Kasa");
                                                items.SubeID = SubeId + "~" + (f.RTS(sube, "Id"));
                                                items.USERNAME = f.RTS(sube, "USERNAME");
                                                items.BELGENO = f.RTS(sube, "BELGENO");
                                                items.ACIKLAMA = f.RTS(sube, "ACIKLAMA");
                                                items.FIRMAADI = f.RTS(sube, "FIRMAADI");
                                                items.ISLEMTARIHI = f.RTS(sube, "ISLEMTARIHI");
                                                items.MALINCINSI = f.RTS(sube, "MALINCINSI");
                                                items.STOKKODU = f.RTS(sube, "STOKKODU");
                                                Liste.Add(items);
                                            }
                                        }
                                        else
                                        {
                                            //LogReportsViewModels items = new LogReportsViewModels();
                                            //items.Sube = SubeAdi; //f.RTS(SubeUrunCiroDt.Rows[0], "Sube");
                                            //items.SubeID = SubeId;
                                            //items.PersonelAdi = "Kasa Satış"; //f.RTS(SubeR, "PersonelAdi");
                                            //foreach (DataRow SubeR in AcikHesapDt.Rows)
                                            //{
                                            //    items.Total += f.RTD(SubeR, "Total");
                                            //}
                                            //Liste.Add(items);
                                        }
                                    }
                                    else
                                    {
                                        if (AppDbType == "3")
                                        {
                                            foreach (DataRow sube in AcikHesapDt.Rows)
                                            {
                                                if (subeid == sube["Id"].ToString())
                                                {
                                                    LogReportsViewModels items = new LogReportsViewModels();
                                                    items.Sube = SubeAdi + "-" + sube["Sube1"].ToString() + "-" + sube["Kasa"].ToString();
                                                    items.SubeID = SubeId + "~" + (sube["Id"].ToString());
                                                    items.USERNAME = f.RTS(sube, "USERNAME");
                                                    items.ACIKLAMA = f.RTS(sube, "ACIKLAMA");
                                                    items.FIRMAADI = f.RTS(sube, "FIRMAADI");
                                                    items.ISLEMTARIHI = f.RTS(sube, "ISLEMTARIHI");
                                                    items.MALINCINSI = f.RTS(sube, "MALINCINSI");
                                                    items.STOKKODU = f.RTS(sube, "STOKKODU");
                                                    Liste.Add(items);
                                                }
                                            }
                                        }
                                        else
                                        {
                                            foreach (DataRow SubeR in AcikHesapDt.Rows)
                                            {
                                                //PersonelCiro items = new PersonelCiro();
                                                //items.Sube = SubeAdi;
                                                //items.SubeID = (SubeId);
                                                //items.ReceivedByUserName = f.RTS(SubeR, "ReceivedByUserName");
                                                //items.CashPayment = f.RTD(SubeR, "CashPayment");
                                                //items.Total = f.RTD(SubeR, "Total");
                                                //items.CreditPayment = f.RTD(SubeR, "CreditPayment");
                                                //items.TicketPayment = f.RTD(SubeR, "TicketPayment");
                                                //Liste.Add(items);
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    LogReportsViewModels items = new LogReportsViewModels();
                                    items.Sube = SubeAdi + " (Data Yok) ";
                                    items.SubeID = (SubeId);
                                    //items.PersonelAdi = "*";
                                    //items.Debit = 0;
                                    Liste.Add(items);
                                }
                            }
                            catch (Exception) { throw new Exception(SubeAdi); }

                        }
                        catch (System.Exception ex)
                        {
                            #region EX
                            //log metnini oluştur
                            //string SystemErrorMessage = ex.Message.ToString();
                            //string LogText = "";
                            //LogText += "Hata Zamanı : " + DateTime.Now.ToString() + Environment.NewLine;
                            //LogText += "Hata Mesajı : " + ErrorMessage + Environment.NewLine;
                            //LogText += "Hata Açıklaması : " + SystemErrorMessage + Environment.NewLine;
                            //LogText += "-----------------" + Environment.NewLine;
                            LogReportsViewModels items = new LogReportsViewModels();
                            //items.CustomerName = cu;
                            //items.Debit = SubeId;
                            items.Sube = ex.Message + " (Erişim Yok) ";
                            items.SubeID = (SubeId);
                            //items.PersonelAdi = "*";
                            //items.Debit = 0;//f.RTD(SubeR, "TUTAR");
                            //items.ErrorMessage = ErrorMessage;
                            //items.ErrorStatus = true;
                            //items.ErrorCode = "01";
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
                            #endregion EX
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


    }
}