using ModuleBased.Dialogue;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ModuleBased.Example.Dialogue {
    public class DefaultCommand : IDialogueCommand {
        public IDialogueBlock Parent { get; set; }

        public virtual IEnumerator Execute() {
            return null;
        }

        public virtual void OnEnd() {
            
        }

        public virtual void OnStart() {
            
        }
    }
}