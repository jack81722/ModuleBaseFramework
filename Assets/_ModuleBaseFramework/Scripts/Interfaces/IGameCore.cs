using System;
using System.Collections;
using System.Collections.Generic;

namespace ModuleBased {
    public interface IGameCore {
        IGameModuleCollection Modules { get; }

        IGameViewCollection Views { get; }
    }
}