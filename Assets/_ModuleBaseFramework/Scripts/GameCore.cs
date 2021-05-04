using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace ModuleBased {
    public class GameCore : IGameCore {
        private Dictionary<Type, IGameModule> _modules;
        private ILogger _logger;
        private bool _isInit;
        private bool _isStart;

        public GameCore(ILogger logger) {
            _modules = new Dictionary<Type, IGameModule>();
            _logger = logger;
        }

        public TMod AddModule<TMod>() where TMod : IGameModule {
            TMod mod = Activator.CreateInstance<TMod>();
            AddModule(mod);
            return mod;
        }

        public void AddModule(IGameModule mod) {
            CheckModuleInstance(mod);
            _modules.Add(mod.GetType(), mod);
            CheckStartAndInitialize(mod);
        }

        public void AddModule<TMod>(TMod mod) where TMod : IGameModule {
            CheckModuleInstance(mod);
            _modules.Add(typeof(TMod), mod);
            CheckStartAndInitialize(mod);
        }

        public IGameModule AddModule(Type modType) {
            CheckModuleType(modType);
            IGameModule mod = (IGameModule)Activator.CreateInstance(modType);
            _modules.Add(modType, mod);
            CheckStartAndInitialize(mod);
            return mod;
        }

        public TMod GetModule<TMod>(bool inherit = false) where TMod : IGameModule {
            if (inherit) {
                foreach(var pair in _modules) {
                    Type modType = typeof(TMod);
                    if (pair.Key == modType || pair.Key.IsSubclassOf(modType)) {
                        return (TMod)pair.Value;
                    }
                }
                return default(TMod);
            } 
            if(_modules.TryGetValue(typeof(TMod), out IGameModule mod)) {
                return (TMod)mod;
            }
            return default(TMod);
        }

        public IGameModule GetModule(Type modType, bool inherit = false) {
            if (!typeof(IGameModule).IsAssignableFrom(modType))
                throw new ArgumentException($"Type({modType}) is not implemented by IGameModule.");
            if (inherit) {
                foreach (var pair in _modules) {
                    if (pair.Key == modType || pair.Key.IsSubclassOf(modType)) {
                        return pair.Value;
                    }
                }
                return null;
            }
            if (_modules.TryGetValue(modType, out IGameModule mod)) {
                return mod;
            }
            return null;
        }

        public bool TryGetModule<TMod>(out TMod mod, bool inherit = false) where TMod : IGameModule {
            bool result = TryGetModule(typeof(TMod), out IGameModule m, inherit);
            if (result)
                mod = (TMod)m;
            else
                mod = default(TMod);
            return result;
        }

        public bool TryGetModule(Type modType, out IGameModule mod, bool inherit = false) {
            if (!typeof(IGameModule).IsAssignableFrom(modType))
                throw new ArgumentException($"Type({modType}) is not implemented by IGameModule.");
            mod = null;
            if (inherit) {
                foreach (var pair in _modules) {
                    if (pair.Key == modType || pair.Key.IsSubclassOf(modType)) {
                        mod = pair.Value;
                        return true;
                    }
                }
                return false;
            }
            if (_modules.TryGetValue(modType, out mod)) {
                return true;
            }
            return false;
        }

        public void InitializeModules() {
            foreach(var mod in _modules.Values) {
                try {
                    mod.OnModuleInitialize();
                }catch(Exception e) {
                    _logger.LogError(e);
                }
            }
            AssignRequiredModules();
            _isInit = true;
        }

        public void StartModules() {
            foreach(var mod in _modules.Values) {
                try {
                    mod.OnModuleStart();
                }catch(Exception e) {
                    _logger.LogError(e);
                }
            }
            _isStart = true;
        }

        private void AssignRequiredModules() {
            foreach(var pair in _modules) {
                Type modType = pair.Key;
                IGameModule modInst = pair.Value;
                AssignRequiredModule(modType, modInst);
            }
        }

        private void AssignRequiredModule(Type modType, IGameModule modInst) {
            MemberInfo[] members = modType.GetMembers(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            
            foreach (var member in members) {
                if (member.IsDefined(typeof(RequireModuleAttribte))) {
                    if (member.MemberType == MemberTypes.Field) {
                        FieldInfo field = (FieldInfo)member;
                        Type reqType = field.FieldType;
                        field.SetValue(modInst, GetModule(reqType, true));
                    }
                    if (member.MemberType == MemberTypes.Property) {
                        PropertyInfo prop = (PropertyInfo)member;
                        Type reqType = prop.PropertyType;
                        prop.SetValue(modInst, GetModule(reqType, true));
                    }
                }
            }
        }

        private void CheckModuleType(Type modType) {
            if (modType == null)
                throw new ArgumentNullException($"Module type is null.");
            if (typeof(IGameModule).IsAssignableFrom(modType))
                throw new ArgumentException($"Module type must be implemented by IGameModule");
        }

        private void CheckModuleInstance(IGameModule mod) {
            if (mod == null)
                throw new ArgumentNullException($"Module is null.");
        }

        private void CheckStartAndInitialize(IGameModule mod) {
            if (_isInit) {
                mod.OnModuleInitialize();
                AssignRequiredModule(mod.GetType(), mod);
            }
            if (_isStart)
                mod.OnModuleStart();
        }
    }
}