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
    public class PaketCiroCRUD
    {
        public static List<PaketCiro> List(DateTime Date1, DateTime Date2, string ID)
        {
            List<PaketCiro> Liste = new List<PaketCiro>();
            ModelFunctions f = new ModelFunctions();
            DateTime startDate = DateTime.Now;

            #region GİRİŞ YAPAN KULLANICININ YETKİLİ OLDUĞU ŞUBELER LİSTELENİYOR
            UserViewModel model = UsersListCRUD.YetkiliSubesi(ID);
            #endregion

            try
            {
                #region SUBSTATION LIST               
                f.SqlConnOpen();
                DataTable dt = f.DataTable("select * from SubeSettings Where Status=1");
                var dtList = dt.AsEnumerable().ToList<DataRow>();
                TimeSpan sure = DateTime.Now - startDate;
                f.SqlConnClose();
                #endregion SUBSTATION LIST

                #region PARALLEL FOREACH

                var locked = new Object();
                Parallel.ForEach(dtList, r =>
                   {
                       //foreach (DataRow r in dt.Rows)
                       //{
                       string AppDbType = f.RTS(r, "AppDbType");
                       string SubeId = f.RTS(r, "Id");
                       string SubeAdi = f.RTS(r, "SubeName");
                       string SubeIP = f.RTS(r, "SubeIP");
                       string SqlName = f.RTS(r, "SqlName");
                       string SqlPassword = f.RTS(r, "SqlPassword");
                       string DBName = f.RTS(r, "DBName");
                       string QueryTimeStart = Date1.ToString("yyyy-MM-dd HH:mm:ss");
                       string QueryTimeEnd = Date2.ToString("yyyy-MM-dd HH:mm:ss");
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
                       if (AppDbType == "1")// 1 = yeni şefim, 2 =eski Şefim, 3 = Faster
                       {
                           Query = File.ReadAllText(HostingEnvironment.MapPath("/Sql/SqlNewSef/PaketCiroNewSefim.sql"), System.Text.UTF8Encoding.Default);
                       }
                       else if (AppDbType == "2")
                       {
                           Query = File.ReadAllText(HostingEnvironment.MapPath("/Sql/PaketCiro.sql"), System.Text.UTF8Encoding.Default);
                       }
                       else if (AppDbType == "3")
                       {
                           //Query = File.ReadAllText(HostingEnvironment.MapPath("/Sql/SqlFaster/SubeToplamCiroFASTER.sql"), System.Text.Encoding.UTF8);
                       }
                       else if (AppDbType == "5")
                       {
                           //if(string.IsNullOrWhiteSpace(SubeId))
                           Query = File.ReadAllText(HostingEnvironment.MapPath("/Sql/VPos/PaketServisRaporu/PaketServis.sql"), System.Text.UTF8Encoding.Default);

                           //if (!string.IsNullOrWhiteSpace(SubeId))
                           //    Query = File.ReadAllText(HostingEnvironment.MapPath("/Sql/VPos/PaketServisRaporu/PaketServisDetay.sql"), System.Text.UTF8Encoding.Default);
                       }
                           #endregion
                           Query = Query.Replace("{TARIH1}", QueryTimeStart);
                       Query = Query.Replace("{TARIH2}", QueryTimeEnd);
                       Query = Query.Replace("{SUBE}", SubeAdi);
                       Query = Query.Replace("{SUBE2}", vPosSubeKodu);
                       Query = Query.Replace("{KASAKODU}", vPosKasaKodu);

                       if (ID == "1")
                       {
                           #region GET DATA
                           try
                           {
                               string connString = "Provider=SQLOLEDB;Server=" + SubeIP + ";User Id=" + SqlName + ";Password=" + SqlPassword + ";Database=" + DBName + "";

                               DataTable AcikHesapDt = new DataTable();
                               AcikHesapDt = f.GetSubeDataWithQuery((f.NewConnectionString(SubeIP, DBName, SqlName, SqlPassword)), Query.ToString());

                               //foreach (DataRow SubeR in AcikHesapDt.Rows)
                               //{
                               if (AcikHesapDt.Rows.Count > 0)
                               {
                                   //items.Sube = SubeCiroDt.Rows[0]["Sube"].ToString(); //f.RTS(SubeR, "Sube");
                                   //items.SubeId = SubeId;
                                   //items.Cash = Convert.ToDecimal(SubeCiroDt.Rows[0]["Cash"]); //f.RTD(SubeR, "Cash");
                                   PaketCiro items = new PaketCiro();
                                   items.Sube = SubeAdi;
                                   items.SubeID = Convert.ToInt32(SubeId);
                                   //items.Adet = f.RTI(SubeR, "ADET");
                                   //items.Debit = f.RTD(SubeR, "TUTAR");
                                   //items.PhoneOrderDebit = f.RTI(SubeR, "PhoneOrderDebit");
                                   items.OrderCount = Convert.ToInt32(AcikHesapDt.Rows[0]["OrderCount"]);  //f.RTI(SubeR, "OrderCount");
                                   items.Total = f.RTD(AcikHesapDt.Rows[0], "Total"); //Convert.ToDecimal(AcikHesapDt.Rows[0]["Total"]); //f.RTD(SubeR, "Total");
                                   items.Debit = f.RTD(AcikHesapDt.Rows[0], "Debit");//Convert.ToDecimal(AcikHesapDt.Rows[0]["Debit"]);//f.RTD(SubeR, "Debit");
                                   items.CashPayment = Convert.ToDecimal(AcikHesapDt.Rows[0]["CashPayment"]);//f.RTD(SubeR, "CashPayment");
                                   items.CreditPayment = Convert.ToDecimal(AcikHesapDt.Rows[0]["CreditPayment"]); //f.RTD(SubeR, "CreditPayment");
                                   items.TicketPayment = Convert.ToDecimal(AcikHesapDt.Rows[0]["TicketPayment"]); //f.RTD(SubeR, "TicketPayment");
                                   items.Discount = Convert.ToDecimal(AcikHesapDt.Rows[0]["Discount"]);//f.RTD(SubeR, "Discount");
                                                                                                       //items.CollectedTotal = f.RTD(SubeR, "CollectedTotal");
                                                                                                       //items.Balance = f.RTD(SubeR, "Balance");
                                   items.OnlinePayment = f.RTD(AcikHesapDt.Rows[0], "OnlinePayment");
                                   items.GETIR = f.RTD(AcikHesapDt.Rows[0], "GETIR");
                                   items.TRENDYOL = f.RTD(AcikHesapDt.Rows[0], "TRENDYOL");
                                   items.YEMEKSEPETI = f.RTD(AcikHesapDt.Rows[0], "YEMEKSEPETI");
                                   lock (locked)
                                   {
                                       Liste.Add(items);
                                   }
                               }
                               else
                               {
                                   PaketCiro items = new PaketCiro
                                   {
                                       Sube = SubeAdi + " (Data Yok)",
                                       SubeID = Convert.ToInt32(SubeId),
                                       Adet = 0,
                                       Debit = 0,
                                       PhoneOrderDebit = 0
                                   };
                                   lock (locked)
                                   {
                                       Liste.Add(items);
                                   }
                               }
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
                               PaketCiro items = new PaketCiro();
                               items.Sube = SubeAdi + " (Erişim Yok)";
                               items.SubeID = Convert.ToInt32(SubeId);
                               //items.CustomerName = cu;
                               //items.Debit = SubeId;
                               items.ErrorMessage = ErrorMessage;
                               items.ErrorStatus = true;
                               items.ErrorCode = "01";
                               // bağlantı hatasında eklenecek
                               //items.Sube = SubeAdi + " (Erişim veya Data Yok) ";
                               //items.SubeID = Convert.ToInt32(SubeId);
                               items.Adet = 0;//f.RTI(SubeR, "ADET");
                               items.Debit = 0;// f.RTD(SubeR, "TUTAR");
                               items.PhoneOrderDebit = 0; //f.RTI(SubeR, "PhoneOrderDebit");
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

                                       DataTable AcikHesapDt = new DataTable();
                                       AcikHesapDt = f.GetSubeDataWithQuery((f.NewConnectionString(SubeIP, DBName, SqlName, SqlPassword)), Query.ToString());

                                       //foreach (DataRow SubeR in AcikHesapDt.Rows)
                                       //{
                                       if (AcikHesapDt.Rows.Count > 0)
                                       {
                                           //items.Sube = SubeCiroDt.Rows[0]["Sube"].ToString(); //f.RTS(SubeR, "Sube");
                                           //items.SubeId = SubeId;
                                           //items.Cash = Convert.ToDecimal(SubeCiroDt.Rows[0]["Cash"]); //f.RTD(SubeR, "Cash");
                                           PaketCiro items = new PaketCiro();
                                           items.Sube = SubeAdi;
                                           items.SubeID = Convert.ToInt32(SubeId);
                                           //items.Adet = f.RTI(SubeR, "ADET");
                                           //items.Debit = f.RTD(SubeR, "TUTAR");
                                           //items.PhoneOrderDebit = f.RTI(SubeR, "PhoneOrderDebit");
                                           items.OrderCount = Convert.ToInt32(AcikHesapDt.Rows[0]["OrderCount"]);  //f.RTI(SubeR, "OrderCount");
                                           items.Total = f.RTD(AcikHesapDt.Rows[0], "Total"); //Convert.ToDecimal(AcikHesapDt.Rows[0]["Total"]); //f.RTD(SubeR, "Total");
                                           items.Debit = f.RTD(AcikHesapDt.Rows[0], "Debit");//Convert.ToDecimal(AcikHesapDt.Rows[0]["Debit"]);//f.RTD(SubeR, "Debit");
                                           items.CashPayment = Convert.ToDecimal(AcikHesapDt.Rows[0]["CashPayment"]);//f.RTD(SubeR, "CashPayment");
                                           items.CreditPayment = Convert.ToDecimal(AcikHesapDt.Rows[0]["CreditPayment"]); //f.RTD(SubeR, "CreditPayment");
                                           items.TicketPayment = Convert.ToDecimal(AcikHesapDt.Rows[0]["TicketPayment"]); //f.RTD(SubeR, "TicketPayment");
                                           items.Discount = Convert.ToDecimal(AcikHesapDt.Rows[0]["Discount"]);//f.RTD(SubeR, "Discount");
                                                                                                               //items.CollectedTotal = f.RTD(SubeR, "CollectedTotal");
                                                                                                               //items.Balance = f.RTD(SubeR, "Balance");
                                           items.OnlinePayment = f.RTD(AcikHesapDt.Rows[0], "OnlinePayment");
                                           items.GETIR = f.RTD(AcikHesapDt.Rows[0], "GETIR");
                                           items.TRENDYOL = f.RTD(AcikHesapDt.Rows[0], "TRENDYOL");
                                           items.YEMEKSEPETI = f.RTD(AcikHesapDt.Rows[0], "YEMEKSEPETI");
                                           lock (locked)
                                           {
                                               Liste.Add(items);
                                           }
                                       }
                                       else
                                       {
                                           PaketCiro items = new PaketCiro();
                                           items.Sube = SubeAdi + " (Data Yok)";
                                           items.SubeID = Convert.ToInt32(SubeId);
                                           items.Adet = 0;
                                           items.Debit = 0;
                                           items.PhoneOrderDebit = 0;
                                           lock (locked)
                                           {
                                               Liste.Add(items);
                                           }
                                       }
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
                                       PaketCiro items = new PaketCiro();
                                       items.Sube = SubeAdi + " (Erişim Yok)";
                                       items.SubeID = Convert.ToInt32(SubeId);
                                       //items.CustomerName = cu;
                                       //items.Debit = SubeId;
                                       items.ErrorMessage = ErrorMessage;
                                       items.ErrorStatus = true;
                                       items.ErrorCode = "01";
                                       // bağlantı hatasında eklenecek
                                       //items.Sube = SubeAdi + " (Erişim veya Data Yok) ";
                                       //items.SubeID = Convert.ToInt32(SubeId);
                                       items.Adet = 0;//f.RTI(SubeR, "ADET");
                                       items.Debit = 0;// f.RTD(SubeR, "TUTAR");
                                       items.PhoneOrderDebit = 0; //f.RTI(SubeR, "PhoneOrderDebit");
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
                #endregion PARALLEL FOREACH
            }
            catch (DataException ex) { }

            return Liste;
        }
    }
}