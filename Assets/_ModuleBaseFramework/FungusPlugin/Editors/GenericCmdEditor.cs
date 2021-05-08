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
        /// <summary>
        /// Dictionary of command name and method information
        /// </summary>
        private IDictionary<string, MethodInfo> _methods;

        /// <summary>
        /// Selection of command name
        /// </summary>
        private SerializedProperty _cmdNameProp;

        /// <summary>
        /// List of command parameters
        /// </summary>
        private SerializedProperty _cmdParamProp;

        /// <summary>
        /// List of command name
        /// </summary>
        private string[] _cmdList;

        public override void OnEnable() {
            base.OnEnable();
            _cmdNameProp = serializedObject.FindProperty("CmdName");
            _cmdParamProp = serializedObject.FindProperty("CmdParams");

            _methods = ModuleCmdCache<TMod>.GetMethods();
            _cmdList = _methods.Keys.ToArray();
        }

        #region -- Draw methods --
        public override void DrawCommandGUI() {
            if (_methods.Count > 0) {
                bool diff = DrawCmdPopup(out string cmdName);
                DrawCmdParams(cmdName, diff);
            }
            serializedObject.ApplyModifiedProperties();
        }

        /// <summary>
        /// Draw single line of cmd popup
        /// </summary>
        private bool DrawCmdPopup(out string cmdName) {
            string curName = _cmdNameProp.stringValue;
            int index = Array.IndexOf(_cmdList, curName);
            if (index < 0)
                index = 0;
            int newIndex = EditorGUILayout.Popup(index, _cmdList);
            bool diff = index != newIndex;
            if (diff || string.IsNullOrEmpty(curName)) {
                _cmdNameProp.stringValue = _cmdList[newIndex];
            }
            cmdName = _cmdList[newIndex];
            return diff;
        }

        /// <summary>
        /// Draw multiple line fields of parameters
        /// </summary>
        private void DrawCmdParams(string cmdName, bool reset) {
            ParameterInfo[] paramInfos = _methods[cmdName].GetParameters();

            _cmdParamProp.arraySize = paramInfos.Length;
            for (int i = 0; i < paramInfos.Length; i++) {
                if (paramInfos[i].ParameterType.IsEnum) {
                    DrawEnumParameter(paramInfos[i], _cmdParamProp.GetArrayElementAtIndex(i), reset);
                }
                else {
                    DrawCommonParameter(paramInfos[i], _cmdParamProp.GetArrayElementAtIndex(i), reset);
                }
            }
        }

        /// <summary>
        /// Draw single line popup of enum parameter
        /// </summary>
        private void DrawEnumParameter(ParameterInfo param, SerializedProperty prop, bool reset) {
            if (string.IsNullOrEmpty(prop.stringValue) || reset)
                prop.stringValue = GetDefaultParameter(param.ParameterType);
            string label = char.ToUpper(param.Name[0]) + param.Name.Substring(1);
            string[] enumList = Enum.GetNames(param.ParameterType);
            int index = Array.IndexOf(enumList, prop.stringValue);
            if (index < 0)
                index = 0;
            int newIndex = EditorGUILayout.Popup(label, index, enumList);
            prop.stringValue = enumList[newIndex];
        }

        /// <summary>
        /// Draw single line field of common parameter
        /// </summary>
        private void DrawCommonParameter(ParameterInfo param, SerializedProperty prop, bool reset) {
            if (string.IsNullOrEmpty(prop.stringValue) || reset)
                prop.stringValue = GetDefaultParameter(param.ParameterType);
            string label = char.ToUpper(param.Name[0]) + param.Name.Substring(1);
            prop.stringValue = EditorGUILayout.TextField(label, prop.stringValue);
        }
        #endregion

        private string GetDefaultParameter(Type type) {
            if (type == typeof(string))
                return string.Empty;
            else if (type.IsEnum)
                return Enum.GetNames(type)[0];
            else {
                var inst = Activator.CreateInstance(type);
                return inst.ToString();
            }
        }
    }
}
#endif