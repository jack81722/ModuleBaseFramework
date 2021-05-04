using System;
using System.Collections;
using System.Collections.Generic;

namespace ModuleBased {
    public interface IGameCore {
        IGameModule AddModule(Type type);

        void AddModule(IGameModule module);

        TMod AddModule<TMod>() where TMod : IGameModule;

        void AddModule<TMod>(TMod mod) where TMod : IGameModule;

        IGameModule GetModule(Type type, bool inherit = false);

        TMod GetModule<TMod>(bool inherit = false) where TMod : IGameModule;

        bool TryGetModule<TMod>(out TMod mod, bool inherit = false) where TMod : IGameModule;

        bool TryGetModule(Type modType, out IGameModule mod, bool inherit = false);

        void InitializeModules();

        void StartModules();
    }
}