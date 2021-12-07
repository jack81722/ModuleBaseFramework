using ModuleBased.ForUnity;
using ModuleBased.Injection;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace ModuleBased.Example
{
    [InjectableFactory(typeof(IConfigModule))]
    public class ConfigModuleFactory : MonoBehaviour, IFactory
    {
        [SerializeField]
        private ConfigScriptableObject _configs;

        public object Create(object args)
        {
            var config = Instantiate(_configs);
            var configMod = new ConfigModule();
            foreach(var record in config.Records)
            {
                switch (record.Type) 
                {
                    case Record.TypeEnum.Bool:
                        configMod.Save(record.Key, (bool)record);
                        break;
                    case Record.TypeEnum.Float:
                        configMod.Save(record.Key, (float)record);
                        break;
                    case Record.TypeEnum.Int:
                        configMod.Save(record.Key, (int)record);
                        break;
                    case Record.TypeEnum.Folder:
                    case Record.TypeEnum.File:
                    case Record.TypeEnum.String:
                        configMod.Save(record.Key, (string)record);
                        break;

                }
            }
            return configMod;
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

            public void Unsubscribe<T>(string key, Action<T> onChanged)
            {
                var binding = SubjectBinding.NewBinding(key, onChanged);
                if (!_bindings.TryGetValue(key, out List<SubjectBinding> list))
                {
                    list = new List<SubjectBinding>();
                    _bindings.Add(key, list);
                }
                list.Remove(binding);
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

    
}
