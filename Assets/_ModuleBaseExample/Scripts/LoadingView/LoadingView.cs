using ModuleBased.ForUnity;
using ModuleBased.Injection;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[Injectable(typeof(LoadingView))]
public class LoadingView : MonoBehaviour, IProgress<float>
{
    [SerializeField]
    private string[] _loadingScenes;

    private string _sceneName;
    private bool _isLoading;
    private bool _cancel;

    private Loading _bar;

    private bool TryRandomLoadingScene(out string scene)
    {
        if (_loadingScenes == null || _loadingScenes.Length <= 0)
        {
            scene = null;
            return false;
        }
        scene = _loadingScenes[UnityEngine.Random.Range(0, _loadingScenes.Length)];
        return true;
    }

    public void BeginLoading()
    {
        if (TryRandomLoadingScene(out string loading))
        {
            _sceneName = loading;
            _isLoading = true;
            var asyncOp = SceneManager.LoadSceneAsync(loading, LoadSceneMode.Additive);
            asyncOp.completed += (op) =>
            {
                if (op.isDone)
                {
                    _bar = FindObjectOfType<Loading>();
                    _isLoading = false;
                    if (_cancel)
                        EndLoading();
                    _cancel = false;
                }
            };
        }
    }

    public void EndLoading()
    {
        if (!string.IsNullOrEmpty(_sceneName))
        {
            if (_isLoading)
            {
                _cancel = true;
                return;
            }
            SceneManager.UnloadSceneAsync(_sceneName);
            _bar = null;
        }
    }

    public void Report(float value)
    {
        if (_bar != null)
        {
            _bar.Report(value);
        }
    }
}
