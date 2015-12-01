using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AiLib.Mvc
{
    // Visual Studio F12
    public class CallerAttribute : Attribute
    {
        public CallerAttribute(Type viewClass, string method = null) { }
    }

    public class ViewClassAttribute : Attribute
    {
        public ViewClassAttribute(Type viewClass, string url = null)
        {
            ViewClass = viewClass;
            Url = url;
        }

        public Type ViewClass { get; set; }
        public string Url { get; set; }
    }

    public class ControllerClassAttribute : Attribute
    {
        public ControllerClassAttribute(Type viewClass, string action = null)
        {
            ControllerClass = viewClass;
            Action = action;
        }

        public Type ControllerClass { get; set; }
        public string Action { get; set; }
    }


    public class RouteClassAttribute : Attribute
    {
        public RouteClassAttribute(Type mapClass)
        {
            MapClass = mapClass;
        }

        public Type MapClass { get; set; }
    }
}
