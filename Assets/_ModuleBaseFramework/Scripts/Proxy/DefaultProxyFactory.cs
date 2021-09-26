using System;

namespace ModuleBased.Proxy
{
    public class DefaultProxyFactory : IProxyFactory
    {
        public object CreateProxy(object realObj)
        {
            var type = realObj.GetType();
            if (type.IsDefined(typeof(ProxyAttribute), true))
            {
                var attr = type.GetCustomAttributes(typeof(ProxyAttribute), true)[0] as ProxyAttribute;
                var proxy = Activator.CreateInstance(attr.Target, new object[] { realObj });
                return proxy;
            }
            return realObj;
        }

        public T CreateProxy<T>(T realObj) where T : class
        {
            return CreateProxy(realObj) as T;
        }
    }
}
