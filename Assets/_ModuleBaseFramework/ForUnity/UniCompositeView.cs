using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ModuleBased.ForUnity {
    public class UniCompositeView : MonoBehaviour, IGameView {
        public ILogger Logger { get; set; }
        public IGameModuleCollection Modules { get; set; }

        protected HashSet<UniLeafView> Leaves;

        public void OnViewInitiailize() {
            Leaves = new HashSet<UniLeafView>();
        }

        public void AddLeafView(UniLeafView view) {
            if (Leaves.Add(view)) {
                view.Logger = Logger;
                view.Modules = Modules;
                OnLeafViewAdded(view);
            }
        }

        public void RemoveLeafView(UniLeafView view) {
            if (Leaves.Remove(view))
                OnLeafViewRemoved(view);
        }

        #region -- Leaf view event methods --
        public virtual void OnLeafViewAdded(UniLeafView view) {

        }

        public virtual void OnLeafViewRemoved(UniLeafView view) {

        }
        #endregion
    }
}