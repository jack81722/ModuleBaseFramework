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
        /// Collection of modules
        /// </summary>
        public IGameModuleCollection Modules { get; private set; }

        /// <summary>
        /// Collection of views
        /// </summary>
        public IGameViewCollection Views { get; private set; }

        /// <summary>
        /// Setup singleton, core, modules, and views
        /// </summary>
        private void Awake() {
            InitializeSingleton();
            InitializeCore();
            AddAndInitializeModules();
            AddAndInitializeViews();
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
            var dgmc = new DefaultGameModuleCollection(logger, proxyFactory);
            //dgmc.OnAddModule += StoreModule;
            Modules = dgmc;
            var dgvc = new DefaultGameViewCollection(logger, dgmc);
            Views = dgvc;
        }

        private void AddAndInitializeModules() {
            // add modules
            IGameModule[] modules = GetComponentsInChildren<IGameModule>();
            foreach (var module in modules) {
                var attr = module.GetType().GetCustomAttribute<ModuleItfAttribute>();
                if (attr != null)
                    Modules.AddModule(attr.ItfType, module);
            }
            Modules.InitializeModules();
        }

        private void AddAndInitializeViews() {
            
            IGameView[] views = GetComponentsInChildren<IGameView>();
            foreach(var view in views) {
                Views.AddView(view);
            }
            Views.InitializeViews();
        }

        private void Start() {
            Modules.StartModules();
        }
        
        /// <summary>
        /// Store the module under the game core transform
        /// </summary>
        /// <param name="module"></param>
        private void StoreModule(object module) {
            Type modType = module.GetType();
            if (modType.IsSubclassOf(typeof(MonoBehaviour))) {
                ((MonoBehaviour)module).transform.parent = transform;
            }
        }

    }

    /// <summary>
    /// Attribute of specific interface with module
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class ModuleItfAttribute : Attribute {
        public Type ItfType { get; }
        public ModuleItfAttribute(Type itfType) {
            ItfType = itfType;
        }
    }
}