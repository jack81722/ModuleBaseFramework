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
        public ModuleProxy<T> CreateProxy<T>(T obj, ILogger logger = null) where T : class {
            return new ModuleProxy<T>(obj, logger);
        }

        public ModuleProxy CreateProxy(Type itfType, object modObj, ILogger logger = null) {
            Type modType = modObj.GetType();
            if (!itfType.IsAssignableFrom(modType))
                throw new InvalidCastException($"Module({modType.Name}) must implemented by interface({itfType.Name}).");
            Type proxyType = typeof(ModuleProxy<>).MakeGenericType(itfType);
            ModuleProxy proxy = Activator.CreateInstance(proxyType, new object[] { modObj, logger }) as ModuleProxy;
            return proxy;
        }
    }
}
#endif