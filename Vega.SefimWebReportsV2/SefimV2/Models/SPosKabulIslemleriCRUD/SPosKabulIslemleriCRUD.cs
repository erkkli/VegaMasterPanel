using SefimV2.ViewModels.SPosKabulIslemleri;
using SefimV2.ViewModels.SPosVeriGonderimi;
using SefimV2.ViewModels.SubeSettings;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using static SefimV2.ViewModels.SPosVeriGonderimi.UrunEditViewModel;

namespace SefimV2.Models
{
    public class SPosKabulIslemleriCRUD
    {
        ModelFunctions modelFunctions = new ModelFunctions();

        public List<BelgeDepoKabul> GetKabulList(int TanimID, int DonemId, string DepoId)
        {

            List<BelgeDepoKabul> items = new List<BelgeDepoKabul>();
            ModelFunctions f = new ModelFunctions();
            try
            {
                f.SqlConnOpen();
                DataTable dt = f.DataTable("SELECT Id,DBName,IP,SqlName,SqlPassword  FROM  VegaDbSettings");
                string Query = "";
                foreach (DataRow r in dt.Rows)
                {
                    string VegaDbId = f.RTS(r, "Id");
                    string VegaDbName = f.RTS(r, "DBName");
                    string VegaDbIp = f.RTS(r, "IP");
                    string VegaDbSqlName = f.RTS(r, "SqlName");
                    string VegaDbSqlPassword = f.RTS(r, "SqlPassword");
                    Query = @"SELECT
            BAS.IND,
            DP1.DEPOKODU AS GONDERENDEPOKODU,
            DP2.DEPOKODU AS KABULEDECEKDEPOKODU,
            BAS.TARIH,
            BAS.BELGENO AS FISNO,
            BAS.OZELKOD3,
            BAS.OZELKOD4
            FROM F0" + TanimID.ToString() + "D" + DonemId.ToString().PadLeft(4, '0') + "TBLDEPOHARBASLIK AS BAS" +
            @" LEFT JOIN F0" + TanimID.ToString() + @"TBLDEPOLAR AS DP1 ON DP1.IND = BAS.DEPO
            LEFT JOIN F0" + TanimID.ToString() + @"TBLDEPOLAR AS DP2 ON DP2.IND = BAS.HAREKETDEPOSU
            WHERE ISNULL(BAS.IPTAL,0)= 0 AND ISNULL(BAS.AK,0)= 0
            AND BAS.BELGETIPI = 128 and DP2.IND=" + DepoId;//File.ReadAllText(HostingEnvironment.MapPath(" /Sql/SqlVegaDb/Firma.sql"), System.Text.UTF8Encoding.Default);
                    string connString = "Provider=SQLOLEDB;Server=" + VegaDbIp + ";User Id=" + VegaDbSqlName + ";Password=" + VegaDbSqlPassword + ";Database=" + VegaDbName + "";
                    f.SqlConnOpen(true, connString);
                    DataTable AcikHesapDt = f.DataTable(Query, true);
                    if (AcikHesapDt.Rows.Count > 0)
                    {


                        foreach (DataRow SubeR in AcikHesapDt.Rows)
                        {
                            int BelgeId = 0;
                            string qr = "select * from Belge where BelgeTip=129 and BelgeNo =" + f.RTI(SubeR, "IND").ToString();

                            DataTable dtt = f.DataTable(qr);

                            if (dtt.Rows.Count > 0)
                            {
                                BelgeId = f.RTI(dtt.Rows[0], "Id");
                            }


                            items.Add(new BelgeDepoKabul
                            {
                                IND = f.RTI(SubeR, "IND"),
                                GONDERENDEPOKODU = f.RTS(SubeR, "GONDERENDEPOKODU").ToString(),
                                KABULEDECEKDEPOKODU = f.RTS(SubeR, "KABULEDECEKDEPOKODU").ToString(),
                                TARIH = Convert.ToDateTime(f.RTS(SubeR, "TARIH")),
                                FISNO = f.RTS(SubeR, "FISNO").ToString(),
                                OZELKOD3 = f.RTS(SubeR, "OZELKOD3").ToString(),
                                OZELKOD4 = f.RTS(SubeR, "OZELKOD4").ToString(),
                                BelgeID = BelgeId
                            });

                        }
                    }
                }
                f.SqlConnClose();
            }
            catch (System.Exception ex) { }

            return items;
        }


