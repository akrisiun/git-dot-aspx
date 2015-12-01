using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
#if WEB
using System.Web;
#endif

namespace AiLib.Render
{
    public interface IRenderXsltBase
    {
        string ID { get; }
        string Xslt { get; }
        int Timeout { get; }

        string GetParam(string key);
        string GetParamSql(string key);

#if WEB
        HttpContext Context { get; }
#endif

    }

    public interface IRenderXslt : IRenderXsltBase
    {

        Dictionary<string, string> SqlParamDict { get; set; }

        void SetParam(string key, string value);
        int UserID { get; }

        System.Collections.Specialized.NameValueCollection QueryString { get; }
        // void TraceWrite(string category, string message);
        void TraceWrite(string category, params object[] parm);

        /// <summary>
        /// Maximum PrmList length for Request query
        /// </summary>
        string VarcharPrm { get; }

        /// <summary>
        /// Maximum PrmAdd length
        /// </summary>
        string VarcharAdd { get; }

        /// <summary>
        /// dbo Sql procedure name
        /// </summary>
        string SqlProc { get; }

        /// <summary>
        /// Default=SNTXCC
        /// </summary>
        string SqlDb { get; }
        string LastErrorStr { get; set; }
        Exception LastError { get; set; }
    }

    public interface ILog
    {
        void Write(string text);
    }

}
