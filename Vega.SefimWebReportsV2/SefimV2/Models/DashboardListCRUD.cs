using SefimV2.ViewModels.User;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Hosting;

namespace SefimV2.Models
{
    public class DashboardListCRUD
    {
        public static SubeCiro DashboardList_(DateTime Date1, DateTime Date2, string ID)
        {
            //SubeCiro modelView = new SubeCiro();
            SubeCiro items = new SubeCiro();
            List<SubeCiro> Liste = new List<SubeCiro>();
            ModelFunctions f = new ModelFunctions();
            DateTime startDate = DateTime.Now;

            #region GİRİŞ YAPAN KULLANICININ YETKİLİ OLDUĞU ŞUBELER LİSTELENİYOR
            UserViewModel model = UsersListCRUD.YetkiliSubesi(ID);
            #endregion
            try
            {
                f.SqlConnOpen();
                DataTable dt = f.DataTable("SELECT * FROM SubeSettings WHERE Status=1  ");
                TimeSpan sure = DateTime.Now - startDate;

                foreach (DataRow r in dt.Rows)
                {
                    string SubeId = f.RTS(r, "ID");
                    string SubeAdi = f.RTS(r, "SubeName");
                    string SubeIP = f.RTS(r, "SubeIP");
                    string SqlName = f.RTS(r, "SqlName");
                    string SqlPassword = f.RTS(r, "SqlPassword");
                    string DBName = f.RTS(r, "DBName");
                    string QueryTimeStart = Date1.ToString("yyyy/MM/dd HH:mm:ss");
                    string QueryTimeEnd = Date2.ToString("yyyy/MM/dd HH:mm:ss");


                    #region  SEFİM YENI - ESKİ FASTER SQL
                    string AppDbType = f.RTS(r, "AppDbType");
                    string Query = "";
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
                    #endregion

                    Query = Query.Replace("{SUBEADI}", SubeAdi);
                    Query = Query.Replace("{TARIH1}", QueryTimeStart);
                    Query = Query.Replace("{TARIH2}", QueryTimeEnd);

                    if (ID == "1")
                    {
                        #region SUPER ADMİN                      
                        #region GET DATA                
                        try
                        {
                            string connString = "Provider=SQLOLEDB;Server=" + SubeIP + ";User ID=" + SqlName + ";Password=" + SqlPassword + ";Database=" + DBName + "";
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
                        catch (System.Exception ex)
                        {
                            #region EX                       
                            //log metnini oluştur
                            string ErrorMessage = "Ciro Raporu Alınamadı.";
                            //string SystemErrorMessage = ex.Message.ToString();
                            //string LogText = "";
                            //LogText += "Hata Zamanı : " + DateTime.Now.ToString() + Environment.NewLine;
                            //LogText += "Hata Mesajı : " + ErrorMessage + Environment.NewLine;
                            //LogText += "Hata Açıklaması : " + SystemErrorMessage + Environment.NewLine;
                            //LogText += "-----------------" + Environment.NewLine;

                            //SubeCiro items = new SubeCiro();
                            items.Sube = SubeAdi;
                            items.SubeId = SubeId;
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
                            #endregion

                        }
                        #endregion
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
                                    string connString = "Provider=SQLOLEDB;Server=" + SubeIP + ";User ID=" + SqlName + ";Password=" + SqlPassword + ";Database=" + DBName + "";
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
                                catch (System.Exception ex)
                                {
                                    #region MyRegion


                                    //log metnini oluştur
                                    string ErrorMessage = "Ciro Raporu Alınamadı.";
                                    //string SystemErrorMessage = ex.Message.ToString();
                                    //string LogText = "";
                                    //LogText += "Hata Zamanı : " + DateTime.Now.ToString() + Environment.NewLine;
                                    //LogText += "Hata Mesajı : " + ErrorMessage + Environment.NewLine;
                                    //LogText += "Hata Açıklaması : " + SystemErrorMessage + Environment.NewLine;
                                    //LogText += "-----------------" + Environment.NewLine;

                                    //SubeCiro items = new SubeCiro();
                                    items.Sube = SubeAdi;
                                    items.SubeId = SubeId;
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
                                    #endregion
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



            return items;
        }
    }
}