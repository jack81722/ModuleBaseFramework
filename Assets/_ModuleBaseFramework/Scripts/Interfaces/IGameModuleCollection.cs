using System;
using System.Collections;
using System.Collections.Generic;

namespace ModuleBased {
    public delegate void ModuleHandler(Type itfType, IGameModule mod);

    public interface IGameModuleCollection : IEnumerable, IEnumerable<IGameModule> {
        /// <summary>
        /// Event of module added
        /// </summary>
        event ModuleHandler OnModuleAdded;

        /// <summary>
        /// Added module by interface key
        /// </summary>
        void AddModule(Type itfType, Type instType);

        void AddModule(Type itfType, IGameModule mod);

        void AddModule<TItf, TMod>() where TItf : class where TMod : IGameModule, TItf;

        void AddModule<TItf>(IGameModule mod) where TItf : class;

        object GetModule(Type itfType);

        TItf GetModule<TItf>() where TItf : class;

        bool TryGetModule<TItf>(out TItf mod) where TItf : class;

        bool TryGetModule(Type itfType, out object mod);

        IEnumerable<Type> GetInterfaceTypes();
    }
}