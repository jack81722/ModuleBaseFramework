#if UNITY_EDITOR
using Fungus.EditorUtils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace ModuleBased.FungusPlugin.Editor {
    public abstract class GenericEventEditor<TItf> : EventHandlerEditor where TItf : class {
        private IDictionary<string, EventInfo> _events;
        private SerializedProperty _eventNameProp;

        private void OnEnable() {
            _events = ModuleEventCache<TItf>.GetEvents();
        }

        protected override void DrawProperties() {
            if (_events.Count <= 0)
                return;
            if(_eventNameProp == null)
                _eventNameProp = serializedObject.FindProperty("EventName");
            string[] nameList = _events.Keys.ToArray();
            string curName = _eventNameProp.stringValue;
            int index = Array.IndexOf(nameList, curName);
            if (index < 0)
                index = 0;
            int newIndex = EditorGUILayout.Popup(index, nameList);
            bool diff = index != newIndex;
            if (diff || string.IsNullOrEmpty(curName)) {
                _eventNameProp.stringValue = nameList[newIndex];
            }
            serializedObject.ApplyModifiedProperties();
        }
    }
}
#endif