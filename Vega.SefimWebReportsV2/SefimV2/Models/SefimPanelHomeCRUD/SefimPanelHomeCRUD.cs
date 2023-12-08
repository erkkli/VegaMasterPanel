using SefimV2.Helper;
using SefimV2.ViewModels.SefimPanelHome;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Configuration;

namespace SefimV2.Models.SefimPanelHomeCRUD
{
    public class SefimPanelHomeCRUD
    {
        #region Config local copy db connction setting       
        ModelFunctions mF = new ModelFunctions();
        static readonly string subeIp = WebConfigurationManager.AppSettings["Server"];
        static readonly string dbName = WebConfigurationManager.AppSettings["DBName"];
        static readonly string sqlKullaniciName = WebConfigurationManager.AppSettings["User"];
        static readonly string sqlKullaniciPassword = WebConfigurationManager.AppSettings["Password"];
        static readonly string ServerAktifMi = WebConfigurationManager.AppSettings["ServerAktifMi"];
        #endregion

        public List<MainViewModel> GetSubeList(string kullaniciId)
        {
            var liste = new List<MainViewModel>();
            var branchAccessList = new List<BranchAccess>();

            try
            {
                var kullaniciSubeYetkisi = UsersListCRUD.KullaniciSubeYetkiListesi(kullaniciId);

                mF.SqlConnOpen();
                DataTable subeList = mF.DataTable("Select Id,SubeName,DBName,SubeIP,SqlName,SqlPassword, * From SubeSettings Where AppDbTypeStatus in (0,1) and Status =1");
                mF.SqlConnClose();

                var locked = new Object();
                var dtList = subeList.AsEnumerable().ToList<DataRow>();
                Parallel.ForEach(dtList, localSube =>
                {
                    var id = mF.RTS(localSube, "Id");
                    if (kullaniciSubeYetkisi != null && kullaniciSubeYetkisi.FR_SubeListesi.Where(x => x.SubeID == Convert.ToInt32(id)).Select(x => x.SubeID).Any())
                    {
                        var subeName = mF.RTS(localSube, "SubeName");
                        var subeIp_ = mF.RTS(localSube, "SubeIP");
                        var dbName_ = mF.RTS(localSube, "DBName");
                        var sqlKullaniciName_ = mF.RTS(localSube, "SqlName");
                        var sqlKullaniciPassword_ = mF.RTS(localSube, "SqlPassword");

                        if (string.IsNullOrEmpty(ServerAktifMi) && ServerAktifMi == "1")
                        {
                            var bussinessHelper = new BussinessHelper();
                            branchAccessList = bussinessHelper.GetBranchAccessList();

                            try
                            {
                                foreach (var branchAccess in branchAccessList)
                                {
                                    MainViewModel model = new MainViewModel
                                    {
                                        Id = id
                                    };

                                    if (model.Id == branchAccess._id)
                                    {
                                        model.SubeName = subeName;
                                        model.SonAktifZamani = branchAccess.time;
                                        model.AktifMi = "Evet";
                                        lock (locked)
                                        {
                                            liste.Add(model);
                                        }
                                    }

                                //if (!liste.Where(x => x.Id == branchAccess._id).Any())
                                //{
                                //    model.SubeName = mF.RTS(localSube, "SubeName");
                                //    model.SonAktifZamani = branchAccess.time;
                                //    model.AktifMi = "Hayır";
                                //    liste.Add(model);
                                //}
                            }

                            }
                            catch (Exception ex)
                            {
                                Singleton.WritingLogFile2("SefimPanelHomeCRUD_ServerAktifMi_1", ex.ToString(), null, ex.StackTrace);
                            }
                        }
                        else
                        {
                            MainViewModel model = new MainViewModel
                            {
                                SubeName = subeName,
                                Id = id
                            };

                            try
                            {
                                mF.SqlConnOpen();
                                var query = "SELECT Count(name)  as DbName FROM master.dbo.sysdatabases WHERE name='" + dbName_ + "'";
                                var dbAktifMi = mF.GetSubeDataWithQuery(mF.NewConnectionString(subeIp_, dbName_, sqlKullaniciName_, sqlKullaniciPassword_), query);
                                mF.SqlConnClose();

                                foreach (DataRow item in dbAktifMi.Rows)
                                {
                                    var dbVarMi = mF.RTI(item, "DbName");
                                    if (dbVarMi == 1)
                                    {
                                        model.SonAktifZamani = DateTime.Now;
                                        model.AktifMi = "Evet";
                                        lock (locked)
                                        {
                                            liste.Add(model);
                                        }
                                    }
                                    else
                                    {
                                        model.SonAktifZamani = DateTime.Now;
                                        model.AktifMi = "Hayır";
                                        lock (locked)
                                        {
                                            liste.Add(model);
                                        }
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                Singleton.WritingLogFile2("SefimPanelHomeCRUD_ServerAktifMi_0", ex.ToString(), null, ex.StackTrace);
                                model.SonAktifZamani = DateTime.Now;
                                model.AktifMi = "Hayır";
                                liste.Add(model);
                            }
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                Singleton.WritingLogFile2("SefimPanelHomeCRUD_GetSubeList", ex.ToString(), null, ex.StackTrace);               
            }
            return liste;
        }
    }
}