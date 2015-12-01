using AiLib.Web;
using System;
using System.Collections.Specialized;
using System.Web.Mvc;
using System.Xml.Linq;

namespace AiLib.MvcHttp
{
    public class SegmentController : Controller
    {
        public virtual SegmentLang Segment { get { return SegmentLang.Instance; } }
        public virtual XElement PageTopXml { get { return SegmentLang.Instance.PageTopXml; } }

        private NameValueCollection _param;
        public virtual NameValueCollection Params
        {
            get { return _param ?? (_param = Request == null ? new NameValueCollection() : Request.QueryString); }
            set { _param = value; }
        }

        protected override void HandleUnknownAction(string actionName)
        {
            if (actionName != null
                && (actionName.Contains(".gif") || actionName.Contains(".png") || actionName.Contains(".jpg") 
                    || actionName.Contains(".ico")
                ))
                return;

            base.HandleUnknownAction(actionName);
        }

    }
}
