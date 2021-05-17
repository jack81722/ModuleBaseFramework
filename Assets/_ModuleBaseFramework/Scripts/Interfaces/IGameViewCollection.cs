using System.Collections;
using System.Collections.Generic;

namespace ModuleBased {
    public interface IGameViewCollection {
        void AddView(IGameView view);

        void InitializeViews();
    }
}