using System;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Net.Http;
using System.Net.Http.Headers;
using vrlibwin.Model.License;
using System.Web.Script.Serialization;
using Newtonsoft.Json;
using SefimV2.Helper;
using System.Web.Configuration;
using System.Data;
using System.Net.NetworkInformation;

//using vr.Data;
//using vrlibwin.Data;

namespace SefimUtil
{
    public class LicenseManager
    {
        public const string Create_URL = "https://lisans.vegayazilim.com.tr/api/Lis/UrunLisansKayit";
        public const string Request_URL = "https://lisans.vegayazilim.com.tr/api/Lis/UrunLisansSorgu";

        public static LicenseStatusResult LicenseCotnrol()
        {
            LicenseStatusResult licenseStatusResult = new LicenseStatusResult();
            licenseStatusResult.Status = false;

            var companyCode = "";
            var deviceID = "";

            var deviceIdUtil = HwUtil.LocalHardwareValue(DataService.GetConnection());
            var myByte = Encoding.UTF8.GetBytes(deviceIdUtil);
            var hexString = BitConverter.ToString(myByte);
            var deviceIdHex = hexString.Replace("-", "");
            string[] res = DataService.GetValue()?.Split(';');


            if (res != null && res.Length >= 2)
            {
                companyCode = res[0];
                deviceID = res[1];
            }
            if (!String.IsNullOrEmpty(companyCode) && !String.IsNullOrEmpty(deviceID) && deviceID == deviceIdHex)
            {
                if (res.Length == 3)
                {
                    licenseStatusResult = LocalLicenseControl(licenseStatusResult, companyCode, deviceID);
                }
                else
                {
                    licenseStatusResult = LicenseCheck(companyCode, deviceID);
                }
            }
            //else
            //{
            //    //lisans bilgileri giriş sayfası
            //}

            return licenseStatusResult;
        }
        private static LicenseStatusResult LocalLicenseControl(LicenseStatusResult licenseStatusResult, string companyCode,string deviceID)
        {

            licenseStatusResult = GetModule();

            var activeUser = CheckConnectionCount();

            if (DateTime.Now.Date < licenseStatusResult.SonErisimZaman.Date && (DateTime.Now.Date - licenseStatusResult.SonErisimZaman.Date).Days >= licenseStatusResult.MinimumErisimGun)
            {
                licenseStatusResult = LicenseCheck(companyCode, deviceID);
                if (!licenseStatusResult.Status)
                {
                    return licenseStatusResult;
                }
            }

            if (!(DateTime.Now.Date <= licenseStatusResult.LisansBitis.Date))
            {
                licenseStatusResult.Status = false;
                licenseStatusResult.Message = "Lisans Süreniz Dolmuştur. Lütfen Servis Sağlayıcınız İle İletişime Geçiniz.";
                return licenseStatusResult;

            }

            if (licenseStatusResult.KullaniciSayisi < activeUser)
            {
                licenseStatusResult.Status = false;
                licenseStatusResult.Message = "Kullanıcı Sayısı Aşıldı";
                return licenseStatusResult;
            }

            licenseStatusResult.Status = true;
            licenseStatusResult.Message = "Bu Ürün Lisanslıdır";
            return licenseStatusResult;
        }
        public static LicenseStatusResult LicenseRegister(string username,string password,string companyCode,string licenceKey)
        {
            var deviceIdUtil = HwUtil.LocalHardwareValue(DataService.GetConnection());
            var myByte = Encoding.UTF8.GetBytes(deviceIdUtil);
            var hexString = BitConverter.ToString(myByte);
            var deviceIdHex = hexString.Replace("-", "");

            LicenseStatusResult licenseStatusResult = new LicenseStatusResult();
            licenseStatusResult.Status = false;
            //lisans kayıt
            HttpResponseMessage responseRequest = null;
            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri(Create_URL);
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            string urlParameters = "?KullaniciAd=" + username +
                                   "&KullaniciSifre=" + password +
                                   "&UrunId=139" +
                                   "&IsletmeKod=" + companyCode +
                                   "&DonanimDeger=" + deviceIdHex +
                                   "&LisansAnahtar=" + licenceKey;

            HttpResponseMessage response = client.GetAsync(urlParameters).Result;
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                licenseStatusResult.Status = false;
                licenseStatusResult.Message = "Sunucuya Erişim Yok";

                return licenseStatusResult;
            }

            licenseStatusResult.Status = response.IsSuccessStatusCode;

            ResultLicense resultSorgu = new ResultLicense();
            using (HttpContent content = response.Content)
            {
                System.Threading.Tasks.Task<string> resultMessage = content.ReadAsStringAsync();
                licenseStatusResult.Message = resultMessage.Result;

                resultSorgu = new JavaScriptSerializer().Deserialize<ResultLicense>(licenseStatusResult.Message);
                licenseStatusResult.Status = resultSorgu.Sonuc;
                if (resultSorgu.Mesaj == null)
                {
                    licenseStatusResult.Message = "Lütfen Lisans Bilgilerini Giriniz.";
                }
                else
                {
                    licenseStatusResult.Message = resultSorgu.Mesaj;
                }

                if (licenseStatusResult.Status)
                {
                    DataService.DeleteValue("139", companyCode);
                    DataService.SetValue("139", deviceIdHex, companyCode);
                }

            }

