using SefimV2.ViewModels.IskontoDetayRaporuViewModel;
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
    public class IskontoDetayRaporuCRUD
    {
        public static List<IskontoDetayRaporuViewModel> List(DateTime Date1, DateTime Date2, string subeid, string productGroup, string ID)
        {
            if (productGroup == "alt=\"expand/collapse\"")
            {
                productGroup = "NULL";
            }

            List<IskontoDetayRaporuViewModel> Liste = new List<IskontoDetayRaporuViewModel>();
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
                    #region SQL QUARY  *(Sefim || (faster || Faster Offline || Faster Online))*

                    ModelFunctions f = new ModelFunctions();
                    string Query = "";
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

                    if (AppDbType == "1" || AppDbType == "2")
                    {
                        if (subeid != null && !subeid.Equals("0"))// sube secili degilse ilk giris yapilan sql
                            Query = File.ReadAllText(HostingEnvironment.MapPath("/Sql/SqlNewSef/IskontDetayRaporu/IskontDetayRaporu2.sql"), System.Text.UTF8Encoding.Default);
                        if (subeid == null || subeid.Equals("0") || subeid == "")
                            Query = File.ReadAllText(HostingEnvironment.MapPath("/Sql/SqlNewSef/IskontDetayRaporu/IskontDetayRaporu1.sql"), System.Text.UTF8Encoding.Default);
                    }
                    else if (AppDbType == "5")
                    {
                        if (subeid != null && !subeid.Equals("0"))// sube secili degilse ilk giris yapilan sql
                            Query = File.ReadAllText(HostingEnvironment.MapPath("/Sql/VPos/IskontDetayRaporu/IskontDetayRaporu2.sql"), System.Text.UTF8Encoding.Default);
                        if (subeid == null || subeid.Equals("0") || subeid == "")
                            Query = File.ReadAllText(HostingEnvironment.MapPath("/Sql/VPos/IskontDetayRaporu/IskontDetayRaporu1.sql"), System.Text.UTF8Encoding.Default);
                    }

                    Query = Query.Replace("{SubeAdi}", SubeAdi);
                    Query = Query.Replace("{ProductId}", productGroup);
                    Query = Query.Replace("{TARIH1}", QueryTimeStart);
                    Query = Query.Replace("{TARIH2}", QueryTimeEnd);
                    Query = Query.Replace("{SUBE2}", vPosSubeKodu);
                    Query = Query.Replace("{KASAKODU}", vPosKasaKodu);



                    #endregion SQL QUARY  *(Sefim || (faster || Faster Offline || Faster Online))*

                    if (ID == "1")
                    {
                        #region GET DATA
                        try
                        {
                            string connString = "Provider=SQLOLEDB;Server=" + SubeIP + ";User Id=" + SqlName + ";Password=" + SqlPassword + ";Database=" + DBName + "";
                            try
                            {
                                DataTable subeList = new DataTable();
                                subeList = f.GetSubeDataWithQuery((f.NewConnectionString(SubeIP, DBName, SqlName, SqlPassword)), Query.ToString());

                                #region MyRegion

                                if (subeList.Rows.Count > 0)
                                {
                                    if (subeid.Equals("")) /*LİSTE*/
                                    {

                                        #region SEFIM ESKI/YENI (SUBE BAZLI)                                       
                                        IskontoDetayRaporuViewModel items = new IskontoDetayRaporuViewModel();
                                        items.Sube = SubeAdi;
                                        items.SubeId = Convert.ToInt32(SubeId);
                                        foreach (DataRow SubeR in subeList.Rows)
                                        {
                                            items.Id = f.RTI(SubeR, "Id");
                                            items.DiscountTotal = f.RTD(SubeR, "DiscountTotal");
                                        }
                                        Liste.Add(items);

                                        #endregion SEFIM ESKI/YENI 

                                    }
                                    else if (!subeid.Equals("") && productGroup == "False" || productGroup != "") /*DETAIL*/
                                    {
                                        #region 2.KIRILIM SEFIM ESKI/YENI 

                                        foreach (DataRow SubeR in subeList.Rows)
                                        {
                                            //CashPayment,CreditPayment,TicketPayment
                                            IskontoDetayRaporuViewModel items = new IskontoDetayRaporuViewModel();
                                            items.Sube = SubeAdi;
                                            items.SubeId = Convert.ToInt32(SubeId);
                                            items.Discount = f.RTD(SubeR, "Discount");   //Convert.ToDecimal(SubeCiroDt.Rows[0]["Price"]); //f.RTD(SubeR, "Cash");
                                            items.TotalPayment = f.RTD(SubeR, "CashPayment") + f.RTD(SubeR, "CreditPayment") + f.RTD(SubeR, "TicketPayment");
                                            items.MasaNo = f.RTS(SubeR, "TableNo"); // PriceSubeCiroDt.Rows[0]["TableNumber"].ToString();  //f.RTD(SubeR, "Credit");                                          
                                            items.Id = f.RTI(SubeR, "Id"); // Convert.ToInt32(SubeCiroDt.Rows[0]["Id"]); //f.RTD(SubeR, "ikram");                                            
                                            items.IskontoAciklamasi = f.RTS(SubeR, "DiscountReason"); //(SubeCiroDt.Rows[0]["DeletingUserName"]).ToString();
                                            items.IskontoTarihi = f.RTS(SubeR, "PaymentTime"); //(SubeCiroDt.Rows[0]["DeleteReason"]).ToString();
                                            items.Personel = f.RTS(SubeR, "ReceivedByUserName"); //(SubeCiroDt.Rows[0]["DeleteDetails"]).ToString();
                                            items.ToplamSatis = items.TotalPayment + f.RTD(SubeR, "Discount");
                                            Liste.Add(items);
                                        }

                                        var totalPaymentSum = Liste.Sum(x => x.TotalPayment);
                                        Liste.FirstOrDefault().Ciro = totalPaymentSum;

                                        #endregion 2.KIRILIM SEFIM ESKI/YENI                                         
                                    }
                                    else /*PRODUCTS_DETAIL*/
                                    {
                                        #region 3. KIRILIM SEFIM ESKI/YENI 

                                        foreach (DataRow SubeR in subeList.Rows)
                                        {
                                            IskontoDetayRaporuViewModel items = new IskontoDetayRaporuViewModel();
                                            items.Sube = f.RTS(SubeR, "Sube");
                                            //items.SubeID = (SubeId);
                                            //items.Miktar = f.RTD(SubeR, "MIKTAR");
                                            //items.ProductName = f.RTS(SubeR, "ProductName");
                                            //items.Debit = f.RTD(SubeR, "TUTAR");
                                            Liste.Add(items);
                                        }
                                        #endregion 3. KIRILIM SEFIM ESKI/YENI                                         
                                    }
                                }
                                else
                                {
                                    IskontoDetayRaporuViewModel items = new IskontoDetayRaporuViewModel();
                                    items.Sube = SubeAdi + " (Data Yok)";
                                    //items.SubeID = (SubeId);
                                    Liste.Add(items);
                                }

                                #endregion
                            }
                            catch (Exception) { throw new Exception(SubeAdi); }
                        }
                        catch (System.Exception ex)
                        {
                            IskontoDetayRaporuViewModel items = new IskontoDetayRaporuViewModel();
                            items.Sube = SubeAdi + " (Data Yok)";
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
                                    string connString = "Provider=SQLOLEDB;Server=" + SubeIP + ";User Id=" + SqlName + ";Password=" + SqlPassword + ";Database=" + DBName + "";
                                    try
                                    {
                                        DataTable subeList = new DataTable();
                                        subeList = f.GetSubeDataWithQuery((f.NewConnectionString(SubeIP, DBName, SqlName, SqlPassword)), Query.ToString());

                                        #region MyRegion

                                        if (subeList.Rows.Count > 0)
                                        {
                                            if (subeid.Equals("")) /*LİSTE*/
                                            {

                                                #region SEFIM ESKI/YENI (SUBE BAZLI)                                       
                                                IskontoDetayRaporuViewModel items = new IskontoDetayRaporuViewModel();
                                                items.Sube = SubeAdi;
                                                items.SubeId = Convert.ToInt32(SubeId);
                                                foreach (DataRow SubeR in subeList.Rows)
                                                {
                                                    items.Id = f.RTI(SubeR, "Id");
                                                    items.DiscountTotal = f.RTD(SubeR, "DiscountTotal");
                                                }
                                                Liste.Add(items);

                                                #endregion SEFIM ESKI/YENI 

                                            }
                                            else if (!subeid.Equals("") && productGroup == "False" || productGroup != "") /*DETAIL*/
                                            {
                                                #region 2.KIRILIM SEFIM ESKI/YENI 

                                                foreach (DataRow SubeR in subeList.Rows)
                                                {
                                                    //CashPayment,CreditPayment,TicketPayment
                                                    IskontoDetayRaporuViewModel items = new IskontoDetayRaporuViewModel();
                                                    items.Sube = SubeAdi;
                                                    items.SubeId = Convert.ToInt32(SubeId);
                                                    items.Discount = f.RTD(SubeR, "Discount");   //Convert.ToDecimal(SubeCiroDt.Rows[0]["Price"]); //f.RTD(SubeR, "Cash");
                                                    items.TotalPayment = f.RTD(SubeR, "CashPayment") + f.RTD(SubeR, "CreditPayment") + f.RTD(SubeR, "TicketPayment");
                                                    items.MasaNo = f.RTS(SubeR, "TableNo"); // PriceSubeCiroDt.Rows[0]["TableNumber"].ToString();  //f.RTD(SubeR, "Credit");                                          
                                                    items.Id = f.RTI(SubeR, "Id"); // Convert.ToInt32(SubeCiroDt.Rows[0]["Id"]); //f.RTD(SubeR, "ikram");                                            
                                                    items.IskontoAciklamasi = f.RTS(SubeR, "DiscountReason"); //(SubeCiroDt.Rows[0]["DeletingUserName"]).ToString();
                                                    items.IskontoTarihi = f.RTS(SubeR, "PaymentTime"); //(SubeCiroDt.Rows[0]["DeleteReason"]).ToString();
                                                    items.Personel = f.RTS(SubeR, "ReceivedByUserName"); //(SubeCiroDt.Rows[0]["DeleteDetails"]).ToString();
                                                    items.ToplamSatis = items.TotalPayment + f.RTD(SubeR, "Discount");
                                                    Liste.Add(items);
                                                }

                                                var totalPaymentSum = Liste.Sum(x => x.TotalPayment);
                                                Liste.FirstOrDefault().Ciro = totalPaymentSum;

                                                #endregion 2.KIRILIM SEFIM ESKI/YENI                                         
                                            }
                                            else /*PRODUCTS_DETAIL*/
                                            {
                                                #region 3. KIRILIM SEFIM ESKI/YENI 

                                                foreach (DataRow SubeR in subeList.Rows)
                                                {
                                                    IskontoDetayRaporuViewModel items = new IskontoDetayRaporuViewModel();
                                                    items.Sube = f.RTS(SubeR, "Sube");
                                                    //items.SubeID = (SubeId);
                                                    //items.Miktar = f.RTD(SubeR, "MIKTAR");
                                                    //items.ProductName = f.RTS(SubeR, "ProductName");
                                                    //items.Debit = f.RTD(SubeR, "TUTAR");
                                                    Liste.Add(items);
                                                }
                                                #endregion 3. KIRILIM SEFIM ESKI/YENI                                         
                                            }
                                        }
                                        else
                                        {
                                            IskontoDetayRaporuViewModel items = new IskontoDetayRaporuViewModel();
                                            items.Sube = SubeAdi + " (Data Yok)";
                                            //items.SubeID = (SubeId);
                                            Liste.Add(items);
                                        }

                                        #endregion
                                    }
                                    catch (Exception) { throw new Exception(SubeAdi); }
                                }
                                catch (System.Exception ex)
                                {
                                    IskontoDetayRaporuViewModel items = new IskontoDetayRaporuViewModel();
                                    items.Sube = SubeAdi + " (Data Yok)";
                                    Liste.Add(items);
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
    }
}