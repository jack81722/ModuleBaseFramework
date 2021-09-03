using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ModuleBased.Rx
{
    public class TaskObservable<T> : ObservableBase<T>
    {
        ETaskObservableType _type;
        Func<Task<T>> _task;
        Func<CancellationToken, Task<T>> _taskWithToken;

        public TaskObservable(Func<Task<T>> task)
        {
            _type = ETaskObservableType.Default;
            _task = task;
        }

        public TaskObservable(Func<CancellationToken, Task<T>> task)
        {
            _type = ETaskObservableType.WithCancel;
            _taskWithToken = task;
        }


        public override IDisposable Subscribe(IObserver<T> observer)
        {
            CancellationDisposable disposable = new CancellationDisposable();
            FromCoroutine<T> coroutine = new FromCoroutine<T>(observer, disposable);
            switch (_type)
            {
                case ETaskObservableType.Default:
                    MainThreadDispatcher.SendStartCoroutine(ToEnumerator(coroutine));
                    break;
                case ETaskObservableType.WithCancel:
                    MainThreadDispatcher.SendStartCoroutine(ToEnumerator(coroutine, disposable.Token));
                    break;
            }
            return disposable;
        }

        private IEnumerator ToEnumerator(IObserver<T> observer)
        {
            Task<T> task = Task.Run(_task);
            TaskYield ty = new TaskYield(task);
            yield return ty;
            try
            {
                switch (task.Status)
                {
                    case TaskStatus.RanToCompletion:
                        observer.OnNext(task.Result);
                        break;
                    case TaskStatus.Faulted:
                        observer.OnError(task.Exception);
                        break;
                    case TaskStatus.Canceled:
                        observer.OnError(task.Exception);
                        break;
                    default:
                        observer.OnError(new Exception("Unknown"));
                        break;
                }
            }
            catch (Exception e)
            {
                observer.OnError(e);
            }
            try
            {
                observer.OnCompleted();
            }
            catch (Exception e)
            {
                observer.OnError(e);
            }
        }

        private IEnumerator ToEnumerator(IObserver<T> observer, CancellationToken token)
        {
            Task<T> task = Task.Run(() => _taskWithToken(token), token);
            TaskYield ty = new TaskYield(task);
            yield return ty;
            try
            {
                switch (task.Status)
                {
                    case TaskStatus.RanToCompletion:
                        observer.OnNext(task.Result);
                        break;
                    case TaskStatus.Faulted:
                        observer.OnError(task.Exception);
                        break;
                    case TaskStatus.Canceled:
                        observer.OnError(task.Exception);
                        break;
                    default:
                        observer.OnError(new Exception("Unknown"));
                        break;
                }
                observer.OnCompleted();
            }
            catch (Exception e)
            {
                observer.OnError(e);
            }
        }
    }

    internal enum ETaskObservableType
    {
        Default,
        WithCancel,
    }
}
