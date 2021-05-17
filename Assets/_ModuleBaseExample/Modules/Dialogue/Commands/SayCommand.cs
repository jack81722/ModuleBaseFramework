using ModuleBased.Dialogue;
using ModuleBased.Example.DialogueViews;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ModuleBased.Example.Dialogue {
    public class SayCommand : IDialogueCommand {
        public string CharName { get; }
        private string _sayText;

        private ITextTimer _textTimer;

        public string CurrentSayText => _sayText.Substring(0, _textTimer.Count);

        public SayCommand(string charName, string sayText) {
            CharName = charName;
            _sayText = sayText;
            _textTimer = new TextPerFrame();
        }

        public IDialogueBlock Parent { get; set; }

        public IEnumerator Execute() {
            _textTimer.Update();
            while (_textTimer.Count < _sayText.Length) {
                _textTimer.Update();
                yield return null;
            }
        }

        public void OnEnd() {
            Parent.Next();
        }

        public void OnStart() {
            
        }
    }

    public class TextPerFrame : ITextTimer {
        public int Count { get => (int)_count; }
        private float _count;
        public float CountPerFrame = 0.5f;

        public void Update() {
            _count += CountPerFrame;
        }

        public void Reset() {
            _count = 0;
        }
    }
}