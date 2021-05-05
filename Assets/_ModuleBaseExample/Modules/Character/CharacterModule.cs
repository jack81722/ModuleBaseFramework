using ModuleBased.FungusPlugin;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ModuleBased.Example {
    public class CharacterModule : MonoBehaviour, IGameModule {

        [SerializeField]
        private GameObject _a;
        [SerializeField]
        private GameObject _b;

        #region -- IGameModule --
        public void OnModuleInitialize() {

        }

        public void OnModuleStart() {

        }
        #endregion

        #region -- Cmds --

        [ModuleCmd]
        public IEnumerator MoveAToDestination() {
            float t = 0;
            Vector3 p = _a.transform.position;
            while (t < 1) {
                float delta = Time.deltaTime;
                t += delta;
                p.x = t;
                _a.transform.position = p;
                yield return null;
            }
            p.x = 1;
            _a.transform.position = p;
        }

        [ModuleCmd]
        public void MoveBToDestination() {

        }
        #endregion

    }
}