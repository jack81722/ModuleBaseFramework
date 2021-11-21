using DG.Tweening;
using ModuleBased.Injection;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ModuleBased.Example.Dialog
{
    public class ChatBox : MonoBehaviour, IDisposable, IPointerClickHandler
    {
        [Inject]
        private ChatBoxPool _pool;

        private string _targetText;
        private string _currentText {
            get
            {
                if (_txtChat != null)
                    return _txtChat.text;
                return string.Empty;
            }
        }

        [SerializeField]
        private Text _txtChat;
        private Tween _tween;

        public bool IsAlive()
        {
            return _tween != null && _tween.IsActive();
        }

        public bool IsPause()
        {
            return IsAlive() && !_tween.IsPlaying();
        }

        public bool IsFinished()
        {
            return IsAlive() && _tween.IsComplete();
        }

        public void Dispose()
        {
            Hide();
            _pool.Push(this);
            killTween();
        }

        public void PlayChat(string text, float speed)
        {
            _txtChat.text = "";
            _targetText = text;
            _tween = _txtChat
                .DOText(_targetText, _targetText.Length / speed)
                .SetEase(Ease.Linear)
                .SetAutoKill(false);
        }

        public void ModifySpeed(float speed)
        {
            killTween();
            _tween = _txtChat
                .DOText(_targetText, _targetText.Length / speed)
                .SetEase(Ease.Linear)
                .SetAutoKill(false);
        }

        public void ContinueChat()
        {
            _tween.Play();
        }

        public void PauseChat()
        {
            _tween.Pause();
        }

        public void Display()
        {
            gameObject.SetActive(true);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (IsFinished())
            {
                Dispose();
            }
            if (!IsPause())
            {
                _tween.Complete();
            }
        }

        private void killTween()
        {
            if (IsAlive())
            {
                _tween.Kill();
            }
        }
    }
}