// Server 2008 route fix handler

using AiLib.Web;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Security.Principal;
using System.Web;
using System.Web.Routing;
using System.Web.SessionState;
using System.Web.UI;
using System.Diagnostics;

namespace AiLib.Web
{
    public enum RequestProcessState { Processed, Error, Unprocessed }
    public enum RequestEvent { BeginRequest, PreRequestHandlerExecute }

    /* Web.Config:
        <system.webServer>
            <modules runAllManagedModulesForAllRequests="true">
              <remove name="SxHttpHandlerModule" />
              <add name="SxHttpHandlerModule" type="AiLib.Render.HandlerModule, AiLib.MVC" />
            </modules>
            <validation validateIntegratedModeConfiguration="false" />
        <system.web>
          <httpHandlers>
          <clear />
              <add verb="*" path="routes.axd" type="AttributeRouting.Web.Logging.LogRoutesHandler, AttributeRouting.Web" />
              <add type="AiLib.Render.HandlerModule, AiLib.MVC" verb="GET" 
                   path="SX.ashx" validate="false" />
    */

    // IHttpHandler for ProcessRequest(HttpContext context) and IsReusable { get; }
    // IHttpModule  for void Init(HttpApplication context)

    public interface IRequestHttpHandler : IHttpModule, IHttpHandler, IDisposable
    {
        void AppRequestPrepare();
        bool ProcessRequestCore(RequestEvent requestEvent);
        RequestProcessState RequestProcessState { get; }
    }

    public class HttpHandler : IRequestHttpHandler
    {
        static List<IHttpModuleSubscriber> subscribers = new List<IHttpModuleSubscriber>();
        static HttpHandler() { }

        #region App static

        public static void AppCheck(HttpApplication appObj = null)
        {
            // var app = appObj ?? System.Web.HttpContext.Current.ApplicationInstance;
        }

        static dynamic Request
        {
            [DebuggerStepThrough]   // Step over
            get
            { return HttpStatic.Request; }
        }
        static dynamic Response
        {
            [DebuggerStepThrough]   // Step over
            get
            { return HttpStatic.Response; }
        }
        static dynamic Server
        {
            [DebuggerStepThrough]   // Step over
            get
            { return HttpStatic.Server; }
        }

        public static void AppError(Exception error = null)
        {
            string reqUrl = Request == null ? "-" : Request.Url.ToString();
            if (!reqUrl.Contains(".aspx"))
                Log.Write("Global (MvcHttp) Application_Error: url=" + reqUrl);

            var err = error ?? HttpStatic.Server.GetLastError();
            var req = Request;
            if (err == null || Segment.Instance.IgnoreError(error, req))
                return;

            string strError = err.ToString();
            var baseEx = err.GetBaseException();
            string baseExText = null;
            if (baseEx != null)
            {
                strError += "\n " + baseEx.Message;
                baseExText = baseEx.StackTrace.ToString();
            }

            Log.Write("Error details " + strError
                    + ": \n ip=" + Request.UserHostAddress + " refer=" + Request.UrlReferrer
                      + ("\n" + baseExText ?? ""));

            if (error == null)
            {
                Response.Write("<br/><span class=\"error\"><pre>Xml Error: "
                               + strError.Replace(": error", ":<br/><b> error</b>") + "</pre></span>");
                Server.ClearError();
            }
        }
        public static void AppEndRequest()
        {
            if (Response == null)
                return;

            if (Response.StatusCode >= 400) // == 404)
            {
                var exception = Server.GetLastError();

                Response.Clear();
                Response.Write("Status=" + Response.StatusCode);
                Response.Write(" Url=" + Request.Url);

                if (exception != null)
                {
                    Response.Write("Error " + exception.Message);
                    Response.Write("<br>" + exception.StackTrace);
                }

                Server.ClearError();
            }
        }

        #endregion

        // for PreRequestHandlerExecuteHandler
        public virtual void AppRequestPrepare()
        {
            if (AiLib.Web.Segment.Instance != null)
            {
                var instance = AiLib.Web.Segment.Instance;
                HttpRequest request = Request;
                instance.RequestPrepare(request);
            }
        }

        #region HttpApplication Context Events

        protected virtual void Init(HttpApplication context)
        {
            HttpContext.Current.Application[HandlerRegistrationFlag] = true;

            context.PreSendRequestHeaders += new EventHandler(PreSendRequestHeadersHandler);
            context.BeginRequest += new EventHandler(BeginRequestHandler);
            context.PreRequestHandlerExecute += new EventHandler(PreRequestHandlerExecuteHandler);

            // Occurs when a security module has established the identity of the user.
            context.Error += new EventHandler(OnError);

            SegmentLang.Instance.InitHandler(context, this);
        }

