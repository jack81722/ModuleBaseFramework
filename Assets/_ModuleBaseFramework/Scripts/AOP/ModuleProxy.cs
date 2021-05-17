#if true
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
    public class ModuleProxy<T> : RealProxy, IRemotingTypeInfo, IGameModule where T : class {
        private T _wrappedObj;

        public string TypeName {
            get { return GetProxiedType().FullName; }
            set { throw new NotSupportedException(); }
        }

        private Dictionary<MethodInfo, List<IModuleProxyAttribute>> _methodAttrs;

        public ModuleProxy(T obj) : base(typeof(T)) {
            _wrappedObj = obj;
            _methodAttrs = new Dictionary<MethodInfo, List<IModuleProxyAttribute>>();
        }

        public override IMessage Invoke(IMessage msg) {
            IMethodCallMessage callMethod = msg as IMethodCallMessage;
            MethodInfo targetMethod = callMethod.MethodBase as MethodInfo;
            IMethodReturnMessage returnMethod = null;
            var attrs = GetAttributes(targetMethod);
            if (targetMethod != null) {
                HandleBeforeExecuting(targetMethod, callMethod.Args, attrs);
                var result = targetMethod.Invoke(_wrappedObj, callMethod.Args);
                HandleAfterExecuteing(targetMethod, callMethod.Args, result, attrs);
                returnMethod = new ReturnMessage(result, null, 0,
                  callMethod.LogicalCallContext, callMethod);
            }
            return returnMethod;
        }

        private IEnumerable<IModuleProxyAttribute> GetAttributes(MethodInfo method) {
            if(!_methodAttrs.TryGetValue(method, out List<IModuleProxyAttribute> list)) {
                return CacheAttributes(method);
            }
            return list;
        }

        private List<IModuleProxyAttribute> CacheAttributes(MethodInfo method) {
            var attrs = method.GetCustomAttributes().OfType<IModuleProxyAttribute>();
            var list = new List<IModuleProxyAttribute>(attrs);
            _methodAttrs.Add(method, list);
            return list;
        }

        #region -- Invoke before/after handlers --
        private void HandleBeforeExecuting(MethodInfo method, object[] args, IEnumerable<IModuleProxyAttribute> attrs) {
            foreach (var attr in attrs)
                attr.OnBefore(method, args);
        }

        private void HandleAfterExecuteing(MethodInfo method, object[] args, object result, IEnumerable<IModuleProxyAttribute> attrs) {
            foreach (var attr in attrs)
                attr.OnAfter(method, args, result);
        }
        #endregion

        #region -- IGameModule methods --
        public void OnModuleInitialize() {
            var mod = _wrappedObj as IGameModule;
            if (mod != null)
                mod.OnModuleInitialize();
        }

        public void OnModuleStart() {
            var mod = _wrappedObj as IGameModule;
            if (mod != null)
                mod.OnModuleStart();
        }
        #endregion

        public bool CanCastTo(Type fromType, object o) {
            return fromType == typeof(IGameModule) || fromType == typeof(T);
        }
    }
}
#endif