using DocumentFormat.OpenXml.Bibliography;
using DocumentFormat.OpenXml.Office2010.Excel;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml.Wordprocessing;
using SefimV2.Helper;
using SefimV2.Models.ProductSefimCRUD;
using SefimV2.ViewModels.User;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Hosting;
using System.Web.Mvc;

namespace SefimV2.Models
{
    public class UrunFiyatCRUD
    {

        public static List<SefimPanelUrunEkleViewModel> ProductList(string SubeIds, bool UpdatedProductListGet = false)
        {
            var list = new List<SefimPanelUrunEkleViewModel>();
            ModelFunctions f = new ModelFunctions();

            BussinessHelper.InvoiceNames = new List<SelectListItem>();
            BussinessHelper.ProductGroups = new List<SelectListItem>();
            BussinessHelper.ProductTypes = new List<SelectListItem>();
            BussinessHelper.Favoritess = new List<SelectListItem>();

            try
            {
                #region SUBSTATION LIST  

                f.SqlConnOpen();
                DataTable dt = f.DataTable("select * from SubeSettings Where Status=1 and ID in(" + SubeIds + ")");
                var dtList = dt.AsEnumerable().ToList<DataRow>();
                f.SqlConnClose();

                #endregion SUBSTATION LIST

                #region PARALLEL FOREACH

                var locked = new Object();
                Parallel.ForEach(dtList, r =>
                {
                    //foreach (DataRow r in dt.Rows)
                    //{
                    string SubeId = f.RTS(r, "ID");
                    string SubeAdi = f.RTS(r, "SubeName");
                    string SubeIP = f.RTS(r, "SubeIP");
                    string SqlName = f.RTS(r, "SqlName");
                    string SqlPassword = f.RTS(r, "SqlPassword");
                    string DBName = f.RTS(r, "DBName");
                    string Query = string.Empty;


                    string queryChoice1 = string.Empty;
                    string queryChoice2 = string.Empty;
                    string queryOptions = string.Empty;
                    string queryOptionCats = string.Empty;
                    string queryBom = string.Empty;
                    string queryBomOptions = string.Empty;

                    if (UpdatedProductListGet)
                    {
                        Query = "select * from Product where ProductGroup NOT LIKE '$%' and  ProductName NOT LIKE '$%'  and   ProductGroup <> '[R]Rezervasyon' and( IsSynced=0 or IsUpdated=1) order by ProductGroup";
                        queryChoice1 = "select * from Choice1 where  ( IsSynced=0 or IsUpdated=1) and ProductId=";
                        queryChoice2 = "select * from Choice2 where  ( IsSynced=0 or IsUpdated=1) and ProductId=";
                        queryOptions = "select * from Options where  ( IsSynced=0 or IsUpdated=1) and ProductId=";
                        queryOptionCats = "select * from OptionCats where ( IsSynced=0 or IsUpdated=1)  and ProductId=";
                        queryBom = "select * from Bom where ( IsSynced=0 or IsUpdated=1)  and ProductId=";
                        queryBomOptions = "select * from BomOptions where ( IsSynced=0 or IsUpdated=1)  and OptionsId=";
                    }
                    else
                    {
                        Query = "select * from Product where ProductGroup NOT LIKE '$%' and  ProductName NOT LIKE '$%' and ProductGroup <> '[R]Rezervasyon' ";
                        queryChoice1 = "select * from Choice1 where ProductId=";
                        queryChoice2 = "select * from Choice2 where ProductId=";
                        queryOptions = "select * from Options where ProductId=";
                        queryOptionCats = "select * from OptionCats where ProductId=";
                        queryBom = "select * from Bom where ProductId=";
                        queryBomOptions = "select * from BomOptions where OptionsId=";
                    }


                    #region GET DATA
                    try
                    {
                        DataTable productDt = new DataTable();
                        productDt = f.GetSubeDataWithQuery(f.NewConnectionString(SubeIP, DBName, SqlName, SqlPassword), Query.ToString());
                        var productList = productDt.AsEnumerable().ToList<DataRow>();

                        foreach (DataRow SubeR in productDt.Rows)
                        {
                            //Parallel.ForEach(productList, SubeR =>
                            //{
                            //    //items.Sube = SubeCiroDt.Rows[0]["Sube"].ToString(); //f.RTS(SubeR, "Sube");
                            //items.SubeId = SubeId;
                            //items.Cash = Convert.ToDecimal(SubeCiroDt.Rows[0]["Cash"]); //f.RTD(SubeR, "Cash");
                            SefimPanelUrunEkleViewModel items = new SefimPanelUrunEkleViewModel
                            {
                                Id = f.RTI(SubeR, "Id"),
                                ProductName = f.RTS(SubeR, "ProductName"),
                                ProductGroup = f.RTS(SubeR, "ProductGroup"),
                                ProductCode = f.RTS(SubeR, "ProductCode"),
                                Order = f.RTS(SubeR, "[Order]"),
                                //Price =  f.RTS(SubeR, "Price").Replace(",", "."),
                                //Price = decimal.Round(Convert.ToDecimal(f.RTS(SubeR, "Price").Replace(",", ".")), 2, MidpointRounding.AwayFromZero).ToString(),
                                VatRate = f.RTS(SubeR, "VatRate").Replace(",", "."),
                                ProductPrice = Convert.ToDecimal(f.RTD(SubeR, "Price"))
                            };

                            items.Price = decimal.Round(items.ProductPrice, 2, MidpointRounding.AwayFromZero).ToString();

                            if (SubeR["FreeItem"] != DBNull.Value)
                            {
                                items.FreeItem = Convert.ToBoolean(f.RTS(SubeR, "FreeItem"));
                            }
                            items.InvoiceName = f.RTS(SubeR, "InvoiceName");
                            items.ProductType = f.RTS(SubeR, "ProductType");
                            items.Plu = f.RTS(SubeR, "Plu");
                            if (SubeR["SkipOptionSelection"] != DBNull.Value)
                            {
                                items.SkipOptionSelection = Convert.ToBoolean(f.RTS(SubeR, "SkipOptionSelection"));
                            }
                            items.Favorites = f.RTS(SubeR, "Favorites");
                            if (SubeR["Aktarildi"] != DBNull.Value)
                            {
                                items.Aktarildi = Convert.ToBoolean(f.RTS(SubeR, "Aktarildi"));
                            }
                            items.ProductPkId = f.RTI(SubeR, "ProductPkId");
                            items.SubeId = Convert.ToInt32(SubeId);
                            items.SubeName = SubeAdi;

                            //items.Adet = f.RTI(SubeR, "ADET");
                            //items.Debit = f.RTD(SubeR, "TUTAR");
                            //items.PhoneOrderDebit = f.RTI(SubeR, "PhoneOrderDebit");

                            items.Choice1 = new List<Choice1>();
                            items.Choice2 = new List<Choice2>();
                            items.Options = new List<Options>();
                            items.OptionCats = new List<OptionCats>();
                            items.Boms = new List<Bom>();
                            items.BomOptionss = new List<BomOptions>();


                            DataTable dtOptions = f.GetSubeDataWithQuery(f.NewConnectionString(SubeIP, DBName, SqlName, SqlPassword), queryOptions.ToString() + items.Id);
                            var dtOptionsList = dtOptions.AsEnumerable().ToList<DataRow>();

                            bool kayitListeyeEklendiMi = false;
                            if (dtOptionsList.Count() > 0)
                            {
                                lock (locked)
                                {
                                    kayitListeyeEklendiMi = true;
                                    list.Add(items);
                                }
                            }
                            var id = items.Id.ToString();
                            Parallel.ForEach(dtOptionsList, opt =>
                            {
                                //Options options = new Options();
                                //options.Id = f.RTI(opt, "Id");
                                //options.Name = f.RTS(opt, "Name");
                                //options.Price = f.RTS(opt, "Price").Replace(",", ".");
                                //if (opt["Quantitative"] != DBNull.Value)
                                //    options.Quantitative = Convert.ToBoolean(f.RTS(opt, "Quantitative"));
                                //options.Category = f.RTS(opt, "Category");
                                //options.ProductId = f.RTI(opt, "ProductId");

                                lock (locked)
                                {

                                    Random rnd = new Random();
                                    id = f.RTI(opt, "Id") + "" + rnd.Next() + "" + items.Id;
                                }
                                SefimPanelUrunEkleViewModel items4 = new SefimPanelUrunEkleViewModel
                                {
                                    Id = Convert.ToInt64(id),
                                    ProductName = f.RTS(opt, "Name"),
                                    Price = f.RTS(opt, "Price").Replace(",", "."),
                                    SubeId = Convert.ToInt32(SubeId),
                                    SubeName = SubeAdi,
                                    OptionsPrice = f.RTD(opt, "Price"),
                                    //ProductGroup = f.RTS(SubeR, "ProductGroup"),
                                    //ProductCode = f.RTS(SubeR, "ProductCode"),
                                };

                                lock (locked)
                                {

                                    list.Add(items4);
                                }

                                //DataTable dtBomOptionsCount = f.GetSubeDataWithQuery(f.NewConnectionString(SubeIP, DBName, SqlName, SqlPassword), queryBomOptions.ToString() + options.Id);
                                //var dtBomOptionsList = dtBomOptionsCount.AsEnumerable().ToList<DataRow>();
                                //Parallel.ForEach(dtBomOptionsList, BomOpt =>
                                //{
                                //    BomOptions bomOpt = new BomOptions
                                //    {
                                //        Id = f.RTI(BomOpt, "Id"),
                                //        OptionsId = f.RTI(BomOpt, "OptionsId"),
                                //        OptionsName = f.RTS(BomOpt, "OptionsName"),
                                //        MaterialName = f.RTS(BomOpt, "MaterialName"),
                                //        Quantity = f.RTS(BomOpt, "Quantity").Replace(",", "."),
                                //        Unit = f.RTS(BomOpt, "Unit"),
                                //        StokID = f.RTI(BomOpt, "StokID"),
                                //        ProductName = f.RTS(BomOpt, "ProductName"),
                                //        SubeId = Convert.ToInt32(SubeId)
                                //    };
                                //    lock (locked)
                                //    {
                                //        items.BomOptionss.Add(bomOpt);
                                //    }
                                //});

                                //lock (locked)
                                //{
                                //    items.Options.Add(options);
                                //}
                            });

                            DataTable dtChoice1 = f.GetSubeDataWithQuery(f.NewConnectionString(SubeIP, DBName, SqlName, SqlPassword), queryChoice1.ToString() + items.Id);
                            var dtChoice1List = dtChoice1.AsEnumerable().ToList<DataRow>();


                            if (dtChoice1List.Count() > 0 && kayitListeyeEklendiMi == false)
                            {
                                lock (locked)
                                {

                                    list.Add(items);
                                }
                            }
                            var idch1 = items.Id.ToString();
                            Parallel.ForEach(dtChoice1List, choise1 =>
                            {
                                //Choice1 choice1 = new Choice1
                                //{
                                //    Id = f.RTI(choise1, "Id"),
                                //    Name = f.RTS(choise1, "Name"),
                                //    Price = f.RTS(choise1, "Price").Replace(",", "."),
                                //    ProductId = f.RTI(choise1, "ProductId")
                                //};
                                lock (locked)
                                {
                                    Random rnd = new Random();
                                    idch1 = f.RTI(choise1, "Id") + "" + rnd.Next() + "" + items.Id;
                                }

                                SefimPanelUrunEkleViewModel items2 = new SefimPanelUrunEkleViewModel
                                {
                                    //decimal.Round(item.NetTutar, 2, MidpointRounding.AwayFromZero)
                                    Id = Convert.ToInt64(idch1),
                                    ProductName = f.RTS(SubeR, "ProductName") + "." + f.RTS(choise1, "Name"),
                                    ProductGroup = f.RTS(SubeR, "ProductGroup"),
                                    ProductCode = f.RTS(SubeR, "ProductCode"),
                                    SubeId = Convert.ToInt32(SubeId),
                                    SubeName = SubeAdi,
                                    Choice1Price = Convert.ToDecimal(f.RTD(choise1, "Price"))
                                };

                                lock (locked)
                                {
                                    var toplamPrice = items.ProductPrice + items2.Choice1Price;
                                    items2.Price = decimal.Round(toplamPrice, 2, MidpointRounding.AwayFromZero).ToString();
                                    //items.Choice1.Add(choice1);
                                    list.Add(items2);
                                }

                                var idch2 = items.Id.ToString();
                                DataTable dtChoise2 = f.GetSubeDataWithQuery(f.NewConnectionString(SubeIP, DBName, SqlName, SqlPassword), queryChoice2.ToString() + items.Id + " and Choice1Id=" + f.RTI(choise1, "Id"));
                                var dtChoice2List = dtChoise2.AsEnumerable().ToList<DataRow>();
                                Parallel.ForEach(dtChoice2List, choise2 =>
                                {

                                    //Choice2 choice2 = new Choice2
                                    //{
                                    //    Id = f.RTI(choise2, "Id"),
                                    //    Name = f.RTS(choise2, "Name"),
                                    //    Price = f.RTS(choise2, "Price").Replace(",", "."),
                                    //    Choice1Id = f.RTI(choise2, "Choice1Id"),
                                    //    ProductId = f.RTI(choise2, "ProductId")
                                    //};
                                    lock (locked)
                                    {
                                        Random rnd1 = new Random();
                                        idch2 = f.RTI(choise2, "Id") + "" + rnd1.Next() + "" + items.Id;
                                    }

                                    SefimPanelUrunEkleViewModel items3 = new SefimPanelUrunEkleViewModel
                                    {
                                        Id = Convert.ToInt64(idch2),
                                        ProductName = f.RTS(SubeR, "ProductName") + "." + f.RTS(choise1, "Name") + "." + f.RTS(choise2, "Name"),
                                        ProductGroup = f.RTS(SubeR, "ProductGroup"),
                                        ProductCode = f.RTS(SubeR, "ProductCode"),
                                        SubeId = Convert.ToInt32(SubeId),
                                        SubeName = SubeAdi,
                                        Choice2Price = Convert.ToDecimal(f.RTD(choise1, "Price"))
                                    };

                                    lock (locked)
                                    {
                                        var toplamPrice2 = items.ProductPrice + items2.Choice1Price + items3.Choice2Price;
                                        items3.Price = decimal.Round(toplamPrice2, 2, MidpointRounding.AwayFromZero).ToString();
                                        //items.Choice2.Add(choice2);
                                        list.Add(items3);
                                    }
                                });
                            });


                            //DataTable dtOptionCats = f.GetSubeDataWithQuery(f.NewConnectionString(SubeIP, DBName, SqlName, SqlPassword), queryOptionCats.ToString() + items.Id);
                            //var dtOptionCatsList = dtOptionCats.AsEnumerable().ToList<DataRow>();
                            //Parallel.ForEach(dtOptionCatsList, optCat =>
                            //{
                            //    OptionCats optionCats = new OptionCats
                            //    {
                            //        Id = f.RTI(optCat, "Id"),
                            //        Name = f.RTS(optCat, "Name"),
                            //        MaxSelections = f.RTS(optCat, "MaxSelections").Replace(",", "."),
                            //        MinSelections = f.RTS(optCat, "MinSelections").Replace(",", "."),
                            //        ProductId = f.RTI(optCat, "ProductId")
                            //    };
                            //    lock (locked)
                            //    {
                            //        items.OptionCats.Add(optionCats);
                            //    }
                            //});

                            //DataTable dtBoms = f.GetSubeDataWithQuery(f.NewConnectionString(SubeIP, DBName, SqlName, SqlPassword), queryBom.ToString() + items.Id);
                            //var dtBomsList = dtBoms.AsEnumerable().ToList<DataRow>();
                            //Parallel.ForEach(dtBomsList, bm =>
                            //{
                            //    Bom bom = new Bom
                            //    {
                            //        Id = f.RTI(bm, "Id"),
                            //        ProductName = f.RTS(bm, "ProductName"),
                            //        MaterialName = f.RTS(bm, "MaterialName"),
                            //        Quantity = f.RTS(bm, "Quantity").Replace(",", "."),
                            //        Unit = f.RTS(bm, "Unit"),
                            //        StokID = f.RTI(bm, "StokID"),
                            //        ProductId = f.RTI(bm, "ProductId"),
                            //        SubeId = Convert.ToInt32(SubeId)
                            //    };
                            //    lock (locked)
                            //    {
                            //        items.Boms.Add(bom);
                            //    }
                            //});

                            ////Masterdaki bom ve bomoptions çekiliyor şubeye bağlı
                            //foreach (var item in SefimPanelUrunEkleCRUD.GetBoms(items.Id, items.SubeId))
                            //{
                            //    lock (locked)
                            //    {
                            //        items.Boms.Add(item);
                            //    }
                            //};

                            //foreach (var item in SefimPanelUrunEkleCRUD.GetBomOptions(items.Id, items.SubeId))
                            //{
                            //    lock (locked)
                            //    {
                            //        items.BomOptionss.Add(item);
                            //    }
                            //};


                            lock (locked)
                            {
                                if (!kayitListeyeEklendiMi)
                                {
                                    list.Add(items);
                                }
                            }
                        }
                        //});//parrallel
                    }
                    catch (Exception ex)
                    {
                        Singleton.WritingLogFile2("SefimPanelUrunEkleCRUDGetData:", ex.Message.ToString(), "", ex.StackTrace);
                    }
                    #endregion GET DATA

                });

                #endregion PARALLEL FOREACH
            }
            catch (DataException ex)
            {
                Singleton.WritingLogFile2("SefimPanelUrunEkleCRUDParallelForeach:", ex.Message.ToString(), "", ex.StackTrace);
            }

            return list;
        }