        /* https://support.microsoft.com/en-us/kb/307985/en-us

        An HttpApplication class provides a number of events with which modules can synchronize. 
        The following events are available for modules to synchronize with on each request. 
        These events are listed in sequential order: 
        1.BeginRequest
        2.AuthenticateRequest
        3.AuthorizeRequest
        4.ResolveRequestCache
        5.AcquireRequestState
        6.PreRequestHandlerExecute
        7.PostRequestHandlerExecute
        8.ReleaseRequestState
        9.UpdateRequestCache
        10.EndRequest

        The following events are available for modules to synchronize with for each request transmission. 
        The order of these events is non-deterministic. 
         
        •PreSendRequestHeaders
        •PreSendRequestContent
        •Error
        */

        protected virtual void OnError(object sender, EventArgs args)
        {
            Exception err = Server == null ? null : Server.GetLastError();
            AppError(err);
        }

        protected virtual void PreSendRequestHeadersHandler(object sender, EventArgs args)
        {
            if (RequestProcessState == RequestProcessState.Error)
                return;

            this.requestProcessState = RequestProcessState.Processed;
            HttpApplication app = (HttpApplication)sender;
            HttpResponse response = app.Response;
            HttpRequest request = app.Request;

            if (response.IsRequestBeingRedirected)
                return;

            //if (response.StatusCode == 403 && request.Url.PathAndQuery == "/")
            //{
            //    response.Redirect("?");
            //    return;
            //}

            if (IsCallBack(request))
            {
                if (IsErrorCode(response.StatusCode))
                {
                    response.Output.Write("Error code {0} {1}", response.StatusCode, response);
                }
            }
        }

        protected virtual void BeginRequestHandler(object sender, EventArgs e)
        {
            bool requestProcessed = (this as IRequestHttpHandler).ProcessRequestCore(RequestEvent.BeginRequest);
            //if(!requestProcessed && UploadProgressManager.IsProcessRequestAllowed()) 
            //    UploadProgressManager.ProcessRequest(ref requestProcessState);
        }

        protected virtual void PreRequestHandlerExecuteHandler(object sender, EventArgs e)
        {
            AppRequestPrepare();
            (this as IRequestHttpHandler).ProcessRequestCore(RequestEvent.PreRequestHandlerExecute);
        }

        RequestProcessState requestProcessState = RequestProcessState.Unprocessed;
        public RequestProcessState RequestProcessState
        {
            get { return requestProcessState; }
        }
        public const string HandlerRegistrationFlag = "HttpHandlerModuleRegistered";

        #endregion

        #region Methods

        bool IRequestHttpHandler.ProcessRequestCore(RequestEvent requestEvent)
        {
            HttpRequest request = // HttpUtils.GetRequest();
                        (HttpContext.Current != null) ? HttpContext.Current.Request : null;

            foreach (IHttpModuleSubscriber subscriber in subscribers)
            {
                if (subscriber.RequestRecipient(request, requestEvent))
                {
                    subscriber.ProcessRequest();
                    return true;
                }
            }
            return false;
        }

        protected virtual bool IsCallBack(HttpRequest request)
        {
            if (request.HttpMethod == "GET")
                return false;
            bool hasParam = false;

            //try
            //{
            //    hasParam = !string.IsNullOrEmpty(HttpUtils.GetValueFromRequest(
            //        request, RenderUtils.CallbackControlIDParamName));
            //}
            //catch (HttpRequestValidationException)
            //{
            //    hasParam = !string.IsNullOrEmpty(HttpUtils.GetValueFromRequest(
            //        request, RenderUtils.CallbackControlIDParamName));
            //}
            return hasParam;
            //|| MvcUtils.IsCallback();
        }

        protected bool IsErrorCode(int code)
        {
            if (code == 404 || code == 500)
                return true;
            return false;
        }

        protected virtual void PrepareContent(ref HttpResponse response)
        {
            List<HttpCookie> cookies = new List<HttpCookie>(response.Cookies.Count);
            for (int i = 0; i < response.Cookies.Count; i++)
                cookies.Add(response.Cookies[i]);

            response.ClearHeaders();
            response.ClearContent();
            response.ContentType = "text/html";
            for (int i = 0; i < cookies.Count; i++)
                response.AppendCookie(cookies[i]);
            response.Cache.SetCacheability(HttpCacheability.NoCache);
        }
        #endregion

        protected virtual void Dispose(bool isDispose) { }

        #region IHttpModule
        void IHttpModule.Init(HttpApplication context)
        {
            Init(context);
        }
        void IHttpModule.Dispose()
        {
            Dispose(true);
        }
        bool IHttpHandler.IsReusable
        {
            get { return true; }
        }

        // void IHttpHandler.ProcessRequest(HttpContext context)
        public virtual void ProcessRequest(HttpContext context)
        {
            CustomCssJsManager.ProcessRequest();
        }

        void IDisposable.Dispose() { this.Dispose(true); }

        #endregion

    }

    #region Dx Utils

    public static class CustomCssJsManager
    {
        const string
            CssFolderParameterName = "cssfolder",
            CssFileParameterName = "cssfile",
            JsFolderParameterName = "jsfolder",
            JsFileParameterName = "jsfile",
            JsFileSetParameterName = "jsfileset",
            CssMimeType = "text/css",
            JsMimeType = "text/javascript",
            CssFileExtension = ".css",
            JsFileExtension = ".js";

