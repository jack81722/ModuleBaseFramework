using System;
using System.Collections;
using ModuleBased.Models;

namespace ModuleBased.Proxy.AOP
{
    public class ModuleAOP<T> : AOPProxy<T>, IGameModule where T : class, IGameModule
    {
        public ModuleAOP(T real) : base(real) { }

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
    }
}
