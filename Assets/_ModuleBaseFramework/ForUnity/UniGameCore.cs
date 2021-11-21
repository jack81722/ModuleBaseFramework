using ModuleBased.DAO;
using ModuleBased.Injection;
using ModuleBased.Proxy;
using ModuleBased.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ModuleBased.ForUnity
{
    public class UniGameCore : MonoBehaviour
    {
        #region -- Singleton --
        /// <summary>
        /// Singleton of game core
        /// </summary>
        private static UniGameCore _singleton;
        public static UniGameCore Singleton {
            get
            {
                if (_singleton == null)
                {
                    _singleton = FindObjectOfType<UniGameCore>();
                    if (_singleton == null)
                    {
                        throw new NullReferenceException("UniGameCore must be place in scene.");
                    }
                }
                return _singleton;
            }
        }
        #endregion

        private IGameCore _core;

        public IGameCore Core => _core;

        private Dictionary<Scene, List<KeyValuePair<Type, object>>> _injectableList;

        protected void Awake()
        {
            InitializeCore();
        }

        public void InitializeCore()
        {
            _core = new GameCore();
            _injectableList = new Dictionary<Scene, List<KeyValuePair<Type, object>>>();
            Setup();
            _core.Launch(DefaultState());
        }

        private void Setup()
        {
            CustomSetup(_core);

            // search all injectable in children
            SearchAndSetup(FindObjectsOfType<MonoBehaviour>());
            SceneManager.sceneLoaded += LoadSceneAndSetup2;
            SceneManager.sceneUnloaded += UnloadSceneAndSetup;
        }

        private void SearchAndSetup(IEnumerable<MonoBehaviour> monos)
        {
            foreach(var mono in monos)
            {
                var monoType = mono.GetType();
                // injectable
                if (monoType.IsDefined(typeof(InjectableAttribute), false))
                {
                    var attrs = monoType.GetCustomAttributes<InjectableAttribute>();
                    foreach (var attr in attrs)
                    {
                        if (!_core.TryAdd(attr.ContractType, out Contraction contraction, attr.Identity))
                            continue;
                        contraction
                            .SetScope(attr.ContractScope)
                            .Concrete(monoType, mono);
                        if (monoType.IsDefined(typeof(CustomProxyAttribute)))
                        {
                            var proxyAttrs = monoType.GetCustomAttributes<CustomProxyAttribute>();
                            foreach (var proxyAttr in proxyAttrs)
                                contraction.WrapCustomProxy(proxyAttr.ProxyType);
                        }
                    }
                }

                if (monoType.IsDefined(typeof(InjectableFactoryAttribute), true))
                {
                    var attrs = monoType.GetCustomAttributes<InjectableFactoryAttribute>();
                    foreach (var attr in attrs)
                    {
                        if (!typeof(IFactory).IsAssignableFrom(monoType))
                        {
                            throw new InvalidCastException($"The {monoType.Name} is not implemented factory.");
                        }
                        if (!_core.TryAdd(attr.ContractType, out Contraction contraction, attr.Identity))
                            continue;
                        contraction
                            .SetScope(attr.ContractScope)
                            .FromFactory((IFactory)mono);
                        if (monoType.IsDefined(typeof(CustomProxyAttribute)))
                        {
                            var proxyAttrs = monoType.GetCustomAttributes<CustomProxyAttribute>();
                            foreach (var proxyAttr in proxyAttrs)
                                contraction.WrapCustomProxy(proxyAttr.ProxyType);
                        }
                    }
                }
            }
        }

        private void LoadSceneAndSetup(Scene scene, LoadSceneMode mode)
        {
            var list = new List<KeyValuePair<Type, object>>();
            _injectableList.Add(scene, list);
            var monos = scene.GetRootGameObjects()
                .Deverge((go) => go.GetComponentsInChildren<MonoBehaviour>());
            var singletons = new List<object>();
            foreach (var mono in monos)
            {
                var monoType = mono.GetType();
                // injectable
                if (monoType.IsDefined(typeof(InjectableAttribute), false))
                {
                    var attrs = monoType.GetCustomAttributes<InjectableAttribute>();
                    foreach (var attr in attrs)
                    {
                        if (!_core.TryAdd(attr.ContractType, out Contraction contraction, attr.Identity))
                            continue;
                        contraction
                            .SetScope(attr.ContractScope)
                            .Concrete(monoType, mono);
                        if (monoType.IsDefined(typeof(CustomProxyAttribute)))
                        {
                            var proxyAttrs = monoType.GetCustomAttributes<CustomProxyAttribute>();
                            foreach (var proxyAttr in proxyAttrs)
                                contraction.WrapCustomProxy(proxyAttr.ProxyType);
                        }
                        list.Add(new KeyValuePair<Type, object>(attr.ContractType, attr.Identity));
                        if(attr.ContractScope == EContractScope.Singleton)
                        {
                            singletons.Add(contraction.Instantiate());
                        }
                    }
                }

                if (monoType.IsDefined(typeof(InjectableFactoryAttribute), true))
                {
                    var attrs = monoType.GetCustomAttributes<InjectableFactoryAttribute>();
                    foreach (var attr in attrs)
                    {
                        if (!typeof(IFactory).IsAssignableFrom(monoType))
                        {
                            throw new InvalidCastException($"The {monoType.Name} is not implemented factory.");
                        }

                        if (!_core.TryAdd(attr.ContractType, out Contraction contraction, attr.Identity))
                            continue;
                        contraction
                            .SetScope(attr.ContractScope)
                            .FromFactory((IFactory)mono);
                        if (monoType.IsDefined(typeof(CustomProxyAttribute)))
                        {
                            var proxyAttrs = monoType.GetCustomAttributes<CustomProxyAttribute>();
                            foreach (var proxyAttr in proxyAttrs)
                                contraction.WrapCustomProxy(proxyAttr.ProxyType);
                        }
                        list.Add(new KeyValuePair<Type, object>(attr.ContractType, attr.Identity));
                        if (attr.ContractScope == EContractScope.Singleton)
                        {
                            singletons.Add(contraction.Instantiate());
                        }
                    }
                }

                
            }

            foreach (var singleton in singletons)
            {
                _core.Inject(singleton);
            }
        }

        private void LoadSceneAndSetup2(Scene scene, LoadSceneMode mode)
        {
            var list = new List<Contraction>();
            var monos = scene.GetRootGameObjects()
                .Deverge((go) => go.GetComponentsInChildren<MonoBehaviour>());

            foreach (var mono in monos)
            {
                var monoType = mono.GetType();
                // injectable
                if (monoType.IsDefined(typeof(InjectableAttribute), false))
                {
                    var attrs = monoType.GetCustomAttributes<InjectableAttribute>();
                    foreach (var attr in attrs)
                    {
                        if (!_core.TryAdd(attr.ContractType, out Contraction contraction, attr.Identity))
                            continue;
                        contraction
                            .SetScope(attr.ContractScope)
                            .Concrete(monoType, mono);
                        if (monoType.IsDefined(typeof(CustomProxyAttribute)))
                        {
                            var proxyAttrs = monoType.GetCustomAttributes<CustomProxyAttribute>();
                            foreach (var proxyAttr in proxyAttrs)
                                contraction.WrapCustomProxy(proxyAttr.ProxyType);
                        }
                        list.Add(contraction);
                    }
                }

                if (monoType.IsDefined(typeof(InjectableFactoryAttribute), true))
                {
                    var attrs = monoType.GetCustomAttributes<InjectableFactoryAttribute>();
                    foreach (var attr in attrs)
                    {
                        if (!typeof(IFactory).IsAssignableFrom(monoType))
                        {
                            throw new InvalidCastException($"The {monoType.Name} is not implemented factory.");
                        }

                        if (!_core.TryAdd(attr.ContractType, out Contraction contraction, attr.Identity))
                            continue;
                        contraction
                            .SetScope(attr.ContractScope)
                            .FromFactory((IFactory)mono);
                        if (monoType.IsDefined(typeof(CustomProxyAttribute)))
                        {
                            var proxyAttrs = monoType.GetCustomAttributes<CustomProxyAttribute>();
                            foreach (var proxyAttr in proxyAttrs)
                                contraction.WrapCustomProxy(proxyAttr.ProxyType);
                        }
                        list.Add(contraction);
                    }
                }
            }

            _core.Initialize(list);
        }

        private void UnloadSceneAndSetup(Scene scene)
        {
            if(!_injectableList.TryGetValue(scene, out List<KeyValuePair<Type, object>> list))
            {
                return;
            }
            foreach (var injectable in list)
            {
                _core.Remove(injectable.Key, injectable.Value);
            }
            _injectableList.Remove(scene);
        }

        protected virtual void CustomSetup(IGameCore core)
        {
        }

        protected virtual IFsmState<IGameCore> DefaultState()
        {
            return null;
        }
    }

    

    /// <summary>
    /// Attribute of injectable monobehaviour
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
    public class InjectableAttribute : Attribute
    {
        public Type ContractType { get; }
        public object Identity { get; }
        public EContractScope ContractScope { get; }

        public InjectableAttribute(Type contractType, object identity = null)
        {
            ContractType = contractType;
            Identity = identity;
            ContractScope = EContractScope.Singleton;
        }

        public InjectableAttribute(Type contractType, EContractScope scope, object identity = null)
        {
            ContractType = contractType;
            Identity = identity;
            ContractScope = scope;
        }
    }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
    public class InjectableFactoryAttribute : Attribute
    {
        public Type ContractType { get; }
        public object Identity { get; }
        public EContractScope ContractScope { get; }

        public InjectableFactoryAttribute(Type contractType, object identity = null)
        {
            ContractType = contractType;
            Identity = identity;
            ContractScope = EContractScope.Singleton;
        }

        public InjectableFactoryAttribute(Type contractType, EContractScope scope, object identity = null)
        {
            ContractType = contractType;
            Identity = identity;
            ContractScope = scope;
        }
    }

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class InjectMethodAttribute : Attribute
    {

    }
}