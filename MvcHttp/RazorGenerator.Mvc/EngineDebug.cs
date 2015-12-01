using RazorGenerator.Mvc;
using AiLib.Web.Reflection;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Routing;
using System.Web.WebPages;
using System.Xml;
using System.Xml.Linq;

namespace AiLib.RazorGenerator.Mvc
{
    public static class EngineDebug
    {
        public static void Output(HttpResponse Response)
        {
            Response.Write("<br/>HostingEnvironment.ApplicationPhysicalPath=" + System.Web.Hosting.HostingEnvironment.ApplicationPhysicalPath);
            Response.Write("<br/>HostingEnvironment.ApplicationVirtualPath=" + System.Web.Hosting.HostingEnvironment.ApplicationVirtualPath);
            Response.Write("<br/>HostingEnvironment.SiteName=" + System.Web.Hosting.HostingEnvironment.SiteName);

            AiLib.RazorGenerator.Mvc.EngineDebug.ListRoutes(Response);
            AiLib.RazorGenerator.Mvc.EngineDebug.ListMap(Response, System.Reflection.Assembly.GetEntryAssembly());

            try
            {
                string url = System.Web.Hosting.HostingEnvironment.ApplicationVirtualPath;
                AiLib.MvcHttp.RoutingModule.EnumerateHttpModules(url, Response);
            }
            catch (Exception ex) { Response.Write(String.Format("Route modules error {0}", ex.Message)); }
        }

        public static void ListMap(HttpResponse Response, Assembly asm)
        {
            try
            {
                var assembly = asm ?? PrecompiledMvcEngine.Assemblies.First();

                var map = (from type in assembly.GetTypes()
                           where typeof(WebPageRenderingBase).IsAssignableFrom(type)
                           let pageVirtualPath =
                               type.GetCustomAttributes(inherit: false).OfType<PageVirtualPathAttribute>().FirstOrDefault()
                           where pageVirtualPath != null
                           select new
                           {
                               VirtualPath = pageVirtualPath.VirtualPath,
                               Type = type.ToString()
                           }
                           );

                Response.Write("<br>");
                Response.Write("Assembly= " + assembly.ToString() + "<br>");
                if (!map.Any())
                    Response.Write("No WebPageRenderingBase atributes [PageVirtualPathAttribute]<br>");
                else
                {
                    var table = new XElement("table",
                            new XElement("tr", new XElement("th", "VirtualPath"), new XElement("th", "Type")),
                            map.Select(item => new XElement("tr",
                                    new XElement("td", item.VirtualPath),
                                    new XElement("td", item.Type)
                                ))
                            );
                    table.Save(Response.OutputStream);
                }

            }
            catch (Exception ex) { Response.Write(ex.Message); }
        }

        public static void ListRoutes(HttpResponse Response)
        {
            RouteCollection routes = RouteTable.Routes;
            Collection<RouteBase> List = routes;

            var table = new XElement("table", new XElement("tr"
                    , new XElement("th", "Route.Defaults")
                    , new XElement("th", "Route.Url")
                    ));
            table.Add(List.Select(item => new XElement("tr"
                    , new XElement("td", item is Route ? ParseDefaults((item as Route).Defaults) : "-")
                    , new XElement("td", ObjectConvert.GetValue<string>(item, "Url"))
                    )));
            Response.Write(new XElement("br"));
            table.Save(Response.OutputStream);
        }

        static string ParseDefaults(RouteValueDictionary defaults)
        {
            string str = "";
            if (defaults == null)
                return str;
            var numer = defaults.GetEnumerator();
            while (numer.MoveNext())
            {
                KeyValuePair<string,object> obj = numer.Current;
                str = obj.Key + "=\"" + obj.Value.ToString() + "\" "
                    + (str.Length == 0 ? "" : ", " + str);
            }
            return "{ " + str + "}";
        }
    }
}
