using ModuleBased.ForUnity;
using ModuleBased.Dialogue;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ModuleBased.Example {
    [ModuleItf(typeof(ICharacterModule))]
    public class CharacterModule : MonoBehaviour, IGameModule, ICharacterModule {

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
        public IEnumerator MoveAToDestination(float x, float time = 1) {
            float t = 0;
            Vector3 p = _a.transform.position;
            float from = p.x;
            while (t < time) {
                float delta = Time.deltaTime;
                t += delta;
                float r = Mathf.Clamp01(t / time);
                p.x = x * t + (1 - t) * from;
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

    public interface ICharacterModule {
        [ModuleCmd]
        IEnumerator MoveAToDestination(float x, float time = 1);

        [ModuleCmd]
        void MoveBToDestination();
    }
}