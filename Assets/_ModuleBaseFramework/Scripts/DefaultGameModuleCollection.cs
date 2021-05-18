using ModuleBased.AOP;
using ModuleBased.DAO;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace ModuleBased {
    public class DefaultGameModuleCollection : IGameModuleCollection {
        private Dictionary<Type, IGameModule> _modules;

        private IGameDaoCollection _daos;

        private ILogger _logger;

        /// <summary>
        /// Proxy factory of modules
        /// </summary>
        private IModuleProxyFactory _proxyFactory;

        private bool _isInit;
        private bool _isStart;

        public delegate void ModuleHandler(IGameModule module);
        public event ModuleHandler OnAddModule;

        public DefaultGameModuleCollection(IGameDaoCollection daos, ILogger logger, IModuleProxyFactory proxyFactory) {
            _daos = daos;
            _logger = logger;
            _proxyFactory = proxyFactory;
            _modules = new Dictionary<Type, IGameModule>();
        }

        #region -- Add module methods --
        public TItf AddModule<TItf, TMod>()
            where TItf : class
            where TMod : IGameModule, TItf {
            return AddModule(typeof(TItf), typeof(TMod)) as TItf;
        }

        public void AddModule<TInterface>(IGameModule mod) where TInterface : class {
            Type itfType = typeof(TInterface);
            Type modType = mod.GetType();
            if (!itfType.IsAssignableFrom(modType))
                throw new ArgumentException("Module is not implemented by interface.");
            _modules.Add(itfType, mod);
            CheckStartAndInitialize(mod);
        }

        public IGameModule AddModule(Type itfType, Type modType) {
            CheckInterfaceType(itfType);
            CheckModuleType(modType);
            IGameModule mod = (IGameModule)Activator.CreateInstance(modType);
            var proxy = _proxyFactory.CreateProxy(itfType, mod) as IGameModule;
            if (proxy == null) {
                _logger.LogError($"Create proxy of module failed.");
                return null;
            }
            _modules.Add(itfType, proxy);
            CheckStartAndInitialize(mod);
            return mod;
        }

        public void AddModule(Type itfType, IGameModule mod) {
            CheckInterfaceType(itfType);
            Type modType = mod.GetType();
            if (!itfType.IsAssignableFrom(modType))
                throw new ArgumentException($"Module({modType.Name}) is not implemented by interface({itfType.Name}).");
            var proxy = _proxyFactory.CreateProxy(itfType, mod);
            var proxyMod = proxy as IGameModule;
            if (proxyMod == null) {
                _logger.LogError($"Create proxy of module failed.");
            } else {
                _modules.Add(itfType, proxyMod);
                CheckStartAndInitialize(proxyMod);
            }
        }
        #endregion


        #region -- Get module methods --
        public IGameModule GetModule(Type itfType) {
            CheckInterfaceType(itfType);
            _modules.TryGetValue(itfType, out IGameModule mod);
            return mod;
        }

        public TItf GetModule<TItf>() where TItf : class {
            Type itfType = typeof(TItf);
            _modules.TryGetValue(itfType, out IGameModule mod);
            return mod as TItf;
        }

        public bool TryGetModule<TItf>(out TItf itf) where TItf : class {
            Type itfType = typeof(TItf);
            bool result = _modules.TryGetValue(itfType, out IGameModule mod);
            itf = mod as TItf;
            return result;
        }

        public bool TryGetModule(Type itfType, out IGameModule mod) {
            bool result = _modules.TryGetValue(itfType, out mod);
            return result;
        }
        #endregion

        public void InitializeModules() {
            if (_isInit)
                return;
            foreach (var mod in _modules.Values) {
                try {
                    mod.OnModuleInitialize();
                }
                catch (Exception e) {
                    _logger.LogError(e);
                }
            }
            AssignRequiredModules();
            AssigneRequiredDaos();
            _isInit = true;
        }

        public void StartModules() {
            if (_isStart)
                return;
            foreach (var mod in _modules.Values) {
                try {
                    mod.OnModuleStart();
                }
                catch (Exception e) {
                    _logger.LogError(e);
                }
            }
            _isStart = true;
        }

        #region -- Assign required element methods --
        private void AssignRequiredModules() {
            foreach (var modInst in _modules.Values) {
                Type modType = modInst.GetType();
                AssignRequiredModule(modType, modInst);
            }
        }

        private void AssignRequiredModule(Type modType, IGameModule modInst) {
            MemberInfo[] members = modType.GetMembers(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

            foreach (var member in members) {
                if (member.IsDefined(typeof(RequireModuleAttribute))) {
                    if (member.MemberType == MemberTypes.Field) {
                        FieldInfo field = (FieldInfo)member;
                        Type reqType = field.FieldType;
                        field.SetValue(modInst, GetModule(reqType));
                    }
                    if (member.MemberType == MemberTypes.Property) {
                        PropertyInfo prop = (PropertyInfo)member;
                        Type reqType = prop.PropertyType;
                        prop.SetValue(modInst, GetModule(reqType));
                    }
                }
            }
        }

        private void AssigneRequiredDaos() {
            foreach (var modInst in _modules.Values) {
                Type modType = modInst.GetType();
                AssignRequiredDao(modType, modInst);
            }
        }

        private void AssignRequiredDao(Type modType, IGameModule modInst) {
            MemberInfo[] members = modType.GetMembers(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            foreach (var member in members) {
                if (member.IsDefined(typeof(RequireDaoAttribute))) {
                    if (member.MemberType == MemberTypes.Field) {
                        FieldInfo field = (FieldInfo)member;
                        Type reqType = field.FieldType;
                        field.SetValue(modInst, _daos.GetDao(reqType));
                    }
                    if (member.MemberType == MemberTypes.Property) {
                        PropertyInfo prop = (PropertyInfo)member;
                        Type reqType = prop.PropertyType;
                        prop.SetValue(modInst, _daos.GetDao(reqType));
                    }
                }
            }
        }

        #endregion

        #region -- Check methods --
        private void CheckInterfaceType(Type type) {
            if (type == null)
                throw new ArgumentNullException($"Interface type is null.");
            if (!type.IsInterface)
                throw new ArgumentException($"Type must be interface.");
        }

        private void CheckModuleType(Type type) {
            if (type == null)
                throw new ArgumentNullException($"Module type is null.");
            if (typeof(IGameModule).IsAssignableFrom(type))
                throw new ArgumentException($"Module type must be implemented by IGameModule");
        }

        private void CheckStartAndInitialize(IGameModule mod) {
            if (_isInit) {
                mod.OnModuleInitialize();
                AssignRequiredModule(mod.GetType(), mod);
            }
            if (_isStart)
                mod.OnModuleStart();
            mod.Modules = this;
            OnAddModule?.Invoke(mod);
        }
        #endregion

        #region -- IEnumerable --
        public IEnumerator<IGameModule> GetEnumerator() {
            return _modules.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return _modules.Values.GetEnumerator();
        }
        #endregion
    }
}