using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace ModuleBased.AOP.Attributes {
    public interface IModuleProxyAttribute {
        void OnBefore(MethodInfo method, object[] args);

        void OnAfter(MethodInfo method, object[] args, object result);
    }
}