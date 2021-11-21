using ModuleBased.Injection;
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
            analyzeInjectMethod();
        }

        private void analyzeInjectMethod()
        {
            var type= GetRoot().GetType();
            var methods = type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic);
            foreach(var method in methods)
            {
                if (method.IsDefined(typeof(InjectAttribute)))
                {
                    _injectMethods.Add(method, new _InjectMethod(core, method));
                }
            }
        }

        protected object[] modifyInjectableArgs(MethodInfo method, object[] args)
        {
            if (method.IsDefined(typeof(InjectAttribute)))
            {
                var modifiedArgs = new object[args.Length];
                var paramList = method.GetParameters();
                for (int i = 0; i < args.Length; i++)
                {
                    if (paramList[i].IsDefined(typeof(InjectAttribute)))
                    {
                        var attr = paramList[i].GetCustomAttribute<InjectAttribute>();
                        modifiedArgs[i] = core.Get(paramList[i].ParameterType, attr.Identity);
                        continue;
                    }
                    modifiedArgs[i] = args[i];
                }
                return modifiedArgs;
            }
            return args;
        }

        public void OnInject()
        {
            core.Inject(GetInner());
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
                foreach(var p in ps)
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

            private object[] modifyInjectableArgs(object[] args)
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
