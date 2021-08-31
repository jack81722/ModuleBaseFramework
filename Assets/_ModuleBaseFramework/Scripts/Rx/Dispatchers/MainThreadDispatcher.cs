using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ModuleBased
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

        public static Coroutine SendStartCoroutine(IEnumerator coroutine)
        {
            return singleton.StartCoroutine(coroutine);
        }
    }
}
