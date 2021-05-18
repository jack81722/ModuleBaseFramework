using ModuleBased.AOP;
using ModuleBased.AOP.Factories;
using ModuleBased.DAO;
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

        private ILogger _logger;

        /// <summary>
        /// Collection of modules
        /// </summary>
        public IGameModuleCollection Modules { get; private set; }

        /// <summary>
        /// Collection of views
        /// </summary>
        public IGameViewCollection Views { get; private set; }

        /// <summary>
        /// Collection of data access objects
        /// </summary>
        public IGameDaoCollection Daos { get; private set; }

        public ScriptableObject[] CustomDaos;

        /// <summary>
        /// Setup singleton, core, modules, and views
        /// </summary>
        private void Awake() {
            InitializeSingleton();
            InitializeCore();
            /*
             * In online game, dao layer must be waiting connection.
             * Module and views should wait dao initialization.
             */
            AddAndInitializeDaos();
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
            _logger = new UniLogger();
            IModuleProxyFactory proxyFactory = new NoneModuleProxyFactory();
            var ddaoc = new DefaultGameDaoCollection();
            Daos = ddaoc;
            var dgmc = new DefaultGameModuleCollection(Daos, _logger, proxyFactory);
            Modules = dgmc;
            var dgvc = new DefaultGameViewCollection(_logger, dgmc);
            Views = dgvc;
        }

        private void AddAndInitializeDaos() {
            foreach(var dao in CustomDaos) {
                Type daoType = dao.GetType();
                if (daoType.IsDefined(typeof(UniDaoAttribute))) {
                    var attr = daoType.GetCustomAttribute<UniDaoAttribute>();
                    Daos.AddDao(attr.ItfType, dao);
                }
            }
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