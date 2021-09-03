using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ModuleBased.Rx
{
    public class Observable 
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
    }
}
