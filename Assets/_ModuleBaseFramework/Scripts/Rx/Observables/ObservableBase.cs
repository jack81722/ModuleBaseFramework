using System;
using System.Collections;
using System.Collections.Generic;

namespace ModuleBased.Rx
{
    public class ObservableBase<T> : IObservable<T>
    {
        public virtual IDisposable Subscribe(IObserver<T> observer)
        {
            return new SingleAssignmentDisposable();
        }
    }
}
