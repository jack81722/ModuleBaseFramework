using Cheap2.Plugin.TweenExt;
using DG.Tweening;
using ModuleBased.Rx;
using System;
using UnityEngine;

namespace ModuleBased.Example.Drama
{
    public class TweenDramaAction : IDramaAction
    {
        IDisposable _disposable;
        Subject<object> subject = new Subject<object>();
        Tween _tween;

        public TweenDramaAction(Tween tween)
        {
            _tween = tween;
            _tween.SetAutoKill(false);
            _disposable = _tween.ToObservable().Subscribe(subject);
        }

        public float TimeScale { get => _tween.timeScale; set => _tween.timeScale = value; }

        public void Dispose()
        {
            subject.Dispose();
            _disposable.Dispose();
        }

        public bool IsCompleted()
        {
            bool result = _tween.IsComplete();
            return result;
        }

        public bool IsPause()
        {
            bool result = _tween.IsActive() && !_tween.IsPlaying();
            return result;
        }

        public void Pause()
        {
            _tween.Pause();
        }

        public void Play()
        {
            _tween.Play();
        }

        public void Resume()
        {
            if (_tween.IsActive() && !_tween.IsPlaying())
                _tween.Play();
        }

        public void Stop()
        {
            if (_tween.IsActive())
                _tween.Kill();
        }

        public void Complete()
        {
            Debug.Log($"complete tween :{_tween.target}");
            if (_tween == null)
                Debug.LogError("tween is null.");
            if(_tween != null && _tween.IsPlaying())
            {
                Debug.Log($"complete {_tween.target}");
                _tween.Complete();
            }
        }

        public IDisposable Subscribe(IObserver<object> observer)
        {
            return subject.Subscribe(observer);
        }
    }
}
