using ModuleBased.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Remoting.Messaging;
using System.Runtime.Remoting.Proxies;
using UnityEngine;

namespace ModuleBased.Proxy
{
    public class ModuleProxy<T> : RealProxy, IGameModule where T : class, IGameModule
    {
        protected T realInstance;

        public ILogger Logger { get => realInstance.Logger; set => realInstance.Logger = value; }
        public IGameModuleCollection Modules { get => realInstance.Modules; set => realInstance.Modules = value; }

        public IEnumerator InitializeModule(IProgress<ProgressInfo> progress)
        {
            return realInstance.InitializeModule(progress);
        }

        public void StartModule()
        {
            realInstance.StartModule();
        }

        public ModuleProxy(T target)
        {
            realInstance = target;
        }

        public override IMessage Invoke(IMessage msg)
        {
            IMethodCallMessage callMethod = msg as IMethodCallMessage;
            MethodInfo targetMethod = callMethod.MethodBase as MethodInfo;
            InvokeBeforeObserver(callMethod.Args);
            var result = targetMethod.Invoke(realInstance, callMethod.Args);
            IMethodReturnMessage returnMethod = new ReturnMessage(result, null, 0,
                callMethod.LogicalCallContext, callMethod);
            InvokeAfterObserver(result);
            return returnMethod;
        }

        List<IObserver<object[]>> _beforObservers = new List<IObserver<object[]>>();
        List<IObserver<object>> _afterObservers = new List<IObserver<object>>();

        protected void InvokeBeforeObserver(object[] args)
        {
            foreach (var observer in _beforObservers)
            {
                // skip null observer
                if (observer == null)
                    continue;
                try
                {
                    observer.OnNext(args);
                }
                catch (Exception e)
                {
                    try
                    {
                        observer.OnError(e);
                    }
                    catch { continue; }
                }
                try
                {
                    observer.OnCompleted();
                }
                catch (Exception e)
                {
                    try
                    {
                        observer.OnError(e);
                    }
                    catch { continue; }
                }
            }
        }

        public void SubscribeBeforeAllMethod(IObserver<object[]> observer)
        {

        }

        public void SubscribeBeforeMethod(IObserver<object[]> observer, params string[] methods)
        { }

        protected void InvokeAfterObserver(object result)
        {
            foreach (var observer in _afterObservers)
            {
                // skip null observer
                if (observer == null)
                    continue;
                try
                {
                    observer.OnNext(result);
                }
                catch (Exception e)
                {
                    try
                    {
                        observer.OnError(e);
                    }
                    catch { continue; }
                }
                try
                {
                    observer.OnCompleted();
                }
                catch (Exception e)
                {
                    try
                    {
                        observer.OnError(e);
                    }
                    catch { continue; }
                }
            }
        }
    }


    public sealed class BeforeProxyObserver : DefaultSubscribe<object[]>
    {
        public BeforeProxyObserver(Action<object[]> onNext, Action<Exception> onError) : base(onNext, onError) { }

        public BeforeProxyObserver(Action<object[]> onNext, Action<Exception> onError, Action onComplete) : base(onNext, onError, onComplete) { }
    }
}
