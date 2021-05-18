using ModuleBased.Dialogue;
using ModuleBased.Example.DialogueViews;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ModuleBased.Example.Dialogue {
    public class SayCommand : DefaultCommand {
        public string CharName { get; }
        private string _sayText;
        private bool _isFinished;

        private ITextTimer _textTimer;

        public string CurrentSayText => _sayText.Substring(0, _textTimer.Count);

        public SayCommand(string charName, string sayText) {
            CharName = charName;
            _sayText = sayText;
            _textTimer = new TextPerFrame();
        }

        public override IEnumerator Execute() {
            // wait say dialogue finished
            while (!_isFinished) {
                yield return null;
            }
        }

        public override void OnEnd() {
            if (_isFinished) {
                // close say dialogue if no next command
                if (!Parent.Next())
                    SayDialogue.Singleton.Close();
            } else {
                _isFinished = true;
            }
        }

        public override void OnStart() {
            _isFinished = false;
            SayDialogue.Singleton.BeginSay(CharName, _sayText, OnEnd);
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