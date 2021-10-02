using System.Collections;
using System.Collections.Generic;

namespace ModuleBased {
    public interface IGameViewCollection : IEnumerable<IGameView>, IEnumerable {
        void AddView(IGameView view);
    }
}