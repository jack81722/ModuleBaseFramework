using ModuleBased.AOP.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Messaging;
using System.Runtime.Remoting.Proxies;

namespace ModuleBased.AOP {
    /// <summary>
    /// Instance type of proxy to handle module
    /// </summary>
    public class ModuleProxy : RealProxy, IRemotingTypeInfo, IGameModule
    {
        protected ILogger logger;
        protected object _wrappedObj;
        public Type ItfType { get; }

        public string TypeName
        {
            get { return GetProxiedType().FullName; }
            set { throw new NotSupportedException(); }
        }

        #region -- Cached attributes --
        private Dictionary<MethodInfo, List<IModuleProxyAttribute>> _methodAttrs;
        private Dictionary<MethodInfo, List<IModuleProxyExceptionAttribute>> _exceptionAttrs;
        #endregion

        public ModuleProxy(Type itfType, object obj, ILogger logger) : base(itfType) {
            this.logger = logger;
            _wrappedObj = obj;
            ItfType = itfType;
            _methodAttrs = new Dictionary<MethodInfo, List<IModuleProxyAttribute>>();
            _exceptionAttrs = new Dictionary<MethodInfo, List<IModuleProxyExceptionAttribute>>();
            foreach (var method in itfType.GetMethods(BindingFlags.Public | BindingFlags.Instance))
            {
                CacheBAAttributes(method);
                CacheExcepAttributes(method);
            }
        }

        public override IMessage Invoke(IMessage msg)
        {
            IMethodCallMessage callMethod = msg as IMethodCallMessage;
            MethodInfo targetMethod = callMethod.MethodBase as MethodInfo;
            IMethodReturnMessage returnMethod = null;
            var attrs = GetBAAttributes(targetMethod);
            if (targetMethod != null)
            {
                HandleBeforeExecuting(targetMethod, callMethod.Args, attrs);
                object result;
                try
                {
                    result = targetMethod.Invoke(_wrappedObj, callMethod.Args);
                    returnMethod = new ReturnMessage(result, null, 0,
                        callMethod.LogicalCallContext, callMethod);
                }
                catch(Exception e)
                {   
                    var excepAttrs = GetExcepAttributes(targetMethod);
                    HandleExcpetion(targetMethod, e, excepAttrs);
                    returnMethod = new ReturnMessage(e, callMethod);
                    return returnMethod;
                }
                // after executing will ignore if throw exception
                HandleAfterExecuteing(targetMethod, callMethod.Args, result, attrs);
            }
            return returnMethod;
        }

        #region -- Attribute methods --
        protected IEnumerable<IModuleProxyAttribute> GetBAAttributes(MethodInfo method)
        {
            if (!_methodAttrs.TryGetValue(method, out List<IModuleProxyAttribute> list))
            {
                list =  CacheBAAttributes(method);
            }
            return list;
        }

        protected List<IModuleProxyAttribute> CacheBAAttributes(MethodInfo method)
        {
            var allAttrs = method.GetCustomAttributes();
            var attrs = allAttrs.OfType<IModuleProxyAttribute>();
            var list = new List<IModuleProxyAttribute>(attrs);
            _methodAttrs.Add(method, list);
            return list;
        }

        protected IEnumerable<IModuleProxyExceptionAttribute> GetExcepAttributes(MethodInfo method)
        {
            if (!_exceptionAttrs.TryGetValue(method, out List<IModuleProxyExceptionAttribute> list))
            {
                list = CacheExcepAttributes(method);
            }
            return list;
        }

        protected List<IModuleProxyExceptionAttribute> CacheExcepAttributes(MethodInfo method)
        {
            var allAttrs = method.GetCustomAttributes();
            var excepAttrs = allAttrs.OfType<IModuleProxyExceptionAttribute>();
            var list = new List<IModuleProxyExceptionAttribute>(excepAttrs);
            _exceptionAttrs.Add(method, list);
            return list;
        }
        #endregion

        #region -- Invoke before/after handlers --
        protected virtual void HandleBeforeExecuting(MethodInfo method, object[] args, IEnumerable<IModuleProxyAttribute> attrs)
        {
            foreach (var attr in attrs)
            {
                try
                {
                    attr.OnBefore(method, args, logger);
                }
                catch(Exception e)
                {
                    logger.LogError(e);
                }
            }
        }

        protected virtual void HandleAfterExecuteing(MethodInfo method, object[] args, object result, IEnumerable<IModuleProxyAttribute> attrs)
        {
            foreach (var attr in attrs)
            {
                try
                {
                    attr.OnAfter(method, args, result, logger);
                }catch(Exception e)
                {
                    logger.LogError(e);
                }
            }
        }

        protected virtual void HandleExcpetion(MethodInfo method, Exception e, IEnumerable<IModuleProxyExceptionAttribute> attrs)
        {
            foreach (var attr in attrs)
            {
                try
                {
                    attr.OnException(method, e, logger);

                }
                catch(Exception ie) // internal exception
                {
                    logger.LogError(ie);
                }
            }
        }
        #endregion

        #region -- IGameModule methods --
        public IGameModuleCollection Modules { get; set; }

        public void OnModuleInitialize()
        {
            var mod = _wrappedObj as IGameModule;
            if (mod != null)
                mod.OnModuleInitialize();
        }

        public void OnModuleStart()
        {
            var mod = _wrappedObj as IGameModule;
            if (mod != null)
                mod.OnModuleStart();
        }
        #endregion

        public bool CanCastTo(Type fromType, object o)
        {
            return fromType == typeof(IGameModule) || fromType == ItfType;
        }
    }


    /// <summary>
    /// Instance type of proxy to handle module
    /// </summary>
    public class ModuleProxy<T> : ModuleProxy where T : class {
        public ModuleProxy(T obj, ILogger logger) : base(typeof(T), obj, logger) { }
    }
}
