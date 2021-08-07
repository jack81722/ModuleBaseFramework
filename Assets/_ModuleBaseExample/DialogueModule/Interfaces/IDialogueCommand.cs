using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ModuleBased.Dialogue {
    public interface IDialogueCommand {
        IDialogueBlock Parent { get; set; }

        /// <summary>
        /// Start command
        /// </summary>
        void OnStart();

        /// <summary>
        /// Execute every frame until command finished.
        /// </summary>
        IEnumerator Execute();

        /// <summary>
        /// End command
        /// </summary>
        void OnEnd();
    }
}