using System.ComponentModel;
using System.Net;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Text;

namespace SefimV2.Repository
{
    public class SystemClass
    {
        #region SEND MAIL 
        public static bool MailSend(string ToEmail, string Subject, string EmailContent, [DefaultValue(false)] bool bannerState)
        {
            bool result = false;
            try
            {
                SmtpClient sc = new SmtpClient();
                sc.Port = 587;
                sc.Host = "smtp.gmail.com";
                sc.EnableSsl = true;

                sc.Credentials = new NetworkCredential("ekrem.demo06@gmail.com", "mnbvcxzaqwerty.");

                MailMessage mail = new MailMessage();

                mail.From = new MailAddress("ekrem.demo06@gmail.com", "Vega Master Test");

                mail.To.Add(ToEmail);

                //mail.To.Add("alici2@mail.com");
                //mail.CC.Add("alici3@mail.com");
                //mail.CC.Add("alici4@mail.com");

                mail.Subject = Subject;
                mail.IsBodyHtml = true;

                #region BANNER GOSTERIMI DINAMIK YAPMAK ICIN 
                if (bannerState == true)
                {
                    EmailContent += mailBanner();
                }
                #endregion


                mail.Body = EmailContent;

                //mail.Attachments.Add(new Attachment(@"C:\Rapor.xlsx"));
                //mail.Attachments.Add(new Attachment(@"C:\Sonuc.pptx"));

                sc.Send(mail);
                result = true;
            }
            catch
            {
                result = false;
            }

            return result;
        }
        #endregion SEND MAIL 

        public static string mailBanner()
        {
            string banner = "<br/><a href='http://vega.com'><br/><img src='http://vega.com/assets/images/mailBanner.jpg'/></a>";
            return banner;
        }

        #region Md5 Fonksiyonu
        public static string md5_hash(string value)
        {
            StringBuilder Sb = new StringBuilder();
            using (MD5 md5Hash = MD5.Create())
            {
                byte[] data = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(value));

                StringBuilder sBuilder = new StringBuilder();

                for (int i = 0; i < data.Length; i++)
                {
                    Sb.Append(data[i].ToString("x2"));
                }
            }

            return Sb.ToString();
        }
        #endregion Md5 Fonksiyonu


        #region summerNote base64 
        public static string Base64Decode(string base64EncodedData)
        {
            var base64EncodedBytes = System.Convert.FromBase64String(base64EncodedData);
            return System.Text.Encoding.UTF8.GetString(base64EncodedBytes);
        }
        public static string Base64Encode(string plainText)
        {
            if (plainText == "" || plainText == null) return "";
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
            return System.Convert.ToBase64String(plainTextBytes);
        }
        #endregion summerNote base64 
    }
}