            client.Dispose();
            return licenseStatusResult;
        }
        public static LicenseStatusResult LicenseCheck( string companyCode,string deviceID)
        {
            LicenseStatusResult licenseStatusResult = new LicenseStatusResult();
            licenseStatusResult.Status = false;

            HttpResponseMessage responseRequest = null;
            HttpClient client = new HttpClient();
            //lisans sorgu
            client.BaseAddress = new Uri(Request_URL);
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            string requestUrlParameters = "?UrunId=139"+
                                          "&IsletmeKod=" + companyCode +
                                          "&DonanimDeger=" + deviceID +
                                          "&DataVersiyon=0" +
                                          "&UygulamaVersiyon=0";

            try
            {
                responseRequest = client.GetAsync(requestUrlParameters).Result;
                if (responseRequest.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    licenseStatusResult.Status = false;
                    licenseStatusResult.Message = "Sunucu ile Bağlantı Kurulamadı.";
                    return licenseStatusResult; //LocalLicenseControl(licenseStatusResult, productID);   
                }
            }
            catch
            {
                licenseStatusResult.Status = false;
                licenseStatusResult.Message = "İnternet Bağlantınızı Kontrol Ediniz.";
                return licenseStatusResult; //LocalLicenseControl(licenseStatusResult, productID);
            }

            licenseStatusResult.Status = responseRequest.IsSuccessStatusCode;

            ResultLicense resultSorgu = new ResultLicense();
            using (HttpContent content = responseRequest.Content)
            {
                System.Threading.Tasks.Task<string> resultMessage = content.ReadAsStringAsync();
                licenseStatusResult.Message = resultMessage.Result;

                resultSorgu = new JavaScriptSerializer().Deserialize<ResultLicense>(licenseStatusResult.Message);
                licenseStatusResult.Status = resultSorgu.Sonuc;

                if (resultSorgu.Sonuc == false && resultSorgu.Mesaj == "Aktif Lisans kaydı bulunamadı")
                {
                    licenseStatusResult.Status = false;
                    licenseStatusResult.Message = resultSorgu.Mesaj;

                    return licenseStatusResult;
                }

                if (resultSorgu.Sonuc)
                {
                    if (resultSorgu.JsonLisansList.Any())
                    {
                        var lisansResultList = resultSorgu.JsonLisansList.Where(x => x.UrunId == 139).ToList();
                        for (int i = 0; i < lisansResultList.Count(); i++)
                        {
                            DataService.SetValue("139", deviceID + ";" + lisansResultList[i].EncodedJson, companyCode);
                        }

                        licenseStatusResult = GetModule();
                        var activeUser = CheckConnectionCount();
                       
                        if (licenseStatusResult.KullaniciSayisi < activeUser)
                        {
                            licenseStatusResult.Status = false;
                            licenseStatusResult.Message = "Kullanıcı Sayısı Aşıldı";
                            return licenseStatusResult;
                        }
                    }
                }
            }

            if (!resultSorgu.Sonuc)
            {
                licenseStatusResult.Status = false;
                licenseStatusResult.Message = resultSorgu.Mesaj;

                return licenseStatusResult;
            }
            else
            {
                licenseStatusResult.Status = true;
                licenseStatusResult.Message = "Bu ürün lisanslı bir üründür.";

                return licenseStatusResult;
            }
        }
        public static LicenseStatusResult GetModule()
        {
            LicenseStatusResult licenseStatusResult=new LicenseStatusResult();
            string keyFix = null;
            LicenseHelper helper = new LicenseHelper();
            string[] keyArr = DataService.GetValue()?.Split(';');
            if (keyArr != null && !String.IsNullOrEmpty(keyArr[2]))
            {
                keyFix = keyArr[2];
            }
            else
            {
                licenseStatusResult.Status = false;
                licenseStatusResult.Message = "Lisans Dosyası Hatalı, Lütfen Lisans Dosyasını silip, tekrar oluşturun.";
                return licenseStatusResult;
            }

            string decodeStr = helper.MyEncodeDecode(keyFix, 'D');
            helper.MyLisansData = JsonConvert.DeserializeObject<LisansData>(decodeStr);
            licenseStatusResult.Moduller = helper.MyLisansData.Moduller;
            licenseStatusResult.KullaniciSayisi = helper.MyLisansData.KullaniciSayisi;
            licenseStatusResult.LisansBitis = helper.MyLisansData.LisansBitis;
            licenseStatusResult.SonErisimZaman = helper.MyLisansData.SonErisimZaman;
            licenseStatusResult.MinimumErisimGun = helper.MyLisansData.MinimumErisimGun;
            licenseStatusResult.UrunId = helper.MyLisansData.UrunId;
            return licenseStatusResult;
        }

        //public static void DeleteLicense(int urunId)
        //{
        //    SqlService.DeleteLicense(urunId);
        //}
        private static SqlConnection con=null;
        public static int CheckConnectionCount()
        {

            var macAddr =
            (
                from nic in NetworkInterface.GetAllNetworkInterfaces()
                where nic.OperationalStatus == OperationalStatus.Up
                select nic.GetPhysicalAddress().ToString()
            ).FirstOrDefault();


            if (con == null)
            {
                string SqlConnString = "Data Source=" + WebConfigurationManager.AppSettings["Server"] + ";Initial Catalog=master;Persist Security Info=True;User ID=" + WebConfigurationManager.AppSettings["User"] + ";Password=" + WebConfigurationManager.AppSettings["Password"] + "; MultipleActiveResultSets=true;Application Name = VegaMaster; Workstation ID = "+macAddr+"; ";
                con = new SqlConnection(SqlConnString);
            }

            if (con.State != ConnectionState.Open)
            {
                con.Open();
            }
            SqlCommand cmd = new SqlCommand("select count(Distinct hostname) as [user] from sys.sysprocesses where RTRIM(program_name) like 'VegaMaster'", con);
            return (int)cmd.ExecuteScalar();
        }        
    }
}
