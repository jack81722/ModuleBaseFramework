using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ModuleBased.Example.Dialogue {
    public class WaitSecondCommand : DefaultCommand {
        public float WaitSecond;
        private float _sec;

        public WaitSecondCommand(float sec = 0) {
            WaitSecond = sec;
        }

        public override IEnumerator Execute() {
            while(_sec < WaitSecond) {
                _sec += Time.deltaTime;
                yield return null;
            }
        }

        public override void OnStart() {
            _sec = 0;
        }
    }
}