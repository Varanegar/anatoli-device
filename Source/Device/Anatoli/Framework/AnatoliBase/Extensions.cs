using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Anatoli.Framework.AnatoliBase
{
    public static class Extensions
    {
        public static string ToCurrency(this double value)
        {
            return Math.Round(value / 10, 0).ToString("N0");
        }
        public static string ToCurrency(this decimal value)
        {
            return Math.Round(value / 10, 0).ToString("N0");
        }
        public static string PersianToArabic(this string str)
        {
            str = str.Replace("ی", "ي");
            str = str.Replace("ک", "ك");
            return str;
        }

        public static DateTime ConvertFromUnixTimestamp(this double timestamp)
        {
            DateTime origin = new DateTime(1970, 1, 1, 0, 0, 0, 0);
            return origin.AddSeconds(timestamp);
        }


        public static double ConvertToUnixTimestamp(this DateTime date)
        {
            DateTime origin = new DateTime(1970, 1, 1, 0, 0, 0, 0);
            TimeSpan diff = date - origin;
            return Math.Floor(diff.TotalSeconds);
        }
    }
}
