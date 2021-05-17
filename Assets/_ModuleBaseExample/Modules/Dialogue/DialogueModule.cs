using ModuleBased.Dialogue;
using ModuleBased.ForUnity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ModuleBased.Example.Dialogue {
    public class DialogueModule : MonoBehaviour, IGameModule, IDialogueModule {

        #region -- Test dialogue text --
        private string[] texts = new string[] {
            @"Hello, I am Coinmouse. Nice to meet you.",
            @"Hi, My name is Julia. Nice to meet you too.",
            @"A car drive cross ... ",
        };

        private string[] names = new string[] {
            "Coinmouse",
            "Julia",
            ""
        };
        #endregion

        public DialogueStartHandler OnStartDialogue { get; set; }

        public void OnModuleInitialize() {
            
        }

        public void OnModuleStart() {
            DialogueBlock block = new DialogueBlock();
            string name, sayText;
            for(int i = 0; i < texts.Length; i++) {
                name = names[i];
                sayText = texts[i];
                SayCommand command = new SayCommand(name, sayText);
                block.AddCommand(command);
            }
            
        }
    }

    /// <summary>
    /// Block will link all of command orderly
    /// </summary>
    public class DialogueBlock : IDialogueBlock {
        private List<IDialogueCommand> _commands;
        private int _cmdIndex;

        public IDialogueCommand Current => _commands[_cmdIndex];

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

        public void Next() {
            if(_cmdIndex < _commands.Count) {
                _cmdIndex++;
                Current.OnStart();
            }
        }


    }
}