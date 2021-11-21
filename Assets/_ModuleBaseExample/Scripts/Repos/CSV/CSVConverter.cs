using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using UnityEngine;

namespace Ero.Daos.Csv
{
    public class GenericCSVConverter<T> : ICSVConverter<T>, ICSVEntityConverter<T>
    {
        private static readonly Type csvPropAttr = typeof(CSVPropertyAttribute);

        private Dictionary<string, FieldInfo> _fieldDict = new Dictionary<string, FieldInfo>();
        private Dictionary<string, PropertyInfo> _propDict = new Dictionary<string, PropertyInfo>();

        public GenericCSVConverter()
        {
            Type targetType = typeof(T);
            CacheFields(targetType);
            CacheProps(targetType);
        }

        private void CacheFields(Type targetType)
        {
            var fields = targetType.GetFields(BindingFlags.Public | BindingFlags.Instance);
            foreach (var field in fields)
            {
                string fieldName = string.Empty;
                if (field.IsDefined(csvPropAttr))
                {
                    var attr = field.GetCustomAttribute<CSVPropertyAttribute>();
                    fieldName = attr.Name;
                }
                if (string.IsNullOrEmpty(fieldName))
                {
                    fieldName = field.Name;
                }
                _fieldDict.Add(fieldName, field);
            }
        }

        private void CacheProps(Type targetType)
        {
            var props = targetType.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (var prop in props)
            {
                string propName = string.Empty;
                if (prop.IsDefined(csvPropAttr))
                {
                    var attr = prop.GetCustomAttribute<CSVPropertyAttribute>();
                    propName = attr.Name;
                }
                if (string.IsNullOrEmpty(propName))
                {
                    propName = prop.Name;
                }
                _propDict.Add(propName, prop);
            }
        }

        /// <summary>
        /// Convert raw data of csv to specific type
        /// </summary>
        public T Convert(string[] headers, string[] values)
        {
            Type targetType = typeof(T);
            object target = Activator.CreateInstance(targetType);
            int count = Mathf.Min(headers.Length, values.Length);
            for (int i = 0; i < count; i++)
            {
                string header = headers[i];
                string value = values[i];
                if (TrySetField(ref target, header, value))
                    continue;
                if (TrySetProp(ref target, header, value))
                    continue;
            }
            return (T)target;
        }

        /// <summary>
        /// Convert entity to specific type
        /// </summary>
        public T Convert(CSVEntity entity)
        {
            Type targetType = typeof(T);
            object target = Activator.CreateInstance(targetType);
            foreach (var row in entity)
            {
                string header = row.Key;
                string value = row.Value;
                if (TrySetField(ref target, header, value))
                    continue;
                if (TrySetProp(ref target, header, value))
                    continue;
            }
            return (T)target;
        }

        private bool TrySetField(ref object target, string header, string value)
        {
            if (!_fieldDict.TryGetValue(header, out FieldInfo fieldInfo))
                return false;
            Type fieldType = fieldInfo.FieldType;
            if (fieldType.IsArray)
            {
                Type elementType = fieldType.GetElementType();
                string[] elem = value.Split(',');
                Array array_value = Array.CreateInstance(elementType, elem.Length);
                for (int j = 0; j < elem.Length; j++)
                {
                    array_value.SetValue(Parse(elementType, elem[j]), j);
                }
                fieldInfo.SetValue(target, array_value);
                return true;
            }
            else if (fieldType.IsEnum)
            {
                fieldInfo.SetValue(target, Enum.Parse(fieldType, value.ToString()));
                return true;
            }
            fieldInfo.SetValue(target, Parse(fieldType, value));
            return true;
        }

        private bool TrySetProp(ref object target, string header, string value)
        {
            if (!_propDict.TryGetValue(header, out PropertyInfo propInfo))
                return false;
            Type propType = propInfo.PropertyType;
            if (propType.IsArray)
            {
                Type elementType = propType.GetElementType();
                string[] elem = value.Split(',');
                Array array_value = Array.CreateInstance(elementType, elem.Length);
                for (int j = 0; j < elem.Length; j++)
                {
                    array_value.SetValue(Parse(elementType, elem[j]), j);
                }
                propInfo.SetValue(target, array_value);
                return true;
            }
            if (propType.IsEnum)
            {
                propInfo.SetValue(target, Enum.Parse(propType, value.ToString()));
                return true;
            }
            propInfo.SetValue(target, Parse(propType, value));
            return true;
        }

        /*
         * Reference : https://social.msdn.microsoft.com/Forums/en-US/d3a139b0-9c14-400d-94f9-440b64a0122a/convert-or-tryparse-from-string-to-t-generic-possible-work-around?forum=csharplanguage
         */
        /// <summary>
        /// Parse the string to specific type
        /// </summary>
        private static object Parse(Type type, string value)
        {
            TypeConverter converter =
               TypeDescriptor.GetConverter(type);

            try
            {
                var obj = converter.ConvertFromString(null,
                    CultureInfo.InvariantCulture, value);
                return obj;
            }
            catch (Exception e)
            {
                Debug.LogWarning($"Faild to convert csv : {e}");
                if (type.IsValueType)
                    return Activator.CreateInstance(type);
                return null;
            }

        }
    }

    /// <summary>
    /// CSVConvert will handle the convertion of csv entity or text
    /// </summary>
    public class GenericCSVConvert<T>
    {
        private static GenericCSVConverter<T> _converter;

        public static T Convert(string[] headers, string[] values)
        {
            if (_converter == null)
                _converter = new GenericCSVConverter<T>();
            return _converter.Convert(headers, values);
        }

        public static T Convert(CSVEntity entity)
        {
            if (_converter == null)
                _converter = new GenericCSVConverter<T>();
            return _converter.Convert(entity);
        }
    }

    /// <summary>
    /// Attribute of the field and property for csv convertible object
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class CSVPropertyAttribute : Attribute
    {
        public string Name { get; }
        public object DefaultValue { get; }

        public CSVPropertyAttribute(string name = "", object defaultValue = null)
        {
            Name = name;
            DefaultValue = defaultValue;
        }
    }


}