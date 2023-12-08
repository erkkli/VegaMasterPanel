using SefimV2.ViewModels.IptalDetayRaporuViewModel;
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
    public class IptalDetayRaporuCRUD
    {
        public static List<IptalDetayRaporuViewModel> List(DateTime Date1, DateTime Date2, string subeid, string productGroup, string ID)
        {
            if (productGroup == "alt=\"expand/collapse\"")
            {
                productGroup = "NULL";
            }

            List<IptalDetayRaporuViewModel> Liste = new List<IptalDetayRaporuViewModel>();
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

                var locked = new Object();
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
                        if (subeid == "" && !subeid.Equals("0"))// sube secili degilse ilk giris yapilan sql
                            Query = File.ReadAllText(HostingEnvironment.MapPath("/Sql/SqlNewSef/IptalDetayRaporu/IptalDetayRaporu1.sql"), System.Text.UTF8Encoding.Default);
                        if (!subeid.Equals("0") && subeid != "")
                            Query = File.ReadAllText(HostingEnvironment.MapPath("/Sql/SqlNewSef/IptalDetayRaporu/IptalDetayRaporu2.sql"), System.Text.UTF8Encoding.Default);
                    }
                    else if (AppDbType == "5")
                    {
                        if (subeid == "" && !subeid.Equals("0"))// sube secili degilse ilk giris yapilan sql
                            Query = File.ReadAllText(HostingEnvironment.MapPath("/Sql/VPos/IptalDetayRaporu/IptalDetayRaporu1.sql"), System.Text.UTF8Encoding.Default);
                        if (!subeid.Equals("0") && subeid != "")
                            Query = File.ReadAllText(HostingEnvironment.MapPath("/Sql/VPos/IptalDetayRaporu/IptalDetayRaporu2.sql"), System.Text.UTF8Encoding.Default);
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
                                        IptalDetayRaporuViewModel items = new IptalDetayRaporuViewModel();
                                        items.Sube = SubeAdi;
                                        items.SubeId = Convert.ToInt32(SubeId);
                                        foreach (DataRow SubeR in subeList.Rows)
                                        {
                                            items.Id = f.RTI(SubeR, "Id");
                                            items.ToplamIptalTutari = f.RTD(SubeR, "ToplamIptalTutari");
                                            items.ToplamIptalMiktari = f.RTD(SubeR, "ToplamIptalMiktari");
                                        }
                                        lock (locked)
                                        {
                                            Liste.Add(items);
                                        }
                                        #endregion SEFIM ESKI/YENI 

                                    }
                                    else if (!subeid.Equals("") && productGroup == "False" || productGroup != "") /*DETAIL*/
                                    {
                                        #region 2.KIRILIM SEFIM ESKI/YENI 

                                        foreach (DataRow SubeR in subeList.Rows)
                                        {
                                            IptalDetayRaporuViewModel items = new IptalDetayRaporuViewModel();
                                            items.Sube = SubeAdi;
                                            items.SubeId = Convert.ToInt32(SubeId);
                                            items.Fiyat = f.RTD(SubeR, "Price");   //Convert.ToDecimal(SubeCiroDt.Rows[0]["Price"]); //f.RTD(SubeR, "Cash");
                                            items.MasaNo = f.RTS(SubeR, "TableNumber"); // PriceSubeCiroDt.Rows[0]["TableNumber"].ToString();  //f.RTD(SubeR, "Credit");
                                            items.BasimAdedi = f.RTS(SubeR, "printcount"); //SubeCiroDt.Rows[0]["Printed"].ToString();
                                            items.Id = f.RTI(SubeR, "Id"); // Convert.ToInt32(SubeCiroDt.Rows[0]["Id"]); //f.RTD(SubeR, "ikram");
                                            items.Miktar = f.RTD(SubeR, "Quantity");  //f.RTD(SubeR, "Quantity");  // f.RTD(SubeCiroDt.Rows[0]["Quantity"]);
                                            items.SilenKullanici = f.RTS(SubeR, "DeletingUserName"); //(SubeCiroDt.Rows[0]["DeletingUserName"]).ToString();
                                            items.SilmeAciklamasi = f.RTS(SubeR, "DeleteReason"); //(SubeCiroDt.Rows[0]["DeleteReason"]).ToString();
                                            items.SilmeDetay = f.RTS(SubeR, "DeleteDetails"); //(SubeCiroDt.Rows[0]["DeleteDetails"]).ToString();
                                            items.SilmeZamani = f.RTS(SubeR, "DeletingTime"); //(SubeCiroDt.Rows[0]["DeletingTime"]).ToString();
                                            items.SiparisZamani = f.RTS(SubeR, "Date"); //(SubeCiroDt.Rows[0]["DeletingTime"]).ToString();
                                            items.SiparisAlanKullanici = f.RTS(SubeR, "UserName"); //(SubeCiroDt.Rows[0]["UserName"]).ToString();
                                            items.UrunAdi = f.RTS(SubeR, "ProductName"); //(SubeCiroDt.Rows[0]["ProductName"]).ToString();

                                            lock (locked)
                                            {
                                                Liste.Add(items);
                                            }
                                        }

                                        #endregion 2.KIRILIM SEFIM ESKI/YENI                                         
                                    }
                                    else /*PRODUCTS_DETAIL*/
                                    {
                                        #region 3. KIRILIM SEFIM ESKI/YENI 

                                        foreach (DataRow SubeR in subeList.Rows)
                                        {
                                            IptalDetayRaporuViewModel items = new IptalDetayRaporuViewModel();
                                            items.Sube = f.RTS(SubeR, "Sube");
                                            //items.SubeID = (SubeId);
                                            //items.Miktar = f.RTD(SubeR, "MIKTAR");
                                            //items.ProductName = f.RTS(SubeR, "ProductName");
                                            //items.Debit = f.RTD(SubeR, "TUTAR");
                                            lock (locked)
                                            {
                                                Liste.Add(items);
                                            }
                                        }
                                        #endregion 3. KIRILIM SEFIM ESKI/YENI                                         
                                    }
                                }
                                else
                                {
                                    IptalDetayRaporuViewModel items = new IptalDetayRaporuViewModel();
                                    items.Sube = SubeAdi + " (Data Yok)";
                                    //items.SubeID = (SubeId);

                                    lock (locked)
                                    {
                                        Liste.Add(items);
                                    }
                                }

                                #endregion
                            }
                            catch (Exception) { throw new Exception(SubeAdi); }
                        }
                        catch (System.Exception ex)
                        {
                            IptalDetayRaporuViewModel items = new IptalDetayRaporuViewModel();
                            items.Sube = SubeAdi + " (Data Yok)";
                            //items.SubeID = (SubeId);
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
                                                IptalDetayRaporuViewModel items = new IptalDetayRaporuViewModel();
                                                items.Sube = SubeAdi;
                                                items.SubeId = Convert.ToInt32(SubeId);
                                                foreach (DataRow SubeR in subeList.Rows)
                                                {
                                                    items.Id = f.RTI(SubeR, "Id");
                                                    items.ToplamIptalTutari = f.RTD(SubeR, "ToplamIptalTutari");
                                                    items.ToplamIptalMiktari = f.RTD(SubeR, "ToplamIptalMiktari");
                                                }
                                                Liste.Add(items);

                                                #endregion SEFIM ESKI/YENI 

                                            }
                                            else if (!subeid.Equals("") && productGroup == "False" || productGroup != "") /*DETAIL*/
                                            {
                                                #region 2.KIRILIM SEFIM ESKI/YENI 

                                                foreach (DataRow SubeR in subeList.Rows)
                                                {
                                                    IptalDetayRaporuViewModel items = new IptalDetayRaporuViewModel();
                                                    items.Sube = SubeAdi;
                                                    items.SubeId = Convert.ToInt32(SubeId);
                                                    items.Fiyat = f.RTD(SubeR, "Price");   //Convert.ToDecimal(SubeCiroDt.Rows[0]["Price"]); //f.RTD(SubeR, "Cash");
                                                    items.MasaNo = f.RTS(SubeR, "TableNumber"); // PriceSubeCiroDt.Rows[0]["TableNumber"].ToString();  //f.RTD(SubeR, "Credit");
                                                    items.BasimAdedi = f.RTS(SubeR, "printcount"); //SubeCiroDt.Rows[0]["Printed"].ToString();
                                                    items.Id = f.RTI(SubeR, "Id"); // Convert.ToInt32(SubeCiroDt.Rows[0]["Id"]); //f.RTD(SubeR, "ikram");
                                                    items.Miktar = f.RTD(SubeR, "Quantity");  //f.RTD(SubeR, "Quantity");  // f.RTD(SubeCiroDt.Rows[0]["Quantity"]);
                                                    items.SilenKullanici = f.RTS(SubeR, "DeletingUserName"); //(SubeCiroDt.Rows[0]["DeletingUserName"]).ToString();
                                                    items.SilmeAciklamasi = f.RTS(SubeR, "DeleteReason"); //(SubeCiroDt.Rows[0]["DeleteReason"]).ToString();
                                                    items.SilmeDetay = f.RTS(SubeR, "DeleteDetails"); //(SubeCiroDt.Rows[0]["DeleteDetails"]).ToString();
                                                    items.SilmeZamani = f.RTS(SubeR, "DeletingTime"); //(SubeCiroDt.Rows[0]["DeletingTime"]).ToString();
                                                    items.SiparisZamani = f.RTS(SubeR, "Date"); //(SubeCiroDt.Rows[0]["DeletingTime"]).ToString();
                                                    items.SiparisAlanKullanici = f.RTS(SubeR, "UserName"); //(SubeCiroDt.Rows[0]["UserName"]).ToString();
                                                    items.UrunAdi = f.RTS(SubeR, "ProductName"); //(SubeCiroDt.Rows[0]["ProductName"]).ToString();
                                                    Liste.Add(items);

                                                }

                                                #endregion 2.KIRILIM SEFIM ESKI/YENI                                         
                                            }
                                            else /*PRODUCTS_DETAIL*/
                                            {
                                                #region 3. KIRILIM SEFIM ESKI/YENI 

                                                foreach (DataRow SubeR in subeList.Rows)
                                                {
                                                    IptalDetayRaporuViewModel items = new IptalDetayRaporuViewModel();
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
                                            IptalDetayRaporuViewModel items = new IptalDetayRaporuViewModel();
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
                                    IptalDetayRaporuViewModel items = new IptalDetayRaporuViewModel();
                                    items.Sube = SubeAdi + " (Data Yok)";
                                    //items.SubeID = (SubeId);
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