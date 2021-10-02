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

}
