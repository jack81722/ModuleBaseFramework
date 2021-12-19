using System;

namespace ModuleBased
{
    public class GenericObserver<T> : IObserver<T>
    {
        private IObserver<object> _observer;

        public GenericObserver(IObserver<object> observer)
        {
            _observer = observer;
        }

        public void OnCompleted()
        {
            _observer.OnCompleted();
        }

        public void OnError(Exception error)
        {
            _observer.OnError(error);
        }

        public void OnNext(T value)
        {
            _observer.OnNext(value);
        }
    }
}
