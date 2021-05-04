#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Fungus.EditorUtils;
using System.Reflection;
using System.Linq;
using System;

namespace ModuleBased.FungusPlugin.Editor {
    public class GenericCmdEditor<TMod> : CommandEditor where TMod : IGameModule {
        private IDictionary<string, MethodInfo> _methods;
        private SerializedProperty _cmdNameProp;

        public override void OnEnable() {
            base.OnEnable();
            _cmdNameProp = serializedObject.FindProperty("CmdName");
            _methods = ModuleCmdCache<TMod>.GetMethods();
        }

        public override void DrawCommandInspectorGUI() {
            if (_methods.Count > 0) {
                string[] nameList = _methods.Keys.ToArray();
                string curName = _cmdNameProp.stringValue;
                int index = Array.IndexOf(nameList, curName);
                if (index < 0)
                    index = 0;
                int newIndex = EditorGUILayout.Popup(index, nameList);
                bool diff = index != newIndex;
                if (diff || string.IsNullOrEmpty(curName)) {
                    _cmdNameProp.stringValue = nameList[newIndex];
                }
            }
            serializedObject.ApplyModifiedProperties();
        }
    }
}
#endif