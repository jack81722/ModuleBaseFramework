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
        private float _timeScale = 1;
        private bool _isCompleted;
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
                _second -= Time.deltaTime * _timeScale;
                yield return null;
            }
        }

        public void Dispose() { }

        public bool IsCompleted()
        {
            return _isCompleted;
        }

        public bool IsPause()
        {
            return false;
        }

        public float TimeScale { get => _timeScale; set => _timeScale = value; }

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

        public void Complete()
        {
            MainThreadDispatcher.SendStopCoroutine(_coroutine);
            onCompleted();
        }

        #region -- Events --
        private void onCompleted()
        {
            if (_isCompleted)
                return;
            _isCompleted = true;
            _subject.OnCompleted();
        }
        #endregion
    }
}
