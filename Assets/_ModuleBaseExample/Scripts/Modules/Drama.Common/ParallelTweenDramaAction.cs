using ModuleBased.Rx;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ModuleBased.Example.Drama
{
    public class ParallelDramaAction : IDramaAction
    {
        private bool _isPause;
        private List<IDramaAction> _actions = new List<IDramaAction>();
        

        public ParallelDramaAction(params IDramaAction[] actions)
        {
            _actions = new List<IDramaAction>(actions);
        }

        public void Dispose()
        {
            _actions.ForEach(act => act.Dispose());
        }

        public bool IsFinished()
        {
            return _actions.TrueForAll(act => act.IsFinished());
        }

        public bool IsPause()
        {
            return _isPause;
        }

        public void ModifySpeed(float speed)
        {
            _actions.ForEach(act => act.ModifySpeed(speed));
        }

        public void Pause()
        {
            _isPause = true;
            _actions.ForEach(act => act.Pause());
        }

        public void Play()
        {
            _actions.ForEach(act => act.Play());
        }

        public void Resume()
        {
            _isPause = false;
            _actions.ForEach(act => act.Resume());
        }

        public IDisposable Subscribe(IObserver<object> observer)
        {
            return _actions.WhenAll().Subscribe(observer);
        }
    }
}
