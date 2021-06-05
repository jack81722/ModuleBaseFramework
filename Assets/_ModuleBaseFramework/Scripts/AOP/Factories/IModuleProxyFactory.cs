using System;

namespace ModuleBased.AOP {
    /// <summary>
    /// Factory of module proxy
    /// </summary>
    public interface IModuleProxyFactory {
        ModuleProxy<T> CreateProxy<T>(T obj, ILogger logger = null) where T : class;

        ModuleProxy CreateProxy(Type itfType, object modObj, ILogger logger = null);
    }

}