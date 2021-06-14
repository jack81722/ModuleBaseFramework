using ModuleBased.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Remoting.Messaging;

namespace ModuleBased.AOP.Attributes
{
    /// <summary>
    /// Log method name before/after method execute
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Method)]
    public class SimpleLogAttribute : Attribute, IModuleProxyBeforeAttribute, IModuleProxyAfterAttribute, IModuleProxyExceptionAttribute
    {
        private bool _logError;

        public SimpleLogAttribute(bool logError = false)
        {
            _logError = logError;
        }

        public void OnAfter(MethodInfo method, object[] args, object result, ILogger logger = null)
        {
            string argStr = args.ToArrayString();
            string msg;
            if (result == null)
                msg = $"After execute method : {method.Name}({argStr})";
            else
                msg = $"After execute method : {method.Name}({argStr}) => result";
            logger?.Log(msg);
        }

        public void OnBefore(MethodInfo method, object[] args, ILogger logger = null)
        {
            string argStr = args.ToArrayString();
            logger?.Log($"Before execute method : {method.Name}({argStr})");
        }

        public void OnException(MethodInfo method, Exception e, ILogger logger = null)
        {
            if(_logError)
                logger?.LogError(e);
        }
    }
}