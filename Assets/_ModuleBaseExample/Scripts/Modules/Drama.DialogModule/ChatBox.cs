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
    public class ChatBox : UIAgent, IDisposable, IObservable<ChatBox>, IDramaAction, IEventInject
    {
        #region -- Required --
        [Inject]
        private IPool<ChatBox> _pool;
        [Inject]
        private ConsoleView _view;
        [Inject]
        private IConfigModule _config;
        #endregion

        #region -- Configs --
        private const string CONFIG_AUTO_PLAY = "dialog_auto_play";
        private const string CONFIG_AUTO_PLAY_DELAY = "dialog_auto_play_delay";

        private bool AutoPlay = false;
        private float AutoPlayDelay = 1;
        #endregion

        #region -- Config events --
        public void OnInject()
        {
            AutoPlay = _config.Load<bool>(CONFIG_AUTO_PLAY);
            AutoPlayDelay = _config.Load<float>(CONFIG_AUTO_PLAY_DELAY);

            _config.Subscribe<bool>(CONFIG_AUTO_PLAY, Listen_AutoPlay);
            _config.Subscribe<float>(CONFIG_AUTO_PLAY_DELAY, Listen_AutoPlayDelay);
        }

        private void Listen_AutoPlay(bool value)
        {
            AutoPlay = value;
            ResetAutoPlayTimer();
        }

        private void Listen_AutoPlayDelay(float value)
        {
            AutoPlayDelay = value;
            ResetAutoPlayTimer();
        }
        #endregion

        #region -- UI fields --
        private string _name;
        private bool hasName => !string.IsNullOrEmpty(_name);

        private string _targetText;
        [SerializeField]
        private Text _txtName;
        [SerializeField]
        private Image _imgBanner;
        [SerializeField]
        private Text _txtChat;
        #endregion

        #region -- Tween fields --
        [SerializeField]
        private float _speed = 20;
        private Tween _tween;

        private bool _isPause;
        private bool _isCompleted;
        [SerializeField]
        private float _autoPlayTimer;
        #endregion

        #region -- IObservable fields --
        private Subject<ChatBox> _subject = new Subject<ChatBox>();
        #endregion

        #region -- Status --
        public bool IsAlive()
        {
            return IsAlive(_tween);
        }

        public bool IsPlaying()
        {
            return IsAlive(_tween) && _tween.IsPlaying();
        }

        public bool IsPause()
        {
            return IsPause(_tween);
        }

        public bool IsTweenCompleted()
        {
            return IsCompleted(_tween);
        }

        public bool IsCompleted()
        {
            if (AutoPlay)
                return IsAutoPlayTimeup() && IsTweenCompleted();
            return IsTweenCompleted() && _isCompleted;
        }
        #endregion

        private bool IsAutoPlayTimeup()
        {
            return _autoPlayTimer <= 0;
        }

        private void Update()
        {
            if (IsTweenCompleted() && AutoPlay)
            {
                if (IsAutoPlayTimeup())
                {
                    Complete();
                    return;
                }
                if (!_isPause)
                    _autoPlayTimer -= Time.deltaTime * TimeScale;
            }
        }

        private void ResetAutoPlayTimer()
        {
            _autoPlayTimer = AutoPlayDelay;
        }

        #region -- IDisposable --
        public override void Dispose(bool destroyed)
        {
            if (destroyed)
                return;
            Hide();
            _subject.Clear();
            _pool.Push(this);
            killTween();
        }

        private void killTween()
        {
            if (IsAlive())
            {
                _tween.Kill();
            }
        }
        #endregion

        #region -- Hide & display --
        public override void Hide()
        {
            base.Hide();
            _imgBanner.enabled = false;
            _txtName.enabled = false;
            _txtChat.enabled = false;
        }

        public override void Display()
        {
            base.Display();
            _imgBanner.enabled = hasName;
            _txtName.enabled = hasName;
            _txtChat.enabled = true;
        }
        #endregion

        public void Set(string name, string text)
        {
            _txtName.text = _name = name;
            _txtChat.text = "";
            _targetText = text;
        }


        #region -- IDramaAction --
        private float _timeScale;
        public float TimeScale {
            get
            {
                return _timeScale;
            }
            set
            {
                _timeScale = value;
                if (_tween == null)
                    return;
                _tween.timeScale = value;
            }
        }

        public void Play()
        {
            _isCompleted = false;
            ResetAutoPlayTimer();
            Display();
            _tween = _txtChat
                .DOText(_targetText, _targetText.Length / _speed)
                .SetEase(Ease.Linear)
                .SetAutoKill(false);
            _tween.onUpdate += () => OnNext(this);
        }
        public void Resume()
        {
            _isPause = false;
            _tween.Play();
        }

        public void Pause()
        {
            _isPause = true;
            _tween.Pause();
        }

        public void Stop()
        {
            _tween.Kill();
        }

        public void Complete()
        {
            if (IsPlaying())
            {
                _tween.Complete();
                return;
            }
            OnCompleted();
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
            if (_isCompleted)
                return;
            _isCompleted = true;
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