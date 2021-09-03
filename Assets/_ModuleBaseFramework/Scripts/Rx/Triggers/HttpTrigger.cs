using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using UnityEngine;
using UnityEngine.Networking;

namespace ModuleBased.Rx
{
    public class HttpTrigger
    {
        #region -- Static methods --
        public static IObservable<string> Get(string url)
        {
            var req = UnityWebRequest.Get(url);
            var asyncOp = req.SendWebRequest();
            CancellationTokenSource cancel = new CancellationTokenSource();
            return new FromCoroutineObservable<string>((source, token) => FetchResult(asyncOp, source, null, cancel.Token));
        }

        private static IEnumerator FetchResult(AsyncOperation asyncOp, IObserver<string> observer, IProgress<float> progress, CancellationToken cancel)
        {
            while (!asyncOp.isDone && !cancel.IsCancellationRequested)
            {
                try
                {
                    progress?.Report(asyncOp.progress);
                }
                catch (Exception e)
                {
                    observer.OnError(e);
                    yield break;
                }
                yield return null;
            }
            var webAsyncOp = (UnityWebRequestAsyncOperation)asyncOp;
            if (!string.IsNullOrEmpty(webAsyncOp.webRequest.error))
            {
                observer.OnError(new Exception(webAsyncOp.webRequest.error));
            }
            if (webAsyncOp.isDone)
            {
                string result = Encoding.UTF8.GetString(webAsyncOp.webRequest.downloadHandler.data);
                try
                {
                    observer.OnNext(result);
                    observer.OnCompleted();
                }
                catch (Exception e)
                {
                    observer.OnError(e);
                }
            }

        }

        #endregion
    }
}
