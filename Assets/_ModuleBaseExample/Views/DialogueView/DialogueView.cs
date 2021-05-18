using ModuleBased.Dialogue;
using ModuleBased.Example.Dialogue;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ModuleBased.Example.DialogueViews {
    public class DialogueView : MonoBehaviour, IGameView {
        #region -- IGameView methods --
        public ILogger Logger { get; set; }
        public IGameModuleCollection Modules { get; set; }

        [RequireModule]
        private IDialogueModule _dialogueModule;

        public void OnViewInitiailize() {
            //_dialogue.OnStartDialogue += Listen_OnStartDialogue;
        }
        #endregion

        private void Listen_OnStartDialogue(IDialogueBlock block) {
            //block.StartExecution();
        }

        private UniDialogueWindowCollection _windows;


        private void Start() {
            //_windows = new UniDialogueWindowCollection();
            //_windows.AddDialogueWindow<ISayWindow>(SayDialogue.Singleton);

            //StartDialogue();
        }

        private void StartDialogue() {
            var charName = "Coinmouse";
            var sayText = "Hello, I am Coinmouse. Nice to meet you.";
            var sayDialog = _windows.GetDialogueWindow<ISayWindow>();
            sayDialog.BeginSay(charName, sayText, EndDialogue1);
        }

        private void EndDialogue1() {
            var charName = "Julia";
            var sayText = "Hello, I am Julia. Nice to meet you too.";
            var sayDialog = _windows.GetDialogueWindow<ISayWindow>();
            sayDialog.BeginSay(charName, sayText, EndDialogue2);
        }

        private void EndDialogue2() {
            var sayText = "Bye Bye";
            var sayDialog = _windows.GetDialogueWindow<ISayWindow>();
            sayDialog.BeginSay(sayText);
        }
    }

    

    public class UniDialogueWindowCollection : IDialogueWindowCollection {
        private Dictionary<Type, MonoBehaviour> _dialogues = new Dictionary<Type, MonoBehaviour>();

        public void AddDialogueWindow<T>(T instance) where T : class {
            AddDialogueWindow(typeof(T), instance);
        }

        public void AddDialogueWindow(Type itfType, object instance) {
            if (!itfType.IsInterface)
                throw new ArgumentException("Dialogue type must be interface.");
            if (!instance.GetType().IsSubclassOf(typeof(MonoBehaviour)))
                throw new ArgumentException("Dialogue instance must be inherited by monobehaviour.");
            _dialogues.Add(itfType, instance as MonoBehaviour);
        }

        public object GetDialogueWindow(Type itfType) {
            if(_dialogues.TryGetValue(itfType, out MonoBehaviour script)) {
                return script;
            }
            return null;
        }

        public T GetDialogueWindow<T>() where T : class {
            return GetDialogueWindow(typeof(T)) as T;
        }
    }
}