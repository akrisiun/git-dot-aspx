using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AiLib.Render;
#if !WEB
    using AiLib.IIS;
#else
    using System.Web;
#endif

namespace AiLib.Web.Render
{
#if WEB

    public abstract class WebControlBase : System.Web.UI.WebControls.WebControl, IRenderBase
    {
        public System.Collections.Specialized.NameValueCollection QueryString
        {
            // HttpRequest.Params or Request.Params gets just about everything:
            // querystring, form, cookie and session variables) from the httprequest,
            // whereas Request.Querystring only will pull the querystring
            get
            {
                if (this.Context != null && this.Context.Request.Params != null)
                    return this.Context.Request.Params;
                return null;
            }
        }

        HttpContext context = null;
        public new HttpContext Context
        {
            get { return HttpContext.Current != null ? base.Context : context; }
            set { context = value; }
        }

        public void TraceWrite(string category, string message)
        {
            if (context == null && HttpContext.Current == null)
                return;
            TraceContext Trace = (Context != null) ? Context.Trace : null;
            if (Trace != null)
                Trace.Write(category, message);
        }

        public abstract string Render(TextWriter writer);

    }

#else
    public abstract class WebControlBase : IRenderBase
    {
        public string ID { get; set; }
        public abstract string Render(TextWriter writer);

        public System.Collections.Specialized.NameValueCollection QueryString
        {
            // HttpRequest.Params or Request.Params gets just about everything:
            // querystring, form, cookie and session variables) from the httprequest,
            // whereas Request.Querystring only will pull the querystring
            get
            {
                //if (this.Context != null && this.Context.Request.Params != null)
                //    return this.Context.Request.Params;
                return null;
            }
        }

        AiLib.IIS.HttpContextBase context = null;
        public HttpContextBase Context
        {
            get { return context ?? (context = HeliosApplication.Context); }
            set { context = value; }
        }

        public void TraceWrite(string category, string message)
        {
            System.Diagnostics.Trace.Write(category + " : " + message);
        } 

    }
#endif

}
