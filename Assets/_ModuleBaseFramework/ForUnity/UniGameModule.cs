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
                yield return e.Current;
            }
            //progress.Report(1, GetType().Name);
            yield return null;
        }

        public void StartModule()
        {
            OnStartingModule();
        }
        #endregion

        #region -- Protected virtual methods --
        /// <summary>
        /// Custom initialize module and report the progress
        /// </summary>
        /// <remarks>
        /// This method will run in coroutine
        /// </remarks>
        protected virtual IEnumerator OnInitializingModule(IProgress<ProgressInfo> progress) 
        {
            yield return null;
        }

        protected virtual void OnStartingModule() { }
        #endregion


    }
}
