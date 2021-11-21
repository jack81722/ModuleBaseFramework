using ModuleBased.ForUnity;
using ModuleBased.Injection;
using ModuleBased.Proxy;
using ModuleBased.Proxy.AOP;
using ModuleBased.Proxy.AOP.Handlers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ModuleBased.Example.Dialog
{   
    [CustomProxy(typeof(DialogModuleProxy))]
    [Injectable(typeof(IDialogModule))]
    public class DialogModule : MonoBehaviour, IDialogModule
    {
        [Inject]
        private IConfigModule _configMod;

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


        public void Pause()
        {
            _chatBox.PauseChat();
        }

        [UniLog(EAOPStatus.Before | EAOPStatus.After | EAOPStatus.Error)]
        [Inject]
        public void Play([Inject]ChatBox box = null)
        {
            Debug.Log("Play");
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

        public void Stop()
        {
            InvokeProxyMethod();
        }
    }

    public interface IDialogModule
    {
        void Play(ChatBox box = null);

        void Pause();

        void Stop();
    }
}