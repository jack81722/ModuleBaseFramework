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
                catch(Exception e)
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
                observer.OnComplete(result);
            }
            
        }

        #endregion
    }

    public class FromCoroutineObservable<T> : ObservableBase<T>
    {
        readonly Func<IObserver<T>, CancellationToken, IEnumerator> _coroutine;

        public FromCoroutineObservable(Func<IObserver<T>, CancellationToken, IEnumerator> coroutine)
        {
            _coroutine = coroutine;
        }

        public override IDisposable Subscribe(IObserver<T> observer)
        {
            var cancel = new DisposableCancellation();
            var token = cancel.Token;
            var source = new FromCoroutine<T>(observer, cancel);

            Debug.Log("Subscribe");
            MainThreadDispatcher.SendStartCoroutine(_coroutine(source, token));
            return cancel;
        }
    }

    public class FromCoroutine<T> : IObserver<T>
    {
        IObserver<T> _observer;
        IDisposable _disposable;

        public FromCoroutine(IObserver<T> observer, IDisposable disposable)
        {
            _observer = observer;
            _disposable = disposable;
        }

        public void OnComplete(T result)
        {
            try
            {
                _observer.OnComplete(result);
            }
            finally
            {
                _disposable.Dispose();
            }
        }

        public void OnError(Exception error)
        {
            try
            {
                _observer.OnError(error);
            }
            finally
            {
                _disposable.Dispose();
            }
        }
    }

    public class DisposableCancellation : IDisposable
    {
        private CancellationTokenSource _cts;

        public DisposableCancellation()
        {
            _cts = new CancellationTokenSource();
        }

        public DisposableCancellation(CancellationTokenSource source)
        {
            if (source == null)
            {
                throw new ArgumentNullException("Source is null.");
            }
            _cts = source;
        }

        public void Dispose()
        {
            _cts.Cancel();
        }

        public CancellationToken Token => _cts.Token;
        public bool IsDisposed => _cts.IsCancellationRequested;
    }


    public class ObservableBase<T> : IObservable<T>
    {
        public virtual IDisposable Subscribe(IObserver<T> observable)
        {
            return new SingleAssignmentDisposable();
        }
    }

    public class Subscribe_<T> : IObserver<T>
    {
        private Action<T> _completeHandler;
        private Action<Exception> _errorHandler;

        public Subscribe_(Action<T> onCompleted, Action<Exception> onError)
        {
            _completeHandler = onCompleted;
            _errorHandler = onError;
        }

        public void OnComplete(T result)
        {
            try
            {
                _completeHandler.Invoke(result);
            }
            catch (Exception e)
            {
                OnError(e);
            }
        }

        public void OnError(Exception error)
        {
            _errorHandler(error);
        }
    }

    /// <summary>
    /// ref : UniRX
    /// </summary>
    public sealed class SingleAssignmentDisposable : IDisposable
    {
        readonly object gate = new object();
        IDisposable current;
        bool disposed;

        public bool IsDisposed { get { lock (gate) { return disposed; } } }

        public IDisposable Disposable
        {
            get
            {
                return current;
            }
            set
            {
                var old = default(IDisposable);
                bool alreadyDisposed;
                lock (gate)
                {
                    alreadyDisposed = disposed;
                    old = current;
                    if (!alreadyDisposed)
                    {
                        if (value == null)
                            return;
                        current = value;
                    }
                }

                if (alreadyDisposed && value != null)
                {
                    value.Dispose();
                    return;
                }

                if (old != null)
                    throw new InvalidOperationException("Disposable is already set");
            }
        }


        public void Dispose()
        {
            IDisposable old = null;

            lock (gate)
            {
                if (!disposed)
                {
                    disposed = true;
                    old = current;
                    current = null;
                }
            }

            if (old != null)
                old.Dispose();
        }
    }
}
