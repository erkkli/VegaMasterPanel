using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SefimV2.Models
{
    public class AdminTanimlar
    {
        public static string Domain = "http://localhost:2222";
        public static string AdminThemeFolder = "~/Theme/Admin";
        public static string AdminEgitimPortalFolder = "/Egitim";
        public static string AdminPageUrl = "/Admin";
        public static string UserID = "1";
        public static string UserName = "admin";
        public static bool SecurityPass = false;
        public static string AllowedFileTypes = ".doc,.docx,.xls,.xlsx,.ppt,.pptx,.xml,.htm,.html,.pdf,.mpg,.mpeg,.mp4,.wma,.wmv,.ogg,.webm,.mp4v,.avi,.mp3,.wav,.mov,.flv,.swf,.txt,.rtf,.css,.js,.jpeg,.jpg,.png,.gif";
        public static string AllowedDocumentTypes = ".doc,.docx,.xls,.xlsx,.ppt,.pptx,.pdf,.txt,.rtf";
        public static string AllowedVideoTypes = ".mpg,.mpeg,.mp4,.wma,.wmv,.ogg,.webm,.mp4v,.avi,.flv,.swf";
        public static string AllowedAudioTypes = ".mp3,.wav,.mov";
        public static string AllowedImageTypes = ".jpeg,.jpg,.png,.gif";
        public static string AllowedScriptTypes = ".css,.js,.htm,.html";
        public static readonly Dictionary<string, string> FileTypeNames = new Dictionary<string, string>{
                                                {"doc"  ,   "Microsoft Word (Eski Sürüm)" },
                                                {"docx" ,   "Microsoft Word" },
                                                {"xls"  ,   "Microsoft Excel (Eski Sürüm)"},
                                                {"xlsx" ,   "Microsoft Excel"},
                                                {"ppt"  ,   "Microsoft Powerpoint (Eski Sürüm)"},
                                                {"pptx" ,   "Microsoft Powerpoint"},
                                                {"xml"  ,   "XML Veri Dosyası"},
                                                {"htm"  ,   "Web Sayfası (HTM)"},
                                                {"html" ,   "Web Sayfası (HTML)"},
                                                {"pdf"  ,   "Taşınabilir Belge Biçimi (PDF)"},
                                                {"mpg"  ,   "Video Dosyası (MPG)"},
                                                {"mpeg" ,   "Video Dosyası (MPEG)"},
                                                {"mp4"  ,   "Video Dosyası (MP4)"},
                                                {"wmv"  ,   "Video Dosyası (WMV)"},
                                                {"ogg"  ,   "Video/Ses Dosyası (OGG)"},
                                                {"webm" ,   "Video Dosyası (WEBM)"},
                                                {"mp4v" ,   "Video Dosyası (MP4V)"},
                                                {"avi"  ,   "Video Dosyası (AVI)"},
                                                {"mov"  ,   "Video Dosyası (MOV)"},
                                                {"wma"  ,   "Ses Dosyası (WMA)"},
                                                {"mp3"  ,   "Ses Dosyası (MP3)"},
                                                {"wav"  ,   "Ses Dosyası (WAV)"},
                                                {"flv"  ,   "Flash Medya Dosyası (FLV)"},
                                                {"swf"  ,   "Flash Dosyası (SWF)"},
                                                {"txt"  ,   "Text Dosyası"},
                                                {"rtf"  ,   "Zengin Metin Dosyası (RTF)"},
                                                {"css"  ,   "Css Dosyası"},
                                                {"js"   ,   "Javascript Dosyası"},
                                                {"jpeg" ,   "Görüntü Dosyası (JPEG)"},
                                                {"jpg"  ,   "Görüntü Dosyası (JPG)"},
                                                {"png"  ,   "Görüntü Dosyası (PNG)"},
                                                {"gif"  ,   "Görüntü Dosyası (GIF)"}
                                                };

        public static readonly Dictionary<string, string> OdemeTipleri = new Dictionary<string, string>{
                                                {"0"    ,   "Havale / EFT"},
                                                {"1"    ,   "Kredi Kartı"},
                                                {"2"    ,   "Nakit"}
                                                };

        public static readonly Dictionary<string, string> SiparisDurumlari = new Dictionary<string, string>{
                                                {"0"    ,   "Yeni Sipariş"},
                                                {"1"    ,   "Onaylandı"},
                                                {"2"    ,   "Tamamlandı"},
                                                {"3"    ,   "İptal Edildi"}
                                                };

        public static readonly Dictionary<string, string> OdemeDurumlari = new Dictionary<string, string>{
                                                {"0"    ,   "Ödeme Bekliyor"},
                                                {"1"    ,   "Ödendi"},
                                                {"2"    ,   "İptal/İade Edildi"}
                                                };

        public static readonly Dictionary<string, string> TeslimatDurumlari = new Dictionary<string, string>{
                                                {"0"    ,   "Onay Bekliyor"},
                                                {"1"    ,   "Hazırlanıyor"},
                                                {"2"    ,   "Kargoya Verildi"},
                                                {"3"    ,   "Teslim Edildi"}
                                                };

        public static readonly Dictionary<string, string> Aylar = new Dictionary<string, string>{
                                                {"1"    ,   "Ocak"},
                                                {"2"    ,   "Şubat"},
                                                {"3"    ,   "Mart"},
                                                {"4"    ,   "Nisan"},
                                                {"5"    ,   "Mayıs"},
                                                {"6"    ,   "Haziran"},
                                                {"7"    ,   "Temmuz"},
                                                {"8"    ,   "Ağustos"},
                                                {"9"    ,   "Eylül"},
                                                {"10"    ,   "Ekim"},
                                                {"11"    ,   "Kasım"},
                                                {"12"    ,   "Aralık"}
                                                };

    }
}
