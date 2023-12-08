using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace SefimV2.Repository
{
    public static class OEncoder
    {
        public static string OEncode(string p)
        {
            return BitConverter.ToString(Encoding.ASCII.GetBytes(Convert.ToBase64String(Encoding.ASCII.GetBytes(p)))).Replace("-", "");
        }

        public static string ODecode(string p)
        {
            return Encoding.ASCII.GetString(Convert.FromBase64String(Encoding.ASCII.GetString(StringToByteArray(p))));
        }

        private static byte[] StringToByteArray(string hex)
        {

            return Enumerable.Range(0, hex.Length)
                             .Where(x => x % 2 == 0)
                             .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
                             .ToArray();
        }
    }
}