using AiLib.Web;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace AiLib.Mvc
{
    public interface IMvcControl : IRenderBase
    {
#if WEB
        MvcHtmlString GetHtml();
#endif
    }

    public abstract class MvcControl : IMvcControl  // , IMvcString
    {
#if WEB
        public abstract MvcHtmlString GetHtml();
#endif
        public abstract string Render(System.IO.TextWriter writer);

        public abstract string ToString(TagRenderMode renderMode);
        public abstract void WriteTo(System.IO.TextWriter writer, TagRenderMode renderMode);
    }

}
