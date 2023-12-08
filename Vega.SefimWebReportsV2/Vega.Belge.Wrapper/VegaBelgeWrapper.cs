using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Vega.Belge.Wrapper
{
    public class VegaBelgeWrapper : IDisposable
    {
        private const int GELIR = 5;
        private const int GIDER = 6;
        private const int TAHSILAT = 13;
        private const int TAHSILATIADE = 12;
        /*satış*/
        private const int SATFAT = 21;
        private const int SATIRS = 27;
        private const int STOKCIKIS = 33;

        /*satış iade*/
        private const int SATFATIADE = 23;
        private const int SATIRSIADE = 29;
        private const int STOKCIKISIADE = 35;

        /*alış*/
        private const int ALFAT = 20;
        private const int ALIRS = 26;
        private const int STOKGIRIS = 32;
        /*alış iade*/
        private const int ALFATIADE = 22;
        private const int ALIRSIADE = 28;
        private const int STOKGIRISIADE = 34;

        private const int SAYIMGIRIS = 93;
        private const int SAYIMCIKIS = 94;
        private const int URETIMGIRIS = 96;
        private const int URETIMCIKIS = 97;
        private const int MALIYET = 99;
        private const int TALEP = 135;
        private const int DEPOTRANSFER = 128;
        private const int DEPOTRANSFERKABUL = 129;
        private const int GIDERFATURASI = 165;

        const string REGSATFAT = "Software\\Vega\\Vegawin\\BelgeOptions\\Tfrmsatisfaturasi2";
        const string REGSTOKCIKIS = "Software\\Vega\\Vegawin\\BelgeOptions\\Tfrmstokcikisfisi2";
        const string REGSATIRS = "Software\\Vega\\Vegawin\\BelgeOptions\\Tfrmsatisirsaliyesi2";

        const string REGALFAT = "Software\\Vega\\Vegawin\\BelgeOptions\\Tfrmalisfaturasi2";
        const string REGSTOKGIRIS = "Software\\Vega\\Vegawin\\BelgeOptions\\Tfrmstokgirisfisi2";
        const string REGALIRS = "Software\\Vega\\Vegawin\\BelgeOptions\\Tfrmalisirsaliyesi2";

        const string REGTAHSIL = "Software\\Vega\\Vegawin\\CariBelgeOptions\\TfrmCariGirisBordrosu";
        const string REGTAHSILIADE = "Software\\Vega\\Vegawin\\CariBelgeOptions\\TfrmCariCikisBordrosu";

        const string REGURETIM = "Software\\Vega\\Vegawin\\CariBelgeOptions\\Tfrmuretimhareketi";
        const string REGSAYIMGIR = "Software\\Vega\\Vegawin\\CariBelgeOptions\\Tfrmsayimgirisfisi";
        const string REGSAYIMCIK= "Software\\Vega\\Vegawin\\CariBelgeOptions\\Tfrmsayimcikisfisi";

        const string REGTALEP = "Software\\Vega\\Vegawin\\CariBelgeOptions\\Tfrmtalepfisi";


        private readonly string firmaPrefix;
        private readonly string donemPrefix;
        private readonly int userNo;
        private string headerColumn = null;
        private string headerValue = null;
        private string itemColumn = null;
        private string itemValue = null;
        private VegaBelgeBaslik globalBaslik = null;
        private VegaBelgeHareket globalHareket = null;
        private VegaBelgeKasa globalKasa = null;
        private string BelgeNo = null;
        private decimal iskontoOran = 0;
        private bool isDelete = false;
        private int izahat = 0;



        bool disposed = false;
        private readonly SqlConnection connection;
        //private readonly SqlConnection masterConnection;
        private readonly SqlTransaction sqlTransaction;
        //ORTAK
        SqlCommand c;
        //ORTAK


        SqlCommand cmdSatFatIrsStokUretSayBaslik;
        SqlCommand cmdSatFatIrsStokUretSayHareket;
        SqlCommand cmdAlFatIrsStokBaslik;
        SqlCommand cmdAlFatIrsStokHareket;
        SqlCommand cmdHareketSatirNo;
        SqlCommand cmdCariGenel;
        SqlCommand cmdCariHareketleri;
        SqlCommand cmdStokHareket;
        SqlCommand cmdDepoEnvater;
        SqlCommand cmdDepoEnvaterTers;
        SqlCommand cmdGetSeriNo;
        SqlCommand cmdUpdateSatFatIrsStokUretSayBaslik;
        SqlCommand cmdUpdateSatFatIrsStokUretSayHareket;
        SqlCommand cmdSayimDosyasi;
        SqlCommand cmdDepoTransferBaslik;
        SqlCommand cmdDepoTransferHareket;
        SqlCommand cmdUpdateDepoTransferBaslik;
        SqlCommand cmdDepoTransferKabulBaslik;
        SqlCommand cmdDepoTransferKabulHareket;
        SqlCommand cmdUpdateDepoTransferKabulBaslik;
        SqlCommand cmdDepoTransferKabulBaslikForDepoTransfer;
        SqlCommand cmdGetBelgeBaslik;
        SqlCommand cmdGetBelgeHareket;
        //sayımdosyası

        SqlCommand cmdUpdateAlFatIrsStokBaslik;
        SqlCommand cmdUpdateAlFatIrsStokHareket;

        SqlCommand cmdUpdateCariGenel;
        SqlCommand cmdUpdateCariHareket;
        SqlCommand cmdUpdateStokHareket;
        SqlCommand cmdStokGetir;
        SqlDataAdapter adpFatIrsStokHareketSatirlariGetir;
        SqlCommand cmdCariOpsiyon;

        SqlCommand cmdDeleteFatIrsStokBaslik;
        SqlCommand cmdDeleteFatIrsStokHareket;
        SqlCommand cmdDeleteCariGenel;
        SqlCommand cmdDeleteCariHareketleri;
        SqlCommand cmdDeleteStokHareket;
        SqlCommand cmdDeleteDepoEnvanter;
        SqlCommand cmdDeleteSayimDosyasi;

        SqlCommand cmdTahsilBaslik;
        SqlCommand cmdTahsilHareket;
        SqlCommand cmdTahsilCariGenel;
        SqlCommand cmdTahsilCariHareket;
        SqlCommand cmdTahsilVisaGiris;
        SqlCommand cmdTahsilVisaPortfoy;
        SqlCommand cmdTahsilNakitCekSenet;
        SqlCommand cmdUpdateTahsilVisaPortfoy;
        SqlCommand cmdUpdateTahsilBaslik;
        SqlCommand cmdUpdateTahsilCariHareket;

        SqlCommand cmdDeleteTahsilBaslik;
        SqlCommand cmdDeleteTahsilHareket;
        SqlCommand cmdDeleteTahsilCariGenel;
        SqlCommand cmdDeleteTahsilCariHareketleri;
        SqlCommand cmdDeleteTahsilStokHareket;
        SqlCommand cmdDeleteTahsilDepoEnvanter;
        SqlCommand cmdDeleteTahsilKasa;
        SqlCommand cmdDeleteTahsilVisaGiris;
        SqlCommand cmdDeleteTahsilVisaPortfoy=new SqlCommand("");
        SqlDataAdapter getVisaGirisForDeletePortfoy;

        SqlCommand cmdKasaIslemi;
        SqlCommand cmdDeleteKasa;

        SqlCommand cmdAlSipBaslik;
        SqlCommand cmdAlSipHareket;
        SqlCommand cmdAlSipList;
        SqlCommand cmdUpdateTalepSipBaslik;

        public VegaBelgeWrapper(string connectionString, string firmaPrefix, string donemPrefix, int userNo)
        {
            connection = new SqlConnection(connectionString);
            //connection = _connection;
            this.firmaPrefix = firmaPrefix;
            this.donemPrefix = donemPrefix;
            this.userNo = userNo;
            if (connection.State != ConnectionState.Open)
                connection.Open();
            sqlTransaction = connection.BeginTransaction();
        }

        #region/*EVRAK İŞLEMLERİ METOD*/
        public void BelgeEkle(VegaBelgeBaslik belgeBaslik) 
        {
            try
            {
                globalBaslik = belgeBaslik;
                switch (globalBaslik.Izahat)
                {
                    case SATFAT:
                        SatFatBaslik();
                        break;
                    case SATFATIADE:
                        SatFatBaslik();
                        break;
                    case SATIRS:
                        SatIrsBaslik();
                        break;
                    case SATIRSIADE:
                        SatIrsBaslik();
                        break;
                    case STOKCIKIS:
                        StokCikisBaslik();
                        break;
                    case STOKCIKISIADE:
                        StokCikisBaslik();
                        break;
                    case ALFAT:
                        AlFatBaslik();
                        break;
                    case GIDERFATURASI:
                        AlFatBaslik();
                        break;
                    case ALIRS:
                        AlIrsBaslik();
                        break;
                    case STOKGIRIS:
                        StokGirisBaslik();
                        break;
                    case ALFATIADE:
                        AlFatBaslik();
                        break;
                    case ALIRSIADE:
                        AlIrsBaslik();
                        break;
                    case STOKGIRISIADE:
                        StokGirisBaslik();
                        break;
                    case TAHSILAT:
                        TahsilatBaslik();
                        break;
                    case TAHSILATIADE:
                        TahsilatIadeBaslik();
                        break;
                    case SAYIMGIRIS:
                        SayimGirisBaslik();
                        break;
                    case SAYIMCIKIS:
                        SayimCikisBaslik();
                        break;
                    case URETIMGIRIS:
                    case URETIMCIKIS:
                        UretimBaslik();
                        break;
                    case GELIR:
                        break;
                    case GIDER:
                        break;
                    case MALIYET:
                        break;
                    case TALEP:
                        TalepBaslik();
                        break;
                    case DEPOTRANSFER:
                        DepoTransferBaslik();
                        break;
                    case DEPOTRANSFERKABUL:
                        DepoTransferKabulBaslik();
                        break;
                }
            }
            catch (Exception e)
            {
                throw e;
            }
        }
        public bool BelgeSil(int docNum,int _izahat) 
        {
            try
            {
                izahat = _izahat;
                isDelete = true;
                switch (izahat)
                {
                    case 21:
                    case 22:
                    case 27:
                    case 28:
                    case 33:
                    case 34:
                    case 93:
                    case 94:
                        DeleteFatIrsStokUretSay(docNum);
                        break;
                    case 12:
                    case 13:
                        DeleteTahsil(docNum,izahat);
                        break;
                    case 96:
                    case 97:
                        DeleteUretim(docNum, izahat);
                        break;
                    case 160:
                        DeleteKasa(docNum,izahat);
                        break;
                }

                sqlTransaction.Commit();
                return true;
            }
            catch (Exception e)
            {
                sqlTransaction.Rollback();
                throw e;
            }
        }
        public string BelgeKaydet() 
        {
            try
            {
                switch (globalBaslik.Izahat)
                {
                    case SATFAT:
                        UpdateSatFatIrsStokUretSay();  
                        UpdateSatFatBaslik();
                        break;
                    case SATFATIADE:
                        UpdateSatFatIrsStokUretSay();
                        UpdateSatFatBaslik();
                        break;
                    case SATIRS:
                        UpdateSatFatIrsStokUretSay();
                        UpdateSatIrsBaslik();
                        break;
                    case SATIRSIADE:
                        UpdateSatFatIrsStokUretSay();
                        UpdateSatIrsBaslik();
                        break;
                    case STOKCIKIS:
                        UpdateSatFatIrsStokUretSay();
                        UpdateStokCikisBaslik();
                        break;
                    case STOKCIKISIADE:
                        UpdateSatFatIrsStokUretSay();
                        UpdateStokCikisBaslik();
                        break;
                    case ALFAT:
                        UpdateAlFatIrsStok();//kapalıydı açıldı
                        UpdateAlFatBaslik();//yoktu eklendi
                        break;
                    case GIDERFATURASI:
                        UpdateAlFatIrsStok();//kapalıydı açıldı
                        UpdateAlFatBaslik();//yoktu eklendi
                        break;
                    case ALIRS:
                        UpdateAlFatIrsStok();//yoktu eklendi.
                        UpdateAlIrsBaslik();//yoktu eklendi
                        break;
                    case STOKGIRIS:
                        break;
                    case ALFATIADE:
                        UpdateAlFatIrsStok();
                        UpdateAlFatBaslik();
                        break;
                    case ALIRSIADE:
                        UpdateAlFatIrsStok();
                        UpdateAlIrsBaslik();
                        break;
                    case STOKGIRISIADE:
                        UpdateAlFatIrsStok();
                        UpdateStokGirisBaslik();
                        break;
                    case TAHSILAT:
                        UpdateTahsilBaslik();
                        break;
                    case TAHSILATIADE:
                        UpdateTahsilIadeBaslik();
                        break;
                    case SAYIMGIRIS:
                        UpdateSatFatIrsStokUretSay();
                        UpdateSayimGirisBaslik();
                        break;
                    case SAYIMCIKIS:
                        UpdateSatFatIrsStokUretSay();
                        UpdateSayimCikisBaslik();
                        break;
                    case URETIMGIRIS:
                    case URETIMCIKIS:
                        UpdateSatFatIrsStokUretSay();
                        UpdateUretimBaslik();
                        break;
                    case GELIR:
                        break;
                    case GIDER:
                        break;
                    case MALIYET:
                        break;
                    case TALEP:
                        UpdateTalepBaslik();
                        break;
                    case DEPOTRANSFER:
                        UpdateDepoTransferBaslik();
                        break;
                    case DEPOTRANSFERKABUL:
                        UpdateDepoTransferKabulBaslik();
                        break;
                }
                sqlTransaction.Commit();
                return globalBaslik.BelgeNo +"*"+globalBaslik.BaslikInd;
            }
            catch (Exception e)
            {
                sqlTransaction.Rollback();
                throw e;
            }
            finally
            {
                connection.Close();
                //Dispose();
            }
        }
       
        public void SatirEkle(VegaBelgeHareket belgeHareket) 
        {
            try
            {
                globalHareket = belgeHareket;
                switch (globalBaslik.Izahat)
                {
                    case SATFAT:
                        SatFatHareket();
                        break;
                    case SATFATIADE:
                        SatFatHareket();
                        break;
                    case SATIRS:
                        SatIrsHareket();
                        break;
                    case SATIRSIADE:
                        SatIrsHareket();
                        break;
                    case STOKCIKIS:
                        StokCikisHareket();
                        break;
                    case STOKCIKISIADE:
                        StokCikisHareket();
                        break;
                    case ALFAT:
                        AlFatHareket();
                        break;
                    case GIDERFATURASI:
                        AlFatHareket();
                        break;
                    case ALIRS:
                        AlIrsHareket();
                        break;
                    case STOKGIRIS:
                        StokGirisHareket();
                        break;
                    case ALFATIADE:
                        AlFatHareket();
                        break;
                    case ALIRSIADE:
                        AlIrsHareket();
                        break;
                    case STOKGIRISIADE:
                        StokGirisHareket();
                        break;
                    case TAHSILAT:
                        TahsilatHareket();
                        break;
                    case TAHSILATIADE:
                        TahsilatIadeHareket();
                        break;
                    case SAYIMGIRIS:
                        SayimGirisHareket();
                        break;
                    case SAYIMCIKIS:
                        SayimCikisHareket();
                        break;
                    case URETIMGIRIS:
                    case URETIMCIKIS:
                        UretimHareket();
                        break;
                    case GELIR:
                        break;
                    case GIDER:
                        break;
                    case MALIYET:
                        break;
                    case TALEP:
                        TalepHareket();
                        break;
                    case DEPOTRANSFER:
                        DepoTransferHareket();
                        break;
                    case DEPOTRANSFERKABUL:
                        DepoTransferKabulHareket();
                        break;
                }
            }
            catch (Exception e)
            {
                throw e;
            }
        }
        public void BaslikAlanEkle(string column,string value) 
        {
            try
            {
                if (String.IsNullOrEmpty(column) && String.IsNullOrEmpty(value))
                {
                    throw new Exception("Kolon veya değer boş olamaz");
                }
                else 
                {
                    headerColumn += "," + column;
                    headerValue += "$'" + value + "'";
                }
            }
            catch (Exception e)
            {
                throw e;
            }
        }
        public void HareketAlaniEkle(string column,string value)
        {
            try
            {
                if (String.IsNullOrEmpty(column) && String.IsNullOrEmpty(value))
                {
                    throw new Exception("Kolon veya değer boş olamaz");
                }
                else
                {
                    itemColumn += "," + column;
                    itemValue += "$'" + value + "'";
                }
            }
            catch (Exception e)
            {
                throw e;
            }
        }
        #endregion/*EVRAK İŞLEMLERİ METOD*/

        #region/*FATIRSSTOK*/
        private void DeleteFatIrsStokUretSay(int docNum) 
        {
            if (connection.State != ConnectionState.Open)
                connection.Open();
            try
            {
                var tableBaslik = "";
                var tableHareket = "";
                switch (izahat)
                {
                    case 21:
                        tableBaslik = "TBLSATFATBASLIK";
                        tableHareket = "TBLSATFATHAREKET";
                        break;
                    case 22:
                        tableBaslik = "TBLALFATBASLIK";
                        tableHareket = "TBLALFATHAREKET";
                        break;
                    case 27:
                        tableBaslik = "TBLSATIRSBASLIK";
                        tableHareket = "TBLSATIRSHAREKET";
                        break;
                    case 28:
                        tableBaslik = "TBLALIRSBASLIK";
                        tableHareket = "TBLALIRSHAREKET";
                        break;
                    case 33:
                        tableBaslik = "TBLSTKCIKBASLIK";
                        tableHareket = "TBLSTKCIKHAREKET";
                        break;
                    case 34:
                        tableBaslik = "TBLSTKGIRBASLIK";
                        tableHareket = "TBLSTKGIRHAREKET";
                        break;
                    case 93:
                        tableBaslik = "TBLSAYIMGIRISBASLIK";
                        tableHareket = "TBLSAYIMGIRISHAREKET";
                        break;
                    case 94:
                        tableBaslik = "TBLSAYIMCIKISBASLIK";
                        tableHareket = "TBLSAYIMCIKISHAREKET";
                        break;
                }
                cmdDeleteFatIrsStokBaslik = new SqlCommand("UPDATE " + firmaPrefix + donemPrefix + tableBaslik + " SET IPTAL=1 WHERE IND=@p1", connection) { Transaction = sqlTransaction };
                cmdDeleteFatIrsStokBaslik.Parameters.AddWithValue("@p1", docNum);
                cmdDeleteFatIrsStokBaslik.ExecuteNonQuery();

                cmdDeleteFatIrsStokHareket = new SqlCommand("DELETE FROM "+firmaPrefix+donemPrefix+tableHareket+" WHERE EVRAKNO=@p1", connection) { Transaction = sqlTransaction };
                cmdDeleteFatIrsStokHareket.Parameters.AddWithValue("@p1", docNum);
                cmdDeleteFatIrsStokHareket.ExecuteNonQuery();

                cmdDeleteCariGenel = new SqlCommand("DELETE FROM " + firmaPrefix + donemPrefix + "TBLCARIGENELHAREKET WHERE BELGEIND=@p1 and BELGEIZAHAT=@p2", connection) { Transaction = sqlTransaction };
                cmdDeleteCariGenel.Parameters.AddWithValue("@p1", docNum);
                cmdDeleteCariGenel.Parameters.AddWithValue("@p2", izahat);
                cmdDeleteCariGenel.ExecuteNonQuery();

                cmdDeleteCariHareketleri = new SqlCommand("DELETE FROM " + firmaPrefix + donemPrefix + "TBLCARIHAREKETLERI WHERE LN=@p1 and IZAHAT=@p2", connection) { Transaction = sqlTransaction };
                cmdDeleteCariHareketleri.Parameters.AddWithValue("@p1", docNum);
                cmdDeleteCariHareketleri.Parameters.AddWithValue("@p2", izahat);
                cmdDeleteCariHareketleri.ExecuteNonQuery();

                cmdDeleteStokHareket = new SqlCommand("DELETE FROM " + firmaPrefix + donemPrefix + "TBLSTOKHAREKETLERI WHERE BELGENO=@p1 and IZAHAT=@p2", connection) { Transaction = sqlTransaction };
                cmdDeleteStokHareket.Parameters.AddWithValue("@p1", docNum);
                cmdDeleteStokHareket.Parameters.AddWithValue("@p2", izahat);
                cmdDeleteStokHareket.ExecuteNonQuery();

                cmdDeleteDepoEnvanter = new SqlCommand("DELETE FROM " + firmaPrefix + donemPrefix + "TBLDEPOENVANTER WHERE BELGEIND=@p1 and BELGETIPI=@p2", connection) { Transaction = sqlTransaction };
                cmdDeleteDepoEnvanter.Parameters.AddWithValue("@p1", docNum);
                cmdDeleteDepoEnvanter.Parameters.AddWithValue("@p2", izahat);
                cmdDeleteDepoEnvanter.ExecuteNonQuery();

                if (izahat == 93 || izahat == 94) 
                {
                    var cmdExists = new SqlCommand("SELECT COUNT(*) FROM sysobjects WHERE Name = '" + firmaPrefix + "TBLSAYIMDOSYALARI' AND xtype='U'", connection) { Transaction = sqlTransaction };
                    if ((int)cmdExists.ExecuteScalar() > 0) 
                    {
                        cmdDeleteSayimDosyasi = new SqlCommand("DELETE FROM " + firmaPrefix + "TBLSAYIMDOSYALARI WHERE SYMDOSYAADI=@p1", connection) { Transaction = sqlTransaction };
                        cmdDeleteSayimDosyasi.Parameters.AddWithValue("@p1", docNum);
                        cmdDeleteSayimDosyasi.ExecuteNonQuery();
                    }
                }

            }
            catch (Exception e)
            {
                throw e;
            }
        }
        #endregion/*FATIRSSTOK*/

        #region/*TAHSIL*/
        private void DeleteTahsil(int docNum,int izahat) 
        {
            if (connection.State != ConnectionState.Open)
                connection.Open();
            try
            {
                var tblBaslikName = "";
                var tblHareketName = "";
                switch (izahat)
                {
                    case 12:
                        tblBaslikName = "TBLCARCIKIADEBASLIK";
                        tblHareketName = "TBLCARCIKIADEHAREKET";
                        break;
                    case 13:
                        tblBaslikName = "TBLCARGIRBASLIK";
                        tblHareketName = "TBLCARGIRHAREKET";
                        break;
                }
                cmdDeleteTahsilBaslik = new SqlCommand("UPDATE " + firmaPrefix + donemPrefix + tblBaslikName + " SET IPTAL=1 WHERE IND=@p1", connection) { Transaction = sqlTransaction };
                cmdDeleteTahsilBaslik.Parameters.AddWithValue("@p1", docNum);
                cmdDeleteTahsilBaslik.ExecuteNonQuery();

                cmdDeleteTahsilHareket = new SqlCommand("DELETE FROM " + firmaPrefix + donemPrefix + tblHareketName+" WHERE EVRAKNO=@p1", connection) { Transaction = sqlTransaction };
                cmdDeleteTahsilHareket.Parameters.AddWithValue("@p1", docNum);
                cmdDeleteTahsilHareket.ExecuteNonQuery();

                cmdDeleteTahsilCariGenel = new SqlCommand("DELETE FROM " + firmaPrefix + donemPrefix + "TBLCARIGENELHAREKET WHERE BELGEIND=@p1 and BELGEIZAHAT=@p2", connection) { Transaction = sqlTransaction };
                cmdDeleteTahsilCariGenel.Parameters.AddWithValue("@p1", docNum);
                cmdDeleteTahsilCariGenel.Parameters.AddWithValue("@p2", izahat);
                cmdDeleteTahsilCariGenel.ExecuteNonQuery();

                cmdDeleteTahsilCariHareketleri = new SqlCommand("DELETE FROM " + firmaPrefix + donemPrefix + "TBLCARIHAREKETLERI WHERE LN=@p1 and IZAHAT=@p2", connection) { Transaction = sqlTransaction };
                cmdDeleteTahsilCariHareketleri.Parameters.AddWithValue("@p1", docNum);
                cmdDeleteTahsilCariHareketleri.Parameters.AddWithValue("@p2", izahat);
                cmdDeleteTahsilCariHareketleri.ExecuteNonQuery();

                cmdDeleteTahsilStokHareket = new SqlCommand("DELETE FROM " + firmaPrefix + donemPrefix + "TBLSTOKHAREKETLERI WHERE BELGENO=@p1 and IZAHAT=@p2", connection) { Transaction = sqlTransaction };
                cmdDeleteTahsilStokHareket.Parameters.AddWithValue("@p1", docNum);
                cmdDeleteTahsilStokHareket.Parameters.AddWithValue("@p2", izahat);
                cmdDeleteTahsilStokHareket.ExecuteNonQuery();

                cmdDeleteTahsilDepoEnvanter = new SqlCommand("DELETE FROM " + firmaPrefix + donemPrefix + "TBLDEPOENVANTER WHERE BELGEIND=@p1 and BELGETIPI=@p2", connection) { Transaction = sqlTransaction };
                cmdDeleteTahsilDepoEnvanter.Parameters.AddWithValue("@p1", docNum);
                cmdDeleteTahsilDepoEnvanter.Parameters.AddWithValue("@p2", izahat);
                cmdDeleteTahsilDepoEnvanter.ExecuteNonQuery();

                cmdDeleteTahsilKasa = new SqlCommand("DELETE FROM " + firmaPrefix + donemPrefix + "TBLKASA WHERE BELGELINK=@p1 AND BELGEIZAHAT=@p2", connection) { Transaction = sqlTransaction };
                cmdDeleteTahsilKasa.Parameters.AddWithValue("@p1", docNum);
                cmdDeleteTahsilKasa.Parameters.AddWithValue("@p2", izahat);
                cmdDeleteTahsilKasa.ExecuteNonQuery();


                switch (izahat)
                {
                    case 12:
                        break;
                    case 13:
                        getVisaGirisForDeletePortfoy = new SqlDataAdapter("SELECT IND FROM " + firmaPrefix + donemPrefix + "TBLVISAGIRIS WHERE EVRAKNO=@p1", connection);
                        getVisaGirisForDeletePortfoy.SelectCommand.Parameters.AddWithValue("@p1", docNum);
                        getVisaGirisForDeletePortfoy.SelectCommand.Transaction = sqlTransaction;
                        DataTable dt = new DataTable();
                        getVisaGirisForDeletePortfoy.Fill(dt);
                        foreach (DataRow row in dt.Rows)
                        {
                            cmdDeleteTahsilVisaPortfoy = new SqlCommand("DELETE FROM " + firmaPrefix + donemPrefix + "TBLVISAPORTFOY WHERE EVRAKNO=@p1", connection) { Transaction = sqlTransaction };
                            cmdDeleteTahsilVisaPortfoy.Parameters.AddWithValue("@p1", row[0]);
                            cmdDeleteTahsilVisaPortfoy.ExecuteNonQuery();
                        }

                        cmdDeleteTahsilVisaGiris = new SqlCommand("DELETE FROM " + firmaPrefix + donemPrefix + "TBLVISAGIRIS WHERE EVRAKNO=@p1", connection) { Transaction = sqlTransaction };
                        cmdDeleteTahsilVisaGiris.Parameters.AddWithValue("@p1", docNum);
                        cmdDeleteTahsilVisaGiris.ExecuteNonQuery();
                        break;
                }


            }
            catch (Exception e)
            {
                throw e;
            }
        }
        #endregion/*TAHSIL*/

        #region/*ÜRETİM*/
        private void DeleteUretim(int docNum, int izahat) 
        {
            if (connection.State != ConnectionState.Open)
                connection.Open();
            try
            {
                var tableBaslik = "";
                var tableHareket = "";
                switch (izahat)
                {
                    case 96:
                    case 97:
                        tableBaslik = "TBLSBASLIK";
                        tableHareket = "TBLSHAREKET";
                        break;

                }
                cmdDeleteFatIrsStokBaslik = new SqlCommand("UPDATE " + firmaPrefix + donemPrefix + tableBaslik + " SET IPTAL=1 WHERE IND=@p1", connection) { Transaction = sqlTransaction };
                cmdDeleteFatIrsStokBaslik.Parameters.AddWithValue("@p1", docNum);
                cmdDeleteFatIrsStokBaslik.ExecuteNonQuery();

                cmdDeleteFatIrsStokHareket = new SqlCommand("DELETE FROM " + firmaPrefix + donemPrefix + tableHareket + " WHERE EVRAKNO=@p1", connection) { Transaction = sqlTransaction };
                cmdDeleteFatIrsStokHareket.Parameters.AddWithValue("@p1", docNum);
                cmdDeleteFatIrsStokHareket.ExecuteNonQuery();

                cmdDeleteStokHareket = new SqlCommand("DELETE FROM " + firmaPrefix + donemPrefix + "TBLSTOKHAREKETLERI WHERE BELGENO=@p1 and IZAHAT=@p2", connection) { Transaction = sqlTransaction };
                cmdDeleteStokHareket.Parameters.AddWithValue("@p1", docNum);
                cmdDeleteStokHareket.Parameters.AddWithValue("@p2", izahat);
                cmdDeleteStokHareket.ExecuteNonQuery();

                cmdDeleteDepoEnvanter = new SqlCommand("DELETE FROM " + firmaPrefix + donemPrefix + "TBLDEPOENVANTER WHERE BELGEIND=@p1 and BELGETIPI=@p2", connection) { Transaction = sqlTransaction };
                cmdDeleteDepoEnvanter.Parameters.AddWithValue("@p1", docNum);
                cmdDeleteDepoEnvanter.Parameters.AddWithValue("@p2", izahat);
                cmdDeleteDepoEnvanter.ExecuteNonQuery();

            }
            catch (Exception e)
            {
                throw e;
            }
        }
        #endregion/*ÜRETİM*/

        #region/*SATFAT METOD*/
        private void SatFatBaslik()
        {
            if (connection.State != ConnectionState.Open)
                connection.Open();
            BelgeNo = BelgeNoGetir();
            globalBaslik.BelgeNo = BelgeNo;
            int giris= ((int)globalBaslik.BelgeAlSat);
            int iade=globalBaslik.Iade;
            //if (globalBaslik.BelgeAlSat == VegaBelgeHareketTipi.Cikis)
            //{
            //    giris = 0;
            //    iade = 0;
            //}
            //else
            //{
            //    giris = 1;
            //    iade = 1;
            //}


            try
            {
                var headerParameter = "";
                if (headerValue != null)
                {
                    var headerArr = headerValue.Split('$');
                    for (int i = 0; i < headerArr.Length; i++)
                    {
                        headerParameter += "," + headerArr[i];
                        //cmdSatFatIrsStokUretSayBaslik.Parameters.AddWithValue("@p" + (42 + i), headerArr[i]);
                    }
                    headerParameter = headerParameter.TrimStart(',');
                    headerParameter = "," + headerParameter;
                }
                
                cmdSatFatIrsStokUretSayBaslik = new SqlCommand("INSERT INTO " + firmaPrefix + donemPrefix + "TBLSATFATBASLIK " +

                    "(BELGENO,TARIH,KDV,ODEMETARIHI,ALT1,DEPO,OZELKOD1,OZELKOD2,CREDATE,LADATE,FIRMANO,GIRIS,BELGETIPI,USERNO,OZELKOD3,OZELKOD4,OZELKOD5,OZELKOD6,OZELKOD7,OZELKOD8,OZELKOD9,HAREKETDEPOSU,UID,PARABIRIMI,KUR,YUVARLAMA,ALLOWYUVARLAMA,IADE,IPTAL,AK,    ODMODIFIED,CONVERTED,ENTEGRE,SATISSEKLI,YURTDISI,MUHASEBELESMEYECEK,KAYNAK,EFATURA,STOKHAREKETEYAZ,CARIHAREKETEYAZ,YAZARKASAFISI " + headerColumn + ") OUTPUT INSERTED.IND VALUES" +
                    "(@p1,@p2,@p3,@p4,@p5,@p6,@p7,@p8,@p9,@p10,@p11,@p12,@p13,@p14,@p15,@p16,@p17,@p18,@p19,@p20,@p21,@p22,@p23,@p24,@p25,@p26,@p27,@p28,@p29,@p30, @p31,@p32,@p33,@p34,@p35,@p36,@p37,@p38,@p39,@p40,@p41 " + headerParameter+")", connection)
                {
                    Transaction = sqlTransaction
                };
                cmdSatFatIrsStokUretSayBaslik.Parameters.AddWithValue("@p1", BelgeNo);
                cmdSatFatIrsStokUretSayBaslik.Parameters.AddWithValue("@p2", globalBaslik.Tarih.Date);
                cmdSatFatIrsStokUretSayBaslik.Parameters.AddWithValue("@p3", globalBaslik.KdvDahil);
                cmdSatFatIrsStokUretSayBaslik.Parameters.AddWithValue("@p4", globalBaslik.Tarih.Date);
                cmdSatFatIrsStokUretSayBaslik.Parameters.AddWithValue("@p5", globalBaslik.Iskonto);
               
                    cmdSatFatIrsStokUretSayBaslik.Parameters.AddWithValue("@p6", (object)globalBaslik.DepoNo ?? DBNull.Value);
                
                cmdSatFatIrsStokUretSayBaslik.Parameters.AddWithValue("@p7", globalBaslik.OzelKod1??"");
                cmdSatFatIrsStokUretSayBaslik.Parameters.AddWithValue("@p8", globalBaslik.OzelKod2??"");
                cmdSatFatIrsStokUretSayBaslik.Parameters.AddWithValue("@p9", DateTime.Now);
                cmdSatFatIrsStokUretSayBaslik.Parameters.AddWithValue("@p10", DateTime.Now);
                cmdSatFatIrsStokUretSayBaslik.Parameters.AddWithValue("@p11", globalBaslik.CariNo);
                cmdSatFatIrsStokUretSayBaslik.Parameters.AddWithValue("@p12", giris);
                cmdSatFatIrsStokUretSayBaslik.Parameters.AddWithValue("@p13", globalBaslik.Izahat);
                cmdSatFatIrsStokUretSayBaslik.Parameters.AddWithValue("@p14", userNo);
                cmdSatFatIrsStokUretSayBaslik.Parameters.AddWithValue("@p15", globalBaslik.OzelKod3??"");
                cmdSatFatIrsStokUretSayBaslik.Parameters.AddWithValue("@p16", globalBaslik.OzelKod4??"");
                cmdSatFatIrsStokUretSayBaslik.Parameters.AddWithValue("@p17", globalBaslik.OzelKod5??"");
                cmdSatFatIrsStokUretSayBaslik.Parameters.AddWithValue("@p18", globalBaslik.OzelKod6??"");
                cmdSatFatIrsStokUretSayBaslik.Parameters.AddWithValue("@p19", globalBaslik.OzelKod7??"");
                cmdSatFatIrsStokUretSayBaslik.Parameters.AddWithValue("@p20", globalBaslik.OzelKod8??"");
                cmdSatFatIrsStokUretSayBaslik.Parameters.AddWithValue("@p21", globalBaslik.OzelKod9??"");
                cmdSatFatIrsStokUretSayBaslik.Parameters.AddWithValue("@p22", globalBaslik.HareketDeposu);
                cmdSatFatIrsStokUretSayBaslik.Parameters.AddWithValue("@p23", "{" + Guid.NewGuid() + "}");
                cmdSatFatIrsStokUretSayBaslik.Parameters.AddWithValue("@p24", "TL");
                cmdSatFatIrsStokUretSayBaslik.Parameters.AddWithValue("@p25", 1);
                cmdSatFatIrsStokUretSayBaslik.Parameters.AddWithValue("@p26", 0);
                cmdSatFatIrsStokUretSayBaslik.Parameters.AddWithValue("@p27", 0);
                cmdSatFatIrsStokUretSayBaslik.Parameters.AddWithValue("@p28", iade);
                cmdSatFatIrsStokUretSayBaslik.Parameters.AddWithValue("@p29", 0);
                cmdSatFatIrsStokUretSayBaslik.Parameters.AddWithValue("@p30", 0);

                cmdSatFatIrsStokUretSayBaslik.Parameters.AddWithValue("@p31", 0);
                cmdSatFatIrsStokUretSayBaslik.Parameters.AddWithValue("@p32", 0);
                cmdSatFatIrsStokUretSayBaslik.Parameters.AddWithValue("@p33", 0);
                cmdSatFatIrsStokUretSayBaslik.Parameters.AddWithValue("@p34", 0);
                cmdSatFatIrsStokUretSayBaslik.Parameters.AddWithValue("@p35", 0);
                cmdSatFatIrsStokUretSayBaslik.Parameters.AddWithValue("@p36", 0);
                cmdSatFatIrsStokUretSayBaslik.Parameters.AddWithValue("@p37", 0);
                cmdSatFatIrsStokUretSayBaslik.Parameters.AddWithValue("@p38", globalBaslik.EFatura);
                cmdSatFatIrsStokUretSayBaslik.Parameters.AddWithValue("@p39", 1);
                cmdSatFatIrsStokUretSayBaslik.Parameters.AddWithValue("@p40", 1);
                cmdSatFatIrsStokUretSayBaslik.Parameters.AddWithValue("@p41", 0);


                globalBaslik.BelgeNo = BelgeNo;

                globalBaslik.BaslikInd = (int)cmdSatFatIrsStokUretSayBaslik.ExecuteScalar();
                InsertSatFatIrsStokUretSay1();
            }
            catch (Exception e)
            {
                throw e;
            }
        }
        private void SatFatHareket()
        {
            if (connection.State != ConnectionState.Open)
                connection.Open();
            try
            {
                var SatirNo = HareketSatirSayisi();
                var stm = StokTipVeMaliyet(globalHareket.StokNo).Split(';');
                var StokTip = stm[0] == "" ? 0 : Convert.ToInt32(stm[0]);
                var Maliyet = stm[1] == "" ? 0 : Convert.ToDecimal(stm[1]);
                var res = StokBirimVeKdv(globalHareket.BirimNo).Split(';');
                var Birim = res[0].Length>5?res[0].Substring(0,5):res[0];
                decimal KDV;
                if (globalHareket.Kdv == null || globalHareket.Kdv == 0)
                    KDV = Convert.ToDecimal(res[1]);
                else
                    KDV = globalHareket.Kdv;

                var itemParameter = "";
                if (itemValue != null)
                {
                    var itemArr = itemValue.Split('$');
                    for (int i = 0; i < itemArr.Length; i++)
                    {
                        itemParameter += ",@p" + (41 + i);
                        cmdSatFatIrsStokUretSayHareket.Parameters.AddWithValue("@p" + (41 + i), itemArr[i]);
                    }
                }
                cmdSatFatIrsStokUretSayHareket = new SqlCommand(@"INSERT INTO " + firmaPrefix + donemPrefix + "TBLSATFATHAREKET " +
                    "(TARIH,EVRAKNO,FIRMANO,STOKNO,MALINCINSI,STOKKODU,STOKTIPI,MIKTAR,BIRIMMIKTAR,BIRIM,BIRIMEX,KDV,FIYATI,GERCEKTOPLAM,DEPO,ENVANTER,ACIKLAMA,GK,SATIRNO,SERIMIKTAR,MASRAF,PARABIRIMI,KUR, DETAY,ISK1,ISK2,ISK3,ISK4,ISK5,ISK6,PERSONEL,PIRIM,PROMOSYON,SATISKOSULU,GRUPMIKTAR,INDIRIM,OTV,OIV,OPSIYON,AFIYATI " + itemColumn + ") OUTPUT INSERTED.IND VALUES" +
                    "(@p1,@p2,@p3,@p4,@p5,@p6,@p7,@p8,@p9,@p10,@p11,@p12,@p13,@p14,@p15,@p16,@p17,@p18,@p19,@p20,@p21,@p22,@p23, @p24,@p25,@p26,@p27,@p28,@p29,@p30,@p31,@p32,@p33,@p34,@p35,@p36,@p37,@p38,@p39,@p40 " + itemParameter+")", connection)
                {
                    Transaction = sqlTransaction
                };
                cmdSatFatIrsStokUretSayHareket.Parameters.AddWithValue("@p1", globalHareket.Tarih.Date);
                cmdSatFatIrsStokUretSayHareket.Parameters.AddWithValue("@p2", globalBaslik.BaslikInd);
                cmdSatFatIrsStokUretSayHareket.Parameters.AddWithValue("@p3", globalBaslik.CariNo);
                cmdSatFatIrsStokUretSayHareket.Parameters.AddWithValue("@p4", globalHareket.StokNo);
                if (String.IsNullOrEmpty(globalHareket.MalinCinsi) || String.IsNullOrEmpty(globalHareket.StokKodu)) 
                {
                   var sak =StokAdiStokKodu(globalHareket.StokNo).Split(';');
                    globalHareket.MalinCinsi = sak[0] == "" ? "" : sak[0];
                    globalHareket.StokKodu= sak[1] == "" ? "" : sak[1];
                }
                cmdSatFatIrsStokUretSayHareket.Parameters.AddWithValue("@p5", globalHareket.MalinCinsi);
                cmdSatFatIrsStokUretSayHareket.Parameters.AddWithValue("@p6", globalHareket.StokKodu);
                cmdSatFatIrsStokUretSayHareket.Parameters.AddWithValue("@p7", StokTip);
                cmdSatFatIrsStokUretSayHareket.Parameters.AddWithValue("@p8", globalHareket.Miktar);
                cmdSatFatIrsStokUretSayHareket.Parameters.AddWithValue("@p9", 1);
                cmdSatFatIrsStokUretSayHareket.Parameters.AddWithValue("@p10", Birim);
                cmdSatFatIrsStokUretSayHareket.Parameters.AddWithValue("@p11", globalHareket.BirimNo);
                cmdSatFatIrsStokUretSayHareket.Parameters.AddWithValue("@p12", KDV);
                if (globalBaslik.KdvDahil == 1) //eğer gelen fiyat kdv dahil ise kdv düşülecek yoksa gelen fiyat kdv haric
                {
                    //decimal iskonto = 0;
                    //if (globalBaslik.Iskonto > 0)
                    //{
                    //    var SatirSayisi = HareketSatirSayisi();
                    //    iskonto = globalBaslik.Iskonto / SatirSayisi;
                    //}
                    //var kdvHaricFiyat = decimal.Round(Math.Round((globalHareket.Fiyat / (1 + (KDV / 100))) * 100) / 100, 2, MidpointRounding.AwayFromZero); 
                    //var kdvHaricGercekFiyat = decimal.Round(Math.Round(((globalHareket.Fiyat * globalHareket.Miktar) / (1 + (KDV / 100))) * 100) / 100, 2, MidpointRounding.AwayFromZero);
                    var kdvHaricFiyat = globalHareket.Fiyat / (1 + (KDV / 100));
                    var kdvHaricGercekFiyat = (globalHareket.Fiyat * globalHareket.Miktar) / (1 + (KDV / 100));
                    //if (iskonto > 0)
                    //    kdvHaricGercekFiyat -= iskonto;
                    cmdSatFatIrsStokUretSayHareket.Parameters.AddWithValue("@p13", kdvHaricFiyat); // KDV HARİC FIYATI
                    cmdSatFatIrsStokUretSayHareket.Parameters.AddWithValue("@p14", kdvHaricGercekFiyat); // KDV HARIC GERCEKTOPLAM
                }
                else 
                {
                    cmdSatFatIrsStokUretSayHareket.Parameters.AddWithValue("@p13", globalHareket.Fiyat); // fiyat kdv hariç geliyor
                    cmdSatFatIrsStokUretSayHareket.Parameters.AddWithValue("@p14", globalHareket.Fiyat * globalHareket.Miktar); // KDV HARIC GERCEKTOPLAM
                }
                cmdSatFatIrsStokUretSayHareket.Parameters.AddWithValue("@p15", globalHareket.DepoNo);
                cmdSatFatIrsStokUretSayHareket.Parameters.AddWithValue("@p16", globalHareket.Miktar);
                cmdSatFatIrsStokUretSayHareket.Parameters.AddWithValue("@p17", globalHareket.Aciklama??"");
                cmdSatFatIrsStokUretSayHareket.Parameters.AddWithValue("@p18", 0);
                cmdSatFatIrsStokUretSayHareket.Parameters.AddWithValue("@p19", SatirNo);
                cmdSatFatIrsStokUretSayHareket.Parameters.AddWithValue("@p20", 1);
                cmdSatFatIrsStokUretSayHareket.Parameters.AddWithValue("@p21", 0);
                cmdSatFatIrsStokUretSayHareket.Parameters.AddWithValue("@p22", "TL");
                cmdSatFatIrsStokUretSayHareket.Parameters.AddWithValue("@p23", 1);

                cmdSatFatIrsStokUretSayHareket.Parameters.AddWithValue("@p24", 0);
                cmdSatFatIrsStokUretSayHareket.Parameters.AddWithValue("@p26", 0);
                cmdSatFatIrsStokUretSayHareket.Parameters.AddWithValue("@p25", 0);
                cmdSatFatIrsStokUretSayHareket.Parameters.AddWithValue("@p27", 0);
                cmdSatFatIrsStokUretSayHareket.Parameters.AddWithValue("@p28", 0);
                cmdSatFatIrsStokUretSayHareket.Parameters.AddWithValue("@p29", 0);
                cmdSatFatIrsStokUretSayHareket.Parameters.AddWithValue("@p30", 0);
                cmdSatFatIrsStokUretSayHareket.Parameters.AddWithValue("@p31", 0);
                cmdSatFatIrsStokUretSayHareket.Parameters.AddWithValue("@p32", 0);
                cmdSatFatIrsStokUretSayHareket.Parameters.AddWithValue("@p33", 0);
                cmdSatFatIrsStokUretSayHareket.Parameters.AddWithValue("@p34", 0);
                cmdSatFatIrsStokUretSayHareket.Parameters.AddWithValue("@p35", 1);
                cmdSatFatIrsStokUretSayHareket.Parameters.AddWithValue("@p36", 0);
                cmdSatFatIrsStokUretSayHareket.Parameters.AddWithValue("@p37", 0);
                cmdSatFatIrsStokUretSayHareket.Parameters.AddWithValue("@p38", 0);
                cmdSatFatIrsStokUretSayHareket.Parameters.AddWithValue("@p39", CariOpsiyonGetir(globalBaslik.CariNo));
                cmdSatFatIrsStokUretSayHareket.Parameters.AddWithValue("@p40", Maliyet);
                var hareketInd = (int)cmdSatFatIrsStokUretSayHareket.ExecuteScalar();
                InsertSatFatIrsStokUretSay2(hareketInd, KDV, StokTip); 
            }
            catch (Exception e)
            {
                throw e;
            }
        }
        private void UpdateSatFatBaslik()
        {
            if (connection.State != ConnectionState.Open)
                connection.Open();
            try
            {
                var res=BaslikTutarVeAraToplam().Split(';');
                var tutar = Convert.ToDecimal(res[0]);
                var aratoplam = Convert.ToDecimal(res[1]);
                cmdUpdateSatFatIrsStokUretSayBaslik = new SqlCommand("UPDATE " + firmaPrefix + donemPrefix + "TBLSATFATBASLIK SET TUTAR=@p1, ARATOPLAM=@p2 WHERE IND=@p3", connection)
                {
                    Transaction = sqlTransaction
                };
                cmdUpdateSatFatIrsStokUretSayBaslik.Parameters.AddWithValue("@p1", tutar);
                cmdUpdateSatFatIrsStokUretSayBaslik.Parameters.AddWithValue("@p2", aratoplam);
                cmdUpdateSatFatIrsStokUretSayBaslik.Parameters.AddWithValue("@p3", globalBaslik.BaslikInd);
                cmdUpdateSatFatIrsStokUretSayBaslik.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                throw e;
            }
        }
        #endregion/*SATFAT METOD*/

        #region/*STOK ÇIKIŞ METOD*/
        private void StokCikisBaslik()
        {
            if (connection.State != ConnectionState.Open)
                connection.Open();
            BelgeNo = BelgeNoGetir();
            globalBaslik.BelgeNo = BelgeNo;
            int giris= ((int)globalBaslik.BelgeAlSat);
            int iade= globalBaslik.Iade;
            //if (globalBaslik.BelgeAlSat == VegaBelgeHareketTipi.Cikis)
            //{
            //    giris = 0;
            //    iade = 0;
            //}
            //else
            //{
            //    giris = 1;
            //    iade = 1;
            //}

            try
            {
                var headerParameter = "";
                if (headerValue != null)
                {
                    var headerArr = headerValue.Split('$');
                    for (int i = 0; i < headerArr.Length; i++)
                    {
                        headerParameter += ",@p" + (31 + i);
                        cmdSatFatIrsStokUretSayBaslik.Parameters.AddWithValue("@p" + (31 + i), headerArr[i]);
                    }
                }
                headerParameter = headerParameter.TrimStart(',');
                cmdSatFatIrsStokUretSayBaslik = new SqlCommand("INSERT INTO " + firmaPrefix + donemPrefix + "TBLSTKCIKBASLIK " +

                    "(BELGENO,TARIH,KDV,ODEMETARIHI,ALT1,DEPO,OZELKOD1,OZELKOD2,CREDATE,LADATE,FIRMANO,GIRIS,BELGETIPI,USERNO,OZELKOD3,OZELKOD4,OZELKOD5,OZELKOD6,OZELKOD7,OZELKOD8,OZELKOD9,HAREKETDEPOSU,UID,PARABIRIMI,KUR,YUVARLAMA,ALLOWYUVARLAMA,IADE,IPTAL,AK,    ODMODIFIED,CONVERTED,ENTEGRE,SATISSEKLI,YURTDISI,MUHASEBELESMEYECEK,KAYNAK,STOKHAREKETEYAZ,CARIHAREKETEYAZ " + headerColumn + ") OUTPUT INSERTED.IND VALUES" +
                    "(@p1,@p2,@p3,@p4,@p5,@p6,@p7,@p8,@p9,@p10,@p11,@p12,@p13,@p14,@p15,@p16,@p17,@p18,@p19,@p20,@p21,@p22,@p23,@p24,@p25,@p26,@p27,@p28,@p29,@p30, @p31,@p32,@p33,@p34,@p35,@p36,@p37,@p38,@p39 " + headerParameter + ")", connection)
                {
                    Transaction = sqlTransaction
                };
                cmdSatFatIrsStokUretSayBaslik.Parameters.AddWithValue("@p1", BelgeNo);
                cmdSatFatIrsStokUretSayBaslik.Parameters.AddWithValue("@p2", globalBaslik.Tarih.Date);
                cmdSatFatIrsStokUretSayBaslik.Parameters.AddWithValue("@p3", globalBaslik.KdvDahil);
                cmdSatFatIrsStokUretSayBaslik.Parameters.AddWithValue("@p4", globalBaslik.Tarih.Date);
                cmdSatFatIrsStokUretSayBaslik.Parameters.AddWithValue("@p5", globalBaslik.Iskonto);
                
                    cmdSatFatIrsStokUretSayBaslik.Parameters.AddWithValue("@p6", (object)globalBaslik.DepoNo ?? DBNull.Value);
                
                cmdSatFatIrsStokUretSayBaslik.Parameters.AddWithValue("@p7", globalBaslik.OzelKod1 ?? "");
                cmdSatFatIrsStokUretSayBaslik.Parameters.AddWithValue("@p8", globalBaslik.OzelKod2 ?? "");
                cmdSatFatIrsStokUretSayBaslik.Parameters.AddWithValue("@p9", DateTime.Now);
                cmdSatFatIrsStokUretSayBaslik.Parameters.AddWithValue("@p10", DateTime.Now);
                cmdSatFatIrsStokUretSayBaslik.Parameters.AddWithValue("@p11", globalBaslik.CariNo);
                cmdSatFatIrsStokUretSayBaslik.Parameters.AddWithValue("@p12", giris);
                cmdSatFatIrsStokUretSayBaslik.Parameters.AddWithValue("@p13", globalBaslik.Izahat);
                cmdSatFatIrsStokUretSayBaslik.Parameters.AddWithValue("@p14", userNo);
                cmdSatFatIrsStokUretSayBaslik.Parameters.AddWithValue("@p15", globalBaslik.OzelKod3 ?? "");
                cmdSatFatIrsStokUretSayBaslik.Parameters.AddWithValue("@p16", globalBaslik.OzelKod4 ?? "");
                cmdSatFatIrsStokUretSayBaslik.Parameters.AddWithValue("@p17", globalBaslik.OzelKod5 ?? "");
                cmdSatFatIrsStokUretSayBaslik.Parameters.AddWithValue("@p18", globalBaslik.OzelKod6 ?? "");
                cmdSatFatIrsStokUretSayBaslik.Parameters.AddWithValue("@p19", globalBaslik.OzelKod7 ?? "");
                cmdSatFatIrsStokUretSayBaslik.Parameters.AddWithValue("@p20", globalBaslik.OzelKod8 ?? "");
                cmdSatFatIrsStokUretSayBaslik.Parameters.AddWithValue("@p21", globalBaslik.OzelKod9 ?? "");
                cmdSatFatIrsStokUretSayBaslik.Parameters.AddWithValue("@p22", globalBaslik.HareketDeposu);
                cmdSatFatIrsStokUretSayBaslik.Parameters.AddWithValue("@p23", "{" + Guid.NewGuid() + "}");
                cmdSatFatIrsStokUretSayBaslik.Parameters.AddWithValue("@p24", "TL");
                cmdSatFatIrsStokUretSayBaslik.Parameters.AddWithValue("@p25", 1);
                cmdSatFatIrsStokUretSayBaslik.Parameters.AddWithValue("@p26", 0);
                cmdSatFatIrsStokUretSayBaslik.Parameters.AddWithValue("@p27", 0);
                cmdSatFatIrsStokUretSayBaslik.Parameters.AddWithValue("@p28", iade);
                cmdSatFatIrsStokUretSayBaslik.Parameters.AddWithValue("@p29", 0);
                cmdSatFatIrsStokUretSayBaslik.Parameters.AddWithValue("@p30", 0);

                cmdSatFatIrsStokUretSayBaslik.Parameters.AddWithValue("@p31", 0);
                cmdSatFatIrsStokUretSayBaslik.Parameters.AddWithValue("@p32", 0);
                cmdSatFatIrsStokUretSayBaslik.Parameters.AddWithValue("@p33", 0);
                cmdSatFatIrsStokUretSayBaslik.Parameters.AddWithValue("@p34", 0);
                cmdSatFatIrsStokUretSayBaslik.Parameters.AddWithValue("@p35", 0);
                cmdSatFatIrsStokUretSayBaslik.Parameters.AddWithValue("@p36", 0);
                cmdSatFatIrsStokUretSayBaslik.Parameters.AddWithValue("@p37", 0);
                cmdSatFatIrsStokUretSayBaslik.Parameters.AddWithValue("@p38", 1);
                cmdSatFatIrsStokUretSayBaslik.Parameters.AddWithValue("@p39", 1);


                globalBaslik.BaslikInd = (int)cmdSatFatIrsStokUretSayBaslik.ExecuteScalar();
                InsertSatFatIrsStokUretSay1();
            }
            catch (Exception e)
            {
                throw e;
            }
        }
        private void StokCikisHareket()
        {
            if (connection.State != ConnectionState.Open)
                connection.Open();
            try
            {
                var SatirNo = HareketSatirSayisi();
                var stm = StokTipVeMaliyet(globalHareket.StokNo).Split(';');
                var StokTip = stm[0] == "" ? 0 : Convert.ToInt32(stm[0]);
                var Maliyet = stm[1] == "" ? 0 : Convert.ToDecimal(stm[1]);
                var res = StokBirimVeKdv(globalHareket.BirimNo).Split(';');
                var Birim = res[0].Length > 5 ? res[0].Substring(0, 5) : res[0];
                decimal KDV;
                if (globalHareket.Kdv == null || globalHareket.Kdv == 0)
                    KDV = Convert.ToDecimal(res[1]);
                else
                    KDV = globalHareket.Kdv;

                var itemParameter = "";
                if (itemValue != null)
                {
                    var itemArr = itemValue.Split('$');
                    for (int i = 0; i < itemArr.Length; i++)
                    {
                        itemParameter += ",@p" + (24 + i);
                        cmdSatFatIrsStokUretSayHareket.Parameters.AddWithValue("@p" + (24 + i), itemArr[i]);
                    }
                }
                cmdSatFatIrsStokUretSayHareket = new SqlCommand(@"INSERT INTO " + firmaPrefix + donemPrefix + "TBLSTKCIKHAREKET " +
                    "(TARIH,EVRAKNO,FIRMANO,STOKNO,MALINCINSI,STOKKODU,STOKTIPI,MIKTAR,BIRIMMIKTAR,BIRIM,BIRIMEX,KDV,FIYATI,GERCEKTOPLAM,DEPO,ENVANTER,ACIKLAMA,GK,SATIRNO,SERIMIKTAR,MASRAF,PARABIRIMI,KUR, DETAY,ISK1,ISK2,ISK3,ISK4,ISK5,ISK6,PERSONEL,PIRIM,PROMOSYON,SATISKOSULU,GRUPMIKTAR,INDIRIM,OTV,OIV,OPSIYON,AFIYATI " + itemColumn + ") OUTPUT INSERTED.IND VALUES" +
                    "(@p1,@p2,@p3,@p4,@p5,@p6,@p7,@p8,@p9,@p10,@p11,@p12,@p13,@p14,@p15,@p16,@p17,@p18,@p19,@p20,@p21,@p22,@p23, @p24,@p25,@p26,@p27,@p28,@p29,@p30,@p31,@p32,@p33,@p34,@p35,@p36,@p37,@p38,@p39,@p40 " + itemParameter + ")", connection)
                {
                    Transaction = sqlTransaction
                };
                cmdSatFatIrsStokUretSayHareket.Parameters.AddWithValue("@p1", globalHareket.Tarih.Date);
                cmdSatFatIrsStokUretSayHareket.Parameters.AddWithValue("@p2", globalBaslik.BaslikInd);
                cmdSatFatIrsStokUretSayHareket.Parameters.AddWithValue("@p3", globalBaslik.CariNo);
                cmdSatFatIrsStokUretSayHareket.Parameters.AddWithValue("@p4", globalHareket.StokNo);
                cmdSatFatIrsStokUretSayHareket.Parameters.AddWithValue("@p5", globalHareket.MalinCinsi);
                cmdSatFatIrsStokUretSayHareket.Parameters.AddWithValue("@p6", globalHareket.StokKodu);
                cmdSatFatIrsStokUretSayHareket.Parameters.AddWithValue("@p7", StokTip);
                cmdSatFatIrsStokUretSayHareket.Parameters.AddWithValue("@p8", globalHareket.Miktar);
                cmdSatFatIrsStokUretSayHareket.Parameters.AddWithValue("@p9", 1);
                cmdSatFatIrsStokUretSayHareket.Parameters.AddWithValue("@p10", Birim);
                cmdSatFatIrsStokUretSayHareket.Parameters.AddWithValue("@p11", globalHareket.BirimNo);
                cmdSatFatIrsStokUretSayHareket.Parameters.AddWithValue("@p12", KDV);
                if (globalBaslik.KdvDahil == 1) //eğer gelen fiyat kdv dahil ise kdv düşülecek yoksa gelen fiyat kdv haric
                {
                    //decimal iskonto = 0;
                    //if (globalBaslik.Iskonto > 0)
                    //{
                    //    var SatirSayisi = HareketSatirSayisi();
                    //    iskonto = globalBaslik.Iskonto / SatirSayisi;
                    //}
                    var kdvHaricFiyat = globalHareket.Fiyat / (1 + (KDV / 100));
                    var kdvHaricGercekFiyat = (globalHareket.Fiyat * globalHareket.Miktar) / (1 + (KDV / 100));
                    //if (iskonto > 0)
                    //    kdvHaricGercekFiyat -= iskonto;
                    cmdSatFatIrsStokUretSayHareket.Parameters.AddWithValue("@p13", kdvHaricFiyat); // KDV HARİC FIYATI
                    cmdSatFatIrsStokUretSayHareket.Parameters.AddWithValue("@p14", kdvHaricGercekFiyat); // KDV HARIC GERCEKTOPLAM
                }
                else
                {
                    cmdSatFatIrsStokUretSayHareket.Parameters.AddWithValue("@p13", globalHareket.Fiyat); // KDV DAHIL FIYATI
                    cmdSatFatIrsStokUretSayHareket.Parameters.AddWithValue("@p14", globalHareket.Fiyat * globalHareket.Miktar); // KDV DAHIL GERCEKTOPLAM
                }
                cmdSatFatIrsStokUretSayHareket.Parameters.AddWithValue("@p15", globalHareket.DepoNo);
                cmdSatFatIrsStokUretSayHareket.Parameters.AddWithValue("@p16", globalHareket.Miktar);
                cmdSatFatIrsStokUretSayHareket.Parameters.AddWithValue("@p17", globalHareket.Aciklama ?? "");
                cmdSatFatIrsStokUretSayHareket.Parameters.AddWithValue("@p18", 0);
                cmdSatFatIrsStokUretSayHareket.Parameters.AddWithValue("@p19", SatirNo);
                cmdSatFatIrsStokUretSayHareket.Parameters.AddWithValue("@p20", 1);
                cmdSatFatIrsStokUretSayHareket.Parameters.AddWithValue("@p21", 0);
                cmdSatFatIrsStokUretSayHareket.Parameters.AddWithValue("@p22", "TL");
                cmdSatFatIrsStokUretSayHareket.Parameters.AddWithValue("@p23", 1);

                cmdSatFatIrsStokUretSayHareket.Parameters.AddWithValue("@p24", 0);
                cmdSatFatIrsStokUretSayHareket.Parameters.AddWithValue("@p26", 0);
                cmdSatFatIrsStokUretSayHareket.Parameters.AddWithValue("@p25", 0);
                cmdSatFatIrsStokUretSayHareket.Parameters.AddWithValue("@p27", 0);
                cmdSatFatIrsStokUretSayHareket.Parameters.AddWithValue("@p28", 0);
                cmdSatFatIrsStokUretSayHareket.Parameters.AddWithValue("@p29", 0);
                cmdSatFatIrsStokUretSayHareket.Parameters.AddWithValue("@p30", 0);
                cmdSatFatIrsStokUretSayHareket.Parameters.AddWithValue("@p31", 0);
                cmdSatFatIrsStokUretSayHareket.Parameters.AddWithValue("@p32", 0);
                cmdSatFatIrsStokUretSayHareket.Parameters.AddWithValue("@p33", 0);
                cmdSatFatIrsStokUretSayHareket.Parameters.AddWithValue("@p34", 0);
                cmdSatFatIrsStokUretSayHareket.Parameters.AddWithValue("@p35", 1);
                cmdSatFatIrsStokUretSayHareket.Parameters.AddWithValue("@p36", 0);
                cmdSatFatIrsStokUretSayHareket.Parameters.AddWithValue("@p37", 0);
                cmdSatFatIrsStokUretSayHareket.Parameters.AddWithValue("@p38", 0);
                cmdSatFatIrsStokUretSayHareket.Parameters.AddWithValue("@p39", CariOpsiyonGetir(globalBaslik.CariNo));
                cmdSatFatIrsStokUretSayHareket.Parameters.AddWithValue("@p40", Maliyet);
                var hareketInd = (int)cmdSatFatIrsStokUretSayHareket.ExecuteScalar();
                InsertSatFatIrsStokUretSay2(hareketInd, KDV, StokTip); 
            }
            catch (Exception e)
            {
                throw e;
            }
        }
        private void UpdateStokCikisBaslik()
        {
            if (connection.State != ConnectionState.Open)
                connection.Open();
            try
            {
                var res = BaslikTutarVeAraToplam().Split(';');
                var tutar = Convert.ToDouble(res[0]);
                var aratoplam = Convert.ToDouble(res[1]);
                cmdUpdateSatFatIrsStokUretSayBaslik = new SqlCommand("UPDATE " + firmaPrefix + donemPrefix + "TBLSTKCIKBASLIK SET TUTAR=@p1, ARATOPLAM=@p2 WHERE IND=@p3", connection)
                {
                    Transaction = sqlTransaction
                };
                cmdUpdateSatFatIrsStokUretSayBaslik.Parameters.AddWithValue("@p1", tutar);
                cmdUpdateSatFatIrsStokUretSayBaslik.Parameters.AddWithValue("@p2", aratoplam);
                cmdUpdateSatFatIrsStokUretSayBaslik.Parameters.AddWithValue("@p3", globalBaslik.BaslikInd);
                cmdUpdateSatFatIrsStokUretSayBaslik.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                throw e;
            }
        }
        #endregion/*STOK ÇIKIŞ METOD*/

        #region/*SATIRS METOD*/
        private void SatIrsBaslik()
        {
            if (connection.State != ConnectionState.Open)
                connection.Open();
            BelgeNo = BelgeNoGetir();
            globalBaslik.BelgeNo = BelgeNo;
            int giris= ((int)globalBaslik.BelgeAlSat);
            int iade= globalBaslik.Iade;
            //if (globalBaslik.BelgeAlSat == VegaBelgeHareketTipi.Cikis)
            //{
            //    giris = 0;
            //    iade = 0;
            //}
            //else
            //{
            //    giris = 1;
            //    iade = 1;
            //}

            try
            {
                var headerParameter = "";
                if (headerValue != null)
                {
                    var headerArr = headerValue.Split('$');
                    for (int i = 0; i < headerArr.Length; i++)
                    {
                        headerParameter += ",@p" + (31 + i);
                        cmdSatFatIrsStokUretSayBaslik.Parameters.AddWithValue("@p" + (31 + i), headerArr[i]);
                    }
                }
                headerParameter = headerParameter.TrimStart(',');
                cmdSatFatIrsStokUretSayBaslik = new SqlCommand("INSERT INTO " + firmaPrefix + donemPrefix + "TBLSATIRSBASLIK " +

                    "(BELGENO,TARIH,KDV,ODEMETARIHI,ALT1,DEPO,OZELKOD1,OZELKOD2,CREDATE,LADATE,FIRMANO,GIRIS,BELGETIPI,USERNO,OZELKOD3,OZELKOD4,OZELKOD5,OZELKOD6,OZELKOD7,OZELKOD8,OZELKOD9,HAREKETDEPOSU,UID,PARABIRIMI,KUR,YUVARLAMA,ALLOWYUVARLAMA,IADE,IPTAL,AK,    ODMODIFIED,CONVERTED,ENTEGRE,SATISSEKLI,YURTDISI,MUHASEBELESMEYECEK,KAYNAK,STOKHAREKETEYAZ,CARIHAREKETEYAZ " + headerColumn + ") OUTPUT INSERTED.IND VALUES" +
                    "(@p1,@p2,@p3,@p4,@p5,@p6,@p7,@p8,@p9,@p10,@p11,@p12,@p13,@p14,@p15,@p16,@p17,@p18,@p19,@p20,@p21,@p22,@p23,@p24,@p25,@p26,@p27,@p28,@p29,@p30, @p31,@p32,@p33,@p34,@p35,@p36,@p37,@p38,@p39 " + headerParameter + ")", connection)
                {
                    Transaction = sqlTransaction
                };
                cmdSatFatIrsStokUretSayBaslik.Parameters.AddWithValue("@p1", BelgeNo);
                cmdSatFatIrsStokUretSayBaslik.Parameters.AddWithValue("@p2", globalBaslik.Tarih.Date);
                cmdSatFatIrsStokUretSayBaslik.Parameters.AddWithValue("@p3", globalBaslik.KdvDahil);
                cmdSatFatIrsStokUretSayBaslik.Parameters.AddWithValue("@p4", globalBaslik.Tarih.Date);
                cmdSatFatIrsStokUretSayBaslik.Parameters.AddWithValue("@p5", globalBaslik.Iskonto);
                
                    cmdSatFatIrsStokUretSayBaslik.Parameters.AddWithValue("@p6", (object)globalBaslik.DepoNo ?? DBNull.Value);
                
                cmdSatFatIrsStokUretSayBaslik.Parameters.AddWithValue("@p7", globalBaslik.OzelKod1 ?? "");
                cmdSatFatIrsStokUretSayBaslik.Parameters.AddWithValue("@p8", globalBaslik.OzelKod2 ?? "");
                cmdSatFatIrsStokUretSayBaslik.Parameters.AddWithValue("@p9", DateTime.Now);
                cmdSatFatIrsStokUretSayBaslik.Parameters.AddWithValue("@p10", DateTime.Now);
                cmdSatFatIrsStokUretSayBaslik.Parameters.AddWithValue("@p11", globalBaslik.CariNo);
                cmdSatFatIrsStokUretSayBaslik.Parameters.AddWithValue("@p12", giris);
                cmdSatFatIrsStokUretSayBaslik.Parameters.AddWithValue("@p13", globalBaslik.Izahat);
                cmdSatFatIrsStokUretSayBaslik.Parameters.AddWithValue("@p14", userNo);
                cmdSatFatIrsStokUretSayBaslik.Parameters.AddWithValue("@p15", globalBaslik.OzelKod3 ?? "");
                cmdSatFatIrsStokUretSayBaslik.Parameters.AddWithValue("@p16", globalBaslik.OzelKod4 ?? "");
                cmdSatFatIrsStokUretSayBaslik.Parameters.AddWithValue("@p17", globalBaslik.OzelKod5 ?? "");
                cmdSatFatIrsStokUretSayBaslik.Parameters.AddWithValue("@p18", globalBaslik.OzelKod6 ?? "");
                cmdSatFatIrsStokUretSayBaslik.Parameters.AddWithValue("@p19", globalBaslik.OzelKod7 ?? "");
                cmdSatFatIrsStokUretSayBaslik.Parameters.AddWithValue("@p20", globalBaslik.OzelKod8 ?? "");
                cmdSatFatIrsStokUretSayBaslik.Parameters.AddWithValue("@p21", globalBaslik.OzelKod9 ?? "");
                cmdSatFatIrsStokUretSayBaslik.Parameters.AddWithValue("@p22", globalBaslik.HareketDeposu);
                cmdSatFatIrsStokUretSayBaslik.Parameters.AddWithValue("@p23", "{" + Guid.NewGuid() + "}");
                cmdSatFatIrsStokUretSayBaslik.Parameters.AddWithValue("@p24", "TL");
                cmdSatFatIrsStokUretSayBaslik.Parameters.AddWithValue("@p25", 1);
                cmdSatFatIrsStokUretSayBaslik.Parameters.AddWithValue("@p26", 0);
                cmdSatFatIrsStokUretSayBaslik.Parameters.AddWithValue("@p27", 0);
                cmdSatFatIrsStokUretSayBaslik.Parameters.AddWithValue("@p28", iade);
                cmdSatFatIrsStokUretSayBaslik.Parameters.AddWithValue("@p29", 0);
                cmdSatFatIrsStokUretSayBaslik.Parameters.AddWithValue("@p30", 0);

                cmdSatFatIrsStokUretSayBaslik.Parameters.AddWithValue("@p31", 0);
                cmdSatFatIrsStokUretSayBaslik.Parameters.AddWithValue("@p32", 0);
                cmdSatFatIrsStokUretSayBaslik.Parameters.AddWithValue("@p33", 0);
                cmdSatFatIrsStokUretSayBaslik.Parameters.AddWithValue("@p34", 0);
                cmdSatFatIrsStokUretSayBaslik.Parameters.AddWithValue("@p35", 0);
                cmdSatFatIrsStokUretSayBaslik.Parameters.AddWithValue("@p36", 0);
                cmdSatFatIrsStokUretSayBaslik.Parameters.AddWithValue("@p37", 0);
                cmdSatFatIrsStokUretSayBaslik.Parameters.AddWithValue("@p38", 1);
                cmdSatFatIrsStokUretSayBaslik.Parameters.AddWithValue("@p39", 1);


                globalBaslik.BaslikInd = (int)cmdSatFatIrsStokUretSayBaslik.ExecuteScalar();
                InsertSatFatIrsStokUretSay1();
            }
            catch (Exception e)
            {
                throw e;
            }
        }
        private void SatIrsHareket()
        {
            if (connection.State != ConnectionState.Open)
                connection.Open();
            try
            {
                var SatirNo = HareketSatirSayisi();
                var stm = StokTipVeMaliyet(globalHareket.StokNo).Split(';');
                var StokTip = stm[0] == "" ? 0 : Convert.ToInt32(stm[0]);
                var Maliyet = stm[1] == "" ? 0 : Convert.ToDecimal(stm[1]);
                var res = StokBirimVeKdv(globalHareket.BirimNo).Split(';');
                var Birim = res[0].Length > 5 ? res[0].Substring(0, 5) : res[0];
                decimal KDV;
                if (globalHareket.Kdv == null || globalHareket.Kdv == 0)
                    KDV = Convert.ToDecimal(res[1]);
                else
                    KDV = globalHareket.Kdv;

                var itemParameter = "";
                if (itemValue != null)
                {
                    var itemArr = itemValue.Split('$');
                    for (int i = 0; i < itemArr.Length; i++)
                    {
                        itemParameter += ",@p" + (24 + i);
                        cmdSatFatIrsStokUretSayHareket.Parameters.AddWithValue("@p" + (24 + i), itemArr[i]);
                    }
                }
                cmdSatFatIrsStokUretSayHareket = new SqlCommand(@"INSERT INTO " + firmaPrefix + donemPrefix + "TBLSATIRSHAREKET " +
                    "(TARIH,EVRAKNO,FIRMANO,STOKNO,MALINCINSI,STOKKODU,STOKTIPI,MIKTAR,BIRIMMIKTAR,BIRIM,BIRIMEX,KDV,FIYATI,GERCEKTOPLAM,DEPO,ENVANTER,ACIKLAMA,GK,SATIRNO,SERIMIKTAR,MASRAF,PARABIRIMI,KUR, DETAY,ISK1,ISK2,ISK3,ISK4,ISK5,ISK6,PERSONEL,PIRIM,PROMOSYON,SATISKOSULU,GRUPMIKTAR,INDIRIM,OTV,OIV,OPSIYON,AFIYATI " + itemColumn + ") OUTPUT INSERTED.IND VALUES" +
                    "(@p1,@p2,@p3,@p4,@p5,@p6,@p7,@p8,@p9,@p10,@p11,@p12,@p13,@p14,@p15,@p16,@p17,@p18,@p19,@p20,@p21,@p22,@p23, @p24,@p25,@p26,@p27,@p28,@p29,@p30,@p31,@p32,@p33,@p34,@p35,@p36,@p37,@p38,@p39,@p40 " + itemParameter + ")", connection)
                {
                    Transaction = sqlTransaction
                };
                cmdSatFatIrsStokUretSayHareket.Parameters.AddWithValue("@p1", globalHareket.Tarih.Date);
                cmdSatFatIrsStokUretSayHareket.Parameters.AddWithValue("@p2", globalBaslik.BaslikInd);
                cmdSatFatIrsStokUretSayHareket.Parameters.AddWithValue("@p3", globalBaslik.CariNo);
                cmdSatFatIrsStokUretSayHareket.Parameters.AddWithValue("@p4", globalHareket.StokNo);
                cmdSatFatIrsStokUretSayHareket.Parameters.AddWithValue("@p5", globalHareket.MalinCinsi);
                cmdSatFatIrsStokUretSayHareket.Parameters.AddWithValue("@p6", globalHareket.StokKodu);
                cmdSatFatIrsStokUretSayHareket.Parameters.AddWithValue("@p7", StokTip);
                cmdSatFatIrsStokUretSayHareket.Parameters.AddWithValue("@p8", globalHareket.Miktar);
                cmdSatFatIrsStokUretSayHareket.Parameters.AddWithValue("@p9", 1);
                cmdSatFatIrsStokUretSayHareket.Parameters.AddWithValue("@p10", Birim);
                cmdSatFatIrsStokUretSayHareket.Parameters.AddWithValue("@p11", globalHareket.BirimNo);
                cmdSatFatIrsStokUretSayHareket.Parameters.AddWithValue("@p12", KDV);
                if (globalBaslik.KdvDahil == 1) //eğer gelen fiyat kdv dahil ise kdv düşülecek yoksa gelen fiyat kdv haric
                {
                    //decimal iskonto = 0;
                    //if (globalBaslik.Iskonto > 0)
                    //{
                    //    var SatirSayisi = HareketSatirSayisi();
                    //    iskonto = globalBaslik.Iskonto / SatirSayisi;
                    //}
                    var kdvHaricFiyat = globalHareket.Fiyat / (1 + (KDV / 100));
                    var kdvHaricGercekFiyat = (globalHareket.Fiyat * globalHareket.Miktar) / (1 + (KDV / 100));
                    //if (iskonto > 0)
                    //    kdvHaricGercekFiyat -= iskonto;
                    cmdSatFatIrsStokUretSayHareket.Parameters.AddWithValue("@p13", kdvHaricFiyat); // KDV HARİC FIYATI
                    cmdSatFatIrsStokUretSayHareket.Parameters.AddWithValue("@p14", kdvHaricGercekFiyat); // KDV HARIC GERCEKTOPLAM
                }
                else
                {
                    cmdSatFatIrsStokUretSayHareket.Parameters.AddWithValue("@p13", globalHareket.Fiyat); // KDV DAHIL FIYATI
                    cmdSatFatIrsStokUretSayHareket.Parameters.AddWithValue("@p14", globalHareket.Fiyat * globalHareket.Miktar); // KDV DAHIL GERCEKTOPLAM
                }
                cmdSatFatIrsStokUretSayHareket.Parameters.AddWithValue("@p15", globalHareket.DepoNo);
                cmdSatFatIrsStokUretSayHareket.Parameters.AddWithValue("@p16", globalHareket.Miktar);
                cmdSatFatIrsStokUretSayHareket.Parameters.AddWithValue("@p17", globalHareket.Aciklama ?? "");
                cmdSatFatIrsStokUretSayHareket.Parameters.AddWithValue("@p18", 0);
                cmdSatFatIrsStokUretSayHareket.Parameters.AddWithValue("@p19", SatirNo);
                cmdSatFatIrsStokUretSayHareket.Parameters.AddWithValue("@p20", 1);
                cmdSatFatIrsStokUretSayHareket.Parameters.AddWithValue("@p21", 0);
                cmdSatFatIrsStokUretSayHareket.Parameters.AddWithValue("@p22", "TL");
                cmdSatFatIrsStokUretSayHareket.Parameters.AddWithValue("@p23", 1);

                cmdSatFatIrsStokUretSayHareket.Parameters.AddWithValue("@p24", 0);
                cmdSatFatIrsStokUretSayHareket.Parameters.AddWithValue("@p26", 0);
                cmdSatFatIrsStokUretSayHareket.Parameters.AddWithValue("@p25", 0);
                cmdSatFatIrsStokUretSayHareket.Parameters.AddWithValue("@p27", 0);
                cmdSatFatIrsStokUretSayHareket.Parameters.AddWithValue("@p28", 0);
                cmdSatFatIrsStokUretSayHareket.Parameters.AddWithValue("@p29", 0);
                cmdSatFatIrsStokUretSayHareket.Parameters.AddWithValue("@p30", 0);
                cmdSatFatIrsStokUretSayHareket.Parameters.AddWithValue("@p31", 0);
                cmdSatFatIrsStokUretSayHareket.Parameters.AddWithValue("@p32", 0);
                cmdSatFatIrsStokUretSayHareket.Parameters.AddWithValue("@p33", 0);
                cmdSatFatIrsStokUretSayHareket.Parameters.AddWithValue("@p34", 0);
                cmdSatFatIrsStokUretSayHareket.Parameters.AddWithValue("@p35", 1);
                cmdSatFatIrsStokUretSayHareket.Parameters.AddWithValue("@p36", 0);
                cmdSatFatIrsStokUretSayHareket.Parameters.AddWithValue("@p37", 0);
                cmdSatFatIrsStokUretSayHareket.Parameters.AddWithValue("@p38", 0);
                cmdSatFatIrsStokUretSayHareket.Parameters.AddWithValue("@p39", CariOpsiyonGetir(globalBaslik.CariNo));
                cmdSatFatIrsStokUretSayHareket.Parameters.AddWithValue("@p40", Maliyet);
                var hareketInd = (int)cmdSatFatIrsStokUretSayHareket.ExecuteScalar();
                InsertSatFatIrsStokUretSay2(hareketInd, KDV, StokTip ); 
            }
            catch (Exception e)
            {
                throw e;
            }
        }
        private void UpdateSatIrsBaslik()
        {
            if (connection.State != ConnectionState.Open)
                connection.Open();
            try
            {
                var res = BaslikTutarVeAraToplam().Split(';');
                var tutar = Convert.ToDouble(res[0]);
                var aratoplam = Convert.ToDouble(res[1]);
                cmdUpdateSatFatIrsStokUretSayBaslik = new SqlCommand("UPDATE " + firmaPrefix + donemPrefix + "TBLSATIRSBASLIK SET TUTAR=@p1, ARATOPLAM=@p2 WHERE IND=@p3", connection)
                {
                    Transaction = sqlTransaction
                };
                cmdUpdateSatFatIrsStokUretSayBaslik.Parameters.AddWithValue("@p1", tutar);
                cmdUpdateSatFatIrsStokUretSayBaslik.Parameters.AddWithValue("@p2", aratoplam);
                cmdUpdateSatFatIrsStokUretSayBaslik.Parameters.AddWithValue("@p3", globalBaslik.BaslikInd);
                cmdUpdateSatFatIrsStokUretSayBaslik.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                throw e;
            }
        }
        #endregion/*SATIRS METOD*/

        #region/*SATFAT SATIRS STOKCIKIS URETIM SAYIM ORTAK METOD*/
        private void InsertSatFatIrsStokUretSay1() 
        {
            if (connection.State != ConnectionState.Open)
                connection.Open();
            try
            {
                cmdCariGenel = new SqlCommand("INSERT INTO " + firmaPrefix + donemPrefix + "TBLCARIGENELHAREKET " +
                    "(FIRMANO,TARIH,VADE,BELGEIND,ISLEMIND,BELGEIZAHAT,ISLEMIZAHAT,BELGELINK,BORC,ALACAK,AYLIKVADE,BELGENO,ISLEMNO,CONVERTED,IPTAL,SIRALAMATARIHI,TAHSILLINK,GECIKMEHESAPLA,PARABIRIMI,KUR,BASLIKPARABIRIMI,BASLIKKURU,ACIKLAMA,SIRALAMATARIHIEX)VALUES" +
                    "(@p1,@p2,@p3,@p4,@p5,@p6,@p7,@p8,@p9,@p10,@p11,@p12,@p13,@p14,@p15,@p16,@p17,@p18,@p19,@p20,@p21,@p22,@p23,CONVERT(decimal(28,8),@p24)  ) ", connection)
                {
                    Transaction = sqlTransaction
                };
                cmdCariGenel.Parameters.AddWithValue("@p1", globalBaslik.CariNo);
                cmdCariGenel.Parameters.AddWithValue("@p2", globalBaslik.Tarih.Date);
                cmdCariGenel.Parameters.AddWithValue("@p3", globalBaslik.Tarih.Date.AddDays(CariOpsiyonGetir(globalBaslik.CariNo)));
                cmdCariGenel.Parameters.AddWithValue("@p4", globalBaslik.BaslikInd);
                cmdCariGenel.Parameters.AddWithValue("@p5", globalBaslik.BaslikInd);
                cmdCariGenel.Parameters.AddWithValue("@p6", globalBaslik.Izahat);
                cmdCariGenel.Parameters.AddWithValue("@p7", globalBaslik.Izahat);
                cmdCariGenel.Parameters.AddWithValue("@p8", DBNull.Value);//belgelink 0 dı emre gök null yaptırdı.
                cmdCariGenel.Parameters.AddWithValue("@p9", 0);
                cmdCariGenel.Parameters.AddWithValue("@p10", 0);
                cmdCariGenel.Parameters.AddWithValue("@p11", 0);
                cmdCariGenel.Parameters.AddWithValue("@p12", BelgeNo);
                cmdCariGenel.Parameters.AddWithValue("@p13", BelgeNo);
                cmdCariGenel.Parameters.AddWithValue("@p14", 0);
                cmdCariGenel.Parameters.AddWithValue("@p15", 0);
                cmdCariGenel.Parameters.AddWithValue("@p16", globalBaslik.Tarih);
                cmdCariGenel.Parameters.AddWithValue("@p17", DBNull.Value);//tahsillink 0 dı emre gök null yaptırdı.
                cmdCariGenel.Parameters.AddWithValue("@p18", DBNull.Value);//gecikmehesapla 0 dı emre gök null yaptırdı.
                cmdCariGenel.Parameters.AddWithValue("@p19", "TL");
                cmdCariGenel.Parameters.AddWithValue("@p20", 1);
                cmdCariGenel.Parameters.AddWithValue("@p21", "TL");
                cmdCariGenel.Parameters.AddWithValue("@p22", 1);
                cmdCariGenel.Parameters.AddWithValue("@p23", "");
                cmdCariGenel.Parameters.AddWithValue("@p24", globalBaslik.Tarih);
                cmdCariGenel.ExecuteNonQuery();

                cmdCariHareketleri = new SqlCommand("INSERT INTO " + firmaPrefix + donemPrefix + "TBLCARIHAREKETLERI " +
                    "(FIRMANO,TARIH,IZAHAT,EVRAKNO,BORC,ALACAK,BAKIYE,LN,IADE,LN2,CONVNUM,CONVSTYLE,PARABIRIMI,KUR,ODEMETARIHI,ISLEMTARIHI,SIRALAMATARIHI,OZELKOD,SIRALAMATARIHIEX,OZELKOD1,OZELKOD2,OZELKOD3,OZELKOD5,OZELKOD6,OZELKOD7,OZELKOD8,OZELKOD9)VALUES" +
                    "(@p1,@p2,@p3,@p4,@p5,@p6,@p7,@p8,@p9,@p10,@p11,@p12,@p13,@p14,@p15,@p16,@p17,@p18,CONVERT(decimal(28, 8),@p19),@p20,@p21,@p22,@p23,@p24,@p25,@p26,@p27)", connection)
                {
                    Transaction = sqlTransaction
                };
                cmdCariHareketleri.Parameters.AddWithValue("@p1", globalBaslik.CariNo);
                cmdCariHareketleri.Parameters.AddWithValue("@p2", globalBaslik.Tarih.Date);
                cmdCariHareketleri.Parameters.AddWithValue("@p3", globalBaslik.Izahat);
                cmdCariHareketleri.Parameters.AddWithValue("@p4", BelgeNo);
                cmdCariHareketleri.Parameters.AddWithValue("@p5", 0);
                cmdCariHareketleri.Parameters.AddWithValue("@p6", 0);
                cmdCariHareketleri.Parameters.AddWithValue("@p7", 0);
                cmdCariHareketleri.Parameters.AddWithValue("@p8", globalBaslik.BaslikInd);
                cmdCariHareketleri.Parameters.AddWithValue("@p9", DBNull.Value);//0
                cmdCariHareketleri.Parameters.AddWithValue("@p10", DBNull.Value);//0
                cmdCariHareketleri.Parameters.AddWithValue("@p11", DBNull.Value);//0
                cmdCariHareketleri.Parameters.AddWithValue("@p12", DBNull.Value);//0
                cmdCariHareketleri.Parameters.AddWithValue("@p13", "TL");
                cmdCariHareketleri.Parameters.AddWithValue("@p14", 1);
                cmdCariHareketleri.Parameters.AddWithValue("@p15", globalBaslik.Tarih.Date.AddDays(CariOpsiyonGetir(globalBaslik.CariNo)));
                cmdCariHareketleri.Parameters.AddWithValue("@p16", DateTime.Now);
                cmdCariHareketleri.Parameters.AddWithValue("@p17", DateTime.Now);//globalBaslik.Tarih
                cmdCariHareketleri.Parameters.AddWithValue("@p18", !string.IsNullOrEmpty(globalBaslik.OzelKod4)? (object)globalBaslik.OzelKod4 : DBNull.Value); 
                cmdCariHareketleri.Parameters.AddWithValue("@p19", globalBaslik.Tarih);
                cmdCariHareketleri.Parameters.AddWithValue("@p20", !string.IsNullOrEmpty(globalBaslik.OzelKod1)? (object)globalBaslik.OzelKod1 : "");
                cmdCariHareketleri.Parameters.AddWithValue("@p21", !string.IsNullOrEmpty(globalBaslik.OzelKod2) ? (object)globalBaslik.OzelKod2 : "");
                cmdCariHareketleri.Parameters.AddWithValue("@p22", !string.IsNullOrEmpty(globalBaslik.OzelKod3) ? (object)globalBaslik.OzelKod3 : "");
                cmdCariHareketleri.Parameters.AddWithValue("@p23", !string.IsNullOrEmpty(globalBaslik.OzelKod5) ? (object)globalBaslik.OzelKod5 : "");
                cmdCariHareketleri.Parameters.AddWithValue("@p24", !string.IsNullOrEmpty(globalBaslik.OzelKod6) ? (object)globalBaslik.OzelKod6 : "");
                cmdCariHareketleri.Parameters.AddWithValue("@p25", !string.IsNullOrEmpty(globalBaslik.OzelKod7) ? (object)globalBaslik.OzelKod7 : "");
                cmdCariHareketleri.Parameters.AddWithValue("@p26", !string.IsNullOrEmpty(globalBaslik.OzelKod8) ? (object)globalBaslik.OzelKod8 : "");
                cmdCariHareketleri.Parameters.AddWithValue("@p27", !string.IsNullOrEmpty(globalBaslik.OzelKod9) ? (object)globalBaslik.OzelKod9 : "");
                cmdCariHareketleri.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                throw e;
            }
        }
        private void InsertSatFatIrsStokUretSay2(int hareketInd,decimal KDV,int StokTip,bool uretim=false) 
        {
            int iade = 0;
            if (uretim)
                iade = 0;
            else
                iade = globalBaslik.Iade; //globalBaslik.BelgeAlSat == VegaBelgeHareketTipi.Cikis ? 0 : 1;
            if (connection.State != ConnectionState.Open)
                connection.Open();
            try
            {
                cmdStokHareket = new SqlCommand("INSERT INTO " + firmaPrefix + donemPrefix + "TBLSTOKHAREKETLERI " +
                    "(EVRAKNO,IZAHAT,TARIH,GIREN,CIKAN,KALAN,TUTAR,FIRMANO,STOKNO,BELGENO,LN,DEPO,KDV,SERINO,PERSONEL,IADE,OPSIYON,BIRIMFIYAT,BIRIMMALIYET,SIRALAMATARIHI,KUR,PARABIRIMI,BIRIMEX,ACIKLAMA,STOKTIPI,SIRALAMATARIHIEX,SATISBIRIMMALIYETI,SATISBIRIMMALIYETIKDVLI)VALUES" +
                    "(@p1,@p2,@p3,@p4,@p5,@p6,@p7,@p8,@p9,@p10,@p11,@p12,@p13,@p14,@p15,@p16,@p17,@p18,@p19,@p20,@p21,@p22,@p23,@p24,@p25,CONVERT(decimal(28, 8),@p26),@p27,@p28)", connection)
                {
                    Transaction = sqlTransaction
                };
                cmdStokHareket.Parameters.AddWithValue("@p1", BelgeNo);
                cmdStokHareket.Parameters.AddWithValue("@p2", globalBaslik.Izahat);
                cmdStokHareket.Parameters.AddWithValue("@p3", globalBaslik.Tarih.Date);
                cmdStokHareket.Parameters.AddWithValue("@p5", 0);
                cmdStokHareket.Parameters.AddWithValue("@p4", 0);
                cmdStokHareket.Parameters.AddWithValue("@p6", 0);
                cmdStokHareket.Parameters.AddWithValue("@p7", 0);
                cmdStokHareket.Parameters.AddWithValue("@p8", globalBaslik.CariNo);
                cmdStokHareket.Parameters.AddWithValue("@p9", globalHareket.StokNo);
                cmdStokHareket.Parameters.AddWithValue("@p10", globalBaslik.BaslikInd);
                cmdStokHareket.Parameters.AddWithValue("@p11", hareketInd);

                
                    cmdStokHareket.Parameters.AddWithValue("@p12", (object)globalHareket.DepoNo ?? DBNull.Value);
                
               
                cmdStokHareket.Parameters.AddWithValue("@p13", KDV);
                if (globalBaslik.Izahat == 94)
                {
                    cmdStokHareket.Parameters.AddWithValue("@p14", DBNull.Value);
                }
                else
                {
                    cmdStokHareket.Parameters.AddWithValue("@p14", 0);
                }
                
                cmdStokHareket.Parameters.AddWithValue("@p15", 0);
                cmdStokHareket.Parameters.AddWithValue("@p16", iade);
                cmdStokHareket.Parameters.AddWithValue("@p17", CariOpsiyonGetir(globalBaslik.CariNo));


             

                if (globalBaslik.Izahat == 96 || globalBaslik.Izahat == 97)
                { 
                    cmdStokHareket.Parameters.AddWithValue("@p18", globalHareket.Fiyat);
                    cmdStokHareket.Parameters.AddWithValue("@p19", globalHareket.Fiyat);
                }
                else
                {
                    if (globalBaslik.KdvDahil == 1) //eğer gelen fiyat kdv dahil ise kdv düşülecek yoksa gelen fiyat kdv haric
                    {
                        var kdvHaricFiyat = globalHareket.Fiyat / (1 + (KDV / 100));
                        decimal iskontotutari = 0;
                        #region **********İskonto**************
                        if (globalHareket.ISK1 > 0)
                        {
                            iskontotutari += (globalHareket.Fiyat - iskontotutari) * globalHareket.ISK1 / 100;
                        }
                        if (globalHareket.ISK2 > 0)
                        {
                            iskontotutari += (globalHareket.Fiyat - iskontotutari) * globalHareket.ISK2 / 100;
                        }
                        if (globalHareket.ISK3 > 0)
                        {
                            iskontotutari += (globalHareket.Fiyat - iskontotutari) * globalHareket.ISK3 / 100;
                        }
                        if (globalHareket.ISK4 > 0)
                        {
                            iskontotutari += (globalHareket.Fiyat - iskontotutari) * globalHareket.ISK4 / 100;
                        }
                        if (globalHareket.ISK5 > 0)
                        {
                            iskontotutari += (globalHareket.Fiyat - iskontotutari) * globalHareket.ISK5 / 100;
                        }
                        if (globalHareket.ISK6 > 0)
                        {
                            iskontotutari += (globalHareket.Fiyat - iskontotutari) * globalHareket.ISK6 / 100;
                        }
                        #endregion **********İskonto**************
                        cmdStokHareket.Parameters.AddWithValue("@p18", kdvHaricFiyat-iskontotutari);
                        cmdStokHareket.Parameters.AddWithValue("@p19", kdvHaricFiyat - iskontotutari);
                    }
                    else
                    {
                        decimal iskontotutari = 0;
                        #region **********İskonto**************
                        if (globalHareket.ISK1 > 0)
                        {
                            iskontotutari += (globalHareket.Fiyat - iskontotutari) * globalHareket.ISK1 / 100;
                        }
                        if (globalHareket.ISK2 > 0)
                        {
                            iskontotutari += (globalHareket.Fiyat - iskontotutari) * globalHareket.ISK2 / 100;
                        }
                        if (globalHareket.ISK3 > 0)
                        {
                            iskontotutari += (globalHareket.Fiyat - iskontotutari) * globalHareket.ISK3 / 100;
                        }
                        if (globalHareket.ISK4 > 0)
                        {
                            iskontotutari += (globalHareket.Fiyat - iskontotutari) * globalHareket.ISK4 / 100;
                        }
                        if (globalHareket.ISK5 > 0)
                        {
                            iskontotutari += (globalHareket.Fiyat - iskontotutari) * globalHareket.ISK5 / 100;
                        }
                        if (globalHareket.ISK6 > 0)
                        {
                            iskontotutari += (globalHareket.Fiyat - iskontotutari) * globalHareket.ISK6 / 100;
                        }
                        #endregion **********İskonto**************
                        cmdStokHareket.Parameters.AddWithValue("@p18", globalHareket.Fiyat- iskontotutari);
                        cmdStokHareket.Parameters.AddWithValue("@p19", globalHareket.Fiyat- iskontotutari);
                    }
                }
                cmdStokHareket.Parameters.AddWithValue("@p20", DateTime.Now);//globalHareket.Tarih);
                cmdStokHareket.Parameters.AddWithValue("@p21", 1);
                cmdStokHareket.Parameters.AddWithValue("@p22", "TL");
                cmdStokHareket.Parameters.AddWithValue("@p23", globalHareket.BirimNo);
                cmdStokHareket.Parameters.AddWithValue("@p24", globalHareket.Aciklama ?? "");
                cmdStokHareket.Parameters.AddWithValue("@p25", StokTip);
                cmdStokHareket.Parameters.AddWithValue("@p26", globalHareket.Tarih);
                if (globalBaslik.Izahat == 94)
                {
                    cmdStokHareket.Parameters.AddWithValue("@p27", DBNull.Value);
                }
                else
                {
                    cmdStokHareket.Parameters.AddWithValue("@p27", 0);
                }
                    
                cmdStokHareket.Parameters.AddWithValue("@p28", DBNull.Value);
                if(globalBaslik.Izahat!=128 && globalBaslik.Izahat!=129 && globalBaslik.Izahat != 135)
                    cmdStokHareket.ExecuteNonQuery();

                cmdDepoEnvater = new SqlCommand("INSERT INTO " + firmaPrefix + donemPrefix + "TBLDEPOENVANTER " +
                    "(TARIH,STOKNO,DEPO,ENVANTER,BELGETIPI,BELGEIND,SIRALAMATARIHI,SIRALAMATARIHIEX,HAREKETIND)VALUES" +
                    "(@p1,@p2,@p3,@p4,@p5,@p6,@p7,CONVERT(decimal(28, 8),@p8),@p9)", connection)
                {
                    Transaction = sqlTransaction
                };
                cmdDepoEnvater.Parameters.AddWithValue("@p1", globalBaslik.Tarih.Date);
                cmdDepoEnvater.Parameters.AddWithValue("@p2", globalHareket.StokNo);
              
                    cmdDepoEnvater.Parameters.AddWithValue("@p3", (object)globalHareket.DepoNo ?? DBNull.Value);
                
             
                if (globalBaslik.Izahat == 21 || globalBaslik.Izahat == 27 || globalBaslik.Izahat == 33 ||
                        globalBaslik.Izahat == 22 || globalBaslik.Izahat == 28 || globalBaslik.Izahat == 34 ||
                        globalBaslik.Izahat == 94 || globalBaslik.Izahat == 97 || globalBaslik.Izahat == 128)
                    cmdDepoEnvater.Parameters.AddWithValue("@p4", -globalHareket.Miktar);
                else
                    cmdDepoEnvater.Parameters.AddWithValue("@p4", globalHareket.Miktar);
                cmdDepoEnvater.Parameters.AddWithValue("@p5", globalBaslik.Izahat);
                cmdDepoEnvater.Parameters.AddWithValue("@p6", globalBaslik.BaslikInd);
                cmdDepoEnvater.Parameters.AddWithValue("@p7", globalHareket.Tarih);
                cmdDepoEnvater.Parameters.AddWithValue("@p8", globalHareket.Tarih);
                cmdDepoEnvater.Parameters.AddWithValue("@p9", hareketInd);
                if(globalBaslik.Izahat!=135 )
                    cmdDepoEnvater.ExecuteNonQuery();




                if (globalBaslik.Izahat == 128 || globalBaslik.Izahat == 129 )
                {
                    cmdDepoEnvaterTers = new SqlCommand("INSERT INTO " + firmaPrefix + donemPrefix + "TBLDEPOENVANTER " +
                        "(TARIH,STOKNO,DEPO,ENVANTER,BELGETIPI,BELGEIND,SIRALAMATARIHI,SIRALAMATARIHIEX,HAREKETIND)VALUES" +
                        "(@p1,@p2,@p3,@p4,@p5,@p6,@p7,CONVERT(decimal(28, 8),@p8),@p9)", connection)
                    {
                        Transaction = sqlTransaction
                    };
                    cmdDepoEnvaterTers.Parameters.AddWithValue("@p1", globalBaslik.Tarih.Date);
                    cmdDepoEnvaterTers.Parameters.AddWithValue("@p2", globalHareket.StokNo);

                    cmdDepoEnvaterTers.Parameters.AddWithValue("@p3", 3);

                    if(globalBaslik.Izahat == 129)
                    cmdDepoEnvaterTers.Parameters.AddWithValue("@p4", -globalHareket.Miktar);
                    else
                    {
                        cmdDepoEnvaterTers.Parameters.AddWithValue("@p4", globalHareket.Miktar);
                    }
                    cmdDepoEnvaterTers.Parameters.AddWithValue("@p5", globalBaslik.Izahat);
                    cmdDepoEnvaterTers.Parameters.AddWithValue("@p6", globalBaslik.BaslikInd);
                    cmdDepoEnvaterTers.Parameters.AddWithValue("@p7", globalHareket.Tarih);
                    cmdDepoEnvaterTers.Parameters.AddWithValue("@p8", globalHareket.Tarih);
                    cmdDepoEnvaterTers.Parameters.AddWithValue("@p9", hareketInd);
                    if (globalBaslik.Izahat != 135)
                        cmdDepoEnvaterTers.ExecuteNonQuery();
                }

           


                if (globalBaslik.Izahat == 93 || globalBaslik.Izahat == 94) 
                {
                    var cmdExists = new SqlCommand("SELECT COUNT(*) FROM sysobjects WHERE Name = '"+firmaPrefix+"TBLSAYIMDOSYALARI' AND xtype='U'", connection) { Transaction = sqlTransaction };
                    if ((int)cmdExists.ExecuteScalar() > 0) 
                    {
                        cmdSayimDosyasi = new SqlCommand("INSERT INTO " + firmaPrefix + "TBLSAYIMDOSYALARI" +
                            "(STOKIND,BIRIMIND,TARIH,DEPOIND,SYMDOSYAADI,SERINO,SAYILAN,ENVANTER,GIRECEKADET,CIKACAKADET,GIRISFIYATI,CIKISFIYATI,SGFIYATTIPI,SCFIYATTIPI,STENVANTEREDAHIL,BIRIMADI,ISLEMTARIHI,AKTARILDI,ADETMIKTAR)VALUES" +
                            "(@p1,@p2,@p3,@p4,@p5,@p6,@p7,@p8,@p9,@p10,@p11,@p12,@p13,@p14,@p15,@p16,@p17,@p18,@p19)", connection)
                        { Transaction = sqlTransaction };
                        cmdSayimDosyasi.Parameters.AddWithValue("@p1", globalHareket.StokNo);
                        cmdSayimDosyasi.Parameters.AddWithValue("@p2", globalHareket.BirimNo);
                        cmdSayimDosyasi.Parameters.AddWithValue("@p3", globalBaslik.Tarih.Date);
                        
                            cmdSayimDosyasi.Parameters.AddWithValue("@p4", (object)globalHareket.DepoNo ?? DBNull.Value);
                        
                       
                        cmdSayimDosyasi.Parameters.AddWithValue("@p5", globalBaslik.BaslikInd);
                        cmdSayimDosyasi.Parameters.AddWithValue("@p6", DBNull.Value);
                        cmdSayimDosyasi.Parameters.AddWithValue("@p7", globalHareket.Sayim);
                        cmdSayimDosyasi.Parameters.AddWithValue("@p8", globalHareket.Envanter);
                        cmdSayimDosyasi.Parameters.AddWithValue("@p9", globalHareket.Sayim - globalHareket.Envanter > 0 ? Math.Abs(globalHareket.Sayim - globalHareket.Envanter) : 0);
                        cmdSayimDosyasi.Parameters.AddWithValue("@p10", globalHareket.Sayim - globalHareket.Envanter < 0 ? Math.Abs(globalHareket.Sayim - globalHareket.Envanter) : 0);
                        cmdSayimDosyasi.Parameters.AddWithValue("@p11", globalHareket.Sayim - globalHareket.Envanter > 0 ? globalHareket.Maliyet : 0);
                        cmdSayimDosyasi.Parameters.AddWithValue("@p12", globalHareket.Sayim - globalHareket.Envanter < 0 ? globalHareket.Maliyet : 0);
                        cmdSayimDosyasi.Parameters.AddWithValue("@p13", 1);
                        cmdSayimDosyasi.Parameters.AddWithValue("@p14", 0);
                        cmdSayimDosyasi.Parameters.AddWithValue("@p15", 1);
                        cmdSayimDosyasi.Parameters.AddWithValue("@p16", globalHareket.BirimNo);
                        cmdSayimDosyasi.Parameters.AddWithValue("@p17", globalBaslik.Tarih);
                        cmdSayimDosyasi.Parameters.AddWithValue("@p18", 1);
                        cmdSayimDosyasi.Parameters.AddWithValue("@p19", 1);
                        cmdSayimDosyasi.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception e)
            {
                throw e;
            }
        }
        private void UpdateSatFatIrsStokUretSay()
        {
            if (connection.State != ConnectionState.Open)
                connection.Open();
            try
            {
                var tblName = "";
                switch (globalBaslik.Izahat)
                {
                    case SATFAT:
                        tblName = "TBLSATFATHAREKET";
                        break;
                    case SATFATIADE:
                        tblName = "TBLSATFATHAREKET";
                        break;
                    case SATIRS:
                        tblName = "TBLSATIRSHAREKET";
                        break;
                    case SATIRSIADE:
                        tblName = "TBLSATIRSHAREKET";
                        break;
                    case STOKCIKIS:
                        tblName = "TBLSTKCIKHAREKET";
                        break;
                    case STOKCIKISIADE:
                        tblName = "TBLSTKCIKHAREKET";
                        break;
                    case SAYIMGIRIS:
                        tblName = "TBLSAYIMGIRISHAREKET";
                        break;
                    case SAYIMCIKIS:
                        tblName = "TBLSAYIMCIKISHAREKET";
                        break;
                    case URETIMGIRIS:
                    case URETIMCIKIS:
                        tblName = "TBLSHAREKET";
                        break;
                }

                //hareketler gerçek toplam iskonto uygula
                
                //if (globalBaslik.Iskonto > 0)
                //{
                //    var tutar = Convert.ToDecimal(BaslikTutarVeAraToplam().Split(';')[0]);
                //    //var aa = Math.Round(IskontoOran(tutar, globalBaslik.Iskonto) * 100);
                //    //iskontoOran = decimal.Round(Math.Round(IskontoOran(tutar, globalBaslik.Iskonto) * 100) / 100, 4, MidpointRounding.AwayFromZero);
                //    iskontoOran = decimal.Round(IskontoOran(tutar, globalBaslik.Iskonto), 13, MidpointRounding.AwayFromZero);
                //    //iskontoOran = IskontoOran(tutar, globalBaslik.Iskonto);
                //    cmdUpdateSatFatIrsStokUretSayHareket = new SqlCommand("UPDATE " + firmaPrefix + donemPrefix + tblName + " SET GERCEKTOPLAM=(GERCEKTOPLAM - (GERCEKTOPLAM * @p1) ) WHERE EVRAKNO=@p2", connection) { Transaction = sqlTransaction };
                //    cmdUpdateSatFatIrsStokUretSayHareket.Parameters.AddWithValue("@p1", iskontoOran);
                //    cmdUpdateSatFatIrsStokUretSayHareket.Parameters.AddWithValue("@p2", globalBaslik.BaslikInd);
                //    cmdUpdateSatFatIrsStokUretSayHareket.ExecuteNonQuery();
                //}

                if (globalBaslik.Izahat != 96 && globalBaslik.Izahat != 97) 
                {
                    //cari genel ve cari hareketler güncelleme
                    if (globalBaslik.KdvDahil == 1) //fatura harekete işlenen fiyat hariç fiyat
                    {
                        cmdUpdateCariGenel = new SqlCommand("UPDATE " + firmaPrefix + donemPrefix + "TBLCARIGENELHAREKET SET BORC=(SELECT SUM(GERCEKTOPLAM *(1+CONVERT(decimal(28,8),KDV)/100)) FROM " + firmaPrefix + donemPrefix + tblName + " WHERE EVRAKNO=@p4) WHERE FIRMANO=@p2 AND BELGEIZAHAT=@p3 AND BELGEIND=@p4", connection)
                        {
                            Transaction = sqlTransaction
                        };
                    }
                    else
                    {
                        cmdUpdateCariGenel = new SqlCommand("UPDATE " + firmaPrefix + donemPrefix + "TBLCARIGENELHAREKET SET BORC=(SELECT SUM(GERCEKTOPLAM) FROM " + firmaPrefix + donemPrefix + tblName + " WHERE EVRAKNO=@p4) WHERE FIRMANO=@p2 AND BELGEIZAHAT=@p3 AND BELGEIND=@p4", connection)
                        {
                            Transaction = sqlTransaction
                        };
                    }
                    cmdUpdateCariGenel.Parameters.AddWithValue("@p2", globalBaslik.CariNo);
                    cmdUpdateCariGenel.Parameters.AddWithValue("@p3", globalBaslik.Izahat);
                    cmdUpdateCariGenel.Parameters.AddWithValue("@p4", globalBaslik.BaslikInd);
                    cmdUpdateCariGenel.ExecuteNonQuery();


                    if (globalBaslik.KdvDahil == 1) //fatura harekete işlenen fiyat hariç fiyat
                    {
                        cmdUpdateCariHareket = new SqlCommand("UPDATE " + firmaPrefix + donemPrefix + "TBLCARIHAREKETLERI SET BORC=(SELECT SUM(GERCEKTOPLAM *(1+CONVERT(decimal(28,8),KDV)/100)) FROM " + firmaPrefix + donemPrefix + tblName + " WHERE EVRAKNO=@p4) WHERE FIRMANO=@p2 AND IZAHAT=@p3 AND LN=@p4", connection)
                        {
                            Transaction = sqlTransaction
                        };
                    }
                    else
                    {
                        cmdUpdateCariHareket = new SqlCommand("UPDATE " + firmaPrefix + donemPrefix + "TBLCARIHAREKETLERI SET BORC=(SELECT SUM(GERCEKTOPLAM) FROM " + firmaPrefix + donemPrefix + tblName + " WHERE EVRAKNO=@p4) WHERE FIRMANO=@p2 AND IZAHAT=@p3 AND LN=@p4", connection)
                        {
                            Transaction = sqlTransaction
                        };
                    }
                    cmdUpdateCariHareket.Parameters.AddWithValue("@p2", globalBaslik.CariNo);
                    cmdUpdateCariHareket.Parameters.AddWithValue("@p3", globalBaslik.Izahat);
                    cmdUpdateCariHareket.Parameters.AddWithValue("@p4", globalBaslik.BaslikInd);
                    cmdUpdateCariHareket.ExecuteNonQuery();
                    //cari genel ve cari hareketler güncelleme
                }

                adpFatIrsStokHareketSatirlariGetir = new SqlDataAdapter("SELECT IND FROM " + firmaPrefix + donemPrefix + tblName + " WHERE EVRAKNO=@p1", connection);
                adpFatIrsStokHareketSatirlariGetir.SelectCommand.Parameters.AddWithValue("@p1", globalBaslik.BaslikInd);
                adpFatIrsStokHareketSatirlariGetir.SelectCommand.Transaction = sqlTransaction;
                DataTable dt = new DataTable();
                adpFatIrsStokHareketSatirlariGetir.Fill(dt);
                foreach (DataRow row in dt.Rows)
                {
                    //çıkış işlemleri satfaturası satış irsaliyesi, stok çıkış fişi
                    if (globalBaslik.Izahat == 21 || globalBaslik.Izahat == 27 || globalBaslik.Izahat == 33 || 
                        globalBaslik.Izahat == 22 || globalBaslik.Izahat == 28 || globalBaslik.Izahat == 34 ||
                        globalBaslik.Izahat == 94 || globalBaslik.Izahat == 97)
                    {
                        if (globalBaslik.Iskonto > 0)
                        {
                           /* cmdUpdateStokHareket = new SqlCommand("UPDATE " + firmaPrefix + donemPrefix + "TBLSTOKHAREKETLERI SET " +
                              "CIKAN=(SELECT MIKTAR FROM " + firmaPrefix + donemPrefix + tblName + " WHERE IND=@p3 and EVRAKNO=@p4), " +
                              "TUTAR=(SELECT FIYATI*(1-CONVERT(decimal(28,8),@p5) FROM " + firmaPrefix + donemPrefix + tblName + " WHERE IND=@p3 and EVRAKNO=@p4) WHERE LN=@p3 AND BELGENO=@p4 AND IZAHAT=@p6", connection)
                            {
                                Transaction = sqlTransaction
                            };*/
                            cmdUpdateStokHareket = new SqlCommand("UPDATE " + firmaPrefix + donemPrefix + "TBLSTOKHAREKETLERI SET " +
                              "CIKAN=(SELECT MIKTAR FROM " + firmaPrefix + donemPrefix + tblName + " WHERE IND=@p3 and EVRAKNO=@p4), " +
                              "TUTAR=(SELECT GERCEKTOPLAM FROM " + firmaPrefix + donemPrefix + tblName + " WHERE IND=@p3 and EVRAKNO=@p4)," +
                              "BIRIMMALIYET=(SELECT GERCEKTOPLAM/MIKTAR FROM " + firmaPrefix + donemPrefix + tblName + " WHERE IND=@p3 and EVRAKNO=@p4) WHERE LN=@p3 AND BELGENO=@p4 AND IZAHAT=@p6", connection)
                            {
                                Transaction = sqlTransaction
                            };

                        }
                        else
                        {
                            cmdUpdateStokHareket = new SqlCommand("UPDATE " + firmaPrefix + donemPrefix + "TBLSTOKHAREKETLERI SET " +
                              "CIKAN=(SELECT MIKTAR FROM " + firmaPrefix + donemPrefix + tblName + " WHERE IND=@p3 and EVRAKNO=@p4), " +
                              "TUTAR=(SELECT (MIKTAR* FIYATI) FROM " + firmaPrefix + donemPrefix + tblName + " WHERE IND=@p3 and EVRAKNO=@p4) WHERE LN=@p3 AND BELGENO=@p4 AND IZAHAT=@p6", connection)
                            {
                                Transaction = sqlTransaction
                            };
                        }
                    }
                    else //giriş işlemleri
                    {
                        if (globalBaslik.Iskonto > 0)
                        {
                            cmdUpdateStokHareket = new SqlCommand("UPDATE " + firmaPrefix + donemPrefix + "TBLSTOKHAREKETLERI SET " +
                              "GIREN=(SELECT MIKTAR FROM " + firmaPrefix + donemPrefix + tblName + " WHERE IND=@p3 and EVRAKNO=@p4), " +
                                 "TUTAR=(SELECT GERCEKTOPLAM FROM " + firmaPrefix + donemPrefix + tblName + " WHERE IND=@p3 and EVRAKNO=@p4) WHERE LN=@p3 AND BELGENO=@p4 AND IZAHAT=@p6", connection)
                            //"TUTAR=(SELECT (FIYATI*(1-CONVERT(decimal(28,8),@p5))) FROM " + firmaPrefix + donemPrefix + tblName + " WHERE IND=@p3 and EVRAKNO=@p4) WHERE LN=@p3 AND BELGENO=@p4 AND IZAHAT=@p6", connection)
                            {
                                Transaction = sqlTransaction
                            };
                        }
                        else
                        {
                            cmdUpdateStokHareket = new SqlCommand("UPDATE " + firmaPrefix + donemPrefix + "TBLSTOKHAREKETLERI SET " +
                              "GIREN=(SELECT MIKTAR FROM " + firmaPrefix + donemPrefix + tblName + " WHERE IND=@p3 and EVRAKNO=@p4), " +
                              "TUTAR=(SELECT GERCEKTOPLAM FROM " + firmaPrefix + donemPrefix + tblName + " WHERE IND=@p3 and EVRAKNO=@p4) WHERE LN=@p3 AND BELGENO=@p4 AND IZAHAT=@p6", connection)
                            {
                                Transaction = sqlTransaction
                            };
                        }
                    }
                    

                    cmdUpdateStokHareket.Parameters.AddWithValue("@p3", Convert.ToInt32(row[0]));
                    cmdUpdateStokHareket.Parameters.AddWithValue("@p4", globalBaslik.BaslikInd);
                    if (globalBaslik.Iskonto > 0)
                    {
                        cmdUpdateStokHareket.Parameters.AddWithValue("@p5", iskontoOran);
                    }
                    cmdUpdateStokHareket.Parameters.AddWithValue("@p6", globalBaslik.Izahat);

                    cmdUpdateStokHareket.ExecuteNonQuery();
                }
            }
            catch (Exception e)
            {
                throw e;
            }
        }
        private decimal IskontoOran(decimal tutar,decimal iskontoTutar) 
        {
            return iskontoTutar / tutar;
        }
        #endregion/*SATFAT SATIRS STOKCIKIS URETIM SAYIM ORTAK METOD*/

        #region/*ALFAT METOD*/
        private void AlFatBaslik()
        {
            if (connection.State != ConnectionState.Open)
                connection.Open();
            if(string.IsNullOrEmpty(globalBaslik.BelgeNo))
            {
                BelgeNo = BelgeNoGetir();
                globalBaslik.BelgeNo = BelgeNo;
            }
            else
            {
                BelgeNo = globalBaslik.BelgeNo;
            }

            int giris= ((int)globalBaslik.BelgeAlSat);
            int iade= globalBaslik.Iade;
            //if (globalBaslik.BelgeAlSat == VegaBelgeHareketTipi.Cikis)
            //{
            //    giris = 0;
            //    iade = 0;
            //}
            //else
            //{
            //    giris = 1;
            //    iade = 1;
            //}

            try
            {
                var headerParameter = "";
                if (headerValue != null)
                {
                    var headerArr = headerValue.Split('$');
                    for (int i = 0; i < headerArr.Length; i++)
                    {
                        headerParameter += ",@p" + (42 + i);
                        cmdAlFatIrsStokBaslik.Parameters.AddWithValue("@p" + (42 + i), headerArr[i]);
                    }
                }
                headerParameter = headerParameter.TrimStart(',');
                cmdAlFatIrsStokBaslik = new SqlCommand("INSERT INTO " + firmaPrefix + donemPrefix + "TBLALFATBASLIK " +

                    "(BELGENO,TARIH,KDV,ODEMETARIHI,ALT1,DEPO,OZELKOD1,OZELKOD2,CREDATE,LADATE,FIRMANO,GIRIS,BELGETIPI,USERNO,OZELKOD3,OZELKOD4,OZELKOD5,OZELKOD6,OZELKOD7,OZELKOD8,OZELKOD9,HAREKETDEPOSU,UID,PARABIRIMI,KUR,YUVARLAMA,ALLOWYUVARLAMA,IADE,IPTAL,AK,    ODMODIFIED,CONVERTED,ENTEGRE,SATISSEKLI,YURTDISI,MUHASEBELESMEYECEK,KAYNAK,EFATURA,STOKHAREKETEYAZ,CARIHAREKETEYAZ,YAZARKASAFISI,ALT2,ALT3,ALT4,ALTNOT ,ODENEN,MASRAF1,MASRAF2,MASRAF3,MASRAF4,MASRAFKDV1,MASRAFKDV2,MASRAFKDV3,MASRAFKDV4,EKBELGETIPI,IRSALIYELIFATURA" + headerColumn + ") OUTPUT INSERTED.IND VALUES" +//EIRSALIYE KALDIRILDI
                    "(@p1,@p2,@p3,@p4,@p5,@p6,@p7,@p8,@p9,@p10,@p11,@p12,@p13,@p14,@p15,@p16,@p17,@p18,@p19,@p20,@p21,@p22,@p23,@p24,@p25,@p26,@p27,@p28,@p29,@p30, @p31,@p32,@p33,@p34,@p35,@p36,@p37,@p38,@p39,@p40,@p41,@p42,@p43,@p44,@p45,@p46 ,@p47,@p48,@p49,@p50,@p51,@p52,@p53,@p54,@p55,@p56" + headerParameter + ")", connection)
                {
                    Transaction = sqlTransaction
                };
                cmdAlFatIrsStokBaslik.Parameters.AddWithValue("@p1", BelgeNo);
                cmdAlFatIrsStokBaslik.Parameters.AddWithValue("@p2", globalBaslik.Tarih.Date);
                cmdAlFatIrsStokBaslik.Parameters.AddWithValue("@p3", globalBaslik.KdvDahil);
                cmdAlFatIrsStokBaslik.Parameters.AddWithValue("@p4", globalBaslik.Tarih.Date);
                cmdAlFatIrsStokBaslik.Parameters.AddWithValue("@p5", globalBaslik.Iskonto);
               
                    cmdAlFatIrsStokBaslik.Parameters.AddWithValue("@p6", (object)globalBaslik.DepoNo ?? DBNull.Value);
                
              
                cmdAlFatIrsStokBaslik.Parameters.AddWithValue("@p7", globalBaslik.OzelKod1 ?? "");
                cmdAlFatIrsStokBaslik.Parameters.AddWithValue("@p8", globalBaslik.OzelKod2 ?? "");
                cmdAlFatIrsStokBaslik.Parameters.AddWithValue("@p9", DateTime.Now);
                cmdAlFatIrsStokBaslik.Parameters.AddWithValue("@p10", DateTime.Now);
                cmdAlFatIrsStokBaslik.Parameters.AddWithValue("@p11", globalBaslik.CariNo);
                cmdAlFatIrsStokBaslik.Parameters.AddWithValue("@p12", giris);
                cmdAlFatIrsStokBaslik.Parameters.AddWithValue("@p13", globalBaslik.Izahat);
                cmdAlFatIrsStokBaslik.Parameters.AddWithValue("@p14", userNo);
                cmdAlFatIrsStokBaslik.Parameters.AddWithValue("@p15", !string.IsNullOrEmpty( globalBaslik.OzelKod3) ? (object)globalBaslik.OzelKod3 : DBNull.Value);
                cmdAlFatIrsStokBaslik.Parameters.AddWithValue("@p16", !string.IsNullOrEmpty(globalBaslik.OzelKod4) ? (object)globalBaslik.OzelKod4 : DBNull.Value);
                cmdAlFatIrsStokBaslik.Parameters.AddWithValue("@p17", !string.IsNullOrEmpty(globalBaslik.OzelKod5) ? (object)globalBaslik.OzelKod5 : DBNull.Value);
                cmdAlFatIrsStokBaslik.Parameters.AddWithValue("@p18", !string.IsNullOrEmpty(globalBaslik.OzelKod6) ? (object)globalBaslik.OzelKod6 : DBNull.Value);
                cmdAlFatIrsStokBaslik.Parameters.AddWithValue("@p19", !string.IsNullOrEmpty(globalBaslik.OzelKod7) ? (object)globalBaslik.OzelKod7 : DBNull.Value);
                cmdAlFatIrsStokBaslik.Parameters.AddWithValue("@p20", !string.IsNullOrEmpty(globalBaslik.OzelKod8) ? (object)globalBaslik.OzelKod8 : DBNull.Value);
                cmdAlFatIrsStokBaslik.Parameters.AddWithValue("@p21", !string.IsNullOrEmpty(globalBaslik.OzelKod9) ? (object)globalBaslik.OzelKod9 : DBNull.Value);
                cmdAlFatIrsStokBaslik.Parameters.AddWithValue("@p22", globalBaslik.HareketDeposu);
                cmdAlFatIrsStokBaslik.Parameters.AddWithValue("@p23", "{" + Guid.NewGuid() + "}");
                cmdAlFatIrsStokBaslik.Parameters.AddWithValue("@p24", "TL");
                cmdAlFatIrsStokBaslik.Parameters.AddWithValue("@p25", 1);
                cmdAlFatIrsStokBaslik.Parameters.AddWithValue("@p26", 0);
                cmdAlFatIrsStokBaslik.Parameters.AddWithValue("@p27", 0);
                cmdAlFatIrsStokBaslik.Parameters.AddWithValue("@p28", iade);
                cmdAlFatIrsStokBaslik.Parameters.AddWithValue("@p29", 0);
                cmdAlFatIrsStokBaslik.Parameters.AddWithValue("@p30", 0);

                cmdAlFatIrsStokBaslik.Parameters.AddWithValue("@p31", 0);
                cmdAlFatIrsStokBaslik.Parameters.AddWithValue("@p32", 0);
                cmdAlFatIrsStokBaslik.Parameters.AddWithValue("@p33", 0);
                cmdAlFatIrsStokBaslik.Parameters.AddWithValue("@p34", 0);
                cmdAlFatIrsStokBaslik.Parameters.AddWithValue("@p35", 0);
                cmdAlFatIrsStokBaslik.Parameters.AddWithValue("@p36", 0);
                cmdAlFatIrsStokBaslik.Parameters.AddWithValue("@p37", 0);
                if (globalBaslik.Izahat == 165)
                {
                    cmdAlFatIrsStokBaslik.Parameters.AddWithValue("@p38", DBNull.Value);
                }
                else
                {
                    cmdAlFatIrsStokBaslik.Parameters.AddWithValue("@p38", globalBaslik.EFatura);
                }
                cmdAlFatIrsStokBaslik.Parameters.AddWithValue("@p39", 1);
                cmdAlFatIrsStokBaslik.Parameters.AddWithValue("@p40", 1);
                cmdAlFatIrsStokBaslik.Parameters.AddWithValue("@p41", 0);
                cmdAlFatIrsStokBaslik.Parameters.AddWithValue("@p42", 0);
                cmdAlFatIrsStokBaslik.Parameters.AddWithValue("@p43", 0);
                cmdAlFatIrsStokBaslik.Parameters.AddWithValue("@p44", 0);
                cmdAlFatIrsStokBaslik.Parameters.AddWithValue("@p45", globalBaslik.AltNot??"");
                cmdAlFatIrsStokBaslik.Parameters.AddWithValue("@p46", 0);

                cmdAlFatIrsStokBaslik.Parameters.AddWithValue("@p47", 0);
                cmdAlFatIrsStokBaslik.Parameters.AddWithValue("@p48", 0);
                cmdAlFatIrsStokBaslik.Parameters.AddWithValue("@p49", 0);
                cmdAlFatIrsStokBaslik.Parameters.AddWithValue("@p50", 0);
                cmdAlFatIrsStokBaslik.Parameters.AddWithValue("@p51", 0);
                cmdAlFatIrsStokBaslik.Parameters.AddWithValue("@p52", 0);
                cmdAlFatIrsStokBaslik.Parameters.AddWithValue("@p53", 0);
                cmdAlFatIrsStokBaslik.Parameters.AddWithValue("@p54", 0);

                cmdAlFatIrsStokBaslik.Parameters.AddWithValue("@p55", 0);
               
                if (globalBaslik.Izahat == 165)
                {
                    cmdAlFatIrsStokBaslik.Parameters.AddWithValue("@p56", DBNull.Value);
                }
                else
                {
                    cmdAlFatIrsStokBaslik.Parameters.AddWithValue("@p56", 1);
                }
                //cmdAlFatIrsStokBaslik.Parameters.AddWithValue("@p56", 0);

                globalBaslik.BaslikInd = (int)cmdAlFatIrsStokBaslik.ExecuteScalar();
                InsertAlFatIrsStok1();
            }
            catch (Exception e)
            {
                throw e;
            }
        }
        private void AlFatHareket()
        {
            if (connection.State != ConnectionState.Open)
                connection.Open();
            try
            {
                var SatirNo = HareketSatirSayisi();
                var stm = StokTipVeMaliyet(globalHareket.StokNo).Split(';');
                var StokTip = stm[0] == "" ? 0 : Convert.ToInt32(stm[0]);
                var Maliyet = stm[1] == "" ? 0 : Convert.ToDecimal(stm[1]);
                var res = StokBirimVeKdv(globalHareket.BirimNo).Split(';');
                var Birim = res[0].Length > 5 ? res[0].Substring(0, 5) : res[0];
                decimal KDV;
                if (globalHareket.Kdv == null || globalHareket.Kdv == 0)
                    KDV = Convert.ToDecimal(res[1]);
                else
                    KDV = globalHareket.Kdv;

                var itemParameter = "";
                if (itemValue != null)
                {
                    var itemArr = itemValue.Split('$');
                    for (int i = 0; i < itemArr.Length; i++)
                    {
                        itemParameter += ",@p" + (24 + i);
                        cmdAlFatIrsStokHareket.Parameters.AddWithValue("@p" + (24 + i), itemArr[i]);
                    }
                }
                cmdAlFatIrsStokHareket = new SqlCommand(@"INSERT INTO " + firmaPrefix + donemPrefix + "TBLALFATHAREKET " +
                    "(TARIH,EVRAKNO,FIRMANO,STOKNO,MALINCINSI,STOKKODU,STOKTIPI,MIKTAR,BIRIMMIKTAR,BIRIM,BIRIMEX,KDV,FIYATI,GERCEKTOPLAM,DEPO,ENVANTER,ACIKLAMA,GK,SATIRNO,SERIMIKTAR,MASRAF,PARABIRIMI,KUR, DETAY,ISK1,ISK2,ISK3,ISK4,ISK5,ISK6,PERSONEL,PIRIM,PROMOSYON,SATISKOSULU,GRUPMIKTAR,INDIRIM,OTV,OIV,OPSIYON,AFIYATI,KARSISTOKKODU,BARKOD,GMIKTAR,ORJFIYAT,YS " + itemColumn + ") OUTPUT INSERTED.IND VALUES" +
                    "(@p1,@p2,@p3,@p4,@p5,@p6,@p7,@p8,@p9,@p10,@p11,@p12,@p13,@p14,@p15,@p16,@p17,@p18,@p19,@p20,@p21,@p22,@p23, @p24,@p25,@p26,@p27,@p28,@p29,@p30,@p31,@p32,@p33,@p34,@p35,@p36,@p37,@p38,@p39,@p40,@p41,@p42,@p43,@p44,1 " + itemParameter + ")", connection)
                {
                    Transaction = sqlTransaction
                };
                cmdAlFatIrsStokHareket.Parameters.AddWithValue("@p1", globalHareket.Tarih.Date);
                cmdAlFatIrsStokHareket.Parameters.AddWithValue("@p2", globalBaslik.BaslikInd);
                cmdAlFatIrsStokHareket.Parameters.AddWithValue("@p3", globalBaslik.CariNo);
                cmdAlFatIrsStokHareket.Parameters.AddWithValue("@p4", globalHareket.StokNo);
                cmdAlFatIrsStokHareket.Parameters.AddWithValue("@p5", globalHareket.MalinCinsi);
                cmdAlFatIrsStokHareket.Parameters.AddWithValue("@p6", globalHareket.StokKodu);
                cmdAlFatIrsStokHareket.Parameters.AddWithValue("@p7", StokTip);
                cmdAlFatIrsStokHareket.Parameters.AddWithValue("@p8", globalHareket.Miktar);
                cmdAlFatIrsStokHareket.Parameters.AddWithValue("@p9", 1);
                cmdAlFatIrsStokHareket.Parameters.AddWithValue("@p10", Birim);
                cmdAlFatIrsStokHareket.Parameters.AddWithValue("@p11", globalHareket.BirimNo);
                cmdAlFatIrsStokHareket.Parameters.AddWithValue("@p12", KDV);
                if (globalBaslik.KdvDahil == 1) //eğer gelen fiyat kdv dahil ise kdv düşülecek yoksa gelen fiyat kdv haric
                {
                    //decimal iskonto = 0;
                    //if (globalBaslik.Iskonto > 0)
                    //{
                    //    var SatirSayisi = HareketSatirSayisi();
                    //    iskonto = globalBaslik.Iskonto / SatirSayisi;
                    //}
                    //var kdvHaricFiyat = decimal.Round(Math.Round((globalHareket.Fiyat / (1 + (KDV / 100))) * 100) / 100, 2, MidpointRounding.AwayFromZero); 
                    //var kdvHaricGercekFiyat = decimal.Round(Math.Round(((globalHareket.Fiyat * globalHareket.Miktar) / (1 + (KDV / 100))) * 100) / 100, 2, MidpointRounding.AwayFromZero);
                    var kdvHaricFiyat = globalHareket.Fiyat / (1 + (KDV / 100));
                    decimal iskontotutari = 0;
                    #region **********İskonto**************
                    if (globalHareket.ISK1 > 0)
                    {
                        iskontotutari += (globalHareket.Fiyat - iskontotutari) * globalHareket.ISK1 / 100;
                    }
                    if (globalHareket.ISK2 > 0)
                    {
                        iskontotutari += (globalHareket.Fiyat - iskontotutari) * globalHareket.ISK2 / 100;
                    }
                    if (globalHareket.ISK3 > 0)
                    {
                        iskontotutari += (globalHareket.Fiyat - iskontotutari) * globalHareket.ISK3 / 100;
                    }
                    if (globalHareket.ISK4 > 0)
                    {
                        iskontotutari += (globalHareket.Fiyat - iskontotutari) * globalHareket.ISK4 / 100;
                    }
                    if (globalHareket.ISK5 > 0)
                    {
                        iskontotutari += (globalHareket.Fiyat - iskontotutari) * globalHareket.ISK5 / 100;
                    }
                    if (globalHareket.ISK6 > 0)
                    {
                        iskontotutari += (globalHareket.Fiyat - iskontotutari) * globalHareket.ISK6 / 100;
                    }
                    #endregion **********İskonto**************

                    if (globalBaslik.Iskonto == null)
                    {
                        globalBaslik.Iskonto = iskontotutari;
                    }
                    else
                    {
                        globalBaslik.Iskonto += iskontotutari;
                    }
                    var kdvHaricGercekFiyat = ((globalHareket.Fiyat-iskontotutari) * globalHareket.Miktar) / (1 + (KDV / 100));

                    //if (iskonto > 0)
                    //    kdvHaricGercekFiyat -= iskonto;
                    cmdAlFatIrsStokHareket.Parameters.AddWithValue("@p13", kdvHaricFiyat); //kdvHaricFiyat); KDV HARİC FIYATI
                    cmdAlFatIrsStokHareket.Parameters.AddWithValue("@p14", kdvHaricGercekFiyat); // KDV HARIC GERCEKTOPLAM
                    cmdAlFatIrsStokHareket.Parameters.AddWithValue("@p40", globalHareket.Fiyat);//Maliyet);
                }
                else
                {
                    decimal iskontotutari = 0;
                    #region **********İskonto**************
                    if (globalHareket.ISK1 > 0)
                    {
                        iskontotutari += (globalHareket.Fiyat - iskontotutari) * globalHareket.ISK1 / 100;
                    }
                    if (globalHareket.ISK2 > 0)
                    {
                        iskontotutari += (globalHareket.Fiyat - iskontotutari) * globalHareket.ISK2 / 100;
                    }
                    if (globalHareket.ISK3 > 0)
                    {
                        iskontotutari += (globalHareket.Fiyat - iskontotutari) * globalHareket.ISK3 / 100;
                    }
                    if (globalHareket.ISK4 > 0)
                    {
                        iskontotutari += (globalHareket.Fiyat - iskontotutari) * globalHareket.ISK4 / 100;
                    }
                    if (globalHareket.ISK5 > 0)
                    {
                        iskontotutari += (globalHareket.Fiyat - iskontotutari) * globalHareket.ISK5 / 100;
                    }
                    if (globalHareket.ISK6 > 0)
                    {
                        iskontotutari += (globalHareket.Fiyat - iskontotutari) * globalHareket.ISK6 / 100;
                    }
                    #endregion **********İskonto**************
                    if (globalBaslik.Iskonto == null)
                    {
                        globalBaslik.Iskonto = iskontotutari;
                    }
                    else
                    {
                        globalBaslik.Iskonto += iskontotutari;
                    }
                    cmdAlFatIrsStokHareket.Parameters.AddWithValue("@p13", globalHareket.Fiyat / (1 + (KDV / 100))); //globalHareket.Fiyat); fiyat kdv hariç geliyor
                    cmdAlFatIrsStokHareket.Parameters.AddWithValue("@p14", (globalHareket.Fiyat-iskontotutari) * globalHareket.Miktar); // KDV HARIC GERCEKTOPLAM
                    cmdAlFatIrsStokHareket.Parameters.AddWithValue("@p40", globalHareket.Fiyat);//Maliyet);
                }
                cmdAlFatIrsStokHareket.Parameters.AddWithValue("@p15", globalHareket.DepoNo);
                cmdAlFatIrsStokHareket.Parameters.AddWithValue("@p16", globalHareket.Miktar);
                cmdAlFatIrsStokHareket.Parameters.AddWithValue("@p17", (object)globalHareket.Aciklama!="" ? (object)globalHareket.Aciklama : DBNull.Value);
                cmdAlFatIrsStokHareket.Parameters.AddWithValue("@p18", 0);
                cmdAlFatIrsStokHareket.Parameters.AddWithValue("@p19", SatirNo);
                cmdAlFatIrsStokHareket.Parameters.AddWithValue("@p20", 1);
                cmdAlFatIrsStokHareket.Parameters.AddWithValue("@p21", 0);
                cmdAlFatIrsStokHareket.Parameters.AddWithValue("@p22", "TL");
                cmdAlFatIrsStokHareket.Parameters.AddWithValue("@p23", 1);

                cmdAlFatIrsStokHareket.Parameters.AddWithValue("@p24", 0);
              
                cmdAlFatIrsStokHareket.Parameters.AddWithValue("@p25", globalHareket.ISK1);//ISK1
                cmdAlFatIrsStokHareket.Parameters.AddWithValue("@p26", globalHareket.ISK2);
                cmdAlFatIrsStokHareket.Parameters.AddWithValue("@p27", globalHareket.ISK3);//ISK2
                cmdAlFatIrsStokHareket.Parameters.AddWithValue("@p28", globalHareket.ISK4);//ISK3
                cmdAlFatIrsStokHareket.Parameters.AddWithValue("@p29", globalHareket.ISK5);//ISK4
                cmdAlFatIrsStokHareket.Parameters.AddWithValue("@p30", globalHareket.ISK6);//ISK5
                cmdAlFatIrsStokHareket.Parameters.AddWithValue("@p31", 0);//ISK6
                cmdAlFatIrsStokHareket.Parameters.AddWithValue("@p32", 0);
                cmdAlFatIrsStokHareket.Parameters.AddWithValue("@p33", 0);
                cmdAlFatIrsStokHareket.Parameters.AddWithValue("@p34", 1);
                cmdAlFatIrsStokHareket.Parameters.AddWithValue("@p35", 1);
                cmdAlFatIrsStokHareket.Parameters.AddWithValue("@p36", 0);
                cmdAlFatIrsStokHareket.Parameters.AddWithValue("@p37", 0);
                cmdAlFatIrsStokHareket.Parameters.AddWithValue("@p38", 0);
                cmdAlFatIrsStokHareket.Parameters.AddWithValue("@p39", CariOpsiyonGetir(globalBaslik.CariNo));
                
                cmdAlFatIrsStokHareket.Parameters.AddWithValue("@p41", "");
                cmdAlFatIrsStokHareket.Parameters.AddWithValue("@p42", "");
                cmdAlFatIrsStokHareket.Parameters.AddWithValue("@p43", globalHareket.Miktar);
                cmdAlFatIrsStokHareket.Parameters.AddWithValue("@p44", globalHareket.Fiyat);
                var hareketInd = (int)cmdAlFatIrsStokHareket.ExecuteScalar();
                InsertAlFatIrsStok2(hareketInd, KDV, StokTip);
            }
            catch (Exception e)
            {
                throw e;
            }
        }
        private void UpdateAlFatBaslik()
        {
            if (connection.State != ConnectionState.Open)
                connection.Open();
            try
            {
                var res = BaslikTutarVeAraToplam().Split(';');
                var tutar = Convert.ToDecimal(res[0]);
                var aratoplam = Convert.ToDecimal(res[1]);
                cmdUpdateAlFatIrsStokBaslik = new SqlCommand("UPDATE " + firmaPrefix + donemPrefix + "TBLALFATBASLIK SET TUTAR=@p1, ARATOPLAM=@p2 WHERE IND=@p3", connection)
                {
                    Transaction = sqlTransaction
                };
                cmdUpdateAlFatIrsStokBaslik.Parameters.AddWithValue("@p1", tutar);
                cmdUpdateAlFatIrsStokBaslik.Parameters.AddWithValue("@p2", aratoplam);
                cmdUpdateAlFatIrsStokBaslik.Parameters.AddWithValue("@p3", globalBaslik.BaslikInd);
                cmdUpdateAlFatIrsStokBaslik.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                throw e;
            }
        }
        #endregion/*ALFAT METOD*/

        #region/*ALIRS METOD*/
        private void AlIrsBaslik()
        {
            if (connection.State != ConnectionState.Open)
                connection.Open();
            if( string.IsNullOrEmpty(globalBaslik.BelgeNo))
            {
                BelgeNo = BelgeNoGetir();
                globalBaslik.BelgeNo = BelgeNo;
            }
            else
            {
                BelgeNo = globalBaslik.BelgeNo;
            }
            
            int giris=((int)globalBaslik.BelgeAlSat);
            int iade= globalBaslik.Iade;
            //if (globalBaslik.BelgeAlSat == VegaBelgeHareketTipi.Cikis)
            //{
            //    giris = 0;
            //    iade = 0;
            //}
            //else
            //{
            //    giris = 1;
            //    iade = 1;
            //}

            try
            {
                var headerParameter = "";
                if (headerValue != null)
                {
                    var headerArr = headerValue.Split('$');
                    for (int i = 0; i < headerArr.Length; i++)
                    {
                        headerParameter += ",@p" + (31 + i);
                        cmdAlFatIrsStokBaslik.Parameters.AddWithValue("@p" + (31 + i), headerArr[i]);
                    }
                }
                headerParameter = headerParameter.TrimStart(',');
                cmdAlFatIrsStokBaslik = new SqlCommand("INSERT INTO " + firmaPrefix + donemPrefix + "TBLALIRSBASLIK " +

                    "(BELGENO,TARIH,KDV,ODEMETARIHI,ALT1,DEPO,OZELKOD1,OZELKOD2,CREDATE,LADATE,FIRMANO,GIRIS,BELGETIPI,USERNO,OZELKOD3,OZELKOD4,OZELKOD5,OZELKOD6,OZELKOD7,OZELKOD8,OZELKOD9,HAREKETDEPOSU,UID,PARABIRIMI,KUR,YUVARLAMA,ALLOWYUVARLAMA,IADE,IPTAL,AK,    ODMODIFIED,CONVERTED,ENTEGRE,SATISSEKLI,YURTDISI,MUHASEBELESMEYECEK,KAYNAK,STOKHAREKETEYAZ,CARIHAREKETEYAZ ,ALT2,ALT3,ALT4,ALTNOT,ODENEN,MASRAF1,MASRAF2,MASRAF3,MASRAF4,MASRAFKDV1,MASRAFKDV2,MASRAFKDV3,MASRAFKDV4,  EIRSALIYE,EKBELGETIPI" + headerColumn + ") OUTPUT INSERTED.IND VALUES" +
                    "(@p1,@p2,@p3,@p4,@p5,@p6,@p7,@p8,@p9,@p10,@p11,@p12,@p13,@p14,@p15,@p16,@p17,@p18,@p19,@p20,@p21,@p22,@p23,@p24,@p25,@p26,@p27,@p28,@p29,@p30, @p31,@p32,@p33,@p34,@p35,@p36,@p37,@p38,@p39,@p40,@p41,@p42,@p43 ,@p44,@p45,@p46,@p47,@p48,@p49,@p50,@p51,@p52,@p53,@p54" + headerParameter + ")", connection)
                {
                    Transaction = sqlTransaction
                };
                cmdAlFatIrsStokBaslik.Parameters.AddWithValue("@p1", BelgeNo);
                cmdAlFatIrsStokBaslik.Parameters.AddWithValue("@p2", globalBaslik.Tarih.Date);
                cmdAlFatIrsStokBaslik.Parameters.AddWithValue("@p3", globalBaslik.KdvDahil);
                cmdAlFatIrsStokBaslik.Parameters.AddWithValue("@p4", globalBaslik.Tarih.Date);
                cmdAlFatIrsStokBaslik.Parameters.AddWithValue("@p5", globalBaslik.Iskonto);

                cmdAlFatIrsStokBaslik.Parameters.AddWithValue("@p6", (object)globalBaslik.DepoNo??DBNull.Value);
                
               
                cmdAlFatIrsStokBaslik.Parameters.AddWithValue("@p7", globalBaslik.OzelKod1 ?? "");
                cmdAlFatIrsStokBaslik.Parameters.AddWithValue("@p8", globalBaslik.OzelKod2 ?? "");
                cmdAlFatIrsStokBaslik.Parameters.AddWithValue("@p9", DateTime.Now);
                cmdAlFatIrsStokBaslik.Parameters.AddWithValue("@p10", DateTime.Now);
                cmdAlFatIrsStokBaslik.Parameters.AddWithValue("@p11", globalBaslik.CariNo);
                cmdAlFatIrsStokBaslik.Parameters.AddWithValue("@p12", giris);
                cmdAlFatIrsStokBaslik.Parameters.AddWithValue("@p13", globalBaslik.Izahat);
                cmdAlFatIrsStokBaslik.Parameters.AddWithValue("@p14", userNo);
                cmdAlFatIrsStokBaslik.Parameters.AddWithValue("@p15", !string.IsNullOrEmpty(globalBaslik.OzelKod3) ? (object)globalBaslik.OzelKod3 : DBNull.Value);
                cmdAlFatIrsStokBaslik.Parameters.AddWithValue("@p16", !string.IsNullOrEmpty( globalBaslik.OzelKod4) ? (object)globalBaslik.OzelKod4 : DBNull.Value);
                cmdAlFatIrsStokBaslik.Parameters.AddWithValue("@p17", !string.IsNullOrEmpty(globalBaslik.OzelKod5) ? (object)globalBaslik.OzelKod5 : DBNull.Value);
                cmdAlFatIrsStokBaslik.Parameters.AddWithValue("@p18", !string.IsNullOrEmpty(globalBaslik.OzelKod6) ? (object)globalBaslik.OzelKod6 : DBNull.Value);
                cmdAlFatIrsStokBaslik.Parameters.AddWithValue("@p19", !string.IsNullOrEmpty(globalBaslik.OzelKod7) ? (object)globalBaslik.OzelKod7 : DBNull.Value);
                cmdAlFatIrsStokBaslik.Parameters.AddWithValue("@p20", !string.IsNullOrEmpty(globalBaslik.OzelKod8) ? (object)globalBaslik.OzelKod8 : DBNull.Value);
                cmdAlFatIrsStokBaslik.Parameters.AddWithValue("@p21", !string.IsNullOrEmpty(globalBaslik.OzelKod9) ? (object)globalBaslik.OzelKod9 : DBNull.Value);
                cmdAlFatIrsStokBaslik.Parameters.AddWithValue("@p22", globalBaslik.HareketDeposu);
                cmdAlFatIrsStokBaslik.Parameters.AddWithValue("@p23", "{" + Guid.NewGuid() + "}");
                cmdAlFatIrsStokBaslik.Parameters.AddWithValue("@p24", "TL");
                cmdAlFatIrsStokBaslik.Parameters.AddWithValue("@p25", 1);
                cmdAlFatIrsStokBaslik.Parameters.AddWithValue("@p26", 0);
                cmdAlFatIrsStokBaslik.Parameters.AddWithValue("@p27", 0);
                cmdAlFatIrsStokBaslik.Parameters.AddWithValue("@p28", iade);
                cmdAlFatIrsStokBaslik.Parameters.AddWithValue("@p29", 0);
                cmdAlFatIrsStokBaslik.Parameters.AddWithValue("@p30", 0);

                cmdAlFatIrsStokBaslik.Parameters.AddWithValue("@p31", 0);
                cmdAlFatIrsStokBaslik.Parameters.AddWithValue("@p32", 0);
                cmdAlFatIrsStokBaslik.Parameters.AddWithValue("@p33", 0);
                cmdAlFatIrsStokBaslik.Parameters.AddWithValue("@p34", 0);
                cmdAlFatIrsStokBaslik.Parameters.AddWithValue("@p35", 0);
                cmdAlFatIrsStokBaslik.Parameters.AddWithValue("@p36", 0);
                cmdAlFatIrsStokBaslik.Parameters.AddWithValue("@p37", 0);
                cmdAlFatIrsStokBaslik.Parameters.AddWithValue("@p38", 1);
                cmdAlFatIrsStokBaslik.Parameters.AddWithValue("@p39", 1);
                cmdAlFatIrsStokBaslik.Parameters.AddWithValue("@p40", 0);
                cmdAlFatIrsStokBaslik.Parameters.AddWithValue("@p41", 0);
                cmdAlFatIrsStokBaslik.Parameters.AddWithValue("@p42", 0);
                cmdAlFatIrsStokBaslik.Parameters.AddWithValue("@p43", globalBaslik.AltNot);
                cmdAlFatIrsStokBaslik.Parameters.AddWithValue("@p44", 0);
                cmdAlFatIrsStokBaslik.Parameters.AddWithValue("@p45", 0);
                cmdAlFatIrsStokBaslik.Parameters.AddWithValue("@p46", 0);
                cmdAlFatIrsStokBaslik.Parameters.AddWithValue("@p47", 0);
                cmdAlFatIrsStokBaslik.Parameters.AddWithValue("@p48", 0);
                cmdAlFatIrsStokBaslik.Parameters.AddWithValue("@p49", 0);
                cmdAlFatIrsStokBaslik.Parameters.AddWithValue("@p50", 0);
                cmdAlFatIrsStokBaslik.Parameters.AddWithValue("@p51", 0);
                cmdAlFatIrsStokBaslik.Parameters.AddWithValue("@p52", 0);
                cmdAlFatIrsStokBaslik.Parameters.AddWithValue("@p53", 0);
                cmdAlFatIrsStokBaslik.Parameters.AddWithValue("@p54", 0);

                globalBaslik.BaslikInd = (int)cmdAlFatIrsStokBaslik.ExecuteScalar();
                InsertAlFatIrsStok1();
            }
            catch (Exception e)
            {
                throw e;
            }
        }
        private void AlIrsHareket()
        {
            if (connection.State != ConnectionState.Open)
                connection.Open();
            try
            {
                var SatirNo = HareketSatirSayisi();
                var stm = StokTipVeMaliyet(globalHareket.StokNo).Split(';');
                var StokTip = stm[0] == "" ? 0 : Convert.ToInt32(stm[0]);
                var Maliyet = stm[1] == "" ? 0 : Convert.ToDecimal(stm[1]);
                var res = StokBirimVeKdv(globalHareket.BirimNo).Split(';');
                var Birim = res[0].Length > 5 ? res[0].Substring(0, 5) : res[0];
                decimal KDV;
                if (globalHareket.Kdv == null || globalHareket.Kdv == 0)
                    KDV = Convert.ToDecimal(res[1]);
                else
                    KDV = globalHareket.Kdv;

                var itemParameter = "";
                if (itemValue != null)
                {
                    var itemArr = itemValue.Split('$');
                    for (int i = 0; i < itemArr.Length; i++)
                    {
                        itemParameter += ",@p" + (24 + i);
                        cmdAlFatIrsStokHareket.Parameters.AddWithValue("@p" + (24 + i), itemArr[i]);
                    }
                }
                cmdAlFatIrsStokHareket = new SqlCommand(@"INSERT INTO " + firmaPrefix + donemPrefix + "TBLALIRSHAREKET " +
                    "(TARIH,EVRAKNO,FIRMANO,STOKNO,MALINCINSI,STOKKODU,STOKTIPI,MIKTAR,BIRIMMIKTAR,BIRIM,BIRIMEX,KDV,FIYATI,GERCEKTOPLAM,DEPO,ENVANTER,ACIKLAMA,GK,SATIRNO,SERIMIKTAR,MASRAF,PARABIRIMI,KUR, DETAY,ISK1,ISK2,ISK3,ISK4,ISK5,ISK6,PERSONEL,PIRIM,PROMOSYON,SATISKOSULU,GRUPMIKTAR,INDIRIM,OTV,OIV,OPSIYON,AFIYATI,KARSISTOKKODU,BARKOD,GMIKTAR,ORJFIYAT,YS " + itemColumn + ") OUTPUT INSERTED.IND VALUES" +
                    "(@p1,@p2,@p3,@p4,@p5,@p6,@p7,@p8,@p9,@p10,@p11,@p12,@p13,@p14,@p15,@p16,@p17,@p18,@p19,@p20,@p21,@p22,@p23, @p24,@p25,@p26,@p27,@p28,@p29,@p30,@p31,@p32,@p33,@p34,@p35,@p36,@p37,@p38,@p39,@p40,@p41,@p42,@p43,@p44,1 " + itemParameter + ")", connection)
                {
                    Transaction = sqlTransaction
                };
                cmdAlFatIrsStokHareket.Parameters.AddWithValue("@p1", globalHareket.Tarih.Date);
                cmdAlFatIrsStokHareket.Parameters.AddWithValue("@p2", globalBaslik.BaslikInd);
                cmdAlFatIrsStokHareket.Parameters.AddWithValue("@p3", globalBaslik.CariNo);
                cmdAlFatIrsStokHareket.Parameters.AddWithValue("@p4", globalHareket.StokNo);
                cmdAlFatIrsStokHareket.Parameters.AddWithValue("@p5", globalHareket.MalinCinsi);
                cmdAlFatIrsStokHareket.Parameters.AddWithValue("@p6", globalHareket.StokKodu);
                cmdAlFatIrsStokHareket.Parameters.AddWithValue("@p7", StokTip);
                cmdAlFatIrsStokHareket.Parameters.AddWithValue("@p8", globalHareket.Miktar);
                cmdAlFatIrsStokHareket.Parameters.AddWithValue("@p9", 1);
                cmdAlFatIrsStokHareket.Parameters.AddWithValue("@p10", Birim);
                cmdAlFatIrsStokHareket.Parameters.AddWithValue("@p11", globalHareket.BirimNo);
                cmdAlFatIrsStokHareket.Parameters.AddWithValue("@p12", KDV);
                if (globalBaslik.KdvDahil == 1) //eğer gelen fiyat kdv dahil ise kdv düşülecek yoksa gelen fiyat kdv haric
                {
                    //decimal iskonto = 0;
                    //if (globalBaslik.Iskonto > 0)
                    //{
                    //    var SatirSayisi = HareketSatirSayisi();
                    //    iskonto = globalBaslik.Iskonto / SatirSayisi;
                    //}
                    var kdvHaricFiyat = globalHareket.Fiyat / (1 + (KDV / 100));
                    decimal iskontotutari = 0;
                    #region **********İskonto**************
                    if (globalHareket.ISK1 > 0)
                    {
                        iskontotutari += (globalHareket.Fiyat - iskontotutari) * globalHareket.ISK1 / 100;
                    }
                    if (globalHareket.ISK2 > 0)
                    {
                        iskontotutari += (globalHareket.Fiyat - iskontotutari) * globalHareket.ISK2 / 100;
                    }
                    if (globalHareket.ISK3 > 0)
                    {
                        iskontotutari += (globalHareket.Fiyat - iskontotutari) * globalHareket.ISK3 / 100;
                    }
                    if (globalHareket.ISK4 > 0)
                    {
                        iskontotutari += (globalHareket.Fiyat - iskontotutari) * globalHareket.ISK4 / 100;
                    }
                    if (globalHareket.ISK5 > 0)
                    {
                        iskontotutari += (globalHareket.Fiyat - iskontotutari) * globalHareket.ISK5 / 100;
                    }
                    if (globalHareket.ISK6 > 0)
                    {
                        iskontotutari += (globalHareket.Fiyat - iskontotutari) * globalHareket.ISK6 / 100;
                    }
                    #endregion **********İskonto**************
                    if (globalBaslik.Iskonto == null)
                    {
                        globalBaslik.Iskonto = iskontotutari;
                    }
                    else
                    {
                        globalBaslik.Iskonto += iskontotutari;
                    }
                    var kdvHaricGercekFiyat = ((globalHareket.Fiyat-iskontotutari) * globalHareket.Miktar) / (1 + (KDV / 100));
                    //if (iskonto > 0)
                    //    kdvHaricGercekFiyat -= iskonto;
                    cmdAlFatIrsStokHareket.Parameters.AddWithValue("@p13", kdvHaricFiyat); //kdvHaricFiyat); KDV HARİC FIYATI
                    cmdAlFatIrsStokHareket.Parameters.AddWithValue("@p14", kdvHaricGercekFiyat); // KDV HARIC GERCEKTOPLAM
                    cmdAlFatIrsStokHareket.Parameters.AddWithValue("@p40", globalHareket.Fiyat);//Maliyet
                }
                else
                {
                    decimal iskontotutari = 0;
                    #region **********İskonto**************
                    if (globalHareket.ISK1 > 0)
                    {
                        iskontotutari += (globalHareket.Fiyat - iskontotutari) * globalHareket.ISK1 / 100;
                    }
                    if (globalHareket.ISK2 > 0)
                    {
                        iskontotutari += (globalHareket.Fiyat - iskontotutari) * globalHareket.ISK2 / 100;
                    }
                    if (globalHareket.ISK3 > 0)
                    {
                        iskontotutari += (globalHareket.Fiyat - iskontotutari) * globalHareket.ISK3 / 100;
                    }
                    if (globalHareket.ISK4 > 0)
                    {
                        iskontotutari += (globalHareket.Fiyat - iskontotutari) * globalHareket.ISK4 / 100;
                    }
                    if (globalHareket.ISK5 > 0)
                    {
                        iskontotutari += (globalHareket.Fiyat - iskontotutari) * globalHareket.ISK5 / 100;
                    }
                    if (globalHareket.ISK6 > 0)
                    {
                        iskontotutari += (globalHareket.Fiyat - iskontotutari) * globalHareket.ISK6 / 100;
                    }
                    #endregion **********İskonto**************
                    if (globalBaslik.Iskonto == null)
                    {
                        globalBaslik.Iskonto = iskontotutari;
                    }
                    else
                    {
                        globalBaslik.Iskonto += iskontotutari;
                    }
                    cmdAlFatIrsStokHareket.Parameters.AddWithValue("@p13", globalHareket.Fiyat / (1 + (KDV / 100))); //globalHareket.Fiyat); KDV DAHIL FIYATI
                    cmdAlFatIrsStokHareket.Parameters.AddWithValue("@p14", (globalHareket.Fiyat-iskontotutari) * globalHareket.Miktar); // KDV DAHIL GERCEKTOPLAM
                    cmdAlFatIrsStokHareket.Parameters.AddWithValue("@p40", globalHareket.Fiyat);//Maliyet
                }
                cmdAlFatIrsStokHareket.Parameters.AddWithValue("@p15", globalHareket.DepoNo);
                cmdAlFatIrsStokHareket.Parameters.AddWithValue("@p16", globalHareket.Miktar);
                cmdAlFatIrsStokHareket.Parameters.AddWithValue("@p17", (object)globalHareket.Aciklama != "" ? (object)globalHareket.Aciklama : DBNull.Value);
                cmdAlFatIrsStokHareket.Parameters.AddWithValue("@p18", 0);
                cmdAlFatIrsStokHareket.Parameters.AddWithValue("@p19", SatirNo);
                cmdAlFatIrsStokHareket.Parameters.AddWithValue("@p20", 1);
                cmdAlFatIrsStokHareket.Parameters.AddWithValue("@p21", 0);
                cmdAlFatIrsStokHareket.Parameters.AddWithValue("@p22", "TL");
                cmdAlFatIrsStokHareket.Parameters.AddWithValue("@p23", 1);

                cmdAlFatIrsStokHareket.Parameters.AddWithValue("@p24", 0);
               
                cmdAlFatIrsStokHareket.Parameters.AddWithValue("@p25", globalHareket.ISK1);//ISK1
                cmdAlFatIrsStokHareket.Parameters.AddWithValue("@p26", globalHareket.ISK2);
                cmdAlFatIrsStokHareket.Parameters.AddWithValue("@p27", globalHareket.ISK3);
                cmdAlFatIrsStokHareket.Parameters.AddWithValue("@p28", globalHareket.ISK4);
                cmdAlFatIrsStokHareket.Parameters.AddWithValue("@p29", globalHareket.ISK5);
                cmdAlFatIrsStokHareket.Parameters.AddWithValue("@p30", globalHareket.ISK6);
                cmdAlFatIrsStokHareket.Parameters.AddWithValue("@p31", 0);
                cmdAlFatIrsStokHareket.Parameters.AddWithValue("@p32", 0);
                cmdAlFatIrsStokHareket.Parameters.AddWithValue("@p33", 0);
                cmdAlFatIrsStokHareket.Parameters.AddWithValue("@p34", 1);
                cmdAlFatIrsStokHareket.Parameters.AddWithValue("@p35", 1);
                cmdAlFatIrsStokHareket.Parameters.AddWithValue("@p36", 0);
                cmdAlFatIrsStokHareket.Parameters.AddWithValue("@p37", 0);
                cmdAlFatIrsStokHareket.Parameters.AddWithValue("@p38", 0);
                cmdAlFatIrsStokHareket.Parameters.AddWithValue("@p39", CariOpsiyonGetir(globalBaslik.CariNo));
            
                cmdAlFatIrsStokHareket.Parameters.AddWithValue("@p41", "");
                cmdAlFatIrsStokHareket.Parameters.AddWithValue("@p42", "");
                cmdAlFatIrsStokHareket.Parameters.AddWithValue("@p43", globalHareket.Miktar);
                cmdAlFatIrsStokHareket.Parameters.AddWithValue("@p44", globalHareket.Fiyat);
                var hareketInd = (int)cmdAlFatIrsStokHareket.ExecuteScalar();
                InsertAlFatIrsStok2(hareketInd, KDV, StokTip); 
            }
            catch (Exception e)
            {
                throw e;
            }
        }
        private void UpdateAlIrsBaslik()
        {
            if (connection.State != ConnectionState.Open)
                connection.Open();
            try
            {
                var res = BaslikTutarVeAraToplam().Split(';');
                var tutar = Convert.ToDouble(res[0]);
                var aratoplam = Convert.ToDouble(res[1]);
                cmdUpdateAlFatIrsStokBaslik = new SqlCommand("UPDATE " + firmaPrefix + donemPrefix + "TBLALIRSBASLIK SET TUTAR=@p1, ARATOPLAM=@p2 WHERE IND=@p3", connection)
                {
                    Transaction = sqlTransaction
                };
                cmdUpdateAlFatIrsStokBaslik.Parameters.AddWithValue("@p1", tutar);
                cmdUpdateAlFatIrsStokBaslik.Parameters.AddWithValue("@p2", aratoplam);
                cmdUpdateAlFatIrsStokBaslik.Parameters.AddWithValue("@p3", globalBaslik.BaslikInd);
                cmdUpdateAlFatIrsStokBaslik.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                throw e;
            }
        }
        #endregion/*ALIRS METOD*/

        #region/*STOK GIRIS METOD*/
        private void StokGirisBaslik()
        {
            if (connection.State != ConnectionState.Open)
                connection.Open();
            BelgeNo = BelgeNoGetir();
            globalBaslik.BelgeNo = BelgeNo;
            int giris= ((int)globalBaslik.BelgeAlSat);
            int iade=globalBaslik.Iade;
            //if (globalBaslik.BelgeAlSat == VegaBelgeHareketTipi.Cikis)
            //{
            //    giris = 0;
            //    iade = 0;
            //}
            //else
            //{
            //    giris = 1;
            //    iade = 1;
            //}

            try
            {
                var headerParameter = "";
                if (headerValue != null)
                {
                    var headerArr = headerValue.Split('$');
                    for (int i = 0; i < headerArr.Length; i++)
                    {
                        headerParameter += ",@p" + (31 + i);
                        cmdAlFatIrsStokBaslik.Parameters.AddWithValue("@p" + (31 + i), headerArr[i]);
                    }
                }
                headerParameter = headerParameter.TrimStart(',');
                cmdAlFatIrsStokBaslik = new SqlCommand("INSERT INTO " + firmaPrefix + donemPrefix + "TBLSTKGIRBASLIK " +

                    "(BELGENO,TARIH,KDV,ODEMETARIHI,ALT1,DEPO,OZELKOD1,OZELKOD2,CREDATE,LADATE,FIRMANO,GIRIS,BELGETIPI,USERNO,OZELKOD3,OZELKOD4,OZELKOD5,OZELKOD6,OZELKOD7,OZELKOD8,OZELKOD9,HAREKETDEPOSU,UID,PARABIRIMI,KUR,YUVARLAMA,ALLOWYUVARLAMA,IADE,IPTAL,AK,    ODMODIFIED,CONVERTED,ENTEGRE,SATISSEKLI,YURTDISI,MUHASEBELESMEYECEK,KAYNAK,STOKHAREKETEYAZ,CARIHAREKETEYAZ " + headerColumn + ") OUTPUT INSERTED.IND VALUES" +
                    "(@p1,@p2,@p3,@p4,@p5,@p6,@p7,@p8,@p9,@p10,@p11,@p12,@p13,@p14,@p15,@p16,@p17,@p18,@p19,@p20,@p21,@p22,@p23,@p24,@p25,@p26,@p27,@p28,@p29,@p30, @p31,@p32,@p33,@p34,@p35,@p36,@p37,@p38,@p39 " + headerParameter + ")", connection)
                {
                    Transaction = sqlTransaction
                };
                cmdAlFatIrsStokBaslik.Parameters.AddWithValue("@p1", BelgeNo);
                cmdAlFatIrsStokBaslik.Parameters.AddWithValue("@p2", globalBaslik.Tarih.Date);
                cmdAlFatIrsStokBaslik.Parameters.AddWithValue("@p3", globalBaslik.KdvDahil);
                cmdAlFatIrsStokBaslik.Parameters.AddWithValue("@p4", globalBaslik.Tarih.Date);
                cmdAlFatIrsStokBaslik.Parameters.AddWithValue("@p5", globalBaslik.Iskonto);
               
                    cmdAlFatIrsStokBaslik.Parameters.AddWithValue("@p6", (object)globalBaslik.DepoNo??DBNull.Value);
                
               
                cmdAlFatIrsStokBaslik.Parameters.AddWithValue("@p7", globalBaslik.OzelKod1 ?? "");
                cmdAlFatIrsStokBaslik.Parameters.AddWithValue("@p8", globalBaslik.OzelKod2 ?? "");
                cmdAlFatIrsStokBaslik.Parameters.AddWithValue("@p9", DateTime.Now);
                cmdAlFatIrsStokBaslik.Parameters.AddWithValue("@p10", DateTime.Now);
                cmdAlFatIrsStokBaslik.Parameters.AddWithValue("@p11", globalBaslik.CariNo);
                cmdAlFatIrsStokBaslik.Parameters.AddWithValue("@p12", giris);
                cmdAlFatIrsStokBaslik.Parameters.AddWithValue("@p13", globalBaslik.Izahat);
                cmdAlFatIrsStokBaslik.Parameters.AddWithValue("@p14", userNo);
                cmdAlFatIrsStokBaslik.Parameters.AddWithValue("@p15", globalBaslik.OzelKod3 ?? "");
                cmdAlFatIrsStokBaslik.Parameters.AddWithValue("@p16", globalBaslik.OzelKod4 ?? "");
                cmdAlFatIrsStokBaslik.Parameters.AddWithValue("@p17", globalBaslik.OzelKod5 ?? "");
                cmdAlFatIrsStokBaslik.Parameters.AddWithValue("@p18", globalBaslik.OzelKod6 ?? "");
                cmdAlFatIrsStokBaslik.Parameters.AddWithValue("@p19", globalBaslik.OzelKod7 ?? "");
                cmdAlFatIrsStokBaslik.Parameters.AddWithValue("@p20", globalBaslik.OzelKod8 ?? "");
                cmdAlFatIrsStokBaslik.Parameters.AddWithValue("@p21", globalBaslik.OzelKod9 ?? "");
                cmdAlFatIrsStokBaslik.Parameters.AddWithValue("@p22", globalBaslik.HareketDeposu);
                cmdAlFatIrsStokBaslik.Parameters.AddWithValue("@p23", "{" + Guid.NewGuid() + "}");
                cmdAlFatIrsStokBaslik.Parameters.AddWithValue("@p24", "TL");
                cmdAlFatIrsStokBaslik.Parameters.AddWithValue("@p25", 1);
                cmdAlFatIrsStokBaslik.Parameters.AddWithValue("@p26", 0);
                cmdAlFatIrsStokBaslik.Parameters.AddWithValue("@p27", 0);
                cmdAlFatIrsStokBaslik.Parameters.AddWithValue("@p28", iade);
                cmdAlFatIrsStokBaslik.Parameters.AddWithValue("@p29", 0);
                cmdAlFatIrsStokBaslik.Parameters.AddWithValue("@p30", 0);
                cmdAlFatIrsStokBaslik.Parameters.AddWithValue("@p31", 0);
                cmdAlFatIrsStokBaslik.Parameters.AddWithValue("@p32", 0);
                cmdAlFatIrsStokBaslik.Parameters.AddWithValue("@p33", 0);
                cmdAlFatIrsStokBaslik.Parameters.AddWithValue("@p34", 0);
                cmdAlFatIrsStokBaslik.Parameters.AddWithValue("@p35", 0);
                cmdAlFatIrsStokBaslik.Parameters.AddWithValue("@p36", 0);
                cmdAlFatIrsStokBaslik.Parameters.AddWithValue("@p37", 0);
                cmdAlFatIrsStokBaslik.Parameters.AddWithValue("@p38", 1);
                cmdAlFatIrsStokBaslik.Parameters.AddWithValue("@p39", 1);


                globalBaslik.BaslikInd = (int)cmdAlFatIrsStokBaslik.ExecuteScalar();
                InsertAlFatIrsStok1();
            }
            catch (Exception e)
            {
                throw e;
            }
        }
        private void StokGirisHareket()
        {
            if (connection.State != ConnectionState.Open)
                connection.Open();
            try
            {
                var SatirNo = HareketSatirSayisi();
                var stm = StokTipVeMaliyet(globalHareket.StokNo).Split(';');
                var StokTip = stm[0] == "" ? 0 : Convert.ToInt32(stm[0]);
                var Maliyet = stm[1] == "" ? 0 : Convert.ToDecimal(stm[1]);
                var res = StokBirimVeKdv(globalHareket.BirimNo).Split(';');
                var Birim = res[0].Length > 5 ? res[0].Substring(0, 5) : res[0];
                decimal KDV;
                if (globalHareket.Kdv == null || globalHareket.Kdv == 0)
                    KDV = Convert.ToDecimal(res[1]);
                else
                    KDV = globalHareket.Kdv;

                var itemParameter = "";
                if (itemValue != null)
                {
                    var itemArr = itemValue.Split('$');
                    for (int i = 0; i < itemArr.Length; i++)
                    {
                        itemParameter += ",@p" + (24 + i);
                        cmdAlFatIrsStokHareket.Parameters.AddWithValue("@p" + (24 + i), itemArr[i]);
                    }
                }
                cmdAlFatIrsStokHareket = new SqlCommand(@"INSERT INTO " + firmaPrefix + donemPrefix + "TBLSTKGIRHAREKET " +
                    "(TARIH,EVRAKNO,FIRMANO,STOKNO,MALINCINSI,STOKKODU,STOKTIPI,MIKTAR,BIRIMMIKTAR,BIRIM,BIRIMEX,KDV,FIYATI,GERCEKTOPLAM,DEPO,ENVANTER,ACIKLAMA,GK,SATIRNO,SERIMIKTAR,MASRAF,PARABIRIMI,KUR, DETAY,ISK1,ISK2,ISK3,ISK4,ISK5,ISK6,PERSONEL,PIRIM,PROMOSYON,SATISKOSULU,GRUPMIKTAR,INDIRIM,OTV,OIV,OPSIYON,AFIYATI " + itemColumn + ") OUTPUT INSERTED.IND VALUES" +
                    "(@p1,@p2,@p3,@p4,@p5,@p6,@p7,@p8,@p9,@p10,@p11,@p12,@p13,@p14,@p15,@p16,@p17,@p18,@p19,@p20,@p21,@p22,@p23, @p24,@p25,@p26,@p27,@p28,@p29,@p30,@p31,@p32,@p33,@p34,@p35,@p36,@p37,@p38,@p39,@p40 " + itemParameter + ")", connection)
                {
                    Transaction = sqlTransaction
                };
                cmdAlFatIrsStokHareket.Parameters.AddWithValue("@p1", globalHareket.Tarih.Date);
                cmdAlFatIrsStokHareket.Parameters.AddWithValue("@p2", globalBaslik.BaslikInd);
                cmdAlFatIrsStokHareket.Parameters.AddWithValue("@p3", globalBaslik.CariNo);
                cmdAlFatIrsStokHareket.Parameters.AddWithValue("@p4", globalHareket.StokNo);
                cmdAlFatIrsStokHareket.Parameters.AddWithValue("@p5", globalHareket.MalinCinsi);
                cmdAlFatIrsStokHareket.Parameters.AddWithValue("@p6", globalHareket.StokKodu);
                cmdAlFatIrsStokHareket.Parameters.AddWithValue("@p7", StokTip);
                cmdAlFatIrsStokHareket.Parameters.AddWithValue("@p8", globalHareket.Miktar);
                cmdAlFatIrsStokHareket.Parameters.AddWithValue("@p9", 1);
                cmdAlFatIrsStokHareket.Parameters.AddWithValue("@p10", Birim);
                cmdAlFatIrsStokHareket.Parameters.AddWithValue("@p11", globalHareket.BirimNo);
                cmdAlFatIrsStokHareket.Parameters.AddWithValue("@p12", KDV);
                if (globalBaslik.KdvDahil == 1) //eğer gelen fiyat kdv dahil ise kdv düşülecek yoksa gelen fiyat kdv haric
                {
                    //decimal iskonto = 0;
                    //if (globalBaslik.Iskonto > 0)
                    //{
                    //    var SatirSayisi = HareketSatirSayisi();
                    //    iskonto = globalBaslik.Iskonto / SatirSayisi;
                    //}
                    var kdvHaricFiyat = globalHareket.Fiyat / (1 + (KDV / 100));
                    var kdvHaricGercekFiyat = (globalHareket.Fiyat * globalHareket.Miktar) / (1 + (KDV / 100));
                    //if (iskonto > 0)
                    //    kdvHaricGercekFiyat -= iskonto;
                    cmdAlFatIrsStokHareket.Parameters.AddWithValue("@p13", kdvHaricFiyat); // KDV HARİC FIYATI
                    cmdAlFatIrsStokHareket.Parameters.AddWithValue("@p14", kdvHaricGercekFiyat); // KDV HARIC GERCEKTOPLAM
                }
                else
                {
                    cmdAlFatIrsStokHareket.Parameters.AddWithValue("@p13", globalHareket.Fiyat); // KDV DAHIL FIYATI
                    cmdAlFatIrsStokHareket.Parameters.AddWithValue("@p14", globalHareket.Fiyat * globalHareket.Miktar); // KDV DAHIL GERCEKTOPLAM
                }
                cmdAlFatIrsStokHareket.Parameters.AddWithValue("@p15", globalHareket.DepoNo);
                cmdAlFatIrsStokHareket.Parameters.AddWithValue("@p16", globalHareket.Miktar);
                cmdAlFatIrsStokHareket.Parameters.AddWithValue("@p17", globalHareket.Aciklama ?? "");
                cmdAlFatIrsStokHareket.Parameters.AddWithValue("@p18", 0);
                cmdAlFatIrsStokHareket.Parameters.AddWithValue("@p19", SatirNo);
                cmdAlFatIrsStokHareket.Parameters.AddWithValue("@p20", 1);
                cmdAlFatIrsStokHareket.Parameters.AddWithValue("@p21", 0);
                cmdAlFatIrsStokHareket.Parameters.AddWithValue("@p22", "TL");
                cmdAlFatIrsStokHareket.Parameters.AddWithValue("@p23", 1);

                cmdAlFatIrsStokHareket.Parameters.AddWithValue("@p24", 0);
                cmdAlFatIrsStokHareket.Parameters.AddWithValue("@p26", 0);
                cmdAlFatIrsStokHareket.Parameters.AddWithValue("@p25", 0);
                cmdAlFatIrsStokHareket.Parameters.AddWithValue("@p27", 0);
                cmdAlFatIrsStokHareket.Parameters.AddWithValue("@p28", 0);
                cmdAlFatIrsStokHareket.Parameters.AddWithValue("@p29", 0);
                cmdAlFatIrsStokHareket.Parameters.AddWithValue("@p30", 0);
                cmdAlFatIrsStokHareket.Parameters.AddWithValue("@p31", 0);
                cmdAlFatIrsStokHareket.Parameters.AddWithValue("@p32", 0);
                cmdAlFatIrsStokHareket.Parameters.AddWithValue("@p33", 0);
                cmdAlFatIrsStokHareket.Parameters.AddWithValue("@p34", 0);
                cmdAlFatIrsStokHareket.Parameters.AddWithValue("@p35", 1);
                cmdAlFatIrsStokHareket.Parameters.AddWithValue("@p36", 0);
                cmdAlFatIrsStokHareket.Parameters.AddWithValue("@p37", 0);
                cmdAlFatIrsStokHareket.Parameters.AddWithValue("@p38", 0);
                cmdAlFatIrsStokHareket.Parameters.AddWithValue("@p39", CariOpsiyonGetir(globalBaslik.CariNo));
                cmdAlFatIrsStokHareket.Parameters.AddWithValue("@p40", Maliyet);
                var hareketInd = (int)cmdAlFatIrsStokHareket.ExecuteScalar();
                InsertAlFatIrsStok2(hareketInd, KDV, StokTip);

            }
            catch (Exception e)
            {
                throw e;
            }
        }
        private void UpdateStokGirisBaslik()
        {
            if (connection.State != ConnectionState.Open)
                connection.Open();
            try
            {
                var res = BaslikTutarVeAraToplam().Split(';');
                var tutar = Convert.ToDouble(res[0]);
                var aratoplam = Convert.ToDouble(res[1]);
                cmdUpdateAlFatIrsStokBaslik = new SqlCommand("UPDATE " + firmaPrefix + donemPrefix + "TBLSTKGIRBASLIK SET TUTAR=@p1, ARATOPLAM=@p2 WHERE IND=@p3", connection)
                {
                    Transaction = sqlTransaction
                };
                cmdUpdateAlFatIrsStokBaslik.Parameters.AddWithValue("@p1", tutar);
                cmdUpdateAlFatIrsStokBaslik.Parameters.AddWithValue("@p2", aratoplam);
                cmdUpdateAlFatIrsStokBaslik.Parameters.AddWithValue("@p3", globalBaslik.BaslikInd);
                cmdUpdateAlFatIrsStokBaslik.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                throw e;
            }
        }
        #endregion/*STOK GIRIS METOD*/

        #region/*TALEP METOD*/

        private void TalepBaslik()
        {
            if (connection.State != ConnectionState.Open)
                connection.Open();
            BelgeNo = BelgeNoGetir();
            globalBaslik.BelgeNo = BelgeNo;
            int giris = ((int)globalBaslik.BelgeAlSat);
            int iade = globalBaslik.Iade;
            //if (globalBaslik.BelgeAlSat == VegaBelgeHareketTipi.Cikis)
            //{
            //    giris = 0;
            //    iade = 0;
            //}
            //else
            //{
            //    giris = 1;
            //    iade = 1;
            //}

            try
            {
                var headerParameter = "";
                if (headerValue != null)
                {
                    var headerArr = headerValue.Split('$');
                    for (int i = 0; i < headerArr.Length; i++)
                    {
                        headerParameter += ",@p" + (31 + i);
                        cmdAlSipBaslik.Parameters.AddWithValue("@p" + (31 + i), headerArr[i]);
                    }
                }
                headerParameter = headerParameter.TrimStart(',');
                cmdAlSipBaslik = new SqlCommand("INSERT INTO " + firmaPrefix + donemPrefix + "TBLALSIPBASLIK " +

                    "(BELGENO,TARIH,KDV,ODEMETARIHI,ALT1,DEPO,OZELKOD1,OZELKOD2,CREDATE,LADATE,FIRMANO,GIRIS,BELGETIPI,USERNO,OZELKOD3,OZELKOD4,OZELKOD5,OZELKOD6,OZELKOD7,OZELKOD8,OZELKOD9,HAREKETDEPOSU,UID,PARABIRIMI,KUR,YUVARLAMA,ALLOWYUVARLAMA,IADE,IPTAL,AK,    ODMODIFIED,CONVERTED,ENTEGRE,SATISSEKLI,YURTDISI,MUHASEBELESMEYECEK,KAYNAK,STOKHAREKETEYAZ,CARIHAREKETEYAZ,ODENEN,ALT2,ALT3,ALT4,EKBELGETIPI,MASRAF1,MASRAF2,MASRAF3,MASRAF4,MASRAFKDV1,MASRAFKDV2,MASRAFKDV3,MASRAFKDV4 " + headerColumn + ") OUTPUT INSERTED.IND VALUES" +
                    "(@p1,@p2,@p3,@p4,@p5,@p6,@p7,@p8,@p9,@p10,@p11,@p12,@p13,@p14,@p15,@p16,@p17,@p18,@p19,@p20,@p21,@p22,@p23,@p24,@p25,@p26,@p27,@p28,@p29,@p30, @p31,@p32,@p33,@p34,@p35,@p36,@p37,@p38,@p39,@p40 ,@p41 ,@p42 ,@p43,@p44 ,@p45,@p46,@p47,@p48 ,@p49,@p50,@p51,@p52" + headerParameter + ")", connection)
                {
                    Transaction = sqlTransaction
                };
                cmdAlSipBaslik.Parameters.AddWithValue("@p1", BelgeNo);
                cmdAlSipBaslik.Parameters.AddWithValue("@p2", globalBaslik.Tarih.Date);
                cmdAlSipBaslik.Parameters.AddWithValue("@p3", globalBaslik.KdvDahil);
                cmdAlSipBaslik.Parameters.AddWithValue("@p4", globalBaslik.Tarih.Date);
                cmdAlSipBaslik.Parameters.AddWithValue("@p5", globalBaslik.Iskonto);

                cmdAlSipBaslik.Parameters.AddWithValue("@p6", (object)globalBaslik.DepoNo ?? DBNull.Value);


                cmdAlSipBaslik.Parameters.AddWithValue("@p7", !string.IsNullOrEmpty(globalBaslik.OzelKod1) ? (object)globalBaslik.OzelKod1 : DBNull.Value);
                cmdAlSipBaslik.Parameters.AddWithValue("@p8", !string.IsNullOrEmpty(globalBaslik.OzelKod2) ? (object)globalBaslik.OzelKod2 : DBNull.Value);
                cmdAlSipBaslik.Parameters.AddWithValue("@p9", DateTime.Now);
                cmdAlSipBaslik.Parameters.AddWithValue("@p10", DateTime.Now);
                cmdAlSipBaslik.Parameters.AddWithValue("@p11", globalBaslik.CariNo);
                cmdAlSipBaslik.Parameters.AddWithValue("@p12", giris);
                cmdAlSipBaslik.Parameters.AddWithValue("@p13", globalBaslik.Izahat);
                cmdAlSipBaslik.Parameters.AddWithValue("@p14", userNo);
                cmdAlSipBaslik.Parameters.AddWithValue("@p15", !string.IsNullOrEmpty(globalBaslik.OzelKod3) ? (object)globalBaslik.OzelKod3 : DBNull.Value);
                cmdAlSipBaslik.Parameters.AddWithValue("@p16", !string.IsNullOrEmpty(globalBaslik.OzelKod4) ? (object)globalBaslik.OzelKod4 : DBNull.Value);
                cmdAlSipBaslik.Parameters.AddWithValue("@p17", !string.IsNullOrEmpty(globalBaslik.OzelKod5) ? (object)globalBaslik.OzelKod5 : DBNull.Value);
                cmdAlSipBaslik.Parameters.AddWithValue("@p18", !string.IsNullOrEmpty(globalBaslik.OzelKod6) ? (object)globalBaslik.OzelKod6 : DBNull.Value);
                cmdAlSipBaslik.Parameters.AddWithValue("@p19", !string.IsNullOrEmpty(globalBaslik.OzelKod7) ? (object)globalBaslik.OzelKod7 : DBNull.Value);
                cmdAlSipBaslik.Parameters.AddWithValue("@p20", !string.IsNullOrEmpty(globalBaslik.OzelKod8) ? (object)globalBaslik.OzelKod8 : DBNull.Value);
                cmdAlSipBaslik.Parameters.AddWithValue("@p21", !string.IsNullOrEmpty(globalBaslik.OzelKod9) ? (object)globalBaslik.OzelKod9 : DBNull.Value);
                cmdAlSipBaslik.Parameters.AddWithValue("@p22", globalBaslik.HareketDeposu);
                cmdAlSipBaslik.Parameters.AddWithValue("@p23", "{" + Guid.NewGuid() + "}");
                cmdAlSipBaslik.Parameters.AddWithValue("@p24", "TL");
                cmdAlSipBaslik.Parameters.AddWithValue("@p25", 1);
                cmdAlSipBaslik.Parameters.AddWithValue("@p26", 0);
                cmdAlSipBaslik.Parameters.AddWithValue("@p27", 0);
                cmdAlSipBaslik.Parameters.AddWithValue("@p28", iade);
                cmdAlSipBaslik.Parameters.AddWithValue("@p29", 0);
                cmdAlSipBaslik.Parameters.AddWithValue("@p30", 0);
                cmdAlSipBaslik.Parameters.AddWithValue("@p31", 0);
                cmdAlSipBaslik.Parameters.AddWithValue("@p32", 0);
                cmdAlSipBaslik.Parameters.AddWithValue("@p33", 0);
                cmdAlSipBaslik.Parameters.AddWithValue("@p34", 0);
                cmdAlSipBaslik.Parameters.AddWithValue("@p35", 0);
                cmdAlSipBaslik.Parameters.AddWithValue("@p36", 0);
                cmdAlSipBaslik.Parameters.AddWithValue("@p37", 0);
                cmdAlSipBaslik.Parameters.AddWithValue("@p38", 1);
                cmdAlSipBaslik.Parameters.AddWithValue("@p39", 1);
                cmdAlSipBaslik.Parameters.AddWithValue("@p40", 0);

                cmdAlSipBaslik.Parameters.AddWithValue("@p41", 0);
                cmdAlSipBaslik.Parameters.AddWithValue("@p42", 0);
                cmdAlSipBaslik.Parameters.AddWithValue("@p43", 0);

                cmdAlSipBaslik.Parameters.AddWithValue("@p44", 0);

                cmdAlSipBaslik.Parameters.AddWithValue("@p45", 0);
                cmdAlSipBaslik.Parameters.AddWithValue("@p46", 0);
                cmdAlSipBaslik.Parameters.AddWithValue("@p47", 0);
                cmdAlSipBaslik.Parameters.AddWithValue("@p48", 0);

                cmdAlSipBaslik.Parameters.AddWithValue("@p49", 0);
                cmdAlSipBaslik.Parameters.AddWithValue("@p50", 0);
                cmdAlSipBaslik.Parameters.AddWithValue("@p51", 0);
                cmdAlSipBaslik.Parameters.AddWithValue("@p52", 0);
                globalBaslik.BaslikInd = (int)cmdAlSipBaslik.ExecuteScalar();
            }
            catch (Exception e)
            {
                throw e;
            }
        }
        private void TalepHareket()
        {
            if (connection.State != ConnectionState.Open)
                connection.Open();
            try
            {
                var SatirNo = HareketSatirSayisi();
                var stm = StokTipVeMaliyet(globalHareket.StokNo).Split(';');
                var StokTip = stm[0] == "" ? 0 : Convert.ToInt32(stm[0]);
                var Maliyet = stm[1] == "" ? 0 : Convert.ToDecimal(stm[1]);
                var res = StokBirimVeKdv(globalHareket.BirimNo).Split(';');
                var Birim = res[0].Length > 5 ? res[0].Substring(0, 5) : res[0];
                decimal KDV;
                if (globalHareket.Kdv == null || globalHareket.Kdv == 0)
                    KDV = Convert.ToDecimal(res[1]);
                else
                    KDV = globalHareket.Kdv;

                var itemParameter = "";
                if (itemValue != null)
                {
                    var itemArr = itemValue.Split('$');
                    for (int i = 0; i < itemArr.Length; i++)
                    {
                        itemParameter += ",@p" + (24 + i);
                        cmdAlSipHareket.Parameters.AddWithValue("@p" + (24 + i), itemArr[i]);
                    }
                }
                cmdAlSipHareket = new SqlCommand(@"INSERT INTO " + firmaPrefix + donemPrefix + "TBLALSIPHAREKET " +
                    "(TARIH,EVRAKNO,FIRMANO,STOKNO,MALINCINSI,STOKKODU,STOKTIPI,MIKTAR,BIRIMMIKTAR,BIRIM,BIRIMEX,KDV,FIYATI,GERCEKTOPLAM,DEPO,ENVANTER,ACIKLAMA,GK,SATIRNO,SERIMIKTAR,MASRAF,PARABIRIMI,KUR, DETAY,ISK1,ISK2,ISK3,ISK4,ISK5,ISK6,PERSONEL,PIRIM,PROMOSYON,SATISKOSULU,GRUPMIKTAR,INDIRIM,OTV,OIV,OPSIYON,AFIYATI,KARSISTOKKODU,BARKOD,YS " + itemColumn + ") OUTPUT INSERTED.IND VALUES" +
                    "(@p1,@p2,@p3,@p4,@p5,@p6,@p7,@p8,@p9,@p10,@p11,@p12,@p13,@p14,@p15,@p16,@p17,@p18,@p19,@p20,@p21,@p22,@p23, @p24,@p25,@p26,@p27,@p28,@p29,@p30,@p31,@p32,@p33,@p34,@p35,@p36,@p37,@p38,@p39,@p40 ,@p41,@p42,@P43 " + itemParameter + ")", connection)
                {
                    Transaction = sqlTransaction
                };
                cmdAlSipHareket.Parameters.AddWithValue("@p1", globalHareket.Tarih.Date);
                cmdAlSipHareket.Parameters.AddWithValue("@p2", globalBaslik.BaslikInd);
                cmdAlSipHareket.Parameters.AddWithValue("@p3", globalBaslik.CariNo);
                cmdAlSipHareket.Parameters.AddWithValue("@p4", globalHareket.StokNo);
                cmdAlSipHareket.Parameters.AddWithValue("@p5", globalHareket.MalinCinsi);
                cmdAlSipHareket.Parameters.AddWithValue("@p6", globalHareket.StokKodu);
                cmdAlSipHareket.Parameters.AddWithValue("@p7", StokTip);
                cmdAlSipHareket.Parameters.AddWithValue("@p8", globalHareket.Miktar);
                cmdAlSipHareket.Parameters.AddWithValue("@p9", 1);
                cmdAlSipHareket.Parameters.AddWithValue("@p10", Birim);
                cmdAlSipHareket.Parameters.AddWithValue("@p11", globalHareket.BirimNo);
                cmdAlSipHareket.Parameters.AddWithValue("@p12", KDV);
                if (globalBaslik.KdvDahil == 1) //eğer gelen fiyat kdv dahil ise kdv düşülecek yoksa gelen fiyat kdv haric
                {
                    //decimal iskonto = 0;
                    //if (globalBaslik.Iskonto > 0)
                    //{
                    //    var SatirSayisi = HareketSatirSayisi();
                    //    iskonto = globalBaslik.Iskonto / SatirSayisi;
                    //}
                    var kdvHaricFiyat = globalHareket.Fiyat / (1 + (KDV / 100));
                    var kdvHaricGercekFiyat = (globalHareket.Fiyat * globalHareket.Miktar) / (1 + (KDV / 100));
                    //if (iskonto > 0)
                    //    kdvHaricGercekFiyat -= iskonto;
                    cmdAlSipHareket.Parameters.AddWithValue("@p13", kdvHaricFiyat); // KDV HARİC FIYATI
                    cmdAlSipHareket.Parameters.AddWithValue("@p14", kdvHaricGercekFiyat); // KDV HARIC GERCEKTOPLAM
                }
                else
                {
                    cmdAlSipHareket.Parameters.AddWithValue("@p13", globalHareket.Fiyat); // KDV DAHIL FIYATI
                    cmdAlSipHareket.Parameters.AddWithValue("@p14", globalHareket.Fiyat * globalHareket.Miktar); // KDV DAHIL GERCEKTOPLAM
                }
                cmdAlSipHareket.Parameters.AddWithValue("@p15", globalHareket.DepoNo);
                cmdAlSipHareket.Parameters.AddWithValue("@p16", globalHareket.Miktar);
                cmdAlSipHareket.Parameters.AddWithValue("@p17", !string.IsNullOrEmpty(globalHareket.Aciklama) ? (object)globalHareket.Aciklama : DBNull.Value);
                cmdAlSipHareket.Parameters.AddWithValue("@p18", 0);
                cmdAlSipHareket.Parameters.AddWithValue("@p19", SatirNo);
                cmdAlSipHareket.Parameters.AddWithValue("@p20", 1);
                cmdAlSipHareket.Parameters.AddWithValue("@p21", 0);
                cmdAlSipHareket.Parameters.AddWithValue("@p22", "TL");
                cmdAlSipHareket.Parameters.AddWithValue("@p23", 1);

                cmdAlSipHareket.Parameters.AddWithValue("@p24", 0);
                cmdAlSipHareket.Parameters.AddWithValue("@p26", 0);
                cmdAlSipHareket.Parameters.AddWithValue("@p25", 0);
                cmdAlSipHareket.Parameters.AddWithValue("@p27", 0);
                cmdAlSipHareket.Parameters.AddWithValue("@p28", 0);
                cmdAlSipHareket.Parameters.AddWithValue("@p29", 0);
                cmdAlSipHareket.Parameters.AddWithValue("@p30", 0);
                cmdAlSipHareket.Parameters.AddWithValue("@p31", 0);
                cmdAlSipHareket.Parameters.AddWithValue("@p32", 0);
                cmdAlSipHareket.Parameters.AddWithValue("@p33", 0);
                cmdAlSipHareket.Parameters.AddWithValue("@p34", 1);//SATISKOSULU
                cmdAlSipHareket.Parameters.AddWithValue("@p35", 1);
                cmdAlSipHareket.Parameters.AddWithValue("@p36", 0);
                cmdAlSipHareket.Parameters.AddWithValue("@p37", 0);
                cmdAlSipHareket.Parameters.AddWithValue("@p38", 0);
                cmdAlSipHareket.Parameters.AddWithValue("@p39", CariOpsiyonGetir(globalBaslik.CariNo));
                cmdAlSipHareket.Parameters.AddWithValue("@p40", Maliyet);

                cmdAlSipHareket.Parameters.AddWithValue("@p41", "");
                cmdAlSipHareket.Parameters.AddWithValue("@p42", "");
                cmdAlSipHareket.Parameters.AddWithValue("@p43", 1);
                var hareketInd = (int)cmdAlSipHareket.ExecuteScalar();
                //InsertAlFatIrsStok2(hareketInd, KDV, StokTip);TBLALSIPLIST
                TalepSipList(hareketInd);
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        private void TalepSipList(int SatirNo)
        {
            //INSERT[dbo].[F0104D0001TBLALSIPLIST]([IND], [BELGENO], [SATIRNO], [KALAN], [REZERV], [YS]) VALUES(100, 100, 100, CAST(200.00000000 AS Decimal(28, 8)), CAST(0.00000000 AS Decimal(28, 8)), NULL)
            if (connection.State != ConnectionState.Open)
                connection.Open();
            try
            {
                cmdAlSipList = new SqlCommand(@"INSERT INTO " + firmaPrefix + donemPrefix + "TBLALSIPLIST " +
                    "([BELGENO], [SATIRNO], [KALAN], [REZERV]) OUTPUT INSERTED.IND VALUES" +
                    "(@p1,@p2,@p3,@p4)", connection)
                {
                    Transaction = sqlTransaction
                };
                cmdAlSipList.Parameters.AddWithValue("@p1", globalBaslik.BaslikInd);
                cmdAlSipList.Parameters.AddWithValue("@p2", SatirNo);
                cmdAlSipList.Parameters.AddWithValue("@p3", globalHareket.Miktar);
                cmdAlSipList.Parameters.AddWithValue("@p4", 0);
                
                var listInd = (int)cmdAlSipList.ExecuteScalar();
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        private void UpdateTalepBaslik()
        {
            if (connection.State != ConnectionState.Open)
                connection.Open();
            try
            {
                var res = BaslikTutarVeAraToplam().Split(';');
                var tutar = Convert.ToDouble(res[0]);
                var aratoplam = Convert.ToDouble(res[1]);
                cmdUpdateTalepSipBaslik = new SqlCommand("UPDATE " + firmaPrefix + donemPrefix + "TBLALSIPBASLIK SET TUTAR=@p1, ARATOPLAM=@p2 WHERE IND=@p3", connection)
                {
                    Transaction = sqlTransaction
                };
                cmdUpdateTalepSipBaslik.Parameters.AddWithValue("@p1", tutar);
                cmdUpdateTalepSipBaslik.Parameters.AddWithValue("@p2", aratoplam);
                cmdUpdateTalepSipBaslik.Parameters.AddWithValue("@p3", globalBaslik.BaslikInd);
                cmdUpdateTalepSipBaslik.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                throw e;
            }
        }
        #endregion/*TALEP METOD*/

        #region/*ALFAT ALIRS STOKGIRIS ORTAK METOD*/
        private void InsertAlFatIrsStok1()
        {
            if (connection.State != ConnectionState.Open)
                connection.Open();
            try
            {
                cmdCariGenel = new SqlCommand("INSERT INTO " + firmaPrefix + donemPrefix + "TBLCARIGENELHAREKET " +
                    "(FIRMANO,TARIH,VADE,BELGEIND,ISLEMIND,BELGEIZAHAT,ISLEMIZAHAT,BELGELINK,BORC,ALACAK,AYLIKVADE,BELGENO,ISLEMNO,CONVERTED,IPTAL,SIRALAMATARIHI,TAHSILLINK,GECIKMEHESAPLA,PARABIRIMI,KUR,BASLIKPARABIRIMI,BASLIKKURU,ACIKLAMA,SIRALAMATARIHIEX)VALUES" +
                    "(@p1,@p2,@p3,@p4,@p5,@p6,@p7,@p8,@p9,@p10,@p11,@p12,@p13,@p14,@p15,@p16,@p17,@p18,@p19,@p20,@p21,@p22,@p23,CONVERT(decimal(28,8),@p24)  ) ", connection)
                {
                    Transaction = sqlTransaction
                };
                cmdCariGenel.Parameters.AddWithValue("@p1", globalBaslik.CariNo);
                cmdCariGenel.Parameters.AddWithValue("@p2", globalBaslik.Tarih.Date);
                cmdCariGenel.Parameters.AddWithValue("@p3", globalBaslik.Tarih.Date.AddDays(CariOpsiyonGetir(globalBaslik.CariNo)));
                cmdCariGenel.Parameters.AddWithValue("@p4", globalBaslik.BaslikInd);
                cmdCariGenel.Parameters.AddWithValue("@p5", globalBaslik.BaslikInd);
                cmdCariGenel.Parameters.AddWithValue("@p6", globalBaslik.Izahat);
                cmdCariGenel.Parameters.AddWithValue("@p7", globalBaslik.Izahat);
                cmdCariGenel.Parameters.AddWithValue("@p8", DBNull.Value);//belgelink 0 dı emre gök null yaptırdı.
                cmdCariGenel.Parameters.AddWithValue("@p9", 0);
                cmdCariGenel.Parameters.AddWithValue("@p10", 0);
                cmdCariGenel.Parameters.AddWithValue("@p11", 0);
                cmdCariGenel.Parameters.AddWithValue("@p12", BelgeNo);
                cmdCariGenel.Parameters.AddWithValue("@p13", BelgeNo);
                cmdCariGenel.Parameters.AddWithValue("@p14", 0);
                cmdCariGenel.Parameters.AddWithValue("@p15", 0);
                cmdCariGenel.Parameters.AddWithValue("@p16", globalBaslik.Tarih);
                cmdCariGenel.Parameters.AddWithValue("@p17", DBNull.Value);//tahsillink 0 dı emre gök null yaptırdı.
                cmdCariGenel.Parameters.AddWithValue("@p18", DBNull.Value);//gecikmehesapla 0 dı emre gök null yaptırdı.
                cmdCariGenel.Parameters.AddWithValue("@p19", "TL");
                cmdCariGenel.Parameters.AddWithValue("@p20", 1);
                cmdCariGenel.Parameters.AddWithValue("@p21", "TL");
                cmdCariGenel.Parameters.AddWithValue("@p22", 1);
                cmdCariGenel.Parameters.AddWithValue("@p23", "");
                cmdCariGenel.Parameters.AddWithValue("@p24", globalBaslik.Tarih);
                cmdCariGenel.ExecuteNonQuery();

                cmdCariHareketleri = new SqlCommand("INSERT INTO " + firmaPrefix + donemPrefix + "TBLCARIHAREKETLERI " +
                    "(FIRMANO,TARIH,IZAHAT,EVRAKNO,BORC,ALACAK,BAKIYE,LN,IADE,LN2,CONVNUM,CONVSTYLE,PARABIRIMI,KUR,ODEMETARIHI,ISLEMTARIHI,SIRALAMATARIHI,OZELKOD,SIRALAMATARIHIEX,OZELKOD1,OZELKOD2,OZELKOD3,OZELKOD5,OZELKOD6,OZELKOD7,OZELKOD8,OZELKOD9)VALUES" +
                    "(@p1,@p2,@p3,@p4,@p5,@p6,@p7,@p8,@p9,@p10,@p11,@p12,@p13,@p14,@p15,@p16,@p17,@p18,CONVERT(decimal(28, 8),@p19),@p20,@p21,@p22,@p23,@p24,@p25,@p26,@p27)", connection)
                {
                    Transaction = sqlTransaction
                };
                cmdCariHareketleri.Parameters.AddWithValue("@p1", globalBaslik.CariNo);
                cmdCariHareketleri.Parameters.AddWithValue("@p2", globalBaslik.Tarih.Date);
                cmdCariHareketleri.Parameters.AddWithValue("@p3", globalBaslik.Izahat);
                cmdCariHareketleri.Parameters.AddWithValue("@p4", BelgeNo);
                cmdCariHareketleri.Parameters.AddWithValue("@p5", 0);
                cmdCariHareketleri.Parameters.AddWithValue("@p6", 0);
                cmdCariHareketleri.Parameters.AddWithValue("@p7", 0);
                cmdCariHareketleri.Parameters.AddWithValue("@p8", globalBaslik.BaslikInd);
                cmdCariHareketleri.Parameters.AddWithValue("@p9", DBNull.Value);//0
                cmdCariHareketleri.Parameters.AddWithValue("@p10", DBNull.Value);//0);
                cmdCariHareketleri.Parameters.AddWithValue("@p11", DBNull.Value);//0);
                cmdCariHareketleri.Parameters.AddWithValue("@p12", DBNull.Value);//0);
                cmdCariHareketleri.Parameters.AddWithValue("@p13", "TL");
                cmdCariHareketleri.Parameters.AddWithValue("@p14", 1);
                cmdCariHareketleri.Parameters.AddWithValue("@p15", globalBaslik.Tarih.Date.AddDays(CariOpsiyonGetir(globalBaslik.CariNo)));
                cmdCariHareketleri.Parameters.AddWithValue("@p16", DateTime.Now);
                cmdCariHareketleri.Parameters.AddWithValue("@p17", DateTime.Now);//globalBaslik.Tarih);
                cmdCariHareketleri.Parameters.AddWithValue("@p18", !string.IsNullOrEmpty(globalBaslik.OzelKod4) ? (object)globalBaslik.OzelKod4 : DBNull.Value);
                cmdCariHareketleri.Parameters.AddWithValue("@p19", globalBaslik.Tarih);
                cmdCariHareketleri.Parameters.AddWithValue("@p20", !string.IsNullOrEmpty(globalBaslik.OzelKod1) ? (object)globalBaslik.OzelKod1 : "");
                cmdCariHareketleri.Parameters.AddWithValue("@p21", !string.IsNullOrEmpty(globalBaslik.OzelKod2) ? (object)globalBaslik.OzelKod2 : "");
                cmdCariHareketleri.Parameters.AddWithValue("@p22", !string.IsNullOrEmpty(globalBaslik.OzelKod3) ? (object)globalBaslik.OzelKod3 : "");
                cmdCariHareketleri.Parameters.AddWithValue("@p23", !string.IsNullOrEmpty(globalBaslik.OzelKod5) ? (object)globalBaslik.OzelKod5 : "");
                cmdCariHareketleri.Parameters.AddWithValue("@p24", !string.IsNullOrEmpty(globalBaslik.OzelKod6) ? (object)globalBaslik.OzelKod6 :"");
                cmdCariHareketleri.Parameters.AddWithValue("@p25", !string.IsNullOrEmpty(globalBaslik.OzelKod7) ? (object)globalBaslik.OzelKod7 : "");
                cmdCariHareketleri.Parameters.AddWithValue("@p26", !string.IsNullOrEmpty(globalBaslik.OzelKod8) ? (object)globalBaslik.OzelKod8 : "");
                cmdCariHareketleri.Parameters.AddWithValue("@p27", !string.IsNullOrEmpty(globalBaslik.OzelKod9) ? (object)globalBaslik.OzelKod9 :"");
                cmdCariHareketleri.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                throw e;
            }
        }
        private void InsertAlFatIrsStok2(int hareketInd, decimal KDV, int StokTip)
        {
            int iade = globalBaslik.Iade; //globalBaslik.BelgeAlSat == VegaBelgeHareketTipi.Cikis ? 0 : 1;
            if (connection.State != ConnectionState.Open)
                connection.Open();
            try
            {
                cmdStokHareket = new SqlCommand("INSERT INTO " + firmaPrefix + donemPrefix + "TBLSTOKHAREKETLERI " +
                    "(EVRAKNO,IZAHAT,TARIH,GIREN,CIKAN,KALAN,TUTAR,FIRMANO,STOKNO,BELGENO,LN,DEPO,KDV,SERINO,PERSONEL,IADE,OPSIYON,BIRIMFIYAT,BIRIMMALIYET,SIRALAMATARIHI,KUR,PARABIRIMI,BIRIMEX,ACIKLAMA,STOKTIPI,SIRALAMATARIHIEX,SATISBIRIMMALIYETI,SATISBIRIMMALIYETIKDVLI)VALUES" +
                    "(@p1,@p2,@p3,@p4,@p5,@p6,@p7,@p8,@p9,@p10,@p11,@p12,@p13,@p14,@p15,@p16,@p17,@p18,@p19,@p20,@p21,@p22,@p23,@p24,@p25,CONVERT(decimal(28, 8),@p26),@p27,@p28)", connection)
                {
                    Transaction = sqlTransaction
                };
                cmdStokHareket.Parameters.AddWithValue("@p1", BelgeNo);
                cmdStokHareket.Parameters.AddWithValue("@p2", globalBaslik.Izahat);
                cmdStokHareket.Parameters.AddWithValue("@p3", globalBaslik.Tarih.Date);
                cmdStokHareket.Parameters.AddWithValue("@p4", 0);
                cmdStokHareket.Parameters.AddWithValue("@p5", 0);
                cmdStokHareket.Parameters.AddWithValue("@p6", 0);
                cmdStokHareket.Parameters.AddWithValue("@p7", 0);
                cmdStokHareket.Parameters.AddWithValue("@p8", globalBaslik.CariNo);
                cmdStokHareket.Parameters.AddWithValue("@p9", globalHareket.StokNo);
                cmdStokHareket.Parameters.AddWithValue("@p10", globalBaslik.BaslikInd);
                cmdStokHareket.Parameters.AddWithValue("@p11", hareketInd);
               
                    cmdStokHareket.Parameters.AddWithValue("@p12", (object)globalHareket.DepoNo ?? DBNull.Value);
                
                
                cmdStokHareket.Parameters.AddWithValue("@p13", KDV);
                cmdStokHareket.Parameters.AddWithValue("@p14", DBNull.Value);//0
                cmdStokHareket.Parameters.AddWithValue("@p15", 0);
                cmdStokHareket.Parameters.AddWithValue("@p16", iade); //
                cmdStokHareket.Parameters.AddWithValue("@p17", CariOpsiyonGetir(globalBaslik.CariNo));

                if (globalBaslik.KdvDahil == 1) //eğer gelen fiyat kdv dahil ise kdv düşülecek yoksa gelen fiyat kdv haric
                {
                    var kdvHaricFiyat = globalHareket.Fiyat / (1 + (KDV / 100));
                    decimal iskontotutari = 0;
                    #region **********İskonto**************
                    if (globalHareket.ISK1 > 0)
                    {
                        iskontotutari += (kdvHaricFiyat - iskontotutari) * globalHareket.ISK1 / 100;
                    }
                    if (globalHareket.ISK2 > 0)
                    {
                        iskontotutari += (kdvHaricFiyat - iskontotutari) * globalHareket.ISK2 / 100;
                    }
                    if (globalHareket.ISK3 > 0)
                    {
                        iskontotutari += (kdvHaricFiyat - iskontotutari) * globalHareket.ISK3 / 100;
                    }
                    if (globalHareket.ISK4 > 0)
                    {
                        iskontotutari += (kdvHaricFiyat - iskontotutari) * globalHareket.ISK4 / 100;
                    }
                    if (globalHareket.ISK5 > 0)
                    {
                        iskontotutari += (kdvHaricFiyat - iskontotutari) * globalHareket.ISK5 / 100;
                    }
                    if (globalHareket.ISK6 > 0)
                    {
                        iskontotutari += (kdvHaricFiyat - iskontotutari) * globalHareket.ISK6 / 100;
                    }
                    #endregion **********İskonto**************
                    cmdStokHareket.Parameters.AddWithValue("@p18", kdvHaricFiyat);
                    cmdStokHareket.Parameters.AddWithValue("@p19", kdvHaricFiyat- iskontotutari);
                }
                else
                {
                    decimal iskontotutari = 0;
                    #region **********İskonto**************
                    if (globalHareket.ISK1 > 0)
                    {
                        iskontotutari += (globalHareket.Fiyat - iskontotutari) * globalHareket.ISK1 / 100;
                    }
                    if (globalHareket.ISK2 > 0)
                    {
                        iskontotutari += (globalHareket.Fiyat - iskontotutari) * globalHareket.ISK2 / 100;
                    }
                    if (globalHareket.ISK3 > 0)
                    {
                        iskontotutari += (globalHareket.Fiyat - iskontotutari) * globalHareket.ISK3 / 100;
                    }
                    if (globalHareket.ISK4 > 0)
                    {
                        iskontotutari += (globalHareket.Fiyat - iskontotutari) * globalHareket.ISK4 / 100;
                    }
                    if (globalHareket.ISK5 > 0)
                    {
                        iskontotutari += (globalHareket.Fiyat - iskontotutari) * globalHareket.ISK5 / 100;
                    }
                    if (globalHareket.ISK6 > 0)
                    {
                        iskontotutari += (globalHareket.Fiyat - iskontotutari) * globalHareket.ISK6 / 100;
                    }
                    #endregion **********İskonto**************
                    cmdStokHareket.Parameters.AddWithValue("@p18", globalHareket.Fiyat);
                    cmdStokHareket.Parameters.AddWithValue("@p19", globalHareket.Fiyat- iskontotutari);
                }
                cmdStokHareket.Parameters.AddWithValue("@p20", globalHareket.Tarih);
                cmdStokHareket.Parameters.AddWithValue("@p21", 1);
                cmdStokHareket.Parameters.AddWithValue("@p22", "TL");
                cmdStokHareket.Parameters.AddWithValue("@p23", globalHareket.BirimNo);
                cmdStokHareket.Parameters.AddWithValue("@p24", !string.IsNullOrEmpty( globalHareket.Aciklama)? (object)globalHareket.Aciklama : DBNull.Value);
                cmdStokHareket.Parameters.AddWithValue("@p25", StokTip);
                cmdStokHareket.Parameters.AddWithValue("@p26", globalHareket.Tarih);
                cmdStokHareket.Parameters.AddWithValue("@p27", DBNull.Value);//0
                cmdStokHareket.Parameters.AddWithValue("@p28", DBNull.Value);
                cmdStokHareket.ExecuteNonQuery();

                cmdDepoEnvater = new SqlCommand("INSERT INTO " + firmaPrefix + donemPrefix + "TBLDEPOENVANTER " +
                    "(TARIH,STOKNO,DEPO,ENVANTER,BELGETIPI,BELGEIND,SIRALAMATARIHI,SIRALAMATARIHIEX,HAREKETIND)VALUES" +
                    "(@p1,@p2,@p3,@p4,@p5,@p6,@p7,CONVERT(decimal(28, 8),@p8),@p9)", connection)
                {
                    Transaction = sqlTransaction
                };
                cmdDepoEnvater.Parameters.AddWithValue("@p1", globalBaslik.Tarih.Date);
                cmdDepoEnvater.Parameters.AddWithValue("@p2", globalHareket.StokNo);
                
                    cmdDepoEnvater.Parameters.AddWithValue("@p3", (object)globalHareket.DepoNo ?? DBNull.Value);
                
               
                cmdDepoEnvater.Parameters.AddWithValue("@p4", globalHareket.Miktar);
                cmdDepoEnvater.Parameters.AddWithValue("@p5", globalBaslik.Izahat);
                cmdDepoEnvater.Parameters.AddWithValue("@p6", globalBaslik.BaslikInd);
                cmdDepoEnvater.Parameters.AddWithValue("@p7", globalHareket.Tarih);
                cmdDepoEnvater.Parameters.AddWithValue("@p8", globalHareket.Tarih);
                cmdDepoEnvater.Parameters.AddWithValue("@p9", hareketInd);
                cmdDepoEnvater.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                throw e;
            }
        }
        private void UpdateAlFatIrsStok()
        {
            if (connection.State != ConnectionState.Open)
                connection.Open();
            try
            {
                var tblName = "";
                switch (globalBaslik.Izahat)
                {
                    case SATFAT:
                        tblName = "TBLSATFATHAREKET";
                        break;
                    case SATFATIADE:
                        tblName = "TBLSATFATHAREKET";
                        break;
                    case SATIRS:
                        tblName = "TBLSATIRSHAREKET";
                        break;
                    case SATIRSIADE:
                        tblName = "TBLSATIRSHAREKET";
                        break;
                    case STOKCIKIS:
                        tblName = "TBLSTKCIKHAREKET";
                        break;
                    case STOKCIKISIADE:
                        tblName = "TBLSTKCIKHAREKET";
                        break;
                    case ALFAT:
                        tblName = "TBLALFATHAREKET";
                        break;
                    case GIDERFATURASI:
                        tblName = "TBLALFATHAREKET";
                        break;
                    case ALIRS:
                        tblName = "TBLALIRSHAREKET";
                        break;
                    case STOKGIRIS:
                        tblName = "TBLSTKGIRHAREKET";
                        break;
                    case ALFATIADE:
                        tblName = "TBLALFATHAREKET";
                        break;
                    case ALIRSIADE:
                        tblName = "TBLALIRSHAREKET";
                        break;
                    case STOKGIRISIADE:
                        tblName = "TBLSTKGIRHAREKET";
                        break;
                }

                //hareketler gerçek toplam iskonto uygula

                //if (globalBaslik.Iskonto > 0)
                //{
                //    var tutar = Convert.ToDecimal(BaslikTutarVeAraToplam().Split(';')[0]);
                //    iskontoOran = decimal.Round(IskontoOran(tutar, globalBaslik.Iskonto), 13, MidpointRounding.AwayFromZero);
                //    cmdUpdateAlFatIrsStokHareket = new SqlCommand("UPDATE " + firmaPrefix + donemPrefix + tblName + " SET GERCEKTOPLAM=(GERCEKTOPLAM - (GERCEKTOPLAM * @p1) ) WHERE EVRAKNO=@p2", connection) { Transaction = sqlTransaction };
                //    cmdUpdateAlFatIrsStokHareket.Parameters.AddWithValue("@p1", iskontoOran);
                //    cmdUpdateAlFatIrsStokHareket.Parameters.AddWithValue("@p2", globalBaslik.BaslikInd);
                //    cmdUpdateAlFatIrsStokHareket.ExecuteNonQuery();
                //}

                //if (globalBaslik.KdvDahil == 1) //fatura harekete işlenen fiyat hariç fiyat
                //{
                    cmdUpdateCariGenel = new SqlCommand("UPDATE " + firmaPrefix + donemPrefix + "TBLCARIGENELHAREKET SET ALACAK=(SELECT SUM(GERCEKTOPLAM *(1+CONVERT(decimal(28,8),KDV)/100)) FROM " + firmaPrefix + donemPrefix + tblName + " WHERE EVRAKNO=@p4) WHERE FIRMANO=@p2 AND BELGEIZAHAT=@p3 AND BELGEIND=@p4", connection)
                    {
                        Transaction = sqlTransaction
                    };
                //}
                //else
                //{
                //    cmdUpdateCariGenel = new SqlCommand("UPDATE " + firmaPrefix + donemPrefix + "TBLCARIGENELHAREKET SET ALACAK=(SELECT SUM(GERCEKTOPLAM) FROM " + firmaPrefix + donemPrefix + tblName + " WHERE EVRAKNO=@p4) WHERE FIRMANO=@p2 AND BELGEIZAHAT=@p3 AND BELGEIND=@p4", connection)
                //    {
                //        Transaction = sqlTransaction
                //    };
                //}
                cmdUpdateCariGenel.Parameters.AddWithValue("@p2", globalBaslik.CariNo);
                cmdUpdateCariGenel.Parameters.AddWithValue("@p3", globalBaslik.Izahat);
                cmdUpdateCariGenel.Parameters.AddWithValue("@p4", globalBaslik.BaslikInd);
                cmdUpdateCariGenel.ExecuteNonQuery();


                //if (globalBaslik.KdvDahil == 1) //fatura harekete işlenen fiyat hariç fiyat
                //{
                    cmdUpdateCariHareket = new SqlCommand("UPDATE " + firmaPrefix + donemPrefix + "TBLCARIHAREKETLERI SET ALACAK=(SELECT SUM(GERCEKTOPLAM *(1+CONVERT(decimal(28,8),KDV)/100)) FROM " + firmaPrefix + donemPrefix + tblName + " WHERE EVRAKNO=@p4) WHERE FIRMANO=@p2 AND IZAHAT=@p3 AND LN=@p4", connection)
                    {
                        Transaction = sqlTransaction
                    };
                //}
                //else
                //{
                //    cmdUpdateCariHareket = new SqlCommand("UPDATE " + firmaPrefix + donemPrefix + "TBLCARIHAREKETLERI SET ALACAK=(SELECT SUM(GERCEKTOPLAM) FROM " + firmaPrefix + donemPrefix + tblName + " WHERE EVRAKNO=@p4) WHERE FIRMANO=@p2 AND IZAHAT=@p3 AND LN=@p4", connection)
                //    {
                //        Transaction = sqlTransaction
                //    };
                //}
                cmdUpdateCariHareket.Parameters.AddWithValue("@p2", globalBaslik.CariNo);
                cmdUpdateCariHareket.Parameters.AddWithValue("@p3", globalBaslik.Izahat);
                cmdUpdateCariHareket.Parameters.AddWithValue("@p4", globalBaslik.BaslikInd);
                cmdUpdateCariHareket.ExecuteNonQuery();


                adpFatIrsStokHareketSatirlariGetir = new SqlDataAdapter("SELECT IND FROM " + firmaPrefix + donemPrefix + tblName + " WHERE EVRAKNO=@p1", connection);
                adpFatIrsStokHareketSatirlariGetir.SelectCommand.Parameters.AddWithValue("@p1", globalBaslik.BaslikInd);
                adpFatIrsStokHareketSatirlariGetir.SelectCommand.Transaction = sqlTransaction;
                DataTable dt = new DataTable();
                adpFatIrsStokHareketSatirlariGetir.Fill(dt);
                foreach (DataRow row in dt.Rows)
                {
                    #region MyRegion
                    //if (globalBaslik.KdvDahil == 1) //fatura harekette işlenen fiyat hariç fiyat
                    //{
                    //    cmdUpdateStokHareket = new SqlCommand("UPDATE " + firmaPrefix + donemPrefix + "TBLSTOKHAREKETLERI SET " +
                    //    "GIREN=(SELECT MIKTAR FROM " + firmaPrefix + donemPrefix + tblName + " WHERE IND=@p3 and EVRAKNO=@p4), " +
                    //    "TUTAR=(SELECT MIKTAR * (FIYATI *(1+CONVERT(decimal(28,8),KDV)/100) ) FROM " + firmaPrefix + donemPrefix + tblName + " WHERE IND=@p3 and EVRAKNO=@p4) WHERE LN=@p3 AND BELGENO=@p4", connection)
                    //    {
                    //        Transaction = sqlTransaction
                    //    };
                    //}
                    //else
                    //{
                    //    cmdUpdateStokHareket = new SqlCommand("UPDATE " + firmaPrefix + donemPrefix + "TBLSTOKHAREKETLERI SET " +
                    //   "GIREN=(SELECT MIKTAR FROM " + firmaPrefix + donemPrefix + tblName + " WHERE IND=@p3 and EVRAKNO=@p4), " +
                    //   "TUTAR=(SELECT (MIKTAR* FIYATI) FROM " + firmaPrefix + donemPrefix + tblName + " WHERE IND=@p3 and EVRAKNO=@p4) WHERE LN=@p3 AND BELGENO=@p4", connection)
                    //    {
                    //        Transaction = sqlTransaction
                    //    };
                    //}
                    //BU BÖLÜM İNCELENECEK tblstokhareketleri tablosuna tutar güncellenirken daima kdv hariç yaptık.. KONTROL EDİLECEK 
                    #endregion


                    if (globalBaslik.Iskonto > 0)
                    {
                        cmdUpdateStokHareket = new SqlCommand("UPDATE " + firmaPrefix + donemPrefix + "TBLSTOKHAREKETLERI SET " +
                          "GIREN=(SELECT MIKTAR FROM " + firmaPrefix + donemPrefix + tblName + " WHERE IND=@p3 and EVRAKNO=@p4), " +
                          "TUTAR=(SELECT GERCEKTOPLAM FROM " + firmaPrefix + donemPrefix + tblName + " WHERE IND=@p3 and EVRAKNO=@p4) WHERE LN=@p3 AND BELGENO=@p4 AND IZAHAT=@p6", connection)
                        {
                            Transaction = sqlTransaction
                        };
                    }
                    else 
                    {
                        cmdUpdateStokHareket = new SqlCommand("UPDATE " + firmaPrefix + donemPrefix + "TBLSTOKHAREKETLERI SET " +
                          "GIREN=(SELECT MIKTAR FROM " + firmaPrefix + donemPrefix + tblName + " WHERE IND=@p3 and EVRAKNO=@p4), " +
                          "TUTAR=(SELECT GERCEKTOPLAM FROM " + firmaPrefix + donemPrefix + tblName + " WHERE IND=@p3 and EVRAKNO=@p4) WHERE LN=@p3 AND BELGENO=@p4 AND IZAHAT=@p6", connection)
                        {
                            Transaction = sqlTransaction
                        };
                    }
                    

                    cmdUpdateStokHareket.Parameters.AddWithValue("@p3", Convert.ToInt32(row[0]));
                    cmdUpdateStokHareket.Parameters.AddWithValue("@p4", globalBaslik.BaslikInd);
                    if (globalBaslik.Iskonto > 0)
                    {
                        cmdUpdateStokHareket.Parameters.AddWithValue("@p5", iskontoOran);
                    }
                    cmdUpdateStokHareket.Parameters.AddWithValue("@p6", globalBaslik.Izahat);

                    cmdUpdateStokHareket.ExecuteNonQuery();
                }
            }
            catch (Exception e)
            {
                throw e;
            }
        }
        #endregion/*ALFAT ALIRS STOKGIRIS ORTAK METOD*/



        #region/*TAHSILAT METOD*/
        private void TahsilatBaslik() 
        {
            if (connection.State != ConnectionState.Open)
                connection.Open();
            BelgeNo = BelgeNoGetir();
            globalBaslik.BelgeNo = BelgeNo;
            int giris;
            if (globalBaslik.BelgeAlSat == VegaBelgeHareketTipi.Cikis)
                giris = 0;
            else
                giris = 1;
            try
            {
                cmdTahsilBaslik = new SqlCommand("INSERT INTO " + firmaPrefix + donemPrefix + "TBLCARGIRBASLIK (FIRMANO,BELGENO,TARIH,IADE,TUTAR,PARABIRIMI,GIRIS,KUR,BELGETIPI,IPTAL,USERNO,OZELKOD1,OZELKOD2,ACIKLAMA,MUHASEBELESMEYECEK,OZELKOD3,OZELKOD4,CREDATE,KAYNAK,OZELKOD5,UID) OUTPUT INSERTED.IND VALUES" +
                    "(@p1,@p2,@p3,@p4,@p5,@p6,@p7,@p8,@p9,@p10,@p11,@p12,@p13,@p14,@p15,@p16,@p17,@p18,@p19,@p20,@p21)", connection)
                {
                    Transaction = sqlTransaction
                };
                cmdTahsilBaslik.Parameters.AddWithValue("@p1", globalBaslik.CariNo);
                cmdTahsilBaslik.Parameters.AddWithValue("@p2", BelgeNo);
                cmdTahsilBaslik.Parameters.AddWithValue("@p3", globalBaslik.Tarih.Date);
                cmdTahsilBaslik.Parameters.AddWithValue("@p4", 0);
                cmdTahsilBaslik.Parameters.AddWithValue("@p5", 0); //TUTAR update ile güncellenecek
                cmdTahsilBaslik.Parameters.AddWithValue("@p6", "TL");
                cmdTahsilBaslik.Parameters.AddWithValue("@p7", giris);
                cmdTahsilBaslik.Parameters.AddWithValue("@p8", 1);
                cmdTahsilBaslik.Parameters.AddWithValue("@p9", globalBaslik.Izahat);
                cmdTahsilBaslik.Parameters.AddWithValue("@p10",0);
                cmdTahsilBaslik.Parameters.AddWithValue("@p11",userNo);
                cmdTahsilBaslik.Parameters.AddWithValue("@p12",globalBaslik.OzelKod1);
                cmdTahsilBaslik.Parameters.AddWithValue("@p13",globalBaslik.OzelKod2);
                cmdTahsilBaslik.Parameters.AddWithValue("@p14",globalBaslik.AltNot);
                cmdTahsilBaslik.Parameters.AddWithValue("@p15",0);
                cmdTahsilBaslik.Parameters.AddWithValue("@p16",globalBaslik.OzelKod3);
                cmdTahsilBaslik.Parameters.AddWithValue("@p17",globalBaslik.OzelKod4);
                cmdTahsilBaslik.Parameters.AddWithValue("@p18",DateTime.Now);
                cmdTahsilBaslik.Parameters.AddWithValue("@p19",0);
                cmdTahsilBaslik.Parameters.AddWithValue("@p20",globalBaslik.OzelKod5);
                cmdTahsilBaslik.Parameters.AddWithValue("@p21", "{" + Guid.NewGuid() + "}");

                globalBaslik.BaslikInd = (int)cmdTahsilBaslik.ExecuteScalar();

                TahsilCariHareket();

            }
            catch (Exception e)
            {
                throw e;
            }
        }
        private void TahsilatHareket()
        {
            if (connection.State != ConnectionState.Open)
                connection.Open();
            try
            {
                int belgeLink = 0;
                int portNo = 0;

                var ress = TahsilVadeSuresiVeKartAdi().Split(';'); //c.Dispose sorun çıkarmasın
                var blokeGun = ress[0] == "" ? 0 : Convert.ToInt32(ress[0]);
                var kartAdi = ress[1];

                cmdTahsilHareket = new SqlCommand("INSERT INTO " + firmaPrefix + donemPrefix + "TBLCARGIRHAREKET (IZAHAT,PORTNO,EVRAKNO,ACIKLAMA,VADE,TUTAR,PARABIRIMI,BELGENO,BELGELINK,STATUS,KUR,BANKANO,FIRMANO,AYLIKVADE) OUTPUT INSERTED.IND VALUES" +
                    "(@p1,@p2,@p3,@p4,@p5,@p6,@p7,@p8,@p9,@p10,@p11,@p12,@p13,@p14)", connection)
                {
                    Transaction = sqlTransaction
                };
                cmdTahsilHareket.Parameters.AddWithValue("@p1", globalHareket.Izahat);

                switch (globalHareket.Izahat)
                {
                    case 1:
                        portNo = -1;
                        belgeLink = -1;
                        cmdTahsilHareket.Parameters.AddWithValue("@p2", portNo);
                        cmdTahsilHareket.Parameters.AddWithValue("@p9", belgeLink);
                        cmdTahsilHareket.Parameters.AddWithValue("@p10", 0);
                        break;
                    case 2:
                        break;
                    case 3:
                        break;
                    case 4:
                        var res = TahsilVisaGirisVePortFoy(kartAdi).Split(';'); //önce giris tablosu sonra portföy tablosu girişteki
                        portNo = Convert.ToInt32(res[1]);
                        belgeLink = Convert.ToInt32(res[0]);
                        cmdTahsilHareket.Parameters.AddWithValue("@p2", portNo);
                        cmdTahsilHareket.Parameters.AddWithValue("@p9", belgeLink);
                        cmdTahsilHareket.Parameters.AddWithValue("@p10", 27);
                        break;
                    case 5:
                        break;
                    case 6:
                        break;
                    case 7:
                        break;
                    case 8:
                        break;
                    case 9:
                        break;
                    case 10:
                        portNo = -1;
                        belgeLink = -1;
                        cmdTahsilHareket.Parameters.AddWithValue("@p2", portNo);
                        cmdTahsilHareket.Parameters.AddWithValue("@p9", belgeLink);
                        cmdTahsilHareket.Parameters.AddWithValue("@p10", 0);
                        break;
                    case 11:
                        portNo = -1;
                        belgeLink = -1;
                        cmdTahsilHareket.Parameters.AddWithValue("@p2", portNo);
                        cmdTahsilHareket.Parameters.AddWithValue("@p9", belgeLink);
                        cmdTahsilHareket.Parameters.AddWithValue("@p10", 0);
                        break;
                    case 12:
                        break;
                }
                cmdTahsilHareket.Parameters.AddWithValue("@p3", globalBaslik.BaslikInd);
                cmdTahsilHareket.Parameters.AddWithValue("@p4", !String.IsNullOrEmpty(kartAdi)?kartAdi:globalHareket.Aciklama);
                cmdTahsilHareket.Parameters.AddWithValue("@p5", globalHareket.Tarih.AddDays(globalHareket.Vade == null ? 0 : globalHareket.Vade + blokeGun).Date);//date eklencek
                cmdTahsilHareket.Parameters.AddWithValue("@p6", globalHareket.Fiyat);
                cmdTahsilHareket.Parameters.AddWithValue("@p7", "TL");
                cmdTahsilHareket.Parameters.AddWithValue("@p8", ""); //çek senet devreye girince belge nosu
                cmdTahsilHareket.Parameters.AddWithValue("@p11", 1);
                cmdTahsilHareket.Parameters.AddWithValue("@p12", globalHareket.BankaId);
                cmdTahsilHareket.Parameters.AddWithValue("@p13", globalBaslik.CariNo);
                cmdTahsilHareket.Parameters.AddWithValue("@p14", 0);
                var hareketInd = (int)cmdTahsilHareket.ExecuteScalar();

                TahsilCariGenel(belgeLink);
                switch (globalHareket.Izahat)
                {
                    case 1:
                    case 2:
                    case 3:
                        TahsilNakitCekSenet(hareketInd);
                        break;
                }

            }
            catch (Exception e)
            {
                throw e;
            }
        }
        private void UpdateTahsilBaslik()
        {
            if (connection.State != ConnectionState.Open)
                connection.Open();
            try
            {
                cmdUpdateTahsilBaslik = new SqlCommand("UPDATE " + firmaPrefix + donemPrefix + "TBLCARGIRBASLIK SET TUTAR=(SELECT SUM(TUTAR) FROM " + firmaPrefix + donemPrefix + "TBLCARGIRHAREKET WHERE EVRAKNO=@p1) WHERE IND=@p1", connection) { Transaction = sqlTransaction };
                cmdUpdateTahsilBaslik.Parameters.AddWithValue("@p1", globalBaslik.BaslikInd);
                cmdUpdateTahsilBaslik.ExecuteNonQuery();

                cmdUpdateTahsilCariHareket = new SqlCommand("UPDATE " + firmaPrefix + donemPrefix + "TBLCARIHAREKETLERI SET ALACAK=(SELECT SUM(TUTAR) FROM " + firmaPrefix + donemPrefix + "TBLCARGIRHAREKET WHERE EVRAKNO=@p1) WHERE LN=@p1 AND IZAHAT=@p2 ", connection) { Transaction = sqlTransaction };
                cmdUpdateTahsilCariHareket.Parameters.AddWithValue("@p1", globalBaslik.BaslikInd);
                cmdUpdateTahsilCariHareket.Parameters.AddWithValue("@p2", globalBaslik.Izahat);
                cmdUpdateTahsilCariHareket.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                throw e;
            }
        }
        #endregion/*TAHSILAT METOD*/

        #region/*TAHSILAT IADE METOD*/
        private void TahsilatIadeBaslik()
        {
            if (connection.State != ConnectionState.Open)
                connection.Open();
            BelgeNo = BelgeNoGetir();
            globalBaslik.BelgeNo = BelgeNo;
            int giris;
            if (globalBaslik.BelgeAlSat == VegaBelgeHareketTipi.Cikis)
                giris = 0;
            else
                giris = 1;
            try
            {
                cmdTahsilBaslik = new SqlCommand("INSERT INTO " + firmaPrefix + donemPrefix + "TBLCARCIKIADEBASLIK (FIRMANO,BELGENO,TARIH,IADE,TUTAR,PARABIRIMI,GIRIS,KUR,BELGETIPI,IPTAL,USERNO,OZELKOD1,OZELKOD2,ACIKLAMA,MUHASEBELESMEYECEK,OZELKOD3,OZELKOD4,CREDATE,KAYNAK,OZELKOD5,UID) OUTPUT INSERTED.IND VALUES" +
                    "(@p1,@p2,@p3,@p4,@p5,@p6,@p7,@p8,@p9,@p10,@p11,@p12,@p13,@p14,@p15,@p16,@p17,@p18,@p19,@p20,@p21)", connection)
                {
                    Transaction = sqlTransaction
                };
                cmdTahsilBaslik.Parameters.AddWithValue("@p1", globalBaslik.CariNo);
                cmdTahsilBaslik.Parameters.AddWithValue("@p2", BelgeNo);
                cmdTahsilBaslik.Parameters.AddWithValue("@p3", globalBaslik.Tarih.Date);
                cmdTahsilBaslik.Parameters.AddWithValue("@p4", 0);
                cmdTahsilBaslik.Parameters.AddWithValue("@p5", 0); //TUTAR update ile güncellenecek
                cmdTahsilBaslik.Parameters.AddWithValue("@p6", "TL");
                cmdTahsilBaslik.Parameters.AddWithValue("@p7", giris);
                cmdTahsilBaslik.Parameters.AddWithValue("@p8", 1);
                cmdTahsilBaslik.Parameters.AddWithValue("@p9", globalBaslik.Izahat);
                cmdTahsilBaslik.Parameters.AddWithValue("@p10", 0);
                cmdTahsilBaslik.Parameters.AddWithValue("@p11", userNo);
                cmdTahsilBaslik.Parameters.AddWithValue("@p12", globalBaslik.OzelKod1);
                cmdTahsilBaslik.Parameters.AddWithValue("@p13", globalBaslik.OzelKod2);
                cmdTahsilBaslik.Parameters.AddWithValue("@p14", globalBaslik.AltNot);
                cmdTahsilBaslik.Parameters.AddWithValue("@p15", 0);
                cmdTahsilBaslik.Parameters.AddWithValue("@p16", globalBaslik.OzelKod3);
                cmdTahsilBaslik.Parameters.AddWithValue("@p17", globalBaslik.OzelKod4);
                cmdTahsilBaslik.Parameters.AddWithValue("@p18", DateTime.Now);
                cmdTahsilBaslik.Parameters.AddWithValue("@p19", 0);
                cmdTahsilBaslik.Parameters.AddWithValue("@p20", globalBaslik.OzelKod5);
                cmdTahsilBaslik.Parameters.AddWithValue("@p21", "{" + Guid.NewGuid() + "}");

                globalBaslik.BaslikInd = (int)cmdTahsilBaslik.ExecuteScalar();

                TahsilCariHareket();

            }
            catch (Exception e)
            {
                throw e;
            }
        }
        private void TahsilatIadeHareket()
        {
            if (connection.State != ConnectionState.Open)
                connection.Open();
            try
            {
                int belgeLink = 0;
                int portNo = 0;

                var ress = TahsilVadeSuresiVeKartAdi().Split(';'); //c.Dispose sorun çıkarmasın
                var blokeGun = ress[0] == "" ? 0 : Convert.ToInt32(ress[0]);
                var kartAdi = ress[1];

                cmdTahsilHareket = new SqlCommand("INSERT INTO " + firmaPrefix + donemPrefix + "TBLCARCIKIADEHAREKET (IZAHAT,PORTNO,EVRAKNO,ACIKLAMA,VADE,TUTAR,PARABIRIMI,BELGENO,BELGELINK,STATUS,KUR,BANKANO,FIRMANO,MUHHESAPKODU,ACIKLAMA2) OUTPUT INSERTED.IND VALUES" +
                    "(@p1,@p2,@p3,@p4,@p5,@p6,@p7,@p8,@p9,@p10,@p11,@p12,@p13,@p14,@p15)", connection)
                {
                    Transaction = sqlTransaction
                };
                cmdTahsilHareket.Parameters.AddWithValue("@p1", globalHareket.Izahat);

                switch (globalHareket.Izahat)
                {
                    case 1:
                        portNo = -1;
                        belgeLink = -1;
                        cmdTahsilHareket.Parameters.AddWithValue("@p2", portNo);
                        cmdTahsilHareket.Parameters.AddWithValue("@p9", belgeLink);
                        cmdTahsilHareket.Parameters.AddWithValue("@p10", 0);
                        break;
                    case 2:
                        break;
                    case 3:
                        break;
                    case 4:
                        var res = TahsilVisaGirisVePortFoy(kartAdi).Split(';'); //önce giris tablosu sonra portföy tablosu girişteki
                        portNo = Convert.ToInt32(res[1]);
                        belgeLink = Convert.ToInt32(res[0]);
                        cmdTahsilHareket.Parameters.AddWithValue("@p2", portNo);
                        cmdTahsilHareket.Parameters.AddWithValue("@p9", belgeLink);
                        cmdTahsilHareket.Parameters.AddWithValue("@p10", 51);
                        break;
                    case 5:
                        break;
                    case 6:
                        break;
                    case 7:
                        break;
                    case 8:
                        break;
                    case 9:
                        break;
                    case 10:
                        portNo = -1;
                        belgeLink = -1;
                        cmdTahsilHareket.Parameters.AddWithValue("@p2", portNo);
                        cmdTahsilHareket.Parameters.AddWithValue("@p9", belgeLink);
                        cmdTahsilHareket.Parameters.AddWithValue("@p10", 0);
                        break;
                    case 11:
                        portNo = -1;
                        belgeLink = -1;
                        cmdTahsilHareket.Parameters.AddWithValue("@p2", portNo);
                        cmdTahsilHareket.Parameters.AddWithValue("@p9", belgeLink);
                        cmdTahsilHareket.Parameters.AddWithValue("@p10", 0);
                        break;
                    case 12:
                        break;
                }
                cmdTahsilHareket.Parameters.AddWithValue("@p3", globalBaslik.BaslikInd);
                cmdTahsilHareket.Parameters.AddWithValue("@p4", globalHareket.Aciklama);
                cmdTahsilHareket.Parameters.AddWithValue("@p5", globalHareket.Tarih.AddDays(globalHareket.Vade == null ? 0 : globalHareket.Vade + blokeGun).Date);//date eklencek
                cmdTahsilHareket.Parameters.AddWithValue("@p6", globalHareket.Fiyat);
                cmdTahsilHareket.Parameters.AddWithValue("@p7", "TL");
                cmdTahsilHareket.Parameters.AddWithValue("@p8", ""); //çek senet devreye girince belge nosu
                cmdTahsilHareket.Parameters.AddWithValue("@p11", 1);
                cmdTahsilHareket.Parameters.AddWithValue("@p12", globalHareket.BankaId);
                cmdTahsilHareket.Parameters.AddWithValue("@p13", globalBaslik.CariNo);
                cmdTahsilHareket.Parameters.AddWithValue("@p14", 0);
                cmdTahsilHareket.Parameters.AddWithValue("@p15", "");
                var hareketInd = (int)cmdTahsilHareket.ExecuteScalar();

                TahsilCariGenel(belgeLink);
                switch (globalHareket.Izahat)
                {
                    case 1:
                    case 2:
                    case 3:
                        TahsilNakitCekSenet(hareketInd);
                        break;
                }

            }
            catch (Exception e)
            {
                throw e;
            }
        }
        private void UpdateTahsilIadeBaslik()
        {
            if (connection.State != ConnectionState.Open)
                connection.Open();
            try
            {
                cmdUpdateTahsilBaslik = new SqlCommand("UPDATE " + firmaPrefix + donemPrefix + "TBLCARCIKIADEBASLIK SET TUTAR=(SELECT SUM(TUTAR) FROM " + firmaPrefix + donemPrefix + "TBLCARCIKIADEHAREKET WHERE EVRAKNO=@p1) WHERE IND=@p1", connection) { Transaction = sqlTransaction };
                cmdUpdateTahsilBaslik.Parameters.AddWithValue("@p1", globalBaslik.BaslikInd);
                cmdUpdateTahsilBaslik.ExecuteNonQuery();

                cmdUpdateTahsilCariHareket = new SqlCommand("UPDATE " + firmaPrefix + donemPrefix + "TBLCARIHAREKETLERI SET BORC=(SELECT SUM(TUTAR) FROM " + firmaPrefix + donemPrefix + "TBLCARCIKIADEHAREKET WHERE EVRAKNO=@p1) WHERE LN=@p1 AND IZAHAT=@p2 ", connection) { Transaction = sqlTransaction };
                cmdUpdateTahsilCariHareket.Parameters.AddWithValue("@p1", globalBaslik.BaslikInd);
                cmdUpdateTahsilCariHareket.Parameters.AddWithValue("@p2", globalBaslik.Izahat);
                cmdUpdateTahsilCariHareket.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                throw e;
            }
        }
        #endregion/*TAHSILAT IADE METOD*/


        #region/*TAHSILAT ORTAK METOD*/
        private void TahsilCariHareket()
        {
            if (connection.State != ConnectionState.Open)
                connection.Open();
            try
            {
                cmdTahsilCariHareket = new SqlCommand("INSERT INTO "+firmaPrefix+donemPrefix + "TBLCARIHAREKETLERI (FIRMANO,TARIH,IZAHAT,EVRAKNO,BORC,ALACAK,LN,PARABIRIMI,KUR,ODEMETARIHI,ISLEMTARIHI,SIRALAMATARIHI,OZELKOD,SIRALAMATARIHIEX,OZELKOD1,OZELKOD2,OZELKOD3,OZELKOD5,OZELKOD6,OZELKOD7,OZELKOD8,OZELKOD9,LN2)VALUES" +
                    "(@p1,@p2,@p3,@p4,@p5,@p6,@p7,@p8,@p9,@p10,@p11,@p12,@p13,CONVERT(decimal(28, 8),@p14),@p15,@p16,@p17,@p18,@p19,@p20,@p21,@p22,@p23)", connection) { Transaction = sqlTransaction };
                cmdTahsilCariHareket.Parameters.AddWithValue("@p1", globalBaslik.CariNo);
                cmdTahsilCariHareket.Parameters.AddWithValue("@p2", globalBaslik.Tarih.Date);
                cmdTahsilCariHareket.Parameters.AddWithValue("@p3", globalBaslik.Izahat);
                cmdTahsilCariHareket.Parameters.AddWithValue("@p4", BelgeNo);
                cmdTahsilCariHareket.Parameters.AddWithValue("@p5", 0); //BORC VEYA ALACAK update ile belge toplamı yazılacak
                cmdTahsilCariHareket.Parameters.AddWithValue("@p6", 0); //BORC VEYA ALACAK update ile belge toplamı yazılacak
                cmdTahsilCariHareket.Parameters.AddWithValue("@p7", globalBaslik.BaslikInd);
                cmdTahsilCariHareket.Parameters.AddWithValue("@p8", "TL");
                cmdTahsilCariHareket.Parameters.AddWithValue("@p9", 1);
                cmdTahsilCariHareket.Parameters.AddWithValue("@p10",globalBaslik.Tarih.Date);
                cmdTahsilCariHareket.Parameters.AddWithValue("@p11",DateTime.Now);
                cmdTahsilCariHareket.Parameters.AddWithValue("@p12", DateTime.Now);//globalBaslik.Tarih);
                cmdTahsilCariHareket.Parameters.AddWithValue("@p13", !string.IsNullOrEmpty(globalBaslik.OzelKod4) ? (object)globalBaslik.OzelKod4 : DBNull.Value);
                cmdTahsilCariHareket.Parameters.AddWithValue("@p14",globalBaslik.Tarih);
                cmdTahsilCariHareket.Parameters.AddWithValue("@p15", !string.IsNullOrEmpty(globalBaslik.OzelKod1) ? (object)globalBaslik.OzelKod1 :"");
                cmdTahsilCariHareket.Parameters.AddWithValue("@p16", !string.IsNullOrEmpty(globalBaslik.OzelKod2) ? (object)globalBaslik.OzelKod2 : "");
                cmdTahsilCariHareket.Parameters.AddWithValue("@p17", !string.IsNullOrEmpty(globalBaslik.OzelKod3) ? (object)globalBaslik.OzelKod3 : "");
                cmdTahsilCariHareket.Parameters.AddWithValue("@p18", !string.IsNullOrEmpty(globalBaslik.OzelKod5) ? (object)globalBaslik.OzelKod5 : "");
                cmdTahsilCariHareket.Parameters.AddWithValue("@p19", !string.IsNullOrEmpty(globalBaslik.OzelKod6) ? (object)globalBaslik.OzelKod6 : "");
                cmdTahsilCariHareket.Parameters.AddWithValue("@p20", !string.IsNullOrEmpty(globalBaslik.OzelKod7) ? (object)globalBaslik.OzelKod7 : "");
                cmdTahsilCariHareket.Parameters.AddWithValue("@p21", !string.IsNullOrEmpty(globalBaslik.OzelKod8) ? (object)globalBaslik.OzelKod8 : "");
                cmdTahsilCariHareket.Parameters.AddWithValue("@p22", !string.IsNullOrEmpty(globalBaslik.OzelKod9) ? (object)globalBaslik.OzelKod9 : "");
                cmdTahsilCariHareket.Parameters.AddWithValue("@p23", DBNull.Value);
                cmdTahsilCariHareket.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                throw e;
            }
        }      
        private void TahsilCariGenel(int belgeLink) 
        {
            if (connection.State != ConnectionState.Open)
                connection.Open();
            try
            {
                cmdTahsilCariGenel = new SqlCommand("INSERT INTO "+firmaPrefix+donemPrefix+ "TBLCARIGENELHAREKET(FIRMANO,TARIH,VADE,BELGEIND,ISLEMIND,BELGEIZAHAT,ISLEMIZAHAT,BELGELINK,BORC,ALACAK,AYLIKVADE,BELGENO,ISLEMNO,CONVERTED,IPTAL,SIRALAMATARIHI,GECIKMEHESAPLA,PARABIRIMI,KUR,BASLIKPARABIRIMI,BASLIKKURU,ACIKLAMA,SIRALAMATARIHIEX,TAHSILLINK)VALUES" +
                    "(@p1,@p2,@p3,@p4,@p5,@p6,@p7,@p8,@p9,@p10,@p11,@p12,@p13,@p14,@p15,@p16,@p17,@p18,@p19,@p20,@p21,@p22,CONVERT(decimal(28, 8),@p23),@p24)", connection) { Transaction = sqlTransaction };
                cmdTahsilCariGenel.Parameters.AddWithValue("@p1", globalBaslik.CariNo);
                cmdTahsilCariGenel.Parameters.AddWithValue("@p2", globalBaslik.Tarih.Date);
                cmdTahsilCariGenel.Parameters.AddWithValue("@p3", globalBaslik.Tarih.Date);
                cmdTahsilCariGenel.Parameters.AddWithValue("@p4", globalBaslik.BaslikInd);
                cmdTahsilCariGenel.Parameters.AddWithValue("@p5", globalBaslik.BaslikInd);
                cmdTahsilCariGenel.Parameters.AddWithValue("@p6", globalBaslik.Izahat);
                cmdTahsilCariGenel.Parameters.AddWithValue("@p7", globalHareket.Izahat);

                    cmdTahsilCariGenel.Parameters.AddWithValue("@p8", belgeLink);
                
               
                switch (globalBaslik.Izahat)
                {
                    case 12:
                        cmdTahsilCariGenel.Parameters.AddWithValue("@p9", globalHareket.Fiyat);
                        cmdTahsilCariGenel.Parameters.AddWithValue("@p10", 0);
                        break;
                    case 13:
                        cmdTahsilCariGenel.Parameters.AddWithValue("@p9", 0);
                        cmdTahsilCariGenel.Parameters.AddWithValue("@p10", globalHareket.Fiyat);
                        break;
                }
                cmdTahsilCariGenel.Parameters.AddWithValue("@p11",0);
                cmdTahsilCariGenel.Parameters.AddWithValue("@p12", BelgeNo);
                cmdTahsilCariGenel.Parameters.AddWithValue("@p13",0);
                cmdTahsilCariGenel.Parameters.AddWithValue("@p14",0);
                cmdTahsilCariGenel.Parameters.AddWithValue("@p15",0);
                cmdTahsilCariGenel.Parameters.AddWithValue("@p16",globalBaslik.Tarih);
                cmdTahsilCariGenel.Parameters.AddWithValue("@p17",DBNull.Value);//1 di emre gök null çektirdi.
                cmdTahsilCariGenel.Parameters.AddWithValue("@p18","TL");
                cmdTahsilCariGenel.Parameters.AddWithValue("@p19",1);
                cmdTahsilCariGenel.Parameters.AddWithValue("@p20","TL");
                cmdTahsilCariGenel.Parameters.AddWithValue("@p21",1);
                cmdTahsilCariGenel.Parameters.AddWithValue("@p22",globalHareket.Aciklama);
                cmdTahsilCariGenel.Parameters.AddWithValue("@p23",globalBaslik.Tarih);
                cmdTahsilCariGenel.Parameters.AddWithValue("@p24", DBNull.Value);

                cmdTahsilCariGenel.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                throw e;
            }
        }
        private string TahsilVisaGirisVePortFoy(string kartAdi) 
        {
            if (connection.State != ConnectionState.Open)
                connection.Open();
            try
            {
                cmdTahsilVisaGiris = new SqlCommand("INSERT INTO "+firmaPrefix+donemPrefix+ "TBLVISAGIRIS (LN,ISLEMTARIHI,BANKA,FIRMANO,EVRAKNO,TAKSITSAYISI,KARTADI) OUTPUT INSERTED.IND VALUES" +
                    "(@p1,@p2,@p3,@p4,@p5,@p6,@p7)", connection) { Transaction=sqlTransaction};
                //kartAdi boşsa yani anlaşma yoksa LN=0 yapılacak // PORTFOYNO=1 yapılacak (portfoy no gerekli olmayabilir)
                if (String.IsNullOrEmpty(kartAdi))
                    cmdTahsilVisaGiris.Parameters.AddWithValue("@p1", 0);
                else
                    cmdTahsilVisaGiris.Parameters.AddWithValue("@p1", 1);

                cmdTahsilVisaGiris.Parameters.AddWithValue("@p2", globalHareket.Tarih.Date);
                cmdTahsilVisaGiris.Parameters.AddWithValue("@p3", globalHareket.BankaId);
                cmdTahsilVisaGiris.Parameters.AddWithValue("@p4", globalBaslik.CariNo);
                cmdTahsilVisaGiris.Parameters.AddWithValue("@p5", globalBaslik.BaslikInd);
                cmdTahsilVisaGiris.Parameters.AddWithValue("@p6", 1);
                cmdTahsilVisaGiris.Parameters.AddWithValue("@p7", kartAdi);
                var girisInd =(int)cmdTahsilVisaGiris.ExecuteScalar();
                //todo tblvisaportföy tablosu insert

                cmdTahsilVisaPortfoy = new SqlCommand("INSERT INTO "+firmaPrefix+donemPrefix+ "TBLVISAPORTFOY(EVRAKNO)OUTPUT INSERTED.IND VALUES(@p1)", connection) { Transaction = sqlTransaction };
                cmdTahsilVisaPortfoy.Parameters.AddWithValue("@p1", girisInd);
                var portfoyInd=(int)cmdTahsilVisaPortfoy.ExecuteScalar();
                cmdUpdateTahsilVisaPortfoy = new SqlCommand("UPDATE " + firmaPrefix + donemPrefix + "TBLVISAPORTFOY SET PORTFOYNO=@p1 WHERE IND=@p1", connection) { Transaction = sqlTransaction };
                cmdUpdateTahsilVisaPortfoy.Parameters.AddWithValue("@p1", portfoyInd);
                cmdUpdateTahsilVisaPortfoy.ExecuteNonQuery();
                return girisInd+";"+portfoyInd;
            }
            catch (Exception e)
            {
                throw e;
            }
        }
        private string TahsilVadeSuresiVeKartAdi() 
        {
            object o = GetSqlValue("SELECT TOP(1) BLOKEGUN,KARTADI FROM " + firmaPrefix + "TBLBNKKARTANLASMAHAREKET WHERE BANKANO='"+ globalHareket.BankaId + "' AND BASLANGICTARIHI<='"+ globalBaslik.Tarih.ToString("yyyy-MM-dd")+ "' AND BITISTARIHI>='" + globalBaslik.Tarih.ToString("yyyy-MM-dd") + "'  ");
            if (o == null)
            {
                return ";";
            }
            return o.ToString();
        }
        private void TahsilNakitCekSenet(int hareketInd) 
        {
            if (connection.State != ConnectionState.Open)
                connection.Open();
            try
            {
                cmdTahsilNakitCekSenet = new SqlCommand("INSERT INTO "+firmaPrefix+donemPrefix+"TBLKASA(TARIH,ISLEM,GELIR,GIDER,PARABIRIMI,KUR,ACIKLAMA,BELGELINK,BELGEIZAHAT,LINELINK,USERNO,ISLEMTIPI,ISLEMTARIHI,SUBEADI,KASAADI,MUSTERITEM,BELGENEVI,ALTHESAP)VALUES" +
                    "(@p1,@p2,@p3,@p4,@p5,@p6,@p7,@p8,@p9,@p10,@p11,@p12,@p13,@p14,@p15,@p16,@p17,@p18)", connection) { Transaction = sqlTransaction };
                cmdTahsilNakitCekSenet.Parameters.AddWithValue("@p1", globalBaslik.Tarih.Date);
                switch (globalBaslik.Izahat)
                {
                    case 12:
                        cmdTahsilNakitCekSenet.Parameters.AddWithValue("@p2", -3);
                        break;
                    case 13:
                        cmdTahsilNakitCekSenet.Parameters.AddWithValue("@p2", -2);
                        break;
                }
                if (globalBaslik.BelgeAlSat == VegaBelgeHareketTipi.Cikis)
                {
                    cmdTahsilNakitCekSenet.Parameters.AddWithValue("@p3", 0);
                    cmdTahsilNakitCekSenet.Parameters.AddWithValue("@p4", globalHareket.Fiyat);
                }
                else 
                {
                    cmdTahsilNakitCekSenet.Parameters.AddWithValue("@p3", globalHareket.Fiyat);
                    cmdTahsilNakitCekSenet.Parameters.AddWithValue("@p4", 0);
                }
                
                cmdTahsilNakitCekSenet.Parameters.AddWithValue("@p5", globalHareket.ParaBirimi);
                cmdTahsilNakitCekSenet.Parameters.AddWithValue("@p6", globalHareket.Kur);
                cmdTahsilNakitCekSenet.Parameters.AddWithValue("@p7", globalHareket.Aciklama);
                cmdTahsilNakitCekSenet.Parameters.AddWithValue("@p8", globalBaslik.BaslikInd);
                cmdTahsilNakitCekSenet.Parameters.AddWithValue("@p9", globalBaslik.Izahat);
                cmdTahsilNakitCekSenet.Parameters.AddWithValue("@p10", hareketInd);
                cmdTahsilNakitCekSenet.Parameters.AddWithValue("@p11", userNo);
                cmdTahsilNakitCekSenet.Parameters.AddWithValue("@p12", globalHareket.Izahat);
                cmdTahsilNakitCekSenet.Parameters.AddWithValue("@p13", globalBaslik.Tarih);
                cmdTahsilNakitCekSenet.Parameters.AddWithValue("@p14",globalBaslik.OzelKod1);
                cmdTahsilNakitCekSenet.Parameters.AddWithValue("@p15",globalBaslik.OzelKod2);
                cmdTahsilNakitCekSenet.Parameters.AddWithValue("@p16",globalBaslik.OzelKod3);
                cmdTahsilNakitCekSenet.Parameters.AddWithValue("@p17",globalBaslik.OzelKod5);
                cmdTahsilNakitCekSenet.Parameters.AddWithValue("@p18","");

                cmdTahsilNakitCekSenet.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                throw e;
            }
        }
        #endregion/*TAHSILAT ORTAK METOD*/

        #region/*ÜRETİM GİRİŞ ÇIKIŞ METOD*/
        private void UretimBaslik() 
        {
            if (connection.State != ConnectionState.Open)
                connection.Open();
            BelgeNo = BelgeNoGetir();
            globalBaslik.BelgeNo = BelgeNo;
            int giris;
            int iade;
            if (globalBaslik.BelgeAlSat == VegaBelgeHareketTipi.Cikis)
            {
                giris = 0;
                iade = 0;
            }
            else
            {
                giris = 1;
                iade = 0;
            }

            try
            {
                var headerParameter = "";
                if (headerValue != null)
                {
                    var headerArr = headerValue.Split('$');
                    for (int i = 0; i < headerArr.Length; i++)
                    {
                        headerParameter += ",@p" + (40 + i);
                        cmdSatFatIrsStokUretSayBaslik.Parameters.AddWithValue("@p" + (40 + i), headerArr[i]);
                    }
                }
                headerParameter = headerParameter.TrimStart(',');
                cmdSatFatIrsStokUretSayBaslik = new SqlCommand("INSERT INTO " + firmaPrefix + donemPrefix + "TBLSBASLIK " +

                       "(BELGENO,TARIH,KDV,ODEMETARIHI,ALT1,DEPO,OZELKOD1,OZELKOD2,CREDATE,LADATE,FIRMANO,GIRIS,BELGETIPI,USERNO,OZELKOD3,OZELKOD4,OZELKOD5,OZELKOD6,OZELKOD7,OZELKOD8,OZELKOD9,HAREKETDEPOSU,UID,PARABIRIMI,KUR,YUVARLAMA,ALLOWYUVARLAMA,IADE,IPTAL,AK,    ODMODIFIED,CONVERTED,ENTEGRE,SATISSEKLI,YURTDISI,MUHASEBELESMEYECEK,KAYNAK,STOKHAREKETEYAZ,CARIHAREKETEYAZ " + headerColumn + ") OUTPUT INSERTED.IND VALUES" +
                    "(@p1,@p2,@p3,@p4,@p5,@p6,@p7,@p8,@p9,@p10,@p11,@p12,@p13,@p14,@p15,@p16,@p17,@p18,@p19,@p20,@p21,@p22,@p23,@p24,@p25,@p26,@p27,@p28,@p29,@p30, @p31,@p32,@p33,@p34,@p35,@p36,@p37,@p38,@p39 " + headerParameter + ")", connection)
                {
                    Transaction = sqlTransaction
                };
                cmdSatFatIrsStokUretSayBaslik.Parameters.AddWithValue("@p1", BelgeNo);
                cmdSatFatIrsStokUretSayBaslik.Parameters.AddWithValue("@p2", globalBaslik.Tarih.Date);
                cmdSatFatIrsStokUretSayBaslik.Parameters.AddWithValue("@p3", globalBaslik.KdvDahil);
                cmdSatFatIrsStokUretSayBaslik.Parameters.AddWithValue("@p4", globalBaslik.Tarih.Date);
                cmdSatFatIrsStokUretSayBaslik.Parameters.AddWithValue("@p5", globalBaslik.Iskonto);
              
                    cmdSatFatIrsStokUretSayBaslik.Parameters.AddWithValue("@p6", (object)globalBaslik.DepoNo ?? DBNull.Value);
                
              
                cmdSatFatIrsStokUretSayBaslik.Parameters.AddWithValue("@p7", globalBaslik.OzelKod1 ?? "");
                cmdSatFatIrsStokUretSayBaslik.Parameters.AddWithValue("@p8", globalBaslik.OzelKod2 ?? "");
                cmdSatFatIrsStokUretSayBaslik.Parameters.AddWithValue("@p9", DateTime.Now);
                cmdSatFatIrsStokUretSayBaslik.Parameters.AddWithValue("@p10", DateTime.Now);
                cmdSatFatIrsStokUretSayBaslik.Parameters.AddWithValue("@p11", globalBaslik.CariNo);
                cmdSatFatIrsStokUretSayBaslik.Parameters.AddWithValue("@p12", giris);
                cmdSatFatIrsStokUretSayBaslik.Parameters.AddWithValue("@p13", globalBaslik.Izahat);
                cmdSatFatIrsStokUretSayBaslik.Parameters.AddWithValue("@p14", userNo);
                cmdSatFatIrsStokUretSayBaslik.Parameters.AddWithValue("@p15", globalBaslik.OzelKod3 ?? "");
                cmdSatFatIrsStokUretSayBaslik.Parameters.AddWithValue("@p16", globalBaslik.OzelKod4 ?? "");
                cmdSatFatIrsStokUretSayBaslik.Parameters.AddWithValue("@p17", globalBaslik.OzelKod5 ?? "");
                cmdSatFatIrsStokUretSayBaslik.Parameters.AddWithValue("@p18", globalBaslik.OzelKod6 ?? "");
                cmdSatFatIrsStokUretSayBaslik.Parameters.AddWithValue("@p19", globalBaslik.OzelKod7 ?? "");
                cmdSatFatIrsStokUretSayBaslik.Parameters.AddWithValue("@p20", globalBaslik.OzelKod8 ?? "");
                cmdSatFatIrsStokUretSayBaslik.Parameters.AddWithValue("@p21", globalBaslik.OzelKod9 ?? "");
                cmdSatFatIrsStokUretSayBaslik.Parameters.AddWithValue("@p22", globalBaslik.HareketDeposu);
                cmdSatFatIrsStokUretSayBaslik.Parameters.AddWithValue("@p23", "{" + Guid.NewGuid() + "}");
                cmdSatFatIrsStokUretSayBaslik.Parameters.AddWithValue("@p24", "TL");
                cmdSatFatIrsStokUretSayBaslik.Parameters.AddWithValue("@p25", 1);
                cmdSatFatIrsStokUretSayBaslik.Parameters.AddWithValue("@p26", 0);
                cmdSatFatIrsStokUretSayBaslik.Parameters.AddWithValue("@p27", 0);
                cmdSatFatIrsStokUretSayBaslik.Parameters.AddWithValue("@p28", iade);
                cmdSatFatIrsStokUretSayBaslik.Parameters.AddWithValue("@p29", 0);
                cmdSatFatIrsStokUretSayBaslik.Parameters.AddWithValue("@p30", 0);
                cmdSatFatIrsStokUretSayBaslik.Parameters.AddWithValue("@p31", 0);
                cmdSatFatIrsStokUretSayBaslik.Parameters.AddWithValue("@p32", 0);
                cmdSatFatIrsStokUretSayBaslik.Parameters.AddWithValue("@p33", 0);
                cmdSatFatIrsStokUretSayBaslik.Parameters.AddWithValue("@p34", 0);
                cmdSatFatIrsStokUretSayBaslik.Parameters.AddWithValue("@p35", 0);
                cmdSatFatIrsStokUretSayBaslik.Parameters.AddWithValue("@p36", 0);
                cmdSatFatIrsStokUretSayBaslik.Parameters.AddWithValue("@p37", 0);
                cmdSatFatIrsStokUretSayBaslik.Parameters.AddWithValue("@p38", 1);
                cmdSatFatIrsStokUretSayBaslik.Parameters.AddWithValue("@p39", 1);

                globalBaslik.BaslikInd = (int)cmdSatFatIrsStokUretSayBaslik.ExecuteScalar();

            }
            catch (Exception e)
            {
                throw e;
            }
        }
        private void UretimHareket()
        {
            if (connection.State != ConnectionState.Open)
                connection.Open();
            try
            {
                var SatirNo = HareketSatirSayisi();
                var stm = StokTipVeMaliyet(globalHareket.StokNo).Split(';');
                var StokTip = stm[0] == "" ? 0 : Convert.ToInt32(stm[0]);
                var Maliyet = stm[1] == "" ? 0 : Convert.ToDecimal(stm[1]);
                var res = StokBirimVeKdv(globalHareket.BirimNo).Split(';');
                var Birim = res[0].Length > 5 ? res[0].Substring(0, 5) : res[0];
                decimal KDV;
                if (globalHareket.Kdv == null || globalHareket.Kdv == 0)
                    KDV = Convert.ToDecimal(res[1]);
                else
                    KDV = globalHareket.Kdv;

                var itemParameter = "";
                if (itemValue != null)
                {
                    var itemArr = itemValue.Split('$');
                    for (int i = 0; i < itemArr.Length; i++)
                    {
                        itemParameter += ",@p" + (41 + i);
                        cmdSatFatIrsStokUretSayHareket.Parameters.AddWithValue("@p" + (41 + i), itemArr[i]);
                    }
                }
                cmdSatFatIrsStokUretSayHareket = new SqlCommand(@"INSERT INTO " + firmaPrefix + donemPrefix + "TBLSHAREKET " +
                    "(TARIH,EVRAKNO,FIRMANO,STOKNO,MALINCINSI,STOKKODU,STOKTIPI,MIKTAR,BIRIMMIKTAR,BIRIM,BIRIMEX,KDV,FIYATI,GERCEKTOPLAM,DEPO,ENVANTER,ACIKLAMA,GK,SATIRNO,SERIMIKTAR,MASRAF,PARABIRIMI,KUR, DETAY,ISK1,ISK2,ISK3,ISK4,ISK5,ISK6,PERSONEL,PIRIM,PROMOSYON,SATISKOSULU,GRUPMIKTAR,INDIRIM,OTV,OIV,OPSIYON,AFIYATI " + itemColumn + ") OUTPUT INSERTED.IND VALUES" +
                    "(@p1,@p2,@p3,@p4,@p5,@p6,@p7,@p8,@p9,@p10,@p11,@p12,@p13,@p14,@p15,@p16,@p17,@p18,@p19,@p20,@p21,@p22,@p23, @p24,@p25,@p26,@p27,@p28,@p29,@p30,@p31,@p32,@p33,@p34,@p35,@p36,@p37,@p38,@p39,@p40 " + itemParameter + ")", connection)
                {
                    Transaction = sqlTransaction
                };
                cmdSatFatIrsStokUretSayHareket.Parameters.AddWithValue("@p1", globalHareket.Tarih.Date);
                cmdSatFatIrsStokUretSayHareket.Parameters.AddWithValue("@p2", globalBaslik.BaslikInd);
                cmdSatFatIrsStokUretSayHareket.Parameters.AddWithValue("@p3", globalBaslik.CariNo);
                cmdSatFatIrsStokUretSayHareket.Parameters.AddWithValue("@p4", globalHareket.StokNo);
                cmdSatFatIrsStokUretSayHareket.Parameters.AddWithValue("@p5", globalHareket.MalinCinsi);
                cmdSatFatIrsStokUretSayHareket.Parameters.AddWithValue("@p6", globalHareket.StokKodu);
                cmdSatFatIrsStokUretSayHareket.Parameters.AddWithValue("@p7", StokTip);
                cmdSatFatIrsStokUretSayHareket.Parameters.AddWithValue("@p8", globalHareket.Miktar);
                cmdSatFatIrsStokUretSayHareket.Parameters.AddWithValue("@p9", 1);
                cmdSatFatIrsStokUretSayHareket.Parameters.AddWithValue("@p10", Birim);
                cmdSatFatIrsStokUretSayHareket.Parameters.AddWithValue("@p11", globalHareket.BirimNo);
                cmdSatFatIrsStokUretSayHareket.Parameters.AddWithValue("@p12", KDV);
                if (globalBaslik.Izahat == 96 || globalBaslik.Izahat == 97)
               {
                    cmdSatFatIrsStokUretSayHareket.Parameters.AddWithValue("@p13", globalHareket.Fiyat); // fiyat kdv hariç geliyor
                    cmdSatFatIrsStokUretSayHareket.Parameters.AddWithValue("@p14", globalHareket.Fiyat * globalHareket.Miktar); // KDV HARIC GERCEKTOPLAM
                }
                else
               
                if (globalBaslik.KdvDahil == 1) //eğer gelen fiyat kdv dahil ise kdv düşülecek yoksa gelen fiyat kdv haric
                {
                    var kdvHaricFiyat = globalHareket.Fiyat / (1 + (KDV / 100));
                    var kdvHaricGercekFiyat = (globalHareket.Fiyat * globalHareket.Miktar) / (1 + (KDV / 100));
                    cmdSatFatIrsStokUretSayHareket.Parameters.AddWithValue("@p13", kdvHaricFiyat); // KDV HARİC FIYATI
                    cmdSatFatIrsStokUretSayHareket.Parameters.AddWithValue("@p14", kdvHaricGercekFiyat); // KDV HARIC GERCEKTOPLAM
                }
                else
                {
                    cmdSatFatIrsStokUretSayHareket.Parameters.AddWithValue("@p13", globalHareket.Fiyat); // fiyat kdv hariç geliyor
                    cmdSatFatIrsStokUretSayHareket.Parameters.AddWithValue("@p14", globalHareket.Fiyat * globalHareket.Miktar); // KDV HARIC GERCEKTOPLAM
                }
                cmdSatFatIrsStokUretSayHareket.Parameters.AddWithValue("@p15", globalHareket.DepoNo);
                cmdSatFatIrsStokUretSayHareket.Parameters.AddWithValue("@p16", globalHareket.Miktar);
                cmdSatFatIrsStokUretSayHareket.Parameters.AddWithValue("@p17", globalHareket.Aciklama ?? "");
                cmdSatFatIrsStokUretSayHareket.Parameters.AddWithValue("@p18", 0);
                cmdSatFatIrsStokUretSayHareket.Parameters.AddWithValue("@p19", SatirNo);
                cmdSatFatIrsStokUretSayHareket.Parameters.AddWithValue("@p20", 1);
                cmdSatFatIrsStokUretSayHareket.Parameters.AddWithValue("@p21", 0);
                cmdSatFatIrsStokUretSayHareket.Parameters.AddWithValue("@p22", "TL");
                cmdSatFatIrsStokUretSayHareket.Parameters.AddWithValue("@p23", 1);
                cmdSatFatIrsStokUretSayHareket.Parameters.AddWithValue("@p24", 0);
                cmdSatFatIrsStokUretSayHareket.Parameters.AddWithValue("@p26", 0);
                cmdSatFatIrsStokUretSayHareket.Parameters.AddWithValue("@p25", 0);
                cmdSatFatIrsStokUretSayHareket.Parameters.AddWithValue("@p27", 0);
                cmdSatFatIrsStokUretSayHareket.Parameters.AddWithValue("@p28", 0);
                cmdSatFatIrsStokUretSayHareket.Parameters.AddWithValue("@p29", 0);
                cmdSatFatIrsStokUretSayHareket.Parameters.AddWithValue("@p30", 0);
                cmdSatFatIrsStokUretSayHareket.Parameters.AddWithValue("@p31", 0);
                cmdSatFatIrsStokUretSayHareket.Parameters.AddWithValue("@p32", 0);
                cmdSatFatIrsStokUretSayHareket.Parameters.AddWithValue("@p33", 0);
                cmdSatFatIrsStokUretSayHareket.Parameters.AddWithValue("@p34", 0);
                cmdSatFatIrsStokUretSayHareket.Parameters.AddWithValue("@p35", 1);
                cmdSatFatIrsStokUretSayHareket.Parameters.AddWithValue("@p36", 0);
                cmdSatFatIrsStokUretSayHareket.Parameters.AddWithValue("@p37", 0);
                cmdSatFatIrsStokUretSayHareket.Parameters.AddWithValue("@p38", 0);
                cmdSatFatIrsStokUretSayHareket.Parameters.AddWithValue("@p39", CariOpsiyonGetir(globalBaslik.CariNo));
                if (globalBaslik.Izahat == 96 || globalBaslik.Izahat == 97)
                {
                    cmdSatFatIrsStokUretSayHareket.Parameters.AddWithValue("@p40", globalHareket.Fiyat);
                }
                else
                    if (globalBaslik.KdvDahil == 1) //eğer gelen fiyat kdv dahil ise kdv düşülecek yoksa gelen fiyat kdv haric
                {
                    var kdvHaricFiyat = globalHareket.Fiyat / (1 + (KDV / 100));
                    cmdSatFatIrsStokUretSayHareket.Parameters.AddWithValue("@p40", kdvHaricFiyat); // KDV HARİC FIYATI                    
                }
                else
                {
                    cmdSatFatIrsStokUretSayHareket.Parameters.AddWithValue("@p40", globalHareket.Fiyat); // fiyat kdv hariç geliyor                    
                }
                //cmdSatFatIrsStokUretSayHareket.Parameters.AddWithValue("@p40", Maliyet);
                var hareketInd = (int)cmdSatFatIrsStokUretSayHareket.ExecuteScalar();
                InsertSatFatIrsStokUretSay2(hareketInd, KDV, StokTip,true);
            }
            catch (Exception e)
            {
                throw e;
            }
        }
        private void UpdateUretimBaslik()
        {
            if (connection.State != ConnectionState.Open)
                connection.Open();
            try
            {
                var res = BaslikTutarVeAraToplam().Split(';');
                var tutar = Convert.ToDecimal(res[0]);
                var aratoplam = Convert.ToDecimal(res[1]);
                cmdUpdateSatFatIrsStokUretSayBaslik = new SqlCommand("UPDATE " + firmaPrefix + donemPrefix + "TBLSBASLIK SET TUTAR=@p1, ARATOPLAM=@p2 WHERE IND=@p3", connection)
                {
                    Transaction = sqlTransaction
                };
                cmdUpdateSatFatIrsStokUretSayBaslik.Parameters.AddWithValue("@p1", tutar);
                cmdUpdateSatFatIrsStokUretSayBaslik.Parameters.AddWithValue("@p2", aratoplam);
                cmdUpdateSatFatIrsStokUretSayBaslik.Parameters.AddWithValue("@p3", globalBaslik.BaslikInd);
                cmdUpdateSatFatIrsStokUretSayBaslik.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                throw e;
            }
        }
        #endregion/*ÜRETİM GİRİŞ ÇIKIŞ METOD*/

        #region/*SAYIM GIRIS CIKIS METOD*/
        private void SayimCikisBaslik()
        {
            if (connection.State != ConnectionState.Open)
                connection.Open();
            BelgeNo = BelgeNoGetir();
            globalBaslik.BelgeNo = BelgeNo;
            int giris= ((int)globalBaslik.BelgeAlSat);
            int iade=globalBaslik.Iade;
            //if (globalBaslik.BelgeAlSat == VegaBelgeHareketTipi.Cikis)
            //{
            //    giris = 0;
            //    iade = 0;
            //}
            //else
            //{
            //    giris = 1;
            //    iade = 1;
            //}


            try
            {
                var headerParameter = "";
                if (headerValue != null)
                {
                    var headerArr = headerValue.Split('$');
                    for (int i = 0; i < headerArr.Length; i++)
                    {
                        headerParameter += ",@p" + (40 + i);
                        cmdSatFatIrsStokUretSayBaslik.Parameters.AddWithValue("@p" + (40 + i), headerArr[i]);
                    }
                }
                headerParameter = headerParameter.TrimStart(',');
                cmdSatFatIrsStokUretSayBaslik = new SqlCommand("INSERT INTO " + firmaPrefix + donemPrefix + "TBLSAYIMCIKISBASLIK " +

                    "(BELGENO,TARIH,KDV,ODEMETARIHI,ALT1,DEPO,OZELKOD1,OZELKOD2,CREDATE,LADATE,FIRMANO,GIRIS,BELGETIPI,USERNO,OZELKOD3,OZELKOD4,OZELKOD5,OZELKOD6,OZELKOD7,OZELKOD8,OZELKOD9,HAREKETDEPOSU,UID,PARABIRIMI,KUR,YUVARLAMA,ALLOWYUVARLAMA,IADE,IPTAL,AK,    ODMODIFIED,CONVERTED,ENTEGRE,SATISSEKLI,YURTDISI,MUHASEBELESMEYECEK,KAYNAK,STOKHAREKETEYAZ,CARIHAREKETEYAZ,  FIRMAADI,ENVANTERUPDATE,ALTBELGENO,ALT2,ALT3,ALT4,SUCCESS,OZELKOD,KALEM1,KALEM2,KALEM3,KALEM4,ODENEN,EKBELGETIPI,SELECTED,MASRAF1,MASRAF2,MASRAF3,MASRAF4,MASRAFKDV1,MASRAFKDV2,MASRAFKDV3,MASRAFKDV4,KDVISK,KONSOLIDE,ODEMEOPSIYONU,TEVKIFATORAN,STATUS,EIRSALIYE " + headerColumn + ") OUTPUT INSERTED.IND VALUES" +
                    "(@p1,@p2,@p3,@p4,@p5,@p6,@p7,@p8,@p9,@p10,@p11,@p12,@p13,@p14,@p15,@p16,@p17,@p18,@p19,@p20,@p21,@p22,@p23,@p24,@p25,@p26,@p27,@p28,@p29,@p30, @p31,@p32,@p33,@p34,@p35,@p36,@p37,@p38,@p39   ,@p40,@p41,@p42,@p43,@p44,@p45,@p46,@p47,@p48,@p49,@p50,@p51,@p52,@p53,@p54,@p55,@p56,@p57,@p58,@p59,@p60,@p61,@p62,@p63,@p64,@p65,@p66,@p67,@p68 " + headerParameter + ")", connection)
                {
                    Transaction = sqlTransaction
                };
                cmdSatFatIrsStokUretSayBaslik.Parameters.AddWithValue("@p1", BelgeNo);
                cmdSatFatIrsStokUretSayBaslik.Parameters.AddWithValue("@p2", globalBaslik.Tarih.Date);
                cmdSatFatIrsStokUretSayBaslik.Parameters.AddWithValue("@p3", globalBaslik.KdvDahil);
                cmdSatFatIrsStokUretSayBaslik.Parameters.AddWithValue("@p4", globalBaslik.Tarih.Date);
                cmdSatFatIrsStokUretSayBaslik.Parameters.AddWithValue("@p5", globalBaslik.Iskonto);
              
                    cmdSatFatIrsStokUretSayBaslik.Parameters.AddWithValue("@p6", (object)globalBaslik.DepoNo ?? DBNull.Value);
                
               
                cmdSatFatIrsStokUretSayBaslik.Parameters.AddWithValue("@p7", globalBaslik.OzelKod1 ?? "");
                cmdSatFatIrsStokUretSayBaslik.Parameters.AddWithValue("@p8", globalBaslik.OzelKod2 ?? "");
                cmdSatFatIrsStokUretSayBaslik.Parameters.AddWithValue("@p9", DateTime.Now);
                cmdSatFatIrsStokUretSayBaslik.Parameters.AddWithValue("@p10", DateTime.Now);
                cmdSatFatIrsStokUretSayBaslik.Parameters.AddWithValue("@p11", globalBaslik.CariNo);
                cmdSatFatIrsStokUretSayBaslik.Parameters.AddWithValue("@p12", giris);
                cmdSatFatIrsStokUretSayBaslik.Parameters.AddWithValue("@p13", globalBaslik.Izahat);
                cmdSatFatIrsStokUretSayBaslik.Parameters.AddWithValue("@p14", userNo);
                cmdSatFatIrsStokUretSayBaslik.Parameters.AddWithValue("@p15", globalBaslik.OzelKod3 ?? "");
                cmdSatFatIrsStokUretSayBaslik.Parameters.AddWithValue("@p16", globalBaslik.OzelKod4 ?? "");
                cmdSatFatIrsStokUretSayBaslik.Parameters.AddWithValue("@p17", globalBaslik.OzelKod5 ?? "");
                cmdSatFatIrsStokUretSayBaslik.Parameters.AddWithValue("@p18", globalBaslik.OzelKod6 ?? "");
                cmdSatFatIrsStokUretSayBaslik.Parameters.AddWithValue("@p19", globalBaslik.OzelKod7 ?? "");
                cmdSatFatIrsStokUretSayBaslik.Parameters.AddWithValue("@p20", globalBaslik.OzelKod8 ?? "");
                cmdSatFatIrsStokUretSayBaslik.Parameters.AddWithValue("@p21", globalBaslik.OzelKod9 ?? "");
                cmdSatFatIrsStokUretSayBaslik.Parameters.AddWithValue("@p22", globalBaslik.HareketDeposu);
                cmdSatFatIrsStokUretSayBaslik.Parameters.AddWithValue("@p23", "");//"{" + Guid.NewGuid() + "}"); UID
                cmdSatFatIrsStokUretSayBaslik.Parameters.AddWithValue("@p24", "TL");
                cmdSatFatIrsStokUretSayBaslik.Parameters.AddWithValue("@p25", 1);
                cmdSatFatIrsStokUretSayBaslik.Parameters.AddWithValue("@p26", 0);
                cmdSatFatIrsStokUretSayBaslik.Parameters.AddWithValue("@p27", 0);
                cmdSatFatIrsStokUretSayBaslik.Parameters.AddWithValue("@p28", iade);
                cmdSatFatIrsStokUretSayBaslik.Parameters.AddWithValue("@p29", 0);
                cmdSatFatIrsStokUretSayBaslik.Parameters.AddWithValue("@p30", 0);

                cmdSatFatIrsStokUretSayBaslik.Parameters.AddWithValue("@p31", 0);
                cmdSatFatIrsStokUretSayBaslik.Parameters.AddWithValue("@p32", 0);
                cmdSatFatIrsStokUretSayBaslik.Parameters.AddWithValue("@p33", 0);
                cmdSatFatIrsStokUretSayBaslik.Parameters.AddWithValue("@p34", 0);
                cmdSatFatIrsStokUretSayBaslik.Parameters.AddWithValue("@p35", 0);
                cmdSatFatIrsStokUretSayBaslik.Parameters.AddWithValue("@p36", 0);
                cmdSatFatIrsStokUretSayBaslik.Parameters.AddWithValue("@p37", 0);
                cmdSatFatIrsStokUretSayBaslik.Parameters.AddWithValue("@p38", 1);
                cmdSatFatIrsStokUretSayBaslik.Parameters.AddWithValue("@p39", 1);



                cmdSatFatIrsStokUretSayBaslik.Parameters.AddWithValue("@p40", "");
                cmdSatFatIrsStokUretSayBaslik.Parameters.AddWithValue("@p41", 0);
                cmdSatFatIrsStokUretSayBaslik.Parameters.AddWithValue("@p42", "");
                cmdSatFatIrsStokUretSayBaslik.Parameters.AddWithValue("@p43", 0);
                cmdSatFatIrsStokUretSayBaslik.Parameters.AddWithValue("@p44", 0);
                cmdSatFatIrsStokUretSayBaslik.Parameters.AddWithValue("@p45", 0);
                cmdSatFatIrsStokUretSayBaslik.Parameters.AddWithValue("@p46", 0);
                cmdSatFatIrsStokUretSayBaslik.Parameters.AddWithValue("@p47", 0);
                cmdSatFatIrsStokUretSayBaslik.Parameters.AddWithValue("@p48", 0);
                cmdSatFatIrsStokUretSayBaslik.Parameters.AddWithValue("@p49", 0);
                cmdSatFatIrsStokUretSayBaslik.Parameters.AddWithValue("@p50", 0);
                cmdSatFatIrsStokUretSayBaslik.Parameters.AddWithValue("@p51", 0);
                cmdSatFatIrsStokUretSayBaslik.Parameters.AddWithValue("@p52", 0);
                cmdSatFatIrsStokUretSayBaslik.Parameters.AddWithValue("@p53", 0);
                cmdSatFatIrsStokUretSayBaslik.Parameters.AddWithValue("@p54", 0);
                cmdSatFatIrsStokUretSayBaslik.Parameters.AddWithValue("@p55", 0);
                cmdSatFatIrsStokUretSayBaslik.Parameters.AddWithValue("@p56", 0);
                cmdSatFatIrsStokUretSayBaslik.Parameters.AddWithValue("@p57", 0);
                cmdSatFatIrsStokUretSayBaslik.Parameters.AddWithValue("@p58", 0);
                cmdSatFatIrsStokUretSayBaslik.Parameters.AddWithValue("@p59", 0);
                cmdSatFatIrsStokUretSayBaslik.Parameters.AddWithValue("@p60", 0);
                cmdSatFatIrsStokUretSayBaslik.Parameters.AddWithValue("@p61", 0);
                cmdSatFatIrsStokUretSayBaslik.Parameters.AddWithValue("@p62", 0);
                cmdSatFatIrsStokUretSayBaslik.Parameters.AddWithValue("@p63", 0);
                cmdSatFatIrsStokUretSayBaslik.Parameters.AddWithValue("@p64", 0);
                cmdSatFatIrsStokUretSayBaslik.Parameters.AddWithValue("@p65", 0);
                cmdSatFatIrsStokUretSayBaslik.Parameters.AddWithValue("@p66", 0);
                cmdSatFatIrsStokUretSayBaslik.Parameters.AddWithValue("@p67", 0);
                cmdSatFatIrsStokUretSayBaslik.Parameters.AddWithValue("@p68", 0);

                globalBaslik.BaslikInd = (int)cmdSatFatIrsStokUretSayBaslik.ExecuteScalar();
                InsertSatFatIrsStokUretSay1();
            }
            catch (Exception e)
            {
                throw e;
            }
        }
        private void SayimCikisHareket()
        {
            if (connection.State != ConnectionState.Open)
                connection.Open();
            try
            {
                var SatirNo = HareketSatirSayisi();
                var stm = StokTipVeMaliyet(globalHareket.StokNo).Split(';');
                var StokTip = stm[0] == "" ? 0 : Convert.ToInt32(stm[0]);
                var Maliyet = stm[1] == "" ? 0 : Convert.ToDecimal(stm[1]);
                var res = StokBirimVeKdv(globalHareket.BirimNo).Split(';');
                var Birim = res[0].Length > 5 ? res[0].Substring(0, 5) : res[0];
                decimal KDV;
                if (globalHareket.Kdv == null || globalHareket.Kdv == 0)
                    KDV = Convert.ToDecimal(res[1]);
                else
                    KDV = globalHareket.Kdv;

                var itemParameter = "";
                if (itemValue != null)
                {
                    var itemArr = itemValue.Split('$');
                    for (int i = 0; i < itemArr.Length; i++)
                    {
                        itemParameter += ",@p" + (41 + i);
                        cmdSatFatIrsStokUretSayHareket.Parameters.AddWithValue("@p" + (41 + i), itemArr[i]);
                    }
                }
                cmdSatFatIrsStokUretSayHareket = new SqlCommand(@"INSERT INTO " + firmaPrefix + donemPrefix + "TBLSAYIMCIKISHAREKET " +
                    "(TARIH,EVRAKNO,FIRMANO,STOKNO,MALINCINSI,STOKKODU,STOKTIPI,MIKTAR,BIRIMMIKTAR,BIRIM,BIRIMEX,KDV,FIYATI,GERCEKTOPLAM,DEPO,ENVANTER,ACIKLAMA,GK,SATIRNO,SERIMIKTAR,MASRAF,PARABIRIMI,KUR, DETAY,ISK1,ISK2,ISK3,ISK4,ISK5,ISK6,PERSONEL,PIRIM,PROMOSYON,SATISKOSULU,GRUPMIKTAR,INDIRIM,OTV,OIV,OPSIYON,AFIYATI,     SELECTED,KDVTUTARI,KARSISTOKKODU,KARSIBARKOD,TAKSIT,BARKOD,PESINAT,MASRAFKDV,TEVKIFATUYG,TEVKIFATTUTAR,YS " + itemColumn + ") OUTPUT INSERTED.IND VALUES" +
                    "(@p1,@p2,@p3,@p4,@p5,@p6,@p7,@p8,@p9,@p10,@p11,@p12,@p13,@p14,@p15,@p16,@p17,@p18,@p19,@p20,@p21,@p22,@p23, @p24,@p25,@p26,@p27,@p28,@p29,@p30,@p31,@p32,@p33,@p34,@p35,@p36,@p37,@p38,@p39,@p40    ,@p41,@p42,@p43,@p44,@p45,@p46,@p47,@p48,@p49,@p50,@p51" + itemParameter + ")", connection)
                {
                    Transaction = sqlTransaction
                };
                cmdSatFatIrsStokUretSayHareket.Parameters.AddWithValue("@p1", globalHareket.Tarih.Date);
                cmdSatFatIrsStokUretSayHareket.Parameters.AddWithValue("@p2", globalBaslik.BaslikInd);
                cmdSatFatIrsStokUretSayHareket.Parameters.AddWithValue("@p3", globalBaslik.CariNo);
                cmdSatFatIrsStokUretSayHareket.Parameters.AddWithValue("@p4", globalHareket.StokNo);
                cmdSatFatIrsStokUretSayHareket.Parameters.AddWithValue("@p5", StokAdi(globalHareket.StokNo).ToString()); //stok adı ve stok kodu aynı
                cmdSatFatIrsStokUretSayHareket.Parameters.AddWithValue("@p6", globalHareket.StokKodu);//StokAdi(globalHareket.StokNo).ToString());
                cmdSatFatIrsStokUretSayHareket.Parameters.AddWithValue("@p7", StokTip);
                cmdSatFatIrsStokUretSayHareket.Parameters.AddWithValue("@p8", globalHareket.Miktar);
                cmdSatFatIrsStokUretSayHareket.Parameters.AddWithValue("@p9", 1);
                cmdSatFatIrsStokUretSayHareket.Parameters.AddWithValue("@p10", Birim);
                cmdSatFatIrsStokUretSayHareket.Parameters.AddWithValue("@p11", globalHareket.BirimNo);
                cmdSatFatIrsStokUretSayHareket.Parameters.AddWithValue("@p12", KDV);
                if (globalBaslik.KdvDahil == 1) //eğer gelen fiyat kdv dahil ise kdv düşülecek yoksa gelen fiyat kdv haric
                {
                    var kdvHaricFiyat = globalHareket.Fiyat / (1 + (KDV / 100));
                    var kdvHaricGercekFiyat = (globalHareket.Fiyat * globalHareket.Miktar) / (1 + (KDV / 100));
                    cmdSatFatIrsStokUretSayHareket.Parameters.AddWithValue("@p13", kdvHaricFiyat); // KDV HARİC FIYATI
                    cmdSatFatIrsStokUretSayHareket.Parameters.AddWithValue("@p14", kdvHaricGercekFiyat); // KDV HARIC GERCEKTOPLAM
                }
                else
                {
                    cmdSatFatIrsStokUretSayHareket.Parameters.AddWithValue("@p13", globalHareket.Fiyat); // fiyat kdv hariç geliyor
                    cmdSatFatIrsStokUretSayHareket.Parameters.AddWithValue("@p14", globalHareket.Fiyat * globalHareket.Miktar); // KDV HARIC GERCEKTOPLAM
                }
                cmdSatFatIrsStokUretSayHareket.Parameters.AddWithValue("@p15", globalHareket.DepoNo);
                cmdSatFatIrsStokUretSayHareket.Parameters.AddWithValue("@p16", globalHareket.Miktar);
                cmdSatFatIrsStokUretSayHareket.Parameters.AddWithValue("@p17", globalHareket.Aciklama ?? "");
                cmdSatFatIrsStokUretSayHareket.Parameters.AddWithValue("@p18", 0);
                cmdSatFatIrsStokUretSayHareket.Parameters.AddWithValue("@p19", SatirNo);
                cmdSatFatIrsStokUretSayHareket.Parameters.AddWithValue("@p20", 1);
                cmdSatFatIrsStokUretSayHareket.Parameters.AddWithValue("@p21", 0);
                cmdSatFatIrsStokUretSayHareket.Parameters.AddWithValue("@p22", "TL");
                cmdSatFatIrsStokUretSayHareket.Parameters.AddWithValue("@p23", 1);
                cmdSatFatIrsStokUretSayHareket.Parameters.AddWithValue("@p24", 0);
                cmdSatFatIrsStokUretSayHareket.Parameters.AddWithValue("@p26", 0);
                cmdSatFatIrsStokUretSayHareket.Parameters.AddWithValue("@p25", 100);//ISK1
                cmdSatFatIrsStokUretSayHareket.Parameters.AddWithValue("@p27", 0);
                cmdSatFatIrsStokUretSayHareket.Parameters.AddWithValue("@p28", 0);
                cmdSatFatIrsStokUretSayHareket.Parameters.AddWithValue("@p29", 0);
                cmdSatFatIrsStokUretSayHareket.Parameters.AddWithValue("@p30", 0);
                cmdSatFatIrsStokUretSayHareket.Parameters.AddWithValue("@p31", 0);
                cmdSatFatIrsStokUretSayHareket.Parameters.AddWithValue("@p32", 0);
                cmdSatFatIrsStokUretSayHareket.Parameters.AddWithValue("@p33", 0);
                cmdSatFatIrsStokUretSayHareket.Parameters.AddWithValue("@p34", 1);
                cmdSatFatIrsStokUretSayHareket.Parameters.AddWithValue("@p35", 1);
                cmdSatFatIrsStokUretSayHareket.Parameters.AddWithValue("@p36", 0);
                cmdSatFatIrsStokUretSayHareket.Parameters.AddWithValue("@p37", 0);
                cmdSatFatIrsStokUretSayHareket.Parameters.AddWithValue("@p38", 0);
                cmdSatFatIrsStokUretSayHareket.Parameters.AddWithValue("@p39", CariOpsiyonGetir(globalBaslik.CariNo));
                cmdSatFatIrsStokUretSayHareket.Parameters.AddWithValue("@p40", Maliyet);

                cmdSatFatIrsStokUretSayHareket.Parameters.AddWithValue("@p41", 0);
                cmdSatFatIrsStokUretSayHareket.Parameters.AddWithValue("@p42", 0);
                cmdSatFatIrsStokUretSayHareket.Parameters.AddWithValue("@p43", "");
                cmdSatFatIrsStokUretSayHareket.Parameters.AddWithValue("@p44", "");
                cmdSatFatIrsStokUretSayHareket.Parameters.AddWithValue("@p45", 0);
                cmdSatFatIrsStokUretSayHareket.Parameters.AddWithValue("@p46", "");
                cmdSatFatIrsStokUretSayHareket.Parameters.AddWithValue("@p47", 0);
                cmdSatFatIrsStokUretSayHareket.Parameters.AddWithValue("@p48", 0);
                cmdSatFatIrsStokUretSayHareket.Parameters.AddWithValue("@p49", 0);
                cmdSatFatIrsStokUretSayHareket.Parameters.AddWithValue("@p50", 0);
                cmdSatFatIrsStokUretSayHareket.Parameters.AddWithValue("@p51", 1);
                
                var hareketInd = (int)cmdSatFatIrsStokUretSayHareket.ExecuteScalar();
                InsertSatFatIrsStokUretSay2(hareketInd, KDV, StokTip);
            }
            catch (Exception e)
            {
                throw e;
            }
        }
        private void UpdateSayimCikisBaslik()
        {
            if (connection.State != ConnectionState.Open)
                connection.Open();
            try
            {
                var res = BaslikTutarVeAraToplam().Split(';');
                var tutar = Convert.ToDecimal(res[0]);
                var aratoplam = Convert.ToDecimal(res[1]);
                cmdUpdateSatFatIrsStokUretSayBaslik = new SqlCommand("UPDATE " + firmaPrefix + donemPrefix + "TBLSAYIMCIKISBASLIK SET TUTAR=@p1, ARATOPLAM=0 WHERE IND=@p3", connection)
                {
                    Transaction = sqlTransaction
                };
                cmdUpdateSatFatIrsStokUretSayBaslik.Parameters.AddWithValue("@p1", tutar);
                cmdUpdateSatFatIrsStokUretSayBaslik.Parameters.AddWithValue("@p2", aratoplam);
                cmdUpdateSatFatIrsStokUretSayBaslik.Parameters.AddWithValue("@p3", globalBaslik.BaslikInd);
                cmdUpdateSatFatIrsStokUretSayBaslik.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        private void SayimGirisBaslik()
        {
            if (connection.State != ConnectionState.Open)
                connection.Open();
            BelgeNo = BelgeNoGetir();
            globalBaslik.BelgeNo = BelgeNo;
            int giris= ((int)globalBaslik.BelgeAlSat);
            int iade=globalBaslik.Iade;
            //if (globalBaslik.BelgeAlSat == VegaBelgeHareketTipi.Cikis)
            //{
            //    giris = 0;
            //    iade = 0;
            //}
            //else
            //{
            //    giris = 1;
            //    iade = 1;
            //}


            try
            {
                var headerParameter = "";
                if (headerValue != null)
                {
                    var headerArr = headerValue.Split('$');
                    for (int i = 0; i < headerArr.Length; i++)
                    {
                        headerParameter += ",@p" + (40 + i);
                        cmdSatFatIrsStokUretSayBaslik.Parameters.AddWithValue("@p" + (40 + i), headerArr[i]);
                    }
                }
                headerParameter = headerParameter.TrimStart(',');
                cmdSatFatIrsStokUretSayBaslik = new SqlCommand("INSERT INTO " + firmaPrefix + donemPrefix + "TBLSAYIMGIRISBASLIK " +

                    "(BELGENO,TARIH,KDV,ODEMETARIHI,ALT1,DEPO,OZELKOD1,OZELKOD2,CREDATE,LADATE,FIRMANO,GIRIS,BELGETIPI,USERNO,OZELKOD3,OZELKOD4,OZELKOD5,OZELKOD6,OZELKOD7,OZELKOD8,OZELKOD9,HAREKETDEPOSU,UID,PARABIRIMI,KUR,YUVARLAMA,ALLOWYUVARLAMA,IADE,IPTAL,AK,    ODMODIFIED,CONVERTED,ENTEGRE,SATISSEKLI,YURTDISI,MUHASEBELESMEYECEK,KAYNAK,STOKHAREKETEYAZ,CARIHAREKETEYAZ,    FIRMAADI,ENVANTERUPDATE,ALTBELGENO,ALT2,ALT3,ALT4,SUCCESS,OZELKOD,KALEM1,KALEM2,KALEM3,KALEM4,ODENEN,EKBELGETIPI,SELECTED,MASRAF1,MASRAF2,MASRAF3,MASRAF4,MASRAFKDV1,MASRAFKDV2,MASRAFKDV3,MASRAFKDV4,KDVISK,KONSOLIDE,ODEMEOPSIYONU,TEVKIFATORAN,STATUS,EIRSALIYE" + headerColumn + ") OUTPUT INSERTED.IND VALUES" +
                    "(@p1,@p2,@p3,@p4,@p5,@p6,@p7,@p8,@p9,@p10,@p11,@p12,@p13,@p14,@p15,@p16,@p17,@p18,@p19,@p20,@p21,@p22,@p23,@p24,@p25,@p26,@p27,@p28,@p29,@p30, @p31,@p32,@p33,@p34,@p35,@p36,@p37,@p38,@p39 ,@p40,@p41,@p42,@p43,@p44,@p45,@p46,@p47,@p48,@p49,@p50,@p51,@p52,@p53,@p54,@p55,@p56,@p57,@p58,@p59,@p60,@p61,@p62,@p63,@p64,@p65,@p66,@p67,@p68" + headerParameter + ")", connection)
                {
                    Transaction = sqlTransaction
                };
                cmdSatFatIrsStokUretSayBaslik.Parameters.AddWithValue("@p1", BelgeNo);
                cmdSatFatIrsStokUretSayBaslik.Parameters.AddWithValue("@p2", globalBaslik.Tarih.Date);
                cmdSatFatIrsStokUretSayBaslik.Parameters.AddWithValue("@p3", globalBaslik.KdvDahil);
                cmdSatFatIrsStokUretSayBaslik.Parameters.AddWithValue("@p4", globalBaslik.Tarih.Date);
                cmdSatFatIrsStokUretSayBaslik.Parameters.AddWithValue("@p5", globalBaslik.Iskonto);
               
                    cmdSatFatIrsStokUretSayBaslik.Parameters.AddWithValue("@p6", (object)globalBaslik.DepoNo ?? DBNull.Value);
                
                
                cmdSatFatIrsStokUretSayBaslik.Parameters.AddWithValue("@p7", globalBaslik.OzelKod1 ?? "");
                cmdSatFatIrsStokUretSayBaslik.Parameters.AddWithValue("@p8", globalBaslik.OzelKod2 ?? "");
                cmdSatFatIrsStokUretSayBaslik.Parameters.AddWithValue("@p9", DateTime.Now);
                cmdSatFatIrsStokUretSayBaslik.Parameters.AddWithValue("@p10", DateTime.Now);
                cmdSatFatIrsStokUretSayBaslik.Parameters.AddWithValue("@p11", globalBaslik.CariNo);
                cmdSatFatIrsStokUretSayBaslik.Parameters.AddWithValue("@p12", giris);
                cmdSatFatIrsStokUretSayBaslik.Parameters.AddWithValue("@p13", globalBaslik.Izahat);
                cmdSatFatIrsStokUretSayBaslik.Parameters.AddWithValue("@p14", userNo);
                cmdSatFatIrsStokUretSayBaslik.Parameters.AddWithValue("@p15", globalBaslik.OzelKod3 ?? "");
                cmdSatFatIrsStokUretSayBaslik.Parameters.AddWithValue("@p16", globalBaslik.OzelKod4 ?? "");
                cmdSatFatIrsStokUretSayBaslik.Parameters.AddWithValue("@p17", globalBaslik.OzelKod5 ?? "");
                cmdSatFatIrsStokUretSayBaslik.Parameters.AddWithValue("@p18", globalBaslik.OzelKod6 ?? "");
                cmdSatFatIrsStokUretSayBaslik.Parameters.AddWithValue("@p19", globalBaslik.OzelKod7 ?? "");
                cmdSatFatIrsStokUretSayBaslik.Parameters.AddWithValue("@p20", globalBaslik.OzelKod8 ?? "");
                cmdSatFatIrsStokUretSayBaslik.Parameters.AddWithValue("@p21", globalBaslik.OzelKod9 ?? "");
                cmdSatFatIrsStokUretSayBaslik.Parameters.AddWithValue("@p22", globalBaslik.HareketDeposu);
                cmdSatFatIrsStokUretSayBaslik.Parameters.AddWithValue("@p23", "");//"{" + Guid.NewGuid() + "}");
                cmdSatFatIrsStokUretSayBaslik.Parameters.AddWithValue("@p24", "TL");
                cmdSatFatIrsStokUretSayBaslik.Parameters.AddWithValue("@p25", 1);
                cmdSatFatIrsStokUretSayBaslik.Parameters.AddWithValue("@p26", 0);
                cmdSatFatIrsStokUretSayBaslik.Parameters.AddWithValue("@p27", 0);
                cmdSatFatIrsStokUretSayBaslik.Parameters.AddWithValue("@p28", iade);
                cmdSatFatIrsStokUretSayBaslik.Parameters.AddWithValue("@p29", 0);
                cmdSatFatIrsStokUretSayBaslik.Parameters.AddWithValue("@p30", 0);

                cmdSatFatIrsStokUretSayBaslik.Parameters.AddWithValue("@p31", 0);
                cmdSatFatIrsStokUretSayBaslik.Parameters.AddWithValue("@p32", 0);
                cmdSatFatIrsStokUretSayBaslik.Parameters.AddWithValue("@p33", 0);
                cmdSatFatIrsStokUretSayBaslik.Parameters.AddWithValue("@p34", 0);
                cmdSatFatIrsStokUretSayBaslik.Parameters.AddWithValue("@p35", 0);
                cmdSatFatIrsStokUretSayBaslik.Parameters.AddWithValue("@p36", 0);
                cmdSatFatIrsStokUretSayBaslik.Parameters.AddWithValue("@p37", 0);
                cmdSatFatIrsStokUretSayBaslik.Parameters.AddWithValue("@p38", 1);
                cmdSatFatIrsStokUretSayBaslik.Parameters.AddWithValue("@p39", 1);

                cmdSatFatIrsStokUretSayBaslik.Parameters.AddWithValue("@p40", "");
                cmdSatFatIrsStokUretSayBaslik.Parameters.AddWithValue("@p41", 0);
                cmdSatFatIrsStokUretSayBaslik.Parameters.AddWithValue("@p42", "");
                cmdSatFatIrsStokUretSayBaslik.Parameters.AddWithValue("@p43", 0);
                cmdSatFatIrsStokUretSayBaslik.Parameters.AddWithValue("@p44", 0);
                cmdSatFatIrsStokUretSayBaslik.Parameters.AddWithValue("@p45", 0);
                cmdSatFatIrsStokUretSayBaslik.Parameters.AddWithValue("@p46", 0);
                cmdSatFatIrsStokUretSayBaslik.Parameters.AddWithValue("@p47", 0);
                cmdSatFatIrsStokUretSayBaslik.Parameters.AddWithValue("@p48", 0);
                cmdSatFatIrsStokUretSayBaslik.Parameters.AddWithValue("@p49", 0);
                cmdSatFatIrsStokUretSayBaslik.Parameters.AddWithValue("@p50", 0);
                cmdSatFatIrsStokUretSayBaslik.Parameters.AddWithValue("@p51", 0);
                cmdSatFatIrsStokUretSayBaslik.Parameters.AddWithValue("@p52", 0);
                cmdSatFatIrsStokUretSayBaslik.Parameters.AddWithValue("@p53", 0);
                cmdSatFatIrsStokUretSayBaslik.Parameters.AddWithValue("@p54", 0);
                cmdSatFatIrsStokUretSayBaslik.Parameters.AddWithValue("@p55", 0);
                cmdSatFatIrsStokUretSayBaslik.Parameters.AddWithValue("@p56", 0);
                cmdSatFatIrsStokUretSayBaslik.Parameters.AddWithValue("@p57", 0);
                cmdSatFatIrsStokUretSayBaslik.Parameters.AddWithValue("@p58", 0);
                cmdSatFatIrsStokUretSayBaslik.Parameters.AddWithValue("@p59", 0);
                cmdSatFatIrsStokUretSayBaslik.Parameters.AddWithValue("@p60", 0);
                cmdSatFatIrsStokUretSayBaslik.Parameters.AddWithValue("@p61", 0);
                cmdSatFatIrsStokUretSayBaslik.Parameters.AddWithValue("@p62", 0);
                cmdSatFatIrsStokUretSayBaslik.Parameters.AddWithValue("@p63", 0);
                cmdSatFatIrsStokUretSayBaslik.Parameters.AddWithValue("@p64", 0);
                cmdSatFatIrsStokUretSayBaslik.Parameters.AddWithValue("@p65", 0);
                cmdSatFatIrsStokUretSayBaslik.Parameters.AddWithValue("@p66", 0);
                cmdSatFatIrsStokUretSayBaslik.Parameters.AddWithValue("@p67", 0);
                cmdSatFatIrsStokUretSayBaslik.Parameters.AddWithValue("@p68", 0);

                globalBaslik.BaslikInd = (int)cmdSatFatIrsStokUretSayBaslik.ExecuteScalar();
                InsertSatFatIrsStokUretSay1();
            }
            catch (Exception e)
            {
                throw e;
            }
        }
        private void SayimGirisHareket()
        {
            if (connection.State != ConnectionState.Open)
                connection.Open();
            try
            {
                var SatirNo = HareketSatirSayisi();
                var stm = StokTipVeMaliyet(globalHareket.StokNo).Split(';');
                var StokTip = stm[0] == "" ? 0 : Convert.ToInt32(stm[0]);
                var Maliyet = stm[1] == "" ? 0 : Convert.ToDecimal(stm[1]);
                var res = StokBirimVeKdv(globalHareket.BirimNo).Split(';');
                var Birim = res[0].Length > 5 ? res[0].Substring(0, 5) : res[0];
                decimal KDV;
                if (globalHareket.Kdv == null || globalHareket.Kdv == 0)
                    KDV = Convert.ToDecimal(res[1]);
                else
                    KDV = globalHareket.Kdv;

                var itemParameter = "";
                if (itemValue != null)
                {
                    var itemArr = itemValue.Split('$');
                    for (int i = 0; i < itemArr.Length; i++)
                    {
                        itemParameter += ",@p" + (41 + i);
                        cmdSatFatIrsStokUretSayHareket.Parameters.AddWithValue("@p" + (41 + i), itemArr[i]);
                    }
                }
                cmdSatFatIrsStokUretSayHareket = new SqlCommand(@"INSERT INTO " + firmaPrefix + donemPrefix + "TBLSAYIMGIRISHAREKET " +
                    "(TARIH,EVRAKNO,FIRMANO,STOKNO,MALINCINSI,STOKKODU,STOKTIPI,MIKTAR,BIRIMMIKTAR,BIRIM,BIRIMEX,KDV,FIYATI,GERCEKTOPLAM,DEPO,ENVANTER,ACIKLAMA,GK,SATIRNO,SERIMIKTAR,MASRAF,PARABIRIMI,KUR, DETAY,ISK1,ISK2,ISK3,ISK4,ISK5,ISK6,PERSONEL,PIRIM,PROMOSYON,SATISKOSULU,GRUPMIKTAR,INDIRIM,OTV,OIV,OPSIYON,AFIYATI,   SELECTED,KDVTUTARI,KARSISTOKKODU,KARSIBARKOD,TAKSIT,BARKOD,PESINAT,MASRAFKDV,TEVKIFATUYG,TEVKIFATTUTAR,YS " + itemColumn + ") OUTPUT INSERTED.IND VALUES" +
                    "(@p1,@p2,@p3,@p4,@p5,@p6,@p7,@p8,@p9,@p10,@p11,@p12,@p13,@p14,@p15,@p16,@p17,@p18,@p19,@p20,@p21,@p22,@p23, @p24,@p25,@p26,@p27,@p28,@p29,@p30,@p31,@p32,@p33,@p34,@p35,@p36,@p37,@p38,@p39,@p40 ,@p41,@p42,@p43,@p44,@p45,@p46,@p47,@p48,@p49,@p50,@p51  " + itemParameter + ")", connection)
                {
                    Transaction = sqlTransaction
                };
                cmdSatFatIrsStokUretSayHareket.Parameters.AddWithValue("@p1", globalHareket.Tarih.Date);
                cmdSatFatIrsStokUretSayHareket.Parameters.AddWithValue("@p2", globalBaslik.BaslikInd);
                cmdSatFatIrsStokUretSayHareket.Parameters.AddWithValue("@p3", globalBaslik.CariNo);
                cmdSatFatIrsStokUretSayHareket.Parameters.AddWithValue("@p4", globalHareket.StokNo);
                cmdSatFatIrsStokUretSayHareket.Parameters.AddWithValue("@p5", StokAdi(globalHareket.StokNo).ToString()); //stok adı ve stok kodu aynı
                cmdSatFatIrsStokUretSayHareket.Parameters.AddWithValue("@p6", globalHareket.StokKodu);//StokAdi(globalHareket.StokNo).ToString());
                cmdSatFatIrsStokUretSayHareket.Parameters.AddWithValue("@p7", StokTip);
                cmdSatFatIrsStokUretSayHareket.Parameters.AddWithValue("@p8", globalHareket.Miktar);
                cmdSatFatIrsStokUretSayHareket.Parameters.AddWithValue("@p9", 1);
                cmdSatFatIrsStokUretSayHareket.Parameters.AddWithValue("@p10", Birim);
                cmdSatFatIrsStokUretSayHareket.Parameters.AddWithValue("@p11", globalHareket.BirimNo);
                cmdSatFatIrsStokUretSayHareket.Parameters.AddWithValue("@p12", KDV);
                if (globalBaslik.KdvDahil == 1) //eğer gelen fiyat kdv dahil ise kdv düşülecek yoksa gelen fiyat kdv haric
                {
                    var kdvHaricFiyat = globalHareket.Fiyat / (1 + (KDV / 100));
                    var kdvHaricGercekFiyat = (globalHareket.Fiyat * globalHareket.Miktar) / (1 + (KDV / 100));
                    cmdSatFatIrsStokUretSayHareket.Parameters.AddWithValue("@p13", kdvHaricFiyat); // KDV HARİC FIYATI
                    cmdSatFatIrsStokUretSayHareket.Parameters.AddWithValue("@p14", kdvHaricGercekFiyat); // KDV HARIC GERCEKTOPLAM
                }
                else
                {
                    cmdSatFatIrsStokUretSayHareket.Parameters.AddWithValue("@p13", globalHareket.Fiyat); // fiyat kdv hariç geliyor
                    cmdSatFatIrsStokUretSayHareket.Parameters.AddWithValue("@p14", globalHareket.Fiyat * globalHareket.Miktar); // KDV HARIC GERCEKTOPLAM
                }
                cmdSatFatIrsStokUretSayHareket.Parameters.AddWithValue("@p15", globalHareket.DepoNo);
                cmdSatFatIrsStokUretSayHareket.Parameters.AddWithValue("@p16", globalHareket.Miktar);
                cmdSatFatIrsStokUretSayHareket.Parameters.AddWithValue("@p17", globalHareket.Aciklama ?? "");
                cmdSatFatIrsStokUretSayHareket.Parameters.AddWithValue("@p18", 0);
                cmdSatFatIrsStokUretSayHareket.Parameters.AddWithValue("@p19", SatirNo);
                cmdSatFatIrsStokUretSayHareket.Parameters.AddWithValue("@p20", 1);
                cmdSatFatIrsStokUretSayHareket.Parameters.AddWithValue("@p21", 0);
                cmdSatFatIrsStokUretSayHareket.Parameters.AddWithValue("@p22", "TL");
                cmdSatFatIrsStokUretSayHareket.Parameters.AddWithValue("@p23", 1);
                cmdSatFatIrsStokUretSayHareket.Parameters.AddWithValue("@p24", 0);
                cmdSatFatIrsStokUretSayHareket.Parameters.AddWithValue("@p26", 0);
                cmdSatFatIrsStokUretSayHareket.Parameters.AddWithValue("@p25", 0);
                cmdSatFatIrsStokUretSayHareket.Parameters.AddWithValue("@p27", 0);
                cmdSatFatIrsStokUretSayHareket.Parameters.AddWithValue("@p28", 0);
                cmdSatFatIrsStokUretSayHareket.Parameters.AddWithValue("@p29", 0);
                cmdSatFatIrsStokUretSayHareket.Parameters.AddWithValue("@p30", 0);
                cmdSatFatIrsStokUretSayHareket.Parameters.AddWithValue("@p31", 0);
                cmdSatFatIrsStokUretSayHareket.Parameters.AddWithValue("@p32", 0);
                cmdSatFatIrsStokUretSayHareket.Parameters.AddWithValue("@p33", 0);
                cmdSatFatIrsStokUretSayHareket.Parameters.AddWithValue("@p34", 0);
                cmdSatFatIrsStokUretSayHareket.Parameters.AddWithValue("@p35", 1);
                cmdSatFatIrsStokUretSayHareket.Parameters.AddWithValue("@p36", 0);
                cmdSatFatIrsStokUretSayHareket.Parameters.AddWithValue("@p37", 0);
                cmdSatFatIrsStokUretSayHareket.Parameters.AddWithValue("@p38", 0);
                cmdSatFatIrsStokUretSayHareket.Parameters.AddWithValue("@p39", CariOpsiyonGetir(globalBaslik.CariNo));
                cmdSatFatIrsStokUretSayHareket.Parameters.AddWithValue("@p40", Maliyet);

                cmdSatFatIrsStokUretSayHareket.Parameters.AddWithValue("@p41", 0);
                cmdSatFatIrsStokUretSayHareket.Parameters.AddWithValue("@p42", 0);
                cmdSatFatIrsStokUretSayHareket.Parameters.AddWithValue("@p43", "");
                cmdSatFatIrsStokUretSayHareket.Parameters.AddWithValue("@p44", "");
                cmdSatFatIrsStokUretSayHareket.Parameters.AddWithValue("@p45", 0);
                cmdSatFatIrsStokUretSayHareket.Parameters.AddWithValue("@p46", "");
                cmdSatFatIrsStokUretSayHareket.Parameters.AddWithValue("@p47", 0);
                cmdSatFatIrsStokUretSayHareket.Parameters.AddWithValue("@p48", 0);
                cmdSatFatIrsStokUretSayHareket.Parameters.AddWithValue("@p49", 0);
                cmdSatFatIrsStokUretSayHareket.Parameters.AddWithValue("@p50", 0);
                cmdSatFatIrsStokUretSayHareket.Parameters.AddWithValue("@p51", 1);
              
                var hareketInd = (int)cmdSatFatIrsStokUretSayHareket.ExecuteScalar();
                InsertSatFatIrsStokUretSay2(hareketInd, KDV, StokTip);
            }
            catch (Exception e)
            {
                throw e;
            }
        }
        private void UpdateSayimGirisBaslik()
        {
            if (connection.State != ConnectionState.Open)
                connection.Open();
            try
            {
                var res = BaslikTutarVeAraToplam().Split(';');
                var tutar = Convert.ToDecimal(res[0]);
                var aratoplam = Convert.ToDecimal(res[1]);
                cmdUpdateSatFatIrsStokUretSayBaslik = new SqlCommand("UPDATE " + firmaPrefix + donemPrefix + "TBLSAYIMGIRISBASLIK SET TUTAR=@p1, ARATOPLAM=@p2 WHERE IND=@p3", connection)
                {
                    Transaction = sqlTransaction
                };
                cmdUpdateSatFatIrsStokUretSayBaslik.Parameters.AddWithValue("@p1", tutar);
                cmdUpdateSatFatIrsStokUretSayBaslik.Parameters.AddWithValue("@p2", aratoplam);
                cmdUpdateSatFatIrsStokUretSayBaslik.Parameters.AddWithValue("@p3", globalBaslik.BaslikInd);
                cmdUpdateSatFatIrsStokUretSayBaslik.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        #endregion/*SAYIM GIRIS CIKIS METOD*/


        #region/*DEPO TRANSFER - KABUL*/
        private void DepoTransferBaslik()
        {
            if (connection.State != ConnectionState.Open)
                connection.Open();
            BelgeNo = BelgeNoGetir();
            globalBaslik.BelgeNo = BelgeNo;
            int giris = ((int)globalBaslik.BelgeAlSat);
            int iade = globalBaslik.Iade;
            //if (globalBaslik.BelgeAlSat == VegaBelgeHareketTipi.Cikis)
            //{
            //    giris = 0;
            //    iade = 0;
            //}
            //else
            //{
            //    giris = 1;
            //    iade = 1;
            //}


            try
            {
                var headerParameter = "";
                if (headerValue != null)
                {
                    var headerArr = headerValue.Split('$');
                    for (int i = 0; i < headerArr.Length; i++)
                    {
                        headerParameter += ",@p" + (40 + i);
                        cmdDepoTransferBaslik.Parameters.AddWithValue("@p" + (40 + i), headerArr[i]);
                    }
                }
                headerParameter = headerParameter.TrimStart(',');
                cmdDepoTransferBaslik = new SqlCommand("INSERT INTO " + firmaPrefix + donemPrefix + "TBLDEPOHARBASLIK " +

                    "(BELGENO,TARIH,KDV,ODEMETARIHI,ALT1,DEPO,OZELKOD1,OZELKOD2,CREDATE,LADATE,GIRIS,BELGETIPI,USERNO,OZELKOD3,OZELKOD4,OZELKOD5,OZELKOD6,OZELKOD7,OZELKOD8,OZELKOD9,HAREKETDEPOSU,UID,PARABIRIMI,KUR,YUVARLAMA,ALLOWYUVARLAMA,IADE,IPTAL,AK,    ODMODIFIED,CONVERTED,ENTEGRE,SATISSEKLI,YURTDISI,MUHASEBELESMEYECEK,KAYNAK,STOKHAREKETEYAZ,CARIHAREKETEYAZ,ALTBELGENO,ALTBELGETARIHI,ALT2,ALT3,ALT4 ,ODENEN,EKBELGETIPI, MASRAF1,MASRAF2,MASRAF3,MASRAF4,MASRAFKDV1,MASRAFKDV2,MASRAFKDV3,MASRAFKDV4" + headerColumn + ") OUTPUT INSERTED.IND VALUES" +
                    "(@p1,@p2,@p3,@p4,@p5,@p6,@p7,@p8,@p9,@p10,@p12,@p13,@p14,@p15,@p16,@p17,@p18,@p19,@p20,@p21,@p22,@p23,@p24,@p25,@p26,@p27,@p28,@p29,@p30, @p31,@p32,@p33,@p34,@p35,@p36,@p37,@p38,@p39,@p40,@p41 ,@p42,@p43,@p44,@p45,@p46, @p47,@p48,@p49,@p50,@p51,@p52,@p53,@p54" + headerParameter + ")", connection)
                {
                    Transaction = sqlTransaction
                };
                cmdDepoTransferBaslik.Parameters.AddWithValue("@p1", BelgeNo);
                cmdDepoTransferBaslik.Parameters.AddWithValue("@p2", globalBaslik.Tarih.Date);
                cmdDepoTransferBaslik.Parameters.AddWithValue("@p3", globalBaslik.KdvDahil);
                cmdDepoTransferBaslik.Parameters.AddWithValue("@p4", globalBaslik.Tarih.Date);
                cmdDepoTransferBaslik.Parameters.AddWithValue("@p5", globalBaslik.Iskonto);

                cmdDepoTransferBaslik.Parameters.AddWithValue("@p6", (object)globalBaslik.DepoNo ?? DBNull.Value);


                cmdDepoTransferBaslik.Parameters.AddWithValue("@p7", globalBaslik.OzelKod1 ?? "");
                cmdDepoTransferBaslik.Parameters.AddWithValue("@p8", globalBaslik.OzelKod2 ?? "");
                cmdDepoTransferBaslik.Parameters.AddWithValue("@p9", DateTime.Now);
                cmdDepoTransferBaslik.Parameters.AddWithValue("@p10", DateTime.Now);
                //cmdDepoTransferBaslik.Parameters.AddWithValue("@p11", null);//globalBaslik.CariNo);
                if (globalBaslik.Izahat == 129)
                {
                    cmdDepoTransferBaslik.Parameters.AddWithValue("@p12", 0);
                }
                else
                {
                    cmdDepoTransferBaslik.Parameters.AddWithValue("@p12", giris);
                }
                cmdDepoTransferBaslik.Parameters.AddWithValue("@p13", globalBaslik.Izahat);
                cmdDepoTransferBaslik.Parameters.AddWithValue("@p14", userNo);
                cmdDepoTransferBaslik.Parameters.AddWithValue("@p15", !string.IsNullOrEmpty(globalBaslik.OzelKod3) ? (object)globalBaslik.OzelKod3 : "");
                cmdDepoTransferBaslik.Parameters.AddWithValue("@p16", !string.IsNullOrEmpty(globalBaslik.OzelKod4) ? (object)globalBaslik.OzelKod4 : "");
                cmdDepoTransferBaslik.Parameters.AddWithValue("@p17", !string.IsNullOrEmpty(globalBaslik.OzelKod5) ? (object)globalBaslik.OzelKod5 : "");
                cmdDepoTransferBaslik.Parameters.AddWithValue("@p18", !string.IsNullOrEmpty(globalBaslik.OzelKod6) ? (object)globalBaslik.OzelKod6 : "");
                cmdDepoTransferBaslik.Parameters.AddWithValue("@p19", !string.IsNullOrEmpty(globalBaslik.OzelKod7) ? (object)globalBaslik.OzelKod7 : "");
                cmdDepoTransferBaslik.Parameters.AddWithValue("@p20", !string.IsNullOrEmpty(globalBaslik.OzelKod8) ? (object)globalBaslik.OzelKod8 : "");
                cmdDepoTransferBaslik.Parameters.AddWithValue("@p21", !string.IsNullOrEmpty(globalBaslik.OzelKod9) ? (object)globalBaslik.OzelKod9 : "");
                cmdDepoTransferBaslik.Parameters.AddWithValue("@p22", globalBaslik.HareketDeposu);
                cmdDepoTransferBaslik.Parameters.AddWithValue("@p23", "{" + Guid.NewGuid() + "}");
                cmdDepoTransferBaslik.Parameters.AddWithValue("@p24", "TL");
                cmdDepoTransferBaslik.Parameters.AddWithValue("@p25", 1);
                cmdDepoTransferBaslik.Parameters.AddWithValue("@p26", 0);
                cmdDepoTransferBaslik.Parameters.AddWithValue("@p27", 0);
                cmdDepoTransferBaslik.Parameters.AddWithValue("@p28", iade);
                cmdDepoTransferBaslik.Parameters.AddWithValue("@p29", 0);
                cmdDepoTransferBaslik.Parameters.AddWithValue("@p30", 0);

                cmdDepoTransferBaslik.Parameters.AddWithValue("@p31", 0);
                cmdDepoTransferBaslik.Parameters.AddWithValue("@p32", 0);
                cmdDepoTransferBaslik.Parameters.AddWithValue("@p33", 0);
                cmdDepoTransferBaslik.Parameters.AddWithValue("@p34", DBNull.Value);
                cmdDepoTransferBaslik.Parameters.AddWithValue("@p35", 0);
                cmdDepoTransferBaslik.Parameters.AddWithValue("@p36", 0);
                cmdDepoTransferBaslik.Parameters.AddWithValue("@p37", 0);
                cmdDepoTransferBaslik.Parameters.AddWithValue("@p38", 0);//1
                cmdDepoTransferBaslik.Parameters.AddWithValue("@p39", 0);//1
                cmdDepoTransferBaslik.Parameters.AddWithValue("@p40", (object)globalBaslik.AltBelgeNo??DBNull.Value);
                cmdDepoTransferBaslik.Parameters.AddWithValue("@p41", (object)globalBaslik.AltBelgeTarihi??DBNull.Value);
                cmdDepoTransferBaslik.Parameters.AddWithValue("@p42", 0);
                cmdDepoTransferBaslik.Parameters.AddWithValue("@p43", 0);
                cmdDepoTransferBaslik.Parameters.AddWithValue("@p44", 0);
                cmdDepoTransferBaslik.Parameters.AddWithValue("@p45", 0);
                cmdDepoTransferBaslik.Parameters.AddWithValue("@p46", 0);

                cmdDepoTransferBaslik.Parameters.AddWithValue("@p47", 0);
                cmdDepoTransferBaslik.Parameters.AddWithValue("@p48", 0);
                cmdDepoTransferBaslik.Parameters.AddWithValue("@p49", 0);
                cmdDepoTransferBaslik.Parameters.AddWithValue("@p50", 0);
                cmdDepoTransferBaslik.Parameters.AddWithValue("@p51", 0);
                cmdDepoTransferBaslik.Parameters.AddWithValue("@p52", 0);
                cmdDepoTransferBaslik.Parameters.AddWithValue("@p53", 0);
                cmdDepoTransferBaslik.Parameters.AddWithValue("@p54", 0);
                globalBaslik.BaslikInd = (int)cmdDepoTransferBaslik.ExecuteScalar();
            }
            catch (Exception e)
            {
                throw e;
            }
        }
        private void DepoTransferHareket()
        {
            if (connection.State != ConnectionState.Open)
                connection.Open();
            try
            {
                var SatirNo = HareketSatirSayisi();
                var stm = StokTipVeMaliyet(globalHareket.StokNo).Split(';');
                var StokTip = stm[0] == "" ? 0 : Convert.ToInt32(stm[0]);
                var Maliyet = stm[1] == "" ? 0 : Convert.ToDecimal(stm[1]);
                var res = StokBirimVeKdv(globalHareket.BirimNo).Split(';');
                var Birim = res[0].Length > 5 ? res[0].Substring(0, 5) : res[0];
                decimal KDV;
                if (globalHareket.Kdv == null || globalHareket.Kdv == 0)
                    KDV = Convert.ToDecimal(res[1]);
                else
                    KDV = globalHareket.Kdv;

                var itemParameter = "";
                if (itemValue != null)
                {
                    var itemArr = itemValue.Split('$');
                    for (int i = 0; i < itemArr.Length; i++)
                    {
                        itemParameter += ",@p" + (41 + i);
                        cmdDepoTransferHareket.Parameters.AddWithValue("@p" + (41 + i), itemArr[i]);
                    }
                }
                cmdDepoTransferHareket = new SqlCommand(@"INSERT INTO " + firmaPrefix + donemPrefix + "TBLDEPOHARHAREKET " +
                    "(TARIH,EVRAKNO,STOKNO,MALINCINSI,STOKKODU,STOKTIPI,MIKTAR,BIRIMMIKTAR,BIRIM,BIRIMEX,KDV,FIYATI,GERCEKTOPLAM,DEPO,ENVANTER,ACIKLAMA,GK,SATIRNO,SERIMIKTAR,MASRAF,PARABIRIMI,KUR, DETAY,ISK1,ISK2,ISK3,ISK4,ISK5,ISK6,PERSONEL,PIRIM,PROMOSYON,SATISKOSULU,GRUPMIKTAR,INDIRIM,OTV,OIV,OPSIYON,AFIYATI,BARKOD,YS " + itemColumn + ") OUTPUT INSERTED.IND VALUES" +
                    "(@p1,@p2,@p4,@p5,@p6,@p7,@p8,@p9,@p10,@p11,@p12,@p13,@p14,@p15,@p16,@p17,@p18,@p19,@p20,@p21,@p22,@p23, @p24,@p25,@p26,@p27,@p28,@p29,@p30,@p31,@p32,@p33,@p34,@p35,@p36,@p37,@p38,@p39,@p40,@p41 ,@p42" + itemParameter + ")", connection)
                {//depo 3 
                    Transaction = sqlTransaction
                };
                cmdDepoTransferHareket.Parameters.AddWithValue("@p1", globalHareket.Tarih.Date);
                cmdDepoTransferHareket.Parameters.AddWithValue("@p2", globalBaslik.BaslikInd);
                //cmdDepoTransferHareket.Parameters.AddWithValue("@p3", null);//globalBaslik.CariNo);
                cmdDepoTransferHareket.Parameters.AddWithValue("@p4", globalHareket.StokNo);
                cmdDepoTransferHareket.Parameters.AddWithValue("@p5", StokAdi(globalHareket.StokNo).ToString()); //stok adı ve stok kodu aynı
                cmdDepoTransferHareket.Parameters.AddWithValue("@p6", globalHareket.StokKodu);//StokAdi(globalHareket.StokNo).ToString());
                cmdDepoTransferHareket.Parameters.AddWithValue("@p7", StokTip);
                cmdDepoTransferHareket.Parameters.AddWithValue("@p8", globalHareket.Miktar);
                cmdDepoTransferHareket.Parameters.AddWithValue("@p9", 1);
                cmdDepoTransferHareket.Parameters.AddWithValue("@p10", Birim);
                cmdDepoTransferHareket.Parameters.AddWithValue("@p11", globalHareket.BirimNo);
                cmdDepoTransferHareket.Parameters.AddWithValue("@p12", KDV);
                if (globalBaslik.KdvDahil == 1) //eğer gelen fiyat kdv dahil ise kdv düşülecek yoksa gelen fiyat kdv haric
                {
                    var kdvHaricFiyat = globalHareket.Fiyat / (1 + (KDV / 100));
                    var kdvHaricGercekFiyat = (globalHareket.Fiyat * globalHareket.Miktar) / (1 + (KDV / 100));
                    cmdDepoTransferHareket.Parameters.AddWithValue("@p13", kdvHaricFiyat); // KDV HARİC FIYATI
                    cmdDepoTransferHareket.Parameters.AddWithValue("@p14", kdvHaricGercekFiyat); // KDV HARIC GERCEKTOPLAM
                }
                else
                {
                    cmdDepoTransferHareket.Parameters.AddWithValue("@p13", globalHareket.Fiyat); // fiyat kdv hariç geliyor
                    cmdDepoTransferHareket.Parameters.AddWithValue("@p14", globalHareket.Fiyat * globalHareket.Miktar); // KDV HARIC GERCEKTOPLAM
                }
                cmdDepoTransferHareket.Parameters.AddWithValue("@p15", 3);//globalHareket.DepoNo);
                cmdDepoTransferHareket.Parameters.AddWithValue("@p16", globalHareket.Miktar);
                cmdDepoTransferHareket.Parameters.AddWithValue("@p17", !string.IsNullOrEmpty(globalHareket.Aciklama) ? (object)globalHareket.Aciklama : DBNull.Value); //globalHareket.Aciklama ?? "");
                cmdDepoTransferHareket.Parameters.AddWithValue("@p18", 0);
                cmdDepoTransferHareket.Parameters.AddWithValue("@p19", SatirNo);
                cmdDepoTransferHareket.Parameters.AddWithValue("@p20", 1);
                cmdDepoTransferHareket.Parameters.AddWithValue("@p21", 0);
                cmdDepoTransferHareket.Parameters.AddWithValue("@p22", "TL");
                cmdDepoTransferHareket.Parameters.AddWithValue("@p23", 1);
                cmdDepoTransferHareket.Parameters.AddWithValue("@p24", 0);
                cmdDepoTransferHareket.Parameters.AddWithValue("@p26", 0);
                cmdDepoTransferHareket.Parameters.AddWithValue("@p25", 0);
                cmdDepoTransferHareket.Parameters.AddWithValue("@p27", 0);
                cmdDepoTransferHareket.Parameters.AddWithValue("@p28", 0);
                cmdDepoTransferHareket.Parameters.AddWithValue("@p29", 0);
                cmdDepoTransferHareket.Parameters.AddWithValue("@p30", 0);
                cmdDepoTransferHareket.Parameters.AddWithValue("@p31", 0);
                cmdDepoTransferHareket.Parameters.AddWithValue("@p32", 0);
                cmdDepoTransferHareket.Parameters.AddWithValue("@p33", 0);
                if (globalBaslik.Izahat == 129)
                {
                    cmdDepoTransferHareket.Parameters.AddWithValue("@p34", 1);
                }
                else
                {
                    cmdDepoTransferHareket.Parameters.AddWithValue("@p34", 0);
                }
               
                cmdDepoTransferHareket.Parameters.AddWithValue("@p35", 1);
                cmdDepoTransferHareket.Parameters.AddWithValue("@p36", 0);
                cmdDepoTransferHareket.Parameters.AddWithValue("@p37", 0);
                cmdDepoTransferHareket.Parameters.AddWithValue("@p38", 0);
                cmdDepoTransferHareket.Parameters.AddWithValue("@p39", CariOpsiyonGetir(globalBaslik.CariNo));
                cmdDepoTransferHareket.Parameters.AddWithValue("@p40", Maliyet);
                cmdDepoTransferHareket.Parameters.AddWithValue("@p41", "");
                cmdDepoTransferHareket.Parameters.AddWithValue("@p42", 1);
                var hareketInd = (int)cmdDepoTransferHareket.ExecuteScalar();
                InsertSatFatIrsStokUretSay2(hareketInd, KDV, StokTip);
            }
            catch (Exception e)
            {
                throw e;
            }
        }
        private void UpdateDepoTransferBaslik()
        {
            if (connection.State != ConnectionState.Open)
                connection.Open();
            try
            {
                var res = BaslikTutarVeAraToplam().Split(';');
                var tutar = Convert.ToDecimal(res[0]);
                var aratoplam = Convert.ToDecimal(res[1]);
                cmdUpdateDepoTransferBaslik = new SqlCommand("UPDATE " + firmaPrefix + donemPrefix + "TBLDEPOHARBASLIK SET TUTAR=@p1, ARATOPLAM=@p2 WHERE IND=@p3", connection)
                {
                    Transaction = sqlTransaction
                };
                cmdUpdateDepoTransferBaslik.Parameters.AddWithValue("@p1", tutar);
                cmdUpdateDepoTransferBaslik.Parameters.AddWithValue("@p2", aratoplam);
                cmdUpdateDepoTransferBaslik.Parameters.AddWithValue("@p3", globalBaslik.BaslikInd);
                cmdUpdateDepoTransferBaslik.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                throw e;
            }
        }


        private void DepoTransferKabulBaslik()
        {
            if (connection.State != ConnectionState.Open)
                connection.Open();
            BelgeNo = BelgeNoGetir();
            globalBaslik.BelgeNo = BelgeNo;
            int giris = ((int)globalBaslik.BelgeAlSat);
            int iade = globalBaslik.Iade;
            //if (globalBaslik.BelgeAlSat == VegaBelgeHareketTipi.Cikis)
            //{
            //    giris = 0;
            //    iade = 0;
            //}
            //else
            //{
            //    giris = 1;
            //    iade = 1;
            //}


            try
            {
                var headerParameter = "";
                if (headerValue != null)
                {
                    var headerArr = headerValue.Split('$');
                    for (int i = 0; i < headerArr.Length; i++)
                    {
                        headerParameter += ",@p" + (40 + i);
                        cmdDepoTransferKabulBaslik.Parameters.AddWithValue("@p" + (40 + i), headerArr[i]);
                    }
                }
                headerParameter = headerParameter.TrimStart(',');
                cmdDepoTransferKabulBaslik = new SqlCommand("INSERT INTO " + firmaPrefix + donemPrefix + "TBLDEPOHARBASLIK " +

                    "(BELGENO,TARIH,KDV,ODEMETARIHI,ALT1,DEPO,OZELKOD1,OZELKOD2,CREDATE,LADATE,FIRMANO,GIRIS,BELGETIPI,USERNO,OZELKOD3,OZELKOD4,OZELKOD5,OZELKOD6,OZELKOD7,OZELKOD8,OZELKOD9,HAREKETDEPOSU,UID,PARABIRIMI,KUR,YUVARLAMA,ALLOWYUVARLAMA,IADE,IPTAL,AK,    ODMODIFIED,CONVERTED,ENTEGRE,SATISSEKLI,YURTDISI,MUHASEBELESMEYECEK,KAYNAK,STOKHAREKETEYAZ,CARIHAREKETEYAZ,ALTBELGENO,ALTBELGETARIHI,ALT2,ALT3,ALT4,ODENEN,EKBELGETIPI, MASRAF1,MASRAF2,MASRAF3,MASRAF4,MASRAFKDV1,MASRAFKDV2,MASRAFKDV3,MASRAFKDV4" + headerColumn + ") OUTPUT INSERTED.IND VALUES" +
                    "(@p1,@p2,@p3,@p4,@p5,@p6,@p7,@p8,@p9,@p10,@p11,@p12,@p13,@p14,@p15,@p16,@p17,@p18,@p19,@p20,@p21,@p22,@p23,@p24,@p25,@p26,@p27,@p28,@p29,@p30, @p31,@p32,@p33,@p34,@p35,@p36,@p37,@p38,@p39,@p40,@p41,@p42,@p43,@p44,@p45,@p46 ,@p47,@p48,@p49,@p50,@p51,@p52,@p53,@p54" + headerParameter + ")", connection)
                {
                    Transaction = sqlTransaction
                };
                cmdDepoTransferKabulBaslik.Parameters.AddWithValue("@p1", BelgeNo);
                cmdDepoTransferKabulBaslik.Parameters.AddWithValue("@p2", globalBaslik.Tarih.Date);
                cmdDepoTransferKabulBaslik.Parameters.AddWithValue("@p3", globalBaslik.KdvDahil);
                cmdDepoTransferKabulBaslik.Parameters.AddWithValue("@p4", globalBaslik.Tarih.Date);
                cmdDepoTransferKabulBaslik.Parameters.AddWithValue("@p5", globalBaslik.Iskonto);

                cmdDepoTransferKabulBaslik.Parameters.AddWithValue("@p6", (object)globalBaslik.DepoNo ?? DBNull.Value);


                cmdDepoTransferKabulBaslik.Parameters.AddWithValue("@p7", globalBaslik.OzelKod1 ?? "");
                cmdDepoTransferKabulBaslik.Parameters.AddWithValue("@p8", globalBaslik.OzelKod2 ?? "");
                cmdDepoTransferKabulBaslik.Parameters.AddWithValue("@p9", DateTime.Now);
                cmdDepoTransferKabulBaslik.Parameters.AddWithValue("@p10", DateTime.Now);
                cmdDepoTransferKabulBaslik.Parameters.AddWithValue("@p11", globalBaslik.DepoTransferId);
                if (globalBaslik.Izahat == 129)
                {
                    cmdDepoTransferKabulBaslik.Parameters.AddWithValue("@p12", 0);
                }
                else
                {
                    cmdDepoTransferKabulBaslik.Parameters.AddWithValue("@p12", giris);
                }
               
                cmdDepoTransferKabulBaslik.Parameters.AddWithValue("@p13", globalBaslik.Izahat);
                cmdDepoTransferKabulBaslik.Parameters.AddWithValue("@p14", userNo);
                cmdDepoTransferKabulBaslik.Parameters.AddWithValue("@p15", !string.IsNullOrEmpty(globalBaslik.OzelKod3) ? (object)globalBaslik.OzelKod3 : "");
                cmdDepoTransferKabulBaslik.Parameters.AddWithValue("@p16", !string.IsNullOrEmpty(globalBaslik.OzelKod4) ? (object)globalBaslik.OzelKod4 : "");
                cmdDepoTransferKabulBaslik.Parameters.AddWithValue("@p17", !string.IsNullOrEmpty(globalBaslik.OzelKod5) ? (object)globalBaslik.OzelKod5 : "");
                cmdDepoTransferKabulBaslik.Parameters.AddWithValue("@p18", !string.IsNullOrEmpty(globalBaslik.OzelKod6) ? (object)globalBaslik.OzelKod6 : "");
                cmdDepoTransferKabulBaslik.Parameters.AddWithValue("@p19", !string.IsNullOrEmpty(globalBaslik.OzelKod7) ? (object)globalBaslik.OzelKod7 : "");
                cmdDepoTransferKabulBaslik.Parameters.AddWithValue("@p20", !string.IsNullOrEmpty(globalBaslik.OzelKod8) ? (object)globalBaslik.OzelKod8 : "");
                cmdDepoTransferKabulBaslik.Parameters.AddWithValue("@p21", !string.IsNullOrEmpty(globalBaslik.OzelKod9) ? (object)globalBaslik.OzelKod9 : "");
                cmdDepoTransferKabulBaslik.Parameters.AddWithValue("@p22", globalBaslik.HareketDeposu);
                cmdDepoTransferKabulBaslik.Parameters.AddWithValue("@p23", "{" + Guid.NewGuid() + "}");
                cmdDepoTransferKabulBaslik.Parameters.AddWithValue("@p24", "TL");
                cmdDepoTransferKabulBaslik.Parameters.AddWithValue("@p25", 1);
                cmdDepoTransferKabulBaslik.Parameters.AddWithValue("@p26", 0);
                cmdDepoTransferKabulBaslik.Parameters.AddWithValue("@p27", 0);
                cmdDepoTransferKabulBaslik.Parameters.AddWithValue("@p28", iade);
                cmdDepoTransferKabulBaslik.Parameters.AddWithValue("@p29", 0);
                cmdDepoTransferKabulBaslik.Parameters.AddWithValue("@p30", 0);

                cmdDepoTransferKabulBaslik.Parameters.AddWithValue("@p31", 0);
                cmdDepoTransferKabulBaslik.Parameters.AddWithValue("@p32", 0);
                cmdDepoTransferKabulBaslik.Parameters.AddWithValue("@p33", 0);
                cmdDepoTransferKabulBaslik.Parameters.AddWithValue("@p34", DBNull.Value);
                cmdDepoTransferKabulBaslik.Parameters.AddWithValue("@p35", 0);
                cmdDepoTransferKabulBaslik.Parameters.AddWithValue("@p36", 0);
                cmdDepoTransferKabulBaslik.Parameters.AddWithValue("@p37", 0);
                cmdDepoTransferKabulBaslik.Parameters.AddWithValue("@p38", 0);//1
                cmdDepoTransferKabulBaslik.Parameters.AddWithValue("@p39", 0);//1

                cmdDepoTransferKabulBaslik.Parameters.AddWithValue("@p40", (object)globalBaslik.AltBelgeNo??DBNull.Value);
                cmdDepoTransferKabulBaslik.Parameters.AddWithValue("@p41", (object)globalBaslik.AltBelgeTarihi??DBNull.Value);
                cmdDepoTransferKabulBaslik.Parameters.AddWithValue("@p42", 0);
                cmdDepoTransferKabulBaslik.Parameters.AddWithValue("@p43", 0);
                cmdDepoTransferKabulBaslik.Parameters.AddWithValue("@p44", 0);
                cmdDepoTransferKabulBaslik.Parameters.AddWithValue("@p45", 0);
                cmdDepoTransferKabulBaslik.Parameters.AddWithValue("@p46", 0);

                cmdDepoTransferKabulBaslik.Parameters.AddWithValue("@p47", 0);
                cmdDepoTransferKabulBaslik.Parameters.AddWithValue("@p48", 0);
                cmdDepoTransferKabulBaslik.Parameters.AddWithValue("@p49", 0);
                cmdDepoTransferKabulBaslik.Parameters.AddWithValue("@p50", 0);
                cmdDepoTransferKabulBaslik.Parameters.AddWithValue("@p51", 0);
                cmdDepoTransferKabulBaslik.Parameters.AddWithValue("@p52", 0);
                cmdDepoTransferKabulBaslik.Parameters.AddWithValue("@p53", 0);
                cmdDepoTransferKabulBaslik.Parameters.AddWithValue("@p54", 0);

                globalBaslik.BaslikInd = (int)cmdDepoTransferKabulBaslik.ExecuteScalar();
            }
            catch (Exception e)
            {
                throw e;
            }
        }
        private void DepoTransferKabulHareket()
        {
            if (connection.State != ConnectionState.Open)
                connection.Open();
            try
            {
                var SatirNo = HareketSatirSayisi();
                var stm = StokTipVeMaliyet(globalHareket.StokNo).Split(';');
                var StokTip = stm[0] == "" ? 0 : Convert.ToInt32(stm[0]);
                var Maliyet = stm[1] == "" ? 0 : Convert.ToDecimal(stm[1]);
                var res = StokBirimVeKdv(globalHareket.BirimNo).Split(';');
                var Birim = res[0].Length > 5 ? res[0].Substring(0, 5) : res[0];
                decimal KDV;
                if (globalHareket.Kdv == null || globalHareket.Kdv == 0)
                    KDV = Convert.ToDecimal(res[1]);
                else
                    KDV = globalHareket.Kdv;

                var itemParameter = "";
                if (itemValue != null)
                {
                    var itemArr = itemValue.Split('$');
                    for (int i = 0; i < itemArr.Length; i++)
                    {
                        itemParameter += ",@p" + (41 + i);
                        cmdDepoTransferKabulHareket.Parameters.AddWithValue("@p" + (41 + i), itemArr[i]);
                    }
                }
                cmdDepoTransferKabulHareket = new SqlCommand(@"INSERT INTO " + firmaPrefix + donemPrefix + "TBLDEPOHARHAREKET " +
                    "(TARIH,EVRAKNO,FIRMANO,STOKNO,MALINCINSI,STOKKODU,STOKTIPI,MIKTAR,BIRIMMIKTAR,BIRIM,BIRIMEX,KDV,FIYATI,GERCEKTOPLAM,DEPO,ENVANTER,ACIKLAMA,GK,SATIRNO,SERIMIKTAR,MASRAF,PARABIRIMI,KUR, DETAY,ISK1,ISK2,ISK3,ISK4,ISK5,ISK6,PERSONEL,PIRIM,PROMOSYON,SATISKOSULU,GRUPMIKTAR,INDIRIM,OTV,OIV,OPSIYON,AFIYATI,BARKOD,YS " + itemColumn + ") OUTPUT INSERTED.IND VALUES" +
                    "(@p1,@p2,@p3,@p4,@p5,@p6,@p7,@p8,@p9,@p10,@p11,@p12,@p13,@p14,@p15,@p16,@p17,@p18,@p19,@p20,@p21,@p22,@p23, @p24,@p25,@p26,@p27,@p28,@p29,@p30,@p31,@p32,@p33,@p34,@p35,@p36,@p37,@p38,@p39,@p40 ,@p41,@p42 " + itemParameter + ")", connection)
                {
                    Transaction = sqlTransaction
                };
                cmdDepoTransferKabulHareket.Parameters.AddWithValue("@p1", globalHareket.Tarih.Date);
                cmdDepoTransferKabulHareket.Parameters.AddWithValue("@p2", globalBaslik.BaslikInd);
                cmdDepoTransferKabulHareket.Parameters.AddWithValue("@p3", DBNull.Value);//globalBaslik.CariNo);
                cmdDepoTransferKabulHareket.Parameters.AddWithValue("@p4", globalHareket.StokNo);
                cmdDepoTransferKabulHareket.Parameters.AddWithValue("@p5", StokAdi(globalHareket.StokNo).ToString()); //stok adı ve stok kodu aynı
                cmdDepoTransferKabulHareket.Parameters.AddWithValue("@p6", globalHareket.StokKodu);//StokAdi(globalHareket.StokNo).ToString());
                cmdDepoTransferKabulHareket.Parameters.AddWithValue("@p7", StokTip);
                cmdDepoTransferKabulHareket.Parameters.AddWithValue("@p8", globalHareket.Miktar);
                cmdDepoTransferKabulHareket.Parameters.AddWithValue("@p9", 1);
                cmdDepoTransferKabulHareket.Parameters.AddWithValue("@p10", Birim);
                cmdDepoTransferKabulHareket.Parameters.AddWithValue("@p11", globalHareket.BirimNo);
                cmdDepoTransferKabulHareket.Parameters.AddWithValue("@p12", KDV);
                if (globalBaslik.KdvDahil == 1) //eğer gelen fiyat kdv dahil ise kdv düşülecek yoksa gelen fiyat kdv haric
                {
                    var kdvHaricFiyat = globalHareket.Fiyat / (1 + (KDV / 100));
                    var kdvHaricGercekFiyat = (globalHareket.Fiyat * globalHareket.Miktar) / (1 + (KDV / 100));
                    cmdDepoTransferKabulHareket.Parameters.AddWithValue("@p13", kdvHaricFiyat); // KDV HARİC FIYATI
                    cmdDepoTransferKabulHareket.Parameters.AddWithValue("@p14", kdvHaricGercekFiyat); // KDV HARIC GERCEKTOPLAM
                }
                else
                {
                    cmdDepoTransferKabulHareket.Parameters.AddWithValue("@p13", globalHareket.Fiyat); // fiyat kdv hariç geliyor
                    cmdDepoTransferKabulHareket.Parameters.AddWithValue("@p14", globalHareket.Fiyat * globalHareket.Miktar); // KDV HARIC GERCEKTOPLAM
                }
                if(globalBaslik.Izahat==128 || globalBaslik.Izahat == 129)
                {
                    cmdDepoTransferKabulHareket.Parameters.AddWithValue("@p15", 3);
                }
                else
                {
                    cmdDepoTransferKabulHareket.Parameters.AddWithValue("@p15", globalHareket.DepoNo);
                }
               
                cmdDepoTransferKabulHareket.Parameters.AddWithValue("@p16", globalHareket.Miktar);
                cmdDepoTransferKabulHareket.Parameters.AddWithValue("@p17", !string.IsNullOrEmpty(globalHareket.Aciklama) ? (object)globalHareket.Aciklama : DBNull.Value);
                cmdDepoTransferKabulHareket.Parameters.AddWithValue("@p18", 0);
                cmdDepoTransferKabulHareket.Parameters.AddWithValue("@p19", SatirNo);
                cmdDepoTransferKabulHareket.Parameters.AddWithValue("@p20", 1);
                cmdDepoTransferKabulHareket.Parameters.AddWithValue("@p21", 0);
                cmdDepoTransferKabulHareket.Parameters.AddWithValue("@p22", "TL");
                cmdDepoTransferKabulHareket.Parameters.AddWithValue("@p23", 1);
                cmdDepoTransferKabulHareket.Parameters.AddWithValue("@p24", 0);
                cmdDepoTransferKabulHareket.Parameters.AddWithValue("@p26", 0);
                cmdDepoTransferKabulHareket.Parameters.AddWithValue("@p25", 0);
                cmdDepoTransferKabulHareket.Parameters.AddWithValue("@p27", 0);
                cmdDepoTransferKabulHareket.Parameters.AddWithValue("@p28", 0);
                cmdDepoTransferKabulHareket.Parameters.AddWithValue("@p29", 0);
                cmdDepoTransferKabulHareket.Parameters.AddWithValue("@p30", 0);
                cmdDepoTransferKabulHareket.Parameters.AddWithValue("@p31", 0);
                cmdDepoTransferKabulHareket.Parameters.AddWithValue("@p32", 0);
                cmdDepoTransferKabulHareket.Parameters.AddWithValue("@p33", 0);
                if (globalBaslik.Izahat == 129)
                {
                    cmdDepoTransferKabulHareket.Parameters.AddWithValue("@p34", 1);
                }
                else
                {
                    cmdDepoTransferKabulHareket.Parameters.AddWithValue("@p34", 0);
                }
               
                cmdDepoTransferKabulHareket.Parameters.AddWithValue("@p35", 1);
                cmdDepoTransferKabulHareket.Parameters.AddWithValue("@p36", 0);
                cmdDepoTransferKabulHareket.Parameters.AddWithValue("@p37", 0);
                cmdDepoTransferKabulHareket.Parameters.AddWithValue("@p38", 0);
                cmdDepoTransferKabulHareket.Parameters.AddWithValue("@p39", CariOpsiyonGetir(globalBaslik.CariNo));
                cmdDepoTransferKabulHareket.Parameters.AddWithValue("@p40", Maliyet);
                cmdDepoTransferKabulHareket.Parameters.AddWithValue("@p41", "");
                cmdDepoTransferKabulHareket.Parameters.AddWithValue("@p42", 1);
                var hareketInd = (int)cmdDepoTransferKabulHareket.ExecuteScalar();
                InsertSatFatIrsStokUretSay2(hareketInd, KDV, StokTip);
            }
            catch (Exception e)
            {
                throw e;
            }
        }
        private void UpdateDepoTransferKabulBaslik()
        {
            if (connection.State != ConnectionState.Open)
                connection.Open();
            try
            {
                var res = BaslikTutarVeAraToplam().Split(';');
                var tutar = Convert.ToDecimal(res[0]);
                var aratoplam = Convert.ToDecimal(res[1]);
                cmdUpdateDepoTransferKabulBaslik = new SqlCommand("UPDATE " + firmaPrefix + donemPrefix + "TBLDEPOHARBASLIK SET TUTAR=@p1, ARATOPLAM=@p2 WHERE IND=@p3", connection)
                {
                    Transaction = sqlTransaction
                };
                cmdUpdateDepoTransferKabulBaslik.Parameters.AddWithValue("@p1", tutar);
                cmdUpdateDepoTransferKabulBaslik.Parameters.AddWithValue("@p2", aratoplam);
                cmdUpdateDepoTransferKabulBaslik.Parameters.AddWithValue("@p3", globalBaslik.BaslikInd);
                cmdUpdateDepoTransferKabulBaslik.ExecuteNonQuery();
                UpdateDepoTransferKabulBaslikForDepoTransfer(globalBaslik.DepoTransferId);
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        private void UpdateDepoTransferKabulBaslikForDepoTransfer(int BaslikId)
        {
            if (connection.State != ConnectionState.Open)
                connection.Open();
            try
            {
                cmdDepoTransferKabulBaslikForDepoTransfer = new SqlCommand("UPDATE " + firmaPrefix + donemPrefix + "TBLDEPOHARBASLIK SET AK=1 WHERE IND=" + BaslikId.ToString(), connection)
                {
                    Transaction = sqlTransaction
                };
                cmdDepoTransferKabulBaslikForDepoTransfer.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                throw e;
            }
        }
        #endregion/*DEPO TRANSFER - KABUL*/



        #region/*KASA GELIR GIDER*/
        public int KasaGelirGider(VegaBelgeKasa vegaBelgeKasa) 
        {
            if (connection.State != ConnectionState.Open)
                connection.Open();
            try
            {
                globalKasa = vegaBelgeKasa;
                cmdKasaIslemi = new SqlCommand("INSERT INTO " + firmaPrefix + donemPrefix + "TBLKASA (TARIH,ISLEM,GELIR,GIDER,PARABIRIMI,KUR,ACIKLAMA,USERNO,ISLEMTIPI,ISLEMTARIHI,SUBEADI,KASAADI,KDVDAHIL) OUTPUT INSERTED.IND VALUES(@p1,@p2,@p3,@p4,@p5,@p6,@p7,@p8,@p9,@p10,@p11,@p12,@p13) ", connection) { Transaction = sqlTransaction };
                cmdKasaIslemi.Parameters.AddWithValue("@p1", globalKasa.Tarih); //startDate.Date
                cmdKasaIslemi.Parameters.AddWithValue("@p2", globalKasa.Gelir > 0 ? GELIR : GIDER); //caseItem.Revenue > 0 ? GELIR : GIDER
                cmdKasaIslemi.Parameters.AddWithValue("@p3", globalKasa.Gelir); //caseItem.Revenue
                cmdKasaIslemi.Parameters.AddWithValue("@p4", globalKasa.Gider); //caseItem.Expense
                cmdKasaIslemi.Parameters.AddWithValue("@p5", globalKasa.ParaBirimi);
                cmdKasaIslemi.Parameters.AddWithValue("@p6", globalKasa.Kur);
                cmdKasaIslemi.Parameters.AddWithValue("@p7", globalKasa.Aciklama); //caseItem.Description
                cmdKasaIslemi.Parameters.AddWithValue("@p8", userNo);
                cmdKasaIslemi.Parameters.AddWithValue("@p9", globalKasa.IslemTipi);
                cmdKasaIslemi.Parameters.AddWithValue("@p10", globalKasa.IslemTarihi); //caseItem.Date
                cmdKasaIslemi.Parameters.AddWithValue("@p11", globalKasa.Sube); //branchItem.Branch.SUBEADI
                cmdKasaIslemi.Parameters.AddWithValue("@p12", globalKasa.Kasa); //branchItem.Case.KASAADI
                cmdKasaIslemi.Parameters.AddWithValue("@p13", globalKasa.KdvDahil);
                globalKasa.BelgeNo = (int)cmdKasaIslemi.ExecuteScalar();
                sqlTransaction.Commit();
                return globalKasa.BelgeNo;
            }
            catch (Exception e)
            {
                sqlTransaction.Rollback();
                throw e;
            }
            finally
            {
                connection.Close();
            }
        }
        private void DeleteKasa(int docNum,int izahat)
        {
            if (connection.State != ConnectionState.Open)
                connection.Open();
            try
            {
                cmdDeleteKasa = new SqlCommand("DELETE FROM " + firmaPrefix + donemPrefix + "TBLKASA WHERE IND=@p1", connection) { Transaction = sqlTransaction };
                cmdDeleteKasa.Parameters.AddWithValue("@p1", docNum);
                cmdDeleteKasa.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                throw e;
            }
        }
        #endregion/*KASA GELIR GIDER*/

        #region/*ORTAK METODLAR*/
        private object StokAdi(int stokNo) 
        {
            cmdStokGetir = new SqlCommand("select MALINCINSI from " + firmaPrefix + "TBLSTOKLAR WHERE IND=" + stokNo, connection)
            {
                Transaction = sqlTransaction
            };
            return cmdStokGetir.ExecuteScalar();
        }

        private string StokAdiStokKodu(int stokNo)
        {
            //cmdStokGetir = new SqlCommand("select MALINCINSI from " + firmaPrefix + "TBLSTOKLAR WHERE IND=" + stokNo, connection)
            //{
            //    Transaction = sqlTransaction
            //};
            //return cmdStokGetir.ExecuteScalar();

            object o = GetSqlValue("select MALINCINSI,STOKKODU from " + firmaPrefix + "TBLSTOKLAR WHERE IND = " + stokNo);
            if (o == null)
            {
                return ";";
            }
            return o.ToString();

        }

        private string StokTipVeMaliyet(int stokNo) 
        {
            //cmdStokTipGetir = new SqlCommand("select STOKTIPI from " + firmaPrefix + "TBLSTOKLAR WHERE IND=" + stokNo, connection)
            //{
            //    Transaction = sqlTransaction
            //};
            //var stoktip = cmdStokTipGetir.ExecuteScalar();
            //if (stoktip != null)
            //    return stoktip.ToString();
            //else
            //    return null;


            object o = GetSqlValue("select STOKTIPI,MALIYET from " + firmaPrefix + "TBLSTOKLAR WHERE IND=" + stokNo);
            if (o == null)
            {
                return ";";
            }
            return o.ToString();
        }
        private string StokBirimVeKdv(int birimNo)
        {
            object o = GetSqlValue("select BIRIMADI,KDV from " + firmaPrefix + "TBLBIRIMLEREX WHERE IND=" + birimNo);
            if (o == null)
            {
                return ";";
            }
            return o.ToString();
        }
        public object GetSqlValue(string sqlCommand)
        {
            if (connection.State != ConnectionState.Open)
                connection.Open();
            lock (connection)
            {
                c = new SqlCommand(sqlCommand, connection)
                {
                    Transaction = sqlTransaction
                };
                SqlDataReader reader = c.ExecuteReader();
                object retval = null;
                try
                {
                    if (!reader.Read())
                    {
                        return null;
                    }

                    if (!reader.HasRows)
                    {
                        return null;
                    }

                    retval = reader.GetValue(0) + ";" + reader.GetValue(1);
                }
                finally
                {
                    reader.Close();
                }
                return retval;
            }
        }
        private int HareketSatirSayisi()
        {
            if (connection.State != ConnectionState.Open)
                connection.Open();
            try
            {
                var tblName = "";
                switch (globalBaslik.Izahat)
                {
                    case SATFAT:
                        tblName = "TBLSATFATHAREKET";
                        break;
                    case SATFATIADE:
                        tblName = "TBLSATFATHAREKET";
                        break;
                    case SATIRS:
                        tblName = "TBLSATIRSHAREKET";
                        break;
                    case SATIRSIADE:
                        tblName = "TBLSATIRSHAREKET";
                        break;
                    case STOKCIKIS:
                        tblName = "TBLSTKCIKHAREKET";
                        break;
                    case STOKCIKISIADE:
                        tblName = "TBLSTKCIKHAREKET";
                        break;
                    case ALFAT:
                        tblName = "TBLALFATHAREKET";
                        break;
                    case GIDERFATURASI:
                        tblName = "TBLALFATHAREKET";
                        break;
                    case ALIRS:
                        tblName = "TBLALIRSHAREKET";
                        break;
                    case STOKGIRIS:
                        tblName = "TBLSTKGIRHAREKET";
                        break;
                    case ALFATIADE:
                        tblName = "TBLALFATHAREKET";
                        break;
                    case ALIRSIADE:
                        tblName = "TBLALIRSHAREKET";
                        break;
                    case STOKGIRISIADE:
                        tblName = "TBLSTKGIRHAREKET";
                        break;
                    case TAHSILAT:
                        tblName = "TBLCARGIRHAREKET";
                        break;
                    case TAHSILATIADE:
                        tblName = "TBLCARCIKIADEHAREKET";
                        break;
                    case SAYIMGIRIS:
                        tblName = "TBLSAYIMGIRISHAREKET";
                        break;
                    case SAYIMCIKIS:
                        tblName = "TBLSAYIMCIKISHAREKET";
                        break;
                    case URETIMGIRIS:
                    case URETIMCIKIS:
                        tblName = "TBLSHAREKET";
                        break;
                    case GELIR:
                        tblName = "";
                        break;
                    case GIDER:
                        tblName = "";
                        break;
                    case MALIYET:
                        break;
                    case TALEP:
                        tblName = "TBLALSIPHAREKET";
                        break;
                    case DEPOTRANSFER:
                        tblName = "TBLDEPOHARHAREKET";
                        break;
                    case DEPOTRANSFERKABUL:
                        tblName = "TBLDEPOHARHAREKET";
                        break;
                }
                cmdHareketSatirNo = new SqlCommand("SELECT COUNT(IND) FROM " + firmaPrefix + donemPrefix + tblName + " WHERE EVRAKNO=" + globalBaslik.BaslikInd, connection)
                {
                    Transaction = sqlTransaction
                };
                var satir = cmdHareketSatirNo.ExecuteScalar();
                if (satir != null)
                {
                    int sat = Convert.ToInt32(satir);
                    if (sat >= 0)
                        sat++;
                    return sat;
                } 
                else
                    return 1;
            }
            catch (Exception e)
            {
                throw e;
            }
        }
        private string BelgeNoGetir()
        {
            if (connection.State != ConnectionState.Open)
                connection.Open();
            try
            {
                var tblName = "";
                var regKey = "";
                switch (globalBaslik.Izahat)
                {
                    case SATFAT:
                        tblName = "TBLSATFATBASLIK";
                        regKey = REGSATFAT;
                        break;
                    case SATFATIADE:
                        tblName = "TBLSATFATBASLIK";
                        regKey = REGSATFAT;
                        break;
                    case SATIRS:
                        tblName = "TBLSATIRSBASLIK";
                        regKey = REGSATIRS;
                        break;
                    case SATIRSIADE:
                        tblName = "TBLSATIRSBASLIK";
                        regKey = REGSATIRS;
                        break;
                    case STOKCIKIS:
                        tblName = "TBLSTKCIKBASLIK";
                        regKey = REGSTOKCIKIS;
                        break;
                    case STOKCIKISIADE:
                        tblName = "TBLSTKCIKBASLIK";
                        regKey = REGSTOKCIKIS;
                        break;
                    case ALFAT:
                        tblName = "TBLALFATBASLIK";
                        regKey = REGALFAT;
                        break;
                    case GIDERFATURASI:
                        tblName = "TBLALFATBASLIK";
                        regKey = REGALFAT;
                        break;
                    case ALIRS:
                        tblName = "TBLALIRSBASLIK";
                        regKey = REGALIRS;
                        break;
                    case STOKGIRIS:
                        tblName = "TBLSTKGIRBASLIK";
                        regKey = REGSTOKGIRIS;
                        break;
                    case ALFATIADE:
                        tblName = "TBLALFATBASLIK";
                        regKey = REGALFAT;
                        break;
                    case ALIRSIADE:
                        tblName = "TBLALIRSBASLIK";
                        regKey = REGALIRS;
                        break;
                    case STOKGIRISIADE:
                        tblName = "TBLSTKGIRBASLIK";
                        regKey = REGSTOKGIRIS;
                        break;
                    case TAHSILAT:
                        tblName = "TBLCARGIRBASLIK";
                        regKey = REGTAHSIL;
                        break;
                    case TAHSILATIADE:
                        tblName = "TBLCARCIKIADEBASLIK";
                        regKey = REGTAHSILIADE;
                        break;
                    case SAYIMGIRIS:
                        tblName = "TBLSAYIMGIRISBASLIK";
                        regKey = REGSAYIMGIR;
                        break;
                    case SAYIMCIKIS:
                        tblName = "TBLSAYIMCIKISBASLIK";
                        regKey = REGSAYIMCIK;
                        break;
                    case URETIMGIRIS:
                    case URETIMCIKIS:
                        tblName = "TBLSBASLIK";
                        regKey = REGURETIM;
                        break;
                    case GELIR:
                        tblName = "";
                        break;
                    case GIDER:
                        tblName = "";
                        break;
                    case MALIYET:
                        break;
                    case TALEP:
                        tblName = "TBLALSIPBASLIK";
                        regKey = REGTALEP;
                        break;
                    case DEPOTRANSFER:
                        tblName = "TBLDEPOHARBASLIK";
                        break;
                    case DEPOTRANSFERKABUL:
                        tblName = "TBLDEPOHARBASLIK";
                        break;
                }
                cmdGetSeriNo = new SqlCommand("select top(1) BELGENO from " + firmaPrefix + donemPrefix + tblName+" order by IND DESC", connection)
                {
                    Transaction = sqlTransaction
                };
                var res = cmdGetSeriNo.ExecuteScalar();
                if (res != null)
                {
                    var datePrefix = "";
                    if (res.ToString().Length  > 8)
                    {
                        datePrefix = res.ToString().Substring(3, 4);
                    }
                    var character = Regex.Match(res.ToString(), @"^[a-zA-Z]").Value;
                    var num = Convert.ToInt32(Regex.Match(res.ToString().Length>8? res.ToString().Substring(8):res.ToString(), @"\d+").Value);
                    num++;
                    var numStr = num.ToString().PadLeft(res.ToString().Length > 8?9:7, '0');
                    var docNum = character +datePrefix+ numStr;
                    return docNum;
                }
                else
                {
                    RegistryKey key = Registry.CurrentUser.CreateSubKey(regKey);
                    object retval = key.GetValue("BelgeSerisi", "A");
                    key.Close();
                    return retval.ToString().PadRight(7, '0') + "1";
                }
            }
            catch (Exception e)
            {
                var docNum = "I0000001";
                return docNum;
                //throw e;
            }
        }
        private string BaslikTutarVeAraToplam()
        {
            if (connection.State != ConnectionState.Open)
                connection.Open();
            try
            {
                var tblName = "";
                switch (globalBaslik.Izahat)
                {
                    case SATFAT:
                        tblName = "TBLSATFATHAREKET";
                        break;
                    case SATFATIADE:
                        tblName = "TBLSATFATHAREKET";
                        break;
                    case SATIRS:
                        tblName = "TBLSATIRSHAREKET";
                        break;
                    case SATIRSIADE:
                        tblName = "TBLSATIRSHAREKET";
                        break;
                    case STOKCIKIS:
                        tblName = "TBLSTKCIKHAREKET";
                        break;
                    case STOKCIKISIADE:
                        tblName = "TBLSTKCIKHAREKET";
                        break;
                    case ALFAT:
                        tblName = "TBLALFATHAREKET";
                        break;
                    case GIDERFATURASI:
                        tblName = "TBLALFATHAREKET";
                        break;
                    case ALIRS:
                        tblName = "TBLALIRSHAREKET";
                        break;
                    case STOKGIRIS:
                        tblName = "TBLSTKGIRHAREKET";
                        break;
                    case ALFATIADE:
                        tblName = "TBLALFATHAREKET";
                        break;
                    case ALIRSIADE:
                        tblName = "TBLALIRSHAREKET";
                        break;
                    case STOKGIRISIADE:
                        tblName = "TBLSTKGIRHAREKET";
                        break;
                    case TAHSILAT:
                        tblName = "TBLCARGIRHAREKET";
                        break;
                    case TAHSILATIADE:
                        tblName = "TBLCARCIKIADEHAREKET";
                        break;
                    case SAYIMGIRIS:
                        tblName = "TBLSAYIMGIRISHAREKET";
                        break;
                    case SAYIMCIKIS:
                        tblName = "TBLSAYIMCIKISHAREKET";
                        break;
                    case URETIMGIRIS:
                    case URETIMCIKIS:
                        tblName = "TBLSHAREKET";
                        break;
                    case GELIR:
                        tblName = "";
                        break;
                    case GIDER:
                        tblName = "";
                        break;
                    case MALIYET:
                        break;
                    case TALEP:
                        tblName = "TBLALSIPHAREKET";
                        break;
                    case DEPOTRANSFER:
                        tblName = "TBLDEPOHARHAREKET";
                        break;
                    case DEPOTRANSFERKABUL:
                        tblName = "TBLDEPOHARHAREKET";
                        break;
                }

                object res;
                //if (globalBaslik.KdvDahil == 1) //fatura harekete işlenen fiyat KDV hariç fiyat
                //{
                //    if (globalBaslik.Iskonto > 0)
                //    {
                //        res = GetSqlValue("SELECT SUM( GERCEKTOPLAM * (1+CONVERT(decimal(28,8),KDV)/100) ),SUM( MIKTAR * ((FIYATI-" + globalBaslik.Iskonto.ToString().Replace(",", ".") + ")*(1+CONVERT(decimal(28,8),KDV)/100)) ) FROM " + firmaPrefix + donemPrefix + tblName + " WHERE EVRAKNO=" + globalBaslik.BaslikInd);
                //    }
                //    else
                //    {
                //        res = GetSqlValue("SELECT SUM( GERCEKTOPLAM * (1+CONVERT(decimal(28,8),KDV)/100) ),SUM( MIKTAR * (FIYATI*(1+CONVERT(decimal(28,8),KDV)/100)) ) FROM " + firmaPrefix + donemPrefix + tblName + " WHERE EVRAKNO=" + globalBaslik.BaslikInd);
                //    }

                //   // res = GetSqlValue("SELECT SUM( GERCEKTOPLAM *(1+CONVERT(decimal,8)/100) ),SUM( MIKTAR * (FIYATI*(1+CONVERT(decimal(28,8),KDV)/100)) ) FROM " + firmaPrefix + donemPrefix + tblName + " WHERE EVRAKNO=" + globalBaslik.BaslikInd);
                //}
                //else
                //{
                //    if (globalBaslik.Iskonto > 0)
                //    {
                //        res = GetSqlValue("SELECT SUM(GERCEKTOPLAM),SUM( MIKTAR * (FIYATI-"+ globalBaslik.Iskonto.ToString().Replace(",", ".") + ")) FROM " + firmaPrefix + donemPrefix + tblName + " WHERE EVRAKNO=" + globalBaslik.BaslikInd);
                //    }
                //    else
                //    {
                //        res = GetSqlValue("SELECT SUM(GERCEKTOPLAM),SUM( MIKTAR * FIYATI) FROM " + firmaPrefix + donemPrefix + tblName + " WHERE EVRAKNO=" + globalBaslik.BaslikInd);
                //    }

                //}
                res = GetSqlValue("SELECT SUM( GERCEKTOPLAM + (GERCEKTOPLAM*KDV/100)),SUM(GERCEKTOPLAM) FROM " + firmaPrefix + donemPrefix + tblName + " WHERE EVRAKNO=" + globalBaslik.BaslikInd);

                if (res == null)
                {
                    return ";";
                }
                return res.ToString();
            }
            catch (Exception e)
            {
                throw e;
            }
        }
        private int CariOpsiyonGetir(int cariId) 
        {
            if (connection.State != ConnectionState.Open)
                connection.Open();
            try
            {
                cmdCariOpsiyon = new SqlCommand("SELECT OPSIYON FROM " + firmaPrefix + "TBLCARI WHERE IND=@p1", connection) { Transaction = sqlTransaction };
                cmdCariOpsiyon.Parameters.AddWithValue("@p1", cariId);
                var res= cmdCariOpsiyon.ExecuteScalar();
                if (res != null)
                {
                    if (!String.IsNullOrEmpty(res.ToString()))
                    {
                        return (int)res;
                    }
                }
                    
                return 0;
            }
            catch (Exception e)
            {
                throw e;
            }
        }
        #endregion/*ORTAK METODLAR*/

        public VegaBelgeBaslik GetBelgeForDepoTransfer(string BelgeNo)
        {
            VegaBelgeBaslik baslik = new VegaBelgeBaslik();
            if (connection.State != ConnectionState.Open)
                connection.Open();
            try
            {
                cmdGetBelgeBaslik = new SqlCommand("select top(1) * from " + firmaPrefix + donemPrefix + "TBLDEPOHARBASLIK where IPTAL=0 and BELGENO='" + BelgeNo + "' and AK=0", connection) { Transaction = sqlTransaction };
                DataTable dt = new DataTable();
                dt.Load(cmdGetBelgeBaslik.ExecuteReader());
                foreach (DataRow r in dt.Rows)
                {
                    baslik.AltNot = r["ALTNOT"].ToString();
                    if(r["IND"] !=DBNull.Value)
                        baslik.BaslikInd = Convert.ToInt32( r["IND"].ToString());
                    baslik.BelgeNo = r["BELGENO"].ToString();
                    if (r["DEPO"] != DBNull.Value)
                        baslik.DepoNo =Convert.ToInt32(r["HAREKETDEPOSU"].ToString());
           
                    if (r["HAREKETDEPOSU"] != DBNull.Value)
                        baslik.HareketDeposu =  Convert.ToInt32(r["DEPO"].ToString());
   
                   
                    if (r["OZELKOD1"] != DBNull.Value)
                        baslik.OzelKod1 = r["OZELKOD1"].ToString();
                    if (r["OZELKOD2"] != DBNull.Value)
                        baslik.OzelKod2 = r["OZELKOD2"].ToString();
                    if (r["OZELKOD3"] != DBNull.Value)
                        baslik.OzelKod3 = r["OZELKOD3"].ToString();
                    if (r["OZELKOD4"] != DBNull.Value)
                        baslik.OzelKod4 = r["OZELKOD4"].ToString();
                    if (r["OZELKOD5"] != DBNull.Value)
                        baslik.OzelKod5 = r["OZELKOD5"].ToString();
                    if (r["OZELKOD6"] != DBNull.Value)
                        baslik.OzelKod6 = r["OZELKOD6"].ToString();
                    if (r["OZELKOD7"] != DBNull.Value)
                        baslik.OzelKod7 = r["OZELKOD7"].ToString();
                    if (r["OZELKOD8"] != DBNull.Value)
                        baslik.OzelKod8 = r["OZELKOD8"].ToString();
                    if (r["OZELKOD9"] != DBNull.Value)
                        baslik.OzelKod9 = r["OZELKOD9"].ToString();
                    if (r["TARIH"] != DBNull.Value)
                        baslik.Tarih = Convert.ToDateTime(r["TARIH"].ToString());
                }
            }
            catch(Exception ex)
            {

            }
            return baslik;
        }
        public List<VegaBelgeHareket> GetBelgeHareketsForDepoTransfer(string BelgeNo)
        {
            List<VegaBelgeHareket> BelgeHareket = new List<VegaBelgeHareket>();
            if (connection.State != ConnectionState.Open)
                connection.Open();
            try
            {
                cmdGetBelgeHareket = new SqlCommand("select * from " + firmaPrefix + donemPrefix + "TBLDEPOHARHAREKET where EVRAKNO='" + BelgeNo + "'", connection) { Transaction = sqlTransaction };
                DataTable dt = new DataTable();
                dt.Load(cmdGetBelgeHareket.ExecuteReader());
                foreach (DataRow r in dt.Rows)
                {
                    VegaBelgeHareket hareket = new VegaBelgeHareket();
                    if (r["ACIKLAMA"] != DBNull.Value)
                        hareket.Aciklama = r["ACIKLAMA"].ToString();
                    if (r["TARIH"] != DBNull.Value)
                        hareket.Tarih = Convert.ToDateTime(r["TARIH"].ToString());
                    if (r["STOKNO"] != DBNull.Value)
                        hareket.StokNo = Convert.ToInt32(r["STOKNO"].ToString());
                    if (r["MALINCINSI"] != DBNull.Value)
                        hareket.MalinCinsi = r["MALINCINSI"].ToString();
                    if (r["STOKKODU"] != DBNull.Value)
                        hareket.StokKodu = r["STOKKODU"].ToString();
                    if (r["MIKTAR"] != DBNull.Value)
                        hareket.Miktar = Convert.ToDecimal(r["MIKTAR"].ToString());
                    if (r["KDV"] != DBNull.Value)
                        hareket.Kdv = Convert.ToDecimal(r["KDV"].ToString());
                    if (r["DEPO"] != DBNull.Value)
                        hareket.DepoNo = Convert.ToInt32(r["DEPO"].ToString());
                    if (r["ENVANTER"] != DBNull.Value)
                        hareket.Envanter = Convert.ToDecimal(r["ENVANTER"].ToString());
                
                    BelgeHareket.Add(hareket);
                }
            }catch(Exception ex)
            {

            }
            return BelgeHareket;
        }
        protected virtual void Dispose(bool disposing) 
        {
            if (disposed)
                return;

            if (disposing) {
                connection.Dispose();
                sqlTransaction.Dispose();

                if (isDelete)
                {
                    if (izahat == 21 || izahat == 27 || izahat == 33 || izahat==96 || izahat==97) 
                    {
                        cmdDeleteFatIrsStokBaslik.Dispose();
                        cmdDeleteFatIrsStokHareket.Dispose();
                        if (izahat != 96 && izahat != 97) 
                        {
                            cmdDeleteCariGenel.Dispose();
                            cmdDeleteCariHareketleri.Dispose();
                        }
                        cmdDeleteDepoEnvanter.Dispose();
                        cmdDeleteStokHareket.Dispose();
                    }
                    if (izahat == 13 || izahat==12) 
                    {
                        cmdDeleteTahsilBaslik.Dispose();
                        cmdDeleteTahsilHareket.Dispose();
                        cmdDeleteTahsilCariGenel.Dispose();
                        cmdDeleteTahsilCariHareketleri.Dispose();
                        cmdDeleteTahsilDepoEnvanter.Dispose();
                        cmdDeleteTahsilStokHareket.Dispose();
                        cmdDeleteTahsilKasa.Dispose();
                        if (izahat == 13) 
                        {
                            cmdDeleteTahsilVisaGiris.Dispose();
                            cmdDeleteTahsilVisaPortfoy.Dispose();
                            getVisaGirisForDeletePortfoy.Dispose();
                        }
                        
                    }
                    if (izahat == 160) 
                    {
                        cmdDeleteKasa.Dispose();
                    }
                }
                else 
                {
                    if (globalBaslik != null)
                    {
                        if (globalBaslik.Izahat == 21 || globalBaslik.Izahat == 23 || 
                            globalBaslik.Izahat == 27 || globalBaslik.Izahat == 29 || 
                            globalBaslik.Izahat == 33 || globalBaslik.Izahat == 35 ||
                            globalBaslik.Izahat == 96 || globalBaslik.Izahat == 97 ||
                            globalBaslik.Izahat == 93 || globalBaslik.Izahat == 94)
                        {
                            cmdSatFatIrsStokUretSayBaslik.Dispose();
                            if(cmdSatFatIrsStokUretSayHareket!=null)
                                cmdSatFatIrsStokUretSayHareket.Dispose();
                            if(cmdUpdateSatFatIrsStokUretSayBaslik!=null)
                                cmdUpdateSatFatIrsStokUretSayBaslik.Dispose();
                            if (globalBaslik.Iskonto > 0)
                            {
                                cmdUpdateSatFatIrsStokUretSayHareket.Dispose();
                            }
                            if(cmdCariOpsiyon!=null)
                                cmdCariOpsiyon.Dispose();
                            if(cmdStokHareket!=null)
                                cmdStokHareket.Dispose();
                            if(cmdDepoEnvater!=null)
                                cmdDepoEnvater.Dispose();
                            if (globalBaslik.Izahat != 96 && globalBaslik.Izahat != 97) //üretim giriş çıkışta carigenelhareket ve carihareketler tablolarına kayıt atılmıyor
                            {
                                if(cmdCariGenel!=null)
                                    cmdCariGenel.Dispose();
                                if(cmdCariHareketleri!=null)
                                    cmdCariHareketleri.Dispose();
                                if(cmdUpdateCariGenel!=null)
                                    cmdUpdateCariGenel.Dispose();
                                if(cmdUpdateCariHareket!=null)
                                    cmdUpdateCariHareket.Dispose();
                                if(cmdUpdateStokHareket!=null)
                                    cmdUpdateStokHareket.Dispose();
                                if(adpFatIrsStokHareketSatirlariGetir!=null)
                                    adpFatIrsStokHareketSatirlariGetir.Dispose();
                            }
                            if (globalBaslik.Izahat == 93 || globalBaslik.Izahat == 94) 
                            {
                                if (cmdSayimDosyasi != null)
                                    cmdSayimDosyasi.Dispose();
                            }

                            if(cmdHareketSatirNo!=null  )
                                cmdHareketSatirNo.Dispose();
                        }

                        if (globalBaslik.Izahat == 20 || globalBaslik.Izahat == 22 || globalBaslik.Izahat == 26 || globalBaslik.Izahat == 28 || globalBaslik.Izahat == 32 || globalBaslik.Izahat == 34)
                        {
                            if (cmdAlFatIrsStokBaslik != null)
                                cmdAlFatIrsStokBaslik.Dispose();
                            
                            if (cmdAlFatIrsStokHareket != null)
                                cmdAlFatIrsStokHareket.Dispose();
                            if (cmdUpdateAlFatIrsStokBaslik != null)
                                cmdUpdateAlFatIrsStokBaslik.Dispose();

                            if ( cmdUpdateAlFatIrsStokHareket!=null)
                            {
                                cmdUpdateAlFatIrsStokHareket.Dispose();
                            }
                            if (cmdCariOpsiyon != null)
                                cmdCariOpsiyon.Dispose();
                            if (cmdCariGenel != null)
                                cmdCariGenel.Dispose();
                            if (cmdCariHareketleri != null)
                                cmdCariHareketleri.Dispose();
                            if (cmdStokHareket != null)
                                cmdStokHareket.Dispose();
                            if (cmdDepoEnvater != null)
                                cmdDepoEnvater.Dispose();
                            if (cmdUpdateCariGenel != null)
                                cmdUpdateCariGenel.Dispose();
                            if (cmdUpdateCariHareket != null)
                                cmdUpdateCariHareket.Dispose();
                            if (cmdUpdateStokHareket != null)
                                cmdUpdateStokHareket.Dispose();
                            if (adpFatIrsStokHareketSatirlariGetir != null)
                                adpFatIrsStokHareketSatirlariGetir.Dispose();
                            if (cmdHareketSatirNo != null)
                                cmdHareketSatirNo.Dispose();
                        }

                        if (globalBaslik.Izahat == 13)
                        {
                            if (cmdTahsilBaslik != null)
                                cmdTahsilBaslik.Dispose();
                            if (cmdTahsilHareket != null)
                                cmdTahsilHareket.Dispose();
                            if (cmdTahsilCariGenel != null)
                                cmdTahsilCariGenel.Dispose();
                            if (cmdTahsilCariHareket != null)
                                cmdTahsilCariHareket.Dispose();

                            if (globalHareket.Izahat == 1 || globalHareket.Izahat == 2 || globalHareket.Izahat == 3)
                            {
                                if (cmdTahsilNakitCekSenet != null)
                                    cmdTahsilNakitCekSenet.Dispose();
                            }
                            if (globalHareket.Izahat == 4)
                            {
                                if (cmdTahsilVisaGiris != null)
                                    cmdTahsilVisaGiris.Dispose();
                                if (cmdTahsilVisaPortfoy != null)
                                    cmdTahsilVisaPortfoy.Dispose();
                                if (cmdUpdateTahsilVisaPortfoy != null)
                                    cmdUpdateTahsilVisaPortfoy.Dispose();
                            }
                            if (cmdUpdateTahsilBaslik != null)
                                cmdUpdateTahsilBaslik.Dispose();
                            if (cmdUpdateTahsilCariHareket != null)
                                cmdUpdateTahsilCariHareket.Dispose();
                        }

                        //ORTAK
                        //cmdStokTipGetir.Dispose();
                        if (cmdGetSeriNo != null)
                            cmdGetSeriNo.Dispose();
                        if (c!=null)
                        {
                            c.Dispose();
                        }
                        //ORTAK
                    }

                    if (globalKasa != null)
                    {
                        if (globalKasa.Izahat == 160)
                        {
                            cmdKasaIslemi.Dispose();
                        }
                    }
                    
                }

                if (cmdAlSipBaslik != null)
                {
                    cmdAlSipBaslik.Dispose();
                }
                if (cmdAlSipHareket != null)
                {
                    cmdAlSipHareket.Dispose();
                }
                if (cmdUpdateTalepSipBaslik != null)
                {
                    cmdUpdateTalepSipBaslik.Dispose();
                }
                if (cmdDepoTransferBaslik != null)
                {
                    cmdDepoTransferBaslik.Dispose();
                }
                if (cmdDepoTransferHareket != null)
                {
                    cmdDepoTransferHareket.Dispose();
                }
                if (cmdUpdateDepoTransferBaslik != null)
                {
                    cmdUpdateDepoTransferBaslik.Dispose();
                }

                if (cmdDepoTransferKabulBaslik != null)
                {
                    cmdDepoTransferKabulBaslik.Dispose();
                }
                if (cmdDepoTransferKabulHareket != null)
                {
                    cmdDepoTransferKabulHareket.Dispose();
                }
                if (cmdUpdateDepoTransferKabulBaslik != null)
                {
                    cmdUpdateDepoTransferKabulBaslik.Dispose();
                }
                if (cmdDepoTransferKabulBaslikForDepoTransfer != null)
                {
                    cmdDepoTransferKabulBaslikForDepoTransfer.Dispose();
                }
                if (cmdGetBelgeBaslik != null)
                {
                    cmdGetBelgeBaslik.Dispose();
                }

            }
            disposed = true;

            globalBaslik = null;
            globalHareket = null;
            headerColumn = null;
            headerValue = null;
            itemColumn = null;
            itemValue = null;
            BelgeNo = null;
        }
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        ~VegaBelgeWrapper()
        {
            Dispose(false);
        }
    }
}
