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
    public static class ObjectConvert
    {
        public static object SetValue<T>(this object obj, string propertyName, T propertyValue,
            Action<Exception> OnError = null)
            where T : IConvertible
        {
            if (OnError != null && string.IsNullOrWhiteSpace(propertyName))
            {
                OnError(new ArgumentException("no property propertyValue"));
                return obj;
            }
            Guard.Check(obj != null && !string.IsNullOrWhiteSpace(propertyName), 
                        "error in SetValue property");

            if (obj is ExpandoObject)
            {
                (obj as IDictionary<string, object>)[propertyName] = propertyValue;
                return obj;
            }

            PropertyInfo pi = obj.GetType().GetProperty(propertyName);
            if (pi == null || !pi.CanWrite)
            {
                if (OnError != null)
                    OnError(new ArgumentException("no property " + propertyName));
                return obj;
            }

            pi.SetValue(obj,
                System.Convert.ChangeType(propertyValue, pi.PropertyType),
                null
            );
            return obj;
        }

        // Safe null values method
        public static T GetValue<T>(this object obj, string propertyName) // where T : IConvertible
        {
            if (obj == null)
                return default(T);

            if (obj is ExpandoObject || obj is IDictionary<string, object>)
            {
                var dict = obj as IDictionary<string, object>;
                object value = null;
                dict.TryGetValue(propertyName, out value);
                return (T)value;
            }

            PropertyDescriptor pi = ReflectionCache.GetProperty(obj, propertyName);
            if (pi == null)    //  || !pi.CanRead // .CanWrite
                return default(T);

            object valueProp = obj.GetPropertyValue(propertyName);
            if (valueProp == null)
                return default(T);

            var type = typeof(T);
            if (type.IsValueType)
            {
                var nullType = Nullable.GetUnderlyingType(type);
                if (nullType != null)
                    return (T)System.Convert.ChangeType(valueProp, nullType);
            }
            else
                if (valueProp.GetType().IsValueType)
                {
                    return (T)System.Convert.ChangeType(valueProp, type);
                }
            else if (type.IsClass)
                return (T)valueProp;

            return (T)System.Convert.ChangeType(valueProp, pi.PropertyType);
        }
        
        #region Change type convert
        
        public static T ChangeType<T>(object value)
        {
            return (T)ChangeTypeObj(value, typeof(T));
        }

        public static Type RemoveNullableFromType(Type type)
        {
            return Nullable.GetUnderlyingType(type) ?? type;
        }

        // https://raw.githubusercontent.com/Microsoft/referencesource/master/System.Web.DynamicData/DynamicData/Util/Misc.cs
        public static object ChangeTypeObj(this object value, Type type)
        {
            //   public static Object ChangeType(Object value, TypeCode typeCode, IFormatProvider provider)
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

        #endregion

    }
}
