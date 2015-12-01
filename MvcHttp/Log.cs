using System;
using System.Configuration;
using System.Diagnostics;
using System.IO;
#if WEB
using System.Web.Hosting;
#endif

namespace AiLib.Web
{
    public class LogWriter : TraceListener
    {
        public override void Write(object o)
        {
            Write(message: o.ToString());
        }

        public override void WriteLine(object o)
        {
            Write(o.ToString() + Environment.NewLine);
        }

        public override void Write(string message)
        {
            var writer = Log.Writer;
            if (writer != null)
                writer.Write(message);
        }

        public override void WriteLine(string message)
        {
            Write(message: message + Environment.NewLine);
        }
    }

    public static class Log
    {
        static Log()
        {
            Guard.Check(ConfigurationManager.AppSettings.Get("logdir") != null,
                        new[] { @".config AppSettings\logdir error" });

#if WEB
            LogName = HostingEnvironment.ApplicationVirtualPath != null 
                      ? HostingEnvironment.ApplicationVirtualPath + ".log"
                      : "prekes.log";
#else
            LogName = AppDomain.CurrentDomain.FriendlyName + ".log";
#endif
            Tracer = new LogWriter();

            lockObj = new object();
            lockStreamObj = new object();
        }

        public static string LogName { get; set; }
        public static TraceListener Tracer { get; set; }

        public static StreamWriter Writer
        {
            get
            {
                FileStream logStream = StreamLog;
                return logStream == null ? null : new StreamWriter(logStream);
            }
        }

        static FileStream StreamLog
        {
            get
            {
                string cFileName = ConfigurationManager.AppSettings.Get("logdir") + "\\" + LogName;
                if (cFileName == null)
                    return null;

                // Create the file and open it
                FileStream oFs = null;
                lock (lockStreamObj)
                {
                    try
                    {
                        if (System.IO.File.Exists(cFileName))
                        {

                            oFs = new FileStream(cFileName, FileMode.Append, FileAccess.Write);
                        }
                        else
                        {
                            oFs = new FileStream(cFileName, FileMode.CreateNew, FileAccess.ReadWrite);
                        }
                    }
                    catch { }   // because it is being used by another process. 
                }
                return oFs;
            }
        }

        static object lockStreamObj;
        static object lockObj;
        public static void Write(string cExpression)
        {
            lock (lockObj)
            {
                try
                {
                    //Create a writer for the file

                    // file '???.log' because it is being used by another process. 
                    using (StreamWriter oWriter = Writer)
                    {
                        // Write the contents
                        oWriter.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                                    + " : " + cExpression);
                        oWriter.Close();
                    }
                }
                catch { }      // Klaida atidarant klaidu faila...
            }

        }

        public static void WriteException(string cExpression, Exception ex)
        {
            lock (lockObj)
            {
                try
                {
                    using (StreamWriter oWriter = Writer)
                    {
                        // Write the contents
                        oWriter.WriteLine(string.Format("{0} : {1}",
                                DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                                cExpression));
                        oWriter.WriteLine(ex.StackTrace);
                        oWriter.Close();
                    }
                }
                catch  // (Exception exp )
                {      // Klaida atidarant klaidu faila...
                }
            }
        }

    }

}