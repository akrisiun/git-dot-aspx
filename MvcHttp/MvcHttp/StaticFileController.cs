using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AiLib.MvcHttp
{
    // TODO
    public class StaticFileController
    {
        //protected override void HandleUnknownAction(string file)
        //{
        //    base.HandleUnknownAction(file);

        //    if (file.StartsWith("fonts") || file.StartsWith("glyphicons"))
        //    {
        //        string map = Server.MapPath("~/") + (file.StartsWith("glyphicons") ? "fonts\\" : "");

        //        HttpResponseBase response = this.ControllerContext.HttpContext.Response;
        //        response.ContentType = "application/vnd.ms-fontobject";

        //        // string str = new ContentDisposition { FileName = file, Inline = Inline }.ToString();
        //        // response.AddHeader("Content-Disposition", str);
        //        response.WriteFile(map + file);
        //    }

        //    /*  http://stackoverflow.com/questions/2871655/proper-mime-type-for-fonts
        //    svg   as "image/svg+xml"
        //    ttf   as "application/x-font-ttf" or "application/x-font-truetype"
        //    woff  as "application/font-woff"
        //    woff2 as "application/font-woff2" (proposed by W3C)
        //    eot   as "application/vnd.ms-fontobject" 
        //    */
        //}
    }
}
