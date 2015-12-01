using System;
using System.Collections;
using System.IO;
using System.Text;

namespace AiLib.Mvc
{
    public static class MvcOutput
    {
        public static StringBuilder AddValues(this StringBuilder str, params object[] values)
        {
            foreach (object val in values)
                str.Append(val == null ? string.Empty : val.ToString());
            return str;
        }

        public static void WriteTo(this TextWriter Output, params object[] values)
        {
            foreach (object val in values)
                Output.Write(val == null ? string.Empty : val.ToString() ?? String.Empty);
        }

        public static void WriteTo(this TextWriter Output, IEnumerable values)
        {
            foreach (object val in values)
                Output.Write(val == null ? string.Empty : val.ToString() ?? String.Empty);
        }
    }
}

