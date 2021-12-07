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
        private GameCoreFsm _fsm;

        private Dictionary<Scene, List<KeyValuePair<Type, object>>> _injectableList;

        protected void Awake()
        {   
            InitializeCore();
        }

        public void InitializeCore()
        {
            _core = new GameCore();
            _fsm = new GameCoreFsm(_core);
            _injectableList = new Dictionary<Scene, List<KeyValuePair<Type, object>>>();
            Setup();
            _fsm.Start(DefaultState());
            //_core.Launch(DefaultState());
        }

        private void Setup()
        {
            _core.Add<IFsm<IGameCore>>()
                .AsSingleton()
                .Concrete(_fsm);
            CustomSetup(_core);

            // search all injectable in children
            SceneManager.sceneLoaded += LoadSceneAndSetup;
            SceneManager.sceneUnloaded += UnloadSceneAndSetup;
        }

        private void LoadSceneAndSetup(Scene scene, LoadSceneMode mode)
        {
            Debug.Log($"Load scene : {scene.name}");
            var list = new List<Contraction>();
            var monos = scene.GetRootGameObjects()
                .Deverge((go) => go.GetComponentsInChildren<MonoBehaviour>());

            List<object> injectTargets = new List<object>();
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

                if (monoType.IsDefined(typeof(InjectAttribute), true))
                {
                    injectTargets.Add(mono);
                }
            }

            _core.Initialize(list);
            foreach(var target in injectTargets)
            {
                _core.Inject(target);
            }
            _core.InvokeLoaded(list);
        }

        private void UnloadSceneAndSetup(Scene scene)
        {
            if (!_injectableList.TryGetValue(scene, out List<KeyValuePair<Type, object>> list))
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