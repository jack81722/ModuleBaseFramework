using ModuleBased.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Remoting.Messaging;
using System.Runtime.Remoting.Proxies;
using UnityEngine;

namespace ModuleBased.Proxy
{
    public class ModuleProxyBase<T> : RealProxy, IGameModule where T : class, IGameModule
    {
        protected T realObj;

        public ModuleProxyBase(T target) 
        {
            realObj = target;
        }

        public ILogger Logger { get => realObj.Logger; set => realObj.Logger = value; }
        public IGameModuleCollection Modules { get => realObj.Modules; set => realObj.Modules = value; }

        public IEnumerator InitializeModule(IProgress<ProgressInfo> progress)
        {
            return realObj.InitializeModule(progress);
        }

        public void StartModule()
        {
            realObj.StartModule();
        }

        public override IMessage Invoke(IMessage msg)
        {
            UnityEngine.Debug.Log("Call message");
            IMethodCallMessage callMethod = msg as IMethodCallMessage;
            MethodInfo targetMethod = callMethod.MethodBase as MethodInfo;
            try
            {
                var result = targetMethod.Invoke(realObj, callMethod.Args);
                return new ReturnMessage(result, null, 0, callMethod.LogicalCallContext, callMethod);

            }
            catch (Exception e)
            {
                return new ReturnMessage(e, callMethod);
            }
            
        }

        

        protected object InvokeProxyMethod(params object[] args)
        {
            StackTrace trace = new StackTrace();
            var frames = trace.GetFrames();
            // skip RunProxyMethod
            
            var method = realObj.GetType().GetMethod(frames[1].GetMethod().Name);
            var result = method.Invoke(realObj, args);
            return result;
        }
    }
    
    public class ModuleProxyFactory
    {
        public static ModuleProxyBase<TReal> Create<TProxy, TReal>(TReal realObj) where TProxy : ModuleProxyBase<TReal> where TReal : class, IGameModule
        {
            var proxy = (TProxy)Activator.CreateInstance(typeof(TProxy), realObj);
            return proxy;
        }
    }

    public class ProxyAttribute : Attribute
    {
        public IObserver<IMessage> Observer;

        public ProxyAttribute(Type type)
        {
            if (typeof(IObserver<IMessage>).IsAssignableFrom(type))
            {
                Observer = (IObserver<IMessage>)Activator.CreateInstance(type);
            }
        }
    }

}
