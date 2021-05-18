using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ModuleBased.Dialogue {
    public class ParallelDialogueBlock : IDialogueBlock, IDialogueCommand {
        private List<IDialogueCommand> _commands;

        public ParallelDialogueBlock() {
            _commands = new List<IDialogueCommand>();
        }

        public IDialogueBlock Parent { get; set; }

        public void AddCommand(IDialogueCommand cmd) {
            _commands.Add(cmd);
        }

        public IEnumerator Execute() {
            return null;
        }

        public bool Next() {
            return false;
        }

        public void OnEnd() {
            Parent.Next();
        }

        public void OnStart() {
            StartExecution();
        }

        public void StartExecution() {
            foreach(var cmd in _commands) {
                cmd.OnStart();
            }
        }
    }
}