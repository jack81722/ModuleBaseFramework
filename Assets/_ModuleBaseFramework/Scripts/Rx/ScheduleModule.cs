using ModuleBased.ForUnity;
using ModuleBased.Rx;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace ModuleBased
{
    [UniModule(typeof(IScheduleModule))]
    public class ScheduleModule : UniGameModule, IScheduleModule
    {
        // how to instantiate module ?

        Dictionary<Type, List<MethodInfo>> _scheduleMethods;

        bool isInit;
        bool isEnd;


        [RequireModule]
        IGameModule _other;

        protected override void OnStartingModule()
        {
            Test();
            //TestTask();
            //TestTaskWithCancel();
            
            //foreach (var mod in Modules)
            //{
            //    var methods = mod.GetType().GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic);
            //    foreach (var method in methods)
            //    {
            //        if (method.IsDefined(typeof(ObserverAttribute)))
            //        {
            //            var ps = method.GetParameters();
            //            if (ps.Length <= 0)
            //                continue;
            //            AddScheduleMethod(ps[0].ParameterType, method);
            //        }
            //    }

            //}
        }


        private void AddScheduleMethod(Type schType, MethodInfo method)
        {
            if (!_scheduleMethods.TryGetValue(schType, out List<MethodInfo> methods))
            {
                methods = new List<MethodInfo>();
                _scheduleMethods.Add(schType, methods);
            }
            methods.Add(method);
        }

        private void RemoveScheduleMethodAll(IGameModule module)
        {


        }

        public void ListenTrigger()
        {
            
        }

        public void Test()
        {
            HttpTrigger.Get("https://www.google.com/")
                .Do(c => Debug.Log($"Do:{c}"))
                .Subscribe(
                    c => Debug.Log(c),
                    e => Debug.LogError(e)
                );
        }

        public void TestTask()
        {
            Observable.Task(RunTaskWithCancel)
                .Subscribe(
                    x => Debug.Log($"Next:{x}"),
                    e => Debug.LogError(e),
                    () => Debug.Log("Finished!")
                );
        }

        public void TestTaskWithCancel()
        {
            var disposable = Observable.Task(RunTaskWithCancel)
                .Subscribe(
                    x => Debug.Log($"Next:{x}"),
                    e => Debug.LogError(e),
                    () => Debug.Log("Finished!")
                );disposable.Dispose();
        }
            

        public async Task<int> RunTask()
        {
            int i = 0;
            for (; i < 5; i++)
            {
                await Task.Delay(1000);
                Debug.Log(i.ToString());
            }
            return i;
        }

        public async Task<int> RunTaskWithCancel(CancellationToken token)
        {
            int i = 0;
            for (; i < 5; i++)
            {
                if (token.IsCancellationRequested)
                {
                    return i;
                }
                await Task.Delay(1000);
                Debug.Log(i.ToString());
            }
            return i;
        }

        [Observer]
        public void TestObserver(RxResult<int> result)
        {
            switch (result.status)
            {
                case EObservableStatus.Completed:
                    Logger.Log("Finished!");
                    break;
                case EObservableStatus.OnError:
                    Logger.LogError(result.Error);
                    break;
                case EObservableStatus.Running:
                    Logger.Log(result.Result);
                    break;
                default:
                    // unactive
                    break;
            }
        }
    }



    public class RxResult<T> : IDisposable
    {
        public EObservableStatus status;
        public T Result { get; }
        public Exception Error { get; }

        public void Dispose()
        {
            var r = Result as IDisposable;
            if (r != null)
            {
                r.Dispose();
            }
        }
    }

    [Flags]
    public enum EObservableStatus
    {
        Unactive,
        OnError,
        Running,
        Completed
    }


    public interface IScheduleModule
    {
    }

    public class ObserverAttribute : Attribute
    {
    }

    public class OriginRx<T> : System.IObservable<T>
    {
        public IDisposable Subscribe(System.IObserver<T> observer)
        {
            throw new NotImplementedException();
        }
    }

    public class OriginObserver<T> : System.IObserver<T>
    {
        public void OnCompleted()
        {
            throw new NotImplementedException();
        }

        public void OnError(Exception error)
        {
            throw new NotImplementedException();
        }

        public void OnNext(T value)
        {
            throw new NotImplementedException();
        }
    }
}
