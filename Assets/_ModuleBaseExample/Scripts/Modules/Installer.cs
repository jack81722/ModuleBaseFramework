using ModuleBased.ForUnity;
using ModuleBased.Injection;
using ModuleBased.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ModuleBased.Example
{
    public sealed class Installer : MonoBehaviour
    {
        private IGameCore core;
        private List<KeyValuePair<Type, object>> _injectableList;

        private void OnEnable()
        {
            _injectableList = new List<KeyValuePair<Type, object>>();
            core = FindObjectOfType<Core>().Core;
            var scene = gameObject.scene;
            scene.GetRootGameObjects()
                .Deverge((go) => go.GetComponentsInChildren<MonoBehaviour>())
                .Divert(
                    (go) => go.GetType().IsDefined(typeof(InjectableAttribute), false), out IEnumerable<MonoBehaviour> injectables,
                    (go) => go.GetType().IsDefined(typeof(InjectableFactoryAttribute), false), out IEnumerable<MonoBehaviour> factories);
            var singletons = new List<object>();
            foreach (var pair in injectables.WithAttr<MonoBehaviour, InjectableAttribute>(false))
            {
                Type contractType = pair.Value.ContractType;
                object identity = pair.Value.Identity;
                EContractScope scope = pair.Value.ContractScope;
                var contraction = core
                    .Add(contractType, identity)
                    .SetScope(scope)
                    .From(pair.Key);
                _injectableList.Add(new KeyValuePair<Type, object>(contractType, identity));
                if (scope == EContractScope.Singleton)
                {
                    singletons.Add(contraction.Instantiate());
                }
            }
            foreach (var pair in factories.WithAttr<MonoBehaviour, InjectableFactoryAttribute>(false))
            {
                Type contractType = pair.Value.ContractType;
                object identity = pair.Value.Identity;
                EContractScope scope = pair.Value.ContractScope;
                var contraction = core
                    .Add(contractType, identity)
                    .SetScope(scope)
                    .FromFactory((IFactory)pair.Key);
                _injectableList.Add(new KeyValuePair<Type, object>(contractType, identity));
                if (scope == EContractScope.Singleton)
                {
                    singletons.Add(contraction.Instantiate());
                }
            }
            foreach (var singleton in singletons)
            {
                core.Inject(singleton);
            }
        }

        private void OnDisable()
        {   
            foreach (var injectable in _injectableList)
            {
                core.Remove(injectable.Key, injectable.Value);
            }
        }
    }
}
