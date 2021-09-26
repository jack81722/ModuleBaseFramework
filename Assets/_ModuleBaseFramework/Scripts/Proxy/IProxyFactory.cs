using System;

namespace ModuleBased.Proxy
{
    public interface IProxyFactory
    {
        object CreateProxy(object realObj);
        
    }
}
