using ModuleBased.ForUnity;
using ModuleBased.Proxy;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ModuleBased.Example
{
    public class MyModule : UniGameModule, IMyModule
    {

        [MyLog]
        public string GetHelloStr(int offset, int count)
        {
            return "Hello".Substring(offset, count);
        }

        [MyLog]
        public void SayHello()
        {
            print("Hello");
        }
    }

    public interface IMyModule
    {
        [MyLog]
        void SayHello();
        string GetHelloStr(int offset, int count);
    }

    public class MyModuleProxy : ModuleProxyBase<MyModule>, IMyModule
    {
        public MyModuleProxy(MyModule target) : base(target)
        {
        }

        public string GetHelloStr(int offset, int count)
        {
            if (realObj.GetType().IsDefined(typeof(MyLogAttribute), true))
            {
                Debug.Log("Before GetHelloStr");
            }
            string result = (string)InvokeProxyMethod(offset, count);
            Debug.Log("After GetHelloStr");
            return result;
        }

        public void SayHello()
        {
            if (realObj.GetType().GetMethod("SayHello").IsDefined(typeof(MyLogAttribute), true))
            {
                Debug.Log("Before SayHello");
            }
            
            InvokeProxyMethod();
            Debug.Log("After SayHello");
        }
    }

    [AttributeUsage(AttributeTargets.Method)]
    public class MyLogAttribute : Attribute
    {
    }

}
