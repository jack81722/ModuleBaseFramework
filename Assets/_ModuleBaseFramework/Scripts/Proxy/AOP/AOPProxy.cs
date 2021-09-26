using System;
using System.Collections.Generic;
using System.Reflection;

namespace ModuleBased.Proxy.AOP
{
    public class AOPProxy<T> : ProxyBase<T> where T : class
    {
        private static readonly EAOPUsage[] _flags = new EAOPUsage[] { EAOPUsage.After, EAOPUsage.Around, EAOPUsage.Before, EAOPUsage.Error };
        private Dictionary<MemberInfo, Dictionary<EAOPUsage, List<IAOPHandler>>> _handlers = new Dictionary<MemberInfo, Dictionary<EAOPUsage, List<IAOPHandler>>>();

        public AOPProxy(T real) : base(real)
        {
            var members = real.GetType().GetMembers(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            foreach (var member in members)
            {
                var attrs = member.GetCustomAttributes(typeof(AOPAttribute), true);
                if (attrs.Length > 0)
                {
                    foreach (var attr in attrs)
                    {
                        var aopAttr = (AOPAttribute)attr;
                        addHandler(member, aopAttr);
                    }
                }
            }
        }

        private void addHandler(MemberInfo member, AOPAttribute attr)
        {
            if (!typeof(IAOPHandler).IsAssignableFrom(attr.HandlerType))
                return;
            var handler = (IAOPHandler)Activator.CreateInstance(attr.HandlerType);
            if (!_handlers.TryGetValue(member, out Dictionary<EAOPUsage, List<IAOPHandler>> dict))
            {
                dict = new Dictionary<EAOPUsage, List<IAOPHandler>>();
                _handlers.Add(member, dict);
            }
            foreach (var flag in _flags)
            {
                if ((attr.Usage & flag) == flag)
                {
                    if (!dict.TryGetValue(flag, out List<IAOPHandler> list))
                    {
                        list = new List<IAOPHandler>();
                        dict.Add(flag, list);
                    }
                    list.Add(handler);
                }
            }
        }

        protected override object InvokeProxyMethod(params object[] args)
        {
            var method = GetMethod(2);
            object result = null;
            try
            {
                AroundInvoke(method, args, null);
                BeforeInvoke(method, args);
                result = method.Invoke(RealObj, args);
                AfterInvoke(method, args, result);
            }
            catch (Exception e)
            {
                OnError(method, args, e);
            }
            finally
            {
                AroundInvoke(method, args, result);
            }
            return result;
        }

        private IEnumerable<IAOPHandler> GetHandlers(MemberInfo member, EAOPUsage usage)
        {
            if (!_handlers.TryGetValue(member, out Dictionary<EAOPUsage, List<IAOPHandler>> dict))
                return null;
            if (!dict.TryGetValue(usage, out List<IAOPHandler> handlers))
                return null;
            return handlers;
        }

        protected void BeforeInvoke(MethodInfo method, object[] args)
        {
            var handlers = GetHandlers(method, EAOPUsage.Before);
            if (handlers == null)
                return;
            foreach (var handler in handlers)
            {
                var aea = new AOPEventArgs
                {
                    Method = method,
                    Args = args,
                    Result = null,
                    Error = null
                };
                handler.OnInvoke(this, aea);
            }
        }

        protected void AfterInvoke(MethodInfo method, object[] args, object result)
        {
            var handlers = GetHandlers(method, EAOPUsage.After);
            if (handlers == null)
                return;
            foreach (var handler in handlers)
            {
                var aea = new AOPEventArgs
                {
                    Method = method,
                    Args = args,
                    Result = null,
                    Error = null
                };
                handler.OnInvoke(this, aea);
            }
        }

        protected void AroundInvoke(MethodInfo method, object[] args, object result)
        {
            var handlers = GetHandlers(method, EAOPUsage.Around);
            if (handlers == null)
                return;
            foreach (var handler in handlers)
            {
                var aea = new AOPEventArgs
                {
                    Method = method,
                    Args = args,
                    Result = null,
                    Error = null
                };
                handler.OnInvoke(this, aea);
            }
        }

        protected void OnError(MethodInfo method, object[] args, Exception e)
        {
            var handlers = GetHandlers(method, EAOPUsage.Error);
            if (handlers == null)
                return;
            foreach (var handler in handlers)
            {
                var aea = new AOPEventArgs
                {
                    Method = method,
                    Args = args,
                    Result = null,
                    Error = null
                };
                handler.OnInvoke(this, aea);
            }
        }
    }
}
