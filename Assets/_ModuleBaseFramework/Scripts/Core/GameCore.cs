using ModuleBased.AOP;
using ModuleBased.AOP.Collections;
using ModuleBased.AOP.Factories;
using ModuleBased.DAO;
using ModuleBased.Models;
using ModuleBased.Rx;
using System;
using System.Collections;

namespace ModuleBased
{
    public partial class GameCore : IGameCore
    {
        #region -- IGameCore properties --
        public IGameModuleCollection Modules { get; }
        public IGameViewCollection Views { get; }
        public IGameDaoCollection Daos { get; }
        #endregion

        /// <summary>
        /// Flag of all modules initialized
        /// </summary>
        private bool _isInit;
        public bool IsInitialized => _isInit;

        /// <summary>
        /// Flag of all modules started
        /// </summary>
        private bool _isStart;
        public bool IsStarted => _isStart;

        /// <summary>
        /// Logger
        /// </summary>
        private ILogger _logger;

        #region -- Constructors --
        public GameCore(ILogger logger, bool enableProxy = false)
        {
            _logger = logger;
            Daos = new DefaultGameDaoCollection();

            if (enableProxy)
            {
                var proxyFactory = new DefaultModuleProxyFactory();
                Modules = new GameModuleProxyCollection(logger, proxyFactory);
            }
            else
                Modules = new DefaultGameModuleCollection(logger);
            Views = new DefaultGameViewCollection(logger, Modules);
        }

        public GameCore(ILogger logger, IModuleProxyFactory proxyFactory)
        {
            _logger = logger;
            Daos = new DefaultGameDaoCollection();
            if (proxyFactory != null)
            {
                Modules = new GameModuleProxyCollection(logger, proxyFactory);
            }
            else
                Modules = new DefaultGameModuleCollection(logger);
            Views = new DefaultGameViewCollection(logger, Modules);
        }
        #endregion

        #region -- Initialize methods --
        public void InstantiateCore()
        {
            if (_isInit)
                return;
            foreach (var mod in Modules)
            {
                AssignLogger(mod);
            }
            Observable
                .ForEach(Modules, (mod) => Observable.Progress<ProgressInfo>(mod.InitializeModule))
                .Batch(5)
                //.WhenAll()
                .Subscribe(
                    (infos) => { },
                    (error) => _logger.LogError(error),
                    () =>
                    {
                        AssignRequiredModules();
                        AssigneRequiredDaos();
                        _isInit = true;
                    }
                );
        }

        #endregion

        #region -- Start methods --
        public void StartCore()
        {
            if (_isStart)
                return;
            foreach (var mod in Modules)
            {
                StartModule(mod);
            }
            _isStart = true;
        }

        private void StartModule(IGameModule module)
        {
            try
            {
                module.StartModule();
            }
            catch (Exception e)
            {
                _logger.LogError(e);
            }
        }
        #endregion



        #region -- Add/Remove module methods --
        /// <summary>
        /// Added module by interface key
        /// </summary>
        public void AddModule(Type itfType, Type modType)
        {
            IGameModule mod = Modules.AddModule(itfType, modType);
            CheckInitializeAndStart(mod);
        }

        public void AddModule(Type itfType, IGameModule mod)
        {
            Modules.AddModule(itfType, mod);
            CheckInitializeAndStart(mod);
        }

        public void AddModule<TItf, TMod>() where TItf : class where TMod : IGameModule, TItf
        {
            IGameModule mod = Modules.AddModule<TItf, TMod>();
            CheckInitializeAndStart(mod);
        }

        public void AddModule<TItf>(IGameModule mod) where TItf : class
        {
            Modules.AddModule<TItf>(mod);
            CheckInitializeAndStart(mod);
        }

        private void CheckInitializeAndStart(IGameModule mod)
        {
            if (_isInit)
            {
                AssignLogger(mod);
                IObservable<ProgressInfo> observable = new ProgressObservable<ProgressInfo>(mod.InitializeModule);
                observable.Subscribe(
                    (p) => { },
                    (e) => { },
                    () =>
                    {

                        AssignRequiredModule(mod.GetType(), mod);
                        AssignRequiredDao(mod.GetType(), mod);
                    });


            }
            if (_isStart)
            {
                StartModule(mod);
            }
        }
        #endregion
    }
}