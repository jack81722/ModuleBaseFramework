using System;

namespace ModuleBased.Proxy
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class ProxyAttribute : Attribute
    {
        public Type Target { get; }

        public ProxyAttribute(Type proxy)
        {
            Target = proxy;
        }
    }
}