        public static List<UrunFiyat> List(DateTime Date1, DateTime Date2, string subeid, string productGroup, string ID)
        {
            if (productGroup == "alt=\"expand/collapse\"")
            {
                productGroup = "NULL";
            }

            List<UrunFiyat> Liste = new List<UrunFiyat>();
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

            string subeid_ = string.Empty;
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
                    string Query = string.Empty;
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
                    string urunEslestirmeVarMi = f.RTS(r, "UrunEslestirmeVarMi");
                    string vPosSubeKodu = r["VPosSubeKodu"].ToString();
                    string vPosKasaKodu = r["VPosKasaKodu"].ToString();

                    string vPosKasaKoduParametr = string.Empty;



                    if (AppDbType == "1" /*|| AppDbType == "2"*/)
                    {
                        if (subeid != null && !subeid.Equals("0") && productGroup == null)// sube secili degilse ilk giris yapilan sql
                        {

                            Query = File.ReadAllText(HostingEnvironment.MapPath("/Sql/SqlUrunFiyat/Product.sql"), System.Text.UTF8Encoding.Default);

                        }
                        if (subeid == null || subeid.Equals("0"))
                        {
                            Query = File.ReadAllText(HostingEnvironment.MapPath("/Sql/SqlUrunFiyat/UrunGrup.sql"), System.Text.UTF8Encoding.Default);
                        }
                        if (subeid != null && !subeid.Equals("0") && productGroup != null)
                        {
                            if (string.IsNullOrEmpty(urunEslestirmeVarMi) || urunEslestirmeVarMi == "False")
                            {
                                Query = File.ReadAllText(HostingEnvironment.MapPath("/Sql/SqlUrunFiyat/UrunGrupDetay.sql"), System.Text.UTF8Encoding.Default);
                            }
                            else
                            {
                                Query = File.ReadAllText(HostingEnvironment.MapPath("/Sql/SqlUrunFiyat/UrunGrupDetayUrunEslestirme.sql"), System.Text.UTF8Encoding.Default);
                            }
                        }
                    }




                    Query = Query.Replace("{SubeAdi}", SubeAdi);
                    Query = Query.Replace("{ProductName}", productGroup);
                    Query = Query.Replace("{TARIH1}", QueryTimeStart);
                    Query = Query.Replace("{TARIH2}", QueryTimeEnd);
                    Query = Query.Replace("{FIRMAIND}", FirmaId);
                    Query = Query.Replace("{SUBE2}", vPosSubeKodu);
                    Query = Query.Replace("{KASAKODU}", vPosKasaKodu);

                    #endregion SQL QUARY  *(Sefim || (faster || Faster Offline || Faster Online))*

                    if (ID == "1")
                    {
                        #region GET DATA

                        try
                        {
                            try
                            {
                                DataTable UrunGrubuDt = new DataTable();
                                UrunGrubuDt = f.GetSubeDataWithQuery((f.NewConnectionString(SubeIP, DBName, SqlName, SqlPassword)), Query.ToString());

                                #region MyRegion

                                if (UrunGrubuDt.Rows.Count > 0)
                                {
                                    if (subeid.Equals("")) /*LİSTE*/
                                    {
                                        if (AppDbType == "3" || AppDbType == "4")//
                                        {

                                        }
                                        else
                                        {
                                            #region SEFIM ESKI/YENI (SUBE BAZLI)  

                                            UrunFiyat items = new UrunFiyat
                                            {
                                                Sube = SubeAdi,
                                                SubeID = (SubeId)
                                            };
                                            foreach (DataRow SubeR in UrunGrubuDt.Rows)
                                            {
                                                items.Price = f.RTD(SubeR, "Price");
                                                items.ProductName = f.RTS(SubeR, "ProductName");
                                                items.ProductCode = f.RTS(SubeR, "ProductCode");
                                                items.VatRate = f.RTD(SubeR, "VatRate");

                                            }
                                            lock (locked)
                                            {
                                                Liste.Add(items);
                                            }
                                            #endregion SEFIM ESKI/YENI 
                                        }
                                    }
                                    else if (!subeid.Equals("") && productGroup == "False" || productGroup != "") /*DETAIL*/
                                    {
                                        if (AppDbType == "3" || AppDbType == "4")
                                        {

                                        }
                                        else
                                        {
                                            #region 2.KIRILIM SEFIM ESKI/YENI 

                                            foreach (DataRow SubeR in UrunGrubuDt.Rows)
                                            {
                                                UrunFiyat items = new UrunFiyat
                                                {
                                                    Sube = SubeAdi,
                                                    SubeID = (SubeId),
                                                    ProductGroup = f.RTS(SubeR, "ProductGroup")
                                                };

                                                if (!subeid.Equals("0"))
                                                {
                                                    items.Price = f.RTD(SubeR, "Price");
                                                    items.ProductName = f.RTS(SubeR, "ProductName");
                                                    items.ProductCode = f.RTS(SubeR, "ProductCode");
                                                    items.VatRate = f.RTD(SubeR, "VatRate");
                                                }
                                                lock (locked)
                                                {
                                                    Liste.Add(items);
                                                }
                                            }

                                            #endregion 2.KIRILIM SEFIM ESKI/YENI 
                                        }
                                    }
                                    else /*PRODUCTS_DETAIL*/
                                    {
                                        if (AppDbType == "3")
                                        {

                                        }
                                        else
                                        {
                                            #region 3. KIRILIM SEFIM ESKI/YENI 

                                            foreach (DataRow SubeR in UrunGrubuDt.Rows)
                                            {
                                                UrunFiyat items = new UrunFiyat
                                                {
                                                    Sube = f.RTS(SubeR, "Sube"),
                                                    SubeID = (SubeId),
                                                    Price = f.RTD(SubeR, "Price"),
                                                    ProductName = f.RTS(SubeR, "ProductName"),
                                                    VatRate = f.RTD(SubeR, "VatRate")
                                                };
                                                lock (locked)
                                                {
                                                    Liste.Add(items);
                                                }
                                            }
                                            #endregion 3. KIRILIM SEFIM ESKI/YENI 
                                        }
                                    }
                                }
                                else
                                {
                                    UrunFiyat items = new UrunFiyat();
                                    items.Sube = SubeAdi + " (Data Yok)";
                                    items.SubeID = (SubeId);
                                    lock (locked)
                                    {
                                        Liste.Add(items);
                                    }
                                }

                                #endregion
                            }
                            catch (Exception) { throw new Exception(SubeAdi); }
                        }
                        catch (Exception ex)
                        {
                            #region EX 

                            Singleton.WritingLogFile2("UrunGrubuCRUD", ex.ToString(), null, ex.StackTrace);
                            UrunFiyat items = new UrunFiyat();
                            items.Sube = ex.Message + " (Erişim Yok)";
                            items.SubeID = (SubeId);
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

                            }
                        }
                        #endregion
                    }
                });

                #endregion PARALLEL FOREACH
            }
            catch (DataException ex) { Singleton.WritingLogFile2("UrunGrubuCRUD", ex.ToString(), null, ex.StackTrace); }
            return Liste;
        }
    }
}