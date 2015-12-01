using AiLib.Web;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace AiLib.MvcHttp
{
    public class NetVersionController : SegmentController
    {
        protected override void HandleUnknownAction(string actionName)
        {
            // base.HandleUnknownAction(actionName);
            Output(this.HttpContext.Response);
        }

        public ActionResult Index(string param = null)
        {
            Output(HttpContext.Response);
            return new EmptyResult();
        }

        // Debug info
        static bool wrote = false;
        public static void Output(HttpResponseBase Response, bool wroteForce = false)
        {
            if (wrote && !wroteForce) return;
            wrote = true;
            Response.Write("Version: " + System.Environment.Version.ToString());
            Response.Write("<br/>UsingIntegratedPipeline=" + System.Web.HttpRuntime.UsingIntegratedPipeline.ToString());
            Response.Write(", IISVersion=" + System.Web.HttpRuntime.IISVersion.ToString());
            Response.Write("<br/>AspInstallDirectory=" + System.Web.HttpRuntime.AspInstallDirectory.ToString());
            Response.Write("<br/>AppDomain.BaseDirectory=" + System.AppDomain.CurrentDomain.BaseDirectory);
            AiLib.RazorGenerator.Mvc.EngineDebug.Output(HttpStatic.Response);

            Response.Write("<br/>Assemblies=" + System.AppDomain.CurrentDomain.GetAssemblies().Length.ToString());
            foreach (var asm in System.AppDomain.CurrentDomain.GetAssemblies())
            {
                try { Response.Write("<br/>" + asm.CodeBase.Replace("file:///", "")); }
                catch { Response.Write("<br/>" + asm.FullName); }
            }
        }

    }
}
