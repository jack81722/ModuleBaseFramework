using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace ModuleBased.Proxy.AOP
{
    public abstract class AOPAttribute : Attribute
    {
        public EAOPStatus Usage { get; }
        public Type HandlerType { get; }

        public AOPAttribute(EAOPStatus usage, Type handlerType)
        {
            Usage = usage;
            HandlerType = handlerType;
        }
    }

    [Flags]
    public enum EAOPStatus : byte
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
        public EAOPStatus Status;
        public object[] Args;
        public object Result;
        public Exception Error;
    }
}
