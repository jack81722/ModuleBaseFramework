using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ModuleBased.Dialogue {
    public interface IDialogueWindowCollection {
        void AddDialogueWindow<T>(T instance) where T : class;

        void AddDialogueWindow(Type itfType, object instance);

        object GetDialogueWindow(Type itfType);

        T GetDialogueWindow<T>() where T : class;
    }
}