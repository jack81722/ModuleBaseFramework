using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;

namespace ModuleBased.Rx
{
    public class FromCoroutineObservable<T> : ObservableBase<T>
    {
        readonly Func<IObserver<T>, CancellationToken, IEnumerator> _coroutine;

        public FromCoroutineObservable(Func<IObserver<T>, CancellationToken, IEnumerator> coroutine)
        {   
            _coroutine = coroutine;
        }

        public override IDisposable Subscribe(IObserver<T> observer)
        {
            var cancel = new CancellationDisposable();
            var token = cancel.Token;
            var source = new FromCoroutine<T>(observer, cancel);

            MainThreadDispatcher.SendStartCoroutine(_coroutine(source, token));
            return cancel;
        }
    }

}
