using System;
using System.Collections.Specialized;
using System.Web;

namespace AiLib.Web
{
    public static class StringEq
    {
        public static string SubStringSafe(this string str, int pos1, int length = 0)
        {
            if (str == null || str.Length == 0 || pos1 > str.Length)
                return str;

            if (pos1 + length > str.Length)
                return str.Substring(pos1);

            return str.Substring(pos1, length);
        }

        public static string ToUpperNull(this string str)
        {
            return string.IsNullOrWhiteSpace(str) ? str : str.ToUpper();
        }

        public static string ToLowerNull(this string str)
        {
            return string.IsNullOrWhiteSpace(str) ? str : str.ToLower();
        }

        public static bool EqualNull(this string str, string toStr, StringComparison compararison = StringComparison.InvariantCultureIgnoreCase)
        {
            return string.IsNullOrWhiteSpace(str) ? str == toStr : str.Equals(toStr, compararison);
        }
    }
}

namespace AiLib.Web.Reflection
{
    public static class WebStringConvert
    {
        #region Web Encodes, Parse

        public static string Encode(this string str)
        {
            return HttpUtility.HtmlEncode(str);
        }

        public static string Decode(this string str)
        { 
            return HttpUtility.HtmlDecode(str);
        }

        public static string HtmlAttributeEncode(this string str)
        { 
            return HttpUtility.HtmlAttributeEncode(str);
        }

        public static NameValueCollection ParseQueryString(string query = null)
        {
            query = query ?? HttpStatic.Request.Url.Query;
            return HttpUtility.ParseQueryString(query);
        }
        #endregion

        #region String case

        // Null safe
        public static bool ContainsCase(this string str, string substring, 
            StringComparison comparisonType = StringComparison.InvariantCultureIgnoreCase)
        {
            if (str == null || str.Length == 0 || substring == null || substring.Length == 0)
                return false;
            return str.IndexOf(substring, comparisonType: comparisonType) >= 0;
        }

        public static bool EqualsCase(this string str, object obj, 
            StringComparison comparisonType = StringComparison.InvariantCultureIgnoreCase)
        {
            if (str == null && obj == null)
                return false;

            var objStr = obj is string ? obj as string : obj.ToString();
            return String.Equals(str, objStr, comparisonType: comparisonType);
        }

        public static bool StrEquals<T>(this T objA, T objB,
          StringComparison comparisonType = StringComparison.InvariantCulture) where T : class
        {
            if (objA == null || objB == null)
                return false;
            if (Object.ReferenceEquals(objA, objB))
                return true;

            var strA = objA.ToString();
            var strB = objB.ToString();
            if (string.IsNullOrWhiteSpace(strA) && string.IsNullOrWhiteSpace(strB))
                return true;

            return string.Equals(strA, strB, comparisonType: comparisonType);
        }

        #endregion

        #region NewLines 

        public static string[] NewLines = new string[] { System.Environment.NewLine };
        public static char[] NewChar = new char[] { '\n' };
        public static string NewCharRemove = "\r";

        // Safe null
        public static string[] SplitNewLines(this string str)
        {
            return str == null ? null : str.Split(NewLines, options: StringSplitOptions.RemoveEmptyEntries);
        }

        public static string[] SplitLines(this string str)
        {
            if (str == null) return new string[1] { null };

            var fixStr = str.Replace(NewCharRemove, string.Empty);
            return fixStr.Split(NewChar, options: StringSplitOptions.None);
        }

        #endregion

        public static bool IsArrayEmpty(this object[] data)
        {
            if (data == null || data.Length == 0)
                return true;
            int index = 0;
            while (index < data.Length)
            {
                if (data[index] == null || string.IsNullOrWhiteSpace(data[index].ToString()) )
                    return true;
                index++;
            }

            return false;
        }

        // Safe null
        public static string PadRight(this string str, int len, char paddingChar = ' ')
        {
            return str == null ? null : str.PadRight(len, paddingChar);
        }
    }
}
