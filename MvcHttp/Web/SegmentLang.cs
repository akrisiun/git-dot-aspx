
using AiLib.Mvc;
using AiLib.Render;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Hosting;
using System.Web.Routing;
using System.IO;
using System.Web.Mvc;
using AiLib.MvcHttp;
using System.Xml.Linq;

namespace AiLib.Web
{
    public class SegmentLang : Segment, ISegment
    {
        public new static SegmentLang Instance;
        static SegmentLang()
        {
            var firmID = ConfigurationManager.AppSettings.Get("dir.firmid") ?? "SA";
            var lang = ConfigurationManager.AppSettings.Get("dir.lang") ?? "LT";
            Instance = new SegmentLang() { FirmID = firmID, Lang = lang };
            Segment.Instance = Instance;
            Instance.afterFirst = false;
            Instance.PhysicalPath = HostingEnvironment.ApplicationPhysicalPath;
        }

        public Exception LastException { get; set; }

        #region Properties
        public new string FirmID
        {
            get { return base.FirmID; }
            set
            {
                if (base.FirmID != value)
                    base.FirmID = value;
            }
        }

        // interface language : LT or EN
        public new string Lang
        {
            get { return Trans.Lang.ToUpper(); }
            set
            {
                if (value == null)
                    return;
                if (Trans.Lang != value.ToLower())
                    Trans.Lang = value.ToLower();
                base.Lang = value;
            }
        }

        #endregion

        #region Session, Request

        public string SiteAlias { get; set; }
        public string VirtualPath { get; set; }
        public string PhysicalPath { get; set; }
        public string IP { get { return HttpStatic.Current == null ? string.Empty : HttpStatic.Request.UserHostAddress.SubStringSafe(0, 20); } }

        [Caller(typeof(HttpHandler), "Init")]
        public virtual void InitHandler(HttpApplication context, IHttpModule module = null)
        {
            Guard.CheckArgumentNull(context);
            VirtualPath = HostingEnvironment.ApplicationVirtualPath;
            if (VirtualPath == null)
                return;     // Test unit
            SiteAlias = HostingEnvironment.SiteName.Replace(" ", "");
            PhysicalPath = HostingEnvironment.ApplicationPhysicalPath;

            // The first System.Exception for the current HTTP request/response process;
            Exception appError = HttpContext.Current == null ? null : HttpContext.Current.Error;
            if (appError != null && CustomError != null)
            {
                CustomError(appError);
            }

            if (module != null) // && !isModuleInit)
            {
                context.PostAuthenticateRequest += new EventHandler(OnPostAuthenticate);
                // pipeline error NullReferenceException PipelineModuleStepContainer.GetStepArray
            }
        }

        public static void OnPostAuthenticate(object sender, EventArgs args)
        {
            var ctx = HttpContext.Current;
            if (ctx == null || ctx.Session == null
                || ctx.User == null || !ctx.User.Identity.IsAuthenticated)
                return;

            Instance.CreateSession();
            //var userClaim = Claim.GetPrincipal(ctx);
        }

        public virtual bool OnFirstRequest(object sender, EventArgs args)
        {
            if (Instance.LastException != null)
            {
                // Startup error
                HttpHandler.AppError(Instance.LastException);
                return false;
            }

            string status = "";
            try
            {
                var req = HttpStatic.Request;
                if (req != null)
                {
                    status = " ip = " + req.UserHostAddress;
                    var browser = req.Browser.Browser + req.Browser.Version + "/" + req.Browser.Platform;
                    status += " " + browser + " ";
                    // + " clr=" + req.Browser.ClrVersion.ToString()

                    var langList = req.UserLanguages;
                    if (langList.Any())
                    {
                        var numer = langList.GetEnumerator();
                        status += " lang=";
                        for (int i = 0; i < 2 && numer.MoveNext(); i++)
                            status += numer.Current + ";";
                    }
                    status += " url=" + (req.Url.ToString() ?? string.Empty);
                }
            }
            catch (Exception) { ; }
            if (status.Length > 90)
                status = status.Substring(0, 90);

            Log.Write("AppStart Init " + status);
            // AiLib.Web.Reflection.WebStringConvert.PadRight  + AiLib.Web.HttpStatic  req.Url.ToString()

            return true;
        }

        public Action<Exception> CustomError { get; set; }

        public override bool IgnoreError(Exception ex, HttpRequest req)
        {
            if (CustomError == null)
                return false;

            CustomError(ex);
            return true;
        }

