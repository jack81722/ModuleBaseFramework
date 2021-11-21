using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Ero.Daos.Csv
{
    /// <summary>
    /// CSVEntity is a temporary cache of csv 
    /// </summary>
    public class CSVEntity : IEnumerable<KeyValuePair<string,string>>, IEnumerable, IDictionary<string, string>
    {
        private Dictionary<string, string> _rawData;

        public CSVEntity()
        {
            _rawData = new Dictionary<string, string>();
        }

        /// <summary>
        /// Get or set value of specific key
        /// </summary>
        public string this[string key]
        {
            get
            {
                if (_rawData.TryGetValue(key, out string value))
                    return value;
                return string.Empty;
            }
            set
            {
                if (_rawData.ContainsKey(key))
                {
                    _rawData[key] = value;
                    return;
                }
                _rawData.Add(key, value);
            }
        }

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            foreach (var pair in _rawData)
            {
                builder.Append(pair.Key);
                builder.Append(":");
                builder.Append(pair.Value);
                builder.AppendLine();
            }
            return builder.ToString();
        }

        public TValue ParseRow<TValue>(string header)
        {
            if(TryGetValue(header, out string valStr))
            {
                return (TValue)Parse(typeof(TValue), valStr);
            }
            return default(TValue);
        }

        public T ToObject<T>()
        {
            return GenericCSVConvert<T>.Convert(this);
        }

        public T ToObject<T>(ICSVEntityConverter<T> converter)
        {
            return converter.Convert(this);
        }

        public T ToObject<T>(ICSVConverter<T> converter)
        {
            string[] headers = Keys.ToArray();
            string[] values = Values.ToArray();
            return converter.Convert(headers, values);
        }

        #region -- IDicionary methods --
        public ICollection<string> Keys => _rawData.Keys;

        public ICollection<string> Values => _rawData.Values;

        public int Count => _rawData.Count;

        public bool IsReadOnly => false;

        public void Add(string key, string value)
        {
            _rawData.Add(key, value);
        }

        public bool ContainsKey(string key)
        {
            return _rawData.ContainsKey(key);
        }

        public bool Remove(string key)
        {
            return _rawData.Remove(key);
        }

        public bool TryGetValue(string key, out string value)
        {
            return _rawData.TryGetValue(key, out value);
        }

        public void Add(KeyValuePair<string, string> item)
        {
            ((IDictionary<string, string>)_rawData).Add(item);
        }

        public void Clear()
        {
            _rawData.Clear();
        }

        public bool Contains(KeyValuePair<string, string> item)
        {
            return ((IDictionary<string, string>)_rawData).Contains(item);
        }

        public void CopyTo(KeyValuePair<string, string>[] array, int arrayIndex)
        {
            ((IDictionary<string, string>)_rawData).CopyTo(array, arrayIndex);
        }

        public bool Remove(KeyValuePair<string, string> item)
        {
            return ((IDictionary<string, string>)_rawData).Remove(item);
        }

        public IEnumerator<KeyValuePair<string, string>> GetEnumerator()
        {
            return _rawData.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _rawData.GetEnumerator();
        }
        #endregion

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
}