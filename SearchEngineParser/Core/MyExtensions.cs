
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

namespace SearchEngineParser.Core
{
    public static class MyExtensions
    {
        public static bool ContainsAny(this string text, string searchin, bool CosiderSpaceAsDelimiter = true, StringComparison comparison = StringComparison.CurrentCulture)
        {
            var delmiter = ";,".ToCharArray();
            if (CosiderSpaceAsDelimiter)
                delmiter = " ;,".ToCharArray();
            
            //*********************
            //string[] col = { ".com", ".cn", ".co.uk", ".net.uk", ".edu", ".net", ".org", ".info" };

            //foreach (var item in col)
            //{
            //    searchin = searchin.ToLower().Replace(item, "");
            //}
            //GetDomain.GetDomainFromUrl("http://www.beta.microsoft.com/path/page.htm")
            var parts = searchin.ToLower().Replace("site:","").Split(delmiter, StringSplitOptions.RemoveEmptyEntries).ToList();
            
            var res = parts.Any(p => text.ToLower().IndexOf(p, comparison) > -1);
            return res;
        }
        public static string ReplaceFirst(this string text, string search, string replace,bool IgnorCase=false )
        {
            int pos = -1;
            if (IgnorCase)
                pos = text.ToLower().IndexOf(search.ToLower());
            else
                pos = text.IndexOf(search);
            if (pos < 0)
            {
                return text;
            }
            return text.Substring(0, pos) + replace + text.Substring(pos + search.Length);
        }
        /// <summary>
        /// Use this method to get distinct results of linq query.
        /// Example of usage:
        /// 1- var query = people.DistinctBy(p => p.Id);
        /// 2- var query = people.DistinctBy(p => new { p.Id, p.Name });
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <typeparam name="TKey"></typeparam>
        /// <param name="source"></param>
        /// <param name="keySelector"></param>
        /// <returns></returns>
        public static IEnumerable<TSource> DistinctBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector)
        {
            HashSet<TKey> seenKeys = new HashSet<TKey>();
            foreach (TSource element in source)
            {
                if (seenKeys.Add(keySelector(element)))
                {
                    yield return element;
                }
            }
        }

        public static string CleanFileNameFromInvalidChars(this string filename)
        {
            string file = filename;
            file = string.Concat(file.Split(System.IO.Path.GetInvalidFileNameChars(), StringSplitOptions.RemoveEmptyEntries));

            if (file.Length > 250)
            {
                file = file.Substring(0, 250);
            }
            return file;
        }

        public static string ReplaceWithIgnoreCase(this string input, string OldValue, string NewValue)
        {
            NewValue = NewValue + "";
            OldValue = OldValue + "";
            return Regex.Replace(input, OldValue, NewValue, RegexOptions.IgnoreCase);

        }

        public static string StripHTML(this string HTMLText)
        {
            if (String.IsNullOrEmpty(HTMLText))
                return "";
            Regex reg = new Regex("<[^>]+>", RegexOptions.IgnoreCase);
            return reg.Replace(HTMLText, "");
        }

        public static int ToInt(this string str)
        {
            return int.Parse(str);
        }
        public static int? ToIntOrNull(this string str)
        {
            try { return int.Parse(str); }
            catch { return null; }
        }
        public static double ToDouble(this string str)
        {
            return double.Parse(str);
        }
        public static double? ToDoubleOrNull(this string str)
        {
            try { return double.Parse(str); }
            catch { return null; }
            
        }
        public static decimal ToDecimal(this string str)
        {
            return decimal.Parse(str);
        }
        public static decimal? ToDecimalOrNull(this string str)
        {
            try { return decimal.Parse(str); }
            catch { return null; }
        }
        public static DateTime ToDateTime(this string str)
        {
            return Convert.ToDateTime(str);
        }
        public static DateTime? ToDateTimeOrNull(this string str)
        {
            try { return DateTime.Parse(str); }
            catch { return null; }
        }
        public static bool ToBool(this string str)
        {
            if (string.IsNullOrWhiteSpace(str)) return false;
            if (str.Trim() == "1" || str.Trim().ToLower() == "true" || str.Trim().ToLower() == "yes" || str.Trim().ToLower() == "y") return true;
            if (str.Trim() == "0" || str.Trim().ToLower() == "false" || str.Trim().ToLower() == "no" || str.Trim().ToLower() == "n") return false;
            return bool.Parse(str);

        }
        public static bool? ToBoolOrNull(this string str)
        {
            if (string.IsNullOrWhiteSpace(str)) return null;
            if (str.Trim() == "1" || str.Trim().ToLower() == "true" || str.Trim().ToLower() == "yes" || str.Trim().ToLower() == "y") return true;
            if (str.Trim() == "0" || str.Trim().ToLower() == "false" || str.Trim().ToLower() == "no" || str.Trim().ToLower() == "n") return false;
            try { return bool.Parse(str); }
            catch { return null; }
        }
        public static string ToHumanReadable(this TimeSpan Duration)
        {
            return Duration.ToAgeSpan().ToString(0, true);
        }
        public static string ToFormattedFileSize(this long FileSize)
        {
            // Returns the human-readable file size for an arbitrary, 64-bit file size 
            // The default format is "0.### XB", e.g. "4.2 KB" or "1.434 GB"
            long i = FileSize;
            string sign = (i < 0 ? "-" : "");
            double readable = (i < 0 ? -i : i);
            string suffix;
            if (i >= 0x1000000000000000) // Exabyte
            {
                suffix = "EB";
                readable = (double)(i >> 50);
            }
            else if (i >= 0x4000000000000) // Petabyte
            {
                suffix = "PB";
                readable = (double)(i >> 40);
            }
            else if (i >= 0x10000000000) // Terabyte
            {
                suffix = "TB";
                readable = (double)(i >> 30);
            }
            else if (i >= 0x40000000) // Gigabyte
            {
                suffix = "GB";
                readable = (double)(i >> 20);
            }
            else if (i >= 0x100000) // Megabyte
            {
                suffix = "MB";
                readable = (double)(i >> 10);
            }
            else if (i >= 0x400) // Kilobyte
            {
                suffix = "KB";
                readable = (double)i;
            }
            else
            {
                return i.ToString(sign + "0 B"); // Byte
            }
            readable /= 1024;

            return sign + readable.ToString("0.## ") + suffix;

            // return String.Format(new FileSizeFormatProvider(), "{0:fs}", FileSize);
        }

      public static   string AddSpacesToSentence(this string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return "";
            StringBuilder newText = new StringBuilder(text.Length * 2);
            newText.Append(text[0]);
            for (int i = 1; i < text.Length; i++)
            {
                if (char.IsUpper(text[i]) && text[i - 1] != ' ')
                    newText.Append(' ');
                newText.Append(text[i]);
            }
            return newText.ToString();
        }

        public static string ToStringOrNone(this string str)
        {
            if (string.IsNullOrWhiteSpace(str))
                return "None";
            else
                return str;
        }
        public static Exception Innerexception(this Exception exception)
        {

            while (exception.InnerException != null)
                exception = exception.InnerException;

            return exception;
        }


      
      

  

      
    }

   


    
}