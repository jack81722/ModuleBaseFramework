using ModuleBased.Rx;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ModuleBased.Example.Drama
{
    public sealed class EmptyDramAction : IDramaAction
    {
        Subject<object> _subject = new Subject<object>();

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

        public void Play() 
        {
            try
            {
                _subject.OnNext(new object());
                _subject.OnCompleted();
            }
            catch (Exception e)
            {
                _subject.OnError(e);
            }
        }

        public void Resume() { }

        public void Stop() { }

        public IDisposable Subscribe(IObserver<object> observer)
        {
            _subject.Subscribe(observer);
            return new SingleAssignmentDisposable();
        }
    }
}
