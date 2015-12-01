using RazorGenerator.Mvc;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace AiLib.Mvc
{
    public static class StartMvc
    {
        static bool isStarted = false;
        public static TraceListener Log { get; set; }
        public static Func<StreamWriter> Writer { get; set; }

        public static void Start(TraceListener logger, Func<StreamWriter> writer)
        {
            // Debugger.Break();
            Log = logger ?? Log;
            Writer = writer ?? Writer;

            Trace.Assert(Log != null);
            Log.Write("StartMvc: Start");
            isStarted = true;
        }

        public static void PostStart()
        {
            //RouteConfig.RegisterRoutes();
            //FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            //BundleConfig.RegisterBundles(BundleTable.Bundles);
        }

        // <add key="dir.WebBin" value="c:\Work_exe\prekesWeb\bin" />
        public static string WebBin { get; set; }

        public static void Init(Assembly asm = null)
        {
            if (Log == null)
                return;         // fatal error

            Log.Write("AppStart Init");
            if (!isStarted)
                Start(StartMvc.Log, StartMvc.Writer);

            if (asm == null)
                asm = System.Reflection.Assembly.GetCallingAssembly();
            PostApp(asm);
        }

        public static void ReflectCheck()
        {
            MvcResolve.ReflectCheck("WebGrease.dll", "WebGrease.Css.CssParser");
            MvcResolve.ReflectCheck("System.Web.Optimization.dll", "System.Web.Optimization.CssMinify");
            MvcResolve.ReflectCheck("Microsoft.Web.Infrastructure.dll", "Microsoft.Web.Infrastructure.DynamicValidationHelper");
        }

        public static void IgnoreRoutes()
        {
            RouteCollection routes = RouteTable.Routes;
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");
            routes.IgnoreRoute("{resource}.ashx/{*pathInfo}");

            routes.IgnoreRoute("{*allaspx}", new { allaspx = @".*\.aspx(/.*)?" });
            routes.IgnoreRoute("{*allico}", new { allico = @"(.*/)?.ico(/.*)?" });
            routes.IgnoreRoute("{*allpng}", new { allpng = @"(.*/)?.png(/.*)?" });
            routes.IgnoreRoute("{*alljpg}", new { alljpg = @"(.*/)?.jpg(/.*)?" });
            routes.IgnoreRoute("{*allsvg}", new { allsvg = @"(.*/)?.svg(/.*)?" });
            routes.IgnoreRoute("{*allwoff}", new { allwoff = @"(.*/)?.woff(/.*)?" });
            routes.IgnoreRoute("{*allttf}", new { allttf = @"(.*/)?.ttf(/.*)?" });

            routes.IgnoreRoute("{*staticfile}",
                // text + images + documents + fonts
                // |png|gif|jpg|ico|eot|svg|ttf|woff
                new { staticfile = @".*\.(css|js|xls|xlsx|doc|docx|pdf)(/.*)?" });
        }

        public static void PostApp(Assembly asm)
        {
            var binDir = ConfigurationManager.AppSettings.Get("Dir.WebBin");
            if (string.IsNullOrWhiteSpace(binDir))
                MvcResolve.Init(binDir);

            // RazorGenearator.Mvc https://github.com/RazorGenerator/RazorGenerator
            PrecompiledMvcEngine.MvcEngine(asm);
        }

        public static void Error(Exception ex = null)
        {
            Exception error = ex;
            HttpServerUtility Server = null;
            try
            {
                var server = HttpContext.Current == null ? null : HttpContext.Current.Server;
                error = ex ?? (server == null ? null : server.GetLastError());
                Server = server;
            }
            catch { ;}

            string strError = error == null ? "-" : error.ToString();

            if (Log != null)
                Log.Write(" Error details " + strError
                          + (error == null || error.StackTrace == null ? String.Empty
                            : "\n" + error.StackTrace.ToString())); // .GetBaseException().StackTrace.ToString());
            if (Server == null)
            {
                Debugger.Break();
                return;
            }
            if (error == null || error.InnerException != null) Debugger.Break();

            var Request = HttpContext.Current.Request;
            var Response = HttpContext.Current.Response;

            Log.Write("Global Application_Error: url=" + Request.Url
                      + ": \n ip=" + Request.UserHostAddress + " refer=" + Request.UrlReferrer);

            if (strError.IndexOf(".XPathException") > 0
                || strError.IndexOf(".HttpCompileException") > 0
                || strError.IndexOf("Could not load") > 0)
            {
                Server.ClearError();
                return;
            }

            Trace.Write("Error " + error.ToString());

            Response.Write("<br/><span class=\"error\"><pre>Xml Error: "
                          + strError.Replace(": error", ":<br/><b> error</b>") + "</pre></span>");
            Server.ClearError();
        }

    }

    public static class MvcResolve
    {
        public static void Init(string bin)
        {
            if (!string.IsNullOrWhiteSpace(bin) && Directory.Exists(bin))
            {
                StartMvc.WebBin = bin;
                AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
                // AppDomain.CurrentDomain.AssemblyLoad += CurrentDomain_AssemblyLoad;
            }
        }


        public static void ReflectCheck(string ddl, string typeName)
        {
            if (StartMvc.WebBin == null || !System.IO.Directory.Exists(StartMvc.WebBin))
            {
                StartMvc.WebBin = ConfigurationManager.AppSettings.Get("dir.WebBin");
                MvcResolve.Init(StartMvc.WebBin);
            }
            string path = StartMvc.WebBin + @"\" + ddl;

            // WRN: Native image will not be probed in LoadFrom context. Native image will only be probed in default load context,
            // like with Assembly.Load().
            try
            {
                AiLib.Web.LogMvc.Write("ReflectCheck " + path);
                var asmNpoi = Assembly.LoadFile(path); // .UnsafeLoadFrom(path);
                var utils = AppDomain.CurrentDomain.CreateInstanceFrom(path, typeName);
            }
            catch (Exception)
            {
                // StartMvc.Error(ex);
            }
        }

        static void CurrentDomain_AssemblyLoad(object sender, AssemblyLoadEventArgs args)
        {
            var asm = args.LoadedAssembly;
        }

        //static string[] webDlls = new[] { "NPOI.nozip", // npoi.nozip.dll
        //    "System.Web.Razor", "System.Web.WebPages", "System.Web.Optimization", "System.Web.Mvc",
        //    "Microsoft.Web.Infrastructure", "Newtonsoft.Json"
        //};

        static Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            string fileDll = new AssemblyName(args.Name).Name + ".dll";
            if (StartMvc.Log != null)
                StartMvc.Log.Write("Resolve name=" + args.Name + " Assembly=" + fileDll);

            string path = Path.GetFullPath(StartMvc.WebBin + @"\" + fileDll);
            if (File.Exists(path))
                return Assembly.LoadFrom(path);

            return args.RequestingAssembly;
        }
    }

    public static class WebConfig
    {
        // public ConfigurationManager Manager { [DebuggerStepThrough] get { return ConfigurationManager; } }

        public static NameValueCollection AppSettings
        { [DebuggerStepThrough] get { return ConfigurationManager.AppSettings; } }

        public static ConnectionStringSettingsCollection ConnectionStrings
        { [DebuggerStepThrough] get { return ConfigurationManager.ConnectionStrings; } }
    }

}
