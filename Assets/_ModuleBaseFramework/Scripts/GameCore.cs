using ModuleBased.AOP;
using ModuleBased.DAO;

namespace ModuleBased {
    public class GameCore : IGameCore {
        public IGameModuleCollection Modules { get; }
        public IGameViewCollection Views { get; }
        public IGameDaoCollection Daos { get; }

        public GameCore(ILogger logger, IModuleProxyFactory proxyFactory) {
            Daos = new DefaultGameDaoCollection();
            Modules = new DefaultGameModuleCollection(Daos, logger, proxyFactory);
            Views = new DefaultGameViewCollection(logger, Modules);
            
        }
    }
}