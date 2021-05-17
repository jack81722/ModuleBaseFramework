using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ModuleBased.Dialogue {
    public interface IDialogueBlock {
        void StartExecution();

        void Next();

        void AddCommand(IDialogueCommand cmd);
    }
}