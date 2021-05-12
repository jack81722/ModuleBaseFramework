using ModuleBased.AOP;
using ModuleBased.AOP.Factories;
using System;
using System.Reflection;
using UnityEngine;

namespace ModuleBased.ForUnity {
    public class UniGameCore : MonoBehaviour, IGameCore {
        /// <summary>
        /// Singleton of game core
        /// </summary>
        private static UniGameCore _singleton;
        public static UniGameCore Singleton {
            get {
                if(_singleton == null) {
                    _singleton = FindObjectOfType<UniGameCore>();
                    if(_singleton == null) {
                        throw new NullReferenceException("UniGameCore must be place in scene.");
                    }
                }
                return _singleton;
            }
        }

        /// <summary>
        /// Internal core
        /// </summary>
        private IGameCore _core;

        private void Awake() {
            InitializeSingleton();
            InitializeCore();
            AddAndInitializeModules();
        }
        private void InitializeSingleton() {
            // singleton
            if (_singleton == null)
                _singleton = this;
            if (_singleton != this) {
                Destroy(gameObject);
            }
        }

        private void InitializeCore() {
            // initialize core
            ILogger logger = new UniLogger();
            IModuleProxyFactory proxyFactory = new NoneModuleProxyFactory();
            _core = new GameCore(logger, proxyFactory);
        }

        private void AddAndInitializeModules() {
            // add modules
            IGameModule[] modules = GetComponentsInChildren<IGameModule>();
            foreach (var module in modules) {
                var attr = module.GetType().GetCustomAttribute<ModuleItfAttribute>();
                if (attr != null)
                    _core.AddModule(attr.ItfType, module);
            }
            _core.InitializeModules();
        }

        private void Start() {
            StartModules();
        }

        #region -- IGameCore --
        public IGameModule AddModule(Type itfType, Type modType) {
            var mod = _core.AddModule(itfType, modType);
            StoreModule(mod);
            return mod;
        }

        public void AddModule(Type itfType, IGameModule mod) {
            _core.AddModule(itfType, mod);
            StoreModule(mod);
        }

        public TItf AddModule<TItf, TMod>()
            where TItf : class
            where TMod : IGameModule, TItf {
            var itf = _core.AddModule<TItf, TMod>();
            StoreModule(itf);
            return itf;
        }

        public void AddModule<TItf>(IGameModule mod) where TItf : class {
            _core.AddModule<TItf>(mod);
            StoreModule(mod);
        }

        public IGameModule GetModule(Type itfType) {
            return _core.GetModule(itfType);
        }

        public TItf GetModule<TItf>() where TItf : class {
            return _core.GetModule<TItf>();
        }

        public bool TryGetModule<TItf>(out TItf mod) where TItf : class {
            return _core.TryGetModule<TItf>(out mod);
        }

        public bool TryGetModule(Type itfType, out IGameModule mod) {
            return _core.TryGetModule(itfType, out mod);
        }


        public void StartModules() {
            _core.StartModules();
        }


        public void InitializeModules() {
            _core.InitializeModules();
        }

        #endregion
        /// <summary>
        /// Store the module under the game core
        /// </summary>
        /// <param name="module"></param>
        private void StoreModule(object module) {
            Type modType = module.GetType();
            if (modType.IsSubclassOf(typeof(MonoBehaviour))) {
                ((MonoBehaviour)module).transform.parent = transform;
            }
        }

    }

    [AttributeUsage(AttributeTargets.Class)]
    public class ModuleItfAttribute : Attribute {
        public Type ItfType { get; }
        public ModuleItfAttribute(Type itfType) {
            ItfType = itfType;
        }
    }
}