using System;
using System.Collections.Generic;
using System.Diagnostics;
#if WEB
using System.Web;
// using AiLib.Web.Render;
#else
    using AiLib.IISHost;
    using AiLib.IIS;
#endif

namespace AiLib.Render
{

    public class VoidLog : ILog
    {
        static VoidLog()
        {
            Instance = new VoidLog();
        }
        public static VoidLog Instance;

        public void Write(string text)
        {
        }
    }

    #if !WEB

    public abstract class WebControlBase
    {
        public string ID { get; set; }
        public abstract void TraceWrite(string format, params object[] parm);
    }

    #else
        public abstract class WebControlBase : System.Web.UI.WebControls.WebControl
        {
            // public string ID { get { return base.ID; set; }
            public abstract void TraceWrite(string format, params object[] parm);
        }
    #endif

    public abstract class RenderXsltBase : WebControlBase
    {
        private string xsltFile = "";
        public String Xslt             // Xlst file
        {
            set
            {
                TraceWrite("sqlxml", "set Xslt: value=" + value);
                xsltFile = value;
            }
            get { return xsltFile; }
        }

        public virtual ILog Log { get { return VoidLog.Instance; } }


        public string SqlProc { get { return null; } }
        public string sqlDb = "SNTXCC";
        public string SqlDb
        {
            get { return sqlDb; }
            set { sqlDb = value; }
        }

        protected string listParam = "";
        protected string listParamNum = "";
        protected string prmAdd = "";
        protected string prmApp = "";
        protected string formParam = "";
        protected string formParamNum = "";

        public String PrmList
        {
            set
            {
                TraceWrite("sqlxml=" + ID, "set PrmList=" + value);
                listParam = value;
            }
            get { return listParam; }
        }
        public String PrmListNum
        {
            set
            {
                TraceWrite("sqlxml=" + ID, "set PrmListNum=" + value);
                listParamNum = value;
            }
            get { return listParamNum; }
        }

        public String PrmAdd
        {
            set
            {
                string prm = value, val = "";
                int pos = prm.IndexOf("=");

                if (pos > 0)
                {
                    val = prm.Substring(pos + 1);
                    prm = prm.Substring(0, pos);

                    TraceWrite("sqlxml id=" + ID, "PrmAdd: " + value + " " + prm + ", " + val);
                    prmAdd = prm + "=" + val;
                }

            }
            get { return prmAdd; }
        }

        public string PrmApp
        {
            get { return prmAdd; }
            set { prmAdd = value; }

        }

        public String FormList
        {
            set
            {
                TraceWrite("sqlxml=" + ID, "set FormList=" + value);
                formParam = value;
            }
            get { return formParam; }
        }
        public String FormListNum
        {
            set
            {
                TraceWrite("sqlxml=" + ID, "set FormListNum=" + value);
                formParamNum = value;
            }
            get { return formParamNum; }
        }

        protected int _timeout = 5000;              // default 5 sek. (xml response timeout)
        public int Timeout
        {
            set
            {
                TraceWrite("sqlxml", "set Timeout: value=" + value);
                _timeout = value;
            }
            get { return _timeout; }
        }

        protected string varcharPrm = "100";
        protected string varcharAdd = "20";

        /// <summary>
        /// Maximum PrmList length for Request query
        /// </summary>
        public string VarcharPrm
        {
            get { return varcharPrm; }
            set
            {
                Assert.IsTrue(Int32.Parse(value) > 0);
                varcharPrm = value;
            }
        }

        /// <summary>
        /// Maximum PrmAdd length
        /// </summary>
        public string VarcharAdd
        {
            get { return varcharAdd; }
            set
            {
                Assert.IsTrue(Int32.Parse(value) > 0);
                varcharAdd = value;
            }
        }

        protected int isDebug = 0;
        public string IsDebug { set { isDebug = Int32.Parse(value); } }

        public Dictionary<string, string> keyParam = null;

        public void SetParam(string key, object value)
        {
            if (value != null)
                SetParam(key, value.ToString());
        }

        public virtual void SetParam(string key, string value)
        {
            if (keyParam == null) keyParam = new Dictionary<string, string>();
            if (keyParam.ContainsKey(key))
                keyParam[key] = value;
            else keyParam.Add(key, value);
        }
        public string GetParam(string key)
        {
            if (keyParam == null || !keyParam.ContainsKey(key)) return String.Empty;
            return keyParam[key];
        }

        public int UserID { get { return 0; } }

        public string LastErrorStr { get; set; }

    }

}