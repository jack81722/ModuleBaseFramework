using System;
using System.Collections;
using System.Collections.Generic;

namespace ModuleBased.Rx
{
    public class FromCoroutine<T> : IObserver<T>
    {
        IObserver<T> _observer;
        IDisposable _disposable;

        public FromCoroutine(IObserver<T> observer, IDisposable disposable)
        {
            _observer = observer;
            _disposable = disposable;
        }

        public void OnCompleted()
        {
            try
            {
                _observer.OnCompleted();
            }
            catch (Exception e)
            {
                OnError(e);
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

        public void OnNext(T value)
        {
            try
            {
                _observer.OnNext(value);
            }
            catch (Exception e)
            {
                OnError(e);
            }
        }
    }

}
