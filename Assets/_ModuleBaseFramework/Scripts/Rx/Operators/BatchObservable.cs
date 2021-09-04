using System;
using System.Collections.Generic;

namespace ModuleBased.Rx
{
    public class BatchObservable<T> : ObservableOperatorBase<T[]>
    {
        private List<Batch<T>> _batched = new List<Batch<T>>();
        private int _index;

        SerialDisposable _serialDisposable;

        public BatchObservable(IEnumerable<IObservable<T>> sources, int batchSize)
        {
            List<IObservable<T>> group = new List<IObservable<T>>();
            var e = sources.GetEnumerator();
            while (e.MoveNext())
            {
                group.Add(e.Current);
                if (group.Count == batchSize)
                {
                    _batched.Add(new Batch<T>(group.ToArray()));
                    group.Clear();
                }
            }
            if (group.Count > 0)
            {
                _batched.Add(new Batch<T>(group.ToArray()));
                group.Clear();
            }
        }

        public override void OnCompleted()
        {
            _index++;
            if (_index >= _batched.Count)
            {
                try
                {
                    observer.OnCompleted();
                }
                catch (Exception e)
                {
                    try
                    {
                        observer.OnError(e);
                    }
                    finally { Dispose(); }
                }
                return;
            }
            try
            {
                var group = _batched[_index];
                _serialDisposable.Dispose();
                _serialDisposable.Disposable = group.Subscribe(this);
            }
            catch (Exception e)
            {
                try
                {
                    observer.OnError(e);
                }
                finally
                {
                    Dispose();
                }
            }

        }

        public override void OnNext(T[] value)
        {
            try
            {
                observer.OnNext(value);
            }
            catch (Exception e)
            {
                try
                {
                    OnError(e);
                }
                finally { Dispose(); }
            }
        }

        public override IDisposable Subscribe(IObserver<T[]> observer)
        {
            this.observer = observer;
            disposable = _serialDisposable = new SerialDisposable();
            var group = _batched[_index];
            _serialDisposable.Disposable = group.Subscribe(this);
            return _serialDisposable;
        }
    }

    public class Batch<T> : ObservableBase<T[]>
    {
        IEnumerable<IObservable<T>> _sources;

        public Batch(IEnumerable<IObservable<T>> sources)
        {
            _sources = sources;
        }

        public override IDisposable Subscribe(IObserver<T[]> observer)
        {
            return _sources.WhenAll()
                .Subscribe(observer);
        }
    }

}
