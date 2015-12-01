using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AiLib
{
    public static class Assert
    {
        public static void IsTrue(bool condition, string errorText)
        {
            if (!condition)
                IsTrue(condition, new[] { errorText });
        }

        // web errors check
        public static void IsTrue(bool condition, object[] param = null)
        {
            if (!condition)
            {       
                string msg = "";
                if (param != null)
                    foreach(string item in param)
                        msg += item ?? "";

                throw new ArgumentException(msg);
            }
        }
    }
}
