using ModuleBased.Dialogue;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ModuleBased.Example.Dialogue {
    public interface IDialogueModule {
        void StartBlock(string blockName);


    }
}