        public virtual void CreateSession()
        {
            var ctx = HttpContext.Current;
            if (ctx == null || ctx.Session == null)
                return;

            SaveSession();
        }

        public virtual void ReadRequest()
        {
            var ctx = HttpContext.Current;
            if (ctx == null || ctx.Request == null || ctx.Request.Params.Count == 0)
                return;

            var user = ctx.User;
            string newLang = ctx.Request.Params["lang"];
            if (newLang == "lt" || newLang == "en")
                newLang = newLang.Substring(0, 2).ToUpper();

            if (!string.IsNullOrWhiteSpace(newLang))
            {
                Instance.Lang = newLang;
                if (ctx.Session != null)
                    Instance.SaveSession();
            }
        }

        public virtual void ReadSession()
        {
            Instance.FirmID = Instance.FirmID ?? ConfigurationManager.AppSettings.Get("dir.firmid");
            var lang = Instance.Lang ?? ConfigurationManager.AppSettings.Get("dir.lang");

            var ctx = HttpContext.Current;
            if (ctx != null && ctx.Session != null)
            {
                lang = ctx.Session["lang"] as string ?? lang;   // Language
                //var principal = Claim.GetPrincipal(ctx);
                //ctx.User = principal ?? ctx.User;
            }
            if (Instance.Lang != lang)
                Instance.Lang = lang;


        }

        public virtual void SaveSession()
        {
            var ctx = HttpContext.Current;
            ctx.Session["lang"] = Instance.Lang;
        }

        #endregion

        public string DataLang { get; set; }    // data language: LT, LV, EE

        #region Request methods

        private bool afterFirst;

        [Caller(typeof(HttpHandler))]
        public override void RequestPrepare(HttpRequest request)
        {
            if (!afterFirst)
            {
                afterFirst = true;

                if (System.Web.Hosting.HostingEnvironment.InitializationException != null
                    && Instance.LastException == null)
                    Instance.LastException = System.Web.Hosting.HostingEnvironment.InitializationException;

                if (!OnFirstRequest(request, EventArgs.Empty))
                    return;
            }

            ReadSession();
            ReadRequest();
            base.RequestPrepare(request);

            if (IsRedirectAspx)
                RedirectAspx();
        }

        [Caller(typeof(RoutingModule))]
        public virtual void ResolveRequest(HttpContextBase context, RouteData routeData = null)
        {
            this.UrlContext = context;
            this.UrlRouteData = routeData;

            if (routeData != null && context.Request.Path.EndsWith("netversion"))
            {
                // special IRouteHandler that tells the routing module to stop processing
                routeData.RouteHandler = new StopRoutingHandler();
                NetVersionController.Output(context.Response);
            }
        }

        #endregion

        public HttpContextBase UrlContext { get; set; }
        public RouteData UrlRouteData { get; set; }

        public virtual bool IsRedirectAspx
        {
            get
            {
                var cfg = ConfigurationManager.AppSettings["redirect.aspx"];
                return string.IsNullOrWhiteSpace(cfg) || !cfg.Equals("0");
            }
        }

        public virtual void RedirectAspx()
        {
            // Redirect
            var ctx = HttpContext.Current;
            if (ctx == null || ctx.Request.Url == null)
                return;

            var request = ctx.Request;
            if (string.IsNullOrWhiteSpace(request.Path)
                || request.Url.LocalPath.Length == 0
                || !request.Path.Contains(".aspx"))
                return;

            string path = Enumerable.LastOrDefault<string>(request.Url.Segments);
            string fileAspx = this.PhysicalPath + request.Url.LocalPath;
            // ctx.Request.Url.AbsolutePath.Replace("/", "\\")
            fileAspx = fileAspx
                     .Replace("/", "\\")
                     .Replace("~\\", "\\")
                     .Replace("\\\\", "\\");

            if (System.IO.File.Exists(fileAspx))
            {
                AiLib.Guard.Log.Write("ASPX: " + fileAspx);

                // <system.web><httpHandlers>
                //  <add path="*.aspx" type="System.Web.UI.PageHandlerFactory" validate="true" verb="*"/>
                return;
            }

            var url = request.Url.ToString();
            //if (url.Contains(".aspx"))
            //{
            //    // var virtualPath = System.Web.Hosting.HostingEnvironment.ApplicationVirtualPath; // == "/"
            //    string map = System.Web.Hosting.HostingEnvironment.ApplicationPhysicalPath + ctx.Request.Url.AbsolutePath.Replace("/", "\\");
            //    map = map.Replace("~\\", "\\")
            //             .Replace("\\\\", "\\");
            //    if (File.Exists(map))
            //        return;
            //}

            if (url.Contains("default.aspx"))
                url = url.Replace("default.aspx", "");
            else
                url = url.Replace(".aspx", "");

            ctx.Response.Redirect(url);
        }

