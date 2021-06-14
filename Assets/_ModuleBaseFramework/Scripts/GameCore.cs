using ModuleBased.AOP;
using ModuleBased.AOP.Collections;
using ModuleBased.AOP.Factories;
using ModuleBased.DAO;
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

        /// <summary>
        /// Flag of all modules started
        /// </summary>
        private bool _isStart;

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
            Modules.OnModuleAdded += Listen_OnModuleAdded;
            Views = new DefaultGameViewCollection(logger, Modules);
        }

        public GameCore(ILogger logger, IModuleProxyFactory proxyFactory)
        {
            _logger = logger;
            Daos = new DefaultGameDaoCollection();
            if(proxyFactory != null)
            {
                Modules = new GameModuleProxyCollection(logger, proxyFactory);
            }else
                Modules = new DefaultGameModuleCollection(logger);
            Modules.OnModuleAdded += Listen_OnModuleAdded;
            Views = new DefaultGameViewCollection(logger, Modules);
        }
        #endregion

        #region -- Initialize methods --
        public void InitializeCore()
        {
            if (_isInit)
                return;
            foreach (var mod in Modules)
            {
                AssignLogger(mod);
                InitializeModule(mod);
            }
            AssignRequiredModules();
            AssigneRequiredDaos();
            _isInit = true;
        }

        private void InitializeModule(IGameModule module)
        {
            try
            {
                module.OnModuleInitialize();
            }
            catch (Exception e)
            {
                _logger.LogError(e);
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
                module.OnModuleStart();
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

        #region -- Listen event methods --
        private void Listen_OnModuleAdded(Type itfType, IGameModule mod)
        {   
            if (_isInit)
            {
                AssignLogger(mod);
                InitializeModule(mod);
                AssignRequiredModule(mod.GetType(), mod);
                AssignRequiredDao(mod.GetType(), mod);
            }
            if (_isStart)
            {
                StartModule(mod);
            }
        }
        #endregion
    }
}