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
    public class DialogModule : MonoBehaviour, IDialogModule
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
        private IPool<ChatBox> _chatBoxPool;
        #endregion

        public IDramaAction Say(string name, string content)
        {
            // default case
            var chatBox = _chatBoxPool.Pop();
            chatBox.Set(name, content);
            return chatBox;
        }
    }

    public class DialogInfo
    {
        public string Name;
        public string Action;
        public string Content;

        public override string ToString()
        {
            return $"(Name:{Name}, Action:{Action}, Content:{Content})";
        }
    }
}