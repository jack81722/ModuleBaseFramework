using ModuleBased.Proxy;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ModuleBased.Example.Drama.Dialog
{
    public class DialogModuleProxy : AOPProxyBase<IDialogModule>, IDialogModule
    {
        public DialogModuleProxy(object real) : base(real)
        {
        }

        public void Load(string chapterName)
        {
            InvokeProxyMethod(chapterName);
        }

        public void Pause()
        {
            InvokeProxyMethod();
        }

        public void Play()
        {
            InvokeProxyMethod();
        }

        public void RegisterAction(string name, Func<string[], IDramaAction> action)
        {
            InvokeProxyMethod(name, action);
        }

        public void Resume()
        {
            InvokeProxyMethod();
        }

        public IDramaAction Say(string name, string content)
        {
            return (IDramaAction)InvokeProxyMethod(name, content);
        }

        public void Stop()
        {
            InvokeProxyMethod();
        }
    }
}
