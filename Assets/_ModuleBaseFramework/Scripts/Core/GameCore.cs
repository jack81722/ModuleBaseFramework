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
        public IGameModuleCollection Modules { get; private set; }
        public IGameViewCollection Views { get; private set; }
        public IGameDaoCollection Daos { get; private set; }
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
        public GameCore(ILogger logger)
        {
            _logger = logger;
            Daos = new DefaultGameDaoCollection();
            Modules = new DefaultGameModuleCollection();
            Views = new DefaultGameViewCollection();
        }
        
        public GameCore(ILogger logger, IGameModuleCollection modules)
        {
            _logger = logger;
            Daos = new DefaultGameDaoCollection();
            Modules = modules;
            Views = new DefaultGameViewCollection();
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
                .WhenAll()
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
            foreach(var view in Views)
            {
                view.Logger = _logger;
                view.Modules = Modules;
                view.InitializeView();
            }
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



    }
}