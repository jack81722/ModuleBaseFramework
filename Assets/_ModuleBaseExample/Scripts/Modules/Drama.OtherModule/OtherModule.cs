using ModuleBased.Injection;
using ModuleBased.Rx;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ModuleBased.Example.Drama.Other
{
    [Inject]
    public class OtherModule : MonoBehaviour, IEventInject
    {
        [Inject]
        private IDramaModule _dramaModule;

        public void OnInject()
        {
            _dramaModule.RegisterAction("Wait", (args) =>
            {
                float sec = float.Parse(args[0]);
                return Wait(sec);
            });
        }

        private IDramaAction Wait(float second)
        {
            return new WaitDramaAction(second);
        }
    }

    public class WaitDramaAction : IDramaAction
    {
        private float _second;
        private bool _isFinished;
        private Coroutine _coroutine;
        private Subject<object> _subject;

        public WaitDramaAction(float second)
        {
            _second = second;
            _subject = new Subject<object>();
        }

        private IEnumerator executeWait()
        {
            while(_second > 0)
            {
                _second -= Time.deltaTime;
                yield return null;
            }
        }

        public void Dispose() { }

        public bool IsFinished()
        {
            return _isFinished;
        }

        public bool IsPause()
        {
            return false;
        }


        public void Pause() { }

        public void Play()
        {
            _coroutine = MainThreadDispatcher.SendStartCoroutine(executeWait(), onCompleted);
        }

        public void Resume() { }

        public void Stop()
        {
            MainThreadDispatcher.SendStopCoroutine(_coroutine);
        }

        public IDisposable Subscribe(IObserver<object> observer)
        {
            return _subject.Subscribe(observer);
        }

        #region -- Events --
        private void onCompleted()
        {
            _subject.OnCompleted();
        }
        #endregion
    }
}
