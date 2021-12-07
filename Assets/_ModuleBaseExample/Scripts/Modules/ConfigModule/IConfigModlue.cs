using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ModuleBased.Example
{
    [CommandGroup("config")]
    public interface IConfigModule
    {
        void Create(string key, object value);

        void Update(string key, object value);

        [Command]
        void Save(string key, object value);

        [Command]
        object Load(string key);

        object LoadOrDefault(string key);

        object LoadOrDefault(string key, object fallback);

        bool ContainsKey(string key);

        void Subcribe<T>(string key, Action<T> onChanged);

        void Unsubscribe<T>(string key, Action<T> onChanged);
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
