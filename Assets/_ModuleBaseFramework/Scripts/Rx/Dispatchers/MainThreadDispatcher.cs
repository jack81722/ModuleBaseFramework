using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ModuleBased.Rx
{
    public class MainThreadDispatcher : MonoBehaviour
    {
        private static MainThreadDispatcher _singleton;
        private static MainThreadDispatcher singleton
        {
            get
            {
                if (_singleton == null)
                {
                    _singleton = FindObjectOfType<MainThreadDispatcher>();
                    if(_singleton == null)
                    {
                        GameObject go = new GameObject("MainThreadDispatcher");
                        _singleton = go.AddComponent<MainThreadDispatcher>();
                    }
                }
                return _singleton;
            }
        }

        public static void SendStartCoroutine(IEnumerator coroutine, Action onCompleted = null, Action<Exception> onError = null)
        {
            singleton.StartCoroutine(singleton.InnerStartCoroutine(coroutine, onCompleted, onError));
        }

        private IEnumerator InnerStartCoroutine(IEnumerator coroutine, Action onCompleted, Action<Exception> onError = null)
        {
            bool running;
            while (true)
            {
                try
                {
                    running = coroutine.MoveNext();
                    if (!running)
                        break;
                }
                catch (Exception e)
                {
                    onError?.Invoke(e);
                    break;
                }
                yield return null;
            }       
            try
            {
                onCompleted?.Invoke();
            }
            catch (Exception e)
            {
                onError?.Invoke(e);
            }
        }
    }
}
