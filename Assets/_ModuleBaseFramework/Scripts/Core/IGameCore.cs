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
    }
}