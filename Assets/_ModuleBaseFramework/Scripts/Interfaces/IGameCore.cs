using ModuleBased.DAO;
using System;
using System.Collections;
using System.Collections.Generic;

namespace ModuleBased
{
    public interface IGameCore
    {
        /// <summary>
        /// Collection of modules
        /// </summary>
        IGameModuleCollection Modules { get; }

        ///// <summary>
        ///// Collection of module proxies
        ///// </summary>
        //IGameModuleCollection ModuleProxies { get; }

        /// <summary>
        /// Collection of views
        /// </summary>
        IGameViewCollection Views { get; }

        /// <summary>
        /// Collection of data access objects
        /// </summary>
        IGameDaoCollection Daos { get; }

        void InstantiateCore();

        void StartCore();

        /// <summary>
        /// Added module by interface key
        /// </summary>
        void AddModule(Type itfType, Type modType);

        void AddModule(Type itfType, IGameModule mod);

        void AddModule<TItf, TMod>() where TItf : class where TMod : IGameModule, TItf;

        void AddModule<TItf>(IGameModule mod) where TItf : class;
    }
}