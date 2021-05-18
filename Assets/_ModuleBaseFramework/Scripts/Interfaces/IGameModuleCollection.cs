using System;
using System.Collections;
using System.Collections.Generic;

namespace ModuleBased {
    public interface IGameModuleCollection : IEnumerable, IEnumerable<IGameModule> {
        IGameModule AddModule(Type itfType, Type instType);

        void AddModule(Type itfType, IGameModule mod);

        TItf AddModule<TItf, TMod>() where TItf : class where TMod : IGameModule, TItf;

        void AddModule<TItf>(IGameModule mod) where TItf : class;

        IGameModule GetModule(Type interfaceType);

        TItf GetModule<TItf>() where TItf : class;

        bool TryGetModule<TItf>(out TItf mod) where TItf : class;

        bool TryGetModule(Type itfType, out IGameModule mod);

        void InitializeModules();

        void StartModules();
    }
}