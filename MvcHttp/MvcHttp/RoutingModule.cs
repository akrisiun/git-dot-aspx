using AiLib.Web;
using System;
using System.Configuration;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text;
using System.Web;
using System.Web.Configuration;
using System.Web.Routing;
using System.Web.Security;

namespace AiLib.MvcHttp
{
    // https://github.com/Microsoft/referencesource/blob/master/System.Web/Routing/UrlRoutingModule.cs
    // System.Web.Routing 
    // [TypeForwardedFrom("System.Web.Routing, Version=3.5.0.0, Culture=Neutral, PublicKeyToken=31bf3856ad364e35")]
    // public class UrlRoutingModule : IHttpModule 

    /* Web.config
      <!--<system.webServer><modules runAllManagedModulesForAllRequests="true">-->
      <remove name="SxRoutingModule" />
      <add name="SxRoutingModule" type="AiLib.MvcHttp.RoutingModule, AiLib.MvcHttp" />
      <remove name="SxHttpHandlerModule" />
      <add name="SxHttpHandlerModule" type="AiLib.Web.HttpHandler, AiLib.MvcHttp" />
    */

    public class RoutingModule : IHttpModule
    {
        #region Static methods

        public static void EnumerateHttpModules(string url, HttpResponse response)
        {
            Configuration config = WebConfigurationManager.OpenWebConfiguration(url);

            // Get the <httpModules> section.
            HttpModulesSection section =
                (HttpModulesSection)config.GetSection("system.web/httpModules");

            StringBuilder output = new StringBuilder();
            output.AppendFormat("<div><httpModules> modules element in {0}:<br/>",
                config.FilePath.ToString());

            for (int i = 0; i < section.Modules.Count; i++)
            {
                output.AppendFormat("<span>{0}, {1}</span><br/>",
                    section.Modules[i].Name.ToString(),
                    section.Modules[i].Type.ToString());
            }

            output.AppendFormat("</div>");

            response.Write(output);
        }

        // https://msdn.microsoft.com/en-us/library/tkwek5a4%28v=vs.85%29.aspx?f=255&MSPPError=-2147217396
        // Example Configuration Code for an HTTP Module:

        public static void AddModule(Configuration config, string moduleName, string moduleClass)
        {
            HttpModulesSection section =
                (HttpModulesSection)config.GetSection("system.web/httpModules");

            // Create a new module action object.
            HttpModuleAction moduleAction = new HttpModuleAction(
                moduleName, moduleClass);
            // "RequestTimeIntervalModule", "Samples.Aspnet.HttpModuleExamples.RequestTimeIntervalModule");

            // Look for an existing configuration for this module.
            int indexOfModule = section.Modules.IndexOf(moduleAction);
            if (-1 != indexOfModule)
            {
                // Console.WriteLine("RequestTimeIntervalModule module is already configured at index {0}", indexOfModule);
            }
            else
            {
                section.Modules.Add(moduleAction);

                if (!section.SectionInformation.IsLocked)
                {
                    config.Save();
                    // Console.WriteLine("RequestTimeIntervalModule module configured.");
                }
            }
        }

        #endregion

        #region URL context

        static RoutingModule()
        {   // debugger entry point
        }

        private static readonly object _requestDataKey = new object();
        private RouteCollection _routeCollection;
        public RouteCollection RouteCollection
        {
            get
            {
                if (_routeCollection == null)
                {
                    _routeCollection = RouteTable.Routes;
                }
                return _routeCollection;
            }
            set
            {
                _routeCollection = value;
            }
        }

        protected virtual void Dispose() { }

        private static readonly object _contextKey = new object();
        protected virtual void Init(HttpApplication application)
        {
            //////////////////////////////////////////////////////////////////
            // Check if this module has been already addded
            if (application.Context.Items[_contextKey] != null)
            {
                return; // already added to the pipeline
            }
            application.Context.Items[_contextKey] = _contextKey;

            // Ideally we would use the MapRequestHandler event.  However, MapRequestHandler is not available
            // in II6 or IIS7 ISAPI Mode. Instead, we use PostResolveRequestCache, which is the event immediately
            // before MapRequestHandler.  This allows use to use one common codepath for all versions of IIS.
            application.PostResolveRequestCache += OnApplicationPostResolveRequestCache;
        }

        private void OnApplicationPostResolveRequestCache(object sender, EventArgs e)
        {
            HttpApplication app = (HttpApplication)sender;
            HttpContextBase context = new HttpContextWrapper(app.Context);
            PostResolveRequestCache(context);
        }

        // [Obsolete("This method is obsolete. Override the Init method to use the PostMapRequestHandler event.")]
        // public virtual void PostMapRequestHandler(HttpContextBase context)

        #endregion

        public virtual void PostResolveRequestCache(HttpContextBase context)
        {
            // Match the incoming URL against the route table
            RouteData routeData = RouteCollection.GetRouteData(context);

            // Do nothing if no route found
            if (routeData == null)
            {
                SegmentLang.Instance.ResolveRequest(context, routeData);
                return;
            }

            // If a route was found, get an IHttpHandler from the route's RouteHandler
            IRouteHandler routeHandler = routeData.RouteHandler;
            if (routeHandler == null)
            {
                SegmentLang.Instance.ResolveRequest(context, routeData);

                throw new InvalidOperationException(
                    String.Format(
                        CultureInfo.CurrentCulture, "NoRouteHandler"));
            }

            // This is a special IRouteHandler that tells the routing module to stop processing
            // routes and to let the fallback handler handle the request.
            if (routeHandler is StopRoutingHandler)
            {
                return;
            }

            RequestContext requestContext = new RequestContext(context, routeData);

            // Dev10 766875	Adding RouteData to HttpContext
            context.Request.RequestContext = requestContext;

            // AiLib
            SegmentLang.Instance.ResolveRequest(context, routeData);

            IHttpHandler httpHandler = routeHandler.GetHttpHandler(requestContext);
            if (httpHandler == null)
            {
                throw new InvalidOperationException(
                    String.Format(
                        CultureInfo.CurrentUICulture,
                        "NoHttpHandler {0}",
                        routeHandler.GetType())
                    );
            }

            var fullName = httpHandler.GetType().FullName;

            // (httpHandler is UrlAuthFailureHandler)
            if (fullName.Contains("UrlAuthFailure")) // System.Web.Routing.UrlAuthFailureHandler 
            {
                //if (FormsAuthenticationModule.FormsAuthRequired)
                //{
                //    UrlAuthorizationModule.ReportUrlAuthorizationFailure(HttpContext.Current, this);
                //    return;
                throw new HttpException(401, "Access denied"); // _Description3
            }

            // Remap IIS7 to our handler:
            // class System.Web.HttpContextBase
            // virtual void RemapHandler(IHttpHandler handler)

            context.RemapHandler(httpHandler);
        }

        #region IHttpModule Members

        void IHttpModule.Dispose()
        {
            Dispose();
        }

        void IHttpModule.Init(HttpApplication application)
        {
            Init(application);
        }
        #endregion
    }

    // namespace System.Web.Routing {
    // Token class used to signal Auth failures, not meant to be used as a handler    
    internal sealed class UrlAuthFailureHandler : IHttpHandler
    {
        public void ProcessRequest(HttpContext context)
        {
            throw new NotImplementedException();
        }

        public bool IsReusable
        {
            get
            {
                return true;
            }
        }
    }
}