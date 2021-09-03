using System;
using System.Collections;
using System.Threading;

namespace ModuleBased.Rx
{
    public class ProgressObservable<T> : IObservable<T>
    {
        private IDisposable _disposable;
        Func<IProgress<T>, IEnumerator> _progCoroutine;

        public ProgressObservable(Func<IProgress<T>, IEnumerator> progCoroutine)
        {
            _progCoroutine = progCoroutine;
        }

        public IDisposable Subscribe(IObserver<T> observer)
        {
            var source = new FromCoroutineObservable<T>(RunProgress);
            _disposable = source.Subscribe(observer);
            return _disposable;
        }

        private IEnumerator RunProgress(IObserver<T> observer, CancellationToken token)
        {
            ProgressObserver<T> po = new ProgressObserver<T>(observer);
            var enumerator = _progCoroutine(po);
            bool isErr = false;
            while (true)
            {
                try
                {
                    if (!enumerator.MoveNext())
                        break;
                }
                catch (Exception e)
                {
                    try
                    { observer.OnError(e); }
                    finally
                    {
                        isErr = true;
                    }
                }
                // break if error
                if (isErr)
                {
                    try
                    {
                        observer.OnCompleted();
                    }
                    finally
                    {
                        _disposable.Dispose();
                    }
                    yield break;
                }
                yield return null;
            }

            try
            {
                observer.OnCompleted();
            }
            finally
            {
                _disposable.Dispose();
            }

        }
    }
}
