using ModuleBased.ForUnity;
using ModuleBased.Injection;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ModuleBased.Example.Dialog
{
    [InjectableFactory(typeof(IDialogModule))]
    public class DialogModule : MonoBehaviour, IDialogModule, IFactory
    {
        public bool AutoPlay = true;
        public float PlaySpeed = 5f;

        private ChatBox _chatBox;
        private int _chatIndex;
        private List<string> _chatBuff = new List<string>
        {
            "xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx",
            "yyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyy",
            "zzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzz",
            "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa"
        };

        public object Create(object args)
        {
            return new DialogModuleProxy(this);
        }

        public void Pause()
        {
            _chatBox.PauseChat();
        }

        public void Play(ChatBox box = null)
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
            _chatBox.PlayChat(_chatBuff[_chatIndex++], PlaySpeed);
        }

        public void Resume()
        {
            if (_chatBox != null && _chatBox.IsPause())
            {
                _chatBox.ContinueChat();
                return;
            }
        }

        public void QuickSkip()
        {

        }

        public void Skip()
        {

        }

        public void Stop()
        {
            if (_chatBox == null)
                return;
            _chatBox.Dispose();
            _chatBox.Hide();
            _chatBox = null;
            _chatIndex = 0;
        }

        private void Update()
        {
            if (_chatBox != null && _chatBox.IsFinished())
            {
                if (AutoPlay)
                {
                    PlayNext();
                }
            }
        }
    }

    public class DialogModuleProxy : IDialogModule
    {
        [Inject]
        private IGameCore _core;
        [Inject]
        private IConfigModule _configMod;

        private IDialogModule _inner;

        public DialogModuleProxy(IDialogModule inner)
        {
            _inner = inner;
        }

        public void Pause()
        {
            _inner.Pause();
        }

        public void Play(ChatBox box = null)
        {
            var chapterName = _configMod.LoadOrDefault("chapter");
            Debug.Log(chapterName);
            var chatBox = _core.Get<ChatBox>();
            _inner.Play(chatBox);
        }

        public void Stop()
        {
            _inner.Stop();
        }
    }

    public interface IDialogModule
    {
        void Play(ChatBox box = null);

        void Pause();

        void Stop();
    }
}