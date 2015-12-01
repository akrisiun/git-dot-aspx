using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Dynamic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;

namespace AiLib.Web.Reflection
{
    public static class ExpandoConvert
    {
        public static T Convert<T>(this ExpandoObject source, T result)
            where T : class
        {
            var ctor = result.GetType().GetConstructors().Single();

            IDictionary<string, object> dict = source;
            var parameters = ctor.GetParameters();
            var parameterValues = parameters.Select(p => dict[p.Name]).ToArray();

            //  Invoke(object[] parameters);
            return (T)ctor.Invoke(parameterValues);
        }

        public static void CloneTo<T>(this ExpandoObject dyn, ref T result) where T : class 
        {
            CloneToObj(dyn, (result as object));
        }

        public static void CloneToObj(this ExpandoObject dyn, object result)
        {
            if (result == null)
                return;

            IDictionary<string, object> dynProp = dyn;
            IEnumerable<PropertyDescriptor> listProperties = WebUtils.Properties(result);
            foreach (PropertyDescriptor prop in listProperties)
            {
                object value = dynProp.ContainsKey(prop.Name) ? dynProp[prop.Name] : null;
                if (value  != null && !DBNull.Value.Equals(value) && !prop.IsReadOnly)
                    prop.SetValue(result, value);
            }
        }

        public static ExpandoObject Muttable(this object obj)
        {
            IDictionary<string, object> result = new ExpandoObject();

            IEnumerable<PropertyDescriptor> listProperties = WebUtils.Properties(obj);
            // var ctor = result.GetType().GetConstructors().Single();

            foreach (PropertyDescriptor item in listProperties)
            {
                var prop = item.Name;
                result.Add(prop, item.GetValue(obj));
            }
            return result as ExpandoObject;
        }

        // Unsafe method
        public static object DynValue(this ExpandoObject obj, string key)
        {
            var dict = obj as IDictionary<string, object>;
            if (string.IsNullOrWhiteSpace(key) || !dict.ContainsKey(key))
                return null;
            return dict[key];
            // return (obj as IDictionary<string, object>)[key];
        }

        public static T DynValue<T>(this ExpandoObject obj, string key) where T : IConvertible
        {
            if (typeof(T).Equals(typeof(string)))           // most popular call
                // http://stackoverflow.com/questions/10387095/cast-int-to-generic-enum-in-c-sharp
                return (T)(object)System.Convert.ToString((obj as IDictionary<string, object>)[key]);

            // https://raw.githubusercontent.com/Microsoft/referencesource/master/mscorlib/system/convert.cs
            // var typeCode = (default(T) as IConvertible).GetTypeCode(); - failed
            T res = (T)System.Convert.ChangeType(
                  (obj as IDictionary<string, object>)[key] as IConvertible, typeof(T), provider: null);
            return res;
        }

        // Unsafe method
        public static ExpandoObject DynValueSet(this ExpandoObject obj, string key, object value)
        {
            (obj as IDictionary<string, object>)[key] = value;
            return obj;
        }

        public static T ChangeType<T>(object value)
        {
            return (T)ChangeTypeObj(value, typeof(T));
        }

        public static Type RemoveNullableFromType(Type type)
        {
            return Nullable.GetUnderlyingType(type) ?? type;
        } 

        // https://raw.githubusercontent.com/Microsoft/referencesource/c697a4b9782dc8c85c02344a847cb68163702aa7/System.Web.DynamicData/DynamicData/Util/Misc.cs
        public static object ChangeTypeObj(this object value, Type type)
        {
            //   public static Object ChangeType(Object value, TypeCode typeCode, IFormatProvider provider) {
            if (type is IConvertible)
            {
                var typeCode = (type as IConvertible).GetTypeCode();
                if (value == null && (typeCode == TypeCode.Empty || typeCode == TypeCode.String || typeCode == TypeCode.Object))
                {
                    return null;
                }
            }


            if (type == null)
            {
                throw new ArgumentNullException("type");
            }

            if (value == null)
            {
                if (TypeAllowsNull(type))
                {
                    return null;
                }
                return System.Convert.ChangeType(value, type, CultureInfo.CurrentCulture);
            }

            type = RemoveNullableFromType(type);

            if (value.GetType() == type)
            {
                return value;
            }

            TypeConverter converter = TypeDescriptor.GetConverter(type);
            if (converter.CanConvertFrom(value.GetType()))
            {
                return converter.ConvertFrom(value);
            }

            TypeConverter otherConverter = TypeDescriptor.GetConverter(value.GetType());
            if (otherConverter.CanConvertTo(type))
            {
                return otherConverter.ConvertTo(value, type);
            }

            throw new InvalidOperationException(String.Format(
                  CultureInfo.CurrentCulture, "cannot convert to {0}", type.FullName));
            //                DynamicDataResources.Misc_CannotConvertType, value.GetType(), type));
        }

        internal static bool TypeAllowsNull(Type type)
        {
            return Nullable.GetUnderlyingType(type) != null || !type.IsValueType;
        }

    }

  }
