using ModuleBased.Example.GameFlow;
using ModuleBased.ForUnity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ModuleBased
{
    public class Core : UniGameCore
    {
        protected override IFsmState<IGameCore> DefaultState()
        {
            return new MainPageState();
        }

    }
}