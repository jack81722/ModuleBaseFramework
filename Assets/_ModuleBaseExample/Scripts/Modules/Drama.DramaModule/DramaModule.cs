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
    public class DramaModule : MonoBehaviour, IDramaModule, IEventInject
    {
        #region -- Configs -- 
        private const string DIALOG_PLAY_SPEED = "dialog_play_speed";
        private const string DIALOG_FASTFORWARD_SPEED = "dialog_fastforward_speed";
        #endregion

        #region -- Config Values --
        public float PlaySpeed = 1f;
        public float FastForwardSpeed = 100f;
        #endregion

        #region -- Required --
        [Inject]
        private IConfigModule _configMod;
        [Inject]
        private DialogRepo _repo;
        [Inject]
        private IDialogModule _dialogModule;
        #endregion

        [SerializeField]
        private bool _isPause;
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
            _current?.Dispose();
            _current = new DramaState(acts);
            _current.SetTimeScale(PlaySpeed);
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
            if (Input.GetKeyDown(KeyCode.P))
            {
                if (_isPause)
                    Resume();
                else
                    Pause();
            }
            if (_current != null)
            {
                if (_current.IsCompleted())
                {
                    PlayNext();
                }
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


        public void CompleteCurrentState()
        {
            if (_current != null)
                _current.Complete();
        }

        public void CompleteOrNextState()
        {
            if (_current == null)
                return;
            if (_current.IsCompleted())
            {
                PlayNext();
                return;
            }
            CompleteCurrentState();
        }

        public void BeginFastForward()
        {   
            PlaySpeed = FastForwardSpeed;
            if (_current == null)
                return;
            _current.SetTimeScale(PlaySpeed);
        }

        public void EndFastForward()
        {
            PlaySpeed = 1;
            if (_current == null)
                return;
            _current.SetTimeScale(PlaySpeed);
        }
        #endregion

        #region -- IPlayable --

        public void Pause()
        {
            _isPause = true;
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
            _isPause = false;
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
            FastForwardSpeed = _configMod.LoadOrDefault<float>(DIALOG_FASTFORWARD_SPEED, 3f);
            PlaySpeed = _configMod.LoadOrDefault<float>(DIALOG_PLAY_SPEED, 1f);

            // subscribe config event
            _configMod.Subscribe<float>(DIALOG_FASTFORWARD_SPEED, listen_FastForwardSpeed);
            _configMod.Subscribe<float>(DIALOG_PLAY_SPEED, listen_PlaySpeed);
        }

        private void listen_PlaySpeed(float speed)
        {
            PlaySpeed = speed;
        }

        private void listen_FastForwardSpeed(float speed)
        {
            FastForwardSpeed = speed;
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
            _configMod.Unsubscribe<float>(DIALOG_FASTFORWARD_SPEED, listen_FastForwardSpeed);
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

        void CompleteOrNextState();

        void BeginFastForward();

        void EndFastForward();
    }

    public interface IPlayable
    {
        void Play();

        void Pause();

        void Resume();

        void Stop();
    }


    public interface IDramaState : IPlayable, IDisposable
    {
        bool IsCompleted();

        void Complete();

        void SetTimeScale(float scale);
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
            _observables.WhenAll().OnCompleted(() =>
            {
                _isCompleted = true;
            });
        }

        private float _timeScale;
        public float TimeScale { get => _timeScale; set => _timeScale = value; }

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
            _observables.ForEach((a) =>
            {
                a?.Play();
                a.TimeScale = _timeScale;
            });
        }

        public void Resume()
        {
            _observables.ForEach((a) => a?.Resume());
        }

        public void Stop()
        {
            _observables.ForEach((a) => a?.Stop());
        }

        public void Complete()
        {
            _observables.ForEach((a) => a?.Complete());
        }

        public void Dispose()
        {
            _observables.ForEach((a) => a.Dispose());
        }

        public void SetTimeScale(float scale)
        {
            _timeScale = scale;
            _observables.ForEach((a) => a.TimeScale = _timeScale);
        }
    }
}
