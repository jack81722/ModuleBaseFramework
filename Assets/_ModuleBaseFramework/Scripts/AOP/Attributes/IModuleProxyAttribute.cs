using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Remoting.Messaging;

namespace ModuleBased.AOP.Attributes {
    public interface IModuleProxyAttribute {
        void OnBefore(MethodInfo method, object[] args, ILogger logger = null);

        void OnAfter(MethodInfo method, object[] args, object result, ILogger logger = null);
    }

    public interface IModuleProxyExceptionAttribute
    {
        void OnException(MethodInfo method, Exception e, ILogger logger = null);
    }
}