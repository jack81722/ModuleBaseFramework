using Cheap2.Plugin.TweenExt;
using DG.Tweening;
using ModuleBased.Rx;
using System;

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
            _disposable = _tween.ToObservable().Subscribe(
                (t) => subject.OnNext(t),
                (e) => subject.OnError(e),
                () =>
                {
                    subject.OnCompleted();
                });
        }

        public void Dispose()
        {
            subject.Dispose();
            _disposable.Dispose();
        }

        public bool IsFinished()
        {
            bool result = _tween.IsComplete();
            return result;
        }

        public bool IsPause()
        {
            bool result = _tween.IsActive() && !_tween.IsPlaying();
            return result;
        }

        public void ModifySpeed(float speed)
        {

            return;
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

        public IDisposable Subscribe(IObserver<object> observer)
        {
            return subject.Subscribe(observer);
        }
    }
}
