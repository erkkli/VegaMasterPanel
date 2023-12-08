using System;
using System.Linq;
using System.Text;

namespace vrlibwin.Model.License
{
    public class LicenseHelper
    {
        //private string BaseAdress { get; set; } = "https://http://localhost:5001";
        private string BaseAdress { get; set; } = "https://lisans.vegayazilim.com.tr";

        private string EncodeDecodeKey { get; set; } = "BHBH"; /* B:Base64, H:Hex */

        public string MasterConnectionString { get; set; } = "";
        public string DefaultConnectionString { get; set; } = "";
        public int UrunId { get; set; } = 0;
        public string IsletmeKod { get; set; } = "";
        public string DonanimDeger { get; set; } = "";
        public string UygulamaVersiyon { get; set; } = "";
        public string DataVersiyon { get; set; } = "";
        public string OzelVeri { get; set; } = "";

        public LisansData MyLisansData { get; set; } = new LisansData();

        public LicenseHelper()
        {

        }

        #region yardımcı fonksiyonlar

        public string MyToStr(string _value)
        {
            return _value == null ? "" : _value.ToString();
        }

        public string MyToTrim(string _value)
        {
            return _value == null ? "" : _value.Trim();
        }

        public string MyToBase64Str(string _str)
        {
            return Convert.ToBase64String(Encoding.Unicode.GetBytes(this.MyToStr(_str)));
        }

        public string MyFromBase64Str(string _str)
        {
            return Encoding.Unicode.GetString(Convert.FromBase64String(this.MyToStr(_str)));
        }

        public string MyStrToHex(string _value, Encoding _encoding)
        {
            var sb = new StringBuilder();

            var bytes = _encoding.GetBytes(this.MyToStr(_value));
            foreach (var t in bytes)
            {
                sb.Append(t.ToString("X2"));
            }

            return sb.ToString();
        }

        public string MyHexToStr(string _value, Encoding _encoding)
        {
            string rValue = this.MyToStr(_value);

            var bytes = new byte[rValue.Length / 2];
            for (var i = 0; i < bytes.Length; i++)
            {
                bytes[i] = Convert.ToByte(rValue.Substring(i * 2, 2), 16);
            }

            rValue = _encoding.GetString(bytes);

            return rValue;
        }

        public string MyEncodeDecode(string str, char ED)
        {
            string key = this.EncodeDecodeKey;
            if (ED == 'D')
            {
                char[] charArray = key.Trim().ToCharArray();
                Array.Reverse(charArray);
                key = new string(charArray);
            }

            foreach (char islem in key.ToArray())
            {
                if (islem == 'B')
                {
                    str = this.MyFromBase64Str(str);
                }

                if (islem == 'H')
                {
                    str = this.MyHexToStr(str, Encoding.UTF8);
                }
            
                if (islem == 'S')
                {
                    str = this.MyToDecrypt(str, key, EnmSCType.Base64, Encoding.UTF8);

                }
            }

            return str;
        }

        private string EncryptOrDecrypt(string _str, string _key)
        {
            var result = new StringBuilder();

            for (int i = 0; i < _str.Length; i++)
            {
                // take next character from string
                char character = _str[i];

                // cast to a uint
                uint charCode = (uint)character;

                // figure out which character to take from the key
                int keyPosition = i % _key.Length; // use modulo to "wrap round"

                // take the key character
                char keyChar = _key[keyPosition];

                // cast it to a uint also
                uint keyCode = (uint)keyChar;

                // perform XOR on the two character codes
                uint combinedCode = charCode ^ keyCode;

                // cast back to a char
                char combinedChar = (char)combinedCode;

                // add to the result
                result.Append(combinedChar);
            }

            return result.ToString();
        }

        public string Encrypt(string text, string key, EnmSCType enmSCType, Encoding encoding)
        {
            text = this.ToStr(text);
            key = this.ToStr(key);

            text = EncryptOrDecrypt(text, key);

            if (enmSCType == EnmSCType.Base64)
            {
                text = Convert.ToBase64String(encoding.GetBytes(text));
            }

            if (enmSCType == EnmSCType.Hex)
            {
                text = MyStrToHex(text, encoding);
            }

            return text;
        }

        public string Decrypt(string text, string key, EnmSCType enmSCType, Encoding encoding)
        {
            text = this.ToStr(text);
            key = this.ToStr(key);

            if (enmSCType == EnmSCType.Base64)
            {
                text = encoding.GetString(Convert.FromBase64String(text));
            }

            if (enmSCType == EnmSCType.Hex)
            {
                text = MyHexToStr(text,encoding);
            }

            text = EncryptOrDecrypt(text, key);

            return text;
        }

        public  string MyToEncrypt( string _str, string _key, EnmSCType enmSCType, Encoding encoding)
        {
            _str = MyToStr(_str);
            _key = MyToStr(_key);
            return  Encrypt(_str, _key, enmSCType, encoding);
        }

        public  string MyToDecrypt( string _str, string _key, EnmSCType enmSCType, Encoding encoding)
        {
            _str = MyToStr(_str);
            _key = MyToStr(_key);
            return Decrypt(_str, _key, enmSCType, encoding);
        }

        private string ToStr(string s)
        {
            return s == null ? "" : s.ToString();
        }

        #endregion
    }

    public enum EnmSCType
    {
        None,
        Base64,
        Hex
    }
}
