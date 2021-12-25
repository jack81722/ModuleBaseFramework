using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModuleBased.Rx
{
    public class Subject<T> : IObservable<T>, IObserver<T>, IDisposable
    {
        private HashSet<IObserver<T>> _observers;
        private IDisposable _disposable;

        public Subject()
        {
            _observers = new HashSet<IObserver<T>>();
        }

        public void Dispose()
        {   
            _disposable?.Dispose();
        }

        public void OnCompleted()
        {
            foreach(var observer in _observers)
            {
                try
                {
                    observer.OnCompleted();
                }catch(Exception e)
                {
                    observer.OnError(e);
                }
                finally
                {
                    Dispose();
                }
            }
        }

        public void OnError(Exception error)
        {
            foreach (var observer in _observers)
            {
                observer.OnError(error);
            }
        }

        public void OnNext(T value)
        {
            foreach (var observer in _observers)
            {
                try
                {
                    observer.OnNext(value);
                }
                catch (Exception e)
                {
                    observer.OnError(e);
                }
                finally
                {
                    Dispose();
                }
            }
        }

        public IDisposable Subscribe(IObserver<T> observer)
        {
            _observers.Add(observer);
            _disposable = new SingleAssignmentDisposable();
            return _disposable;
        }

        public void Clear()
        {
            _observers.Clear();
        }
    }
}
