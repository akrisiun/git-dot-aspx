using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Dynamic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Xml.Linq;

namespace AiLib.Web.Reflection
{
    public static class WebUtils
    {
        // Null safe
        public static void WriteLiteralArray(this TextWriter output, object[] array)
        {
            if (array == null)
                return;
            foreach (object item in array)
                output.WriteLine(item.ToString());
        }

        // Null safe
        public static void WriteLiteralLinq(this TextWriter output, IEnumerable array)
        {
            if (array == null)
                return;
            foreach (object item in array)
                output.WriteLine(item.ToString());
        }

        public static IEnumerable<PropertyDescriptor> Properties(this object obj)
        {
            return ReflectionCache.GetProperties(obj);
        }

        public static XElement AsXml(this object obj, string name = null)
        {
            IEnumerable<PropertyDescriptor> prop = WebUtils.Properties(obj);
            XElement result = new XElement(name ?? obj.GetType().Name);
            foreach (PropertyDescriptor desc in prop)
            {
                object propValue = WebUtils.GetPropertyValue(obj, desc.Name);
                if (propValue != null)
                    result.Add(new XAttribute(desc.Name, propValue));
            }
            return result;
        }

        public static object GetPropertyValue(this object obj, string property)
        {
            Guard.Check(!(obj is ExpandoObject));

            object value = null;
            // bool success = 
			   ReflectionCache.TryToExtractValueFromDescriptor(obj, property, out value);
            return value;
        }

        public static void SetPropertyValue(this object obj, string propertyName, object propertyValue)
        {
            if (propertyValue == null)
                return;

            Guard.Check(!(obj is ExpandoObject));

            PropertyInfo propertyInfo = GetProperty(obj, propertyName);
            if (propertyInfo == null)
                throw new ArgumentException(string.Format(
                    "An error has occurred setting property with the '{0}' name does not exist in {1}.",
                    propertyName, obj.GetType().Name));

            object value = null;
            if (propertyInfo.PropertyType == typeof(string))
            {
                value = propertyValue as string;
            }
            else if (propertyValue as string != null)
            {
                TypeConverter typeConverter = GetPropertyTypeConverter(propertyInfo);
                if (typeConverter != null)
                    value = typeConverter.ConvertFrom(null, CultureInfo.CurrentCulture, propertyValue);
                if (propertyInfo.PropertyType.IsEnum)
                    value = Enum.Parse(propertyInfo.PropertyType, propertyValue as string);

                if (value == null)
                {
                    typeConverter = TypeDescriptor.GetConverter(propertyInfo.PropertyType);
                    // Converter &lt; transforming
                    value = typeConverter.ConvertFromString(null, CultureInfo.GetCultureInfo("en-US"),
                                    propertyValue as string);
                }
            }
            else
            {
                if (propertyInfo.PropertyType == typeof(Int32?))
                    value = Convert.ToInt32(propertyValue);
                else if (propertyInfo.PropertyType == typeof(decimal?))
                    value = Convert.ToDecimal(propertyValue);
                else if (propertyInfo.PropertyType == typeof(double?))
                    value = Convert.ToDouble(propertyValue);
                else 
                    value = propertyValue;
            }

            // http://stackoverflow.com/questions/622664/what-is-immutability-and-why-should-i-worry-about-it
            if (propertyInfo.CanWrite)
                propertyInfo.SetValue(obj, value, null);
        }
       
        private static object TryGetPropertyValue(object obj, string propertyName, out bool success)
        {
            object value = null;
            success = ReflectionCache.TryToExtractValueFromDescriptor(obj, propertyName, out value);
            return value;
        }

        public static TypeConverter GetPropertyTypeConverter(System.Reflection.PropertyInfo propertyInfo)
        {
            foreach (object attribute in propertyInfo.GetCustomAttributes(typeof(TypeConverterAttribute), false))
            {
                TypeConverterAttribute attr = attribute as TypeConverterAttribute;
                if (!attr.IsDefaultAttribute())
                {
                    try
                    {
                        var converter = Activator.CreateInstance(Type.GetType(attr.ConverterTypeName)) 
                                        as TypeConverter;
                        return converter;
                    }
                    catch { }
                }
            }
            return null;
        }

        #region DX Property methods

        /* private static object GetCompositePropertyValue(object obj, string propertyName)
        {
            List<string> propertyNames = new List<string>(propertyName.Split(PropertyPathSeparator));
            propertyNames.Remove(string.Empty);
            object result = obj;
            foreach (string propName in propertyNames)
                result = GetPropertyValue(result, propName);
            return result;
        }  */

        public static PropertyInfo GetProperty(object obj, string propertyName)
        {
            PropertyInfo[] properties = obj.GetType().GetProperties();
            foreach (PropertyInfo property in properties)
            {
                if (string.Equals(property.Name, propertyName, StringComparison.OrdinalIgnoreCase))
                    return property;
            }
            return null;
        }

        public static PropertyDescriptor GetPropertyDesc(object obj, string propertyName)
        {
            IEnumerable<PropertyDescriptor> prop = ReflectionCache.GetProperties(obj);
            return Enumerable.Where(prop, (el) => el.Name == propertyName).First();
        }

        #endregion
    }
}
