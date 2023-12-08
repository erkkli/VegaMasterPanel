using SefimV2.ViewModels.KisiOrtalamaRaporlar;
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
    public class KisiOrtalamaRaporlarCRUD
    {
        public static List<KisiOrtalamaViewModel> List(DateTime Date1, DateTime Date2, string ID)
        {
            List<KisiOrtalamaViewModel> Liste = new List<KisiOrtalamaViewModel>();
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
                try
                {
                    #region PARALEL FORECH

                    var dtList = dt.AsEnumerable().ToList<DataRow>();
                    Parallel.ForEach(dtList, r =>
                    {
                        ModelFunctions f = new ModelFunctions();
                        //foreach (DataRow r in dt.Rows)
                        //{
                        string SubeId = r["ID"].ToString();
                        string SubeAdi = r["SubeName"].ToString();
                        string SubeIP = r["SubeIP"].ToString();
                        string SqlName = r["SqlName"].ToString();
                        string SqlPassword = r["SqlPassword"].ToString();
                        string DBName = r["DBName"].ToString();
                        string QueryTimeStart = Date1.ToString("yyyy/MM/dd HH:mm:ss");
                        string QueryTimeEnd = Date2.ToString("yyyy/MM/dd HH:mm:ss");
                        #region  SEFİM YENI - ESKİ FASTER SQL
                        string AppDbType = f.RTS(r, "AppDbType");
                        string Query = "";
                        if (AppDbType == "1")// 1 = yeni şefim, 2 =eski Şefim, 3 = Faster
                        {
                            Query = File.ReadAllText(HostingEnvironment.MapPath("/Sql/SqlNewSef/KisiOrtalamaRaporlarNewSefim.sql"), System.Text.UTF8Encoding.Default);
                        }
                        else if (AppDbType == "2")
                        {
                            Query = File.ReadAllText(HostingEnvironment.MapPath("/Sql/KisiOrtalamaRaporlar.sql"), System.Text.UTF8Encoding.Default);
                        }
                        else if (AppDbType == "3")
                        {
                            //Query = File.ReadAllText(HostingEnvironment.MapPath("/Sql/SqlFaster/KisiOrtalamaRaporlarFASTER.sql"), System.Text.Encoding.UTF8);
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
                                string connString = "Provider=SQLOLEDB;Server=" + SubeIP + ";User ID=" + SqlName + ";Password=" + SqlPassword + ";Database=" + DBName + "";
                                try
                                {
                                    DataTable AcikHesapDt = new DataTable();
                                    AcikHesapDt = f.GetSubeDataWithQuery((f.NewConnectionString(SubeIP, DBName, SqlName, SqlPassword)), Query.ToString());

                                    if (AcikHesapDt.Rows.Count > 0)
                                    {
                                        KisiOrtalamaViewModel items = new KisiOrtalamaViewModel();
                                        items.Sube = SubeAdi;
                                        items.SubeID = Convert.ToInt32(SubeId);
                                        //items.Adet = f.RTI(SubeR, "ADET");
                                        //items.Debit = f.RTD(SubeR, "TUTAR");
                                        //items.PhoneOrderDebit = f.RTI(SubeR, "PhoneOrderDebit");
                                        //KasaCiroDt.Rows[0]["Sube"].ToString();                               
                                        items.Kisi = Convert.ToInt32(AcikHesapDt.Rows[0]["Kisi"].ToString()); //f.RTI(SubeR, "Kisi");
                                        items.Total = Convert.ToDecimal(AcikHesapDt.Rows[0]["Total"].ToString());  //f.RTD(SubeR, "Total");
                                        items.Ortalama = Convert.ToDecimal(AcikHesapDt.Rows[0]["Ortalama"].ToString()); //f.RTD(SubeR, "Ortalama");
                                        Liste.Add(items);
                                    }
                                    else
                                    {
                                        KisiOrtalamaViewModel items = new KisiOrtalamaViewModel();
                                        items.ErrorStatus = true;
                                        items.ErrorCode = "01";
                                        items.Sube = SubeAdi + " (Data Yok)";                                       
                                        items.Kisi = 0;//f.RTI(SubeR, "ADET");
                                        items.Total = 0;// f.RTD(SubeR, "TUTAR");
                                        items.Ortalama = 0; //f.RTI(SubeR, "PhoneOrderDebit");
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
                                    string ErrorMessage = "Kasa Raporu Alınamadı.";
                                    //string SystemErrorMessage = ex.Message.ToString();
                                    //string LogText = "";
                                    //LogText += "Hata Zamanı : " + DateTime.Now.ToString() + Environment.NewLine;
                                    //LogText += "Hata Mesajı : " + ErrorMessage + Environment.NewLine;
                                    //LogText += "Hata Açıklaması : " + SystemErrorMessage + Environment.NewLine;
                                    //LogText += "-----------------" + Environment.NewLine;
                                    KisiOrtalamaViewModel items = new KisiOrtalamaViewModel();
                                    //items.CustomerName = cu;
                                    //items.Debit = SubeId;
                                    items.ErrorMessage = ErrorMessage;
                                    items.ErrorStatus = true;
                                    items.ErrorCode = "01";
                                    // bağlantı hatasında eklenecek
                                    items.Sube = ex.Message + " (Erişim Yok)";
                                    items.SubeID = Convert.ToInt32(SubeId);
                                    items.Kisi = 0;//f.RTI(SubeR, "ADET");
                                    items.Total = 0;// f.RTD(SubeR, "TUTAR");
                                    items.Ortalama = 0; //f.RTI(SubeR, "PhoneOrderDebit");
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
                                        string connString = "Provider=SQLOLEDB;Server=" + SubeIP + ";User ID=" + SqlName + ";Password=" + SqlPassword + ";Database=" + DBName + "";
                                        try
                                        {
                                            //f.SqlConnOpen(true, connString);
                                            //DataTable AcikHesapDt = f.DataTable(Query, true);
                                            DataTable AcikHesapDt = new DataTable();
                                            AcikHesapDt = f.GetSubeDataWithQuery((f.NewConnectionString(SubeIP, DBName, SqlName, SqlPassword)), Query.ToString());

                                            if (AcikHesapDt.Rows.Count > 0)
                                            {
                                                KisiOrtalamaViewModel items = new KisiOrtalamaViewModel();
                                                items.Sube = SubeAdi;
                                                items.SubeID = Convert.ToInt32(SubeId);
                                                //items.Adet = f.RTI(SubeR, "ADET");
                                                //items.Debit = f.RTD(SubeR, "TUTAR");
                                                //items.PhoneOrderDebit = f.RTI(SubeR, "PhoneOrderDebit");
                                                //KasaCiroDt.Rows[0]["Sube"].ToString();                               
                                                items.Kisi = Convert.ToInt32(AcikHesapDt.Rows[0]["Kisi"].ToString()); //f.RTI(SubeR, "Kisi");
                                                items.Total = Convert.ToDecimal(AcikHesapDt.Rows[0]["Total"].ToString());  //f.RTD(SubeR, "Total");
                                                items.Ortalama = Convert.ToDecimal(AcikHesapDt.Rows[0]["Ortalama"].ToString()); //f.RTD(SubeR, "Ortalama");
                                                Liste.Add(items);
                                            }
                                            else
                                            {
                                                KisiOrtalamaViewModel items = new KisiOrtalamaViewModel();
                                                items.ErrorStatus = true;
                                                items.ErrorCode = "01";
                                                items.Sube = SubeAdi + " (Data Yok)";
                                                items.Kisi = 0;//f.RTI(SubeR, "ADET");
                                                items.Total = 0;// f.RTD(SubeR, "TUTAR");
                                                items.Ortalama = 0; //f.RTI(SubeR, "PhoneOrderDebit");
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
                                            string ErrorMessage = "Kasa Raporu Alınamadı.";
                                            //string SystemErrorMessage = ex.Message.ToString();
                                            //string LogText = "";
                                            //LogText += "Hata Zamanı : " + DateTime.Now.ToString() + Environment.NewLine;
                                            //LogText += "Hata Mesajı : " + ErrorMessage + Environment.NewLine;
                                            //LogText += "Hata Açıklaması : " + SystemErrorMessage + Environment.NewLine;
                                            //LogText += "-----------------" + Environment.NewLine;
                                            KisiOrtalamaViewModel items = new KisiOrtalamaViewModel();
                                            //items.CustomerName = cu;
                                            //items.Debit = SubeId;
                                            items.ErrorMessage = ErrorMessage;
                                            items.ErrorStatus = true;
                                            items.ErrorCode = "01";
                                            // bağlantı hatasında eklenecek
                                            items.Sube = ex.Message + " (Erişim Yok)";
                                            items.SubeID = Convert.ToInt32(SubeId);
                                            items.Kisi = 0;//f.RTI(SubeR, "ADET");
                                            items.Total = 0;// f.RTD(SubeR, "TUTAR");
                                            items.Ortalama = 0; //f.RTI(SubeR, "PhoneOrderDebit");
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
                                        catch (Exception)
                                        { }
                                    }
                                    #endregion
                                }
                            }
                            #endregion
                        }
                    });
                    #endregion PARALEL FORECH
                }
                catch (Exception) { }
            }
            catch (DataException ex) { }

            return Liste;
        }
    }
}