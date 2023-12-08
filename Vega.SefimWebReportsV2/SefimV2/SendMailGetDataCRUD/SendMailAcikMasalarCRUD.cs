using SefimV2.Models;
using SefimV2.ViewModels.User;
using SefimV2.ViewModelSendMail.AcikMasalarReportSendMail;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Hosting;

namespace SefimV2.SendMailGetDataCRUD
{
    public class SendMailAcikMasalarCRUD
    {
        public static List<AcikMasalarReportSendMailViewModel> List(DateTime Date1, DateTime Date2, string subeid, string masano, string ID)
        {
            List<AcikMasalarReportSendMailViewModel> Liste = new List<AcikMasalarReportSendMailViewModel>();
            //List<AcikMasalarReportSendMailViewModel> ListeTotal = new List<AcikMasalarReportSendMailViewModel>();
            ModelFunctions f = new ModelFunctions();
            DateTime startDate = DateTime.Now;

            #region GİRİŞ YAPAN KULLANICININ YETKİLİ OLDUĞU ŞUBELER LİSTELENİYOR
            UserViewModel model = UsersListCRUD.YetkiliSubesi(ID);
            #endregion

            try
            {
                f.SqlConnOpen();

                string filter = "Where Status=1";

                if (!subeid.Equals(""))
                    filter += " and Id=" + subeid;

                DataTable dt = f.DataTable("select * from SubeSettings " + filter);
                TimeSpan sure = DateTime.Now - startDate;

                foreach (DataRow r in dt.Rows)
                {
                    string SubeId = f.RTS(r, "Id");
                    string SubeAdi = f.RTS(r, "SubeName");
                    string SubeIP = f.RTS(r, "SubeIP");
                    string SqlName = f.RTS(r, "SqlName");
                    string SqlPassword = f.RTS(r, "SqlPassword");
                    string DBName = f.RTS(r, "DBName");
                    string QueryTimeStart = Date1.ToString("yyyy-MM-dd HH:mm:ss");
                    string QueryTimeEnd = Date2.ToString("yyyy-MM-dd HH:mm:ss");
                    string Query = "";

                    if (subeid.Equals(""))
                        Query = File.ReadAllText(HostingEnvironment.MapPath("/Sql/AcikMasalar.sql"), System.Text.Encoding.UTF8);
                    if (!subeid.Equals("") && masano.Equals(""))
                        Query = File.ReadAllText(HostingEnvironment.MapPath("/Sql/AcikMasaDetay.sql"), System.Text.Encoding.UTF8);
                    if (!subeid.Equals("") && !masano.Equals(""))
                        Query = File.ReadAllText(HostingEnvironment.MapPath("/Sql/AcikMasaUrunleriDetay.sql"), System.Text.Encoding.UTF8);

                    Query = Query.Replace("{TARIH1}", QueryTimeStart);
                    Query = Query.Replace("{TARIH2}", QueryTimeEnd);
                    Query = Query.Replace("{SubeAdi}", SubeAdi);
                    if (!subeid.Equals("") && !masano.Equals(""))
                        Query = Query.Replace("{TableNumber}", masano);

                    if (ID == "1")
                    {
                        #region GET DATA
                        try
                        {
                            string connString = "Provider=SQLOLEDB;Server=" + SubeIP + ";User Id=" + SqlName + ";Password=" + SqlPassword + ";Database=" + DBName + "";
                            f.SqlConnOpen(true, connString);
                            DataTable AcikHesapDt = f.DataTable(Query, true);

                            if (AcikHesapDt.Rows.Count > 0)
                            {
                                if (subeid.Equals(""))
                                {
                                    AcikMasalarReportSendMailViewModel items = new AcikMasalarReportSendMailViewModel();
                                    //items.SubeID = Convert.ToInt32(SubeId);
                                    items.Sube = SubeAdi;
                                    //items.TarihMin = f.RTS(AcikHesapDt.Rows[0], "TarihMin");
                                    //items.TarihMax = f.RTS(AcikHesapDt.Rows[0], "TarihMax");
                                    items.MasaNo = f.RTS(AcikHesapDt.Rows[0], "TOPLAM_MASA");
                                    items.Tutar = f.RTD(AcikHesapDt.Rows[0], "TOPLAM_TUTAR");
                                    foreach (DataRow SubeR in AcikHesapDt.Rows)
                                    {
                                        items.MasaSayisi += 1; //Convert.ToInt32(f.RTS(SubeR, "TOPLAM_MASA"));
                                        items.ToplamTutar += f.RTD(SubeR, "TOPLAM_TUTAR");
                                    }
                                    Liste.Add(items);
                                }
                                else
                                {
                                    foreach (DataRow SubeR in AcikHesapDt.Rows)
                                    {
                                        AcikMasalarReportSendMailViewModel items = new AcikMasalarReportSendMailViewModel();
                                        //items.SubeID = Convert.ToInt32(SubeId);
                                        items.Sube = SubeAdi;
                                        ////items.TarihMin = f.RTS(SubeR, "TarihMin");
                                        ////items.TarihMax = f.RTS(SubeR, "TarihMax");
                                        items.MasaNo = f.RTS(SubeR, "TableNumber");
                                        items.Tutar = f.RTD(SubeR, "TUTAR");
                                        //items.TotalDebit += f.RTD(SubeR, "Tutar");
                                        //items.GecenSure = Convert.ToDateTime(DateTime.Now - (Convert.ToDateTime(f.RTS(SubeR, "TarihMax")))).ToShortTimeString();                                  
                                        if (!subeid.Equals("") && masano != null)
                                            items.Urun += f.RTS(SubeR, "ProductName");
                                        Liste.Add(items);
                                    }
                                }
                                //foreach (DataRow SubeR in dt.Rows)
                                //{
                                //    AcikMasalarReportSendMailViewModel itemTotal = new AcikMasalarReportSendMailViewModel();
                                //    //itemTotal.SubeID = Convert.ToInt32(SubeId);
                                //    itemTotal.Sube = SubeAdi;
                                //    //itemTotal.TarihMin = f.RTS(SubeR, "TarihMin");
                                //    //itemTotal.TarihMax = f.RTS(SubeR, "TarihMax");
                                //    itemTotal.MasaNo = f.RTS(SubeR, "TOPLAM_MASA");
                                //    itemTotal.Tutar = f.RTD(SubeR, "TOPLAM_TUTAR");
                                //    itemTotal.ToplamTutar += f.RTD(SubeR, "TOPLAM_TUTAR");
                                //    ListeTotal.Add(itemTotal);
                                //}
                            }                       
                            //var _TableNumber = AcikHesapDt.AsEnumerable().GroupBy(x => x.Field<string>("TableNumber"), (row, g) => new { Column = row, count = g.Count() });
                            //foreach (var item in _TableNumber)
                            //{
                            //    masaSayisi += item.count;
                            //}
                            //Liste.ForEach(x => x.MasaSayisi = masaSayisi);
                            //Liste.ForEach(x => x.ListTotal = ListeTotal);
                            f.SqlConnClose(true);
                        }
                        catch (System.Exception ex)
                        {

                            //log metnini oluştur
                            string ErrorMessage = "Kasa Raporu Alınamadı.";
                            string SystemErrorMessage = ex.Message.ToString();
                            string LogText = "";
                            LogText += "Hata Zamanı : " + DateTime.Now.ToString() + Environment.NewLine;
                            LogText += "Hata Mesajı : " + ErrorMessage + Environment.NewLine;
                            LogText += "Hata Açıklaması : " + SystemErrorMessage + Environment.NewLine;
                            LogText += "-----------------" + Environment.NewLine;

                            AcikMasalarReportSendMailViewModel items = new AcikMasalarReportSendMailViewModel();
                            //items.CustomerName = cu;
                            //items.Debit = SubeId;
                            //items.ErrorMessage = ErrorMessage;
                            //items.ErrorStatus = true;
                            //items.ErrorCode = "01";
                            Liste.Add(items);

                            string LogFolder = "/Uploads/Logs/Error";
                            if (Directory.Exists(HostingEnvironment.MapPath(LogFolder)) == false) { Directory.CreateDirectory(HostingEnvironment.MapPath(LogFolder)); }
                            string LogFile = "Sube-" + SubeId + ".txt";
                            string LogFilePath = HostingEnvironment.MapPath(LogFolder + "/" + LogFile);


                            if (File.Exists(LogFilePath) == false)
                            {
                                string FirstLine = "Created on " + DateTime.Now.ToString() + Environment.NewLine + Environment.NewLine;
                                File.WriteAllText(LogFilePath, FirstLine);
                            }
                            File.AppendAllText(LogFilePath, LogText);
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
                                    f.SqlConnOpen(true, connString);
                                    DataTable AcikHesapDt = f.DataTable(Query, true);

                                    if (AcikHesapDt.Rows.Count > 0)
                                    {
                                        if (subeid.Equals(""))
                                        {
                                            AcikMasalarReportSendMailViewModel items = new AcikMasalarReportSendMailViewModel();
                                            //items.SubeID = Convert.ToInt32(SubeId);
                                            items.Sube = SubeAdi;
                                            //items.TarihMin = f.RTS(AcikHesapDt.Rows[0], "TarihMin");
                                            //items.TarihMax = f.RTS(AcikHesapDt.Rows[0], "TarihMax");
                                            items.MasaNo = f.RTS(AcikHesapDt.Rows[0], "TOPLAM_MASA");
                                            items.Tutar = f.RTD(AcikHesapDt.Rows[0], "TOPLAM_TUTAR");
                                            foreach (DataRow SubeR in AcikHesapDt.Rows)
                                            {
                                                items.MasaSayisi += 1; //Convert.ToInt32(f.RTS(SubeR, "TOPLAM_MASA"));
                                                items.ToplamTutar = f.RTD(SubeR, "TOPLAM_TUTAR");
                                            }
                                            Liste.Add(items);
                                        }
                                        else
                                        {
                                            foreach (DataRow SubeR in AcikHesapDt.Rows)
                                            {
                                                AcikMasalarReportSendMailViewModel items = new AcikMasalarReportSendMailViewModel();
                                                //items.SubeID = Convert.ToInt32(SubeId);
                                                items.Sube = SubeAdi;
                                                ////items.TarihMin = f.RTS(SubeR, "TarihMin");
                                                ////items.TarihMax = f.RTS(SubeR, "TarihMax");
                                                items.MasaNo = f.RTS(SubeR, "TableNumber");
                                                items.Tutar = f.RTD(SubeR, "TUTAR");
                                                //items.TotalDebit += f.RTD(SubeR, "Tutar");
                                                //items.GecenSure = Convert.ToDateTime(DateTime.Now - (Convert.ToDateTime(f.RTS(SubeR, "TarihMax")))).ToShortTimeString();                                  
                                                if (!subeid.Equals("") && masano != null)
                                                    items.Urun += f.RTS(SubeR, "ProductName");
                                                Liste.Add(items);
                                            }
                                        }
                                        //foreach (DataRow SubeR in dt.Rows)
                                        //{
                                        //    AcikMasalarReportSendMailViewModel itemTotal = new AcikMasalarReportSendMailViewModel();
                                        //    //itemTotal.SubeID = Convert.ToInt32(SubeId);
                                        //    itemTotal.Sube = SubeAdi;
                                        //    //itemTotal.TarihMin = f.RTS(SubeR, "TarihMin");
                                        //    //itemTotal.TarihMax = f.RTS(SubeR, "TarihMax");
                                        //    itemTotal.MasaNo = f.RTS(SubeR, "TOPLAM_MASA");
                                        //    itemTotal.Tutar = f.RTD(SubeR, "TOPLAM_TUTAR");
                                        //    itemTotal.ToplamTutar += f.RTD(SubeR, "TOPLAM_TUTAR");
                                        //    ListeTotal.Add(itemTotal);
                                        //}
                                    }
                                  
                                    //var _TableNumber = AcikHesapDt.AsEnumerable().GroupBy(x => x.Field<string>("TableNumber"), (row, g) => new { Column = row, count = g.Count() });
                                    //foreach (var item in _TableNumber)
                                    //{
                                    //    masaSayisi += item.count;
                                    //}

                                    //Liste.ForEach(x => x.MasaSayisi = masaSayisi);
                                    //Liste.ForEach(x => x.ListTotal = ListeTotal);
                                    f.SqlConnClose(true);
                                }
                                catch (System.Exception ex)
                                {

                                    //log metnini oluştur
                                    string ErrorMessage = "Kasa Raporu Alınamadı.";
                                    string SystemErrorMessage = ex.Message.ToString();
                                    string LogText = "";
                                    LogText += "Hata Zamanı : " + DateTime.Now.ToString() + Environment.NewLine;
                                    LogText += "Hata Mesajı : " + ErrorMessage + Environment.NewLine;
                                    LogText += "Hata Açıklaması : " + SystemErrorMessage + Environment.NewLine;
                                    LogText += "-----------------" + Environment.NewLine;

                                    AcikMasalarReportSendMailViewModel items = new AcikMasalarReportSendMailViewModel();
                                    //items.CustomerName = cu;
                                    //items.Debit = SubeId;
                                    //items.ErrorMessage = ErrorMessage;
                                    //items.ErrorStatus = true;
                                    //items.ErrorCode = "01";
                                    Liste.Add(items);

                                    string LogFolder = "/Uploads/Logs/Error";
                                    if (Directory.Exists(HostingEnvironment.MapPath(LogFolder)) == false) { Directory.CreateDirectory(HostingEnvironment.MapPath(LogFolder)); }
                                    string LogFile = "Sube-" + SubeId + ".txt";
                                    string LogFilePath = HostingEnvironment.MapPath(LogFolder + "/" + LogFile);


                                    if (File.Exists(LogFilePath) == false)
                                    {
                                        string FirstLine = "Created on " + DateTime.Now.ToString() + Environment.NewLine + Environment.NewLine;
                                        File.WriteAllText(LogFilePath, FirstLine);
                                    }
                                    File.AppendAllText(LogFilePath, LogText);
                                }
                                #endregion
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