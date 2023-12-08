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
    public class UrunGrubuCRUD
    {
        public static List<UrunGrubu> List(DateTime Date1, DateTime Date2, string subeid, string productGroup, string ID)
        {
            List<UrunGrubu> Liste = new List<UrunGrubu>();
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
                    filter += " and ID=" + subeid_;
                DataTable dt = ff.DataTable("select * from SubeSettings " + filter);
                ff.SqlConnClose();
                #endregion SUBSTATION LIST

                #region PARALLEL FOREACH
                var dtList = dt.AsEnumerable().ToList<DataRow>();
                Parallel.ForEach(dtList, r =>
                {
                    ModelFunctions f = new ModelFunctions();
                    string Query = "";
                    string SubeId = r["ID"].ToString();
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

                    if (AppDbType == "1" || AppDbType == "2")
                    {
                        if (subeid != null && !subeid.Equals("0") && productGroup == null)// sube secili degilse ilk giris yapilan sql
                            Query = File.ReadAllText(HostingEnvironment.MapPath("/Sql/UrunGrupUrunKategori.sql"), System.Text.UTF8Encoding.Default);
                        if (subeid == null || subeid.Equals("0"))
                            Query = File.ReadAllText(HostingEnvironment.MapPath("/Sql/UrunGrup.sql"), System.Text.UTF8Encoding.Default);
                        if (subeid != null && !subeid.Equals("0") && productGroup != null)
                            Query = File.ReadAllText(HostingEnvironment.MapPath("/Sql/UrunGrupDetay.sql"), System.Text.UTF8Encoding.Default);
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
                        #endregion #region Kırılımlarda Ana Şube Altındaki IND id alıyorum 

                        if (AppDbTypeStatus == "True")
                        {
                            if (subeid != null && !subeid.Equals("0") && productGroup == null)// sube secili degilse ilk giris yapilan sql

                                #region FASTER ONLINE QUARY
                                Query =
                                         " declare @Sube nvarchar(100) = '{SubeAdi}';" +
                                         " declare @par1 nvarchar(20) = '{TARIH1}';" +
                                         " declare @par2 nvarchar(20) = '{TARIH2}';" +
                                         " ( SELECT (SELECT SUBEADI FROM " + FirmaId_SUBE + " WHERE IND=FSB.SUBEIND) AS Sube1," +
                                         " ( SELECT KASAADI FROM " + FirmaId_KASA + " WHERE IND=FSB.KASAIND) AS Kasa," +
                                         " ( SELECT IND FROM TBLFASTERKASALAR WHERE KASANO=FSB.KASAIND) AS ID," +
                                         " SUM(FSH.MIKTAR) AS MIKTAR, SUM((((MIKTAR*SATISFIYATI) * (100-ISNULL(FSH.ISK1,0))/100) * (100-ISNULL(FSH.ISK2,0))/100) * (100-ISNULL(FSB.ALTISKORAN,0))/100)  AS TUTAR, STK.KOD1 AS ProductName" +
                                         " FROM TBLFASTERSATISHAREKET AS FSH " +
                                         " LEFT JOIN TBLFASTERSATISBASLIK AS FSB ON FSH.BASLIKIND=FSB.BASLIKIND AND FSH.SUBEIND=FSB.SUBEIND AND FSH.KASAIND=FSB.KASAIND " +
                                         " LEFT JOIN F0" + FirmaId + "TBLSTOKLAR AS STK ON FSH.STOKIND=STK.IND WHERE FSH.ISLEMTARIHI>=@par1 AND FSH.ISLEMTARIHI<=@par2 AND ISNULL(FSB.IADE,0)=0 GROUP BY FSB.SUBEIND,FSB.KASAIND,STK.KOD1)";
                            #endregion FASTER ONLINE QUARY

                            if (subeid == null || subeid.Equals("0"))
                                Query = File.ReadAllText(HostingEnvironment.MapPath("/Sql/SqlFaster/UrunGrup/UrunGrupFasterOFLINE.sql"), System.Text.UTF8Encoding.Default);
                            if (subeid != null && !subeid.Equals("0") && productGroup != null)
                                #region FASTER ONLINE 2.KIRILIM
                                Query =
                                         " declare @Sube nvarchar(100) = '{SubeAdi}';" +
                                         " declare @par1 nvarchar(20) = '{TARIH1}';" +
                                         " declare @par2 nvarchar(20) = '{TARIH2}';" +
                                         " ( SELECT (SELECT SUBEADI FROM F0" + FirmaId + "TBLKRDSUBELER WHERE IND=FSB.SUBEIND) AS Sube1," +
                                         " ( SELECT KASAADI FROM F0" + FirmaId + "TBLKRDKASALAR WHERE IND=FSB.KASAIND) AS Kasa," +
                                         " ( SELECT IND FROM TBLFASTERKASALAR WHERE KASANO=FSB.KASAIND) AS ID," +
                                         " SUM(FSH.MIKTAR) AS MIKTAR, SUM((((MIKTAR*SATISFIYATI) * (100-ISNULL(FSH.ISK1,0))/100) * (100-ISNULL(FSH.ISK2,0))/100) * (100-ISNULL(FSB.ALTISKORAN,0))/100)  AS TUTAR, STK.MALINCINSI AS ProductName " +
                                         " FROM TBLFASTERSATISHAREKET AS FSH " +
                                         " LEFT JOIN TBLFASTERSATISBASLIK AS FSB ON FSH.BASLIKIND=FSB.BASLIKIND AND FSH.SUBEIND=FSB.SUBEIND AND FSH.KASAIND=FSB.KASAIND  " +
                                         " LEFT JOIN F0" + FirmaId + "TBLSTOKLAR AS STK ON FSH.STOKIND=STK.IND WHERE FSH.ISLEMTARIHI>=@par1 AND FSH.ISLEMTARIHI<=@par2 AND ISNULL(FSB.IADE,0)=0" +
                                         " GROUP BY FSB.SUBEIND,FSB.KASAIND,STK.MALINCINSI)";
                            #endregion FASTER ONLINE 2.KIRILIM
                        }
                        else
                        {
                            if (subeid != null && !subeid.Equals("0") && productGroup == null)// sube secili degilse ilk giris yapilan sql
                                Query = File.ReadAllText(HostingEnvironment.MapPath("/Sql/SqlFaster/UrunGrup/UrunGrupFasterOFLINE.sql"), System.Text.UTF8Encoding.Default);
                            if (subeid == null || subeid.Equals("0"))
                                Query = File.ReadAllText(HostingEnvironment.MapPath("/Sql/SqlFaster/UrunGrup/UrunGrupFasterOFLINE.sql"), System.Text.UTF8Encoding.Default);
                            if (subeid != null && !subeid.Equals("0") && productGroup != null)
                                Query = File.ReadAllText(HostingEnvironment.MapPath("/Sql/SqlFaster/UrunGrup/UrunGrupDetayFasterOFLINE.sql"), System.Text.UTF8Encoding.Default);
                        }
                    }

                    Query = Query.Replace("{SubeAdi}", SubeAdi);
                    Query = Query.Replace("{ProductName}", productGroup);
                    Query = Query.Replace("{TARIH1}", QueryTimeStart);
                    Query = Query.Replace("{TARIH2}", QueryTimeEnd);

                    if (ID == "1")
                    {
                        #region GET DATA
                        try
                        {
                            string connString = "Provider=SQLOLEDB;Server=" + SubeIP + ";User ID=" + SqlName + ";Password=" + SqlPassword + ";Database=" + DBName + "";
                            try
                            {
                                DataTable UrunGrubuDt = new DataTable();
                                UrunGrubuDt = f.GetSubeDataWithQuery((f.NewConnectionString(SubeIP, DBName, SqlName, SqlPassword)), Query.ToString());

                                #region MyRegion

                                if (UrunGrubuDt.Rows.Count > 0)
                                {
                                    if (subeid.Equals(""))
                                    {
                                        if (AppDbType == "3")
                                        {
                                            #region FASTER-(AppDbType = 3 faster kullanan şube)
                                            foreach (DataRow SubeR in UrunGrubuDt.Rows)
                                            {
                                                UrunGrubu items = new UrunGrubu();
                                                items.Sube = SubeAdi + "-" + SubeR["Sube1"].ToString() + "-" + SubeR["Kasa"].ToString();
                                                items.SubeID = SubeId + "~" + SubeR["ID"].ToString();
                                                items.Miktar = Convert.ToDecimal(SubeR["MIKTAR"]); //f.RTD(SubeR, "MIKTAR");
                                                items.ProductName = f.RTS(SubeR, "ProductName");
                                                items.ToplamMiktar = Convert.ToDecimal(SubeR["MIKTAR"]); //f.RTD(SubeR, "MIKTAR");
                                                items.TotalDebit = Convert.ToDecimal(SubeR["TUTAR"]); //f.RTD(SubeR, "TUTAR");
                                                Liste.Add(items);
                                            }
                                            #endregion   FASTER-(AppDbType = 3 faster kullanan şube)
                                        }
                                        else
                                        {
                                            UrunGrubu items = new UrunGrubu();
                                            items.Sube = SubeAdi;
                                            items.SubeID = (SubeId);
                                            foreach (DataRow SubeR in UrunGrubuDt.Rows)
                                            {
                                                //if (!subeid.Equals("0"))
                                                //{
                                                items.Miktar = f.RTD(SubeR, "MIKTAR");
                                                items.ProductName = f.RTS(SubeR, "ProductName");
                                                items.ToplamMiktar += f.RTD(SubeR, "MIKTAR");
                                                items.TotalDebit += f.RTD(SubeR, "TUTAR");
                                                //}
                                            }
                                            Liste.Add(items);
                                        }
                                    }
                                    else if (!subeid.Equals("") && productGroup == null)
                                    {
                                        if (AppDbType == "3")
                                        {
                                            foreach (DataRow SubeR in UrunGrubuDt.Rows)
                                            {
                                                if (subeid == SubeR["ID"].ToString())
                                                {
                                                    UrunGrubu items = new UrunGrubu();
                                                    items.Sube = SubeAdi;
                                                    items.SubeID = (SubeId);
                                                    items.ProductGroup = f.RTS(SubeR, "ProductName");
                                                    if (!subeid.Equals("0"))
                                                    {
                                                        items.Miktar = f.RTD(SubeR, "MIKTAR");
                                                        items.ProductName = f.RTS(SubeR, "ProductName");
                                                    }
                                                    items.Debit = f.RTD(SubeR, "TUTAR");
                                                    Liste.Add(items);
                                                }
                                            }
                                        }
                                        else
                                        {
                                            foreach (DataRow SubeR in UrunGrubuDt.Rows)
                                            {
                                                UrunGrubu items = new UrunGrubu();
                                                items.Sube = SubeAdi;
                                                items.SubeID = (SubeId);
                                                items.ProductGroup = f.RTS(SubeR, "ProductGroup");
                                                if (!subeid.Equals("0"))
                                                {
                                                    items.Miktar = f.RTD(SubeR, "MIKTAR");
                                                    items.ProductName = f.RTS(SubeR, "ProductName");
                                                }
                                                items.Debit = f.RTD(SubeR, "TUTAR");
                                                Liste.Add(items);
                                            }
                                        }
                                    }
                                    else
                                    {
                                        if (AppDbType == "3")
                                        {
                                            foreach (DataRow SubeR in UrunGrubuDt.Rows)
                                            {
                                                if (subeid == SubeR["ID"].ToString())
                                                {
                                                    UrunGrubu items = new UrunGrubu();
                                                    items.Sube = f.RTS(SubeR, "Sube");
                                                    items.SubeID = (SubeId);
                                                    items.Miktar = f.RTD(SubeR, "MIKTAR");
                                                    items.ProductName = f.RTS(SubeR, "ProductName");
                                                    items.Debit = f.RTD(SubeR, "TUTAR");
                                                    Liste.Add(items);
                                                }
                                            }
                                        }
                                        else
                                        {
                                            foreach (DataRow SubeR in UrunGrubuDt.Rows)
                                            {
                                                UrunGrubu items = new UrunGrubu();
                                                items.Sube = f.RTS(SubeR, "Sube");
                                                items.SubeID = (SubeId);
                                                items.Miktar = f.RTD(SubeR, "MIKTAR");
                                                items.ProductName = f.RTS(SubeR, "ProductName");
                                                items.Debit = f.RTD(SubeR, "TUTAR");
                                                Liste.Add(items);
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    UrunGrubu items = new UrunGrubu();
                                    items.Sube = SubeAdi + " (Data Yok)";
                                    items.SubeID = (SubeId);
                                    Liste.Add(items);
                                }

                                #endregion
                            }
                            catch (Exception) { throw new Exception(SubeAdi); }
                        }
                        catch (System.Exception ex)
                        {
                            #region EX                         
                            //log metnini oluştur
                            string ErrorMessage = "Şube/Ürün Raporu Alınamadı.";
                            //string SystemErrorMessage = ex.Message.ToString();
                            //string LogText = "";
                            //LogText += "Hata Zamanı : " + DateTime.Now.ToString() + Environment.NewLine;
                            //LogText += "Hata Mesajı : " + ErrorMessage + Environment.NewLine;
                            //LogText += "Hata Açıklaması : " + SystemErrorMessage + Environment.NewLine;
                            //LogText += "-----------------" + Environment.NewLine;
                            UrunGrubu items = new UrunGrubu();
                            items.Sube = ex.Message + " (Erişim Yok)";
                            items.SubeID = (SubeId);
                            items.ErrorMessage = ErrorMessage;
                            items.ErrorStatus = true;
                            items.ErrorCode = "01";
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
                                #region GET DATA
                                try
                                {
                                    string connString = "Provider=SQLOLEDB;Server=" + SubeIP + ";User ID=" + SqlName + ";Password=" + SqlPassword + ";Database=" + DBName + "";
                                    try
                                    {
                                        DataTable UrunGrubuDt = new DataTable();
                                        UrunGrubuDt = f.GetSubeDataWithQuery((f.NewConnectionString(SubeIP, DBName, SqlName, SqlPassword)), Query.ToString());

                                        #region MyRegion

                                        if (UrunGrubuDt.Rows.Count > 0)
                                        {
                                            if (subeid.Equals(""))
                                            {
                                                if (AppDbType == "3")
                                                {
                                                    #region FASTER-(AppDbType = 3 faster kullanan şube)
                                                    foreach (DataRow SubeR in UrunGrubuDt.Rows)
                                                    {
                                                        UrunGrubu items = new UrunGrubu();
                                                        items.Sube = SubeAdi + "-" + SubeR["Sube1"].ToString() + "-" + SubeR["Kasa"].ToString();
                                                        items.SubeID = SubeId + "~" + SubeR["ID"].ToString();
                                                        items.Miktar = Convert.ToDecimal(SubeR["MIKTAR"]); //f.RTD(SubeR, "MIKTAR");
                                                        items.ProductName = f.RTS(SubeR, "ProductName");
                                                        items.ToplamMiktar = Convert.ToDecimal(SubeR["MIKTAR"]); //f.RTD(SubeR, "MIKTAR");
                                                        items.TotalDebit = Convert.ToDecimal(SubeR["TUTAR"]); //f.RTD(SubeR, "TUTAR");
                                                        Liste.Add(items);
                                                    }
                                                    #endregion   FASTER-(AppDbType = 3 faster kullanan şube)
                                                }
                                                else
                                                {
                                                    UrunGrubu items = new UrunGrubu();
                                                    items.Sube = SubeAdi;
                                                    items.SubeID = (SubeId);
                                                    foreach (DataRow SubeR in UrunGrubuDt.Rows)
                                                    {
                                                        //if (!subeid.Equals("0"))
                                                        //{
                                                        items.Miktar = f.RTD(SubeR, "MIKTAR");
                                                        items.ProductName = f.RTS(SubeR, "ProductName");
                                                        items.ToplamMiktar += f.RTD(SubeR, "MIKTAR");
                                                        items.TotalDebit += f.RTD(SubeR, "TUTAR");
                                                        //}
                                                    }
                                                    Liste.Add(items);
                                                }
                                            }
                                            else if (!subeid.Equals("") && productGroup == null)
                                            {
                                                if (AppDbType == "3")
                                                {
                                                    foreach (DataRow SubeR in UrunGrubuDt.Rows)
                                                    {
                                                        if (subeid == SubeR["ID"].ToString())
                                                        {
                                                            UrunGrubu items = new UrunGrubu();
                                                            items.Sube = SubeAdi;
                                                            items.SubeID = (SubeId);
                                                            items.ProductGroup = f.RTS(SubeR, "ProductName");
                                                            if (!subeid.Equals("0"))
                                                            {
                                                                items.Miktar = f.RTD(SubeR, "MIKTAR");
                                                                items.ProductName = f.RTS(SubeR, "ProductName");
                                                            }
                                                            items.Debit = f.RTD(SubeR, "TUTAR");
                                                            Liste.Add(items);
                                                        }
                                                    }
                                                }
                                                else
                                                {
                                                    foreach (DataRow SubeR in UrunGrubuDt.Rows)
                                                    {
                                                        UrunGrubu items = new UrunGrubu();
                                                        items.Sube = SubeAdi;
                                                        items.SubeID = (SubeId);
                                                        items.ProductGroup = f.RTS(SubeR, "ProductGroup");
                                                        if (!subeid.Equals("0"))
                                                        {
                                                            items.Miktar = f.RTD(SubeR, "MIKTAR");
                                                            items.ProductName = f.RTS(SubeR, "ProductName");
                                                        }
                                                        items.Debit = f.RTD(SubeR, "TUTAR");
                                                        Liste.Add(items);
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                if (AppDbType == "3")
                                                {
                                                    foreach (DataRow SubeR in UrunGrubuDt.Rows)
                                                    {
                                                        if (subeid == SubeR["ID"].ToString())
                                                        {
                                                            UrunGrubu items = new UrunGrubu();
                                                            items.Sube = f.RTS(SubeR, "Sube");
                                                            items.SubeID = (SubeId);
                                                            items.Miktar = f.RTD(SubeR, "MIKTAR");
                                                            items.ProductName = f.RTS(SubeR, "ProductName");
                                                            items.Debit = f.RTD(SubeR, "TUTAR");
                                                            Liste.Add(items);
                                                        }
                                                    }
                                                }
                                                else
                                                {
                                                    foreach (DataRow SubeR in UrunGrubuDt.Rows)
                                                    {
                                                        UrunGrubu items = new UrunGrubu();
                                                        items.Sube = f.RTS(SubeR, "Sube");
                                                        items.SubeID = (SubeId);
                                                        items.Miktar = f.RTD(SubeR, "MIKTAR");
                                                        items.ProductName = f.RTS(SubeR, "ProductName");
                                                        items.Debit = f.RTD(SubeR, "TUTAR");
                                                        Liste.Add(items);
                                                    }
                                                }
                                            }
                                        }
                                        else
                                        {
                                            UrunGrubu items = new UrunGrubu();
                                            items.Sube = SubeAdi + " (Data Yok)";
                                            items.SubeID = (SubeId);
                                            Liste.Add(items);
                                        }

                                        #endregion
                                    }
                                    catch (Exception) { throw new Exception(SubeAdi); }
                                }
                                catch (System.Exception ex)
                                {
                                    #region EX                         
                                    //log metnini oluştur
                                    string ErrorMessage = "Şube/Ürün Raporu Alınamadı.";
                                    //string SystemErrorMessage = ex.Message.ToString();
                                    //string LogText = "";
                                    //LogText += "Hata Zamanı : " + DateTime.Now.ToString() + Environment.NewLine;
                                    //LogText += "Hata Mesajı : " + ErrorMessage + Environment.NewLine;
                                    //LogText += "Hata Açıklaması : " + SystemErrorMessage + Environment.NewLine;
                                    //LogText += "-----------------" + Environment.NewLine;
                                    UrunGrubu items = new UrunGrubu();
                                    items.Sube = ex.Message + " (Erişim Yok)";
                                    items.SubeID = (SubeId);
                                    items.ErrorMessage = ErrorMessage;
                                    items.ErrorStatus = true;
                                    items.ErrorCode = "01";
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