using ModuleBased.Example.Drama.Dialog;
using ModuleBased.Example.Drama.Portrait;
using ModuleBased.ForUnity;
using ModuleBased.Injection;
using ModuleBased.Rx;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ModuleBased.Example.Drama
{
    [Injectable(typeof(IDramaModule))]
    public class DramaModule : MonoBehaviour, IDramaModule
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
        private DialogRepo _repo;
        [Inject]
        private IDialogModule _dialogModule;
        #endregion

        private IDramaState _current;
        private int _currentIndex = -1;
        private List<ChatModel> _scriptRaws = new List<ChatModel>();
        private Dictionary<string, DramaActionHandler> _actions = new Dictionary<string, DramaActionHandler>();


        private void PlayNext()
        {
            // end
            if (_currentIndex >= _scriptRaws.Count - 1)
                return;
            _currentIndex++;
            var model = _scriptRaws[_currentIndex];
            var acts = AnalyzeAction(model);
            _current = new DramaState(acts);
            _current.Play();
        }

        private IEnumerable<IDramaAction> AnalyzeAction(ChatModel model)
        {
            // chat box
            if (!string.IsNullOrEmpty(model.Content))
            {
                yield return _dialogModule.Say(model.Name, model.Content);
            }
            // actions
            if (string.IsNullOrEmpty(model.Padding))
                yield break;
            var lines = model.Padding.Split('\n');
            foreach (var line in lines)
            {
                string actName = line.Substring(0, line.IndexOf('('));
                if (!_actions.TryGetValue(actName, out DramaActionHandler action))
                {
                    continue;
                }
                var argLine = line.Replace(actName, "").Replace("(", "").Replace(")", "");
                var args = argLine.Split(',').Select((a) => a.Trim(' ')).ToArray();
                yield return action.Invoke(args);
            }
        }

        #region -- Unity APIs --
        private void Update()
        {
            if (_current != null && _current.IsCompleted())
            {
                PlayNext();
            }
        }
        #endregion

        #region -- IDramaModule --
        public void Rewind()
        {
            _current = null;
            _currentIndex = -1;
        }

        public void Load(string dramaScript)
        {
            _scriptRaws.Clear();
            var model = _repo.GetByChapterName(dramaScript);
            _scriptRaws.AddRange(model.Chats);
        }

        public void RegisterAction(string name, DramaActionHandler handler)
        {
            if (_actions.ContainsKey(name))
                return;
            _actions.Add(name, handler);
        }
        #endregion

        #region -- IPlayable --

        public void Pause()
        {
            if (_current != null)
                _current.Pause();
        }

        public void Play()
        {
            if (_current == null)
            {
                PlayNext();
                return;
            }
            _current.Play();
        }

        public void Resume()
        {
            if (_current != null)
                _current.Resume();
        }

        public void Stop()
        {
            if (_current != null)
                _current.Stop();
        }

        #endregion

        #region -- Events --
        public void OnInject()
        {
            // load current config value
            AutoPlay = _configMod.LoadOrDefault<bool>(DIALOG_AUTO_PLAY, false);
            AutoPlayDelay = _configMod.LoadOrDefault<float>(DIALOG_AUTO_PLAY_DELAY, 1.5f);
            PlaySpeed = _configMod.LoadOrDefault<float>(DIALOG_PLAY_SPEED, 5);
            EnableSkip = _configMod.LoadOrDefault<bool>(DIALOG_ENABLE_SKIP, false);
            SkipSpeed = _configMod.LoadOrDefault<float>(DIALOG_SKIP_SPEED, 100f);

            // subscribe config event
            _configMod.Subscribe<bool>(DIALOG_AUTO_PLAY, listen_AutoPlay);
            _configMod.Subscribe<float>(DIALOG_AUTO_PLAY_DELAY, listen_AutoPlayDelay);
            _configMod.Subscribe<float>(DIALOG_PLAY_SPEED, listen_DialogSpeed);
            _configMod.Subscribe<bool>(DIALOG_ENABLE_SKIP, listen_EnableSkip);
            _configMod.Subscribe<float>(DIALOG_SKIP_SPEED, listen_SkipSpeed);


        }

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

        }

        private void listen_EnableSkip(bool skip)
        {

        }

        private void listen_SkipSpeed(float speed)
        {
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
            _configMod.Unsubscribe<bool>(DIALOG_ENABLE_SKIP, listen_EnableSkip);
            _configMod.Unsubscribe<float>(DIALOG_SKIP_SPEED, listen_SkipSpeed);
        }

        private void OnDestroy()
        {
            Dispose(true);
        }
        #endregion
    }

    public delegate IDramaAction DramaActionHandler(string[] args);

    public interface IDramaModule : IPlayable
    {
        void Load(string dramaScript);
        void RegisterAction(string name, DramaActionHandler handler);
    }

    public interface IPlayable
    {
        void Play();

        void Pause();

        void Resume();

        void Stop();
    }


    public interface IDramaState : IPlayable
    {
        bool IsCompleted();
    }

    public class DramaState : IDramaState
    {
        List<IDramaAction> _observables;
        bool _isCompleted = false;

        public DramaState(IEnumerable<IDramaAction> actions)
        {
            _observables = new List<IDramaAction>(actions);
            if (_observables.Count < 1)
                _isCompleted = true;
            _observables.WhenAll().OnCompleted(() => _isCompleted = true);
        }

        public bool IsCompleted()
        {
            return _isCompleted;
        }

        public void Pause()
        {
            _observables.ForEach((a) => a?.Pause());
        }

        public void Play()
        {
            _observables.ForEach((a) => a?.Play());
        }

        public void Resume()
        {
            _observables.ForEach((a) => a?.Resume());
        }

        public void Stop()
        {
            _observables.ForEach((a) => a?.Stop());
        }
    }
}
