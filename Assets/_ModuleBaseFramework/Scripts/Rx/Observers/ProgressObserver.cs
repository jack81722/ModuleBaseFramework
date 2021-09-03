using System;

namespace ModuleBased.Rx
{
    public class ProgressObserver<T> : IProgress<T>, IObserver<T>
    {
        IObserver<T> _observer;

        public ProgressObserver(IObserver<T> observer)
        {
            _observer = observer;
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
            _observer.OnError(error);
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

        public void Report(T value)
        {
            OnNext(value);
        }
    }

}
