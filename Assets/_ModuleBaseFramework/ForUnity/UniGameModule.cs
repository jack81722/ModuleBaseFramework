using ModuleBased.Models;
using System;
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


        public IEnumerator InitializeModule(IProgress<ProgressInfo> progress)
        {   
            var e = OnInitializingModule(progress);
            while (e.MoveNext())
            {
                yield return null;
            }
            progress.Report(1, GetType().Name);
        }

        public void StartModule()
        {
            OnStartingModule();
        }
        #endregion

        #region -- Protected virtual methods --
        protected virtual IEnumerator OnInitializingModule(IProgress<ProgressInfo> progress) 
        {
            yield return null;
        }

        protected virtual void OnStartingModule() { }
        #endregion


    }
}
