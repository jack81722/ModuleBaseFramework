using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ModuleBased.Dialogue {
    /// <summary>
    /// Executing commands parallely
    /// </summary>
    public sealed class ParallelDialogueBlock : IDialogueBlock, IDialogueCommand {
        private List<IDialogueCommand> _commands;

        public ParallelDialogueBlock() {
            _commands = new List<IDialogueCommand>();
        }

        #region -- IDialogueCommand --
        public IDialogueBlock Parent { get; set; }

       
        public IEnumerator Execute() {
            return StartExecution();
        }

        public void OnEnd() { }

        public void OnStart() { }
        #endregion

        #region -- IDialogueBlock --
        public void AddCommand(IDialogueCommand cmd) {
            _commands.Add(cmd);
        }

        public IEnumerator StartExecution() {
            IEnumerator[] enumerators = new IEnumerator[_commands.Count];
            int i = 0;
            foreach (var cmd in _commands) {
                cmd.OnStart();
                enumerators[i++] = cmd.Execute();
            }
            bool running = true;
            while (running) {
                running = false;
                foreach (var e in enumerators) {
                    running |= (e != null && e.MoveNext());
                }
                yield return null;
            }

            foreach (var cmd in _commands) {
                cmd.OnEnd();
            }
        }

        public bool Next() {
            return false;
        }
        #endregion
    }
}