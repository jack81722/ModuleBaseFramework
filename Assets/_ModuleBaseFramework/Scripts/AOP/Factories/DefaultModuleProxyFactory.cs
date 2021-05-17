#if true
using System;
using System.Runtime.Remoting.Proxies;

namespace ModuleBased.AOP.Factories {
    /// <summary>
    /// Default module proxy factory
    /// </summary>
    /// <remarks>
    /// Spawn module proxy while adding module
    /// </remarks>
    public class DefaultModuleProxyFactory : IModuleProxyFactory {
        public T CreateProxy<T>(T obj) where T : class {
            return new ModuleProxy<T>(obj).GetTransparentProxy() as T;
        }

        public object CreateProxy(Type itfType, object modObj) {
            Type modType = modObj.GetType();
            if (!itfType.IsAssignableFrom(modType))
                throw new InvalidCastException($"Module({modType.Name}) must implemented by interface({itfType.Name}).");
            Type proxyType = typeof(ModuleProxy<>).MakeGenericType(itfType);
            object proxy = Activator.CreateInstance(proxyType, new object[] { modObj });
            var transProxy = ((RealProxy)proxy).GetTransparentProxy();
            return Convert.ChangeType(transProxy, itfType);
        }
    }
}
#endif