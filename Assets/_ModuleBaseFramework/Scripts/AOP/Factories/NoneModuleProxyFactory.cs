using System;

namespace ModuleBased.AOP.Factories {

    /// <summary>
    /// None module proxy factory
    /// </summary>
    /// <remarks>
    /// No spawn any proxy and return actual instance while add module
    /// </remarks>
    public class NoneModuleProxyFactory : IModuleProxyFactory {
        public T CreateProxy<T>(T obj) where T : class {
            return obj;
        }

        public object CreateProxy(Type itfType, object modObj) {
            return modObj;
        }
    }
}