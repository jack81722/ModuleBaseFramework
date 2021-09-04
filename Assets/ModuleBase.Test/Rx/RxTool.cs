using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Test.Rx
{
    public partial class RxTool
    {
        public static readonly System.Exception TestError = new System.Exception("TestError");
        public static readonly System.Exception CancelError = new System.Exception("CaneelError");

        public static IEnumerator NormalEum()
        {
            for (int i = 0; i < 100; i++)
            {
                yield return null;
            }
        }

        public static IEnumerator NormalEumWithToken(CancellationToken token)
        {
            for (int i = 0; i < 100; i++)
            {
                if (token.IsCancellationRequested)
                    yield break;
                yield return null;
            }
        }

        public static IEnumerator ErrorEnum()
        {
            yield return null;
            throw TestError;
        }

        public static IEnumerator ErrorEnumWithToken(CancellationToken token)
        {
            for (int i = 0; i < 100; i++)
            {
                if (token.IsCancellationRequested)
                    yield break;
                yield return null;
            }
            throw TestError;
        }

        public static Func<IObserver<T>, CancellationToken, IEnumerator> GetWrappedEnum<T>(Func<IEnumerator> enumFunc, T returnValue)
        {
            return (observer, token) => WrapEnum(observer, token, (t) => enumFunc(), returnValue);
        }

        public static Func<IObserver<T>, CancellationToken, IEnumerator> GetWrappedEnum<T>(Func<CancellationToken, IEnumerator> enumFunc, T returnValue)
        {
            return (observer, token) => WrapEnum(observer, token, enumFunc, returnValue);
        }

        private static IEnumerator WrapEnum<T>(IObserver<T> observer, CancellationToken token, Func<CancellationToken, IEnumerator> enumFunc, T returnValue)
        {
            bool keepWait;
            var enumerator = enumFunc(token);
            while (!token.IsCancellationRequested)
            {
                try
                {
                    keepWait = enumerator.MoveNext();
                }
                catch (Exception e)
                {
                    try
                    {
                        observer.OnError(e);
                    }
                    finally { keepWait = false; }
                }
                if (!keepWait)
                {
                    observer.OnCompleted();
                    yield break;
                }
            }
            try
            {
                observer.OnNext(returnValue);
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

    public partial class RxTool
    {
        public static Task<T> NormalTask<T>(CancellationToken token, T result)
        {
            TaskCompletionSource<T> tcs = new TaskCompletionSource<T>();
            Task.Factory.StartNew(async () =>
            {
                await Task.Delay(10);
                tcs.SetResult(result);
                //for (int i = 0; i < 100; i++)
                //{
                //    if (token.IsCancellationRequested)
                //        tcs.SetCanceled();
                //    await Task.Delay(10);
                //}
                //tcs.SetResult(result);
            });
            return tcs.Task;
        }

        public static Task<T> ErrorTask<T>(CancellationToken token, T result)
        {
            TaskCompletionSource<T> tcs = new TaskCompletionSource<T>();
            Task.Run(async () =>
            {
                for (int i = 0; i < 100; i++)
                {
                    if (token.IsCancellationRequested)
                        tcs.SetCanceled();
                    await Task.Delay(10);
                }
                tcs.SetException(TestError);
            });
            return tcs.Task;
        }
    }

    public class TestObserver
    {
        private List<IObserverFlag> _flagList = new List<IObserverFlag>();
        CancellationTokenSource _cts;

        public void Wait(int milliseconds, Action onCompleted, Action onTimeout)
        {
            _cts = new CancellationTokenSource();
            _cts.CancelAfter(milliseconds);

            while (!_flagList.TrueForAll(f => f.IsCompleted()))
            {
                if (_cts.IsCancellationRequested)
                {
                    onTimeout();
                    return;
                }
            }
            onCompleted();
        }


        public IObserver<T> NewObserver<T>(Action<T> onNext = null, Action<Exception> onError = null, Action onCompleted = null)
        {
            SubObserver<T> observer = new SubObserver<T>(onNext, onError, onCompleted);
            _flagList.Add(observer);
            return observer;
        }

        public class SubObserver<T> : IObserver<T>, IObserverFlag
        {
            bool _isCompleted;
            bool _isError;
            Action<T> _nextHandler;
            Action<Exception> _errorHandler;
            Action _completeHandler;

            public SubObserver(Action<T> onNext = null, Action<Exception> onError = null, Action onCompleted = null)
            {
                _nextHandler = onNext;
                _errorHandler = onError;
                _completeHandler = onCompleted;
            }

            public bool IsCompleted()
            {
                return _isCompleted;
            }

            public bool IsError()
            {
                return _isError;
            }

            public void OnCompleted()
            {
                _isCompleted = true;
                _completeHandler?.Invoke();
            }

            public void OnError(Exception error)
            {
                _isError = true;
                _errorHandler?.Invoke(error);
            }

            public void OnNext(T value)
            {
                _nextHandler?.Invoke(value);
            }
        }

        public interface IObserverFlag
        {
            bool IsCompleted();
            bool IsError();
        }
    }

    public static class TestObserverExtension
    {
        public static IDisposable Subscribe<T>(this IObservable<T> source, TestObserver test, Action<T> onNext = null, Action<Exception> onError = null, Action onCompleted = null)
        {
            return source.Subscribe(test.NewObserver(onNext, onError, onCompleted));
        }

    }
}