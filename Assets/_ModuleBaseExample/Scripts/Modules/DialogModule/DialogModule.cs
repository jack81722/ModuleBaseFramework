using ModuleBased.ForUnity;
using ModuleBased.Injection;
using ModuleBased.Proxy;
using ModuleBased.Proxy.AOP;
using ModuleBased.Proxy.AOP.Handlers;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ModuleBased.Example.Dialog
{
    [CustomProxy(typeof(DialogModuleProxy))]
    [Injectable(typeof(IDialogModule))]
    public class DialogModule : MonoBehaviour, IDialogModule, IEventInject, IDisposable
    {
        #region -- Configs -- 
        private const string DIALOG_AUTO_PLAY = "dialog_auto_play";
        private const string DIALOG_AUTO_PLAY_DELAY = "dialog_auto_play_delay";
        private const string DIALOG_PLAY_SPEED = "dialog_play_speed";
        private const string DIALOG_ENABLE_SKIP = "dialog_enable_skip";
        private const string DIALOG_SKIP_SPEED = "dialog_skip_speed";
        #endregion

        #region -- Config Values --
        public bool AutoPlay = true;
        public float AutoPlayDelay = 1.5f;
        public float PlaySpeed = 5f;
        public bool EnableSkip = false;
        public float SkipSpeed = 100f;
        #endregion

        #region -- Required --
        [Inject]
        private IConfigModule _configMod;
        #endregion

        private ChatBox _chatBox;
        private int _chatIndex;
        private List<string> _chatBuff = new List<string>
        {
            "xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx",
            "yyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyy",
            "zzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzz",
            "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa"
        };

        public void OnInject()
        {
            // load current config value
            AutoPlay = _configMod.LoadOrDefault<bool>(DIALOG_AUTO_PLAY, false);
            AutoPlayDelay = _configMod.LoadOrDefault<float>(DIALOG_AUTO_PLAY_DELAY, 1.5f);
            PlaySpeed = _configMod.LoadOrDefault<float>(DIALOG_PLAY_SPEED, 5);
            EnableSkip = _configMod.LoadOrDefault<bool>(DIALOG_ENABLE_SKIP, false);
            SkipSpeed = _configMod.LoadOrDefault<float>(DIALOG_SKIP_SPEED, 100f);

            // subscribe config event
            _configMod.Subcribe<bool>(DIALOG_AUTO_PLAY, listen_AutoPlay);
            _configMod.Subcribe<float>(DIALOG_AUTO_PLAY_DELAY, listen_AutoPlayDelay);
            _configMod.Subcribe<float>(DIALOG_PLAY_SPEED, listen_DialogSpeed);
            _configMod.Subcribe<bool>(DIALOG_ENABLE_SKIP, listen_EnableSkip);
            _configMod.Subcribe<float>(DIALOG_SKIP_SPEED, listen_SkipSpeed);
        }

        #region -- Events --
        private void listen_AutoPlay(bool auto)
        {
            AutoPlay = auto;
        }

        private void listen_AutoPlayDelay(float delay)
        {
            AutoPlayDelay = delay;
        }

        private void listen_DialogSpeed(float speed)
        {
            if (_chatBox != null)
            {
                _chatBox.ModifySpeed(speed);
            }
        }

        private void listen_EnableSkip(bool skip)
        {
            EnableSkip = skip;
            SkipSpeed = _configMod.LoadOrDefault<float>(DIALOG_SKIP_SPEED);
            if (skip)
                _chatBox?.ModifySpeed(_configMod.LoadOrDefault<float>(DIALOG_SKIP_SPEED));
        }

        private void listen_SkipSpeed(float speed)
        {
            SkipSpeed = speed;
            if (EnableSkip)
                _chatBox?.ModifySpeed(_configMod.LoadOrDefault<float>(DIALOG_SKIP_SPEED));
        }
        #endregion

        #region -- IDispose --
        public void Dispose()
        {
            Dispose(false);
        }

        private void Dispose(bool destroyed)
        {
            if (!destroyed)
            {
                Destroy(gameObject);
                return;
            }
            _configMod.Unsubscribe<bool>(DIALOG_AUTO_PLAY, listen_AutoPlay);
            _configMod.Unsubscribe<float>(DIALOG_AUTO_PLAY_DELAY, listen_AutoPlayDelay);
            _configMod.Unsubscribe<float>(DIALOG_PLAY_SPEED, listen_DialogSpeed);
        }

        private void OnDestroy()
        {
            Dispose(true);
        }
        #endregion

        [UniLog(EAOPStatus.Before | EAOPStatus.Error)]
        public void Pause()
        {
            _chatBox.PauseChat();
        }

        [UniLog(EAOPStatus.Before | EAOPStatus.Error)]
        public void Play([Inject] ChatBox box = null)
        {
            if (box == null)
                return;
            _chatBox?.Dispose();
            _chatBox = box;
            PlayNext();
        }

        private void PlayNext()
        {
            if (_chatIndex >= _chatBuff.Count)
            {
                return;
            }
            _chatBox.Display();
            float speed = EnableSkip ? SkipSpeed : PlaySpeed;
            _chatBox.PlayChat(_chatBuff[_chatIndex++], speed);
        }

        [UniLog(EAOPStatus.Before | EAOPStatus.Error)]
        public void Resume()
        {
            if (_chatBox != null && _chatBox.IsPause())
            {
                _chatBox.ContinueChat();
                return;
            }
        }

        public void QuickSkip(bool skip)
        {
            _configMod.Save(DIALOG_ENABLE_SKIP, skip);
        }

        public void Skip()
        {

        }

        [UniLog(EAOPStatus.Before | EAOPStatus.Error)]
        public void Stop()
        {
            if (_chatBox == null)
                return;
            _chatBox.Dispose();
            _chatBox.Hide();
            _chatBox = null;
            _chatIndex = 0;
        }

        #region -- Unity APIs --
        private float _autoPlayTimer = 0f;
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.S))
            {
                QuickSkip(!EnableSkip);
            }

            if (_chatBox != null && _chatBox.IsFinished())
            {
                if (EnableSkip)
                {
                    PlayNext();
                }
                if (AutoPlay)
                {
                    if (_autoPlayTimer < AutoPlayDelay)
                    {
                        _autoPlayTimer += Time.deltaTime;
                    }
                    else
                    {
                        PlayNext();
                        _autoPlayTimer = 0;
                    }
                }
            }
        }
        #endregion
    }

    public class DialogModuleProxy : AOPProxyBase<IDialogModule>, IDialogModule
    {
        public DialogModuleProxy(object real) : base(real)
        {
        }

        public void Pause()
        {
            InvokeProxyMethod();
        }

        public void Play(ChatBox box = null)
        {
            InvokeProxyMethod(box);
        }

        public void Resume()
        {
            InvokeProxyMethod();
        }

        public void Stop()
        {
            InvokeProxyMethod();
        }
    }

    public interface IDialogModule
    {
        void Play(ChatBox box = null);

        void Pause();

        void Resume();

        void Stop();
    }
}