using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ModuleBased.Example.Dialogue {
    public class WaitFrameCommand : DefaultCommand {
        public int WaitFrame;
        private int _frame;

        public override IEnumerator Execute() {
            while (_frame < WaitFrame) {
                _frame++;
                yield return null;
            }
        }

        public override void OnStart() {
            _frame = 0;
        }
    }
}