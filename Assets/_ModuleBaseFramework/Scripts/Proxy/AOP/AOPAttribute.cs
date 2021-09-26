using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace ModuleBased.Proxy.AOP
{
    public abstract class AOPAttribute : Attribute
    {
        public EAOPUsage Usage { get; }
        public Type HandlerType { get; }

        public AOPAttribute(EAOPUsage usage, Type handlerType)
        {
            Usage = usage;
            HandlerType = handlerType;
        }
    }

    [Flags]
    public enum EAOPUsage : byte
    {
        Around = 1,
        Before = 2,
        After = 4,
        Error = 8
    }

    public interface IAOPHandler
    {
        void OnInvoke(object sender, AOPEventArgs args);
    }

    public struct AOPEventArgs
    {
        public MethodInfo Method;
        public object[] Args;
        public object Result;
        public Exception Error;
    }
}
