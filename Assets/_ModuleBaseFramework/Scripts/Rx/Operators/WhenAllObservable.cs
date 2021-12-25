using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModuleBased.Rx
{
    public class WhenAllObservable<T> : ObservableOperatorBase<T, T[]>
    {
        private int _waitCount;
        private IObservable<T>[] _sources;
        private List<T> _results;

        public WhenAllObservable(params IObservable<T>[] sources)
        {
            _sources = sources;
            _waitCount = _sources.Length;
            _results = new List<T>(_sources.Length);
        }

        public override void OnCompleted()
        {
            _waitCount--;
            if (_waitCount <= 0)
            {
                try
                {
                    observer.OnNext(_results.ToArray());
                }
                catch (Exception e)
                {
                    try
                    { OnError(e); }
                    finally { Dispose(); }
                    return;
                }
                try
                { observer.OnCompleted(); }
                finally { Dispose(); }
            }

        }

        public override void OnNext(T value)
        {
            _results.Add(value);
        }

        public override IDisposable Subscribe(IObserver<T[]> observer)
        {
            CompositeDisposable composite = new CompositeDisposable();
            this.observer = observer;
            foreach(var source in _sources)
            {
                if (source == null)
                    continue;
                composite.Add(source.Subscribe(this));
            }
            disposable = composite;
            return composite;
        }

    }
}
