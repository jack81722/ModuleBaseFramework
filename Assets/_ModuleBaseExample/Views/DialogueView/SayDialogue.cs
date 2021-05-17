using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ModuleBased.Example.DialogueViews {
    public class SayDialogue : MonoBehaviour, ISayWindow {

        public Image UImg_CharBanner;
        public Text UTxt_CharName;
        /// <summary>
        /// UI text component of saying
        /// </summary>
        public Text UTxt_Say;

        public string CharacterName { get; set; }
        public string SayText { get; set; }

        private ITextTimer _textTimer;

        private Coroutine _sayCoroutine;
        
        private EndSayCallback _onEndSay;

        private bool _isFinished;

        public void BeginSay(string sayText, EndSayCallback endSay = null) {
            gameObject.SetActive(true);
            if (_textTimer == null)
                _textTimer = new TextPerTime();
            else
                _textTimer.Reset();
            SetCharacterName("");
            SayText = sayText;
            _onEndSay = endSay;
            _isFinished = false;
            _sayCoroutine = StartCoroutine(OnSaying());
        }

        public void BeginSay(string charName, string sayText, EndSayCallback endSay = null) {
            gameObject.SetActive(true);
            if (_textTimer == null)
                _textTimer = new TextPerTime();
            else
                _textTimer.Reset();
            SetCharacterName(charName);
            SayText = sayText;
            _onEndSay = endSay;
            _isFinished = false;
            _sayCoroutine = StartCoroutine(OnSaying());
        }

        public void SetCharacterName(string name) {
            if (string.IsNullOrEmpty(name))
                UImg_CharBanner.gameObject.SetActive(false);
            else {
                UImg_CharBanner.gameObject.SetActive(true);
                UTxt_CharName.text = name;
            }
        }

        /// <summary>
        /// Show all text word by word
        /// </summary>
        /// <returns></returns>
        private IEnumerator OnSaying() {
            _textTimer.Update();
            while (_textTimer.Count < SayText.Length) {
                UTxt_Say.text = SayText.Substring(0, _textTimer.Count);
                _textTimer.Update();
                yield return null;
            }
            ShowAllTextImmediatly();
        }

        /// <summary>
        /// Show all text immediately
        /// </summary>
        public void ShowAllTextImmediatly() {
            if (_sayCoroutine != null)
                StopCoroutine(_sayCoroutine);
            UTxt_Say.text = SayText;
            _isFinished = true;
        }

        
        public void EndSaying() {
            if(_onEndSay == null)
                gameObject.SetActive(false);
            else
                _onEndSay?.Invoke();
        }

        public void Listen_OnClick() {
            if (_isFinished)
                EndSaying();
            else
                ShowAllTextImmediatly();
        }
    }


    public interface ITextTimer {
        int Count { get; }

        void Update();

        void Reset();
    }

    public class TextPerTime : ITextTimer {
        public int Count { get => (int)_count; }
        private float _count;
        public float WriteSpeed = 20f;

        public void Update() {
            _count += Time.deltaTime * WriteSpeed;
        }

        public void Reset() {
            _count = 0;
        }
    }
}