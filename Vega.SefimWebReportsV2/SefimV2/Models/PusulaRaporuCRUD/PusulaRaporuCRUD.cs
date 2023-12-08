using SefimV2.Helper;
using SefimV2.ViewModels.PusulaRaporuViewModel;
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
    public class PusulaRaporuCRUD
    {
        public static List<PusulaRaporuViewModel> List(DateTime Date1, DateTime Date2, string subeid, string productGroup, string ID)
        {
            if (productGroup == "alt=\"expand/collapse\"")
            {
                productGroup = "NULL";
            }

            List<PusulaRaporuViewModel> Liste = new List<PusulaRaporuViewModel>();
            ModelFunctions ff = new ModelFunctions();
            DateTime startDate = DateTime.Now;
            #region GİRİŞ YAPAN KULLANICININ YETKİLİ OLDUĞU ŞUBELER LİSTELENİYOR
            UserViewModel model = UsersListCRUD.YetkiliSubesi(ID);
            #endregion

            #region VEGA DB database ismini çekmek için.
            string vega_Db = "";
            ff.SqlConnOpen();
            DataTable dataVegaDb = ff.DataTable("select* from VegaDbSettings ");
            var vegaDBList = dataVegaDb.AsEnumerable().ToList<DataRow>();
            foreach (var item in vegaDBList)
            {
                vega_Db = item["DBName"].ToString();
            }
            ff.SqlConnClose();
            #endregion VEGA DB database ismini çekmek için. 


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
                    string vPosSubeId = r["VPosSubeKodu"].ToString();
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

                    #region Faster Kasalar seçildiyse ona göre query oluşturuluyor. 
                    string FasterSubeIND = f.RTS(r, "FasterSubeID");
                    string QueryFasterSube = string.Empty;
                    if (FasterSubeIND != null)
                    {
                        QueryFasterSube = "  and  FSH.SUBEIND IN(" + FasterSubeIND + ") ";
                    }
                    #endregion Faster Kasalar seçildiyse ona göre query oluşturuluyor.

                    if (AppDbType == "1" || AppDbType == "2")
                    {
                        if (subeid != null && !subeid.Equals("0") && productGroup == null)// sube secili degilse ilk giris yapilan sql
                            Query = File.ReadAllText(HostingEnvironment.MapPath("/Sql/SqlNewSef/SqlPusulaRaporu/PusulaRaporuBirinciKirilim.sql"), System.Text.UTF8Encoding.Default);
                        if (subeid == null || subeid.Equals("0"))
                            Query = File.ReadAllText(HostingEnvironment.MapPath("/Sql/UrunGrup.sql"), System.Text.UTF8Encoding.Default);
                        if (subeid != null && !subeid.Equals("0") && productGroup != null)
                            Query = File.ReadAllText(HostingEnvironment.MapPath("/Sql/SqlNewSef/SqlPusulaRaporu/PusulaRaporuIkinciKirilim.sql"), System.Text.UTF8Encoding.Default);
                    }
                    else if (AppDbType == "5")
                    {
                        if (subeid != null && !subeid.Equals("0") && productGroup == null)// sube secili degilse ilk giris yapilan sql
                            Query = File.ReadAllText(HostingEnvironment.MapPath("/Sql/VPos/PusulaAdisyonRaporu/PusulaAdisyon.sql"), System.Text.UTF8Encoding.Default);
                        if (subeid == null || subeid.Equals("0"))
                            //Query = File.ReadAllText(HostingEnvironment.MapPath("/Sql/VPos/UrunGrubuSatisRaporu/UrunGrup.sql"), System.Text.UTF8Encoding.Default);
                            Query = File.ReadAllText(HostingEnvironment.MapPath("/Sql/VPos/PusulaAdisyonRaporu/PusulaAdisyonDetay.sql"), System.Text.UTF8Encoding.Default);
                        if (subeid != null && !subeid.Equals("0") && productGroup != null)
                            Query = File.ReadAllText(HostingEnvironment.MapPath("/Sql/VPos/PusulaAdisyonRaporu/PusulaAdisyonDetay.sql"), System.Text.UTF8Encoding.Default);
                    }

                    Query = Query.Replace("{SubeAdi}", SubeAdi);
                    Query = Query.Replace("{ProductId}", productGroup);
                    Query = Query.Replace("{TARIH1}", QueryTimeStart);
                    Query = Query.Replace("{TARIH2}", QueryTimeEnd);
                    Query = Query.Replace("{FIRMAIND}", FirmaId);
                    Query = Query.Replace("{SUBE2}", vPosSubeKodu);
                    Query = Query.Replace("{KASAKODU}", vPosKasaKodu);
                    Query = Query.Replace("{ID}", productGroup);

                    #endregion SQL QUARY  *(Sefim || (faster || Faster Offline || Faster Online))*

                    if (ID == "1")
                    {
                        #region GET DATA
                        try
                        {
                            string connString = "Provider=SQLOLEDB;Server=" + SubeIP + ";User Id=" + SqlName + ";Password=" + SqlPassword + ";Database=" + DBName + "";
                            try
                            {
                                DataTable UrunGrubuDt = new DataTable();
                                UrunGrubuDt = f.GetSubeDataWithQuery((f.NewConnectionString(SubeIP, DBName, SqlName, SqlPassword)), Query.ToString());

                                #region MyRegion

                                if (UrunGrubuDt.Rows.Count > 0)
                                {
                                    if (subeid.Equals("")) /*LİSTE*/
                                    {

                                        #region SEFIM ESKI/YENI (SUBE BAZLI)                                       
                                        PusulaRaporuViewModel items = new PusulaRaporuViewModel();
                                        items.Sube = SubeAdi;
                                        items.SubeId = Convert.ToInt32(SubeId);
                                        foreach (DataRow SubeR in UrunGrubuDt.Rows)
                                        {
                                            items.Id = f.RTI(SubeR, "Id");
                                            //items.MasaNo = f.RTS(SubeR, "TableNumber");
                                            //items.Tutar = f.RTD(SubeR, "Total");
                                            //items.IlkSiparisAlan = f.RTS(SubeR, "FirstUserName");
                                            //items.SonSiparisAlan = f.RTS(SubeR, "LastUserName");
                                            //items.IlkSiparisZamani = f.RTS(SubeR, "FirstOrderTime");
                                            //items.SonSiparisZamani = f.RTS(SubeR, "LastOrderTime");
                                            //items.Sure = "";
                                            //items.TahsilatYapn = f.RTS(SubeR, "Cashier");
                                            //items.Musteri = f.RTS(SubeR, "CustomerName");
                                        }
                                        Liste.Add(items);

                                        #endregion SEFIM ESKI/YENI 

                                    }
                                    else if (!subeid.Equals("") && productGroup == "False" || productGroup != "") /*DETAIL*/
                                    {
                                        #region 2.KIRILIM SEFIM ESKI/YENI 

                                        foreach (DataRow SubeR in UrunGrubuDt.Rows)
                                        {
                                            PusulaRaporuViewModel items = new PusulaRaporuViewModel();
                                            items.AppDbType = AppDbType;

                                            if (AppDbType == "5")
                                                items.VPosId = f.RTS(SubeR, "Id");
                                            else
                                                items.Id = f.RTI(SubeR, "Id");

                                            items.Sube = SubeAdi;
                                            items.MasaNo = f.RTS(SubeR, "TableNumber");
                                            items.Tutar = f.RTD(SubeR, "Total");
                                            items.IlkSiparisAlan = f.RTS(SubeR, "FirstUserName");
                                            items.SonSiparisAlan = f.RTS(SubeR, "LastUserName");
                                            items.IlkSiparisZamani = f.RTS(SubeR, "FirstOrderTime");
                                            items.SonSiparisZamani = f.RTS(SubeR, "LastOrderTime");
                                            items.Sure = "";
                                            items.TahsilatYapn = f.RTS(SubeR, "Cashier");
                                            items.Musteri = f.RTS(SubeR, "CustomerName");

                                            items.SiparisZamani = f.RTS(SubeR, "SiparisZamani");
                                            items.SiparisiAlan = f.RTS(SubeR, "UserName");
                                            items.UrunAdi = f.RTS(SubeR, "ProductName");
                                            items.Miktar = f.RTD(SubeR, "Quantity");
                                            items.Fiyat = f.RTD(SubeR, "Price");
                                            items.MasaTasima = f.RTS(SubeR, "Comment");//f.RTS(SubeR, "UserName");

                                            items.CreditPayment = f.RTD(SubeR, "CreditPayment");
                                            items.CashPayment = f.RTD(SubeR, "CashPayment");
                                            items.TicketPayment = f.RTD(SubeR, "TicketPayment");
                                            items.OnlinePayment = f.RTD(SubeR, "OnlinePayment");
                                            items.Discount = f.RTD(SubeR, "Discount");
                                            items.AcikHesap = f.RTD(SubeR, "Debit");
                                            items.OdemeZamani = f.RTS(SubeR, "PaymentTİme");
                                            items.CariAdi = f.RTS(SubeR, "CustomerName");

                                            Liste.Add(items);
                                        }

                                        #endregion 2.KIRILIM SEFIM ESKI/YENI                                         
                                    }
                                    else /*PRODUCTS_DETAIL*/
                                    {
                                        #region 3. KIRILIM SEFIM ESKI/YENI 

                                        foreach (DataRow SubeR in UrunGrubuDt.Rows)
                                        {
                                            PusulaRaporuViewModel items = new PusulaRaporuViewModel();
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
                                    PusulaRaporuViewModel items = new PusulaRaporuViewModel
                                    {
                                        Sube = SubeAdi + " (Data Yok)"
                                    };
                                    //items.SubeID = (SubeId);
                                    Liste.Add(items);
                                }

                                #endregion
                            }
                            catch (Exception ex)
                            {
                                Singleton.WritingLogFile2("PusulaRaporuCRUD", ex.ToString(), null, "");
                                throw new Exception(SubeAdi);
                            }
                        }
                        catch (Exception ex)
                        {
                            #region EX                         
                            Singleton.WritingLogFile2("PusulaRaporuCRUD", ex.ToString(), null, "");

                            PusulaRaporuViewModel items = new PusulaRaporuViewModel
                            {
                                Sube = ex.Message + " (Erişim Yok)"
                            };

                            Liste.Add(items);

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
                                #region GET DATA
                                try
                                {
                                    string connString = "Provider=SQLOLEDB;Server=" + SubeIP + ";User Id=" + SqlName + ";Password=" + SqlPassword + ";Database=" + DBName + "";
                                    try
                                    {
                                        DataTable UrunGrubuDt = new DataTable();
                                        UrunGrubuDt = f.GetSubeDataWithQuery((f.NewConnectionString(SubeIP, DBName, SqlName, SqlPassword)), Query.ToString());

                                        #region MyRegion

                                        if (UrunGrubuDt.Rows.Count > 0)
                                        {
                                            if (subeid.Equals("")) /*LİSTE*/
                                            {

                                                #region SEFIM ESKI/YENI (SUBE BAZLI)                                       
                                                PusulaRaporuViewModel items = new PusulaRaporuViewModel();
                                                items.Sube = SubeAdi;
                                                items.SubeId = Convert.ToInt32(SubeId);
                                                foreach (DataRow SubeR in UrunGrubuDt.Rows)
                                                {
                                                    items.Id = f.RTI(SubeR, "Id");
                                                    //items.MasaNo = f.RTS(SubeR, "TableNumber");
                                                    //items.Tutar = f.RTD(SubeR, "Total");
                                                    //items.IlkSiparisAlan = f.RTS(SubeR, "FirstUserName");
                                                    //items.SonSiparisAlan = f.RTS(SubeR, "LastUserName");
                                                    //items.IlkSiparisZamani = f.RTS(SubeR, "FirstOrderTime");
                                                    //items.SonSiparisZamani = f.RTS(SubeR, "LastOrderTime");
                                                    //items.Sure = "";
                                                    //items.TahsilatYapn = f.RTS(SubeR, "Cashier");
                                                    //items.Musteri = f.RTS(SubeR, "CustomerName");
                                                }
                                                Liste.Add(items);

                                                #endregion SEFIM ESKI/YENI 

                                            }
                                            else if (!subeid.Equals("") && productGroup == "False" || productGroup != "") /*DETAIL*/
                                            {
                                                #region 2.KIRILIM SEFIM ESKI/YENI 

                                                foreach (DataRow SubeR in UrunGrubuDt.Rows)
                                                {
                                                    PusulaRaporuViewModel items = new PusulaRaporuViewModel();
                                                    items.AppDbType = AppDbType;

                                                    if (AppDbType == "5")
                                                        items.VPosId = f.RTS(SubeR, "Id");
                                                    else
                                                        items.Id = f.RTI(SubeR, "Id");

                                                    items.Sube = SubeAdi;
                                                    items.MasaNo = f.RTS(SubeR, "TableNumber");
                                                    items.Tutar = f.RTD(SubeR, "Total");
                                                    items.IlkSiparisAlan = f.RTS(SubeR, "FirstUserName");
                                                    items.SonSiparisAlan = f.RTS(SubeR, "LastUserName");
                                                    items.IlkSiparisZamani = f.RTS(SubeR, "FirstOrderTime");
                                                    items.SonSiparisZamani = f.RTS(SubeR, "LastOrderTime");
                                                    items.Sure = "";
                                                    items.TahsilatYapn = f.RTS(SubeR, "Cashier");
                                                    items.Musteri = f.RTS(SubeR, "CustomerName");

                                                    items.SiparisZamani = f.RTS(SubeR, "SiparisZamani");
                                                    items.SiparisiAlan = f.RTS(SubeR, "UserName");
                                                    items.UrunAdi = f.RTS(SubeR, "ProductName");
                                                    items.Miktar = f.RTD(SubeR, "Quantity");
                                                    items.Fiyat = f.RTD(SubeR, "Price");
                                                    items.MasaTasima = f.RTS(SubeR, "Comment");//f.RTS(SubeR, "UserName");

                                                    items.CreditPayment = f.RTD(SubeR, "CreditPayment");
                                                    items.CashPayment = f.RTD(SubeR, "CashPayment");
                                                    items.TicketPayment = f.RTD(SubeR, "TicketPayment");
                                                    items.OnlinePayment = f.RTD(SubeR, "OnlinePayment");
                                                    items.Discount = f.RTD(SubeR, "Discount");
                                                    items.AcikHesap = f.RTD(SubeR, "Debit");
                                                    items.OdemeZamani = f.RTS(SubeR, "PaymentTİme");
                                                    items.CariAdi = f.RTS(SubeR, "CustomerName");

                                                    Liste.Add(items);
                                                }

                                                #endregion 2.KIRILIM SEFIM ESKI/YENI                                         
                                            }
                                            else /*PRODUCTS_DETAIL*/
                                            {
                                                #region 3. KIRILIM SEFIM ESKI/YENI 

                                                foreach (DataRow SubeR in UrunGrubuDt.Rows)
                                                {
                                                    PusulaRaporuViewModel items = new PusulaRaporuViewModel();
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
                                            PusulaRaporuViewModel items = new PusulaRaporuViewModel
                                            {
                                                Sube = SubeAdi + " (Data Yok)"
                                            };
                                            //items.SubeID = (SubeId);
                                            Liste.Add(items);
                                        }

                                        #endregion
                                    }
                                    catch (Exception ex)
                                    {
                                        Singleton.WritingLogFile2("PusulaRaporuCRUD", ex.ToString(), null, "");
                                        throw new Exception(SubeAdi);
                                    }
                                }
                                catch (Exception ex)
                                {
                                    #region EX                         
                                    Singleton.WritingLogFile2("PusulaRaporuCRUD", ex.ToString(), null, "");

                                    PusulaRaporuViewModel items = new PusulaRaporuViewModel
                                    {
                                        Sube = ex.Message + " (Erişim Yok)"
                                    };

                                    Liste.Add(items);

                                    #endregion EX
                                }
                                #endregion GET DATA
                            }
                        }
                        #endregion
                    }
                });
                #endregion PARALLEL FOREACH
            }
            catch (DataException ex)
            {
                Singleton.WritingLogFile2("PusulaRaporuCRUD", ex.ToString(), null, "");
            }
            return Liste;
        }
    }
}