using System.Collections;
using System.Collections.Generic;

namespace ModuleBased {
    public interface IGameView {
        ILogger Logger { get; set; }
        IGameModuleCollection Modules { get; set; }

        void InitializeView();
    }
}