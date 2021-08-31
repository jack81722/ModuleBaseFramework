using System;

namespace ModuleBased
{
    public class DefaultSubscribe<T> : IObserver<T>
    {
        private Action _completeHandler;
        private Action<T> _nextHandler;
        private Action<Exception> _errorHandler;

        public DefaultSubscribe(Action<T> onNext, Action<Exception> onError)
        {
            _nextHandler = onNext;
            _errorHandler = onError;
        }

        public DefaultSubscribe(Action<T> onNext, Action<Exception> onError, Action onComplete)
        {
            _nextHandler = onNext;
            _errorHandler = onError;
            _completeHandler = onComplete;
        }

        public void OnCompleted()
        {
            try
            {
                _completeHandler.Invoke();
            }
            catch (Exception e)
            {
                OnError(e);
            }
        }

        public void OnNext(T value)
        {
            try
            {
                _nextHandler.Invoke(value);
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
}