        public List<SubeSettingsViewModel> GetSubeList()
        {
            var liste = new List<SubeSettingsViewModel>();
            modelFunctions.SqlConnOpen();
            var dt = modelFunctions.DataTable("Select Id, SubeName,DBName, * From SubeSettings Where AppDbTypeStatus in (0,1) ");
            foreach (DataRow r in dt.Rows)
            {
                SubeSettingsViewModel model = new SubeSettingsViewModel();
                model.ID = Convert.ToInt32(modelFunctions.RTS(r, "Id"));
                model.SubeName = (modelFunctions.RTS(r, "SubeName"));
                //model.SubeIP = modelFunctions.RTS(r, "SubeIP");
                //model.SqlName = modelFunctions.RTS(r, "SqlName");
                //model.SqlPassword = modelFunctions.RTS(r, "SqlPassword");
                model.DBName = modelFunctions.RTS(r, "DBName");
                //model.FirmaID = modelFunctions.RTS(r, "FirmaID");                
                //model.Status = Convert.ToBoolean(modelFunctions.RTS(r, "Status"));
                //model.AppDbType = Convert.ToInt32(modelFunctions.RTS(r, "AppDbType"));
                //model.AppDbTypeStatus = Convert.ToBoolean(modelFunctions.RTS(r, "AppDbTypeStatus"));
                liste.Add(model);
            }
            modelFunctions.SqlConnClose();


            return liste;
        }

        public UrunEditViewModel GetByIdForEdit(int SubeId)
        {
            var modelSube = SqlData.GetSube(SubeId);
            DataTable dataSubeUrunlist = modelFunctions.GetSubeDataWithQuery((modelFunctions.NewConnectionString(modelSube.SubeIP, modelSube.DBName, modelSube.SqlName, modelSube.SqlPassword)), SqlData.getProductSqlQuery());

            var urunList = new UrunEditViewModel();
            urunList.UrunEditList = new List<UrunEdit>();
            urunList.SubeId = SubeId;
            urunList.SubeAdi = modelSube.SubeName;

            foreach (DataRow item in dataSubeUrunlist.Rows)
            {
                UrunEdit model = new UrunEdit();
                model.Id = modelFunctions.RTI(item, "Id");
                model.InvoiceName = modelFunctions.RTS(item, "InvoiceName");
                //model.Order = modelFunctions.RTS(item, "[Order]");
                model.Plu = modelFunctions.RTS(item, "Plu");
                model.Price = modelFunctions.RTD(item, "Price");
                model.ProductCode = modelFunctions.RTS(item, "ProductCode");
                model.ProductGroup = modelFunctions.RTS(item, "ProductGroup");
                model.ProductName = modelFunctions.RTS(item, "ProductName");
                model.ProductType = modelFunctions.RTS(item, "ProductType");
                model.VatRate = modelFunctions.RTD(item, "VatRate");
                urunList.UrunEditList.Add(model);
            }

            return urunList;
        }

        public ActionResultMessages Update(UrunEditViewModel model)
        {
            var result = new ActionResultMessages()
            {
                IsSuccess = true,
                UserMessage = "İşlem Başarılı",
            };

            var modelSube = SqlData.GetSube(model.SubeId);
            var sqlData = new SqlData(new SqlConnection(modelFunctions.NewConnectionString(modelSube.SubeIP, modelSube.DBName, modelSube.SqlName, modelSube.SqlPassword)));
            var dataSubeUrunlist = modelFunctions.GetSubeDataWithQuery((modelFunctions.NewConnectionString(modelSube.SubeIP, modelSube.DBName, modelSube.SqlName, modelSube.SqlPassword)), SqlData.getProductSqlQuery());

            foreach (var modelList in model.UrunEditList)
            {
                foreach (DataRow dataUrunSube in dataSubeUrunlist.Rows)
                {
                    if (modelList.Id == modelFunctions.RTI(dataUrunSube, "Id") && modelList.Price != modelFunctions.RTD(dataUrunSube, "Price"))
                    {
                        sqlData.ExecuteSql("update Product set Price = @par1 Where Id = @par2", new object[] { modelList.Price, modelList.Id });
                        break;
                    }
                }
            }

            return result;
        }
    }
}