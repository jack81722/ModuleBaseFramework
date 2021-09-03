using ModuleBased.AOP;
using ModuleBased.AOP.Collections;
using ModuleBased.AOP.Factories;
using ModuleBased.DAO;
using ModuleBased.Models;
using ModuleBased.Rx;
using System;
using System.Reflection;

namespace ModuleBased
{
    public class GameCore : IGameCore
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
            IObservable<ProgressInfo>[] initProgs = new IObservable<ProgressInfo>[Modules.Count];
            int i = 0;
            foreach (var mod in Modules)
            {
                AssignLogger(mod);
                initProgs[i++] = new ProgressObservable<ProgressInfo>(mod.InitializeModule);
            }
            initProgs.WhenAll()
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

        #region -- Assign required element methods --
        private void AssignLogger(IGameModule module)
        {
            module.Logger = _logger;
        }

        private void AssignRequiredModules()
        {
            foreach (var modInst in Modules)
            {
                Type modType = modInst.GetType();
                AssignRequiredModule(modType, modInst);
            }
        }

        private void AssignRequiredModule(Type modType, IGameModule modInst)
        {
            MemberInfo[] members = modType.GetMembers(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

            foreach (var member in members)
            {
                if (member.IsDefined(typeof(RequireModuleAttribute)))
                {
                    if (member.MemberType == MemberTypes.Field)
                    {
                        FieldInfo field = (FieldInfo)member;
                        Type reqType = field.FieldType;
                        field.SetValue(modInst, Modules.GetModule(reqType));
                    }
                    if (member.MemberType == MemberTypes.Property)
                    {
                        PropertyInfo prop = (PropertyInfo)member;
                        Type reqType = prop.PropertyType;
                        prop.SetValue(modInst, Modules.GetModule(reqType));
                    }
                }
            }
        }

        private void AssigneRequiredDaos()
        {
            foreach (var modInst in Modules)
            {
                Type modType = modInst.GetType();
                AssignRequiredDao(modType, modInst);
            }
        }

        private void AssignRequiredDao(Type modType, IGameModule modInst)
        {
            MemberInfo[] members = modType.GetMembers(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            foreach (var member in members)
            {
                if (member.IsDefined(typeof(RequireDaoAttribute)))
                {
                    if (member.MemberType == MemberTypes.Field)
                    {
                        FieldInfo field = (FieldInfo)member;
                        Type reqType = field.FieldType;
                        field.SetValue(modInst, Daos.GetDao(reqType));
                    }
                    if (member.MemberType == MemberTypes.Property)
                    {
                        PropertyInfo prop = (PropertyInfo)member;
                        Type reqType = prop.PropertyType;
                        prop.SetValue(modInst, Daos.GetDao(reqType));
                    }
                }
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