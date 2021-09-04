using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ModuleBased.Rx
{
    public static class Observable 
    {
        public static IObservable<T> Task<T>(Func<Task<T>> task)
        {
            return new TaskObservable<T>(task);
        }

        public static IObservable<T> Task<T>(Func<CancellationToken, Task<T>> task)
        {
            return new TaskObservable<T>(task);
        }

        public static IObservable<T> FromCoroutine<T>(Func<IObserver<T>, IEnumerator> coroutine)
        {
            return new FromCoroutineObservable<T>((observer, token) => coroutine(observer));
        }

        public static IObservable<T> FromCoroutine<T>(Func<IObserver<T>, CancellationToken, IEnumerator> coroutine)
        {
            return new FromCoroutineObservable<T>(coroutine);
        }

        public static IObservable<IEnumerable<TR>> FromSelect<T,TR>(IEnumerable<T> sources, Func<T, TR> selector)
        {
            return new FromSelectObservable<T, TR>(sources, selector);
        }

        public static IEnumerable<IObservable<TR>> ForEach<T, TR>(IEnumerable<T> sources, Func<T, IObservable<TR>> func)
        {
            int count = sources.Count();
            IObservable<TR>[] observables = new IObservable<TR>[count];
            int i = 0;
            foreach(var source in sources)
            {
                observables[i++] = func(source);
            }
            return observables;
        }

        public static IObservable<T> Progress<T>(Func<IProgress<T>, IEnumerator> progCoroutine)
        {
            return new ProgressObservable<T>(progCoroutine);
        }
    }
}
