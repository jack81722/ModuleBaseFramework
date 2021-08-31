using System;
using System.Collections;
using System.Collections.Generic;

namespace ModuleBased.Rx
{
    public class ContinueObservable<TSource, TResult> : ObservableOperatorBase<TSource, TResult>
    {
        Func<TSource, IObservable<TResult>> _next;

        SerialDisposable serialDisposable;

        bool _isValid;
        TSource _value;

        public ContinueObservable(IObservable<TSource> source, Func<TSource, IObservable<TResult>> next) : base(source)
        {
            _next = next;
        }

        public override void OnCompleted()
        {
            if (_isValid)
            {
                IObservable<TResult> next = _next(_value);
                serialDisposable.Disposable = next.Subscribe(observer);
            }
            OnError(new InvalidOperationException("Result of source observable is invalid."));
        }


        public override void OnNext(TSource value)
        {
            _isValid = true;
            _value = value;
        }

        public override IDisposable Subscribe(IObserver<TResult> observer)
        {
            this.observer = observer;
            serialDisposable = new SerialDisposable();
            serialDisposable.Disposable = source.Subscribe(this);
            disposable = serialDisposable;
            return disposable;
        }
    }
}
