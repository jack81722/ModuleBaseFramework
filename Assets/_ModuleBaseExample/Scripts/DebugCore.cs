using ModuleBased.Example.GameFlow;
using ModuleBased.ForUnity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ModuleBased
{
    public class DebugCore : UniGameCore
    {
        protected override IFsmState<IGameCore> DefaultState()
        {
            return new DebugState();
        }

    }

    public class DebugState : IFsmState<IGameCore>
    {
        public void OnEnter(IFsm<IGameCore> fsm)
        {
            
        }

        public void OnExit(IFsm<IGameCore> fms)
        {
         
        }

        public void OnUpdate(IFsm<IGameCore> fsm)
        {
            
        }
    }
}