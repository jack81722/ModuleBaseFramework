using ModuleBased.ForUnity;
using ModuleBased.Proxy;
using ModuleBased.Proxy.AOP;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ModuleBased.Example
{
    [UniModule(typeof(IMyModule))]
    [Proxy(typeof(MyModuleProxy))]
    public class MyModule : UniGameModule, IMyModule
    {
        [MyLog(EAOPStatus.Around)]
        public string GetHelloStr(int offset, int count)
        {
            return "Hello".Substring(offset, count);
        }

        [MyLog(EAOPStatus.Before)]
        public void SayHello()
        {
            print("Hello");
        }
    }

    public interface IMyModule
    {
        void SayHello();
        string GetHelloStr(int offset, int count);
    }

    public class MyModuleProxy : ModuleAOP<MyModule>, IMyModule
    {
        public MyModuleProxy(MyModule real) : base(real)
        {
        }

        public string GetHelloStr(int offset, int count)
        {
            return (string)InvokeProxyMethod(offset, count);
        }

        public void SayHello()
        {
            InvokeProxyMethod();
        }
    }

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = false)]
    public class MyLogAttribute : AOPAttribute
    {
        public MyLogAttribute(EAOPStatus usage) : base(usage, typeof(MyLog))
        {
        }
    }

    public class MyLog : IAOPHandler
    {
        public void OnInvoke(object sender, AOPEventArgs args)
        {
            Debug.Log(args.Method.Name);
            if(args.Result != null)
            {
                Debug.Log($"Result: {args.Result}");
            }
        }
    }

}
