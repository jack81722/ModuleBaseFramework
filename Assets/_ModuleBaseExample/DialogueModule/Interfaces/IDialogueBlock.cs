using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ModuleBased.Dialogue {
    public interface IDialogueBlock {
        IEnumerator StartExecution();

        bool Next();

        void AddCommand(IDialogueCommand cmd);
    }
}