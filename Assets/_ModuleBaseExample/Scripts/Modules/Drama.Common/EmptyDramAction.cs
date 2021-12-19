using ModuleBased.Rx;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ModuleBased.Example.Drama
{
    public sealed class EmptyDramAction : IDramaAction
    {
        public void Dispose() { }

        public bool IsFinished()
        {
            return true;
        }

        public bool IsPause()
        {
            return false;
        }

        public void ModifySpeed(float speed) { }

        public void Pause() { }

        public void Play() { }

        public void Resume() { }

        public IDisposable Subscribe(IObserver<object> observer)
        {
            try
            {
                observer.OnNext(new object());
                observer.OnCompleted();
            }
            catch (Exception e)
            {
                observer.OnError(e);
            }
            return new SingleAssignmentDisposable();
        }
    }
}
