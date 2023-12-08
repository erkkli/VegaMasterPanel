using SefimV2.Helper;
using SefimV2.ViewModels.User;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Web.Hosting;

namespace SefimV2.Models
{
    public class DashboardListCRUD
    {
        public static SubeCiro DashboardList_(DateTime Date1, DateTime Date2, string ID)
        {
            var items = new SubeCiro();
            var Liste = new List<SubeCiro>();
            var f = new ModelFunctions();
            var startDate = DateTime.Now;

            #region GİRİŞ YAPAN KULLANICININ YETKİLİ OLDUĞU ŞUBELER LİSTELENİYOR
            UserViewModel model = UsersListCRUD.YetkiliSubesi(ID);
            #endregion
            try
            {
                f.SqlConnOpen();
                DataTable dt = f.DataTable("SELECT * FROM SubeSettings WHERE Status=1 ");
                TimeSpan sure = DateTime.Now - startDate;

                foreach (DataRow r in dt.Rows)
                {
                    string AppDbType = f.RTS(r, "AppDbType");
                    string SubeId = f.RTS(r, "Id");
                    string SubeAdi = f.RTS(r, "SubeName");
                    string SubeIP = f.RTS(r, "SubeIP");
                    string SqlName = f.RTS(r, "SqlName");
                    string SqlPassword = f.RTS(r, "SqlPassword");
                    string DBName = f.RTS(r, "DBName");
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
                        Query = File.ReadAllText(HostingEnvironment.MapPath("/Sql/SqlNewSef/SubeToplamCiroNewSefim.sql"), System.Text.UTF8Encoding.Default);
                    }
                    else if (AppDbType == "2")
                    {
                        Query = File.ReadAllText(HostingEnvironment.MapPath("/Sql/SubeToplamCiro.sql"), System.Text.UTF8Encoding.Default);
                    }
                    else if (AppDbType == "3")
                    {
                        Query = File.ReadAllText(HostingEnvironment.MapPath("/Sql/SqlFaster/SubeToplamCiroFASTER.sql"), System.Text.UTF8Encoding.Default);
                    }
                    else if (AppDbType == "5")
                    {
                        Query = File.ReadAllText(HostingEnvironment.MapPath("/Sql/VPos/DashboardSubeToplamCiro/SubeToplamCiroDashbord.sql"), System.Text.UTF8Encoding.Default);
                    }
                    #endregion SEFİM YENI - ESKİ FASTER SQL

                    Query = Query.Replace("{SUBEADI}", SubeAdi);
                    Query = Query.Replace("{TARIH1}", QueryTimeStart);
                    Query = Query.Replace("{TARIH2}", QueryTimeEnd);
                    Query = Query.Replace("{SUBE2}", vPosSubeKodu);
                    Query = Query.Replace("{KASAKODU}", vPosKasaKodu);

                    if (ID == "1")
                    {
                        #region SUPER ADMİN                      
                        #region GET DATA                
                        try
                        {
                            string connString = "Provider=SQLOLEDB;Server=" + SubeIP + ";User Id=" + SqlName + ";Password=" + SqlPassword + ";Database=" + DBName + "";
                            f.SqlConnOpen(true, connString);
                            DataTable SubeCiroDt = f.DataTable(Query, true);
                            //var bugunsatis = temp_satis != null ? temp_satis.Value : 0;
                            foreach (DataRow SubeR in SubeCiroDt.Rows)
                            {
                                //SubeCiro items = new SubeCiro();
                                items.Sube = f.RTS(SubeR, "Sube");
                                items.SubeId = SubeId;
                                //items.Cash += f.RTD(SubeR, "Cash");
                                //items.Credit += f.RTD(SubeR, "Credit");
                                //items.Ticket += f.RTD(SubeR, "Ticket");
                                items.ikram += f.RTD(SubeR, "ikram");
                                items.TableNo += f.RTD(SubeR, "TableNo");
                                items.Discount += f.RTD(SubeR, "Discount");
                                items.iptal += f.RTD(SubeR, "iptal");
                                items.Zayi += f.RTD(SubeR, "Zayi");
                                //items.GecenZaman = Convert.ToInt32(sure.TotalSeconds);
                                //items.Debit = f.RTD(SubeR, "Debit");
                                //items.OpenTable = f.RTD(SubeR, "OpenTable");
                                items.Ciro += f.RTD(SubeR, "ToplamCiro");
                                items.ToplamCiro = String.Format("{0:C}", items.Ciro).ToString();
                                items.ToplamIkram = String.Format("{0:C}", items.ikram).ToString();
                                items.ToplamTableNo = String.Format("{0:C}", items.TableNo).ToString();
                                items.ToplamDiscount = String.Format("{0:C}", items.Discount).ToString();
                                items.ToplamIptal = String.Format("{0:C}", items.iptal).ToString();
                                items.ToplamZayi = String.Format("{0:C}", items.Zayi).ToString();
                                Liste.Add(items);
                            }

                            f.SqlConnClose(true);
                        }
                        catch (Exception ex)
                        {
                            #region EX                       
                            //log metnini oluştur
                            Singleton.WritingLogFile("DashboardListCRUD", ex.Message.ToString());
                            string ErrorMessage = "Ciro Raporu Alınamadı.";
                            items.Sube = SubeAdi;
                            items.SubeId = SubeId;
                            items.ErrorMessage = ErrorMessage;
                            items.ErrorStatus = true;
                            items.ErrorCode = "01";
                            Liste.Add(items);

                            #endregion EX    
                        }
                        #endregion  GET DATA  
                        #endregion
                    }
                    else
                    {
                        #region KULLANICININ ŞUBE YETKİ KONTROLU YAPILIYOR             
                        foreach (var item in model.FR_SubeListesi)
                        {
                            if (item.SubeID == Convert.ToInt32(SubeId))
                            {
                                //return Liste;
                                #region GET DATA                
                                try
                                {
                                    string connString = "Provider=SQLOLEDB;Server=" + SubeIP + ";User Id=" + SqlName + ";Password=" + SqlPassword + ";Database=" + DBName + "";
                                    f.SqlConnOpen(true, connString);
                                    DataTable SubeCiroDt = f.DataTable(Query, true);
                                    foreach (DataRow SubeR in SubeCiroDt.Rows)
                                    {
                                        //SubeCiro items = new SubeCiro();
                                        items.Sube = f.RTS(SubeR, "Sube");
                                        items.SubeId = SubeId;
                                        items.Cash = f.RTD(SubeR, "Cash");
                                        items.Credit = f.RTD(SubeR, "Credit");
                                        items.Ticket = f.RTD(SubeR, "Ticket");
                                        //items.ikram = f.RTD(SubeR, "ikram");
                                        //items.TableNo = f.RTD(SubeR, "TableNo");
                                        //items.Discount = f.RTD(SubeR, "Discount");
                                        //items.iptal = f.RTD(SubeR, "iptal");
                                        //items.Zayi = f.RTD(SubeR, "Zayi");
                                        //items.GecenZaman = Convert.ToInt32(sure.TotalSeconds);
                                        //items.Debit = f.RTD(SubeR, "Debit");
                                        //items.OpenTable = f.RTD(SubeR, "OpenTable");
                                        items.Ciro = f.RTD(SubeR, "ToplamCiro");
                                        //items.Ciro = items.Cash + items.Credit + items.Ticket + items.Debit;
                                        Liste.Add(items);
                                    }
                                    f.SqlConnClose(true);
                                }
                                catch (Exception ex)
                                {
                                    #region EX                       
                                    //log metnini oluştur
                                    Singleton.WritingLogFile("DashboardListCRUD", ex.Message.ToString());
                                    string ErrorMessage = "Ciro Raporu Alınamadı.";
                                    items.Sube = SubeAdi;
                                    items.SubeId = SubeId;
                                    items.ErrorMessage = ErrorMessage;
                                    items.ErrorStatus = true;
                                    items.ErrorCode = "01";
                                    Liste.Add(items);

                                    #endregion EX    
                                }
                                #endregion GET DATA  
                            }
                        }
                        #endregion KULLANICININ ŞUBE YETKİ KONTROLU YAPILIYOR   
                    }
                }
                f.SqlConnClose();
            }
            catch (DataException ex)
            {
                Singleton.WritingLogFile("DashboardListCRUD", ex.Message.ToString());
            }
            return items;
        }
    }
}