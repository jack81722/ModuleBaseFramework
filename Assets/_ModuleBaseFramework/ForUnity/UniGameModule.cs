using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ModuleBased.ForUnity
{   
    public class UniGameModule : MonoBehaviour, IGameModule
    {
        #region -- IGameModule methods --
        public ILogger Logger { get; set; }
        public IGameModuleCollection Modules { get; set; }

        public void InitializeModule()
        {   
            OnInitializingModule();
        }

        public void StartModule()
        {
            OnStartingModule();
        }

        public virtual void OnRemoved() { }

        #endregion

        #region -- Protected virtual methods --
        protected virtual void OnInitializingModule() { }

        protected virtual void OnStartingModule() { }
        #endregion
    }
}
