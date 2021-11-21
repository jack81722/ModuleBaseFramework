using ModuleBased.Injection;
using System;

namespace ModuleBased.Proxy
{ 
    public static class ProxyExtension
    {
        public static Contraction WrapCustomProxy(this Contraction contraction, Type proxyType)
        {
            contraction.InterceptorTypes.Add(proxyType);
            return contraction;
        }

        public static Contraction WrapCustomProxy<T>(this Contraction contraction) where T : ProxyBase
        {
            return contraction.WrapCustomProxy(typeof(T));
        }
    }
}
