using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Hosting;
using System.Web.Routing;
using System.Web.SessionState;

namespace AiLib.Web
{
    public static class HttpStatic
    {
        static HttpStatic()
        {
            EmptyNameValueCollection = new NameValueCollection();
        }

        #region fluent Query parse

        // not null
        public static string PathAndQuery
        {
            get
            {
                return Request == null ? string.Empty
                       : (Request.Url.PathAndQuery ?? string.Empty);
            }
        }

        public readonly static NameValueCollection EmptyNameValueCollection;

        public static T Get<T>(this NameValueCollection query, string key, object emptyVal = null) // where T : IConvertible
        {
            T result = default(T);
            if (emptyVal != null)
                result = (T)emptyVal;

            if (query == null || query.Count == 0)
                return result; // null

            var value = query.Get(key);
            if (value == null)
                return result;

            var type = typeof(T);
            if (type == typeof(string) || type.IsClass)
            {
                result = (T)(value as object);
                if (result == null || result is string && (String.IsNullOrEmpty(result as string)))
                    result = (T)emptyVal;    // empty string
            }
            else if (type.IsValueType && !string.IsNullOrWhiteSpace(value))
            {
                var nullType = Nullable.GetUnderlyingType(type);
                if (nullType != null)
                    result = (T)System.Convert.ChangeType(value, nullType);
                else
                    result = (T)System.Convert.ChangeType(value, type);
            }
            return result;
        }

        public static string GetNotNull(this NameValueCollection query, string key)
        {
            if (query == null || query.Count == 0)
                return string.Empty;
            return query.Get(key) ?? String.Empty;
        }

        public static string GetEval(this NameValueCollection query, string key, string emptyVal)
        {
            if (query == null || query.Count == 0)
                return emptyVal ?? string.Empty;
            var val = query.Get(key);
            return (val != null && val.Length > 0) ? val : (emptyVal ?? string.Empty);
        }

        public static NameValueCollection ParseQueryString(this Uri uri)
        {
            return HttpUtility.ParseQueryString(uri.Query);
        }

        public static NameValueCollection ParseQueryString(string query = null)
        {
            if (query == null && (HttpContext.Current == null || HttpContext.Current.Request == null))
                return EmptyNameValueCollection;

            query = query ?? Request.Url.Query;
            return HttpUtility.ParseQueryString(query) ?? EmptyNameValueCollection;
        }

        public static string MapPath(string virtPath) //, string root = "~/")
        {
            if (Server == null)
                return virtPath.Replace("~/", "");

            return PathUtility.MapPath(virtPath);
        }

        #endregion

        public static dynamic WebContext
        {
            [DebuggerStepThrough]
            get
            {
                return new
                {
                    Request = HttpStatic.Request,
                    RequestContext = HttpStatic.RequestContext,
                    Response = HttpStatic.Response,
                    Server = HttpStatic.Server,
                    Session = HttpStatic.Session,
                    User = HttpStatic.User
                };
            }
        }

        #region static HttpContext Status

        public static string PathRoot { get { return VirtualPathUtility.AppendTrailingSlash(HostingEnvironment.ApplicationVirtualPath); } }

        public static string PathSegment(int depth = 0, RequestContext context = null)
        {
            if (depth < 0)
            {
                string query = HttpStatic.Request.Url.Query;
                if (string.IsNullOrWhiteSpace(query))
                    return string.Empty;
                var split = query.Replace("?", "").Split(new[] { '&' });
                if (split.Length > 0)
                    return split[0] ?? String.Empty;
                return string.Empty;
            }

            context = context ?? RequestContext;
            if (context == null)
                return PathRoot;

            RouteValueDictionary values = context.RouteData.Values;
            if (values.Count > 0 && values.Count >= depth)
                return System.Linq.Enumerable.ElementAtOrDefault(values, depth).Value.ToString();

            var path = HostingEnvironment.ApplicationVirtualPath;
            if (!string.IsNullOrWhiteSpace(path) && path != "/")
                return System.IO.Path.GetFileNameWithoutExtension(path);
            return string.Empty;
        }

        public static HttpContext Current { get { return HttpContext.Current; } }

        public static HttpRequest Request
        {
            [DebuggerStepThrough]
            get { return HttpContext.Current == null ? null : HttpContext.Current.Request; }
        }

        public static RequestContext RequestContext
        {
            [DebuggerStepThrough]
            get { return HttpContext.Current == null ? null : HttpContext.Current.Request.RequestContext; }
        }

        public static HttpResponse Response
        {
            [DebuggerStepThrough]
            get { return HttpContext.Current == null ? null : HttpContext.Current.Response; }
        }


        public static HttpServerUtility Server
        {
            [DebuggerStepThrough]
            get { return HttpContext.Current == null ? null : HttpContext.Current.Server; }
        }


        public static HttpSessionState Session
        {
            [DebuggerStepThrough]
            get { return HttpContext.Current == null ? null : HttpContext.Current.Session; }
        }

        // Gets or sets security information for the current HTTP request.
        // public static IUserPrincipal UserClaim { get { return User as IUserPrincipal; } }

        public static IPrincipal User
        {
            [DebuggerStepThrough]
            get
            { return HttpContext.Current == null ? null : HttpContext.Current.User; }
            set
            {
                if (HttpContext.Current != null)
                    HttpContext.Current.User = value;
            }
        }

        #endregion

        public static dynamic Hosting
        {
            [DebuggerStepThrough]
            get
            {
                var obj = // ExpandoConvert.Muttable(
                          new
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
                return obj;
            }
        }

    }
}
