using ModuleBased.Dialogue;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ModuleBased.Example.Dialogue {
    public interface IDialogueModule {
        DialogueStartHandler OnStartDialogue { get; set; }
    }

    public delegate void DialogueStartHandler(IDialogueCommand cmd);
}