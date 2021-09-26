using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace ModuleBased.Proxy.AOP
{
    public class AOPProxy<T> : ProxyBase<T> where T : class
    {
        private Dictionary<EAOPUsage, List<IAOPHandler>> _handlers = new Dictionary<EAOPUsage, List<IAOPHandler>>
        {
            { EAOPUsage.After, new List<IAOPHandler>() },
            { EAOPUsage.Before, new List<IAOPHandler>() },
            { EAOPUsage.Around, new List<IAOPHandler>() },
            { EAOPUsage.Error, new List<IAOPHandler>() }
        };

        public AOPProxy(T real) : base(real)
        {
            var members = real.GetType().GetMembers(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            foreach (var member in members)
            {
                var attrs = member.GetCustomAttributes(typeof(AOPAttribute), true);
                if (attrs.Length > 0)
                {
                    UnityEngine.Debug.Log(member.Name);
                    foreach (var attr in attrs)
                    {
                        var aopAttr = (AOPAttribute)attr;
                        if (!typeof(IAOPHandler).IsAssignableFrom(aopAttr.HandlerType))
                            continue;
                        var handler = (IAOPHandler)Activator.CreateInstance(aopAttr.HandlerType);
                        foreach(var flag in _handlers.Keys)
                        {
                            if ((aopAttr.Usage & flag) == flag)
                            {
                                _handlers[flag].Add(handler);
                            }
                        }
                    }
                }
            }
        }

        protected override object InvokeProxyMethod(params object[] args)
        {
            var method = GetMethod(2);
            UnityEngine.Debug.Log(method == null);
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

        protected void BeforeInvoke(MethodInfo method, object[] args)
        {            
            foreach(var handler in _handlers[EAOPUsage.Before])
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
            foreach (var handler in _handlers[EAOPUsage.After])
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

        protected void AroundInvoke(MethodInfo method, object[] args, object result) {
            foreach (var handler in _handlers[EAOPUsage.Around])
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
            foreach (var handler in _handlers[EAOPUsage.Error])
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
