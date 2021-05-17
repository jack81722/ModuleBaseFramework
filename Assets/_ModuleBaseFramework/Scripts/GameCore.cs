using ModuleBased.AOP;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace ModuleBased {
    public class GameCore : IGameCore {
        public IGameModuleCollection Modules { get; }
        public IGameViewCollection Views { get; }

        public GameCore(ILogger logger, IModuleProxyFactory proxyFactory) {
            Modules = new DefaultGameModuleCollection(logger, proxyFactory);
            Views = new DefaultGameViewCollection(logger, Modules);
        }
    }
}