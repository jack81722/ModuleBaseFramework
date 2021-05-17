using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ModuleBased.Dialogue {
    public interface IDialogueCommand {
        IDialogueBlock Parent { get; set; }
        void OnStart();

        IEnumerator Execute();

        void OnEnd();
    }
}