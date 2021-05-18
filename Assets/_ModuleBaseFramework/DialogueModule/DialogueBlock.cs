using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ModuleBased.Dialogue {
    /// <summary>
    /// Block will link all of command orderly
    /// </summary>
    public class DialogueBlock : IDialogueBlock, IDialogueCommand {
        private List<IDialogueCommand> _commands;
        private int _cmdIndex;

        public IDialogueCommand Current => _commands[_cmdIndex];
        private IEnumerator _curEnumerator;

        public IDialogueBlock Parent { get; set; }

        public DialogueBlock() {
            _commands = new List<IDialogueCommand>();
        }

        public void StartExecution() {
            if (_commands.Count > 0) {
                _cmdIndex = 0;
                Current.OnStart();
            }
        }

        public void AddCommand(IDialogueCommand cmd) {
            if (cmd == null)
                return;
            cmd.Parent = this;
            _commands.Add(cmd);
        }

        public bool Next() {
            _cmdIndex++;
            bool result = _cmdIndex < _commands.Count;
            if (result)
                Current.OnStart();
            return result;
        }

        public void OnStart() {
            //Current.OnStart();
            _curEnumerator = Current.Execute();
        }

        public IEnumerator Execute() {
            return _curEnumerator;
        }

        public void OnEnd() {
            _curEnumerator = null;
        }
    }
}