using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace ModuleBased.AOP.Attributes {
    /// <summary>
    /// Log method name before/after method execute
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Method)]
    public class SimpleLogAttribute : Attribute, IModuleProxyAttribute {
        public void OnAfter(MethodInfo method, object[] args, object result) {
#if UNITY_STANDALONE
            UnityEngine.Debug.Log($"After execute method : {method.Name}");
#endif
        }

        public void OnBefore(MethodInfo method, object[] args) {
#if UNITY_STANDALONE
            UnityEngine.Debug.Log($"Before execute method : {method.Name}");
#endif
        }
    }
}