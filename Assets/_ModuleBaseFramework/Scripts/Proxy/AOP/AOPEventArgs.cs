using System;
using System.Reflection;

namespace ModuleBased.Proxy.AOP
{
    [Flags]
    public enum EAOPStatus : byte
    {
        Around = 1,
        Before = 2,
        After = 4,
        Error = 8
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
