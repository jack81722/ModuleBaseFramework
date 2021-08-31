using System;
using System.Collections;
using System.Collections.Generic;

namespace ModuleBased.Rx
{
    public class SelectObservable<TSource, TResult> : ObservableOperatorBase<TSource, TResult>
    {
        Func<TSource, TResult> selector;

        public SelectObservable(IObservable<TSource> source, Func<TSource, TResult> selector): base(source)
        {
            this.selector = selector;
        }

        public override void OnCompleted()
        {
            try
            {
                observer.OnCompleted();
            }
            catch (Exception e)
            {
                OnError(e);
            }
        }

        public override void OnNext(TSource value)
        {
            try
            {
                TResult result = selector.Invoke(value);
                observer.OnNext(result);
            }
            catch (Exception e)
            {
                OnError(e);
            }
        }
    }
}
