using Cheap2.Plugin.Pool;
using ModuleBased.Example.Drama.Portrait;
using ModuleBased.ForUnity;
using ModuleBased.Injection;
using ModuleBased.Proxy;
using ModuleBased.Proxy.AOP;
using ModuleBased.Proxy.AOP.Handlers;
using ModuleBased.Rx;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace ModuleBased.Example.Drama.Dialog
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
        [Inject]
        private IPortraitModule _portraitModule;


        // BackgroundModule
        [Inject]
        private IPool<ChatBox> _chatBoxPool;
        [Inject]
        private DialogRepo _repo;
        #endregion

        private IDramaAction _action;
        private int _actIndex;
        private List<DialogInfo> _dialogBuff = new List<DialogInfo>();

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
            if (_action != null)
            {
                _action.ModifySpeed(speed);
            }
        }

        private void listen_EnableSkip(bool skip)
        {
            EnableSkip = skip;
            SkipSpeed = _configMod.LoadOrDefault<float>(DIALOG_SKIP_SPEED);
            if (skip)
                _action?.ModifySpeed(_configMod.LoadOrDefault<float>(DIALOG_SKIP_SPEED));
        }

        private void listen_SkipSpeed(float speed)
        {
            SkipSpeed = speed;
            if (EnableSkip)
                _action?.ModifySpeed(_configMod.LoadOrDefault<float>(DIALOG_SKIP_SPEED));
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

        #region -- IDialogModule
        [UniLog(EAOPStatus.Before | EAOPStatus.Error)]
        public void Pause()
        {
            _action.Pause();
        }

        [UniLog(EAOPStatus.Before)]
        public void Load(string chapterName)
        {
            DialogModel model = _repo.GetByChapterName(chapterName);
            _dialogBuff = model.Chats.Select(c => new DialogInfo { Name = c.Name, Content = c.Content }).ToList();
        }

        [UniLog(EAOPStatus.Before | EAOPStatus.Error)]
        public void Play()
        {
            _action?.Dispose();
            PlayNext();
        }

        [UniLog(EAOPStatus.Before | EAOPStatus.Error)]
        public void Resume()
        {
            if (_action != null && _action.IsPause())
            {
                _action.Resume();
                return;
            }
        }

        [UniLog(EAOPStatus.Before | EAOPStatus.Error)]
        public void Stop()
        {
            if (_action == null)
                return;
            _action.Dispose();
            _action = null;
            _actIndex = 0;
        }
        #endregion

        Dictionary<string, Func<string[], IDramaAction>> _actions = new Dictionary<string, Func<string[], IDramaAction>>();
        public void RegisterAction(string name, Func<string[], IDramaAction> action)
        {
            _actions.Add(name, action);
        }

        private void PlayNext()
        {
            if (_actIndex >= _dialogBuff.Count)
                return;
            if (_action != null)
                _action.Dispose();
            var dialog = _dialogBuff[_actIndex++];
            _action = ExecuteDialog(dialog);
            _action.Play();
        }

        private IDramaAction ExecuteDialog(DialogInfo info)
        {
            float speed = EnableSkip ? SkipSpeed : PlaySpeed;
            if (info.Name == "Action")
            {
                List<IDramaAction> actions = new List<IDramaAction>();
                var lines = info.Content.Split('\n');
                foreach (var line in lines)
                {
                    string actName = line.Substring(0, line.IndexOf('('));
                    if (_actions.TryGetValue(actName, out Func<string[], IDramaAction> action))
                    {
                        var argLine = line.Replace(actName, "").Replace("(", "").Replace(")", "");
                        var args = argLine.Split(',').Select((a) => a.Trim(' ')).ToArray();
                        actions.Add(action.Invoke(args));
                    }
                }
                return new ParallelDramaAction(actions.ToArray());
            }
            // default case
            var chatBox = _chatBoxPool.Pop();
            chatBox.Set(info.Name, info.Content, speed);
            return chatBox;
        }

        public void QuickSkip(bool skip)
        {
            _configMod.Save(DIALOG_ENABLE_SKIP, skip);
        }

        public void Skip()
        {

        }

        #region -- Unity APIs --
        private float _autoPlayTimer = 0f;
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.S))
            {
                QuickSkip(!EnableSkip);
            }

            if (_action != null && _action.IsFinished())
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

    public class DialogInfo
    {
        public string Name;
        public string Content;

        public override string ToString()
        {
            return $"(Name:{Name}, Content:{Content})";
        }
    }
}