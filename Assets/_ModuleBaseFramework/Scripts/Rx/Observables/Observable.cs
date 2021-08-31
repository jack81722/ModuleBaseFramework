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

    }
}
