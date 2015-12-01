using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Web.Routing;

namespace AiLib.MvcHttp
{
    public static class HostMap
    {
        public static Route MapRoute(this RouteCollection routes,
            string name, string url, object defaults, object constraints, string[] namespaces)
        {
            if (routes == null)
            {
                throw new ArgumentNullException("routes");
            }
            if (url == null)
            {
                throw new ArgumentNullException("url");
            }

            // System.Web.Mvc.MvcRouteHandler
            Route route = new Route(url, new MvcRouteHandler())
            {
                Defaults = CreateRouteValueDictionaryUncached(defaults),
                Constraints = CreateRouteValueDictionaryUncached(constraints),
                DataTokens = new RouteValueDictionary()
            };

            //ConstraintValidation.Validate(route);

            //if ((namespaces != null) && (namespaces.Length > 0))
            //{
            //    route.DataTokens[RouteDataTokenKeys.Namespaces] = namespaces;
            //}

            routes.Add(name, route);

            return route;
        }

        private static RouteValueDictionary CreateRouteValueDictionaryUncached(object values)
        {
            var dictionary1 = values as IDictionary<string, object>;
            if (dictionary1 != null)
            {
                return new RouteValueDictionary(dictionary1);
            }

            // System.Web.Mvc.TypeHelper
            // return TypeHelperHost.ObjectToDictionaryUncached(values);
            RouteValueDictionary dictionary = new RouteValueDictionary();

            if (values != null)
            {
                //foreach (PropertyDescriptor helper in AiLib.Reflection.ReflectionUtils.GetProperties(values))
                //{
                //    dictionary.Add(helper.Name, helper.GetValue(values));
                //}
            }

            return dictionary;
        }

    }


}