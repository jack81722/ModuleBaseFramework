using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Cheap2.Plugin.TweenExt
{
    public class TweenObservable : IObservable<Tween>
    {
        private Tween _tween;
        private List<IObserver<Tween>> _observers;

        public TweenObservable(Tween tween)
        {
            _tween = tween;
            _observers = new List<IObserver<Tween>>();
            _tween.onComplete += onCompleted;
            _tween.onUpdate += () => onNext(_tween);
        }

        private void onNext(Tween tween)
        {
            foreach (var observer in _observers)
            {
                try
                {
                    observer.OnNext(tween);
                }
                catch (Exception e)
                {
                    observer.OnError(e);
                }
            }
        }

        private void onCompleted()
        {
            foreach (var observer in _observers)
            {
                try
                {
                    observer.OnCompleted();
                }
                catch (Exception e)
                {
                    observer.OnError(e);
                }
            }
        }

        public IDisposable Subscribe(IObserver<Tween> observer)
        {
            _observers.Add(observer);
            return EmptyDisposable.Singleton;
        }
    }
}