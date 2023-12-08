using SefimV2.Helper;
using SefimV2.ViewModels.User;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Web.Hosting;

namespace SefimV2.Models
{
    public class KuryeCiroCRUD
    {
        public static List<KuryeCiro> List(DateTime Date1, DateTime Date2, string subeid, string ID, bool PersonelToplamlariGetir = false)
        {
            List<KuryeCiro> Liste = new List<KuryeCiro>();
            ModelFunctions f = new ModelFunctions();
            DateTime startDate = DateTime.Now;
            #region GİRİŞ YAPAN KULLANICININ YETKİLİ OLDUĞU ŞUBELER LİSTELENİYOR
            UserViewModel model = UsersListCRUD.YetkiliSubesi(ID);
            #endregion
            try
            {
                f.SqlConnOpen();
                string filter = "Where Status=1";
                if (subeid != null && !subeid.Equals("0") && !subeid.Equals(""))
                    filter += " and Id=" + subeid;

                DataTable dt = f.DataTable("select * from SubeSettings " + filter);
                foreach (DataRow r in dt.Rows)
                {
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
                  
                    string Query = string.Empty;
                    if (AppDbType == "1")// 1 = yeni şefim, 2 =eski Şefim, 3 = Faster
                    {
                        if (!PersonelToplamlariGetir)
                        {
                            Query = File.ReadAllText(HostingEnvironment.MapPath("/Sql/SqlNewSef/KuryeCiroNewSefim.sql"), System.Text.UTF8Encoding.Default);
                        }
                        else
                        {
                            Query = File.ReadAllText(HostingEnvironment.MapPath("/Sql/SqlNewSef/KuryeCiroNewSefimPersonelTotal.sql"), System.Text.UTF8Encoding.Default);
                        }
                    }
                    else if (AppDbType == "2")
                    {
                        Query = File.ReadAllText(HostingEnvironment.MapPath("/Sql/KuryeCiro.sql"), System.Text.UTF8Encoding.Default);
                    }
                    else if (AppDbType == "3")
                    {
                        //Query = File.ReadAllText(HostingEnvironment.MapPath("/Sql/SqlFaster/SubeToplamCiroFASTER.sql"), System.Text.Encoding.UTF8);
                    }
                    else if (AppDbType == "5")
                    {
                        if (!PersonelToplamlariGetir)
                        {
                            //Query = File.ReadAllText(HostingEnvironment.MapPath("/Sql/VPos/PaketServisRaporu/PaketServis.sql"), System.Text.UTF8Encoding.Default);
                            Query = File.ReadAllText(HostingEnvironment.MapPath("/Sql/VPos/PaketServisRaporu/PaketServisDetay.sql"), System.Text.UTF8Encoding.Default);
                        }
                        else
                        {
                            Query = File.ReadAllText(HostingEnvironment.MapPath("/Sql/VPos/PaketServisRaporu/PaketServisDetay.sql"), System.Text.UTF8Encoding.Default);
                        }
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
                            DataTable kuryeCiroDT = new DataTable();
                            kuryeCiroDT = f.GetSubeDataWithQuery((f.NewConnectionString(SubeIP, DBName, SqlName, SqlPassword)), Query.ToString());

                            foreach (DataRow SubeR in kuryeCiroDT.Rows)
                            {
                                KuryeCiro items = new KuryeCiro
                                {
                                    Sube = SubeAdi,
                                    SubeID = Convert.ToInt32(SubeId),
                                    //items.Adet = f.RTI(SubeR, "ADET");
                                    //items.Debit = f.RTD(SubeR, "TUTAR");
                                    //items.PhoneOrderDebit = f.RTI(SubeR, "PhoneOrderDebit");
                                    PersonelAdi = f.RTS(SubeR, "Deliverer"),
                                    OrderCount = f.RTI(SubeR, "OrderCount"),
                                    Total = f.RTD(SubeR, "Total"),
                                    Debit = f.RTD(SubeR, "Debit"),
                                    CashPayment = f.RTD(SubeR, "CashPayment"),
                                    CreditPayment = f.RTD(SubeR, "CreditPayment"),
                                    TicketPayment = f.RTD(SubeR, "TicketPayment"),
                                    Discount = f.RTD(SubeR, "Discount"),
                                    CollectedTotal = f.RTD(SubeR, "CollectedTotal"),
                                    Balance = f.RTD(SubeR, "Balance"),
                                    SiparisSaati = f.RTS(SubeR, "CreationTime"),
                                    OrderNo = f.RTS(SubeR, "OrderNo")
                                };

                                Liste.Add(items);
                            }
                        }
                        catch (Exception ex)
                        {
                            //Log
                            Singleton.WritingLogFile2("KuryeCiroCRUD", ex.ToString(), null, ex.StackTrace);

                            KuryeCiro items = new KuryeCiro
                            {
                                Sube = SubeAdi + " (Erişim veya Data Yok) ",
                                SubeID = Convert.ToInt32(SubeId),
                                //items.CustomerName = cu;
                                //items.Debit = SubeId;
                                ErrorMessage = "Kasa Raporu Alınamadı.",
                                ErrorStatus = true,
                                ErrorCode = "01"
                            };
                            Liste.Add(items);
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
                                    DataTable kuryeCiroDT = new DataTable();
                                    kuryeCiroDT = f.GetSubeDataWithQuery((f.NewConnectionString(SubeIP, DBName, SqlName, SqlPassword)), Query.ToString());

                                    foreach (DataRow SubeR in kuryeCiroDT.Rows)
                                    {
                                        KuryeCiro items = new KuryeCiro
                                        {
                                            Sube = SubeAdi,
                                            SubeID = Convert.ToInt32(SubeId),
                                            //items.Adet = f.RTI(SubeR, "ADET");
                                            //items.Debit = f.RTD(SubeR, "TUTAR");
                                            //items.PhoneOrderDebit = f.RTI(SubeR, "PhoneOrderDebit");
                                            PersonelAdi = f.RTS(SubeR, "Deliverer"),
                                            OrderCount = f.RTI(SubeR, "OrderCount"),
                                            Total = f.RTD(SubeR, "Total"),
                                            Debit = f.RTD(SubeR, "Debit"),
                                            CashPayment = f.RTD(SubeR, "CashPayment"),
                                            CreditPayment = f.RTD(SubeR, "CreditPayment"),
                                            TicketPayment = f.RTD(SubeR, "TicketPayment"),
                                            Discount = f.RTD(SubeR, "Discount"),
                                            CollectedTotal = f.RTD(SubeR, "CollectedTotal"),
                                            Balance = f.RTD(SubeR, "Balance"),
                                            SiparisSaati = f.RTS(SubeR, "CreationTime"),
                                            OrderNo = f.RTS(SubeR, "OrderNo")
                                        };

                                        Liste.Add(items);
                                    }
                                }
                                catch (Exception ex)
                                {
                                    //Log
                                    Singleton.WritingLogFile2("KuryeCiroCRUD", ex.ToString(), null, ex.StackTrace);

                                    KuryeCiro items = new KuryeCiro
                                    {
                                        Sube = SubeAdi + " (Erişim veya Data Yok) ",
                                        SubeID = Convert.ToInt32(SubeId),
                                        //items.CustomerName = cu;
                                        //items.Debit = SubeId;
                                        ErrorMessage = "Kasa Raporu Alınamadı.",
                                        ErrorStatus = true,
                                        ErrorCode = "01"
                                    };
                                    Liste.Add(items);
                                }
                                #endregion GET DATA 
                            }
                        }
                        #endregion
                    }
                }
                f.SqlConnClose();
            }
            catch (DataException ex) { }

            return Liste;
        }
    }
}