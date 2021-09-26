using System;
using System.Collections;
using System.Collections.Generic;

namespace ModuleBased.Proxy
{
    public class ProxyModuleCollection : IGameModuleCollection
    {
        private IGameModuleCollection _wrapped;
        private IProxyFactory _factory;

        public ProxyModuleCollection(IGameModuleCollection collection, IProxyFactory factory)
        {
            _wrapped = collection;
            _factory = factory;
        }

        public int Count => _wrapped.Count;

        public IGameModule AddModule(Type itfType, Type modType)
        {
            var instance = Activator.CreateInstance(modType);
            var proxy = _factory.CreateProxy(instance) as IGameModule;
            _wrapped.AddModule(itfType, proxy);
            return proxy;
        }

        public void AddModule(Type itfType, IGameModule mod)
        {
            var proxy = _factory.CreateProxy(mod) as IGameModule;
            _wrapped.AddModule(itfType, proxy);
        }

        public IGameModule AddModule<TItf, TMod>()
            where TItf : class
            where TMod : IGameModule, TItf
        {
            return AddModule(typeof(TItf), typeof(TMod));
        }

        public void AddModule<TItf>(IGameModule mod) where TItf : class
        {
            AddModule(typeof(TItf), mod);
        }

        public IEnumerable<KeyValuePair<Type, IGameModule>> GetAllModules()
        {
            return _wrapped.GetAllModules();
        }

        public IEnumerator<IGameModule> GetEnumerator()
        {
            return _wrapped.GetEnumerator();
        }

        public IEnumerable<Type> GetInterfaceTypes()
        {
            return _wrapped.GetInterfaceTypes();
        }

        public object GetModule(Type itfType)
        {
            return _wrapped.GetModule(itfType);
        }

        public TItf GetModule<TItf>() where TItf : class
        {
            return _wrapped.GetModule<TItf>();
        }

        public bool TryGetModule<TItf>(out TItf mod) where TItf : class
        {
            return _wrapped.TryGetModule(out mod);
        }

        public bool TryGetModule(Type itfType, out object mod)
        {
            return _wrapped.TryGetModule(itfType, out mod);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _wrapped.GetEnumerator();
        }
    }
}
