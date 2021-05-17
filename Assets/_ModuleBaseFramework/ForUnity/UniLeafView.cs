using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ModuleBased.ForUnity {
    public class UniLeafView : MonoBehaviour, IGameView {
        public ILogger Logger { get; set; }
        public IGameModuleCollection Modules { get; set; }

        public virtual void OnViewInitiailize() { }
    }
}