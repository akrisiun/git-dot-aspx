using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace AiLib.Web
{
    // https://bitbucket.org/davidebbo/webactivator/src/
    // http://ilearnable.net/2010/11/22/webactivator-preapplicationstartmethod/

    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
    public sealed class PreApplicationStartMethodAttribute : Attribute
    {
        private Type _type;
        private string _methodName;

        public PreApplicationStartMethodAttribute(Type type, string methodName)
        {
            _type = type;
            _methodName = methodName;
        }

        public Type Type
        {
            get
            {
                return _type;
            }
        }

        public string MethodName
        {
            get
            {
                return _methodName;
            }
        }

        public void InvokeMethod()
        {
            // Get the method
            MethodInfo method = Type.GetMethod(
                MethodName,
                BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);

            if (method == null)
            {
                throw new ArgumentException(
                    String.Format("The type {0} doesn't have a static method named {1}",
                        Type, MethodName));
            }

            // Invoke it
            method.Invoke(null, null);
        }
    }

    // Implementation:
    public class PreApplicationStartCode
    {
        public static void Start()
        {
            // Go through all the bin assemblies
            foreach (var assemblyFile in Directory.GetFiles(HttpRuntime.BinDirectory, "*.dll"))
            {
                var assembly = Assembly.LoadFrom(assemblyFile);

                // Go through all the PreApplicationStartMethodAttribute attributes
                // Note that this is *our* attribute, not the System.Web namesake
                foreach (PreApplicationStartMethodAttribute preStartAttrib in assembly.GetCustomAttributes(
                    typeof(PreApplicationStartMethodAttribute),
                    inherit: false))
                {

                    // Invoke the method that the attribute points to
                    preStartAttrib.InvokeMethod();
                }
            }
        }
    }
}
