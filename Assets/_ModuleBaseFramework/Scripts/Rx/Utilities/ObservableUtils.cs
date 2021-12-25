using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace ModuleBased.Rx
{
    public static partial class ObservableUtils
    {
        public static IDisposable Subscribe<T>(this IObservable<T> source, Action<T> onNext, Action<Exception> onError)
        {
            return source.Subscribe(new DefaultSubscribe<T>(onNext, onError));
        }

        public static IDisposable Subscribe<T>(this IObservable<T> source, Action<T> onNext, Action<Exception> onError, Action onComplete)
        {
            return source.Subscribe(new DefaultSubscribe<T>(onNext, onError, onComplete));
        }
    }

    public static partial class ObservableUtils
    {
        public static IObservable<TResult> ContinueWith<TSource, TResult>(this IObservable<TSource> source, Func<TSource, IObservable<TResult>> next)
        {
            return new ContinueObservable<TSource, TResult>(source, next);
        }

        public static IObservable<T> Do<T>(this IObservable<T> source, Action<T> onNext, Action<Exception> onError = null, Action onComplete = null)
        {
            return new DoObservable<T>(source, onNext, onError, onComplete);
        }

        public static IObservable<T[]> WhenAll<T>(this IEnumerable<IObservable<T>> sources)
        {
            return new WhenAllObservable<T>(sources.ToArray());
        }

        public static IObservable<T[]> Batch<T>(this IEnumerable<IObservable<T>> sources, int batchSize)
        {
            return new BatchObservable<T>(sources, batchSize);
        }

        public static IObservable<TR> Select<T, TR>(this IObservable<T> source, Func<T, TR> selector)
        {
            return new SelectObservable<T, TR>(source, selector);
        }

        public static IObservable<T> OnCompleted<T>(this IObservable<T> source, Action onCompleted)
        {
            source.Subscribe(null, null, onCompleted);
            return source;
        }
    }
}
