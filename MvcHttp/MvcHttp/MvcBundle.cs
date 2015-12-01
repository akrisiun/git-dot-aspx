using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Xml.Linq;

namespace AiLib.MvcHttp
{
    public static class MvcBundle
    {
        public static BundleCollection Bundles()
        {
            return BundleTable.Bundles;
        }

        // #if WEB
        public static UrlHelper Url(HttpContext Current = null)
        {
            return new System.Web.Mvc.UrlHelper(
                ((Current ?? HttpContext.Current).Handler as System.Web.Mvc.MvcHandler).RequestContext);
        }

        public static string Css(string contentCss, string path = "~/bundles/")
        {
            return BundleTable.Bundles.ResolveBundleUrl((path ?? "") + contentCss);
        }

        public static string Js(string scriptsJs, string path = "~/bundles/")
        {
            return BundleTable.Bundles.ResolveBundleUrl((path ?? "") + scriptsJs);
        }

        // <script src="@Url.Content("~/Scripts/divelemx.js")" type="text/javascript"></script>
        public static XElement ScriptSrc(this UrlHelper Url, string js, string path = "~/bundles/")
        {
            return MvcTag.ScriptSrc(Url, (path ?? "") + js, "");
        }

        // <link href="@Url.Content("~/Content/jquery-ui-min.css")"  rel="stylesheet" type="text/css" />
        public static XElement LinkHref(this UrlHelper Url, string css = "css", string path = "~/bundles/")
        {
            return MvcTag.LinkHrefCss(Url, (path ?? "") + css, "");
        }

        public static XElement LinkHrefCss(this UrlHelper Url, string css, string path = "~/css/")
        {
            return MvcTag.LinkHrefCss(Url, (path ?? "") + css, "");
        }


        public static ScriptBundle ScriptBundle(string name, string path = "~/bundles/")
        {
            return new ScriptBundle((path ?? "") + name);
        }

        public static Bundle IncludeJS(this Bundle scripts,
               string scriptsJS, string path = "~/Scripts/")
        {
            return scripts.Include((path ?? "") + scriptsJS);
        }

        public static StyleBundle StyleBundle(string name = "css", string path = "~/bundles/")
        {
            return new StyleBundle((path ?? "") + name);
        }

        public static Bundle IncludeCss(this Bundle styles,
                string contentCss, string path = "~/css/")
        {
            return styles.Include((path ?? "") + contentCss);
        }

        public static ScriptBundle IncludeJQuery(this ScriptBundle scripts, string version = "-1.11.2.min")
        {
            return scripts.Include("~/Scripts/jq/jquery" + (version ?? "") + ".js") as ScriptBundle;
        }

        public static StyleBundle IncludeCssBootStrap(this StyleBundle scripts)
        {
            return scripts.Include("~/css/bootstrap.css") as StyleBundle;
        }


        public static string WriteCssHref(this TextWriter output, UrlHelper url, string css, string path = "~/css/")
        {
            output.Write(LinkHrefCss(url, css, path));
            return String.Empty;
        }

        public static string WriteJsSrc(this TextWriter output, UrlHelper url, string js, string path = "~/Scripts/")
        {
            output.Write(ScriptSrc(url, js, path));
            return String.Empty;
        }

        public static string WriteRaw(this TextWriter output, object raw)
        {
            if (raw != null)
                output.Write(raw.ToString());
            return String.Empty;
        }

    }

    public static class Optimization
    {
        public static IHtmlString RenderJS(string url = "~/bundles/jquery")
        {
            return System.Web.Optimization.Scripts.Render(url);
        }

        public static IHtmlString Render(params string[] paths)
        {
            return System.Web.Optimization.Scripts.Render(paths);
        }

        public static IHtmlString RenderCss(string url = "~/css/css")
        {
            return System.Web.Optimization.Styles.Render(url);
        }
    }

    public static class MvcTag
    {
        public static XElement ScriptSrc(UrlHelper Url, string js, string path = "~/Scripts/")
        {
            return new XElement("script",
                 new XAttribute("src", Url.Content((path ?? "") + js)),
                 new XAttribute("type", "text/javascript"),
                 " ");
        }

        public static XElement LinkHrefCss(UrlHelper Url, string css = "css", string path = "~/css/")
        {
            return new XElement("link",
                 new XAttribute("href", Url.Content((path ?? "") + css)),
                 new XAttribute("rel", "stylesheet"),
                 new XAttribute("type", "text/css")
               );
        }
    }

}