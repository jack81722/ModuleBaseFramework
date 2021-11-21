using System;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.Remoting.Messaging;
using System.Runtime.Remoting.Proxies;

namespace ModuleBased.Proxy
{
    public abstract class ProxyBase : RealProxy, IProxy, IDisposable
    {
        private object _innerObj;
        private object _rootObj;

        public ProxyBase(Type targetType, object inner) : base(targetType)
        {
            _innerObj = inner;
            var proxy = inner as IProxy;
            if (proxy != null)
            {
                _rootObj = proxy.GetRoot();
            }
            else
            {
                _rootObj = inner;
            }
        }

        public override IMessage Invoke(IMessage msg)
        {
            IMethodCallMessage callMethod = msg as IMethodCallMessage;
            MethodInfo targetMethod = callMethod.MethodBase as MethodInfo;
            try
            {
                var result = targetMethod.Invoke(_innerObj, callMethod.Args);
                return new ReturnMessage(result, null, 0, callMethod.LogicalCallContext, callMethod);
            }
            catch (Exception e)
            {
                return new ReturnMessage(e, callMethod);
            }
        }

        protected virtual object InvokeProxyMethod(params object[] args)
        {
            var method = GetMethod(2);
            var result = method.Invoke(_innerObj, args);
            return result;
        }

        protected MethodInfo GetMethod(int frame)
        {
            StackTrace trace = new StackTrace();
            var frames = trace.GetFrames();
            var method = _innerObj.GetType().GetMethod(frames[frame].GetMethod().Name);
            return method;
        }
        
        public object GetInner()
        {
            return _innerObj;
        }

        public object GetRoot()
        {
            return _rootObj;
        }

        public void Dispose()
        {
            (_innerObj as IDisposable)?.Dispose();
        }
    }

    public abstract class ProxyBase<T> : ProxyBase where T : class
    {

        public ProxyBase(object real) : base(typeof(T), real)
        {
            Type assertType = typeof(T);
            Type realType = real.GetType();
            if (realType != assertType &&
                realType.IsSubclassOf(assertType) &&
                assertType.IsAssignableFrom(realType))
                throw new ArgumentException($"Not the instance of the specific type ({typeof(T).Name}).");
        }

        public ProxyBase(T real) : base(typeof(T), real) 
        {
            Type assertType = typeof(T);
            Type realType = real.GetType();
            if (realType != assertType &&
                realType.IsSubclassOf(assertType) &&
                assertType.IsAssignableFrom(realType))
                throw new ArgumentException($"Not the instance of the specific type ({typeof(T).Name}).");
        }
    }

    public interface IProxy
    {
        object GetInner();

        object GetRoot();
    }
}
