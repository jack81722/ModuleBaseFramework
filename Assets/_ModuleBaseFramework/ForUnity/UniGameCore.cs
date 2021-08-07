using ModuleBased.AOP;
using ModuleBased.AOP.Factories;
using ModuleBased.DAO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ModuleBased.ForUnity
{
    public class UniGameCore : MonoBehaviour, IGameCore
    {
        #region -- Singleton --
        /// <summary>
        /// Singleton of game core
        /// </summary>
        private static UniGameCore _singleton;
        public static UniGameCore Singleton
        {
            get
            {
                if (_singleton == null)
                {
                    _singleton = FindObjectOfType<UniGameCore>();
                    if (_singleton == null)
                    {
                        throw new NullReferenceException("UniGameCore must be place in scene.");
                    }
                }
                return _singleton;
            }
        }
        #endregion

        #region -- Pirvate fields --

        private IGameCore _core;

        private ILogger _logger;
        #endregion

        #region -- IGameCore properties --
        public IGameModuleCollection Modules => _core.Modules;

        public IGameViewCollection Views => _core.Views;

        public IGameDaoCollection Daos => _core.Daos;
        #endregion

        #region -- Public fields --
        public bool UseProxy;
        public ScriptableObject[] CustomDaos;
        #endregion

        #region -- Unity APIs --
        /// <summary>
        /// Setup singleton, core, modules, and views
        /// </summary>
        private void Awake()
        {
            if (InitializeSingleton())
            {
                HandoverModules();
                InstatiateCore();
                /*
                 * In online game, dao layer must be waiting connection.
                 * Module and views should wait dao initialization.
                 */
                InstallDaos();
                SearchModules();
                SearchViews();
                InitializeAll();
            }
        }

        private void Start()
        {
            StartCore();
        }
        #endregion

        #region -- Initialize methods --
        private bool InitializeSingleton()
        {
            // singleton
            if (_singleton == null)
            {
                _singleton = this;
                return true;
            }
            if (_singleton != this)
            {
                Destroy(gameObject);
                return false;
            }
            return true;
        }

        /// <summary>
        /// Handover modules from game core to another game core
        /// </summary>
        public void HandoverModules()
        {
            //var cores = FindObjectsOfType<UniGameCore>();
            //foreach (var core in cores)
            //{
            //    if (core != this)
            //    {
            //        // handover modules
            //        foreach (var pair in core.Modules.GetAllModules())
            //        {
            //            AddModule(pair.Key, pair.Value);
            //        }
            //    }
            //}
        }

        public void InstatiateCore()
        {
            if (_logger == null)
                _logger = new UniLogger();
            if (_core == null)
                _core = new GameCore(_logger, UseProxy);
        }

        private void InstallDaos()
        {
            foreach (var dao in CustomDaos)
            {
                Type daoType = dao.GetType();
                if (daoType.IsDefined(typeof(UniDaoAttribute)))
                {
                    var attr = daoType.GetCustomAttribute<UniDaoAttribute>();
                    Daos.AddDao(attr.ItfType, dao);
                }
            }
        }

        private void SearchModules()
        {
            IEnumerable<IGameModule> modules = GetComponentsInChildren<IGameModule>();
            foreach (var module in modules)
            {
                var attr = module.GetType().GetCustomAttribute<UniModuleAttribute>();
                if (attr != null)
                    Modules.AddModule(attr.ItfType, module);
                StoreUniModule(module);
            }
        }

        private void SearchViews()
        {
            IGameView[] views = GetComponentsInChildren<IGameView>();
            foreach (var view in views)
            {
                Views.AddView(view);
            }
        }

        public void InitializeAll()
        {
            _core.InstatiateCore();
            Views.InitializeViews();
        }
        #endregion

        public void StartCore()
        {
            _core.StartCore();
        }

        #region -- Add module methods --
        /// <summary>
        /// Added module by interface key
        /// </summary>
        public void AddModule(Type itfType, Type modType)
        {
            _core.AddModule(itfType, modType);
        }

        public void AddModule(Type itfType, IGameModule mod)
        {
            _core.AddModule(itfType, mod);
        }

        public void AddModule<TItf, TMod>() where TItf : class where TMod : IGameModule, TItf
        {
            _core.AddModule<TItf, TMod>();
        }

        public void AddModule<TItf>(IGameModule mod) where TItf : class
        {
            _core.AddModule<TItf>(mod);
        }
        #endregion

        #region -- UniModule methods --
        /// <summary>
        /// Store the unimodule under the game core transform
        /// </summary>
        private void StoreUniModule(object module)
        {
            Type modType = module.GetType();
            if (modType.IsSubclassOf(typeof(MonoBehaviour)))
            {
                ((MonoBehaviour)module).transform.parent = transform;
            }
        }

        public void AddUniModule(object module)
        {
            Type modType = module.GetType();
            if (!typeof(IGameModule).IsAssignableFrom(modType))
            {
                _logger.LogError($"Module({modType.Name}) is not implemented by GameModule.");
                return;
            }
            var attr = module.GetType().GetCustomAttribute<UniModuleAttribute>();
            if (attr == null)
            {
                _logger.LogError($"Unknown interface of module.");
                return;
            }
            IGameModule gm = (IGameModule)module;
            Modules.AddModule(attr.ItfType, gm);

        }
        #endregion
    }

    /// <summary>
    /// Attribute of specific interface with module
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class UniModuleAttribute : Attribute
    {
        public Type ItfType { get; }
        public UniModuleAttribute(Type itfType)
        {
            ItfType = itfType;
        }
    }
}