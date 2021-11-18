using ModuleBased.ForUnity;
using ModuleBased.Injection;
using ModuleBased.Rx;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ModuleBased.Example.GameFlow
{

    public abstract class SceneFactory<TView> : MonoBehaviour, IFactory where TView : UnityEngine.Object
    {
        protected abstract string SceneName { get; }

        public object Create(object args)
        {
            var subject = new CreateViewSubject<TView>();
            var asyncOp = SceneManager.LoadSceneAsync(SceneName, LoadSceneMode.Additive);
            asyncOp.completed += (op) =>
            {
                if (op.isDone)
                {
                    var view = FindObjectOfType<TView>();
                    subject.Finish(view);
                }
            };
            return subject;
        }

        protected class CreateViewSubject<TViewImpl> : IObservable<TViewImpl>
        {
            [Inject]
            private IGameCore _core;

            private List<IObserver<TViewImpl>> _observers = new List<IObserver<TViewImpl>>();

            public void Finish(TViewImpl view)
            {
                try
                {
                    _core.Inject(view);
                }
                catch (Exception ex)
                {
                    foreach (var observer in _observers)
                    {
                        observer.OnError(ex);
                    }
                    return;
                }
                foreach (var observer in _observers)
                {
                    try
                    {
                        observer.OnNext(view);
                    }
                    catch (Exception e)
                    {
                        observer.OnError(e);
                    }
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

            public IDisposable Subscribe(IObserver<TViewImpl> observer)
            {
                _observers.Add(observer);
                return new SingleAssignmentDisposable();
            }
        }

    }
}