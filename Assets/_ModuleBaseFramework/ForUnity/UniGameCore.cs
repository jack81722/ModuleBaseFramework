using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ModuleBased.ForUnity {
    public class UniGameCore : MonoBehaviour, IGameCore {
        /// <summary>
        /// Singleton of game core
        /// </summary>
        private static UniGameCore _singleton;
        public static UniGameCore Singleton {
            get {
                if(_singleton == null) {
                    _singleton = FindObjectOfType<UniGameCore>();
                    if(_singleton == null) {
                        throw new NullReferenceException("UniGameCore must be place in scene.");
                    }
                }
                return _singleton;
            }
        }

        /// <summary>
        /// Internal core
        /// </summary>
        private IGameCore _core;

        private void Awake() {
            // singleton
            if (_singleton == null)
                _singleton = this;
            if(_singleton != this) {
                Destroy(gameObject);
            }
            // initialize
            ILogger logger = new UniLogger();
            _core = new GameCore(logger);
            IGameModule[] modules = GetComponentsInChildren<IGameModule>();
            foreach(var module in modules) {
                _core.AddModule(module);
            }
            _core.InitializeModules();
        }

        private void Start() {
            StartModules();
        }

        #region -- IGameCore --
        public IGameModule AddModule(Type type) {
            IGameModule mod = _core.AddModule(type);
            StoreModule(mod);
            return mod;
        }

        public void AddModule(IGameModule module) {
            _core.AddModule(module);

            StoreModule(module);
        }

        public TMod AddModule<TMod>() where TMod : IGameModule {
            TMod mod = _core.AddModule<TMod>();
            StoreModule(mod);
            return mod;
        }

        public void AddModule<TMod>(TMod mod) where TMod : IGameModule {
            _core.AddModule<TMod>(mod);
            StoreModule(mod);
        }

        public IGameModule GetModule(Type type, bool inherit = false) {
            return _core.GetModule(type, inherit);
        }

        public TMod GetModule<TMod>(bool inherit = false) where TMod : IGameModule {
            return _core.GetModule<TMod>(inherit);
        }

        public bool TryGetModule<TMod>(out TMod mod, bool inherit = false) where TMod : IGameModule {
            return _core.TryGetModule<TMod>(out mod, inherit);
        }

        public bool TryGetModule(Type modType, out IGameModule mod, bool inherit = false) {
            return _core.TryGetModule(modType, out mod, inherit);
        }

        public void StartModules() {
            _core.StartModules();
        }


        public void InitializeModules() {
            _core.InitializeModules();
        }

        #endregion
        /// <summary>
        /// Store the module under the game core
        /// </summary>
        /// <param name="module"></param>
        private void StoreModule(IGameModule module) {
            
            Type modType = module.GetType();
            if (modType.IsSubclassOf(typeof(MonoBehaviour))) {
                ((MonoBehaviour)module).transform.parent = transform;
            }
        }

    }
}