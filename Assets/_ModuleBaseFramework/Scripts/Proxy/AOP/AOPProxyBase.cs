using ModuleBased.Proxy.AOP;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Remoting.Messaging;

namespace ModuleBased.Proxy
{
    public class AOPProxyBase<T> : InjectProxy<T> where T : class
    {
        private static readonly EAOPStatus[] _flags = new EAOPStatus[] { EAOPStatus.After, EAOPStatus.Around, EAOPStatus.Before, EAOPStatus.Error };
        private Dictionary<MemberInfo, Dictionary<EAOPStatus, List<IAOPHandler>>> _handlers = new Dictionary<MemberInfo, Dictionary<EAOPStatus, List<IAOPHandler>>>();

        public AOPProxyBase(object real) : base(real)
        {
            instantiateAOPHandler();
        }

        public override IMessage Invoke(IMessage msg)
        {
            IMethodCallMessage callMethod = msg as IMethodCallMessage;
            MethodInfo targetMethod = callMethod.MethodBase as MethodInfo;
            object[] args = callMethod.Args;
            object result;

            aroundInvoke(targetMethod, args, null);
            beforeInvoke(targetMethod, args);
            try
            {
                //args = ModifyInjectableArgs(realObj.GetType().GetMethod(targetMethod.Name), args);
                result = targetMethod.Invoke(GetInner(), args);
            }
            catch (Exception e)
            {
                return new ReturnMessage(e, callMethod);
            }
            afterInvoke(targetMethod, args, result);
            aroundInvoke(targetMethod, args, result);
            return new ReturnMessage(result, null, 0, callMethod.LogicalCallContext, callMethod);
        }

        protected override object InvokeProxyMethod(params object[] args)
        {
            var method = GetMethod(2);
            object result = null;
            args = modifyInjectableArgs(GetInner().GetType().GetMethod(method.Name), args);
            aroundInvoke(method, args, null);
            beforeInvoke(method, args);
            try
            {   
                result = method.Invoke(GetInner(), args);
            }
            catch (Exception e)
            {
                errorInvoke(method, args, e);
            }
            finally
            {
                afterInvoke(method, args, result);
                aroundInvoke(method, args, result);
            }
            return result;
        }

        private void instantiateAOPHandler()
        {
            var members = GetInner().GetType().GetMembers(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            foreach (var member in members)
            {
                var attrs = member.GetCustomAttributes(typeof(AOPAttribute), true);
                foreach (var attr in attrs)
                {
                    var aopAttr = (AOPAttribute)attr;
                    addHandler(member, aopAttr);
                }
            }
        }

        private void addHandler(MemberInfo member, AOPAttribute attr)
        {
            if (!typeof(IAOPHandler).IsAssignableFrom(attr.HandlerType))
                return;
            var handler = (IAOPHandler)Activator.CreateInstance(attr.HandlerType);
            if (!_handlers.TryGetValue(member, out Dictionary<EAOPStatus, List<IAOPHandler>> dict))
            {
                dict = new Dictionary<EAOPStatus, List<IAOPHandler>>();
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

        private IEnumerable<IAOPHandler> getHandlers(MemberInfo member, EAOPStatus usage)
        {
            if (!_handlers.TryGetValue(member, out Dictionary<EAOPStatus, List<IAOPHandler>> dict))
                return null;
            if (!dict.TryGetValue(usage, out List<IAOPHandler> handlers))
                return null;
            return handlers;
        }

        protected void beforeInvoke(MethodInfo method, object[] args)
        {
            var handlers = getHandlers(method, EAOPStatus.Before);
            if (handlers == null)
                return;
            foreach (var handler in handlers)
            {
                var aea = new AOPEventArgs
                {
                    Method = method,
                    Status = EAOPStatus.Before,
                    Args = args,
                    Result = null,
                    Error = null
                };
                try
                {
                    handler.OnInvoke(this, aea);
                }
                catch (Exception e)
                {
                    aea = new AOPEventArgs
                    {
                        Method = method,
                        Status = EAOPStatus.Before,
                        Args = args,
                        Result = null,
                        Error = e
                    };
                    handler.OnInvoke(this, aea);
                }
            }
        }

        protected void afterInvoke(MethodInfo method, object[] args, object result)
        {
            var handlers = getHandlers(method, EAOPStatus.After);
            if (handlers == null)
                return;
            foreach (var handler in handlers)
            {
                var aea = new AOPEventArgs
                {
                    Method = method,
                    Status = EAOPStatus.After,
                    Args = args,
                    Result = result,
                    Error = null
                };
                try
                {
                    handler.OnInvoke(this, aea);
                }
                catch (Exception e)
                {
                    aea = new AOPEventArgs
                    {
                        Method = method,
                        Status = EAOPStatus.Before,
                        Args = args,
                        Result = null,
                        Error = e
                    };
                    handler.OnInvoke(this, aea);
                }
            }
        }

        protected void aroundInvoke(MethodInfo method, object[] args, object result)
        {
            var handlers = getHandlers(method, EAOPStatus.Around);
            if (handlers == null)
                return;
            foreach (var handler in handlers)
            {
                var aea = new AOPEventArgs
                {
                    Method = method,
                    Status = EAOPStatus.Around,
                    Args = args,
                    Result = result,
                    Error = null
                };
                try
                {
                    handler.OnInvoke(this, aea);
                }
                catch (Exception e)
                {
                    aea = new AOPEventArgs
                    {
                        Method = method,
                        Status = EAOPStatus.Before,
                        Args = args,
                        Result = null,
                        Error = e
                    };
                    handler.OnInvoke(this, aea);
                }
            }
        }

        protected void errorInvoke(MethodInfo method, object[] args, Exception e)
        {
            var handlers = getHandlers(method, EAOPStatus.Error);
            if (handlers == null)
                return;
            foreach (var handler in handlers)
            {
                var aea = new AOPEventArgs
                {
                    Method = method,
                    Status = EAOPStatus.Error,
                    Args = args,
                    Result = null,
                    Error = e
                };
                handler.OnInvoke(this, aea);
            }
        }
    }
}
