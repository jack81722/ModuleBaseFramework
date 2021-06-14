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

        private IGameCore _core;

        private ILogger _logger;

        #region -- IGameCore properties --
        public IGameModuleCollection Modules => _core.Modules;
        
        public IGameViewCollection Views => _core.Views;
        
        public IGameDaoCollection Daos => _core.Daos;
        #endregion

        #region -- Public fields --
        public bool UseProxy;
        public ScriptableObject[] CustomDaos;
        #endregion

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

        public void InitializeCore() {
            if(_logger == null)
                _logger = new UniLogger();
            if(_core == null)
                _core = new GameCore(_logger, UseProxy);
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
            _core.InitializeCore();
        }

        private void AddAndInitializeViews() {
            
            IGameView[] views = GetComponentsInChildren<IGameView>();
            foreach(var view in views) {
                Views.AddView(view);
            }
            Views.InitializeViews();
        }

        public void StartCore()
        {
            _core.StartCore();
        }

        private void Start() {
            StartCore();
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