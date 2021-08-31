using System;
using System.Collections;
using System.Collections.Generic;

namespace ModuleBased.Rx
{
    public class DoObservable<T> : ObservableOperatorBase<T>
    {
        Action<T> _nextHandler;
        Action<Exception> _errorHandler;
        Action _completeHandler;

        public DoObservable(IObservable<T> source, Action<T> onNext, Action<Exception> onError = null, Action onComplete = null) : base(source)
        {
            _nextHandler = onNext;
            _errorHandler = onError;
            _completeHandler = onComplete;
        }

        public override void OnCompleted()
        {
            try
            {
                _completeHandler?.Invoke();
            }
            catch (Exception e)
            {
                observer.OnError(e);
                Dispose();
                return;
            }

            try
            {
                observer.OnCompleted();
            }
            finally { Dispose(); }
        }

        public override void OnError(Exception error)
        {
            try
            {
                _errorHandler?.Invoke(error);

            }
            catch (Exception e)
            {
                try
                { observer.OnError(e); }
                finally { Dispose(); }
                return;
            }
            try
            {
                observer.OnError(error);
            }
            finally { Dispose(); }
        }

        public override void OnNext(T value)
        {
            try
            {
                _nextHandler.Invoke(value);
            }
            catch (Exception e)
            {
                try
                { observer.OnError(e); }
                finally { Dispose(); }
                return;
            }
            observer.OnNext(value);
        }
    }
}
