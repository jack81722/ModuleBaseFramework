using System;
using System.Collections.Generic;
using System.Linq;

namespace ModuleBased.Rx
{
    public class FromSelectObservable<TSource, TResult> : ObservableBase<IEnumerable<TResult>>
    {
        private readonly IEnumerable<TSource> _sources;
        private readonly Func<TSource, TResult> _selector;

        public FromSelectObservable(IEnumerable<TSource> sources, Func<TSource, TResult> selector)
        {
            _sources = sources;
            _selector = selector;
        }

        public override IDisposable Subscribe(IObserver<IEnumerable<TResult>> observer)
        {
            IDisposable disposable = new SingleAssignmentDisposable();
            int count = _sources.Count();
            TResult[] results = new TResult[count];
            int i = 0;
            foreach (TSource source in _sources)
            {
                try
                {
                    results[i++] = _selector(source);
                }
                catch (Exception e)
                {
                    observer.OnError(e);
                }
            }
            try
            {
                observer.OnNext(results);
            }
            catch(Exception e)
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
            return disposable;
        }
    }
}
