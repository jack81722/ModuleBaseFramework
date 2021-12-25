using Cheap2.Plugin.Pool;
using DG.Tweening;
using ModuleBased.Injection;
using ModuleBased.Rx;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ModuleBased.Example.Drama.Dialog
{
    public class ChatBox : UIAgent, IDisposable, IPointerClickHandler, IObservable<ChatBox>, IDramaAction
    {
        #region -- Required --
        [Inject]
        private IPool<ChatBox> _pool;
        [Inject]
        private ConsoleView _view;
        #endregion

        private string _name;
        private string _targetText;
        private float _speed = 1;

        [SerializeField]
        private Text _txtChat;
        private Tween _tween;
        private Subject<ChatBox> _subject = new Subject<ChatBox>();

        public bool IsAlive()
        {
            return IsAlive(_tween);
        }

        public bool IsPause()
        {
            return IsPause(_tween);
        }

        public bool IsFinished()
        {
            return IsFinished(_tween);
        }

        public override void Dispose(bool destroyed)
        {
            if (destroyed)
                return;
            Hide();
            _subject.Clear();
            _pool.Push(this);
            killTween();
        }

        public override void Hide()
        {
            base.Hide();
            _txtChat.enabled = false;
        }

        public override void Display()
        {
            base.Display();
            _txtChat.enabled = true;
        }

        public void Set(string name, string text)
        {
            _name = name;
            _txtChat.text = "";
            _targetText = text;
        }

        private void killTween()
        {
            if (IsAlive())
            {
                _tween.Kill();
            }
        }

        #region -- IDialogAction --
        public void Play()
        {
            Display();
            _tween = _txtChat
                .DOText(_targetText, _targetText.Length / _speed)
                .SetEase(Ease.Linear)
                .SetAutoKill(false);
            _tween.onUpdate += () => OnNext(this);
        }
        public void Resume()
        {
            _tween.Play();
        }

        public void Pause()
        {
            _tween.Pause();
        }


        public void Stop()
        {
            _tween.Kill();
        }
        #endregion

        #region -- IPointerClickHandler --
        public void OnPointerClick(PointerEventData eventData)
        {
            if (IsFinished())
            {
                OnCompleted();
                Dispose();
            }
            if (!IsPause())
            {
                _tween.Complete();
            }
        }
        #endregion

        

        #region -- IObservable --
        public IDisposable Subscribe(IObserver<ChatBox> observer)
        {
            return _subject.Subscribe(observer);
        }

        private void OnNext(ChatBox box)
        {
            _subject.OnNext(box);
        }

        private void OnCompleted()
        {
            _subject.OnCompleted();
        }

        private void OnError(Exception e)
        {
            _subject.OnError(e);
            _view?.LogError(e);
        }

        public IDisposable Subscribe(IObserver<object> observer)
        {
            return Subscribe(new GenericObserver<ChatBox>(observer));
        }
        #endregion
    }
}