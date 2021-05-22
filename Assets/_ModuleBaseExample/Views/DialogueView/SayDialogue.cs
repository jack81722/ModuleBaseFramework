using Game.Math.Eaze;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ModuleBased.Example.DialogueViews {
    public class SayDialogue : MonoBehaviour, ISayWindow {
        private static SayDialogue _singleton;
        public static SayDialogue Singleton {
            get {
                if (_singleton == null) {
                    _singleton = FindObjectOfType<SayDialogue>();
                    if (_singleton == null) {
                        var prefab = Resources.Load<SayDialogue>("Prefabs/SayDialogue");
                        _singleton = Instantiate(prefab);
                        _singleton.gameObject.SetActive(false);
                    }
                }
                return _singleton;
            }
        }

        public Image UImg_CharBanner;
        public Text UTxt_CharName;
        /// <summary>
        /// UI text component of saying
        /// </summary>
        public Text UTxt_Say;

        public MaskableGraphic[] UIElements;
        private Color[] _uiColors;

        
        public string CharacterName { get; set; }
        public string SayText { get; set; }

        private ITextTimer _textTimer;

        private Coroutine _sayCoroutine;

        public bool FadeOutOnEnd;
        public EEazeType FadeEazeType;
        public float FadeDuration;
        private IEazeFunc _eazeFunc;
        private Coroutine _fadeCoroutine;

        private EndSayCallback _onEndSay;

        private bool _isFinished;

        private void Awake() {
            if (_singleton != null && _singleton != this) {
                Destroy(gameObject);
            }
            else {
                _singleton = this;
            }
            _uiColors = UIElements.Select(ui => ui.color).ToArray();
        }

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
            StartSayCoroutine();
            StopFadeCoroutine();
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
            StartSayCoroutine();
            StopFadeCoroutine();
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
            _onEndSay?.Invoke();
        }


        public void EndSaying() {
            _onEndSay?.Invoke();
        }

        public void Listen_OnClick() {
            if (_isFinished)
                EndSaying();
            else
                ShowAllTextImmediatly();
        }

        public void Close() {
            if (FadeOutOnEnd)
                FadeOut();
            else
                gameObject.SetActive(false);
        }

        /// <summary>
        /// Set dialogue element transparent (0~1)
        /// </summary>
        public void SetTransparent(float f) {
            for (int i = 0; i < UIElements.Length; i++) {
                Color color = UIElements[i].color;
                color.a = _uiColors[i].a * f;
                UIElements[i].color = color;
            }
        }

        #region -- Coroutine methods --
        private void StartSayCoroutine() {
            StopSayCoroutine();
            _sayCoroutine = StartCoroutine(OnSaying());
        }

        private void StopSayCoroutine() {
            if (_sayCoroutine != null)
                StopCoroutine(_sayCoroutine);
        }

        private void StopFadeCoroutine() {
            if (_fadeCoroutine != null)
                StopCoroutine(_fadeCoroutine);
            SetTransparent(1);
        }

        #endregion

        #region -- Fade out methods --
        private IEnumerator FadingOut(float duration) {
            float timer = 0;
            while (timer < duration) {
                float dt = Time.deltaTime;
                SetTransparent(1 - _eazeFunc.Eaze(timer / duration));
                timer += dt;
                yield return null;
            }
            SetTransparent(0);
            gameObject.SetActive(false);
        }

        public void FadeOut() {
            Debug.Log("Fade out ui");
            _eazeFunc = EazeFunc.GetEazeFunc(FadeEazeType);
            _fadeCoroutine = StartCoroutine(FadingOut(FadeDuration));
        }
        #endregion
    }

    [Serializable]
    public enum EFadeType {
        Immediate,
        FadeOut,
        FadeIn
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