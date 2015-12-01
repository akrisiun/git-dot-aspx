using AiLib.Web;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Hosting;
using System.Xml.Linq;
using System.Xml.XPath;

namespace AiLib
{
    /// <summary>
    /// Translations.xml reader
    /// </summary>
    public static class Trans
    {
        public static string Tr(this string key, string lang)
        {
            if (string.IsNullOrWhiteSpace(lang))
                lang = Lang;

            lang = lang.ToLower();
            if (string.IsNullOrWhiteSpace(key) || doc == null || lang.Length != 2)
                return key;

            XElement node = GetNode(key);
            var ret = key;
            if (node == null || !node.HasElements)
                return ret;
            try
            {
                if (node.Element(lang) != null)
                    ret = node.Element(lang).Value;
            }
            catch
            {
                ret = "[lang=" + lang ?? "??" + "]" + key;
            }
            return ret;
        }

        public static string Tr(this string key)
        {
            if (string.IsNullOrWhiteSpace(key) || doc == null)
                return key;
            XElement node = GetNode(key);

            var ret = key;
            if (node == null || !node.HasElements)
                return ret;
            try
            {
                if (node.Element(Lang) != null)
                    ret = node.Element(Lang).Value;
                else if (TrLang != null && node.Element(TrLang) != null)
                    ret = node.Element(TrLang).Value;
                // else ret = key;
            }
            catch
            {
                ret = "[lang=" + Lang ?? "??" + "]" + key;
            }

            return ret;
        }

        private static XElement GetNode(string key)
        {
            var lastTime = LastWrite.GetTimeStamp(LastFile.File, LastFile.TimeUtc);
            if (doc == null || lastTime > LastFile.TimeUtc)
            {
                LoadXml(); // reload
            }

            var list = doc.Root.Elements();
            XElement node = list.Where<XElement>(
                phrase => phrase.Name == "phrase" && phrase.Elements("key").Any()
                          && phrase.Element("key").Value == key).FirstOrDefault();

            if (node == null && !AiLib.Web.Segment.Instance.isRelease)
            {
                var el = new XElement("phrase", new XElement("key", key));
                el.Add(new XElement("lt", key));
                el.Add(new XElement("en", key));
                doc.Root.Add(el);
                lock (lockObj)
                {
                    doc.Save(TransFile);
                }
            }

            return node ?? new XElement("phrase");
        }

        private static object lockObj;

        public static string TransFile
        {
            get { return LastFile.File; }
            set
            {
               LastFile = LastFile.SetFile(value);
            }
        }

        private static LastWrite LastFile { get; set; }

        public static string Lang { get; set; }
        public static string TrLang { get; set; }

        public static XDocument doc { get; set; }

        static Trans()
        {
            LastFile = new LastWrite { File = null };
            Lang = "lt";    // default language
            TrLang =        // second language (posible is null)
                ConfigurationManager.AppSettings.Get("tr.lang");
            if (String.IsNullOrWhiteSpace(TrLang))
                TrLang = "en";
            else
                TrLang = TrLang.ToLowerInvariant();            

            TransFile = "Translate.xml";    // default file name
            lockObj = "transLock";
        }

        public static void LoadXml()
        {
            var path = HostingEnvironment.ApplicationPhysicalPath ?? System.AppDomain.CurrentDomain.BaseDirectory;
            var file = Path.GetFileName(TransFile);
            var filePath = TransFile == null || string.IsNullOrWhiteSpace(file) ? Path.Combine(path, "Translate.xml")
                         : Path.Combine(path, TransFile);

            string url = "";
            if (HttpStatic.Current != null)
                url = " " + HttpStatic.Current.Request.Url.OriginalString.SubStringSafe(0, 50)
                 + " IP=" + HttpStatic.Current.Request.UserHostAddress;
            AiLib.Web.Log.Write("Trans.LoadXml " + filePath + url);
            if (!File.Exists(filePath))
            {
                AiLib.Web.Log.Write("no file " + filePath);
                return;
            }

            doc = XDocument.Load(filePath);
            if (doc == null)
                return;
            TransFile = filePath;

            Lang = ConfigurationManager.AppSettings.Get("dir.lang");
            //  "dir.lang" value = "LT" />
            if (string.IsNullOrWhiteSpace(Lang))
            {
                if (doc.Root.Attributes("lang").Any())
                    Lang = doc.Root.Attribute("lang").Value;
                else
                    Lang = "en"; // new default
            }
            Lang = Lang.Substring(0, 2).ToLower();
        }

        public struct LastWrite
        {
            public string File { get; set; }
            public DateTime TimeUtc { get; set; }

            public LastWrite SetFile(string value)
            {
                File = value;
                File = File.Replace(@"\\", @"\");
                TimeUtc = GetTimeStamp(value, TimeUtc);
                return this;
            }

            public static DateTime GetTimeStamp(string filePath, DateTime fallback)
            {
                DateTime timestamp = default(DateTime);
                try
                {
                    timestamp = System.IO.File.GetLastWriteTimeUtc(filePath);
                    if (timestamp.Year == 1601)
                    {
                        // 1601 is returned if GetLastWriteTimeUtc for some reason cannot read the timestamp.
                        timestamp = fallback;
                    }
                }
                catch (Exception) { ;}
                return timestamp;
            }
        }
    }
}
