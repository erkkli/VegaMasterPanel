using SefimV2.Helper;
using SefimV2.ViewModels.KisiOrtalamaRaporlar;
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
    public class KisiOrtalamaRaporlarCRUD
    {
        public static List<KisiOrtalamaViewModel> List(DateTime Date1, DateTime Date2, string ID)
        {
            var Liste = new List<KisiOrtalamaViewModel>();
            var ff = new ModelFunctions();
            var startDate = DateTime.Now;

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

                    #region PARALEL FORECH

                    var dtList = dt.AsEnumerable().ToList<DataRow>();
                    Parallel.ForEach(dtList, r =>
                    {
                        ModelFunctions f = new ModelFunctions();
                        //foreach (DataRow r in dt.Rows)
                        //{  
                        string AppDbType = f.RTS(r, "AppDbType");
                        string SubeId = r["Id"].ToString();
                        string SubeAdi = r["SubeName"].ToString();
                        string SubeIP = r["SubeIP"].ToString();
                        string SqlName = r["SqlName"].ToString();
                        string SqlPassword = r["SqlPassword"].ToString();
                        string DBName = r["DBName"].ToString();
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
                        else if (AppDbType == "5")
                        {
                            Query = File.ReadAllText(HostingEnvironment.MapPath("/Sql/VPos/KisiOrtalamaRaporu/KisiOrtalamaRaporu.sql"), System.Text.UTF8Encoding.Default);
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
                            catch (Exception ex)
                            {
                                #region EX
                                Singleton.WritingLog("KisiOrtalamaRaporlarCRUD", ex.Message);
                                var items = new KisiOrtalamaViewModel
                                {
                                    Sube = ex.Message + " (Erişim Yok)",
                                    SubeID = Convert.ToInt32(SubeId),
                                };
                                Liste.Add(items);

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
                                    catch (Exception ex)
                                    {
                                        #region EX
                                        Singleton.WritingLog("KisiOrtalamaRaporlarCRUD", ex.Message);
                                        var items = new KisiOrtalamaViewModel
                                        {
                                            Sube = ex.Message + " (Erişim Yok)",
                                            SubeID = Convert.ToInt32(SubeId),
                                        };
                                        Liste.Add(items);

                                        #endregion

                                    }
                                    #endregion
                                }
                            }
                            #endregion
                        }
                    });
                    #endregion PARALEL FORECH
                }
                catch (Exception ex)
                {
                    Singleton.WritingLog("KisiOrtalamaRaporlarCRUD", ex.Message);
                }
            
            return Liste;
        }
    }
}