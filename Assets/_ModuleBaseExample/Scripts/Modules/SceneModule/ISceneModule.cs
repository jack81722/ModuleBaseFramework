using System;
using UnityEngine.SceneManagement;

namespace ModuleBased.Example.Scenes
{
    public interface ISceneModule
    {
        IObservable<Scene> LoadScene(string sceneName);

        void UnloadScene(string sceneName, Action<string> onCompleted = null);
    }
}