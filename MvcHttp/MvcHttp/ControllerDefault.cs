using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Web.Routing;

namespace AiLib.MvcHttp
{
    // http://pratapreddypilaka.blogspot.com/2011/10/custom-controller-factory-structure-map.html

    public class ControllerDefault : DefaultControllerFactory
    {
        public static void SetFactory<T>() where T : Controller
        {
            DefaultType = typeof(T);
            ControllerBuilder.Current.SetControllerFactory(new ControllerDefault());
        }

        public static void MapDefault(string controller = null, string action = "Default")
        {
            RouteCollection routes = RouteTable.Routes;

            routes.IgnoreRoute("{WebPage}.aspx/{*pathInfo}");
            routes.MapRoute(
                         name: "Default",
                         url: "{controller}/{action}/{ext}",
                         defaults: new
                         {
                             controller = controller,
                             action = action,
                             ext = UrlParameter.Optional
                         }
                     );
        }

        public static string Segment2(RequestContext requestContext)
        {
            var routeData = requestContext.RouteData;
            string segment2 = string.Empty;
            if (routeData.Values != null)
            {
                var values = routeData.Values.GetEnumerator();
                while (values.MoveNext())
                {
                    if (!string.IsNullOrWhiteSpace(values.Current.Value as string))
                    {
                        segment2 = values.Current.Value as string;
                        break;
                    }
                }
            }
            return segment2;
        }

        public static Type DefaultType { get; private set; }
        public static Func<RequestContext, string, Controller> ChooseController { get; set; }

        public override IController CreateController(RequestContext requestContext, string controllerName)
        {
            Controller controller = null;

            if (ChooseController != null)
                controller = ChooseController(requestContext, controllerName);

            if (controller == null)
                controller = Activator.CreateInstance(DefaultType) as Controller;

            return controller;
        }

        public override void ReleaseController(IController controller)
        {
            var disposable = controller as IDisposable;
            if (disposable != null)
            {
                disposable.Dispose();
            }
        }
    }
}
