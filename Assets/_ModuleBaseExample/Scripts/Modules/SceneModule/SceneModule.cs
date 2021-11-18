using ModuleBased.ForUnity;
using ModuleBased.Injection;
using ModuleBased.Models;
using ModuleBased.Rx;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ModuleBased.Example.Scenes
{
    [Injectable(typeof(ISceneModule))]
    public class SceneModule : UniGameModule, ISceneModule
    {
        private HashSet<string> _loadedScenes;
        public Scene[] LoadingScenes;

        protected override IEnumerator OnInitializingModule(IProgress<ProgressInfo> progress)
        {
            _loadedScenes = new HashSet<string>();
            yield return null;
        }

        public IObservable<Scene> LoadScene(string sceneName)
        {
            if (_loadedScenes.Contains(sceneName))
                throw new InvalidOperationException("Duplicate scene loaded.");
            Scene scene = SceneManager.GetSceneByName(sceneName);
            _LoadSceneSubject subject = new _LoadSceneSubject();
            var operation = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
            operation.completed += (op) =>
            {
                if (op.isDone)
                {
                    _loadedScenes.Add(sceneName);
                    subject.Finish(scene);
                }
            };
            return subject;
        }

        public void UnloadScene(string sceneName, Action<string> onCompleted)
        {
            var operation = SceneManager.UnloadSceneAsync(sceneName);
            operation.completed += (op) =>
            {
                if (op.isDone)
                {
                    _loadedScenes.Remove(sceneName);
                    onCompleted?.Invoke(sceneName);
                }
            };
        }

        private class _LoadSceneSubject : IObservable<Scene>
        {
            private List<IObserver<Scene>> _observers = new List<IObserver<Scene>>();

            public void Finish(Scene view)
            {
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

            public IDisposable Subscribe(IObserver<Scene> observer)
            {
                _observers.Add(observer);
                return new SingleAssignmentDisposable();
            }
        }
    }
}