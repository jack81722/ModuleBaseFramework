using ModuleBased.AOP;
using ModuleBased.DAO;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace ModuleBased
{
    public class DefaultGameModuleCollection : IGameModuleCollection
    {
        private Dictionary<Type, IGameModule> _modules;
        private ILogger _logger;

        public DefaultGameModuleCollection(ILogger logger)
        {
            _logger = logger;
            _modules = new Dictionary<Type, IGameModule>();
        }

        #region -- Add module methods --
        public IGameModule AddModule<TItf, TMod>()
            where TItf : class
            where TMod : IGameModule, TItf
        {
            return (TMod)AddModule(typeof(TItf), typeof(TMod));
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
            _modules.Add(itfType, mod);
            InvokeAddedEvent(itfType, mod);
        }
        #endregion

        #region -- Get module methods --
        public object GetModule(Type itfType)
        {
            CheckInterfaceType(itfType);
            _modules.TryGetValue(itfType, out IGameModule mod);
            return mod;
        }

        public TItf GetModule<TItf>() where TItf : class
        {
            Type itfType = typeof(TItf);
            _modules.TryGetValue(itfType, out IGameModule mod);
            return mod as TItf;
        }

        public bool TryGetModule<TItf>(out TItf itf) where TItf : class
        {
            Type itfType = typeof(TItf);
            bool result = _modules.TryGetValue(itfType, out IGameModule mod);
            itf = mod as TItf;
            return result;
        }

        public bool TryGetModule(Type itfType, out object mod)
        {
            bool result = _modules.TryGetValue(itfType, out IGameModule module);
            mod = module;
            return result;
        }

        public IEnumerable<KeyValuePair<Type, IGameModule>> GetAllModules()
        {
            return _modules;
        }

        public IEnumerable<Type> GetInterfaceTypes()
        {
            return _modules.Keys;
        }
        #endregion

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