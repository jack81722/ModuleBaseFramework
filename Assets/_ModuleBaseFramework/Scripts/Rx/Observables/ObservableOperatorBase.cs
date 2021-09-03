using System;

namespace ModuleBased.Rx
{
    public abstract class ObservableOperatorBase<T> : ObservableBase<T>, IObserver<T>
    {
        protected IObservable<T> source;
        protected IDisposable disposable;
        protected IObserver<T> observer;

        public abstract void OnCompleted();

        public virtual void OnError(Exception error)
        {
            try
            {
                observer.OnError(error);
            }
            finally { Dispose(); }
        }

        public abstract void OnNext(T value);

        public override IDisposable Subscribe(IObserver<T> observer)
        {
            this.observer = observer;
            UnityEngine.Debug.Log($"source ? {source == null}");
            disposable = source.Subscribe(this);
            return disposable;
        }


        public void Dispose()
        {
            disposable.Dispose();
        }
    }

    public abstract class ObservableOperatorBase<TSource, TResult> : ObservableBase<TResult>, IObserver<TSource>
    {
        protected IObservable<TSource> source;
        protected IDisposable disposable;
        protected IObserver<TResult> observer;

        public abstract void OnCompleted();

        public virtual void OnError(Exception error)
        {
            try
            {
                observer.OnError(error);
            }
            finally
            {
                Dispose();
            }
        }

        public abstract void OnNext(TSource value);

        public override IDisposable Subscribe(IObserver<TResult> observer)
        {
            this.observer = observer;
            disposable = source.Subscribe(this);
            return disposable;
        }

        public virtual void Dispose()
        {
            disposable.Dispose();
        }
    }
}
