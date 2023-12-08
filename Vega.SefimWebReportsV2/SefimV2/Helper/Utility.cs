using System;
using System.Globalization;
using System.Linq;

namespace SefimV2.Helper
{
    public class Utility
    {
        internal static int getInt(object p)
        {
            if (p == null) return 0;
            if (p is DBNull) return 0;
            if (p is Int32) return (int)p;
            int retVal = 0;
            int.TryParse(p.ToString(), out retVal);
            return retVal;
        }
        //
        internal static bool getBool(object p)
        {
            if (p == null) return false;
            if (p is DBNull) return false;
            if (p is Boolean) return (bool)p;
            return Boolean.TrueString.Equals(p.ToString());
        }

        internal static decimal getDecimal(object p)
        {
            if (p == null) return 0m;
            if (p is DBNull) return 0m;
            if (p is Decimal) return (decimal)p;
            if (p is string)
            {
                p = ((string)p).Replace(",", NumberFormatInfo.CurrentInfo.CurrencyDecimalSeparator);
            }
            decimal retVal = 0m;
            decimal.TryParse(p.ToString(), out retVal);
            return retVal;
        }

        internal static string getString(object p)
        {
            if (p == null)
                return "";
            if (p is DBNull)
                return "";

            return p.ToString();
        }

        internal static string qs(string val)
        {
            const string quotes = "'";
            if (val == null) return val;
            string retval = "";
            for (int i = 0; i < val.Length; i++)
            {
                retval += val[i];
                if (quotes.Contains(val[i]))
                    retval += val[i];
            }
            retval = "'" + retval + "'";
            return retval;
        }

        internal static DateTime ParseOLEDate(string p)
        {
            double i = 0f;
            string doubleStr = p;
            doubleStr = doubleStr.Replace('.', CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator[0]);
            doubleStr = doubleStr.Replace(',', CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator[0]);

            if (double.TryParse(doubleStr, out i)) //excelde date alanlari double 23123.22 gibi gelebiliyor.
            {
                return DateTime.FromOADate(i);
            }
            else
            {
                //String olarak yazilmiş bir date olabilir.
                DateTime retval;
                if (DateTime.TryParse(p, out retval))
                    return retval;
                else
                    throw new ApplicationException(p + " string değeri bir tarih değerine dönüştürülemedi.");

            }
        }
    }
}