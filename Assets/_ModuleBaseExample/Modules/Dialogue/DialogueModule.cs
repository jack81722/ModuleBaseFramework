using ModuleBased.Dialogue;
using ModuleBased.ForUnity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ModuleBased.Example.Dialogue {
    [ModuleItf(typeof(IDialogueModule))]
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
            for (int i = 0; i < texts.Length; i++) {
                name = names[i];
                sayText = texts[i];
                SayCommand command = new SayCommand(name, sayText);
                block.AddCommand(command);
            }
            block.StartExecution();
        }
    }

    
}