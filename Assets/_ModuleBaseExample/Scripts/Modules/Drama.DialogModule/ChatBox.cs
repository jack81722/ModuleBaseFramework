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
        private string _currentText {
            get
            {
                if (_txtChat != null)
                    return _txtChat.text;
                return string.Empty;
            }
        }
        private float _speed = 1;

        [SerializeField]
        private Text _txtChat;
        private Tween _tween;

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

        public void Set(string name, string text, float speed)
        {
            _name = name;
            _txtChat.text = "";
            _targetText = text;
            _speed = speed;
        }

        private void killTween()
        {
            if (IsAlive())
            {
                _tween.Kill();
            }
        }

        #region -- IDialogAction --
        public void ModifySpeed(float speed)
        {
            killTween();
            _tween = _txtChat
                .DOText(_targetText, _targetText.Length / speed)
                .SetEase(Ease.Linear)
                .SetAutoKill(false);
        }

        public void Play()
        {
            Display();
            _tween = _txtChat
                .DOText(_targetText, _targetText.Length / _speed)
                .SetEase(Ease.Linear)
                .SetAutoKill(false);
            _tween.onComplete += OnCompleted;
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
        #endregion

        #region -- IPointerClickHandler --
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
        #endregion

        

        #region -- IObservable --
        private List<IObserver<ChatBox>> _observers = new List<IObserver<ChatBox>>();

        public IDisposable Subscribe(IObserver<ChatBox> observer)
        {
            var disposable = new SingleAssignmentDisposable();
            if (observer != null)
                return disposable;
            _observers.Add(observer);
            return disposable;
        }

        private void OnNext(ChatBox box)
        {
            _observers.ForEach((o) =>
            {
                try
                {
                    o.OnNext(box);
                }
                catch (Exception e)
                {
                    OnError(e);
                }
            });
        }

        private void OnCompleted()
        {
            _observers.ForEach((o) =>
            {
                try
                {
                    o.OnCompleted();
                }
                catch (Exception e)
                {
                    OnError(e);
                }
            });
        }

        private void OnError(Exception e)
        {
            _view?.LogError(e);
        }

        public IDisposable Subscribe(IObserver<object> observer)
        {
            return Subscribe(new GenericObserver<ChatBox>(observer));
        }


        #endregion
    }
}