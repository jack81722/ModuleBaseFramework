using System;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.Remoting.Messaging;
using System.Runtime.Remoting.Proxies;

namespace ModuleBased.Proxy
{
    public abstract class ProxyBase : RealProxy
    {
        protected object realObj;

        public ProxyBase(object real)
        {
            realObj = real;
        }

        public override IMessage Invoke(IMessage msg)
        {
            IMethodCallMessage callMethod = msg as IMethodCallMessage;
            MethodInfo targetMethod = callMethod.MethodBase as MethodInfo;
            try
            {
                var result = targetMethod.Invoke(realObj, callMethod.Args);
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
            var result = method.Invoke(realObj, args);
            return result;
        }

        protected MethodInfo GetMethod(int frame)
        {
            StackTrace trace = new StackTrace();
            var frames = trace.GetFrames();
            var method = realObj.GetType().GetMethod(frames[frame].GetMethod().Name);
            return method;
        }
    }

    public abstract class ProxyBase<T> : ProxyBase where T : class
    {
        protected T RealObj => (T)realObj;

        public ProxyBase(object real) : base(real)
        {
            if (real.GetType() != typeof(T))
                throw new ArgumentException("Not the instance of the specific type.");
        }

        public ProxyBase(T real) : base(real) { }
    }
}
