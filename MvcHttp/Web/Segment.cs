using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

#if WEB
using System.Web;
using System.Configuration;
using AiLib.Render;
#else
using AiLib.IIS;
#endif

namespace AiLib.Web
{
    public interface ISegment
    {
        string Lang { get; set; }
        string FirmID { get; set; }

        string SiteDir { get; set; }
        string SiteUrl { get; }
        string Site { get; }        // site code = PRK
        int UserID { get; set; }
        bool isRelease { get; }
        bool IgnoreError(Exception ex, HttpRequest req);

        void AppInit();
        void RequestPrepare(HttpRequest Request);
    }

    public class Segment : ISegment
    {
        public static ISegment Instance;

        static Segment()
        {
            Instance = new Segment();
            (Instance as Segment).isStarted = false;
        }

        public static Segment Create()
        {
            Instance = new Segment();
            return Instance as Segment;
        }

        public virtual void AppInit()
        {
            // var app = HttpContext.Current.ApplicationInstance;
            this.SiteDir = System.Web.Hosting.HostingEnvironment.ApplicationPhysicalPath;
        }

        private bool isStarted;

        public virtual void RequestPrepare(HttpRequest Request)
        {
            if (!isStarted)
                Log.Write("Request=" + Request.Url
                         + " ip=" + (Request.UserHostAddress ?? "")
#if WEB
                         + (Request.UserHostAddress != Request.UserHostName ? 
                         " " + Request.UserHostName ?? "" : "")
#endif
);
            isStarted = true;
            var lang = Request.QueryString.Get("lang");
            if (!string.IsNullOrWhiteSpace(lang) && lang.Length == 2
                && Trans.Lang != lang.ToLower())
                Trans.Lang = lang.ToLower();
        }

        public virtual bool IgnoreError(Exception ex, HttpRequest req)
        {
            return false;
        }

        public string Lang { get; set; }
        public string FirmID { get; set; }

        public virtual string Site { get { return "PRK"; } set { } }    // prekes web default
        public string SiteDir { get; set; }
        public string SiteUrl { get; set; }
        public int UserID { get; set; }

        public virtual bool isRelease
        {
            get {
#if WEB
                if (HttpContext.Current == null) return true;
                var req = HttpContext.Current.Request;
                return req == null || !req.Url.Host.Contains("local");
#else
                return false;
#endif
            }
        }
    }

    //IHttpModuleSubscriber
    //{
    //    bool RequestRecipient(HttpRequest request, RequestEvent requestEvent);
    //    void ProcessRequest();

}
