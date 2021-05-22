using Game.Math.Eaze;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ModuleBased.Example.Dialogue {
    public class PortraitFadeInCommand : DefaultCommand {
        // how to get portrait
        public EEazeType FadeInEaze;
        private IEazeFunc _eazeFunc;

        public override void OnStart() {
            
        }
    }
}