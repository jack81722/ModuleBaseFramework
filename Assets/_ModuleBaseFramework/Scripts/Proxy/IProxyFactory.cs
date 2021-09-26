using System;

namespace ModuleBased.Proxy
{
    public interface IProxyFactory
    {
        object CreateProxy(object realObj);

        T CreateProxy<T>(T realObj) where T : class;
    }
}
