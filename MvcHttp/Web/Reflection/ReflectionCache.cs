using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace AiLib.Web.Reflection
{
    public static class ReflectionCache
    {
        #region Values Get, Property Cache

        private static PropertyCache propertyCache;
        static ReflectionCache() { propertyCache = new PropertyCache(); }

        private class PropertyCache
        {
            public struct TypePropertyPair
            {
                public Type type;
                public string propertyName;
            }

            static Dictionary<TypePropertyPair, PropertyDescriptor> cache = new Dictionary<TypePropertyPair, PropertyDescriptor>();

            public PropertyDescriptor GetPropertyDescriptor(object source, string propertyName)
            {
                TypePropertyPair key = new TypePropertyPair() { type = source != null ? source.GetType() : null, propertyName = propertyName };
                if (key.type == null || key.propertyName == null)
                    return null;
                if (cache.ContainsKey(key))
                    return cache[key];
                else
                {
                    PropertyDescriptorCollection properties = TypeDescriptor.GetProperties(source);
                    PropertyDescriptor descriptor = properties == null ? null : properties.Find(propertyName, ignoreCase: true);
                    if (descriptor == null) return null;

                    lock (cache)
                    {
                        if (!cache.ContainsKey(key))
                            cache.Add(key, descriptor);
                    }
                    return descriptor;
                }
            }
        }

        public static IEnumerable<PropertyDescriptor> GetProperties(object source) // this object source)
        {
            PropertyDescriptorCollection properties = TypeDescriptor.GetProperties(source);
            var numer = properties.GetEnumerator();
            while (numer.MoveNext())
                // https://raw.githubusercontent.com/Microsoft/referencesource/c697a4b9782dc8c85c02344a847cb68163702aa7/System/compmod/system/componentmodel/ReflectPropertyDescriptor.cs
                // ReflectPropertyDescriptor
                yield return numer.Current as PropertyDescriptor;
        }

        public static PropertyDescriptor GetProperty(object source, string propertyName) // this object source)
        {
            return propertyCache.GetPropertyDescriptor(source, propertyName);
        }

        public static bool TryToExtractValueFromDescriptor(object source, string propertyName, out object value)
        {
            var descriptor = propertyCache.GetPropertyDescriptor(source, propertyName);
            if (descriptor != null)
            {
                value = descriptor.GetValue(source);
                return true;
            }

            value = null;
            return false;
        }

        #endregion
    }
}
