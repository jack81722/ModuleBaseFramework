using System;
using System.Collections;
using System.Collections.Generic;

namespace ModuleBased {
    public delegate void ModuleHandler(Type itfType, IGameModule mod);

    public interface IGameModuleCollection : IEnumerable, IEnumerable<IGameModule> {
        int Count { get; }

        /// <summary>
        /// Added module by interface key
        /// </summary>
        IGameModule AddModule(Type itfType, Type modType);

        void AddModule(Type itfType, IGameModule mod);

        IGameModule AddModule<TItf, TMod>() where TItf : class where TMod : IGameModule, TItf;

        void AddModule<TItf>(IGameModule mod) where TItf : class;

        object GetModule(Type itfType);

        TItf GetModule<TItf>() where TItf : class;

        bool TryGetModule<TItf>(out TItf mod) where TItf : class;

        bool TryGetModule(Type itfType, out object mod);

        IEnumerable<KeyValuePair<Type, IGameModule>> GetAllModules();

        IEnumerable<Type> GetInterfaceTypes();
    }

    public interface IReadOnlyGameModuleCollection
    {
        object GetModule(Type itfType);

        TItf GetModule<TItf>() where TItf : class;

        bool TryGetModule<TItf>(out TItf mod) where TItf : class;

        bool TryGetModule(Type itfType, out object mod);

        IEnumerable<Type> GetInterfaceTypes();
    }
}