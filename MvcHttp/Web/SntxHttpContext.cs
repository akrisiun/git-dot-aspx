using System;
using System.IO;
using System.Web;
using System.Web.SessionState;

namespace AiLib.Web
{
    // Runtime and Test units context
    // HttpContext.Current -> HttpContextWrapper
    public class SntxHttpContext
    {
        static SntxHttpContext()
        {
            Instance = new SntxHttpContext(null);
            if (HttpContext.Current != null)
            {
                // HttpContextWrapper : HttpContextBase [TypeForwardedFrom("System.Web.Abstractions, Version=3.5.0.0, Culture=Neutral, PublicKeyToken=31bf3856ad364e35")]
                Instance.wrapper = new HttpContextWrapper(HttpContext.Current);
            }
        }
        public static SntxHttpContext Test(HttpContextBase wrapper)
        {
            return new SntxHttpContext(wrapper);
        }

        public static SntxHttpContext Instance;
        public static HttpContextBase Wrapper { get { return Instance.wrapper; } }

        #region Wrapper static
        public static HttpRequestBase Request
        {
            get { return Instance == null || Instance.wrapper == null ? null : Instance.wrapper.Request; }
        }

        public static HttpResponseBase Response
        {
            get { return Instance == null || Instance.wrapper == null ? null : Instance.wrapper.Response; }
        }

        public static HttpServerUtilityBase Server
        {
            get { return Instance == null || Instance.wrapper == null ? null : Instance.wrapper.Server; }
        }

        public static HttpSessionStateBase Session
        {
            get { return Instance == null || Instance.wrapper == null ? null : Instance.wrapper.Session; }
        }

        #endregion

        public static HttpApplicationStateBase AppState
        {
            get
            {
                return Instance == null || Instance.wrapper == null ? null : Instance.wrapper.Application;
            }
        }
        public static HttpApplication Application
        {
            get
            {
                return Instance == null || Instance.wrapper == null ? null : Instance.wrapper.ApplicationInstance;
            }
        }

        protected HttpContextBase wrapper;
        public SntxHttpContext(HttpContextBase wrapper)
        {
            this.wrapper = wrapper;
        }

        // private readonly IHttpContextFactory factory;
        // public WebAppContext(IHttpContextFactory factory)

        public string[] ParseURL()
        {
            var fragments = new string[3];

            var url = wrapper.Request.Url.Segments;
            return fragments;
        }
    }

    public class TestHttpContext : HttpContextBase, IHttpContextBase
    {
        protected TestHttpContext()
            : base()
        {
            //  When overridden in a derived class, gets an array of errors (if any) that
            //     accumulated when an HTTP request was being processed.

            TestRequest = null;
            TestResponse = null;
            TestServer = new TestHttpServer();
            TestSession = null;
            trace = new TraceContext(HttpContext.Current);
        }

        public static TestHttpContext Create() { return new TestHttpContext(); }

        public override HttpRequestBase Request { get { return TestRequest ?? SntxHttpContext.Request; } }
        public override HttpResponseBase Response { get { return TestResponse ?? SntxHttpContext.Response; } }
        public override HttpServerUtilityBase Server { get { return TestServer ?? SntxHttpContext.Server; } }
        public override HttpSessionStateBase Session { get { return TestSession ?? SntxHttpContext.Session; } }
        public override TraceContext Trace { get { return trace; } }

        protected TraceContext trace;

        public virtual HttpRequestBase TestRequest { get; set; }
        public virtual HttpResponseBase TestResponse { get; set; }
        public virtual HttpServerUtilityBase TestServer { get; set; }
        public virtual HttpSessionStateBase TestSession { get; set; }

        public IHttpContextBase Context { get; set; }

    }

    public class TestHttpServer : HttpServerUtilityBase
    {
        public string BaseUrl { get; set; }

        public TestHttpServer()
        {
            BaseUrl = AppDomain.CurrentDomain.BaseDirectory;
        }

        public override string MapPath(string path)
        {
            // return base.MapPath(path);
            if (path.Contains("~/"))
                return Path.GetFullPath(path.Replace("~/", BaseUrl));
            if (path.Contains("~"))
                return path.Replace("~", BaseUrl);
            if (!path.Contains(":"))
                return Path.Combine(BaseUrl, path);
            return path;
        }
    
    }

    public interface IHttpContextFactory
    {
        IHttpContextBase Context { get; }  // like HttpContext 
    }

    // [TypeForwardedFrom("System.Web.Abstractions, Version=3.5.0.0, Culture=Neutral, PublicKeyToken=31bf3856ad364e35")]
    // public class HttpContextWrapper : HttpContextBase

    public interface IHttpContextBase
    {
        HttpRequestBase Request { get; }
        HttpResponseBase Response { get; }
        HttpServerUtilityBase Server { get; }
        HttpSessionStateBase Session { get; }
    }

    //mockContext.Request.Stub(x => x.Url).Return(new Uri("http://localhost/subpath"));
    //factory.Stub(x => x.Context).Return(mockContext.Context);
    //var context = new WebAppContext(factory);

    public class WebContextFactory : IHttpContextFactory
    {
        public IHttpContextBase Context { get { return new SystemWebContextFactory(new HttpContextWrapper(HttpContext.Current)); } }

        public class SystemWebContextFactory : IHttpContextBase
        {
            public SystemWebContextFactory(HttpContextWrapper context) { Context = context; }
            protected HttpContextWrapper Context;

            public HttpRequestBase Request { get { return Context.Request; } }
            public HttpResponseBase Response { get { return Context.Response; } }
            public HttpServerUtilityBase Server { get { return Context.Server; } }
            public HttpSessionStateBase Session { get { return Context.Session; } }
        }
    }

}
