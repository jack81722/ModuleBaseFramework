using ModuleBased.ForUnity;
using ModuleBased.Injection;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using UnityEngine;

namespace ModuleBased.Example
{
    [InjectableFactory(typeof(IConfigModule))]
    public class ConfigModuleFactory : MonoBehaviour, IFactory
    {
        [SerializeField]
        List<ConfigStringRow> StringRowList;
        [SerializeField]
        List<ConfigFloatRow> FloatRowList;
        [SerializeField]
        List<ConfigIntRow> IntRowList;
        [SerializeField]
        List<ConfigObjectRow> ObjectRowList;
        [SerializeField]
        List<ConfigBoolRow> BoolRowList;

        public object Create(object args)
        {
            var configMod = new ConfigModule();
            AddCustomConfig(configMod, StringRowList);
            AddCustomConfig(configMod, FloatRowList);
            AddCustomConfig(configMod, IntRowList);
            AddCustomConfig(configMod, ObjectRowList);
            AddCustomConfig(configMod, BoolRowList);
            return configMod;
        }

        private void AddCustomConfig<T>(IConfigModule configMod, IEnumerable<ConfigRow<T>> rows)
        {
            foreach (var row in rows)
            {
                configMod.Create(row.Key, row.Value);
            }
        }

        private class ConfigModule : IConfigModule
        {
            private Dictionary<string, object> _table = new Dictionary<string, object>();
            private Dictionary<string, List<SubjectBinding>> _bindings = new Dictionary<string, List<SubjectBinding>>();

            public object Load(string key)
            {
                return _table[key];
            }

            public object LoadOrDefault(string key)
            {
                if (!_table.TryGetValue(key, out object value))
                {
                    _table.Add(key, null);
                    return null;
                }
                return value;
            }

            public object LoadOrDefault(string key, object fallback)
            {
                if (!_table.TryGetValue(key, out object value))
                {
                    _table.Add(key, value);
                    return fallback;
                }
                return value;
            }

            public void Create(string key, object value)
            {
                if (_table.ContainsKey(key))
                {
                    throw new InvalidOperationException("Duplicate key.");
                }
                _table.Add(key, value);
            }

            public void Update(string key, object value)
            {
                if (!_table.ContainsKey(key))
                {
                    throw new InvalidOperationException("Key not found.");
                }
                _table[key] = value;
            }

            public void Save(string key, object value)
            {
                try
                {
                    if (!_table.ContainsKey(key))
                    {
                        _table.Add(key, value);
                        return;
                    }
                    _table[key] = value;
                }
                finally
                {
                    Invoke(key, value);
                }
            }

            public bool ContainsKey(string key)
            {
                return _table.ContainsKey(key);
            }

            public void Subcribe<T>(string key, Action<T> onChanged)
            {
                var binding = SubjectBinding.NewBinding(key, onChanged);
                if (!_bindings.TryGetValue(key, out List<SubjectBinding> list))
                {
                    list = new List<SubjectBinding>();
                    _bindings.Add(key, list);
                }
                list.Add(binding);
                binding.Invoke(LoadOrDefault(key));
            }

            private void Invoke(string key, object value)
            {
                if (!_bindings.TryGetValue(key, out List<SubjectBinding> list))
                {
                    return;
                }
                foreach (var binding in list)
                {
                    try
                    {
                        binding.Invoke(value);
                    }
                    catch (Exception e)
                    {
                        Debug.LogError(e);
                    }
                }
            }
        }

        private struct SubjectBinding
        {
            private string _key;
            private Type _subjectType;
            private Action<object> _onChanged;

            public static SubjectBinding NewBinding<T>(string key, Action<T> onChanged)
            {
                return new SubjectBinding
                {
                    _key = key,
                    _subjectType = typeof(T),
                    _onChanged = (obj) => onChanged((T)obj)
                };
            }

            public void Invoke(object value)
            {
                _onChanged.Invoke(value);
            }
        }
    }

    public interface IConfigModule
    {
        void Create(string key, object value);

        void Update(string key, object value);

        void Save(string key, object value);

        object Load(string key);

        object LoadOrDefault(string key);

        object LoadOrDefault(string key, object fallback);

        bool ContainsKey(string key);

        void Subcribe<T>(string key, Action<T> onChanged);
    }

    public static class ConfigExtension
    {
        public static void Create<T>(this IConfigModule config, string key, T value)
        {
            config.Create(key, value);
        }

        public static bool TryCreate<T>(this IConfigModule config, string key, T value)
        {
            try
            {
                config.Create(key, value);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static void Update<T>(this IConfigModule config, string key, T value)
        {
            config.Update(key, value);
        }

        public static bool TryUpdate<T>(this IConfigModule config, string key, T value)
        {
            try
            {
                config.Update(key, value);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static void Save<T>(this IConfigModule config, string key, T value)
        {
            config.Save(key, value);
        }

        public static T Load<T>(this IConfigModule config, string key)
        {
            return (T)config.Load(key);
        }

        public static T LoadOrDefault<T>(this IConfigModule config, string key)
        {
            var value = config.LoadOrDefault(key);
            if (value == null)
            {
                return default(T);
            }
            return (T)value;
        }

        public static T LoadOrDefault<T>(this IConfigModule config, string key, T fallback)
        {
            var value = config.LoadOrDefault(key, fallback);
            if (value == null)
            {
                return fallback;
            }
            return (T)value;

        }
    }
}