        static void SetCaching()
        {
            HttpResponse response = HttpUtils.GetResponse();
            response.Cache.VaryByParams[CssFolderParameterName] = true;
            response.Cache.VaryByParams[CssFileParameterName] = true;
            response.Cache.VaryByParams[JsFolderParameterName] = true;
            response.Cache.VaryByParams[JsFileParameterName] = true;
            response.Cache.VaryByParams[JsFileSetParameterName] = true;
            response.Cache.SetCacheability(HttpCacheability.ServerAndPrivate);
            response.Cache.SetOmitVaryStar(true);
            response.Cache.SetLastModifiedFromFileDependencies();
        }

        public static void ProcessRequest()
        {
            HttpResponse response = HttpUtils.GetResponse();
            response.Clear();
            try
            {
                MakeResponse();
                SetCaching();
            }
            catch (Exception ex)
            {
                response.Clear();
                response.StatusCode = 500;
                response.StatusDescription = ex.Message;
            }
        }

        static void MakeResponse()
        {
            HttpRequest request = HttpUtils.GetRequest();
            HttpResponse response = HttpUtils.GetResponse();
            string cssFolderParameter = request.QueryString[CssFolderParameterName];
            if (!string.IsNullOrEmpty(cssFolderParameter))
            {
                response.ContentType = CssMimeType;
                WriteFolderToResponse(cssFolderParameter, CssFileExtension);
            }
        }

        public static bool IsAppRelativePath(string path)
        {
            if ((path.Length > 0) && (path[0] == '~'))
                return (path.Length == 1) || IsRooted(path.Substring(1));
            else
                return false;
        }

        static bool IsRooted(string basepath)
        {
            return !string.IsNullOrEmpty(basepath) && ((basepath[0] == '\\') || (basepath[0] == '/'));
        }

        static string GetPhysicalPath(string path)
        {
            //if (UrlUtils.IsAbsolutePhysicalPath(path))
            //    throw new Exception(
            //    @"An absolute physical path is not allowed. Use a virtual path instead, e.g., ""~\scripts\file1.js"".");

            if (!IsAppRelativePath(path))
                path = "~/" + path;
            HttpRequest request = HttpUtils.GetRequest();
            string physicalPath = request.MapPath(path);
            if (!physicalPath.StartsWith(request.PhysicalApplicationPath))
                throw new Exception(@"Cannot use the specified path to exit above the application directory");
            return physicalPath;
        }

        static void WriteFileToResponse(string fileName, string allowedExtension)
        {
            if (Path.GetExtension(fileName).ToLowerInvariant() != allowedExtension) return;
            string content = File.ReadAllText(fileName);
            HttpResponse response = HttpUtils.GetResponse();
            response.Write(content);
            response.AddFileDependency(fileName);
        }

        static void WriteFolderToResponse(string directoryPath, string fileExtension)
        {
            const char AllFilesMaskToken = '*';
            string physicalPath = GetPhysicalPath(directoryPath);
            string[] fileNames = Directory.GetFiles(physicalPath, AllFilesMaskToken + fileExtension);
            foreach (string fileName in fileNames)
                WriteFileToResponse(fileName, fileExtension);
            HttpUtils.GetResponse().AddFileDependency(physicalPath);
        }
        static void WriteJsFileSetToResponse(string fileSetParam)
        {
            const char FileNameSeparator = ';';
            string[] fileNames = fileSetParam.Split(FileNameSeparator);
            foreach (string fileName in fileNames)
                WriteFileToResponse(GetPhysicalPath(fileName), JsFileExtension);
        }

    }

    public static class HttpUtils
    {
        public const string HttpHandlerModuleName = "HttpHandlerModule";

        public static HttpRequest GetRequest()
        {
            if (HttpContext.Current != null)
                return HttpContext.Current.Request;
            return null;
        }

        public static HttpRequest GetRequest(Control control)
        {
            return GetRequest(control.Page);
        }
        public static HttpRequest GetRequest(Page page)
        {
            if (HttpContext.Current != null)
                return HttpContext.Current.Request;
            else if (page != null)
                return page.Request;
            else
                return null;
        }


        public static HttpResponse GetResponse()
        {
            return GetResponse((Page)null);
        }
        public static HttpResponse GetResponse(Control control)
        {
            return GetResponse(control.Page);
        }
        public static HttpResponse GetResponse(Page page)
        {
            if (HttpContext.Current != null)
                return HttpContext.Current.Response;
            else if (page != null)
                return page.Response;
            else
                return null;
        }
        public static NameValueCollection GetQuery()
        {
            return GetQuery((Page)null);
        }
        public static NameValueCollection GetQuery(Control control)
        {
            return GetQuery(control.Page);
        }
        public static NameValueCollection GetQuery(Page page)
        {
            HttpRequest request = GetRequest(page);
            return request != null ? request.QueryString : null;
        }
    }

    #endregion

    public interface IHttpModuleSubscriber
    {
        bool RequestRecipient(HttpRequest request, RequestEvent requestEvent);
        void ProcessRequest();
    }

}
