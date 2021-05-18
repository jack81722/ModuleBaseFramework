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

        //public void StartExecution() {
        //    if (_commands.Count > 0) {
        //        _cmdIndex = 0;
        //        Current.OnStart();
        //    }
        //}

        public IEnumerator StartExecution() {
            _cmdIndex = 0;
            IEnumerator curEnumerator;
            while (_cmdIndex < _commands.Count) {
                Current.OnStart();
                curEnumerator = Current.Execute();
                while (curEnumerator != null && curEnumerator.MoveNext()) {
                    yield return null;
                }
                Current.OnEnd();
                _cmdIndex++;
            }
        }

        public void AddCommand(IDialogueCommand cmd) {
            if (cmd == null)
                return;
            cmd.Parent = this;
            _commands.Add(cmd);
        }

        public bool Next() {
            bool result = _cmdIndex + 1 < _commands.Count;
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