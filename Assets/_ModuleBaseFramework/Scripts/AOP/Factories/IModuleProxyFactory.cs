using System;

namespace ModuleBased.AOP {
    /// <summary>
    /// Factory of module proxy
    /// </summary>
    public interface IModuleProxyFactory {
        T CreateProxy<T>(T obj) where T : class;

        object CreateProxy(Type itfType, object modObj);
    }

}