using System;

namespace ModuleBased.Proxy
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class CustomProxyAttribute : Attribute
    {
        public Type ProxyType { get; }

        public CustomProxyAttribute(Type proxy)
        {
            ProxyType = proxy;
        }
    }
}
