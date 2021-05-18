using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
            return StartExecution();
        }

        public bool Next() {
            return false;
        }

        public void OnEnd() {
            //Parent.Next();
        }

        public void OnStart() {
            //StartExecution();
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
                    running |= e.MoveNext();
                }
                yield return null;
            }

            foreach (var cmd in _commands) {
                cmd.OnEnd();
            }
        }
    }
}