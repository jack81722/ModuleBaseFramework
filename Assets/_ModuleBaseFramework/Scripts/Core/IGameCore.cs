using ModuleBased.DAO;
using ModuleBased.Injection;
using System;
using System.Collections;
using System.Collections.Generic;

namespace ModuleBased
{
    public interface IGameCore
    {
        void Launch(IFsmState<IGameCore> state = null);

        IEnumerator InitializeAll();
        void Initialize(IEnumerable<Contraction> contractions);

        Contraction Add(Type contractType, object identity = null);

        void Remove(Type contractType, object identity = null);

        object Get(Type contractType, object identity = null);

        void Destroy(Type contractType, object identity = null);

        void Inject(object target);
    }

}