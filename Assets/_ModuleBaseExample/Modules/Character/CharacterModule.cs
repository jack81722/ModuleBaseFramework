using ModuleBased.ForUnity;
using ModuleBased.Dialogue;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using ModuleBased.Models;

namespace ModuleBased.Example {
    [UniModule(typeof(ICharacterModule))]
    public class CharacterModule : MonoBehaviour, IGameModule, ICharacterModule {
        [SerializeField]
        private GameObject _a;
        [SerializeField]
        private GameObject _b;

        #region -- IGameModule --
        public IGameModuleCollection Modules { get; set; }
        public ILogger Logger { get; set; }


        public IEnumerator InitializeModule(IProgress<ProgressInfo> progress)
        {   
            progress.Report(1, "Character");
            yield break;
        }

        public void StartModule() {

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

        public void OnRemoved()
        {
            throw new System.NotImplementedException();
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