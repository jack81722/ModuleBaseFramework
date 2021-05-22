using Game.Math.Eaze;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ModuleBased.Example.DialogueViews {

    public class PortraitTweener : MonoBehaviour {
        public Image UImg_Portrait;

        public List<Sprite> Portraits;

        private IEazeFunc _eazeFunc;

        public void SetPortrait(string name) {
            var portrait = Portraits.Find(p => p.name == name);
            if (portrait != null)
                UImg_Portrait.sprite = portrait;
        }

        public void SetTransparent(float a) {

        }

        public void SetColor(Color color) {

        }
    }
}