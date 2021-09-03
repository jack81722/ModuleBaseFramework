using System;
using System.Collections;
using System.Collections.Generic;

namespace ModuleBased.AOP.Collections
{
    /// <summary>
    /// Collection of game module proxies
    /// </summary>
    /// <remarks>
    /// This collection will create and cache the proxy of game module.
    /// It would return proxy of interface while calling get module.
    /// </remarks>
    public class GameModuleProxyCollection : IGameModuleCollection
    {
        private Dictionary<Type, ModuleProxy> _proxies;
        private Dictionary<Type, IGameModule> _modules;
        private ILogger _logger;

        private IModuleProxyFactory _proxyFactory;

        public int Count => _modules.Count;

        public GameModuleProxyCollection(ILogger logger, IModuleProxyFactory proxyFactory)
        {
            _logger = logger;
            _proxies = new Dictionary<Type, ModuleProxy>();
            _modules = new Dictionary<Type, IGameModule>();
            _proxyFactory = proxyFactory;
        }

        #region -- Add module methods --
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

        public IGameModule AddModule(Type itfType, Type modType)
        {
            CheckModuleType(modType);
            IGameModule mod = (IGameModule)Activator.CreateInstance(modType);
            AddModule(itfType, mod);
            return mod;
        }

        public void AddModule(Type itfType, IGameModule mod)
        {
            CheckInterfaceType(itfType);
            Type modType = mod.GetType();
            if (!itfType.IsAssignableFrom(modType))
                throw new ArgumentException($"Module({modType.Name}) is not implemented by interface({itfType.Name}).");
            var proxy = WrapModule(itfType, mod);
            _proxies.Add(itfType, proxy);
            _modules.Add(itfType, mod);
            InvokeAddedEvent(itfType, mod);
        }
        #endregion

        #region -- Get module methods --
        public object GetModule(Type itfType)
        {
            CheckInterfaceType(itfType);
            _proxies.TryGetValue(itfType, out ModuleProxy mod);
            return Convert.ChangeType(mod.GetTransparentProxy(), itfType);
        }

        public TItf GetModule<TItf>() where TItf : class
        {
            Type itfType = typeof(TItf);
            _proxies.TryGetValue(itfType, out ModuleProxy mod);
            return mod.GetTransparentProxy() as TItf;
        }

        public bool TryGetModule<TItf>(out TItf itf) where TItf : class
        {
            Type itfType = typeof(TItf);
            bool result = _proxies.TryGetValue(itfType, out ModuleProxy mod);
            itf = mod.GetTransparentProxy() as TItf;
            return result;
        }

        public bool TryGetModule(Type itfType, out object mod)
        {
            bool result = _proxies.TryGetValue(itfType, out ModuleProxy module);
            if (module != null)
                mod = Convert.ChangeType(module.GetTransparentProxy(), itfType);
            else
                mod = null;

            return result;
        }

        public IEnumerable<KeyValuePair<Type, IGameModule>> GetAllModules()
        {
            return _modules;
        }

        public IEnumerable<Type> GetInterfaceTypes()
        {
            return _proxies.Keys;
        }
        #endregion

        private ModuleProxy WrapModule(Type itfType, IGameModule module)
        {
            return _proxyFactory.CreateProxy(itfType, module, _logger);
        }

        #region -- Check methods --
        private void CheckInterfaceType(Type type)
        {
            if (type == null)
                throw new ArgumentNullException($"Interface type is null.");
            if (!type.IsInterface)
                throw new ArgumentException($"Type must be interface.");
        }

        private void CheckModuleType(Type type)
        {
            if (type == null)
                throw new ArgumentNullException($"Module type is null.");
            if (typeof(IGameModule).IsAssignableFrom(type))
                throw new ArgumentException($"Module type must be implemented by IGameModule");
        }

        private void InvokeAddedEvent(Type itfType, IGameModule mod)
        {
            try
            {
                mod.Modules = this;
            }
            catch (Exception e)
            {
                _logger.LogError(e);
            }
        }
        #endregion

        #region -- IEnumerable --
        public IEnumerator<IGameModule> GetEnumerator()
        {
            return _modules.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _modules.GetEnumerator();
        }

        
        #endregion
    }
}