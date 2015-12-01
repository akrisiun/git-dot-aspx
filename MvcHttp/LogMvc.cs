using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AiLib.Web
{
    public class LogMvcWriter : TraceListener
    {
        public void Write(string text, params object[] args)
        {
            var writer = LogMvc.Writer == null ? null : LogMvc.Writer();
            if (writer != null)
            {
                writer.Write(String.Format(text, args));
                writer.Close();
            }
        }

        public void WriteException(string cExpression, Exception ex)
        {
            if (LogMvc.Writer != null)
                LogMvc.WriteException(cExpression, ex);
        }

        public override void Write(string message)
        {
            this.Write(text: message);
        }

        public override void WriteLine(string message)
        {
            this.Write(text: message + Environment.NewLine);
        }
    }

    public static class LogMvc
    {
        public static Func<StreamWriter> Writer { get; set; } // { return Log.Writer; } }
        // static void Close() { if (Writer != null) Writer.Close();  }

        public static void Write(string cExpression)
        {
            StreamWriter oWriter = Writer == null ? null : Writer();
            if (oWriter == null)
                return;
            
            oWriter.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                                  + " : " + cExpression);
            oWriter.Close();
        }

        public static void WriteException(string cExpression, Exception ex)
        {
            StreamWriter oWriter = Writer();
            if (oWriter == null)
                return;

            // Write the contents
            oWriter.WriteLine(string.Format("{0} : {1}",
                    DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                    cExpression));
            oWriter.WriteLine(ex.StackTrace);
            oWriter.Flush();
        }
    }

}
