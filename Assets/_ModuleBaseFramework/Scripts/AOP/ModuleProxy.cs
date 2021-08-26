using ModuleBased.AOP.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Messaging;
using System.Runtime.Remoting.Proxies;

namespace ModuleBased.AOP
{
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
        private Dictionary<MethodInfo, List<IModuleProxyBeforeAttribute>> _beforeAttrs;
        private Dictionary<MethodInfo, List<IModuleProxyAfterAttribute>> _afterAttrs;
        private Dictionary<MethodInfo, List<IModuleProxyExceptionAttribute>> _exceptionAttrs;
        #endregion

        public ModuleProxy(Type itfType, object obj, ILogger logger) : base(itfType)
        {
            this.logger = logger;
            _wrappedObj = obj;
            ItfType = itfType;
            _beforeAttrs = new Dictionary<MethodInfo, List<IModuleProxyBeforeAttribute>>();
            _afterAttrs = new Dictionary<MethodInfo, List<IModuleProxyAfterAttribute>>();
            _exceptionAttrs = new Dictionary<MethodInfo, List<IModuleProxyExceptionAttribute>>();
            foreach (var method in itfType.GetMethods(BindingFlags.Public | BindingFlags.Instance))
            {
                CacheBeforeAttributes(method);
                CacheAfterAttributes(method);
                CacheExcepAttributes(method);
            }
        }

        public override IMessage Invoke(IMessage msg)
        {
            IMethodCallMessage callMethod = msg as IMethodCallMessage;
            MethodInfo targetMethod = callMethod.MethodBase as MethodInfo;
            IMethodReturnMessage returnMethod = null;
            var beforeAttrs = GetBeforeAttributes(targetMethod);
            if (targetMethod != null)
            {
                HandleBeforeExecuting(targetMethod, callMethod.Args, beforeAttrs);
                object result;
                try
                {
                    result = targetMethod.Invoke(_wrappedObj, callMethod.Args);
                    returnMethod = new ReturnMessage(result, null, 0,
                        callMethod.LogicalCallContext, callMethod);
                }
                catch (Exception e)
                {
                    var excepAttrs = GetExcepAttributes(targetMethod);
                    HandleExcpetion(targetMethod, e, excepAttrs);
                    returnMethod = new ReturnMessage(e, callMethod);
                    return returnMethod;
                }
                var afterAttrs = GetAfterAttributes(targetMethod);
                // after executing will ignore if throw exception
                HandleAfterExecuteing(targetMethod, callMethod.Args, result, afterAttrs);
            }
            return returnMethod;
        }

        #region -- Attribute methods --
        protected IEnumerable<IModuleProxyBeforeAttribute> GetBeforeAttributes(MethodInfo method)
        {
            if (!_beforeAttrs.TryGetValue(method, out List<IModuleProxyBeforeAttribute> list))
            {
                list = CacheBeforeAttributes(method);
            }
            return list;
        }

        protected List<IModuleProxyBeforeAttribute> CacheBeforeAttributes(MethodInfo method)
        {
            var allAttrs = method.GetCustomAttributes();
            var attrs = allAttrs.OfType<IModuleProxyBeforeAttribute>();
            var list = new List<IModuleProxyBeforeAttribute>(attrs);
            _beforeAttrs.Add(method, list);
            return list;
        }

        protected IEnumerable<IModuleProxyAfterAttribute> GetAfterAttributes(MethodInfo method)
        {
            if (!_afterAttrs.TryGetValue(method, out List<IModuleProxyAfterAttribute> list))
            {
                list = CacheAfterAttributes(method);
            }
            return list;
        }

        protected List<IModuleProxyAfterAttribute> CacheAfterAttributes(MethodInfo method)
        {
            var allAttrs = method.GetCustomAttributes();
            var attrs = allAttrs.OfType<IModuleProxyAfterAttribute>();
            var list = new List<IModuleProxyAfterAttribute>(attrs);
            _afterAttrs.Add(method, list);
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
        protected virtual void HandleBeforeExecuting(MethodInfo method, object[] args, IEnumerable<IModuleProxyBeforeAttribute> attrs)
        {
            foreach (var attr in attrs)
            {
                try
                {
                    attr.OnBefore(method, args, logger);
                }
                catch (Exception e)
                {
                    logger.LogError(e);
                }
            }
        }

        protected virtual void HandleAfterExecuteing(MethodInfo method, object[] args, object result, IEnumerable<IModuleProxyAfterAttribute> attrs)
        {
            foreach (var attr in attrs)
            {
                try
                {
                    attr.OnAfter(method, args, result, logger);
                }
                catch (Exception e)
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
                catch (Exception ie) // internal exception
                {
                    logger.LogError(ie);
                }
            }
        }
        #endregion

        #region -- IGameModule methods --
        public IGameModuleCollection Modules { get; set; }
        public ILogger Logger
        {
            get
            {
                return logger;
            }
            set
            {
                logger = value;
                IGameModule mod = _wrappedObj as IGameModule;
                if (mod != null)
                    mod.Logger = logger;
            }
        }

        public void InitializeModule()
        {
            var mod = _wrappedObj as IGameModule;
            if (mod != null)
                mod.InitializeModule();
        }

        public void StartModule()
        {
            var mod = _wrappedObj as IGameModule;
            if (mod != null)
                mod.StartModule();
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
    public class ModuleProxy<T> : ModuleProxy where T : class
    {
        public ModuleProxy(T obj, ILogger logger) : base(typeof(T), obj, logger) { }
    }
}
