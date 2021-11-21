using ModuleBased.Injection;
using ModuleBased.Utils;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace ModuleBased.Proxy
{
    public class InjectProxy<T> : ProxyBase<T>, IEventInject where T : class
    {
        [Inject]
        protected IGameCore core;
        private Dictionary<MethodInfo, _InjectMethod> _injectMethods;

        public InjectProxy(object real) : base(real)
        {
            _injectMethods = new Dictionary<MethodInfo, _InjectMethod>();
        }

        private void analyzeInjectMethod()
        {
            var type = GetRoot().GetType();
            var methods = type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic);
            foreach (var method in methods)
            {
                if (method.GetParameters().ExistsAttr<ParameterInfo, InjectAttribute>(true))
                {
                    _injectMethods.Add(method, new _InjectMethod(core, method));
                }
            }
        }

        protected object[] modifyInjectableArgs(MethodInfo method, object[] args)
        {

            if(_injectMethods.TryGetValue(method, out _InjectMethod m))
            {
                var injected = m.ModifyInjectableArgs(args);
                return injected;
            }
            return args;
        }

        public void OnInject()
        {
            core.Inject(GetInner());
            analyzeInjectMethod();
        }

        private class _InjectMethod
        {
            private IGameCore _core;
            private MethodInfo _methodInfo;
            private InjectAttribute[] _paramAttrs;
            public _InjectMethod(IGameCore core, MethodInfo method)
            {
                _core = core;
                _methodInfo = method;
                anaylyze();
            }

            private void anaylyze()
            {
                var ps = _methodInfo.GetParameters();
                _paramAttrs = new InjectAttribute[ps.Length];
                int i = 0;
                foreach (var p in ps)
                {
                    if (p.IsDefined(typeof(InjectAttribute)))
                    {
                        _paramAttrs[i++] = p.GetCustomAttribute<InjectAttribute>();
                    }
                    else
                    {
                        i++;
                    }
                }
            }

            public object[] ModifyInjectableArgs(object[] args)
            {
                var modifiedArgs = new object[args.Length];
                var paramList = _methodInfo.GetParameters();
                for (int i = 0; i < args.Length; i++)
                {
                    var attr = _paramAttrs[i];
                    if (attr != null)
                    {
                        modifiedArgs[i] = _core.Get(paramList[i].ParameterType, attr.Identity);
                    }
                    else
                    {
                        modifiedArgs[i] = args[i];
                    }
                }
                return modifiedArgs;
            }
        }
    }
}
