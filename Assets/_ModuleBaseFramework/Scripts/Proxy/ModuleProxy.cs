using ModuleBased.Models;
using System;
using System.Collections;
using System.Reflection;
using System.Runtime.Remoting.Messaging;

namespace ModuleBased.Proxy
{
    public class ModuleProxyBase<T> : ProxyBase<T>, IGameModule where T : class, IGameModule
    {
        public ModuleProxyBase(T real) : base(real) { }

        public ILogger Logger { get => RealObj.Logger; set => RealObj.Logger = value; }
        public IGameModuleCollection Modules { get => RealObj.Modules; set => RealObj.Modules = value; }

        public IEnumerator InitializeModule(IProgress<ProgressInfo> progress)
        {
            return RealObj.InitializeModule(progress);
        }

        public void StartModule()
        {
            RealObj.StartModule();
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

        
    }
}
