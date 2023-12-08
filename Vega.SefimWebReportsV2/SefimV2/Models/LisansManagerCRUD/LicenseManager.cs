using Newtonsoft.Json;
using SefimV2.Helper;
using System;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Text;
using System.Web.Configuration;
using System.Web.Script.Serialization;
using vrlibwin.Model.License;
//using vr.Data;
//using vrlibwin.Data;

namespace SefimUtil
{
    public class LicenseManager
    {
        public const string Create_URL = "https://lisans.vegayazilim.com.tr/api/Lis/UrunLisansKayit";
        public const string Request_URL = "https://lisans.vegayazilim.com.tr/api/Lis/UrunLisansSorgu";

        public static LicenseStatusResult LicenseCotnrol(string productId)
        {
            var licenseStatusResult = new LicenseStatusResult
            {
                Status = false
            };

            try
            {
                var companyCode = string.Empty;
                var deviceID = string.Empty;
                var deviceIdUtil = HwUtil.LocalHardwareValue(DataService.GetConnection());
                var myByte = Encoding.UTF8.GetBytes(deviceIdUtil);
                var hexString = BitConverter.ToString(myByte);
                var deviceIdHex = hexString.Replace("-", "");
                string[] res = DataService.GetValue(productId)?.Split(';');

                if (res != null && res.Length >= 2)
                {
                    companyCode = res[0];
                    deviceID = res[1];
                }

                if (!string.IsNullOrEmpty(companyCode) && !string.IsNullOrEmpty(deviceID) && deviceID == deviceIdHex)
                {
                    if (res.Length == 3)
                    {
                        licenseStatusResult = LocalLicenseControl(licenseStatusResult, companyCode, deviceID, productId);
                    }
                    else
                    {
                        licenseStatusResult = LicenseCheck(companyCode, deviceID, productId);
                    }
                }
                else
                {
                    //lisans bilgileri giriş sayfası
                    licenseStatusResult.Message = "Lisans Bulunamadı.Lisans Talep Ediniz veya Bilgilerinizi Kontrol Ediniz.";
                }

                return licenseStatusResult;
            }
            catch (Exception ex)
            {
                Singleton.WritingLog("LicenseManagerException", "Hata" + ex);
                licenseStatusResult.Message = "Bir Hata Oluştu";
                return licenseStatusResult;
            }
        }
        private static LicenseStatusResult LocalLicenseControl(LicenseStatusResult licenseStatusResult, string companyCode, string deviceID, string productId)
        {
            licenseStatusResult = GetModule(productId);
            var activeUser = CheckConnectionCount();

            if (DateTime.Now.Date < licenseStatusResult.SonErisimZaman.Date || (DateTime.Now.Date - licenseStatusResult.SonErisimZaman.Date).Days >= licenseStatusResult.MinimumErisimGun)
            {
                licenseStatusResult = LicenseCheck(companyCode, deviceID, productId);
                if (!licenseStatusResult.Status)
                {
                    return licenseStatusResult;
                }
            }

            //Güncelleme bitiş tarihi build tarihinden önce ise bloklanacak.(26052021)
            DateTime guncellemeBitis = Convert.ToDateTime(licenseStatusResult.GuncellemeBitis.ToString("dd'/'MM'/'yyyy"));
            DateTime BuildDate = new FileInfo(Assembly.GetExecutingAssembly().Location).LastWriteTime;
            DateTime buildDate = Convert.ToDateTime(BuildDate.ToString("dd'/'MM'/'yyyy"));

            if (guncellemeBitis.Date < buildDate.Date)
            {
                licenseStatusResult.Status = false;
                licenseStatusResult.Message = "Lisans Süreniz Dolmuştur (" + guncellemeBitis + "). Lütfen Servis Sağlayıcınız İle İletişime Geçiniz.";
                return licenseStatusResult;
            }

            //if (!(DateTime.Now.Date <= licenseStatusResult.LisansBitis.Date))
            //{
            //    licenseStatusResult.Status = false;
            //    licenseStatusResult.Message = "Lisans Süreniz Dolmuştur. Lütfen Servis Sağlayıcınız İle İletişime Geçiniz.";
            //    return licenseStatusResult;
            //}

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
        public static LicenseStatusResult LicenseRegister(string username, string password, string companyCode, string licenceKey, string productId)
        {
            var licenseStatusResult = new LicenseStatusResult
            {
                Status = false
            };

            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            // Use SecurityProtocolType.Ssl3 if needed for compatibility reasons
            //lisans kayıt
            var client = new HttpClient
            {
                BaseAddress = new Uri(Create_URL)
            };

            try
            {
                var deviceIdUtil = HwUtil.LocalHardwareValue(DataService.GetConnection());
                var myByte = Encoding.UTF8.GetBytes(deviceIdUtil);
                var hexString = BitConverter.ToString(myByte);
                var deviceIdHex = hexString.Replace("-", "");

                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                //client.DefaultRequestHeaders.ConnectionClose = true;//!!
                string urlParameters = "?KullaniciAd=" + username +
                                       "&KullaniciSifre=" + password +
                                       "&UrunId=" + productId +
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
                        DataService.DeleteValue(productId, companyCode);
                        DataService.SetValue(productId, deviceIdHex, companyCode);
                    }
                }

                client.Dispose();
                return licenseStatusResult;
            }
            catch (Exception ex)
            {
                Singleton.WritingLog("LicenseRegisterException", "Hata" + ex);
                licenseStatusResult.Message = "Bir Hata Oluştu. Lisansa Bağlanamıyor.";
                client.Dispose();
                return licenseStatusResult;
            }
        }
        public static LicenseStatusResult LicenseCheck(string companyCode, string deviceID, string productId)
        {
            var licenseStatusResult = new LicenseStatusResult
            {
                Status = false
            };

            HttpResponseMessage responseRequest = null;
            HttpClient client = new HttpClient
            {
                //lisans sorgu
                BaseAddress = new Uri(Request_URL)
            };
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            string requestUrlParameters = "?UrunId=" + productId +
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
            catch (Exception ex)
            {
                Singleton.WritingLog("ChackException", "Exception:" + ex.Message);
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

                if (resultSorgu.Sonuc == false && resultSorgu.Mesaj == "Aktif Lisans kaydı bulunamadı.")
                {
                    licenseStatusResult.Status = false;
                    licenseStatusResult.Message = resultSorgu.Mesaj;

                    return licenseStatusResult;
                }

                if (resultSorgu.Sonuc)
                {
                    if (resultSorgu.JsonLisansList.Any())
                    {
                        var lisansResultList = resultSorgu.JsonLisansList.Where(x => x.UrunId == Convert.ToInt32(productId)).ToList();
                        for (int i = 0; i < lisansResultList.Count(); i++)
                        {
                            DataService.SetValue(productId, deviceID + ";" + lisansResultList[i].EncodedJson, companyCode);
                        }

                        licenseStatusResult = GetModule(productId);
                        var activeUser = CheckConnectionCount();

                        if (licenseStatusResult.KullaniciSayisi < activeUser)
                        {
                            licenseStatusResult.Status = false;
                            licenseStatusResult.Message = "Kullanıcı Sayısı Aşıldı.";
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
        public static LicenseStatusResult GetModule(string productId)
        {
            var licenseStatusResult = new LicenseStatusResult();
            var helper = new LicenseHelper();
            string keyFix = null;
            string[] keyArr = DataService.GetValue(productId)?.Split(';');
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
            licenseStatusResult.GuncellemeBitis = helper.MyLisansData.GuncellemeBitis;
            return licenseStatusResult;
        }

        //public static void DeleteLicense(int urunId)
        //{
        //    SqlService.DeleteLicense(urunId);
        //}
        public static int CheckConnectionCount()
        {
            SqlConnection con = null;
            var macAddr =
            (
                from nic in NetworkInterface.GetAllNetworkInterfaces()
                where nic.OperationalStatus == OperationalStatus.Up
                select nic.GetPhysicalAddress().ToString()
            ).FirstOrDefault();


            if (con == null)
            {
                string SqlConnString = "Data Source=" + WebConfigurationManager.AppSettings["Server"] + ";Initial Catalog=master;Persist Security Info=True;User Id=" + WebConfigurationManager.AppSettings["User"] + ";Password=" + WebConfigurationManager.AppSettings["Password"] + "; MultipleActiveResultSets=true;Application Name = VegaMaster; Workstation Id = " + macAddr + "; ";
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