        #region RequestContext

        public virtual string PathSegment(int depth = 0)
        {
            var segment = AiLib.Web.HttpStatic.PathSegment(depth, _context);
            return segment;
        }

        public virtual string PathAndQuery()
        {
            return AiLib.Web.HttpStatic.PathAndQuery;
        }

        protected RequestContext _context;
        public RequestContext RequestContext
        {
            get { return _context ?? HttpStatic.RequestContext; }
            set { _context = value; }
        }

        public static dynamic WebContext
        {
            get
            {
                return HttpStatic.WebContext;

            }
        }

        public static dynamic Hosting
        {
            get
            {
                return new
                {
                    Environment = new
                    {
                        // object that contains information about the application host.
                        ApplicationHost = HostingEnvironment.ApplicationHost,
                        // The unique identifier of the application.
                        ApplicationID = HostingEnvironment.ApplicationID as string,
                        // physical path on disk to the application's directory.
                        ApplicationPhysicalPath = HostingEnvironment.ApplicationPhysicalPath as string,
                        ApplicationVirtualPath = HostingEnvironment.ApplicationVirtualPath as string,
                        // thrown during initialization of the System.Web.Hosting.HostingEnvironment
                        InitializationException = HostingEnvironment.InitializationException,
                        IsDevelopmentEnvironment = HostingEnvironment.IsDevelopmentEnvironment as bool?,
                        // application domain is hosted by an System.Web.Hosting.ApplicationManager
                        IsHosted = HostingEnvironment.IsHosted as bool?,
                        // name of the site.
                        SiteName = HostingEnvironment.SiteName as string
                    },
                    AppDomain = System.AppDomain.CurrentDomain
                };
            }
        }

        #endregion

        #region PageTop menu render

        public virtual void PageTopCss(UrlHelper url, TextWriter output)
        {
            //var rootCss = "~/css/";
            //MvcBundle.WriteCssHref(output, url, "pagetopx.css", rootCss);
            //output.Write(Environment.NewLine);

            //MvcBundle.WriteCssHref(output, url, "detailx.css", rootCss);
            //output.Write(Environment.NewLine);
        }

        public virtual XElement PageTopXml
        {
            get
            {
                var root = new XElement("Root",
                    new XAttribute("LINK", PathSegment(0)),
                    new XAttribute("FIRMID", FirmID),
                    new XAttribute("LANG", Lang),
                    new XAttribute("USERID", UserID),
                    new XAttribute("PathRoot", HttpStatic.PathRoot),
                    new XAttribute("PathAndQuery", HttpStatic.PathAndQuery)
                );
                return root;
            }
        }

        public virtual string PageTopRender(TextWriter output, XElement xml = null, string xlst = @"../pagemenux.xslt",  string link = null)
        {
            xml = xml ?? PageTopXml;
            if (link != null)
                xml.Attribute("LINK").SetValue(link);

            var menuXslt = SegmentLang.Instance.PhysicalPath + xlst;
            if (!File.Exists(menuXslt))
                menuXslt = SegmentLang.Instance.PhysicalPath + @"../pagemenux.xslt";

            var render = new PageTopRender { Xml = xml, Xslt = menuXslt, Link = link };
            render.Render(output);

            return String.Empty;
        }

        public virtual void LayoutContent(TextWriter output, object title = null,
            string id = "content", Action inside = null)
        {
            output.Write(@"<div id=""content"">

            ");

            if (title is XElement)
                output.Write(title as XElement);
            else if (!string.IsNullOrWhiteSpace(title as string))
                output.Write(new XElement("h4", title));

            if (inside != null)
                inside();

            output.Write(@"
            <div>
            ");
        }
        #endregion
    }

    public class PageTopRender : IRenderBase
    {
        public string Xslt { get; set; }
        public XElement Xml { get; set; }
        public string Link { get; set; }

        public virtual string Render(TextWriter writer)
        {
            try
            {
                var xslt = XElement.Load(Xslt);
                var info = new RequestInfo(null, HttpStatic.Request);
                XPathWeb.XTransformToWriter(Xml, writer, xslt, info);
            }
            catch (Exception ex)
            {
                SegmentLang.Instance.LastException = ex;
                SegmentLang.Instance.IgnoreError(ex, HttpStatic.Request);
            }
            return string.Empty;
        }
    }

}
