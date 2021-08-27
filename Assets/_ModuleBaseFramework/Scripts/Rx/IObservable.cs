using System;

namespace ModuleBased.Rx
{
    public interface IObservable<T>
    {
        IDisposable Subscribe(IObserver<T> observable);
    }
}
